﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    public class ButtonTip
    {
        public static readonly int margin = 2;
        public static readonly float textureScale = 0.25f;
        private static readonly SpriteFont font = SonOfRobinGame.fontMedium;

        public readonly string text;
        public readonly List<string> textureNames;
        public readonly List<Texture2D> textures;
        public readonly int width;
        public readonly int height;

        public ButtonTip(string text, string textureName)
        {
            this.text = text;
            this.textureNames = new List<string> { textureName };
            this.textures = this.GetTextures();
            Vector2 size = this.WholeSize;
            this.width = (int)size.X;
            this.height = (int)size.Y;
        }
        public ButtonTip(string text, List<string> textureNames)
        {
            this.text = text;
            this.textureNames = textureNames;
            this.textures = this.GetTextures();
            Vector2 size = this.WholeSize;
            this.width = (int)size.X;
            this.height = (int)size.Y;
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

                Vector2 txtSize = font.MeasureString(this.text);
                sumWidth += (int)txtSize.X;
                sumHeight = Math.Max((int)txtSize.Y, sumHeight);

                return new Vector2(sumWidth, sumHeight);
            }
        }
        private List<Texture2D> GetTextures()
        {
            var textures = new List<Texture2D> { };

            foreach (string textureName in this.textureNames)
            { textures.Add(SonOfRobinGame.textureByName[textureName]); }

            return textures;
        }
        public void Draw(ControlTips controlTips, int drawOffsetX)
        {
            Vector2 basePos = new Vector2(drawOffsetX, 0);

            int textureWidth, textureHeight;
            int textureOffsetX = 0;
            int textureMaxHeight = 0;
            foreach (Texture2D texture in this.textures)
            {
                textureWidth = (int)(texture.Width * textureScale);
                textureHeight = (int)(texture.Height * textureScale);
                textureMaxHeight = Math.Max(textureHeight, textureMaxHeight);

                DrawTexture(texture: texture, pos: basePos + new Vector2(textureOffsetX, 0), opacity: controlTips.viewParams.opacity);
                textureOffsetX += textureWidth + margin;
            }

            Vector2 txtSize = font.MeasureString(this.text);
            Vector2 txtPos = basePos + new Vector2(textureOffsetX, (int)(textureMaxHeight / 2) - (int)(txtSize.Y / 2f));

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                { SonOfRobinGame.spriteBatch.DrawString(font, this.text, txtPos + new Vector2(x, y), Color.Black * controlTips.viewParams.drawOpacity); }
            }

            SonOfRobinGame.spriteBatch.DrawString(font, this.text, txtPos, Color.White * controlTips.viewParams.drawOpacity);

            //Helpers.DrawRectangleOutline(rect: new Rectangle((int)txtPos.X, (int)txtPos.Y, (int)font.MeasureString(this.text).X, (int)font.MeasureString(this.text).Y), color: Color.YellowGreen, borderWidth: 1); // testing rect size
        }

        private static void DrawTexture(Texture2D texture, Vector2 pos, float opacity)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle destRectangle = new Rectangle((int)pos.X, (int)pos.Y, (int)(texture.Width * textureScale), (int)(texture.Height * textureScale));
            SonOfRobinGame.spriteBatch.Draw(texture, destRectangle, sourceRectangle, Color.White * opacity);

            //Helpers.DrawRectangleOutline(rect: destRectangle, color: Color.Red, borderWidth: 1); // testing rect size
        }
    }
}
