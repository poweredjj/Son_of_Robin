using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class FpsCounter : Scene
    {
        private bool hasBeenInitialized;
        private Rectangle bgRect;
        private Rectangle textRect;

        private string CounterText { get { return $"FPS {SonOfRobinGame.fps.Frames}"; } }
        private SpriteFont Font { get { return SonOfRobinGame.fontPixelMix5; } }

        public FpsCounter() : base(inputType: InputTypes.None, priority: -2, blocksUpdatesBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.hasBeenInitialized = false;
        }

        private void Initialize()
        {
            if (this.hasBeenInitialized) throw new ArgumentException("Fps counter has already been initialized.");

            this.AdaptToNewSize();
            this.transManager.AddTransition(this.GetTransition(inTrans: true));
            this.hasBeenInitialized = true;
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding) this.transManager.AddTransition(this.GetTransition(inTrans: false));
            else base.Remove();
        }

        public override void Update(GameTime gameTime)
        { }

        public Transition GetTransition(bool inTrans)
        {
            this.AdaptToNewSize();

            return new Transition(transManager: this.transManager, outTrans: !inTrans, baseParamName: "PosX", targetVal: this.viewParams.PosX + this.viewParams.Width, duration: 12, endRemoveScene: !inTrans);
        }

        protected override void AdaptToNewSize()
        {
            Vector2 textSize = this.Font.MeasureString(this.CounterText);
            double textAspectRatio = textSize.Y / textSize.X;

            int width = (int)(SonOfRobinGame.VirtualWidth * 0.1f);
            int height = (int)(width * textAspectRatio);

            this.textRect = new Rectangle(x: 0, y: 0, width: width, height: height);
            this.bgRect = this.textRect;

            bgRect.Inflate(2, 2);

            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = bgRect.Height;

            this.viewParams.PutViewAtTheRight();
            this.viewParams.PutViewAtTheTop();
        }

        public override void Draw()
        {
            if (!SonOfRobinGame.initialLoadingFinished) return;
            if (!this.hasBeenInitialized) this.Initialize();

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, this.bgRect, Color.Black * 0.4f * this.viewParams.drawOpacity);

            Helpers.DrawTextInsideRectWithOutline(font: SonOfRobinGame.fontFreeSansBold24, text: this.CounterText, rectangle: this.textRect, color: Color.White * this.viewParams.drawOpacity, outlineColor: Color.Black, outlineSize: 1, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center, drawTestRect: true);
        }

    }
}
