using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace SonOfRobin
{
    public class GfxConverter
    {
        public static BitArray LoadPNGAsBitArray(string path)
        {
            Texture2D texture = LoadTextureFromPNG(path);

            if (texture == null) return null;

            int width = texture.Width;
            int height = texture.Height;

            BitArray bitArray = new BitArray(width * height);

            var colorArray1D = new Color[width * height];
            texture.GetData(colorArray1D);

            for (int y = 0; y < height; y++)
            {
                int yFactor = y * width;

                for (int x = 0; x < width; x++)
                {
                    Color pixel = colorArray1D[yFactor + x];
                    bitArray[yFactor + x] = pixel == Color.Black;
                }
            }

            return bitArray;
        }

        public static void Save2DByteArrayToPNG(Byte[,] array2D, string path)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);

            using (var image = new Image<L8>(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte arrayVal = array2D[x, y];
                        image[x, y] = new L8(arrayVal);
                    }
                }

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    image.Save(fileStream, new PngEncoder());
                }
            }
        }

        public static byte[,] LoadPNGAs2DByteArray(string path)
        {
            Texture2D texture = LoadTextureFromPNG(path);

            if (texture == null) return null;

            int width = texture.Width;
            int height = texture.Height;

            var colorArray1D = new Color[width * height];
            texture.GetData(colorArray1D);

            Byte[,] array2D = new byte[width, height];

            for (int y = 0; y < height; y++)
            {
                int yFactor = y * width;

                for (int x = 0; x < width; x++)
                {
                    Color pixel = colorArray1D[yFactor + x];
                    array2D[x, y] = pixel.R;
                }
            }

            return array2D;
        }

        public static Texture2D CropTexture(Texture2D baseTexture, Rectangle cropRect)
        {
            // Copy the data from the cropped region into a buffer, then into the new texture
            Color[] data = new Color[cropRect.Width * cropRect.Height];
            baseTexture.GetData(0, cropRect, data, 0, cropRect.Width * cropRect.Height);

            Texture2D croppedTexture = new Texture2D(SonOfRobinGame.GfxDev, cropRect.Width, cropRect.Height);
            croppedTexture.SetData(data);

            return croppedTexture;
        }

        public static Texture2D CropTextureAndAddPadding(Texture2D baseTexture, Rectangle cropRect, int padding)
        {
            Color[] colorData = new Color[cropRect.Width * cropRect.Height];
            baseTexture.GetData(0, cropRect, colorData, 0, cropRect.Width * cropRect.Height);

            int paddedWidth = cropRect.Width + (padding * 2);
            int paddedHeight = cropRect.Height + (padding * 2);

            Color[] paddedArray1D = new Color[paddedWidth * paddedHeight]; // filling the padding is not needed - default array color is transparent

            // copying pixels from original texture data
            for (int y = 0; y < cropRect.Height; y++)
            {
                int yMultipliedOutput = y * paddedWidth;
                int yMultipliedInput = y * cropRect.Width;

                for (int x = 0; x < cropRect.Width; x++)
                {
                    paddedArray1D[yMultipliedOutput + x] = colorData[yMultipliedInput + x];
                }
            }

            var texture = new Texture2D(graphicsDevice: SonOfRobinGame.GfxDev, width: paddedWidth, height: paddedHeight);
            texture.SetData(paddedArray1D);

            return texture;
        }

        public static Color[,] ConvertTextureToGrid(Texture2D texture, int x, int y, int width, int height)
        {
            // getting 1D pixel array
            Color[] rawData = new Color[width * height];
            Rectangle extractRegion = new Rectangle(x, y, width, height);
            texture.GetData<Color>(0, extractRegion, rawData, 0, width * height);

            // getting 2D pixel grid
            Color[,] rawDataAsGrid = new Color[width, height];
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    // Assumes row major ordering of the array.
                    rawDataAsGrid[column, row] = rawData[row * width + column];
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

        public static Texture2D LoadTextureFromPNG(string path)
        {
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Open);
                Texture2D loadedTexture = Texture2D.FromStream(SonOfRobinGame.GfxDev, fileStream);
                fileStream.Dispose();
                return loadedTexture;
            }
            catch (FileNotFoundException)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"FileNotFoundException while trying to read {Path.GetFileName(path)}.");
            }
            catch (IOException) // png file corrupted
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"IOException while trying to read {Path.GetFileName(path)}.");
            }
            catch (InvalidOperationException) // png file corrupted
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"InvalidOperationException while trying to read {Path.GetFileName(path)}.");
            }

            return null;
        }

        public static Texture2D ConvertColorArray2DToTexture(Color[,] array2D)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);

            var array1D = ConvertColorArray2DTo1D(width: width, height: height, array2D: array2D);
            var texture = ConvertColorArray1DToTexture(array1D: array1D, width: width, height: height);
            return texture;
        }

        public static Texture2D ConvertColorArray1DToTexture(Color[] array1D, int width, int height)
        {
            var texture = new Texture2D(SonOfRobinGame.GfxDev, width, height);
            texture.SetData(array1D);
            return texture;
        }

        public static Texture2D Convert2DGreyscaleArrayToTexture(byte[,] inputArray)
        {
            int width = inputArray.GetLength(0);
            int height = inputArray.GetLength(1);

            Color[,] graphics = new Color[width, height];

            Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    byte brightness = inputArray[x, y];
                    graphics[x, y] = new Color(brightness, brightness, brightness, (byte)255);
                }
            });

            var texture = new Texture2D(graphicsDevice: SonOfRobinGame.GfxDev, width: width, height: height);
            var array1D = ConvertColorArray2DTo1D(width: width, height: height, array2D: graphics);
            texture.SetData(array1D);

            return texture;
        }

        public static Texture2D Convert2DColorArrayToTexture(Color[,] inputArray)
        {
            int width = inputArray.GetLength(0);
            int height = inputArray.GetLength(1);

            Color[,] graphics = new Color[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    graphics[x, y] = inputArray[x, y];
                }
            }

            var texture = new Texture2D(graphicsDevice: SonOfRobinGame.GfxDev, width: width, height: height);
            var array1D = ConvertColorArray2DTo1D(width: width, height: height, array2D: graphics);
            texture.SetData(array1D);

            return texture;
        }

        public static Color[,] ConvertColorArray1DTo2D(int width, int height, Color[] array1D)
        {
            Color[,] array2D = new Color[width, height];

            // correct order for Texture2D is y,x
            Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    array2D[x, y] = array1D[(y * width) + x];
                }
            });

            return array2D;
        }

        public static Color[] ConvertColorArray2DTo1D(int width, int height, Color[,] array2D)
        {
            Color[] array1D = new Color[width * height];

            // correct order for Texture2D is y,x

            Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
                        {
                            for (int x = 0; x < width; x++)
                            { array1D[(y * width) + x] = array2D[x, y]; }
                        });

            return array1D;
        }

        public static Color[,] ConvertTextureToColorArray(Texture2D texture)
        {
            int width = texture.Width;
            int height = texture.Height;

            var array1D = new Color[width * height];
            texture.GetData(array1D);
            var array2D = ConvertColorArray1DTo2D(array1D: array1D, width: width, height: height);

            return array2D;
        }

        public static Byte[,] ConvertColorArrayToByteArray(Color[,] colorArray)
        {
            // uses green value of color array

            int width = colorArray.GetLength(0);
            int height = colorArray.GetLength(1);

            var byteArray = new byte[width, height];

            Parallel.For(0, height, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
                        {
                            for (int x = 0; x < width; x++)
                            { byteArray[x, y] = colorArray[x, y].G; }
                        });

            return byteArray;
        }

        public static byte[,] ConvertByteArray1DTo2D(int width, int height, byte[] array1D)
        {
            byte[,] array2D = new byte[width, height];

            for (int y = 0; y < height; y++)
            {
                int yFactor = y * width;
                for (int x = 0; x < width; x++)
                {
                    array2D[x, y] = array1D[(yFactor) + x];
                }
            }

            return array2D;
        }

        public static byte[] ConvertByteArray2DTo1D(byte[,] array2D)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);

            byte[] array1D = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                int yFactor = y * width;

                for (int x = 0; x < width; x++)
                {
                    array1D[yFactor + x] = array2D[x, y];
                }
            }

            return array1D;
        }
    }
}