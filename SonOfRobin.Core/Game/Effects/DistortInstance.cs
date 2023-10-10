using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class DistortInstance : EffInstance
    {
        private readonly ScrollingSurface scrollingSurface;
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;
        private readonly float globalDistortionPower;
        private readonly float distortionFromOffsetPower;
        private readonly float distortionSizeMultiplier;
        private readonly float distortionOverTimePower;
        private readonly float distortionOverTimeDuration;
        private readonly Vector2 baseTextureSize;

        public DistortInstance(ScrollingSurface scrollingSurface, Texture2D distortTexture, float globalDistortionPower, float distortionFromOffsetPower, float distortionSizeMultiplier = 1f, float distortionOverTimePower = 0f, int distortionOverTimeDuration = 60, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectDistort, framesLeft: framesLeft, priority: priority)
        {
            this.globalDistortionPower = globalDistortionPower;
            this.distortionFromOffsetPower = distortionFromOffsetPower;
            this.distortionSizeMultiplier = distortionSizeMultiplier;
            this.distortionOverTimePower = distortionOverTimePower;
            this.distortionOverTimeDuration = distortionOverTimeDuration;
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

            this.effect.Parameters["globalDistortionPower"].SetValue(this.globalDistortionPower);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["distortionFromOffsetPower"].SetValue(this.distortionFromOffsetPower);
            this.effect.Parameters["distortionSizeMultiplier"].SetValue(this.distortionSizeMultiplier);
           
            this.effect.Parameters["currentUpdate"].SetValue(SonOfRobinGame.CurrentUpdate);
            this.effect.Parameters["distortionOverTimePower"].SetValue(this.distortionOverTimePower);
            this.effect.Parameters["distortionOverTimeDuration"].SetValue(this.distortionOverTimeDuration);

            // MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} offset {this.scrollingSurface.offset}", textColor: Color.Orange);

            base.TurnOn(currentUpdate: currentUpdate, drawColor);
        }
    }
}