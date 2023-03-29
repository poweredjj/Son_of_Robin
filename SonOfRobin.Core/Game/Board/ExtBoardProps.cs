using BigGustave;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private readonly string templateFolder;
        private readonly Dictionary<Name, string> pngPathByName;

        private readonly bool loadedFromTemplate; // to avoid saving template, after being loaded (needed because saving is not done inside constructor)

        private readonly List<Name>[,] containsPropertiesTrueGridCell; // this values are stored in terrain, instead of cell
        private readonly List<Name>[,] containsPropertiesFalseGridCell; // this values are stored in terrain, instead of cell

        public bool CreationInProgress { get; private set; }

        public ExtBoardProps(Grid grid)
        {
            this.Grid = grid;

            this.CreationInProgress = true;
            this.templateFolder = this.Grid.gridTemplate.templatePath;

            this.pngPathByName = new Dictionary<Name, string>();

            foreach (Name name in allExtPropNames)
            {
                this.pngPathByName[name] = Path.Combine(this.templateFolder, $"ext_bitmap_{name}.png");
            }

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
            this.SaveTemplate();
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
            var loadedDict = new Dictionary<string, object>();
            Dictionary<Name, BitArray> extDataByProperty = new Dictionary<Name, BitArray>();

            foreach (Name name in allExtPropNames)
            {
                BitArray bitArray = GfxConverter.LoadPNGAsBitArray(pngPathByName[name]);
                if (bitArray == null) return null;

                extDataByProperty[name] = bitArray;
            }

            loadedDict["extDataByProperty"] = extDataByProperty;

            var containsTrue = this.LoadNameListFromBitmaps(true);
            if (containsTrue == null) return null;
            else loadedDict["containsPropertiesTrueGridCell"] = containsTrue;

            var containsFalse = this.LoadNameListFromBitmaps(false);
            if (containsFalse == null) return null;
            else loadedDict["containsPropertiesFalseGridCell"] = containsFalse;

            return loadedDict;
        }

        public void SaveTemplate()
        {
            if (this.loadedFromTemplate) return;

            foreach (var kvp in this.extDataByProperty)
            {
                Name name = kvp.Key;
                BitArray bitArray = kvp.Value;
                GfxConverter.SaveBitArrayToPng(width: this.Grid.dividedWidth, height: this.Grid.dividedHeight, bitArray: bitArray, path: this.pngPathByName[name]);
            }

            this.SaveNameListAsBitmaps(true);
            this.SaveNameListAsBitmaps(false);
        }

        private void SaveNameListAsBitmaps(bool contains)
        {
            var containsPropertiesGridCell = contains ? this.containsPropertiesTrueGridCell : this.containsPropertiesFalseGridCell;
            string filenamePrefix = $"ext_contains_{contains}";

            int width = this.Grid.noOfCellsX;
            int height = this.Grid.noOfCellsY;

            foreach (Name name in allExtPropNames)
            {
                PngBuilder builder = PngBuilder.Create(width: width, height: height, hasAlphaChannel: false);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color pixel = containsPropertiesGridCell[x, y].Contains(name) ? Color.Black : Color.White;
                        builder.SetPixel(pixel.R, pixel.G, pixel.B, x, y);
                    }
                }

                using (var memoryStream = new MemoryStream())
                {
                    builder.Save(memoryStream);
                    FileReaderWriter.SaveMemoryStream(memoryStream: memoryStream, path: Path.Combine(this.templateFolder, $"{filenamePrefix}_{name}.png"));
                }
            }
        }

        private List<Name>[,] LoadNameListFromBitmaps(bool contains)
        {
            string filenamePrefix = $"ext_contains_{contains}";

            int width = this.Grid.noOfCellsX;
            int height = this.Grid.noOfCellsY;

            var containsPropertiesGridCell = new List<Name>[this.Grid.noOfCellsX, this.Grid.noOfCellsY];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    containsPropertiesGridCell[x, y] = new List<Name>();
                }
            }

            foreach (Name name in allExtPropNames)
            {
                string path = Path.Combine(this.templateFolder, $"{filenamePrefix}_{name}.png");
                Texture2D texture = GfxConverter.LoadTextureFromPNG(path);
                if (texture == null) return null;

                var colorArray1D = new Color[width * height];
                texture.GetData(colorArray1D);

                for (int y = 0; y < height; y++)
                {
                    int yFactor = y * width;

                    for (int x = 0; x < width; x++)
                    {
                        Color pixel = colorArray1D[yFactor + x];
                        if (pixel == Color.Black && !containsPropertiesGridCell[x, y].Contains(name)) containsPropertiesGridCell[x, y].Add(name);
                    }
                }
            }

            return containsPropertiesGridCell;
        }
    }
}