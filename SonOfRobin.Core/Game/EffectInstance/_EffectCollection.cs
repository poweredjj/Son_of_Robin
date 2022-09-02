using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class EffectCol // collection of effect instances
    {
        private readonly World world;
        private List<EffInstance> effectInstanceList;
        public bool ThereAreEffectsToRender { get { return this.effectInstanceList.Count > 0; } }

        private List<EffInstance> EffectsLeftToRender
        {
            get
            {
                int currentUpdate = this.world == null ? SonOfRobinGame.currentUpdate : this.world.currentUpdate;
                return this.effectInstanceList.Where(effInstance => !effInstance.WasUsedInThisFrame(currentUpdate)).OrderBy(effInstance => effInstance.priority).ToList();
            }
        }

        public EffectCol(World world)
        {
            this.world = world; // used for currentUpdate only
            this.effectInstanceList = new List<EffInstance> { };
        }

        public void AddEffect(EffInstance effInstance, bool ignoreIfDuplicated = true)
        {
            // Draw() clears effect list, if draw is missing (frameskip) - duplicates will occur
            if (ignoreIfDuplicated && this.ThisEffectHasBeenApplied(effect: effInstance.effect)) return;

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

        public bool TurnOnNextEffect(Scene scene, int currentUpdateToUse)
        {
            if (this.effectInstanceList.Count == 0) return false;

            var effectsLeftToRender = this.EffectsLeftToRender;

            foreach (EffInstance effInstance in effectsLeftToRender)
            {
                scene.StartNewSpriteBatch(enableEffects: true);
                effInstance.TurnOn(currentUpdateToUse);
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
