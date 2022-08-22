using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FpsCounter : Scene
    {
        private Rectangle bgRect;
        private Rectangle textRect;
        private Rectangle graphRect;
        private static readonly int outerMargin = SonOfRobinGame.platform == Platform.Mobile ? 12 : 5;
        private static readonly int innerMargin = 3;
        private static readonly int historyLength = 70;

        private static readonly Dictionary<int, Color> colorDict = new Dictionary<int, Color> {
            { 40, Color.White },
            { 30, Color.Yellow },
            { 15, Color.Orange },
            { -1, Color.Red },
            };

        private readonly List<double> percentageHistory = new List<double>();

        private int counterValue;
        private string CounterText { get { return $"FPS {this.counterValue}"; } }
        private double CounterPercentage { get { return Math.Min(this.counterValue / 60d, 1d); } }

        private Color CounterColor
        {
            get
            {
                int counterValue = this.counterValue;

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
        {
            this.counterValue = (int)SonOfRobinGame.fps.FPS;
            this.percentageHistory.Add(this.CounterPercentage);
            if (this.percentageHistory.Count > historyLength) this.percentageHistory.RemoveAt(0);
        }

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
            this.graphRect = new Rectangle(x: this.textRect.X, y: this.textRect.Y, width: Preferences.FpsCounterShowGraph ? historyLength : 0, height: this.textRect.Height);
            this.textRect.X += this.graphRect.Width;

            this.bgRect = new Rectangle(x: this.graphRect.X, y: this.graphRect.Y, width: textRect.Width + graphRect.Width, height: this.graphRect.Height);

            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = textRect.Height;

            if (Preferences.FpsCounterPosRight) this.viewParams.PutViewAtTheRight(margin: outerMargin);
            else this.viewParams.PutViewAtTheLeft(margin: outerMargin);

            this.viewParams.PutViewAtTheTop(margin: outerMargin);

            bgRect.Width += innerMargin + outerMargin;
            bgRect.Height += innerMargin + outerMargin;
            bgRect.X -= Preferences.FpsCounterPosRight ? innerMargin : outerMargin;
            bgRect.Y -= outerMargin;
        }

        public override void Draw()
        {
            // background

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, this.bgRect, Color.Black * 0.6f * this.viewParams.drawOpacity);

            // text

            Helpers.DrawTextInsideRect(font: SonOfRobinGame.fontFreeSansBold24, text: this.CounterText, rectangle: this.textRect, color: this.CounterColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);

            // graph

            if (Preferences.FpsCounterShowGraph)
            {
                int oneEntryWidth = this.graphRect.Width / historyLength;
                for (int i = 0; i < this.percentageHistory.Count; i++)
                {
                    double percentage = this.percentageHistory[i];
                    int barYOffset = this.graphRect.Height - ((int)(this.graphRect.Height * percentage));

                    Rectangle graphBarRect = new Rectangle(x: this.graphRect.X + (i * oneEntryWidth), y: this.graphRect.Y + barYOffset, width: oneEntryWidth, height: 1);

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, graphBarRect, Color.White * this.viewParams.drawOpacity);
                }
            }

        }

    }
}
