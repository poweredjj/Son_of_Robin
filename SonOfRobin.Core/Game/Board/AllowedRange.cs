using System;

namespace SonOfRobin
{
    [Serializable]
    public class AllowedRange
    {
        private byte min;
        private byte max;
        private bool isReadOnly;

        public AllowedRange(byte min, byte max)
        {
            this.min = min;
            this.max = max;
            this.isReadOnly = false;
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void CheckWritePermission()
        {
            if (this.isReadOnly) throw new ArgumentException("Trying to modify read-only AllowedRange.");
        }

        public void ExpandRange(byte expandedMin, byte expandedMax)
        {
            this.CheckWritePermission();

            this.min = Math.Min(this.min, expandedMin);
            this.max = Math.Max(this.max, expandedMax);
        }

        public bool IsInRange(byte value)
        {
            return this.min <= value && value <= this.max;
        }
        public AllowedRange GetRangeCopy(bool isReadOnly)
        {
            var rangeCopy = new AllowedRange(min: this.min, max: this.max);
            if (isReadOnly) rangeCopy.MakeReadOnly();

            return rangeCopy;
        }

    }
}
