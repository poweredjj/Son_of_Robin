using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
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
            this.bitArray.Set((y * this.width) + x, value);
        }

        public bool GetVal(int x, int y)
        {
            return this.bitArray.Get((y * this.width) + x);
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

        public List<BitArrayWrapperChunk> SplitIntoChunks(int chunkWidth, int chunkHeight, int xOverlap = 0, int yOverlap = 0)
        {
            if (chunkWidth <= 0 || chunkHeight <= 0) throw new ArgumentException("Chunk dimensions and BitArray dimensions must be positive.");

            int numChunksX = (int)Math.Ceiling((double)(this.width - xOverlap) / (double)(chunkWidth - xOverlap));
            int numChunksY = (int)Math.Ceiling((double)(this.height - yOverlap) / (double)(chunkHeight - yOverlap));

            var chunks = new List<BitArrayWrapperChunk>();

            for (int chunkY = 0; chunkY < numChunksY; chunkY++)
            {
                for (int chunkX = 0; chunkX < numChunksX; chunkX++)
                {
                    int xOffset = chunkX * (chunkWidth - xOverlap);
                    int yOffset = chunkY * (chunkHeight - yOverlap);

                    int currentChunkWidth = Math.Min(chunkWidth + xOverlap, this.width - (chunkX * (chunkWidth - xOverlap)));
                    int currentChunkHeight = Math.Min(chunkHeight + xOverlap, this.height - (chunkY * (chunkHeight - yOverlap)));

                    chunks.Add(new BitArrayWrapperChunk(bitArrayWrapper: this, width: currentChunkWidth, height: currentChunkHeight, xOffset: xOffset, yOffset: yOffset));
                }
            }

            return chunks;
        }
    }

    public readonly struct BitArrayWrapperChunk
    {
        public readonly BitArrayWrapper bitArrayWrapper;
        public readonly int width;
        public readonly int height;
        public readonly int xOffset;
        public readonly int yOffset;

        public BitArrayWrapperChunk(BitArrayWrapper bitArrayWrapper, int width, int height, int xOffset, int yOffset)
        {
            this.bitArrayWrapper = bitArrayWrapper;
            this.width = width;
            this.height = height;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
        }

        public bool GetVal(int x, int y)
        {
            return this.bitArrayWrapper.GetVal(x: x + this.xOffset, y: y + this.yOffset);
        }
    }
}