using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class MapOverlay : Scene
    {
        // Simple renderer of final map surface.
        // Allows for transitions separate from base map.

        private readonly Map map;
        public MapOverlay(Map map) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.map = map;
        }

        public void AddTransition()
        {

            switch (this.map.Mode)
            {
                case Map.MapMode.Off:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 15,
                      paramsToChange: new Dictionary<string, float> { { "Opacity", 0f } });

                    break;

                case Map.MapMode.Mini:

                    // TODO add transition code

                    break;

                case Map.MapMode.Full:

                    // this.viewParams.Opacity = 0f;

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 15,
                        paramsToChange: new Dictionary<string, float> { { "Opacity", 1f } });

                    break;

                default:
                    throw new ArgumentException($"Unsupported mode - {this.map.Mode}.");
            }

        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            if ((this.map.Mode == Map.MapMode.Off && !this.transManager.HasAnyTransition) || this.map.FinalMapToDisplay == null) return;

            SonOfRobinGame.spriteBatch.End();

            var mapBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            // SonOfRobinGame.spriteBatch.Begin(blendState: mapBlend);
            SonOfRobinGame.spriteBatch.Begin();

            SonOfRobinGame.spriteBatch.Draw(this.map.FinalMapToDisplay, this.map.FinalMapToDisplay.Bounds, Color.White * this.viewParams.drawOpacity);

            SonOfRobinGame.spriteBatch.End();
            SonOfRobinGame.spriteBatch.Begin();
        }

    }
}
