using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct AnimFrameTemplateData
    {
        // used to store anim frame data needed for further processing

        public readonly int atlasX;
        public readonly int atlasY;
        public readonly int width;
        public readonly int height;
        public readonly bool crop;
        public readonly int padding;

        public AnimFrameTemplateData(int atlasX, int atlasY, int width, int height, bool crop, int padding)
        {
            this.atlasX = atlasX;
            this.atlasY = atlasY;
            this.width = width;
            this.height = height;
            this.crop = crop;
            this.padding = padding;
        }
    }

    public class AnimFrame
    {
        public readonly string atlasName;
        private readonly AnimFrameTemplateData animFrameTemplateData;
        public Texture2D Texture { get; private set; }
        public Vector2 RotationOrigin { get; private set; }
        public Vector2 TextureSize { get; private set; }
        public Rectangle TextureRect { get; private set; }
        public bool HasBeenProcessed { get; private set; }
        public int GfxWidth { get; private set; }
        public int GfxHeight { get; private set; }
        public int ColWidth { get; private set; }
        public int ColHeight { get; private set; }
        public Vector2 GfxOffset { get; private set; }
        public Vector2 ColOffset { get; private set; }

        public readonly string id;
        private readonly float depthPercent;
        public readonly byte layer;
        public readonly byte duration;
        public readonly float scale;
        public readonly bool ignoreWhenCalculatingMaxSize;

        public static AnimFrame GetFrame(string atlasName, int atlasX, int atlasY, int width, int height, byte layer, byte duration, bool crop = false, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            // Some frames are duplicated and can be reused.

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
            this.HasBeenProcessed = false;
            this.id = GetID(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent);

            AnimData.frameById[this.id] = this;
            this.animFrameTemplateData = new AnimFrameTemplateData(atlasX: atlasX, atlasY: atlasY, width: width, height: height, crop: crop, padding: padding);

            this.depthPercent = depthPercent;
            this.atlasName = atlasName;
            this.scale = scale;
            this.layer = layer;
            this.duration = duration; // duration == 0 will stop the animation
            this.ignoreWhenCalculatingMaxSize = ignoreWhenCalculatingMaxSize;
        }

        public void Process()
        {
            if (this.HasBeenProcessed) throw new ArgumentException($"AnimFrame has already been processed - {this.id}.");

            Texture2D atlasTexture = SonOfRobinGame.textureByName[this.atlasName];
            Rectangle cropRect = GetCropRect(texture: atlasTexture, textureX: this.animFrameTemplateData.atlasX, textureY: this.animFrameTemplateData.atlasY, width: this.animFrameTemplateData.width, height: this.animFrameTemplateData.height, crop: this.animFrameTemplateData.crop);

            // padding makes the edge texture filtering smooth and allows for border effects outside original texture edges
            this.Texture = GfxConverter.CropTextureAndAddPadding(baseTexture: atlasTexture, cropRect: cropRect, padding: this.animFrameTemplateData.padding);

            this.RotationOrigin = new Vector2(this.Texture.Width / 2f, this.Texture.Height / 2f);

            this.TextureSize = new Vector2(this.Texture.Width, this.Texture.Height);
            this.TextureRect = new Rectangle(x: 0, y: 0, width: this.Texture.Width, height: this.Texture.Height);

            Rectangle colBounds = this.FindCollisionBounds();

            this.ColWidth = (int)(colBounds.Width * scale);
            this.ColHeight = (int)(colBounds.Height * scale);

            this.GfxWidth = (int)(this.Texture.Width * scale);
            this.GfxHeight = (int)(this.Texture.Height * scale);

            this.ColOffset = new Vector2(-(int)(colBounds.Width * 0.5f), -(int)(colBounds.Height * 0.5f)); // has to go first...
            this.GfxOffset = new Vector2(this.ColOffset.X - colBounds.X, this.ColOffset.Y - colBounds.Y); // because it is used here

            this.ColOffset *= scale;
            this.GfxOffset *= scale;

            this.HasBeenProcessed = true;
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

            int bottomPartHeight = Math.Max((int)(this.Texture.Width * this.depthPercent), 20);
            bottomPartHeight = Math.Min(bottomPartHeight, this.Texture.Height);

            int sliceWidth = this.Texture.Width;
            int sliceHeight = this.layer == 1 ? bottomPartHeight : this.Texture.Height;
            int sliceX = 0;
            int sliceY = this.Texture.Height - sliceHeight;

            var boundsRect = FindNonTransparentPixelsRect(texture: this.Texture, textureX: sliceX, textureY: sliceY, width: sliceWidth, height: sliceHeight, minAlpha: 240); // 240

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

            int correctedSourceHeight = this.Texture.Height;
            if (submergeCorrection > 0)
            {
                // first pass - whole sprite visible through water
                SonOfRobinGame.SpriteBatch.Draw(this.Texture, destRect, this.TextureRect, Color.Blue * opacity * 0.2f);

                correctedSourceHeight = Math.Max(this.Texture.Height / 2, this.Texture.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle sourceRectangle = new Rectangle(x: 0, y: 0, width: this.Texture.Width, correctedSourceHeight);

            // Helpers.DrawRectangleOutline(rect: destRect, color: Color.YellowGreen, borderWidth: 2); // testing rect size

            SonOfRobinGame.SpriteBatch.Draw(this.Texture, destRect, sourceRectangle, color * opacity);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity)
        {
            // invoke from Sprite class

            SonOfRobinGame.SpriteBatch.Draw(this.Texture, position: position, sourceRectangle: this.TextureRect, color: color * opacity, rotation: rotation, origin: this.RotationOrigin, scale: this.scale, effects: SpriteEffects.None, layerDepth: 0);
        }

        public void DrawAndKeepInRectBounds(Rectangle destBoundsRect, Color color, float opacity = 1f)
        {
            // general use

            Helpers.DrawTextureInsideRect(texture: this.Texture, rectangle: destBoundsRect, color: color * opacity);
        }
    }
}