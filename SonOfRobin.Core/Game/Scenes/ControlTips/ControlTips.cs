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

                foreach (ButtonTip tip in this.tipList)
                {
                    wholeSize.X += tip.width + tipMargin;
                    wholeSize.Y = Math.Max(wholeSize.Y, tip.height);
                }

                return wholeSize;
            }
        }

        public enum TipsLayout
        {
            Empty, Menu, MenuWithoutClosing, Map, InventorySelect, InventoryDrag, PieceContext, TextWindow, WorldMain, WorldShoot, WorldSleep,
        }
        public static readonly int tipMargin = 12;

        public TipsLayout currentLayout;
        public List<ButtonTip> tipList;
        public Scene currentScene;

        public ControlTips() : base(inputType: InputTypes.None, tipsLayout: TipsLayout.Empty, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty)
        {
            this.SwitchToLayout(tipsLayout: TipsLayout.Empty);
        }

        public override void Update(GameTime gameTime)
        {
            bool worldMode = this.currentLayout == TipsLayout.WorldMain || this.currentLayout == TipsLayout.WorldShoot || this.currentLayout == TipsLayout.WorldSleep;
            ViewParams sceneViewParams = this.currentScene.viewParams;

            float scale = 1f / ((float)SonOfRobinGame.VirtualHeight * 0.04f / (float)this.viewParams.height);
            //scale = 1f; // testing

            if (worldMode)
            { if (this.viewParams.width / scale > SonOfRobinGame.VirtualWidth) scale = 1f / (SonOfRobinGame.VirtualWidth / (float)this.viewParams.width); }
            else
            { if (this.viewParams.width / scale > sceneViewParams.width / sceneViewParams.scaleX) scale = 1f / (sceneViewParams.width / sceneViewParams.scaleX / (float)this.viewParams.width); }

            this.viewParams.scaleX = scale;
            this.viewParams.scaleY = scale;

            if (worldMode)
            {
                this.viewParams.CenterView(horizontally: true, vertically: false);
                this.viewParams.PutViewAtTheBottom();
            }
            else
            {
                this.viewParams.posX = (sceneViewParams.posX * scale) + ((sceneViewParams.width / 2f * scale) - (this.viewParams.width / 2f));
                this.viewParams.posY = (sceneViewParams.posY + sceneViewParams.height) * scale;
                this.viewParams.posY = Math.Min(this.viewParams.posY, (SonOfRobinGame.VirtualHeight - (this.viewParams.height / scale)) * scale);
            }
        }

        public override void Draw()
        {
            if (!Preferences.showControlTips) return;

            int drawOffsetX = 0;

            foreach (ButtonTip tip in this.tipList)
            {
                tip.Draw(controlTips: this, drawOffsetX: drawOffsetX);
                drawOffsetX += tip.width + tipMargin;
            }

            if (Preferences.DebugMode) SonOfRobinGame.spriteBatch.DrawString(SonOfRobinGame.fontSmall, $"{this.currentLayout}", Vector2.Zero, Color.White);
        }

        public static ControlTips GetTopTips()
        {
            ControlTips controlTips;
            var tipsScene = GetTopSceneOfType(typeof(ControlTips));
            if (tipsScene == null) return null;
            controlTips = (ControlTips)tipsScene;
            return controlTips;
        }

        public void SwitchToLayout(TipsLayout tipsLayout)
        {
            if (this.currentLayout == tipsLayout) return;

            this.tipList = new List<ButtonTip> { };

            switch (tipsLayout)
            {
                case TipsLayout.Empty:
                    break;

                case TipsLayout.Map:
                    this.tipList.Add(new ButtonTip(text: "return", textureNames: new List<string> { "Xbox 360/360_B", "Xbox 360/360_Dpad_Right" }));
                    break;

                case TipsLayout.InventorySelect:
                    this.tipList.Add(new ButtonTip(text: "navigation", textureNames: new List<string> { "Xbox 360/360_Dpad", "Xbox 360/360_Left_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "switch", textureNames: new List<string> { "Xbox 360/360_LB", "Xbox 360/360_RB" }));
                    this.tipList.Add(new ButtonTip(text: "pick one", textureName: "Xbox 360/360_X"));
                    this.tipList.Add(new ButtonTip(text: "pick stack", textureName: "Xbox 360/360_Y"));
                    this.tipList.Add(new ButtonTip(text: "use", textureName: "Xbox 360/360_A"));
                    this.tipList.Add(new ButtonTip(text: "return", textureName: "Xbox 360/360_B"));
                    break;

                case TipsLayout.InventoryDrag:
                    this.tipList.Add(new ButtonTip(text: "navigation", textureNames: new List<string> { "Xbox 360/360_Dpad", "Xbox 360/360_Left_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "switch", textureNames: new List<string> { "Xbox 360/360_LB", "Xbox 360/360_RB" }));
                    this.tipList.Add(new ButtonTip(text: "release", textureNames: new List<string> { "Xbox 360/360_X", "Xbox 360/360_Y" }));
                    this.tipList.Add(new ButtonTip(text: "return", textureName: "Xbox 360/360_B"));

                    break;

                case TipsLayout.PieceContext:
                    this.tipList.Add(new ButtonTip(text: "navigation", textureNames: new List<string> { "Xbox 360/360_Dpad", "Xbox 360/360_Left_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "confirm", textureName: "Xbox 360/360_A"));
                    this.tipList.Add(new ButtonTip(text: "return", textureNames: new List<string> { "Xbox 360/360_B", "Xbox 360/360_Start" }));
                    break;

                case TipsLayout.TextWindow:
                    this.tipList.Add(new ButtonTip(text: "confirm", textureNames: new List<string> { "Xbox 360/360_A", "Xbox 360/360_B", "Xbox 360/360_X", "Xbox 360/360_Y" }));
                    this.tipList.Add(new ButtonTip(text: "menu", textureName: "Xbox 360/360_Start"));
                    break;

                case TipsLayout.WorldMain:
                    this.tipList.Add(new ButtonTip(text: "walk", textureName: "Xbox 360/360_Left_Stick"));
                    this.tipList.Add(new ButtonTip(text: "camera", textureName: "Xbox 360/360_Right_Stick"));
                    this.tipList.Add(new ButtonTip(text: "interact", textureName: "Xbox 360/360_A"));
                    this.tipList.Add(new ButtonTip(text: "aim", textureNames: new List<string> { "Xbox 360/360_LT", "Xbox 360/360_Right_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "use tool", textureName: "Xbox 360/360_RT"));
                    this.tipList.Add(new ButtonTip(text: "run", textureName: "Xbox 360/360_B"));
                    this.tipList.Add(new ButtonTip(text: "pick up", textureName: "Xbox 360/360_X"));
                    this.tipList.Add(new ButtonTip(text: "inventory", textureName: "Xbox 360/360_Y"));
                    this.tipList.Add(new ButtonTip(text: "craft", textureName: "Xbox 360/360_Dpad_Up"));
                    this.tipList.Add(new ButtonTip(text: "map", textureName: "Xbox 360/360_Dpad_Right"));
                    this.tipList.Add(new ButtonTip(text: "prev item", textureName: "Xbox 360/360_LB"));
                    this.tipList.Add(new ButtonTip(text: "next item", textureName: "Xbox 360/360_RB"));
                    this.tipList.Add(new ButtonTip(text: "menu", textureName: "Xbox 360/360_Start"));
                    break;

                case TipsLayout.WorldShoot:
                    this.tipList.Add(new ButtonTip(text: "walk", textureName: "Xbox 360/360_Left_Stick"));
                    this.tipList.Add(new ButtonTip(text: "aim", textureNames: new List<string> { "Xbox 360/360_LT", "Xbox 360/360_Right_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "shoot", textureName: "Xbox 360/360_RT"));
                    this.tipList.Add(new ButtonTip(text: "menu", textureName: "Xbox 360/360_Start"));
                    break;

                case TipsLayout.WorldSleep:
                    this.tipList.Add(new ButtonTip(text: "wake up", textureNames: new List<string> { "Xbox 360/360_A", "Xbox 360/360_B", "Xbox 360/360_X", "Xbox 360/360_Y" }));
                    this.tipList.Add(new ButtonTip(text: "menu", textureName: "Xbox 360/360_Start"));
                    break;

                case TipsLayout.Menu:
                    this.tipList.Add(new ButtonTip(text: "navigation", textureNames: new List<string> { "Xbox 360/360_Dpad", "Xbox 360/360_Left_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "confirm", textureName: "Xbox 360/360_A"));
                    this.tipList.Add(new ButtonTip(text: "return", textureNames: new List<string> { "Xbox 360/360_B", "Xbox 360/360_Start" }));
                    break;

                case TipsLayout.MenuWithoutClosing:
                    this.tipList.Add(new ButtonTip(text: "navigation", textureNames: new List<string> { "Xbox 360/360_Dpad", "Xbox 360/360_Left_Stick" }));
                    this.tipList.Add(new ButtonTip(text: "confirm", textureName: "Xbox 360/360_A"));
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported tipsLayout - {tipsLayout}.");
            }

            Vector2 wholeSize = this.WholeSize;
            this.viewParams.width = (int)wholeSize.X;
            this.viewParams.height = (int)wholeSize.Y;

            this.currentLayout = tipsLayout;
        }

    }
}
