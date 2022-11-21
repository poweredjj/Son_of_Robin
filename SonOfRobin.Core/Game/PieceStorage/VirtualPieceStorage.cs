using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VirtualPieceStorage : PieceStorage
    {
        private readonly PieceStorage[,] storageArray;
        private readonly int arrayWidth;
        private readonly int arrayHeight;

        private readonly int[] maxWidthForColumns;
        private readonly int[] maxHeightForRows;
        private readonly Point[,] offsetArray; // offset for every storage

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

        public override List<StorageSlot> AllSlots
        { get { return this.allSlots; } }

        public VirtualPieceStorage(BoardPiece storagePiece, World world, PieceStorage[,] storageArray) :
            base(width: 1, height: 1, world: world, storagePiece: storagePiece, storageType: storageArray[0, 0].storageType)
        {
            this.storageArray = storageArray;
            this.arrayWidth = this.storageArray.GetLength(0);
            this.arrayHeight = this.storageArray.GetLength(1);

            this.maxWidthForColumns = this.CalculateMaxWidthForColumns();
            this.maxHeightForRows = this.CalculateMaxHeightForRows();

            this.offsetArray = this.CalculateOffsetArray();

            this.totalWidth = this.CalculateTotalWidth();
            this.totalHeight = this.CalculateTotalHeight();

            this.allowedPieceNames = this.GetAllowedNames();
            this.allSlots = this.GetAllSlots();

            this.lockedSlot = new StorageSlot(storage: this, posX: 0, posY: 0);
            this.lockedSlot.locked = true;
            this.lockedSlot.hidden = true;
        }

        //public override StorageSlot GetSlot(int x, int y)
        //{
        //    if (x >= this.Width || y >= this.Height) return null;

        //    int arrayX, arrayY;

        //    int currentSlotPosX = 0;
        //    for (int currentX = 0; currentX < this.arrayWidth; x++)
        //    {
        //        if (this.arr)

        //    }

        //    return this.slots[x, y];

        //}

        private int[] CalculateMaxWidthForColumns()
        {
            int[] maxWidthForColumns = new int[this.arrayWidth];

            for (int x = 0; x < this.arrayWidth; x++)
            {
                int maxColumnWidth = 0;

                for (int y = 0; y < this.arrayHeight; y++)
                {
                    if (this.storageArray[x, y] == null) continue;
                    maxColumnWidth = Math.Max(maxColumnWidth, this.storageArray[x, y].Width);
                }

                maxWidthForColumns[x] = maxColumnWidth;
            }

            return maxWidthForColumns;
        }

        private int[] CalculateMaxHeightForRows()
        {
            int[] maxHeightForRows = new int[this.arrayHeight];

            for (int y = 0; y < this.arrayHeight; y++)
            {
                int maxRowHeight = 0;

                for (int x = 0; x < this.arrayWidth; x++)
                {
                    if (this.storageArray[x, y] == null) continue;
                    maxRowHeight = Math.Max(maxRowHeight, this.storageArray[x, y].Height);
                }

                maxHeightForRows[y] = maxRowHeight;
            }

            return maxHeightForRows;
        }


        private Point[,] CalculateOffsetArray()
        {
            List<int> xOffsetList = new List<int> { 0 }; // first storage should start with offset == 0

            for (int x = 0; x < this.arrayWidth - 1; x++)
            {
                xOffsetList.Add(xOffsetList.Last() + this.maxWidthForColumns[x]);
            }

            List<int> yOffsetList = new List<int> { 0 }; // first storage should start with offset == 0

            for (int y = 0; y < this.arrayHeight - 1; y++)
            {

                yOffsetList.Add(yOffsetList.Last() + this.maxHeightForRows[y]);
            }

            var newOffsetArray = new Point[this.arrayWidth, this.arrayHeight];

            for (int x = 0; x < this.arrayWidth; x++)
            {
                for (int y = 0; y < this.arrayHeight; y++)
                {
                    newOffsetArray[x, y] = new Point(xOffsetList[x], yOffsetList[y]);
                }
            }

            return newOffsetArray;
        }

        private byte CalculateTotalWidth()
        {
            byte addedWidth = 0;

            for (int x = 0; x < this.arrayWidth; x++)
            {
                addedWidth += (byte)this.maxWidthForColumns[x];
            }

            return addedWidth;
        }

        private byte CalculateTotalHeight()
        {
            byte addedHeight = 0;

            for (int y = 0; y < this.arrayHeight; y++)
            {
                addedHeight += (byte)this.maxHeightForRows[y];
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