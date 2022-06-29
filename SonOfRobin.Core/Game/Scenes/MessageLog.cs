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
        struct DebugMessage
        {
            public readonly string text;
            public readonly int deletionFrame;
            public readonly Color color;
            public readonly MsgType msgType;
            private static int lastDeletionFrame = 0;

            public DebugMessage(string message, int currentFrame, Color color, MsgType msgType)
            {
                Console.WriteLine(message); // additional output

                this.text = message;
                this.color = color;
                this.msgType = msgType;

                int delay = (MessageLog.messages.Count == 0) ? 180 : 90 / Math.Min(MessageLog.messages.Count, 3);

                this.deletionFrame = Math.Max(lastDeletionFrame, currentFrame) + delay;
                lastDeletionFrame = this.deletionFrame;
            }
        }

        private static readonly SpriteFont font = SonOfRobinGame.fontSmall;
        private static readonly int margin = 5;
        private static readonly int txtSeparator = 3;
        private static readonly int freePixelsAboveMessages = 140;

        private static readonly List<MsgType> displayedLevelsTemplateDebug = new List<MsgType> { MsgType.User, MsgType.Debug };
        private static readonly List<MsgType> displayedLevelsTemplateUser = new List<MsgType> { MsgType.User };

        private static List<MsgType> DisplayedLevels { get { return Preferences.DebugMode ? displayedLevelsTemplateDebug : displayedLevelsTemplateUser; } }

        private static List<DebugMessage> messages = new List<DebugMessage> { };

        public MessageLog() : base(inputType: InputTypes.None, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: true, touchLayout: TouchLayout.Empty)
        { }

        public override void Update(GameTime gameTime)
        { }

        public override void Draw()
        {
            int currentFrame = SonOfRobinGame.currentUpdate;

            if (messages.Count == 0) return;

            DeleteOldMessages(currentFrame);

            var messagesToDisplay = messages.ToList();
            messagesToDisplay.Reverse();

            int currentOffsetY = 0;

            foreach (DebugMessage debugMessage in messagesToDisplay)
            {
                if (!DisplayedLevels.Contains(debugMessage.msgType)) continue;

                string currentLineOfText = debugMessage.text;

                Vector2 txtSize = font.MeasureString(currentLineOfText);
                currentOffsetY += (int)txtSize.Y + txtSeparator;

                if (SonOfRobinGame.VirtualHeight - currentOffsetY < freePixelsAboveMessages) return;

                Vector2 txtPos = new Vector2(margin, Convert.ToInt16(SonOfRobinGame.VirtualHeight - (currentOffsetY + margin)));

                float textOpacity = Math.Min(Math.Max((float)(debugMessage.deletionFrame - currentFrame) / 30f, 0), 1);
                float outlineOpacity = (textOpacity == 1) ? 1 : textOpacity / 4;

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    { SonOfRobinGame.spriteBatch.DrawString(font, currentLineOfText, txtPos + new Vector2(x, y), Color.Black * outlineOpacity); }
                }

                SonOfRobinGame.spriteBatch.DrawString(font, currentLineOfText, txtPos, debugMessage.color * textOpacity);

            }
        }
        public static void AddMessage(int currentFrame, string message, Color color, MsgType msgType)
        { messages.Add(new DebugMessage(currentFrame: currentFrame, message: message, color: color, msgType: msgType)); }

        public static void AddMessage(int currentFrame, string message, MsgType msgType)
        { messages.Add(new DebugMessage(currentFrame: currentFrame, message: message, color: Color.White, msgType: msgType)); }

        private static void DeleteOldMessages(int currentFrame)
        { messages = messages.Where(debugMessage => currentFrame < debugMessage.deletionFrame).ToList(); }
    }

}
