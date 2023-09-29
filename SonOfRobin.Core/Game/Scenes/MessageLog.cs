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
        public readonly struct ItemTextureData
        {
            public readonly Texture2D texture;
            public readonly Rectangle rectangle;
            public readonly Color color;

            public ItemTextureData(Texture2D texture, Rectangle rectangle, Color color)
            {
                this.texture = texture;
                this.rectangle = rectangle;
                this.color = color;
            }
        }

        private readonly struct Message
        {
            public readonly bool isDebug;
            public readonly int createdFrame;
            public readonly string text;
            public readonly int deletionFrame;
            public readonly Color textColor;
            public readonly Color bgColor;
            public readonly Texture2D texture;

            public Message(bool isDebug, string message, Color textColor, Color bgColor, int lastDeletionFrame, int messagesCount, Texture2D texture = null)
            {
                this.isDebug = isDebug;
                this.createdFrame = SonOfRobinGame.CurrentUpdate;
                this.text = message;
                this.textColor = textColor;
                this.bgColor = bgColor;
                this.texture = texture;

                int delay = messagesCount == 0 ? 60 * 3 : 90 / Math.Min(messagesCount, 3);

                this.deletionFrame = Math.Max(lastDeletionFrame, SonOfRobinGame.CurrentUpdate) + delay;
            }
        }

        private static readonly SpriteFontBase fontRegular = SonOfRobinGame.FontVCROSD.GetFont(21);
        private static readonly SpriteFontBase fontDebug = SonOfRobinGame.FontPressStart2P.GetFont(8);

        private readonly int marginX;
        private readonly int marginY;

        private int lastDeletionFrame;
        private string lastDebugMessage;
        private string lastUserMessage;
        private int screenHeight;
        private readonly TriSliceBG triSliceBG;

        private List<Message> messages;
        private HashSet<string> displayedStrings;

        public MessageLog() : base(inputType: InputTypes.None, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.marginX = SonOfRobinGame.platform == Platform.Desktop ? 7 : 20;
            this.marginY = SonOfRobinGame.platform == Platform.Desktop ? 2 : 5;

            this.lastDeletionFrame = 0;
            this.lastDebugMessage = "";
            this.lastUserMessage = "";

            this.triSliceBG = new TriSliceBG(
                textureLeft: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogLeft),
                textureMid: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogMid),
                textureRight: TextureBank.GetTexture(TextureBank.TextureName.TriSliceBGMessageLogRight));

            this.messages = new();
            this.displayedStrings = new();
        }

        public override void Update()
        {
            this.DeleteOldMessages();
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

            if (GetTopSceneOfType(typeof(DebugScene)) != null) maxDrawHeight = (int)DebugScene.lastTextSize.Y;

            var itemTextureDataList = new List<ItemTextureData>();

            for (int messageNo = messagesToDisplay.Count - 1; messageNo >= 0; messageNo--)
            {
                if (currentPosY >= maxDrawHeight)
                {
                    Message message = messagesToDisplay[messageNo];
                    SpriteFontBase font = message.isDebug ? fontDebug : fontRegular;

                    Vector2 textSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: message.text);
                    int bgInflateSize = message.texture != null ? 10 : 4;
                    if (message.isDebug) bgInflateSize = 2;

                    Vector2 entryPos = new Vector2(this.marginX, currentPosY);

                    Rectangle bgRect = new Rectangle(
                        x: (int)entryPos.X,
                        y: (int)entryPos.Y,
                        width: (int)textSize.X + (bgInflateSize * 4),
                        height: (int)textSize.Y + (bgInflateSize * 2));

                    Vector2 txtPos = new(entryPos.X + bgInflateSize * 2, currentPosY + bgInflateSize);

                    Rectangle textureRect = Rectangle.Empty;

                    if (message.texture != null)
                    {
                        textureRect = new Rectangle(x: (int)txtPos.X, y: (int)entryPos.Y, width: bgRect.Height, height: bgRect.Height);
                        int textureMargin = (int)(bgInflateSize * 1.5f);
                        bgRect.Width += textureRect.Width + textureMargin;
                        txtPos.X += textureRect.Width + textureMargin;
                    }

                    int entryHeight = bgRect.Height + 2;
                    currentPosY -= entryHeight;

                    float opacity = (float)Helpers.ConvertRange(oldMin: message.deletionFrame, oldMax: message.deletionFrame - 60, newMin: 0, newMax: 1, oldVal: currentFrame, clampToEdges: true);

                    bgRect.Y -= entryHeight; // moving up by the size of one entry
                    txtPos.Y -= entryHeight; // moving up by the size of one entry
                    textureRect.Y -= entryHeight; // moving up by the size of one entry

                    float flashOpacity = (float)Helpers.ConvertRange(oldMin: message.createdFrame + 20, oldMax: message.createdFrame, newMin: 0, newMax: 0.6, oldVal: currentFrame, clampToEdges: true);
                    Color bgColor = message.bgColor * 0.5f;
                    if (flashOpacity > 0 && !message.isDebug) bgColor = Helpers.Blend2Colors(firstColor: bgColor, secondColor: Color.White, firstColorOpacity: 1 - flashOpacity, secondColorOpacity: flashOpacity);

                    triSliceBG.Draw(triSliceRect: bgRect, color: bgColor * opacity);

                    if (message.texture != null)
                    {
                        Texture2D bgTexture = TextureBank.GetTexture(TextureBank.TextureName.WhiteHorizontalLine);
                        Rectangle textureHighlightRect = textureRect;
                        textureRect.Inflate(0, -1); // highlight should be a little smaller than the background

                        textureHighlightRect.Inflate(textureHighlightRect.Width / 4, 0);
                        textureRect.Inflate(0, -2); // texture should be a little smaller than the background

                        SonOfRobinGame.SpriteBatch.Draw(bgTexture, textureHighlightRect, bgTexture.Bounds, Color.White * opacity * 0.85f);
                        itemTextureDataList.Add(new ItemTextureData(texture: message.texture, rectangle: textureRect, color: Color.White * opacity));
                    }

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

            if (itemTextureDataList.Count > 0)
            {
                SonOfRobinGame.SpriteBatch.End();

                // SamplerState.LinearWrap must be turned off first, otherwise it would cause glitches on item texture edges

                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
                foreach (ItemTextureData itemTextureData in itemTextureDataList)
                {
                    Helpers.DrawTextureInsideRect(texture: itemTextureData.texture, rectangle: itemTextureData.rectangle, color: itemTextureData.color, drawTestRect: false);
                }
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public static void Add(string text, bool debugMessage = false, bool avoidDuplicates = false, Color textColor = default, Color bgColor = default, Texture2D texture = null)
        {
            if (SonOfRobinGame.MessageLog == null)
            {
                Console.WriteLine(text);
                return;
            }

            MessageLog messageLog = SonOfRobinGame.MessageLog;

            if (textColor == default) textColor = Color.White;
            if (bgColor == default) bgColor = Color.Black;

            if (avoidDuplicates)
            {
                if ((debugMessage && messageLog.lastDebugMessage == text && (!Preferences.DebugMode || messageLog.displayedStrings.Contains(text))) ||
                    (!debugMessage && messageLog.lastUserMessage == text && messageLog.displayedStrings.Contains(text))) return;
            }

            Console.WriteLine(text); // additional output
            if (debugMessage) messageLog.lastDebugMessage = text;
            else messageLog.lastUserMessage = text;

            if (debugMessage && !Preferences.DebugMode) return;

            Message message = new Message(isDebug: debugMessage, message: text, textColor: textColor, bgColor: bgColor, texture: texture, lastDeletionFrame: messageLog.lastDeletionFrame, messagesCount: messageLog.messages.Count);

            messageLog.messages.Add(message);
            messageLog.displayedStrings.Add(text);

            messageLog.lastDeletionFrame = message.deletionFrame;
        }

        private void DeleteOldMessages()
        {
            int currentUpdate = SonOfRobinGame.CurrentUpdate;
            var filteredMessages = this.messages.Where(message => currentUpdate < message.deletionFrame);
            if (!Preferences.DebugMode) filteredMessages = filteredMessages.Where(message => !message.isDebug);

            this.messages = filteredMessages.ToList();

            this.displayedStrings.Clear();
            this.displayedStrings = filteredMessages.Select(m => m.text).ToHashSet();
        }
    }
}