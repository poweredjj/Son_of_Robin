using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class Helpers
    {
        public const double Rad2Deg = 180.0 / Math.PI;
        public const double Deg2Rad = Math.PI / 180.0;

        public static void DrawRectangleOutline(Rectangle rect, Color color, int borderWidth)
        {
            // top
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), color);
            // bottom
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(rect.X, (rect.Y + rect.Height) - borderWidth, rect.Width, borderWidth), color);
            // left
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), color);
            // right
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, new Rectangle((rect.X + rect.Width) - borderWidth, rect.Y, borderWidth, rect.Height), color);
        }

        public static string FirstCharToLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0])) return str;
            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string FirstCharToUpperCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsUpper(str[0])) return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static void SetProperty(Object targetObj, string propertyName, Object newValue)
        {
            Type type = targetObj.GetType();
            FieldInfo field = type.GetField(propertyName);
            if (field != null) field.SetValue(targetObj, newValue);
            else
            {
                var property = type.GetProperty(propertyName);
                property.SetValue(targetObj, newValue);
            }
        }

        public static Object GetProperty(Object targetObj, string propertyName)
        {
            Type type = targetObj.GetType();

            FieldInfo field = type.GetField(propertyName);
            if (field != null) return field.GetValue(targetObj);
            else
            {
                var property = type.GetProperty(propertyName);
                return property.GetValue(targetObj);
            }
        }

        public static float GetAngleBetweenTwoPoints(Vector2 start, Vector2 end)
        {
            return (float)Math.Atan2((start.Y - end.Y) * -1f, end.X - start.X);
        }

        public static string ToSentenceCase(string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }


    }
}
