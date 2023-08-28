using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class InfoWindow : Scene
    {
        public struct TextEntry
        {
            public enum Justify : byte
            {
                Left = 0,
                Center = 1,
                Right = 2,
            };

            public int Width { get { return (int)(this.textWithImages.textWidth * this.scale); } }
            public int Height { get { return (int)(this.textWithImages.textHeight * this.scale); } }

            public readonly Color color;
            public readonly TextWithImages textWithImages;
            public readonly float scale;
            public readonly int progressCurrentVal;
            public readonly int progressMaxVal;
            public readonly bool progressBarMode;
            public readonly Justify justify;

            public TextEntry(Color color, float scale = 1f, string text = "", int progressCurrentVal = -1, int progressMaxVal = -1, Justify justify = Justify.Left, List<Texture2D> imageList = null, bool animate = false, int framesPerChar = 0, int charsPerFrame = 1)
            {
                this.progressBarMode = progressCurrentVal > -1 && progressMaxVal > -1;

                if (this.progressBarMode) text = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

                this.textWithImages = new TextWithImages(font: font, text: text, imageList: imageList, animate: animate, framesPerChar: framesPerChar, charsPerFrame: charsPerFrame);

                this.justify = justify;
                this.color = color;
                this.scale = scale;
                this.progressCurrentVal = progressCurrentVal;
                this.progressMaxVal = progressMaxVal;

                if (this.progressCurrentVal > this.progressMaxVal) throw new ArgumentException($"Progress bar current value ({this.progressCurrentVal}) is greater than maximum value ({this.progressMaxVal}).");
            }

            public void Update()
            {
                this.textWithImages.Update();
            }

            public void ResetAnim()
            {
                this.textWithImages.ResetAnim();
            }

            private bool Equals(TextEntry entryToCompare)
            {
                return entryToCompare.color == this.color &&
                       entryToCompare.scale == this.scale &&
                       entryToCompare.textWithImages.Equals(this.textWithImages) &&
                       entryToCompare.progressBarMode == this.progressBarMode &&
                       entryToCompare.progressCurrentVal == this.progressCurrentVal &&
                       entryToCompare.progressMaxVal == this.progressMaxVal &&
                       entryToCompare.justify == this.justify;
            }

            public static bool CompareTwoLists(List<TextEntry> list1, List<TextEntry> list2)
            {
                if (list1.Count != list2.Count) return false;

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!list1[i].Equals(list2[i])) return false;
                }

                return true;
            }
        }

        private static readonly SpriteFont font = SonOfRobinGame.FontTommy40;

        private const float maxWindowWidthPercent = 0.35f;
        private const float maxWindowHeightPercent = 0.7f;
        private const float maxLineHeightPercent = 0.05f;

        private const float marginPercent = 0.025f;
        private const int transDuration = 6; // 6

        private int Margin
        {
            get
            {
                int margin = Convert.ToInt32(SonOfRobinGame.VirtualHeight * marginPercent);
                if (SonOfRobinGame.platform == Platform.Mobile) margin = (int)(margin * 1.2f);
                return margin;
            }
        }

        public readonly float bgOpacity;
        public readonly Color bgColor;
        private List<TextEntry> entryList;
        public bool isActive;

        private float GlobalScale
        {
            get
            {
                float maxWindowWidth = SonOfRobinGame.VirtualWidth * maxWindowWidthPercent;
                float maxWindowHeight = SonOfRobinGame.VirtualHeight * maxWindowHeightPercent;
                float maxLineHeight = SonOfRobinGame.VirtualHeight * maxLineHeightPercent;
                if (SonOfRobinGame.platform == Platform.Desktop) maxLineHeight *= 0.6f;

                int margin = this.Margin;

                int totalWidth = 0;
                int totalHeight = margin;
                int totalHeightWithoutMargins = 0;
                int lineCount = 0;

                foreach (TextEntry entry in this.entryList)
                {
                    totalWidth = Math.Max(totalWidth, entry.Width);
                    totalHeight += entry.Height + margin;
                    totalHeightWithoutMargins += entry.Height;
                    lineCount += entry.textWithImages.noOfLines;
                }

                float maxWindowHeightByLines = (int)((maxLineHeight + margin) * lineCount) + margin; // line counting variant - can exceed maximum window height...
                maxWindowHeight = Math.Min(maxWindowHeight, maxWindowHeightByLines); // ...so it is compared with the "real" maximum window height

                float scaleX = maxWindowWidth / totalWidth;
                float scaleY = maxWindowHeight / totalHeight;

                float scale = Math.Min(scaleX, scaleY);

                return scale;
            }
        }

        private Vector2 MaxEntrySize
        {
            get
            {
                if (this.entryList.Count == 0) return Vector2.Zero;

                float globalScale = this.GlobalScale;

                var widthList = new List<float>();
                var heightList = new List<float>();

                foreach (TextEntry entry in this.entryList)
                {
                    widthList.Add(entry.Width * globalScale);
                    heightList.Add(entry.Height * globalScale);
                }

                return new Vector2(widthList.Max(), heightList.Max());
            }
        }

        private Rectangle BgRect
        {
            get
            {
                int margin = this.Margin;
                Vector2 maxEntrySize = this.MaxEntrySize;

                float globalScale = this.GlobalScale;

                int width = (int)(maxEntrySize.X + (margin * 2));
                int height = margin;

                foreach (TextEntry entry in this.entryList)
                {
                    height += (int)((entry.Height * globalScale) + margin);
                }

                return new Rectangle(0, 0, width, height);
            }
        }

        public InfoWindow(Color bgColor, float bgOpacity = 0.9f) : base(inputType: InputTypes.None, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.isActive = false;
            this.bgOpacity = bgOpacity;
            this.bgColor = bgColor;
            this.viewParams.Opacity = 0f;
            this.entryList = new List<TextEntry> { };
        }

        public Vector2 MeasureEntries(List<TextEntry> entryList)
        {
            List<TextEntry> savedEntryList = this.entryList;
            this.entryList = entryList;
            this.UpdateViewSizes();
            Vector2 windowSize = new Vector2(this.viewParams.Width, this.viewParams.Height);

            this.entryList = savedEntryList;

            return windowSize;
        }

        private Vector2 GetCenterPosForEntryList(List<TextEntry> entryList)
        {
            var previousList = this.entryList;
            this.entryList = entryList;
            this.UpdateViewSizes();

            Vector2 centerPos = new Vector2(this.viewParams.CenterPosX, this.viewParams.CenterPosY);

            this.entryList = previousList;
            this.UpdateViewSizes();

            return centerPos;
        }

        public void TurnOn(int curVal, int maxVal, string text, bool addNumbers = true, bool addTransition = false, bool turnOffInput = false)
        {
            // simple progress bar usage

            curVal = Math.Min(maxVal, curVal);

            var entryList = new List<TextEntry> { };

            int lineCounter = 0;
            foreach (string line in text.Split('\n'))
            {
                float scale = lineCounter == 0 ? 1.3f : 1f;

                entryList.Add(new TextEntry(text: line, color: Color.White, scale: scale, justify: TextEntry.Justify.Center));
                lineCounter++;
            }

            entryList.Add(new TextEntry(color: Color.White, progressCurrentVal: curVal, progressMaxVal: maxVal, justify: TextEntry.Justify.Center));

            if (addNumbers)
            {
                int percentage = (int)((float)curVal / (float)maxVal * 100);
                entryList.Add(new TextEntry(text: $"{curVal}/{maxVal}   {percentage}%", color: Color.White, justify: TextEntry.Justify.Center));
            }

            this.TurnOn(entryList: entryList, newPosX: 0, newPosY: 0, addTransition: addTransition, centerHoriz: true, centerVert: true, turnOffInput: turnOffInput);
        }

        public void TurnOn(List<TextEntry> entryList, int newPosX, int newPosY, bool addTransition = true, bool centerHoriz = false, bool centerVert = false, bool turnOffInput = false)
        {
            // normal usage
            if (turnOffInput) this.InputType = InputTypes.Normal; // captures input and deactivates normal input in the scenes below

            if (centerHoriz || centerVert)
            {
                Vector2 centerPos = this.GetCenterPosForEntryList(entryList);

                if (centerHoriz) newPosX = Convert.ToInt32(centerPos.X);
                if (centerVert) newPosY = Convert.ToInt32(centerPos.Y);
            }

            bool listsEqual = TextEntry.CompareTwoLists(this.entryList, entryList);
            if (!listsEqual)
            {
                this.entryList = entryList;
                foreach (TextEntry textEntry in this.entryList) textEntry.ResetAnim();
            }

            if (!addTransition)
            {
                this.transManager.ClearPreviousTransitions(copyDrawToBase: false);
                this.viewParams.PosX = newPosX;
                this.viewParams.PosY = newPosY;
                this.viewParams.Opacity = 1f;
                this.isActive = true;
                return;
            }

            if (this.isActive)
            {
                int oldPosX = Convert.ToInt32(this.viewParams.PosX);
                int oldPosY = Convert.ToInt32(this.viewParams.PosY);
                int transPosX = Convert.ToInt32(this.transManager.GetTargetValue("PosX"));
                int transPosY = Convert.ToInt32(this.transManager.GetTargetValue("PosY"));
                bool posXEqual = oldPosX == newPosX || transPosX == newPosX;
                bool posYEqual = oldPosY == newPosY || transPosY == newPosY;
                if (posXEqual && posYEqual && listsEqual) return;
            }

            this.UpdateViewSizes();
            this.transManager.ClearPreviousTransitions(copyDrawToBase: true);

            var paramsToChange = new Dictionary<string, float> { };
            paramsToChange["Opacity"] = 1f;
            if (this.isActive)
            {
                paramsToChange["PosX"] = newPosX;
                paramsToChange["PosY"] = newPosY;
            }
            else
            {
                this.viewParams.PosX = newPosX;
                this.viewParams.PosY = newPosY;
            }

            this.transManager.ClearPreviousTransitions(copyDrawToBase: true);
            this.transManager.AddMultipleTransitions(paramsToChange: paramsToChange, outTrans: false, duration: transDuration, endCopyToBase: false, startSwapParams: true);

            this.isActive = true;
        }

        public void TurnOff(bool addTransition = true)
        {
            this.InputType = InputTypes.None; // restores input

            if (!this.isActive) return;

            this.transManager.ClearPreviousTransitions(copyDrawToBase: true);

            if (addTransition)
            {
                this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: true, baseParamName: "Opacity", targetVal: 0f, duration: transDuration, endCopyToBase: true));
            }
            else this.viewParams.Opacity = 0f;

            this.isActive = false;
        }

        public override void Update()
        {
            if (!this.isActive) return;

            foreach (TextEntry textEntry in this.entryList) textEntry.Update();
            this.UpdateViewSizes();
        }

        public void UpdateViewSizes()
        {
            Rectangle bgRect = this.BgRect;
            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = bgRect.Height;
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Rectangle bgRect = this.BgRect;
            int margin = this.Margin;

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bgRect, this.bgColor * this.bgOpacity * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);

            float globalScale = this.GlobalScale;
            float shadowOffset = Math.Max(4f * globalScale, 1);

            int posX = margin;
            int posY = margin;

            foreach (TextEntry entry in this.entryList)
            {
                // setting positions for current entry

                Vector2 basePos = new Vector2(posX, posY);
                Vector2 realEntrySize = new Vector2(entry.Width, entry.Height) * globalScale;

                switch (entry.justify)
                {
                    case TextEntry.Justify.Left:
                        break;

                    case TextEntry.Justify.Center:
                        basePos.X += (bgRect.Width / 2) - margin - (realEntrySize.X / 2);
                        break;

                    case TextEntry.Justify.Right:
                        basePos.X += bgRect.Width - realEntrySize.X - (margin * 2);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported justify type - '{entry.justify}'.");
                }

                Vector2 textPos = new Vector2(basePos.X, basePos.Y);

                // drawing entry

                if (entry.progressBarMode)
                {
                    int currentBarWidth = (int)(realEntrySize.X * ((float)entry.progressCurrentVal / (float)entry.progressMaxVal));

                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle((int)textPos.X, (int)textPos.Y,
                        (int)realEntrySize.X, (int)(entry.Height * globalScale)),
                        entry.color * this.viewParams.drawOpacity * 0.4f);

                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle((int)textPos.X, (int)textPos.Y,
                    currentBarWidth, (int)(entry.Height * globalScale)),
                    entry.color * this.viewParams.drawOpacity);
                }
                else
                {
                    entry.textWithImages.Draw(position: textPos, color: entry.color * this.viewParams.drawOpacity, imageOpacity: this.viewParams.drawOpacity, shadowColor: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, shadowOffset: new Vector2(shadowOffset), textScale: globalScale * entry.scale);
                }

                // Helpers.DrawRectangleOutline(rect: new Rectangle(x: (int)basePos.X, y: (int)basePos.Y, width: (int)realEntrySize.X, height: (int)realEntrySize.Y), color: Color.White, borderWidth: 1); // for testing

                posY += (int)((entry.Height * globalScale) + margin);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}