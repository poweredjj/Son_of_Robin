using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
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

        private readonly Dictionary<Sprite, SwayData> swayDataBySprite;

        public int SwaySpriteCount
        { get { return swayDataBySprite.Count; } }

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
            Vector2 sourceOffset = sourceSprite.position - targetSprite.position;
            float distance = Vector2.Distance(targetSprite.position, sourceSprite.position);
            float maxDistance = (sourceSprite.colRect.Width / 2) + (targetSprite.colRect.Width / 2);
            float strength = 1f - (distance / maxDistance);

            bool isLeft = sourceOffset.X <= 0;
            bool isPlayer = sourceSprite.boardPiece.GetType() == typeof(Player);

            this.AddGenericForce(targetSprite: targetSprite, isLeft: isLeft, strength: strength, soundIgnore3D: isPlayer, soundVolume: isPlayer ? 0.35f : 0.2f);
        }

        public void AddGenericForce(Sprite targetSprite, bool isLeft, float strength, int delay = 0, int duration = 1, bool playSound = false, bool soundIgnore3D = false, float soundVolume = 1f)
        {
            if (!this.swayDataBySprite.ContainsKey(targetSprite))
            {
                this.swayDataBySprite[targetSprite] = new SwayData(targetSprite);

                if (playSound) new Sound(nameList: new List<SoundData.Name> { SoundData.Name.HitSmallPlant1, SoundData.Name.HitSmallPlant2, SoundData.Name.HitSmallPlant3 }, boardPiece: targetSprite.boardPiece, ignore3DAlways: soundIgnore3D, maxPitchVariation: 0.3f, volume: soundVolume).Play();
            }

            this.swayDataBySprite[targetSprite].swayForceList.Add(new SwayForce(isLeft: isLeft, strength: strength, delay: delay, duration: duration));
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

        public float TargetRotation
        { get { return 1.2f * (this.isLeft ? 1f : -1f); } }

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
}