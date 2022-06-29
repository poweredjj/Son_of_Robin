using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class SolidColor : Scene
    {
        public Color color;
        public readonly bool clearScreen;

        public SolidColor(Color color, float viewOpacity, bool clearScreen, bool inTrans = false) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: clearScreen, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.color = color;
            this.viewParams.opacity = viewOpacity;
            this.clearScreen = clearScreen;

            if (inTrans) this.AddTransition(new Transition(type: Transition.TransType.In, duration: 30, scene: this, blockInput: false, paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }, removeScene: true));
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            if (this.clearScreen)
            { SonOfRobinGame.graphicsDevice.Clear(this.color * this.viewParams.drawOpacity); }
            else
            {
                Rectangle rect = new Rectangle(0, 0, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight);
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, rect, this.color * this.viewParams.drawOpacity);
            }
        }

    }
}
