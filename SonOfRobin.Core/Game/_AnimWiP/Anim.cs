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
        public readonly AnimFrameNew[] frameArray;

        public Anim(AnimPkg animPkg, int size, AnimFrameNew[] frameArray, string name = "default", bool looped = true, bool pingPong = false, string switchName = "")
        {
            this.animPkg = animPkg;
            this.name = name;
            this.size = size;
            this.looped = looped;
            this.pingPong = pingPong;

            if (looped)
            {
                AnimFrameNew[] frameArrayLooped = new AnimFrameNew[frameArray.Length * 2];

                for (int i = 0; i < frameArray.Length; i++)
                {
                    frameArrayLooped[i] = frameArray[i];
                    frameArrayLooped[frameArray.Length + (frameArray.Length - i - 1)] = frameArray[i];
                }

                frameArray = frameArrayLooped;
            }

            this.frameArray = frameArray;
            this.duration = frameArray.Select(f => (int)f.duration).Sum();
            this.switchWhenComplete = switchName != "";
            this.switchName = switchName;
        }
    }
}