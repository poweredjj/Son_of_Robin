using System;

namespace SonOfRobin
{
    public struct CountComparison
    {
        public enum Comparison
        { Greater, GreaterOrEqual, Equal, LessOrEqual, Less }

        public readonly PieceTemplate.Name name;
        private readonly int count;
        private readonly Comparison comparison;

        public CountComparison(PieceTemplate.Name name, int count, Comparison comparison = Comparison.GreaterOrEqual)
        {
            this.name = name;
            this.count = count;
            this.comparison = comparison;
        }

        public bool Check(int countCrafted)
        {
            switch (this.comparison)
            {
                case Comparison.Greater:
                    return countCrafted > this.count;

                case Comparison.GreaterOrEqual:
                    return countCrafted >= this.count;

                case Comparison.Equal:
                    return countCrafted == this.count;

                case Comparison.LessOrEqual:
                    return countCrafted <= this.count;

                case Comparison.Less:
                    return countCrafted < this.count;

                default:
                    throw new ArgumentException($"Unsupported comparison - {this.comparison}.");
            }
        }
    }
}