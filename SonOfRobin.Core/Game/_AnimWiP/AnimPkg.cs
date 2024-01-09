﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
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
            colWidth = (int)(colWidth * scale);
            colHeight = (int)(colHeight * scale);

            AnimPkg animPkg = new(pkgName: pkgName, colWidth: colWidth, colHeight: colHeight);

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
                animPkg.AddAnim(new(animPkg: animPkg, name: "default", size: animSize, frameArray: new AnimFrameNew[] { frameList[0] }));
            }

            // walking
            foreach (var kvp in yByDirection)
            {
                string animName = kvp.Key;
                int directionOffsetY = kvp.Value;

                var frameList = new List<AnimFrameNew>
                {
                    new AnimFrameNew(atlasName: altasName, layer: 1, cropRect: new Rectangle(x: width + setOffsetX, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 8, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection),

                    new AnimFrameNew(atlasName: altasName, layer: 1, cropRect: new Rectangle(x: (width * 2) + setOffsetX, y: setOffsetY + directionOffsetY, width: width, height: height), duration: 8, scale: scale, gfxOffsetCorrection: gfxOffsetCorrection)
                };

                animPkg.AddAnim(new(animPkg: animPkg, name: $"walk-{animName}", size: animSize, frameArray: frameList.ToArray()));
            }

            return animPkg;
        }
    }
}