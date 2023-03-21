using System;
using System.Collections.Generic;

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
        }

        public float GetIntensity(DateTime datetime)
        {
            if (datetime < startTime || datetime > endTime) return 0;

            TimeSpan elapsedTime = datetime - startTime;
            TimeSpan timeInTransition = TimeSpan.FromSeconds(Math.Max(0, (elapsedTime - this.duration).TotalSeconds + transitionLength.TotalSeconds));
            float transitionProgress = (float)timeInTransition.TotalSeconds / (float)transitionLength.TotalSeconds;

            if (transitionProgress < 0 || transitionProgress > 1) throw new InvalidOperationException("Invalid transition progress.");

            return Math.Min(1f, transitionProgress) * intensity;
        }
    }

    public class Weather
    {
        public enum WeatherType
        { Wind, Clouds, Rain }

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

            this.Update();
        }

        public float GetIntensityForWeatherType(WeatherType type)
        {
            return this.currentIntensityForType[type];
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

        private void AddNewWeatherEvents(WeatherType type, DateTime startTime, DateTime endTime)
        {


        }

        public void Update()
        {
            DateTime islandDateTime = this.islandClock.IslandDateTime;
            weatherEvents.RemoveAll(e => e.endTime < islandDateTime);

            this.UpdateCurrentIntensities(islandDateTime);

            if (this.forecastEnd > islandDateTime + minForecastDuration) return;

            // Generate new weather events
            DateTime eventStartTime = islandDateTime + minForecastDuration;
            DateTime eventEndTime = islandDateTime + maxForecastDuration;

            while (eventEndTime < islandDateTime + maxForecastDuration)
            {
                //WeatherType eventType = WeatherType.Clouds;
                //float intensity = (float)this.random.NextDouble();

                //// Increase wind probability when it is cloudy
                //if (eventType == WeatherType.Clouds)
                //{
                //    if (this.random.NextDouble() < 0.7)
                //    {
                //        eventType = WeatherType.Wind;
                //    }
                //}

                //// Add new event type "Rain"
                //if (eventType == WeatherType.Clouds)
                //{
                //    if (this.random.NextDouble() < 0.3)
                //    {
                //        eventType = WeatherType.Rain;
                //    }
                //}

                //// Make sure it only rains when cloudy
                //if (eventType == WeatherType.Rain)
                //{
                //    intensity = (float)this.random.NextDouble() * 0.5f;
                //}

                //TimeSpan duration = TimeSpan.FromHours(this.random.Next(6, 24));
                //TimeSpan transitionLength = TimeSpan.FromMinutes(this.random.Next(5, 30));

                //WeatherEvent newEvent = new WeatherEvent(eventType, intensity, eventStartTime, duration, transitionLength);
                //this.weatherEvents.Add(newEvent);

                //eventStartTime = eventEndTime;
                //eventEndTime = eventStartTime.Add(duration).Add(transitionLength);
            }

            this.forecastEnd = eventEndTime;
        }

        private void UpdateCurrentIntensities(DateTime dateTime)
        {
            foreach (WeatherType type in this.currentIntensityForType.Keys) this.currentIntensityForType[type] = 0;

            foreach (WeatherEvent weatherEvent in this.weatherEvents)
            {
                this.currentIntensityForType[weatherEvent.type] = Math.Min(this.currentIntensityForType[weatherEvent.type] + weatherEvent.GetIntensity(dateTime), 1);
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

            this.weatherEvents.AddRange(weatherEvents);
            this.forecastEnd = forecastEnd;
        }
    }
}