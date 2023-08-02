using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MeatDryingRack : BoardPiece
    {
        private static readonly List<PieceTemplate.Name> rawMeatNames = new List<PieceTemplate.Name> { PieceTemplate.Name.MeatRawRegular, PieceTemplate.Name.MeatRawPrime };

        private int dryingStartFrame;
        private int dryingDoneFrame;
        public bool IsOn { get; private set; }

        private TimeSpan TimeToFinishDrying
        { get { return TimeSpan.FromSeconds((int)Math.Ceiling((float)(this.dryingDoneFrame - (float)this.world.CurrentUpdate) / 60f)); } }

        public MeatDryingRack(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty, soundPack: soundPack)
        {
            this.PieceStorage = new PieceStorage(width: 2, height: 2, storagePiece: this, storageType: PieceStorage.StorageType.Drying);
            this.ConfigureStorage();
        }

        private void ConfigureStorage()
        {
            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.allowedPieceNames = rawMeatNames.ToList();
                slot.allowedPieceNames.Add(PieceTemplate.Name.MeatDried);
                slot.stackLimit = 1;
            }
        }

        public void StartDrying()
        {
            // TODO add code
        }

        public void ShowDryingProgress()
        {
            new TextWindow(text: $"Drying will be done in {TimeSpanToString(this.TimeToFinishDrying)}.", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(newAnimName: "on");
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);
            this.soundPack.Play(PieceSoundPack.Action.IsOn);
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.BloodDripping, duration: 3 * 60);
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(newAnimName: "off");
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.soundPack.Play(PieceSoundPack.Action.TurnOff);
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["dryingRack_dryingStartFrame"] = this.dryingStartFrame;
            pieceData["dryingRack_dryingDoneFrame"] = this.dryingDoneFrame;
            pieceData["dryingRack_IsOn"] = this.IsOn;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.dryingStartFrame = (int)(Int64)pieceData["dryingRack_dryingStartFrame"];
            this.dryingDoneFrame = (int)(Int64)pieceData["dryingRack_dryingDoneFrame"];
            this.IsOn = (bool)pieceData["dryingRack_IsOn"];
            this.ConfigureStorage();
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1)) timeLeftString = $"{timeSpan.TotalSeconds} s";
            else timeLeftString = timeSpan.ToString("mm\\:ss");

            return timeLeftString;
        }
    }
}