﻿using Microsoft.Xna.Framework;
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
            private readonly string statName;
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

            public StatBar(BoardPiece boardPiece, string statName, Color colorMin, Color colorMax, int maxVal, bool centerX = true, string label = "", int width = 50, int height = 6, Texture2D texture = null, bool ignoreIfAtMax = false, SpriteFont font = null)
            {
                if (label == "" && texture == null) throw new ArgumentException("Label and texture are both undefined.");

                this.boardPiece = boardPiece;
                this.label = label;
                this.statName = statName;
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

            public bool Draw(Vector2 position)
            {
                int currentVal = (int)Helpers.GetProperty(targetObj: this.boardPiece, propertyName: this.statName);

                if (this.ignoreIfAtMax && this.maxVal == currentVal) return false;

                float lengthPercentage = 0;
                try
                { lengthPercentage = (float)currentVal / (float)this.maxVal; }
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

                if (this.centerX) position.X -= this.width / 2;

                if (this.texture != null)
                {
                    // texture


                }
                else
                {
                    // label
                    Vector2 labelSize = font.MeasureString(this.label);
                    Vector2 labelPos = new Vector2(position.X - (labelSize.X + 4), position.Y + (this.height / 2f) - (labelSize.Y / 2f));
                }


                return true;
            }
        }

        private readonly World world;
        private readonly BoardPiece boardPiece;
        private readonly Vector2 positionOverride; // static position to use
        private int showUntilFrame;
        private List<StatBar> statBarList;
        private readonly int margin;

        public StatBarManager(BoardPiece boardPiece, int margin = 4)
        {
            this.showUntilFrame = 0;
            this.statBarList = new List<StatBar>();
            this.boardPiece = boardPiece;
            this.world = boardPiece.world;
            this.margin = margin;
        }

        public StatBarManager(BoardPiece boardPiece, Vector2 positionOverride, int margin = 4)
        {
            this.showUntilFrame = 0;
            this.statBarList = new List<StatBar>();
            this.world = boardPiece.world;
            this.margin = margin;
            this.positionOverride = positionOverride;
        }

        public void AddStatBar(StatBar statBar)
        {
            this.statBarList.Add(statBar);
        }

        public void Draw()
        {
            if (this.world.currentUpdate < this.showUntilFrame) return;

            Vector2 offset = Vector2.Zero;
            Vector2 baseDrawPos = this.boardPiece == null ? this.positionOverride : new Vector2(this.boardPiece.sprite.gfxRect.Center.X, this.boardPiece.sprite.gfxRect.Bottom);

            foreach (StatBar statBar in this.statBarList)
            {
                bool barDrawn = statBar.Draw(baseDrawPos + offset);
                if (barDrawn) offset.Y += statBar.height + this.margin;
            }
        }

    }
}
