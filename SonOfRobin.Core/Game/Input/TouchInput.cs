using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Studies.Joystick.Input;
using System;
using System.Linq;


namespace SonOfRobin
{
    public enum TouchLayout
    {
        Uninitialized, // initial layout for TouchOverlay
        Empty,
        MenuRightReturn,
        MenuMiddleReturn,
        MenuLeftReturn,
        WorldMain,
        WorldShoot,
        WorldSleep,
        Inventory,
        Map,
    }
    public class TouchInput
    {
        public static TouchLayout currentLayout;

        private static float stickScale = -100; // dummy value
        private static int screenWidth = -100;
        private static int screenHeight = -100;
        public static bool showSticks = false;

        public static DualStick dualStick;

        private static Vector2 leftStick = new Vector2(0, 0);
        private static Vector2 rightStick = new Vector2(0, 0);

        private static Vector2 emptyStick = new Vector2(0, 0);
        public static Vector2 LeftStick { get { return Input.InputActive ? leftStick : emptyStick; } }
        public static Vector2 RightStick { get { return Input.InputActive ? rightStick : emptyStick; } }

        private static TouchCollection touchPanelState = new TouchCollection { };
        private static readonly TouchCollection emptyTouchList = new TouchCollection { };
        public static TouchCollection TouchPanelState { get { return Input.InputActive ? touchPanelState : emptyTouchList; } }
        public static bool IsGestureAvailable { get { return Input.InputActive ? TouchPanel.IsGestureAvailable : false; } } // should be used before reading gesture directly

        public static bool IsStateAvailable(TouchLocationState state)
        {
            var matchingTypes = TouchPanelState.Where(touch => touch.State == state).ToList();
            return matchingTypes.Count > 0;
        }

        public static void GetState(GameTime gameTime)
        {
            if (SonOfRobinGame.platform != Platform.Mobile) return;
            Refresh();

            touchPanelState = TouchPanel.GetState();

            dualStick.Update(gameTime);

            leftStick = dualStick.LeftStick.GetRelativeVector(dualStick.aliveZoneSize);
            rightStick = dualStick.RightStick.GetRelativeVector(dualStick.aliveZoneSize);

            VirtButton.UpdateAll();
        }

        private static void Refresh()
        {
            if (stickScale == Preferences.globalScale && screenWidth == SonOfRobinGame.graphics.PreferredBackBufferWidth && screenHeight == SonOfRobinGame.graphics.PreferredBackBufferHeight) return;

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Changing touch sticks scale from {stickScale} to {Preferences.globalScale}.", color: Color.White);

            TouchPanel.EnableMouseGestures = SonOfRobinGame.fakeMobileMode;
            TouchPanel.EnableMouseTouchPoint = SonOfRobinGame.fakeMobileMode;

            dualStick = new DualStick(aliveZoneSize: TouchPanel.DisplayHeight * 0.25f, deadZoneSize: 16f); // DualStick accepts touch panel size values
            dualStick.LeftStick.SetAsFixed();
            dualStick.RightStick.SetAsFixed();

            stickScale = Preferences.globalScale;
            screenWidth = SonOfRobinGame.graphics.PreferredBackBufferWidth;
            screenHeight = SonOfRobinGame.graphics.PreferredBackBufferHeight;
        }

