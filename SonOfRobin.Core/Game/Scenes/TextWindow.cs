using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class TextWindow : Scene
    {
        public static readonly SpriteFont font = SonOfRobinGame.fontTommy40;
        public static readonly int maxWidth = 90;
        public static readonly string imageMarkerStart = "|";
        public static readonly string imageMarkerExtend = "'";

        private readonly bool autoClose;
        private int blockingFramesLeft;
        public readonly string text;
        private readonly int textWidth;
        private readonly int textHeight;
        private readonly Color textColor;
        private readonly Color bgColor;
        private float textScale;
        private readonly List<Texture2D> imageList;
        private readonly bool useTransition;
        private readonly bool useTransitionOpen;
        private readonly bool useTransitionClose;
        private int charCounter;
        private int currentCharFramesLeft;
        private bool animationFinished;
        private readonly int framesPerChar;
        private Scheduler.TaskName closingTask;
        private Object closingTaskHelper;
        private string AnimatedText { get { return this.text.Substring(0, this.charCounter); } }
        private int MaxWindowWidth { get { return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.8f); } }
        private int MaxWindowHeight { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.7f); } }
        private int Margin { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.03f); } }

        private bool IsADuplicate
        {
            get
            {
                foreach (TextWindow textWindow in GetAllScenesOfType(typeof(TextWindow)))
                {
                    if (textWindow != this && textWindow.text == this.text) return true;
                }
                return false;
            }
        }

        public TextWindow(string text, Color textColor, Color bgColor, bool animate = true, int framesPerChar = 0, bool useTransition = true, bool checkForDuplicate = false, bool blocksUpdatesBelow = false, int blockInputDuration = 0, Scheduler.TaskName closingTask = Scheduler.TaskName.Empty, Object closingTaskHelper = null, bool useTransitionOpen = false, bool useTransitionClose = false, bool autoClose = false, InputTypes inputType = InputTypes.Normal, int priority = 0, List<Texture2D> imageList = null) : base(inputType: inputType, priority: priority, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.imageList = imageList == null ? new List<Texture2D>() : imageList;
            this.text = SplitTextAndResizeMarkers(text: text, maxWidth: maxWidth, imageList: this.imageList);
            Vector2 textSize = font.MeasureString(this.text);
            this.textWidth = (int)textSize.X;
            this.textHeight = (int)textSize.Y;
            this.textScale = 1f; // to be updated below
            this.textColor = textColor;
            this.bgColor = bgColor;
            this.useTransition = useTransition;
            this.useTransitionOpen = useTransitionOpen;
            this.useTransitionClose = useTransitionClose;
            this.blockingFramesLeft = blockInputDuration;
            this.autoClose = autoClose;

            this.charCounter = animate ? 0 : this.text.Length;
            this.currentCharFramesLeft = framesPerChar;
            this.animationFinished = !animate;
            this.framesPerChar = framesPerChar;

            this.closingTask = closingTask;
            this.closingTaskHelper = closingTaskHelper;

            this.SetTipsAndTouchLayout();

            if (checkForDuplicate && this.IsADuplicate)
            {
                this.Remove();
                return;
            }

            this.UpdateViewParams();
            if (this.useTransition || this.useTransitionOpen) this.AddInOutTransition(inTrans: true);

            this.ValidateImagesCount();
        }

        private void ValidateImagesCount()
        {
            MatchCollection matches = Regex.Matches(this.text, $@"\{imageMarkerStart}"); // $@ is needed for "\" character inside interpolated string

            if (this.imageList.Count != matches.Count) throw new ArgumentException($"TextWindow - count of markers ({matches.Count}) and images ({this.imageList.Count}) does not match.\n{this.text}");
        }

        private void SetTipsAndTouchLayout()
        {
            World world = World.GetTopWorld();
            bool addOkButton = this.blockingFramesLeft == 0;
            bool addCancelButton = world != null && world.CineMode;

            if (addOkButton && addCancelButton)
            {
                this.tipsLayout = ControlTips.TipsLayout.TextWindowOkCancel;
                this.touchLayout = TouchLayout.TextWindowOkCancel;
            }
            else if (addOkButton && !addCancelButton)
            {
                this.tipsLayout = ControlTips.TipsLayout.TextWindowOk;
                this.touchLayout = TouchLayout.TextWindowOk;
            }
            else if (!addOkButton && addCancelButton)
            {
                this.tipsLayout = ControlTips.TipsLayout.TextWindowCancel;
                this.touchLayout = TouchLayout.TextWindowCancel;
            }
            else
            {
                this.tipsLayout = ControlTips.TipsLayout.Empty;
                this.touchLayout = TouchLayout.Empty;
            }
        }

        public void RemoveWithoutExecutingTask()
        {
            // Useful for skipping scenes - task chains are stored in closingTaskHelper, so disabling closing task throws away any leftover "cinematics" tasks.
            this.closingTask = Scheduler.TaskName.Empty;
            this.Remove();
        }

        public override void Remove()
        {
            //   MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Removing TextWindow '{this.text}' (frame {SonOfRobinGame.currentUpdate}).");

            if (!this.transManager.IsEnding)
            {
                if (this.useTransition || this.useTransitionClose)
                {
                    if (this.closingTask != Scheduler.TaskName.Empty) Input.GlobalInputActive = false; // input should be disabled until closing task is executed

                    this.AddInOutTransition(inTrans: false);
                    return;
                }
            }

            base.Remove();
            if (this.closingTask != Scheduler.TaskName.Empty) new Scheduler.Task(taskName: this.closingTask, executeHelper: this.closingTaskHelper, delay: 0, turnOffInputUntilExecution: true);
        }

        public void AddClosingTask(Scheduler.TaskName closingTask, Object closingTaskHelper)
        {
            this.closingTask = closingTask;
            this.closingTaskHelper = closingTaskHelper;
        }

        private void AddInOutTransition(bool inTrans)
        {
            float zoomScale = 8f;
            bool removeScene = !inTrans;

            this.transManager.AddMultipleTransitions(outTrans: !inTrans, duration: 10, endRemoveScene: removeScene, paramsToChange:
                 new Dictionary<string, float> {
                    { "Opacity", 0f },
                    { "PosX", (SonOfRobinGame.VirtualWidth * 0.5f * zoomScale) - ((float)this.viewParams.Width / zoomScale)},
                    { "PosY", (SonOfRobinGame.VirtualHeight * 0.6f * zoomScale) - ((float)this.viewParams.Height / zoomScale)},
                    { "ScaleX", zoomScale },
                    { "ScaleY", zoomScale }}
                );

            if (inTrans) this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: true, duration: 15, playCount: -1, replaceBaseValue: false, baseParamName: "Rot", targetVal: 0.13f, stageTransform: Transition.Transform.Sinus, pingPongCycles: false, cycleMultiplier: 0.17f));
        }

        private static string SplitTextAndResizeMarkers(string text, int maxWidth, List<Texture2D> imageList)
        {
            int imageNo = 0;
            string newText = "";

            string[] lineList = text.Split(Environment.NewLine.ToCharArray());

            int lineNo, wordNo, lineWidth;

            lineNo = 0;
            foreach (string line in lineList)
            {
                if (lineNo > 0) newText += "\n";
                lineWidth = 0;

                string[] wordList = line.Split(' ');

                wordNo = 0;
                foreach (string word in wordList)
                {
                    if (wordNo > 0) newText += " ";

                    if (lineWidth + word.Length > maxWidth)
                    {
                        newText += "\n";
                        lineWidth = 0;
                    }

                    string wordWithResizedMarker = word;

                    if (word.Contains(imageMarkerStart))
                    {
                        wordWithResizedMarker = ResizeImageMarker(text: wordWithResizedMarker, image: imageList[imageNo]);
                        imageNo++;
                    }

                    newText += wordWithResizedMarker;
                    lineWidth += wordWithResizedMarker.Length;
                    wordNo++;
                }

                lineNo++;
            }

            return newText;
        }

        private static string ResizeImageMarker(string text, Texture2D image)
        {
            float imageScale = (float)image.Height / font.MeasureString(text).Y;
            int targetWidth = (int)((float)image.Width / imageScale);

            int extendCharCount = 0;
            int lastDelta = 99999;
            string lastResizedMarker = imageMarkerStart;
            while (true)
            {
                string resizedMarker = imageMarkerStart + string.Concat(Enumerable.Repeat(imageMarkerExtend, extendCharCount));

                int delta = (int)Math.Abs(targetWidth - font.MeasureString(resizedMarker).X);
                if (delta > lastDelta) break;
                else
                {
                    lastDelta = delta;
                    lastResizedMarker = resizedMarker;
                    extendCharCount++;
                }
            }

            if (text.IndexOf(imageMarkerStart) != text.LastIndexOf(imageMarkerStart)) throw new ArgumentException("Found multiple markers within one word.");

            return text.Replace(imageMarkerStart, lastResizedMarker);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.blockingFramesLeft > 0 && this.animationFinished)
            {
                this.blockingFramesLeft--;
                if (this.blockingFramesLeft == 0) this.SetTipsAndTouchLayout();
            }

            this.Animate();
            this.UpdateViewParams();

            bool okButtonPressed = false;
            bool cancelButtonPressed = this.CheckForInputCancel();
            if (cancelButtonPressed)
            {
                if (this.tipsLayout == ControlTips.TipsLayout.TextWindowCancel || this.tipsLayout == ControlTips.TipsLayout.TextWindowOkCancel)
                {
                    var confirmationData = new Dictionary<string, Object> { { "question", "Do you want to skip?" }, { "taskName", Scheduler.TaskName.SkipCinematics }, { "executeHelper", null } };
                    new Scheduler.Task(taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                    return;
                }
                else okButtonPressed = cancelButtonPressed; // if there is no cancel button, cancel button can be used as "ok"
            }

            if (!okButtonPressed) okButtonPressed = this.CheckForInputOk();
            if (okButtonPressed)
            {
                if (this.animationFinished) this.Remove();
                else
                {
                    this.charCounter = this.text.Length;
                    this.animationFinished = true;
                }
            }

            if (this.autoClose && this.animationFinished && this.blockingFramesLeft == 0) this.Remove();
        }
        public bool CheckForInputOk()
        {
            if (this.blockingFramesLeft > 0) return false;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm)) return true;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            { if (touch.State == TouchLocationState.Pressed) return true; } // the whole screen area is one big "OK" button

            return false;
        }

        public bool CheckForInputCancel()
        {
            return InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip);
        }

        private void Animate()
        {
            if (this.animationFinished) return;

            this.currentCharFramesLeft--;
            if (this.currentCharFramesLeft <= 0)
            {
                this.charCounter++;
                this.currentCharFramesLeft = framesPerChar;

                if (this.charCounter == this.text.Length)
                {
                    this.animationFinished = true;
                    return;
                }
            }
        }

        private void UpdateViewParams()
        {
            int maxTextWidth = this.MaxWindowWidth - (this.Margin * 2);
            int maxTextHeight = this.MaxWindowHeight - (this.Margin * 2);

            float scaleX = (float)maxTextWidth / (float)this.textWidth;
            float scaleY = (float)maxTextHeight / (float)this.textHeight;
            this.textScale = Math.Min(scaleX, scaleY);
            this.textScale = Math.Min(this.textScale, (float)SonOfRobinGame.VirtualWidth / 2000f);

            int scaledTextWidth = (int)(this.textWidth * this.textScale);
            int scaledTextHeight = (int)(this.textHeight * this.textScale);

            this.viewParams.Width = scaledTextWidth + (this.Margin * 2);
            this.viewParams.Height = scaledTextHeight + (this.Margin * 2);

            this.viewParams.PosX = (SonOfRobinGame.VirtualWidth / 2) - (this.viewParams.Width / 2);
            this.viewParams.PosY = (SonOfRobinGame.VirtualHeight * 0.8f) - this.viewParams.Height;
        }

        public override void Draw()
        {
            int margin = this.Margin;
            int bgShadowOffset = (int)(SonOfRobinGame.VirtualHeight * 0.02f);
            int textShadowOffset = (int)Math.Max(SonOfRobinGame.VirtualHeight * 0.003f, 1);

            Rectangle bgShadowRect = new Rectangle(bgShadowOffset, bgShadowOffset, this.viewParams.Width, this.viewParams.Height);
            Rectangle bgRect = new Rectangle(0, 0, this.viewParams.Width, this.viewParams.Height);
            Vector2 textPos = new Vector2(margin, margin);
            Vector2 textShadowPos = new Vector2(textPos.X + textShadowOffset, textPos.Y + textShadowOffset);

            string currentText = this.AnimatedText;

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgShadowRect, Color.Black * 0.4f * this.viewParams.drawOpacity);

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, this.bgColor * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: this.textColor * this.viewParams.drawOpacity, borderWidth: 2);

            Color shadowColor = Color.Black * 0.5f * this.viewParams.drawOpacity;

            SonOfRobinGame.spriteBatch.DrawString(font, currentText, position: textShadowPos, color: shadowColor, origin: Vector2.Zero, scale: this.textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            SonOfRobinGame.spriteBatch.DrawString(font, currentText, position: textPos, color: this.textColor * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: this.textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            int imageNo = 0;
            foreach (Texture2D image in this.imageList)
            {
                int markerIndex = this.GetImageMarkerIndex(currentText: currentText, imageNo: imageNo);

                if (markerIndex == -1) break;
                else
                {
                    Rectangle markerRect = this.GetImageMarkerRect(markerIndex: markerIndex, baseTextPos: textPos);

                    Rectangle markerShadowRect = new Rectangle(x: markerRect.X + textShadowOffset, y: markerRect.Y + textShadowOffset, width: markerRect.Width, height: markerRect.Height);

                    // painting over the marker, to hide it
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, markerShadowRect, this.bgColor * this.viewParams.drawOpacity);
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, markerRect, this.bgColor * this.viewParams.drawOpacity);

                    Helpers.DrawTextureInsideRect(texture: image, rectangle: markerShadowRect, color: shadowColor, drawTestRect: false);
                    Helpers.DrawTextureInsideRect(texture: image, rectangle: markerRect, color: Color.White * this.viewParams.drawOpacity, drawTestRect: false);
                }
                imageNo++;
            }
        }

        private int GetImageMarkerIndex(string currentText, int imageNo)
        {
            int markerIndex = -1;
            int matchNo = 0;
            foreach (Match match in Regex.Matches(currentText, $@"\{imageMarkerStart}"))  // $@ is needed for "\" character inside interpolated string
            {
                if (matchNo == imageNo)
                {
                    markerIndex = match.Index;
                    break;
                }
                matchNo++;
            }

            return markerIndex;
        }

        private Rectangle GetImageMarkerRect(int markerIndex, Vector2 baseTextPos)
        {
            string fullMarker = imageMarkerStart;
            int currentMarkerPos = markerIndex;
            while (true)
            {
                currentMarkerPos++;

                if (Convert.ToString(this.text[currentMarkerPos]) != imageMarkerExtend) break;
                else fullMarker += this.text[currentMarkerPos];
            }

            string textBeforeMarker = this.text.Substring(0, markerIndex);

            int lastNewLineIndex = textBeforeMarker.LastIndexOf('\n');
            if (lastNewLineIndex == -1) lastNewLineIndex = 0; // if no newline was found, LastIndexOf() == -1
            else lastNewLineIndex += 1; // deleting "\n" from the start

            lastNewLineIndex = Math.Max(lastNewLineIndex, 0);

            string thisLineText = textBeforeMarker.Substring(lastNewLineIndex, textBeforeMarker.Length - lastNewLineIndex);
            string textBeforeThisLine = textBeforeMarker.Substring(0, Math.Max(lastNewLineIndex - 1, 0));

            int posX = (int)(font.MeasureString(thisLineText).X * this.textScale);
            int posY = (int)(font.MeasureString(textBeforeThisLine).Y * this.textScale);
            Vector2 rectSize = font.MeasureString(fullMarker) * this.textScale;

            return new Rectangle(x: (int)baseTextPos.X + posX, y: (int)baseTextPos.Y + posY, width: (int)rectSize.X, height: (int)rectSize.Y);
        }

    }
}
