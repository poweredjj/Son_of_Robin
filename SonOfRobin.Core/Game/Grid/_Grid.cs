using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SixLabors.ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SonOfRobin
{
    public class Grid
    {
        public enum Stage : byte
        {
            TerrainLoad,
            TerrainGenerate,
            TerrainUpdateMinMax,
            TerrainSave,
            ExtDataCheck,
            ExtDataSetSea,
            ExtDataSetBeach,
            ExtDataSetBiomes,
            ExtDataSetPropertiesGrid,
            ExtDataSetFinish,
            NamedLocationsGenerate,
            MeshesLoad,
            MeshesGenerate,
            AllowedNamesFill,
        }

        public static readonly int allStagesCount = ((Stage[])Enum.GetValues(typeof(Stage))).Length;

        private static readonly Dictionary<Stage, string> namesForStages = new()
        {
            { Stage.TerrainLoad, "loading terrain" },
            { Stage.TerrainGenerate, "generating terrain (values)" },
            { Stage.TerrainUpdateMinMax, "generating terrain (min + max)" },
            { Stage.TerrainSave, "saving terrain" },
            { Stage.ExtDataCheck, "loading extended data" },
            { Stage.ExtDataSetSea, "setting extended data (sea)" },
            { Stage.ExtDataSetBeach, "setting extended data (beach)" },
            { Stage.ExtDataSetBiomes, "setting extended data (biomes)" },
            { Stage.ExtDataSetPropertiesGrid, "setting extended data (properties grid)" },
            { Stage.ExtDataSetFinish, "saving extended data" },
            { Stage.NamedLocationsGenerate, "generating named locations" },
            { Stage.MeshesLoad, "loading meshes" },
            { Stage.MeshesGenerate, "generating meshes" },
            { Stage.AllowedNamesFill, "filling lists of allowed names" },
        };

        private Task backgroundTask;
        private Stage currentStage;
        private DateTime stageStartTime;
        public readonly bool serializable;
        public bool CreationInProgress { get; private set; }

        public readonly GridTemplate gridTemplate;
        public readonly World world;
        public readonly Level level;

        public readonly int width;
        public readonly int height;
        public readonly int dividedWidth;
        public readonly int dividedHeight;

        public readonly int noOfCellsX;
        public readonly int noOfCellsY;
        public readonly int cellWidth;
        public readonly int cellHeight;
        public readonly int resDivider;
        public Texture2D WholeIslandPreviewTexture { get; private set; }

        public readonly Dictionary<Terrain.Name, Terrain> terrainByName;
        public ExtBoardProps ExtBoardProps { get; private set; }
        public readonly NamedLocations namedLocations;
        public MeshGrid MeshGrid { get; private set; }
        private bool meshGridLoaded;

        public readonly Cell[,] cellGrid;
        public readonly Cell[] allCells;

        private readonly Dictionary<PieceTemplate.Name, Cell[]> cellArraysForPieceNamesMasterDict; // pieces and cells that those pieces can (initially) be placed into
        private readonly Dictionary<PieceTemplate.Name, Queue<Cell>> cellSetsForPieceNamesWorkingQueueDict;

        public Grid(Level level, int resDivider, int cellWidth = 0, int cellHeight = 0)
        {
            this.CreationInProgress = true;
            this.currentStage = 0;

            this.level = level;
            this.world = this.level.world;
            this.resDivider = resDivider;
            this.terrainByName = new Dictionary<Terrain.Name, Terrain>();
            this.namedLocations = new NamedLocations(grid: this);
            this.serializable = this.level.levelType == Level.LevelType.Island;

            this.width = this.level.width;
            this.height = this.level.height;
            this.dividedWidth = (int)Math.Ceiling((double)this.width / (double)this.resDivider);
            this.dividedHeight = (int)Math.Ceiling((double)this.height / (double)this.resDivider);

            if (cellWidth == 0 && cellHeight == 0)
            {
                Point cellSize = GridTemplate.ProperCellSize;
                this.cellWidth = cellSize.X;
                this.cellHeight = cellSize.Y;
            }
            else
            {
                this.cellWidth = cellWidth;
                this.cellHeight = cellHeight;
            }

            if (this.cellWidth % 2 != 0) throw new ArgumentException($"Cell width {this.cellWidth} is not divisible by 2.");
            if (this.cellHeight % 2 != 0) throw new ArgumentException($"Cell height {this.cellHeight} is not divisible by 2.");

            this.gridTemplate = new GridTemplate(seed: this.level.seed, width: this.level.width, height: this.level.height, cellWidth: this.cellWidth, cellHeight: this.cellHeight, resDivider: this.resDivider, createdDate: DateTime.Now, saveHeader: this.serializable);

            this.noOfCellsX = (int)Math.Ceiling((float)this.level.width / (float)this.cellWidth);
            this.noOfCellsY = (int)Math.Ceiling((float)this.level.height / (float)this.cellHeight);

            this.cellArraysForPieceNamesMasterDict = new Dictionary<PieceTemplate.Name, Cell[]>();
            this.cellSetsForPieceNamesWorkingQueueDict = new Dictionary<PieceTemplate.Name, Queue<Cell>>();
            this.cellGrid = this.MakeGrid();
            this.allCells = this.GetAllCells();
            this.CalculateSurroundingCells();
            this.meshGridLoaded = false;

            if (this.CopyBoardFromTemplate())
            {
                this.CreationInProgress = false;
                return;
            }

            this.PrepareNextStage(incrementCurrentStage: false);
        }

        public void Destroy()
        {
            // for properly disposing used objects
            this.WholeIslandPreviewTexture?.Dispose();
            this.WholeIslandPreviewTexture = null;
        }

        public IEnumerable<Cell> CellsVisitedByPlayer
        { get { return this.allCells.Where(cell => cell.visitedByPlayer); } }

        public IEnumerable<Cell> CellsNotVisitedByPlayer
        { get { return this.allCells.Where(cell => !cell.visitedByPlayer); } }

        public float VisitedCellsPercentage
        { get { return (float)this.allCells.Where(cell => cell.visitedByPlayer).Count() / (float)this.allCells.Length; } }

        public Dictionary<string, Object> Serialize()
        {
            // this data is included in save file (not in template)

            var cellsVisitedByPlayer = new ConcurrentBag<int> { };

            var cellGrid = this.cellGrid; // needed for parallel execution
            int noOfCellsX = this.noOfCellsX; // needed for parallel execution
            Parallel.For(0, this.noOfCellsY, SonOfRobinGame.defaultParallelOptions, cellNoY =>
            {
                for (int cellNoX = 0; cellNoX < noOfCellsX; cellNoX++)
                {
                    Cell cell = cellGrid[cellNoX, cellNoY];
                    if (cell.visitedByPlayer) cellsVisitedByPlayer.Add(cell.cellIndex);
                }
            });

            Dictionary<string, Object> gridData = new()
            {
                { "cellWidth", this.cellWidth },
                { "cellHeight", this.cellHeight },
                { "namedLocations", this.namedLocations.Serialize() },
                { "cellsVisitedByPlayer", cellsVisitedByPlayer },
            };

            return gridData;
        }

        public static Grid Deserialize(Dictionary<string, Object> gridData, Level level, int resDivider)
        {
            // this data is included in save file (not in template)

            int cellWidth = (int)(Int64)gridData["cellWidth"];
            int cellHeight = (int)(Int64)gridData["cellHeight"];

            Grid grid = new(level: level, cellWidth: cellWidth, cellHeight: cellHeight, resDivider: resDivider);
            grid.namedLocations.Deserialize(gridData["namedLocations"]);

            if (gridData.ContainsKey("cellsVisitedByPlayer"))
            {
                var cellsVisitedByPlayer = (ConcurrentBag<int>)gridData["cellsVisitedByPlayer"];

                Parallel.ForEach(cellsVisitedByPlayer, SonOfRobinGame.defaultParallelOptions, cellIndex =>
                {
                    grid.allCells[cellIndex].visitedByPlayer = true;
                });
            }

            return grid;
        }

        public static Grid GetMatchingTemplateFromSceneStack(int seed, int width, int height, int cellWidth = 0, int cellHeight = 0, bool ignoreCellSize = false)
        {
            var existingWorlds = Scene.GetAllScenesOfType(typeof(World));
            if (existingWorlds.Count == 0) return null;

            foreach (Scene scene in existingWorlds)
            {
                World existingWorld = (World)scene;
                Level level = existingWorld.IslandLevel;

                if (level != null &&
                    !level.creationInProgress &&
                    level.grid != null &&
                    !level.grid.CreationInProgress &&
                    seed == level.seed &&
                    width == level.width &&
                    height == level.height &&
                    (ignoreCellSize || (cellWidth == level.grid.cellWidth && cellHeight == level.grid.cellHeight)))
                {
                    return level.grid;
                }
            }

            return null;
        }

        public bool CopyBoardFromTemplate()
        {
            if (!this.serializable) return false;

            // looking for matching template

            Grid templateGrid = GetMatchingTemplateFromSceneStack(seed: this.level.seed, width: this.level.width, height: this.level.height, cellWidth: this.cellWidth, cellHeight: this.cellHeight);
            if (templateGrid == null) return false;

            // copying terrain

            foreach (var kvp in templateGrid.terrainByName)
            {
                Terrain.Name terrainName = kvp.Key;
                Terrain terrain = kvp.Value;
                this.terrainByName[terrainName] = terrain;
                this.terrainByName[terrainName].AttachToNewGrid(this);
            }

            // copying ext data

            this.ExtBoardProps = templateGrid.ExtBoardProps;
            this.ExtBoardProps.AttachToNewGrid(this);

            // copying whole island texture

            if (templateGrid.WholeIslandPreviewTexture != null)
            {
                this.WholeIslandPreviewTexture?.Dispose(); // just in case
                this.WholeIslandPreviewTexture = new Texture2D(templateGrid.WholeIslandPreviewTexture.GraphicsDevice, templateGrid.WholeIslandPreviewTexture.Width, templateGrid.WholeIslandPreviewTexture.Height);

                Color[] pixelData = new Color[templateGrid.WholeIslandPreviewTexture.Width * templateGrid.WholeIslandPreviewTexture.Height];
                templateGrid.WholeIslandPreviewTexture.GetData(pixelData);
                this.WholeIslandPreviewTexture.SetData(pixelData);
            }

            // copying named locations
            this.namedLocations.CopyLocationsFromTemplate(templateGrid.namedLocations);

            // copying meshes
            this.MeshGrid = templateGrid.MeshGrid;

            // copying cell data

            Parallel.For(0, templateGrid.noOfCellsX, SonOfRobinGame.defaultParallelOptions, x =>
            {
                for (int y = 0; y < templateGrid.noOfCellsY; y++)
                    this.cellGrid[x, y].CopyFromTemplate(templateGrid.cellGrid[x, y]);
            });

            // finishing

            this.FillCellListsForPieceNames();

            return true;
        }

        public void CompleteCreation()
        {
            this.UpdateProgressBar();

            if (this.backgroundTask != null && this.backgroundTask.IsFaulted)
            {
                SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: true);

                this.CreationInProgress = false;
            }

            if (this.backgroundTask != null && this.backgroundTask.IsCompleted) this.CreationInProgress = false;

            if (this.world.demoMode) this.ProcessAllCreationStages(); // demo mode must be processed normally
            else
            {
                if (this.backgroundTask == null) this.backgroundTask = Task.Run(() => this.ProcessAllCreationStages());
            }

            this.UpdateProgressBar(); // repeated
        }

        private void ProcessAllCreationStages()
        {
            while (this.CreationInProgress && !this.world.HasBeenRemoved)
            {
                this.ProcessOneCreationStage();
            }

            this.backgroundTask = null;
        }

        private void ProcessOneCreationStage()
        {
            switch (currentStage)
            {
                case Stage.TerrainLoad:
                    {
                        Level.LevelType levelType = this.world.ActiveLevel.levelType;
                        switch (levelType)
                        {
                            case Level.LevelType.Island:

                                this.terrainByName[Terrain.Name.Height] = new Terrain(
                                    grid: this, name: Terrain.Name.Height, frequency: 8f, octaves: 9, persistence: 0.5f, lacunarity: 1.9f, gain: 0.55f, addBorder: true);

                                this.terrainByName[Terrain.Name.Humidity] = new Terrain(
                                    grid: this, name: Terrain.Name.Humidity, frequency: 4.3f, octaves: 9, persistence: 0.6f, lacunarity: 1.7f, gain: 0.6f);

                                this.terrainByName[Terrain.Name.Biome] = new Terrain(
                                    grid: this, name: Terrain.Name.Biome, frequency: 7f, octaves: 3, persistence: 0.7f, lacunarity: 1.4f, gain: 0.3f, addBorder: true);

                                break;

                            case Level.LevelType.Cave:

                                this.terrainByName[Terrain.Name.Height] = new Terrain(
                                    grid: this, name: Terrain.Name.Height, frequency: 32f, octaves: 11, persistence: 0.58f, lacunarity: 1.8f, gain: 0.68f, addBorder: true, rangeConversions: new List<Terrain.RangeConversion> {
                                        new Terrain.RangeConversion(inMin: 0, inMax: 116, outMin: 0, outMax: 0),
                                        new Terrain.RangeConversion(inMin: 200, inMax: 255, outMin: Terrain.lavaMin, outMax: 255),
                                        new Terrain.RangeConversion(inMin: Terrain.rocksLevelMin, inMax: Terrain.lavaMin, outMin: Terrain.rocksLevelMin - 1, outMax: Terrain.rocksLevelMin - 1) });

                                this.terrainByName[Terrain.Name.Humidity] = new Terrain(
                                    grid: this, name: Terrain.Name.Humidity, frequency: 4.3f, octaves: 9, persistence: 0.6f, lacunarity: 1.7f, gain: 0.6f);

                                this.terrainByName[Terrain.Name.Biome] = new Terrain(
                                    grid: this, name: Terrain.Name.Biome, frequency: 7f, octaves: 3, persistence: 0.7f, lacunarity: 1.4f, gain: 0.3f, addBorder: true);

                                break;

                            case Level.LevelType.OpenSea:

                                this.terrainByName[Terrain.Name.Height] = new Terrain(
                                    grid: this, name: Terrain.Name.Height, frequency: 0f, octaves: 0, persistence: 0f, lacunarity: 0f, gain: 0f, filledWithValue: true, valueToFill: Terrain.waterLevelMax);

                                this.terrainByName[Terrain.Name.Humidity] = new Terrain(
                                    grid: this, name: Terrain.Name.Height, frequency: 0f, octaves: 0, persistence: 0f, lacunarity: 0f, gain: 0f, filledWithValue: true, valueToFill: 0);

                                this.terrainByName[Terrain.Name.Biome] = new Terrain(
                                    grid: this, name: Terrain.Name.Height, frequency: 0f, octaves: 0, persistence: 0f, lacunarity: 0f, gain: 0f, filledWithValue: true, valueToFill: 0);

                                break;

                            default:
                                throw new ArgumentException($"Unsupported levelType - {levelType}.");
                        }

                        Parallel.ForEach(this.terrainByName.Values, SonOfRobinGame.defaultParallelOptions, terrain =>
                        {
                            terrain.TryToLoadSavedTerrain();
                        });
                    }

                    break;

                case Stage.TerrainGenerate:

                    foreach (Terrain currentTerrain in this.terrainByName.Values)
                    {
                        // different terrain types cannot be processed in parallel, because noise generator settings would get corrupted
                        currentTerrain.GenerateNoiseMap();
                    }

                    if (this.level.levelType == Level.LevelType.Cave)
                    {
                        // removing (filling with 0) all areas, except the largest one
                        {
                            var walkableFloorRawPixelsBag = new ConcurrentBag<Point>();
                            Parallel.For(0, this.dividedHeight, rawY =>
                            {
                                for (int rawX = 0; rawX < this.dividedWidth; rawX++)
                                {
                                    if (this.terrainByName[Terrain.Name.Height].GetMapDataRaw(rawX, rawY) > 29) walkableFloorRawPixelsBag.Add(new Point(rawX, rawY));
                                }
                            });

                            var walkablePixelCoordsByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: this.dividedWidth, height: this.dividedHeight, pointsBag: walkableFloorRawPixelsBag);

                            var largestArea = walkablePixelCoordsByRegion.OrderByDescending(sublist => sublist.Count).FirstOrDefault();

                            Parallel.ForEach(walkablePixelCoordsByRegion.Where(sublist => sublist != largestArea), SonOfRobinGame.defaultParallelOptions, rawCoordsList =>
                            {
                                Terrain terrainHeight = this.terrainByName[Terrain.Name.Height];
                                foreach (Point rawCoords in rawCoordsList)
                                {
                                    terrainHeight.SetMapDataRaw(rawCoords: rawCoords, value: 0);
                                }
                            });
                        }

                        {
                            // removing (filling with largest non-lava value) small lava areas

                            var lavaFloorRawPixelsBag = new ConcurrentBag<Point>();
                            Parallel.For(0, this.dividedHeight, rawY =>
                            {
                                for (int rawX = 0; rawX < this.dividedWidth; rawX++)
                                {
                                    if (this.terrainByName[Terrain.Name.Height].GetMapDataRaw(rawX, rawY) >= Terrain.lavaMin) lavaFloorRawPixelsBag.Add(new Point(rawX, rawY));
                                }
                            });

                            var lavaPixelCoordsByRegion = Helpers.SlicePointBagIntoConnectedRegions(width: this.dividedWidth, height: this.dividedHeight, pointsBag: lavaFloorRawPixelsBag);

                            Parallel.ForEach(lavaPixelCoordsByRegion.Where(sublist => sublist.Count < 16), SonOfRobinGame.defaultParallelOptions, rawCoordsList =>
                            {
                                Terrain terrainHeight = this.terrainByName[Terrain.Name.Height];
                                foreach (Point rawCoords in rawCoordsList)
                                {
                                    terrainHeight.SetMapDataRaw(rawCoords: rawCoords, value: Terrain.lavaMin - 1);
                                }
                            });
                        }
                    }

                    break;

                case Stage.TerrainUpdateMinMax:
                    Parallel.ForEach(this.terrainByName.Values, SonOfRobinGame.defaultParallelOptions, terrain =>
                    {
                        terrain.UpdateMinMaxGridCell();
                    });

                    break;

                case Stage.TerrainSave:
                    if (this.serializable)
                    {
                        Parallel.ForEach(this.terrainByName.Values, SonOfRobinGame.defaultParallelOptions, terrain =>
                        {
                            terrain.SaveTemplate();
                        });
                    }

                    break;

                case Stage.ExtDataCheck:
                    if (this.ExtBoardProps == null) this.ExtBoardProps = new ExtBoardProps(grid: this);

                    break;

                case Stage.ExtDataSetSea:
                    if (this.ExtBoardProps.CreationInProgress) this.ExtCalculateSea();

                    break;

                case Stage.ExtDataSetBeach:
                    if (this.ExtBoardProps.CreationInProgress) this.ExtCalculateOuterBeach();

                    break;

                case Stage.ExtDataSetBiomes:
                    if (this.ExtBoardProps.CreationInProgress) this.ExtCalculateBiomes();

                    break;

                case Stage.ExtDataSetPropertiesGrid:
                    if (this.ExtBoardProps.CreationInProgress) this.ExtBoardProps.CreateContainsPropertiesGrid();

                    break;

                case Stage.ExtDataSetFinish:
                    if (this.ExtBoardProps.CreationInProgress) this.ExtBoardProps.EndCreationAndSave(saveTemplate: this.serializable);

                    break;

                case Stage.NamedLocationsGenerate:
                    if (this.level.levelType == Level.LevelType.Island) this.namedLocations.GenerateLocations();

                    break;

                case Stage.MeshesLoad:
                    Mesh[] meshArray = null;
                    if (this.serializable) meshArray = MeshGenerator.LoadMeshes(this);

                    if (meshArray != null)
                    {
                        this.MeshGrid = MeshGenerator.CreateMeshGrid(meshArray: meshArray, grid: this);
                        this.meshGridLoaded = true;
                    }

                    break;

                case Stage.MeshesGenerate:
                    if (!this.meshGridLoaded) this.MeshGrid = MeshGenerator.CreateMeshGrid(meshArray: MeshGenerator.GenerateMeshes(grid: this, saveTemplate: this.serializable), grid: this);

                    break;

                case Stage.AllowedNamesFill:
                    Parallel.ForEach(this.allCells, SonOfRobinGame.defaultParallelOptions, cell =>
                    {
                        cell.FillAllowedNames(); // needs to be invoked after calculating final terrain and ExtProps
                        cell.FillMiscProperties();
                    });

                    this.FillCellListsForPieceNames();

                    this.CreationInProgress = false;
                    break;

                default:
                    if ((int)this.currentStage < allStagesCount) throw new ArgumentException("Not all steps has been processed.");
                    break;
            }

            TimeSpan creationDuration = DateTime.Now - this.stageStartTime;
            MessageLog.Add(debugMessage: true, text: $"{namesForStages[this.currentStage]} - time: {creationDuration:hh\\:mm\\:ss\\.fff}", textColor: Color.GreenYellow);

            this.PrepareNextStage(incrementCurrentStage: true);
        }

        private void FillCellListsForPieceNames()
        {
            var concurrentCellSetsForPieceNames = new ConcurrentDictionary<PieceTemplate.Name, Cell[]>(); // for parallel processing

            Parallel.ForEach(PieceTemplate.allNames, pieceName =>
            {
                var cellList = new List<Cell>();

                foreach (Cell cell in this.allCells)
                {
                    if (cell.allowedNames.Contains(pieceName)) cellList.Add(cell);
                }

                Random random = new Random(this.level.seed + (int)pieceName); // to keep "random" hashset order the same for every seed
                concurrentCellSetsForPieceNames[pieceName] = cellList.OrderBy(cell => random.Next()).ToArray();
            });

            foreach (var kvp in concurrentCellSetsForPieceNames)
            {
                this.cellArraysForPieceNamesMasterDict[kvp.Key] = kvp.Value;
            }
        }

        private void ExtCalculateSea()
        {
            if (this.level.levelType == Level.LevelType.Cave) return;
            else if (this.level.levelType == Level.LevelType.OpenSea)
            {
                this.ExtBoardProps.FillWithTrue(name: ExtBoardProps.Name.Sea);
                return;
            }

            // the algorithm will only work, if water surrounds the island

            int maxX = (this.level.width / this.resDivider) - 1;
            int maxY = (this.level.height / this.resDivider) - 1;

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
                 terrainSearches: new List<TerrainSearch> { new TerrainSearch(name: Terrain.Name.Height, min: 0, max: (byte)Terrain.waterLevelMax) },
                 setNameIfInsideRange: true,
                 nameToSetIfInRange: ExtBoardProps.Name.Sea,
                 setNameIfOutsideRange: true,
                 nameToSetIfOutsideRange: ExtBoardProps.Name.OuterBeach
                 );
        }

        private void ExtCalculateOuterBeach()
        {
            if (this.level.levelType == Level.LevelType.Cave) return;
            else if (this.level.levelType == Level.LevelType.OpenSea)
            {
                this.ExtBoardProps.FillWithTrue(name: ExtBoardProps.Name.OuterBeach);
                return;
            }

            ConcurrentBag<Point> beachEdgePointListRaw = this.GetAllRawCoordinatesWithExtProperty(nameToUse: ExtBoardProps.Name.OuterBeach, value: true);

            this.FloodFillExtProps(
                 startingPoints: beachEdgePointListRaw,
                 terrainSearches: new List<TerrainSearch> { new TerrainSearch(name: Terrain.Name.Height, min: Terrain.waterLevelMax, max: (byte)(Terrain.waterLevelMax + 5)) },
                 setNameIfInsideRange: true,
                 nameToSetIfInRange: ExtBoardProps.Name.OuterBeach
                 );
        }

        private void ExtCalculateBiomes()
        {
            if (this.level.levelType == Level.LevelType.Cave) return;
            else if (this.level.levelType == Level.LevelType.OpenSea)
            {
                foreach (ExtBoardProps.Name name in ExtBoardProps.allBiomes)
                {
                    this.ExtBoardProps.FillWithFalse(name: name);
                }
                return;
            }

            // setting up variables

            var biomeCountByName = new Dictionary<ExtBoardProps.Name, int>();

            var pointCollectionsForBiomes = new Dictionary<ExtBoardProps.Name, List<ConcurrentBag<Point>>>();

            Random random = new(this.level.seed); // based on original seed (to ensure that biome order will be identical for given seed)
            foreach (ExtBoardProps.Name name in ExtBoardProps.allBiomes.OrderBy(name => random.Next())) // shuffled biome list
            {
                biomeCountByName[name] = 0;
                pointCollectionsForBiomes[name] = new List<ConcurrentBag<Point>>();
            }

            int maxX = this.level.width / this.resDivider;
            int maxY = this.level.height / this.resDivider;

            byte biomeMinVal = Terrain.biomeMin;
            byte biomeMaxVal = 255;

            TerrainSearch biomeSearch = new(name: Terrain.Name.Biome, min: biomeMinVal, max: biomeMaxVal);
            var biomeSearchList = new List<TerrainSearch> { biomeSearch };

            var checkedCoordsRaw = new bool[maxX, maxY];

            // preparing point list for every biome (checking every pixel and using flood fill)

            Point currentPoint = new(0, 0);

            while (true) // using Parallel here would result in a random biome distribution
            {
                if (!checkedCoordsRaw[currentPoint.X, currentPoint.Y])
                {
                    byte value = this.terrainByName[Terrain.Name.Biome].GetMapDataRaw(currentPoint.X, currentPoint.Y);

                    if (biomeMinVal <= value && value <= biomeMaxVal)
                    {
                        ConcurrentBag<Point> biomeRawPoints = this.FloodFillExtProps(
                              startingPoints: new ConcurrentBag<Point> { currentPoint },
                              terrainSearches: biomeSearchList
                              );

                        ExtBoardProps.Name biomeName = biomeCountByName
                            .OrderBy(kvp => kvp.Value * ExtBoardProps.biomeMultiplier[kvp.Key])
                            .First().Key;

                        pointCollectionsForBiomes[biomeName].Add(biomeRawPoints);
                        biomeCountByName[biomeName]++;

                        foreach (Point filledPoint in biomeRawPoints)
                        {
                            checkedCoordsRaw[filledPoint.X, filledPoint.Y] = true;
                        }
                    }

                    checkedCoordsRaw[currentPoint.X, currentPoint.Y] = true;
                }

                currentPoint.X++;
                if (currentPoint.X >= maxX)
                {
                    currentPoint.X = 0;
                    currentPoint.Y++;
                    if (currentPoint.Y >= maxY) break;
                }
            }

            // setting biome constrains

            int minPointCount = 500000 / (this.resDivider * this.resDivider);
            var tempConstrainedPointsListForBiomes = new Dictionary<ExtBoardProps.Name, List<ConcurrentBag<Point>>>();
            var tempRawPointsForCreatedBiomes = new Dictionary<ExtBoardProps.Name, ConcurrentBag<List<Point>>>();

            foreach (var kvp in pointCollectionsForBiomes)
            {
                ExtBoardProps.Name biomeName = kvp.Key;
                List<ConcurrentBag<Point>> pointBags = kvp.Value;

                tempConstrainedPointsListForBiomes[biomeName] = new List<ConcurrentBag<Point>>();
                tempRawPointsForCreatedBiomes[biomeName] = new ConcurrentBag<List<Point>>();

                foreach (ConcurrentBag<Point> currentPointBag in pointBags)
                {
                    if (currentPointBag.Count < minPointCount) continue;

                    var constrainedPointBag = new ConcurrentBag<Point>();
                    Parallel.ForEach(currentPointBag, SonOfRobinGame.defaultParallelOptions, point =>
                    {
                        if (this.CheckIfPointMeetsSearchCriteria(terrainSearches: ExtBoardProps.biomeConstrains[biomeName], point: point, xyRaw: true)) constrainedPointBag.Add(point);
                    });

                    tempConstrainedPointsListForBiomes[biomeName].Add(constrainedPointBag);
                }
            }

            // slicing constrained points into connected regions

            foreach (var kvp in tempConstrainedPointsListForBiomes)
            {
                ExtBoardProps.Name biomeName = kvp.Key;
                List<ConcurrentBag<Point>> listOfPointBags = kvp.Value;

                Parallel.ForEach(listOfPointBags, SonOfRobinGame.defaultParallelOptions, constrainedPointBag =>
                {
                    foreach (List<Point> slicedPointsList in Helpers.SlicePointBagIntoConnectedRegions(width: maxX, height: maxY, pointsBag: constrainedPointBag))
                    {
                        if (slicedPointsList.Count >= minPointCount) tempRawPointsForCreatedBiomes[biomeName].Add(slicedPointsList);
                    }
                });
            }

            // setting biome values

            foreach (var kvp in tempRawPointsForCreatedBiomes)
            {
                ExtBoardProps.Name biomeName = kvp.Key;
                ConcurrentBag<List<Point>> listOfPointBags = kvp.Value;

                Parallel.ForEach(listOfPointBags, SonOfRobinGame.defaultParallelOptions, pointList =>
                {
                    foreach (Point point in pointList)
                    {
                        // can write to array using parallel, if every thread accesses its own indices
                        this.ExtBoardProps.SetValueRaw(name: biomeName, value: true, rawX: point.X, rawY: point.Y);
                    }
                });
            }
        }

        private ConcurrentBag<Point> GetAllRawCoordinatesWithExtProperty(ExtBoardProps.Name nameToUse, bool value)
        {
            int maxX = this.level.width / this.resDivider;
            int maxY = this.level.height / this.resDivider;

            var pointBag = new ConcurrentBag<Point>();

            Parallel.For(0, maxX, SonOfRobinGame.defaultParallelOptions, rawX =>
            {
                for (int rawY = 0; rawY < maxY; rawY++)
                {
                    if (this.ExtBoardProps.GetValueRaw(name: nameToUse, rawX: rawX, rawY: rawY, boundsCheck: false) == value)
                    {
                        pointBag.Add(new Point(rawX, rawY));
                    }
                }
            });

            return pointBag;
        }

        private bool CheckIfPointMeetsSearchCriteria(List<TerrainSearch> terrainSearches, Point point, bool xyRaw)
        {
            foreach (TerrainSearch terrainSearch in terrainSearches)
            {
                byte value = xyRaw ?
                    this.terrainByName[terrainSearch.name].GetMapDataRaw(point.X, point.Y) :
                    this.terrainByName[terrainSearch.name].GetMapData(point.X, point.Y);

                if (terrainSearch.min > value || value > terrainSearch.max) return false;
            }
            return true;
        }

        private ConcurrentBag<Point> FloodFillExtProps(ConcurrentBag<Point> startingPoints, List<TerrainSearch> terrainSearches, ExtBoardProps.Name nameToSetIfInRange = ExtBoardProps.Name.Sea, bool setNameIfInsideRange = false, bool setNameIfOutsideRange = false, ExtBoardProps.Name nameToSetIfOutsideRange = ExtBoardProps.Name.Sea)
        {
            int dividedWidth = this.level.width / this.resDivider;
            int dividedHeight = this.level.height / this.resDivider;

            Point[] offsetArray = new Point[]
            {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1),
            };

            bool[,] processedMap = new bool[dividedWidth, dividedHeight]; // working inside "compressed" map space, to avoid unnecessary calculations

            var nextPoints = new ConcurrentBag<Point>(startingPoints);
            var rawPointsInsideRange = new ConcurrentBag<Point>();

            while (true)
            {
                HashSet<Point> currentPoints = new(nextPoints); // using HashSet to filter out duplicates
                nextPoints = new ConcurrentBag<Point>(); // because there is no Clear() for ConcurrentBag

                Parallel.ForEach(currentPoints, SonOfRobinGame.defaultParallelOptions, currentPoint =>
                {
                    // array can be written to using parallel, if every thread accesses its own indices

                    if (this.CheckIfPointMeetsSearchCriteria(terrainSearches: terrainSearches, point: currentPoint, xyRaw: true))
                    {
                        rawPointsInsideRange.Add(currentPoint);
                        if (setNameIfInsideRange) this.ExtBoardProps.SetValueRaw(name: nameToSetIfInRange, value: true, rawX: currentPoint.X, rawY: currentPoint.Y);

                        foreach (Point currentOffset in offsetArray)
                        {
                            Point nextPoint = new(currentPoint.X + currentOffset.X, currentPoint.Y + currentOffset.Y);

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
                        if (setNameIfOutsideRange) this.ExtBoardProps.SetValueRaw(name: nameToSetIfOutsideRange, value: true, rawX: currentPoint.X, rawY: currentPoint.Y);
                    }

                    processedMap[currentPoint.X, currentPoint.Y] = true;
                });

                if (nextPoints.IsEmpty) break;
            }

            return rawPointsInsideRange;
        }

        public List<Cell> GetCellsForPieceName(PieceTemplate.Name pieceName)
        {
            return this.cellArraysForPieceNamesMasterDict[pieceName].ToList(); // ToList() - to avoid modifying original list
        }

        public Cell GetRandomCellForPieceName(PieceTemplate.Name pieceName, bool returnDummyCellIfInsideCamera) // to avoid calling GetCellsForPieceName() for getting one random cell only
        {
            if (!this.cellSetsForPieceNamesWorkingQueueDict.ContainsKey(pieceName) || this.cellSetsForPieceNamesWorkingQueueDict[pieceName].Count == 0)
            {
                this.cellSetsForPieceNamesWorkingQueueDict[pieceName] = new Queue<Cell>(this.cellArraysForPieceNamesMasterDict[pieceName]);
            }

            var cellQueue = this.cellSetsForPieceNamesWorkingQueueDict[pieceName];
            if (cellQueue.Count == 0)
            {
                MessageLog.Add(debugMessage: true, text: $"No cells suitable for creation of {PieceInfo.GetInfo(pieceName).readableName}.", avoidDuplicates: true);
                return this.allCells[0]; // to properly return a (useless) cell
            }
            else
            {
                while (true)
                {
                    Cell nextCell = cellQueue.Dequeue();
                    if (returnDummyCellIfInsideCamera && this.world.camera.viewRect.Intersects(nextCell.rect)) return this.allCells[0];
                    else return nextCell;
                }
            }
        }

        private void PrepareNextStage(bool incrementCurrentStage)
        {
            if (incrementCurrentStage) this.currentStage++;
            this.stageStartTime = DateTime.Now;
        }

        private void UpdateProgressBar()
        {
            if (this.world.demoMode) return;

            float percentage = FullScreenProgressBar.CalculatePercentage(currentLocalStep: 0, totalLocalSteps: 1, currentGlobalStep: 1 + (int)this.currentStage, totalGlobalSteps: SonOfRobinGame.enteringIslandGlobalSteps);

            string detailedInfo = null;
            if (Preferences.progressBarShowDetails)
            {
                int stageNo = Math.Min(Convert.ToInt32(this.currentStage), namesForStages.Count - 1);

                detailedInfo = $"{namesForStages[(Stage)stageNo]}...";
            }

            if (!this.world.HasBeenRemoved) SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: LoadingTips.GetTip(), optionalText: detailedInfo);
        }

        public static void AddToGroup(Sprite sprite, Cell.Group groupName)
        {
            if (!sprite.gridGroups.Contains(groupName)) sprite.gridGroups.Add(groupName);
            if (sprite.currentCell == null) return;

            sprite.currentCell.AddToGroup(sprite: sprite, groupName: groupName);
        }

        public static void RemoveFromGroup(Sprite sprite, Cell.Group groupName)
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

        public static void RemoveSprite(Sprite sprite)
        {
            if (sprite.currentCell == null) return;
            sprite.currentCell.RemoveSprite(sprite);
        }

        public List<Sprite> GetSpritesFromSurroundingCells(Sprite sprite, Cell.Group groupName)
        {
            Cell cell = sprite.currentCell == null ? this.FindMatchingCell(sprite.position) : sprite.currentCell;
            return cell.GetSpritesFromSurroundingCells(groupName);
        }

        public List<Cell> GetCellsInsideRect(Rectangle rectangle, bool addPadding)
        {
            int padding = addPadding ? 1 : 0;

            int xMinCellNo = Math.Max(FindMatchingCellInSingleAxis(position: rectangle.Left, cellLength: this.cellWidth) - padding, 0);
            int xMaxCellNo = Math.Min(FindMatchingCellInSingleAxis(position: rectangle.Right, cellLength: this.cellWidth) + padding, this.noOfCellsX - 1);
            int yMinCellNo = Math.Max(FindMatchingCellInSingleAxis(position: rectangle.Top, cellLength: this.cellHeight) - padding, 0);
            int yMaxCellNo = Math.Min(FindMatchingCellInSingleAxis(position: rectangle.Bottom, cellLength: this.cellHeight) + padding, this.noOfCellsY - 1);

            List<Cell> cellsInsideRect = new();

            for (int x = xMinCellNo; x <= xMaxCellNo; x++)
            {
                for (int y = yMinCellNo; y <= yMaxCellNo; y++)
                { cellsInsideRect.Add(this.cellGrid[x, y]); }
            }

            return cellsInsideRect;
        }

        public List<Cell> GetCellsWithinDistance(Vector2 position, int distance)
        {
            int xMinCellNo = FindMatchingCellInSingleAxis(
                position: Math.Clamp(value: (int)(position.X - distance), min: 0, max: this.level.width - 1), cellLength: this.cellWidth);

            int xMaxCellNo = FindMatchingCellInSingleAxis(
                position: Math.Clamp(value: (int)(position.X + distance), min: 0, max: this.level.width - 1), cellLength: this.cellWidth);

            int yMinCellNo = FindMatchingCellInSingleAxis(
                position: Math.Clamp(value: (int)(position.Y - distance), min: 0, max: this.level.height - 1), cellLength: this.cellHeight);

            int yMaxCellNo = FindMatchingCellInSingleAxis(
                position: Math.Clamp(value: (int)(position.Y + distance), min: 0, max: this.level.height - 1), cellLength: this.cellHeight);

            List<Cell> cellsWithinDistance = new();

            for (int x = xMinCellNo; x <= xMaxCellNo; x++)
            {
                for (int y = yMinCellNo; y <= yMaxCellNo; y++)
                {
                    cellsWithinDistance.Add(this.cellGrid[x, y]);
                }
            }

            return cellsWithinDistance;
        }

        public List<Sprite> GetSpritesWithinDistance(Sprite mainSprite, int distance, Cell.Group groupName)
        {
            var cellsWithinDistance = this.GetCellsWithinDistance(position: mainSprite.position, distance: distance);
            var spritesWithinDistance = new List<Sprite>();

            foreach (Cell cell in cellsWithinDistance)
            {
                foreach (Sprite currentSprite in cell.spriteGroups[groupName])
                {
                    if (Vector2.Distance(currentSprite.position, mainSprite.position) <= distance && currentSprite != mainSprite) spritesWithinDistance.Add(currentSprite);
                }
            }

            return spritesWithinDistance;
        }

        public List<BoardPiece> GetPiecesWithinDistance(Sprite mainSprite, int distance, Cell.Group groupName, int offsetX = 0, int offsetY = 0, bool compareWithBottom = false)
        {
            var cellsWithinDistance = this.GetCellsWithinDistance(position: mainSprite.position, distance: distance);
            var piecesWithinDistance = new List<BoardPiece>();

            Vector2 centerPos = mainSprite.position + new Vector2(offsetX, offsetY);

            if (compareWithBottom)
            {
                foreach (Cell cell in cellsWithinDistance)
                {
                    foreach (Sprite sprite in cell.spriteGroups[groupName])
                    {
                        if (Vector2.Distance(new Vector2(sprite.GfxRect.Center.X, sprite.GfxRect.Bottom), centerPos) <= distance && sprite != mainSprite)
                        {
                            piecesWithinDistance.Add(sprite.boardPiece);
                        }
                    }
                }
            }
            else
            {
                foreach (Cell cell in cellsWithinDistance)
                {
                    foreach (Sprite sprite in cell.spriteGroups[groupName])
                    {
                        if (Vector2.Distance(sprite.position, centerPos) <= distance && sprite != mainSprite)
                        {
                            piecesWithinDistance.Add(sprite.boardPiece);
                        }
                    }
                }
            }

            return piecesWithinDistance;
        }

        public List<BoardPiece> GetPiecesInsideTriangle(Point point1, Point point2, Point point3, Cell.Group groupName)
        {
            int xMin = (int)Math.Min(Math.Min(point1.X, point2.X), point3.X);
            int xMax = (int)Math.Max(Math.Max(point1.X, point2.X), point3.X);
            int yMin = (int)Math.Min(Math.Min(point1.Y, point2.Y), point3.Y);
            int yMax = (int)Math.Max(Math.Max(point1.Y, point2.Y), point3.Y);

            Rectangle rect = new(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);

            var cellsInsideRect = this.GetCellsInsideRect(rectangle: rect, addPadding: true);

            var spritesInsideTriangle = new List<Sprite>();
            foreach (Cell cell in cellsInsideRect)
            {
                spritesInsideTriangle.AddRange(cell.spriteGroups[groupName].Where(
                      currentSprite => rect.Contains(currentSprite.position) && Helpers.IsPointInsideTriangle(point: new Point((int)currentSprite.position.X, (int)currentSprite.position.Y), triangleA: point1, triangleB: point2, triangleC: point3)));
            }

            var piecesInsideTriangle = spritesInsideTriangle.Select(sprite => sprite.boardPiece);
            return piecesInsideTriangle.ToList();
        }

        public List<BoardPiece> GetPiecesInCameraView(Cell.Group groupName, bool compareWithCameraRect = false)
        {
            Camera camera = this.world.camera;
            var visibleCells = this.GetCellsInsideRect(rectangle: camera.viewRect, addPadding: true);
            var piecesInCameraView = new List<BoardPiece>();

            if (compareWithCameraRect)
            {
                foreach (Cell cell in visibleCells) // making sure that every piece is actually inside camera rect
                {
                    foreach (Sprite sprite in cell.spriteGroups[groupName])
                    {
                        if (camera.viewRect.Intersects(sprite.GfxRect)) piecesInCameraView.Add(sprite.boardPiece);
                    }
                }
            }
            else
            {
                foreach (Cell cell in visibleCells) // visibleCells area is larger than camera view
                {
                    foreach (Sprite sprite in cell.spriteGroups[groupName])
                    {
                        piecesInCameraView.Add(sprite.boardPiece);
                    }
                }
            }

            return piecesInCameraView;
        }

        private Cell[] GetAllCells()
        {
            var allCells = new Cell[this.noOfCellsX * this.noOfCellsY];

            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                {
                    Cell cell = this.cellGrid[x, y];
                    allCells[cell.cellIndex] = cell;
                }
            }
            return allCells;
        }

        public ConcurrentBag<Sprite> GetSpritesFromAllCells(Cell.Group groupName, bool visitedByPlayerOnly = false)
        {
            var cells = (IEnumerable<Cell>)this.allCells;
            if (visitedByPlayerOnly) cells = this.allCells.Where(cell => cell.visitedByPlayer);

            var allSprites = new ConcurrentBag<Sprite> { };

            Parallel.ForEach(cells, SonOfRobinGame.defaultParallelOptions, cell =>
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName])
                {
                    allSprites.Add(sprite);
                }
            });

            return allSprites;
        }

        public List<Sprite> GetSpritesForRect(Cell.Group groupName, Rectangle rectangle, bool visitedByPlayerOnly = false, bool addPadding = true)
        {
            var cells = this.GetCellsInsideRect(rectangle: rectangle, addPadding: addPadding);
            if (visitedByPlayerOnly) cells = cells.Where(cell => cell.visitedByPlayer).ToList();

            var allSprites = new List<Sprite>();
            foreach (Cell cell in cells)
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName])
                {
                    if (sprite.GfxRect.Intersects(rectangle)) allSprites.Add(sprite);
                }
            }

            return allSprites;
        }

        public ConcurrentBag<Sprite> GetSpritesForRectParallel(Cell.Group groupName, Rectangle rectangle, bool visitedByPlayerOnly = false, bool addPadding = true)
        {
            var cells = this.GetCellsInsideRect(rectangle: rectangle, addPadding: addPadding);
            if (visitedByPlayerOnly) cells = cells.Where(cell => cell.visitedByPlayer).ToList();

            var allSprites = new ConcurrentBag<Sprite> { };
            Parallel.ForEach(cells, SonOfRobinGame.defaultParallelOptions, cell =>
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName])
                {
                    if (sprite.GfxRect.Intersects(rectangle)) allSprites.Add(sprite);
                }
            });

            return allSprites;
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

        public void GenerateWholeIslandPreviewTextureIfMissing()
        {
            if (this.WholeIslandPreviewTexture != null) return;

            Point wholeIslandPreviewSize = new Point(Math.Min(this.width / this.resDivider, 2000), Math.Min(this.height / this.resDivider, 2000));

            RenderTarget2D previewRenderTarget = new RenderTarget2D(
                graphicsDevice: SonOfRobinGame.GfxDev,
                width: wholeIslandPreviewSize.X,
                height: wholeIslandPreviewSize.Y,
                mipMap: false,
                preferredFormat: SurfaceFormat.Color, preferredDepthFormat: DepthFormat.None);

            Scene.SetRenderTarget(previewRenderTarget);
            Scene.SetupPolygonDrawing(allowRepeat: true, transformMatrix: Matrix.CreateScale(1f / (float)this.width * (float)wholeIslandPreviewSize.X));
            BasicEffect basicEffect = SonOfRobinGame.BasicEffect;

            SonOfRobinGame.GfxDev.Clear(this.level.hasWater ? Map.waterColor : Color.Black);

            foreach (Mesh mesh in this.MeshGrid.allMeshes.Distinct().OrderBy(mesh => mesh.meshDef.drawPriority))
            {
                basicEffect.Texture = mesh.meshDef.mapTexture;

                foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    mesh.Draw(processTweeners: false);
                }
            }

            this.WholeIslandPreviewTexture = previewRenderTarget;

            // GfxConverter.SaveTextureAsPNG(filename: Path.Combine(this.gridTemplate.templatePath, "whole_map.png"), texture: this.WholeIslandPreviewTexture); // for testing
        }

        public int DrawBackground()
        {
            bool updateFog = false;
            Camera camera = this.world.camera;
            Rectangle cameraRect = this.world.camera.viewRect;

            foreach (Cell cell in this.GetCellsInsideRect(rectangle: cameraRect, addPadding: false))
            {
                if (this.world.MapEnabled && !cell.visitedByPlayer && cameraRect.Intersects(cell.rect) && camera.IsTrackingPlayer && this.world.Player.CanSeeAnything)
                {
                    cell.SetAsVisited();
                    updateFog = true;
                }
            }

            if (updateFog) this.world.map.backgroundNeedsUpdating = true;

            BasicEffect basicEffect = SonOfRobinGame.BasicEffect;

            int trianglesDrawn = 0;

            var meshesToDraw = this.MeshGrid.GetMeshesForRect(cameraRect)
                .Where(mesh => mesh.boundsRect.Intersects(cameraRect))
                .Distinct()
                .OrderBy(mesh => mesh.meshDef.drawPriority);

            foreach (Mesh mesh in meshesToDraw)
            {
                basicEffect.Texture = mesh.meshDef.texture;
                SonOfRobinGame.GfxDev.BlendState = mesh.meshDef.blendState;

                foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    mesh.Draw(processTweeners: true);
                    trianglesDrawn += mesh.triangleCount;
                }
            }

            if (Preferences.debugShowMeshBounds)
            {
                SpriteFontBase font = SonOfRobinGame.FontVCROSD.GetFont(18);

                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.world.TransformMatrix);
                foreach (Mesh mesh in meshesToDraw)
                {
                    SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: mesh.boundsRect, color: Color.White, thickness: 1f);

                    string meshText = $"{mesh.boundsRect.X},{mesh.boundsRect.Y}\n{mesh.boundsRect.Width}x{mesh.boundsRect.Height}\n{mesh.textureName}\ntris: {mesh.triangleCount}";
                    font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: meshText, position: new Vector2(mesh.boundsRect.Left, mesh.boundsRect.Top), color: Color.White, effect: FontSystemEffect.Stroked, effectAmount: 2);
                }
                SonOfRobinGame.SpriteBatch.End();
            }
            return trianglesDrawn;
        }

        public void DrawSunShadows(IEnumerable<Sprite> spritesCastingShadows, AmbientLight.SunLightData sunLightData)
        {
            Rectangle cameraRect = this.world.camera.viewRect;

            Vector2 normalizedSunPos = new(sunLightData.sunPos.X + cameraRect.Center.X, sunLightData.sunPos.Y + cameraRect.Center.Y);
            float sunAngle = Helpers.GetAngleBetweenTwoPoints(start: new Vector2(cameraRect.Center.X, cameraRect.Center.Y), end: normalizedSunPos);
            int sunDistance = 10000;
            Vector2 sunOffset = new(sunDistance * (float)Math.Cos(sunAngle), sunDistance * (float)Math.Sin(sunAngle));
            normalizedSunPos = new Vector2(cameraRect.Center.X, cameraRect.Center.Y) + sunOffset;

            bool shadowLeftSide = sunLightData.sunPos.X < 0;
            bool shadowTopSide = sunLightData.sunPos.Y > 0; // must be reversed

            foreach (Sprite shadowSprite in spritesCastingShadows.OrderBy(s => s.AnimFrame.layer).ThenByDescending(s => Vector2.DistanceSquared(normalizedSunPos, s.position)))
            {
                if (shadowSprite.boardPiece.pieceInfo.shadowNotDrawn) continue;

                Rectangle gfxRect = shadowSprite.GfxRect;

                if (!shadowSprite.IsInCameraRect && !shadowSprite.boardPiece.HasFlatShadow)
                {
                    if (shadowSprite.position.Y < cameraRect.Top &&
                        (shadowTopSide || gfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Top - shadowSprite.position.Y)))
                        continue;

                    if (shadowSprite.position.Y > cameraRect.Bottom &&
                        (!shadowTopSide || gfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Bottom - shadowSprite.position.Y)))
                        continue;

                    if (shadowSprite.position.X > cameraRect.Right &&
                        (shadowLeftSide || gfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Right - shadowSprite.position.X)))
                        continue;

                    if (shadowSprite.position.X < cameraRect.Left &&
                        (!shadowLeftSide || gfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Left - shadowSprite.position.X)))
                        continue;
                }

                Sprite.DrawShadow(color: Color.Black, shadowSprite: shadowSprite, lightPos: normalizedSunPos, shadowAngle: Helpers.GetAngleBetweenTwoPoints(start: normalizedSunPos, end: shadowSprite.position), yScaleForce: sunLightData.sunShadowsLength);

                if (cameraRect.Intersects(gfxRect)) shadowSprite.DrawRoutine(calculateSubmerge: true); // erasing shadowSprite from the shadow
            }
        }

        public BoardPiece[] DrawSprites()
        {
            // Sprites should be drawn all at once, because cell-based drawing causes Y sorting order incorrect
            // in cases of sprites overlapping cell boundaries.

            List<BoardPiece> visiblePieces = this.GetPiecesInCameraView(groupName: Cell.Group.Visible, compareWithCameraRect: true);
            var offScreenParticleEmitterPieces = this.level.recentParticlesManager.OffScreenPieces;
            var piecesToDraw = visiblePieces.Concat(offScreenParticleEmitterPieces).ToArray();

            foreach (BoardPiece piece in piecesToDraw
                .OrderBy(o => o.sprite.AnimFrame.layer)
                .ThenBy(o => o.sprite.GfxRect.Bottom))
            {
                piece.sprite.Draw();
            }

            StatBar.DrawAll();

            return piecesToDraw;
        }

        public void DrawDebugData(bool drawCellData, bool drawPieceData)
        {
            if (!drawCellData && !drawPieceData) return;

            var visibleCells = this.GetCellsInsideRect(rectangle: this.world.camera.viewRect, addPadding: false);

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

        private Cell[,] MakeGrid()
        {
            Cell[,] cellGrid = new Cell[this.noOfCellsX, this.noOfCellsY];

            int cellIndex = 0;
            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                {
                    cellGrid[x, y] = new Cell(grid: this, cellIndex: cellIndex, cellNoX: x, cellNoY: y, cellWidth: this.cellWidth, cellHeight: this.cellHeight);
                    cellIndex++;
                }
            }

            return cellGrid;
        }

        private void CalculateSurroundingCells() // surrouding cells are calculated only once, because they do not change
        {
            foreach (Cell cell in this.allCells)
            { cell.surroundingCells = this.GetCellsWithinDistance(cell: cell, distance: 1); }
        }

        public List<Cell> GetCellsWithinDistance(Cell cell, int distance)
        {
            if (cell.grid != this) throw new ArgumentException("This cell is from another grid.");

            List<Cell> cellsWithinDistance = new();

            for (int offsetX = -distance; offsetX < distance + 1; offsetX++)
            {
                for (int offsetY = -1; offsetY < distance + 1; offsetY++)
                {
                    int currentCellX = cell.cellNoX + offsetX;
                    int currentCellY = cell.cellNoY + offsetY;
                    if (currentCellX >= 0 && currentCellX < this.noOfCellsX && currentCellY >= 0 && currentCellY < this.noOfCellsY)
                    {
                        cellsWithinDistance.Add(this.cellGrid[currentCellX, currentCellY]);
                    }
                }
            }

            return cellsWithinDistance;
        }
    }
}