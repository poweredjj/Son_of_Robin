namespace SonOfRobin
{
    public class RollingText : Scene
    {
        public RollingText(int priority = 1) : base(inputType: InputTypes.None, priority: priority, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            // TODO add code
        }

        public override void Update()
        {
            // TODO add code
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            // TODO add code

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}