using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class FieldTip
    {
        public enum Alignment : byte
        {
            Center = 0,
            TopOut = 1,
            TopIn = 2,
            BottomOut = 3,
            BottomIn = 4,
            LeftIn = 5,
            RightIn = 6,
            LeftOut = 7,
            RightOut = 8,
        };

        private static readonly List<Alignment> inAlignment = new List<Alignment> { Alignment.Center, Alignment.LeftIn, Alignment.RightIn, Alignment.TopIn, Alignment.BottomIn };
        private const int maxInactiveDuration = 10;
        private const int movementSlowdown = 6;
        private const int opacitySlowdown = 6;

        private static readonly Dictionary<string, FieldTip> tipsDict = new Dictionary<string, FieldTip>();
        private static readonly List<Sprite> thisFrameSpritesShown = new List<Sprite>();

        private readonly World world;
        private readonly Texture2D texture;
        private readonly Rectangle textureSourceRect;

        private Alignment alignment;

        private int lastFrameActive;

        private Sprite targetSprite;

        private Vector2 targetPos;
        private Vector2 currentPos;
        private float targetOpacity;
        private float currentOpacity;

        private int TextureWidth
        { get { return (int)(this.texture.Width * Preferences.fieldControlTipsScale); } }

        private int TextureHeight
        { get { return (int)(this.texture.Height * Preferences.fieldControlTipsScale); } }

        private FieldTip(World world, Texture2D texture, Sprite targetSprite, Alignment alignment)
        {
            this.world = world;
            this.texture = texture;
            this.textureSourceRect = new Rectangle(0, 0, this.texture.Width, this.texture.Height);

            this.currentOpacity = 0f;
            this.targetOpacity = 1f;

            this.alignment = alignment;
            this.lastFrameActive = this.world.CurrentUpdate;

            this.targetSprite = targetSprite;

            tipsDict[texture.Name] = this;

            this.targetPos = this.CalculatePosition();
            this.currentPos = this.targetPos; // needed for obstruction calculations
            this.MoveIfObstructsPlayerOrOtherTip();
            this.currentPos = this.targetPos; // setting non-obstructing position as current, to show at the right place from the start
        }

        private void Update(Sprite targetSprite, Alignment alignment)
        {
            this.alignment = alignment;
            this.lastFrameActive = world.CurrentUpdate;

            this.targetSprite = targetSprite;
            if (this.currentOpacity == 0f) this.currentPos = this.targetPos;

            this.targetOpacity = 1f;
        }

        private void Remove()
        {
            tipsDict.Remove(this.texture.Name);
        }

        public static void AddUpdateTip(World world, Texture2D texture, Sprite targetSprite, Alignment alignment)
        {
            if (texture.Name == null) return;
            if (!tipsDict.ContainsKey(texture.Name) || tipsDict[texture.Name].world != world) new FieldTip(world: world, texture: texture, targetSprite: targetSprite, alignment: alignment);
            else tipsDict[texture.Name].Update(targetSprite: targetSprite, alignment: alignment);
        }

        private void ChangeOpacityIfObstructsTarget()
        {
            if (this.targetSprite == null) return;
            if (inAlignment.Contains(this.alignment) && (targetSprite.GfxRect.Width < this.TextureWidth || targetSprite.GfxRect.Height < this.TextureHeight)) this.targetOpacity = 0.5f;
        }

        private Rectangle CalculateDestRect(Vector2 position)
        {
            int textureWidth = this.TextureWidth;
            int textureHeight = this.TextureHeight;
            return new Rectangle(x: (int)position.X, y: (int)position.Y, width: textureWidth, height: textureHeight);
        }

        public static void DrawFieldTips(World world)
        {
            thisFrameSpritesShown.Clear();

            foreach (FieldTip fieldTip in tipsDict.Values.ToList())
            {
                fieldTip.UpdateAndDraw(world);
            }
        }

        public void UpdateAndDraw(World world)
        {
            if (this.world != world)
            {
                this.Remove();
                return;
            }

            if (this.targetSprite != null && !this.targetSprite.boardPiece.exists) this.targetSprite = null;

            this.targetPos = this.CalculatePosition();
            this.MoveIfObstructsPlayerOrOtherTip();

            if (this.world.CurrentUpdate - this.lastFrameActive > maxInactiveDuration || !this.world.inputActive) this.targetOpacity = 0f;
            else this.ChangeOpacityIfObstructsTarget();

            this.currentPos += (this.targetPos - this.currentPos) / movementSlowdown;

            this.currentOpacity += (this.targetOpacity - this.currentOpacity) / opacitySlowdown;
            if (this.currentOpacity < 0.05f)
            {
                this.Remove();
                return;
            }

            // drawing button hint

            Rectangle destRect = this.CalculateDestRect(this.currentPos);
            SonOfRobinGame.SpriteBatch.Draw(this.texture, this.CalculateDestRect(this.currentPos), this.textureSourceRect, Color.White * this.currentOpacity);

            // drawing piece name label

            if (this.targetSprite != null && !thisFrameSpritesShown.Contains(this.targetSprite))
            {
                // Helpers.DrawRectangleOutline(rect: destRect, color: Color.LightBlue, borderWidth: 1); // for testing

                thisFrameSpritesShown.Add(this.targetSprite); // to avoid drawing the same label multiple times

                int textWidth = destRect.Width;
                int textHeight = destRect.Height;
                int margin = 0;

                bool topSide = this.world.Player.sprite.position.Y > destRect.Y;

                int textPosY = topSide ? (int)(destRect.Y - textHeight - margin) : (int)(destRect.Y + destRect.Height + margin);

                float textOpacity = this.targetOpacity == 0.5f ? this.currentOpacity * 2f : this.currentOpacity;

                Rectangle pieceNameRect = new(x: destRect.X + ((destRect.Width - textWidth) / 2), y: textPosY, width: textWidth, height: textHeight);

                Helpers.DrawTextInsideRectWithShadow(font: SonOfRobinGame.FontTommy40, text: this.targetSprite.boardPiece.readableName.Replace(" ", "\n"), rectangle: pieceNameRect, color: Color.White * textOpacity, shadowColor: Color.Black * textOpacity, alignX: Helpers.AlignX.Center, alignY: topSide ? Helpers.AlignY.Bottom : Helpers.AlignY.Top, shadowOffset: 1, drawTestRect: false);
            }
        }

        private void MoveIfObstructsPlayerOrOtherTip()
        {
            var rectsToCheck = new List<Rectangle>();

            if (this.world.Player != null) rectsToCheck.Add(this.world.Player.sprite.GfxRect);
            foreach (FieldTip fieldTip in tipsDict.Values)
            {
                if (fieldTip != this) rectsToCheck.Add(fieldTip.CalculateDestRect(fieldTip.currentPos));
            }
            if (rectsToCheck.Count == 0) return;

            Vector2 checkPos = this.currentPos + (this.targetPos - this.currentPos) / 2;
            Rectangle thisTipRect = this.CalculateDestRect(position: checkPos);

            foreach (Rectangle otherTipRect in rectsToCheck)
            {
                if (thisTipRect.Intersects(otherTipRect))
                {
                    int newPosX = Math.Abs(thisTipRect.Left - otherTipRect.Right) < Math.Abs(thisTipRect.Right - otherTipRect.Left) ?
                        otherTipRect.Right : otherTipRect.Left - this.TextureWidth;

                    int newPosY = Math.Abs(thisTipRect.Top - otherTipRect.Bottom) < Math.Abs(thisTipRect.Bottom - otherTipRect.Top) ?
                        otherTipRect.Bottom : otherTipRect.Top - this.TextureHeight;

                    if (Math.Abs(this.targetPos.X - newPosX) > Math.Abs(this.targetPos.Y - newPosY)) this.targetPos.Y = newPosY;
                    else this.targetPos.X = newPosX;
                }
            }
        }

        private Vector2 CalculatePosition()
        {
            if (this.targetSprite == null) return this.targetPos;

            int textureWidth = this.TextureWidth;
            int textureHeight = this.TextureHeight;
            Rectangle targetRect = this.targetSprite.GfxRect;

            Vector2 position;
            switch (this.alignment)
            {
                case Alignment.Center:
                    position = new Vector2(
                        targetRect.Center.X - (textureWidth / 2),
                        targetRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.TopOut:
                    position = new Vector2(
                        targetRect.Center.X - (textureWidth / 2),
                        targetRect.Top - textureHeight);
                    break;

                case Alignment.TopIn:
                    position = new Vector2(
                        targetRect.Center.X - (textureWidth / 2),
                        targetRect.Top);
                    break;

                case Alignment.LeftIn:
                    position = new Vector2(
                        targetRect.Left,
                        targetRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.RightIn:
                    position = new Vector2(
                        targetRect.Right - textureWidth,
                        targetRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.LeftOut:
                    position = new Vector2(
                        targetRect.Left - textureWidth,
                        targetRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.RightOut:
                    position = new Vector2(
                        targetRect.Right,
                        targetRect.Center.Y - (textureHeight / 2));
                    break;

                case Alignment.BottomOut:
                    position = new Vector2(
                        targetRect.Center.X - (textureWidth / 2),
                        targetRect.Bottom);
                    break;

                case Alignment.BottomIn:
                    position = new Vector2(
                        targetRect.Center.X - (textureWidth / 2),
                        targetRect.Bottom - textureHeight);
                    break;

                default:
                    throw new ArgumentException($"Unsupported alignment - '{alignment}'.");
            }

            return position;
        }
    }
}