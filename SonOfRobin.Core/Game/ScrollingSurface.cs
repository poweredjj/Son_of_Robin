using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class ScrollingSurfaceManager
    {
        private static readonly Color waterColor = new Color(12, 122, 156);
        private readonly ScrollingSurface oceanFloor;
        private readonly ScrollingSurface waterCaustics;
        public readonly ScrollingSurface hotAir;
        public readonly ScrollingSurface fog;
        public readonly World world;

        public ScrollingSurfaceManager(World world)
        {
            BlendState waterBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            this.world = world;

            // to properly scroll, every ScrollingSurface needs to have effInstance = DistortInstance set 

            Texture2D textureDistort = TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor);

            this.oceanFloor = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.55f, opacityTweenVal: 0.25f, useTweenForOffset: false, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingOceanFloor));
            this.oceanFloor.effInstance = new DistortInstance(scrollingSurface: this.oceanFloor, distortTexture: textureDistort, globalDistortionPower: 0.12f, distortionFromOffsetPower: 0f, distortionOverTimePower: 1f, distortionOverTimeDuration: 60);

            this.waterCaustics = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.25f, opacityTweenVal: 0.25f, useTweenForOffset: true, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics), blendState: waterBlend);
            this.waterCaustics.effInstance = new DistortInstance(scrollingSurface: this.waterCaustics, distortTexture: textureDistort, globalDistortionPower: 1f, distortionFromOffsetPower: 1f, distortionSizeMultiplier: 2.6f, distortionOverTimePower: 0.2f, distortionOverTimeDuration: 120);

            this.fog = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: true, maxScrollingOffsetX: 60, maxScrollingOffsetY: 60, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingFog));
            this.fog.effInstance = new DistortInstance(scrollingSurface: this.fog, distortTexture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor), globalDistortionPower: 0.9f, distortionFromOffsetPower: 0f, distortionSizeMultiplier: 0.35f, distortionOverTimePower: 3.5f, distortionOverTimeDuration: 100);

            this.hotAir = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: false, linearScrollPerFrame: new Vector2(-1, 1), world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor));
            this.hotAir.effInstance = new DistortInstance(scrollingSurface: this.hotAir, distortTexture: textureDistort, globalDistortionPower: 0f, distortionFromOffsetPower: 0f);
        }

        public void Update(bool updateFog, bool updateHotAir)
        {
            this.oceanFloor.Update();
            this.waterCaustics.Update();
            if (updateFog) this.fog.Update();
            if (updateHotAir) this.hotAir.Update();
        }

        public void DrawAllWater()
        {
            bool waterFound = false;
            foreach (Cell cell in this.world.Grid.GetCellsInsideRect(this.world.camera.viewRect, addPadding: false))
            {
                if (cell.HasWater)
                {
                    waterFound = true;
                    break;
                }
            }
            if (!waterFound) return;

            SonOfRobinGame.GfxDev.Clear(waterColor);
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix);

            if (!Preferences.highQualityWater)
            {
                SonOfRobinGame.SpriteBatch.End();
                return;
            }

            this.oceanFloor.Draw();
            this.waterCaustics.Draw();

            SonOfRobinGame.SpriteBatch.End();
        }
    }

    public class ScrollingSurface
    {
        private readonly World world;
        public readonly Texture2D texture;
        private readonly bool useTweenForOpacity;
        private readonly bool useTweenForOffset;
        private readonly float opacityTweenVal;
        private readonly Vector2 maxScrollingOffset;
        public EffInstance effInstance;
        private readonly BlendState blendState;
        private readonly Vector2 linearScrollPerFrame;

        public Vector2 offset;
        public float opacity;

        private readonly Tweener tweener;

        public ScrollingSurface(World world, Texture2D texture, bool useTweenForOpacity, bool useTweenForOffset, float opacityBaseVal, float opacityTweenVal, int maxScrollingOffsetX = 150, int maxScrollingOffsetY = 150, BlendState blendState = null, Vector2 linearScrollPerFrame = default)
        {
            this.world = world;
            this.texture = texture;
            this.effInstance = null;
            this.blendState = blendState == null ? BlendState.AlphaBlend : blendState;
            this.useTweenForOpacity = useTweenForOpacity;
            this.useTweenForOffset = useTweenForOffset;
            this.linearScrollPerFrame = linearScrollPerFrame;

            this.opacityTweenVal = opacityTweenVal;
            this.maxScrollingOffset = new Vector2(maxScrollingOffsetX, maxScrollingOffsetY);

            this.offset = new Vector2(0, 0);
            this.opacity = opacityBaseVal;

            this.tweener = new Tweener();

            this.SetTweener();
        }

        private void SetTweener()
        {
            Tween tweenOffset = this.tweener.FindTween(target: this, memberName: "offset");

            if (this.useTweenForOffset && (tweenOffset == null || !tweenOffset.IsAlive))
            {
                Vector2 newPos = Vector2.Zero;

                newPos = this.offset + new Vector2(
                    this.world.random.Next((int)-this.maxScrollingOffset.X, (int)this.maxScrollingOffset.X),
                    this.world.random.Next((int)-this.maxScrollingOffset.Y, (int)this.maxScrollingOffset.Y));

                this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.offset, toValue: newPos, duration: this.world.random.Next(3, 8), delay: 0)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut)
                    .OnEnd(t => this.SetTweener());
            }

            Tween tweenOpacity = this.tweener.FindTween(target: this, memberName: "opacity");

            if (this.useTweenForOpacity && (tweenOpacity == null || !tweenOpacity.IsAlive))
            {
                this.tweener.TweenTo(target: this, expression: scrollingSurface => scrollingSurface.opacity, toValue: this.opacityTweenVal, duration: this.world.random.Next(3, 12), delay: this.world.random.Next(5, 8))
                    .AutoReverse()
                    .Easing(EasingFunctions.SineInOut)
                    .OnEnd(t => this.SetTweener());
            }
        }

        public void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            this.offset += this.linearScrollPerFrame;
        }

        public void Draw(float opacityOverride = -1f)
        {
            SonOfRobinGame.SpriteBatch.End();
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix, blendState: this.blendState, effect: this.effInstance == null ? null : this.effInstance.effect, samplerState: SamplerState.LinearWrap, sortMode: SpriteSortMode.Immediate);

            float drawOpacity = opacityOverride == -1f ? this.opacity : opacityOverride;
            Color drawColor = Color.White * drawOpacity;

            this.effInstance?.TurnOn(currentUpdate: this.world.CurrentUpdate, drawColor: drawColor);

            // offset is not taken into account here - it will be applied using shader

            Rectangle viewRect = this.world.camera.viewRect;

            int startColumn = viewRect.X / this.texture.Width;
            int startRow = viewRect.Y / this.texture.Height;

            if (viewRect.X < (startColumn * this.texture.Width)) startColumn--;
            if (viewRect.Y < (startRow * this.texture.Height)) startRow--;

            int drawRectX = startColumn * this.texture.Width;
            int drawRectY = startRow * this.texture.Height;
            int drawRectWidth = viewRect.Width + (viewRect.X - drawRectX) + 4;
            int drawRectHeight = viewRect.Height + (viewRect.Y - drawRectY) + 3;

            Rectangle destRect = new(x: drawRectX, y: drawRectY, width: drawRectWidth, height: drawRectHeight);
            Rectangle sourceRect = destRect;
            sourceRect.Location = Point.Zero;

            // DO NOT use worldspace rect for drawing, because precision errors will pixelate water on mobile

            SonOfRobinGame.SpriteBatch.Draw(this.texture, destRect, sourceRect, drawColor);
        }
    }
}