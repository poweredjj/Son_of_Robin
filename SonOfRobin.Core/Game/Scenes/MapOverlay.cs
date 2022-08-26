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
            int margin = (int)(SonOfRobinGame.VirtualWidth * 0.05f);
            float miniScale = 3f;

            int miniWidth = (int)(this.map.FinalMapToDisplay.Width / miniScale);
            int miniHeight = (int)(this.map.FinalMapToDisplay.Height / miniScale);

            int miniPosX = (int)((SonOfRobinGame.VirtualWidth - miniWidth - margin) * miniScale);
            int miniPosY = (int)((SonOfRobinGame.VirtualHeight - miniHeight - margin) * miniScale);

            switch (this.map.Mode)
            {
                case Map.MapMode.Off:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 15,
                        paramsToChange: new Dictionary<string, float> {
                        { "Opacity", 0f },
                        });

                    break;

                case Map.MapMode.Mini:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 15,
                        paramsToChange: new Dictionary<string, float> {
                        { "Opacity", 0.7f },
                        { "PosX", miniPosX },
                        { "PosY", miniPosY },
                        { "ScaleX", miniScale },
                        { "ScaleY", miniScale },
                        });

                    break;

                case Map.MapMode.Full:

                    this.transManager.AddMultipleTransitions(outTrans: true, duration: 15,
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

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            if ((this.map.Mode == Map.MapMode.Off && !this.transManager.HasAnyTransition) || this.map.FinalMapToDisplay == null) return;

            SonOfRobinGame.spriteBatch.End();

            var mapBlend = BlendState.AlphaBlend;

            //var mapBlend = new BlendState
            //{
            //    AlphaBlendFunction = BlendFunction.Add,
            //    AlphaSourceBlend = Blend.SourceAlpha,
            //    AlphaDestinationBlend = Blend.DestinationAlpha,

            //    ColorBlendFunction = BlendFunction.Add,
            //    ColorSourceBlend = Blend.SourceColor,
            //    ColorDestinationBlend = Blend.DestinationColor,
            //};

            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix, blendState: mapBlend);

            SonOfRobinGame.spriteBatch.Draw(this.map.FinalMapToDisplay, this.map.FinalMapToDisplay.Bounds, Color.White * this.viewParams.drawOpacity);

            SonOfRobinGame.spriteBatch.End();
            SonOfRobinGame.spriteBatch.Begin();
        }

    }
}
