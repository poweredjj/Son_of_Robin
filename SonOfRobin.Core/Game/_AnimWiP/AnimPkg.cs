using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimPkg
    {
        public readonly Rectangle colRect;
        public readonly AnimFrame croppedFrame;
        private readonly Dictionary<string, Anim> animByName;
        public readonly bool leftRightOnly;

        public AnimPkg(Rectangle colRect, AnimFrame croppedFrame, bool leftRightOnly = false)
        {
            this.colRect = colRect;
            this.animByName = new Dictionary<string, Anim>();
            this.croppedFrame = croppedFrame;
            this.leftRightOnly = leftRightOnly;
        }

        public void AddAnim(Anim anim)
        {
            animByName[anim.name] = anim;
        }

        public Anim GetAnim(string name)
        {
            return animByName[name];
        }
    }
}