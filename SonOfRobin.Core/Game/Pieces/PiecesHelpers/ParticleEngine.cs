using Microsoft.Xna.Framework;
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
        public enum Preset { Fireplace, BurnFlame, Cooking, Brewing, WaterWalk, WaterWave }

        public class PresetData
        {
            public readonly ParticleEmitter particleEmitter;
            public readonly int defaultParticlesToEmit;
            public int framesLeft;
            public bool IsActive { get; private set; }

            public PresetData(int defaultParticlesToEmit, ParticleEmitter particleEmitter)
            {
                this.defaultParticlesToEmit = defaultParticlesToEmit;

                this.particleEmitter = particleEmitter;
                this.framesLeft = 0;
                this.IsActive = false;
            }

            public void TurnOn(int particlesToEmit = 0, int duration = 0)
            {
                if (duration > 0) this.framesLeft = duration + 1;
                this.particleEmitter.Parameters.Quantity = particlesToEmit == 0 ? this.defaultParticlesToEmit : particlesToEmit;
                this.IsActive = true;
            }

            public void TurnOff()
            {
                this.particleEmitter.Parameters.Quantity = 0;
                this.IsActive = false;
            }
        }

        public static readonly Preset[] allPresets = (Preset[])Enum.GetValues(typeof(Preset));

        private readonly Sprite sprite;
        private readonly ParticleEffect particleEffect;
        private readonly Dictionary<Preset, PresetData> dataByPreset;

        public bool HasAnyParticles { get { return this.ActiveParticlesCount > 0; } }

        public int ActiveParticlesCount
        { get { return this.particleEffect.Emitters.Select(x => x.ActiveParticles).Sum(); } }

        public ParticleEngine(Sprite sprite)
        {
            this.sprite = sprite;
            this.dataByPreset = new Dictionary<Preset, PresetData>();
            this.particleEffect = new ParticleEffect(autoTrigger: false);
            this.particleEffect.Emitters = new List<ParticleEmitter>();
        }

        public void AddPreset(Preset preset)
        {
            var textureNameDict = new Dictionary<Preset, string> {
                {Preset.Fireplace, "circle_16x16_sharp" },
                {Preset.BurnFlame, "circle_16x16_soft" },
                {Preset.Cooking, "circle_16x16_sharp" },
                {Preset.Brewing, "bubble_16x16" },
                {Preset.WaterWalk, "circle_16x16_sharp" },
                {Preset.WaterWave, "circle_16x16_soft" },
            };

            TextureRegion2D textureRegion = new TextureRegion2D(TextureBank.GetTexture(textureNameDict[preset]));

            int defaultParticlesToEmit;
            ParticleEmitter particleEmitter;

            switch (preset)
            {
                case Preset.Fireplace:
                    defaultParticlesToEmit = 1;

                    particleEmitter = new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(1.5), Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Color = HslColor.FromRgb(Color.Yellow),
                            Speed = new Range<float>(5f, 20f),
                            Quantity = 0,
                        },

                        Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
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
                                        new HueInterpolator { StartValue = 60f, EndValue = 20f }
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 45f},
                            }
                    };

                    break;

                case Preset.BurnFlame:
                    defaultParticlesToEmit = 2;

                    particleEmitter = new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(1.0), Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Color = HslColor.FromRgb(Color.Yellow),
                            Speed = new Range<float>(5f, 20f),
                            Quantity = 0,
                        },

                        Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.02f),
                                            EndValue = new Vector2(1.5f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.8f,
                                            EndValue = 0f
                                        },
                                        new HueInterpolator { StartValue = 60f, EndValue = 20f }
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 150f},
                            }
                    };

                    break;

                case Preset.Cooking:
                    defaultParticlesToEmit = 1;

                    particleEmitter = new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(1.5), Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Color = HslColor.FromRgb(Color.White),
                            Speed = new Range<float>(5f, 20f),
                            Quantity = 0,
                        },

                        Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.02f),
                                            EndValue = new Vector2(0.5f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.5f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 45f},
                            }
                    };

                    break;

                case Preset.Brewing:
                    defaultParticlesToEmit = 1;

                    particleEmitter = new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(1.5), Profile.BoxFill(width: this.sprite.ColRect.Width, height: this.sprite.ColRect.Height))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Color = HslColor.FromRgb(Color.Red),
                            Speed = new Range<float>(6f, 25f),
                            Quantity = 0,
                            Rotation = 0,
                        },

                        Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new HueInterpolator { StartValue = 0f, EndValue = 360f },
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.05f),
                                            EndValue = new Vector2(0.75f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1f,
                                            EndValue = 0.2f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 45f},
                            }
                    };

                    break;

                case Preset.WaterWalk:
                    defaultParticlesToEmit = 3;

                    particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.35f), Profile.Circle(radius: 15, radiate: Profile.CircleRadiation.Out))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Scale = new Range<float>(0.1f, 0.4f),
                            Color = HslColor.FromRgb(Color.Cyan),
                            Speed = new Range<float>(6f, 25f),
                            Quantity = 0,
                        },

                        Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.35f),
                                            EndValue = new Vector2(0.01f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.42f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = Vector2.UnitY, Strength = 15f},
                            }
                    };

                    break;

                case Preset.WaterWave:
                    defaultParticlesToEmit = 3;

                    float axisX = MathF.Cos(this.sprite.rotation - (float)(Math.PI / 2));
                    float axisY = MathF.Sin(this.sprite.rotation - (float)(Math.PI / 2));

                    particleEmitter = new ParticleEmitter(textureRegion, 700, TimeSpan.FromSeconds(1.1f), Profile.Line(axis: new Vector2(axisX, axisY), length: this.sprite.GfxRect.Height * 0.85f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Color = HslColor.FromRgb(Color.Cyan),
                            Speed = new Range<float>(10f, 45f),
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
                                            StartValue = 0.25f,
                                            EndValue = 0f
                                        },
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(1.0f),
                                            EndValue = new Vector2(3.1f)
                                        },
                                    }
                                },
                                new DragModifier
                                {
                                    Density = 0.7f, DragCoefficient = 1f
                                },
                            }
                    };

                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{preset}'.");
            }

            this.particleEffect.Emitters.Add(particleEmitter);
            this.dataByPreset[preset] = new PresetData(defaultParticlesToEmit: defaultParticlesToEmit, particleEmitter: particleEmitter);
        }

        public static void TurnOn(Sprite sprite, Preset preset, int particlesToEmit = 0, int duration = 0)
        {
            if (sprite.particleEngine == null) sprite.particleEngine = new ParticleEngine(sprite);
            if (!sprite.particleEngine.dataByPreset.ContainsKey(preset)) sprite.particleEngine.AddPreset(preset);

            sprite.particleEngine.dataByPreset[preset].TurnOn(particlesToEmit: particlesToEmit, duration: duration);
        }

        public static void TurnOff(Sprite sprite, Preset preset)
        {
            if (sprite.particleEngine == null || !sprite.particleEngine.dataByPreset.ContainsKey(preset)) return;

            sprite.particleEngine.dataByPreset[preset].TurnOff();
        }

        public static void TurnOffAll(Sprite sprite)
        {
            if (sprite.particleEngine == null) return;

            foreach (PresetData presetData in sprite.particleEngine.dataByPreset.Values)
            {
                presetData.TurnOff();
            }
        }

        public void Dispose()
        {
            this.particleEffect.Dispose();
        }

        public Dictionary<string, Object> Serialize()
        {
            var activePresetsList = new List<Preset>();

            foreach (var kvp in this.dataByPreset)
            {
                Preset preset = kvp.Key;
                PresetData presetData = kvp.Value;

                if (presetData.IsActive && presetData.framesLeft == 0)
                {
                    activePresetsList.Add(preset);
                }
            }

            if (!activePresetsList.Any()) return null;

            Dictionary<string, Object> particleData = new Dictionary<string, object>
            {
                { "activePresetsList", activePresetsList },
            };

            return particleData;
        }

        public static void Deserialize(Object particleData, Sprite sprite)
        {
            var particleDict = (Dictionary<string, Object>)particleData;

            var activePresetsList = (List<Preset>)particleDict["activePresetsList"];
            foreach (Preset preset in activePresetsList)
            {
                TurnOn(sprite: sprite, preset: preset);
            }
        }

        public void Update()
        {
            if (!this.dataByPreset.Any()) return;

            foreach (PresetData presetData in this.dataByPreset.Values)
            {
                if (presetData.framesLeft > 0)
                {
                    presetData.framesLeft--;
                    if (presetData.framesLeft == 0) presetData.TurnOff();
                }
            }

            Preset preset = this.dataByPreset.First().Key;

            switch (preset)
            {
                case Preset.Fireplace:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.BurnFlame:
                    this.particleEffect.Position = this.sprite.position;
                    break;

                case Preset.Cooking:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Top);
                    break;

                case Preset.Brewing:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.WaterWalk:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Bottom);
                    break;

                case Preset.WaterWave:
                    this.particleEffect.Position = this.sprite.position;
                    break;

                default:
                    throw new ArgumentException($"Unsupported preset - '{preset}'.");
            }

            this.particleEffect.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw()
        {
            SonOfRobinGame.SpriteBatch.End(); // otherwise flicker will occur (interaction with drawing water caustics, real reason unknown)
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.sprite.world.TransformMatrix);
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffect);
        }
    }
}