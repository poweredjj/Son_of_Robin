using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class TextWindow : Scene
    {
        public static readonly SpriteFont font = SonOfRobinGame.fontHuge;
        public static readonly int maxWidth = 90;

        private readonly bool autoClose;
        private int blockingFramesLeft;
        public readonly string text;
        private readonly int textWidth;
        private readonly int textHeight;
        private readonly Color textColor;
        private readonly Color bgColor;
        private float textScale;
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

        public TextWindow(string text, Color textColor, Color bgColor, bool animate = true, int framesPerChar = 0, bool useTransition = true, bool checkForDuplicate = false, bool blocksUpdatesBelow = false, int blockInputDuration = 0, Scheduler.TaskName closingTask = Scheduler.TaskName.Empty, Object closingTaskHelper = null, bool useTransitionOpen = false, bool useTransitionClose = false, bool autoClose = false, InputTypes inputType = InputTypes.Normal, int priority = 0) : base(inputType: inputType, priority: priority, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: blockInputDuration > 0 ? ControlTips.TipsLayout.Empty : ControlTips.TipsLayout.TextWindow)
        {
            this.text = SplitText(text: text, maxWidth: maxWidth);
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

            this.charCounter = animate ? 0 : text.Length;
            this.currentCharFramesLeft = framesPerChar;
            this.animationFinished = !animate;
            this.framesPerChar = framesPerChar;

            this.closingTask = closingTask;
            this.closingTaskHelper = closingTaskHelper;

            if (checkForDuplicate && this.IsADuplicate)
            {
                this.Remove();
                return;
            }

            this.UpdateViewParams();
            if (this.useTransition || this.useTransitionOpen) this.AddInOutTransition(inTrans: true);
        }

        public override void Remove()
        {
            if (this.transition == null)
            {
                if (this.useTransition || this.useTransitionClose)
                {
                    if (this.closingTask != Scheduler.TaskName.Empty) Input.GlobalInputActive = false; // input should be disabled until closing task is executed

                    this.AddInOutTransition(inTrans: false);
                    return;
                }
            }

            base.Remove();
            if (this.closingTask != Scheduler.TaskName.Empty) new Scheduler.Task(menu: null, taskName: this.closingTask, executeHelper: this.closingTaskHelper, delay: 0, turnOffInput: true);
        }

        public void AddClosingTask(Scheduler.TaskName closingTask, Object closingTaskHelper)
        {
            this.closingTask = closingTask;
            this.closingTaskHelper = closingTaskHelper;
        }

        private void AddInOutTransition(bool inTrans)
        {
            float zoomScale = 8f;
            Transition.TransType transType = inTrans ? Transition.TransType.From : Transition.TransType.To;
            bool removeScene = !inTrans;

            this.AddTransition(new Transition(type: transType, duration: 10, scene: this, blockInput: false, removeScene: removeScene, paramsToChange: new Dictionary<string, float> {
                    { "opacity", 0f },
                    { "posX", (SonOfRobinGame.VirtualWidth * 0.5f * zoomScale) - ((float)this.viewParams.width / zoomScale)},
                    { "posY", (SonOfRobinGame.VirtualHeight * 0.6f * zoomScale) - ((float)this.viewParams.height / zoomScale)},
                    { "scaleX", zoomScale },
                    { "scaleY", zoomScale }
            }));
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
            if (this.blockingFramesLeft > 0 && this.animationFinished)
            {
                this.blockingFramesLeft--;
                if (this.blockingFramesLeft == 0) this.tipsLayout = ControlTips.TipsLayout.TextWindow;
            }

            this.Animate();
            this.UpdateViewParams();

            bool anyButtonPressed = this.CheckForAnyInput();

            if (anyButtonPressed)
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
        public bool CheckForAnyInput()
        {
            if (this.blockingFramesLeft > 0) return false;

            if (Keyboard.HasBeenPressed(Keys.Space) ||
                Keyboard.HasBeenPressed(Keys.RightShift) ||
                Keyboard.HasBeenPressed(Keys.Enter) ||
                Keyboard.HasBeenPressed(Keys.Escape)) return true;

            if (GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.A) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.X) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Y)) return true;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            { if (touch.State == TouchLocationState.Pressed) return true; }

            return false;
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

            this.viewParams.width = scaledTextWidth + (this.Margin * 2);
            this.viewParams.height = scaledTextHeight + (this.Margin * 2);

            this.viewParams.posX = (SonOfRobinGame.VirtualWidth / 2) - (this.viewParams.width / 2);
            this.viewParams.posY = (SonOfRobinGame.VirtualHeight * 0.8f) - this.viewParams.height;
        }

        public override void Draw()
        {
            int margin = this.Margin;
            int bgShadowOffset = (int)(SonOfRobinGame.VirtualHeight * 0.02f);
            int textShadowOffset = (int)(SonOfRobinGame.VirtualHeight * 0.003f);

            Rectangle bgShadowRect = new Rectangle(bgShadowOffset, bgShadowOffset, this.viewParams.width, this.viewParams.height);
            Rectangle bgRect = new Rectangle(0, 0, this.viewParams.width, this.viewParams.height);
            Vector2 textPos = new Vector2(margin, margin);
            Vector2 textShadowPos = new Vector2(textPos.X + textShadowOffset, textPos.Y + textShadowOffset);

            string currentText = this.AnimatedText;

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgShadowRect, Color.Black * 0.4f * this.viewParams.drawOpacity);

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, this.bgColor * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: this.textColor * this.viewParams.drawOpacity, borderWidth: 2);

            SonOfRobinGame.spriteBatch.DrawString(font, currentText, position: textShadowPos, color: Color.Black * 0.5f * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: this.textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            SonOfRobinGame.spriteBatch.DrawString(font, currentText, position: textPos, color: this.textColor * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: this.textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

    }
}
