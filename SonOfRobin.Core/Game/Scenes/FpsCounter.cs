using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FpsCounter : Scene
    {
        public struct FpsValueGroup
        {
            public readonly int fps;
            public readonly int length;

            public FpsValueGroup(int fps, int length)
            {
                this.fps = fps;
                this.length = length;
            }
        }

        public class FpsHistory
        {
            public int StoreCapacity { get; set; }
            private readonly List<int> recordList;

            public List<FpsValueGroup> FPSValueGroupList
            {
                get
                {
                    var fpsValueGroupList = new List<FpsValueGroup>();

                    int lastVal = -1;
                    int length = 1;
                    int listCount = this.recordList.Count;

                    for (int i = 0; i < listCount; i++)
                    {
                        int record = this.recordList[i];

                        if (record == lastVal && i < listCount - 1) length++;
                        else
                        {
                            if (lastVal != -1) fpsValueGroupList.Add(new FpsValueGroup(fps: lastVal, length: length));

                            lastVal = record;
                            length = 1;
                        }
                    }

                    return fpsValueGroupList;
                }
            }

            public FpsHistory(int storedValuesCount)
            {
                this.StoreCapacity = storedValuesCount;
                this.recordList = new List<int>();
            }

            public void AddRecord(int fps)
            {
                this.recordList.Add(fps);
                while (true)
                {
                    if (this.recordList.Count > this.StoreCapacity) this.recordList.RemoveAt(0);
                    else break;
                }
            }
        }

        private Rectangle bgRect;
        private Rectangle textRect;
        private Rectangle graphRect;
        private static readonly int outerMargin = SonOfRobinGame.platform == Platform.Mobile ? 14 : 6;
        private const int innerMargin = 3;

        private static readonly Dictionary<int, Color> colorDict = new Dictionary<int, Color> {
            { 40, Color.White },
            { 30, Color.Yellow },
            { 15, Color.Orange },
            { -1, Color.Red },
            };

        private FpsHistory fpsHistory;
        private int counterValue;
        private string CounterText { get { return $"FPS {this.counterValue}"; } }

        private Color CounterColor
        {
            get
            {
                int counterValue = this.counterValue;

                foreach (var kvp in colorDict)
                {
                    if (counterValue > kvp.Key) return kvp.Value;
                }

                throw new ArgumentException($"Cannot calculate FPS counter color for value {counterValue}.");
            }
        }

        public FpsCounter() : base(inputType: InputTypes.None, priority: -2, blocksUpdatesBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.fpsHistory = new FpsHistory(Preferences.FpsCounterGraphLength);
            this.AdaptToNewSize();
            this.transManager.AddTransition(this.GetTransition(inTrans: true));
        }

        public void ResizeFpsHistory(int newStoredValuesCount)
        {
            if (newStoredValuesCount < 1) throw new ArgumentException($"FPS history stored values count {newStoredValuesCount} cannot be less than 1.");

            this.fpsHistory.StoreCapacity = newStoredValuesCount;
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding) this.transManager.AddTransition(this.GetTransition(inTrans: false));
            else base.Remove();
        }

        public override void Update(GameTime gameTime)
        {
            this.counterValue = (int)SonOfRobinGame.fps.FPS;
            if (Preferences.FpsCounterShowGraph) this.fpsHistory.AddRecord(this.counterValue);
        }

        private Transition GetTransition(bool inTrans)
        {
            return new Transition(
                transManager: this.transManager,
                outTrans: !inTrans,
                baseParamName: "PosY",
                targetVal: -this.viewParams.Height,
                duration: 12,
                endRemoveScene: !inTrans
                );
        }

        protected override void AdaptToNewSize()
        {
            int width = (int)(SonOfRobinGame.VirtualWidth * 0.03f);
            width = Math.Max(width, 40);
            int height = (int)(width * 0.3);

            this.textRect = new Rectangle(x: 0, y: 0, width: width, height: height);
            this.graphRect = new Rectangle(x: this.textRect.X, y: this.textRect.Y, width: Preferences.FpsCounterShowGraph ? this.fpsHistory.StoreCapacity : 0, height: this.textRect.Height);
            this.textRect.X += this.graphRect.Width;

            this.bgRect = new Rectangle(x: this.graphRect.X, y: this.graphRect.Y, width: textRect.Width + graphRect.Width, height: this.graphRect.Height);

            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = textRect.Height;

            if (Preferences.FpsCounterPosRight) this.viewParams.PutViewAtTheRight(margin: outerMargin);
            else this.viewParams.PutViewAtTheLeft(margin: outerMargin);

            this.viewParams.PutViewAtTheTop(margin: outerMargin);

            bgRect.Width += innerMargin + outerMargin;
            bgRect.Height += innerMargin + outerMargin;
            bgRect.X -= Preferences.FpsCounterPosRight ? innerMargin : outerMargin;
            bgRect.Y -= outerMargin;
        }

        public override void Draw()
        {
            // background

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.bgRect, Color.Black * 0.6f * this.viewParams.drawOpacity);

            // text

            Helpers.DrawTextInsideRect(font: SonOfRobinGame.FontFreeSansBold24, text: this.CounterText, rectangle: this.textRect, color: this.CounterColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);

            // graph

            if (Preferences.FpsCounterShowGraph)
            {
                int oneEntryWidth = this.graphRect.Width / this.fpsHistory.StoreCapacity;

                int recordCounter = 0;
                int lastYOffset = 0;
                foreach (FpsValueGroup fpsValueGroup in this.fpsHistory.FPSValueGroupList)
                {
                    double percentage = (double)fpsValueGroup.fps / 60d;
                    int barYOffset = this.graphRect.Height - ((int)(this.graphRect.Height * percentage));

                    int minOffset = Math.Min(barYOffset, lastYOffset);
                    int maxOffset = Math.Max(barYOffset, lastYOffset);

                    // vertical line - connecting 2 different values
                    Rectangle graphBarVertRect = new Rectangle(x: this.graphRect.X + (recordCounter * oneEntryWidth), y: this.graphRect.Y + minOffset, width: oneEntryWidth, height: maxOffset - minOffset + 1);
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, graphBarVertRect, Color.White * this.viewParams.drawOpacity);

                    // horizontal line - connecting the same aggregated values
                    Rectangle graphBarHorizRect = new Rectangle(x: this.graphRect.X + (recordCounter * oneEntryWidth), y: this.graphRect.Y + barYOffset, width: oneEntryWidth * fpsValueGroup.length, height: 1);
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, graphBarHorizRect, Color.White * this.viewParams.drawOpacity);

                    recordCounter += fpsValueGroup.length;
                    lastYOffset = barYOffset;
                }
            }

        }
    }
}
