using System;

namespace SonOfRobin
{
    [Serializable]
    public class AllowedRange
    {
        public byte Min { get; private set; }
        public byte Max { get; private set; }

        public AllowedRange(byte min, byte max)
        {
            this.Min = min;
            this.Max = max;
        }

        public void ExpandRange(byte expandedMin, byte expandedMax)
        {
            this.Min = Math.Min(this.Min, expandedMin);
            this.Max = Math.Max(this.Max, expandedMax);
        }

        public bool IsInRange(byte value)
        {
            return this.Min <= value && value <= this.Max;
        }
    }
}