using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public abstract class ImageObj
    {
        private readonly int id;
        public virtual int Width { get; }
        public virtual int Height { get; }

        public bool Equals(ImageObj otherImgObj)
        {
            return this.id == otherImgObj.id;
        }

        public ImageObj(int id)
        {
            this.id = id;
        }

        public virtual void DrawInsideRect(Rectangle rect, Color color = default, Helpers.AlignX alignX = Helpers.AlignX.Center, Helpers.AlignY alignY = Helpers.AlignY.Center, bool drawTestRect = false)
        { }
    }
}