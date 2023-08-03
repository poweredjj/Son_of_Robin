using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MeatDryingRack : BoardPiece
    {
        private class SlotExtensionDrying
        {
            public readonly StorageSlot slot;
            private readonly World world;
            public BoardPiece Meat { get; private set; }
            public int StartFrame { get; private set; }
            public int FinishFrame { get; private set; }
            public int FramesLeft { get { return Math.Max(this.FinishFrame - this.world.CurrentUpdate, 0); } }
            public bool HasFinished { get { return this.Meat != null && this.FramesLeft == 0; } }
            public bool CanBeDried { get { return this.Meat != null && rawMeatNames.Contains(this.Meat.name); } }

            public SlotExtensionDrying(StorageSlot slot)
            {
                this.slot = slot;
                this.world = this.slot.storage.world;
            }

            public void Update()
            {
                if (this.slot.IsEmpty)
                {
                    if (this.Meat != null) this.RemoveMeat();
                }
                else
                {
                    BoardPiece meat = this.slot.TopPiece;
                    if (meat != this.Meat) this.ReassignMeat();

                    if (this.HasFinished)
                    {
                        this.slot.GetAllPieces(remove: true);

                        BoardPiece driedMeat = PieceTemplate.Create(templateName: PieceTemplate.Name.MeatDried, world: this.world);
                        driedMeat.Mass = meat.Mass;

                        this.slot.DestroyPieceAndReplaceWithAnother(driedMeat);

                        this.ReassignMeat();
                    }
                }
            }

            private void ReassignMeat()
            {
                BoardPiece meat = slot.TopPiece;
                if (meat == null)
                {
                    this.RemoveMeat();
                    return;
                }

                this.Meat = meat;
                this.StartFrame = this.world.CurrentUpdate;
                this.FinishFrame = (int)(meat.Mass * 10) + this.world.CurrentUpdate; // * 200
            }

            private void RemoveMeat()
            {
                this.Meat = null;
                this.StartFrame = 0;
                this.FinishFrame = 0;
            }

            public Dictionary<string, Object> Serialize()
            {
                var dryingData = new Dictionary<string, object>();

                dryingData["StartFrame"] = this.StartFrame;
                dryingData["FinishFrame"] = this.FinishFrame;

                return dryingData;
            }

            public void Deserialize(Dictionary<string, Object> dryingData)
            {
                this.ReassignMeat(); // must go first...

                // ...and then overwrite frames
                this.StartFrame = (int)(Int64)dryingData["StartFrame"];
                this.FinishFrame = (int)(Int64)dryingData["FinishFrame"];
            }
        }

        private static readonly List<PieceTemplate.Name> rawMeatNames = new List<PieceTemplate.Name> { PieceTemplate.Name.MeatRawRegular, PieceTemplate.Name.MeatRawPrime };

        private List<SlotExtensionDrying> slotExtensionList; // slots extended with drying data

        public MeatDryingRack(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.DryMeat, soundPack: soundPack)
        {
            this.PieceStorage = new PieceStorage(width: 2, height: 2, storagePiece: this, storageType: PieceStorage.StorageType.Drying);
            this.slotExtensionList = new List<SlotExtensionDrying>();
            this.ConfigureStorage();
        }

        private void ConfigureStorage()
        {
            this.slotExtensionList.Clear();

            int slotNo = 0;
            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.allowedPieceNames = rawMeatNames.ToList();
                slot.allowedPieceNames.Add(PieceTemplate.Name.MeatDried);
                slot.stackLimit = 1;
                this.slotExtensionList.Add(new SlotExtensionDrying(slot));
                slotNo++;
            }
        }

        public override void SM_DryMeat()
        {
            List<BoardPiece> meatList = this.PieceStorage.GetAllPieces();

            int meatCount = meatList.Count;
            this.sprite.AssignNewName(newAnimName: meatCount > 0 ? $"on_{meatCount}" : "off", checkForCollision: false); // collision check must be turned off, to work if player is nearby

            if (meatCount == 0) return;

            if (this.world.CurrentUpdate % 30 == 0) ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.MeatDrying, duration: 1);
            this.showStatBarsTillFrame = this.world.CurrentUpdate + 1000;

            foreach (SlotExtensionDrying slotExtension in this.slotExtensionList)
            {
                slotExtension.Update();
            }
        }

        public override void DrawStatBar()
        {
            foreach (SlotExtensionDrying slotExtension in this.slotExtensionList)
            {
                if (slotExtension.CanBeDried)
                {
                    int dryingDuration = slotExtension.FinishFrame - slotExtension.StartFrame;
                    int dryingCurrentFrame = this.world.CurrentUpdate - slotExtension.StartFrame;

                    new StatBar(label: "", value: dryingCurrentFrame, valueMax: dryingDuration, colorMin: new Color(0, 141, 166), colorMax: new Color(0, 210, 247), posX: this.sprite.GfxRect.Center.X - 1, posY: this.sprite.GfxRect.Center.Y + 4, ignoreIfAtMax: true, height: 4, width: (int)(this.sprite.GfxRect.Width * 0.8f));
                }
            }

            base.DrawStatBar();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            var slotExtensionDataList = new List<object>();
            foreach (SlotExtensionDrying slotExtension in this.slotExtensionList)
            {
                slotExtension.Update();
                slotExtensionDataList.Add(slotExtension.Serialize());
            }

            pieceData["dryingRack_slotExtensionDataList"] = slotExtensionDataList;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            var slotExtensionDataList = (List<Object>)pieceData["dryingRack_slotExtensionDataList"];

            this.ConfigureStorage();

            int slotNo = 0;
            foreach (SlotExtensionDrying slotExtension in this.slotExtensionList)
            {
                var slotExtensionData = (Dictionary<string, object>)slotExtensionDataList[slotNo];
                slotExtension.Deserialize(slotExtensionData);
                slotNo++;
            }
        }
    }
}