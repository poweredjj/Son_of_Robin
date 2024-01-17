using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public readonly struct AnimFrameNew
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
        public readonly ImageObj imageObj;

        public AnimFrameNew(string atlasName, int layer, Rectangle cropRect, float scale = 1f, int duration = 0, Vector2 gfxOffsetCorrection = default, bool mirrorX = false, bool mirrorY = false, bool ignoreWhenCalculatingMaxSize = false)
        {
            this.atlasName = atlasName;
            this.scale = scale;
            this.cropRect = cropRect;

            this.gfxWidth = (int)(this.cropRect.Width * this.scale);
            this.gfxHeight = (int)(this.cropRect.Height * this.scale);

            // base - needed to properly align rect with position
            this.gfxOffsetBase = new Vector2(-(float)this.cropRect.Width / 2f, -(float)this.cropRect.Height / 2f) * this.scale;
            // correction - added on top of base, as a way to control final offset value
            this.gfxOffsetCorrection = (gfxOffsetCorrection == default ? Vector2.Zero : gfxOffsetCorrection) * this.scale;

            this.rotationOrigin = new Vector2((float)this.cropRect.Width / 2f, (float)this.cropRect.Height / 2f);

            if (mirrorX && mirrorY) this.spriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            else if (mirrorX) this.spriteEffects = SpriteEffects.FlipHorizontally;
            else if (mirrorY) this.spriteEffects = SpriteEffects.FlipVertically;
            else this.spriteEffects = SpriteEffects.None;

            this.layer = layer;
            this.duration = duration;
            this.ignoreWhenCalculatingMaxSize = ignoreWhenCalculatingMaxSize;

            this.imageObj = new AnimFrameObj(this);
        }

        public AnimFrameNew MakeCopyWithEditedGfxOffsetCorrection(Vector2 gfxOffsetCorrection)
        {
            // to make a copy with edited gfxOffsetCorrection

            return new AnimFrameNew(atlasName: this.atlasName, layer: this.layer, cropRect: this.cropRect, scale: this.scale, duration: this.duration, gfxOffsetCorrection: gfxOffsetCorrection, mirrorX: this.spriteEffects == SpriteEffects.FlipHorizontally, mirrorY: this.spriteEffects == SpriteEffects.FlipVertically, ignoreWhenCalculatingMaxSize: this.ignoreWhenCalculatingMaxSize);
        }

        public Texture2D Texture
        {
            get
            {
                Texture2D texture = TextureBank.GetTexture(this.atlasName);
                texture ??= TextureBank.GetTexture(TextureBank.TextureName.GfxCorrupted);
                return texture;
            }
        }

        public Rectangle GetGfxRectForPos(Vector2 position)
        {
            return new(
                x: (int)(position.X + this.gfxOffsetBase.X + this.gfxOffsetCorrection.X),
                y: (int)(position.Y + this.gfxOffsetBase.Y + this.gfxOffsetCorrection.Y),
                width: this.gfxWidth,
                height: this.gfxHeight);
        }

        private int CalculateHeightForSubmergeCorrection(int submergeCorrection)
        {
            return submergeCorrection > 0 ? Math.Max(this.cropRect.Height / 2, this.cropRect.Height - submergeCorrection) : this.cropRect.Height;
        }

        public void Draw(Vector2 position, Color color, float rotation, float opacity, int submergeCorrection = 0, Vector2 rotationOriginOverride = default)
        {
            // destRect should not be used to draw, because of reduced (integer only) draw precision

            Vector2 rotationOriginToUse = this.rotationOrigin;

            if (rotationOriginOverride != default)
            {
                rotationOriginToUse = rotationOriginOverride;
                position += (rotationOriginToUse - this.rotationOrigin) * this.scale;
            }

            int correctedSourceHeight = this.CalculateHeightForSubmergeCorrection(submergeCorrection);

            if (submergeCorrection > 0)
            {
                // first pass - whole sprite visible through water

                SonOfRobinGame.SpriteBatch.Draw(texture: this.Texture, position: position + this.gfxOffsetCorrection, sourceRectangle: this.cropRect, color: Color.Blue * opacity * 0.2f, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: this.spriteEffects, layerDepth: 0);
            }

            Rectangle srcRect = new(x: this.cropRect.X, y: this.cropRect.Y, width: this.cropRect.Width, correctedSourceHeight);

            SonOfRobinGame.SpriteBatch.Draw(texture: this.Texture, position: position + this.gfxOffsetCorrection, sourceRectangle: srcRect, color: color * opacity, rotation: rotation, origin: rotationOriginToUse, scale: this.scale, effects: this.spriteEffects, layerDepth: 0);
        }
    }
}