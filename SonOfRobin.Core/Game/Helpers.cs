using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class Helpers
    {
        public struct TemplateData
        {
            public readonly int seed;
            public readonly int width;
            public readonly int height;

            public TemplateData(string folderName)
            {
                this.seed = Convert.ToInt32(folderName.Split('_')[1]);
                string widthHeight = folderName.Split('_')[2];
                this.width = Convert.ToInt32(widthHeight.Split('x')[0]);
                this.height = Convert.ToInt32(widthHeight.Split('x')[1]);
            }
        }


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

        public static Vector2 VectorAbsMax(Vector2 vector, float maxVal)
        {
            if (vector.X > 0) vector.X = Math.Min(vector.X, maxVal);
            else vector.X = Math.Max(-maxVal, vector.X);

            if (vector.Y > 0) vector.Y = Math.Min(vector.Y, maxVal);
            else vector.Y = Math.Max(-maxVal, vector.Y);

            return vector;
        }

        public static bool IsPointInsideTriangle(Point point, Point triangleA, Point triangleB, Point triangleC)
        {
            var s = (triangleA.X - triangleC.X) * (point.Y - triangleC.Y) - (triangleA.Y - triangleC.Y) * (point.X - triangleC.X);
            var t = (triangleB.X - triangleA.X) * (point.Y - triangleA.Y) - (triangleB.Y - triangleA.Y) * (point.X - triangleA.X);

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            var d = (triangleC.X - triangleB.X) * (point.Y - triangleB.Y) - (triangleC.Y - triangleB.Y) * (point.X - triangleB.X);
            return d == 0 || (d < 0) == (s + t <= 0);
        }



    }
}
