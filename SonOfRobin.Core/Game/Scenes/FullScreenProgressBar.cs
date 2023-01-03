using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class FullScreenProgressBar : Scene
    {
        private static ContentManager privateContentManager;
        private static Color barAndTextColor = new Color(104, 195, 252);

        private Texture2D backgroundTexture;
        public float ProgressBarPercentage { get; private set; }

        private string progressBarText;

        public FullScreenProgressBar(string textureName = null) : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty, blocksDrawsBelow: true)
        {
            if (textureName == null) textureName = AnimData.loadingGfxNames[SonOfRobinGame.random.Next(0, AnimData.loadingGfxNames.Count)];

            this.UpdateTexture(textureName);

            this.ProgressBarPercentage = 0.01f;
            this.progressBarText = "";
        }

        public static void AssignContentManager(ContentManager contentManager)
        {
            privateContentManager = contentManager;
        }

        public void UpdateProgressBar(float percentage, string text)
        {
            if (percentage < 0 || percentage > 1) throw new ArgumentException($"Invalid percentage value - {percentage}.");
            this.ProgressBarPercentage = percentage;

            if (text != null) this.progressBarText = text;
        }

        public void UpdateTexture(string textureName)
        {
            privateContentManager.Unload(); // will unload every texture in privateContentManager

            this.backgroundTexture = privateContentManager.Load<Texture2D>($"gfx/Loading/{textureName}");
        }

        public override void Remove()
        {
            base.Remove();

            privateContentManager.Unload(); // will unload every texture in privateContentManager
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            Color bgColor = new Color(252, 252, 246);

            SonOfRobinGame.GfxDev.Clear(bgColor);

            int progressRectHeight = (int)(SonOfRobinGame.VirtualHeight * 0.17f);

            Rectangle imageRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight - progressRectHeight);
            Rectangle progressRect = new Rectangle(x: 0, y: imageRect.Height, width: SonOfRobinGame.VirtualWidth, height: progressRectHeight);
            progressRect.Offset(0, -progressRectHeight * 0.35f); // should be slightly above the "correct" position

            Rectangle offScreenImageRect = imageRect; // imageRect wider than the screen
            offScreenImageRect.Inflate(offScreenImageRect.Width, 0);

            Rectangle realImageRect = Helpers.DrawTextureInsideRect(texture: this.backgroundTexture, rectangle: offScreenImageRect, color: Color.White * 0.6f);
            Rectangle bottomGradRect = realImageRect;

            int bottomGradRectHeight = (int)(bottomGradRect.Height * 0.95);
            int bottomGradRectOffset = bottomGradRect.Height - bottomGradRectHeight;
            bottomGradRect.Offset(0, bottomGradRectOffset);
            bottomGradRect.Height = bottomGradRectHeight;

            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientBottom, destinationRectangle: bottomGradRect, color: bgColor);

            if (realImageRect.Width < imageRect.Width)
            {
                // adding gradients on the sides, if background is smaller than the screen
                int gradWidth = realImageRect.Width / 3;

                Rectangle leftGradRect = new Rectangle(x: realImageRect.X, y: realImageRect.Y, width: gradWidth, height: realImageRect.Height);
                Rectangle rightGradRect = new Rectangle(x: realImageRect.X + (realImageRect.Width - gradWidth), y: realImageRect.Y, width: gradWidth, height: realImageRect.Height);

                SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientLeft, destinationRectangle: leftGradRect, color: bgColor);
                SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.GradientRight, destinationRectangle: rightGradRect, color: bgColor);
            }

            // SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: progressRect, color: Color.Brown * 0.25f); // for testing

            Rectangle textRect = new Rectangle(
                x: progressRect.X, y: progressRect.Y + (int)(progressRect.Height * 0.32f),
                width: progressRect.Width, height: (int)(progressRect.Height * 0.22f));

            textRect.Inflate(-textRect.Width * 0.05f, 0);

            Rectangle progressBarRect = new Rectangle(
                x: progressRect.X, y: progressRect.Y + (progressRect.Height / 2),
                width: progressRect.Width, height: progressRect.Height / 2);

            progressBarRect.Inflate(-progressBarRect.Width * 0.05f, -progressBarRect.Height * 0.43f);
            progressBarRect.Width = (int)(progressBarRect.Width * this.ProgressBarPercentage);

            // Helpers.DrawRectangleOutline(rect: textRect, color: Color.Blue, borderWidth: 1); // for testing
            // Helpers.DrawRectangleOutline(rect: progressBarRect, color: Color.Cyan, borderWidth: 1); // for testing

            Helpers.DrawTextInsideRect(font: SonOfRobinGame.FontFreeSansBold24, text: this.progressBarText, rectangle: textRect, color: barAndTextColor, alignX: Helpers.AlignX.Left, alignY: Helpers.AlignY.Bottom);

            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: progressBarRect, color: barAndTextColor);
        }
    }
}