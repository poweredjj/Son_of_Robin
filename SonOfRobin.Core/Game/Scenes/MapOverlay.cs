using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class MapOverlay : Scene
    {
        // Simple renderer of final map surface.
        // Allows for transitions independent from base map.

        private readonly Map map;

        public MapOverlay(Map map) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.map = map;
        }

        public void AddTransition(bool setInstantly)
        {
            int duration = 10;

            switch (this.map.Mode)
            {
                case Map.MapMode.Off:

                    if (setInstantly) this.viewParams.Opacity = 0f;
                    else
                    {
                        this.transManager.AddMultipleTransitions(outTrans: true, duration: duration, endCopyToBase: true, paramsToChange: new Dictionary<string, float> { { "Opacity", 0f } });
                    }

                    break;

                case Map.MapMode.Mini:
                    float scaleMini = 3f;

                    int widthMini = (int)((float)Map.FinalMapToDisplaySize.X / scaleMini);
                    int heightMini = (int)((float)Map.FinalMapToDisplaySize.Y / scaleMini);
                    int margin = (int)(SonOfRobinGame.ScreenWidth * 0.05f);

                    int posXMini = (int)((SonOfRobinGame.ScreenWidth - (widthMini + margin)) * scaleMini);
                    int posYMini = (int)((SonOfRobinGame.ScreenHeight - (heightMini + margin)) * scaleMini);

                    this.viewParams.PosX = posXMini;
                    this.viewParams.PosY = posYMini;
                    if (!setInstantly) this.viewParams.PosY += heightMini * scaleMini;

                    this.viewParams.Opacity = 0.7f;
                    this.viewParams.ScaleX = scaleMini;
                    this.viewParams.ScaleY = scaleMini;

                    if (!setInstantly) this.transManager.AddMultipleTransitions(outTrans: true, duration: duration, endCopyToBase: true, paramsToChange: new Dictionary<string, float> { { "PosY", posYMini } });

                    break;

                case Map.MapMode.Full:
                    if (setInstantly)
                    {
                        this.viewParams.Opacity = 1f;
                        this.viewParams.PosX = 0;
                        this.viewParams.PosY = 0;
                        this.viewParams.ScaleX = 1f;
                        this.viewParams.ScaleY = 1f;
                    }
                    else
                    {
                        this.transManager.AddMultipleTransitions(outTrans: true, duration: duration, endCopyToBase: true,
                            paramsToChange: new Dictionary<string, float> {
                        { "Opacity", 1f },
                        { "PosX", 0 },
                        { "PosY", 0 },
                        { "ScaleX", 1f },
                        { "ScaleY", 1f },
                        });
                    }

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
            SonOfRobinGame.SpriteBatch.Draw(this.map.FinalMapToDisplay, new Rectangle(0, 0, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight), Color.White * opacity * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.End();
        }
    }
}