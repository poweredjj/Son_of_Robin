using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Craft
    {
        public enum Category { Field, Essential, Basic, Advanced, Master, Alchemy, Furnace, Anvil }

        public class Recipe
        {
            public readonly PieceTemplate.Name pieceToCreate;
            public readonly int amountToCreate;
            public readonly Dictionary<PieceTemplate.Name, byte> ingredients;
            public readonly List<PieceTemplate.Name> unlocksWhenCrafted;
            public readonly bool isHidden;
            public readonly bool isReversible;

            public Recipe(PieceTemplate.Name pieceToCreate, Dictionary<PieceTemplate.Name, byte> ingredients, bool isReversible = false, int amountToCreate = 1, bool isHidden = false, List<PieceTemplate.Name> unlocksWhenCrafted = null)
            {
                this.pieceToCreate = pieceToCreate;
                this.amountToCreate = amountToCreate;
                this.ingredients = ingredients;
                this.isHidden = isHidden;
                this.unlocksWhenCrafted = unlocksWhenCrafted == null ? new List<PieceTemplate.Name> { } : unlocksWhenCrafted;
                this.isReversible = isReversible;
                if (this.isReversible) Yield.antiCraftRecipes[this.pieceToCreate] = this;
            }

            public Yield ConvertToYield()
            {
                var finalDroppedPieces = new List<Yield.DroppedPiece> { };

                foreach (var kvp in this.ingredients)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    { finalDroppedPieces.Add(new Yield.DroppedPiece(pieceName: kvp.Key, chanceToDrop: 70, maxNumberToDrop: 1)); }
                }

                BoardPiece.Category category = PieceInfo.GetInfo(pieceToCreate).category;
                List<Yield.DebrisType> debrisTypeList;

                switch (category)
                {
                    case BoardPiece.Category.Wood:
                        debrisTypeList = new List<Yield.DebrisType> { Yield.DebrisType.Wood };
                        break;

                    case BoardPiece.Category.Stone:
                        debrisTypeList = new List<Yield.DebrisType> { Yield.DebrisType.Stone };
                        break;

                    case BoardPiece.Category.Metal:
                        debrisTypeList = new List<Yield.DebrisType> { Yield.DebrisType.Wood };
                        break;

                    case BoardPiece.Category.SmallPlant:
                        debrisTypeList = new List<Yield.DebrisType> { Yield.DebrisType.Plant };
                        break;

                    case BoardPiece.Category.Flesh:
                        debrisTypeList = new List<Yield.DebrisType> { Yield.DebrisType.Blood };
                        break;

                    case BoardPiece.Category.Crystal:
                        debrisTypeList = new List<Yield.DebrisType> { Yield.DebrisType.Crystal };
                        break;

                    case BoardPiece.Category.Indestructible:
                        debrisTypeList = new List<Yield.DebrisType>();
                        break;

                    default:
                        throw new ArgumentException($"Unsupported category - '{category}'.");
                }

                Yield.antiCraftRecipes[this.pieceToCreate] = this;
                return new Yield(firstDroppedPieces: new List<Yield.DroppedPiece> { }, finalDroppedPieces: finalDroppedPieces, debrisTypeList: debrisTypeList);
            }

            public bool CheckIfStorageContainsAllIngredients(PieceStorage storage)
            {
                var quantityLeft = PieceStorage.CheckMultipleStoragesForSpecifiedPieces(storageList: new List<PieceStorage> { storage }, quantityByPiece: this.ingredients);
                return quantityLeft.Count == 0;
            }

            public bool CheckIfStorageContainsAllIngredients(List<PieceStorage> storageList)
            {
                var quantityLeft = PieceStorage.CheckMultipleStoragesForSpecifiedPieces(storageList: storageList, quantityByPiece: this.ingredients);
                return quantityLeft.Count == 0;
            }

            public List<BoardPiece> TryToProducePieces(Player player, bool showMessages)
            {
                // checking if crafting is possible

                var craftedPieces = new List<BoardPiece>();

                var storagesToTakeFrom = player.CraftStorages;

                if (!this.CheckIfStorageContainsAllIngredients(storageList: storagesToTakeFrom))
                {
                    new TextWindow(text: "Not enough ingredients.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Not enough ingredients to craft '{this.pieceToCreate}'.");
                    return craftedPieces;
                }

                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.pieceToCreate);
                bool canBePickedUp = pieceInfo.canBePickedUp;

                if (canBePickedUp && !PieceStorage.StorageListCanFitSpecifiedPieces(storageList: storagesToTakeFrom, pieceName: this.pieceToCreate, quantity: this.amountToCreate))
                {
                    foreach (PieceStorage storage in storagesToTakeFrom)
                    {
                        storage.Sort();
                    }

                    if (!PieceStorage.StorageListCanFitSpecifiedPieces(storageList: storagesToTakeFrom, pieceName: this.pieceToCreate, quantity: this.amountToCreate))
                    {
                        new TextWindow(text: "Not enough inventory space to craft.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
                        return craftedPieces;
                    }
                }

                // preparing to craft

                World world = player.world;

                if (!canBePickedUp && !world.BuildMode)
                {
                    world.EnterBuildMode(recipe: this);
                    return craftedPieces;
                }

                // crafting

                PieceStorage.DestroySpecifiedPiecesInMultipleStorages(storageList: storagesToTakeFrom, quantityByPiece: this.ingredients);

                if (canBePickedUp)
                {
                    var storagesToPutInto = pieceInfo.type == typeof(Tool) || pieceInfo.type == typeof(PortableLight) ? player.CraftStoragesToolbarFirst : player.CraftStorages;

                    for (int i = 0; i < this.amountToCreate; i++)
                    {
                        BoardPiece piece = PieceTemplate.Create(templateName: this.pieceToCreate, world: world);
                        craftedPieces.Add(piece);
                        bool pieceInserted = false;

                        foreach (PieceStorage storage in storagesToPutInto)
                        {
                            if (storage.CanFitThisPiece(piece))
                            {
                                storage.AddPiece(piece: piece);
                                pieceInserted = true;
                                break;
                            }
                        }

                        if (!pieceInserted) throw new ArgumentException($"{pieceInfo.name} could not fit into any storage.");
                    }
                }
                else // !canBePickedUp
                {
                    if (!world.BuildMode) throw new ArgumentException("World is not in BuildMode.");

                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.pieceToCreate, world: world, position: player.simulatedPieceToBuild.sprite.position, ignoreCollisions: true);
                    craftedPieces.Add(piece);

                    if (!piece.sprite.IsOnBoard) throw new ArgumentException($"Piece has not been placed correctly on the board - {piece.name}.");

                    if (piece.GetType() == typeof(Plant))
                    {
                        ((Plant)piece).massTakenMultiplier *= 1.5f; // when the player plants something, it should grow better than normal
                    }
                }

                // unlocking other recipes and showing messages          

                if (showMessages) this.UnlockNewRecipesAndShowSummary(world);

                return craftedPieces;
            }

            public void UnlockNewRecipesAndShowSummary(World world)
            {
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.pieceToCreate);

                string creationType = pieceInfo.type == typeof(Plant) ? "planted" : "crafted";

                string message = this.amountToCreate == 1 ?
                    $"|  {Helpers.FirstCharToUpperCase(PieceInfo.GetInfo(this.pieceToCreate).readableName)} has been {creationType}." :
                    $"|  {Helpers.FirstCharToUpperCase(PieceInfo.GetInfo(this.pieceToCreate).readableName)} x{this.amountToCreate} has been {creationType}.";

                var taskChain = new List<Object>();

                taskChain.Add(new HintMessage(text: message, boxType: HintMessage.BoxType.GreenBox, delay: 0, blockInput: false, useTransition: true,
                    imageList: new List<Texture2D> { PieceInfo.GetInfo(this.pieceToCreate).frame.texture }, sound: SoundData.Name.Ding).ConvertToTask());

                HintEngine hintEngine = world.hintEngine;

                hintEngine.Disable(Tutorials.Type.Craft);
                if (this.pieceToCreate == PieceTemplate.Name.Map) hintEngine.Disable(PieceHint.Type.MapCanMake);
                if (this.pieceToCreate == PieceTemplate.Name.WorkshopEssential) hintEngine.Disable(Tutorials.Type.BuildWorkshop);

                var unlockedPieces = new List<PieceTemplate.Name>();

                foreach (PieceTemplate.Name newUnlockedPiece in this.unlocksWhenCrafted)
                {
                    if (!world.discoveredRecipesForPieces.Contains(newUnlockedPiece))
                    {
                        unlockedPieces.Add(newUnlockedPiece);
                        world.discoveredRecipesForPieces.Add(newUnlockedPiece);
                    }
                }

                if (unlockedPieces.Count > 0)
                {
                    if (!pieceInfo.canBePickedUp) Menu.RebuildAllMenus();

                    string unlockedRecipesMessage = unlockedPieces.Count == 1 ? "New recipe unlocked" : "New recipes unlocked:\n";
                    var imageList = new List<Texture2D>();

                    foreach (PieceTemplate.Name name in unlockedPieces)
                    {
                        PieceInfo.Info unlockedPieceInfo = PieceInfo.GetInfo(name);
                        unlockedRecipesMessage += $"\n|  {unlockedPieceInfo.readableName}";
                        imageList.Add(unlockedPieceInfo.frame.texture);
                    }

                    taskChain.Add(new HintMessage(text: unlockedRecipesMessage, imageList: imageList, boxType: HintMessage.BoxType.LightBlueBox, delay: 0, blockInput: false, animate: true, useTransition: true, sound: SoundData.Name.Notification1).ConvertToTask());
                }

                if (pieceInfo.canBePickedUp) taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CheckForPieceHints, delay: 0, storeForLaterUse: true));

                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, executeHelper: taskChain);
            }
        }

        public static bool categoriesCreated = false;
        private static Dictionary<Category, List<Recipe>> recipesByCategory = new Dictionary<Category, List<Recipe>> { };

        private static List<Recipe> AllRecipes
        {
            get
            {
                var allRecipes = new List<Recipe>();
                foreach (List<Recipe> recipeList in recipesByCategory.Values)
                {
                    allRecipes.AddRange(recipeList);
                }

                return allRecipes;
            }
        }

        private static List<Recipe> HiddenRecipes { get { return AllRecipes.Where(recipe => recipe.isHidden).ToList(); } }

        public static List<Recipe> GetRecipesForCategory(Category category, bool includeHidden = false, List<PieceTemplate.Name> discoveredRecipes = null)
        {
            if (includeHidden || Preferences.debugShowAllRecipes) return recipesByCategory[category];
            else return recipesByCategory[category].Where(recipe => !recipe.isHidden || discoveredRecipes.Contains(recipe.pieceToCreate)).ToList();
        }

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
        {
            recipesByCategory[category] = recipeList.OrderBy(r => r.pieceToCreate).ToList();
        }

        public static void PopulateAllCategories()
        {
            if (categoriesCreated) return;

            // field
            {
                List<Recipe> fieldRecipes = new List<Recipe> {

                    new Recipe(pieceToCreate: PieceTemplate.Name.WorkshopEssential, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogRegular, 6 } }, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.WorkshopBasic }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.WorkshopBasic, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 40 }, { PieceTemplate.Name.WoodLogHard, 2 }, { PieceTemplate.Name.Stone, 10 }, { PieceTemplate.Name.Granite, 2 } }, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.WorkshopAdvanced, PieceTemplate.Name.Furnace, PieceTemplate.Name.HotPlate }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.WorkshopAdvanced, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 12 },  { PieceTemplate.Name.Nail, 30 },  { PieceTemplate.Name.IronPlate, 2 } }, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.WorkshopMaster, PieceTemplate.Name.WorkshopAlchemy }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.WorkshopMaster, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 24 },  { PieceTemplate.Name.Nail, 50 }, { PieceTemplate.Name.IronPlate, 3 }, { PieceTemplate.Name.Clay, 3 }, { PieceTemplate.Name.IronRod, 3 }, { PieceTemplate.Name.EmptyBottle, 2 }, { PieceTemplate.Name.Leather, 5 } }, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.WorkshopAlchemy, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 4 }, { PieceTemplate.Name.Granite, 2 }, { PieceTemplate.Name.WoodPlank, 4 }, { PieceTemplate.Name.Clay, 1 }, { PieceTemplate.Name.IronPlate, 3 }, { PieceTemplate.Name.EmptyBottle, 2 } }, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.Campfire, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 8 }}, isReversible: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.TentSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 20 }, { PieceTemplate.Name.WoodLogRegular, 4 }}, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.TentMedium }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.TentMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 10 }, { PieceTemplate.Name.Stick, 25 }, { PieceTemplate.Name.Nail, 40 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.TentBig }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.TentBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 20 }, { PieceTemplate.Name.Stick, 60 }, { PieceTemplate.Name.Nail, 100 }, { PieceTemplate.Name.WoodPlank, 20 }, { PieceTemplate.Name.WoodLogHard, 4 }}, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ChestWooden, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 20 },  { PieceTemplate.Name.WoodLogHard, 2 }}, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ChestIron }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ChestIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronPlate, 2 },{ PieceTemplate.Name.WoodPlank, 4 }, { PieceTemplate.Name.Nail, 10 } }, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.Furnace, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 20 }, { PieceTemplate.Name.Granite, 6 }, { PieceTemplate.Name.WoodPlank, 16 }, { PieceTemplate.Name.Clay, 2 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.Anvil }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.Anvil, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 3 }, { PieceTemplate.Name.Granite, 3 }, { PieceTemplate.Name.WoodLogHard, 3 }}, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.HotPlate, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Granite, 2 }, { PieceTemplate.Name.Stone, 6 } }, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.CookingPot }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.CookingPot, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 3 }, { PieceTemplate.Name.IronPlate, 3 } }, isReversible: true, isHidden: true),
                };

                AddCategory(category: Category.Field, recipeList: fieldRecipes);
            }

            // essential workshop (wood)
            {
                List<Recipe> essentialWorkshopRecipes = new List<Recipe> {

                    new Recipe(pieceToCreate: PieceTemplate.Name.AxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }}, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeStone }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 2 }}, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeStone }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.SpearWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLogRegular, 4 }}, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearStone }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.BowWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.Leather, 1 }}, isReversible: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ArrowWood, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }}, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.ArrowStone }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.TorchSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 1 }, { PieceTemplate.Name.Fat, 1 }}),

                    new Recipe(pieceToCreate: PieceTemplate.Name.WoodPlank, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogRegular, 1 }}),
                };

                AddCategory(category: Category.Essential, recipeList: essentialWorkshopRecipes);

                // basic workshop (stone)

                List<Recipe> basicWorkshopRecipes = new List<Recipe> {

                    new Recipe(pieceToCreate: PieceTemplate.Name.AxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeIron }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeIron }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ScytheStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ScytheIron } ),

                    new Recipe(pieceToCreate: PieceTemplate.Name.SpearStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearIron }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ShovelStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.Granite, 1 }}, isReversible: true, isHidden: false, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelIron }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ArrowStone, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.Stone, 2 }}, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowIron}),

                    new Recipe(pieceToCreate: PieceTemplate.Name.Map, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 1 }}, isReversible: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.BeltMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 3 }}, isReversible: true),
                };

                basicWorkshopRecipes.InsertRange(0, essentialWorkshopRecipes);

                AddCategory(category: Category.Basic, recipeList: basicWorkshopRecipes);

                // advanced workshop (iron)

                var advancedRecipes = new List<Recipe>
                {
                    new Recipe(pieceToCreate: PieceTemplate.Name.AxeIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronRod, 1 }}, isReversible: true, isHidden: true,unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.AxeCrystal }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronRod, 1 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeCrystal }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ScytheIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronRod, 1 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ScytheCrystal } ),

                    new Recipe(pieceToCreate: PieceTemplate.Name.SpearPoisoned, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.SpearStone, 1 }, { PieceTemplate.Name.BottleOfPoison, 3 }}, isReversible: false, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.SpearIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLogHard, 2 }, { PieceTemplate.Name.IronRod, 1 }}, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearCrystal }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ShovelIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronPlate, 2 }}, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelCrystal }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.TorchBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.BottleOfOil, 1 }}, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ArrowPoisoned, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.ArrowStone, 3 }, { PieceTemplate.Name.BottleOfPoison, 1 }}, isReversible: false, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ArrowIron, amountToCreate: 10, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 6 }, { PieceTemplate.Name.IronRod, 1 }}, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowCrystal}),

                    new Recipe(pieceToCreate: PieceTemplate.Name.BackpackMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 5 }}, isReversible: true),
                };

                advancedRecipes.InsertRange(0, basicWorkshopRecipes);

                AddCategory(category: Category.Advanced, recipeList: advancedRecipes);

                // master workshop (crystal)

                var masterRecipes = new List<Recipe>
                {
                    new Recipe(pieceToCreate: PieceTemplate.Name.AxeCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Nail, 5 }, { PieceTemplate.Name.Crystal, 2 }}, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PickaxeCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Nail, 5 }, { PieceTemplate.Name.Crystal, 2 }}, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.SpearCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Nail, 5 }, { PieceTemplate.Name.Crystal, 2 }}, isReversible: false, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ShovelCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Crystal, 1 }}, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ScytheCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Crystal, 2 }}, isReversible: true, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.ArrowCrystal, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Crystal, 1 }}, isReversible: false, isHidden: true),
                };

                masterRecipes.InsertRange(0, advancedRecipes);

                AddCategory(category: Category.Master, recipeList: masterRecipes);
            }

            // alchemy
            {
                var alchemyRecipes = new List<Recipe>
                {
                    new Recipe(pieceToCreate: PieceTemplate.Name.PotionHealing, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Tomato, 4 }, { PieceTemplate.Name.HerbsRed, 3 }}, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PotionStrength } ),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PotionMaxHP, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Apple, 2 }, { PieceTemplate.Name.HerbsGreen, 3 }}, isReversible: false),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PotionStrength, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Banana, 2 }, { PieceTemplate.Name.HerbsYellow, 3 }}, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PotionHaste } ),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PotionHaste, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Acorn, 2 }, { PieceTemplate.Name.HerbsCyan, 3 }}, isReversible: false, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PotionMaxStamina, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Apple, 2 }, { PieceTemplate.Name.HerbsBlue, 2 }}, isReversible: false,  unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PotionFatigue } ),

                    new Recipe(pieceToCreate: PieceTemplate.Name.PotionFatigue, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Cherry, 2 }, { PieceTemplate.Name.HerbsViolet, 2 }}, isReversible: false, isHidden: true),

                    new Recipe(pieceToCreate: PieceTemplate.Name.BottleOfOil, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Fat, 2 }}, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.TorchBig }),

                    new Recipe(pieceToCreate: PieceTemplate.Name.BottleOfPoison, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.HerbsBlack, 2 }}, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.SpearPoisoned, PieceTemplate.Name.ArrowPoisoned }),
                };

                AddCategory(category: Category.Alchemy, recipeList: alchemyRecipes);
            }

            // furnace
            {
                var furnaceRecipes = new List<Recipe> {

                    new Recipe(pieceToCreate: PieceTemplate.Name.IronBar, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronOre, 1 }, { PieceTemplate.Name.Coal, 1 }}, isReversible: false),

                    new Recipe(pieceToCreate: PieceTemplate.Name.EmptyBottle, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.GlassSand, 3 }, { PieceTemplate.Name.Coal, 1 }}, isReversible: false),

                    new Recipe(pieceToCreate: PieceTemplate.Name.EmptyBottle, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.GlassSand, 8 }, { PieceTemplate.Name.Coal, 2 }}, isReversible: false),
                };

                AddCategory(category: Category.Furnace, recipeList: furnaceRecipes);
            }

            // anvil
            {
                var anvilRecipes = new List<Recipe>
                {
                    new Recipe(pieceToCreate: PieceTemplate.Name.IronRod, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 1 } }, isReversible: false),
                    new Recipe(pieceToCreate: PieceTemplate.Name.IronPlate, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 1 } }, isReversible: false),
                    new Recipe(pieceToCreate: PieceTemplate.Name.Nail, amountToCreate: 10, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronRod, 1 }}),
                };

                AddCategory(category: Category.Anvil, recipeList: anvilRecipes);
            }

            categoriesCreated = true;
            CheckIfRecipesAreCorrect();
        }

        private static void CheckIfRecipesAreCorrect()
        {
            foreach (List<Recipe> recipeList in recipesByCategory.Values)
            {
                foreach (Recipe recipe in recipeList)
                {
                    if (!PieceInfo.GetInfo(recipe.pieceToCreate).canBePickedUp && recipe.amountToCreate > 1) throw new ArgumentException($"Cannot create multiple pieces in the field - {recipe.pieceToCreate}.");

                    if (recipe.amountToCreate > 1 && recipe.isReversible) throw new ArgumentException($"Found reversible recipe, that can be reversed - {recipe.pieceToCreate}."); // this would lead to item duplication exploit
                }
            }

            CheckIfAllRecipesCanBeUnlocked();
        }
        private static void CheckIfAllRecipesCanBeUnlocked()
        {
            List<Recipe> allRecipes = AllRecipes;

            var recipesThatAreUnlocked = new List<PieceTemplate.Name>();

            foreach (Recipe recipe in allRecipes)
            {
                if (recipe.unlocksWhenCrafted.Contains(recipe.pieceToCreate)) throw new ArgumentException($"Recipe for '{recipe.pieceToCreate}' unlocks itself.");
                foreach (PieceTemplate.Name pieceName in recipe.unlocksWhenCrafted)
                {
                    recipesThatAreUnlocked.Add(pieceName);
                }
            }

            foreach (Recipe recipe in allRecipes)
            {
                if (!recipe.isHidden && recipesThatAreUnlocked.Contains(recipe.pieceToCreate)) throw new ArgumentException($"Recipe for '{recipe.pieceToCreate}' cannot be unlocked, because it is not hidden.");
            }

            List<Recipe> notUnlockableRecipes = new List<Recipe>();

            foreach (Recipe initialHiddenRecipe in HiddenRecipes)
            {
                Recipe currentHiddenRecipe = initialHiddenRecipe;

                bool nextLevelRecipeFound = false; // found next level in a "chain" of hidden recipes unlocking each other
                bool recipeCanBeUnlocked = false;

                while (true)
                {
                    nextLevelRecipeFound = false;
                    foreach (Recipe recipe in allRecipes)
                    {
                        if (recipe.unlocksWhenCrafted.Contains(currentHiddenRecipe.pieceToCreate))
                        {
                            if (recipe.isHidden)
                            {
                                currentHiddenRecipe = recipe;
                                nextLevelRecipeFound = true;
                            }
                            else
                            {
                                recipeCanBeUnlocked = true;
                                break;
                            }
                        }
                    }

                    if (!nextLevelRecipeFound) break;
                }
                if (!recipeCanBeUnlocked) notUnlockableRecipes.Add(initialHiddenRecipe);
            }

            if (notUnlockableRecipes.Count > 0)
            {
                throw new ArgumentException("Not unlockable recipes found: " + String.Join(", ", notUnlockableRecipes.Select(recipe => recipe.pieceToCreate).ToList()));
            }
        }

    }
}
