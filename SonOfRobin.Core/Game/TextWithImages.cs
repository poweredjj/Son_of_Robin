using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SonOfRobin
{
    public class TextWithImages
    {
        public struct ImageInfo
        {
            public readonly Texture2D texture;
            public readonly int markerStart;
            public readonly int markerLength;
            public readonly Rectangle rectangle; // scaled at 100%, need to be adjusted for other scales

            public ImageInfo(Texture2D texture, int markerIndex, int markerLength, Rectangle rectangle)
            {
                this.texture = texture;
                this.markerStart = markerIndex;
                this.markerLength = markerLength;
                this.rectangle = rectangle;
            }

            public bool Equals(ImageInfo otherInfo)
            {
                return this.texture == otherInfo.texture &&
                       this.markerStart == otherInfo.markerStart &&
                       this.markerLength == otherInfo.markerLength;
            }

            public Rectangle GetAdjustedRectangle(float scale, Vector2 basePos, bool treatImagesAsSquares)
            {
                Rectangle scaledRect = new Rectangle(
                    x: (int)(this.rectangle.X * scale) + (int)basePos.X,
                    y: (int)(this.rectangle.Y * scale) + (int)basePos.Y,
                    width: (int)((treatImagesAsSquares ? this.rectangle.Height : this.rectangle.Width) * scale),
                    height: (int)(this.rectangle.Height * scale));

                return scaledRect;
            }
        }

        public static readonly string imageMarker = "|";

        public readonly SpriteFontBase font;

        public string TextOriginal
        { get { return this.textOriginal; } }

        private readonly string textOriginal;
        private readonly string textWithResizedMarkers;
        private readonly List<ImageInfo> imageInfoList;

        private List<Texture2D> ImageList
        { get { return this.imageInfoList.Select(info => info.texture).ToList(); } }

        private readonly bool treatImagesAsSquares;
        private readonly bool animate;
        private readonly Sound animSound;

        private readonly float minMarkerWidthMultiplier; // marker width = marker height * multiplier
        private readonly Helpers.AlignX imageAlignX;
        public readonly int textWidth;
        public readonly int textHeight;
        public readonly int noOfLines;

        private int charCounter;
        private int currentCharFramesLeft;
        public bool AnimationFinished { get; private set; }
        private readonly int framesPerChar;
        private readonly int charsPerFrame;

        public string Text
        { get { return this.textWithResizedMarkers; } }

        private string AnimatedText
        { get { return this.textWithResizedMarkers.Substring(0, this.charCounter); } }

        public TextWithImages(SpriteFontBase font, string text, List<Texture2D> imageList, bool animate = false, int framesPerChar = 0, int charsPerFrame = 1, Sound animSound = null, bool treatImagesAsSquares = false, float minMarkerWidthMultiplier = 0, Helpers.AlignX imageAlignX = Helpers.AlignX.Center)
        {
            if (imageList == null) imageList = new List<Texture2D>();

            this.font = font;
            this.textOriginal = text;
            this.treatImagesAsSquares = treatImagesAsSquares;
            this.minMarkerWidthMultiplier = minMarkerWidthMultiplier;
            this.imageAlignX = imageAlignX;

            this.animate = animate;
            this.animSound = animSound;
            this.ValidateImagesCount(imageList);
            var tuple = this.GetTextWithResizedMarkersAndImageInfo(imageList);
            this.textWithResizedMarkers = tuple.Item1;
            this.imageInfoList = tuple.Item2;

            Vector2 textSize = Helpers.MeasureStringCorrectly(font: this.font, stringToMeasure: this.textWithResizedMarkers);
            this.textWidth = (int)textSize.X;
            this.textHeight = (int)textSize.Y;
            this.noOfLines = this.textWithResizedMarkers.Split('\n').Length;

            this.charCounter = this.animate ? 0 : this.textWithResizedMarkers.Length;
            this.currentCharFramesLeft = framesPerChar;
            this.AnimationFinished = !this.animate;
            this.framesPerChar = framesPerChar;
            this.charsPerFrame = charsPerFrame;
        }

        public bool Equals(TextWithImages textToCompare)
        {
            bool firstCheck =
                this.font == textToCompare.font &&
                this.textWithResizedMarkers == textToCompare.textWithResizedMarkers &&
                this.imageInfoList.Count == textToCompare.imageInfoList.Count;

            if (!firstCheck) return false;

            for (int i = 0; i < this.imageInfoList.Count; i++)
            {
                if (!this.imageInfoList[i].Equals(textToCompare.imageInfoList[i])) return false;
            }

            return true;
        }

        public bool ImageListEqual(List<Texture2D> imageListToCompare)
        {
            if (this.imageInfoList.Count == 0 && imageListToCompare == null) return true;
            return this.ImageList == imageListToCompare;
        }

        public void FinishAnimationNow()
        {
            if (!this.animate) return;
            this.charCounter = this.textWithResizedMarkers.Length;
            this.AnimationFinished = true;
        }

        public void ResetAnim()
        {
            if (!this.animate) return;

            this.charCounter = 0;
            this.AnimationFinished = false;
        }

        private void ValidateImagesCount(List<Texture2D> imageList)
        {
            MatchCollection matches = Regex.Matches(this.textOriginal, $@"\{imageMarker}"); // $@ is needed for "\" character inside interpolated string

            if (imageList.Count != matches.Count) throw new ArgumentException($"TextWindow - count of markers ({matches.Count}) and images ({imageList.Count}) does not match.\n{this.textOriginal}");
        }

        private (string, List<ImageInfo>) GetTextWithResizedMarkersAndImageInfo(List<Texture2D> imageList)
        {
            if (imageList.Count == 0) return (this.textOriginal.ToString(), new List<ImageInfo>());

            int imageNo = 0;
            string newText = "";

            List<ImageInfo> newImageInfoList = new List<ImageInfo>();

            int resizedTextCharCounter = 0;
            for (int charCounter = 0; charCounter < this.TextOriginal.Length; charCounter++)
            {
                string newCharacter = this.textOriginal[charCounter].ToString();

                if (newCharacter == imageMarker)
                {
                    Texture2D image = imageList[imageNo];

                    newCharacter = GetResizedMarker(font: font, image: image, treatImagesAsSquares: this.treatImagesAsSquares, minMarkerLength: this.minMarkerWidthMultiplier);
                    newText += newCharacter; // has to go before GetImageMarkerRect()

                    int start = resizedTextCharCounter;
                    int length = newCharacter.Length;

                    Rectangle markerRect = GetImageMarkerRect(font: this.font, text: newText, start: start, length: length);
                    newImageInfoList.Add(new ImageInfo(texture: image, markerIndex: start, markerLength: length, rectangle: markerRect));

                    resizedTextCharCounter += length;
                    imageNo++;
                }
                else
                {
                    resizedTextCharCounter++;
                    newText += newCharacter;
                }
            }

            return (newText, newImageInfoList);
        }

        private static string GetResizedMarker(SpriteFontBase font, Texture2D image, bool treatImagesAsSquares, float minMarkerLength = 0)
        {
            float imageScale = (float)image.Height / Helpers.MeasureStringCorrectly(font: font, stringToMeasure: " ").Y;
            int targetWidth = (int)((float)(treatImagesAsSquares ? image.Height : image.Width) / imageScale);
            if (minMarkerLength != 0) targetWidth = (int)((float)image.Height / imageScale * minMarkerLength);

            int markerCharCount = 1;
            int lastCharCount = 1;
            int lastDelta = 99999;

            while (true)
            {
                string resizedMarker = string.Concat(Enumerable.Repeat(" ", markerCharCount));
                int delta = (int)Math.Abs(targetWidth - Helpers.MeasureStringCorrectly(font: font, stringToMeasure: resizedMarker).X);
                if (delta > lastDelta) break;
                else
                {
                    lastDelta = delta;
                    lastCharCount = markerCharCount;
                    markerCharCount++;
                }
            }

            return string.Concat(Enumerable.Repeat(" ", lastCharCount));
        }

        private static Rectangle GetImageMarkerRect(SpriteFontBase font, string text, int start, int length)
        {
            string textBeforeMarker = text.Substring(0, start);
            string fullMarker = text.Substring(start, length);

            int lastNewLineIndex = textBeforeMarker.LastIndexOf('\n');
            if (lastNewLineIndex == -1) lastNewLineIndex = 0; // if no newline was found, LastIndexOf() == -1
            else lastNewLineIndex += 1; // deleting "\n" from the start

            lastNewLineIndex = Math.Max(lastNewLineIndex, 0);

            string thisLineText = textBeforeMarker.Substring(lastNewLineIndex, textBeforeMarker.Length - lastNewLineIndex);
            string textBeforeThisLine = textBeforeMarker.Substring(0, Math.Max(lastNewLineIndex - 1, 0));

            int posX = (int)Helpers.MeasureStringCorrectly(font: font, stringToMeasure: thisLineText).X;
            int posY = (int)Helpers.MeasureStringCorrectly(font: font, stringToMeasure: textBeforeThisLine).Y;

            Vector2 rectSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: fullMarker);

            return new Rectangle(x: posX, y: posY, width: (int)rectSize.X, height: (int)rectSize.Y);
        }

        public void Update()
        {
            this.Animate();
        }

        private void Animate()
        {
            if (this.AnimationFinished) return;

            this.currentCharFramesLeft--;
            if (this.currentCharFramesLeft <= 0)
            {
                if (this.animSound != null && Sound.textWindowAnimOn) this.animSound.Play();
                this.charCounter = Math.Min(this.charCounter + this.charsPerFrame, this.textWithResizedMarkers.Length);
                this.currentCharFramesLeft = framesPerChar;

                if (this.charCounter == this.textWithResizedMarkers.Length)
                {
                    this.AnimationFinished = true;
                    return;
                }
            }
        }

        public void Draw(Vector2 position, Color color, bool drawShadow = false, float textScale = 1f, float imageOpacity = 1f, float inflatePercent = 0f, Vector2 shadowOffset = default, Color shadowColor = default)
        {
            if (shadowOffset == default) shadowOffset = new Vector2(Math.Max(SonOfRobinGame.ScreenHeight * 0.002f, 1));

            string currentText = this.AnimatedText;

            Vector2 textScaleVector = new Vector2(textScale);

            if (drawShadow && shadowColor.A > 0)
            {
                this.font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: currentText, position: position + shadowOffset, color: shadowColor, scale: textScaleVector, effect: FontSystemEffect.Blurry, effectAmount: 3);
            }

            this.font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: currentText, position: position, color: color, scale: textScaleVector);

            int imageNo = 0;
            foreach (ImageInfo imageInfo in this.imageInfoList)
            {
                if (imageInfo.markerStart > this.charCounter) break;
                else
                {
                    Rectangle imageRect = imageInfo.GetAdjustedRectangle(scale: textScale, basePos: position, treatImagesAsSquares: this.treatImagesAsSquares);

                    if (inflatePercent > 0) imageRect.Inflate(imageRect.Width * inflatePercent, imageRect.Height * inflatePercent);

                    if (drawShadow)
                    {
                        Rectangle imageShadowRect = new Rectangle(x: imageRect.X + ((int)shadowOffset.X * 2), y: imageRect.Y + ((int)shadowOffset.Y * 2), width: imageRect.Width, height: imageRect.Height);
                        Helpers.DrawTextureInsideRect(texture: imageInfo.texture, rectangle: imageShadowRect, color: shadowColor, drawTestRect: false, alignX: this.imageAlignX);
                    }

                    Helpers.DrawTextureInsideRect(texture: imageInfo.texture, rectangle: imageRect, color: Color.White * imageOpacity, drawTestRect: false, alignX: this.imageAlignX);
                }
                imageNo++;
            }
        }
    }
}