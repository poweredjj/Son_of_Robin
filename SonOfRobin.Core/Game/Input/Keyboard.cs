using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Keyboard
    // expands Keyboard class with new functions
    {
        private static KeyboardState currentKeyState;
        private static KeyboardState previousKeyState;
        public static KeyboardState CurrentKeyState { get { return currentKeyState; } }

        public static void GetState()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }

        public static bool IsPressed(Keys key)
        { return Input.InputActive && currentKeyState.IsKeyDown(key); }

        public static bool HasBeenPressed(Keys key)
        { return Input.InputActive && currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key); }

        public static bool HasBeenReleased(Keys key)
        { return Input.InputActive && !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key); }
    }
}
