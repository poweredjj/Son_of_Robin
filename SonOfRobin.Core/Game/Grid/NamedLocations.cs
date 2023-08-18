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
            Volcano = 2,
            Swamp = 3,
            Desert = 4,
        }

        public class NameRandomizer
        {
            // keeps count of randomly chosen words and tries to balance them

            private readonly Dictionary<string, int> nameCounters;
            private readonly Random random;

            public NameRandomizer(Random random)
            {
                this.random = random;
                this.nameCounters = new Dictionary<string, int>();
            }

            public string RandomizeFromList(List<string> nameList)
            {
                foreach (string name in nameList)
                {
                    if (!this.nameCounters.ContainsKey(name)) this.nameCounters[name] = 0;
                }

                var filteredNameList = this.nameCounters.Where(kv => nameList.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

                int minCounterVal = filteredNameList.Values.Min();
                var stringsWithLowestCounterVal = filteredNameList.Where(kv => kv.Value == minCounterVal).Select(kv => kv.Key).ToList();

                return stringsWithLowestCounterVal[this.random.Next(stringsWithLowestCounterVal.Count)];
            }

            public void IncreaseCounterForVal(string name)
            {
                this.nameCounters[name]++;
            }
        }

        private static readonly Category[] allCategories = (Category[])Enum.GetValues(typeof(Category));

        private static readonly Dictionary<Category, List<string>> adjectiveListByCategory = new()
        {
            { Category.Hills, new List<string>{ "Windy", "Grand", "Patrick's", "Mysterious", "Windswept", "Majestic", "Enchanted", "Enigmatic", "Misty", "Mystical", "Verdant", "Timeless", "Twilight", "Celestial", "Solstice", "Seraph's", "Starlight", "Cascade", "Moonshadow", "Sunfire", "Eldertree", "Thunder", "Astral", "Crystal", "Obsidian", "Nova", "Starfall", "Pauline's", "Horizon Hues", "Caribbean", "Sunburst", "Sunkissed", "Azure", "Meridian" } },

            { Category.Lake, new List<string>{ "Deep", "Shiny", "Golden", "Lover's", "Shallow", "Silent", "Tranquil", "Reflective", "Serene", "Crystaline", "Secretive", "Ethereal", "Undisturbed", "Luminous", "Whispering", "Magician's", "Emerald Mirror", "Turquoise", "Paradise", "Palm Breeze", "Coconut", "Calypso", "Coral", "Mariner's", "Mango", "Siren's", "Flamingo", "Monkey", "Mermaid's" } },

            { Category.Volcano, new List<string>{ "Fiery", "Lava", "Infernal", "Flaming", "Smoldering", "Molten", "Blazing", "Hellish" } },

            { Category.Swamp, new List<string>{ "Muddy", "Mangrove", "Soggy", "Mosquito", "Murky", "Fogbound", "Murmuring", "Trecherous", "Drifting", "Whispering", "Venomous", "Sinking", "Withering", "Malarial", "Forsaken", "Foggy", "Cursed", "Ghostly", "Misty", "Dying", "Desolate", "Gloomveil", "Sorrowful", "Lamenting", "Melancholy", "Wilted", "Hopeless", "Anguished", "Fading", "Black" } },

            { Category.Desert, new List<string>{ "Scorched" } },
            };

        private static readonly Dictionary<Category, List<string>> nounListByCategory = new()
        {
            { Category.Hills, new List<string>{ "Hills", "Mountain", "Plateau", "Highlands", "Summit", "Ridge", "Crest", "Uplands", "Slopes", "Overlook", "Mesa", "Peaks", "Spire", "Peak", "Cliffs" } },

            { Category.Lake, new List<string>{ "Lake", "Pond", "Depths", "Waters", "Lagoon", "Cove", "Mirage", "Abyss", "Expanse", "Reservoir", "Basin" } },

            { Category.Volcano, new List<string>{ "Volcano", "Crater", "Caldera", "Peak" } },

            { Category.Swamp, new List<string>{ "Swamp", "Mire", "Quagmire", "Boglands", "Marsh", "Gloommarsh", "Morass", "Bog", "Waters", "Fenlands", "Damplands", "Fen" } },

            { Category.Desert, new List<string>{ "Desert" } },
            };

        public class Location
        {
            public readonly Grid grid;
            public readonly string name;
            public readonly Category category;
            public readonly List<Point> coordsList;
            public readonly Rectangle areaRect;
            public readonly Rectangle textRect;
            public readonly List<Cell> cells;
            public bool hasBeenDiscovered;

            public Location(Grid grid, string name, Category category, List<Point> coordsList, bool hasBeenDiscovered = false)
            {
                this.grid = grid;
                this.name = name;
                this.category = category;
                this.coordsList = coordsList;
                this.areaRect = this.GetAreaRect(this.coordsList);
                this.cells = this.GetCells(this.coordsList);
                this.textRect = areaRect;
                this.textRect.Inflate(-areaRect.Width / 6, -areaRect.Height / 6);
                this.hasBeenDiscovered = hasBeenDiscovered;
            }

            public bool IsPointInsideLocation(Point point)
            {
                if (!this.areaRect.Contains(point)) return false;

                foreach (Cell cell in this.cells)
                {
                    if (cell.rect.Contains(point)) return true;
                }

                return false;
            }

            public void DrawCellRects(Color color)
            {
                foreach (Cell cell in this.cells)
                {
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, cell.rect, color);
                }
            }

            private List<Cell> GetCells(List<Point> coordsList)
            {
                return coordsList.Select(c => this.grid.cellGrid[c.X, c.Y]).ToList();
            }

            private Rectangle GetAreaRect(List<Point> coordsList)
            {
                int xMin = Int32.MaxValue;
                int xMax = 0;
                int yMin = Int32.MaxValue;
                int yMax = 0;

                foreach (Point point in coordsList)
                {
                    Rectangle cellRect = this.grid.cellGrid[point.X, point.Y].rect;

                    xMin = Math.Min(xMin, cellRect.Left);
                    xMax = Math.Max(xMax, cellRect.Right);
                    yMin = Math.Min(yMin, cellRect.Top);
                    yMax = Math.Max(yMax, cellRect.Bottom);
                }

                return new(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);
            }

            public Object Serialize()
            {
                List<int> coordsListX = this.coordsList.Select(point => point.X).ToList();
                List<int> coordsListY = this.coordsList.Select(point => point.Y).ToList();

                var locationData = new Dictionary<string, object>

                {
                    { "name", this.name },
                    { "category", this.category },
                    { "coordsListX", coordsListX }, // directly serializing List<Point> would bloat save file
                    { "coordsListY", coordsListY }, // directly serializing List<Point> would bloat save file
                    { "hasBeenDiscovered", this.hasBeenDiscovered },
                };

                return locationData;
            }

            public static Location Deserialize(Grid grid, Object locationData)
            {
                var locationDict = (Dictionary<string, object>)locationData;

                string name = (string)locationDict["name"];
                Category category = (Category)(Int64)locationDict["category"];
                bool hasBeenDiscovered = (bool)locationDict["hasBeenDiscovered"];

                List<int> coordsListX = (List<int>)locationDict["coordsListX"];
                List<int> coordsListY = (List<int>)locationDict["coordsListY"];
                List<Point> coordsList = coordsListX.Zip(coordsListY, (x, y) => new Point(x, y)).ToList();

                return new(grid: grid, name: name, category: category, coordsList: coordsList, hasBeenDiscovered: hasBeenDiscovered);
            }
        }

        public readonly struct SearchEntryTerrain
        {
            public readonly Terrain.Name name;
            public readonly byte minVal;
            public readonly byte maxVal;
            public readonly bool strictSearch;

            public SearchEntryTerrain(Terrain.Name name, byte minVal, byte maxVal, bool strictSearch = false)
            {
                this.name = name;
                this.minVal = minVal;
                this.maxVal = maxVal;
                this.strictSearch = strictSearch;
            }
        }

        public readonly struct SearchEntryExtProps
        {
            public readonly bool strictSearch;
            public readonly ExtBoardProps.Name name;
            public readonly bool value;

            public SearchEntryExtProps(ExtBoardProps.Name name, bool value, bool strictSearch = false)
            {
                this.name = name;
                this.value = value;
                this.strictSearch = strictSearch;
            }
        }

        public readonly struct CellSearch
        {
            public readonly List<SearchEntryTerrain> searchEntriesTerrain;
            public readonly List<SearchEntryExtProps> searchEntriesExtProps;

            public CellSearch(List<SearchEntryTerrain> searchEntriesTerrain = null, List<SearchEntryExtProps> searchEntriesExtProps = null)
            {
                if (searchEntriesTerrain == null) searchEntriesTerrain = new List<SearchEntryTerrain>();
                if (searchEntriesExtProps == null) searchEntriesExtProps = new List<SearchEntryExtProps>();

                if (!searchEntriesTerrain.Any() && !searchEntriesExtProps.Any()) throw new ArgumentException("Invalid search criteria.");

                this.searchEntriesTerrain = searchEntriesTerrain;
                this.searchEntriesExtProps = searchEntriesExtProps;
            }

            public bool CellMeetsCriteria(Cell cell)
            {
                Grid grid = cell.grid;
                int cellNoX = cell.cellNoX;
                int cellNoY = cell.cellNoY;

                foreach (SearchEntryTerrain searchEntryTerrain in this.searchEntriesTerrain)
                {
                    if (!TerrainIsInRange(grid: grid, cellNoX: cellNoX, cellNoY: cellNoY, terrainName: searchEntryTerrain.name, minVal: searchEntryTerrain.minVal, maxVal: searchEntryTerrain.maxVal, strictSearch: searchEntryTerrain.strictSearch)) return false;
                }

                foreach (SearchEntryExtProps searchEntryExtProps in this.searchEntriesExtProps)
                {
                    if (!grid.CheckIfContainsExtPropertyForCell(name: searchEntryExtProps.name, value: searchEntryExtProps.value, cellNoX: cellNoX, cellNoY: cellNoY)) return false;
                    if (searchEntryExtProps.strictSearch && grid.CheckIfContainsExtPropertyForCell(name: searchEntryExtProps.name, value: !searchEntryExtProps.value, cellNoX: cellNoX, cellNoY: cellNoY)) return false;
                }

                return true;
            }

            private static bool TerrainIsInRange(Grid grid, int cellNoX, int cellNoY, Terrain.Name terrainName, byte minVal, byte maxVal, bool strictSearch)
            {
                // strictSearch: true == whole cell must match search criteria, false == any cell part must match search criteria

                byte minCellVal = grid.GetMinValueForCell(terrainName: terrainName, cellNoX: cellNoX, cellNoY: cellNoY);
                byte maxCellVal = grid.GetMaxValueForCell(terrainName: terrainName, cellNoX: cellNoX, cellNoY: cellNoY);

                return strictSearch ?
                    minCellVal >= minVal && maxCellVal <= maxVal :
                    maxCellVal >= minVal && minCellVal <= maxVal;
            }
        }

        private readonly Grid grid;
        private readonly List<Location> locationList;
        private Location currentLocation;
        private bool locationsCreated;
        private readonly Random random;
        private NameRandomizer nameRandomizer;
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
            this.nameRandomizer = new(random: this.random);
        }

        public Location UpdateCurrentLocation(Vector2 playerPos)
        {
            foreach (Location location in this.locationList)
            {
                if (location.IsPointInsideLocation(new Point((int)playerPos.X, (int)playerPos.Y)))
                {
                    if (location == this.currentLocation) return null;
                    this.currentLocation = location;
                    return location;
                }
            }

            this.currentLocation = null;
            return this.currentLocation;
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
                this.locationList.Add(Location.Deserialize(grid: this.grid, locationData: singleLocationData));
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

            var testCategories = new List<Category> { Category.Desert };

            foreach (Category category in allCategories) // allCategories
            {
                this.CreateLocationsForCategory(category);
            }

            foreach (Location location in this.locationList)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"Location: {location.name} {location.areaRect}"); // for testing
                location.hasBeenDiscovered = true; // for testing
            }

            this.locationsCreated = true;
            this.nameRandomizer = null;
        }

        private void CreateLocationsForCategory(Category category)
        {
            CellSearch cellSearch;
            int minCells, maxCells, density; // density: low number == high density, high number == low density

            switch (category)
            {
                case Category.Hills:

                    cellSearch = new CellSearch(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.rocksLevelMin + 15, maxVal: 255),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin),
                        }
                        );

                    minCells = 15;
                    maxCells = 200;
                    density = 1; // 2

                    break;

                case Category.Lake:

                    cellSearch = new CellSearch(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: Terrain.waterLevelMax),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin),},
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.Sea, value: false, strictSearch: true) }
                        );

                    minCells = 20;
                    maxCells = 500;
                    density = 1;

                    break;

                case Category.Volcano:

                    cellSearch = new CellSearch(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin + 1, maxVal: 255) }
                        );

                    minCells = 1;
                    maxCells = 150;
                    density = 1;

                    break;

                case Category.Swamp:

                    cellSearch = new CellSearch(
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true) }
                        );

                    minCells = 20;
                    maxCells = 400;
                    density = 1;

                    break;

                case Category.Desert:

                    cellSearch = new(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin),
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: Terrain.rocksLevelMin - 1),
                        },
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.Sea, value: false, strictSearch: true) }
                        );

                    minCells = 10;
                    maxCells = 400;
                    density = 1;

                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            var cellCoordsByRegion = this.SplitCellBagIntoRegions(this.FindAllCellCoordsThatMeetCriteria(cellSearch));

            foreach (List<Point> coordsList in cellCoordsByRegion)
            {
                if (coordsList.Count < minCells || coordsList.Count > maxCells || this.random.Next(density) != 0) continue;

                this.locationList.Add(new(grid: this.grid, name: this.GetRegionName(category), coordsList: coordsList, category: category));
            }
        }

        private string GetRegionName(Category category)
        {
            int maxTryCount = 10;

            for (int currentTry = 0; currentTry < maxTryCount; currentTry++)
            {
                string adjective = this.nameRandomizer.RandomizeFromList(adjectiveListByCategory[category]);
                string noun = this.nameRandomizer.RandomizeFromList(nounListByCategory[category]);

                string regionName = $"{adjective} {noun}";

                bool nameAlreadyExists = false;
                foreach (Location location in this.locationList)
                {
                    if (location.name == regionName)
                    {
                        nameAlreadyExists = true;
                        break;
                    }
                }

                if (!nameAlreadyExists || currentTry == maxTryCount - 1)
                {
                    // used names counters must be increased
                    this.nameRandomizer.IncreaseCounterForVal(adjective);
                    this.nameRandomizer.IncreaseCounterForVal(noun);

                    return regionName;
                }
            }

            return "";
        }

        private ConcurrentBag<Point> FindAllCellCoordsThatMeetCriteria(CellSearch cellSearch)
        {
            var cellCoordsBag = new ConcurrentBag<Point>();

            Parallel.ForEach(this.grid.allCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse / 2 }, cell =>
            {
                if (cellSearch.CellMeetsCriteria(cell: cell)) cellCoordsBag.Add(new Point(cell.cellNoX, cell.cellNoY));
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

            if (!cellCoordsLeftToProcess.Any()) return cellCoordsByRegion;

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
    }
}