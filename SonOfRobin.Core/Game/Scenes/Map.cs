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
        private readonly EffectCol effectCol;
        public bool FullScreen { get { return this.Mode == MapMode.Full; } }

        private MapMode mode;
        private float scaleMultiplier;
        private bool dirtyBackground;
        public bool dirtyFog;
        private RenderTarget2D miniatureTerrainGfx;
        private RenderTarget2D miniatureCombinedGfx;
        public RenderTarget2D FinalMapToDisplay { get; private set; }
        private Vector2 lastTouchPos; public BoardPiece MapMarker { get; private set; }
        private float InitialZoom { get { return Preferences.WorldScale / 2; } }
        private static readonly Color paperColor = BoardGraphics.colorsByName[BoardGraphics.Colors.Beach1];

        public Map(World world, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.mapOverlay = new MapOverlay(this);
            this.AddLinkedScene(this.mapOverlay);
            this.world = world;
            this.worldRect = new Rectangle(x: 0, y: 0, width: world.width, height: world.height);
            this.camera = new Camera(world: this.world, useWorldScale: false, useFluidMotionForMove: false, useFluidMotionForZoom: true, keepInWorldBounds: false);
            this.mode = MapMode.Off;
            this.dirtyFog = true;
            this.lastTouchPos = Vector2.Zero;
            this.effectCol = new EffectCol(world: world);
            this.effectCol.AddEffect(new SketchInstance(fgColor: new Color(107, 98, 87, 255), bgColor: paperColor, framesLeft: -1));
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
            this.UpdateResolution(force: true);
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

            this.UpdateResolution(force: true);
            this.UpdateBackground();
        }

        public void UpdateResolution(bool force = false)
        {
            float multiplierX = (float)SonOfRobinGame.VirtualWidth / (float)world.width;
            float multiplierY = (float)SonOfRobinGame.VirtualHeight / (float)world.height;
            this.scaleMultiplier = Math.Min(multiplierX, multiplierY);

            this.SetViewParamsForMiniature();

            if (force || this.miniatureTerrainGfx == null || this.miniatureTerrainGfx.Width != this.viewParams.Width || this.miniatureTerrainGfx.Height != this.viewParams.Height)
            {
                if (this.miniatureTerrainGfx != null) this.miniatureTerrainGfx.Dispose();
                this.miniatureTerrainGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
                if (this.FinalMapToDisplay != null) this.FinalMapToDisplay.Dispose();

                this.FinalMapToDisplay = new RenderTarget2D(SonOfRobinGame.graphicsDevice, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None);

                this.dirtyBackground = true;
                this.dirtyFog = true;
            }

            this.UpdateCombinedGfx();
        }

        public void UpdateBackground()
        {
            if (this.dirtyBackground)
            {
                this.SetViewParamsForMiniature();

                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map background (fullscreen {this.FullScreen})");

                SetRenderTarget(this.miniatureTerrainGfx);
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);

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

            this.UpdateCombinedGfx();
        }

        private void UpdateCombinedGfx()
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

            int cellWidth = this.world.grid.allCells[0].width;
            int cellHeight = this.world.grid.allCells[0].height;
            int destCellWidth = (int)Math.Ceiling(cellWidth * this.scaleMultiplier);
            int destCellHeight = (int)Math.Ceiling(cellHeight * this.scaleMultiplier);

            foreach (Cell cell in Preferences.DebugShowWholeMap ? this.world.grid.allCells : this.world.grid.CellsVisitedByPlayer)
            {
                Rectangle srcDestRect = new Rectangle(
                    (int)Math.Floor(cell.xMin * this.scaleMultiplier),
                    (int)Math.Floor(cell.yMin * this.scaleMultiplier),
                    destCellWidth,
                    destCellHeight);

                SonOfRobinGame.spriteBatch.Draw(this.miniatureTerrainGfx, srcDestRect, srcDestRect, Color.White);
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
                        this.blocksUpdatesBelow = this.FullScreen && !Preferences.debugAllowMapAnimation;
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

                this.mapOverlay.AddTransition();
            }
        }

        public bool CheckIfPlayerCanReadTheMap(bool showMessage)
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
            if (this.Mode == MapMode.Off) return;

            if (this.MapMarker != null && !this.MapMarker.exists) this.MapMarker = null; // if marker had destroyed itself

            if (!this.CheckIfPlayerCanReadTheMap(showMessage: true))
            {
                this.TurnOff();
                return;
            }

            if (this.Mode == MapMode.Full) this.ProcessInput();
            else this.camera.TrackCoords(this.world.player.sprite.position);

            this.camera.Update(cameraCorrection: Vector2.Zero);
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
            this.camera.SetViewParams(this);
        }

        private void ProcessInput()
        {
            var touches = TouchInput.TouchPanelState;
            if (!touches.Any()) this.lastTouchPos = Vector2.Zero;

            // map mode switch

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapSwitch))
            {
                this.SwitchToNextMode();
                return;
            }

            // place or remove marker

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapToggleMarker))
            {
                if (this.MapMarker == null) this.MapMarker = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.camera.CurrentPos, templateName: PieceTemplate.Name.MapMarker);
                else
                {
                    this.MapMarker.Destroy();

                    if (Math.Abs(Vector2.Distance(this.MapMarker.sprite.position, this.camera.CurrentPos)) < 15) this.MapMarker = null;

                    else this.MapMarker = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.camera.CurrentPos, templateName: PieceTemplate.Name.MapMarker);

                }

                return;
            }

            // center on player

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapCenterPlayer)) this.camera.TrackCoords(position: this.world.player.sprite.position, moveInstantly: true);

            // zoom

            if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn) || InputMapper.IsPressed(InputMapper.Action.MapZoomOut))
            {
                bool zoomByMouse = Mouse.ScrollWheelRolledUp || Mouse.ScrollWheelRolledDown;

                float zoomMultiplier = zoomByMouse ? 0.4f : 0.035f;
                float zoomChangeVal = (zoomByMouse ? this.camera.TargetZoom : this.camera.CurrentZoom) * zoomMultiplier;

                float currentZoom = this.camera.CurrentZoom; // value to be replaced

                if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn))
                {
                    currentZoom = this.camera.CurrentZoom + zoomChangeVal;
                    currentZoom = Math.Min(currentZoom, this.InitialZoom);
                }

                if (InputMapper.IsPressed(InputMapper.Action.MapZoomOut))
                {
                    currentZoom = this.camera.CurrentZoom - zoomChangeVal;
                    currentZoom = Math.Max(currentZoom, this.scaleMultiplier * 0.8f);
                }

                this.camera.SetZoom(zoom: currentZoom, setInstantly: !zoomByMouse, zoomSpeedMultiplier: zoomByMouse ? 5f : 1f);
            }

            // movement

            Vector2 movement = InputMapper.Analog(InputMapper.Action.MapMove) * 10 / this.camera.CurrentZoom;
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

                        movement /= this.camera.CurrentZoom;
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

        }

        public override void RenderToTarget()
        {
            if (this.Mode == MapMode.Off || (this.Mode == MapMode.Mini && SonOfRobinGame.currentUpdate % 2 != 0)) return;

            this.UpdateBackground();

            this.SetViewParamsForTargetRender();
            SetRenderTarget(this.FinalMapToDisplay);

            // filling with water color

            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            // drawing paper map background texture

            Rectangle extendedMapRect = this.worldRect;
            extendedMapRect.Inflate(extendedMapRect.Width * 0.1f, extendedMapRect.Width * 0.1f); // width should be used twice

            SonOfRobinGame.spriteBatch.Draw(AnimData.framesForPkgs[AnimData.PkgName.Map].texture, extendedMapRect, Color.White);

            // drawing detailed or miniature background 

            float showDetailedMapAtZoom = (float)SonOfRobinGame.VirtualWidth / (float)this.world.width * 1.4f;
            bool showDetailedMap = this.camera.CurrentZoom >= showDetailedMapAtZoom;

            this.effectCol.TurnOnNextEffect(scene: this, currentUpdateToUse: SonOfRobinGame.currentUpdate);

            SonOfRobinGame.spriteBatch.Draw(this.miniatureCombinedGfx, this.worldRect, Color.White);
            if (showDetailedMap)
            {
                foreach (Cell cell in this.world.grid.GetCellsInsideRect(this.camera.viewRect))
                {
                    if (cell.VisitedByPlayer || Preferences.DebugShowWholeMap) cell.DrawBackground(opacity: 1f);
                }
            }

            this.StartNewSpriteBatch(enableEffects: false); // turning off effects

            // drawing pieces

            Rectangle worldCameraRectForSpriteSearch = this.camera.viewRect;
            // mini map displays far pieces on the sides
            if (this.Mode == MapMode.Mini) worldCameraRectForSpriteSearch.Inflate(worldCameraRectForSpriteSearch.Width, worldCameraRectForSpriteSearch.Height);

            var spritesBag = world.grid.GetSpritesForRect(groupName: Cell.Group.ColMovement, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, rectangle: worldCameraRectForSpriteSearch);

            var typesShownAlways = new List<Type> { typeof(Player), typeof(Workshop), typeof(Cooker), typeof(Shelter) };
            var namesShownAlways = new List<PieceTemplate.Name> { PieceTemplate.Name.MapMarker };
            var typesShownIfDiscovered = new List<Type> { typeof(Container) };
            var namesShownIfDiscovered = new List<PieceTemplate.Name> { PieceTemplate.Name.CrateStarting, PieceTemplate.Name.CrateRegular, PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig };

            if (Preferences.DebugShowWholeMap)
            {
                typesShownAlways.Add(typeof(Animal));
                typesShownAlways.AddRange(typesShownIfDiscovered);
            }

            if (this.MapMarker != null) spritesBag.Add(this.MapMarker.sprite);

            float spriteSize = 1f / this.camera.CurrentZoom * 0.25f;

            // regular "foreach", because spriteBatch is not thread-safe
            foreach (Sprite sprite in spritesBag.OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom))
            {
                BoardPiece piece = sprite.boardPiece;
                PieceTemplate.Name name = piece.name;
                Type pieceType = piece.GetType();

                bool showSprite = false;

                if (typesShownAlways.Contains(pieceType) || namesShownAlways.Contains(name)) showSprite = true;

                if (!showSprite && (sprite.hasBeenDiscovered) &&
                    (namesShownIfDiscovered.Contains(name) ||
                    typesShownIfDiscovered.Contains(pieceType))) showSprite = true;

                if (showSprite)
                {
                    Rectangle destRect = sprite.gfxRect;
                    destRect.Inflate(destRect.Width * spriteSize, destRect.Height * spriteSize);

                    if (this.Mode == MapMode.Mini && !this.camera.viewRect.Contains(destRect))
                    {
                        destRect.X = Math.Max(this.camera.viewRect.Left, destRect.X);
                        destRect.X = Math.Min(this.camera.viewRect.Right - destRect.Width, destRect.X);
                        destRect.Y = Math.Max(this.camera.viewRect.Top, destRect.Y);
                        destRect.Y = Math.Min(this.camera.viewRect.Bottom - destRect.Height, destRect.Y);
                    }

                    if (Preferences.debugAllowMapAnimation) sprite.UpdateAnimation();
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
