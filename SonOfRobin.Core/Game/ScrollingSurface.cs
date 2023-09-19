﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class ScrollingSurfaceManager
    {
        private static readonly Color waterColor = new Color(12, 122, 156);
        private readonly ScrollingSurface oceanFloor;
        private readonly ScrollingSurface waterCaustics1;
        private readonly ScrollingSurface waterCaustics2;
        private readonly ScrollingSurface fog;
        public readonly World world;

        public ScrollingSurfaceManager(World world)
        {
            this.world = world;

            this.oceanFloor = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.55f, opacityTweenVal: 0.25f, useTweenForOffset: false, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingOceanFloor));
            this.waterCaustics1 = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.2f, opacityTweenVal: 0.05f, useTweenForOffset: true, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics1));
            this.waterCaustics2 = new ScrollingSurface(useTweenForOpacity: true, opacityBaseVal: 0.2f, opacityTweenVal: 0.05f, useTweenForOffset: true, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingWaterCaustics2));
            this.fog = new ScrollingSurface(useTweenForOpacity: false, opacityBaseVal: 1f, opacityTweenVal: 1f, useTweenForOffset: true, maxScrollingOffset: 60, world: world, texture: TextureBank.GetTexture(TextureBank.TextureName.RepeatingFog));
        }

        public void Update(bool updateFog)
        {
            this.oceanFloor.Update();
            this.waterCaustics1.Update();
            this.waterCaustics2.Update();
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

            this.StartSpriteBatch(waterBlendMode: false);
            SonOfRobinGame.GfxDev.Clear(waterColor);

            if (!Preferences.highQualityWater)
            {
                SonOfRobinGame.SpriteBatch.End();
                return;
            }

            this.oceanFloor.Draw();

            SonOfRobinGame.SpriteBatch.End();
            this.StartSpriteBatch(waterBlendMode: true);

            this.waterCaustics1.Draw();
            this.waterCaustics2.Draw();

            SonOfRobinGame.SpriteBatch.End();
        }

        public void DrawFog(float opacity)
        {
            this.fog.Draw(opacity);
        }

        public void StartSpriteBatch(bool waterBlendMode = false)
        {
            BlendState blendState = waterBlendMode ?
                new BlendState
                {
                    AlphaBlendFunction = BlendFunction.ReverseSubtract,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.One,

                    ColorBlendFunction = BlendFunction.Add,
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.One,
                } :
                BlendState.AlphaBlend;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix, blendState: blendState, samplerState: SamplerState.LinearWrap);
        }
    }

    public class ScrollingSurface
    {
        private readonly World world;
        private readonly Texture2D texture;
        private readonly bool useTweenForOpacity;
        private readonly bool useTweenForOffset;
        private readonly float opacityTweenVal;
        private readonly int maxScrollingOffset;

        public Vector2 offset;
        public float opacity;

        private readonly Tweener tweener;

        public ScrollingSurface(World world, Texture2D texture, bool useTweenForOpacity, bool useTweenForOffset, float opacityBaseVal, float opacityTweenVal, int maxScrollingOffset = 150)
        {
            this.world = world;
            this.texture = texture;
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
            Rectangle viewRect = this.world.camera.viewRect;

            int offsetX = (int)this.offset.X;
            int offsetY = (int)this.offset.Y;

            Color drawColor = Color.White * (opacityOverride == -1f ? this.opacity : opacityOverride);

            int startColumn = (int)((viewRect.X - this.offset.X) / this.texture.Width);
            int startRow = (int)((viewRect.Y - this.offset.Y) / this.texture.Height);

            if (viewRect.X < (startColumn * this.texture.Width) + offsetX) startColumn--;
            if (viewRect.Y < (startRow * this.texture.Height) + offsetY) startRow--;

            int drawRectX = (startColumn * this.texture.Width) + offsetX;
            int drawRectY = (startRow * this.texture.Height) + offsetY;
            int drawRectWidth = viewRect.Width + (viewRect.X - drawRectX) + 4;
            int drawRectHeight = viewRect.Height + (viewRect.Y - drawRectY) + 3;

            Rectangle destRect = new(x: drawRectX, y: drawRectY, width: drawRectWidth, height: drawRectHeight);

            Rectangle sourceRect = destRect;
            sourceRect.Location = Point.Zero;

            SonOfRobinGame.SpriteBatch.Draw(this.texture, destRect, sourceRect, drawColor);
        }
    }
}