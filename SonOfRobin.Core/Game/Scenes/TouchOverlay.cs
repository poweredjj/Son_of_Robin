using Microsoft.Xna.Framework;

namespace SonOfRobin

{
    public class TouchOverlay : Scene
    {
        public TouchOverlay() : base(inputType: InputTypes.Always, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Uninitialized, tipsLayout: ControlTips.TipsLayout.Empty)
        { }

        public override void Remove()
        {
            base.Remove();
            VirtButton.RemoveAll();
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            if (TouchInput.showSticks && Preferences.enableTouchJoysticks) TouchInput.dualStick.Draw(spriteBatch: SonOfRobinGame.spriteBatch);
            VirtButton.DrawAll();
        }
    }
}
