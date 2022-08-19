using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FpsCounter : Scene
    {
        private Rectangle bgRect;
        private Rectangle textRect;
        private static readonly int margin = 2;

        private static readonly Dictionary<int, Color> colorDict = new Dictionary<int, Color> {
            { 40, Color.White },
            { 30, Color.Yellow },
            { 15, Color.Orange },
            { -1, Color.Red },
            };

        private int CounterValue { get { return (int)SonOfRobinGame.fps.Frames; } }
        private string CounterText { get { return $"FPS {this.CounterValue}"; } }

        private Color CounterColor
        {
            get
            {
                int counterValue = this.CounterValue;

                foreach (var kvp in colorDict)
                {
                    if (counterValue > kvp.Key) return kvp.Value;
                }

                throw new ArgumentException($"Cannot calculate FPS counter color for value {counterValue}.");
            }
        }

        public FpsCounter() : base(inputType: InputTypes.None, priority: -2, blocksUpdatesBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.AdaptToNewSize();
            this.transManager.AddTransition(this.GetTransition(inTrans: true));
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding) this.transManager.AddTransition(this.GetTransition(inTrans: false));
            else base.Remove();
        }

        public override void Update(GameTime gameTime)
        { }

        private Transition GetTransition(bool inTrans)
        {
            return new Transition(
                transManager: this.transManager,
                outTrans: !inTrans,
                baseParamName: "PosX",
                targetVal: Preferences.FpsCounterPosRight ? this.viewParams.PosX + this.viewParams.Width : -this.viewParams.Width,
                duration: 12,
                endRemoveScene: !inTrans
                );
        }

        protected override void AdaptToNewSize()
        {
            int width = (int)(SonOfRobinGame.VirtualWidth * 0.03f);
            width = Math.Max(width, 40);
            int height = (int)(width * 0.3);

            this.textRect = new Rectangle(x: 0, y: 0, width: width, height: height);

            this.viewParams.Width = textRect.Width;
            this.viewParams.Height = textRect.Height;

            if (Preferences.FpsCounterPosRight) this.viewParams.PutViewAtTheRight(margin: margin);
            else this.viewParams.PutViewAtTheLeft(margin: margin);

            this.viewParams.PutViewAtTheTop(margin: margin);

            this.bgRect = this.textRect;
            bgRect.Inflate(2, 2);
        }

        public override void Draw()
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, this.bgRect, Color.Black * 0.6f * this.viewParams.drawOpacity);

            Helpers.DrawTextInsideRect(font: SonOfRobinGame.fontFreeSansBold24, text: this.CounterText, rectangle: this.textRect, color: this.CounterColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
        }

    }
}
