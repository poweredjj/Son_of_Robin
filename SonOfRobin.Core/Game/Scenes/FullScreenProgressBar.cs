﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class FullScreenProgressBar : Scene
    {
        private static ContentManager privateContentManager;
        private static Color barAndTextColor = new Color(104, 195, 252);

        public float ProgressBarPercentage { get; private set; }
        private bool isActive;
        private string progressBarText;
        private string textureName;
        private Texture2D texture;

        public FullScreenProgressBar() : base(inputType: InputTypes.None, priority: 0, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.TurnOff(force: true);
        }

        public static void AssignContentManager(ContentManager contentManager)
        {
            privateContentManager = contentManager;
        }

        public void TurnOn(string textureName = null, float percentage = -1f, string text = null)
        {
            this.blocksDrawsBelow = true;

            if (!this.isActive && textureName == null) textureName = AnimData.loadingGfxNames[SonOfRobinGame.random.Next(0, AnimData.loadingGfxNames.Count)];
            if (textureName != null) this.UpdateTexture(textureName);

            if (percentage != -1f)
            {
                if (percentage < 0 || percentage > 1) throw new ArgumentException($"Invalid percentage value - {percentage}.");
                this.ProgressBarPercentage = percentage;
            }

            if (text != null) this.progressBarText = text;

            this.isActive = true;
        }

        public void TurnOff(bool force = false)
        {
            if (!this.isActive && !force) return;

            this.blocksDrawsBelow = false;

            this.textureName = "";
            if (this.texture != null)
            {
                this.texture.Dispose();
                this.texture = null;
                privateContentManager.Unload(); // will unload every texture in privateContentManager
            }
            this.ProgressBarPercentage = 0.005f;
            this.progressBarText = "";

            this.isActive = false;
        }

        private void UpdateTexture(string textureName)
        {
            if (this.textureName == textureName) return;

            privateContentManager.Unload(); // will unload every texture in privateContentManager

            this.texture = privateContentManager.Load<Texture2D>($"gfx/Loading/{textureName}");
            this.textureName = textureName;
        }

        public void AdvancePercentageALittle()
        {
            float percentageLeft = 1f - this.ProgressBarPercentage;
            float multiplier = percentageLeft > 0.5f ? 0.02f : 0.04f;
            this.ProgressBarPercentage += percentageLeft * multiplier;
        }

        public static float CalculatePercentage(int currentLocalStep, int totalLocalSteps, int currentGlobalStep, int totalGlobalSteps)
        {
            if (currentLocalStep < 0) throw new ArgumentException($"CurrentStep cannot be negative - {currentLocalStep}.");
            if (totalLocalSteps < 0) throw new ArgumentException($"TotalLocalSteps cannot be negative - {totalLocalSteps}.");
            if (totalLocalSteps < currentLocalStep) throw new ArgumentException($"TotalLocalSteps {totalLocalSteps} cannot be less than currentLocalStep {currentLocalStep}.");

            if (currentGlobalStep < 0) throw new ArgumentException($"CurrentGlobalStep cannot be negative - {currentGlobalStep}.");
            if (totalGlobalSteps < 0) throw new ArgumentException($"TotalGlobalSteps cannot be negative - {totalGlobalSteps}.");
            if (totalGlobalSteps < currentGlobalStep) throw new ArgumentException($"TotalGlobalSteps {totalGlobalSteps} cannot be less than currentGlobalStep {currentGlobalStep}.");

            float globalPercentage = 0;
            float localPercentage = 0;

            try { globalPercentage = (float)currentGlobalStep / (float)totalGlobalSteps; } catch (DivideByZeroException) { }
            try { localPercentage = (float)currentLocalStep / (float)totalLocalSteps; } catch (DivideByZeroException) { }

            float localPercentageDivided = localPercentage;
            try { localPercentageDivided = localPercentage / (float)totalGlobalSteps; } catch (DivideByZeroException) { }

            return globalPercentage + localPercentageDivided;
        }

        public override void Remove()
        {
            this.TurnOff();
            base.Remove();
        }

        public override void Update(GameTime gameTime)
        {
            if (!this.isActive) return;
        }

        public override void Draw()
        {
            if (!this.isActive) return;

            Color bgColor = new Color(252, 252, 246);

            SonOfRobinGame.GfxDev.Clear(bgColor);

            int progressRectHeight = (int)(SonOfRobinGame.VirtualHeight * 0.17f);

            Rectangle imageRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight - progressRectHeight);
            Rectangle progressRect = new Rectangle(x: 0, y: imageRect.Height, width: SonOfRobinGame.VirtualWidth, height: progressRectHeight);
            progressRect.Offset(0, -progressRectHeight * 0.35f); // should be slightly above the "correct" position

            Rectangle offScreenImageRect = imageRect; // imageRect wider than the screen
            offScreenImageRect.Inflate(offScreenImageRect.Width, 0);

            Rectangle realImageRect = Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: offScreenImageRect, color: Color.White * 0.6f);
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
                width: progressRect.Width, height: (int)(progressRect.Height * 0.25f));

            if (SonOfRobinGame.platform == Platform.Desktop) textRect.Inflate(0, -textRect.Height * 0.22f);
            if (this.progressBarText.Contains("\n"))
            {
                int expansion = textRect.Height;
                textRect.Height += expansion;
                textRect.Y -= expansion;
            }

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