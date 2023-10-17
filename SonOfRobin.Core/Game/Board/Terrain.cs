using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public readonly struct TerrainSearch
    {
        public readonly Terrain.Name name;
        public readonly byte min;
        public readonly byte max;

        public TerrainSearch(Terrain.Name name, byte min, byte max)
        {
            this.name = name;
            this.min = min;
            this.max = max;
        }
    }

    public class Terrain
    {
        public enum Name : byte
        {
            Height,
            Humidity,
            Biome,
        }

        public static readonly Name[] allTerrains = (Name[])Enum.GetValues(typeof(Name));

        public const byte waterLevelMax = 84;
        public const byte rocksLevelMin = 160;
        public const byte volcanoEdgeMin = 210;
        public const byte lavaMin = 225;
        public const byte biomeMin = 156;
        public const byte biomeDeep = 220;

        public readonly Name name;
        private Byte[,] mapData;
        public Grid Grid { get; private set; }
        public bool CreationInProgress { get; private set; }

        private readonly string terrainPngPath;
        private readonly string minValPngPath;
        private readonly string maxValPngPath;

        private readonly FastNoiseLite noise;

        private readonly int seed;
        private readonly bool addBorder;
        private readonly RangeConversion[] rangeConversions;

        private readonly float frequency;
        private readonly int octaves;
        private readonly float persistence;
        private readonly float lacunarity;
        private readonly float gain;

        private double[] gradientLineX;
        private double[] gradientLineY;

        private byte[,] minValGridCell; // this values are stored in terrain, instead of cell
        private byte[,] maxValGridCell; // this values are stored in terrain, instead of cell

        public Terrain(Grid grid, Name name, float frequency, int octaves, float persistence, float lacunarity, float gain, bool addBorder = false, List<RangeConversion> rangeConversions = null)
        {
            this.CreationInProgress = true;

            this.name = name;
            this.Grid = grid;
            this.seed = grid.gridTemplate.seed;

            this.noise = new FastNoiseLite(this.seed);

            this.frequency = frequency;
            this.octaves = octaves;
            this.persistence = persistence;
            this.lacunarity = lacunarity;
            this.gain = gain;

            this.addBorder = addBorder;
            this.rangeConversions = rangeConversions == null ? new RangeConversion[0] : rangeConversions.ToArray();

            string templatePath = this.Grid.gridTemplate.templatePath;
            this.terrainPngPath = Path.Combine(templatePath, $"terrain_{Convert.ToString(name).ToLower()}_flipped.png");
            this.minValPngPath = Path.Combine(templatePath, $"terrain__val_{Convert.ToString(name).ToLower()}_min.png");
            this.maxValPngPath = Path.Combine(templatePath, $"terrain__val_{Convert.ToString(name).ToLower()}_max.png");
        }

        public void TryToLoadSavedTerrain()
        {
            byte[,] loadedMinVal = null;
            byte[,] loadedMaxVal = null;
            byte[,] loadedMapData = null;

            if (this.Grid.serializable)
            {
                loadedMinVal = GfxConverter.LoadGreyscalePNGAs2DByteArray(this.minValPngPath);
                if (loadedMinVal != null) loadedMaxVal = GfxConverter.LoadGreyscalePNGAs2DByteArray(this.maxValPngPath);
                if (loadedMaxVal != null) loadedMapData = GfxConverter.LoadGreyscalePNGAs2DByteArraySquareFlipped(this.terrainPngPath);
            }

            if (loadedMinVal == null || loadedMaxVal == null || loadedMapData == null)
            {
                MessageLog.Add(debugMessage: true, text: $"terrain {Convert.ToString(name).ToLower()} - creating new", textColor: Color.Yellow);

                var gradientLines = this.CreateGradientLines();
                this.gradientLineX = gradientLines.Item1;
                this.gradientLineY = gradientLines.Item2;

                this.mapData = new byte[this.Grid.dividedWidth, this.Grid.dividedHeight];
                this.minValGridCell = new byte[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
                this.maxValGridCell = new byte[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
            }
            else
            {
                MessageLog.Add(debugMessage: true, text: $"terrain {Convert.ToString(name).ToLower()} - loaded");

                this.mapData = loadedMapData;
                this.minValGridCell = loadedMinVal;
                this.maxValGridCell = loadedMaxVal;
                this.CreationInProgress = false;
            }
        }

        public void GenerateNoiseMap()
        {
            if (!this.CreationInProgress) return;

            this.noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            this.noise.SetFractalOctaves(this.octaves);
            this.noise.SetFractalLacunarity(this.lacunarity);
            this.noise.SetFractalGain(this.gain);
            this.noise.SetFrequency(this.frequency / 45000);
            this.noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            this.noise.SetFractalWeightedStrength(this.persistence);

            int resDivider = this.Grid.resDivider;

            Parallel.For(0, this.Grid.dividedHeight, SonOfRobinGame.defaultParallelOptions, y =>
            {
                int realY = y * resDivider;

                for (int x = 0; x < this.Grid.dividedWidth; x++)
                {
                    int realX = x * resDivider;

                    double rawNoiseValue = this.noise.GetNoise(realX, realY) + 1; // 0-2 range
                    if (this.addBorder) rawNoiseValue = Math.Max(rawNoiseValue - Math.Max(gradientLineX[realX], gradientLineY[realY]), 0);

                    this.mapData[x, y] = (byte)(rawNoiseValue * 128); // 0-255 range;  can write to array using parallel, if every thread accesses its own indices

                    foreach (RangeConversion rangeConversion in this.rangeConversions)
                    {
                        this.mapData[x, y] = rangeConversion.ConvertRange(this.mapData[x, y]);
                    }
                }
            });

            this.UpdateMinMaxGridCell();
        }

        public void SaveTemplate()
        {
            if (!this.CreationInProgress) return;

            GfxConverter.Save2DByteArrayGreyscaleToPNGSquareFlipped(array2D: this.mapData, path: this.terrainPngPath);
            GfxConverter.Save2DByteArrayGreyscaleToPNG(array2D: this.minValGridCell, path: this.minValPngPath);
            GfxConverter.Save2DByteArrayGreyscaleToPNG(array2D: this.maxValGridCell, path: this.maxValPngPath);

            this.CreationInProgress = false;
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

        public readonly struct RangeConversion
        {
            public readonly byte inMin;
            public readonly byte inMax;
            public readonly byte outMin;
            public readonly byte outMax;

            public RangeConversion(byte inMin, byte inMax, byte outMin, byte outMax)
            {
                this.inMin = inMin;
                this.inMax = inMax;
                this.outMin = outMin;
                this.outMax = outMax;
            }

            public byte ConvertRange(byte value)
            {
                return value < inMin || value > inMax ?
                    value :
                    (byte)Helpers.ConvertRange(oldMin: this.inMin, oldMax: this.inMax, newMin: this.outMin, newMax: this.outMax, oldVal: value, clampToEdges: true);
            }
        }
    }
}