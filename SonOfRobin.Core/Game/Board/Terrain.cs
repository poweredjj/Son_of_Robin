using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class Terrain
    {
        public enum Name
        {
            Height,
            Humidity,
            Biome
        }

        public static readonly Name[] allTerrains = (Name[])Enum.GetValues(typeof(Name));

        public static readonly byte waterLevelMax = 84;
        public static readonly byte volcanoEdgeMin = 210;
        public static readonly byte lavaMin = 225;
        public static readonly byte biomeMin = 156;
        public static readonly byte biomeDeep = 220;

        public readonly Name name;
        private readonly Byte[,] mapData;

        public readonly Grid grid;
        public readonly World world;

        private readonly string templatePath;

        private readonly float frequency;
        private readonly int octaves;
        private readonly float persistence;
        private readonly float lacunarity;
        private readonly float gain;

        private readonly double[] gradientLineX;
        private readonly double[] gradientLineY;

        private readonly byte[,] minValGridCell; // this values are stored in terrain, instead of cell
        private readonly byte[,] maxValGridCell; // this values are stored in terrain, instead of cell

        public Terrain(World world, Name name, float frequency, int octaves, float persistence, float lacunarity, float gain, bool addBorder = false)
        {
            this.name = name;
            this.world = world;
            this.grid = this.world.grid;

            this.frequency = frequency;
            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.gain = gain;

            this.templatePath = Path.Combine(this.grid.gridTemplate.templatePath, $"{Convert.ToString(name).ToLower()}.map");

            var serializedTerrainData = this.LoadTemplate();

            if (serializedTerrainData == null)
            {
                var gradientLines = this.CreateGradientLines();
                this.gradientLineX = gradientLines.Item1;
                this.gradientLineY = gradientLines.Item2;

                this.mapData = this.CreateNoiseMap(addBorder: addBorder);

                this.minValGridCell = new byte[this.grid.noOfCellsX, this.grid.noOfCellsY];
                this.maxValGridCell = new byte[this.grid.noOfCellsX, this.grid.noOfCellsY];
                this.UpdateMinMaxGridCell();

                this.SaveTemplate();
            }
            else
            {
                this.mapData = (Byte[,])serializedTerrainData["mapData"];
                this.minValGridCell = (byte[,])serializedTerrainData["minValGridCell"];
                this.maxValGridCell = (byte[,])serializedTerrainData["maxValGridCell"];
            }
        }

        public Byte GetMapData(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            return this.mapData[x / this.grid.resDivider, y / this.grid.resDivider];
        }

        public Byte GetMapDataRaw(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            // direct access, without taking resDivider into account
            return this.mapData[x, y];
        }

        public byte GetMinValueForCell(int cellNoX, int cellNoY)
        {
            return this.minValGridCell[cellNoX, cellNoY];
        }

        public byte GetMaxValueForCell(int cellNoX, int cellNoY)
        {
            return this.maxValGridCell[cellNoX, cellNoY];
        }

        private Dictionary<string, object> LoadTemplate()
        {
            var loadedData = FileReaderWriter.Load(this.templatePath);
            if (loadedData == null) return null;
            else return (Dictionary<string, object>)loadedData;
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
                { "minValGridCell", this.minValGridCell },
                { "maxValGridCell", this.maxValGridCell },
            };

            return serializedMapData;
        }

        private byte[,] CreateNoiseMap(bool addBorder = false)
        {
            FastNoiseLite noise = this.world.noise;

            noise.SetSeed(this.world.seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalOctaves(this.octaves);
            noise.SetFractalLacunarity(this.lacunarity);
            noise.SetFractalGain(this.gain);
            noise.SetFrequency(this.frequency / 30000);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalWeightedStrength(this.persistence);

            var newMapData = new byte[this.grid.dividedWidth, this.grid.dividedHeight];

            int resDivider = this.grid.resDivider;

            Parallel.For(0, this.grid.dividedHeight, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                int realY = y * resDivider;

                for (int x = 0; x < this.grid.dividedWidth; x++)
                {
                    int realX = x * resDivider;

                    double rawNoiseValue = noise.GetNoise(realX, realY) + 1; // 0-2 range
                    if (addBorder) rawNoiseValue = Math.Max(rawNoiseValue - Math.Max(gradientLineX[realX], gradientLineY[realY]), 0);

                    byte newVal = (byte)(rawNoiseValue * 128); // 0-255 range
                    newMapData[x, y] = newVal; // can write to array using parallel, if every thread accesses its own indices
                }
            });

            return newMapData;
        }

        private (double[], double[]) CreateGradientLines()
        {
            ushort gradientSize = Convert.ToUInt16(Math.Min(this.world.width / 8, this.world.height / 8));
            float edgeValue = 1.5f;
            double[] gradientX = CreateGradientLine(length: this.world.width, gradientSize: gradientSize, edgeValue: edgeValue);
            double[] gradientY = CreateGradientLine(length: this.world.height, gradientSize: gradientSize, edgeValue: edgeValue);
            return (gradientX, gradientY);
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

        private void UpdateMinMaxGridCell()
        {
            foreach (Cell cell in this.grid.allCells)
            {
                int xMinRaw = cell.xMin / this.grid.resDivider;
                int xMaxRaw = cell.xMax / this.grid.resDivider;
                int yMinRaw = cell.yMin / this.grid.resDivider;
                int yMaxRaw = cell.yMax / this.grid.resDivider;

                byte minVal = 255;
                byte maxVal = 0;

                for (int x = xMinRaw; x <= xMaxRaw; x++)
                {
                    for (int y = yMinRaw; y <= yMaxRaw; y++)
                    {
                        byte value = this.GetMapDataRaw(x, y);
                        minVal = Math.Min(minVal, value);
                        maxVal = Math.Max(maxVal, value);
                    }
                }

                this.minValGridCell[cell.cellNoX, cell.cellNoY] = minVal;
                this.maxValGridCell[cell.cellNoX, cell.cellNoY] = maxVal;
            }
        }
    }
}