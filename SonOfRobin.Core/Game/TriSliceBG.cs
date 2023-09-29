using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public struct TriSliceBG
    {
        private readonly Texture2D textureLeft;
        private readonly Texture2D textureMid;
        private readonly Texture2D textureRight;

        private float textureLeftScaledWidth;
        private float textureMidScaledWidth;
        private float textureRightScaledWidth;

        private Rectangle leftRect;
        private Rectangle midRect;
        private Rectangle rightRect;

        private int midTextureRepeats;
        private Rectangle midSourceBoundsRect;

        public TriSliceBG(Texture2D textureLeft, Texture2D textureMid, Texture2D textureRight)
        {
            if (textureLeft.Height != textureMid.Height || textureLeft.Height != textureRight.Height) throw new ArgumentException("Height of all textures must be equal.");

            this.textureLeft = textureLeft;
            this.textureMid = textureMid;
            this.textureRight = textureRight;

            this.textureLeftScaledWidth = 0;
            this.textureMidScaledWidth = 0;
            this.textureRightScaledWidth = 0;

            this.leftRect = new Rectangle();
            this.midRect = new Rectangle();
            this.rightRect = new Rectangle();

            this.midTextureRepeats = 1;
            this.midSourceBoundsRect = this.textureMid.Bounds;
        }

        public void StartSpriteBatch(Scene scene)
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scene.TransformMatrix, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);
        }

        public void Draw(Rectangle triSliceRect, Color color)
        {
            float scale = (float)triSliceRect.Height / (float)this.textureLeft.Height;

            this.textureLeftScaledWidth = (float)(this.textureLeft.Width * scale);
            this.textureMidScaledWidth = Math.Max((float)this.textureMid.Width * scale, 1);
            this.textureRightScaledWidth = (float)this.textureRight.Width * scale;

            // making sure, that last mid texture occurence is drawn fully
            this.midTextureRepeats = (int)Math.Ceiling((triSliceRect.Width - (textureLeftScaledWidth + textureRightScaledWidth)) / textureMidScaledWidth);

            this.leftRect.X = triSliceRect.X;
            this.leftRect.Y = triSliceRect.Y;
            this.leftRect.Width = (int)textureLeftScaledWidth;
            this.leftRect.Height = triSliceRect.Height;

            this.midRect.X = this.leftRect.Right;
            this.midRect.Y = triSliceRect.Y;
            this.midRect.Width = (int)textureMidScaledWidth * this.midTextureRepeats;
            this.midRect.Height = triSliceRect.Height;

            this.midSourceBoundsRect.Width = this.textureMid.Width * this.midTextureRepeats;
            this.midSourceBoundsRect.Height = this.textureMid.Height;

            this.rightRect.X = this.midRect.Right;
            this.rightRect.Y = triSliceRect.Y;
            this.rightRect.Width = (int)textureRightScaledWidth;
            this.rightRect.Height = triSliceRect.Height;

            SonOfRobinGame.SpriteBatch.Draw(this.textureLeft, this.leftRect, this.textureLeft.Bounds, color);
            SonOfRobinGame.SpriteBatch.Draw(this.textureMid, this.midRect, this.midSourceBoundsRect, color);
            SonOfRobinGame.SpriteBatch.Draw(this.textureRight, this.rightRect, this.textureRight.Bounds, color);
        }
    }
}