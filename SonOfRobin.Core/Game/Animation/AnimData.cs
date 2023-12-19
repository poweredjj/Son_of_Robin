using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class AnimData
    {
        public const float currentVersion = 1.000034f; // version number should be incremented when any existing asset is updated

        public static readonly PkgName[] allPkgNames = (PkgName[])Enum.GetValues(typeof(PkgName));
        public static HashSet<PkgName> LoadedPkgs { get; private set; } = new HashSet<PkgName>();

        public static HashSet<string> foundFramePngs = new DirectoryInfo(SonOfRobinGame.animCachePath).GetFiles().Select(f => f.Name).Where(f => f.EndsWith(".png")).ToHashSet();

        public static readonly Dictionary<string, AnimFrame> frameById = new(); // needed to access frames directly by id (for loading and saving game)
        public static readonly Dictionary<string, AnimFrame[]> frameArrayById = new();
        private static readonly Dictionary<PkgName, AnimFrame> croppedFramesForPkgs = new(); // default frames for packages (cropped)
        public static readonly Dictionary<PkgName, int> animSizesForPkgs = new(); // information about saved package sizes
        public static int loadedFramesCount = 0;

        public static readonly Dictionary<string, Texture2D> textureDict = new();
        public static Dictionary<string, object> jsonDict = new();

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

            FurnaceConstructionSite = 271,
            FurnaceComplete = 89,
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

            DragonBonesTest1 = 301,

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
        }

        public static bool LoadPackage(PkgName pkgName)
        {
            if (LoadedPkgs.Contains(pkgName)) return false;

            // MessageLog.Add(debugMessage: true, text: $"Loading anim package: {pkgName}");

            switch (pkgName)
            {
                case PkgName.Empty:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "transparent_pixel", layer: 2, crop: false, padding: 0));
                    break;

                case PkgName.NoAnim:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "no_anim", layer: 2));
                    break;

                case PkgName.WhiteSpotLayerMinus1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "white_spot", layer: -1, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerZero:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "white_spot", layer: 0, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerOne:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "white_spot", layer: 1, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerTwo:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "white_spot", layer: 2, scale: 1f));
                    break;

                case PkgName.WhiteSpotLayerThree:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "white_spot", layer: 3, scale: 1f));
                    break;

                case PkgName.FlowersWhite:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                    AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 32, y: 352, width: 32, height: 32));
                    break;

                case PkgName.FlowersYellow1:
                    {
                        int layer = 0;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: layer, x: 0, y: 320, width: 32, height: 32));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: layer, x: 0, y: 352, width: 32, height: 32));
                        break;
                    }

                case PkgName.FlowersYellow2:
                    {
                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 832, y: 128, width: 64, height: 64, scale: 0.5f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 768, y: 0, width: 64, height: 64, scale: 0.5f));
                        break;
                    }

                case PkgName.FlowersRed:
                    {
                        int layer = 0;
                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: layer, x: 0, y: 320, width: 32, height: 32));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: layer, x: 64, y: 352, width: 32, height: 32));
                        break;
                    }

                case PkgName.Rushes:
                    {
                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_19ba32a6", layer: 1, x: 96, y: 192, width: 32, height: 32));
                        break;
                    }

                case PkgName.GrassDesert:
                    {
                        int layer = 0;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "flowers", layer: layer, x: 160, y: 224, width: 24, height: 24));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "flowers", layer: layer, x: 288, y: 160, width: 32, height: 32));
                        break;
                    }

                case PkgName.GrassRegular:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                    AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 1, x: 32, y: 320, width: 32, height: 32));
                    AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "grass_regular_x3", layer: 1));

                    break;

                case PkgName.PlantPoison:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "plant_poison", layer: 0, scale: 0.4f));
                    AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "plant_poison", layer: 1, scale: 0.6f));
                    break;

                case PkgName.CoffeeShrub:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "coffee_shrub", layer: layer, scale: 0.06f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "coffee_shrub", layer: layer, scale: 0.06f));
                        break;
                    }

                case PkgName.CarrotPlant:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "carrot_plant_empty", layer: layer, scale: 0.1f));
                        AddFrameArray(pkgName: pkgName, animName: "has_fruits", frameArray: ConvertImageToFrameArray(atlasName: "carrot_plant_has_carrot", layer: layer, scale: 0.1f)); // using different plant graphics when carrot is present, instead of drawing the carrot separately (because the carrot should be underground)
                        break;
                    }

                case PkgName.TomatoPlant:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tomato_plant_small", layer: layer, scale: 0.1f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tomato_plant_medium", layer: layer, scale: 0.08f));
                        AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "tomato_plant_big", layer: layer, scale: 0.08f));

                        break;
                    }

                case PkgName.MushroomPlant:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animSize: 0, frameArray: ConvertImageToFrameArray(atlasName: "mushroom_plant", layer: layer, scale: 0.6f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "mushroom_plant", layer: layer, scale: 0.8f));
                        AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "mushroom_plant", layer: layer, scale: 1.0f));
                        break;
                    }

                case PkgName.Cactus:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "qYFvsmq", layer: layer, x: 352, y: 320, width: 32, height: 32));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "qYFvsmq", layer: layer, x: 288, y: 320, width: 32, height: 64));
                        break;
                    }

                case PkgName.PalmTree:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "palmtree_small", layer: layer, scale: 0.7f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: layer, x: 123, y: 326, width: 69, height: 80, crop: false));
                        AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: layer, x: 8, y: 145, width: 66, height: 102, crop: false));
                        AddFrameArray(pkgName: pkgName, animSize: 3, frameArray: ConvertImageToFrameArray(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: layer, x: 108, y: 145, width: 72, height: 102, crop: false));
                        break;
                    }

                case PkgName.TreeBig:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "sapling_tall", layer: layer, scale: 0.5f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_19ba32a6", layer: layer, x: 199, y: 382, width: 47, height: 66));
                        AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "tile_19ba32a6", layer: layer, x: 0, y: 0, width: 63, height: 97));
                        break;
                    }

                case PkgName.TreeSmall1:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "sapling_short", layer: layer, scale: 0.5f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: layer, x: 192, y: 352, width: 32, height: 32));
                        AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: layer, x: 160, y: 320, width: 32, height: 64));
                        break;
                    }

                case PkgName.TreeSmall2:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "sapling_short", layer: layer, scale: 0.5f));
                        AddFrameArray(pkgName: pkgName, animSize: 1, frameArray: ConvertImageToFrameArray(atlasName: "tileb", layer: layer, x: 128, y: 192, width: 32, height: 32));
                        AddFrameArray(pkgName: pkgName, animSize: 2, frameArray: ConvertImageToFrameArray(atlasName: "tileb", layer: layer, x: 160, y: 192, width: 32, height: 64));
                        break;
                    }

                case PkgName.TreeStump:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tree_stump", layer: 1, scale: 1f));
                    break;

                case PkgName.WaterLily1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_19ba32a6", layer: 0, x: 384, y: 64, width: 32, height: 32));
                    break;

                case PkgName.WaterLily2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_19ba32a6", layer: 0, x: 416, y: 0, width: 32, height: 32));
                    break;

                case PkgName.WaterLily3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_19ba32a6", layer: 0, x: 448, y: 0, width: 32, height: 32));
                    break;

                case PkgName.MineralsBig1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_1", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_2", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_3", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig4:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_4", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig5:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_5", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig6:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_6", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig7:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_7", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig8:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_8", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig9:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_9", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig10:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_10", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig11:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_11", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig12:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_12", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig13:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_13", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsBig14:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_big_14", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsSmall1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_small_1", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsSmall2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_small_2", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsSmall3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_small_3", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsSmall4:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "minerals_small_4", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                    break;

                case PkgName.MineralsMossyBig1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_1", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_2", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_3", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig4:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_4", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig5:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_5", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig6:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_6", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig7:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_7", layer: 1, scale: 0.28f, depthPercent: 0.4f));

                    break;

                case PkgName.MineralsMossyBig8:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_8", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig9:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_9", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig10:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_10", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig11:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_11", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossyBig12:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_big_12", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                    break;

                case PkgName.MineralsMossySmall1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_small_1", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsMossySmall2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_small_2", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsMossySmall3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_small_3", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsMossySmall4:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mossy_minerals_small_4", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                    break;

                case PkgName.MineralsCave:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "cave_minerals", layer: 1, scale: 0.3f, depthPercent: 0.75f));
                    break;

                case PkgName.JarWhole:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "jar_sealed", layer: 1, scale: 0.6f));
                    break;

                case PkgName.JarBroken:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "jar_broken", layer: 1, scale: 0.6f));
                    break;

                case PkgName.ChestWooden:
                    AddChestPackage(pkgName);
                    break;

                case PkgName.ChestStone:
                    AddChestPackage(pkgName);
                    break;

                case PkgName.ChestIron:
                    AddChestPackage(pkgName);
                    break;

                case PkgName.ChestCrystal:
                    AddChestPackage(pkgName);
                    break;

                case PkgName.ChestTreasureBlue:
                    AddChestPackage(pkgName);
                    break;

                case PkgName.ChestTreasureRed:
                    AddChestPackage(pkgName);
                    break;

                case PkgName.WoodLogRegular:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "wood_regular", layer: 1, scale: 0.75f));
                    break;

                case PkgName.WoodLogHard:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "wood_hard", layer: 1, scale: 0.5f));
                    break;

                case PkgName.WoodPlank:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "craft_items", layer: 0, x: 32, y: 0, width: 32, height: 32, scale: 0.8f));
                    break;

                case PkgName.Nail:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "craft_items", layer: 0, x: 0, y: 0, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.Rope:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "rope", layer: 0, scale: 1f));
                    break;

                case PkgName.HideCloth:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "hidecloth", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Crate:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName,
                        frameArray: ConvertImageToFrameArray(atlasName: "chests", layer: layer, x: 128, y: 0, width: 32, height: 48));
                        var frameArray = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "chests", layer: layer, x: 0, y: 96, width: 32, height: 48, duration: 6),
                            ConvertImageToFrame(atlasName: "chests", layer: layer, x: 0, y: 48, width: 32, height: 48, duration: 6),
                            ConvertImageToFrame(atlasName: "chests", layer: layer, x: 0, y: 0, width: 32, height: 48, duration: 0)
                        };
                        AddFrameArray(pkgName: pkgName, animName: "closing", frameArray: frameArray);
                        break;
                    }

                case PkgName.WorkshopEssential:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_essential", layer: layer, scale: 0.5f));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_essential", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.WorkshopBasic:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_basic", layer: layer, scale: 0.5f));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_basic", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.WorkshopAdvanced:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_advanced", layer: layer, scale: 0.5f));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_advanced", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.WorkshopMaster:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_master", layer: layer, scale: 0.5f));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_master", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.WorkshopMeatHarvesting:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_meat_harvesting_off", layer: layer, scale: 0.5f));
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_meat_harvesting_on", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.WorkshopLeatherBasic:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_leather_basic", layer: layer, scale: 0.5f));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_leather_basic", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.WorkshopLeatherAdvanced:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "workshop_leather_advanced", layer: layer, scale: 0.5f));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "workshop_leather_advanced", layer: layer, scale: 0.5f));
                        break;
                    }

                case PkgName.MeatDryingRackRegular:
                    {
                        float depthPercent = 0.6f;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "meat_drying_rack_regular_off", layer: 1, depthPercent: depthPercent));

                        for (int i = 1; i <= 4; i++)
                        {
                            AddFrameArray(pkgName: pkgName, animName: $"on_{i}", frameArray: ConvertImageToFrameArray(atlasName: $"meat_drying_rack_regular_on_{i}", layer: 1, depthPercent: depthPercent));
                        }
                        break;
                    }

                case PkgName.MeatDryingRackWide:
                    {
                        float depthPercent = 0.6f;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "meat_drying_rack_wide_off", layer: 1, depthPercent: depthPercent));

                        for (int i = 1; i <= 6; i++)
                        {
                            AddFrameArray(pkgName: pkgName, animName: $"on_{i}", frameArray: ConvertImageToFrameArray(atlasName: $"meat_drying_rack_wide_on_{i}", layer: 1, depthPercent: depthPercent));
                        }
                        break;
                    }

                case PkgName.AlchemyLabStandard:
                    {
                        int layer = 1;
                        float scale = 0.5f;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "alchemy_lab_standard", layer: layer, scale: scale));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "alchemy_lab_standard", layer: layer, scale: scale));
                        break;
                    }

                case PkgName.AlchemyLabAdvanced:
                    {
                        int layer = 1;
                        float scale = 0.5f;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "alchemy_lab_advanced", layer: layer, scale: scale));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "alchemy_lab_advanced", layer: layer, scale: scale));
                        break;
                    }

                case PkgName.FurnaceConstructionSite:
                    {
                        float scale = 0.2f;

                        for (int animSize = 0; animSize <= 2; animSize++)
                        {
                            AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: $"furnace/furnace_construction_{animSize}", layer: 1, scale: scale, crop: false), animSize: animSize, updateCroppedFramesForPkgs: animSize == 0); // animSize == 0 should serve as an example (whole blueprint visible)
                        }

                        break;
                    }

                case PkgName.FurnaceComplete:
                    {
                        int layer = 1;
                        float scale = 0.2f;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "furnace/furnace_off", layer: layer, scale: scale, crop: false));
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "furnace/furnace_on", layer: layer, scale: scale, crop: false));
                        break;
                    }

                case PkgName.Anvil:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "anvil", layer: layer));
                        // the same as "off"
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "anvil", layer: layer));
                        break;
                    }

                case PkgName.HotPlate:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "hot_plate_off", layer: layer));
                        var frameArray = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "hot_plate_on_1", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "hot_plate_on_2", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "hot_plate_on_3", layer: layer, duration: 6)
                        };
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArray);
                        break;
                    }

                case PkgName.CookingPot:
                    {
                        int layer = 1;

                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "Candacis_flames1", layer: layer, x: 192, y: 144, width: 32, height: 32));

                        var frameArrayOn = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "Candacis_flames1", layer: layer, x: 0, y: 144, width: 32, height: 32, duration: 6),
                            ConvertImageToFrame(atlasName: "Candacis_flames1", layer: layer, x: 32, y: 144, width: 32, height: 32, duration: 6),
                            ConvertImageToFrame(atlasName: "Candacis_flames1", layer: layer, x: 64, y: 144, width: 32, height: 32, duration: 6)
                        };
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArrayOn);

                        break;
                    }

                case PkgName.Totem:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "totem", layer: 1, scale: 0.25f, depthPercent: 0.15f));
                    break;

                case PkgName.RuinsWallHorizontal1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "ruins_wall_horizontal_1", layer: 1));
                    break;

                case PkgName.RuinsWallHorizontal2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "ruins_wall_horizontal_2", layer: 1));
                    break;

                case PkgName.RuinsWallWallVertical:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "ruins_wall_vertical", layer: 1, depthPercent: 0.75f));
                    break;

                case PkgName.RuinsColumn:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "ruins_column", layer: 1));
                    break;

                case PkgName.RuinsRubble:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "ruins_rubble", layer: 1));
                    break;

                case PkgName.Stick:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "sticks", layer: 0, x: 26, y: 73, width: 25, height: 21));
                    break;

                case PkgName.Stone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "stone", layer: 1, scale: 0.5f));
                    break;

                case PkgName.Granite:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "granite", layer: 1, scale: 1f));
                    break;

                case PkgName.Clay:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "clay", layer: 0, scale: 0.7f));
                    break;

                case PkgName.Apple:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "apple", layer: 0, scale: 0.075f));
                    break;

                case PkgName.Banana:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "banana", layer: 0, scale: 0.08f));
                    break;

                case PkgName.Cherry:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "cherry", layer: 0, scale: 0.12f));
                    break;

                case PkgName.Tomato:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tomato", layer: 0, scale: 0.07f));
                    break;

                case PkgName.Carrot:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "carrot", layer: 0, scale: 0.08f));
                    break;

                case PkgName.Acorn:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "acorn", layer: 0, scale: 0.13f));
                    break;

                case PkgName.Mushroom:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "mushroom", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SeedBag:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "seed_bag", layer: 0, scale: 0.08f));
                    break;

                case PkgName.CoffeeRaw:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "coffee_raw", layer: 0, scale: 1f));
                    break;

                case PkgName.CoffeeRoasted:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "coffee_roasted", layer: 0, scale: 1f));
                    break;

                case PkgName.Clam:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 256, width: 64, height: 64, scale: 0.5f));
                    break;

                case PkgName.MeatRawRegular:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "meat_raw_regular", layer: 0, scale: 0.1f));
                    break;

                case PkgName.MeatRawPrime:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "meat_raw_prime", layer: 0, scale: 0.1f));
                    break;

                case PkgName.MeatDried:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "meat_dried", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Fat:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fat", layer: 0, scale: 0.08f));
                    break;

                case PkgName.Burger:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "burger", layer: 0, scale: 0.07f));
                    break;

                case PkgName.MealStandard:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fancy_food2", layer: 0, x: 288, y: 64, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.Leather:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "craft_items", layer: 0, x: 96, y: 96, width: 32, height: 32, scale: 0.75f));
                    break;

                case PkgName.KnifeSimple:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "knife_simple", layer: 1, scale: 1f));
                    break;

                case PkgName.AxeWood:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "axe_wooden", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));
                    break;

                case PkgName.AxeStone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "big_icons_candacis", layer: 0, x: 0, y: 0, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.AxeIron:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "big_icons_candacis", layer: 0, x: 48, y: 0, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.AxeCrystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "axe_crystal", layer: 1, scale: 0.5f));
                    break;

                case PkgName.PickaxeWood:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 0, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.PickaxeStone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 48, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.PickaxeIron:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 192, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.PickaxeCrystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 384, y: 384, width: 48, height: 48, scale: 0.7f));
                    break;

                case PkgName.ScytheStone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "scythe_stone", layer: 0));
                    break;

                case PkgName.ScytheIron:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "scythe_iron", layer: 0));
                    break;

                case PkgName.ScytheCrystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "scythe_crystal", layer: 0));
                    break;

                case PkgName.SpearWood:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "spear_wood", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SpearStone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "spear_stone", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SpearIron:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "spear_iron", layer: 0, scale: 0.5f));
                    break;

                case PkgName.SpearCrystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "spear_crystal", layer: 0, scale: 0.5f));
                    break;

                case PkgName.ShovelStone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "shovel_stone", layer: 0, scale: 0.7f));
                    break;

                case PkgName.ShovelIron:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "shovel_iron", layer: 0, scale: 0.7f));
                    break;

                case PkgName.ShovelCrystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "shovel_crystal", layer: 0, scale: 0.7f));
                    break;

                case PkgName.BowBasic:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bow_basic", layer: 0, scale: 0.25f));
                    break;

                case PkgName.BowAdvanced:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bow_advanced", layer: 0, scale: 0.25f));
                    break;

                case PkgName.ArrowWood:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "arrow_wood", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowStone:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "arrow_stone", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowIron:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "arrow_iron", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowCrystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "arrow_crystal", layer: 0, scale: 0.75f));
                    break;

                case PkgName.ArrowExploding:
                    {
                        int layer = 0;
                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "arrow_burning_off", layer: layer, scale: 0.75f));
                        AddFrameArray(pkgName: pkgName, animName: "burning", frameArray: ConvertImageToFrameArray(atlasName: "arrow_burning_on", layer: layer, scale: 0.75f));
                        break;
                    }

                case PkgName.CoalDeposit:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 0, y: 96, width: 48, height: 48, scale: 1f));
                    break;

                case PkgName.IronDeposit:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 96, y: 96, width: 48, height: 48, scale: 1f));
                    break;

                case PkgName.CrystalDepositSmall:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "crystal_deposit_small", layer: 1));
                    break;

                case PkgName.CrystalDepositBig:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "crystal_deposit_big", layer: 1));
                    break;

                case PkgName.DigSite:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "dig_site", layer: 0));
                    break;

                case PkgName.DigSiteGlass:
                    {
                        int layer = 0;
                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "dig_site_glass", layer: layer, duration: 450),
                            ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: layer, duration: 1),
                            ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: layer, duration: 1),
                            ConvertImageToFrame(atlasName: "dig_site_glass_shine_3", layer: layer, duration: 1),
                            ConvertImageToFrame(atlasName: "dig_site_glass_shine_2", layer: layer, duration: 1),
                            ConvertImageToFrame(atlasName: "dig_site_glass_shine_1", layer: layer, duration: 1),
                        });
                        break;
                    }

                case PkgName.DigSiteRuins:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "dig_site_ruins", layer: 0, scale: 0.35f));
                    break;

                case PkgName.Coal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 288, y: 144, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.IronOre:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 48, y: 144, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.IronBar:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 48, y: 96, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.IronRod:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "iron_rod", layer: 0, scale: 1f));
                    break;

                case PkgName.IronPlate:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "iron_plate", layer: 0, scale: 1f));
                    break;

                case PkgName.GlassSand:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tools_gravel", layer: 0, x: 192, y: 144, width: 48, height: 48, scale: 0.5f));
                    break;

                case PkgName.Crystal:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "crystal", layer: 1, scale: 0.5f));
                    break;

                case PkgName.PlayerBoy:
                    AddRPGMakerPackageV1(pkgName: pkgName, atlasName: "characters/actor29rec4", setNoX: 0, setNoY: 0, animSize: 0);
                    break;

                case PkgName.PlayerGirl:
                    AddRPGMakerPackageV1(pkgName: pkgName, atlasName: "characters/recolor_pt2", setNoX: 0, setNoY: 0, animSize: 0);
                    break;

                case PkgName.PlayerTestDemoness:
                    AddRPGMakerPackageV1(pkgName: pkgName, atlasName: "characters/demoness", setNoX: 0, setNoY: 0, animSize: 0);
                    break;

                case PkgName.FoxGinger:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxRed:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxWhite:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxGray:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxBlack:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxChocolate:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxBrown:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.FoxYellow:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog1:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 0, setNoY: 0, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog2:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 1, setNoY: 0, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog3:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 2, setNoY: 0, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog4:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 3, setNoY: 0, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog5:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 0, setNoY: 1, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog6:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 1, setNoY: 1, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog7:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 2, setNoY: 1, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.Frog8:
                    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 3, setNoY: 1, animSize: 0, scale: 1f);
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitBrown:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitDarkBrown:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitGray:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitBlack:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitLightGray:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitBeige:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitWhite:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.RabbitLightBrown:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                    break;

                case PkgName.BearBrown:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearWhite:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearOrange:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearBlack:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearDarkBrown:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearGray:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearRed:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.BearBeige:
                    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                    break;

                case PkgName.TentModern:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tent_modern", layer: 1, scale: 0.6f, depthPercent: 0.6f));
                    break;

                case PkgName.TentModernPacked:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tent_modern_packed", layer: 1, scale: 0.12f));
                    break;

                case PkgName.TentSmall:
                    // TODO replace with A - frame tent asset(when found)
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 0.5f, depthPercent: 0.45f));
                    break;

                case PkgName.TentMedium:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 1f, depthPercent: 0.45f));
                    break;

                case PkgName.TentBig:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tent_big", layer: 1, x: 15, y: 0, width: 191, height: 162, scale: 1f, depthPercent: 0.6f));
                    break;

                case PkgName.BackpackSmall:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "backpack_small", layer: 1, scale: 0.1f));
                    break;

                case PkgName.BackpackMedium:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "backpack_medium", layer: 1, scale: 0.5f));
                    break;

                case PkgName.BackpackBig:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "backpack_big", layer: 1, scale: 0.1f));
                    break;

                case PkgName.BackpackLuxurious:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "backpack_luxurious", layer: 1, scale: 0.1f));
                    break;

                case PkgName.BeltSmall:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "belt_small", layer: 0, scale: 1f));
                    break;

                case PkgName.BeltMedium:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "belt_medium", layer: 0, scale: 0.12f));
                    break;

                case PkgName.BeltBig:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "belt_big", layer: 0, scale: 0.06f));
                    break;

                case PkgName.BeltLuxurious:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "belt_luxurious", layer: 0, scale: 0.06f));
                    break;

                case PkgName.Map:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "map_item", layer: 0, scale: 0.06f, crop: false));
                    break;

                case PkgName.HatSimple:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "hat_simple", layer: 0, scale: 0.5f));
                    break;

                case PkgName.BootsProtective:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boots_protective", layer: 0, scale: 1f));
                    break;

                case PkgName.BootsMountain:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boots_mountain", layer: 0, scale: 1f));
                    break;

                case PkgName.BootsSpeed:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boots_speed", layer: 0, scale: 1f));
                    break;

                case PkgName.BootsAllTerrain:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boots_all_terrain", layer: 0, scale: 1f));
                    break;

                case PkgName.GlovesStrength:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "gloves_strength", layer: 0, scale: 0.7f));
                    break;

                case PkgName.GlassesBlue:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "glasses_blue", layer: 0, scale: 0.5f));
                    break;

                case PkgName.Dungarees:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "dungarees", layer: 0, scale: 1f));
                    break;

                case PkgName.LanternFrame:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "lantern_frame", layer: 0, scale: 0.075f));
                    break;

                case PkgName.Candle:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "candle", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Lantern:
                    {
                        float scale = 0.075f;
                        int layer = 0;

                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "lantern_on", layer: layer, scale: scale));
                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "lantern_off", layer: layer, scale: scale));
                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "lantern_off", layer: layer, scale: scale));

                        break;
                    }

                case PkgName.SmallTorch:
                    {
                        float scale = 0.07f;
                        int layer = 0;

                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "small_torch_on", layer: layer, scale: scale));
                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "small_torch_off", layer: layer, scale: scale));
                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "small_torch_off", layer: layer, scale: scale));

                        break;
                    }

                case PkgName.BigTorch:
                    {
                        float scale = 0.1f;
                        int layer = 0;

                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "big_torch_on", layer: layer, scale: scale));
                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "big_torch_off", layer: layer, scale: scale));
                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "big_torch_off", layer: layer, scale: scale));

                        break;
                    }

                case PkgName.CampfireSmall:
                    {
                        int layer = 1;

                        var frameArray = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 288, y: 0, width: 48, height: 48, crop: false),
                            ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 336, y: 0, width: 48, height: 48, crop: false),
                            ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 384, y: 0, width: 48, height: 48, crop: false)
                        };
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArray);
                        frameArray = ConvertImageToFrameArray(atlasName: "flames", layer: layer, x: 288, y: 96, width: 48, height: 48, crop: false);
                        AddFrameArray(pkgName: pkgName, frameArray);
                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: frameArray);

                        break;
                    }

                case PkgName.CampfireMedium:
                    {
                        int layer = 1;

                        var frameArray = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "campfire_medium_on_1", layer: layer, duration: 6, crop: false),
                            ConvertImageToFrame(atlasName: "campfire_medium_on_2", layer: layer, duration: 6, crop: false),
                            ConvertImageToFrame(atlasName: "campfire_medium_on_3", layer: layer, duration: 6, crop: false)
                        };
                        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArray);
                        AddFrameArray(pkgName: pkgName, ConvertImageToFrameArray(atlasName: "campfire_medium_off", layer: layer, crop: false));
                        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: frameArray);

                        break;
                    }

                case PkgName.BoatConstruction:
                    {
                        float depthPercent = 0.7f;
                        float scale = 0.7f;

                        for (int animSize = 0; animSize <= 5; animSize++)
                        {
                            AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: $"boat/boat_construction_{animSize}", layer: 1, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true, crop: false), animSize: animSize, updateCroppedFramesForPkgs: animSize == 0); // animSize == 0 should serve as an example (whole blueprint visible)
                        }

                        break;
                    }

                case PkgName.BoatCompleteStanding:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boat/boat_complete", layer: 1, scale: 0.7f, depthPercent: 0.7f, ignoreWhenCalculatingMaxSize: true, crop: false));
                    break;

                case PkgName.BoatCompleteCruising:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boat/boat_complete", layer: 0, scale: 0.7f, depthPercent: 0.7f, ignoreWhenCalculatingMaxSize: true, crop: false));
                    break;

                case PkgName.ShipRescue:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "ship_rescue", layer: 1, scale: 1.5f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.HerbsBlack:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_black", layer: 0));
                    break;

                case PkgName.HerbsCyan:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_cyan", layer: 0));
                    break;

                case PkgName.HerbsBlue:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_blue", layer: 0));
                    break;

                case PkgName.HerbsGreen:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_green", layer: 0));
                    break;

                case PkgName.HerbsYellow:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_yellow", layer: 0));
                    break;

                case PkgName.HerbsRed:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_red", layer: 0));
                    break;

                case PkgName.HerbsViolet:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_violet", layer: 0));
                    break;

                case PkgName.HerbsBrown:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_brown", layer: 0));
                    break;

                case PkgName.HerbsDarkViolet:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_dark_violet", layer: 0));
                    break;

                case PkgName.HerbsDarkGreen:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "herbs_dark_green", layer: 0));
                    break;

                case PkgName.EmptyBottle:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bottle_empty", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionRed:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 384, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionBlue:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 448, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionViolet:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 416, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionYellow:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 352, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionCyan:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 288, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionGreen:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 320, y: 128, width: 32, height: 32, scale: 0.5f));
                    break;

                case PkgName.PotionBlack:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "potion_black", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionDarkViolet:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "potion_dark_violet", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionDarkYellow:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "potion_dark_yellow", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionDarkGreen:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "potion_dark_green", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionLightYellow:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bottle_oil", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionTransparent:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "potion_transparent", layer: 0, scale: 0.5f));
                    break;

                case PkgName.PotionBrown:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "potion_brown", layer: 0, scale: 0.5f));
                    break;

                case PkgName.BloodSplatter1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 416, y: 320, width: 32, height: 32));
                    break;

                case PkgName.BloodSplatter2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 448, y: 320, width: 32, height: 32));
                    break;

                case PkgName.BloodSplatter3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_custom01", layer: 0, x: 448, y: 288, width: 32, height: 32));
                    break;

                case PkgName.HumanSkeleton:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "tile_rtp-addons", layer: 0, x: 320, y: 160, width: 32, height: 32));
                    break;

                case PkgName.Hole:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "hole", layer: 0, scale: 1f));
                    break;

                case PkgName.Explosion:
                    {
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

                            AddFrameArray(pkgName: pkgName, animSize: size, animName: "default", frameArray: frameList.ToArray());
                        }

                        break;
                    }

                case PkgName.SkullAndBones:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "skull_and_bones", layer: 2, scale: 1f));
                    break;

                case PkgName.MusicNoteSmall:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "music_note", layer: 2));
                    break;

                case PkgName.MusicNoteBig:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "music_note", layer: 2, scale: 2.5f));
                    break;

                case PkgName.Miss:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "miss", layer: 2));
                    break;

                case PkgName.Attack:
                    {
                        int layer = 2;

                        var frameArray = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "attack", layer: layer, duration: 6, x: 0, y: 20, width: 23, height: 23),
                            ConvertImageToFrame(atlasName: "attack", layer: layer, duration: 6, x: 22, y: 13, width: 29, height: 33),
                            ConvertImageToFrame(atlasName: "attack", layer: layer, duration: 6, x: 50, y: 0, width: 36, height: 65)
                        };
                        AddFrameArray(pkgName: pkgName, frameArray: frameArray);

                        break;
                    }

                case PkgName.MapMarker:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "map_marker", layer: 2, crop: false, padding: 0, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.Backlight:
                    {
                        int layer = 0;

                        var frameArray = new AnimFrame[]
                        {
                            ConvertImageToFrame(atlasName: "backlight_1", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "backlight_2", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "backlight_3", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "backlight_4", layer: layer, duration: 20),
                            ConvertImageToFrame(atlasName: "backlight_3", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "backlight_2", layer: layer, duration: 6),
                            ConvertImageToFrame(atlasName: "backlight_1", layer: layer, duration: 6)
                        };
                        AddFrameArray(pkgName: pkgName, frameArray: frameArray);
                        break;
                    }

                case PkgName.Crosshair:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "crosshair", layer: 2));
                    break;

                case PkgName.Flame:
                    {
                        byte animSize = 0;
                        int layer = 1;

                        foreach (float scale in new List<float> { 0.5f, 0.75f, 1f, 1.25f })
                        {
                            var frameArray = new AnimFrame[]
                            {
                                ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 432, y: 48, width: 48, height: 48, crop: true, scale: scale),
                                ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 480, y: 48, width: 48, height: 48, crop: true, scale: scale),
                                ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 528, y: 48, width: 48, height: 48, crop: true, scale: scale)
                            };
                            AddFrameArray(pkgName: pkgName, animSize: animSize, frameArray: frameArray);

                            animSize++;
                        }

                        foreach (float scale in new List<float> { 1f, 1.5f, 1.8f, 2f })
                        {
                            var frameArray = new AnimFrame[]
                            {
                                ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 432, y: 0, width: 48, height: 48, crop: true, scale: scale),
                                ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 480, y: 0, width: 48, height: 48, crop: true, scale: scale),
                                ConvertImageToFrame(atlasName: "flames", layer: layer, duration: 6, x: 528, y: 0, width: 48, height: 48, crop: true, scale: scale)
                            };
                            AddFrameArray(pkgName: pkgName, animSize: animSize, frameArray: frameArray);

                            animSize++;
                        }

                        break;
                    }

                case PkgName.Upgrade:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "upgrade", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WaterDrop:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "water_drop", layer: 0, scale: 0.5f));
                    break;

                case PkgName.Zzz:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "zzz", layer: 2));
                    break;

                case PkgName.Heart:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "heart_16x16", layer: 2));
                    break;

                case PkgName.Hammer:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "hammer", layer: 0, scale: 0.1f));
                    break;

                case PkgName.Fog1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.Fog2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));

                    break;

                case PkgName.Fog3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fog_1", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.Fog4:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fog_2", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.BubbleExclamationRed:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bubble_exclamation_red", layer: 2, scale: 0.2f));
                    break;

                case PkgName.BubbleExclamationBlue:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bubble_exclamation_blue", layer: 2, scale: 0.2f));
                    break;

                case PkgName.BubbleCraftGreen:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "bubble_craft_green", layer: 2, scale: 0.2f));
                    break;

                case PkgName.SeaWave:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "wave", layer: 0, scale: 0.5f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog1:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_1", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog2:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_2", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog3:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_3", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog4:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog5:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog6:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_3", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog7:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_1", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog8:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_2", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.WeatherFog9:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "weather_fog_3", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                    break;

                case PkgName.FertileGroundSmall:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fertile_ground_small", layer: -1, scale: 1f));
                    break;

                case PkgName.FertileGroundMedium:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fertile_ground_medium", layer: -1, scale: 1f));
                    break;

                case PkgName.FertileGroundLarge:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fertile_ground_big", layer: -1, scale: 1.3f));
                    break;

                case PkgName.FenceHorizontalShort:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fence_horizontal_short", layer: 1, depthPercent: 0.2f));
                    break;

                case PkgName.FenceVerticalShort:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fence_vertical_short", layer: 1, depthPercent: 0.9f));
                    break;

                case PkgName.FenceHorizontalLong:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fence_horizontal_long", layer: 1, depthPercent: 0.2f));
                    break;

                case PkgName.FenceVerticalLong:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "fence_vertical_long", layer: 1, depthPercent: 0.95f));
                    break;

                case PkgName.CaveEntrance:
                    {
                        float scale = 2f;
                        float depthPercent = 0.95f;

                        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "cave_entrance_open", scale: scale, layer: 1, depthPercent: depthPercent));
                        AddFrameArray(pkgName: pkgName, animName: "blocked", frameArray: ConvertImageToFrameArray(atlasName: "cave_entrance_blocked", scale: scale, layer: 1, depthPercent: depthPercent));

                        break;
                    }

                case PkgName.CaveExit:
                    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "cave_exit", scale: 2f, layer: 1, depthPercent: 0.95f));
                    break;

                case PkgName.DragonBonesTest1:
                    AddDragonBonesPackage(pkgName: pkgName, jsonName: "ubbie_tex.json");

                    break;

                default:
                    throw new ArgumentException($"Unsupported pkgName - {pkgName}.");
            }

            LoadedPkgs.Add(pkgName);

            return true;
        }

        public static void AddFrameArray(PkgName pkgName, AnimFrame[] frameArray, int animSize = 0, string animName = "default", bool updateCroppedFramesForPkgs = true)
        {
            if (updateCroppedFramesForPkgs && (!animSizesForPkgs.ContainsKey(pkgName) || animSizesForPkgs[pkgName] < animSize))
            {
                croppedFramesForPkgs[pkgName] = frameArray[0].GetCroppedFrameCopy();
                animSizesForPkgs[pkgName] = animSize;
            }

            string completeAnimID = $"{pkgName}-{animSize}-{animName}";
            frameArrayById[completeAnimID] = frameArray;
        }

        public static AnimFrame[] ConvertImageToFrameArray(string atlasName, int layer, int x = 0, int y = 0, int width = 0, int height = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {
            return new AnimFrame[]
            {
                ConvertImageToFrame(atlasName: atlasName, layer: layer, x: x, y: y, width: width, height: height, crop: crop, duration: 0, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize)
            };
        }

        public static AnimFrame ConvertImageToFrame(string atlasName, int layer, int x = 0, int y = 0, int width = 0, int height = 0, short duration = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f, int padding = 1, bool ignoreWhenCalculatingMaxSize = false)
        {

            return AnimFrame.GetFrame(atlasName: atlasName, atlasX: x, atlasY: y, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent, padding: padding, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize);
        }

        private static void AddChestPackage(PkgName pkgName)
        {
            var chestDict = new Dictionary<PkgName, string> {
                { PkgName.ChestWooden, "chest_wooden/chest_wooden_" },
                { PkgName.ChestStone, "chest_stone/chest_stone_" },
                { PkgName.ChestIron, "chest_iron/chest_iron_" },
                { PkgName.ChestCrystal, "chest_crystal/chest_crystal_" },
                { PkgName.ChestTreasureBlue, "chest_blue/chest_blue_" },
                { PkgName.ChestTreasureRed, "chest_red/chest_red_" },
            };

            string chestPath = chestDict[pkgName];

            float scale = 0.18f;
            bool crop = false;
            float depthPercent = 0.47f;
            int duration = 3;

            var openingFrameList = new List<AnimFrame>();
            for (int i = 2; i <= 6; i++)
            {
                openingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (short)(i < 6 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
            }
            AddFrameArray(pkgName: pkgName, animName: "opening", frameArray: openingFrameList.ToArray());

            var closingFrameList = new List<AnimFrame>();
            for (int i = 5; i >= 1; i--)
            {
                closingFrameList.Add(ConvertImageToFrame(atlasName: $"{chestPath}{i}", layer: 1, duration: (short)(i > 1 ? duration : 0), crop: crop, scale: scale, depthPercent: depthPercent));
            }
            AddFrameArray(pkgName: pkgName, animName: "closing", frameArray: closingFrameList.ToArray());

            AddFrameArray(pkgName: pkgName, animName: "closed", frameArray: ConvertImageToFrameArray(atlasName: $"{chestPath}1", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
            AddFrameArray(pkgName: pkgName, animName: "open", frameArray: ConvertImageToFrameArray(atlasName: $"{chestPath}6", layer: 1, crop: crop, scale: scale, depthPercent: depthPercent));
        }

        public static void AddRPGMakerPackageV1(PkgName pkgName, string atlasName, byte setNoX, byte setNoY, int animSize, bool crop = false, float scale = 1f)
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
                var frameArray = new AnimFrame[]
                {
                     AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale)
                };

                AddFrameArray(pkgName: pkgName, animSize: animSize, animName: $"stand-{kvp.Key}", frameArray: frameArray);
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                var frameArray = new AnimFrame[]
                {
                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale),

                    AnimFrame.GetFrame(atlasName: atlasName, atlasX: (width * 2) + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale)
                };

                AddFrameArray(pkgName: pkgName, animSize: animSize, animName: $"walk-{kvp.Key}", frameArray: frameArray);
            }

            AddFrameArray(pkgName: pkgName, animSize: animSize, frameArray: new AnimFrame[] { croppedFramesForPkgs[pkgName] }); // adding default frame
        }

        public static void AddRPGMakerPackageV2ForSizeDict(PkgName pkgName, string atlasName, int setNoX, int setNoY, Dictionary<byte, float> scaleForSizeDict)
        {
            foreach (var kvp in scaleForSizeDict)
            {
                byte animSize = kvp.Key;
                float scale = kvp.Value;
                AddRPGMakerPackageV2(pkgName: pkgName, atlasName: atlasName, setNoX: setNoX, setNoY: setNoY, animSize: animSize, scale: scale);
            }
        }

        public static void AddRPGMakerPackageV2(PkgName pkgName, string atlasName, int setNoX, int setNoY, byte animSize, bool crop = false, float scale = 1f)
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
                AddFrameArray(pkgName: pkgName, animSize: animSize, animName: $"stand-{kvp.Key}",
                    frameArray: new AnimFrame[] { AnimFrame.GetFrame(atlasName: atlasName, atlasX: width + offsetX, atlasY: kvp.Value + offsetY, width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale) });
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                var frameArray = new AnimFrame[]
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

                AddFrameArray(pkgName: pkgName, animSize: animSize, animName: $"walk-{kvp.Key}", frameArray: frameArray);
            }

            AddFrameArray(pkgName: pkgName, animSize: animSize, frameArray: new AnimFrame[] { croppedFramesForPkgs[pkgName] }); // adding default frame
        }

        public static void AddDragonBonesPackage(PkgName pkgName, string jsonName)
        {
            var jsonData = FileReaderWriter.LoadJson(path: Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones", jsonName));
            var animDict = (JObject)jsonData;

            string atlasName = ((string)animDict["imagePath"]).Replace(".png", "");
            string atlasImagePath = $"_DragonBones/{atlasName}";

            Texture2D atlasTexture = TextureBank.GetTexture(atlasImagePath); // for testing


            var animDataList = animDict["SubTexture"];

            var a = 1;
        }

        private static string JsonDataPath
        { get { return Path.Combine(SonOfRobinGame.animCachePath, "_data.json"); } }

        public static bool LoadJsonDict()
        {
            // one big json is used to speed up loading / saving data

            Dictionary<string, Object> loadedJsonDict;

            try
            {
                var loadedJson = FileReaderWriter.LoadJson(path: JsonDataPath);
                if (loadedJson == null) return false;

                loadedJsonDict = (Dictionary<string, Object>)loadedJson;
            }
            catch (InvalidCastException)
            { return false; }

            if (!loadedJsonDict.ContainsKey("version") ||
                !loadedJsonDict.ContainsKey("frameDict")) return false;

            if ((float)(double)loadedJsonDict["version"] != currentVersion) return false;

            jsonDict = (Dictionary<string, Object>)loadedJsonDict["frameDict"];

            return true;
        }

        public static void SaveJsonDict()
        {
            var savedDict = new Dictionary<string, object>
            {
                { "version", currentVersion },
                { "frameDict", jsonDict },
            };

            FileReaderWriter.SaveJson(path: JsonDataPath, savedObj: savedDict, compress: true);
            // MessageLog.Add(debugMessage: true, text: "Animation json saved.");
        }

        public static void PurgeDiskCache()
        {
            foundFramePngs.Clear();

            try
            {
                var directory = new DirectoryInfo(SonOfRobinGame.animCachePath);
                foreach (FileInfo file in directory.GetFiles())
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

        public static AnimFrame GetCroppedFrameForPackage(PkgName pkgName)
        {
            return croppedFramesForPkgs[pkgName];
        }

        public static void DisposeUsedAtlases()
        {
            // Should be used after loading textures from all atlasses.
            // Deleted textures will not be available for use any longer.

            var usedAtlasNames = new HashSet<string>();
            foreach (AnimFrame[] frameArray in frameArrayById.Values)
            {
                foreach (AnimFrame animFrame in frameArray)
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