using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class Helpers
    {
        public const double Rad2Deg = 180.0 / Math.PI;
        public const double Deg2Rad = Math.PI / 180.0;

        public enum AlignX : byte
        {
            Left = 0,
            Center = 1,
            Right = 2,
        };

        public enum AlignY : byte
        {
            Top = 0,
            Center = 1,
            Bottom = 2,
        };

        private static int hashCounter = 0;

        public static string GetUniqueHash()
        {
            hashCounter++;
            return $"{Math.Abs(DateTime.Now.GetHashCode())}_{hashCounter}";
        }

        public static void DrawTextWithOutline(SpriteFont font, string text, Vector2 pos, Color color, Color outlineColor, int outlineSize = 1, bool centered = false, float scale = 1f)
        {
            if (centered)
            {
                Vector2 textSize = font.MeasureString(text) * scale;
                pos.X -= textSize.X / 2;
                pos.Y -= textSize.Y / 2;
            }

            if (outlineSize > 0)
            {
                for (int x = -outlineSize; x <= outlineSize; x++)
                {
                    for (int y = -outlineSize; y <= outlineSize; y++)
                    {
                        SonOfRobinGame.SpriteBatch.DrawString(spriteFont: font, text: text, position: pos + new Vector2(x, y), color: outlineColor, rotation: 0, origin: Vector2.Zero, scale: scale, effects: SpriteEffects.None, layerDepth: 0);

                    }
                }
            }

            SonOfRobinGame.SpriteBatch.DrawString(spriteFont: font, text: text, position: pos, color: color, rotation: 0, origin: Vector2.Zero, scale: scale, effects: SpriteEffects.None, layerDepth: 0);
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

        public static Rectangle DrawTextureInsideRect(Texture2D texture, Rectangle rectangle, Color color, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, bool drawTestRect = false, float rotation = 0)
        {
            float scale = Math.Min((float)rectangle.Width / (float)texture.Width, (float)rectangle.Height / (float)texture.Height);

            if (rotation != 0)
            {
                // rotated textured must fit inside the rect too
                float rotatedScale = Math.Min((float)rectangle.Width / (float)texture.Height, (float)rectangle.Height / (float)texture.Width);
                if (rotatedScale < scale) scale = rotatedScale;
            }

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

            if (rotation == 0) SonOfRobinGame.SpriteBatch.Draw(texture: texture, destinationRectangle: destRect, color: color);
            else
            {
                destRect.X += destRect.Width / 2;
                destRect.Y += destRect.Height / 2;

                SonOfRobinGame.SpriteBatch.Draw(texture: texture, sourceRectangle: new Rectangle(x: 0, y: 0, width: texture.Width, height: texture.Height), origin: new Vector2(texture.Width * 0.5f, texture.Height * 0.5f), destinationRectangle: destRect, color: color, rotation: rotation, effects: SpriteEffects.None, layerDepth: 0);
            }

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

        public static string CapitalizeFirstLetterOfWords(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input);
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

        public static Vector2 VectorKeepBelowSetValue(Vector2 vector, float maxVal)
        {
            bool isXNegative = vector.X < 0;
            bool isYNegative = vector.Y < 0;

            float newXComponent = Math.Min(Math.Abs(vector.X), maxVal);
            float newYComponent = Math.Min(Math.Abs(vector.Y), maxVal);

            if (isXNegative) newXComponent = -newXComponent;
            if (isYNegative) newYComponent = -newYComponent;

            return new Vector2(newXComponent, newYComponent);
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

        public static bool DoRectangleAndCircleOverlap(Vector2 circleCenter, float circleRadius, Rectangle rect)
        {
            // Get the rectangle half width and height
            float rW = rect.Width / 2;
            float rH = rect.Height / 2;

            // Get the positive distance. This exploits the symmetry so that we now are
            // just solving for one corner of the rectangle (memory tell me it fabs for
            // floats but I could be wrong and its abs)
            float distX = Math.Abs(circleCenter.X - (rect.Left + rW));
            float distY = Math.Abs(circleCenter.Y - (rect.Top + rH));

            if (distX >= circleRadius + rW || distY >= circleRadius + rH) return false;
            if (distX < rW || distY < rH) return true;

            // Now only circles C and D left to test
            // get the distance to the corner
            distX -= rW;
            distY -= rH;

            // Find distance to corner and compare to circle radius
            // (squared and the sqrt root is not needed)
            if (distX * distX + distY * distY < circleRadius * circleRadius) return true;
            return false;
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
            return (float)(random.NextSingle() * (maxVal - minVal) + minVal);
        }

        public static Dictionary<DateTime, TimeSpan> GetTimeOfDayOccurrences(DateTime startTime, DateTime endTime, TimeSpan checkTimeStart, TimeSpan checkTimeEnd)
        {
            // start example: TimeSpan checkTimeStart = TimeSpan.FromHours(6);
            // end example: TimeSpan checkTimeEnd = TimeSpan.FromHours(8);

            // Create a dictionary to store the start time and duration of each occurrence
            Dictionary<DateTime, TimeSpan> occurrences = new Dictionary<DateTime, TimeSpan>();

            // Loop through each day in the specified time range
            for (DateTime date = startTime.Date; date <= endTime.Date; date = date.AddDays(1))
            {
                // Combine the date with the start and end times for the day
                DateTime dayStart = date + checkTimeStart;
                DateTime dayEnd = date + checkTimeEnd;

                // Check if the time period overlaps with the specified time range
                if (dayStart <= endTime && dayEnd >= startTime)
                {
                    // Calculate the start and end times for the overlapping time period
                    DateTime periodStart = dayStart < startTime ? startTime : dayStart;
                    DateTime periodEnd = dayEnd > endTime ? endTime : dayEnd;

                    // Add the start time and duration of the period to the dictionary
                    occurrences.Add(periodStart, periodEnd - periodStart);
                }
            }

            return occurrences;
        }

        public static Color Blend2Colors(Color firstColor, Color secondColor, float firstColorOpacity, float secondColorOpacity)
        {
            return new Color(
                        (byte)((firstColor.R * firstColorOpacity) + (secondColor.R * secondColorOpacity)),
                        (byte)((firstColor.G * firstColorOpacity) + (secondColor.G * secondColorOpacity)),
                        (byte)((firstColor.B * firstColorOpacity) + (secondColor.B * secondColorOpacity)),
                        (byte)((firstColor.A * firstColorOpacity) + (secondColor.A * secondColorOpacity))
                        );
        }

        public static Color DarkenFirstColorWithSecond(Color firstColor, Color secondColor, float firstColorOpacity, float secondColorOpacity)
        {
            return new Color(
                     Math.Min((byte)((firstColor.R * firstColorOpacity) + (secondColor.R * secondColorOpacity)), firstColor.R),
                     Math.Min((byte)((firstColor.G * firstColorOpacity) + (secondColor.G * secondColorOpacity)), firstColor.G),
                     Math.Min((byte)((firstColor.B * firstColorOpacity) + (secondColor.B * secondColorOpacity)), firstColor.B),
                     Math.Max((byte)((firstColor.A * firstColorOpacity) + (secondColor.A * secondColorOpacity)), firstColor.A)
                     );
        }

        public static int CastObjectToInt(object obj) // for json deserialization, because it doesn't store types properly
        {
            int convertedObj;
            try
            { convertedObj = (int)(Int64)obj; }
            catch (Exception)
            { convertedObj = (int)obj; }

            return convertedObj;
        }

        public static ushort CastObjectToUshort(object obj) // for json deserialization, because it doesn't store types properly
        {
            ushort convertedObj;
            try
            { convertedObj = (ushort)(Int64)obj; }
            catch (Exception)
            { convertedObj = Convert.ToUInt16(obj); }

            return convertedObj;
        }

        public static byte CastObjectToByte(object obj) // for json deserialization, because it doesn't store types properly
        {
            byte convertedObj;
            try
            { convertedObj = (byte)(Int64)obj; }
            catch (Exception)
            { convertedObj = (byte)obj; }

            return convertedObj;
        }

        public static float CastObjectToFloat(object obj) // for json deserialization, because it doesn't store types properly
        {
            float convertedObj;
            try
            { convertedObj = (float)(double)obj; }
            catch (Exception)
            { convertedObj = (float)obj; }

            return convertedObj;
        }

        public static Point FitIntoSize(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            double sourceAspectRatio = (double)sourceWidth / sourceHeight;
            double targetAspectRatio = (double)targetWidth / targetHeight;

            int width, height;

            if (sourceAspectRatio > targetAspectRatio)
            {
                // Letterboxing on top and bottom
                height = targetHeight;
                width = (int)(height * sourceAspectRatio);

                // Check if the resulting width exceeds the target width
                if (width > targetWidth)
                {
                    width = targetWidth;
                    height = (int)(width / sourceAspectRatio);
                }
            }
            else
            {
                // Letterboxing on left and right
                width = targetWidth;
                height = (int)(width / sourceAspectRatio);

                // Check if the resulting height exceeds the target height
                if (height > targetHeight)
                {
                    height = targetHeight;
                    width = (int)(height * sourceAspectRatio);
                }
            }

            return new Point(width, height);
        }

        public static bool ZipFiles(string sourcePath, string zipPath)
        {
            try
            {
                // Create a new zip archive
                using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    // Get a list of all files in the source path
                    string[] files = Directory.GetFiles(sourcePath);

                    foreach (string file in files)
                    {
                        // Add each file to the archive
                        string fileName = Path.GetFileName(file);
                        archive.CreateEntryFromFile(file, fileName);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"An error occurred while compressing files:\n{ex.Message}", color: Color.Orange);
                return false;
            }
        }

        public static bool ExtractZipFile(string zipPath, string extractPath)
        {
            try
            {
                // Create the extraction directory if it doesn't exist
                Directory.CreateDirectory(extractPath);

                // Extract the zip file
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryPath = Path.Combine(extractPath, entry.FullName);

                        // Ensure the entry's parent directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                        // Extract the entry
                        entry.ExtractToFile(entryPath, overwrite: true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"An error occurred while extracting files:\n{ex.Message}", color: Color.Orange);
                return false;
            }
        }
    }
}