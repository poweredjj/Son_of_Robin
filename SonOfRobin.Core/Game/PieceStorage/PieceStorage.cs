using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SonOfRobin
{
    public class PieceStorage
    {
        public enum StorageType { Inventory, Chest, Toolbar, Fruits }

        public readonly World world;
        public readonly StorageType storageType;
        public readonly BoardPiece storagePiece;
        public readonly byte width;
        public readonly byte height;
        private readonly StorageSlot[,] slots;
        private readonly byte stackLimit;
        public int EmptySlotsCount { get { return this.EmptySlots.Count; } }
        public List<StorageSlot> EmptySlots { get { return AllSlots.Where(slot => slot.IsEmpty).ToList(); } }
        public int NotEmptySlotsCount { get { return this.NotEmptySlots.Count; } }
        public List<StorageSlot> NotEmptySlots { get { return AllSlots.Where(slot => !slot.IsEmpty).ToList(); } }
        public int FullSlotsCount { get { return this.FullSlots.Count; } }
        public List<StorageSlot> FullSlots { get { return AllSlots.Where(slot => slot.IsFull).ToList(); } }
        public int OccupiedSlotsCount { get { return this.OccupiedSlots.Count; } }
        public List<StorageSlot> OccupiedSlots { get { return AllSlots.Where(slot => !slot.IsEmpty).ToList(); } }

        public StorageSlot LastOccupiedSlot
        {
            get
            {
                var occupiedSlots = this.OccupiedSlots;
                if (occupiedSlots.Count == 0) return null;
                return occupiedSlots[occupiedSlots.Count - 1];
            }
        }

        public List<StorageSlot> AllSlots
        {
            get
            {
                var allSlots = new List<StorageSlot> { };

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    { allSlots.Add(slots[x, y]); }

                }
                return allSlots;
            }
        }

        public PieceStorage(byte width, byte height, World world, BoardPiece storagePiece, StorageType storageType, byte stackLimit = 255)
        {
            Debug.Assert(width > 0 && height > 0);

            this.storagePiece = storagePiece;
            this.storageType = storageType;
            this.stackLimit = stackLimit;
            this.world = world;
            this.width = width;
            this.height = height;
            this.slots = this.MakeEmptySlots();
        }

        private StorageSlot[,] MakeEmptySlots()
        {
            var emptySlots = new StorageSlot[this.width, this.height];

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                { emptySlots[x, y] = new StorageSlot(storage: this, posX: (byte)x, posY: (byte)y, stackLimit: this.stackLimit); }
            }

            return emptySlots;
        }

        public bool AddPiece(BoardPiece piece, bool dropIfDoesNotFit = false, bool addMovement = false)
        {
            StorageSlot slot = this.FindCorrectSlot(piece);
            if (slot == null)
            {
                if (dropIfDoesNotFit) DropPieceToTheGround(piece: piece, addMovement: addMovement);
                return false;
            }

            piece.sprite.RemoveFromGrid();
            slot.AddPiece(piece);

            return true;
        }


        public StorageSlot FindCorrectSlot(BoardPiece piece)
        {
            // trying to find existing stack
            foreach (StorageSlot slot in AllSlots)
            { if (!slot.IsEmpty && slot.PieceName == piece.name && slot.HasSpaceLeft) return slot; }

            foreach (StorageSlot slot in AllSlots)
            { if (slot.CanFitThisPiece(piece)) return slot; }

            return null;
        }

        public StorageSlot GetSlot(int x, int y)
        {
            try
            { return this.slots[x, y]; }
            catch (IndexOutOfRangeException)
            { return null; }
        }

        public List<BoardPiece> RemoveAllPiecesFromSlot(StorageSlot slot, bool dropToTheGround = false, bool addMovement = false)
        {
            List<BoardPiece> pieceList = slot.GetAllPieces(remove: true);

            if (dropToTheGround)
            {
                foreach (BoardPiece piece in pieceList)
                { this.DropPieceToTheGround(piece: piece, addMovement: addMovement); }
            }

            return pieceList;
        }

        public BoardPiece RemoveTopPiece(int x, int y)
        {
            try
            {
                StorageSlot slot = this.slots[x, y];
                return slot.RemoveTopPiece();
            }
            catch (IndexOutOfRangeException)
            { MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Can't get piece - inventory index out of bounds ({x},{y})", color: Color.White); }

            return null;
        }

        public BoardPiece GetTopPiece(StorageSlot slot)
        { return slot.GetTopPiece(); }

        public BoardPiece RemoveTopPiece(StorageSlot slot)
        { return slot.RemoveTopPiece(); }

        public void DropTopPieceFromSlot(int x, int y, bool addMovement = false)
        {
            try
            {
                StorageSlot slot = this.slots[x, y];
                this.DropTopPieceFromSlot(slot: slot, addMovement: addMovement);
            }
            catch (IndexOutOfRangeException)
            { MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Can't drop piece - inventory index out of bounds ({x},{y})", color: Color.White); }
        }


        public void DropTopPieceFromSlot(StorageSlot slot, bool destroyIfFreeSpotNotFound = false, bool addMovement = false)
        {
            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Dropping piece...", color: Color.White);

            BoardPiece piece = slot.RemoveTopPiece();
            if (piece == null) return;

            bool freeSpotFound = this.DropPieceToTheGround(piece: piece, addMovement: addMovement);
            if (!freeSpotFound)
            { if (!destroyIfFreeSpotNotFound) slot.AddPiece(piece); }
            else
            { MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"{piece.name} has been dropped.", color: Color.White); }
        }

        public List<BoardPiece> GetAllPieces()
        {
            var pieceList = new List<BoardPiece> { };

            foreach (StorageSlot slot in this.AllSlots)
            { pieceList.AddRange(slot.GetAllPieces(remove: false)); }
            return pieceList;
        }


        public void DropAllPiecesToTheGround(bool addMovement = false)
        {
            foreach (StorageSlot slot in this.AllSlots)
            {
                foreach (BoardPiece piece in slot.GetAllPieces(remove: true))
                { this.DropPieceToTheGround(piece: piece, addMovement: addMovement); }
            }
        }

        public bool DropPieceToTheGround(BoardPiece piece, bool addMovement)
        {
            if (this.GetType() == typeof(Player))
            {
                // item should fall naturally to places, where player can go to
                piece.sprite.allowedFields = this.storagePiece.sprite.allowedFields; 
            }

            bool freeSpotFound = piece.sprite.MoveToClosestFreeSpot(startPosition: this.storagePiece.sprite.position);
            if (freeSpotFound)
            {
                piece.AddToStateMachines();
                piece.AddPlannedDestruction();

                if (addMovement)
                {
                    Vector2 passiveMovement = (this.storagePiece.sprite.position - piece.sprite.position) * -1 * this.world.random.Next(3, 25);
                    piece.AddPassiveMovement(movement: passiveMovement);
                }
            }

            return freeSpotFound;
        }
        public void DestroyBrokenPieces()
        {
            foreach (StorageSlot slot in AllSlots)
            { slot.DestroyBrokenPieces(); }
        }

        public void DestroySpecifiedPieces(Dictionary<PieceTemplate.Name, byte> quantityByPiece)
        {
            // this method does not check if all pieces are present

            var quantityLeft = quantityByPiece.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (StorageSlot slot in this.OccupiedSlots)
            {
                PieceTemplate.Name pieceName = slot.PieceName;

                if (quantityLeft.ContainsKey(pieceName) && quantityLeft[pieceName] > 0)
                {
                    while (true)
                    {
                        slot.RemoveTopPiece();
                        quantityLeft[pieceName] -= 1;
                        if (quantityLeft[pieceName] == 0 || slot.pieceList.Count == 0) break;
                    }
                }
            }

            Debug.Assert(quantityLeft.Where(kvp => kvp.Value > 0).ToList().Count == 0);
        }

        public bool CheckIfContainsSpecifiedPieces(Dictionary<PieceTemplate.Name, byte> quantityByPiece)
        {
            var quantityLeft = quantityByPiece.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (PieceTemplate.Name name in quantityLeft.Keys.ToList())
            {
                foreach (StorageSlot slot in this.OccupiedSlots)
                {
                    foreach (BoardPiece piece in slot.pieceList)
                    {
                        if (piece.name != name || quantityLeft[name] == 0) break;
                        if (quantityLeft[name] > 0) quantityLeft[name] -= 1;
                    }
                }
            }

            return quantityLeft.Where(kvp => kvp.Value > 0).ToList().Count == 0;
        }

        public int CountPieceOccurences(PieceTemplate.Name pieceName)
        {
            int occurences = 0;

            foreach (StorageSlot slot in this.OccupiedSlots)
            {
                foreach (BoardPiece piece in slot.pieceList)
                { if (piece.name == pieceName) occurences++; }
            }

            return occurences;
        }

        public Dictionary<string, Object> Serialize()
        {
            var slotData = new List<Object> { };

            foreach (StorageSlot slot in this.AllSlots)
            { slotData.Add(slot.Serialize()); }

            var lockedSlots = new List<int> { };
            int slotNo = 0;
            foreach (StorageSlot slot in this.AllSlots)
            {
                if (slot.locked) lockedSlots.Add(slotNo);
                slotNo++;
            }

            var storageDict = new Dictionary<string, Object>
            {
              {"width", this.width},
              {"height", this.height},
              {"stackLimit", this.stackLimit},
              {"slotData", slotData},
              {"storageType", storageType},
              {"lockedSlots", lockedSlots},
            };

            return storageDict;
        }

        public static PieceStorage Deserialize(Object storageData, World world, BoardPiece storagePiece)
        {
            if (storageData == null) return null;

            var storageDict = (Dictionary<string, Object>)storageData;

            byte width = (byte)storageDict["width"];
            byte height = (byte)storageDict["height"];
            byte stackLimit = (byte)storageDict["stackLimit"];
            var slotData = (List<Object>)storageDict["slotData"];
            StorageType storageType = (StorageType)storageDict["storageType"];
            var lockedSlots = (List<int>)storageDict["lockedSlots"];

            PieceStorage storage = new PieceStorage(width: width, height: height, world: world, storagePiece: storagePiece, storageType: storageType, stackLimit: stackLimit);

            int slotNo = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    storage.slots[x, y].Deserialize(slotData[slotNo]);
                    slotNo++;
                }
            }

            slotNo = 0;
            foreach (StorageSlot slot in storage.AllSlots)
            {
                if (lockedSlots.Contains(slotNo)) slot.locked = true;

                slotNo++;
            }

            return storage;
        }

    }
}
