using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class JustWater : Scene
    {
        private readonly Texture2D oceanFloorTexture;
        private readonly Texture2D waterCausticsTexture;

        private readonly Tweener tweener;
        public Vector2 oceanFloorOffset;
        public Vector2 causticsOffset1;
        public Vector2 causticsOffset2;
        public float causticsOpacity1;
        public float causticsOpacity2;

        private BlendState waterBlend = new()
        {
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,

            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
        };

        public JustWater() : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.oceanFloorTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingOceanFloor);
            this.waterCausticsTexture = TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics);

            this.oceanFloorOffset = Vector2.Zero;
            this.causticsOffset1 = Vector2.Zero;
            this.causticsOffset2 = Vector2.Zero;
            this.causticsOpacity1 = 0.4f;
            this.causticsOpacity2 = 0f;

            this.tweener = new Tweener();

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOffset1, toValue: new Vector2(waterCausticsTexture.Width * 1, waterCausticsTexture.Height * 2), duration: 16)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOffset2, toValue: new Vector2(waterCausticsTexture.Width * -2, waterCausticsTexture.Height * -1), duration: 12)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOpacity1, toValue: 0f, duration: 16)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);

            this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.causticsOpacity1, toValue: 0.4f, duration: 20)
                .AutoReverse()
                .Easing(EasingFunctions.SineInOut);
        }

        public override void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            this.oceanFloorOffset += new Vector2(0.25f, 0.25f);
        }

        public override void Draw()
        {
            // drawing ocean floor

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);

            SonOfRobinGame.GfxDev.Clear(Color.RoyalBlue * this.viewParams.drawOpacity);

            Rectangle oceanFloorSourceRect = new((int)this.oceanFloorOffset.X, (int)this.oceanFloorOffset.Y, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight);

            Rectangle oceanFloorDestRect = new(x: 0, y: 0, width: SonOfRobinGame.ScreenWidth, height: SonOfRobinGame.ScreenHeight);
            SonOfRobinGame.SpriteBatch.Draw(this.oceanFloorTexture, oceanFloorDestRect, oceanFloorSourceRect, Color.White * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.End();

            // drawing caustics

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, blendState: waterBlend, samplerState: SamplerState.LinearWrap);

            foreach (var kvp in new Dictionary<Vector2, float> { { this.causticsOffset1, this.causticsOpacity1 }, { this.causticsOffset2, this.causticsOpacity2 }, })
            {
                Vector2 causticsOffset = kvp.Key;
                float opacity = kvp.Value;

                Rectangle causticsSourceRect = new((int)causticsOffset.X, (int)causticsOffset.Y, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight);

                Rectangle causticsDestRect = new(x: 0, y: 0, width: SonOfRobinGame.ScreenWidth, height: SonOfRobinGame.ScreenHeight);
                SonOfRobinGame.SpriteBatch.Draw(this.waterCausticsTexture, causticsDestRect, causticsSourceRect, Color.White * opacity);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}