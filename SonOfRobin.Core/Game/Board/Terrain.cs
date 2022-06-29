using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SonOfRobin
{

    public enum TerrainName
    {
        Height,
        Humidity
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

        public Byte[,] mapData;

        public static byte waterLevelMax = 84;
        public static byte volcanoLevelMin = 210;

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
            this.templatePath = Path.Combine(this.world.grid.templatePath, $"{Convert.ToString(name).ToLower()}_{cell.cellNoX}_{cell.cellNoY}.map");

            bool mapLoaded = this.LoadTemplate();
            if (!mapLoaded)
            {
                this.CreateGradientLines();
                this.CreateNoiseMap(addBorder: addBorder);
                this.SaveTemplate();
            }
        }

        private bool LoadTemplate()
        {
            var loadedData = (byte[,])FileReaderWriter.Load(this.templatePath); ;
            if (loadedData is null) return false;

            this.mapData = loadedData;
            return true;
        }

        public void SaveTemplate()
        {
            FileReaderWriter.Save(path: this.templatePath, savedObj: this.mapData);
        }

        private void CreateNoiseMap(bool addBorder = false)
        {
            var noise = this.world.noise;

            bool scaleWorld = true;

            noise.SetSeed(this.world.seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalOctaves(this.octaves);
            noise.SetFractalLacunarity(this.lacunarity);
            noise.SetFractalGain(this.gain);
            noise.SetFrequency((scaleWorld) ? this.frequency / this.world.width : this.frequency / 20000);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalWeightedStrength(this.persistence);

            this.mapData = new byte[cell.width, cell.height];

            for (int y = 0; y < cell.height; y++)
            {
                for (int x = 0; x < cell.width; x++)
                {
                    int globalX = x + cell.xMin;
                    int globalY = y + cell.yMin;

                    double rawNoiseValue = noise.GetNoise(globalX, globalY) + 1; // 0-2 range

                    if (addBorder)
                    {
                        rawNoiseValue = Math.Max(rawNoiseValue - Math.Max(gradientLineX[globalX], gradientLineY[globalY]), 0);
                    }

                    byte mapValue = Convert.ToByte((rawNoiseValue) * 128); // 0-255 range
                    this.mapData[x, y] = mapValue;
                }
            }

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
                    gradLine[i] = ((i - (length - gradientSize)) * valueMultiplier);
                }
            }

            return gradLine;
        }

    }
}
