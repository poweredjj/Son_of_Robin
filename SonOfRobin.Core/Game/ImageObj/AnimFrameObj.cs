using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class AnimFrameObj : ImageObj
    {
        private readonly AnimFrame animFrame;
        public override int Width { get => this.animFrame.cropRect.Width; }
        public override int Height { get => this.animFrame.cropRect.Height; }

        public AnimFrameObj(AnimFrame animFrame) : base(id: animFrame.GetHashCode())
        {
            this.animFrame = animFrame;
        }

        public override void DrawInsideRect(Rectangle rect, Color color = default, Helpers.AlignX alignX = Helpers.AlignX.Center, Helpers.AlignY alignY = Helpers.AlignY.Center, bool drawTestRect = false)
        {
            if (color == default) color = Color.White;

            this.animFrame.DrawInsideRect(rect: rect, color: color, alignX: alignX, alignY: alignY, drawTestRect);
        }
    }
}