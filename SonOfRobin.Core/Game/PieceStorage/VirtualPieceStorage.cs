using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class VirtualPieceStorage : PieceStorage
    {
        public class VirtPieceStoragePack
        {
            public readonly PieceStorage storage;
            public readonly bool newRow;
            public Point Offset { get; private set; }
            public bool OffsetSet { get; private set; }

            public VirtPieceStoragePack(PieceStorage storage, bool newRow = false)
            {
                this.storage = storage;
                this.newRow = newRow;
                this.OffsetSet = false;
            }

            public void SetOffset(Point offset)
            {
                this.Offset = offset;
                this.OffsetSet = true;
            }
        }

        private readonly List<VirtPieceStoragePack> virtStoragePackList;

        public VirtualPieceStorage(BoardPiece storagePiece, World world, List<VirtPieceStoragePack> virtStoragePackList) :
            base(width: 1, height: 2, world: world, storagePiece: storagePiece, storageType: StorageType.Virtual)
        {
            this.virtStoragePackList = virtStoragePackList;
            this.Recalculate();
        }

        public void Recalculate()
        {
            Point storageSize = GetStorageSize(virtStoragePackList);

            this.Width = (byte)storageSize.X;
            this.Height = (byte)storageSize.Y;

            this.slots = this.MakeVirtualSlots();
        }

        private static Point GetStorageSize(List<VirtPieceStoragePack> virtStoragePackList)
        {
            UpdateStoragePackOffsets(virtStoragePackList);

            int maxWidth = 0;
            int maxHeight = 0;

            foreach (VirtPieceStoragePack storagePack in virtStoragePackList)
            {
                if (!storagePack.OffsetSet) throw new ArgumentException("Offset not set.");

                maxWidth = Math.Max(maxWidth, storagePack.Offset.X + storagePack.storage.Width);
                maxHeight = Math.Max(maxHeight, storagePack.Offset.Y + storagePack.storage.Height);
            }

            return new Point(maxWidth, maxHeight);
        }

        private static void UpdateStoragePackOffsets(List<VirtPieceStoragePack> virtStoragePackList, bool force = false)
        {
            bool offsetNeedsToBeCalculated = virtStoragePackList.Where(storagePack => !storagePack.OffsetSet).ToList().Any();
            if (!offsetNeedsToBeCalculated && !force) return;

            int padding = 1;

            Point creationCursor = new Point(0, 0);
            Point prevStorageSize = new Point(0, 0);

            foreach (VirtPieceStoragePack storagePack in virtStoragePackList)
            {
                PieceStorage storage = storagePack.storage;

                if (storagePack.newRow)
                {
                    creationCursor.X = 0;
                    creationCursor.Y += prevStorageSize.Y;
                    if (prevStorageSize.Y > 0) creationCursor.Y += padding;
                }
                else
                {
                    creationCursor.X += prevStorageSize.X;
                    if (prevStorageSize.X > 0) creationCursor.X += padding;
                }

                storagePack.SetOffset(creationCursor);

                prevStorageSize.X = storage.Width;
                prevStorageSize.Y = storage.Height;
            }
        }

        private StorageSlot[,] MakeVirtualSlots() // overwrites original slots with virtual ones
        {
            var virtSlots = new StorageSlot[this.Width, this.Height];

            foreach (VirtPieceStoragePack storagePack in this.virtStoragePackList)
            {
                PieceStorage storage = storagePack.storage;
                foreach (StorageSlot slot in storage.AllSlots)
                {
                    Point globalSlotPos = new Point(storagePack.Offset.X + slot.posX, storagePack.Offset.Y + slot.posY);
                    virtSlots[globalSlotPos.X, globalSlotPos.Y] = new VirtualSlot(realSlot: slot, storage: this, posX: (byte)globalSlotPos.X, posY: (byte)globalSlotPos.Y);
                }
            }

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    if (virtSlots[x, y] == null)
                    {
                        StorageSlot lockedSlot = new StorageSlot(storage: this, posX: (byte)x, posY: (byte)y);
                        lockedSlot.locked = true;
                        lockedSlot.hidden = true;

                        virtSlots[x, y] = lockedSlot;
                    }
                }
            }

            return virtSlots;
        }

        public override void Sort()
        {
            foreach (VirtPieceStoragePack storagePack in this.virtStoragePackList)
            {
                storagePack.storage.Sort();
            }
        }
    }
}