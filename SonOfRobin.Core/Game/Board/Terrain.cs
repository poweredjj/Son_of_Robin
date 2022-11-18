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
        public Grid Grid { get; private set; }

        private readonly int seed;

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

        public Terrain(Grid grid, Name name, float frequency, int octaves, float persistence, float lacunarity, float gain, bool addBorder = false)
        {
            this.name = name;
            this.Grid = grid;
            this.seed = grid.gridTemplate.seed;

            this.frequency = frequency;
            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.gain = gain;

            this.templatePath = Path.Combine(this.Grid.gridTemplate.templatePath, $"{Convert.ToString(name).ToLower()}.map");

            var serializedTerrainData = this.LoadTemplate();

            if (serializedTerrainData == null)
            {
                var gradientLines = this.CreateGradientLines();
                this.gradientLineX = gradientLines.Item1;
                this.gradientLineY = gradientLines.Item2;

                this.mapData = this.CreateNoiseMap(addBorder: addBorder);

                this.minValGridCell = new byte[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
                this.maxValGridCell = new byte[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
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

        public void AttachToNewGrid(Grid grid)
        {
            this.Grid = grid;
        }


        public Byte GetMapData(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            return this.mapData[x / this.Grid.resDivider, y / this.Grid.resDivider];
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
            FastNoiseLite noise = new FastNoiseLite(this.seed);

            noise.SetSeed(this.seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalOctaves(this.octaves);
            noise.SetFractalLacunarity(this.lacunarity);
            noise.SetFractalGain(this.gain);
            noise.SetFrequency(this.frequency / 45000);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalWeightedStrength(this.persistence);

            var newMapData = new byte[this.Grid.dividedWidth, this.Grid.dividedHeight];

            int resDivider = this.Grid.resDivider;

            Parallel.For(0, this.Grid.dividedHeight, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                int realY = y * resDivider;

                for (int x = 0; x < this.Grid.dividedWidth; x++)
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
            ushort gradientSize = Convert.ToUInt16(Math.Min(this.Grid.width / 8, this.Grid.height / 8));
            float edgeValue = 1.5f;
            double[] gradientX = CreateGradientLine(length: this.Grid.width, gradientSize: gradientSize, edgeValue: edgeValue);
            double[] gradientY = CreateGradientLine(length: this.Grid.height, gradientSize: gradientSize, edgeValue: edgeValue);
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
            foreach (Cell cell in this.Grid.allCells)
            {
                int xMinRaw = cell.xMin / this.Grid.resDivider;
                int xMaxRaw = cell.xMax / this.Grid.resDivider;
                int yMinRaw = cell.yMin / this.Grid.resDivider;
                int yMaxRaw = cell.yMax / this.Grid.resDivider;

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