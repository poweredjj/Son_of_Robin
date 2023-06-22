using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class AnimData
    {
        public const float currentVersion = 1.000003f; // version number should be incremented with any change of graphics assets

        public static readonly Dictionary<string, AnimFrame> frameById = new Dictionary<string, AnimFrame>(); // needed to access frames directly by id (for loading and saving game)
        public static readonly Dictionary<string, List<AnimFrame>> frameListById = new Dictionary<string, List<AnimFrame>>();
        public static readonly Dictionary<PkgName, AnimFrame> framesForPkgs = new Dictionary<PkgName, AnimFrame>(); // default frames for packages

        public static readonly Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Object> jsonDict = new Dictionary<string, Object>();

        public enum PkgName
        {
            NoAnim,
            Empty,

            Cactus,

            FlowersWhite,
            FlowersYellow1,
            FlowersYellow2,
            FlowersRed,

            Rushes,
            GrassDesert,
            GrassRegular,
            PlantPoison,
            CoffeeShrub,
            CarrotPlant,
            TomatoPlant,

            PalmTree,
            TreeBig,
            TreeSmall1,
            TreeSmall2,

            TreeStump,

            WaterLily1,
            WaterLily2,
            WaterLily3,

            Zzz,
            Heart,

            Fog1,
            Fog2,
            Fog3,
            Fog4,
            Fog5,
            Fog6,
            Fog7,
            Fog8,

            MineralsBig1,
            MineralsBig2,
            MineralsBig3,
            MineralsBig4,
            MineralsBig5,
            MineralsBig6,
            MineralsBig7,
            MineralsBig8,
            MineralsBig9,
            MineralsBig10,
            MineralsBig11,
            MineralsBig12,
            MineralsBig13,
            MineralsBig14,

            MineralsSmall1,
            MineralsSmall2,
            MineralsSmall3,
            MineralsSmall4,

            MineralsMossyBig1,
            MineralsMossyBig2,
            MineralsMossyBig3,
            MineralsMossyBig4,
            MineralsMossyBig5,
            MineralsMossyBig6,
            MineralsMossyBig7,
            MineralsMossyBig8,
            MineralsMossyBig9,
            MineralsMossyBig10,
            MineralsMossyBig11,
            MineralsMossyBig12,

            MineralsMossySmall1,
            MineralsMossySmall2,
            MineralsMossySmall3,
            MineralsMossySmall4,

            JarWhole,
            JarBroken,

            ChestWooden,
            ChestStone,
            ChestIron,
            ChestCrystal,
            ChestTreasureBlue,
            ChestTreasureRed,

            WoodLogRegular,
            WoodLogHard,
            WoodPlank,

            Nail,
            Rope,
            Crate,

            WorkshopEssential,
            WorkshopBasic,
            WorkshopAdvanced,
            WorkshopMaster,

            AlchemyLabStandard,
            AlchemyLabAdvanced,

            WorkshopLeatherBasic,
            WorkshopLeatherAdvanced,

            UpgradeBench,

            Furnace,
            Anvil,
            HotPlate,
            CookingPot,

            Stick1,
            Stick2,
            Stick3,
            Stick4,
            Stick5,
            Stick6,
            Stick7,

            Stone,
            Granite,

            Clay,

            Apple,
            Banana,
            Cherry,
            Tomato,
            Carrot,
            Acorn,

            SeedBag,
            CoffeeRaw,
            CoffeeRoasted,

            Clam,

            MeatRawRegular,
            MeatDried,
            Fat,
            Burger,
            MealStandard,
            Leather,

            KnifeSimple,

            AxeWood,
            AxeStone,
            AxeIron,
            AxeCrystal,

            PickaxeWood,
            PickaxeStone,
            PickaxeIron,
            PickaxeCrystal,

            ScytheStone,
            ScytheIron,
            ScytheCrystal,

            SpearWood,
            SpearStone,
            SpearIron,
            SpearCrystal,

            ShovelStone,
            ShovelIron,
            ShovelCrystal,

            BowBasic,
            BowAdvanced,

            ArrowWood,
            ArrowStone,
            ArrowIron,
            ArrowCrystal,
            ArrowBurning,

            CoalDeposit,
            IronDeposit,

            CrystalDepositSmall,
            CrystalDepositBig,

            DigSite,
            DigSiteGlass,

            Coal,
            IronOre,
            IronBar,
            IronRod,
            IronPlate,
            GlassSand,
            Crystal,

            PlayerBoy,
            PlayerGirl,
            PlayerTestDemoness,

            CrabBeige,
            CrabBrown,
            CrabDarkBrown,
            CrabGray,
            CrabGreen,
            CrabLightBlue,
            CrabRed,
            CrabYellow,

            FoxBlack,
            FoxBrown,
            FoxChocolate,
            FoxGinger,
            FoxGray,
            FoxRed,
            FoxWhite,
            FoxYellow,

            Frog1,
            Frog2,
            Frog3,
            Frog4,
            Frog5,
            Frog6,
            Frog7,
            Frog8,

            RabbitBeige,
            RabbitBlack,
            RabbitBrown,
            RabbitDarkBrown,
            RabbitGray,
            RabbitLightBrown,
            RabbitLightGray,
            RabbitWhite,

            TigerOrangeMedium,
            TigerOrangeLight,
            TigerGray,
            TigerWhite,
            TigerOrangeDark,
            TigerBrown,
            TigerYellow,
            TigerBlack,

            DebrisPlant,
            DebrisStone,
            DebrisWood,
            DebrisCrystal,
            DebrisCeramic1,
            DebrisCeramic2,
            DebrisLeaf1,
            DebrisLeaf2,
            DebrisLeaf3,
            BloodDrop1,
            BloodDrop2,
            BloodDrop3,
            DebrisStar1,
            DebrisStar2,
            DebrisStar3,
            DebrisHeart1,
            DebrisHeart2,
            DebrisHeart3,
            DebrisSoot1,
            DebrisSoot2,
            DebrisSoot3,

            TentSmall,
            TentMedium,
            TentBig,

            BackpackMediumOutline,

            BackpackSmall,
            BackpackMedium,
            BackpackBig,

            BeltSmall,
            BeltMedium,
            BeltBig,
            Map,

            HatSimple,
            BootsProtective,
            Dungarees,

            LanternFrame,
            Lantern,
            Candle,

            SmallTorch,
            BigTorch,
            Campfire,

            HerbsBlack,
            HerbsCyan,
            HerbsBlue,
            HerbsGreen,
            HerbsYellow,
            HerbsRed,
            HerbsViolet,

            EmptyBottle,
            PotionRed,
            PotionBlue,
            PotionViolet,
            PotionYellow,
            PotionCyan,
            PotionGreen,
            PotionBlack,
            PotionDarkViolet,
            PotionDarkYellow,
            PotionDarkGreen,
            PotionLightYellow,
            PotionTransparent,
            PotionBrown,

            BloodSplatter1,
            BloodSplatter2,
            BloodSplatter3,

            HumanSkeleton,

            Hole,

            WhiteSpot,
            NewIcon,
            MapEdges,
            SmallWhiteCircle,
            Explosion,
            AnimalIcon,
            SkullAndBones,
            MusicNoteSmall,
            MusicNoteBig,
            Biceps,
            Bed,
            Miss,
            Attack,
            MapMarker,
            Backlight,
            Crosshair,
            Flame,
            Upgrade,
            WaterDrop,
            RainDrops,

            BubbleExclamationRed,
            BubbleExclamationBlue,
            BubbleCraftGreen,

            SeaWave,
            MeatRawPrime,
        }

        public static void AddFrameList(PkgName animPackage, byte animSize, List<AnimFrame> frameList, string animName = "default")
        {
            if (!framesForPkgs.ContainsKey(animPackage)) framesForPkgs[animPackage] = frameList[0];

            string completeAnimID = $"{animPackage}-{animSize}-{animName}";
            frameListById[completeAnimID] = new List<AnimFrame>(frameList);
        }

        // animation loading should be split in multiple methods, to avoid loading them all at once

        public static void CreateAnimsPlants()
        {
            {
                PkgName packageName = PkgName.GrassRegular;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "grass_regular_x3", layer: 1));
            }
            {
                PkgName packageName = PkgName.GrassDesert;
                AddFrameList(animPackage: packageName, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 160, y: 224, width: 24, height: 24));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 288, y: 160, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersYellow1;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersYellow2;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x",
                    layer: 0, x: 832, y: 128, width: 64, height: 64, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x",
                    layer: 1, x: 768, y: 0, width: 64, height: 64, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.FlowersRed;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 64, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersWhite;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 32, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Rushes;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 96, y: 192, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Cactus;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 352, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 288, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.PalmTree;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "palmtree_small", layer: 1, scale: 0.7f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 123, y: 326, width: 69, height: 80, crop: false));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 8, y: 145, width: 66, height: 102, crop: false));
                AddFrameList(animPackage: packageName, animSize: 3,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 108, y: 145, width: 72, height: 102, crop: false));
            }
            {
                PkgName packageName = PkgName.TreeSmall1;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sapling_short", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 192, y: 352, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 160, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeSmall2;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sapling_short", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 128, y: 192, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 160, y: 192, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeBig;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sapling_tall", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 199, y: 382, width: 47, height: 66));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 0, y: 0, width: 63, height: 97));
            }
            {
                PkgName packageName = PkgName.TomatoPlant;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_small", layer: 1, scale: 0.1f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_medium", layer: 1, scale: 0.08f));
                AddFrameList(animPackage: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_big", layer: 1, scale: 0.08f));
            }
            AddFrameList(animPackage: PkgName.Tomato, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tomato", layer: 0, scale: 0.07f));
            {
                PkgName packageName = PkgName.CoffeeShrub;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "coffee_shrub", layer: 1, scale: 0.06f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "coffee_shrub", layer: 1, scale: 0.06f));
            }
            AddFrameList(animPackage: PkgName.CarrotPlant, animName: "default", animSize: 0, frameList: ConvertImageToFrameList(atlasName: "carrot_plant_empty", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.CarrotPlant, animName: "has_fruits", animSize: 0, frameList: ConvertImageToFrameList(atlasName: "carrot_plant_has_carrot", layer: 1, scale: 0.1f)); // using different plant graphics when carrot is present, instead of drawing the carrot separately (because the carrot should be underground)
        }

        public static void CreateAnimsCharacters()
        {
            AddRPGMakerPackageV1(packageName: PkgName.PlayerBoy, atlasName: "characters/actor29rec4", setNoX: 0, setNoY: 0, animSize: 0);
            AddRPGMakerPackageV1(packageName: PkgName.PlayerGirl, atlasName: "characters/recolor_pt2", setNoX: 0, setNoY: 0, animSize: 0);
            AddRPGMakerPackageV1(packageName: PkgName.PlayerTestDemoness, atlasName: "characters/demoness", setNoX: 0, setNoY: 0, animSize: 0);

            foreach (var kvp in new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } })
            {
                string atlasName = "characters/rabbits";
                byte animSize = kvp.Key;
                float scale = kvp.Value;

                AddRPGMakerPackageV2(packageName: PkgName.RabbitBrown, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitDarkBrown, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitGray, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitBlack, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitLightGray, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitBeige, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitWhite, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.RabbitLightBrown, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: animSize, scale: scale);
            }

            foreach (var kvp in new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } })
            {
                string atlasName = "characters/fox";
                byte animSize = kvp.Key;
                float scale = kvp.Value;

                AddRPGMakerPackageV2(packageName: PkgName.FoxGinger, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxRed, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxWhite, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxGray, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxBlack, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxChocolate, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxBrown, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.FoxYellow, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: animSize, scale: scale);
            }

            foreach (var kvp in new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } })
            {
                string atlasName = "characters/tiger";
                byte animSize = kvp.Key;
                float scale = kvp.Value;

                AddRPGMakerPackageV2(packageName: PkgName.TigerOrangeMedium, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerOrangeLight, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerGray, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerWhite, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerOrangeDark, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerBrown, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerYellow, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.TigerBlack, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: animSize, scale: scale);
            }
            {
                float scale = 1f;
                string atlasName = "characters/frogs_small";
                AddRPGMakerPackageV2(packageName: PkgName.Frog1, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog2, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog3, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog4, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog5, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog6, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog7, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog8, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: 0, scale: scale);
            }

            foreach (var kvp in new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } })
            {
                string atlasName = "characters/frogs_big";
                byte animSize = kvp.Key;
                float scale = kvp.Value;

                AddRPGMakerPackageV2(packageName: PkgName.Frog1, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog2, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog3, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog4, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog5, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog6, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog7, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog8, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: animSize, scale: scale);
            }

            //{
            //    string atlasNameSmall = "characters/crabs_small";
            //    string atlasNameBig = "characters/crabs_big";

            //    AddRPGMakerPackageV2(packageName: PkgName.CrabRed, atlasName: atlasNameSmall, setNoX: 0, setNoY: 0, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabRed, atlasName: atlasNameBig, setNoX: 0, setNoY: 0, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabBrown, atlasName: atlasNameSmall, setNoX: 1, setNoY: 0, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabBrown, atlasName: atlasNameBig, setNoX: 1, setNoY: 0, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabGreen, atlasName: atlasNameSmall, setNoX: 2, setNoY: 0, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabGreen, atlasName: atlasNameBig, setNoX: 2, setNoY: 0, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabLightBlue, atlasName: atlasNameSmall, setNoX: 3, setNoY: 0, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabLightBlue, atlasName: atlasNameBig, setNoX: 3, setNoY: 0, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabGray, atlasName: atlasNameSmall, setNoX: 0, setNoY: 1, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabGray, atlasName: atlasNameBig, setNoX: 0, setNoY: 1, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabGray, atlasName: atlasNameSmall, setNoX: 1, setNoY: 1, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabYellow, atlasName: atlasNameBig, setNoX: 1, setNoY: 1, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabYellow, atlasName: atlasNameSmall, setNoX: 2, setNoY: 1, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabBeige, atlasName: atlasNameBig, setNoX: 2, setNoY: 1, animSize: 1);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabDarkBrown, atlasName: atlasNameSmall, setNoX: 3, setNoY: 1, animSize: 0, crop: true);
            //    AddRPGMakerPackageV2(packageName: PkgName.CrabDarkBrown, atlasName: atlasNameBig, setNoX: 3, setNoY: 1, animSize: 1);
            //}
        }

        public static void CreateAnimsMisc1()
        {
            AddFrameList(animPackage: PkgName.NoAnim, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "no_anim", layer: 1, x: 0, y: 0, width: 0, height: 0));

            AddFrameList(animPackage: PkgName.MineralsBig1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_1", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_2", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_3", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_4", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig5, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_5", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig6, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_6", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig7, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_7", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig8, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_8", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig9, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_9", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig10, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_10", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig11, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_11", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig12, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_12", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig13, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_13", layer: 1, scale: 0.3f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsBig14, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_14", layer: 1, scale: 0.3f, depthPercent: 0.6f));

            AddFrameList(animPackage: PkgName.MineralsSmall1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_1", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_2", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_3", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_4", layer: 1, scale: 0.3f, depthPercent: 0.35f));

            AddFrameList(animPackage: PkgName.MineralsMossyBig1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_1", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_2", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_3", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_4", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig5, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_5", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig6, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_6", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig7, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_7", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig8, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_8", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig9, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_9", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig10, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_10", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig11, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_11", layer: 1, scale: 0.28f, depthPercent: 0.6f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig12, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_12", layer: 1, scale: 0.28f, depthPercent: 0.6f));

            AddFrameList(animPackage: PkgName.MineralsMossySmall1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_1", layer: 1, scale: 0.2f, depthPercent: 0.45f));
            AddFrameList(animPackage: PkgName.MineralsMossySmall2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_2", layer: 1, scale: 0.2f, depthPercent: 0.45f));
            AddFrameList(animPackage: PkgName.MineralsMossySmall3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_3", layer: 1, scale: 0.2f, depthPercent: 0.45f));
            AddFrameList(animPackage: PkgName.MineralsMossySmall4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_4", layer: 1, scale: 0.2f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.WaterLily1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 384, y: 64, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.WaterLily2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 416, y: 0, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.WaterLily3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 448, y: 0, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodSplatter1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 416, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.BloodSplatter2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.BloodSplatter3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 288, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodDrop1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 451, y: 290, width: 7, height: 8));
            AddFrameList(animPackage: PkgName.BloodDrop2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 456, y: 326, width: 8, height: 5));
            AddFrameList(animPackage: PkgName.BloodDrop3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 467, y: 336, width: 7, height: 6));

            AddFrameList(animPackage: PkgName.WoodPlank, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 32, y: 0, width: 32, height: 32, scale: 0.8f));
            AddFrameList(animPackage: PkgName.Stick1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 0, x: 26, y: 73, width: 25, height: 21));
            AddFrameList(animPackage: PkgName.Stick2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 0, x: 100, y: 73, width: 25, height: 22));
            AddFrameList(animPackage: PkgName.Stick3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 0, x: 23, y: 105, width: 25, height: 25));
            AddFrameList(animPackage: PkgName.Stick4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 0, x: 100, y: 104, width: 25, height: 25));
            AddFrameList(animPackage: PkgName.Stick5, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 0, x: 22, y: 72, width: 25, height: 25));
            AddFrameList(animPackage: PkgName.Stick6, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 0, x: 53, y: 68, width: 25, height: 25));
            AddFrameList(animPackage: PkgName.Stick7, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 0, x: 100, y: 70, width: 25, height: 25));
            AddFrameList(animPackage: PkgName.Banana, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "banana", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.Cherry, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "cherry", layer: 0, scale: 0.12f));
            AddFrameList(animPackage: PkgName.Apple, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "apple", layer: 0, scale: 0.075f));
            AddFrameList(animPackage: PkgName.BowBasic, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bow_basic", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.BowAdvanced, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bow_advanced", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.SeedBag, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "seed_bag", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.CoffeeRaw, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "coffee_raw", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.CoffeeRoasted, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "coffee_roasted", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.WoodLogRegular, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "wood_regular", layer: 1, scale: 0.75f));
            AddFrameList(animPackage: PkgName.WoodLogHard, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "wood_hard", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SkullAndBones, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "skull_and_bones", layer: 2, scale: 1f));
            AddFrameList(animPackage: PkgName.IronPlate, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "iron_plate", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.IronRod, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "iron_rod", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.AxeCrystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "axe_crystal", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Stone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "stone", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Granite, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "granite", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.Crystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "crystal", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.CrystalDepositSmall, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "crystal_deposit_small", layer: 1));
            AddFrameList(animPackage: PkgName.CrystalDepositBig, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "crystal_deposit_big", layer: 1));
            AddFrameList(animPackage: PkgName.DigSite, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "dig_site", layer: 0));
            AddFrameList(animPackage: PkgName.DebrisCrystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "crystal_shard", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.DebrisLeaf1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "leaf_1", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.DebrisLeaf2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "leaf_2", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.DebrisLeaf3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "leaf_3", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.Burger, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "burger", layer: 0, scale: 0.07f));
            AddFrameList(animPackage: PkgName.PotionBlack, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "potion_black", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionDarkViolet, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "potion_dark_violet", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionDarkYellow, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "potion_dark_yellow", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionDarkGreen, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "potion_dark_green", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionBrown, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "potion_brown", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionTransparent, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "potion_transparent", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionLightYellow, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bottle_oil", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.WaterDrop, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x1", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SeaWave, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "wave", layer: 0, scale: 0.5f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x1", layer: 2, scale: 0.06f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x2", layer: 2, scale: 0.28f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x3", layer: 2, scale: 0.28f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 3, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x4", layer: 2, scale: 0.29f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 4, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x5", layer: 2, scale: 0.3f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 5, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x6", layer: 2, scale: 0.31f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 6, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x7", layer: 2, scale: 0.32f));
            AddFrameList(animPackage: PkgName.RainDrops, animSize: 7, frameList: ConvertImageToFrameList(atlasName: "water drops/water_drop_x8", layer: 2, scale: 0.33f));
            AddFrameList(animPackage: PkgName.BubbleExclamationRed, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bubble_exclamation_red", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BubbleExclamationBlue, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bubble_exclamation_blue", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BubbleCraftGreen, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bubble_craft_green", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.PlantPoison, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "plant_poison", layer: 0, scale: 0.4f));
            AddFrameList(animPackage: PkgName.PlantPoison, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "plant_poison", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.Map, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "parchment", layer: 0, scale: 0.03f));
            AddFrameList(animPackage: PkgName.MapEdges, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "parchment_edges", layer: 0, scale: 0.03f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.BackpackMediumOutline, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "backpack_medium_outline", layer: 1, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BackpackSmall, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "backpack_small", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.BackpackMedium, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "backpack_medium", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.BackpackBig, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "backpack_big", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.Miss, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "miss", layer: 2));
            AddFrameList(animPackage: PkgName.Zzz, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "zzz", layer: 2));
            AddFrameList(animPackage: PkgName.Heart, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2));
            AddFrameList(animPackage: PkgName.Fog1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_3", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_4", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog5, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_1", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog6, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_2", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog7, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_3", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog8, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fog_4", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Empty, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "transparent_pixel", layer: 2, crop: false, padding: 0, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.DebrisStar1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "star", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.DebrisStar2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "star", layer: 0, scale: 0.12f));
            AddFrameList(animPackage: PkgName.DebrisStar3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "star", layer: 0, scale: 0.05f));
            AddFrameList(animPackage: PkgName.DebrisHeart1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2, scale: 0.8f)); // layer 2 to appear above everything
            AddFrameList(animPackage: PkgName.DebrisHeart2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2, scale: 0.9f)); // layer 2 to appear above everything
            AddFrameList(animPackage: PkgName.DebrisHeart3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2, scale: 1.0f)); // layer 2 to appear above everything
            AddFrameList(animPackage: PkgName.Biceps, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "biceps", layer: 2));
            AddFrameList(animPackage: PkgName.AnimalIcon, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "animal", layer: 2));
            AddFrameList(animPackage: PkgName.Bed, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bed", layer: 2));
            AddFrameList(animPackage: PkgName.Crosshair, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "crosshair", layer: 2));
            AddFrameList(animPackage: PkgName.ScytheStone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "scythe_stone", layer: 0));
            AddFrameList(animPackage: PkgName.ScytheIron, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "scythe_iron", layer: 0));
            AddFrameList(animPackage: PkgName.ScytheCrystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "scythe_crystal", layer: 0));
            AddFrameList(animPackage: PkgName.EmptyBottle, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "bottle_empty", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Acorn, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "acorn", layer: 0, scale: 0.13f));
            AddFrameList(animPackage: PkgName.ArrowWood, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_wood", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowStone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_stone", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowIron, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_iron", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowCrystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_crystal", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.DebrisStone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "stone_small", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.DebrisPlant, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "grass_blade", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.KnifeSimple, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "knife_simple", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.BeltSmall, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "belt_small", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.BeltMedium, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "belt_medium", layer: 0, scale: 0.12f));
            AddFrameList(animPackage: PkgName.BeltBig, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "belt_big", layer: 0, scale: 0.06f));
            AddFrameList(animPackage: PkgName.HatSimple, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "hat_simple", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.BootsProtective, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "boots_protective", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.HerbsGreen, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_green", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBlack, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_black", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBlue, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_blue", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsCyan, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_cyan", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsYellow, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_yellow", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsRed, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_red", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsViolet, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_violet", layer: 0));
            AddFrameList(animPackage: PkgName.SpearWood, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "spear_wood", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearStone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "spear_stone", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearIron, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "spear_iron", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearCrystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "spear_crystal", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Fat, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fat", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.ShovelStone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "shovel_stone", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.ShovelIron, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "shovel_iron", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.ShovelCrystal, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "shovel_crystal", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.Clay, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "clay", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.Hole, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "hole", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.MeatRawRegular, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "meat_raw_regular", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.MeatRawPrime, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "meat_raw_prime", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.MeatDried, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "meat_dried", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.JarWhole, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "jar_sealed", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.JarBroken, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "jar_broken", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.TreeStump, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tree_stump", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.Carrot, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "carrot", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.DebrisCeramic1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "debris_ceramic_1", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.DebrisCeramic2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "debris_ceramic_2", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.DebrisSoot1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "debris_soot_1", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.DebrisSoot2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "debris_soot_2", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.DebrisSoot3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "debris_soot_3", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.MusicNoteSmall, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2));
            AddFrameList(animPackage: PkgName.MusicNoteBig, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2, scale: 2.5f));
            AddFrameList(animPackage: PkgName.WhiteSpot, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.NewIcon, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "new", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.SmallWhiteCircle, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "small_white_circle", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Upgrade, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "upgrade", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Rope, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "rope", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.MapMarker, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "map_marker", layer: 2, crop: false, padding: 0, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Candle, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "candle", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.Dungarees, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "dungarees", layer: 0, scale: 1f));
        }

        public static void CreateAnimsMisc2()
        {
            var chestDict = new Dictionary<PkgName, string> {
                { PkgName.ChestWooden, "chest_wooden/chest_wooden_" },
                { PkgName.ChestStone, "chest_stone/chest_stone_" },
                { PkgName.ChestIron, "chest_iron/chest_iron_" },
                { PkgName.ChestCrystal, "chest_crystal/chest_crystal_" },
                { PkgName.ChestTreasureBlue, "chest_blue/chest_blue_" },
                { PkgName.ChestTreasureRed, "chest_red/chest_red_" },
            };

            foreach (var kvp in chestDict)
            {
                PkgName packageName = kvp.Key;
                string chestPath = kvp.Value;

                float scale = 0.18f;
                bool crop = false;
                float depthPercent = 0.47f;
                int duration = 3;

                var openingFrameList = new List<AnimFrame>();
                for (int i = 2; i <= 6; i++)
                {
                    openingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: i < 6 ? duration : 0, crop: crop, scale: scale, depthPercent: depthPercent));
                }
                AddFrameList(animPackage: packageName, animSize: 0, animName: "opening", frameList: openingFrameList);

                var closingFrameList = new List<AnimFrame>();
                for (int i = 5; i >= 1; i--)
                {
                    closingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: i > 1 ? duration : 0, crop: crop, scale: scale, depthPercent: depthPercent));
                }
                AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: closingFrameList);

                AddFrameList(animPackage: packageName, animSize: 0, animName: "closed", frameList: ConvertImageToFrameList(atlasName: $"{chestPath}1", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "open", frameList: ConvertImageToFrameList(atlasName: $"{chestPath}6", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
            }

            {
                PkgName packageName = PkgName.Crate;
                AddFrameList(animPackage: packageName, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "chests", layer: 1, x: 128, y: 0, width: 32, height: 48));
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 96, width: 32, height: 48, duration: 6),
                    ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 48, width: 32, height: 48, duration: 6),
                    ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 0, width: 32, height: 48, duration: 0)
                };
                AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.WorkshopEssential;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_essential", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_essential", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopBasic;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopAdvanced;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopMaster;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_master", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_master", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopLeatherBasic;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_basic", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_basic", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopLeatherAdvanced;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_advanced", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_advanced", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.AlchemyLabStandard;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_standard", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_standard", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.AlchemyLabAdvanced;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_advanced", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_advanced", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.UpgradeBench;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "upgrade_bench", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "upgrade_bench", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.Furnace;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 0, y: 144, width: 48, height: 48));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
                frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 48, y: 144, width: 48, height: 48));
            }
            {
                PkgName packageName = PkgName.Anvil;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "anvil", layer: 1));
                // the same as "off"
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "anvil", layer: 1));
            }
            {
                PkgName packageName = PkgName.CookingPot;
                var frameListOff = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 192, y: 144, width: 32, height: 32, duration: 6),
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 224, y: 144, width: 32, height: 32, duration: 6),
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 256, y: 144, width: 32, height: 32, duration: 6)
                };
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: frameListOff);
                var frameListOn = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 0, y: 144, width: 32, height: 32, duration: 6),
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 32, y: 144, width: 32, height: 32, duration: 6),
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 64, y: 144, width: 32, height: 32, duration: 6)
                };
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: frameListOn);
            }
            {
                PkgName packageName = PkgName.HotPlate;

                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                    frameList: ConvertImageToFrameList(atlasName: "hot_plate_off", layer: 1));
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "hot_plate_on_1", layer: 1, duration: 6),
                    ConvertImageToFrame(atlasName: "hot_plate_on_2", layer: 1, duration: 6),
                    ConvertImageToFrame(atlasName: "hot_plate_on_3", layer: 1, duration: 6)
                };
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Backlight;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "backlight_1", layer: 0, duration: 6),
                    ConvertImageToFrame(atlasName: "backlight_2", layer: 0, duration: 6),
                    ConvertImageToFrame(atlasName: "backlight_3", layer: 0, duration: 6),
                    ConvertImageToFrame(atlasName: "backlight_4", layer: 0, duration: 20),
                    ConvertImageToFrame(atlasName: "backlight_3", layer: 0, duration: 6),
                    ConvertImageToFrame(atlasName: "backlight_2", layer: 0, duration: 6),
                    ConvertImageToFrame(atlasName: "backlight_1", layer: 0, duration: 6)
                };
                AddFrameList(animPackage: packageName, animSize: 0, frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Attack;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 0, y: 20, width: 23, height: 23),
                    ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 22, y: 13, width: 29, height: 33),
                    ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 50, y: 0, width: 36, height: 65)
                };
                AddFrameList(animPackage: packageName, animSize: 0, frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Flame;

                byte animSize = 0;
                foreach (float scale in new List<float> { 0.5f, 0.75f, 1f, 1.25f })
                {
                    var frameList = new List<AnimFrame>
                    {
                        ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 432, y: 48, width: 48, height: 48, crop: true, scale: scale),
                        ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 480, y: 48, width: 48, height: 48, crop: true, scale: scale),
                        ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 528, y: 48, width: 48, height: 48, crop: true, scale: scale)
                    };
                    AddFrameList(animPackage: packageName, animSize: animSize, frameList: frameList);

                    animSize++;
                }

                foreach (float scale in new List<float> { 1f, 1.5f, 1.8f, 2f })
                {
                    var frameList = new List<AnimFrame>
                    {
                        ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 432, y: 0, width: 48, height: 48, crop: true, scale: scale),
                        ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 480, y: 0, width: 48, height: 48, crop: true, scale: scale),
                        ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 528, y: 0, width: 48, height: 48, crop: true, scale: scale)
                    };
                    AddFrameList(animPackage: packageName, animSize: animSize, frameList: frameList);

                    animSize++;
                }
            }
            {
                PkgName packageName = PkgName.SmallTorch;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "small_torch_on", layer: 0, scale: 0.07f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "small_torch_off", layer: 0, scale: 0.07f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "small_torch_off", layer: 0, scale: 0.07f));
            }
            {
                PkgName packageName = PkgName.BigTorch;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "big_torch_on", layer: 0, scale: 0.1f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "big_torch_off", layer: 0, scale: 0.1f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "big_torch_off", layer: 0, scale: 0.1f));
            }
            AddFrameList(animPackage: PkgName.LanternFrame, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "lantern_frame", layer: 0, scale: 1f));
            {
                PkgName packageName = PkgName.Lantern;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "lantern_on", layer: 0, scale: 1f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "lantern_off", layer: 0, scale: 1f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "lantern_off", layer: 0, scale: 1f));
            }
            {
                PkgName packageName = PkgName.Campfire;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 288, y: 0, width: 48, height: 48, crop: false),
                    ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 336, y: 0, width: 48, height: 48, crop: false),
                    ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 384, y: 0, width: 48, height: 48, crop: false)
                };
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: frameList);
                frameList = ConvertImageToFrameList(atlasName: "flames", layer: 1, x: 288, y: 96, width: 48, height: 48, crop: false);
                AddFrameList(animPackage: packageName, animSize: 0, frameList);
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Explosion;

                for (byte size = 0; size < 4; size++)
                {
                    var frameList = new List<AnimFrame> { };

                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            frameList.Add(ConvertImageToFrame(atlasName: "explosion", layer: 1, duration: 2, x: x * 32, y: y * 32, width: 32, height: 32, scale: 1.5f * (size + 1), crop: false, ignoreWhenCalculatingMaxSize: true));
                        }
                    }

                    AddFrameList(animPackage: packageName, animSize: size, animName: "default", frameList: frameList);
                }
            }

            AddFrameList(animPackage: PkgName.DigSiteGlass, animSize: 0, animName: "default", frameList: new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "dig_site_glass", layer: 0, duration: 450),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_3", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: 0, duration: 1),
                });

            AddFrameList(animPackage: PkgName.ArrowBurning, animName: "default", animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_burning_off", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowBurning, animName: "burning", animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_burning_on", layer: 0, scale: 0.75f));

            AddFrameList(animPackage: PkgName.DigSite, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "dig_site", layer: 0));

            AddFrameList(animPackage: PkgName.Clam, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 256, width: 64, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.MealStandard, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "fancy_food2", layer: 0, x: 288, y: 64, width: 32, height: 32, scale: 0.5f));

            AddFrameList(animPackage: PkgName.AxeWood, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "axe_wooden", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));
            AddFrameList(animPackage: PkgName.AxeStone, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis",
                layer: 0, x: 0, y: 0, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.AxeIron, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis",
                layer: 0, x: 48, y: 0, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.PickaxeWood, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 0, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeStone, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeIron, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeCrystal, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 384, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.DebrisWood, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 0, x: 100, y: 70, width: 25, height: 25, scale: 0.3f));

            AddFrameList(animPackage: PkgName.IronDeposit, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 96, y: 96, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.CoalDeposit, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 0, y: 96, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.IronOre, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.GlassSand, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.IronBar, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 96, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Coal, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 288, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Leather, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 96, y: 96, width: 32, height: 32, scale: 0.75f));

            AddFrameList(animPackage: PkgName.Nail, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 0, y: 0, width: 32, height: 32, scale: 0.5f));

            AddFrameList(animPackage: PkgName.TentSmall, animSize: 0, // TODO replace with A - frame tent asset(when found)
                frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 0.5f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.TentMedium, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 1f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.TentBig, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tent_big", layer: 1, x: 15, y: 0, width: 191, height: 162, scale: 1f, depthPercent: 0.6f));

            AddFrameList(animPackage: PkgName.HumanSkeleton, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "tile_rtp-addons", layer: 0, x: 320, y: 160, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.PotionRed, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 384, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionBlue, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 448, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionViolet, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 416, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionYellow, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 352, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionCyan, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 288, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionGreen, animSize: 0,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 320, y: 128, width: 32, height: 32, scale: 0.5f));
        }

        public static List<AnimFrame> ConvertImageToFrameList(string atlasName, byte layer, int x = 0, int y = 0, int width = 0, int height = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            List<AnimFrame> frameList = new List<AnimFrame>
            {
                ConvertImageToFrame(atlasName: atlasName, layer: layer, x: x, y: y, width: width, height: height, crop: crop, duration: 0, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize)
            };
            return frameList;
        }

        public static AnimFrame ConvertImageToFrame(string atlasName, byte layer, int x = 0, int y = 0, int width = 0, int height = 0, int duration = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            Texture2D atlasTexture = TextureBank.GetTexture(atlasName);
            if (width == 0) width = atlasTexture.Width;
            if (height == 0) height = atlasTexture.Height;

            return AnimFrame.GetFrame(atlasName: atlasName, atlasX: x, atlasY: y, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize);
        }

        public static void AddRPGMakerPackageV1(PkgName packageName, string atlasName, byte setNoX, byte setNoY, byte animSize, bool crop = false, float scale = 1f)
        {
            int offsetX = setNoX * 96;
            int offsetY = setNoY * 128;
            int width = 32;
            int height = 32;

            var yByDirection = new Dictionary<string, int>(){
                { "down", 0 },
                { "up", height * 3 },
                { "right", height * 2 },
                { "left", height },
                };

            // standing
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new List<AnimFrame>
                {
                     AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new List<AnimFrame>
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: (width * 2) + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }

            AddFrameList(animPackage: packageName, animSize: animSize, frameList: new List<AnimFrame> { framesForPkgs[packageName] }); // adding default frame
        }

        public static void AddRPGMakerPackageV2(PkgName packageName, string atlasName, byte setNoX, byte setNoY, byte animSize, bool crop = false, float scale = 1f)
        {
            int offsetX = setNoX * 144;
            int offsetY = setNoY * 192;
            int width = 48;
            int height = 48;

            var yByDirection = new Dictionary<string, int>(){
                { "down", 0 },
                { "up", height * 3 },
                { "left", height },
                { "right", height * 2 },
                };

            // standing
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new List<AnimFrame>
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new List<AnimFrame>
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: 0 + offsetX, atlasY: kvp.Value + offsetY,
                    width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY,
                    width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: (width * 2) + offsetX, atlasY: kvp.Value + offsetY,
                    width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY,
                    width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }

            AddFrameList(animPackage: packageName, animSize: animSize, frameList: new List<AnimFrame> { framesForPkgs[packageName] }); // adding default frame
        }

        private static string JsonDataPath
        { get { return Path.Combine(SonOfRobinGame.animCachePath, "_data.json"); } }

        public static bool LoadJsonDict()
        {
            // one big json is used to speed up loading / saving data

            Dictionary<string, Object> loadedJsonDict;

            try
            {
                var loadedJson = FileReaderWriter.Load(path: JsonDataPath);
                if (loadedJson == null) return false;

                loadedJsonDict = (Dictionary<string, Object>)loadedJson;
            }
            catch (InvalidCastException)
            { return false; }

            try
            {
                float jsonVersion = (float)(double)loadedJsonDict["currentVersion"];
                if (jsonVersion != currentVersion) return false;
            }
            catch (InvalidCastException) { return false; }
            catch (KeyNotFoundException) { return false; }

            jsonDict = loadedJsonDict;
            return true;
        }

        public static void PurgeDiskCache()
        {
            try
            {
                var directory = new DirectoryInfo(SonOfRobinGame.animCachePath);
                foreach (var file in directory.GetFiles())
                {
                    try
                    { file.Delete(); }
                    catch (UnauthorizedAccessException)
                    { } // ignore read-only files
                    catch (IOException)
                    { } // ignore locked files
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) // handle other I/O errors
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Failed to purge anim cache: {ex.Message}");
            }

            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Anim cache purged.");
        }

        public static void SaveJsonDict()
        {
            jsonDict["currentVersion"] = currentVersion;

            FileReaderWriter.Save(path: JsonDataPath, savedObj: jsonDict, compress: true);
        }

        public static void DeleteUsedAtlases()
        {
            // Should be used after loading textures from all atlasses.
            // Deleted textures will not be available for use any longer.

            var usedAtlasNames = new List<string>();

            foreach (List<AnimFrame> frameList in frameListById.Values)
            {
                foreach (AnimFrame animFrame in frameList)
                {
                    if (animFrame.atlasName != null && !usedAtlasNames.Contains(animFrame.atlasName)) usedAtlasNames.Add(animFrame.atlasName);
                }
            }

            foreach (string atlasName in usedAtlasNames)
            {
                TextureBank.DisposeTexture(atlasName);
            }
        }
    }
}