using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Craft
    {
        public enum Category { Field, Workshop, Furnace }

        public class Recipe
        {
            public readonly PieceTemplate.Name pieceToCreate;
            public readonly int amountToCreate;
            public readonly Dictionary<PieceTemplate.Name, byte> ingredients;

            public Recipe(PieceTemplate.Name pieceToCreate, Dictionary<PieceTemplate.Name, byte> ingredients, bool isReversible = false, int amountToCreate = 1)
            {
                this.pieceToCreate = pieceToCreate;
                this.amountToCreate = amountToCreate;
                this.ingredients = ingredients;
                if (isReversible) this.ConvertToYield();
            }

            private void ConvertToYield()
            {
                var finalDroppedPieces = new List<Yield.DroppedPiece> { };

                foreach (var kvp in this.ingredients)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    { finalDroppedPieces.Add(new Yield.DroppedPiece(pieceName: kvp.Key, chanceToDrop: 70, maxNumberToDrop: 1)); }
                }

                Yield.antiCraft[this.pieceToCreate] = new Yield(firstDroppedPieces: new List<Yield.DroppedPiece> { }, finalDroppedPieces: finalDroppedPieces);
            }

            public bool CheckIfStorageContainsAllIngredients(PieceStorage storage)
            { return storage.CheckIfContainsSpecifiedPieces(quantityByPiece: this.ingredients); }

            public bool TryToProducePieces(PieceStorage storage)
            {
                if (!this.CheckIfStorageContainsAllIngredients(storage))
                {
                    new TextWindow(text: "Not enough ingredients.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, blockInputDuration: 30);
                    MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Not enough ingredients to craft '{this.pieceToCreate}'.");
                    return false;
                }

                storage.DestroySpecifiedPieces(this.ingredients);

                for (int i = 0; i < this.amountToCreate; i++)
                {
                    BoardPiece piece = PieceTemplate.CreateOnBoard(templateName: this.pieceToCreate, world: storage.world, position: storage.storagePiece.sprite.position);

                    if (piece.sprite.placedCorrectly)
                    {
                        piece.sprite.MoveToClosestFreeSpot(storage.storagePiece.sprite.position);
                        if (storage.CanFitThisPiece(piece)) storage.AddPiece(piece: piece);
                    }
                    else
                    {
                        MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Could not create '{piece.name}' on the board. Attempting to craft directly to storage.");
                        piece = PieceTemplate.CreateOffBoard(templateName: this.pieceToCreate, world: storage.world);
                        if (piece.sprite.placedCorrectly)
                        {
                            storage.AddPiece(piece: PieceTemplate.CreateOffBoard(templateName: this.pieceToCreate, world: storage.world), dropIfDoesNotFit: true);
                        }
                        else
                        {
                            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"A second attempt to craft '{piece.name}' has failed.");
                            return false;
                        }
                    }
                }
                string message = this.amountToCreate == 1 ? "Item has been crafted." : $"{this.amountToCreate} items has been crafted.";
                new TextWindow(text: message, textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: false, closingTask: Scheduler.TaskName.CheckForPieceHints);
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"'{this.pieceToCreate}' has been crafted.");

                return true;
            }
        }

        public static bool categoriesCreated = false;
        public static Dictionary<Category, List<Recipe>> recipesByCategory = new Dictionary<Category, List<Recipe>> { };

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

            List<Recipe> recipeList;

            // field craft

            recipeList = new List<Recipe> {
                new Recipe(pieceToCreate: PieceTemplate.Name.AxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLog, 1 }}, isReversible: true),
                new Recipe(pieceToCreate: PieceTemplate.Name.RegularWorkshop, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLog, 4 } }, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.TentSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 20 }, { PieceTemplate.Name.WoodLog, 4 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.TentMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 10 }, { PieceTemplate.Name.Stick, 25 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.TentBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 20 }, { PieceTemplate.Name.Stick, 60 }, { PieceTemplate.Name.Nail, 100 }, { PieceTemplate.Name.WoodPlank, 30 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.ChestWooden, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 16 },  { PieceTemplate.Name.Nail, 40 } }, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.ChestIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 2 },{ PieceTemplate.Name.WoodPlank, 4 }, { PieceTemplate.Name.Nail, 30 } }, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.Furnace, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 20 }, { PieceTemplate.Name.WoodLog, 4 }, { PieceTemplate.Name.Coal, 4 } }, isReversible: true),

               new Recipe(pieceToCreate: PieceTemplate.Name.CookingPot, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 5 } }, isReversible: true),

            };
            AddCategory(category: Category.Field, recipeList: recipeList);

            // workshop

            recipeList = new List<Recipe> {
                new Recipe(pieceToCreate: PieceTemplate.Name.Map, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 1 }}, isReversible: false),

                new Recipe(pieceToCreate: PieceTemplate.Name.BackpackMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 5 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.BeltMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 3 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.WoodPlank, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLog, 1 }}),

                new Recipe(pieceToCreate: PieceTemplate.Name.Nail, amountToCreate: 10, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 1 }}),

                new Recipe(pieceToCreate: PieceTemplate.Name.BatWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLog, 2 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.BowWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLog, 1 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.Sling, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 1 }, { PieceTemplate.Name.Leather, 1 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.GreatSling, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.Leather, 2 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.StoneAmmo, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 1 }}, isReversible: false),

                new Recipe(pieceToCreate: PieceTemplate.Name.ArrowWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 1 }}, isReversible: false),

                new Recipe(pieceToCreate: PieceTemplate.Name.ArrowWood, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }}, isReversible: false),

                new Recipe(pieceToCreate: PieceTemplate.Name.ArrowIron, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLog, 1 }, { PieceTemplate.Name.IronBar, 1 }}, isReversible: false),

                new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLog, 3 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLog, 1 }, { PieceTemplate.Name.Stone, 2 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLog, 1 }, { PieceTemplate.Name.IronBar, 2 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.AxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLog, 1 }, { PieceTemplate.Name.Stone, 2 }}, isReversible: true),

                new Recipe(pieceToCreate: PieceTemplate.Name.AxeIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLog, 1 }, { PieceTemplate.Name.IronBar, 2 }}, isReversible: true),

            };
            AddCategory(category: Category.Workshop, recipeList: recipeList);

            // furnace

            var furnaceRecipeList = new List<Recipe> {
                new Recipe(pieceToCreate: PieceTemplate.Name.IronBar, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronOre, 1 }, { PieceTemplate.Name.Coal, 1 }}, isReversible: false),
            };
            AddCategory(category: Category.Furnace, recipeList: furnaceRecipeList);

            categoriesCreated = true;
        }

    }
}
