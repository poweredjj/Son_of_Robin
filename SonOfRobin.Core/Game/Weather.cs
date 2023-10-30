using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    [Serializable]
    public readonly struct WeatherEvent
    {
        public readonly Weather.WeatherType type;
        public readonly float intensity;
        public readonly DateTime startTime;
        public readonly DateTime endTime;
        public readonly TimeSpan duration;
        public readonly TimeSpan transitionLength;

        public WeatherEvent(Weather.WeatherType type, float intensity, DateTime startTime, TimeSpan duration, TimeSpan transitionLength)
        {
            if (transitionLength + transitionLength >= duration) throw new ArgumentException("Transitions are longer than duration.");

            this.type = type;
            if (intensity < 0 || intensity > 1) throw new ArgumentOutOfRangeException($"Intensity must be in 0-1 range: {intensity}.");
            this.intensity = intensity;
            this.startTime = startTime;
            if (duration == TimeSpan.Zero) throw new ArgumentOutOfRangeException($"Duration must be positive: {duration}.");
            this.duration = duration;
            this.endTime = startTime + duration;
            this.transitionLength = transitionLength;

            // SonOfRobinGame.messageLog.AddMessage(text: $"New WeatherEvent {this.type}, D {this.startTime:d HH\\:mm} - D {this.endTime:d HH\\:mm} ({this.duration:hh\\:mm}), transLength {transitionLength:hh\\:mm}, intensity {intensity}"); // for testing
        }

        public float GetIntensity(DateTime datetime)
        {
            if (datetime < this.startTime || datetime > this.endTime) return 0;

            TimeSpan elapsedTime = datetime - this.startTime;
            float transitionProgress;

            if (elapsedTime <= this.transitionLength)
            {
                // start transition
                transitionProgress = (float)Helpers.ConvertRange(oldMin: 0, oldMax: this.transitionLength.Ticks, newMin: 0, newMax: 1, oldVal: elapsedTime.Ticks, clampToEdges: true);
            }
            else if (elapsedTime > this.transitionLength && elapsedTime < this.duration - this.transitionLength)
            {
                // between transitions (full intensity)
                transitionProgress = 1f;
            }
            else
            {
                // end transition
                transitionProgress = 1f - (float)Helpers.ConvertRange(oldMin: this.duration.Ticks - this.transitionLength.Ticks, oldMax: this.duration.Ticks, newMin: 0, newMax: 1, oldVal: elapsedTime.Ticks, clampToEdges: true);
            }

            if (transitionProgress < 0 || transitionProgress > 1) throw new ArgumentOutOfRangeException($"Invalid transition progress {transitionProgress}.");

            return this.intensity * transitionProgress;
        }
    }

    public class Weather
    {
        public enum WeatherType : byte
        {
            Wind = 0,
            Clouds = 1,
            Rain = 2,
            Lightning = 3,
            Fog = 4,
        }

        public static readonly WeatherType[] allTypes = (WeatherType[])Enum.GetValues(typeof(WeatherType));
        private static readonly DateTime veryOldDate = new DateTime(1900, 1, 1);
        private static readonly TimeSpan minForecastDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan heatTransitionDuration = TimeSpan.FromMinutes(25);
        private static readonly TimeSpan maxForecastDuration = minForecastDuration + minForecastDuration;

        public static readonly List<SoundData.Name> thunderNames = Enum.GetValues(typeof(SoundData.Name))
            .Cast<SoundData.Name>().Where(x => x.ToString().StartsWith("Thunder")).ToList();

        public static readonly Sound thunderSound = new Sound(nameList: thunderNames, cooldown: 60, ignore3DAlways: true, maxPitchVariation: 0.5f);

        private readonly World world;
        private Rectangle lastRainRect;
        private readonly IslandClock islandClock;
        private DateTime forecastEnd;
        private readonly Dictionary<WeatherType, float> currentIntensityForType;
        private readonly Dictionary<WeatherType, float> previousIntensityForType;
        private bool firstForecastCreated;
        private readonly Sound windSound;
        private readonly Sound rainSound;
        private readonly BoardPiece rainEmitter; // every particle effect should have its own emitter, to allow free placement of each effect
        private readonly LinearGravityModifier rainWindModifier;
        private readonly LinearGravityModifier rainGravityModifier;
        private List<BoardPiece> fogPieces;
        private readonly List<WeatherEvent> weatherEvents;
        public float CloudsPercentage { get; private set; }
        public float FogPercentage { get; private set; }
        public float SunVisibility { get; private set; }
        public float WindPercentage { get; private set; }
        public float RainPercentage { get; private set; }
        public float LightningPercentage { get; private set; }
        public float HeatPercentage { get; private set; }

        public bool IsRaining
        { get { return this.RainPercentage > 0.2f; } }

        public float WindOriginX { get; private set; }
        public float WindOriginY { get; private set; }
        public DateTime NextGlobalWindBlow { get; private set; }
        public DateTime NextLocalizedWindBlow { get; private set; }
        public Vector2 LightningPosMultiplier { get; private set; } // to be multiplied by sunPos

        public Weather(World world, IslandClock islandClock)
        {
            this.world = world;
            this.lastRainRect = new Rectangle();
            this.islandClock = islandClock;
            this.rainEmitter = PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.ParticleEmitterWeather, world: this.world);
            this.rainWindModifier = new LinearGravityModifier();
            this.rainGravityModifier = new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 0f };
            this.fogPieces = new List<BoardPiece>();
            this.weatherEvents = new List<WeatherEvent>();
            this.forecastEnd = veryOldDate; // to ensure first update
            this.firstForecastCreated = false;

            this.CloudsPercentage = 0f;
            this.FogPercentage = 0f;
            this.SunVisibility = 1f;
            this.WindOriginX = 0f;
            this.WindOriginY = 0f;
            this.NextGlobalWindBlow = veryOldDate;
            this.NextLocalizedWindBlow = veryOldDate;
            this.WindPercentage = 0f;
            this.LightningPercentage = 0f;
            this.HeatPercentage = 0f;

            this.currentIntensityForType = new Dictionary<WeatherType, float>();
            this.previousIntensityForType = new Dictionary<WeatherType, float>();
            foreach (WeatherType type in allTypes)
            {
                this.currentIntensityForType[type] = 0;
                this.previousIntensityForType[type] = 0;
            }

            this.windSound = new Sound(name: SoundData.Name.WeatherWind, maxPitchVariation: 0.5f, volume: 0.2f, isLooped: true, ignore3DAlways: true);
            this.rainSound = new Sound(name: SoundData.Name.Rain, maxPitchVariation: 0.5f, volume: 0.2f, isLooped: true, ignore3DAlways: true);
        }

        public float GetIntensityForWeatherType(WeatherType type)
        {
            return this.currentIntensityForType[type];
        }

        public Dictionary<WeatherType, float> GetIntensityForAllWeatherTypes()
        {
            return this.currentIntensityForType;
        }

        public float GetIntensityForWeatherType(WeatherType type, DateTime dateTime)
        {
            if (dateTime > this.forecastEnd)
            {
                MessageLog.Add(debugMessage: true, text: $"Forecast exceeded by {(dateTime - this.forecastEnd).TotalHours} hours when trying to get weather for {type}.");
                return 0;
            }

            float maxIntensity = 0;
            foreach (WeatherEvent weatherEvent in this.weatherEvents)
            {
                if (weatherEvent.type == type) maxIntensity = Math.Max(maxIntensity, weatherEvent.GetIntensity(dateTime));
            }

            return maxIntensity;
        }

        public bool LightningJustStruck
        { get { return this.currentIntensityForType[WeatherType.Lightning] == 1 && this.previousIntensityForType[WeatherType.Lightning] < 1; } }

        public void Update()
        {
            if (this.world.ActiveLevel.levelType != Level.LevelType.Island)
            {
                this.CloudsPercentage = 0f;
                this.FogPercentage = 0f;
                this.SunVisibility = 0f;
                this.WindPercentage = 0f;
                this.RainPercentage = 0f;
                this.HeatPercentage = 0f;

                if (this.rainSound.IsPlaying) this.rainSound.Stop();
                if (this.windSound.IsPlaying) this.windSound.Stop();

                return;
            }

            DateTime islandDateTime = this.islandClock.IslandDateTime;
            weatherEvents.RemoveAll(e => e.endTime < islandDateTime);

            this.UpdateCurrentIntensities();

            this.CloudsPercentage = Math.Min(this.GetIntensityForWeatherType(WeatherType.Clouds) * 2, 1);
            this.FogPercentage = this.GetIntensityForWeatherType(WeatherType.Fog);
            this.SunVisibility = Math.Max(0, 1f - (this.CloudsPercentage + (this.FogPercentage * 0.5f)));
            this.WindPercentage = this.GetIntensityForWeatherType(WeatherType.Wind);
            this.RainPercentage = this.GetIntensityForWeatherType(WeatherType.Rain);

            this.HeatPercentage = 0;

            if (this.SunVisibility >= 0.8f && !this.IsRaining)
            {
                IslandClock.PartOfDay currentPartOfDay = this.world.islandClock.CurrentPartOfDay;

                if (currentPartOfDay == IslandClock.PartOfDay.Morning)
                {
                    TimeSpan timeUntilNoon = this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Noon);
                    if (timeUntilNoon <= heatTransitionDuration) this.HeatPercentage = 1f - ((float)timeUntilNoon.Ticks / (float)heatTransitionDuration.Ticks);
                }
                else if (currentPartOfDay == IslandClock.PartOfDay.Noon)
                {
                    this.HeatPercentage = 1f;
                    TimeSpan timeUntilAfternoon = this.world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Afternoon);
                    if (timeUntilAfternoon <= heatTransitionDuration) this.HeatPercentage = (float)timeUntilAfternoon.Ticks / (float)heatTransitionDuration.Ticks;
                }
            }

            this.LightningPercentage = this.GetIntensityForWeatherType(WeatherType.Lightning);
            if (this.previousIntensityForType[WeatherType.Lightning] < this.currentIntensityForType[WeatherType.Lightning])
            {
                // lightning in-transition should be ignored
                this.LightningPercentage = 0;
            }

            this.ProcessGlobalWind(islandDateTime);
            this.ProcessFog();
            this.ProcessRain();
            this.ProcessLightning();

            if (this.HeatPercentage > 0f && (this.world.globalEffect == null || this.world.globalEffect.effect != SonOfRobinGame.EffectMosaic))
            {
                this.world.globalEffect = new HeatWaveInstance(world: this.world, baseTexture: this.world.CameraViewRenderTarget, framesLeft: 2);
                this.world.globalEffect.intensityForTweener = this.HeatPercentage;
            }

            if (this.forecastEnd < islandDateTime + minForecastDuration) this.GenerateForecast();
        }

        private void ProcessLightning()
        {
            if (this.previousIntensityForType[WeatherType.Lightning] > 0 && this.LightningPercentage == 0)
            {
                // setting a random camera corner

                float x = (float)this.world.random.NextSingle();
                float y = (float)this.world.random.NextSingle();

                if (this.world.random.Next(2) == 0) x = (int)x;
                else y = (int)y;

                this.LightningPosMultiplier = new Vector2(x, y);
            }

            if (this.LightningJustStruck)
            {
                thunderSound.Play();
                new RumbleEvent(force: 0.1f, smallMotor: true, bigMotor: true, fadeInSeconds: 0.14f, durationSeconds: 0.14f);

                // making a shockwave
                if (this.world.random.Next(3) == 0)
                {
                    Rectangle cameraRect = this.world.camera.viewRect;

                    Vector2 lightningOriginLocation = new Vector2(
                        cameraRect.X + (this.LightningPosMultiplier.X * cameraRect.Width),
                        cameraRect.Y + (this.LightningPosMultiplier.Y * cameraRect.Height)
                        );

                    var plantPieceList = this.world.Grid.GetPiecesInCameraView(groupName: Cell.Group.ColPlantGrowth);
                    foreach (BoardPiece piece in plantPieceList)
                    {
                        Sprite sprite = piece.sprite;

                        if (IsSpriteAffectedByWind(sprite: sprite, strongWind: true))
                        {
                            float distance = Vector2.Distance(lightningOriginLocation, sprite.position);

                            world.swayManager.AddSwayEvent(targetSprite: sprite, sourceSprite: null, targetRotation: (sprite.position - lightningOriginLocation).X > 0 ? 0.15f : -0.15f, playSound: false, delayFrames: 20 + ((int)distance / 120));
                        }
                    }
                }

                // setting fire to a tree
                if (this.world.random.Next(15) == 0)
                {
                    var blockingSpritesShuffled = this.world.Grid.GetPiecesInCameraView(groupName: Cell.Group.ColMovement).OrderBy(item => this.world.random.Next());
                    foreach (BoardPiece piece in blockingSpritesShuffled)
                    {
                        if (piece.GetType() == typeof(Plant) && piece.Mass > 500)
                        {
                            var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.Visible, mainSprite: piece.sprite, distance: 800, compareWithBottom: true);

                            foreach (BoardPiece nearbyPiece in nearbyPieces)
                            {
                                if (nearbyPiece.createdByPlayer) return; // it's better to not strike anywhere near player's pieces (workshops, plants, etc.)
                            }
                            piece.HeatLevel += 1;
                            piece.buffEngine.AddBuff(buff: new Buff(type: BuffEngine.BuffType.HeatLevelLocked, autoRemoveDelay: 60 * 60 * 3, value: null), world: this.world);
                            ParticleEngine.TurnOn(sprite: piece.sprite, preset: ParticleEngine.Preset.Lightning, duration: 1);

                            break;
                        }
                    }
                }
            }
        }

        private void ProcessFog()
        {
            bool canSeeThroughFog = this.world.Player != null && this.world.Player.buffEngine.HasBuff(BuffEngine.BuffType.CanSeeThroughFog);
            Rectangle cameraRect = this.world.camera.viewRect;

            if (this.FogPercentage == 0 || canSeeThroughFog)
            {
                foreach (BoardPiece fogPiece in this.fogPieces)
                {
                    if (canSeeThroughFog || !fogPiece.sprite.GfxRect.Intersects(cameraRect)) fogPiece.Destroy();
                    else
                    {
                        if (fogPiece.sprite.opacityFade == null) new OpacityFade(sprite: fogPiece.sprite, destOpacity: 0, duration: this.world.random.Next(30, 60 * 4), destroyPiece: true);
                    }
                }
                return;
            }

            Vector2 cameraCenter = new Vector2(cameraRect.Center.X, cameraRect.Center.Y);
            Rectangle extendedCameraRect = cameraRect;
            extendedCameraRect.Inflate(cameraRect.Width / 4, cameraRect.Height / 4);

            this.fogPieces = this.fogPieces.Where(fogPiece => fogPiece.exists).ToList();

            foreach (BoardPiece fogPiece in this.fogPieces)
            {
                if (!fogPiece.sprite.GfxRect.Intersects(extendedCameraRect) && Vector2.Distance(cameraCenter, fogPiece.sprite.position) > cameraRect.Width * 2) fogPiece.Destroy();
            }

            int fogPiecesInCameraCount = this.fogPieces.Where(fogPiece => fogPiece.sprite.GfxRect.Intersects(cameraRect)).Count();
            int maxPiecesInCamera = (int)((float)extendedCameraRect.Width * (float)extendedCameraRect.Height / 30000f * this.FogPercentage);

            int fogPiecesToCreate = maxPiecesInCamera - fogPiecesInCameraCount;
            if (fogPiecesToCreate <= 0) return;

            Random random = this.world.random;

            for (int pieceNo = 0; pieceNo < fogPiecesToCreate; pieceNo++)
            {
                BoardPiece fogPiece = PieceTemplate.CreateAndPlaceOnBoard(
                    world: this.world,
                    position: new Vector2(random.Next(extendedCameraRect.Left, extendedCameraRect.Right), random.Next(extendedCameraRect.Top, extendedCameraRect.Bottom)),
                    templateName: PieceTemplate.Name.WeatherFog);

                this.fogPieces.Add(fogPiece);
            }
        }

        private void ProcessRain()
        {
            if (this.RainPercentage == 0)
            {
                if (this.rainSound.IsPlaying) this.rainSound.Stop();
                ParticleEngine.TurnOff(sprite: this.rainEmitter.sprite, preset: ParticleEngine.Preset.WeatherRain);
                return;
            }

            if (!this.rainSound.IsPlaying) this.rainSound.Play();
            this.rainSound.AdjustVolume(this.RainPercentage);

            if (this.world.globalEffect == null || this.world.globalEffect.effect != SonOfRobinGame.EffectMosaic)
            {
                this.world.globalEffect = new RainInstance(baseTexture: this.world.CameraViewRenderTarget, framesLeft: 5);
                this.world.globalEffect.intensityForTweener = this.RainPercentage;
            }

            Rectangle cameraRect = this.world.camera.viewRect;

            bool firstRun = !this.rainEmitter.sprite.IsOnBoard;
            if (firstRun) this.rainEmitter.sprite.PlaceOnBoard(position: new Vector2(cameraRect.Center.X, cameraRect.Center.Y), randomPlacement: false, ignoreCollisions: true, precisePlacement: true);

            int raindropsCount = (int)(this.RainPercentage * 2) * (cameraRect.Width * cameraRect.Height / 20000);
            if (SonOfRobinGame.fps.FPS <= 30) raindropsCount = Math.Max(raindropsCount / 4, 1);

            float targetRotation = 0.9f * this.WindPercentage;
            if (this.WindOriginX == 0) targetRotation *= -1;

            float rainBaseScale = (float)Helpers.ConvertRange(oldMin: 0f, oldMax: 1f, newMin: 0.08f, newMax: 0.14f, oldVal: this.RainPercentage, clampToEdges: true);

            int windFactorX = (int)(this.WindPercentage * 10) + this.world.random.Next(2);
            if (this.WindOriginX == 1) windFactorX *= -1; // wind blowing from the right
            this.rainWindModifier.Direction = new Vector2(windFactorX, 0);
            this.rainWindModifier.Strength = this.WindPercentage * 160f;

            this.rainGravityModifier.Strength = (float)Helpers.ConvertRange(oldMin: 0f, oldMax: 1f, newMin: 500f, newMax: 1800f, oldVal: this.RainPercentage, clampToEdges: true);
            this.rainEmitter.sprite.SetNewPosition(newPos: new Vector2(this.world.camera.viewRect.Center.X, this.world.camera.viewRect.Center.Y), ignoreCollisions: true);

            ParticleEngine.TurnOn(sprite: this.rainEmitter.sprite, preset: ParticleEngine.Preset.WeatherRain, particlesToEmit: raindropsCount, duration: 1);
            ParticleEmitter particleEmitter = ParticleEngine.GetEmitterForPreset(sprite: this.rainEmitter.sprite, preset: ParticleEngine.Preset.WeatherRain);
            if (!particleEmitter.Modifiers.Contains(this.rainWindModifier)) particleEmitter.Modifiers.Add(this.rainWindModifier);
            if (!particleEmitter.Modifiers.Contains(this.rainGravityModifier)) particleEmitter.Modifiers.Add(this.rainGravityModifier);
            particleEmitter.Parameters.Rotation = targetRotation;
            particleEmitter.Offset = new Vector2(this.WindPercentage * ((float)cameraRect.Width * 0.6f) * (this.WindOriginX == 1 ? 1f : -1f), 0);
            particleEmitter.LifeSpan = TimeSpan.FromSeconds(2.5f / this.RainPercentage);
            particleEmitter.Parameters.Scale = new Range<float>(rainBaseScale * 0.9f, rainBaseScale * 1.05f);

            if (firstRun || ((SonOfRobinGame.CurrentUpdate % 5 == 0) && (Math.Abs(this.lastRainRect.Width - cameraRect.Width) > 10)))
            {
                this.lastRainRect = cameraRect;
                particleEmitter.Profile = Profile.BoxFill(width: cameraRect.Width * 2.2f, height: cameraRect.Height / 2);
                MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} rain - cameraRect changed {cameraRect.Width}x{cameraRect.Height}");
            }
        }

        private void ProcessGlobalWind(DateTime islandDateTime)
        {
            if (this.WindPercentage == 0 && this.windSound.IsPlaying) this.windSound.Stop();
            else
            {
                if (!this.windSound.IsPlaying) this.windSound.Play();
                this.windSound.AdjustVolume(this.WindPercentage);
            }

            if (!Preferences.plantsSway) return;

            if (this.WindPercentage < 0.2)
            {
                this.WindOriginX = -1;
                this.WindOriginY = -1;
                this.NextGlobalWindBlow = veryOldDate;

                return;
            }

            if (this.NextGlobalWindBlow > islandDateTime || Scene.GetTopSceneOfType(typeof(TextWindow)) != null) return;

            if (this.WindOriginX == -1 || this.WindOriginY == -1)
            {
                this.WindOriginX = this.world.random.Next(2);
                this.WindOriginY = (float)this.world.random.NextSingle();
            }

            TimeSpan minCooldown = TimeSpan.FromMinutes(1 - (this.WindPercentage * 1));
            TimeSpan maxCooldown = TimeSpan.FromMinutes(8 - (this.WindPercentage * 3));
            TimeSpan windCooldown = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxCooldown - minCooldown).Ticks) + minCooldown.Ticks);

            this.NextGlobalWindBlow = islandDateTime + windCooldown;

            Rectangle cameraRect = this.world.camera.viewRect;
            int x = cameraRect.X + (int)(cameraRect.Width * WindOriginX);
            int y = cameraRect.Y + (int)(cameraRect.Height * WindOriginY);
            Vector2 windOriginLocation = new Vector2(x, y);

            float targetRotation = 0.38f * this.WindPercentage;

            int rotationSlowdown = this.world.random.Next(10, 25);
            rotationSlowdown = (int)((float)rotationSlowdown / this.WindPercentage);

            bool strongWind = this.WindPercentage >= 0.75f;

            var affectedPiecesList = this.world.Grid.GetPiecesInCameraView(groupName: Cell.Group.Visible);
            foreach (BoardPiece piece in affectedPiecesList)
            {
                Sprite sprite = piece.sprite;

                if (IsSpriteAffectedByWind(sprite: sprite, strongWind: strongWind))
                {
                    float distance = Vector2.Distance(windOriginLocation, sprite.position);
                    float finalRotation = (sprite.position - windOriginLocation).X > 0 ? targetRotation : -targetRotation;
                    if (sprite.BlocksMovement) finalRotation *= 0.3f;

                    int delayFrames = (int)distance / 20;

                    this.world.swayManager.AddSwayEvent(targetSprite: sprite, sourceSprite: null, targetRotation: finalRotation, playSound: false, delayFrames: delayFrames, rotationSlowdown: rotationSlowdown);

                    if (piece.pieceInfo.windParticlesDict.Count > 0 &&
                        (piece.pieceInfo.plantAdultSizeMass == 0 || sprite.AnimSize > 0) &&
                        this.world.random.Next(5) == 0
                        )
                    {
                        new Scheduler.Task(taskName: Scheduler.TaskName.TurnOnWindParticles, executeHelper: piece, delay: delayFrames);
                    }
                }
            }

            if (strongWind && !this.world.BuildMode && !this.world.CineMode) // moving pieces
            {
                foreach (BoardPiece piece in affectedPiecesList)
                {
                    Type pieceType = piece.GetType();
                    Sprite sprite = piece.sprite;

                    if (piece.pieceInfo.canBePickedUp || pieceType == typeof(Animal) || (pieceType == typeof(Player) && piece.name != PieceTemplate.Name.PlayerGhost))
                    {
                        float distance = Vector2.Distance(windOriginLocation, sprite.position);

                        Vector2 movement = (sprite.position - windOriginLocation) / this.world.random.Next(1, 3);

                        var movementData = new Dictionary<string, Object> { { "boardPiece", piece }, { "movement", movement } };
                        new Scheduler.Task(taskName: Scheduler.TaskName.AddPassiveMovement, delay: (int)distance / 20, executeHelper: movementData);
                    }
                }
            }
        }

        private static bool IsSpriteAffectedByWind(Sprite sprite, bool strongWind)
        {
            return !sprite.boardPiece.pieceInfo.canBePickedUp && sprite.boardPiece.pieceInfo.isAffectedByWind && (!sprite.BlocksMovement || strongWind);
        }

        public void AddLocalizedWind(Vector2 windOriginLocation)
        {
            if (!Preferences.plantsSway) return;

            DateTime islandDateTime = this.islandClock.IslandDateTime;

            if (this.NextLocalizedWindBlow > islandDateTime) return;

            // BoardPiece crossHair = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: windOriginLocation, templateName: PieceTemplate.Name.Crosshair); // for testing
            // new LevelEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: 120, boardPiece: crossHair); // for testing
            // SonOfRobinGame.messageLog.AddMessage(text: $"Adding localized wind at {windOriginLocation.X},{windOriginLocation.Y}"); // for testing

            TimeSpan minCooldown = TimeSpan.FromMinutes(1);
            TimeSpan maxCooldown = TimeSpan.FromMinutes(3);
            TimeSpan windCooldown = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxCooldown - minCooldown).Ticks) + minCooldown.Ticks);

            this.NextLocalizedWindBlow = islandDateTime + windCooldown;
            float targetRotation = 0.2f;

            foreach (BoardPiece piece in this.world.Grid.GetPiecesInCameraView(groupName: Cell.Group.ColPlantGrowth))
            {
                Sprite sprite = piece.sprite;

                if (!sprite.BlocksMovement)
                {
                    float distance = Vector2.Distance(windOriginLocation, sprite.position);
                    float finalRotation = (sprite.position - windOriginLocation).X > 0 ? targetRotation : -targetRotation;
                    if (sprite.BlocksMovement) finalRotation *= 0.3f;

                    this.world.swayManager.AddSwayEvent(targetSprite: sprite, sourceSprite: null, targetRotation: finalRotation, playSound: false, delayFrames: (int)distance / 20, rotationSlowdown: this.world.random.Next(8, 18));
                }
            }
        }

        private void GenerateForecast()
        {
            DateTime islandDateTime = this.islandClock.IslandDateTime;

            DateTime forecastStartTime = islandDateTime;

            if (!this.firstForecastCreated)
            {
                forecastStartTime = islandDateTime + TimeSpan.FromHours(10); // no weather effects for the first few hours
                this.firstForecastCreated = true;
            }
            else
            {
                if (this.weatherEvents.Count > 0)
                {
                    var weatherEventsByEndTime = this.weatherEvents.OrderByDescending(e => e.endTime).ToList();
                    forecastStartTime = weatherEventsByEndTime[0].endTime;
                }
            }

            this.forecastEnd = forecastStartTime + maxForecastDuration;

            float minVal = 0.2f;
            float maxVal = 0.8f;
            float addChanceFactor = 0.2f; // minimum value in 0 - 1 range, that will generate a single event

            if (this.world.random.Next(6) == 0)
            {
                // bad weather happens from time to time
                minVal += Helpers.GetRandomFloatForRange(random: this.world.random, minVal: 0.0f, maxVal: 0.5f);
                maxVal = 1.0f;
                addChanceFactor = 0.0f;
            }

            float cloudsMaxIntensity = Helpers.GetRandomFloatForRange(random: this.world.random, minVal: minVal, maxVal: maxVal);
            float rainMaxIntensity = Helpers.GetRandomFloatForRange(random: this.world.random, minVal: minVal, maxVal: maxVal);
            float windMaxIntensity = Helpers.GetRandomFloatForRange(random: this.world.random, minVal: minVal, maxVal: maxVal);
            float fogMaxIntensity = Helpers.GetRandomFloatForRange(random: this.world.random, minVal: minVal, maxVal: maxVal);

            // adding clouds
            this.AddNewWeatherEvents(type: WeatherType.Clouds, startTime: forecastStartTime, endTime: this.forecastEnd, minDuration: TimeSpan.FromMinutes(20), maxDuration: TimeSpan.FromHours(8), minGap: TimeSpan.FromMinutes(30), maxGap: TimeSpan.FromHours(10), maxIntensity: cloudsMaxIntensity, addChanceFactor: addChanceFactor);

            this.AddNewWeatherEvents(type: WeatherType.Wind, startTime: forecastStartTime, endTime: this.forecastEnd, minDuration: TimeSpan.FromMinutes(20), maxDuration: TimeSpan.FromHours(2), minGap: TimeSpan.FromMinutes(30), maxGap: TimeSpan.FromHours(6), maxIntensity: windMaxIntensity, addChanceFactor: Math.Min(addChanceFactor + 0.2f, 1.0f));

            // adding rain
            foreach (WeatherEvent weatherEvent in this.weatherEvents.ToList())
            {
                if (weatherEvent.type == WeatherType.Clouds && weatherEvent.intensity >= 0.5f)
                {
                    TimeSpan minDuration = TimeSpan.FromTicks((long)(weatherEvent.duration.Ticks * 0.4d));
                    TimeSpan maxDuration = TimeSpan.FromTicks((long)(weatherEvent.duration.Ticks * 0.9d));
                    TimeSpan maxGap = TimeSpan.FromTicks(weatherEvent.duration.Ticks - maxDuration.Ticks);

                    this.AddNewWeatherEvents(type: WeatherType.Rain, startTime: weatherEvent.startTime, endTime: weatherEvent.endTime, minDuration: minDuration, maxDuration: maxDuration, minGap: TimeSpan.FromMinutes(0), maxGap: maxGap, maxIntensity: rainMaxIntensity, addChanceFactor: addChanceFactor);

                    this.AddNewWeatherEvents(type: WeatherType.Wind, startTime: weatherEvent.startTime, endTime: weatherEvent.endTime, minDuration: minDuration, maxDuration: maxDuration, minGap: TimeSpan.FromMinutes(0), maxGap: maxGap, maxIntensity: windMaxIntensity, addChanceFactor: Math.Max(addChanceFactor - 0.1f, 0.0f));
                }
            }

            // adding lightning
            foreach (WeatherEvent weatherEvent in this.weatherEvents.ToList())
            {
                if (weatherEvent.type == WeatherType.Rain && weatherEvent.intensity >= 0.6f)
                {
                    this.AddNewWeatherEvents(type: WeatherType.Lightning, startTime: weatherEvent.startTime, endTime: weatherEvent.endTime, minDuration: TimeSpan.FromSeconds(25), maxDuration: TimeSpan.FromSeconds(55), minGap: TimeSpan.FromMinutes(1), maxGap: TimeSpan.FromMinutes(15), maxIntensity: 1f, addChanceFactor: addChanceFactor, randomizeIntensity: false);
                }
            }

            // adding fog
            var morningTimes = Helpers.GetTimeOfDayOccurrences(startTime: islandDateTime, endTime: this.forecastEnd, checkTimeStart: TimeSpan.FromHours(4), checkTimeEnd: TimeSpan.FromHours(6));
            var eveningTimes = Helpers.GetTimeOfDayOccurrences(startTime: islandDateTime, endTime: this.forecastEnd, checkTimeStart: TimeSpan.FromHours(16), checkTimeEnd: TimeSpan.FromHours(18));

            Dictionary<DateTime, TimeSpan> whenFogCanOccur = morningTimes.Union(eveningTimes).ToDictionary(x => x.Key, x => x.Value);

            foreach (var kvp in whenFogCanOccur)
            {
                DateTime startTime = kvp.Key;
                TimeSpan duration = kvp.Value;
                DateTime endTime = startTime + duration;

                this.AddNewWeatherEvents(type: WeatherType.Fog, startTime: startTime, endTime: endTime, minDuration: TimeSpan.FromTicks(duration.Ticks / 2), maxDuration: duration, minGap: TimeSpan.FromMinutes(0), maxGap: TimeSpan.FromTicks(duration.Ticks / 5), maxIntensity: fogMaxIntensity, addChanceFactor: addChanceFactor);
            }
        }

        private void AddNewWeatherEvents(WeatherType type, DateTime startTime, DateTime endTime, TimeSpan minDuration, TimeSpan maxDuration, TimeSpan minGap, TimeSpan maxGap, float maxIntensity, float addChanceFactor = 1, bool randomizeIntensity = true)
        {
            DateTime timeCursor = startTime;

            while (true)
            {
                TimeSpan gap = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxGap - minGap).Ticks) + minGap.Ticks);
                timeCursor += gap;

                if (timeCursor >= endTime) return; // no point in adding event, if timeCursor is outside range

                if (timeCursor + maxDuration > endTime || timeCursor + minDuration > endTime)
                {
                    maxDuration = endTime - timeCursor;
                    minDuration = TimeSpan.FromTicks(maxDuration.Ticks / 3);

                    if (minDuration < TimeSpan.FromMinutes(10)) return;
                }

                TimeSpan duration = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxDuration - minDuration).Ticks) + minDuration.Ticks);

                TimeSpan maxTransition = TimeSpan.FromTicks((long)(duration.Ticks / 4));
                TimeSpan minTransition = TimeSpan.FromTicks((long)(maxTransition.Ticks / 3));
                TimeSpan transition = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxTransition - minTransition).Ticks) + minTransition.Ticks);

                float intensity = randomizeIntensity ? Helpers.GetRandomFloatForRange(random: this.world.random, minVal: 0.5f, maxVal: maxIntensity) : maxIntensity;

                bool add = addChanceFactor == 0 || this.world.random.NextSingle() >= addChanceFactor;
                if (add) this.weatherEvents.Add(new WeatherEvent(type: type, intensity: intensity, startTime: timeCursor, duration: duration, transitionLength: transition));

                timeCursor += duration;
                if (timeCursor >= endTime) return;
            }
        }

        private void UpdateCurrentIntensities()
        {
            foreach (WeatherType type in allTypes)
            {
                this.previousIntensityForType[type] = this.currentIntensityForType[type];
                this.currentIntensityForType[type] = 0;
            }

            DateTime islandDateTime = this.islandClock.IslandDateTime;
            foreach (WeatherEvent weatherEvent in this.weatherEvents)
            {
                this.currentIntensityForType[weatherEvent.type] = Math.Max(this.currentIntensityForType[weatherEvent.type], weatherEvent.GetIntensity(islandDateTime));
            }
        }

        public void RemoveAllEventsForDuration(TimeSpan duration)
        {
            DateTime endTime = this.world.islandClock.IslandDateTime + duration;
            weatherEvents.RemoveAll(e => e.startTime < endTime);

            if (endTime > this.forecastEnd) this.forecastEnd = endTime; // to prevent from overwriting with new forecast
        }

        public void CreateVeryBadWeatherForDuration(TimeSpan duration)
        {
            TimeSpan transitionDuration = TimeSpan.FromMinutes(8);
            if (duration < transitionDuration * 2) return;

            DateTime startTime = this.world.islandClock.IslandDateTime;
            DateTime endTime = startTime + duration;

            this.weatherEvents.Add(new WeatherEvent(type: WeatherType.Clouds, intensity: 1f, startTime: startTime, duration: duration, transitionLength: transitionDuration));

            this.weatherEvents.Add(new WeatherEvent(type: WeatherType.Rain, intensity: 1f, startTime: startTime, duration: duration, transitionLength: transitionDuration / 3));

            this.weatherEvents.Add(new WeatherEvent(type: WeatherType.Fog, intensity: 1f, startTime: startTime, duration: duration, transitionLength: transitionDuration));

            this.weatherEvents.Add(new WeatherEvent(type: WeatherType.Wind, intensity: 0.45f, startTime: startTime, duration: duration, transitionLength: transitionDuration));

            this.AddNewWeatherEvents(type: WeatherType.Lightning, startTime: startTime, endTime: endTime, minDuration: TimeSpan.FromSeconds(25), maxDuration: TimeSpan.FromSeconds(55), minGap: TimeSpan.FromMinutes(1), maxGap: TimeSpan.FromMinutes(15), maxIntensity: 1f, addChanceFactor: 0.8f, randomizeIntensity: false);
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> weatherData = new()
            {
                { "weatherEvents", this.weatherEvents },
                { "currentIntensityForType", this.currentIntensityForType },
                { "forecastEnd", this.forecastEnd },
                { "firstForecastCreated", this.firstForecastCreated },
                { "CloudsPercentage", this.CloudsPercentage },
                { "FogPercentage", this.FogPercentage },
                { "SunVisibility", this.SunVisibility },
                { "WindOriginX", this.WindOriginX },
                { "WindOriginY", this.WindOriginY },
                { "WindPercentage", this.WindPercentage },
                { "RainPercentage", this.RainPercentage },
                { "NextGlobalWindBlow", this.NextGlobalWindBlow },
                { "NextLocalizedWindBlow", this.NextLocalizedWindBlow },
            };

            return weatherData;
        }

        public void Deserialize(Dictionary<string, Object> weatherData)
        {
            var weatherEvents = (List<WeatherEvent>)weatherData["weatherEvents"];
            this.weatherEvents.Clear();
            this.weatherEvents.AddRange(weatherEvents);

            this.currentIntensityForType.Clear();

            foreach (WeatherType type in allTypes)
            {
                this.currentIntensityForType[type] = 0; // for compatibility with older saves (when introducing new weather type)
            }

            foreach (var kvp in (Dictionary<WeatherType, float>)weatherData["currentIntensityForType"])
            {
                this.currentIntensityForType[kvp.Key] = kvp.Value;
            }

            this.forecastEnd = (DateTime)weatherData["forecastEnd"];
            this.firstForecastCreated = (bool)weatherData["firstForecastCreated"];
            this.CloudsPercentage = (float)(double)weatherData["CloudsPercentage"];
            this.FogPercentage = (float)(double)weatherData["FogPercentage"];
            this.SunVisibility = (float)(double)weatherData["SunVisibility"];
            this.WindOriginX = (float)(double)weatherData["WindOriginX"];
            this.WindOriginY = (float)(double)weatherData["WindOriginY"];
            this.WindPercentage = (float)(double)weatherData["WindPercentage"];
            this.RainPercentage = (float)(double)weatherData["RainPercentage"];
            this.NextGlobalWindBlow = (DateTime)weatherData["NextGlobalWindBlow"];
            this.NextLocalizedWindBlow = (DateTime)weatherData["NextLocalizedWindBlow"];
        }

        public void AddEvent(WeatherEvent weatherEvent)
        {
            this.weatherEvents.Add(weatherEvent);
        }
    }
}