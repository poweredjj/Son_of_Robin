using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;

namespace SonOfRobin
{
    public class WaterSurfaceManager
    {
        private static readonly Color waterColor = new Color(12, 122, 156);
        private readonly WaterSurface waterBottom;
        private readonly WaterSurface waterCaustics1;
        private readonly WaterSurface waterCaustics2;
        public readonly World world;

        public WaterSurfaceManager(World world)
        {
            this.world = world;

            this.waterBottom = new WaterSurface(world: world, texture: SonOfRobinGame.textureByName["water textures/water_bottom"]);
            this.waterCaustics1 = new WaterSurface(world: world, texture: SonOfRobinGame.textureByName["water textures/water_caustics1"]);
            this.waterCaustics2 = new WaterSurface(world: world, texture: SonOfRobinGame.textureByName["water textures/water_caustics2"]);
        }

        public void Update()
        {
            this.waterBottom.Update();
            this.waterCaustics1.Update();
            this.waterCaustics2.Update();
        }

        public void Draw()
        {
            SonOfRobinGame.GfxDev.Clear(waterColor);

            if (!Preferences.highQualityWater) return;

            this.waterBottom.Draw(opacity: 0.7f);

            SonOfRobinGame.SpriteBatch.End();

            BlendState waterBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix, blendState: waterBlend);

            this.waterCaustics1.Draw(opacity: 0.15f);
            this.waterCaustics2.Draw(opacity: 0.15f);

            SonOfRobinGame.SpriteBatch.End();

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix);
        }
    }

    public class WaterSurface
    {
        private readonly World world;
        private readonly Texture2D texture;

        public Vector2 offset;
        public float baseOpacity;

        private readonly int columns;
        private readonly int rows;

        private readonly Tweener tweener;

        public WaterSurface(World world, Texture2D texture)
        {
            this.world = world;
            this.texture = texture;

            this.offset = new Vector2(0, 0);
            this.baseOpacity = 1f;

            this.rows = (int)(this.world.width / this.texture.Width);
            this.columns = (int)(this.world.height / this.texture.Height);

            this.tweener = new Tweener();

            this.SetTweener();
        }

        private void SetTweener()
        {
            Tween tweenOffset = this.tweener.FindTween(target: this, memberName: "offset");

            if (tweenOffset == null || !tweenOffset.IsAlive)
            {
                Vector2 newPos = Vector2.Zero;

                int maxDistance = 150;

                newPos = this.offset + new Vector2(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));

                this.tweener.TweenTo(target: this, expression: waterSurface => waterSurface.offset, toValue: newPos, duration: this.world.random.Next(3, 8), delay: 0)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut)
                    .OnEnd(t => this.SetTweener());
            }

            Tween tweenOpacity = this.tweener.FindTween(target: this, memberName: "baseOpacity");

            if (tweenOpacity == null || !tweenOpacity.IsAlive)
            {
                this.tweener.TweenTo(target: this, expression: waterSurface => waterSurface.baseOpacity, toValue: 0.2f, duration: this.world.random.Next(3, 12), delay: this.world.random.Next(5, 8))
                              .AutoReverse()
                              .Easing(EasingFunctions.SineInOut)
                              .OnEnd(t => this.SetTweener());
            }
        }

        public void Update()
        {
            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(float opacity)
        {
            Rectangle viewRect = this.world.camera.viewRect;

            int offsetX = (int)this.offset.X;
            int offsetY = (int)this.offset.Y;

            Color drawColor = Color.White * opacity * this.baseOpacity;

            int startColumn = (int)((viewRect.X - this.offset.X) / this.texture.Width);
            int startRow = (int)((viewRect.Y - this.offset.Y) / this.texture.Height);
            int endColumn = Math.Max(columns, startColumn + (int)(viewRect.Width / this.texture.Width) + 1);
            int endRow = Math.Max(rows, startRow + (int)(viewRect.Height / this.texture.Height) + 1);

            if (viewRect.X < (startColumn * this.texture.Width) + offsetX) startColumn--;
            if (viewRect.Y < (startRow * this.texture.Height) + offsetY) startRow--;

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    int x = (j * this.texture.Width) + offsetX;
                    int y = (i * this.texture.Height) + offsetY;

                    SonOfRobinGame.SpriteBatch.Draw(this.texture, new Vector2(x, y), drawColor);
                }
            }
        }
    }
}