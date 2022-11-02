using System;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class ExtBoardProperties
    {
        public enum ExtPropName { Water, Sea, OuterBeach, OuterBeachEdge };
        private static readonly ExtPropName[] allExtPropNames = (ExtPropName[])Enum.GetValues(typeof(ExtPropName));

        public readonly Grid grid;
        public readonly int dividedWidth;
        public readonly int dividedHeight;

        private readonly Dictionary<ExtPropName, bool[,]> extDataByProperty;
        private readonly string templatePath;
        public bool CreationInProgress { get; private set; }

        public ExtBoardProperties(Grid grid)
        {
            this.grid = grid;
            this.dividedWidth = this.grid.world.width / this.grid.resDivider;
            this.dividedHeight = this.grid.world.height / this.grid.resDivider;

            this.CreationInProgress = true;
            this.templatePath = Path.Combine(this.grid.gridTemplate.templatePath, "properties.ext");

            this.extDataByProperty = this.LoadTemplate();
            if (this.extDataByProperty == null)
            {
                this.extDataByProperty = new Dictionary<ExtPropName, bool[,]>();

                foreach (ExtPropName extPropName in allExtPropNames)
                {
                    this.extDataByProperty[extPropName] = new bool[this.dividedWidth, this.dividedHeight];
                }
            }
            else this.CreationInProgress = false;
        }

        public void SetData(ExtPropName name, bool value, int x, int y)
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot call SetData() after ending creation process.");

            this.extDataByProperty[name][x / this.grid.resDivider, y / this.grid.resDivider] = value;
        }

        public void SetDataRaw(ExtPropName name, bool value, int x, int y)
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot call SetData() after ending creation process.");

            // direct access, without taking resDivider into account
            this.extDataByProperty[name][x, y] = value;
        }

        public void EndCreationAndSave()
        {
            this.CreationInProgress = false;
            this.SaveTemplate();
        }

        public bool GetData(ExtPropName name, int x, int y)
        {
            if (this.CreationInProgress) throw new ArgumentException("Cannot call GetData() before ending creation process.");

            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            return this.extDataByProperty[name][x / this.grid.resDivider, y / this.grid.resDivider];
        }

        public bool GetDataRaw(ExtPropName name, int x, int y)
        {
            if (this.CreationInProgress) throw new ArgumentException("Cannot call GetData() before ending creation process.");

            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            // direct access, without taking resDivider into account
            return this.extDataByProperty[name][x, y];
        }

        private Dictionary<ExtPropName, bool[,]> LoadTemplate()
        {
            var loadedData = FileReaderWriter.Load(this.templatePath);
            if (loadedData == null) return null;
            else return (Dictionary<ExtPropName, bool[,]>)loadedData;
        }

        public void SaveTemplate()
        {
            FileReaderWriter.Save(path: this.templatePath, savedObj: this.extDataByProperty);
        }
    }
}