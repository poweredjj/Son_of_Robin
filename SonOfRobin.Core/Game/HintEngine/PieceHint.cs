using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public struct CountComparison
    {
        public enum Comparison { Greater, GreaterOrEqual, Equal, LessOrEqual, Less }

        public readonly PieceTemplate.Name name;
        private readonly int count;
        private readonly Comparison comparison;

        public CountComparison(PieceTemplate.Name name, int count, Comparison comparison = Comparison.GreaterOrEqual)
        {
            this.name = name;
            this.count = count;
            this.comparison = comparison;
        }

        public bool Check(int countCrafted)
        {
            switch (this.comparison)
            {
                case Comparison.Greater:
                    return countCrafted > this.count;

                case Comparison.GreaterOrEqual:
                    return countCrafted >= this.count;

                case Comparison.Equal:
                    return countCrafted == this.count;

                case Comparison.LessOrEqual:
                    return countCrafted <= this.count;

                case Comparison.Less:
                    return countCrafted < this.count;

                default:
                    throw new ArgumentException($"Unsupported comparison - {this.comparison}.");
            }
        }
    }

    public struct PieceHint
    {
        public enum Type { CrateStarting, CrateAnother, WoodNegative, WoodPositive, DigSiteNegative, DigSitePositive, StoneNegative, StonePositive, CrystalNegative, CrystalPositive, AnimalNegative, AnimalBow, AnimalBat, AnimalAxe, BowNoAmmo, ShellIsNotUseful, ClamField, ClamInventory, FruitTree, BananaTree, TomatoPlant, IronDepositNegative, IronDepositPositive, CoalDepositNegative, CoalDepositPositive, Cooker, LeatherPositive, BackpackPositive, BeltPositive, MapPositive, RedExclamation, Acorn, TorchNegative, TorchPositive, Fireplace, HerbsRed, HerbsYellow, HerbsViolet, HerbsCyan, HerbsBlue, HerbsBlack, GlassSand, CanBuildWorkshop, SmallBase }

        public enum Comparison { Greater, GreaterOrEqual, Equal, LessOrEqual, Less }

        public static readonly List<PieceHint> pieceHintList = new List<PieceHint>();

        private static int nearbyPiecesFrameChecked = 0;
        private static readonly List<BoardPiece> nearbyPieces = new List<BoardPiece>();


        private static List<BoardPiece> GetNearbyPieces(Player player)
        {
            if (SonOfRobinGame.currentUpdate != nearbyPiecesFrameChecked)
            {
                nearbyPieces.Clear();

                var nearbyPiecesTempList = player.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 300).OrderBy(piece => Vector2.Distance(player.sprite.position, piece.sprite.position)).ToList();

                nearbyPieces.AddRange(nearbyPiecesTempList);
                nearbyPiecesFrameChecked = SonOfRobinGame.currentUpdate;
            }

            return nearbyPieces;
        }

        public static void RefreshData()
        {
            pieceHintList.Clear();
            pieceHintList.AddRange(PieceHintData.GetData());
        }

        private readonly Type type;
        private readonly List<Type> alsoDisables;
        private readonly List<HintMessage> messageList;
        private readonly List<PieceTemplate.Name> fieldPiecesNearby;
        private readonly List<Tutorials.Type> tutorialsToActivate;
        private readonly HintEngine.Type generalHintToActivate;
        private readonly bool fieldPieceHasNotEmptyStorage;
        private readonly List<PieceTemplate.Name> playerOwnsAnyOfThesePieces;
        private readonly List<PieceTemplate.Name> playerOwnsAllOfThesePieces;
        private readonly List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces;
        private readonly List<IslandClock.PartOfDay> partsOfDay;
        private readonly List<CountComparison> piecesCraftedCount;
        private readonly Dictionary<PieceTemplate.Name, int> usedIngredientsCount;
        private readonly Dictionary<PieceTemplate.Name, int> existingPiecesCount;
        private readonly bool fieldOnly;
        private readonly bool menuOnly;

        public PieceHint(Type type, List<PieceTemplate.Name> fieldPiecesNearby = null, List<PieceTemplate.Name> playerOwnsAnyOfThesePieces = null, List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces = null, List<PieceTemplate.Name> playerOwnsAllOfThesePieces = null, List<Type> alsoDisables = null, bool fieldPieceHasNotEmptyStorage = false, string message = null, List<Texture2D> imageList = null, List<HintMessage> messageList = null, List<Tutorials.Type> tutorialsToActivate = null, HintEngine.Type generalHintToActivate = HintEngine.Type.Empty, List<IslandClock.PartOfDay> partsOfDay = null, List<CountComparison> piecesCraftedCount = null, Dictionary<PieceTemplate.Name, int> usedIngredientsCount = null, Dictionary<PieceTemplate.Name, int> existingPiecesCount = null, bool fieldOnly = false, bool menuOnly = false)
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

            this.fieldOnly = fieldOnly || this.fieldPiecesNearby != null; // fieldPiecesNearby need to be shown on the field
            this.menuOnly = menuOnly;

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
                world.hintEngine.ShowGeneralHint(type: this.generalHintToActivate, ignoreDelay: true);
                return;
            }

            var messagesToDisplay = this.messageList.ToList();
            messagesToDisplay.AddRange(this.GetTutorials(world.hintEngine.shownTutorials));

            if (this.fieldPiecesNearby != null)
            {
                foreach (BoardPiece piece in GetNearbyPieces(world.player))
                {
                    if (this.fieldPiecesNearby.Contains(piece.name))
                    {
                        Sound.QuickPlay(SoundData.Name.Notification3);
                        HintEngine.ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: messagesToDisplay);
                        break;
                    }
                }
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

            if (Scheduler.HasTaskChainInQueue) return false;

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
            if (player.world.hintEngine.shownPieceHints.Contains(this.type)) return false;
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

            if (this.fieldPiecesNearby != null)
            {
                bool fieldPieceFound = false;

                if (!fieldPieceFound)
                {
                    foreach (BoardPiece piece in GetNearbyPieces(player))
                    {
                        if (this.fieldPiecesNearby.Contains(piece.name))
                        {
                            if (!this.fieldPieceHasNotEmptyStorage || piece.pieceStorage?.OccupiedSlotsCount > 0) // for showing fruits
                            {
                                fieldPieceFound = true;
                                break;
                            }
                        }
                    }
                }

                if (!fieldPieceFound) return false;
            }

            // parts of day
            if (this.partsOfDay != null && !this.partsOfDay.Contains(player.world.islandClock.CurrentPartOfDay)) return false;

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
                    if (!pieceCraftedCount.Check(player.world.craftStats.HowMuchHasBeenCrafted(pieceCraftedCount.name))) return false;
                }
            }

            // player - has used these ingredients to craft

            if (this.usedIngredientsCount != null)
            {
                foreach (var kvp in this.usedIngredientsCount)
                {
                    PieceTemplate.Name ingredientName = kvp.Key;
                    int minimumUsedCount = kvp.Value;

                    if (player.world.craftStats.HowMuchIngredientHasBeenUsed(ingredientName) < minimumUsedCount) return false;
                }
            }

            // field pieces anywhere
            if (this.existingPiecesCount != null && !player.world.grid.SpecifiedPiecesCountIsMet(this.existingPiecesCount)) return false;

            return true;
        }

        private static bool CheckIfPlayerOwnsPiece(Player player, PieceTemplate.Name name)
        {
            var playerStorages = new List<PieceStorage> { player.toolStorage, player.pieceStorage };

            foreach (PieceStorage currentStorage in playerStorages)
            {
                BoardPiece foundPiece = currentStorage.GetFirstPieceOfName(name: name, removePiece: false);
                if (foundPiece != null) return true;
            }

            return false;
        }

    }
}
