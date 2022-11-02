using System;
using System.IO;

namespace SonOfRobin
{
    public class BoardPropertiesArray
    {
        [Serializable]
        public struct ExtProperties
        {
            public readonly bool water;
            public readonly bool sea;
            public readonly bool outerBeach;
            public readonly bool outerBeachEdge;

            public ExtProperties(bool water, bool sea, bool outerBeach, bool outerBeachEdge)
            {
                this.water = water;
                this.sea = sea;
                this.outerBeach = outerBeach;
                this.outerBeachEdge = outerBeachEdge;
            }
        }

        public readonly World world;
        public readonly Cell cell;
        private readonly ExtProperties[,] extData;
        private readonly string templatePath;
        private bool creationInProgress;

        public BoardPropertiesArray(World world, Cell cell)
        {
            this.world = world;
            this.cell = cell;
            this.creationInProgress = true;
            this.templatePath = Path.Combine(this.world.grid.gridTemplate.templatePath, $"extData_{cell.cellNoX}_{cell.cellNoY}.map");

            this.extData = this.LoadTemplate();
            if (this.extData == null) this.extData = new ExtProperties[this.cell.dividedWidth, this.cell.dividedHeight];
            else this.creationInProgress = false;
        }

        public ExtProperties GetData(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            return extData[x / this.cell.grid.resDivider, y / this.cell.grid.resDivider];
        }

        public ExtProperties GetDataRaw(int x, int y)
        {
            if (x < 0) throw new IndexOutOfRangeException($"X {x} cannot be less than 0.");
            if (y < 0) throw new IndexOutOfRangeException($"Y {y} cannot be less than 0.");

            // direct access, without taking resDivider into account
            return extData[x, y];
        }

        private ExtProperties[,] LoadTemplate()
        {
            var loadedData = FileReaderWriter.Load(this.templatePath);
            if (loadedData == null) return null;

            return (ExtProperties[,])loadedData;
        }

        public void SaveTemplate()
        {
            FileReaderWriter.Save(path: this.templatePath, savedObj: this.extData);
        }



    }
}
