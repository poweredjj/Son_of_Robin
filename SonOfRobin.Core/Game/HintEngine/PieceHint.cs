using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{


    public struct PieceHint
    {
        public enum Type { CrateStarting, CrateAnother, WoodNegative, WoodPositive, StoneNegative, StonePositive, AnimalNegative, AnimalSling, AnimalBow, AnimalBat, AnimalAxe, SlingNoAmmo, BowNoAmmo, ShellIsNotUseful, FruitTree, BananaTree, TomatoPlant, IronDepositNegative, IronDepositPositive, CoalDepositNegative, CoalDepositPositive, Cooker, LeatherPositive, BackpackPositive, BeltPositive, MapCanMake, MapPositive, RedExclamation, Acorn, TorchNegative, TorchPositive }

        public static readonly List<PieceHint> pieceHintList = new List<PieceHint>
        {
                new PieceHint(type: Type.CrateStarting, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateStarting}, canBeForced: true,
                    message: "I've seen crates like this on the ship.\nIt could contain valuable supplies.\nI should try to break it open.",
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BreakThing}),

                new PieceHint(type: Type.CrateAnother, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateRegular},
                    message: "I should check what's inside this crate.",
                    alsoDisables: new List<Type> {Type.CrateStarting},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BreakThing}),

                new PieceHint(type: Type.ShellIsNotUseful, canBeForced: true,
                    message: "This shell is pretty, but I don't think it will be useful.",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Shell}),

                new PieceHint(type: Type.Acorn, canBeForced: true,
                    message: "After cooking, this acorn should be edible.",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Acorn}),

                new PieceHint(type: Type.RedExclamation, canBeForced: true,
                    message: "This animal is attacking me!",
                    fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Exclamation},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.AnimalAttacking}),

                new PieceHint(type: Type.LeatherPositive, canBeForced: true,
                    message: "If I had more leather, I could make a backpack or a belt.",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(type: Type.MapCanMake, canBeForced: true,
                    message: "I can use this leather to make a map.\nBut I need a workshop to make it.",
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BuildWorkshop},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(type: Type.MapPositive, canBeForced: true,
                    message: "I should equip this map to use it.",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Map},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip}),

                new PieceHint(type: Type.BackpackPositive, canBeForced: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "This backpack has a lot of free space.\nI should equip it now.", boxType: HintMessage.BoxType.Dialogue)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BackpackMedium}),

                new PieceHint(type: Type.BeltPositive, canBeForced: true,
                    message: "I should equip my belt now.",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BeltMedium},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip}),

                new PieceHint(type: Type.SlingNoAmmo, canBeForced: true,
                    message: "My sling is useless without stones...",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Sling, PieceTemplate.Name.GreatSling},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.StoneAmmo},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShootProjectile}),

                new PieceHint(type: Type.BowNoAmmo, canBeForced: true,
                    message: "I need arrows...",
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowWood},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShootProjectile}),

                new PieceHint(type: Type.AnimalNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I think I need some weapon to hunt this animal.",
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood, PieceTemplate.Name.BatWood, PieceTemplate.Name.Sling,
                        PieceTemplate.Name.GreatSling}),

                new PieceHint(type: Type.AnimalAxe, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I could try to use my axe to hunt this animal.",
                    alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron}),

                new PieceHint(type: Type.AnimalBat,
                    fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "My wooden bat should be great for animal hunting.",
                    alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BatWood}),

                new PieceHint(type: Type.AnimalSling, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "My sling should be enough to hunt an animal. Right?", alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Sling, PieceTemplate.Name.GreatSling}),

                new PieceHint(type: Type.AnimalBow, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "My bow should be great for hunting.",
                    alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood}),

                new PieceHint(type: Type.WoodNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could get some wood using my bare hands, but an axe would be better.",
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron }),

                new PieceHint(type: Type.WoodPositive,  fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could use my axe to get some wood.",
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.GetWood},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron }),

                new PieceHint(type: Type.StoneNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                    message: "If I had a pickaxe, I could mine stones from this mineral.",
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(type: Type.StonePositive, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                    message: "I could use my pickaxe to mine stones.",
                    alsoDisables: new List<Type> {Type.StoneNegative},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(type: Type.CoalDepositNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    message: "I think this is a coal deposit. If I had a pickaxe, I could get coal.",
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(type: Type.CoalDepositPositive,  fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    message: "I could use my pickaxe to mine coal here.",
                    alsoDisables: new List<Type> {Type.CoalDepositNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(type: Type.IronDepositNegative, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    message: "I think this is an iron deposit. If I had a pickaxe, I could mine iron ore.",
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(type: Type.IronDepositPositive,  fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    message: "I could use my pickaxe to mine iron ore here.",
                    alsoDisables: new List<Type> {Type.IronDepositNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron }),

                new PieceHint(type: Type.FruitTree, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AppleTree, PieceTemplate.Name.CherryTree},
                    message: "This fruit looks edible. I should shake it off this tree.", fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(type: Type.BananaTree, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BananaTree},
                    message: "A banana! It could be possible, to shake it off this tree.", fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(type: Type.TomatoPlant, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TomatoPlant},
                    message: "A tomato... Looks tasty.", fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(type: Type.Cooker, fieldPieces: new List<PieceTemplate.Name> {PieceTemplate.Name.CookingPot},
                    message: "I can cook now!",
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Cook}),

                new PieceHint(type: Type.TorchNegative,
                    messageList: new List<HintMessage> {new HintMessage("It's getting dark."), new HintMessage("I need some light. A torch, maybe?")},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Torch},
                    partsOfDay: new List<IslandClock.PartOfDay> {IslandClock.PartOfDay.Evening, IslandClock.PartOfDay.Night}),

                new PieceHint(type: Type.TorchPositive, canBeForced: true,
                    message: "This torch will make navigating at night a lot easier.",
                    alsoDisables: new List<Type> {Type.TorchNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Torch},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Torch}),
        };

        private readonly Type type;
        private readonly List<Type> alsoDisables;
        private readonly bool canBeForced;
        private readonly List<HintMessage> messageList;
        private readonly List<PieceTemplate.Name> fieldPieces;
        private readonly List<Tutorials.Type> tutorialsToActivate;
        private readonly bool fieldPieceHasNotEmptyStorage;
        private readonly List<PieceTemplate.Name> playerOwnsAnyOfThesePieces;
        private readonly List<PieceTemplate.Name> playerOwnsAllOfThesePieces;
        private readonly List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces;
        private readonly List<IslandClock.PartOfDay> partsOfDay;

        public PieceHint(Type type, List<PieceTemplate.Name> fieldPieces = null, List<PieceTemplate.Name> playerOwnsAnyOfThesePieces = null, List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces = null, List<PieceTemplate.Name> playerOwnsAllOfThesePieces = null, List<Type> alsoDisables = null, bool fieldPieceHasNotEmptyStorage = false, bool canBeForced = false, string message = null, List<HintMessage> messageList = null, List<Tutorials.Type> tutorialsToActivate = null, List<IslandClock.PartOfDay> partsOfDay = null)
        {
            this.type = type;
            this.alsoDisables = alsoDisables == null ? new List<Type> { } : alsoDisables;
            this.canBeForced = canBeForced;
            this.fieldPieces = fieldPieces;
            this.fieldPieceHasNotEmptyStorage = fieldPieceHasNotEmptyStorage;
            this.playerOwnsAnyOfThesePieces = playerOwnsAnyOfThesePieces;
            this.playerOwnsAllOfThesePieces = playerOwnsAllOfThesePieces;
            this.playerDoesNotOwnAnyOfThesePieces = playerDoesNotOwnAnyOfThesePieces;
            this.partsOfDay = partsOfDay;
            this.messageList = messageList;
            if (message != null) this.messageList = new List<HintMessage> { new HintMessage(text: message) };
            this.tutorialsToActivate = tutorialsToActivate;
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

        private void Show(World world, List<BoardPiece> fieldPiecesNearby)
        {
            var messagesToDisplay = this.messageList.ToList();
            messagesToDisplay.AddRange(this.GetTutorials(world.hintEngine.shownTutorials));

            if (this.fieldPieces != null)
            {
                foreach (BoardPiece piece in fieldPiecesNearby)
                {
                    if (this.fieldPieces.Contains(piece.name))
                    {
                        HintEngine.ShowPieceDuringPause(world: world, pieceToShow: piece, messageList: messagesToDisplay);
                        break;
                    }
                }
            }
            else HintEngine.ShowMessageDuringPause(messagesToDisplay);
        }

        public static bool CheckForHintToShow(HintEngine hintEngine, Player player, bool forcedMode = false, bool ignoreInputActive = false, List<Type> typesToCheckOnly = null)
        {
            if (!player.world.inputActive && !ignoreInputActive) return false;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Checking piece hints.");


            var fieldPiecesNearby = player.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 300);
            fieldPiecesNearby = fieldPiecesNearby.OrderBy(piece => Vector2.Distance(player.sprite.position, piece.sprite.position)).ToList();

            foreach (PieceHint hint in pieceHintList)
            {
                if (typesToCheckOnly != null && !typesToCheckOnly.Contains(hint.type)) continue;

                if (!hintEngine.shownPieceHints.Contains(hint.type) && hint.CheckIfConditionsAreMet(player: player, fieldPiecesNearby: fieldPiecesNearby))
                {
                    if (!forcedMode || hint.canBeForced)
                    {
                        hint.Show(world: player.world, fieldPiecesNearby: fieldPiecesNearby);
                        hintEngine.Disable(hint.type);
                        foreach (Type type in hint.alsoDisables)
                        { hintEngine.Disable(type); }

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
