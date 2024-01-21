using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class BlendStateEditor : Scene
    {
        // Allows for brute forcing needed BlendState settings.

        private static BlendState TargetBlendState { get { return World.lightBlend; } set { World.lightBlend = value; } }

        private readonly BlendState blendState;
        private readonly SpriteFontBase font;

        public BlendStateEditor() : base(inputType: InputTypes.Always, priority: 0, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.blendState = MakeBlendStateCopy(TargetBlendState);
            this.font = SonOfRobinGame.FontVCROSD.GetFont(18);
        }

        private static BlendState MakeBlendStateCopy(BlendState blendState)
        {
            return new BlendState
            {
                AlphaBlendFunction = blendState.AlphaBlendFunction,
                AlphaSourceBlend = blendState.AlphaSourceBlend,
                AlphaDestinationBlend = blendState.AlphaDestinationBlend,

                ColorBlendFunction = blendState.ColorBlendFunction,
                ColorSourceBlend = blendState.ColorSourceBlend,
                ColorDestinationBlend = blendState.ColorDestinationBlend,
            };
        }

        public override void Update()
        {
            bool blendChanged = false;

            if (Keyboard.HasBeenPressed(Keys.Insert))
            {
                this.blendState.AlphaBlendFunction = GetNextBlendFunction(this.blendState.AlphaBlendFunction);
                blendChanged = true;
            }

            if (Keyboard.HasBeenPressed(Keys.Home))
            {
                this.blendState.AlphaSourceBlend = GetNextBlend(this.blendState.AlphaSourceBlend);
                blendChanged = true;
            }
            
            if (Keyboard.HasBeenPressed(Keys.PageUp))
            {
                this.blendState.AlphaDestinationBlend = GetNextBlend(this.blendState.AlphaDestinationBlend);
                blendChanged = true;
            }

            if (Keyboard.HasBeenPressed(Keys.Delete))
            {
                this.blendState.ColorBlendFunction = GetNextBlendFunction(this.blendState.ColorBlendFunction);
                blendChanged = true;
            }

            if (Keyboard.HasBeenPressed(Keys.End))
            {
                this.blendState.ColorSourceBlend = GetNextBlend(this.blendState.ColorSourceBlend);
                blendChanged = true;
            }

            if (Keyboard.HasBeenPressed(Keys.PageDown))
            {
                this.blendState.ColorDestinationBlend = GetNextBlend(this.blendState.ColorDestinationBlend);
                blendChanged = true;
            }

            if (blendChanged) TargetBlendState = MakeBlendStateCopy(this.blendState);
        }

        private static Blend GetNextBlend(Blend blend)
        {
            switch (blend)
            {
                case Blend.One:
                    return Blend.Zero;

                case Blend.Zero:
                    return Blend.SourceColor;

                case Blend.SourceColor:
                    return Blend.InverseSourceColor;

                case Blend.InverseSourceColor:
                    return Blend.SourceAlpha;

                case Blend.SourceAlpha:
                    return Blend.InverseSourceAlpha;

                case Blend.InverseSourceAlpha:
                    return Blend.DestinationColor;

                case Blend.DestinationColor:
                    return Blend.InverseDestinationColor;

                case Blend.InverseDestinationColor:
                    return Blend.DestinationAlpha;

                case Blend.DestinationAlpha:
                    return Blend.InverseDestinationAlpha;

                case Blend.InverseDestinationAlpha:
                    return Blend.BlendFactor;

                case Blend.BlendFactor:
                    return Blend.InverseBlendFactor;

                case Blend.InverseBlendFactor:
                    return Blend.SourceAlphaSaturation;

                case Blend.SourceAlphaSaturation:
                    return Blend.One;

                default:
                    throw new ArgumentException($"Unsupported blend - {blend}.");
            }
        }

        private static BlendFunction GetNextBlendFunction(BlendFunction blendFunction)
        {
            switch (blendFunction)
            {
                case BlendFunction.Add:
                    return BlendFunction.Subtract;

                case BlendFunction.Subtract:
                    return BlendFunction.ReverseSubtract;

                case BlendFunction.ReverseSubtract:
                    return BlendFunction.Add;

                default:
                    throw new ArgumentException($"Unsupported blendFunction - {blendFunction}.");
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

            List<string> textList = [];
            textList.Add($"AlphaBlendFunction: {this.blendState.AlphaBlendFunction}");
            textList.Add($"AlphaSourceBlend: {this.blendState.AlphaSourceBlend}");
            textList.Add($"AlphaDestinationBlend: {this.blendState.AlphaDestinationBlend}");
            textList.Add($"ColorBlendFunction: {this.blendState.ColorBlendFunction}");
            textList.Add($"ColorSourceBlend: {this.blendState.ColorSourceBlend}");
            textList.Add($"ColorDestinationBlend: {this.blendState.ColorDestinationBlend}");


            this.font.DrawText(
                batch: SonOfRobinGame.SpriteBatch,
                text: string.Join("\n", textList),
                position: new Vector2(4, 4),
                color: Color.White,
                scale: Vector2.One,
                effect: FontSystemEffect.Stroked,
                effectAmount: 1);

            SonOfRobinGame.SpriteBatch.End();

        }
    }
}