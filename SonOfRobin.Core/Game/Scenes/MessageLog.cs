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
        private readonly struct Message
        {
            public readonly string text;
            public readonly int deletionFrame;
            public readonly Color color;

            public Message(string message, Color color)
            {
                this.text = message;
                this.color = color;

                int delay = (messages.Count == 0) ? 180 : 90 / Math.Min(messages.Count, 3);

                this.deletionFrame = Math.Max(lastDeletionFrame, SonOfRobinGame.CurrentUpdate) + delay;
                lastDeletionFrame = this.deletionFrame;
            }
        }

        private static int lastDeletionFrame = 0;
        private static readonly SpriteFont font = SonOfRobinGame.FontPressStart2P5;
        private const int txtSeparator = 3;
        private const int freePixelsAboveMessages = 160;
        private static readonly TimeSpan maxDuplicateCheckDuration = TimeSpan.FromSeconds(20);
        private static DateTime lastDebugMessageAdded = DateTime.MinValue;
        private static DateTime lastUserMessageAdded = DateTime.MinValue;
        private static string lastDebugMessage = "";
        private static string lastUserMessage = "";

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
                string currentLineOfText = message.text;

                Vector2 txtSize = font.MeasureString(currentLineOfText);
                currentOffsetY += (int)txtSize.Y + txtSeparator;

                if (this.screenHeight - currentOffsetY >= freePixelsAboveMessages)
                {
                    Vector2 txtPos = new(this.marginX, Convert.ToInt16(this.screenHeight - (currentOffsetY + this.marginY)));

                    float textOpacity = Math.Clamp(value: (float)(message.deletionFrame - currentFrame), min: 0, max: 1);
                    float outlineOpacity = (textOpacity == 1) ? 1 : textOpacity / 4;

                    Helpers.DrawTextWithOutline(font: font, text: currentLineOfText, pos: txtPos, color: message.color * textOpacity, outlineColor: Color.Black * outlineOpacity, outlineSize: 1);
                }
                else break;
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public static void AddMessage(string message, bool debugMessage = false, bool avoidDuplicates = false)
        {
            AddMessage(message: message, color: Color.White, debugMessage: debugMessage, avoidDuplicates: avoidDuplicates);
        }

        public static void AddMessage(string message, Color color, bool debugMessage = false, bool avoidDuplicates = false)
        {
            if (avoidDuplicates)
            {
                if ((debugMessage && lastDebugMessage == message && DateTime.Now - lastDebugMessageAdded < maxDuplicateCheckDuration) ||
                    (!debugMessage && lastUserMessage == message && DateTime.Now - lastUserMessageAdded < maxDuplicateCheckDuration)) return;
            }

            Console.WriteLine(message); // additional output
            if (debugMessage)
            {
                lastDebugMessage = message;
                lastDebugMessageAdded = DateTime.Now;
            }
            else
            {
                lastUserMessage = message;
                lastUserMessageAdded = DateTime.Now;
            }

            if (debugMessage && !Preferences.DebugMode) return;

            messages.Add(new Message(message: message, color: color));
        }

        private static void DeleteOldMessages(int currentFrame)
        {
            messages = messages.Where(message => currentFrame < message.deletionFrame).ToList();
        }
    }
}