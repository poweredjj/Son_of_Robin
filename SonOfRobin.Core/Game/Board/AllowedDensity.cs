using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AllowedDensity
    {
        private readonly ushort radius;
        private readonly int maxNoOfPiecesTotal;
        private readonly int maxNoOfPiecesSameName;
        private readonly int maxNoOfPiecesSameClass;
        private readonly int maxNoOfPiecesBlocking;
        private readonly bool forbidOverlapSameClass;

        public AllowedDensity(ushort radius = 0, int maxNoOfPiecesTotal = -1, int maxNoOfPiecesSameName = -1, int maxNoOfPiecesSameClass = -1, int maxNoOfPiecesBlocking = -1, bool forbidOverlapSameClass = false)
        {
            this.radius = radius;
            this.maxNoOfPiecesTotal = maxNoOfPiecesTotal; // -1 means no limit
            this.maxNoOfPiecesSameName = maxNoOfPiecesSameName; // -1 means no limit
            this.maxNoOfPiecesSameClass = maxNoOfPiecesSameClass; // -1 means no limit
            this.maxNoOfPiecesBlocking = maxNoOfPiecesBlocking; // -1 means no limit
            this.forbidOverlapSameClass = forbidOverlapSameClass;
        }

        public bool CanBePlacedHere(BoardPiece piece)
        {
            var nearbyPieces = piece.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: piece.sprite, distance: this.radius);

            if (this.maxNoOfPiecesTotal != -1 && nearbyPieces.Count > this.maxNoOfPiecesTotal) return false;
            if (this.maxNoOfPiecesSameName != -1 && CheckSameNameCount(piece: piece, nearbyPieces: nearbyPieces) > this.maxNoOfPiecesSameName) return false;
            if (this.maxNoOfPiecesSameClass != -1 && CheckSameClass(piece: piece, nearbyPieces: nearbyPieces) > this.maxNoOfPiecesSameClass) return false;
            if (this.maxNoOfPiecesBlocking != -1 && CheckBlocking(piece: piece, nearbyPieces: nearbyPieces) > this.maxNoOfPiecesBlocking) return false;
            if (this.forbidOverlapSameClass && CheckOverlapSameClass(piece: piece)) return false;

            return true;
        }

        private static int CheckSameNameCount(BoardPiece piece, IEnumerable<BoardPiece> nearbyPieces)
        {
            PieceTemplate.Name name = piece.name;
            return nearbyPieces.Where(checkedPiece => checkedPiece.name == name && checkedPiece != piece).Count();
        }

        private static int CheckSameClass(BoardPiece piece, IEnumerable<BoardPiece> nearbyPieces)
        {
            Type type = piece.GetType();
            return nearbyPieces.Where(checkedPiece => checkedPiece.GetType() == type && checkedPiece != piece).Count();
        }

        private static int CheckBlocking(BoardPiece piece, IEnumerable<BoardPiece> nearbyPieces)
        {
            return nearbyPieces.Where(checkedPiece => checkedPiece.sprite.BlocksMovement && checkedPiece != piece).Count();
        }

        private static bool CheckOverlapSameClass(BoardPiece piece)
        {
            Type type = piece.GetType();

            foreach (Sprite sprite in piece.sprite.GetCollidingSprites(new List<Cell.Group> { Cell.Group.Visible }))
            {
                if (sprite.boardPiece.GetType() == type) return true;
            }
            return false;
        }
    }
}