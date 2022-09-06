using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class BoardTextureUpscaler3x
    {
        private static readonly int resizeFactor = 3;

        public static Texture2D UpscaleTexture(Texture2D sourceTexture)
        {
            // slow, should be used for debugging only

            Color[,] sourceRGBGrid = GfxConverter.ConvertTextureToGrid(texture: sourceTexture, x: 0, y: 0, width: sourceTexture.Width, height: sourceTexture.Height);
            Color[,] upscaledRGBGrid = UpscaleColorGrid(sourceRGBGrid: sourceRGBGrid);

            return GfxConverter.Convert2DArrayToTexture(upscaledRGBGrid);
        }

        public static Color[,] UpscaleColorGrid(Color[,] sourceRGBGrid)
        {
            int sourceWidth = sourceRGBGrid.GetLength(0);
            int sourceHeight = sourceRGBGrid.GetLength(1);
            int targetWidth = sourceWidth * resizeFactor;
            int targetHeight = sourceHeight * resizeFactor;

            Color[,] upscaledRGBGrid = new Color[targetWidth, targetHeight];

            Color[,] sourceGrid3x3 = new Color[3, 3];
            Color[,] targetGrid3x3 = new Color[3, 3];

            for (int baseY = 0; baseY < sourceHeight; baseY++)
            {
                for (int baseX = 0; baseX < sourceWidth; baseX++)
                {
                    // preparing source grid for interpolation

                    for (int yOffset = 0; yOffset < 3; yOffset++)
                    {
                        int currentY = baseY + yOffset - 1;

                        for (int xOffset = 0; xOffset < 3; xOffset++)
                        {
                            try
                            {
                                sourceGrid3x3[xOffset, yOffset] = sourceRGBGrid[baseX + xOffset - 1, currentY];
                            }
                            catch (IndexOutOfRangeException)
                            {
                                continue;
                            }
                        }
                    }

                    // updating target 3x3 grid

                    Upscale3x3Grid(srcGrid: sourceGrid3x3, targetGrid: targetGrid3x3);

                    // updating upscaled grid using data from target 3x3 grid

                    for (int yOffset = 0; yOffset < 3; yOffset++)
                    {
                        for (int xOffset = 0; xOffset < 3; xOffset++)
                        {
                            try
                            {
                                upscaledRGBGrid[(baseX * resizeFactor) + xOffset, (baseY * resizeFactor) + yOffset] = targetGrid3x3[xOffset, yOffset];
                            }
                            catch (IndexOutOfRangeException)
                            { }

                        }
                    }
                }
            }

            return upscaledRGBGrid;
        }

        private static Color[,] Upscale3x3Grid(Color[,] srcGrid, Color[,] targetGrid)
        {
            // base code, before replacing letters with srcGrid locations

            //Color A = srcGrid[0, 0];
            //Color B = srcGrid[1, 0];
            //Color C = srcGrid[2, 0];
            //Color D = srcGrid[0, 1];
            //Color E = srcGrid[1, 1];
            //Color F = srcGrid[2, 1];
            //Color G = srcGrid[0, 2];
            //Color H = srcGrid[1, 2];
            //Color I = srcGrid[2, 2];

            //targetGrid[0, 0] = D == B && D != H && B != F ? D : E;
            //targetGrid[1, 0] = (D == B && D != H && B != F && E != C) || (B == F && B != D && F != H && E != A) ? B : E;
            //targetGrid[2, 0] = (B == F && B != D && F != H) ? F : E;
            //targetGrid[0, 1] = (H == D && H != F && D != B && E != A) || (D == B && D != H && B != F && E != G) ? D : E;
            //targetGrid[1, 1] = E;
            //targetGrid[2, 1] = (B == F && B != D && F != H && E != I) || (F == H && F != B && H != D && E != C) ? F : E;
            //targetGrid[0, 2] = H == D && H != F && D != B ? D : E;
            //targetGrid[1, 2] = (F == H && F != B && H != D && E != G) || (H == D && H != F && D != B && E != I) ? H : E;
            //targetGrid[2, 2] = F == H && F != B && H != D ? F : E;

            // optimized code

            targetGrid[0, 0] = srcGrid[0, 1] == srcGrid[1, 0] && srcGrid[0, 1] != srcGrid[1, 2] && srcGrid[1, 0] != srcGrid[2, 1] ? srcGrid[0, 1] : srcGrid[1, 1];
            targetGrid[1, 0] = (srcGrid[0, 1] == srcGrid[1, 0] && srcGrid[0, 1] != srcGrid[1, 2] && srcGrid[1, 0] != srcGrid[2, 1] && srcGrid[1, 1] != srcGrid[2, 0]) || (srcGrid[1, 0] == srcGrid[2, 1] && srcGrid[1, 0] != srcGrid[0, 1] && srcGrid[2, 1] != srcGrid[1, 2] && srcGrid[1, 1] != srcGrid[0, 0]) ? srcGrid[1, 0] : srcGrid[1, 1];
            targetGrid[2, 0] = (srcGrid[1, 0] == srcGrid[2, 1] && srcGrid[1, 0] != srcGrid[0, 1] && srcGrid[2, 1] != srcGrid[1, 2]) ? srcGrid[2, 1] : srcGrid[1, 1];
            targetGrid[0, 1] = (srcGrid[1, 2] == srcGrid[0, 1] && srcGrid[1, 2] != srcGrid[2, 1] && srcGrid[0, 1] != srcGrid[1, 0] && srcGrid[1, 1] != srcGrid[0, 0]) || (srcGrid[0, 1] == srcGrid[1, 0] && srcGrid[0, 1] != srcGrid[1, 2] && srcGrid[1, 0] != srcGrid[2, 1] && srcGrid[1, 1] != srcGrid[0, 2]) ? srcGrid[0, 1] : srcGrid[1, 1];
            targetGrid[1, 1] = srcGrid[1, 1];
            targetGrid[2, 1] = (srcGrid[1, 0] == srcGrid[2, 1] && srcGrid[1, 0] != srcGrid[0, 1] && srcGrid[2, 1] != srcGrid[1, 2] && srcGrid[1, 1] != srcGrid[2, 2]) || (srcGrid[2, 1] == srcGrid[1, 2] && srcGrid[2, 1] != srcGrid[1, 0] && srcGrid[1, 2] != srcGrid[0, 1] && srcGrid[1, 1] != srcGrid[2, 0]) ? srcGrid[2, 1] : srcGrid[1, 1];
            targetGrid[0, 2] = srcGrid[1, 2] == srcGrid[0, 1] && srcGrid[1, 2] != srcGrid[2, 1] && srcGrid[0, 1] != srcGrid[1, 0] ? srcGrid[0, 1] : srcGrid[1, 1];
            targetGrid[1, 2] = (srcGrid[2, 1] == srcGrid[1, 2] && srcGrid[2, 1] != srcGrid[1, 0] && srcGrid[1, 2] != srcGrid[0, 1] && srcGrid[1, 1] != srcGrid[0, 2]) || (srcGrid[1, 2] == srcGrid[0, 1] && srcGrid[1, 2] != srcGrid[2, 1] && srcGrid[0, 1] != srcGrid[1, 0] && srcGrid[1, 1] != srcGrid[2, 2]) ? srcGrid[1, 2] : srcGrid[1, 1];
            targetGrid[2, 2] = srcGrid[2, 1] == srcGrid[1, 2] && srcGrid[2, 1] != srcGrid[1, 0] && srcGrid[1, 2] != srcGrid[0, 1] ? srcGrid[2, 1] : srcGrid[1, 1];

            return targetGrid;
        }

    }
}
