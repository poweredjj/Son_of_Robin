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
        public static readonly Dictionary<RepeatingPattern.Name, List<byte>> patternNamesByHeight = GetColorsByHeight();
        public static readonly Dictionary<RepeatingPattern.Name, List<byte>> patternNamesByHumidity = GetColorsByHumidity();

        private readonly Cell cell;
        public Texture2D Texture { get; private set; }

        private bool textureSimulationCalculated;
        private Color textureSimulationColor; // texture "preview" for display when the texture is unloaded

        private readonly string templatePath;
        private bool savedToDisk;

        public BoardGraphics(Grid grid, Cell cell)
        {
            this.cell = cell;
            this.templatePath = Path.Combine(grid.gridTemplate.templatePath, $"background_{cell.cellNoX}_{cell.cellNoY}.png");
            this.savedToDisk = File.Exists(this.templatePath);
            this.textureSimulationCalculated = false;
        }

        public Color TextureSimulationColor
        {
            get
            {
                if (!this.textureSimulationCalculated)
                {
                    this.TextureSimulationColor = CreateTexturedPixel(grid: this.cell.grid, x: this.cell.xCenter, y: this.cell.yCenter);
                }

                return this.textureSimulationColor;
            }

            private set
            {
                this.textureSimulationColor = value;
                this.textureSimulationCalculated = true;
            }
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
            this.Texture = texture;
            if (textureSimulationColor != null) this.TextureSimulationColor = textureSimulationColor;
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
                    colorArray[(y * width) + x] = CreateTexturedPixel(grid: grid, x: (int)(x / multiplier), y: (int)(y / multiplier));
                }
            }

            Texture2D texture = new Texture2D(SonOfRobinGame.GfxDev, width, height);
            texture.SetData(colorArray);
            return texture;
        }

        private Color[,] CreateBitmapFromTerrain()
        {
            // can be run in parallel, because it does not use graphicsDevice

            // creating base 2D color array with rgb color data

            int sourceWidth = this.cell.width / this.cell.grid.resDivider;
            int sourceHeight = this.cell.height / this.cell.grid.resDivider;
            int resDivider = this.cell.grid.resDivider;

            RepeatingPattern.Name[,] smallGrid = new RepeatingPattern.Name[sourceWidth, sourceHeight];

            for (int localX = 0; localX < sourceWidth + 0; localX++)
            {
                for (int localY = 0; localY < sourceHeight + 0; localY++)
                {
                    smallGrid[localX, localY] = FindPatternNameForPixel(grid: this.cell.grid, x: this.cell.xMin + (localX * resDivider), y: this.cell.yMin + (localY * resDivider));
                }
            }

            // the upscaling method is used here directly, to interpolate edges correctly (map data from other cells is needed)

            // upscaling color grid

            int resizeFactor = BoardTextureUpscaler3x.resizeFactor;
            int targetWidth = sourceWidth * resizeFactor;
            int targetHeight = sourceHeight * resizeFactor;

            RepeatingPattern.Name[,] upscaledGrid = new RepeatingPattern.Name[targetWidth, targetHeight];

            // filling upscaled grid

            List<Point> edgePointList = new List<Point>();

            for (int localY = 0; localY < sourceHeight; localY++)
            {
                for (int localX = 0; localX < sourceWidth; localX++)
                {
                    try
                    {
                        BoardTextureUpscaler3x.Upscale3x3PatternNameGrid(source: smallGrid, target: upscaledGrid, sourceOffsetX: localX - 1, sourceOffsetY: localY - 1, targetOffsetX: localX * resizeFactor, targetOffsetY: localY * resizeFactor);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        edgePointList.Add(new Point(localX, localY)); // pixels outside the edge will not be found - adding to edge list
                    }
                }
            }

            // filling edges

            RepeatingPattern.Name[,] workingGrid3x3 = new RepeatingPattern.Name[3, 3]; // working grid is needed, because the edges are missing and using sourceOffset will not work

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
                            workingGrid3x3[xOffset + 1, yOffset + 1] = FindPatternNameForPixel(grid: this.cell.grid, x: worldSpaceX, y: worldSpaceY);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // pixel outside world bounds - inserting the nearest correct position
                            workingGrid3x3[xOffset + 1, yOffset + 1] = smallGrid[point.X, point.Y];
                        }
                    }
                }

                BoardTextureUpscaler3x.Upscale3x3PatternNameGrid(source: workingGrid3x3, target: upscaledGrid, targetOffsetX: point.X * resizeFactor, targetOffsetY: point.Y * resizeFactor);
            }

            // putting upscaled color grid into PngBuilder

            Color[,] upscaledColorGrid = new Color[targetWidth, targetHeight];

            var builder = PngBuilder.Create(width: targetWidth, height: targetHeight, hasAlphaChannel: true);

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    RepeatingPattern.Name patternName = upscaledGrid[x, y];

                    Color pixel = RepeatingPattern.patternDict[patternName].GetValue(
                        x: this.cell.xMin + (x * resDivider / BoardTextureUpscaler3x.resizeFactor),
                        y: this.cell.yMin + (y * resDivider / BoardTextureUpscaler3x.resizeFactor));

                    upscaledColorGrid[x, y] = pixel;

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

        private static RepeatingPattern.Name FindPatternNameForPixel(Grid grid, int x, int y)
        {
            byte pixelHeight = grid.GetFieldValue(terrainName: Terrain.Name.Height, x: x, y: y);
            byte pixelHumidity = grid.GetFieldValue(terrainName: Terrain.Name.Humidity, x: x, y: y);
            byte pixelBiome = grid.GetFieldValue(terrainName: Terrain.Name.Biome, x: x, y: y);
            Dictionary<ExtBoardProps.Name, bool> extDataValDict = grid.GetExtValueDict(x, y);

            RepeatingPattern.Name patternName = RepeatingPattern.Name.white;
            bool heightPatternFound = false;

            foreach (var kvp in patternNamesByHeight)
            {
                if (kvp.Value[1] >= pixelHeight && pixelHeight >= kvp.Value[0])
                {
                    patternName = kvp.Key;
                    heightPatternFound = true;
                    break;
                }
            }

            if (!heightPatternFound)
            {
                foreach (var kvp in patternNamesByHumidity)
                {
                    if (kvp.Value[1] >= pixelHumidity && pixelHumidity >= kvp.Value[0])
                    {
                        patternName = kvp.Key;
                        break;
                    }
                }
            }

            if (pixelBiome >= Terrain.biomeMin)
            {
                if (extDataValDict[ExtBoardProps.Name.BiomeSwamp]) patternName = RepeatingPattern.Name.swamp;

                // future biome colors should be defined here
            }

            // if (extDataValDict[ExtBoardProps.ExtPropName.OuterBeach]) pixel = Blend2Colors(bottomColor: pixel, topColor: Color.Cyan * 0.8f); // for testing
            // if (extDataValDict[ExtBoardProps.ExtPropName.Sea]) pixel = Blend2Colors(bottomColor: pixel, topColor: Color.Red * 0.8f); // for testing
            // if (extDataValDict[ExtBoardProps.ExtPropName.BiomeSwamp]) pixel = Blend2Colors(bottomColor: pixel, topColor: Color.Green * 0.8f); // for testing

            return patternName;
        }

        public static Color CreateTexturedPixel(Grid grid, int x, int y)
        {
            RepeatingPattern.Name patternName = FindPatternNameForPixel(grid: grid, x: x, y: y);
            return RepeatingPattern.patternDict[patternName].GetValue(x, y);
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

        private static Dictionary<RepeatingPattern.Name, List<byte>> GetColorsByHeight()
        {
            return new Dictionary<RepeatingPattern.Name, List<byte>>() {
                { RepeatingPattern.Name.water_deep, new List<byte>(){0,(byte)(Terrain.waterLevelMax / 3)} },
                { RepeatingPattern.Name.water_medium, new List<byte>(){ (byte)(Terrain.waterLevelMax / 3), (byte)(Terrain.waterLevelMax / 3 * 2)} },
                { RepeatingPattern.Name.water_shallow, new List<byte>(){ (byte)(Terrain.waterLevelMax / 3 * 2), Terrain.waterLevelMax - 2} },
                { RepeatingPattern.Name.water_supershallow, new List<byte>(){ Terrain.waterLevelMax - 1, Terrain.waterLevelMax} },
                { RepeatingPattern.Name.beach_bright, new List<byte>(){Terrain.waterLevelMax, 95} },
                { RepeatingPattern.Name.beach_dark, new List<byte>(){96, 105} },
                { RepeatingPattern.Name.mountain_low, new List<byte>(){160, 178} },
                { RepeatingPattern.Name.mountain_medium, new List<byte>(){178, 194} },
                { RepeatingPattern.Name.mountain_high, new List<byte>(){194, Terrain.volcanoEdgeMin} },
                { RepeatingPattern.Name.volcano_edge, new List<byte>(){Terrain.volcanoEdgeMin, Terrain.lavaMin} },
                { RepeatingPattern.Name.lava, new List<byte>(){Terrain.lavaMin, 255} },
            };
        }

        private static Dictionary<RepeatingPattern.Name, List<byte>> GetColorsByHumidity()
        {
            return new Dictionary<RepeatingPattern.Name, List<byte>>() {
                { RepeatingPattern.Name.sand, new List<byte>(){0, 75} },
                { RepeatingPattern.Name.ground_bad, new List<byte>(){75, 115} },
                { RepeatingPattern.Name.ground_good, new List<byte>(){115, 120} },
                { RepeatingPattern.Name.grass_bad, new List<byte>(){120, 160} },
                { RepeatingPattern.Name.grass_good, new List<byte>(){160, 255} },
            };
        }
    }
}