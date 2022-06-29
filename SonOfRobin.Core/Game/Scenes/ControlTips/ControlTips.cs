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
            Uninitialized, Empty, Menu, MenuWithoutClosing, Map, InventorySelect, InventoryDrag, PieceContext, TextWindow, WorldMain, WorldShoot, WorldSleep, QuitLoading
        }
        public static readonly int tipMargin = 12;

        public TipsLayout currentLayout;
        public List<ButtonTip> tipList;
        public Scene currentScene;

        public ControlTips() : base(inputType: InputTypes.None, tipsLayout: TipsLayout.Uninitialized, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty)
        {
            this.SwitchToLayout(tipsLayout: TipsLayout.Empty);
        }

        public override void Update(GameTime gameTime)
        {
            bool bigMode = this.currentLayout == TipsLayout.WorldMain || this.currentLayout == TipsLayout.WorldShoot || this.currentLayout == TipsLayout.WorldSleep || this.currentLayout == TipsLayout.InventorySelect || this.currentLayout == TipsLayout.InventoryDrag;
            ViewParams sceneViewParams = this.currentScene.viewParams;

            float scale = 1f / ((float)SonOfRobinGame.VirtualHeight * 0.04f / (float)this.viewParams.height);
            //scale = 1f; // testing

            if (bigMode)
            { if (this.viewParams.width / scale > SonOfRobinGame.VirtualWidth) scale = 1f / (SonOfRobinGame.VirtualWidth / (float)this.viewParams.width); }
            else
            { if (this.viewParams.width / scale > sceneViewParams.width / sceneViewParams.scaleX) scale = 1f / (sceneViewParams.width / sceneViewParams.scaleX / (float)this.viewParams.width); }

            this.viewParams.scaleX = scale;
            this.viewParams.scaleY = scale;

            if (bigMode)
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

        public void RefreshLayout()
        {
            TipsLayout currentLayout = this.currentLayout;
            this.SwitchToLayout(TipsLayout.Empty);
            this.SwitchToLayout(currentLayout);
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
                    this.tipList.Add(new ButtonTip(text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB, ButtonScheme.dpadRight }));
                    break;

                case TipsLayout.InventorySelect:
                    this.tipList.Add(new ButtonTip(text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "switch", textures: new List<Texture2D> { ButtonScheme.buttonLB, ButtonScheme.buttonRB }));
                    this.tipList.Add(new ButtonTip(text: "pick stack", textures: new List<Texture2D> { ButtonScheme.buttonX }));
                    this.tipList.Add(new ButtonTip(text: "pick one", textures: new List<Texture2D> { ButtonScheme.buttonY }));
                    this.tipList.Add(new ButtonTip(text: "use", textures: new List<Texture2D> { ButtonScheme.buttonA }));
                    this.tipList.Add(new ButtonTip(text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB }));
                    break;

                case TipsLayout.InventoryDrag:
                    this.tipList.Add(new ButtonTip(text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "switch", textures: new List<Texture2D> { ButtonScheme.buttonLB, ButtonScheme.buttonRB }));
                    this.tipList.Add(new ButtonTip(text: "release", textures: new List<Texture2D> { ButtonScheme.buttonX, ButtonScheme.buttonY, ButtonScheme.buttonA }));
                    this.tipList.Add(new ButtonTip(text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB }));

                    break;

                case TipsLayout.PieceContext:
                    this.tipList.Add(new ButtonTip(text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA }));
                    this.tipList.Add(new ButtonTip(text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB }));
                    break;

                case TipsLayout.TextWindow:
                    this.tipList.Add(new ButtonTip(text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA, ButtonScheme.buttonB, ButtonScheme.buttonX, ButtonScheme.buttonY }));
                    break;

                case TipsLayout.WorldMain:
                    this.tipList.Add(new ButtonTip(text: "walk", textures: new List<Texture2D> { ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "camera", textures: new List<Texture2D> { ButtonScheme.rightStick }));
                    this.tipList.Add(new ButtonTip(text: "interact", textures: new List<Texture2D> { ButtonScheme.buttonA }));
                    this.tipList.Add(new ButtonTip(text: "aim", textures: new List<Texture2D> { ButtonScheme.buttonLT, ButtonScheme.rightStick }));
                    this.tipList.Add(new ButtonTip(text: "use tool", textures: new List<Texture2D> { ButtonScheme.buttonRT }));
                    this.tipList.Add(new ButtonTip(text: "run", textures: new List<Texture2D> { ButtonScheme.buttonB }));
                    this.tipList.Add(new ButtonTip(text: "pick up", textures: new List<Texture2D> { ButtonScheme.buttonX }));
                    this.tipList.Add(new ButtonTip(text: "inventory", textures: new List<Texture2D> { ButtonScheme.buttonY }));
                    this.tipList.Add(new ButtonTip(text: "equip", textures: new List<Texture2D> { ButtonScheme.dpadLeft }));
                    this.tipList.Add(new ButtonTip(text: "craft", textures: new List<Texture2D> { ButtonScheme.dpadUp }));
                    this.tipList.Add(new ButtonTip(text: "map", textures: new List<Texture2D> { ButtonScheme.dpadRight }));
                    this.tipList.Add(new ButtonTip(text: "prev item", textures: new List<Texture2D> { ButtonScheme.buttonLB }));
                    this.tipList.Add(new ButtonTip(text: "next item", textures: new List<Texture2D> { ButtonScheme.buttonRB }));
                    this.tipList.Add(new ButtonTip(text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart }));
                    break;

                case TipsLayout.WorldShoot:
                    this.tipList.Add(new ButtonTip(text: "walk", textures: new List<Texture2D> { ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "aim", textures: new List<Texture2D> { ButtonScheme.buttonLT, ButtonScheme.rightStick }));
                    this.tipList.Add(new ButtonTip(text: "shoot", textures: new List<Texture2D> { ButtonScheme.buttonRT }));
                    this.tipList.Add(new ButtonTip(text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart }));
                    break;

                case TipsLayout.WorldSleep:
                    this.tipList.Add(new ButtonTip(text: "wake up", textures: new List<Texture2D> { ButtonScheme.buttonA, ButtonScheme.buttonB, ButtonScheme.buttonX, ButtonScheme.buttonY }));
                    this.tipList.Add(new ButtonTip(text: "menu", textures: new List<Texture2D> { ButtonScheme.buttonStart }));
                    break;

                case TipsLayout.Menu:
                    this.tipList.Add(new ButtonTip(text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA }));
                    this.tipList.Add(new ButtonTip(text: "return", textures: new List<Texture2D> { ButtonScheme.buttonB, ButtonScheme.buttonStart }));
                    break;

                case TipsLayout.MenuWithoutClosing:
                    this.tipList.Add(new ButtonTip(text: "navigation", textures: new List<Texture2D> { ButtonScheme.dpad, ButtonScheme.leftStick }));
                    this.tipList.Add(new ButtonTip(text: "confirm", textures: new List<Texture2D> { ButtonScheme.buttonA }));
                    break;

                case TipsLayout.QuitLoading:
                    this.tipList.Add(new ButtonTip(text: "cancel", textures: new List<Texture2D> { ButtonScheme.buttonB }));
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
