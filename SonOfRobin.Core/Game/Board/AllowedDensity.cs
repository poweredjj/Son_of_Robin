using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AllowedDensity
    {
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

        public bool CanBePlacedHere(BoardPiece piece)
        {
            var nearbyPieces = piece.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: piece.sprite, distance: this.radious);

            if (this.maxNoOfPiecesTotal != -1 && nearbyPieces.Count() > this.maxNoOfPiecesTotal) return false;
            if (this.maxNoOfPiecesSameName != -1 && CheckSameNameCount(name: piece.name, nearbyPieces: nearbyPieces) > this.maxNoOfPiecesSameName) return false;
            if (this.maxNoOfPiecesSameClass != -1 && CheckSameClass(type: piece.GetType(), nearbyPieces: nearbyPieces) > this.maxNoOfPiecesSameClass) return false;
            if (this.maxNoOfPiecesBlocking != -1 && CheckBlocking(nearbyPieces) > this.maxNoOfPiecesBlocking) return false;

            return true;
        }

        private static int CheckSameNameCount(PieceTemplate.Name name, IEnumerable<BoardPiece> nearbyPieces)
        {
            return nearbyPieces.Where(piece => piece.name == name).Count();
        }

        private static int CheckSameClass(Type type, IEnumerable<BoardPiece> nearbyPieces)
        {
            return nearbyPieces.Where(piece => piece.GetType() == type).Count();
        }

        private static int CheckBlocking(IEnumerable<BoardPiece> nearbyPieces)
        {
            return nearbyPieces.Where(piece => piece.sprite.BlocksMovement).Count();
        }
    }
}