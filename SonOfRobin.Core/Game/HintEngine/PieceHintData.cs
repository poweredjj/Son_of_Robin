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
                    type: PieceHint.Type.CrateAnother,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CrateRegular}),

                new PieceHint(
                    type: PieceHint.Type.TentModernPacked,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.TentModernPacked},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Craft}),

                new PieceHint(
                    type: PieceHint.Type.HerbsRed,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HerbsRed}),

                new PieceHint(
                    type: PieceHint.Type.HerbsYellow,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HerbsYellow}),

                new PieceHint(
                    type: PieceHint.Type.HerbsCyan,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HerbsCyan}),

                new PieceHint(
                    type: PieceHint.Type.HerbsBlue,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HerbsBlue}),

                new PieceHint(
                    type: PieceHint.Type.HerbsViolet,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HerbsViolet}),

               new PieceHint(
                    type: PieceHint.Type.CoffeeRaw,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.CoffeeRaw }),

                new PieceHint(
                    type: PieceHint.Type.HerbsBlack,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HerbsBlack}),

                new PieceHint(
                    type: PieceHint.Type.GlassSand,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.GlassSand}),

                new PieceHint(
                    type: PieceHint.Type.ClamInventory,
                    alsoDisables: new PieceHint.Type[] { PieceHint.Type.ClamField },
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Clam}),

                new PieceHint(
                    type: PieceHint.Type.ClamField,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Clam}),

                new PieceHint(
                    type: PieceHint.Type.Acorn,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Acorn}),

                new PieceHint(
                    type: PieceHint.Type.RedExclamation,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.BubbleExclamationRed},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.AnimalAttacking}),

                new PieceHint(
                    type: PieceHint.Type.LeatherPositive,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Leather}),

                new PieceHint(
                    type: PieceHint.Type.MapPositive,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Map },
                    playerEquipmentDoesNotContainThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.Map },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.Equip }),

                new PieceHint(
                    type: PieceHint.Type.BackpackPositive,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BackpackSmall },
                    playerEquipmentDoesNotContainThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.BackpackSmall },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.Equip }
                    ),

                new PieceHint(
                    type: PieceHint.Type.BeltPositive,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BeltSmall },
                    playerEquipmentDoesNotContainThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.BeltSmall },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.Equip }),

                new PieceHint(
                    type: PieceHint.Type.BowNoAmmo,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BowBasic, PieceTemplate.Name.BowAdvanced },
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowWood, PieceTemplate.Name.ArrowStone},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.ShootProjectile}),

                new PieceHint(
                    type: PieceHint.Type.AnimalAxe,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal}),

                new PieceHint(
                    type: PieceHint.Type.AnimalSpear,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.SpearWood, PieceTemplate.Name.SpearStone, PieceTemplate.Name.SpearIron, PieceTemplate.Name.SpearCrystal}),

                new PieceHint(
                    type: PieceHint.Type.AnimalBow,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.AnimalNegative},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BowBasic, PieceTemplate.Name.BowAdvanced }),

                new PieceHint(
                    type: PieceHint.Type.AnimalNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.Frog, PieceTemplate.Name.Rabbit, PieceTemplate.Name.Fox},
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{ PieceTemplate.Name.BowBasic, PieceTemplate.Name.SpearStone, PieceTemplate.Name.BowAdvanced }),

                new PieceHint(
                    type: PieceHint.Type.DigSitePositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BeachDigSite, PieceTemplate.Name.ForestDigSite, PieceTemplate.Name.SwampDigSite, PieceTemplate.Name.GlassDigSite },
                    alsoDisables: new PieceHint.Type[] { PieceHint.Type.DigSiteNegative },
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: PieceHint.Type.DigSiteNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BeachDigSite, PieceTemplate.Name.ForestDigSite, PieceTemplate.Name.SwampDigSite, PieceTemplate.Name.GlassDigSite },
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),

                new PieceHint(
                    type: PieceHint.Type.DigSiteGlass,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.GlassDigSite }),

                new PieceHint(
                    type: PieceHint.Type.WoodNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.WoodPositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.TreeBig, PieceTemplate.Name.TreeSmall},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.GetWood},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.AxeWood, PieceTemplate.Name.AxeStone, PieceTemplate.Name.AxeIron, PieceTemplate.Name.AxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.StonePositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.MineralsMossyBig, PieceTemplate.Name.MineralsMossySmall},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.StoneNegative},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.StoneNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.MineralsSmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.MineralsMossyBig, PieceTemplate.Name.MineralsMossySmall},
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.CrystalPositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.CrystalDepositBig },
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.CrystalNegative},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.PickaxeIron }),

                new PieceHint(
                    type: PieceHint.Type.CrystalNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.CrystalDepositBig },
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{ PieceTemplate.Name.PickaxeIron }),

                new PieceHint(
                    type: PieceHint.Type.CoalDepositPositive,  fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.CoalDepositNegative },
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.CoalDepositNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CoalDeposit},
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.IronDepositPositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.IronDepositNegative },
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Mine},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.IronDepositNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.IronDeposit},
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.PickaxeWood, PieceTemplate.Name.PickaxeStone, PieceTemplate.Name.PickaxeIron, PieceTemplate.Name.PickaxeCrystal }),

                new PieceHint(
                    type: PieceHint.Type.FruitTree,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.AppleTree, PieceTemplate.Name.CherryTree},
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.BananaTree,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.BananaTree},
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.TomatoPlant,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.TomatoPlant},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.CarrotPlant,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CarrotPlant},
                    fieldPieceHasNotEmptyStorage: true,
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.ShakeFruit}),

                new PieceHint(
                    type: PieceHint.Type.HotPlate,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.HotPlate},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Cook}),

                new PieceHint(
                    type: PieceHint.Type.Cooker,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CookingPot},
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.HotPlate },
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Cook}),

                new PieceHint(
                    type: PieceHint.Type.Furnace,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.FurnaceComplete},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Smelt}),

                new PieceHint(
                    type: PieceHint.Type.HarvestingWorkshop,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.WorkshopMeatHarvesting},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.HarvestMeat}),

                new PieceHint(
                    type: PieceHint.Type.AlchemyLab,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.AlchemyLabStandard },
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.PotionBrew}),

                new PieceHint(
                    type: PieceHint.Type.TorchPositive,
                    alsoDisables: new PieceHint.Type[] {PieceHint.Type.TorchNegative },
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Torch},
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.TorchSmall, PieceTemplate.Name.TorchBig}),

                new PieceHint(
                    type: PieceHint.Type.TorchNegative,
                    playerDoesNotOwnAnyOfThesePieces: new PieceTemplate.Name[]{PieceTemplate.Name.TorchSmall},
                    partsOfDay: new HashSet<IslandClock.PartOfDay> {IslandClock.PartOfDay.Evening, IslandClock.PartOfDay.Night}),

                new PieceHint(
                    type: PieceHint.Type.Fireplace,
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.Fireplace},
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CampfireSmall}),

                new PieceHint(
                    type: PieceHint.Type.SharedChest,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.ChestCrystal }),

                new PieceHint(
                    type: PieceHint.Type.CanDestroyEssentialWorkshop,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.WorkshopBasic }),

                new PieceHint(
                    type: PieceHint.Type.CanBuildWorkshop,
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.BuildWorkshop },
                    playerOwnsAllOfThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.WoodLogRegular },
                    piecesCraftedCount: new CountComparison[] {
                        new CountComparison(name: PieceTemplate.Name.WorkshopEssential, count: 0, comparison: CountComparison.Comparison.Equal) }),

                new PieceHint(
                    type: PieceHint.Type.CineCrateStarting,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.CrateStarting},
                    tutorialsToActivate: new Tutorials.Type[] {Tutorials.Type.BreakThing}),

                new PieceHint(
                    type: PieceHint.Type.CineTentModernCantPack,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> {PieceTemplate.Name.TentModern}),

                new PieceHint(
                    type: PieceHint.Type.CineSmallBase,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    generalHintToActivate: HintEngine.Type.CineSmallBase,
                    piecesCraftedCount: new CountComparison[] {
                        new CountComparison(name: PieceTemplate.Name.WorkshopBasic, count: 1),
                        new CountComparison(name: PieceTemplate.Name.TentModern, count: 1)},
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int>{{ PieceTemplate.Name.WorkshopBasic, 1 }, { PieceTemplate.Name.TentModern, 1 } },
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.WorkshopBasic }),

                 new PieceHint(
                    type: PieceHint.Type.CineRuins,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.RuinsWall, PieceTemplate.Name.RuinsColumn, PieceTemplate.Name.RuinsRubble, PieceTemplate.Name.RuinsDigSite }),

                 new PieceHint(
                    type: PieceHint.Type.CineCave,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.CaveEntranceOutside },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.Caves }),

                 new PieceHint(
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    type: PieceHint.Type.CineTotem,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Totem }),

                new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors1,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    islandTimeElapsedHours: 1,
                    distanceWalkedKilometers: 1f),

               new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors2,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    distanceWalkedKilometers: 8f,
                    shownPieceHints: new HashSet<PieceHint.Type> { PieceHint.Type.CineLookForSurvivors1 }),

               new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors3,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    mapDiscoveredPercentage: 0.18f,
                    distanceWalkedKilometers: 20f,
                    shownPieceHints: new HashSet<PieceHint.Type> { PieceHint.Type.CineLookForSurvivors2 }
                    ),

                new PieceHint(
                    type: PieceHint.Type.CineLookForSurvivors4,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    mapDiscoveredPercentage: 0.19f,
                    distanceWalkedKilometers: 23f,
                    shownPieceHints: new HashSet<PieceHint.Type> { PieceHint.Type.CineLookForSurvivors3 },
                    recipesToUnlock: new PieceTemplate.Name[] { PieceTemplate.Name.BoatConstructionSite }
                    ),

               new PieceHint(
                    type: PieceHint.Type.CineDay1,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    partsOfDay: new HashSet<IslandClock.PartOfDay> { IslandClock.PartOfDay.Morning, IslandClock.PartOfDay.Noon },
                    islandTimeElapsedHours: 20),

               new PieceHint(
                    type: PieceHint.Type.CineDay2,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    partsOfDay: new HashSet<IslandClock.PartOfDay> { IslandClock.PartOfDay.Morning },
                    islandTimeElapsedHours: 40,
                    shownPieceHints: new HashSet<PieceHint.Type> { PieceHint.Type.CineDay1 }),

                new PieceHint(
                    type: PieceHint.Type.CineDay3,
                    showCineCurtains: true,
                    ignoreHintSetting: true,
                    partsOfDay: new HashSet<IslandClock.PartOfDay> { IslandClock.PartOfDay.Morning },
                    islandTimeElapsedHours: 6 * 24,
                    shownPieceHints: new HashSet<PieceHint.Type> { PieceHint.Type.CineDay2 }),

                new PieceHint(
                    type: PieceHint.Type.ConstructionSite,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.FurnaceConstructionSite },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.ConstructionSites }),

                new PieceHint(
                    type: PieceHint.Type.DangerousBear,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Bear }),

                new PieceHint(
                    type: PieceHint.Type.ExplosiveGas,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.SwampGas },
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.BowBasic, PieceTemplate.Name.BowAdvanced },
                    playerOwnsAllOfThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.ArrowExploding }),

                new PieceHint(
                    type: PieceHint.Type.TreasureJar,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.JarTreasureRich, PieceTemplate.Name.JarTreasurePoor }),

                new PieceHint(
                    type: PieceHint.Type.CandleForLantern,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.LanternEmpty },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.CombineItems }),

                new PieceHint(
                    type: PieceHint.Type.DeadAnimal,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Rabbit, PieceTemplate.Name.Frog, PieceTemplate.Name.Fox, PieceTemplate.Name.Bear }),

                new PieceHint(
                    type: PieceHint.Type.PoisonousMeat,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.MeatRawRegular, PieceTemplate.Name.MeatRawPrime }),

                new PieceHint(
                    type: PieceHint.Type.MakeOilNegative,
                    playerOwnsAllOfThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.Fat },
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int> { { PieceTemplate.Name.AlchemyLabStandard, 0 } }
                    ),

                new PieceHint(
                    type: PieceHint.Type.MakeOilPositiveFat,
                    playerOwnsAllOfThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.Fat },
                    piecesCraftedCount: new CountComparison[] {
                        new CountComparison(name: PieceTemplate.Name.AlchemyLabStandard, count: 1, comparison: CountComparison.Comparison.GreaterOrEqual) },
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int> { { PieceTemplate.Name.AlchemyLabStandard, 1 } }
                    ),

                new PieceHint(
                    type: PieceHint.Type.MakeOilPositiveSeeds,
                    playerOwnsAllOfThesePieces: new PieceTemplate.Name[] { PieceTemplate.Name.SeedsGeneric },
                    piecesCraftedCount: new CountComparison[] {
                        new CountComparison(name: PieceTemplate.Name.AlchemyLabStandard, count: 1, comparison: CountComparison.Comparison.GreaterOrEqual) },
                    existingPiecesCount: new Dictionary<PieceTemplate.Name, int> { { PieceTemplate.Name.AlchemyLabStandard, 1 } }
                    ),

                new PieceHint(
                    type: PieceHint.Type.PlantingNegative,
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.SeedsGeneric },
                    piecesCraftedCount: new CountComparison[] { new CountComparison(name: PieceTemplate.Name.FertileGroundSmall, count: 0, comparison: CountComparison.Comparison.Equal) }),

               new PieceHint(
                    type: PieceHint.Type.PlantingPositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.FertileGroundSmall },
                    alsoDisables: new PieceHint.Type[] { PieceHint.Type.PlantingNegative },
                    tutorialsToActivate: new Tutorials.Type[] { Tutorials.Type.Plant }),

               new PieceHint(
                    type: PieceHint.Type.WoodenFenceNegative,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.FertileGroundSmall }),

               new PieceHint(
                    type: PieceHint.Type.WoodenFencePositive,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.FenceHorizontalShort, PieceTemplate.Name.FenceVerticalShort },
                    alsoDisables: new PieceHint.Type[] { PieceHint.Type.WoodenFenceNegative }),

               new PieceHint(
                    type: PieceHint.Type.SwampDigSite,
                    fieldPiecesNearby: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.SwampDigSite },
                    playerOwnsAnyOfThesePieces: new HashSet<PieceTemplate.Name> { PieceTemplate.Name.ShovelStone, PieceTemplate.Name.ShovelIron, PieceTemplate.Name.ShovelCrystal }),
            };

            CheckData(newPieceHintList);

            return newPieceHintList;
        }

        public static List<HintMessage> GetMessageList(PieceHint.Type type, World world)
        {
            switch (type)
            {
                case PieceHint.Type.CrateAnother:
                    return new List<HintMessage> {
                        new HintMessage(text: "I should check what's inside this | crate.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CrateRegular) }, blockInputDefaultDuration: true) };

                case PieceHint.Type.TentModernPacked:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.TentModernPacked).readableName} is in good shape.\nI should be able to | assemble it.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentModernPacked), PieceInfo.GetTexture(PieceTemplate.Name.TentModern) })};

                case PieceHint.Type.WoodNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could get some wood using my | bare hands,\nbut an | axe would be better.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.KnifeSimple), PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) })};

                case PieceHint.Type.WoodPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could use my | axe to get some wood.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) })};

                case PieceHint.Type.DigSiteNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "I think that something could be buried | here.\nIf I had a | shovel, I could dig there.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BeachDigSite), PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone) })};

                case PieceHint.Type.DigSitePositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could use my | shovel to dig | here.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.ShovelStone), PieceInfo.GetTexture(PieceTemplate.Name.ForestDigSite) })};

                case PieceHint.Type.StoneNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "If I had a | pickaxe, I could mine stones from this | mineral.\nRegular | axe could work too, but not as effective.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall), PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) })};

                case PieceHint.Type.StonePositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could use my | pickaxe to mine | stones.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.MineralsSmall) })};

                case PieceHint.Type.CrystalNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: $"Wow, this | crystal looks very strong.\nI think that a pickaxe made of wood or stone\nwould be too weak to break it.\nMaybe an | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} could work, though?",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositBig), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeIron) } )};

                case PieceHint.Type.CrystalPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.PickaxeIron).readableName} should be enough to break this | crystal.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.PickaxeIron), PieceInfo.GetTexture(PieceTemplate.Name.CrystalDepositBig) })};

                case PieceHint.Type.AnimalNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "I think I need some | | weapon to hunt this animal.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.SpearWood), PieceInfo.GetTexture(PieceTemplate.Name.BowBasic) })};

                case PieceHint.Type.AnimalBow:
                    return new List<HintMessage> {
                        new HintMessage(text: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.BowBasic).readableName} should be great for hunting.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BowBasic) })};

                case PieceHint.Type.AnimalSpear:
                    return new List<HintMessage> {
                        new HintMessage(text: $"My | {PieceInfo.GetInfo(PieceTemplate.Name.SpearWood).readableName} should be great for animal hunting.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.SpearWood) })};

                case PieceHint.Type.AnimalAxe:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could try to use my | axe to hunt this animal.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.AxeStone) })};

                case PieceHint.Type.BowNoAmmo:
                    return new List<HintMessage> {
                        new HintMessage(text: "I need | arrows...",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.ArrowStone) })};

                case PieceHint.Type.ClamField:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.Clam).readableName} should be edible after cooking.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Clam) })};

                case PieceHint.Type.ClamInventory:
                    return new List<HintMessage> {
                        new HintMessage(text: $"I cannot eat this | {PieceInfo.GetInfo(PieceTemplate.Name.Clam).readableName} raw.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Clam) })};

                case PieceHint.Type.FruitTree:
                    return new List<HintMessage> {
                        new HintMessage(text: "This fruit looks edible. I should shake it off this | tree.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TreeBig) })};

                case PieceHint.Type.BananaTree:
                    return new List<HintMessage> {
                        new HintMessage(text: "A | banana! It could be possible, to shake it off | this tree.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.BananaTree) })};

                case PieceHint.Type.TomatoPlant:
                    return new List<HintMessage> {
                        new HintMessage(text: "A | tomato... Looks tasty.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Tomato) })};

                case PieceHint.Type.IronDepositNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "I think this is an | iron deposit.\nIf I had a | pickaxe, I could mine | iron ore.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.IronOre) })};

                case PieceHint.Type.IronDepositPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could use my | pickaxe to mine | iron ore here |.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.IronDeposit) })};

                case PieceHint.Type.CoalDepositNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "I think this | is a coal deposit.\nIf I had a | pickaxe, I could get | coal.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit), PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.Coal) })};

                case PieceHint.Type.CoalDepositPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I could use my | pickaxe to mine | coal here |.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.PickaxeStone), PieceInfo.GetTexture(PieceTemplate.Name.Coal), PieceInfo.GetTexture(PieceTemplate.Name.CoalDeposit) })};

                case PieceHint.Type.HotPlate:
                    return new List<HintMessage> {
                        new HintMessage(text: "| I can cook simple meals now!",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HotPlate) })};

                case PieceHint.Type.Cooker:
                    return new List<HintMessage> {
                        new HintMessage(text: "| Now I can cook like a pro!",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CookingPot) })};

                case PieceHint.Type.Furnace:
                    return new List<HintMessage> {
                        new HintMessage(text: "Finally! Now I can | smelt!",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.FurnaceComplete) })};

                case PieceHint.Type.LeatherPositive:
                    return new List<HintMessage> {
                    new HintMessage(text: $"If I had more | { PieceInfo.GetInfo(PieceTemplate.Name.Leather).readableName },\nI could make a | backpack or a | belt.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Leather), PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall), PieceInfo.GetTexture(PieceTemplate.Name.BeltSmall)}, blockInputDefaultDuration: true)};

                case PieceHint.Type.BackpackPositive:
                    return new List<HintMessage> {
                    new HintMessage(text: "This backpack | will allow me to carry more items.\nI should equip it now.",
                    imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.BackpackSmall)}, blockInputDefaultDuration: true)};

                case PieceHint.Type.BeltPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I should equip my | belt now.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BeltSmall) })};

                case PieceHint.Type.MapPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: $"I should equip this {PieceInfo.GetInfo(PieceTemplate.Name.Map).readableName} | to use it.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Map) })};

                case PieceHint.Type.RedExclamation:
                    return new List<HintMessage> {
                        new HintMessage(text: "This animal is | attacking me!",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BubbleExclamationRed) })};

                case PieceHint.Type.Acorn:
                    return new List<HintMessage> {
                        new HintMessage(text: $"After cooking, this | {PieceInfo.GetInfo(PieceTemplate.Name.Acorn).readableName} should be edible.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Acorn) })};

                case PieceHint.Type.TorchNegative:
                    return new List<HintMessage> {
                        new HintMessage("It's getting dark.", blockInputDefaultDuration: true),
                        new HintMessage(text: "I need some light. A | torch, maybe?\nOr a | bonfire?", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchSmall), PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall)  }, blockInputDefaultDuration: true)};

                case PieceHint.Type.TorchPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "This | torch will make navigating at night a lot easier.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TorchBig) })};

                case PieceHint.Type.Fireplace:
                    return new List<HintMessage> {
                        new HintMessage(text: "This | bonfire looks ok.\nAll I need now is some | | | wood...?",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CampfireSmall), PieceInfo.GetTexture(PieceTemplate.Name.Stick), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular), PieceInfo.GetTexture(PieceTemplate.Name.WoodPlank) })};

                case PieceHint.Type.HerbsRed:
                    return new List<HintMessage> {
                        new HintMessage(text: "I think I could use these | herbs to make a healing potion.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsRed) })};

                case PieceHint.Type.HerbsYellow:
                    return new List<HintMessage> {
                        new HintMessage(text: "These | herbs could be used to make a strength potion.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsYellow) })};

                case PieceHint.Type.HerbsViolet:
                    return new List<HintMessage> {
                        new HintMessage(text: "Hmm... These | herbs should make my fatigue go away.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsViolet)})};

                case PieceHint.Type.HerbsCyan:
                    return new List<HintMessage> {
                        new HintMessage(text: "Hmm... These | herbs sure look interesting.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsCyan) })};

                case PieceHint.Type.HerbsBlue:
                    return new List<HintMessage> {
                        new HintMessage(text: "These | herbs could be used to make a stamina potion.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlue) })};

                case PieceHint.Type.HerbsBlack:
                    return new List<HintMessage> {
                        new HintMessage(text: "These | herbs look poisonous.\nThey could be useful.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.HerbsBlack) })};

                case PieceHint.Type.GlassSand:
                    return new List<HintMessage> {
                        new HintMessage(text: "This | is no ordinary sand!\nIt can be used to make glass.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.GlassSand) })};

                case PieceHint.Type.CanBuildWorkshop:
                    return new List<HintMessage> {
                        new HintMessage(text: $"If I had more | wood, I could build an | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName}.",
                        imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.WoodLogRegular).Texture, PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential) })};

                case PieceHint.Type.DangerousBear:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.Bear).readableName} looks very dangerous!\nI'd rather stay away.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Bear) })};

                case PieceHint.Type.DigSiteGlass:
                    return new List<HintMessage> {
                        new HintMessage(text: "This spot | looks interesting. I might find something useful there.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.GlassDigSite) })};

                case PieceHint.Type.CarrotPlant:
                    return new List<HintMessage> {
                        new HintMessage(text: "A | carrot... Looks tasty.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Carrot) })};

                case PieceHint.Type.ExplosiveGas:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This gas seems to be | flammable.\nI should take care when using || {PieceInfo.GetInfo(PieceTemplate.Name.ArrowExploding).readableName} there.",
                        imageList: new List<Texture2D> { AnimData.GetCroppedFrameForPackage(AnimData.PkgName.Flame).Texture, PieceInfo.GetTexture(PieceTemplate.Name.ArrowExploding), PieceInfo.GetTexture(PieceTemplate.Name.BowBasic) })};

                case PieceHint.Type.TreasureJar:
                    return new List<HintMessage> {
                        new HintMessage(text: $"I need to destroy this | {PieceInfo.GetInfo(PieceTemplate.Name.JarTreasureRich).readableName} to see what's inside.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.JarTreasureRich) })};

                case PieceHint.Type.CandleForLantern:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.LanternEmpty).readableName} will not work without a | {PieceInfo.GetInfo(PieceTemplate.Name.Candle).readableName} inside.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.LanternEmpty), PieceInfo.GetTexture(PieceTemplate.Name.Candle) })};

                case PieceHint.Type.CoffeeRaw:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This is | {PieceInfo.GetInfo(PieceTemplate.Name.CoffeeRaw).readableName}!\nI could roast it in a | {PieceInfo.GetInfo(PieceTemplate.Name.FurnaceComplete).readableName}.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw), PieceInfo.GetTexture(PieceTemplate.Name.FurnaceComplete) })};

                case PieceHint.Type.SharedChest:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.ChestCrystal).readableName} defies the laws of physics, somehow.\nEvery other | {PieceInfo.GetInfo(PieceTemplate.Name.ChestCrystal).readableName} will share the items that are stored inside.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.ChestCrystal), PieceInfo.GetTexture(PieceTemplate.Name.ChestCrystal) })};

                case PieceHint.Type.CanDestroyEssentialWorkshop:
                    return new List<HintMessage> {
                        new HintMessage(text: $"Now that I have this | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopBasic).readableName},\nI could destroy | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopEssential).readableName}\nto get back some | construction materials.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WorkshopBasic), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopEssential), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular) })};

                case PieceHint.Type.AlchemyLab:
                    return new List<HintMessage> {
                        new HintMessage(text: "| I can start brewing\n| potions now!",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.AlchemyLabStandard), AnimData.GetCroppedFrameForPackage(AnimData.PkgName.PotionRed).Texture })};

                case PieceHint.Type.PoisonousMeat:
                    return new List<HintMessage> {
                        new HintMessage(text: $"This | {PieceInfo.GetInfo(PieceTemplate.Name.MeatRawRegular).readableName} looks poisonous.\nI should | cook or | dry it before eating.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatRawRegular), PieceInfo.GetTexture(PieceTemplate.Name.HotPlate), PieceInfo.GetTexture(PieceTemplate.Name.MeatDryingRackRegular) })};

                case PieceHint.Type.MakeOilPositiveFat:
                    return new List<HintMessage> {
                        new HintMessage(text: $"I could use my | {PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName}\nto make a | {PieceInfo.GetInfo(PieceTemplate.Name.BottleOfOil).readableName}\nfrom this | {PieceInfo.GetInfo(PieceTemplate.Name.Fat).readableName}.",
                        imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).Texture, PieceInfo.GetTexture(PieceTemplate.Name.BottleOfOil), PieceInfo.GetTexture(PieceTemplate.Name.Fat) })};

                case PieceHint.Type.MakeOilNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: $"If I had an | {PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName},\nI could make a | {PieceInfo.GetInfo(PieceTemplate.Name.BottleOfOil).readableName}\nfrom this | {PieceInfo.GetInfo(PieceTemplate.Name.Fat).readableName}.",
                        imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).Texture, PieceInfo.GetTexture(PieceTemplate.Name.BottleOfOil), PieceInfo.GetTexture(PieceTemplate.Name.Fat) })};

                case PieceHint.Type.MakeOilPositiveSeeds:
                    return new List<HintMessage> {
                        new HintMessage(text: $"I could use my | {PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).readableName}\nto make a | {PieceInfo.GetInfo(PieceTemplate.Name.BottleOfOil).readableName}\nfrom these | {PieceInfo.GetInfo(PieceTemplate.Name.SeedsGeneric).readableName}.",
                        imageList: new List<Texture2D> { PieceInfo.GetInfo(PieceTemplate.Name.AlchemyLabStandard).Texture, PieceInfo.GetTexture(PieceTemplate.Name.BottleOfOil), PieceInfo.GetTexture(PieceTemplate.Name.SeedsGeneric) })};

                case PieceHint.Type.PlantingNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "I need a | fertile ground to plant these seeds.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.FertileGroundMedium) })};

                case PieceHint.Type.PlantingPositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "Now I can plant seeds | | |.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.SeedsGeneric), PieceInfo.GetTexture(PieceTemplate.Name.Acorn), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw) })};

                case PieceHint.Type.WoodenFenceNegative:
                    return new List<HintMessage> {
                        new HintMessage(text: "Maybe I should build a | wooden fence, to protect my | | crops.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.FenceHorizontalShort), PieceInfo.GetTexture(PieceTemplate.Name.CarrotPlant), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw) })};

                case PieceHint.Type.WoodenFencePositive:
                    return new List<HintMessage> {
                        new HintMessage(text: "I might be able to jump over this | fence.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.FenceHorizontalShort) })};

                case PieceHint.Type.DeadAnimal:
                    return world != null && world.Player.Skill == Player.SkillName.Hunter ?
                        new List<HintMessage> {
                        new HintMessage(text: $"I know how to | process this animal right now,\nbut if I had a | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopMeatHarvesting).readableName}, I could get more | meat.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.KnifeSimple), PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMeatHarvesting), PieceInfo.GetTexture(PieceTemplate.Name.MeatRawPrime) })} :
                        new List<HintMessage> {
                        new HintMessage(text: $"I need to use | {PieceInfo.GetInfo(PieceTemplate.Name.WorkshopMeatHarvesting).readableName}\nto process this animal.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMeatHarvesting) })};

                case PieceHint.Type.HarvestingWorkshop:
                    return new List<HintMessage> {
                        new HintMessage(text: "| Now I can harvest meat from | animals!",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.WorkshopMeatHarvesting), TextureBank.GetTexture(textureName: TextureBank.TextureName.Animal)})};

                case PieceHint.Type.SwampDigSite:
                    return new List<HintMessage> {
                        new HintMessage(text: "I think that | something valuable could be | buried | here...",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.ChestTreasureBig), PieceInfo.GetTexture(PieceTemplate.Name.ShovelIron), PieceInfo.GetTexture(PieceTemplate.Name.SwampDigSite) })};

                case PieceHint.Type.CineCrateStarting:
                    return new List<HintMessage> {
                        new HintMessage(text: "I've seen | crates like this on the ship.\nIt could contain valuable supplies.\nI should try to | break it open.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.CrateStarting), PieceInfo.GetTexture(PieceTemplate.Name.KnifeSimple) })};

                case PieceHint.Type.CineTentModernCantPack:
                    return new List<HintMessage> {
                        new HintMessage(text: "I won't be able to | pack this | tent back without damaging it.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.TentModernPacked), PieceInfo.GetTexture(PieceTemplate.Name.TentModern) }),
                        new HintMessage(text: "I cannot | take it with me.\nIt will have to | stay assembled here.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.BackpackMedium), PieceInfo.GetTexture(PieceTemplate.Name.TentModern) })
                    };

                case PieceHint.Type.ConstructionSite:
                    return new List<HintMessage> {
                        new HintMessage(text: "Well, this looks like a nice construction site.",
                        imageList: new List<Texture2D> { })};

                case PieceHint.Type.CineSmallBase:
                    return new List<HintMessage> { };

                case PieceHint.Type.CineRuins:
                    return new List<HintMessage> {
                        new HintMessage(text: "I didn't expect that I'd find | ruins here.\nSome people must have lived here a long ago...\nI should look around.",
                        imageList: new List<Texture2D> { AnimData.GetCroppedFrameForPackage(AnimData.PkgName.RuinsWallHorizontal1).Texture })};

                case PieceHint.Type.CineCave:
                    return new List<HintMessage> {
                        new HintMessage(text: "This looks like a | cave.\nI wonder what's inside?\nStill, it might be dangerous...",
                        imageList: new List<Texture2D> { AnimData.GetCroppedFrameForPackage(AnimData.PkgName.CaveEntrance).Texture })};

                case PieceHint.Type.CineTotem:
                    return new List<HintMessage> {
                        new HintMessage(text: $"Hmm...\nI can feel mysterious aura surrounding this | {PieceInfo.GetInfo(PieceTemplate.Name.Totem).readableName}.\nMaybe I should examine it.",
                        imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Totem) })};

                case PieceHint.Type.CineLookForSurvivors1:
                    return new List<HintMessage> {
                   new HintMessage(text: "Are there other | | survivors?", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.PlayerBoy), PieceInfo.GetTexture(PieceTemplate.Name.PlayerGirl)}, blockInputDefaultDuration: true),
                   new HintMessage(text: "I should look for them... ", blockInputDefaultDuration: true)};

                case PieceHint.Type.CineLookForSurvivors2:
                    return new List<HintMessage> {
                    new HintMessage(text: "I don't see any survivors.", blockInputDefaultDuration: true),
                    new HintMessage(text: "Let's not lose hope... ", blockInputDefaultDuration: true)};

                case PieceHint.Type.CineLookForSurvivors3:
                    return new List<HintMessage> {
                    new HintMessage(text: "I guess I'm the only one here...\nAll the other passengers have... | Well...", imageList: new List<Texture2D>{ AnimData.GetCroppedFrameForPackage(AnimData.PkgName.SkullAndBones).Texture }, blockInputDefaultDuration: true),
                    new HintMessage(text: "No point in thinking about it now.\nI have to focus on | | | my own survival!", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeIron), PieceInfo.GetTexture(PieceTemplate.Name.Meal), PieceInfo.GetTexture(PieceTemplate.Name.TentMedium)}, blockInputDefaultDuration: true)};

                case PieceHint.Type.CineLookForSurvivors4:
                    return new List<HintMessage> {
                    new HintMessage(text: "I don't want to stay on | this island forever...", imageList: new List<Texture2D>{ AnimData.GetCroppedFrameForPackage(AnimData.PkgName.PalmTree).Texture }, blockInputDefaultDuration: true),
                    new HintMessage(text: "I think I have an idea how to build a | boat!", imageList: new List<Texture2D>{ AnimData.GetCroppedFrameForPackage(AnimData.PkgName.BoatCompleteStanding).Texture }, blockInputDefaultDuration: true)};

                case PieceHint.Type.CineDay1:
                    return new List<HintMessage> {
                    new HintMessage(text: "A day has passed already.\nIs there any way for me to return home?", blockInputDefaultDuration: true),
                    new HintMessage(text: "Right now, there are more important matters.\nLike | food, | shelter...", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.TentMedium) }, blockInputDefaultDuration: true)};

                case PieceHint.Type.CineDay2:
                    return new List<HintMessage> {
                    new HintMessage(text: "Another day has passed.", blockInputDefaultDuration: true),
                    new HintMessage(text: "I guess I'm gonna | live here for a while...", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.TentMedium) }, blockInputDefaultDuration: true),
                    };

                case PieceHint.Type.CineDay3:
                    return new List<HintMessage> {
                    new HintMessage(text: "What day is it?", blockInputDefaultDuration: true),
                    new HintMessage(text: "I've lost the track of time already...", blockInputDefaultDuration: true),
                    new HintMessage(text: "Well, what matters is that I can | survive.", imageList: new List<Texture2D>{ PieceInfo.GetTexture(PieceTemplate.Name.AxeIron) }, blockInputDefaultDuration: true),
                    };

                default:
                    throw new ArgumentException($"Unsupported type - {type}.");
            }
        }

        private static void CheckData(List<PieceHint> pieceHintList)
        {
            foreach (PieceHint.Type type in PieceHint.allTypes)
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