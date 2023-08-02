using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class AnimData
    {
        public const float currentVersion = 1.000007f; // version number should be incremented when any existing asset is updated

        public static readonly Dictionary<string, AnimFrame> frameById = new Dictionary<string, AnimFrame>(); // needed to access frames directly by id (for loading and saving game)
        public static readonly Dictionary<string, List<AnimFrame>> frameListById = new Dictionary<string, List<AnimFrame>>();
        public static readonly Dictionary<PkgName, AnimFrame> framesForPkgs = new Dictionary<PkgName, AnimFrame>(); // default frames for packages

        public static readonly Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Object> jsonDict = new Dictionary<string, Object>();

        public enum PkgName : ushort
        {
            NoAnim = 0,
            Empty = 1,
            WhiteSpotLayerMinus1 = 305,
            WhiteSpotLayerOne = 268,
            WhiteSpotLayerZero = 303,
            WhiteSpotLayerTwo = 304,

            FlowersWhite = 3,
            FlowersYellow1 = 4,
            FlowersYellow2 = 5,
            FlowersRed = 6,

            Rushes = 7,
            GrassDesert = 8,
            GrassRegular = 9,
            PlantPoison = 10,
            CoffeeShrub = 11,
            CarrotPlant = 12,
            TomatoPlant = 13,

            Cactus = 2,

            PalmTree = 14,
            TreeBig = 15,
            TreeSmall1 = 16,
            TreeSmall2 = 17,

            TreeStump = 18,

            WaterLily1 = 19,
            WaterLily2 = 20,
            WaterLily3 = 21,

            MineralsBig1 = 32,
            MineralsBig2 = 33,
            MineralsBig3 = 34,
            MineralsBig4 = 35,
            MineralsBig5 = 36,
            MineralsBig6 = 37,
            MineralsBig7 = 38,
            MineralsBig8 = 39,
            MineralsBig9 = 40,
            MineralsBig10 = 41,
            MineralsBig11 = 42,
            MineralsBig12 = 43,
            MineralsBig13 = 44,
            MineralsBig14 = 45,

            MineralsSmall1 = 46,
            MineralsSmall2 = 47,
            MineralsSmall3 = 48,
            MineralsSmall4 = 49,

            MineralsMossyBig1 = 50,
            MineralsMossyBig2 = 51,
            MineralsMossyBig3 = 52,
            MineralsMossyBig4 = 53,
            MineralsMossyBig5 = 54,
            MineralsMossyBig6 = 55,
            MineralsMossyBig7 = 56,
            MineralsMossyBig8 = 57,
            MineralsMossyBig9 = 58,
            MineralsMossyBig10 = 59,
            MineralsMossyBig11 = 60,
            MineralsMossyBig12 = 61,

            MineralsMossySmall1 = 62,
            MineralsMossySmall2 = 63,
            MineralsMossySmall3 = 64,
            MineralsMossySmall4 = 65,

            JarWhole = 66,
            JarBroken = 67,

            ChestWooden = 68,
            ChestStone = 69,
            ChestIron = 70,
            ChestCrystal = 71,
            ChestTreasureBlue = 72,
            ChestTreasureRed = 73,

            WoodLogRegular = 74,
            WoodLogHard = 75,
            WoodPlank = 76,

            Nail = 77,
            Rope = 78,
            Crate = 79,

            WorkshopEssential = 80,
            WorkshopBasic = 81,
            WorkshopAdvanced = 82,
            WorkshopMaster = 83,

            WorkshopMeatHarvesting = 306,

            WorkshopLeatherBasic = 86,
            WorkshopLeatherAdvanced = 87,

            AlchemyLabStandard = 84,
            AlchemyLabAdvanced = 85,

            Furnace = 89,
            Anvil = 90,
            HotPlate = 91,
            CookingPot = 92,

            Stick1 = 93,
            Stick2 = 94,
            Stick3 = 95,
            Stick4 = 96,
            Stick5 = 97,
            Stick6 = 98,
            Stick7 = 99,

            Stone = 100,
            Granite = 101,

            Clay = 102,

            Apple = 103,
            Banana = 104,
            Cherry = 105,
            Tomato = 106,
            Carrot = 107,
            Acorn = 108,

            SeedBag = 109,
            CoffeeRaw = 110,
            CoffeeRoasted = 111,

            Clam = 112,

            MeatRawRegular = 113,
            MeatRawPrime = 292,
            MeatDried = 114,
            Fat = 115,
            Burger = 116,
            MealStandard = 117,
            Leather = 118,

            KnifeSimple = 119,

            AxeWood = 120,
            AxeStone = 121,
            AxeIron = 122,
            AxeCrystal = 123,

            PickaxeWood = 124,
            PickaxeStone = 125,
            PickaxeIron = 126,
            PickaxeCrystal = 127,

            ScytheStone = 128,
            ScytheIron = 129,
            ScytheCrystal = 130,

            SpearWood = 131,
            SpearStone = 132,
            SpearIron = 133,
            SpearCrystal = 134,

            ShovelStone = 135,
            ShovelIron = 136,
            ShovelCrystal = 137,

            BowBasic = 138,
            BowAdvanced = 139,

            ArrowWood = 140,
            ArrowStone = 141,
            ArrowIron = 142,
            ArrowCrystal = 143,
            ArrowExploding = 144,

            CoalDeposit = 145,
            IronDeposit = 146,

            CrystalDepositSmall = 147,
            CrystalDepositBig = 148,

            DigSite = 149,
            DigSiteGlass = 150,

            Coal = 151,
            IronOre = 152,
            IronBar = 153,
            IronRod = 154,
            IronPlate = 155,
            GlassSand = 156,
            Crystal = 157,

            PlayerBoy = 158,
            PlayerGirl = 159,
            PlayerTestDemoness = 160,

            FoxBlack = 169,
            FoxBrown = 170,
            FoxChocolate = 171,
            FoxGinger = 172,
            FoxGray = 173,
            FoxRed = 174,
            FoxWhite = 175,
            FoxYellow = 176,

            Frog1 = 177,
            Frog2 = 178,
            Frog3 = 179,
            Frog4 = 180,
            Frog5 = 181,
            Frog6 = 182,
            Frog7 = 183,
            Frog8 = 184,

            RabbitBeige = 185,
            RabbitBlack = 186,
            RabbitBrown = 187,
            RabbitDarkBrown = 188,
            RabbitGray = 189,
            RabbitLightBrown = 190,
            RabbitLightGray = 191,
            RabbitWhite = 192,

            TigerOrangeMedium = 193,
            TigerOrangeLight = 194,
            TigerGray = 195,
            TigerWhite = 196,
            TigerOrangeDark = 197,
            TigerBrown = 198,
            TigerYellow = 199,
            TigerBlack = 200,

            TentSmall = 222,
            TentMedium = 223,
            TentBig = 224,

            BackpackMediumOutline = 225,

            BackpackSmall = 226,
            BackpackMedium = 227,
            BackpackBig = 228,

            BeltSmall = 229,
            BeltMedium = 230,
            BeltBig = 231,
            Map = 232,

            HatSimple = 233,
            BootsProtective = 234,
            Dungarees = 235,

            LanternFrame = 236,
            Candle = 238,
            Lantern = 237,

            SmallTorch = 239,
            BigTorch = 240,
            Campfire = 241,

            HerbsBlack = 242,
            HerbsCyan = 243,
            HerbsBlue = 244,
            HerbsGreen = 245,
            HerbsYellow = 246,
            HerbsRed = 247,
            HerbsViolet = 248,
            HerbsBrown = 300,
            HerbsDarkViolet = 301,
            HerbsDarkGreen = 302,

            EmptyBottle = 249,
            PotionRed = 250,
            PotionBlue = 251,
            PotionViolet = 252,
            PotionYellow = 253,
            PotionCyan = 254,
            PotionGreen = 255,
            PotionBlack = 256,
            PotionDarkViolet = 257,
            PotionDarkYellow = 258,
            PotionDarkGreen = 259,
            PotionLightYellow = 260,
            PotionTransparent = 261,
            PotionBrown = 262,

            BloodSplatter1 = 263,
            BloodSplatter2 = 264,
            BloodSplatter3 = 265,

            HumanSkeleton = 266,
            Hole = 267,

            Explosion = 272,
            SkullAndBones = 274,
            MusicNoteSmall = 275,
            MusicNoteBig = 276,
            Miss = 279,
            Attack = 280,
            MapMarker = 281,
            Backlight = 282,
            Crosshair = 283,
            Flame = 284,
            Upgrade = 285,
            WaterDrop = 286,
            RainDrops = 287,
            Star = 213,
            Zzz = 22,
            Heart = 23,

            Fog1 = 24,
            Fog2 = 25,
            Fog3 = 26,
            Fog4 = 27,
            Fog5 = 28,
            Fog6 = 29,
            Fog7 = 30,
            Fog8 = 31,

            BubbleExclamationRed = 288,
            BubbleExclamationBlue = 289,
            BubbleCraftGreen = 290,

            SeaWave = 291,

            FertileGroundSmall = 293,
            FertileGroundMedium = 294,
            FertileGroundLarge = 295,
            FenceHorizontalShort = 296,
            FenceVerticalShort = 297,
            FenceHorizontalLong = 298,
            FenceVerticalLong = 299,

            // obsolete below (kept for compatibility with old saves)

            DebrisPlantObsolete = 201,
            DebrisStoneObsolete = 202,
            DebrisWoodObsolete = 203,
            DebrisCrystalObsolete = 204,
            DebrisCeramic1Obsolete = 205,
            DebrisCeramic2Obsolete = 206,
            DebrisLeaf1Obsolete = 207,
            DebrisLeaf2Obsolete = 208,
            DebrisLeaf3Obsolete = 209,
            BloodDrop1Obsolete = 210,
            BloodDrop2Obsolete = 211,
            BloodDrop3Obsolete = 212,
            DebrisStar2Obsolete = 214,
            DebrisStar3Obsolete = 215,
            DebrisHeart1Obsolete = 216,
            DebrisHeart2Obsolete = 217,
            DebrisHeart3Obsolete = 218,
            DebrisSoot1Obsolete = 219,
            DebrisSoot2Obsolete = 220,
            DebrisSoot3Obsolete = 221,
            CrabBeige = 161,
            CrabBrown = 162,
            CrabDarkBrown = 163,
            CrabGray = 164,
            CrabGreen = 165,
            CrabLightBlue = 166,
            CrabRed = 167,
            CrabYellow = 168,
            Bed = 278,
            AnimalIcon = 273,
            NewIcon = 269,
            MapEdges = 270,
            UpgradeBench = 88,
            SmallWhiteCircle = 271,
            Biceps = 277,
        }

        public static void AddFrameList(PkgName animPackage, int animSize, List<AnimFrame> frameList, string animName = "default")
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
        }

        public static void CreateAnimsMisc1()
        {
            AddFrameList(animPackage: PkgName.NoAnim, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "no_anim", layer: 1, x: 0, y: 0, width: 0, height: 0));

            AddFrameList(animPackage: PkgName.MineralsBig1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_1", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_2", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_3", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_4", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig5, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_5", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig6, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_6", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig7, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_7", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig8, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_8", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig9, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_9", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig10, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_10", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig11, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_11", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig12, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_12", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig13, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_13", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig14, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_big_14", layer: 1, scale: 0.3f, depthPercent: 0.4f));

            AddFrameList(animPackage: PkgName.MineralsSmall1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_1", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_2", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_3", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "minerals_small_4", layer: 1, scale: 0.3f, depthPercent: 0.35f));

            AddFrameList(animPackage: PkgName.MineralsMossyBig1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_1", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig2, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_2", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig3, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_3", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig4, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_4", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig5, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_5", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig6, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_6", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig7, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_7", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig8, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_8", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig9, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_9", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig10, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_10", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig11, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_11", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig12, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_12", layer: 1, scale: 0.28f, depthPercent: 0.4f));

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
            AddFrameList(animPackage: PkgName.Empty, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "transparent_pixel", layer: 2, crop: false, padding: 0));
            AddFrameList(animPackage: PkgName.Star, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "star", layer: 0, scale: 0.1f));
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
            AddFrameList(animPackage: PkgName.HerbsBrown, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_brown", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsDarkViolet, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_dark_violet", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsDarkGreen, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "herbs_dark_green", layer: 0));
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
            AddFrameList(animPackage: PkgName.FertileGroundSmall, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_small", layer: -1, scale: 1f));
            AddFrameList(animPackage: PkgName.FertileGroundMedium, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_medium", layer: -1, scale: 1f));
            AddFrameList(animPackage: PkgName.FertileGroundLarge, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_big", layer: -1, scale: 1.3f));
            AddFrameList(animPackage: PkgName.FenceHorizontalShort, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fence_horizontal_short", layer: 1, depthPercent: 0.2f));
            AddFrameList(animPackage: PkgName.FenceVerticalShort, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fence_vertical_short", layer: 1, depthPercent: 0.9f));
            AddFrameList(animPackage: PkgName.FenceHorizontalLong, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fence_horizontal_long", layer: 1, depthPercent: 0.2f));
            AddFrameList(animPackage: PkgName.FenceVerticalLong, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "fence_vertical_long", layer: 1, depthPercent: 0.95f));
            AddFrameList(animPackage: PkgName.MeatDried, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "meat_dried", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.JarWhole, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "jar_sealed", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.JarBroken, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "jar_broken", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.TreeStump, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "tree_stump", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.Carrot, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "carrot", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.MusicNoteSmall, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2));
            AddFrameList(animPackage: PkgName.MusicNoteBig, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2, scale: 2.5f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerMinus1, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: -1, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerZero, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerOne, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerTwo, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 2, scale: 1f));
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
                    openingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (ushort)(i < 6 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
                }
                AddFrameList(animPackage: packageName, animSize: 0, animName: "opening", frameList: openingFrameList);

                var closingFrameList = new List<AnimFrame>();
                for (int i = 5; i >= 1; i--)
                {
                    closingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (ushort)(i > 1 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
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
                PkgName packageName = PkgName.WorkshopMeatHarvesting;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_meat_harvesting_off", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_meat_harvesting_on", layer: 1, scale: 0.5f));
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

                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                    frameList: ConvertImageToFrameList(atlasName: "Candacis_flames1", layer: 1, x: 192, y: 144, width: 32, height: 32));

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

            AddFrameList(animPackage: PkgName.ArrowExploding, animName: "default", animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_burning_off", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowExploding, animName: "burning", animSize: 0, frameList: ConvertImageToFrameList(atlasName: "arrow_burning_on", layer: 0, scale: 0.75f));

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

        public static List<AnimFrame> ConvertImageToFrameList(string atlasName, int layer, int x = 0, int y = 0, int width = 0, int height = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            List<AnimFrame> frameList = new List<AnimFrame>
            {
                ConvertImageToFrame(atlasName: atlasName, layer: layer, x: x, y: y, width: width, height: height, crop: crop, duration: 0, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize)
            };
            return frameList;
        }

        public static AnimFrame ConvertImageToFrame(string atlasName, int layer, int x = 0, int y = 0, int width = 0, int height = 0, ushort duration = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            Texture2D atlasTexture = TextureBank.GetTexture(atlasName);
            if (width == 0) width = atlasTexture.Width;
            if (height == 0) height = atlasTexture.Height;

            return AnimFrame.GetFrame(atlasName: atlasName, atlasX: x, atlasY: y, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize);
        }

        public static void AddRPGMakerPackageV1(PkgName packageName, string atlasName, byte setNoX, byte setNoY, int animSize, bool crop = false, float scale = 1f)
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