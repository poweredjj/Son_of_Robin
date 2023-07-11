﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FruitEngine
    {
        public readonly byte maxNumber;
        public readonly bool hiddenFruits;
        public readonly float oneFruitTargetMass;
        private readonly float yOffsetPercent; // -1 to 1
        private readonly float xOffsetPercent; // -1 to 1
        private readonly float areaWidthPercent; // 0 to 1
        private readonly float areaHeightPercent; // 0 to 1
        public readonly PieceTemplate.Name fruitName;
        public Plant plant;
        private float currentMass;

        public FruitEngine(byte maxNumber, float oneFruitMass, PieceTemplate.Name fruitName, float xOffsetPercent = 0, float yOffsetPercent = 0, float areaWidthPercent = 1f, float areaHeightPercent = 1f, bool hiddenFruits = false)
        {
            this.maxNumber = maxNumber;
            this.hiddenFruits = hiddenFruits;
            this.oneFruitTargetMass = oneFruitMass;
            this.xOffsetPercent = xOffsetPercent;
            this.yOffsetPercent = yOffsetPercent;
            this.areaWidthPercent = areaWidthPercent;
            this.areaHeightPercent = areaHeightPercent;
            this.fruitName = fruitName;
            this.currentMass = 0;
            this.plant = null; // to be updated after creating the plant
        }

        private bool FruitCanBeAdded
        { get { return this.currentMass > this.oneFruitTargetMass && this.plant.Mass >= this.plant.pieceInfo.adultSizeMass; } }

        public int MaxAreaWidth
        { get { return (int)((float)this.plant.sprite.GfxRect.Width * (float)this.areaWidthPercent * 0.5f); } }

        public int MaxAreaHeight
        { get { return (int)((float)this.plant.sprite.GfxRect.Height * (float)this.areaHeightPercent * 0.5f); } }

        public float XOffset
        { get { return (float)this.plant.sprite.GfxRect.Width * (float)this.xOffsetPercent; } }

        public float YOffset
        { get { return (float)this.plant.sprite.GfxRect.Height * (float)this.yOffsetPercent; } }

        private Vector2 FruitCenterPos
        { get { return new Vector2(this.plant.sprite.GfxRect.Center.X + this.XOffset, this.plant.sprite.GfxRect.Center.Y + this.YOffset); } }

        public Rectangle FruitAreaRect
        {
            get
            {
                Vector2 fruitCenterPos = this.FruitCenterPos;
                int maxAreaWidth = this.MaxAreaWidth;
                int maxAreaHeight = this.MaxAreaHeight;

                return new Rectangle((int)fruitCenterPos.X - maxAreaWidth, (int)fruitCenterPos.Y - maxAreaHeight, maxAreaWidth * 2, maxAreaHeight * 2);
            }
        }

        public void Serialize(Dictionary<string, Object> pieceData)
        {
            pieceData["fruitEngine_currentMass"] = this.currentMass;
        }

        public void Deserialize(Dictionary<string, Object> pieceData)
        {
            this.currentMass = (float)(double)pieceData["fruitEngine_currentMass"];
            this.PutAllFruitsOnBoardAgain();
        }

        public void AddMass(float mass)
        {
            this.currentMass += mass;
        }

        public void TryToConvertMassIntoFruit()
        {
            if (this.FruitCanBeAdded) this.AddFruit();
        }

        private void AddFruit()
        {
            BoardPiece fruitPiece = PieceTemplate.Create(templateName: this.fruitName, world: this.plant.world);

            if (this.plant.PieceStorage.FindCorrectSlot(piece: fruitPiece) != null)
            {
                this.plant.PieceStorage.AddPiece(piece: fruitPiece, dropIfDoesNotFit: true, addMovement: true);
                this.SetFruitPos(fruit: fruitPiece);
            }
            // if there's no room for more fruits - new one should just go to waste, resetting currentMass (to avoid adding fruit every frame from now on)
            this.currentMass = 0; // resetting whether or not new fruit can fit

            if (this.plant.sprite.CheckIfAnimNameExists("has_fruits")) this.plant.sprite.AssignNewName("has_fruits");
        }

        public void SetFruitPos(BoardPiece fruit)
        {
            int maxAreaWidth = this.MaxAreaWidth;
            int maxAreaHeight = this.MaxAreaHeight;

            Vector2 fruitPos = this.FruitCenterPos;
            fruitPos.X += plant.world.random.Next(-maxAreaWidth, maxAreaWidth);
            fruitPos.Y += plant.world.random.Next(-maxAreaHeight, maxAreaHeight);

            PutFruitOnBoard(fruit: fruit, position: fruitPos);
        }

        private static void PutFruitOnBoard(BoardPiece fruit, Vector2 position)
        {
            // Placing the fruit on board (to allow drawing), but not on the grid (to prevent direct interaction).

            fruit.sprite.PlaceOnBoard(randomPlacement: false, position: position, ignoreCollisions: true);
            Grid.RemoveSprite(fruit.sprite); // the fruit should be on board, but not on the grid itself (to prevent direct interaction)
        }

        private void PutAllFruitsOnBoardAgain()
        {
            foreach (BoardPiece fruit in this.plant.PieceStorage.GetAllPieces())
            {
                PutFruitOnBoard(fruit: fruit, position: fruit.sprite.position);
            }
        }

        public void SetAllFruitPosAgain()
        {
            foreach (BoardPiece fruit in this.plant.PieceStorage.GetAllPieces())
            {
                this.SetFruitPos(fruit: fruit);
            }
        }
    }
}