using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class AnimDataNew
    {
        public static readonly AnimData.PkgName[] allPkgNames = (AnimData.PkgName[])Enum.GetValues(typeof(AnimData.PkgName));
        public static readonly Dictionary<AnimData.PkgName, AnimPkg> pkgByName = new();
        public static readonly Dictionary<string, AnimFrame> frameById = new(); // needed to access frames directly by id (for loading and saving game)

        public static void LoadPackage(AnimData.PkgName pkgName)
        {
            if (pkgByName.ContainsKey(pkgName))
            {
                MessageLog.Add(debugMessage: true, text: $"Package '{pkgName}' already loaded, ignoring.");
                return;
            }

            AnimPkg animPkg;

            switch (pkgName)
            {
                case AnimData.PkgName.Empty:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 2, animSize: 0, altasName: "_processed_transparent_pixel", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.NoAnim:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 63, height: 16, scale: 1f, layer: 2, animSize: 0, altasName: "_processed_no_anim", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.WhiteSpotLayerMinus1:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: -1, animSize: 0, altasName: "_processed_white_spot", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.WhiteSpotLayerZero:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_white_spot", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.WhiteSpotLayerOne:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 1, animSize: 0, altasName: "_processed_white_spot", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.WhiteSpotLayerTwo:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 2, animSize: 0, altasName: "_processed_white_spot", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.WhiteSpotLayerThree:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 1, height: 1, scale: 1f, layer: 3, animSize: 0, altasName: "_processed_white_spot", hasOnePixelMargin: false);
                    break;

                case AnimData.PkgName.FlowersWhite:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_flowers_white", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 23, height: 19), gfxOffsetCorrection: new Vector2(0, -1))]));

                        break;
                    }

                case AnimData.PkgName.FlowersYellow1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_flowers_yellow_1_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 17), gfxOffsetCorrection: new Vector2(0, 1))]));
                        break;
                    }

                case AnimData.PkgName.FlowersYellow2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_flowers_yellow_2_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 46, height: 41), scale: 0.5f)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_flowers_yellow_2_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 38, height: 49), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -7))]));
                        break;
                    }

                case AnimData.PkgName.FlowersRed:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_flowers_red", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 25), gfxOffsetCorrection: new Vector2(0, -2))]));
                        break;
                    }

                case AnimData.PkgName.Rushes:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_rushes", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 34), gfxOffsetCorrection: new Vector2(0, -8))]));
                        break;
                    }

                case AnimData.PkgName.GrassDesert:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 12);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_desert_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 23, height: 21))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_desert_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 19))]));
                        break;
                    }

                case AnimData.PkgName.GrassRegular:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s0", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 19, height: 15))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 20), gfxOffsetCorrection: new Vector2(0, -2))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_grass_s1", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 20), scale: 1.2f, gfxOffsetCorrection: new Vector2(0, -3))]));

                        break;
                    }

                case AnimData.PkgName.PlantPoison:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 11);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_plant_poison", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 33), scale: 0.4f)]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_plant_poison", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 33), scale: 0.6f, gfxOffsetCorrection: new Vector2(0, -1))]));
                        break;
                    }

                case AnimData.PkgName.CoffeeShrub:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 15);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_coffee_shrub", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 486, height: 577), scale: 0.05f, gfxOffsetCorrection: new Vector2(0, -136))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_coffee_shrub", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 486, height: 577), scale: 0.06f, gfxOffsetCorrection: new Vector2(0, -160))]));
                        break;
                    }

                case AnimData.PkgName.CarrotPlant:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 26, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_carrot_plant_empty", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 298, height: 283), scale: 0.1f, gfxOffsetCorrection: new Vector2(0, -37))], name: "default"));

                        // using different plant graphics when carrot is present, instead of drawing the carrot separately (because the carrot should be underground)
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_carrot_plant_has_carrot", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 295, height: 351), scale: 0.1f, gfxOffsetCorrection: new Vector2(0, -70))], name: "has_fruits"));
                        break;
                    }

                case AnimData.PkgName.TomatoPlant:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 28, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_tomato_plant_small", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 296, height: 288), scale: 0.1f, gfxOffsetCorrection: new Vector2(0, -13))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_tomato_plant_medium", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 477, height: 459), scale: 0.08f, gfxOffsetCorrection: new Vector2(0, -64))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_tomato_plant_big", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 494, height: 980), scale: 0.08f, gfxOffsetCorrection: new Vector2(0, -332))]));
                        break;
                    }

                case AnimData.PkgName.MushroomPlant:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 18, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mushroom_plant", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 29), scale: 0.6f, gfxOffsetCorrection: new Vector2(1, -2))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_mushroom_plant", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 29), scale: 0.8f, gfxOffsetCorrection: new Vector2(1, -5))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_mushroom_plant", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 29), scale: 1.0f, gfxOffsetCorrection: new Vector2(1, -7))]));
                        break;
                    }

                case AnimData.PkgName.Cactus:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 18);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_cactus_s0", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 26, height: 30), gfxOffsetCorrection: new Vector2(0, -5))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_cactus_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 53), gfxOffsetCorrection: new Vector2(0, -13))]));
                        break;
                    }

                case AnimData.PkgName.PalmTree:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_palmtree_small", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 39), scale: 0.7f, gfxOffsetCorrection: new Vector2(0, -8))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_palmtree_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 71, height: 82), gfxOffsetCorrection: new Vector2(1, -20))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_palmtree_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 68, height: 104), gfxOffsetCorrection: new Vector2(15, -36))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 3, frameArray: [new AnimFrameNew(atlasName: "_processed_palmtree_s3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 74, height: 104), gfxOffsetCorrection: new Vector2(14, -37))]));
                        break;
                    }

                case AnimData.PkgName.TreeBig:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 14, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_sapling_tall", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 99), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -32))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_big_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 49, height: 64), gfxOffsetCorrection: new Vector2(0, -22))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_big_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 65, height: 96), gfxOffsetCorrection: new Vector2(0, -38))]));
                        break;
                    }

                case AnimData.PkgName.TreeSmall1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 15);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_sapling_short", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 44, height: 55), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -10))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_small_1_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 33, height: 34), gfxOffsetCorrection: new Vector2(0, -6))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_small_1_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 66), gfxOffsetCorrection: new Vector2(0, -22))]));
                        break;
                    }

                case AnimData.PkgName.TreeSmall2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 15);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_sapling_short", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 44, height: 55), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -10))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 1, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_small_2_s1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 32), gfxOffsetCorrection: new Vector2(0, -6))]));
                        animPkg.AddAnim(new(animPkg: animPkg, size: 2, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_small_2_s2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 66), gfxOffsetCorrection: new Vector2(0, -22))]));
                        break;
                    }

                case AnimData.PkgName.TreeStump:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 16, colHeight: 17);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_tree_stump", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 24, height: 20), scale: 1f)]));
                        break;
                    }

                case AnimData.PkgName.WaterLily1:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 34, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_waterlily1", hasOnePixelMargin: true);
                    break;

                case AnimData.PkgName.WaterLily2:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 32, height: 32, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_waterlily2", hasOnePixelMargin: true);
                    break;

                case AnimData.PkgName.WaterLily3:
                    animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 32, height: 34, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_waterlily3", hasOnePixelMargin: true);
                    break;

                case AnimData.PkgName.MineralsBig1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 37);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 222, height: 310), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -80))]));
                        break;
                    }

                // AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_1", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                // break;

                //case AnimData.PkgName.MineralsBig2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_2", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_3", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig4:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_4", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig5:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_5", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig6:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_6", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig7:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_7", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig8:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_8", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig9:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_9", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig10:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_10", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig11:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_11", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig12:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_12", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig13:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_13", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsBig14:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_big_14", layer: 1, scale: 0.3f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsSmall1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_small_1", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                //    break;

                //case AnimData.PkgName.MineralsSmall2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_small_2", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                //    break;

                //case AnimData.PkgName.MineralsSmall3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_small_3", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                //    break;

                //case AnimData.PkgName.MineralsSmall4:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_minerals_small_4", layer: 1, scale: 0.3f, depthPercent: 0.35f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_1", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_2", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_3", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig4:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_4", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig5:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_5", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig6:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_6", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig7:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_7", layer: 1, scale: 0.28f, depthPercent: 0.4f));

                //    break;

                //case AnimData.PkgName.MineralsMossyBig8:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_8", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig9:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_9", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig10:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_10", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig11:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_11", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossyBig12:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_big_12", layer: 1, scale: 0.28f, depthPercent: 0.4f));
                //    break;

                //case AnimData.PkgName.MineralsMossySmall1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_small_1", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                //    break;

                //case AnimData.PkgName.MineralsMossySmall2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_small_2", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                //    break;

                //case AnimData.PkgName.MineralsMossySmall3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_small_3", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                //    break;

                //case AnimData.PkgName.MineralsMossySmall4:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mossy_minerals_small_4", layer: 1, scale: 0.2f, depthPercent: 0.45f));
                //    break;

                //case AnimData.PkgName.MineralsCave:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_cave_minerals", layer: 1, scale: 0.3f, depthPercent: 0.75f));
                //    break;

                //case AnimData.PkgName.JarWhole:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_jar_sealed", layer: 1, scale: 0.6f));
                //    break;

                //case AnimData.PkgName.JarBroken:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_jar_broken", layer: 1, scale: 0.6f));
                //    break;

                //case AnimData.PkgName.ChestWooden:
                //    AddChestPackage(pkgName);
                //    break;

                //case AnimData.PkgName.ChestStone:
                //    AddChestPackage(pkgName);
                //    break;

                //case AnimData.PkgName.ChestIron:
                //    AddChestPackage(pkgName);
                //    break;

                //case AnimData.PkgName.ChestCrystal:
                //    AddChestPackage(pkgName);
                //    break;

                //case AnimData.PkgName.ChestTreasureBlue:
                //    AddChestPackage(pkgName);
                //    break;

                //case AnimData.PkgName.ChestTreasureRed:
                //    AddChestPackage(pkgName);
                //    break;

                //case AnimData.PkgName.WoodLogRegular:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_wood_regular", layer: 1, scale: 0.75f));
                //    break;

                //case AnimData.PkgName.WoodLogHard:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_wood_hard", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.WoodPlank:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_wood_plank", layer: 0, scale: 0.8f));
                //    break;

                //case AnimData.PkgName.Nail:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_nail", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.Rope:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_rope", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.HideCloth:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_hidecloth", layer: 0, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.Crate:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_crate", layer: 1));
                //    break;

                //case AnimData.PkgName.WorkshopEssential:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_essential", layer: layer, scale: 0.5f));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_essential", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.WorkshopBasic:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_basic", layer: layer, scale: 0.5f));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_basic", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.WorkshopAdvanced:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_advanced", layer: layer, scale: 0.5f));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_advanced", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.WorkshopMaster:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_master", layer: layer, scale: 0.5f));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_master", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.WorkshopMeatHarvesting:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_meat_harvesting_off", layer: layer, scale: 0.5f));
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_meat_harvesting_on", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.WorkshopLeatherBasic:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_leather_basic", layer: layer, scale: 0.5f));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_leather_basic", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.WorkshopLeatherAdvanced:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_leather_advanced", layer: layer, scale: 0.5f));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_workshop_leather_advanced", layer: layer, scale: 0.5f));
                //        break;
                //    }

                //case AnimData.PkgName.MeatDryingRackRegular:
                //    {
                //        float depthPercent = 0.6f;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_meat_drying_rack_regular_off", layer: 1, depthPercent: depthPercent));

                //        for (int i = 1; i <= 4; i++)
                //        {
                //            AddFrameArray(pkgName: pkgName, animName: $"on_{i}", frameArray: ConvertImageToFrameArray(atlasName: $"_processed_meat_drying_rack_regular_on_{i}", layer: 1, depthPercent: depthPercent));
                //        }
                //        break;
                //    }

                //case AnimData.PkgName.MeatDryingRackWide:
                //    {
                //        float depthPercent = 0.6f;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_meat_drying_rack_wide_off", layer: 1, depthPercent: depthPercent));

                //        for (int i = 1; i <= 6; i++)
                //        {
                //            AddFrameArray(pkgName: pkgName, animName: $"on_{i}", frameArray: ConvertImageToFrameArray(atlasName: $"_processed_meat_drying_rack_wide_on_{i}", layer: 1, depthPercent: depthPercent));
                //        }
                //        break;
                //    }

                //case AnimData.PkgName.AlchemyLabStandard:
                //    {
                //        int layer = 1;
                //        float scale = 0.5f;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_alchemy_lab_standard", layer: layer, scale: scale));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_alchemy_lab_standard", layer: layer, scale: scale));
                //        break;
                //    }

                //case AnimData.PkgName.AlchemyLabAdvanced:
                //    {
                //        int layer = 1;
                //        float scale = 0.5f;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_alchemy_lab_advanced", layer: layer, scale: scale));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_alchemy_lab_advanced", layer: layer, scale: scale));
                //        break;
                //    }

                //case AnimData.PkgName.FurnaceConstructionSite:
                //    {
                //        float scale = 0.2f;

                //        for (int animSize = 0; animSize <= 2; animSize++)
                //        {
                //            AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: $"furnace/_processed_furnace_construction_{animSize}", layer: 1, scale: scale, crop: false), animSize: animSize, updateCroppedFramesForPkgs: animSize == 0); // animSize == 0 should serve as an example (whole blueprint visible)
                //        }

                //        break;
                //    }

                //case AnimData.PkgName.FurnaceComplete:
                //    {
                //        int layer = 1;
                //        float scale = 0.2f;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "furnace/_processed_furnace_off", layer: layer, scale: scale, crop: false));
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "furnace/_processed_furnace_on", layer: layer, scale: scale, crop: false));
                //        break;
                //    }

                //case AnimData.PkgName.Anvil:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_anvil", layer: layer));
                //        // the same as "off"
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_anvil", layer: layer));
                //        break;
                //    }

                //case AnimData.PkgName.HotPlate:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_hot_plate_off", layer: layer));
                //        var frameArray = new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_hot_plate_on_1", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_hot_plate_on_2", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_hot_plate_on_3", layer: layer, duration: 6)
                //        };
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArray);
                //        break;
                //    }

                //case AnimData.PkgName.CookingPot:
                //    {
                //        int layer = 1;

                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_cooking_pot_off", layer: layer));

                //        var frameArrayOn = new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_cooking_pot_on_1", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_cooking_pot_on_2", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_cooking_pot_on_3", layer: layer, duration: 6)
                //        };
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArrayOn);

                //        break;
                //    }

                //case AnimData.PkgName.Totem:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_totem", layer: 1, scale: 0.25f, depthPercent: 0.15f));
                //    break;

                //case AnimData.PkgName.RuinsWallHorizontal1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_ruins_wall_horizontal_1", layer: 1));
                //    break;

                //case AnimData.PkgName.RuinsWallHorizontal2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_ruins_wall_horizontal_2", layer: 1));
                //    break;

                //case AnimData.PkgName.RuinsWallWallVertical:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_ruins_wall_vertical", layer: 1, depthPercent: 0.75f));
                //    break;

                //case AnimData.PkgName.RuinsColumn:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_ruins_column", layer: 1));
                //    break;

                //case AnimData.PkgName.RuinsRubble:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_ruins_rubble", layer: 1));
                //    break;

                //case AnimData.PkgName.Stick:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_stick", layer: 0));
                //    break;

                //case AnimData.PkgName.Stone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_stone", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.Granite:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_granite", layer: 1, scale: 1f));
                //    break;

                //case AnimData.PkgName.Clay:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_clay", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.Apple:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_apple", layer: 0, scale: 0.075f));
                //    break;

                //case AnimData.PkgName.Banana:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_banana", layer: 0, scale: 0.08f));
                //    break;

                //case AnimData.PkgName.Cherry:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_cherry", layer: 0, scale: 0.12f));
                //    break;

                //case AnimData.PkgName.Tomato:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_tomato", layer: 0, scale: 0.07f));
                //    break;

                //case AnimData.PkgName.Carrot:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_carrot", layer: 0, scale: 0.08f));
                //    break;

                //case AnimData.PkgName.Acorn:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_acorn", layer: 0, scale: 0.13f));
                //    break;

                //case AnimData.PkgName.Mushroom:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_mushroom", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.SeedBag:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_seed_bag", layer: 0, scale: 0.08f));
                //    break;

                //case AnimData.PkgName.CoffeeRaw:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_coffee_raw", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.CoffeeRoasted:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_coffee_roasted", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.Clam:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_clam", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.MeatRawRegular:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_meat_raw_regular", layer: 0, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.MeatRawPrime:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_meat_raw_prime", layer: 0, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.MeatDried:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_meat_dried", layer: 0, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.Fat:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fat", layer: 0, scale: 0.08f));
                //    break;

                //case AnimData.PkgName.Burger:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_burger", layer: 0, scale: 0.07f));
                //    break;

                //case AnimData.PkgName.MealStandard:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_meal_standard", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.Leather:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_leather", layer: 0, scale: 0.75f));
                //    break;

                //case AnimData.PkgName.KnifeSimple:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_knife_simple", layer: 1, scale: 1f));
                //    break;

                //case AnimData.PkgName.AxeWood:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_axe_wooden", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.AxeStone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_axe_stone", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.AxeIron:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_axe_iron", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.AxeCrystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_axe_crystal", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PickaxeWood:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_pickaxe_wood", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.PickaxeStone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_pickaxe_stone", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.PickaxeIron:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_pickaxe_iron", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.PickaxeCrystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_pickaxe_crystal", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.ScytheStone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_scythe_stone", layer: 0));
                //    break;

                //case AnimData.PkgName.ScytheIron:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_scythe_iron", layer: 0));
                //    break;

                //case AnimData.PkgName.ScytheCrystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_scythe_crystal", layer: 0));
                //    break;

                //case AnimData.PkgName.SpearWood:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_spear_wood", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.SpearStone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_spear_stone", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.SpearIron:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_spear_iron", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.SpearCrystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_spear_crystal", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.ShovelStone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_shovel_stone", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.ShovelIron:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_shovel_iron", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.ShovelCrystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_shovel_crystal", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.BowBasic:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bow_basic", layer: 0, scale: 0.25f));
                //    break;

                //case AnimData.PkgName.BowAdvanced:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bow_advanced", layer: 0, scale: 0.25f));
                //    break;

                //case AnimData.PkgName.ArrowWood:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_arrow_wood", layer: 0, scale: 0.75f));
                //    break;

                //case AnimData.PkgName.ArrowStone:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_arrow_stone", layer: 0, scale: 0.75f));
                //    break;

                //case AnimData.PkgName.ArrowIron:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_arrow_iron", layer: 0, scale: 0.75f));
                //    break;

                //case AnimData.PkgName.ArrowCrystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_arrow_crystal", layer: 0, scale: 0.75f));
                //    break;

                //case AnimData.PkgName.ArrowExploding:
                //    {
                //        int layer = 0;
                //        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "_processed_arrow_burning_off", layer: layer, scale: 0.75f));
                //        AddFrameArray(pkgName: pkgName, animName: "burning", frameArray: ConvertImageToFrameArray(atlasName: "_processed_arrow_burning_on", layer: layer, scale: 0.75f));
                //        break;
                //    }

                //case AnimData.PkgName.CoalDeposit:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_coal_deposit", layer: 1, scale: 1f));
                //    break;

                //case AnimData.PkgName.IronDeposit:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_iron_deposit", layer: 1, scale: 1f));
                //    break;

                //case AnimData.PkgName.CrystalDepositSmall:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_crystal_deposit_small", layer: 1));
                //    break;

                //case AnimData.PkgName.CrystalDepositBig:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_crystal_deposit_big", layer: 1));
                //    break;

                //case AnimData.PkgName.DigSite:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_dig_site", layer: 0));
                //    break;

                //case AnimData.PkgName.DigSiteGlass:
                //    {
                //        int layer = 0;
                //        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_dig_site_glass", layer: layer, duration: 450),
                //            ConvertImageToFrame(atlasName: "_processed_dig_site_glass_shine_1", layer: layer, duration: 1),
                //            ConvertImageToFrame(atlasName: "_processed_dig_site_glass_shine_2", layer: layer, duration: 1),
                //            ConvertImageToFrame(atlasName: "_processed_dig_site_glass_shine_3", layer: layer, duration: 1),
                //            ConvertImageToFrame(atlasName: "_processed_dig_site_glass_shine_2", layer: layer, duration: 1),
                //            ConvertImageToFrame(atlasName: "_processed_dig_site_glass_shine_1", layer: layer, duration: 1),
                //        });
                //        break;
                //    }

                //case AnimData.PkgName.DigSiteRuins:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_dig_site_ruins", layer: 0, scale: 0.35f));
                //    break;

                //case AnimData.PkgName.Coal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_coal", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.IronOre:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_iron_ore", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.IronBar:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_iron_bar", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.IronRod:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_iron_rod", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.IronPlate:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_iron_plate", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.GlassSand:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_glass_sand", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.Crystal:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_crystal", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PlayerBoy:
                //    AddRPGMakerPackageV1(pkgName: pkgName, atlasName: "characters/actor29rec4", setNoX: 0, setNoY: 0, animSize: 0);
                //    break;

                //case AnimData.PkgName.PlayerGirl:
                //    AddRPGMakerPackageV1(pkgName: pkgName, atlasName: "characters/recolor_pt2", setNoX: 0, setNoY: 0, animSize: 0);
                //    break;

                //case AnimData.PkgName.FoxGinger:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxRed:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxWhite:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxGray:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxBlack:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxChocolate:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxBrown:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.FoxYellow:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/fox", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog1:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 0, setNoY: 0, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog2:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 1, setNoY: 0, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog3:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 2, setNoY: 0, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog4:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 3, setNoY: 0, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog5:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 0, setNoY: 1, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog6:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 1, setNoY: 1, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog7:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 2, setNoY: 1, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.Frog8:
                //    AddRPGMakerPackageV2(pkgName: pkgName, atlasName: "characters/frogs_small", setNoX: 3, setNoY: 1, animSize: 0, scale: 1f);
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/frogs_big", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 1, 0.6f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitBrown:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitDarkBrown:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitGray:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitBlack:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitLightGray:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitBeige:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitWhite:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.RabbitLightBrown:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/rabbits", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 0.8f }, { 2, 1.0f } });
                //    break;

                //case AnimData.PkgName.BearBrown:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 0, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearWhite:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 1, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearOrange:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 2, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearBlack:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 3, setNoY: 0, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearDarkBrown:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 0, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearGray:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 1, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearRed:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 2, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.BearBeige:
                //    AddRPGMakerPackageV2ForSizeDict(pkgName: pkgName, atlasName: "characters/bear", setNoX: 3, setNoY: 1, scaleForSizeDict: new Dictionary<byte, float> { { 0, 0.6f }, { 1, 1.0f }, { 2, 1.3f } });
                //    break;

                //case AnimData.PkgName.TentModern:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_tent_modern", layer: 1, scale: 0.6f, depthPercent: 0.6f));
                //    break;

                //case AnimData.PkgName.TentModernPacked:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_tent_modern_packed", layer: 1, scale: 0.12f));
                //    break;

                //case AnimData.PkgName.TentSmall:
                //    // TODO replace with A - frame tent asset(when found)
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_tent_medium", layer: 1, scale: 0.5f, depthPercent: 0.45f));
                //    break;

                //case AnimData.PkgName.TentMedium:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_tent_medium", layer: 1, scale: 1f, depthPercent: 0.45f));
                //    break;

                //case AnimData.PkgName.TentBig:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_tent_big", layer: 1, scale: 1f, depthPercent: 0.6f));
                //    break;

                //case AnimData.PkgName.BackpackSmall:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_backpack_small", layer: 1, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.BackpackMedium:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_backpack_medium", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.BackpackBig:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_backpack_big", layer: 1, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.BackpackLuxurious:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_backpack_luxurious", layer: 1, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.BeltSmall:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_belt_small", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.BeltMedium:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_belt_medium", layer: 0, scale: 0.12f));
                //    break;

                //case AnimData.PkgName.BeltBig:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_belt_big", layer: 0, scale: 0.06f));
                //    break;

                //case AnimData.PkgName.BeltLuxurious:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_belt_luxurious", layer: 0, scale: 0.06f));
                //    break;

                //case AnimData.PkgName.Map:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_map_item", layer: 0, scale: 0.06f, crop: false));
                //    break;

                //case AnimData.PkgName.HatSimple:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_hat_simple", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.BootsProtective:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_boots_protective", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.BootsMountain:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_boots_mountain", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.BootsSpeed:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_boots_speed", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.BootsAllTerrain:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_boots_all_terrain", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.GlovesStrength:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_gloves_strength", layer: 0, scale: 0.7f));
                //    break;

                //case AnimData.PkgName.GlassesBlue:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_glasses_blue", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.Dungarees:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_dungarees", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.LanternFrame:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_lantern_frame", layer: 0, scale: 0.075f));
                //    break;

                //case AnimData.PkgName.Candle:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_candle", layer: 0, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.Lantern:
                //    {
                //        float scale = 0.075f;
                //        int layer = 0;

                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_lantern_on", layer: layer, scale: scale));
                //        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "_processed_lantern_off", layer: layer, scale: scale));
                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_lantern_off", layer: layer, scale: scale));

                //        break;
                //    }

                //case AnimData.PkgName.SmallTorch:
                //    {
                //        float scale = 0.07f;
                //        int layer = 0;

                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_small_torch_on", layer: layer, scale: scale));
                //        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "_processed_small_torch_off", layer: layer, scale: scale));
                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_small_torch_off", layer: layer, scale: scale));

                //        break;
                //    }

                //case AnimData.PkgName.BigTorch:
                //    {
                //        float scale = 0.1f;
                //        int layer = 0;

                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: ConvertImageToFrameArray(atlasName: "_processed_big_torch_on", layer: layer, scale: scale));
                //        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "_processed_big_torch_off", layer: layer, scale: scale));
                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: ConvertImageToFrameArray(atlasName: "_processed_big_torch_off", layer: layer, scale: scale));

                //        break;
                //    }

                //case AnimData.PkgName.CampfireSmall:
                //    {
                //        int layer = 1;

                //        var frameArray = new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_campfire_on_1", layer: layer, duration: 6, crop: false),
                //            ConvertImageToFrame(atlasName: "_processed_campfire_on_2", layer: layer, duration: 6, crop: false),
                //            ConvertImageToFrame(atlasName: "_processed_campfire_on_3", layer: layer, duration: 6, crop: false)
                //        };
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArray);

                //        frameArray = ConvertImageToFrameArray(atlasName: "_processed_campfire_off", layer: layer, crop: false);
                //        AddFrameArray(pkgName: pkgName, frameArray);
                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: frameArray);

                //        break;
                //    }

                //case AnimData.PkgName.CampfireMedium:
                //    {
                //        int layer = 1;

                //        var frameArray = new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_campfire_medium_on_1", layer: layer, duration: 6, crop: false),
                //            ConvertImageToFrame(atlasName: "_processed_campfire_medium_on_2", layer: layer, duration: 6, crop: false),
                //            ConvertImageToFrame(atlasName: "_processed_campfire_medium_on_3", layer: layer, duration: 6, crop: false)
                //        };
                //        AddFrameArray(pkgName: pkgName, animName: "on", frameArray: frameArray);
                //        AddFrameArray(pkgName: pkgName, ConvertImageToFrameArray(atlasName: "_processed_campfire_medium_off", layer: layer, crop: false));
                //        AddFrameArray(pkgName: pkgName, animName: "off", frameArray: frameArray);

                //        break;
                //    }

                //case AnimData.PkgName.BoatConstruction:
                //    {
                //        float depthPercent = 0.7f;
                //        float scale = 0.7f;

                //        for (int animSize = 0; animSize <= 5; animSize++)
                //        {
                //            AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: $"boat/_processed_boat_construction_{animSize}", layer: 1, scale: scale, depthPercent: depthPercent, ignoreWhenCalculatingMaxSize: true, crop: false), animSize: animSize, updateCroppedFramesForPkgs: animSize == 0); // animSize == 0 should serve as an example (whole blueprint visible)
                //        }

                //        break;
                //    }

                //case AnimData.PkgName.BoatCompleteStanding:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boat/_processed_boat_complete", layer: 1, scale: 0.7f, depthPercent: 0.7f, ignoreWhenCalculatingMaxSize: true, crop: false));
                //    break;

                //case AnimData.PkgName.BoatCompleteCruising:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "boat/_processed_boat_complete", layer: 0, scale: 0.7f, depthPercent: 0.7f, ignoreWhenCalculatingMaxSize: true, crop: false));
                //    break;

                //case AnimData.PkgName.ShipRescue:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_ship_rescue", layer: 1, scale: 1.5f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.HerbsBlack:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_black", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsCyan:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_cyan", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsBlue:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_blue", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsGreen:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_green", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsYellow:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_yellow", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsRed:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_red", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsViolet:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_violet", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsBrown:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_brown", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsDarkViolet:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_dark_violet", layer: 0));
                //    break;

                //case AnimData.PkgName.HerbsDarkGreen:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_herbs_dark_green", layer: 0));
                //    break;

                //case AnimData.PkgName.EmptyBottle:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bottle_empty", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionRed:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_red", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionBlue:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_blue", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionViolet:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_violet", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionYellow:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_yellow", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionCyan:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_cyan", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionGreen:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_green", layer: 1, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionBlack:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_black", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionDarkViolet:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_dark_violet", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionDarkYellow:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_dark_yellow", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionDarkGreen:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_dark_green", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionLightYellow:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bottle_oil", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionTransparent:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_transparent", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.PotionBrown:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_potion_brown", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.BloodSplatter1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_blood_splatter_1", layer: 0));
                //    break;

                //case AnimData.PkgName.BloodSplatter2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_blood_splatter_2", layer: 0));
                //    break;

                //case AnimData.PkgName.BloodSplatter3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_blood_splatter_3", layer: 0));
                //    break;

                //case AnimData.PkgName.HumanSkeleton:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_human_skeleton", layer: 0));
                //    break;

                //case AnimData.PkgName.Hole:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_hole", layer: 0, scale: 1f));
                //    break;

                //case AnimData.PkgName.Explosion:
                //    {
                //        for (byte size = 0; size < 4; size++)
                //        {
                //            var frameList = new List<AnimFrame> { };

                //            for (int y = 0; y < 4; y++)
                //            {
                //                for (int x = 0; x < 4; x++)
                //                {
                //                    frameList.Add(ConvertImageToFrame(atlasName: "explosion", layer: 1, duration: 2, x: x * 32, y: y * 32, width: 32, height: 32, scale: 1.5f * (size + 1), crop: false, ignoreWhenCalculatingMaxSize: true));
                //                }
                //            }

                //            AddFrameArray(pkgName: pkgName, animSize: size, animName: "default", frameArray: frameList.ToArray());
                //        }

                //        break;
                //    }

                //case AnimData.PkgName.SkullAndBones:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_skull_and_bones", layer: 2, scale: 1f));
                //    break;

                //case AnimData.PkgName.MusicNoteSmall:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_music_note", layer: 2));
                //    break;

                //case AnimData.PkgName.MusicNoteBig:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_music_note", layer: 2, scale: 2.5f));
                //    break;

                //case AnimData.PkgName.Miss:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_miss", layer: 2));
                //    break;

                //case AnimData.PkgName.Attack:
                //    {
                //        int layer = 2;

                //        var frameArray = new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_attack_1", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_attack_2", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_attack_3", layer: layer, duration: 6)
                //        };
                //        AddFrameArray(pkgName: pkgName, frameArray: frameArray);

                //        break;
                //    }

                //case AnimData.PkgName.MapMarker:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_map_marker", layer: 2, crop: false, padding: 0, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.Backlight:
                //    {
                //        int layer = 0;

                //        var frameArray = new AnimFrame[]
                //        {
                //            ConvertImageToFrame(atlasName: "_processed_backlight_1", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_backlight_2", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_backlight_3", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_backlight_4", layer: layer, duration: 20),
                //            ConvertImageToFrame(atlasName: "_processed_backlight_3", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_backlight_2", layer: layer, duration: 6),
                //            ConvertImageToFrame(atlasName: "_processed_backlight_1", layer: layer, duration: 6)
                //        };
                //        AddFrameArray(pkgName: pkgName, frameArray: frameArray);
                //        break;
                //    }

                //case AnimData.PkgName.Crosshair:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_crosshair", layer: 2));
                //    break;

                //case AnimData.PkgName.Flame:
                //    {
                //        byte animSize = 0;
                //        int layer = 1;

                //        foreach (float scale in new List<float> { 0.5f, 0.75f, 1f, 1.25f })
                //        {
                //            var frameArray = new AnimFrame[]
                //            {
                //                ConvertImageToFrame(atlasName: "_processed_flame_small_1", layer: layer, duration: 6, crop: true, scale: scale),
                //                ConvertImageToFrame(atlasName: "_processed_flame_small_2", layer: layer, duration: 6, crop: true, scale: scale),
                //                ConvertImageToFrame(atlasName: "_processed_flame_small_3", layer: layer, duration: 6, crop: true, scale: scale)
                //            };
                //            AddFrameArray(pkgName: pkgName, animSize: animSize, frameArray: frameArray);

                //            animSize++;
                //        }

                //        foreach (float scale in new List<float> { 1f, 1.5f, 1.8f, 2f })
                //        {
                //            var frameArray = new AnimFrame[]
                //            {
                //                ConvertImageToFrame(atlasName: "_processed_flame_big_1", layer: layer, duration: 6, crop: true, scale: scale),
                //                ConvertImageToFrame(atlasName: "_processed_flame_big_2", layer: layer, duration: 6, crop: true, scale: scale),
                //                ConvertImageToFrame(atlasName: "_processed_flame_big_3", layer: layer, duration: 6, crop: true, scale: scale)
                //            };
                //            AddFrameArray(pkgName: pkgName, animSize: animSize, frameArray: frameArray);

                //            animSize++;
                //        }

                //        break;
                //    }

                //case AnimData.PkgName.Upgrade:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_upgrade", layer: 0, scale: 1f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WaterDrop:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_water_drop", layer: 0, scale: 0.5f));
                //    break;

                //case AnimData.PkgName.Zzz:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_zzz", layer: 2));
                //    break;

                //case AnimData.PkgName.Heart:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_heart_16x16", layer: 2));
                //    break;

                //case AnimData.PkgName.Hammer:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_hammer", layer: 0, scale: 0.1f));
                //    break;

                //case AnimData.PkgName.Fog1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.Fog2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));

                //    break;

                //case AnimData.PkgName.Fog3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fog_1", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.Fog4:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fog_2", layer: 2, scale: 1.8f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.BubbleExclamationRed:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bubble_exclamation_red", layer: 2, scale: 0.2f));
                //    break;

                //case AnimData.PkgName.BubbleExclamationBlue:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bubble_exclamation_blue", layer: 2, scale: 0.2f));
                //    break;

                //case AnimData.PkgName.BubbleCraftGreen:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_bubble_craft_green", layer: 2, scale: 0.2f));
                //    break;

                //case AnimData.PkgName.SeaWave:
                //    foreach (var kvp in new Dictionary<int, float> { { 0, 0.2f }, { 1, 0.3f }, { 2, 0.4f }, { 3, 0.5f }, { 4, 0.6f }, { 5, 0.7f }, { 6, 0.8f } })
                //    {
                //        int animSize = kvp.Key;
                //        float scale = kvp.Value;

                //        AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_wave", layer: 0, scale: scale, ignoreWhenCalculatingMaxSize: true), animSize: animSize);
                //    }

                //    break;

                //case AnimData.PkgName.WeatherFog1:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_1", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog2:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_2", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog3:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_3", layer: 2, scale: 1.0f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog4:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_1", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog5:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_2", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog6:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_3", layer: 2, scale: 1.4f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog7:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_1", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog8:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_2", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.WeatherFog9:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_weather_fog_3", layer: 2, scale: 2.0f, ignoreWhenCalculatingMaxSize: true));
                //    break;

                //case AnimData.PkgName.FertileGroundSmall:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fertile_ground_small", layer: -1, scale: 1f));
                //    break;

                //case AnimData.PkgName.FertileGroundMedium:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fertile_ground_medium", layer: -1, scale: 1f));
                //    break;

                //case AnimData.PkgName.FertileGroundLarge:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fertile_ground_big", layer: -1, scale: 1.3f));
                //    break;

                //case AnimData.PkgName.FenceHorizontalShort:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fence_horizontal_short", layer: 1, depthPercent: 0.2f));
                //    break;

                //case AnimData.PkgName.FenceVerticalShort:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fence_vertical_short", layer: 1, depthPercent: 0.9f));
                //    break;

                //case AnimData.PkgName.FenceHorizontalLong:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fence_horizontal_long", layer: 1, depthPercent: 0.2f));
                //    break;

                //case AnimData.PkgName.FenceVerticalLong:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_fence_vertical_long", layer: 1, depthPercent: 0.95f));
                //    break;

                //case AnimData.PkgName.CaveEntrance:
                //    {
                //        float scale = 2f;
                //        float depthPercent = 0.95f;

                //        AddFrameArray(pkgName: pkgName, animName: "default", frameArray: ConvertImageToFrameArray(atlasName: "_processed_cave_entrance_open", scale: scale, layer: 1, depthPercent: depthPercent));
                //        AddFrameArray(pkgName: pkgName, animName: "blocked", frameArray: ConvertImageToFrameArray(atlasName: "_processed_cave_entrance_blocked", scale: scale, layer: 1, depthPercent: depthPercent));

                //        break;
                //    }

                //case AnimData.PkgName.CaveExit:
                //    AddFrameArray(pkgName: pkgName, frameArray: ConvertImageToFrameArray(atlasName: "_processed_cave_exit", scale: 2f, layer: 1, depthPercent: 0.95f));
                //    break;

                case AnimData.PkgName.DragonBonesTestFemaleMage:

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

                    animPkg = null; // for testing

                    break;

                default:
                    // throw new ArgumentException($"Unsupported pkgName - {pkgName}."); // TODO enable after removing original AnimData
                    animPkg = null; // TODO remove after removing original AnimData
                    break;
            }

            pkgByName[pkgName] = animPkg;
        }

        public static AnimPkg MakePackageForSingleImage(AnimData.PkgName pkgName, string altasName, int width, int height, int animSize, int layer, bool hasOnePixelMargin = false, float scale = 1f, bool mirrorX = false, bool mirrorY = false)
        {
            int colWidth = (int)((hasOnePixelMargin ? width - 2 : width) * scale);
            int colHeight = (int)((hasOnePixelMargin ? height - 2 : height) * scale);

            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: [new AnimFrameNew(atlasName: altasName, layer: layer, cropRect: new Rectangle(x: 0, y: 0, width: width, height: height), duration: 0, mirrorX: mirrorX, mirrorY: mirrorY, scale: scale)]));

            return animPkg;
        }

        public static AnimPkg MakePackageForRpgMakerV1Data(AnimData.PkgName pkgName, string altasName, int colWidth, int colHeight, Vector2 gfxOffsetCorrection, int setNoX, int setNoY, int animSize, float scale = 1f)
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

                var frameList = new List<AnimFrameNew>
                {
                    new AnimFrameNew(atlasName: altasName, layer: 1, cropRect: new Rectangle(x: width + setOffsetX, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection)
                };

                animPkg.AddAnim(new(animPkg: animPkg, name: $"stand-{animName}", size: animSize, frameArray: frameList.ToArray()));
                if (!defaultSet)
                {
                    animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: animSize, frameArray: new AnimFrameNew[] { frameList[0] }));
                    defaultSet = true;
                }
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrameNew> { };

                foreach (int x in new int[] { setOffsetX + width, setOffsetX + (width * 2) })
                {
                    frameList.Add(new AnimFrameNew(atlasName: altasName, layer: 1, cropRect: new Rectangle(x: x, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 8, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection));
                }

                animPkg.AddAnim(new(animPkg: animPkg, name: $"walk-{animName}", size: animSize, frameArray: frameList.ToArray()));
            }

            return animPkg;
        }

        public static AnimPkg MakePackageForRPGMakerPackageV2UsingSizeDict(AnimData.PkgName pkgName, string atlasName, int colWidth, int colHeight, int setNoX, int setNoY, Dictionary<byte, float> scaleForSizeDict, Vector2 gfxOffsetCorrection)
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

        public static AnimPkg MakePackageForRpgMakerV2Data(AnimData.PkgName pkgName, string atlasName, int colWidth, int colHeight, Vector2 gfxOffsetCorrection, int setNoX, int setNoY, int animSize, float scale = 1f, AnimPkg animPkg = null)
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

                var frameList = new List<AnimFrameNew>
                {
                    new AnimFrameNew(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: width + setOffsetX, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection)
                };

                animPkg.AddAnim(new(animPkg: animPkg, name: $"stand-{animName}", size: animSize, frameArray: frameList.ToArray()));
                if (!defaultSet)
                {
                    animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: animSize, frameArray: new AnimFrameNew[] { frameList[0] }));
                    defaultSet = true;
                }
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrameNew> { };

                foreach (int x in new int[] { setOffsetX, setOffsetX + width, setOffsetX + (width * 2), setOffsetX + width })
                {
                    frameList.Add(new AnimFrameNew(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: x, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 8, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection));
                }

                animPkg.AddAnim(new(animPkg: animPkg, name: $"walk-{animName}", size: animSize, frameArray: frameList.ToArray()));
            }

            return animPkg;
        }

        public static AnimPkg MakePackageForDragonBonesAnims(AnimData.PkgName pkgName, int colWidth, int colHeight, string[] jsonNameArray, int animSize, float scale, bool baseAnimsFaceRight, Dictionary<string, int> durationDict = null, List<string> nonLoopedAnims = null, Dictionary<string, Vector2> offsetDict = null, Dictionary<string, string> switchDict = null, Vector2 globalOffsetCorrection = default)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

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

                var animDict = new Dictionary<string, Dictionary<int, AnimFrameNew>>();

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
                        gfxOffsetCorrection.X += fullWidth / 6.2f * (mirrorX ? 1f : -1f); // 6.2f seems to center animation properly, TODO check if gonna work in other anims

                        if (offsetDict.ContainsKey(animNameWithDirection)) gfxOffsetCorrection += offsetDict[animNameWithDirection]; // corrections for individual anims
                        gfxOffsetCorrection += globalOffsetCorrection;

                        AnimFrameNew animFrame = new AnimFrameNew(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: x, y: y, width: croppedWidth, height: croppedHeight), duration: duration, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, mirrorX: mirrorX);

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

                    AnimFrameNew[] frameArray = new AnimFrameNew[framesCount];
                    foreach (var kvp2 in frameDict)
                    {
                        int frameNo = kvp2.Key;
                        AnimFrameNew animFrame = kvp2.Value;

                        if (nonLoopedAnim && frameNo == framesCount - 1)
                        {
                            animFrame = new AnimFrameNew(atlasName: animFrame.atlasName, layer: animFrame.layer, cropRect: animFrame.cropRect, duration: 0, scale: animFrame.scale, gfxOffsetCorrection: animFrame.gfxOffsetCorrection / animFrame.scale, mirrorX: animFrame.spriteEffects == SpriteEffects.FlipHorizontally, ignoreWhenCalculatingMaxSize: animFrame.ignoreWhenCalculatingMaxSize);
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