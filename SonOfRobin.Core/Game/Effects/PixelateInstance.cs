using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class PixelateInstance : EffInstance
    {
        public Vector2 pixelSize;
        private readonly Vector2 textureSize;

        public PixelateInstance(Vector2 pixelSize, Vector2 textureSize, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectPixelate, framesLeft: framesLeft, priority: priority)
        {
            this.pixelSize = pixelSize;
            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate, Color drawColor = default)
        {
            this.effect.Parameters["effectSize"].SetValue(Vector2.Max(this.pixelSize * this.intensityForTweener, Vector2.One));
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}