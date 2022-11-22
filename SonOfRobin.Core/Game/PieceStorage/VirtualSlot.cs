namespace SonOfRobin
{
    public class VirtualSlot : StorageSlot
    {
        private readonly VirtualPieceStorage virtualPieceStorage;

        public VirtualSlot(StorageSlot realSlot, PieceStorage storage, byte posX, byte posY) :
            base(storage: realSlot.storage, posX: posX, posY: posY, stackLimit: realSlot.stackLimit, allowedPieceNames: realSlot.allowedPieceNames)
        {
            this.virtualPieceStorage = (VirtualPieceStorage)storage;

            this.pieceList = realSlot.pieceList;
            this.label = realSlot.label;
            this.locked = realSlot.locked;
            this.hidden = realSlot.hidden;
        }
    }
}