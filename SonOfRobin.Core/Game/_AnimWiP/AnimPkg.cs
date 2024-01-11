using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class AnimPkg
    {
        public readonly AnimData.PkgName pkgName;
        public readonly Rectangle colRect;
        private readonly Dictionary<int, Dictionary<string, Anim>> animDict;
        public readonly bool horizontalOrientationsOnly;
        public List<Anim> AllAnimList { get; private set; } // "flat" list of all anims
        public int[] AllAnimSizes { get { return this.animDict.Keys.ToArray(); } }

        public AnimPkg(AnimData.PkgName pkgName, int colWidth, int colHeight, bool horizontalOrientationsOnly = false)
        {
            this.pkgName = pkgName;
            this.colRect = new Rectangle(x: -colWidth / 2, y: -colHeight / 2, width: colWidth, height: colHeight); // colRect should always be centered
            this.animDict = [];
            this.AllAnimList = [];
            this.horizontalOrientationsOnly = horizontalOrientationsOnly;
        }

        public void AddAnim(Anim anim)
        {
            if (!this.animDict.ContainsKey(anim.size)) this.animDict[anim.size] = [];
            if (this.animDict[anim.size].ContainsKey(anim.name)) return; // to prevent from adding multiple animations with the same size and name
            this.animDict[anim.size][anim.name] = anim;
            this.AllAnimList.Add(anim);
        }

        public AnimPkg MakeCopyWithEditedColOffset(int colWidth, int colHeight)
        {
            AnimPkg animPkg = new AnimPkg(pkgName: this.pkgName, colWidth: colWidth, colHeight: colHeight, horizontalOrientationsOnly: this.horizontalOrientationsOnly);

            foreach (Anim anim in this.AllAnimList)
            {
                animPkg.AddAnim(anim);
            }

            return animPkg;
        }

        public void EditGfxOffsetCorrection(Vector2 gfxOffsetCorrection)
        {
            foreach (Anim anim in this.AllAnimList)
            {
                anim.EditGfxOffsetCorrection(gfxOffsetCorrection);
            }
        }

        public Anim GetAnim(int size, string name)
        {
            return this.animDict[size][name];
        }

        public string[] GetAnimNamesForSize(int size)
        {
            return this.animDict[size].Keys.ToArray();
        }

        public Rectangle GetColRectForPos(Vector2 position)
        {
            return new(
                x: (int)position.X + this.colRect.X,
                y: (int)position.Y + this.colRect.Y,
                width: this.colRect.Width,
                height: this.colRect.Height);
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

        public static AnimPkg MakePackageForDragonBonesAnims(AnimData.PkgName pkgName, int colWidth, int colHeight, string[] jsonNameArray, int animSize, float scale, bool baseAnimsFaceRight, Dictionary<string, int> durationDict, string[] nonLoopedAnims, Dictionary<string, Vector2> offsetDict)
        {
            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

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
                        gfxOffsetCorrection.X += fullWidth / 6.0f * (mirrorX ? 1f : -1f);

                        //if (offsetDict.ContainsKey(animNameWithDirection)) gfxOffsetCorrection += offsetDict[animNameWithDirection];

                        AnimFrameNew animFrame = new AnimFrameNew(atlasName: atlasName, layer: 1, cropRect: new Rectangle(x: x, y: y, width: croppedWidth, height: croppedHeight), duration: duration, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection, mirrorX: mirrorX);

                        animDict[animNameWithDirection][frameNo] = animFrame;
                    }
                }

                foreach (var kvp1 in animDict)
                {
                    string animName = (string)kvp1.Key;
                    var frameDict = kvp1.Value;
                    int framesCount = frameDict.Keys.Max() + 1;

                    bool nonLoopedAnim = nonLoopedAnims.Where(n => animName.Contains(n)).Any();

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

                    animPkg.AddAnim(new(animPkg: animPkg, name: animName, size: animSize, frameArray: frameArray));
                }
            }

            return animPkg;
        }
    }
}