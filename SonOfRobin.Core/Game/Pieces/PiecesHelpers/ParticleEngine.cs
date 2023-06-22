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

        public bool IsOn { get; private set; }
        private Preset preset;
        private Sprite sprite;
        private Texture2D texture;
        private ParticleEffect particleEffect;

        public ParticleEngine(Preset preset, Sprite sprite)
        {
            this.IsOn = false;
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
            if (presetsToTurnOn.Contains(this.preset)) this.TurnOn();
        }

        public void TurnOn()
        {
            if (this.IsOn) return;

            TextureRegion2D textureRegion = new TextureRegion2D(this.texture);

            switch (this.preset)
            {
                case Preset.Fireplace:
                    this.particleEffect.Emitters = new List<ParticleEmitter>
                    {
                        new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(2.0), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters

                            {
                                Speed = new Range<float>(12f, 60f),
                                Quantity = 1,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ColorInterpolator
                                        {
                                            StartValue = Color.Yellow.ToHsl(),
                                            EndValue = Color.Orange.ToHsl()
                                        },
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.05f, 0.05f),
                                            EndValue = new Vector2(0.8f, 0.8f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1f,
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

            this.IsOn = true;

            this.Update();
        }

        public void TurnOff()
        {
            if (!this.IsOn) return;

            foreach (ParticleEmitter particleEmitter in this.particleEffect.Emitters)
            {
                particleEmitter.Capacity = 0;
            }

            this.IsOn = false;
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