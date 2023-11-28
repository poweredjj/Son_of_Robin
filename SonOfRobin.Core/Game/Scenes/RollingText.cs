using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RollingText : Scene
    {
        private const int yMargin = 10;

        private readonly SolidColor solidColorBg;
        private readonly int bgFramesCount;
        private readonly int scrollEveryNthFrame;
        private readonly bool canBeSkipped;

        private readonly List<TextWithImages> remainingTextList;
        private readonly Dictionary<TextWithImages, int> offsetByText = new();

        private readonly Scheduler.ExecutionDelegate runAtTheEndDlgt;

        private readonly float offsetPercentX;

        private int currentOffsetY;

        public RollingText(List<TextWithImages> textList, Color bgColor, float offsetPercentX = 0, int scrollEveryNthFrame = 1, int bgFramesCount = 60 * 2, Scheduler.ExecutionDelegate runAtTheEndDlgt = null, bool canBeSkipped = false, int priority = 1) :

            base(inputType: InputTypes.Normal, priority: priority, alwaysUpdates: false, alwaysDraws: false, touchLayout: canBeSkipped ? TouchLayout.TextWindowCancel : TouchLayout.Empty, tipsLayout: canBeSkipped ? ControlTips.TipsLayout.TextWindowCancel : ControlTips.TipsLayout.Empty)
        {
            this.bgFramesCount = bgFramesCount;
            this.solidColorBg = new SolidColor(color: bgColor, viewOpacity: 0f);
            this.solidColorBg.transManager.AddTransition(new Transition(transManager: this.solidColorBg.transManager, outTrans: true, startDelay: 0, duration: this.bgFramesCount, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: 1.0f, endCopyToBase: true));

            this.MoveAboveScene(this.solidColorBg);

            this.remainingTextList = textList;
            this.currentOffsetY = -this.bgFramesCount;
            this.offsetPercentX = offsetPercentX;
            this.scrollEveryNthFrame = scrollEveryNthFrame;
            this.canBeSkipped = canBeSkipped;

            this.offsetByText = new Dictionary<TextWithImages, int>();
            int currentOffsetY = SonOfRobinGame.VirtualHeight;
            foreach (TextWithImages textWithImages in textList)
            {
                this.offsetByText[textWithImages] = currentOffsetY;
                currentOffsetY += textWithImages.textHeight + yMargin;
            }

            this.runAtTheEndDlgt = runAtTheEndDlgt;
        }

        public override void Update()
        {
            if (SonOfRobinGame.CurrentUpdate % this.scrollEveryNthFrame == 0) this.currentOffsetY++;

            foreach (TextWithImages textWithImages in this.remainingTextList)
            {
                textWithImages.Update();
            }

            this.DeleteDisplayedTexts();

            bool executeSkip = this.canBeSkipped && InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip);

            if (this.remainingTextList.Count == 0 || executeSkip)
            {
                this.solidColorBg.transManager.AddTransition(new Transition(transManager: this.solidColorBg.transManager, outTrans: true, startDelay: 0, duration: this.bgFramesCount, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: 0f, endRemoveScene: true));

                if (this.runAtTheEndDlgt != null) new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: this.bgFramesCount, executeHelper: this.runAtTheEndDlgt);

                this.Remove();
            }
        }

        private void DeleteDisplayedTexts()
        {
            var textsToRemove = new List<TextWithImages>();

            foreach (TextWithImages textWithImages in this.remainingTextList)
            {
                int maxTextY = this.offsetByText[textWithImages] + textWithImages.textHeight;
                bool textIsNoLongerDisplayed = maxTextY < this.currentOffsetY;

                if (textIsNoLongerDisplayed) textsToRemove.Add(textWithImages);
                else break;
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
                int offsetX = (int)(SonOfRobinGame.VirtualWidth * this.offsetPercentX);

                Vector2 textPos = new(
                    x: (SonOfRobinGame.VirtualWidth / 2) - (textWithImages.textWidth / 2) + offsetX,
                    y: this.offsetByText[textWithImages] - this.currentOffsetY);

                textWithImages.Draw(position: textPos, color: Color.White, drawShadow: true, shadowColor: Color.Black * 0.85f, textScale: 1f);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}