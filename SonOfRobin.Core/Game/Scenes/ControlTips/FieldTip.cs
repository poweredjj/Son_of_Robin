using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FieldTip
    {
        public enum Alignment { Center, Above, Below, Left, Right };
        private static readonly List<FieldTip> fieldTipList = new List<FieldTip>();

        private readonly Texture2D texture;
        private readonly Rectangle pieceRect;
        private readonly Alignment alignment;
        private Rectangle destRect;

        public FieldTip(Texture2D texture, Rectangle pieceRect, Alignment alignment)
        {
            this.texture = texture;
            this.pieceRect = pieceRect;
            this.alignment = alignment;

            this.CalculateDestRect();

            fieldTipList.Add(this);
        }

        private void CalculateDestRect()
        {
            int textureWidth = (int)(this.texture.Width * Preferences.fieldControlTipsScale);
            int textureHeight = (int)(this.texture.Height * Preferences.fieldControlTipsScale);

            Vector2 position;
            switch (this.alignment)
            {
                case Alignment.Center:
                    position = new Vector2(
                        this.pieceRect.Center.X - (textureWidth / 2),
                        this.pieceRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.Above:
                    position = new Vector2(
                        this.pieceRect.Center.X - (textureWidth / 2),
                        this.pieceRect.Top - textureHeight);
                    break;

                case Alignment.Left:
                    position = new Vector2(
                        this.pieceRect.Left - textureWidth,
                        this.pieceRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.Right:
                    position = new Vector2(
                        this.pieceRect.Right,
                        this.pieceRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.Below:
                    position = new Vector2(
                     this.pieceRect.Center.X - (textureWidth / 2),
                     this.pieceRect.Bottom);
                    break;

                default:
                    throw new ArgumentException($"Unsupported alignment - '{alignment}'.");
            }

            this.destRect = new Rectangle(x: (int)position.X, y: (int)position.Y, width: textureWidth, height: textureHeight);
        }


        private static void MoveCollidingTips()
        {
            int margin = 2;
            bool moveBoth = true;

            for (int i = 0; i < 10; i++)
            {
                bool collisionsFound = false;

                foreach (FieldTip tip1 in fieldTipList)
                {
                    foreach (FieldTip tip2 in fieldTipList)
                    {
                        if (tip1 != tip2 && tip1.destRect.Intersects(tip2.destRect))
                        {
                            if (tip1.destRect.Y == tip2.destRect.Y)
                            {
                                int direction = tip1.destRect.X < tip2.destRect.X ? -1 : 1;

                                if (moveBoth)
                                {
                                    tip1.destRect.X -= (tip1.destRect.Width / 2 * direction) + margin;
                                    tip2.destRect.X += (tip2.destRect.Width / 2 * direction) + margin;
                                }
                                else tip2.destRect.X += (tip2.destRect.Width * direction) + margin;
                            }
                            else
                            {
                                int direction = tip1.destRect.Y < tip2.destRect.Y ? -1 : 1;

                                if (moveBoth)
                                {
                                    tip1.destRect.Y -= (tip1.destRect.Height / 2 * direction) + margin;
                                    tip2.destRect.Y += (tip2.destRect.Height / 2 * direction) + margin;
                                }
                                else tip2.destRect.Y += (tip2.destRect.Height * direction) + margin;
                            }

                            collisionsFound = true;
                        }
                    }
                }

                moveBoth = false;
                if (!collisionsFound) break;
            }
        }

        public static void DrawFieldTips(World world)
        {
            if (Preferences.showControlTips && Preferences.showFieldControlTips)
            {
                MoveCollidingTips();

                SonOfRobinGame.spriteBatch.End();
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: world.TransformMatrix);

                foreach (FieldTip fieldTip in fieldTipList)
                { fieldTip.Draw(); }
            }

            fieldTipList.Clear();
        }

        public void Draw()
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, this.texture.Width, this.texture.Height);
            SonOfRobinGame.spriteBatch.Draw(this.texture, this.destRect, sourceRectangle, Color.White);
        }
    }
}
