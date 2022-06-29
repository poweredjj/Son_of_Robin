using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public readonly bool isCorrect;
        private readonly World world;
        private readonly Sprite followingSprite;
        private readonly Sprite targetSprite;
        private readonly Vector2 offset;
        private readonly XAlign followingXAlign;
        private readonly YAlign followingYAlign;
        private readonly XAlign targetXAlign;
        private readonly YAlign targetYAlign;
        private readonly int firstTrackingFrame;
        private readonly int lastTrackingFrame;
        private readonly bool bounceWhenRemoved;
        public bool BothSpritesExist { get { return this.targetSprite.boardPiece.exists && this.followingSprite.boardPiece.exists; } }
        public bool ShouldBeRemoved
        {
            get
            {
                // tracking queue could bloat over time - it may be helpful to add more "remove exceptions" in the future

                return !this.BothSpritesExist ||
                    (this.followingSprite.boardPiece.serialize == false &&
                    this.followingSprite.opacity < 0.05f &&
                    this.followingSprite.opacityFade != null &&
                    this.world.currentUpdate > this.firstTrackingFrame + 100);
            }
        }

        public Tracking(World world, Sprite targetSprite, Sprite followingSprite, int offsetX = 0, int offsetY = 0,
            XAlign followingXAlign = XAlign.Center, YAlign followingYAlign = YAlign.Center,
            XAlign targetXAlign = XAlign.Center, YAlign targetYAlign = YAlign.Center,
            int turnOffDelay = 0, bool bounceWhenRemoved = false)
        {
            this.isCorrect = false;
            this.world = world;
            this.firstTrackingFrame = this.world.currentUpdate;
            this.lastTrackingFrame = turnOffDelay == 0 ? 0 : this.world.currentUpdate + turnOffDelay;
            this.bounceWhenRemoved = bounceWhenRemoved;

            this.followingSprite = followingSprite;
            this.followingXAlign = followingXAlign;
            this.followingYAlign = followingYAlign;

            this.targetSprite = targetSprite;
            this.targetXAlign = targetXAlign;
            this.targetYAlign = targetYAlign;

            this.offset = new Vector2(offsetX, offsetY);

            if (!this.BothSpritesExist) return;

            this.isCorrect = true;

            this.AddToTrackingQueue();
        }


        public Dictionary<string, Object> Serialize()
        {
            var trackingData = new Dictionary<string, Object> {
                {"followingSprite_id", this.followingSprite.id},
                {"followingXAlign", this.followingXAlign},
                {"followingYAlign", this.followingYAlign},

                {"targetSprite_id", this.targetSprite.id},
                {"targetXAlign", this.targetXAlign},
                {"targetYAlign", this.targetYAlign},

                {"offsetX", (int)this.offset.X},
                {"offsetY", (int)this.offset.Y},

                {"lastTrackingFrame", this.lastTrackingFrame},
                {"bounceWhenRemoved", this.bounceWhenRemoved},
            };

            return trackingData;
        }

        public static void Deserialize(World world, Dictionary<string, Object> trackingData, Dictionary<string, BoardPiece> piecesByID)
        // deserialize
        {
            // if object was destroyed, it will no longer be available after loading
            if (!piecesByID.ContainsKey((string)trackingData["followingSprite_id"]) || !piecesByID.ContainsKey((string)trackingData["targetSprite_id"])) return;

            Sprite followingSprite = piecesByID[(string)trackingData["followingSprite_id"]].sprite;
            XAlign followingXAlign = (XAlign)trackingData["followingXAlign"];
            YAlign followingYAlign = (YAlign)trackingData["followingYAlign"];

            Sprite targetSprite = piecesByID[(string)trackingData["targetSprite_id"]].sprite;
            XAlign targetXAlign = (XAlign)trackingData["targetXAlign"];
            YAlign targetYAlign = (YAlign)trackingData["targetYAlign"];

            int offsetX = (int)trackingData["offsetX"];
            int offsetY = (int)trackingData["offsetY"];

            bool bounceWhenRemoved = (bool)trackingData["bounceWhenRemoved"];
            int lastTrackingFrame = (int)trackingData["lastTrackingFrame"];
            int delay = Math.Max(world.currentUpdate, lastTrackingFrame - world.currentUpdate);

            new Tracking(world: world, targetSprite: targetSprite, followingSprite: followingSprite, followingXAlign: followingXAlign, followingYAlign: followingYAlign, targetXAlign: targetXAlign, targetYAlign: targetYAlign, offsetX: offsetX, offsetY: offsetY, turnOffDelay: delay, bounceWhenRemoved: bounceWhenRemoved);
        }

        private void AddToTrackingQueue()
        {
            this.world.trackingQueue[this.followingSprite.id] = this;
        }

        private void RemoveFromTrackingQueue()
        {
            this.world.trackingQueue.Remove(this.followingSprite.id);
            if (this.bounceWhenRemoved && this.followingSprite.boardPiece.exists)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"'{this.followingSprite.boardPiece.name}' removed from tracking queue - adding bounce.");

                Vector2 passiveMovement = new Vector2(this.world.random.Next(-700, 700), this.world.random.Next(-700, 700));
                this.followingSprite.boardPiece.AddPassiveMovement(movement: passiveMovement);
            }
        }

        public static void RemoveFromTrackingQueue(BoardPiece pieceToRemove, World world)
        {
            world.trackingQueue.Remove(pieceToRemove.id);
        }

        public static void ProcessTrackingQueue(World world)
        {
            // parallel processing causes data corruption and crashes
            foreach (var tracking in world.trackingQueue.Values.ToList())
            {
                if (tracking.ShouldBeRemoved) tracking.RemoveFromTrackingQueue();
                else tracking.SetPosition();
            }
        }


        private void SetPosition()
        {
            if (!this.followingSprite.boardPiece.exists ||
                !this.targetSprite.boardPiece.exists ||
                (this.lastTrackingFrame > 0 && this.world.currentUpdate >= this.lastTrackingFrame))
            {
                this.RemoveFromTrackingQueue();
                return;
            }

            // followingOffset - what point of followingSprite should be treated as its center
            Vector2 followingOffset = GetSpriteOffset(sprite: this.followingSprite, xAlign: this.followingXAlign, yAlign: followingYAlign);

            // targetOffset - what point of targetSprite should followingSprite be centered around
            Vector2 targetOffset = GetSpriteOffset(sprite: this.targetSprite, xAlign: this.targetXAlign, yAlign: targetYAlign);

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
