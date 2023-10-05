using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class ButtonScheme
    {
        public enum Type : byte
        {
            Xbox360,
            XboxSeries,
            DualShock4,
            DualSense,
            SwitchProController,
        }

        private static Type currentType;

        public static readonly Texture2D plus = TextureBank.GetTexture("input/plus"); // for buttons that need to be pressed together

        public static Texture2D buttonA;
        public static Texture2D buttonB;
        public static Texture2D buttonX;
        public static Texture2D buttonY;

        public static Texture2D buttonBack;
        public static Texture2D buttonStart;

        public static Texture2D dpad;
        public static Texture2D dpadDown;
        public static Texture2D dpadUp;
        public static Texture2D dpadLeft;
        public static Texture2D dpadRight;

        public static Texture2D buttonLB;
        public static Texture2D buttonRB;
        public static Texture2D buttonLT;
        public static Texture2D buttonRT;

        public static Texture2D leftStick;
        public static Texture2D rightStick;
        public static Texture2D leftStickClick;
        public static Texture2D rightStickClick;

        public static void ChangeType(Type type)
        {
            currentType = type;

            switch (currentType)
            {
                case Type.Xbox360:
                    {
                        string subdir = "input/Xbox 360";

                        buttonA = TextureBank.GetTexture($"{subdir}/360_A");
                        buttonB = TextureBank.GetTexture($"{subdir}/360_B");
                        buttonX = TextureBank.GetTexture($"{subdir}/360_X");
                        buttonY = TextureBank.GetTexture($"{subdir}/360_Y");

                        buttonBack = TextureBank.GetTexture($"{subdir}/360_Back");
                        buttonStart = TextureBank.GetTexture($"{subdir}/360_Start");

                        dpad = TextureBank.GetTexture($"{subdir}/360_Dpad");
                        dpadDown = TextureBank.GetTexture($"{subdir}/360_Dpad_Down");
                        dpadUp = TextureBank.GetTexture($"{subdir}/360_Dpad_Up");
                        dpadLeft = TextureBank.GetTexture($"{subdir}/360_Dpad_Left");
                        dpadRight = TextureBank.GetTexture($"{subdir}/360_Dpad_Right");

                        buttonLB = TextureBank.GetTexture($"{subdir}/360_LB");
                        buttonRB = TextureBank.GetTexture($"{subdir}/360_RB");
                        buttonLT = TextureBank.GetTexture($"{subdir}/360_LT");
                        buttonRT = TextureBank.GetTexture($"{subdir}/360_RT");

                        leftStick = TextureBank.GetTexture($"{subdir}/360_Left_Stick");
                        rightStick = TextureBank.GetTexture($"{subdir}/360_Right_Stick");
                        leftStickClick = TextureBank.GetTexture($"{subdir}/360_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexture($"{subdir}/360_Right_Stick_Click");

                        break;
                    }

                case Type.XboxSeries:
                    {
                        string subdir = $"input/Xbox Series";

                        buttonA = TextureBank.GetTexture($"{subdir}/XboxSeriesX_A");
                        buttonB = TextureBank.GetTexture($"{subdir}/XboxSeriesX_B");
                        buttonX = TextureBank.GetTexture($"{subdir}/XboxSeriesX_X");
                        buttonY = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Y");

                        buttonBack = TextureBank.GetTexture($"{subdir}/XboxSeriesX_View");
                        buttonStart = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Menu");

                        dpad = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Dpad");
                        dpadDown = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Dpad_Down");
                        dpadUp = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Dpad_Up");
                        dpadLeft = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Dpad_Left");
                        dpadRight = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Dpad_Right");

                        buttonLB = TextureBank.GetTexture($"{subdir}/XboxSeriesX_LB");
                        buttonRB = TextureBank.GetTexture($"{subdir}/XboxSeriesX_RB");
                        buttonLT = TextureBank.GetTexture($"{subdir}/XboxSeriesX_LT");
                        buttonRT = TextureBank.GetTexture($"{subdir}/XboxSeriesX_RT");

                        leftStick = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Left_Stick");
                        rightStick = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Right_Stick");
                        leftStickClick = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexture($"{subdir}/XboxSeriesX_Right_Stick_Click");

                        break;
                    }

                case Type.DualShock4:
                    {
                        string subdir = $"input/PS4";

                        buttonA = TextureBank.GetTexture($"{subdir}/PS4_Cross");
                        buttonB = TextureBank.GetTexture($"{subdir}/PS4_Circle");
                        buttonX = TextureBank.GetTexture($"{subdir}/PS4_Square");
                        buttonY = TextureBank.GetTexture($"{subdir}/PS4_Triangle");

                        buttonBack = TextureBank.GetTexture($"{subdir}/PS4_Share"); // TODO check if this mapping is correct
                        buttonStart = TextureBank.GetTexture($"{subdir}/PS4_Options");

                        dpad = TextureBank.GetTexture($"{subdir}/PS4_Dpad");
                        dpadDown = TextureBank.GetTexture($"{subdir}/PS4_Dpad_Down");
                        dpadUp = TextureBank.GetTexture($"{subdir}/PS4_Dpad_Up");
                        dpadLeft = TextureBank.GetTexture($"{subdir}/PS4_Dpad_Left");
                        dpadRight = TextureBank.GetTexture($"{subdir}/PS4_Dpad_Right");

                        buttonLB = TextureBank.GetTexture($"{subdir}/PS4_L1");
                        buttonRB = TextureBank.GetTexture($"{subdir}/PS4_R1");
                        buttonLT = TextureBank.GetTexture($"{subdir}/PS4_L2");
                        buttonRT = TextureBank.GetTexture($"{subdir}/PS4_R2");

                        leftStick = TextureBank.GetTexture($"{subdir}/PS4_Left_Stick");
                        rightStick = TextureBank.GetTexture($"{subdir}/PS4_Right_Stick");
                        leftStickClick = TextureBank.GetTexture($"{subdir}/PS4_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexture($"{subdir}/PS4_Right_Stick_Click");

                        break;
                    }

                case Type.DualSense:
                    {
                        string subdir = $"input/PS5";

                        buttonA = TextureBank.GetTexture($"{subdir}/PS5_Cross");
                        buttonB = TextureBank.GetTexture($"{subdir}/PS5_Circle");
                        buttonX = TextureBank.GetTexture($"{subdir}/PS5_Square");
                        buttonY = TextureBank.GetTexture($"{subdir}/PS5_Triangle");

                        buttonBack = TextureBank.GetTexture($"{subdir}/PS5_Share"); // TODO check if this mapping is correct
                        buttonStart = TextureBank.GetTexture($"{subdir}/PS5_Options");

                        dpad = TextureBank.GetTexture($"{subdir}/PS5_Dpad");
                        dpadDown = TextureBank.GetTexture($"{subdir}/PS5_Dpad_Down");
                        dpadUp = TextureBank.GetTexture($"{subdir}/PS5_Dpad_Up");
                        dpadLeft = TextureBank.GetTexture($"{subdir}/PS5_Dpad_Left");
                        dpadRight = TextureBank.GetTexture($"{subdir}/PS5_Dpad_Right");

                        buttonLB = TextureBank.GetTexture($"{subdir}/PS5_L1");
                        buttonRB = TextureBank.GetTexture($"{subdir}/PS5_R1");
                        buttonLT = TextureBank.GetTexture($"{subdir}/PS5_L2");
                        buttonRT = TextureBank.GetTexture($"{subdir}/PS5_R2");

                        leftStick = TextureBank.GetTexture($"{subdir}/PS5_Left_Stick");
                        rightStick = TextureBank.GetTexture($"{subdir}/PS5_Right_Stick");
                        leftStickClick = TextureBank.GetTexture($"{subdir}/PS5_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexture($"{subdir}/PS5_Right_Stick_Click");

                        break;
                    }

                case Type.SwitchProController:
                    {
                        string subdir = $"input/Switch";

                        buttonA = TextureBank.GetTexture($"{subdir}/Switch_B");
                        buttonB = TextureBank.GetTexture($"{subdir}/Switch_A");
                        buttonX = TextureBank.GetTexture($"{subdir}/Switch_Y");
                        buttonY = TextureBank.GetTexture($"{subdir}/Switch_X");

                        buttonBack = TextureBank.GetTexture($"{subdir}/Switch_Minus");
                        buttonStart = TextureBank.GetTexture($"{subdir}/Switch_Plus");

                        dpad = TextureBank.GetTexture($"{subdir}/Switch_Dpad");
                        dpadDown = TextureBank.GetTexture($"{subdir}/Switch_Dpad_Down");
                        dpadUp = TextureBank.GetTexture($"{subdir}/Switch_Dpad_Up");
                        dpadLeft = TextureBank.GetTexture($"{subdir}/Switch_Dpad_Left");
                        dpadRight = TextureBank.GetTexture($"{subdir}/Switch_Dpad_Right");

                        buttonLB = TextureBank.GetTexture($"{subdir}/Switch_LB");
                        buttonRB = TextureBank.GetTexture($"{subdir}/Switch_RB");
                        buttonLT = TextureBank.GetTexture($"{subdir}/Switch_LT");
                        buttonRT = TextureBank.GetTexture($"{subdir}/Switch_RT");

                        leftStick = TextureBank.GetTexture($"{subdir}/Switch_Left_Stick");
                        rightStick = TextureBank.GetTexture($"{subdir}/Switch_Right_Stick");
                        leftStickClick = TextureBank.GetTexture("input/Xbox 360/360_Left_Stick_Click"); // graphics missing
                        rightStickClick = TextureBank.GetTexture("input/Xbox 360/360_Right_Stick_Click");  // graphics missing

                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported button scheme type - {currentType}.");
            }

            SonOfRobinGame.ControlTips.RefreshLayout();
        }
    }
}