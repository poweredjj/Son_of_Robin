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
                { "craftedRecipes", this.craftedRecipes },
                { "craftedPieces", this.craftedPieces },
                { "usedIngredients", this.usedIngredients },
                { "CraftedPiecesTotal", this.CraftedPiecesTotal },
                { "UsedIngredientsTotal", this.UsedIngredientsTotal },
                { "SmartCraftingReducedIngredientCount", this.SmartCraftingReducedIngredientCount },
            };

            return statsData;
        }

        public void Deserialize(Dictionary<string, Object> statsData)
        {
            this.TotalNoOfCrafts = (int)(Int64)statsData["TotalNoOfCrafts"];
            this.craftedRecipes = (Dictionary<string, int>)statsData["craftedRecipes"];
            this.craftedPieces = (Dictionary<PieceTemplate.Name, int>)statsData["craftedPieces"];
            this.usedIngredients = (Dictionary<PieceTemplate.Name, int>)statsData["usedIngredients"];
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

        public void DisplayCraftedPiecesSummary()
        {
            this.DisplaySummary(collectionToShow: this.craftedPieces, header: "Crafted pieces");
        }

        public void DisplayUsedIngredientsSummary()
        {
            this.DisplaySummary(collectionToShow: this.usedIngredients, header: "Used ingredients");
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

        private void DisplaySummary(Dictionary<PieceTemplate.Name, int> collectionToShow, string header)
        {
            if (collectionToShow.Keys.Count == 0)
            {
                new TextWindow(text: "|  No items has been crafted.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Exclamation) }, textColor: Color.White, bgColor: Color.Blue, useTransition: true, animate: true);
                return;
            }

            int piecesForPage = 10;
            var taskChain = new List<Object>();

            string piecesText = "";
            List<Texture2D> imageList = new List<Texture2D>();

            List<PieceTemplate.Name> pieceNames = collectionToShow.Keys.OrderBy(n => PieceInfo.GetInfo(n).readableName).ToList();
            int piecesTotal = collectionToShow.Values.Sum();

            int currentPagePiecesCount = 0;
            int pageNo = 1;
            int totalPages = (int)Math.Ceiling((float)pieceNames.Count / (float)piecesForPage);

            foreach (PieceTemplate.Name pieceName in pieceNames)
            {
                int pieceCount = collectionToShow[pieceName];
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceName);

                piecesText += $"\n|  x{pieceCount}  {pieceInfo.readableName}";
                imageList.Add(pieceInfo.texture);

                currentPagePiecesCount++;

                if (currentPagePiecesCount >= piecesForPage || pieceName == pieceNames.Last())
                {
                    string pageText = $"{header} ({piecesTotal}):\n{piecesText}";
                    if (totalPages > 1) pageText += $"\n\npage {pageNo}/{totalPages}";

                    taskChain.Add(new HintMessage(text: pageText, imageList: imageList.ToList(), boxType: HintMessage.BoxType.BlueBox, delay: 0).ConvertToTask());

                    piecesText = "";
                    imageList.Clear();
                    currentPagePiecesCount = 0;
                    pageNo++;
                }
            }

            if (this.SmartCraftingReducedIngredientCount > 0)
            {
                taskChain.Add(new HintMessage(text: $"Smart craft\nreduced ingredient count: {this.SmartCraftingReducedIngredientCount}", boxType: HintMessage.BoxType.BlueBox, delay: 0).ConvertToTask());
            }

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }
    }
}