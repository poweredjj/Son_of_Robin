using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class ButtonScheme
    {
        public enum Type { M, S, N }

        private static Type currentType;

        public static Texture2D plus; // for buttons that need to be pressed together

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

        public static string buttonNameA;
        public static string buttonNameB;
        public static string buttonNameX;
        public static string buttonNameY;

        public static string buttonNameBack;
        public static string buttonNameStart;

        public static string buttonNameLB;
        public static string buttonNameRB;
        public static string buttonNameLT;
        public static string buttonNameRT;

        public static void ChangeType(Type type)
        {
            currentType = type;

            plus = SonOfRobinGame.content.Load<Texture2D>("gfx/plus");

            switch (currentType)
            {
                case Type.M:
                    buttonA = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_A");
                    buttonB = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_B");
                    buttonX = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_X");
                    buttonY = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Y");

                    buttonBack = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Back");
                    buttonStart = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Start");

                    dpad = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Dpad");
                    dpadDown = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Down");
                    dpadUp = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Up");
                    dpadLeft = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Left");
                    dpadRight = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Dpad_Right");

                    buttonLB = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_LB");
                    buttonRB = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_RB");
                    buttonLT = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_LT");
                    buttonRT = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_RT");

                    leftStick = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Left_Stick");
                    rightStick = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Right_Stick");
                    leftStickClick = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Left_Stick_Click");
                    rightStickClick = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Right_Stick_Click");

                    buttonNameA = "A";
                    buttonNameB = "B";
                    buttonNameX = "X";
                    buttonNameY = "Y";
                    buttonNameBack = "back";
                    buttonNameStart = "start";
                    buttonNameLB = "LB";
                    buttonNameRB = "RB";
                    buttonNameLT = "LT";
                    buttonNameRT = "RT";

                    break;

                case Type.S:

                    buttonA = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Cross");
                    buttonB = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Circle");
                    buttonX = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Square");
                    buttonY = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Triangle");

                    buttonBack = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Share"); // TODO check if this mapping is correct
                    buttonStart = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Options");

                    dpad = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Dpad");
                    dpadDown = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Dpad_Down");
                    dpadUp = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Dpad_Up");
                    dpadLeft = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Dpad_Left");
                    dpadRight = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Dpad_Right");

                    buttonLB = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_L1");
                    buttonRB = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_R1");
                    buttonLT = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_L2");
                    buttonRT = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_R2");

                    leftStick = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Left_Stick");
                    rightStick = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Right_Stick");
                    leftStickClick = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Left_Stick_Click");
                    rightStickClick = SonOfRobinGame.content.Load<Texture2D>("gfx/PS4/PS4_Right_Stick_Click");

                    buttonNameA = "cross";
                    buttonNameB = "circle";
                    buttonNameX = "square";
                    buttonNameY = "triangle";
                    buttonNameBack = "share";
                    buttonNameStart = "options";
                    buttonNameLB = "L1";
                    buttonNameRB = "R1";
                    buttonNameLT = "L2";
                    buttonNameRT = "R2";

                    break;

                case Type.N:
                    buttonA = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_B");
                    buttonB = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_A");
                    buttonX = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Y");
                    buttonY = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_X");

                    buttonBack = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Minus");
                    buttonStart = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Plus");

                    dpad = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Dpad");
                    dpadDown = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Dpad_Down");
                    dpadUp = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Dpad_Up");
                    dpadLeft = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Dpad_Left");
                    dpadRight = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Dpad_Right");

                    buttonLB = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_LB");
                    buttonRB = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_RB");
                    buttonLT = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_LT");
                    buttonRT = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_RT");

                    leftStick = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Left_Stick");
                    rightStick = SonOfRobinGame.content.Load<Texture2D>("gfx/Switch/Switch_Right_Stick");
                    leftStickClick = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Left_Stick_Click"); // graphics missing 
                    rightStickClick = SonOfRobinGame.content.Load<Texture2D>("gfx/Xbox 360/360_Right_Stick_Click");  // graphics missing 

                    buttonNameA = "B";
                    buttonNameB = "A";
                    buttonNameX = "Y";
                    buttonNameY = "X";
                    buttonNameBack = "minus";
                    buttonNameStart = "plus";
                    buttonNameLB = "L";
                    buttonNameRB = "R";
                    buttonNameLT = "zL";
                    buttonNameRT = "zR";

                    break;

                default:
                    throw new DivideByZeroException($"Unsupported button scheme type - {currentType}.");
            }

            SonOfRobinGame.controlTips.RefreshLayout();
        }

    }
}
