﻿using BigGustave;
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
        public Texture2D texture;
        private readonly string templatePath;
        private bool savedToDisk;

        public static readonly Dictionary<Colors, Color> colorsByName = GetColorsByName();
        private static readonly Dictionary<Colors, List<byte>> colorsByHeight = GetColorsByHeight();
        private static readonly Dictionary<Colors, List<byte>> colorsByHumidity = GetColorsByHumidity();

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
            if (this.texture != null) return;

            this.texture = GfxConverter.LoadTextureFromPNG(this.templatePath);
            if (this.texture == null)
            {
                this.CreateBitmapFromTerrain();
                this.texture = GfxConverter.LoadTextureFromPNG(this.templatePath); // trying to create texture on disk first...
                if (this.texture == null)
                {
                    Color[,] colorGrid = this.CreateBitmapFromTerrain(); // ...if this fails (disk error, locked file, etc.), get texture data directly
                    this.texture = GfxConverter.Convert2DArrayToTexture(colorGrid);
                }
            }

            this.cell.grid.loadedTexturesCount++;
        }

        public void UnloadTexture()
        {
            if (this.texture == null) return;

            this.texture.Dispose();
            this.texture = null;
            this.cell.grid.loadedTexturesCount--;
        }

        public void CreateAndSavePngTemplate()
        {
            if (this.savedToDisk) return;

            this.CreateBitmapFromTerrain();
            this.savedToDisk = true;
        }

        public static Texture2D CreateEntireMapTexture(int width, int height, Grid grid, float multiplier)
        {
            var colorArray = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelX = (int)(x / multiplier);
                    int pixelY = (int)(y / multiplier);

                    byte pixelHeight = grid.GetFieldValue(x: pixelX, y: pixelY, terrainName: TerrainName.Height);
                    byte pixelHumidity = grid.GetFieldValue(x: pixelX, y: pixelY, terrainName: TerrainName.Humidity);
                    byte pixelDanger = grid.GetFieldValue(x: pixelX, y: pixelY, terrainName: TerrainName.Danger);
                    colorArray[(y * width) + x] = CreatePixel(pixelHeight: pixelHeight, pixelHumidity: pixelHumidity, pixelDanger: pixelDanger);
                }
            }

            Texture2D texture = new Texture2D(SonOfRobinGame.graphicsDevice, width, height);
            texture.SetData(colorArray);
            return texture;
        }

        private Color[,] CreateBitmapFromTerrain()
        {
            // can be run in parallel, because it does not use graphicsDevice

            // creating base 2D color array with rgb color data

            int sourceWidth = this.cell.dividedWidth;
            int sourceHeight = this.cell.dividedHeight;
            int resDivider = this.cell.grid.resDivider;

            Terrain heightTerrain = this.cell.terrainByName[TerrainName.Height];
            Terrain humidityTerrain = this.cell.terrainByName[TerrainName.Humidity];
            Terrain dangerTerrain = this.cell.terrainByName[TerrainName.Danger];

            Color[,] smallColorGrid = new Color[sourceWidth, sourceHeight];

            for (int localX = 0; localX < this.cell.dividedWidth; localX++)
            {
                for (int localY = 0; localY < this.cell.dividedHeight; localY++)
                {
                    smallColorGrid[localX, localY] = CreatePixel(
                        pixelHeight: heightTerrain.GetMapDataRaw(localX, localY),
                        pixelHumidity: humidityTerrain.GetMapDataRaw(localX, localY),
                        pixelDanger: dangerTerrain.GetMapDataRaw(localX, localY));

                    { // start of ExtBoardProperties visualisation (edges of cells will be unpainted) - for testing
                        int realX = this.cell.xMin + (localX * this.cell.grid.resDivider);
                        int realY = this.cell.yMin + (localY * this.cell.grid.resDivider);

                        bool sea = this.cell.grid.GetExtProperty(name: ExtBoardProperties.ExtPropName.OuterBeachEdge, x: realX, y: realY);
                        if (sea) smallColorGrid[localX, localY] = Color.HotPink;
                    } // end of ExtBoardProperties visualisation (edges of cells will be unpainted) - for testing
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
                        try
                        {
                            int localX = point.X + xOffset; // do not use to calculate world space
                            int localY = point.Y + yOffset; // do not use to calculate world space

                            // looking for pixel locally
                            workingGrid3x3[xOffset + 1, yOffset + 1] = CreatePixel(
                                pixelHeight: heightTerrain.GetMapDataRaw(localX, localY),
                                pixelHumidity: humidityTerrain.GetMapDataRaw(localX, localY),
                                pixelDanger: dangerTerrain.GetMapDataRaw(localX, localY));
                        }
                        catch (IndexOutOfRangeException)
                        {
                            try
                            {
                                // looking for pixel in the whole grid
                                int worldSpaceX = this.cell.xMin + (point.X * resDivider) + xOffset;
                                int worldSpaceY = this.cell.yMin + (point.Y * resDivider) + yOffset;

                                workingGrid3x3[xOffset + 1, yOffset + 1] = CreatePixel(
                                    pixelHeight: this.cell.grid.GetFieldValue(terrainName: TerrainName.Height, x: worldSpaceX, y: worldSpaceY),
                                    pixelHumidity: this.cell.grid.GetFieldValue(terrainName: TerrainName.Humidity, x: worldSpaceX, y: worldSpaceY),
                                    pixelDanger: this.cell.grid.GetFieldValue(terrainName: TerrainName.Danger, x: worldSpaceX, y: worldSpaceY));
                            }
                            catch (IndexOutOfRangeException)
                            {
                                // pixel outside world bounds - inserting the nearest correct position
                                workingGrid3x3[xOffset + 1, yOffset + 1] = smallColorGrid[point.X, point.Y];
                            }
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

        private static Color CreatePixel(byte pixelHeight, byte pixelHumidity, byte pixelDanger)
        {
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

            if (pixelDanger >= Terrain.safeZoneMax)
            {
                byte dangerAlpha = (byte)(((float)(pixelDanger - Terrain.safeZoneMax + 20) / (255 - Terrain.safeZoneMax)) * 150); // last value is max possible alpha
                dangerAlpha = (byte)((int)(dangerAlpha / 15) * 15); // converting gradient to discrete shades

                pixel = Blend2Colors(bottomColor: pixel, topColor: new Color((byte)40, (byte)0, (byte)0, dangerAlpha));
            }

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
                {Colors.Mountains4, new List<byte>(){194, Terrain.volcanoEdgeMin}},
                {Colors.VolcanoEdge, new List<byte>(){Terrain.volcanoEdgeMin, Terrain.lavaMin}},
                {Colors.VolcanoInside1, new List<byte>(){Terrain.lavaMin, 225}},
                {Colors.VolcanoInside2, new List<byte>(){225, 255}},
            };
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
