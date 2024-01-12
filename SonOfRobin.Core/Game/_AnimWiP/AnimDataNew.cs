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

                    animPkg = MakePackageForDragonBonesAnims(pkgName: AnimData.PkgName.DragonBonesTestFemaleMage, colWidth: 20, colHeight: 20, jsonNameArray: jsonNameArray, animSize: 0, scale: 0.5f, baseAnimsFaceRight: false, durationDict: durationDict, switchDict: switchDict, nonLoopedAnims: nonLoopedAnims, globalOffsetCorrection: new Vector2(0, -39));

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