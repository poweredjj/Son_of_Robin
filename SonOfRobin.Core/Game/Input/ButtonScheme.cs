using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class ButtonScheme
    {
        public enum Type
        { Xbox360, XboxSeries, DualShock4, DualSense, SwitchProController }

        private static Type currentType;

        public static readonly Texture2D plus = TextureBank.GetTexturePersistent("input/plus"); // for buttons that need to be pressed together

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

                        buttonA = TextureBank.GetTexturePersistent($"{subdir}/360_A");
                        buttonB = TextureBank.GetTexturePersistent($"{subdir}/360_B");
                        buttonX = TextureBank.GetTexturePersistent($"{subdir}/360_X");
                        buttonY = TextureBank.GetTexturePersistent($"{subdir}/360_Y");

                        buttonBack = TextureBank.GetTexturePersistent($"{subdir}/360_Back");
                        buttonStart = TextureBank.GetTexturePersistent($"{subdir}/360_Start");

                        dpad = TextureBank.GetTexturePersistent($"{subdir}/360_Dpad");
                        dpadDown = TextureBank.GetTexturePersistent($"{subdir}/360_Dpad_Down");
                        dpadUp = TextureBank.GetTexturePersistent($"{subdir}/360_Dpad_Up");
                        dpadLeft = TextureBank.GetTexturePersistent($"{subdir}/360_Dpad_Left");
                        dpadRight = TextureBank.GetTexturePersistent($"{subdir}/360_Dpad_Right");

                        buttonLB = TextureBank.GetTexturePersistent($"{subdir}/360_LB");
                        buttonRB = TextureBank.GetTexturePersistent($"{subdir}/360_RB");
                        buttonLT = TextureBank.GetTexturePersistent($"{subdir}/360_LT");
                        buttonRT = TextureBank.GetTexturePersistent($"{subdir}/360_RT");

                        leftStick = TextureBank.GetTexturePersistent($"{subdir}/360_Left_Stick");
                        rightStick = TextureBank.GetTexturePersistent($"{subdir}/360_Right_Stick");
                        leftStickClick = TextureBank.GetTexturePersistent($"{subdir}/360_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexturePersistent($"{subdir}/360_Right_Stick_Click");

                        break;
                    }

                case Type.XboxSeries:
                    {
                        string subdir = $"input/Xbox Series";

                        buttonA = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_A");
                        buttonB = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_B");
                        buttonX = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_X");
                        buttonY = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Y");

                        buttonBack = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_View");
                        buttonStart = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Menu");

                        dpad = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Dpad");
                        dpadDown = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Dpad_Down");
                        dpadUp = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Dpad_Up");
                        dpadLeft = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Dpad_Left");
                        dpadRight = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Dpad_Right");

                        buttonLB = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_LB");
                        buttonRB = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_RB");
                        buttonLT = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_LT");
                        buttonRT = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_RT");

                        leftStick = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Left_Stick");
                        rightStick = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Right_Stick");
                        leftStickClick = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexturePersistent($"{subdir}/XboxSeriesX_Right_Stick_Click");

                        break;
                    }

                case Type.DualShock4:
                    {
                        string subdir = $"input/PS4";

                        buttonA = TextureBank.GetTexturePersistent($"{subdir}/PS4_Cross");
                        buttonB = TextureBank.GetTexturePersistent($"{subdir}/PS4_Circle");
                        buttonX = TextureBank.GetTexturePersistent($"{subdir}/PS4_Square");
                        buttonY = TextureBank.GetTexturePersistent($"{subdir}/PS4_Triangle");

                        buttonBack = TextureBank.GetTexturePersistent($"{subdir}/PS4_Share"); // TODO check if this mapping is correct
                        buttonStart = TextureBank.GetTexturePersistent($"{subdir}/PS4_Options");

                        dpad = TextureBank.GetTexturePersistent($"{subdir}/PS4_Dpad");
                        dpadDown = TextureBank.GetTexturePersistent($"{subdir}/PS4_Dpad_Down");
                        dpadUp = TextureBank.GetTexturePersistent($"{subdir}/PS4_Dpad_Up");
                        dpadLeft = TextureBank.GetTexturePersistent($"{subdir}/PS4_Dpad_Left");
                        dpadRight = TextureBank.GetTexturePersistent($"{subdir}/PS4_Dpad_Right");

                        buttonLB = TextureBank.GetTexturePersistent($"{subdir}/PS4_L1");
                        buttonRB = TextureBank.GetTexturePersistent($"{subdir}/PS4_R1");
                        buttonLT = TextureBank.GetTexturePersistent($"{subdir}/PS4_L2");
                        buttonRT = TextureBank.GetTexturePersistent($"{subdir}/PS4_R2");

                        leftStick = TextureBank.GetTexturePersistent($"{subdir}/PS4_Left_Stick");
                        rightStick = TextureBank.GetTexturePersistent($"{subdir}/PS4_Right_Stick");
                        leftStickClick = TextureBank.GetTexturePersistent($"{subdir}/PS4_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexturePersistent($"{subdir}/PS4_Right_Stick_Click");

                        break;
                    }

                case Type.DualSense:
                    {
                        string subdir = $"input/PS5";

                        buttonA = TextureBank.GetTexturePersistent($"{subdir}/PS5_Cross");
                        buttonB = TextureBank.GetTexturePersistent($"{subdir}/PS5_Circle");
                        buttonX = TextureBank.GetTexturePersistent($"{subdir}/PS5_Square");
                        buttonY = TextureBank.GetTexturePersistent($"{subdir}/PS5_Triangle");

                        buttonBack = TextureBank.GetTexturePersistent($"{subdir}/PS5_Share"); // TODO check if this mapping is correct
                        buttonStart = TextureBank.GetTexturePersistent($"{subdir}/PS5_Options");

                        dpad = TextureBank.GetTexturePersistent($"{subdir}/PS5_Dpad");
                        dpadDown = TextureBank.GetTexturePersistent($"{subdir}/PS5_Dpad_Down");
                        dpadUp = TextureBank.GetTexturePersistent($"{subdir}/PS5_Dpad_Up");
                        dpadLeft = TextureBank.GetTexturePersistent($"{subdir}/PS5_Dpad_Left");
                        dpadRight = TextureBank.GetTexturePersistent($"{subdir}/PS5_Dpad_Right");

                        buttonLB = TextureBank.GetTexturePersistent($"{subdir}/PS5_L1");
                        buttonRB = TextureBank.GetTexturePersistent($"{subdir}/PS5_R1");
                        buttonLT = TextureBank.GetTexturePersistent($"{subdir}/PS5_L2");
                        buttonRT = TextureBank.GetTexturePersistent($"{subdir}/PS5_R2");

                        leftStick = TextureBank.GetTexturePersistent($"{subdir}/PS5_Left_Stick");
                        rightStick = TextureBank.GetTexturePersistent($"{subdir}/PS5_Right_Stick");
                        leftStickClick = TextureBank.GetTexturePersistent($"{subdir}/PS5_Left_Stick_Click");
                        rightStickClick = TextureBank.GetTexturePersistent($"{subdir}/PS5_Right_Stick_Click");

                        break;
                    }

                case Type.SwitchProController:
                    {
                        string subdir = $"input/Switch";

                        buttonA = TextureBank.GetTexturePersistent($"{subdir}/Switch_B");
                        buttonB = TextureBank.GetTexturePersistent($"{subdir}/Switch_A");
                        buttonX = TextureBank.GetTexturePersistent($"{subdir}/Switch_Y");
                        buttonY = TextureBank.GetTexturePersistent($"{subdir}/Switch_X");

                        buttonBack = TextureBank.GetTexturePersistent($"{subdir}/Switch_Minus");
                        buttonStart = TextureBank.GetTexturePersistent($"{subdir}/Switch_Plus");

                        dpad = TextureBank.GetTexturePersistent($"{subdir}/Switch_Dpad");
                        dpadDown = TextureBank.GetTexturePersistent($"{subdir}/Switch_Dpad_Down");
                        dpadUp = TextureBank.GetTexturePersistent($"{subdir}/Switch_Dpad_Up");
                        dpadLeft = TextureBank.GetTexturePersistent($"{subdir}/Switch_Dpad_Left");
                        dpadRight = TextureBank.GetTexturePersistent($"{subdir}/Switch_Dpad_Right");

                        buttonLB = TextureBank.GetTexturePersistent($"{subdir}/Switch_LB");
                        buttonRB = TextureBank.GetTexturePersistent($"{subdir}/Switch_RB");
                        buttonLT = TextureBank.GetTexturePersistent($"{subdir}/Switch_LT");
                        buttonRT = TextureBank.GetTexturePersistent($"{subdir}/Switch_RT");

                        leftStick = TextureBank.GetTexturePersistent($"{subdir}/Switch_Left_Stick");
                        rightStick = TextureBank.GetTexturePersistent($"{subdir}/Switch_Right_Stick");
                        leftStickClick = TextureBank.GetTexturePersistent("input/Xbox 360/360_Left_Stick_Click"); // graphics missing
                        rightStickClick = TextureBank.GetTexturePersistent("input/Xbox 360/360_Right_Stick_Click");  // graphics missing

                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported button scheme type - {currentType}.");
            }

            SonOfRobinGame.ControlTips.RefreshLayout();
        }
    }
}