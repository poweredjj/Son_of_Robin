using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public enum AnimPkg
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
        Shell1,
        Shell2,
        Shell3,
        Shell4,
        Shell5,
        Shell6,
        Shell7,
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

        ChestWooden,
        ChestMetal,
        WoodLog,
        WoodPlank,
        Crate,

        WoodenTable,
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

        RawMeat,
        CookedMeat,
        MealStandard,
        Leather,

        TomatoPlant,

        Hand,
        AxeWood,
        AxeStone,
        AxeIron,
        AxeDiamond,
        BatWood,

        Sling,
        GreatSling,
        BowWood,

        ArrowWood,
        ArrowIron,
        StoneSmall,

        PickaxeWood,
        PickaxeStone,
        PickaxeIron,
        PickaxeDiamond,

        CoalDeposit,
        IronDeposit,

        Coal,
        IronOre,
        IronBar,

        Blonde,
        CrabBeige,
        CrabBrown,
        CrabDarkBrown,
        CrabGray,
        CrabGreen,
        CrabLightBlue,
        CrabRed,
        CrabYellow,
        DemonMaidPink,
        DemonMaidYellow,
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
        Sailor,

        DebrisStone,
        DebrisWood,
        BloodDrop1,
        BloodDrop2,
        BloodDrop3,

        TentSmall,
        TentMedium,
        TentBig,

        BackpackMedium,
        BeltMedium,

        Map
    }

    public class AnimData
    {
        public static Dictionary<string, List<AnimFrame>> frameListById = new Dictionary<string, List<AnimFrame>>();
        public static void AddFrameList(AnimPkg animPackage, byte animSize, string animName, List<AnimFrame> frameList)
        {
            string completeAnimID = $"{animPackage}-{animSize}-{animName}";
            frameListById[completeAnimID] = new List<AnimFrame>(frameList);
        }

        public static void CreateAllAnims()

        {
            AnimPkg packageName;
            List<AnimFrame> frameList;
            float scale;

            packageName = AnimPkg.NoAnim;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "no_anim", layer: 1, x: 0, y: 0, width: 0, height: 0));

            packageName = AnimPkg.GrassRegular;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
                frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 320, width: 32, height: 32));

            packageName = AnimPkg.GrassDesert;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 160, y: 224, width: 24, height: 24));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "flowers", layer: 0, x: 288, y: 160, width: 32, height: 32));

            packageName = AnimPkg.FlowersYellow1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 352, width: 32, height: 32));

            packageName = AnimPkg.FlowersYellow2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 832, y: 128, width: 64, height: 64, scale: 0.5f));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 768, y: 0, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.FlowersWhite;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 32, y: 352, width: 32, height: 32));

            packageName = AnimPkg.Rushes;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 0, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 96, y: 192, width: 32, height: 32));

            packageName = AnimPkg.Cactus;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 352, y: 320, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "qYFvsmq", layer: 1, x: 288, y: 320, width: 32, height: 64));

            packageName = AnimPkg.PalmTree;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "palmtree_small", layer: 1));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 123, y: 326, width: 69, height: 80, crop: false));
            AddFrameList(animPackage: packageName, animSize: 2, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 8, y: 145, width: 66, height: 102, crop: false));
            AddFrameList(animPackage: packageName, animSize: 3, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "NicePng_pine-tree-clipart-png_1446450", layer: 1, x: 108, y: 145, width: 72, height: 102, crop: false));

            packageName = AnimPkg.TreeSmall1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 192, y: 352, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "dead",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 448, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 160, y: 320, width: 32, height: 64));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "dead",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 0, y: 416, width: 32, height: 64));

            packageName = AnimPkg.TreeSmall2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 128, y: 192, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "dead",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 32, y: 448, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 160, y: 192, width: 32, height: 64));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "dead",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 0, y: 416, width: 32, height: 64));

            packageName = AnimPkg.TreeBig;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 199, y: 382, width: 47, height: 66));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "dead",
            frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 0, y: 224, width: 64, height: 64));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 0, y: 0, width: 63, height: 97));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "dead",
            frameList: ConvertImageToFrameList(atlasName: "tileb", layer: 1, x: 0, y: 224, width: 64, height: 64));

            packageName = AnimPkg.WaterLily1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 384, y: 64, width: 32, height: 32));

            packageName = AnimPkg.WaterLily2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 416, y: 0, width: 32, height: 32));

            packageName = AnimPkg.WaterLily3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 448, y: 0, width: 32, height: 32));

            packageName = AnimPkg.TomatoPlant;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "celianna_farmnature_crops_transparent", layer: 1, x: 297, y: 8, width: 14, height: 13));
            AddFrameList(animPackage: packageName, animSize: 1, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "celianna_farmnature_crops_transparent", layer: 1, x: 328, y: 4, width: 17, height: 24));

            packageName = AnimPkg.MineralsSmall1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 192, y: 256, width: 27, height: 32));

            packageName = AnimPkg.MineralsSmall2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 224, y: 256, width: 30, height: 32));

            packageName = AnimPkg.MineralsSmall3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 1, x: 224, y: 320, width: 32, height: 32));

            packageName = AnimPkg.MineralsSmall4;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 896, y: 192, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.MineralsBig1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 128, y: 320, width: 64, height: 51));

            packageName = AnimPkg.MineralsBig2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 192, y: 320, width: 32, height: 64));

            packageName = AnimPkg.MineralsBig3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 224, y: 320, width: 32, height: 64));

            packageName = AnimPkg.MineralsBig4;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 1, x: 896, y: 256, width: 128, height: 64, scale: 0.5f));

            packageName = AnimPkg.Shell1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 256, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.Shell2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 192, y: 256, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.Shell3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 320, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.Shell4;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 192, y: 320, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.Shell5;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tilees by guth_zpsfn3wpjdu_2x", layer: 0, x: 128, y: 384, width: 64, height: 64, scale: 0.5f));

            packageName = AnimPkg.Shell6;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 594, y: 975, width: 28, height: 37, scale: 0.5f));

            packageName = AnimPkg.Shell7;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "8f296dbbaf43865bc29e99660fe7b5af_2x", layer: 0, x: 596, y: 848, width: 33, height: 33, scale: 0.5f));

            packageName = AnimPkg.WaterRock1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 256, y: 0, width: 32, height: 28));

            packageName = AnimPkg.WaterRock2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 292, y: 2, width: 21, height: 21));

            packageName = AnimPkg.WaterRock3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 322, y: 0, width: 27, height: 26));

            packageName = AnimPkg.WaterRock4;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 1, x: 361, y: 0, width: 43, height: 30));

            packageName = AnimPkg.WaterRock5;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 256, y: 64, width: 32, height: 32));

            packageName = AnimPkg.WaterRock6;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 324, y: 38, width: 27, height: 49));

            packageName = AnimPkg.WaterRock7;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_19ba32a6", layer: 0, x: 288, y: 32, width: 32, height: 32));

            packageName = AnimPkg.BloodSplatter1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 416, y: 320, width: 32, height: 32));

            packageName = AnimPkg.BloodSplatter2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 320, width: 32, height: 32));

            packageName = AnimPkg.BloodSplatter3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 448, y: 288, width: 32, height: 32));

            packageName = AnimPkg.BloodDrop1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 451, y: 290, width: 7, height: 8));

            packageName = AnimPkg.BloodDrop2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 456, y: 326, width: 8, height: 5));

            packageName = AnimPkg.BloodDrop3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tile_custom01", layer: 0, x: 467, y: 336, width: 7, height: 6));

            packageName = AnimPkg.ChestWooden;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "open",
            frameList: ConvertImageToFrameList(atlasName: "chests", layer: 0, x: 0, y: 144, width: 32, height: 48));
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 48, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 96, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 144, width: 32, height: 48, duration: 0));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "opening", frameList: frameList);

            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 96, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 48, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 0, width: 32, height: 48, duration: 0));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);

            packageName = AnimPkg.ChestMetal;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "open",
            frameList: ConvertImageToFrameList(atlasName: "chests", layer: 0, x: 32, y: 144, width: 32, height: 48));
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 48, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 96, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 144, width: 32, height: 48, duration: 0));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "opening", frameList: frameList);
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 96, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 48, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 32, y: 0, width: 32, height: 48, duration: 0));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);

            packageName = AnimPkg.Crate;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "chests", layer: 0, x: 128, y: 0, width: 32, height: 48));

            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 96, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 48, width: 32, height: 48, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "chests", layer: 0, x: 0, y: 0, width: 32, height: 48, duration: 0));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "closing", frameList: frameList);

            packageName = AnimPkg.WoodenTable;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
            frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 480, y: 576, width: 32, height: 32));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
            frameList: ConvertImageToFrameList(atlasName: "d9ffec650d3104f5c4564c9055787530", layer: 1, x: 480, y: 576, width: 32, height: 32)); // the same as "off"

            packageName = AnimPkg.Furnace;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "off",
            frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 0, y: 144, width: 48, height: 48));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "on",
            frameList: ConvertImageToFrameList(atlasName: "mv_blacksmith_by_schwarzenacht_dapf6ek", layer: 1, x: 48, y: 144, width: 48, height: 48));

            packageName = AnimPkg.CookingPot;
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 192, y: 144, width: 32, height: 32, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 224, y: 144, width: 32, height: 32, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 256, y: 144, width: 32, height: 32, duration: 6));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "off", frameList: frameList);
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 0, y: 144, width: 32, height: 32, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 32, y: 144, width: 32, height: 32, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "Candacis_flames1", layer: 1, x: 64, y: 144, width: 32, height: 32, duration: 6));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "on", frameList: frameList);

            packageName = AnimPkg.WoodLog;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 98, y: 48, width: 45, height: 46, scale: 0.5f));

            packageName = AnimPkg.WoodPlank;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 32, y: 0, width: 32, height: 32, scale: 1f));

            packageName = AnimPkg.Stick1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 26, y: 73, width: 25, height: 21));

            packageName = AnimPkg.Stick2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 100, y: 73, width: 25, height: 22));

            packageName = AnimPkg.Stick3;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 23, y: 105, width: 25, height: 25));

            packageName = AnimPkg.Stick4;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks1", layer: 1, x: 100, y: 104, width: 25, height: 25));

            packageName = AnimPkg.Stick5;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 1, x: 22, y: 72, width: 25, height: 25));

            packageName = AnimPkg.Stick6;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 1, x: 53, y: 68, width: 25, height: 25));

            packageName = AnimPkg.Stick7;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 1, x: 100, y: 70, width: 25, height: 25));

            packageName = AnimPkg.Stone1;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "stones", layer: 1, x: 11, y: 92, width: 23, height: 14));

            packageName = AnimPkg.Stone2;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "stones", layer: 1, x: 64, y: 78, width: 16, height: 16));

            packageName = AnimPkg.Apple;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 160, y: 192, width: 32, height: 32));

            packageName = AnimPkg.Banana;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 192, y: 192, width: 32, height: 32));

            packageName = AnimPkg.Cherry;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 0, y: 192, width: 32, height: 32, scale: 0.75f));

            packageName = AnimPkg.Tomato;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "fancy_food", layer: 0, x: 232, y: 207, width: 18, height: 18, scale: 0.5f));

            packageName = AnimPkg.RawMeat;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "steak_t-bone", layer: 0, x: 3, y: 9, width: 27, height: 23, scale: 0.75f));

            packageName = AnimPkg.CookedMeat;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "Cooked Meat", layer: 0, scale: 0.17f));

            packageName = AnimPkg.MealStandard;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "fancy_food2", layer: 0, x: 288, y: 64, width: 32, height: 32, scale: 0.5f));

            packageName = AnimPkg.Hand;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: ConvertImageToFrameList(atlasName: "hand", layer: 1));

            packageName = AnimPkg.AxeWood;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "axe_wooden", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));

            packageName = AnimPkg.AxeStone;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 0, y: 0, width: 48, height: 48, scale: 0.5f));

            packageName = AnimPkg.AxeIron;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "big_icons_candacis", layer: 0, x: 48, y: 0, width: 48, height: 48, scale: 0.5f));

            packageName = AnimPkg.AxeDiamond;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "axe_diamond", layer: 0, x: 0, y: 0, width: 30, height: 32, scale: 0.7f));

            packageName = AnimPkg.PickaxeWood;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 0, y: 384, width: 48, height: 48, scale: 0.7f));

            packageName = AnimPkg.PickaxeStone;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 384, width: 48, height: 48, scale: 0.7f));

            packageName = AnimPkg.PickaxeIron;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 192, y: 384, width: 48, height: 48, scale: 0.7f));

            packageName = AnimPkg.PickaxeDiamond;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 528, y: 384, width: 48, height: 48, scale: 0.7f));

            packageName = AnimPkg.BatWood;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "weapons1", layer: 0, x: 494, y: 340, width: 43, height: 42, scale: 0.65f));

            packageName = AnimPkg.BowWood;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "bows", layer: 0, x: 522, y: 17, width: 22, height: 50, scale: 0.7f));

            packageName = AnimPkg.ArrowWood;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "arrow_wood", layer: 0, scale: 0.75f));

            packageName = AnimPkg.ArrowIron;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "arrow_iron", layer: 0, scale: 0.75f));

            packageName = AnimPkg.Sling;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sling", layer: 0, scale: 0.5f));

            packageName = AnimPkg.GreatSling;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "greatsling", layer: 0, scale: 0.5f));

            packageName = AnimPkg.StoneSmall;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "stone_ammo", layer: 0, scale: 0.5f));

            packageName = AnimPkg.DebrisStone;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "stone_ammo", layer: 0, scale: 0.25f));

            packageName = AnimPkg.DebrisWood;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "sticks2", layer: 0, x: 100, y: 70, width: 25, height: 25, scale: 0.3f));

            packageName = AnimPkg.IronDeposit;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 96, y: 96, width: 48, height: 48, scale: 1f));

            packageName = AnimPkg.CoalDeposit;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "gems__rpg_maker_mv__by_petschko-d9euoxr", layer: 1, x: 0, y: 96, width: 48, height: 48, scale: 1f));

            packageName = AnimPkg.IronOre;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 144, width: 48, height: 48, scale: 0.5f));

            packageName = AnimPkg.IronBar;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 48, y: 96, width: 48, height: 48, scale: 0.5f));

            packageName = AnimPkg.Coal;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tools_gravel", layer: 0, x: 288, y: 144, width: 48, height: 48, scale: 0.5f));

            packageName = AnimPkg.Backlight;
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_1", layer: 0, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_2", layer: 0, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_3", layer: 0, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_4", layer: 0, duration: 20));
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_3", layer: 0, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_2", layer: 0, duration: 6));
            frameList.Add(ConvertImageToFrame(atlasName: "backlight_1", layer: 0, duration: 6));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: frameList);

            packageName = AnimPkg.Attack;
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "attack_effect_sprite_sheets", layer: 2, duration: 6, x: 3524, y: 1520, width: 23, height: 23));
            frameList.Add(ConvertImageToFrame(atlasName: "attack_effect_sprite_sheets", layer: 2, duration: 6, x: 3546, y: 1513, width: 29, height: 33));
            frameList.Add(ConvertImageToFrame(atlasName: "attack_effect_sprite_sheets", layer: 2, duration: 6, x: 3574, y: 1500, width: 36, height: 65));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: frameList);

            packageName = AnimPkg.Miss;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "miss", layer: 2));

            packageName = AnimPkg.Zzz;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "zzz", layer: 2));

            packageName = AnimPkg.Heart;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "heart_16x16", layer: 2));

            packageName = AnimPkg.Crosshair;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "crosshair", layer: 2));

            packageName = AnimPkg.Flame;
            frameList = new List<AnimFrame>();
            frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 432, y: 48, width: 48, height: 48));
            frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 480, y: 48, width: 48, height: 48));
            frameList.Add(ConvertImageToFrame(atlasName: "flames", layer: 1, duration: 6, x: 528, y: 48, width: 48, height: 48));
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default", frameList: frameList);

            packageName = AnimPkg.Leather;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 96, y: 96, width: 32, height: 32, scale: 0.75f));

            packageName = AnimPkg.Nail;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "craft_items", layer: 0, x: 0, y: 0, width: 32, height: 32, scale: 0.5f));

            packageName = AnimPkg.TentSmall; // TODO replace with A-frame tent asset (when found)
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 0.5f, depthPercent: 0.45f));

            packageName = AnimPkg.TentMedium;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tent_medium", layer: 1, x: 0, y: 0, width: 117, height: 101, scale: 1f, depthPercent: 0.45f));

            packageName = AnimPkg.TentBig;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "tent_big", layer: 1, x: 15, y: 0, width: 191, height: 162, scale: 1f, depthPercent: 0.6f));

            packageName = AnimPkg.BackpackMedium;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "backpack", layer: 1, scale: 0.5f));

            packageName = AnimPkg.BeltMedium;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "belt", layer: 1, scale: 0.06f));

            packageName = AnimPkg.Map;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "parchment", layer: 0, scale: 0.03f));

            packageName = AnimPkg.Exclamation;
            AddFrameList(animPackage: packageName, animSize: 0, animName: "default",
            frameList: ConvertImageToFrameList(atlasName: "exclamation", layer: 2, scale: 0.2f));

            // RPGMaker characters
            AddRPGMakerPackageV1(packageName: AnimPkg.Blonde, atlasName: "actor29rec4", setNoX: 0, setNoY: 0, animSize: 0);
            AddRPGMakerPackageV1(packageName: AnimPkg.DemonMaidPink, atlasName: "demonmaid", setNoX: 0, setNoY: 0, animSize: 0);
            AddRPGMakerPackageV1(packageName: AnimPkg.DemonMaidYellow, atlasName: "demonmaid2", setNoX: 0, setNoY: 0, animSize: 0);
            AddRPGMakerPackageV1(packageName: AnimPkg.Sailor, atlasName: "recolor_pt2", setNoX: 0, setNoY: 0, animSize: 0);

            scale = 0.6f;
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBrown, atlasName: "rabbits", setNoX: 0, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitDarkBrown, atlasName: "rabbits", setNoX: 1, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitGray, atlasName: "rabbits", setNoX: 2, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBlack, atlasName: "rabbits", setNoX: 3, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitLightGray, atlasName: "rabbits", setNoX: 0, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBeige, atlasName: "rabbits", setNoX: 1, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitWhite, atlasName: "rabbits", setNoX: 2, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitLightBrown, atlasName: "rabbits", setNoX: 3, setNoY: 1, animSize: 0, scale: scale);

            scale = 0.8f;
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBrown, atlasName: "rabbits", setNoX: 0, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitDarkBrown, atlasName: "rabbits", setNoX: 1, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitGray, atlasName: "rabbits", setNoX: 2, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBlack, atlasName: "rabbits", setNoX: 3, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitLightGray, atlasName: "rabbits", setNoX: 0, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBeige, atlasName: "rabbits", setNoX: 1, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitWhite, atlasName: "rabbits", setNoX: 2, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitLightBrown, atlasName: "rabbits", setNoX: 3, setNoY: 1, animSize: 1, scale: scale);

            scale = 1f;
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBrown, atlasName: "rabbits", setNoX: 0, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitDarkBrown, atlasName: "rabbits", setNoX: 1, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitGray, atlasName: "rabbits", setNoX: 2, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBlack, atlasName: "rabbits", setNoX: 3, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitLightGray, atlasName: "rabbits", setNoX: 0, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitBeige, atlasName: "rabbits", setNoX: 1, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitWhite, atlasName: "rabbits", setNoX: 2, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.RabbitLightBrown, atlasName: "rabbits", setNoX: 3, setNoY: 1, animSize: 2, scale: scale);

            scale = 0.6f;
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxGinger, atlasName: "fox", setNoX: 0, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxRed, atlasName: "fox", setNoX: 1, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxWhite, atlasName: "fox", setNoX: 2, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxGray, atlasName: "fox", setNoX: 3, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxBlack, atlasName: "fox", setNoX: 0, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxChocolate, atlasName: "fox", setNoX: 1, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxBrown, atlasName: "fox", setNoX: 2, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxYellow, atlasName: "fox", setNoX: 3, setNoY: 1, animSize: 0, scale: scale);

            scale = 0.8f;
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxGinger, atlasName: "fox", setNoX: 0, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxRed, atlasName: "fox", setNoX: 1, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxWhite, atlasName: "fox", setNoX: 2, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxGray, atlasName: "fox", setNoX: 3, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxBlack, atlasName: "fox", setNoX: 0, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxChocolate, atlasName: "fox", setNoX: 1, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxBrown, atlasName: "fox", setNoX: 2, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxYellow, atlasName: "fox", setNoX: 3, setNoY: 1, animSize: 1, scale: scale);

            scale = 1f;
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxGinger, atlasName: "fox", setNoX: 0, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxRed, atlasName: "fox", setNoX: 1, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxWhite, atlasName: "fox", setNoX: 2, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxGray, atlasName: "fox", setNoX: 3, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxBlack, atlasName: "fox", setNoX: 0, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxChocolate, atlasName: "fox", setNoX: 1, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxBrown, atlasName: "fox", setNoX: 2, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.FoxYellow, atlasName: "fox", setNoX: 3, setNoY: 1, animSize: 2, scale: scale);

            scale = 1f;
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog1, atlasName: "frogs_small", setNoX: 0, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog2, atlasName: "frogs_small", setNoX: 1, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog3, atlasName: "frogs_small", setNoX: 2, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog4, atlasName: "frogs_small", setNoX: 3, setNoY: 0, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog5, atlasName: "frogs_small", setNoX: 0, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog6, atlasName: "frogs_small", setNoX: 1, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog7, atlasName: "frogs_small", setNoX: 2, setNoY: 1, animSize: 0, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog8, atlasName: "frogs_small", setNoX: 3, setNoY: 1, animSize: 0, scale: scale);

            scale = 0.4f;
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog1, atlasName: "frogs_big", setNoX: 0, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog2, atlasName: "frogs_big", setNoX: 1, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog3, atlasName: "frogs_big", setNoX: 2, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog4, atlasName: "frogs_big", setNoX: 3, setNoY: 0, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog5, atlasName: "frogs_big", setNoX: 0, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog6, atlasName: "frogs_big", setNoX: 1, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog7, atlasName: "frogs_big", setNoX: 2, setNoY: 1, animSize: 1, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog8, atlasName: "frogs_big", setNoX: 3, setNoY: 1, animSize: 1, scale: scale);

            scale = 0.75f;
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog1, atlasName: "frogs_big", setNoX: 0, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog2, atlasName: "frogs_big", setNoX: 1, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog3, atlasName: "frogs_big", setNoX: 2, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog4, atlasName: "frogs_big", setNoX: 3, setNoY: 0, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog5, atlasName: "frogs_big", setNoX: 0, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog6, atlasName: "frogs_big", setNoX: 1, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog7, atlasName: "frogs_big", setNoX: 2, setNoY: 1, animSize: 2, scale: scale);
            AddRPGMakerPackageV2(packageName: AnimPkg.Frog8, atlasName: "frogs_big", setNoX: 3, setNoY: 1, animSize: 2, scale: scale);

            AddRPGMakerPackageV2(packageName: AnimPkg.CrabRed, atlasName: "crabs_small", setNoX: 0, setNoY: 0, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabRed, atlasName: "crabs_big", setNoX: 0, setNoY: 0, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabBrown, atlasName: "crabs_small", setNoX: 1, setNoY: 0, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabBrown, atlasName: "crabs_big", setNoX: 1, setNoY: 0, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabGreen, atlasName: "crabs_small", setNoX: 2, setNoY: 0, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabGreen, atlasName: "crabs_big", setNoX: 2, setNoY: 0, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabLightBlue, atlasName: "crabs_small", setNoX: 3, setNoY: 0, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabLightBlue, atlasName: "crabs_big", setNoX: 3, setNoY: 0, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabGray, atlasName: "crabs_small", setNoX: 0, setNoY: 1, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabGray, atlasName: "crabs_big", setNoX: 0, setNoY: 1, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabGray, atlasName: "crabs_small", setNoX: 1, setNoY: 1, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabYellow, atlasName: "crabs_big", setNoX: 1, setNoY: 1, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabYellow, atlasName: "crabs_small", setNoX: 2, setNoY: 1, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabBeige, atlasName: "crabs_big", setNoX: 2, setNoY: 1, animSize: 1);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabDarkBrown, atlasName: "crabs_small", setNoX: 3, setNoY: 1, animSize: 0, crop: true);
            AddRPGMakerPackageV2(packageName: AnimPkg.CrabDarkBrown, atlasName: "crabs_big", setNoX: 3, setNoY: 1, animSize: 1);
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

        public static void AddRPGMakerPackageV1(AnimPkg packageName, string atlasName, byte setNoX, byte setNoY, byte animSize, bool crop = false, float scale = 1f)
        {
            ushort offsetX = Convert.ToUInt16(setNoX * 96);
            ushort offsetY = Convert.ToUInt16(setNoY * 128);
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

        public static void AddRPGMakerPackageV2(AnimPkg packageName, string atlasName, byte setNoX, byte setNoY, byte animSize, bool crop = false, float scale = 1f)
        {
            ushort offsetX = Convert.ToUInt16(setNoX * 144);
            ushort offsetY = Convert.ToUInt16(setNoY * 192);
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
