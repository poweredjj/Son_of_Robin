using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Yield
    {
        public enum DebrisType
        { Stone, Wood, Leaf, Blood, Plant, Crystal, Ceramic, Star, Soot, Heart }

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

        public List<DebrisType> DebrisTypeList
        { get { return this.debrisTypeList; } }

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

        public Yield(DebrisType debrisType)
        {
            // for dropping debris only

            this.debrisTypeList = new List<DebrisType> { debrisType };
        }

        public Yield(List<DebrisType> debrisTypeList)
        {
            // for dropping debris only

            this.debrisTypeList = debrisTypeList;
        }

        public int DropFirstPieces(int hitPower, BoardPiece piece)
        {
            int droppedPiecesCount = this.DropPieces(piece: piece, multiplier: hitPower / piece.maxHitPoints, droppedPieceList: this.firstDroppedPieces);
            this.DropDebris(piece: piece);
            return droppedPiecesCount;
        }

        public int DropFinalPieces(BoardPiece piece, float hitPower = 1f)
        {
            int droppedPiecesCount = this.DropPieces(piece: piece, multiplier: 1f, droppedPieceList: this.finalDroppedPieces, hitPower: hitPower);
            this.DropDebris(piece: piece, hitPower: hitPower);
            return droppedPiecesCount;
        }

        public List<BoardPiece> GetAllPieces(BoardPiece piece)
        {
            var firstPieces = this.GetPieces(piece: piece, multiplier: 1f, droppedPieceList: this.firstDroppedPieces);
            var finalPieces = this.GetPieces(piece: piece, multiplier: 1f, droppedPieceList: this.finalDroppedPieces);
            return firstPieces.Concat(finalPieces).ToList();
        }

        public void DropDebris(BoardPiece piece, bool ignoreProcessingTime = false, List<DebrisType> debrisTypeListOverride = null, float hitPower = 1f)
        {
            var debrisTypeListToUse = debrisTypeListOverride == null ? this.debrisTypeList : debrisTypeListOverride;

            if (!debrisTypeListToUse.Any()) return; // to speed up

            // debris should be created on screen, when there is available CPU time
            if (!Preferences.showDebris ||
                !piece.world.camera.viewRect.Contains(piece.sprite.position) ||
                (!ignoreProcessingTime && !piece.world.CanProcessMoreNonPlantsNow)) return;

            var debrisList = new List<DroppedPiece> { };

            foreach (DebrisType debrisType in debrisTypeListToUse)
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

                    case DebrisType.Soot:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisSoot, chanceToDrop: 100, minNumberToDrop: 8, maxNumberToDrop: 30));
                        break;

                    case DebrisType.Heart:
                        debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisHeart, chanceToDrop: 100, minNumberToDrop: 6, maxNumberToDrop: 6));
                        break;

                    default:
                        throw new ArgumentException($"Unsupported debris type - {debrisType}.");
                }
            }

            this.DropPieces(piece: piece, multiplier: 1f, droppedPieceList: debrisList, hitPower: hitPower);
        }

        private List<BoardPiece> GetPieces(BoardPiece piece, float multiplier, List<DroppedPiece> droppedPieceList)
        {
            Type type = piece.GetType();

            int extraDroppedPieces = 0;
            if (type == typeof(Animal))
            {
                Animal animal = (Animal)piece;
                extraDroppedPieces = (int)(animal.MaxMassPercentage * 2);
            }
            else if (type == typeof(Plant))
            {
                if (piece.Mass < ((Plant)piece).pieceInfo.plantAdultSizeMass)
                {
                    multiplier /= 6f;
                    // MessageLog.AddMessage(msgType: MsgType.User, message: $"Plant {this.mainPiece.readableName} is not adult, multiplier changed to {multiplier}."); // for testing
                }
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

        private int DropPieces(BoardPiece piece, float multiplier, List<DroppedPiece> droppedPieceList, float hitPower = 1f)
        {
            int droppedPiecesCount = 0;
            var piecesToDrop = this.GetPieces(piece: piece, multiplier: multiplier, droppedPieceList: droppedPieceList);
            int noOfTries = 10;

            foreach (BoardPiece yieldPiece in piecesToDrop)
            {
                for (int j = 0; j < noOfTries; j++)
                {
                    yieldPiece.sprite.allowedTerrain = new AllowedTerrain(); // to avoid restricting placement
                    yieldPiece.PlaceOnBoard(randomPlacement: false, position: piece.sprite.position, closestFreeSpot: true);

                    if (yieldPiece.sprite.IsOnBoard)
                    {
                        droppedPiecesCount++;

                        // duplicated in PieceTemplate
                        yieldPiece.soundPack.Play(PieceSoundPack.Action.HasAppeared);
                        if (yieldPiece.pieceInfo.appearDebris != null) yieldPiece.pieceInfo.appearDebris.DropDebris(piece: piece, ignoreProcessingTime: true);

                        // if (yieldPiece.GetType() != typeof(Debris)) MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.CurrentUpdate} yield: {yieldPiece.readableName} - {yieldPiece.readableName} dropped.");

                        yieldPiece.sprite.allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll }); // where player can go

                        Vector2 posDiff = Helpers.VectorAbsMax(vector: piece.sprite.position - yieldPiece.sprite.position, maxVal: 4f);
                        posDiff += new Vector2(yieldPiece.world.random.Next(-8, 8), yieldPiece.world.random.Next(-8, 8)); // to add a lot of variation
                        yieldPiece.AddPassiveMovement(movement: posDiff * -1 * yieldPiece.world.random.Next(20, 80) * hitPower);
                        break;
                    }
                }
            }

            return droppedPiecesCount;
        }
    }
}