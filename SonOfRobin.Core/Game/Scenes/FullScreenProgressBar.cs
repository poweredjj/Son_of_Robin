using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class FullScreenProgressBar : Scene
    {
        private readonly Texture2D backgroundTexture;
        private static ContentManager privateContentManager;

        public FullScreenProgressBar() : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            string loadingTextureName = AnimData.loadingGfxNames[SonOfRobinGame.random.Next(0, AnimData.loadingGfxNames.Count)];

            this.backgroundTexture = privateContentManager.Load<Texture2D>($"gfx/Loading/{loadingTextureName}");
        }

        public static void AssignContentManager(ContentManager contentManager)
        {
            privateContentManager = contentManager;
        }

        public override void Remove()
        {
            base.Remove();

            privateContentManager.Unload();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw()
        {
            SonOfRobinGame.GfxDev.Clear(Color.White);
            Rectangle testRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);
            Rectangle destRect = Helpers.DrawTextureInsideRect(texture: this.backgroundTexture, rectangle: testRect, color: Color.White);

            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientBottom, destinationRectangle: destRect, color: Color.White);
        }
    }
}