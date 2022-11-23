using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AllowedDensity
    {
        private BoardPiece piece;
        private Sprite sprite;
        private readonly ushort radious;
        private readonly int maxNoOfPiecesTotal;
        private readonly int maxNoOfPiecesSameName;
        private readonly int maxNoOfPiecesSameClass;
        private readonly int maxNoOfPiecesBlocking;

        public AllowedDensity(ushort radious, int maxNoOfPiecesTotal = -1, int maxNoOfPiecesSameName = -1, int maxNoOfPiecesSameClass = -1, int maxNoOfPiecesBlocking = -1)
        {
            this.radious = radious;
            this.maxNoOfPiecesTotal = maxNoOfPiecesTotal; // -1 means no limit
            this.maxNoOfPiecesSameName = maxNoOfPiecesSameName; // -1 means no limit
            this.maxNoOfPiecesSameClass = maxNoOfPiecesSameClass; // -1 means no limit
            this.maxNoOfPiecesBlocking = maxNoOfPiecesBlocking; // -1 means no limit
        }

        public void FinishCreation(BoardPiece piece, Sprite sprite)
        {
            this.piece = piece;
            this.sprite = sprite;
        }

        public bool CanBePlacedHere()
        {
            var nearbyPieces = this.piece.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: this.sprite, distance: this.radious);

            if (this.maxNoOfPiecesTotal != -1 && nearbyPieces.Count > this.maxNoOfPiecesTotal) return false;
            if (this.maxNoOfPiecesSameName != -1 && this.CheckSameNameCount(nearbyPieces) > this.maxNoOfPiecesSameName) return false;
            if (this.maxNoOfPiecesSameClass != -1 && this.CheckSameClass(nearbyPieces) > this.maxNoOfPiecesSameClass) return false;
            if (this.maxNoOfPiecesBlocking != -1 && this.CheckBlocking(nearbyPieces) > this.maxNoOfPiecesBlocking) return false;

            return true;
        }

        private int CheckSameNameCount(List<BoardPiece> nearbyPieces)
        {
            PieceTemplate.Name name = this.piece.name;
            return nearbyPieces.Where(piece => piece.name == name).ToList().Count;
        }

        private int CheckSameClass(List<BoardPiece> nearbyPieces)
        {
            var className = this.piece.GetType();
            return nearbyPieces.Where(piece => piece.GetType() == className).ToList().Count;
        }

        private int CheckBlocking(List<BoardPiece> nearbyPieces)
        {
            return nearbyPieces.Where(piece => piece.sprite.blocksMovement).ToList().Count;
        }
    }
}