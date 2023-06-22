using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class ParticleEngine
    {
        public enum Preset { Empty, Fireplace }

        private static readonly List<Preset> presetsToTurnOn = new List<Preset> { };

        private Preset preset;
        private readonly int particlesQuantity;
        private Sprite sprite;
        private Texture2D texture;
        private ParticleEffect particleEffect;

        public ParticleEngine(Preset preset, Sprite sprite)
        {
            this.preset = preset;
            this.sprite = sprite;

            string textureName;

            switch (this.preset)
            {
                case Preset.Fireplace:
                    textureName = "circle";
                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.preset}'.");
            }

            this.texture = TextureBank.GetTexture(textureName);
            this.particleEffect = new ParticleEffect(autoTrigger: false);

            TextureRegion2D textureRegion = new TextureRegion2D(this.texture);

            switch (this.preset)
            {
                case Preset.Fireplace:

                    this.particlesQuantity = 1;

                    this.particleEffect.Emitters = new List<ParticleEmitter>
                    {
                        new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(1.5), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(5f, 20f),
                                Quantity = 0,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ColorInterpolator
                                        {
                                            StartValue = HslColor.FromRgb(Color.Yellow),
                                            EndValue = HslColor.FromRgb(Color.Yellow)
                                        },
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.02f),
                                            EndValue = new Vector2(0.5f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.8f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 45f},
                            }
                        }
                    };

                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.preset}'.");
            }

            if (presetsToTurnOn.Contains(this.preset)) this.TurnOn();
        }

        public void TurnOn(int particlesQuantity = 0)
        {
            foreach (ParticleEmitter particleEmitter in this.particleEffect.Emitters)
            {
                particleEmitter.Parameters.Quantity = particlesQuantity == 0 ? this.particlesQuantity : particlesQuantity;
            }

            this.Update();
        }

        public void TurnOff()
        {
            foreach (ParticleEmitter particleEmitter in this.particleEffect.Emitters)
            {
                particleEmitter.Parameters.Quantity = 0;
            }
        }

        public void Dispose()
        {
            this.particleEffect.Dispose();
            this.sprite.particleEngine = null;
        }

        public void Update()
        {
            switch (this.preset)
            {
                case Preset.Fireplace:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.preset}'.");
            }

            this.particleEffect.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw()
        {
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffect);
        }
    }
}