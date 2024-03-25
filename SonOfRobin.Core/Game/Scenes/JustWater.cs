using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class JustWater : Scene
    {
        private readonly BlendState waterBlend = new()
        {
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,

            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
        };

        private readonly Texture2D oceanFloorTexture;
        private readonly Texture2D waterCausticsTexture;
        private readonly Texture2D distortionTexture;

        private readonly Tweener tweener;
        public Vector2 causticsOffset1;
        public Vector2 causticsOffset2;
        public float causticsOpacity1;
        public float causticsOpacity2;
        private readonly HeatMaskDistortionInstance heatMaskDistortInstance;

        public JustWater() : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.oceanFloorTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingOceanFloor);
            this.waterCausticsTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics);
            this.distortionTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor);

            this.heatMaskDistortInstance = new HeatMaskDistortionInstance(distortTexture: this.distortionTexture);
            this.heatMaskDistortInstance.baseTexture = this.waterCausticsTexture;


            this.causticsOffset1 = Vector2.Zero;
            this.causticsOffset2 = Vector2.Zero;
            this.causticsOpacity1 = 0.4f;
            this.causticsOpacity2 = 0f;

            this.tweener = new Tweener();

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOffset1, toValue: new Vector2(waterCausticsTexture.Width * 1, waterCausticsTexture.Height * 2), duration: 16)
                .AutoReverse()
                .RepeatForever()
                .Easing(EasingFunctions.SineInOut);

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOffset2, toValue: new Vector2(waterCausticsTexture.Width * -2, waterCausticsTexture.Height * -1), duration: 12)
                .AutoReverse()
                .RepeatForever()
                .Easing(EasingFunctions.QuadraticInOut);

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOpacity1, toValue: 0f, duration: 16)
                .AutoReverse()
                .RepeatForever()
                .Easing(EasingFunctions.SineInOut);

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOpacity2, toValue: 0.4f, duration: 20)
                .AutoReverse()
                .RepeatForever()
                .Easing(EasingFunctions.SineInOut);

            this.tweener.TweenTo(target: this.viewParams, expression: viewParams => viewParams.drawScaleX, toValue: 0.6f, duration: 20)
                .AutoReverse()
                .RepeatForever()
                .Easing(EasingFunctions.SineInOut);

            this.tweener.TweenTo(target: this.viewParams, expression: viewParams => viewParams.drawScaleY, toValue: 0.6f, duration: 20)
                .AutoReverse()
                .RepeatForever()
                .Easing(EasingFunctions.SineInOut);
        }

        public override void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw()
        {
            // drawing ocean floor

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);
            SonOfRobinGame.GfxDev.Clear(ScrollingSurfaceManager.waterColor * this.viewParams.drawOpacity);

            Rectangle oceanFloorDestRect = new(x: 0, y: 0, width: SonOfRobinGame.ScreenWidth, height: SonOfRobinGame.ScreenHeight);

            SonOfRobinGame.SpriteBatch.Draw(this.oceanFloorTexture, oceanFloorDestRect, oceanFloorDestRect, Color.White * 0.6f * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.End();

            // drawing caustics

            var offsetArray = new Vector2[] { this.causticsOffset1, this.causticsOffset2 };
            var opacityArray = new float[] { this.causticsOpacity1, this.causticsOpacity2 };

            Rectangle causticsDestRect = new(x: 0, y: 0, width: SonOfRobinGame.ScreenWidth, height: SonOfRobinGame.ScreenHeight);

            for (int i = 0; i < offsetArray.Length; i++)
            {
                Vector2 causticsOffset = offsetArray[i];
                float opacity = opacityArray[i];

                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate, blendState: waterBlend, samplerState: SamplerState.LinearWrap);

                this.heatMaskDistortInstance.baseTextureOffset = causticsOffset;
                this.heatMaskDistortInstance.intensityForTweener = opacity * 4f;
                this.heatMaskDistortInstance.TurnOn(currentUpdate: SonOfRobinGame.CurrentUpdate, drawColor: Color.White);

                SonOfRobinGame.SpriteBatch.Draw(this.waterCausticsTexture, causticsDestRect, causticsDestRect, Color.White * opacity);

                SonOfRobinGame.SpriteBatch.End();
            }
        }
    }
}