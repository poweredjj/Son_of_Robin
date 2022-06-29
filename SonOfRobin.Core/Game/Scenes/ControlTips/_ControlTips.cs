using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            Uninitialized, Empty, Menu, MenuWithoutClosing, Map, InventorySelect, InventoryDrag, PieceContext, TextWindowOk, TextWindowCancel, TextWindowOkCancel, WorldMain, WorldShoot, WorldSleep, WorldSpectator, QuitLoading
        }
        public static readonly int tipMargin = 12;

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
            ControlTips tips;
            var tipsScene = GetTopSceneOfType(typeof(ControlTips));
            if (tipsScene == null)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "No tips scene was found.");
                return null;
            }

            tips = (ControlTips)tipsScene;
            return tips;
        }

        public static void TipHighlightOnNextFrame(string tipName)
        {
            ControlTips topTips = GetTopTips();
            if (topTips == null) return;

            if (!topTips.tipCollection.ContainsKey(tipName))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No tip named '{tipName}' was found.");
                return;
            }

            topTips.tipCollection[tipName].highlighter.SetOnForNextRead();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.currentScene == null) return;

            bool bigMode =
                this.currentLayout == TipsLayout.WorldMain ||
                this.currentLayout == TipsLayout.WorldShoot ||
                this.currentLayout == TipsLayout.WorldSpectator ||
                this.currentLayout == TipsLayout.InventorySelect ||
                this.currentLayout == TipsLayout.InventoryDrag;

            // To take original scene transition into account:
            // 1. Scene transformations have to be applied manually (because normally it would be invoked after all scenes Update().
            // 2. Draw parameters have to be used.

            ViewParams sceneViewParams;

            if (this.currentLayout == TipsLayout.QuitLoading || this.currentLayout == TipsLayout.WorldSleep)
            {
                // in this case, tips should be aligned with progress bar
                sceneViewParams = SonOfRobinGame.progressBar.viewParams;
                SonOfRobinGame.progressBar.transManager.Update();
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
            if (!Preferences.showControlTips) return;

            int drawOffsetX = 0;

            foreach (ButtonTip tip in this.tipCollection.Values)
            {
                tip.Draw(controlTips: this, drawOffsetX: drawOffsetX);
                drawOffsetX += tip.width + tipMargin;
            }

            if (Preferences.DebugMode) SonOfRobinGame.spriteBatch.DrawString(SonOfRobinGame.fontPressStart2P5, $"{this.currentLayout}", Vector2.Zero, Color.White);
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

            World world = World.GetTopWorld();
            this.tipCollection.Clear();

            switch (tipsLayout)
            {
                case TipsLayout.Empty:
                    break;

                case TipsLayout.Map:
                    new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB, ButtonScheme.dpadRight });
                    break;

                case TipsLayout.InventorySelect:
                    new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "switch", textures: new List<Texture2D> { ButtonScheme.buttonLB, ButtonScheme.buttonRB });
                    new ButtonTip(tipCollection: this.tipCollection, text: "pick stack", textures: new List<Texture2D> { ButtonScheme.buttonX });
                    new ButtonTip(tipCollection: this.tipCollection, text: "pick one", textures: new List<Texture2D> { ButtonScheme.buttonY });
                    new ButtonTip(tipCollection: this.tipCollection, text: "use", textures: new List<Texture2D> { ButtonScheme.buttonA });
                    new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB });
                    break;

                case TipsLayout.InventoryDrag:
                    new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "switch", textures: new List<Texture2D> { ButtonScheme.buttonLB, ButtonScheme.buttonRB });
                    new ButtonTip(tipCollection: this.tipCollection, text: "release", textures: new List<Texture2D> { ButtonScheme.buttonX, ButtonScheme.buttonY, ButtonScheme.buttonA });
                    new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB });

                    break;

                case TipsLayout.PieceContext:
                    new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA });
                    new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB });
                    break;

                case TipsLayout.TextWindowOk:
                    new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA });
                    break;

                case TipsLayout.TextWindowCancel:
                    new ButtonTip(tipCollection: this.tipCollection, text: "skip", textures: new List<Texture2D> { ButtonScheme.buttonB });
                    break;

                case TipsLayout.TextWindowOkCancel:
                    new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA });
                    new ButtonTip(tipCollection: this.tipCollection, text: "skip", textures: new List<Texture2D> { ButtonScheme.buttonB });
                    break;

                case TipsLayout.WorldMain:
                    new ButtonTip(tipCollection: this.tipCollection, text: "walk", textures: new List<Texture2D> { ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "camera", textures: new List<Texture2D> { ButtonScheme.rightStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "interact", isHighlighted: false, textures: new List<Texture2D> { ButtonScheme.buttonA });
                    new ButtonTip(tipCollection: this.tipCollection, text: "aim", isHighlighted: false, textures: new List<Texture2D> { ButtonScheme.buttonLT });
                    new ButtonTip(tipCollection: this.tipCollection, text: "use item", isHighlighted: false, textures: new List<Texture2D> { ButtonScheme.buttonRT });
                    new ButtonTip(tipCollection: this.tipCollection, text: "run", textures: new List<Texture2D> { ButtonScheme.buttonB });
                    new ButtonTip(tipCollection: this.tipCollection, text: "pick up", isHighlighted: false, textures: new List<Texture2D> { ButtonScheme.buttonX });
                    new ButtonTip(tipCollection: this.tipCollection, text: "inventory", textures: new List<Texture2D> { ButtonScheme.buttonY });
                    new ButtonTip(tipCollection: this.tipCollection, text: "equip", textures: new List<Texture2D> { ButtonScheme.dpadLeft });
                    new ButtonTip(tipCollection: this.tipCollection, text: "craft", textures: new List<Texture2D> { ButtonScheme.dpadUp });
                    new ButtonTip(tipCollection: this.tipCollection, text: "map", highlightCoupledObj: world, highlightCoupledVarName: "MapEnabled", textures: new List<Texture2D> { ButtonScheme.dpadRight });
                    new ButtonTip(tipCollection: this.tipCollection, text: "prev item", textures: new List<Texture2D> { ButtonScheme.buttonLB });
                    new ButtonTip(tipCollection: this.tipCollection, text: "next item", textures: new List<Texture2D> { ButtonScheme.buttonRB });
                    new ButtonTip(tipCollection: this.tipCollection, text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart });
                    break;

                case TipsLayout.WorldShoot:
                    new ButtonTip(tipCollection: this.tipCollection, text: "walk", textures: new List<Texture2D> { ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "aim", textures: new List<Texture2D> { ButtonScheme.buttonLT, ButtonScheme.plus, ButtonScheme.rightStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "shoot", textures: new List<Texture2D> { ButtonScheme.buttonRT });
                    new ButtonTip(tipCollection: this.tipCollection, text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart });
                    break;

                case TipsLayout.WorldSleep:
                    new ButtonTip(tipCollection: this.tipCollection, text: "wake up", highlightCoupledObj: world.player, highlightCoupledVarName: "CanWakeNow", textures: new List<Texture2D> { ButtonScheme.buttonA, ButtonScheme.buttonB, ButtonScheme.buttonX, ButtonScheme.buttonY });
                    new ButtonTip(tipCollection: this.tipCollection, text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart });
                    break;

                case TipsLayout.WorldSpectator:
                    new ButtonTip(tipCollection: this.tipCollection, text: "walk", textures: new List<Texture2D> { ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "camera", textures: new List<Texture2D> { ButtonScheme.rightStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart });
                    break;

                case TipsLayout.Menu:
                    new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA });
                    new ButtonTip(tipCollection: this.tipCollection, text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB, ButtonScheme.buttonStart });
                    break;

                case TipsLayout.MenuWithoutClosing:
                    new ButtonTip(tipCollection: this.tipCollection, text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick });
                    new ButtonTip(tipCollection: this.tipCollection, text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA });
                    break;

                case TipsLayout.QuitLoading:
                    new ButtonTip(tipCollection: this.tipCollection, text: "cancel", textures: new List<Texture2D> { ButtonScheme.buttonB });
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported tipsLayout - {tipsLayout}.");
            }

            Vector2 wholeSize = this.WholeSize;
            this.viewParams.Width = (int)wholeSize.X;
            this.viewParams.Height = (int)wholeSize.Y;

            this.currentLayout = tipsLayout;
        }

    }
}
