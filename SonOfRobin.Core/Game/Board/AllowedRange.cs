using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    public class AllowedRange
    {
        private byte min;
        private byte max;

        public AllowedRange(byte min, byte max)
        {
            this.min = min;
            this.max = max;
        }

        public void ExpandRange(byte expandedMin, byte expandedMax)
        {
            this.min = Math.Min(this.min, expandedMin);
            this.max = Math.Max(this.max, expandedMax);
        }

        public bool IsInRange(byte value)
        { return this.min <= value && value <= this.max; }

    }
}
