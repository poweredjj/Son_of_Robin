using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class BurnInstance : EffInstance
    {
        private readonly Vector4 color;
        private readonly float opacity;

        public BurnInstance(Color color, float opacity, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBurn, framesLeft: framesLeft, priority: priority)
        {
            this.color = color.ToVector4();
            this.opacity = opacity;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["newColor"].SetValue(this.color);
            this.effect.Parameters["opacity"].SetValue(this.opacity);

            base.TurnOn(currentUpdate);
        }
    }
}