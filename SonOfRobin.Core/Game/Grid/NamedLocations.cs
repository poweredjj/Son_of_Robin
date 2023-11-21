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
            Hills,
            Lake,
            Volcano,
            Swamp,
            Desert,
            Island,
            Shore,
            Grassland,
            Ruins,
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

        private static readonly Dictionary<Category, Color> colorByCategory = new()
        {
            { Category.Hills, new Color(43, 43, 43) },
            { Category.Lake, new Color(37, 37, 138) },
            { Category.Volcano, new Color(237, 0, 0) },
            { Category.Swamp, new Color(26, 82, 0) },
            { Category.Desert, new Color(122, 98, 0) },
            { Category.Island, new Color(4, 184, 157) },
            { Category.Shore, new Color(186, 137, 32) },
            { Category.Grassland, new Color(27, 168, 2) },
            { Category.Ruins, new Color(112, 112, 112) },
        };

        private static readonly Dictionary<Category, List<string>> adjectiveListByCategory = new()
        {
            { Category.Hills, new List<string>{ "Windy", "Grand", "Patrick's", "Mysterious", "Windswept", "Majestic", "Enchanted", "Enigmatic", "Misty", "Mystical", "Verdant", "Timeless", "Twilight", "Celestial", "Solstice", "Seraph's", "Starlight", "Cascade", "Moonshadow", "Sunfire", "Eldertree", "Thunder", "Astral", "Crystal", "Obsidian", "Nova", "Starfall", "Pauline's", "Horizon Hues", "Caribbean", "Sunburst", "Sunkissed", "Azure", "Meridian" } },

            { Category.Lake, new List<string>{ "Deep", "Shiny", "Golden", "Lover's", "Shallow", "Silent", "Tranquil", "Reflective", "Serene", "Crystaline", "Secretive", "Ethereal", "Undisturbed", "Luminous", "Whispering", "Magician's", "Emerald Mirror", "Turquoise", "Paradise", "Palm Breeze", "Coconut", "Calypso", "Coral", "Mariner's", "Mango", "Siren's", "Flamingo", "Monkey", "Mermaid's" } },

            { Category.Volcano, new List<string>{ "Fiery", "Lava", "Infernal", "Flaming", "Smoldering", "Molten", "Blazing", "Hellish" } },

            { Category.Swamp, new List<string>{ "Muddy", "Mangrove", "Soggy", "Mosquito", "Murky", "Fogbound", "Murmuring", "Trecherous", "Drifting", "Whispering", "Venomous", "Sinking", "Withering", "Malarial", "Forsaken", "Foggy", "Cursed", "Ghostly", "Misty", "Dying", "Desolate", "Gloomveil", "Sorrowful", "Lamenting", "Melancholy", "Wilted", "Hopeless", "Anguished", "Fading", "Black" } },

            { Category.Desert, new List<string>{ "Scorched", "Arid", "Mirage", "Ephemeral", "Saharan", "Zephyr", "Desolate", "Fading", "Forsaken", "Lost", "Forgotten", "Lonely", "Dusty" } },

            { Category.Island, new List<string>{ "Seagull's", "Tiny", "Sunkissed", "Palmshade", "Coral", "Coconut", "Fool's", "Mermaid's", "Treasure", "Tranquil", "Driftwood", "Bahama", "Reefside", "Smuggler's", "Hidden", "Rogue's" } },

            { Category.Shore, new List<string>{ "Captain's", "Mariner's", "Buccaneer's", "Palmshade", "Swashbuckler's", "Marauder's", "Cutlass", "Bounty", "Dead Man's", "Corsair's", "Pirate's", "Tropic", "Shipwrecked", "Survivor's", "Marooned", "Columbus" } },

            { Category.Grassland, new List<string>{ "Green", "Grassy", "Breezy", "Whispering", "Serene", "Abundant", "Palmshade", "Harmony", "Explorer's", "Bounty", "Governor's", "Tranquil", "Restful", "Tropic", "Verdant", "St. Augustine's", "Saint Francis'", "Vespucci's" } },

            { Category.Ruins, new List<string>{ "Ancient", "Lost", "Jungle", "Ceremonial", "Mystical", "Crumbling", "Forgotten", "Decaying", "Faded", "Mossy", "Vineswept", "Eroding" } },
            };

        private static readonly Dictionary<Category, List<string>> nounListByCategory = new()
        {
            { Category.Hills, new List<string>{ "Hills", "Mountain", "Plateau", "Highlands", "Summit", "Ridge", "Crest", "Uplands", "Slopes", "Overlook", "Mesa", "Peaks", "Spire", "Peak", "Cliffs" } },

            { Category.Lake, new List<string>{ "Lake", "Pond", "Depths", "Waters", "Lagoon", "Cove", "Mirage", "Abyss", "Expanse", "Reservoir", "Basin" } },

            { Category.Volcano, new List<string>{ "Volcano", "Crater", "Caldera", "Peak" } },

            { Category.Swamp, new List<string>{ "Swamp", "Mire", "Quagmire", "Boglands", "Marsh", "Gloommarsh", "Morass", "Bog", "Waters", "Fenlands", "Damplands", "Fen" } },

            { Category.Desert, new List<string>{ "Desert", "Sands", "Dunes", "Wasteland", "Erg", "Quicksands" } },

            { Category.Island, new List<string>{ "Islet", "Refuge", "Retreat", "Haven", "Shoal", "Sandbar", "Atoll", "Cay", "Hideaway" } },

            { Category.Shore, new List<string>{ "Shore", "Beachfront", "Riviera", "Beach", "Haven", "Coast", "Sand"  } },

            { Category.Grassland, new List<string>{ "Grassland", "Meadow", "Savanna", "Expanse", "Plains", "Prairie", "Oasis", "Steppe", "Glade" } },

            { Category.Ruins, new List<string>{ "Ruins", "Remnants", "Enclave", "Court", "Mausoleum", "Keep", "Sanctum", "Site", "Grounds", "Totems", "Citadel", "Ruinscape" } },
            };

        public class Location
        {
            public readonly Grid grid;
            public readonly string name;
            public readonly Category category;
            public readonly List<Point> coordsList;
            public readonly Rectangle areaRect;
            public readonly List<Cell> cells;
            public bool hasBeenDiscovered;

            public Color Color
            { get { return colorByCategory[this.category]; } }

            public Location(Grid grid, string name, Category category, List<Point> coordsList, bool hasBeenDiscovered = false)
            {
                this.grid = grid;
                this.name = name;
                this.category = category;
                this.coordsList = coordsList;
                this.areaRect = this.GetAreaRect(this.coordsList);
                this.cells = this.GetCells(this.coordsList);
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

        public readonly struct CellSearch
        {
            public readonly List<SearchEntryTerrain> searchEntriesTerrain;
            public readonly List<SearchEntryExtProps> searchEntriesExtProps;

            public CellSearch(List<SearchEntryTerrain> searchEntriesTerrain = null, List<SearchEntryExtProps> searchEntriesExtProps = null)
            {
                if (searchEntriesTerrain == null) searchEntriesTerrain = new List<SearchEntryTerrain>();
                if (searchEntriesExtProps == null) searchEntriesExtProps = new List<SearchEntryExtProps>();

                if (searchEntriesTerrain.Count == 0 && searchEntriesExtProps.Count == 0) throw new ArgumentException("Invalid search criteria.");

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
                    if (!grid.ExtBoardProps.CheckIfContainsPropertyForCell(name: searchEntryExtProps.name, value: searchEntryExtProps.value, cellNoX: cellNoX, cellNoY: cellNoY)) return false;

                    if (searchEntryExtProps.strictSearch && grid.ExtBoardProps.CheckIfContainsPropertyForCell(name: searchEntryExtProps.name, value: !searchEntryExtProps.value, cellNoX: cellNoX, cellNoY: cellNoY)) return false;
                }

                return true;
            }

            private static bool TerrainIsInRange(Grid grid, int cellNoX, int cellNoY, Terrain.Name terrainName, byte minVal, byte maxVal, bool strictSearch)
            {
                // strictSearch: true == whole cell must match search criteria, false == any cell part must match search criteria

                byte minCellVal = grid.terrainByName[terrainName].GetMinValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);
                byte maxCellVal = grid.terrainByName[terrainName].GetMaxValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);

                return strictSearch ?
                    minCellVal >= minVal && maxCellVal <= maxVal :
                    maxCellVal >= minVal && minCellVal <= maxVal;
            }
        }

        private static readonly TimeSpan discoveryCooldown = TimeSpan.FromMinutes(5);
        private const int playerLocationCheckCooldownFrames = 60 * 2;

        private readonly Grid grid;
        public readonly List<Location> locationList;
        private Location playerLocation;
        private DateTime lastDiscovery;
        private bool locationsCreated;
        private readonly Random random;
        private NameRandomizer nameRandomizer;
        private int playerLocationLastCheckedFrame;

        public List<Location> DiscoveredLocations
        { get { return this.locationList.Where(location => location.hasBeenDiscovered).ToList(); } }

        public int DiscoveredLocationsCount
        { get { return this.DiscoveredLocations.Count; } }

        public int AllLocationsCount
        { get { return this.locationList.Count; } }

        public NamedLocations(Grid grid)
        {
            this.grid = grid;
            this.locationList = new List<Location>();
            this.locationsCreated = false;
            this.random = new(this.grid.level.seed);
            this.nameRandomizer = new(random: this.random);
            this.playerLocation = null;
            this.lastDiscovery = DateTime.Now;
            this.playerLocationLastCheckedFrame = 0;
        }

        public Location PlayerLocation
        {
            get
            {
                if (this.grid.world.CurrentUpdate > this.playerLocationLastCheckedFrame + playerLocationCheckCooldownFrames)
                {
                    this.playerLocationLastCheckedFrame = this.grid.world.CurrentUpdate;

                    Point playerPos = new((int)this.grid.world.Player.sprite.position.X, (int)this.grid.world.Player.sprite.position.Y);
                    this.playerLocation = null;

                    foreach (Location location in this.locationList)
                    {
                        if (location.IsPointInsideLocation(playerPos))
                        {
                            this.playerLocation = location;
                            break;
                        }
                    }
                }

                return this.playerLocation;
            }
        }

        public void ProcessDiscovery()
        {
            if (this.playerLocation == null ||
                DateTime.Now - this.lastDiscovery < discoveryCooldown ||
                Scene.GetTopSceneOfType(typeof(TextWindow)) != null)
            {
                return;
            }

            this.playerLocation.hasBeenDiscovered = true;

            World world = this.grid.world;
            Player player = world.Player;
            Rectangle viewRect = world.camera.viewRect;

            this.lastDiscovery = DateTime.Now;

            Vector2 locationCenter = new(this.playerLocation.areaRect.Center.X, this.playerLocation.areaRect.Center.Y);

            Vector2 distanceFromLocationCenter = Helpers.VectorKeepBelowSetValue(vector: locationCenter - player.sprite.position, maxValX: viewRect.Width / 2, maxValY: viewRect.Height / 2);

            Vector2 pointToShow = player.sprite.position + distanceFromLocationCenter;

            var taskChain = new List<Object> { };

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 1, executeHelper: true, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 0.4f }, { "zoomSpeedMultiplier", 0.4f } }, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetMovementSpeed, delay: 0, executeHelper: 0.3f, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackCoordsSmoothly, delay: 0, executeHelper: pointToShow, storeForLaterUse: true));

            taskChain.Add(new HintMessage(text: $"Discovered '{this.playerLocation.name}'.", boxType: HintMessage.BoxType.GreenBox, delay: 40, blockInput: false, useTransition: true, startingSound: SoundData.Name.TrumpetChime).ConvertToTask());

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraResetMovementSpeed, delay: 0, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackPiece, delay: 0, executeHelper: player, storeForLaterUse: true));
            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 1f } }, storeForLaterUse: true));

            taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SetCineMode, delay: 0, executeHelper: false, storeForLaterUse: true));

            new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
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
            this.locationsCreated = true;
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

            // var testCategories = new List<Category> { Category.Grassland };

            foreach (Category category in allCategories) // allCategories
            {
                this.CreateLocationsForCategory(category);
            }

            foreach (Location location in this.locationList)
            {
                // SonOfRobinGame.messageLog.AddMessage(text: $"Location: {location.name} {location.areaRect}"); // for testing
                // location.hasBeenDiscovered = true; // for testing
            }

            this.locationsCreated = true;
            this.nameRandomizer = null;
        }

        private void CreateLocationsForCategory(Category category)
        {
            List<CellSearch> cellSearches = new();
            int minCells, maxCells, density; // density: low number == high density, high number == low density

            switch (category)
            {
                case Category.Hills:

                    cellSearches.Add(new CellSearch(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.rocksLevelMin + 15, maxVal: 255),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin),
                        }
                        ));

                    minCells = 15;
                    maxCells = 200;
                    density = 1;

                    break;

                case Category.Lake:

                    cellSearches.Add(new CellSearch(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: Terrain.waterLevelMax) },
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.Sea, value: false, strictSearch: true) }
                        ));

                    minCells = 20;
                    maxCells = 500;
                    density = 1;

                    break;

                case Category.Volcano:

                    cellSearches.Add(new CellSearch(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin + 1, maxVal: 255) }
                        ));

                    minCells = 1;
                    maxCells = 150;
                    density = 1;

                    break;

                case Category.Swamp:

                    cellSearches.Add(new CellSearch(
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true) }
                        ));

                    minCells = 20;
                    maxCells = 400;
                    density = 1;

                    break;

                case Category.Ruins:

                    cellSearches.Add(new CellSearch(
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true) }
                        ));

                    minCells = 2;
                    maxCells = 800;
                    density = 1;

                    break;

                case Category.Desert:

                    cellSearches.Add(new(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin),
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax, maxVal: Terrain.rocksLevelMin),
                        },
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.OuterBeach, value: false, strictSearch: true) }
                        ));

                    minCells = 10;
                    maxCells = 400;
                    density = 1;

                    break;

                case Category.Island:

                    cellSearches.Add(new(
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.OuterBeach, value: true) }
                        ));

                    minCells = 5;
                    maxCells = 100;
                    density = 1;

                    break;

                case Category.Shore:

                    // multiple searches, to break up shoreline into smaller pieces

                    cellSearches.Add(new(
                            searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 128, strictSearch: true),
                        },
                        searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.OuterBeach, value: true) }
                        ));

                    cellSearches.Add(new(
                           searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 129, maxVal: 255, strictSearch: true),
                       },
                       searchEntriesExtProps: new List<SearchEntryExtProps> { new SearchEntryExtProps(name: ExtBoardProps.Name.OuterBeach, value: true) }
                       ));

                    minCells = 25;
                    maxCells = 800;
                    density = 1;

                    break;

                case Category.Grassland:

                    cellSearches.Add(new(
                        searchEntriesTerrain: new List<SearchEntryTerrain> {
                            new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 118, maxVal: 255),
                            new SearchEntryTerrain(name: Terrain.Name.Biome, minVal: 0, maxVal: Terrain.biomeMin, strictSearch: true),
                            new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin, strictSearch: true),
                        }
                        ));

                    minCells = 15;
                    maxCells = 250;
                    density = 1;

                    break;

                default:
                    throw new ArgumentException($"Unsupported category - {category}.");
            }

            foreach (CellSearch cellSearch in cellSearches)
            {
                var cellCoordsByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: this.grid.noOfCellsX, height: this.grid.noOfCellsY, pointsBag: this.FindAllCellCoordsThatMeetCriteria(cellSearch));
                foreach (List<Point> coordsList in cellCoordsByRegion)
                {
                    if (coordsList.Count < minCells || coordsList.Count > maxCells || this.random.Next(density) != 0) continue;

                    this.locationList.Add(new(grid: this.grid, name: this.GetRegionName(category), coordsList: coordsList, category: category));
                }
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

            Parallel.ForEach(this.grid.allCells, SonOfRobinGame.defaultParallelOptions, cell =>
            {
                if (cellSearch.CellMeetsCriteria(cell: cell)) cellCoordsBag.Add(new Point(cell.cellNoX, cell.cellNoY));
            });

            return cellCoordsBag;
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
}