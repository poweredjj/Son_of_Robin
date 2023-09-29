using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Input
    {
        public enum ControlType : byte
        {
            Gamepad,
            KeyboardAndMouse,
            Touch,
        }

        private static bool localInputActive = true;
        private static bool globalInputActive = true;
        private static int globalInputReactivateUpdate = 0;
        public static ControlType CurrentControlType { get; private set; } = SonOfRobinGame.platform == Platform.Mobile ? ControlType.Touch : ControlType.KeyboardAndMouse;

        public static bool InputActive
        { get { return localInputActive == true && globalInputActive == true; } set { localInputActive = value; } }

        public static bool GlobalInputActive
        {
            get { return globalInputActive; }
            set
            {
                globalInputActive = value;
                if (!value) globalInputReactivateUpdate = SonOfRobinGame.CurrentUpdate + 600;
            }
        }

        public static void UpdateInput()
        {
            bool savedGlobalInput = globalInputActive;
            bool savedLocalInput = localInputActive;

            InputActive = true; // must be true, to capture all input correctly at the start

            Mouse.GetState();
            Keyboard.GetState();
            GamePad.GetPreviousState(PlayerIndex.One);
            TouchInput.GetState();
            RefreshControlType();
            RumbleManager.Update();
            SonOfRobinGame.Game.IsMouseVisible = CurrentControlType != ControlType.Gamepad;

            globalInputActive = savedGlobalInput;
            localInputActive = savedLocalInput;

            ReactivateGlobalInput();
        }

        private static void RefreshControlType()
        {
            if (SonOfRobinGame.CurrentUpdate % 12 != 0) return;

            ControlType prevControlType = CurrentControlType;

            if (CurrentControlType != ControlType.KeyboardAndMouse)
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

                if (keyboardOrMousePressed) CurrentControlType = ControlType.KeyboardAndMouse;
            }

            if (CurrentControlType != ControlType.Gamepad)
            {
                var padState = Microsoft.Xna.Framework.Input.GamePad.GetState(index: 0, deadZoneMode: GamePadDeadZone.Circular);
                var defaultState = GamePadState.Default;

                if (padState.ThumbSticks != defaultState.ThumbSticks ||
                    padState.DPad != defaultState.DPad ||
                    padState.Buttons != defaultState.Buttons ||
                    padState.Triggers != defaultState.Triggers) CurrentControlType = ControlType.Gamepad;
            }

            if (CurrentControlType != ControlType.Touch)
            {
                if (Preferences.EnableTouchButtons && TouchInput.IsBeingTouchedInAnyWay) CurrentControlType = ControlType.Touch;
            }

            if (prevControlType != CurrentControlType)
            {
                MessageLog.Add(debugMessage: true, text: $"Switching control type to {CurrentControlType}");

                InputVis.Refresh();
                Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to refresh everything that is connected to tips type
            }
        }

        private static void ReactivateGlobalInput()
        {
            // globalInputActive should only be turned off temporarily

            if (GlobalInputActive) return;
            if (SonOfRobinGame.CurrentUpdate >= globalInputReactivateUpdate)
            {
                GlobalInputActive = true;
                MessageLog.Add(debugMessage: true, text: "GlobalInputActive had to be restored.", textColor: Color.White);
            }
        }
    }
}