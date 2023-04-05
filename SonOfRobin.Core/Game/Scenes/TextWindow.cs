using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class TextWindow : Scene
    {
        public static readonly SpriteFont font = SonOfRobinGame.FontTommy40;

        private readonly bool autoClose;
        private int blockingFramesLeft;
        private readonly TextWithImages textWithImages;
        private readonly Color textColor;
        private readonly Color bgColor;
        private readonly bool drawShadow;
        private float textScale;
        private readonly bool useTransition;
        private readonly bool useTransitionOpen;
        private readonly bool useTransitionClose;
        private Scheduler.TaskName closingTask;
        private Object closingTaskHelper;

        private int MaxWindowWidth
        { get { return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.8f); } }

        private int MaxWindowHeight
        { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.7f); } }

        private int Margin
        { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.03f); } }

        private bool IsADuplicate
        {
            get
            {
                foreach (TextWindow textWindow in GetAllScenesOfType(typeof(TextWindow), includeWaiting: true))
                {
                    if (textWindow != this && textWindow.textWithImages.Text == this.textWithImages.Text) return true;
                }
                return false;
            }
        }

        public TextWindow(string text, Color textColor, Color bgColor, bool animate = true, int framesPerChar = 0, int charsPerFrame = 1, bool useTransition = true, bool checkForDuplicate = false, bool blocksUpdatesBelow = false, int blockInputDuration = 0, Scheduler.TaskName closingTask = Scheduler.TaskName.Empty, object closingTaskHelper = null, bool useTransitionOpen = false, bool useTransitionClose = false, bool autoClose = false, InputTypes inputType = InputTypes.Normal, int priority = 0, List<Texture2D> imageList = null, SoundData.Name startingSound = SoundData.Name.Empty, Sound animSound = null, bool treatImagesAsSquares = false, int maxWidth = 90, bool drawShadow = true) :

            base(inputType: inputType, priority: priority, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty, startingSound: startingSound, waitForOtherScenesOfTypeToEnd: true)
        {
            this.textWithImages = new TextWithImages(font: font, text: SplitText(text: text, maxWidth: maxWidth), imageList: imageList, animate: animate, framesPerChar: framesPerChar, charsPerFrame: charsPerFrame, animSound: animSound, treatImagesAsSquares: treatImagesAsSquares);

            this.textScale = 1f; // to be updated below
            this.textColor = textColor;
            this.bgColor = bgColor;
            this.drawShadow = drawShadow;
            this.useTransition = useTransition;
            this.useTransitionOpen = useTransitionOpen;
            this.useTransitionClose = useTransitionClose;
            this.blockingFramesLeft = blockInputDuration;
            this.autoClose = autoClose;

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

        private static string SplitText(string text, int maxWidth)
        {
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

                    newText += word;
                    lineWidth += word.Length;
                    wordNo++;
                }

                lineNo++;
            }

            return newText;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.blockingFramesLeft > 0 && this.textWithImages.AnimationFinished)
            {
                this.blockingFramesLeft--;
                if (this.blockingFramesLeft == 0) this.SetTipsAndTouchLayout();
            }

            this.textWithImages.Update();
            this.UpdateViewParams();

            bool okButtonPressed = false;
            bool cancelButtonPressed = this.CheckForInputCancel();
            if (cancelButtonPressed)
            {
                if (this.tipsLayout == ControlTips.TipsLayout.TextWindowCancel || this.tipsLayout == ControlTips.TipsLayout.TextWindowOkCancel)
                {
                    new Scheduler.Task(taskName: Scheduler.TaskName.SkipCinematics);

                    return;
                }
                else okButtonPressed = cancelButtonPressed; // if there is no cancel button, cancel button can be used as "ok"
            }

            if (!okButtonPressed) okButtonPressed = this.CheckForInputOk();
            if (okButtonPressed)
            {
                if (this.textWithImages.AnimationFinished) this.Remove();
                else this.textWithImages.FinishAnimationNow();
            }

            if (this.autoClose && this.textWithImages.AnimationFinished && this.blockingFramesLeft == 0) this.Remove();
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

        private void UpdateViewParams()
        {
            int maxTextWidth = this.MaxWindowWidth - (this.Margin * 2);
            int maxTextHeight = this.MaxWindowHeight - (this.Margin * 2);

            float scaleX = (float)maxTextWidth / (float)this.textWithImages.textWidth;
            float scaleY = (float)maxTextHeight / (float)this.textWithImages.textHeight;
            this.textScale = Math.Min(scaleX, scaleY);
            this.textScale = Math.Min(this.textScale, (float)SonOfRobinGame.VirtualWidth / 2000f);

            int scaledTextWidth = (int)(this.textWithImages.textWidth * this.textScale);
            int scaledTextHeight = (int)(this.textWithImages.textHeight * this.textScale);

            this.viewParams.Width = scaledTextWidth + (this.Margin * 2);
            this.viewParams.Height = scaledTextHeight + (this.Margin * 2);

            this.viewParams.PosX = (SonOfRobinGame.VirtualWidth / 2) - (this.viewParams.Width / 2);
            this.viewParams.PosY = (SonOfRobinGame.VirtualHeight * 0.8f) - this.viewParams.Height;
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            int margin = this.Margin;
            int bgShadowOffset = (int)(SonOfRobinGame.VirtualHeight * 0.02f);
            int textShadowOffset = (int)Math.Max(SonOfRobinGame.VirtualHeight * 0.003f, 1);

            Rectangle bgShadowRect = new Rectangle(bgShadowOffset, bgShadowOffset, this.viewParams.Width, this.viewParams.Height);
            Rectangle bgRect = new Rectangle(0, 0, this.viewParams.Width, this.viewParams.Height);
            Vector2 textPos = new Vector2(margin, margin);

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bgShadowRect, Color.Black * 0.4f * this.viewParams.drawOpacity);

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bgRect, this.bgColor * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: this.textColor * this.viewParams.drawOpacity, borderWidth: 2);

            if (this.drawShadow)
            {
                this.textWithImages.Draw(position: textPos, color: this.textColor, imageOpacity: this.viewParams.drawOpacity, shadowColor: Color.Black * 0.5f, shadowOffset: new Vector2(textShadowOffset), textScale: this.textScale);
            }
            else
            {
                this.textWithImages.Draw(position: textPos, color: this.textColor, imageOpacity: this.viewParams.drawOpacity, textScale: this.textScale);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}