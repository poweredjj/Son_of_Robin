using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class EffectCol // collection of effect instances
    {
        private readonly World world;
        private HashSet<EffInstance> effectInstanceSet;

        public EffectCol(World world)
        {
            this.world = world; // used for currentUpdate only
            this.effectInstanceSet = new HashSet<EffInstance> { };
        }

        public bool ThereAreEffectsToRender
        { get { return this.effectInstanceSet.Count > 0; } }

        private List<EffInstance> EffectsLeftToRender
        {
            get
            {
                int currentUpdate = this.world == null ? SonOfRobinGame.CurrentUpdate : this.world.CurrentUpdate;
                return this.effectInstanceSet.Where(effInstance => !effInstance.WasUsedInThisFrame(currentUpdate)).OrderBy(effInstance => effInstance.priority).ToList();
            }
        }

        public void AddEffect(EffInstance effInstance, bool ignoreIfDuplicated = true)
        {
            // Draw() clears effect list, if draw is missing (frameskip) - duplicates will occur
            if (ignoreIfDuplicated && this.ThisEffectHasBeenApplied(effect: effInstance.effect)) return;

            this.effectInstanceSet.Add(effInstance);
        }

        public void RemoveAllEffects()
        {
            this.effectInstanceSet.Clear();
        }

        public void RemoveEffectsOfType(Effect effect)
        {
            this.effectInstanceSet = this.effectInstanceSet.Where(effInstance => effInstance.effect != effect).ToHashSet();
        }

        private bool ThisEffectHasBeenApplied(Effect effect)
        {
            foreach (EffInstance effInstance in this.effectInstanceSet)
            { if (effInstance.effect == effect) return true; }
            return false;
        }

        public bool HasEffectOfType(Type type)
        {
            foreach (EffInstance effect in this.effectInstanceSet)
            {
                if (effect.GetType() == type) return true;
            }

            return false;
        }

        public bool TurnOnNextEffect(Scene scene, int currentUpdateToUse)
        {
            if (this.effectInstanceSet.Count == 0) return false;

            var effectsLeftToRender = this.EffectsLeftToRender;

            foreach (EffInstance effInstance in effectsLeftToRender)
            {
                SonOfRobinGame.SpriteBatch.End();
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scene.TransformMatrix, sortMode: SpriteSortMode.Immediate);
                effInstance.TurnOn(currentUpdate: currentUpdateToUse, drawColor: Color.White);
                if (effInstance.framesLeft == 0)
                {
                    this.effectInstanceSet.Remove(effInstance);
                    if (!this.effectInstanceSet.Contains(effInstance)) break;
                }
            }

            return effectsLeftToRender.Count > 1; // this bool means, that there will be more effects after this one
        }
    }
}