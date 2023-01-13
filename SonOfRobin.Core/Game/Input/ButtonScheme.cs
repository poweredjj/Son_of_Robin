using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class ButtonScheme
    {
        public enum Type
        { Xbox360, XboxSeries, DualShock4, DualSense, SwitchProController }

        private static Type currentType;

        public static readonly Texture2D plus = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/input/plus"); // for buttons that need to be pressed together

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
                        string subdir = "gfx/input/Xbox 360";

                        buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_A");
                        buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_B");
                        buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_X");
                        buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Y");

                        buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Back");
                        buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Start");

                        dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Dpad");
                        dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Dpad_Down");
                        dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Dpad_Up");
                        dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Dpad_Left");
                        dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Dpad_Right");

                        buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_LB");
                        buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_RB");
                        buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_LT");
                        buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_RT");

                        leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Left_Stick");
                        rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Right_Stick");
                        leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Left_Stick_Click");
                        rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/360_Right_Stick_Click");

                        break;
                    }

                case Type.XboxSeries:
                    {
                        string subdir = $"gfx/input/Xbox Series";

                        buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_A");
                        buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_B");
                        buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_X");
                        buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Y");

                        buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_View");
                        buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Menu");

                        dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Dpad");
                        dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Dpad_Down");
                        dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Dpad_Up");
                        dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Dpad_Left");
                        dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Dpad_Right");

                        buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_LB");
                        buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_RB");
                        buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_LT");
                        buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_RT");

                        leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Left_Stick");
                        rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Right_Stick");
                        leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Left_Stick_Click");
                        rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/XboxSeriesX_Right_Stick_Click");

                        break;
                    }

                case Type.DualShock4:
                    {
                        string subdir = $"gfx/input/PS4";

                        buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Cross");
                        buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Circle");
                        buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Square");
                        buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Triangle");

                        buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Share"); // TODO check if this mapping is correct
                        buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Options");

                        dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Dpad");
                        dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Dpad_Down");
                        dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Dpad_Up");
                        dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Dpad_Left");
                        dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Dpad_Right");

                        buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_L1");
                        buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_R1");
                        buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_L2");
                        buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_R2");

                        leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Left_Stick");
                        rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Right_Stick");
                        leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Left_Stick_Click");
                        rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS4_Right_Stick_Click");

                        break;
                    }

                case Type.DualSense:
                    {
                        string subdir = $"gfx/input/PS5";

                        buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Cross");
                        buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Circle");
                        buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Square");
                        buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Triangle");

                        buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Share"); // TODO check if this mapping is correct
                        buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Options");

                        dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Dpad");
                        dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Dpad_Down");
                        dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Dpad_Up");
                        dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Dpad_Left");
                        dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Dpad_Right");

                        buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_L1");
                        buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_R1");
                        buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_L2");
                        buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_R2");

                        leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Left_Stick");
                        rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Right_Stick");
                        leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Left_Stick_Click");
                        rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/PS5_Right_Stick_Click");

                        break;
                    }

                case Type.SwitchProController:
                    {
                        string subdir = $"gfx/input/Switch";

                        buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_B");
                        buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_A");
                        buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Y");
                        buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_X");

                        buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Minus");
                        buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Plus");

                        dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Dpad");
                        dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Dpad_Down");
                        dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Dpad_Up");
                        dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Dpad_Left");
                        dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Dpad_Right");

                        buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_LB");
                        buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_RB");
                        buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_LT");
                        buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_RT");

                        leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Left_Stick");
                        rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>($"{subdir}/Switch_Right_Stick");
                        leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/input/Xbox 360/360_Left_Stick_Click"); // graphics missing
                        rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/input/Xbox 360/360_Right_Stick_Click");  // graphics missing

                        break;
                    }

                default:
                    throw new ArgumentException($"Unsupported button scheme type - {currentType}.");
            }

            SonOfRobinGame.ControlTips.RefreshLayout();
        }
    }
}