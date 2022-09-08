using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Input
    {
        public enum ControlType { Gamepad, KeyboardAndMouse, Touch }

        private static bool localInputActive = true;
        private static bool globalInputActive = true;
        private static int globalInputReactivateUpdate = 0;
        public static ControlType currentControlType { get; private set; } = ControlType.Touch;

        public static bool InputActive { get { return localInputActive == true && globalInputActive == true; } set { localInputActive = value; } }
        public static bool GlobalInputActive
        {
            get { return globalInputActive; }
            set
            {
                globalInputActive = value;
                if (!value) globalInputReactivateUpdate = SonOfRobinGame.currentUpdate + 600;
            }
        }

        public static void UpdateInput(GameTime gameTime)
        {
            bool savedGlobalInput = globalInputActive;
            bool savedLocalInput = localInputActive;

            InputActive = true; // must be true, to capture all input correctly at the start

            Mouse.GetState();
            Keyboard.GetState();
            GamePad.GetPreviousState(PlayerIndex.One);
            TouchInput.GetState(gameTime: gameTime);
            RefreshTipsType();

            globalInputActive = savedGlobalInput;
            localInputActive = savedLocalInput;

            ReactivateGlobalInput();
        }

        private static void RefreshTipsType()
        {
            if (SonOfRobinGame.currentUpdate % 12 != 0) return;

            ControlType prevControlType = currentControlType;

            if (currentControlType != ControlType.KeyboardAndMouse)
            {
                bool keyboardOrMousePressed = false;

                if (Keyboard.CurrentKeyState.GetPressedKeys().Length != 0)
                {
                    keyboardOrMousePressed = true;
                }
                else
                {
                    MouseState currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
                    if (SonOfRobinGame.platform == Platform.Desktop &&
                        (currentMouseState.LeftButton == ButtonState.Pressed ||
                        currentMouseState.MiddleButton == ButtonState.Pressed ||
                        currentMouseState.RightButton == ButtonState.Pressed))
                        keyboardOrMousePressed = true;
                }

                if (keyboardOrMousePressed) currentControlType = ControlType.KeyboardAndMouse;
            }

            if (currentControlType != ControlType.Gamepad)
            {
                var padState = Microsoft.Xna.Framework.Input.GamePad.GetState(index: 0, deadZoneMode: GamePadDeadZone.Circular);
                var defaultState = GamePadState.Default;

                if (padState.ThumbSticks != defaultState.ThumbSticks ||
                    padState.DPad != defaultState.DPad ||
                    padState.Buttons != defaultState.Buttons ||
                    padState.Triggers != defaultState.Triggers) currentControlType = ControlType.Gamepad;
            }

            if (currentControlType != ControlType.Touch)
            {
                if (SonOfRobinGame.platform == Platform.Mobile && TouchInput.IsBeingTouchedInAnyWay) currentControlType = ControlType.Touch;
            }

            if (prevControlType != currentControlType)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Switching control type to {currentControlType}");

                InputVis.Refresh();
                Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to refresh everything that is connected to tips type
            }
        }

        private static void ReactivateGlobalInput()
        {
            // globalInputActive should only be turned off temporarily 

            if (GlobalInputActive) return;
            if (SonOfRobinGame.currentUpdate >= globalInputReactivateUpdate)
            {
                GlobalInputActive = true;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "GlobalInputActive had to be restored.", color: Color.White);
            }
        }
    }

}
