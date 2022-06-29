using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Input
    {
        public enum TipsTypeToShow { Gamepad, Keyboard }

        private static bool localInputActive = true;
        private static bool globalInputActive = true;
        private static int globalInputReactivateUpdate = 0;
        public static TipsTypeToShow tipsTypeToShow = TipsTypeToShow.Gamepad;

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

            Mouse.GetState(); // not really used
            Keyboard.GetState();
            GamePad.GetPreviousState(PlayerIndex.One);
            TouchInput.GetState(gameTime: gameTime);

            globalInputActive = savedGlobalInput;
            localInputActive = savedLocalInput;

            RefreshTipsType();
            ReactivateGlobalInput();
        }

        private static void RefreshTipsType()
        {
            if (SonOfRobinGame.currentUpdate % 30 != 0) return;

            TipsTypeToShow prevTipsType = tipsTypeToShow;
            if (tipsTypeToShow != TipsTypeToShow.Keyboard && Keyboard.CurrentKeyState.GetPressedKeys().Length != 0) tipsTypeToShow = TipsTypeToShow.Keyboard;

            if (tipsTypeToShow != TipsTypeToShow.Gamepad)
            {
                var padState = Microsoft.Xna.Framework.Input.GamePad.GetState(index: 0, deadZoneMode: GamePadDeadZone.Circular);
                var defaultState = GamePadState.Default;

                if (padState.ThumbSticks != defaultState.ThumbSticks ||
                    padState.DPad != defaultState.DPad ||
                    padState.Buttons != defaultState.Buttons ||
                    padState.Triggers != defaultState.Triggers) tipsTypeToShow = TipsTypeToShow.Gamepad;
            }

            if (prevTipsType != tipsTypeToShow)
            {
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
