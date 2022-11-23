using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class ColorizeInstance : EffInstance
    {
        private readonly Vector4 color;

        public ColorizeInstance(Color color, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectColorize, framesLeft: framesLeft, priority: priority)
        {
            this.color = color.ToVector4();
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["newColor"].SetValue(this.color);

            base.TurnOn(currentUpdate);
        }
    }
}