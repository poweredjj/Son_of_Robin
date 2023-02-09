using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;

namespace SonOfRobin
{
    public class WaterSurfaceCollection
    {
        private readonly WaterSurface waterSurface1;
        private readonly WaterSurface waterSurface2;

        public WaterSurfaceCollection(World world)
        {
            Texture2D texture1 = SonOfRobinGame.textureByName["water textures/water_texture1"];
            Texture2D texture2 = SonOfRobinGame.textureByName["water textures/water_texture2"];

            if (texture1.Width != texture2.Width) throw new ArgumentException("Water texture width mismatch.");
            if (texture1.Height != texture2.Height) throw new ArgumentException("Water texture height mismatch.");

            this.waterSurface1 = new WaterSurface(world: world, texture: texture1);
            this.waterSurface2 = new WaterSurface(world: world, texture: texture2);
        }

        public void Draw()
        {
            this.waterSurface1.Draw(opacity: 1f);
            //this.waterSurface2.Draw(opacity: 0.5f); // TODO enable
        }
    }

    public class WaterSurface
    {
        private readonly Texture2D texture;

        private readonly World world;
        private Point offset;

        private readonly Vector2 segmentSize; // the size of a single texture

        private readonly int columns;
        private readonly int rows;

        private readonly Tweener tweener;

        public WaterSurface(World world, Texture2D texture)
        {
            this.world = world;
            this.texture = texture;

            this.segmentSize = new Vector2(this.texture.Width, this.texture.Height);

            this.offset = Point.Zero;

            this.rows = (int)(this.world.width / segmentSize.X);
            this.columns = (int)(this.world.height / segmentSize.Y);

            this.tweener = new Tweener();

            int maxDistance = 100;

            Point newPos = this.offset + new Point(this.world.random.Next(-maxDistance, maxDistance), this.world.random.Next(-maxDistance, maxDistance));

            //this.tweener.TweenTo(target: this, expression: waterSurface => waterSurface.offset, toValue: newPos, duration: this.world.random.Next(3, 6), delay: 0)
            //       .RepeatForever(repeatDelay: 0.0f)
            //       .AutoReverse()
            //       .Easing(EasingFunctions.QuadraticInOut);
        }

        public void Draw(float opacity)
        {
            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            Rectangle viewRect = this.world.camera.viewRect;

            Color drawColor = Color.White * opacity;

            int startColumn = (int)((viewRect.X - this.offset.X) / segmentSize.X);
            int startRow = (int)((viewRect.Y - this.offset.Y) / segmentSize.Y);
            int endColumn = Math.Max(columns, startColumn + (int)(viewRect.Width / segmentSize.X) + 1);
            int endRow = Math.Max(rows, startRow + (int)(viewRect.Height / segmentSize.Y) + 1);

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    int x = (int)(j * segmentSize.X) + this.offset.X;
                    int y = (int)(i * segmentSize.Y) + this.offset.Y;

                    SonOfRobinGame.SpriteBatch.Draw(this.texture, new Vector2(x, y), drawColor);
                }
            }
        }
    }
}