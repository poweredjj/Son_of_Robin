using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class DistortWaterInstance : EffInstance
    {
        private readonly ScrollingSurface scrollingSurface;
        private readonly Texture2D waterTexture;
        private readonly Texture2D distortTexture;

        public DistortWaterInstance(ScrollingSurface scrollingSurface, Texture2D waterTexture, Texture2D distortTexture, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectDistortWater, framesLeft: framesLeft, priority: priority)
        {
            this.scrollingSurface = scrollingSurface;
            this.waterTexture = waterTexture;
            this.distortTexture = distortTexture;
        }

        public override void TurnOn(int currentUpdate, Color drawColor = default)
        {
            this.effect.Parameters["WaterTexture"].SetValue(this.waterTexture);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["baseTextureOffset"].SetValue(this.scrollingSurface.offset / new Vector2(this.waterTexture.Width, this.waterTexture.Height));
            base.TurnOn(currentUpdate: currentUpdate, drawColor);
        }
    }
}