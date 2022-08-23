using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Craft
    {
        public enum Category { Field, Essential, Basic, Advanced, Master, Alchemy, Furnace, Anvil, LeatherBasic, LeatherAdvanced }

        public class Recipe
        {
            private static readonly Dictionary<string, Recipe> recipeByID = new Dictionary<string, Recipe>();

            public readonly string id;
            public readonly PieceTemplate.Name pieceToCreate;
            public readonly int amountToCreate;
            public readonly Dictionary<PieceTemplate.Name, byte> ingredients;
            public readonly float fatigue;
            public readonly int duration;
            public readonly int maxLevel;
            public readonly int craftCountToLevelUp;
            public readonly float maxLevelFatigueMultiplier;
            public readonly float maxLevelDurationMultiplier;
            public readonly List<PieceTemplate.Name> unlocksWhenCrafted;
            public readonly int craftCountToUnlock;
            public readonly bool isHidden;
            public readonly bool isReversible;

            public Recipe(PieceTemplate.Name pieceToCreate, Dictionary<PieceTemplate.Name, byte> ingredients, float fatigue, int duration = -1, bool isReversible = false, int amountToCreate = 1, bool isHidden = false, List<PieceTemplate.Name> unlocksWhenCrafted = null, int craftCountToUnlock = 1, bool checkIfAlreadyAdded = true, int maxLevel = 2, int craftCountToLevelUp = 3, float maxLevelFatigueMultiplier = 0.6f, float maxLevelDurationMultiplier = 0.6f)
            {
                this.pieceToCreate = pieceToCreate;
                this.amountToCreate = amountToCreate;
                this.id = $"{this.pieceToCreate}_{this.amountToCreate}";
                this.ingredients = ingredients;
                this.fatigue = fatigue;
                this.duration = duration == -1 ? (int)(this.fatigue * 10) : duration; // if duration was not specified, it will be calculated from fatigue
                this.maxLevel = maxLevel;
                this.craftCountToLevelUp = craftCountToLevelUp;
                this.maxLevelFatigueMultiplier = maxLevelFatigueMultiplier;
                this.maxLevelDurationMultiplier = maxLevelDurationMultiplier;
                this.isHidden = isHidden;
                this.unlocksWhenCrafted = unlocksWhenCrafted == null ? new List<PieceTemplate.Name> { } : unlocksWhenCrafted;
                this.craftCountToUnlock = craftCountToUnlock;
                this.isReversible = isReversible;
                if (this.isReversible) Yield.antiCraftRecipes[this.pieceToCreate] = this;

                if (checkIfAlreadyAdded && recipeByID.ContainsKey(this.id)) throw new ArgumentException($"Recipe with ID {this.id} has already been added.");
                if (this.maxLevelFatigueMultiplier > 1) throw new ArgumentException($"Max level fatigue multiplier ({this.maxLevelFatigueMultiplier}) cannot be greater than 1.");
                if (this.maxLevelDurationMultiplier > 1) throw new ArgumentException($"Max level duration multiplier ({this.maxLevelDurationMultiplier}) cannot be greater than 1.");
                if (this.maxLevel <= 1) throw new ArgumentException($"Max level ({this.maxLevel}) cannot be less than 1.");
                recipeByID[this.id] = this;
            }

            public static Recipe GetRecipeByID(string id)
            {
                return recipeByID[id];
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

            public List<BoardPiece> TryToProducePieces(Player player, bool showMessages, bool craftOnTheGround = false)
            {
                // checking if crafting is possible

                var craftedPieces = new List<BoardPiece>();

                var storagesToTakeFrom = player.CraftStorages;

                if (!this.CheckIfStorageContainsAllIngredients(storageList: storagesToTakeFrom))
                {
                    new TextWindow(text: "Not enough ingredients.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, startingSound: SoundData.Name.Error);
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Not enough ingredients to craft '{this.pieceToCreate}'.");
                    return craftedPieces;
                }

                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.pieceToCreate);
                bool canBePickedUp = pieceInfo.canBePickedUp;

                if (canBePickedUp && !PieceStorage.StorageListCanFitSpecifiedPieces(storageList: storagesToTakeFrom, pieceName: this.pieceToCreate, quantity: this.amountToCreate))
                {
                    foreach (PieceStorage storage in storagesToTakeFrom) storage.Sort(); // trying to make room in storages by sorting pieces

                    if (!craftOnTheGround && !PieceStorage.StorageListCanFitSpecifiedPieces(storageList: storagesToTakeFrom, pieceName: this.pieceToCreate, quantity: this.amountToCreate))
                    {
                        var craftParams = new Dictionary<string, object> { { "recipe", this }, { "craftOnTheGround", true } };

                        var confirmationData = new Dictionary<string, Object> {
                            { "question", "Not enough inventory space to craft. Craft on the ground?" },
                            { "taskName", Scheduler.TaskName.Craft },
                            { "executeHelper", craftParams }, { "blocksUpdatesBelow", true } };

                        Sound.QuickPlay(SoundData.Name.Notification4);
                        MenuTemplate.CreateConfirmationMenu(confirmationData: confirmationData);
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

                if (canBePickedUp) world.islandClock.Advance(amount: this.duration, ignorePause: true); // build mode advances clock gradually, not all at once

                // crafting

                world.craftStats.AddRecipe(recipe: this, craftCount: 1);

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

                        if (!pieceInserted)
                        {
                            if (craftOnTheGround) storagesToPutInto[0].AddPiece(piece: piece, dropIfDoesNotFit: true, addMovement: true);

                            else throw new ArgumentException($"{pieceInfo.name} could not fit into any storage.");
                        }
                    }
                }
                else // !canBePickedUp
                {
                    if (!world.BuildMode) throw new ArgumentException("World is not in BuildMode.");

                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.pieceToCreate, world: world, position: player.simulatedPieceToBuild.sprite.position, ignoreCollisions: true);
                    craftedPieces.Add(piece);

                    piece.canBeHit = false; // to protect crafted item from accidental player hit

                    if (!piece.sprite.IsOnBoard) throw new ArgumentException($"Piece has not been placed correctly on the board - {piece.name}.");

                    if (piece.GetType() == typeof(Plant))
                    {
                        ((Plant)piece).massTakenMultiplier *= 1.5f; // when the player plants something, it should grow better than normal
                    }
                }

                // unlocking other recipes and showing messages          

                world.player.Fatigue += this.fatigue;

                if (showMessages) this.UnlockNewRecipesAndShowSummary(world);

                return craftedPieces;
            }

            public void UnlockNewRecipesAndShowSummary(World world)
            {
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.pieceToCreate);

                string creationType = pieceInfo.type == typeof(Plant) ? "planted" : "crafted";

                string pieceName = Helpers.FirstCharToUpperCase(PieceInfo.GetInfo(this.pieceToCreate).readableName);

                string message = this.amountToCreate == 1 ?
                    $"|  {pieceName} has been {creationType}." :
                    $"|  {pieceName} x{this.amountToCreate} has been {creationType}.";

                var taskChain = new List<Object>();

                SoundData.Name soundName = !pieceInfo.canBePickedUp && pieceInfo.type != typeof(Plant) ? SoundData.Name.Ding1 : SoundData.Name.Ding3;

                taskChain.Add(new HintMessage(text: message, boxType: HintMessage.BoxType.GreenBox, delay: 0, blockInput: false, useTransition: true,
                    imageList: new List<Texture2D> { PieceInfo.GetInfo(this.pieceToCreate).texture }, startingSound: soundName).ConvertToTask());

                HintEngine hintEngine = world.hintEngine;

                hintEngine.Disable(Tutorials.Type.Craft);
                if (this.pieceToCreate == PieceTemplate.Name.WorkshopEssential) hintEngine.Disable(Tutorials.Type.BuildWorkshop);

                var unlockedPieces = new List<PieceTemplate.Name>();
                foreach (PieceTemplate.Name newUnlockedPiece in this.unlocksWhenCrafted)
                {
                    if (!world.discoveredRecipesForPieces.Contains(newUnlockedPiece))
                    {
                        if (world.craftStats.HowManyTimesHasBeenCrafted(this) >= this.craftCountToUnlock)
                        {
                            unlockedPieces.Add(newUnlockedPiece);
                            world.discoveredRecipesForPieces.Add(newUnlockedPiece);
                        }
                    }
                }

                float recipeLevel = world.craftStats.GetRecipeLevel(this);
                bool recipeLevelUp = world.craftStats.RecipeJustLevelledUp(this);

                if (recipeLevelUp)
                {
                    bool levelMaster = recipeLevel == this.maxLevel;
                    string recipeNewLevelName = levelMaster ? "master |" : $"{(int)recipeLevel + 1}";

                    var imageList = new List<Texture2D> { PieceInfo.GetInfo(this.pieceToCreate).texture };
                    if (levelMaster) imageList.Add(PieceInfo.GetInfo(PieceTemplate.Name.DebrisStar).texture);

                    taskChain.Add(new HintMessage(text: $"{pieceName} |\nrecipe level up {(int)recipeLevel} -> {recipeNewLevelName}", imageList: imageList, boxType: levelMaster ? HintMessage.BoxType.GoldBox : HintMessage.BoxType.LightBlueBox, delay: 0, blockInput: false, animate: true, useTransition: true, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1).ConvertToTask());
                }

                if (unlockedPieces.Count > 0)
                {
                    Menu.RebuildAllMenus();

                    string unlockedRecipesMessage = unlockedPieces.Count == 1 ? "New recipe unlocked" : "New recipes unlocked:\n";
                    var imageList = new List<Texture2D>();

                    foreach (PieceTemplate.Name name in unlockedPieces)
                    {
                        PieceInfo.Info unlockedPieceInfo = PieceInfo.GetInfo(name);
                        unlockedRecipesMessage += $"\n|  {unlockedPieceInfo.readableName}";
                        imageList.Add(unlockedPieceInfo.texture);
                    }

                    taskChain.Add(new HintMessage(text: unlockedRecipesMessage, imageList: imageList, boxType: HintMessage.BoxType.LightBlueBox, delay: 0, blockInput: false, animate: true, useTransition: true, startingSound: SoundData.Name.Notification1).ConvertToTask());
                }

                if (!pieceInfo.canBePickedUp)
                {
                    taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));
                }

                var executeHelper = new Dictionary<string, Object>();
                if (pieceInfo.canBePickedUp) executeHelper["newOwnedPiece"] = this.pieceToCreate;
                else executeHelper["fieldPiece"] = this.pieceToCreate;

                taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CheckForPieceHints, delay: 0, storeForLaterUse: true, executeHelper: executeHelper));

                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, executeHelper: taskChain);
            }
        }

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
            {
                // field

                List<Recipe> fieldRecipes = CraftData.GetFieldRecipes();
                AddCategory(category: Category.Field, recipeList: fieldRecipes);
            }

            {
                // essential workshop (wood)

                List<Recipe> essentialWorkshopRecipes = CraftData.GetEssentialWorkshopRecipes();
                AddCategory(category: Category.Essential, recipeList: essentialWorkshopRecipes);

                // basic workshop (stone)

                List<Recipe> basicWorkshopRecipes = CraftData.GetBasicWorkshopRecipes();
                basicWorkshopRecipes.InsertRange(0, essentialWorkshopRecipes);

                AddCategory(category: Category.Basic, recipeList: basicWorkshopRecipes);

                // advanced workshop (iron)

                var advancedRecipes = CraftData.GetAdvancedWorkshopRecipes();
                advancedRecipes.InsertRange(0, basicWorkshopRecipes);

                AddCategory(category: Category.Advanced, recipeList: advancedRecipes);

                // master workshop (crystal)

                var masterRecipes = CraftData.GetMasterWorkshopRecipes();
                masterRecipes.InsertRange(0, advancedRecipes);

                AddCategory(category: Category.Master, recipeList: masterRecipes);
            }

            {
                // alchemy

                var alchemyRecipes = CraftData.GetAlchemyRecipes();
                AddCategory(category: Category.Alchemy, recipeList: alchemyRecipes);
            }

            {
                // furnace

                var furnaceRecipes = CraftData.GetFurnaceRecipes();
                AddCategory(category: Category.Furnace, recipeList: furnaceRecipes);
            }

            {
                // anvil

                var anvilRecipes = CraftData.GetAnvilRecipes();
                AddCategory(category: Category.Anvil, recipeList: anvilRecipes);
            }

            {
                // leather

                List<Recipe> basicLeatherRecipes = CraftData.GetBasicLeatherRecipes();
                AddCategory(category: Category.LeatherBasic, recipeList: basicLeatherRecipes);

                List<Recipe> advancedLeatherRecipes = CraftData.GetAdvancedLeatherRecipes();
                advancedLeatherRecipes.InsertRange(0, basicLeatherRecipes);
                AddCategory(category: Category.LeatherAdvanced, recipeList: advancedLeatherRecipes);
            }

            CheckIfRecipesAreCorrect();
        }

        private static void CheckIfRecipesAreCorrect()
        {
            foreach (List<Recipe> recipeList in recipesByCategory.Values)
            {
                foreach (Recipe recipe in recipeList)
                {
                    if (!PieceInfo.GetInfo(recipe.pieceToCreate).canBePickedUp && recipe.amountToCreate > 1) throw new ArgumentException($"Cannot create multiple pieces in the field - {recipe.pieceToCreate}.");

                    if (recipe.amountToCreate > 1 && recipe.isReversible) throw new ArgumentException($"Found recipe for amount > 1, that can be reversed - {recipe.pieceToCreate}."); // this could lead to item duplication exploit
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
