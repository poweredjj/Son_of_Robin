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
        private static readonly SpriteFont font = SonOfRobinGame.fontSuperSmall;

        private readonly Color color;

        private readonly string label;
        private readonly Vector2 labelSize;
        private readonly Vector2 labelPos;

        private readonly int posX;
        private readonly int posY;

        private readonly int valueWidth;
        private readonly int fullWidth;
        private readonly int fullHeight;

        private readonly bool centerX;


        public StatBar(int value, int valueMax, Color colorMin, Color colorMax, int posX, int posY, string label, int width = 40, int height = 2, bool ignoreIfAtMax = false, bool centerX = true, bool drawFromTop = true, bool labelAtLeft = true)
        {
            if (ignoreIfAtMax && value == valueMax) return;

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
            this.valueWidth = Convert.ToInt32(this.fullWidth * lengthPercentage);

            this.posX = this.centerX ? (posX - (this.fullWidth / 2)) : posX;
            int verticalOffset = 3 + (currentBatchCount * (this.fullHeight + 4));
            this.posY = drawFromTop ? (posY + verticalOffset) : (posY - verticalOffset);

            this.label = label;
            this.labelSize = font.MeasureString(this.label);

            float labelPosX = labelAtLeft ? this.posX - (labelSize.X + 4) : this.posX + this.fullWidth + 4;
            float labelPosY = drawFromTop ? this.posY + (this.fullHeight / 2f) - (labelSize.Y / 2f) : this.posY + (this.fullHeight / 2f) + (labelSize.Y / 2f);

            this.labelPos = new Vector2((int)labelPosX, (int)labelPosY);

            barsToDraw.Add(this);
            currentBatchCount++;
        }

        public static void FinishThisBatch()
        { currentBatchCount = 0; }

        private void DrawTxtOutline()
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                { SonOfRobinGame.spriteBatch.DrawString(font, this.label, this.labelPos + new Vector2(x, y), Color.Black); }
            }
        }

        private void DrawTxt()
        { SonOfRobinGame.spriteBatch.DrawString(font, this.label, this.labelPos, Color.White); }


        private void DrawBarOutline()
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(this.posX - 1, this.posY - 1, this.fullWidth + 2, this.fullHeight + 2), Color.Black);
        }

        private void DrawBar()
        {
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(this.posX, this.posY, this.valueWidth, this.fullHeight), this.color);
        }

        public static void DrawAll()
        {
            foreach (StatBar bar in barsToDraw)
            {
                bar.DrawBarOutline();
                bar.DrawTxtOutline();
            }

            foreach (StatBar bar in barsToDraw)
            {
                bar.DrawBar();
                bar.DrawTxt();
            }
            barsToDraw.Clear();
            currentBatchCount = 0;
        }

    }
}
