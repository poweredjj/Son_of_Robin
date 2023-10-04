using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class BlurInstance : EffInstance
    {
        private readonly Point blurSize;
        private readonly Vector2 textureSize;

        public BlurInstance(Point blurSize, Vector2 textureSize, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBlur, framesLeft: framesLeft, priority: priority)
        {
            this.blurSize = blurSize;
            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["blurX"].SetValue(this.blurSize.X);
            this.effect.Parameters["blurY"].SetValue(this.blurSize.Y);
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate);
        }
    }
}