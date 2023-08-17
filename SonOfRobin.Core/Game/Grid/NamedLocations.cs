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

            Rectangle areaRect = this.FindRandomRectThatMeetsCriteria(random: random, checkTerrain: true, checkExpProps: false, terrainName: Terrain.Name.Height, terrainMinVal: Terrain.rocksLevelMin, terrainMaxVal: 255, extPropsName: ExtBoardProps.Name.Sea, expPropsVal: false, maxCells: 150);
            if (areaRect.Width != 1 && areaRect.Height != 1)
            {
                this.locationList.Add(new Location(name: "testy hills", areaRect: areaRect));
                this.locationList.Last().hasBeenDiscovered = true; // for testing
            }

            foreach (Location location in this.locationList)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"Location: {location.name} {location.areaRect}");
            }

            this.locationsCreated = true;
        }

        private Rectangle FindRandomRectThatMeetsCriteria(Random random, bool checkTerrain, bool checkExpProps, Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, ExtBoardProps.Name extPropsName, bool expPropsVal, int maxCells = 0)
        {
            if (!checkTerrain && !checkExpProps) throw new ArgumentException("No search criteria.");

            int randomColumn = random.Next(this.grid.noOfCellsX);

            var foundCellsForColumn = new ConcurrentBag<Point>();

            Parallel.For(0, this.grid.noOfCellsY, y =>
            {
                int currentColumn = randomColumn;

                for (int i = 0; i < 30; i++)
                {
                    currentColumn++;
                    if (currentColumn >= this.grid.noOfCellsX) currentColumn = 0;

                    Point currentCell = new(currentColumn, y);

                    bool isWithinRange = (!checkTerrain || this.CheckIfCellTerrainIsInRange(terrainName: terrainName, terrainMinVal: terrainMinVal, terrainMaxVal: terrainMaxVal, coordinates: currentCell)) &&
                        (!checkExpProps || this.grid.CheckIfContainsExtPropertyForCell(name: extPropsName, value: expPropsVal, cellNoX: currentCell.X, cellNoY: currentCell.Y));

                    if (isWithinRange) foundCellsForColumn.Add(currentCell);
                }
            });

            var foundCellsList = foundCellsForColumn.ToList();
            if (foundCellsList.Any())
            {
                var randomPoint = foundCellsList[random.Next(foundCellsList.Count)];

                var pointsBagForFilledArea = this.FloodFillIfInRange(startingCells: new ConcurrentBag<Point> { randomPoint }, checkTerrain: checkTerrain, checkExpProps: checkExpProps, terrainName: terrainName, terrainMinVal: terrainMinVal, terrainMaxVal: terrainMaxVal, extPropsName: extPropsName, expPropsVal: expPropsVal, maxCells: maxCells);

                return this.GetLocationRect(pointsBagForFilledArea);
            }

            return new Rectangle(0, 0, 0, 0);
        }

        private ConcurrentBag<Point> FloodFillIfInRange(ConcurrentBag<Point> startingCells, bool checkTerrain, bool checkExpProps, Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, ExtBoardProps.Name extPropsName, bool expPropsVal, int maxCells = 0)
        {
            if (!checkTerrain && !checkExpProps) throw new ArgumentException("No search criteria.");

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

            bool[,] processedCells = new bool[noOfCellsX, noOfCellsY];

            var nextCells = new ConcurrentBag<Point>(startingCells);
            var cellsInsideRange = new ConcurrentBag<Point>();

            while (true)
            {
                List<Point> currentCells = nextCells.Distinct().ToList(); // using Distinct() to filter out duplicates
                nextCells = new ConcurrentBag<Point>(); // because there is no Clear() for ConcurrentBag

                Parallel.ForEach(currentCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentCell =>
                {
                    // array can be written to using parallel, if every thread accesses its own indices

                    bool isWithinRange = (!checkTerrain || this.CheckIfCellTerrainIsInRange(terrainName: terrainName, terrainMinVal: terrainMinVal, terrainMaxVal: terrainMaxVal, coordinates: currentCell)) &&
                    (!checkExpProps || grid.CheckIfContainsExtPropertyForCell(name: extPropsName, value: expPropsVal, cellNoX: currentCell.X, cellNoY: currentCell.Y));

                    if (isWithinRange)
                    {
                        cellsInsideRange.Add(currentCell);

                        foreach (Point currentOffset in offsetList)
                        {
                            Point nextPoint = new(currentCell.X + currentOffset.X, currentCell.Y + currentOffset.Y);

                            if (nextPoint.X >= 0 && nextPoint.X < noOfCellsX &&
                                nextPoint.Y >= 0 && nextPoint.Y < noOfCellsY &&
                                !processedCells[nextPoint.X, nextPoint.Y])
                            {
                                nextCells.Add(nextPoint);
                            }
                        }
                    }

                    processedCells[currentCell.X, currentCell.Y] = true;
                });

                if (!nextCells.Any() || (maxCells != 0 && cellsInsideRange.Count > maxCells)) break;
            }

            return cellsInsideRange; // cell coords
        }

        private Rectangle GetLocationRect(ConcurrentBag<Point> pointsBag)
        {
            int xMin = Int32.MaxValue;
            int xMax = 0;
            int yMin = Int32.MaxValue;
            int yMax = 0;

            foreach (Point point in pointsBag)
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

        private bool CheckIfCellTerrainIsInRange(Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, Point coordinates)
        {
            return this.grid.GetMinValueForCell(terrainName: terrainName, cellNoX: coordinates.X, cellNoY: coordinates.Y) >= terrainMinVal &&
                   this.grid.GetMaxValueForCell(terrainName: terrainName, cellNoX: coordinates.X, cellNoY: coordinates.Y) <= terrainMaxVal;
        }
    }
}