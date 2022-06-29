using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public struct PieceHint
    {
        public enum Type { CrateStarting, CrateAnother, WoodNegative, WoodPositive, DigSiteNegative, DigSitePositive, StoneNegative, StonePositive, CrystalNegative, CrystalPositive, AnimalNegative, AnimalBow, AnimalBat, AnimalAxe, BowNoAmmo, ShellIsNotUseful, Clam, FruitTree, BananaTree, TomatoPlant, IronDepositNegative, IronDepositPositive, CoalDepositNegative, CoalDepositPositive, Cooker, LeatherPositive, BackpackPositive, BeltPositive, MapCanMake, MapPositive, RedExclamation, Acorn, TorchNegative, TorchPositive, Fireplace, HerbsRed, HerbsYellow, HerbsViolet, HerbsCyan, HerbsBlue, HerbsBlack, GlassSand, CanBuildWorkshop, EssentialWorkshopCrafted, SmallBase }

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

            var newPieceHintList = new List<PieceHint>
            {
                // hints that disable other hints, should go before those

                new PieceHint(
                    type: Type.CrateAnother, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateRegular},
                    message: "I should check what's inside this | crate.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CrateRegular) },
                    alsoDisables: new List<Type> {Type.CrateStarting},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BreakThing}),

                new PieceHint(
                    type: Type.CrateStarting, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateStarting}, canBeForced: true,
                    message: "I've seen | crates like this on the ship.\nIt could contain valuable supplies.\nI should try to break it open.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CrateStarting)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BreakThing}),

                new PieceHint(
                    type: Type.ShellIsNotUseful, canBeForced: true,
                    message: "This | shell is pretty, but I don't think it will be useful.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Shell) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Shell}),

                new PieceHint(
                    type: Type.HerbsRed, canBeForced: true,
                    message: "I think I could use these | herbs to make a healing potion.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsRed) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsRed}),

                new PieceHint(
                    type: Type.HerbsYellow, canBeForced: true,
                    message: "These | herbs could be used to make a strength potion.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsYellow) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsYellow}),

                new PieceHint(
                    type: Type.HerbsCyan, canBeForced: true,
                    message: "Hmm... These | herbs sure look interesting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsCyan)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsCyan}),

                new PieceHint(
                    type: Type.HerbsBlue, canBeForced: true,
                    message: "These | herbs could be used to make a stamina potion.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlue)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsBlue}),

                new PieceHint(
                    type: Type.HerbsViolet, canBeForced: true,
                    message: "Hmm... These | herbs should make my fatigue go away.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsViolet)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsViolet}),

                new PieceHint(
                    type: Type.HerbsBlack, canBeForced: true,
                    message: "These | herbs look poisonous.\nIt could be useful.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlack)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsBlack}),

                new PieceHint(
                    type: Type.GlassSand, canBeForced: true,
                    message: "This | is no ordinary sand!\nIt can be used to make glass.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.GlassSand)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.GlassSand}),

                new PieceHint(
                    type: Type.Clam,
                    message: "This | clam should be edible after cooking.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Clam)},
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Clam}),

                new PieceHint(
                    type: Type.Acorn, canBeForced: true,
                    message: "After cooking, this | acorn should be edible.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Acorn)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Acorn}),

                new PieceHint(
                    type: Type.RedExclamation, canBeForced: true,
                    message: "This animal is | attacking me!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Exclamation) },
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Exclamation},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.AnimalAttacking}),

                new PieceHint(
                    type: Type.LeatherPositive, canBeForced: true,
                    message: "If I had more | leather, I could make a | backpack or a | belt.",
                     imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackMedium), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(
                    type: Type.MapCanMake, canBeForced: true,
                    message: "I can use this | leather to make a | map.\nBut I need a | basic workshop to make it.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.Map),  PieceInfo.GetTexture(PieceTemplate.Name.WorkshopBasic)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(
                    type: Type.MapPositive, canBeForced: true,
                    message: "I should equip this map | to use it.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Map) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Map},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip}),

                new PieceHint(
                    type: Type.BackpackPositive, canBeForced: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "This backpack | has a lot of free space.\nI should equip it now.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BackpackMedium)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BackpackMedium) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BackpackMedium}),

                new PieceHint(
                    type: Type.BeltPositive, canBeForced: true,
                    message: "I should equip my | belt now.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BeltMedium},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip}),

                new PieceHint(
                    type: Type.BowNoAmmo, canBeForced: true,
                    message: "I need | arrows...",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ArrowStone) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowWood},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShootProjectile}),

                new PieceHint(
                    type: Type.AnimalAxe, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I could try to use my | axe to hunt this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal}),

                new PieceHint(
                    type: Type.AnimalBat,
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.SpearWood).readableName} should be great for animal hunting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SpearWood) },
                    alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearWood, PieceTemplate.Name.SpearStone}),

                new PieceHint(
                    type: Type.AnimalBow, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.BowWood).readableName} should be great for hunting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BowWood) },
                    alsoDisables: new List<Type> {Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood}),

                new PieceHint(
                    type: Type.AnimalNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I think I need some | | weapon to hunt this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SpearWood), PieceInfo.GetTexture(PieceTemplate.Name.BowWood) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood, PieceTemplate.Name.SpearStone}),

                new PieceHint(
                    type: Type.DigSitePositive,  fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.ForestDigSite },
                    message: "I could use my | shovel to dig | here.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone), PieceInfo.GetTexture(PieceTemplate.Name.BeachDigSite) },
                    alsoDisables: new List<Type> { Type.DigSiteNegative },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: Type.DigSiteNegative, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.BeachDigSite, PieceTemplate.Name.ForestDigSite, PieceTemplate.Name.DangerDigSite },
                    message: "I think that something could be buried | here.\nIf I had a | shovel, I could dig here.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BeachDigSite),  PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: Type.WoodNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could get some wood using my | bare hands,\nbut an | axe would be better.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Hand),  PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: Type.WoodPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could use my | axe to get some wood.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.GetWood},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: Type.StonePositive, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                    message: "I could use my | pickaxe to mine | stones.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall) },
                    alsoDisables: new List<Type> {Type.StoneNegative},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: Type.StoneNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                    message: "If I had a | pickaxe, I could mine stones from this | mineral.\nRegular | axe could work too, but not as effective.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall), PieceInfo.GetTexture(PieceTemplate.Name.AxeStone)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: Type.CrystalPositive, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.CrystalDepositBig },
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} should be enough to break this | crystal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeIron), PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositBig) },
                    alsoDisables: new List<Type> {Type.CrystalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.PickaxeIron }),

                new PieceHint(
                    type: Type.CrystalNegative, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.CrystalDepositBig },
                    message: $"Wow, this | crystal looks very strong.\nI think that a low level pickaxe would be too weak to break it.\nMaybe an | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} could work, though?",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositBig), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeIron) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.PickaxeIron }),

                new PieceHint(
                    type: Type.CoalDepositPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    message: "I could use my | pickaxe to mine | coal here |.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.Coal), PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit)},
                    alsoDisables: new List<Type> {Type.CoalDepositNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: Type.CoalDepositNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    message: "I think this | is a coal deposit.\nIf I had a | pickaxe, I could get | coal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.Coal)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: Type.IronDepositPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    message: "I could use my | pickaxe to mine | iron ore here |.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit)},
                    alsoDisables: new List<Type> {Type.IronDepositNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: Type.IronDepositNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    message: "I think this is an | iron deposit.\nIf I had a | pickaxe, I could mine | iron ore.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.IronOre)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: Type.FruitTree, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.AppleTree, PieceTemplate.Name.CherryTree},
                    message: "This fruit looks edible. I should shake it off this | tree.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.TreeBig) },
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: Type.BananaTree, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.BananaTree},
                    message: "A | banana! It could be possible, to shake it off | this tree.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.BananaTree) },
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: Type.TomatoPlant, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TomatoPlant},
                    message: "A | tomato... Looks tasty.", fieldPieceHasNotEmptyStorage: true,
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Tomato)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: Type.Cooker, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CookingPot},
                    message: "| I can cook now!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CookingPot)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Cook}),

                new PieceHint(
                    type: Type.TorchPositive, canBeForced: true,
                    message: "This | torch will make navigating at night a lot easier.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall) },
                    alsoDisables: new List<Type> {Type.TorchNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Torch},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TorchSmall}),

                new PieceHint(
                    type: Type.TorchNegative,
                    messageList: new List<HintMessage> {
                        new HintMessage("It's getting dark.", blockInput: true),
                        new HintMessage(text: "I need some light. A | torch, maybe?\nOr a | bonfire?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall), PieceInfo.GetTexture(PieceTemplate.Name.Campfire)  }, blockInput: true)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TorchSmall},
                    partsOfDay: new List<IslandClock.PartOfDay> {IslandClock.PartOfDay.Evening, IslandClock.PartOfDay.Night}),

                new PieceHint(
                    type: Type.Fireplace, canBeForced: true,
                    message: "This | bonfire looks ok.\nAll I need now is some | | wood...\nOr | coal?",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Campfire), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank), PieceInfo.GetTexture(PieceTemplate.Name.Coal)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Fireplace},
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Campfire}),

                new PieceHint(
                    type: Type.EssentialWorkshopCrafted, canBeForced: true,
                    alsoDisables: new List<Type> {Type.CanBuildWorkshop },
                    message: $"With this | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName} I can make useful items.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).texture },
                    piecesCraftedCount: new Dictionary<PieceTemplate.Name, int> {{ PieceTemplate.Name.WorkshopEssential, 1 }}),

                new PieceHint(
                    type: Type.CanBuildWorkshop, canBeForced: true,
                    message: $"If I had more | wood, I could build an | { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName}.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.WoodLogRegular).texture, PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BuildWorkshop},
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLogRegular }),

                new PieceHint(
                    type: Type.SmallBase, canBeForced: true,
                    generalHintToActivate: HintEngine.Type.CineSmallBase,
                    piecesCraftedCount: new Dictionary<PieceTemplate.Name, int> {{ PieceTemplate.Name.WorkshopEssential, 1 }, { PieceTemplate.Name.TentSmall, 1 }}, existingPiecesCount: new Dictionary<PieceTemplate.Name, int>{{ PieceTemplate.Name.WorkshopEssential, 1 }, { PieceTemplate.Name.TentSmall, 1 }}),
                };

            pieceHintList.AddRange(newPieceHintList);
        }

        private readonly Type type;
        private readonly List<Type> alsoDisables;
        private readonly bool canBeForced;
        private readonly List<HintMessage> messageList;
        private readonly List<PieceTemplate.Name> fieldPiecesNearby;
        private readonly List<Tutorials.Type> tutorialsToActivate;
        private readonly HintEngine.Type generalHintToActivate;
        private readonly bool fieldPieceHasNotEmptyStorage;
        private readonly List<PieceTemplate.Name> playerOwnsAnyOfThesePieces;
        private readonly List<PieceTemplate.Name> playerOwnsAllOfThesePieces;
        private readonly List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces;
        private readonly List<IslandClock.PartOfDay> partsOfDay;
        private readonly Dictionary<PieceTemplate.Name, int> piecesCraftedCount;
        private readonly Dictionary<PieceTemplate.Name, int> usedIngredientsCount;
        private readonly Dictionary<PieceTemplate.Name, int> existingPiecesCount;


        public PieceHint(Type type, List<PieceTemplate.Name> fieldPiecesNearby = null, List<PieceTemplate.Name> playerOwnsAnyOfThesePieces = null, List<PieceTemplate.Name> playerDoesNotOwnAnyOfThesePieces = null, List<PieceTemplate.Name> playerOwnsAllOfThesePieces = null, List<Type> alsoDisables = null, bool fieldPieceHasNotEmptyStorage = false, bool canBeForced = false, string message = null, List<Texture2D> imageList = null, List<HintMessage> messageList = null, List<Tutorials.Type> tutorialsToActivate = null, HintEngine.Type generalHintToActivate = HintEngine.Type.Empty, List<IslandClock.PartOfDay> partsOfDay = null, Dictionary<PieceTemplate.Name, int> piecesCraftedCount = null, Dictionary<PieceTemplate.Name, int> usedIngredientsCount = null, Dictionary<PieceTemplate.Name, int> existingPiecesCount = null)
        {
            this.type = type;
            this.alsoDisables = alsoDisables == null ? new List<Type> { } : alsoDisables;
            this.canBeForced = canBeForced;

            this.fieldPiecesNearby = fieldPiecesNearby;
            this.fieldPieceHasNotEmptyStorage = fieldPieceHasNotEmptyStorage;
            this.playerOwnsAnyOfThesePieces = playerOwnsAnyOfThesePieces;
            this.playerOwnsAllOfThesePieces = playerOwnsAllOfThesePieces;
            this.playerDoesNotOwnAnyOfThesePieces = playerDoesNotOwnAnyOfThesePieces;
            this.partsOfDay = partsOfDay;
            this.piecesCraftedCount = piecesCraftedCount;
            this.usedIngredientsCount = usedIngredientsCount;
            this.existingPiecesCount = existingPiecesCount;

            this.messageList = messageList;
            if (message != null) this.messageList = new List<HintMessage> { new HintMessage(text: message, imageList: imageList, blockInput: true) };
            this.tutorialsToActivate = tutorialsToActivate;
            this.generalHintToActivate = generalHintToActivate;

            if (this.generalHintToActivate != HintEngine.Type.Empty)
            {
                if (this.tutorialsToActivate != null) throw new ArgumentException("General hint and tutorial cannot be both active.");
                if (this.messageList?.Count > 0) throw new ArgumentException("General hint and message list cannot be both active.");
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

            foreach (PieceHint hint in pieceHintList)
            {
                if (typesToCheckOnly != null && !typesToCheckOnly.Contains(hint.type)) continue;

                if (hint.CheckIfConditionsAreMet(player: player))
                {
                    if (!forcedMode || hint.canBeForced)
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
            }

            return false;
        }

        private bool CheckIfConditionsAreMet(Player player)
        {
            if (player.world.hintEngine.shownPieceHints.Contains(this.type)) return false;

            // field pieces nearby

            if (this.fieldPiecesNearby != null)
            {
                bool fieldPieceFound = false;

                foreach (BoardPiece piece in GetNearbyPieces(player))
                {
                    if (this.fieldPiecesNearby.Contains(piece.name))
                    {
                        if (!this.fieldPieceHasNotEmptyStorage || piece.pieceStorage?.OccupiedSlotsCount > 0)
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

            // player - has crafted these pieces

            if (this.piecesCraftedCount != null)
            {
                foreach (var kvp in this.piecesCraftedCount)
                {
                    PieceTemplate.Name pieceName = kvp.Key;
                    int minimumCraftedCount = kvp.Value;

                    if (player.world.craftStats.HowMuchHasBeenCrafted(pieceName) < minimumCraftedCount) return false;
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
