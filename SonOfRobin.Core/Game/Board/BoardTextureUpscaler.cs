using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BoardTextureUpscaler
    {
        public static Dictionary<string, Texture2D> TextureByName { get; private set; } = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> solvedCasesByID = new Dictionary<string, Texture2D> { };

        private static readonly List<string> textureNames = new List<string>
        {
            "upscale_x2_source_corner_LL",
            "upscale_x2_source_corner_LR",
            "upscale_x2_source_corner_TL",
            "upscale_x2_source_corner_TR",
            "upscale_x2_source_diag_v1",
            "upscale_x2_source_left-right_v1",
            "upscale_x2_source_top-bottom_v1",

            "upscale_x3_source_bottom_v1",
            "upscale_x3_source_diag_LT-RB_v1",
            "upscale_x3_source_diag_RT-LB_v1",
            "upscale_x3_source_L_v1",
            "upscale_x3_source_R_v1",
            "upscale_x3_source_T_v1",

            "upscale_x2_corner_LL",
            "upscale_x2_corner_LR",
            "upscale_x2_corner_TL",
            "upscale_x2_corner_TR",
            "upscale_x2_diag_v1",
            "upscale_x2_diag_v2",
            "upscale_x2_diag_v3",
            "upscale_x2_left-righ_v2",
            "upscale_x2_left-right_v1",
            "upscale_x2_left-right_v3",
            "upscale_x2_top-bottom_v1",
            "upscale_x2_top-bottom_v2",
            "upscale_x2_top-bottom_v3",

            "upscale_x3_diag_RT-LB_v1",
            "upscale_x3_diag_RT-LB_v2",
            "upscale_x3_diag_RT-LB_v3",
            "upscale_x3_L_v1",
            "upscale_x3_L_v2",
            "upscale_x3_L_v3",
            "upscale_x3_R_v1",
            "upscale_x3_R_v2",
            "upscale_x3_R_v3",
            "upscale_x3_T_v1",
            "upscale_x3_T_v2",
            "upscale_x3_T_v3",
            "upscale_x3_bottom_v1",
            "upscale_x3_bottom_v2",
            "upscale_x3_bottom_v3",
            "upscale_x3_diag_LT-RB_v1",
            "upscale_x3_diag_LT-RB_v2",
            "upscale_x3_diag_LT-RB_v3",
        };

        public static void LoadAllTextures()
        {
            foreach (string textureName in textureNames)
            {
                TextureByName[textureName] = SonOfRobinGame.content.Load<Texture2D>($"gfx/Upscale_matrix/{textureName}");
            }
        }


        public BoardTextureUpscaler()
        {


        }

        public class Case
        {
            public readonly Color[,] inputArray; // original array with RGB values
            public readonly int[,] sourceArray; // array of unique color indices
            public readonly Dictionary<int, Color> indexToColorDict; // dictionary for replacing color indices with source colors
            public readonly string caseID;


            public Case(Color[,] sourceArray)
            {
                this.inputArray = sourceArray;
                this.caseID = GetCaseIDForArray(this.inputArray);

                var tuple = this.ConvertInputArrayToSourceArray();
                this.sourceArray = tuple.Item1;
                this.indexToColorDict = tuple.Item2;
            }

            private (int[,], Dictionary<int, Color>) ConvertInputArrayToSourceArray()
            {
                int[,] sourceArray = new int[2, 2];

                var colorIndexDict = new Dictionary<Color, int>();
                var colorsAdded = new List<Color>();
                int colorCounter = 0;

                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        Color currentColor = this.inputArray[x, y];
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
