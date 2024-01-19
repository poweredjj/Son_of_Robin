using Microsoft.Xna.Framework;
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
        public static readonly PkgName[] allPkgNames = (PkgName[])Enum.GetValues(typeof(PkgName));
        public static readonly Dictionary<PkgName, AnimPkg> pkgByName = [];
        private static readonly Dictionary<PkgName, ImageObj> imgObjByName = [];

        public static ImageObj GetImageObj(PkgName pkgName)
        {
            if (!imgObjByName.ContainsKey(pkgName)) imgObjByName[pkgName] = new AnimFrameObj(pkgByName[pkgName].presentationFrame);
            return imgObjByName[pkgName];
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
            MineralsSmall5 = 302,
            MineralsSmall6 = 303,
            MineralsSmall7 = 304,
            MineralsSmall8 = 305,
            MineralsSmall9 = 306,
            MineralsSmall10 = 307,

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

            DragonBonesTestFemaleMage = 301,

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

        public static void PrepareAllAnims()
        {
            DateTime startTime = DateTime.Now;

            foreach (PkgName pkgName in allPkgNames)
            {
                PreparePackage(pkgName);
            }

            TimeSpan creationDuration = DateTime.Now - startTime;
            MessageLog.Add(debugMessage: true, text: $"Anims creation time: {creationDuration:hh\\:mm\\:ss\\.fff}", textColor: Color.GreenYellow);
        }

        public static List<AnimFrame> GetAllFrames()
        {
            var allFrames = new List<AnimFrame>();

            foreach (AnimPkg animPkg in AnimData.pkgByName.Values)
            {
                foreach (Anim anim in animPkg.AllAnimList)
                {
                    foreach (AnimFrame frame in anim.frameArray)
                    {
                        allFrames.Add(frame);
                    }
                }
            }

            return allFrames;
        }

        public static void PreparePackage(PkgName pkgName)
        {
            if (pkgByName.ContainsKey(pkgName))
            {
                MessageLog.Add(debugMessage: true, text: $"Package '{pkgName}' has been prepared already, ignoring.");
                return;
            }

            AnimPkg animPkg;

            switch (pkgName)
            {
                case PkgName.Empty:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 2, animSize: 0, altasName: "transparent_pixel", hasOnePixelMargin: false);
                    break;

                case PkgName.NoAnim:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 63, height: 16, scale: 1f, layer: 2, animSize: 0, altasName: "no_anim", hasOnePixelMargin: false);
                    break;

                case PkgName.WhiteSpotLayerMinus1:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: -1, animSize: 0, altasName: "white_spot", hasOnePixelMargin: false);
                    break;

                case PkgName.WhiteSpotLayerZero:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 0, animSize: 0, altasName: "white_spot", hasOnePixelMargin: false);
                    break;

                case PkgName.WhiteSpotLayerOne:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 1, animSize: 0, altasName: "white_spot", hasOnePixelMargin: false);
                    break;

                case PkgName.WhiteSpotLayerTwo:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 2, animSize: 0, altasName: "white_spot", hasOnePixelMargin: false);
                    break;

                case PkgName.WhiteSpotLayerThree:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 3, animSize: 0, altasName: "white_spot", hasOnePixelMargin: false);
                    break;

                case PkgName.FlowersWhite:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "flowers_white", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 23, height: 19), gfxOffsetCorrection: new Vector2(0, -1), shadowOriginFactor: new Vector2(0.5f, 0.79f), shadowHeightMultiplier: 0.55f, hasFlatShadow: false)]));

                        break;
                    }

                case PkgName.FlowersYellow1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "flowers_yellow_1_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 17), gfxOffsetCorrection: new Vector2(0, 1))]));
                        break;
                    }

                case PkgName.FlowersYellow2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "flowers_yellow_2_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 46, height: 41), scale: 0.5f)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "flowers_yellow_2_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 38, height: 49), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -7), shadowOriginFactor: new Vector2(0.37f, 0.89f), shadowPosOffset: new Vector2(0, 3.5f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.FlowersRed:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "flowers_red", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 25), gfxOffsetCorrection: new Vector2(0, -2), shadowOriginFactor: new Vector2(0.5f, 0.77f), shadowHeightMultiplier: 0.55f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.Rushes:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "rushes", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 34), gfxOffsetCorrection: new Vector2(0, -8), shadowOriginFactor: new Vector2(0.53f, 0.84f), shadowPosOffset: new Vector2(-1.5f, 0), shadowHeightMultiplier: 0.45f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.GrassDesert:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 12);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "grass_desert_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 23, height: 21))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "grass_desert_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 19))]));
                        break;
                    }

                case PkgName.GrassRegular:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "grass_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 20), gfxOffsetCorrection: new Vector2(0, -2), shadowOriginFactor: new Vector2(0.5f, 0.8f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "grass_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 20), scale: 1.2f, gfxOffsetCorrection: new Vector2(0, -3), shadowOriginFactor: new Vector2(0.5f, 0.8f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));

                        break;
                    }

                case PkgName.PlantPoison:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 11);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "plant_poison", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 33), scale: 0.4f, shadowOriginFactor: new Vector2(0.5f, 0.81f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "plant_poison", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 33), scale: 0.6f, gfxOffsetCorrection: new Vector2(0, -1), shadowOriginFactor: new Vector2(0.5f, 0.81f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.CoffeeShrub:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 15);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "coffee_shrub", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 486, height: 577), scale: 0.05f, gfxOffsetCorrection: new Vector2(0, -136), shadowOriginFactor: new Vector2(0.42f, 0.99f), shadowPosOffset: new Vector2(-1.5f, 7.5f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "coffee_shrub", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 486, height: 577), scale: 0.06f, gfxOffsetCorrection: new Vector2(0, -160), shadowOriginFactor: new Vector2(0.42f, 0.99f), shadowPosOffset: new Vector2(-1.5f, 7.5f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.CarrotPlant:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 26, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "carrot_plant_empty", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 298, height: 283), scale: 0.1f, gfxOffsetCorrection: new Vector2(0, -37), shadowOriginFactor: new Vector2(0.53f, 0.95f), shadowPosOffset: new Vector2(0f, 6f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)], name: "default"));

                        // using different plant graphics when carrot is present, instead of drawing the carrot separately (because the carrot should be underground)
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "carrot_plant_has_carrot", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 295, height: 351), scale: 0.1f, gfxOffsetCorrection: new Vector2(0, -70), shadowOriginFactor: new Vector2(0.53f, 0.95f), shadowPosOffset: new Vector2(0f, 6f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)], name: "has_fruits"));
                        break;
                    }

                case PkgName.TomatoPlant:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 28, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tomato_plant_small", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 296, height: 288), scale: 0.1f, gfxOffsetCorrection: new Vector2(0, -13), shadowOriginFactor: new Vector2(0.6f, 0.97f), shadowPosOffset: new Vector2(2.5f, 11.5f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "tomato_plant_medium", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 477, height: 459), scale: 0.08f, gfxOffsetCorrection: new Vector2(0, -64), shadowOriginFactor: new Vector2(0.58f, 1.01f), shadowPosOffset: new Vector2(2.5f, 11.5f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "tomato_plant_big", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 494, height: 980), scale: 0.08f, gfxOffsetCorrection: new Vector2(0, -332), shadowOriginFactor: new Vector2(0.58f, 0.99f), shadowPosOffset: new Vector2(2.5f, 11.5f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MushroomPlant:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 18, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mushroom_plant", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 29), scale: 0.6f, gfxOffsetCorrection: new Vector2(1, -2), shadowOriginFactor: new Vector2(0.46f, 1.03f), shadowPosOffset: new Vector2(0f, 4f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "mushroom_plant", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 29), scale: 0.8f, gfxOffsetCorrection: new Vector2(1, -5), shadowOriginFactor: new Vector2(0.46f, 1.03f), shadowPosOffset: new Vector2(0f, 4f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "mushroom_plant", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 29), scale: 1.0f, gfxOffsetCorrection: new Vector2(1, -7), shadowOriginFactor: new Vector2(0.46f, 1.03f), shadowPosOffset: new Vector2(0f, 4f), shadowHeightMultiplier: 0.5f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.Cactus:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 18, colHeight: 18);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "cactus_s0", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 26, height: 29), gfxOffsetCorrection: new Vector2(0f, -5f), shadowOriginFactor: new Vector2(0.5f, 0.98f), shadowPosOffset: new Vector2(-2.5f, 4.5f), shadowHeightMultiplier: 0.8f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "cactus_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 49), gfxOffsetCorrection: new Vector2(0f, -13f), shadowOriginFactor: new Vector2(0.5f, 0.98f), shadowPosOffset: new Vector2(0f, 4.5f), shadowHeightMultiplier: 0.8f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.PalmTree:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "palmtree_small", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 39), scale: 0.7f, gfxOffsetCorrection: new Vector2(0, -8), shadowOriginFactor: new Vector2(0.49f, 0.85f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "palmtree_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 71, height: 82), gfxOffsetCorrection: new Vector2(1, -20), shadowOriginFactor: new Vector2(0.49f, 0.85f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "palmtree_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 68, height: 104), gfxOffsetCorrection: new Vector2(15, -36), shadowOriginFactor: new Vector2(0.28f, 0.93f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 3, frameArray: [new AnimFrame(atlasName: "palmtree_s3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 74, height: 104), gfxOffsetCorrection: new Vector2(14, -37), shadowOriginFactor: new Vector2(0.31f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TreeBig:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 14, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "sapling_tall", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 99), scale: 0.5f, gfxOffsetCorrection: new Vector2(0f, -32f), shadowOriginFactor: new Vector2(0.5f, 0.97f), shadowPosOffset: new Vector2(0f, 7f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "tree_big_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 49, height: 64), gfxOffsetCorrection: new Vector2(0, -22), shadowOriginFactor: new Vector2(0.53f, 0.94f), shadowPosOffset: new Vector2(2.5f, -1.5f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "tree_big_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 65, height: 96), gfxOffsetCorrection: new Vector2(0, -38), shadowOriginFactor: new Vector2(0.5f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TreeSmall1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 15);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "sapling_short", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 44, height: 55), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -10), shadowPosOffset: new Vector2(0, 7.5f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "tree_small_1_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 33, height: 34), gfxOffsetCorrection: new Vector2(0f, -22f), shadowOriginFactor: new Vector2(0.5f, 0.92f), shadowHeightMultiplier: 0.7f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "tree_small_1_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 66), gfxOffsetCorrection: new Vector2(0, -22), shadowOriginFactor: new Vector2(0.5f, 0.84f), shadowHeightMultiplier: 0.7f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TreeSmall2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 15);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "sapling_short", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 44, height: 55), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -10), shadowPosOffset: new Vector2(0, 7.5f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrame(atlasName: "tree_small_2_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 32), gfxOffsetCorrection: new Vector2(0f, -6f), shadowOriginFactor: new Vector2(0.5f, 0.85f), shadowHeightMultiplier: 1f, hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrame(atlasName: "tree_small_2_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 66), gfxOffsetCorrection: new Vector2(0, -22), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TreeStump:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 17);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tree_stump", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 20), scale: 1f, shadowOriginFactor: new Vector2(0.5f, 0.72f), shadowHeightMultiplier: 0.6f, hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.WaterLily1:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 34, scale: 1f, layer: 0, animSize: 0, altasName: "waterlily1", hasOnePixelMargin: true);
                    break;

                case PkgName.WaterLily2:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 32, height: 32, scale: 1f, layer: 0, animSize: 0, altasName: "waterlily2", hasOnePixelMargin: true);
                    break;

                case PkgName.WaterLily3:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 32, height: 34, scale: 1f, layer: 0, animSize: 0, altasName: "waterlily3", hasOnePixelMargin: true);
                    break;

                case PkgName.MineralsBig1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 37);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 222, height: 310), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -80f), shadowOriginFactor: new Vector2(0.5f, 0.93f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 37);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 210, height: 308), scale: 0.3f, gfxOffsetCorrection: new Vector2(2f, -76f), shadowOriginFactor: new Vector2(0.5f, 0.93f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig3:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 77, colHeight: 41);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 312, height: 346), scale: 0.3f, gfxOffsetCorrection: new Vector2(14f, -67f), shadowOriginFactor: new Vector2(0.5f, 0.9f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig4:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 55, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 213, height: 312), scale: 0.3f, gfxOffsetCorrection: new Vector2(4f, -77f), shadowOriginFactor: new Vector2(0.5f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig5:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 74, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_5", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 298, height: 294), scale: 0.3f, gfxOffsetCorrection: new Vector2(16f, -66f), shadowOriginFactor: new Vector2(0.48f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig6:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 55, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_6", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 217, height: 306), scale: 0.3f, gfxOffsetCorrection: new Vector2(3f, -80f), shadowOriginFactor: new Vector2(0.5f, 0.96f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig7:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 69, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_7", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 251, height: 297), scale: 0.3f, gfxOffsetCorrection: new Vector2(1f, -75f), shadowOriginFactor: new Vector2(0.5f, 0.96f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig8:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 56, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_8", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 228, height: 312), scale: 0.3f, gfxOffsetCorrection: new Vector2(-1, -81), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig9:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_9", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 230, height: 342), scale: 0.3f, gfxOffsetCorrection: new Vector2(1f, -97f), shadowOriginFactor: new Vector2(0.5f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig10:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_10", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 231, height: 301), scale: 0.3f, gfxOffsetCorrection: new Vector2(4, -85), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig11:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 90, colHeight: 44);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_11", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 321, height: 333), scale: 0.3f, gfxOffsetCorrection: new Vector2(2f, -78f), shadowOriginFactor: new Vector2(0.5f, 0.93f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig12:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 60, colHeight: 42);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_12", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 217, height: 328), scale: 0.3f, gfxOffsetCorrection: new Vector2(2f, -84f), shadowOriginFactor: new Vector2(0.5f, 0.92f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig13:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 59, colHeight: 42);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_13", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 229, height: 308), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -81), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsBig14:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_big_14", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 266, height: 269), scale: 0.3f, gfxOffsetCorrection: new Vector2(-10f, -54f), shadowOriginFactor: new Vector2(0.5f, 0.9f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 232, height: 163), scale: 0.3f, gfxOffsetCorrection: new Vector2(-1f, -21f), shadowOriginFactor: new Vector2(0.5f, 0.87f), hasFlatShadow: false)]));
                        break;
                    }
                case PkgName.MineralsSmall2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 45, colHeight: 22);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 186, height: 186), scale: 0.3f, gfxOffsetCorrection: new Vector2(11f, -47f), shadowOriginFactor: new Vector2(0.5f, 0.95f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall3:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 68, colHeight: 26);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 256, height: 164), scale: 0.3f, gfxOffsetCorrection: new Vector2(2f, -25f), shadowOriginFactor: new Vector2(0.5f, 0.89f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall4:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 68, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 261, height: 145), scale: 0.3f, gfxOffsetCorrection: new Vector2(2f, -19f), shadowOriginFactor: new Vector2(0.5f, 0.88f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall5:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 26);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_5", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 161, height: 175), scale: 0.3f, gfxOffsetCorrection: new Vector2(3, -35), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall6:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 52, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_6", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 216, height: 156), scale: 0.3f, gfxOffsetCorrection: new Vector2(4, -32), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall7:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 47, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_7", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 195, height: 181), scale: 0.3f, gfxOffsetCorrection: new Vector2(8, -32), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall8:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 47, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_8", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 182, height: 172), scale: 0.3f, gfxOffsetCorrection: new Vector2(-6, -33), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall9:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 23);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_9", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 151, height: 154), scale: 0.3f, gfxOffsetCorrection: new Vector2(-3, -33), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsSmall10:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 22);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "minerals_small_10", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 254, height: 150), scale: 0.3f, gfxOffsetCorrection: new Vector2(-2, -26), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 74, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 265, height: 387), scale: 0.3f, gfxOffsetCorrection: new Vector2(-2, -122), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 35);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 232, height: 295), scale: 0.3f, gfxOffsetCorrection: new Vector2(-8, -72), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig3:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 55, colHeight: 34);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 233, height: 295), scale: 0.3f, gfxOffsetCorrection: new Vector2(-1f, -69f), shadowOriginFactor: new Vector2(0.5f, 0.91f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig4:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 72, colHeight: 36);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 304, height: 394), scale: 0.3f, gfxOffsetCorrection: new Vector2(-2f, -122f), shadowOriginFactor: new Vector2(0.5f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig5:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 34);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_5", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 258, height: 384), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -124f), shadowOriginFactor: new Vector2(0.5f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig6:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 34);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_6", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 270, height: 381), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -124), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig7:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 65, colHeight: 34);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_7", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 231, height: 307), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -88f), shadowOriginFactor: new Vector2(0.5f, 0.95f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig8:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 58, colHeight: 38);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_8", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 209, height: 305), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -80f), shadowOriginFactor: new Vector2(0.5f, 0.92f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig9:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 74, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_9", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 265, height: 302), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -103f), shadowOriginFactor: new Vector2(0.5f, 0.95f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig10:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 79, colHeight: 18);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_10", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 275, height: 291), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -107f), shadowOriginFactor: new Vector2(0.5f, 0.96f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig11:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 71, colHeight: 19);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_11", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 254, height: 292), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -107f), shadowOriginFactor: new Vector2(0.5f, 0.97f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossyBig12:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 84, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_big_12", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 298, height: 295), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -104f), shadowOriginFactor: new Vector2(0.5f, 0.96f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossySmall1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 77, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_small_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 266, height: 199), scale: 0.3f, gfxOffsetCorrection: new Vector2(1f, -64f), shadowOriginFactor: new Vector2(0.5f, 0.94f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossySmall2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 77, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_small_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 272, height: 286), scale: 0.3f, gfxOffsetCorrection: new Vector2(1f, -64f), shadowOriginFactor: new Vector2(0.5f, 0.91f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossySmall3:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 71, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_small_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 252, height: 166), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -36f), shadowOriginFactor: new Vector2(0.5f, 0.9f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsMossySmall4:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 67, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "mossy_minerals_small_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 246, height: 183), scale: 0.3f, gfxOffsetCorrection: new Vector2(0f, -43f), shadowOriginFactor: new Vector2(0.5f, 0.92f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.MineralsCave:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 48, colHeight: 29);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "cave_minerals", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 170, height: 157), scale: 0.3f, gfxOffsetCorrection: new Vector2(1f, -27f), shadowOriginFactor: new Vector2(0.5f, 0.89f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.JarWhole:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "jar_sealed", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 43, height: 47), scale: 0.6f, gfxOffsetCorrection: new Vector2(0, -10), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.JarBroken:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "jar_broken", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 41, height: 47), scale: 0.6f, gfxOffsetCorrection: new Vector2(0, -10), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.ChestWooden:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chests/chest_wooden/chest_wooden_", cropRect: new Rectangle(x: 0, y: 0, width: 229, height: 259), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case PkgName.ChestStone:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chests/chest_stone/chest_stone_", cropRect: new Rectangle(x: 0, y: 0, width: 227, height: 258), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case PkgName.ChestIron:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chests/chest_iron/chest_iron_", cropRect: new Rectangle(x: 0, y: 0, width: 229, height: 259), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case PkgName.ChestCrystal:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chests/chest_crystal/chest_crystal_", cropRect: new Rectangle(x: 0, y: 0, width: 226, height: 258), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case PkgName.ChestTreasureBlue:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 46, colHeight: 19, atlasNameBase: "chests/chest_blue/chest_blue_", cropRect: new Rectangle(x: 0, y: 0, width: 270, height: 308), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -95));
                        break;
                    }

                case PkgName.ChestTreasureRed:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 46, colHeight: 19, atlasNameBase: "chests/chest_red/chest_red_", cropRect: new Rectangle(x: 0, y: 0, width: 270, height: 307), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -95));
                        break;
                    }

                case PkgName.WoodLogRegular:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 19, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "wood_regular", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 30, height: 30), scale: 0.75f, gfxOffsetCorrection: new Vector2(1, -4))]));
                        break;
                    }

                case PkgName.WoodLogHard:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 19, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "wood_hard", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 44, height: 44), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -6))]));
                        break;
                    }

                case PkgName.WoodPlank:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 23, scale: 0.8f, layer: 0, animSize: 0, altasName: "wood_plank", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Nail:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 31, height: 27, scale: 0.5f, layer: 0, animSize: 0, altasName: "nail", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Rope:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 25, scale: 1f, layer: 0, animSize: 0, altasName: "rope", hasOnePixelMargin: true);
                        break;
                    }
                case PkgName.HideCloth:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 504, height: 330, scale: 0.1f, layer: 0, animSize: 0, altasName: "hidecloth", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Crate:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 23, colHeight: 16);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "crate", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 25, height: 31), scale: 1f, gfxOffsetCorrection: new Vector2(1, -6), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.WorkshopEssential:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "workshop_essential", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.WorkshopBasic:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "workshop_basic", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.WorkshopAdvanced:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "workshop_advanced", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.WorkshopMaster:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "workshop_master", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 82, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.WorkshopMeatHarvesting:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "workshop_meat_harvesting_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [new AnimFrame(atlasName: "workshop_meat_harvesting_on", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.WorkshopLeatherBasic:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "workshop_leather_basic", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.WorkshopLeatherAdvanced:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "workshop_leather_advanced", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 82, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.MeatDryingRackRegular:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 75, colHeight: 35);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "meat_drying_rack_regular_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 77, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));

                        for (int i = 1; i <= 4; i++)
                        {
                            animPkg.AddAnim(new(animPkg: animPkg, name: $"on_{i}", size: 0, frameArray: [new AnimFrame(atlasName: $"meat_drying_rack_regular_on_{i}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 77, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));
                        }
                        break;
                    }

                case PkgName.MeatDryingRackWide:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 100, colHeight: 35);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "meat_drying_rack_wide_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 102, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));

                        for (int i = 1; i <= 6; i++)
                        {
                            animPkg.AddAnim(new(animPkg: animPkg, name: $"on_{i}", size: 0, frameArray: [new AnimFrame(atlasName: $"meat_drying_rack_wide_on_{i}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 102, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));
                        }
                        break;
                    }

                case PkgName.AlchemyLabStandard:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "alchemy_lab_standard", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 64, height: 90), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -19), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.AlchemyLabAdvanced:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "alchemy_lab_advanced", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 82, height: 90), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -19), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.FurnaceConstructionSite:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 52, colHeight: 37);

                        for (int animSize = 0; animSize <= 2; animSize++)
                        {
                            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: [new AnimFrame(atlasName: $"furnace/furnace_construction_{animSize}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 603), scale: 0.2f, gfxOffsetCorrection: new Vector2(-2f, -200f), shadowOriginFactor: new Vector2(0.5f, 0.97f), hasFlatShadow: false)]));
                        }

                        animPkg.presentationFrame = animPkg.GetAnim(size: 0, name: "default").frameArray[0]; // animSize == 0 should serve as an example (whole blueprint visible)

                        break;
                    }

                case PkgName.FurnaceComplete:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 52, colHeight: 37);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "furnace/furnace_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 603), scale: 0.2f, gfxOffsetCorrection: new Vector2(-2f, -200f), shadowOriginFactor: new Vector2(0.5f, 0.97f), hasFlatShadow: false)]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [new AnimFrame(atlasName: "furnace/furnace_on", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 603), scale: 0.2f, gfxOffsetCorrection: new Vector2(-2f, -200f), shadowOriginFactor: new Vector2(0.5f, 0.97f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.Anvil:
                    {
                        AnimFrame frame = new AnimFrame(atlasName: "anvil", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 50, height: 33), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -4f), shadowOriginFactor: new Vector2(0.5f, 0.87f), hasFlatShadow: false);

                        animPkg = new(pkgName: pkgName, colWidth: 34, colHeight: 21);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case PkgName.HotPlate:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 32, colHeight: 24);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "hot_plate_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 40, height: 43), scale: 1f, gfxOffsetCorrection: new Vector2(1, -8))]));

                        var frameList = new List<AnimFrame>();

                        for (int i = 0; i < 3; i++)
                        {
                            frameList.Add(new AnimFrame(atlasName: $"hot_plate_on_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 40, height: 43), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, -8)));
                        }

                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: frameList.ToArray()));
                        break;
                    }

                case PkgName.CookingPot:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 32, colHeight: 17);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "cooking_pot_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 30), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -4f), shadowOriginFactor: new Vector2(0.5f, 0.87f), hasFlatShadow: false)]));

                        var frameList = new List<AnimFrame>();

                        for (int i = 0; i < 3; i++)
                        {
                            frameList.Add(new AnimFrame(atlasName: $"cooking_pot_on_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 30), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1f, -4f), shadowOriginFactor: new Vector2(0.5f, 0.87f), hasFlatShadow: false));
                        }

                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: frameList.ToArray()));
                        break;
                    }

                case PkgName.Totem:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 21, colHeight: 40);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "totem", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 186, height: 467), scale: 0.25f, gfxOffsetCorrection: new Vector2(-1f, -149f), shadowPosOffset: new Vector2(0f, 12f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.RuinsWallHorizontal1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 79, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "ruins_wall_horizontal_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 81, height: 66), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -19f), shadowOriginFactor: new Vector2(0.5f, 0.93f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.RuinsWallHorizontal2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "ruins_wall_horizontal_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 64, height: 66), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -19f), shadowOriginFactor: new Vector2(0.5f, 0.96f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.RuinsWallWallVertical:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 35);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "ruins_wall_vertical", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 14, height: 66), scale: 1f, gfxOffsetCorrection: new Vector2(0f, -14f), shadowPosOffset: new Vector2(0f, 12f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.RuinsColumn:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 27, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "ruins_column", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 32), scale: 1f, gfxOffsetCorrection: new Vector2(0f, -5f), shadowOriginFactor: new Vector2(0.5f, 0.86f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.RuinsRubble:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 29, colHeight: 18);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "ruins_rubble", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 28), scale: 1f, gfxOffsetCorrection: new Vector2(0f, -3f), shadowOriginFactor: new Vector2(0.5f, 0.85f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.Stick:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 22, scale: 1f, layer: 0, animSize: 0, altasName: "stick", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Stone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 21, height: 20, scale: 0.5f, layer: 0, animSize: 0, altasName: "stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Granite:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 19, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "granite", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 21, height: 19), scale: 1f, gfxOffsetCorrection: new Vector2(0, -2))]));
                        break;
                    }

                case PkgName.Clay:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 25, height: 23, scale: 0.7f, layer: 0, animSize: 0, altasName: "clay", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Apple:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 218, height: 264, scale: 0.075f, layer: 0, animSize: 0, altasName: "apple", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Banana:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 432, height: 342, scale: 0.08f, layer: 0, animSize: 0, altasName: "banana", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Cherry:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 242, height: 158, scale: 0.12f, layer: 0, animSize: 0, altasName: "cherry", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Tomato:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 234, height: 257, scale: 0.07f, layer: 0, animSize: 0, altasName: "tomato", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Carrot:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 195, height: 445, scale: 0.08f, layer: 0, animSize: 0, altasName: "carrot", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Acorn:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 131, height: 202, scale: 0.13f, layer: 0, animSize: 0, altasName: "acorn", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Mushroom:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 32, height: 30, scale: 0.5f, layer: 0, animSize: 0, altasName: "mushroom", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.SeedBag:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 211, height: 258, scale: 0.08f, layer: 0, animSize: 0, altasName: "seed_bag", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.CoffeeRaw:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 28, height: 24, scale: 1f, layer: 0, animSize: 0, altasName: "coffee_raw", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.CoffeeRoasted:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 28, height: 24, scale: 1f, layer: 0, animSize: 0, altasName: "coffee_roasted", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Clam:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 28, height: 30, scale: 0.5f, layer: 0, animSize: 0, altasName: "clam", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.MeatRawRegular:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 228, height: 161, scale: 0.1f, layer: 0, animSize: 0, altasName: "meat_raw_regular", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.MeatRawPrime:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 249, height: 221, scale: 0.1f, layer: 0, animSize: 0, altasName: "meat_raw_prime", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.MeatDried:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 298, height: 145, scale: 0.1f, layer: 0, animSize: 0, altasName: "meat_dried", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Fat:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 236, height: 201, scale: 0.08f, layer: 0, animSize: 0, altasName: "fat", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Burger:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 256, height: 233, scale: 0.07f, layer: 0, animSize: 0, altasName: "burger", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.MealStandard:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "meal_standard", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Leather:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 23, height: 26, scale: 0.75f, layer: 0, animSize: 0, altasName: "leather", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.KnifeSimple:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 24, height: 26, scale: 1f, layer: 0, animSize: 0, altasName: "knife_simple", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.AxeWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.7f, layer: 0, animSize: 0, altasName: "axe_wooden", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.AxeStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 41, scale: 0.5f, layer: 0, animSize: 0, altasName: "axe_stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.AxeIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 41, scale: 0.5f, layer: 0, animSize: 0, altasName: "axe_iron", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.AxeCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 41, scale: 0.5f, layer: 0, animSize: 0, altasName: "axe_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.PickaxeWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "pickaxe_wood", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.PickaxeStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "pickaxe_stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.PickaxeIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "pickaxe_iron", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.PickaxeCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "pickaxe_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ScytheStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 21, scale: 1f, layer: 0, animSize: 0, altasName: "scythe_stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ScytheIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 21, scale: 1f, layer: 0, animSize: 0, altasName: "scythe_iron", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ScytheCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 21, scale: 1f, layer: 0, animSize: 0, altasName: "scythe_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.SpearWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 54, height: 54, scale: 0.5f, layer: 0, animSize: 0, altasName: "spear_wood", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.SpearStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 54, height: 54, scale: 0.5f, layer: 0, animSize: 0, altasName: "spear_stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.SpearIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 57, height: 57, scale: 0.5f, layer: 0, animSize: 0, altasName: "spear_iron", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.SpearCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 57, height: 57, scale: 0.5f, layer: 0, animSize: 0, altasName: "spear_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ShovelStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 0.7f, layer: 0, animSize: 0, altasName: "shovel_stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ShovelIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 0.7f, layer: 0, animSize: 0, altasName: "shovel_iron", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ShovelCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 0.7f, layer: 0, animSize: 0, altasName: "shovel_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.BowBasic:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 78, height: 116, scale: 0.25f, layer: 0, animSize: 0, altasName: "bow_basic", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.BowAdvanced:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 78, height: 116, scale: 0.25f, layer: 0, animSize: 0, altasName: "bow_advanced", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ArrowWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 26, scale: 0.75f, layer: 0, animSize: 0, altasName: "arrow_wood", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ArrowStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 26, scale: 0.75f, layer: 0, animSize: 0, altasName: "arrow_stone", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ArrowIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 27, height: 27, scale: 0.75f, layer: 0, animSize: 0, altasName: "arrow_iron", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ArrowCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 27, height: 27, scale: 0.75f, layer: 0, animSize: 0, altasName: "arrow_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.ArrowExploding:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "arrow_burning_off", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 27, height: 27), scale: 0.75f, gfxOffsetCorrection: new Vector2(0, 0))]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "burning", size: 0, frameArray: [new AnimFrame(atlasName: "arrow_burning_on", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 32), scale: 0.75f, gfxOffsetCorrection: new Vector2(-3, -2))]));
                        break;
                    }

                case PkgName.CoalDeposit:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 22);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "coal_deposit", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 47), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -9f), shadowOriginFactor: new Vector2(0.5f, 0.88f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.IronDeposit:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 22);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "iron_deposit", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 47), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -9f), shadowOriginFactor: new Vector2(0.5f, 0.88f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.CrystalDepositSmall:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 19);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "crystal_deposit_small", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 47), scale: 1f, gfxOffsetCorrection: new Vector2(1f, -10f), shadowOriginFactor: new Vector2(0.5f, 0.9f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.CrystalDepositBig:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 45, colHeight: 30);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "crystal_deposit_big", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 96), scale: 1f, gfxOffsetCorrection: new Vector2(0f, -29f), shadowOriginFactor: new Vector2(0.5f, 0.93f), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.DigSite:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 80, height: 80, scale: 1f, layer: 0, animSize: 0, altasName: "dig_site", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.DigSiteGlass:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 80, colHeight: 80);

                        var frameList = new List<AnimFrame>();

                        frameList.Add(new AnimFrame(atlasName: "dig_site_glass", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 80, height: 80), scale: 1f, duration: 450));

                        foreach (int frameNo in new List<int> { 1, 2, 3, 2, 1 })
                        {
                            frameList.Add(new AnimFrame(atlasName: $"dig_site_glass_shine_{frameNo}", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 80, height: 80), scale: 1f, duration: 2));
                        }

                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: frameList.ToArray()));
                        break;
                    }

                case PkgName.DigSiteRuins:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 162, height: 164, scale: 0.35f, layer: 0, animSize: 0, altasName: "dig_site_ruins", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Coal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 49, height: 48, scale: 0.5f, layer: 0, animSize: 0, altasName: "coal", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.IronOre:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 49, height: 48, scale: 0.5f, layer: 0, animSize: 0, altasName: "iron_ore", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.IronBar:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 24, height: 39, scale: 0.5f, layer: 0, animSize: 0, altasName: "iron_bar", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.IronRod:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 22, scale: 1f, layer: 0, animSize: 0, altasName: "iron_rod", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.IronPlate:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 18, height: 26, scale: 1f, layer: 0, animSize: 0, altasName: "iron_plate", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.GlassSand:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 49, height: 48, scale: 0.5f, layer: 0, animSize: 0, altasName: "glass_sand", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.Crystal:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 14, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "crystal", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 32), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -5))]));
                        break;
                    }

                case PkgName.PlayerBoy:
                    {
                        animPkg = MakePackageForRpgMakerV1Data(pkgName: pkgName, scale: 1f, animSize: 0, colWidth: 14, colHeight: 14, altasName: "characters/actor29rec4", gfxOffsetCorrection: new Vector2(0, -9), setNoX: 0, setNoY: 0);
                        break;

                        // , gfxOffsetCorrection: new Vector2(0f, -9f), shadowOriginFactor: new Vector2(0.5f, 0.95f), shadowPosOffset: new Vector2(0f, 1.5f), hasFlatShadow: false
                    }

                case PkgName.PlayerGirl:
                    {
                        animPkg = MakePackageForRpgMakerV1Data(pkgName: pkgName, scale: 1f, animSize: 0, colWidth: 14, colHeight: 14, altasName: "characters/recolor_pt2", gfxOffsetCorrection: new Vector2(0, -9), setNoX: 0, setNoY: 0);
                        break;
                    }

                case PkgName.DragonBonesTestFemaleMage:

                    string[] jsonNameArray = new string[] {
                            "female_mage_tex_cropped.json",
                        };

                    var durationDict = new Dictionary<string, int>
                    {
                        { "stand", 3 },
                        { "weak", 3 },
                        { "walk", 2 },
                        { "attack", 1 },
                        { "dead", 2 },
                        { "damage", 2 },
                    };

                    var switchDict = new Dictionary<string, string>
                    {
                        { "attack", "stand" },
                        { "damage", "stand" },
                    };

                    List<string> nonLoopedAnims = new List<string> { "dead", "attack", "damage" };

                    animPkg = MakePackageForDragonBonesAnims(pkgName: pkgName, colWidth: 20, colHeight: 20, jsonNameArray: jsonNameArray, animSize: 0, scale: 0.5f, baseAnimsFaceRight: false, durationDict: durationDict, switchDict: switchDict, nonLoopedAnims: nonLoopedAnims, globalOffsetCorrection: new Vector2(0, -39));

                    break;

                case PkgName.FoxGinger:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxRed:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxWhite:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxGray:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxBlack:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxChocolate:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxBrown:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.FoxYellow:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/fox", colWidth: 16, colHeight: 20, gfxOffsetCorrection: new Vector2(1, -11), setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog1:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog2:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog3:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog4:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog5:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog6:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog7:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.Frog8:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/frogs_small", colWidth: 13, colHeight: 10, gfxOffsetCorrection: new Vector2(0, -17), setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitBrown:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitDarkBrown:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitGray:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitBlack:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitLightGray:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitBeige:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitWhite:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.RabbitLightBrown:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", colWidth: 14, colHeight: 12, gfxOffsetCorrection: new Vector2(1, -14), setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearBrown:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearWhite:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearOrange:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearBlack:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearDarkBrown:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearGray:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearRed:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.BearBeige:
                    {
                        animPkg = MakePackageForRPGMakerPackageV2UsingSizeDict(pkgName: pkgName, atlasName: "characters/bear", colWidth: 19, colHeight: 18, gfxOffsetCorrection: new Vector2(1, -12), setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.8f }, { 1, 0.9f }, { 2, 1.0f } });
                        break;
                    }

                case PkgName.TentModern:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 87, colHeight: 75);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tent_modern", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 146, height: 205), scale: 0.6f, gfxOffsetCorrection: new Vector2(1, -39), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TentModernPacked:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 33, colHeight: 9);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tent_modern_packed", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 188), scale: 0.12f, gfxOffsetCorrection: new Vector2(4, -50), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TentSmall: // TODO replace with A - frame tent asset(when found)
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 50, colHeight: 27);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tent_medium", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 118, height: 103), scale: 0.5f, gfxOffsetCorrection: new Vector2(2, -22), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TentMedium:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 102, colHeight: 48);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tent_medium", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 118, height: 103), scale: 1f, gfxOffsetCorrection: new Vector2(1, -24), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.TentBig:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 176, colHeight: 92);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "tent_big", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 190, height: 162), scale: 1f, gfxOffsetCorrection: new Vector2(0, -28), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.BackpackSmall:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 208, height: 181, scale: 0.1f, layer: 1, animSize: 0, altasName: "backpack_small", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.BackpackMedium:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 46, height: 46, scale: 0.5f, layer: 1, animSize: 0, altasName: "backpack_medium", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.BackpackBig:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 236, height: 258, scale: 0.1f, layer: 1, animSize: 0, altasName: "backpack_big", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.BackpackLuxurious:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 236, height: 258, scale: 0.1f, layer: 1, animSize: 0, altasName: "backpack_luxurious", hasOnePixelMargin: true);
                        break;
                    }

                case PkgName.BeltSmall:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 21, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "belt_small", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BeltMedium:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 243, height: 134, scale: 0.12f, layer: 0, animSize: 0, altasName: "belt_medium", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BeltBig:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 494, height: 192, scale: 0.06f, layer: 0, animSize: 0, altasName: "belt_big", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BeltLuxurious:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 494, height: 192, scale: 0.06f, layer: 0, animSize: 0, altasName: "belt_luxurious", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Map:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 319, height: 240, scale: 0.06f, layer: 0, animSize: 0, altasName: "map_item", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HatSimple:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 28, scale: 0.5f, layer: 0, animSize: 0, altasName: "hat_simple", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BootsProtective:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 20, height: 25, scale: 1f, layer: 1, animSize: 0, altasName: "boots_protective", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BootsMountain:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 21, height: 25, scale: 1f, layer: 1, animSize: 0, altasName: "boots_mountain", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BootsSpeed:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 27, height: 31, scale: 1f, layer: 1, animSize: 0, altasName: "boots_speed", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BootsAllTerrain:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 17, height: 26, scale: 1f, layer: 1, animSize: 0, altasName: "boots_all_terrain", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.GlovesStrength:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 31, height: 25, scale: 0.7f, layer: 0, animSize: 0, altasName: "gloves_strength", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.GlassesBlue:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 24, scale: 0.5f, layer: 0, animSize: 0, altasName: "glasses_blue", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Dungarees:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 17, height: 27, scale: 1f, layer: 0, animSize: 0, altasName: "dungarees", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.LanternFrame:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 203, height: 438, scale: 0.075f, layer: 0, animSize: 0, altasName: "lantern_frame", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Candle:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 78, height: 182, scale: 0.1f, layer: 0, animSize: 0, altasName: "candle", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Lantern:
                    {
                        AnimFrame frameOn = new AnimFrame(atlasName: "lantern_on", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 201, height: 437), scale: 0.075f, gfxOffsetCorrection: new Vector2(8, -3));

                        AnimFrame frameOff = new AnimFrame(atlasName: "lantern_off", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 201, height: 437), scale: 0.075f, gfxOffsetCorrection: new Vector2(8, -3));

                        animPkg = new(pkgName: pkgName, colWidth: 15, colHeight: 32);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frameOff]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: [frameOff]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frameOn]));
                        break;
                    }

                case PkgName.SmallTorch:
                    {
                        AnimFrame frameOn = new AnimFrame(atlasName: "small_torch_on", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 258, height: 278), scale: 0.07f, gfxOffsetCorrection: new Vector2(-31, -59));

                        AnimFrame frameOff = new AnimFrame(atlasName: "small_torch_off", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 180, height: 157), scale: 0.07f, gfxOffsetCorrection: new Vector2(8, 1));

                        animPkg = new(pkgName: pkgName, colWidth: 13, colHeight: 12);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frameOff]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: [frameOff]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frameOn]));
                        break;
                    }

                case PkgName.BigTorch:
                    {
                        AnimFrame frameOn = new AnimFrame(atlasName: "big_torch_on", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 258, height: 278), scale: 0.1f, gfxOffsetCorrection: new Vector2(44, -59));

                        AnimFrame frameOff = new AnimFrame(atlasName: "big_torch_off", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 180, height: 157), scale: 0.1f, gfxOffsetCorrection: new Vector2(5, 0));

                        animPkg = new(pkgName: pkgName, colWidth: 17, colHeight: 16);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frameOff]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: [frameOff]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frameOn]));
                        break;
                    }

                case PkgName.CampfireSmall:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 16);

                        var frameList = new List<AnimFrame>();

                        for (int i = 0; i < 3; i++)
                        {
                            frameList.Add(new AnimFrame(atlasName: $"campfire_on_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 50, height: 50), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, -8), hasFlatShadow: false));
                        }
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: frameList.ToArray()));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: frameList.ToArray()));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "campfire_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 50, height: 50), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, -8), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.CampfireMedium:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 33, colHeight: 20);

                        var frameList = new List<AnimFrame>();

                        for (int i = 0; i < 3; i++)
                        {
                            frameList.Add(new AnimFrame(atlasName: $"campfire_medium_on_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 36, height: 45), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(0, -8), hasFlatShadow: false));
                        }
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: frameList.ToArray()));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: frameList.ToArray()));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrame(atlasName: "campfire_medium_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 36, height: 45), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(0, -8), hasFlatShadow: false)]));
                        break;
                    }

                case PkgName.BoatConstruction:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 329, colHeight: 66);

                        for (int animSize = 0; animSize <= 5; animSize++)
                        {
                            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: [new AnimFrame(atlasName: $"boat/boat_construction_{animSize}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 500, height: 183), scale: 0.7f, gfxOffsetCorrection: new Vector2(7, -24), ignoreWhenCalculatingMaxSize: true)]));
                        }

                        animPkg.presentationFrame = animPkg.GetAnim(size: 0, name: "default").frameArray[0]; // animSize == 0 should serve as an example (whole blueprint visible)

                        break;
                    }

                case PkgName.BoatCompleteStanding:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 329, colHeight: 66);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "boat/boat_complete", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 500, height: 183), scale: 0.7f, gfxOffsetCorrection: new Vector2(7, -24), ignoreWhenCalculatingMaxSize: true)]));
                        break;
                    }

                case PkgName.BoatCompleteCruising:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 329, colHeight: 66);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "boat/boat_complete", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 500, height: 183), scale: 0.7f, gfxOffsetCorrection: new Vector2(7, -24), ignoreWhenCalculatingMaxSize: true)]));
                        break;
                    }

                case PkgName.ShipRescue:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 1310, colHeight: 1041);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "ship_rescue", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 873, height: 694), scale: 1.5f, ignoreWhenCalculatingMaxSize: true)]));
                        break;
                    }

                case PkgName.HerbsBlack:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_black", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsCyan:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_cyan", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsBlue:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_blue", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsGreen:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_green", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsYellow:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_yellow", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsRed:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_red", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsViolet:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_violet", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsBrown:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_brown", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsDarkViolet:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_dark_violet", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HerbsDarkGreen:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 19, scale: 1f, layer: 0, animSize: 0, altasName: "herbs_dark_green", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.EmptyBottle:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "bottle_empty", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionRed:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_red", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionBlue:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_blue", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionViolet:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_violet", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionYellow:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_yellow", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionCyan:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_cyan", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionGreen:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_green", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionBlack:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_black", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionDarkViolet:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_dark_violet", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionDarkYellow:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_dark_yellow", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionDarkGreen:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_dark_green", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionLightYellow:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "bottle_oil", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionTransparent:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_transparent", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.PotionBrown:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "potion_brown", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BloodSplatter1:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 34, scale: 1f, layer: 0, animSize: 0, altasName: "blood_splatter_1", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BloodSplatter2:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 1f, layer: 0, animSize: 0, altasName: "blood_splatter_2", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BloodSplatter3:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 27, height: 24, scale: 1f, layer: 0, animSize: 0, altasName: "blood_splatter_3", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.HumanSkeleton:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 1f, layer: 0, animSize: 0, altasName: "human_skeleton", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Hole:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 31, scale: 1f, layer: 0, animSize: 0, altasName: "hole", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Explosion:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 48, colHeight: 48); // colRect does not match bigger sizes (gfxRect should be used for detecting collisions instead)

                        for (byte size = 0; size < 4; size++)
                        {
                            var frameList = new List<AnimFrame>();

                            for (int y = 0; y < 4; y++)
                            {
                                for (int x = 0; x < 4; x++)
                                {
                                    frameList.Add(new AnimFrame(atlasName: "explosion", layer: 1, cropRect: new Rectangle(x: x * 32, y: y * 32, width: 32, height: 32), scale: 1.5f * (size + 1), duration: 2, ignoreWhenCalculatingMaxSize: true));
                                }
                            }

                            animPkg.AddAnim(new(animPkg: animPkg, size: size, frameArray: frameList.ToArray()));
                        }

                        break;
                    }

                case PkgName.SkullAndBones:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 32, scale: 1f, layer: 2, animSize: 0, altasName: "skull_and_bones", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.MusicNoteSmall:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 18, height: 21, scale: 1f, layer: 2, animSize: 0, altasName: "music_note", hasOnePixelMargin: false);
                        break;
                    };

                case PkgName.MusicNoteBig:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 18, height: 21, scale: 2.5f, layer: 2, animSize: 0, altasName: "music_note", hasOnePixelMargin: false);
                        break;
                    };

                case PkgName.Miss:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 40, height: 21, scale: 1f, layer: 2, animSize: 0, altasName: "miss", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Attack:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 34, colHeight: 58);

                        var frameList = new List<AnimFrame>();

                        frameList.Add(new AnimFrame(atlasName: "attack_1", layer: 2, cropRect: new Rectangle(x: 0, y: 0, width: 18, height: 18), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(0, 0)));
                        frameList.Add(new AnimFrame(atlasName: "attack_2", layer: 2, cropRect: new Rectangle(x: 0, y: 0, width: 28, height: 30), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(0, 0)));
                        frameList.Add(new AnimFrame(atlasName: "attack_3", layer: 2, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 58), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(0, 0)));

                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: frameList.ToArray()));

                        break;
                    }

                case PkgName.MapMarker:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 16, height: 16, scale: 1f, layer: 2, animSize: 0, altasName: "map_marker", hasOnePixelMargin: false);
                        break;
                    };

                case PkgName.Backlight:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 26, colHeight: 13);

                        AnimFrame frame1 = new AnimFrame(atlasName: "backlight_1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 17, height: 9), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, 1));
                        AnimFrame frame2 = new AnimFrame(atlasName: "backlight_2", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 21, height: 11), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, 1));
                        AnimFrame frame3 = new AnimFrame(atlasName: "backlight_3", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 26, height: 13), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, 1));
                        AnimFrame frame4 = new AnimFrame(atlasName: "backlight_4", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 14), scale: 1f, duration: 20, gfxOffsetCorrection: new Vector2(1, 1));

                        var frameList = new List<AnimFrame> { frame1, frame2, frame3, frame4, frame3, frame2, frame1 };

                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: frameList.ToArray()));

                        break;
                    }

                case PkgName.Crosshair:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 40, height: 40, scale: 1f, layer: 2, animSize: 0, altasName: "crosshair", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Flame:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 46); // colRect does not match bigger sizes (gfxRect should be used for detecting collisions instead)

                        int animSize = 0;

                        foreach (float scale in new List<float> { 0.5f, 0.75f, 1f, 1.25f })
                        {
                            var frameList = new List<AnimFrame>();

                            for (int i = 0; i < 3; i++)
                            {
                                frameList.Add(new AnimFrame(atlasName: $"flame_small_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 40, height: 46), scale: scale, duration: 6, gfxOffsetCorrection: new Vector2(1, 1)));
                            }

                            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: frameList.ToArray()));

                            animSize++;
                        }

                        foreach (float scale in new List<float> { 1f, 1.5f, 1.8f, 2f })
                        {
                            var frameList = new List<AnimFrame>();

                            for (int i = 0; i < 3; i++)
                            {
                                frameList.Add(new AnimFrame(atlasName: $"flame_big_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 70, height: 66), scale: scale, duration: 6, gfxOffsetCorrection: new Vector2(1, 1)));
                            }

                            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: frameList.ToArray()));

                            animSize++;
                        }

                        break;
                    }

                case PkgName.Upgrade:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 31, height: 31, scale: 1f, layer: 0, animSize: 0, altasName: "upgrade", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.WaterDrop:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 124, height: 183, scale: 0.5f, layer: 0, animSize: 0, altasName: "water_drop", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Zzz:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 39, height: 15, scale: 1f, layer: 2, animSize: 0, altasName: "zzz", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Heart:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 18, height: 18, scale: 1f, layer: 2, animSize: 0, altasName: "heart_16x16", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Hammer:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 182, height: 336, scale: 0.1f, layer: 0, animSize: 0, altasName: "hammer", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.Fog1:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 352, height: 352, scale: 1.4f, layer: 2, animSize: 0, altasName: "fog_1", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.Fog2:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 352, height: 352, scale: 1.4f, layer: 2, animSize: 0, altasName: "fog_2", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.Fog3:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 352, height: 352, scale: 1.8f, layer: 2, animSize: 0, altasName: "fog_1", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.Fog4:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 352, height: 352, scale: 1.8f, layer: 2, animSize: 0, altasName: "fog_2", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.BubbleExclamationRed:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 128, height: 131, scale: 0.2f, layer: 2, animSize: 0, altasName: "bubble_exclamation_red", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BubbleExclamationBlue:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 128, height: 131, scale: 0.2f, layer: 2, animSize: 0, altasName: "bubble_exclamation_blue", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.BubbleCraftGreen:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 128, height: 131, scale: 0.2f, layer: 2, animSize: 0, altasName: "bubble_craft_green", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.SeaWave:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 40); // colRect does not match bigger sizes (gfxRect should be used for detecting collisions instead)

                        foreach (var kvp in new Dictionary<int, float> { { 0, 0.2f }, { 1, 0.3f }, { 2, 0.4f }, { 3, 0.5f }, { 4, 0.6f }, { 5, 0.7f }, { 6, 0.8f } })
                        {
                            int animSize = kvp.Key;
                            float scale = kvp.Value;

                            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: [new AnimFrame(atlasName: "wave", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 89, height: 315), scale: scale, ignoreWhenCalculatingMaxSize: true)]));
                        }
                        break;
                    }

                case PkgName.WeatherFog1:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 85, scale: 1.0f, layer: 2, animSize: 0, altasName: "weather_fog_1", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog2:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 84, scale: 1.0f, layer: 2, animSize: 0, altasName: "weather_fog_2", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog3:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 85, scale: 1.0f, layer: 2, animSize: 0, altasName: "weather_fog_3", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog4:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 85, scale: 1.4f, layer: 2, animSize: 0, altasName: "weather_fog_1", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog5:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 84, scale: 1.4f, layer: 2, animSize: 0, altasName: "weather_fog_2", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog6:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 85, scale: 1.4f, layer: 2, animSize: 0, altasName: "weather_fog_3", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog7:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 85, scale: 2.0f, layer: 2, animSize: 0, altasName: "weather_fog_1", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog8:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 84, scale: 2.0f, layer: 2, animSize: 0, altasName: "weather_fog_2", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.WeatherFog9:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 130, height: 85, scale: 2.0f, layer: 2, animSize: 0, altasName: "weather_fog_3", hasOnePixelMargin: true, ignoreWhenCalculatingMaxSize: true);
                        break;
                    };

                case PkgName.FertileGroundSmall:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 65, height: 55, scale: 1.0f, layer: -1, animSize: 0, altasName: "fertile_ground_small", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.FertileGroundMedium:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 102, height: 75, scale: 1.0f, layer: -1, animSize: 0, altasName: "fertile_ground_medium", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.FertileGroundLarge:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 134, height: 134, scale: 1.0f, layer: -1, animSize: 0, altasName: "fertile_ground_big", hasOnePixelMargin: true);
                        break;
                    };

                case PkgName.FenceHorizontalShort:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 120, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "fence_horizontal_short", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 122, height: 50), scale: 1f, gfxOffsetCorrection: new Vector2(0, -19))]));
                        break;
                    }

                case PkgName.FenceVerticalShort:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 13, colHeight: 121);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "fence_vertical_short", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 14, height: 146), scale: 1f, gfxOffsetCorrection: new Vector2(0, -11))]));
                        break;
                    }

                case PkgName.FenceHorizontalLong:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 224, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "fence_horizontal_long", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 226, height: 50), scale: 1f, gfxOffsetCorrection: new Vector2(0, -19))]));
                        break;
                    }

                case PkgName.FenceVerticalLong:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 13, colHeight: 219);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrame(atlasName: "fence_vertical_long", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 14, height: 244), scale: 1f, gfxOffsetCorrection: new Vector2(0, -11))]));
                        break;
                    }

                case PkgName.CaveEntrance:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 92, colHeight: 53);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: [new AnimFrame(atlasName: "cave_entrance_open", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 47, height: 28), scale: 2f)]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "blocked", size: 0, frameArray: [new AnimFrame(atlasName: "cave_entrance_blocked", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 47, height: 28), scale: 2f)]));
                        break;
                    }

                case PkgName.CaveExit:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 84, colHeight: 55);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: 0, frameArray: [new AnimFrame(atlasName: "cave_exit", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 43, height: 29), scale: 2f, gfxOffsetCorrection: new Vector2(-0, -0))]));
                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported pkgName - {pkgName}.");
            }

            animPkg.AddDefaultAnimsIfMissing();

            pkgByName[pkgName] = animPkg;
        }

        public static AnimPkg MakePackageForSingleImage(PkgName pkgName, string altasName, int width, int height, int animSize, int layer, bool hasOnePixelMargin = false, float scale = 1f, bool mirrorX = false, bool mirrorY = false, bool ignoreWhenCalculatingMaxSize = false)
        {
            int colWidth = (int)((hasOnePixelMargin ? width - 2 : width) * scale);
            int colHeight = (int)((hasOnePixelMargin ? height - 2 : height) * scale);

            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: [new AnimFrame(atlasName: altasName, layer: layer, cropRect: new Rectangle(x: 0, y: 0, width: width, height: height), duration: 0, mirrorX: mirrorX, mirrorY: mirrorY, scale: scale, ignoreWhenCalculatingMaxSize: ignoreWhenCalculatingMaxSize)]));

            return animPkg;
        }

        private static AnimPkg AddChestPackage(PkgName pkgName, int colWidth, int colHeight, string atlasNameBase, Rectangle cropRect, int frameDuration, int animLength, float scale, Vector2 gfxOffsetCorrection)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

            var openingFrameArray = new AnimFrame[animLength];
            var closingFrameArray = new AnimFrame[animLength];

            for (int i = 0; i < animLength; i++)
            {
                openingFrameArray[i] = new AnimFrame(atlasName: $"{atlasNameBase}{i + 1}", layer: 1, cropRect: cropRect, duration: i < animLength - 1 ? frameDuration : 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, hasFlatShadow: false);
                closingFrameArray[animLength - i - 1] = new AnimFrame(atlasName: $"{atlasNameBase}{i + 1}", layer: 1, cropRect: cropRect, duration: i > 0 ? frameDuration : 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, hasFlatShadow: false);
            }

            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "opening", frameArray: openingFrameArray));
            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "closing", frameArray: closingFrameArray));
            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "closed", frameArray: [openingFrameArray[0]]));
            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "open", frameArray: [closingFrameArray[0]]));

            return animPkg;
        }

        public static AnimPkg MakePackageForRpgMakerV1Data(PkgName pkgName, string altasName, int colWidth, int colHeight, Vector2 gfxOffsetCorrection, int setNoX, int setNoY, int animSize, float scale = 1f)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: (int)(colWidth * scale), colHeight: (int)(colHeight * scale));

            int setOffsetX = setNoX * 96;
            int setOffsetY = setNoY * 128;
            int width = 32;
            int height = 32;

            var yByDirection = new Dictionary<string, int>(){
                { "down", 0 },
                { "up", height * 3 },
                { "right", height * 2 },
                { "left", height },
                };

            bool defaultSet = false;

            // standing
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrame>
                {
                    new AnimFrame(atlasName: altasName, layer: 1, cropRect: new Rectangle(x: width + setOffsetX, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, hasFlatShadow: false)
                };

                animPkg.AddAnim(new(animPkg: animPkg, name: $"stand-{animName}", size: animSize, frameArray: frameList.ToArray()));
                if (!defaultSet)
                {
                    animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: animSize, frameArray: new AnimFrame[] { frameList[0] }));
                    defaultSet = true;
                }
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrame> { };

                foreach (int x in new int[] { setOffsetX + width, setOffsetX + (width * 2) })
                {
                    frameList.Add(new AnimFrame(atlasName: altasName, layer: 1, cropRect: new Rectangle(x: x, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 8, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection));
                }

                animPkg.AddAnim(new(animPkg: animPkg, name: $"walk-{animName}", size: animSize, frameArray: frameList.ToArray()));
            }

            return animPkg;
        }

        public static AnimPkg MakePackageForRPGMakerPackageV2UsingSizeDict(PkgName pkgName, string atlasName, int colWidth, int colHeight, int setNoX, int setNoY, Dictionary<byte, float> scaleForSizeDict, Vector2 gfxOffsetCorrection)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

            foreach (var kvp in scaleForSizeDict)
            {
                byte animSize = kvp.Key;
                float scale = kvp.Value;
                MakePackageForRpgMakerV2Data(pkgName: pkgName, atlasName: atlasName, setNoX: setNoX, setNoY: setNoY, animSize: animSize, scale: scale, colWidth: colWidth, colHeight: colHeight, gfxOffsetCorrection: gfxOffsetCorrection, animPkg: animPkg);
            }

            return animPkg;
        }

        public static AnimPkg MakePackageForRpgMakerV2Data(PkgName pkgName, string atlasName, int colWidth, int colHeight, Vector2 gfxOffsetCorrection, int setNoX, int setNoY, int animSize, float scale = 1f, AnimPkg animPkg = null)
        {
            if (animPkg == null) animPkg = new(pkgName: pkgName, colWidth: (int)(colWidth * scale), colHeight: (int)(colHeight * scale));

            int setOffsetX = setNoX * 144;
            int setOffsetY = setNoY * 192;
            int width = 48;
            int height = 48;

            var yByDirection = new Dictionary<string, int>(){
                { "down", 0 },
                { "up", height * 3 },
                { "right", height * 2 },
                { "left", height },
                };

            bool defaultSet = false;

            // standing
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrame>
                {
                    new AnimFrame(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: width + setOffsetX, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, hasFlatShadow: false)
                };

                animPkg.AddAnim(new(animPkg: animPkg, name: $"stand-{animName}", size: animSize, frameArray: frameList.ToArray()));
                if (!defaultSet)
                {
                    animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: animSize, frameArray: new AnimFrame[] { frameList[0] }));
                    defaultSet = true;
                }
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrame> { };

                foreach (int x in new int[] { setOffsetX, setOffsetX + width, setOffsetX + (width * 2), setOffsetX + width })
                {
                    frameList.Add(new AnimFrame(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: x, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 8, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection));
                }

                animPkg.AddAnim(new(animPkg: animPkg, name: $"walk-{animName}", size: animSize, frameArray: frameList.ToArray()));
            }

            return animPkg;
        }

        public static AnimPkg MakePackageForDragonBonesAnims(PkgName pkgName, int colWidth, int colHeight, string[] jsonNameArray, int animSize, float scale, bool baseAnimsFaceRight, Dictionary<string, int> durationDict = null, List<string> nonLoopedAnims = null, Dictionary<string, Vector2> offsetDict = null, Dictionary<string, string> switchDict = null, Vector2 globalOffsetCorrection = default)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight, horizontalOrientationsOnly: true);

            durationDict ??= [];
            offsetDict ??= [];
            switchDict ??= [];
            nonLoopedAnims ??= [];
            if (globalOffsetCorrection == default) globalOffsetCorrection = Vector2.Zero;

            // in case an offset is needed for individual animations: var offsetDict = new Dictionary<string, Vector2> { { "stand-left", new Vector2(0, 0) }, };

            foreach (string jsonName in jsonNameArray)
            {
                string jsonPath = Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones", jsonName);
                object jsonData = FileReaderWriter.LoadJson(path: jsonPath, useStreamReader: true); // StreamReader is necessary for Android (otherwise, DirectoryNotFound will occur)
                JObject jsonDict = (JObject)jsonData;

                string atlasName = $"_DragonBones/{((string)jsonDict["imagePath"]).Replace(".png", "")}";
                var animDataList = jsonDict["SubTexture"];

                var animDict = new Dictionary<string, Dictionary<int, AnimFrame>>();

                foreach (var animData in animDataList)
                {
                    string name = ((string)animData["name"]).ToLower();
                    int underscoreIndex = name.LastIndexOf('_');
                    int frameNoIndex = underscoreIndex + 1;

                    string baseAnimName = name.Substring(0, underscoreIndex);
                    int duration = durationDict.ContainsKey(baseAnimName) ? durationDict[baseAnimName] : 1;
                    int frameNo = Convert.ToInt32(name.Substring(frameNoIndex, name.Length - frameNoIndex));

                    int x = (int)animData["x"];
                    int y = (int)animData["y"];
                    int croppedWidth = (int)animData["width"];
                    int croppedHeight = (int)animData["height"];
                    int fullWidth = (int)animData["frameWidth"];
                    int fullHeight = (int)animData["frameHeight"];
                    int drawXOffset = (int)animData["frameX"]; // draw offset needed to maintain uncropped pos
                    int drawYOffset = (int)animData["frameY"]; // draw offset needed to maintain uncropped pos

                    foreach (string direction in new string[] { "right", "left" })
                    {
                        string animNameWithDirection = $"{baseAnimName}-{direction}";
                        if (!animDict.ContainsKey(animNameWithDirection)) animDict[animNameWithDirection] = [];

                        bool mirrorX = (direction == "left" && baseAnimsFaceRight) || (direction == "right" && !baseAnimsFaceRight);

                        Vector2 gfxOffsetCorrection = new Vector2(mirrorX ? drawXOffset : -drawXOffset, -drawYOffset) / 2f;
                        gfxOffsetCorrection.X += fullWidth / 6.2f * (mirrorX ? 1f : -1f); // 6.2f seems to center animation properly

                        if (offsetDict.ContainsKey(animNameWithDirection)) gfxOffsetCorrection += offsetDict[animNameWithDirection]; // corrections for individual anims
                        gfxOffsetCorrection += globalOffsetCorrection;

                        AnimFrame animFrame = new AnimFrame(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: x, y: y, width: croppedWidth, height: croppedHeight), duration: duration, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, mirrorX: mirrorX, hasFlatShadow: false);

                        animDict[animNameWithDirection][frameNo] = animFrame;
                    }
                }

                foreach (var kvp1 in animDict)
                {
                    string animNameWithDirection = (string)kvp1.Key;

                    int joinerIndex = animNameWithDirection.LastIndexOf('-');
                    string animName = animNameWithDirection.Substring(0, joinerIndex);
                    string direction = animNameWithDirection.Substring(joinerIndex + 1, animNameWithDirection.Length - joinerIndex - 1);

                    var frameDict = kvp1.Value;
                    int framesCount = frameDict.Keys.Max() + 1;

                    bool nonLoopedAnim = nonLoopedAnims.Where(n => animNameWithDirection.Contains(n)).Any();

                    AnimFrame[] frameArray = new AnimFrame[framesCount];
                    foreach (var kvp2 in frameDict)
                    {
                        int frameNo = kvp2.Key;
                        AnimFrame animFrame = kvp2.Value;

                        if (nonLoopedAnim && frameNo == framesCount - 1)
                        {
                            animFrame = new AnimFrame(atlasName: animFrame.atlasName, layer: animFrame.layer, cropRect: animFrame.cropRect, duration: 0, scale: animFrame.scale, gfxOffsetCorrection: animFrame.gfxOffsetCorrection / animFrame.scale, mirrorX: animFrame.spriteEffects == SpriteEffects.FlipHorizontally, ignoreWhenCalculatingMaxSize: animFrame.ignoreWhenCalculatingMaxSize);
                        }

                        frameArray[frameNo] = animFrame;
                    }

                    string switchName = switchDict.ContainsKey(animName) ? $"{switchDict[animName]}-{direction}" : "";
                    animPkg.AddAnim(new(animPkg: animPkg, name: animNameWithDirection, size: animSize, frameArray: frameArray, switchName: switchName));
                }
            }

            return animPkg;
        }
    }
}