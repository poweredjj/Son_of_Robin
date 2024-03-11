using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SonOfRobin
{
    public class GfxConverter
    {
        public static void Save2DByteArrayGreyscaleToPNGSquareFlipped(Byte[,] array2D, string path)
        {
            // use only with square bitmaps, to be read by counterpart method

            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);
            if (width != height) throw new ArgumentException($"Width {width} does not match height {height}.");

            var array1D = new byte[width * height];

            // copies 2D array to 1D array the wrong way, has to be cancelled out by Buffer.BlockCopy when loading
            Buffer.BlockCopy(array2D, 0, array1D, 0, array2D.Length * sizeof(byte));

            using (var image = Image.LoadPixelData<L8>(array1D, width, height))
            {
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        image.Save(fileStream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level6 });
                    }
                }
                catch (DirectoryNotFoundException)
                { }
            }
        }

        public static byte[,] LoadGreyscalePNGAs2DByteArraySquareFlipped(string path)
        {
            // use only with square bitmaps, saved with counterpart method

            try
            {
                using (var image = Image.Load<L8>(path))
                {
                    int width = image.Width;
                    int height = image.Height;
                    if (width != height) throw new ArgumentException($"Width {width} does not match height {height}.");

                    var array2D = new byte[width, height];

                    var _IMemoryGroup = image.GetPixelMemoryGroup();
                    int currentMemoryPos = 0;
                    foreach (var group in _IMemoryGroup)
                    {
                        var array1D = MemoryMarshal.AsBytes(group.Span).ToArray();

                        // copies 1D array to 2D array the wrong way, but cancels out Buffer.BlockCopy used while saving
                        Buffer.BlockCopy(array1D, 0, array2D, currentMemoryPos, array1D.Length * sizeof(byte));

                        currentMemoryPos += group.Length;
                    }

                    return array2D;
                }
            }
            catch (FileNotFoundException) { return null; }
            catch (UnknownImageFormatException) { return null; } // file corrupted
            //catch (ArgumentException) { return null; } // file corrupted
        }

        public static void Save2DByteArrayGreyscaleToPNG(Byte[,] array2D, string path)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);

            var array1D = new byte[width * height];

            for (int i = 0; i < array1D.Length; i++)
            {
                array1D[i] = array2D[i % width, i / width];
            }

            using (var image = Image.LoadPixelData<L8>(array1D, width, height))
            {
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        image.Save(fileStream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level6 });
                    }
                }
                catch (DirectoryNotFoundException)
                { }
            }
        }

        public static byte[,] LoadGreyscalePNGAs2DByteArray(string path)
        {
            try
            {
                using (var image = Image.Load<L8>(path))
                {
                    int width = image.Width;
                    int height = image.Height;

                    var array2D = new byte[width, height];

                    var _IMemoryGroup = image.GetPixelMemoryGroup();
                    int currentMemoryPos = 0;
                    foreach (var group in _IMemoryGroup)
                    {
                        var array1D = MemoryMarshal.AsBytes(group.Span).ToArray();

                        for (int i = 0; i < group.Length; i++)
                        {
                            array2D[(currentMemoryPos + i) % width, (currentMemoryPos + i) / width] = array1D[i];
                        }
                        currentMemoryPos += group.Length;
                    }

                    return array2D;
                }
            }
            catch (FileNotFoundException) { return null; }
            catch (UnknownImageFormatException) { return null; } // file corrupted
            catch (DirectoryNotFoundException) { return null; }
        }

        public static byte[] LoadPNGAs1DByteArray(string path)
        {
            // untested

            try
            {
                using (var image = Image.Load<L8>(path))
                {
                    int width = image.Width;
                    int height = image.Height;

                    var array1D = new byte[width * height];

                    var _IMemoryGroup = image.GetPixelMemoryGroup();
                    int currentMemoryPos = 0;
                    foreach (var group in _IMemoryGroup)
                    {
                        var bufferArray1D = MemoryMarshal.AsBytes(group.Span).ToArray();
                        Buffer.BlockCopy(array1D, 0, bufferArray1D, currentMemoryPos, array1D.Length * sizeof(byte));

                        currentMemoryPos += group.Length;
                    }

                    return array1D;
                }
            }
            catch (FileNotFoundException) { return null; }
            catch (UnknownImageFormatException) { return null; } // file corrupted
        }

        public static Texture2D CropTexture(Texture2D baseTexture, Rectangle cropRect)
        {
            // Copy the data from the cropped region into a buffer, then into the new texture
            Color[] data = new Color[cropRect.Width * cropRect.Height];
            baseTexture.GetData(0, cropRect, data, 0, cropRect.Width * cropRect.Height);

            Texture2D croppedTexture = new(SonOfRobinGame.GfxDev, cropRect.Width, cropRect.Height);
            croppedTexture.SetData(data);

            return croppedTexture;
        }

        public static Texture2D CropTextureAndAddPaddingCpu(Texture2D baseTexture, Rectangle cropRect, int padding, bool mirrorX = false)
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

                if (mirrorX)
                {
                    for (int x = 0; x < cropRect.Width; x++)
                    {
                        paddedArray1D[yMultipliedOutput + x] = colorData[yMultipliedInput + (cropRect.Width - x - 1)];
                    }
                }
                else
                {
                    for (int x = 0; x < cropRect.Width; x++)
                    {
                        paddedArray1D[yMultipliedOutput + x] = colorData[yMultipliedInput + x];
                    }
                }
            }

            Texture2D texture = new(graphicsDevice: SonOfRobinGame.GfxDev, width: paddedWidth, height: paddedHeight);
            texture.SetData(paddedArray1D);

            return texture;
        }

        public static Texture2D CropTextureAndAddPaddingGpu(Texture2D baseTexture, Rectangle cropRect, int padding, bool mirrorX = false)
        {
            int paddedWidth = cropRect.Width + (padding * 2);
            int paddedHeight = cropRect.Height + (padding * 2);

            RenderTarget2D croppedTexture = new(SonOfRobinGame.GfxDev, paddedWidth, paddedHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            Scene.SetRenderTarget(croppedTexture);
            SonOfRobinGame.GfxDev.Clear(Color.Transparent); // prevents from colored texture background on some devices (old Android, etc.)
            SonOfRobinGame.SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate);

            Rectangle destRect = new(x: padding, y: padding, width: cropRect.Width, height: cropRect.Height);

            SonOfRobinGame.SpriteBatch.Draw(texture: baseTexture, sourceRectangle: cropRect, origin: Vector2.Zero, destinationRectangle: destRect, color: Color.White, rotation: 0f, effects: mirrorX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth: 0);

            SonOfRobinGame.SpriteBatch.End();
            Scene.SetRenderTarget(null);

            return croppedTexture;
        }

        public static Color[,] ConvertTextureToGrid(Texture2D texture, int x, int y, int width, int height)
        {
            // getting 1D pixel array
            Color[] rawData = new Color[width * height];
            Rectangle extractRegion = new(x, y, width, height);
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

        public static void SaveTextureAsPNG(string pngPath, Texture2D texture)
        {
            try
            {
                Stream stream = File.Create(pngPath);
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                stream.Dispose();
            }
            catch (IOException)
            { }
        }

        public static void SaveTextureAsPNGResized(Texture2D texture, int maxWidth, int maxHeight, string pngPath)
        {
            if (maxWidth < 0) throw new ArgumentException($"MaxWidth {maxWidth} cannot be less than 0.");
            if (maxHeight < 0) throw new ArgumentException($"MaxHeight {maxHeight} cannot be less than 0.");

            // Get the texture data into an array
            Color[] textureData = new Color[texture.Width * texture.Height];
            texture.GetData(textureData);

            // Create an ImageSharp image from the texture data
            using (var image = new Image<Rgb24>(texture.Width, texture.Height)) // Rgb24 is used, otherwise alpha is wrong (Texture2D.FromStream)
            {
                // Parallelize the loop that copies pixel data from textureData to the image
                Parallel.For(0, texture.Height, SonOfRobinGame.defaultParallelOptions, y =>
                {
                    for (int x = 0; x < texture.Width; x++)
                    {
                        int index = y * texture.Width + x;
                        image[x, y] = new Rgb24(textureData[index].R, textureData[index].G, textureData[index].B);
                    }
                });

                // Calculate the new dimensions for scaling while preserving aspect ratio
                int newWidth, newHeight;
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    var newSize = Helpers.FitIntoSize(sourceWidth: image.Width, sourceHeight: image.Height, targetWidth: maxWidth, targetHeight: maxHeight);
                    newWidth = newSize.X;
                    newHeight = newSize.Y;
                }
                else
                {
                    newWidth = image.Width;
                    newHeight = image.Height;
                }

                // Resize the image
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(newWidth, newHeight),
                    Mode = ResizeMode.Max
                }));

                // Save the scaled image as a PNG
                using (var outputStream = new FileStream(pngPath, FileMode.Create))
                {
                    image.Save(outputStream, new PngEncoder { CompressionLevel = PngCompressionLevel.Level6 });
                }
            }
        }

        public static Texture2D LoadTextureFromPNG(string path)
        {
            FileStream fileStream = OpenFileStream(path);
            return LoadTextureFromFileStream(fileStream);
        }

        public static FileStream OpenFileStream(string path)
        {
            try
            {
                FileStream fileStream = new(path, FileMode.Open);
                return fileStream;
            }
            catch (FileNotFoundException)
            {
                // turned off, because it makes a lot of messages when loading anims for the first time
                // MessageLog.Add(debugMessage: true, text: $"FileNotFoundException while trying to read {Path.GetFileName(path)}.");
            }
            catch (IOException) // png file corrupted
            { MessageLog.Add(debugMessage: true, text: $"IOException while trying to read {Path.GetFileName(path)}."); }
            catch (InvalidOperationException) // png file corrupted
            { MessageLog.Add(debugMessage: true, text: $"InvalidOperationException while trying to read {Path.GetFileName(path)}."); }

            return null;
        }

        public static Texture2D LoadTextureFromFileStream(FileStream fileStream)
        {
            if (fileStream == null) return null;

            Texture2D loadedTexture;
            try
            {
                loadedTexture = Texture2D.FromStream(SonOfRobinGame.GfxDev, fileStream);
            }
            catch (InvalidOperationException)
            {
                MessageLog.Add(debugMessage: true, text: "InvalidOperationException while trying to read texture from fileStream.");

                fileStream.Dispose();
                return null;
            }

            fileStream.Dispose();
            return loadedTexture;
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

            Parallel.For(0, height, SonOfRobinGame.defaultParallelOptions, y =>
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
            Parallel.For(0, height, SonOfRobinGame.defaultParallelOptions, y =>
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

            Parallel.For(0, height, SonOfRobinGame.defaultParallelOptions, y =>
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

            Parallel.For(0, height, SonOfRobinGame.defaultParallelOptions, y =>
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
                    array2D[x, y] = array1D[yFactor + x];
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

        public static uint GenerateTextureChecksum(Texture2D texture)
        {
            uint hash = 2166136261u;

            // Get the pixel data from the texture
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            // Compute the hash
            for (int i = 0; i < data.Length; i++)
            {
                hash = (hash ^ data[i].PackedValue) * 16777619u;
            }

            return hash;
        }
    }
}