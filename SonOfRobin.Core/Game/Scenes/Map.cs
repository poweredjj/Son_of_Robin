using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Map : Scene
    {
        private readonly World world;
        public readonly bool fullScreen;
        private float multiplier;
        private bool dirtyBackground;
        private RenderTarget2D terrainGfx;
        public bool dirtyFog;
        private RenderTarget2D combinedGfx;
        private ConcurrentBag<Sprite> spritesBag;

        // max screen percentage, that minimap may occupy
        private static readonly float minimapMaxPercentWidth = 0.35f;
        private static readonly float minimapMaxPercentHeight = 0.5f;

        private static readonly List<PieceTemplate.Name> depositList = new List<PieceTemplate.Name> {
            PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.GlassDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig };

        private void UpdateViewPos()
        {
            if (this.fullScreen)
            { this.viewParams.CenterView(); }
            else
            {
                var margin = (int)Math.Ceiling(Math.Min(SonOfRobinGame.VirtualWidth / 30f, SonOfRobinGame.VirtualHeight / 30f));

                this.viewParams.PosX = SonOfRobinGame.VirtualWidth - this.viewParams.Width - margin;
                this.viewParams.PosY = SonOfRobinGame.VirtualHeight - this.viewParams.Height - margin;
            }
        }

        public Map(World world, bool fullScreen, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.drawActive = false;
            this.updateActive = false;
            this.world = world;
            this.fullScreen = fullScreen;
            this.dirtyFog = true;
            this.spritesBag = new ConcurrentBag<Sprite> { };
        }

        protected override void AdaptToNewSize()
        {
            this.UpdateResolution();
        }

        public void TurnOn(bool addTransition = true)
        {
            if (this.fullScreen) this.InputType = InputTypes.Normal;

            this.updateActive = true;
            this.drawActive = true;
            this.blocksDrawsBelow = this.fullScreen;

            this.UpdateResolution();

            if (addTransition) this.AddTransition(inTrans: true);

            this.blocksUpdatesBelow = this.fullScreen && !Preferences.DebugMode; // fullscreen map should only be "live animated" in debug mode
        }

        public void ForceRender()
        {
            this.dirtyBackground = true;
            this.dirtyFog = true;

            this.UpdateResolution();
            this.UpdateBackground();
        }

        public void UpdateResolution() // main rendering code
        {
            float maxPercentWidth = this.fullScreen ? 1 : minimapMaxPercentWidth;
            float maxPercentHeight = this.fullScreen ? 1 : minimapMaxPercentHeight;

            float multiplierX = (float)SonOfRobinGame.VirtualWidth / (float)world.width * maxPercentWidth;
            float multiplierY = (float)SonOfRobinGame.VirtualHeight / (float)world.height * maxPercentHeight;
            this.multiplier = Math.Min(multiplierX, multiplierY);

            this.viewParams.Width = (int)(world.width * this.multiplier);
            this.viewParams.Height = (int)(world.height * this.multiplier);

            if (!this.fullScreen) this.viewParams.Opacity = 0.7f;

            this.UpdateViewPos();

            if (this.terrainGfx == null || this.terrainGfx.Width != this.viewParams.Width || this.terrainGfx.Height != this.viewParams.Height)
            {
                if (this.terrainGfx != null) this.terrainGfx.Dispose();
                this.terrainGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
                this.dirtyBackground = true;
                this.dirtyFog = true;
            }
            this.UpdateFogOfWar();
        }

        public void UpdateBackground()
        {
            if (this.dirtyBackground)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map background (fullscreen {this.fullScreen})");

                this.StartRenderingToTarget(this.terrainGfx);
                SonOfRobinGame.graphicsDevice.Clear(Color.Transparent);

                int width = (int)(this.world.width * this.multiplier);
                int height = (int)(this.world.height * this.multiplier);

                Texture2D mapTexture = BoardGraphics.CreateEntireMapTexture(width: width, height: height, grid: this.world.grid, multiplier: this.multiplier);
                Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
                SonOfRobinGame.spriteBatch.Draw(mapTexture, sourceRectangle, sourceRectangle, Color.White);

                this.EndRenderingToTarget();

                this.dirtyBackground = false;
                this.dirtyFog = true;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} map background updated ({this.viewParams.Width}x{this.viewParams.Height}).", color: Color.White);
            }

            this.UpdateFogOfWar();
        }

        private void UpdateFogOfWar()
        {
            if (this.combinedGfx != null && this.dirtyFog == false) return;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map fog (fullscreen {this.fullScreen})");

            if (this.combinedGfx == null || this.combinedGfx.Width != this.viewParams.Width || this.combinedGfx.Height != this.viewParams.Height)
            {
                if (this.combinedGfx != null) this.combinedGfx.Dispose();
                this.combinedGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            this.StartRenderingToTarget(this.combinedGfx);
            SonOfRobinGame.graphicsDevice.Clear(Color.Transparent);
            SonOfRobinGame.spriteBatch.Draw(this.terrainGfx, this.terrainGfx.Bounds, Color.White);
            Rectangle destinationRectangle;

            if (!Preferences.DebugShowWholeMap)
            {
                int cellWidth = this.world.grid.allCells[0].width;
                int cellHeight = this.world.grid.allCells[0].height;
                int destCellWidth = (int)Math.Ceiling(cellWidth * this.multiplier);
                int destCellHeight = (int)Math.Ceiling(cellHeight * this.multiplier);
                Color fogColor = this.fullScreen ? new Color(50, 50, 50) : new Color(80, 80, 80);

                Rectangle sourceRectangle = new Rectangle(0, 0, cellWidth, cellHeight);
                SonOfRobinGame.graphicsDevice.Clear(new Color(0, 0, 0, 0));

                foreach (Cell cell in this.world.grid.CellsNotVisitedByPlayer)
                {
                    destinationRectangle = new Rectangle(
                        (int)Math.Floor(cell.xMin * this.multiplier),
                        (int)Math.Floor(cell.yMin * this.multiplier),
                        destCellWidth,
                        destCellHeight);

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, destinationRectangle, sourceRectangle, fogColor);
                }
            }
            this.EndRenderingToTarget();

            this.dirtyFog = false;
        }

        public void TurnOff(bool addTransition = true)
        {
            this.InputType = InputTypes.None;
            this.blocksDrawsBelow = false;
            if (addTransition) this.AddTransition(inTrans: false);

            this.blocksUpdatesBelow = false;
        }

        public void AddTransition(bool inTrans)
        {
            bool turnOffDraw = !inTrans;
            bool turnOffUpdate = !inTrans;

            this.UpdateViewPos();

            if (this.fullScreen)
            {
                Rectangle viewRect = this.world.camera.viewRect;
                float transScaleX = SonOfRobinGame.VirtualWidth / (float)this.world.width * this.world.viewParams.ScaleX;
                float transScaleY = SonOfRobinGame.VirtualHeight / (float)this.world.height * this.world.viewParams.ScaleY;
                float transScale = Math.Min(transScaleX, transScaleY);

                float transPosX = 1f / (float)this.world.width * SonOfRobinGame.VirtualWidth;
                float transPosY = 1f / (float)this.world.height * SonOfRobinGame.VirtualHeight;
                float transPos = Math.Min(transPosX, transPosY);

                this.transManager.AddMultipleTransitions(outTrans: !inTrans, duration: 15, endTurnOffDraw: turnOffDraw, endTurnOffUpdate: turnOffUpdate,
                    paramsToChange: new Dictionary<string, float> {
                        { "PosX", -viewRect.Left * transPos },
                        { "PosY", -viewRect.Top * transPos },
                        { "ScaleX", transScale },
                        { "ScaleY", transScale } });
            }
            else
            {
                this.transManager.AddMultipleTransitions(outTrans: !inTrans, duration: 8, endTurnOffDraw: turnOffDraw, endTurnOffUpdate: turnOffUpdate,
                    paramsToChange: new Dictionary<string, float> { { "PosY", this.viewParams.PosY + this.viewParams.Height } });
            }
        }

        public override void Update(GameTime gameTime)
        {
            this.UpdateBackground(); // it's best to update background graphics in Update() (SetRenderTarget in Draw() must go first)
            this.ProcessInput();
        }

        private void ProcessInput()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.MapSwitch)) this.world.ToggleMapMode();
        }

        public override void Draw()
        {
            // filling screen with water color
            if (this.fullScreen) SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            // drawing terrain and fog of war
            SonOfRobinGame.spriteBatch.Draw(this.combinedGfx, new Rectangle(0, 0, this.viewParams.Width, this.viewParams.Height), Color.White * this.viewParams.drawOpacity);

            // drawing pieces
            var groupName = this.fullScreen ? Cell.Group.Visible : Cell.Group.MiniMap;

            byte fillSize;
            byte outlineSize = 1;
            Color fillColor;
            Color outlineColor = new Color(0, 0, 0);
            bool drawOutline;
            bool showOutsideCamera;
            Rectangle cameraRect = this.world.camera.viewRect;
            BoardPiece piece;

            // sprites bag should be only updated once in a while
            if ((this.world.currentUpdate % 4 == 0 && SonOfRobinGame.lastDrawDelay < 20) || this.world.currentUpdate % 60 != 0) this.spritesBag = world.grid.GetAllSprites(groupName: groupName, visitedByPlayerOnly: !Preferences.DebugShowWholeMap);

            // regular "foreach", because spriteBatch is not thread-safe
            foreach (Sprite sprite in this.spritesBag)
            {
                drawOutline = false;
                showOutsideCamera = false;
                piece = sprite.boardPiece;

                if (piece.GetType() == typeof(Player))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.White;
                    outlineSize = 8;
                    outlineColor = Color.Black;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Workshop))
                {
                    showOutsideCamera = sprite.hasBeenDiscovered;
                    fillSize = 4;
                    fillColor = Color.Aqua;
                    outlineSize = 8;
                    outlineColor = Color.Blue;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Cooker))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = new Color(255, 0, 0);
                    outlineSize = 8;
                    outlineColor = new Color(128, 0, 0);
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Shelter))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.Chartreuse;
                    outlineSize = 8;
                    outlineColor = Color.SeaGreen;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Container))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.Fuchsia;
                    outlineSize = 8;
                    outlineColor = Color.DarkOrchid;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Tool))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.LightCyan;
                    outlineSize = 8;
                    outlineColor = Color.Teal;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Equipment))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.Gold;
                    outlineSize = 8;
                    outlineColor = Color.Chocolate;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Fruit))
                {
                    showOutsideCamera = sprite.hasBeenDiscovered;
                    fillSize = 2;
                    fillColor = Color.Lime;
                    outlineSize = 4;
                    outlineColor = Color.DarkGreen;
                    drawOutline = true;
                }

                else if (piece.GetType() == typeof(Animal))
                {
                    fillColor = PieceInfo.GetInfo(piece.name).isCarnivorous ? Color.Red : Color.Yellow;
                    fillSize = 3;
                }

                else if (piece.GetType() == typeof(Plant))
                {
                    fillSize = 2;
                    fillColor = piece.alive ? Color.Green : Color.DarkGreen;
                }

                else if (piece.GetType() == typeof(Decoration))
                {
                    if (piece.name == PieceTemplate.Name.CrateStarting || piece.name == PieceTemplate.Name.CrateRegular)
                    {
                        showOutsideCamera = sprite.hasBeenDiscovered;
                        fillSize = 4;
                        fillColor = Color.BurlyWood;
                        outlineSize = 8;
                        outlineColor = Color.Maroon;
                        drawOutline = true;
                    }
                    else if (depositList.Contains(piece.name))
                    {
                        showOutsideCamera = sprite.hasBeenDiscovered;
                        fillSize = 4;
                        fillColor = Color.Yellow;
                        outlineSize = 8;
                        outlineColor = Color.Red;
                        drawOutline = true;
                    }

                    else
                    {
                        fillSize = 3;
                        fillColor = Color.Black;
                    }
                }

                else if (piece.GetType() == typeof(Collectible))
                {
                    fillSize = 2;
                    fillColor = Color.Gray;
                }

                else
                { continue; }

                if (!showOutsideCamera && !cameraRect.Contains(sprite.gfxRect) && !Preferences.debugShowAllMapPieces) continue;

                if (drawOutline) this.DrawSpriteSquare(sprite: sprite, size: outlineSize, color: outlineColor);
                this.DrawSpriteSquare(sprite: sprite, size: fillSize, color: fillColor);
            }

            // drawing camera FOV
            var viewRect = this.world.camera.viewRect;

            int fovX = (int)(viewRect.Left * this.multiplier);
            int fovY = (int)(viewRect.Top * this.multiplier);
            int fovWidth = (int)(viewRect.Width * this.multiplier);
            int fovHeight = (int)(viewRect.Height * this.multiplier);
            int borderWidth = this.fullScreen ? 2 : 1;

            Rectangle fovRect = new Rectangle(fovX, fovY, fovWidth, fovHeight);
            Helpers.DrawRectangleOutline(rect: fovRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: borderWidth);
        }

        private void DrawSpriteSquare(Sprite sprite, byte size, Color color)
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(
                   (int)((sprite.position.X * this.multiplier) - size / 2),
                   (int)((sprite.position.Y * this.multiplier) - size / 2),
                   size, size), color * this.viewParams.drawOpacity);
        }

        public void StartRenderingToTarget(RenderTarget2D newRenderTarget)
        {
            SonOfRobinGame.spriteBatch.Begin();
            SonOfRobinGame.graphicsDevice.SetRenderTarget(newRenderTarget); // SetRenderTarget() wipes the target black!
        }

        public void StartRenderingToTarget(RenderTarget2D newRenderTarget, BlendState blendState)
        {
            SonOfRobinGame.spriteBatch.Begin(blendState: blendState);
            SonOfRobinGame.graphicsDevice.SetRenderTarget(newRenderTarget); // SetRenderTarget() wipes the target black!
        }

        public void EndRenderingToTarget()
        { SonOfRobinGame.spriteBatch.End(); }

    }
}
