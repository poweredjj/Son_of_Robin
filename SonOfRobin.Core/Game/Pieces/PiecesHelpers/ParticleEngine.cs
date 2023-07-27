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
        public enum Preset { Fireplace, BurnFlame, Cooking, Brewing, WaterWalk, WaterWave, CookingFinish, BrewingFinish, Excavated, MudWalk, LavaFlame, DebrisWood, DebrisLeaf, DebrisGrass, DebrisStone, DebrisCrystal, DebrisCeramic, DebrisBlood, DebrisStar, DebrisHeart, DebrisSoot }

        public class PresetData
        {
            private readonly ParticleEmitter particleEmitter;
            public readonly int defaultParticlesToEmit;
            public readonly int particleToEmitMaxVariation;
            private int currentParticlesToEmit;
            private int framesLeft;
            public int delayFramesLeft { get; private set; }
            public bool IsActive { get; private set; }
            public bool Serialize { get { return this.IsActive && this.framesLeft == 0; } }
            public bool HasFinished { get { return !this.IsActive && this.particleEmitter.ActiveParticles == 0; } }

            public PresetData(int defaultParticlesToEmit, ParticleEmitter particleEmitter, int particlesToEmitMaxVariation = 0, int maxDelay = 0)
            {
                if (particlesToEmitMaxVariation < 0) throw new ArgumentOutOfRangeException($"particleToEmitMaxVariation cannot be < 0 - {particlesToEmitMaxVariation}");

                this.defaultParticlesToEmit = defaultParticlesToEmit;
                this.particleToEmitMaxVariation = particlesToEmitMaxVariation;
                this.currentParticlesToEmit = 0;

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
        private readonly ParticleEffect particleEffect;
        private readonly Dictionary<Preset, PresetData> dataByPreset;

        public bool HasAnyParticles { get { return this.ActiveParticlesCount > 0; } }

        public int ActiveParticlesCount
        { get { return this.particleEffect.Emitters.Select(x => x.ActiveParticles).Sum(); } }

        private ParticleEngine(Sprite sprite)
        {
            this.sprite = sprite;
            this.dataByPreset = new Dictionary<Preset, PresetData>();
            this.particleEffect = new ParticleEffect(autoTrigger: false);
            this.particleEffect.Emitters = new List<ParticleEmitter>();
        }

        public void ReassignSprite(Sprite newSprite)
        {
            if (this.sprite != null) this.sprite.particleEngine = null;
            this.sprite = newSprite;
            newSprite.particleEngine = this;
        }

        public void AddPreset(Preset preset)
        {
            var textureNameDict = new Dictionary<Preset, string> {
                { Preset.Fireplace, "circle_16x16_sharp" },
                { Preset.BurnFlame, "circle_16x16_soft" },
                { Preset.Cooking, "circle_16x16_sharp" },
                { Preset.Brewing, "bubble_16x16" },
                { Preset.WaterWalk, "circle_16x16_sharp" },
                { Preset.WaterWave, "circle_16x16_soft" },
                { Preset.CookingFinish, "circle_16x16_soft" },
                { Preset.BrewingFinish, "bubble_16x16" },
                { Preset.Excavated, "circle_16x16_sharp" },
                { Preset.MudWalk, "circle_16x16_soft" },
                { Preset.LavaFlame, "circle_16x16_sharp" },
                { Preset.DebrisWood, "debris_wood" },
                { Preset.DebrisLeaf, "debris_leaf" },
                { Preset.DebrisGrass, "debris_grass" },
                { Preset.DebrisStone, "debris_stone" },
                { Preset.DebrisCrystal, "debris_crystal" },
                { Preset.DebrisCeramic, "debris_ceramic" },
                { Preset.DebrisBlood, "debris_blood" },
                { Preset.DebrisStar, "debris_star" },
                { Preset.DebrisHeart, "debris_heart" },
                { Preset.DebrisSoot, "debris_soot" },
            };

            TextureRegion2D textureRegion = new TextureRegion2D(TextureBank.GetTexture($"particles/{textureNameDict[preset]}"));

            int defaultParticlesToEmit;
            int particlesToEmitMaxVariation = 0;
            int maxDelay = 0;
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

                case Preset.CookingFinish:
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
                                new DragModifier
                                {
                                    Density = 1f, DragCoefficient = 1f
                                },
                            }
                    };
                    break;

                case Preset.BrewingFinish:
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
                                new DragModifier
                                {
                                    Density = 1.2f, DragCoefficient = 1.2f
                                },
                            }
                    };
                    break;

                case Preset.Excavated:
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
                                new DragModifier
                                {
                                    Density = 1f, DragCoefficient = 1f
                                },
                            }
                    };
                    break;

                case Preset.MudWalk:
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

                case Preset.LavaFlame:
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

                case Preset.DebrisWood:
                    defaultParticlesToEmit = 10;

                    particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(120f, 1200f),
                            Scale = new Range<float>(0.2f, 0.5f),
                            Rotation = new Range<float>(-2f, 2f),
                            Mass = new Range<float>(1f, 2f),
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
                          }
                    };
                    break;

                case Preset.DebrisLeaf:
                    defaultParticlesToEmit = 4;

                    particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5f), Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(120f, 1200f),
                            Scale = new Range<float>(0.3f, 0.6f),
                            Rotation = new Range<float>(-2f, 2f),
                            Mass = new Range<float>(1f, 2f),
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
                          }
                    };
                    break;

                case Preset.DebrisGrass:
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
                          }
                    };
                    break;

                case Preset.DebrisStone:
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
                          }
                    };
                    break;

                case Preset.DebrisCrystal:
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
                          }
                    };
                    break;

                case Preset.DebrisCeramic:
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
                          }
                    };
                    break;

                case Preset.DebrisBlood:
                    defaultParticlesToEmit = 12;

                    particleEmitter = new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(4.0f), Profile.Point())
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },

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

                case Preset.DebrisStar:
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
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

                case Preset.DebrisHeart:
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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
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

                case Preset.DebrisSoot:
                    {
                        int gfxArea = this.sprite.GfxRect.Width * this.sprite.GfxRect.Height;
                        defaultParticlesToEmit = gfxArea / 10;

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
                            new DragModifier
                            { Density = 0.2f, DragCoefficient = 40f },
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
                                            StartValue = 0.9f,
                                            EndValue = 0f
                                        },
                                    }
                                },
                          }
                        };
                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported preset - '{preset}'.");
            }

            this.particleEffect.Emitters.Add(particleEmitter);
            this.dataByPreset[preset] = new PresetData(defaultParticlesToEmit: defaultParticlesToEmit, particleEmitter: particleEmitter, particlesToEmitMaxVariation: particlesToEmitMaxVariation, maxDelay: maxDelay);
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
            this.particleEffect.Dispose();
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

            foreach (var kvp in this.dataByPreset.ToList())
            {
                Preset presetName = kvp.Key;
                PresetData presetData = kvp.Value;

                if (presetData.IsActive) presetData.Update();
                if (presetData.HasFinished) this.dataByPreset.Remove(presetName);
            }

            if (!this.dataByPreset.Any()) return;

            Preset preset = this.dataByPreset.First().Key;
            switch (preset)
            {
                case Preset.Fireplace:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.GfxRect.Center.Y);
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

                case Preset.MudWalk:
                    this.particleEffect.Position = new Vector2(this.sprite.ColRect.Center.X, this.sprite.ColRect.Bottom);
                    break;

                case Preset.BurnFlame:
                    this.particleEffect.Position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                case Preset.DebrisSoot:
                    this.particleEffect.Position = new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Center.Y);
                    break;

                default:
                    this.particleEffect.Position = this.sprite.position;
                    break;
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