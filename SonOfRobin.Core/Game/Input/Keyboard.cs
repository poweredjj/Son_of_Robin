using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Keyboard
    // expands Keyboard class with new functions
    {
        static KeyboardState currentKeyState;
        static KeyboardState previousKeyState;
        static Dictionary<Keys, int> keysHeld = new Dictionary<Keys, int> { };

        public static KeyboardState GetState()
        {
            previousKeyState = currentKeyState;
            currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            UpdateKeysHeld();

            return currentKeyState;
        }

        private static void UpdateKeysHeld()
        {
            var pressedKeys = new List<Keys> { };
            foreach (Keys key in currentKeyState.GetPressedKeys())
            {
                pressedKeys.Add(key);

                if (!keysHeld.ContainsKey(key)) keysHeld[key] = 1;
                else keysHeld[key]++;
            }
            foreach (Keys key in keysHeld.Keys)
            {
                if (!pressedKeys.Contains(key)) keysHeld.Remove(key);
            }
        }

        public static bool IsPressed(Keys key)
        { return Input.InputActive && currentKeyState.IsKeyDown(key); }

        public static bool HasBeenPressed(Keys key, bool repeat = false, int repeatThreshold = 0, int repeatFrames = 0)
        {
            if (!Input.InputActive) return false;

            if (repeat)
            {
                if (repeatThreshold == 0) repeatThreshold = Input.defaultButtonRepeatThreshold;
                if (repeatFrames == 0) repeatFrames = Input.defaultButtonRepeatFrames;
            }

            if (repeat && keysHeld.ContainsKey(key) && keysHeld[key] >= repeatThreshold + repeatFrames)
            {
                keysHeld[key] = Math.Max(keysHeld[key] - repeatFrames, 0);
                return true;
            }
            return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
        }

        public static bool HasBeenReleased(Keys key)
        { return Input.InputActive && !currentKeyState.IsKeyDown(key) && previousKeyState.IsKeyDown(key); }
    }
}
