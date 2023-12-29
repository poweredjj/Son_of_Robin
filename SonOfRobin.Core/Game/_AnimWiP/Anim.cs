using System.Linq;

namespace SonOfRobin
{
    public readonly struct Anim
    {
        public readonly string name;
        public readonly int size;
        public readonly bool looped;
        public readonly bool pingPong;
        public readonly int duration;
        public readonly bool switchWhenComplete;
        public readonly string switchName;
        public readonly AnimFrame[] frameArray;

        public Anim(string name, int size, AnimFrame[] frameArray, bool looped, bool pingPong, string switchName = "")
        {
            this.name = name;
            this.size = size;
            this.looped = looped;
            this.pingPong = pingPong;

            if (looped)
            {
                AnimFrame[] frameArrayLooped = new AnimFrame[frameArray.Length * 2];

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