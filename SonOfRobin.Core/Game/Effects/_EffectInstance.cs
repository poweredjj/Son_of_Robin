﻿using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public abstract class EffInstance // Every effect must have its counterpart in child class, to be used.
    {
        public int framesLeft; // -1 == plays indefinitely
        public readonly int priority;
        public readonly Effect effect;
        private int lastFrameFired;

        public EffInstance(Effect effect, int framesLeft, int priority)
        {
            this.effect = effect;
            this.framesLeft = framesLeft;
            this.priority = priority;
        }

        public bool WasUsedInThisFrame(int currentUpdate)
        {
            return this.lastFrameFired == currentUpdate;
        }

        public virtual void TurnOn(int currentUpdate)
        {
            this.lastFrameFired = currentUpdate;
            if (this.framesLeft > -1) this.framesLeft -= 1;
            effect.CurrentTechnique.Passes[0].Apply();
        }

    }

}
