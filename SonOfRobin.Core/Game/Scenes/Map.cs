﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Map : Scene
    {
        public enum MapMode : byte
        {
            Mini,
            Full,
            Off,
        }

        public readonly World world;
        private readonly Camera camera;
        private readonly MapOverlay mapOverlay;
        private readonly EffInstance sketchEffect;
        private readonly Sound soundMarkerPlace = new(name: SoundData.Name.Ding4, pitchChange: 0f);
        public readonly Sound soundMarkerRemove = new(name: SoundData.Name.Ding4, pitchChange: -0.3f);

        public bool FullScreen
        { get { return this.Mode == MapMode.Full; } }

        private MapMode mode;
        private float scaleMultiplier;
        public bool dirtyFog;
        private RenderTarget2D lowResGround;
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

            if (this.lowResGround != null) this.lowResGround.Dispose();
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
            this.dirtyFog = true;

            this.UpdateResolution(force: true);
            this.UpdateBackground();
        }

        public void UpdateResolution(bool force = false)
        {
            float multiplierX = (float)SonOfRobinGame.VirtualWidth / (float)world.width;
            float multiplierY = (float)SonOfRobinGame.VirtualHeight / (float)world.height;
            this.scaleMultiplier = Math.Min(multiplierX, multiplierY) * 2;

            this.SetViewParamsForMiniature();

            if (force || this.lowResGround == null || this.lowResGround.Width != this.viewParams.Width || this.lowResGround.Height != this.viewParams.Height)
            {
                if (this.lowResGround != null) this.lowResGround.Dispose();
                this.lowResGround = new RenderTarget2D(SonOfRobinGame.GfxDev, this.viewParams.Width, this.viewParams.Height, true, SurfaceFormat.Color, DepthFormat.None);
                if (this.FinalMapToDisplay != null) this.FinalMapToDisplay.Dispose();

                this.FinalMapToDisplay = new RenderTarget2D(SonOfRobinGame.GfxDev, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None);

                this.dirtyFog = true;
            }

            this.UpdateBackground();
        }

        public void UpdateBackground()
        {
            if (this.lowResGround != null && this.dirtyFog == false) return;

            this.SetViewParamsForMiniature();

            MessageLog.AddMessage(debugMessage: true, message: $"{SonOfRobinGame.CurrentUpdate} updating map fog (fullscreen {this.FullScreen})");

            if (this.lowResGround == null || this.lowResGround.Width != this.viewParams.Width || this.lowResGround.Height != this.viewParams.Height)
            {
                if (this.lowResGround != null) this.lowResGround.Dispose();
                this.lowResGround = new RenderTarget2D(SonOfRobinGame.GfxDev, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            this.world.Grid.GenerateWholeIslandPreviewTextureIfMissing();

            SetRenderTarget(this.lowResGround);
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.GfxDev.Clear(Color.Transparent);

            int cellWidth = this.world.Grid.allCells[0].width;
            int cellHeight = this.world.Grid.allCells[0].height;
            int destCellWidth = (int)Math.Ceiling(cellWidth * this.scaleMultiplier);
            int destCellHeight = (int)Math.Ceiling(cellHeight * this.scaleMultiplier);

            float sourceMultiplier = 1f / (float)this.world.width * (float)this.world.Grid.WholeIslandPreviewTexture.Width;

            Rectangle srcRect = new Rectangle(0, 0, (int)(cellWidth * sourceMultiplier), (int)(cellHeight * sourceMultiplier));
            Rectangle destRect = new Rectangle(0, 0, destCellWidth, destCellHeight);

            foreach (Cell cell in Preferences.DebugShowWholeMap ? this.world.Grid.allCells : this.world.Grid.CellsVisitedByPlayer)
            {
                srcRect.X = (int)Math.Floor(cell.xMin * sourceMultiplier);
                srcRect.Y = (int)Math.Floor(cell.yMin * sourceMultiplier);

                destRect.X = (int)Math.Floor(cell.xMin * this.scaleMultiplier);
                destRect.Y = (int)Math.Floor(cell.yMin * this.scaleMultiplier);

                SonOfRobinGame.SpriteBatch.Draw(texture: this.world.Grid.WholeIslandPreviewTexture, destinationRectangle: destRect, sourceRectangle: srcRect, color: Color.White);
            }

            SonOfRobinGame.SpriteBatch.End();

            this.dirtyFog = false;
        }

        public void SwitchToNextMode()
        {
            this.Mode = this.Mode switch
            {
                MapMode.Mini => MapMode.Full,
                MapMode.Full => MapMode.Off,
                MapMode.Off => MapMode.Mini,
                _ => throw new ArgumentException($"Unsupported mode - {this.Mode}."),
            };
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
                if (this.world.HintEngine.shownTutorials.Contains(Tutorials.Type.TooDarkToReadMap))
                {
                    MessageLog.AddMessage(message: "Too dark to read the map.", avoidDuplicates: true);
                }
                else Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.TooDarkToReadMap, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return canBeTurnedOn;
        }

        public override void Update()
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

            float analogZoom = InputMapper.Analog(InputMapper.Action.MapZoom).Y;

            if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn) || InputMapper.IsPressed(InputMapper.Action.MapZoomOut) || TouchInput.IsBeingTouchedInAnyWay || analogZoom != 0)
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
                else
                {
                    if (analogZoom != 0) analogZoom = -analogZoom * currentZoom * 0.04f; // "* currentZoom" for proportional zooming

                    if (analogZoom == 0) analogZoom = TouchInput.GetZoomDelta(ignoreLeftStick: false, ignoreRightStick: false, ignoreVirtButtons: true, ignoreInventory: false, ignorePlayerPanel: false) * currentZoom * 1.5f;

                    currentZoom += analogZoom;
                }

                currentZoom = Math.Min(currentZoom, this.InitialZoom);
                currentZoom = Math.Max(currentZoom, this.scaleMultiplier * 0.4f);

                this.camera.SetZoom(zoom: currentZoom, setInstantly: !zoomByMouse, zoomSpeedMultiplier: zoomByMouse ? 5f : 1f);
            }

            // location names toggle

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapToggleLocations)) Preferences.mapShowLocationNames = !Preferences.mapShowLocationNames;

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
            if (this.Mode == MapMode.Off) return;

            this.UpdateBackground();

            this.SetViewParamsForTargetRender();
            SetRenderTarget(this.FinalMapToDisplay);

            // filling with water color

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.GfxDev.Clear(waterColor);

            // drawing paper map background texture

            Texture2D mapTexture = AnimData.framesForPkgs[AnimData.PkgName.Map].texture;
            Rectangle viewRect = this.camera.viewRect;
            Rectangle worldRect = this.world.worldRect;
            Rectangle extendedMapRect = this.world.worldRect;

            extendedMapRect.Inflate(extendedMapRect.Width * 0.1f, extendedMapRect.Height * 0.1f);

            SonOfRobinGame.SpriteBatch.Draw(mapTexture, extendedMapRect, Color.White); // should always be drawn (edges transparency is not perfect and blue background would be showing a little otherwise)

            // drawing ground
            bool showDetailedMap = this.camera.CurrentZoom >= 0.1f;
            // MessageLog.AddMessage( message: $"{SonOfRobinGame.CurrentUpdate} zoom {this.camera.CurrentZoom} showDetailedMap {showDetailedMap}");

            if (showDetailedMap)
            {
                var visibleCells = this.world.Grid.GetCellsInsideRect(rectangle: viewRect, addPadding: false);

                var cellsToDraw = new List<Cell>();
                var cellsToErase = new List<Cell>();

                if (!Preferences.DebugShowWholeMap)
                {
                    foreach (Cell cell in visibleCells)
                    {
                        if (cell.VisitedByPlayer) cellsToDraw.Add(cell);
                        else cellsToErase.Add(cell);
                    }
                }
                else cellsToDraw = visibleCells.ToList();

                foreach (Cell cell in cellsToDraw)
                {
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, cell.rect, waterColor);
                }

                SonOfRobinGame.SpriteBatch.End();

                SetupPolygonDrawing(allowRepeat: true, transformMatrix: this.TransformMatrix);
                BasicEffect basicEffect = SonOfRobinGame.BasicEffect;
                basicEffect.TextureEnabled = true;

                var meshesToDraw = new List<Mesh>();
                foreach (Mesh mesh in this.world.Grid.MeshGrid.GetMeshesForRect(viewRect))
                {
                    foreach (Cell cell in cellsToDraw)
                    {
                        if (cell.rect.Intersects(mesh.boundsRect))
                        {
                            meshesToDraw.Add(mesh);
                            break;
                        }
                    }
                }

                foreach (Mesh mesh in meshesToDraw.Distinct().OrderBy(mesh => mesh.drawPriority))
                {
                    basicEffect.Texture = TextureBank.GetTexturePersistent(mesh.textureName.Replace("textures/", "textures/map_"));

                    foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
                    {
                        effectPass.Apply();
                        mesh.Draw();
                    }
                }

                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

                float rectMultiplierX = 1f / (float)extendedMapRect.Width * (float)mapTexture.Width;
                float rectMultiplierY = 1f / (float)extendedMapRect.Height * (float)mapTexture.Height;

                Point extendedMapRectOffset = new Point(
                    (int)((float)(extendedMapRect.Width - worldRect.Width) * rectMultiplierX / 2f),
                    (int)((float)(extendedMapRect.Height - worldRect.Height) * rectMultiplierY / 2f)
                    );

                Rectangle sourceRect = new Rectangle(x: 0, y: 0, width: (int)(this.world.Grid.cellWidth * rectMultiplierX), height: (int)(this.world.Grid.cellHeight * rectMultiplierY));

                foreach (Cell cell in cellsToErase)
                {
                    sourceRect.X = (int)(cell.rect.X * rectMultiplierX) + extendedMapRectOffset.X;
                    sourceRect.Y = (int)(cell.rect.Y * rectMultiplierY) + extendedMapRectOffset.Y;

                    SonOfRobinGame.SpriteBatch.Draw(texture: mapTexture, sourceRectangle: sourceRect, destinationRectangle: cell.rect, color: Color.White);
                }
            }
            else // !showDetailedMap
            {
                SonOfRobinGame.SpriteBatch.Draw(this.lowResGround, worldRect, Color.White);
            }

            // drawing last steps

            float spriteSize = 1f / this.camera.CurrentZoom * (this.Mode == MapMode.Mini ? 1f : 0.25f); // to keep sprite size constant, regardless of zoom

            int totalSteps = this.world.Player.LastSteps.Count;
            int stepNo = 0;

            Texture2D stepTexture = TextureBank.GetTexture(TextureBank.TextureName.WhiteCircleSmall);
            Rectangle stepTextureRect = new(x: 0, y: 0, width: stepTexture.Width, stepTexture.Height);

            foreach (Vector2 stepPos in this.world.Player.LastSteps)
            {
                if (viewRect.Contains(stepPos))
                {
                    int maxSteps = Player.maxLastStepsCount;

                    float opacity = 1f;
                    if (totalSteps >= maxSteps / 2 && stepNo < maxSteps / 2)
                    {
                        float opacityFactor = 1f - (float)Math.Abs(stepNo - (maxSteps / 2)) / (maxSteps / 2);
                        opacity *= opacityFactor;
                    }

                    int rectSize = 8;
                    Rectangle blackRect = new(x: (int)(stepPos.X - (rectSize / 2)), y: (int)(stepPos.Y - (rectSize / 2)), width: rectSize, height: rectSize);
                    blackRect.Inflate(blackRect.Width * spriteSize, blackRect.Height * spriteSize);

                    SonOfRobinGame.SpriteBatch.Draw(stepTexture, blackRect, stepTextureRect, stepDotColor * opacity);
                }
                stepNo++;
            }

            // drawing pieces

            Rectangle worldCameraRectForSpriteSearch = viewRect;
            // mini map displays far pieces on the sides
            if (this.Mode == MapMode.Mini) worldCameraRectForSpriteSearch.Inflate(worldCameraRectForSpriteSearch.Width, worldCameraRectForSpriteSearch.Height);
            else worldCameraRectForSpriteSearch.Inflate(worldCameraRectForSpriteSearch.Width / 8, worldCameraRectForSpriteSearch.Height / 8);

            var spritesBag = this.world.Grid.GetSpritesForRectParallel(groupName: Cell.Group.ColMovement, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, rectangle: worldCameraRectForSpriteSearch, addPadding: false);

            var typesShownAlways = new List<Type> { typeof(Player), typeof(Workshop), typeof(Cooker), typeof(Shelter), typeof(AlchemyLab), typeof(Fireplace) };
            var namesShownAlways = new List<PieceTemplate.Name> { PieceTemplate.Name.MapMarker, PieceTemplate.Name.FenceHorizontalShort, PieceTemplate.Name.FenceVerticalShort };
            var typesShownIfDiscovered = new List<Type> { typeof(Container) };
            var namesShownIfDiscovered = new List<PieceTemplate.Name> { PieceTemplate.Name.CrateStarting, PieceTemplate.Name.CrateRegular, PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig, PieceTemplate.Name.Totem };

            if (Preferences.DebugShowWholeMap)
            {
                typesShownAlways.Add(typeof(Animal));
                typesShownAlways.AddRange(typesShownIfDiscovered);
                namesShownAlways.AddRange(namesShownIfDiscovered);
            }

            if (this.MapMarker != null) spritesBag.Add(this.MapMarker.sprite);

            // regular "foreach", because SpriteBatch is not thread-safe
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

                    int maxSize = 300; // to avoid sprites being too large (big tent, for example)

                    if (destRect.Width > destRect.Height)
                    {
                        if (destRect.Width > maxSize)
                        {
                            float aspect = (float)destRect.Height / (float)destRect.Width;
                            int widthReduction = destRect.Width - maxSize;
                            destRect.Inflate(-widthReduction / 2, -widthReduction * aspect / 2);
                        }
                    }
                    else
                    {
                        if (destRect.Height > maxSize)
                        {
                            float aspect = (float)destRect.Width / (float)destRect.Height;
                            int heightReduction = destRect.Height - maxSize;
                            destRect.Inflate(-heightReduction * aspect / 2, -heightReduction / 2);
                        }
                    }

                    if (this.Mode == MapMode.Mini && !viewRect.Contains(destRect))
                    {
                        destRect.X = Math.Max(viewRect.Left, destRect.X);
                        destRect.X = Math.Min(viewRect.Right - destRect.Width, destRect.X);
                        destRect.Y = Math.Max(viewRect.Top, destRect.Y);
                        destRect.Y = Math.Min(viewRect.Bottom - destRect.Height, destRect.Y);
                        opacity = 0.6f;
                    }

                    if (Preferences.debugAllowMapAnimation) sprite.UpdateAnimation(checkForCollision: false);
                    sprite.AnimFrame.Draw(destRect: destRect, color: Color.White, opacity: opacity);
                }
            }

            // drawing named locations

            if (Preferences.mapShowLocationNames && this.Mode == MapMode.Full)
            {
                SpriteFont locationFont = SonOfRobinGame.FontTommy20;
                var drawnNamesRects = new List<Rectangle>();

                float locationTextScale = Math.Min(1f / this.camera.CurrentZoom * 0.25f, 2f) * 3f;
                int outlineSize = Math.Max((int)(4 * locationTextScale), 4);

                foreach (NamedLocations.Location location in this.world.Grid.namedLocations.DiscoveredLocations)
                {
                    if (location.areaRect.Intersects(viewRect))
                    {
                        if (Preferences.debugShowNamedLocationAreas)
                        {
                            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, location.areaRect, location.Color * 0.25f);
                            Helpers.DrawRectangleOutline(rect: location.areaRect, color: Color.Black, borderWidth: outlineSize);
                            location.DrawCellRects(new Color(Math.Min(location.Color.R * 2, 255), Math.Min(location.Color.G * 2, 255), Math.Min(location.Color.B * 2, 255)) * 0.35f);
                        }

                        Vector2 textSize = locationFont.MeasureString(location.name);

                        Rectangle newTextRect = new(x: 0, y: 0, width: (int)(textSize.X * locationTextScale), height: (int)(textSize.Y * locationTextScale));
                        newTextRect.X = location.areaRect.Center.X - (newTextRect.Width / 2);
                        newTextRect.Y = location.areaRect.Center.Y - (newTextRect.Height / 2);

                        foreach (Rectangle checkedRect in drawnNamesRects)
                        {
                            if (newTextRect.Intersects(checkedRect))
                            {
                                bool newRectIsHigher = newTextRect.Center.Y < checkedRect.Center.Y;

                                int overlapY = newRectIsHigher ?
                                    newTextRect.Bottom - checkedRect.Top :
                                    checkedRect.Bottom - newTextRect.Top;

                                newTextRect.Y += overlapY * (newRectIsHigher ? -1 : 1);

                                break;
                            }
                        }

                        Helpers.DrawTextInsideRectWithOutline(font: locationFont, rectangle: newTextRect, text: location.name, color: Color.White, outlineColor: location.Color, outlineSize: outlineSize, drawTestRect: false);

                        drawnNamesRects.Add(newTextRect);
                    }
                }
            }

            // drawing map edges over everything

            SonOfRobinGame.SpriteBatch.Draw(TextureBank.GetTexture(TextureBank.TextureName.MapEdges), extendedMapRect, Color.White);

            // drawing crosshair

            if (this.Mode == MapMode.Full)
            {
                int crossHairSize = (int)(viewRect.Width * 0.02f);
                int crosshairHalfSize = crossHairSize / 2;

                Rectangle crosshairRect = new(x: (int)this.camera.CurrentPos.X - crosshairHalfSize, y: (int)this.camera.CurrentPos.Y - crosshairHalfSize, width: crossHairSize, height: crossHairSize);
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