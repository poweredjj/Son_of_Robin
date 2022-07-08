using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class TileMapProcessScene : Scene
    {
        private readonly int width;
        private readonly int height;
        private readonly int seed;
        public readonly TileMap map;
        private readonly bool showVis;
        private readonly bool showProgressBar;
        private readonly int elementsPerFrame;

        public TileMapProcessScene(TileMap.MapType mapType, int width, int height, int seed, bool showVis = false, bool showProgressBar = true, int elementsPerFrame = 2) :

            base(inputType: InputTypes.Normal, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.width = width;
            this.height = height;
            this.seed = seed;

            this.showVis = showVis;
            this.showProgressBar = showProgressBar;
            this.elementsPerFrame = elementsPerFrame;

            this.map = new TileMap(mapType: mapType, width: this.width, height: this.height, seed: this.seed, showProgressBar: this.showProgressBar);
        }


        public override void Update(GameTime gameTime)
        {
            if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                this.Remove();
                return;
            }

            if (this.map.ReadyToUse) return;

            this.map.ProcessNextGeneratorStep(processCount: this.elementsPerFrame);
        }

        public override void Draw()
        {
            if (!this.showVis && !this.map.ReadyToUse) return;

            SonOfRobinGame.graphicsDevice.Clear(Color.DarkBlue * this.viewParams.drawOpacity);

            Rectangle mapRect = new Rectangle(x: 0, y: 0, width: this.map.width, height: this.map.height);
            Rectangle screenRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);

            double scaleX = (double)screenRect.Width / (double)mapRect.Width;
            double scaleY = (double)screenRect.Height / (double)mapRect.Height;
            double scale = Math.Min(scaleX, scaleY);

            Rectangle scaledMapRect = new Rectangle(x: 0, y: 0, width: (int)(mapRect.Width * scale), height: (int)(mapRect.Height * scale));

            int offsetX = (screenRect.Width - scaledMapRect.Width) / 2;
            int offsetY = (screenRect.Height - scaledMapRect.Height) / 2;

            int tileSize = (int)Math.Ceiling((double)scaledMapRect.Height / (double)this.map.height);
            if (tileSize > 5) tileSize += 2;

            Rectangle destRect = new Rectangle(x: 0, y: 0, width: tileSize, height: tileSize);

            for (int x = 0; x < this.map.width; x++)
            {
                for (int y = 0; y < this.map.height; y++)
                {
                    Texture2D texture = this.map.GetTileTexture(x, y);
                    if (texture == null || texture == SonOfRobinGame.whiteRectangle) continue;

                    destRect.X = (int)(x * scale) + offsetX;
                    destRect.Y = (int)(y * scale) + offsetY;
                    SonOfRobinGame.spriteBatch.Draw(texture: texture, destinationRectangle: destRect, color: Color.White * this.viewParams.drawOpacity);
                }
            }
        }
    }
}
