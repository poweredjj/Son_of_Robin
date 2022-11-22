using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class VirtualSlot : StorageSlot
    {
        private readonly StorageSlot realSlot;

        public override int PieceCount
        { get { return this.realSlot.PieceCount; } }

        public override bool IsFull
        { get { return this.realSlot.IsFull; } }

        public override PieceTemplate.Name PieceName
        { get { return this.realSlot.PieceName; } }

        public VirtualSlot(StorageSlot realSlot, PieceStorage storage, byte posX, byte posY, byte stackLimit, List<PieceTemplate.Name> allowedPieceNames) :
            base(storage: storage, posX: posX, posY: posY, stackLimit: stackLimit, allowedPieceNames: allowedPieceNames)
        {
            this.realSlot = realSlot;
        }

        public override void AddPiece(BoardPiece piece)
        {
            this.realSlot.AddPiece(piece);
        }

        public override bool CanFitThisPiece(BoardPiece piece, int pieceCount = 1)
        {
            return this.realSlot.CanFitThisPiece(piece: piece, pieceCount: pieceCount);
        }

        public override int HowManyPiecesOfNameCanFit(PieceTemplate.Name pieceName)
        {
            return this.realSlot.HowManyPiecesOfNameCanFit(pieceName);
        }

        public override BoardPiece RemoveTopPiece()
        {
            return this.realSlot.RemoveTopPiece();
        }

        public override List<BoardPiece> GetAllPieces(bool remove)
        {
            return this.realSlot.GetAllPieces(remove);
        }

        public override void DestroyBrokenPieces()
        {
            this.realSlot.DestroyBrokenPieces();
        }

        public override void Draw(Rectangle destRect, float opacity, bool drawNewIcon)
        {
            this.realSlot.Draw(destRect: destRect, opacity: opacity, drawNewIcon: drawNewIcon);
        }

        public override void DestroyPieceAndReplaceWithAnother(BoardPiece piece)
        {
            this.realSlot.DestroyPieceAndReplaceWithAnother(piece);
        }

        public override Object Serialize()
        {
            return this.realSlot.Serialize();
        }

        public override void Deserialize(Object slotData)
        {
            this.realSlot.Deserialize(slotData);
        }
    }
}