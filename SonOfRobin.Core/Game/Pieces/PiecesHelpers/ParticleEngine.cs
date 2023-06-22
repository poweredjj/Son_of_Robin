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
using System.Linq;

namespace SonOfRobin
{
    public class ParticleEngine
    {
        public enum Preset { Empty, Fireplace }

        private static readonly List<Preset> presetsToTurnOn = new List<Preset> { };

        private Preset preset;
        private readonly int defaultParticlesQuantity;
        private int currentParticlesQuantity;
        private Sprite sprite;
        private Texture2D texture;
        private ParticleEffect particleEffect;
        public bool HasAnyParticles { get { return this.ActiveParticlesCount > 0; } }

        public int ActiveParticlesCount
        { get { return this.particleEffect.Emitters.Select(x => x.ActiveParticles).Sum(); } }

        public ParticleEngine(Preset preset, Sprite sprite)
        {
            this.preset = preset;
            this.sprite = sprite;
            this.currentParticlesQuantity = 0;

            string textureName;

            switch (this.preset)
            {
                case Preset.Fireplace:
                    textureName = "circle_16x16";
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

                    this.defaultParticlesQuantity = 1;

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
            this.currentParticlesQuantity = particlesQuantity == 0 ? this.defaultParticlesQuantity : particlesQuantity;

            foreach (ParticleEmitter particleEmitter in this.particleEffect.Emitters)
            {
                particleEmitter.Parameters.Quantity = this.currentParticlesQuantity;
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
        }

        public bool Equals(ParticleEngine particleEngine)
        {
            if (particleEngine == null) return false;

            return
                this.preset == particleEngine.preset &&
                this.currentParticlesQuantity == particleEngine.currentParticlesQuantity;
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> particleData = new Dictionary<string, object>
            {
                { "preset", this.preset },
                { "currentParticlesQuantity", this.currentParticlesQuantity },
            };

            return particleData;
        }

        public static ParticleEngine Deserialize(Object particleData, Sprite sprite)
        {
            var particleDict = (Dictionary<string, Object>)particleData;

            Preset preset = (Preset)(Int64)particleDict["preset"];
            int currentParticlesQuantity = (int)(Int64)particleDict["currentParticlesQuantity"];

            ParticleEngine particleEngine = new ParticleEngine(preset: preset, sprite: sprite);

            if (currentParticlesQuantity > 0) particleEngine.TurnOn(currentParticlesQuantity);
            return particleEngine;
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