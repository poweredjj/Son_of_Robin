using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class VirtualPieceStorage : PieceStorage
    {
        private readonly PieceStorage[,] storageArray;
        private readonly int arrayWidth;
        private readonly int arrayHeight;

        private readonly byte totalWidth;
        private readonly byte totalHeight;
        public override byte Width
        { get { return this.totalWidth; } }
        public override byte Height
        { get { return this.totalHeight; } }

        private readonly StorageSlot lockedSlot;

        private readonly List<PieceTemplate.Name> allowedPieceNames;
        public override List<PieceTemplate.Name> AllowedPieceNames
        { get { return this.allowedPieceNames; } }

        private readonly List<StorageSlot> allSlots;
        public override List<StorageSlot> AllSlots { get { return this.allSlots; } }

        public VirtualPieceStorage(BoardPiece storagePiece, World world, PieceStorage[,] storageArray) :
            base(width: 1, height: 1, world: world, storagePiece: storagePiece, storageType: storageArray[0, 0].storageType)
        {
            this.storageArray = storageArray;
            this.arrayWidth = this.storageArray.GetLength(0);
            this.arrayHeight = this.storageArray.GetLength(1);

            this.totalWidth = this.CalculateTotalWidth();
            this.totalHeight = this.CalculateTotalHeight();

            this.allowedPieceNames = this.GetAllowedNames();
            this.allSlots = this.GetAllSlots();

            this.lockedSlot = new StorageSlot(storage: this, posX: 0, posY: 0);
            this.lockedSlot.locked = true;
            this.lockedSlot.hidden = true;
        }

        private byte CalculateTotalWidth()
        {
            byte addedWidth = 0;

            for (int x = 0; x < this.arrayWidth; x++)
            {
                byte maxColumnWidth = 0;

                for (int y = 0; y < this.arrayHeight; y++)
                {
                    if (this.storageArray[x, y] == null) continue;
                    maxColumnWidth = Math.Max(maxColumnWidth, this.storageArray[x, y].Width);
                }

                addedWidth += maxColumnWidth;
            }

            return addedWidth;
        }

        private byte CalculateTotalHeight()
        {
            byte addedHeight = 0;

            for (int y = 0; y < this.arrayHeight; y++)
            {
                byte maxRowHeight = 0;

                for (int x = 0; x < this.arrayWidth; x++)
                {
                    if (this.storageArray[x, y] == null) continue;
                    maxRowHeight = Math.Max(maxRowHeight, this.storageArray[x, y].Height);
                }

                addedHeight += maxRowHeight;
            }

            return addedHeight;
        }

        private List<PieceTemplate.Name> GetAllowedNames()
        {
            var allAllowedNames = new List<PieceTemplate.Name>();

            for (int x = 0; x < this.arrayWidth; x++)
            {
                for (int y = 0; y < this.arrayHeight; y++)
                {
                    PieceStorage pieceStorage = this.storageArray[x, y];
                    if (pieceStorage == null || pieceStorage.AllowedPieceNames == null) continue;

                    foreach (PieceTemplate.Name name in pieceStorage.AllowedPieceNames)
                    {
                        if (!allAllowedNames.Contains(name)) allAllowedNames.Add(name);
                    }
                }
            }

            return allAllowedNames;
        }

        private List<StorageSlot> GetAllSlots()
        {
            var foundSlots = new List<StorageSlot>();

            for (int x = 0; x < this.arrayWidth; x++)
            {
                for (int y = 0; y < this.arrayHeight; y++)
                {
                    PieceStorage pieceStorage = this.storageArray[x, y];
                    if (pieceStorage == null) continue;

                    foreach (StorageSlot slot in pieceStorage.AllSlots)
                    {
                        foundSlots.Add(slot);
                    }
                }
            }

            return foundSlots;
        }


    }
}