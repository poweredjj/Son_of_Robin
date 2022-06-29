using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace SonOfRobin
{
    public class BoardGraphics
    {
        public bool creationInProgress;
        public readonly World world;
        private readonly Cell cell;
        public Texture2D texture;
        private readonly string templatePath;
        public readonly bool templateExists;
        Color[] tempColorArray;

        public static Dictionary<Colors, Color> colorsByName = GetColorsByName();
        private static Dictionary<Colors, List<byte>> colorsByHeight = GetColorsByHeight();
        private static Dictionary<Colors, List<byte>> colorsByHumidity = GetColorsByHumidity();

        public enum Colors
        {
            WaterDeep,
            WaterMedium,
            WaterShallow,
            Beach1,
            Beach2,
            Ground,
            Mountains1,
            Mountains2,
            Mountains3,
            Mountains4,
            VolcanoEdge,
            VolcanoInside1,
            VolcanoInside2,
            Sand,
            GroundBad,
            GroundGood,
            GrassBad,
            GrassGood
        }

        public BoardGraphics(World world, Cell cell)
        {
            this.creationInProgress = true;
            this.world = world;
            this.cell = cell;
            this.templatePath = Path.Combine(this.world.templatePath, $"background_{cell.cellNoX}_{cell.cellNoY}.png");

            this.templateExists = (File.Exists(this.templatePath));

            if (!this.templateExists) this.CreateColorArrayFromTerrain();
        }

        public void CompleteCreation()
        {
            if (this.templateExists)
            {
                bool textureLoaded = this.LoadTemplate();
                if (!textureLoaded)
                {
                    this.CreateColorArrayFromTerrain();
                    this.CreateTextureFromColorArray();
                    this.SaveTemplate();
                }
            }
            else
            {
                this.CreateTextureFromColorArray();
                this.SaveTemplate();
            }

            this.creationInProgress = false;
        }

        private bool LoadTemplate()
        {
            var loadedTexture = GfxConverter.LoadTextureFromPNG(this.templatePath);
            if (loadedTexture is null) { return false; }

            this.texture = loadedTexture;
            return true;
        }

        private void SaveTemplate()
        {
            GfxConverter.SaveTextureAsPNG(texture: this.texture, filename: this.templatePath);
        }

        private void CreateColorArrayFromTerrain()
        {

            this.tempColorArray = new Color[this.cell.width * this.cell.height];

            var mapDataHeight = this.cell.terrainByName[TerrainName.Height].mapData;
            var mapDataHumidity = this.cell.terrainByName[TerrainName.Humidity].mapData;

            for (int y = 0; y < this.cell.height; y++)
            {
               for (int x = 0; x < this.cell.width; x++)
               {
                   Color pixel = new Color();

                   foreach (var kvp in colorsByHeight)
                   {
                       if (kvp.Value[1] >= mapDataHeight[x, y] && mapDataHeight[x, y] >= kvp.Value[0])
                       {
                           pixel = colorsByName[kvp.Key];
                           break;
                       }
                   }

                   if (pixel.A < 255)
                   {
                       foreach (var kvp in colorsByHumidity)
                       {
                           if (kvp.Value[1] >= mapDataHumidity[x, y] && mapDataHumidity[x, y] >= kvp.Value[0])
                           {
                               pixel = Blend2Colors(colorsByName[kvp.Key], pixel);
                               break;
                           }
                       }
                   }

                   tempColorArray[(y * this.cell.width) + x] = pixel;
               }
           }


        }

        public void CreateTextureFromColorArray()
        {
            this.texture = new Texture2D(SonOfRobinGame.graphicsDevice, this.cell.width, this.cell.height);
            this.texture.SetData(this.tempColorArray);
            this.tempColorArray = null;
        }



        public static Color Blend2Colors(Color color1, Color color2)
        {
            if (color1.A == 0) { return color2; }

            var alpha2 = Convert.ToSingle(color2.A) / 255;
            var alpha1 = 1 - alpha2;

            var newColor = new Color(
                Convert.ToByte((((float)color1.R * alpha1) + (float)color2.R * alpha2)),
                Convert.ToByte((((float)color1.G * alpha1) + (float)color2.G * alpha2)),
                Convert.ToByte((((float)color1.B * alpha1) + (float)color2.B * alpha2)),
                (byte)255);

            return newColor;
        }

        private static Dictionary<Colors, Color> GetColorsByName()
        {
            var colorsByName = new Dictionary<Colors, Color>()
            {
                // height definitions
                {Colors.WaterDeep, new Color(11,46,176,255)},
                {Colors.WaterMedium, new Color(35,78,207,255)},
                {Colors.WaterShallow, new Color(65,105,225,255)},
                {Colors.Beach1, new Color(214,199,133,255)},
                {Colors.Beach2, new Color(214,199,133,128)},
                {Colors.Ground, new Color(0,0,0,0)},
                {Colors.Mountains1, new Color(180,180,180,128)},
                {Colors.Mountains2, new Color(180,180,180,255)},
                {Colors.Mountains3, new Color(209,209,209,255)},
                {Colors.Mountains4, new Color(225,225,225,255)},
                {Colors.VolcanoEdge, new Color(64,64,64,255)},
                {Colors.VolcanoInside1, new Color(255,81,0,255)},
                {Colors.VolcanoInside2, new Color(255,179,0,255)},

                // humidity definitions
                {Colors.Sand, new Color(227,210,102,255)},
                {Colors.GroundBad, new Color(207,167,58,255)},
                {Colors.GroundGood, new Color(173,128,54,255)},
                {Colors.GrassBad, new Color(141,181,67,255)},
                {Colors.GrassGood, new Color(78,186,0,255)},
            };

            return colorsByName;
        }

        private static Dictionary<Colors, List<byte>> GetColorsByHeight()
        {
            return new Dictionary<Colors, List<byte>>() {
                {Colors.WaterDeep, new List<byte>(){0,Convert.ToByte(Terrain.waterLevelMax / 3)}},
                {Colors.WaterMedium, new List<byte>(){Convert.ToByte(Terrain.waterLevelMax / 3), Convert.ToByte((Terrain.waterLevelMax / 3)*2)}},
                {Colors.WaterShallow, new List<byte>(){Convert.ToByte((Terrain.waterLevelMax / 3)*2), Terrain.waterLevelMax}},
                {Colors.Beach1, new List<byte>(){Terrain.waterLevelMax, 100}},
                {Colors.Beach2, new List<byte>(){100, 105}},
                {Colors.Ground, new List<byte>(){105, 160}},
                {Colors.Mountains1, new List<byte>(){160, 163}},
                {Colors.Mountains2, new List<byte>(){163, 178}},
                {Colors.Mountains3, new List<byte>(){178, 194}},
                {Colors.Mountains4, new List<byte>(){194, Terrain.volcanoLevelMin}},
                {Colors.VolcanoEdge, new List<byte>(){Terrain.volcanoLevelMin, 215}},
                {Colors.VolcanoInside1, new List<byte>(){215, 225}},
                {Colors.VolcanoInside2, new List<byte>(){225, 255}},
            }; ;
        }

        private static Dictionary<Colors, List<byte>> GetColorsByHumidity()
        {
            return new Dictionary<Colors, List<byte>>() {
                {Colors.Sand, new List<byte>(){0, 80}},
                {Colors.GroundBad, new List<byte>(){80, 115}},
                {Colors.GroundGood, new List<byte>(){115, 150}},
                {Colors.GrassBad, new List<byte>(){150, 200}},
                {Colors.GrassGood, new List<byte>(){200, 255}},
            };
        }

    }
}
