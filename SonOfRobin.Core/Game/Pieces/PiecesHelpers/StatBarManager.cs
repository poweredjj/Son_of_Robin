using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class StatBarManager
    {
        public struct StatBar
        {
            private readonly string label;
            private readonly string statName;
            private readonly SpriteFont font;
            private readonly Texture2D texture;
            private readonly int fullWidth;
            public readonly int fullHeight;
            private readonly Color colorMin;
            private readonly Color colorMax;
            private readonly bool centerX;
            public int yOffset;

            public StatBar(string label, string statName, SpriteFont font, Texture2D texture, int fullWidth, int fullHeight, Color colorMin, Color colorMax, bool centerX = true)
            {
                this.label = label;
                this.statName = statName;
                this.font = font;
                this.texture = texture;
                this.fullWidth = fullWidth;
                this.fullHeight = fullHeight;
                this.colorMin = colorMin;
                this.colorMax = colorMax;
                this.centerX = centerX;
                this.yOffset = 0;
            }

            public void Draw(SpriteBatch spriteBatch, Vector2 position)
            {


            }
        }

        private readonly World world;
        private readonly BoardPiece boardPiece; // boardPiece - its position will be used
        private readonly Vector2 position; // static position to use
        private int showUntilFrame;
        private List<StatBar> statBarList;

        public StatBarManager(BoardPiece boardPiece)
        {
            this.showUntilFrame = 0;
            this.statBarList = new List<StatBar>();
            this.boardPiece = boardPiece;
            this.world = boardPiece.world;
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
            int yOffset = 0;
            foreach (StatBar bar in this.statBarList)
            {
                yOffset += bar.fullHeight;
            }
            this.statBarList.Add(statBar);
            statBar.yOffset = yOffset;
        }

        public void Draw()
        {
            if (this.world.currentUpdate < this.showUntilFrame) return;

            Vector2 baseDrawPos = this.boardPiece == null ? this.position : boardPiece.sprite.position;



        }

    }
}
