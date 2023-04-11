using Microsoft.Xna.Framework;
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
        private readonly Dictionary<Terrain.Name, byte> bestEnvironment;
        public readonly PlantReproductionData reproduction;
        private readonly byte massToBurn;
        public float massTakenMultiplier;
        private readonly int maxExistingNumber;
        private float occupiedFieldWealth;
        public readonly float adultSizeMass;
        public readonly FruitEngine fruitEngine;

        public Plant(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, Dictionary<Terrain.Name, byte> bestEnvironment, int[] maxMassForSize, string readableName, string description, Category category,
            int maxAge, PlantReproductionData reproduction, byte massToBurn, float massTakenMultiplier,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int mass = 1, int staysAfterDeath = 800, int generation = 0, Yield yield = null, int maxHitPoints = 1, FruitEngine fruitEngine = null, Scheduler.TaskName boardTask = Scheduler.TaskName.Empty, AllowedDensity allowedDensity = null, LightEngine lightEngine = null, int maxExistingNumber = 0, PieceSoundPack soundPack = null, float adultSizeMass = 0) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, blocksPlantGrowth: true, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, mass: mass, maxMassForSize: maxMassForSize, staysAfterDeath: staysAfterDeath, maxAge: maxAge, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, boardTask: boardTask, readableName: readableName, description: description, allowedDensity: allowedDensity, category: category, lightEngine: lightEngine, activeState: State.PlantGrowthAndReproduction, soundPack: soundPack)
        {
            this.bestEnvironment = bestEnvironment;
            this.reproduction = reproduction;
            this.massToBurn = massToBurn;
            this.massTakenMultiplier = massTakenMultiplier;
            this.maxExistingNumber = maxExistingNumber;
            this.occupiedFieldWealth = -1f; // to be changed
            this.adultSizeMass = adultSizeMass;
            this.fruitEngine = fruitEngine;
            if (this.fruitEngine != null)
            {
                this.fruitEngine.plant = this;
                this.PieceStorage = new PieceStorage(width: this.fruitEngine.maxNumber, height: 1, storagePiece: this, storageType: PieceStorage.StorageType.Fruits, stackLimit: 1);
            }

            this.sprite.AddFadeInAnim();
        }

        private float OccupiedFieldWealth
        {
            get
            {
                if (this.occupiedFieldWealth != -1f) return this.occupiedFieldWealth;

                // calculated once - because occupied field value will not change

                float totalWealth = 0;
                foreach (var kvp in this.bestEnvironment)
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

            pieceData["plant_massTakenMultiplier"] = this.massTakenMultiplier;
            if (this.fruitEngine != null) this.fruitEngine.Serialize(pieceData);

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.massTakenMultiplier = (float)(double)pieceData["plant_massTakenMultiplier"];
            if (this.fruitEngine != null) this.fruitEngine.Deserialize(pieceData);
        }

        public void DropFruit()
        {
            var occupiedSlots = this.PieceStorage.OccupiedSlots;
            if (occupiedSlots.Count == 0)
            {
                new TextWindow(text: "There is nothing left to shake off.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.world.DialogueSound);

                return;
            }

            if (this.name == PieceTemplate.Name.BananaTree) this.world.HintEngine.Disable(PieceHint.Type.BananaTree);
            if (this.name == PieceTemplate.Name.TomatoPlant) this.world.HintEngine.Disable(PieceHint.Type.TomatoPlant);
            if (this.name == PieceTemplate.Name.CarrotPlant) this.world.HintEngine.Disable(PieceHint.Type.CarrotPlant);
            if (this.name == PieceTemplate.Name.CherryTree || this.name == PieceTemplate.Name.AppleTree) this.world.HintEngine.Disable(PieceHint.Type.FruitTree);

            Yield debrisYield = new Yield(boardPiece: this, debrisTypeList: this.yield.DebrisTypeList);
            debrisYield.DropDebris();
            Sound.QuickPlay(SoundData.Name.DropPlant);

            this.PieceStorage.DropPiecesFromSlot(slot: occupiedSlots[0], addMovement: true);
            if (this.PieceStorage.OccupiedSlotsCount == 0) this.sprite.AssignNewName("default"); // swapping from "has_fruits", if plant has such animation
        }

        public override void SM_GrowthAndReproduction()
        {
            float massTaken = this.OccupiedFieldWealth * this.massTakenMultiplier * this.efficiency * 25;
            if (this.fruitEngine != null)
            {
                if (this.PieceStorage.EmptySlotsCount > 0 || this.world.CurrentUpdate % 100 == 0)
                {
                    this.fruitEngine.AddMass(massTaken / 90); // some mass is "copied" to fruit
                    if (this.Mass > this.reproduction.massNeeded / 2) this.fruitEngine.TryToConvertMassIntoFruit();
                }
            }

            this.Mass += -this.massToBurn + massTaken;

            bool canReproduce = this.maxExistingNumber == 0 || this.world.pieceCountByName[this.name] < this.maxExistingNumber;
            if (canReproduce && this.Mass > this.reproduction.massNeeded + this.startingMass)
            {
                BoardPiece newPlant = PieceTemplate.CreateAndPlaceOnBoard(world: this.world, position: this.sprite.position, templateName: this.name, generation: this.generation + 1);
                if (newPlant.sprite.IsOnBoard)
                {
                    this.Mass -= this.reproduction.massLost;
                    this.bioWear += this.reproduction.bioWear;
                }
            }
        }
    }
}