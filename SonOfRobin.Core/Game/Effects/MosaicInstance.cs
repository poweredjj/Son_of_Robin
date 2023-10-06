using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class MosaicInstance : EffInstance
    {
        public Vector2 blurSize;
        private readonly Vector2 textureSize;

        public MosaicInstance(Vector2 blurSize, Vector2 textureSize, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectMosaic, framesLeft: framesLeft, priority: priority)
        {
            this.blurSize = blurSize;
            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["blurSize"].SetValue(Vector2.Max(this.blurSize * this.intensityForTweener, Vector2.Zero));
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate);
        }
    }
}