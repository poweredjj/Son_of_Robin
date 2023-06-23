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
        public class PresetData
        {
            public readonly ParticleEmitter particleEmitter;
            public readonly int particlesToEmitDefault;
            public int framesLeft;
            public bool IsActive { get; private set; }

            public PresetData(int particlesToEmitDefault, ParticleEmitter particleEmitter)
            {
                this.particlesToEmitDefault = particlesToEmitDefault;
                this.particleEmitter = particleEmitter;
                this.framesLeft = 0;
                this.IsActive = false;
            }

            public void TurnOn(int particlesToEmit = 0, int duration = 0)
            {
                if (duration > 0) this.framesLeft = duration + 1;
                this.particleEmitter.Parameters.Quantity = particlesToEmit == 0 ? this.particlesToEmitDefault : particlesToEmit;
                this.IsActive = true;
            }

            public void TurnOff()
            {
                this.particleEmitter.Parameters.Quantity = 0;
                this.IsActive = false;
            }
        }

        public enum Preset { Empty, Fireplace, WaterWalk }

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
                {Preset.Fireplace, "circle_16x16" },
                {Preset.WaterWalk, "circle_16x16" },
            };

            TextureRegion2D textureRegion = new TextureRegion2D(TextureBank.GetTexture(textureNameDict[preset]));

            int particlesToEmitDefault;
            ParticleEmitter particleEmitter;

            switch (preset)
            {
                case Preset.Fireplace:
                    particlesToEmitDefault = 1;

                    particleEmitter = new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(1.5), Profile.Point())
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
                    };

                    break;

                case Preset.WaterWalk:
                    particlesToEmitDefault = 3;
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

                default:
                    throw new ArgumentException($"Unsupported preset - '{preset}'.");
            }

            this.particleEffect.Emitters.Add(particleEmitter);
            this.dataByPreset[preset] = new PresetData(particlesToEmitDefault: particlesToEmitDefault, particleEmitter: particleEmitter);
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
            foreach (var kvp in this.dataByPreset)
            {
                Preset preset = kvp.Key;
                PresetData presetData = kvp.Value;

                if (presetData.framesLeft > 0)
                {
                    presetData.framesLeft--;
                    if (presetData.framesLeft == 0) presetData.TurnOff();
                }

                switch (preset)
                {
                    case Preset.Fireplace:
                        this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                        break;

                    case Preset.WaterWalk:
                        this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Bottom);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported preset - '{preset}'.");
                }
            }

            this.particleEffect.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw()
        {
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffect);
        }
    }
}