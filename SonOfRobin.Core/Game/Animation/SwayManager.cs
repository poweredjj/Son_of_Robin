﻿using Microsoft.Xna.Framework;
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

        private readonly Dictionary<string, SwayEvent> swayEventsBySpriteID;
        private List<WaitingSwayEvent> waitingEvents;

        public float GetOriginalRotationForSprite(Sprite sprite) // for shadows
        {
            if (this.swayEventsBySpriteID.ContainsKey(sprite.id)) return swayEventsBySpriteID[sprite.id].originalRotation;
            else return sprite.rotation;
        }

        public int SwayEventsCount
        { get { return swayEventsBySpriteID.Count; } }

        public SwayManager()
        {
            this.swayEventsBySpriteID = new Dictionary<string, SwayEvent>();
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
            if (delayFrames > 0)
            {
                this.waitingEvents.Add(new WaitingSwayEvent(startFrame: targetSprite.world.CurrentUpdate + delayFrames, targetSprite: targetSprite, sourceSprite: sourceSprite, targetRotation: targetRotation, playSound: playSound, rotationSlowdown: rotationSlowdown));
                return;
            }

            if (swayEventsBySpriteID.ContainsKey(targetSprite.id)) return;
            this.swayEventsBySpriteID[targetSprite.id] = new SwayEvent(sourceSprite: sourceSprite, targetSprite: targetSprite, targetRotation: targetRotation, playSound: playSound, rotationSlowdown: rotationSlowdown);
        }

        public void FinishAndRemoveAllEvents()
        {
            foreach (SwayEvent swayEvent in this.swayEventsBySpriteID.Values)
            {
                swayEvent.Finish();
            }

            this.swayEventsBySpriteID.Clear();
        }

        public void Update(World world)
        {
            if (!Preferences.plantsSway) return;

            this.ProcessWaitingEvents(world);

            List<string> spriteIDsToRemove = new List<string>();

            foreach (var kvp in this.swayEventsBySpriteID)
            {
                SwayEvent swayEvent = kvp.Value;

                swayEvent.Update();
                if (swayEvent.HasEnded) spriteIDsToRemove.Add(kvp.Key);
            }

            foreach (string spriteID in spriteIDsToRemove)
            {
                this.swayEventsBySpriteID.Remove(spriteID);
            }
        }

        private void ProcessWaitingEvents(World world)
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
        public readonly float originalRotation;
        private float targetRotation;
        public readonly Sprite sourceSprite;
        public readonly Sprite targetSprite;
        public readonly int rotationSlowdown;
        public bool HasEnded { get; private set; }

        public SwayEvent(Sprite targetSprite, Sprite sourceSprite, float targetRotation = 0, bool playSound = true, int rotationSlowdown = 4)
        {
            if (rotationSlowdown < 1) throw new ArgumentOutOfRangeException($"Rotation slowdown cannot be less than 1 - {rotationSlowdown}");

            this.HasEnded = false;
            this.sourceSprite = sourceSprite;
            this.targetSprite = targetSprite;

            this.originalRotation = this.targetSprite.rotation;
            this.targetSprite.rotationOriginOverride = new Vector2(targetSprite.frame.textureSize.X * 0.5f, targetSprite.frame.textureSize.Y);
            this.targetRotation = targetRotation;
            this.rotationSlowdown = rotationSlowdown;

            if (playSound)
            {
                bool isPlayer = this.sourceSprite != null && this.sourceSprite.boardPiece.GetType() == typeof(Player);

                new Sound(nameList: new List<SoundData.Name> { SoundData.Name.HitSmallPlant1, SoundData.Name.HitSmallPlant2, SoundData.Name.HitSmallPlant3 }, boardPiece: this.targetSprite.boardPiece, ignore3DAlways: isPlayer, maxPitchVariation: 0.3f, volume: isPlayer ? 0.35f : 0.2f).Play();
            }

            this.Update();
        }

        public void Update()
        {
            if (!this.targetSprite.IsInCameraRect || !this.targetSprite.boardPiece.exists)
            {
                this.Finish();
                return;
            }

            if (this.sourceSprite != null && this.targetSprite.colRect.Intersects(this.sourceSprite.colRect))
            {
                Vector2 sourceOffset = sourceSprite.position - targetSprite.position;
                float distance = Vector2.Distance(targetSprite.position, sourceSprite.position);
                float maxDistance = (sourceSprite.colRect.Width / 2) + (targetSprite.colRect.Width / 2);
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

            this.targetSprite.rotation += (this.targetRotation - this.targetSprite.rotation) / this.rotationSlowdown; // movement smoothing
        }

        public void Finish()
        {
            this.HasEnded = true;
            this.targetSprite.rotation = this.originalRotation;
            this.targetSprite.rotationOriginOverride = Vector2.Zero;
        }
    }
}