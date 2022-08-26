using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class MapRenderer : Scene
    {
        // Simple renderer of final map surface.
        // Allows for transitions separate from base map.

        private readonly Map map;
        public MapRenderer(Map map) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.map = map;
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw()
        {
            if (this.map.FinalMapToDisplay == null) return;


            SonOfRobinGame.spriteBatch.End();
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);

            SonOfRobinGame.spriteBatch.Draw(this.map.FinalMapToDisplay, this.map.FinalMapToDisplay.Bounds, Color.White * this.viewParams.drawOpacity);
        }

    }
}
