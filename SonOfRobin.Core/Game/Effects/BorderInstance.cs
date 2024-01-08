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
        private readonly float cropXMin;
        private readonly float cropXMax;
        private readonly float cropYMin;
        private readonly float cropYMax;

        public BorderInstance(Color outlineColor, Vector2 textureSize, bool drawFill = true, int borderThickness = 1, int framesLeft = 1, int priority = 1, Rectangle cropRect = default) : base(effect: SonOfRobinGame.EffectBorder, framesLeft: framesLeft, priority: priority)
        {
            this.outlineColor = outlineColor.ToVector4();
            this.outlineThickness = Math.Max(borderThickness, 1);
            this.drawFill = drawFill;
            this.textureSize = textureSize;

            if (cropRect == default)
            {
                this.cropXMin = 0f;
                this.cropXMax = 1f;
                this.cropYMin = 0f;
                this.cropYMax = 1f;
            }
            else
            {
                this.cropXMin = (float)cropRect.Left / textureSize.X;
                this.cropXMax = (float)cropRect.Right / textureSize.X;
                this.cropYMin = (float)cropRect.Top / textureSize.Y;
                this.cropYMax = (float)cropRect.Bottom / textureSize.Y;
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