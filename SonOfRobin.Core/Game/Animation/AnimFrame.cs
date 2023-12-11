using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class AnimFrame
    {
        public readonly string atlasName;
        public readonly string id;
        public readonly string textureID; // some id parameters are irrevelant to the texture itself, so alternative id is used
        private readonly float depthPercent;

        public readonly int gfxWidth;
        public readonly int gfxHeight;
        public readonly int colWidth;
        public readonly int colHeight;
        public readonly Vector2 gfxOffset;
        public readonly Vector2 colOffset;
        private readonly string pngPath;
        public readonly Vector2 textureSize;
        public readonly Rectangle textureRect;
        public readonly Vector2 rotationOrigin;

        public readonly int layer;
        public readonly short duration;
        public readonly float scale;
        public readonly bool ignoreWhenCalculatingMaxSize;
        private Texture2D texture;

        public readonly bool cropped;
        public readonly int srcAtlasX;
        public readonly int srcAtlasY;
        public readonly int srcWidth;
        public readonly int srcHeight;
        private bool PngPathExists { get { return File.Exists(this.pngPath); } }

        public Texture2D Texture
        {
            get
            {
                if (this.texture == null)
                {
                    MessageLog.Add(debugMessage: true, text: $"Loading anim frame: {Path.GetFileName(this.pngPath)}...");
                    this.texture = GfxConverter.LoadTextureFromPNG(this.pngPath);
                    if (this.texture == null)
                    {
                        this.texture = TextureBank.GetTexture(TextureBank.TextureName.GfxBroken);
                        AnimData.DeleteJson(); // deleting json to force rebuild at next restart
                    }
                }
                return this.texture;
            }
        }

        public static AnimFrame GetFrame(string atlasName, int atlasX, int atlasY, int width, int height, int layer, short duration, bool crop = false, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            // some frames are duplicated and can be reused (this can be verified by checking ID)

            string id = GetID(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent);

            if (AnimData.frameById.ContainsKey(id)) return AnimData.frameById[id];

            Dictionary<string, Object> jsonData = null;
            try
            { jsonData = (Dictionary<string, Object>)AnimData.jsonDict[id]; }
            catch (InvalidCastException) { }
            catch (KeyNotFoundException) { }

            if (jsonData != null)
            {
                AnimFrame deserializedFrame = new(jsonData);
                if (deserializedFrame.PngPathExists) return deserializedFrame;
            }

            return new AnimFrame(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize);
        }

        private static string GetID(string atlasName, int atlasX, int atlasY, int width, int height, int layer, int duration, bool crop, float scale, float depthPercent)
        {
            return $"{atlasName.Replace("/", "+")}_{atlasX},{atlasY}_{width}x{height}_{layer}_{duration}_{crop}_{scale}_{depthPercent}";
        }

        private Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, Object> {
                    { "atlasName", this.atlasName },
                    { "id", this.id },
                    { "textureID", this.textureID },
                    { "depthPercent", this.depthPercent },
                    { "colWidth", this.colWidth },
                    { "colHeight", this.colHeight },
                    { "gfxWidth", this.gfxWidth },
                    { "gfxHeight", this.gfxHeight },
                    { "colOffsetX", this.colOffset.X },
                    { "colOffsetY", this.colOffset.Y },
                    { "gfxOffsetX", this.gfxOffset.X },
                    { "gfxOffsetY", this.gfxOffset.Y },
                    { "textureSizeX", this.textureSize.X },
                    { "textureSizeY", this.textureSize.Y },
                    { "textureRect", this.textureRect },
                    { "rotationOriginX", this.rotationOrigin.X },
                    { "rotationOriginY", this.rotationOrigin.Y },
                    { "pngPath", this.pngPath },
                    { "layer", this.layer },
                    { "duration", this.duration },
                    { "scale", this.scale },
                    { "ignoreWhenCalculatingMaxSize", this.ignoreWhenCalculatingMaxSize },
                    { "cropped", this.cropped },
                    { "srcAtlasX", this.srcAtlasX },
                    { "srcAtlasY", this.srcAtlasY },
                    { "srcWidth", this.srcWidth },
                    { "srcHeight", this.srcHeight },
                    };
        }

        private AnimFrame(Dictionary<string, Object> jsonData)
        {
            this.atlasName = (string)jsonData["atlasName"];
            this.id = (string)jsonData["id"];
            this.textureID = (string)jsonData["textureID"];
            this.depthPercent = (float)(double)jsonData["depthPercent"];
            this.colWidth = (int)(Int64)jsonData["colWidth"];
            this.colHeight = (int)(Int64)jsonData["colHeight"];
            this.gfxWidth = (int)(Int64)jsonData["gfxWidth"];
            this.gfxHeight = (int)(Int64)jsonData["gfxHeight"];
            this.colOffset = new Vector2((float)(double)jsonData["colOffsetY"], (float)(double)jsonData["colOffsetY"]);
            this.gfxOffset = new Vector2((float)(double)jsonData["gfxOffsetX"], (float)(double)jsonData["gfxOffsetY"]);
            this.textureSize = new Vector2((float)(double)jsonData["textureSizeX"], (float)(double)jsonData["textureSizeY"]);
            this.textureRect = (Rectangle)jsonData["textureRect"];
            this.rotationOrigin = new Vector2((float)(double)jsonData["rotationOriginX"], (float)(double)jsonData["rotationOriginY"]);
            this.pngPath = (string)jsonData["pngPath"];
            this.layer = (int)(Int64)jsonData["layer"];
            this.duration = (short)(Int64)jsonData["duration"];
            this.scale = (float)(double)jsonData["scale"];
            this.ignoreWhenCalculatingMaxSize = (bool)jsonData["ignoreWhenCalculatingMaxSize"];
            this.cropped = (bool)jsonData["cropped"];
            this.srcAtlasX = (int)(Int64)jsonData["srcAtlasX"];
            this.srcAtlasY = (int)(Int64)jsonData["srcAtlasY"];
            this.srcWidth = (int)(Int64)jsonData["srcWidth"];
            this.srcHeight = (int)(Int64)jsonData["srcHeight"];

            AnimData.frameById[this.id] = this;
        }

        public AnimFrame GetCroppedFrameCopy()
        {
            if (this.cropped || (this.srcWidth == 1 && this.srcHeight == 1)) return this;

            return GetFrame(atlasName: this.atlasName, atlasX: this.srcAtlasX, atlasY: this.srcAtlasY, width: this.srcWidth, height: this.srcHeight, layer: this.layer, duration: this.duration, crop: true, scale: this.scale, depthPercent: this.depthPercent, ignoreWhenCalculatingMaxSize: true);
        }

        private AnimFrame(string atlasName, int atlasX, int atlasY, int width, int height, int layer, short duration, bool crop, float scale, float depthPercent, int padding, bool ignoreWhenCalculatingMaxSize)
        {
            // should not be invoked from other classes directly

            this.id = GetID(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent);
            this.textureID = GetID(atlasName: atlasName, atlasX: atlasX, atlasY: atlasY, width: width, height: height, layer: 0, duration: 0, crop: crop, scale: 0, depthPercent: 0);

            AnimData.frameById[this.id] = this;
            this.pngPath = Path.Combine(SonOfRobinGame.animCachePath, $"{this.textureID}.png");

            this.cropped = crop;
            this.srcAtlasX = atlasX;
            this.srcAtlasY = atlasY;
            this.srcWidth = width;
            this.srcHeight = height;

            this.depthPercent = depthPercent;
            this.atlasName = atlasName;
            this.scale = scale;
            this.layer = layer;
            this.duration = duration; // duration == 0 will stop the animation
            this.ignoreWhenCalculatingMaxSize = ignoreWhenCalculatingMaxSize;

            bool textureFound = AnimData.textureDict.ContainsKey(this.textureID);

            if (textureFound) this.texture = AnimData.textureDict[this.textureID];
            else this.texture = GfxConverter.LoadTextureFromPNG(this.pngPath);

            if (!textureFound)
            {
                Texture2D atlasTexture = TextureBank.GetTexture(this.atlasName);
                Rectangle cropRect = GetCropRect(texture: atlasTexture, textureX: atlasX, textureY: atlasY, width: width, height: height, crop: crop);

                // padding makes the edge texture filtering smooth and allows for border effects outside original texture edges
                this.texture = GfxConverter.CropTextureAndAddPadding(baseTexture: atlasTexture, cropRect: cropRect, padding: padding);
                GfxConverter.SaveTextureAsPNG(pngPath: pngPath, texture: this.texture);

                AnimData.textureDict[this.textureID] = this.texture;
            }

            this.textureSize = new Vector2(this.Texture.Width, this.Texture.Height);
            this.textureRect = new Rectangle(x: 0, y: 0, width: this.Texture.Width, height: this.Texture.Height);
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

            AnimData.jsonDict[this.id] = this.Serialize();

            if (!this.cropped) this.GetCroppedFrameCopy(); // creating cropped copy also, in case it's needed
        }

        private Rectangle FindCollisionBounds()
        {
            int sliceWidth = this.texture.Width;
            int sliceHeight = this.texture.Height;

            if (this.layer == 1)
            {
                // checking bottom part of the frame for collision bounds
                sliceHeight = Math.Max((int)(this.texture.Height * this.depthPercent), 20);
                sliceHeight = Math.Min(sliceHeight, this.texture.Height);
            }

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
            Rectangle extractRegion = new(textureX, textureY, width, height);
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

            Texture2D textureToDraw = this.Texture;

            int correctedSourceHeight = this.Texture.Height;
            if (submergeCorrection > 0)
            {
                // first pass - whole sprite visible through water
                SonOfRobinGame.SpriteBatch.Draw(textureToDraw, destRect, this.textureRect, Color.Blue * opacity * 0.2f);

                correctedSourceHeight = Math.Max(textureToDraw.Height / 2, textureToDraw.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle sourceRectangle = new(x: 0, y: 0, width: textureToDraw.Width, correctedSourceHeight);

            SonOfRobinGame.SpriteBatch.Draw(textureToDraw, destRect, sourceRectangle, color * opacity);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity, Vector2 rotationOriginOverride)
        {
            // invoke from Sprite class

            Vector2 rotationOriginToUse = this.rotationOrigin;

            if (rotationOriginOverride != Vector2.Zero)
            {
                rotationOriginToUse = rotationOriginOverride;
                position += (rotationOriginToUse - this.rotationOrigin) * this.scale;
            }

            SonOfRobinGame.SpriteBatch.Draw(this.Texture, position: position, sourceRectangle: this.textureRect, color: color * opacity, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: SpriteEffects.None, layerDepth: 0);
        }

        public void DrawAndKeepInRectBounds(Rectangle destBoundsRect, Color color, float opacity = 1f)
        {
            // general use

            Helpers.DrawTextureInsideRect(texture: this.Texture, rectangle: destBoundsRect, color: color * opacity);
        }
    }
}