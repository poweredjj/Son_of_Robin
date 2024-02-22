using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Container : BoardPiece
    {
        public Container(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, byte storageWidth, byte storageHeight, string readableName, string description,
            byte animSize = 0, string animName = "open", int maxHitPoints = 1, HashSet<PieceTemplate.Name> allowedPieceNames = null, PieceStorage.StorageType storageType = PieceStorage.StorageType.Chest) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, activeState: State.Empty)
        {
            this.PieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, storagePiece: this, storageType: storageType, allowedPieceNames: allowedPieceNames);
        }

        public override PieceStorage PieceStorage // makes accessing GlobalChestStorage possible
        {
            get
            {
                if (this.pieceInfo != null &&
                    this.pieceInfo.containerStorageIsGlobal &&
                    this.world?.Player?.GlobalChestStorage != null
                    && this.alive) // "alive" check is needed when destroying chest (also during crafting, when destroying Player.simulatedPieceToBuild)
                    return this.world.Player.GlobalChestStorage;

                return base.PieceStorage;
            }
        }

        public override bool ShowStatBars
        { get { return true; } }

        public override void DrawStatBar()
        {
            int notEmptySlotsCount = this.PieceStorage.OccupiedSlotsCount;

            if (notEmptySlotsCount > 0)
            {
                new StatBar(label: "", value: notEmptySlotsCount, valueMax: this.PieceStorage.AllSlotsCount, colorMin: new Color(0, 255, 255), colorMax: new Color(0, 128, 255), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, image: AnimData.GetImageObj(AnimData.PkgName.ChestIron));
            }

            base.DrawStatBar();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            // data to serialize here
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            if (this.pieceInfo.containerStorageIsGlobal)
            {
                // serialization of global content is unwanted (cloned items that will fall out when destroying a global chest) and replacing deserialized contents is needed
                base.PieceStorage.DestroyAllPieces();
            }
        }

        public void Open()
        {
            if (this.sprite.AnimName == "open") return; // "opening" animation is not used, because it won't complete before opening inventory
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Open, ignore3D: true);
            this.sprite.AssignNewName(newAnimName: "open");
            if (this.PieceStorage.storageType == PieceStorage.StorageType.Chest) new RumbleEvent(force: 0.22f, bigMotor: false, smallMotor: true, fadeInSeconds: 0.45f, durationSeconds: 0, fadeOutSeconds: 0);
        }

        public void Close()
        {
            if (this.PieceStorage.OccupiedSlots.Count == 0 || this.sprite.AnimName == "closing") return;
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.Close, ignore3D: true);
            this.sprite.AssignNewName(newAnimName: "closing");
            if (this.PieceStorage.storageType == PieceStorage.StorageType.Chest) new RumbleEvent(force: 1f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.15f);
        }
    }
}