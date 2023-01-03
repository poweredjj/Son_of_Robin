using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class FullScreenProgressBar : Scene
    {
        private readonly Texture2D backgroundTexture;
        private static ContentManager privateContentManager;

        public FullScreenProgressBar(string textureName = null) : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty, blocksDrawsBelow: true)
        {
            if (textureName == null) textureName = AnimData.loadingGfxNames[SonOfRobinGame.random.Next(0, AnimData.loadingGfxNames.Count)];

            this.backgroundTexture = privateContentManager.Load<Texture2D>($"gfx/Loading/{textureName}");
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

            int progressRectHeight = (int)(SonOfRobinGame.VirtualHeight * 0.2f);

            Rectangle imageRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight - progressRectHeight);
            Rectangle progressRect = new Rectangle(x: 0, y: imageRect.Height, width: SonOfRobinGame.VirtualWidth, height: progressRectHeight);

            Rectangle offScreenImageRect = imageRect; // imageRect wider than the screen
            offScreenImageRect.Inflate(offScreenImageRect.Width, 0);

            Rectangle realImageRect = Helpers.DrawTextureInsideRect(texture: this.backgroundTexture, rectangle: offScreenImageRect, color: Color.White * 0.6f);
            Rectangle bottomGradRect = realImageRect;

            int bottomGradRectHeight = bottomGradRect.Height / 2;
            int bottomGradRectOffset = bottomGradRect.Height - bottomGradRectHeight;
            bottomGradRect.Offset(0, bottomGradRectOffset);
            bottomGradRect.Height = bottomGradRectHeight;

            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientBottom, destinationRectangle: bottomGradRect, color: Color.White);

            if (realImageRect.Width < imageRect.Width)
            {
                // adding gradients on the sides, if background is smaller than the screen
                int gradWidth = realImageRect.Width / 4;

                Rectangle leftGradRect = new Rectangle(x: realImageRect.X, y: realImageRect.Y, width: gradWidth, height: realImageRect.Height);
                Rectangle rightGradRect = new Rectangle(x: realImageRect.X + (realImageRect.Width - gradWidth), y: realImageRect.Y, width: gradWidth, height: realImageRect.Height);

                SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientLeft, destinationRectangle: leftGradRect, color: Color.White);
                SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientRight, destinationRectangle: rightGradRect, color: Color.White);
            }

            Helpers.DrawRectangleOutline(rect: progressRect, color: Color.Green, borderWidth: 1);
        }
    }
}