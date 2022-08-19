using Microsoft.Xna.Framework;
using System;

namespace SonOfRobin
{
    public class FpsCounter : Scene
    {
        private Rectangle bgRect;
        private Rectangle textRect;
        private static readonly int margin = 2;
        private string CounterText { get { return $"FPS {SonOfRobinGame.fps.Frames}"; } }

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
            int width = (int)(SonOfRobinGame.VirtualWidth * 0.07f);
            width = Math.Min(width, 55);
            width = Math.Max(width, 35);
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

            Helpers.DrawTextInsideRect(font: SonOfRobinGame.fontFreeSansBold24, text: this.CounterText, rectangle: this.textRect, color: Color.White * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
        }

    }
}
