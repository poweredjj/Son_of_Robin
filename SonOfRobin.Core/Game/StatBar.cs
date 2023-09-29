using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class StatBar
    {
        private static readonly List<StatBar> barsToDraw = new List<StatBar> { };
        private static int currentBatchCount = 0;

        private static readonly SpriteFontBase defaultFont = SonOfRobinGame.FontPixelMix.GetFont(8);
        private static SpriteFontBase currentBatchFont = SonOfRobinGame.FontPixelMix.GetFont(8);
        private readonly SpriteFontBase font;
        private readonly Color color;

        private readonly bool labelAtLeft;

        private readonly string label;
        private readonly Texture2D texture;
        private readonly Vector2 labelSize;
        private readonly Vector2 labelPos;

        private readonly int posX;
        private readonly int posY;

        private readonly int valueWidth;
        private readonly int fullWidth;
        private readonly int fullHeight;

        private readonly bool centerX;

        public static int BatchHeight
        {
            get
            {
                int height = 0;

                foreach (StatBar bar in barsToDraw)
                { height = Math.Max(height, bar.posY + bar.fullHeight); }

                return height;
            }
        }

        public StatBar(int value, int valueMax, Color colorMin, Color colorMax, int posX, int posY, string label = "", Texture2D texture = null, int width = 50, int height = 6, bool ignoreIfAtMax = false, bool centerX = true, bool drawFromTop = true, bool labelAtLeft = true, int vOffsetCorrection = 0)
        {
            if (ignoreIfAtMax && value == valueMax) return;

            this.labelAtLeft = labelAtLeft;

            this.font = currentBatchFont;
            this.centerX = centerX;

            float lengthPercentage = 0;
            try
            { lengthPercentage = (float)value / (float)valueMax; }
            catch (DivideByZeroException)
            { }

            colorMin *= 1f - lengthPercentage;
            colorMax *= lengthPercentage;

            this.color = new Color(
                colorMin.R + colorMax.R,
                colorMin.G + colorMax.G,
                colorMin.B + colorMax.B
                );

            this.fullWidth = width;
            this.fullHeight = height;
            this.valueWidth = (int)(this.fullWidth * lengthPercentage);

            this.posX = this.centerX ? (posX - (this.fullWidth / 2)) : posX;
            int verticalOffset = 3 + (currentBatchCount * (this.fullHeight + 4 + vOffsetCorrection));
            this.posY = drawFromTop ? posY + verticalOffset : posY - verticalOffset;

            this.label = label;
            this.labelSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: this.label);
            this.texture = texture;

            float labelPosX = this.labelAtLeft ? this.posX - (labelSize.X + 4) : this.posX + this.fullWidth + 4;
            float labelPosY = drawFromTop ? this.posY + (this.fullHeight / 2f) - (labelSize.Y / 2f) : this.posY + (this.fullHeight / 2f) + (labelSize.Y / 2f);

            this.labelPos = new Vector2((int)labelPosX, (int)labelPosY);

            barsToDraw.Add(this);
            currentBatchCount++;
        }

        public static void ChangeBatchFont(SpriteFontBase spriteFontBase)
        {
            // changes font of next declared stat bars

            currentBatchFont = spriteFontBase;
        }

        public static void FinishThisBatch()
        {
            //  if (currentBatchCount == 0) SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Finished empty stat bar batch"); // for testing

            currentBatchCount = 0;
            currentBatchFont = defaultFont;
        }

        private void DrawTxt()
        {
            if (this.texture != null) return;

            this.font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: this.label, position: this.labelPos, color: Color.White, effect: FontSystemEffect.Stroked, effectAmount: 1);
        }

        private void DrawBarOutline()
        {
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(this.posX - 1, this.posY - 1, this.fullWidth + 2, this.fullHeight + 2), Color.Black);
        }

        private void DrawTexture()
        {
            if (this.texture == null) return;

            int rectWidth = this.fullWidth * 3;
            int margin = (int)(this.fullHeight * 1.2);

            Rectangle destRect = new Rectangle(x: this.labelAtLeft ? this.posX - rectWidth - margin : this.posX + this.fullWidth + margin, y: this.posY, width: rectWidth, height: this.fullHeight);
            destRect.Inflate(0, 2);

            Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: destRect, color: Color.White, alignX: labelAtLeft ? Helpers.AlignX.Right : Helpers.AlignX.Left, drawTestRect: false);
        }

        private void DrawBar()
        {
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(this.posX, this.posY, this.valueWidth, this.fullHeight), this.color);
        }

        public static void DrawAll()
        {
            foreach (StatBar bar in barsToDraw)
            {
                bar.DrawBarOutline();
            }

            foreach (StatBar bar in barsToDraw)
            {
                bar.DrawBar();
                bar.DrawTxt();
                bar.DrawTexture();
            }

            barsToDraw.Clear();
            FinishThisBatch();
        }
    }
}