using System;

namespace SonOfRobin
{
    public class Description
    {
        public static string GetName(PieceTemplate.Name name)
        {
            switch (name)
            {
                case PieceTemplate.Name.GrassRegular:
                    return "Regular grass";
                case PieceTemplate.Name.GrassDesert:
                    return "Desert grass";
                case PieceTemplate.Name.WaterLily:
                    return "Water lily";
                case PieceTemplate.Name.FlowersPlain:
                    return "Regular flower";
                case PieceTemplate.Name.FlowersMountain:
                    return "Mountain flower";
                case PieceTemplate.Name.TreeSmall:
                    return "Small tree";
                case PieceTemplate.Name.TreeBig:
                    return "Big tree";
                case PieceTemplate.Name.PalmTree:
                    return "Palm tree";
                case PieceTemplate.Name.AppleTree:
                    return "Apple tree";
                case PieceTemplate.Name.CherryTree:
                    return "Cherry tree";
                case PieceTemplate.Name.BananaTree:
                    return "Banana tree";
                case PieceTemplate.Name.TomatoPlant:
                    return "Tomato plant";
                case PieceTemplate.Name.RawMeat:
                    return "Raw meat";
                case PieceTemplate.Name.CookedMeat:
                    return "Cooked meat";
                case PieceTemplate.Name.Meal:
                    return "Cooked meal";
                case PieceTemplate.Name.WaterRock:
                    return "Water rock";
                case PieceTemplate.Name.MineralsSmall:
                    return "Small minerals";
                case PieceTemplate.Name.MineralsBig:
                    return "Big minerals";
                case PieceTemplate.Name.ChestWooden:
                    return "Wooden chest";
                case PieceTemplate.Name.ChestIron:
                    return "Iron chest";
                case PieceTemplate.Name.CrateStarting:
                    return "Supply crate";
                case PieceTemplate.Name.CrateRegular:
                    return "Supply crate";
                case PieceTemplate.Name.RegularWorkshop:
                    return "Crafting workshop";
                case PieceTemplate.Name.CookingPot:
                    return "Cooking pot";
                case PieceTemplate.Name.WoodLog:
                    return "Wood log";
                case PieceTemplate.Name.WoodPlank:
                    return "Wood plank";
                case PieceTemplate.Name.CoalDeposit:
                    return "Coal deposit";
                case PieceTemplate.Name.IronDeposit:
                    return "Iron deposit";
                case PieceTemplate.Name.IronOre:
                    return "Iron ore";
                case PieceTemplate.Name.IronBar:
                    return "Iron bar";
                case PieceTemplate.Name.FlameRegular:
                    return "Flame";
                case PieceTemplate.Name.AxeWood:
                    return "Wooden axe";
                case PieceTemplate.Name.AxeStone:
                    return "Stone axe";
                case PieceTemplate.Name.AxeIron:
                    return "Iron axe";
                case PieceTemplate.Name.PickaxeWood:
                    return "Wooden pickaxe";
                case PieceTemplate.Name.PickaxeStone:
                    return "Stone pickaxe"; ;
                case PieceTemplate.Name.PickaxeIron:
                    return "Iron pickaxe";
                case PieceTemplate.Name.BatWood:
                    return "Wooden bat";
                case PieceTemplate.Name.GreatSling:
                    return "Great sling";
                case PieceTemplate.Name.BowWood:
                    return "Wooden bow";
                case PieceTemplate.Name.StoneAmmo:
                    return "Small stone";
                case PieceTemplate.Name.ArrowWood:
                    return "Wooden arrow";
                case PieceTemplate.Name.ArrowIron:
                    return "Iron arrow";
                case PieceTemplate.Name.DebrisStone:
                    return "Stone debris";
                case PieceTemplate.Name.DebrisWood:
                    return "Wooden debris";
                case PieceTemplate.Name.BloodDrop:
                    return "Drop of blood";
                case PieceTemplate.Name.TentSmall:
                    return "Small tent";
                case PieceTemplate.Name.TentMedium:
                    return "Medium tent";
                case PieceTemplate.Name.TentBig:
                    return "Big tent";
                case PieceTemplate.Name.BackpackMedium:
                    return "Medium backpack";
                case PieceTemplate.Name.BeltMedium:
                    return "Medium belt";
                default:
                    return Convert.ToString(name);
            }
        }

