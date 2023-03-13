using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SwayManager
    {
        public int SwayEventsCount
        { get { return swayEventsBySpriteID.Count; } }
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
        public bool HasEnded { get; private set; }

        public SwayEvent(Sprite sourceSprite, Sprite targetSprite)
        {
            this.HasEnded = false;
            this.sourceSprite = sourceSprite;
            this.targetSprite = targetSprite;

            this.originalRotation = this.sourceSprite.rotation;
            this.targetSprite.rotationOriginOverride = new Vector2(targetSprite.frame.texture.Width / 2f, targetSprite.frame.texture.Height);

            this.Update();
        }

        public void Update()
        {
            if (!this.targetSprite.colRect.Intersects(this.sourceSprite.colRect) || !this.targetSprite.boardPiece.exists)
            {
                this.Finish();
                return;
            }

            Vector2 sourceOffset = sourceSprite.position - targetSprite.position;
            float distance = Vector2.Distance(targetSprite.position, sourceSprite.position);
            float maxDistance = (sourceSprite.colRect.Width / 2) + (targetSprite.colRect.Width / 2);
            float distanceFactor = 1f - (distance / maxDistance);

            float rotationChange = 0.6f * distanceFactor;
            if (sourceOffset.X > 0) rotationChange *= 1;

            this.targetSprite.rotation = this.originalRotation + rotationChange;
        }

        public void Finish()
        {
            this.HasEnded = true;
            this.targetSprite.rotation = this.originalRotation;
            this.targetSprite.rotationOriginOverride = Vector2.Zero;
        }
    }
}