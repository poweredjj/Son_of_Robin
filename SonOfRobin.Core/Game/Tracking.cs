using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SonOfRobin

{

    public enum XAlign
    {
        Center,
        Left,
        Right
    }

    public enum YAlign
    {
        Center,
        Top,
        Bottom
    }
    public class Tracking
    {
        private readonly World world;
        private readonly Sprite followingSprite;
        private readonly Sprite targetSprite;
        private readonly Vector2 offset;
        private readonly XAlign followingXAlign;
        private readonly YAlign followingYAlign;
        private readonly XAlign targetXAlign;
        private readonly YAlign targetYAlign;

        public Tracking(World world, Sprite targetSprite, Sprite followingSprite, int offsetX = 0, int offsetY = 0,
            XAlign followingXAlign = XAlign.Center, YAlign followingYAlign = YAlign.Center,
            XAlign targetXAlign = XAlign.Center, YAlign targetYAlign = YAlign.Center)
        {
            this.world = world;

            this.followingSprite = followingSprite;
            this.followingXAlign = followingXAlign;
            this.followingYAlign = followingYAlign;

            this.targetSprite = targetSprite;
            this.targetXAlign = targetXAlign;
            this.targetYAlign = targetYAlign;

            this.offset = new Vector2(offsetX, offsetY);

            this.AddToTrackingQueue();
        }

        public Tracking(World world, Dictionary<string, Object> trackingData, Dictionary<string, BoardPiece> piecesByOldId)
        // deserialize
        {
            // if object was destroyed, it will no longer be available after loading
            if (!piecesByOldId.ContainsKey((string)trackingData["followingSprite_old_id"]) || !piecesByOldId.ContainsKey((string)trackingData["targetSprite_old_id"])) return;

            this.world = world;

            this.followingSprite = piecesByOldId[(string)trackingData["followingSprite_old_id"]].sprite;
            this.followingXAlign = (XAlign)trackingData["followingXAlign"];
            this.followingYAlign = (YAlign)trackingData["followingYAlign"];

            this.targetSprite = piecesByOldId[(string)trackingData["targetSprite_old_id"]].sprite;
            this.targetXAlign = (XAlign)trackingData["targetXAlign"];
            this.targetYAlign = (YAlign)trackingData["targetYAlign"];

            this.offset = new Vector2((float)trackingData["offsetX"], (float)trackingData["offsetY"]);

            this.AddToTrackingQueue();
        }

        public Dictionary<string, Object> Serialize()
        {
            var trackingData = new Dictionary<string, Object> {
                {"followingSprite_old_id", this.followingSprite.id},
                {"followingXAlign", this.followingXAlign},
                {"followingYAlign", this.followingYAlign},

                {"targetSprite_old_id", this.targetSprite.id},
                {"targetXAlign", this.targetXAlign},
                {"targetYAlign", this.targetYAlign},

                {"offsetX", this.offset.X},
                {"offsetY", this.offset.Y},
            };

            return trackingData;
        }


        private void AddToTrackingQueue()
        { this.world.trackingQueue[this.followingSprite.id] = this; }
        private void RemoveFromTrackingQueue()
        { this.world.trackingQueue.Remove(this.followingSprite.id); }

        public static void ProcessTrackingQueue(World world)
        {
            // parallel processing causes data corruption and crashes
            foreach (var tracking in world.trackingQueue.Values.ToList())
            { 
                tracking.SetPosition();
            }
        }

        private void SetPosition()
        {
            if (!this.followingSprite.boardPiece.exists || !this.targetSprite.boardPiece.exists)
            {
                this.RemoveFromTrackingQueue();
                return;
            }

            Vector2 followingOffset = GetSpriteOffset(sprite: this.followingSprite, xAlign: this.followingXAlign, yAlign: followingYAlign);
            // followingOffset - what point of followingSprite should be treated as its center

            Vector2 targetOffset = GetSpriteOffset(sprite: this.targetSprite, xAlign: this.targetXAlign, yAlign: targetYAlign);
            // targetOffset - what point of targetSprite should followingSprite be centered around

            // this.offset - independent offset, used regardless of two previous
            followingSprite.SetNewPosition(newPos: targetSprite.position + this.offset + targetOffset - followingOffset, ignoreCollisions: true);

        }

        private static Vector2 GetSpriteOffset(Sprite sprite, XAlign xAlign, YAlign yAlign)
        {
            int targetX;
            int targetY;

            switch (xAlign)
            {
                case XAlign.Center:
                    targetX = sprite.gfxRect.Center.X;
                    break;

                case XAlign.Left:
                    targetX = sprite.gfxRect.Left;
                    break;

                case XAlign.Right:
                    targetX = sprite.gfxRect.Right;
                    break;

                default:
                    throw new ArgumentException($"Unsupported xAlign - {xAlign}.");
            }

            switch (yAlign)
            {
                case YAlign.Center:
                    targetY = sprite.gfxRect.Center.Y;
                    break;

                case YAlign.Top:
                    targetY = sprite.gfxRect.Top;
                    break;

                case YAlign.Bottom:
                    targetY = sprite.gfxRect.Bottom;
                    break;

                default:
                    throw new ArgumentException($"Unsupported yAlign - {yAlign}.");
            }

            return new Vector2(targetX - sprite.position.X, targetY - sprite.position.Y);
        }


    }
}
