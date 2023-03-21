using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class Helpers
    {
        public const double Rad2Deg = 180.0 / Math.PI;
        public const double Deg2Rad = Math.PI / 180.0;

        public enum AlignX
        { Left, Center, Right };

        public enum AlignY
        { Top, Center, Bottom };

        private static int hashCounter = 0;

        public static string GetUniqueHash()
        {
            hashCounter++;
            return $"{Math.Abs(DateTime.Now.GetHashCode())}_{hashCounter}";
        }

        public static void DrawTextWithOutline(SpriteFont font, string text, Vector2 pos, Color color, Color outlineColor, int outlineSize = 1)
        {
            if (outlineSize > 0)
            {
                for (int x = -outlineSize; x <= outlineSize; x++)
                {
                    for (int y = -outlineSize; y <= outlineSize; y++)
                    {
                        SonOfRobinGame.SpriteBatch.DrawString(font, text, pos + new Vector2(x, y), outlineColor);
                    }
                }
            }

            SonOfRobinGame.SpriteBatch.DrawString(font, text, pos, color);
        }

        public static void DrawTextInsideRectWithOutline(SpriteFont font, Rectangle rectangle, string text, Color color, Color outlineColor, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, int outlineSize = 0, bool drawTestRect = false)
        {
            var outlineRectList = new List<Rectangle>();

            if (outlineSize > 0)
            {
                for (int x = -outlineSize; x <= outlineSize; x++)
                {
                    for (int y = -outlineSize; y <= outlineSize; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        Rectangle outlineRect = rectangle;
                        outlineRect.X += x;
                        outlineRect.Y += y;
                        outlineRectList.Add(outlineRect);
                    }
                }
            }

            foreach (Rectangle outlineRect in outlineRectList)
            {
                DrawTextInsideRect(font: font, rectangle: outlineRect, text: text, color: outlineColor, alignX: alignX, alignY: alignY, drawTestRect: false);
            }

            DrawTextInsideRect(font: font, rectangle: rectangle, text: text, color: color, alignX: alignX, alignY: alignY, drawTestRect: drawTestRect);
        }

        public static void DrawTextInsideRectWithShadow(SpriteFont font, Rectangle rectangle, string text, Color color, Color shadowColor, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, int shadowOffsetX = 0, int shadowOffsetY = 0, int shadowOffset = 0, bool drawTestRect = false)
        {
            if (shadowOffset != 0)
            {
                shadowOffsetX = shadowOffset;
                shadowOffsetY = shadowOffset;
            }

            if (shadowOffsetX != 0 && shadowOffsetY != 0)
            {
                Rectangle shadowRect = rectangle;
                shadowRect.X += shadowOffsetX;
                shadowRect.Y += shadowOffsetY;

                DrawTextInsideRect(font: font, rectangle: shadowRect, text: text, color: shadowColor, alignX: alignX, alignY: alignY, drawTestRect: drawTestRect);
            }

            DrawTextInsideRect(font: font, rectangle: rectangle, text: text, color: color, alignX: alignX, alignY: alignY, drawTestRect: drawTestRect);
        }

        public static void DrawTextInsideRect(SpriteFont font, Rectangle rectangle, string text, Color color, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, bool drawTestRect = false)
        {
            Vector2 textSize = font.MeasureString(text);
            float scale = Math.Min(rectangle.Width / textSize.X, rectangle.Height / textSize.Y);

            if (drawTestRect) DrawRectangleOutline(rect: rectangle, color: Color.White, borderWidth: 1);

            int xOffset, yOffset;
            switch (alignX)
            {
                case AlignX.Left:
                    xOffset = 0;
                    break;

                case AlignX.Center:
                    xOffset = (int)((rectangle.Width - (textSize.X * scale)) / 2);
                    break;

                case AlignX.Right:
                    xOffset = (int)(rectangle.Width - (textSize.X * scale));
                    break;

                default:
                    throw new ArgumentException($"Unsupported alignX - {alignX}.");
            }

            switch (alignY)
            {
                case AlignY.Top:
                    yOffset = 0;
                    break;

                case AlignY.Center:
                    yOffset = (int)((rectangle.Height - (textSize.Y * scale)) / 2);
                    break;

                case AlignY.Bottom:
                    yOffset = (int)(rectangle.Height - (textSize.Y * scale));
                    break;

                default:
                    throw new ArgumentException($"Unsupported alignY - {alignY}.");
            }

            SonOfRobinGame.SpriteBatch.DrawString(font, text, position: new Vector2(rectangle.X + xOffset, rectangle.Y + yOffset), color: color, origin: Vector2.Zero, scale: scale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        public static Rectangle DrawTextureInsideRect(Texture2D texture, Rectangle rectangle, Color color, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, bool drawTestRect = false)
        {
            float scale = Math.Min((float)rectangle.Width / (float)texture.Width, (float)rectangle.Height / (float)texture.Height);
            Vector2 scaledTexture = new Vector2(texture.Width * scale, texture.Height * scale);

            if (drawTestRect) DrawRectangleOutline(rect: rectangle, color: Color.White, borderWidth: 1);

            int xOffset, yOffset;
            switch (alignX)
            {
                case AlignX.Left:
                    xOffset = 0;
                    break;

                case AlignX.Center:
                    xOffset = (int)((rectangle.Width - scaledTexture.X) / 2);
                    break;

                case AlignX.Right:
                    xOffset = (int)(rectangle.Width - scaledTexture.X);
                    break;

                default:
                    throw new ArgumentException($"Unsupported alignX - {alignX}.");
            }

            switch (alignY)
            {
                case AlignY.Top:
                    yOffset = 0;
                    break;

                case AlignY.Center:
                    yOffset = (int)((rectangle.Height - scaledTexture.Y) / 2);
                    break;

                case AlignY.Bottom:
                    yOffset = (int)(rectangle.Height - scaledTexture.Y);
                    break;

                default:
                    throw new ArgumentException($"Unsupported alignY - {alignY}.");
            }

            Rectangle destRect = new Rectangle(x: rectangle.X + xOffset, y: rectangle.Y + yOffset, width: (int)(texture.Width * scale), height: (int)(texture.Height * scale));
            if (drawTestRect) DrawRectangleOutline(rect: destRect, color: Color.Green, borderWidth: 1);
            SonOfRobinGame.SpriteBatch.Draw(texture: texture, destinationRectangle: destRect, color: color);

            return destRect;
        }

        public static void DrawRectangleOutline(Rectangle rect, Color color, int borderWidth)
        {
            // top
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), color);
            // bottom
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(rect.X, (rect.Y + rect.Height) - borderWidth, rect.Width, borderWidth), color);
            // left
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), color);
            // right
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, new Rectangle((rect.X + rect.Width) - borderWidth, rect.Y, borderWidth, rect.Height), color);
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

        public static float GetAngleBetweenTwoPoints(Point start, Vector2 end)
        {
            return (float)Math.Atan2((start.Y - end.Y) * -1f, end.X - start.X);
        }

        public static float GetAngleBetweenTwoPoints(Point start, Point end)
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

        public static List<object> GetDuplicates(List<object> objectList)
        {
            return objectList.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
        }

        public static List<List<T>> SplitList<T>(List<T> listToSplit, int maxCount)
        {
            if (maxCount < 1) throw new ArgumentException($"MaxCount ({maxCount}) cannot be less than 1.");

            var list = new List<List<T>>();
            for (int i = 0; i < listToSplit.Count; i += maxCount)
            {
                list.Add(listToSplit.GetRange(i, Math.Min(maxCount, listToSplit.Count - i)));
            }

            return list;
        }

        public static double ConvertRange(double oldMin, double oldMax, double newMin, double newMax, double oldVal, bool clampToEdges)
        {
            double newVal = ((oldVal - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;

            if (clampToEdges)
            {
                newVal = Math.Min(newVal, newMax);
                newVal = Math.Max(newVal, newMin);
            }

            return newVal;
        }

        public static bool IsPowerOfTwo(ulong x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static float GetRandomFloatForRange(Random random, float minVal, float maxVal)
        {
            return (float)(random.NextDouble() * (maxVal - minVal) + minVal);
        }
    }
}