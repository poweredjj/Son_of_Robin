﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class ExtBoardProps
    {
        public enum Name : byte
        {
            Sea = 0,
            OuterBeach = 1,

            // each biome name must start with "Biome"
            BiomeSwamp = 2,
            BiomeRuins = 3,
        };

        private static readonly Name[] allExtPropNames = (Name[])Enum.GetValues(typeof(Name));
        public static readonly List<Name> allBiomes = allExtPropNames.Where(name => name.ToString().ToLower().StartsWith("biome")).ToList();

        public static readonly Dictionary<Name, int> biomeMultiplier = new()
        {
            // 1 == highest occurence
            { Name.BiomeSwamp, 1 },
            { Name.BiomeRuins, 3 },
        };

        public static readonly Dictionary<Name, List<TerrainSearch>> biomeConstrains = new()
        {
            { Name.BiomeSwamp, new List<TerrainSearch>{
                 new TerrainSearch(name: Terrain.Name.Height, min: 106, max: Terrain.rocksLevelMin - 1),
                 new TerrainSearch(name: Terrain.Name.Humidity, min: 80, max: 255),
            } },

            { Name.BiomeRuins, new List<TerrainSearch>{
                 new TerrainSearch(name: Terrain.Name.Height, min: 120, max: Terrain.rocksLevelMin - 1),
                 new TerrainSearch(name: Terrain.Name.Biome, min: Terrain.biomeMin + ((Terrain.biomeDeep - Terrain.biomeMin) / 5), max: 255),
            } }
        };

        public Grid Grid { get; private set; }

        private readonly string templateFolder;
        private readonly Dictionary<Name, string> extDataPNGPathByName;

        private readonly bool loadedFromTemplate; // to avoid saving template, after being loaded (needed because saving is not done inside constructor)

        private readonly Dictionary<Name, BitArrayWrapper> extDataByProperty;
        private readonly Dictionary<Name, BitArrayWrapper> containsPropertiesTrueGridCell;  // this values are stored in terrain, instead of cell
        private readonly Dictionary<Name, BitArrayWrapper> containsPropertiesFalseGridCell;  // this values are stored in terrain, instead of cell

        public bool CreationInProgress { get; private set; }

        public ExtBoardProps(Grid grid)
        {
            this.Grid = grid;

            this.CreationInProgress = true;
            this.templateFolder = this.Grid.gridTemplate.templatePath;

            this.extDataPNGPathByName = new Dictionary<Name, string>();
            this.extDataByProperty = new Dictionary<Name, BitArrayWrapper>();
            this.containsPropertiesTrueGridCell = new Dictionary<Name, BitArrayWrapper>();
            this.containsPropertiesFalseGridCell = new Dictionary<Name, BitArrayWrapper>();

            foreach (Name name in allExtPropNames)
            {
                this.extDataPNGPathByName[name] = Path.Combine(this.templateFolder, $"ext_bitmap_{name}.png");
            }

            bool loadedCorrectly = this.LoadTemplate();
            if (!loadedCorrectly)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "ext - creating new", color: Color.Yellow);

                this.extDataByProperty = this.MakeArrayCollection(); // need to be repeated

                foreach (Name name in allExtPropNames)
                {
                    this.containsPropertiesTrueGridCell[name] = new BitArrayWrapper(this.Grid.noOfCellsX, this.Grid.noOfCellsY);
                    this.containsPropertiesFalseGridCell[name] = new BitArrayWrapper(this.Grid.noOfCellsX, this.Grid.noOfCellsY);
                }

                this.loadedFromTemplate = false;
            }
            else
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "ext - loaded");
                this.CreationInProgress = false;
                this.loadedFromTemplate = true;
            }
        }

        public void AttachToNewGrid(Grid grid)
        {
            this.Grid = grid;
        }

        private Dictionary<Name, BitArrayWrapper> MakeArrayCollection()
        {
            var arrayCollection = new Dictionary<Name, BitArrayWrapper>();

            foreach (Name extPropName in allExtPropNames)
            {
                arrayCollection[extPropName] = new BitArrayWrapper(this.Grid.dividedWidth, this.Grid.dividedHeight);
            }

            return arrayCollection;
        }

        public void CreateContainsPropertiesGrid()
        {
            foreach (Cell cell in this.Grid.allCells)
            {
                int xMinRaw = cell.xMin / this.Grid.resDivider;
                int xMaxRaw = cell.xMax / this.Grid.resDivider;
                int yMinRaw = cell.yMin / this.Grid.resDivider;
                int yMaxRaw = cell.yMax / this.Grid.resDivider;

                foreach (var kvp in this.extDataByProperty)
                {
                    Name name = kvp.Key;

                    for (int rawX = xMinRaw; rawX <= xMaxRaw; rawX++)
                    {
                        for (int rawY = yMinRaw; rawY <= yMaxRaw; rawY++)
                        {
                            bool value = kvp.Value.GetVal(rawX, rawY);
                            if (value) this.containsPropertiesTrueGridCell[name].SetVal(x: cell.cellNoX, y: cell.cellNoY, value: true);
                            if (!value) this.containsPropertiesFalseGridCell[name].SetVal(x: cell.cellNoX, y: cell.cellNoY, value: true);
                        }
                    }
                }
            }
        }

        public void EndCreationAndSave()
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot end creation more than once.");

            this.CreationInProgress = false;
            this.SaveTemplate();
        }

        public bool CheckIfContainsPropertyForCell(Name name, bool value, int cellNoX, int cellNoY)
        {
            if (value) return this.containsPropertiesTrueGridCell[name].GetVal(cellNoX, cellNoY);
            else return this.containsPropertiesFalseGridCell[name].GetVal(cellNoX, cellNoY);
        }

        public Dictionary<Name, bool> GetValueDict(int x, int y, bool xyRaw = false)
        {
            var valueDict = new Dictionary<Name, bool>();

            foreach (Name name in this.extDataByProperty.Keys)
            {
                valueDict[name] = this.GetValue(name: name, x: x, y: y, xyRaw: xyRaw);
            }

            return valueDict;
        }

        public bool GetValue(Name name, int x, int y, bool xyRaw = false)
        {
            if (xyRaw) return this.extDataByProperty[name].GetVal(x, y);
            else return this.extDataByProperty[name].GetVal(x / this.Grid.resDivider, y / this.Grid.resDivider);
        }

        public void SetValue(Name name, bool value, int x, int y, bool xyRaw = false)
        {
            if (xyRaw) this.extDataByProperty[name].SetVal(x: x, y: y, value: value);
            else this.extDataByProperty[name].SetVal(x: x / this.Grid.resDivider, y: y / this.Grid.resDivider, value: value);
        }

        private bool LoadTemplate()
        {
            foreach (Name name in allExtPropNames)
            {
                // creating dictionary entries (will get corrupted sometimes if created in parallel)
                this.extDataByProperty[name] = null;
                this.containsPropertiesTrueGridCell[name] = null;
                this.containsPropertiesFalseGridCell[name] = null;
            }

            Parallel.ForEach(allExtPropNames, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, name =>
            {
                BitArrayWrapper bitArrayWrapper = BitArrayWrapper.LoadFromPNG(extDataPNGPathByName[name]);
                if (bitArrayWrapper != null) this.extDataByProperty[name] = bitArrayWrapper;
            });

            foreach (Name name in allExtPropNames)
            {
                if (this.extDataByProperty[name] == null) return false;
            }

            Parallel.ForEach(allExtPropNames, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, name =>
            {
                this.containsPropertiesTrueGridCell[name] = BitArrayWrapper.LoadFromPNG(GetContainsPropertiesPNGPath(name: name, contains: true));
                this.containsPropertiesFalseGridCell[name] = BitArrayWrapper.LoadFromPNG(GetContainsPropertiesPNGPath(name: name, contains: false));
            });

            foreach (Name name in allExtPropNames)
            {
                if (this.containsPropertiesTrueGridCell[name] == null) return false;
                if (this.containsPropertiesFalseGridCell[name] == null) return false;
            }

            return true;
        }

        public void SaveTemplate()
        {
            if (this.loadedFromTemplate) return;

            Parallel.ForEach(this.extDataByProperty, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, kvp =>
            {
                kvp.Value.SaveToPNG(this.extDataPNGPathByName[kvp.Key]);
            });

            Parallel.ForEach(allExtPropNames, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, name =>
            {
                this.containsPropertiesTrueGridCell[name].SaveToPNG(GetContainsPropertiesPNGPath(name: name, contains: true));
                this.containsPropertiesFalseGridCell[name].SaveToPNG(GetContainsPropertiesPNGPath(name: name, contains: false));
            });
        }

        private string GetContainsPropertiesPNGPath(Name name, bool contains)
        {
            return Path.Combine(this.templateFolder, $"ext_contains_{name}_{contains}.png");
        }
    }
}