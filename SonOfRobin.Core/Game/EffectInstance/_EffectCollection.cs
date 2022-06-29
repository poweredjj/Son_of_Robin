using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class EffectCol // collection of effect instances
    {
        private readonly World world;
        private readonly Sprite sprite;
        private List<EffInstance> effectInstanceList;
        public bool ThereAreEffectsToRender { get { return this.effectInstanceList.Count > 0; } }

        private List<EffInstance> EffectsLeftToRender
        {
            get
            {
                return this.effectInstanceList.Where(effInstance => !effInstance.WasUsedInThisFrame(this.world.currentUpdate)).OrderBy(effInstance => effInstance.priority).ToList();
            }
        }

        public EffectCol(Sprite sprite)
        {
            this.sprite = sprite;
            this.world = sprite.world;
            this.effectInstanceList = new List<EffInstance> { };
        }

        public void AddEffect(EffInstance effInstance, bool ignoreIfDuplicated = true)
        {
            //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"{this.sprite.boardPiece.id} {effInstance.GetType()} {this.world.currentUpdate}");

            // Draw() clears effect list, if draw is missing (frameskip) - duplicates will occur
            if (ignoreIfDuplicated && this.ThisEffectHasBeenApplied(effect: effInstance.effect))
            {
                //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"duplicated {this.sprite.boardPiece.id} {this.world.currentUpdate}", color: Color.Cyan);
                return;
            }
            this.effectInstanceList.Add(effInstance);
        }

        public void RemoveAllEffects()
        {
            this.effectInstanceList.Clear();
        }

        public void RemoveEffectsOfType(Effect effect)
        {
            this.effectInstanceList = this.effectInstanceList.Where(effInstance => effInstance.effect != effect).ToList();
        }

        private bool ThisEffectHasBeenApplied(Effect effect)
        {
            foreach (EffInstance effInstance in this.effectInstanceList)
            { if (effInstance.effect == effect) return true; }
            return false;
        }

        public bool TurnOnNextEffect(World world)
        {
            if (this.effectInstanceList.Count == 0) return false;

            var effectsLeftToRender = this.EffectsLeftToRender;

            foreach (EffInstance effInstance in effectsLeftToRender)
            {
                world.StartNewSpriteBatch(enableEffects: true);
                effInstance.TurnOn(world.currentUpdate);
                if (effInstance.framesLeft == 0)
                {
                    this.effectInstanceList.Remove(effInstance);
                    if (!this.effectInstanceList.Contains(effInstance)) break;
                }
            }

            return effectsLeftToRender.Count > 1; // this bool means, that there will be more effects after this one
        }

    }

}
