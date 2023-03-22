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

            MessageLog.AddMessage(msgType: MsgType.User, message: $"New WeatherEvent {this.type}, {this.startTime:HH\\:mm} - {this.endTime:HH\\:mm} ({this.duration:hh\\:mm}), transLength {transitionLength:hh\\:mm}, intensity {intensity}"); // for testing
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
        { Wind, Clouds, Rain, Fog }

        public static readonly WeatherType[] allTypes = (WeatherType[])Enum.GetValues(typeof(WeatherType));

        private static readonly TimeSpan minForecastDuration = TimeSpan.FromDays(1);
        private static readonly TimeSpan maxForecastDuration = minForecastDuration + minForecastDuration;

        private readonly Random random;
        private readonly IslandClock islandClock;
        private readonly List<WeatherEvent> weatherEvents;
        private DateTime forecastEnd;
        private readonly Dictionary<WeatherType, float> currentIntensityForType;

        public Weather(Random random, IslandClock islandClock)
        {
            this.random = random;
            this.islandClock = islandClock;
            this.weatherEvents = new List<WeatherEvent>();
            this.forecastEnd = new DateTime(1900, 1, 1); // to ensure first update

            this.currentIntensityForType = new Dictionary<WeatherType, float>();
            foreach (WeatherType type in allTypes)
            {
                this.currentIntensityForType[type] = 0;
            }
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

            float intensity = 0;
            foreach (WeatherEvent weatherEvent in this.weatherEvents)
            {
                if (weatherEvent.type == type) intensity = Math.Min(intensity + weatherEvent.GetIntensity(dateTime), 1);
            }

            return intensity;
        }

        public void Update()
        {
            DateTime islandDateTime = this.islandClock.IslandDateTime;
            weatherEvents.RemoveAll(e => e.endTime < islandDateTime);

            this.UpdateCurrentIntensities();

            if (this.forecastEnd < islandDateTime + minForecastDuration) this.GenerateForecast();
        }

        private void GenerateForecast()
        {
            DateTime islandDateTime = this.islandClock.IslandDateTime;

            DateTime forecastStartTime = islandDateTime + minForecastDuration;
            DateTime forecastEndTime = islandDateTime + maxForecastDuration;

            float minVal = 0.2f;
            float maxVal = 1f;

            if (this.random.Next(0, 4) != 0)
            {
                minVal += Helpers.GetRandomFloatForRange(random: this.random, minVal: 0.0f, maxVal: 0.5f);
            }

            float cloudsMaxIntensity = Helpers.GetRandomFloatForRange(random: this.random, minVal: minVal, maxVal: maxVal);
            float rainMaxIntensity = Helpers.GetRandomFloatForRange(random: this.random, minVal: minVal, maxVal: maxVal);
            float windMaxIntensity = Helpers.GetRandomFloatForRange(random: this.random, minVal: minVal, maxVal: maxVal);
            float fogMaxIntensity = Helpers.GetRandomFloatForRange(random: this.random, minVal: minVal, maxVal: maxVal);

            // adding clouds
            this.AddNewWeatherEvents(type: WeatherType.Clouds, startTime: forecastStartTime, endTime: forecastEndTime, minDuration: TimeSpan.FromHours(0), maxDuration: TimeSpan.FromHours(8), minGap: TimeSpan.FromMinutes(30), maxGap: TimeSpan.FromHours(10), maxIntensity: cloudsMaxIntensity);

            // adding rain
            foreach (WeatherEvent weatherEvent in this.weatherEvents.ToList())
            {
                if (weatherEvent.type == WeatherType.Clouds && weatherEvent.intensity >= 0.5f)
                {
                    TimeSpan minDuration = TimeSpan.FromTicks((long)(weatherEvent.duration.Ticks * 0.4d));
                    TimeSpan maxDuration = TimeSpan.FromTicks((long)(weatherEvent.duration.Ticks * 0.9d));
                    TimeSpan maxGap = TimeSpan.FromTicks(weatherEvent.duration.Ticks - maxDuration.Ticks);

                    this.AddNewWeatherEvents(type: WeatherType.Rain, startTime: weatherEvent.startTime, endTime: weatherEvent.endTime, minDuration: minDuration, maxDuration: maxDuration, minGap: TimeSpan.FromMinutes(0), maxGap: maxGap, maxIntensity: cloudsMaxIntensity);
                }
            }

            this.forecastEnd = forecastEndTime;
        }

        private void AddNewWeatherEvents(WeatherType type, DateTime startTime, DateTime endTime, TimeSpan minDuration, TimeSpan maxDuration, TimeSpan minGap, TimeSpan maxGap, float maxIntensity)
        {
            DateTime currentSegmentStart = startTime;

            while (true)
            {
                TimeSpan gap = TimeSpan.FromTicks((long)(random.NextDouble() * (maxGap - minGap).Ticks) + minGap.Ticks);
                TimeSpan duration = TimeSpan.FromTicks((long)(random.NextDouble() * (maxDuration - minDuration).Ticks) + minDuration.Ticks);

                TimeSpan maxTransition = TimeSpan.FromTicks((long)(duration.Ticks / 4));
                TimeSpan minTransition = TimeSpan.FromTicks((long)(maxTransition.Ticks / 3));
                TimeSpan transition = TimeSpan.FromTicks((long)(random.NextDouble() * (maxTransition - minTransition).Ticks) + minTransition.Ticks);

                DateTime eventStart = currentSegmentStart + gap;
                float intensity = Helpers.GetRandomFloatForRange(random: this.random, minVal: 0.5f, maxVal: maxIntensity);

                this.weatherEvents.Add(new WeatherEvent(type: type, intensity: intensity, startTime: eventStart, duration: duration, transitionLength: transition));

                currentSegmentStart += gap + duration;
                if (currentSegmentStart >= endTime) break;
            }
        }

        private void UpdateCurrentIntensities()
        {
            DateTime islandDateTime = this.islandClock.IslandDateTime;

            foreach (WeatherType type in this.currentIntensityForType.Keys.ToList()) this.currentIntensityForType[type] = 0;

            foreach (WeatherEvent weatherEvent in this.weatherEvents)
            {
                this.currentIntensityForType[weatherEvent.type] = Math.Min(this.currentIntensityForType[weatherEvent.type] + weatherEvent.GetIntensity(islandDateTime), 1);
            }
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> weatherData = new Dictionary<string, object>
            {
                { "weatherEvents", this.weatherEvents },
                { "forecastEnd", this.forecastEnd },
            };

            return weatherData;
        }

        public void Deserialize(Dictionary<string, Object> weatherData)
        {
            var weatherEvents = (List<WeatherEvent>)weatherData["weatherEvents"];
            var forecastEnd = (DateTime)weatherData["forecastEnd"];

            this.weatherEvents.Clear();
            this.weatherEvents.AddRange(weatherEvents);
            this.forecastEnd = forecastEnd;
        }

        public void AddEvent(WeatherEvent weatherEvent)
        {
            this.weatherEvents.Add(weatherEvent);
        }
    }
}