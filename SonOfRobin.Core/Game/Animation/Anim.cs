using Microsoft.Xna.Framework;
using System.Linq;

namespace SonOfRobin
{
    public readonly struct Anim
    {
        public readonly string name;
        public readonly int size;
        public readonly AnimPkg animPkg;
        public readonly bool looped;
        public readonly bool pingPong;
        public readonly int duration;
        public readonly bool switchWhenComplete;
        public readonly string switchName;
        public readonly AnimFrame[] frameArray;

        public Anim(AnimPkg animPkg, int size, AnimFrame[] frameArray, string name = "default", bool pingPong = false, string switchName = "")
        {
            this.animPkg = animPkg;
            this.name = name;
            this.size = size;
            this.looped = !frameArray.Any(obj => obj.duration == 0);
            this.pingPong = pingPong;

            if (pingPong)
            {
                AnimFrame[] frameArrayPingPong = new AnimFrame[frameArray.Length * 2];

                for (int i = 0; i < frameArray.Length; i++)
                {
                    frameArrayPingPong[i] = frameArray[i];
                    frameArrayPingPong[frameArray.Length + (frameArray.Length - i - 1)] = frameArray[i];
                }

                frameArray = frameArrayPingPong;
            }

            this.frameArray = frameArray;
            this.duration = frameArray.Select(f => (int)f.duration).Sum();
            this.switchWhenComplete = switchName != "";
            this.switchName = switchName;
        }

        public void EditGfxOffsetCorrection(Vector2 gfxOffsetCorrection)
        {
            for (int i = 0; i < this.frameArray.Length; i++)
            {
                this.frameArray[i] = this.frameArray[i].MakeCopyWithEditedGfxOffsetCorrection(gfxOffsetCorrection);
            }
        }

        public void EditShadowOriginFactor(Vector2 shadowOriginFactor)
        {
            for (int i = 0; i < this.frameArray.Length; i++)
            {
                this.frameArray[i] = this.frameArray[i].MakeCopyWithEditedShadowOriginFactor(shadowOriginFactor);
            }
        }
    }
}