using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public struct PieceHint
    {
        public enum Type : byte
        {
            CrateAnother = 1,
            TentModernPacked = 72,
            WoodNegative = 2,
            WoodPositive = 3,
            DigSiteNegative = 4,
            DigSitePositive = 5,
            StoneNegative = 6,
            StonePositive = 7,
            CrystalNegative = 8,
            CrystalPositive = 9,
            AnimalNegative = 10,
            AnimalBow = 11,
            AnimalSpear = 12,
            AnimalAxe = 13,
            BowNoAmmo = 14,
            ClamField = 15,
            ClamInventory = 16,
            FruitTree = 17,
            BananaTree = 18,
            TomatoPlant = 19,
            IronDepositNegative = 20,
            IronDepositPositive = 21,
            CoalDepositNegative = 22,
            CoalDepositPositive = 23,
            HotPlate = 24,
            Cooker = 25,
            LeatherPositive = 26,
            BackpackPositive = 27,
            BeltPositive = 28,
            MapPositive = 29,
            RedExclamation = 30,
            Acorn = 31,
            TorchNegative = 32,
            TorchPositive = 33,
            Fireplace = 34,
            HerbsRed = 35,
            HerbsYellow = 36,
            HerbsViolet = 37,
            HerbsCyan = 38,
            HerbsBlue = 39,
            HerbsBlack = 40,
            GlassSand = 41,
            CanBuildWorkshop = 42,
            DangerousBear = 43,
            DigSiteGlass = 44,
            CarrotPlant = 45,
            ExplosiveGas = 46,
            TreasureJar = 47,
            CandleForLantern = 48,
            CoffeeRaw = 49,
            SharedChest = 50,
            CanDestroyEssentialWorkshop = 51,
            AlchemyLab = 52,
            PoisonousMeat = 53,
            MakeOilPositiveFat = 54,
            MakeOilNegative = 55,
            MakeOilPositiveSeeds = 56,
            PlantingNegative = 57,
            PlantingPositive = 58,
            WoodenFenceNegative = 59,
            WoodenFencePositive = 60,
            DeadAnimal = 61,
            HarvestingWorkshop = 62,
            SwampDigSite = 64,

            CineCrateStarting = 0,
            CineTentModernCantPack = 73,
            ConstructionSite = 76,
            CineSmallBase = 65,
            CineRuins = 74,
            CineCave = 75,
            CineTotem = 63,
            CineLookForSurvivors1 = 66,
            CineLookForSurvivors2 = 67,
            CineLookForSurvivors3 = 68,
            CineLookForSurvivors4 = 77,
            CineDay1 = 69,
            CineDay2 = 70,
            CineDay3 = 71,
        }

        public static readonly Type[] allTypes = (Type[])Enum.GetValues(typeof(Type));

        public static readonly List<PieceHint> pieceHintList = new();
        private static int nearbyPiecesFrameChecked = 0;
        private static readonly List<BoardPiece> nearbyPieces = new();

        public readonly Type type;
        private readonly Type[] alsoDisables;
        private readonly HintMessage[] messageList;
        private readonly HashSet<PieceTemplate.Name> fieldPiecesNearby;
        private readonly Tutorials.Type[] tutorialsToActivate;
        private readonly HintEngine.Type generalHintToActivate;
        private readonly PieceTemplate.Name[] recipesToUnlock;
        private readonly HintEngine.Type[] shownGeneralHints;
        private readonly HashSet<Type> shownPieceHints;
        private readonly Tutorials.Type[] shownTutorials;
        private readonly float distanceWalkedKilometers;
        private readonly float mapDiscoveredPercentage; // 0 - 1
        private readonly int islandTimeElapsedHours;
        private readonly bool fieldPieceHasNotEmptyStorage; // can be used for checking for fruits, etc.
        private readonly HashSet<PieceTemplate.Name> playerOwnsAnyOfThesePieces;
        private readonly PieceTemplate.Name[] playerEquipmentDoesNotContainThesePieces;
        private readonly PieceTemplate.Name[] playerOwnsAllOfThesePieces;
        private readonly PieceTemplate.Name[] playerDoesNotOwnAnyOfThesePieces;
        private readonly HashSet<IslandClock.PartOfDay> partsOfDay;
        private readonly CountComparison[] piecesCraftedCount;
        private readonly Dictionary<PieceTemplate.Name, int> usedIngredientsCount;
        private readonly Dictionary<PieceTemplate.Name, int> existingPiecesCount;
        private readonly bool fieldOnly;
        private readonly bool menuOnly;
        private readonly bool ignoreHintSetting;
        private readonly bool showCineCurtains;

        public PieceHint(Type type, HashSet<PieceTemplate.Name> fieldPiecesNearby = null, HashSet<PieceTemplate.Name> playerOwnsAnyOfThesePieces = null, PieceTemplate.Name[] playerEquipmentDoesNotContainThesePieces = null, PieceTemplate.Name[] playerDoesNotOwnAnyOfThesePieces = null, PieceTemplate.Name[] playerOwnsAllOfThesePieces = null, Type[] alsoDisables = null, bool fieldPieceHasNotEmptyStorage = false, string message = null, List<Texture2D> imageList = null, HintMessage[] messageList = null, Tutorials.Type[] tutorialsToActivate = null, HintEngine.Type generalHintToActivate = HintEngine.Type.Empty, PieceTemplate.Name[] recipesToUnlock = null, HashSet<IslandClock.PartOfDay> partsOfDay = null, CountComparison[] piecesCraftedCount = null, Dictionary<PieceTemplate.Name, int> usedIngredientsCount = null, Dictionary<PieceTemplate.Name, int> existingPiecesCount = null, HintEngine.Type[] shownGeneralHints = null, HashSet<Type> shownPieceHints = null, Tutorials.Type[] shownTutorials = null, float distanceWalkedKilometers = 0, float mapDiscoveredPercentage = 0, int islandTimeElapsedHours = 0, bool fieldOnly = false, bool menuOnly = false, bool ignoreHintSetting = false, bool showCineCurtains = false)
        {
            this.type = type;
            this.alsoDisables = alsoDisables == null ? new Type[] { } : alsoDisables;

            this.fieldPiecesNearby = fieldPiecesNearby;
            this.fieldPieceHasNotEmptyStorage = fieldPieceHasNotEmptyStorage;
            this.playerOwnsAnyOfThesePieces = playerOwnsAnyOfThesePieces;
            this.playerEquipmentDoesNotContainThesePieces = playerEquipmentDoesNotContainThesePieces;
            this.playerOwnsAllOfThesePieces = playerOwnsAllOfThesePieces;
            this.playerDoesNotOwnAnyOfThesePieces = playerDoesNotOwnAnyOfThesePieces;
            this.partsOfDay = partsOfDay;
            this.piecesCraftedCount = piecesCraftedCount;
            this.usedIngredientsCount = usedIngredientsCount;
            this.existingPiecesCount = existingPiecesCount;
            this.shownGeneralHints = shownGeneralHints;
            this.shownPieceHints = shownPieceHints;
            this.shownTutorials = shownTutorials;
            this.distanceWalkedKilometers = distanceWalkedKilometers;
            this.mapDiscoveredPercentage = mapDiscoveredPercentage;
            this.islandTimeElapsedHours = islandTimeElapsedHours;

            this.fieldOnly = fieldOnly || this.fieldPiecesNearby != null; // fieldPiecesNearby need to be shown on the field
            this.menuOnly = menuOnly;

            this.ignoreHintSetting = ignoreHintSetting;

            if (this.fieldOnly && this.menuOnly) throw new ArgumentException("fieldOnly and menuOnly cannot both be active.");

            this.messageList = messageList;
            if (message != null) this.messageList = new HintMessage[] { new HintMessage(text: message, imageList: imageList, blockInput: true) };
            this.tutorialsToActivate = tutorialsToActivate;
            this.generalHintToActivate = generalHintToActivate;
            this.recipesToUnlock = recipesToUnlock;
            this.showCineCurtains = showCineCurtains;

            if (this.generalHintToActivate != HintEngine.Type.Empty)
            {
                if (this.tutorialsToActivate != null) throw new ArgumentException("General hint and tutorial cannot both be active.");
                if (this.messageList?.Length > 0) throw new ArgumentException("General hint and message list cannot both be active.");
            }
        }

        public static void RefreshData()
        {
            pieceHintList.Clear();
            pieceHintList.AddRange(PieceHintData.GetData());
        }

        private static List<BoardPiece> GetNearbyPieces(Player player)
        {
            if (SonOfRobinGame.CurrentUpdate != nearbyPiecesFrameChecked)
            {
                nearbyPieces.Clear();

                var nearbyPiecesTempList = player.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 300).OrderBy(piece => Vector2.Distance(player.sprite.position, piece.sprite.position)).ToList();

                nearbyPieces.AddRange(nearbyPiecesTempList);
                nearbyPiecesFrameChecked = SonOfRobinGame.CurrentUpdate;
            }

            return nearbyPieces;
        }

        private readonly List<HintMessage> GetTutorials(HashSet<Tutorials.Type> shownTutorials)
        {
            var messageList = new List<HintMessage> { };
            if (this.tutorialsToActivate == null || !Preferences.showHints) return messageList;

            foreach (Tutorials.Type type in this.tutorialsToActivate)
            {
                if (!shownTutorials.Contains(type))
                {
                    messageList.AddRange(Tutorials.tutorials[type].MessagesToDisplay);
                    shownTutorials.Add(type);
                }
            }

            return messageList;
        }

        private void Show(World world)
        {
            if (this.showCineCurtains) world.cineCurtains.Enabled = true;

            if (this.generalHintToActivate != HintEngine.Type.Empty)
            {
                world.HintEngine.ShowGeneralHint(type: this.generalHintToActivate, ignoreDelay: true);
                return;
            }

            var messagesToDisplay = this.messageList.ToList();

            if (this.recipesToUnlock != null)
            {
                string unlockedRecipesMessage = this.recipesToUnlock.Length == 1 ? "New recipe unlocked" : "New recipes unlocked:\n";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name name in this.recipesToUnlock)
                {
                    if (!world.discoveredRecipesForPieces.Contains(name)) world.discoveredRecipesForPieces.Add(name);

                    PieceInfo.Info unlockedPieceInfo = PieceInfo.GetInfo(name);
                    unlockedRecipesMessage += $"\n|  {unlockedPieceInfo.readableName}";
                    imageList.Add(unlockedPieceInfo.texture);
                }

                messagesToDisplay.AddRange(new List<HintMessage> { new HintMessage(text: unlockedRecipesMessage, imageList: imageList, startingSound: SoundData.Name.Notification1) });
            }

            messagesToDisplay.AddRange(this.GetTutorials(world.HintEngine.shownTutorials));

            if (this.fieldPiecesNearby != null)
            {
                BoardPiece nearbyPiece = this.GetFirstCorrectFieldPieceNearby(world.Player);
                Sound.QuickPlay(SoundData.Name.Notification3);
                HintEngine.ShowPieceDuringPause(world: world, pieceToShow: nearbyPiece, messageList: messagesToDisplay);
            }
            else
            {
                Sound.QuickPlay(SoundData.Name.Notification3);
                HintEngine.ShowMessageDuringPause(messagesToDisplay);
            }
        }

        public static bool CheckForHintToShow(HintEngine hintEngine, Player player, bool ignoreInputActive = false, List<Type> typesToCheckOnly = null, PieceTemplate.Name fieldPieceNameToCheck = PieceTemplate.Name.Empty, PieceTemplate.Name newOwnedPieceNameToCheck = PieceTemplate.Name.Empty)
        {
            if (!player.world.inputActive && !ignoreInputActive) return false;
            MessageLog.Add(debugMessage: true, text: "Checking piece hints.");

            if (Scheduler.HasTaskChainInQueue || player.sleepMode != Player.SleepMode.Awake) return false;
            // only one hint should be shown at once - waitingScenes cause playing next scene after turning off CineMode (playing scene without game being paused)

            foreach (PieceHint hint in pieceHintList)
            {
                if (typesToCheckOnly != null && !typesToCheckOnly.Contains(hint.type)) continue;

                if (hint.CheckIfConditionsAreMet(player: player, fieldPieceNameToCheck: fieldPieceNameToCheck, newOwnedPieceNameToCheck: newOwnedPieceNameToCheck))
                {
                    hint.Show(world: player.world);
                    hintEngine.Disable(hint.type);
                    foreach (Type type in hint.alsoDisables)
                    {
                        hintEngine.Disable(type);
                    }

                    return true; // only one hint should be shown at once
                }
            }

            return false;
        }

        private bool CheckIfConditionsAreMet(Player player, PieceTemplate.Name fieldPieceNameToCheck = PieceTemplate.Name.Empty, PieceTemplate.Name newOwnedPieceNameToCheck = PieceTemplate.Name.Empty)
        {
            World world = player.world;

            if (!Preferences.showHints && !this.ignoreHintSetting) return false;
            if (world.HintEngine.shownPieceHints.Contains(this.type)) return false;
            if (this.menuOnly && Scene.GetTopSceneOfType(typeof(Menu)) == null) return false;
            if (this.fieldOnly && Scene.GetTopSceneOfType(typeof(Menu)) != null) return false;

            // if "selective" pieces are present, only hints containing them should be checked

            if (fieldPieceNameToCheck != PieceTemplate.Name.Empty)
            {
                if (this.fieldPiecesNearby == null || !this.fieldPiecesNearby.Contains(fieldPieceNameToCheck)) return false;
            }

            if (newOwnedPieceNameToCheck != PieceTemplate.Name.Empty)
            {
                bool anyListHasNot = this.playerOwnsAnyOfThesePieces == null || !this.playerOwnsAnyOfThesePieces.Contains(newOwnedPieceNameToCheck);
                bool allListHasNot = this.playerOwnsAllOfThesePieces == null || !this.playerOwnsAllOfThesePieces.Contains(newOwnedPieceNameToCheck);

                if (anyListHasNot && allListHasNot) return false;
            }

            // field pieces nearby
            if (this.fieldPiecesNearby != null && this.GetFirstCorrectFieldPieceNearby(player) == null) return false;

            // parts of day
            if (this.partsOfDay != null && !this.partsOfDay.Contains(world.islandClock.CurrentPartOfDay)) return false;

            // player - owns single piece

            if (this.playerOwnsAnyOfThesePieces != null && newOwnedPieceNameToCheck == PieceTemplate.Name.Empty)
            {
                bool playerOwnsSinglePiece = false;

                foreach (PieceTemplate.Name name in this.playerOwnsAnyOfThesePieces)
                {
                    if (CheckIfPlayerOwnsPiece(player: player, name: name))
                    {
                        playerOwnsSinglePiece = true;
                        break;
                    }
                }

                if (!playerOwnsSinglePiece) return false;
            }

            // player - owns all pieces

            if (this.playerOwnsAllOfThesePieces != null && newOwnedPieceNameToCheck == PieceTemplate.Name.Empty)
            {
                foreach (PieceTemplate.Name name in this.playerOwnsAllOfThesePieces)
                {
                    if (!CheckIfPlayerOwnsPiece(player: player, name: name)) return false;
                }
            }

            // player - has none of these items equipped
            if (this.playerEquipmentDoesNotContainThesePieces != null)
            {
                foreach (PieceTemplate.Name name in this.playerEquipmentDoesNotContainThesePieces)
                {
                    if (player.EquipStorage.GetFirstPieceOfName(name: name, removePiece: false) != null) return false;
                }
            }

            // player - does not own any of these pieces

            if (this.playerDoesNotOwnAnyOfThesePieces != null)
            {
                foreach (PieceTemplate.Name name in this.playerDoesNotOwnAnyOfThesePieces)
                {
                    if (CheckIfPlayerOwnsPiece(player: player, name: name)) return false;
                }
            }

            // player - has crafted these pieces

            if (this.piecesCraftedCount != null)
            {
                foreach (CountComparison pieceCraftedCount in this.piecesCraftedCount)
                {
                    if (!pieceCraftedCount.Check(world.craftStats.HowManyHasBeenCrafted(pieceCraftedCount.name))) return false;
                }
            }

            // player - has used these ingredients to craft

            if (this.usedIngredientsCount != null)
            {
                foreach (var kvp in this.usedIngredientsCount)
                {
                    PieceTemplate.Name ingredientName = kvp.Key;
                    int minimumUsedCount = kvp.Value;

                    if (world.craftStats.HowManyIngredientHasBeenUsed(ingredientName) < minimumUsedCount) return false;
                }
            }

            // field pieces anywhere
            if (this.existingPiecesCount != null && !world.SpecifiedPiecesCountIsMet(this.existingPiecesCount)) return false;

            // shown general hints
            if (this.shownGeneralHints != null)
            {
                foreach (HintEngine.Type type in this.shownGeneralHints) if (!world.HintEngine.shownGeneralHints.Contains(type)) return false;
            }

            // shown piece hints
            if (this.shownPieceHints != null)
            {
                foreach (Type type in this.shownPieceHints)
                {
                    if (!world.HintEngine.shownPieceHints.Contains(type)) return false;
                }
            }

            // shown tutorials
            if (this.shownTutorials != null)
            {
                foreach (Tutorials.Type type in this.shownTutorials) if (!world.HintEngine.shownTutorials.Contains(type)) return false;
            }

            // distance walked
            if (this.distanceWalkedKilometers > 0 && player.DistanceWalkedKilometers < this.distanceWalkedKilometers) return false;

            // map discovered percentage
            if (this.mapDiscoveredPercentage > 0 && world.IslandLevel.grid.VisitedCellsPercentage < this.mapDiscoveredPercentage) return false;

            // island time elapsed
            if (this.islandTimeElapsedHours > 0 && world.islandClock.IslandTimeElapsed.TotalHours < this.islandTimeElapsedHours) return false;

            return true;
        }

        private readonly BoardPiece GetFirstCorrectFieldPieceNearby(Player player)
        {
            foreach (BoardPiece piece in GetNearbyPieces(player))
            {
                if (this.fieldPiecesNearby.Contains(piece.name))
                {
                    // fieldPieceHasNotEmptyStorage can be used for checking fruits
                    if (!this.fieldPieceHasNotEmptyStorage || piece.PieceStorage?.OccupiedSlotsCount > 0) return piece;
                }
            }

            return null;
        }

        private static bool CheckIfPlayerOwnsPiece(Player player, PieceTemplate.Name name)
        {
            var playerStorages = new List<PieceStorage> { player.ToolStorage, player.PieceStorage };

            foreach (PieceStorage currentStorage in playerStorages)
            {
                BoardPiece foundPiece = currentStorage.GetFirstPieceOfName(name: name, removePiece: false);
                if (foundPiece != null) return true;
            }

            return false;
        }
    }
}