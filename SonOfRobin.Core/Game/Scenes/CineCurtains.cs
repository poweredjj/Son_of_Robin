using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class CineCurtains : Scene
    {
        private readonly Tweener tweener;
        public float showPercentage;
        private Rectangle topRect;
        private Rectangle bottomRect;

        public CineCurtains() : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.tweener = new Tweener();
            this.showPercentage = 0f;
            this.topRect = new Rectangle(x: 0, y: 0, width: 1, height: 1);
            this.bottomRect = new Rectangle(x: 0, y: 0, width: 1, height: 1);
        }

        public bool Enabled
        {
            get { return this.showPercentage > 0; }
            set
            {
                if (this.Enabled == value) return;

                Tween tweenCurtain = this.tweener.FindTween(target: this, memberName: "showPercentage");
                tweenCurtain?.CancelAndComplete();

                this.tweener.TweenTo(target: this, expression: curtains => curtains.showPercentage, toValue: value ? 1f : 0f, duration: 0.35f) // 0.35f
                    .Easing(EasingFunctions.CircleOut);
            }
        }

        public override void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw()
        {
            if (this.showPercentage == 0) return;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            int screenWidth = SonOfRobinGame.ScreenWidth;
            int screenHeight = SonOfRobinGame.ScreenHeight;

            int curtainHeight = (int)(screenHeight * 0.12f * this.showPercentage);

            this.topRect.Width = screenWidth;
            this.topRect.Height = curtainHeight;

            this.bottomRect.Y = screenHeight - curtainHeight;
            this.bottomRect.Width = screenWidth;
            this.bottomRect.Height = curtainHeight;

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, topRect, Color.Black);
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bottomRect, Color.Black);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}