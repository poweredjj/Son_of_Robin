using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SwayManager
    {
        private readonly Dictionary<string, SwayEvent> swayEventsBySpriteID;

        public SwayManager()
        {
            this.swayEventsBySpriteID = new Dictionary<string, SwayEvent>();
        }

        public void CheckForSwayEvents(Sprite sourceSprite)
        {
            if (!Preferences.plantsSway) return;

            List<Sprite> collidingSpritesList = sourceSprite.GetCollidingSpritesAtPosition(positionToCheck: sourceSprite.position, cellGroupsToCheck: new List<Cell.Group> { Cell.Group.ColPlantGrowth });

            foreach (Sprite targetSprite in collidingSpritesList)
            {
                if (swayEventsBySpriteID.ContainsKey(targetSprite.id))
                {
                    if (swayEventsBySpriteID[targetSprite.id].sourceSprite.id != sourceSprite.id) return;

                    swayEventsBySpriteID[targetSprite.id].Finish();
                    swayEventsBySpriteID.Remove(targetSprite.id);
                }
                this.AddSwayEvent(sourceSprite: sourceSprite, targetSprite: targetSprite);
            }
        }

        public void AddSwayEvent(Sprite sourceSprite, Sprite targetSprite)
        {
            this.swayEventsBySpriteID[targetSprite.id] = new SwayEvent(sourceSprite: sourceSprite, targetSprite: targetSprite);
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
    }

    public class SwayEvent
    {
        private readonly float originalRotation;
        public readonly Sprite sourceSprite;
        public readonly Sprite targetSprite;
        private readonly Tweener tweener;
        public bool HasEnded { get; private set; }

        public SwayEvent(Sprite sourceSprite, Sprite targetSprite)
        {
            this.HasEnded = false;
            this.sourceSprite = sourceSprite;
            this.targetSprite = targetSprite;
            this.tweener = new Tweener();

            Vector2 sourceOffset = sourceSprite.position - targetSprite.position;

            this.originalRotation = this.sourceSprite.rotation;
            this.targetSprite.rotationOriginOverride = new Vector2(targetSprite.frame.texture.Width / 2f, targetSprite.frame.texture.Height);

            float targetRotation = this.originalRotation + 0.3f; // TODO add proper calculations

            if (sourceOffset.X > 0) targetRotation *= -1;

            float targetDuation = 0.5f; // TODO add proper calculations

            this.tweener.TweenTo(target: this.targetSprite, expression: sprite => sprite.rotation, toValue: targetRotation, duration: targetDuation, delay: 0)
                .AutoReverse()
                .Easing(EasingFunctions.CubicInOut)
                .OnEnd(t => this.Finish());

            this.Update();
        }

        public void Finish()
        {
            this.HasEnded = true;
            this.targetSprite.rotation = this.originalRotation;
            this.targetSprite.rotationOriginOverride = Vector2.Zero;
        }

        public void Update()
        {
            if (!this.targetSprite.boardPiece.exists)
            {
                this.Finish();
                return;
            }

            this.tweener.Update((float)Scene.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}