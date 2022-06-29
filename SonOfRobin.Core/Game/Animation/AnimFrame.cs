using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimFrame
    {
        public static readonly List<byte> allLayers = new List<byte> { }; // needed to draw frames in correct order
        public static readonly List<string> usedAtlasNames = new List<string>(); // for deleting used atlases after creating cropped textures

        public readonly Texture2D texture;
        public readonly Vector2 textureSize;
        public readonly Rectangle textureRect;
        public readonly Vector2 rotationOrigin;

        public readonly string id;
        private readonly float depthPercent;
        public readonly int gfxWidth;
        public readonly int gfxHeight;
        public readonly int colWidth;
        public readonly int colHeight;
        public readonly Vector2 gfxOffset;
        public readonly Vector2 colOffset;
        public readonly byte layer;
        public readonly byte duration;
        public readonly float scale;

        public AnimFrame(string atlasName, int atlasX, int atlasY, int width, int height, byte layer, byte duration, bool crop = false, float scale = 1f, float depthPercent = 0.25f, int padding = 1)
        {
            this.id = $"{atlasName}_{atlasX},{atlasY}_{width}x{height}_{layer}_{duration}_{crop}_{scale}_{depthPercent}";
            AnimData.frameById[this.id] = this;

            if (!usedAtlasNames.Contains(atlasName)) usedAtlasNames.Add(atlasName);

            this.depthPercent = depthPercent;

            Texture2D atlasTexture = SonOfRobinGame.textureByName[atlasName];
            Rectangle cropRect = GetCropRect(texture: atlasTexture, textureX: atlasX, textureY: atlasY, width: width, height: height, crop: crop);

            // padding makes the edge texture filtering smooth and allows for border effects outside original texture edges
            this.texture = GfxConverter.CropTextureAndAddPadding(baseTexture: atlasTexture, cropRect: cropRect, padding: padding);

            this.rotationOrigin = new Vector2(this.texture.Width / 2f, this.texture.Height / 2f);

            this.textureSize = new Vector2(this.texture.Width, this.texture.Height);
            this.textureRect = new Rectangle(x: 0, y: 0, width: this.texture.Width, height: this.texture.Height);

            this.scale = scale;
            this.layer = layer;
            if (!allLayers.Contains(layer))
            {
                allLayers.Add(layer);
                allLayers.Sort();
            }

            var colBounds = this.FindCollisionBounds();

            this.colWidth = (int)(colBounds.Width * scale);
            this.colHeight = (int)(colBounds.Height * scale);

            this.gfxWidth = (int)(this.texture.Width * scale);
            this.gfxHeight = (int)(this.texture.Height * scale);

            this.colOffset = new Vector2(-(int)(colBounds.Width * 0.5f), -(int)(colBounds.Height * 0.5f)); // has to go first...
            this.gfxOffset = new Vector2(this.colOffset.X - colBounds.X, this.colOffset.Y - colBounds.Y); // because it is used here

            this.colOffset *= scale;
            this.gfxOffset *= scale;

            this.duration = duration; // duration == 0 will stop the animation
        }

        public static void DeleteUsedAtlases()
        {
            // Should be used after loading textures from all atlases.
            // Deleted textures will not be available for use any longer.

            foreach (string atlasName in usedAtlasNames)
            {
                SonOfRobinGame.textureByName[atlasName].Dispose();
                SonOfRobinGame.textureByName.Remove(atlasName);
            }
        }

        public static AnimFrame GetFrameById(string frameId)
        { return AnimData.frameById[frameId]; }

        private Rectangle FindCollisionBounds()
        {
            // checking bottom part of the frame for collision bounds

            int bottomPartHeight = Math.Max((int)(this.texture.Width * this.depthPercent), 20);
            bottomPartHeight = Math.Min(bottomPartHeight, this.texture.Height);

            int sliceWidth = this.texture.Width;
            int sliceHeight = this.layer == 1 ? bottomPartHeight : this.texture.Height;
            int sliceX = 0;
            int sliceY = this.texture.Height - sliceHeight;

            var boundsRect = FindNonTransparentPixelsRect(texture: this.texture, textureX: sliceX, textureY: sliceY, width: sliceWidth, height: sliceHeight, minAlpha: 240); // 240
            // bounds value would be incorrect without adding the base slice value

            boundsRect.X += sliceX;
            boundsRect.Y += sliceY;

            return boundsRect;
        }

        private static Rectangle FindNonTransparentPixelsRect(Texture2D texture, int textureX, int textureY, int width, int height, int minAlpha)
        {
            var rawDataAsGrid = GfxConverter.ConvertTextureToGrid(texture: texture, x: textureX, y: textureY, width: width, height: height);

            int xMin = width;
            int xMax = 0;
            int yMin = height;
            int yMax = 0;

            bool opaquePixelsFound = false;
            Color pixel;
            int[] alphaValues = { minAlpha, 1 }; // if there are no desired alpha values, minimum value is used instead

            foreach (int currentMinAlpha in alphaValues)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        pixel = rawDataAsGrid[y, x];
                        if (pixel.A >= minAlpha) // looking for non-transparent pixels
                        {
                            opaquePixelsFound = true;
                            if (x < xMin) xMin = x;
                            if (x > xMax) xMax = x;
                            if (y < yMin) yMin = y;
                            if (y > yMax) yMax = y;
                        }
                    }
                }

                if (opaquePixelsFound) break;
            }

            if (!opaquePixelsFound) // resetting to the whole size, if bounds could not be found
            {
                xMin = 0;
                xMax = width;
                yMin = 0;
                yMax = height;
            }

            return new Rectangle(x: xMin, y: yMin, width: xMax - xMin + 1, height: yMax - yMin + 1);
        }

        public static Rectangle GetCropRect(Texture2D texture, int textureX, int textureY, int width, int height, bool crop)
        {
            if (crop)
            {
                var croppedBounds = FindNonTransparentPixelsRect(texture: texture, textureX: textureX, textureY: textureY, width: width, height: height, minAlpha: 1);
                croppedBounds.X += textureX;
                croppedBounds.Y += textureY;
                return croppedBounds;
            }
            else return new Rectangle(x: textureX, y: textureY, width: width, height: height);
        }

        public void Draw(Rectangle destRect, Color color, float opacity, int submergeCorrection = 0)
        {
            // invoke from Sprite class

            int correctedSourceHeight = this.texture.Height;
            if (submergeCorrection > 0)
            {
                correctedSourceHeight = Math.Max(this.texture.Height / 2, this.texture.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle sourceRectangle = new Rectangle(x: 0, y: 0, width: this.texture.Width, correctedSourceHeight);
            Rectangle destinationRectangle = new Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height);

            // Helpers.DrawRectangleOutline(rect: destinationRectangle, color: Color.YellowGreen, borderWidth: 2); // testing rect size

            SonOfRobinGame.spriteBatch.Draw(this.texture, destinationRectangle, sourceRectangle, color * opacity);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity)
        {
            // invoke from Sprite class

            SonOfRobinGame.spriteBatch.Draw(this.texture, position: position, sourceRectangle: this.textureRect, color: color * opacity, rotation: rotation, origin: this.rotationOrigin, scale: this.scale, effects: SpriteEffects.None, layerDepth: 0);
        }

        public void DrawAndKeepInRectBounds(Rectangle destBoundsRect, Color color, float opacity = 1f)
        {
            // general use

            Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: destBoundsRect, color: color * opacity);
        }

    }
}
