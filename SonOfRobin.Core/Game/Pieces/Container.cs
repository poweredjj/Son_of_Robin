using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Container : BoardPiece
    {
        public readonly bool pieceStorageIsGlobal;

        public Container(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, byte storageWidth, byte storageHeight, string readableName, string description, Category category,
            byte animSize = 0, string animName = "open", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, Yield appearDebris = null, int maxHitPoints = 1, PieceSoundPack soundPack = null, bool pieceStorageIsGlobal = false) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, category: category, activeState: State.Empty, soundPack: soundPack, appearDebris: appearDebris, boardTask: Scheduler.TaskName.OpenContainer)
        {
            this.pieceStorageIsGlobal = pieceStorageIsGlobal;

            this.soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.ChestOpen));
            this.soundPack.AddAction(action: PieceSoundPack.Action.Close, sound: new Sound(name: SoundData.Name.ChestClose));

            this.PieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, storagePiece: this, storageType: PieceStorage.StorageType.Chest);
        }

        public override PieceStorage PieceStorage // makes accessing GlobalChestStorage possible
        {
            get
            {
                // "alive" check is needed when destroying chest (also during crafting, when destroying Player.simulatedPieceToBuild)
                if (this.pieceStorageIsGlobal && this.world?.Player?.GlobalChestStorage != null && this.alive) return this.world.Player.GlobalChestStorage;
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
                new StatBar(label: "", value: notEmptySlotsCount, valueMax: this.PieceStorage.AllSlotsCount, colorMin: new Color(0, 255, 255), colorMax: new Color(0, 128, 255), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.ChestIron].texture);
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

            if (pieceStorageIsGlobal)
            {
                // serialization of global content is unwanted (cloned items that will fall out when destroying a global chest) and replacing deserialized contents is needed
                base.PieceStorage.DestroyAllPieces();
            }
        }

        public void Open()
        {
            if (this.sprite.AnimName == "open") return; // "opening" animation is not used, because it won't complete before opening inventory
            this.soundPack.Play(PieceSoundPack.Action.Open);
            this.sprite.AssignNewName(newAnimName: "open");
            if (this.PieceStorage.storageType == PieceStorage.StorageType.Chest) new RumbleEvent(force: 0.22f, bigMotor: false, smallMotor: true, fadeInSeconds: 0.45f, durationSeconds: 0, fadeOutSeconds: 0);
        }

        public void Close()
        {
            if (this.PieceStorage.OccupiedSlots.Count == 0 || this.sprite.AnimName == "closing") return;
            this.soundPack.Play(PieceSoundPack.Action.Close);
            this.sprite.AssignNewName(newAnimName: "closing");
            if (this.PieceStorage.storageType == PieceStorage.StorageType.Chest) new RumbleEvent(force: 1f, bigMotor: true, fadeInSeconds: 0, durationSeconds: 0, fadeOutSeconds: 0.15f);
        }
    }
}