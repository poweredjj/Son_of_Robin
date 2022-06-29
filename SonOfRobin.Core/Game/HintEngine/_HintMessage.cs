using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public struct HintMessage
    {
        public enum BoxType { Dialogue, GreenBox, BlueBox, LightBlueBox, RedBox }

        public readonly string text;
        public readonly BoxType boxType;
        public readonly int delay;
        public readonly bool fieldOnly;

        public HintMessage(string text) // shortened version for dialogue
        {
            this.text = text;
            this.boxType = BoxType.Dialogue;
            this.delay = 1;
            this.fieldOnly = false;
        }

        public HintMessage(string text, BoxType boxType, int delay = 1, bool fieldOnly = false)
        {
            this.text = text;
            this.boxType = boxType;
            this.delay = delay;
            this.fieldOnly = fieldOnly;
        }

        public HintMessage(string text, int delay = 1, bool fieldOnly = false)
        {
            this.text = text;
            this.boxType = BoxType.Dialogue;
            this.delay = delay;
            this.fieldOnly = fieldOnly;
        }

        public Scheduler.Task ConvertToTask(bool useTransitionOpen = false, bool useTransitionClose = false)
        {
            Color bgColor, textColor;

            switch (this.boxType)
            {
                case BoxType.Dialogue:
                    bgColor = Color.White;
                    textColor = Color.Black;
                    break;

                case BoxType.GreenBox:
                    bgColor = Color.Green;
                    textColor = Color.White;
                    break;

                case BoxType.BlueBox:
                    bgColor = Color.Blue;
                    textColor = Color.White;
                    break;

                case BoxType.LightBlueBox:
                    bgColor = Color.DodgerBlue;
                    textColor = Color.White;
                    break;

                case BoxType.RedBox:
                    bgColor = Color.DarkRed;
                    textColor = Color.White;
                    break;

                default:
                    { throw new DivideByZeroException($"Unsupported hint boxType - {boxType}."); }
            }

            var textWindowData = new Dictionary<string, Object> {
                { "text", this.text },
                { "bgColor", new List<byte> {bgColor.R, bgColor.G, bgColor.B } },
                { "textColor", new List<byte> {textColor.R, textColor.G, textColor.B }  },
                { "checkForDuplicate", true },
                { "useTransition", false },
                { "useTransitionOpen", useTransitionOpen },
                { "useTransitionClose", useTransitionClose },
                { "blocksUpdatesBelow", false },
                { "blockInputDuration", HintEngine.blockInputDuration }
            };

            return new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.OpenTextWindow, turnOffInputUntilExecution: true, delay: this.delay, executeHelper: textWindowData, storeForLaterUse: true);
        }

        public static List<Object> ConvertToTasks(List<HintMessage> messageList)
        {
            var taskChain = new List<Object> { };

            int counter = 0;
            bool useTransitionOpen, useTransitionClose;
            foreach (HintMessage message in messageList)
            {
                useTransitionOpen = counter == 0;
                useTransitionClose = counter + 1 == messageList.Count;

                taskChain.Add(message.ConvertToTask(useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose));

                counter++;
            }

            return taskChain;
        }
    }
}
