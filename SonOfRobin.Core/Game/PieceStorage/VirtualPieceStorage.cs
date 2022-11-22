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
            public bool StorageResized { get { return !(this.storageWidth == this.storage.Width && this.storageHeight == this.storage.Height); } }

            private byte storageWidth;
            private byte storageHeight;

            public VirtPieceStoragePack(PieceStorage storage, bool newRow = false)
            {
                this.storage = storage;
                this.newRow = newRow;

                this.OffsetSet = false;

                this.UpdateStorageSize();
            }

            public void UpdateStorageSize()
            {
                this.storageWidth = storage.Width;
                this.storageHeight = storage.Height;
            }

            public void SetOffset(Point offset)
            {
                this.Offset = offset;
                this.OffsetSet = true;
            }
        }

        private readonly List<VirtPieceStoragePack> virtStoragePackList;
        private readonly int padding;

        public VirtualPieceStorage(BoardPiece storagePiece, List<VirtPieceStoragePack> virtStoragePackList, string label, int padding = 1) :
            base(width: 1, height: 1, storagePiece: storagePiece, storageType: StorageType.Virtual, label: label)
        {
            this.virtStoragePackList = virtStoragePackList;
            this.padding = padding;
            this.Recalculate();
        }

        public void Recalculate()
        {
            Point storageSize = GetStorageSize();

            this.Width = (byte)storageSize.X;
            this.Height = (byte)storageSize.Y;

            this.slots = this.MakeVirtualSlots();
            this.UpdateSlotPosByID();
        }

        public override void Update()
        {
            base.Update();

            var resizedStorages = this.virtStoragePackList.Where(storagePack => storagePack.StorageResized);
            if (resizedStorages.Any()) this.Recalculate();
            foreach (VirtPieceStoragePack storagePack in resizedStorages)
            {
                storagePack.UpdateStorageSize();
            }
        }

        private Point GetStorageSize()
        {
            this.UpdateStoragePackOffsets();

            int maxWidth = 0;
            int maxHeight = 0;

            foreach (VirtPieceStoragePack storagePack in this.virtStoragePackList)
            {
                if (!storagePack.OffsetSet) throw new ArgumentException("Offset not set.");

                maxWidth = Math.Max(maxWidth, storagePack.Offset.X + storagePack.storage.Width);
                maxHeight = Math.Max(maxHeight, storagePack.Offset.Y + storagePack.storage.Height);
            }

            return new Point(maxWidth, maxHeight);
        }

        private void UpdateStoragePackOffsets()
        {
            Point creationCursor = new Point(0, 0);
            Point prevStorageSize = new Point(0, 0);

            foreach (VirtPieceStoragePack storagePack in this.virtStoragePackList)
            {
                if (storagePack.newRow)
                {
                    creationCursor.X = 0;
                    creationCursor.Y += prevStorageSize.Y;
                    if (prevStorageSize.Y > 0) creationCursor.Y += this.padding;
                }
                else
                {
                    creationCursor.X += prevStorageSize.X;
                    if (prevStorageSize.X > 0) creationCursor.X += this.padding;
                }

                storagePack.SetOffset(creationCursor);

                prevStorageSize.X = storagePack.storage.Width;
                prevStorageSize.Y = storagePack.storage.Height;
            }
        }

        private StorageSlot[,] MakeVirtualSlots()
        {
            // slots that will be assigned to multiple storages (original and this one)
            var virtSlots = new StorageSlot[this.Width, this.Height];

            foreach (VirtPieceStoragePack storagePack in this.virtStoragePackList)
            {
                PieceStorage storage = storagePack.storage;
                foreach (StorageSlot slot in storage.AllSlots)
                {
                    Point localSlotPos = storage.GetSlotPos(slot);
                    Point globalSlotPos = new Point(storagePack.Offset.X + localSlotPos.X, storagePack.Offset.Y + localSlotPos.Y);
                    virtSlots[globalSlotPos.X, globalSlotPos.Y] = slot;
                }
            }

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    if (virtSlots[x, y] == null)
                    {
                        StorageSlot lockedSlot = new StorageSlot(storage: this)
                        {
                            locked = true,
                            hidden = true
                        };

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