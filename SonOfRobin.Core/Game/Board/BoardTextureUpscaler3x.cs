using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

            Color[,] sourceGrid3x3 = new Color[3, 3];
            Color[,] upscaledGrid = new Color[targetWidth, targetHeight];

            for (int baseY = 1; baseY < sourceHeight - 1; baseY++)
            {
                for (int baseX = 1; baseX < sourceWidth - 1; baseX++)
                {
                    // preparing source grid for interpolation

                    sourceGrid3x3[0, 0] = sourceRGBGrid[baseX - 1, baseY - 1];
                    sourceGrid3x3[1, 0] = sourceRGBGrid[baseX, baseY - 1];
                    sourceGrid3x3[2, 0] = sourceRGBGrid[baseX + 1, baseY - 1];

                    sourceGrid3x3[0, 1] = sourceRGBGrid[baseX - 1, baseY];
                    sourceGrid3x3[1, 1] = sourceRGBGrid[baseX, baseY];
                    sourceGrid3x3[2, 1] = sourceRGBGrid[baseX + 1, baseY];

                    sourceGrid3x3[0, 2] = sourceRGBGrid[baseX - 1, baseY + 1];
                    sourceGrid3x3[1, 2] = sourceRGBGrid[baseX, baseY + 1];
                    sourceGrid3x3[2, 2] = sourceRGBGrid[baseX + 1, baseY + 1];

                    Upscale3x3Grid(src: sourceGrid3x3, target: upscaledGrid, offsetX: baseX * resizeFactor, offsetY: baseY * resizeFactor);
                }
            }

            return upscaledGrid;
        }

        private static Color[,] Upscale3x3Grid(Color[,] src, Color[,] target, int offsetX, int offsetY)
        {
            // base code, before replacing letters with src locations and adding offset

            //Color A = src[0, 0];
            //Color B = src[1, 0];
            //Color C = src[2, 0];
            //Color D = src[0, 1];
            //Color E = src[1, 1];
            //Color F = src[2, 1];
            //Color G = src[0, 2];
            //Color H = src[1, 2];
            //Color I = src[2, 2];

            //target[0, 0] = D == B && D != H && B != F ? D : E;
            //target[1, 0] = (D == B && D != H && B != F && E != C) || (B == F && B != D && F != H && E != A) ? B : E;
            //target[2, 0] = (B == F && B != D && F != H) ? F : E;
            //target[0, 1] = (H == D && H != F && D != B && E != A) || (D == B && D != H && B != F && E != G) ? D : E;
            //target[1, 1] = E;
            //target[2, 1] = (B == F && B != D && F != H && E != I) || (F == H && F != B && H != D && E != C) ? F : E;
            //target[0, 2] = H == D && H != F && D != B ? D : E;
            //target[1, 2] = (F == H && F != B && H != D && E != G) || (H == D && H != F && D != B && E != I) ? H : E;
            //target[2, 2] = F == H && F != B && H != D ? F : E;

            // optimized code

            target[0 + offsetX, 0 + offsetY] = src[0, 1] == src[1, 0] && src[0, 1] != src[1, 2] && src[1, 0] != src[2, 1] ? src[0, 1] : src[1, 1];
            target[1 + offsetX, 0 + offsetY] = (src[0, 1] == src[1, 0] && src[0, 1] != src[1, 2] && src[1, 0] != src[2, 1] && src[1, 1] != src[2, 0]) || (src[1, 0] == src[2, 1] && src[1, 0] != src[0, 1] && src[2, 1] != src[1, 2] && src[1, 1] != src[0, 0]) ? src[1, 0] : src[1, 1];
            target[2 + offsetX, 0 + offsetY] = (src[1, 0] == src[2, 1] && src[1, 0] != src[0, 1] && src[2, 1] != src[1, 2]) ? src[2, 1] : src[1, 1];
            target[0 + offsetX, 1 + offsetY] = (src[1, 2] == src[0, 1] && src[1, 2] != src[2, 1] && src[0, 1] != src[1, 0] && src[1, 1] != src[0, 0]) || (src[0, 1] == src[1, 0] && src[0, 1] != src[1, 2] && src[1, 0] != src[2, 1] && src[1, 1] != src[0, 2]) ? src[0, 1] : src[1, 1];
            target[1 + offsetX, 1 + offsetY] = src[1, 1];
            target[2 + offsetX, 1 + offsetY] = (src[1, 0] == src[2, 1] && src[1, 0] != src[0, 1] && src[2, 1] != src[1, 2] && src[1, 1] != src[2, 2]) || (src[2, 1] == src[1, 2] && src[2, 1] != src[1, 0] && src[1, 2] != src[0, 1] && src[1, 1] != src[2, 0]) ? src[2, 1] : src[1, 1];
            target[0 + offsetX, 2 + offsetY] = src[1, 2] == src[0, 1] && src[1, 2] != src[2, 1] && src[0, 1] != src[1, 0] ? src[0, 1] : src[1, 1];
            target[1 + offsetX, 2 + offsetY] = (src[2, 1] == src[1, 2] && src[2, 1] != src[1, 0] && src[1, 2] != src[0, 1] && src[1, 1] != src[0, 2]) || (src[1, 2] == src[0, 1] && src[1, 2] != src[2, 1] && src[0, 1] != src[1, 0] && src[1, 1] != src[2, 2]) ? src[1, 2] : src[1, 1];
            target[2 + offsetX, 2 + offsetY] = src[2, 1] == src[1, 2] && src[2, 1] != src[1, 0] && src[1, 2] != src[0, 1] ? src[2, 1] : src[1, 1];

            return target;
        }

    }
}
