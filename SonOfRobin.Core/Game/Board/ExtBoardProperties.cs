using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class ExtBoardProperties
    {
        public enum ExtPropName { Water, Sea, OuterBeach, OuterBeachEdge };
        private static readonly ExtPropName[] allExtPropNames = (ExtPropName[])Enum.GetValues(typeof(ExtPropName));

        public readonly Cell cell;
        private readonly Dictionary<ExtPropName, bool[,]> extDataByProperty;
        private readonly string templatePath;
        public bool CreationInProgress { get; private set; }

        public ExtBoardProperties(Cell cell)
        {
            this.cell = cell;
            this.CreationInProgress = true;
            this.templatePath = Path.Combine(this.cell.grid.gridTemplate.templatePath, $"properties_{cell.cellNoX}_{cell.cellNoY}.ext");

            this.extDataByProperty = this.LoadTemplate();
            if (this.extDataByProperty == null)
            {
                this.extDataByProperty = new Dictionary<ExtPropName, bool[,]>();

                foreach (ExtPropName extPropName in allExtPropNames)
                {
                    this.extDataByProperty[extPropName] = new bool[this.cell.dividedWidth, this.cell.dividedHeight];
                }
            }
            else this.CreationInProgress = false;

            this.EndCreationAndSave(); // for testing
        }

        public void SetData(ExtPropName name, bool value, int x, int y)
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot call SetData() after ending creation process.");

            this.extDataByProperty[name][x / this.cell.grid.resDivider, y / this.cell.grid.resDivider] = value;
        }

        public void SetDataRaw(ExtPropName name, bool value, int x, int y)
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot call SetData() after ending creation process.");

            this.extDataByProperty[name][x, y] = value;
        }

        public void EndCreationAndSave()
        {
            this.CreationInProgress = false;
            this.SaveTemplate();
        }

        public bool GetData(ExtPropName name, int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            return this.extDataByProperty[name][x / this.cell.grid.resDivider, y / this.cell.grid.resDivider];
        }

        public bool GetDataRaw(ExtPropName name, int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            // direct access, without taking resDivider into account
            return this.extDataByProperty[name][x, y];
        }

        private Dictionary<ExtPropName, bool[,]> LoadTemplate()
        {
            var loadedData = FileReaderWriter.Load(this.templatePath);
            if (loadedData == null) return null;

            return (Dictionary<ExtPropName, bool[,]>)loadedData;
        }

        public void SaveTemplate()
        {
            FileReaderWriter.Save(path: this.templatePath, savedObj: this.extDataByProperty);
        }
    }
}