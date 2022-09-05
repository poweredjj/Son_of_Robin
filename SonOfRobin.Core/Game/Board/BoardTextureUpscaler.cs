using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BoardTextureUpscaler
    {
        private static Dictionary<string, byte[,]> indexGridByName = new Dictionary<string, byte[,]>();
        private static Dictionary<string, string> gridNameByID = new Dictionary<string, string>();
        private static Dictionary<string, Texture2D> solvedCaseByID = new Dictionary<string, Texture2D> { };

        public static Dictionary<string, List<String>> upscaleNamesDict = new Dictionary<string, List<String>>
        {
            // source (small bitmap) name: all replacement (large bitmap) names

            {"upscale_x1_source", new List<String> { "upscale_x1" } },

            {"upscale_x2_source_corner_LL", new List<String> { "upscale_x2_corner_LL" } },
            {"upscale_x2_source_corner_LR", new List<String> { "upscale_x2_corner_LR" } },
            {"upscale_x2_source_corner_TL", new List<String> { "upscale_x2_corner_TL" } },
            {"upscale_x2_source_corner_TR", new List<String> { "upscale_x2_corner_TR" } },

            {"upscale_x2_source_diag", new List<String> { "upscale_x2_diag_v1", "upscale_x2_diag_v2", "upscale_x2_diag_v3" } },
            {"upscale_x2_source_left-right", new List<String> { "upscale_x2_left-right_v1", "upscale_x2_left-right_v2", "upscale_x2_left-right_v3" } },
            {"upscale_x2_source_top-bottom", new List<String> { "upscale_x2_top-bottom_v1", "upscale_x2_top-bottom_v2", "upscale_x2_top-bottom_v3" } },

            {"upscale_x3_source_left", new List<String> { "upscale_x3_left_v1", "upscale_x3_left_v2", "upscale_x3_left_v3" } },
            {"upscale_x3_source_right", new List<String> { "upscale_x3_right_v1", "upscale_x3_right_v2", "upscale_x3_right_v3" } },
            {"upscale_x3_source_top", new List<String> { "upscale_x3_top_v1", "upscale_x3_top_v2", "upscale_x3_top_v3" } },
            {"upscale_x3_source_bottom", new List<String> { "upscale_x3_bottom_v1", "upscale_x3_bottom_v2", "upscale_x3_bottom_v3" } },

            {"upscale_x3_source_diag_LT-RB", new List<String> { "upscale_x3_diag_LT-RB_v1", "upscale_x3_diag_LT-RB_v2", "upscale_x3_diag_LT-RB_v3" } },
            {"upscale_x3_source_diag_RT-LB", new List<String> { "upscale_x3_diag_RT-LB_v1", "upscale_x3_diag_RT-LB_v2", "upscale_x3_diag_RT-LB_v3" } },

            {"upscale_x4_source", new List<String> { "upscale_x4_v1", "upscale_x4_v2", "upscale_x4_v3" } },
        };


        public static void PrepareUpscaleTemplates()
        {
            var textureNamesList = new List<string>();
            var cornersByName = new Dictionary<string, Color[,]>();

            // creating texture names list

            foreach (var kvp in upscaleNamesDict)
            {
                string sourceName = kvp.Key;

                textureNamesList.Add(sourceName);
                foreach (string upscaledName in kvp.Value)
                {
                    textureNamesList.Add(upscaledName);
                }
            }

            // reading texture data and converting to color index grids (+ storing corners)

            foreach (var textureName in textureNamesList)
            {
                Texture2D texture = SonOfRobinGame.content.Load<Texture2D>($"gfx/Upscale_matrix/{textureName}");

                Color[,] colorGrid = GfxConverter.ConvertTextureToGrid(texture: texture, x: 0, y: 0, width: texture.Width, height: texture.Height);

                var corners = new Color[2, 2];
                corners[0, 0] = colorGrid[0, 0];
                corners[1, 0] = colorGrid[texture.Width - 1, 0];
                corners[0, 1] = colorGrid[0, texture.Height - 1];
                corners[1, 1] = colorGrid[texture.Width - 1, texture.Height - 1];
                cornersByName[textureName] = corners;

                var tuple = ConvertColorGridToIndexGrid(colorGrid);
                var indexGrid = tuple.Item1;

                indexGridByName[textureName] = indexGrid;

                if (upscaleNamesDict.ContainsKey(textureName)) gridNameByID[GetIDForGrid(indexGrid)] = textureName;

                texture.Dispose();
            }

            // checking if source corners match upscaled ones

            foreach (var kvp in upscaleNamesDict)
            {
                string sourceName = kvp.Key;

                foreach (string upscaledName in kvp.Value)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            if (cornersByName[sourceName][x, y] != cornersByName[upscaledName][x, y])
                            {
                                throw new ArgumentException($"Corner colors do not match - {sourceName} vs {upscaledName}.");
                            }
                        }
                    }
                }
            }

            // checking if conversion can be executed properly

            Random random = SonOfRobinGame.random;

            foreach (string sourceName in upscaleNamesDict.Keys)
            {
                var sourceIndexGrid = indexGridByName[sourceName];

                int width = sourceIndexGrid.GetLength(0);
                int height = sourceIndexGrid.GetLength(1);


                var indexToColorDict = new Dictionary<byte, Color>();
                var addedColors = new List<Color>();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte index = sourceIndexGrid[x, y];
                        if (!indexToColorDict.ContainsKey(index))
                        {
                            while (true)
                            {
                                Color color = new Color(r: random.Next(0, 255), g: random.Next(0, 255), b: random.Next(0, 255), alpha: random.Next(0, 255));
                                if (!addedColors.Contains(color))
                                {
                                    indexToColorDict[index] = color;
                                    addedColors.Add(color);

                                    break;
                                }
                            }
                        }
                    }
                }


                var sourceGridRGB = Case.ConvertIndexGridToColorGrid(indexGrid: sourceIndexGrid, indexToColorDict: indexToColorDict);

                new Case(sourceGridRGB);


            }
        }

        public static (byte[,], Dictionary<byte, Color>) ConvertColorGridToIndexGrid(Color[,] gridRGB)
        {
            int width = gridRGB.GetLength(0);
            int height = gridRGB.GetLength(1);

            byte[,] indexGrid = new byte[width, height];

            var colorIndexDict = new Dictionary<Color, byte>();
            var colorsAdded = new List<Color>();
            byte colorCounter = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color currentColor = gridRGB[x, y];
                    if (!colorsAdded.Contains(currentColor))
                    {
                        colorIndexDict[currentColor] = colorCounter;
                        colorCounter++;
                        colorsAdded.Add(currentColor);
                    }

                    indexGrid[x, y] = colorIndexDict[currentColor];
                }
            }

            return (indexGrid, colorIndexDict.ToDictionary(x => x.Value, x => x.Key));
        }

        public static string GetIDForGrid(Color[,] grid)
        {
            string id = "";

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    id += $"{grid[x, y]}";
                }
            }

            return id;
        }

        public static string GetIDForGrid(byte[,] grid)
        {
            string id = "";

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    id += $"{grid[x, y]}";
                }
            }

            return id;
        }


        public BoardTextureUpscaler(Texture2D inputTexture)
        {


        }

        public class Case
        {
            public readonly string caseID;
            public readonly string indexGridID;
            public readonly Dictionary<byte, Color> indexToColorDict; // dictionary for replacing color indices with source colors
            public readonly Color[,] sourceGridRGB; // original grid with RGB values
            public readonly byte[,] sourceIndexGrid; // grid of unique color indices
            public readonly Color[,] resizedGridRGB; // original grid with RGB values

            public Case(Color[,] sourceGridRGB)
            {
                if (sourceGridRGB == null) throw new ArgumentNullException("Input grid cannot be null.");
                if (sourceGridRGB.GetLength(0) != 2) throw new ArgumentException($"Input grid x size {sourceGridRGB.GetLength(0)} must be 2.");
                if (sourceGridRGB.GetLength(1) != 2) throw new ArgumentException($"Input grid y size {sourceGridRGB.GetLength(1)} must be 2.");

                this.sourceGridRGB = sourceGridRGB;
                this.caseID = GetIDForGrid(this.sourceGridRGB);

                var tuple = ConvertColorGridToIndexGrid(this.sourceGridRGB);
                this.sourceIndexGrid = tuple.Item1;
                this.indexToColorDict = tuple.Item2;
                this.indexGridID = GetIDForGrid(this.sourceIndexGrid);

                var resizedIndexGrid = GetMatchingResizedGrid();
                this.resizedGridRGB = ConvertIndexGridToColorGrid(indexGrid: resizedIndexGrid, indexToColorDict: this.indexToColorDict);
            }

            private byte[,] GetMatchingResizedGrid()
            {
                if (!gridNameByID.ContainsKey(this.indexGridID)) throw new ArgumentException("Cannot find matching source grid.");

                string sourceName = gridNameByID[this.indexGridID];
                var upscaledNames = upscaleNamesDict[sourceName];

                var upscaledGridName = upscaledNames[SonOfRobinGame.random.Next(0, upscaledNames.Count)];
                return indexGridByName[upscaledGridName];
            }

            public static Color[,] ConvertIndexGridToColorGrid(byte[,] indexGrid, Dictionary<byte, Color> indexToColorDict)
            {
                int width = indexGrid.GetLength(0);
                int height = indexGrid.GetLength(1);

                Color[,] rgbGrid = new Color[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        rgbGrid[x, y] = indexToColorDict[indexGrid[x, y]];
                    }
                }

                return rgbGrid;
            }

        }

    }
}
