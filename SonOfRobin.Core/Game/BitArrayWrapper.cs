using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

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
            this.bitArray = new BitArray(this.width * this.height);
        }

        public BitArrayWrapper(int width, int height, bool[] boolArray)
        {
            this.width = width;
            this.height = height;
            this.bitArray = new BitArray(boolArray);
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
            var workingArray = new bool[this.width * this.height];
            this.bitArray.CopyTo(workingArray, 0);

            using (var image = new Image<L8>(this.width, this.height)) // default image pixel value == 0
            {
                for (int i = 0; i < workingArray.Length; i++)
                {
                    if (!workingArray[i]) image[i % this.width, i / this.width] = new L8(255);
                }

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    image.Save(fileStream, new PngEncoder()
                    {
                        CompressionLevel = PngCompressionLevel.BestCompression,
                        // ColorType = PngColorType.Palette, // DO NOT USE THIS OPTION - will freeze at bigger sizes
                        // BitDepth = PngBitDepth.Bit1 // DO NOT USE THIS OPTION - will freeze at bigger sizes
                    }
                    );
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

                    var boolArray = new bool[width * height];

                    var _IMemoryGroup = image.GetPixelMemoryGroup();
                    int currentMemoryPos = 0;
                    foreach (var group in _IMemoryGroup)
                    {
                        var array1D = MemoryMarshal.AsBytes(group.Span).ToArray();

                        for (int i = 0; i < group.Length; i++)
                        {
                            if (array1D[i] == 0) boolArray[currentMemoryPos + i] = true;
                        }

                        currentMemoryPos += group.Length;
                    }

                    return new(width: width, height: height, boolArray: boolArray);
                }
            }
            catch (FileNotFoundException) { return null; }
            catch (UnknownImageFormatException) { return null; } // file corrupted
        }
    }
}