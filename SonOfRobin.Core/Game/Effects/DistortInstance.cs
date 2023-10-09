using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class DistortInstance : EffInstance
    {
        private readonly ScrollingSurface scrollingSurface;
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;
        private readonly float distortionMultiplier;

        public DistortInstance(ScrollingSurface scrollingSurface, Texture2D distortTexture, float distortionMultiplier, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectDistort, framesLeft: framesLeft, priority: priority)
        {
            this.distortionMultiplier = distortionMultiplier;
            this.scrollingSurface = scrollingSurface;
            this.baseTexture = this.scrollingSurface.texture;
            this.distortTexture = distortTexture;
        }

        public override void TurnOn(int currentUpdate, Color drawColor = default)
        {
            this.effect.Parameters["distortionMultiplier"].SetValue(this.distortionMultiplier);
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["baseTextureOffset"].SetValue(this.scrollingSurface.offset / new Vector2(this.baseTexture.Width, this.baseTexture.Height));
            base.TurnOn(currentUpdate: currentUpdate, drawColor);
        }
    }
}