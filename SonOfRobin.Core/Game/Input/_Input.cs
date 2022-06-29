using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class Input
    {
        private static bool localInputActive = true;
        private static bool globalInputActive = true;
        private static int globalInputReactivateUpdate = 0;

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

            ReactivateGlobalInput();
        }


        private static void ReactivateGlobalInput()
        {
            // globalInputActive should only be turned off temporarily 

            if (GlobalInputActive) return;
            if (SonOfRobinGame.currentUpdate >= globalInputReactivateUpdate)
            {
                GlobalInputActive = true;
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "GlobalInputActive had to be restored).", color: Color.White);
            }
        }

    }

}
