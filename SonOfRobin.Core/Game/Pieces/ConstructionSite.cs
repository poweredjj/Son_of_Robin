using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class ConstructionSite : BoardPiece
    {
        private int currentLevel;
        private readonly int maxLevel;
        private readonly Dictionary<int, Dictionary<PieceTemplate.Name, int>> ingredientsForLevels;

        private Dictionary<PieceTemplate.Name, int> CurrentIngredientsDict { get { return this.ingredientsForLevels[this.currentLevel]; } }

        private StorageSlot ConstructTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        public ConstructionSite(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, int maxLevel, Dictionary<int, Dictionary<PieceTemplate.Name, int>> ingredientsForLevels,
            byte animSize = 0) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty)
        {
            this.ingredientsForLevels = ingredientsForLevels;

            this.currentLevel = 0;
            this.maxLevel = maxLevel;

            for (int i = 0; i <= this.maxLevel; i++)
            {
                if (!this.ingredientsForLevels.ContainsKey(i)) throw new ArgumentOutOfRangeException($"No ingredients for level {i}.");
            }

            this.ConfigureStorage();
        }

        private void ConfigureStorage()
        {
            Dictionary<PieceTemplate.Name, int> currentIngredientsDict = this.CurrentIngredientsDict;

            int storageWidth = currentIngredientsDict.MaxBy(kvp => kvp.Value).Value;

            this.PieceStorage = new PieceStorage(width: (byte)storageWidth, height: (byte)(currentIngredientsDict.Count + 1), storagePiece: this, storageType: PieceStorage.StorageType.Construction);

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot constructTriggerSlot = this.ConstructTriggerSlot;
            constructTriggerSlot.locked = false;
            constructTriggerSlot.hidden = false;
            constructTriggerSlot.AddPiece(PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.ConstructTrigger, world: this.world));
            constructTriggerSlot.locked = true;

            for (int i = 1; i < storageWidth; i++)
            {
                StorageSlot slot = this.PieceStorage.GetSlot(i, 0);
                slot.locked = true;
                slot.hidden = true;
            }

            int storageY = 1;
            foreach (var kvp in currentIngredientsDict)
            {
                PieceTemplate.Name ingredientName = kvp.Key;
                int slotCount = kvp.Value;

                for (int i = 0; i < slotCount; i++)
                {
                    StorageSlot slot = this.PieceStorage.GetSlot(i, storageY);

                    slot.locked = false;
                    slot.hidden = false;
                    slot.allowedPieceNames = new HashSet<PieceTemplate.Name> { ingredientName };
                    slot.label = PieceInfo.GetInfo(ingredientName).readableName;
                }

                storageY++;
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