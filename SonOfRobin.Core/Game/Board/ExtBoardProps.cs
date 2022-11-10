using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public struct BiomeConstrain
    {
        public readonly Terrain.Name terrainName;
        public readonly byte min;
        public readonly byte max;

        public BiomeConstrain(Terrain.Name terrainName, byte min, byte max)
        {
            this.terrainName = terrainName;
            this.min = min;
            this.max = max;
        }
    }

    public class ExtBoardProps
    {
        public enum ExtPropName
        { Sea, OuterBeach, BiomeSwamp }; // each biome name must start with "biome"

        private static readonly ExtPropName[] allExtPropNames = (ExtPropName[])Enum.GetValues(typeof(ExtPropName));
        public static readonly List<ExtPropName> allBiomes = allExtPropNames.Where(name => name.ToString().ToLower().StartsWith("biome")).ToList();

        public static readonly Dictionary<ExtPropName, List<BiomeConstrain>> biomeConstrains = new Dictionary<ExtPropName, List<BiomeConstrain>> {
            { ExtPropName.BiomeSwamp, new List<BiomeConstrain>{
                 new BiomeConstrain(terrainName: Terrain.Name.Height, min: 106, max: 160),
                 new BiomeConstrain(terrainName: Terrain.Name.Humidity, min: 80, max: 255),
            } }
        };

        public readonly Grid grid;

        private readonly Dictionary<ExtPropName, BitArray> extDataByProperty;
        private readonly string templatePath;
        private readonly bool loadedFromTemplate; // to avoid saving template, after being loaded (needed because saving is not done inside constructor)

        private readonly List<ExtPropName>[,] containsPropertiesTrueGridCell; // this values are stored in terrain, instead of cell
        private readonly List<ExtPropName>[,] containsPropertiesFalseGridCell; // this values are stored in terrain, instead of cell

        public bool CreationInProgress { get; private set; }

        public ExtBoardProps(Grid grid)
        {
            this.grid = grid;

            this.CreationInProgress = true;
            this.templatePath = Path.Combine(this.grid.gridTemplate.templatePath, $"properties.ext");

            var serializedData = this.LoadTemplate();
            if (serializedData == null)
            {
                this.extDataByProperty = this.MakeArrayCollection();

                this.containsPropertiesTrueGridCell = new List<ExtPropName>[this.grid.noOfCellsX, this.grid.noOfCellsY];
                this.containsPropertiesFalseGridCell = new List<ExtPropName>[this.grid.noOfCellsX, this.grid.noOfCellsY];
                this.loadedFromTemplate = false;
            }
            else
            {
                this.extDataByProperty = (Dictionary<ExtPropName, BitArray>)serializedData["extDataByProperty"];
                this.containsPropertiesTrueGridCell = (List<ExtPropName>[,])serializedData["containsPropertiesTrueGridCell"];
                this.containsPropertiesFalseGridCell = (List<ExtPropName>[,])serializedData["containsPropertiesFalseGridCell"];
                this.CreationInProgress = false;
                this.loadedFromTemplate = true;
            }
        }

        private Dictionary<ExtPropName, BitArray> MakeArrayCollection()
        {
            var arrayCollection = new Dictionary<ExtPropName, BitArray>();

            foreach (ExtPropName extPropName in allExtPropNames)
            {
                arrayCollection[extPropName] = new BitArray(this.grid.dividedWidth * this.grid.dividedHeight);
            }

            return arrayCollection;
        }

        public void EndCreationAndSave()
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot end creation more than once.");

            foreach (Cell cell in this.grid.allCells)
            {
                int xMinRaw = cell.xMin / this.grid.resDivider;
                int xMaxRaw = cell.xMax / this.grid.resDivider;
                int yMinRaw = cell.yMin / this.grid.resDivider;
                int yMaxRaw = cell.yMax / this.grid.resDivider;

                this.containsPropertiesTrueGridCell[cell.cellNoX, cell.cellNoY] = new List<ExtPropName>();
                this.containsPropertiesFalseGridCell[cell.cellNoX, cell.cellNoY] = new List<ExtPropName>();

                List<ExtPropName> containsPropertiesTrue = this.containsPropertiesTrueGridCell[cell.cellNoX, cell.cellNoY];
                List<ExtPropName> containsPropertiesFalse = this.containsPropertiesFalseGridCell[cell.cellNoX, cell.cellNoY];

                foreach (var kvp in this.extDataByProperty)
                {
                    ExtPropName name = kvp.Key;

                    for (int x = xMinRaw; x <= xMaxRaw; x++)
                    {
                        for (int y = yMinRaw; y <= yMaxRaw; y++)
                        {
                            bool value = kvp.Value.Get(Convert2DCoordinatesTo1D(x, y));
                            if (value && !containsPropertiesTrue.Contains(name)) containsPropertiesTrue.Add(name);
                            if (!value && !containsPropertiesFalse.Contains(name)) containsPropertiesFalse.Add(name);
                        }
                    }
                }
            }

            this.CreationInProgress = false;
            if (!this.loadedFromTemplate) this.SaveTemplate();
        }

        public bool CheckIfContainsPropertyForCell(ExtPropName name, bool value, int cellNoX, int cellNoY)
        {
            if (value) return this.containsPropertiesTrueGridCell[cellNoX, cellNoY].Contains(name);
            else return this.containsPropertiesFalseGridCell[cellNoX, cellNoY].Contains(name);
        }

        private int ConvertRaw2DCoordinatesTo1D(int x, int y)
        {
            return (y * this.grid.dividedWidth) + x;
        }

        private int Convert2DCoordinatesTo1D(int x, int y)
        {
            return (y / this.grid.resDivider * this.grid.dividedWidth) + (x / this.grid.resDivider);
        }

        public bool GetValue(ExtPropName name, int x, int y, bool xyRaw)
        {
            if (xyRaw) return this.extDataByProperty[name].Get(this.ConvertRaw2DCoordinatesTo1D(x, y));
            else return this.extDataByProperty[name].Get(this.Convert2DCoordinatesTo1D(x, y));
        }

        public Dictionary<ExtPropName, bool> GetValueDict(int x, int y, bool xyRaw)
        {
            var valueDict = new Dictionary<ExtPropName, bool>();

            foreach (ExtPropName name in this.extDataByProperty.Keys)
            {
                valueDict[name] = this.GetValue(name: name, x: x, y: y, xyRaw: xyRaw);
            }

            return valueDict;
        }

        public void SetValue(ExtPropName name, bool value, int x, int y, bool xyRaw)
        {
            if (xyRaw) this.extDataByProperty[name].Set(index: this.ConvertRaw2DCoordinatesTo1D(x, y), value: value);
            else this.extDataByProperty[name].Set(index: this.Convert2DCoordinatesTo1D(x, y), value: value);
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
            var serializedData = new Dictionary<string, object>
            {
                { "extDataByProperty", this.extDataByProperty },
                { "containsPropertiesTrueGridCell", this.containsPropertiesTrueGridCell },
                { "containsPropertiesFalseGridCell", this.containsPropertiesFalseGridCell },
            };

            return serializedData;
        }
    }
}