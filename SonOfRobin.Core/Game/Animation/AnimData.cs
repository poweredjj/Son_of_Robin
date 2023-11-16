using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class AnimData
    {
        public const float currentVersion = 1.000024f; // version number should be incremented when any existing asset is updated

        public static readonly Dictionary<string, AnimFrame> frameById = new(); // needed to access frames directly by id (for loading and saving game)
        public static readonly Dictionary<string, List<AnimFrame>> frameListById = new();
        public static readonly Dictionary<PkgName, AnimFrame> croppedFramesForPkgs = new(); // default frames for packages (cropped)
        public static readonly Dictionary<PkgName, int> animSizesForPkgs = new(); // information about saved package sizes

        public static readonly Dictionary<string, Texture2D> textureDict = new();
        public static Dictionary<string, Object> jsonDict = new();

        public enum PkgName : ushort
        {
            NoAnim = 0,
            Empty = 1,

            WhiteSpotLayerMinus1 = 2,
            WhiteSpotLayerZero = 4,
            WhiteSpotLayerOne = 3,
            WhiteSpotLayerTwo = 5,
            WhiteSpotLayerThree = 285,

            FlowersWhite = 6,
            FlowersYellow1 = 7,
            FlowersYellow2 = 8,
            FlowersRed = 9,

            Rushes = 10,
            GrassDesert = 11,
            GrassRegular = 12,
            PlantPoison = 13,
            CoffeeShrub = 14,
            CarrotPlant = 15,
            TomatoPlant = 16,
            MushroomPlant = 305,

            Cactus = 17,

            PalmTree = 18,
            TreeBig = 19,
            TreeSmall1 = 20,
            TreeSmall2 = 21,

            TreeStump = 22,

            WaterLily1 = 23,
            WaterLily2 = 24,
            WaterLily3 = 25,

            MineralsBig1 = 26,
            MineralsBig2 = 27,
            MineralsBig3 = 28,
            MineralsBig4 = 29,
            MineralsBig5 = 30,
            MineralsBig6 = 31,
            MineralsBig7 = 32,
            MineralsBig8 = 33,
            MineralsBig9 = 34,
            MineralsBig10 = 35,
            MineralsBig11 = 36,
            MineralsBig12 = 37,
            MineralsBig13 = 38,
            MineralsBig14 = 39,

            MineralsSmall1 = 40,
            MineralsSmall2 = 41,
            MineralsSmall3 = 42,
            MineralsSmall4 = 43,

            MineralsMossyBig1 = 44,
            MineralsMossyBig2 = 45,
            MineralsMossyBig3 = 46,
            MineralsMossyBig4 = 47,
            MineralsMossyBig5 = 48,
            MineralsMossyBig6 = 49,
            MineralsMossyBig7 = 50,
            MineralsMossyBig8 = 51,
            MineralsMossyBig9 = 52,
            MineralsMossyBig10 = 53,
            MineralsMossyBig11 = 54,
            MineralsMossyBig12 = 55,

            MineralsMossySmall1 = 56,
            MineralsMossySmall2 = 57,
            MineralsMossySmall3 = 58,
            MineralsMossySmall4 = 59,

            MineralsCave = 306,

            JarWhole = 60,
            JarBroken = 61,

            ChestWooden = 62,
            ChestStone = 63,
            ChestIron = 64,
            ChestCrystal = 65,
            ChestTreasureBlue = 66,
            ChestTreasureRed = 67,

            WoodLogRegular = 68,
            WoodLogHard = 69,
            WoodPlank = 70,

            Nail = 71,
            Rope = 72,
            Crate = 73,

            WorkshopEssential = 74,
            WorkshopBasic = 75,
            WorkshopAdvanced = 76,
            WorkshopMaster = 77,

            WorkshopMeatHarvesting = 78,

            WorkshopLeatherBasic = 79,
            WorkshopLeatherAdvanced = 80,

            MeatDryingRackRegular = 81,
            MeatDryingRackWide = 82,

            AlchemyLabStandard = 83,
            AlchemyLabAdvanced = 84,

            Furnace = 85,
            Anvil = 86,
            HotPlate = 87,
            CookingPot = 88,

            Totem = 89,
            RuinsWallHorizontal1 = 90,
            RuinsWallHorizontal2 = 91,
            RuinsWallWallVertical = 92,
            RuinsColumn = 93,
            RuinsRubble = 94,

            Stick = 95,
            Stone = 96,
            Granite = 97,

            Clay = 98,

            Apple = 99,
            Banana = 100,
            Cherry = 101,
            Tomato = 102,
            Carrot = 103,
            Acorn = 104,
            Mushroom = 307,

            SeedBag = 105,
            CoffeeRaw = 106,
            CoffeeRoasted = 107,

            Clam = 108,

            MeatRawRegular = 109,
            MeatRawPrime = 110,
            MeatDried = 111,
            Fat = 112,
            Burger = 113,
            MealStandard = 114,
            Leather = 115,

            KnifeSimple = 116,

            AxeWood = 117,
            AxeStone = 118,
            AxeIron = 119,
            AxeCrystal = 120,

            PickaxeWood = 121,
            PickaxeStone = 122,
            PickaxeIron = 123,
            PickaxeCrystal = 124,

            ScytheStone = 125,
            ScytheIron = 126,
            ScytheCrystal = 127,

            SpearWood = 128,
            SpearStone = 129,
            SpearIron = 130,
            SpearCrystal = 131,

            ShovelStone = 132,
            ShovelIron = 133,
            ShovelCrystal = 134,

            BowBasic = 135,
            BowAdvanced = 136,

            ArrowWood = 137,
            ArrowStone = 138,
            ArrowIron = 139,
            ArrowCrystal = 140,
            ArrowExploding = 141,

            CoalDeposit = 142,
            IronDeposit = 143,

            CrystalDepositSmall = 144,
            CrystalDepositBig = 145,

            DigSite = 146,
            DigSiteGlass = 147,
            DigSiteRuins = 148,

            Coal = 149,
            IronOre = 150,
            IronBar = 151,
            IronRod = 152,
            IronPlate = 153,
            GlassSand = 154,
            Crystal = 155,

            PlayerBoy = 156,
            PlayerGirl = 157,
            PlayerTestDemoness = 158,

            FoxBlack = 159,
            FoxBrown = 160,
            FoxChocolate = 161,
            FoxGinger = 162,
            FoxGray = 163,
            FoxRed = 164,
            FoxWhite = 165,
            FoxYellow = 166,

            Frog1 = 167,
            Frog2 = 168,
            Frog3 = 169,
            Frog4 = 170,
            Frog5 = 171,
            Frog6 = 172,
            Frog7 = 173,
            Frog8 = 174,

            RabbitBeige = 175,
            RabbitBlack = 176,
            RabbitBrown = 177,
            RabbitDarkBrown = 178,
            RabbitGray = 179,
            RabbitLightBrown = 180,
            RabbitLightGray = 181,
            RabbitWhite = 182,

            TigerOrangeMedium = 183,
            TigerOrangeLight = 184,
            TigerGray = 185,
            TigerWhite = 186,
            TigerOrangeDark = 187,
            TigerBrown = 188,
            TigerYellow = 189,
            TigerBlack = 190,

            BearBrown = 297,
            BearWhite = 298,
            BearOrange = 299,
            BearBlack = 300,
            BearDarkBrown = 301,
            BearGray = 302,
            BearRed = 303,
            BearBeige = 304,

            TentModern = 283,
            TentModernPacked = 284,
            TentSmall = 191,
            TentMedium = 192,
            TentBig = 193,

            BackpackMediumOutline = 194,

            BackpackSmall = 195,
            BackpackMedium = 196,
            BackpackBig = 197,
            BackpackLuxurious = 198,

            BeltSmall = 199,
            BeltMedium = 200,
            BeltBig = 201,
            BeltLuxurious = 202,
            Map = 203,

            HatSimple = 204,
            BootsProtective = 205,
            BootsMountain = 206,
            BootsSpeed = 207,
            BootsAllTerrain = 208,
            GlovesStrength = 209,
            GlassesBlue = 210,
            Dungarees = 211,

            LanternFrame = 212,
            Candle = 213,
            Lantern = 214,

            SmallTorch = 215,
            BigTorch = 216,
            CampfireSmall = 217,
            CampfireMedium = 218,

            BoatConstruction = 308,
            BoatComplete = 310,

            HerbsBlack = 219,
            HerbsCyan = 220,
            HerbsBlue = 221,
            HerbsGreen = 222,
            HerbsYellow = 223,
            HerbsRed = 224,
            HerbsViolet = 225,
            HerbsBrown = 226,
            HerbsDarkViolet = 227,
            HerbsDarkGreen = 228,

            EmptyBottle = 229,
            PotionRed = 230,
            PotionBlue = 231,
            PotionViolet = 232,
            PotionYellow = 233,
            PotionCyan = 234,
            PotionGreen = 235,
            PotionBlack = 236,
            PotionDarkViolet = 237,
            PotionDarkYellow = 238,
            PotionDarkGreen = 239,
            PotionLightYellow = 240,
            PotionTransparent = 241,
            PotionBrown = 242,

            BloodSplatter1 = 243,
            BloodSplatter2 = 244,
            BloodSplatter3 = 245,

            HumanSkeleton = 246,
            Hole = 247,

            Explosion = 248,
            SkullAndBones = 249,
            MusicNoteSmall = 250,
            MusicNoteBig = 251,
            Miss = 252,
            Attack = 253,
            MapMarker = 254,
            Backlight = 255,
            Crosshair = 256,
            Flame = 257,
            Upgrade = 258,
            WaterDrop = 259,
            Star = 261,
            Zzz = 262,
            Heart = 263,
            Hammer = 309,

            Fog1 = 264,
            Fog2 = 265,
            Fog3 = 266,
            Fog4 = 267,

            BubbleExclamationRed = 272,
            BubbleExclamationBlue = 273,
            BubbleCraftGreen = 274,

            SeaWave = 275,
            WeatherFog1 = 286,
            WeatherFog2 = 287,
            WeatherFog3 = 288,
            WeatherFog4 = 289,
            WeatherFog5 = 290,
            WeatherFog6 = 291,
            WeatherFog7 = 292,
            WeatherFog8 = 293,
            WeatherFog9 = 294,

            FertileGroundSmall = 276,
            FertileGroundMedium = 277,
            FertileGroundLarge = 278,
            FenceHorizontalShort = 279,
            FenceVerticalShort = 280,
            FenceHorizontalLong = 281,
            FenceVerticalLong = 282,

            CaveEntrance = 295,
            CaveExit = 296,

            // obsolete below (kept for compatibility with old saves)
        }

        public static void AddFrameList(PkgName animPackage, List<AnimFrame> frameList, int animSize = 0, string animName = "default")
        {
            if (!animSizesForPkgs.ContainsKey(animPackage) || animSizesForPkgs[animPackage] < animSize)
            {
                croppedFramesForPkgs[animPackage] = frameList[0].GetCroppedFrameCopy();
                animSizesForPkgs[animPackage] = animSize;
            }

            string completeAnimID = $"{animPackage}-{animSize}-{animName}";
            frameListById[completeAnimID] = new List<AnimFrame>(frameList);
        }

        // animation loading should be split in multiple methods, to avoid loading them all at once

        public static void CreateAnimsPlants()
        {
            {
                PkgName packageName = PkgName.GrassRegular;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "grass_regular_x3", layer: 1));
            }
            {
                PkgName packageName = PkgName.GrassDesert;
                AddFrameList(animPackage: packageName,
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 160, y: 224, width: 24, height: 24));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 288, y: 160, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersYellow1;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.MushroomPlant;
                AddFrameList(animPackage: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mushroom_plant", layer: 1, scale: 0.6f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "mushroom_plant", layer: 1, scale: 0.8f));
                AddFrameList(animPackage: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "mushroom_plant", layer: 1, scale: 1.0f));
            }
            {
                PkgName packageName = PkgName.FlowersYellow2;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x",
                    layer: 0, x: 832, y: 128, width: 64, height: 64, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x",
                    layer: 1, x: 768, y: 0, width: 64, height: 64, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.FlowersRed;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 64, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersWhite;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 32, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Rushes;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 96, y: 192, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Cactus;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 352, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 288, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.PalmTree;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "palmtree_small", layer: 1, scale: 0.7f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 123, y: 326, width: 69, height: 80, crop: false));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 8, y: 145, width: 66, height: 102, crop: false));
                AddFrameList(animPackage: packageName, animSize: 3,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 108, y: 145, width: 72, height: 102, crop: false));
            }
            {
                PkgName packageName = PkgName.TreeSmall1;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "sapling_short", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 192, y: 352, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 160, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeSmall2;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "sapling_short", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 128, y: 192, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 160, y: 192, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeBig;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "sapling_tall", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 199, y: 382, width: 47, height: 66));
                AddFrameList(animPackage: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 0, y: 0, width: 63, height: 97));
            }
            {
                PkgName packageName = PkgName.TomatoPlant;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_small", layer: 1, scale: 0.1f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_medium", layer: 1, scale: 0.08f));
                AddFrameList(animPackage: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_big", layer: 1, scale: 0.08f));
            }
            AddFrameList(animPackage: PkgName.Tomato, frameList: ConvertImageToFrameList(atlasName: "tomato", layer: 0, scale: 0.07f));
            {
                PkgName packageName = PkgName.CoffeeShrub;
                AddFrameList(animPackage: packageName, frameList: ConvertImageToFrameList(atlasName: "coffee_shrub", layer: 1, scale: 0.06f));
                AddFrameList(animPackage: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "coffee_shrub", layer: 1, scale: 0.06f));
            }
            AddFrameList(animPackage: PkgName.CarrotPlant, animName: "default", frameList: ConvertImageToFrameList(atlasName: "carrot_plant_empty", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.CarrotPlant, animName: "has_fruits", frameList: ConvertImageToFrameList(atlasName: "carrot_plant_has_carrot", layer: 1, scale: 0.1f)); // using different plant graphics when carrot is present, instead of drawing the carrot separately (because the carrot should be underground)
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

            foreach (var kvp in new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } })
            {
                string atlasName = "characters/bear";
                byte animSize = kvp.Key;
                float scale = kvp.Value;

                AddRPGMakerPackageV2(packageName: PkgName.BearBrown, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearWhite, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearOrange, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearBlack, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearDarkBrown, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearGray, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearRed, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.BearBeige, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: animSize, scale: scale);
            }
            {
                float scale = 1f;
                byte animSize = 0;
                string atlasName = "characters/frogs_small";
                AddRPGMakerPackageV2(packageName: PkgName.Frog1, atlasName: atlasName, setNoX: 0, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog2, atlasName: atlasName, setNoX: 1, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog3, atlasName: atlasName, setNoX: 2, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog4, atlasName: atlasName, setNoX: 3, setNoY: 0, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog5, atlasName: atlasName, setNoX: 0, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog6, atlasName: atlasName, setNoX: 1, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog7, atlasName: atlasName, setNoX: 2, setNoY: 1, animSize: animSize, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog8, atlasName: atlasName, setNoX: 3, setNoY: 1, animSize: animSize, scale: scale);
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
            AddFrameList(animPackage: PkgName.NoAnim, frameList: ConvertImageToFrameList(atlasName: "no_anim", layer: 1, x: 0, y: 0, width: 0, height: 0));

            AddFrameList(animPackage: PkgName.MineralsBig1, frameList: ConvertImageToFrameList(atlasName: "minerals_big_1", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig2, frameList: ConvertImageToFrameList(atlasName: "minerals_big_2", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig3, frameList: ConvertImageToFrameList(atlasName: "minerals_big_3", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig4, frameList: ConvertImageToFrameList(atlasName: "minerals_big_4", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig5, frameList: ConvertImageToFrameList(atlasName: "minerals_big_5", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig6, frameList: ConvertImageToFrameList(atlasName: "minerals_big_6", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig7, frameList: ConvertImageToFrameList(atlasName: "minerals_big_7", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig8, frameList: ConvertImageToFrameList(atlasName: "minerals_big_8", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig9, frameList: ConvertImageToFrameList(atlasName: "minerals_big_9", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig10, frameList: ConvertImageToFrameList(atlasName: "minerals_big_10", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig11, frameList: ConvertImageToFrameList(atlasName: "minerals_big_11", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig12, frameList: ConvertImageToFrameList(atlasName: "minerals_big_12", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig13, frameList: ConvertImageToFrameList(atlasName: "minerals_big_13", layer: 1, scale: 0.3f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsBig14, frameList: ConvertImageToFrameList(atlasName: "minerals_big_14", layer: 1, scale: 0.3f, depthPercent: 0.4f));

            AddFrameList(animPackage: PkgName.MineralsSmall1, frameList: ConvertImageToFrameList(atlasName: "minerals_small_1", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall2, frameList: ConvertImageToFrameList(atlasName: "minerals_small_2", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall3, frameList: ConvertImageToFrameList(atlasName: "minerals_small_3", layer: 1, scale: 0.3f, depthPercent: 0.35f));
            AddFrameList(animPackage: PkgName.MineralsSmall4, frameList: ConvertImageToFrameList(atlasName: "minerals_small_4", layer: 1, scale: 0.3f, depthPercent: 0.35f));

            AddFrameList(animPackage: PkgName.MineralsMossyBig1, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_1", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig2, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_2", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig3, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_3", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig4, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_4", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig5, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_5", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig6, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_6", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig7, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_7", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig8, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_8", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig9, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_9", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig10, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_10", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig11, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_11", layer: 1, scale: 0.28f, depthPercent: 0.4f));
            AddFrameList(animPackage: PkgName.MineralsMossyBig12, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_12", layer: 1, scale: 0.28f, depthPercent: 0.4f));

            AddFrameList(animPackage: PkgName.MineralsMossySmall1, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_1", layer: 1, scale: 0.2f, depthPercent: 0.45f));
            AddFrameList(animPackage: PkgName.MineralsMossySmall2, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_2", layer: 1, scale: 0.2f, depthPercent: 0.45f));
            AddFrameList(animPackage: PkgName.MineralsMossySmall3, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_3", layer: 1, scale: 0.2f, depthPercent: 0.45f));
            AddFrameList(animPackage: PkgName.MineralsMossySmall4, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_4", layer: 1, scale: 0.2f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.MineralsCave, frameList: ConvertImageToFrameList(atlasName: "cave_minerals", layer: 1, scale: 0.3f, depthPercent: 0.75f));

            AddFrameList(animPackage: PkgName.WaterLily1, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 384, y: 64, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.WaterLily2, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 416, y: 0, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.WaterLily3, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 448, y: 0, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodSplatter1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 416, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.BloodSplatter2, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: PkgName.BloodSplatter3, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 288, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.WoodPlank, frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 32, y: 0, width: 32, height: 32, scale: 0.8f));
            AddFrameList(animPackage: PkgName.Stick, frameList: ConvertImageToFrameList(atlasName: "sticks", layer: 0, x: 26, y: 73, width: 25, height: 21));
            AddFrameList(animPackage: PkgName.Banana, frameList: ConvertImageToFrameList(atlasName: "banana", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.Cherry, frameList: ConvertImageToFrameList(atlasName: "cherry", layer: 0, scale: 0.12f));
            AddFrameList(animPackage: PkgName.Apple, frameList: ConvertImageToFrameList(atlasName: "apple", layer: 0, scale: 0.075f));
            AddFrameList(animPackage: PkgName.BowBasic, frameList: ConvertImageToFrameList(atlasName: "bow_basic", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.BowAdvanced, frameList: ConvertImageToFrameList(atlasName: "bow_advanced", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.SeedBag, frameList: ConvertImageToFrameList(atlasName: "seed_bag", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.CoffeeRaw, frameList: ConvertImageToFrameList(atlasName: "coffee_raw", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.CoffeeRoasted, frameList: ConvertImageToFrameList(atlasName: "coffee_roasted", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.WoodLogRegular, frameList: ConvertImageToFrameList(atlasName: "wood_regular", layer: 1, scale: 0.75f));
            AddFrameList(animPackage: PkgName.WoodLogHard, frameList: ConvertImageToFrameList(atlasName: "wood_hard", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SkullAndBones, frameList: ConvertImageToFrameList(atlasName: "skull_and_bones", layer: 2, scale: 1f));
            AddFrameList(animPackage: PkgName.IronPlate, frameList: ConvertImageToFrameList(atlasName: "iron_plate", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.IronRod, frameList: ConvertImageToFrameList(atlasName: "iron_rod", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.AxeCrystal, frameList: ConvertImageToFrameList(atlasName: "axe_crystal", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Stone, frameList: ConvertImageToFrameList(atlasName: "stone", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Granite, frameList: ConvertImageToFrameList(atlasName: "granite", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.Crystal, frameList: ConvertImageToFrameList(atlasName: "crystal", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.CrystalDepositSmall, frameList: ConvertImageToFrameList(atlasName: "crystal_deposit_small", layer: 1));
            AddFrameList(animPackage: PkgName.CrystalDepositBig, frameList: ConvertImageToFrameList(atlasName: "crystal_deposit_big", layer: 1));
            AddFrameList(animPackage: PkgName.DigSite, frameList: ConvertImageToFrameList(atlasName: "dig_site", layer: 0));
            AddFrameList(animPackage: PkgName.Burger, frameList: ConvertImageToFrameList(atlasName: "burger", layer: 0, scale: 0.07f));
            AddFrameList(animPackage: PkgName.PotionBlack, frameList: ConvertImageToFrameList(atlasName: "potion_black", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionDarkViolet, frameList: ConvertImageToFrameList(atlasName: "potion_dark_violet", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionDarkYellow, frameList: ConvertImageToFrameList(atlasName: "potion_dark_yellow", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionDarkGreen, frameList: ConvertImageToFrameList(atlasName: "potion_dark_green", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionBrown, frameList: ConvertImageToFrameList(atlasName: "potion_brown", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionTransparent, frameList: ConvertImageToFrameList(atlasName: "potion_transparent", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionLightYellow, frameList: ConvertImageToFrameList(atlasName: "bottle_oil", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.WaterDrop, frameList: ConvertImageToFrameList(atlasName: "water_drop", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SeaWave, frameList: ConvertImageToFrameList(atlasName: "wave", layer: 0, scale: 0.5f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog1, frameList: ConvertImageToFrameList(atlasName: "weather_fog_1", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog2, frameList: ConvertImageToFrameList(atlasName: "weather_fog_2", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog3, frameList: ConvertImageToFrameList(atlasName: "weather_fog_3", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog4, frameList: ConvertImageToFrameList(atlasName: "weather_fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog5, frameList: ConvertImageToFrameList(atlasName: "weather_fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog6, frameList: ConvertImageToFrameList(atlasName: "weather_fog_3", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog7, frameList: ConvertImageToFrameList(atlasName: "weather_fog_1", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog8, frameList: ConvertImageToFrameList(atlasName: "weather_fog_2", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.WeatherFog9, frameList: ConvertImageToFrameList(atlasName: "weather_fog_3", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.BubbleExclamationRed, frameList: ConvertImageToFrameList(atlasName: "bubble_exclamation_red", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BubbleExclamationBlue, frameList: ConvertImageToFrameList(atlasName: "bubble_exclamation_blue", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BubbleCraftGreen, frameList: ConvertImageToFrameList(atlasName: "bubble_craft_green", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.PlantPoison, frameList: ConvertImageToFrameList(atlasName: "plant_poison", layer: 0, scale: 0.4f));
            AddFrameList(animPackage: PkgName.PlantPoison, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "plant_poison", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.Map, frameList: ConvertImageToFrameList(atlasName: "map_item", layer: 0, scale: 0.06f, crop: false));
            AddFrameList(animPackage: PkgName.BackpackMediumOutline, frameList: ConvertImageToFrameList(atlasName: "backpack_medium_outline", layer: 1, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BackpackSmall, frameList: ConvertImageToFrameList(atlasName: "backpack_small", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.BackpackMedium, frameList: ConvertImageToFrameList(atlasName: "backpack_medium", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.BackpackBig, frameList: ConvertImageToFrameList(atlasName: "backpack_big", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.BackpackLuxurious, frameList: ConvertImageToFrameList(atlasName: "backpack_luxurious", layer: 1, scale: 0.1f));
            AddFrameList(animPackage: PkgName.Miss, frameList: ConvertImageToFrameList(atlasName: "miss", layer: 2));
            AddFrameList(animPackage: PkgName.Zzz, frameList: ConvertImageToFrameList(atlasName: "zzz", layer: 2));
            AddFrameList(animPackage: PkgName.Heart, frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2));
            AddFrameList(animPackage: PkgName.Hammer, frameList: ConvertImageToFrameList(atlasName: "hammer", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.Fog1, frameList: ConvertImageToFrameList(atlasName: "fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog2, frameList: ConvertImageToFrameList(atlasName: "fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog3, frameList: ConvertImageToFrameList(atlasName: "fog_1", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Fog4, frameList: ConvertImageToFrameList(atlasName: "fog_2", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Empty, frameList: ConvertImageToFrameList(atlasName: "transparent_pixel", layer: 2, crop: false, padding: 0));
            AddFrameList(animPackage: PkgName.Star, frameList: ConvertImageToFrameList(atlasName: "star", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.Crosshair, frameList: ConvertImageToFrameList(atlasName: "crosshair", layer: 2));
            AddFrameList(animPackage: PkgName.ScytheStone, frameList: ConvertImageToFrameList(atlasName: "scythe_stone", layer: 0));
            AddFrameList(animPackage: PkgName.ScytheIron, frameList: ConvertImageToFrameList(atlasName: "scythe_iron", layer: 0));
            AddFrameList(animPackage: PkgName.ScytheCrystal, frameList: ConvertImageToFrameList(atlasName: "scythe_crystal", layer: 0));
            AddFrameList(animPackage: PkgName.EmptyBottle, frameList: ConvertImageToFrameList(atlasName: "bottle_empty", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Acorn, frameList: ConvertImageToFrameList(atlasName: "acorn", layer: 0, scale: 0.13f));
            AddFrameList(animPackage: PkgName.ArrowWood, frameList: ConvertImageToFrameList(atlasName: "arrow_wood", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowStone, frameList: ConvertImageToFrameList(atlasName: "arrow_stone", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowIron, frameList: ConvertImageToFrameList(atlasName: "arrow_iron", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowCrystal, frameList: ConvertImageToFrameList(atlasName: "arrow_crystal", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.KnifeSimple, frameList: ConvertImageToFrameList(atlasName: "knife_simple", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.BeltSmall, frameList: ConvertImageToFrameList(atlasName: "belt_small", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.BeltMedium, frameList: ConvertImageToFrameList(atlasName: "belt_medium", layer: 0, scale: 0.12f));
            AddFrameList(animPackage: PkgName.BeltBig, frameList: ConvertImageToFrameList(atlasName: "belt_big", layer: 0, scale: 0.06f));
            AddFrameList(animPackage: PkgName.BeltLuxurious, frameList: ConvertImageToFrameList(atlasName: "belt_luxurious", layer: 0, scale: 0.06f));
            AddFrameList(animPackage: PkgName.HatSimple, frameList: ConvertImageToFrameList(atlasName: "hat_simple", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.BootsProtective, frameList: ConvertImageToFrameList(atlasName: "boots_protective", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.BootsMountain, frameList: ConvertImageToFrameList(atlasName: "boots_mountain", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.BootsSpeed, frameList: ConvertImageToFrameList(atlasName: "boots_speed", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.BootsAllTerrain, frameList: ConvertImageToFrameList(atlasName: "boots_all_terrain", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.GlovesStrength, frameList: ConvertImageToFrameList(atlasName: "gloves_strength", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.GlassesBlue, frameList: ConvertImageToFrameList(atlasName: "glasses_blue", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.HerbsGreen, frameList: ConvertImageToFrameList(atlasName: "herbs_green", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBlack, frameList: ConvertImageToFrameList(atlasName: "herbs_black", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBlue, frameList: ConvertImageToFrameList(atlasName: "herbs_blue", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsCyan, frameList: ConvertImageToFrameList(atlasName: "herbs_cyan", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsYellow, frameList: ConvertImageToFrameList(atlasName: "herbs_yellow", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsRed, frameList: ConvertImageToFrameList(atlasName: "herbs_red", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsViolet, frameList: ConvertImageToFrameList(atlasName: "herbs_violet", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBrown, frameList: ConvertImageToFrameList(atlasName: "herbs_brown", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsDarkViolet, frameList: ConvertImageToFrameList(atlasName: "herbs_dark_violet", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsDarkGreen, frameList: ConvertImageToFrameList(atlasName: "herbs_dark_green", layer: 0));
            AddFrameList(animPackage: PkgName.SpearWood, frameList: ConvertImageToFrameList(atlasName: "spear_wood", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearStone, frameList: ConvertImageToFrameList(atlasName: "spear_stone", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearIron, frameList: ConvertImageToFrameList(atlasName: "spear_iron", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearCrystal, frameList: ConvertImageToFrameList(atlasName: "spear_crystal", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Fat, frameList: ConvertImageToFrameList(atlasName: "fat", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.ShovelStone, frameList: ConvertImageToFrameList(atlasName: "shovel_stone", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.ShovelIron, frameList: ConvertImageToFrameList(atlasName: "shovel_iron", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.ShovelCrystal, frameList: ConvertImageToFrameList(atlasName: "shovel_crystal", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.Clay, frameList: ConvertImageToFrameList(atlasName: "clay", layer: 0, scale: 0.7f));
            AddFrameList(animPackage: PkgName.Hole, frameList: ConvertImageToFrameList(atlasName: "hole", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.MeatRawRegular, frameList: ConvertImageToFrameList(atlasName: "meat_raw_regular", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.MeatRawPrime, frameList: ConvertImageToFrameList(atlasName: "meat_raw_prime", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.FertileGroundSmall, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_small", layer: -1, scale: 1f));
            AddFrameList(animPackage: PkgName.FertileGroundMedium, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_medium", layer: -1, scale: 1f));
            AddFrameList(animPackage: PkgName.FertileGroundLarge, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_big", layer: -1, scale: 1.3f));
            AddFrameList(animPackage: PkgName.FenceHorizontalShort, frameList: ConvertImageToFrameList(atlasName: "fence_horizontal_short", layer: 1, depthPercent: 0.2f));
            AddFrameList(animPackage: PkgName.FenceVerticalShort, frameList: ConvertImageToFrameList(atlasName: "fence_vertical_short", layer: 1, depthPercent: 0.9f));
            AddFrameList(animPackage: PkgName.FenceHorizontalLong, frameList: ConvertImageToFrameList(atlasName: "fence_horizontal_long", layer: 1, depthPercent: 0.2f));
            AddFrameList(animPackage: PkgName.FenceVerticalLong, frameList: ConvertImageToFrameList(atlasName: "fence_vertical_long", layer: 1, depthPercent: 0.95f));
            AddFrameList(animPackage: PkgName.Totem, frameList: ConvertImageToFrameList(atlasName: "totem", layer: 1, scale: 0.25f, depthPercent: 0.15f));
            AddFrameList(animPackage: PkgName.RuinsColumn, frameList: ConvertImageToFrameList(atlasName: "ruins_column", layer: 1));
            AddFrameList(animPackage: PkgName.RuinsRubble, frameList: ConvertImageToFrameList(atlasName: "ruins_rubble", layer: 1));
            AddFrameList(animPackage: PkgName.RuinsWallHorizontal1, frameList: ConvertImageToFrameList(atlasName: "ruins_wall_horizontal_1", layer: 1));
            AddFrameList(animPackage: PkgName.RuinsWallHorizontal2, frameList: ConvertImageToFrameList(atlasName: "ruins_wall_horizontal_2", layer: 1));
            AddFrameList(animPackage: PkgName.RuinsWallWallVertical, frameList: ConvertImageToFrameList(atlasName: "ruins_wall_vertical", layer: 1, depthPercent: 0.75f));
            AddFrameList(animPackage: PkgName.MeatDried, frameList: ConvertImageToFrameList(atlasName: "meat_dried", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.JarWhole, frameList: ConvertImageToFrameList(atlasName: "jar_sealed", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.JarBroken, frameList: ConvertImageToFrameList(atlasName: "jar_broken", layer: 1, scale: 0.6f));
            AddFrameList(animPackage: PkgName.TreeStump, frameList: ConvertImageToFrameList(atlasName: "tree_stump", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.Carrot, frameList: ConvertImageToFrameList(atlasName: "carrot", layer: 0, scale: 0.08f));
            AddFrameList(animPackage: PkgName.MusicNoteSmall, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2));
            AddFrameList(animPackage: PkgName.MusicNoteBig, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2, scale: 2.5f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerMinus1, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: -1, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerZero, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerOne, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 1, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerTwo, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 2, scale: 1f));
            AddFrameList(animPackage: PkgName.WhiteSpotLayerThree, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 3, scale: 1f));
            AddFrameList(animPackage: PkgName.Upgrade, frameList: ConvertImageToFrameList(atlasName: "upgrade", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Rope, frameList: ConvertImageToFrameList(atlasName: "rope", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.MapMarker, frameList: ConvertImageToFrameList(atlasName: "map_marker", layer: 2, crop: false, padding: 0, ignoreWhenCalculatingMaxSize: true));
            AddFrameList(animPackage: PkgName.Candle, frameList: ConvertImageToFrameList(atlasName: "candle", layer: 0, scale: 0.1f));
            AddFrameList(animPackage: PkgName.Dungarees, frameList: ConvertImageToFrameList(atlasName: "dungarees", layer: 0, scale: 1f));
            AddFrameList(animPackage: PkgName.Mushroom, frameList: ConvertImageToFrameList(atlasName: "mushroom", layer: 0, scale: 0.5f));
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
                    openingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (short)(i < 6 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
                }
                AddFrameList(animPackage: packageName, animName: "opening", frameList: openingFrameList);

                var closingFrameList = new List<AnimFrame>();
                for (int i = 5; i >= 1; i--)
                {
                    closingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (short)(i > 1 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
                }
                AddFrameList(animPackage: packageName, animName: "closing", frameList: closingFrameList);

                AddFrameList(animPackage: packageName, animName: "closed", frameList: ConvertImageToFrameList(atlasName: $"{chestPath}1", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
                AddFrameList(animPackage: packageName, animName: "open", frameList: ConvertImageToFrameList(atlasName: $"{chestPath}6", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
            }

            {
                PkgName packageName = PkgName.Crate;
                AddFrameList(animPackage: packageName,
                frameList: ConvertImageToFrameList(atlasName: "chests", layer: 1, x: 128, y: 0, width: 32, height: 48));
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 96, width: 32, height: 48, duration: 6),
                    ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 48, width: 32, height: 48, duration: 6),
                    ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 0, width: 32, height: 48, duration: 0)
                };
                AddFrameList(animPackage: packageName, animName: "closing", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.WorkshopEssential;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_essential", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_essential", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopBasic;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopAdvanced;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopMaster;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_master", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_master", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopLeatherBasic;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_basic", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_basic", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopLeatherAdvanced;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_advanced", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_advanced", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.WorkshopMeatHarvesting;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_meat_harvesting_off", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_meat_harvesting_on", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.MeatDryingRackRegular;
                float depthPercent = 0.6f;

                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "meat_drying_rack_regular_off", layer: 1, depthPercent: depthPercent));

                for (int i = 1; i <= 4; i++)
                {
                    AddFrameList(animPackage: packageName, animName: $"on_{i}", frameList: ConvertImageToFrameList(atlasName: $"meat_drying_rack_regular_on_{i}", layer: 1, depthPercent: depthPercent));
                }
            }
            {
                PkgName packageName = PkgName.MeatDryingRackWide;
                float depthPercent = 0.6f;

                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "meat_drying_rack_wide_off", layer: 1, depthPercent: depthPercent));

                for (int i = 1; i <= 6; i++)
                {
                    AddFrameList(animPackage: packageName, animName: $"on_{i}", frameList: ConvertImageToFrameList(atlasName: $"meat_drying_rack_wide_on_{i}", layer: 1, depthPercent: depthPercent));
                }
            }
            {
                PkgName packageName = PkgName.AlchemyLabStandard;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_standard", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_standard", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.AlchemyLabAdvanced;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_advanced", layer: 1, scale: 0.5f));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_advanced", layer: 1, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.Furnace;
                AddFrameList(animPackage: packageName, animName: "off",
                frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 0, y: 144, width: 48, height: 48));
                AddFrameList(animPackage: packageName, animName: "on",
                frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 48, y: 144, width: 48, height: 48));
            }
            {
                PkgName packageName = PkgName.Anvil;
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "anvil", layer: 1));
                // the same as "off"
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "anvil", layer: 1));
            }
            {
                PkgName packageName = PkgName.CookingPot;

                AddFrameList(animPackage: packageName, animName: "off",
                    frameList: ConvertImageToFrameList(atlasName: "Candacis_flames1", layer: 1, x: 192, y: 144, width: 32, height: 32));

                var frameListOn = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 0, y: 144, width: 32, height: 32, duration: 6),
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 32, y: 144, width: 32, height: 32, duration: 6),
                    ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 64, y: 144, width: 32, height: 32, duration: 6)
                };
                AddFrameList(animPackage: packageName, animName: "on", frameList: frameListOn);
            }
            {
                PkgName packageName = PkgName.HotPlate;

                AddFrameList(animPackage: packageName, animName: "off",
                    frameList: ConvertImageToFrameList(atlasName: "hot_plate_off", layer: 1));
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "hot_plate_on_1", layer: 1, duration: 6),
                    ConvertImageToFrame(atlasName: "hot_plate_on_2", layer: 1, duration: 6),
                    ConvertImageToFrame(atlasName: "hot_plate_on_3", layer: 1, duration: 6)
                };
                AddFrameList(animPackage: packageName, animName: "on", frameList: frameList);
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
                AddFrameList(animPackage: packageName, frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Attack;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 0, y: 20, width: 23, height: 23),
                    ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 22, y: 13, width: 29, height: 33),
                    ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 50, y: 0, width: 36, height: 65)
                };
                AddFrameList(animPackage: packageName, frameList: frameList);
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
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "small_torch_on", layer: 0, scale: 0.07f));
                AddFrameList(animPackage: packageName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "small_torch_off", layer: 0, scale: 0.07f));
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "small_torch_off", layer: 0, scale: 0.07f));
            }
            {
                PkgName packageName = PkgName.BigTorch;
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "big_torch_on", layer: 0, scale: 0.1f));
                AddFrameList(animPackage: packageName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "big_torch_off", layer: 0, scale: 0.1f));
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "big_torch_off", layer: 0, scale: 0.1f));
            }
            {
                float scale = 0.075f;

                AddFrameList(animPackage: PkgName.LanternFrame, frameList: ConvertImageToFrameList(atlasName: "lantern_frame", layer: 0, scale: scale));

                PkgName packageName = PkgName.Lantern;
                AddFrameList(animPackage: packageName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "lantern_on", layer: 0, scale: scale));
                AddFrameList(animPackage: packageName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "lantern_off", layer: 0, scale: scale));
                AddFrameList(animPackage: packageName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "lantern_off", layer: 0, scale: scale));
            }
            {
                PkgName packageName = PkgName.CampfireSmall;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 288, y: 0, width: 48, height: 48, crop: false),
                    ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 336, y: 0, width: 48, height: 48, crop: false),
                    ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 384, y: 0, width: 48, height: 48, crop: false)
                };
                AddFrameList(animPackage: packageName, animName: "on", frameList: frameList);
                frameList = ConvertImageToFrameList(atlasName: "flames", layer: 1, x: 288, y: 96, width: 48, height: 48, crop: false);
                AddFrameList(animPackage: packageName, frameList);
                AddFrameList(animPackage: packageName, animName: "off", frameList: frameList);
            }

            {
                PkgName packageName = PkgName.CampfireMedium;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "campfire_medium_on_1", layer: 1, duration: 6, crop: false),
                    ConvertImageToFrame(atlasName: "campfire_medium_on_2", layer: 1, duration: 6, crop: false),
                    ConvertImageToFrame(atlasName: "campfire_medium_on_3", layer: 1, duration: 6, crop: false)
                };
                AddFrameList(animPackage: packageName, animName: "on", frameList: frameList);
                frameList = ConvertImageToFrameList(atlasName: "campfire_medium_off", layer: 1, crop: false);
                AddFrameList(animPackage: packageName, frameList);
                AddFrameList(animPackage: packageName, animName: "off", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.CaveEntrance;

                float scale = 2f;
                float depthPercent = 0.95f;

                AddFrameList(animPackage: packageName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "cave_entrance_open", scale: scale, layer: 1, depthPercent: depthPercent));
                AddFrameList(animPackage: packageName, animName: "blocked", frameList: ConvertImageToFrameList(atlasName: "cave_entrance_blocked", scale: scale, layer: 1, depthPercent: depthPercent));

                AddFrameList(animPackage: PkgName.CaveExit, frameList: ConvertImageToFrameList(atlasName: "cave_exit", scale: scale, layer: 1, depthPercent: depthPercent));
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

            {
                float depthPercent = 0.7f;
                float scale = 0.7f;

                for (int animSize = 0; animSize <= 5; animSize++)
                {
                    AddFrameList(animPackage: PkgName.BoatConstruction, frameList: ConvertImageToFrameList(atlasName: $"boat/boat_construction_{animSize}", layer: 1, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true), animSize: animSize);
                }

                AddFrameList(animPackage: PkgName.BoatComplete, frameList: ConvertImageToFrameList(atlasName: "boat/boat_complete", layer: 1, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true));
            }

            AddFrameList(animPackage: PkgName.DigSiteGlass, animName: "default", frameList: new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "dig_site_glass", layer: 0, duration: 450),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_3", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: 0, duration: 1),
                    ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: 0, duration: 1),
                });

            AddFrameList(animPackage: PkgName.DigSite, frameList: ConvertImageToFrameList(atlasName: "dig_site", layer: 0));
            AddFrameList(animPackage: PkgName.DigSiteRuins, frameList: ConvertImageToFrameList(atlasName: "dig_site_ruins", layer: 0, scale: 0.35f));

            AddFrameList(animPackage: PkgName.ArrowExploding, animName: "default", frameList: ConvertImageToFrameList(atlasName: "arrow_burning_off", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowExploding, animName: "burning", frameList: ConvertImageToFrameList(atlasName: "arrow_burning_on", layer: 0, scale: 0.75f));

            AddFrameList(animPackage: PkgName.Clam,
                frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 256, width: 64, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.MealStandard,
                frameList: ConvertImageToFrameList(atlasName: "fancy_food2", layer: 0, x: 288, y: 64, width: 32, height: 32, scale: 0.5f));

            AddFrameList(animPackage: PkgName.AxeWood, frameList: ConvertImageToFrameList(atlasName: "axe_wooden", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));
            AddFrameList(animPackage: PkgName.AxeStone, frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis",
                layer: 0, x: 0, y: 0, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.AxeIron, frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis",
                layer: 0, x: 48, y: 0, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.PickaxeWood,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 0, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeStone,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeIron,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeCrystal,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 384, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.IronDeposit,
                frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 96, y: 96, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.CoalDeposit,
                frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 0, y: 96, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.IronOre,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.GlassSand,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.IronBar,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 96, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Coal,
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 288, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Leather,
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 96, y: 96, width: 32, height: 32, scale: 0.75f));

            AddFrameList(animPackage: PkgName.Nail,
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 0, y: 0, width: 32, height: 32, scale: 0.5f));

            AddFrameList(animPackage: PkgName.TentModern,
                frameList: ConvertImageToFrameList(atlasName: "tent_modern", layer: 1, scale: 0.6f, depthPercent: 0.6f));

            AddFrameList(animPackage: PkgName.TentModernPacked, frameList: ConvertImageToFrameList(atlasName: "tent_modern_packed", layer: 1, scale: 0.12f));

            AddFrameList(animPackage: PkgName.TentSmall, // TODO replace with A - frame tent asset(when found)
                frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 0.5f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.TentMedium,
                frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 1f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.TentBig,
                frameList: ConvertImageToFrameList(atlasName: "tent_big", layer: 1, x: 15, y: 0, width: 191, height: 162, scale: 1f, depthPercent: 0.6f));

            AddFrameList(animPackage: PkgName.HumanSkeleton,
                frameList: ConvertImageToFrameList(atlasName: "tile_rtp-addons", layer: 0, x: 320, y: 160, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.PotionRed,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 384, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionBlue,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 448, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionViolet,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 416, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionYellow,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 352, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionCyan,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 288, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionGreen,
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 320, y: 128, width: 32, height: 32, scale: 0.5f));
        }

        public static List<AnimFrame> ConvertImageToFrameList(string atlasName, int layer, int x = 0, int y = 0, int width = 0, int height = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            List<AnimFrame> frameList = new()
            {
                ConvertImageToFrame(atlasName: atlasName, layer: layer, x: x, y: y, width: width, height: height, crop: crop, duration: 0, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize)
            };
            return frameList;
        }

        public static AnimFrame ConvertImageToFrame(string atlasName, int layer, int x = 0, int y = 0, int width = 0, int height = 0, short duration = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
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
                List<AnimFrame> frameList = new()
                {
                     AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new()
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: (width * 2) + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }

            AddFrameList(animPackage: packageName, animSize: animSize, frameList: new List<AnimFrame> { croppedFramesForPkgs[packageName] }); // adding default frame
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
                List<AnimFrame> frameList = new()
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale)
                };

                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new()
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

            AddFrameList(animPackage: packageName, animSize: animSize, frameList: new List<AnimFrame> { croppedFramesForPkgs[packageName] }); // adding default frame
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
                MessageLog.Add(debugMessage: true, text: $"Failed to purge anim cache: {ex.Message}");
            }

            MessageLog.Add(debugMessage: true, text: "Anim cache purged.");
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

            var usedAtlasNames = new HashSet<string>();

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