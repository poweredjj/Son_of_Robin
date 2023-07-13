﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PlantReproductionData
    {
        public readonly int massNeeded;
        public readonly int massLost;
        public readonly float bioWear;

        public PlantReproductionData(int massNeeded, int massLost, float bioWear)
        {
            this.massNeeded = massNeeded;
            this.massLost = massLost;
            this.bioWear = bioWear;
        }
    }

    public class Plant : BoardPiece
    {
        public float massTakenMultiplier;
        private float occupiedFieldWealth;
        public readonly FruitEngine fruitEngine;
        private int lastFrameProcessed; // for time delta calculation

        public Plant(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            int maxAge, float massTakenMultiplier,
            byte animSize = 0, string animName = "default", float speed = 1, int staysAfterDeath = 800, int generation = 0, Yield yield = null, int maxHitPoints = 1, FruitEngine fruitEngine = null, AllowedDensity allowedDensity = null, LightEngine lightEngine = null, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, name: name, allowedTerrain: allowedTerrain, staysAfterDeath: staysAfterDeath, maxAge: maxAge, generation: generation, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, allowedDensity: allowedDensity, lightEngine: lightEngine, activeState: State.PlantGrowthAndReproduction, soundPack: soundPack)
        {
            this.lastFrameProcessed = this.world == null ? 0 : world.CurrentUpdate;
            this.massTakenMultiplier = massTakenMultiplier;
            this.occupiedFieldWealth = -1f;
            this.fruitEngine = fruitEngine;
            if (this.fruitEngine != null)
            {
                this.fruitEngine.plant = this;
                this.PieceStorage = new PieceStorage(width: this.fruitEngine.maxNumber, height: 1, storagePiece: this, storageType: PieceStorage.StorageType.Fruits, stackLimit: 1);
            }
        }

        private float OccupiedFieldWealth
        {
            get
            {
                if (this.occupiedFieldWealth != -1f) return this.occupiedFieldWealth;

                // calculated once - because occupied field value will not change

                float totalWealth = 0;
                foreach (var kvp in this.pieceInfo.plantBestEnvironment)
                {
                    totalWealth += 255 - Math.Abs(kvp.Value - this.sprite.GetFieldValue(kvp.Key));
                }
                totalWealth /= 255;

                this.occupiedFieldWealth = totalWealth;
                return this.occupiedFieldWealth;
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            if (this.pieceInfo.massTakenMultiplier != this.massTakenMultiplier) pieceData["plant_massTakenMultiplier"] = this.massTakenMultiplier;
            if (this.fruitEngine != null) this.fruitEngine.Serialize(pieceData);

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            if (pieceData.ContainsKey("plant_massTakenMultiplier")) this.massTakenMultiplier = (float)(double)pieceData["plant_massTakenMultiplier"];
            if (this.fruitEngine != null) this.fruitEngine.Deserialize(pieceData);
        }

        public void SimulateUnprocessedGrowth() // for plants that are placed during world creation
        {
            this.lastFrameProcessed -= 60 * 60 * this.world.random.Next(1, 10);
        }

        public void DropSeeds()
        {
            if (this.pieceInfo.plantDropSeedChance == 0 || this.IsBurning) return;

            bool dropSeed = this.world.random.Next(this.pieceInfo.plantDropSeedChance) == 0;
            if (dropSeed)
            {
                BoardPiece seedsPiece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: this.sprite.position, templateName: PieceTemplate.Name.SeedsGeneric, closestFreeSpot: true);

                Seed seeds = (Seed)seedsPiece;
                seeds.PlantToGrow = this.name;
            }
        }

        public bool DropFruit()
        {
            var occupiedSlots = this.PieceStorage.OccupiedSlots;
            if (occupiedSlots.Count == 0)
            {
                new TextWindow(text: "There is nothing left to shake off.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.world.DialogueSound);

                return false;
            }

            if (this.name == PieceTemplate.Name.BananaTree) this.world.HintEngine.Disable(PieceHint.Type.BananaTree);
            if (this.name == PieceTemplate.Name.TomatoPlant) this.world.HintEngine.Disable(PieceHint.Type.TomatoPlant);
            if (this.name == PieceTemplate.Name.CarrotPlant) this.world.HintEngine.Disable(PieceHint.Type.CarrotPlant);
            if (this.name == PieceTemplate.Name.CherryTree || this.name == PieceTemplate.Name.AppleTree) this.world.HintEngine.Disable(PieceHint.Type.FruitTree);

            Yield debrisYield = new Yield(boardPiece: this, debrisTypeList: this.yield.DebrisTypeList);
            debrisYield.DropDebris();
            Sound.QuickPlay(SoundData.Name.DropPlant);

            this.PieceStorage.DropPiecesFromSlot(slot: occupiedSlots[0], addMovement: true);
            if (this.alive && this.PieceStorage.OccupiedSlotsCount == 0) this.sprite.AssignNewName("default"); // swapping from "has_fruits", if the plant has such animation

            return true;
        }

        public override void SM_GrowthAndReproduction()
        {
            int growthSlowdown = Preferences.debugFastPlantGrowth ? 1 : 90; // 1 - fastest, more - slower

            int timeDelta = Math.Max((this.world.CurrentUpdate - this.lastFrameProcessed) / growthSlowdown, 1); // timeDelta must be slowed down, for reasonable growth rate
            this.lastFrameProcessed = this.world.CurrentUpdate;

            float massTaken = this.OccupiedFieldWealth * this.massTakenMultiplier * this.efficiency * 25 * timeDelta;
            if (this.fruitEngine != null)
            {
                if (this.PieceStorage.EmptySlotsCount > 0 || this.world.CurrentUpdate % 100 == 0)
                {
                    this.fruitEngine.AddMass(massTaken / 90); // some mass is "copied" to fruit
                    if (this.Mass > this.pieceInfo.plantReproductionData.massNeeded / 2) this.fruitEngine.TryToConvertMassIntoFruit();
                }
            }

            this.Mass += (-this.pieceInfo.plantMassToBurn * timeDelta) + massTaken;

            while (true)
            {
                bool canReproduce = this.pieceInfo.plantMaxExistingNumber == 0 || this.world.pieceCountByName[this.name] < this.pieceInfo.plantMaxExistingNumber;
                if (canReproduce && this.Mass > this.pieceInfo.plantReproductionData.massNeeded + this.pieceInfo.startingMass)
                {
                    BoardPiece newPlant = PieceTemplate.Create(world: world, templateName: this.name, generation: this.generation + 1);
                    if (this.sprite.allowedTerrain.HasBeenChanged) newPlant.sprite.allowedTerrain.CopyTerrainFromTemplate(this.sprite.allowedTerrain);
                    newPlant.PlaceOnBoard(randomPlacement: false, position: this.sprite.position);

                    if (newPlant.sprite.IsOnBoard)
                    {
                        this.Mass -= this.pieceInfo.plantReproductionData.massLost;
                        this.bioWear += this.pieceInfo.plantReproductionData.bioWear;
                    }
                }

                if (!canReproduce || !this.world.CanProcessMoreNonPlantsNow) break;
            }

            this.GrowOlder(timeDelta: timeDelta);
        }
    }
}