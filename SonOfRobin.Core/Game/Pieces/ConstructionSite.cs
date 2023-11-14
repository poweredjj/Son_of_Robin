using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class ConstructionSite : BoardPiece
    {
        private int currentLevel;
        private readonly int maxLevel;
        private readonly Dictionary<int, Dictionary<PieceTemplate.Name, int>> ingredientsForLevels;

        public ConstructionSite(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, int maxLevel,
            byte animSize = 0) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty)
        {
            this.currentLevel = 0;
            this.maxLevel = maxLevel;

            this.CreateStorage();
            this.ConfigureStorage();
        }

        private StorageSlot ConstructTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        private void CreateStorage()
        {
            byte storageWidth = (byte)(this.ingredientSpace + 1);
            storageWidth = Math.Max(storageWidth, (byte)3);
            byte storageHeight = 2;

            this.PieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, storagePiece: this, storageType: PieceStorage.StorageType.Construction);
        }

        private void ConfigureStorage()
        {
            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot flameTriggerSlot = this.ConstructTriggerSlot;

            flameTriggerSlot.locked = false;
            flameTriggerSlot.hidden = false;
            if (flameTriggerSlot.IsEmpty) flameTriggerSlot.AddPiece(PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.CookingTrigger, world: this.world));
            flameTriggerSlot.locked = true;

            StorageSlot mealSlot = this.MealSlot;
            mealSlot.locked = false;
            mealSlot.hidden = false;
            mealSlot.allowedPieceNames = new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Meal };
            mealSlot.label = "meal";

            StorageSlot fuelSlot = this.FuelSlot;
            fuelSlot.locked = false;
            fuelSlot.hidden = false;
            fuelSlot.allowedPieceNames = fuelNames;
            fuelSlot.label = "fuel";
            fuelSlot.stackLimit = 255;

            for (int x = 0; x < this.ingredientSpace; x++)
            {
                StorageSlot ingredientSlot = this.PieceStorage.GetSlot(x + 1, 1);
                ingredientSlot.locked = false;
                ingredientSlot.hidden = false;
                ingredientSlot.label = "ingredient";
                ingredientSlot.allowedPieceNames = ingredientNames;
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["constructionSite_currentLevel"] = this.currentLevel;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.currentLevel = (int)(Int64)pieceData["constructionSite_currentLevel"];

            this.ConfigureStorage();
        }

        public override void DrawStatBar()
        {
            if (this.world.CurrentUpdate < this.cookingDoneFrame)
            {
                // new StatBar(label: "", value: cookingCurrentFrame, valueMax: cookingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.croppedFramesForPkgs[AnimData.PkgName.Flame].texture);
            }

            base.DrawStatBar();
        }
    }
}