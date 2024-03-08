using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class IslandClock
    {
        public enum PartOfDay : byte
        {
            Morning,
            Noon,
            Afternoon,
            Evening,
            Night,
        }

        private const double timeConversionFactor = 1.5;

        public static readonly TimeSpan startTimeOffset = TimeSpan.FromHours(7); // 7 - to start the game in the morning, not at midnight
        public static readonly DateTime startingDate = new DateTime(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0) + startTimeOffset;
        public int ElapsedUpdates { get; private set; }
        public bool IsPaused { get { return this.isPaused; } }
        private bool isPaused;
        private bool initComplete;
        private readonly World world;

        public IslandClock(int elapsedUpdates = -1, World world = null)
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

            this.isPaused = false;
            this.world = world;
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

        public void Advance(int amount = 1, bool ignorePause = false, bool advanceWorldCurrentUpdate = true)
        {
            if (amount < 1) throw new ArgumentException($"Advance value ({amount}) has to be >= 1.");
            if (!this.initComplete) throw new ArgumentException("Island Clock has not been initialized.");

            this.world.forceRenderNextFrame = true; // to ensure that world will be rendered (if invoked outside Update())

            if (ignorePause || !this.isPaused)
            {
                this.ElapsedUpdates += amount;
                if (advanceWorldCurrentUpdate && this.world != null) this.world.CurrentUpdate += amount;
            }
        }

        public static TimeSpan ConvertUpdatesCountToTimeSpan(int updatesCount)
        { return TimeSpan.FromSeconds(updatesCount * timeConversionFactor); }

        public static int ConvertTimeSpanToUpdates(TimeSpan timeSpan)
        { return (int)(timeSpan.TotalSeconds / timeConversionFactor); }

        public TimeSpan IslandTimeElapsed
        { get { return ConvertUpdatesCountToTimeSpan(this.ElapsedUpdates) + startTimeOffset; } }

        public DateTime IslandDateTime
        { get { return startingDate + this.IslandTimeElapsed - startTimeOffset; } }

        public TimeSpan TimeOfDay
        { get { return this.IslandDateTime.TimeOfDay; } }

        public int CurrentDayNo
        { get { return (int)this.IslandTimeElapsed.TotalDays + 1; } }

        public PartOfDay CurrentPartOfDay
        { get { return GetPartOfDayForDateTime(this.IslandDateTime); } }

        public static PartOfDay GetPartOfDayForDateTime(DateTime dateTime)
        { return PartOfDayDefinition.GetPartOfDayForDateTime(dateTime); }

        public static TimeSpan GetTimeUntilPartOfDayForDateTime(DateTime dateTime, PartOfDay partOfDayToCalculate)
        { return PartOfDayDefinition.GetTimeUntilPartOfDay(dateTime: dateTime, partOfDayToCalculate: partOfDayToCalculate); }

        public TimeSpan TimeUntilPartOfDay(PartOfDay partOfDayToCalculate)
        { return PartOfDayDefinition.GetTimeUntilPartOfDay(dateTime: this.IslandDateTime, partOfDayToCalculate: partOfDayToCalculate); }

        public string CurrentPartOfDaySentence
        {
            get
            {
                PartOfDay currentPartOfDay = this.CurrentPartOfDay;

                return currentPartOfDay switch
                {
                    PartOfDay.Morning => "in the morning",
                    PartOfDay.Noon => "at noon",
                    PartOfDay.Afternoon => "in the afternoon",
                    PartOfDay.Evening => "in the evening",
                    PartOfDay.Night => "at night",
                    _ => throw new ArgumentException($"Unsupported PartOfDay - '{currentPartOfDay}'."),
                };
            }
        }

        public readonly struct PartOfDayDefinition
        {
            private static readonly List<PartOfDayDefinition> allDefinitions = new List<PartOfDayDefinition> {
                new PartOfDayDefinition(partOfDay: PartOfDay.Night, startTime: TimeSpan.FromHours(0), endTime: TimeSpan.FromHours(4)),
                new PartOfDayDefinition(partOfDay: PartOfDay.Morning, startTime: TimeSpan.FromHours(4), endTime: TimeSpan.FromHours(11)),
                new PartOfDayDefinition(partOfDay: PartOfDay.Noon, startTime: TimeSpan.FromHours(11), endTime: TimeSpan.FromHours(13)),
                new PartOfDayDefinition(partOfDay: PartOfDay.Afternoon, startTime: TimeSpan.FromHours(13), endTime: TimeSpan.FromHours(16)),
                new PartOfDayDefinition(partOfDay: PartOfDay.Evening, startTime: TimeSpan.FromHours(16), endTime: TimeSpan.FromHours(19)),
                new PartOfDayDefinition(partOfDay: PartOfDay.Night, startTime: TimeSpan.FromHours(19), endTime: TimeSpan.FromHours(24)),
            }.OrderByDescending(t => t.startTime).ToList();

            public readonly PartOfDay partOfDay;
            public readonly TimeSpan startTime;
            public readonly TimeSpan endTime;

            public PartOfDayDefinition(PartOfDay partOfDay, TimeSpan startTime, TimeSpan endTime)
            {
                this.partOfDay = partOfDay;
                this.startTime = startTime;
                this.endTime = endTime;
            }

            private bool ContainsThisTimeOfDay(TimeSpan timeOfDay)
            {
                return timeOfDay >= this.startTime && timeOfDay < this.endTime;
            }

            public static PartOfDay GetPartOfDayForDateTime(DateTime dateTime)
            {
                TimeSpan timeOfDay = dateTime.TimeOfDay;

                foreach (PartOfDayDefinition definition in allDefinitions)
                {
                    if (definition.ContainsThisTimeOfDay(timeOfDay)) return definition.partOfDay;
                }

                throw new ArgumentException($"Cannot calculate PartOfDay for '{dateTime}'.");
            }

            public static TimeSpan GetTimeUntilPartOfDay(DateTime dateTime, PartOfDay partOfDayToCalculate)
            {
                TimeSpan timeOfDay = dateTime.TimeOfDay;

                foreach (int hoursToAdd in new List<int> { 0, 24 })
                {
                    foreach (PartOfDayDefinition definition in allDefinitions)
                    {
                        if (definition.partOfDay == partOfDayToCalculate)
                        {
                            TimeSpan startTime = definition.startTime + TimeSpan.FromHours(hoursToAdd);
                            if (startTime > timeOfDay) return startTime - timeOfDay;
                        }
                    }
                }

                throw new ArgumentException($"Cannot calculate TimeUntilPartOfDay for '{dateTime}'.");
            }
        }
    }
}