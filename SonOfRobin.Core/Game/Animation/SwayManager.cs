using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SwayManager
    {
        public class SwayData
        {
            private readonly Dictionary<Sprite, SwayData> swayDataBySprite;
            private readonly Sprite targetSprite;
            private readonly float originalRotation;
            public float targetRotation;
            public List<SwayForce> swayForceList;

            public SwayData(Dictionary<Sprite, SwayData> swayDataBySprite, Sprite targetSprite)
            {
                this.swayDataBySprite = swayDataBySprite;
                this.targetSprite = targetSprite;
                this.originalRotation = this.targetSprite.rotation;
                this.targetRotation = 0f;
                this.swayForceList = new List<SwayForce>();
            }

            public void Remove()
            {
                this.targetSprite.rotation = this.originalRotation;
                this.swayDataBySprite.Remove(this.targetSprite);
            }
        }

        private const float maxRotation = 1.3f;
        private readonly Dictionary<Sprite, SwayData> swayDataBySprite;
        private Dictionary<Sprite, Sound> hitSoundBySourceSprite;

        public int SwaySpriteCount
        { get { return swayDataBySprite.Count; } }

        public int SoundCount
        { get { return hitSoundBySourceSprite.Count; } }

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
            this.hitSoundBySourceSprite = new Dictionary<Sprite, Sound>();
        }

        public void MakeSmallPlantsReactToStep(World world, Sprite sourceSprite)
        {
            if (!Preferences.plantsSway) return;

            List<Sprite> collidingSpritesList = sourceSprite.GetCollidingSpritesAtPosition(positionToCheck: sourceSprite.position, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.ColPlantGrowth });

            if (!collidingSpritesList.Any()) return;

            if (!this.hitSoundBySourceSprite.ContainsKey(sourceSprite))
            {
                bool isPlayer = sourceSprite.boardPiece.GetType() == typeof(Player);

                Sound hitSound = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.HitSmallPlant1, SoundData.Name.HitSmallPlant2, SoundData.Name.HitSmallPlant3 }, boardPiece: sourceSprite.boardPiece, ignore3DAlways: isPlayer, maxPitchVariation: 0.3f, volume: isPlayer ? 0.4f : 0.25f, cooldown: 16);
                hitSoundBySourceSprite[sourceSprite] = hitSound;
            }
            hitSoundBySourceSprite[sourceSprite].Play();

            foreach (Sprite targetSprite in collidingSpritesList)
            {
                Vector2 sourceOffset = sourceSprite.position - targetSprite.position;
                float distance = Vector2.Distance(targetSprite.position, sourceSprite.position);
                float maxDistance = (sourceSprite.colRect.Width / 2) + (targetSprite.colRect.Width / 2);
                float strength = 1f - (distance / maxDistance);

                this.AddGenericForce(world: world, targetSprite: targetSprite, targetAngle: -sourceOffset.X, strength: strength, durationFrames: 2);
            }
        }

        public void AddGenericForce(World world, Sprite targetSprite, float targetAngle, float strength, int durationFrames, int delayFrames = 0)
        {
            if (!this.swayDataBySprite.ContainsKey(targetSprite))
            {
                this.swayDataBySprite[targetSprite] = new SwayData(swayDataBySprite: this.swayDataBySprite, targetSprite: targetSprite);
            }

            this.swayDataBySprite[targetSprite].swayForceList.Add(new SwayForce(world: world, targetAngle: targetAngle, strength: strength, delayFrames: delayFrames, durationFrames: durationFrames));
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
                        else averageRotation += swayForce.currentAngle;
                    }

                    if (!swayData.swayForceList.Any() && Math.Abs(targetSprite.rotation - swayData.targetRotation) < 0.01) spritesToRemove.Add(targetSprite);
                    else
                    {
                        swayData.targetRotation = Math.Min(Math.Max(averageRotation, -maxRotation), maxRotation);
                        targetSprite.rotation += (swayData.targetRotation - targetSprite.rotation) / 4; // movement smoothing
                    }
                }
            }

            foreach (Sprite sprite in spritesToRemove)
            {
                swayDataBySprite[sprite].Remove();
            }

            this.hitSoundBySourceSprite = this.hitSoundBySourceSprite.Where(kvp => kvp.Value.IsPlaying).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    public class SwayForce
    {
        private readonly World world;
        public readonly float strength;
        public readonly float targetAngle;
        public float currentAngle;

        private readonly int startFrame;
        private readonly int endFrame;
        private readonly float frameFactor;
        public bool HasEnded { get; private set; }

        public SwayForce(World world, float targetAngle, float strength, int durationFrames, int delayFrames = 0)
        {
            this.world = world;
            this.HasEnded = false;

            this.strength = strength;
            this.currentAngle = 0f;
            this.targetAngle = targetAngle;

            this.startFrame = world.CurrentUpdate + delayFrames;
            this.endFrame = this.startFrame + durationFrames;
            this.frameFactor = (float)(this.currentAngle - this.targetAngle) / (float)durationFrames;

            this.Update();
        }

        public void Update()
        {
            if (this.world.CurrentUpdate > this.endFrame)
            {
                this.HasEnded = true;
                return;
            }

            if (this.world.CurrentUpdate >= this.startFrame) this.currentAngle += this.frameFactor;
        }
    }
}