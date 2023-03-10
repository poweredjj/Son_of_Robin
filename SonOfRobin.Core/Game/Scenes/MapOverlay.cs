using Microsoft.Xna.Framework;
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
            int margin = (int)(SonOfRobinGame.VirtualWidth * 0.05f);
            float scaleMini = 3f;
            float scaleFull = 1f;

            int widthMini = (int)(this.map.FinalMapToDisplay.Width / scaleMini);
            int heightMini = (int)(this.map.FinalMapToDisplay.Height / scaleMini);

            int posXMini = (int)((SonOfRobinGame.VirtualWidth - widthMini - margin) * scaleMini);
            int posYMini = (int)((SonOfRobinGame.VirtualHeight - heightMini - margin) * scaleMini);

            int posXFull = 0;
            int posYFull = 0;

            float opacityMini = 0.7f;
            float opacityFull = 1f;

            switch (this.map.Mode)
            {
                case Map.MapMode.Off:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 10, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> {
                        { "Opacity", 0f },
                        });

                    break;

                case Map.MapMode.Mini:

                    this.viewParams.PosX = posXMini;
                    this.viewParams.PosY = SonOfRobinGame.VirtualHeight * scaleMini;
                    this.viewParams.Opacity = opacityMini;
                    this.viewParams.ScaleX = scaleMini;
                    this.viewParams.ScaleY = scaleMini;

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 10, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> {
                        { "PosX", posXMini },
                        { "PosY", posYMini },
                        { "ScaleX", scaleMini },
                        { "ScaleY", scaleMini },
                        });

                    break;

                case Map.MapMode.Full:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 10, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> {
                        { "Opacity", opacityFull },
                        { "PosX", posXFull },
                        { "PosY", posYFull },
                        { "ScaleX", scaleFull },
                        { "ScaleY", scaleFull },
                        });

                    break;

                default:
                    throw new ArgumentException($"Unsupported mode - {this.map.Mode}.");
            }
        }

        public override void Update(GameTime gameTime)
        {
            this.map.blocksDrawsBelow = this.map.Mode == Map.MapMode.Full && this.viewParams.ScaleX == 1;
        }

        public override void Draw()
        {
            if ((this.map.Mode == Map.MapMode.Off && !this.transManager.HasAnyTransition) || this.map.FinalMapToDisplay == null) return;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.SpriteBatch.Draw(this.map.FinalMapToDisplay, this.map.FinalMapToDisplay.Bounds, Color.White * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.End();
        }
    }
}