using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class Helpers
    {
        public const double Rad2Deg = 180.0 / Math.PI;
        public const double Deg2Rad = Math.PI / 180.0;

        public enum AlignX : byte
        {
            Left,
            Center,
            Right,
        };

        public enum AlignY : byte
        {
            Top,
            Center,
            Bottom,
        };

        private static int currentID = (int)(DateTime.Now.Ticks & 0x7FFFFFFF);

        public static int GetUniqueID()
        {
            // doesn't work in parallel
            currentID++;
            return currentID;
        }

        public static void DrawTextInsideRectWithShadow(SpriteFontBase font, Rectangle rectangle, string text, Color color, Color shadowColor, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, int shadowOffsetX = 0, int shadowOffsetY = 0, int shadowOffset = 0, bool drawTestRect = false, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
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

                DrawTextInsideRect(font: font, rectangle: shadowRect, text: text, color: shadowColor, alignX: alignX, alignY: alignY, drawTestRect: drawTestRect, effect: effect, effectAmount: effectAmount);
            }

            DrawTextInsideRect(font: font, rectangle: rectangle, text: text, color: color, alignX: alignX, alignY: alignY, drawTestRect: drawTestRect, effect: effect, effectAmount: effectAmount);
        }

        public static void DrawTextInsideRect(SpriteFontBase font, Rectangle rectangle, string text, Color color, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, bool drawTestRect = false, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0, int drawTimes = 1)
        {
            Vector2 textSize = MeasureStringCorrectly(font: font, stringToMeasure: text);
            float scale = Math.Min(rectangle.Width / textSize.X, rectangle.Height / textSize.Y);
            scale = Math.Max(scale, 0.01f); // to avoid division by zero

            if (drawTestRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: rectangle, color: Color.White, thickness: 1f);

            float xOffset = alignX switch
            {
                AlignX.Left => 0,
                AlignX.Center => (rectangle.Width - (textSize.X * scale)) / 2,
                AlignX.Right => rectangle.Width - (textSize.X * scale),
                _ => throw new ArgumentException($"Unsupported alignX - {alignX}."),
            };

            float yOffset = alignY switch
            {
                AlignY.Top => 0,
                AlignY.Center => (rectangle.Height - (textSize.Y * scale)) / 2,
                AlignY.Bottom => rectangle.Height - (textSize.Y * scale),
                _ => throw new ArgumentException($"Unsupported alignY - {alignY}."),
            };

            for (int i = 0; i < drawTimes; i++)
            {
                font.DrawText(
                    batch: SonOfRobinGame.SpriteBatch,
                    text: text,
                    position: new Vector2(rectangle.X + xOffset, rectangle.Y + yOffset),
                    color: color,
                    scale: new Vector2(scale),
                    effect: effectAmount == 0 ? FontSystemEffect.None : effect,
                    effectAmount: (int)Math.Ceiling(effectAmount / scale));
            }
        }

        public static Rectangle DrawTextureInsideRect(Texture2D texture, Rectangle rectangle, Color color, AlignX alignX = AlignX.Center, AlignY alignY = AlignY.Center, bool drawTestRect = false, float rotation = 0, Rectangle srcRect = default, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            if (srcRect == default) srcRect = texture.Bounds;

            int srcWidth = srcRect.Width;
            int srcHeight = srcRect.Height;

            float scale = Math.Min((float)rectangle.Width / (float)srcWidth, (float)rectangle.Height / (float)srcHeight);

            if (rotation != 0)
            {
                // rotated textured must fit inside the rect too
                float rotatedScale = Math.Min((float)rectangle.Width / (float)srcHeight, (float)rectangle.Height / (float)srcWidth);
                if (rotatedScale < scale) scale = rotatedScale;
            }

            Vector2 scaledTexture = new(srcWidth * scale, srcHeight * scale);

            if (drawTestRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: rectangle, color: Color.White, thickness: 1f);

            var xOffset = alignX switch
            {
                AlignX.Left => 0,
                AlignX.Center => (int)((rectangle.Width - scaledTexture.X) / 2),
                AlignX.Right => (int)(rectangle.Width - scaledTexture.X),
                _ => throw new ArgumentException($"Unsupported alignX - {alignX}."),
            };

            var yOffset = alignY switch
            {
                AlignY.Top => 0,
                AlignY.Center => (int)((rectangle.Height - scaledTexture.Y) / 2),
                AlignY.Bottom => (int)(rectangle.Height - scaledTexture.Y),
                _ => throw new ArgumentException($"Unsupported alignY - {alignY}."),
            };

            Rectangle destRect = new(x: rectangle.X + xOffset, y: rectangle.Y + yOffset, width: (int)(srcWidth * scale), height: (int)(srcHeight * scale));
            if (drawTestRect) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: destRect, color: Color.Green, thickness: 1f);

            if (rotation == 0) SonOfRobinGame.SpriteBatch.Draw(texture: texture, sourceRectangle: srcRect, destinationRectangle: destRect, color: color, rotation: 0f, origin: Vector2.Zero, effects: spriteEffects, layerDepth: 0f);
            else
            {
                destRect.Offset(destRect.Width / 2, destRect.Height / 2);

                SonOfRobinGame.SpriteBatch.Draw(texture: texture, sourceRectangle: srcRect, origin: new Vector2(srcWidth * 0.5f, srcHeight * 0.5f), destinationRectangle: destRect, color: color, rotation: rotation, effects: spriteEffects, layerDepth: 0);
            }

            return destRect;
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
            return VectorKeepBelowSetValue(vector: vector, maxValX: maxVal, maxValY: maxVal);
        }

        public static Vector2 VectorKeepBelowSetValue(Vector2 vector, float maxValX, float maxValY)
        {
            bool isXNegative = vector.X < 0;
            bool isYNegative = vector.Y < 0;

            float newXComponent = Math.Min(Math.Abs(vector.X), maxValX);
            float newYComponent = Math.Min(Math.Abs(vector.Y), maxValY);

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

        public static double ConvertRange(double oldMin, double oldMax, double newMin, double newMax, double oldVal, bool clampToEdges, bool reverseVal = false)
        {
            double newVal = ((oldVal - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;
            if (clampToEdges) newVal = Math.Clamp(value: newVal, min: newMin, max: newMax);
            return reverseVal ? newMax - newVal : newVal;
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
                MessageLog.Add(text: $"An error occurred while compressing files:\n{ex.Message}", textColor: Color.Orange);
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
                MessageLog.Add(text: $"An error occurred while extracting files:\n{ex.Message}", textColor: Color.Orange);
                return false;
            }
        }

        public static string ConvertTimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1)) timeLeftString = timeSpan.ToString("ss");
            else if (timeSpan < TimeSpan.FromHours(1)) timeLeftString = timeSpan.ToString("mm\\:ss");
            else if (timeSpan < TimeSpan.FromDays(1)) timeLeftString = timeSpan.ToString("hh\\:mm\\:ss");
            else timeLeftString = timeSpan.ToString("dd\\:hh\\:mm\\:ss");

            return timeLeftString;
        }

        public static List<List<Point>> SlicePointBagIntoConnectedRegions(int width, int height, ConcurrentBag<Point> pointsBag)
        {
            // preparing data

            // List of offset points representing neighboring positions (up, down, left, right)
            Point[] offsetList = new Point[]
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1),
            };

            // Create a HashSet for faster point existence checks
            var pointsSet = new HashSet<Point>(pointsBag);
            pointsBag.Clear(); // saving memory

            // Create a 2D array to track processed points
            bool[,] bitmapPointIsProcessed = new bool[width, height];

            // Initialize variables for tracking regions and points
            int currentRegion = 0;
            var pointsByRegion = new List<List<Point>>();

            // filling regions

            // Process each point only once
            foreach (Point start in pointsSet)
            {
                if (bitmapPointIsProcessed[start.X, start.Y]) continue;

                var thisRegionPoints = new List<Point>();
                var stack = new Stack<Point>();
                stack.Push(start);

                // Explore the region connected to the starting point using Depth-First Search (DFS)
                while (stack.Count > 0)
                {
                    Point currentPoint = stack.Pop();

                    if (currentPoint.X < 0 || currentPoint.X >= width ||
                        currentPoint.Y < 0 || currentPoint.Y >= height ||
                        bitmapPointIsProcessed[currentPoint.X, currentPoint.Y])
                        continue;

                    thisRegionPoints.Add(currentPoint);
                    bitmapPointIsProcessed[currentPoint.X, currentPoint.Y] = true;

                    foreach (Point offset in offsetList)
                    {
                        Point nextPoint = new(currentPoint.X + offset.X, currentPoint.Y + offset.Y);
                        if (pointsSet.Contains(nextPoint)) stack.Push(nextPoint);
                    }
                }

                pointsByRegion.Add(thisRegionPoints);
                currentRegion++;
            }

            return pointsByRegion;
        }

        public static Vector2 MeasureStringCorrectly(SpriteFontBase font, string stringToMeasure)
        {
            // measures string the same way SpriteFont measures (ignoring the area below baseline)

            if (stringToMeasure.Length == 0) return Vector2.Zero;

            Vector2 stringSize = font.MeasureString(stringToMeasure);
            stringSize.Y = font.LineHeight * stringToMeasure.Split("\n").Length;

            return stringSize;
        }

        public static string EncodeTimeSpanAsHash(TimeSpan timeSpan)
        {
            return ComputeSHA256Hash($"{timeSpan}_this_text_is_for_making_the_string_more_unique");
        }

        public static string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string KeepTextLineBelowGivenLength(string text, int maxLength)
        {
            string newText = "";

            string[] lineList = text.Split(Environment.NewLine.ToCharArray());

            int lineNo, wordNo, lineWidth;

            lineNo = 0;
            foreach (string line in lineList)
            {
                if (lineNo > 0) newText += "\n";
                lineWidth = 0;

                string[] wordList = line.Split(' ');

                wordNo = 0;
                foreach (string word in wordList)
                {
                    if (wordNo > 0) newText += " ";

                    if (lineWidth + word.Length > maxLength)
                    {
                        newText += "\n";
                        lineWidth = 0;
                    }

                    newText += word;
                    lineWidth += word.Length;
                    wordNo++;
                }

                lineNo++;
            }

            return newText;
        }

        public static List<TextWithImages> MakeCreditsTextList()
        {
            SpriteFontBase fontTitle = SonOfRobinGame.FontTommy.GetFont(RollingText.TitleFontSize);
            SpriteFontBase fontText = SonOfRobinGame.FontTommy.GetFont(RollingText.RegularFontSize);

            var textList = new List<TextWithImages>();

            textList.Add(new TextWithImages(font: fontTitle, text: "|  Son of Robin\n", imageList: new List<ImageObj> { TextureBank.GetImageObj(TextureBank.TextureName.LoadingGfx) }));

            textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<ImageObj>()));

            textList.Add(new TextWithImages(font: fontText, text: "Developed with  |  MonoGame framework", imageList: new List<ImageObj> { TextureBank.GetImageObj(TextureBank.TextureName.MonoGame) }));
            textList.Add(new TextWithImages(font: fontText, text: "Sounds:  |  freesound.org", imageList: new List<ImageObj> { PieceInfo.GetImageObj(PieceTemplate.Name.MusicNote) }));
            textList.Add(new TextWithImages(font: fontText, text: "Text rendering: FontStashSharp library", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Controller icons:  |  Nicolae (Xelu) Berbece", imageList: new List<ImageObj> { new TextureObj(ButtonScheme.dpad) }));
            textList.Add(new TextWithImages(font: fontText, text: "FastNoiseLite library: Jordan Peck", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Studies.Joystick library: Luiz Ossinho", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "ImageSharp library: Six Labors", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Earcut polygon triangulation library: Oberbichler + Mapbox", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Free graphics assets: RPGMaker forums, opengameart.org, other sites", imageList: new List<ImageObj>()));

            textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Flowers by FSCM Productions https://fscmproductions.bandcamp.com", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Music promoted by https://www.free-stock-music.com", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Creative Commons / Attribution 4.0 International (CC BY 4.0)", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "https://creativecommons.org/licenses/by/4.0/", imageList: new List<ImageObj>()));

            textList.Add(new TextWithImages(font: fontTitle, text: " ", imageList: new List<ImageObj>()));


            textList.Add(new TextWithImages(font: fontTitle, text: "|  Testers  |", imageList: new List<ImageObj> { new TextureObj(ButtonScheme.dpad), new TextureObj(ButtonScheme.dpad) }));

            textList.Add(new TextWithImages(font: fontText, text: "Faye", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Hellwoman", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Glonfindel", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "Kaeru  |", imageList: new List<ImageObj> { PieceInfo.GetImageObj(PieceTemplate.Name.Frog) }));
            textList.Add(new TextWithImages(font: fontText, text: "Retro Marek", imageList: new List<ImageObj>()));

            textList.Add(new TextWithImages(font: fontText, text: " ", imageList: new List<ImageObj>()));
            textList.Add(new TextWithImages(font: fontText, text: "developed by Ahoy Games 2021 - 2024", imageList: new List<ImageObj>()));

            return textList;
        }
    }
}