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
            public int XOffset { get; private set; }
            public int YOffset { get; private set; }
            public bool OffsetSet { get; private set; }

            public VirtPieceStoragePack(PieceStorage storage, bool newRow)
            {
                this.storage = storage;
                this.newRow = newRow;
                this.OffsetSet = false;
            }

            public void SetOffset(Point offset)
            {
                if (this.OffsetSet) throw new ArgumentException("Cannot set offset twice.");

                this.XOffset = offset.X;
                this.YOffset = offset.Y;

                this.OffsetSet = true;
            }
        }

        private readonly List<VirtPieceStoragePack> virtStoragePackList;

        public VirtualPieceStorage(BoardPiece storagePiece, World world, List<VirtPieceStoragePack> pieceStoragePackList) :
            base(width: (byte)GetStorageSize(pieceStoragePackList).X, height: (byte)GetStorageSize(pieceStoragePackList).Y, world: world, storagePiece: storagePiece, storageType: pieceStoragePackList[0].storage.storageType)
        {
            this.virtStoragePackList = pieceStoragePackList;
        }

        private static Point GetStorageSize(List<VirtPieceStoragePack> pieceStoragePackList)
        {
            Point creationCursor = new Point(0, 0);

            int maxWidth = 0;
            int totalHeight = 0;

            Point prevStorageSize = new Point(0, 0);

            foreach (VirtPieceStoragePack storagePack in pieceStoragePackList)
            {
                PieceStorage storage = storagePack.storage;

                if (storagePack.newRow)
                {
                    creationCursor.X = 0;
                    creationCursor.Y += prevStorageSize.Y;
                }
                else creationCursor.X += prevStorageSize.X;

                if (!storagePack.OffsetSet) storagePack.SetOffset(creationCursor);

                maxWidth = Math.Max(maxWidth, creationCursor.X + storage.Width);

                prevStorageSize.X = storage.Width;
                prevStorageSize.Y = storage.Height;

                if (storagePack.storage == pieceStoragePackList.Last().storage) totalHeight = creationCursor.Y + storage.Height;
            }

            return new Point(maxWidth, totalHeight);
        }
    }
}