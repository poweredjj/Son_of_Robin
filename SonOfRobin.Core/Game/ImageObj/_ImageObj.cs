using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public abstract class ImageObj
    {
        public virtual int ID { get; }
        public virtual int Width { get; }
        public virtual int Height { get; }

        public bool Equals(ImageObj otherImgObj)
        {
            return this.ID == otherImgObj.ID;
        }

        public ImageObj()
        { }

        public virtual void DrawInsideRect(Rectangle rect, Color color = default, Helpers.AlignX alignX = Helpers.AlignX.Center, Helpers.AlignY alignY = Helpers.AlignY.Center)
        { }
    }
}