using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimFrame
    {
        public readonly string atlasName;
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
        public readonly bool ignoreWhenCalculatingMaxSize;
        public readonly Texture2D texture;
        public readonly Vector2 textureSize;
        public readonly Rectangle textureRect;
        public readonly Vector2 rotationOrigin;

        public static AnimFrame GetFrame(string atlasName, int atlasX, int atlasY, int width, int height, byte layer, byte duration, bool crop = false, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            // some frames are duplicated and can be reused (this can be verified by checking ID)

            string id = GetID(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent);

            if (AnimData.frameById.ContainsKey(id)) return AnimData.frameById[id];
            else return new AnimFrame(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize);
        }

        private static string GetID(string atlasName, int atlasX, int atlasY, int width, int height, byte layer, byte duration, bool crop, float scale, float depthPercent)
        {
            return $"{atlasName}_{atlasX},{atlasY}_{width}x{height}_{layer}_{duration}_{crop}_{scale}_{depthPercent}";
        }

        private AnimFrame(string atlasName, int atlasX, int atlasY, int width, int height, byte layer, byte duration, bool crop, float scale, float depthPercent, int padding, bool ignoreWhenCalculatingMaxSize)
        {
            // should not be invoked from other classes directly

            this.id = GetID(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent);

            AnimData.frameById[this.id] = this;

            this.depthPercent = depthPercent;
            this.atlasName = atlasName;
            this.scale = scale;
            this.layer = layer;
            this.duration = duration; // duration == 0 will stop the animation
            this.ignoreWhenCalculatingMaxSize = ignoreWhenCalculatingMaxSize;

            Texture2D atlasTexture = SonOfRobinGame.textureByName[this.atlasName];
            Rectangle cropRect = GetCropRect(texture: atlasTexture, textureX: atlasX, textureY: atlasY, width: width, height: height, crop: crop);

            // padding makes the edge texture filtering smooth and allows for border effects outside original texture edges
            this.texture = GfxConverter.CropTextureAndAddPadding(baseTexture: atlasTexture, cropRect: cropRect, padding: padding);

            this.textureSize = new Vector2(this.texture.Width, this.texture.Height);
            this.textureRect = new Rectangle(x: 0, y: 0, width: this.texture.Width, height: this.texture.Height);
            this.rotationOrigin = new Vector2(this.textureSize.X * 0.5f, this.textureSize.Y * 0.5f); // rotationOrigin must not take scale into account, to work properly

            Rectangle colBounds = this.FindCollisionBounds();

            this.colWidth = (int)(colBounds.Width * scale);
            this.colHeight = (int)(colBounds.Height * scale);

            this.gfxWidth = (int)(this.texture.Width * scale);
            this.gfxHeight = (int)(this.texture.Height * scale);

            this.colOffset = new Vector2(-(int)(colBounds.Width * 0.5f), -(int)(colBounds.Height * 0.5f)); // has to go first...
            this.gfxOffset = new Vector2(this.colOffset.X - colBounds.X, this.colOffset.Y - colBounds.Y); // because it is used here

            this.colOffset *= scale;
            this.gfxOffset *= scale;
        }

        public static void DeleteUsedAtlases()
        {
            // Should be used after loading textures from all atlasses.
            // Deleted textures will not be available for use any longer.

            var usedAtlasNames = new List<string>();

            foreach (List<AnimFrame> frameList in AnimData.frameListById.Values)
            {
                foreach (AnimFrame animFrame in frameList)
                {
                    if (animFrame.atlasName != null && !usedAtlasNames.Contains(animFrame.atlasName)) usedAtlasNames.Add(animFrame.atlasName);
                }
            }

            foreach (string atlasName in usedAtlasNames)
            {
                SonOfRobinGame.textureByName[atlasName].Dispose();
                SonOfRobinGame.textureByName.Remove(atlasName);
            }
        }

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
            Color[] colorData = new Color[width * height];
            Rectangle extractRegion = new Rectangle(textureX, textureY, width, height);
            texture.GetData<Color>(0, extractRegion, colorData, 0, width * height);

            int xMin = width;
            int xMax = 0;
            int yMin = height;
            int yMax = 0;

            bool opaquePixelsFound = false;
            int[] alphaValues = { minAlpha, 1 }; // if there are no desired alpha values, minimum value is used instead

            foreach (int currentMinAlpha in alphaValues)
            {
                for (int y = 0; y < height; y++)
                {
                    int yMultipliedInput = y * width;

                    for (int x = 0; x < width; x++)
                    {
                        Color pixel = colorData[yMultipliedInput + x];

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
                // first pass - whole sprite visible through water
                SonOfRobinGame.SpriteBatch.Draw(this.texture, destRect, this.textureRect, Color.Blue * opacity * 0.2f);

                correctedSourceHeight = Math.Max(this.texture.Height / 2, this.texture.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle sourceRectangle = new Rectangle(x: 0, y: 0, width: this.texture.Width, correctedSourceHeight);

            // Helpers.DrawRectangleOutline(rect: destRect, color: Color.YellowGreen, borderWidth: 2); // testing rect size

            SonOfRobinGame.SpriteBatch.Draw(this.texture, destRect, sourceRectangle, color * opacity);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity, Vector2 rotationOriginOverride, bool rotationOriginPosCorrection = false)
        {
            // invoke from Sprite class

            Vector2 rotationOriginToUse = this.rotationOrigin;

            if (rotationOriginOverride != Vector2.Zero)
            {
                rotationOriginToUse = rotationOriginOverride;
                if (rotationOriginPosCorrection) position += (rotationOriginToUse - this.rotationOrigin) * this.scale;
            }

            SonOfRobinGame.SpriteBatch.Draw(this.texture, position: position, sourceRectangle: this.textureRect, color: color * opacity, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: SpriteEffects.None, layerDepth: 0);
        }

        public void DrawAndKeepInRectBounds(Rectangle destBoundsRect, Color color, float opacity = 1f)
        {
            // general use

            Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: destBoundsRect, color: color * opacity);
        }
    }
}