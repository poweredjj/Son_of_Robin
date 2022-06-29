using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Yield
    {
        public enum DebrisType { Stone, Wood, Leaf, Blood, Plant, Crystal, Ceramic, Star }
        public static Dictionary<PieceTemplate.Name, Craft.Recipe> antiCraftRecipes = new Dictionary<PieceTemplate.Name, Craft.Recipe> { };
        public struct DroppedPiece
        {
            public readonly PieceTemplate.Name pieceName;
            public readonly int chanceToDrop; // 0 - 100
            public readonly byte minNumberToDrop;
            public readonly byte maxNumberToDrop;

            public DroppedPiece(PieceTemplate.Name pieceName, int chanceToDrop, byte maxNumberToDrop, byte minNumberToDrop = 1)
            {
                this.pieceName = pieceName;
                this.chanceToDrop = chanceToDrop;
                this.minNumberToDrop = minNumberToDrop;
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

        public Yield(DebrisType debrisType, BoardPiece boardPiece = null)
        {
            // for dropping debris only

            this.debrisTypeList = new List<DebrisType> { debrisType };
            if (boardPiece != null) this.AddPiece(boardPiece);
        }

        public Yield(List<DebrisType> debrisTypeList, BoardPiece boardPiece = null)
        {
            // for dropping debris only

            this.debrisTypeList = debrisTypeList;
            if (boardPiece != null) this.AddPiece(boardPiece);
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

        public List<BoardPiece> GetAllPieces()
        {
            var firstPieces = this.GetPieces(multiplier: 1f, droppedPieceList: this.firstDroppedPieces);
            var finalPieces = this.GetPieces(multiplier: 1f, droppedPieceList: this.finalDroppedPieces);
            return firstPieces.Concat(finalPieces).ToList();
        }

        public void DropDebris(bool ignoreProcessingTime = false)
        {
            if (!this.debrisTypeList.Any()) return; // to speed up

            // debris should be created on screen, when there is available CPU time
            if (!Preferences.showDebris ||
                !this.mainPiece.world.camera.viewRect.Contains(this.mainPiece.sprite.position) ||
                (!ignoreProcessingTime && !this.mainPiece.world.CanProcessMoreNonPlantsNow)) return;

            var debrisList = new List<DroppedPiece> { };

            foreach (DebrisType debrisType in this.debrisTypeList)
            {
                switch (debrisType)
                {
                    case DebrisType.Stone:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisStone, chanceToDrop: 100, minNumberToDrop: 30, maxNumberToDrop: 60));
                        break;

                    case DebrisType.Wood:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisWood, chanceToDrop: 100, minNumberToDrop: 15, maxNumberToDrop: 30));
                        break;

                    case DebrisType.Leaf:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisLeaf, chanceToDrop: 100, minNumberToDrop: 15, maxNumberToDrop: 30));
                        break;

                    case DebrisType.Plant:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisPlant, chanceToDrop: 100, minNumberToDrop: 10, maxNumberToDrop: 20));
                        break;

                    case DebrisType.Crystal:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisCrystal, chanceToDrop: 100, minNumberToDrop: 10, maxNumberToDrop: 20));
                        break;

                    case DebrisType.Ceramic:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisCeramic, chanceToDrop: 100, minNumberToDrop: 10, maxNumberToDrop: 20));
                        break;

                    case DebrisType.Blood:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.BloodDrop, chanceToDrop: 100, minNumberToDrop: 15, maxNumberToDrop: 35));
                        break;

                    case DebrisType.Star:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisStar, chanceToDrop: 100, minNumberToDrop: 40, maxNumberToDrop: 70));
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported debris type - {debrisTypeList}.");
                }
            }

            this.DropPieces(multiplier: 1f, droppedPieceList: debrisList);
        }

        private List<BoardPiece> GetPieces(float multiplier, List<DroppedPiece> droppedPieceList)
        {
            int extraDroppedPieces = 0;
            if (this.mainPiece?.GetType() == typeof(Animal))
            {
                Animal animal = (Animal)this.mainPiece;
                extraDroppedPieces = (int)(animal.MaxMassPercentage * 2);
            }

            World world = World.GetTopWorld(); // do not store world, check it every time (otherwise changing world will make creating pieces impossible)
            Random random = BoardPiece.Random;

            var piecesList = new List<BoardPiece>();

            foreach (DroppedPiece droppedPiece in droppedPieceList)
            {
                if (random.Next(0, 100) <= droppedPiece.chanceToDrop * multiplier)
                {
                    int numberToDrop = random.Next(droppedPiece.minNumberToDrop, droppedPiece.maxNumberToDrop + extraDroppedPieces + 1);

                    for (int i = 0; i < numberToDrop; i++)
                    {
                        piecesList.Add(PieceTemplate.Create(world: world, templateName: droppedPiece.pieceName));
                    }
                }
            }

            return piecesList;
        }

        private void DropPieces(float multiplier, List<DroppedPiece> droppedPieceList)
        {
            var piecesToDrop = this.GetPieces(multiplier: multiplier, droppedPieceList: droppedPieceList);
            int noOfTries = 10;

            foreach (BoardPiece yieldPiece in piecesToDrop)
            {
                for (int j = 0; j < noOfTries; j++)
                {
                    yieldPiece.PlaceOnBoard(position: this.mainPiece.sprite.position, closestFreeSpot: true);

                    if (yieldPiece.sprite.IsOnBoard)
                    {
                        //  MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Yield - {yieldPiece.readableName} ID {yieldPiece.id} dropped.");

                        yieldPiece.sprite.allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll }); // where player can go

                        Vector2 posDiff = Helpers.VectorAbsMax(vector: this.mainPiece.sprite.position - yieldPiece.sprite.position, maxVal: 4f);
                        posDiff += new Vector2(yieldPiece.world.random.Next(-8, 8), yieldPiece.world.random.Next(-8, 8)); // to add a lot of variation
                        yieldPiece.AddPassiveMovement(movement: posDiff * -1 * yieldPiece.world.random.Next(20, 80));
                        break;
                    }
                }
            }
        }

    }
}
