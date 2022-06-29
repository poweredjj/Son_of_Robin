using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class ProgressBar : Scene
    {
        private static bool active = false;
        private static string textDisplayed = "";
        private static int currentValue = 0;
        private static int maxValue = 0;
        private static readonly SpriteFont font = SonOfRobinGame.fontMedium;
        private static SolidColor backgroundScene = null;

        private readonly static int txtMargin = 5;
        private static readonly Color defaultBgColor = Color.RoyalBlue;

        public ProgressBar() : base(inputType: InputTypes.None, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        { }

        public static void ChangeValues(int curVal, int maxVal, string text)
        {
            currentValue = curVal;
            maxValue = maxVal;
            textDisplayed = text;
            active = true;
        }

        public static void TurnOffBgColor()
        {
            backgroundScene.AddTransition(new Transition(type: Transition.TransType.In, duration: 40, scene: backgroundScene, blockInput: false, paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }, removeScene: true));
        }

        public static void TurnOnBgColor()
        {
            if (backgroundScene == null)
            { backgroundScene = new SolidColor(color: defaultBgColor, viewOpacity: 1f, clearScreen: false); }
            else
            { backgroundScene.color = defaultBgColor; }

            backgroundScene.MoveToTop();
            backgroundScene.MoveDown();
        }

        public static void Hide()
        {
            active = false;

            if (backgroundScene != null) backgroundScene.AddTransition(new Transition(type: Transition.TransType.Out, duration: 30, scene: backgroundScene, blockInput: false, paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }, removeScene: true));
        }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            if (!active) return;

            SpriteBatch spriteBatch = SonOfRobinGame.spriteBatch;

            int fullBarWidth = Convert.ToInt16(SonOfRobinGame.VirtualWidth / 3);
            int fullBarHeight = Convert.ToInt16(fullBarWidth / 15);
            int currentBarWidth = Convert.ToInt16((float)fullBarWidth * ((float)currentValue / (float)maxValue));

            Vector2 barPos = GetCenterPos(width: fullBarWidth, height: fullBarHeight);

            Vector2 mainTextSize = font.MeasureString(textDisplayed);
            Vector2 mainTxtPos = new Vector2(barPos.X, barPos.Y - (mainTextSize.Y + txtMargin));

            spriteBatch.DrawString(font, textDisplayed, mainTxtPos + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(font, textDisplayed, mainTxtPos, Color.White);

            int percentage = (int)(((float)currentValue / (float)maxValue) * 100);
            string numbersText = $"{currentValue}/{maxValue}   {percentage}%";
            Vector2 numbersTextSize = font.MeasureString(numbersText);
            Vector2 numbersTxtPos = new Vector2(Convert.ToInt32(barPos.X + ((fullBarWidth / 2)) - (numbersTextSize.X / 2)), Convert.ToInt32(barPos.Y + fullBarHeight + txtMargin));

            spriteBatch.DrawString(font, numbersText, numbersTxtPos + new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(font, numbersText, numbersTxtPos, Color.White);

            spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((int)barPos.X, (int)barPos.Y, fullBarWidth, fullBarHeight), Color.White * 0.4f);
            spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((int)barPos.X, (int)barPos.Y, currentBarWidth, fullBarHeight), Color.White);
        }

        private static Vector2 GetCenterPos(int width, int height)
        {
            return new Vector2(
                 Convert.ToInt16(SonOfRobinGame.VirtualWidth / 2 - (width / 2)),
                 Convert.ToInt16(SonOfRobinGame.VirtualHeight / 2 - (height / 2)));
        }

        private static Vector2 GetCenterPos(float width, float height)
        {
            return new Vector2(
                 Convert.ToInt16(SonOfRobinGame.VirtualWidth / 2 - (width / 2)),
                 Convert.ToInt16(SonOfRobinGame.VirtualHeight / 2 - (height / 2)));
        }

    }

}
