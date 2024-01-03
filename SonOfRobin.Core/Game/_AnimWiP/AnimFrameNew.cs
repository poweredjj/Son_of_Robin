using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class AnimFrameNew
    {
        public readonly string atlasName;
        public Rectangle CropRect
        {
            get
            {
                if (!this.isCropped && !this.initialized) this.FinishInitialization();
                return this.cropRect;
            }
        }
        private Rectangle cropRect;
        public Vector2 GfxOffset
        {
            get
            {
                if (!this.initialized) this.FinishInitialization();
                return this.gfxOffset;
            }
        }
        private Vector2 gfxOffset;
        public readonly bool isCropped;
        private Texture2D texture;

        private Vector2 rotationOrigin;
        public Vector2 RotationOrigin
        {
            get
            {
                if (!this.initialized) this.FinishInitialization();
                return this.rotationOrigin;
            }
        }

        public readonly SpriteEffects spriteEffects;
        public readonly bool mirrorY;
        public readonly int layer;
        public readonly int duration;
        public readonly float scale;
        public readonly bool ignoreWhenCalculatingMaxSize;
        private bool initialized = false;

        public AnimFrameNew(string atlasName, int layer, float scale = 1f, int duration = 0, Vector2 gfxOffset = default, Rectangle cropRect = default, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            this.atlasName = atlasName;
            this.cropRect = cropRect;
            this.isCropped = cropRect != default;
            this.gfxOffset = gfxOffset;
            this.scale = scale;
            this.spriteEffects = spriteEffects;
            this.layer = layer;
            this.duration = duration;
            this.gfxOffset = gfxOffset;
            this.initialized = false; // some parameters can only be set after loading texture
        }

        public Texture2D Texture
        {
            get
            {
                if (this.texture == null) this.LoadAtlasTexture();
                return this.texture;
            }
        }

        private void LoadAtlasTexture()
        {
            MessageLog.Add(debugMessage: true, text: $"Loading atlas texture: {this.atlasName}...");
            this.texture = TextureBank.GetTexture(this.atlasName);
            if (this.texture == null) this.texture = TextureBank.GetTexture(TextureBank.TextureName.GfxCorrupted);
            else AnimData.loadedFramesCount++;
        }

        private void FinishInitialization()
        {
            if (this.texture == null) this.LoadAtlasTexture();

            if (!this.isCropped) this.cropRect = new Rectangle(x: 0, y: 0, width: this.texture.Width, height: this.texture.Height);
            this.rotationOrigin = new Vector2((float)this.cropRect.Width / 2f, (float)this.cropRect.Height / 2f);
            if (this.gfxOffset == default) this.gfxOffset = new Vector2(-(float)this.cropRect.Width / 2f, -(float)this.cropRect.Height / 2f);
            this.initialized = true;
        }

        public void Draw(Rectangle destRect, Color color, float opacity, int submergeCorrection = 0)
        {
            // invoke from Sprite class

            Texture2D textureToDraw = this.Texture;
            if (!this.initialized) this.FinishInitialization(); // has to be checked and invoked here, because one texture can be shared across multiple AnimFrames

            int correctedSourceHeight = this.cropRect.Height;
            if (submergeCorrection > 0)
            {
                // first pass - whole sprite visible through water
                SonOfRobinGame.SpriteBatch.Draw(texture: textureToDraw, destinationRectangle: destRect, sourceRectangle: this.cropRect, color: Color.Blue * opacity * 0.2f);

                correctedSourceHeight = Math.Max(this.cropRect.Height / 2, this.cropRect.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle srcRect = new(x: this.cropRect.X, y: this.cropRect.Y, width: this.cropRect.Width, correctedSourceHeight);

            SonOfRobinGame.SpriteBatch.Draw(texture: textureToDraw, origin: Vector2.Zero, destinationRectangle: destRect, sourceRectangle: srcRect, color: color * opacity, rotation: 0f, effects: this.spriteEffects, layerDepth: 0);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity, Vector2 rotationOriginOverride = default)
        {
            // invoke from Sprite class

            Texture2D texture = this.Texture; // invoked to update other params
            if (!this.initialized) this.FinishInitialization(); // has to be checked and invoked here, because one texture can be shared across multiple AnimFrames

            Vector2 rotationOriginToUse = this.rotationOrigin;

            if (rotationOriginOverride != default)
            {
                rotationOriginToUse = rotationOriginOverride;
                position += (rotationOriginToUse - this.rotationOrigin) * this.scale;
            }

            SonOfRobinGame.SpriteBatch.Draw(texture: texture, position: position, sourceRectangle: this.cropRect, color: color * opacity, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: this.spriteEffects, layerDepth: 0);
        }
    }
}