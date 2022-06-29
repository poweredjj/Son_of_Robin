using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class InfoWindow : Scene
    {
        public struct TextEntry
        {
            public enum Justify { Left, Center, Right };

            public readonly Color color;
            public readonly string text;
            public readonly float scale;
            public readonly int sizeX;
            public readonly int sizeY;
            public readonly AnimFrame imageFrame;
            public readonly int progressCurrentVal;
            public readonly int progressMaxVal;
            public readonly bool progressBarMode;
            public readonly Justify justify;
            public TextEntry(Color color, float scale = 1f, AnimFrame frame = null, string text = "", int progressCurrentVal = -1, int progressMaxVal = -1, Justify justify = Justify.Left)
            {
                this.progressBarMode = progressCurrentVal > -1 && progressMaxVal > -1;
                this.text = this.progressBarMode ? "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" : text;
                this.justify = justify;
                this.color = color;
                this.scale = scale;
                this.imageFrame = frame;
                this.progressCurrentVal = progressCurrentVal;
                this.progressMaxVal = progressMaxVal;

                if (this.progressCurrentVal > this.progressMaxVal) throw new ArgumentException($"Progress bar current value ({this.progressCurrentVal}) is greater than maximum value ({this.progressMaxVal}).");

                Vector2 entrySize = font.MeasureString(this.text) * this.scale;

                this.sizeX = (int)entrySize.X;
                this.sizeY = (int)entrySize.Y;
            }

            private bool Equals(TextEntry entryToCompare)
            {
                return entryToCompare.text == this.text &&
                    entryToCompare.color == this.color &&
                    entryToCompare.scale == this.scale &&
                    entryToCompare.imageFrame == this.imageFrame &&
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

        private static readonly SpriteFont font = SonOfRobinGame.fontTommy40;
        private static readonly float marginPercent = 0.018f;
        private static readonly float entryWidthPercent = 0.35f;
        private static readonly float entryHeightPercent = 0.045f;
        private static readonly int transDuration = 6; // 6

        public readonly float bgOpacity;
        public readonly Color bgColor;
        private List<TextEntry> entryList;
        public bool isActive;

        private int Margin { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * marginPercent); } }
        private float GlobalTextScale
        {
            get
            {
                float textScale;
                float minScale = 9999f;
                float maxTextWidth = SonOfRobinGame.VirtualWidth * entryWidthPercent;
                float maxTextHeight = SonOfRobinGame.VirtualHeight * entryHeightPercent;

                foreach (TextEntry entry in this.entryList)
                {
                    textScale = Math.Min(maxTextWidth / entry.sizeX, maxTextHeight / entry.sizeY);
                    if (textScale < minScale) minScale = textScale;
                }

                if (SonOfRobinGame.platform == Platform.Mobile) minScale *= 1.5f;

                return minScale;
            }
        }

        private Vector2 MaxEntrySize
        {
            get
            {
                float maxEncounteredWidth = 0f;
                float maxEncounteredHeight = 0f;
                float globalTextScale = this.GlobalTextScale;
                float textWidth, textHeight;

                foreach (TextEntry entry in this.entryList)
                {
                    textWidth = entry.sizeX * globalTextScale;
                    textHeight = entry.sizeY * globalTextScale;

                    if (entry.imageFrame != null) textWidth += Margin + textHeight;

                    if (textWidth > maxEncounteredWidth) maxEncounteredWidth = textWidth;
                    if (textHeight > maxEncounteredHeight) maxEncounteredHeight = textHeight;
                }

                return new Vector2(maxEncounteredWidth, maxEncounteredHeight);
            }
        }

        private Rectangle BgRect
        {
            get
            {
                int margin = this.Margin;
                Vector2 maxEntrySize = this.MaxEntrySize;
                float globalTextScale = this.GlobalTextScale;

                int width = (int)(maxEntrySize.X + (margin * 2));
                int height = margin;

                foreach (TextEntry entry in this.entryList)
                {
                    if (entry.imageFrame == null) height += (int)((entry.sizeY * globalTextScale) + margin);
                    else height += (int)(maxEntrySize.Y + margin);
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

            float scale;
            int lineCounter = 0;
            foreach (string line in text.Split('\n'))
            {
                scale = lineCounter == 0 ? 1.3f : 1f;

                entryList.Add(new TextEntry(text: line, color: Color.White, scale: scale, justify: TextEntry.Justify.Center));
                lineCounter++;
            }

            entryList.Add(new TextEntry(color: Color.White, progressCurrentVal: curVal, progressMaxVal: maxVal, justify: TextEntry.Justify.Center));

            if (addNumbers)
            {
                int percentage = (int)((float)curVal / (float)maxVal * 100);
                entryList.Add(new TextEntry(text: $"{curVal}/{maxVal}   {percentage}%", color: Color.White, justify: TextEntry.Justify.Center));
            }

            this.TurnOn(entryList: entryList, newPosX: 0, newPosY: 0, addTransition: addTransition, centerHoriz: true, centerVert: true);
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

            if (!addTransition)
            {
                this.transManager.ClearPreviousTransitions(copyDrawToBase: false);
                this.viewParams.PosX = newPosX;
                this.viewParams.PosY = newPosY;
                this.viewParams.Opacity = 1f;
                this.entryList = entryList;
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

                if (posXEqual && posYEqual && TextEntry.CompareTwoLists(this.entryList, entryList)) return;
            }

            this.entryList = entryList;
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

        public override void Update(GameTime gameTime)
        {
            if (!this.isActive) return;

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
            Rectangle bgRect = this.BgRect;
            int margin = this.Margin;

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, this.bgColor * this.bgOpacity * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);

            float globalTextScale = this.GlobalTextScale;

            Vector2 basePos, textPos, shadowPos, realEntrySize;
            float shadowOffset = Math.Max(4f * globalTextScale, 1);

            int posX = margin;
            int posY = margin;

            Vector2 maxEntrySize = this.MaxEntrySize;
            int frameWidth = (int)maxEntrySize.Y + margin;

            foreach (TextEntry entry in this.entryList)
            {
                // setting positions for current entry

                basePos = new Vector2(posX, posY);

                realEntrySize = new Vector2(entry.sizeX, entry.sizeY) * globalTextScale;
                if (entry.imageFrame != null) realEntrySize.X += frameWidth;

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

                textPos = new Vector2(basePos.X, basePos.Y);
                if (entry.imageFrame != null)
                {
                    textPos.X += frameWidth;
                    textPos.Y += (maxEntrySize.Y / 2) - (realEntrySize.Y / 2);
                }
                shadowPos = new Vector2(textPos.X + shadowOffset, textPos.Y + shadowOffset);

                // drawing entry

                if (entry.imageFrame != null)
                {
                    Rectangle gfxRect = new Rectangle((int)basePos.X, (int)basePos.Y, (int)maxEntrySize.Y, (int)maxEntrySize.Y);
                    entry.imageFrame.DrawAndKeepInRectBounds(destBoundsRect: gfxRect, color: Color.White * this.viewParams.drawOpacity);
                }

                if (entry.progressBarMode)
                {
                    int currentBarWidth = (int)(realEntrySize.X * ((float)entry.progressCurrentVal / (float)entry.progressMaxVal));

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((int)textPos.X, (int)textPos.Y,
                        (int)realEntrySize.X, (int)(entry.sizeY * globalTextScale)),
                        entry.color * this.viewParams.drawOpacity * 0.4f);

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((int)textPos.X, (int)textPos.Y,
                    (int)currentBarWidth, (int)(entry.sizeY * globalTextScale)),
                    entry.color * this.viewParams.drawOpacity);
                }
                else
                {
                    SonOfRobinGame.spriteBatch.DrawString(font, entry.text, position: shadowPos, color: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, origin: Vector2.Zero, scale: globalTextScale * entry.scale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                    SonOfRobinGame.spriteBatch.DrawString(font, entry.text, position: textPos, color: entry.color * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: globalTextScale * entry.scale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
                }

                if (entry.imageFrame != null) posY += (int)maxEntrySize.Y + margin;
                else posY += (int)((entry.sizeY * globalTextScale) + margin);
            }
        }

    }
}
