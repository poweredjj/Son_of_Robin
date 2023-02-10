using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;

namespace SonOfRobin
{
    public class WaterSurfaceManager
    {
        private readonly WaterSurface waterSurface1;
        private readonly WaterSurface waterSurface2;

        public WaterSurfaceManager(World world)
        {
            Texture2D texture1 = SonOfRobinGame.textureByName["water textures/water_texture1"];
            Texture2D texture2 = SonOfRobinGame.textureByName["water textures/water_texture2"];

            this.waterSurface1 = new WaterSurface(world: world, texture: texture1);
            this.waterSurface2 = new WaterSurface(world: world, texture: texture2);
        }

        public void Draw()
        {
            this.waterSurface1.Draw(opacity: 1f);
            this.waterSurface2.Draw(opacity: 0.5f);
        }
    }

    public class WaterSurface
    {
        private readonly Texture2D texture;

        private readonly World world;

        public Vector2 offset;

        private readonly int columns;
        private readonly int rows;

        private readonly Tweener tweener;

        public WaterSurface(World world, Texture2D texture)
        {
            this.world = world;
            this.texture = texture;

            this.offset = new Vector2(0, 0);

            this.rows = (int)(this.world.width / this.texture.Width);
            this.columns = (int)(this.world.height / this.texture.Height);

            this.tweener = new Tweener();

            this.SetTweener();
        }

        private void SetTweener()
        {
            Tween tween = this.tweener.FindTween(target: this, memberName: "offset");

            if (tween == null || !tween.IsAlive)
            {
                Vector2 newPos = Vector2.Zero;

                int maxDistance = 200;

                newPos = this.offset + new Vector2(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));

                this.tweener.TweenTo(target: this, expression: waterSurface => waterSurface.offset, toValue: newPos, duration: this.world.random.Next(3, 8), delay: 0)
                    .AutoReverse()
                    .Easing(EasingFunctions.QuadraticInOut)
                    .OnEnd(t => this.SetTweener());
            }
        }

        public void Draw(float opacity)
        {
            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            Rectangle viewRect = this.world.camera.viewRect;

            int offsetX = (int)this.offset.X;
            int offsetY = (int)this.offset.Y;

            Color drawColor = Color.White * opacity;

            int startColumn = (int)((viewRect.X - this.offset.X) / this.texture.Width);
            int startRow = (int)((viewRect.Y - this.offset.Y) / this.texture.Height);
            int endColumn = Math.Max(columns, startColumn + (int)(viewRect.Width / this.texture.Width) + 1);
            int endRow = Math.Max(rows, startRow + (int)(viewRect.Height / this.texture.Height) + 1);

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    int x = (int)(j * this.texture.Width) + offsetX;
                    int y = (int)(i * this.texture.Height) + offsetY;

                    SonOfRobinGame.SpriteBatch.Draw(this.texture, new Vector2(x, y), drawColor);
                }
            }
        }
    }
}