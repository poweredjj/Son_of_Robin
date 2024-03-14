using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;

namespace SonOfRobin
{
    public class ScrollingSurfaceManager
    {
        public static readonly Color waterColor = new(12, 122, 156);
        private readonly ScrollingSurface oceanFloor;
        private readonly ScrollingSurface waterCaustics;
        private readonly ScrollingSurface waterStarsReflection;
        private readonly ScrollingSurface cloudReflectionWhite;
        private readonly ScrollingSurface cloudReflectionDark;
        public readonly ScrollingSurface hotAir;
        public readonly ScrollingSurface fog;
        public readonly World world;

        public ScrollingSurfaceManager(World world)
        {
            BlendState additiveBlend = new()
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            this.world = world;

            // to properly scroll, every ScrollingSurface needs to have effInstance = ScrollingSurfaceDrawInstance set

            Texture2D textureDistort = TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor);

            this.oceanFloor = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.55f, opacityTweenVal: 0.25f, useTweenForOffset: false, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingOceanFloor));
            this.oceanFloor.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.oceanFloor, world: this.world, distortTexture: textureDistort, globalDistortionPower: 0.12f, distortionFromOffsetPower: 0f, distortionOverTimePower: 1f, distortionOverTimeDuration: 60);

            this.waterCaustics = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.25f, opacityTweenVal: 0.25f, useTweenForOffset: true, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics), blendState: additiveBlend);
            this.waterCaustics.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.waterCaustics, world: this.world, distortTexture: textureDistort, globalDistortionPower: 1f, distortionFromOffsetPower: 1f, distortionSizeMultiplier: 2.6f, distortionOverTimePower: 0.2f, distortionOverTimeDuration: 120);

            this.waterStarsReflection = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: false, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingStars), blendState: additiveBlend);
            this.waterStarsReflection.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.waterStarsReflection, world: this.world, distortTexture: textureDistort, globalDistortionPower: 0.3f, distortionFromOffsetPower: 0.3f, distortionSizeMultiplier: 2.5f, distortionOverTimePower: 0.2f, distortionOverTimeDuration: 120, cameraPosOffsetPower: -0.25f);

            this.cloudReflectionWhite = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, scale: 2.0f, useTweenForOffset: false, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingCloudsWhite), blendState: additiveBlend);
            this.cloudReflectionWhite.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.cloudReflectionWhite, world: this.world, distortTexture: textureDistort, globalDistortionPower: 0.3f, distortionFromOffsetPower: 0.3f, distortionSizeMultiplier: 2.5f, distortionOverTimePower: 0.2f, distortionOverTimeDuration: 120, cameraPosOffsetPower: -0.2f);

            this.cloudReflectionDark = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, scale: 3.0f, useTweenForOffset: false, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingCloudsDark));
            this.cloudReflectionDark.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.cloudReflectionDark, world: this.world, distortTexture: textureDistort, globalDistortionPower: 0.3f, distortionFromOffsetPower: 0.7f, distortionSizeMultiplier: 1.5f, distortionOverTimePower: 0.3f, distortionOverTimeDuration: 120, cameraPosOffsetPower: -0.2f);

            this.fog = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: true, maxScrollingOffsetX: 60, maxScrollingOffsetY: 60, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingFog));
            this.fog.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.fog, world: this.world, distortTexture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor), globalDistortionPower: 0.9f, distortionFromOffsetPower: 0f, distortionSizeMultiplier: 0.35f, distortionOverTimePower: 3.5f, distortionOverTimeDuration: 100);

            this.hotAir = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: false, world: this.world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor));
            this.hotAir.effInstance = new ScrollingSurfaceDrawInstance(scrollingSurface: this.hotAir, world: this.world, distortTexture: textureDistort, globalDistortionPower: 0f, distortionFromOffsetPower: 0f);
            this.hotAir.updateDlgt = () => { this.hotAir.offset += new Vector2(-1, 1); };
        }

        public void Update(bool updateFog, bool updateHotAir)
        {
            this.oceanFloor.Update();
            this.waterCaustics.Update();
            this.waterStarsReflection.Update();
            this.cloudReflectionWhite.Update();
            this.cloudReflectionWhite.offset += new Vector2(this.world.weather.WindOriginX, this.world.weather.WindOriginY) * (this.world.weather.WindPercentage + 0.2f) * 0.5f;
            if (updateFog) this.fog.Update();
            if (updateHotAir) this.hotAir.Update();
        }

        public void DrawAllWater(float starsOpacity, float sunShadowsOpacity)
        {
            bool waterFound = false;

            Span<Cell> visibleCellsAsSpan = this.world.Grid.GetCellsInsideRect(this.world.camera.viewRect, addPadding: false).AsSpan();
            for (int i = 0; i < visibleCellsAsSpan.Length; i++)
            {
                if (visibleCellsAsSpan[i].HasWater)
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

            if (starsOpacity > 0) this.waterStarsReflection.Draw(opacityOverride: starsOpacity);
            if (sunShadowsOpacity > 0) this.cloudReflectionWhite.Draw(opacityOverride: sunShadowsOpacity);

            if (this.world.weather.CloudsPercentage > 0) this.cloudReflectionDark.Draw(opacityOverride: Math.Min(this.world.weather.CloudsPercentage * 1.2f, 0.8f));

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
        private readonly float scale;
        public Scheduler.ExecutionDelegate updateDlgt;

        public Vector2 offset;
        public float opacity;

        private readonly Tweener tweener;

        public ScrollingSurface(World world, Texture2D texture, bool useTweenForOpacity, bool useTweenForOffset, float opacityBaseVal, float opacityTweenVal, float scale = 1f, int maxScrollingOffsetX = 150, int maxScrollingOffsetY = 150, BlendState blendState = null)
        {
            this.world = world;
            this.texture = texture;
            this.scale = scale;
            this.effInstance = null;
            this.blendState = blendState == null ? BlendState.AlphaBlend : blendState;
            this.useTweenForOpacity = useTweenForOpacity;
            this.useTweenForOffset = useTweenForOffset;
            this.updateDlgt = null; // to be updated manually, after executing constructor

            this.opacityTweenVal = opacityTweenVal;
            this.maxScrollingOffset = new Vector2(maxScrollingOffsetX, maxScrollingOffsetY);

            this.offset = Vector2.Zero;
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

            if (this.updateDlgt != null) new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, executeHelper: this.updateDlgt, delay: 0);
        }

        public void Draw(float opacityOverride = -1f, Color drawColorOverride = default, bool endSpriteBatch = true)
        {
            if (endSpriteBatch) SonOfRobinGame.SpriteBatch.End();
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix, blendState: this.blendState, effect: this.effInstance == null ? null : this.effInstance.effect, samplerState: SamplerState.LinearWrap, sortMode: SpriteSortMode.Immediate);

            float drawOpacity = opacityOverride == -1f ? this.opacity : opacityOverride;
            Color drawColor = (drawColorOverride == default ? Color.White : drawColorOverride) * drawOpacity;

            this.effInstance?.TurnOn(currentUpdate: this.world.CurrentUpdate, drawColor: drawColor);

            // offset is not taken into account here - it will be applied using shader

            Rectangle viewRect = this.world.camera.viewRect;

            int realTexWidth = (int)((float)this.texture.Width * this.scale);
            int realTexHeight = (int)((float)this.texture.Height * this.scale);

            int startColumn = viewRect.X / realTexWidth;
            int startRow = viewRect.Y / realTexHeight;

            if (viewRect.X < (startColumn * realTexWidth)) startColumn--;
            if (viewRect.Y < (startRow * realTexHeight)) startRow--;

            int drawRectX = startColumn * realTexWidth;
            int drawRectY = startRow * realTexHeight;
            int drawRectWidth = viewRect.Width + (viewRect.X - drawRectX) + 4;
            int drawRectHeight = viewRect.Height + (viewRect.Y - drawRectY) + 3;

            Rectangle destRect = new(x: drawRectX, y: drawRectY, width: drawRectWidth, height: drawRectHeight);
            Rectangle sourceRect = destRect;
            sourceRect.Width = (int)((float)sourceRect.Width / this.scale);
            sourceRect.Height = (int)((float)sourceRect.Height / this.scale);

            // sourceRect.Location = Point.Zero;

            // DO NOT use worldspace rect for drawing, because precision errors will pixelate water on mobile

            SonOfRobinGame.SpriteBatch.Draw(this.texture, destRect, sourceRect, drawColor);
        }
    }
}