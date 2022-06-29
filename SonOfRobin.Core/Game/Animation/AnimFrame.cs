using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimFrame
    {
        public readonly string id;
        private readonly Texture2D atlasTexture;
        private readonly ushort atlasX;
        private readonly ushort atlasY;
        public readonly ushort gfxWidth;
        public readonly ushort gfxHeight;
        public readonly ushort colWidth;
        public readonly ushort colHeight;
        public readonly byte layer;
        public readonly byte duration;
        public readonly Vector2 gfxOffset;
        public readonly Vector2 colOffset;

        public static List<byte> allLayers = new List<byte> { }; // needed to draw frames in correct order
        public static Dictionary<string, AnimFrame> frameById = new Dictionary<string, AnimFrame>(); // needed to access frames directly by id (for loading and saving game)


        public AnimFrame(string atlasName, ushort atlasX, ushort atlasY, ushort width, ushort height, byte layer, byte duration, bool crop = false)
        {
            this.id = $"{atlasName}_{atlasX},{atlasY}_{width}x{height}_{layer}_{duration}_{crop}";
            frameById[this.id] = this;

            this.atlasTexture = SonOfRobinGame.textureByName[atlasName];

            var cropData = CropFrame(texture: this.atlasTexture, textureX: atlasX, textureY: atlasY, width: width, height: height, crop: crop);
            atlasX = cropData["croppedAtlasX"];
            atlasY = cropData["croppedAtlasY"];
            width = cropData["croppedWidth"];
            height = cropData["croppedHeight"];

            this.atlasX = atlasX;
            this.atlasY = atlasY;
            this.layer = layer;
            if (!allLayers.Contains(layer))
            {
                allLayers.Add(layer);
                allLayers.Sort();
            }

            var colBounds = this.FindCollisionBounds(croppedAtlasX: atlasX, croppedAtlasY: atlasY, croppedWidth: width, croppedHeight: height);
            var colXMin = colBounds[0]; var colXMax = colBounds[1]; var colYMin = colBounds[2]; var colYMax = colBounds[3];

            this.colWidth = Convert.ToUInt16(colXMax - colXMin + 1);
            this.colHeight = Convert.ToUInt16(colYMax - colYMin + 1);
            this.colOffset = new Vector2(-Convert.ToInt32(this.colWidth * 0.5f), -Convert.ToInt32(this.colHeight * 0.5f));

            this.gfxWidth = width;
            this.gfxHeight = height;

            this.gfxOffset = new Vector2(
                this.colOffset.X - (colXMin - atlasX),
                this.colOffset.Y - (colYMin - atlasY));

            this.duration = duration; // duration == 0 will stop the animation
        }

        public static AnimFrame GetFrameById(string frameId)
        { return frameById[frameId]; }

        private ushort[] FindCollisionBounds(ushort croppedAtlasX, ushort croppedAtlasY, ushort croppedWidth, ushort croppedHeight)
        {
            // checking bottom part of the frame for collision bounds

            ushort bottomPartHeight = Math.Max(Convert.ToUInt16(croppedHeight * 0.25), (ushort)20);
            bottomPartHeight = Math.Min(bottomPartHeight, croppedHeight);

            ushort sliceWidth = croppedWidth;
            ushort sliceHeight = (this.layer == 1) ? bottomPartHeight : croppedHeight;
            ushort sliceX = croppedAtlasX;
            ushort sliceY = Convert.ToUInt16(croppedAtlasY + (croppedHeight - sliceHeight));

            var bounds = FindBoundaries(texture: this.atlasTexture, textureX: sliceX, textureY: sliceY, width: sliceWidth, height: sliceHeight, minAlpha: 240);
            // bounds value would be incorrect without adding the base slice value
            bounds[0] += sliceX;
            bounds[1] += sliceX;
            bounds[2] += sliceY;
            bounds[3] += sliceY;

            return bounds;
        }

        private static ushort[] FindBoundaries(Texture2D texture, ushort textureX, ushort textureY, ushort width, ushort height, int minAlpha)
        {
            var rawDataAsGrid = ConvertTextureToGrid(texture: texture, x: textureX, y: textureY, width: width, height: height);

            ushort xMin = (ushort)(width - 1); ushort xMax = 0; ushort yMin = (ushort)(height - 1); ushort yMax = 0;

            bool opaquePixelsFound = false;

            int[] alphaValues = { minAlpha, 1 }; // if there are no desired alpha values, minimum value is used instead
            foreach (int currentMinAlpha in alphaValues)

            {
                for (int x = 0; x <= width - 1; x++)
                {
                    for (int y = 0; y <= height - 1; y++)
                    {
                        Color pixel = rawDataAsGrid[y, x];
                        if (pixel.A >= minAlpha) // looking for non-transparent pixels
                        {
                            opaquePixelsFound = true;
                            if (x < xMin) xMin = Convert.ToUInt16(x);
                            if (x > xMax) xMax = Convert.ToUInt16(x);
                            if (y < yMin) yMin = Convert.ToUInt16(y);
                            if (y > yMax) yMax = Convert.ToUInt16(y);
                        }
                    }
                }

                if (opaquePixelsFound) { break; }
            }

            if (!opaquePixelsFound) // resetting to the whole size, if bounds could not be found
            { xMin = 0; xMax = Convert.ToUInt16(width - 1); yMin = 0; yMax = Convert.ToUInt16(height - 1); }

            ushort[] boundsList = { xMin, xMax, yMin, yMax };
            return boundsList;
        }

        public static Dictionary<string, ushort> CropFrame(Texture2D texture, ushort textureX, ushort textureY, ushort width, ushort height, bool crop)
        {
            var cropData = new Dictionary<string, ushort>() {
                { "croppedAtlasX", textureX },
                { "croppedAtlasY", textureY },
                { "croppedWidth", width },
                { "croppedHeight", height } };

            if (crop)
            {
                var croppedBounds = FindBoundaries(texture: texture, textureX: textureX, textureY: textureY, width: width, height: height, minAlpha: 1);

                var croppedXMin = croppedBounds[0]; var gfxXMax = croppedBounds[1]; var gfxYMin = croppedBounds[2]; var gfxYMax = croppedBounds[3];
                var croppedWidth = Convert.ToUInt16((gfxXMax - croppedXMin) + 1);
                var croppedHeight = Convert.ToUInt16((gfxYMax - gfxYMin) + 1);
                var croppedAtlasX = Convert.ToUInt16(textureX + croppedXMin);
                var croppedAtlasY = Convert.ToUInt16(textureY + gfxYMin);

                cropData = new Dictionary<string, ushort>() {
                    { "croppedAtlasX", croppedAtlasX },
                    { "croppedAtlasY", croppedAtlasY },
                    { "croppedWidth", croppedWidth },
                    { "croppedHeight", croppedHeight }};
            }

            return cropData;
        }

        public static Color[,] ConvertTextureToGrid(Texture2D texture, ushort x, ushort y, ushort width, ushort height)
        {
            // getting 1D pixel array
            Color[] rawData = new Color[width * height];
            Rectangle extractRegion = new Rectangle(x, y, width, height);
            texture.GetData<Color>(0, extractRegion, rawData, 0, width * height);

            // getting 2D pixel grid
            Color[,] rawDataAsGrid = new Color[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    // Assumes row major ordering of the array.
                    rawDataAsGrid[row, column] = rawData[row * width + column];
                }
            }
            return rawDataAsGrid;
        }

        public void Draw(Rectangle gfxRect, Color color)
        {
            // invoke from Sprite class
            Rectangle sourceRectangle = new Rectangle(this.atlasX, this.atlasY, gfxRect.Width, gfxRect.Height);
            Rectangle destinationRectangle = new Rectangle(gfxRect.X, gfxRect.Y, gfxRect.Width, gfxRect.Height);

            SonOfRobinGame.spriteBatch.Draw(this.atlasTexture, destinationRectangle, sourceRectangle, color);
        }

        public void DrawAndKeepInRectBounds(Rectangle destBoundsRect, Color color)
        {
            // general use

            Rectangle sourceRect = new Rectangle(this.atlasX, this.atlasY, this.gfxWidth, gfxHeight);

            int destBoundsShorterSide = Math.Min(destBoundsRect.Width, destBoundsRect.Height);
            int sourceLongerSide = Math.Max(sourceRect.Width, sourceRect.Height);
            float sourceScale = (float)destBoundsShorterSide / (float)sourceLongerSide;

            int destWidth = Convert.ToInt32((float)sourceRect.Width * (float)sourceScale);
            int destHeight = Convert.ToInt32((float)sourceRect.Height * (float)sourceScale);

            Rectangle destRect = new Rectangle(destBoundsRect.X, destBoundsRect.Y, destWidth, destHeight);

            destRect.X += Convert.ToInt32(((float)destBoundsRect.Width - (float)destRect.Width) / 2f);
            destRect.Y += Convert.ToInt32(((float)destBoundsRect.Height - (float)destRect.Height) / 2f);

            //Helpers.DrawRectangleOutline(rect: destRect, color: Color.YellowGreen, borderWidth: 2); // testing rect size

            SonOfRobinGame.spriteBatch.Draw(texture: this.atlasTexture, destinationRectangle: destRect, sourceRectangle: sourceRect, color: color);
        }

    }
}
