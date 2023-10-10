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

            this.oceanFloor = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.55f, opacityTweenVal: 0.25f, useTweenForOffset: false, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingOceanFloor));
            this.oceanFloor.effInstance = new DistortInstance(scrollingSurface: this.oceanFloor, distortTexture: textureDistort, distortionPowerMultiplier: 0.12f);

            this.waterCaustics = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.25f, opacityTweenVal: 0.25f, useTweenForOffset: true, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics), blendState: waterBlend);
            this.waterCaustics.effInstance = new DistortInstance(scrollingSurface: this.waterCaustics, distortTexture: textureDistort, distortionPowerMultiplier: 1f, distortionSizeMultiplier: 2.6f);

            this.fog = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: true, maxScrollingOffset: 60, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingFog));
            this.fog.effInstance = new DistortInstance(scrollingSurface: this.fog, distortTexture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingPerlinNoiseColor), distortionPowerMultiplier: 1.6f, distortionSizeMultiplier: 0.4f);
        }

        public void Update(bool updateFog)
        {
            this.oceanFloor.Update();
            this.waterCaustics.Update();
            if (updateFog) this.fog.Update();
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

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix);
            SonOfRobinGame.GfxDev.Clear(waterColor);

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
        private readonly int maxScrollingOffset;
        public EffInstance effInstance;
        private readonly BlendState blendState;

        public Vector2 offset;
        public float opacity;

        private readonly Tweener tweener;

        public ScrollingSurface(World world, Texture2D texture, bool useTweenForOpacity, bool useTweenForOffset, float opacityBaseVal, float opacityTweenVal, int maxScrollingOffset = 150, BlendState blendState = null)
        {
            this.world = world;
            this.texture = texture;
            this.effInstance = null;
            this.blendState = blendState == null ? BlendState.AlphaBlend : blendState;
            this.useTweenForOpacity = useTweenForOpacity;
            this.useTweenForOffset = useTweenForOffset;

            this.opacityTweenVal = opacityTweenVal;
            this.maxScrollingOffset = maxScrollingOffset;

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
                    this.world.random.Next(-this.maxScrollingOffset, this.maxScrollingOffset),
                    this.world.random.Next(-this.maxScrollingOffset, this.maxScrollingOffset));

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