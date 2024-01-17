using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public readonly struct HintMessage
    {
        public enum BoxType : byte
        {
            Dialogue,
            InvertedDialogue,
            GreenBox,
            BlueBox,
            LightBlueBox,
            RedBox,
            GoldBox,
        }

        public readonly string text;
        public readonly List<ImageObj> imageList;
        public readonly BoxType boxType;
        public readonly int delay;
        public readonly bool fieldOnly;
        public readonly int blockInputDuration;
        public readonly bool animate;
        public readonly bool useTransition;
        public readonly bool autoClose;
        public readonly bool noInput;
        public readonly SoundData.Name startingSound;

        public HintMessage(string text, int delay = 1, bool fieldOnly = false, bool blockInputDefaultDuration = false, int blockInputDuration = 0, List<ImageObj> imageList = null, BoxType boxType = BoxType.Dialogue, bool animate = true, bool useTransition = false, bool autoClose = false, bool noInput = false, SoundData.Name startingSound = SoundData.Name.Empty)
        {
            this.text = text;
            this.imageList = imageList == null ? new List<ImageObj>() : imageList;
            this.boxType = boxType;
            this.delay = delay;
            this.fieldOnly = fieldOnly;
            if (blockInputDefaultDuration) blockInputDuration = HintEngine.blockInputDuration;
            this.blockInputDuration = blockInputDuration;
            this.animate = animate;
            this.useTransition = useTransition;
            this.startingSound = startingSound;
            this.autoClose = autoClose;
            this.noInput = noInput;

            this.ValidateImagesCount();
        }

        private void ValidateImagesCount()
        {
            MatchCollection matches = Regex.Matches(this.text, $@"\{TextWithImages.imageMarker}"); // $@ is needed for "\" character inside interpolated string

            if (this.imageList.Count != matches.Count) throw new ArgumentException($"HintMessage - count of markers ({matches.Count}) and images ({this.imageList.Count}) does not match.\n{this.text}");
        }

        public Scheduler.Task ConvertToTask(bool useTransitionOpen = false, bool useTransitionClose = false, bool playSound = true, bool storeForLaterUse = true)
        {
            Color bgColor, textColor;

            switch (this.boxType)
            {
                case BoxType.Dialogue:
                    bgColor = Color.White;
                    textColor = Color.Black;
                    break;

                case BoxType.InvertedDialogue:
                    bgColor = Color.Black;
                    textColor = Color.White;
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

                case BoxType.GoldBox:
                    bgColor = Color.DarkGoldenrod;
                    textColor = Color.PaleGoldenrod;
                    break;

                default:
                    { throw new ArgumentException($"Unsupported hint boxType - {boxType}."); }
            }

            Sound animSound = this.boxType == BoxType.Dialogue || this.boxType == BoxType.InvertedDialogue ? World.GetTopWorld().DialogueSound : null;

            var textWindowData = new Dictionary<string, Object> {
                { "text", this.text },
                { "imageList", this.imageList },
                { "bgColor", new List<byte> { bgColor.R, bgColor.G, bgColor.B } },
                { "textColor", new List<byte> { textColor.R, textColor.G, textColor.B }  },
                { "checkForDuplicate", true },
                { "animate", this.animate },
                { "useTransition", this.useTransition },
                { "useTransitionOpen", useTransitionOpen },
                { "useTransitionClose", useTransitionClose },
                { "blocksUpdatesBelow", false },
                { "startingSound", this.startingSound },
                { "animSound", animSound },
                { "autoClose", this.autoClose },
                { "noInput", this.noInput },
                { "blockInputDuration", this.blockInputDuration }
            };

            if (!playSound) textWindowData.Remove("sound");

            return new Scheduler.Task(taskName: Scheduler.TaskName.ShowTextWindow, turnOffInputUntilExecution: true, delay: this.delay, executeHelper: textWindowData, storeForLaterUse: storeForLaterUse);
        }

        public static List<Object> ConvertToTasks(List<HintMessage> messageList, bool playSounds = true)
        {
            var taskChain = new List<Object> { };

            int counter = 0;
            bool useTransitionOpen, useTransitionClose;

            foreach (HintMessage message in messageList)
            {
                useTransitionOpen = message.useTransition && counter == 0;
                useTransitionClose = message.useTransition && counter + 1 == messageList.Count;

                taskChain.Add(message.ConvertToTask(useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose, playSound: playSounds));

                counter++;
            }

            return taskChain;
        }
    }
}