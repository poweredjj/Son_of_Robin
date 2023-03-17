using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SwayManager
    {
        public class SwayData
        {
            private readonly Sprite targetSprite;
            private readonly float originalRotation;
            public float targetRotation;
            public List<SwayForce> swayForceList;
            public SwayData(Sprite targetSprite)
            {
                this.targetSprite = targetSprite;
                this.originalRotation = this.targetSprite.rotation;
                this.targetRotation = 0f;
                this.swayForceList = new List<SwayForce>();
            }

            public void Remove()
            {
                this.targetSprite.rotation = this.originalRotation;
            }
        }

        private readonly Dictionary<string, SwayEvent> swayEventsBySpriteID;
        private readonly Dictionary<Sprite, SwayData> swayDataBySprite;

        public int SwaySpriteCount { get { return swayDataBySprite.Count; } }

        public int SwayForceCount
        {
            get
            {
                int forceCount = 0;
                foreach (SwayData swayData in swayDataBySprite.Values)
                {
                    forceCount += swayData.swayForceList.Count;
                }

                return forceCount;
            }
        }

        public int SwayEventsCount { get { return swayEventsBySpriteID.Count; } }

        public SwayManager()
        {
            this.swayDataBySprite = new Dictionary<Sprite, SwayData>();
        }

        public void MakeSmallPlantsReactToStep(Sprite sourceSprite)
        {
            if (!Preferences.plantsSway) return;

            List<Sprite> collidingSpritesList = sourceSprite.GetCollidingSpritesAtPosition(positionToCheck: sourceSprite.position, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.ColPlantGrowth });

            foreach (Sprite targetSprite in collidingSpritesList)
            {
                this.AddStepForce(sourceSprite: sourceSprite, targetSprite: targetSprite);
            }
        }

        private void AddStepForce(Sprite sourceSprite, Sprite targetSprite)
        {
            if (!this.swayDataBySprite.ContainsKey(targetSprite))
            {
                this.swayDataBySprite[targetSprite] = new SwayData(targetSprite);

                bool isPlayer = sourceSprite.boardPiece.GetType() == typeof(Player);

                new Sound(nameList: new List<SoundData.Name> { SoundData.Name.HitSmallPlant1, SoundData.Name.HitSmallPlant2, SoundData.Name.HitSmallPlant3 }, boardPiece: targetSprite.boardPiece, ignore3DAlways: isPlayer, maxPitchVariation: 0.3f, volume: isPlayer ? 0.35f : 0.2f).Play();
            }

            Vector2 sourceOffset = sourceSprite.position - targetSprite.position;
            float distance = Vector2.Distance(targetSprite.position, sourceSprite.position);
            float maxDistance = (sourceSprite.colRect.Width / 2) + (targetSprite.colRect.Width / 2);
            float strength = 1f - (distance / maxDistance);

            bool isLeft = sourceOffset.X <= 0;


            // this.targetRotation = this.originalRotation + rotationChange;

            this.swayDataBySprite[targetSprite].swayForceList.Add(new SwayForce(isLeft: isLeft, strength: strength, delay: 0, duration: 1));
        }

        public void CheckForSwayEvents(Sprite sourceSprite)
        {
            if (!Preferences.plantsSway) return;

            List<Sprite> collidingSpritesList = sourceSprite.GetCollidingSpritesAtPosition(positionToCheck: sourceSprite.position, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.ColPlantGrowth });

            foreach (Sprite targetSprite in collidingSpritesList)
            {
                this.AddSwayEvent(sourceSprite: sourceSprite, targetSprite: targetSprite, playSound: true);
            }
        }

        public void AddSwayEvent(Sprite targetSprite, Sprite sourceSprite = null, float targetRotation = 0f, bool playSound = true)
        {
            if (swayEventsBySpriteID.ContainsKey(targetSprite.id)) return;
            this.swayEventsBySpriteID[targetSprite.id] = new SwayEvent(sourceSprite: sourceSprite, targetSprite: targetSprite, targetRotation: targetRotation, playSound: playSound);
        }

        public void FinishAndRemoveAllEvents()
        {
            foreach (SwayData swayData in this.swayDataBySprite.Values)
            {
                swayData.Remove();
            }

            this.swayDataBySprite.Clear();
        }

        public void Update()
        {
            if (!Preferences.plantsSway) return;

            List<Sprite> spritesToRemove = new List<Sprite>();

            foreach (var kvp in this.swayDataBySprite)
            {
                Sprite targetSprite = kvp.Key;

                if (!targetSprite.IsInCameraRect) spritesToRemove.Add(targetSprite);
                else
                {
                    SwayData swayData = kvp.Value;
                    List<SwayForce> forcesToRemove = new List<SwayForce>();

                    float averageRotation = 0f;

                    foreach (SwayForce swayForce in swayData.swayForceList)
                    {
                        swayForce.Update();
                        if (swayForce.HasEnded) forcesToRemove.Add(swayForce);
                        else averageRotation += swayForce.TargetRotation;
                    }

                    if (!swayData.swayForceList.Any()) spritesToRemove.Add(targetSprite);
                    else
                    {
                        swayData.targetRotation = averageRotation;
                        targetSprite.rotation += (swayData.targetRotation - targetSprite.rotation) / 4; // movement smoothing
                    }

                }
            }

            foreach (Sprite sprite in spritesToRemove)
            {
                swayDataBySprite[sprite].Remove();
            }
        }
    }

    public class SwayForce
    {
        public bool HasEnded { get; private set; }
        public float strength;
        public readonly bool isLeft;

        public float TargetRotation { get { return 1.2f * (this.isLeft ? 1f : -1f); } }

        private readonly Tweener tweener;

        public SwayForce(bool isLeft, float strength, int delay = 0, int duration = 1)
        {
            this.isLeft = isLeft;
            this.strength = strength;

            this.tweener = new Tweener();

            this.tweener.TweenTo(target: this, expression: force => force.strength, toValue: strength, duration: duration, delay: delay)
               .Easing(EasingFunctions.QuadraticInOut)
               .OnEnd(t => this.End());
        }

        public void Update()
        {
            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void End()
        {
            this.HasEnded = true;
        }
    }

    public class SwayEvent
    {
        private readonly float originalRotation;
        private float targetRotation;
        public readonly Sprite sourceSprite;
        public readonly Sprite targetSprite;
        public bool HasEnded { get; private set; }

        public SwayEvent(Sprite targetSprite, Sprite sourceSprite, float targetRotation = 0, bool playSound = true)
        {
            this.HasEnded = false;
            this.sourceSprite = sourceSprite;
            this.targetSprite = targetSprite;

            this.originalRotation = this.targetSprite.rotation;
            this.targetSprite.rotationOriginOverride = new Vector2(targetSprite.frame.textureSize.X * 0.5f, targetSprite.frame.textureSize.Y);
            this.targetRotation = targetRotation;

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

            this.targetSprite.rotation += (this.targetRotation - this.targetSprite.rotation) / 4; // movement smoothing
        }

        public void Finish()
        {
            this.HasEnded = true;
            this.targetSprite.rotation = this.originalRotation;
            this.targetSprite.rotationOriginOverride = Vector2.Zero;
        }
    }
}