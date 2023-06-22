using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin

{
    public class ControlTips : Scene
    {
        private Vector2 WholeSize
        {
            get
            {
                Vector2 wholeSize = Vector2.Zero;

                foreach (ButtonTip tip in this.tipCollection.Values)
                {
                    wholeSize.X += tip.width + tipMargin;
                    wholeSize.Y = Math.Max(wholeSize.Y, tip.height);
                }

                return wholeSize;
            }
        }

        public enum TipsLayout
        {
            Uninitialized, Empty, Menu, MenuWithoutClosing, Map, InventorySelect, InventoryDrag, PieceContext, TextWindowOk, TextWindowCancel, TextWindowOkCancel, WorldMain, WorldShoot, WorldSleep, WorldBuild, WorldSpectator, QuitLoading
        }

        public const int tipMargin = 12;

        public Dictionary<string, ButtonTip> tipCollection;
        public TipsLayout currentLayout;
        private Scene currentScene;

        public ControlTips() : base(inputType: InputTypes.None, tipsLayout: TipsLayout.Uninitialized, priority: -2, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty)
        {
            this.tipCollection = new Dictionary<string, ButtonTip> { };
            this.SwitchToLayout(tipsLayout: TipsLayout.Empty);
        }

        public static ControlTips GetTopTips()
        {
            var tipsScene = GetTopSceneOfType(typeof(ControlTips));
            if (tipsScene == null)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "No tips scene was found.");
                return null;
            }

            return (ControlTips)tipsScene;
        }

        public static void TipHighlightOnNextFrame(string tipName)
        {
            ControlTips topTips = GetTopTips();
            if (topTips == null) return;

            if (!topTips.tipCollection.ContainsKey(tipName))
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No tip named '{tipName}' was found.");
                return;
            }

            topTips.tipCollection[tipName].highlighter.SetOnForNextRead();
        }

        public override void Update()
        {
            if (this.currentScene == null || Input.currentControlType == Input.ControlType.Touch) return;

            bool bigMode =
                this.currentLayout == TipsLayout.WorldMain ||
                this.currentLayout == TipsLayout.WorldShoot ||
                this.currentLayout == TipsLayout.WorldSpectator ||
                this.currentLayout == TipsLayout.WorldBuild ||
                this.currentLayout == TipsLayout.Map ||
                this.currentLayout == TipsLayout.InventorySelect ||
                this.currentLayout == TipsLayout.InventoryDrag ||
                this.currentLayout == TipsLayout.QuitLoading;

            // To take original scene transition into account:
            // 1. Scene transformations have to be applied manually (because normally it would be invoked after all scenes Update()).
            // 2. Draw parameters have to be used.

            ViewParams sceneViewParams;

            if (this.currentLayout == TipsLayout.WorldSleep)
            {
                // in this case, tips should be aligned with progress bar
                sceneViewParams = SonOfRobinGame.SmallProgressBar.viewParams;
                SonOfRobinGame.SmallProgressBar.transManager.Update();
            }
            else
            {
                sceneViewParams = this.currentScene.viewParams;
                this.currentScene.transManager.Update();
            }

            float scale = 1f / ((float)SonOfRobinGame.VirtualHeight * 0.04f / (float)this.viewParams.Height);

            if (bigMode)
            { if (this.viewParams.Width / scale > SonOfRobinGame.VirtualWidth) scale = 1f / (SonOfRobinGame.VirtualWidth / (float)this.viewParams.Width); }
            else
            { if (this.viewParams.Width / scale > sceneViewParams.Width / sceneViewParams.ScaleX) scale = 1f / (sceneViewParams.Width / sceneViewParams.ScaleX / (float)this.viewParams.Width); }

            this.viewParams.ScaleX = scale;
            this.viewParams.ScaleY = scale;

            if (bigMode)
            {
                this.viewParams.CenterView(horizontally: true, vertically: false);
                this.viewParams.PutViewAtTheBottom();
            }
            else
            {
                this.viewParams.PosX = (sceneViewParams.PosX * scale) + ((sceneViewParams.Width / 2f * scale) - (this.viewParams.Width / 2f));
                this.viewParams.PosY = (sceneViewParams.drawPosY + sceneViewParams.drawHeight) * scale / sceneViewParams.drawScaleY;
                this.viewParams.PosY = Math.Min(this.viewParams.drawPosY, (SonOfRobinGame.VirtualHeight - (this.viewParams.Height / scale)) * scale);
            }
        }

        public override void Draw()
        {
            if (!Preferences.ShowControlTips || Input.currentControlType == Input.ControlType.Touch) return;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            int drawOffsetX = 0;

            foreach (ButtonTip tip in this.tipCollection.Values)
            {
                tip.Draw(controlTips: this, drawOffsetX: drawOffsetX);
                drawOffsetX += tip.width + tipMargin;
            }

            if (Preferences.DebugMode)
            {
                Helpers.DrawTextWithOutline(font: SonOfRobinGame.FontPressStart2P5, text: $"{this.currentLayout}", pos: Vector2.Zero, color: Color.White, outlineColor: Color.Black, outlineSize: 1);
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public static void RefreshTopTipsLayout()
        {
            ControlTips topTips = GetTopTips();
            if (topTips != null) topTips.RefreshLayout();
        }

        public void RefreshLayout()
        {
            this.SwitchToLayout(tipsLayout: currentLayout, force: true);
        }

        public void AssignScene(Scene scene)
        {
            if (this.currentScene == scene && this.tipsLayout == scene?.tipsLayout) return;

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Switching tips layout to '{tipsLayout}' with new scene.", color: Color.LightCyan);

            this.currentScene = scene;
            this.SwitchToLayout(this.currentScene == null ? TipsLayout.Empty : scene.tipsLayout);
        }

        private void SwitchToLayout(TipsLayout tipsLayout, bool force = false)
        {
            if (this.currentLayout == tipsLayout && !force) return;

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Switching layout: '{this.currentLayout}' to '{tipsLayout}'.", color: Color.LightGreen);

            Preferences preferences = new Preferences();
            World world = World.GetTopWorld();
            var scrollTextures = InputMapper.GetTextures(InputMapper.Action.GlobalScrollUp);
            scrollTextures.AddRange(InputMapper.GetTextures(InputMapper.Action.GlobalScrollDown));

            this.tipCollection.Clear();

            switch (tipsLayout)
            {
                case TipsLayout.Empty:
                    break;

                case TipsLayout.Menu:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: InputVis.LeftStickTextureList);
                        if (Input.currentControlType == Input.ControlType.KeyboardAndMouse) new ButtonTip(tipCollection: this.tipCollection, text: "scroll", textures: scrollTextures);
                        new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm));
                        new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        break;
                    }

                case TipsLayout.MenuWithoutClosing:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: InputVis.LeftStickTextureList);
                        if (Input.currentControlType == Input.ControlType.KeyboardAndMouse) new ButtonTip(tipCollection: this.tipCollection, text: "scroll", textures: scrollTextures);
                        new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm));
                        break;
                    }

                case TipsLayout.TextWindowOk:
                    new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm));
                    break;

                case TipsLayout.TextWindowCancel:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "skip", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        break;
                    }

                case TipsLayout.TextWindowOkCancel:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm));
                        new ButtonTip(tipCollection: this.tipCollection, text: "skip", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        break;
                    }

                case TipsLayout.QuitLoading:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "cancel", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        break;
                    }

                case TipsLayout.WorldMain:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "menu", textures: InputMapper.GetTextures(InputMapper.Action.WorldPauseMenu));
                        new ButtonTip(tipCollection: this.tipCollection, text: "walk", textures: InputMapper.GetTextures(InputMapper.Action.WorldWalk));
                        var cameraTextures = InputMapper.GetTextures(InputMapper.Action.WorldCameraMove); // there is no camera mapping for keyboard
                        if (cameraTextures.Count > 0) new ButtonTip(tipCollection: this.tipCollection, text: "camera", textures: cameraTextures);
                        new ButtonTip(tipCollection: this.tipCollection, text: "sprint", isHighlighted: false, textures: InputMapper.GetTextures(InputMapper.Action.WorldSprintToggle));
                        new ButtonTip(tipCollection: this.tipCollection, text: "inventory", textures: InputMapper.GetTextures(InputMapper.Action.WorldInventory));
                        new ButtonTip(tipCollection: this.tipCollection, text: "pick up", isHighlighted: false, textures: InputMapper.GetTextures(InputMapper.Action.WorldPickUp));
                        new ButtonTip(tipCollection: this.tipCollection, text: "craft", textures: InputMapper.GetTextures(InputMapper.Action.WorldFieldCraft));
                        new ButtonTip(tipCollection: this.tipCollection, text: "interact", isHighlighted: false, textures: InputMapper.GetTextures(InputMapper.Action.WorldInteract));
                        new ButtonTip(tipCollection: this.tipCollection, text: "map", highlightCoupledObj: world, highlightCoupledVarName: "MapEnabled", textures: InputMapper.GetTextures(InputMapper.Action.WorldMapToggle));
                        new ButtonTip(tipCollection: this.tipCollection, text: "zoom out", highlightCoupledObj: preferences, highlightCoupledVarName: "CanZoomOut", textures: InputMapper.GetTextures(InputMapper.Action.WorldCameraZoomOut));
                        new ButtonTip(tipCollection: this.tipCollection, text: "use item", isHighlighted: false, textures: InputMapper.GetTextures(InputMapper.Action.WorldUseToolbarPiece));
                        new ButtonTip(tipCollection: this.tipCollection, text: "prev item", textures: InputMapper.GetTextures(InputMapper.Action.ToolbarPrev));
                        new ButtonTip(tipCollection: this.tipCollection, text: "next item", textures: InputMapper.GetTextures(InputMapper.Action.ToolbarNext));
                        break;
                    }

                case TipsLayout.WorldShoot:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "walk", textures: InputMapper.GetTextures(InputMapper.Action.WorldWalk));
                        var cameraTextures = InputMapper.GetTextures(InputMapper.Action.WorldCameraMove); // there is no camera mapping for keyboard
                        if (cameraTextures.Count > 0) new ButtonTip(tipCollection: this.tipCollection, text: "aim", textures: cameraTextures);
                        new ButtonTip(tipCollection: this.tipCollection, text: "zoom out", highlightCoupledObj: preferences, highlightCoupledVarName: "CanZoomOut", textures: InputMapper.GetTextures(InputMapper.Action.WorldCameraZoomOut));
                        new ButtonTip(tipCollection: this.tipCollection, text: "shoot", textures: InputMapper.GetTextures(InputMapper.Action.WorldUseToolbarPiece));
                        break;
                    }

                case TipsLayout.WorldSleep:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "wake up", highlightCoupledObj: world.Player, highlightCoupledVarName: "CanWakeNow", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        break;
                    }

                case TipsLayout.WorldBuild:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "move", textures: InputMapper.GetTextures(InputMapper.Action.WorldWalk));
                        new ButtonTip(tipCollection: this.tipCollection, text: "zoom out", highlightCoupledObj: preferences, highlightCoupledVarName: "CanZoomOut", textures: InputMapper.GetTextures(InputMapper.Action.WorldCameraZoomOut));
                        new ButtonTip(tipCollection: this.tipCollection, text: "place", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm), isHighlighted: false);
                        new ButtonTip(tipCollection: this.tipCollection, text: "cancel", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));

                        break;
                    }

                case TipsLayout.WorldSpectator:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "menu", textures: InputMapper.GetTextures(InputMapper.Action.WorldPauseMenu));
                        new ButtonTip(tipCollection: this.tipCollection, text: "walk", textures: InputMapper.GetTextures(InputMapper.Action.WorldWalk));
                        var cameraTextures = InputMapper.GetTextures(InputMapper.Action.WorldCameraMove); // there is no camera mapping for keyboard
                        if (cameraTextures.Count > 0) new ButtonTip(tipCollection: this.tipCollection, text: "camera", textures: cameraTextures);
                        break;
                    }

                case TipsLayout.Map:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "move", textures: InputMapper.GetTextures(InputMapper.Action.WorldWalk));
                        new ButtonTip(tipCollection: this.tipCollection, text: "zoom in", textures: InputMapper.GetTextures(InputMapper.Action.MapZoomIn));
                        new ButtonTip(tipCollection: this.tipCollection, text: "zoom out", highlightCoupledObj: preferences, highlightCoupledVarName: "CanZoomOut", textures: InputMapper.GetTextures(InputMapper.Action.MapZoomOut));
                        new ButtonTip(tipCollection: this.tipCollection, text: "toggle marker", textures: InputMapper.GetTextures(InputMapper.Action.MapToggleMarker));
                        new ButtonTip(tipCollection: this.tipCollection, text: "center on player", textures: InputMapper.GetTextures(InputMapper.Action.MapCenterPlayer));
                        new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: InputMapper.GetTextures(InputMapper.Action.MapSwitch));
                        break;
                    }

                case TipsLayout.InventorySelect:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: InputVis.LeftStickTextureList);
                        new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        new ButtonTip(tipCollection: this.tipCollection, text: "switch", textures: InputMapper.GetTextures(InputMapper.Action.InvSwitch));
                        new ButtonTip(tipCollection: this.tipCollection, text: "pick stack", textures: InputMapper.GetTextures(InputMapper.Action.InvPickStack));
                        new ButtonTip(tipCollection: this.tipCollection, text: "pick one", textures: InputMapper.GetTextures(InputMapper.Action.InvPickOne));
                        new ButtonTip(tipCollection: this.tipCollection, text: "sort", textures: InputMapper.GetTextures(InputMapper.Action.InvSort));
                        new ButtonTip(tipCollection: this.tipCollection, text: "use", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm));
                        break;
                    }

                case TipsLayout.InventoryDrag:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: InputVis.LeftStickTextureList);
                        new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        new ButtonTip(tipCollection: this.tipCollection, text: "switch", textures: InputMapper.GetTextures(InputMapper.Action.InvSwitch));
                        new ButtonTip(tipCollection: this.tipCollection, text: "release", textures: InputMapper.GetTextures(InputMapper.Action.InvRelease));
                        break;
                    }

                case TipsLayout.PieceContext:
                    {
                        new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: InputVis.LeftStickTextureList);
                        new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: InputMapper.GetTextures(InputMapper.Action.GlobalConfirm));
                        new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: InputMapper.GetTextures(InputMapper.Action.GlobalCancelReturnSkip));
                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported tipsLayout - {tipsLayout}.");
            }

            Vector2 wholeSize = this.WholeSize;
            this.viewParams.Width = (int)wholeSize.X;
            this.viewParams.Height = (int)wholeSize.Y;

            this.currentLayout = tipsLayout;
        }
    }
}