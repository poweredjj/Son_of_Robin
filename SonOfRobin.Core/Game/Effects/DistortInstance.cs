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
        private readonly Vector2 baseTextureSize;

        public DistortInstance(ScrollingSurface scrollingSurface, Texture2D distortTexture, float distortionMultiplier, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectDistort, framesLeft: framesLeft, priority: priority)
        {
            this.distortionMultiplier = distortionMultiplier;
            this.scrollingSurface = scrollingSurface;
            this.baseTexture = this.scrollingSurface.texture;
            this.distortTexture = distortTexture;
            this.baseTextureSize = new Vector2(this.baseTexture.Width, this.baseTexture.Height);
        }

        public override void TurnOn(int currentUpdate, Color drawColor = default)
        {
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);

            this.effect.Parameters["distortionMultiplier"].SetValue(this.distortionMultiplier);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["baseTextureOffset"].SetValue(this.scrollingSurface.offset);

            Vector2 baseOffsetCorrection = new Vector2(
                (this.scrollingSurface.offset.X - (int)this.scrollingSurface.offset.X) / this.baseTexture.Width,
                (this.scrollingSurface.offset.Y - (int)this.scrollingSurface.offset.Y) / this.baseTexture.Height
                );

            this.effect.Parameters["baseTextureCorrection"].SetValue(baseOffsetCorrection); // fractional part of offset
            this.effect.Parameters["baseTextureSize"].SetValue(this.baseTextureSize);

            // MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} offset {this.scrollingSurface.offset}", textColor: Color.Orange);

            base.TurnOn(currentUpdate: currentUpdate, drawColor);
        }
    }
}