using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class BlurInstance : EffInstance
    {
        public Vector2 blurSize;
        private readonly Vector2 textureSize;
        private Tweener tweener;

        private readonly Vector2 startBlurSize;
        private readonly Vector2 endBlurSize;
        private readonly bool autoreverse;

        public BlurInstance(Vector2 startBlurSize, Vector2 textureSize, int framesLeft = 1, Vector2 endBlurSize = default, bool autoreverse = false, int priority = 1) : base(effect: SonOfRobinGame.EffectBlur, framesLeft: framesLeft, priority: priority)
        {
            this.startBlurSize = startBlurSize;
            this.endBlurSize = endBlurSize;
            this.autoreverse = autoreverse;

            this.blurSize = startBlurSize;
            this.textureSize = textureSize;
        }

        private void SetTweener()
        {
            if (this.framesLeft > 0)
            {
                this.tweener = new Tweener();

                if (this.endBlurSize == default) this.blurSize = Vector2.Zero;
                this.blurSize = Vector2.Zero;

                this.tweener.TweenTo(target: this, expression: instance => instance.blurSize, toValue: this.endBlurSize == default ? this.startBlurSize : this.endBlurSize, duration: (float)framesLeft / (this.autoreverse ? 2 : 1) / 60, delay: 0);

                Tween tween = this.tweener.FindTween(target: this, memberName: "blurSize");

                if (this.autoreverse) tween.AutoReverse();

                if (this.autoreverse) tween.Easing(EasingFunctions.QuadraticIn);
                else tween.Easing(EasingFunctions.Linear);
            }
        }

        public override void TurnOn(int currentUpdate)
        {
            if (this.tweener == null && this.framesLeft > 0) this.SetTweener();

            this.tweener?.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            // MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} blurSize {this.blurSize}");

            this.effect.Parameters["blurX"].SetValue((int)this.blurSize.X);
            this.effect.Parameters["blurY"].SetValue((int)this.blurSize.Y);
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate);
        }
    }
}