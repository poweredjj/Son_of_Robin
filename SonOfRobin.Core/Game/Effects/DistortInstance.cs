using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class DistortInstance : EffInstance
    {
        private readonly ScrollingSurface scrollingSurface;
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;
        private readonly float distortionPowerMultiplier;
        private readonly float distortionSizeMultiplier;
        private readonly Vector2 baseTextureSize;

        public DistortInstance(ScrollingSurface scrollingSurface, Texture2D distortTexture, float distortionPowerMultiplier, float distortionSizeMultiplier = 1f, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectDistort, framesLeft: framesLeft, priority: priority)
        {
            this.distortionPowerMultiplier = distortionPowerMultiplier;
            this.distortionSizeMultiplier = distortionSizeMultiplier;
            this.scrollingSurface = scrollingSurface;
            this.baseTexture = this.scrollingSurface.texture;
            this.distortTexture = distortTexture;
            this.baseTextureSize = new Vector2(this.baseTexture.Width, this.baseTexture.Height);
        }

        public override void TurnOn(int currentUpdate, Color drawColor = default)
        {
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["baseTextureOffset"].SetValue(this.scrollingSurface.offset);
            this.effect.Parameters["baseTextureSize"].SetValue(this.baseTextureSize);

            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["distortionPowerMultiplier"].SetValue(this.distortionPowerMultiplier);
            this.effect.Parameters["distortionSizeMultiplier"].SetValue(this.distortionSizeMultiplier);

            // MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} offset {this.scrollingSurface.offset}", textColor: Color.Orange);

            base.TurnOn(currentUpdate: currentUpdate, drawColor);
        }
    }
}