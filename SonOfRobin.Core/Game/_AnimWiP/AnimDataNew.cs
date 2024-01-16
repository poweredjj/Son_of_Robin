using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SonOfRobin.AnimData;

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

                //case AnimData.PkgName.MineralsBig1:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 37);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 222, height: 310), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -80))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig2:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 37);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 210, height: 308), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -76))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig3:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 77, colHeight: 41);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 312, height: 346), scale: 0.3f, gfxOffsetCorrection: new Vector2(14, -67))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig4:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 55, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 213, height: 312), scale: 0.3f, gfxOffsetCorrection: new Vector2(4, -77))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig5:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 74, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_5", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 298, height: 294), scale: 0.3f, gfxOffsetCorrection: new Vector2(16, -66))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig6:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 55, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_6", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 217, height: 306), scale: 0.3f, gfxOffsetCorrection: new Vector2(3, -80))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig7:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 69, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_7", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 251, height: 297), scale: 0.3f, gfxOffsetCorrection: new Vector2(1, -75))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig8:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 56, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_8", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 228, height: 312), scale: 0.3f, gfxOffsetCorrection: new Vector2(-1, -81))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig9:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_9", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 230, height: 342), scale: 0.3f, gfxOffsetCorrection: new Vector2(1, -97))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig10:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_10", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 231, height: 301), scale: 0.3f, gfxOffsetCorrection: new Vector2(4, -85))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig11:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 90, colHeight: 44);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_11", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 321, height: 333), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -78))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig12:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 60, colHeight: 42);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_12", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 217, height: 328), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -84))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig13:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 59, colHeight: 42);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_13", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 229, height: 308), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -81))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsBig14:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_big_14", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 266, height: 269), scale: 0.3f, gfxOffsetCorrection: new Vector2(-10, -54))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall1:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 25);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 232, height: 163), scale: 0.3f, gfxOffsetCorrection: new Vector2(-1, -21))]));
                //        break;
                //    }
                //case AnimData.PkgName.MineralsSmall2:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 45, colHeight: 22);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 186, height: 186), scale: 0.3f, gfxOffsetCorrection: new Vector2(11, -47))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall3:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 68, colHeight: 26);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 256, height: 164), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -25))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall4:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 68, colHeight: 24);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 261, height: 145), scale: 0.3f, gfxOffsetCorrection: new Vector2(2, -19))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall5:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 26);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_5", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 161, height: 175), scale: 0.3f, gfxOffsetCorrection: new Vector2(3, -35))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall6:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 52, colHeight: 25);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_6", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 216, height: 156), scale: 0.3f, gfxOffsetCorrection: new Vector2(4, -32))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall7:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 47, colHeight: 25);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_7", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 195, height: 181), scale: 0.3f, gfxOffsetCorrection: new Vector2(8, -32))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall8:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 47, colHeight: 25);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_8", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 182, height: 172), scale: 0.3f, gfxOffsetCorrection: new Vector2(-6, -33))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall9:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 23);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_9", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 151, height: 154), scale: 0.3f, gfxOffsetCorrection: new Vector2(-3, -33))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsSmall10:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 22);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_minerals_small_10", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 254, height: 150), scale: 0.3f, gfxOffsetCorrection: new Vector2(-2, -26))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig1:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 74, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 265, height: 387), scale: 0.3f, gfxOffsetCorrection: new Vector2(-2, -122))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig2:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 57, colHeight: 35);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 232, height: 295), scale: 0.3f, gfxOffsetCorrection: new Vector2(-8, -72))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig3:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 55, colHeight: 34);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 233, height: 295), scale: 0.3f, gfxOffsetCorrection: new Vector2(-1, -69))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig4:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 72, colHeight: 36);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 304, height: 394), scale: 0.3f, gfxOffsetCorrection: new Vector2(1, -130))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig5:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 34);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_5", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 258, height: 384), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -124))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig6:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 70, colHeight: 34);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_6", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 270, height: 381), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -124))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig7:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 65, colHeight: 34);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_7", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 231, height: 307), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -88))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig8:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 58, colHeight: 38);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_8", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 209, height: 305), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -80))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig9:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 74, colHeight: 25);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_9", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 265, height: 302), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -103))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig10:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 79, colHeight: 18);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_10", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 275, height: 291), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -107))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig11:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 71, colHeight: 19);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_11", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 254, height: 292), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -107))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossyBig12:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 84, colHeight: 20);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_big_12", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 298, height: 295), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -104))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossySmall1:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 77, colHeight: 20);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_small_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 266, height: 199), scale: 0.3f, gfxOffsetCorrection: new Vector2(1, -64))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossySmall2:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 77, colHeight: 20);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_small_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 272, height: 286), scale: 0.3f, gfxOffsetCorrection: new Vector2(1, -64))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossySmall3:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 71, colHeight: 20);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_small_3", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 252, height: 166), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -36))]));
                //        break;
                //    }

                //case AnimData.PkgName.MineralsMossySmall4:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 67, colHeight: 20);
                //        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_mossy_minerals_small_4", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 246, height: 183), scale: 0.3f, gfxOffsetCorrection: new Vector2(0, -43))]));
                //        break;
                //    }

                case AnimData.PkgName.MineralsCave:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 48, colHeight: 29);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_cave_minerals", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 170, height: 157), scale: 0.3f, gfxOffsetCorrection: new Vector2(1, -27))]));
                        break;
                    }

                case AnimData.PkgName.JarWhole:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_jar_sealed", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 43, height: 47), scale: 0.6f, gfxOffsetCorrection: new Vector2(0, -10))]));
                        break;
                    }

                case AnimData.PkgName.JarBroken:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_jar_broken", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 41, height: 47), scale: 0.6f, gfxOffsetCorrection: new Vector2(0, -10))]));
                        break;
                    }

                case AnimData.PkgName.ChestWooden:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chest_wooden/_processed_chest_wooden_", cropRect: new Rectangle(x: 0, y: 0, width: 229, height: 259), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case AnimData.PkgName.ChestStone:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chest_stone/_processed_chest_stone_", cropRect: new Rectangle(x: 0, y: 0, width: 227, height: 258), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case AnimData.PkgName.ChestIron:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chest_iron/_processed_chest_iron_", cropRect: new Rectangle(x: 0, y: 0, width: 229, height: 259), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case AnimData.PkgName.ChestCrystal:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 37, colHeight: 15, atlasNameBase: "chest_crystal/_processed_chest_crystal_", cropRect: new Rectangle(x: 0, y: 0, width: 226, height: 258), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -81));
                        break;
                    }

                case AnimData.PkgName.ChestTreasureBlue:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 46, colHeight: 19, atlasNameBase: "chest_blue/_processed_chest_blue_", cropRect: new Rectangle(x: 0, y: 0, width: 270, height: 308), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -95));
                        break;
                    }

                case AnimData.PkgName.ChestTreasureRed:
                    {
                        animPkg = AddChestPackage(pkgName: pkgName, colWidth: 46, colHeight: 19, atlasNameBase: "chest_red/_processed_chest_red_", cropRect: new Rectangle(x: 0, y: 0, width: 270, height: 307), frameDuration: 3, animLength: 6, scale: 0.18f, gfxOffsetCorrection: new Vector2(-3, -95));
                        break;
                    }

                case AnimData.PkgName.WoodLogRegular:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 19, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_wood_regular", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 30, height: 30), scale: 0.75f, gfxOffsetCorrection: new Vector2(1, -4))]));
                        break;
                    }

                case AnimData.PkgName.WoodLogHard:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 19, colHeight: 14);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_wood_hard", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 44, height: 44), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -6))]));
                        break;
                    }

                case AnimData.PkgName.WoodPlank:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 23, height: 23, scale: 0.8f, layer: 0, animSize: 0, altasName: "_processed_wood_plank", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Nail:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 31, height: 27, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_nail", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Rope:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 25, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_rope", hasOnePixelMargin: true);
                        break;
                    }
                case AnimData.PkgName.HideCloth:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 504, height: 330, scale: 0.1f, layer: 0, animSize: 0, altasName: "_processed_hidecloth", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Crate:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 23, colHeight: 16);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_crate", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 25, height: 31), scale: 1f, gfxOffsetCorrection: new Vector2(1, -6))]));
                        break;
                    }

                case AnimData.PkgName.WorkshopEssential:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_workshop_essential", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14));

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.WorkshopBasic:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_workshop_basic", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14));

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.WorkshopAdvanced:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_workshop_advanced", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14));

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.WorkshopMaster:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_workshop_master", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 82, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14));

                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.WorkshopMeatHarvesting:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_workshop_meat_harvesting_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14))]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_workshop_meat_harvesting_on", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14))]));
                        break;
                    }

                case AnimData.PkgName.WorkshopLeatherBasic:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_workshop_leather_basic", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 62, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14));

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.WorkshopLeatherAdvanced:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_workshop_leather_advanced", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 82, height: 78), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -14));

                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.MeatDryingRackRegular:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 75, colHeight: 35);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_meat_drying_rack_regular_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 77, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));

                        for (int i = 1; i <= 4; i++)
                        {
                            animPkg.AddAnim(new(animPkg: animPkg, name: $"on_{i}", size: 0, frameArray: [new AnimFrameNew(atlasName: $"_processed_meat_drying_rack_regular_on_{i}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 77, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));
                        }
                        break;
                    }

                case AnimData.PkgName.MeatDryingRackWide:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 100, colHeight: 35);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_meat_drying_rack_wide_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 102, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));

                        for (int i = 1; i <= 6; i++)
                        {
                            animPkg.AddAnim(new(animPkg: animPkg, name: $"on_{i}", size: 0, frameArray: [new AnimFrameNew(atlasName: $"_processed_meat_drying_rack_wide_on_{i}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 102, height: 77), scale: 1f, gfxOffsetCorrection: new Vector2(1, -18))]));
                        }
                        break;
                    }

                case AnimData.PkgName.AlchemyLabStandard:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_alchemy_lab_standard", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 64, height: 90), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -19));

                        animPkg = new(pkgName: pkgName, colWidth: 30, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.AlchemyLabAdvanced:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_alchemy_lab_advanced", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 82, height: 90), scale: 0.5f, gfxOffsetCorrection: new Vector2(1, -19));

                        animPkg = new(pkgName: pkgName, colWidth: 40, colHeight: 24);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                //case AnimData.PkgName.FurnaceConstructionSite:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 52, colHeight: 37);

                //        for (int animSize = 0; animSize <= 2; animSize++)
                //        {
                //            animPkg.AddAnim(new(animPkg: animPkg, size: animSize, frameArray: [new AnimFrameNew(atlasName: $"furnace/_processed_furnace_construction_{animSize}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 603), scale: 0.2f, gfxOffsetCorrection: new Vector2(-2, -200))]));
                //        }

                //        animPkg.presentationFrame = animPkg.GetAnim(size: 0, name: "default").frameArray[0]; // animSize == 0 should serve as an example (whole blueprint visible)

                //        break;
                //    }

                //case AnimData.PkgName.FurnaceComplete:
                //    {
                //        animPkg = new(pkgName: pkgName, colWidth: 52, colHeight: 37);
                //        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrameNew(atlasName: "furnace/_processed_furnace_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 603), scale: 0.2f, gfxOffsetCorrection: new Vector2(-2, -200))]));
                //        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [new AnimFrameNew(atlasName: "furnace/_processed_furnace_on", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 291, height: 603), scale: 0.2f, gfxOffsetCorrection: new Vector2(-2, -200))]));
                //        break;
                //    }

                case AnimData.PkgName.Anvil:
                    {
                        AnimFrameNew frame = new AnimFrameNew(atlasName: "_processed_anvil", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 50, height: 33), scale: 1f, gfxOffsetCorrection: new Vector2(1, -4));

                        animPkg = new(pkgName: pkgName, colWidth: 34, colHeight: 21);
                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [frame]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: [frame]));
                        break;
                    }

                case AnimData.PkgName.HotPlate:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 32, colHeight: 24);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_hot_plate_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 40, height: 43), scale: 1f, gfxOffsetCorrection: new Vector2(1, -8))]));

                        var frameList = new List<AnimFrameNew>();

                        for (int i = 0; i < 3; i++)
                        {
                            frameList.Add(new AnimFrameNew(atlasName: $"_processed_hot_plate_on_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 40, height: 43), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, -8)));
                        }

                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: frameList.ToArray()));
                        break;
                    }

                case AnimData.PkgName.CookingPot:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 32, colHeight: 17);

                        animPkg.AddAnim(new(animPkg: animPkg, name: "off", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_cooking_pot_off", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 30), scale: 1f, gfxOffsetCorrection: new Vector2(1, -4))]));

                        var frameList = new List<AnimFrameNew>();

                        for (int i = 0; i < 3; i++)
                        {
                            frameList.Add(new AnimFrameNew(atlasName: $"_processed_cooking_pot_on_{i + 1}", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 34, height: 30), scale: 1f, duration: 6, gfxOffsetCorrection: new Vector2(1, -4)));
                        }

                        animPkg.AddAnim(new(animPkg: animPkg, name: "on", size: 0, frameArray: frameList.ToArray()));
                        break;
                    }

                case AnimData.PkgName.Totem:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 21, colHeight: 40);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_totem", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 186, height: 467), scale: 0.25f, gfxOffsetCorrection: new Vector2(-1, -149))]));
                        break;
                    }

                case AnimData.PkgName.RuinsWallHorizontal1:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 79, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_ruins_wall_horizontal_1", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 81, height: 66), scale: 1f, gfxOffsetCorrection: new Vector2(1, -19))]));
                        break;
                    }

                case AnimData.PkgName.RuinsWallHorizontal2:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 62, colHeight: 25);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_ruins_wall_horizontal_2", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 64, height: 66), scale: 1f, gfxOffsetCorrection: new Vector2(1, -19))]));
                        break;
                    }

                case AnimData.PkgName.RuinsWallWallVertical:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 12, colHeight: 35);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_ruins_wall_vertical", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 14, height: 66), scale: 1f, gfxOffsetCorrection: new Vector2(0, -14))]));
                        break;
                    }

                case AnimData.PkgName.RuinsColumn:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 27, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_ruins_column", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 29, height: 32), scale: 1f, gfxOffsetCorrection: new Vector2(0, -5))]));
                        break;
                    }

                case AnimData.PkgName.RuinsRubble:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 29, colHeight: 18);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_ruins_rubble", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 31, height: 28), scale: 1f, gfxOffsetCorrection: new Vector2(0, -3))]));
                        break;
                    }

                case AnimData.PkgName.Stick:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 22, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_stick", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Stone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 21, height: 20, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Granite:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 19, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_granite", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 21, height: 19), scale: 1f, gfxOffsetCorrection: new Vector2(0, -2))]));
                        break;
                    }

                case AnimData.PkgName.Clay:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 25, height: 23, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_clay", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Apple:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 218, height: 264, scale: 0.075f, layer: 0, animSize: 0, altasName: "_processed_apple", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Banana:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 432, height: 342, scale: 0.08f, layer: 0, animSize: 0, altasName: "_processed_banana", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Cherry:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 242, height: 158, scale: 0.12f, layer: 0, animSize: 0, altasName: "_processed_cherry", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Tomato:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 234, height: 257, scale: 0.07f, layer: 0, animSize: 0, altasName: "_processed_tomato", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Carrot:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 195, height: 445, scale: 0.08f, layer: 0, animSize: 0, altasName: "_processed_carrot", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Acorn:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 131, height: 202, scale: 0.13f, layer: 0, animSize: 0, altasName: "_processed_acorn", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Mushroom:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 32, height: 30, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_mushroom", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.SeedBag:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 211, height: 258, scale: 0.08f, layer: 0, animSize: 0, altasName: "_processed_seed_bag", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.CoffeeRaw:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 28, height: 24, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_coffee_raw", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.CoffeeRoasted:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 28, height: 24, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_coffee_roasted", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Clam:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 28, height: 30, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_clam", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.MeatRawRegular:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 228, height: 161, scale: 0.1f, layer: 0, animSize: 0, altasName: "_processed_meat_raw_regular", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.MeatRawPrime:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 249, height: 221, scale: 0.1f, layer: 0, animSize: 0, altasName: "_processed_meat_raw_prime", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.MeatDried:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 298, height: 145, scale: 0.1f, layer: 0, animSize: 0, altasName: "_processed_meat_dried", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Fat:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 236, height: 201, scale: 0.08f, layer: 0, animSize: 0, altasName: "_processed_fat", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Burger:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 256, height: 233, scale: 0.07f, layer: 0, animSize: 0, altasName: "_processed_burger", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.MealStandard:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 34, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_meal_standard", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Leather:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 23, height: 26, scale: 0.75f, layer: 0, animSize: 0, altasName: "_processed_leather", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.KnifeSimple:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 24, height: 26, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_knife_simple", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.AxeWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 29, height: 34, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_axe_wooden", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.AxeStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 41, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_axe_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.AxeIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 41, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_axe_iron", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.AxeCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 50, height: 41, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_axe_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.PickaxeWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_pickaxe_wood", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.PickaxeStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_pickaxe_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.PickaxeIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_pickaxe_iron", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.PickaxeCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 43, height: 40, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_pickaxe_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ScytheStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 21, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_scythe_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ScytheIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 21, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_scythe_iron", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ScytheCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 21, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_scythe_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.SpearWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 54, height: 54, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_spear_wood", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.SpearStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 54, height: 54, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_spear_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.SpearIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 57, height: 57, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_spear_iron", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.SpearCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 57, height: 57, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_spear_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ShovelStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_shovel_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ShovelIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_shovel_iron", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ShovelCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 34, height: 33, scale: 0.7f, layer: 0, animSize: 0, altasName: "_processed_shovel_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.BowBasic:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 78, height: 116, scale: 0.25f, layer: 0, animSize: 0, altasName: "_processed_bow_basic", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.BowAdvanced:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 78, height: 116, scale: 0.25f, layer: 0, animSize: 0, altasName: "_processed_bow_advanced", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ArrowWood:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 26, scale: 0.75f, layer: 0, animSize: 0, altasName: "_processed_arrow_wood", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ArrowStone:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 26, height: 26, scale: 0.75f, layer: 0, animSize: 0, altasName: "_processed_arrow_stone", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ArrowIron:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 27, height: 27, scale: 0.75f, layer: 0, animSize: 0, altasName: "_processed_arrow_iron", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ArrowCrystal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 27, height: 27, scale: 0.75f, layer: 0, animSize: 0, altasName: "_processed_arrow_crystal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.ArrowExploding:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 20, colHeight: 20);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_arrow_burning_off", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 27, height: 27), scale: 0.75f, gfxOffsetCorrection: new Vector2(0, 0))]));
                        animPkg.AddAnim(new(animPkg: animPkg, name: "burning", size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_arrow_burning_on", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 32), scale: 0.75f, gfxOffsetCorrection: new Vector2(-3, -2))]));
                        break;
                    }

                case AnimData.PkgName.CoalDeposit:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 22);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_coal_deposit", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 47), scale: 1f, gfxOffsetCorrection: new Vector2(1, -9))]));
                        break;
                    }

                case AnimData.PkgName.IronDeposit:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 22);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_iron_deposit", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 47), scale: 1f, gfxOffsetCorrection: new Vector2(1, -9))]));
                        break;
                    }

                case AnimData.PkgName.CrystalDepositSmall:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 46, colHeight: 19);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_crystal_deposit_small", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 47), scale: 1f, gfxOffsetCorrection: new Vector2(1, -10))]));
                        break;
                    }

                case AnimData.PkgName.CrystalDepositBig:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 45, colHeight: 30);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_crystal_deposit_big", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 48, height: 96), scale: 1f, gfxOffsetCorrection: new Vector2(0, -29))]));
                        break;
                    }

                case AnimData.PkgName.DigSite:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 80, height: 80, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_dig_site", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.DigSiteGlass:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 80, colHeight: 80);

                        var frameList = new List<AnimFrameNew>();

                        frameList.Add(new AnimFrameNew(atlasName: "_processed_dig_site_glass", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 80, height: 80), scale: 1f, duration: 450));

                        foreach (int frameNo in new List<int> { 1, 2, 3, 2, 1 })
                        {
                            frameList.Add(new AnimFrameNew(atlasName: $"_processed_dig_site_glass_shine_{frameNo}", layer: 0, cropRect: new Rectangle(x: 0, y: 0, width: 80, height: 80), scale: 1f, duration: 2));
                        }

                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: frameList.ToArray()));
                        break;
                    }

                case AnimData.PkgName.DigSiteRuins:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 162, height: 164, scale: 0.35f, layer: 0, animSize: 0, altasName: "_processed_dig_site_ruins", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Coal:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 49, height: 48, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_coal", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.IronOre:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 49, height: 48, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_iron_ore", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.IronBar:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 24, height: 39, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_iron_bar", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.IronRod:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 22, height: 22, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_iron_rod", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.IronPlate:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 18, height: 26, scale: 1f, layer: 0, animSize: 0, altasName: "_processed_iron_plate", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.GlassSand:
                    {
                        animPkg = MakePackageForSingleImage(pkgName: pkgName, width: 49, height: 48, scale: 0.5f, layer: 0, animSize: 0, altasName: "_processed_glass_sand", hasOnePixelMargin: true);
                        break;
                    }

                case AnimData.PkgName.Crystal:
                    {
                        animPkg = new(pkgName: pkgName, colWidth: 14, colHeight: 10);
                        animPkg.AddAnim(new(animPkg: animPkg, size: 0, frameArray: [new AnimFrameNew(atlasName: "_processed_crystal", layer: 1, cropRect: new Rectangle(x: 0, y: 0, width: 32, height: 32), scale: 0.5f, gfxOffsetCorrection: new Vector2(0, -5))]));
                        break;
                    }

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

        private static AnimPkg AddChestPackage(PkgName pkgName, int colWidth, int colHeight, string atlasNameBase, Rectangle cropRect, int frameDuration, int animLength, float scale, Vector2 gfxOffsetCorrection)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

            var openingFrameArray = new AnimFrameNew[animLength];
            var closingFrameArray = new AnimFrameNew[animLength];

            for (int i = 0; i < animLength; i++)
            {
                openingFrameArray[i] = new AnimFrameNew(atlasName: $"{atlasNameBase}{i + 1}", layer: 1, cropRect: cropRect, duration: i < animLength - 1 ? frameDuration : 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection);
                closingFrameArray[animLength - i - 1] = new AnimFrameNew(atlasName: $"{atlasNameBase}{i + 1}", layer: 1, cropRect: cropRect, duration: i > 0 ? frameDuration : 0, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection);
            }

            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "opening", frameArray: openingFrameArray));
            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "closing", frameArray: closingFrameArray));
            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "closed", frameArray: [openingFrameArray[0]]));
            animPkg.AddAnim(new Anim(animPkg: animPkg, size: 0, name: "open", frameArray: [closingFrameArray[0]]));

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