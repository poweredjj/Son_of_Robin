using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimPkg
    {
        public readonly AnimData.PkgName pkgName;
        public readonly Rectangle colRect;
        public readonly AnimFrame croppedFrame;
        private readonly Dictionary<int, Dictionary<string, Anim>> animDict;
        public readonly bool leftRightOnly;

        public AnimPkg(AnimData.PkgName pkgName, Rectangle colRect, AnimFrame croppedFrame, bool leftRightOnly = false)
        {
            this.pkgName = pkgName;
            this.colRect = colRect;
            this.animDict = new Dictionary<int, Dictionary<string, Anim>>();
            this.croppedFrame = croppedFrame;
            this.leftRightOnly = leftRightOnly;
        }

        public void AddAnim(Anim anim)
        {
            if (!this.animDict.ContainsKey(anim.size)) this.animDict[anim.size] = new Dictionary<string, Anim>();
            this.animDict[anim.size][anim.name] = anim;
        }

        public Anim GetAnim(int size, string name)
        {
            return this.animDict[size][name];
        }
    }
}