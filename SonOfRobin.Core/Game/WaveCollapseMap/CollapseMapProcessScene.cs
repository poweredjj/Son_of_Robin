using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class CollapseMapProcessScene : Scene
    {
        public enum MapType { DefaultOverworld, Test1, Test2 }

        private readonly MapType mapType;
        private readonly int width;
        private readonly int height;
        private readonly int seed;
        private readonly Random random;
        public readonly WaveCollapseMap map;
        private readonly bool showVis;
        private readonly bool showProgressBar;
        private readonly int elementsPerFrame;

        public CollapseMapProcessScene(MapType mapType, int width, int height, int seed, bool showVis = false, bool showProgressBar = true, int elementsPerFrame = 2) :

            base(inputType: InputTypes.Normal, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.mapType = mapType;
            this.width = width;
            this.height = height;
            this.seed = seed;

            this.showVis = showVis;
            this.showProgressBar = showProgressBar;
            this.elementsPerFrame = elementsPerFrame;

            this.map = this.CreateMap();
            this.random = this.map.random;
            this.CollapseSelectedElements();
        }

        private WaveCollapseMap CreateMap()
        {

            Dictionary<WaveCollapseMap.Name, float> limitedPercentageByName = null;
            bool addBorder = false;
            WaveCollapseMap.Name borderName = WaveCollapseMap.Name.Empty;
            List<WaveCollapseMap.Name> nameWhiteList = null;
            List<WaveCollapseMap.Name> nameBlackList = null;
            bool correctErrors = false;

            switch (this.mapType)
            {
                case MapType.DefaultOverworld:
                    addBorder = true;
                    borderName = WaveCollapseMap.Name.WaterDeep;

                    limitedPercentageByName = new Dictionary<WaveCollapseMap.Name, float> {
                    { WaveCollapseMap.Name.MountainsLow, 0.04f },
                    { WaveCollapseMap.Name.MountainsMedium, 0.03f },
                    { WaveCollapseMap.Name.MountainsHigh, 0.03f },
                    { WaveCollapseMap.Name.Lava, 0.001f },
                    { WaveCollapseMap.Name.Sand, 0.25f },
                    { WaveCollapseMap.Name.WaterShallow, 0.06f },
                    { WaveCollapseMap.Name.WaterMedium, 0.22f },
                    { WaveCollapseMap.Name.WaterDeep, 0.22f },
                };

                    break;

                case MapType.Test1:
                    addBorder = true;
                    borderName = WaveCollapseMap.Name.WaterDeep;

                    nameWhiteList = new List<WaveCollapseMap.Name> { WaveCollapseMap.Name.WaterDeep, WaveCollapseMap.Name.WaterMedium, WaveCollapseMap.Name.Sand, WaveCollapseMap.Name.WaterShallow, WaveCollapseMap.Name.Dirt };

                    break;

                case MapType.Test2:

                    break;

                default:
                    throw new ArgumentException($"Unsupported mapType: {mapType}.");
            }

            return new WaveCollapseMap(width: this.width, height: this.height, seed: this.seed, addBorder: addBorder, borderName: borderName, nameWhiteList: nameWhiteList, nameBlackList: nameBlackList, correctErrors: correctErrors, limitedPercentageByName: limitedPercentageByName, showProgressBar: this.showProgressBar);
        }

        private void CollapseSelectedElements()
        {
            switch (this.mapType)
            {
                case MapType.DefaultOverworld:
                    this.map.CollapseRandomElements(count: this.random.Next(2, 4), nearbyElementCount: this.random.Next(1, 3), nameList: new List<WaveCollapseMap.Name> { WaveCollapseMap.Name.Lava });
                    this.map.CollapseRandomElements(count: this.random.Next(2, 3), nearbyElementCount: this.random.Next(10, 25), nameList: new List<WaveCollapseMap.Name> { WaveCollapseMap.Name.WaterShallow });
                    this.map.CollapseRandomElements(count: this.random.Next(1, 2), nearbyElementCount: this.random.Next(30, 70), nameList: new List<WaveCollapseMap.Name> { WaveCollapseMap.Name.Sand });
                    this.map.CollapseRandomElements(count: this.random.Next(5, 10), nearbyElementCount: this.random.Next(100, 250), nameList: new List<WaveCollapseMap.Name> { WaveCollapseMap.Name.Dirt });
                    this.map.CollapseRandomElements(count: this.random.Next(5, 8), nearbyElementCount: this.random.Next(4, 12), nameList: new List<WaveCollapseMap.Name> { WaveCollapseMap.Name.Grass, WaveCollapseMap.Name.Dirt });

                    break;

                case MapType.Test1:
                    break;

                case MapType.Test2:
                    break;

                default:
                    throw new ArgumentException($"Unsupported mapType: {mapType}.");
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                this.Remove();
                return;
            }

            if (this.map.IsFullyCollapsed) return;

            this.map.ProcessNextBatchInteractively(elementsPerFrame: this.elementsPerFrame);
        }

        public override void Draw()
        {
            if (!this.showVis && !this.map.IsFullyCollapsed) return;

            SonOfRobinGame.graphicsDevice.Clear(Color.White * this.viewParams.drawOpacity);

            Rectangle mapRect = new Rectangle(x: 0, y: 0, width: this.map.width, height: this.map.height);
            Rectangle screenRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);

            double scaleX = (double)screenRect.Width / (double)mapRect.Width;
            double scaleY = (double)screenRect.Height / (double)mapRect.Height;
            double scale = Math.Min(scaleX, scaleY);

            Rectangle scaledMapRect = new Rectangle(x: 0, y: 0, width: (int)(mapRect.Width * scale), height: (int)(mapRect.Height * scale));

            int offsetX = (screenRect.Width - scaledMapRect.Width) / 2;
            int offsetY = (screenRect.Height - scaledMapRect.Height) / 2;

            int tileSize = (int)Math.Ceiling((double)scaledMapRect.Height / (double)this.map.height);

            Rectangle destRect = new Rectangle(x: 0, y: 0, width: tileSize, height: tileSize);

            for (int x = 0; x < this.map.width; x++)
            {
                for (int y = 0; y < this.map.height; y++)
                {
                    Texture2D texture = this.map.GetElementTexture(x, y);

                    destRect.X = (int)(x * scale) + offsetX;
                    destRect.Y = (int)(y * scale) + offsetY;
                    SonOfRobinGame.spriteBatch.Draw(texture: texture, destinationRectangle: destRect, color: Color.White * this.viewParams.drawOpacity);
                }
            }
        }
    }
}
