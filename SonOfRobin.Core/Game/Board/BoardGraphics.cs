using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace SonOfRobin
{
    public class BoardGraphics
    {
        public static readonly Dictionary<RepeatingPattern.Name, List<byte>> patternNamesByHeight = GetPatternNameByHeight();
        public static readonly Dictionary<RepeatingPattern.Name, List<byte>> patternNamesByHumidity = GetPatternNameByHumidity();

        private readonly Cell cell;
        public FileStream fileStream;
        public Texture2D Texture { get; private set; }
        public bool hasBeenCopiedElsewhere;

        public readonly string templatePath;

        private bool PNGTemplateFound;
        private int PNGTemplateCheckCooldown; // next frame, when checking PNG template will be possible

        public bool PNGTemplateExists
        {
            get
            {
                if (this.PNGTemplateFound) return true;
                if (SonOfRobinGame.CurrentDraw < this.PNGTemplateCheckCooldown) return false;

                this.PNGTemplateFound = File.Exists(templatePath); // buffering info if PNG found, because of disk operation cost
                this.PNGTemplateCheckCooldown = SonOfRobinGame.CurrentDraw + 20;
                return this.PNGTemplateFound;
            }
        }

        public BoardGraphics(Grid grid, Cell cell)
        {
            this.cell = cell;
            this.templatePath = Path.Combine(grid.gridTemplate.templatePath, $"background_{cell.cellNoX}_{cell.cellNoY}.png");
            this.Texture = null;
            this.hasBeenCopiedElsewhere = false;
            this.PNGTemplateFound = false;
            this.PNGTemplateCheckCooldown = 0;
        }

        public void LoadTexture()
        {
            if (this.Texture != null) return;

            if (this.fileStream == null) this.fileStream = GfxConverter.OpenFileStream(cell.boardGraphics.templatePath);
            if (this.fileStream != null)
            {
                this.Texture = GfxConverter.LoadTextureFromFileStream(this.fileStream);
                this.fileStream = null;
            }

            if (this.Texture == null) SonOfRobinGame.BoardTextureProcessor.AddCellToProcess(this.cell);
        }

        public void ReplaceTexture(Texture2D texture)
        {
            if (this.Texture != null) this.Texture.Dispose();
            this.Texture = texture;
        }

        public void UnloadTexture()
        {
            if (this.Texture == null) return;

            this.Texture.Dispose();
            this.Texture = null;
        }

        public static string GetWholeIslandMapPath(Grid grid)
        {
            return Path.Combine(grid.gridTemplate.templatePath, "whole_island.png");
        }


        public void CreateBitmapFromTerrainAndSaveAsPNG()
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

            List<Point> edgePointList = new();

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

            var workingGrid3x3 = new RepeatingPattern.Name[3, 3]; // working grid is needed, because the edges are missing and using sourceOffset will not work

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

            // putting upscaled color grid into image
            var image = new Image<Rgba32>(targetWidth, targetHeight);

            double multiplier = (float)resDivider / (float)BoardTextureUpscaler3x.resizeFactor;

            for (int y = 0; y < targetHeight; y++)
            {
                int yFactor = (int)((float)y * multiplier);

                for (int x = 0; x < targetWidth; x++)
                {
                    Color pixel = RepeatingPattern.patternDict[upscaledGrid[x, y]].GetValue(
                        x: this.cell.xMin + (int)((float)x * multiplier),
                        y: this.cell.yMin + yFactor);

                    image[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            // saving as PNG
            try
            {
                using (var fileStreamToSave = new FileStream(this.templatePath, FileMode.Create))
                {
                    image.Save(fileStreamToSave, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression });
                }
                this.fileStream = GfxConverter.OpenFileStream(cell.boardGraphics.templatePath);
            }
            catch (IOException)
            { }
        }

        private static RepeatingPattern.Name FindPatternNameForPixel(Grid grid, int x, int y)
        {
            byte pixelHeight = grid.terrainByName[Terrain.Name.Height].GetMapData(x, y);
            byte pixelHumidity = grid.terrainByName[Terrain.Name.Humidity].GetMapData(x, y);
            byte pixelBiome = grid.terrainByName[Terrain.Name.Biome].GetMapData(x, y);

            //{ // for testing
            //    Dictionary<ExtBoardProps.Name, bool> extDataValDict = grid.GetExtValueDict(x, y);
            //    if (extDataValDict[ExtBoardProps.Name.OuterBeach]) return RepeatingPattern.Name.just_red;
            //    if (extDataValDict[ExtBoardProps.Name.Sea]) return RepeatingPattern.Name.just_cyan;
            //    if (extDataValDict[ExtBoardProps.Name.BiomeSwamp]) return RepeatingPattern.Name.just_blue;
            //} // for testing

            if (pixelBiome >= Terrain.biomeMin)
            {
                Dictionary<ExtBoardProps.Name, bool> extDataValDict = grid.ExtBoardProps.GetValueDict(x: x, y: y);

                if (extDataValDict[ExtBoardProps.Name.BiomeSwamp]) return RepeatingPattern.Name.swamp;
                else if (extDataValDict[ExtBoardProps.Name.BiomeRuins]) return RepeatingPattern.Name.ruins;
            }

            foreach (var kvp in patternNamesByHeight)
            {
                if (kvp.Value[1] >= pixelHeight && pixelHeight >= kvp.Value[0]) return kvp.Key;
            }

            foreach (var kvp in patternNamesByHumidity)
            {
                if (kvp.Value[1] >= pixelHumidity && pixelHumidity >= kvp.Value[0]) return kvp.Key;
            }

            return RepeatingPattern.Name.unset; // to indicate areas that should be filled but are not
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

        private static Dictionary<RepeatingPattern.Name, List<byte>> GetPatternNameByHeight()
        {
            return new Dictionary<RepeatingPattern.Name, List<byte>>() {
                { RepeatingPattern.Name.water_deep, new List<byte>(){0,(byte)(Terrain.waterLevelMax / 3)} },
                { RepeatingPattern.Name.water_medium, new List<byte>(){ (byte)(Terrain.waterLevelMax / 3), (byte)(Terrain.waterLevelMax / 3 * 2)} },
                { RepeatingPattern.Name.water_shallow, new List<byte>(){ (byte)(Terrain.waterLevelMax / 3 * 2), Terrain.waterLevelMax - 2} },
                { RepeatingPattern.Name.water_supershallow, new List<byte>(){ Terrain.waterLevelMax - 1, Terrain.waterLevelMax} },
                { RepeatingPattern.Name.beach_bright, new List<byte>(){Terrain.waterLevelMax, 95} },
                { RepeatingPattern.Name.beach_dark, new List<byte>(){96, 105} },
                { RepeatingPattern.Name.mountain_low, new List<byte>(){Terrain.rocksLevelMin, 178} },
                { RepeatingPattern.Name.mountain_medium, new List<byte>(){178, 194} },
                { RepeatingPattern.Name.mountain_high, new List<byte>(){194, Terrain.volcanoEdgeMin} },
                { RepeatingPattern.Name.volcano_edge, new List<byte>(){Terrain.volcanoEdgeMin, Terrain.lavaMin} },
                { RepeatingPattern.Name.lava, new List<byte>(){Terrain.lavaMin, 255} },
            };
        }

        private static Dictionary<RepeatingPattern.Name, List<byte>> GetPatternNameByHumidity()
        {
            return new Dictionary<RepeatingPattern.Name, List<byte>>() {
                { RepeatingPattern.Name.sand, new List<byte>(){0, 75} },
                { RepeatingPattern.Name.ground_bad, new List<byte>(){75, 115} },
                { RepeatingPattern.Name.ground_good, new List<byte>(){115, 120} },
                { RepeatingPattern.Name.grass_bad, new List<byte>(){120, 160 } },
                { RepeatingPattern.Name.grass_good, new List<byte>(){160, 255} },
            };
        }
    }
}