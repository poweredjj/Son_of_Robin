using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Map : Scene
    {
        public enum MapMode
        {
            Off,
            Mini,
            Full
        }

        private readonly World world;
        private readonly Camera camera;
        private readonly Rectangle worldRect;
        public bool FullScreen { get { return this.Mode == MapMode.Full; } }
        public MapMode Mode { get; private set; }

        private float scaleMultiplier;
        private bool dirtyBackground;
        public bool dirtyFog;
        private RenderTarget2D terrainGfx;
        private RenderTarget2D combinedGfx;
        private ConcurrentBag<Sprite> spritesBag;

        private static readonly List<PieceTemplate.Name> depositNameList = new List<PieceTemplate.Name> {
            PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig };

        public Map(World world, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.drawActive = false;
            this.updateActive = false;
            this.world = world;
            this.worldRect = new Rectangle(x: 0, y: 0, width: world.width, height: world.height);
            this.camera = new Camera(world: this.world, displayScene: this, useFluidMotion: false);
            this.Mode = MapMode.Off;
            this.dirtyFog = true;
            this.spritesBag = new ConcurrentBag<Sprite> { };
        }

        protected override void AdaptToNewSize()
        {
            this.UpdateResolution();
        }

        public void TurnOff()
        {
            this.Mode = MapMode.Off;

            this.InputType = InputTypes.None;
            this.blocksDrawsBelow = false;
            this.blocksUpdatesBelow = false;
        }

        private void TurnOn()
        {
            if (this.FullScreen) this.InputType = InputTypes.Normal;

            this.camera.TrackPiece(trackedPiece: this.world.player, moveInstantly: true);
            this.camera.SetZoom(zoom: 0.5f, setInstantly: true);

            this.updateActive = true;
            this.drawActive = true;
            this.blocksDrawsBelow = this.FullScreen;

            this.UpdateResolution();

            this.blocksUpdatesBelow = this.FullScreen && !Preferences.DebugMode; // fullscreen map should only be "live animated" in debug mode
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
            float multiplierX = (float)SonOfRobinGame.VirtualWidth / (float)world.width;
            float multiplierY = (float)SonOfRobinGame.VirtualHeight / (float)world.height;
            this.scaleMultiplier = Math.Min(multiplierX, multiplierY);

            this.SetViewParamsForMiniature();
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
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map background (fullscreen {this.FullScreen})");

                this.StartRenderingToTarget(this.terrainGfx);
                SonOfRobinGame.graphicsDevice.Clear(Color.Transparent);

                int width = (int)(this.world.width * this.scaleMultiplier);
                int height = (int)(this.world.height * this.scaleMultiplier);

                Texture2D mapTexture = BoardGraphics.CreateEntireMapTexture(width: width, height: height, grid: this.world.grid, multiplier: this.scaleMultiplier);
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

            this.SetViewParamsForMiniature();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map fog (fullscreen {this.FullScreen})");

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
                int destCellWidth = (int)Math.Ceiling(cellWidth * this.scaleMultiplier);
                int destCellHeight = (int)Math.Ceiling(cellHeight * this.scaleMultiplier);
                Color fogColor = this.FullScreen ? new Color(50, 50, 50) : new Color(80, 80, 80);

                Rectangle sourceRectangle = new Rectangle(0, 0, cellWidth, cellHeight);
                SonOfRobinGame.graphicsDevice.Clear(new Color(0, 0, 0, 0));

                foreach (Cell cell in this.world.grid.CellsNotVisitedByPlayer)
                {
                    destinationRectangle = new Rectangle(
                        (int)Math.Floor(cell.xMin * this.scaleMultiplier),
                        (int)Math.Floor(cell.yMin * this.scaleMultiplier),
                        destCellWidth,
                        destCellHeight);

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, destinationRectangle, sourceRectangle, fogColor);
                }
            }
            this.EndRenderingToTarget();

            this.dirtyFog = false;
        }

        public void SwitchToNextMode()
        {
            switch (this.Mode)
            {
                case MapMode.Off:
                    Sound.QuickPlay(SoundData.Name.TurnPage);
                    this.Mode = MapMode.Full;
                    this.TurnOn();
                    break;

                // TODO enable mini mode

                //case MapMode.Mini:
                //    Sound.QuickPlay(SoundData.Name.PaperMove1);
                //    this.Mode = MapMode.Full;
                //    break;

                case MapMode.Full:
                    this.Mode = MapMode.Off;
                    Sound.QuickPlay(SoundData.Name.PaperMove2);
                    this.TurnOff();
                    break;

                default:
                    throw new ArgumentException($"Unsupported mode - {this.Mode}.");
            }

            this.AddTransition();
        }

        public void AddTransition()
        {
            this.UpdateViewPos();

            // this.viewParams.Opacity = this.FullScreen ? 1f : 0.7f;

            switch (this.Mode)
            {
                case MapMode.Off:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 15, endTurnOffDraw: true, endTurnOffUpdate: true,
                        paramsToChange: new Dictionary<string, float> { { "Opacity", 0f } });

                    break;

                case MapMode.Mini:

                    // TODO add transition code

                    break;

                case MapMode.Full:

                    this.viewParams.Opacity = 1f;

                    this.transManager.AddMultipleTransitions(outTrans: false, duration: 15, endTurnOffDraw: false, endTurnOffUpdate: false,
                        paramsToChange: new Dictionary<string, float> {
                        { "PosX", this.world.viewParams.drawPosX},
                        { "PosY", this.world.viewParams.drawPosY},
                        { "ScaleX", this.world.viewParams.drawScaleX },
                        { "ScaleY", this.world.viewParams.drawScaleY },
                        });

                    break;

                default:
                    throw new ArgumentException($"Unsupported mode - {this.Mode}.");
            }

        }

        public bool CheckIfCanBeTurnedOn(bool showMessage = true)
        {
            bool canBeTurnedOn = this.world.player.CanSeeAnything;

            if (!canBeTurnedOn && showMessage && GetTopSceneOfType(typeof(TextWindow)) == null)
            {
                if (this.world.hintEngine.shownTutorials.Contains(Tutorials.Type.TooDarkToReadMap)) new TextWindow(text: "It is too dark to read the map.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.world.DialogueSound);
                else Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.TooDarkToReadMap, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return canBeTurnedOn;
        }

        public override void Update(GameTime gameTime)
        {
            if (!this.CheckIfCanBeTurnedOn(showMessage: true))
            {
                this.TurnOff();
                return;
            }

            this.UpdateBackground(); // it's best to update background graphics in Update() (SetRenderTarget in Draw() must go first)
            this.ProcessInput();
            this.SetViewParamsForRender();
        }

        private void SetViewParamsForMiniature()
        {
            this.viewParams.Width = (int)(this.world.width * this.scaleMultiplier);
            this.viewParams.Height = (int)(this.world.height * this.scaleMultiplier);
            this.viewParams.ScaleX = 1f;
            this.viewParams.ScaleY = 1f;
            this.viewParams.PosX = 0;
            this.viewParams.PosY = 0;
        }

        private void SetViewParamsForRender()
        {
            this.viewParams.Width = this.world.width;
            this.viewParams.Height = this.world.height;
            this.viewParams.ScaleX = 1 / this.camera.currentZoom;
            this.viewParams.ScaleY = 1 / this.camera.currentZoom;
            this.camera.Update();
            this.viewParams.PosX = this.camera.viewPos.X;
            this.viewParams.PosY = this.camera.viewPos.Y;
        }

        private void UpdateViewPos()
        {
            if (this.FullScreen) this.viewParams.CenterView();
            else
            {
                var margin = (int)Math.Ceiling(Math.Min(SonOfRobinGame.VirtualWidth / 30f, SonOfRobinGame.VirtualHeight / 30f));

                this.viewParams.PosX = SonOfRobinGame.VirtualWidth - this.viewParams.Width - margin;
                this.viewParams.PosY = SonOfRobinGame.VirtualHeight - this.viewParams.Height - margin;
            }
        }

        private void ProcessInput()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.MapSwitch)) this.world.ToggleMapMode();
        }

        public override void Draw()
        {
            // filling screen with water color

            if (this.FullScreen) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, this.worldRect, BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep] * this.viewParams.drawOpacity);

            // drawing terrain and fog of war
            SonOfRobinGame.spriteBatch.Draw(this.combinedGfx, this.worldRect, Color.White * this.viewParams.drawOpacity);

            // drawing pieces
            var groupName = this.FullScreen ? Cell.Group.Visible : Cell.Group.MiniMap;

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
                Type pieceType = piece.GetType();

                if (pieceType == typeof(Player))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.White;
                    outlineSize = 8;
                    outlineColor = Color.Black;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Workshop))
                {
                    showOutsideCamera = sprite.hasBeenDiscovered;
                    fillSize = 4;
                    fillColor = Color.Aqua;
                    outlineSize = 8;
                    outlineColor = Color.Blue;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Cooker))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = new Color(255, 0, 0);
                    outlineSize = 8;
                    outlineColor = new Color(128, 0, 0);
                    drawOutline = true;
                }

                else if (pieceType == typeof(Shelter))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.Chartreuse;
                    outlineSize = 8;
                    outlineColor = Color.SeaGreen;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Container))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.Fuchsia;
                    outlineSize = 8;
                    outlineColor = Color.DarkOrchid;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Tool))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.LightCyan;
                    outlineSize = 8;
                    outlineColor = Color.Teal;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Equipment))
                {
                    showOutsideCamera = true;
                    fillSize = 4;
                    fillColor = Color.Gold;
                    outlineSize = 8;
                    outlineColor = Color.Chocolate;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Fruit))
                {
                    showOutsideCamera = sprite.hasBeenDiscovered;
                    fillSize = 2;
                    fillColor = Color.Lime;
                    outlineSize = 4;
                    outlineColor = Color.DarkGreen;
                    drawOutline = true;
                }

                else if (pieceType == typeof(Animal))
                {
                    fillColor = PieceInfo.GetInfo(piece.name).isCarnivorous ? Color.Red : Color.Yellow;
                    fillSize = 3;
                }

                else if (pieceType == typeof(Plant))
                {
                    fillSize = 2;
                    fillColor = piece.alive ? Color.Green : Color.DarkGreen;
                }

                else if (pieceType == typeof(Decoration))
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
                    else if (depositNameList.Contains(piece.name))
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

                else if (pieceType == typeof(Collectible))
                {
                    fillSize = 2;
                    fillColor = Color.Gray;
                }

                else continue;

                if (!showOutsideCamera && !cameraRect.Contains(sprite.gfxRect) && !Preferences.debugShowAllMapPieces) continue;

                if (drawOutline) this.DrawSpriteSquare(sprite: sprite, size: outlineSize, color: outlineColor);
                this.DrawSpriteSquare(sprite: sprite, size: fillSize, color: fillColor);
            }

            // drawing camera FOV
            var viewRect = this.world.camera.viewRect;

            Helpers.DrawRectangleOutline(rect: viewRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: this.FullScreen ? 2 : 1);
        }

        private void DrawSpriteSquare(Sprite sprite, byte size, Color color)
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle,
                new Rectangle(
                    x: (int)(sprite.position.X - size / 2),
                    y: (int)(sprite.position.Y - size / 2),
                    width: size, height: size),
                color * this.viewParams.drawOpacity);
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
