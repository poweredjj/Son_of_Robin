using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class SketchInstance : EffInstance
    {
        private readonly Vector4 fgColor;
        private readonly Vector4 bgColor;

        public SketchInstance(Color fgColor, Color bgColor, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectSketch, framesLeft: framesLeft, priority: priority)
        {
            this.fgColor = fgColor.ToVector4();
            this.bgColor = bgColor.ToVector4();
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["fgColor"].SetValue(this.fgColor);
            this.effect.Parameters["bgColor"].SetValue(this.bgColor);

            base.TurnOn(currentUpdate);
        }
    }
}