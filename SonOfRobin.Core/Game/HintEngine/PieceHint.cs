using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public struct PieceHint
    {
        public enum Name { CrateStarting, CrateAnother, WoodNegative, WoodPositive, StoneNegative, StonePositive, AnimalNegative, AnimalSling, AnimalBow, AnimalBat, AnimalAxe, SlingNoAmmo, BowNoAmmo, ShellIsNotUseful, FruitTree, BananaTree, TomatoPlant, IronDepositNegative, IronDepositPositive, CoalDepositNegative, CoalDepositPositive, Cooker, LeatherPositive, BackpackPositive, BeltPositive, MapCanMake, MapPositive }

        public static readonly List<PieceHint> pieceHintList = new List<PieceHint>
        {
                new PieceHint(name: Name.CrateStarting, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateStarting},
                message: "I've seen crates like this on the ship.\nIt could contain valuable supplies.\nI should try to break it open."),

                new PieceHint(name: Name.CrateAnother, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateRegular},
                message: "I should check what's inside this crate.", alsoDisables: new List<Name> {Name.CrateStarting}),

                new PieceHint(name: Name.ShellIsNotUseful, canBeForced: true,
                message: "This shell is pretty, but I don't think it will be useful.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Shell}),

                new PieceHint(name: Name.LeatherPositive, canBeForced: true,
                message: "If I had more leather, I could make a backpack or a belt.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(name: Name.MapCanMake, canBeForced: true,
                message: "I can use this leather to make a map.\nBut I need a workshop to make it.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(name: Name.MapPositive, canBeForced: true,
                message: "I should equip this map to use it.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Map}),

                new PieceHint(name: Name.BackpackPositive, canBeForced: true,
                message: "This backpack has a lot of free space.\nI should equip it now.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BackpackMedium}),

                new PieceHint(name: Name.BeltPositive, canBeForced: true,
                message: "I should equip my belt now.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BeltMedium}),

                new PieceHint(name: Name.SlingNoAmmo, canBeForced: true,
                message: "My sling is useless without stones...",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Sling, PieceTemplate.Name.GreatSling},
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.StoneAmmo}),

                new PieceHint(name: Name.BowNoAmmo, canBeForced: true,
                message: "I need arrows...",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood},
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowWood}),

                new PieceHint(name: Name.AnimalNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                message: "I think I need some weapon to hunt this animal.",
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood, PieceTemplate.Name.BatWood, PieceTemplate.Name.Sling, PieceTemplate.Name.GreatSling}),

                new PieceHint(name: Name.AnimalAxe, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                message: "I could try to use my axe to hunt this animal, but I don't think it will be very effective.", alsoDisables: new List<Name> {Name.AnimalNegative},
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron}),

                new PieceHint(name: Name.AnimalBat, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                message: "It should be possible to hunt animals with my wooden bat.", alsoDisables: new List<Name> {Name.AnimalNegative},
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BatWood}),

                new PieceHint(name: Name.AnimalSling, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                message: "My sling should be enough to hunt an animal. Right?", alsoDisables: new List<Name> {Name.AnimalNegative},
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Sling, PieceTemplate.Name.GreatSling}),

                new PieceHint(name: Name.AnimalBow, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                message: "My bow should be great for hunting.", alsoDisables: new List<Name> {Name.AnimalNegative},
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood}),

                new PieceHint(name: Name.WoodNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                message: "I could get some wood using my bare hands, but an axe would be better.",
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron }),

                new PieceHint(name: Name.WoodPositive,  fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                message: "I could use my axe to get some wood.",
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron }),

                new PieceHint(name: Name.StoneNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                message: "If I had a pickaxe, I could mine stones from this mineral.",
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(name: Name.StonePositive, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                message: "I could use my pickaxe to mine stones.", alsoDisables: new List<Name> {Name.StoneNegative},
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(name: Name.CoalDepositNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                message: "I think this is a coal deposit. If I had a pickaxe, I could get coal.",
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(name: Name.CoalDepositPositive,  fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                message: "I could use my pickaxe to mine coal here.", alsoDisables: new List<Name> {Name.CoalDepositNegative },
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(name: Name.IronDepositNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                message: "I think this is an iron deposit. If I had a pickaxe, I could mine iron ore.",
                playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(name: Name.IronDepositPositive,  fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                message: "I could use my pickaxe to mine iron ore here.", alsoDisables: new List<Name> {Name.IronDepositNegative },
                playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(name: Name.FruitTree, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AppleTree, PieceTemplate.Name.CherryTree},
                message: "This fruit looks edible. I should shake it off this tree.", fieldPieceHasNotEmptyStorage: true),

                new PieceHint(name: Name.BananaTree, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BananaTree},
                message: "A banana! It could be possible, to shake it off this tree.", fieldPieceHasNotEmptyStorage: true),

                new PieceHint(name: Name.TomatoPlant, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TomatoPlant},
                message: "A tomato... Looks tasty.", fieldPieceHasNotEmptyStorage: true),

                new PieceHint(name: Name.Cooker, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CookingPot},
                message: "To cook something in here, I will need ingredients and wood."),
        };

        private readonly Name name;
        private readonly List<Name> alsoDisables;
        private readonly bool canBeForced;
        private readonly string message;
        private readonly List<PieceTemplate.Name> fieldPieces;
        private readonly bool fieldPieceHasNotEmptyStorage;
        private readonly List<PieceTemplate.Name> playerOwnsAnyOfThesePieces;
        private readonly List<PieceTemplate.Name> playerOwnsAllOfThesePieces;
        private readonly List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces;

        public PieceHint(Name name, string message, List<PieceTemplate.Name> fieldPieces = null, List<PieceTemplate.Name> playerOwnsAnyOfThesePieces = null, List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces = null, List<PieceTemplate.Name> playerOwnsAllOfThesePieces = null, List<Name> alsoDisables = null, bool fieldPieceHasNotEmptyStorage = false, bool canBeForced = false)
        {
            this.name = name;
            this.alsoDisables = alsoDisables == null ? new List<Name> { } : alsoDisables;
            this.canBeForced = canBeForced;
            this.fieldPieces = fieldPieces;
            this.fieldPieceHasNotEmptyStorage = fieldPieceHasNotEmptyStorage;
            this.playerOwnsAnyOfThesePieces = playerOwnsAnyOfThesePieces;
            this.playerOwnsAllOfThesePieces = playerOwnsAllOfThesePieces;
            this.playerDoesNotOwnAnyOfThesePieces = playerDoesNotOwnAnyOfThesePieces;
            this.message = message;
        }

        private void Show(Player player, List<BoardPiece> fieldPiecesNearby)
        {
            if (this.fieldPieces != null)
            {
                foreach (BoardPiece piece in fieldPiecesNearby)
                {
                    if (this.fieldPieces.Contains(piece.name))
                    {
                        HintEngine.ShowPieceDuringPause(world: player.world, pieceToShow: piece, message: this.message);
                        break;
                    }
                }
            }
            else
            { HintEngine.ShowMessageDuringPause(this.message); }
        }

        public static bool CheckForHintToShow(Player player, List<PieceHint.Name> shownPieceHints, bool forcedMode = false, bool ignoreInputActive = false)
        {
            if (!player.world.inputActive && !ignoreInputActive) return false;

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Checking piece hints.");

            var fieldPiecesNearby = player.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 200);
            fieldPiecesNearby = fieldPiecesNearby.OrderBy(piece => Vector2.Distance(player.sprite.position, piece.sprite.position)).ToList();

            foreach (PieceHint hint in pieceHintList)
            {
                if (!shownPieceHints.Contains(hint.name) && hint.CheckIfConditionsAreMet(player: player, fieldPiecesNearby: fieldPiecesNearby))
                {
                    if (!forcedMode || hint.canBeForced)
                    {
                        hint.Show(player: player, fieldPiecesNearby: fieldPiecesNearby);
                        shownPieceHints.Add(hint.name);
                        foreach (Name name in hint.alsoDisables)
                        { shownPieceHints.Add(name); }

                        return true; // only one hint should be shown at once
                    }
                }
            }

            return false;
        }

        private bool CheckIfConditionsAreMet(Player player, List<BoardPiece> fieldPiecesNearby)
        {
            // field pieces

            if (this.fieldPieces != null)
            {
                bool fieldPieceFound = false;

                foreach (BoardPiece piece in fieldPiecesNearby)
                {
                    if (this.fieldPieces.Contains(piece.name))
                    {
                        if (!this.fieldPieceHasNotEmptyStorage || piece.pieceStorage?.NotEmptySlotsCount > 0)
                        {
                            fieldPieceFound = true;
                            break;
                        }
                    }
                }

                if (!fieldPieceFound) return false;
            }

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
