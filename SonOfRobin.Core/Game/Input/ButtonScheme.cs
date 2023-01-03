using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class ButtonScheme
    {
        public enum Type
        { Xbox360, XboxSeries, DualShock4, DualSense, SwitchProController }

        private static Type currentType;

        public static readonly Texture2D plus = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/plus"); // for buttons that need to be pressed together

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
                    buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_A");
                    buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_B");
                    buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_X");
                    buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Y");

                    buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Back");
                    buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Start");

                    dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Dpad");
                    dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Down");
                    dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Up");
                    dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Left");
                    dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Right");

                    buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_LB");
                    buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_RB");
                    buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_LT");
                    buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_RT");

                    leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Left_Stick");
                    rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Right_Stick");
                    leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Left_Stick_Click");
                    rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Right_Stick_Click");

                    break;

                case Type.XboxSeries:
                    buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_A");
                    buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_B");
                    buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_X");
                    buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Y");

                    buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_View");
                    buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Menu");

                    dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Dpad");
                    dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Dpad_Down");
                    dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Dpad_Up");
                    dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Dpad_Left");
                    dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Dpad_Right");

                    buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_LB");
                    buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_RB");
                    buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_LT");
                    buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_RT");

                    leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Left_Stick");
                    rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Right_Stick");
                    leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Left_Stick_Click");
                    rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox Series/XboxSeriesX_Right_Stick_Click");

                    break;

                case Type.DualShock4:

                    buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Cross");
                    buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Circle");
                    buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Square");
                    buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Triangle");

                    buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Share"); // TODO check if this mapping is correct
                    buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Options");

                    dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Dpad");
                    dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Dpad_Down");
                    dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Dpad_Up");
                    dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Dpad_Left");
                    dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Dpad_Right");

                    buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_L1");
                    buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_R1");
                    buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_L2");
                    buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_R2");

                    leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Left_Stick");
                    rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Right_Stick");
                    leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Left_Stick_Click");
                    rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS4/PS4_Right_Stick_Click");

                    break;

                case Type.DualSense:

                    buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Cross");
                    buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Circle");
                    buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Square");
                    buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Triangle");

                    buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Share"); // TODO check if this mapping is correct
                    buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Options");

                    dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Dpad");
                    dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Dpad_Down");
                    dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Dpad_Up");
                    dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Dpad_Left");
                    dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Dpad_Right");

                    buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_L1");
                    buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_R1");
                    buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_L2");
                    buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_R2");

                    leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Left_Stick");
                    rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Right_Stick");
                    leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Left_Stick_Click");
                    rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/PS5/PS5_Right_Stick_Click");

                    break;

                case Type.SwitchProController:

                    buttonA = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_B");
                    buttonB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_A");
                    buttonX = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Y");
                    buttonY = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_X");

                    buttonBack = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Minus");
                    buttonStart = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Plus");

                    dpad = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Dpad");
                    dpadDown = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Dpad_Down");
                    dpadUp = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Dpad_Up");
                    dpadLeft = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Dpad_Left");
                    dpadRight = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Dpad_Right");

                    buttonLB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_LB");
                    buttonRB = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_RB");
                    buttonLT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_LT");
                    buttonRT = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_RT");

                    leftStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Left_Stick");
                    rightStick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Switch/Switch_Right_Stick");
                    leftStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Left_Stick_Click"); // graphics missing
                    rightStickClick = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/Xbox 360/360_Right_Stick_Click");  // graphics missing

                    break;

                default:
                    throw new ArgumentException($"Unsupported button scheme type - {currentType}.");
            }

            SonOfRobinGame.ControlTips.RefreshLayout();
        }
    }
}