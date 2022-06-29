using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public enum MsgType
    {
        Debug,
        User
    }
    public class MessageLog : Scene
    {
        struct Message
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

                this.deletionFrame = Math.Max(lastDeletionFrame, SonOfRobinGame.currentUpdate) + delay;
                lastDeletionFrame = this.deletionFrame;
            }
        }

        private static readonly SpriteFont font = SonOfRobinGame.fontPressStart2P5;
        private static readonly int txtSeparator = 3;
        private static readonly int freePixelsAboveMessages = 160;

        private static readonly List<MsgType> displayedLevelsTemplateDebug = new List<MsgType> { MsgType.User, MsgType.Debug };
        private static readonly List<MsgType> displayedLevelsTemplateUser = new List<MsgType> { MsgType.User };
        private static List<MsgType> DisplayedLevels { get { return Preferences.DebugMode ? displayedLevelsTemplateDebug : displayedLevelsTemplateUser; } }

        private static List<Message> messages = new List<Message> { };

        private int marginX, marginY;
        private int screenHeight;

        public MessageLog() : base(inputType: InputTypes.None, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.marginX = SonOfRobinGame.platform == Platform.Desktop ? 7 : 20;
            this.marginY = SonOfRobinGame.platform == Platform.Desktop ? 2 : 5;
        }

        public override void Update(GameTime gameTime)
        {
            int currentFrame = SonOfRobinGame.currentUpdate;
            DeleteOldMessages(currentFrame);
            this.screenHeight = Preferences.ShowControlTips ? (int)(SonOfRobinGame.VirtualHeight * 0.94f) : (int)(SonOfRobinGame.VirtualHeight * 1f);
        }

        public override void Draw()
        {
            if (messages.Count == 0) return;

            int currentFrame = SonOfRobinGame.currentUpdate;

            var messagesToDisplay = messages.ToList();
            messagesToDisplay.Reverse();

            int currentOffsetY = 0;

            foreach (Message message in messagesToDisplay)
            {
                if (!DisplayedLevels.Contains(message.msgType)) continue;

                string currentLineOfText = message.text;

                Vector2 txtSize = font.MeasureString(currentLineOfText);
                currentOffsetY += (int)txtSize.Y + txtSeparator;

                if (this.screenHeight - currentOffsetY < freePixelsAboveMessages) return;

                Vector2 txtPos = new Vector2(this.marginX, Convert.ToInt16(this.screenHeight - (currentOffsetY + this.marginY)));

                float textOpacity = Math.Min(Math.Max((float)(message.deletionFrame - currentFrame) / 30f, 0), 1);
                float outlineOpacity = (textOpacity == 1) ? 1 : textOpacity / 4;

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    { SonOfRobinGame.spriteBatch.DrawString(font, currentLineOfText, txtPos + new Vector2(x, y), Color.Black * outlineOpacity); }
                }

                SonOfRobinGame.spriteBatch.DrawString(font, currentLineOfText, txtPos, message.color * textOpacity);
            }
        }
        public static void AddMessage(string message, Color color, MsgType msgType)
        { if (DisplayedLevels.Contains(msgType)) messages.Add(new Message(message: message, color: color, msgType: msgType)); }

        public static void AddMessage(string message, MsgType msgType)
        { if (DisplayedLevels.Contains(msgType)) messages.Add(new Message(message: message, color: Color.White, msgType: msgType)); }

        private static void DeleteOldMessages(int currentFrame)
        { messages = messages.Where(message => currentFrame < message.deletionFrame).ToList(); }
    }

}
