using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimData
    {
        public readonly static Dictionary<string, AnimFrame> frameById = new Dictionary<string, AnimFrame>(); // needed to access frames directly by id (for loading and saving game)
        public static readonly Dictionary<string, List<AnimFrame>> frameListById = new Dictionary<string, List<AnimFrame>>();
        public static readonly Dictionary<PkgName, AnimFrame> framesForPkgs = new Dictionary<PkgName, AnimFrame>(); // default frames for packages

        public enum PkgName
        {
            NoAnim,
            BloodSplatter1,
            BloodSplatter2,
            BloodSplatter3,
            Cactus,
            FlowersWhite,
            FlowersYellow1,
            FlowersYellow2,
            GrassDesert,
            GrassRegular,
            MineralsBig1,
            MineralsBig2,
            MineralsBig3,
            MineralsBig4,
            MineralsSmall1,
            MineralsSmall2,
            MineralsSmall3,
            MineralsSmall4,

            PalmTree,
            Rushes,

            Clam,
            Shell1,
            Shell2,
            Shell3,
            Shell4,
            TreeBig,
            TreeSmall1,
            TreeSmall2,
            WaterLily1,
            WaterLily2,
            WaterLily3,
            WaterRock1,
            WaterRock2,
            WaterRock3,
            WaterRock4,
            WaterRock5,
            WaterRock6,
            WaterRock7,

            Zzz,
            Heart,
            Miss,
            Attack,
            Backlight,
            Crosshair,
            Exclamation,
            Flame,
            WaterDrop,

            ChestWooden,
            ChestMetal,
            WoodLog,
            WoodPlank,
            Crate,

            WorkshopBasic,
            WorkshopAdvanced,
            WorkshopAlchemy,

            CookingPot,
            Furnace,
            Nail,

            Stick1,
            Stick2,
            Stick3,
            Stick4,
            Stick5,
            Stick6,
            Stick7,
            Stone1,
            Stone2,

            Apple,
            Banana,
            Cherry,
            Tomato,
            Acorn,

            RawMeat,
            Fat,
            CookedMeat,
            MealStandard,
            Leather,

            TomatoPlant,

            Hand,

            AxeWood,
            AxeStone,
            AxeIron,

            PickaxeWood,
            PickaxeStone,
            PickaxeIron,

            Scythe,
            SpearStone,
            SpearPoisoned,
            SpearIron,


            BowWood,

            ArrowWood,
            ArrowPoisoned,
            ArrowIron,
            StoneSmall,

            CoalDeposit,
            IronDeposit,
            GlassDeposit,

            Coal,
            IronOre,
            IronBar,
            GlassSand,

            Blonde,
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
            BloodDrop1,
            BloodDrop2,
            BloodDrop3,

            TentSmall,
            TentMedium,
            TentBig,

            Bag,
            BagOutline,
            BackpackMedium,
            BeltMedium,
            Map,

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

            Skeleton,

            EmptyBottle,
            PotionRed,
            PotionBlue,
            PotionYellow,
            PotionCyan,
            PotionBlack,
            PotionLightYellow
        }

        public static void AddFrameList(PkgName animPackage, byte animSize, string animName, List<AnimFrame> frameList)
        {
            if (!framesForPkgs.ContainsKey(animPackage)) framesForPkgs[animPackage] = frameList[0];

            string completeAnimID = $"{animPackage}-{animSize}-{animName}";
            frameListById[completeAnimID] = new List<AnimFrame>(frameList);
        }

        public static void CreateAllAnims()
        {
            {
                PkgName packageName = PkgName.GrassRegular;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                    frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                    frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 320, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.GrassDesert;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 160, y: 224, width: 24, height: 24));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 288, y: 160, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersYellow1;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.FlowersYellow2;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 832, y: 128, width: 64, height: 64, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 768, y: 0, width: 64, height: 64, scale: 0.5f));
            }
            {
                PkgName packageName = PkgName.FlowersWhite;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 32, y: 352, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Rushes;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 96, y: 192, width: 32, height: 32));
            }
            {
                PkgName packageName = PkgName.Cactus;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 352, y: 320, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 288, y: 320, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.PalmTree;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "palmtree_small", layer: 1));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 123, y: 326, width: 69, height: 80, crop: false));
                AddFrameList(animPackage: packageName, animSize: 2, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 8, y: 145, width: 66, height: 102, crop: false));
                AddFrameList(animPackage: packageName, animSize: 3, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 108, y: 145, width: 72, height: 102, crop: false));
            }
            {
                PkgName packageName = PkgName.TreeSmall1;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 192, y: 352, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "dead",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 448, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 160, y: 320, width: 32, height: 64));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "dead",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 0, y: 416, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeSmall2;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 128, y: 192, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "dead",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 448, width: 32, height: 32));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 160, y: 192, width: 32, height: 64));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "dead",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 0, y: 416, width: 32, height: 64));
            }
            {
                PkgName packageName = PkgName.TreeBig;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 199, y: 382, width: 47, height: 66));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "dead",
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 0, y: 224, width: 64, height: 64));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 0, y: 0, width: 63, height: 97));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "dead",
                frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 0, y: 224, width: 64, height: 64));
            }
            {
                PkgName packageName = PkgName.TomatoPlant;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "celianna_farmnature_crops_transparent", layer: 1, x: 297, y: 8, width: 14, height: 13));
                AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "celianna_farmnature_crops_transparent", layer: 1, x: 328, y: 4, width: 17, height: 24));
            }
            {
                PkgName packageName = PkgName.ChestWooden;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "open",
                frameList: ConvertImageToFrameList(atlasName: "chests", layer: 0, x: 0, y: 144, width: 32, height: 48));
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 48, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 96, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 144, width: 32, height: 48, duration: 0));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "opening", frameList: frameList);
                frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 96, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 48, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 0, width: 32, height: 48, duration: 0));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.ChestMetal;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "open",
                frameList: ConvertImageToFrameList(atlasName: "chests", layer: 0, x: 32, y: 144, width: 32, height: 48));
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 48, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 96, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 144, width: 32, height: 48, duration: 0));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "opening", frameList: frameList);
                frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 96, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 48, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 0, width: 32, height: 48, duration: 0));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Crate;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "chests", layer: 0, x: 128, y: 0, width: 32, height: 48));
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 96, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 48, width: 32, height: 48, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 0, width: 32, height: 48, duration: 0));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.WorkshopBasic;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
                frameList: ConvertImageToFrameList(atlasName: "workshop_basic", layer: 1, scale: 0.5f)); ; // the same as "off"
            }
            {
                PkgName packageName = PkgName.WorkshopAdvanced;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
                frameList: ConvertImageToFrameList(atlasName: "workshop_advanced", layer: 1, scale: 0.5f)); ; // the same as "off"
            }
            {
                PkgName packageName = PkgName.WorkshopAlchemy;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                frameList: ConvertImageToFrameList(atlasName: "alchemy_lab", layer: 1, scale: 0.5f));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
                frameList: ConvertImageToFrameList(atlasName: "alchemy_lab", layer: 1, scale: 0.5f)); ; // the same as "off"
            }
            {
                PkgName packageName = PkgName.Furnace;
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
                frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 0, y: 144, width: 48, height: 48));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
                frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 48, y: 144, width: 48, height: 48));
            }
            {
                PkgName packageName = PkgName.CookingPot;
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 192, y: 144, width: 32, height: 32, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 224, y: 144, width: 32, height: 32, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 256, y: 144, width: 32, height: 32, duration: 6));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: frameList);
                frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 0, y: 144, width: 32, height: 32, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 32, y: 144, width: 32, height: 32, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 64, y: 144, width: 32, height: 32, duration: 6));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Backlight;
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_1", layer: 0, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_2", layer: 0, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_3", layer: 0, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_4", layer: 0, duration: 20));
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_3", layer: 0, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_2", layer: 0, duration: 6));
                frameList.Add(ConvertImageToFrame(atlasName: "backlight_1", layer: 0, duration: 6));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Attack;
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 0, y: 20, width: 23, height: 23));
                frameList.Add(ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 22, y: 13, width: 29, height: 33));
                frameList.Add(ConvertImageToFrame(atlasName: "attack", layer: 2, duration: 6, x: 50, y: 0, width: 36, height: 65));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: frameList);
            }
            {
                PkgName packageName = PkgName.Flame;
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 432, y: 48, width: 48, height: 48));
                frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 480, y: 48, width: 48, height: 48));
                frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 528, y: 48, width: 48, height: 48));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: frameList);
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
            {
                PkgName packageName = PkgName.Campfire;
                var frameList = new List<AnimFrame>();
                frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 288, y: 0, width: 48, height: 48, crop: false));
                frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 336, y: 0, width: 48, height: 48, crop: false));
                frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 384, y: 0, width: 48, height: 48, crop: false));
                AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: frameList);
                frameList = ConvertImageToFrameList(atlasName: "flames", layer: 1, x: 288, y: 96, width: 48, height: 48, crop: false);
                AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList);
                AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList);
            }

            AddFrameList(animPackage: PkgName.NoAnim, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "no_anim", layer: 1, x: 0, y: 0, width: 0, height: 0));

            AddFrameList(animPackage: PkgName.WaterLily1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 384, y: 64, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.WaterLily2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 416, y: 0, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.WaterLily3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 448, y: 0, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.MineralsSmall1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 192, y: 256, width: 27, height: 32));

            AddFrameList(animPackage: PkgName.MineralsSmall2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 224, y: 256, width: 30, height: 32));

            AddFrameList(animPackage: PkgName.MineralsSmall3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 224, y: 320, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.MineralsSmall4, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 896, y: 192, width: 64, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.MineralsBig1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 128, y: 320, width: 64, height: 51));

            AddFrameList(animPackage: PkgName.MineralsBig2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 192, y: 320, width: 32, height: 64));

            AddFrameList(animPackage: PkgName.MineralsBig3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 224, y: 320, width: 32, height: 64));

            AddFrameList(animPackage: PkgName.MineralsBig4, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 896, y: 256, width: 128, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Clam, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 256, width: 64, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Shell1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 192, y: 256, width: 64, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Shell2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 192, y: 320, width: 64, height: 64, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Shell3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 594, y: 975, width: 28, height: 37, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Shell4, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 596, y: 848, width: 33, height: 33, scale: 0.5f));

            AddFrameList(animPackage: PkgName.WaterRock1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 256, y: 0, width: 32, height: 28));

            AddFrameList(animPackage: PkgName.WaterRock2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 292, y: 2, width: 21, height: 21));

            AddFrameList(animPackage: PkgName.WaterRock3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 322, y: 0, width: 27, height: 26));

            AddFrameList(animPackage: PkgName.WaterRock4, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 361, y: 0, width: 43, height: 30));

            AddFrameList(animPackage: PkgName.WaterRock5, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 256, y: 64, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.WaterRock6, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 324, y: 38, width: 27, height: 49));

            AddFrameList(animPackage: PkgName.WaterRock7, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 288, y: 32, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodSplatter1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 416, y: 320, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodSplatter2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 320, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodSplatter3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 288, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.BloodDrop1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 451, y: 290, width: 7, height: 8));

            AddFrameList(animPackage: PkgName.BloodDrop2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 456, y: 326, width: 8, height: 5));

            AddFrameList(animPackage: PkgName.BloodDrop3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 467, y: 336, width: 7, height: 6));

            AddFrameList(animPackage: PkgName.WoodLog, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 98, y: 48, width: 45, height: 46, scale: 0.5f));

            AddFrameList(animPackage: PkgName.WoodPlank, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 32, y: 0, width: 32, height: 32, scale: 1f));

            AddFrameList(animPackage: PkgName.Stick1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 26, y: 73, width: 25, height: 21));

            AddFrameList(animPackage: PkgName.Stick2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 100, y: 73, width: 25, height: 22));

            AddFrameList(animPackage: PkgName.Stick3, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 23, y: 105, width: 25, height: 25));

            AddFrameList(animPackage: PkgName.Stick4, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 100, y: 104, width: 25, height: 25));

            AddFrameList(animPackage: PkgName.Stick5, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 1, x: 22, y: 72, width: 25, height: 25));

            AddFrameList(animPackage: PkgName.Stick6, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 1, x: 53, y: 68, width: 25, height: 25));

            AddFrameList(animPackage: PkgName.Stick7, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 1, x: 100, y: 70, width: 25, height: 25));

            AddFrameList(animPackage: PkgName.Stone1, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "stones", layer: 1, x: 11, y: 92, width: 23, height: 14));

            AddFrameList(animPackage: PkgName.Stone2, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "stones", layer: 1, x: 64, y: 78, width: 16, height: 16));

            AddFrameList(animPackage: PkgName.Apple, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 160, y: 192, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.Banana, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 192, y: 192, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.Cherry, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 0, y: 192, width: 32, height: 32, scale: 0.75f));

            AddFrameList(animPackage: PkgName.Tomato, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 232, y: 207, width: 18, height: 18, scale: 0.5f));

            AddFrameList(animPackage: PkgName.RawMeat, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "steak_t-bone", layer: 0, x: 3, y: 9, width: 27, height: 23, scale: 0.75f));

            AddFrameList(animPackage: PkgName.MealStandard, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "fancy_food2", layer: 0, x: 288, y: 64, width: 32, height: 32, scale: 0.5f));

            AddFrameList(animPackage: PkgName.AxeWood, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "axe_wooden", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));

            AddFrameList(animPackage: PkgName.AxeStone, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 0, y: 0, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.AxeIron, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 48, y: 0, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.PickaxeWood, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 0, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeStone, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.PickaxeIron, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 384, width: 48, height: 48, scale: 0.7f));

            AddFrameList(animPackage: PkgName.BowWood, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "bows", layer: 0, x: 522, y: 17, width: 22, height: 50, scale: 0.7f));

            AddFrameList(animPackage: PkgName.DebrisWood, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 0, x: 100, y: 70, width: 25, height: 25, scale: 0.3f));

            AddFrameList(animPackage: PkgName.IronDeposit, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 96, y: 96, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.GlassDeposit, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 1, x: 288, y: 240, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.CoalDeposit, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 0, y: 96, width: 48, height: 48, scale: 1f));

            AddFrameList(animPackage: PkgName.IronOre, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.GlassSand, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.IronBar, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 96, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Coal, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 288, y: 144, width: 48, height: 48, scale: 0.5f));

            AddFrameList(animPackage: PkgName.Leather, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 96, y: 96, width: 32, height: 32, scale: 0.75f));

            AddFrameList(animPackage: PkgName.Nail, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 0, y: 0, width: 32, height: 32, scale: 0.5f));

            AddFrameList(animPackage: PkgName.TentSmall, animSize: 0, animName: "default", // TODO replace with A - frame tent asset(when found)
                frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 0.5f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.TentMedium, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 1f, depthPercent: 0.45f));

            AddFrameList(animPackage: PkgName.TentBig, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tent_big", layer: 1, x: 15, y: 0, width: 191, height: 162, scale: 1f, depthPercent: 0.6f));

            AddFrameList(animPackage: PkgName.Skeleton, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_rtp-addons", layer: 0, x: 320, y: 160, width: 32, height: 32));

            AddFrameList(animPackage: PkgName.PotionRed, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 384, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionBlue, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 448, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionYellow, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 352, y: 128, width: 32, height: 32, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionCyan, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 288, y: 128, width: 32, height: 32, scale: 0.5f));

            // "one-liners"

            AddFrameList(animPackage: PkgName.PotionBlack, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "potion_black", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.PotionLightYellow, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "bottle_oil", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.CookedMeat, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "Cooked Meat", layer: 0, scale: 0.17f));
            AddFrameList(animPackage: PkgName.WaterDrop, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "water_drop", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Exclamation, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "exclamation", layer: 2, scale: 0.2f));
            AddFrameList(animPackage: PkgName.Map, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "parchment", layer: 0, scale: 0.03f));
            AddFrameList(animPackage: PkgName.Bag, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "bag", layer: 1, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BagOutline, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "bag_outline", layer: 1, scale: 0.2f));
            AddFrameList(animPackage: PkgName.BackpackMedium, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "backpack", layer: 1, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Miss, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "miss", layer: 2));
            AddFrameList(animPackage: PkgName.Zzz, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "zzz", layer: 2));
            AddFrameList(animPackage: PkgName.Heart, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2));
            AddFrameList(animPackage: PkgName.Crosshair, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "crosshair", layer: 2));
            AddFrameList(animPackage: PkgName.Scythe, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "scythe", layer: 0));
            AddFrameList(animPackage: PkgName.EmptyBottle, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "bottle_empty", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Acorn, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "acorn", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.ArrowWood, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "arrow_wood", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowPoisoned, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "arrow_poison", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.ArrowIron, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "arrow_iron", layer: 0, scale: 0.75f));
            AddFrameList(animPackage: PkgName.DebrisStone, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "stone_small", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.DebrisPlant, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "grass_blade", layer: 0, scale: 0.25f));
            AddFrameList(animPackage: PkgName.Hand, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "hand", layer: 1));
            AddFrameList(animPackage: PkgName.BeltMedium, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "belt", layer: 1, scale: 0.06f));
            AddFrameList(animPackage: PkgName.HerbsGreen, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_green", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBlack, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_black", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsBlue, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_blue", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsCyan, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_cyan", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsYellow, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_yellow", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsRed, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_red", layer: 0));
            AddFrameList(animPackage: PkgName.HerbsViolet, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "herbs_violet", layer: 0));
            AddFrameList(animPackage: PkgName.SpearStone, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "spear_stone", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearPoisoned, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "spear_poison", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.SpearIron, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "spear_iron", layer: 0, scale: 0.5f));
            AddFrameList(animPackage: PkgName.Fat, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "piece_of_fat", layer: 0, scale: 0.25f));


            // RPGMaker characters
            AddRPGMakerPackageV1(packageName: PkgName.Blonde, atlasName: "actor29rec4", setNoX: 0, setNoY: 0, animSize: 0);

            foreach (var kvp in new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } })
            {
                string atlasName = "rabbits";
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
                string atlasName = "fox";
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
                string atlasName = "tiger";
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
                AddRPGMakerPackageV2(packageName: PkgName.Frog1, atlasName: "frogs_small", setNoX: 0, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog2, atlasName: "frogs_small", setNoX: 1, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog3, atlasName: "frogs_small", setNoX: 2, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog4, atlasName: "frogs_small", setNoX: 3, setNoY: 0, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog5, atlasName: "frogs_small", setNoX: 0, setNoY: 1, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog6, atlasName: "frogs_small", setNoX: 1, setNoY: 1, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog7, atlasName: "frogs_small", setNoX: 2, setNoY: 1, animSize: 0, scale: scale);
                AddRPGMakerPackageV2(packageName: PkgName.Frog8, atlasName: "frogs_small", setNoX: 3, setNoY: 1, animSize: 0, scale: scale);
            }

            foreach (var kvp in new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } })
            {
                string atlasName = "frogs_big";
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

            {
                AddRPGMakerPackageV2(packageName: PkgName.CrabRed, atlasName: "crabs_small", setNoX: 0, setNoY: 0, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabRed, atlasName: "crabs_big", setNoX: 0, setNoY: 0, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabBrown, atlasName: "crabs_small", setNoX: 1, setNoY: 0, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabBrown, atlasName: "crabs_big", setNoX: 1, setNoY: 0, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabGreen, atlasName: "crabs_small", setNoX: 2, setNoY: 0, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabGreen, atlasName: "crabs_big", setNoX: 2, setNoY: 0, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabLightBlue, atlasName: "crabs_small", setNoX: 3, setNoY: 0, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabLightBlue, atlasName: "crabs_big", setNoX: 3, setNoY: 0, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabGray, atlasName: "crabs_small", setNoX: 0, setNoY: 1, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabGray, atlasName: "crabs_big", setNoX: 0, setNoY: 1, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabGray, atlasName: "crabs_small", setNoX: 1, setNoY: 1, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabYellow, atlasName: "crabs_big", setNoX: 1, setNoY: 1, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabYellow, atlasName: "crabs_small", setNoX: 2, setNoY: 1, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabBeige, atlasName: "crabs_big", setNoX: 2, setNoY: 1, animSize: 1);
                AddRPGMakerPackageV2(packageName: PkgName.CrabDarkBrown, atlasName: "crabs_small", setNoX: 3, setNoY: 1, animSize: 0, crop: true);
                AddRPGMakerPackageV2(packageName: PkgName.CrabDarkBrown, atlasName: "crabs_big", setNoX: 3, setNoY: 1, animSize: 1);
            }
        }
        public static List<AnimFrame> ConvertImageToFrameList(string atlasName, byte layer, ushort x = 0, ushort y = 0, ushort width = 0, ushort height = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f)
        {
            List<AnimFrame> frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: atlasName, layer: layer, x: x, y: y, width: width, height: height, crop: crop, duration: 0, scale: scale, depthPercent: depthPercent));
            return frameList;
        }
        public static AnimFrame ConvertImageToFrame(string atlasName, byte layer, ushort x = 0, ushort y = 0, ushort width = 0, ushort height = 0, byte duration = 0, bool crop = true, float scale = 1f, float depthPercent = 0.25f)
        {
            Texture2D atlasTexture = SonOfRobinGame.textureByName[atlasName];
            if (width == 0) width = (ushort)atlasTexture.Width;
            if (height == 0) height = (ushort)atlasTexture.Height;

            return new AnimFrame(atlasName: atlasName, atlasX: x, atlasY: y, width: width, height: height, layer: layer, duration: duration, crop: crop, scale: scale, depthPercent: depthPercent);
        }

        public static void AddRPGMakerPackageV1(PkgName packageName, string atlasName, byte setNoX, byte setNoY, byte animSize, bool crop = false, float scale = 1f)
        {
            ushort offsetX = (ushort)(setNoX * 96);
            ushort offsetY = (ushort)(setNoY * 128);
            ushort width = 32;
            ushort height = 32;

            List<AnimFrame> frameList = new List<AnimFrame>();

            var yByDirection = new Dictionary<string, int>(){
                { "right", height * 2},
                {"left", height },
                {"up", height * 3},
                {"down", 0 },
                };

            // standing
            foreach (var kvp in yByDirection)
            {
                frameList.Clear();
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16(width + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale));
                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }
            AddFrameList(animPackage: packageName, animSize: animSize, animName: "default", frameList: frameList);

            // walking
            foreach (var kvp in yByDirection)
            {
                frameList.Clear();
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16(width + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale));
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16((width * 2) + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale));
                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }
        }

        public static void AddRPGMakerPackageV2(PkgName packageName, string atlasName, byte setNoX, byte setNoY, byte animSize, bool crop = false, float scale = 1f)
        {
            ushort offsetX = (ushort)(setNoX * 144);
            ushort offsetY = (ushort)(setNoY * 192);
            ushort width = 48;
            ushort height = 48;

            List<AnimFrame> frameList = new List<AnimFrame>();

            var yByDirection = new Dictionary<string, int>(){
                { "right", height * 2},
                {"left", height },
                {"up", height * 3},
                {"down", 0 },
                };

            // standing
            foreach (var kvp in yByDirection)
            {
                frameList.Clear();
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16(width + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 0, crop: crop, scale: scale));
                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"stand-{kvp.Key}", frameList: frameList);
            }
            AddFrameList(animPackage: packageName, animSize: animSize, animName: "default", frameList: frameList);

            // walking
            foreach (var kvp in yByDirection)
            {
                frameList.Clear();
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16(0 + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale));
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16(width + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale));
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16((width * 2) + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale));
                frameList.Add(new AnimFrame(atlasName: atlasName, atlasX: Convert.ToUInt16(width + offsetX), atlasY: Convert.ToUInt16(kvp.Value + offsetY), width: width, height: height, layer: 1, duration: 8, crop: crop, scale: scale));
                AddFrameList(animPackage: packageName, animSize: animSize, animName: $"walk-{kvp.Key}", frameList: frameList);
            }
        }

    }
}
