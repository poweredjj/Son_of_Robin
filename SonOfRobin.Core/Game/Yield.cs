using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Yield
    {
        public static Dictionary<PieceTemplate.Name, Craft.Recipe> antiCraftRecipes = new() { };
        public static HashSet<PieceTemplate.Name> piecesNotMultipliedByBonus = new() { PieceTemplate.Name.Hole, PieceTemplate.Name.TreeStump };

        public readonly struct DroppedPiece
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

        public List<ParticleEngine.Preset> DebrisTypeList
        { get { return this.debrisTypeList; } }

        private readonly List<ParticleEngine.Preset> debrisTypeList;
        private readonly List<DroppedPiece> firstDroppedPieces; // during hitting the piece
        private readonly List<DroppedPiece> finalDroppedPieces; // after destroying the piece

        public Yield(List<DroppedPiece> firstDroppedPieces, List<DroppedPiece> finalDroppedPieces, List<ParticleEngine.Preset> debrisTypeList)
        {
            this.firstDroppedPieces = firstDroppedPieces;
            this.finalDroppedPieces = finalDroppedPieces;
            this.debrisTypeList = debrisTypeList == null ? new List<ParticleEngine.Preset>() : debrisTypeList;
        }

        public Yield(List<DroppedPiece> firstDroppedPieces, List<DroppedPiece> finalDroppedPieces, ParticleEngine.Preset debrisType)
        {
            this.firstDroppedPieces = firstDroppedPieces;
            this.finalDroppedPieces = finalDroppedPieces;
            this.debrisTypeList = new List<ParticleEngine.Preset> { debrisType };
        }

        public Yield(ParticleEngine.Preset debrisType)
        {
            // for dropping debris only

            this.debrisTypeList = new List<ParticleEngine.Preset> { debrisType };
        }

        public Yield(List<ParticleEngine.Preset> debrisTypeList)
        {
            // for dropping debris only

            this.debrisTypeList = debrisTypeList;
        }

        public int DropFirstPieces(int hitPower, BoardPiece piece)
        {
            int droppedPiecesCount = DropPieces(piece: piece, chanceMultiplier: hitPower / piece.maxHitPoints, droppedPieceList: this.firstDroppedPieces);
            this.DropDebris(piece: piece);
            return droppedPiecesCount;
        }

        public int DropFinalPieces(BoardPiece piece, float chanceMultiplier = 1f, float countMultiplier = 1f)
        {
            int droppedPiecesCount = DropPieces(piece: piece, chanceMultiplier: chanceMultiplier, countMultiplier: countMultiplier, droppedPieceList: this.finalDroppedPieces);
            this.DropDebris(piece: piece);
            return droppedPiecesCount;
        }

        public List<BoardPiece> GetAllPieces(BoardPiece piece)
        {
            var firstPieces = GetPieces(piece: piece, chanceMultiplier: 1f, droppedPieceList: this.firstDroppedPieces);
            var finalPieces = GetPieces(piece: piece, chanceMultiplier: 1f, droppedPieceList: this.finalDroppedPieces);
            return firstPieces.Concat(finalPieces).ToList();
        }

        public void DropDebris(BoardPiece piece, List<ParticleEngine.Preset> debrisTypeListOverride = null, int particlesToEmit = 0)
        {
            if (!piece.sprite.IsInCameraRect) return;
            var debrisTypeListToUse = debrisTypeListOverride == null ? this.debrisTypeList : debrisTypeListOverride;

            foreach (ParticleEngine.Preset debrisType in debrisTypeListToUse)
            {
                switch (debrisType)
                {
                    case ParticleEngine.Preset.DebrisStar:
                        {
                            BoardPiece particleEmitter = PieceTemplate.CreateAndPlaceOnBoard(world: piece.world, position: piece.sprite.position, templateName: PieceTemplate.Name.ParticleEmitterEnding, precisePlacement: true);
                            particleEmitter.sprite.AssignNewPackage(AnimData.PkgName.WhiteSpotLayerZero);
                            ParticleEngine.TurnOn(sprite: particleEmitter.sprite, preset: ParticleEngine.Preset.DebrisStar, duration: 6, update: true, particlesToEmit: particlesToEmit);
                            break;
                        }

                    case ParticleEngine.Preset.DebrisSoot:
                        ParticleEngine.TurnOn(sprite: piece.sprite, preset: ParticleEngine.Preset.DebrisSoot, duration: 1, update: true, particlesToEmit: particlesToEmit);
                        break;

                    case ParticleEngine.Preset.DebrisHeart:
                        {
                            BoardPiece particleEmitter = PieceTemplate.CreateAndPlaceOnBoard(world: piece.world, position: piece.sprite.position, templateName: PieceTemplate.Name.ParticleEmitterEnding, precisePlacement: true);
                            particleEmitter.sprite.AssignNewPackage(AnimData.PkgName.WhiteSpotLayerZero);
                            ParticleEngine.TurnOn(sprite: particleEmitter.sprite, preset: ParticleEngine.Preset.DebrisHeart, duration: 3, update: true, particlesToEmit: particlesToEmit);
                            break;
                        }

                    default:
                        ParticleEngine.TurnOn(sprite: piece.sprite, preset: debrisType, duration: 3, update: true, particlesToEmit: particlesToEmit);
                        break;
                }
            }
        }

        private static List<BoardPiece> GetPieces(BoardPiece piece, float chanceMultiplier, List<DroppedPiece> droppedPieceList, float countMultiplier = 1f)
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
                    chanceMultiplier /= 6f;
                    // MessageLog.AddMessage( message: $"Plant {this.mainPiece.readableName} is not adult, multiplier changed to {multiplier}."); // for testing
                }
            }

            World world = World.GetTopWorld(); // do not store world, check it every time (otherwise changing world will make creating pieces impossible)
            Random random = BoardPiece.Random;

            var piecesList = new List<BoardPiece>();

            foreach (DroppedPiece droppedPiece in droppedPieceList)
            {
                if (random.Next(100) <= droppedPiece.chanceToDrop * chanceMultiplier)
                {                  
                    int dropCount = random.Next(droppedPiece.minNumberToDrop, droppedPiece.maxNumberToDrop + extraDroppedPieces + 1);
                    int bonusCount = 0;
                    if (countMultiplier != 1 && !piecesNotMultipliedByBonus.Contains(droppedPiece.pieceName))
                    {
                        int originalDropCount = dropCount;
                        dropCount = (int)Math.Max(dropCount * countMultiplier, 1);
                        if (dropCount > originalDropCount)
                        {
                            bonusCount = dropCount - originalDropCount;
                            string countText = bonusCount > 1 ? $" x{bonusCount}" : "";
                            MessageLog.AddMessage( message: $"Bonus drop - {PieceInfo.GetInfo(droppedPiece.pieceName).readableName}{countText}", color: Color.Yellow);
                            Sound.QuickPlay(name: SoundData.Name.BonusItem);
                        }
                    }

                    int effectDuration = (int)(60 * 2.5f);

                    for (int i = 0; i < dropCount; i++)
                    {
                        BoardPiece newPiece = PieceTemplate.CreatePiece(world: world, templateName: droppedPiece.pieceName);
                        if (bonusCount > 0)
                        {
                            newPiece.sprite.effectCol.AddEffect(new ColorizeInstance(color: Color.Orange, framesLeft: effectDuration, fadeFramesLeft: effectDuration));
                            bonusCount--;
                        }

                        piecesList.Add(newPiece);
                    }
                }
            }

            return piecesList;
        }

        private static int DropPieces(BoardPiece piece, float chanceMultiplier, List<DroppedPiece> droppedPieceList, float countMultiplier = 1f)
        {
            int droppedPiecesCount = 0;
            var piecesToDrop = GetPieces(piece: piece, chanceMultiplier: chanceMultiplier, countMultiplier: countMultiplier, droppedPieceList: droppedPieceList);
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
                        yieldPiece.activeSoundPack.Play(PieceSoundPackTemplate.Action.HasAppeared);
                        yieldPiece.pieceInfo.appearDebris?.DropDebris(piece: piece);

                        // if (yieldPiece.GetType() != typeof(Debris)) MessageLog.AddMessage( message: $"{SonOfRobinGame.CurrentUpdate} yield: {yieldPiece.readableName} - {yieldPiece.readableName} dropped.");

                        yieldPiece.sprite.allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll }); // where player can go

                        Vector2 posDiff = Helpers.VectorKeepBelowSetValue(vector: piece.sprite.position - yieldPiece.sprite.position, maxVal: 4f);
                        posDiff += new Vector2(yieldPiece.world.random.Next(-8, 8), yieldPiece.world.random.Next(-8, 8)); // to add a lot of variation
                        yieldPiece.AddPassiveMovement(movement: posDiff * -1 * yieldPiece.world.random.Next(20, 80));
                        break;
                    }
                }
            }

            return droppedPiecesCount;
        }
    }
}