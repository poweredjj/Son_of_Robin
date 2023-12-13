using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DragonBonesMG.Display
{
    public class TexturePart : IDrawableDb
    {

        private readonly Texture2D _texture;
        private readonly Rectangle _bounds;

        public TexturePart(Texture2D texture, Rectangle bounds)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            _texture = texture;
            _bounds = bounds;
        }

        /// <summary>
        /// Assumes SpriteBatch.Begin() has been called.
        /// </summary>
        public void Draw(SpriteBatch s)
        {
            // Use center of the texture as the origin
            s.Draw(_texture, -_bounds.Size.ToVector2() / 2f, _bounds, Color.White);
        }

        public void Draw(SpriteBatch s, Color color, SpriteEffects effect = SpriteEffects.None)
        {
            s.Draw(texture: _texture, position: -_bounds.Size.ToVector2() / 2f, scale: Vector2.One, sourceRectangle: _bounds, color: color, rotation: 0, origin: Vector2.Zero, effects: effect, layerDepth: 0);
        }

        public Texture2D RenderToTexture(SpriteBatch s)
        {
            var graphics = s.GraphicsDevice;
            var prevTargets = graphics.GetRenderTargets();

            var target = new RenderTarget2D(graphics, _bounds.Width, _bounds.Height);
            graphics.SetRenderTarget(target);
            s.Begin();

            s.Draw(texture: _texture, destinationRectangle: target.Bounds, color: Color.White);

            s.End();
            graphics.SetRenderTargets(prevTargets);
            return target;
        }
    }
}