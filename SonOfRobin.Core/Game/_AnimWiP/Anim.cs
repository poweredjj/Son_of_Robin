using System.Linq;

namespace SonOfRobin
{
    public readonly struct Anim
    {
        public readonly string name;
        public readonly int size;
        public readonly AnimFrame[] frameArray;
        public readonly bool isLooped;
        public readonly int duration;
        public readonly bool switchWhenComplete;
        public readonly string switchName;

        public Anim(string name, int size, AnimFrame[] frameArray, bool isLooped, string switchName = "")
        {
            this.name = name;
            this.size = size;
            this.frameArray = frameArray;
            this.isLooped = isLooped;
            this.duration = frameArray.Select(f => (int)f.duration).Sum();
            this.switchWhenComplete = switchName != "";
            this.switchName = switchName;
        }
    }
}