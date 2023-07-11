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

    public class TrackingManager
    {
        public readonly World world;
        private readonly Dictionary<string, Tracking> trackingQueue;

        public int TrackingCount
        { get { return trackingQueue.Count; } }

        public TrackingManager(World world)
        {
            this.world = world;
            this.trackingQueue = new Dictionary<string, Tracking>();
        }

        public void AddToQueue(Tracking tracking)
        {
            this.trackingQueue[tracking.followingSprite.id] = tracking;
        }

        public void ProcessQueue()
        {
            // parallel processing causes data corruption and crashes
            foreach (var tracking in this.trackingQueue.Values.ToList())
            {
                if (tracking.ShouldThisBeRemoved(this.world)) this.RemoveFromQueue(tracking);
                else tracking.SetPosition(trackingManager: this);
            }
        }

        public void RemoveFromQueue(Tracking tracking)
        {
            this.trackingQueue.Remove(tracking.followingSprite.id);
            if (tracking.bounceWhenRemoved && tracking.followingSprite.boardPiece.exists)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"'{tracking.followingSprite.boardPiece.name}' removed from tracking queue - adding bounce.");

                Vector2 passiveMovement = new Vector2(this.world.random.Next(-700, 700), this.world.random.Next(-700, 700));
                tracking.followingSprite.boardPiece.AddPassiveMovement(movement: passiveMovement);
            }
        }

        public void RemoveFromQueue(BoardPiece pieceToRemove)
        {
            this.trackingQueue.Remove(pieceToRemove.id);
        }

        public Sprite GetTargetSprite(Sprite followingSprite)
        {
            if (this.trackingQueue.ContainsKey(followingSprite.id)) return this.trackingQueue[followingSprite.id].targetSprite;
            return null;
        }

        public List<Object> Serialize()
        {
            var trackingData = new List<Object> { };
            foreach (Tracking tracking in this.trackingQueue.Values)
            {
                trackingData.Add(tracking.Serialize());
            }

            return trackingData;
        }

        public void Deserialize(List<Object> trackingDataList)
        {
            foreach (Dictionary<string, Object> trackingData in trackingDataList)
            {
                Tracking.Deserialize(world: this.world, trackingData: trackingData);
            }
        }
    }

    public class Tracking
    {
        public readonly bool isCorrect;
        public readonly Sprite followingSprite;
        public readonly Sprite targetSprite;
        private readonly Vector2 offset;
        private readonly XAlign followingXAlign;
        private readonly YAlign followingYAlign;
        private readonly XAlign targetXAlign;
        private readonly YAlign targetYAlign;
        private readonly int firstTrackingFrame;
        private readonly int lastTrackingFrame;
        public readonly bool bounceWhenRemoved;
        private readonly int followSlowDown;

        public bool BothSpritesExistAndOnBoard
        {
            get
            {
                return this.targetSprite.boardPiece.exists &&
                    this.targetSprite.boardPiece.sprite.IsOnBoard &&
                    this.followingSprite.boardPiece.exists &&
                    this.followingSprite.boardPiece.sprite.IsOnBoard;
            }
        }

        public Tracking(World world, Sprite targetSprite, Sprite followingSprite, int offsetX = 0, int offsetY = 0,
            XAlign followingXAlign = XAlign.Center, YAlign followingYAlign = YAlign.Center,
            XAlign targetXAlign = XAlign.Center, YAlign targetYAlign = YAlign.Center,
            int turnOffDelay = 0, bool bounceWhenRemoved = false, int followSlowDown = 0, bool addToQueue = true)
        {
            this.isCorrect = false;
            this.firstTrackingFrame = world.CurrentUpdate;
            this.lastTrackingFrame = turnOffDelay == 0 ? 0 : world.CurrentUpdate + turnOffDelay;
            this.bounceWhenRemoved = bounceWhenRemoved;
            this.followSlowDown = followSlowDown;
            if (this.followSlowDown < 0) new ArgumentException($"followSlowDown ({followSlowDown}) cannot be < 0.");

            this.followingSprite = followingSprite;
            this.followingXAlign = followingXAlign;
            this.followingYAlign = followingYAlign;

            this.targetSprite = targetSprite;
            this.targetXAlign = targetXAlign;
            this.targetYAlign = targetYAlign;

            this.offset = new Vector2(offsetX, offsetY);

            if (!this.BothSpritesExistAndOnBoard) return;

            this.isCorrect = true;

            if (addToQueue)
            {
                world.trackingManager.AddToQueue(this);
                this.SetPosition(trackingManager: world.trackingManager); // to set position right away
            }
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
                {"followSlowDown", this.followSlowDown},
            };

            return trackingData;
        }

        public static void Deserialize(World world, Dictionary<string, Object> trackingData)
        // deserialize
        {
            // if object was destroyed, it will no longer be available after loading
            if (!world.piecesByOldID.ContainsKey((string)trackingData["followingSprite_id"]) ||
                !world.piecesByOldID.ContainsKey((string)trackingData["targetSprite_id"])) return;

            Sprite followingSprite = world.piecesByOldID[(string)trackingData["followingSprite_id"]].sprite;
            XAlign followingXAlign = (XAlign)(Int64)trackingData["followingXAlign"];
            YAlign followingYAlign = (YAlign)(Int64)trackingData["followingYAlign"];

            Sprite targetSprite = world.piecesByOldID[(string)trackingData["targetSprite_id"]].sprite;
            XAlign targetXAlign = (XAlign)(Int64)trackingData["targetXAlign"];
            YAlign targetYAlign = (YAlign)(Int64)trackingData["targetYAlign"];

            int offsetX = (int)(Int64)trackingData["offsetX"];
            int offsetY = (int)(Int64)trackingData["offsetY"];

            bool bounceWhenRemoved = (bool)trackingData["bounceWhenRemoved"];
            int followSlowDown = (int)(Int64)trackingData["followSlowDown"];
            int lastTrackingFrame = (int)(Int64)trackingData["lastTrackingFrame"];
            int delay = Math.Max(world.CurrentUpdate, lastTrackingFrame - world.CurrentUpdate);

            new Tracking(world: world, targetSprite: targetSprite, followingSprite: followingSprite, followingXAlign: followingXAlign, followingYAlign: followingYAlign, targetXAlign: targetXAlign, targetYAlign: targetYAlign, offsetX: offsetX, offsetY: offsetY, turnOffDelay: delay, bounceWhenRemoved: bounceWhenRemoved, followSlowDown: followSlowDown, addToQueue: true);
        }

        public bool ShouldThisBeRemoved(World world)
        {
            return !this.BothSpritesExistAndOnBoard ||
                       (this.followingSprite.boardPiece.pieceInfo.serialize == false &&
                       this.followingSprite.opacity < 0.05f &&
                       this.followingSprite.opacityFade != null &&
                       world.CurrentUpdate > this.firstTrackingFrame + 100);
        }

        public void SetPosition(TrackingManager trackingManager)
        {
            if (!this.followingSprite.boardPiece.exists ||
                !this.targetSprite.boardPiece.exists ||
                (this.lastTrackingFrame > 0 && trackingManager.world.CurrentUpdate >= this.lastTrackingFrame))
            {
                trackingManager.RemoveFromQueue(this);
                return;
            }

            // followingOffset - what point of followingSprite should be treated as its center
            Vector2 followingOffset = GetSpriteOffset(sprite: this.followingSprite, xAlign: this.followingXAlign, yAlign: followingYAlign);

            // targetOffset - what point of targetSprite should followingSprite be centered around
            Vector2 targetOffset = GetSpriteOffset(sprite: this.targetSprite, xAlign: this.targetXAlign, yAlign: targetYAlign);

            // this.offset - independent offset, used regardless of two previous

            Vector2 targetPos = targetSprite.position + this.offset + targetOffset - followingOffset;

            Vector2 newPos;
            if (this.followSlowDown == 0) newPos = targetPos;
            else
            {
                Vector2 oldPos = followingSprite.position;
                newPos.X = oldPos.X + (targetPos.X - oldPos.X) / this.followSlowDown;
                newPos.Y = oldPos.Y + (targetPos.Y - oldPos.Y) / this.followSlowDown;
            }

            followingSprite.SetNewPosition(newPos: newPos, ignoreCollisions: true);
        }

        private static Vector2 GetSpriteOffset(Sprite sprite, XAlign xAlign, YAlign yAlign)
        {
            int targetX;
            int targetY;

            switch (xAlign)
            {
                case XAlign.Center:
                    targetX = sprite.GfxRect.Center.X;
                    break;

                case XAlign.Left:
                    targetX = sprite.GfxRect.Left;
                    break;

                case XAlign.Right:
                    targetX = sprite.GfxRect.Right;
                    break;

                default:
                    throw new ArgumentException($"Unsupported xAlign - {xAlign}.");
            }

            switch (yAlign)
            {
                case YAlign.Center:
                    targetY = sprite.GfxRect.Center.Y;
                    break;

                case YAlign.Top:
                    targetY = sprite.GfxRect.Top;
                    break;

                case YAlign.Bottom:
                    targetY = sprite.GfxRect.Bottom;
                    break;

                default:
                    throw new ArgumentException($"Unsupported yAlign - {yAlign}.");
            }

            return new Vector2(targetX - sprite.position.X, targetY - sprite.position.Y);
        }
    }
}