﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public struct PieceHint
    {
        public enum Type
        { CrateStarting, CrateAnother, WoodNegative, WoodPositive, DigSiteNegative, DigSitePositive, StoneNegative, StonePositive, CrystalNegative, CrystalPositive, AnimalNegative, AnimalBow, AnimalSpear, AnimalAxe, BowNoAmmo, ClamField, ClamInventory, FruitTree, BananaTree, TomatoPlant, IronDepositNegative, IronDepositPositive, CoalDepositNegative, CoalDepositPositive, HotPlate, Cooker, LeatherPositive, BackpackPositive, BeltPositive, MapPositive, RedExclamation, Acorn, TorchNegative, TorchPositive, Fireplace, HerbsRed, HerbsYellow, HerbsViolet, HerbsCyan, HerbsBlue, HerbsBlack, GlassSand, CanBuildWorkshop, SmallBase, DangerousTiger, DigSiteGlass, CarrotPlant, ExplosiveGas, TreasureJar, CandleForLantern, CoffeeRaw, SharedChest, CanDestroyEssentialWorkshop, AlchemyLab, PoisonousMeat, MakeOilPositiveFat, MakeOilNegative, CineLookForSurvivors1, CineLookForSurvivors2, CineLookForSurvivors3, CineDay2, CineDay3, CineDay4, MakeOilPositiveSeeds, PlantingNegative, PlantingPositive, WoodenFenceNegative, WoodenFencePositive }

        public enum Comparison
        { Greater, GreaterOrEqual, Equal, LessOrEqual, Less }

        public static readonly List<PieceHint> pieceHintList = new List<PieceHint>();
        private static int nearbyPiecesFrameChecked = 0;
        private static readonly List<BoardPiece> nearbyPieces = new List<BoardPiece>();

        public readonly Type type;
        private readonly List<Type> alsoDisables;
        private readonly List<HintMessage> messageList;
        private readonly List<PieceTemplate.Name> fieldPiecesNearby;
        private readonly List<Tutorials.Type> tutorialsToActivate;
        private readonly HintEngine.Type generalHintToActivate;
        private readonly List<HintEngine.Type> shownGeneralHints;
        private readonly List<Type> shownPieceHints;
        private readonly List<Tutorials.Type> shownTutorials;
        private readonly float distanceWalkedKilometers;
        private readonly float mapDiscoveredPercentage; // 0 - 1
        private readonly int islandTimeElapsedHours;
        private readonly bool fieldPieceHasNotEmptyStorage; // can be used for checking for fruits, etc.
        private readonly List<PieceTemplate.Name> playerOwnsAnyOfThesePieces;
        private readonly List<PieceTemplate.Name> playerOwnsAllOfThesePieces;
        private readonly List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces;
        private readonly List<IslandClock.PartOfDay> partsOfDay;
        private readonly List<CountComparison> piecesCraftedCount;
        private readonly Dictionary<PieceTemplate.Name, int> usedIngredientsCount;
        private readonly Dictionary<PieceTemplate.Name, int> existingPiecesCount;
        private readonly bool fieldOnly;
        private readonly bool menuOnly;
        private readonly bool ignoreHintSetting;

        public PieceHint(Type type, List<PieceTemplate.Name> fieldPiecesNearby = null, List<PieceTemplate.Name> playerOwnsAnyOfThesePieces = null, List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces = null, List<PieceTemplate.Name> playerOwnsAllOfThesePieces = null, List<Type> alsoDisables = null, bool fieldPieceHasNotEmptyStorage = false, string message = null, List<Texture2D> imageList = null, List<HintMessage> messageList = null, List<Tutorials.Type> tutorialsToActivate = null, HintEngine.Type generalHintToActivate = HintEngine.Type.Empty, List<IslandClock.PartOfDay> partsOfDay = null, List<CountComparison> piecesCraftedCount = null, Dictionary<PieceTemplate.Name, int> usedIngredientsCount = null, Dictionary<PieceTemplate.Name, int> existingPiecesCount = null, List<HintEngine.Type> shownGeneralHints = null, List<Type> shownPieceHints = null, List<Tutorials.Type> shownTutorials = null, float distanceWalkedKilometers = 0, float mapDiscoveredPercentage = 0, int islandTimeElapsedHours = 0, bool fieldOnly = false, bool menuOnly = false, bool ignoreHintSetting = false)
        {
            this.type = type;
            this.alsoDisables = alsoDisables == null ? new List<Type> { } : alsoDisables;

            this.fieldPiecesNearby = fieldPiecesNearby;
            this.fieldPieceHasNotEmptyStorage = fieldPieceHasNotEmptyStorage;
            this.playerOwnsAnyOfThesePieces = playerOwnsAnyOfThesePieces;
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
            if (message != null) this.messageList = new List<HintMessage> { new HintMessage(text: message, imageList: imageList, blockInput: true) };
            this.tutorialsToActivate = tutorialsToActivate;
            this.generalHintToActivate = generalHintToActivate;

            if (this.generalHintToActivate != HintEngine.Type.Empty)
            {
                if (this.tutorialsToActivate != null) throw new ArgumentException("General hint and tutorial cannot both be active.");
                if (this.messageList?.Count > 0) throw new ArgumentException("General hint and message list cannot both be active.");
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

        private List<HintMessage> GetTutorials(List<Tutorials.Type> shownTutorials)
        {
            var messageList = new List<HintMessage> { };
            if (this.tutorialsToActivate == null) return messageList;

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
            if (this.generalHintToActivate != HintEngine.Type.Empty)
            {
                world.HintEngine.ShowGeneralHint(type: this.generalHintToActivate, ignoreDelay: true);
                return;
            }

            var messagesToDisplay = this.messageList.ToList();
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
            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Checking piece hints.");

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

            if (this.playerOwnsAnyOfThesePieces != null)
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

            if (this.playerOwnsAllOfThesePieces != null)
            {
                foreach (PieceTemplate.Name name in this.playerOwnsAllOfThesePieces)
                {
                    if (!CheckIfPlayerOwnsPiece(player: player, name: name)) return false;
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
                foreach (Type type in this.shownPieceHints) if (!world.HintEngine.shownPieceHints.Contains(type)) return false;
            }

            // shown tutorials
            if (this.shownTutorials != null)
            {
                foreach (Tutorials.Type type in this.shownTutorials) if (!world.HintEngine.shownTutorials.Contains(type)) return false;
            }

            // distance walked
            if (this.distanceWalkedKilometers > 0 && player.DistanceWalkedKilometers < this.distanceWalkedKilometers) return false;

            // map discovered percentage
            if (this.mapDiscoveredPercentage > 0 && world.Grid.VisitedCellsPercentage < this.mapDiscoveredPercentage) return false;

            // island time elapsed
            if (this.islandTimeElapsedHours > 0 && world.islandClock.IslandTimeElapsed.TotalHours < this.islandTimeElapsedHours) return false;

            return true;
        }

        private BoardPiece GetFirstCorrectFieldPieceNearby(Player player)
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