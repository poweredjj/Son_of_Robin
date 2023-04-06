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

        public static List<Texture2D> LeftStickTextureList
        {
            get
            {
                switch (Input.currentControlType)
                {
                    case Input.ControlType.Touch:
                        return new List<Texture2D>();

                    case Input.ControlType.Gamepad:
                        return GetAnalogTextureList(InputMapper.currentMappingGamepad.leftStick.Analog);

                    case Input.ControlType.KeyboardAndMouse:
                        return GetAnalogTextureList(InputMapper.currentMappingKeyboard.leftStick.Analog);

                    default:
                        throw new ArgumentException($"Unsupported tipsTypeToShow - '{Input.currentControlType}'.");
                }
            }
        }

        public static List<Texture2D> RightStickTextureList
        {
            get
            {
                switch (Input.currentControlType)
                {
                    case Input.ControlType.Touch:
                        return new List<Texture2D>();

                    case Input.ControlType.Gamepad:
                        return GetAnalogTextureList(InputMapper.currentMappingGamepad.rightStick.Analog);

                    case Input.ControlType.KeyboardAndMouse:
                        return GetAnalogTextureList(InputMapper.currentMappingKeyboard.rightStick.Analog);

                    default:
                        throw new ArgumentException($"Unsupported tipsTypeToShow - '{Input.currentControlType}'.");
                }
            }
        }

        public static Texture2D GetTexture(object anyType)
        {
            Type type = anyType.GetType();

            if (type == typeof(InputMapper.AnalogType))
            {
                var analogTextureList = GetAnalogTextureList((InputMapper.AnalogType)anyType);
                return analogTextureList.Count == 0 ? null : analogTextureList[0];
            }
            else if (type == typeof(StoredInput))
            {
                StoredInput storedInput = (StoredInput)anyType;

                switch (storedInput.type)
                {
                    case StoredInput.Type.Key:
                        return GetTexture(storedInput.Key);

                    case StoredInput.Type.Button:
                        return GetTexture(storedInput.Button);

                    case StoredInput.Type.Analog:
                        return GetTexture(storedInput.Analog);

                    default:
                        throw new ArgumentException($"Unsupported type - '{storedInput.type}'.");
                }
            }
            else throw new ArgumentException($"Unsupported anyType - '{type}'.");
        }

        public static List<Texture2D> GetAnalogTextureList(InputMapper.AnalogType analogType)
        {
            var textureList = new List<Texture2D>();

            switch (analogType)
            {
                case InputMapper.AnalogType.Empty:
                    break;

                case InputMapper.AnalogType.PadLeft:
                    textureList.Add(leftStick);
                    break;

                case InputMapper.AnalogType.PadRight:
                    textureList.Add(rightStick);
                    break;

                case InputMapper.AnalogType.VirtLeft:
                    break;

                case InputMapper.AnalogType.VirtRight:
                    break;

                case InputMapper.AnalogType.FromKeys:
                    textureList.Add(GetTexture(InputMapper.currentMappingKeyboard.left));
                    textureList.Add(GetTexture(InputMapper.currentMappingKeyboard.right));
                    textureList.Add(GetTexture(InputMapper.currentMappingKeyboard.up));
                    textureList.Add(GetTexture(InputMapper.currentMappingKeyboard.down));
                    break;

                default:
                    throw new ArgumentException($"Unsupported analogType - '{analogType}'.");
            }

            return textureList;
        }

        public static Texture2D GetTexture(Keys key)
        {
            Texture2D texture = KeyboardScheme.GetTexture(key);

            if (texture == null)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Visual for key '{key}' has not been found.", color: Color.LightBlue);
                return SonOfRobinGame.WhiteRectangle;
            }

            return texture;
        }

        public static Texture2D GetTexture(Buttons button)
        {
            if (!buttonTextures.ContainsKey(button))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Visual for button '{button}' has not been found.", color: Color.LightBlue);
                return SonOfRobinGame.WhiteRectangle;
            }

            return buttonTextures[button];
        }

        public static Texture2D GetTexture(InputMapper.MouseAction mouseAction)
        {
            switch (mouseAction)
            {
                case InputMapper.MouseAction.LeftButton:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Left_Key_Light");

                case InputMapper.MouseAction.LeftButtonVisOnly:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Left_Key_Light");

                case InputMapper.MouseAction.MiddleButton:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Middle_Key_Light");

                case InputMapper.MouseAction.MiddleButtonVisOnly:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Middle_Key_Light");

                case InputMapper.MouseAction.RightButton:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Right_Key_Light");

                case InputMapper.MouseAction.RightButtonVisOnly:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Right_Key_Light");

                case InputMapper.MouseAction.ScrollUp:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Scroll_Up_Light");

                case InputMapper.MouseAction.ScrollDown:
                    return TextureBank.GetTexture("input/Mouse/Mouse_Scroll_Down_Light");

                default:
                    throw new ArgumentException($"Unsupported mouseAction - '{mouseAction}'.");
            }

            throw new ArgumentException($"Unsupported mouseAction - '{mouseAction}'.");
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