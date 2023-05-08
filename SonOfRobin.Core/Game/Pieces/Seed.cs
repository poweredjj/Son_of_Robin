﻿using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Seed : BoardPiece
    {
        private PieceTemplate.Name plantToGrow;

        public Seed(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, float fireAffinity,
            byte animSize = 0, string animName = "default", bool blocksMovement = false, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, int generation = 0, int mass = 1, bool rotatesWhenDropped = true, byte stackSize = 1) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: null, generation: generation, canBePickedUp: true, mass: mass, toolbarTask: Scheduler.TaskName.Plant, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, category: Category.Indestructible, activeState: State.Empty, fireAffinity: fireAffinity, stackSize: stackSize)
        {
        }

        public bool PlantToGrowHasBeenAssigned
        { get { return this.plantToGrow != PieceTemplate.Name.Empty; } }

        public PieceTemplate.Name PlantToGrow
        {
            get
            {
                if (!this.PlantToGrowHasBeenAssigned) throw new ArgumentException("PlantToGrow has not been assigned.");
                return this.plantToGrow;
            }
            set
            {
                if (this.PlantToGrowHasBeenAssigned) throw new ArgumentException($"Cannot assign {value}, because plant name has already been assigned.");
                this.plantToGrow = value;
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["seed_plantToGrow"] = this.plantToGrow;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            if (pieceData.ContainsKey("seed_plantToGrow")) // for compatibility with older saves
            {
                this.plantToGrow = (PieceTemplate.Name)(Int64)pieceData["seed_plantToGrow"];
            }
        }
    }
}