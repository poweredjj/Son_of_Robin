using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Threading.Tasks;
using System.IO;


namespace SonOfRobin
{
    public class GfxConverter
    {

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

        public static Texture2D Convert2DArrayToTexture(byte[,] inputArray)
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
