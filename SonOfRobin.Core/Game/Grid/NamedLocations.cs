using Microsoft.Xna.Framework;
using System;
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

        private static readonly List<string> openingList = new List<string> { "Unending", "Grand", "Barren", "Solitary", "Echoing", "Whispering" };

        private static readonly Dictionary<Category, List<string>> adjectiveListByCategory = new Dictionary<Category, List<string>> {
            { Category.Hills, new List<string>{ "Windy", "Grand", "Patrick's", "Mysterious", "Lover's", "Windswept", "Majestic", "Enchanted", "Enigmatic", "Serene", "Tranquil", "Misty", "Mystical", "Verdant", "Timeless" } },
            { Category.Lake, new List<string>{ "Deep", "Shiny", "Golden", "Shallow", "Silent", "Tranquil", "Reflective", "Serene", "Crystaline", "Secretive", "Ethereal", "Undisturbed", "Luminous", "Whispering", "Magicians" } },
            };

        private static readonly Dictionary<Category, List<string>> nounListByCategory = new Dictionary<Category, List<string>> {
            { Category.Hills, new List<string>{ "Hills", "Mountain", "Plateau", "Highlands", "Summit", "Ridge", "Crest", "Uplands", "Slopes", "Overlook", "Mesa", "Peaks" } },
            { Category.Lake, new List<string>{ "Lake", "Pond", "Depths", "Waters", "Lagoon", "Cove", "Mirage", "Abyss", "Expanse", "Mirage", "Mirage", "Reservoir", "Mirage" } },
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
            public readonly bool terrainStrictSearch;
            public readonly Terrain.Name terrainName;
            public readonly byte terrainMinVal;
            public readonly byte terrainMaxVal;

            public readonly bool checkExpProps;
            public readonly bool expPropsStrictSearch;
            public readonly ExtBoardProps.Name extPropsName;
            public readonly bool expPropsVal;

            public SearchCriteria(bool checkTerrain = false, bool terrainStrictSearch = false, Terrain.Name terrainName = Terrain.Name.Height, byte terrainMinVal = 0, byte terrainMaxVal = 255, bool checkExpProps = false, bool expPropsStrictSearch = false, ExtBoardProps.Name extPropsName = ExtBoardProps.Name.Sea, bool expPropsVal = true)
            {
                if (!checkTerrain && !checkExpProps) throw new ArgumentException("Invalid search criteria.");

                this.checkTerrain = checkTerrain;
                this.terrainStrictSearch = terrainStrictSearch;
                this.terrainName = terrainName;
                this.terrainMinVal = terrainMinVal;
                this.terrainMaxVal = terrainMaxVal;

                this.checkExpProps = checkExpProps;
                this.expPropsStrictSearch = expPropsStrictSearch;
                this.extPropsName = extPropsName;
                this.expPropsVal = expPropsVal;
            }
        }

        private readonly Grid grid;
        private readonly List<Location> locationList;
        private Location currentLocation;
        private bool locationsCreated;
        private readonly Random random;
        public List<Location> DiscoveredLocations { get { return this.locationList.Where(location => location.hasBeenDiscovered).ToList(); } }
        public int DiscoveredLocationsCount { get { return this.DiscoveredLocations.Count; } }
        public int AllLocationsCount { get { return this.locationList.Count; } }

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

        public void SetAllLocationsAsDiscovered()
        {
            foreach (Location location in this.locationList)
            {
                location.hasBeenDiscovered = true;
            }
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
                // location.hasBeenDiscovered = true; // for testing
            }

            this.locationsCreated = true;
        }

        private void CreateLocationsForCategory(Category category)
        {
            SearchCriteria searchCriteria;
            int minCells, maxCells, density; // density: low number == high density, high number == low density

            switch (category)
            {
                case Category.Hills:
                    searchCriteria = new(
                        checkTerrain: true,
                        terrainName: Terrain.Name.Height,
                        terrainMinVal: Terrain.rocksLevelMin + 15,
                        terrainMaxVal: 255
                        );

                    minCells = 10;
                    maxCells = 100;
                    density = 2; // 2

                    break;

                case Category.Lake:
                    searchCriteria = new(
                        checkTerrain: true,
                        terrainName: Terrain.Name.Height,
                        terrainMinVal: 0,
                        terrainMaxVal: Terrain.waterLevelMax,
                        checkExpProps: true,
                        expPropsStrictSearch: true,
                        extPropsName: ExtBoardProps.Name.Sea,
                        expPropsVal: false
                        );

                    minCells = 10;
                    maxCells = 80;
                    density = 1;

                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            var cellCoordsByRegion = this.SplitCellBagIntoRegions(this.FindAllCellCoordsThatMeetCriteria(searchCriteria));

            int regionNo = 0;
            foreach (List<Point> coordsList in cellCoordsByRegion)
            {
                if (coordsList.Count < minCells || coordsList.Count > maxCells || this.random.Next(density) != 0) continue;

                Rectangle areaRect = this.GetLocationRect(coordsList);

                bool collidesWithAnotherLocation = false;
                foreach (Location location in this.locationList)
                {
                    if (location.areaRect.Intersects(areaRect))
                    {
                        collidesWithAnotherLocation = true;
                        break;
                    }
                }
                if (collidesWithAnotherLocation) continue;

                regionNo++;
                this.locationList.Add(new Location(name: this.GetRegionName(category), category: category, areaRect: areaRect));
            }
        }

        private string GetRegionName(Category category)
        {
            int maxTryCount = 15;

            for (int currentTry = 0; currentTry < maxTryCount; currentTry++)
            {
                string opening = this.random.Next(8) == 0 ? openingList[this.random.Next(openingList.Count)] + " " : "";
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
                bool isWithinRange = true;

                if (isWithinRange && searchCriteria.checkTerrain) isWithinRange = this.CheckIfCellTerrainIsInRange(searchCriteria: searchCriteria, cellNoX: cell.cellNoX, cellNoY: cell.cellNoY);

                if (isWithinRange && searchCriteria.checkExpProps)
                {
                    isWithinRange = this.grid.CheckIfContainsExtPropertyForCell(name: searchCriteria.extPropsName, value: searchCriteria.expPropsVal, cellNoX: cell.cellNoX, cellNoY: cell.cellNoY);

                    if (isWithinRange && searchCriteria.expPropsStrictSearch)
                    {
                        // strict search - making sure that cell does not contain other value anywhere
                        isWithinRange = !this.grid.CheckIfContainsExtPropertyForCell(name: searchCriteria.extPropsName, value: !searchCriteria.expPropsVal, cellNoX: cell.cellNoX, cellNoY: cell.cellNoY);
                    }
                }

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
                Point cellCenter = this.grid.cellGrid[point.X, point.Y].rect.Center; // center is used to avoid using extreme edges, which are not accurate

                xMin = Math.Min(xMin, cellCenter.X);
                xMax = Math.Max(xMax, cellCenter.X);
                yMin = Math.Min(yMin, cellCenter.Y);
                yMax = Math.Max(yMax, cellCenter.Y);
            }

            return new Rectangle(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);
        }

        private bool CheckIfCellTerrainIsInRange(SearchCriteria searchCriteria, int cellNoX, int cellNoY)
        {
            // strictSearch: true == whole cell must match search criteria, false == any cell part must match search criteria

            return searchCriteria.terrainStrictSearch ?
                this.grid.GetMinValueForCell(terrainName: searchCriteria.terrainName, cellNoX: cellNoX, cellNoY: cellNoY) >= searchCriteria.terrainMinVal &&
                this.grid.GetMaxValueForCell(terrainName: searchCriteria.terrainName, cellNoX: cellNoX, cellNoY: cellNoY) <= searchCriteria.terrainMaxVal :

                this.grid.GetMaxValueForCell(terrainName: searchCriteria.terrainName, cellNoX: cellNoX, cellNoY: cellNoY) >= searchCriteria.terrainMinVal &&
                this.grid.GetMinValueForCell(terrainName: searchCriteria.terrainName, cellNoX: cellNoX, cellNoY: cellNoY) <= searchCriteria.terrainMaxVal;
        }
    }
}