using Microsoft.Xna.Framework;
using System;

namespace SonOfRobin
{
    public class BorderInstance : EffInstance
    {
        private readonly Vector4 outlineColor;
        private readonly int outlineThickness;
        private readonly bool drawFill;
        private readonly Vector2 textureSize;

        public BorderInstance(Color outlineColor, Vector2 textureSize, bool drawFill = true, int borderThickness = 1, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBorder, framesLeft: framesLeft, priority: priority)
        {
            this.outlineColor = outlineColor.ToVector4();
            this.outlineThickness = Math.Max(borderThickness, 1);
            this.drawFill = drawFill;
            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate, Color drawColor)
        {
            this.effect.Parameters["outlineColor"].SetValue(this.outlineColor);
            this.effect.Parameters["outlineThickness"].SetValue((int)(this.outlineThickness * this.intensityForTweener));
            this.effect.Parameters["drawFill"].SetValue(this.drawFill);
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}