using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MeatDryingRack : BoardPiece
    {
        private class MeatDrying
        {
            public readonly BoardPiece meat;
            private readonly int finishFrame;

            public int FramesLeft { get { return Math.Max(this.finishFrame - this.meat.world.CurrentUpdate, 0); } }
            public bool HasFinished { get { return this.FramesLeft == 0; } }

            public MeatDrying(BoardPiece meat)
            {
                this.meat = meat;
                this.finishFrame = (int)(meat.Mass * 100) + meat.world.CurrentUpdate;
            }
        }

        private static readonly List<PieceTemplate.Name> rawMeatNames = new List<PieceTemplate.Name> { PieceTemplate.Name.MeatRawRegular, PieceTemplate.Name.MeatRawPrime };

        private static readonly Dictionary<int, string> animNameDict = new Dictionary<int, string>
            {
                { 0, "off" },
                { 1, "on_1" },
                { 2, "on_2" },
                { 3, "on_3" },
                { 4, "on_4" },
            };

        private Dictionary<int, MeatDrying> dryingForSlotNo;

        private bool isOn;

        public MeatDryingRack(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.DryMeat, soundPack: soundPack)
        {
            this.isOn = false;
            this.PieceStorage = new PieceStorage(width: 2, height: 2, storagePiece: this, storageType: PieceStorage.StorageType.Drying);
            this.dryingForSlotNo = new Dictionary<int, MeatDrying>();
            this.ConfigureStorage();
        }

        private void ConfigureStorage()
        {
            int slotNo = 0;
            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.allowedPieceNames = rawMeatNames.ToList();
                slot.allowedPieceNames.Add(PieceTemplate.Name.MeatDried);
                slot.stackLimit = 1;
                this.dryingForSlotNo[slotNo] = null;
                slotNo++;
            }
        }

        public override void SM_DryMeat()
        {
            List<BoardPiece> meatList = this.PieceStorage.GetAllPieces();

            int meatCount = meatList.Count;
            this.sprite.AssignNewName(newAnimName: animNameDict[meatCount]);

            this.isOn = meatCount > 0;
            if (!this.isOn) return;

            int slotNo = 0;
            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                if (slot.IsEmpty)
                {
                    this.dryingForSlotNo[slotNo] = null;
                }
                else
                {
                    BoardPiece meat = slot.GetAllPieces(remove: false)[0];
                    if (this.dryingForSlotNo[slotNo] != null && this.dryingForSlotNo[slotNo].meat != meat)
                    {
                        this.dryingForSlotNo[slotNo] = new MeatDrying(meat: meat);
                    }

                    // TODO add drying code

                }

                slotNo++;
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            // pieceData["dryingRack_dryFinishFrameForSlotNo"] = this.dryFinishFrameForSlotNo;
            // TODO add dryingForSlotNo serialization

            pieceData["dryingRack_isOn"] = this.isOn;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            // this.dryFinishFrameForSlotNo = (Dictionary<int, int>)pieceData["dryingRack_dryFinishFrameForSlotNo"];
            // TODO add dryingForSlotNo deserialization

            this.isOn = (bool)pieceData["dryingRack_isOn"];
            this.ConfigureStorage();
        }
    }
}