        public static string GetDescription(PieceTemplate.Name name)
        {
            switch (name)
            {
                case PieceTemplate.Name.Apple:
                    return "Fruit, can be eaten.";
                case PieceTemplate.Name.Banana:
                    return "Fruit, can be eaten.";
                case PieceTemplate.Name.Cherry:
                    return "Fruit, can be eaten.";
                case PieceTemplate.Name.Tomato:
                    return "Vegetable, can be eaten.";
                case PieceTemplate.Name.RawMeat:
                    return "Can be eaten.";
                case PieceTemplate.Name.Leather:
                    return "Crafting material.";
                case PieceTemplate.Name.CookedMeat:
                    return "Can be eaten.";
                case PieceTemplate.Name.Meal:
                    return "Cooked meal. Can be eaten.";
                case PieceTemplate.Name.ChestWooden:
                    return "Can store items.";
                case PieceTemplate.Name.ChestIron:
                    return "Can store items.";
                case PieceTemplate.Name.RegularWorkshop:
                    return "For advanced item crafting.";
                case PieceTemplate.Name.Furnace:
                    return "For ore smelting.";
                case PieceTemplate.Name.CookingPot:
                    return "For cooking.";
                case PieceTemplate.Name.Stick:
                    return "Crafting material.";
                case PieceTemplate.Name.Stone:
                    return "Crafting material.";
                case PieceTemplate.Name.WoodLog:
                    return "Crafting material and fuel.";
                case PieceTemplate.Name.WoodPlank:
                    return "Crafting material and fuel.";
                case PieceTemplate.Name.Nail:
                    return "Crafting material.";
                case PieceTemplate.Name.Shell:
                    return "Totally useless.";
                case PieceTemplate.Name.Coal:
                    return "Crafting material and fuel.";
                case PieceTemplate.Name.IronOre:
                    return "Can be used to make iron bars.";
                case PieceTemplate.Name.IronBar:
                    return "Crafting material.";
                case PieceTemplate.Name.AxeWood:
                    break;
                case PieceTemplate.Name.AxeStone:
                    break;
                case PieceTemplate.Name.AxeIron:
                    break;
                case PieceTemplate.Name.PickaxeWood:
                    break;
                case PieceTemplate.Name.PickaxeStone:
                    break;
                case PieceTemplate.Name.PickaxeIron:
                    break;
                case PieceTemplate.Name.BatWood:
                    break;
                case PieceTemplate.Name.Sling:
                    break;
                case PieceTemplate.Name.GreatSling:
                    break;
                case PieceTemplate.Name.BowWood:
                    break;
                case PieceTemplate.Name.StoneAmmo:
                    break;
                case PieceTemplate.Name.ArrowWood:
                    break;
                case PieceTemplate.Name.ArrowIron:
                    break;
                case PieceTemplate.Name.DebrisStone:
                    break;
                case PieceTemplate.Name.DebrisWood:
                    break;
                case PieceTemplate.Name.BloodDrop:
                    break;
                case PieceTemplate.Name.TentSmall:
                    break;
                case PieceTemplate.Name.TentMedium:
                    break;
                case PieceTemplate.Name.TentBig:
                    break;
                case PieceTemplate.Name.BackpackMedium:
                    break;
                case PieceTemplate.Name.BeltMedium:
                    break;
                case PieceTemplate.Name.Map:
                    break;
                default:
                    return null;
            }


            return "This description is not written yet."; // TODO add all descriptions
        }


    }
}
