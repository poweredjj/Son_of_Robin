using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class BlurInstance : EffInstance
    {
        public Vector2 blurSize;
        private readonly Vector2 textureSize;
        private readonly Tweener tweener;

        public BlurInstance(Point blurSize, Vector2 textureSize, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBlur, framesLeft: framesLeft, priority: priority)
        {
            Vector2 blurSizeVector = new Vector2(blurSize.X, blurSize.Y);

            this.blurSize = blurSizeVector;

            if (this.framesLeft > 0)
            {
                this.tweener = new Tweener();
                this.blurSize = Vector2.Zero;

                this.tweener.TweenTo(target: this, expression: instance => instance.blurSize, toValue: blurSizeVector, duration: (float)framesLeft / 2 / 60, delay: 0)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticIn);
            }

            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.tweener?.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            // MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} blurSize {this.blurSize}");

            this.effect.Parameters["blurX"].SetValue((int)this.blurSize.X);
            this.effect.Parameters["blurY"].SetValue((int)this.blurSize.Y);
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate);
        }
    }
}