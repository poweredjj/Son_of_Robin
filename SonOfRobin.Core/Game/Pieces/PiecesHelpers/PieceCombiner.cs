using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceCombiner
    {
        private static readonly List<Combination> combinationList = new List<Combination>
        {
            new Combination(srcName1: PieceTemplate.Name.LanternEmpty, srcName2: PieceTemplate.Name.Candle, resultName: PieceTemplate.Name.LanternFull),
        };

        public class Combination
        {
            public readonly PieceTemplate.Name srcName1;
            public readonly PieceTemplate.Name srcName2;
            public readonly PieceTemplate.Name resultName;

            public Combination(PieceTemplate.Name srcName1, PieceTemplate.Name srcName2, PieceTemplate.Name resultName)
            {
                this.srcName1 = srcName1;
                this.srcName2 = srcName2;
                this.resultName = resultName;
            }

            public BoardPiece TryToCombineThesePieces(BoardPiece piece1, BoardPiece piece2)
            {
                if ((this.srcName1 == piece1.name && this.srcName2 == piece2.name) || (this.srcName1 == piece2.name && this.srcName2 == piece1.name))
                {
                    return PieceTemplate.CreatePiece(templateName: this.resultName, world: piece1.world);
                }
                else return null;
            }
        }

        public static BoardPiece TryToCombinePieces(BoardPiece piece1, BoardPiece piece2)
        {
            foreach (Combination combination in combinationList)
            {
                BoardPiece combinedPiece = combination.TryToCombineThesePieces(piece1, piece2);
                if (combinedPiece != null) return combinedPiece;
            }

            return null;
        }

        public static List<PieceTemplate.Name> CombinesWith(PieceTemplate.Name name)
        {
            var combinesWith = new List<PieceTemplate.Name>();

            foreach (Combination combination in combinationList)
            {
                PieceTemplate.Name counterpartName = PieceTemplate.Name.Empty;

                if (combination.srcName1 == name) counterpartName = combination.srcName2;
                if (combination.srcName2 == name) counterpartName = combination.srcName1;

                if (counterpartName != PieceTemplate.Name.Empty && !combinesWith.Contains(counterpartName)) combinesWith.Add(counterpartName);
            }

            return combinesWith;
        }
    }
}