using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class RainInstance : EffInstance
    {
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;
        private readonly Vector2 baseTextureSize;
        private readonly Vector2 distortTextureSize;

        public RainInstance(Texture2D baseTexture, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectRain, framesLeft: framesLeft, priority: priority)
        {
            this.baseTexture = baseTexture;
            this.distortTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterDrops);
            this.baseTextureSize = new Vector2(this.baseTexture.Width, this.baseTexture.Height);
            this.distortTextureSize = new Vector2(this.distortTexture.Width, this.distortTexture.Height);
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["baseTextureSize"].SetValue(this.baseTextureSize);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["distortionTextureSize"].SetValue(this.distortTextureSize);
            this.effect.Parameters["rainPower"].SetValue(this.intensityForTweener);
            this.effect.Parameters["currentUpdate"].SetValue(SonOfRobinGame.CurrentUpdate);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}