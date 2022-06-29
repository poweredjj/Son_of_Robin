using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class SolidColor : Scene
    {
        public Color color;
        public readonly bool clearScreen;

        public SolidColor(Color color, float viewOpacity, bool clearScreen = false, int priority = 1) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, blocksDrawsBelow: clearScreen, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.color = color;
            this.viewParams.Opacity = viewOpacity;
            this.clearScreen = clearScreen;
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            if (this.clearScreen) SonOfRobinGame.graphicsDevice.Clear(this.color * this.viewParams.drawOpacity);
            else
            {
                Rectangle rect = new Rectangle(0, 0, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight);
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, rect, this.color * this.viewParams.drawOpacity);
            }
        }

    }
}
