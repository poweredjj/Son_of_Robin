using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly SpriteFontBase locationFont;
        private readonly Camera camera;
        private Task bgTaskForMeshes;
        private Task bgTaskForSprites;
        private Rectangle bgTaskMeshesLastCameraRect;
        private Rectangle bgTaskSpritesLastCameraRect;
        private List<Sprite> bgTaskSpritesToShow;
        private List<Mesh> bgTaskMeshesToShow;
        private readonly MapOverlay mapOverlay;
        private readonly Sound soundMarkerPlace = new(name: SoundData.Name.Ding4, pitchChange: 0f);
        public readonly Sound soundMarkerRemove = new(name: SoundData.Name.Ding4, pitchChange: -0.3f);

        public bool FullScreen
        { get { return this.Mode == MapMode.Full; } }

        private MapMode mode;
        private float scaleMultiplier;
        public bool backgroundNeedsUpdating;
        private RenderTarget2D lowResGround;
        public RenderTarget2D FinalMapToDisplay { get; private set; }

        public readonly Dictionary<Color, BoardPiece> mapMarkerByColor;

        private static float InitialZoom
        { get { return Preferences.WorldScale / 2; } }

        private bool ShowDetailedMap
        { get { return this.camera.CurrentZoom >= showDetailedMapZoom; } }

        private const float showDetailedMapZoom = 0.08f;
        public static readonly Color waterColor = new Color(8, 108, 160, 255);
        private static readonly Color stepDotColor = new Color(56, 36, 0);

        public Map(World world, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.mapOverlay = new MapOverlay(this);
            this.AddLinkedScene(this.mapOverlay);
            this.world = world;
            this.camera = new Camera(world: this.world, useWorldScale: false, useFluidMotionForMove: true, useFluidMotionForZoom: true, keepInWorldBounds: false);
            this.mode = MapMode.Off;
            this.backgroundNeedsUpdating = true;
            this.bgTaskMeshesLastCameraRect = new Rectangle();
            this.bgTaskSpritesLastCameraRect = new Rectangle();
            this.bgTaskMeshesToShow = new List<Mesh>();
            this.bgTaskSpritesToShow = new List<Sprite>();
            this.locationFont = SonOfRobinGame.FontTommy.GetFont(20);
            this.mapMarkerByColor = new Dictionary<Color, BoardPiece>
            {
                { Color.Blue, null },
                { new Color(15, 128, 0), null },
                { Color.Red, null },
                { new Color(93, 6, 99), null },
                { new Color(97, 68, 15), null },
            };
        }

        public override void Remove()
        {
            base.Remove();

            if (this.lowResGround != null) this.lowResGround.Dispose();
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
            this.backgroundNeedsUpdating = true;

            this.UpdateResolution();
        }

        public Dictionary<string, Object> Serialize()
        {
            var markerPosDict = new Dictionary<Vector2, byte[]>();
            foreach (var kvp in this.mapMarkerByColor)
            {
                Color markerColor = kvp.Key;
                BoardPiece markerPiece = kvp.Value;

                if (markerPiece == null) continue;

                var colorArray = new byte[] { markerColor.R, markerColor.G, markerColor.B, markerColor.A };
                markerPosDict[markerPiece.sprite.position] = colorArray;
            }

            var mapDataDict = new Dictionary<string, Object>
            {
              { "markerPosDict", markerPosDict },
            };

            return mapDataDict;
        }

        public void Deserialize(Object mapData)
        {
            var mapDataDict = (Dictionary<string, Object>)mapData;

            var markerPosDict = (Dictionary<Vector2, byte[]>)mapDataDict["markerPosDict"];
            foreach (var kvp in markerPosDict)
            {
                Vector2 markerPos = kvp.Key;
                var colorArray = (byte[])kvp.Value;

                Color markerColor = new Color(r: colorArray[0], g: colorArray[1], b: colorArray[2], alpha: colorArray[3]);

                if (this.mapMarkerByColor.ContainsKey(markerColor))
                {
                    this.mapMarkerByColor[markerColor] = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: markerPos, templateName: PieceTemplate.Name.MapMarker);
                }
                else
                {
                    SonOfRobinGame.MessageLog.Add(debugMessage: true, text: $"Cannot deserialize marker - color not found: {colorArray}.");
                }
            }
        }

        public void UpdateResolution()
        {
            float multiplierX = (float)SonOfRobinGame.VirtualWidth / (float)world.width;
            float multiplierY = (float)SonOfRobinGame.VirtualHeight / (float)world.height;
            this.scaleMultiplier = Math.Min(multiplierX, multiplierY) * 2;

            this.FinalMapToDisplay?.Dispose();

            this.FinalMapToDisplay = new RenderTarget2D(SonOfRobinGame.GfxDev, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None);

            this.SetViewParamsForMiniature();

            if (this.lowResGround == null || this.lowResGround.Width != this.viewParams.Width || this.lowResGround.Height != this.viewParams.Height)
            {
                this.lowResGround?.Dispose();
                this.lowResGround = new RenderTarget2D(SonOfRobinGame.GfxDev, this.viewParams.Width, this.viewParams.Height, true, SurfaceFormat.Color, DepthFormat.None);
            }

            this.backgroundNeedsUpdating = true;
        }

        public void UpdateBackground()
        {
            if (this.lowResGround != null && this.backgroundNeedsUpdating == false) return;

            this.world.Grid.GenerateWholeIslandPreviewTextureIfMissing();

            this.SetViewParamsForMiniature();

            SonOfRobinGame.MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} updating map background (fullscreen {this.FullScreen})");

            if (this.lowResGround == null || this.lowResGround.Width != this.viewParams.Width || this.lowResGround.Height != this.viewParams.Height)
            {
                this.lowResGround?.Dispose();
                this.lowResGround = new RenderTarget2D(SonOfRobinGame.GfxDev, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

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

            this.backgroundNeedsUpdating = false;
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
                        this.camera.TrackPiece(trackedPiece: this.world.Player, moveInstantly: true);
                        this.camera.SetZoom(zoom: InitialZoom, setInstantly: true);
                        this.camera.ResetMovementSpeed();
                        this.UpdateResolution();
                        this.blocksUpdatesBelow = false;
                        this.InputType = InputTypes.None;
                        this.drawActive = true;
                        break;

                    case MapMode.Full:
                        Sound.QuickPlay(SoundData.Name.PaperMove1);
                        this.camera.SetMovementSpeed(4f * this.camera.CurrentZoom);
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
                    SonOfRobinGame.MessageLog.Add(text: "Too dark to read the map.", texture: PieceInfo.GetTexture(PieceTemplate.Name.Map), bgColor: new Color(105, 3, 18), avoidDuplicates: true);
                }
                else Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.TooDarkToReadMap, world: this.world, ignoreDelay: true, ignoreHintsSetting: true);
            }

            return canBeTurnedOn;
        }

        public override void Update()
        {
            if (this.Mode == MapMode.Off) return;

            this.KeepBackgroundTasksAlive();

            foreach (Color markerColor in this.mapMarkerByColor.Keys.ToList())
            {
                BoardPiece mapMarker = this.mapMarkerByColor[markerColor];
                if (mapMarker != null && !mapMarker.exists) this.mapMarkerByColor[markerColor] = null; // if marker had destroyed itself

                mapMarker = this.mapMarkerByColor[markerColor]; // refreshing, if changed above

                if (mapMarker != null && !mapMarker.sprite.IsOnBoard)
                {
                    mapMarker.Destroy();
                    this.mapMarkerByColor[markerColor] = null;
                }
            }

            if (!this.CheckIfPlayerCanReadTheMap(showMessage: true))
            {
                this.TurnOff();
                return;
            }

            if (this.Mode == MapMode.Full)
            {
                foreach (BoardPiece mapMarker in this.mapMarkerByColor.Values)
                {
                    if (mapMarker != null)
                    {
                        VirtButton.ButtonHighlightOnNextFrame(VButName.MapDeleteMarkers);
                        ControlTips.TipHighlightOnNextFrame(tipName: "delete all markers");
                        break;
                    }
                }

                this.ProcessInput();
            }

            this.camera.Update(cameraCorrection: Vector2.Zero, calculateAheadCorrection: false);
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

            // removing all markers

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapDeleteMarkers))
            {
                bool anyMarkerRemoved = false;
                foreach (Color markerColor in this.mapMarkerByColor.Keys.ToList())
                {
                    if (this.mapMarkerByColor[markerColor] != null)
                    {
                        this.mapMarkerByColor[markerColor].Destroy();
                        this.world.map.mapMarkerByColor[markerColor] = null;
                        anyMarkerRemoved = true;
                    }
                }

                if (anyMarkerRemoved) soundMarkerRemove.Play();
                return;
            }

            // place or remove marker

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapToggleMarker))
            {
                // trying to remove existing marker

                bool markerRemoved = false;

                foreach (var kvp in new Dictionary<Color, BoardPiece>(this.world.map.mapMarkerByColor))
                {
                    Color markerColor = kvp.Key;
                    BoardPiece markerPiece = kvp.Value;

                    if (markerPiece != null)
                    {
                        if (Math.Abs(Vector2.Distance(markerPiece.sprite.position, this.camera.CurrentPos)) < 15 / this.camera.CurrentZoom)
                        {
                            markerPiece.Destroy();
                            this.world.map.mapMarkerByColor[markerColor] = null;
                            soundMarkerRemove.Play();
                            markerRemoved = true;
                            break;
                        }
                    }
                }

                if (markerRemoved) return;

                // placing new marker (if possible)

                foreach (var kvp in new Dictionary<Color, BoardPiece>(this.world.map.mapMarkerByColor))
                {
                    Color markerColor = kvp.Key;
                    BoardPiece markerPiece = kvp.Value;

                    if (markerPiece == null)
                    {
                        markerPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.camera.CurrentPos, templateName: PieceTemplate.Name.MapMarker);
                        this.world.map.mapMarkerByColor[markerColor] = markerPiece;
                        soundMarkerPlace.Play();
                        return;
                    }
                }

                Sound.QuickPlay(SoundData.Name.Error);
                SonOfRobinGame.MessageLog.Add(text: "Cannot place new marker - all markers are in use.", texture: PieceInfo.GetTexture(PieceTemplate.Name.MapMarker), bgColor: new Color(105, 3, 18), avoidDuplicates: true);
                return;
            }

            // center on player

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapCenterPlayer)) this.camera.TrackCoords(position: this.world.Player.sprite.position, moveInstantly: true);

            // zoom

            float analogZoom = InputMapper.Analog(InputMapper.Action.MapZoom).Y;

            if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn) || InputMapper.IsPressed(InputMapper.Action.MapZoomOut) || TouchInput.IsBeingTouchedInAnyWay || analogZoom != 0)
            {
                bool zoomInstantly = false;
                bool zoomByMouse = Mouse.ScrollWheelRolledUp || Mouse.ScrollWheelRolledDown;

                float zoomMultiplier = zoomByMouse ? 0.4f : 0.035f;
                float zoomChangeVal = (zoomByMouse ? this.camera.TargetZoom : this.camera.CurrentZoom) * zoomMultiplier;

                float currentZoom = this.camera.TargetZoom; // value to be replaced

                bool zoomButtonPressed = InputMapper.IsPressed(InputMapper.Action.MapZoomIn) || InputMapper.IsPressed(InputMapper.Action.MapZoomOut);
                if (zoomButtonPressed)
                {
                    if (InputMapper.IsPressed(InputMapper.Action.MapZoomIn)) currentZoom = this.camera.TargetZoom + zoomChangeVal;
                    if (InputMapper.IsPressed(InputMapper.Action.MapZoomOut)) currentZoom = this.camera.TargetZoom - zoomChangeVal;
                }
                else
                {
                    if (analogZoom != 0) analogZoom = -analogZoom * currentZoom * 0.04f; // "* currentZoom" for proportional zooming

                    if (analogZoom == 0)
                    {
                        analogZoom = TouchInput.GetZoomDelta(ignoreLeftStick: false, ignoreRightStick: false, ignoreVirtButtons: true, ignoreInventory: false, ignorePlayerPanel: false) * currentZoom * 1.5f;
                        if (analogZoom != 0f) zoomInstantly = true;
                    }

                    currentZoom += analogZoom;
                }

                currentZoom = Math.Min(currentZoom, InitialZoom);
                currentZoom = Math.Max(currentZoom, this.scaleMultiplier * 0.4f);

                this.camera.SetZoom(zoom: currentZoom, setInstantly: zoomInstantly, zoomSpeedMultiplier: zoomByMouse ? 5f : 2.5f);
            }

            // location names toggle

            if (InputMapper.HasBeenPressed(InputMapper.Action.MapToggleLocations)) Preferences.mapShowLocationNames = !Preferences.mapShowLocationNames;

            // movement

            bool moveInstantly = false;
            Vector2 movement = InputMapper.Analog(InputMapper.Action.MapMove) * 10 / this.camera.CurrentZoom;
            if (movement == Vector2.Zero)
            {
                // using touch
                movement = TouchInput.GetMovementDelta(ignoreLeftStick: false, ignoreRightStick: false, ignoreVirtButtons: true, ignoreInventory: false, ignorePlayerPanel: false) / Preferences.GlobalScale / this.camera.CurrentZoom;
                if (movement != Vector2.Zero) moveInstantly = true;
            }

            if (movement != Vector2.Zero)
            {
                Vector2 newPos = this.camera.TrackedPos + movement;

                newPos.X = Math.Clamp(value: newPos.X, min: 0, max: this.world.width);
                newPos.Y = Math.Clamp(value: newPos.Y, min: 0, max: this.world.height);
                this.camera.TrackCoords(position: newPos, moveInstantly: moveInstantly);
            }

            if (!TouchInput.IsBeingTouchedInAnyWay && movement == Vector2.Zero)
            {
                foreach (BoardPiece mapMarker in this.mapMarkerByColor.Values)
                {
                    if (mapMarker != null && Vector2.Distance(this.camera.CurrentPos, mapMarker.sprite.position) < 15f / this.camera.CurrentZoom)
                    {
                        Vector2 coordsToTrack = mapMarker.sprite.position + new Vector2(1f / this.camera.CurrentZoom, 1f / this.camera.CurrentZoom);
                        if (this.camera.GetTargetCoords() != coordsToTrack)
                        {
                            new RumbleEvent(force: 0.02f, smallMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.08f, minSecondsSinceLastRumble: 0.5f);
                            this.camera.TrackCoords(position: coordsToTrack, moveInstantly: moveInstantly);
                        }

                        break;
                    }
                }
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

            // SonOfRobinGame.messageLog.AddMessage(text: $"{SonOfRobinGame.CurrentUpdate} zoom {this.camera.CurrentZoom}");

            // drawing ground
            if (this.ShowDetailedMap)
            {
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, worldRect, waterColor);
                SonOfRobinGame.SpriteBatch.End();

                SetupPolygonDrawing(allowRepeat: true, transformMatrix: this.TransformMatrix);
                BasicEffect basicEffect = SonOfRobinGame.BasicEffect;

                foreach (Mesh mesh in this.bgTaskMeshesToShow.ToArray())
                {
                    basicEffect.Texture = mesh.meshDef.mapTexture;

                    foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
                    {
                        effectPass.Apply();
                        mesh.Draw(processTweeners: false);
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

                var visibleCells = this.world.Grid.GetCellsInsideRect(rectangle: viewRect, addPadding: false);

                if (!Preferences.DebugShowWholeMap)
                {
                    foreach (Cell cell in visibleCells)
                    {
                        if (!cell.visitedByPlayer)
                        {
                            sourceRect.X = (int)(cell.rect.X * rectMultiplierX) + extendedMapRectOffset.X;
                            sourceRect.Y = (int)(cell.rect.Y * rectMultiplierY) + extendedMapRectOffset.Y;

                            SonOfRobinGame.SpriteBatch.Draw(texture: mapTexture, sourceRectangle: sourceRect, destinationRectangle: cell.rect, color: Color.White);
                        }
                    }
                }

                float lowResOpacity = 1f - (float)Helpers.ConvertRange(oldMin: showDetailedMapZoom, oldMax: showDetailedMapZoom + 0.02f, newMin: 0, newMax: 1, oldVal: this.camera.CurrentZoom, clampToEdges: true);
                if (lowResOpacity > 0) SonOfRobinGame.SpriteBatch.Draw(this.lowResGround, worldRect, Color.White * lowResOpacity);
            }
            else // do not show detailed map
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

            foreach (Sprite sprite in this.bgTaskSpritesToShow.ToArray())
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

            // drawing named locations

            if (Preferences.mapShowLocationNames && this.Mode == MapMode.Full && this.camera.CurrentZoom >= 0.04f)
            {
                var drawnNamesRects = new List<Rectangle>();

                float locationTextScale = Math.Min(1f / this.camera.CurrentZoom * 0.25f, 2f) * 6f;
                int outlineSize = (int)Math.Ceiling(locationTextScale * 2f);

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

                        Vector2 textSize = Helpers.MeasureStringCorrectly(font: locationFont, stringToMeasure: location.name);

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

                        Helpers.DrawTextInsideRect(font: locationFont, text: location.name, rectangle: newTextRect, color: Color.Black * this.viewParams.drawOpacity, effect: FontSystemEffect.Blurry, effectAmount: outlineSize * 2, drawTestRect: false, drawTimes: 3);

                        HslColor locationColorHSL = location.Color.ToHsl();
                        locationColorHSL = new HslColor(h: locationColorHSL.H, s: locationColorHSL.S, l: Math.Max(locationColorHSL.L * 1.5f, 0.85f));

                        Helpers.DrawTextInsideRect(font: locationFont, text: location.name, rectangle: newTextRect, color: locationColorHSL.ToRgb() * this.viewParams.drawOpacity, effect: FontSystemEffect.Stroked, effectAmount: outlineSize, drawTestRect: false);

                        drawnNamesRects.Add(newTextRect);
                    }
                }
            }

            // drawing map edges over everything

            SonOfRobinGame.SpriteBatch.Draw(TextureBank.GetTexture(TextureBank.TextureName.MapEdges), extendedMapRect, Color.White);

            // drawing map markers

            if (this.Mode == MapMode.Full)
            {
                int markerSizePixels = (int)(Preferences.MapMarkerRealSize * this.viewParams.ScaleY / Preferences.GlobalScale);
                Rectangle markerRect = new Rectangle(x: 0, y: 0, width: markerSizePixels, height: markerSizePixels);

                foreach (var kvp in this.world.map.mapMarkerByColor)
                {
                    Color markerColor = kvp.Key;
                    BoardPiece markerPiece = kvp.Value;

                    if (markerPiece != null && markerPiece.exists)
                    {
                        markerRect.X = (int)markerPiece.sprite.position.X - (markerRect.Width / 2);
                        markerRect.Y = (int)markerPiece.sprite.position.Y - (markerRect.Height / 2);

                        markerPiece.sprite.effectCol.AddEffect(new ColorizeInstance(color: markerColor, priority: 0));
                        markerPiece.sprite.effectCol.TurnOnNextEffect(scene: this, currentUpdateToUse: this.world.CurrentUpdate);

                        markerPiece.sprite.AnimFrame.DrawAndKeepInRectBounds(destBoundsRect: markerRect, color: Color.White);

                        SonOfRobinGame.SpriteBatch.End();
                        SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
                    }
                }
            }

            // drawing crosshair

            if (this.Mode == MapMode.Full)
            {
                int crossHairSize = (int)(viewRect.Width * 0.02f);
                int crosshairHalfSize = crossHairSize / 2;

                Rectangle crosshairRect = new(x: (int)this.camera.CurrentPos.X - crosshairHalfSize, y: (int)this.camera.CurrentPos.Y - crosshairHalfSize, width: crossHairSize, height: crossHairSize);
                AnimFrame crosshairFrame = PieceInfo.GetInfo(PieceTemplate.Name.Crosshair).frame;
                crosshairFrame.DrawAndKeepInRectBounds(destBoundsRect: crosshairRect, color: Color.White);
            }

            // drawing camera rect (debug)
            if (Preferences.debugShowOutsideCamera) Helpers.DrawRectangleOutline(rect: this.camera.viewRect, color: Color.White, borderWidth: 3);

            SonOfRobinGame.SpriteBatch.End();
        }

        public override void Draw()
        {
            // is being drawn in MapOverlay scene
        }

        private void KeepBackgroundTasksAlive()
        {
            if (this.bgTaskForMeshes != null && this.bgTaskForMeshes.IsFaulted)
            {
                if (SonOfRobinGame.platform != Platform.Mobile) SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.bgTaskForMeshes.Exception, showTextWindow: false);
                SonOfRobinGame.MessageLog.Add(debugMessage: true, text: "An error occured while processing background task (meshes). Restarting task.", textColor: Color.Orange);
            }
            if (this.bgTaskForMeshes == null || this.bgTaskForMeshes.IsFaulted) this.bgTaskForMeshes = Task.Run(() => this.BGMeshesTaskLoop());

            if (this.bgTaskForSprites != null && this.bgTaskForSprites.IsFaulted)
            {
                if (SonOfRobinGame.platform != Platform.Mobile) SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.bgTaskForSprites.Exception, showTextWindow: false);
                SonOfRobinGame.MessageLog.Add(debugMessage: true, text: "An error occured while processing background task (pieces). Restarting task.", textColor: Color.Orange);
            }
            if (this.bgTaskForSprites == null || this.bgTaskForSprites.IsFaulted) this.bgTaskForSprites = Task.Run(() => this.BGSpritesTaskLoop());
        }

        private void BGMeshesTaskLoop()
        {
            var meshesToShow = new List<Mesh>();
            Rectangle meshSearchRect;

            while (true)
            {
                if (this.HasBeenRemoved) return;

                // ShowDetailedMap is not checked, to allow for early meshes preparation in case of a rapid zoom
                if (this.Mode == MapMode.Off || this.bgTaskMeshesLastCameraRect == this.camera.viewRect)
                {
                    Thread.Sleep(1); // to avoid high CPU usage
                    continue;
                }

                meshSearchRect = this.camera.viewRect;
                this.bgTaskMeshesLastCameraRect = meshSearchRect;

                meshSearchRect.Inflate(meshSearchRect.Width / 8, meshSearchRect.Height / 8); // to allow some overlap for scrolling

                meshesToShow.Clear();
                try
                {
                    meshesToShow.AddRange(this.world.Grid.MeshGrid.GetMeshesForRect(meshSearchRect)
                        .Where(mesh => mesh.boundsRect.Intersects(meshSearchRect))
                        .OrderBy(mesh => mesh.meshDef.drawPriority)
                        .Distinct()
                        .ToList());
                }
                catch (InvalidOperationException) { continue; }

                while (true) // target list should not be replaced when in use
                {
                    if (ProcessingMode != ProcessingModes.RenderToTarget)
                    {
                        this.bgTaskMeshesToShow = new List<Mesh>(meshesToShow); // making list copy, to safely use Clear() on the local list
                        break;
                    }
                    else Thread.Sleep(1); // will freeze without this
                }
            }
        }

        // static variables are down here for easier editing

        private static readonly List<Type> typesShownAlways = new List<Type> { typeof(Player), typeof(Workshop), typeof(Cooker), typeof(Shelter), typeof(AlchemyLab), typeof(Fireplace) };
        private static readonly List<PieceTemplate.Name> namesShownAlways = new List<PieceTemplate.Name> { PieceTemplate.Name.MapMarker, PieceTemplate.Name.FenceHorizontalShort, PieceTemplate.Name.FenceVerticalShort };
        private static readonly List<Type> typesShownIfDiscovered = new List<Type> { typeof(Container) };
        private static readonly List<PieceTemplate.Name> namesShownIfDiscovered = new List<PieceTemplate.Name> { PieceTemplate.Name.CrateStarting, PieceTemplate.Name.CrateRegular, PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig, PieceTemplate.Name.Totem };

        private void BGSpritesTaskLoop()
        {
            var cameraSprites = new List<Sprite>();
            var spritesToShow = new List<Sprite>();

            while (true)
            {
                if (this.HasBeenRemoved) return;

                if (this.Mode == MapMode.Off || this.bgTaskSpritesLastCameraRect == this.camera.viewRect)
                {
                    Thread.Sleep(1); // to avoid high CPU usage
                    continue;
                }

                Rectangle viewRect = this.camera.viewRect;
                this.bgTaskSpritesLastCameraRect = viewRect;
                cameraSprites.Clear();
                spritesToShow.Clear();

                Rectangle worldCameraRectForSpriteSearch = viewRect;

                // mini map displays far pieces on the sides
                if (this.Mode == MapMode.Mini) worldCameraRectForSpriteSearch.Inflate(worldCameraRectForSpriteSearch.Width, worldCameraRectForSpriteSearch.Height);
                else worldCameraRectForSpriteSearch.Inflate(worldCameraRectForSpriteSearch.Width / 8, worldCameraRectForSpriteSearch.Height / 8);

                try
                {
                    cameraSprites.AddRange(this.world.Grid.GetSpritesForRect(groupName: Cell.Group.ColMovement, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, rectangle: worldCameraRectForSpriteSearch, addPadding: false));
                }
                catch (InvalidOperationException) // collection modified while iterating
                { continue; }

                foreach (Sprite sprite in cameraSprites)
                {
                    BoardPiece piece = sprite.boardPiece;
                    PieceTemplate.Name name = piece.name;
                    Type pieceType = piece.GetType();

                    bool showSprite = false;

                    if (typesShownAlways.Contains(pieceType) || namesShownAlways.Contains(name) || Preferences.DebugShowWholeMap) showSprite = true;

                    if (!showSprite && sprite.hasBeenDiscovered &&
                        (namesShownIfDiscovered.Contains(name) ||
                        typesShownIfDiscovered.Contains(pieceType))) showSprite = true;

                    if (showSprite) spritesToShow.Add(sprite);
                }

                spritesToShow = spritesToShow.OrderBy(o => o.AnimFrame.layer).ThenBy(o => o.GfxRect.Bottom).ToList();

                while (true) // target list should not be replaced when in use
                {
                    if (ProcessingMode != ProcessingModes.RenderToTarget)
                    {
                        this.bgTaskSpritesToShow = new List<Sprite>(spritesToShow); // making list copy, to safely use Clear() on the local list
                        break;
                    }
                    else Thread.Sleep(1); // will freeze without this
                }
            }
        }
    }
}