using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private readonly MapOverlay mapOverlay;
        private readonly EffInstance sketchEffect;
        private readonly Sound soundMarkerPlace = new Sound(name: SoundData.Name.Ding4, pitchChange: 0f);
        public readonly Sound soundMarkerRemove = new Sound(name: SoundData.Name.Ding4, pitchChange: -0.3f);

        public bool FullScreen
        { get { return this.Mode == MapMode.Full; } }

        private MapMode mode;
        private float scaleMultiplier;
        private bool dirtyBackground;
        public bool dirtyFog;
        private RenderTarget2D lowResWholeTerrainGfx;
        private RenderTarget2D lowResWholeCombinedGfx;
        public RenderTarget2D FinalMapToDisplay { get; private set; }
        public BoardPiece MapMarker { get; private set; }

        private float InitialZoom
        { get { return Preferences.WorldScale / 2; } }

        private static readonly Color paperColor = new Color(214, 199, 133, 255);
        public static readonly Color waterColor = new Color(8, 108, 160, 255);
        private static readonly Color stepDotColor = new Color(56, 36, 0);

        public Map(World world, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.mapOverlay = new MapOverlay(this);
            this.AddLinkedScene(this.mapOverlay);
            this.world = world;
            this.camera = new Camera(world: this.world, useWorldScale: false, useFluidMotionForMove: false, useFluidMotionForZoom: true, keepInWorldBounds: false);
            this.mode = MapMode.Off;
            this.dirtyFog = true;
            this.sketchEffect = new SketchInstance(fgColor: new Color(107, 98, 87, 255), bgColor: paperColor, framesLeft: -1);
        }

        public override void Remove()
        {
            base.Remove();

            if (this.lowResWholeTerrainGfx != null) this.lowResWholeTerrainGfx.Dispose();
            if (this.lowResWholeCombinedGfx != null) this.lowResWholeCombinedGfx.Dispose();
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

            if (force || this.lowResWholeTerrainGfx == null || this.lowResWholeTerrainGfx.Width != this.viewParams.Width || this.lowResWholeTerrainGfx.Height != this.viewParams.Height)
            {
                if (this.lowResWholeTerrainGfx != null) this.lowResWholeTerrainGfx.Dispose();
                this.lowResWholeTerrainGfx = new RenderTarget2D(SonOfRobinGame.GfxDev, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
                if (this.FinalMapToDisplay != null) this.FinalMapToDisplay.Dispose();

                this.FinalMapToDisplay = new RenderTarget2D(SonOfRobinGame.GfxDev, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None);

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

                SetRenderTarget(this.lowResWholeTerrainGfx);
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

                int width = (int)(this.world.width * this.scaleMultiplier);
                int height = (int)(this.world.height * this.scaleMultiplier);

                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.CurrentUpdate} updating map background {width}x{height} (fullscreen {this.FullScreen})");

                Texture2D mapTexture = BoardGraphics.GetMapTextureScaledForScreenSize(grid: this.world.Grid, width: width, height: height);
                Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
                SonOfRobinGame.SpriteBatch.Draw(mapTexture, sourceRectangle, sourceRectangle, Color.White);
                SonOfRobinGame.SpriteBatch.End();
                mapTexture.Dispose();

                this.dirtyBackground = false;
                this.dirtyFog = true;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.CurrentUpdate} map background updated ({this.viewParams.Width}x{this.viewParams.Height}).", color: Color.White);
            }

            this.UpdateCombinedGfx();
        }

        private void UpdateCombinedGfx()
        {
            if (this.lowResWholeCombinedGfx != null && this.dirtyFog == false) return;

            this.SetViewParamsForMiniature();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.CurrentUpdate} updating map fog (fullscreen {this.FullScreen})");

            if (this.lowResWholeCombinedGfx == null || this.lowResWholeCombinedGfx.Width != this.viewParams.Width || this.lowResWholeCombinedGfx.Height != this.viewParams.Height)
            {
                if (this.lowResWholeCombinedGfx != null) this.lowResWholeCombinedGfx.Dispose();
                this.lowResWholeCombinedGfx = new RenderTarget2D(SonOfRobinGame.GfxDev, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            SetRenderTarget(this.lowResWholeCombinedGfx);
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.GfxDev.Clear(Color.Transparent);

            int cellWidth = this.world.Grid.allCells[0].width;
            int cellHeight = this.world.Grid.allCells[0].height;
            int destCellWidth = (int)Math.Ceiling(cellWidth * this.scaleMultiplier);
            int destCellHeight = (int)Math.Ceiling(cellHeight * this.scaleMultiplier);

            foreach (Cell cell in Preferences.DebugShowWholeMap ? this.world.Grid.allCells : this.world.Grid.CellsVisitedByPlayer)
            {
                Rectangle srcDestRect = new Rectangle(
                    (int)Math.Floor(cell.xMin * this.scaleMultiplier),
                    (int)Math.Floor(cell.yMin * this.scaleMultiplier),
                    destCellWidth,
                    destCellHeight);

                SonOfRobinGame.SpriteBatch.Draw(this.lowResWholeTerrainGfx, srcDestRect, srcDestRect, Color.White);
            }

            SonOfRobinGame.SpriteBatch.End();

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
                        this.camera.TrackCoords(position: this.world.Player.sprite.position, moveInstantly: true);
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
            bool canBeTurnedOn = this.world.Player.CanSeeAnything;

            if (!canBeTurnedOn && showMessage && this.world.Player.activeState != BoardPiece.State.PlayerControlledSleep && GetTopSceneOfType(typeof(TextWindow)) == null)
            {
                if (this.world.HintEngine.shownTutorials.Contains(Tutorials.Type.TooDarkToReadMap)) new TextWindow(text: "It is too dark to read the map.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.world.DialogueSound);
                else Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.TooDarkToReadMap, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return canBeTurnedOn;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Mode == MapMode.Off) return;

            if (this.MapMarker != null && !this.MapMarker.exists) this.MapMarker = null; // if marker had destroyed itself
            if (this.MapMarker != null && !this.MapMarker.sprite.IsOnBoard)
            {
                this.MapMarker.Destroy();
                this.MapMarker = null;
            }

            if (!this.CheckIfPlayerCanReadTheMap(showMessage: true))
            {
                this.TurnOff();
                return;
            }

            if (this.Mode == MapMode.Full) this.ProcessInput();
            else this.camera.TrackCoords(this.world.Player.sprite.position);

            this.camera.Update(cameraCorrection: Vector2.Zero);
        }

        private void SetViewParamsForMiniature()
        {
            this.viewParams.Width = (int)(this.world.width * this.scaleMultiplier);
            this.viewParams.Height = (int)(this.world.height * this.scaleMultiplier);
            this.viewParams.ScaleX = Preferences.GlobalScale;
            this.viewParams.ScaleY = Preferences.GlobalScale;
            this.viewParams.PosX = 0;
            this.viewParams.PosY = 0;
        }

        private void SetViewParamsForTargetRender()
        {
            this.camera.SetViewParams(this);
        }

        private void ProcessInput()
        {
            // map mode switch

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapSwitch))
            {
                this.SwitchToNextMode();
                return;
            }

            // place or remove marker

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapToggleMarker))
            {
                if (this.MapMarker == null)
                {
                    this.MapMarker = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.camera.CurrentPos, templateName: PieceTemplate.Name.MapMarker);
                    soundMarkerPlace.Play();
                }
                else
                {
                    if (Math.Abs(Vector2.Distance(this.MapMarker.sprite.position, this.camera.CurrentPos)) < 15)
                    {
                        this.MapMarker.Destroy();
                        this.MapMarker = null;
                        soundMarkerRemove.Play();
                    }
                    else
                    {
                        this.MapMarker.sprite.SetNewPosition(newPos: this.camera.CurrentPos, ignoreCollisions: true);
                        soundMarkerPlace.Play();
                    }
                }

                return;
            }

            // center on player

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapCenterPlayer)) this.camera.TrackCoords(position: this.world.Player.sprite.position, moveInstantly: true);

            // zoom

            if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn) || InputMapper.IsPressed(InputMapper.Action.MapZoomOut) || TouchInput.IsBeingTouchedInAnyWay)
            {
                bool zoomByMouse = Mouse.ScrollWheelRolledUp || Mouse.ScrollWheelRolledDown;

                float zoomMultiplier = zoomByMouse ? 0.4f : 0.035f;
                float zoomChangeVal = (zoomByMouse ? this.camera.TargetZoom : this.camera.CurrentZoom) * zoomMultiplier;

                float currentZoom = this.camera.CurrentZoom; // value to be replaced

                bool zoomButtonPressed = InputMapper.IsPressed(InputMapper.Action.MapZoomIn) || InputMapper.IsPressed(InputMapper.Action.MapZoomOut);
                if (zoomButtonPressed)
                {
                    if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn)) currentZoom = this.camera.CurrentZoom + zoomChangeVal;
                    if (InputMapper.IsPressed(InputMapper.Action.MapZoomOut)) currentZoom = this.camera.CurrentZoom - zoomChangeVal;
                }
                else currentZoom += TouchInput.GetZoomDelta(ignoreLeftStick: false, ignoreRightStick: false, ignoreVirtButtons: true, ignoreInventory: false, ignorePlayerPanel: false);

                currentZoom = Math.Min(currentZoom, this.InitialZoom);
                currentZoom = Math.Max(currentZoom, this.scaleMultiplier * 0.8f);

                this.camera.SetZoom(zoom: currentZoom, setInstantly: !zoomByMouse, zoomSpeedMultiplier: zoomByMouse ? 5f : 1f);
            }

            // movement

            Vector2 movement = InputMapper.Analog(InputMapper.Action.MapMove) * 10 / this.camera.CurrentZoom;
            if (movement == Vector2.Zero)
            {
                // using touch
                movement = TouchInput.GetMovementDelta(ignoreLeftStick: false, ignoreRightStick: false, ignoreVirtButtons: true, ignoreInventory: false, ignorePlayerPanel: false) / Preferences.GlobalScale / this.camera.CurrentZoom;
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
            if (this.Mode == MapMode.Off || (this.Mode == MapMode.Mini && SonOfRobinGame.CurrentUpdate % 2 != 0)) return;

            this.UpdateBackground();

            this.SetViewParamsForTargetRender();
            SetRenderTarget(this.FinalMapToDisplay);

            // filling with water color

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.GfxDev.Clear(waterColor);

            // drawing paper map background texture

            Rectangle extendedMapRect = this.world.worldRect;
            extendedMapRect.Inflate(extendedMapRect.Width * 0.1f, extendedMapRect.Height * 0.1f);

            SonOfRobinGame.SpriteBatch.Draw(AnimData.framesForPkgs[AnimData.PkgName.Map].texture, extendedMapRect, Color.White);
            SonOfRobinGame.SpriteBatch.End();

            // drawing background

            float showDetailedMapAtZoom = (float)SonOfRobinGame.VirtualWidth / (float)this.world.width * 1.4f;
            bool showDetailedMap = this.camera.CurrentZoom >= showDetailedMapAtZoom;

            var cellsToDraw = (IEnumerable<Cell>)new List<Cell>(); // to be replaced later
            bool foundCellsWithMissingTextures = false;

            if (showDetailedMap)
            {
                cellsToDraw = this.world.Grid.GetCellsInsideRect(viewRect: this.camera.viewRect, addPadding: false);
                if (!Preferences.DebugShowWholeMap) cellsToDraw = cellsToDraw.Where(cell => cell.VisitedByPlayer);

                foreach (Cell cell in cellsToDraw)
                {
                    if (cell.boardGraphics.Texture == null)
                    {
                        foundCellsWithMissingTextures = true;
                        break;
                    }
                }

                if (foundCellsWithMissingTextures)
                {
                    // loading and unloading textures should be done during "foundCellsWithMissingTextures" only, to avoid unnecessary texture loading

                    this.world.Grid.UnloadTexturesIfMemoryLow(this.camera);
                    this.world.Grid.LoadClosestTexturesInCameraView(camera: this.camera, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, maxNoToLoad: 200);
                }
            }

            if (!showDetailedMap || foundCellsWithMissingTextures)
            {
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate);
                this.sketchEffect.TurnOn(currentUpdate: SonOfRobinGame.CurrentUpdate);

                SonOfRobinGame.SpriteBatch.Draw(this.lowResWholeCombinedGfx, this.world.worldRect, Color.White);
                SonOfRobinGame.SpriteBatch.End();
            }

            if (showDetailedMap)
            {
                // Water have to be drawn without the shader (will not display correctly otherwise),
                // but have to retain lowResWholeCombinedGfx water color (changed by shader).
                Color waterColorWithShader = new Color(89, 99, 81);

                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix); // starting new spriteBatch, to turn off effects for drawing water rectangles
                foreach (Cell cell in cellsToDraw)
                {
                    cell.DrawBackgroundWaterSimulation(waterColor: waterColorWithShader);
                }
                SonOfRobinGame.SpriteBatch.End();

                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate);
                this.sketchEffect.TurnOn(currentUpdate: SonOfRobinGame.CurrentUpdate); // turning effects back on
                foreach (Cell cell in cellsToDraw)
                {
                    if (cell.boardGraphics.Texture != null) cell.DrawBackground(opacity: 1f);
                }

                SonOfRobinGame.SpriteBatch.End();
            }

            // drawing last steps (without effects)

            float spriteSize = 1f / this.camera.CurrentZoom * (this.Mode == MapMode.Mini ? 1f : 0.25f); // to keep sprite size constant, regardless of zoom

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            int totalSteps = this.world.Player.LastSteps.Count;
            int stepNo = 0;
            foreach (Vector2 stepPos in this.world.Player.LastSteps)
            {
                if (this.camera.viewRect.Contains(stepPos))
                {
                    int maxSteps = Player.maxLastStepsCount;

                    float opacity = 1f;
                    if (totalSteps >= maxSteps / 2 && stepNo < maxSteps / 2)
                    {
                        float opacityFactor = 1f - (float)Math.Abs(stepNo - (maxSteps / 2)) / (maxSteps / 2);
                        opacity *= opacityFactor;
                    }

                    int rectSize = 8;
                    Rectangle blackRect = new Rectangle(x: (int)(stepPos.X - (rectSize / 2)), y: (int)(stepPos.Y - (rectSize / 2)), width: rectSize, height: rectSize);
                    blackRect.Inflate(blackRect.Width * spriteSize, blackRect.Height * spriteSize);

                    AnimData.framesForPkgs[AnimData.PkgName.SmallWhiteCircle].Draw(blackRect, color: stepDotColor, opacity: opacity);
                }
                stepNo++;
            }

            // drawing pieces (without effects)

            Rectangle worldCameraRectForSpriteSearch = this.camera.viewRect;
            // mini map displays far pieces on the sides
            if (this.Mode == MapMode.Mini) worldCameraRectForSpriteSearch.Inflate(worldCameraRectForSpriteSearch.Width, worldCameraRectForSpriteSearch.Height);

            var spritesBag = world.Grid.GetSpritesForRect(groupName: Cell.Group.ColMovement, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, rectangle: worldCameraRectForSpriteSearch);

            var typesShownAlways = new List<Type> { typeof(Player), typeof(Workshop), typeof(Cooker), typeof(Shelter) };
            var namesShownAlways = new List<PieceTemplate.Name> { PieceTemplate.Name.MapMarker };
            var typesShownIfDiscovered = new List<Type> { typeof(Container) };
            var namesShownIfDiscovered = new List<PieceTemplate.Name> { PieceTemplate.Name.CrateStarting, PieceTemplate.Name.CrateRegular, PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig };

            if (Preferences.DebugShowWholeMap)
            {
                typesShownAlways.Add(typeof(Animal));
                typesShownAlways.AddRange(typesShownIfDiscovered);
                namesShownAlways.AddRange(namesShownIfDiscovered);
            }

            if (this.MapMarker != null) spritesBag.Add(this.MapMarker.sprite);

            // regular "foreach", because spriteBatch is not thread-safe
            foreach (Sprite sprite in spritesBag.OrderBy(o => o.AnimFrame.layer).ThenBy(o => o.GfxRect.Bottom))
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
                    float opacity = 1f;
                    Rectangle destRect = sprite.GfxRect;

                    destRect.Inflate(destRect.Width * spriteSize, destRect.Height * spriteSize);

                    int maxWidth = 300; // to avoid sprites being too large (big tent, for example)
                    if (destRect.Width > maxWidth)
                    {
                        float aspect = (float)destRect.Height / (float)destRect.Width;
                        int widthReduction = destRect.Width - maxWidth;
                        destRect.Inflate(-widthReduction / 2, (-widthReduction * aspect) / 2);
                    }

                    if (this.Mode == MapMode.Mini && !this.camera.viewRect.Contains(destRect))
                    {
                        destRect.X = Math.Max(this.camera.viewRect.Left, destRect.X);
                        destRect.X = Math.Min(this.camera.viewRect.Right - destRect.Width, destRect.X);
                        destRect.Y = Math.Max(this.camera.viewRect.Top, destRect.Y);
                        destRect.Y = Math.Min(this.camera.viewRect.Bottom - destRect.Height, destRect.Y);
                        opacity = 0.6f;
                    }

                    if (Preferences.debugAllowMapAnimation) sprite.UpdateAnimation(checkForCollision: false);
                    sprite.AnimFrame.Draw(destRect: destRect, color: Color.White, opacity: opacity);
                }
            }

            // drawing map edges over everything

            SonOfRobinGame.SpriteBatch.Draw(AnimData.framesForPkgs[AnimData.PkgName.MapEdges].texture, extendedMapRect, Color.White);

            // drawing crosshair

            if (this.Mode == MapMode.Full)
            {
                int crossHairSize = (int)(this.camera.viewRect.Width * 0.02f);
                int crosshairHalfSize = crossHairSize / 2;

                Rectangle crosshairRect = new Rectangle(x: (int)this.camera.CurrentPos.X - crosshairHalfSize, y: (int)this.camera.CurrentPos.Y - crosshairHalfSize, width: crossHairSize, height: crossHairSize);
                AnimFrame crosshairFrame = PieceInfo.GetInfo(PieceTemplate.Name.Crosshair).frame;
                crosshairFrame.DrawAndKeepInRectBounds(destBoundsRect: crosshairRect, color: Color.White);
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public override void Draw()
        {
            // is being drawn in MapOverlay scene
        }
    }
}