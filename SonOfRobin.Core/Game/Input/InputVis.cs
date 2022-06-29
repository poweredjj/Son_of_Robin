using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class InputVis
    {
        private static Dictionary<Buttons, Texture2D> buttonTextures = new Dictionary<Buttons, Texture2D>();

        public static Texture2D leftStick = null;
        public static Texture2D rightStick = null;
        public static Texture2D dPad = null;
        public static readonly Texture2D wasd = SonOfRobinGame.content.Load<Texture2D>("gfx/Keyboard/wasd_light");
        public static readonly Texture2D arrows = SonOfRobinGame.content.Load<Texture2D>("gfx/Keyboard/arrows_light");
        public static readonly Texture2D numpad = SonOfRobinGame.content.Load<Texture2D>("gfx/Keyboard/numpad_light");

        public static List<Texture2D> LeftStickTextureList
        {
            get
            {
                var leftStickTexture = LeftStickTexture;
                if (leftStickTexture == null) return new List<Texture2D>();
                return new List<Texture2D> { LeftStickTexture };
            }
        }

        public static List<Texture2D> RightStickTextureList
        {
            get
            {
                var rightStickTexture = RightStickTexture;
                if (rightStickTexture == null) return new List<Texture2D>();
                return new List<Texture2D> { RightStickTexture };
            }
        }

        public static Texture2D LeftStickTexture
        {
            get
            {
                switch (Input.tipsTypeToShow)
                {
                    case Input.TipsTypeToShow.Gamepad:
                        return GetTexture(InputMapper.currentMappingGamepad.leftStick);
                    case Input.TipsTypeToShow.Keyboard:
                        return GetTexture(InputMapper.currentMappingKeyboard.leftStick);
                    default:
                        throw new ArgumentException($"Unsupported tipsTypeToShow - '{Input.tipsTypeToShow}'.");
                }
            }
        }
        public static Texture2D RightStickTexture
        {
            get
            {
                switch (Input.tipsTypeToShow)
                {
                    case Input.TipsTypeToShow.Gamepad:
                        return GetTexture(InputMapper.currentMappingGamepad.rightStick);
                    case Input.TipsTypeToShow.Keyboard:
                        return GetTexture(InputMapper.currentMappingGamepad.rightStick);
                    default:
                        throw new ArgumentException($"Unsupported tipsTypeToShow - '{Input.tipsTypeToShow}'.");
                }
            }
        }

        public static Texture2D GetTexture(object freeType)
        {
            if (freeType.GetType() == typeof(InputMapper.AnalogType)) return GetTexture((InputMapper.AnalogType)freeType);
            else if (freeType.GetType() == typeof(Keys)) return GetTexture((Keys)freeType);
            else if (freeType.GetType() == typeof(Buttons)) return GetTexture((Buttons)freeType);
            else throw new ArgumentException($"Unsupported freeType - '{freeType.GetType()}'.");
        }

        public static Texture2D GetTexture(InputMapper.AnalogType analogType)
        {
            switch (analogType)
            {
                case InputMapper.AnalogType.Empty:
                    return null;

                case InputMapper.AnalogType.PadLeft:
                    return leftStick;

                case InputMapper.AnalogType.PadRight:
                    return rightStick;

                case InputMapper.AnalogType.WASD:
                    return wasd;

                case InputMapper.AnalogType.Arrows:
                    return arrows;

                case InputMapper.AnalogType.Numpad:
                    return numpad;

                case InputMapper.AnalogType.VirtLeft:
                    return null;

                case InputMapper.AnalogType.VirtRight:
                    return null;

                default:
                    throw new ArgumentException($"Unsupported analogType - '{analogType}'.");
            }
        }

        public static Texture2D GetTexture(Keys key)
        {
            Texture2D texture = KeyboardScheme.GetTexture(key);

            if (texture == null)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Visual for key '{key}' has not been found.", color: Color.LightBlue);
                return SonOfRobinGame.whiteRectangle;
            }

            return texture;
        }

        public static Texture2D GetTexture(Buttons button)
        {
            if (!buttonTextures.ContainsKey(button))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Visual for button '{button}' has not been found.", color: Color.LightBlue);
                return SonOfRobinGame.whiteRectangle;
            }

            return buttonTextures[button];
        }

        public static void Refresh()
        {
            leftStick = ButtonScheme.leftStick;
            rightStick = ButtonScheme.rightStick;
            dPad = ButtonScheme.dpad;

            buttonTextures[Buttons.A] = ButtonScheme.buttonA;
            buttonTextures[Buttons.B] = ButtonScheme.buttonB;
            buttonTextures[Buttons.X] = ButtonScheme.buttonX;
            buttonTextures[Buttons.Y] = ButtonScheme.buttonY;

            buttonTextures[Buttons.Back] = ButtonScheme.buttonBack;
            buttonTextures[Buttons.Start] = ButtonScheme.buttonStart;

            buttonTextures[Buttons.DPadDown] = ButtonScheme.dpadDown;
            buttonTextures[Buttons.DPadUp] = ButtonScheme.dpadUp;
            buttonTextures[Buttons.DPadLeft] = ButtonScheme.dpadLeft;
            buttonTextures[Buttons.DPadRight] = ButtonScheme.dpadRight;

            buttonTextures[Buttons.LeftShoulder] = ButtonScheme.buttonLB;
            buttonTextures[Buttons.RightShoulder] = ButtonScheme.buttonRB;
            buttonTextures[Buttons.LeftTrigger] = ButtonScheme.buttonLT;
            buttonTextures[Buttons.RightTrigger] = ButtonScheme.buttonRT;

            buttonTextures[Buttons.LeftStick] = ButtonScheme.leftStickClick;
            buttonTextures[Buttons.RightStick] = ButtonScheme.rightStickClick;
            buttonTextures[Buttons.LeftTrigger] = ButtonScheme.buttonLT;
            buttonTextures[Buttons.RightTrigger] = ButtonScheme.buttonRT;
        }
    }

}
