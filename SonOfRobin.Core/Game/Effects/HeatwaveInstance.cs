using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class HeatWaveInstance : EffInstance
    {
        private readonly World world;
        private readonly Texture2D baseTexture;
        private readonly Texture2D distortTexture;
        private readonly Vector2 baseTextureSize;
        private readonly Vector2 distortTextureSize;

        public HeatWaveInstance(World world, Texture2D baseTexture, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectHeatwave, framesLeft: framesLeft, priority: priority)
        {
            this.world = world;
            this.baseTexture = baseTexture;
            this.distortTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor);
            this.baseTextureSize = new Vector2(this.baseTexture.Width, this.baseTexture.Height);
            this.distortTextureSize = new Vector2(this.distortTexture.Width, this.distortTexture.Height);
        }

        private Vector2 DistortionTextureOffset
        {
            get
            {
                float currentTextureWidth = this.distortTexture.Width / (Preferences.GlobalScale / this.world.viewParams.ScaleX);
                float currentTextureHeight = this.distortTexture.Height / (Preferences.GlobalScale / this.world.viewParams.ScaleY);

                return new Vector2(
                    (int)Math.Floor(this.world.camera.CurrentPos.X) % currentTextureWidth / this.distortTexture.Width,
                    (int)Math.Floor(this.world.camera.CurrentPos.Y) % currentTextureHeight / this.distortTexture.Height);
            }
        }

        public override void TurnOn(int currentUpdate, Color drawColor)
        {
            this.effect.Parameters["BaseTexture"].SetValue(this.baseTexture);
            this.effect.Parameters["baseTextureSize"].SetValue(this.baseTextureSize);
            this.effect.Parameters["DistortTexture"].SetValue(this.distortTexture);
            this.effect.Parameters["distortionTextureSize"].SetValue(this.distortTextureSize);
            this.effect.Parameters["heatPower"].SetValue(this.intensityForTweener);
            this.effect.Parameters["currentUpdate"].SetValue(SonOfRobinGame.CurrentUpdate);
            this.effect.Parameters["distortionTextureOffset"].SetValue(this.DistortionTextureOffset);
            this.effect.Parameters["drawScale"].SetValue(new Vector2(
                Preferences.GlobalScale / this.world.viewParams.ScaleX,
                Preferences.GlobalScale / this.world.viewParams.ScaleY));

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}