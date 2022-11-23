using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class VirtPieceStoragePack
    {
        public readonly PieceStorage storage;
        public readonly bool newRow;
        public Point Offset { get; private set; }
        public bool OffsetSet { get; private set; }

        public bool StorageResized
        { get { return !(this.storageWidth == this.storage.Width && this.storageHeight == this.storage.Height); } }

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
}