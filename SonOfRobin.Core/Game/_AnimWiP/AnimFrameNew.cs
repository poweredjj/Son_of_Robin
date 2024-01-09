﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class AnimFrameNew
    {
        public readonly string atlasName;
        public readonly Rectangle cropRect;
        public readonly Vector2 gfxOffsetBase;
        public readonly Vector2 gfxOffsetCorrection;
        public readonly int gfxWidth; // final draw size
        public readonly int gfxHeight; // final draw size
        public readonly Vector2 rotationOrigin;
        public readonly SpriteEffects spriteEffects;
        public readonly int layer;
        public readonly int duration;
        public readonly float scale;
        public readonly bool ignoreWhenCalculatingMaxSize;

        private Texture2D texture;

        public AnimFrameNew(string atlasName, int layer, Rectangle cropRect, float scale = 1f, int duration = 0, Vector2 gfxOffsetCorrection = default, bool mirrorX = false, bool mirrorY = false, bool ignoreWhenCalculatingMaxSize = false)
        {
            this.atlasName = atlasName;
            this.scale = scale;
            this.cropRect = cropRect;

            this.gfxWidth = (int)(this.cropRect.Width * this.scale);
            this.gfxHeight = (int)(this.cropRect.Height * this.scale);

            //this.gfxOffset = gfxOffsetCorrection == default ?
            //    new Vector2(-(float)this.cropRect.Width / 2f, -(float)this.cropRect.Height / 2f) * this.scale :
            //    gfxOffsetCorrection * this.scale;

            this.gfxOffsetBase = new Vector2(-(float)this.cropRect.Width / 2f, -(float)this.cropRect.Height / 2f) * this.scale;
            this.gfxOffsetCorrection = (gfxOffsetCorrection == default ? Vector2.Zero : gfxOffsetCorrection) * this.scale;

            this.rotationOrigin = new Vector2((float)this.cropRect.Width / 2f, (float)this.cropRect.Height / 2f);

            if (mirrorX && mirrorY) this.spriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            else if (mirrorX) this.spriteEffects = SpriteEffects.FlipHorizontally;
            else if (mirrorY) this.spriteEffects = SpriteEffects.FlipVertically;
            else this.spriteEffects = SpriteEffects.None;

            this.layer = layer;
            this.duration = duration;
            this.ignoreWhenCalculatingMaxSize = ignoreWhenCalculatingMaxSize;
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

        public Rectangle GetGfxRectForPos(Vector2 position)
        {
            return new(
                x: (int)(position.X + this.gfxOffsetBase.X + this.gfxOffsetCorrection.X),
                y: (int)(position.Y + this.gfxOffsetBase.Y + this.gfxOffsetCorrection.Y),
                width: this.gfxWidth,
                height: this.gfxHeight);
        }

        public void Draw(Rectangle destRect, Color color, float opacity, int submergeCorrection = 0)
        {
            // invoke from Sprite class

            int correctedSourceHeight = this.cropRect.Height;
            if (submergeCorrection > 0)
            {
                // first pass - whole sprite visible through water
                SonOfRobinGame.SpriteBatch.Draw(texture: this.Texture, destinationRectangle: destRect, sourceRectangle: this.cropRect, color: Color.Blue * opacity * 0.2f);

                correctedSourceHeight = Math.Max(this.cropRect.Height / 2, this.cropRect.Height - submergeCorrection);
                destRect.Height = (int)(correctedSourceHeight * this.scale);
            }

            Rectangle srcRect = new(x: this.cropRect.X, y: this.cropRect.Y, width: this.cropRect.Width, correctedSourceHeight);

            SonOfRobinGame.SpriteBatch.Draw(texture: this.Texture, origin: Vector2.Zero, destinationRectangle: destRect, sourceRectangle: srcRect, color: color * opacity, rotation: 0f, effects: this.spriteEffects, layerDepth: 0);
        }

        public void DrawWithRotation(Vector2 position, Color color, float rotation, float opacity, Vector2 rotationOriginOverride = default)
        {
            // invoke from Sprite class

            Vector2 rotationOriginToUse = this.rotationOrigin;

            if (rotationOriginOverride != default)
            {
                rotationOriginToUse = rotationOriginOverride;
                position += (rotationOriginToUse - this.rotationOrigin) * this.scale;
            }

            SonOfRobinGame.SpriteBatch.Draw(texture: this.Texture, position: position + this.gfxOffsetCorrection, sourceRectangle: this.cropRect, color: color * opacity, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: this.spriteEffects, layerDepth: 0);
        }
    }
}