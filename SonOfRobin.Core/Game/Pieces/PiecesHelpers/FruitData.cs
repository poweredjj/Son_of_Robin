using System;
using System.Collections.Generic;
using System.Text;

namespace SonOfRobin
{
    public class FruitData
    {
        public readonly byte maxNumber;
        public readonly float oneFruitMass;
        public readonly float yOffsetPercent; // -1 to 1
        public readonly float xOffsetPercent; // -1 to 1
        public readonly float areaWidthPercent; // 0 to 1
        public readonly float areaHeightPercent; // 0 to 1
        public readonly PieceTemplate.Name fruitName;

        public FruitData(byte maxNumber, float oneFruitMass, PieceTemplate.Name fruitName, float xOffsetPercent = 0, float yOffsetPercent = 0, float areaWidthPercent = 1f, float areaHeightPercent = 1f)
        {
            this.maxNumber = maxNumber;
            this.oneFruitMass = oneFruitMass;
            this.xOffsetPercent = xOffsetPercent;
            this.yOffsetPercent = yOffsetPercent;
            this.areaWidthPercent = areaWidthPercent;
            this.areaHeightPercent = areaHeightPercent;
            this.fruitName = fruitName;
        }

    }
}
