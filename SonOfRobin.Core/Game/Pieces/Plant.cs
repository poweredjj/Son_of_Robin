using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct PlantReproductionData
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
        private readonly Dictionary<TerrainName, byte> bestEnvironment;
        private readonly PlantReproductionData reproduction;
        private readonly byte massToBurn;
        private readonly float massTakenMultiplier;
        private readonly float occupiedFieldWealth;
        public readonly FruitData fruitData;
        private float fruitMass;

        public Plant(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<TerrainName, byte> bestEnvironment, Dictionary<byte, int> maxMassBySize,
            int maxAge, PlantReproductionData reproduction, byte massToBurn, float massTakenMultiplier,
            byte animSize = 0, string animName = "default", float speed = 1, bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int mass = 1, int staysAfterDeath = 800, int generation = 0, Yield yield = null, int maxHitPoints = 1, FruitData fruitData = null, Scheduler.ActionName boardAction = Scheduler.ActionName.Empty) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, speed: speed, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, checksFullCollisions: true, mass: mass, maxMassBySize: maxMassBySize, staysAfterDeath: staysAfterDeath, maxAge: maxAge, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, boardAction: boardAction)
        {
            this.activeState = State.PlantGrowthAndReproduction;
            this.bestEnvironment = bestEnvironment;
            this.reproduction = reproduction;
            this.massToBurn = massToBurn;
            this.massTakenMultiplier = massTakenMultiplier;
            this.occupiedFieldWealth = this.GetOccupiedFieldWealth(); // calculated once, because a field value doesn't change
            this.fruitData = fruitData;
            this.fruitMass = 0;
            if (this.fruitData != null)
            {
                this.pieceStorage = new PieceStorage(width: this.fruitData.maxNumber, height: 1, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Fruits, stackLimit: 1);
            }

        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["plant_fruitMass"] = this.fruitMass;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);

            this.fruitMass = (float)pieceData["plant_fruitMass"];
        }

        private float GetOccupiedFieldWealth()
        {
            float totalWealth = 0;
            foreach (var kvp in this.bestEnvironment)
            { totalWealth += 255 - Math.Abs(kvp.Value - this.sprite.GetFieldValue(kvp.Key)); }
            return totalWealth / 255;
        }

        public void DropFruit()
        {
            var occupiedSlots = this.pieceStorage.OccupiedSlots;
            if (occupiedSlots.Count == 0) return;

            this.pieceStorage.DropTopPieceFromSlot(slot: occupiedSlots[0], addMovement: true);
        }

        public override void SM_GrowthAndReproduction()
        {
            float massTaken = this.occupiedFieldWealth * this.massTakenMultiplier * this.efficiency * 25;
            if (this.fruitData != null)
            {
                if (this.pieceStorage.EmptySlotsCount > 0 || this.world.random.Next(0, 100) == 0)
                {
                    this.fruitMass += massTaken / 90; // some mass is "copied" to fruit
                    if (this.Mass > this.reproduction.massNeeded / 2 && this.fruitMass > this.fruitData.oneFruitMass)
                    {
                        this.AddFruit();
                        this.fruitMass = 0;
                    }
                }
            }

            this.Mass += -this.massToBurn + massTaken;

            if (this.Mass > this.reproduction.massNeeded + this.startingMass)
            {
                BoardPiece newPlant = PieceTemplate.CreateOnBoard(world: this.world, position: this.sprite.position, templateName: this.name, generation: this.generation + 1);
                if (newPlant.sprite.placedCorrectly)
                {
                    this.Mass -= this.reproduction.massLost;
                    this.bioWear += this.reproduction.bioWear;
                }
            }
        }

        private void AddFruit()
        {
            var fruit = (Fruit)PieceTemplate.CreateOffBoard(templateName: this.fruitData.fruitName, world: this.world);
            fruit.spawnerName = this.name;

            if (this.pieceStorage.FindCorrectSlot(piece: fruit) == null)
            {
                List<StorageSlot> notEmptySlots = this.pieceStorage.NotEmptySlots;
                StorageSlot slot = notEmptySlots[world.random.Next(0, notEmptySlots.Count)];
                this.pieceStorage.RemoveAllPiecesFromSlot(slot: slot, dropToTheGround: true);
            }

            this.pieceStorage.AddPiece(piece: fruit, dropIfDoesNotFit: true, addMovement: true);
            this.SetFruitPos(fruit);
        }

        private void SetFruitPos(BoardPiece fruit)
        {
            int maxAreaWidth = (int)((float)this.sprite.gfxRect.Width * (float)this.fruitData.areaWidthPercent * 0.5f);
            int maxAreaHeight = (int)((float)this.sprite.gfxRect.Height * (float)this.fruitData.areaHeightPercent * 0.5f);
            float xOffset = ((float)this.sprite.gfxRect.Width * (float)this.fruitData.xOffsetPercent);
            float yOffset = ((float)this.sprite.gfxRect.Height * (float)this.fruitData.yOffsetPercent);

            Vector2 fruitPos = new Vector2(
                this.sprite.gfxRect.Center.X + xOffset + this.world.random.Next(-maxAreaWidth, maxAreaWidth),
                this.sprite.gfxRect.Center.Y + yOffset + this.world.random.Next(-maxAreaHeight, maxAreaHeight));

            // grid location should not be updated, because it would put the fruit on the board
            fruit.sprite.SetNewPosition(newPos: fruitPos, ignoreCollisions: true, updateGridLocation: false);
        }

        public void SetAllFruitPosAgain()
        {
            foreach (BoardPiece fruit in this.pieceStorage.GetAllPieces())
            { this.SetFruitPos(fruit); }
        }


    }

}
