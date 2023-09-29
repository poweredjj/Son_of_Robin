using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MessageLog : Scene
    {
        private readonly struct Message
        {
            public readonly bool isDebug;
            public readonly string text;
            public readonly int deletionFrame;
            public readonly Color textColor;
            public readonly Color bgColor;
            public readonly Texture2D texture;

            public Message(bool isDebug, string message, Color textColor, Color bgColor, int lastDeletionFrame, int messagesCount, Texture2D texture = null)
            {
                this.isDebug = isDebug;
                this.text = message;
                this.textColor = textColor;
                this.bgColor = bgColor;
                this.texture = texture;

                int delay = messagesCount == 0 ? 60 * 3 : 90 / Math.Min(messagesCount, 3);

                this.deletionFrame = Math.Max(lastDeletionFrame, SonOfRobinGame.CurrentUpdate) + delay;
            }
        }

        private static readonly SpriteFontBase font = SonOfRobinGame.FontVCROSD.GetFont(13);
        private static readonly TimeSpan maxDuplicateCheckDuration = TimeSpan.FromSeconds(20);

        private readonly int marginX;
        private readonly int marginY;

        private int lastDeletionFrame;
        private DateTime lastDebugMessageAdded;
        private DateTime lastUserMessageAdded;
        private string lastDebugMessage;
        private string lastUserMessage;
        private int screenHeight;
        private readonly TriSliceBG triSliceBG;

        private List<Message> messages;

        public MessageLog() : base(inputType: InputTypes.None, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.marginX = SonOfRobinGame.platform == Platform.Desktop ? 7 : 20;
            this.marginY = SonOfRobinGame.platform == Platform.Desktop ? 2 : 5;

            this.lastDeletionFrame = 0;
            this.lastDebugMessageAdded = DateTime.MinValue;
            this.lastUserMessageAdded = DateTime.MinValue;
            this.lastDebugMessage = "";
            this.lastUserMessage = "";

            this.triSliceBG = new TriSliceBG(
                textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogLeft),
                textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogMid),
                textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogRight));

            this.messages = new();
        }

        public override void Update()
        {
            int currentFrame = SonOfRobinGame.CurrentUpdate;
            this.DeleteOldMessages(currentFrame);
            this.screenHeight = Preferences.ShowControlTips ? (int)(SonOfRobinGame.VirtualHeight * 0.94f) : (int)(SonOfRobinGame.VirtualHeight * 1f);
        }

        public override void Draw()
        {
            if (this.messages.Count == 0) return;

            List<Message> messagesToDisplay;

            try
            { messagesToDisplay = this.messages.ToList(); }
            catch (ArgumentException) { return; } // if some background process tries to add message right now

            TriSliceBG.StartSpriteBatch(this); // needed to correctly draw triSliceBG

            int currentFrame = SonOfRobinGame.CurrentUpdate;
            int currentPosY = this.screenHeight - this.marginY;
            int maxDrawHeight = (int)(this.screenHeight * 0.2f);

            for (int messageNo = messagesToDisplay.Count - 1; messageNo >= 0; messageNo--)
            {
                if (currentPosY >= maxDrawHeight)
                {
                    Message message = messagesToDisplay[messageNo];
                    Vector2 txtSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: message.text);
                    int bgInflateSize = message.texture != null ? 8 : 4;

                    Vector2 entryPos = new Vector2(this.marginX, currentPosY);
                    Vector2 txtPos = new(entryPos.X + (bgInflateSize * 2), currentPosY + bgInflateSize);

                    Rectangle bgRect = new Rectangle(
                        x: (int)entryPos.X,
                        y: (int)entryPos.Y,
                        width: (int)txtSize.X + (bgInflateSize * 4),
                        height: (int)txtSize.Y + (bgInflateSize * 2));

                    int entryHeight = bgRect.Height + 2;
                    currentPosY -= entryHeight;

                    bgRect.Y -= entryHeight; // moving up by the size of one entry
                    txtPos.Y -= entryHeight; // moving up by the size of one entry

                    float opacity = (float)Helpers.ConvertRange(oldMin: message.deletionFrame, oldMax: message.deletionFrame - 60, newMin: 0, newMax: 1, oldVal: currentFrame, clampToEdges: true);

                    triSliceBG.Draw(triSliceRect: bgRect, color: message.bgColor * 0.5f * opacity);

                    font.DrawText(
                        batch: SonOfRobinGame.SpriteBatch,
                        text: message.text,
                        position: txtPos,
                        color: message.textColor * opacity,
                        effect: FontSystemEffect.Stroked,
                        effectAmount: 1);
                }
                else break;
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public void Add(string text, bool debugMessage = false, bool avoidDuplicates = false, Color textColor = default, Color bgColor = default, Texture2D texture = null)
        {
            if (textColor == default) textColor = Color.White;
            if (bgColor == default) bgColor = Color.Black;

            if (avoidDuplicates)
            {
                if ((debugMessage && this.lastDebugMessage == text && DateTime.Now - this.lastDebugMessageAdded < maxDuplicateCheckDuration) ||
                    (!debugMessage && this.lastUserMessage == text && DateTime.Now - this.lastUserMessageAdded < maxDuplicateCheckDuration)) return;
            }

            Console.WriteLine(text); // additional output
            if (debugMessage)
            {
                this.lastDebugMessage = text;
                this.lastDebugMessageAdded = DateTime.Now;
            }
            else
            {
                this.lastUserMessage = text;
                this.lastUserMessageAdded = DateTime.Now;
            }

            if (debugMessage && !Preferences.DebugMode) return;

            Message message = new Message(isDebug: debugMessage, message: text, textColor: textColor, bgColor: bgColor, texture: texture, lastDeletionFrame: this.lastDeletionFrame, messagesCount: this.messages.Count);
            messages.Add(message);
            this.lastDeletionFrame = message.deletionFrame;
        }

        private void DeleteOldMessages(int currentFrame)
        {
            var filteredMessages = this.messages.Where(message => currentFrame < message.deletionFrame);
            if (!Preferences.DebugMode) filteredMessages = filteredMessages.Where(message => !message.isDebug);

            this.messages = filteredMessages.ToList();
        }
    }
}