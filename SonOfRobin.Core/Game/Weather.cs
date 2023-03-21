using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Weather
    {
        [Serializable]
        public struct Event
        {
            public enum Type
            { Wind, Clouds }

            public readonly Type type;
            public readonly float intensity;
            public readonly DateTime startTime;
            public readonly DateTime endTime;
            public readonly TimeSpan duration;
            public readonly TimeSpan transitionLength;

            public Event(Type type, float intensity, DateTime startTime, TimeSpan duration, TimeSpan transitionLength)
            {
                if (transitionLength + transitionLength >= duration) throw new ArgumentException("Transitions are longer than duration.");

                this.type = type;
                this.intensity = intensity;
                this.startTime = startTime;
                this.duration = duration;
                this.endTime = startTime + duration;
                this.transitionLength = transitionLength;
            }

            public float GetIntensity(DateTime datetime)
            {
                if (datetime < startTime || datetime > endTime)
                    throw new ArgumentOutOfRangeException(nameof(datetime), "Datetime is outside of the event timeframe.");

                TimeSpan elapsedTime = datetime - startTime;
                TimeSpan timeInTransition = TimeSpan.FromSeconds(Math.Max(0, (elapsedTime - duration).TotalSeconds + transitionLength.TotalSeconds));
                float transitionProgress = (float)timeInTransition.TotalSeconds / (float)transitionLength.TotalSeconds;

                if (transitionProgress < 0 || transitionProgress > 1) throw new InvalidOperationException("Invalid transition progress.");
                if (intensity < 0) throw new InvalidOperationException("Invalid intensity.");
                if (duration == TimeSpan.Zero) throw new InvalidOperationException("Duration must be positive.");

                return Math.Min(1f, transitionProgress) * intensity;
            }
        }

        private static readonly TimeSpan minForecastDuration = TimeSpan.FromDays(1);

        private readonly IslandClock islandClock;
        private readonly List<Event> weatherEvents;
        private DateTime forecastEnd;

        public Weather(IslandClock islandClock)
        {
            this.islandClock = islandClock;
            this.weatherEvents = new List<Event>();
            this.forecastEnd = new DateTime(1900, 1, 1); // to ensure first update
            this.Update();
        }

        public void Update()
        {
            DateTime islandDateTime = this.islandClock.IslandDateTime;
            weatherEvents.RemoveAll(e => e.endTime < islandDateTime);

            if (this.forecastEnd < islandDateTime + minForecastDuration)
            {
                // TODO add code
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
            var weatherEvents = (List<Event>)weatherData["weatherEvents"];
            var forecastEnd = (DateTime)weatherData["forecastEnd"];

            this.weatherEvents.AddRange(weatherEvents);
            this.forecastEnd = forecastEnd;
        }
    }
}