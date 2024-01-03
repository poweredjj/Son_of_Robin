using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class AnimFrameNew
    {
        public readonly string atlasName;
        public Rectangle CropRect { get; private set; }
        public Vector2 GfxOffset { get; private set; }
        public readonly bool isCropped;
        private Texture2D texture;
        public Vector2 RotationOrigin { get; private set; }

        public readonly bool mirrorX; // TODO find a way to draw it properly without using RenderTarget
        public readonly int layer;
        public readonly int duration;
        public readonly float scale;
        public readonly bool ignoreWhenCalculatingMaxSize;
        private bool initialized = false;

        public AnimFrameNew(string atlasName, int layer, float scale = 1f, int duration = 0, Vector2 gfxOffset = default, Rectangle cropRect = default, bool mirrorX = false)
        {
            this.atlasName = atlasName;
            this.CropRect = cropRect;
            this.isCropped = cropRect != default;
            this.GfxOffset = gfxOffset;
            this.scale = scale;
            this.mirrorX = mirrorX;
            this.layer = layer;
            this.duration = duration;
            this.GfxOffset = gfxOffset;
            this.initialized = false; // some parameters can only be set after loading texture
        }

        public Texture2D Texture
        {
            get
            {
                if (this.texture == null)
                {
                    MessageLog.Add(debugMessage: true, text: $"Loading anim frame: {this.atlasName}...");
                    this.texture = TextureBank.GetTexture(this.atlasName);
                    if (this.texture == null) this.texture = TextureBank.GetTexture(TextureBank.TextureName.GfxCorrupted);
                    else AnimData.loadedFramesCount++;
                }
                return this.texture;
            }
        }

        private void FinishInitialization()
        {
            if (!this.isCropped) this.CropRect = new Rectangle(x: 0, y: 0, width: this.texture.Width, height: this.texture.Height);
            this.RotationOrigin = new Vector2((float)this.CropRect.Width / 2f, (float)this.CropRect.Height / 2f);
            if (this.GfxOffset == default) this.GfxOffset = new Vector2(-(float)this.CropRect.Width / 2f, -(float)this.CropRect.Height / 2f);
            this.initialized = true;
        }

        public void Draw(Rectangle destRect, Color color, float opacity, int submergeCorrection = 0)
        {
            // invoke from Sprite class

            Texture2D textureToDraw = this.Texture;
            if (!this.initialized) this.FinishInitialization(); // has to be checked and invoked here, because one texture can be shared across multiple AnimFrames

            int correctedSourceHeight = this.CropRect.Height;
            if (submergeCorrection > 0)
            {
                // first pass - whole sprite visible through water
                SonOfRobinGame.SpriteBatch.Draw(texture: textureToDraw, destinationRectangle: destRect, sourceRectangle: this.CropRect, color: Color.Blue * opacity * 0.2f);

                correctedSourceHeight = Math.Max(this.CropRect.Height / 2, this.CropRect.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle srcRect = new(x: this.CropRect.X, y: this.CropRect.Y, width: this.CropRect.Width, correctedSourceHeight);

            SonOfRobinGame.SpriteBatch.Draw(texture: textureToDraw, origin: Vector2.Zero, destinationRectangle: destRect, sourceRectangle: srcRect, color: color * opacity, rotation: 0f, effects: this.mirrorX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth: 0);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity, Vector2 rotationOriginOverride = default)
        {
            // invoke from Sprite class

            Texture2D texture = this.Texture; // invoked to update other params

            Vector2 rotationOriginToUse = this.RotationOrigin;

            if (rotationOriginOverride != default)
            {
                rotationOriginToUse = rotationOriginOverride;
                position += (rotationOriginToUse - this.RotationOrigin) * this.scale;
            }

            SonOfRobinGame.SpriteBatch.Draw(texture: texture, position: position, sourceRectangle: this.CropRect, color: color * opacity, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: this.mirrorX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth: 0);
        }
    }
}