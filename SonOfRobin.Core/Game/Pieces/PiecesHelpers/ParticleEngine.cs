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
        public enum Preset
        {
            Fireplace = 0,
            BurnFlame = 1,
            Cooking = 2,
            Brewing = 3,
            CookingFinish = 4,
            BrewingFinish = 5,
            BloodDripping = 6,
            MeatDrying = 7,

            WaterWalk = 8,
            MudWalk = 9,
            WaterWave = 10,
            LavaFlame = 11,

            SwampGas = 12,
            Lightning = 13,
            Excavated = 14,
            DustPuff = 15,
            SmokePuff = 16,

            DebrisWood = 17,
            DebrisLeaf = 18,
            DebrisGrass = 19,
            DebrisStone = 20,
            DebrisCrystal = 21,
            DebrisCeramic = 22,
            DebrisBlood = 23,
            DebrisStar = 24,
            DebrisHeart = 25,
            DebrisSoot = 26,

            WeatherRain = 27,

            WindLeaf = 28,
            WindPetal = 29,

            HeatSmall = 31,
            HeatMedium = 30,
            HeatBig = 32,
        }

        private static readonly Dictionary<Preset, TextureBank.TextureName> textureNameDict = new Dictionary<Preset, TextureBank.TextureName> {
                { Preset.Fireplace, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.BurnFlame, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.WeatherRain, TextureBank.TextureName.ParticleWeatherRain },
                { Preset.Cooking, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.Brewing, TextureBank.TextureName.ParticleBubble },
                { Preset.WaterWalk, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.WaterWave, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.CookingFinish, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.BrewingFinish, TextureBank.TextureName.ParticleBubble },
                { Preset.Excavated, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.DustPuff, TextureBank.TextureName.ParticleDustPuff },
                { Preset.SmokePuff, TextureBank.TextureName.ParticleSmokePuff },
                { Preset.MudWalk, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.LavaFlame, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.DebrisWood, TextureBank.TextureName.ParticleDebrisWood },
                { Preset.DebrisLeaf, TextureBank.TextureName.ParticleDebrisLeaf },
                { Preset.WindLeaf, TextureBank.TextureName.ParticleDebrisLeaf },
                { Preset.WindPetal, TextureBank.TextureName.ParticleDebrisPetal },
                { Preset.DebrisGrass, TextureBank.TextureName.ParticleDebrisGrass },
                { Preset.DebrisStone, TextureBank.TextureName.ParticleDebrisStone },
                { Preset.DebrisCrystal, TextureBank.TextureName.ParticleDebrisCrystal },
                { Preset.DebrisCeramic, TextureBank.TextureName.ParticleDebrisCeramic },
                { Preset.DebrisBlood, TextureBank.TextureName.ParticleDebrisBlood },
                { Preset.DebrisStar, TextureBank.TextureName.ParticleDebrisStar },
                { Preset.DebrisHeart, TextureBank.TextureName.ParticleDebrisHeart },
                { Preset.DebrisSoot, TextureBank.TextureName.ParticleDebrisSoot },
                { Preset.SwampGas, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.Lightning, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.BloodDripping, TextureBank.TextureName.ParticleCircleSharp },
                { Preset.MeatDrying, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.HeatSmall, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.HeatMedium, TextureBank.TextureName.ParticleCircleSoft },
                { Preset.HeatBig, TextureBank.TextureName.ParticleCircleSoft },
            };

        public class PresetData
        {
            public readonly ParticleEmitter particleEmitter;
            public readonly int defaultParticlesToEmit;
            public readonly int particleToEmitMaxVariation;
            public readonly bool drawAsDistortion;

            private int currentParticlesToEmit;
            private int framesLeft;
            public int delayFramesLeft { get; private set; }
            public bool IsActive { get; private set; }

            public bool Serialize
            { get { return this.IsActive && this.framesLeft == 0; } }

            public bool HasFinished
            { get { return !this.IsActive && this.particleEmitter.ActiveParticles == 0; } }

            public PresetData(int defaultParticlesToEmit, ParticleEmitter particleEmitter, int particlesToEmitMaxVariation = 0, int maxDelay = 0, bool drawAsDistortion = false)
            {
                if (particlesToEmitMaxVariation < 0) throw new ArgumentOutOfRangeException($"particleToEmitMaxVariation cannot be < 0 - {particlesToEmitMaxVariation}");

                this.defaultParticlesToEmit = defaultParticlesToEmit;
                this.particleToEmitMaxVariation = particlesToEmitMaxVariation;
                this.currentParticlesToEmit = 0;
                this.drawAsDistortion = drawAsDistortion;

                this.particleEmitter = particleEmitter;
                this.delayFramesLeft = maxDelay > 0 ? SonOfRobinGame.random.Next(maxDelay + 1) : 0;
                this.framesLeft = 0;
                this.IsActive = false;
            }

            public void TurnOn(int particlesToEmit = 0, int duration = 0)
            {
                if (duration > 0) this.framesLeft = duration + 1;

                this.currentParticlesToEmit = particlesToEmit == 0 ? this.defaultParticlesToEmit : particlesToEmit;
                this.IsActive = true;
                this.Update(moveCounters: false);
            }

            public void Update(bool moveCounters = true)
            {
                if (moveCounters && this.delayFramesLeft > 0)
                {
                    this.delayFramesLeft--;
                    if (this.delayFramesLeft > 0) return;
                }

                if (this.delayFramesLeft == 0)
                {
                    int particlesToEmit = this.currentParticlesToEmit;

                    if (SonOfRobinGame.fps.FPS >= 40)
                    {
                        int variation = this.particleToEmitMaxVariation > 0 ? SonOfRobinGame.random.Next(this.particleToEmitMaxVariation + 1) : 0;
                        particlesToEmit += variation;
                    }
                    else particlesToEmit = Math.Min(particlesToEmit, 1);

                    this.particleEmitter.Parameters.Quantity = particlesToEmit;
                }

                if (moveCounters && this.framesLeft > 0)
                {
                    this.framesLeft--;
                    if (this.framesLeft == 0) this.TurnOff();
                }
            }

            public void TurnOff()
            {
                this.framesLeft = 0;
                this.delayFramesLeft = 0;
                this.particleEmitter.Parameters.Quantity = 0;
                this.IsActive = false;
            }
        }

        public static readonly Preset[] allPresets = (Preset[])Enum.GetValues(typeof(Preset));

        private Sprite sprite;
        private readonly ParticleEffect particleEffectDraw;
        private readonly ParticleEffect particleEffectDistortion;
        private readonly Dictionary<Preset, PresetData> dataByPreset;

        public bool HasAnyParticles
        { get { return this.ActiveParticlesCountDraw > 0 || this.ActiveParticlesCountDistortion > 0; } }

        public int ActiveParticlesCountDraw
        { get { return this.particleEffectDraw.Emitters.Select(x => x.ActiveParticles).Sum(); } }

        public int ActiveParticlesCountDistortion
        { get { return this.particleEffectDistortion.Emitters.Select(x => x.ActiveParticles).Sum(); } }

        private ParticleEngine(Sprite sprite)
        {
            this.sprite = sprite;
            this.dataByPreset = new Dictionary<Preset, PresetData>();
            this.particleEffectDraw = new ParticleEffect(autoTrigger: false);
            this.particleEffectDraw.Emitters = new List<ParticleEmitter>();

            this.particleEffectDistortion = new ParticleEffect(autoTrigger: false);
            this.particleEffectDistortion.Emitters = new List<ParticleEmitter>();
        }

        public void ReassignSprite(Sprite newSprite)
        {
            if (this.sprite != null) this.sprite.particleEngine = null;
            this.sprite = newSprite;
            newSprite.particleEngine = this;
        }

        public bool HasPreset(Preset preset)
        {
            return this.dataByPreset.ContainsKey(preset);
        }

        public void AddPreset(Preset preset)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(TextureBank.GetTexture(textureNameDict[preset]));

            int defaultParticlesToEmit;
            int particlesToEmitMaxVariation = 0;
            int maxDelay = 0;
            bool drawAsDistortion = false;
            ParticleEmitter particleEmitter;

            switch (preset)
            {
                case Preset.Fireplace:
                    {
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
                    }

                case Preset.HeatSmall:
                    {
                        defaultParticlesToEmit = 1;
                        drawAsDistortion = true;

                        particleEmitter = new ParticleEmitter(textureRegion, 200, TimeSpan.FromSeconds(1.8),
                            Profile.BoxFill(width: this.sprite.GfxRect.Width, height: this.sprite.GfxRect.Height))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(new Color(5, 5, 5)),
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
                                            StartValue = new Vector2(0.3f),
                                            EndValue = new Vector2(3.0f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.2f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 26f},
                            }
                        };
                        break;
                    }

                case Preset.HeatMedium:
                    {
                        defaultParticlesToEmit = 1;
                        drawAsDistortion = true;

                        particleEmitter = new ParticleEmitter(textureRegion, 200, TimeSpan.FromSeconds(1.8), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(new Color(14, 14, 14)),
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
                                            StartValue = new Vector2(0.3f),
                                            EndValue = new Vector2(3.3f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 20f},
                            }
                        };
                        break;
                    }

                case Preset.HeatBig:
                    {
                        defaultParticlesToEmit = 1;
                        drawAsDistortion = true;

                        particleEmitter = new ParticleEmitter(textureRegion, 300, TimeSpan.FromSeconds(3.0),
                            Profile.BoxFill(width: this.sprite.GfxRect.Width, height: this.sprite.GfxRect.Height))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(new Color(5, 5, 5)),
                                Speed = new Range<float>(10f, 35f),
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
                                            StartValue = new Vector2(0.5f),
                                            EndValue = new Vector2(8.0f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.17f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier { Direction = -Vector2.UnitY, Strength = 85f },
                            }
                        };
                        break;
                    }

                case Preset.BurnFlame:
                    {
                        float shortEdge = Math.Min(this.sprite.GfxRect.Width, this.sprite.GfxRect.Height);
                        float longEdge = Math.Max(this.sprite.GfxRect.Width, this.sprite.GfxRect.Height);

                        bool longRectangle = longEdge / shortEdge > 1.6f;
                        float sizeMultiplier = longRectangle ? 2f : 1f;

                        defaultParticlesToEmit = longRectangle ? 2 : 1;

                        Profile profile = longRectangle ?
                            Profile.BoxFill(width: this.sprite.GfxRect.Width, height: this.sprite.GfxRect.Height) :
                            Profile.Circle(radius: this.sprite.GfxRect.Width / 3, radiate: Profile.CircleRadiation.Out);

                        particleEmitter = new ParticleEmitter(textureRegion, 300, TimeSpan.FromSeconds(1.0), profile: profile)
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
                                            StartValue = new Vector2(0.4f * sizeMultiplier),
                                            EndValue = new Vector2(1.5f * sizeMultiplier)
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
                    }

                case Preset.Cooking:
                    {
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
                    }

                case Preset.Brewing:
                    {
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
                    }

                case Preset.WaterWalk:
                    {
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
                    }

                case Preset.WaterWave:
                    {
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
                                new DragModifier { Density = 0.7f, DragCoefficient = 1f },
                            }
                        };
                        break;
                    }

                case Preset.CookingFinish:
                    {
                        defaultParticlesToEmit = 8;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.0f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(Color.White),
                                Speed = new Range<float>(10f, 120f),
                                Quantity = 1,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(2.0f),
                                            EndValue = new Vector2(0.5f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.35f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 1f, DragCoefficient = 1f },
                            }
                        };
                        break;
                    }

                case Preset.BrewingFinish:
                    {
                        defaultParticlesToEmit = 10;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(0.8f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(Color.Red),
                                Speed = new Range<float>(40f, 180f),
                                Quantity = 1,
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
                                            StartValue = 1.0f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 1.2f, DragCoefficient = 1.2f },
                            }
                        };
                        break;
                    }

                case Preset.Excavated:
                    {
                        defaultParticlesToEmit = 10;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.0f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(new Color(128, 106, 50)),
                                Speed = new Range<float>(15f, 140f),
                                Quantity = 1,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(2.0f),
                                            EndValue = new Vector2(0.8f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.2f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 1f, DragCoefficient = 1f },
                            }
                        };
                        break;
                    }

                case Preset.DustPuff:
                    {
                        defaultParticlesToEmit = 1;

                        particleEmitter = new ParticleEmitter(
                            textureRegion, 5, TimeSpan.FromSeconds(0.7f),
                            profile: Profile.BoxFill(width: this.sprite.GfxRect.Width * 0.7f, height: this.sprite.GfxRect.Height * 0.7f))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(12f, 23f),
                                Quantity = 1,
                                Rotation = new Range<float>(-2f, 2f),
                            },

                            Modifiers =
                            {
                                new RotationModifier { RotationRate = (float)(SonOfRobinGame.random.NextDouble() * 2f) - 1f },
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.5f),
                                            EndValue = new Vector2(1.0f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1.2f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 1f, DragCoefficient = 1f },
                            }
                        };
                        break;
                    }

                case Preset.SmokePuff:
                    {
                        defaultParticlesToEmit = 1;

                        particleEmitter = new ParticleEmitter(
                            textureRegion, 5, TimeSpan.FromSeconds(2.0f),
                            profile: Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(12f, 18f),
                                Quantity = 1,
                                Rotation = new Range<float>(-2f, 2f),
                            },

                            Modifiers =
                            {
                                new RotationModifier { RotationRate = (float)(SonOfRobinGame.random.NextDouble() * 2.0f) - 1f },
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.7f),
                                            EndValue = new Vector2(1.4f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 2.0f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 1f, DragCoefficient = 1f },
                            }
                        };
                        break;
                    }

                case Preset.MudWalk:
                    {
                        defaultParticlesToEmit = 3;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.0f), Profile.Circle(radius: 11, radiate: Profile.CircleRadiation.Out))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(new Color(29, 51, 4)),
                                Speed = new Range<float>(10f, 45f),
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
                                            StartValue = new Vector2(0.55f),
                                            EndValue = new Vector2(2.4f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1.0f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = Vector2.UnitY, Strength = 8f},
                                new DragModifier { Density = 2.2f, DragCoefficient = 1.6f },
                            }
                        };
                        break;
                    }

                case Preset.LavaFlame:
                    {
                        defaultParticlesToEmit = 1;
                        maxDelay = 130;

                        particleEmitter = new ParticleEmitter(textureRegion, 60, TimeSpan.FromSeconds(3.5), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(Color.Yellow),
                                Speed = new Range<float>(2f, 7f),
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
                                            StartValue = new Vector2(0.22f),
                                            EndValue = new Vector2(3.2f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.43f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 7f},
                            }
                        };
                        break;
                    }

                case Preset.DebrisWood:
                    {
                        defaultParticlesToEmit = 10;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(100f, 900f),
                                Scale = new Range<float>(0.2f, 0.5f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.4f),
                            },

                            Modifiers =
                        {
                            new RotationModifier { RotationRate = 3.0f },
                            new VelocityModifier()
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 0f, EndValue = 1f }
                                },
                                VelocityThreshold = 2f
                            },
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                          }
                        };
                        break;
                    }

                case Preset.DebrisLeaf:
                    {
                        defaultParticlesToEmit = 4;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(100f, 1000f),
                                Scale = new Range<float>(0.3f, 0.6f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.3f),
                            },

                            Modifiers =
                        {
                            new RotationModifier
                            { RotationRate = -3.0f },
                            new VelocityModifier()
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 0f, EndValue = 1f }
                                },
                                VelocityThreshold = 2f
                            },
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                          }
                        };
                        break;
                    }

                case Preset.WindLeaf:
                    {
                        defaultParticlesToEmit = 4;

                        particleEmitter = new ParticleEmitter(textureRegion, 200, TimeSpan.FromSeconds(3.0f),
                            profile: Profile.Point()) // to be replaced with Spray()
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(40f, 800f),
                                Scale = new Range<float>(0.25f, 0.6f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(0.1f, 4.8f),
                            },

                            Modifiers =
                            {
                                new RotationModifier
                                { RotationRate = (float)(SonOfRobinGame.random.NextDouble() * 8f) - 4f },
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new OpacityInterpolator
                                        { StartValue = 2.0f, EndValue = 0f },
                                    },
                                },
                                new DragModifier { Density = 1.0f, DragCoefficient = 1 },
                            }
                        };
                        break;
                    }

                case Preset.WindPetal:
                    {
                        defaultParticlesToEmit = 1;

                        particleEmitter = new ParticleEmitter(textureRegion, 50, TimeSpan.FromSeconds(2.5f),
                            profile: Profile.Point()) // to be replaced with Spray()
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(40f, 800f),
                                Scale = new Range<float>(0.0625f, 0.14f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(0.1f, 4.8f),
                            },

                            Modifiers =
                            {
                                new RotationModifier
                                { RotationRate = (float)(SonOfRobinGame.random.NextDouble() * 8f) - 4f },
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new OpacityInterpolator
                                        { StartValue = 2.0f, EndValue = 0f },
                                    },
                                },
                                new DragModifier { Density = 1.0f, DragCoefficient = 1 },
                            }
                        };
                        break;
                    }

                case Preset.DebrisGrass:
                    {
                        defaultParticlesToEmit = 7;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(100f, 750f),
                                Scale = new Range<float>(0.15f, 0.4f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2f),
                            },

                            Modifiers =
                        {
                            new RotationModifier
                            { RotationRate = 0.8f },
                            new VelocityModifier()
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 0f, EndValue = 1f }
                                },
                                VelocityThreshold = 5f
                            },
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                          }
                        };
                        break;
                    }

                case Preset.DebrisStone:
                    {
                        defaultParticlesToEmit = 16;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 1000f),
                                Scale = new Range<float>(0.3f, 0.6f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.5f),
                            },

                            Modifiers =
                        {
                            new RotationModifier
                            { RotationRate = 3.0f },
                            new VelocityModifier()
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 0f, EndValue = 1f }
                                },
                                VelocityThreshold = 2f
                            },
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                          }
                        };
                        break;
                    }

                case Preset.DebrisCrystal:
                    {
                        defaultParticlesToEmit = 6;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 1000f),
                                Scale = new Range<float>(0.4f, 0.7f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2f),
                            },

                            Modifiers =
                        {
                            new RotationModifier
                            { RotationRate = 1.0f },
                            new VelocityModifier()
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 0f, EndValue = 1f }
                                },
                                VelocityThreshold = 2f
                            },
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                          }
                        };
                        break;
                    }

                case Preset.DebrisCeramic:
                    {
                        defaultParticlesToEmit = 6;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(2.0f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 900f),
                                Scale = new Range<float>(0.3f, 1.4f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.8f),
                            },

                            Modifiers =
                        {
                            new RotationModifier
                            { RotationRate = 1.0f },
                            new VelocityModifier()
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 0f, EndValue = 1f }
                                },
                                VelocityThreshold = 2f
                            },
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                          }
                        };
                        break;
                    }

                case Preset.DebrisBlood:
                    {
                        defaultParticlesToEmit = 12;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(10.0f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 900f),
                                Scale = new Range<float>(0.3f, 1.0f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.8f),
                            },

                            Modifiers =
                        {
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },

                            new AgeModifier
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 4.0f, EndValue = 0f },
                                }
                            },
                          }
                        };
                        break;
                    }

                case Preset.DebrisStar:
                    {
                        defaultParticlesToEmit = 8;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(2f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 950f),
                                Scale = new Range<float>(0.1f, 0.6f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.2f),
                            },

                            Modifiers =
                        {
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                            new AgeModifier
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 4.0f, EndValue = 0f },
                                },
                            },
                          }
                        };
                        break;
                    }

                case Preset.DebrisHeart:
                    {
                        defaultParticlesToEmit = 3;

                        particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(2f), Profile.Point())
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 700f),
                                Scale = new Range<float>(0.6f, 1.0f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1f, 2.2f),
                            },

                            Modifiers =
                        {
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                            new AgeModifier
                            {
                                Interpolators =
                                {
                                    new OpacityInterpolator
                                    { StartValue = 4.0f, EndValue = 0f },
                                },
                            },
                          }
                        };
                        break;
                    }

                case Preset.DebrisSoot:
                    {
                        int gfxArea = this.sprite.GfxRect.Width * this.sprite.GfxRect.Height;
                        defaultParticlesToEmit = gfxArea / 15;

                        particleEmitter = new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(1.8f),
                            profile: Profile.BoxFill(width: this.sprite.GfxRect.Width, height: this.sprite.GfxRect.Height))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(120f, 850f),
                                Rotation = new Range<float>(-2f, 2f),
                                Mass = new Range<float>(1.5f, 2.6f),
                            },

                            Modifiers =
                            {
                            new DragModifier { Density = 0.2f, DragCoefficient = 40f },
                             new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(this.sprite.BlocksMovement ? 0.9f : 0.6f),
                                            EndValue = new Vector2(0.05f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.7f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                          }
                        };
                        break;
                    }

                case Preset.SwampGas:
                    {
                        defaultParticlesToEmit = 1;
                        maxDelay = 30;
                        particlesToEmitMaxVariation = 4;

                        particleEmitter = new ParticleEmitter(textureRegion, SonOfRobinGame.random.Next(30, 200), TimeSpan.FromSeconds(4.5f), Profile.Spray(new Vector2(0, -1.5f), 1.0f))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(new Color(71, 49, 9)),
                                Speed = new Range<float>(20f, 55f),
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.6f),
                                            EndValue = new Vector2(3.4f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.25f,
                                            EndValue = 0.0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 1f, DragCoefficient = 0.13f },
                                new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 15f }
                            }
                        };
                        break;
                    }

                case Preset.Lightning:
                    {
                        int viewRectHeight = this.sprite.world.camera.viewRect.Height;

                        defaultParticlesToEmit = viewRectHeight * 3; // partially outside viewRect, to allow scrolling
                        float lineVectorX = (SonOfRobinGame.random.NextSingle() * 0.2f) - 0.1f;

                        particleEmitter = new ParticleEmitter(textureRegion, defaultParticlesToEmit, TimeSpan.FromSeconds(1.5f), Profile.Line(axis: new Vector2(lineVectorX, 1), length: viewRectHeight))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(Color.White),
                                Speed = new Range<float>(2f, 4f),
                                Scale = 0.17f,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1.0f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                            },
                        };
                        break;
                    }

                case Preset.BloodDripping:
                    {
                        defaultParticlesToEmit = 1;

                        particleEmitter = new ParticleEmitter(textureRegion, 100, TimeSpan.FromSeconds(5.5f),
                            Profile.BoxFill(width: (int)(this.sprite.GfxRect.Width * 0.8f), height: this.sprite.GfxRect.Height / 2))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(Color.DarkRed),
                                Speed = 0,
                                Quantity = 1,
                            },

                            Modifiers =
                            {
                                new AgeModifier
                                {
                                    Interpolators =
                                    {
                                        new ScaleInterpolator
                                        {
                                            StartValue = new Vector2(0.0f),
                                            EndValue = new Vector2(0.7f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 1.0f,
                                            EndValue = 0.0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 2.4f, DragCoefficient = 2.4f },
                                new LinearGravityModifier {Direction = Vector2.UnitY, Strength = 35f},
                            }
                        };
                        break;
                    }

                case Preset.MeatDrying:
                    {
                        defaultParticlesToEmit = 1;

                        particleEmitter = new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(3.5),
                            Profile.BoxFill(width: (int)(this.sprite.GfxRect.Width * 0.7f), height: this.sprite.GfxRect.Height / 4))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Color = HslColor.FromRgb(Color.White),
                                Speed = 0,
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
                                            StartValue = new Vector2(0.00f),
                                            EndValue = new Vector2(3.5f)
                                        },
                                        new OpacityInterpolator
                                        {
                                            StartValue = 0.7f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                                new DragModifier { Density = 2.4f, DragCoefficient = 2.4f },
                                new LinearGravityModifier { Direction = -Vector2.UnitY, Strength = 7f },
                            }
                        };
                        break;
                    }

                case Preset.WeatherRain:
                    {
                        defaultParticlesToEmit = 2;

                        Rectangle cameraRect = this.sprite.world.camera.viewRect;

                        particleEmitter = new ParticleEmitter(textureRegion, 600000,
                            TimeSpan.FromSeconds(1), // to be dynamically replaced
                            profile: Profile.BoxFill(width: cameraRect.Width * 2.2f, height: cameraRect.Height / 2))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(10f, 20f),
                                Quantity = 0,
                                Scale = 0.125f,
                                Rotation = 0f,
                                Opacity = 1.0f,
                            },
                        };
                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported preset - '{preset}'.");
            }

            if (drawAsDistortion) this.particleEffectDistortion.Emitters.Add(particleEmitter);
            else this.particleEffectDraw.Emitters.Add(particleEmitter);

            this.dataByPreset[preset] = new PresetData(defaultParticlesToEmit: defaultParticlesToEmit, particleEmitter: particleEmitter, particlesToEmitMaxVariation: particlesToEmitMaxVariation, maxDelay: maxDelay, drawAsDistortion: drawAsDistortion);
        }

        public static ParticleEmitter GetEmitterForPreset(Sprite sprite, Preset preset)
        {
            return sprite.particleEngine != null && sprite.particleEngine.dataByPreset.ContainsKey(preset) ?
                sprite.particleEngine.dataByPreset[preset].particleEmitter : null;
        }

        public static void TurnOn(Sprite sprite, Preset preset, int particlesToEmit = 0, int duration = 0, bool update = false)
        {
            if (duration > 0 && !sprite.IsInCameraRect) return;

            if (sprite.particleEngine == null) sprite.particleEngine = new ParticleEngine(sprite);
            if (!sprite.particleEngine.dataByPreset.ContainsKey(preset)) sprite.particleEngine.AddPreset(preset);

            sprite.particleEngine.dataByPreset[preset].TurnOn(particlesToEmit: particlesToEmit, duration: duration);
            if (update) sprite.particleEngine.Update();
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
            this.particleEffectDraw.Dispose();
            this.particleEffectDistortion.Dispose();
        }

        public Dictionary<string, Object> Serialize()
        {
            var activePresetsList = new List<Preset>();

            foreach (var kvp in this.dataByPreset)
            {
                Preset preset = kvp.Key;
                PresetData presetData = kvp.Value;

                if (presetData.Serialize) activePresetsList.Add(preset);
            }

            if (activePresetsList.Count == 0) return null;

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
            if (this.dataByPreset.Count == 0) return;

            foreach (var kvp in this.dataByPreset.ToList())
            {
                Preset presetName = kvp.Key;
                PresetData presetData = kvp.Value;

                if (presetData.IsActive) presetData.Update();
                if (presetData.HasFinished) this.dataByPreset.Remove(presetName);
            }

            if (this.dataByPreset.Count == 0) return;

            Preset preset = this.dataByPreset.First().Key;
            Vector2 position;

            switch (preset)
            {
                case Preset.Fireplace:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.Cooking:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Top);
                    break;

                case Preset.Brewing:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.WaterWalk:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Bottom);
                    break;

                case Preset.MudWalk:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Bottom);
                    break;

                case Preset.BurnFlame:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.DebrisSoot:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.DustPuff:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.SmokePuff:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.WindLeaf:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.WindPetal:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.BloodDripping:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y - (this.sprite.GfxRect.Height / 5));
                    break;

                case Preset.Lightning:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Top - (this.sprite.world.camera.viewRect.Height / 2));
                    break;

                case Preset.MeatDrying:
                    position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y - (this.sprite.GfxRect.Height / 6));
                    break;

                case Preset.WeatherRain:
                    position = new Vector2(this.sprite.position.X, this.sprite.position.Y - this.sprite.world.camera.viewRect.Height);
                    break;

                case Preset.HeatSmall:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.HeatMedium:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.HeatBig:
                    position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                default:
                    position = this.sprite.position;
                    break;
            }

            this.particleEffectDraw.Position = position;
            this.particleEffectDistortion.Position = position;

            this.particleEffectDraw.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            this.particleEffectDistortion.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw()
        {
            SonOfRobinGame.SpriteBatch.End(); // otherwise flicker will occur (interaction with drawing water caustics, real reason unknown)
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.sprite.world.TransformMatrix);
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffectDraw);
        }

        public void DrawDistortion()
        {
            SonOfRobinGame.SpriteBatch.Draw(this.particleEffectDistortion);
        }
    }
}