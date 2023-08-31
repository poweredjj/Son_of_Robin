﻿using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class ColorizeInstance : EffInstance
    {
        private readonly Vector4 color;
        private readonly int fadeFramesTotal;
        private int fadeFramesLeft;

        public ColorizeInstance(Color color, int framesLeft = 1, int priority = 1, int fadeFramesLeft = 0) : base(effect: SonOfRobinGame.EffectColorize, framesLeft: framesLeft, priority: priority)
        {
            this.color = color.ToVector4();
            this.framesLeft = framesLeft;
            this.fadeFramesTotal = fadeFramesLeft;
            this.fadeFramesLeft = fadeFramesLeft;
        }

        public override void TurnOn(int currentUpdate)
        {
            float opacity = 1f;
            if (this.fadeFramesTotal > 0)
            {
                this.fadeFramesLeft--;
                opacity = (float)this.fadeFramesLeft / (float)this.fadeFramesTotal;
            }

            this.effect.Parameters["colorizeColor"].SetValue(this.color);
            this.effect.Parameters["opacity"].SetValue(opacity);

            base.TurnOn(currentUpdate);
        }
    }
}