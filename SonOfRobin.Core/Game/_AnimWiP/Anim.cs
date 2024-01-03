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

        public Anim(AnimPkg animPkg, int size, AnimFrameNew[] frameArray, string name = "default", bool pingPong = false, string switchName = "")
        {
            this.animPkg = animPkg;
            this.name = name;
            this.size = size;
            this.looped = !frameArray.Any(obj => obj.duration == 0);
            this.pingPong = pingPong;

            if (pingPong)
            {
                AnimFrameNew[] frameArrayPingPong = new AnimFrameNew[frameArray.Length * 2];

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
    }
}