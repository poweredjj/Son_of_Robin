using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SonOfRobin
{
    public class PieceStorage
    {
        public enum StorageType { Inventory, Cooking, Fireplace, Chest, Tools, Equip, Fruits }

        public readonly World world;
        public readonly StorageType storageType;
        public readonly BoardPiece storagePiece;
        private byte width;
        private byte height;
        public byte Width { get { return width; } }
        public byte Height { get { return height; } }

        private StorageSlot[,] slots;
        private readonly byte stackLimit;
        public List<PieceTemplate.Name> allowedPieceNames;
        public StorageSlot lastUsedSlot; // last used by Inventory class
        public int AllSlotsCount { get { return this.width * this.height; } }
        public int EmptySlotsCount { get { return this.EmptySlots.Count; } }
        public List<StorageSlot> EmptySlots { get { return AllSlots.Where(slot => slot.IsEmpty).ToList(); } }
        public int FullSlotsCount { get { return this.FullSlots.Count; } }
        public List<StorageSlot> FullSlots { get { return AllSlots.Where(slot => slot.IsFull).ToList(); } }
        public int NotFullSlotsCount { get { return this.NotFullSlots.Count; } }
        public List<StorageSlot> NotFullSlots { get { return AllSlots.Where(slot => !slot.IsFull).ToList(); } }
        public int OccupiedSlotsCount { get { return this.OccupiedSlots.Count; } }
        public List<StorageSlot> OccupiedSlots { get { return AllSlots.Where(slot => !slot.IsEmpty).ToList(); } }

        public int StoredPiecesCount
        {
            get
            {
                int piecesCount = 0;

                foreach (StorageSlot slot in this.OccupiedSlots)
                { piecesCount += slot.PieceCount; }

                return piecesCount;
            }
        }

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

        public PieceStorage(byte width, byte height, World world, BoardPiece storagePiece, StorageType storageType, byte stackLimit = 255, List<PieceTemplate.Name> allowedPieceNames = null)
        {
            Debug.Assert(width > 0 && height > 0);

            this.storagePiece = storagePiece;
            this.storageType = storageType;
            this.stackLimit = stackLimit;
            this.world = world;
            this.width = width;
            this.height = height;
            this.allowedPieceNames = allowedPieceNames;
            this.slots = this.MakeEmptySlots();
        }

        public void AssignAllowedPieceNames(List<PieceTemplate.Name> allowedPieceNames)
        {
            foreach (StorageSlot slot in this.AllSlots)
            { slot.allowedPieceNames = allowedPieceNames; }
        }

        private StorageSlot[,] MakeEmptySlots()
        {
            var emptySlots = new StorageSlot[this.width, this.height];

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                { emptySlots[x, y] = new StorageSlot(storage: this, posX: (byte)x, posY: (byte)y, stackLimit: this.stackLimit, allowedPieceNames: allowedPieceNames); }
            }

            return emptySlots;
        }

        public void Resize(byte newWidth, byte newHeight)
        {
            if (this.width == newWidth && this.height == newHeight) return;

            var excessPieces = new List<BoardPiece> { };

            // dropping pieces from excess slots
            if (this.width > newWidth || this.height > newHeight)
            {
                for (int x = 0; x < this.width; x++)
                {
                    for (int y = 0; y < this.height; y++)
                    {
                        if (x >= newWidth || y >= newHeight)
                        {
                            excessPieces.AddRange(this.RemoveAllPiecesFromSlot(slot: this.slots[x, y], dropToTheGround: false));
                        }
                    }
                }
            }

            // creating new slots array
            var newSlots = new StorageSlot[newWidth, newHeight];

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    if (x >= this.width || y >= this.height)
                    {
                        newSlots[x, y] = new StorageSlot(storage: this, posX: (byte)x, posY: (byte)y, stackLimit: this.stackLimit, allowedPieceNames: allowedPieceNames);
                    }
                    else
                    {
                        newSlots[x, y] = this.slots[x, y];
                    }
                }
            }

            this.width = newWidth;
            this.height = newHeight;
            this.slots = newSlots;

            foreach (BoardPiece piece in excessPieces)
            {
                this.AddPiece(piece: piece, dropIfDoesNotFit: true, addMovement: true);
            }
        }

        public bool AddPiece(BoardPiece piece, bool dropIfDoesNotFit = false, bool addMovement = false)
        {
            StorageSlot slot = this.FindCorrectSlot(piece);
            if (slot == null)
            {
                if (dropIfDoesNotFit) DropPieceToTheGround(piece: piece, addMovement: addMovement);
                return false;
            }

            if (piece.sprite.IsOnBoard) piece.RemoveFromBoard();
            slot.AddPiece(piece);

            return true;
        }

        public bool CanFitThisPiece(BoardPiece piece)
        {
            return this.FindCorrectSlot(piece) != null;
        }

        public StorageSlot FindCorrectSlot(BoardPiece piece)
        {
            // trying to find existing stack
            foreach (StorageSlot slot in AllSlots)
            { if (!slot.IsEmpty && slot.PieceName == piece.name && slot.CanFitThisPiece(piece)) return slot; }

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
            { MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Can't get piece - inventory index out of bounds ({x},{y})", color: Color.White); }

            return null;
        }
        public BoardPiece GetTopPiece(StorageSlot slot)
        { return slot.TopPiece; }

        public BoardPiece RemoveTopPiece(StorageSlot slot)
        { return slot.RemoveTopPiece(); }

        public void DropTopPieceFromSlot(int x, int y, bool addMovement = false)
        {
            try
            {
                StorageSlot slot = this.slots[x, y];
                this.DropPiecesFromSlot(slot: slot, addMovement: addMovement);
            }
            catch (IndexOutOfRangeException)
            { MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Can't drop piece - inventory index out of bounds ({x},{y})", color: Color.White); }
        }
        public void DropPiecesFromSlot(StorageSlot slot, bool destroyIfFreeSpotNotFound = false, bool addMovement = false, bool dropAllPieces = false)
        {
            while (true)
            {
                BoardPiece piece = slot.RemoveTopPiece();
                if (piece == null) return;

                bool freeSpotFound = this.DropPieceToTheGround(piece: piece, addMovement: addMovement);
                if (!freeSpotFound)
                {
                    if (!destroyIfFreeSpotNotFound) slot.AddPiece(piece);
                    return;
                }
                else MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{piece.name} has been dropped.", color: Color.White);

                if (!dropAllPieces) return;
            }
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
            if (piece.GetType() == typeof(Player)) MessageLog.AddMessage(msgType: MsgType.Debug, message: "Dropping piece...", color: Color.White);

            // the piece should fall naturally to places, where player can go to
            piece.sprite.allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

            piece.PlaceOnBoard(position: this.storagePiece.sprite.position, closestFreeSpot: true);

            if (addMovement && piece.sprite.IsOnBoard)
            {
                Vector2 passiveMovement = (this.storagePiece.sprite.position - piece.sprite.position) * -1 * this.world.random.Next(3, 25);
                piece.AddPassiveMovement(movement: passiveMovement);
            }

            return piece.sprite.IsOnBoard;
        }
        public void DestroyBrokenPieces()
        {
            foreach (StorageSlot slot in AllSlots)
            { slot.DestroyBrokenPieces(); }
        }

        public void DestroyOneSpecifiedPiece(PieceTemplate.Name pieceName)
        {
            this.DestroySpecifiedPieces(new Dictionary<PieceTemplate.Name, byte> { { pieceName, 1 } });
        }

        public void DestroySpecifiedPieces(Dictionary<PieceTemplate.Name, byte> quantityByPiece)
        {
            DestroySpecifiedPiecesInMultipleStorages(storageList: new List<PieceStorage> { this }, quantityByPiece: quantityByPiece);
        }

        public static bool StorageListCanFitSpecifiedPieces(List<PieceStorage> storageList, PieceTemplate.Name pieceName, int quantity)
        {
            foreach (PieceStorage storage in storageList)
            {
                foreach (StorageSlot slot in storage.NotFullSlots)
                {
                    quantity -= slot.HowManyPiecesOfNameCanFit(pieceName: pieceName);
                    if (quantity <= 0) return true;
                }
            }

            return false;
        }

        public static void DestroySpecifiedPiecesInMultipleStorages(List<PieceStorage> storageList, Dictionary<PieceTemplate.Name, byte> quantityByPiece, bool keepContainers = true)
        {
            // this method does not check if all pieces are present

            var quantityLeft = quantityByPiece.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (PieceStorage storage in storageList)
            {
                foreach (StorageSlot slot in storage.OccupiedSlots)
                {
                    PieceTemplate.Name pieceName = slot.PieceName;

                    if (quantityLeft.ContainsKey(pieceName) && quantityLeft[pieceName] > 0)
                    {
                        while (true)
                        {
                            if (keepContainers && PieceInfo.GetInfo(pieceName).convertsWhenUsed)
                            {
                                BoardPiece emptyContainter = PieceTemplate.Create(templateName: PieceInfo.GetInfo(pieceName).convertsToWhenUsed, world: storage.world);
                                slot.DestroyPieceAndReplaceWithAnother(emptyContainter);
                                break; // this slot should not contain more items of this kind
                            }
                            else
                            {
                                slot.RemoveTopPiece();
                            }

                            quantityLeft[pieceName] -= 1;
                            if (quantityLeft[pieceName] == 0 || slot.pieceList.Count == 0) break;
                        }
                    }
                }
            }

            if (quantityLeft.Where(kvp => kvp.Value > 0).ToList().Count != 0) throw new ArgumentException("Not all pieces to destroy were found in multiple storages.");
        }

        public static Dictionary<PieceTemplate.Name, byte> CheckMultipleStoragesForSpecifiedPieces(List<PieceStorage> storageList, Dictionary<PieceTemplate.Name, byte> quantityByPiece)
        {
            var quantityLeft = quantityByPiece.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (PieceTemplate.Name name in quantityLeft.Keys.ToList())
            {
                foreach (PieceStorage storage in storageList)
                {
                    foreach (StorageSlot slot in storage.OccupiedSlots)
                    {
                        foreach (BoardPiece piece in slot.pieceList)
                        {
                            if (piece.name != name || quantityLeft[name] == 0) break;
                            if (quantityLeft[name] > 0) quantityLeft[name] -= 1;
                        }
                    }
                }
            }

            return quantityLeft.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public BoardPiece GetFirstPieceOfType(Type type, bool removePiece = false)
        {
            BoardPiece piece;

            foreach (StorageSlot slot in this.OccupiedSlots)
            {
                piece = slot.TopPiece;
                if (piece.GetType() == type)
                {
                    if (removePiece) return slot.RemoveTopPiece();
                    return piece;
                }
            }

            return null;
        }

        public BoardPiece GetFirstPieceOfName(PieceTemplate.Name name, bool removePiece = false)
        {
            BoardPiece piece;

            foreach (StorageSlot slot in this.OccupiedSlots)
            {
                piece = slot.TopPiece;
                if (piece.name == name)
                {
                    if (removePiece) return slot.RemoveTopPiece();
                    return piece;
                }
            }

            return null;
        }

        public int CountPieceOccurences(PieceTemplate.Name pieceName)
        {
            return CountPieceOccurencesInMultipleStorages(storageList: new List<PieceStorage> { this }, pieceName: pieceName);
        }

        public static int CountPieceOccurencesInMultipleStorages(List<PieceStorage> storageList, PieceTemplate.Name pieceName)
        {
            int occurences = 0;

            foreach (PieceStorage storage in storageList)
            {
                foreach (StorageSlot slot in storage.OccupiedSlots)
                {
                    foreach (BoardPiece piece in slot.pieceList)
                    { if (piece.name == pieceName) occurences++; }
                }
            }

            return occurences;
        }

        public bool ContainsThisPieceID(string pieceID)
        {
            return this.FindPieceWithThisID(pieceID) != null;
        }

        public BoardPiece FindPieceWithThisID(string pieceID)
        {
            foreach (StorageSlot slot in this.OccupiedSlots)
            {
                foreach (BoardPiece piece in slot.pieceList)
                {
                    if (piece.id == pieceID) return piece;
                }
            }

            return null;
        }

        public void Sort()
        {
            if (this.storageType == StorageType.Equip) return; // equip should not be sorted, to avoid removing and adding buffs

            int pieceCountInitial = this.StoredPiecesCount;

            var allPieces = new List<BoardPiece> { };
            foreach (StorageSlot slot in this.AllSlots)
            {
                allPieces.AddRange(this.RemoveAllPiecesFromSlot(slot: slot, dropToTheGround: false));
            }

            allPieces = allPieces.OrderBy(piece => piece.readableName).ToList();
            foreach (BoardPiece piece in allPieces)
            {
                this.AddPiece(piece: piece, dropIfDoesNotFit: true, addMovement: true);
            }

            int pieceCountAfterSort = this.StoredPiecesCount;

            if (pieceCountInitial != pieceCountAfterSort) throw new ArgumentException($"Initial piece count ({pieceCountInitial}) has changed after sorting ({pieceCountAfterSort}).");
        }

        public Dictionary<string, Object> Serialize()
        {
            var slotData = new List<Object> { };

            foreach (StorageSlot slot in this.AllSlots)
            { slotData.Add(slot.Serialize()); }

            var storageDict = new Dictionary<string, Object>
            {
              {"width", this.width},
              {"height", this.height},
              {"stackLimit", this.stackLimit},
              {"slotData", slotData},
              {"storageType", storageType},
            };

            if (this.lastUsedSlot != null)
            {
                storageDict["lastUsedSlotX"] = this.lastUsedSlot.posX;
                storageDict["lastUsedSlotY"] = this.lastUsedSlot.posY;
            }

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

            PieceStorage storage = new PieceStorage(width: width, height: height, world: world, storagePiece: storagePiece, storageType: storageType, stackLimit: stackLimit);
            if (storageDict.ContainsKey("lastUsedSlotX") && storageDict.ContainsKey("lastUsedSlotY"))
            {
                byte lastUsedSlotX = (byte)storageDict["lastUsedSlotX"];
                byte lastUsedSlotY = (byte)storageDict["lastUsedSlotY"];
                storage.lastUsedSlot = storage.GetSlot(x: lastUsedSlotX, y: lastUsedSlotY);
            }

            int slotNo = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    storage.slots[x, y].Deserialize(slotData[slotNo]);
                    slotNo++;
                }
            }

            return storage;
        }

    }
}
