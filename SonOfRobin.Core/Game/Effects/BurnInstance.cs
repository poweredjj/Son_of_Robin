namespace SonOfRobin
{
    public class BurnInstance : EffInstance
    {
        private readonly float intensity;
        private readonly float phaseModifier; // to diversify animations between boardPieces

        public BurnInstance(float intensity, BoardPiece boardPiece, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBurn, framesLeft: framesLeft, priority: priority)
        {
            this.intensity = intensity;
            this.phaseModifier = boardPiece.GetType() == typeof(Plant) ? boardPiece.sprite.position.X : 1;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["intensity"].SetValue(this.intensity);
            this.effect.Parameters["time"].SetValue(SonOfRobinGame.CurrentUpdate / 35f);
            this.effect.Parameters["phaseModifier"].SetValue(this.phaseModifier / 10f);

            base.TurnOn(currentUpdate);
        }
    }
}