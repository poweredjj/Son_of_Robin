﻿using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class BlurInstance : EffInstance
    {
        public Vector2 blurSize;
        private readonly Vector2 textureSize;

        public BlurInstance(Vector2 blurSize, Vector2 textureSize, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBlur, framesLeft: framesLeft, priority: priority)
        {
            this.blurSize = blurSize;
            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyFirstPass = true)
        {
            this.effect.Parameters["blurSize"].SetValue(Vector2.Max(this.blurSize * this.intensityForTweener, Vector2.One));
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}