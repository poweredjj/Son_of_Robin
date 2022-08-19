using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Keyboard
    // expands Keyboard class with new functions
    {
        public static KeyboardState CurrentKeyState { get; private set; }
        private static KeyboardState previousKeyState;


        public static void GetState()
        {
            previousKeyState = CurrentKeyState;
            CurrentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }

        public static bool IsPressed(Keys key)
        { return Input.InputActive && CurrentKeyState.IsKeyDown(key); }

        public static bool HasBeenPressed(Keys key)
        { return Input.InputActive && CurrentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key); }

        public static bool HasBeenReleased(Keys key)
        { return Input.InputActive && !CurrentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key); }
    }
}
