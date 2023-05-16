using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class IslandClock
    {
        public enum PartOfDay
        { Morning, Noon, Afternoon, Evening, Night }

        public static readonly Dictionary<TimeSpan, PartOfDay> partsOfDayByStartTimeAscending = new Dictionary<TimeSpan, PartOfDay>
        {
            { TimeSpan.FromHours(0), PartOfDay.Night },
            { TimeSpan.FromHours(4), PartOfDay.Morning },
            { TimeSpan.FromHours(11), PartOfDay.Noon },
            { TimeSpan.FromHours(14), PartOfDay.Evening },
            { TimeSpan.FromHours(19), PartOfDay.Night },
        }.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);

        public static readonly Dictionary<TimeSpan, PartOfDay> partsOfDayByStartTimeDescending = partsOfDayByStartTimeAscending.OrderByDescending(t => t.Key)
            .ToDictionary(t => t.Key, t => t.Value);

        public static readonly TimeSpan startTimeOffset = TimeSpan.FromHours(7); // 7 - to start the game in the morning, not at midnight
        public static readonly DateTime startingDate = new DateTime(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0) + startTimeOffset;
        public int ElapsedUpdates { get; private set; }
        private bool isPaused;
        private bool initComplete;
        public int multiplier;

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

        public static TimeSpan ConvertUpdatesCountToTimeSpan(int updatesCount)
        { return TimeSpan.FromSeconds(updatesCount * 1.5); } // * 1.5

        public TimeSpan IslandTimeElapsed
        { get { return ConvertUpdatesCountToTimeSpan(this.ElapsedUpdates) + startTimeOffset; } }

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

                foreach (var kvp in partsOfDayByStartTimeDescending)
                {
                    if (timeOfDay >= kvp.Key) return kvp.Value;
                }

                throw new ArgumentException($"Cannot calculate CurrentPartOfDay for '{this.TimeOfDay}'.");
            }
        }

        public TimeSpan TimeUntilPartOfDay(PartOfDay partOfDayToCalculate)
        {
            foreach (var kvp in partsOfDayByStartTimeAscending)
            {
                PartOfDay partOfDayToCheck = kvp.Value;

                if (partOfDayToCheck == partOfDayToCalculate)
                {
                    TimeSpan timeLeft = kvp.Key - this.TimeOfDay;
                    if (TimeSpan.Compare(timeLeft, TimeSpan.Zero) < 0) timeLeft += TimeSpan.FromHours(24);

                    return timeLeft;
                }
            }

            throw new ArgumentException($"Cannot calculate TimeUntilPartOfDay for '{partOfDayToCalculate}'.");
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
    }
}