using Microsoft.Xna.Framework.Graphics;
using System;
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
                    type: PieceHint.Type.CoffeeRaw,
                    message: $"This is | {PieceInfo.GetInfo(PieceTemplate.Name.CoffeeRaw).readableName}!\nI could roast it in a | {PieceInfo.GetInfo(PieceTemplate.Name.Furnace).readableName}.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw), PieceInfo.GetTexture(PieceTemplate.Name.Furnace) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.CoffeeRaw }),

                new PieceHint(
                    type: PieceHint.Type.HerbsBlack,
                    message: "These | herbs look poisonous.\nThey could be useful.",
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
                    message: $"I cannot eat this | { PieceInfo.GetInfo(PieceTemplate.Name.Clam).readableName } raw.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Clam)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Clam}),

                new PieceHint(
                    type: PieceHint.Type.ClamField,
                    message: $"This | { PieceInfo.GetInfo(PieceTemplate.Name.Clam).readableName } should be edible after cooking.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Clam)},
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Clam}),

                new PieceHint(
                    type: PieceHint.Type.Acorn,
                    message: $"After cooking, this | { PieceInfo.GetInfo(PieceTemplate.Name.Acorn).readableName } should be edible.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Acorn)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Acorn}),

                new PieceHint(
                    type: PieceHint.Type.RedExclamation,
                    message: "This animal is | attacking me!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BubbleExclamationRed) },
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.BubbleExclamationRed},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.AnimalAttacking}),

                new PieceHint(
                    type: PieceHint.Type.LeatherPositive,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: $"If I had more | { PieceInfo.GetInfo(PieceTemplate.Name.Leather).readableName },\nI could make a | backpack or a | belt.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall), PieceInfo.GetTexture(PieceTemplate.Name.BeltSmall)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(
                    type: PieceHint.Type.MapPositive,
                    message: $"I should equip this { PieceInfo.GetInfo(PieceTemplate.Name.Map).readableName } | to use it.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Map) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.Map },
                    playerEquipmentDoesNotContainThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.Map },
                    tutorialsToActivate: new List<Tutorials.Type> { Tutorials.Type.Equip }),

                new PieceHint(
                    type: PieceHint.Type.BackpackPositive,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "This backpack | will allow me to carry more items.\nI should equip it now.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BackpackSmall },
                    playerEquipmentDoesNotContainThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BackpackSmall },
                    tutorialsToActivate: new List<Tutorials.Type> { Tutorials.Type.Equip }
                    ),

                new PieceHint(
                    type: PieceHint.Type.BeltPositive,
                    message: "I should equip my | belt now.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BeltSmall) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BeltSmall },
                    playerEquipmentDoesNotContainThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BeltSmall },
                    tutorialsToActivate: new List<Tutorials.Type> { Tutorials.Type.Equip }),

                new PieceHint(
                    type: PieceHint.Type.BowNoAmmo,
                    message: "I need | arrows...",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ArrowStone) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BowBasic, PieceTemplate.Name.BowAdvanced },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowWood, PieceTemplate.Name.ArrowStone},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShootProjectile}),

                new PieceHint(
                    type: PieceHint.Type.AnimalAxe, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I could try to use my | axe to hunt this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal}),

                new PieceHint(
                    type: PieceHint.Type.AnimalSpear,
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.SpearWood).readableName} should be great for animal hunting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SpearWood) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearWood, PieceTemplate.Name.SpearStone, PieceTemplate.Name.SpearIron, PieceTemplate.Name.SpearCrystal}),

                new PieceHint(
                    type: PieceHint.Type.AnimalBow, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.BowBasic).readableName} should be great for hunting.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BowBasic) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BowBasic, PieceTemplate.Name.BowAdvanced }),

                new PieceHint(
                    type: PieceHint.Type.AnimalNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    message: "I think I need some | | weapon to hunt this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SpearWood), PieceInfo.GetTexture(PieceTemplate.Name.BowBasic) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.BowBasic, PieceTemplate.Name.SpearStone, PieceTemplate.Name.BowAdvanced }),

                new PieceHint(
                    type: PieceHint.Type.DigSitePositive,  fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.BeachDigSite, PieceTemplate.Name.ForestDigSite, PieceTemplate.Name.SwampDigSite, PieceTemplate.Name.GlassDigSite },
                    message: "I could use my | shovel to dig | here.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone), PieceInfo.GetTexture(PieceTemplate.Name.ForestDigSite) },
                    alsoDisables: new List<PieceHint.Type> { PieceHint.Type.DigSiteNegative },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: PieceHint.Type.DigSiteNegative, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.BeachDigSite, PieceTemplate.Name.ForestDigSite, PieceTemplate.Name.SwampDigSite, PieceTemplate.Name.GlassDigSite },
                    message: "I think that something could be buried | here.\nIf I had a | shovel, I could dig there.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BeachDigSite),  PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: PieceHint.Type.DigSiteGlass, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.GlassDigSite },
                    message: "This spot | looks interesting. I might find something useful there.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.GlassDigSite) }),

                new PieceHint(
                    type: PieceHint.Type.WoodNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could get some wood using my | bare hands,\nbut an | axe would be better.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.KnifeSimple),  PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.WoodPositive,  fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    message: "I could use my | axe to get some wood.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.GetWood},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.StonePositive, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.MineralsMossyBig, PieceTemplate.Name.MineralsMossySmall},
                    message: "I could use my | pickaxe to mine | stones.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall) },
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.StoneNegative},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.StoneNegative, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.MineralsMossyBig, PieceTemplate.Name.MineralsMossySmall},
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
                    message: $"Wow, this | crystal looks very strong.\nI think that a pickaxe made of wood or stone\nwould be too weak to break it.\nMaybe an | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} could work, though?",
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
                    type: PieceHint.Type.CarrotPlant, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CarrotPlant},
                    message: "A | carrot... Looks tasty.", fieldPieceHasNotEmptyStorage: true,
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Carrot)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.HotPlate, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.HotPlate},
                    message: "| I can cook simple meals now!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.HotPlate) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Cook}),

                new PieceHint(
                    type: PieceHint.Type.Cooker, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CookingPot},
                    message: "| Now I can cook like a pro!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CookingPot)},
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.HotPlate },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Cook}),

                new PieceHint(
                    type: PieceHint.Type.HarvestingWorkshop, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.WorkshopMeatHarvesting},
                    message: "| Now I can harvest meat from | animals!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMeatHarvesting), TextureBank.GetTexture(textureName: TextureBank.TextureName.Animal) },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.HarvestMeat}),

                new PieceHint(
                    type: PieceHint.Type.Totem, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.Totem },
                    message: $"Hmm...\nI can feel mysterious aura surrounding this | { PieceInfo.GetInfo(PieceTemplate.Name.Totem).readableName }.\nMaybe I should examine it.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Totem) }),

                new PieceHint(
                    type: PieceHint.Type.AlchemyLab, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.AlchemyLabStandard },
                    message: "| I can start brewing\n| potions now!",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AlchemyLabStandard), AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture },
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.PotionBrew}),

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
                        new HintMessage(text: "I need some light. A | torch, maybe?\nOr a | bonfire?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall), PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall)  }, blockInput: true)},
                    playerDoesNotOwnAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.TorchSmall},
                    partsOfDay: new List<IslandClock.PartOfDay> {IslandClock.PartOfDay.Evening, IslandClock.PartOfDay.Night}),

                new PieceHint(
                    type: PieceHint.Type.Fireplace,
                    message: "This | bonfire looks ok.\nAll I need now is some | | wood...\nOr | coal?",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank), PieceInfo.GetTexture(PieceTemplate.Name.Coal)},
                    tutorialsToActivate: new List<Tutorials.Type> {Tutorials.Type.Fireplace},
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.CampfireSmall}),

                new PieceHint(
                    type: PieceHint.Type.SharedChest,
                    message: $"This | { PieceInfo.GetInfo(PieceTemplate.Name.ChestCrystal).readableName } defies the laws of physics, somehow.\nEvery other | { PieceInfo.GetInfo(PieceTemplate.Name.ChestCrystal).readableName } will share the items that are stored inside.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ChestCrystal), PieceInfo.GetTexture(PieceTemplate.Name.ChestCrystal) },
                    fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.ChestCrystal }),

                new PieceHint(
                    type: PieceHint.Type.CanDestroyEssentialWorkshop,
                    message: $"Now that I have this | { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopBasic).readableName },\nI could destroy | { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName }\nto get back some | construction materials.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.WorkshopBasic), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular) },
                    fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.WorkshopBasic }),

                new PieceHint(
                    type: PieceHint.Type.CanBuildWorkshop,
                    message: $"If I had more | wood, I could build an | { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName }.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.WoodLogRegular).texture, PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) },
                    tutorialsToActivate: new List<Tutorials.Type> { Tutorials.Type.BuildWorkshop },
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLogRegular },
                    piecesCraftedCount: new List<CountComparison> {
                        new CountComparison(name: PieceTemplate.Name.WorkshopEssential, count: 0, comparison: CountComparison.Comparison.Equal) }),

                new PieceHint(
                    type: PieceHint.Type.CineSmallBase,
                    generalHintToActivate: HintEngine.Type.CineSmallBase,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    piecesCraftedCount: new List<CountComparison> {
                        new CountComparison(name: PieceTemplate.Name.WorkshopEssential, count: 1),
                        new CountComparison(name: PieceTemplate.Name.TentSmall, count: 1)},
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int>{{ PieceTemplate.Name.WorkshopEssential, 1 }, { PieceTemplate.Name.TentSmall, 1 } }, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.TentSmall, PieceTemplate.Name.WorkshopEssential }),

                new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors1,
                    showCineCurtains: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "Are there other | | survivors?", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PlayerBoy), PieceInfo.GetTexture(PieceTemplate.Name.PlayerGirl)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "I should look for them... ", boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    ignoreHintSetting: true,
                    islandTimeElapsedHours: 1,
                    distanceWalkedKilometers: 1f),

                new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors2,
                    showCineCurtains: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "I don't see any survivors.", boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "Let's not lose hope... ", boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    ignoreHintSetting: true,
                    distanceWalkedKilometers: 8f,
                    shownPieceHints: new List<PieceHint.Type> { PieceHint.Type.CineLookForSurvivors1 }),

                new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors3,
                    showCineCurtains: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "I guess I'm the only one here...\nAll the other passengers have... | Well...", imageList: new List<Texture2D>{ AnimData.framesForPkgs[AnimData.PkgName.SkullAndBones].texture }, boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "No point in thinking about it now.\nI have to focus on | | | my own survival!", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeIron), PieceInfo.GetTexture(PieceTemplate.Name.Meal), PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    ignoreHintSetting: true,
                    mapDiscoveredPercentage: 0.22f,
                    distanceWalkedKilometers: 20f,
                    shownPieceHints: new List<PieceHint.Type> { PieceHint.Type.CineLookForSurvivors2 }),

                 new PieceHint(
                    type: PieceHint.Type.CineDay2,
                    showCineCurtains: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "A day has passed already.\nIs there any way for me to return home?", boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "Right now, there are more important matters.\nLike | food, | shelter...", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.TentMedium) }, boxType: HintMessage.BoxType.Dialogue, blockInput: true)},
                    ignoreHintSetting: true,
                    partsOfDay: new List<IslandClock.PartOfDay>{ IslandClock.PartOfDay.Morning, IslandClock.PartOfDay.Noon },
                    islandTimeElapsedHours: 20),

                  new PieceHint(
                    type: PieceHint.Type.CineDay3,
                    showCineCurtains: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "Another day has passed.", boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "I guess I'm gonna | live here for a while...", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.TentMedium) }, boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    },
                    ignoreHintSetting: true,
                    partsOfDay: new List<IslandClock.PartOfDay>{ IslandClock.PartOfDay.Morning },
                    islandTimeElapsedHours: 40,
                    shownPieceHints: new List<PieceHint.Type> { PieceHint.Type.CineDay2 }),

                   new PieceHint(
                    type: PieceHint.Type.CineDay4,
                    showCineCurtains: true,
                    messageList: new List<HintMessage> {
                    new HintMessage(text: "What day is it?", boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "I've lost the track of time already...", boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    new HintMessage(text: "Well, what matters is that I can | survive.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeIron) }, boxType: HintMessage.BoxType.Dialogue, blockInput: true),
                    },
                    ignoreHintSetting: true,
                    partsOfDay: new List<IslandClock.PartOfDay>{ IslandClock.PartOfDay.Morning },
                    islandTimeElapsedHours: 6 * 24,
                    shownPieceHints: new List<PieceHint.Type> { PieceHint.Type.CineDay3 }),

                new PieceHint(
                    type: PieceHint.Type.DangerousTiger, fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.Tiger},
                    message: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.Tiger).readableName} looks very dangerous!\nI'd rather stay away.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Tiger) }),

                new PieceHint(
                    type: PieceHint.Type.ExplosiveGas, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.SwampGas },
                    message: $"This gas seems to be | flammable.\nI should take care when using || {PieceInfo.GetInfo(PieceTemplate.Name.ArrowExploding).readableName} there.",
                    imageList: new List<Texture2D>{ AnimData.framesForPkgs[AnimData.PkgName.Flame].texture, PieceInfo.GetTexture(PieceTemplate.Name.ArrowExploding), PieceInfo.GetTexture(PieceTemplate.Name.BowBasic) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.BowBasic, PieceTemplate.Name.BowAdvanced },
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.ArrowExploding }),

                new PieceHint(
                    type: PieceHint.Type.TreasureJar, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.JarTreasureRich, PieceTemplate.Name.JarTreasurePoor },
                    message: $"I need to destroy this | {PieceInfo.GetInfo(PieceTemplate.Name.JarTreasureRich).readableName} to see what's inside.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.JarTreasureRich) }),

                new PieceHint(
                    type: PieceHint.Type.CandleForLantern,
                    message: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.LanternEmpty).readableName} will not work without a | {PieceInfo.GetInfo(PieceTemplate.Name.Candle).readableName} inside.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.LanternEmpty), PieceInfo.GetTexture(PieceTemplate.Name.Candle) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.LanternEmpty },
                    tutorialsToActivate: new List<Tutorials.Type> { Tutorials.Type.CombineItems }),

                new PieceHint(
                    type: PieceHint.Type.DeadAnimal,
                    message: $"I need to use | { PieceInfo.GetInfo(PieceTemplate.Name.WorkshopMeatHarvesting).readableName }\nto process this animal.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMeatHarvesting) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.Rabbit, PieceTemplate.Name.Frog, PieceTemplate.Name.Fox, PieceTemplate.Name.Tiger }),

                new PieceHint(
                    type: PieceHint.Type.PoisonousMeat,
                    message: $"This | { PieceInfo.GetInfo(PieceTemplate.Name.MeatRawRegular).readableName } looks poisonous.\nI should | cook or | dry it before eating.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.MeatRawRegular), PieceInfo.GetTexture(PieceTemplate.Name.HotPlate), PieceInfo.GetTexture(PieceTemplate.Name.MeatDryingRackRegular) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.MeatRawRegular, PieceTemplate.Name.MeatRawPrime }),

                new PieceHint(
                    type: PieceHint.Type.MakeOilNegative,
                    message: $"If I had an | { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName },\nI could make a | { PieceInfo.GetInfo(PieceTemplate.Name.BottleOfOil).readableName }\nfrom this | { PieceInfo.GetInfo(PieceTemplate.Name.Fat).readableName }.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).texture, PieceInfo.GetTexture(PieceTemplate.Name.BottleOfOil), PieceInfo.GetTexture(PieceTemplate.Name.Fat) },
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.Fat },
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int> { { PieceTemplate.Name.AlchemyLabStandard, 0 } }
                    ),

                new PieceHint(
                    type: PieceHint.Type.MakeOilPositiveFat,
                    message: $"I could use my | { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName }\nto make a | { PieceInfo.GetInfo(PieceTemplate.Name.BottleOfOil).readableName }\nfrom this | { PieceInfo.GetInfo(PieceTemplate.Name.Fat).readableName }.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).texture, PieceInfo.GetTexture(PieceTemplate.Name.BottleOfOil), PieceInfo.GetTexture(PieceTemplate.Name.Fat) },
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.Fat },
                    piecesCraftedCount: new List<CountComparison> {
                        new CountComparison(name: PieceTemplate.Name.AlchemyLabStandard, count: 1, comparison: CountComparison.Comparison.GreaterOrEqual) },
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int> { { PieceTemplate.Name.AlchemyLabStandard, 1 } }
                    ),

                new PieceHint(
                    type: PieceHint.Type.MakeOilPositiveSeeds,
                    message: $"I could use my | { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName }\nto make a | { PieceInfo.GetInfo(PieceTemplate.Name.BottleOfOil).readableName }\nfrom these | { PieceInfo.GetInfo(PieceTemplate.Name.SeedsGeneric).readableName }.",
                    imageList: new List<Texture2D>{ PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).texture, PieceInfo.GetTexture(PieceTemplate.Name.BottleOfOil), PieceInfo.GetTexture(PieceTemplate.Name.SeedsGeneric) },
                    playerOwnsAllOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.SeedsGeneric },
                    piecesCraftedCount: new List<CountComparison> {
                        new CountComparison(name: PieceTemplate.Name.AlchemyLabStandard, count: 1, comparison: CountComparison.Comparison.GreaterOrEqual) },
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int> { { PieceTemplate.Name.AlchemyLabStandard, 1 } }
                    ),

                new PieceHint(
                    type: PieceHint.Type.PlantingNegative,
                    message: "I need a | fertile ground to plant these seeds.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.FertileGroundMedium) },
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> { PieceTemplate.Name.SeedsGeneric },
                    piecesCraftedCount: new List<CountComparison> { new CountComparison(name: PieceTemplate.Name.FertileGroundSmall, count: 0, comparison: CountComparison.Comparison.Equal) }),

               new PieceHint(
                    type: PieceHint.Type.PlantingPositive, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.FertileGroundSmall },
                    message: "Now I can plant seeds | | |.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.SeedsGeneric), PieceInfo.GetTexture(PieceTemplate.Name.Acorn), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw)},
                    alsoDisables: new List<PieceHint.Type> {PieceHint.Type.PlantingNegative },
                    tutorialsToActivate: new List<Tutorials.Type> { Tutorials.Type.Plant }),

               new PieceHint(
                    type: PieceHint.Type.WoodenFenceNegative, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.FertileGroundSmall },
                    message: "Maybe I should build a | wooden fence, to protect my | | crops.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.FenceHorizontalShort), PieceInfo.GetTexture(PieceTemplate.Name.CarrotPlant), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw)}),

               new PieceHint(
                    type: PieceHint.Type.WoodenFencePositive, fieldPiecesNearby: new List<PieceTemplate.Name> { PieceTemplate.Name.FenceHorizontalShort, PieceTemplate.Name.FenceVerticalShort },
                    message: "I might be able to jump over this | fence.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.FenceHorizontalShort) },
                    alsoDisables: new List<PieceHint.Type> { PieceHint.Type.WoodenFenceNegative }),

               new PieceHint(
                    type: PieceHint.Type.SwampDigSite,
                    message: "I think that | something valuable could be | buried | here...",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.ChestTreasureBig), PieceInfo.GetTexture(PieceTemplate.Name.ShovelIron), PieceInfo.GetTexture(PieceTemplate.Name.SwampDigSite) },
                    fieldPiecesNearby: new List<PieceTemplate.Name> {PieceTemplate.Name.SwampDigSite},
                    playerOwnsAnyOfThesePieces: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal}),
            };

            CheckData(newPieceHintList);

            return newPieceHintList;
        }

        private static void CheckData(List<PieceHint> pieceHintList)
        {
            foreach (PieceHint.Type type in (PieceHint.Type[])Enum.GetValues(typeof(PieceHint.Type)))
            {
                bool hintFound = false;

                foreach (PieceHint pieceHint in pieceHintList)
                {
                    if (pieceHint.type == type)
                    {
                        hintFound = true;
                        break;
                    }
                }

                if (!hintFound) throw new ArgumentException($"No pieceHint for type {type}.");
            }
        }
    }
}