using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Mouse
    // expands Mouse class with new functions
    {
        private static MouseState currentMouseState;
        private static MouseState previousMouseState;

        public static bool LeftIsDown
        { get { return Input.InputActive && currentMouseState.LeftButton == ButtonState.Pressed; } }

        public static bool LeftHasBeenPressed
        { get { return Input.InputActive && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released; } }

        public static bool LeftHasBeenReleased
        { get { return Input.InputActive && currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed; } }

        public static bool MiddleIsDown
        { get { return Input.InputActive && currentMouseState.MiddleButton == ButtonState.Pressed; } }

        public static bool MiddleHasBeenPressed
        { get { return Input.InputActive && currentMouseState.MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Released; } }

        public static bool MiddleHasBeenReleased
        { get { return Input.InputActive && currentMouseState.MiddleButton == ButtonState.Released && previousMouseState.MiddleButton == ButtonState.Pressed; } }

        public static bool RightIsDown
        { get { return Input.InputActive && currentMouseState.RightButton == ButtonState.Pressed; } }

        public static bool RightHasBeenPressed
        { get { return Input.InputActive && currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released; } }

        public static bool RightHasBeenReleased
        { get { return Input.InputActive && currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed; } }

        public static int ScrollWheelValue
        { get { return Input.InputActive ? currentMouseState.ScrollWheelValue : 0; } }

        public static bool ScrollWheelRolledUp
        { get { return Input.InputActive && currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue; } }

        public static bool ScrollWheelRolledDown
        { get { return Input.InputActive && currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue; } }

        public static int XValue
        { get { return Input.InputActive ? currentMouseState.X : 0; } }

        public static int YValue
        { get { return Input.InputActive ? currentMouseState.Y : 0; } }

        public static Vector2 Position
        { get { return Input.InputActive ? new Vector2(currentMouseState.X, currentMouseState.Y) : Vector2.Zero; } }

        public static MouseState GetState()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            if (Preferences.swapMouseButtons)
            { // reversing swapped mouse buttons
                currentMouseState = new MouseState(x: currentMouseState.X, y: currentMouseState.Y, scrollWheel: currentMouseState.ScrollWheelValue, leftButton: currentMouseState.RightButton, middleButton: currentMouseState.MiddleButton, rightButton: currentMouseState.LeftButton, xButton1: currentMouseState.XButton1, xButton2: currentMouseState.XButton2);
            }

            return previousMouseState;
        }
    }
}