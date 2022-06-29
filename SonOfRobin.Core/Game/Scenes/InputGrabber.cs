using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class InputGrabber : Scene
    {
        public static readonly List<Buttons> allButtons = new List<Buttons> { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.Start, Buttons.Back, Buttons.LeftStick, Buttons.RightStick, Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp, Buttons.DPadDown };
        public static readonly List<Keys> allKeys = KeyboardScheme.KeyTextures.Keys.ToList();

        private readonly bool grabButtons;
        private readonly bool grabKeys;

        private readonly object targetObj;
        private readonly string targetPropertyName;

        private int waitFramesLeft;
        public static bool GrabberIsActive
        { get { return GetTopSceneOfType(typeof(InputGrabber)) != null; } }

        public InputGrabber(object targetObj, string targetPropertyName, bool grabButtons, bool grabKeys, int waitFramesLeft = 60 * 8) : base(inputType: InputTypes.Normal, priority: -1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.CaptureInputCancel, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.waitFramesLeft = waitFramesLeft;

            this.grabButtons = grabButtons;
            this.grabKeys = grabKeys;

            this.targetObj = targetObj;
            this.targetPropertyName = targetPropertyName;

            if (!this.grabButtons && !this.grabKeys) throw new ArgumentException("No input source is selected.");
        }

        public override void Update(GameTime gameTime)
        {
            // virt button is needed for exiting in touch mode (when no other input is available)

            if (VirtButton.HasButtonBeenPressed(VButName.Return))
            {
                this.Remove();
                return;
            }

            // checking for buttons
            if (this.grabButtons)
            {
                foreach (Buttons button in allButtons)
                {
                    if (GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: button))
                    {
                        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"button grabbed {button}");
                        Helpers.SetProperty(targetObj: this.targetObj, propertyName: this.targetPropertyName, newValue: button);
                        this.Remove();
                        return;
                    }
                }
            }

            // checking for keys
            if (this.grabKeys)
            {
                foreach (Keys key in allKeys)
                {
                    if (Keyboard.HasBeenPressed(key))
                    {
                        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"key grabbed {key}");
                        Helpers.SetProperty(targetObj: this.targetObj, propertyName: this.targetPropertyName, newValue: key);
                        this.Remove();
                        return;
                    }
                }
            }

            // checking for timeout
            this.waitFramesLeft--;
            if (this.waitFramesLeft == 0) this.Remove();
        }

        public override void Draw()
        { }
    }
}
