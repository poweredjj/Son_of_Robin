using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class AnimData
    {
        public const float currentVersion = 1.000026f; // version number should be incremented when any existing asset is updated

        private static readonly HashSet<PkgName> loadedPkgs = new HashSet<PkgName>();

        public static readonly Dictionary<string, AnimFrame> frameById = new(); // needed to access frames directly by id (for loading and saving game)
        public static readonly Dictionary<string, List<AnimFrame>> frameListById = new();
        private static readonly Dictionary<PkgName, AnimFrame> croppedFramesForPkgs = new(); // default frames for packages (cropped)
        public static readonly Dictionary<PkgName, int> animSizesForPkgs = new(); // information about saved package sizes

        public static readonly Dictionary<string, Texture2D> textureDict = new();
        public static Dictionary<string, Object> jsonDict = new();

        public static AnimFrame GetCroppedFrameForPackage(PkgName pkgName)
        {
            LoadPackage(pkgName);
            if (croppedFramesForPkgs.ContainsKey(pkgName)) return croppedFramesForPkgs[pkgName];

            LoadPackage(PkgName.NoAnim);
            return croppedFramesForPkgs[PkgName.NoAnim];
        }

        public enum PkgName : ushort
        {
            NoAnim = 0,
            Empty = 1,

            WhiteSpotLayerMinus1 = 2,
            WhiteSpotLayerZero = 3,
            WhiteSpotLayerOne = 4,
            WhiteSpotLayerTwo = 5,
            WhiteSpotLayerThree = 6,

            FlowersWhite = 7,
            FlowersYellow1 = 8,
            FlowersYellow2 = 9,
            FlowersRed = 10,

            Rushes = 11,
            GrassDesert = 12,
            GrassRegular = 13,
            PlantPoison = 14,
            CoffeeShrub = 15,
            CarrotPlant = 16,
            TomatoPlant = 17,
            MushroomPlant = 18,

            Cactus = 19,

            PalmTree = 20,
            TreeBig = 21,
            TreeSmall1 = 22,
            TreeSmall2 = 23,

            TreeStump = 24,

            WaterLily1 = 25,
            WaterLily2 = 26,
            WaterLily3 = 27,

            MineralsBig1 = 28,
            MineralsBig2 = 29,
            MineralsBig3 = 30,
            MineralsBig4 = 31,
            MineralsBig5 = 32,
            MineralsBig6 = 33,
            MineralsBig7 = 34,
            MineralsBig8 = 35,
            MineralsBig9 = 36,
            MineralsBig10 = 37,
            MineralsBig11 = 38,
            MineralsBig12 = 39,
            MineralsBig13 = 40,
            MineralsBig14 = 41,

            MineralsSmall1 = 42,
            MineralsSmall2 = 43,
            MineralsSmall3 = 44,
            MineralsSmall4 = 45,

            MineralsMossyBig1 = 46,
            MineralsMossyBig2 = 47,
            MineralsMossyBig3 = 48,
            MineralsMossyBig4 = 49,
            MineralsMossyBig5 = 50,
            MineralsMossyBig6 = 51,
            MineralsMossyBig7 = 52,
            MineralsMossyBig8 = 53,
            MineralsMossyBig9 = 54,
            MineralsMossyBig10 = 55,
            MineralsMossyBig11 = 56,
            MineralsMossyBig12 = 57,

            MineralsMossySmall1 = 58,
            MineralsMossySmall2 = 59,
            MineralsMossySmall3 = 60,
            MineralsMossySmall4 = 61,

            MineralsCave = 62,

            JarWhole = 63,
            JarBroken = 64,

            ChestWooden = 65,
            ChestStone = 66,
            ChestIron = 67,
            ChestCrystal = 68,
            ChestTreasureBlue = 69,
            ChestTreasureRed = 70,

            WoodLogRegular = 71,
            WoodLogHard = 72,
            WoodPlank = 73,

            Nail = 74,
            Rope = 75,
            HideCloth = 76,
            Crate = 77,

            WorkshopEssential = 78,
            WorkshopBasic = 79,
            WorkshopAdvanced = 80,
            WorkshopMaster = 81,

            WorkshopMeatHarvesting = 82,

            WorkshopLeatherBasic = 83,
            WorkshopLeatherAdvanced = 84,

            MeatDryingRackRegular = 85,
            MeatDryingRackWide = 86,

            AlchemyLabStandard = 87,
            AlchemyLabAdvanced = 88,

            Furnace = 89,
            Anvil = 90,
            HotPlate = 91,
            CookingPot = 92,

            Totem = 93,
            RuinsWallHorizontal1 = 94,
            RuinsWallHorizontal2 = 95,
            RuinsWallWallVertical = 96,
            RuinsColumn = 97,
            RuinsRubble = 98,

            Stick = 99,
            Stone = 100,
            Granite = 101,

            Clay = 102,

            Apple = 103,
            Banana = 104,
            Cherry = 105,
            Tomato = 106,
            Carrot = 107,
            Acorn = 108,
            Mushroom = 109,

            SeedBag = 110,
            CoffeeRaw = 111,
            CoffeeRoasted = 112,

            Clam = 113,

            MeatRawRegular = 114,
            MeatRawPrime = 115,
            MeatDried = 116,
            Fat = 117,
            Burger = 118,
            MealStandard = 119,
            Leather = 120,

            KnifeSimple = 121,

            AxeWood = 122,
            AxeStone = 123,
            AxeIron = 124,
            AxeCrystal = 125,

            PickaxeWood = 126,
            PickaxeStone = 127,
            PickaxeIron = 128,
            PickaxeCrystal = 129,

            ScytheStone = 130,
            ScytheIron = 131,
            ScytheCrystal = 132,

            SpearWood = 133,
            SpearStone = 134,
            SpearIron = 135,
            SpearCrystal = 136,

            ShovelStone = 137,
            ShovelIron = 138,
            ShovelCrystal = 139,

            BowBasic = 140,
            BowAdvanced = 141,

            ArrowWood = 142,
            ArrowStone = 143,
            ArrowIron = 144,
            ArrowCrystal = 145,
            ArrowExploding = 146,

            CoalDeposit = 147,
            IronDeposit = 148,

            CrystalDepositSmall = 149,
            CrystalDepositBig = 150,

            DigSite = 151,
            DigSiteGlass = 152,
            DigSiteRuins = 153,

            Coal = 154,
            IronOre = 155,
            IronBar = 156,
            IronRod = 157,
            IronPlate = 158,
            GlassSand = 159,
            Crystal = 160,

            PlayerBoy = 161,
            PlayerGirl = 162,
            PlayerTestDemoness = 163,

            FoxBlack = 164,
            FoxBrown = 165,
            FoxChocolate = 166,
            FoxGinger = 167,
            FoxGray = 168,
            FoxRed = 169,
            FoxWhite = 170,
            FoxYellow = 171,

            Frog1 = 172,
            Frog2 = 173,
            Frog3 = 174,
            Frog4 = 175,
            Frog5 = 176,
            Frog6 = 177,
            Frog7 = 178,
            Frog8 = 179,

            RabbitBeige = 180,
            RabbitBlack = 181,
            RabbitBrown = 182,
            RabbitDarkBrown = 183,
            RabbitGray = 184,
            RabbitLightBrown = 185,
            RabbitLightGray = 186,
            RabbitWhite = 187,

            BearBrown = 188,
            BearWhite = 189,
            BearOrange = 190,
            BearBlack = 191,
            BearDarkBrown = 192,
            BearGray = 193,
            BearRed = 194,
            BearBeige = 195,

            TentModern = 196,
            TentModernPacked = 197,
            TentSmall = 198,
            TentMedium = 199,
            TentBig = 200,

            BackpackSmall = 202,
            BackpackMedium = 203,
            BackpackBig = 204,
            BackpackLuxurious = 205,

            BeltSmall = 206,
            BeltMedium = 207,
            BeltBig = 208,
            BeltLuxurious = 209,
            Map = 210,

            HatSimple = 211,
            BootsProtective = 212,
            BootsMountain = 213,
            BootsSpeed = 214,
            BootsAllTerrain = 215,
            GlovesStrength = 216,
            GlassesBlue = 217,
            Dungarees = 218,

            LanternFrame = 219,
            Candle = 220,
            Lantern = 221,

            SmallTorch = 222,
            BigTorch = 223,
            CampfireSmall = 224,
            CampfireMedium = 225,

            BoatConstruction = 226,
            BoatCompleteStanding = 227,
            BoatCompleteCruising = 228,
            ShipRescue = 229,

            HerbsBlack = 230,
            HerbsCyan = 231,
            HerbsBlue = 232,
            HerbsGreen = 233,
            HerbsYellow = 234,
            HerbsRed = 235,
            HerbsViolet = 236,
            HerbsBrown = 237,
            HerbsDarkViolet = 238,
            HerbsDarkGreen = 239,

            EmptyBottle = 240,
            PotionRed = 241,
            PotionBlue = 242,
            PotionViolet = 243,
            PotionYellow = 244,
            PotionCyan = 245,
            PotionGreen = 246,
            PotionBlack = 247,
            PotionDarkViolet = 248,
            PotionDarkYellow = 249,
            PotionDarkGreen = 250,
            PotionLightYellow = 251,
            PotionTransparent = 252,
            PotionBrown = 253,

            BloodSplatter1 = 254,
            BloodSplatter2 = 255,
            BloodSplatter3 = 256,

            HumanSkeleton = 257,
            Hole = 258,

            Explosion = 259,
            SkullAndBones = 260,
            MusicNoteSmall = 261,
            MusicNoteBig = 262,
            Miss = 263,
            Attack = 264,
            MapMarker = 265,
            Backlight = 266,
            Crosshair = 267,
            Flame = 268,
            Upgrade = 269,
            WaterDrop = 270,
            Zzz = 272,
            Heart = 273,
            Hammer = 274,

            Fog1 = 275,
            Fog2 = 276,
            Fog3 = 277,
            Fog4 = 278,

            BubbleExclamationRed = 279,
            BubbleExclamationBlue = 280,
            BubbleCraftGreen = 281,

            SeaWave = 282,
            WeatherFog1 = 283,
            WeatherFog2 = 284,
            WeatherFog3 = 285,
            WeatherFog4 = 286,
            WeatherFog5 = 287,
            WeatherFog6 = 288,
            WeatherFog7 = 289,
            WeatherFog8 = 290,
            WeatherFog9 = 291,

            FertileGroundSmall = 292,
            FertileGroundMedium = 293,
            FertileGroundLarge = 294,
            FenceHorizontalShort = 295,
            FenceVerticalShort = 296,
            FenceHorizontalLong = 297,
            FenceVerticalLong = 298,

            CaveEntrance = 299,
            CaveExit = 300,

            // obsolete below (kept for compatibility with old saves)

            BackpackMediumOutlineObsolete = 201,
            StarObsolete = 271,
        }

        public static void LoadPackage(PkgName pkgName)
        {
            if (loadedPkgs.Contains(pkgName)) return;

            MessageLog.Add(debugMessage: true, text: $"Loading anim package: {pkgName}");

            switch (pkgName)
            {
                case PkgName.Empty:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "transparent_pixel", layer: 2, crop: false, padding: 0));
                    break;

                case PkgName.NoAnim:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "no_anim", layer: 1, x: 0, y: 0, width: 0, height: 0));
                    break;

                case PkgName.WhiteSpotLayerMinus1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: -1, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerZero:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 0, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerOne:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 1, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerTwo:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 2, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerThree:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "white_spot", layer: 3, scale: 1f));
                    break;

                case PkgName.FlowersWhite:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                    AddFrameList(pkgName: pkgName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 32, y: 352, width: 32, height: 32));
                    break;

                case PkgName.FlowersYellow1:
                    break;

                case PkgName.FlowersYellow2:
                    break;

                case PkgName.FlowersRed:
                    break;

                case PkgName.Rushes:
                    break;

                case PkgName.GrassDesert:
                    break;

                case PkgName.GrassRegular:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                    AddFrameList(pkgName: pkgName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 320, width: 32, height: 32));
                    AddFrameList(pkgName: pkgName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "grass_regular_x3", layer: 1));

                    break;

                case PkgName.PlantPoison:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "plant_poison", layer: 0, scale: 0.4f));
                    AddFrameList(pkgName: pkgName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "plant_poison", layer: 1, scale: 0.6f));
                    break;

                case PkgName.CoffeeShrub:
                    break;

                case PkgName.CarrotPlant:
                    break;

                case PkgName.TomatoPlant:
                    break;

                case PkgName.MushroomPlant:
                    break;

                case PkgName.Cactus:
                    break;

                case PkgName.PalmTree:
                    break;

                case PkgName.TreeBig:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "sapling_tall", layer: 1, scale: 0.5f));
                    AddFrameList(pkgName: pkgName, animSize: 1,
                    frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 199, y: 382, width: 47, height: 66));
                    AddFrameList(pkgName: pkgName, animSize: 2,
                    frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 0, y: 0, width: 63, height: 97));
                    break;

                case PkgName.TreeSmall1:
                    break;

                case PkgName.TreeSmall2:
                    break;

                case PkgName.TreeStump:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tree_stump", layer: 1, scale: 1f));
                    break;

                case PkgName.WaterLily1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 384, y: 64, width: 32, height: 32));
                    break;

                case PkgName.WaterLily2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 416, y: 0, width: 32, height: 32));
                    break;

                case PkgName.WaterLily3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 448, y: 0, width: 32, height: 32));
                    break;

                case PkgName.MineralsBig1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_1", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_2", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_3", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig4:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_4", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig5:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_5", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig6:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_6", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig7:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_7", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig8:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_8", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig9:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_9", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig10:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_10", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig11:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_11", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig12:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_12", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig13:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_13", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig14:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_big_14", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsSmall1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_small_1", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsSmall2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_small_2", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsSmall3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_small_3", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsSmall4:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "minerals_small_4", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsMossyBig1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_1", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_2", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_3", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig4:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_4", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig5:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_5", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig6:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_6", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig7:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_7", layer: 1, scale: 0.28f, depthPercent: 0.4f));

                    break;

                case PkgName.MineralsMossyBig8:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_8", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig9:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_9", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig10:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_10", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig11:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_11", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig12:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_big_12", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossySmall1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_1", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsMossySmall2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_2", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsMossySmall3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_3", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsMossySmall4:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mossy_minerals_small_4", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsCave:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "cave_minerals", layer: 1, scale: 0.3f, depthPercent: 0.75f));
                    break;

                case PkgName.JarWhole:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "jar_sealed", layer: 1, scale: 0.6f));
                    break;

                case PkgName.JarBroken:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "jar_broken", layer: 1, scale: 0.6f));
                    break;

                case PkgName.ChestWooden:
                    break;

                case PkgName.ChestStone:
                    break;

                case PkgName.ChestIron:
                    break;

                case PkgName.ChestCrystal:
                    break;

                case PkgName.ChestTreasureBlue:
                    break;

                case PkgName.ChestTreasureRed:
                    break;

                case PkgName.WoodLogRegular:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "wood_regular", layer: 1, scale: 0.75f));
                    break;

                case PkgName.WoodLogHard:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "wood_hard", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WoodPlank:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 32, y: 0, width: 32, height: 32, scale: 0.8f));
                    break;

                case PkgName.Nail:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 0, y: 0, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.Rope:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "rope", layer: 0, scale: 1f));
                    break;

                case PkgName.HideCloth:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "hidecloth", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Crate:
                    {
                        AddFrameList(pkgName: pkgName,
                        frameList: ConvertImageToFrameList(atlasName: "chests", layer: 1, x: 128, y: 0, width: 32, height: 48));
                        var frameList = new List<AnimFrame>
                        {
                            ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 96, width: 32, height: 48, duration: 6),
                            ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 48, width: 32, height: 48, duration: 6),
                            ConvertImageToFrame(atlasName: "chests", layer: 1, x: 0, y: 0, width: 32, height: 48, duration: 0)
                        };
                        AddFrameList(pkgName: pkgName, animName: "closing", frameList: frameList);
                        break;
                    }

                case PkgName.WorkshopEssential:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_essential", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_essential", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WorkshopBasic:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WorkshopAdvanced:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WorkshopMaster:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_master", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_master", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WorkshopMeatHarvesting:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_meat_harvesting_off", layer: 1, scale: 0.5f));
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_meat_harvesting_on", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WorkshopLeatherBasic:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_basic", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_basic", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WorkshopLeatherAdvanced:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_advanced", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "workshop_leather_advanced", layer: 1, scale: 0.5f));
                    break;

                case PkgName.MeatDryingRackRegular:
                    {
                        float depthPercent = 0.6f;

                        AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "meat_drying_rack_regular_off", layer: 1, depthPercent: depthPercent));

                        for (int i = 1; i <= 4; i++)
                        {
                            AddFrameList(pkgName: pkgName, animName: $"on_{i}", frameList: ConvertImageToFrameList(atlasName: $"meat_drying_rack_regular_on_{i}", layer: 1, depthPercent: depthPercent));
                        }
                        break;
                    }

                case PkgName.MeatDryingRackWide:
                    {
                        float depthPercent = 0.6f;

                        AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "meat_drying_rack_wide_off", layer: 1, depthPercent: depthPercent));

                        for (int i = 1; i <= 6; i++)
                        {
                            AddFrameList(pkgName: pkgName, animName: $"on_{i}", frameList: ConvertImageToFrameList(atlasName: $"meat_drying_rack_wide_on_{i}", layer: 1, depthPercent: depthPercent));
                        }
                        break;
                    }

                case PkgName.AlchemyLabStandard:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_standard", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_standard", layer: 1, scale: 0.5f));
                    break;

                case PkgName.AlchemyLabAdvanced:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_advanced", layer: 1, scale: 0.5f));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "alchemy_lab_advanced", layer: 1, scale: 0.5f));
                    break;

                case PkgName.Furnace:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 0, y: 144, width: 48, height: 48));
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 48, y: 144, width: 48, height: 48));
                    break;

                case PkgName.Anvil:
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "anvil", layer: 1));
                    // the same as "off"
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "anvil", layer: 1));
                    break;

                case PkgName.HotPlate:
                    {
                        AddFrameList(pkgName: pkgName, animName: "off",
          frameList: ConvertImageToFrameList(atlasName: "hot_plate_off", layer: 1));
                        var frameList = new List<AnimFrame>
                        {
                            ConvertImageToFrame(atlasName: "hot_plate_on_1", layer: 1, duration: 6),
                            ConvertImageToFrame(atlasName: "hot_plate_on_2", layer: 1, duration: 6),
                            ConvertImageToFrame(atlasName: "hot_plate_on_3", layer: 1, duration: 6)
                        };
                        AddFrameList(pkgName: pkgName, animName: "on", frameList: frameList);
                        break;
                    }

                case PkgName.CookingPot:
                    {
                        AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "Candacis_flames1", layer: 1, x: 192, y: 144, width: 32, height: 32));

                        var frameListOn = new List<AnimFrame>
                        {
                            ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 0, y: 144, width: 32, height: 32, duration: 6),
                            ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 32, y: 144, width: 32, height: 32, duration: 6),
                            ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 64, y: 144, width: 32, height: 32, duration: 6)
                        };
                        AddFrameList(pkgName: pkgName, animName: "on", frameList: frameListOn);

                        break;
                    }

                case PkgName.Totem:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "totem", layer: 1, scale: 0.25f, depthPercent: 0.15f));
                    break;

                case PkgName.RuinsWallHorizontal1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "ruins_wall_horizontal_1", layer: 1));
                    break;

                case PkgName.RuinsWallHorizontal2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "ruins_wall_horizontal_2", layer: 1));
                    break;

                case PkgName.RuinsWallWallVertical:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "ruins_wall_vertical", layer: 1, depthPercent: 0.75f));
                    break;

                case PkgName.RuinsColumn:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "ruins_column", layer: 1));
                    break;

                case PkgName.RuinsRubble:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "ruins_rubble", layer: 1));
                    break;

                case PkgName.Stick:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "sticks", layer: 0, x: 26, y: 73, width: 25, height: 21));
                    break;

                case PkgName.Stone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "stone", layer: 1, scale: 0.5f));
                    break;

                case PkgName.Granite:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "granite", layer: 1, scale: 1f));
                    break;

                case PkgName.Clay:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "clay", layer: 0, scale: 0.7f));
                    break;

                case PkgName.Apple:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "apple", layer: 0, scale: 0.075f));
                    break;

                case PkgName.Banana:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "banana", layer: 0, scale: 0.08f));
                    break;

                case PkgName.Cherry:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "cherry", layer: 0, scale: 0.12f));
                    break;

                case PkgName.Tomato:
                    break;

                case PkgName.Carrot:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "carrot", layer: 0, scale: 0.08f));
                    break;

                case PkgName.Acorn:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "acorn", layer: 0, scale: 0.13f));
                    break;

                case PkgName.Mushroom:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "mushroom", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SeedBag:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "seed_bag", layer: 0, scale: 0.08f));
                    break;

                case PkgName.CoffeeRaw:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "coffee_raw", layer: 0, scale: 1f));
                    break;

                case PkgName.CoffeeRoasted:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "coffee_roasted", layer: 0, scale: 1f));
                    break;

                case PkgName.Clam:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 256, width: 64, height: 64, scale: 0.5f));
                    break;

                case PkgName.MeatRawRegular:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "meat_raw_regular", layer: 0, scale: 0.1f));
                    break;

                case PkgName.MeatRawPrime:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "meat_raw_prime", layer: 0, scale: 0.1f));
                    break;

                case PkgName.MeatDried:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "meat_dried", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Fat:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fat", layer: 0, scale: 0.08f));
                    break;

                case PkgName.Burger:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "burger", layer: 0, scale: 0.07f));
                    break;

                case PkgName.MealStandard:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fancy_food2", layer: 0, x: 288, y: 64, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.Leather:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 96, y: 96, width: 32, height: 32, scale: 0.75f));
                    break;

                case PkgName.KnifeSimple:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "knife_simple", layer: 1, scale: 1f));
                    break;

                case PkgName.AxeWood:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "axe_wooden", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));
                    break;

                case PkgName.AxeStone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 0, y: 0, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.AxeIron:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 48, y: 0, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.AxeCrystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "axe_crystal", layer: 1, scale: 0.5f));
                    break;

                case PkgName.PickaxeWood:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 0, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.PickaxeStone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.PickaxeIron:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.PickaxeCrystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 384, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.ScytheStone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "scythe_stone", layer: 0));
                    break;

                case PkgName.ScytheIron:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "scythe_iron", layer: 0));
                    break;

                case PkgName.ScytheCrystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "scythe_crystal", layer: 0));
                    break;

                case PkgName.SpearWood:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "spear_wood", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SpearStone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "spear_stone", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SpearIron:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "spear_iron", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SpearCrystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "spear_crystal", layer: 0, scale: 0.5f));
                    break;

                case PkgName.ShovelStone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "shovel_stone", layer: 0, scale: 0.7f));
                    break;

                case PkgName.ShovelIron:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "shovel_iron", layer: 0, scale: 0.7f));
                    break;

                case PkgName.ShovelCrystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "shovel_crystal", layer: 0, scale: 0.7f));
                    break;

                case PkgName.BowBasic:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bow_basic", layer: 0, scale: 0.25f));
                    break;

                case PkgName.BowAdvanced:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bow_advanced", layer: 0, scale: 0.25f));
                    break;

                case PkgName.ArrowWood:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "arrow_wood", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowStone:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "arrow_stone", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowIron:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "arrow_iron", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowCrystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "arrow_crystal", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowExploding:
                    AddFrameList(pkgName: pkgName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "arrow_burning_off", layer: 0, scale: 0.75f));
                    AddFrameList(pkgName: pkgName, animName: "burning", frameList: ConvertImageToFrameList(atlasName: "arrow_burning_on", layer: 0, scale: 0.75f));
                    break;

                case PkgName.CoalDeposit:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 0, y: 96, width: 48, height: 48, scale: 1f));
                    break;

                case PkgName.IronDeposit:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 96, y: 96, width: 48, height: 48, scale: 1f));
                    break;

                case PkgName.CrystalDepositSmall:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "crystal_deposit_small", layer: 1));
                    break;

                case PkgName.CrystalDepositBig:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "crystal_deposit_big", layer: 1));
                    break;

                case PkgName.DigSite:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "dig_site", layer: 0));
                    break;

                case PkgName.DigSiteGlass:
                    AddFrameList(pkgName: pkgName, animName: "default", frameList: new List<AnimFrame>
                    {
                        ConvertImageToFrame(atlasName: "dig_site_glass", layer: 0, duration: 450),
                        ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: 0, duration: 1),
                        ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: 0, duration: 1),
                        ConvertImageToFrame(atlasName: "dig_site_glass_shine_3", layer: 0, duration: 1),
                        ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: 0, duration: 1),
                        ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: 0, duration: 1),
                    });
                    break;

                case PkgName.DigSiteRuins:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "dig_site_ruins", layer: 0, scale: 0.35f));
                    break;

                case PkgName.Coal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 288, y: 144, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.IronOre:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 144, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.IronBar:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 96, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.IronRod:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "iron_rod", layer: 0, scale: 1f));
                    break;

                case PkgName.IronPlate:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "iron_plate", layer: 0, scale: 1f));
                    break;

                case PkgName.GlassSand:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 144, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.Crystal:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "crystal", layer: 1, scale: 0.5f));
                    break;

                case PkgName.PlayerBoy:
                    break;

                case PkgName.PlayerGirl:
                    break;

                case PkgName.PlayerTestDemoness:
                    break;

                case PkgName.FoxBlack:
                    break;

                case PkgName.FoxBrown:
                    break;

                case PkgName.FoxChocolate:
                    break;

                case PkgName.FoxGinger:
                    break;

                case PkgName.FoxGray:
                    break;

                case PkgName.FoxRed:
                    break;

                case PkgName.FoxWhite:
                    break;

                case PkgName.FoxYellow:
                    break;

                case PkgName.Frog1:
                    break;

                case PkgName.Frog2:
                    break;

                case PkgName.Frog3:
                    break;

                case PkgName.Frog4:
                    break;

                case PkgName.Frog5:
                    break;

                case PkgName.Frog6:
                    break;

                case PkgName.Frog7:
                    break;

                case PkgName.Frog8:
                    break;

                case PkgName.RabbitBeige:
                    break;

                case PkgName.RabbitBlack:
                    break;

                case PkgName.RabbitBrown:
                    break;

                case PkgName.RabbitDarkBrown:
                    break;

                case PkgName.RabbitGray:
                    break;

                case PkgName.RabbitLightBrown:
                    break;

                case PkgName.RabbitLightGray:
                    break;

                case PkgName.RabbitWhite:
                    break;

                case PkgName.BearBrown:
                    break;

                case PkgName.BearWhite:
                    break;

                case PkgName.BearOrange:
                    break;

                case PkgName.BearBlack:
                    break;

                case PkgName.BearDarkBrown:
                    break;

                case PkgName.BearGray:
                    break;

                case PkgName.BearRed:
                    break;

                case PkgName.BearBeige:
                    break;

                case PkgName.TentModern:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tent_modern", layer: 1, scale: 0.6f, depthPercent: 0.6f));
                    break;

                case PkgName.TentModernPacked:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tent_modern_packed", layer: 1, scale: 0.12f));
                    break;

                case PkgName.TentSmall:
                    // TODO replace with A - frame tent asset(when found)
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 0.5f, depthPercent: 0.45f));
                    break;

                case PkgName.TentMedium:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 1f, depthPercent: 0.45f));
                    break;

                case PkgName.TentBig:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tent_big", layer: 1, x: 15, y: 0, width: 191, height: 162, scale: 1f, depthPercent: 0.6f));
                    break;

                case PkgName.BackpackSmall:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "backpack_small", layer: 1, scale: 0.1f));
                    break;

                case PkgName.BackpackMedium:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "backpack_medium", layer: 1, scale: 0.5f));
                    break;

                case PkgName.BackpackBig:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "backpack_big", layer: 1, scale: 0.1f));
                    break;

                case PkgName.BackpackLuxurious:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "backpack_luxurious", layer: 1, scale: 0.1f));
                    break;

                case PkgName.BeltSmall:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "belt_small", layer: 0, scale: 1f));
                    break;

                case PkgName.BeltMedium:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "belt_medium", layer: 0, scale: 0.12f));
                    break;

                case PkgName.BeltBig:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "belt_big", layer: 0, scale: 0.06f));
                    break;

                case PkgName.BeltLuxurious:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "belt_luxurious", layer: 0, scale: 0.06f));
                    break;

                case PkgName.Map:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "map_item", layer: 0, scale: 0.06f, crop: false));
                    break;

                case PkgName.HatSimple:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "hat_simple", layer: 0, scale: 0.5f));
                    break;

                case PkgName.BootsProtective:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "boots_protective", layer: 0, scale: 1f));
                    break;

                case PkgName.BootsMountain:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "boots_mountain", layer: 0, scale: 1f));
                    break;

                case PkgName.BootsSpeed:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "boots_speed", layer: 0, scale: 1f));
                    break;

                case PkgName.BootsAllTerrain:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "boots_all_terrain", layer: 0, scale: 1f));
                    break;

                case PkgName.GlovesStrength:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "gloves_strength", layer: 0, scale: 0.7f));
                    break;

                case PkgName.GlassesBlue:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "glasses_blue", layer: 0, scale: 0.5f));
                    break;

                case PkgName.Dungarees:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "dungarees", layer: 0, scale: 1f));
                    break;

                case PkgName.LanternFrame:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "lantern_frame", layer: 0, scale: 0.075f));
                    break;

                case PkgName.Candle:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "candle", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Lantern:
                    {
                        float scale = 0.075f;

                        AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "lantern_on", layer: 0, scale: scale));
                        AddFrameList(pkgName: pkgName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "lantern_off", layer: 0, scale: scale));
                        AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "lantern_off", layer: 0, scale: scale));

                        break;
                    }

                case PkgName.SmallTorch:
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "small_torch_on", layer: 0, scale: 0.07f));
                    AddFrameList(pkgName: pkgName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "small_torch_off", layer: 0, scale: 0.07f));
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "small_torch_off", layer: 0, scale: 0.07f));
                    break;

                case PkgName.BigTorch:
                    AddFrameList(pkgName: pkgName, animName: "on", frameList: ConvertImageToFrameList(atlasName: "big_torch_on", layer: 0, scale: 0.1f));
                    AddFrameList(pkgName: pkgName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "big_torch_off", layer: 0, scale: 0.1f));
                    AddFrameList(pkgName: pkgName, animName: "off", frameList: ConvertImageToFrameList(atlasName: "big_torch_off", layer: 0, scale: 0.1f));
                    break;

                case PkgName.CampfireSmall:
                    {
                        var frameList = new List<AnimFrame>
                        {
                            ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 288, y: 0, width: 48, height: 48, crop: false),
                            ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 336, y: 0, width: 48, height: 48, crop: false),
                            ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 384, y: 0, width: 48, height: 48, crop: false)
                        };
                        AddFrameList(pkgName: pkgName, animName: "on", frameList: frameList);
                        frameList = ConvertImageToFrameList(atlasName: "flames", layer: 1, x: 288, y: 96, width: 48, height: 48, crop: false);
                        AddFrameList(pkgName: pkgName, frameList);
                        AddFrameList(pkgName: pkgName, animName: "off", frameList: frameList);

                        break;
                    }

                case PkgName.CampfireMedium:
                    break;

                case PkgName.BoatConstruction:
                    break;

                case PkgName.BoatCompleteStanding:
                    break;

                case PkgName.BoatCompleteCruising:
                    break;

                case PkgName.ShipRescue:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "ship_rescue", layer: 1, scale: 1.5f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.HerbsBlack:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_black", layer: 0));
                    break;

                case PkgName.HerbsCyan:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_cyan", layer: 0));
                    break;

                case PkgName.HerbsBlue:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_blue", layer: 0));
                    break;

                case PkgName.HerbsGreen:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_green", layer: 0));
                    break;

                case PkgName.HerbsYellow:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_yellow", layer: 0));
                    break;

                case PkgName.HerbsRed:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_red", layer: 0));
                    break;

                case PkgName.HerbsViolet:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_violet", layer: 0));
                    break;

                case PkgName.HerbsBrown:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_brown", layer: 0));
                    break;

                case PkgName.HerbsDarkViolet:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_dark_violet", layer: 0));
                    break;

                case PkgName.HerbsDarkGreen:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "herbs_dark_green", layer: 0));
                    break;

                case PkgName.EmptyBottle:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bottle_empty", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionRed:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 384, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionBlue:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 448, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionViolet:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 416, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionYellow:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 352, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionCyan:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 288, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionGreen:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 320, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionBlack:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "potion_black", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionDarkViolet:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "potion_dark_violet", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionDarkYellow:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "potion_dark_yellow", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionDarkGreen:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "potion_dark_green", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionLightYellow:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bottle_oil", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionTransparent:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "potion_transparent", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionBrown:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "potion_brown", layer: 0, scale: 0.5f));
                    break;

                case PkgName.BloodSplatter1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 416, y: 320, width: 32, height: 32));
                    break;

                case PkgName.BloodSplatter2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 320, width: 32, height: 32));
                    break;

                case PkgName.BloodSplatter3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 288, width: 32, height: 32));
                    break;

                case PkgName.HumanSkeleton:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "tile_rtp-addons", layer: 0, x: 320, y: 160, width: 32, height: 32));
                    break;

                case PkgName.Hole:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "hole", layer: 0, scale: 1f));
                    break;

                case PkgName.Explosion:
                    break;

                case PkgName.SkullAndBones:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "skull_and_bones", layer: 2, scale: 1f));
                    break;

                case PkgName.MusicNoteSmall:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2));
                    break;

                case PkgName.MusicNoteBig:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "music_note", layer: 2, scale: 2.5f));
                    break;

                case PkgName.Miss:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "miss", layer: 2));
                    break;

                case PkgName.Attack:
                    {
                        var frameList = new List<AnimFrame>
                        {
                            ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 0, y: 20, width: 23, height: 23),
                            ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 22, y: 13, width: 29, height: 33),
                            ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 50, y: 0, width: 36, height: 65)
                        };
                        AddFrameList(pkgName: pkgName, frameList: frameList);

                        break;
                    }

                case PkgName.MapMarker:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "map_marker", layer: 2, crop: false, padding: 0, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.Backlight:
                    {
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
                        AddFrameList(pkgName: pkgName, frameList: frameList);
                        break;
                    }

                case PkgName.Crosshair:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "crosshair", layer: 2));
                    break;

                case PkgName.Flame:
                    {
                        byte animSize = 0;
                        foreach (float scale in new List<float> { 0.5f, 0.75f, 1f, 1.25f })
                        {
                            var frameList = new List<AnimFrame>
                            {
                                ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 432, y: 48, width: 48, height: 48, crop: true, scale: scale),
                                ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 480, y: 48, width: 48, height: 48, crop: true, scale: scale),
                                ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 528, y: 48, width: 48, height: 48, crop: true, scale: scale)
                            };
                            AddFrameList(pkgName: pkgName, animSize: animSize, frameList: frameList);

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
                            AddFrameList(pkgName: pkgName, animSize: animSize, frameList: frameList);

                            animSize++;
                        }

                        break;
                    }

                case PkgName.Upgrade:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "upgrade", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WaterDrop:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "water_drop", layer: 0, scale: 0.5f));
                    break;

                case PkgName.Zzz:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "zzz", layer: 2));
                    break;

                case PkgName.Heart:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2));
                    break;

                case PkgName.Hammer:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "hammer", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Fog1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.Fog2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));

                    break;

                case PkgName.Fog3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fog_1", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.Fog4:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fog_2", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.BubbleExclamationRed:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bubble_exclamation_red", layer: 2, scale: 0.2f));
                    break;

                case PkgName.BubbleExclamationBlue:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bubble_exclamation_blue", layer: 2, scale: 0.2f));
                    break;

                case PkgName.BubbleCraftGreen:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "bubble_craft_green", layer: 2, scale: 0.2f));
                    break;

                case PkgName.SeaWave:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "wave", layer: 0, scale: 0.5f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog1:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_1", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog2:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_2", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog3:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_3", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog4:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog5:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog6:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_3", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog7:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_1", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog8:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_2", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog9:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "weather_fog_3", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.FertileGroundSmall:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_small", layer: -1, scale: 1f));
                    break;

                case PkgName.FertileGroundMedium:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_medium", layer: -1, scale: 1f));
                    break;

                case PkgName.FertileGroundLarge:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fertile_ground_big", layer: -1, scale: 1.3f));
                    break;

                case PkgName.FenceHorizontalShort:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fence_horizontal_short", layer: 1, depthPercent: 0.2f));
                    break;

                case PkgName.FenceVerticalShort:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fence_vertical_short", layer: 1, depthPercent: 0.9f));
                    break;

                case PkgName.FenceHorizontalLong:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fence_horizontal_long", layer: 1, depthPercent: 0.2f));
                    break;

                case PkgName.FenceVerticalLong:
                    AddFrameList(pkgName: pkgName, frameList: ConvertImageToFrameList(atlasName: "fence_vertical_long", layer: 1, depthPercent: 0.95f));
                    break;

                case PkgName.CaveEntrance:
                    break;

                case PkgName.CaveExit:
                    break;

                default:
                    throw new ArgumentException($"Unsupported pkgName - {pkgName}.");
            }

            loadedPkgs.Add(pkgName);
        }

        public static void AddFrameList(PkgName pkgName, List<AnimFrame> frameList, int animSize = 0, string animName = "default")
        {
            if (!animSizesForPkgs.ContainsKey(pkgName) || animSizesForPkgs[pkgName] < animSize)
            {
                croppedFramesForPkgs[pkgName] = frameList[0].GetCroppedFrameCopy();
                animSizesForPkgs[pkgName] = animSize;
            }

            string completeAnimID = $"{pkgName}-{animSize}-{animName}";
            frameListById[completeAnimID] = new List<AnimFrame>(frameList);
        }

        // animation loading should be split in multiple methods, to avoid loading them all at once

        public static void CreateAnimsPlants()
        {
            {
                PkgName packageName = PkgName.GrassDesert;
                AddFrameList(pkgName: packageName,
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 160, y: 224, width: 24, height: 24));
                AddFrameList(pkgName: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 288, y: 160, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersYellow1;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.MushroomPlant;
                AddFrameList(pkgName: packageName, animSize: 0, frameList: ConvertImageToFrameList(atlasName: "mushroom_plant", layer: 1, scale: 0.6f));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "mushroom_plant", layer: 1, scale: 0.8f));
                AddFrameList(pkgName: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "mushroom_plant", layer: 1, scale: 1.0f));
            }
            {
                PkgName packageName = PkgName.FlowersYellow2;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x",
                    layer: 0, x: 832, y: 128, width: 64, height: 64, scale: 0.5f));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x",
                    layer: 1, x: 768, y: 0, width: 64, height: 64, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.FlowersRed;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 64, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Rushes;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 96, y: 192, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Cactus;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 352, y: 320, width: 32, height: 32));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 288, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.PalmTree;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "palmtree_small", layer: 1, scale: 0.7f));
                AddFrameList(pkgName: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 123, y: 326, width: 69, height: 80, crop: false));
                AddFrameList(pkgName: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 8, y: 145, width: 66, height: 102, crop: false));
                AddFrameList(pkgName: packageName, animSize: 3,
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 108, y: 145, width: 72, height: 102, crop: false));
            }
            {
                PkgName packageName = PkgName.TreeSmall1;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "sapling_short", layer: 1, scale: 0.5f));
                AddFrameList(pkgName: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 192, y: 352, width: 32, height: 32));
                AddFrameList(pkgName: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 160, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeSmall2;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "sapling_short", layer: 1, scale: 0.5f));
                AddFrameList(pkgName: packageName, animSize: 1,
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 128, y: 192, width: 32, height: 32));
                AddFrameList(pkgName: packageName, animSize: 2,
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 160, y: 192, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TomatoPlant;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_small", layer: 1, scale: 0.1f));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_medium", layer: 1, scale: 0.08f));
                AddFrameList(pkgName: packageName, animSize: 2, frameList: ConvertImageToFrameList(atlasName: "tomato_plant_big", layer: 1, scale: 0.08f));
            }
            AddFrameList(pkgName: PkgName.Tomato, frameList: ConvertImageToFrameList(atlasName: "tomato", layer: 0, scale: 0.07f));
            {
                PkgName packageName = PkgName.CoffeeShrub;
                AddFrameList(pkgName: packageName, frameList: ConvertImageToFrameList(atlasName: "coffee_shrub", layer: 1, scale: 0.06f));
                AddFrameList(pkgName: packageName, animSize: 1, frameList: ConvertImageToFrameList(atlasName: "coffee_shrub", layer: 1, scale: 0.06f));
            }
            AddFrameList(pkgName: PkgName.CarrotPlant, animName: "default", frameList: ConvertImageToFrameList(atlasName: "carrot_plant_empty", layer: 1, scale: 0.1f));
            AddFrameList(pkgName: PkgName.CarrotPlant, animName: "has_fruits", frameList: ConvertImageToFrameList(atlasName: "carrot_plant_has_carrot", layer: 1, scale: 0.1f)); // using different plant graphics when carrot is present, instead of drawing the carrot separately (because the carrot should be underground)
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
                AddFrameList(pkgName: packageName, animName: "opening", frameList: openingFrameList);

                var closingFrameList = new List<AnimFrame>();
                for (int i = 5; i >= 1; i--)
                {
                    closingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (short)(i > 1 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
                }
                AddFrameList(pkgName: packageName, animName: "closing", frameList: closingFrameList);

                AddFrameList(pkgName: packageName, animName: "closed", frameList: ConvertImageToFrameList(atlasName: $"{chestPath}1", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
                AddFrameList(pkgName: packageName, animName: "open", frameList: ConvertImageToFrameList(atlasName: $"{chestPath}6", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
            }

            {
                PkgName packageName = PkgName.CampfireMedium;
                var frameList = new List<AnimFrame>
                {
                    ConvertImageToFrame(atlasName: "campfire_medium_on_1", layer: 1, duration: 6, crop: false),
                    ConvertImageToFrame(atlasName: "campfire_medium_on_2", layer: 1, duration: 6, crop: false),
                    ConvertImageToFrame(atlasName: "campfire_medium_on_3", layer: 1, duration: 6, crop: false)
                };
                AddFrameList(pkgName: packageName, animName: "on", frameList: frameList);
                frameList = ConvertImageToFrameList(atlasName: "campfire_medium_off", layer: 1, crop: false);
                AddFrameList(pkgName: packageName, frameList);
                AddFrameList(pkgName: packageName, animName: "off", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.CaveEntrance;

                float scale = 2f;
                float depthPercent = 0.95f;

                AddFrameList(pkgName: packageName, animName: "default", frameList: ConvertImageToFrameList(atlasName: "cave_entrance_open", scale: scale, layer: 1, depthPercent: depthPercent));
                AddFrameList(pkgName: packageName, animName: "blocked", frameList: ConvertImageToFrameList(atlasName: "cave_entrance_blocked", scale: scale, layer: 1, depthPercent: depthPercent));

                AddFrameList(pkgName: PkgName.CaveExit, frameList: ConvertImageToFrameList(atlasName: "cave_exit", scale: scale, layer: 1, depthPercent: depthPercent));
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

                    AddFrameList(pkgName: packageName, animSize: size, animName: "default", frameList: frameList);
                }
            }

            {
                float depthPercent = 0.7f;
                float scale = 0.7f;

                for (int animSize = 0; animSize <= 5; animSize++)
                {
                    AddFrameList(pkgName: PkgName.BoatConstruction, frameList: ConvertImageToFrameList(atlasName: $"boat/boat_construction_{animSize}", layer: 1, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true, crop: false), animSize: animSize);
                }

                AddFrameList(pkgName: PkgName.BoatCompleteStanding, frameList: ConvertImageToFrameList(atlasName: "boat/boat_complete", layer: 1, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true, crop: false));

                AddFrameList(pkgName: PkgName.BoatCompleteCruising, frameList: ConvertImageToFrameList(atlasName: "boat/boat_complete", layer: 0, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true, crop: false));
            }
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

                AddFrameList(pkgName: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                List<AnimFrame> frameList = new()
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: (width * 2) + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale)
                };

                AddFrameList(pkgName: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }

            AddFrameList(pkgName: packageName, animSize: animSize, frameList: new List<AnimFrame> { croppedFramesForPkgs[packageName] }); // adding default frame
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

                AddFrameList(pkgName: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
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

                AddFrameList(pkgName: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }

            AddFrameList(pkgName: packageName, animSize: animSize, frameList: new List<AnimFrame> { croppedFramesForPkgs[packageName] }); // adding default frame
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