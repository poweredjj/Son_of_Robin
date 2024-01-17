using Microsoft.Xna.Framework;
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

        public void RegisterRecipeUse(Craft.Recipe recipe)
        {
            this.TotalNoOfCrafts++;

            {
                string id = recipe.id;
                if (!this.craftedRecipes.ContainsKey(id)) this.craftedRecipes[id] = 0;
                this.craftedRecipes[id]++;
            }

            {
                PieceTemplate.Name craftedPiece = recipe.pieceToCreate;
                if (!this.craftedPieces.ContainsKey(craftedPiece)) this.craftedPieces[craftedPiece] = 0;
                this.craftedPieces[craftedPiece] += recipe.amountToCreate;
                this.CraftedPiecesTotal += recipe.amountToCreate;
            }

            foreach (var kvp in recipe.ingredients)
            {
                PieceTemplate.Name ingredientName = kvp.Key;
                int ingredientCount = kvp.Value;

                if (!this.usedIngredients.ContainsKey(ingredientName)) this.usedIngredients[ingredientName] = 0;
                this.usedIngredients[ingredientName] += ingredientCount;
                this.UsedIngredientsTotal += ingredientCount;
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

        public int HowManyHasBeenCrafted(PieceTemplate.Name name)
        {
            return this.craftedPieces.ContainsKey(name) ? this.craftedPieces[name] : 0;
        }

        public int UniqueRecipesCraftedTotal
        { get { return this.craftedPieces.Keys.Count; } }

        public bool WasThisIngredientUsed(PieceTemplate.Name name)
        {
            return this.usedIngredients.ContainsKey(name);
        }

        public int HowManyIngredientHasBeenUsed(PieceTemplate.Name name)
        {
            return this.usedIngredients.ContainsKey(name) ? this.usedIngredients[name] : 0;
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

        public void CreateMenuEntriesForVegetationPlantedSummary(Menu menu)
        {
            this.CreateMenuEntriesForSummary(menu: menu, color: new Color(41, 145, 0), collectionToShow: this.craftedPieces, header: "Vegetation planted", showOnlyPlants: true);
        }

        public void CreateMenuEntriesForCraftedPiecesSummary(Menu menu)
        {
            this.CreateMenuEntriesForSummary(menu: menu, color: new Color(0, 141, 184), collectionToShow: this.craftedPieces, header: "Crafted items", showOnlyPlants: false);
        }

        public void CreateMenuEntriesForUsedIngredientsSummary(Menu menu)
        {
            this.CreateMenuEntriesForSummary(menu: menu, color: new Color(152, 67, 217), collectionToShow: this.usedIngredients, header: "Used ingredients", showOnlyPlants: false);
        }

        private void CreateMenuEntriesForSummary(Menu menu, Color color, Dictionary<PieceTemplate.Name, int> collectionToShow, string header, bool showOnlyPlants)
        {
            var plantNames = PieceInfo.namesForType[typeof(Plant)];
            var seedNames = PieceInfo.namesForType[typeof(Seed)];

            var allowedNames = new List<PieceTemplate.Name>();
            if (showOnlyPlants) allowedNames.AddRange(plantNames);
            else allowedNames = PieceTemplate.allNames.Where(name => !plantNames.Contains(name) && !seedNames.Contains(name)).ToList();

            var pieceNames = collectionToShow.Keys.Where(name => allowedNames.Contains(name)).OrderBy(n => PieceInfo.GetInfo(n).readableName);
            if (pieceNames.Count() == 0) return;

            int entriesPerPage = 15;
            int pageCounter = 1;
            int pieceCounter = 0;

            var textLines = new List<string>();
            var imageList = new List<ImageObj>();

            bool showPageCounter = pieceNames.Count() > entriesPerPage;

            foreach (PieceTemplate.Name pieceName in pieceNames)
            {
                int pieceCount = collectionToShow[pieceName];
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

                textLines.Add($"|  x{pieceCount}  {pieceInfo.readableName}");
                imageList.Add(pieceInfo.imageObj);

                pieceCounter++;
                if (pieceCounter >= entriesPerPage || pieceName == pieceNames.Last())
                {
                    string fullHeader = showPageCounter ? $"{header} - page {pageCounter}" : $"{header}";

                    var infoTextList = new List<InfoWindow.TextEntry>
                    {
                        new InfoWindow.TextEntry(text: fullHeader, color: Color.White, scale: 1f),
                        new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList.ToList(), color: Color.White, scale: 1f, minMarkerWidthMultiplier: 1.4f, imageAlignX: Helpers.AlignX.Left)
                    };

                    string nameString = showPageCounter ? $"{header.ToLower()} - page {pageCounter}" : $"{header.ToLower()}";
                    Invoker invoker = new Invoker(menu: menu, name: nameString, taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);

                    invoker.bgColor = color;

                    pageCounter++;
                    pieceCounter = 0;
                    textLines.Clear();
                    imageList.Clear();
                }
            }
        }
    }
}