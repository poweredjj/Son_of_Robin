using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class ExtBoardProperties
    {
        public enum ExtPropName
        { Sea, OuterBeach, OuterBeachEdge };

        private static readonly ExtPropName[] allExtPropNames = (ExtPropName[])Enum.GetValues(typeof(ExtPropName));

        public readonly Cell cell;

        private readonly Dictionary<ExtPropName, BitArray> extDataByProperty;
        private readonly List<ExtPropName> containsProperties;
        private readonly string templatePath;
        public bool CreationInProgress { get; private set; }

        public ExtBoardProperties(Cell cell)
        {
            this.cell = cell;

            this.CreationInProgress = true;
            this.templatePath = Path.Combine(this.cell.grid.gridTemplate.templatePath, $"properties_{cell.cellNoX}_{cell.cellNoY}.ext");

            var serializedData = this.LoadTemplate();
            if (serializedData == null)
            {
                this.extDataByProperty = this.MakeArrayCollection();
                this.containsProperties = new List<ExtPropName>();
            }
            else
            {
                this.extDataByProperty = (Dictionary<ExtPropName, BitArray>)serializedData["extDataByProperty"];
                this.containsProperties = (List<ExtPropName>)serializedData["containsProperties"];
                this.CreationInProgress = false;
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
                        if (kvp.Value.Get(Convert2DCoordinatesTo1D(x, y)) && !this.containsProperties.Contains(name)) this.containsProperties.Add(name);
                    }
                }
            }

            this.CreationInProgress = false;
            this.SaveTemplate();
        }

        public bool CheckIfContainsProperty(ExtPropName name)
        {
            return this.containsProperties.Contains(name);
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
                { "containsProperties", this.containsProperties },
            };

            return serializedData;
        }
    }
}