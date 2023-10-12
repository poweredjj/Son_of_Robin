using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct TriSliceBG
    {
        public enum Preset : byte
        {
            Message,
            Buff,
            MenuGray,
            MenuBrown,
            MenuSilver,
            MenuGold,
        }

        private static readonly Dictionary<Preset, TriSliceBG> bgByPreset = new Dictionary<Preset, TriSliceBG>();

        private readonly Texture2D textureLeft;
        private readonly Texture2D textureMid;
        private readonly Texture2D textureRight;

        private float textureLeftScaledWidth;
        private float textureMidScaledWidth;
        private float textureRightScaledWidth;

        private Rectangle leftRect;
        private Rectangle midRect;
        private Rectangle rightRect;

        private int midTextureRepeats;
        private Rectangle midSourceBoundsRect;

        private TriSliceBG(Texture2D textureLeft, Texture2D textureMid, Texture2D textureRight)
        {
            if (textureLeft.Height != textureMid.Height || textureLeft.Height != textureRight.Height) throw new ArgumentException("Height of all textures must be equal.");

            this.textureLeft = textureLeft;
            this.textureMid = textureMid;
            this.textureRight = textureRight;

            this.textureLeftScaledWidth = 0;
            this.textureMidScaledWidth = 0;
            this.textureRightScaledWidth = 0;

            this.leftRect = new Rectangle();
            this.midRect = new Rectangle();
            this.rightRect = new Rectangle();

            this.midTextureRepeats = 1;
            this.midSourceBoundsRect = this.textureMid.Bounds;
        }

        public static void StartSpriteBatch(Scene scene)
        {
            // needed for mid textures with Width > 1 (for proper wrapping)
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scene.TransformMatrix, blendState: BlendState.AlphaBlend, samplerState: SamplerState.LinearWrap);
        }

        public void Draw(Rectangle triSliceRect, Color color, bool keepExactRectSize = false)
        {
            float scale = (float)triSliceRect.Height / (float)this.textureLeft.Height;

            this.textureLeftScaledWidth = (float)(this.textureLeft.Width * scale);
            this.textureMidScaledWidth = Math.Max((float)this.textureMid.Width * scale, 1);
            this.textureRightScaledWidth = (float)this.textureRight.Width * scale;

            this.leftRect.X = triSliceRect.X;
            this.leftRect.Y = triSliceRect.Y;
            this.leftRect.Width = (int)textureLeftScaledWidth;
            this.leftRect.Height = triSliceRect.Height;

            this.midRect.X = this.leftRect.Right;
            this.midRect.Y = triSliceRect.Y;

            if (keepExactRectSize || this.textureMid.Width == 1)
            {
                // keeping exact size (but mid texture might be cut)
                this.midRect.Width = (int)(triSliceRect.Width - textureLeftScaledWidth - textureRightScaledWidth);
            }
            else
            {
                // making sure, that last mid texture occurence is drawn fully (but drawn rect might be slightly wider)
                this.midTextureRepeats = (int)Math.Ceiling((triSliceRect.Width - (textureLeftScaledWidth + textureRightScaledWidth)) / textureMidScaledWidth);
                this.midRect.Width = (int)textureMidScaledWidth * this.midTextureRepeats;
            }

            this.midRect.Height = triSliceRect.Height;

            this.midSourceBoundsRect.Width = this.textureMid.Width * this.midTextureRepeats;
            this.midSourceBoundsRect.Height = this.textureMid.Height;

            this.rightRect.X = this.midRect.Right;
            this.rightRect.Y = triSliceRect.Y;
            this.rightRect.Width = (int)textureRightScaledWidth;
            this.rightRect.Height = triSliceRect.Height;

            SonOfRobinGame.SpriteBatch.Draw(this.textureLeft, this.leftRect, this.textureLeft.Bounds, color);
            SonOfRobinGame.SpriteBatch.Draw(this.textureMid, this.midRect, this.midSourceBoundsRect, color);
            SonOfRobinGame.SpriteBatch.Draw(this.textureRight, this.rightRect, this.textureRight.Bounds, color);
        }

        public static TriSliceBG GetBGForPreset(Preset preset)
        {
            if (!bgByPreset.ContainsKey(preset))
            {
                TriSliceBG triSliceBG = preset switch
                {
                    Preset.Message => new TriSliceBG(
                                            textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogLeft),
                                            textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogMid),
                                            textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogRight)),

                    Preset.Buff => new TriSliceBG(
                                            textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGBuffLeft),
                                            textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGBuffMid),
                                            textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGBuffRight)),

                    Preset.MenuBrown => new TriSliceBG(
                                                textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuBrownLeft),
                                                textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuBrownMid),
                                                textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuBrownRight)),

                    Preset.MenuGray => new TriSliceBG(
                                                textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuGrayLeft),
                                                textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuGrayMid),
                                                textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuGrayRight)),

                    Preset.MenuSilver => new TriSliceBG(
                                                textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuSilverLeft),
                                                textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuSilverMid),
                                                textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuSilverRight)),

                    Preset.MenuGold => new TriSliceBG(
                                            textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuGoldLeft),
                                            textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuGoldMid),
                                            textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMenuGoldRight)),

                    _ => throw new ArgumentException($"Unsupported preset - {preset}."),
                };

                bgByPreset[preset] = triSliceBG;
            }

            return bgByPreset[preset];
        }
    }
}