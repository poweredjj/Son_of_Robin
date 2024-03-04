using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class MainLogoInstance : EffInstance
    {
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;
        private readonly Vector2 baseTextureSize;
        public Vector2 distortTextureOffset;

        public MainLogoInstance(Texture2D baseTexture, Texture2D distortTexture, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectMainLogo, framesLeft: framesLeft, priority: priority)
        {
            this.baseTexture = baseTexture;
            this.distortTexture = distortTexture;
            this.baseTextureSize = new Vector2(this.baseTexture.Width, this.baseTexture.Height);
            this.distortTextureOffset = Vector2.Zero;
        }

        public override void TurnOn(int currentUpdate, Color drawColor)
        {
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["baseTextureSize"].SetValue(this.baseTextureSize);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["distortPower"].SetValue(this.intensityForTweener);
            this.effect.Parameters["distortionTextureOffset"].SetValue(this.distortTextureOffset);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}