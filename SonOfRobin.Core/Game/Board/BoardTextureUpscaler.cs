using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BoardTextureUpscaler
    {
        private static Dictionary<string, byte[,]> colorGridByName = new Dictionary<string, byte[,]>();
        private static Dictionary<string, Texture2D> solvedCasesByID = new Dictionary<string, Texture2D> { };

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

            // reading texture data and converting to color index arrays (+ storing corners)

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

                var tuple = ConvertColorArrayToIndexArray(colorGrid);
                colorGridByName[textureName] = tuple.Item1;

                texture.Dispose();
            }

            // checking if corners match

            foreach (var kvp in upscaleNamesDict)
            {
                string sourceName = kvp.Key;

                foreach (string upscaledName in kvp.Value)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            if (cornersByName[sourceName][x, y] != cornersByName[upscaledName][x, y]) throw new ArgumentException($"Corner colors do not match - {sourceName} vs {upscaledName}.");
                        }
                    }
                }
            }
        }

        public static (byte[,], Dictionary<byte, Color>) ConvertColorArrayToIndexArray(Color[,] inputArray)
        {
            int width = inputArray.GetLength(0);
            int height = inputArray.GetLength(1);

            byte[,] sourceArray = new byte[width, height];

            var colorIndexDict = new Dictionary<Color, byte>();
            var colorsAdded = new List<Color>();
            byte colorCounter = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color currentColor = inputArray[x, y];
                    if (!colorsAdded.Contains(currentColor))
                    {
                        colorIndexDict[currentColor] = colorCounter;
                        colorCounter++;
                        colorsAdded.Add(currentColor);
                    }

                    sourceArray[x, y] = colorIndexDict[currentColor];
                }
            }

            return (sourceArray, colorIndexDict.ToDictionary(x => x.Value, x => x.Key));
        }


        public BoardTextureUpscaler(Texture2D inputTexture)
        {


        }

        public class Case
        {
            public readonly Color[,] inputArray; // original array with RGB values
            public byte[,] SourceArray { get; private set; } // array of unique color indices
            public readonly Dictionary<byte, Color> indexToColorDict; // dictionary for replacing color indices with source colors
            public readonly string caseID;

            public Case(Color[,] sourceArray)
            {
                this.inputArray = sourceArray;
                this.caseID = GetCaseIDForArray(this.inputArray);

                var tuple = ConvertColorArrayToIndexArray(this.inputArray);
                this.SourceArray = tuple.Item1;
                this.indexToColorDict = tuple.Item2;
            }

            public static string GetCaseIDForArray(Color[,] inputArray)
            {
                if (inputArray == null) throw new ArgumentNullException("Input array cannot be null.");
                if (inputArray.GetLength(0) != 2) throw new ArgumentException($"Input array x size {inputArray.GetLength(0)} must be 2.");
                if (inputArray.GetLength(1) != 2) throw new ArgumentException($"Input array y size {inputArray.GetLength(1)} must be 2.");

                return $"{inputArray[0, 0]},{inputArray[0, 1]},{inputArray[1, 0]},{inputArray[1, 1]}";
            }

        }

    }
}
