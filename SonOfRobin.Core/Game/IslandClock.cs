using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class IslandClock
    {
        public enum PartOfDay { Morning, Noon, Afternoon, Evening, Night }

        public static readonly Dictionary<int, PartOfDay> partsOfDayByEndingTime = new Dictionary<int, PartOfDay>
        {
            // must be sorted from lowest to highest values
            {4, PartOfDay.Night},
            {11, PartOfDay.Morning},
            {14, PartOfDay.Noon},
            {19, PartOfDay.Evening},
            {24, PartOfDay.Night},
        };

        public static readonly TimeSpan startTimeOffset = TimeSpan.FromHours(1); // 7 - to start game in the morning, not at midnight
        public static readonly DateTime startingDate = new DateTime(year: 2020, month: 1, day: 1, hour: 0, minute: 0, second: 0) + startTimeOffset;
        private readonly World world;
        private readonly int frozenUpdate;
        private int CurrentUpdate { get { return this.world != null ? this.world.currentUpdate : this.frozenUpdate; } }
        public TimeSpan IslandTimeElapsed
        { get { return TimeSpan.FromSeconds(this.CurrentUpdate / 3) + startTimeOffset; } }
        public DateTime IslandDateTime
        { get { return startingDate + this.IslandTimeElapsed - startTimeOffset; } }

        public TimeSpan TimeOfDay { get { return this.IslandDateTime.TimeOfDay; } }
        public int CurrentDayNo
        { get { return (int)this.IslandTimeElapsed.TotalDays + 1; } }
        public PartOfDay CurrentPartOfDay
        {
            get
            {
                TimeSpan timeOfDay = this.TimeOfDay;

                foreach (var kvp in partsOfDayByEndingTime)
                { if (timeOfDay.Hours < kvp.Key) return kvp.Value; }

                throw new ArgumentException($"Cannot calculate CurrentPartOfDay for '{this.TimeOfDay}'.");
            }
        }

        public IslandClock(World world)
        {
            // Normal use - for continous time tracking.
            this.world = world;
        }
        public IslandClock(int frozenUpdate)
        {
            // Alternative use - to convert a single currentUpdate value.
            this.frozenUpdate = frozenUpdate;
        }
    }
}
