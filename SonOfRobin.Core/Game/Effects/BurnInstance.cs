using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class BurnInstance : EffInstance
    {
        private readonly float intensity;
        private readonly float phaseModifier; // to diversify animations between boardPieces
        private readonly bool checkAlpha;

        public BurnInstance(float intensity, BoardPiece boardPiece = null, bool checkAlpha = true, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBurn, framesLeft: framesLeft, priority: priority)
        {
            this.intensity = intensity;
            this.checkAlpha = checkAlpha;
            this.phaseModifier = boardPiece != null && boardPiece.GetType() == typeof(Plant) ? boardPiece.sprite.position.X : 1;
        }

        public override void TurnOn(int currentUpdate, Color drawColor, bool applyPassZero = true)
        {
            this.effect.Parameters["intensity"].SetValue(this.intensity * this.intensityForTweener);
            this.effect.Parameters["time"].SetValue(SonOfRobinGame.CurrentUpdate / 35f);
            this.effect.Parameters["phaseModifier"].SetValue(this.phaseModifier / 10f);
            this.effect.Parameters["checkAlpha"].SetValue(this.checkAlpha);

            base.TurnOn(currentUpdate: currentUpdate, drawColor: drawColor);
        }
    }
}