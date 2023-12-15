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
            switch (this.map.Mode)
            {
                case Map.MapMode.Off:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 10, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> {
                        { "Opacity", 0f },
                        });

                    break;

                case Map.MapMode.Mini:
                    float scaleMini = 3f;

                    int widthMini = (int)((float)this.map.FinalMapToDisplay.Width / scaleMini);
                    int heightMini = (int)((float)this.map.FinalMapToDisplay.Height / scaleMini);
                    int margin = (int)(SonOfRobinGame.VirtualWidth * 0.05f);

                    int posXMini = (int)((SonOfRobinGame.VirtualWidth - (widthMini + margin)) * scaleMini);
                    int posYMini = (int)((SonOfRobinGame.VirtualHeight - (heightMini + margin)) * scaleMini);

                    this.viewParams.PosX = posXMini;
                    this.viewParams.PosY = posYMini + (heightMini * scaleMini);
                    this.viewParams.Opacity = 0.7f;
                    this.viewParams.ScaleX = scaleMini;
                    this.viewParams.ScaleY = scaleMini;

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 10, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> {
                        { "PosY", posYMini },
                        });

                    break;

                case Map.MapMode.Full:
                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 10, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> {
                        { "Opacity", 1f },
                        { "PosX", 0 },
                        { "PosY", 0 },
                        { "ScaleX", 1f },
                        { "ScaleY", 1f },
                        });

                    break;

                default:
                    throw new ArgumentException($"Unsupported mode - {this.map.Mode}.");
            }
        }

        public override void Update()
        {
            this.map.blocksDrawsBelow = this.map.Mode == Map.MapMode.Full && this.viewParams.ScaleX == 1;
        }

        public override void Draw()
        {
            if ((this.map.Mode == Map.MapMode.Off && !this.transManager.HasAnyTransition) ||
                this.map.FinalMapToDisplay == null)
            {
                return;
            }

            float opacity = 1f - this.map.world.cineCurtains.showPercentage;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.SpriteBatch.Draw(this.map.FinalMapToDisplay, new Rectangle(0, 0, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight), Color.White * opacity * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.End();
        }
    }
}