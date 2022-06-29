using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SonOfRobin
{
    public class StorageSlot
    {
        private readonly PieceStorage storage;
        public List<BoardPiece> pieceList;
        public readonly byte posX;
        public readonly byte posY;
        private readonly byte stackLimit;
        public bool locked;


        private BoardPiece TopPiece { get { return this.pieceList[this.pieceList.Count - 1]; } }

        public int PieceCount { get { return this.pieceList.Count; } }
        public bool IsEmpty { get { return this.pieceList.Count == 0 && this.locked == false; } }
        public bool IsFull
        {
            get
            {
                if (this.locked) return true;

                if (this.pieceList.Count == 0)
                { return false; }
                else
                { return Math.Min(this.pieceList[0].stackSize, this.stackLimit) == this.pieceList.Count; }
            }
        }

        public bool HasSpaceLeft
        {
            get
            {
                if (this.locked) return false;
                return this.pieceList.Count == 0 || this.pieceList.Count < Math.Min(this.pieceList[0].stackSize, this.stackLimit);
            }
        }

        public PieceTemplate.Name PieceName { get { return pieceList[0].name; } }

        public StorageSlot(PieceStorage storage, byte posX, byte posY, byte stackLimit = 255)
        {
            this.storage = storage;
            this.posX = posX;
            this.posY = posY;
            this.pieceList = new List<BoardPiece> { };
            this.locked = false;
            this.stackLimit = stackLimit;
        }

        public void AddPiece(BoardPiece piece)
        {
            if (this.locked) return;
            Debug.Assert(this.CanFitThisPiece(piece));
            this.pieceList.Add(piece);
        }
        public bool CanFitThisPiece(BoardPiece piece)
        {
            if (this.locked) return false;
            if (piece.GetType() == typeof(Container) && this.storage.storageType == PieceStorage.StorageType.Chest) return false;
            return this.IsEmpty || (piece.name == this.PieceName && this.pieceList.Count < Math.Min(this.pieceList[0].stackSize, this.stackLimit));
        }

        public BoardPiece RemoveTopPiece()
        {
            if (this.IsEmpty || this.locked) return null;

            int pieceIndex = this.pieceList.Count - 1;
            BoardPiece removedPiece = this.pieceList[pieceIndex];
            this.pieceList.RemoveAt(pieceIndex);

            return removedPiece;
        }

        public BoardPiece GetTopPiece()
        {
            if (this.IsEmpty) return null;

            int pieceIndex = this.pieceList.Count - 1;
            return this.pieceList[pieceIndex];
        }

        public List<BoardPiece> GetAllPieces(bool remove)
        {
            if (this.locked) return new List<BoardPiece> { };

            var removedPieces = new List<BoardPiece>(this.pieceList);
            if (remove) this.pieceList.Clear();
            return removedPieces;
        }

        public void Draw(Rectangle destRect, float opacity)
        {
            if (this.IsEmpty) return;
            Sprite sprite = this.TopPiece.sprite;

            sprite.Draw(destRect: destRect, opacity: opacity);

            if (sprite.boardPiece.hitPoints < sprite.boardPiece.maxHitPoints)
            {
                new StatBar(label: "", value: (int)sprite.boardPiece.hitPoints, valueMax: (int)sprite.boardPiece.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0),
                    posX: destRect.Center.X, posY: destRect.Y + (int)(destRect.Width * 0.8f), width: (int)(destRect.Width * 0.8f), height: (int)(destRect.Height * 0.1f));

                StatBar.FinishThisBatch();
                StatBar.DrawAll();
            }

        }

        public List<Object> Serialize()
        {
            var slotData = new List<Object> { };

            foreach (BoardPiece piece in this.pieceList)
            { slotData.Add(piece.Serialize()); }

            return slotData;
        }

        public void DestroyBrokenPieces()
        {
            if (this.locked) return;
            this.pieceList = this.pieceList.Where(piece => piece.hitPoints > 0).ToList();
        }

        public void Deserialize(Object slotData)
        {
            // repeated in World

            var slotList = (List<Object>)slotData;

            foreach (Object pieceObj in slotList)
            {
                var pieceData = (Dictionary<string, Object>)pieceObj;

                PieceTemplate.Name templateName = (PieceTemplate.Name)pieceData["base_name"];

                bool female = false;
                bool randomSex = true;

                if (pieceData.ContainsKey("animal_female"))
                {
                    randomSex = false;
                    female = (bool)pieceData["animal_female"];
                }

                var newBoardPiece = PieceTemplate.CreateOffBoard(world: this.storage.world, templateName: templateName, randomSex: randomSex, female: female);
                newBoardPiece.Deserialize(pieceData);
                pieceList.Add(newBoardPiece);
            }
        }
    }
}
