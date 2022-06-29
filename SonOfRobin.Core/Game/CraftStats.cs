using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class CraftStats
    {
        public int TotalNoOfCrafts { get { return this.totalNoOfCrafts; } }
        private int totalNoOfCrafts;
        public int CraftedPiecesTotal { get { return this.craftedPiecesTotal; } }
        private int craftedPiecesTotal;
        public int UsedIngredientsTotal { get { return this.usedIngredientsTotal; } }
        private int usedIngredientsTotal;

        private Dictionary<string, int> craftedRecipes;
        private Dictionary<PieceTemplate.Name, int> craftedPieces;
        private Dictionary<PieceTemplate.Name, int> usedIngredients;

        public CraftStats()
        {
            this.totalNoOfCrafts = 0;
            this.craftedRecipes = new Dictionary<string, int>();
            this.craftedPieces = new Dictionary<PieceTemplate.Name, int>();
            this.usedIngredients = new Dictionary<PieceTemplate.Name, int>();
            this.craftedPiecesTotal = 0;
            this.usedIngredientsTotal = 0;
        }

        public void AddRecipe(Craft.Recipe recipe, int craftCount = 1)
        {
            if (craftCount < 1) throw new ArgumentException($"craftCount ({craftCount}) is less than 1");

            this.totalNoOfCrafts += craftCount;

            {
                string id = recipe.id;
                if (!this.craftedRecipes.ContainsKey(id)) this.craftedRecipes[id] = 0;
                this.craftedRecipes[id] += craftCount;
            }

            {
                PieceTemplate.Name craftedPiece = recipe.pieceToCreate;
                if (!this.craftedPieces.ContainsKey(craftedPiece)) this.craftedPieces[craftedPiece] = 0;
                this.craftedPieces[craftedPiece] += craftCount;
                this.craftedPiecesTotal += craftCount;
            }

            foreach (var kvp in recipe.ingredients)
            {
                PieceTemplate.Name ingredientName = kvp.Key;
                int ingredientCount = kvp.Value;

                if (!this.usedIngredients.ContainsKey(ingredientName)) this.usedIngredients[ingredientName] = 0;
                this.usedIngredients[ingredientName] += ingredientCount * craftCount;
                this.usedIngredientsTotal += ingredientCount * craftCount;
            }
        }

        public Dictionary<string, object> Serialize()
        {
            var statsData = new Dictionary<string, object>
            {
                { "totalNoOfCrafts", this.totalNoOfCrafts },
                { "craftedRecipes", this.craftedRecipes },
                { "craftedPieces", this.craftedPieces },
                { "usedIngredients", this.usedIngredients },
                { "craftedPiecesTotal", this.craftedPiecesTotal },
                { "usedIngredientsTotal", this.usedIngredientsTotal },
            };

            return statsData;
        }

        public void Deserialize(Dictionary<string, Object> statsData)
        {
            this.totalNoOfCrafts = (int)statsData["totalNoOfCrafts"];
            this.craftedRecipes = (Dictionary<string, int>)statsData["craftedRecipes"];
            this.craftedPieces = (Dictionary<PieceTemplate.Name, int>)statsData["craftedPieces"];
            this.usedIngredients = (Dictionary<PieceTemplate.Name, int>)statsData["usedIngredients"];
            this.craftedPiecesTotal = (int)statsData["craftedPiecesTotal"];
            this.usedIngredientsTotal = (int)statsData["usedIngredientsTotal"];
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

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
        }
    }
}
