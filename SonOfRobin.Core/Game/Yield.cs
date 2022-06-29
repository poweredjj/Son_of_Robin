using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Yield
    {
        public enum DebrisType { Stone, Wood, Leaf, Blood, Plant, Crystal }
        public static Dictionary<PieceTemplate.Name, Craft.Recipe> antiCraftRecipes = new Dictionary<PieceTemplate.Name, Craft.Recipe> { };
        public struct DroppedPiece
        {
            public readonly PieceTemplate.Name pieceName;
            public readonly int chanceToDrop; // 0 - 100
            public readonly byte maxNumberToDrop;

            public DroppedPiece(PieceTemplate.Name pieceName, int chanceToDrop, byte maxNumberToDrop)
            {
                this.pieceName = pieceName;
                this.chanceToDrop = chanceToDrop;
                this.maxNumberToDrop = maxNumberToDrop;
            }
        }

        private BoardPiece mainPiece;
        private float firstPiecesDivider;
        private readonly List<DebrisType> debrisTypeList;
        private readonly List<DroppedPiece> firstDroppedPieces; // during hitting the piece
        private readonly List<DroppedPiece> finalDroppedPieces; // after destroying the piece

        public Yield(List<DroppedPiece> firstDroppedPieces, List<DroppedPiece> finalDroppedPieces, List<DebrisType> debrisTypeList)
        {
            this.firstDroppedPieces = firstDroppedPieces;
            this.finalDroppedPieces = finalDroppedPieces;
            this.debrisTypeList = debrisTypeList == null ? new List<DebrisType>() : debrisTypeList;
        }

        public Yield(List<DroppedPiece> firstDroppedPieces, List<DroppedPiece> finalDroppedPieces, DebrisType debrisType)
        {
            this.firstDroppedPieces = firstDroppedPieces;
            this.finalDroppedPieces = finalDroppedPieces;
            this.debrisTypeList = new List<DebrisType> { debrisType };
        }

        public void AddPiece(BoardPiece mainPiece)
        {
            // must be added after creating main piece
            if (this.mainPiece != null) throw new ArgumentException($"Cannot add another piece ('{mainPiece.readableName}') to yield.");

            this.mainPiece = mainPiece;
            this.firstPiecesDivider = mainPiece.maxHitPoints;
        }

        public void DropFirstPieces(int hitPower)
        {
            this.DropPieces(multiplier: hitPower / this.firstPiecesDivider, droppedPieceList: this.firstDroppedPieces);
            this.DropDebris();
        }

        public void DropFinalPieces()
        {
            this.DropPieces(multiplier: 1f, droppedPieceList: this.finalDroppedPieces);
            this.DropDebris();
        }

        public void DropDebris()
        {
            if (!this.debrisTypeList.Any()) return; // to speed up
            if (!Preferences.showDebris || !this.mainPiece.world.camera.viewRect.Contains(this.mainPiece.sprite.position)) return; // debris should not be created off-screen

            var debrisList = new List<DroppedPiece> { };

            foreach (DebrisType debrisType in this.debrisTypeList)
            {
                switch (debrisType)
                {
                    case DebrisType.Stone:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisStone, chanceToDrop: 100, maxNumberToDrop: 60));
                        break;

                    case DebrisType.Wood:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisWood, chanceToDrop: 100, maxNumberToDrop: 30));
                        break;

                    case DebrisType.Leaf:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisLeaf, chanceToDrop: 100, maxNumberToDrop: 30));
                        break;

                    case DebrisType.Plant:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisPlant, chanceToDrop: 100, maxNumberToDrop: 15));
                        break;

                    case DebrisType.Crystal:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisCrystal, chanceToDrop: 100, maxNumberToDrop: 20));
                        break;

                    case DebrisType.Blood:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.BloodDrop, chanceToDrop: 100, maxNumberToDrop: 35));
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported debris type - {debrisTypeList}.");
                }
            }

            this.DropPieces(multiplier: 1f, droppedPieceList: debrisList);
        }

        private void DropPieces(float multiplier, List<DroppedPiece> droppedPieceList)
        {
            int extraDroppedPieces = 0;
            if (this.mainPiece.GetType() == typeof(Animal))
            {
                Animal animal = (Animal)this.mainPiece;
                extraDroppedPieces = (int)(animal.MaxMassPercentage * 2);
            }

            World world = World.GetTopWorld(); // do not store world, check it every time (otherwise changing world will make creating pieces impossible)

            int noOfTries = 10;

            foreach (DroppedPiece droppedPiece in droppedPieceList)
            {
                if (world.random.Next(0, 100) <= droppedPiece.chanceToDrop * multiplier)
                {
                    int numberToDrop = world.random.Next(1, droppedPiece.maxNumberToDrop + extraDroppedPieces + 1);

                    for (int i = 0; i < numberToDrop; i++)
                    {
                        for (int j = 0; j < noOfTries; j++)
                        {
                            Sprite.maxDistanceOverride = (j + 1) * 5;

                            BoardPiece yieldPiece = PieceTemplate.CreateOnBoard(world: world, position: this.mainPiece.sprite.position, templateName: droppedPiece.pieceName);

                            if (yieldPiece.sprite.placedCorrectly)
                            {
                                yieldPiece.sprite.allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll }); // where player can go

                                Vector2 posDiff = Helpers.VectorAbsMax(vector: this.mainPiece.sprite.position - yieldPiece.sprite.position, maxVal: 3f);
                                yieldPiece.AddPassiveMovement(movement: posDiff * world.random.Next(-100, -20));
                                break;
                            }
                        }
                    }
                }
            }

        }

    }
}
