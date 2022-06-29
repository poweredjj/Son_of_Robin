using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
        private RenderTarget2D fogGfx;

        // max screen percentage, that minimap may occupy
        private static readonly float minimapMaxPercentWidth = 0.35f;
        private static readonly float minimapMaxPercentHeight = 0.5f;

        private void UpdateViewPos()
        {
            if (this.fullScreen)
            { this.viewParams.CenterView(); }
            else
            {
                var margin = Convert.ToInt32(Math.Ceiling(Math.Min(SonOfRobinGame.VirtualWidth / 30f, SonOfRobinGame.VirtualHeight / 30f)));

                this.viewParams.posX = SonOfRobinGame.VirtualWidth - this.viewParams.width - margin;
                this.viewParams.posY = SonOfRobinGame.VirtualHeight - this.viewParams.height - margin;
            }
        }

        private Transition InTransition
        { get { return this.GetTransition(inTrans: true); } }

        private Transition OutTransition
        { get { return this.GetTransition(inTrans: false); } }

        public Map(World world, bool fullScreen, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.drawActive = false;
            this.updateActive = false;
            this.world = world;
            this.fullScreen = fullScreen;
            this.dirtyFog = true;
        }

        public void TurnOn(bool addTransition = true)
        {
            if (this.fullScreen) this.InputType = InputTypes.Normal;

            this.updateActive = true;
            this.drawActive = true;
            this.blocksDrawsBelow = this.fullScreen;

            float maxPercentWidth = this.fullScreen ? 1 : minimapMaxPercentWidth;
            float maxPercentHeight = this.fullScreen ? 1 : minimapMaxPercentHeight;

            float multiplierX = ((float)SonOfRobinGame.VirtualWidth / (float)world.width) * maxPercentWidth;
            float multiplierY = ((float)SonOfRobinGame.VirtualHeight / (float)world.height) * maxPercentHeight;
            this.multiplier = Math.Min(multiplierX, multiplierY);

            this.viewParams.width = Convert.ToInt32(world.width * this.multiplier);
            this.viewParams.height = Convert.ToInt32(world.height * this.multiplier);

            this.UpdateViewPos();
            if (addTransition) this.AddTransition(this.InTransition);

            if (this.terrainGfx == null || this.terrainGfx.Width != this.viewParams.width || this.terrainGfx.Height != this.viewParams.height)
            {
                this.terrainGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.width, this.viewParams.height, false, SurfaceFormat.Color, DepthFormat.None);
                this.dirtyBackground = true;
                this.dirtyFog = true;
            }
            this.UpdateFogOfWar();
        }

        private void UpdateFogOfWar()
        {
            if (this.fogGfx != null && this.dirtyFog == false) return;

            this.fogGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.width, this.viewParams.height, false, SurfaceFormat.Color, DepthFormat.None);
            this.StartRenderingToTarget(this.fogGfx);

            int cellWidth = this.world.grid.allCells[0].width;
            int cellHeight = this.world.grid.allCells[0].height;
            int destCellWidth = Convert.ToInt32(Math.Ceiling(cellWidth * this.multiplier));
            int destCellHeight = Convert.ToInt32(Math.Ceiling(cellHeight * this.multiplier));

            Rectangle sourceRectangle = new Rectangle(0, 0, cellWidth, cellHeight);
            SonOfRobinGame.graphicsDevice.Clear(new Color(0, 0, 0, 0));

            foreach (Cell cell in this.world.grid.CellsNotVisitedByPlayer)
            {
                Rectangle destinationRectangle = new Rectangle(
                    Convert.ToInt32(Math.Floor(cell.xMin * this.multiplier)),
                    Convert.ToInt32(Math.Floor(cell.yMin * this.multiplier)),
                    destCellWidth,
                    destCellHeight);

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, destinationRectangle, sourceRectangle, new Color(50, 50, 50));
            }

            this.EndRenderingToTarget();

            this.dirtyFog = false;
        }

        public void TurnOff(bool addTransition = true)
        {
            this.InputType = InputTypes.None;
            this.blocksDrawsBelow = false;
            if (addTransition) this.AddTransition(this.OutTransition);
        }

        public Transition GetTransition(bool inTrans)
        {
            Transition.TransType transType = inTrans ? Transition.TransType.From : Transition.TransType.To;
            bool turnOffDraw = !inTrans;
            bool turnOffUpdate = !inTrans;

            this.UpdateViewPos();

            if (this.fullScreen)
            {
                Rectangle viewRect = this.world.camera.viewRect;
                float transScaleX = SonOfRobinGame.VirtualWidth / (float)this.world.width * this.world.viewParams.scaleX;
                float transScaleY = SonOfRobinGame.VirtualHeight / (float)this.world.height * this.world.viewParams.scaleY;
                float transScale = Math.Min(transScaleX, transScaleY);

                float transPosX = 1f / (float)this.world.width * SonOfRobinGame.VirtualWidth;
                float transPosY = 1f / (float)this.world.height * SonOfRobinGame.VirtualHeight;
                float transPos = Math.Min(transPosX, transPosY);

                return new Transition(type: transType, duration: 15, scene: this, blockInput: false, paramsToChange: new Dictionary<string, float> { { "posX", -viewRect.Left * transPos }, { "posY", -viewRect.Top * transPos }, { "scaleX", transScale }, { "scaleY", transScale } }, turnOffDraw: turnOffDraw, turnOffUpdate: turnOffUpdate);
            }
            else
            {
                return new Transition(type: transType, duration: 8, scene: this, blockInput: false,
                    paramsToChange: new Dictionary<string, float> { { "posY", this.viewParams.posY + this.viewParams.height } }, turnOffDraw: turnOffDraw, turnOffUpdate: turnOffUpdate);
            }
        }

        public void UpdateResolution()
        { if (this.updateActive) this.TurnOn(addTransition: false); }

        public void UpdateBackground()
        {
            this.UpdateFogOfWar();

            if (!this.dirtyBackground) return;

            this.StartRenderingToTarget(this.terrainGfx);

            int width = (int)(this.world.width * this.multiplier);
            int height = (int)(this.world.height * this.multiplier);

            Texture2D mapTexture = BoardGraphics.CreateEntireMapTexture(width: width, height: height, grid: this.world.grid, multiplier: this.multiplier);
            Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
            SonOfRobinGame.spriteBatch.Draw(mapTexture, sourceRectangle, sourceRectangle, Color.White);

            this.EndRenderingToTarget();

            this.dirtyBackground = false;
            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Map background updated ({this.viewParams.width}x{this.viewParams.height}).", color: Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            this.UpdateBackground(); // it's best to update background graphics in Update() (SetRenderTarget in Draw() must go first)
            this.ProcessInput();
        }

        private void ProcessInput()
        {
            if (GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadRight) ||
                Keyboard.HasBeenPressed(Keys.Escape) ||
                Keyboard.HasBeenPressed(Keys.M) ||
                VirtButton.HasButtonBeenPressed(VButName.Return)) this.world.ToggleMapMode();
        }

        public override void Draw()
        {
            // filling screen with water color
            if (this.fullScreen) SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            // drawing background (terrain)
            SonOfRobinGame.spriteBatch.Draw(this.terrainGfx, new Rectangle(0, 0, this.viewParams.width, this.viewParams.height), Color.White * this.viewParams.drawOpacity);

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

            foreach (Sprite sprite in world.grid.GetAllSprites(groupName: groupName, visitedByPlayerOnly: !Preferences.debugShowWholeMap)) // regular "foreach", because spriteBatch is not thread-safe
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
                    fillColor = piece.name == PieceTemplate.Name.Fox ? Color.Red : Color.Yellow;
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
                    else if (piece.name == PieceTemplate.Name.CoalDeposit || piece.name == PieceTemplate.Name.IronDeposit)
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

            // drawing fog of war
            if (!Preferences.debugShowWholeMap) SonOfRobinGame.spriteBatch.Draw(this.fogGfx, this.fogGfx.Bounds, Color.White * this.viewParams.drawOpacity);

            // drawing camera FOV
            var viewRect = this.world.camera.viewRect;

            int fovX = Convert.ToInt32(viewRect.Left * this.multiplier);
            int fovY = Convert.ToInt32(viewRect.Top * this.multiplier);
            int fovWidth = Convert.ToInt32(viewRect.Width * this.multiplier);
            int fovHeight = Convert.ToInt32(viewRect.Height * this.multiplier);
            int borderWidth = this.fullScreen ? 2 : 1;

            Rectangle fovRect = new Rectangle(fovX, fovY, fovWidth, fovHeight);
            Helpers.DrawRectangleOutline(rect: fovRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: borderWidth);
        }

        private void DrawSpriteSquare(Sprite sprite, byte size, Color color)
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(
                   Convert.ToInt32((sprite.position.X * this.multiplier) - size / 2),
                   Convert.ToInt32((sprite.position.Y * this.multiplier) - size / 2),
                   size, size), color * this.viewParams.drawOpacity);
        }

        public void StartRenderingToTarget(RenderTarget2D newRenderTarget)
        {
            SonOfRobinGame.spriteBatch.Begin();
            SonOfRobinGame.graphicsDevice.SetRenderTarget(newRenderTarget);
        }

        public void StartRenderingToTarget(RenderTarget2D newRenderTarget, BlendState blendState)
        {
            SonOfRobinGame.spriteBatch.Begin(blendState: blendState);
            SonOfRobinGame.graphicsDevice.SetRenderTarget(newRenderTarget);
        }

        public void EndRenderingToTarget()
        { SonOfRobinGame.spriteBatch.End(); }

    }
}
