using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class NamedLocations
    {
        public enum Category
        {
            Hills = 0,
            Lake = 1,
        }

        private static readonly Category[] allCategories = (Category[])Enum.GetValues(typeof(Category));

        private static readonly List<string> openingList = new List<string> { "Unending", "The grand", "Dawn's first", "Almost like a", "Ungodly", "Barren" };

        private static readonly Dictionary<Category, List<string>> adjectiveListByCategory = new Dictionary<Category, List<string>> {
            { Category.Hills, new List<string>{ "Windy", "Grand", "Patrick's", "Mysterious" } },
            { Category.Lake, new List<string>{ "Deep", "Shiny", "Golden", "Shallow", "Silent" } },
            };

        private static readonly Dictionary<Category, List<string>> nounListByCategory = new Dictionary<Category, List<string>> {
            { Category.Hills, new List<string>{ "Hills", "Mountain", "Plateau" } },
            { Category.Lake, new List<string>{ "Lake", "Pond", "Depths", "Waters" } },
            };

        public class Location
        {
            public readonly string name;
            public readonly Category category;
            public readonly Rectangle areaRect;
            public readonly Rectangle textRect;
            public bool hasBeenDiscovered;

            public Location(string name, Category category, Rectangle areaRect, bool hasBeenDiscovered = false)
            {
                this.name = name;
                this.category = category;
                this.areaRect = areaRect;
                this.textRect = areaRect;
                this.textRect.Inflate(-areaRect.Width / 6, -areaRect.Height / 6);
                this.hasBeenDiscovered = hasBeenDiscovered;
            }

            public Object Serialize()
            {
                var locationData = new Dictionary<string, object>
            {
                { "name", this.name },
                { "category", this.category },
                { "areaRect", this.areaRect },
                { "hasBeenDiscovered", this.hasBeenDiscovered },
            };

                return locationData;
            }

            public static Location Deserialize(Object locationData)
            {
                var locationDict = (Dictionary<string, object>)locationData;

                string name = (string)locationDict["name"];
                Category category = (Category)(Int64)locationDict["category"];
                Rectangle areaRect = (Rectangle)locationDict["areaRect"];
                bool hasBeenDiscovered = (bool)locationDict["hasBeenDiscovered"];

                return new(name: name, category: category, areaRect: areaRect, hasBeenDiscovered: hasBeenDiscovered);
            }
        }

        public readonly struct SearchCriteria
        {
            public readonly bool checkTerrain;
            public readonly Terrain.Name terrainName;
            public readonly byte terrainMinVal;
            public readonly byte terrainMaxVal;

            public readonly bool checkExpProps;
            public readonly ExtBoardProps.Name extPropsName;
            public readonly bool expPropsVal;

            public SearchCriteria(bool checkTerrain = false, bool checkExpProps = false, Terrain.Name terrainName = Terrain.Name.Height, byte terrainMinVal = 0, byte terrainMaxVal = 0, ExtBoardProps.Name extPropsName = ExtBoardProps.Name.Sea, bool expPropsVal = false)
            {
                if (!checkTerrain && !checkExpProps) throw new ArgumentException("Invalid search criteria.");

                this.checkTerrain = checkTerrain;
                this.terrainName = terrainName;
                this.terrainMinVal = terrainMinVal;
                this.terrainMaxVal = terrainMaxVal;

                this.checkExpProps = checkExpProps;
                this.extPropsName = extPropsName;
                this.expPropsVal = expPropsVal;
            }
        }

        private readonly Grid grid;
        private readonly List<Location> locationList;
        private Location currentLocation;
        private bool locationsCreated;
        private readonly Random random;
        public IEnumerable DiscoveredLocations { get { return this.locationList.Where(location => location.hasBeenDiscovered); } }

        public NamedLocations(Grid grid)
        {
            this.grid = grid;
            this.locationList = new List<Location>();
            this.currentLocation = null;
            this.locationsCreated = false;
            this.random = new(this.grid.world.seed);
        }

        public Location UpdateCurrentLocation(Vector2 playerPos)
        {
            foreach (Location location in this.locationList)
            {
                if (location.areaRect.Contains(playerPos))
                {
                    if (location == currentLocation) return null;
                    currentLocation = location;
                    return location;
                }
            }

            currentLocation = null;
            return currentLocation;
        }

        public Object Serialize()
        {
            var locationData = new List<object>();

            foreach (Location location in this.locationList)
            {
                locationData.Add(location.Serialize());
            }

            return locationData;
        }

        public void Deserialize(Object locationData)
        {
            this.locationList.Clear();

            foreach (object singleLocationData in (List<object>)locationData)
            {
                this.locationList.Add(Location.Deserialize(singleLocationData));
            }
            this.locationsCreated |= true;
        }

        public void CopyLocationsFromTemplate(NamedLocations templateLocations)
        {
            this.locationList.Clear();
            foreach (Location location in templateLocations.locationList)
            {
                location.hasBeenDiscovered = false;
                this.locationList.Add(location);
            }
        }

        public void GenerateLocations()
        {
            if (this.locationsCreated) return;

            foreach (Category category in allCategories)
            {
                this.CreateLocationsForCategory(category);
            }

            foreach (Location location in this.locationList)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"Location: {location.name} {location.areaRect}"); // for testing
                location.hasBeenDiscovered = true; // for testing
            }

            this.locationsCreated = true;
        }

        private void CreateLocationsForCategory(Category category)
        {
            SearchCriteria searchCriteria;
            int minCells;
            int maxCells;

            switch (category)
            {
                case Category.Hills:
                    searchCriteria = new(checkTerrain: true, checkExpProps: false, terrainName: Terrain.Name.Height, terrainMinVal: Terrain.rocksLevelMin, terrainMaxVal: 255, extPropsName: ExtBoardProps.Name.Sea, expPropsVal: false);
                    minCells = 10;
                    maxCells = 120;

                    break;

                case Category.Lake:
                    searchCriteria = new(checkTerrain: true, checkExpProps: true, terrainName: Terrain.Name.Height, terrainMinVal: 0, terrainMaxVal: Terrain.waterLevelMax, extPropsName: ExtBoardProps.Name.Sea, expPropsVal: false);
                    minCells = 10;
                    maxCells = 100;

                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            var cellCoordsByRegion = this.SplitCellBagIntoRegions(this.FindAllCellCoordsThatMeetCriteria(searchCriteria));

            int regionNo = 0;
            foreach (List<Point> coordsList in cellCoordsByRegion)
            {
                if (coordsList.Count < minCells || coordsList.Count > maxCells || this.random.Next(2) == 0) continue;

                regionNo++;
                this.locationList.Add(new Location(name: this.GetRegionName(category), category: category, areaRect: this.GetLocationRect(coordsList)));
            }
        }

        private string GetRegionName(Category category)
        {
            int maxTryCount = 15;

            for (int currentTry = 0; currentTry < maxTryCount; currentTry++)
            {
                string opening = this.random.Next(5) == 0 ? openingList[this.random.Next(openingList.Count)] + " " : "";
                string adjective = adjectiveListByCategory[category][this.random.Next(adjectiveListByCategory[category].Count)];
                string noun = nounListByCategory[category][this.random.Next(nounListByCategory[category].Count)];

                string regionName = $"{opening}{adjective} {noun}";

                bool nameAlreadyExists = false;
                foreach (Location location in this.locationList)
                {
                    if (location.name == regionName)
                    {
                        nameAlreadyExists = true;
                        break;
                    }
                }

                if (!nameAlreadyExists || currentTry == maxTryCount - 1) return regionName;
            }

            return "";
        }

        private ConcurrentBag<Point> FindAllCellCoordsThatMeetCriteria(SearchCriteria searchCriteria)
        {
            var cellCoordsBag = new ConcurrentBag<Point>();

            Parallel.ForEach(this.grid.allCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse / 2 }, cell =>
            {
                bool isWithinRange = (!searchCriteria.checkTerrain || this.CheckIfCellTerrainIsInRange(terrainName: searchCriteria.terrainName, terrainMinVal: searchCriteria.terrainMinVal, terrainMaxVal: searchCriteria.terrainMaxVal, cellNoX: cell.cellNoX, cellNoY: cell.cellNoY)) &&
                (!searchCriteria.checkExpProps || this.grid.CheckIfContainsExtPropertyForCell(name: searchCriteria.extPropsName, value: searchCriteria.expPropsVal, cellNoX: cell.cellNoX, cellNoY: cell.cellNoY));

                if (isWithinRange) cellCoordsBag.Add(new Point(cell.cellNoX, cell.cellNoY));
            });

            return cellCoordsBag;
        }

        private List<List<Point>> SplitCellBagIntoRegions(ConcurrentBag<Point> cellCoordsBag)
        {
            // preparing data

            List<Point> offsetList = new()
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1),
            };

            int noOfCellsX = this.grid.noOfCellsX;
            int noOfCellsY = this.grid.noOfCellsY;

            var gridCellExists = new bool[noOfCellsX, noOfCellsY];
            var gridCellIsProcessed = new bool[noOfCellsX, noOfCellsY];

            Parallel.ForEach(cellCoordsBag, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse / 2 }, cellCoords =>
            {
                gridCellExists[cellCoords.X, cellCoords.Y] = true;
            });

            Parallel.For(0, noOfCellsY, y =>
            {
                for (int x = 0; x < noOfCellsX; x++)
                {
                    gridCellIsProcessed[x, y] = !gridCellExists[x, y];
                }
            });

            int currentRegion = 0;
            var cellCoordsByRegion = new List<List<Point>>();
            var cellCoordsLeftToProcess = cellCoordsBag.Distinct().ToList();

            // filling regions

            while (true)
            {
                var nextCoords = new ConcurrentBag<Point> { cellCoordsLeftToProcess.First() };
                var thisRegionCoords = new ConcurrentBag<Point>();

                while (true)
                {
                    List<Point> currentCellCoords = nextCoords.Distinct().ToList(); // using Distinct() to filter out duplicates
                    nextCoords = new ConcurrentBag<Point>(); // because there is no Clear() for ConcurrentBag

                    Parallel.ForEach(currentCellCoords, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentCell =>
                    {
                        // array can be written to using parallel, if every thread accesses its own indices

                        if (gridCellExists[currentCell.X, currentCell.Y])
                        {
                            thisRegionCoords.Add(currentCell);
                            foreach (Point currentOffset in offsetList)
                            {
                                Point nextPoint = new(currentCell.X + currentOffset.X, currentCell.Y + currentOffset.Y);

                                if (nextPoint.X >= 0 && nextPoint.X < noOfCellsX &&
                                    nextPoint.Y >= 0 && nextPoint.Y < noOfCellsY &&
                                    !gridCellIsProcessed[nextPoint.X, nextPoint.Y])
                                {
                                    nextCoords.Add(nextPoint);
                                }
                            }
                        }

                        gridCellIsProcessed[currentCell.X, currentCell.Y] = true;
                    });

                    if (!nextCoords.Any()) break;
                }

                cellCoordsLeftToProcess = cellCoordsLeftToProcess.Where(coords => !thisRegionCoords.Contains(coords)).ToList();
                cellCoordsByRegion.Add(thisRegionCoords.Distinct().ToList());
                currentRegion++;

                if (!cellCoordsLeftToProcess.Any()) return cellCoordsByRegion;
            }
        }

        private Rectangle GetLocationRect(List<Point> coordsList)
        {
            int xMin = Int32.MaxValue;
            int xMax = 0;
            int yMin = Int32.MaxValue;
            int yMax = 0;

            foreach (Point point in coordsList)
            {
                Cell cell = this.grid.cellGrid[point.X, point.Y];
                Rectangle cellRect = cell.rect;
                xMin = Math.Min(xMin, cellRect.Left);
                xMax = Math.Max(xMax, cellRect.Right);
                yMin = Math.Min(yMin, cellRect.Top);
                yMax = Math.Max(yMax, cellRect.Bottom);
            }

            return new Rectangle(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);
        }

        private bool CheckIfCellTerrainIsInRange(Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, int cellNoX, int cellNoY)
        {
            return this.grid.GetMinValueForCell(terrainName: terrainName, cellNoX: cellNoX, cellNoY: cellNoY) >= terrainMinVal &&
                   this.grid.GetMaxValueForCell(terrainName: terrainName, cellNoX: cellNoX, cellNoY: cellNoY) <= terrainMaxVal;
        }
    }
}