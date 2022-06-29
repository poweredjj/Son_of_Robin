using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FieldTip
    {
        public enum Alignment { Center, TopOut, TopIn, BottomOut, BottomIn, LeftIn, RightIn, LeftOut, RightOut };
        private static readonly List<Alignment> inAlignment = new List<Alignment> { Alignment.Center, Alignment.LeftIn, Alignment.RightIn, Alignment.TopIn, Alignment.BottomIn };
        private static readonly int maxInactiveDuration = 10;
        private static readonly int movementSlowdown = 6;
        private static readonly int opacitySlowdown = 6;

        private static readonly Dictionary<Texture2D, FieldTip> tipsDict = new Dictionary<Texture2D, FieldTip>();

        private readonly World world;
        private readonly Texture2D texture;

        private Alignment alignment;

        private int lastFrameActive;
        private bool teleportToTarget;

        private Sprite targetSprite;

        private Vector2 targetPos;
        private Vector2 currentPos;
        private float targetOpacity;
        private float currentOpacity;
        private int TextureWidth { get { return (int)(this.texture.Width * Preferences.fieldControlTipsScale); } }
        private int TextureHeight { get { return (int)(this.texture.Height * Preferences.fieldControlTipsScale); } }

        private FieldTip(World world, Texture2D texture, Sprite targetSprite, Alignment alignment)
        {
            this.world = world;
            this.texture = texture;

            this.currentOpacity = 0f;
            this.targetOpacity = 1f;

            this.alignment = alignment;
            this.lastFrameActive = this.world.currentUpdate;

            this.targetSprite = targetSprite;
            this.currentPos = this.targetPos;

            this.teleportToTarget = true;

            tipsDict[texture] = this;
        }

        private void Update(Sprite targetSprite, Alignment alignment)
        {
            this.alignment = alignment;
            this.lastFrameActive = world.currentUpdate;

            this.targetSprite = targetSprite;
            if (this.currentOpacity == 0f) this.currentPos = this.targetPos;

            this.targetOpacity = 1f;
        }

        private void Remove()
        {
            tipsDict.Remove(this.texture);
        }
        public static void AddUpdateTip(World world, Texture2D texture, Sprite targetSprite, Alignment alignment)
        {
            if (!tipsDict.ContainsKey(texture) || tipsDict[texture].world != world) new FieldTip(world: world, texture: texture, targetSprite: targetSprite, alignment: alignment);
            else tipsDict[texture].Update(targetSprite: targetSprite, alignment: alignment);
        }

        private void ChangeOpacityIfObstructsTarget()
        {
            if (this.targetSprite == null) return;
            if (inAlignment.Contains(this.alignment) && (targetSprite.gfxRect.Width < this.TextureWidth || targetSprite.gfxRect.Height < this.TextureHeight)) this.targetOpacity = 0.6f;
        }

        private Rectangle CalculateDestRect(Vector2 position)
        {
            int textureWidth = this.TextureWidth;
            int textureHeight = this.TextureHeight;
            return new Rectangle(x: (int)position.X, y: (int)position.Y, width: textureWidth, height: textureHeight);
        }

        public static void DrawFieldTips(World world)
        {
            if (Preferences.showControlTips && Preferences.showFieldControlTips)
            {
                SonOfRobinGame.spriteBatch.End();
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: world.TransformMatrix);

                foreach (FieldTip fieldTip in tipsDict.Values)
                { fieldTip.UpdateAndDraw(); }
            }
        }

        public void UpdateAndDraw()
        {
            if (this.targetSprite != null && !this.targetSprite.boardPiece.exists) this.targetSprite = null;

            this.targetPos = this.CalculatePosition();
            this.MoveIfObstructsPlayer();

            if (this.world.currentUpdate - this.lastFrameActive > maxInactiveDuration) this.targetOpacity = 0f;
            else this.ChangeOpacityIfObstructsTarget();

            if (this.teleportToTarget)
            {
                this.currentPos = this.targetPos;
                this.teleportToTarget = false;
            }
            else this.currentPos += (this.targetPos - this.currentPos) / movementSlowdown;

            this.currentOpacity += (this.targetOpacity - this.currentOpacity) / opacitySlowdown;
            if (this.currentOpacity < 0.05f)
            {
                this.Remove();
                return;
            }

            Rectangle sourceRectangle = new Rectangle(0, 0, this.texture.Width, this.texture.Height);
            SonOfRobinGame.spriteBatch.Draw(this.texture, this.CalculateDestRect(this.currentPos), sourceRectangle, Color.White * this.currentOpacity);
        }

        private void MoveIfObstructsPlayer()
        {
            Rectangle targetRect = this.CalculateDestRect(position: this.targetPos);

            if (this.world.player != null && targetRect.Intersects(this.world.player.sprite.gfxRect))
            {
                Rectangle playerRect = this.world.player.sprite.gfxRect;

                int newPosX = Math.Abs(targetRect.Left - playerRect.Right) < Math.Abs(targetRect.Right - playerRect.Left) ?
                    playerRect.Right : playerRect.Left - this.TextureWidth;

                int newPosY = Math.Abs(targetRect.Top - playerRect.Bottom) < Math.Abs(targetRect.Bottom - playerRect.Top) ?
                    playerRect.Bottom : playerRect.Top - this.TextureHeight;

                if (Math.Abs(this.targetPos.X - newPosX) > Math.Abs(this.targetPos.Y - newPosY)) this.targetPos.Y = newPosY;
                else this.targetPos.X = newPosX;
            }
        }

        private Vector2 CalculatePosition()
        {
            if (this.targetSprite == null) return this.targetPos;

            int textureWidth = this.TextureWidth;
            int textureHeight = this.TextureHeight;
            Rectangle targetRect = this.targetSprite.gfxRect;

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
