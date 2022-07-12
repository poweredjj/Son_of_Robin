using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceHintData
    {
        public static List<PieceHint> GetData()
        {
            var newPieceHintList = new List<PieceHint>
            {
                // hints that disable other hints, should go before those

                new PieceHint(
                    type: PieceHint.Type.CrateAnother, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateRegular},
                    message: "I should check what's inside this | crate.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CrateRegular) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.CrateStarting},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BreakThing}),

                new PieceHint(
                    type: PieceHint.Type.CrateStarting, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CrateStarting},
                    message: "I've seen | crates like this on the ship.\nIt could contain valuable supplies.\nI should try to break it open.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CrateStarting)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BreakThing}),

                new PieceHint(
                    type: PieceHint.Type.ShellIsNotUseful,
                    message: "This | shell is pretty, but I don't think it will be useful.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Shell) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Shell}),

                new PieceHint(
                    type: PieceHint.Type.HerbsRed,
                    message: "I think I could use these | herbs to make a healing potion.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsRed) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsRed}),

                new PieceHint(
                    type: PieceHint.Type.HerbsYellow,
                    message: "These | herbs could be used to make a strength potion.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsYellow) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsYellow}),

                new PieceHint(
                    type: PieceHint.Type.HerbsCyan,
                    message: "Hmm... These | herbs sure look interesting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsCyan)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsCyan}),

                new PieceHint(
                    type: PieceHint.Type.HerbsBlue,
                    message: "These | herbs could be used to make a stamina potion.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlue)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsBlue}),

                new PieceHint(
                    type: PieceHint.Type.HerbsViolet,
                    message: "Hmm... These | herbs should make my fatigue go away.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsViolet)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsViolet}),

                new PieceHint(
                    type: PieceHint.Type.HerbsBlack,
                    message: "These | herbs look poisonous.\nIt could be useful.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlack)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.HerbsBlack}),

                new PieceHint(
                    type: PieceHint.Type.GlassSand,
                    message: "This | is no ordinary sand!\nIt can be used to make glass.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.GlassSand)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.GlassSand}),

                new PieceHint(
                    type: PieceHint.Type.ClamInventory,
                    alsoDisables: new List<PieceHint.Type> { PieceHint.Type.ClamField },
                    message: "This | is no ordinary sand!\nIt can be used to make glass.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.GlassSand)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.GlassSand}),

                new PieceHint(
                    type: PieceHint.Type.ClamField,
                    message: "This | clam should be edible after cooking.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Clam)},
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Clam}),

                new PieceHint(
                    type: PieceHint.Type.Acorn,
                    message: "After cooking, this | acorn should be edible.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Acorn)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Acorn}),

                new PieceHint(
                    type: PieceHint.Type.RedExclamation,
                    message: "This animal is | attacking me!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Exclamation) },
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Exclamation},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.AnimalAttacking}),

                new PieceHint(
                    type: PieceHint.Type.LeatherPositive,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: $"I can use this | leather to make a | map.\nBut I need a | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopBasic).readableName} to make it.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.Map),  PieceInfo.GetTexture(PieceTemplate.Name.WorkshopBasic)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "If I had more | leather,\nI could make a | backpack or a | belt.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall), PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(
                    type: PieceHint.Type.MapPositive,
                    message: "I should equip this map | to use it.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Map) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Map},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip}),

                new PieceHint(
                    type: PieceHint.Type.BackpackPositive,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "This backpack | has a lot of free space.\nI should equip it now.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BackpackSmall}),

                new PieceHint(
                    type: PieceHint.Type.BeltPositive,
                    message: "I should equip my | belt now.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BeltMedium)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BeltMedium},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Equip}),

                new PieceHint(
                    type: PieceHint.Type.BowNoAmmo,
                    message: "I need | arrows...",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ArrowStone) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowWood},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShootProjectile}),

                new PieceHint(
                    type: PieceHint.Type.AnimalAxe, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I could try to use my | axe to hunt this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal}),

                new PieceHint(
                    type: PieceHint.Type.AnimalBat,
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.SpearWood).readableName} should be great for animal hunting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SpearWood) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearWood, PieceTemplate.Name.SpearStone}),

                new PieceHint(
                    type: PieceHint.Type.AnimalBow, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.BowWood).readableName} should be great for hunting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BowWood) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood}),

                new PieceHint(
                    type: PieceHint.Type.AnimalNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I think I need some | | weapon to hunt this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SpearWood), PieceInfo.GetTexture(PieceTemplate.Name.BowWood) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowWood, PieceTemplate.Name.SpearStone}),

                new PieceHint(
                    type: PieceHint.Type.DigSitePositive,  fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.ForestDigSite },
                    message: "I could use my | shovel to dig | here.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone), PieceInfo.GetTexture(PieceTemplate.Name.BeachDigSite) },
                    alsoDisables: new List<PieceHint.Type> { PieceHint.Type.DigSiteNegative },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: PieceHint.Type.DigSiteNegative, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.BeachDigSite, PieceTemplate.Name.ForestDigSite, PieceTemplate.Name.DangerDigSite },
                    message: "I think that something could be buried | here.\nIf I had a | shovel, I could dig here.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BeachDigSite),  PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: PieceHint.Type.WoodNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could get some wood using my | bare hands,\nbut an | axe would be better.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Hand),  PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.WoodPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could use my | axe to get some wood.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.GetWood},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.StonePositive, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                    message: "I could use my | pickaxe to mine | stones.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.StoneNegative},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.StoneNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.WaterRock},
                    message: "If I had a | pickaxe, I could mine stones from this | mineral.\nRegular | axe could work too, but not as effective.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall), PieceInfo.GetTexture(PieceTemplate.Name.AxeStone)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.CrystalPositive, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.CrystalDepositBig },
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} should be enough to break this | crystal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeIron), PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositBig) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.CrystalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.PickaxeIron }),

                new PieceHint(
                    type: PieceHint.Type.CrystalNegative, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.CrystalDepositBig },
                    message: $"Wow, this | crystal looks very strong.\nI think that a low level pickaxe would be too weak to break it.\nMaybe an | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} could work, though?",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositBig), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeIron) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.PickaxeIron }),

                new PieceHint(
                    type: PieceHint.Type.CoalDepositPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    message: "I could use my | pickaxe to mine | coal here |.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.Coal), PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit)},
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.CoalDepositNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.CoalDepositNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    message: "I think this | is a coal deposit.\nIf I had a | pickaxe, I could get | coal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.Coal)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.IronDepositPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    message: "I could use my | pickaxe to mine | iron ore here |.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit)},
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.IronDepositNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.IronDepositNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    message: "I think this is an | iron deposit.\nIf I had a | pickaxe, I could mine | iron ore.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.IronOre)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.FruitTree, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.AppleTree, PieceTemplate.Name.CherryTree},
                    message: "This fruit looks edible. I should shake it off this | tree.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.TreeBig) },
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.BananaTree, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.BananaTree},
                    message: "A | banana! It could be possible, to shake it off | this tree.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.BananaTree) },
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.TomatoPlant, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TomatoPlant},
                    message: "A | tomato... Looks tasty.", fieldPieceHasNotEmptyStorage: true,
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Tomato)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.Cooker, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CookingPot},
                    message: "| I can cook now!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CookingPot)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Cook}),

                new PieceHint(
                    type: PieceHint.Type.TorchPositive,
                    message: "This | torch will make navigating at night a lot easier.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.TorchNegative },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Torch},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TorchSmall}),

                new PieceHint(
                    type: PieceHint.Type.TorchNegative,
                    messageList: new List<HintMessage> {
                        new HintMessage("It's getting dark.", blockInput: true),
                        new HintMessage(text: "I need some light. A | torch, maybe?\nOr a | bonfire?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall), PieceInfo.GetTexture(PieceTemplate.Name.Campfire)  }, blockInput: true)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TorchSmall},
                    partsOfDay: new List<IslandClock.PartOfDay> {IslandClock.PartOfDay.Evening, IslandClock.PartOfDay.Night}),

                new PieceHint(
                    type: PieceHint.Type.Fireplace,
                    message: "This | bonfire looks ok.\nAll I need now is some | | wood...\nOr | coal?",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Campfire), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank), PieceInfo.GetTexture(PieceTemplate.Name.Coal)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Fireplace},
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Campfire}),

                new PieceHint(
                    type: PieceHint.Type.EssentialWorkshopCrafted,
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.CanBuildWorkshop },
                    message: $"With this | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName} I can make useful items.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).texture },
                    piecesCraftedCount: new Dictionary<PieceTemplate.Name, int> {{ PieceTemplate.Name.WorkshopEssential, 1 }}),

                new PieceHint(
                    type: PieceHint.Type.CanBuildWorkshop,
                    message: $"If I had more | wood, I could build an | { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName}.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.WoodLogRegular).texture, PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.BuildWorkshop},
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLogRegular }),

                new PieceHint(
                    type: PieceHint.Type.SmallBase,
                    generalHintToActivate: HintEngine.Type.CineSmallBase,
                    piecesCraftedCount: new Dictionary<PieceTemplate.Name, int> {{ PieceTemplate.Name.WorkshopEssential, 1 }, { PieceTemplate.Name.TentSmall, 1 }}, existingPiecesCount: new Dictionary<PieceTemplate.Name, int>{{ PieceTemplate.Name.WorkshopEssential, 1 }, { PieceTemplate.Name.TentSmall, 1 } }, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.TentSmall, PieceTemplate.Name.WorkshopEssential }),
                };

            return newPieceHintList;
        }

    }
}
