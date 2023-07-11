using System;

namespace SonOfRobin
{
    public class ReadOnlyParams
    {
        public bool Serialize { get; private set; }

        private ReadOnlyParams()
        {
            this.Serialize = true;
        }

        public static ReadOnlyParams GetParamsForName(PieceTemplate.Name name)
        {
            ReadOnlyParams readOnlyParams = new ReadOnlyParams();

            switch (name)
            {
                case PieceTemplate.Name.Empty:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.PlayerBoy:
                    break;

                case PieceTemplate.Name.PlayerGirl:
                    break;

                case PieceTemplate.Name.PlayerTestDemoness:
                    break;

                case PieceTemplate.Name.PlayerGhost:
                    break;

                case PieceTemplate.Name.GrassRegular:
                    break;

                case PieceTemplate.Name.GrassGlow:
                    break;

                case PieceTemplate.Name.GrassDesert:
                    break;

                case PieceTemplate.Name.PlantPoison:
                    break;

                case PieceTemplate.Name.Rushes:
                    break;

                case PieceTemplate.Name.WaterLily:
                    break;

                case PieceTemplate.Name.FlowersPlain:
                    break;

                case PieceTemplate.Name.FlowersRed:
                    break;

                case PieceTemplate.Name.FlowersMountain:
                    break;

                case PieceTemplate.Name.Cactus:
                    break;

                case PieceTemplate.Name.SeedsGeneric:
                    break;

                case PieceTemplate.Name.CoffeeRaw:
                    break;

                case PieceTemplate.Name.CoffeeRoasted:
                    break;

                case PieceTemplate.Name.TreeSmall:
                    break;

                case PieceTemplate.Name.TreeBig:
                    break;

                case PieceTemplate.Name.PalmTree:
                    break;

                case PieceTemplate.Name.Oak:
                    break;

                case PieceTemplate.Name.AppleTree:
                    break;

                case PieceTemplate.Name.CherryTree:
                    break;

                case PieceTemplate.Name.BananaTree:
                    break;

                case PieceTemplate.Name.CarrotPlant:
                    break;

                case PieceTemplate.Name.TomatoPlant:
                    break;

                case PieceTemplate.Name.CoffeeShrub:
                    break;

                case PieceTemplate.Name.Apple:
                    break;

                case PieceTemplate.Name.Banana:
                    break;

                case PieceTemplate.Name.Cherry:
                    break;

                case PieceTemplate.Name.Tomato:
                    break;

                case PieceTemplate.Name.Carrot:
                    break;

                case PieceTemplate.Name.Acorn:
                    break;

                case PieceTemplate.Name.MeatRawRegular:
                    break;

                case PieceTemplate.Name.MeatDried:
                    break;

                case PieceTemplate.Name.Fat:
                    break;

                case PieceTemplate.Name.Leather:
                    break;

                case PieceTemplate.Name.Burger:
                    break;

                case PieceTemplate.Name.Meal:
                    break;

                case PieceTemplate.Name.Rabbit:
                    break;

                case PieceTemplate.Name.Fox:
                    break;

                case PieceTemplate.Name.Tiger:
                    break;

                case PieceTemplate.Name.Frog:
                    break;

                case PieceTemplate.Name.MineralsBig:
                    break;

                case PieceTemplate.Name.MineralsSmall:
                    break;

                case PieceTemplate.Name.MineralsMossyBig:
                    break;

                case PieceTemplate.Name.MineralsMossySmall:
                    break;

                case PieceTemplate.Name.JarTreasure:
                    break;

                case PieceTemplate.Name.JarBroken:
                    break;

                case PieceTemplate.Name.CrateStarting:
                    break;

                case PieceTemplate.Name.CrateRegular:
                    break;

                case PieceTemplate.Name.ChestWooden:
                    break;

                case PieceTemplate.Name.ChestStone:
                    break;

                case PieceTemplate.Name.ChestIron:
                    break;

                case PieceTemplate.Name.ChestCrystal:
                    break;

                case PieceTemplate.Name.ChestTreasureNormal:
                    break;

                case PieceTemplate.Name.ChestTreasureBig:
                    break;

                case PieceTemplate.Name.Campfire:
                    break;

                case PieceTemplate.Name.WorkshopEssential:
                    break;

                case PieceTemplate.Name.WorkshopBasic:
                    break;

                case PieceTemplate.Name.WorkshopAdvanced:
                    break;

                case PieceTemplate.Name.WorkshopMaster:
                    break;

                case PieceTemplate.Name.WorkshopLeatherBasic:
                    break;

                case PieceTemplate.Name.WorkshopLeatherAdvanced:
                    break;

                case PieceTemplate.Name.AlchemyLabStandard:
                    break;

                case PieceTemplate.Name.AlchemyLabAdvanced:
                    break;

                case PieceTemplate.Name.Furnace:
                    break;

                case PieceTemplate.Name.Anvil:
                    break;

                case PieceTemplate.Name.HotPlate:
                    break;

                case PieceTemplate.Name.CookingPot:
                    break;

                case PieceTemplate.Name.UpgradeBench:
                    break;

                case PieceTemplate.Name.Stick:
                    break;

                case PieceTemplate.Name.WoodLogRegular:
                    break;

                case PieceTemplate.Name.WoodLogHard:
                    break;

                case PieceTemplate.Name.WoodPlank:
                    break;

                case PieceTemplate.Name.Stone:
                    break;

                case PieceTemplate.Name.Granite:
                    break;

                case PieceTemplate.Name.Clay:
                    break;

                case PieceTemplate.Name.Rope:
                    break;

                case PieceTemplate.Name.Clam:
                    break;

                case PieceTemplate.Name.CoalDeposit:
                    break;

                case PieceTemplate.Name.IronDeposit:
                    break;

                case PieceTemplate.Name.BeachDigSite:
                    break;

                case PieceTemplate.Name.ForestDigSite:
                    break;

                case PieceTemplate.Name.DesertDigSite:
                    break;

                case PieceTemplate.Name.GlassDigSite:
                    break;

                case PieceTemplate.Name.SwampDigSite:
                    break;

                case PieceTemplate.Name.CrystalDepositSmall:
                    break;

                case PieceTemplate.Name.CrystalDepositBig:
                    break;

                case PieceTemplate.Name.Coal:
                    break;

                case PieceTemplate.Name.IronOre:
                    break;

                case PieceTemplate.Name.IronBar:
                    break;

                case PieceTemplate.Name.IronRod:
                    break;

                case PieceTemplate.Name.IronNail:
                    break;

                case PieceTemplate.Name.IronPlate:
                    break;

                case PieceTemplate.Name.GlassSand:
                    break;

                case PieceTemplate.Name.Crystal:
                    break;

                case PieceTemplate.Name.Backlight:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.BloodSplatter:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.Attack:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.Miss:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.Zzz:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.Heart:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.MapMarker:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.MusicNote:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.Crosshair:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.BubbleExclamationRed:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.BubbleExclamationBlue:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.BubbleCraftGreen:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.RainDrop:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.Explosion:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.BurningFlame:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.CookingTrigger:
                    break;

                case PieceTemplate.Name.UpgradeTrigger:
                    break;

                case PieceTemplate.Name.BrewTrigger:
                    break;

                case PieceTemplate.Name.FireplaceTriggerOn:
                    break;

                case PieceTemplate.Name.FireplaceTriggerOff:
                    break;

                case PieceTemplate.Name.KnifeSimple:
                    break;

                case PieceTemplate.Name.AxeWood:
                    break;

                case PieceTemplate.Name.AxeStone:
                    break;

                case PieceTemplate.Name.AxeIron:
                    break;

                case PieceTemplate.Name.AxeCrystal:
                    break;

                case PieceTemplate.Name.PickaxeWood:
                    break;

                case PieceTemplate.Name.PickaxeStone:
                    break;

                case PieceTemplate.Name.PickaxeIron:
                    break;

                case PieceTemplate.Name.PickaxeCrystal:
                    break;

                case PieceTemplate.Name.SpearWood:
                    break;

                case PieceTemplate.Name.SpearStone:
                    break;

                case PieceTemplate.Name.SpearIron:
                    break;

                case PieceTemplate.Name.SpearCrystal:
                    break;

                case PieceTemplate.Name.ScytheStone:
                    break;

                case PieceTemplate.Name.ScytheIron:
                    break;

                case PieceTemplate.Name.ScytheCrystal:
                    break;

                case PieceTemplate.Name.ShovelStone:
                    break;

                case PieceTemplate.Name.ShovelIron:
                    break;

                case PieceTemplate.Name.ShovelCrystal:
                    break;

                case PieceTemplate.Name.BowBasic:
                    break;

                case PieceTemplate.Name.BowAdvanced:
                    break;

                case PieceTemplate.Name.ArrowWood:
                    break;

                case PieceTemplate.Name.ArrowStone:
                    break;

                case PieceTemplate.Name.ArrowIron:
                    break;

                case PieceTemplate.Name.ArrowCrystal:
                    break;

                case PieceTemplate.Name.ArrowBurning:
                    break;

                case PieceTemplate.Name.DebrisPlant:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisStone:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisWood:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisLeaf:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisCrystal:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisCeramic:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisStar:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisSoot:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.DebrisHeart:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.BloodDrop:
                    break;

                case PieceTemplate.Name.TentSmall:
                    break;

                case PieceTemplate.Name.TentMedium:
                    break;

                case PieceTemplate.Name.TentBig:
                    break;

                case PieceTemplate.Name.BackpackSmall:
                    break;

                case PieceTemplate.Name.BackpackMedium:
                    break;

                case PieceTemplate.Name.BackpackBig:
                    break;

                case PieceTemplate.Name.BeltSmall:
                    break;

                case PieceTemplate.Name.BeltMedium:
                    break;

                case PieceTemplate.Name.BeltBig:
                    break;

                case PieceTemplate.Name.Map:
                    break;

                case PieceTemplate.Name.Dungarees:
                    break;

                case PieceTemplate.Name.HatSimple:
                    break;

                case PieceTemplate.Name.BootsProtective:
                    break;

                case PieceTemplate.Name.TorchSmall:
                    break;

                case PieceTemplate.Name.TorchBig:
                    break;

                case PieceTemplate.Name.LanternEmpty:
                    break;

                case PieceTemplate.Name.LanternFull:
                    break;

                case PieceTemplate.Name.Candle:
                    break;

                case PieceTemplate.Name.HumanSkeleton:
                    break;

                case PieceTemplate.Name.PredatorRepellant:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.HerbsBlack:
                    break;

                case PieceTemplate.Name.HerbsBlue:
                    break;

                case PieceTemplate.Name.HerbsCyan:
                    break;

                case PieceTemplate.Name.HerbsGreen:
                    break;

                case PieceTemplate.Name.HerbsYellow:
                    break;

                case PieceTemplate.Name.HerbsRed:
                    break;

                case PieceTemplate.Name.HerbsViolet:
                    break;

                case PieceTemplate.Name.EmptyBottle:
                    break;

                case PieceTemplate.Name.PotionGeneric:
                    break;

                case PieceTemplate.Name.PotionCoffee:
                    break;

                case PieceTemplate.Name.BottleOfOil:
                    break;

                case PieceTemplate.Name.Hole:
                    break;

                case PieceTemplate.Name.TreeStump:
                    break;

                case PieceTemplate.Name.LavaFlame:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SwampGas:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.LavaGas:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SoundSeaWavesObsolete:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SoundLakeWaves:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SoundSeaWind:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SoundNightCrickets:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SoundNoonCicadas:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SoundLava:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.SeaWave:
                    readOnlyParams.Serialize = false;
                    break;

                case PieceTemplate.Name.MeatRawPrime:
                    break;

                case PieceTemplate.Name.ParticleEmitter:
                    readOnlyParams.Serialize = false;
                    break;

                default:
                    throw new ArgumentException($"Unsupported name - {name}.");
            }

            return readOnlyParams;
        }
    }
}