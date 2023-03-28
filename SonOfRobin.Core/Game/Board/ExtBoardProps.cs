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
        public enum Name
        { Sea, OuterBeach, BiomeSwamp }; // each biome name must start with "biome"

        private static readonly Name[] allExtPropNames = (Name[])Enum.GetValues(typeof(Name));
        public static readonly List<Name> allBiomes = allExtPropNames.Where(name => name.ToString().ToLower().StartsWith("biome")).ToList();

        public static readonly Dictionary<Name, List<BiomeConstrain>> biomeConstrains = new Dictionary<Name, List<BiomeConstrain>> {
            { Name.BiomeSwamp, new List<BiomeConstrain>{
                 new BiomeConstrain(terrainName: Terrain.Name.Height, min: 106, max: 159),
                 new BiomeConstrain(terrainName: Terrain.Name.Humidity, min: 80, max: 255),
            } }
        };

        public Grid Grid { get; private set; }

        private readonly Dictionary<Name, BitArray> extDataByProperty;
        private readonly string templatePath;
        private readonly bool loadedFromTemplate; // to avoid saving template, after being loaded (needed because saving is not done inside constructor)

        private readonly List<Name>[,] containsPropertiesTrueGridCell; // this values are stored in terrain, instead of cell
        private readonly List<Name>[,] containsPropertiesFalseGridCell; // this values are stored in terrain, instead of cell

        public bool CreationInProgress { get; private set; }

        public ExtBoardProps(Grid grid)
        {
            this.Grid = grid;

            this.CreationInProgress = true;
            this.templatePath = Path.Combine(this.Grid.gridTemplate.templatePath, $"properties_ext.json");

            var serializedData = this.LoadTemplate();
            if (serializedData == null)
            {
                this.extDataByProperty = this.MakeArrayCollection();

                this.containsPropertiesTrueGridCell = new List<Name>[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
                this.containsPropertiesFalseGridCell = new List<Name>[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
                this.loadedFromTemplate = false;
            }
            else
            {
                this.extDataByProperty = (Dictionary<Name, BitArray>)serializedData["extDataByProperty"];
                this.containsPropertiesTrueGridCell = (List<Name>[,])serializedData["containsPropertiesTrueGridCell"];
                this.containsPropertiesFalseGridCell = (List<Name>[,])serializedData["containsPropertiesFalseGridCell"];
                this.CreationInProgress = false;
                this.loadedFromTemplate = true;
            }
        }

        public void AttachToNewGrid(Grid grid)
        {
            this.Grid = grid;
        }

        private Dictionary<Name, BitArray> MakeArrayCollection()
        {
            var arrayCollection = new Dictionary<Name, BitArray>();

            foreach (Name extPropName in allExtPropNames)
            {
                arrayCollection[extPropName] = new BitArray(this.Grid.dividedWidth * this.Grid.dividedHeight);
            }

            return arrayCollection;
        }

        public void EndCreationAndSave()
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot end creation more than once.");

            foreach (Cell cell in this.Grid.allCells)
            {
                int xMinRaw = cell.xMin / this.Grid.resDivider;
                int xMaxRaw = cell.xMax / this.Grid.resDivider;
                int yMinRaw = cell.yMin / this.Grid.resDivider;
                int yMaxRaw = cell.yMax / this.Grid.resDivider;

                this.containsPropertiesTrueGridCell[cell.cellNoX, cell.cellNoY] = new List<Name>();
                this.containsPropertiesFalseGridCell[cell.cellNoX, cell.cellNoY] = new List<Name>();

                List<Name> containsPropertiesTrue = this.containsPropertiesTrueGridCell[cell.cellNoX, cell.cellNoY];
                List<Name> containsPropertiesFalse = this.containsPropertiesFalseGridCell[cell.cellNoX, cell.cellNoY];

                foreach (var kvp in this.extDataByProperty)
                {
                    Name name = kvp.Key;

                    for (int rawX = xMinRaw; rawX <= xMaxRaw; rawX++)
                    {
                        for (int rawY = yMinRaw; rawY <= yMaxRaw; rawY++)
                        {
                            bool value = kvp.Value.Get(ConvertRaw2DCoordinatesTo1D(rawX, rawY));
                            if (value && !containsPropertiesTrue.Contains(name)) containsPropertiesTrue.Add(name);
                            if (!value && !containsPropertiesFalse.Contains(name)) containsPropertiesFalse.Add(name);
                        }
                    }
                }
            }

            this.CreationInProgress = false;
            if (!this.loadedFromTemplate) this.SaveTemplate();
        }

        public bool CheckIfContainsPropertyForCell(Name name, bool value, int cellNoX, int cellNoY)
        {
            if (value) return this.containsPropertiesTrueGridCell[cellNoX, cellNoY].Contains(name);
            else return this.containsPropertiesFalseGridCell[cellNoX, cellNoY].Contains(name);
        }

        private int ConvertRaw2DCoordinatesTo1D(int x, int y)
        {
            return (y * this.Grid.dividedWidth) + x;
        }

        private int Convert2DCoordinatesTo1D(int x, int y)
        {
            return (y / this.Grid.resDivider * this.Grid.dividedWidth) + (x / this.Grid.resDivider);
        }

        public bool GetValue(Name name, int x, int y, bool xyRaw)
        {
            if (xyRaw) return this.extDataByProperty[name].Get(this.ConvertRaw2DCoordinatesTo1D(x, y));
            else return this.extDataByProperty[name].Get(this.Convert2DCoordinatesTo1D(x, y));
        }

        public Dictionary<Name, bool> GetValueDict(int x, int y, bool xyRaw)
        {
            var valueDict = new Dictionary<Name, bool>();

            foreach (Name name in this.extDataByProperty.Keys)
            {
                valueDict[name] = this.GetValue(name: name, x: x, y: y, xyRaw: xyRaw);
            }

            return valueDict;
        }

        public void SetValue(Name name, bool value, int x, int y, bool xyRaw)
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