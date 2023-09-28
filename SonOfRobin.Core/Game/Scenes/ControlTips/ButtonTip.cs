using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class ButtonTip
    {
        public const int margin = 2;
        public const float textureScale = 0.25f;
        private static readonly SpriteFontBase font = SonOfRobinGame.FontFreeSansBold.GetFont(24);
        private const float fontScale = 0.5f; // base font scale (relative to icons); the whole controlTips scene can be scaled down if needed

        public readonly string text;
        public readonly List<Texture2D> textures;
        public readonly int width;
        public readonly int height;
        public Highlighter highlighter;

        public ButtonTip(Dictionary<string, ButtonTip> tipCollection, string text, List<Texture2D> textures,
            bool isHighlighted = true, string highlightCoupledVarName = null, Object highlightCoupledObj = null)
        {
            this.text = text;
            this.textures = textures;
            Vector2 size = this.WholeSize;
            this.width = (int)size.X;
            this.height = (int)size.Y;
            this.highlighter = new Highlighter(isOn: isHighlighted, coupledObj: highlightCoupledObj, coupledVarName: highlightCoupledVarName);

            tipCollection[this.text] = this;
        }

        private Vector2 WholeSize
        {
            get
            {
                int sumWidth = 0;
                int sumHeight = 0;
                int textureWidth, textureHeight;

                foreach (Texture2D texture in this.textures)
                {
                    textureWidth = (int)(texture.Width * textureScale);
                    textureHeight = (int)(texture.Height * textureScale);
                    sumWidth += textureWidth + margin;
                    sumHeight = Math.Max(textureHeight, sumHeight);
                }

                Vector2 txtSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: this.text) * fontScale;
                sumWidth += (int)txtSize.X;
                sumHeight = Math.Max((int)txtSize.Y, sumHeight);

                return new Vector2(sumWidth, sumHeight);
            }
        }

        public void Draw(ControlTips controlTips, int drawOffsetX, float globalOpacity)
        {
            Vector2 basePos = new(drawOffsetX, 0);

            float opacityMultiplier = this.highlighter.IsOn ? 1f : 0.5f;

            int textureOffsetX = 0;
            int textureMaxHeight = 0;
            foreach (Texture2D texture in this.textures)
            {
                int textureWidth = (int)(texture.Width * textureScale);
                int textureHeight = (int)(texture.Height * textureScale);
                textureMaxHeight = Math.Max(textureHeight, textureMaxHeight);

                DrawTexture(texture: texture, pos: basePos + new Vector2(textureOffsetX, 0), opacity: controlTips.viewParams.Opacity * opacityMultiplier * globalOpacity);
                textureOffsetX += textureWidth + margin;
            }

            Vector2 txtSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: this.text) * fontScale;
            Vector2 txtPos = basePos + new Vector2(textureOffsetX, (int)(textureMaxHeight / 2) - (int)(txtSize.Y / 2f));
            Vector2 fontScaleVector = new Vector2(fontScale);
            Vector2 shadowOffset = new Vector2(txtSize.Y * 0.05f);
            int outlineSize = (int)(2f * (1f / fontScale));

            font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: this.text, position: txtPos + shadowOffset, scale: fontScaleVector, color: Color.Black, effect: FontSystemEffect.Blurry, effectAmount: outlineSize * 2);

            font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: this.text, position: txtPos, scale: fontScaleVector, color: Color.White, effect: FontSystemEffect.Stroked, effectAmount: outlineSize);

            // Helpers.DrawRectangleOutline(rect: new Rectangle((int)txtPos.X, (int)txtPos.Y, (int)(font.MeasureString(this.text).X * fontScale), (int)(font.MeasureString(this.text).Y * fontScale)), color: Color.YellowGreen, borderWidth: 1); // testing rect size
        }

        private static void DrawTexture(Texture2D texture, Vector2 pos, float opacity)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle destRectangle = new Rectangle((int)pos.X, (int)pos.Y, (int)(texture.Width * textureScale), (int)(texture.Height * textureScale));
            SonOfRobinGame.SpriteBatch.Draw(texture, destRectangle, sourceRectangle, Color.White * opacity);

            //Helpers.DrawRectangleOutline(rect: destRectangle, color: Color.Red, borderWidth: 1); // testing rect size
        }
    }
}