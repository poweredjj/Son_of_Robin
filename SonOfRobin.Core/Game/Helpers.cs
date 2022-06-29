using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SonOfRobin
{
    public class Helpers
    {
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
            if (field != null)
            { field.SetValue(targetObj, newValue); }
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
            if (field != null)
            { return field.GetValue(targetObj); }
            else
            {
                var property = type.GetProperty(propertyName);
                return property.GetValue(targetObj);
            }
        }

    }
}
