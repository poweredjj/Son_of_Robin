using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class ExtBoardProps
    {
        public enum ExtPropName
        { Sea, OuterBeach, BiomeDangerous, BiomeSwamp }; // each biome name must start with "biome"

        private static readonly ExtPropName[] allExtPropNames = (ExtPropName[])Enum.GetValues(typeof(ExtPropName));
        public static readonly List<ExtPropName> allBiomes = allExtPropNames.Where(name => name.ToString().ToLower().StartsWith("biome")).ToList();

        public static readonly Dictionary<ExtPropName, Color> colorsForBiomes = new Dictionary<ExtPropName, Color>
        {
            { ExtPropName.BiomeDangerous, new Color((byte)40, (byte)0, (byte)0, (byte)100) },
            { ExtPropName.BiomeSwamp, new Color((byte)7, (byte)133, (byte)53, (byte)200) },
        };

        public readonly Cell cell;

        private readonly Dictionary<ExtPropName, BitArray> extDataByProperty;
        private readonly List<ExtPropName> containsPropertiesTrue;
        private readonly List<ExtPropName> containsPropertiesFalse;
        private readonly string templatePath;
        private readonly bool loadedFromTemplate; // to avoid saving template, after being loaded (needed because saving is not done inside constructor)
        public bool CreationInProgress { get; private set; }

        public ExtBoardProps(Cell cell)
        {
            this.cell = cell;

            this.CreationInProgress = true;
            this.templatePath = Path.Combine(this.cell.grid.gridTemplate.templatePath, $"properties_{cell.cellNoX}_{cell.cellNoY}.ext");

            var serializedData = this.LoadTemplate();
            if (serializedData == null)
            {
                this.extDataByProperty = this.MakeArrayCollection();
                this.containsPropertiesTrue = new List<ExtPropName>();
                this.containsPropertiesFalse = new List<ExtPropName>();
                this.loadedFromTemplate = false;
            }
            else
            {
                this.extDataByProperty = (Dictionary<ExtPropName, BitArray>)serializedData["extDataByProperty"];
                this.containsPropertiesTrue = (List<ExtPropName>)serializedData["containsPropertiesTrue"];
                this.containsPropertiesFalse = (List<ExtPropName>)serializedData["containsPropertiesFalse"];
                this.CreationInProgress = false;
                this.loadedFromTemplate = true;
            }
        }

        private Dictionary<ExtPropName, BitArray> MakeArrayCollection()
        {
            var arrayCollection = new Dictionary<ExtPropName, BitArray>();

            foreach (ExtPropName extPropName in allExtPropNames)
            {
                arrayCollection[extPropName] = new BitArray(this.cell.dividedWidth * this.cell.dividedHeight);
            }

            return arrayCollection;
        }

        public void EndCreationAndSave()
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot end creation more than once.");

            foreach (var kvp in this.extDataByProperty)
            {
                ExtPropName name = kvp.Key;

                for (int x = 0; x < this.cell.dividedWidth; x++)
                {
                    for (int y = 0; y < this.cell.dividedHeight; y++)
                    {
                        bool value = kvp.Value.Get(Convert2DCoordinatesTo1D(x, y));
                        if (value && !this.containsPropertiesTrue.Contains(name)) this.containsPropertiesTrue.Add(name);
                        if (!value && !this.containsPropertiesFalse.Contains(name)) this.containsPropertiesFalse.Add(name);
                    }
                }
            }

            this.CreationInProgress = false;
            if (!this.loadedFromTemplate) this.SaveTemplate();
        }

        public bool CheckIfContainsProperty(ExtPropName name, bool value)
        {
            if (value) return this.containsPropertiesTrue.Contains(name);
            else return this.containsPropertiesFalse.Contains(name);
        }

        private int ConvertRaw2DCoordinatesTo1D(int x, int y)
        {
            return (y * this.cell.dividedWidth) + x;
        }

        private int Convert2DCoordinatesTo1D(int x, int y)
        {
            return (y / this.cell.grid.resDivider * this.cell.dividedWidth) + (x / this.cell.grid.resDivider);
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
                { "containsPropertiesTrue", this.containsPropertiesTrue },
                { "containsPropertiesFalse", this.containsPropertiesFalse },
            };

            return serializedData;
        }
    }
}