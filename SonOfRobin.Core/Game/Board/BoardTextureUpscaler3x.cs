using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class BoardTextureUpscaler3x
    {
        public const int resizeFactor = 3;

        public static Texture2D UpscaleTexture(Texture2D sourceTexture)
        {
            // slow, should be used for debugging only

            Color[,] sourceRGBGrid = GfxConverter.ConvertTextureToGrid(texture: sourceTexture, x: 0, y: 0, width: sourceTexture.Width, height: sourceTexture.Height);
            Color[,] upscaledRGBGrid = UpscaleColorGrid(sourceGrid: sourceRGBGrid);

            return GfxConverter.Convert2DArrayToTexture(upscaledRGBGrid);
        }

        public static Color[,] UpscaleColorGrid(Color[,] sourceGrid)
        {
            int sourceWidth = sourceGrid.GetLength(0);
            int sourceHeight = sourceGrid.GetLength(1);
            int targetWidth = sourceWidth * resizeFactor;
            int targetHeight = sourceHeight * resizeFactor;

            Color[,] upscaledGrid = new Color[targetWidth, targetHeight];

            // filling the whole grid

            List<Point> edgePointList = new List<Point>();

            for (int baseY = 0; baseY < sourceHeight; baseY++)
            {
                int targetOffsetY = baseY * resizeFactor;

                for (int baseX = 0; baseX < sourceWidth; baseX++)
                {
                    try
                    {
                        Upscale3x3Grid(source: sourceGrid, target: upscaledGrid, sourceOffsetX: baseX - 1, sourceOffsetY: baseY - 1, targetOffsetX: baseX * resizeFactor, targetOffsetY: targetOffsetY);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        edgePointList.Add(new Point(baseX, baseY)); // pixels outside the edge will not be found - adding to edge list
                    }
                }
            }

            // filling edges

            Color[,] workingGrid3x3 = new Color[3, 3]; // working grid is needed, because the edges are missing and just using sourceOffset will not work

            foreach (Point point in edgePointList)
            {
                for (int yOffset = 0; yOffset < 3; yOffset++)
                {
                    for (int xOffset = 0; xOffset < 3; xOffset++)
                    {
                        try
                        {
                            workingGrid3x3[xOffset, yOffset] = sourceGrid[point.X + xOffset - 1, point.Y + yOffset - 1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            workingGrid3x3[xOffset, yOffset] = sourceGrid[point.X, point.Y]; // inserting middle value, if the pixel is outside grid bounds
                        }
                    }
                }

                Upscale3x3Grid(source: workingGrid3x3, target: upscaledGrid, targetOffsetX: point.X * resizeFactor, targetOffsetY: point.Y * resizeFactor);
            }

            return upscaledGrid;
        }

        public static Color[,] Upscale3x3Grid(Color[,] source, Color[,] target, int targetOffsetX = 0, int targetOffsetY = 0, int sourceOffsetX = 0, int sourceOffsetY = 0)
        {
            Color leftTop = source[sourceOffsetX, sourceOffsetY];
            Color middleTop = source[1 + sourceOffsetX, sourceOffsetY];
            Color rightTop = source[2 + sourceOffsetX, sourceOffsetY];
            Color leftMiddle = source[sourceOffsetX, 1 + sourceOffsetY];
            Color center = source[1 + sourceOffsetX, 1 + sourceOffsetY];
            Color rightMiddle = source[2 + sourceOffsetX, 1 + sourceOffsetY];
            Color leftBottom = source[sourceOffsetX, 2 + sourceOffsetY];
            Color middleBottom = source[1 + sourceOffsetX, 2 + sourceOffsetY];
            Color rightBottom = source[2 + sourceOffsetX, 2 + sourceOffsetY];

            target[targetOffsetX, targetOffsetY] = leftMiddle == middleTop && leftMiddle != middleBottom && middleTop != rightMiddle ? leftMiddle : center;
            target[1 + targetOffsetX, targetOffsetY] = (leftMiddle == middleTop && leftMiddle != middleBottom && middleTop != rightMiddle && center != rightTop) || (middleTop == rightMiddle && middleTop != leftMiddle && rightMiddle != middleBottom && center != leftTop) ? middleTop : center;
            target[2 + targetOffsetX, targetOffsetY] = (middleTop == rightMiddle && middleTop != leftMiddle && rightMiddle != middleBottom) ? rightMiddle : center;
            target[targetOffsetX, 1 + targetOffsetY] = (middleBottom == leftMiddle && middleBottom != rightMiddle && leftMiddle != middleTop && center != leftTop) || (leftMiddle == middleTop && leftMiddle != middleBottom && middleTop != rightMiddle && center != leftBottom) ? leftMiddle : center;
            target[1 + targetOffsetX, 1 + targetOffsetY] = center;
            target[2 + targetOffsetX, 1 + targetOffsetY] = (middleTop == rightMiddle && middleTop != leftMiddle && rightMiddle != middleBottom && center != rightBottom) || (rightMiddle == middleBottom && rightMiddle != middleTop && middleBottom != leftMiddle && center != rightTop) ? rightMiddle : center;
            target[targetOffsetX, 2 + targetOffsetY] = middleBottom == leftMiddle && middleBottom != rightMiddle && leftMiddle != middleTop ? leftMiddle : center;
            target[1 + targetOffsetX, 2 + targetOffsetY] = (rightMiddle == middleBottom && rightMiddle != middleTop && middleBottom != leftMiddle && center != leftBottom) || (middleBottom == leftMiddle && middleBottom != rightMiddle && leftMiddle != middleTop && center != rightBottom) ? middleBottom : center;
            target[2 + targetOffsetX, 2 + targetOffsetY] = rightMiddle == middleBottom && rightMiddle != middleTop && middleBottom != leftMiddle ? rightMiddle : center;

            return target;
        }
    }
}