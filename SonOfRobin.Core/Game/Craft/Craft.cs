﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Craft
    {
        public enum Category : byte
        {
            Field,
            Essential,
            Basic,
            Advanced,
            Master,
            Anvil,
            LeatherBasic,
            LeatherAdvanced,
        }

        public class Recipe
        {
            private static readonly Dictionary<string, Recipe> recipeByID = new();

            public readonly string id;
            public readonly string readableID;
            public readonly PieceTemplate.Name pieceToCreate;
            public readonly int amountToCreate;
            public readonly Dictionary<PieceTemplate.Name, byte> ingredients;
            private readonly float fatigue;
            private readonly int duration;
            public readonly int maxLevel;
            public readonly int craftCountToLevelUp;
            public readonly float masterLevelFatigueMultiplier;
            public readonly float masterLevelDurationMultiplier;
            public readonly List<PieceTemplate.Name> unlocksWhenCrafted;
            public readonly int craftCountToUnlock;
            public readonly bool isHidden;
            public readonly bool isReversible;
            public readonly int useOnlyIngredientsWithID; // for identifying correct seed when planting

            public Recipe(PieceTemplate.Name pieceToCreate, Dictionary<PieceTemplate.Name, byte> ingredients, float fatigue, int duration = -1, bool isReversible = false, int amountToCreate = 1, bool isHidden = false, List<PieceTemplate.Name> unlocksWhenCrafted = null, int craftCountToUnlock = 1, int maxLevel = -1, int craftCountToLevelUp = -1, float fatigueMultiplier = 0.5f, float durationMultiplier = 0.5f, bool isTemporary = false, int useOnlyIngredientsWithID = -1)
            {
                this.pieceToCreate = pieceToCreate;
                this.amountToCreate = amountToCreate;
                this.id = $"{(int)this.pieceToCreate}_{this.amountToCreate}";
                this.readableID = $"{this.pieceToCreate}_{this.amountToCreate}";
                this.ingredients = ingredients;
                this.fatigue = fatigue;
                this.duration = duration == -1 ? (int)(this.fatigue * 10) : duration; // if duration was not specified, it will be calculated from fatigue

                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(pieceToCreate);
                bool isEquipOrConstructionSite = pieceInfo.type == typeof(Equipment) || pieceInfo.type == typeof(ConstructionSite);

                if (maxLevel == -1)
                {
                    maxLevel = pieceInfo.canBePickedUp ? 4 : 2;
                    if (isEquipOrConstructionSite) maxLevel = 1;
                }
                this.maxLevel = maxLevel;
                if (craftCountToLevelUp == -1) craftCountToLevelUp = pieceInfo.canBePickedUp && !isEquipOrConstructionSite ? 3 : 1;
                this.craftCountToLevelUp = craftCountToLevelUp;

                this.masterLevelFatigueMultiplier = fatigueMultiplier;
                this.masterLevelDurationMultiplier = durationMultiplier;
                this.isHidden = isHidden;
                this.unlocksWhenCrafted = unlocksWhenCrafted == null ? new List<PieceTemplate.Name> { } : unlocksWhenCrafted;
                this.craftCountToUnlock = craftCountToUnlock;
                this.isReversible = isReversible;
                if (this.isReversible) Yield.antiCraftRecipes[this.pieceToCreate] = this;
                this.useOnlyIngredientsWithID = useOnlyIngredientsWithID;

                if (recipeByID.ContainsKey(this.id)) throw new ArgumentException($"Recipe with ID {this.id} has already been added.");
                if (this.masterLevelFatigueMultiplier > 1) throw new ArgumentException($"Master level fatigue multiplier ({this.masterLevelFatigueMultiplier}) cannot be greater than 1.");
                if (this.masterLevelDurationMultiplier > 1) throw new ArgumentException($"Master level duration multiplier ({this.masterLevelDurationMultiplier}) cannot be greater than 1.");
                if (this.maxLevel < 0) throw new ArgumentException($"Max level ({this.maxLevel}) cannot be less than 0.");
                if (!isTemporary) recipeByID[this.id] = this;
            }

            public static Recipe GetRecipeByID(string id)
            {
                return recipeByID[id];
            }

            private float GetRecipeLevelMultiplier(CraftStats craftStats)
            {
                if (this.maxLevel == 0) return 0;

                int recipeLevel = craftStats.GetRecipeLevel(this);
                float levelMultiplier = (float)recipeLevel / (float)this.maxLevel;
                return levelMultiplier;
            }

            public float GetRealFatigue(CraftStats craftStats, Player player)
            {
                float recipeLevelMultiplier = this.GetRecipeLevelMultiplier(craftStats) * 0.6f;
                float craftLevelMultiplier = (float)player.CraftLevel / (float)Player.maxCraftLevel * 0.4f;
                // max possible sum of both multipliers should == 1

                float fatigueDifferenceForMasterLevel = this.fatigue * (1f - this.masterLevelFatigueMultiplier);
                float fatigueDifference = fatigueDifferenceForMasterLevel * (recipeLevelMultiplier + craftLevelMultiplier);

                float realFatigue = this.fatigue - fatigueDifference;
                if (player.Skill == Player.SkillName.Crafter) realFatigue *= 0.7f;
                //  SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} real fatigue {realFatigue}");
                return realFatigue;
            }

            public int GetRealDuration(CraftStats craftStats, Player player)
            {
                float recipeLevelMultiplier = this.GetRecipeLevelMultiplier(craftStats) * 0.6f;
                float craftLevelMultiplier = (float)player.CraftLevel / (float)Player.maxCraftLevel * 0.4f;
                // max possible sum of both multipliers should == 1

                float durationDifferenceForMasterLevel = this.duration * (1f - this.masterLevelDurationMultiplier);
                float durationDifference = durationDifferenceForMasterLevel * (recipeLevelMultiplier + craftLevelMultiplier);

                int realDuration = (int)(this.duration - durationDifference);
                if (player.Skill == Player.SkillName.Crafter) realDuration = (int)((float)realDuration * 0.7f);
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} real duration {realDuration}");

                return realDuration;
            }

            public Yield ConvertToYield()
            {
                var finalDroppedPieces = new List<Yield.DroppedPiece> { };

                foreach (var kvp in this.ingredients)
                {
                    PieceTemplate.Name ingredientName = kvp.Key;
                    int ingredientCount = kvp.Value;

                    if (CraftData.namesExcludedFromAntiCraft.Contains(ingredientName)) continue;

                    for (int i = 0; i < ingredientCount; i++)
                    {
                        finalDroppedPieces.Add(new Yield.DroppedPiece(pieceName: ingredientName, chanceToDrop: 70, maxNumberToDrop: 1));
                    }
                }

                BoardPiece.Category category = PieceInfo.GetInfo(pieceToCreate).category;
                List<ParticleEngine.Preset> debrisTypeList = category switch
                {
                    BoardPiece.Category.Wood => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                    BoardPiece.Category.Stone => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                    BoardPiece.Category.Metal => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                    BoardPiece.Category.SmallPlant => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                    BoardPiece.Category.Flesh => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisBlood },
                    BoardPiece.Category.Leather => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                    BoardPiece.Category.Crystal => new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisCrystal },
                    BoardPiece.Category.Indestructible => new List<ParticleEngine.Preset>(),
                    _ => throw new ArgumentException($"Unsupported category - '{category}'."),
                };

                Yield.antiCraftRecipes[this.pieceToCreate] = this;
                return new Yield(firstDroppedPieces: new List<Yield.DroppedPiece> { }, finalDroppedPieces: finalDroppedPieces, firstDebrisTypeList: debrisTypeList, multipliedByBonus: false);
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

                var storagesToTakeFrom = player.GetCraftStoragesToTakeFrom(showCraftMarker: false);
                var storagesToPutInto = player.CraftStoragesToPutInto;

                if (!this.CheckIfStorageContainsAllIngredients(storageList: storagesToTakeFrom))
                {
                    new TextWindow(text: "Not enough ingredients.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, startingSound: SoundData.Name.Error);
                    return craftedPieces;
                }

                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.pieceToCreate);
                bool canBePickedUp = pieceInfo.canBePickedUp;

                if (canBePickedUp && !PieceStorage.StorageListCanFitSpecifiedPieces(storageList: storagesToPutInto, pieceName: this.pieceToCreate, quantity: this.amountToCreate))
                {
                    foreach (PieceStorage storage in storagesToPutInto) storage.Sort(); // trying to make room in storages by sorting pieces

                    if (!craftOnTheGround && !PieceStorage.StorageListCanFitSpecifiedPieces(storageList: storagesToPutInto, pieceName: this.pieceToCreate, quantity: this.amountToCreate))
                    {
                        var craftParams = new Dictionary<string, object> { { "recipe", this }, { "craftOnTheGround", true } };

                        var confirmationData = new Dictionary<string, Object> {
                            { "taskName", Scheduler.TaskName.Craft },
                            { "executeHelper", craftParams } };

                        Sound.QuickPlay(SoundData.Name.Notification4);
                        MenuTemplate.CreateConfirmationMenu(question: "Not enough inventory space to craft. Craft on the ground?", confirmationData: confirmationData, blocksUpdatesBelow: true);
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

                // build mode advances island clock (and adds fatigue) gradually, not all at once
                if (canBePickedUp)
                {
                    world.islandClock.Advance(amount: this.GetRealDuration(craftStats: world.craftStats, player: world.Player), ignorePause: true);
                    player.Fatigue += this.GetRealFatigue(craftStats: world.craftStats, player: world.Player);
                    player.Fatigue = Math.Min(world.Player.Fatigue, world.Player.maxFatigue - 20); // to avoid falling asleep just after crafting
                }

                // crafting

                world.craftStats.RegisterRecipeUse(recipe: this);

                // checking smart crafting - randomly reducing used ingredients count

                var ingredientsCopy = new Dictionary<PieceTemplate.Name, byte>(this.ingredients); // copy - to allow reducing ingredient count

                int smartCraftChance = 30; // smaller number == higher chance
                smartCraftChance /= player.CraftLevel;

                int recipeLevel = world.craftStats.GetRecipeLevel(this);
                if (recipeLevel > 0 || recipeLevel < this.maxLevel) smartCraftChance /= 2;
                if (recipeLevel == this.maxLevel) smartCraftChance /= 3;

                if (world.random.Next(smartCraftChance) == 0)
                {
                    var multipleIngredientNames = this.ingredients.Where(kvp => kvp.Value > 1).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).Keys;

                    if (multipleIngredientNames.Count > 0)
                    {
                        PieceTemplate.Name randomNameToReduce = multipleIngredientNames.ElementAt(world.random.Next(multipleIngredientNames.Count));
                        byte quantity = ingredientsCopy[randomNameToReduce];

                        float reduceMultiplier = (float)(0.2 + (world.random.NextSingle() * 0.3)); // 0.2 - 0.5
                        byte amountToReduce = Math.Max((byte)(quantity * reduceMultiplier), (byte)1);
                        ingredientsCopy[randomNameToReduce] -= amountToReduce;
                        world.craftStats.AddSmartCraftingReducedAmount(ingredientName: randomNameToReduce, reducedAmount: amountToReduce);
                    }
                }

                PieceStorage.DestroySpecifiedPiecesInMultipleStorages(storageList: storagesToTakeFrom, quantityByPiece: ingredientsCopy, withThisIDOnly: this.useOnlyIngredientsWithID);

                if (canBePickedUp)
                {
                    for (int i = 0; i < this.amountToCreate; i++)
                    {
                        BoardPiece piece = PieceTemplate.CreatePiece(templateName: this.pieceToCreate, world: world, createdByPlayer: true);

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

                    BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(templateName: this.pieceToCreate, world: world, position: player.simulatedPieceToBuild.sprite.position, ignoreCollisions: true, createdByPlayer: true);
                    craftedPieces.Add(piece);

                    if (!piece.sprite.IsOnBoard) throw new ArgumentException($"Piece has not been placed correctly on the board - {piece.name}.");

                    if (piece.GetType() == typeof(Plant))
                    {
                        FertileGround fertileGround = Plant.GetFertileGround(piece);

                        ((Plant)piece).massTakenMultiplier *= fertileGround.pieceInfo.fertileGroundSoilWealthMultiplier; // when the player plants something, it should grow better than normal
                    }
                }

                // unlocking other recipes and showing messages

                if (showMessages) this.UnlockNewRecipesAndShowSummary(world: world, craftedPieces: craftedPieces);

                return craftedPieces;
            }

            public void UnlockNewRecipesAndShowSummary(World world, List<BoardPiece> craftedPieces = null)
            {
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.pieceToCreate);

                string creationType = pieceInfo.type == typeof(Plant) ? "planted" : "crafted";

                string pieceName = Helpers.FirstCharToUpperCase(PieceInfo.GetInfo(this.pieceToCreate).readableName);

                string message = this.amountToCreate == 1 ?
                    $"|  {pieceName} has been {creationType}." :
                    $"|  {pieceName} x{this.amountToCreate} has been {creationType}.";

                bool tutorialAdded = false;

                var taskChain = new List<Object>();

                SoundData.Name soundName = !pieceInfo.canBePickedUp && pieceInfo.type != typeof(Plant) ? SoundData.Name.Ding1 : SoundData.Name.Ding3;

                taskChain.Add(new HintMessage(text: message, boxType: HintMessage.BoxType.GreenBox, delay: 0, blockInputDefaultDuration: false, useTransition: true,
                    imageList: new List<ImageObj> { PieceInfo.GetImageObj(this.pieceToCreate) }, startingSound: soundName).ConvertToTask());

                if (world.craftStats.LastCraftWasSmart)
                {
                    taskChain.Add(new HintMessage(text: $"Used less ingredients: | x{world.craftStats.LastSmartCraftReducedIngredientCount}", boxType: HintMessage.BoxType.GreenBox, delay: 0, blockInputDefaultDuration: false, useTransition: true,
                        imageList: new List<ImageObj> { PieceInfo.GetImageObj(world.craftStats.LastSmartCraftReducedIngredientName) }, startingSound: SoundData.Name.Ding1).ConvertToTask());

                    if (!tutorialAdded && !world.HintEngine.shownTutorials.Contains(Tutorials.Type.SmartCrafting))
                    {
                        Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                        { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.SmartCrafting, world: world, ignoreDelay: true); };
                        taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, storeForLaterUse: true, executeHelper: showTutorialDlgt));
                        tutorialAdded = true;
                    }

                    world.craftStats.ResetLastSmartCraft();
                }

                if (craftedPieces != null)
                {
                    BoardPiece firstCraftedPiece = craftedPieces[0];
                    var pieceType = firstCraftedPiece.GetType();

                    if ((pieceType == typeof(Tool) || pieceType == typeof(Projectile)) &&
                        world.random.Next(14 - ((world.Player.CraftLevel - 1) * 3)) == 0)
                    {
                        int bonusHitPoints = world.random.Next((int)(firstCraftedPiece.maxHitPoints * 0.2f), (int)(firstCraftedPiece.maxHitPoints * 0.7f));

                        taskChain.Add(new HintMessage(text: $"| {Helpers.FirstCharToUpperCase(firstCraftedPiece.readableName)}\n| +{bonusHitPoints} bonus hit points!\n| {firstCraftedPiece.maxHitPoints} | {firstCraftedPiece.maxHitPoints + bonusHitPoints}", boxType: HintMessage.BoxType.GreenBox, delay: 0, blockInputDefaultDuration: false, useTransition: true,
                            imageList: new List<ImageObj> { firstCraftedPiece.pieceInfo.imageObj, TextureBank.GetImageObj(TextureBank.TextureName.SimpleArrowUp), TextureBank.GetImageObj(TextureBank.TextureName.SimpleHeart), TextureBank.GetImageObj(TextureBank.TextureName.SimpleArrowRight) }, startingSound: SoundData.Name.Ding1).ConvertToTask());

                        foreach (BoardPiece craftedPiece in craftedPieces)
                        {
                            craftedPiece.maxHitPoints += bonusHitPoints;
                            craftedPiece.HitPoints = craftedPiece.maxHitPoints;
                        }
                    }
                }

                HintEngine hintEngine = world.HintEngine;

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

                bool recipeLevelUp = world.craftStats.RecipeJustLevelledUp(this);
                if (recipeLevelUp)
                {
                    int recipeLevel = world.craftStats.GetRecipeLevel(this);
                    bool levelMaster = recipeLevel == this.maxLevel;
                    string recipeNewLevelName = levelMaster ? "master |" : $"{recipeLevel + 1}";

                    var imageList = new List<ImageObj> { PieceInfo.GetImageObj(this.pieceToCreate) };
                    if (levelMaster) imageList.Add(TextureBank.GetImageObj(TextureBank.TextureName.Star));

                    taskChain.Add(new HintMessage(text: $"{pieceName} |\nRecipe level up!\n       {recipeLevel} -> {recipeNewLevelName}", imageList: imageList, boxType: levelMaster ? HintMessage.BoxType.GoldBox : HintMessage.BoxType.LightBlueBox, delay: 0, blockInputDefaultDuration: false, animate: true, useTransition: true, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1).ConvertToTask());
                }

                if (unlockedPieces.Count > 0)
                {
                    Menu.RebuildAllMenus();

                    string unlockedRecipesMessage = unlockedPieces.Count == 1 ? "New recipe unlocked" : "New recipes unlocked:\n";
                    var imageList = new List<ImageObj>();

                    foreach (PieceTemplate.Name name in unlockedPieces)
                    {
                        PieceInfo.Info unlockedPieceInfo = PieceInfo.GetInfo(name);
                        unlockedRecipesMessage += $"\n|  {unlockedPieceInfo.readableName}";
                        imageList.Add(unlockedPieceInfo.imageObj);
                    }

                    taskChain.Add(new HintMessage(text: unlockedRecipesMessage, imageList: imageList, boxType: HintMessage.BoxType.LightBlueBox, delay: 0, blockInputDefaultDuration: false, animate: true, useTransition: true, startingSound: SoundData.Name.Notification1).ConvertToTask());
                }

                bool craftLevelUp = world.Player.CheckForCraftLevelUp();
                if (craftLevelUp)
                {
                    Player player = world.Player;

                    bool levelMaster = player.CraftLevel == Player.maxCraftLevel;
                    // levelMaster = true; // for testing

                    string newLevelName = levelMaster ? "master |" : $"{player.CraftLevel}";

                    var imageList = new List<ImageObj> { PieceInfo.GetImageObj(PieceTemplate.Name.WorkshopMaster) };
                    if (levelMaster) imageList.Add(TextureBank.GetImageObj(TextureBank.TextureName.Star));

                    taskChain.Add(new HintMessage(text: $"| Craft level up!\n       Level {player.CraftLevel - 1} -> {newLevelName}", imageList: imageList, boxType: levelMaster ? HintMessage.BoxType.GoldBox : HintMessage.BoxType.LightBlueBox, delay: 0, blockInputDefaultDuration: false, animate: true, useTransition: true, startingSound: levelMaster ? SoundData.Name.Chime : SoundData.Name.Notification1).ConvertToTask());
                }

                if (!pieceInfo.canBePickedUp)
                {
                    taskChain.Insert(0, new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));
                }

                var executeHelper = new Dictionary<string, Object>();
                if (pieceInfo.canBePickedUp) executeHelper["newOwnedPiece"] = this.pieceToCreate;
                else executeHelper["fieldPiece"] = this.pieceToCreate;

                if (!tutorialAdded && craftLevelUp && !world.HintEngine.shownTutorials.Contains(Tutorials.Type.GeneralCraftLevels))
                {
                    Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                    { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.GeneralCraftLevels, world: world, ignoreDelay: true); };

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, storeForLaterUse: true, executeHelper: showTutorialDlgt));
                    tutorialAdded = true;
                }

                if (!tutorialAdded && recipeLevelUp && !world.HintEngine.shownTutorials.Contains(Tutorials.Type.CraftRecipeLevels))
                {
                    Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                    { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.CraftRecipeLevels, world: world, ignoreDelay: true); };

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, storeForLaterUse: true, executeHelper: showTutorialDlgt));
                    tutorialAdded = true;
                }

                if (!tutorialAdded && world.Player.ResourcefulCrafter && !world.HintEngine.shownTutorials.Contains(Tutorials.Type.ResourcefulCrafting))
                {
                    Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                    { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.ResourcefulCrafting, world: world, ignoreDelay: true); };

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, storeForLaterUse: true, executeHelper: showTutorialDlgt));
                    tutorialAdded = true;
                }

                if (pieceInfo.canBePickedUp)
                {
                    // showing in menu
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CheckForPieceHints, delay: 0, storeForLaterUse: true, executeHelper: executeHelper));
                }
                else
                {
                    // showing on the field (LevelEvent works better for this case)
                    new LevelEvent(eventName: LevelEvent.EventName.CheckForPieceHints, level: world.ActiveLevel, delay: 60 * 1, boardPiece: null, eventHelper: executeHelper);
                }

                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, executeHelper: taskChain);
                new RumbleEvent(force: 0.15f, bigMotor: true, fadeInSeconds: 0.2f, durationSeconds: 0, fadeOutSeconds: 0.2f);
            }
        }

        private static readonly Dictionary<Category, List<Recipe>> recipesByCategory = new() { };

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

        private static List<Recipe> HiddenRecipes
        { get { return AllRecipes.Where(recipe => recipe.isHidden).ToList(); } }

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
            recipesByCategory[category] = recipeList.ToList();
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

        public static void UnlockRecipesAddedInGameUpdate(World world)
        {
            var unlockedPieces = new List<PieceTemplate.Name>();

            foreach (Recipe recipe in AllRecipes)
            {
                foreach (PieceTemplate.Name newUnlockedPiece in recipe.unlocksWhenCrafted)
                {
                    if (!world.discoveredRecipesForPieces.Contains(newUnlockedPiece))
                    {
                        if (world.craftStats.HowManyTimesHasBeenCrafted(recipe) >= recipe.craftCountToUnlock)
                        {
                            unlockedPieces.Add(newUnlockedPiece);
                            world.discoveredRecipesForPieces.Add(newUnlockedPiece);
                        }
                    }
                }
            }

            if (unlockedPieces.Count > 0)
            {
                Menu.RebuildAllMenus();

                string unlockedRecipesMessage = unlockedPieces.Count == 1 ? "New recipe unlocked (game update)" : "New recipes unlocked (game update):\n";
                var imageList = new List<ImageObj>();

                foreach (PieceTemplate.Name name in unlockedPieces)
                {
                    PieceInfo.Info unlockedPieceInfo = PieceInfo.GetInfo(name);
                    unlockedRecipesMessage += $"\n|  {unlockedPieceInfo.readableName}";
                    imageList.Add(unlockedPieceInfo.imageObj);
                }

                Sound.QuickPlay(SoundData.Name.Notification1);
                HintEngine.ShowMessageDuringPause(new List<HintMessage> { new HintMessage(text: unlockedRecipesMessage, blockInputDefaultDuration: true, useTransition: true, imageList: imageList, boxType: HintMessage.BoxType.GreenBox) });
            }
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

                    if (!nextLevelRecipeFound)
                    {
                        // for recipes unlocked in PieceHints
                        foreach (PieceHint pieceHint in PieceHint.pieceHintList)
                        {
                            if (pieceHint.recipesToUnlock != null && pieceHint.recipesToUnlock.Contains(currentHiddenRecipe.pieceToCreate))
                            {
                                recipeCanBeUnlocked = true;
                                break;
                            }
                        }

                        break;
                    };
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