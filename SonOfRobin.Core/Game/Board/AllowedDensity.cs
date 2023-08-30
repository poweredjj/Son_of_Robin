using System;
using System.Collections.Generic;

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
            int nearbyPieceCount = nearbyPieces.Count;

            if (this.maxNoOfPiecesTotal != -1 && nearbyPieceCount > this.maxNoOfPiecesTotal) return false;
            if (this.maxNoOfPiecesSameName != -1 && !CheckSameNameCount(piece: piece, nearbyPieces: nearbyPieces, maxCount: this.maxNoOfPiecesSameName)) return false;
            if (this.maxNoOfPiecesSameClass != -1 && !CheckSameClass(piece: piece, nearbyPieces: nearbyPieces, maxCount: this.maxNoOfPiecesSameClass)) return false;
            if (this.maxNoOfPiecesBlocking != -1 && !CheckBlocking(nearbyPieces: nearbyPieces, maxCount: this.maxNoOfPiecesBlocking)) return false;
            if (this.forbidOverlapSameClass && CheckOverlapSameClass(piece: piece)) return false;

            return true;
        }

        private static bool CheckSameNameCount(BoardPiece piece, List<BoardPiece> nearbyPieces, int maxCount)
        {
            int pieceCounter = 0;
            foreach (BoardPiece nearbyPiece in nearbyPieces)
            {
                if (nearbyPiece.name == piece.name)
                {
                    pieceCounter++;
                    if (pieceCounter > maxCount) return false;
                }
            }

            return true;
        }

        private static bool CheckSameClass(BoardPiece piece, List<BoardPiece> nearbyPieces, int maxCount)
        {
            Type type = piece.GetType();

            int pieceCounter = 0;
            foreach (BoardPiece nearbyPiece in nearbyPieces)
            {
                if (nearbyPiece.GetType() == type)
                {
                    pieceCounter++;
                    if (pieceCounter > maxCount) return false;
                }
            }

            return true;
        }

        private static bool CheckBlocking(List<BoardPiece> nearbyPieces, int maxCount)
        {
            int pieceCounter = 0;
            foreach (BoardPiece nearbyPiece in nearbyPieces)
            {
                if (nearbyPiece.sprite.BlocksMovement) pieceCounter++;
                {
                    if (pieceCounter > maxCount) return false;
                }
            }

            return true;
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