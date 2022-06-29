using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class GamePad
    // expands GamePad class with new functions

    {
        private static GamePadState zeroState = GamePadState.Default; // dummy input, returned when input is turned off

        private static readonly Dictionary<PlayerIndex, GamePadState> currentPadStateByIndex = new Dictionary<PlayerIndex, GamePadState> { };
        private static readonly Dictionary<PlayerIndex, GamePadState> previousPadStateByIndex = new Dictionary<PlayerIndex, GamePadState> { };

        public static void GetPreviousState(PlayerIndex playerIndex)
        {
            try
            {
                previousPadStateByIndex[playerIndex] = currentPadStateByIndex[playerIndex];
            }
            catch (KeyNotFoundException)
            {
                previousPadStateByIndex[playerIndex] = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex);
            }

            currentPadStateByIndex[playerIndex] = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex);
        }


        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            if (!Input.InputActive) return zeroState;
            return currentPadStateByIndex[playerIndex];
        }

        public static GamePadState GetState(int index, GamePadDeadZone deadZoneMode)
        {
            if (!Input.InputActive) return zeroState;
            return Microsoft.Xna.Framework.Input.GamePad.GetState(index: index, deadZoneMode: deadZoneMode);
        }

        public static bool IsPressed(PlayerIndex playerIndex, Buttons button)
        { return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonDown(button); }

        public static bool HasBeenPressed(PlayerIndex playerIndex, Buttons button)
        { return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonDown(button) && previousPadStateByIndex[playerIndex].IsButtonUp(button); }

        public static bool HasBeenReleased(PlayerIndex playerIndex, Buttons button)
        { return Input.InputActive && currentPadStateByIndex[playerIndex].IsButtonUp(button) && previousPadStateByIndex[playerIndex].IsButtonDown(button); }
    }
}
