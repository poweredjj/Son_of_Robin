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

            private readonly string label;
            private readonly string statName;
            private readonly SpriteFont font;
            private readonly Texture2D texture;
            private readonly int fullWidth;
            public readonly int fullHeight;
            private readonly Color colorMin;
            private readonly Color colorMax;
            private readonly bool centerX;
            private readonly bool ignoreIfAtMax;

            public StatBar(string statName, SpriteFont font, Color colorMin, Color colorMax, bool centerX = true, string label = "", int width = 50, int height = 6, Texture2D texture = null, bool ignoreIfAtMax = false)
            {
                if (label == "" && texture == null) throw new ArgumentException("Label and texture are both undefined.");

                this.label = label;
                this.statName = statName;
                this.font = font;
                this.texture = texture;
                this.fullWidth = width;
                this.fullHeight = height;
                this.colorMin = colorMin;
                this.colorMax = colorMax;
                this.centerX = centerX;
                this.ignoreIfAtMax = ignoreIfAtMax;
            }

            public bool Draw(Vector2 position)
            {
                // TODO add draw code

                return false;
            }
        }

        private readonly World world;
        private readonly BoardPiece boardPiece; // boardPiece - its position will be used
        private readonly Vector2 position; // static position to use
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

        public StatBarManager(Vector2 position, World world)
        {
            this.showUntilFrame = 0;
            this.statBarList = new List<StatBar>();
            this.position = position;
            this.world = world;
        }

        public void AddStatBar(StatBar statBar)
        {
            this.statBarList.Add(statBar);
        }

        public void Draw()
        {
            if (this.world.currentUpdate < this.showUntilFrame) return;

            Vector2 offset = Vector2.Zero;
            Vector2 baseDrawPos = this.boardPiece == null ? this.position : boardPiece.sprite.position;

            foreach (StatBar statBar in this.statBarList)
            {
                bool barDrawn = statBar.Draw(baseDrawPos + offset);
                if (barDrawn) offset.Y += statBar.fullHeight;
            }
        }

    }
}
