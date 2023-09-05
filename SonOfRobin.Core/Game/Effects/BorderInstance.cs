using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class BorderInstance : EffInstance
    {
        private readonly Vector3 outlineColor;
        private readonly bool drawFill;
        private readonly Vector2 textureSize;
        private readonly int borderThickness;

        public BorderInstance(Color outlineColor, Vector2 textureSize, bool drawFill = true, int borderThickness = 1, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBorder, framesLeft: framesLeft, priority: priority)
        {
            this.outlineColor = outlineColor.ToVector3();
            this.drawFill = drawFill;
            this.textureSize = textureSize;
            this.borderThickness = borderThickness;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["outlineColor"].SetValue(this.outlineColor);
            this.effect.Parameters["drawFill"].SetValue(this.drawFill);
            this.effect.Parameters["textureSize"].SetValue(this.textureSize);
            this.effect.Parameters["borderThickness"].SetValue(this.borderThickness);

            base.TurnOn(currentUpdate);
        }
    }
}