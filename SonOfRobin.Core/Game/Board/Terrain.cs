using System;
using System.Collections.Generic;
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
        private readonly byte minVal;
        private readonly byte maxVal;

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

            var serializedMapData = this.LoadTemplate();

            if (serializedMapData == null)
            {
                this.CreateGradientLines();

                var tuple = this.CreateNoiseMap(addBorder: addBorder);
                this.mapData = tuple.Item1;
                this.minVal = tuple.Item2;
                this.maxVal = tuple.Item3;

                this.SaveTemplate();
            }
            else
            {
                this.mapData = (Byte[,])serializedMapData["mapData"];
                this.minVal = (byte)serializedMapData["minVal"];
                this.maxVal = (byte)serializedMapData["maxVal"];
            }
        }

        public Byte GetMapData(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            return mapData[x / this.cell.grid.resDivider, y / this.cell.grid.resDivider];
        }

        public Byte GetMapDataRaw(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            // direct access, without taking resDivider into account
            return mapData[x, y];
        }

        private Dictionary<string, object> LoadTemplate()
        {
            var loadedData = (Dictionary<string, object>)FileReaderWriter.Load(this.templatePath);
            if (loadedData == null) return null;

            return loadedData;
        }

        public void SaveTemplate()
        {
            FileReaderWriter.Save(path: this.templatePath, savedObj: this.Serialize());
        }

        private Dictionary<string, object> Serialize()
        {
            var serializedMapData = new Dictionary<string, object>
            {
                { "mapData", this.mapData },
                { "minVal", this.minVal },
                { "maxVal", this.maxVal },
            };

            return serializedMapData;
        }

        private (byte[,], byte, byte) CreateNoiseMap(bool addBorder = false)
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

            int resDivider = this.cell.grid.resDivider;

            byte minVal = 255;
            byte maxVal = 0;

            for (int y = 0; y < this.cell.dividedHeight; y++)
            {
                int realY = y * resDivider;
                for (int x = 0; x < this.cell.dividedWidth; x++)
                {
                    int realX = x * resDivider;

                    int globalX = Math.Min(realX + this.cell.xMin, this.world.width - 1);
                    int globalY = Math.Min(realY + this.cell.yMin, this.world.height - 1);

                    double rawNoiseValue = noise.GetNoise(globalX, globalY) + 1; // 0-2 range
                    if (addBorder) rawNoiseValue = Math.Max(rawNoiseValue - Math.Max(gradientLineX[globalX], gradientLineY[globalY]), 0);

                    byte newVal = (byte)(rawNoiseValue * 128); // 0-255 range
                    newMapData[x, y] = newVal;

                    minVal = Math.Min(minVal, newVal);
                    maxVal = Math.Max(maxVal, newVal);
                }
            }

            return (newMapData, minVal, maxVal);
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
