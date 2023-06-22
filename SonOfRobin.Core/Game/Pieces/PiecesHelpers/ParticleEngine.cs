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
        public enum Preset { Empty, Fireplace, WaterSplash }

        private static readonly List<Preset> presetsToTurnOn = new List<Preset> { };
        public Preset CurrentPreset { get; private set; }
        private int particlesToEmitTarget;
        private int particlesToEmitCurrent;
        private int framesLeft;
        private Sprite sprite;
        private Texture2D texture;
        private ParticleEffect particleEffect;
        public bool HasAnyParticles { get { return this.ActiveParticlesCount > 0; } }

        public int ActiveParticlesCount
        { get { return this.particleEffect.Emitters.Select(x => x.ActiveParticles).Sum(); } }

        public ParticleEngine(Preset preset, Sprite sprite)
        {
            this.sprite = sprite;
            this.particlesToEmitCurrent = 0;
            this.framesLeft = 0;

            this.ApplyPreset(preset);
        }

        public void ApplyPreset(Preset preset)
        {
            this.CurrentPreset = preset;

            string textureName;

            switch (this.CurrentPreset)
            {
                case Preset.Fireplace:
                    textureName = "circle_16x16";
                    break;

                case Preset.WaterSplash:
                    textureName = "circle_16x16";
                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.CurrentPreset}'.");
            }

            this.texture = TextureBank.GetTexture(textureName);
            this.particleEffect = new ParticleEffect(autoTrigger: false);

            TextureRegion2D textureRegion = new TextureRegion2D(this.texture);

            switch (this.CurrentPreset)
            {
                case Preset.Fireplace:

                    this.particlesToEmitTarget = 1;

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

                case Preset.WaterSplash:

                    this.particlesToEmitTarget = 1;

                    this.particleEffect.Emitters = new List<ParticleEmitter>
                    {
                        new ParticleEmitter(textureRegion, 150, TimeSpan.FromSeconds(2), Profile.Circle(radius: 15, radiate: Profile.CircleRadiation.Out))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Scale = 0.3f,
                                Color = HslColor.FromRgb(Color.Cyan),
                                Speed = new Range<float>(5f, 20f),
                                Quantity = 0,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.75f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = Vector2.UnitY, Strength = 15f},
                            }
                        }
                    };

                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.CurrentPreset}'.");
            }

            if (presetsToTurnOn.Contains(this.CurrentPreset)) this.TurnOn();
        }

        public void TurnOn(int particlesToEmit = 0, int duration = 0)
        {
            if (duration > 0) this.framesLeft = duration + 1;
            this.particlesToEmitCurrent = particlesToEmit == 0 ? this.particlesToEmitTarget : particlesToEmit;

            foreach (ParticleEmitter particleEmitter in this.particleEffect.Emitters)
            {
                particleEmitter.Parameters.Quantity = this.particlesToEmitCurrent;
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
                this.CurrentPreset == particleEngine.CurrentPreset &&
                this.particlesToEmitCurrent == particleEngine.particlesToEmitCurrent;
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> particleData = new Dictionary<string, object>
            {
                { "preset", this.CurrentPreset },
                { "particlesToEmitCurrent", this.particlesToEmitCurrent },
            };

            return particleData;
        }

        public static ParticleEngine Deserialize(Object particleData, Sprite sprite)
        {
            if (sprite.particleEngine != null) sprite.particleEngine.Dispose();

            var particleDict = (Dictionary<string, Object>)particleData;

            Preset preset = (Preset)(Int64)particleDict["preset"];
            int particlesToEmitCurrent = (int)(Int64)particleDict["particlesToEmitCurrent"];

            ParticleEngine particleEngine = new ParticleEngine(preset: preset, sprite: sprite);
            if (particlesToEmitCurrent > 0) particleEngine.TurnOn(particlesToEmitCurrent);
            return particleEngine;
        }

        public void Update()
        {
            if (this.framesLeft > 0) this.framesLeft--;
            if (this.framesLeft == 0) this.TurnOff();

            switch (this.CurrentPreset)
            {
                case Preset.Fireplace:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.WaterSplash:
                    this.particleEffect.Position = this.sprite.position;
                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{this.CurrentPreset}'.");
            }

            this.particleEffect.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw()
        {
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffect);
        }
    }
}