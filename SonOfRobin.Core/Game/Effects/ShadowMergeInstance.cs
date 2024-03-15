using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class ShadowMergeInstance : EffInstance
    {
        private readonly Texture2D shadowTexture;
        private readonly Texture2D lightTexture;

        public ShadowMergeInstance(Texture2D shadowTexture, Texture2D lightTexture, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectShadowMerge, framesLeft: framesLeft, priority: priority)
        {
            this.shadowTexture = shadowTexture;
            this.lightTexture = lightTexture;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["LightTexture"].SetValue(this.lightTexture);
            this.effect.Parameters["ShadowTexture"].SetValue(this.shadowTexture);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}