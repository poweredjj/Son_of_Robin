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
                switch (Input.tipsTypeToShow)
                {
                    case Input.TipsTypeToShow.Gamepad:
                        return GetAnalogTextureList(InputMapper.currentMappingGamepad.leftStick);
                    case Input.TipsTypeToShow.Keyboard:
                        return GetAnalogTextureList(InputMapper.currentMappingKeyboard.leftStick);
                    default:
                        throw new ArgumentException($"Unsupported tipsTypeToShow - '{Input.tipsTypeToShow}'.");
                }
            }
        }
        public static List<Texture2D> RightStickTextureList
        {
            get
            {
                switch (Input.tipsTypeToShow)
                {
                    case Input.TipsTypeToShow.Gamepad:
                        return GetAnalogTextureList(InputMapper.currentMappingGamepad.rightStick);
                    case Input.TipsTypeToShow.Keyboard:
                        return GetAnalogTextureList(InputMapper.currentMappingKeyboard.rightStick);
                    default:
                        throw new ArgumentException($"Unsupported tipsTypeToShow - '{Input.tipsTypeToShow}'.");
                }
            }
        }

        public static Texture2D GetTexture(object anyType)
        {
            if (anyType.GetType() == typeof(InputMapper.AnalogType))
            {
                var analogTextureList = GetAnalogTextureList((InputMapper.AnalogType)anyType);
                return analogTextureList.Count == 0 ? null : analogTextureList[0];
            }
            else if (anyType.GetType() == typeof(Keys)) return GetTexture((Keys)anyType);
            else if (anyType.GetType() == typeof(Buttons)) return GetTexture((Buttons)anyType);
            else throw new ArgumentException($"Unsupported anyType - '{anyType.GetType()}'.");
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

        public static Texture2D GetTexture(InputMapper.MouseAction mouseAction)
        {
            switch (mouseAction)
            {
                case InputMapper.MouseAction.LeftButton:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Left_Key_Light"];

                case InputMapper.MouseAction.LeftButtonVisOnly:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Left_Key_Light"];

                case InputMapper.MouseAction.MiddleButton:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Middle_Key_Light"];

                case InputMapper.MouseAction.MiddleButtonVisOnly:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Middle_Key_Light"];

                case InputMapper.MouseAction.RightButton:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Right_Key_Light"];

                case InputMapper.MouseAction.RightButtonVisOnly:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Right_Key_Light"];

                case InputMapper.MouseAction.ScrollUp:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Scroll_Up_Light"];

                case InputMapper.MouseAction.ScrollDown:
                    return SonOfRobinGame.textureByName["Mouse/Mouse_Scroll_Down_Light"];

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
