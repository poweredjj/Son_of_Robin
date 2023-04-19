namespace SonOfRobin
{
    public class BurnInstance : EffInstance
    {
        private readonly float intensity;

        public BurnInstance(float intensity, int framesLeft = 1, int priority = 1) : base(effect: SonOfRobinGame.EffectBurn, framesLeft: framesLeft, priority: priority)
        {
            this.intensity = intensity;
        }

        public override void TurnOn(int currentUpdate)
        {
            this.effect.Parameters["intensity"].SetValue(this.intensity);
            this.effect.Parameters["time"].SetValue(SonOfRobinGame.CurrentUpdate / 35f);

            base.TurnOn(currentUpdate);
        }
    }
}