﻿using Microsoft.Xna.Framework;
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
        private RenderTarget2D miniatureTerrainGfx;
        private RenderTarget2D miniatureCombinedGfx;
        private Vector2 lastTouchPos;

        private float InitialZoom { get { return Preferences.WorldScale / 2; } }

        private static readonly List<PieceTemplate.Name> depositNameList = new List<PieceTemplate.Name> {
            PieceTemplate.Name.CoalDeposit, PieceTemplate.Name.IronDeposit, PieceTemplate.Name.CrystalDepositSmall, PieceTemplate.Name.CrystalDepositBig };

        private static readonly Color fogColor = new Color(50, 50, 50);

        public Map(World world, TouchLayout touchLayout) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, touchLayout: touchLayout, tipsLayout: ControlTips.TipsLayout.Map)
        {
            this.drawActive = false;
            this.updateActive = false;
            this.world = world;
            this.worldRect = new Rectangle(x: 0, y: 0, width: world.width, height: world.height);
            this.camera = new Camera(world: this.world, displayScene: this, useFluidMotion: false);
            this.Mode = MapMode.Off;
            this.dirtyFog = true;
            this.lastTouchPos = Vector2.Zero;
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

            this.camera.TrackCoords(position: this.world.player.sprite.position, moveInstantly: true);
            this.camera.SetZoom(zoom: this.InitialZoom, setInstantly: true);

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

            if (this.miniatureTerrainGfx == null || this.miniatureTerrainGfx.Width != this.viewParams.Width || this.miniatureTerrainGfx.Height != this.viewParams.Height)
            {
                if (this.miniatureTerrainGfx != null) this.miniatureTerrainGfx.Dispose();
                this.miniatureTerrainGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
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

                this.StartRenderingToTarget(this.miniatureTerrainGfx);
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
            if (this.miniatureCombinedGfx != null && this.dirtyFog == false) return;

            this.SetViewParamsForMiniature();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} updating map fog (fullscreen {this.FullScreen})");

            if (this.miniatureCombinedGfx == null || this.miniatureCombinedGfx.Width != this.viewParams.Width || this.miniatureCombinedGfx.Height != this.viewParams.Height)
            {
                if (this.miniatureCombinedGfx != null) this.miniatureCombinedGfx.Dispose();
                this.miniatureCombinedGfx = new RenderTarget2D(SonOfRobinGame.graphicsDevice, this.viewParams.Width, this.viewParams.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }

            this.StartRenderingToTarget(this.miniatureCombinedGfx);
            SonOfRobinGame.graphicsDevice.Clear(Color.Transparent);
            SonOfRobinGame.spriteBatch.Draw(this.miniatureTerrainGfx, this.miniatureTerrainGfx.Bounds, Color.White);
            Rectangle destinationRectangle;

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
            this.camera.Update();
            this.UpdateViewPos();

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
            if (InputMapper.HasBeenPressed(InputMapper.Action.MapSwitch))
            {
                this.SwitchToNextMode();
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
                currentZoom = Math.Max(currentZoom, this.scaleMultiplier);
                this.camera.SetZoom(currentZoom);
            }

            var touches = TouchInput.TouchPanelState;
            if (!touches.Any()) this.lastTouchPos = Vector2.Zero;

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
                Rectangle cameraRect = this.camera.viewRect;
                int cameraHalfWidth = cameraRect.Width / 2;
                int cameraHalfHeight = cameraRect.Height / 2;

                Vector2 newPos = this.camera.TrackedPos + movement;

                newPos.X = Math.Max(newPos.X, cameraHalfWidth);
                newPos.X = Math.Min(newPos.X, this.world.width - cameraHalfWidth);
                newPos.Y = Math.Max(newPos.Y, cameraHalfHeight);
                newPos.Y = Math.Min(newPos.Y, this.world.height - cameraHalfHeight);

                this.camera.TrackCoords(newPos);
            }

            this.camera.Update();

        }
        private Vector2 DrawOffset
        {
            get
            {
                Vector2 drawOffset = Vector2.Zero;

                int displayedMapWidth = (int)(this.world.width * this.camera.currentZoom);
                int displayedMapHeight = (int)(this.world.width * this.camera.currentZoom);

                if (displayedMapWidth < SonOfRobinGame.VirtualWidth) drawOffset.X += (SonOfRobinGame.VirtualWidth - displayedMapWidth) / 2 / this.camera.currentZoom;
                if (displayedMapHeight < SonOfRobinGame.VirtualHeight) drawOffset.Y += (SonOfRobinGame.VirtualHeight - displayedMapHeight) / 2 / this.camera.currentZoom;

                return drawOffset;
            }
        }

        public override void Draw()
        {
            // filling screen with water color

            if (this.FullScreen && !this.transManager.HasAnyTransition) SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            // calculating centered world rectangle

            Rectangle worldRectCentered = this.worldRect;
            Vector2 drawOffset = this.DrawOffset;
            int drawOffsetX = (int)drawOffset.X;
            int drawOffsetY = (int)drawOffset.Y;
            worldRectCentered.X += (int)drawOffset.X;
            worldRectCentered.Y += (int)drawOffset.Y;

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
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, worldRectCentered, fogColor * this.viewParams.drawOpacity);

                foreach (Cell cell in visibleCells)
                {
                    if (cell.VisitedByPlayer || Preferences.DebugShowWholeMap) cell.DrawBackground(drawOffsetX: drawOffsetX, drawOffsetY: drawOffsetY, opacity: this.viewParams.drawOpacity);
                }
            }

            // drawing miniature background

            if (miniatureOpacity > 0) SonOfRobinGame.spriteBatch.Draw(this.miniatureCombinedGfx, worldRectCentered, Color.White * this.viewParams.drawOpacity * miniatureOpacity);

            // drawing pieces

            var groupName = this.FullScreen ? Cell.Group.Visible : Cell.Group.MiniMap;

            Rectangle worldCameraRect = this.world.camera.viewRect;

            var spritesBag = world.grid.GetSprites(groupName: groupName, visitedByPlayerOnly: !Preferences.DebugShowWholeMap, camera: this.camera);

            var typesShownOutsideCamera = new List<Type> { typeof(Player), typeof(Workshop), typeof(Cooker), typeof(Shelter), };
            var typesShownIfDiscovered = new List<Type> { typeof(Container), typeof(Tool), typeof(Equipment), typeof(Fruit), };
            var typesShownInCamera = new List<Type> { typeof(Animal), typeof(Plant), typeof(Collectible) };

            var namesShownIfDiscovered = new List<PieceTemplate.Name> { PieceTemplate.Name.CrateStarting, PieceTemplate.Name.CrateRegular };
            namesShownIfDiscovered.AddRange(depositNameList);

            // regular "foreach", because spriteBatch is not thread-safe
            foreach (Sprite sprite in spritesBag.OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom))
            {
                BoardPiece piece = sprite.boardPiece;
                Type pieceType = piece.GetType();

                bool showSprite = false;

                if (typesShownOutsideCamera.Contains(pieceType)) showSprite = true;

                if (!showSprite && sprite.hasBeenDiscovered &&
                    (namesShownIfDiscovered.Contains(sprite.boardPiece.name) ||
                    typesShownIfDiscovered.Contains(pieceType))) showSprite = true;

                if (!showSprite && typesShownInCamera.Contains(pieceType) && sprite.IsInCameraRect) showSprite = true;


                if (showSprite) sprite.DrawRoutine(calculateSubmerge: false, offsetX: drawOffsetX, offsetY: drawOffsetY);
            }

            // drawing camera FOV

            Rectangle worldCameraRectCentered = this.world.camera.viewRect;
            worldCameraRectCentered.X += drawOffsetX;
            worldCameraRectCentered.Y += drawOffsetY;

            Helpers.DrawRectangleOutline(rect: worldCameraRectCentered, color: Color.White * this.viewParams.drawOpacity, borderWidth: this.FullScreen ? 4 : 2);
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
