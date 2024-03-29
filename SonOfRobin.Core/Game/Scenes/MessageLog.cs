﻿using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MessageLog : Scene
    {
        private class Message
        {
            public readonly SpriteFontBase font;
            private readonly Vector2 textSize;
            public readonly bool isDebug;
            private readonly TriSliceBG triSliceBG;
            public int flashFrame;
            public readonly string text;
            public int deletionFrame;
            private readonly Color textColor;
            private readonly Vector2 textPos;
            private readonly Color bgColor;
            public readonly Rectangle bgRect;
            private readonly ImageObj image;
            private readonly Texture2D highlightTexture;
            private readonly Rectangle highlightRect;
            private readonly Rectangle imageRect;
            public Vector2 basePos;
            public bool markedForDeletion;

            public Message(TriSliceBG triSliceBG, bool isDebug, string message, Color textColor, Color bgColor, int lastDeletionFrame, int messagesCount, ImageObj image = null)
            {
                this.markedForDeletion = false;
                this.triSliceBG = triSliceBG;
                this.font = isDebug ?
                     SonOfRobinGame.FontPressStart2P.GetFont(Math.Max(Preferences.messageLogScale, 1f) * 8) :
                     SonOfRobinGame.FontVCROSD.GetFont(21 * Preferences.messageLogScale);

                this.isDebug = isDebug;
                this.flashFrame = SonOfRobinGame.CurrentUpdate;
                this.text = message;
                this.textColor = textColor;
                this.bgColor = bgColor;
                this.image = image;
                this.textSize = Helpers.MeasureStringCorrectly(font: this.font, stringToMeasure: this.text);
                this.basePos = Vector2.Zero;

                int bgInflateSize = this.image != null ? 10 : 4;
                if (this.isDebug) bgInflateSize = 2;

                this.bgRect = new Rectangle(x: 0, y: 0, width: (int)textSize.X + (bgInflateSize * 4), height: (int)textSize.Y + (bgInflateSize * 2));
                this.textPos = new(bgInflateSize * 2, bgInflateSize);
                this.imageRect = Rectangle.Empty;
                this.highlightTexture = null;
                this.highlightRect = Rectangle.Empty;

                if (this.image != null)
                {
                    this.imageRect = new Rectangle(x: (int)this.textPos.X, y: 0, width: bgRect.Height, height: bgRect.Height);
                    int textureMargin = (int)(bgInflateSize * 1.5f);
                    this.bgRect.Width += this.imageRect.Width + textureMargin;
                    this.textPos.X += this.imageRect.Width + textureMargin;

                    this.highlightTexture = TextureBank.GetTexture(TextureBank.TextureName.WhiteHorizontalLine);
                    this.highlightRect = this.imageRect;
                    this.highlightRect.Inflate(this.highlightRect.Width / 4, -1); // highlight should be a little smaller than the background

                    this.imageRect.Inflate(0, -3); // texture should be a little smaller than the background
                }

                int displayDuration = messagesCount == 0 ? (int)(60 * 3.5f) : 90 / Math.Min(messagesCount, 3);
                this.deletionFrame = Math.Max(lastDeletionFrame, SonOfRobinGame.CurrentUpdate) + displayDuration;
                this.deletionFrame = Math.Min(this.deletionFrame, SonOfRobinGame.CurrentUpdate + (60 * 5)); // to make sure that it won't be displayed for too long
            }

            private static Rectangle AddOffsetToRect(Rectangle rectangle, Vector2 offset)
            {
                Rectangle rectangleWithOffset = rectangle;
                rectangleWithOffset.Offset(offset);
                return rectangleWithOffset;
            }

            private float Opacity
            {
                get
                {
                    return (float)Helpers.ConvertRange(oldMin: this.deletionFrame, oldMax: this.deletionFrame - 60, newMin: 0, newMax: 1, oldVal: SonOfRobinGame.CurrentUpdate, clampToEdges: true);
                }
            }

            private float FlashOpacity
            { get { return (float)Helpers.ConvertRange(oldMin: this.flashFrame + 20, oldMax: this.flashFrame, newMin: 0, newMax: 0.6, oldVal: SonOfRobinGame.CurrentUpdate, clampToEdges: true); } }

            private Rectangle ImageRectWithOffset
            { get { return AddOffsetToRect(rectangle: this.imageRect, offset: this.basePos); } }

            private Rectangle HighlightRectWithOffset
            { get { return AddOffsetToRect(rectangle: this.highlightRect, offset: this.basePos); } }

            public Rectangle BGRectWithOffset
            { get { return AddOffsetToRect(rectangle: this.bgRect, offset: this.basePos); } }

            private Vector2 TextPosWithOffset
            { get { return this.textPos + this.basePos; } }

            public void Draw()
            {
                float flashOpacity = this.FlashOpacity;
                Color finalBGColor = this.bgColor * 0.5f;
                float opacity = this.Opacity;

                if (flashOpacity > 0 && !this.isDebug) finalBGColor = Helpers.Blend2Colors(firstColor: finalBGColor, secondColor: Color.White, firstColorOpacity: 1 - flashOpacity, secondColorOpacity: flashOpacity);

                this.triSliceBG.Draw(triSliceRect: this.BGRectWithOffset, color: finalBGColor * opacity);

                if (this.image != null)
                {
                    SonOfRobinGame.SpriteBatch.Draw(this.highlightTexture, this.HighlightRectWithOffset, this.highlightTexture.Bounds, Color.White * opacity * 0.85f);
                    this.image.DrawInsideRect(rect: this.ImageRectWithOffset, color: Color.White * opacity);
                }

                this.font.DrawText(
                batch: SonOfRobinGame.SpriteBatch,
                text: this.text,
                position: this.TextPosWithOffset,
                color: this.textColor * opacity,
                effect: FontSystemEffect.Stroked,
                effectAmount: 1);
            }
        }

        private const int messageMargin = 2;
        private readonly int leftMargin;
        private readonly int bottomMargin;
        private int screenHeight;
        private readonly TriSliceBG triSliceBG;
        private string lastDebugMessage;

        private List<Message> messages;
        private HashSet<string> displayedStrings;
        private int baseline;

        private int MaxDeletionFrame
        {
            get
            {
                while (true)
                {
                    try
                    {
                        return this.messages.Count > 0 ? this.messages.Select(m => m.deletionFrame).Max() : SonOfRobinGame.CurrentUpdate;
                    }
                    catch (InvalidOperationException)
                    { } // if background task adds a message in the meantime
                }
            }
        }

        public MessageLog() : base(inputType: InputTypes.None, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.leftMargin = SonOfRobinGame.platform == Platform.Desktop ? 7 : 20;
            this.bottomMargin = SonOfRobinGame.platform == Platform.Desktop ? 2 : 5;

            this.triSliceBG = TriSliceBG.GetBGForPreset(TriSliceBG.Preset.Message);

            this.messages = new();
            this.lastDebugMessage = "";
            this.displayedStrings = new();
            this.ResetBaseline();
        }

        public void ResetBaseline()
        {
            this.baseline = this.screenHeight;
        }

        protected override void AdaptToNewSize()
        {
            this.ResetBaseline();
        }

        public override void Update()
        {
            this.DeleteOldMessages();
            this.screenHeight = Preferences.ShowControlTips ? (int)(SonOfRobinGame.ScreenHeight * 0.94f) : SonOfRobinGame.ScreenHeight;

            int minBaselineVal = this.screenHeight - this.bottomMargin;

            if (this.baseline > minBaselineVal) this.baseline -= (int)(5f * Preferences.messageLogScale);
            this.baseline = Math.Max(this.baseline, minBaselineVal);
        }

        public override void Draw()
        {
            if (this.messages.Count == 0) return;

            List<Message> messagesToDisplay;

            try
            { messagesToDisplay = this.messages.ToList(); }
            catch (ArgumentException) { return; } // if some background process tries to add message right now

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Vector2 currentPos = new Vector2(this.leftMargin, this.baseline);
            int maxDrawHeight = 0;

            bool debugActive = GetTopSceneOfType(typeof(DebugScene)) != null;
            Rectangle debugTextRect = new(0, 0, SonOfRobinGame.ScreenWidth, (int)DebugScene.lastTextSize.Y);

            Rectangle drawAreaRect = new(0, maxDrawHeight, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight - maxDrawHeight);

            for (int messageNo = messagesToDisplay.Count - 1; messageNo >= 0; messageNo--)
            {
                Message message = messagesToDisplay[messageNo];
                currentPos.Y -= message.BGRectWithOffset.Height + messageMargin;

                message.basePos = Preferences.messageLogAtRight ? new Vector2(SonOfRobinGame.ScreenWidth - message.bgRect.Width - currentPos.X, currentPos.Y) : currentPos;

                Rectangle messageRect = message.BGRectWithOffset;

                if (messageRect.Bottom < 0 || (debugActive && messageRect.Intersects(debugTextRect)))
                {
                    // message is above screen or collides with debug text
                    message.markedForDeletion = true;
                    continue;
                }
                else if (!messageRect.Intersects(drawAreaRect)) continue; // message is (probably) below screen
                else message.Draw();
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public static void Add(string text, bool debugMessage = false, bool avoidDuplicates = false, Color textColor = default, Color bgColor = default, ImageObj imageObj = null)
        {
            if (SonOfRobinGame.MessageLog == null)
            {
                Console.WriteLine(text);
                return;
            }

            MessageLog messageLog = SonOfRobinGame.MessageLog;

            if (textColor == default) textColor = Color.White;
            if (bgColor == default) bgColor = Color.Black;

            if (avoidDuplicates && messageLog.displayedStrings.Contains(text))
            {
                foreach (Message messageToCheck in messageLog.messages)
                {
                    if (messageToCheck.text == text)
                    {
                        messageToCheck.flashFrame = SonOfRobinGame.CurrentUpdate;
                        messageToCheck.deletionFrame = SonOfRobinGame.CurrentUpdate + (60 * 2);
                        return;
                    }
                }
            }

            if (debugMessage)
            {
                if (messageLog.lastDebugMessage == text) return;
                messageLog.lastDebugMessage = text;
            }

            Console.WriteLine(text); // additional output

            if (debugMessage && !Preferences.DebugMode) return;

            Message message = new Message(triSliceBG: messageLog.triSliceBG, isDebug: debugMessage, message: text, textColor: textColor, bgColor: bgColor, image: imageObj, lastDeletionFrame: messageLog.MaxDeletionFrame, messagesCount: messageLog.messages.Count);

            if (messageLog.messages.Count > 0) messageLog.baseline += messageMargin + message.bgRect.Height;

            messageLog.messages.Add(message);
            messageLog.displayedStrings.Add(text);
        }

        private void DeleteOldMessages()
        {
            int currentUpdate = SonOfRobinGame.CurrentUpdate;
            var filteredMessages = this.messages.Where(message => currentUpdate < message.deletionFrame && !message.markedForDeletion);
            if (!Preferences.DebugMode) filteredMessages = filteredMessages.Where(message => !message.isDebug);

            this.messages = filteredMessages.ToList();

            this.displayedStrings.Clear();
            this.displayedStrings = filteredMessages.Select(m => m.text).ToHashSet();
        }
    }
}