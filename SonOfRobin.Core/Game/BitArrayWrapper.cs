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

        public virtual void SetVal(int x, int y, bool value)
        {
            if (x < 0 || y < 0 || x >= this.width || y >= this.height) return;
            this.bitArray.Set((y * this.width) + x, value);
        }

        public virtual bool GetVal(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.width || y >= this.height) return false;
            return this.bitArray.Get((y * this.width) + x);
        }

        public virtual bool GetValNoBoundsCheck(int x, int y)
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

        public virtual bool SaveToPNG(string path)
        {
            var workingArray = new bool[this.width * this.height];
            this.bitArray.CopyTo(workingArray, 0);

            using (var image = new Image<L8>(this.width, this.height)) // default image pixel value == 0
            {
                for (int i = 0; i < workingArray.Length; i++)
                {
                    if (!workingArray[i]) image[i % this.width, i / this.width] = new L8(255);
                }

                try
                {
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
                catch (DirectoryNotFoundException) { return false; }
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
            catch (DirectoryNotFoundException) { return null; }
        }

        public List<BitArrayWrapperChunk> SplitIntoChunks(int chunkWidth, int chunkHeight)
        {
            if (chunkWidth <= 0 || chunkHeight <= 0) throw new ArgumentException("Chunk dimensions and BitArray dimensions must be positive.");

            int numChunksX = (int)Math.Ceiling((double)this.width / (double)chunkWidth);
            int numChunksY = (int)Math.Ceiling((double)this.height / (double)chunkHeight);

            var chunks = new List<BitArrayWrapperChunk>();

            for (int chunkY = 0; chunkY < numChunksY; chunkY++)
            {
                for (int chunkX = 0; chunkX < numChunksX; chunkX++)
                {
                    int xPos = chunkX * chunkWidth;
                    int yPos = chunkY * chunkHeight;

                    int currentChunkWidth = chunkWidth;
                    int currentChunkHeight = chunkHeight;

                    currentChunkWidth = Math.Min(currentChunkWidth, this.width - xPos);
                    currentChunkHeight = Math.Min(currentChunkHeight, this.height - yPos);

                    chunks.Add(new BitArrayWrapperChunk(bitArrayWrapper: this, width: currentChunkWidth, height: currentChunkHeight, xOffset: xPos, yOffset: yPos));
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

        public bool HasAnyPixelSet
        {
            get
            {
                for (int y = 0; y < this.height; y++)
                {
                    for (int x = 0; x < this.width; x++)
                    {
                        if (this.GetVal(x, y)) return true;
                    }
                }

                return false;
            }
        }

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

    public class BitArrayWrapperEmpty : BitArrayWrapper
    {
        public BitArrayWrapperEmpty() : base(width: 1, height: 1)
        {
        }

        public override bool GetVal(int x, int y)
        {
            return base.GetVal(0, 0);
        }

        public override bool GetValNoBoundsCheck(int x, int y)
        {
            return base.GetVal(0, 0);
        }

        public override void SetVal(int x, int y, bool value)
        {
            base.SetVal(0, 0, value);
        }

        public override bool SaveToPNG(string path)
        {
            return false;
        }
    }
}