using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class BorderInstance : EffInstance
    {

        private readonly Vector3 outlineColor;
        private readonly Vector2 textureSize;

        public BorderInstance(Color outlineColor, Vector2 textureSize, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.effectBorder, framesLeft: framesLeft, priority: priority)
        {
            this.outlineColor = outlineColor.ToVector3();
            this.textureSize = textureSize;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["xOutlineColor"].SetValue(this.outlineColor);
            this.effect.Parameters["xTextureSize"].SetValue(this.textureSize);

            base.TurnOn(currentUpdate);
        }

    }
}
