using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class IslandClock
    {
        public enum PartOfDay
        { Morning, Noon, Afternoon, Evening, Night }

        public static readonly Dictionary<int, PartOfDay> partsOfDayByEndingTime = new Dictionary<int, PartOfDay>
        {
            // must be sorted from lowest to highest values
            { 4, PartOfDay.Night },
            { 11, PartOfDay.Morning },
            { 14, PartOfDay.Noon },
            { 19, PartOfDay.Evening },
            { 24, PartOfDay.Night },
        };

        public static readonly TimeSpan startTimeOffset = TimeSpan.FromHours(7); // 7 - to start the game in the morning, not at midnight
        public static readonly DateTime startingDate = new DateTime(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0) + startTimeOffset;
        public int ElapsedUpdates { get; private set; }
        private bool isPaused;
        private bool initComplete;
        public int multiplier;
        public TimeSpan IslandTimeElapsed
        { get { return TimeSpan.FromSeconds(this.ElapsedUpdates * 1.5) + startTimeOffset; } }  // * 1.5
        public DateTime IslandDateTime
        { get { return startingDate + this.IslandTimeElapsed - startTimeOffset; } }
        public TimeSpan TimeOfDay
        { get { return this.IslandDateTime.TimeOfDay; } }

        public int CurrentDayNo
        { get { return (int)this.IslandTimeElapsed.TotalDays + 1; } }

        public PartOfDay CurrentPartOfDay
        {
            get
            {
                TimeSpan timeOfDay = this.TimeOfDay;

                foreach (var kvp in partsOfDayByEndingTime)
                {
                    if (timeOfDay.Hours < kvp.Key) return kvp.Value;
                }

                throw new ArgumentException($"Cannot calculate CurrentPartOfDay for '{this.TimeOfDay}'.");
            }
        }

        public string CurrentPartOfDaySentence
        {
            get
            {
                PartOfDay currentPartOfDay = this.CurrentPartOfDay;

                switch (currentPartOfDay)
                {
                    case PartOfDay.Morning:
                        return "in the morning";

                    case PartOfDay.Noon:
                        return "at noon";

                    case PartOfDay.Afternoon:
                        return "in the afternoon";

                    case PartOfDay.Evening:
                        return "in the evening";

                    case PartOfDay.Night:
                        return "at night";

                    default:
                        throw new ArgumentException($"Unsupported PartOfDay - '{currentPartOfDay}'.");
                }
            }
        }

        public IslandClock(int elapsedUpdates = -1)
        {
            if (elapsedUpdates < -1) throw new ArgumentException($"elapsedUpdates ({elapsedUpdates}) cannot be < 0."); // -1 value is used as a "not initialized" value

            if (elapsedUpdates == -1)
            {
                this.initComplete = false;
            }
            else
            {
                this.ElapsedUpdates = elapsedUpdates;
                this.initComplete = true;
            }

            this.multiplier = 1;
            this.isPaused = false;
        }

        public void Initialize(int elapsedUpdates)
        {
            if (this.initComplete) throw new ArgumentException($"Island clock has already been initialized - value {this.ElapsedUpdates}.");

            this.ElapsedUpdates = elapsedUpdates;
            this.initComplete = true;
        }

        public void Pause()
        {
            this.isPaused = true;
        }

        public void Resume()
        {
            this.isPaused = false;
        }

        public void Advance(int amount = 1, bool ignorePause = false, bool ignoreMultiplier = false)
        {
            if (amount < 1) throw new ArgumentException($"Advance value ({amount}) has to be >= 1.");
            if (!this.initComplete) throw new ArgumentException("Island Clock has not been initialized.");

            if (ignorePause || !this.isPaused)
            {
                if (ignoreMultiplier) this.ElapsedUpdates += amount;
                else this.ElapsedUpdates += amount * this.multiplier;
            }
        }
    }
}