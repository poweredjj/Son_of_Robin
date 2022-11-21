using System.Collections.Generic;

namespace SonOfRobin
{
    public class CraftData
    {
        private const float fatigueItemSmall = 150f;
        private const float fatigueItemMedium = 200f;
        private const float fatigueItemBig = 300f;
        private const float fatigueStructureSmall = 350f;
        private const float fatigueStructureMedium = 600f;
        private const float fatigueStructureBig = 700f;

        public static List<Craft.Recipe> GetFieldRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopEssential, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogRegular, 6 } }, fatigue: fatigueStructureSmall, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.WorkshopBasic }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopBasic, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 20 }, { PieceTemplate.Name.WoodLogHard, 4 }, { PieceTemplate.Name.Stone, 5 }, { PieceTemplate.Name.Granite, 2 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.WorkshopAdvanced, PieceTemplate.Name.Furnace, PieceTemplate.Name.HotPlate, PieceTemplate.Name.WorkshopLeatherBasic }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopAdvanced, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 12 },  { PieceTemplate.Name.Nail, 30 },  { PieceTemplate.Name.IronPlate, 2 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.WorkshopMaster, PieceTemplate.Name.WorkshopAlchemy, PieceTemplate.Name.WorkshopLeatherAdvanced }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopMaster, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 24 },  { PieceTemplate.Name.Nail, 50 }, { PieceTemplate.Name.IronPlate, 3 }, { PieceTemplate.Name.Clay, 3 }, { PieceTemplate.Name.IronRod, 3 }, { PieceTemplate.Name.EmptyBottle, 2 }, { PieceTemplate.Name.Leather, 5 } }, fatigue: fatigueStructureMedium, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopAlchemy, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 4 }, { PieceTemplate.Name.Granite, 2 }, { PieceTemplate.Name.WoodPlank, 4 }, { PieceTemplate.Name.Clay, 1 }, { PieceTemplate.Name.IronPlate, 3 }, { PieceTemplate.Name.EmptyBottle, 2 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.UpgradeBench, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 12 },  { PieceTemplate.Name.Nail, 10 },  { PieceTemplate.Name.IronPlate, 1 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopLeatherBasic, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 12 },  { PieceTemplate.Name.WoodLogHard, 6 }, { PieceTemplate.Name.Leather, 1 }, { PieceTemplate.Name.Clay, 3 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WorkshopLeatherAdvanced, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 6 },  { PieceTemplate.Name.WoodLogHard, 3 }, { PieceTemplate.Name.Leather, 4 }, { PieceTemplate.Name.IronPlate, 2 }, { PieceTemplate.Name.Nail, 10 }, { PieceTemplate.Name.IronRod, 3 }, }, fatigue: fatigueStructureMedium, isReversible: true, isHidden: true ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.Campfire, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 8 }}, fatigue: fatigueStructureSmall, isReversible: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.TentSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 20 }, { PieceTemplate.Name.WoodLogRegular, 4 }}, fatigue: fatigueStructureSmall,  isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.TentMedium }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.TentMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 8 }, { PieceTemplate.Name.Stick, 30 }, { PieceTemplate.Name.Rope, 10 }, { PieceTemplate.Name.Granite, 4 }}, fatigue: fatigueStructureMedium, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.TentBig }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.TentBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 16 }, { PieceTemplate.Name.Rope, 15 }, { PieceTemplate.Name.Stick, 50 }, { PieceTemplate.Name.Nail, 100 }, { PieceTemplate.Name.WoodPlank, 20 }, { PieceTemplate.Name.WoodLogHard, 4 }}, fatigue: fatigueStructureBig, durationMultiplier: 0.4f, fatigueMultiplier: 0.6f, maxLevel: 1, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ChestWooden, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodPlank, 20 },  { PieceTemplate.Name.WoodLogHard, 2 }}, fatigue: fatigueStructureSmall, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ChestStone }, craftCountToUnlock: 2),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ChestStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Granite, 3 },{ PieceTemplate.Name.WoodPlank, 10 }, { PieceTemplate.Name.Clay, 3 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ChestIron }, craftCountToUnlock: 2),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ChestIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronPlate, 2 },{ PieceTemplate.Name.WoodPlank, 4 }, { PieceTemplate.Name.Nail, 10 } }, fatigue: fatigueStructureMedium, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.Furnace, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stone, 20 }, { PieceTemplate.Name.Granite, 6 }, { PieceTemplate.Name.WoodPlank, 16 }, { PieceTemplate.Name.Clay, 2 }}, fatigue: fatigueStructureMedium, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.Anvil }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.Anvil, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 3 }, { PieceTemplate.Name.Granite, 3 }, { PieceTemplate.Name.WoodLogHard, 3 }}, fatigue: fatigueItemMedium, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.HotPlate, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Granite, 2 }, { PieceTemplate.Name.Stone, 6 } }, fatigue: fatigueStructureSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.CookingPot }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.CookingPot, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 3 }, { PieceTemplate.Name.IronPlate, 3 } }, fatigue: fatigueStructureMedium, isReversible: true, isHidden: true),
                };
        }

        public static List<Craft.Recipe> GetEssentialWorkshopRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.AxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }}, fatigue: fatigueItemSmall, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeStone }, craftCountToUnlock: 3),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PickaxeWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 2 }}, fatigue: fatigueItemSmall, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeStone }, craftCountToUnlock: 3),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.SpearWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLogRegular, 4 }}, fatigue: fatigueItemSmall, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearStone }, craftCountToUnlock: 3),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BowWood, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.Rope, 1 }}, fatigue: fatigueItemMedium, isReversible: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ArrowWood, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.ArrowStone }, craftCountToUnlock: 3),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.TorchSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 1 }, { PieceTemplate.Name.Fat, 1 }}, fatigue: fatigueItemSmall),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.WoodPlank, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogRegular, 1 }}, fatigue: 40) };
        }

        public static List<Craft.Recipe> GetBasicWorkshopRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.AxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, fatigue: fatigueItemSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.AxeIron }, craftCountToUnlock: 2),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PickaxeStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, fatigue: fatigueItemSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeIron }, craftCountToUnlock: 2),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ScytheStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, fatigue: fatigueItemMedium, isReversible: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ScytheIron }, craftCountToUnlock: 2 ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.SpearStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLogRegular, 1 }, { PieceTemplate.Name.Granite, 1 }}, fatigue: fatigueItemSmall, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearIron }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ShovelStone, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.Granite, 1 }}, fatigue: fatigueItemMedium, isReversible: true, isHidden: false, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelIron }, craftCountToUnlock: 2),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ArrowStone, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.Stone, 2 }}, fatigue: fatigueItemSmall, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.ArrowIron }, craftCountToUnlock: 3),
                };
        }

        public static List<Craft.Recipe> GetAdvancedWorkshopRecipes()
        {
            return new List<Craft.Recipe> {

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.AxeIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronRod, 1 }}, fatigue: fatigueItemMedium, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.AxeCrystal }, craftCountToUnlock: 2),

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PickaxeIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronRod, 1 }}, fatigue: fatigueItemMedium, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PickaxeCrystal }, craftCountToUnlock: 2),

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ScytheIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronRod, 1 }}, fatigue: fatigueItemMedium, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ScytheCrystal } , craftCountToUnlock: 2),

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.SpearIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.WoodLogHard, 2 }, { PieceTemplate.Name.IronRod, 1 }}, fatigue: fatigueItemMedium, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.SpearCrystal }, craftCountToUnlock: 2),

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ShovelIron, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.IronPlate, 2 }}, fatigue: fatigueItemMedium, isReversible: true, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ShovelCrystal }, craftCountToUnlock: 2),

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.TorchBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.WoodLogHard, 1 }, { PieceTemplate.Name.BottleOfOil, 1 }}, fatigue: fatigueItemBig, isHidden: true),

                    new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ArrowIron, amountToCreate: 10, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 6 }, { PieceTemplate.Name.IronRod, 1 }}, fatigue: fatigueItemMedium, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.ArrowCrystal}, craftCountToUnlock: 3),
                };
        }

        public static List<Craft.Recipe> GetMasterWorkshopRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.AxeCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Nail, 5 }, { PieceTemplate.Name.Crystal, 2 }}, fatigue: fatigueItemBig, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PickaxeCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Nail, 5 }, { PieceTemplate.Name.Crystal, 2 }}, fatigue: fatigueItemBig, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.SpearCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 3 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Nail, 5 }, { PieceTemplate.Name.Crystal, 2 }}, fatigue: fatigueItemBig, isReversible: false, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ShovelCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Crystal, 1 }}, fatigue: fatigueItemBig, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ScytheCrystal, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Stick, 2 }, { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Crystal, 2 }}, fatigue: fatigueItemBig, isReversible: true, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.ArrowCrystal, amountToCreate: 5, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronRod, 1 }, { PieceTemplate.Name.Crystal, 1 }}, fatigue: fatigueItemBig, isReversible: false, isHidden: true),
                };
        }

        public static List<Craft.Recipe> GetAlchemyRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionHealing, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Tomato, 4 }, { PieceTemplate.Name.HerbsRed, 3 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PotionStrength }, craftCountToUnlock: 3 ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionMaxHPIncrease, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Apple, 2 }, { PieceTemplate.Name.HerbsGreen, 3 }}, fatigue: fatigueItemSmall, isReversible: false),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionStrength, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Banana, 2 }, { PieceTemplate.Name.HerbsYellow, 3 }}, fatigue: fatigueItemSmall, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PotionHaste }, craftCountToUnlock: 3 ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionHaste, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Acorn, 2 }, { PieceTemplate.Name.HerbsCyan, 3 }}, fatigue: fatigueItemSmall, isReversible: false, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionMaxStamina, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Apple, 2 }, { PieceTemplate.Name.HerbsBlue, 2 }}, fatigue: fatigueItemSmall, isReversible: false,  unlocksWhenCrafted: new List<PieceTemplate.Name> {PieceTemplate.Name.PotionFatigue }, craftCountToUnlock: 3 ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionFatigue, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Cherry, 2 }, { PieceTemplate.Name.HerbsViolet, 2 }}, fatigue: fatigueItemSmall, isReversible: false, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BottleOfOil, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.Fat, 2 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.TorchBig }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionPoison, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.HerbsBlack, 2 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.UpgradeBench }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionSlowdown, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.HerbsBlack, 1 }, { PieceTemplate.Name.HerbsYellow, 2 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.UpgradeBench }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionWeakness, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.HerbsBlack, 1 }, { PieceTemplate.Name.HerbsYellow, 2 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.UpgradeBench }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.PotionMaxHPDecrease, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.EmptyBottle, 1 }, { PieceTemplate.Name.HerbsBlack, 1 }, { PieceTemplate.Name.HerbsGreen, 2 }}, fatigue: fatigueItemSmall, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.UpgradeBench }),
                };
        }

        public static List<Craft.Recipe> GetFurnaceRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.IronBar, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronOre, 1 }, { PieceTemplate.Name.Coal, 1 }}, fatigue: fatigueItemMedium, isReversible: false),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.EmptyBottle, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.GlassSand, 3 }, { PieceTemplate.Name.Coal, 1 }}, fatigue: fatigueItemMedium, isReversible: false),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.EmptyBottle, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.GlassSand, 8 }, { PieceTemplate.Name.Coal, 2 }}, fatigue: fatigueItemBig, isReversible: false),
                };
        }

        public static List<Craft.Recipe> GetAnvilRecipes()
        {
            return new List<Craft.Recipe> {

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.IronRod, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 1 } }, fatigue: fatigueItemMedium, isReversible: false),
                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.IronPlate, amountToCreate: 3, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronBar, 1 } }, fatigue: fatigueItemMedium, isReversible: false),
                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.Nail, amountToCreate: 10, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.IronRod, 1 }}, fatigue: fatigueItemMedium),
                };
        }

        public static List<Craft.Recipe> GetBasicLeatherRecipes()
        {
            return new List<Craft.Recipe>
            {
                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.Rope, amountToCreate: 2, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 1 }}, fatigue: fatigueItemMedium, isReversible: false ),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.Map, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 1 }}, fatigue: fatigueItemMedium, isReversible: false),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.HatSimple, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 2 }, { PieceTemplate.Name.Rope, 1 }}, fatigue: fatigueItemMedium, isReversible: false),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BackpackSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 4 }, { PieceTemplate.Name.Rope, 2 }}, fatigue: fatigueItemBig, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.BackpackMedium }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BeltSmall, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 1 },{ PieceTemplate.Name.Rope, 1 }}, fatigue: fatigueItemMedium, isReversible: false, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.BeltMedium }),
            };
        }

        public static List<Craft.Recipe> GetAdvancedLeatherRecipes()
        {
            return new List<Craft.Recipe>
            {
                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BootsProtective, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 3 }, { PieceTemplate.Name.Rope, 2 }, { PieceTemplate.Name.Nail, 10 }, { PieceTemplate.Name.IronPlate, 1 }}, fatigue: fatigueItemMedium, isReversible: false),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BackpackMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 5 }, { PieceTemplate.Name.Rope, 4 }, { PieceTemplate.Name.Nail, 10 }}, fatigue: fatigueItemMedium, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.BackpackBig }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BackpackBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 8 }, { PieceTemplate.Name.Rope, 6 }, { PieceTemplate.Name.Nail, 20 }, { PieceTemplate.Name.IronRod, 2 }}, fatigue: fatigueItemBig, isReversible: false, isHidden: true),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BeltMedium, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 2 },{ PieceTemplate.Name.Rope, 2 }}, fatigue: fatigueItemMedium, isReversible: false, isHidden: true, unlocksWhenCrafted: new List<PieceTemplate.Name> { PieceTemplate.Name.BeltBig }),

                new Craft.Recipe(pieceToCreate: PieceTemplate.Name.BeltBig, ingredients: new Dictionary<PieceTemplate.Name, byte> { { PieceTemplate.Name.Leather, 3 },{ PieceTemplate.Name.Rope, 3 }, { PieceTemplate.Name.Nail, 15 }, { PieceTemplate.Name.BottleOfOil, 1 }}, fatigue: fatigueItemBig, isReversible: false, isHidden: true),
            };
        }

    }
}
