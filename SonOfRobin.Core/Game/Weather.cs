using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    [Serializable]
    public struct WeatherEvent
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

            // MessageLog.AddMessage(msgType: MsgType.User, message: $"New WeatherEvent {this.type}, D {this.startTime:d HH\\:mm} - D {this.endTime:d HH\\:mm} ({this.duration:hh\\:mm}), transLength {transitionLength:hh\\:mm}, intensity {intensity}"); // for testing
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
        public enum WeatherType
        { Wind, Clouds, Rain, Lightning, Fog }

        public static readonly WeatherType[] allTypes = (WeatherType[])Enum.GetValues(typeof(WeatherType));
        private static readonly DateTime veryOldDate = new DateTime(1900, 1, 1);
        private static readonly TimeSpan minForecastDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan maxForecastDuration = minForecastDuration + minForecastDuration;

        public static readonly List<SoundData.Name> thunderNames = Enum.GetValues(typeof(SoundData.Name))
            .Cast<SoundData.Name>().Where(x => x.ToString().StartsWith("Thunder")).ToList();

        public static readonly Sound thunderSound = new Sound(nameList: thunderNames, cooldown: 60, ignore3DAlways: true, maxPitchVariation: 0.5f);

        private readonly World world;
        private readonly IslandClock islandClock;
        private readonly List<WeatherEvent> weatherEvents;
        private DateTime forecastEnd;
        private readonly Dictionary<WeatherType, float> currentIntensityForType;
        private readonly Dictionary<WeatherType, float> previousIntensityForType;
        private bool firstForecastCreated;
        private readonly Sound windSound;
        private readonly Sound rainSound;
        public float CloudsPercentage { get; private set; }
        public float FogPercentage { get; private set; }
        public float SunVisibility { get; private set; }
        public float WindPercentage { get; private set; }
        public float RainPercentage { get; private set; }
        public float LightningPercentage { get; private set; }

        public bool IsRaining
        { get { return this.RainPercentage > 0.2f; } }

        public float WindOriginX { get; private set; }
        public float WindOriginY { get; private set; }
        public DateTime NextGlobalWindBlow { get; private set; }
        public DateTime NextLocalizedWindBlow { get; private set; }
        private int rainCooldownFramesLeft;
        public Vector2 LightningPosMultiplier { get; private set; } // to be multiplied by sunPos

        public Weather(World world, IslandClock islandClock)
        {
            this.world = world;
            this.islandClock = islandClock;
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
            this.rainCooldownFramesLeft = -1;
            this.WindPercentage = 0f;

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
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Forecast exceeded by {(dateTime - this.forecastEnd).TotalHours} hours when trying to get weather for {type}.");
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
            DateTime islandDateTime = this.islandClock.IslandDateTime;
            weatherEvents.RemoveAll(e => e.endTime < islandDateTime);

            this.UpdateCurrentIntensities();

            this.CloudsPercentage = Math.Min(this.GetIntensityForWeatherType(WeatherType.Clouds) * 2, 1);
            this.FogPercentage = this.GetIntensityForWeatherType(WeatherType.Fog);
            this.SunVisibility = Math.Max(0, 1f - (this.CloudsPercentage + (this.FogPercentage * 0.5f)));
            this.WindPercentage = this.GetIntensityForWeatherType(WeatherType.Wind);
            this.RainPercentage = this.GetIntensityForWeatherType(WeatherType.Rain);

            this.LightningPercentage = this.GetIntensityForWeatherType(WeatherType.Lightning);
            if (this.previousIntensityForType[WeatherType.Lightning] < this.currentIntensityForType[WeatherType.Lightning])
            {
                // lightning in-transition should be ignored
                this.LightningPercentage = 0;
            }

            this.ProcessGlobalWind(islandDateTime);
            this.ProcessRain();
            this.ProcessLightning();

            if (this.forecastEnd < islandDateTime + minForecastDuration) this.GenerateForecast();
        }

        private void ProcessLightning()
        {
            if (this.previousIntensityForType[WeatherType.Lightning] > 0 && this.LightningPercentage == 0)
            {
                // setting a random camera corner

                float x = (float)this.world.random.NextDouble();
                float y = (float)this.world.random.NextDouble();

                if (this.world.random.Next(2) == 0) x = (int)x;
                else y = (int)y;

                this.LightningPosMultiplier = new Vector2(x, y);
            }

            if (this.LightningJustStruck)
            {
                thunderSound.Play();

                if (this.world.random.Next(3) == 0)
                {
                    // making a shockwave

                    Rectangle cameraRect = this.world.camera.viewRect;

                    Vector2 lightningOriginLocation = new Vector2(
                        cameraRect.X + (this.LightningPosMultiplier.X * cameraRect.Width),
                        cameraRect.Y + (this.LightningPosMultiplier.Y * cameraRect.Height)
                        );

                    var plantSpriteList = new List<Sprite>();
                    world.Grid.GetSpritesInCameraViewAndPutIntoList(camera: world.camera, groupName: Cell.Group.ColPlantGrowth, spriteListToFill: plantSpriteList);

                    foreach (Sprite sprite in plantSpriteList)
                    {
                        if (IsSpriteAffectedByWind(sprite: sprite, strongWind: true))
                        {
                            float distance = Vector2.Distance(lightningOriginLocation, sprite.position);

                            world.swayManager.AddSwayEvent(targetSprite: sprite, sourceSprite: null, targetRotation: (sprite.position - lightningOriginLocation).X > 0 ? 0.15f : -0.15f, playSound: false, delayFrames: 20 + ((int)distance / 120));
                        }
                    }
                }
            }
        }

        private void ProcessRain()
        {
            if (this.RainPercentage == 0 && this.rainSound.IsPlaying) this.rainSound.Stop();
            else
            {
                if (!this.rainSound.IsPlaying) this.rainSound.Play();
                this.rainSound.AdjustVolume(this.RainPercentage);
            }

            if (this.RainPercentage == 0)
            {
                this.rainCooldownFramesLeft = -1;
                return;
            }

            this.rainCooldownFramesLeft--;
            if (this.rainCooldownFramesLeft > 0) return;

            this.rainCooldownFramesLeft = this.world.random.Next(2, 6) - (int)(this.RainPercentage * 5);

            Rectangle extendedViewRect = this.world.camera.ExtendedViewRect;

            byte dropSize = (byte)Helpers.ConvertRange(oldMin: 0, oldMax: 1, newMin: 0, newMax: 7, oldVal: this.RainPercentage, clampToEdges: true);

            int raindropsCount = 2 + (int)(this.RainPercentage * 3) * (this.world.camera.viewRect.Width * this.world.camera.viewRect.Height / 1000000);
            raindropsCount = Math.Min(raindropsCount, 5);

            int leftCameraEdge = Math.Max(extendedViewRect.Left, 0); // clipping to island edges
            int rightCameraEdge = Math.Min(extendedViewRect.Right, this.world.width); // clipping to island edges

            for (int i = 0; i < raindropsCount; i++)
            {
                Vector2 position = new Vector2(this.world.random.Next(leftCameraEdge, rightCameraEdge), world.camera.viewRect.Top);
                BoardPiece rainDrop = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: position, templateName: PieceTemplate.Name.RainDrop, closestFreeSpot: true);

                rainDrop.sprite.AssignNewSize(dropSize);
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

            if (this.NextGlobalWindBlow > islandDateTime) return;

            if (this.WindOriginX == -1 || this.WindOriginY == -1)
            {
                this.WindOriginX = this.world.random.Next(0, 2);
                this.WindOriginY = (float)this.world.random.NextDouble();
            }

            TimeSpan minCooldown = TimeSpan.FromMinutes(1 - (this.WindPercentage * 1));
            TimeSpan maxCooldown = TimeSpan.FromMinutes(8 - (this.WindPercentage * 3));
            TimeSpan windCooldown = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxCooldown - minCooldown).Ticks) + minCooldown.Ticks);

            this.NextGlobalWindBlow = islandDateTime + windCooldown;

            Rectangle cameraRect = this.world.camera.viewRect;
            int x = cameraRect.X + (int)(cameraRect.Width * WindOriginX);
            int y = cameraRect.Y + (int)(cameraRect.Height * WindOriginY);
            Vector2 windOriginLocation = new Vector2(x, y);

            var affectedSpriteList = new List<Sprite>();
            this.world.Grid.GetSpritesInCameraViewAndPutIntoList(camera: this.world.camera, groupName: Cell.Group.Visible, spriteListToFill: affectedSpriteList);

            float targetRotation = 0.38f * this.WindPercentage;

            int rotationSlowdown = this.world.random.Next(10, 25);
            rotationSlowdown = (int)((float)rotationSlowdown / this.WindPercentage);

            bool strongWind = this.WindPercentage >= 0.75f;

            foreach (Sprite sprite in affectedSpriteList)
            {
                if (IsSpriteAffectedByWind(sprite: sprite, strongWind: strongWind))
                {
                    float distance = Vector2.Distance(windOriginLocation, sprite.position);
                    float finalRotation = (sprite.position - windOriginLocation).X > 0 ? targetRotation : -targetRotation;
                    if (sprite.blocksMovement) finalRotation *= 0.3f;

                    this.world.swayManager.AddSwayEvent(targetSprite: sprite, sourceSprite: null, targetRotation: finalRotation, playSound: false, delayFrames: (int)distance / 20, rotationSlowdown: rotationSlowdown);
                }
            }

            if (strongWind) // moving pieces
            {
                foreach (Sprite sprite in affectedSpriteList)
                {
                    Type pieceType = sprite.boardPiece.GetType();

                    if (sprite.boardPiece.canBePickedUp || pieceType == typeof(Animal) || pieceType == typeof(Debris) || (pieceType == typeof(Player) && sprite.boardPiece.name != PieceTemplate.Name.PlayerGhost))
                    {
                        float distance = Vector2.Distance(windOriginLocation, sprite.position);

                        Vector2 movement = (sprite.position - windOriginLocation) / this.world.random.Next(1, 3);

                        var movementData = new Dictionary<string, Object> { { "boardPiece", sprite.boardPiece }, { "movement", movement } };
                        new Scheduler.Task(taskName: Scheduler.TaskName.AddPassiveMovement, delay: (int)distance / 20, executeHelper: movementData);
                    }
                }
            }
        }

        private static bool IsSpriteAffectedByWind(Sprite sprite, bool strongWind)
        {
            return !sprite.boardPiece.canBePickedUp && sprite.isAffectedByWind && (!sprite.blocksMovement || strongWind);
        }

        public void AddLocalizedWind(Vector2 windOriginLocation)
        {
            if (!Preferences.plantsSway) return;

            DateTime islandDateTime = this.islandClock.IslandDateTime;

            if (this.NextLocalizedWindBlow > islandDateTime) return;

            // BoardPiece crossHair = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: windOriginLocation, templateName: PieceTemplate.Name.Crosshair); // for testing
            // new WorldEvent(eventName: WorldEvent.EventName.Destruction, world: this.world, delay: 120, boardPiece: crossHair); // for testing
            // MessageLog.AddMessage(msgType: MsgType.User, message: $"Adding localized wind at {windOriginLocation.X},{windOriginLocation.Y}"); // for testing

            TimeSpan minCooldown = TimeSpan.FromMinutes(1);
            TimeSpan maxCooldown = TimeSpan.FromMinutes(3);
            TimeSpan windCooldown = TimeSpan.FromTicks((long)(this.world.random.NextDouble() * (maxCooldown - minCooldown).Ticks) + minCooldown.Ticks);

            this.NextLocalizedWindBlow = islandDateTime + windCooldown;

            var affectedSpriteList = new List<Sprite>();
            this.world.Grid.GetSpritesInCameraViewAndPutIntoList(camera: this.world.camera, groupName: Cell.Group.ColPlantGrowth, spriteListToFill: affectedSpriteList);

            float targetRotation = 0.2f;

            foreach (Sprite sprite in affectedSpriteList)
            {
                if (!sprite.blocksMovement)
                {
                    float distance = Vector2.Distance(windOriginLocation, sprite.position);
                    float finalRotation = (sprite.position - windOriginLocation).X > 0 ? targetRotation : -targetRotation;
                    if (sprite.blocksMovement) finalRotation *= 0.3f;

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
                if (this.weatherEvents.Any())
                {
                    var weatherEventsByEndTime = this.weatherEvents.OrderByDescending(e => e.endTime).ToList();
                    forecastStartTime = weatherEventsByEndTime[0].endTime;
                }
            }

            this.forecastEnd = forecastStartTime + maxForecastDuration;

            float minVal = 0.2f;
            float maxVal = 0.8f;
            float addChanceFactor = 0.3f; // minimum value in 0 - 1 range, that will generate a single event

            if (this.world.random.Next(0, 3) == 0)
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
                TimeSpan gap = TimeSpan.FromTicks((long)(world.random.NextDouble() * (maxGap - minGap).Ticks) + minGap.Ticks);
                timeCursor += gap;

                if (timeCursor >= endTime) return; // no point in adding event, if timeCursor is outside range

                if (timeCursor + maxDuration > endTime || timeCursor + minDuration > endTime)
                {
                    maxDuration = endTime - timeCursor;
                    minDuration = TimeSpan.FromTicks(maxDuration.Ticks / 3);

                    if (minDuration < TimeSpan.FromMinutes(10)) return;
                }

                TimeSpan duration = TimeSpan.FromTicks((long)(world.random.NextDouble() * (maxDuration - minDuration).Ticks) + minDuration.Ticks);

                TimeSpan maxTransition = TimeSpan.FromTicks((long)(duration.Ticks / 4));
                TimeSpan minTransition = TimeSpan.FromTicks((long)(maxTransition.Ticks / 3));
                TimeSpan transition = TimeSpan.FromTicks((long)(world.random.NextDouble() * (maxTransition - minTransition).Ticks) + minTransition.Ticks);

                float intensity = randomizeIntensity ? Helpers.GetRandomFloatForRange(random: this.world.random, minVal: 0.5f, maxVal: maxIntensity) : maxIntensity;

                bool add = addChanceFactor == 0 || world.random.NextDouble() >= addChanceFactor;
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

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> weatherData = new Dictionary<string, object>
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
                { "rainCooldownFramesLeft", this.rainCooldownFramesLeft },
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
                this.currentIntensityForType[type] = 0; // for old saves compatibility
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
            this.rainCooldownFramesLeft = (int)(Int64)weatherData["rainCooldownFramesLeft"];
        }

        public void AddEvent(WeatherEvent weatherEvent)
        {
            this.weatherEvents.Add(weatherEvent);
        }
    }
}