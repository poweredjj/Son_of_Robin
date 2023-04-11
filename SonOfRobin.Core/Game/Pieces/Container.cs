﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Container : BoardPiece
    {
        public Container(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, byte storageWidth, byte storageHeight, string readableName, string description, Category category,
            byte animSize = 0, string animName = "open", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, Yield appearDebris = null, int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, category: category, activeState: State.Empty, movesWhenDropped: false, soundPack: soundPack, appearDebris: appearDebris)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.ChestOpen));
            this.soundPack.AddAction(action: PieceSoundPack.Action.Close, sound: new Sound(name: SoundData.Name.ChestClose));

            this.boardTask = Scheduler.TaskName.OpenContainer;
            this.PieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, storagePiece: this, storageType: PieceStorage.StorageType.Chest);
        }

        public override bool ShowStatBars
        { get { return true; } }

        public override void DrawStatBar()
        {
            int notEmptySlotsCount = this.PieceStorage.OccupiedSlotsCount;

            if (notEmptySlotsCount > 0)
            {
                new StatBar(label: "", value: notEmptySlotsCount, valueMax: this.PieceStorage.AllSlotsCount, colorMin: new Color(0, 255, 255), colorMax: new Color(0, 128, 255), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.ChestIron].texture);
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
            // data to deserialize here
        }

        public void Open()
        {
            if (this.sprite.animName == "open" || this.sprite.animName == "opening") return;
            this.soundPack.Play(PieceSoundPack.Action.Open);
            this.sprite.AssignNewName(animName: "opening");
        }

        public void Close()
        {
            if (this.PieceStorage.OccupiedSlots.Count == 0 || this.sprite.animName == "closing") return;
            this.soundPack.Play(PieceSoundPack.Action.Close);
            this.sprite.AssignNewName(animName: "closing");
        }
    }
}