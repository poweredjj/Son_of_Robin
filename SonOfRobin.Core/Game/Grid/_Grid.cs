using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class Grid
    {
        public enum Stage
        { LoadTerrain, GenerateTerrain, CheckExtData, SetExtDataSea, SetExtDataBeach, SetExtDataBiomes, SetExtDataBiomesConstrains, SetExtDataFinish, FillAllowedNames, ProcessTextures, LoadTextures }

        private static readonly int allStagesCount = ((Stage[])Enum.GetValues(typeof(Stage))).Length;

        private static readonly Dictionary<Stage, string> namesForStages = new Dictionary<Stage, string> {
            { Stage.LoadTerrain, "loading terrain" },
            { Stage.GenerateTerrain, "generating terrain" },
            { Stage.CheckExtData, "loading extended data" },
            { Stage.SetExtDataSea, "setting extended data (sea)" },
            { Stage.SetExtDataBeach, "setting extended data (beach)" },
            { Stage.SetExtDataBiomes, "setting extended data (biomes)" },
            { Stage.SetExtDataBiomesConstrains, "setting extended data (constrains)" },
            { Stage.SetExtDataFinish, "saving extended data" },
            { Stage.FillAllowedNames, "filling lists of allowed names" },
            { Stage.ProcessTextures, "processing textures" },
            { Stage.LoadTextures, "loading textures" },
        };

        private Stage currentStage;
        private DateTime stageStartTime;
        private int stageStartFrame;
        public bool CreationInProgress { get; private set; }

        public readonly GridTemplate gridTemplate;
        public readonly World world;

        public readonly int width;
        public readonly int height;
        public readonly int dividedWidth;
        public readonly int dividedHeight;

        public readonly int noOfCellsX;
        public readonly int noOfCellsY;
        public readonly int cellWidth;
        public readonly int cellHeight;
        public readonly int resDivider;

        private readonly Dictionary<Terrain.Name, Terrain> terrainByName;
        private ExtBoardProps extBoardProps;

        public readonly Cell[,] cellGrid;
        public readonly List<Cell> allCells;
        public readonly List<Cell> cellsToProcessOnStart;

        private readonly Dictionary<PieceTemplate.Name, List<Cell>> cellListsForPieceNames; // pieces and cells that those pieces can (initially) be placed into

        private ConcurrentBag<Point> tempRawPointsForBiomeCreation;
        private Dictionary<ExtBoardProps.Name, ConcurrentBag<Point>> tempPointsForCreatedBiomes;
        private readonly Dictionary<ExtBoardProps.Name, int> biomeCountByName; // to ensure biome diversity
        public int loadedTexturesCount;

        public bool ProcessingStageComplete
        { get { return this.cellsToProcessOnStart.Count == 0; } }

        public List<Cell> CellsVisitedByPlayer
        { get { return this.allCells.Where(cell => cell.VisitedByPlayer).ToList(); } }

        public List<Cell> CellsNotVisitedByPlayer
        { get { return this.allCells.Where(cell => !cell.VisitedByPlayer).ToList(); } }

        public Grid(World world, int resDivider, int cellWidth = 0, int cellHeight = 0)
        {
            this.CreationInProgress = true;
            this.currentStage = 0;

            this.world = world;
            this.resDivider = resDivider;

            this.terrainByName = new Dictionary<Terrain.Name, Terrain>();

            this.width = this.world.width;
            this.height = this.world.height;
            this.dividedWidth = (int)Math.Ceiling((double)this.width / (double)this.resDivider);
            this.dividedHeight = (int)Math.Ceiling((double)this.height / (double)this.resDivider);

            if (cellWidth == 0 && cellHeight == 0)
            {
                Vector2 maxFrameSize = CalculateMaxFrameSize();
                this.cellWidth = (int)(maxFrameSize.X * 1.1);
                this.cellHeight = (int)(maxFrameSize.Y * 1.1);

                this.cellWidth = (int)Math.Ceiling(this.cellWidth / 2d) * 2;
                this.cellHeight = (int)Math.Ceiling(this.cellWidth / 2d) * 2;
            }
            else
            {
                this.cellWidth = cellWidth;
                this.cellHeight = cellHeight;
            }

            if (this.cellWidth % 2 != 0) throw new ArgumentException($"Cell width {this.cellWidth} is not divisible by 2.");
            if (this.cellHeight % 2 != 0) throw new ArgumentException($"Cell height {this.cellHeight} is not divisible by 2.");

            this.gridTemplate = new GridTemplate(seed: this.world.seed, width: this.world.width, height: this.world.height, cellWidth: this.cellWidth, cellHeight: this.cellHeight, resDivider: this.resDivider);

            this.noOfCellsX = (int)Math.Ceiling((float)this.world.width / (float)this.cellWidth);
            this.noOfCellsY = (int)Math.Ceiling((float)this.world.height / (float)this.cellHeight);

            this.cellListsForPieceNames = new Dictionary<PieceTemplate.Name, List<Cell>>();
            this.cellGrid = this.MakeGrid();
            this.allCells = this.GetAllCells();
            this.CalculateSurroundingCells();

            if (this.CopyBoardFromTemplate())
            {
                this.CreationInProgress = false;
                return;
            }

            this.cellsToProcessOnStart = new List<Cell>();
            this.tempRawPointsForBiomeCreation = new ConcurrentBag<Point>();
            this.tempPointsForCreatedBiomes = new Dictionary<ExtBoardProps.Name, ConcurrentBag<Point>>();
            this.biomeCountByName = new Dictionary<ExtBoardProps.Name, int>();

            foreach (ExtBoardProps.Name name in ExtBoardProps.allBiomes.OrderBy(name => this.world.random.Next()).ToList()) // shuffled biome list
            {
                this.biomeCountByName[name] = 0;
                this.tempPointsForCreatedBiomes[name] = new ConcurrentBag<Point>();
            }

            this.PrepareNextStage();
        }

        public Dictionary<string, Object> Serialize()
        {
            // this data is included in save file (not in template)

            var cellData = new List<Object> { };

            foreach (Cell cell in this.allCells)
            {
                cellData.Add(cell.Serialize());
            }

            Dictionary<string, Object> gridData = new Dictionary<string, object>
            {
                { "cellWidth", this.cellWidth },
                { "cellHeight", this.cellHeight },
                { "cellData", cellData },
            };

            return gridData;
        }

        public static Grid Deserialize(Dictionary<string, Object> gridData, World world, int resDivider)
        {
            // this data is included in save file (not in template)

            int cellWidth = (int)gridData["cellWidth"];
            int cellHeight = (int)gridData["cellHeight"];
            var cellData = (List<Object>)gridData["cellData"];

            Grid grid = new Grid(world: world, cellWidth: cellWidth, cellHeight: cellHeight, resDivider: resDivider);

            for (int i = 0; i < grid.allCells.Count; i++)
            {
                Cell cell = grid.allCells[i];
                cell.Deserialize(cellData[i]);
            }

            return grid;
        }

        public bool CopyBoardFromTemplate()
        {
            var existingWorlds = Scene.GetAllScenesOfType(typeof(World));
            if (existingWorlds.Count == 0) return false;

            World newWorld = this.world;

            foreach (Scene scene in existingWorlds)
            {
                World oldWorld = (World)scene;
                if (!oldWorld.WorldCreationInProgress &&
                    oldWorld.Grid != null &&
                    !oldWorld.Grid.CreationInProgress &&
                    newWorld.seed == oldWorld.seed &&
                    newWorld.width == oldWorld.width &&
                    newWorld.height == oldWorld.height)
                {
                    Grid templateGrid = oldWorld.Grid;

                    foreach (var kvp in templateGrid.terrainByName)
                    {
                        Terrain.Name terrainName = kvp.Key;
                        Terrain terrain = kvp.Value;
                        this.terrainByName[terrainName] = terrain;
                        this.terrainByName[terrainName].AttachToNewGrid(this);
                    }

                    this.extBoardProps = templateGrid.extBoardProps;
                    this.extBoardProps.AttachToNewGrid(this);

                    for (int x = 0; x < templateGrid.noOfCellsX; x++)
                    {
                        for (int y = 0; y < templateGrid.noOfCellsY; y++)
                            this.cellGrid[x, y].CopyFromTemplate(templateGrid.cellGrid[x, y]);
                    }

                    this.loadedTexturesCount = this.allCells.Where(cell => cell.boardGraphics.Texture != null).ToList().Count;

                    this.FillCellListsForPieceNames();

                    return true;
                }
            }
            return false;
        }

        public void ProcessNextCreationStage()
        {
            if (SonOfRobinGame.CurrentUpdate < this.stageStartFrame + 3)
            {
                // first frame of each stage should update progress bar
                this.UpdateProgressBar();
                return;
            }

            List<Cell> cellProcessingQueue;

            switch (currentStage)
            {
                case Stage.LoadTerrain:
                    {
                        this.terrainByName[Terrain.Name.Height] = new Terrain(
                            grid: this, name: Terrain.Name.Height, frequency: 8f, octaves: 9, persistence: 0.5f, lacunarity: 1.9f, gain: 0.55f, addBorder: true);

                        this.terrainByName[Terrain.Name.Humidity] = new Terrain(
                            grid: this, name: Terrain.Name.Humidity, frequency: 4.3f, octaves: 9, persistence: 0.6f, lacunarity: 1.7f, gain: 0.6f);

                        this.terrainByName[Terrain.Name.Biome] = new Terrain(
                            grid: this, name: Terrain.Name.Biome, frequency: 7f, octaves: 3, persistence: 0.7f, lacunarity: 1.4f, gain: 0.3f, addBorder: true);

                        Parallel.ForEach(this.terrainByName.Values, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, terrain =>
                        {
                            terrain.TryToLoadSavedTerrain();
                        });
                    }

                    this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.GenerateTerrain:

                    bool processedOneUpdateCycle = false;
                    int processSegments = 5;

                    foreach (Terrain currentTerrain in this.terrainByName.Values)
                    {
                        if (currentTerrain.CreationInProgress)
                        {
                            currentTerrain.UpdateNoiseMap(this.dividedHeight / processSegments);
                            processedOneUpdateCycle = true;
                            break;
                        }
                    }

                    int noOfCellsToRemove = Math.Min(this.allCells.Count / processSegments / this.terrainByName.Count, this.cellsToProcessOnStart.Count);
                    if (noOfCellsToRemove > 0) this.cellsToProcessOnStart.RemoveRange(0, noOfCellsToRemove); // to update progress bar

                    if (!processedOneUpdateCycle) this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.CheckExtData:

                    if (this.extBoardProps == null) this.extBoardProps = new ExtBoardProps(grid: this);
                    this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.SetExtDataSea:

                    if (this.extBoardProps.CreationInProgress) this.ExtCalculateSea();
                    else this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.SetExtDataBeach:

                    if (this.extBoardProps.CreationInProgress) this.ExtCalculateOuterBeach();
                    else this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.SetExtDataBiomes:

                    if (this.extBoardProps.CreationInProgress) this.ExtCalculateBiomes();
                    else this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.SetExtDataBiomesConstrains:

                    if (this.extBoardProps.CreationInProgress) this.ExtApplyBiomeConstrains();
                    else this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.SetExtDataFinish:

                    if (this.extBoardProps.CreationInProgress) this.ExtEndCreation();
                    else this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.FillAllowedNames:

                    Parallel.ForEach(this.allCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
                    {
                        cell.FillAllowedNames(); // needs to be invoked after calculating final terrain and ExtProps
                    });

                    this.FillCellListsForPieceNames();
                    this.cellsToProcessOnStart.Clear();

                    break;

                case Stage.ProcessTextures:

                    int texturesCount = Directory.GetFiles(this.gridTemplate.templatePath).Where(file => file.EndsWith(".png")).ToList().Count;
                    bool allTexturesFound = texturesCount == this.allCells.Count;

                    cellProcessingQueue = new List<Cell> { };

                    int noOfCellsToProcess = allTexturesFound ? this.allCells.Count : 70;
                    for (int i = 0; i < noOfCellsToProcess; i++)
                    {
                        cellProcessingQueue.Add(this.cellsToProcessOnStart[0]);
                        this.cellsToProcessOnStart.RemoveAt(0);
                        if (this.cellsToProcessOnStart.Count == 0) break;
                    }

                    Parallel.ForEach(cellProcessingQueue, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
                    {
                        cell.UpdateBoardGraphics();
                    });

                    break;

                case Stage.LoadTextures:

                    if (Preferences.loadWholeMap)
                    {
                        while (true)
                        {
                            Cell cell = this.cellsToProcessOnStart[0];
                            this.cellsToProcessOnStart.RemoveAt(0);
                            cell.boardGraphics.LoadTexture();

                            if (this.ProcessingStageComplete)
                            {
                                this.CreationInProgress = false;
                                return;
                            }

                            if (Scene.UpdateTimeElapsed.Milliseconds > 600) break;
                        }
                    }
                    else
                    {
                        this.cellsToProcessOnStart.Clear();
                        this.CreationInProgress = false;
                        return;
                    }

                    break;

                default:
                    if ((int)this.currentStage < allStagesCount) throw new ArgumentException("Not all steps has been processed.");

                    this.CreationInProgress = false;
                    break;
            }

            this.UpdateProgressBar();

            if (this.ProcessingStageComplete)
            {
                TimeSpan creationDuration = DateTime.Now - this.stageStartTime;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{namesForStages[this.currentStage]} - time: {creationDuration:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

                this.currentStage++;
                this.PrepareNextStage();
            }
        }

        private void FillCellListsForPieceNames()
        {
            foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
            {
                this.cellListsForPieceNames[pieceName] = new List<Cell>();

                foreach (Cell cell in this.allCells)
                {
                    if (cell.allowedNames.Contains(pieceName)) this.cellListsForPieceNames[pieceName].Add(cell);
                }
            }
        }

        private void ExtCalculateSea()
        {
            // the algorithm will only work, if water surrounds the island

            int maxX = (this.world.width / this.resDivider) - 1;
            int maxY = (this.world.height / this.resDivider) - 1;

            var startingPointsRaw = new ConcurrentBag<Point>();

            foreach (int x in new List<int> { 0, maxX / 2, maxX })
            {
                foreach (int y in new List<int> { 0, maxY / 2, maxY })
                {
                    if (x == 0 || x == maxX || y == 0 || y == maxY) startingPointsRaw.Add(new Point(x, y)); // adding edges only
                }
            }

            this.FloodFillExtProps(
                 startingPoints: startingPointsRaw,
                 terrainName: Terrain.Name.Height,
                 minVal: 0, maxVal: (byte)(Terrain.waterLevelMax),
                 nameToSetIfInRange: ExtBoardProps.Name.Sea,
                 setNameIfOutsideRange: true,
                 nameToSetIfOutsideRange: ExtBoardProps.Name.OuterBeach
                 );

            this.cellsToProcessOnStart.Clear();
        }

        private void ExtCalculateOuterBeach()
        {
            ConcurrentBag<Point> beachEdgePointListRaw = this.GetAllRawCoordinatesWithExtProperty(nameToUse: ExtBoardProps.Name.OuterBeach, value: true);

            byte minVal = Terrain.waterLevelMax;
            byte maxVal = (byte)(Terrain.waterLevelMax + 5);

            this.FloodFillExtProps(
                 startingPoints: beachEdgePointListRaw,
                 terrainName: Terrain.Name.Height,
                 minVal: minVal, maxVal: maxVal,
                 nameToSetIfInRange: ExtBoardProps.Name.OuterBeach,
                 setNameIfOutsideRange: false,
                 nameToSetIfOutsideRange: ExtBoardProps.Name.OuterBeach // doesn't matter, if setNameIfOutsideRange is false
                 );

            this.cellsToProcessOnStart.Clear();
        }

        private void ExtCalculateBiomes()
        {
            byte minVal = Terrain.biomeMin;
            byte maxVal = 255;

            if (!tempRawPointsForBiomeCreation.Any())
            {
                this.tempRawPointsForBiomeCreation = this.GetAllRawCoordinatesWithRangeAndWithoutSpecifiedExtProps(terrainName: Terrain.Name.Biome, minVal: minVal, maxVal: maxVal, extPropNames: ExtBoardProps.allBiomes);
            }
            else
            {
                this.tempRawPointsForBiomeCreation = this.FilterPointsWithoutSpecifiedExtProps(oldPointBag: this.tempRawPointsForBiomeCreation, extPropNames: ExtBoardProps.allBiomes, xyRaw: true);
            }

            if (this.tempRawPointsForBiomeCreation.Any())
            {
                var biomeName = this.biomeCountByName.OrderBy(kvp => kvp.Value).First().Key;
                this.biomeCountByName[biomeName]++;

                ConcurrentBag<Point> biomePoints = this.FloodFillExtProps(
                      startingPoints: new ConcurrentBag<Point> { this.tempRawPointsForBiomeCreation.First() },
                      terrainName: Terrain.Name.Biome,
                      minVal: minVal, maxVal: maxVal,
                      nameToSetIfInRange: biomeName,
                      setNameIfOutsideRange: false,
                      nameToSetIfOutsideRange: biomeName // doesn't matter, if setNameIfOutsideRange is false
                      );

                Parallel.ForEach(biomePoints, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, point =>
                {
                    this.tempPointsForCreatedBiomes[biomeName].Add(point);
                });

                int cellsToRemove = this.cellsToProcessOnStart.Count / 6;
                if (cellsToRemove > 0) this.cellsToProcessOnStart.RemoveRange(0, cellsToRemove); // to update progress bar
            }
            else this.cellsToProcessOnStart.Clear();
        }

        private void ExtApplyBiomeConstrains()
        {
            foreach (var kvp in this.tempPointsForCreatedBiomes)
            {
                ExtBoardProps.Name biomeName = kvp.Key;
                ConcurrentBag<Point> biomePoints = kvp.Value;

                if (ExtBoardProps.biomeConstrains.ContainsKey(biomeName))
                {
                    ConcurrentBag<Point> pointsToRemoveFromBiome = this.FilterPointsOutsideBiomeConstrains(oldPointBag: biomePoints, constrainsList: ExtBoardProps.biomeConstrains[biomeName], xyRaw: false);

                    Parallel.ForEach(pointsToRemoveFromBiome, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, point =>
                    {
                        this.SetExtProperty(name: biomeName, value: false, x: point.X, y: point.Y); // can write to array using parallel, if every thread accesses its own indices
                    });
                }
            }

            this.cellsToProcessOnStart.Clear();
        }

        private void ExtEndCreation()
        {
            this.extBoardProps.EndCreationAndSave();

            this.tempRawPointsForBiomeCreation = null;
            this.tempPointsForCreatedBiomes = null;
            this.cellsToProcessOnStart.Clear();
        }

        private ConcurrentBag<Point> GetAllRawCoordinatesWithExtProperty(ExtBoardProps.Name nameToUse, bool value)
        {
            var pointBag = new ConcurrentBag<Point>();

            Parallel.For(0, this.world.width / this.resDivider, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, rawX =>
            {
                for (int rawY = 0; rawY < this.world.height / this.resDivider; rawY++)
                {
                    if (this.GetExtProperty(name: nameToUse, x: rawX * this.resDivider, y: rawY * this.resDivider) == value)
                    {
                        pointBag.Add(new Point(rawX, rawY));
                    }
                }
            });

            return pointBag;
        }

        private ConcurrentBag<Point> GetAllRawCoordinatesWithRangeAndWithoutSpecifiedExtProps(Terrain.Name terrainName, byte minVal, byte maxVal, List<ExtBoardProps.Name> extPropNames)
        {
            var pointBag = new ConcurrentBag<Point>();

            Parallel.For(0, this.world.width / this.resDivider, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, rawX =>
            {
                for (int rawY = 0; rawY < this.world.height / this.resDivider; rawY++)
                {
                    byte value = this.GetFieldValue(terrainName: terrainName, x: rawX * this.resDivider, y: rawY * this.resDivider);

                    if (minVal <= value && value <= maxVal)
                    {
                        bool extPropsFound = false;

                        var extDataValDict = this.GetExtValueDict(x: rawX * this.resDivider, y: rawY * this.resDivider);
                        foreach (var kvp in extDataValDict)
                        {
                            if (kvp.Value && extPropNames.Contains(kvp.Key))
                            {
                                extPropsFound = true;
                                break;
                            }
                        }

                        if (!extPropsFound) pointBag.Add(new Point(rawX, rawY));
                    }
                }
            });

            return pointBag;
        }

        private ConcurrentBag<Point> FilterPointsWithoutSpecifiedExtProps(ConcurrentBag<Point> oldPointBag, List<ExtBoardProps.Name> extPropNames, bool xyRaw)
        {
            var newPointBag = new ConcurrentBag<Point>();

            Parallel.ForEach(oldPointBag, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentPoint =>
            {
                var extPropsFound = false;

                int x = xyRaw ? currentPoint.X * this.resDivider : currentPoint.X;
                int y = xyRaw ? currentPoint.Y * this.resDivider : currentPoint.Y;

                var extDataValDict = this.GetExtValueDict(x: x, y: y);
                foreach (var kvp in extDataValDict)
                {
                    if (kvp.Value && extPropNames.Contains(kvp.Key))
                    {
                        extPropsFound = true;
                        break;
                    }
                }

                if (!extPropsFound) newPointBag.Add(currentPoint);
            });

            return newPointBag;
        }

        private ConcurrentBag<Point> FilterPointsOutsideBiomeConstrains(ConcurrentBag<Point> oldPointBag, List<BiomeConstrain> constrainsList, bool xyRaw)
        {
            var newPointBag = new ConcurrentBag<Point>();

            Parallel.ForEach(oldPointBag, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentPoint =>
            {
                bool withinConstrains = true;

                foreach (BiomeConstrain constrain in constrainsList)
                {
                    int x = xyRaw ? currentPoint.X * this.resDivider : currentPoint.X;
                    int y = xyRaw ? currentPoint.Y * this.resDivider : currentPoint.Y;

                    byte value = this.GetFieldValue(terrainName: constrain.terrainName, x: x, y: y);

                    if (value < constrain.min || value > constrain.max)
                    {
                        withinConstrains = false;
                        break;
                    }
                }

                if (!withinConstrains) newPointBag.Add(currentPoint);
            });

            return newPointBag;
        }

        private ConcurrentBag<Point> FloodFillExtProps(ConcurrentBag<Point> startingPoints, Terrain.Name terrainName, byte minVal, byte maxVal, ExtBoardProps.Name nameToSetIfInRange, bool setNameIfOutsideRange, ExtBoardProps.Name nameToSetIfOutsideRange)
        {
            int dividedWidth = this.world.width / this.resDivider;
            int dividedHeight = this.world.height / this.resDivider;

            List<Point> offsetList = new List<Point> {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1),
            };

            bool[,] processedMap = new bool[dividedWidth, dividedHeight]; // working inside "compressed" map space, to avoid unnecessary calculations

            var nextPoints = new ConcurrentBag<Point>(startingPoints);
            var pointsInsideRange = new ConcurrentBag<Point>();

            while (true)
            {
                List<Point> currentPoints = nextPoints.Distinct().ToList(); // using Distinct() to filter out duplicates
                nextPoints = new ConcurrentBag<Point>(); // because there is no Clear() for ConcurrentBag

                Parallel.ForEach(currentPoints, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, currentPoint =>
                {
                    // array can be written to using parallel, if every thread accesses its own indices

                    int realX = currentPoint.X * this.resDivider;
                    int realY = currentPoint.Y * this.resDivider;

                    byte value = this.GetFieldValue(terrainName: terrainName, x: realX, y: realY);

                    if (minVal <= value && value <= maxVal) // point within range
                    {
                        pointsInsideRange.Add(new Point(realX, realY));
                        this.SetExtProperty(name: nameToSetIfInRange, value: true, x: realX, y: realY);

                        foreach (Point currentOffset in offsetList)
                        {
                            Point nextPoint = new Point(currentPoint.X + currentOffset.X, currentPoint.Y + currentOffset.Y);

                            if (nextPoint.X >= 0 && nextPoint.X < dividedWidth &&
                                nextPoint.Y >= 0 && nextPoint.Y < dividedHeight &&
                                !processedMap[nextPoint.X, nextPoint.Y])
                            {
                                nextPoints.Add(nextPoint);
                            }
                        }
                    }
                    else
                    {
                        if (setNameIfOutsideRange) this.SetExtProperty(name: nameToSetIfOutsideRange, value: true, x: realX, y: realY);
                    }

                    processedMap[currentPoint.X, currentPoint.Y] = true;
                });

                if (!nextPoints.Any()) break;
            }

            return pointsInsideRange;
        }

        public List<Cell> GetCellsForPieceName(PieceTemplate.Name pieceName)
        {
            return this.cellListsForPieceNames[pieceName].ToList(); // ToList() - to avoid modifying original list
        }

        public Cell GetRandomCellForPieceName(PieceTemplate.Name pieceName) // to avoid calling GetCellsForPieceName() for getting one random cell only
        {
            var cellList = this.cellListsForPieceNames[pieceName];

            if (!cellList.Any())
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"No cells suitable for creation of {PieceInfo.GetInfo(pieceName).readableName}.", avoidDuplicates: true);
                return this.allCells[0]; // to properly return a (useless) cell
            }

            return cellList[this.world.random.Next(0, cellList.Count)];
        }

        private void PrepareNextStage()
        {
            this.stageStartFrame = SonOfRobinGame.CurrentUpdate;
            this.stageStartTime = DateTime.Now;
            this.cellsToProcessOnStart.Clear();
            this.cellsToProcessOnStart.AddRange(this.allCells);
        }

        private void UpdateProgressBar()
        {
            if (this.world.demoMode) return;

            TimeSpan timeLeft = CalculateTimeLeft(startTime: this.stageStartTime, completeAmount: this.allCells.Count - this.cellsToProcessOnStart.Count, totalAmount: this.allCells.Count);
            string timeLeftString = timeLeft == TimeSpan.FromSeconds(0) ? "" : TimeSpanToString(timeLeft + TimeSpan.FromSeconds(1));

            string seedText = String.Format("{0:0000}", this.world.seed);
            string message = $"preparing island\nstep {(int)this.currentStage + 1}/{allStagesCount}\nseed {seedText}\n{this.world.width} x {this.world.height}\n{namesForStages[this.currentStage]} {timeLeftString}";

            SonOfRobinGame.ProgressBar.TurnOn(
                            curVal: this.allCells.Count - this.cellsToProcessOnStart.Count,
                            maxVal: this.allCells.Count,
                            text: message);
        }

        private static TimeSpan CalculateTimeLeft(DateTime startTime, int completeAmount, int totalAmount)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            try
            {
                TimeSpan timePerCell = TimeSpan.FromSeconds(elapsedTime.TotalSeconds / completeAmount);
                TimeSpan timeLeft = TimeSpan.FromSeconds(timePerCell.TotalSeconds * (totalAmount - completeAmount));

                return timeLeft;
            }
            catch (OverflowException)
            { return TimeSpan.FromMinutes(0); }
        }

        public void AddToGroup(Sprite sprite, Cell.Group groupName)
        {
            if (!sprite.gridGroups.Contains(groupName)) sprite.gridGroups.Add(groupName);
            if (sprite.currentCell == null) return;

            sprite.currentCell.AddToGroup(sprite: sprite, groupName: groupName);
        }

        public void RemoveFromGroup(Sprite sprite, Cell.Group groupName)
        {
            if (sprite.gridGroups.Contains(groupName)) sprite.gridGroups.Remove(groupName);
            if (sprite.currentCell == null) return;

            sprite.currentCell.RemoveFromGroup(sprite: sprite, groupName: groupName);
        }

        public void AddSprite(Sprite sprite)
        {
            var correctCellLocation = this.FindMatchingCell(sprite.position);

            sprite.currentCell = correctCellLocation;
            sprite.currentCell.UpdateGroups(sprite: sprite, groupNames: sprite.gridGroups);
        }

        public void UpdateLocation(Sprite sprite)
        {
            var newCell = this.FindMatchingCell(position: sprite.position);

            if (sprite.currentCell == newCell) return;
            else if (sprite.currentCell == null)
            {
                this.AddSprite(sprite: sprite);
                return;
            }

            sprite.currentCell.MoveSpriteToOtherCell(sprite: sprite, newCell: newCell);
            sprite.currentCell = newCell;
        }

        public void RemoveSprite(Sprite sprite)
        {
            if (sprite.currentCell == null) return;
            sprite.currentCell.RemoveSprite(sprite);
        }

        public List<Sprite> GetSpritesFromSurroundingCells(Sprite sprite, Cell.Group groupName)
        {
            Cell cell = sprite.currentCell == null ? this.FindMatchingCell(sprite.position) : sprite.currentCell;
            return cell.GetSpritesFromSurroundingCells(groupName);
        }

        public List<Cell> GetCellsInsideRect(Rectangle viewRect, bool addPadding = true)
        {
            // addPadding: +1 cell on each side, to ensure visibility of sprites, that cross their cells' boundaries
            int padding = addPadding ? 1 : 0;

            int xMinCellNo = Math.Max(FindMatchingCellInSingleAxis(position: viewRect.Left, cellLength: this.cellWidth) - padding, 0);
            int xMaxCellNo = Math.Min(FindMatchingCellInSingleAxis(position: viewRect.Right, cellLength: this.cellWidth) + padding, this.noOfCellsX - 1);
            int yMinCellNo = Math.Max(FindMatchingCellInSingleAxis(position: viewRect.Top, cellLength: this.cellHeight) - padding, 0);
            int yMaxCellNo = Math.Min(FindMatchingCellInSingleAxis(position: viewRect.Bottom, cellLength: this.cellHeight) + padding, this.noOfCellsY - 1);

            List<Cell> cellsInsideRect = new List<Cell>();

            for (int x = xMinCellNo; x <= xMaxCellNo; x++)
            {
                for (int y = yMinCellNo; y <= yMaxCellNo; y++)
                { cellsInsideRect.Add(this.cellGrid[x, y]); }
            }

            return cellsInsideRect;
        }

        public List<Cell> GetCellsWithinDistance(Vector2 position, ushort distance)
        {
            List<Cell> cellsWithinDistance = new List<Cell>();

            int xMin = Math.Min(Math.Max(Convert.ToInt32(position.X - distance), 0), this.world.width - 1);
            int xMax = Math.Min(Math.Max(Convert.ToInt32(position.X + distance), 0), this.world.width - 1);
            int yMin = Math.Min(Math.Max(Convert.ToInt32(position.Y - distance), 0), this.world.height - 1);
            int yMax = Math.Min(Math.Max(Convert.ToInt32(position.Y + distance), 0), this.world.height - 1);

            int xMinCellNo = FindMatchingCellInSingleAxis(position: xMin, cellLength: this.cellWidth);
            int xMaxCellNo = FindMatchingCellInSingleAxis(position: xMax, cellLength: this.cellWidth);
            int yMinCellNo = FindMatchingCellInSingleAxis(position: yMin, cellLength: this.cellHeight);
            int yMaxCellNo = FindMatchingCellInSingleAxis(position: yMax, cellLength: this.cellHeight);

            for (int x = xMinCellNo; x <= xMaxCellNo; x++)
            {
                for (int y = yMinCellNo; y <= yMaxCellNo; y++)
                { cellsWithinDistance.Add(this.cellGrid[x, y]); }
            }

            return cellsWithinDistance;
        }

        public List<Sprite> GetSpritesWithinDistance(Sprite mainSprite, ushort distance, Cell.Group groupName)
        {
            var cellsWithinDistance = this.GetCellsWithinDistance(position: mainSprite.position, distance: distance);
            var spritesWithinDistance = new List<Sprite>();

            foreach (Cell cell in cellsWithinDistance)
            {
                foreach (Sprite currentSprite in cell.spriteGroups[groupName].Values)
                {
                    if (Vector2.Distance(currentSprite.position, mainSprite.position) <= distance && currentSprite != mainSprite) spritesWithinDistance.Add(currentSprite);
                }
            }

            return spritesWithinDistance;
        }

        public List<BoardPiece> GetPiecesWithinDistance(Sprite mainSprite, ushort distance, Cell.Group groupName, int offsetX = 0, int offsetY = 0, bool compareWithBottom = false)
        {
            var cellsWithinDistance = this.GetCellsWithinDistance(position: mainSprite.position, distance: distance);
            var spritesWithinDistance = new List<Sprite>();

            Vector2 centerPos = mainSprite.position + new Vector2(offsetX, offsetY);

            if (compareWithBottom)
            {
                foreach (Cell cell in cellsWithinDistance)
                {
                    spritesWithinDistance.AddRange(
                        cell.spriteGroups[groupName].Values.Where(
                            currentSprite => Vector2.Distance(new Vector2(currentSprite.gfxRect.Center.X, currentSprite.gfxRect.Bottom), centerPos) <= distance &&
                            currentSprite != mainSprite).ToList());
                }
            }
            else
            {
                foreach (Cell cell in cellsWithinDistance)
                {
                    spritesWithinDistance.AddRange(
                        cell.spriteGroups[groupName].Values.Where(
                            currentSprite => Vector2.Distance(currentSprite.position, centerPos) <= distance &&
                            currentSprite != mainSprite).ToList());
                }
            }

            var piecesWithinDistance = spritesWithinDistance.Select(sprite => sprite.boardPiece).ToList();
            return piecesWithinDistance;
        }

        public List<BoardPiece> GetPiecesInsideTriangle(Point point1, Point point2, Point point3, Cell.Group groupName)
        {
            int xMin = (int)Math.Min(Math.Min(point1.X, point2.X), point3.X);
            int xMax = (int)Math.Max(Math.Max(point1.X, point2.X), point3.X);
            int yMin = (int)Math.Min(Math.Min(point1.Y, point2.Y), point3.Y);
            int yMax = (int)Math.Max(Math.Max(point1.Y, point2.Y), point3.Y);

            Rectangle rect = new Rectangle(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);

            var cellsInsideRect = this.GetCellsInsideRect(rect);

            var spritesInsideTriangle = new List<Sprite>();
            foreach (Cell cell in cellsInsideRect)
            {
                spritesInsideTriangle.AddRange(cell.spriteGroups[groupName].Values.Where(
                      currentSprite => rect.Contains(currentSprite.position) && Helpers.IsPointInsideTriangle(point: new Point((int)currentSprite.position.X, (int)currentSprite.position.Y), triangleA: point1, triangleB: point2, triangleC: point3)).ToList());
            }

            var piecesInsideTriangle = spritesInsideTriangle.Select(sprite => sprite.boardPiece).ToList();
            return piecesInsideTriangle;
        }

        public void GetSpritesInCameraViewAndPutIntoList(List<Sprite> spriteListToFill, Camera camera, Cell.Group groupName, bool compareWithCameraRect = false)
        {
            spriteListToFill.Clear();

            var visibleCells = this.GetCellsInsideRect(camera.viewRect);

            if (compareWithCameraRect)
            {
                foreach (Cell cell in visibleCells) // making sure that every piece is actually inside camera rect
                {
                    spriteListToFill.AddRange(cell.spriteGroups[groupName].Values.Where(sprite => camera.viewRect.Intersects(sprite.gfxRect)));
                }
            }
            else
            {
                foreach (Cell cell in visibleCells) // visibleCells area is larger than camera view
                {
                    spriteListToFill.AddRange(cell.spriteGroups[groupName].Values);
                }
            }
        }

        private List<Cell> GetAllCells()
        {
            List<Cell> allCells = new List<Cell>();

            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                { allCells.Add(this.cellGrid[x, y]); }
            }
            return allCells;
        }

        public ConcurrentBag<Sprite> GetSpritesFromAllCells(Cell.Group groupName, bool visitedByPlayerOnly = false)
        {
            var cells = this.allCells;
            if (visitedByPlayerOnly) cells = this.allCells.Where(cell => cell.VisitedByPlayer).ToList();

            var allSprites = new ConcurrentBag<Sprite> { };

            Parallel.ForEach(cells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName].Values)
                {
                    allSprites.Add(sprite);
                }
            });

            return allSprites;
        }

        public ConcurrentBag<Sprite> GetSpritesForRect(Cell.Group groupName, Rectangle rectangle, bool visitedByPlayerOnly = false)
        {
            var cells = this.GetCellsInsideRect(rectangle);
            if (visitedByPlayerOnly) cells = cells.Where(cell => cell.VisitedByPlayer).ToList();

            var allSprites = new ConcurrentBag<Sprite> { };

            Parallel.ForEach(cells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName].Values)
                {
                    allSprites.Add(sprite);
                }
            });

            return allSprites;
        }

        public bool SpecifiedPiecesCountIsMet(Dictionary<PieceTemplate.Name, int> piecesToCount)
        {
            var countByName = this.CountSpecifiedPieces(piecesToCount.Keys.ToList());

            foreach (var kvp in piecesToCount)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                if (countByName[pieceName] < kvp.Value) return false;
            }

            return true;
        }

        public Dictionary<PieceTemplate.Name, int> CountSpecifiedPieces(List<PieceTemplate.Name> nameList)
        {
            var countByName = new Dictionary<PieceTemplate.Name, int>();
            foreach (PieceTemplate.Name name in nameList) countByName[name] = 0;

            foreach (var sprite in this.GetSpritesFromAllCells(Cell.Group.All))
            {
                if (nameList.Contains(sprite.boardPiece.name) && sprite.boardPiece.exists) countByName[sprite.boardPiece.name]++;
            }

            return countByName;
        }

        public void DrawBackground(Camera camera)
        {
            bool updateFog = false;
            Rectangle cameraRect = camera.viewRect;

            var visibleCells = this.GetCellsInsideRect(camera.viewRect);
            foreach (Cell cell in visibleCells)
            {
                cell.DrawBackground(drawSimulation: true);

                if (this.world.MapEnabled && !cell.VisitedByPlayer && cameraRect.Intersects(cell.rect) && camera.IsTrackingPlayer && this.world.Player.CanSeeAnything)
                {
                    cell.SetAsVisited();
                    updateFog = true;
                }
            }

            if (updateFog) this.world.map.dirtyFog = true;
        }

        public int DrawSprites(Camera camera, List<Sprite> blockingLightSpritesList)
        {
            if (Preferences.drawSunShadows)
            {
                AmbientLight.SunLightData sunLightData = AmbientLight.SunLightData.CalculateSunLight(this.world.islandClock.IslandDateTime);
                if (sunLightData.sunShadowsColor != Color.Transparent)
                {
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        Vector2 sunPos = new Vector2(shadowSprite.gfxRect.Center.X + sunLightData.sunPos.X, shadowSprite.gfxRect.Bottom + sunLightData.sunPos.Y);
                        float shadowAngle = Helpers.GetAngleBetweenTwoPoints(start: sunPos, end: shadowSprite.position);

                        Sprite.DrawShadow(color: sunLightData.sunShadowsColor, shadowSprite: shadowSprite, lightPos: sunPos, shadowAngle: shadowAngle, yScaleForce: sunLightData.sunShadowsLength);
                    }
                }
            }

            // Sprites should be drawn all at once, because cell-based drawing causes Y sorting order incorrect
            // in cases of sprites overlapping cell boundaries.

            var visibleSprites = camera.GetVisibleSprites(groupName: Cell.Group.Visible, compareWithCameraRect: true);

            foreach (Sprite sprite in visibleSprites.OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom))
            { sprite.Draw(); }

            StatBar.DrawAll();

            return visibleSprites.Count;
        }

        public void DrawDebugData(bool drawCellData, bool drawPieceData)
        {
            if (!drawCellData && !drawPieceData) return;

            var visibleCells = this.GetCellsInsideRect(this.world.camera.viewRect);

            foreach (Cell cell in visibleCells)
            {
                cell.DrawDebugData(groupName: Cell.Group.ColMovement, drawCellData: drawCellData, drawPieceData: drawPieceData);
            }
        }

        private static int FindMatchingCellInSingleAxis(int position, int cellLength)
        {
            return position / cellLength;
        }

        public Cell FindMatchingCell(Vector2 position)
        {
            int cellNoX = (int)Math.Floor(position.X / this.cellWidth);
            int cellNoY = (int)Math.Floor(position.Y / this.cellHeight);
            return this.cellGrid[cellNoX, cellNoY];
        }

        public byte GetMinValueForCell(Terrain.Name terrainName, int cellNoX, int cellNoY)
        {
            return this.terrainByName[terrainName].GetMinValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);
        }

        public byte GetMaxValueForCell(Terrain.Name terrainName, int cellNoX, int cellNoY)
        {
            return this.terrainByName[terrainName].GetMaxValueForCell(cellNoX: cellNoX, cellNoY: cellNoY);
        }

        public bool CheckIfContainsExtPropertyForCell(ExtBoardProps.Name name, bool value, int cellNoX, int cellNoY)
        {
            return this.extBoardProps.CheckIfContainsPropertyForCell(name: name, value: value, cellNoX: cellNoX, cellNoY: cellNoY);
        }

        public byte GetFieldValue(Terrain.Name terrainName, int x, int y, bool xyRaw = false)
        {
            if (xyRaw) return this.terrainByName[terrainName].GetMapDataRaw(x, y);
            else return this.terrainByName[terrainName].GetMapData(x, y);
        }

        public byte GetFieldValue(Terrain.Name terrainName, Vector2 position, bool xyRaw = false)
        {
            if (xyRaw) return this.terrainByName[terrainName].GetMapDataRaw((int)position.X, (int)position.Y);
            else return this.terrainByName[terrainName].GetMapData((int)position.X, (int)position.Y);
        }

        public Dictionary<ExtBoardProps.Name, bool> GetExtValueDict(int x, int y)
        {
            return this.extBoardProps.GetValueDict(x: x, y: y, xyRaw: false);
        }

        public bool GetExtProperty(ExtBoardProps.Name name, Vector2 position)
        {
            return this.extBoardProps.GetValue(name: name, x: (int)position.X, y: (int)position.Y, xyRaw: false);
        }

        public bool GetExtProperty(ExtBoardProps.Name name, int x, int y)
        {
            return this.extBoardProps.GetValue(name: name, x: x, y: y, xyRaw: false);
        }

        public void SetExtProperty(ExtBoardProps.Name name, bool value, int x, int y)
        {
            this.extBoardProps.SetValue(name: name, value: value, x: x, y: y, xyRaw: false);
        }

        private Cell[,] MakeGrid()
        {
            Cell[,] cellGrid = new Cell[this.noOfCellsX, this.noOfCellsY];

            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                {
                    cellGrid[x, y] = new Cell(grid: this, cellNoX: x, cellNoY: y, cellWidth: this.cellWidth, cellHeight: this.cellHeight, random: this.world.random);
                }
            }

            return cellGrid;
        }

        private void CalculateSurroundingCells() // surrouding cells are calculated only once, because they do not change
        {
            foreach (Cell cell in this.allCells)
            { cell.surroundingCells = GetCellsWithinDistance(cell: cell, distance: 1); }
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1))
            { timeLeftString = timeSpan.ToString("ss"); }
            else if (timeSpan < TimeSpan.FromHours(1))
            { timeLeftString = timeSpan.ToString("mm\\:ss"); }
            else if (timeSpan < TimeSpan.FromDays(1))
            { timeLeftString = timeSpan.ToString("hh\\:mm\\:ss"); }
            else
            { timeLeftString = timeSpan.ToString("dd\\:hh\\:mm\\:ss"); }

            return timeLeftString;
        }

        private List<Cell> GetCellsWithinDistance(Cell cell, int distance)
        {
            List<Cell> cellsWithinDistance = new List<Cell>();

            for (int offsetX = -distance; offsetX < distance + 1; offsetX++)
            {
                for (int offsetY = -1; offsetY < distance + 1; offsetY++)
                {
                    int currentCellX = cell.cellNoX + offsetX;
                    int currentCellY = cell.cellNoY + offsetY;
                    if (currentCellX >= 0 && currentCellX < this.noOfCellsX && currentCellY >= 0 && currentCellY < this.noOfCellsY)
                    { cellsWithinDistance.Add(this.cellGrid[currentCellX, currentCellY]); }
                }
            }

            return cellsWithinDistance;
        }

        private static Vector2 CalculateMaxFrameSize()
        {
            int frameMaxWidth = 0;
            int frameMaxHeight = 0;

            foreach (var frameList in AnimData.frameListById)
            {
                foreach (AnimFrame frame in frameList.Value)
                {
                    int scaledWidth = (int)(frame.gfxWidth * frame.scale);
                    int scaledHeight = (int)(frame.gfxHeight * frame.scale);

                    if (scaledWidth > frameMaxWidth) frameMaxWidth = scaledWidth;
                    if (scaledHeight > frameMaxHeight) frameMaxHeight = scaledHeight;
                }
            }

            return new Vector2(frameMaxWidth, frameMaxHeight);
        }

        public void LoadClosestTexturesInCameraView(Camera camera, bool visitedByPlayerOnly, bool loadMoreThanOne)
        {
            if (Preferences.loadWholeMap || SonOfRobinGame.LastUpdateDelay > 20) return;

            while (true)
            {
                var cellsInCameraViewWithNoTextures = this.GetCellsInsideRect(camera.viewRect).Where(cell => cell.boardGraphics.Texture == null);
                if (visitedByPlayerOnly) cellsInCameraViewWithNoTextures = cellsInCameraViewWithNoTextures.Where(cell => cell.VisitedByPlayer);
                if (!cellsInCameraViewWithNoTextures.Any()) return;

                Vector2 cameraCenter = camera.CurrentPos;

                var cellsByDistance = cellsInCameraViewWithNoTextures.OrderBy(cell => cell.GetDistance(cameraCenter));
                cellsByDistance.First().boardGraphics.LoadTexture();

                if (!loadMoreThanOne || Scene.UpdateTimeElapsed.Milliseconds > 10) return;
            }
        }

        public void LoadAllTexturesInCameraView()
        // cannot be processed using parallel (textures don't work in parallel processing)
        {
            if (Preferences.loadWholeMap) return;

            foreach (Cell cell in this.GetCellsInsideRect(this.world.camera.viewRect))
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Processing cell in camera view {cell.cellNoX},{cell.cellNoY}.", color: Color.White);
                cell.boardGraphics.LoadTexture();
            }
        }

        public void UnloadTexturesIfMemoryLow(Camera camera)
        {
            if (Preferences.loadWholeMap) return;

            switch (SonOfRobinGame.platform)
            {
                case Platform.Desktop:
                    if (!SonOfRobinGame.DesktopMemoryLow) return;
                    break;

                case Platform.Mobile:
                    if (this.loadedTexturesCount < Preferences.mobileMaxLoadedTextures) return;
                    break;

                default:
                    throw new ArgumentException($"Textures unloading - unsupported platform {SonOfRobinGame.platform}.");
            }

            var cellsInCameraView = this.GetCellsInsideRect(camera.viewRect);
            var cellsToUnload = this.allCells.Where(cell => !cellsInCameraView.Contains(cell) && cell.boardGraphics.Texture != null).ToList();

            foreach (Cell cell in cellsToUnload)
            {
                cell.boardGraphics.UnloadTexture();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Unloaded texture from cell {cell.cellNoX},{cell.cellNoY}.", color: Color.Pink);
            }
            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Finished unloading textures.", color: Color.Pink);
            GC.Collect();
        }

        public void UnloadAllTextures()
        {
            foreach (Cell cell in this.allCells)
            {
                cell.boardGraphics.UnloadTexture();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Unloaded texture from cell {cell.cellNoX},{cell.cellNoY}.", color: Color.Pink);
            }
            GC.Collect();
        }
    }
}