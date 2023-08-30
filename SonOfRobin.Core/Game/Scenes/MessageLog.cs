using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public enum MsgType : byte
    {
        Debug,
        User,
    }

    public class MessageLog : Scene
    {
        private struct Message
        {
            public readonly string text;
            public readonly int deletionFrame;
            public readonly Color color;
            public readonly MsgType msgType;
            private static int lastDeletionFrame = 0;

            public Message(string message, Color color, MsgType msgType)
            {
                Console.WriteLine(message); // additional output

                this.text = message;
                this.color = color;
                this.msgType = msgType;

                int delay = (MessageLog.messages.Count == 0) ? 180 : 90 / Math.Min(MessageLog.messages.Count, 3);

                this.deletionFrame = Math.Max(lastDeletionFrame, SonOfRobinGame.CurrentUpdate) + delay;
                lastDeletionFrame = this.deletionFrame;
            }
        }

        private static readonly SpriteFont font = SonOfRobinGame.FontPressStart2P5;
        private const int txtSeparator = 3;
        private const int freePixelsAboveMessages = 160;

        private static readonly HashSet<MsgType> displayedLevelsTemplateDebug = new() { MsgType.User, MsgType.Debug };
        private static readonly HashSet<MsgType> displayedLevelsTemplateUser = new() { MsgType.User };

        private static HashSet<MsgType> DisplayedLevels
        { get { return Preferences.DebugMode ? displayedLevelsTemplateDebug : displayedLevelsTemplateUser; } }

        private static List<Message> messages = new() { };

        private readonly int marginX, marginY;
        private int screenHeight;

        public MessageLog() : base(inputType: InputTypes.None, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.marginX = SonOfRobinGame.platform == Platform.Desktop ? 7 : 20;
            this.marginY = SonOfRobinGame.platform == Platform.Desktop ? 2 : 5;
        }

        public override void Update()
        {
            int currentFrame = SonOfRobinGame.CurrentUpdate;
            DeleteOldMessages(currentFrame);
            this.screenHeight = Preferences.ShowControlTips ? (int)(SonOfRobinGame.VirtualHeight * 0.94f) : (int)(SonOfRobinGame.VirtualHeight * 1f);
        }

        public override void Draw()
        {
            if (messages.Count == 0) return;

            List<Message> messagesToDisplay;

            try
            { messagesToDisplay = messages.ToList(); }
            catch (ArgumentException) { return; } // if some background process tries to add message right now

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            messagesToDisplay.Reverse();

            int currentFrame = SonOfRobinGame.CurrentUpdate;
            int currentOffsetY = 0;

            foreach (Message message in messagesToDisplay)
            {
                if (!DisplayedLevels.Contains(message.msgType)) continue;

                string currentLineOfText = message.text;

                Vector2 txtSize = font.MeasureString(currentLineOfText);
                currentOffsetY += (int)txtSize.Y + txtSeparator;

                if (this.screenHeight - currentOffsetY >= freePixelsAboveMessages)
                {
                    Vector2 txtPos = new(this.marginX, Convert.ToInt16(this.screenHeight - (currentOffsetY + this.marginY)));

                    float textOpacity = Math.Min(Math.Max((float)(message.deletionFrame - currentFrame) / 30f, 0), 1);
                    float outlineOpacity = (textOpacity == 1) ? 1 : textOpacity / 4;

                    Helpers.DrawTextWithOutline(font: font, text: currentLineOfText, pos: txtPos, color: message.color * textOpacity, outlineColor: Color.Black * outlineOpacity, outlineSize: 1);
                }
                else break;
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        private static bool CheckIfDuplicate(string text)
        {
            foreach (Message message in messages)
            {
                if (message.text == text) return true;
            }

            return false;
        }

        public static void AddMessage(string message, Color color, MsgType msgType, bool avoidDuplicates = false)
        {
            if ((avoidDuplicates && CheckIfDuplicate(message)) || !DisplayedLevels.Contains(msgType)) return;

            messages.Add(new Message(message: message, color: color, msgType: msgType));
        }

        public static void AddMessage(string message, MsgType msgType, bool avoidDuplicates = false)
        {
            if ((avoidDuplicates && CheckIfDuplicate(message)) || !DisplayedLevels.Contains(msgType)) return;

            messages.Add(new Message(message: message, color: Color.White, msgType: msgType));
        }

        private static void DeleteOldMessages(int currentFrame)
        {
            messages = messages.Where(message => currentFrame < message.deletionFrame).ToList();
        }
    }
}