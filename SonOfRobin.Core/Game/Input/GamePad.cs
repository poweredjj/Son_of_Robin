using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class GamePad
    // expands GamePad class with new functions

    {
        private static readonly Dictionary<PlayerIndex, GamePadState> currentPadStateByIndex = new Dictionary<PlayerIndex, GamePadState> { };
        private static readonly Dictionary<PlayerIndex, GamePadState> previousPadStateByIndex = new Dictionary<PlayerIndex, GamePadState> { };
        private static readonly Dictionary<PlayerIndex, List<Buttons>> currentDigitalFromAnalogByIndex = new Dictionary<PlayerIndex, List<Buttons>> { };
        private static readonly Dictionary<PlayerIndex, List<Buttons>> previousDigitalFromAnalogByIndex = new Dictionary<PlayerIndex, List<Buttons>> { };

        public static void GetPreviousState(PlayerIndex playerIndex)
        {
            try
            {
                previousPadStateByIndex[playerIndex] = currentPadStateByIndex[playerIndex];
                previousDigitalFromAnalogByIndex[playerIndex] = currentDigitalFromAnalogByIndex[playerIndex];
            }
            catch (KeyNotFoundException)
            {
                previousPadStateByIndex[playerIndex] = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex);
                previousDigitalFromAnalogByIndex[playerIndex] = new List<Buttons> { };
            }

            currentPadStateByIndex[playerIndex] = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex);
            currentDigitalFromAnalogByIndex[playerIndex] = ConvertAnalogToDigital(playerIndex);
        }


        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            if (!Input.InputActive) return GamePadState.Default;
            return currentPadStateByIndex[playerIndex];
        }

        public static GamePadState GetState(int index, GamePadDeadZone deadZoneMode)
        {
            if (!Input.InputActive) return GamePadState.Default;
            return Microsoft.Xna.Framework.Input.GamePad.GetState(index: index, deadZoneMode: deadZoneMode);
        }

        private static List<Buttons> ConvertAnalogToDigital(PlayerIndex playerIndex)
        {
            var foundDirections = new List<Buttons> { };

            var thumbsticks = new List<Vector2> { currentPadStateByIndex[playerIndex].ThumbSticks.Left, currentPadStateByIndex[playerIndex].ThumbSticks.Right };
            foreach (Vector2 thumbstick in thumbsticks)
            {
                if (thumbstick.X < -0.5f) foundDirections.Add(Buttons.DPadLeft);
                if (thumbstick.X > 0.5f) foundDirections.Add(Buttons.DPadRight);
                if (thumbstick.Y > 0.5f) foundDirections.Add(Buttons.DPadUp);
                if (thumbstick.Y < -0.5f) foundDirections.Add(Buttons.DPadDown);

                if (foundDirections.Count > 0) break; // if left thumbstick was moved, right thumbstick should be ignored
            }

            return foundDirections;
        }

        public static bool IsPressed(PlayerIndex playerIndex, Buttons button, bool analogAsDigital = false)
        {
            if (analogAsDigital && Input.InputActive && currentDigitalFromAnalogByIndex[playerIndex].Contains(button)) return true;
            return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonDown(button);
        }

        public static bool HasBeenPressed(PlayerIndex playerIndex, Buttons button, bool analogAsDigital = false)
        {
            if (analogAsDigital && Input.InputActive && currentDigitalFromAnalogByIndex[playerIndex].Contains(button) && !previousDigitalFromAnalogByIndex[playerIndex].Contains(button)) return true;

            return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonDown(button) && previousPadStateByIndex[playerIndex].IsButtonUp(button);
        }

        public static bool HasBeenReleased(PlayerIndex playerIndex, Buttons button, bool analogAsDigital = false)
        {
            if (analogAsDigital && Input.InputActive && !currentDigitalFromAnalogByIndex[playerIndex].Contains(button) && previousDigitalFromAnalogByIndex[playerIndex].Contains(button)) return true;
            return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonUp(button) && previousPadStateByIndex[playerIndex].IsButtonDown(button);
        }
    }
}
