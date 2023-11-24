using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RollingText : Scene
    {
        private const int yMargin = 10;

        private readonly Dictionary<TextWithImages, int> offsetByText = new();
        private readonly Scheduler.ExecutionDelegate runAtTheEndDlgt;

        private readonly List<TextWithImages> remainingTextList;

        private int currentOffsetY;

        public RollingText(List<TextWithImages> textList, Scheduler.ExecutionDelegate runAtTheEndDlgt = null, int priority = 1) : base(inputType: InputTypes.None, priority: priority, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.remainingTextList = textList;
            this.currentOffsetY = SonOfRobinGame.VirtualHeight;

            this.offsetByText = new Dictionary<TextWithImages, int>();
            int currentOffsetY = 0;
            foreach (TextWithImages textWithImages in textList)
            {
                this.offsetByText[textWithImages] = currentOffsetY;
                currentOffsetY += textWithImages.textHeight + yMargin;
            }

            this.runAtTheEndDlgt = runAtTheEndDlgt;
        }

        public override void Update()
        {
            this.currentOffsetY--;

            foreach (TextWithImages textWithImages in this.remainingTextList)
            {
                textWithImages.Update();
            }

            this.DeleteDisplayedTexts();

            if (this.remainingTextList.Count == 0)
            {
                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: this.runAtTheEndDlgt, storeForLaterUse: true);
                this.Remove();
            }
        }

        private void DeleteDisplayedTexts()
        {
            int minY = this.currentOffsetY - SonOfRobinGame.VirtualHeight;

            var textsToRemove = new List<TextWithImages>();

            foreach (TextWithImages textWithImages in this.remainingTextList)
            {
                int minTextY = this.offsetByText[textWithImages];
                int maxTextY = minTextY + textWithImages.textHeight;

                if (minTextY < minY && maxTextY < minY) textsToRemove.Add(textWithImages);
            }

            foreach (TextWithImages textWithImages in textsToRemove)
            {
                this.remainingTextList.Remove(textWithImages);
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            foreach (TextWithImages textWithImages in this.remainingTextList)
            {
                Vector2 textPos = new Vector2(
                    x: (SonOfRobinGame.VirtualWidth / 2) - (textWithImages.textWidth / 2),
                    y: this.currentOffsetY + this.offsetByText[textWithImages]);

                textWithImages.Draw(position: textPos, color: Color.White, drawShadow: true, textScale: 1f);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}