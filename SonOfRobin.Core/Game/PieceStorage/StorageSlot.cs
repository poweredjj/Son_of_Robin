using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class StorageSlot
    {
        public readonly PieceStorage storage;
        public List<BoardPiece> pieceList;
        public readonly byte posX;
        public readonly byte posY;
        public byte stackLimit;
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

        public int PieceCount
        { get { return this.pieceList.Count; } }

        public bool IsEmpty
        { get { return this.PieceCount == 0; } }

        public bool IsFull
        {
            get
            {
                if (this.pieceList.Count == 0) return false;
                else return Math.Min(this.pieceList[0].stackSize, this.stackLimit) == this.pieceList.Count;
            }
        }

        public PieceTemplate.Name PieceName
        { get { return pieceList[0].name; } }

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

            if (!this.CanFitThisPiece(piece)) throw new ArgumentException($"This piece '{piece.name}' cannot fit in this slot.");

            if (piece?.pieceStorage?.OccupiedSlotsCount > 0)
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

        public bool CanFitThisPiece(BoardPiece piece, int pieceCount = 1)
        {
            if (this.locked || !piece.canBePickedUp) return false;
            if (this.allowedPieceNames != null && !this.allowedPieceNames.Contains(piece.name)) return false;

            if (this.IsEmpty) return pieceCount <= Math.Min(piece.stackSize, this.stackLimit);
            else return piece.name == this.PieceName && this.pieceList.Count + pieceCount <= Math.Min(this.pieceList[0].stackSize, this.stackLimit);
        }

        public int HowManyPiecesOfNameCanFit(PieceTemplate.Name pieceName)
        {
            PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

            if (this.locked || !pieceInfo.canBePickedUp) return 0;
            if (this.allowedPieceNames != null && !this.allowedPieceNames.Contains(pieceName)) return 0;
            if (this.IsEmpty) return Math.Min(pieceInfo.stackSize, this.stackLimit);
            if (pieceName == this.PieceName) return Math.Min(this.pieceList[0].stackSize, this.stackLimit) - this.pieceList.Count;

            return 0;
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

        public void DestroyBrokenPieces()
        {
            if (this.locked) return;

            var brokenPieces = this.pieceList.Where(piece => piece.hitPoints <= 0).ToList();
            foreach (BoardPiece piece in brokenPieces)
            { this.AddRemoveBuffs(piece: piece, add: false); }

            this.pieceList = this.pieceList.Where(piece => piece.hitPoints > 0).ToList();
        }

        public void DestroyPieceAndReplaceWithAnother(BoardPiece piece)
        {
            // target piece stack size should be 1; otherwise it makes no sense
            if (this.pieceList.Count > 1) throw new ArgumentException($"Cannot replace {this.storage.storageType} slot (current stack size {this.pieceList.Count}) contents with {piece.name}.");

            this.pieceList.Clear();
            this.pieceList.Add(piece);
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
                { "allowedPieceNames", this.allowedPieceNames },
                { "stackLimit", this.stackLimit },
                { "pieceList", pieceList },
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
            this.stackLimit = (byte)slotDict["stackLimit"];
            var pieceListObj = (List<Object>)slotDict["pieceList"];

            // repeated in World

            foreach (Object pieceObj in pieceListObj)
            {
                var pieceData = (Dictionary<string, Object>)pieceObj;

                PieceTemplate.Name templateName = (PieceTemplate.Name)pieceData["base_name"];

                bool female = false;
                bool randomSex = true;

                if (pieceData.ContainsKey("base_female"))
                {
                    randomSex = false;
                    female = (bool)pieceData["base_female"];
                }

                var newBoardPiece = PieceTemplate.Create(world: this.storage.world, templateName: templateName, randomSex: randomSex, female: female);
                newBoardPiece.Deserialize(pieceData);
                this.pieceList.Add(newBoardPiece);
            }
        }

        public void Draw(Rectangle destRect, float opacity, bool drawNewIcon)
        {
            if (this.hidden || this.IsEmpty) return;
            Sprite sprite = this.TopPiece.sprite;
            BoardPiece piece = sprite.boardPiece;
            World world = piece.world;

            sprite.UpdateAnimation();
            sprite.DrawAndKeepInRectBounds(destRect: destRect, opacity: opacity);

            if (piece.hitPoints < piece.maxHitPoints)
            {
                new StatBar(label: "", value: (int)piece.hitPoints, valueMax: (int)piece.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0),
                    posX: destRect.Center.X, posY: destRect.Y + (int)(destRect.Width * 0.8f), width: (int)(destRect.Width * 0.8f), height: (int)(destRect.Height * 0.1f));

                StatBar.FinishThisBatch();
                StatBar.DrawAll();
            }

            // drawing "new" icon
            if (drawNewIcon && !world.identifiedPieces.Contains(piece.name))
            {
                int rectSize = (int)(destRect.Height * 0.6f);
                int rectOffset = (int)(rectSize * 0.2f);

                Rectangle newRect = new Rectangle(x: destRect.X - rectOffset, y: destRect.Y - rectOffset, width: (int)(destRect.Height * 0.6f), height: (int)(destRect.Height * 0.6f));
                Texture2D newIconTexture = AnimData.framesForPkgs[AnimData.PkgName.NewIcon].texture;

                Helpers.DrawTextureInsideRect(texture: newIconTexture, rectangle: newRect, color: Color.White * opacity, alignX: Helpers.AlignX.Left, alignY: Helpers.AlignY.Top);
            }
        }
    }
}