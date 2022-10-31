using System;

namespace SonOfRobin
{
    [Serializable]
    public class AllowedRange
    {
        public byte Min { get; private set; }
        public byte Max { get; private set; }
        private bool isReadOnly;

        public AllowedRange(byte min, byte max)
        {
            this.Min = min;
            this.Max = max;
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

            this.Min = Math.Min(this.Min, expandedMin);
            this.Max = Math.Max(this.Max, expandedMax);
        }

        public bool IsInRange(byte value)
        {
            return this.Min <= value && value <= this.Max;
        }
        public AllowedRange GetRangeCopy(bool isReadOnly)
        {
            var rangeCopy = new AllowedRange(min: this.Min, max: this.Max);
            if (isReadOnly) rangeCopy.MakeReadOnly();

            return rangeCopy;
        }

    }
}
