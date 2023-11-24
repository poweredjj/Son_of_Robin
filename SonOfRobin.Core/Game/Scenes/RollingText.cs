using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RollingText : Scene
    {
        private const int yMargin = 10;

        private readonly TextWithImages[] textList;
        private readonly Dictionary<TextWithImages, int> offsetByText = new Dictionary<TextWithImages, int>();
        private readonly Scheduler.ExecutionDelegate runAtTheEndDlgt;

        public RollingText(TextWithImages[] textList, Scheduler.ExecutionDelegate runAtTheEndDlgt = null, int priority = 1) : base(inputType: InputTypes.None, priority: priority, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.textList = textList;

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
            foreach (TextWithImages textWithImages in this.textList)
            {
                textWithImages.Update();
            }

            if (this.runAtTheEndDlgt != null) // TODO run after showing whole text
            {
                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, delay: 0, executeHelper: this.runAtTheEndDlgt, storeForLaterUse: true);
                this.Remove();
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            foreach (TextWithImages textWithImages in this.textList)
            {
                textWithImages.Draw(position: Vector2.Zero, color: Color.White, drawShadow: true, textScale: 1f);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}