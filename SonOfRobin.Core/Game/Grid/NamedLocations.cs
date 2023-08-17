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
        public class Location
        {
            public readonly string name;
            public readonly Rectangle areaRect;
            public readonly Rectangle textRect;
            public bool hasBeenDiscovered;

            public Location(string name, Rectangle areaRect)
            {
                this.name = name;
                this.areaRect = areaRect;
                this.textRect = areaRect;
                this.textRect.Inflate(-areaRect.Width / 6, -areaRect.Height / 6);
                this.hasBeenDiscovered = false;
            }

            public Object Serialize()
            {
                var locationData = new Dictionary<string, object>
            {
                { "name", this.name },
                { "areaRect", this.areaRect },
                { "hasBeenDiscovered", this.hasBeenDiscovered },
            };

                return locationData;
            }

            public static Location Deserialize(Object locationData)
            {
                var locationDict = (Dictionary<string, object>)locationData;

                string name = (string)locationDict["name"];
                Rectangle areaRect = (Rectangle)locationDict["areaRect"];
                bool hasBeenDiscovered = (bool)locationDict["hasBeenDiscovered"];

                return new(name: name, areaRect: areaRect)
                {
                    hasBeenDiscovered = hasBeenDiscovered
                };
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
        public IEnumerable DiscoveredLocations { get { return this.locationList.Where(location => location.hasBeenDiscovered); } }

        public NamedLocations(Grid grid)
        {
            this.grid = grid;
            this.locationList = new List<Location>();
            this.currentLocation = null;
            this.locationsCreated = false;
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

            Random random = new(this.grid.world.seed);

            var cellCoordsBag = this.FindAllCellCoordsThatMeetCriteria(new(checkTerrain: true, checkExpProps: false, terrainName: Terrain.Name.Height, terrainMinVal: Terrain.rocksLevelMin, terrainMaxVal: 255, extPropsName: ExtBoardProps.Name.Sea, expPropsVal: false));

            var cellCoordsByRegion = this.SplitCellBagIntoRegions(cellCoordsBag);

            foreach (var kvp in cellCoordsByRegion)
            {
                int regionNo = kvp.Key;
                List<Point> cellCoords = kvp.Value;
                Rectangle areaRect = this.GetLocationRect(cellCoords);
                this.locationList.Add(new Location(name: $"testy hills {regionNo}", areaRect: areaRect));
                this.locationList.Last().hasBeenDiscovered = true; // for testing
            }

            //Rectangle areaRect = this.FindRandomRectThatMeetsCriteria(random: random, searchCriteria: new(checkTerrain: true, checkExpProps: false, terrainName: Terrain.Name.Height, terrainMinVal: Terrain.rocksLevelMin, terrainMaxVal: 255, extPropsName: ExtBoardProps.Name.Sea, expPropsVal: false), maxCells: 120);
            //if (areaRect.Width != 1 && areaRect.Height != 1)
            //{
            //    this.locationList.Add(new Location(name: "testy hills", areaRect: areaRect));
            //    this.locationList.Last().hasBeenDiscovered = true; // for testing
            //}

            foreach (Location location in this.locationList)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"Location: {location.name} {location.areaRect}");
            }

            this.locationsCreated = true;
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

        private Dictionary<int, List<Point>> SplitCellBagIntoRegions(ConcurrentBag<Point> cellCoordsBag)
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
            var cellCoordsByRegion = new Dictionary<int, List<Point>>();
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
                cellCoordsByRegion[currentRegion] = thisRegionCoords.Distinct().ToList();
                currentRegion++;

                if (!cellCoordsLeftToProcess.Any()) return cellCoordsByRegion;
            }
        }

        private Rectangle FindRandomRectThatMeetsCriteria(Random random, SearchCriteria searchCriteria, int maxCells = 0)
        {
            int randomColumn = random.Next(this.grid.noOfCellsX);
            var foundCellCoords = new ConcurrentBag<Point>();
            Parallel.For(0, this.grid.noOfCellsY, y =>
            {
                int currentColumn = randomColumn;

                for (int i = 0; i < 30; i++)
                {
                    currentColumn++;
                    if (currentColumn >= this.grid.noOfCellsX) currentColumn = 0;

                    Point currentCell = new(currentColumn, y);

                    bool isWithinRange = (!searchCriteria.checkTerrain || this.CheckIfCellTerrainIsInRange(terrainName: searchCriteria.terrainName, terrainMinVal: searchCriteria.terrainMinVal, terrainMaxVal: searchCriteria.terrainMaxVal, cellNoX: currentCell.X, cellNoY: currentCell.Y)) &&
                        (!searchCriteria.checkExpProps || this.grid.CheckIfContainsExtPropertyForCell(name: searchCriteria.extPropsName, value: searchCriteria.expPropsVal, cellNoX: currentCell.X, cellNoY: currentCell.Y));

                    if (isWithinRange) foundCellCoords.Add(currentCell);
                }
            });

            var foundCellsList = foundCellCoords.ToList();
            if (foundCellsList.Any())
            {
                var randomPoint = foundCellsList[random.Next(foundCellsList.Count)];

                var pointsBagForFilledArea = this.FloodFillIfInRange(startingCells: new ConcurrentBag<Point> { randomPoint }, searchCriteria: searchCriteria, maxCells: maxCells);
                return this.GetLocationRect(pointsBagForFilledArea.ToList());
            }

            return new Rectangle(0, 0, 0, 0);
        }

        private ConcurrentBag<Point> FloodFillIfInRange(ConcurrentBag<Point> startingCells, SearchCriteria searchCriteria, int maxCells = 0)
        {
            List<Point> offsetList = new()
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1),
            };

            Grid grid = this.grid;
            int noOfCellsX = grid.noOfCellsX;
            int noOfCellsY = grid.noOfCellsY;

            bool[,] processedCellCoords = new bool[noOfCellsX, noOfCellsY];

            var nextCellCoords = new ConcurrentBag<Point>(startingCells);
            var cellCoordsInsideRange = new ConcurrentBag<Point>();

            while (true)
            {
                List<Point> currentCellCoords = nextCellCoords.Distinct().ToList(); // using Distinct() to filter out duplicates
                nextCellCoords = new ConcurrentBag<Point>(); // because there is no Clear() for ConcurrentBag

                Parallel.ForEach(currentCellCoords, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentCoords =>
                {
                    // array can be written to using parallel, if every thread accesses its own indices

                    bool isWithinRange = (!searchCriteria.checkTerrain || this.CheckIfCellTerrainIsInRange(terrainName: searchCriteria.terrainName, terrainMinVal: searchCriteria.terrainMinVal, terrainMaxVal: searchCriteria.terrainMaxVal, cellNoX: currentCoords.X, cellNoY: currentCoords.Y)) &&
                    (!searchCriteria.checkExpProps || grid.CheckIfContainsExtPropertyForCell(name: searchCriteria.extPropsName, value: searchCriteria.expPropsVal, cellNoX: currentCoords.X, cellNoY: currentCoords.Y));

                    if (isWithinRange)
                    {
                        cellCoordsInsideRange.Add(currentCoords);

                        foreach (Point currentOffset in offsetList)
                        {
                            Point nextPoint = new(currentCoords.X + currentOffset.X, currentCoords.Y + currentOffset.Y);

                            if (nextPoint.X >= 0 && nextPoint.X < noOfCellsX &&
                                nextPoint.Y >= 0 && nextPoint.Y < noOfCellsY &&
                                !processedCellCoords[nextPoint.X, nextPoint.Y])
                            {
                                nextCellCoords.Add(nextPoint);
                            }
                        }
                    }

                    processedCellCoords[currentCoords.X, currentCoords.Y] = true;
                });

                if (!nextCellCoords.Any() || (maxCells != 0 && cellCoordsInsideRange.Count > maxCells)) break;
            }

            return cellCoordsInsideRange;
        }

        private Rectangle GetLocationRect(List<Point> pointList)
        {
            int xMin = Int32.MaxValue;
            int xMax = 0;
            int yMin = Int32.MaxValue;
            int yMax = 0;

            foreach (Point point in pointList)
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