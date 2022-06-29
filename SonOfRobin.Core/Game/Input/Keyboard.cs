using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace SonOfRobin
{
    public class Keyboard
    // expands Keyboard class with new functions
    {
        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;

        public static KeyboardState GetState()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            return currentKeyState;
        }

        public static bool IsPressed(Keys key)
        { return Input.InputActive && currentKeyState.IsKeyDown(key); }

        public static bool HasBeenPressed(Keys key)
        { return Input.InputActive && currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key); }

        public static bool HasBeenReleased(Keys key)
        { return Input.InputActive && !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key); }
    }
}
