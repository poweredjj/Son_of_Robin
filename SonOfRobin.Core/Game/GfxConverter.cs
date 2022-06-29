using BigGustave;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class GfxConverter
    {

        public static void SaveColorArrayAsPNG(int width, int height, Color[,] colorArray, string pngPath, bool hasAlphaChannel = true)
        {
            // TODO test if it works correctly

            var builder = PngBuilder.Create(width: width, height: height, hasAlphaChannel: hasAlphaChannel);

            Color pixel;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixel = colorArray[x, y];

                    builder.SetPixel(pixel.R, pixel.G, pixel.B, x, y);
                }
            }

            using (var memoryStream = new MemoryStream())
            {
                builder.Save(memoryStream);
                FileReaderWriter.SaveMemoryStream(memoryStream: memoryStream, path: pngPath);
            }
        }

        public static Texture2D CropTexture(Texture2D baseTexture, Rectangle cropRect)
        {
            // Copy the data from the cropped region into a buffer, then into the new texture
            Color[] data = new Color[cropRect.Width * cropRect.Height];
            baseTexture.GetData(0, cropRect, data, 0, cropRect.Width * cropRect.Height);

            Texture2D croppedTexture = new Texture2D(SonOfRobinGame.graphicsDevice, cropRect.Width, cropRect.Height);
            croppedTexture.SetData(data);

            return croppedTexture;
        }

        public static Texture2D CropTextureAndAddPadding(Texture2D baseTexture, Rectangle cropRect, int padding)
        {
            Color[] colorData = new Color[cropRect.Width * cropRect.Height];
            baseTexture.GetData(0, cropRect, colorData, 0, cropRect.Width * cropRect.Height);

            int paddedWidth = cropRect.Width + (padding * 2);
            int paddedHeight = cropRect.Height + (padding * 2);

            Color[,] colorData2DPadded = new Color[paddedWidth, paddedHeight];

            // filling the padding

            List<int> rowsToFill = new List<int> { };
            List<int> columnsToFill = new List<int> { };
            for (int i = 0; i < padding; i++)
            {
                columnsToFill.Add(i);
                columnsToFill.Add(paddedWidth - i - 1);
                rowsToFill.Add(i);
                rowsToFill.Add(paddedHeight - i - 1);
            }

            foreach (int x in columnsToFill)
            {
                for (int y = 0; y < paddedHeight; y++)
                {
                    colorData2DPadded[x, y] = Color.Transparent;
                }
            }

            foreach (int y in rowsToFill)
            {
                for (int x = 0; x < paddedWidth; x++)
                {
                    colorData2DPadded[x, y] = Color.Transparent;
                }
            }

            // copying pixels from original texture data

            for (int x = 0; x < cropRect.Width; x++)
            {
                for (int y = 0; y < cropRect.Height; y++)
                {
                    colorData2DPadded[x + padding, y + padding] = colorData[(y * cropRect.Width) + x];
                }

            }

            return Convert2DArrayToTexture(colorData2DPadded);
        }

        public static Color[,] ConvertTextureToGrid(Texture2D texture, int x, int y, int width, int height)
        {
            // getting 1D pixel array
            Color[] rawData = new Color[width * height];
            Rectangle extractRegion = new Rectangle(x, y, width, height);
            texture.GetData<Color>(0, extractRegion, rawData, 0, width * height);

            // getting 2D pixel grid
            Color[,] rawDataAsGrid = new Color[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    // Assumes row major ordering of the array.
                    rawDataAsGrid[row, column] = rawData[row * width + column];
                }
            }
            return rawDataAsGrid;
        }


        public static void SaveTextureAsPNG(string filename, Texture2D texture)
        {
            try
            {
                Stream stream = File.Create(filename);
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                stream.Dispose();
            }
            catch (IOException)
            { }
        }

        public static Texture2D LoadTextureFromPNG(string filename)
        {
            try
            {
                FileStream fileStream = new FileStream(filename, FileMode.Open);
                Texture2D loadedTexture = Texture2D.FromStream(SonOfRobinGame.graphicsDevice, fileStream);
                fileStream.Dispose();
                return loadedTexture;
            }
            catch (FileNotFoundException)
            { }
            catch (InvalidOperationException) // png file corrupted
            { }

            return null;
        }

        public static Texture2D ConvertColorArray2DToTexture(Color[,] array2D)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);

            var array1D = ConvertArray2DTo1D(width: width, height: height, array2D: array2D);
            var texture = ConvertColorArray1DToTexture(array1D: array1D, width: width, height: height);
            return texture;
        }

        public static Texture2D ConvertColorArray1DToTexture(Color[] array1D, int width, int height)
        {
            var texture = new Texture2D(SonOfRobinGame.graphicsDevice, width, height);
            texture.SetData(array1D);
            return texture;
        }

        public static Texture2D Convert2DGreyscaleArrayToTexture(byte[,] inputArray)
        {
            int width = inputArray.GetLength(0);
            int height = inputArray.GetLength(1);

            Color[,] graphics = new Color[width, height];

            var allY = Enumerable.Range(0, height).ToList();

            Parallel.ForEach(allY, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    byte brightness = inputArray[x, y];
                    graphics[x, y] = new Color(brightness, brightness, brightness, (byte)255);
                }
            });

            var texture = new Texture2D(graphicsDevice: SonOfRobinGame.graphicsDevice, width: width, height: height);
            var array1D = ConvertArray2DTo1D(width: width, height: height, array2D: graphics);
            texture.SetData(array1D);

            return texture;
        }

        public static Texture2D Convert2DArrayToTexture(Color[,] inputArray)
        {
            int width = inputArray.GetLength(0);
            int height = inputArray.GetLength(1);

            Color[,] graphics = new Color[width, height];

            var allY = Enumerable.Range(0, height).ToList();

            Parallel.ForEach(allY, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    graphics[x, y] = inputArray[x, y];
                }
            });

            var texture = new Texture2D(graphicsDevice: SonOfRobinGame.graphicsDevice, width: width, height: height);
            var array1D = ConvertArray2DTo1D(width: width, height: height, array2D: graphics);
            texture.SetData(array1D);

            return texture;
        }

        public static Color[,] ConvertArray1DTo2D(int width, int height, Color[] array1D)
        {
            Color[,] array2D = new Color[width, height];

            var allY = Enumerable.Range(0, height).ToList();

            // correct order for Texture2D is y,x
            Parallel.ForEach(allY, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                { array2D[x, y] = array1D[(y * width) + x]; }
            });

            return array2D;
        }
        public static Color[] ConvertArray2DTo1D(int width, int height, Color[,] array2D)
        {
            Color[] array1D = new Color[width * height];

            var allY = Enumerable.Range(0, height).ToList();

            // correct order for Texture2D is y,x
            Parallel.ForEach(allY, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                { array1D[(y * width) + x] = array2D[x, y]; }
            });

            return array1D;
        }


        public static Color[,] ConvertTextureToColorArray(Texture2D texture, int width, int height)
        {
            var array1D = new Color[width * height];
            texture.GetData(array1D);
            var array2D = ConvertArray1DTo2D(array1D: array1D, width: width, height: height);

            return array2D;
        }

        public static Byte[,] ConvertColorArrayToByteArray(Color[,] colorArray)
        {
            // uses green value of color array

            int width = colorArray.GetLength(0);
            int height = colorArray.GetLength(1);

            var byteArray = new byte[width, height];

            var allY = Enumerable.Range(0, height).ToList();

            Parallel.ForEach(allY, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                { byteArray[x, y] = colorArray[x, y].G; }
            });

            return byteArray;
        }

    }
}
