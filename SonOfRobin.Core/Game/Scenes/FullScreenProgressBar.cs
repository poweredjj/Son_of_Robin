using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class FullScreenProgressBar : Scene
    {
        private static readonly List<string> loadingGfxNames = Enumerable.Range(1, 38) // to be loaded on demand
            .Select(i => $"loading_{i}") // numbering scheme
            .Concat(new List<string> { }) // "original" names can be placed here
            .ToList();

        private static readonly Color bgColor = new(252, 252, 246);
        private static readonly Color primaryTextColor = new(105, 215, 252);
        private static readonly Color optionalTextColor = new(156, 181, 252);
        public float ProgressBarPercentage { get; private set; }
        private readonly SpriteFontBase font;
        private bool isActive;
        private string progressBarText;
        private string optionalText;
        private string textureName;
        private Texture2D texture;
        private Texture2D loadingWheel;
        private float loadingWheelRotation;

        public FullScreenProgressBar() : base(inputType: InputTypes.None, priority: 0, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.TurnOff(force: true);

            this.font = SonOfRobinGame.FontFreeSansBold.GetFont(24);

            this.loadingWheel = TextureBank.GetTexture(TextureBank.TextureName.LoadingWheel);
            this.loadingWheelRotation = 0;
        }

        public void TurnOn(string text, string optionalText = null, string textureName = null, float percentage = -1f)
        {
            this.blocksDrawsBelow = true;

            if (!this.isActive && textureName == null) textureName = loadingGfxNames[SonOfRobinGame.random.Next(loadingGfxNames.Count)];
            if (textureName != null) this.UpdateTexture(textureName);

            if (percentage != -1f)
            {
                if (percentage < 0 || percentage > 1) throw new ArgumentException($"Invalid percentage value - {percentage}.");
                this.ProgressBarPercentage = percentage;
            }

            this.progressBarText = text;
            if (text.Contains("\n")) throw new ArgumentException($"ProgressBarText cannot contain newline - '{this.progressBarText}'.");

            this.optionalText = optionalText;
            if (this.optionalText != null && this.optionalText.Contains("\n")) throw new ArgumentException($"OptionalText cannot contain newline - '{this.optionalText}'.");

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
                TextureBank.FlushTemporaryTextures();
            }
            this.ProgressBarPercentage = 0.005f;
            this.progressBarText = "";
            this.optionalText = null;
            this.loadingWheelRotation = 0;

            this.isActive = false;

            LoadingTips.ChangeTip();
        }

        private void UpdateTexture(string textureName)
        {
            if (this.textureName == textureName) return;

            TextureBank.FlushTemporaryTextures();

            this.texture = TextureBank.GetTexture(fileName: $"Loading/{textureName}", persistent: false);
            this.textureName = textureName;
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

        public override void Update()
        {
            if (!this.isActive) return;
        }

        public override void Draw()
        {
            if (!this.isActive) return;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            SonOfRobinGame.GfxDev.Clear(bgColor);

            int progressRectHeight = (int)(SonOfRobinGame.ScreenHeight * 0.17f);
            Rectangle imageRect = new Rectangle(x: 0, y: 0, width: SonOfRobinGame.ScreenWidth, height: SonOfRobinGame.ScreenHeight - progressRectHeight);
            Rectangle progressRect = new Rectangle(x: 0, y: imageRect.Height, width: SonOfRobinGame.ScreenWidth, height: progressRectHeight);
            progressRect.Offset(0, -progressRectHeight * 0.35f); // should be slightly above the "correct" position

            int wheelRectHeight = (int)(progressRectHeight * 0.6f);
            int wheelRectWidth = (int)(progressRectHeight * 0.75f);
            Rectangle wheelRect = new Rectangle(x: progressRect.X + progressRect.Width - wheelRectWidth, y: 0, width: wheelRectWidth, height: wheelRectHeight); // y to be set below
            wheelRect.Offset(0, (int)(wheelRect.Height * 0.25f));
            wheelRect.Height -= (int)(wheelRect.Height * 0.25f);
            progressRect.Width -= wheelRect.Width;

            Rectangle offScreenImageRect = imageRect; // imageRect wider than the screen
            offScreenImageRect.Inflate(offScreenImageRect.Width, 0);

            Rectangle realImageRect = Helpers.DrawTextureInsideRect(texture: this.texture, rectangle: offScreenImageRect, color: Color.White * 1f);
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

            Rectangle textRect = new Rectangle(
                x: progressRect.X + wheelRect.Width, y: progressRect.Y + (int)(progressRect.Height * 0.36f),
                width: progressRect.Width - wheelRect.Width, height: (int)(progressRect.Height * 0.25f));

            if (SonOfRobinGame.platform == Platform.Desktop) textRect.Inflate(0, -textRect.Height * 0.22f);

            textRect.Inflate(-textRect.Width * 0.05f, 0);

            Color barColor = primaryTextColor;

            if (this.optionalText != null)
            {
                barColor = optionalTextColor;

                Rectangle optionalTextRect = textRect;
                textRect.Offset(0, -textRect.Height); // main text should be moved up
                optionalTextRect.Offset(0, textRect.Height / 3);

                Helpers.DrawTextInsideRect(font: font, text: this.optionalText, rectangle: optionalTextRect, color: optionalTextColor, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Bottom);

                // Helpers.DrawRectangleOutline(rect: optionalTextRect, color: Color.Cyan, borderWidth: 1); // for testing
            }

            wheelRect.Y = textRect.Bottom - textRect.Height;

            Rectangle progressBarRect = new Rectangle(
                x: progressRect.X, y: progressRect.Y + (progressRect.Height / 2),
                width: progressRect.Width, height: progressRect.Height / 2);

            progressBarRect.Inflate(-progressBarRect.Width * 0.05f, -progressBarRect.Height * 0.43f);
            progressBarRect.Width = (int)(progressBarRect.Width * this.ProgressBarPercentage);

            Helpers.DrawTextInsideRect(font: font, text: this.progressBarText, rectangle: textRect, color: primaryTextColor, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Bottom);

            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: progressBarRect, color: barColor);

            Helpers.DrawTextureInsideRect(texture: this.loadingWheel, rectangle: wheelRect, color: Color.White * 0.8f, rotation: this.loadingWheelRotation, drawTestRect: false);
            this.loadingWheelRotation += 0.02f;

            //Helpers.DrawRectangleOutline(rect: progressRect, color: Color.Green, borderWidth: 1); // for testing
            //Helpers.DrawRectangleOutline(rect: bottomGradRect, color: Color.Pink, borderWidth: 1); // for testing
            //Helpers.DrawRectangleOutline(rect: textRect, color: Color.Blue, borderWidth: 1); // for testing
            //Helpers.DrawRectangleOutline(rect: progressBarRect, color: Color.Cyan, borderWidth: 1); // for testing
            //Helpers.DrawRectangleOutline(rect: wheelRect, color: Color.Lime, borderWidth: 1); // for testing

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}