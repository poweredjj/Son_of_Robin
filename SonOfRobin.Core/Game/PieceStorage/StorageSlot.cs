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
        public readonly byte stackLimit;
        public string label;
        public bool locked;
        public bool hidden;
        public List<PieceTemplate.Name> allowedPieceNames;
        public BoardPiece TopPiece
        {
            get
            {
                if (this.IsEmpty) return null;
                return this.pieceList[this.pieceList.Count - 1];
            }
        }
        public int PieceCount { get { return this.pieceList.Count; } }
        public bool IsEmpty { get { return this.pieceList.Count == 0; } }
        public bool IsFull
        {
            get
            {
                if (this.pieceList.Count == 0) return false;
                else return Math.Min(this.pieceList[0].stackSize, this.stackLimit) == this.pieceList.Count;
            }
        }
        public PieceTemplate.Name PieceName { get { return pieceList[0].name; } }

        public StorageSlot(PieceStorage storage, byte posX, byte posY, byte stackLimit = 255, List<PieceTemplate.Name> allowedPieceNames = null)
        {
            this.storage = storage;
            this.posX = posX;
            this.posY = posY;
            this.pieceList = new List<BoardPiece> { };
            this.locked = false;
            this.hidden = false;
            this.label = "";
            this.stackLimit = stackLimit;
            this.allowedPieceNames = allowedPieceNames;
        }

        public void AddPiece(BoardPiece piece)
        {
            if (this.locked) return;
            Debug.Assert(this.CanFitThisPiece(piece));

            if (piece?.pieceStorage?.NotEmptySlotsCount > 0)
            { piece.pieceStorage.DropAllPiecesToTheGround(addMovement: true); }

            this.pieceList.Add(piece);
            this.AddRemoveBuffs(piece: piece, add: true);
        }

        private void AddRemoveBuffs(BoardPiece piece, bool add)
        {
            if (this.storage.storageType != PieceStorage.StorageType.Equip || piece.GetType() != typeof(Equipment)) return;

            Equipment equipPiece = (Equipment)piece;

            if (add) this.storage.storagePiece.buffEngine.AddBuffs(world: piece.world, equipPiece.buffList);
            else this.storage.storagePiece.buffEngine.RemoveBuffs(equipPiece.buffList);
        }

        public bool CanFitThisPiece(BoardPiece piece)
        {
            if (this.locked || !piece.canBePickedUp) return false;
            if (this.allowedPieceNames != null && !this.allowedPieceNames.Contains(piece.name)) return false;
            return this.IsEmpty || (piece.name == this.PieceName && this.pieceList.Count < Math.Min(this.pieceList[0].stackSize, this.stackLimit));
        }

        public BoardPiece RemoveTopPiece()
        {
            if (this.IsEmpty || this.locked) return null;

            int pieceIndex = this.pieceList.Count - 1;
            BoardPiece removedPiece = this.pieceList[pieceIndex];
            this.pieceList.RemoveAt(pieceIndex);
            this.AddRemoveBuffs(piece: removedPiece, add: false);

            return removedPiece;
        }

        public List<BoardPiece> GetAllPieces(bool remove)
        {
            if (this.locked) return new List<BoardPiece> { };

            var allPieces = new List<BoardPiece>(this.pieceList);
            if (remove)
            {
                foreach (BoardPiece piece in allPieces)
                { this.AddRemoveBuffs(piece: piece, add: false); }
                this.pieceList.Clear();
            }
            return allPieces;
        }

        public void Draw(Rectangle destRect, float opacity)
        {
            if (this.hidden || this.IsEmpty) return;
            Sprite sprite = this.TopPiece.sprite;

            sprite.UpdateAnimation();
            sprite.DrawAndKeepInRectBounds(destRect: destRect, opacity: opacity);

            if (sprite.boardPiece.hitPoints < sprite.boardPiece.maxHitPoints)
            {
                new StatBar(label: "", value: (int)sprite.boardPiece.hitPoints, valueMax: (int)sprite.boardPiece.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0),
                    posX: destRect.Center.X, posY: destRect.Y + (int)(destRect.Width * 0.8f), width: (int)(destRect.Width * 0.8f), height: (int)(destRect.Height * 0.1f));

                StatBar.FinishThisBatch();
                StatBar.DrawAll();
            }
        }
        public void DestroyBrokenPieces()
        {
            if (this.locked) return;

            var brokenPieces = this.pieceList.Where(piece => piece.hitPoints <= 0).ToList();
            foreach (BoardPiece piece in brokenPieces)
            { this.AddRemoveBuffs(piece: piece, add: false); }

            this.pieceList = this.pieceList.Where(piece => piece.hitPoints > 0).ToList();
        }

        public Object Serialize()
        {
            var pieceList = new List<Object> { };

            foreach (BoardPiece piece in this.pieceList)
            { if (piece.serialize) pieceList.Add(piece.Serialize()); }

            var slotData = new Dictionary<string, object>
            {
                { "locked", this.locked},
                { "hidden", this.hidden},
                { "label", this.label},
                {"allowedPieceNames", this.allowedPieceNames },
                {"pieceList", pieceList },
            };

            return slotData;
        }

        public void Deserialize(Object slotData)
        {
            var slotDict = (Dictionary<string, object>)slotData;

            this.locked = (bool)slotDict["locked"];
            this.hidden = (bool)slotDict["hidden"];
            this.label = (string)slotDict["label"];
            this.allowedPieceNames = (List<PieceTemplate.Name>)slotDict["allowedPieceNames"];
            var pieceListObj = (List<Object>)slotDict["pieceList"];

            // repeated in World

            foreach (Object pieceObj in pieceListObj)
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
                this.pieceList.Add(newBoardPiece);
            }
        }
    }
}
