﻿using System;
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
            public int FramesLeft { get { return Math.Max(this.FinishFrame - this.Meat.world.CurrentUpdate, 0); } }
            public bool HasFinished { get { return this.Meat != null && this.FramesLeft == 0; } }

            public SlotExtensionDrying(StorageSlot slot)
            {
                this.slot = slot;
                this.world = this.slot.storage.world;
            }

            public void ReassignMeat()
            {
                BoardPiece meat = slot.TopPiece;
                if (meat == null)
                {
                    this.RemoveMeat();
                    return;
                }

                this.Meat = meat;
                this.StartFrame = world.CurrentUpdate;
                this.FinishFrame = (int)(meat.Mass * 100) + this.world.CurrentUpdate;
            }

            public void RemoveMeat()
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
            this.sprite.AssignNewName(newAnimName: meatCount > 0 ? $"on_{meatCount}" : "off");

            if (meatCount == 0) return;

            //foreach (BoardPiece meat in meatList)
            //{
            //    if (!this.dryingForSlots.ContainsKey(meat.id)) { }
            //}

            //int slotNo = 0;
            //foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            //{
            //    if (slot.IsEmpty) this.dryingForSlots[slotNo] = null;
            //    else
            //    {
            //        BoardPiece meat = slot.GetAllPieces(remove: false)[0];
            //        if (this.dryingForSlots[slotNo] != null && this.dryingForSlots[slotNo].Meat != meat && rawMeatNames.Contains(meat.name))
            //        {
            //            this.dryingForSlots[slotNo] = new MeatDrying(meat: meat);
            //        }

            //        if (this.dryingForSlots[slotNo] != null && this.dryingForSlots[slotNo].HasFinished)
            //        {
            //            slot.GetAllPieces(remove: true);

            //            BoardPiece driedMeat = PieceTemplate.Create(templateName: PieceTemplate.Name.MeatDried, world: this.world);
            //            driedMeat.Mass = meat.Mass;

            //            slot.DestroyPieceAndReplaceWithAnother(driedMeat);
            //        }
            //    }

            //    slotNo++;
            //}
        }

        public override void DrawStatBar()
        {
            //int dryingStartFrame = 0;
            //int dryingFinishFrame = 0;

            //foreach (SlotExtensionDrying meatDrying in dryingForSlots.Values)
            //{
            //    if (meatDrying != null && meatDrying.FinishFrame > dryingFinishFrame)
            //    {
            //        dryingStartFrame = meatDrying.StartFrame;
            //        dryingFinishFrame = meatDrying.FinishFrame;
            //    }
            //}

            //int dryingDuration = dryingFinishFrame - dryingStartFrame;
            //int dryingCurrentFrame = this.world.CurrentUpdate - dryingStartFrame;

            //new StatBar(label: "", value: dryingCurrentFrame, valueMax: dryingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: true, texture: AnimData.framesForPkgs[AnimData.PkgName.MeatDried].texture);

            //base.DrawStatBar();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            var slotExtensionDataList = new List<object>();
            foreach (SlotExtensionDrying meatDrying in this.slotExtensionList)
            {
                slotExtensionDataList.Add(meatDrying.Serialize());
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