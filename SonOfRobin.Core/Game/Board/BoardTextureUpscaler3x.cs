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

                    Color[,] targetGrid3x3 = Upscale3x3Grid(sourceGrid3x3: sourceGrid3x3);
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


        private static Color[,] Upscale3x3Grid(Color[,] sourceGrid3x3)
        {
            Color[,] targetGrid3x3 = new Color[3, 3];

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    targetGrid3x3[x, y] = sourceGrid3x3[1, 1];
                }
            }

            if (sourceGrid3x3[1, 0] == sourceGrid3x3[0, 1] && sourceGrid3x3[1, 0] != sourceGrid3x3[2, 1] && sourceGrid3x3[0, 1] != sourceGrid3x3[1, 2]) targetGrid3x3[0, 0] = sourceGrid3x3[1, 0];

            if ((sourceGrid3x3[1, 0] == sourceGrid3x3[0, 1] && sourceGrid3x3[1, 0] != sourceGrid3x3[2, 1] && sourceGrid3x3[0, 1] != sourceGrid3x3[1, 2] && sourceGrid3x3[1, 1] != sourceGrid3x3[0, 2]) || (sourceGrid3x3[0, 1] == sourceGrid3x3[1, 2] && sourceGrid3x3[0, 1] != sourceGrid3x3[1, 0] && sourceGrid3x3[1, 2] != sourceGrid3x3[2, 1] && sourceGrid3x3[1, 1] != sourceGrid3x3[0, 0])) targetGrid3x3[1, 0] = sourceGrid3x3[0, 1];

            if (sourceGrid3x3[0, 1] == sourceGrid3x3[1, 2] && sourceGrid3x3[0, 1] != sourceGrid3x3[1, 0] && sourceGrid3x3[1, 2] != sourceGrid3x3[2, 1]) targetGrid3x3[2, 0] = sourceGrid3x3[1, 2];

            if ((sourceGrid3x3[2, 1] == sourceGrid3x3[1, 0] && sourceGrid3x3[2, 1] != sourceGrid3x3[1, 2] && sourceGrid3x3[1, 0] != sourceGrid3x3[0, 1] && sourceGrid3x3[1, 1] != sourceGrid3x3[0, 0]) || (sourceGrid3x3[1, 0] == sourceGrid3x3[0, 1] && sourceGrid3x3[1, 0] != sourceGrid3x3[2, 1] && sourceGrid3x3[0, 1] != sourceGrid3x3[1, 2] && sourceGrid3x3[1, 1] != sourceGrid3x3[2, 0])) targetGrid3x3[0, 1] = sourceGrid3x3[1, 0];

            if ((sourceGrid3x3[0, 1] == sourceGrid3x3[1, 2] && sourceGrid3x3[0, 1] != sourceGrid3x3[1, 0] && sourceGrid3x3[1, 2] != sourceGrid3x3[2, 1] && sourceGrid3x3[1, 1] != sourceGrid3x3[2, 2]) || (sourceGrid3x3[1, 2] == sourceGrid3x3[2, 1] && sourceGrid3x3[1, 2] != sourceGrid3x3[0, 1] && sourceGrid3x3[2, 1] != sourceGrid3x3[1, 0] && sourceGrid3x3[1, 1] != sourceGrid3x3[0, 2])) targetGrid3x3[2, 1] = sourceGrid3x3[1, 2];

            if (sourceGrid3x3[2, 1] == sourceGrid3x3[1, 0] && sourceGrid3x3[2, 1] != sourceGrid3x3[1, 2] && sourceGrid3x3[1, 0] != sourceGrid3x3[0, 1]) targetGrid3x3[0, 2] = sourceGrid3x3[1, 0];

            if ((sourceGrid3x3[1, 2] == sourceGrid3x3[2, 1] && sourceGrid3x3[1, 2] != sourceGrid3x3[0, 1] && sourceGrid3x3[2, 1] != sourceGrid3x3[1, 0] && sourceGrid3x3[1, 1] != sourceGrid3x3[2, 0]) || (sourceGrid3x3[2, 1] == sourceGrid3x3[1, 0] && sourceGrid3x3[2, 1] != sourceGrid3x3[1, 2] && sourceGrid3x3[1, 0] != sourceGrid3x3[0, 1] && sourceGrid3x3[1, 1] != sourceGrid3x3[2, 2])) targetGrid3x3[1, 2] = sourceGrid3x3[2, 1];

            if (sourceGrid3x3[1, 2] == sourceGrid3x3[2, 1] && sourceGrid3x3[1, 2] != sourceGrid3x3[0, 1] && sourceGrid3x3[2, 1] != sourceGrid3x3[1, 0]) targetGrid3x3[2, 2] = sourceGrid3x3[1, 2];

            return targetGrid3x3;
        }


    }
}
