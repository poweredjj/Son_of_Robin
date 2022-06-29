using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Craft
    {
        public enum Category { Basic, Normal, Cooking }

        public class Recipe
        {
            public readonly PieceTemplate.Name pieceToCreate;
            public readonly Dictionary<PieceTemplate.Name, byte> ingredients;

            public Recipe(PieceTemplate.Name pieceToCreate, Dictionary<PieceTemplate.Name, byte> ingredients)
            {
                this.pieceToCreate = pieceToCreate;
                this.ingredients = ingredients;
                this.AddFramesToDisplay();
            }

            private void AddFramesToDisplay()
            {
                var namesToBeAdded = new List<PieceTemplate.Name> { };
                namesToBeAdded.AddRange(this.ingredients.Keys.ToList());
                namesToBeAdded.Add(this.pieceToCreate);

                World world = World.GetTopWorld();

                foreach (PieceTemplate.Name name in namesToBeAdded)
                {
                    if (framesToDisplay.ContainsKey(name)) continue;

                    BoardPiece piece = PieceTemplate.CreateOffBoard(templateName: name, world: world);
                    framesToDisplay[name] = piece.sprite.frame;
                }
            }
            public bool CheckIfStorageContainsAllIngredients(PieceStorage storage)
            { return storage.CheckIfContainsSpecifiedPieces(quantityByPiece: this.ingredients); }

            public bool TryToProducePiece(PieceStorage storage)
            {
                if (!this.CheckIfStorageContainsAllIngredients(storage))
                {
                    MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Not enough ingredients to craft '{this.pieceToCreate}'.");
                    return false;
                }
                storage.DestroySpecifiedPieces(this.ingredients);
                storage.AddPiece(piece: PieceTemplate.CreateOffBoard(templateName: this.pieceToCreate, world: storage.world), dropIfDoesNotFit: true);
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"'{this.pieceToCreate}' has been crafted.");
                return true;
            }
        }

        public static bool categoriesCreated = false;
        public static Dictionary<Category, List<Recipe>> recipesByCategory = new Dictionary<Category, List<Recipe>> { };

        public static Dictionary<PieceTemplate.Name, AnimFrame> framesToDisplay = new Dictionary<PieceTemplate.Name, AnimFrame> { }; // used to draw images of ingredients and pieces to craft

        public static Recipe GetRecipe(PieceTemplate.Name templateName)
        {
            foreach (List<Recipe> recipeList in recipesByCategory.Values)
            {
                foreach (Recipe recipe in recipeList)
                { if (recipe.pieceToCreate == templateName) return recipe; }
            }
            return null;
        }

        private static void AddCategory(Category category, List<Recipe> recipeList)
        { recipesByCategory[category] = recipeList; }


        public static void PopulateAllCategories()
        {
            if (categoriesCreated) return;

            List<Recipe> basicRecipeList;
            List<Recipe> recipeList;

            // TODO add recipes for all categories here

            basicRecipeList = new List<Recipe> {
                new Recipe(pieceToCreate: PieceTemplate.Name.AxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodPile, 1 }}),
                new Recipe(pieceToCreate: PieceTemplate.Name.WoodWorkshop, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPile, 4 } }),

            };
            AddCategory(category: Category.Basic, recipeList: basicRecipeList);

            recipeList = new List<Recipe> {
                new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodPile, 3 }}),
                new Recipe(pieceToCreate: PieceTemplate.Name.AxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodPile, 1 }, { PieceTemplate.Name.Stone, 2 }}),
                new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodPile, 3 }, { PieceTemplate.Name.Stone, 2 }}),
                new Recipe(pieceToCreate: PieceTemplate.Name.ChestWooden, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPile, 8 } }),
                new Recipe(pieceToCreate: PieceTemplate.Name.ChestMetal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPile, 20 } }), // TODO update with some metal
            };
            recipeList.InsertRange(0, basicRecipeList);
            AddCategory(category: Category.Normal, recipeList: recipeList);

            categoriesCreated = true;
        }

    }
}