        public static void SwitchToLayout(TouchLayout touchLayout)
        {
            if (SonOfRobinGame.platform != Platform.Mobile || touchLayout == currentLayout) return;

            currentLayout = touchLayout;

            VirtButton.RemoveAll();

            if (Preferences.DebugMode) AddDebugButtons();

            float size = 0.085f;
            float xPos, yPos, xShift, yShift;

            switch (touchLayout)
            {
                case TouchLayout.Empty:
                    showSticks = false;
                    return;

                case TouchLayout.WorldMain:
                    showSticks = true;
                    xShift = 0.09f;
                    yShift = 0.20f;

                    // right side

                    xPos = 0.76f;
                    yPos = 0.12f;

                    new VirtButton(name: VButName.Map, label: "MAP", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);
                    xPos += xShift;
                    new VirtButton(name: VButName.ZoomOut, label: "ZOOM\nOUT", colorPressed: Color.Orange, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, switchButton: true);
                    xPos += xShift;
                    new VirtButton(name: VButName.Run, label: "RUN", colorPressed: Color.Red, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);
                    yPos += yShift;
                    new VirtButton(name: VButName.Interact, label: "INTERACT", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);
                    xPos -= xShift;
                    new VirtButton(name: VButName.UseTool, label: "USE\nITEM", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);
                    xPos -= xShift;
                    new VirtButton(name: VButName.PickUp, label: "PICK\nUP", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                    // left side

                    xPos = 0.06f;
                    yPos = 0.12f;

                    new VirtButton(name: VButName.Inventory, label: "ITEMS", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);
                    xPos += xShift;
                    new VirtButton(name: VButName.FieldCraft, label: "FIELD\nCRAFT", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);
                    xPos += xShift;
                    new VirtButton(name: VButName.PauseMenu, label: "MENU", colorPressed: Color.Yellow, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                    return;

                case TouchLayout.WorldShoot:
                    showSticks = true;

                    new VirtButton(name: VButName.Shoot, label: "SHOOT", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: 0.06f, posY0to1: 0.32f, width0to1: size, height0to1: size);

                    return;

                case TouchLayout.WorldSleep:
                    showSticks = false;

                    new VirtButton(name: VButName.UseTool, label: "WAKE UP", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: 0.94f, posY0to1: 0.32f, width0to1: size, height0to1: size);

                    return;

                case TouchLayout.Map:
                    showSticks = false;

                    xPos = 0.76f;
                    yPos = 0.12f;

                    new VirtButton(name: VButName.Return, label: "RETURN", colorPressed: Color.CornflowerBlue, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                    return;

                case TouchLayout.Inventory:
                    showSticks = false;

                    size = 0.07f;
                    xPos = 0.04f;
                    new VirtButton(name: VButName.Return, label: "RETURN", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: 0.12f, width0to1: size, height0to1: size);
                    new VirtButton(name: VButName.DragSingle, label: "DRAG\nSINGLE", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: 0.88f, width0to1: size, height0to1: size, switchButton: true);
                    return;

                case TouchLayout.MenuLeftReturn:
                    showSticks = false;

                    xPos = 0.55f;
                    yPos = 0.85f;
                    new VirtButton(name: VButName.Return, label: "RETURN", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                    return;

                case TouchLayout.MenuRightReturn:
                    showSticks = false;

                    xPos = 0.45f;
                    yPos = 0.85f;
                    new VirtButton(name: VButName.Return, label: "RETURN", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                    return;

                case TouchLayout.MenuMiddleReturn:
                    showSticks = false;

                    size = 0.07f;
                    xPos = 0.04f;
                    yPos = 0.15f;
                    new VirtButton(name: VButName.Return, label: "RETURN", colorPressed: Color.LightGreen, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                    return;

                default:
                    throw new DivideByZeroException($"Unsupported touch layout - {touchLayout}.");
            }
        }

        public static void AddDebugButtons()
        {
            float xPos = 0.32f;
            float yPos = 0.07f;
            float width = 0.065f;
            float height = 0.065f;
            float xShift = 0.07f;
            float yShift = 0.15f;

            new VirtButton(name: VButName.DebugPause, label: "||", colorPressed: Color.Violet, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);
            xPos += xShift;
            new VirtButton(name: VButName.DebugPlay, label: "|>", colorPressed: Color.Violet, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);
            xPos += xShift;
            new VirtButton(name: VButName.DebugFastForward, label: ">>", colorPressed: Color.Violet, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);
            xPos += xShift;
            new VirtButton(name: VButName.DebugClear, label: "clear", colorPressed: Color.Violet, colorReleased: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);                 
        }


    }
}
