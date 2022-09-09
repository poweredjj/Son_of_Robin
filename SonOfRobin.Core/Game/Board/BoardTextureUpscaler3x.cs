﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class BoardTextureUpscaler3x
    {
        private static readonly int resizeFactor = 3;

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

            Color[,] workingGrid3x3 = new Color[3, 3];
            Color[,] upscaledGrid = new Color[targetWidth, targetHeight];

            // filling the middle

            for (int baseY = 1; baseY < sourceHeight - 1; baseY++)
            {
                for (int baseX = 1; baseX < sourceWidth - 1; baseX++)
                {
                    workingGrid3x3[0, 0] = sourceGrid[baseX - 1, baseY - 1];
                    workingGrid3x3[1, 0] = sourceGrid[baseX, baseY - 1];
                    workingGrid3x3[2, 0] = sourceGrid[baseX + 1, baseY - 1];

                    workingGrid3x3[0, 1] = sourceGrid[baseX - 1, baseY];
                    workingGrid3x3[1, 1] = sourceGrid[baseX, baseY];
                    workingGrid3x3[2, 1] = sourceGrid[baseX + 1, baseY];

                    workingGrid3x3[0, 2] = sourceGrid[baseX - 1, baseY + 1];
                    workingGrid3x3[1, 2] = sourceGrid[baseX, baseY + 1];
                    workingGrid3x3[2, 2] = sourceGrid[baseX + 1, baseY + 1];

                    Upscale3x3Grid(src: workingGrid3x3, target: upscaledGrid, targetOffsetX: baseX * resizeFactor, targetOffsetY: baseY * resizeFactor);
                }
            }

            List<Point> pointList = new List<Point>();

            // making edges coordinates list

            for (int baseX = 0; baseX < sourceWidth; baseX++)
            {
                foreach (int baseY in new int[] { 0, sourceHeight - 1 })
                {
                    Point newPoint = new Point(baseX, baseY);
                    if (!pointList.Contains(newPoint)) pointList.Add(newPoint);
                }
            }

            for (int baseY = 0; baseY < sourceHeight; baseY++)
            {
                foreach (int baseX in new int[] { 0, sourceWidth - 1 })
                {
                    Point newPoint = new Point(baseX, baseY);
                    if (!pointList.Contains(newPoint)) pointList.Add(newPoint);
                }
            }

            // filling edges

            foreach (Point point in pointList)
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

                Upscale3x3Grid(src: workingGrid3x3, target: upscaledGrid, targetOffsetX: point.X * resizeFactor, targetOffsetY: point.Y * resizeFactor);
            }

            return upscaledGrid;
        }

        private static Color[,] Upscale3x3Grid(Color[,] src, Color[,] target, int targetOffsetX, int targetOffsetY, int sourceOffsetX = 0, int sourceOffsetY = 0)
        {
            Color leftTop = src[0 + sourceOffsetX, 0 + sourceOffsetY];
            Color middleTop = src[1 + sourceOffsetX, 0 + sourceOffsetY];
            Color rightTop = src[2 + sourceOffsetX, 0 + sourceOffsetY];
            Color leftMiddle = src[0 + sourceOffsetX, 1 + sourceOffsetY];
            Color center = src[1 + sourceOffsetX, 1 + sourceOffsetY];
            Color rightMiddle = src[2 + sourceOffsetX, 1 + sourceOffsetY];
            Color leftBottom = src[0 + sourceOffsetX, 2 + sourceOffsetY];
            Color middleBottom = src[1 + sourceOffsetX, 2 + sourceOffsetY];
            Color rightBottom = src[2 + sourceOffsetX, 2 + sourceOffsetY];

            target[0 + targetOffsetX, 0 + targetOffsetY] = leftMiddle == middleTop && leftMiddle != middleBottom && middleTop != rightMiddle ? leftMiddle : center;
            target[1 + targetOffsetX, 0 + targetOffsetY] = (leftMiddle == middleTop && leftMiddle != middleBottom && middleTop != rightMiddle && center != rightTop) || (middleTop == rightMiddle && middleTop != leftMiddle && rightMiddle != middleBottom && center != leftTop) ? middleTop : center;
            target[2 + targetOffsetX, 0 + targetOffsetY] = (middleTop == rightMiddle && middleTop != leftMiddle && rightMiddle != middleBottom) ? rightMiddle : center;
            target[0 + targetOffsetX, 1 + targetOffsetY] = (middleBottom == leftMiddle && middleBottom != rightMiddle && leftMiddle != middleTop && center != leftTop) || (leftMiddle == middleTop && leftMiddle != middleBottom && middleTop != rightMiddle && center != leftBottom) ? leftMiddle : center;
            target[1 + targetOffsetX, 1 + targetOffsetY] = center;
            target[2 + targetOffsetX, 1 + targetOffsetY] = (middleTop == rightMiddle && middleTop != leftMiddle && rightMiddle != middleBottom && center != rightBottom) || (rightMiddle == middleBottom && rightMiddle != middleTop && middleBottom != leftMiddle && center != rightTop) ? rightMiddle : center;
            target[0 + targetOffsetX, 2 + targetOffsetY] = middleBottom == leftMiddle && middleBottom != rightMiddle && leftMiddle != middleTop ? leftMiddle : center;
            target[1 + targetOffsetX, 2 + targetOffsetY] = (rightMiddle == middleBottom && rightMiddle != middleTop && middleBottom != leftMiddle && center != leftBottom) || (middleBottom == leftMiddle && middleBottom != rightMiddle && leftMiddle != middleTop && center != rightBottom) ? middleBottom : center;
            target[2 + targetOffsetX, 2 + targetOffsetY] = rightMiddle == middleBottom && rightMiddle != middleTop && middleBottom != leftMiddle ? rightMiddle : center;

            return target;
        }

    }
}
