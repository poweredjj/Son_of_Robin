using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class ParticleEngine
    {
        public enum Preset { Empty, Test1 }

        private Preset preset;
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
                case Preset.Test1:
                    textureName = "circle";
                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.preset}'.");
            }

            this.texture = TextureBank.GetTexture(textureName);
            TextureRegion2D textureRegion = new TextureRegion2D(this.texture);

            this.particleEffect = new ParticleEffect(autoTrigger: false)
            {
                Position = this.sprite.position,
                Emitters = new List<ParticleEmitter>
                  {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(2.5),
                    Profile.BoxUniform(100,250))
                    {
                        Parameters = new ParticleReleaseParameters

                        {
                            Speed = new Range<float>(0f, 50f),
                            Quantity = 3,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1.0f, 2.5f)
                        },

                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators =
                                {
                                    new ColorInterpolator
                                    {
                                        StartValue = new HslColor(0.33f, 0.5f, 0.5f),
                                        EndValue = new HslColor(0.5f, 0.9f, 1.0f)
                                    }
                                }
                            },
                            new RotationModifier {RotationRate = -2.1f},
                            new RectangleContainerModifier {Width = 800, Height = 480},
                            new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 30f},
                        }
                    }
                }
            };
        }

        public void Dispose()
        {
            this.particleEffect.Dispose();
        }

        public void Update()
        {
            this.particleEffect.Position = this.sprite.position;
            this.particleEffect.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw()
        {
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffect);
        }
    }
}