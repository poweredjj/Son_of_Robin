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
            public bool HasBeenDiscovered { get; private set; }

            public Location(string name, Rectangle areaRect)
            {
                this.name = name;
                this.areaRect = areaRect;
                this.textRect = areaRect;
                this.textRect.Inflate(-areaRect.Width / 6, -areaRect.Height / 6);
                this.HasBeenDiscovered = false;
            }

            public void SetAsDiscovered()
            {
                this.HasBeenDiscovered = true;
            }
        }

        private readonly Grid grid;
        private readonly List<Location> locationList;
        private Location currentLocation;
        private List<int> discoveredLocationsToDeserialize;
        public IEnumerable DiscoveredLocations { get { return this.locationList.Where(location => location.HasBeenDiscovered); } }

        public NamedLocations(Grid grid)
        {
            this.grid = grid;
            this.locationList = new List<Location>();
            this.currentLocation = null;
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
            var locationData = new List<int>();

            int locationNo = 0;
            foreach (Location location in this.locationList)
            {
                if (location.HasBeenDiscovered) locationData.Add(locationNo);
                locationNo++;
            }

            return locationData;
        }

        public void Deserialize(Object locationData)
        {
            this.discoveredLocationsToDeserialize = (List<int>)locationData;
        }

        public void GenerateLocations(int seed)
        {
            Random random = new(seed);

            this.locationList.Add(new Location(name: "test location", areaRect: new Rectangle(1000, 1000, 500, 500)));
            this.locationList.Last().SetAsDiscovered();

            var randomHillCell = this.FindRandomCellThatMeetsCriteria(random: random, checkTerrain: true, checkExpProps: false, terrainName: Terrain.Name.Height, terrainMinVal: Terrain.rocksLevelMin, terrainMaxVal: 255, extPropsName: ExtBoardProps.Name.Sea, expPropsVal: false);
            if (randomHillCell != null)
            {
                this.locationList.Add(new Location(name: "testy hills", areaRect: randomHillCell.rect));
                this.locationList.Last().SetAsDiscovered();
            }

            if (this.discoveredLocationsToDeserialize != null)
            {
                foreach (int locationNo in (List<int>)this.discoveredLocationsToDeserialize)
                {
                    this.locationList[locationNo].SetAsDiscovered();
                }
                this.discoveredLocationsToDeserialize = null;
            }
        }

        private Cell FindRandomCellThatMeetsCriteria(Random random, bool checkTerrain, bool checkExpProps, Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, ExtBoardProps.Name extPropsName, bool expPropsVal)
        {
            if (!checkTerrain && !checkExpProps) throw new ArgumentException("No search criteria.");

            int randomColumn = random.Next(this.grid.noOfCellsX);

            var foundCellsForColumn = new ConcurrentBag<Point>();

            Parallel.For(0, this.grid.noOfCellsY, y =>
            {
                for (int x = 0; x < this.grid.noOfCellsX; x++)
                {
                    Point currentCell = new(x, y);

                    bool isWithinRange = (!checkTerrain || this.CheckIfCellTerrainIsInRange(terrainName: terrainName, terrainMinVal: terrainMinVal, terrainMaxVal: terrainMaxVal, coordinates: currentCell)) &&
                        (!checkExpProps || this.grid.CheckIfContainsExtPropertyForCell(name: extPropsName, value: expPropsVal, cellNoX: currentCell.X, cellNoY: currentCell.Y));

                    if (isWithinRange) foundCellsForColumn.Add(currentCell);
                }
            });

            var foundCellsList = foundCellsForColumn.ToList();
            if (foundCellsList.Any())
            {
                var randomPoint = foundCellsList[random.Next(foundCellsList.Count)];
                return this.grid.cellGrid[randomPoint.X, randomPoint.Y];
            }

            return null;
        }

        private ConcurrentBag<Point> FloodFillIfInRange(ConcurrentBag<Point> startingCells, bool checkTerrain, bool checkExpProps, Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, ExtBoardProps.Name extPropsName, bool expPropsVal)
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

                if (!nextCells.Any()) break;
            }

            return cellsInsideRange; // cell coords
        }

        private bool CheckIfCellTerrainIsInRange(Terrain.Name terrainName, byte terrainMinVal, byte terrainMaxVal, Point coordinates)
        {
            return this.grid.GetMinValueForCell(terrainName: terrainName, cellNoX: coordinates.X, cellNoY: coordinates.Y) >= terrainMinVal &&
                   this.grid.GetMaxValueForCell(terrainName: terrainName, cellNoX: coordinates.X, cellNoY: coordinates.Y) <= terrainMaxVal;
        }
    }
}