using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class KeyboardScheme
    {
        public static Dictionary<Keys, Texture2D> KeyTextures { get { return keyTextures; } }

        private static readonly Dictionary<Keys, Texture2D> keyTextures = new Dictionary<Keys, Texture2D>();

        private static readonly Dictionary<Keys, string> keyNames = new Dictionary<Keys, string>
        {
            {Keys.Back, "Backspace_Alt_Key_Light"},
            {Keys.Tab, "Tab_Key_Light"},
            {Keys.Enter, "Enter_Alt_Key_Light"},
            {Keys.CapsLock, "Caps_Lock_Key_Light"},
            {Keys.Escape, "Esc_Key_Light"},
            {Keys.Space, "Space_Key_Light"},
            {Keys.PageUp, "Page_Up_Key_Light"},
            {Keys.PageDown, "Page_Down_Key_Light"},
            {Keys.End, "End_Key_Light"},
            {Keys.Home, "Home_Key_Light"},
            {Keys.Left, "Arrow_Left_Key_Light"},
            {Keys.Up, "Arrow_Up_Key_Light"},
            {Keys.Right, "Arrow_Right_Key_Light"},
            {Keys.Down, "Arrow_Down_Key_Light"},
            {Keys.Insert, "Insert_Key_Light"},
            {Keys.Delete, "Del_Key_Light"},
            {Keys.D0, "0_Key_Light"},
            {Keys.D1, "1_Key_Light"},
            {Keys.D2, "2_Key_Light"},
            {Keys.D3, "3_Key_Light"},
            {Keys.D4, "4_Key_Light"},
            {Keys.D5, "5_Key_Light"},
            {Keys.D6, "6_Key_Light"},
            {Keys.D7, "7_Key_Light"},
            {Keys.D8, "8_Key_Light"},
            {Keys.D9, "9_Key_Light"},
            {Keys.A, "A_Key_Light"},
            {Keys.B, "B_Key_Light"},
            {Keys.C, "C_Key_Light"},
            {Keys.D, "D_Key_Light"},
            {Keys.E, "E_Key_Light"},
            {Keys.F, "F_Key_Light"},
            {Keys.G, "G_Key_Light"},
            {Keys.H, "H_Key_Light"},
            {Keys.I, "I_Key_Light"},
            {Keys.J, "J_Key_Light"},
            {Keys.K, "K_Key_Light"},
            {Keys.L, "L_Key_Light"},
            {Keys.M, "M_Key_Light"},
            {Keys.N, "N_Key_Light"},
            {Keys.O, "O_Key_Light"},
            {Keys.P, "P_Key_Light"},
            {Keys.Q, "Q_Key_Light"},
            {Keys.R, "R_Key_Light"},
            {Keys.S, "S_Key_Light"},
            {Keys.T, "T_Key_Light"},
            {Keys.U, "U_Key_Light"},
            {Keys.V, "V_Key_Light"},
            {Keys.W, "W_Key_Light"},
            {Keys.X, "X_Key_Light"},
            {Keys.Y, "Y_Key_Light"},
            {Keys.Z, "Z_Key_Light"},
            {Keys.LeftWindows, "Win_Key_Light"},
            {Keys.RightWindows, "Win_Key_Light"},
            {Keys.NumPad0, "0_Key_Light"},
            {Keys.NumPad1, "1_Key_Light"},
            {Keys.NumPad2, "2_Key_Light"},
            {Keys.NumPad3, "3_Key_Light"},
            {Keys.NumPad4, "4_Key_Light"},
            {Keys.NumPad5, "5_Key_Light"},
            {Keys.NumPad6, "6_Key_Light"},
            {Keys.NumPad7, "7_Key_Light"},
            {Keys.NumPad8, "8_Key_Light"},
            {Keys.NumPad9, "9_Key_Light"},
            {Keys.Multiply, "Asterisk_Key_Light"},
            {Keys.Divide, "Slash_Key_Light"},
            {Keys.OemPipe, "Hash_Key_Light"},
            {Keys.Add, "Plus_Tall_Key_Light"},
            {Keys.Subtract, "Minus_Key_Light"},
            {Keys.F1, "F1_Key_Light"},
            {Keys.F2, "F2_Key_Light"},
            {Keys.F3, "F3_Key_Light"},
            {Keys.F4, "F4_Key_Light"},
            {Keys.F5, "F5_Key_Light"},
            {Keys.F6, "F6_Key_Light"},
            {Keys.F7, "F7_Key_Light"},
            {Keys.F8, "F8_Key_Light"},
            {Keys.F9, "F9_Key_Light"},
            {Keys.F10, "F10_Key_Light"},
            {Keys.F11, "F11_Key_Light"},
            {Keys.F12, "F12_Key_Light"},
            {Keys.NumLock, "Num_Lock_Key_Light"},
            {Keys.LeftShift, "Shift_Key_L_Light"},
            {Keys.RightShift, "Shift_Key_R_Light"},
            {Keys.LeftControl, "Ctrl_Key_Light_L"},
            {Keys.RightControl, "Ctrl_Key_Light_R"},
            {Keys.LeftAlt, "Alt_Key_L_Light"},
            {Keys.RightAlt, "Alt_Key_R_Light"}, // under Windows, right alt is being recognized as left control + left alt
            {Keys.OemSemicolon, "Semicolon_Key_Light"},
            {Keys.OemPlus, "Plus_Key_Light"},
            {Keys.OemComma, "Mark_Left_Key_Light"},
            {Keys.OemPeriod, "Mark_Right_Key_Light"},
            {Keys.OemMinus, "Minus_Key_Light"},
            {Keys.OemQuestion, "Question_Key_Light"},
            {Keys.OemTilde, "Tilda_Key_Light"},
            {Keys.OemOpenBrackets, "Bracket_Left_Key_Light"},
            {Keys.OemCloseBrackets, "Bracket_Right_Key_Light"},
            {Keys.OemQuotes, "Quote_Key_Light"},
            {Keys.OemBackslash, "Backslash_Key_Light"}
        };

        public static void LoadAllKeys()
        { LoadKeys(keyNames.Keys.ToList()); }

        public static void LoadKeys(List<Keys> keyList)
        {
            foreach (Keys key in keyList)
            {
                keyTextures[key] = SonOfRobinGame.content.Load<Texture2D>($"gfx/Keyboard/{keyNames[key]}");
            }
        }
        public static Texture2D GetTexture(Keys key)
        {
            if (!keyTextures.ContainsKey(key)) return null;
            return keyTextures[key];
        }
    }

}

