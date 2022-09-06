using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class BoardTextureUpscaler3x
    {
        private static readonly int resizeFactor = 3;

        private readonly int sourceWidth;
        private readonly int sourceHeight;
        private readonly int targetWidth;
        private readonly int targetHeight;

        private readonly Color[,] sourceRGBGrid;
        private readonly Color[,] upscaledRGBGrid;

        public static Color[,] UpscaleColorGrid(Color[,] sourceRGBGrid)
        {
            BoardTextureUpscaler3x upscaler = new BoardTextureUpscaler3x(sourceRGBGrid: sourceRGBGrid);
            return upscaler.upscaledRGBGrid;
        }

        public static Texture2D UpscaleTexture(Texture2D sourceTexture)
        {
            // slow, should be used for debugging only

            Color[,] sourceRGBGrid = GfxConverter.ConvertTextureToGrid(texture: sourceTexture, x: 0, y: 0, width: sourceTexture.Width, height: sourceTexture.Height);
            Color[,] upscaledRGBGrid = UpscaleColorGrid(sourceRGBGrid: sourceRGBGrid);

            return GfxConverter.Convert2DArrayToTexture(upscaledRGBGrid);
        }

        private BoardTextureUpscaler3x(Color[,] sourceRGBGrid)
        {
            this.sourceWidth = sourceRGBGrid.GetLength(0);
            this.sourceHeight = sourceRGBGrid.GetLength(1);
            this.targetWidth = this.sourceWidth * resizeFactor;
            this.targetHeight = this.sourceHeight * resizeFactor;

            if (this.sourceWidth % 2 != 0) throw new ArgumentException($"Source width {this.sourceWidth} is not divisible by 2.");
            if (sourceHeight % 2 != 0) throw new ArgumentException($"Source height {this.sourceHeight} is not divisible by 2.");

            this.sourceRGBGrid = sourceRGBGrid;
            this.upscaledRGBGrid = new Color[this.targetWidth, this.targetHeight];

            this.FillUpscaledGrid();
        }

        private void FillUpscaledGrid()
        {
            Color[,] targetGrid3x3 = new Color[3, 3];

            for (int baseY = 1; baseY < this.sourceHeight - 1; baseY++)
            {
                for (int baseX = 1; baseX < this.sourceWidth - 1; baseX++)
                {
                    // preparing source grid for interpolation

                    Color[,] sourceGrid3x3 = new Color[3, 3];
                    for (int yOffset = 0; yOffset < 3; yOffset++)
                    {
                        int currentY = baseY + yOffset - 1;

                        for (int xOffset = 0; xOffset < 3; xOffset++)
                        {
                            try
                            {
                                sourceGrid3x3[xOffset, yOffset] = this.sourceRGBGrid[baseX + xOffset - 1, currentY];
                            }
                            catch (IndexOutOfRangeException)
                            { }
                        }
                    }

                    // creating target grid and filling with middle value

                    targetGrid3x3 = Upscale3x3Grid(srcGrid: sourceGrid3x3, targetGrid: targetGrid3x3);
                    for (int yOffset = 0; yOffset < 3; yOffset++)
                    {
                        for (int xOffset = 0; xOffset < 3; xOffset++)
                        {
                            try
                            {
                                this.upscaledRGBGrid[(baseX * resizeFactor) + xOffset, (baseY * resizeFactor) + yOffset] = targetGrid3x3[xOffset, yOffset];
                            }
                            catch (IndexOutOfRangeException)
                            { }

                        }
                    }
                }
            }
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
