using System;

namespace SonOfRobin
{
    public class ReadOnlyParams
    {
        public bool Serialize { get; private set; }
        public float FireAffinity { get; private set; }


        private ReadOnlyParams()
        {
            this.Serialize = true;
            this.FireAffinity = 0.0f;
        }

        public static ReadOnlyParams GetParamsForName(PieceTemplate.Name name)
        {
            ReadOnlyParams readOnlyParams = new ReadOnlyParams();

            switch (name)
            {
                case PieceTemplate.Name.Empty:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.PlayerBoy:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.PlayerGirl:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.PlayerTestDemoness:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.PlayerGhost:
                    readOnlyParams.FireAffinity = 0f;
                    break;

                case PieceTemplate.Name.GrassRegular:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.GrassGlow:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.GrassDesert:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.PlantPoison:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.Rushes:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.WaterLily:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.FlowersPlain:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.FlowersRed:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.FlowersMountain:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.Cactus:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.SeedsGeneric:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.CoffeeRaw:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.CoffeeRoasted:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.TreeSmall:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.TreeBig:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.PalmTree:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.Oak:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.AppleTree:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.CherryTree:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.BananaTree:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.CarrotPlant:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.TomatoPlant:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.CoffeeShrub:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.Apple:
                    readOnlyParams.FireAffinity = 0.15f;
                    break;

                case PieceTemplate.Name.Banana:
                    readOnlyParams.FireAffinity = 0.15f;
                    break;

                case PieceTemplate.Name.Cherry:
                    readOnlyParams.FireAffinity = 0.15f;
                    break;

                case PieceTemplate.Name.Tomato:
                    readOnlyParams.FireAffinity = 0.15f;
                    break;

                case PieceTemplate.Name.Carrot:
                    readOnlyParams.FireAffinity = 0.15f;
                    break;

                case PieceTemplate.Name.Acorn:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.MeatRawRegular:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.MeatDried:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.Fat:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.Leather:
                    readOnlyParams.FireAffinity = 0.7f;
                    break;

                case PieceTemplate.Name.Burger:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.Meal:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.Rabbit:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.Fox:
                    readOnlyParams.FireAffinity = 0.65f;
                    break;

                case PieceTemplate.Name.Tiger:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.Frog:
                    readOnlyParams.FireAffinity = 0.15f;
                    break;

                case PieceTemplate.Name.MineralsBig:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.MineralsSmall:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.MineralsMossyBig:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.MineralsMossySmall:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.JarTreasure:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.JarBroken:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.CrateStarting:
                    readOnlyParams.FireAffinity = 0.7f;
                    break;

                case PieceTemplate.Name.CrateRegular:
                    readOnlyParams.FireAffinity = 0.7f;
                    break;

                case PieceTemplate.Name.ChestWooden:
                    readOnlyParams.FireAffinity = 1f;
                    break;

                case PieceTemplate.Name.ChestStone:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.ChestIron:
                    readOnlyParams.FireAffinity = 0f;
                    break;

                case PieceTemplate.Name.ChestCrystal:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.ChestTreasureNormal:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.ChestTreasureBig:
                    readOnlyParams.FireAffinity = 0f;
                    break;

                case PieceTemplate.Name.Campfire:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.WorkshopEssential:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.WorkshopBasic:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.WorkshopAdvanced:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.WorkshopMaster:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.WorkshopLeatherBasic:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.WorkshopLeatherAdvanced:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.AlchemyLabStandard:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.AlchemyLabAdvanced:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.Furnace:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Anvil:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.HotPlate:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.CookingPot:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.UpgradeBench:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.Stick:
                    readOnlyParams.FireAffinity = 1.0f;
                    break;

                case PieceTemplate.Name.WoodLogRegular:
                    readOnlyParams.FireAffinity = 1.0f;
                    break;

                case PieceTemplate.Name.WoodLogHard:
                    readOnlyParams.FireAffinity = 0.9f;
                    break;

                case PieceTemplate.Name.WoodPlank:
                    readOnlyParams.FireAffinity = 1f;
                    break;

                case PieceTemplate.Name.Stone:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Granite:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Clay:
                    readOnlyParams.FireAffinity = 0.1f;
                    break;

                case PieceTemplate.Name.Rope:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.Clam:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.CoalDeposit:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.IronDeposit:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BeachDigSite:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.ForestDigSite:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DesertDigSite:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.GlassDigSite:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SwampDigSite:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.CrystalDepositSmall:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.CrystalDepositBig:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Coal:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.IronOre:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.IronBar:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.IronRod:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.IronNail:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.IronPlate:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.GlassSand:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Crystal:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Backlight:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BloodSplatter:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Attack:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Miss:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Zzz:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Heart:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.MapMarker:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.MusicNote:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Crosshair:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BubbleExclamationRed:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BubbleExclamationBlue:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BubbleCraftGreen:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.RainDrop:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Explosion:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BurningFlame:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.CookingTrigger:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.UpgradeTrigger:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BrewTrigger:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.FireplaceTriggerOn:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.FireplaceTriggerOff:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.KnifeSimple:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.AxeWood:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.AxeStone:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.AxeIron:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.AxeCrystal:
                    readOnlyParams.FireAffinity = 0.1f;
                    break;

                case PieceTemplate.Name.PickaxeWood:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.PickaxeStone:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.PickaxeIron:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.PickaxeCrystal:
                    readOnlyParams.FireAffinity = 0.1f;
                    break;

                case PieceTemplate.Name.SpearWood:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.SpearStone:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.SpearIron:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.SpearCrystal:
                    readOnlyParams.FireAffinity = 0.1f;
                    break;

                case PieceTemplate.Name.ScytheStone:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.ScytheIron:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.ScytheCrystal:
                    readOnlyParams.FireAffinity = 0.1f;
                    break;

                case PieceTemplate.Name.ShovelStone:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.ShovelIron:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.ShovelCrystal:
                    readOnlyParams.FireAffinity = 0.1f;
                    break;

                case PieceTemplate.Name.BowBasic:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.BowAdvanced:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.ArrowWood:
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.ArrowStone:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.ArrowIron:
                    readOnlyParams.FireAffinity = 0.7f;
                    break;

                case PieceTemplate.Name.ArrowCrystal:
                    readOnlyParams.FireAffinity = 0.4f;
                    break;

                case PieceTemplate.Name.ArrowBurning:
                    readOnlyParams.FireAffinity = 0.7f;
                    break;

                case PieceTemplate.Name.DebrisPlant:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisStone:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisWood:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisLeaf:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisCrystal:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisCeramic:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisStar:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisSoot:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.DebrisHeart:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BloodDrop:
                    readOnlyParams.FireAffinity = 0f;
                    break;

                case PieceTemplate.Name.TentSmall:
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.TentMedium:
                    readOnlyParams.FireAffinity = 0.7f;
                    break;

                case PieceTemplate.Name.TentBig:
                    readOnlyParams.FireAffinity = 0.6f;
                    break;

                case PieceTemplate.Name.BackpackSmall:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.BackpackMedium:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.BackpackBig:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.BeltSmall:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.BeltMedium:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.BeltBig:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.Map:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.Dungarees:
                    readOnlyParams.FireAffinity = 0.5f;
                    break;

                case PieceTemplate.Name.HatSimple:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.BootsProtective:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.TorchSmall:
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.TorchBig:
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.LanternEmpty:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.LanternFull:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.Candle:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HumanSkeleton:
                    readOnlyParams.FireAffinity = 0.3f;
                    break;

                case PieceTemplate.Name.PredatorRepellant:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.HerbsBlack:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HerbsBlue:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HerbsCyan:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HerbsGreen:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HerbsYellow:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HerbsRed:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.HerbsViolet:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.EmptyBottle:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.PotionGeneric:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.PotionCoffee:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.BottleOfOil:
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.Hole:
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.TreeStump:
                    readOnlyParams.FireAffinity = 0.8f;
                    break;

                case PieceTemplate.Name.LavaFlame:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SwampGas:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.LavaGas:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 1;
                    break;

                case PieceTemplate.Name.SoundSeaWavesObsolete:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SoundLakeWaves:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SoundSeaWind:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SoundNightCrickets:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SoundNoonCicadas:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SoundLava:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.SeaWave:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                case PieceTemplate.Name.MeatRawPrime:
                    readOnlyParams.FireAffinity = 0.2f;
                    break;

                case PieceTemplate.Name.ParticleEmitter:
                    readOnlyParams.Serialize = false;
                    readOnlyParams.FireAffinity = 0;
                    break;

                default:
                    throw new ArgumentException($"Unsupported name - {name}.");
            }

            return readOnlyParams;
        }
    }
}