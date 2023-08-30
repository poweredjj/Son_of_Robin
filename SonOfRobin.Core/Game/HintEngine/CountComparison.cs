using System;

namespace SonOfRobin
{
    public readonly struct CountComparison
    {
        public enum Comparison : byte
        {
            Greater,
            GreaterOrEqual,
            Equal,
            LessOrEqual,
            Less,
        }

        public readonly PieceTemplate.Name name;
        private readonly int count;
        private readonly Comparison comparison;

        public CountComparison(PieceTemplate.Name name, int count, Comparison comparison = Comparison.GreaterOrEqual)
        {
            this.name = name;
            this.count = count;
            this.comparison = comparison;
        }

        public readonly bool Check(int countCrafted)
        {
            return this.comparison switch
            {
                Comparison.Greater => countCrafted > this.count,
                Comparison.GreaterOrEqual => countCrafted >= this.count,
                Comparison.Equal => countCrafted == this.count,
                Comparison.LessOrEqual => countCrafted <= this.count,
                Comparison.Less => countCrafted < this.count,
                _ => throw new ArgumentException($"Unsupported comparison - {this.comparison}."),
            };
        }
    }
}