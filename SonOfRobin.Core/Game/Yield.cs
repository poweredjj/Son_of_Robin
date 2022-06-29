using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Yield
    {
        public enum DebrisType { None, Stone, Wood, Blood }

        public static Dictionary<PieceTemplate.Name, Yield> antiCraft = new Dictionary<PieceTemplate.Name, Yield> { };

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
        private readonly DebrisType debrisType;
        private readonly List<DroppedPiece> firstDroppedPieces; // during hitting the piece
        private readonly List<DroppedPiece> finalDroppedPieces; // after destroying the piece

        public Yield(List<DroppedPiece> firstDroppedPieces, List<DroppedPiece> finalDroppedPieces, DebrisType debrisType = DebrisType.None)
        {
            this.firstDroppedPieces = firstDroppedPieces;
            this.finalDroppedPieces = finalDroppedPieces;
            this.debrisType = debrisType;
        }

        public void AddPiece(BoardPiece mainPiece)
        {
            // must be added after creating main piece
            this.mainPiece = mainPiece;
            this.firstPiecesDivider = mainPiece.maxHitPoints;
        }

        public void DropFirstPieces(int hitPower)
        {
            this.DropDebris();
            this.DropPieces(multiplier: hitPower / this.firstPiecesDivider, droppedPieceList: this.firstDroppedPieces);
        }

        public void DropFinalPieces()
        {
            this.DropDebris();
            this.DropPieces(multiplier: 1, droppedPieceList: this.finalDroppedPieces);
        }

        public void DropDebris()
        {
            if (this.debrisType == DebrisType.None) return; // to speed up
            if (!this.mainPiece.world.camera.viewRect.Contains(this.mainPiece.sprite.position)) return; // debris should not be created off-screen

            var debrisList = new List<DroppedPiece> { };

            switch (this.debrisType)
            {
                case DebrisType.None:
                    return;

                case DebrisType.Stone:
                    debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisStone, chanceToDrop: 400, maxNumberToDrop: 60));
                    break;

                case DebrisType.Wood:
                    debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.DebrisWood, chanceToDrop: 400, maxNumberToDrop: 30));
                    break;

                case DebrisType.Blood:
                    debrisList.Add(new DroppedPiece(pieceName: PieceTemplate.Name.BloodDrop, chanceToDrop: 400, maxNumberToDrop: 35));
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported debris type - {debrisType}.");
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

            foreach (DroppedPiece droppedPiece in droppedPieceList)
            {
                if (world.random.Next(0, 100) <= droppedPiece.chanceToDrop * multiplier)
                {
                    int numberToDrop = world.random.Next(1, droppedPiece.maxNumberToDrop + 1 + extraDroppedPieces);

                    for (int i = 0; i < numberToDrop; i++)
                    {
                        BoardPiece yieldPiece = PieceTemplate.CreateOnBoard(world: world, position: this.mainPiece.sprite.position, templateName: droppedPiece.pieceName);

                        if (yieldPiece.sprite.placedCorrectly)
                        {
                            yieldPiece.sprite.MoveToClosestFreeSpot(startPosition: this.mainPiece.sprite.position);

                            Vector2 passiveMovement = (this.mainPiece.sprite.position - yieldPiece.sprite.position) * -1 * world.random.Next(3, 40);
                            yieldPiece.AddPassiveMovement(movement: passiveMovement);
                        }

                    }
                }
            }

        }

    }
}
