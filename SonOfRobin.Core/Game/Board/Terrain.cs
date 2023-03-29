﻿using System;
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

        public const byte waterLevelMax = 84;
        public const byte volcanoEdgeMin = 210;
        public const byte lavaMin = 225;
        public const byte biomeMin = 156;
        public const byte biomeDeep = 220;

        public readonly Name name;
        private Byte[,] mapData;
        public Grid Grid { get; private set; }
        public bool CreationInProgress { get; private set; }
        private int nextUpdateStartingRow;

        private readonly string templatePath;
        private readonly string terrainPngPath;
        private readonly string minValPngPath;
        private readonly string maxValPngPath;

        private readonly FastNoiseLite noise;

        private readonly int seed;
        private readonly bool addBorder;
        private readonly float frequency;
        private readonly int octaves;
        private readonly float persistence;
        private readonly float lacunarity;
        private readonly float gain;

        private double[] gradientLineX;
        private double[] gradientLineY;

        private byte[,] minValGridCell; // this values are stored in terrain, instead of cell
        private byte[,] maxValGridCell; // this values are stored in terrain, instead of cell

        public Terrain(Grid grid, Name name, float frequency, int octaves, float persistence, float lacunarity, float gain, bool addBorder = false)
        {
            this.CreationInProgress = true;
            this.nextUpdateStartingRow = 0;

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

            string templatePath = this.Grid.gridTemplate.templatePath;

            this.templatePath = Path.Combine(templatePath, $"terrain_{Convert.ToString(name).ToLower()}.json");
            this.terrainPngPath = Path.Combine(templatePath, $"terrain_{Convert.ToString(name).ToLower()}.png");
            this.minValPngPath = Path.Combine(templatePath, $"terrain_minval_{Convert.ToString(name).ToLower()}.png");
            this.maxValPngPath = Path.Combine(templatePath, $"terrain_maxval_{Convert.ToString(name).ToLower()}.png");
        }

        public void TryToLoadSavedTerrain()
        {
            byte[,] loadedMapData = GfxConverter.LoadPNGAs2DByteArray(this.terrainPngPath);

            Dictionary<string, object> serializedTerrainData = null;

            if (loadedMapData != null) serializedTerrainData = this.LoadTemplate();

            if (loadedMapData == null || serializedTerrainData == null)
            {
                var gradientLines = this.CreateGradientLines();
                this.gradientLineX = gradientLines.Item1;
                this.gradientLineY = gradientLines.Item2;

                this.mapData = new byte[this.Grid.dividedWidth, this.Grid.dividedHeight];
                this.minValGridCell = new byte[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
                this.maxValGridCell = new byte[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
            }
            else
            {
                this.mapData = loadedMapData;
                this.minValGridCell = (byte[,])serializedTerrainData["minValGridCell"];
                this.maxValGridCell = (byte[,])serializedTerrainData["maxValGridCell"];
                this.CreationInProgress = false;
            }
        }

        public void UpdateNoiseMap(int rowsToCalculate)
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

            bool lastUpdate = false;

            int yMin = this.nextUpdateStartingRow;
            int yMax = this.nextUpdateStartingRow + rowsToCalculate;
            if (yMax >= this.Grid.dividedHeight)
            {
                yMax = this.Grid.dividedHeight;
                lastUpdate = true;
            }

            Parallel.For(yMin, yMax, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, y =>
            {
                int realY = y * resDivider;

                for (int x = 0; x < this.Grid.dividedWidth; x++)
                {
                    int realX = x * resDivider;

                    double rawNoiseValue = noise.GetNoise(realX, realY) + 1; // 0-2 range
                    if (this.addBorder) rawNoiseValue = Math.Max(rawNoiseValue - Math.Max(gradientLineX[realX], gradientLineY[realY]), 0);

                    this.mapData[x, y] = (byte)(rawNoiseValue * 128); // 0-255 range;  can write to array using parallel, if every thread accesses its own indices
                }
            });

            if (lastUpdate) this.FinishUpdating();
            else this.nextUpdateStartingRow = yMax;
        }

        private void FinishUpdating()
        {
            this.UpdateMinMaxGridCell();
            this.SaveTemplate();
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

        private Dictionary<string, object> LoadTemplate()
        {
            var loadedData = FileReaderWriter.Load(this.templatePath);
            if (loadedData == null) return null;
            else return (Dictionary<string, object>)loadedData;
        }

        public void SaveTemplate()
        {
            GfxConverter.Save2DByteArrayToPng(array2D: this.mapData, path: this.terrainPngPath);
            GfxConverter.Save2DByteArrayToPng(array2D: this.minValGridCell, path: this.minValPngPath);
            GfxConverter.Save2DByteArrayToPng(array2D: this.maxValGridCell, path: this.maxValPngPath);

            FileReaderWriter.Save(path: this.templatePath, savedObj: this.Serialize());
        }

        private Dictionary<string, object> Serialize()
        {
            var serializedMapData = new Dictionary<string, object>
            {
                { "minValGridCell", this.minValGridCell },
                { "maxValGridCell", this.maxValGridCell },
            };

            return serializedMapData;
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