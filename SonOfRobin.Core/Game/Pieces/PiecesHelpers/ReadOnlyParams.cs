namespace SonOfRobin
{
    public struct ReadOnlyParams
    {
        public readonly bool serialize;
        public readonly float fireAffinity;
        public readonly int[] maxMassForSize;
        public readonly float adultSizeMass;
        public readonly bool movesWhenDropped;

        public ReadOnlyParams(bool serialize = true, float fireAffinity = 0f, int[] maxMassForSize = null, float adultSizeMass = 0f, bool movesWhenDropped = true)
        {
            this.serialize = serialize;
            this.fireAffinity = fireAffinity;
            this.maxMassForSize = maxMassForSize;
            this.adultSizeMass = adultSizeMass; // AdultSizeMass should be greater than animSize for sapling (to avoid showing fruits on sapling)
            this.movesWhenDropped = movesWhenDropped;
        }
    }
}