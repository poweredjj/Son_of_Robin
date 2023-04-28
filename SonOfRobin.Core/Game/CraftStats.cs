using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class CraftStats
    {
        public int TotalNoOfCrafts { get; private set; }
        public int CraftedPiecesTotal { get; private set; }
        public int UsedIngredientsTotal { get; private set; }
        public int SmartCraftingReducedIngredientCount { get; private set; }

        private Dictionary<string, int> craftedRecipes;
        private Dictionary<PieceTemplate.Name, int> craftedPieces;
        private Dictionary<PieceTemplate.Name, int> usedIngredients;

        public bool LastCraftWasSmart { get; private set; }
        public PieceTemplate.Name LastSmartCraftReducedIngredientName { get; private set; }
        public byte LastSmartCraftReducedIngredientCount { get; private set; }

        public CraftStats()
        {
            this.TotalNoOfCrafts = 0;
            this.craftedRecipes = new Dictionary<string, int>();
            this.craftedPieces = new Dictionary<PieceTemplate.Name, int>();
            this.usedIngredients = new Dictionary<PieceTemplate.Name, int>();
            this.CraftedPiecesTotal = 0;
            this.UsedIngredientsTotal = 0;
            this.SmartCraftingReducedIngredientCount = 0;

            this.ResetLastSmartCraft();
        }

        public void ResetLastSmartCraft()
        {
            // has to be called after displaying smart craft messages

            this.LastCraftWasSmart = false;
            this.LastSmartCraftReducedIngredientName = PieceTemplate.Name.Empty;
            this.LastSmartCraftReducedIngredientCount = 0;
        }

        public void AddRecipe(Craft.Recipe recipe, int craftCount = 1)
        {
            if (craftCount < 1) throw new ArgumentException($"craftCount ({craftCount}) is less than 1");

            this.TotalNoOfCrafts += craftCount;

            {
                string id = recipe.id;
                if (!this.craftedRecipes.ContainsKey(id)) this.craftedRecipes[id] = 0;
                this.craftedRecipes[id] += craftCount;
            }

            {
                PieceTemplate.Name craftedPiece = recipe.pieceToCreate;
                if (!this.craftedPieces.ContainsKey(craftedPiece)) this.craftedPieces[craftedPiece] = 0;
                this.craftedPieces[craftedPiece] += craftCount;
                this.CraftedPiecesTotal += craftCount;
            }

            foreach (var kvp in recipe.ingredients)
            {
                PieceTemplate.Name ingredientName = kvp.Key;
                int ingredientCount = kvp.Value;

                if (!this.usedIngredients.ContainsKey(ingredientName)) this.usedIngredients[ingredientName] = 0;
                this.usedIngredients[ingredientName] += ingredientCount * craftCount;
                this.UsedIngredientsTotal += ingredientCount * craftCount;
            }
        }

        public void AddSmartCraftingReducedAmount(PieceTemplate.Name ingredientName, byte reducedAmount)
        {
            this.LastCraftWasSmart = true;
            this.LastSmartCraftReducedIngredientName = ingredientName;
            this.LastSmartCraftReducedIngredientCount = reducedAmount;

            this.SmartCraftingReducedIngredientCount += reducedAmount;
        }

        public Dictionary<string, object> Serialize()
        {
            var statsData = new Dictionary<string, object>
            {
                { "TotalNoOfCrafts", this.TotalNoOfCrafts },
                { "craftedRecipesInt", this.craftedRecipes }, // bases on newer recipe.id, that uses int instead of PieceTemplate.Name
                { "craftedPieces", this.craftedPieces.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) }, // serialized as <int, int>, otherwise enums are serialized as strings
                { "usedIngredients", this.usedIngredients.ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value) }, // serialized as <int, int>, otherwise enums are serialized as strings
                { "CraftedPiecesTotal", this.CraftedPiecesTotal },
                { "UsedIngredientsTotal", this.UsedIngredientsTotal },
                { "SmartCraftingReducedIngredientCount", this.SmartCraftingReducedIngredientCount },
            };

            return statsData;
        }

        public void Deserialize(Dictionary<string, Object> statsData)
        {
            this.TotalNoOfCrafts = (int)(Int64)statsData["TotalNoOfCrafts"];
            this.craftedRecipes = (Dictionary<string, int>)statsData["craftedRecipesInt"];
            this.craftedPieces = ((Dictionary<int, int>)statsData["craftedPieces"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
            this.usedIngredients = ((Dictionary<int, int>)statsData["usedIngredients"]).ToDictionary(kvp => (PieceTemplate.Name)kvp.Key, kvp => kvp.Value);
            this.CraftedPiecesTotal = (int)(Int64)statsData["CraftedPiecesTotal"];
            this.UsedIngredientsTotal = (int)(Int64)statsData["UsedIngredientsTotal"];
            this.SmartCraftingReducedIngredientCount = (int)(Int64)statsData["SmartCraftingReducedIngredientCount"];
        }

        public bool HasBeenCrafted(Craft.Recipe recipe)
        {
            return this.craftedRecipes.ContainsKey(recipe.id);
        }

        public bool HasBeenCrafted(PieceTemplate.Name name)
        {
            return this.craftedPieces.ContainsKey(name);
        }

        public int HowManyTimesHasBeenCrafted(Craft.Recipe recipe)
        {
            return this.craftedRecipes.ContainsKey(recipe.id) ? this.craftedRecipes[recipe.id] : 0;
        }

        public int HowMuchHasBeenCrafted(PieceTemplate.Name name)
        {
            return this.craftedPieces.ContainsKey(name) ? this.craftedPieces[name] : 0;
        }

        public bool WasThisIngredientUsed(PieceTemplate.Name name)
        {
            return this.usedIngredients.ContainsKey(name);
        }

        public int HowMuchIngredientHasBeenUsed(PieceTemplate.Name name)
        {
            return this.usedIngredients.ContainsKey(name) ? this.usedIngredients[name] : 0;
        }

        public List<List<InfoWindow.TextEntry>> GetTextEntryListForVegetationPlantedSummary()
        {
            return this.GetTextEntryListForSummary(collectionToShow: this.craftedPieces, header: "Vegetation planted", showOnlyPlantsAndFruits: true);
        }

        public List<List<InfoWindow.TextEntry>> GetTextEntryListForCraftedPiecesSummary()
        {
            return this.GetTextEntryListForSummary(collectionToShow: this.craftedPieces, header: "Crafted items", showOnlyPlantsAndFruits: false);
        }

        public List<List<InfoWindow.TextEntry>> GetTextEntryListForUsedIngredientsSummary()
        {
            return this.GetTextEntryListForSummary(collectionToShow: this.usedIngredients, header: "Used ingredients", showOnlyPlantsAndFruits: false);
        }

        public int GetRecipeLevel(Craft.Recipe recipe)
        {
            int craftCount = this.HowManyTimesHasBeenCrafted(recipe);
            float recipeLevel = (float)craftCount / (float)recipe.craftCountToLevelUp;
            return (int)Math.Min(recipeLevel, recipe.maxLevel);
        }

        public bool RecipeJustLevelledUp(Craft.Recipe recipe)
        {
            // repeated calculations, because Math.Min() would cause level up to be reported every time at master level

            int craftCount = this.HowManyTimesHasBeenCrafted(recipe);
            float recipeLevel = (float)craftCount / (float)recipe.craftCountToLevelUp;
            return recipeLevel > 0 && recipeLevel <= recipe.maxLevel && recipeLevel == Math.Floor(recipeLevel);
        }

        private List<List<InfoWindow.TextEntry>> GetTextEntryListForSummary(Dictionary<PieceTemplate.Name, int> collectionToShow, string header, bool showOnlyPlantsAndFruits)
        {
            var listOfInfoTextList = new List<List<InfoWindow.TextEntry>>();

            var plantNames = PieceInfo.namesForType[typeof(Plant)];
            var fruitNames = PieceInfo.namesForType[typeof(Fruit)];

            var allowedNames = new List<PieceTemplate.Name>();
            if (showOnlyPlantsAndFruits) allowedNames.AddRange(plantNames);
            else allowedNames = PieceTemplate.allNames.Where(name => !plantNames.Contains(name) && !fruitNames.Contains(name)).ToList();

            var pieceNames = collectionToShow.Keys.Where(name => allowedNames.Contains(name)).OrderBy(n => PieceInfo.GetInfo(n).readableName);
            if (!pieceNames.Any()) return null;

            int entriesPerPage = 15;
            int pageCounter = 1;
            int pieceCounter = 0;

            var textLines = new List<string>();
            var imageList = new List<Texture2D>();

            bool showPageCounter = pieceNames.Count() > entriesPerPage;

            foreach (PieceTemplate.Name pieceName in pieceNames)
            {
                int pieceCount = collectionToShow[pieceName];
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

                textLines.Add($"|  x{pieceCount}  {pieceInfo.readableName}");
                imageList.Add(pieceInfo.texture);

                pieceCounter++;
                if (pieceCounter >= entriesPerPage || pieceName == pieceNames.Last())
                {
                    string fullHeader = showPageCounter ? $"{header} - page {pageCounter}" : $"{header}";

                    var currentPageInfoTextList = new List<InfoWindow.TextEntry>
                    {
                        new InfoWindow.TextEntry(text: fullHeader, color: Color.White, scale: 1f),
                        new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList.ToList(), color: Color.White, scale: 1f)
                    };

                    listOfInfoTextList.Add(currentPageInfoTextList);

                    pageCounter++;
                    pieceCounter = 0;
                    textLines.Clear();
                    imageList.Clear();
                }
            }

            return listOfInfoTextList;
        }
    }
}