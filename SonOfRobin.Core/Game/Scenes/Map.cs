using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Map : Scene
    {
        public enum MapMode
        {
            Mini,
            Full,
            Off
        }

        private readonly World world;
        private readonly Camera camera;
        private readonly Rectangle worldRect;
        private readonly MapOverlay mapOverlay;
        public bool FullScreen { get { return this.Mode == MapMode.Full; } }

        private MapMode mode;
        private float scaleMultiplier;
        private bool dirtyBackground;
        public bool dirtyFog;
        private RenderTarget2D miniatureTerrainGfx;
        private RenderTarget2D miniatureCombinedGfx;
        public RenderTarget2D FinalMapToDisplay { get; private set; }

        private Vector2 lastTouchPos;
        private BoardPiece mapMarker;
        private float InitialZoom { get { return Preferences.WorldScale / 2; } }

        private static readonly Color fogColor = new Color(50, 50, 50);

        public Map(World world, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.mapOverlay = new MapOverlay(this);
            this.AddLinkedScene(this.mapOverlay);
            this.world = world;
            this.worldRect = new Rectangle(x: 0, y: 0, width: world.width, height: world.height);
            this.camera = new Camera(world: this.world, displayScene: this, useFluidMotion: false, keepInWorldBounds: false);
            this.mode = MapMode.Off;
            this.dirtyFog = true;
            this.lastTouchPos = Vector2.Zero;
        }

        public override void Remove()
        {
            base.Remove();

            if (this.miniatureTerrainGfx != null) this.miniatureTerrainGfx.Dispose();
            if (this.miniatureCombinedGfx != null) this.miniatureCombinedGfx.Dispose();
            if (this.FinalMapToDisplay != null) this.FinalMapToDisplay.Dispose();
        }

        protected override void AdaptToNewSize()
        {
            this.UpdateResolution();
            this.mapOverlay.AddTransition();
        }

        public void TurnOff()
        {
            this.Mode = MapMode.Off;
        }

        public void ForceRender()
        {
            this.dirtyBackground = true;
            this.dirtyFog = true;

            this.UpdateResolution();
            this.UpdateBackground();
        }

        public void UpdateResolution()
        {
            float multiplierX = (float)SonOfRobinGame.VirtualWidth / (float)world.width;
            float multiplierY = (float)SonOfRobinGame.VirtualHeight / (float)world.height;
            this.scaleMultiplier = Math.Min(multiplierX, multiplierY);

            this.SetViewParamsForMiniature();

            if (this.miniatureTerrainGfx == null || this.miniatureTerrainGfx.Width != this.viewParams.Width || this.miniatureTerrainGfx.Height != this.viewParams.Height)
            {
                if (this.miniatureTerrainGfx != null) this.miniatureTerrainGfx.Dispose();
                this.miniatureTerrainGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
                if (this.FinalMapToDisplay != null) this.FinalMapToDisplay.Dispose();

                this.FinalMapToDisplay = new RenderTarget2D(SonOfRobinGame.graphicsDevice, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None);

                this.dirtyBackground = true;
                this.dirtyFog = true;
            }

            this.UpdateFogOfWar();
        }

        public void UpdateBackground()
        {
            if (this.dirtyBackground)
            {
                this.SetViewParamsForMiniature();

                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map background (fullscreen {this.FullScreen})");

                SetRenderTarget(this.miniatureTerrainGfx);
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
                SonOfRobinGame.graphicsDevice.Clear(Color.Transparent);

                int width = (int)(this.world.width * this.scaleMultiplier);
                int height = (int)(this.world.height * this.scaleMultiplier);

                Texture2D mapTexture = BoardGraphics.CreateEntireMapTexture(width: width, height: height, grid: this.world.grid, multiplier: this.scaleMultiplier);
                Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
                SonOfRobinGame.spriteBatch.Draw(mapTexture, sourceRectangle, sourceRectangle, Color.White);

                SonOfRobinGame.spriteBatch.End();

                this.dirtyBackground = false;
                this.dirtyFog = true;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} map background updated ({this.viewParams.Width}x{this.viewParams.Height}).", color: Color.White);
            }

            this.UpdateFogOfWar();
        }

        private void UpdateFogOfWar()
        {
            if (this.miniatureCombinedGfx != null && this.dirtyFog == false) return;

            this.SetViewParamsForMiniature();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map fog (fullscreen {this.FullScreen})");

            if (this.miniatureCombinedGfx == null || this.miniatureCombinedGfx.Width != this.viewParams.Width || this.miniatureCombinedGfx.Height != this.viewParams.Height)
            {
                if (this.miniatureCombinedGfx != null) this.miniatureCombinedGfx.Dispose();
                this.miniatureCombinedGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            SetRenderTarget(this.miniatureCombinedGfx);
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.graphicsDevice.Clear(Color.Transparent);
            SonOfRobinGame.spriteBatch.Draw(this.miniatureTerrainGfx, this.miniatureTerrainGfx.Bounds, Color.White);

            if (!Preferences.DebugShowWholeMap)
            {
                int cellWidth = this.world.grid.allCells[0].width;
                int cellHeight = this.world.grid.allCells[0].height;
                int destCellWidth = (int)Math.Ceiling(cellWidth * this.scaleMultiplier);
                int destCellHeight = (int)Math.Ceiling(cellHeight * this.scaleMultiplier);

                Rectangle sourceRectangle = new Rectangle(0, 0, cellWidth, cellHeight);
                SonOfRobinGame.graphicsDevice.Clear(new Color(0, 0, 0, 0));

                foreach (Cell cell in this.world.grid.CellsNotVisitedByPlayer)
                {
                    Rectangle destinationRectangle = new Rectangle(
                        (int)Math.Floor(cell.xMin * this.scaleMultiplier),
                        (int)Math.Floor(cell.yMin * this.scaleMultiplier),
                        destCellWidth,
                        destCellHeight);

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, destinationRectangle, sourceRectangle, fogColor);
                }
            }
            SonOfRobinGame.spriteBatch.End();

            this.dirtyFog = false;
        }

        public void SwitchToNextMode()
        {
            switch (this.Mode)
            {
                case MapMode.Mini:
                    this.Mode = MapMode.Full;
                    break;

                case MapMode.Full:
                    this.Mode = MapMode.Off;
                    break;

                case MapMode.Off:
                    this.Mode = MapMode.Mini;
                    break;

                default:
                    throw new ArgumentException($"Unsupported mode - {this.Mode}.");
            }
        }

        public MapMode Mode
        {
            get { return this.mode; }
            private set
            {
                if (this.mode == value) return;
                this.mode = value;

                switch (this.mode)
                {
                    case MapMode.Mini:
                        Sound.QuickPlay(SoundData.Name.TurnPage);
                        this.camera.TrackCoords(position: this.world.player.sprite.position, moveInstantly: true);
                        this.camera.SetZoom(zoom: this.InitialZoom, setInstantly: true);
                        this.UpdateResolution();
                        this.blocksUpdatesBelow = false;
                        this.InputType = InputTypes.None;
                        this.drawActive = true;
                        break;

                    case MapMode.Full:
                        Sound.QuickPlay(SoundData.Name.PaperMove1);
                        this.blocksUpdatesBelow = this.FullScreen && !Preferences.DebugMode; // fullscreen map should only be "live animated" in debug mode
                        this.InputType = InputTypes.Normal;
                        this.drawActive = true;
                        break;

                    case MapMode.Off:
                        Sound.QuickPlay(SoundData.Name.PaperMove2);
                        this.blocksUpdatesBelow = false;
                        this.InputType = InputTypes.None;
                        this.drawActive = false;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported mode - {this.Mode}.");
                }

                this.camera.Update();
                this.mapOverlay.AddTransition();
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
            if (this.mapMarker != null) FieldTip.AddUpdateTip(world: this.world, texture: this.mapMarker.sprite.frame.texture, targetSprite: this.mapMarker.sprite, alignment: FieldTip.Alignment.Center); // needed to update tips, when map is not active
            // TODO add field tip option, for keeping tip inside camera rect

            if (this.Mode == MapMode.Off) return;

            if (!this.CheckIfCanBeTurnedOn(showMessage: true))
            {
                this.TurnOff();
                return;
            }

            if (this.Mode == MapMode.Full) this.ProcessInput();
            else this.camera.TrackCoords(this.world.player.sprite.position);

            this.camera.Update();
            this.world.grid.UnloadTexturesIfMemoryLow(this.camera);
            this.world.grid.LoadClosestTextureInCameraView(this.camera);
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

        private void SetViewParamsForTargetRender()
        {
            this.viewParams.Width = this.world.width;
            this.viewParams.Height = this.world.height;
            this.viewParams.ScaleX = 1 / this.camera.currentZoom;
            this.viewParams.ScaleY = 1 / this.camera.currentZoom;
            this.camera.Update();
            this.viewParams.PosX = this.camera.viewPos.X;
            this.viewParams.PosY = this.camera.viewPos.Y;
        }

        private void ProcessInput()
        {
            var touches = TouchInput.TouchPanelState;
            if (!touches.Any()) this.lastTouchPos = Vector2.Zero;

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapSwitch))
            {
                this.SwitchToNextMode();
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm))
            {
                if (this.mapMarker == null) this.mapMarker = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.camera.CurrentPos, templateName: PieceTemplate.Name.MapMarker);
                else this.mapMarker.sprite.SetNewPosition(this.camera.CurrentPos);

                return;
            }

            float zoomMultiplier = Mouse.ScrollWheelRolledUp || Mouse.ScrollWheelRolledDown ? 0.15f : 0.035f;
            float zoomChangeVal = this.camera.currentZoom * zoomMultiplier;

            if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn))
            {
                float currentZoom = this.camera.currentZoom + zoomChangeVal;
                currentZoom = Math.Min(currentZoom, this.InitialZoom);
                this.camera.SetZoom(currentZoom);
            }

            if (InputMapper.IsPressed(InputMapper.Action.MapZoomOut))
            {
                float currentZoom = this.camera.currentZoom - zoomChangeVal;
                currentZoom = Math.Max(currentZoom, this.scaleMultiplier * 0.7f);
                this.camera.SetZoom(currentZoom);
            }

            Vector2 movement = InputMapper.Analog(InputMapper.Action.MapMove) * 10 / this.camera.currentZoom;

            if (movement == Vector2.Zero)
            {
                foreach (TouchLocation touch in TouchInput.TouchPanelState)
                {
                    if (touch.State == TouchLocationState.Released)
                    {
                        this.lastTouchPos = Vector2.Zero;
                        break;
                    }

                    if (!TouchInput.IsPointActivatingAnyTouchInterface(point: touch.Position, checkLeftStick: false, checkRightStick: false, checkVirtButtons: true, checkInventory: false, checkPlayerPanel: false))
                    {
                        if (touch.State == TouchLocationState.Pressed)
                        {
                            this.lastTouchPos = touch.Position;
                            break;
                        }
                        else if (touch.State == TouchLocationState.Moved)
                        {
                            movement = this.lastTouchPos - touch.Position;
                            this.lastTouchPos = touch.Position;
                        }

                        movement /= this.camera.currentZoom;
                    }
                }
            }

            if (movement != Vector2.Zero)
            {
                Vector2 newPos = this.camera.TrackedPos + movement;

                newPos.X = Math.Max(newPos.X, 0);
                newPos.X = Math.Min(newPos.X, this.world.width);
                newPos.Y = Math.Max(newPos.Y, 0);
                newPos.Y = Math.Min(newPos.Y, this.world.height);

                this.camera.TrackCoords(newPos);
            }

            this.camera.Update();
            this.SetViewParamsForTargetRender();

        }

        public override void RenderToTarget()
        {
            if (this.Mode == MapMode.Off || (this.Mode == MapMode.Mini && SonOfRobinGame.currentUpdate % 2 != 0)) return;

            this.UpdateBackground();

            this.SetViewParamsForTargetRender();
            SetRenderTarget(this.FinalMapToDisplay);
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);

            // filling with water color

            SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.Beach1]);

            // calculating miniature opacity

            float showMiniatureAtZoom = (float)SonOfRobinGame.VirtualWidth / (float)this.world.width * 1.4f;
            float showFullScaleAtZoom = this.InitialZoom;

            float miniatureOpacity = (float)Helpers.ConvertRange(oldMin: showFullScaleAtZoom, oldMax: showMiniatureAtZoom, newMin: 0f, newMax: 1f, oldVal: this.camera.currentZoom, clampToEdges: true);

            // MessageLog.AddMessage(msgType: MsgType.User, message: $"Zoom {this.camera.currentZoom} miniatureOpacity {miniatureOpacity} showMiniatureAtZoom {showMiniatureAtZoom}");

            // drawing detailed background

            var visibleCells = this.world.grid.GetCellsInsideRect(this.camera.viewRect);

            int drawnCellCount = miniatureOpacity < 1 ? visibleCells.Count : 0;

            // MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.currentUpdate} - drawn map cells count: {drawnCellCount}");

            if (miniatureOpacity < 1)
            {
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, this.worldRect, fogColor);

                foreach (Cell cell in visibleCells)
                {
                    if (cell.VisitedByPlayer || Preferences.DebugShowWholeMap) cell.DrawBackground(opacity: 1f);
                }
            }

            // drawing miniature background

            if (miniatureOpacity > 0) SonOfRobinGame.spriteBatch.Draw(this.miniatureCombinedGfx, this.worldRect, Color.White * miniatureOpacity);

            // drawing pieces

            Rectangle worldCameraRect = this.world.camera.viewRect;

            var spritesBag = world.grid.GetSprites(groupName: Cell.Group.Visible, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, camera: this.camera);

            var typesShownAlways = new List<Type> { typeof(Player), typeof(Workshop), typeof(Cooker), typeof(Shelter) };
            var namesShownAlways = new List<PieceTemplate.Name> { PieceTemplate.Name.MapMarker };
            var typesShownIfDiscovered = new List<Type> { typeof(Container) };
            var namesShownIfDiscovered = new List<PieceTemplate.Name> { PieceTemplate.Name.CrateStarting, PieceTemplate.Name.CrateRegular, PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig };

            float spriteSize = 1f / this.camera.currentZoom * 0.25f;

            // drawing marker

            if (this.mapMarker != null) spritesBag.Add(this.mapMarker.sprite);

            // regular "foreach", because spriteBatch is not thread-safe
            foreach (Sprite sprite in spritesBag.OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom))
            {
                BoardPiece piece = sprite.boardPiece;
                PieceTemplate.Name name = piece.name;
                Type pieceType = piece.GetType();

                bool showSprite = false;

                if (typesShownAlways.Contains(pieceType) || namesShownAlways.Contains(name)) showSprite = true;

                if (!showSprite && sprite.hasBeenDiscovered &&
                    (namesShownIfDiscovered.Contains(name) ||
                    typesShownIfDiscovered.Contains(pieceType))) showSprite = true;

                if (showSprite)
                {
                    Rectangle destRect = sprite.gfxRect;

                    destRect.Inflate(destRect.Width * spriteSize, destRect.Height * spriteSize);

                    if (this.Mode == MapMode.Mini && name == PieceTemplate.Name.MapMarker && !this.camera.viewRect.Contains(sprite.position))
                    {
                        destRect.X = Math.Max(this.camera.viewRect.Left, destRect.X);
                        destRect.X = Math.Min(this.camera.viewRect.Right - destRect.Width, destRect.X);
                        destRect.Y = Math.Max(this.camera.viewRect.Top, destRect.Y);
                        destRect.Y = Math.Min(this.camera.viewRect.Bottom - destRect.Height, destRect.Y);
                    }

                    sprite.frame.Draw(destRect: destRect, color: Color.White, opacity: 1f);
                }
            }

            // drawing crosshair

            if (this.Mode == MapMode.Full)
            {
                int crossHairSize = (int)(this.camera.viewRect.Width * 0.02f);
                int crosshairHalfSize = crossHairSize / 2;

                Rectangle crosshairRect = new Rectangle(x: (int)this.camera.CurrentPos.X - crosshairHalfSize, y: (int)this.camera.CurrentPos.Y - crosshairHalfSize, width: crossHairSize, height: crossHairSize);
                AnimFrame crosshairFrame = PieceInfo.GetInfo(PieceTemplate.Name.Crosshair).frame;
                crosshairFrame.DrawAndKeepInRectBounds(destBoundsRect: crosshairRect, color: Color.White);
            }

            SonOfRobinGame.spriteBatch.End();
        }

        public override void Draw()
        {
            // is being drawn in MapOverlay scene
        }

    }
}
