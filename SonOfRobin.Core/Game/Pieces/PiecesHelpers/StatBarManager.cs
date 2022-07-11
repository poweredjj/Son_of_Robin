using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class StatBarManager
    {
        public struct StatBar
        {
            private static readonly SpriteFont defaultFont = SonOfRobinGame.fontPixelMix5;

            private readonly BoardPiece boardPiece;
            private readonly string curStatName;
            private readonly string maxStatName;
            private readonly string label;
            private readonly SpriteFont font;
            private readonly Texture2D texture;
            private readonly int width;
            public readonly int height;
            private readonly Color colorMin;
            private readonly Color colorMax;
            private readonly bool centerX;
            private readonly bool ignoreIfAtMax;
            private readonly int maxVal;

            public int CurrentValue
            {
                get
                {
                    object valObj = Helpers.GetProperty(targetObj: this.boardPiece, propertyName: this.curStatName);
                    return ConvertValObj(valObj);
                }
            }

            public int MaxValue
            {
                get
                {
                    return this.maxVal == -1 ? ConvertValObj(Helpers.GetProperty(targetObj: this.boardPiece, propertyName: this.maxStatName)) : this.maxVal;
                }
            }

            public StatBar(BoardPiece boardPiece, string curStatName, Color colorMin, Color colorMax, bool centerX = true, string label = "", int width = 50, int height = 6, Texture2D texture = null, bool ignoreIfAtMax = false, SpriteFont font = null, string maxStatName = "", int maxVal = -1)
            {
                if (label == "" && texture == null) throw new ArgumentException("Label and texture are both undefined.");
                if (maxStatName == "" && maxVal == -1) throw new ArgumentException("MaxStatName and maxVal are both undefined.");

                this.boardPiece = boardPiece;
                this.label = label;
                this.curStatName = curStatName;
                this.maxStatName = maxStatName;
                this.font = font ?? defaultFont;
                this.texture = texture;
                this.width = width;
                this.height = height;
                this.colorMin = colorMin;
                this.colorMax = colorMax;
                this.centerX = centerX;
                this.ignoreIfAtMax = ignoreIfAtMax;
                this.maxVal = maxVal;
            }

            private static int ConvertValObj(object valObj)
            {
                if (valObj.GetType() == typeof(int)) return (int)valObj;
                else
                {
                    float floatVal = (float)valObj;
                    return (int)floatVal;
                }
            }

            public bool Draw(Vector2 position)
            {
                int currentValue = CurrentValue;
                int maxValue = this.MaxValue;

                if (this.ignoreIfAtMax && currentValue == maxValue) return false;

                if (this.centerX) position.X -= this.width / 2;

                if (this.texture != null)
                {
                    // texture

                    int rectWidth = this.width * 3;
                    int margin = (int)(this.height * 1.2);

                    Rectangle destRect = new Rectangle(x: (int)(position.X - rectWidth - margin), y: (int)position.Y, width: rectWidth, height: this.height);
                    destRect.Inflate(0, 2);

                    Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: destRect, color: Color.White, alignX: Helpers.AlignX.Right, drawTestRect: false);
                }
                else
                {
                    // label
                    Vector2 labelSize = font.MeasureString(this.label);
                    Vector2 labelPos = new Vector2(position.X - (labelSize.X + 4), position.Y + (this.height / 2f) - (labelSize.Y / 2f));

                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (x == 0 && y == 0) continue;
                            SonOfRobinGame.spriteBatch.DrawString(font, this.label, labelPos + new Vector2(x, y), Color.Black);
                        }
                    }

                    SonOfRobinGame.spriteBatch.DrawString(font, this.label, labelPos, Color.White);
                }

                float lengthPercentage = 0;
                try
                { lengthPercentage = (float)currentValue / (float)maxValue; }
                catch (DivideByZeroException)
                { }

                Color colorMinMultiplied = this.colorMin * (1f - lengthPercentage);
                Color colorMaxMultiplied = this.colorMax * lengthPercentage;

                Color color = new Color(
                    colorMinMultiplied.R + colorMaxMultiplied.R,
                    colorMinMultiplied.G + colorMaxMultiplied.G,
                    colorMinMultiplied.B + colorMaxMultiplied.B
                    );

                int valueWidth = (int)(this.width * lengthPercentage);

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((int)position.X - 1, (int)position.Y - 1, this.width + 2, this.height + 2), Color.Black);
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((int)position.X, (int)position.Y, valueWidth, this.height), color);

                return true;
            }
        }

        private readonly World world;
        private readonly BoardPiece boardPiece;
        private readonly Vector2 positionOverride; // static position to use
        private int showUntilFrame;
        private readonly List<StatBar> statBarList;
        private readonly int margin;
        private readonly int offsetY;

        public StatBarManager(BoardPiece boardPiece, int margin = 4, int offsetY = 4)
        {
            this.showUntilFrame = 0;
            this.statBarList = new List<StatBar>();
            this.boardPiece = boardPiece;
            this.world = boardPiece.world;
            this.margin = margin;
            this.offsetY = offsetY;
        }

        public StatBarManager(BoardPiece boardPiece, Vector2 positionOverride, int margin = 4, int offsetY = 0)
        {
            this.showUntilFrame = 0;
            this.statBarList = new List<StatBar>();
            this.world = boardPiece.world;
            this.margin = margin;
            this.offsetY = offsetY;
            this.positionOverride = positionOverride;
        }

        public void Serialize(Dictionary<string, Object> pieceData)
        {
            pieceData["statBarManager_showUntilFrame"] = this.showUntilFrame;
        }

        public void Deserialize(Dictionary<string, Object> pieceData)
        {
            this.showUntilFrame = (int)pieceData["statBarManager_showUntilFrame"];
        }

        public void AddStatBar(StatBar statBar)
        {
            this.statBarList.Add(statBar);
        }

        public void EnableDraw(int duration)
        {
            this.showUntilFrame = this.world.currentUpdate + duration;
        }

        public void EnableDrawForever()
        {
            this.showUntilFrame = 2147483647;
        }

        public void Draw()
        {
            if (this.world.currentUpdate < this.showUntilFrame) return;

            Vector2 offset = new Vector2(0, this.offsetY);
            Vector2 baseDrawPos = this.boardPiece == null ? this.positionOverride : new Vector2(this.boardPiece.sprite.gfxRect.Center.X, this.boardPiece.sprite.gfxRect.Bottom);

            foreach (StatBar statBar in this.statBarList)
            {
                bool barDrawn = statBar.Draw(baseDrawPos + offset);
                if (barDrawn) offset.Y += statBar.height + this.margin;
            }
        }

    }
}
