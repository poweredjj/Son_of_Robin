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

        private readonly World world;
        private readonly List<Location> locationList;
        private Location currentLocation;
        public IEnumerable DiscoveredLocations { get { return this.locationList.Where(location => location.HasBeenDiscovered); } }
        public IEnumerable UndiscoveredLocations { get { return this.locationList.Where(location => !location.HasBeenDiscovered); } }

        public NamedLocations(World world)
        {
            this.world = world;
            this.locationList = new List<Location>();
            this.currentLocation = null;
        }

        public void GenerateLocations(int seed)
        {
            Random random = new(seed);

            // TODO add code

            this.locationList.Add(new Location(name: "test location", areaRect: new Rectangle(1000, 1000, 500, 500)));
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
            this.GenerateLocations(seed: this.world.seed);

            foreach (int locationNo in (List<int>)locationData)
            {
                this.locationList[locationNo].SetAsDiscovered();
            }
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

            Grid grid = this.world.Grid;
            int noOfCellsX = grid.noOfCellsX;
            int noOfCellsY = grid.noOfCellsY;

            bool[,] gridToFill = new bool[noOfCellsX, noOfCellsY];

            var nextCells = new ConcurrentBag<Point>(startingCells);
            var cellsInsideRange = new ConcurrentBag<Point>();

            while (true)
            {
                List<Point> currentCells = nextCells.Distinct().ToList(); // using Distinct() to filter out duplicates
                nextCells = new ConcurrentBag<Point>(); // because there is no Clear() for ConcurrentBag

                Parallel.ForEach(currentCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentCell =>
                {
                    // array can be written to using parallel, if every thread accesses its own indices

                    bool isWithinRange = false;

                    if (checkTerrain)
                    {
                        byte currentCellMinVal = grid.GetMinValueForCell(terrainName: terrainName, cellNoX: currentCell.X, cellNoY: currentCell.Y);
                        byte currentCellMaxVal = grid.GetMaxValueForCell(terrainName: terrainName, cellNoX: currentCell.X, cellNoY: currentCell.Y);

                        if (currentCellMinVal >= terrainMinVal && currentCellMaxVal <= terrainMaxVal) isWithinRange = true;
                    }
                    if (checkExpProps && isWithinRange)
                    {
                        bool currentCellExtPropsVal = grid.CheckIfContainsExtPropertyForCell(name: extPropsName, value: expPropsVal, cellNoX: currentCell.X, cellNoY: currentCell.Y);

                        if (expPropsVal == currentCellExtPropsVal) isWithinRange = true;
                    }

                    if (isWithinRange)
                    {
                        cellsInsideRange.Add(currentCell);

                        foreach (Point currentOffset in offsetList)
                        {
                            Point nextPoint = new(currentCell.X + currentOffset.X, currentCell.Y + currentOffset.Y);

                            if (nextPoint.X >= 0 && nextPoint.X < noOfCellsX &&
                                nextPoint.Y >= 0 && nextPoint.Y < noOfCellsY &&
                                !gridToFill[nextPoint.X, nextPoint.Y])
                            {
                                nextCells.Add(nextPoint);
                            }
                        }
                    }

                    gridToFill[currentCell.X, currentCell.Y] = true;
                });

                if (!nextCells.Any()) break;
            }

            return cellsInsideRange; // cell coords
        }
    }
}