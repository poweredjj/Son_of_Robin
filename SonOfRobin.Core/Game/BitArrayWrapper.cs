using BigGustave;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.IO;

namespace SonOfRobin
{
    public class BitArrayWrapper
    {
        public readonly int width;
        public readonly int height;
        private readonly BitArray bitArray;

        public BitArrayWrapper(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.bitArray = new BitArray(width * height);
        }

        public void SetVal(int x, int y, bool value)
        {
            this.bitArray.Set(this.ConvertRaw2DCoordinatesTo1D(x, y), value);
        }

        public bool GetVal(int x, int y)
        {
            return this.bitArray.Get(this.ConvertRaw2DCoordinatesTo1D(x, y));
        }

        private int ConvertRaw2DCoordinatesTo1D(int x, int y)
        {
            return (y * this.width) + x;
        }

        public void FillWithTrue()
        {
            this.bitArray.SetAll(true);
        }

        public void FillWithFalse()
        {
            this.bitArray.SetAll(false);
        }

        public bool SaveToPNG(string path)
        {
            // BigGustave PngBuilder cannot write odd width properly
            bool widthOdd = this.width % 2 != 0;
            int correctedWidth = widthOdd ? this.width + 1 : this.width;

            PngBuilder builder = PngBuilder.Create(width: correctedWidth, height: this.height, hasAlphaChannel: false);

            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    Color pixel = this.GetVal(x, y) ? Color.Black : Color.White;
                    builder.SetPixel(pixel.R, pixel.G, pixel.B, x, y);
                }
            }

            if (widthOdd) // adding red line along "extra" edge, that it can be detected when opening
            {
                for (int y = 0; y < this.height; y++)
                {
                    builder.SetPixel(255, 0, 0, correctedWidth - 1, y);
                }
            }

            using (var memoryStream = new MemoryStream())
            {
                builder.Save(memoryStream);
                return FileReaderWriter.SaveMemoryStream(memoryStream: memoryStream, path: path);
            }
        }

        public static BitArrayWrapper LoadFromPNG(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    Png image = Png.Open(stream);

                    int width = image.Width;
                    int height = image.Height;

                    Pixel topRightEdgePixel = image.GetPixel(width - 1, 0);
                    Pixel bottomRightEdgePixel = image.GetPixel(width - 1, height - 1);
                    if (topRightEdgePixel.R == 255 && topRightEdgePixel.G == 0 && topRightEdgePixel.B == 0 &&
                        bottomRightEdgePixel.R == 255 && bottomRightEdgePixel.G == 0 && bottomRightEdgePixel.B == 0)
                    {
                        // detecting and correcting red edge marker (last column, that needs to be ignored)
                        width--;
                    }

                    BitArrayWrapper bitArrayWrapper = new BitArrayWrapper(width: width, height: height);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Pixel pixel = image.GetPixel(x, y);
                            bitArrayWrapper.SetVal(x, y, pixel.G != 255); // checking red channel is enough
                        }
                    }

                    return bitArrayWrapper;
                }
            }
            catch (FileNotFoundException) { return null; }
            catch (ArgumentOutOfRangeException) { return null; } // file corrupted
        }
    }
}