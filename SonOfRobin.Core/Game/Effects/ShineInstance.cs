using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class ShineInstance : EffInstance
    {
        private readonly Vector4 color;
        private readonly int fadeFramesTotal;
        private int fadeFramesLeft;

        public ShineInstance(Color color, int framesLeft = 1, int priority = 1, int fadeFramesLeft = 0) : base(effect: SonOfRobinGame.EffectShine, framesLeft: framesLeft, priority: priority)
        {
            this.color = color.ToVector4();
            this.fadeFramesTotal = fadeFramesLeft;
            this.fadeFramesLeft = fadeFramesLeft;
        }

        public override void TurnOn(int currentUpdate, Color drawColor)
        {
            float opacity = 1f;

            if (this.fadeFramesTotal > 0)
            {
                this.fadeFramesLeft--;
                opacity = (float)this.fadeFramesLeft / (float)this.fadeFramesTotal;
            }

            this.effect.Parameters["shineColor"].SetValue(this.color);
            this.effect.Parameters["opacity"].SetValue(opacity * this.intensityForTweener);
            this.effect.Parameters["drawColor"].SetValue(drawColor.ToVector4());

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}