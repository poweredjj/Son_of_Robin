using BigGustave;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            PngBuilder builder = PngBuilder.Create(width: this.width, height: this.height, hasAlphaChannel: false);

            for (int y = 0; y < this.height; y++)
            {
                for (int x = 0; x < this.width; x++)
                {
                    Color pixel = this.GetVal(x, y) ? Color.Black : Color.White;
                    builder.SetPixel(pixel.R, pixel.G, pixel.B, x, y);
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
            Texture2D texture = GfxConverter.LoadTextureFromPNG(path);
            if (texture == null) return null;

            int width = texture.Width;
            int height = texture.Height;

            BitArrayWrapper bitArrayWrapper = new BitArrayWrapper(width: width, height: height);

            Color[] colorArray1D = new Color[width * height];
            texture.GetData(colorArray1D);

            for (int i = 0; i < bitArrayWrapper.bitArray.Length; i++)
            {
                // texture2D data is structured in the same way as bitArray - it just needs to be copied
                bitArrayWrapper.bitArray.Set(i, colorArray1D[i] == Color.Black);
            }

            return bitArrayWrapper;
        }
    }
}