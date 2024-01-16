using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public abstract class ImageObj
    {
        public virtual int Width { get; }
        public virtual int Height { get; }

        public ImageObj()
        { }

        public virtual void Draw(Rectangle rect, Color color = default, Helpers.AlignX alignX = Helpers.AlignX.Center, Helpers.AlignY alignY = Helpers.AlignY.Center)
        { }
    }
}