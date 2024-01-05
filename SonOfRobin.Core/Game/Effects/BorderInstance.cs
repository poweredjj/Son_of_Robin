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
        private readonly int cropXMin;
        private readonly int cropXMax;
        private readonly int cropYMin;
        private readonly int cropYMax;

        public BorderInstance(Color outlineColor, Vector2 textureSize, bool drawFill = true, int borderThickness = 1, int framesLeft = 1, int priority = 1, Rectangle cropRect = default) : base(effect: SonOfRobinGame.EffectBorder, framesLeft: framesLeft, priority: priority)
        {
            this.outlineColor = outlineColor.ToVector4();
            this.outlineThickness = Math.Max(borderThickness, 1);
            this.drawFill = drawFill;
            this.textureSize = textureSize;

            if (cropRect == default)
            {
                this.cropXMin = 0;
                this.cropXMax = (int)textureSize.X;
                this.cropYMin = 0;
                this.cropYMax = (int)textureSize.Y;
            }
            else
            {
                this.cropXMin = cropRect.Left;
                this.cropXMax = cropRect.Right;
                this.cropYMin = cropRect.Top;
                this.cropYMax = cropRect.Bottom;
            }
        }

        public override void TurnOn(int currentUpdate, Color drawColor)
        {
            this.effect.Parameters["outlineColor"].SetValue(this.outlineColor);
            this.effect.Parameters["outlineThickness"].SetValue((int)(this.outlineThickness * this.intensityForTweener));
            this.effect.Parameters["drawFill"].SetValue(this.drawFill);
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);
            this.effect.Parameters["cropXMin"].SetValue(this.cropXMin);
            this.effect.Parameters["cropXMax"].SetValue(this.cropXMax);
            this.effect.Parameters["cropYMin"].SetValue(this.cropYMin);
            this.effect.Parameters["cropYMax"].SetValue(this.cropYMax);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}