using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class TextureObj : ImageObj
    {
        private readonly Texture2D texture;

        public override int ID { get => this.texture.GetHashCode(); }
        public override int Width { get => this.texture.Width; }
        public override int Height { get => this.texture.Height; }

        public TextureObj(Texture2D texture) : base()
        {
            this.texture = texture;
        }

        public override void DrawInsideRect(Rectangle rect, Color color = default, Helpers.AlignX alignX = Helpers.AlignX.Center, Helpers.AlignY alignY = Helpers.AlignY.Center)
        {
            if (color == default) color = Color.White;

            Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: rect, color: color, alignX: alignX, alignY: alignY);
        }
    }
}