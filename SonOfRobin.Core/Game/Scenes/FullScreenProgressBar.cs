using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class FullScreenProgressBar : Scene
    {
        private readonly Texture2D backgroundTexture;

        public FullScreenProgressBar() : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            string loadingTextureName = AnimData.loadingGfxNames[SonOfRobinGame.random.Next(0, AnimData.loadingGfxNames.Count)];
            this.backgroundTexture = SonOfRobinGame.DisposableContentMgr.Load<Texture2D>($"gfx/Loading/{loadingTextureName}");
        }

        public override void Remove()
        {
            base.Remove();
            this.backgroundTexture.Dispose();
            SonOfRobinGame.DisposableContentMgr.Unload();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw()
        {
            SonOfRobinGame.GfxDev.Clear(Color.White);

            Rectangle testRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);

            Helpers.DrawTextureInsideRect(texture: this.backgroundTexture, rectangle: testRect, color: Color.White);
        }
    }
}