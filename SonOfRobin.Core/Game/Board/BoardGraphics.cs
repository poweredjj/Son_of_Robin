using BigGustave;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class BoardGraphics
    {
        private readonly Cell cell;
        public Texture2D Texture { get; private set; }

        private Color textureSimulationColor; // texture "preview" for display when the texture is unloaded
        public Color TextureSimulationColor
        {
            get
            {
                if (this.textureSimulationColor == null)
                {
                    this.TextureSimulationColor = CreatePixel(grid: this.cell.grid, x: this.cell.xCenter, y: this.cell.yCenter);
                }

                return this.textureSimulationColor;
            }

            private set
            {
                this.textureSimulationColor = value;
            }
        }

        private readonly string templatePath;
        private bool savedToDisk;

        public static readonly Dictionary<Colors, Color> colorsByName = GetColorsByName();
        public static readonly Dictionary<Colors, List<byte>> colorsByHeight = GetColorsByHeight();
        public static readonly Dictionary<Colors, List<byte>> colorsByHumidity = GetColorsByHumidity();

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

        public BoardGraphics(Grid grid, Cell cell)
        {
            this.cell = cell;
            this.templatePath = Path.Combine(grid.gridTemplate.templatePath, $"background_{cell.cellNoX}_{cell.cellNoY}.png");
            this.savedToDisk = File.Exists(this.templatePath);
        }

        public void LoadTexture()
        {
            if (this.Texture != null) return;

            this.Texture = GfxConverter.LoadTextureFromPNG(this.templatePath);
            if (this.Texture == null)
            {
                this.CreateBitmapFromTerrain();
                this.Texture = GfxConverter.LoadTextureFromPNG(this.templatePath); // trying to create texture on disk first...
                if (this.Texture == null)
                {
                    Color[,] colorGrid = this.CreateBitmapFromTerrain(); // ...if this fails (disk error, locked file, etc.), get texture data directly
                    this.Texture = GfxConverter.Convert2DArrayToTexture(colorGrid);
                }
            }

            this.cell.grid.loadedTexturesCount++;
        }

        public void ReplaceTexture(Texture2D texture, Color textureSimulationColor)
        {
            if (this.Texture != null) this.Texture.Dispose();
            else this.cell.grid.loadedTexturesCount++;
            this.Texture = texture;
            this.TextureSimulationColor = textureSimulationColor;
        }

        public void UnloadTexture()
        {
            if (this.Texture == null) return;

            this.Texture.Dispose();
            this.Texture = null;
            this.cell.grid.loadedTexturesCount--;
        }

        public void CreateAndSavePngTemplate()
        {
            if (this.savedToDisk) return;

            this.CreateBitmapFromTerrain();
            this.savedToDisk = true;
        }

        public static Texture2D CreateEntireMapTexture(Grid grid, int width, int height, float multiplier)
        {
            var colorArray = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorArray[(y * width) + x] = CreatePixel(grid: grid, x: (int)(x / multiplier), y: (int)(y / multiplier));
                }
            }

            Texture2D texture = new Texture2D(SonOfRobinGame.graphicsDevice, width, height);
            texture.SetData(colorArray);
            return texture;
        }

        private Color[,] CreateBitmapFromTerrainTest() // for visual testing without upscaling turned on
        {
            // can be run in parallel, because it does not use graphicsDevice

            // creating base 2D color array with rgb color data

            int sourceWidth = this.cell.width / this.cell.grid.resDivider;
            int sourceHeight = this.cell.height / this.cell.grid.resDivider;
            int resDivider = this.cell.grid.resDivider;

            Color[,] smallColorGrid = new Color[sourceWidth, sourceHeight];

            for (int localX = 0; localX < sourceWidth + 0; localX++)
            {
                for (int localY = 0; localY < sourceHeight + 0; localY++)
                {
                    int worldSpaceX = this.cell.xMin + (localX * resDivider);
                    int worldSpaceY = this.cell.yMin + (localY * resDivider);

                    worldSpaceX = Math.Min(Math.Max(worldSpaceX, 0), this.cell.world.width - 1);
                    worldSpaceY = Math.Min(Math.Max(worldSpaceY, 0), this.cell.world.height - 1);

                    smallColorGrid[localX, localY] = CreatePixel(grid: this.cell.grid, x: worldSpaceX, y: worldSpaceY);
                }
            }

            return smallColorGrid;
        }

        private Color[,] CreateBitmapFromTerrain()
        {
            // can be run in parallel, because it does not use graphicsDevice

            // creating base 2D color array with rgb color data

            int sourceWidth = this.cell.width / this.cell.grid.resDivider;
            int sourceHeight = this.cell.height / this.cell.grid.resDivider;
            int resDivider = this.cell.grid.resDivider;

            Color[,] smallColorGrid = new Color[sourceWidth, sourceHeight];

            for (int localX = 0; localX < sourceWidth + 0; localX++)
            {
                for (int localY = 0; localY < sourceHeight + 0; localY++)
                {
                    smallColorGrid[localX, localY] = CreatePixel(grid: this.cell.grid, x: this.cell.xMin + (localX * resDivider), y: this.cell.yMin + (localY * resDivider));
                }
            }

            // the upscaling method is used here directly, to interpolate edges correctly (map data from other cells is needed)

            // upscaling color grid

            int resizeFactor = BoardTextureUpscaler3x.resizeFactor;
            int targetWidth = sourceWidth * resizeFactor;
            int targetHeight = sourceHeight * resizeFactor;

            Color[,] upscaledColorGrid = new Color[targetWidth, targetHeight];

            // filling upscaled grid

            List<Point> edgePointList = new List<Point>();

            for (int localY = 0; localY < sourceHeight; localY++)
            {
                for (int localX = 0; localX < sourceWidth; localX++)
                {
                    try
                    {
                        BoardTextureUpscaler3x.Upscale3x3Grid(source: smallColorGrid, target: upscaledColorGrid, sourceOffsetX: localX - 1, sourceOffsetY: localY - 1, targetOffsetX: localX * resizeFactor, targetOffsetY: localY * resizeFactor);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        edgePointList.Add(new Point(localX, localY)); // pixels outside the edge will not be found - adding to edge list
                    }
                }
            }

            // filling edges

            Color[,] workingGrid3x3 = new Color[3, 3]; // working grid is needed, because the edges are missing and using sourceOffset will not work

            foreach (Point point in edgePointList)
            {
                for (int yOffset = -1; yOffset < 2; yOffset++)
                {
                    for (int xOffset = -1; xOffset < 2; xOffset++)
                    {
                        int worldSpaceX = this.cell.xMin + ((point.X + xOffset) * resDivider);
                        int worldSpaceY = this.cell.yMin + ((point.Y + yOffset) * resDivider);

                        try
                        {
                            workingGrid3x3[xOffset + 1, yOffset + 1] = CreatePixel(grid: this.cell.grid, x: worldSpaceX, y: worldSpaceY);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // pixel outside world bounds - inserting the nearest correct position
                            workingGrid3x3[xOffset + 1, yOffset + 1] = smallColorGrid[point.X, point.Y];
                        }
                    }
                }

                BoardTextureUpscaler3x.Upscale3x3Grid(source: workingGrid3x3, target: upscaledColorGrid, targetOffsetX: point.X * resizeFactor, targetOffsetY: point.Y * resizeFactor);
            }

            // putting upscaled color grid into PngBuilder

            var builder = PngBuilder.Create(width: targetWidth, height: targetHeight, hasAlphaChannel: true);

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    Color pixel = upscaledColorGrid[x, y];
                    builder.SetPixel(pixel.R, pixel.G, pixel.B, x, y);
                }
            }

            // saving PngBuilder to file

            using (var memoryStream = new MemoryStream())
            {
                builder.Save(memoryStream);

                try
                {
                    FileReaderWriter.SaveMemoryStream(memoryStream: memoryStream, this.templatePath);
                }
                catch (IOException)
                {
                    // write error
                }
            }

            return upscaledColorGrid;
        }

        private static Color CreatePixel(Grid grid, int x, int y)
        {
            byte pixelHeight = grid.GetFieldValue(terrainName: Terrain.Name.Height, x: x, y: y);
            byte pixelHumidity = grid.GetFieldValue(terrainName: Terrain.Name.Humidity, x: x, y: y);
            byte pixelBiome = grid.GetFieldValue(terrainName: Terrain.Name.Biome, x: x, y: y);
            Dictionary<ExtBoardProps.ExtPropName, bool> extDataValDict = grid.GetExtValueDict(x, y);

            Color pixel = new Color();

            foreach (var kvp in colorsByHeight)
            {
                if (kvp.Value[1] >= pixelHeight && pixelHeight >= kvp.Value[0])
                {
                    pixel = colorsByName[kvp.Key];
                    break;
                }
            }

            if (pixel.A < 255)
            {
                foreach (var kvp in colorsByHumidity)
                {
                    if (kvp.Value[1] >= pixelHumidity && pixelHumidity >= kvp.Value[0])
                    {
                        pixel = Blend2Colors(bottomColor: colorsByName[kvp.Key], topColor: pixel);
                        break;
                    }
                }
            }

            if (pixelBiome >= Terrain.biomeMin)
            {
                if (extDataValDict[ExtBoardProps.ExtPropName.BiomeSwamp])
                {
                    float pixelMultiplier = 0.6f;

                    pixel = new Color(
                        r: (byte)(pixel.R * pixelMultiplier),
                        g: (byte)(pixel.G * pixelMultiplier),
                        b: (byte)(pixel.B * pixelMultiplier),
                        alpha: pixel.A);

                    Color biomeColor = new Color(83, 97, 55, 128);
                    pixel = Blend2Colors(bottomColor: pixel, topColor: biomeColor);
                }
            }

            // if (extDataValDict[ExtBoardProps.ExtPropName.OuterBeach]) pixel = Blend2Colors(bottomColor: pixel, topColor: Color.Cyan * 0.8f); // for testing
            // if (extDataValDict[ExtBoardProps.ExtPropName.Sea]) pixel = Blend2Colors(bottomColor: pixel, topColor: Color.Red * 0.8f); // for testing

            return pixel;
        }

        public static Color Blend2Colors(Color bottomColor, Color topColor)
        {
            if (topColor.A == 255) return topColor;

            float topAlpha = Convert.ToSingle(topColor.A) / 255;
            float bottomAlpha = 1f - topAlpha;

            return new Color(
                (byte)((bottomColor.R * bottomAlpha) + (topColor.R * topAlpha)),
                (byte)((bottomColor.G * bottomAlpha) + (topColor.G * topAlpha)),
                (byte)((bottomColor.B * bottomAlpha) + (topColor.B * topAlpha)),
                (byte)255);
        }

        private static Dictionary<Colors, Color> GetColorsByName()
        {
            var colorsByName = new Dictionary<Colors, Color>()
            {
                // height definitions
                { Colors.WaterDeep, new Color(11,46,176,255) },
                { Colors.WaterMedium, new Color(35,78,207,255) },
                { Colors.WaterShallow, new Color(65,105,225,255) },
                { Colors.Beach1, new Color(214,199,133,255) },
                { Colors.Beach2, new Color(214,199,133,128) },
                { Colors.Ground, new Color(0,0,0,0) },
                { Colors.Mountains1, new Color(180,180,180,128) },
                { Colors.Mountains2, new Color(180,180,180,255) },
                { Colors.Mountains3, new Color(209,209,209,255) },
                { Colors.Mountains4, new Color(225,225,225,255) },
                { Colors.VolcanoEdge, new Color(64,64,64,255) },
                { Colors.VolcanoInside1, new Color(255,81,0,255) },
                { Colors.VolcanoInside2, new Color(255,179,0,255) },

                // humidity definitions
                { Colors.Sand, new Color(227,210,102,255) },
                { Colors.GroundBad, new Color(207,167,58,255) },
                { Colors.GroundGood, new Color(173,128,54,255) },
                { Colors.GrassBad, new Color(141,181,67,255) },
                { Colors.GrassGood, new Color(78,186,0,255) },
            };

            return colorsByName;
        }

        private static Dictionary<Colors, List<byte>> GetColorsByHeight()
        {
            return new Dictionary<Colors, List<byte>>() {
                { Colors.WaterDeep, new List<byte>(){0,(byte)(Terrain.waterLevelMax / 3)} },
                { Colors.WaterMedium, new List<byte>(){ (byte)(Terrain.waterLevelMax / 3), (byte)(Terrain.waterLevelMax / 3 * 2)} },
                { Colors.WaterShallow, new List<byte>(){ (byte)(Terrain.waterLevelMax / 3 * 2), Terrain.waterLevelMax} },
                { Colors.Beach1, new List<byte>(){Terrain.waterLevelMax, 100} },
                { Colors.Beach2, new List<byte>(){100, 105} },
                { Colors.Ground, new List<byte>(){105, 160} },
                { Colors.Mountains1, new List<byte>(){160, 163} },
                { Colors.Mountains2, new List<byte>(){163, 178} },
                { Colors.Mountains3, new List<byte>(){178, 194} },
                { Colors.Mountains4, new List<byte>(){194, Terrain.volcanoEdgeMin} },
                { Colors.VolcanoEdge, new List<byte>(){Terrain.volcanoEdgeMin, Terrain.lavaMin} },
                { Colors.VolcanoInside1, new List<byte>(){Terrain.lavaMin, 225} },
                { Colors.VolcanoInside2, new List<byte>(){225, 255} },
            };
        }

        private static Dictionary<Colors, List<byte>> GetColorsByHumidity()
        {
            return new Dictionary<Colors, List<byte>>() {
                { Colors.Sand, new List<byte>(){0, 80} },
                { Colors.GroundBad, new List<byte>(){80, 115} },
                { Colors.GroundGood, new List<byte>(){115, 150} },
                { Colors.GrassBad, new List<byte>(){150, 200} },
                { Colors.GrassGood, new List<byte>(){200, 255} },
            };
        }
    }
}