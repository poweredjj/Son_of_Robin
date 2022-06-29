using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
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
        static PlayerIndex buttonsHeldGamepadIndex = 0;
        static Dictionary<Buttons, int> buttonsHeld = new Dictionary<Buttons, int> { };
        static Dictionary<Buttons, int> buttonsHeldDigitalFromAnalog = new Dictionary<Buttons, int> { };

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

            UpdateButtonsHeld(playerIndex);
        }

        public static GamePadState GetState(int index, GamePadDeadZone deadZoneMode)
        {
            if (!Input.InputActive) return GamePadState.Default;

            return Microsoft.Xna.Framework.Input.GamePad.GetState(index: index, deadZoneMode: deadZoneMode);
        }

        private static void UpdateButtonsHeld(PlayerIndex playerIndex)
        {
            if (buttonsHeldGamepadIndex != playerIndex)
            {
                buttonsHeld.Clear();
                buttonsHeldDigitalFromAnalog.Clear();
            }
            buttonsHeldGamepadIndex = playerIndex;

            var buttonList = (Buttons[])Enum.GetValues(typeof(Buttons));
            foreach (var button in buttonList)
            {
                if (currentPadStateByIndex[playerIndex].IsButtonDown(button))
                {
                    if (!buttonsHeld.ContainsKey(button)) buttonsHeld[button] = 1;
                    else buttonsHeld[button]++;
                }
                else buttonsHeld.Remove(button);
            }

            foreach (var button in new List<Buttons> { Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp, Buttons.DPadDown })
            {
                if (currentDigitalFromAnalogByIndex[playerIndex].Contains(button))
                {
                    if (!buttonsHeldDigitalFromAnalog.ContainsKey(button)) buttonsHeldDigitalFromAnalog[button] = 1;
                    else buttonsHeldDigitalFromAnalog[button]++;
                }
                else buttonsHeldDigitalFromAnalog.Remove(button);
            }
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

        public static bool HasBeenPressed(PlayerIndex playerIndex, Buttons button, bool analogAsDigital = false, bool repeat = false, int repeatThreshold = 0, int repeatFrames = 0)
        {
            if (!Input.InputActive) return false;

            if (repeat)
            {
                if (repeatThreshold == 0) repeatThreshold = Input.defaultButtonRepeatThreshold;
                if (repeatFrames == 0) repeatFrames = Input.defaultButtonRepeatFrames;
            }

            if (repeat && buttonsHeld.ContainsKey(button) && buttonsHeld[button] >= repeatThreshold + repeatFrames)
            {
                buttonsHeld[button] = Math.Max(buttonsHeld[button] - repeatFrames, 0);
                return true;
            }

            if (repeat && analogAsDigital && buttonsHeldDigitalFromAnalog.ContainsKey(button) &&
                buttonsHeldDigitalFromAnalog[button] >= repeatThreshold + repeatFrames)
            {
                buttonsHeldDigitalFromAnalog[button] = Math.Max(buttonsHeldDigitalFromAnalog[button] - repeatFrames, 0);
                return true;
            }

            if (analogAsDigital && currentDigitalFromAnalogByIndex[playerIndex].Contains(button) && !previousDigitalFromAnalogByIndex[playerIndex].Contains(button)) return true;

            return currentPadStateByIndex[playerIndex].IsButtonDown(button) && previousPadStateByIndex[playerIndex].IsButtonUp(button);
        }

        public static bool HasBeenReleased(PlayerIndex playerIndex, Buttons button, bool analogAsDigital = false)
        {
            if (analogAsDigital && Input.InputActive && !currentDigitalFromAnalogByIndex[playerIndex].Contains(button) && previousDigitalFromAnalogByIndex[playerIndex].Contains(button)) return true;
            return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonUp(button) && previousPadStateByIndex[playerIndex].IsButtonDown(button);
        }
    }
}
