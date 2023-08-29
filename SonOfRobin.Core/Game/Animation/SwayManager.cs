using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SwayManager
    {
        public struct WaitingSwayEvent
        {
            // must match AddSwayEvent() parameters

            public readonly int startFrame;
            public readonly Sprite targetSprite;
            public readonly Sprite sourceSprite;
            public readonly float targetRotation;
            public readonly bool playSound;
            public readonly int rotationSlowdown;

            public WaitingSwayEvent(int startFrame, Sprite targetSprite, Sprite sourceSprite, float targetRotation, bool playSound, int rotationSlowdown)
            {
                this.startFrame = startFrame;
                this.targetSprite = targetSprite;
                this.sourceSprite = sourceSprite;
                this.targetRotation = targetRotation;
                this.rotationSlowdown = rotationSlowdown;
                this.playSound = playSound;
            }
        }

        private readonly World world;
        private readonly Dictionary<int, SwayEvent> swayEventsBySpriteID;
        private List<WaitingSwayEvent> waitingEvents;

        public int SwayEventsCount
        { get { return swayEventsBySpriteID.Count; } }

        public SwayManager(World world)
        {
            this.world = world;
            this.swayEventsBySpriteID = new Dictionary<int, SwayEvent>();
            this.waitingEvents = new List<WaitingSwayEvent>();
        }

        public void MakeSmallPlantsReactToStep(Sprite sourceSprite)
        {
            if (!Preferences.plantsSway || sourceSprite.boardPiece.name == PieceTemplate.Name.PlayerGhost) return;

            List<Sprite> collidingSpritesList = sourceSprite.GetCollidingSpritesAtPosition(positionToCheck: sourceSprite.position, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.ColPlantGrowth });

            foreach (Sprite targetSprite in collidingSpritesList)
            {
                if (targetSprite.boardPiece.GetType() == typeof(Plant)) this.AddSwayEvent(sourceSprite: sourceSprite, targetSprite: targetSprite, playSound: true);
            }
        }

        public void AddSwayEvent(Sprite targetSprite, Sprite sourceSprite = null, float targetRotation = 0f, bool playSound = true, int delayFrames = 0, int rotationSlowdown = 4)
        {
            if (!targetSprite.IsOnBoard)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Cannot add SwayEvent to sprite {targetSprite.boardPiece.name} that is not on board - ignoring.");
                return;
            }

            if (delayFrames > 0)
            {
                this.waitingEvents.Add(new WaitingSwayEvent(startFrame: targetSprite.world.CurrentUpdate + delayFrames, targetSprite: targetSprite, sourceSprite: sourceSprite, targetRotation: targetRotation, playSound: playSound, rotationSlowdown: rotationSlowdown));
                return;
            }

            if (swayEventsBySpriteID.ContainsKey(targetSprite.id)) return;
            this.swayEventsBySpriteID[targetSprite.id] = new SwayEvent(world: this.world, sourceSprite: sourceSprite, targetSprite: targetSprite, targetRotation: targetRotation, playSound: playSound, rotationSlowdown: rotationSlowdown);
        }

        public void FinishAndRemoveAllEvents()
        {
            foreach (SwayEvent swayEvent in this.swayEventsBySpriteID.Values)
            {
                swayEvent.Finish();
            }

            this.swayEventsBySpriteID.Clear();
        }

        public void Update()
        {
            if (!Preferences.plantsSway) return;

            this.ProcessWaitingEvents();

            var spriteIDsToRemove = new List<int>();

            foreach (var kvp in this.swayEventsBySpriteID)
            {
                SwayEvent swayEvent = kvp.Value;

                swayEvent.Update(this.world);
                if (swayEvent.HasEnded) spriteIDsToRemove.Add(kvp.Key);
            }

            foreach (int spriteID in spriteIDsToRemove)
            {
                this.swayEventsBySpriteID.Remove(spriteID);
            }
        }

        private void ProcessWaitingEvents()
        {
            var newWaitingEvents = new List<WaitingSwayEvent>();

            foreach (WaitingSwayEvent waitingSwayEvent in this.waitingEvents)
            {
                if (world.CurrentUpdate >= waitingSwayEvent.startFrame)
                {
                    if (waitingSwayEvent.targetSprite.IsInCameraRect) this.AddSwayEvent(targetSprite: waitingSwayEvent.targetSprite, sourceSprite: waitingSwayEvent.sourceSprite, targetRotation: waitingSwayEvent.targetRotation, playSound: waitingSwayEvent.playSound, delayFrames: 0, rotationSlowdown: waitingSwayEvent.rotationSlowdown);
                }
                else newWaitingEvents.Add(waitingSwayEvent);
            }

            this.waitingEvents = newWaitingEvents;
        }
    }

    public class SwayEvent
    {
        private readonly float originalRotation;
        private float targetRotation;
        public readonly Sprite sourceSprite;
        public readonly Sprite targetSprite;
        public readonly int rotationSlowdown;
        public bool HasEnded { get; private set; }

        public SwayEvent(World world, Sprite targetSprite, Sprite sourceSprite, float targetRotation = 0, bool playSound = true, int rotationSlowdown = 4)
        {
            if (rotationSlowdown < 1) throw new ArgumentOutOfRangeException($"Rotation slowdown cannot be less than 1 - {rotationSlowdown}");

            this.HasEnded = false;
            this.sourceSprite = sourceSprite;
            this.targetSprite = targetSprite;

            this.originalRotation = this.targetSprite.rotation;
            this.targetSprite.rotationOriginOverride = new Vector2(targetSprite.AnimFrame.textureSize.X * 0.5f, targetSprite.AnimFrame.textureSize.Y);
            this.targetRotation = targetRotation;
            this.rotationSlowdown = rotationSlowdown;

            if (playSound)
            {
                bool isPlayer = this.sourceSprite != null && this.sourceSprite.boardPiece.GetType() == typeof(Player);

                new Sound(nameList: new List<SoundData.Name> { SoundData.Name.HitSmallPlant1, SoundData.Name.HitSmallPlant2, SoundData.Name.HitSmallPlant3 }, boardPiece: this.targetSprite.boardPiece, ignore3DAlways: isPlayer, maxPitchVariation: 0.3f, volume: isPlayer ? 0.35f : 0.2f).Play();

                if (isPlayer)
                {
                    float rumbleMagnifier = 0;
                    if (targetSprite.GfxRect.Height >= sourceSprite.GfxRect.Height * 0.85f) rumbleMagnifier = 1.8f;
                    new RumbleEvent(force: 0.012f + (rumbleMagnifier * 0.025f), smallMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.035f + (rumbleMagnifier * 0.04f));
                }
            }

            this.Update(world);
        }

        public void Update(World world)
        {
            if (!this.targetSprite.IsInCameraRect || !this.targetSprite.boardPiece.exists)
            {
                this.Finish();
                return;
            }

            if (this.sourceSprite != null && this.targetSprite.ColRect.Intersects(this.sourceSprite.ColRect))
            {
                Vector2 sourceOffset = sourceSprite.position - targetSprite.position;
                float distance = Vector2.Distance(targetSprite.position, sourceSprite.position);
                float maxDistance = (sourceSprite.ColRect.Width / 2) + (targetSprite.ColRect.Width / 2);
                float distanceFactor = 1f - (distance / maxDistance);

                float rotationChange = 1.2f * distanceFactor;
                if (sourceOffset.X > 0) rotationChange *= -1;

                this.targetRotation = this.originalRotation + rotationChange;
            }
            else
            {
                if (this.sourceSprite != null) this.targetRotation = this.originalRotation;

                if (Math.Abs(this.targetSprite.rotation - this.targetRotation) < 0.01)
                {
                    if (this.sourceSprite == null && this.targetRotation != this.originalRotation)
                    {
                        this.targetRotation = this.originalRotation;
                        return;
                    }

                    this.Finish();
                    return;
                }
            }

            this.targetSprite.rotation += (this.targetRotation - this.targetSprite.rotation) / Math.Max(this.rotationSlowdown, 1); // movement smoothing
        }

        public void Finish()
        {
            this.HasEnded = true;
            this.targetSprite.rotation = this.originalRotation;
            this.targetSprite.rotationOriginOverride = Vector2.Zero;
        }
    }
}