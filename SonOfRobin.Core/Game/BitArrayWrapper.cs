using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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
            byte byteBlack = 0;
            byte byteWhite = 255;

            using (var image = new Image<L8>(this.width, this.height))
            {
                for (int y = 0; y < this.height; y++)
                {
                    for (int x = 0; x < this.width; x++)
                    {
                        image[x, y] = new L8(this.GetVal(x, y) ? byteBlack : byteWhite);
                    }
                }

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    image.Save(fileStream, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });
                    return true;
                }
            }
        }

        public static BitArrayWrapper LoadFromPNG(string path)
        {
            try
            {
                using (var image = Image.Load<L8>(path))
                {
                    int width = image.Width;
                    int height = image.Height;

                    BitArrayWrapper bitArrayWrapper = new BitArrayWrapper(width: width, height: height);

                    for (int y = 0; y < height; y++)
                    {
                        int yFactor = y * width;

                        for (int x = 0; x < width; x++)
                        {
                            bitArrayWrapper.bitArray.Set(yFactor + x, image[x, y].PackedValue == 0);
                        }
                    }

                    return bitArrayWrapper;
                }
            }
            catch (FileNotFoundException) { return null;}
            catch (UnknownImageFormatException) { return null;} // file corrupted      
        }
    }
}