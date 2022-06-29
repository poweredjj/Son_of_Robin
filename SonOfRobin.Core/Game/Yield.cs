using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Yield
    {
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

        private readonly World world;
        private BoardPiece mainPiece;
        private float firstPiecesMultiplier;
        private readonly List<DroppedPiece> firstDroppedPieces; // during hitting the piece
        private readonly List<DroppedPiece> finalDroppedPieces; // after destroying the piece

        public Yield(World world, List<DroppedPiece> firstDroppedPieces, List<DroppedPiece> finalDroppedPieces)
        {
            this.world = world;
            this.firstDroppedPieces = firstDroppedPieces;
            this.finalDroppedPieces = finalDroppedPieces;
        }

        public void AddPiece(BoardPiece mainPiece)
        {
            // must be added after creating main piece
            this.mainPiece = mainPiece;
            this.firstPiecesMultiplier = 1 / mainPiece.maxHitPoints;
        }

        public void DropFirstPieces(int hitPower)
        { this.DropPieces(multiplier: this.firstPiecesMultiplier * hitPower, droppedPieceList: this.firstDroppedPieces); }

        public void DropFinalPieces()
        { this.DropPieces(multiplier: 1, droppedPieceList: this.finalDroppedPieces); }

        private void DropPieces(float multiplier, List<DroppedPiece> droppedPieceList)
        {
            foreach (DroppedPiece droppedPiece in droppedPieceList)
            {
                if (this.world.random.Next(0, 101) <= droppedPiece.chanceToDrop * multiplier)
                {
                    int numberToDrop = this.world.random.Next(1, droppedPiece.maxNumberToDrop + 1);

                    for (int i = 0; i < numberToDrop; i++)
                    {
                        BoardPiece yieldPiece = PieceTemplate.CreateOnBoard(world: this.world, position: this.mainPiece.sprite.position, templateName: droppedPiece.pieceName);

                        if (yieldPiece.sprite.placedCorrectly)
                        {
                            yieldPiece.sprite.MoveToClosestFreeSpot(startPosition: this.mainPiece.sprite.position);

                            if (yieldPiece.GetType() == typeof(Collectible))
                            {
                                Collectible collectible = (Collectible)yieldPiece;
                                Vector2 passiveMovement = (this.mainPiece.sprite.position - collectible.sprite.position) * -1 * this.world.random.Next(2, 20);
                                collectible.AddPassiveMovement(movement: passiveMovement);
                            }

                            var backlight = PieceTemplate.CreateOnBoard(world: world, position: yieldPiece.sprite.position, templateName: PieceTemplate.Name.Backlight);
                            new Tracking(world: world, targetSprite: yieldPiece.sprite, followingSprite: backlight.sprite, targetXAlign: XAlign.Center, targetYAlign: YAlign.Center);
                        }

                    }
                }
            }

        }

    }
}
