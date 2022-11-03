using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class ExtBoardProperties
    {
        public enum ExtPropName
        { Sea, OuterBeach, OuterBeachEdge };

        public readonly Cell cell;

        private readonly List<ExtPropName>[,] extPropListGrid;
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
                this.extPropListGrid = this.MakeEmptyGrid();
                this.containsProperties = new List<ExtPropName>();
            }
            else
            {
                this.extPropListGrid = (List<ExtPropName>[,])serializedData["extPropListGrid"];
                this.containsProperties = (List<ExtPropName>)serializedData["containsProperties"];
                this.CreationInProgress = false;
            }
        }

        private List<ExtPropName>[,] MakeEmptyGrid()
        {
            List<ExtPropName>[,] emptyGrid = new List<ExtPropName>[this.cell.dividedWidth, this.cell.dividedHeight];
            for (int x = 0; x < this.cell.dividedWidth; x++)
            {
                for (int y = 0; y < this.cell.dividedHeight; y++)
                {
                    emptyGrid[x, y] = new List<ExtPropName>();
                }
            }

            return emptyGrid;
        }
        public void EndCreationAndSave()
        {
            for (int x = 0; x < this.cell.dividedWidth; x++)
            {
                for (int y = 0; y < this.cell.dividedHeight; y++)
                {
                    foreach (ExtPropName name in this.extPropListGrid[x, y])
                    {
                        if (!this.containsProperties.Contains(name)) this.containsProperties.Add(name);
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

        private List<ExtPropName> GetList(int x, int y, bool xyRaw)
        {
            return xyRaw ? this.extPropListGrid[x, y] : this.extPropListGrid[x / this.cell.grid.resDivider, y / this.cell.grid.resDivider];
        }

        public void AddProperty(ExtPropName name, int x, int y, bool xyRaw)
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot modify data after ending creation process.");

            List<ExtPropName> list = this.GetList(x: x, y: y, xyRaw: xyRaw);
            if (!list.Contains(name)) list.Add(name);
        }

        public void RemoveProperty(ExtPropName name, int x, int y, bool xyRaw)
        {
            if (!this.CreationInProgress) throw new ArgumentException("Cannot modify data after ending creation process.");

            List<ExtPropName> list = this.GetList(x: x, y: y, xyRaw: xyRaw);
            if (list.Contains(name)) list.Remove(name);
        }

        public bool CheckIfHasProperty(ExtPropName name, int x, int y, bool xyRaw)
        {
            return this.GetList(x: x, y: y, xyRaw: xyRaw).Contains(name);
        }

        public List<ExtPropName> GetAllProperties(int x, int y, bool xyRaw)
        {
            return this.GetList(x: x, y: y, xyRaw: xyRaw).ToList();
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
                { "extPropListGrid", this.extPropListGrid },
                { "containsProperties", this.containsProperties },
            };

            return serializedData;
        }
    }
}