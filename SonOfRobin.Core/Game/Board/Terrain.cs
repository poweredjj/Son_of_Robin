using System;
using System.IO;

namespace SonOfRobin
{
    public enum TerrainName
    {
        Height,
        Humidity,
        Danger
    }
    public class Terrain
    {
        public readonly World world;
        public readonly Cell cell;
        public readonly TerrainName name;
        private readonly string templatePath;
        private readonly float frequency;
        private readonly int octaves;
        private readonly float persistence;
        private readonly float lacunarity;
        private readonly float gain;

        private readonly Byte[,] mapData;

        public static byte waterLevelMax = 84;
        public static byte volcanoEdgeMin = 210;
        public static byte lavaMin = 225;
        public static byte safeZoneMax = 150;

        private static int gradientWidth;
        private static int gradientHeight;
        private static double[] gradientLineX;
        private static double[] gradientLineY;

        public Terrain(World world, Cell cell, TerrainName name, float frequency, int octaves, float persistence, float lacunarity, float gain, bool addBorder = false)
        {
            this.world = world;
            this.cell = cell;
            this.name = name;
            this.frequency = frequency;
            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.gain = gain;
            this.templatePath = Path.Combine(this.world.grid.gridTemplate.templatePath, $"{Convert.ToString(name).ToLower()}_{cell.cellNoX}_{cell.cellNoY}.map");

            this.mapData = this.LoadTemplate();

            if (this.mapData == null)
            {
                this.CreateGradientLines();
                this.mapData = this.CreateNoiseMap(addBorder: addBorder);
                this.SaveTemplate();
            }
        }

        public Byte GetMapData(int x, int y)
        { return mapData[x / this.cell.grid.resDivider, y / this.cell.grid.resDivider]; }

        private byte[,] LoadTemplate()
        {
            var loadedData = (byte[,])FileReaderWriter.Load(this.templatePath); ;
            if (loadedData == null) return null;

            return loadedData;
        }

        public void SaveTemplate()
        {
            FileReaderWriter.Save(path: this.templatePath, savedObj: this.mapData);
        }
        private byte[,] CreateNoiseMap(bool addBorder = false)
        {
            var noise = this.world.noise;

            bool scaleWorld = true;

            noise.SetSeed(this.world.seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalOctaves(this.octaves);
            noise.SetFractalLacunarity(this.lacunarity);
            noise.SetFractalGain(this.gain);
            noise.SetFrequency(scaleWorld ? this.frequency / this.world.width : this.frequency / 20000);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalWeightedStrength(this.persistence);

            var newMapData = new byte[this.cell.dividedWidth, this.cell.dividedHeight];

            int globalX, globalY;
            double rawNoiseValue;
            int realX, realY;
            int resDivider = this.cell.grid.resDivider;

            for (int y = 0; y < this.cell.dividedHeight; y++)
            {
                realY = y * resDivider;
                for (int x = 0; x < this.cell.dividedWidth; x++)
                {
                    realX = x * resDivider;

                    globalX = realX + this.cell.xMin;
                    globalY = realY + this.cell.yMin;

                    rawNoiseValue = noise.GetNoise(globalX, globalY) + 1; // 0-2 range
                    if (addBorder) rawNoiseValue = Math.Max(rawNoiseValue - Math.Max(gradientLineX[globalX], gradientLineY[globalY]), 0);

                    newMapData[x, y] = (byte)(rawNoiseValue * 128); // 0-255 range
                }
            }

            return newMapData;
        }

        private void CreateGradientLines()
        {
            // Gradient lines are static (to avoid calculating them for every cell),
            // so their sizes should be compared with world size.

            if (gradientWidth == this.world.width && gradientHeight == this.world.height) return;

            ushort gradientSize = Convert.ToUInt16(Math.Min(this.world.width / 8, this.world.height / 8));
            float edgeValue = 1.5f;
            gradientLineX = CreateGradientLine(length: this.world.width, gradientSize: gradientSize, edgeValue: edgeValue);
            gradientLineY = CreateGradientLine(length: this.world.height, gradientSize: gradientSize, edgeValue: edgeValue);
            gradientWidth = this.world.width;
            gradientHeight = this.world.height;
        }

        private static double[] CreateGradientLine(int length, ushort gradientSize, float edgeValue)
        {
            double valueMultiplier = (double)edgeValue / (double)gradientSize;
            double[] gradLine = new double[length];

            for (int i = 0; i < length; i++)
            {
                if (i < gradientSize)
                {
                    gradLine[i] = edgeValue - (i * valueMultiplier);
                }
                else if (i >= length - gradientSize)
                {
                    gradLine[i] = (i - (length - gradientSize)) * valueMultiplier;
                }
            }

            return gradLine;
        }
    }
}
