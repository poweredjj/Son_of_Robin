using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class AnimFrameObj : ImageObj
    {
        private readonly AnimFrameNew animFrame;
        public override int Width { get => animFrame.cropRect.Width; }
        public override int Height { get => animFrame.cropRect.Height; }

        public AnimFrameObj(AnimFrameNew animFrame) : base()
        {
            this.animFrame = animFrame;
        }

        public override void Draw(Rectangle rect, Color color = default, Helpers.AlignX alignX = Helpers.AlignX.Center, Helpers.AlignY alignY = Helpers.AlignY.Center)
        {
            if (color == default) color = Color.White;

            Helpers.DrawTextureInsideRect(texture: this.animFrame.Texture, rectangle: rect, srcRect: this.animFrame.cropRect, color: color, alignX: alignX, alignY: alignY, drawTestRect: false);
        }
    }
}