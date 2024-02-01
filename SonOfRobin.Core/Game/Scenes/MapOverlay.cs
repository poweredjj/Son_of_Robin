using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class MapOverlay : Scene
    {
        public enum Corner: byte { TopLeft, TopRight, BottomLeft, BottomRight }

        // Simple renderer of final map surface.
        // Allows for transitions independent from base map.

        private static readonly Color frameColor = new(71, 37, 0);

        private readonly Map map;
        private Rectangle drawRect;
        private Corner corner;

        public MapOverlay(Map map) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.map = map;
            this.corner = Preferences.mapCorner;
            this.viewParams.Opacity = 0;
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

                    bool atLeft = Preferences.mapCorner == Corner.BottomLeft || Preferences.mapCorner == Corner.TopLeft;
                    bool atTop = Preferences.mapCorner == Corner.TopLeft || Preferences.mapCorner == Corner.TopRight;

                    int widthMini = (int)((float)Map.FinalMapToDisplaySize.X / scaleMini);
                    int heightMini = (int)((float)Map.FinalMapToDisplaySize.Y / scaleMini);
                    int margin = (int)(SonOfRobinGame.ScreenWidth * 0.01f);

                    int posXMini = atLeft ? margin : (int)((SonOfRobinGame.ScreenWidth - widthMini) * scaleMini) - margin;
                    int posYMini = atTop ? margin : (int)((SonOfRobinGame.ScreenHeight - heightMini) * scaleMini) - margin;

                    this.viewParams.PosX = posXMini;
                    this.viewParams.PosY = posYMini;
                   
                    this.viewParams.ScaleX = scaleMini;
                    this.viewParams.ScaleY = scaleMini;

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: setInstantly ? 1 : duration, endCopyToBase: true,
                        paramsToChange: new Dictionary<string, float> { { "Opacity", 0.7f } });

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

            this.corner = Preferences.mapCorner;
        }

        public override void Update()
        {
            this.map.blocksDrawsBelow = this.map.Mode == Map.MapMode.Full && this.viewParams.ScaleX == 1;
            if (this.corner != Preferences.mapCorner) this.AddTransition(setInstantly: false);
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

            this.drawRect = new Rectangle(0, 0, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight);

            SonOfRobinGame.SpriteBatch.Draw(this.map.FinalMapToDisplay, this.drawRect, Color.White * opacity * this.viewParams.drawOpacity);

            if (this.map.Mode == Map.MapMode.Mini && !this.transManager.HasAnyTransition) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.drawRect, color: frameColor * opacity * 0.7f, thickness: 6);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}