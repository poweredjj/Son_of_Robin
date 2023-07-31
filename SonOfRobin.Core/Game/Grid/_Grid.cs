﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            LoadTerrain = 0,
            GenerateTerrain = 1,
            SaveTerrain = 2,
            CheckExtData = 3,
            SetExtDataSea = 4,
            SetExtDataBeach = 5,
            SetExtDataBiomes = 6,
            SetExtDataBiomesConstrains = 7,
            SetExtDataPropertiesGrid = 8,
            SetExtDataFinish = 9,
            FillAllowedNames = 10,
            MakeEntireMapImage = 11,
        }

        public static readonly int allStagesCount = ((Stage[])Enum.GetValues(typeof(Stage))).Length;

        private static readonly Dictionary<Stage, string> namesForStages = new()
        {
            { Stage.LoadTerrain, "loading terrain" },
            { Stage.GenerateTerrain, "generating terrain" },
            { Stage.SaveTerrain, "saving terrain" },
            { Stage.CheckExtData, "loading extended data" },
            { Stage.SetExtDataSea, "setting extended data (sea)" },
            { Stage.SetExtDataBeach, "setting extended data (beach)" },
            { Stage.SetExtDataBiomes, "setting extended data (biomes)" },
            { Stage.SetExtDataBiomesConstrains, "setting extended data (constrains)" },
            { Stage.SetExtDataPropertiesGrid, "setting extended data (properties grid)" },
            { Stage.SetExtDataFinish, "saving extended data" },
            { Stage.FillAllowedNames, "filling lists of allowed names" },
            { Stage.MakeEntireMapImage, "making entire map image" },
        };

        private Task backgroundTask;
        private Stage currentStage;
        private DateTime stageStartTime;
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

        public readonly Point wholeIslandPreviewSize;
        public readonly float wholeIslandPreviewScale;
        public Texture2D WholeIslandPreviewTexture { get; private set; }

        private readonly Dictionary<Terrain.Name, Terrain> terrainByName;
        private ExtBoardProps extBoardProps;

        public readonly Cell[,] cellGrid;
        public readonly List<Cell> allCells;

        private readonly Dictionary<PieceTemplate.Name, List<Cell>> cellListsForPieceNames; // pieces and cells that those pieces can (initially) be placed into

        private Dictionary<ExtBoardProps.Name, ConcurrentBag<Point>> tempPointsForCreatedBiomes;
        private readonly Dictionary<ExtBoardProps.Name, int> biomeCountByName; // to ensure biome diversity
        public int loadedTexturesCount;
        private DateTime lastUnloadedTime;

        public Grid(World world, int resDivider, int cellWidth = 0, int cellHeight = 0)
        {
            this.CreationInProgress = true;
            this.currentStage = 0;
            this.lastUnloadedTime = DateTime.Now;

            this.world = world;
            this.resDivider = resDivider;
            this.terrainByName = new Dictionary<Terrain.Name, Terrain>();

            this.width = this.world.width;
            this.height = this.world.height;
            this.dividedWidth = (int)Math.Ceiling((double)this.width / (double)this.resDivider);
            this.dividedHeight = (int)Math.Ceiling((double)this.height / (double)this.resDivider);

            this.wholeIslandPreviewSize = new Point(Math.Min(this.width / this.resDivider, 2000), Math.Min(this.height / this.resDivider, 2000));
            this.wholeIslandPreviewScale = (float)wholeIslandPreviewSize.X / (float)this.width;

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

            this.gridTemplate = new GridTemplate(seed: this.world.seed, width: this.world.width, height: this.world.height, cellWidth: this.cellWidth, cellHeight: this.cellHeight, resDivider: this.resDivider, createdDate: DateTime.Now);

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

            this.tempPointsForCreatedBiomes = new Dictionary<ExtBoardProps.Name, ConcurrentBag<Point>>();
            this.biomeCountByName = new Dictionary<ExtBoardProps.Name, int>();

            Random tempRandom = new(this.world.seed); // separate from world.random, to avoid changing world.random state (difference when copying grid from template)
            foreach (ExtBoardProps.Name name in ExtBoardProps.allBiomes.OrderBy(name => tempRandom.Next()).ToList()) // shuffled biome list
            {
                this.biomeCountByName[name] = 0;
                this.tempPointsForCreatedBiomes[name] = new ConcurrentBag<Point>();
            }

            this.PrepareNextStage(incrementCurrentStage: false);
        }

        public void Destroy()
        {
            // for properly disposing used objects
            if (this.WholeIslandPreviewTexture != null) this.WholeIslandPreviewTexture.Dispose();
            this.WholeIslandPreviewTexture = null;

            Parallel.ForEach(this.allCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            {
                if (!cell.boardGraphics.hasBeenCopiedElsewhere) cell.boardGraphics.UnloadTexture();
            });
        }

        public IEnumerable<Cell> CellsVisitedByPlayer
        { get { return this.allCells.Where(cell => cell.VisitedByPlayer); } }

        public IEnumerable<Cell> CellsNotVisitedByPlayer
        { get { return this.allCells.Where(cell => !cell.VisitedByPlayer); } }

        public float VisitedCellsPercentage
        { get { return (float)this.allCells.Where(cell => cell.VisitedByPlayer).Count() / (float)this.allCells.Count; } }

        public Dictionary<string, Object> Serialize()
        {
            // this data is included in save file (not in template)

            var cellData = new List<Object> { };

            foreach (Cell cell in this.allCells)
            {
                cellData.Add(cell.Serialize());
            }

            Dictionary<string, Object> gridData = new()
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

            int cellWidth = (int)(Int64)gridData["cellWidth"];
            int cellHeight = (int)(Int64)gridData["cellHeight"];
            var cellData = (List<Object>)gridData["cellData"];

            Grid grid = new(world: world, cellWidth: cellWidth, cellHeight: cellHeight, resDivider: resDivider);

            for (int i = 0; i < grid.allCells.Count; i++)
            {
                Cell cell = grid.allCells[i];
                cell.Deserialize(cellData[i]);
            }

            return grid;
        }

        public static Grid GetMatchingTemplateFromSceneStack(int seed, int width, int height, int cellWidth = 0, int cellHeight = 0, bool ignoreCellSize = false)
        {
            var existingWorlds = Scene.GetAllScenesOfType(typeof(World));
            if (!existingWorlds.Any()) return null;

            foreach (Scene scene in existingWorlds)
            {
                World existingWorld = (World)scene;

                if (!existingWorld.WorldCreationInProgress &&
                    existingWorld.Grid != null &&
                    !existingWorld.Grid.CreationInProgress &&
                    seed == existingWorld.seed &&
                    width == existingWorld.width &&
                    height == existingWorld.height &&
                    (ignoreCellSize || (cellWidth == existingWorld.Grid.cellWidth && cellHeight == existingWorld.Grid.cellHeight)))
                {
                    return existingWorld.Grid;
                }
            }

            return null;
        }

        public bool CopyBoardFromTemplate()
        {
            // looking for matching template

            Grid templateGrid = GetMatchingTemplateFromSceneStack(seed: this.world.seed, width: this.world.width, height: this.world.height, cellWidth: this.cellWidth, cellHeight: this.cellHeight);
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

            this.extBoardProps = templateGrid.extBoardProps;
            this.extBoardProps.AttachToNewGrid(this);

            // copying whole island texture

            if (this.WholeIslandPreviewTexture != null) this.WholeIslandPreviewTexture.Dispose();

            this.WholeIslandPreviewTexture = new Texture2D(templateGrid.WholeIslandPreviewTexture.GraphicsDevice, templateGrid.WholeIslandPreviewTexture.Width, templateGrid.WholeIslandPreviewTexture.Height);

            Color[] pixelData = new Color[templateGrid.WholeIslandPreviewTexture.Width * templateGrid.WholeIslandPreviewTexture.Height];
            templateGrid.WholeIslandPreviewTexture.GetData(pixelData);
            this.WholeIslandPreviewTexture.SetData(pixelData);

            // copying cell data

            Parallel.For(0, templateGrid.noOfCellsX, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, x =>
            {
                for (int y = 0; y < templateGrid.noOfCellsY; y++)
                    this.cellGrid[x, y].CopyFromTemplate(templateGrid.cellGrid[x, y]);
            });

            // finishing

            this.loadedTexturesCount = this.allCells.Where(cell => cell.boardGraphics.Texture != null).Count();

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

                    break;

                case Stage.GenerateTerrain:
                    foreach (Terrain currentTerrain in this.terrainByName.Values)
                    {
                        // different terrain types cannot be processed in parallel, because noise generator settings would get corrupted
                        currentTerrain.UpdateNoiseMap();
                    }

                    break;

                case Stage.SaveTerrain:
                    Parallel.ForEach(this.terrainByName.Values, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, terrain =>
                    {
                        terrain.SaveTemplate();
                    });

                    break;

                case Stage.CheckExtData:
                    if (this.extBoardProps == null) this.extBoardProps = new ExtBoardProps(grid: this);

                    break;

                case Stage.SetExtDataSea:
                    if (this.extBoardProps.CreationInProgress) this.ExtCalculateSea();

                    break;

                case Stage.SetExtDataBeach:
                    if (this.extBoardProps.CreationInProgress) this.ExtCalculateOuterBeach();

                    break;

                case Stage.SetExtDataBiomes:
                    if (this.extBoardProps.CreationInProgress) this.ExtCalculateBiomes();

                    break;

                case Stage.SetExtDataBiomesConstrains:
                    if (this.extBoardProps.CreationInProgress) this.ExtApplyBiomeConstrains();

                    break;

                case Stage.SetExtDataPropertiesGrid:
                    if (this.extBoardProps.CreationInProgress) this.extBoardProps.CreateContainsPropertiesGrid();

                    break;

                case Stage.SetExtDataFinish:
                    if (this.extBoardProps.CreationInProgress) this.ExtEndCreation();

                    break;

                case Stage.FillAllowedNames:
                    Parallel.ForEach(this.allCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
                    {
                        cell.FillAllowedNames(); // needs to be invoked after calculating final terrain and ExtProps
                        cell.FillMiscProperties();
                    });

                    this.FillCellListsForPieceNames();

                    break;

                case Stage.MakeEntireMapImage:
                    string mapImagePath = BoardGraphics.GetWholeIslandMapPath(this);

                    try
                    { this.WholeIslandPreviewTexture = GfxConverter.LoadTextureFromPNG(mapImagePath); }
                    catch (AggregateException)
                    { }

                    if (this.WholeIslandPreviewTexture == null)
                    {
                        BoardGraphics.CreateAndSaveEntireMapImage(this);
                        this.WholeIslandPreviewTexture = GfxConverter.LoadTextureFromPNG(mapImagePath);
                    }

                    this.CreationInProgress = false;

                    break;

                default:
                    if ((int)this.currentStage < allStagesCount) throw new ArgumentException("Not all steps has been processed.");
                    break;
            }

            TimeSpan creationDuration = DateTime.Now - this.stageStartTime;
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{namesForStages[this.currentStage]} - time: {creationDuration:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

            this.PrepareNextStage(incrementCurrentStage: true);
        }

        private void FillCellListsForPieceNames()
        {
            var concurrentCellListsForPieceNames = new ConcurrentDictionary<PieceTemplate.Name, List<Cell>>(); // for parallel processing

            Parallel.ForEach(PieceTemplate.allNames, pieceName =>
            {
                var cellList = new List<Cell>();

                foreach (Cell cell in this.allCells)
                {
                    if (cell.allowedNames.Contains(pieceName)) cellList.Add(cell);
                }

                concurrentCellListsForPieceNames[pieceName] = cellList;
            });

            foreach (var kvp in concurrentCellListsForPieceNames)
            {
                this.cellListsForPieceNames[kvp.Key] = kvp.Value;
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
        }

        private void ExtCalculateBiomes()
        {
            var tempRawPointsForBiomeCreation = new ConcurrentBag<Point>();

            while (true)
            {
                byte minVal = Terrain.biomeMin;
                byte maxVal = 255;

                if (!tempRawPointsForBiomeCreation.Any())
                {
                    tempRawPointsForBiomeCreation = this.GetAllRawCoordinatesWithRangeAndWithoutSpecifiedExtProps(terrainName: Terrain.Name.Biome, minVal: minVal, maxVal: maxVal, extPropNames: ExtBoardProps.allBiomes);
                }
                else
                {
                    tempRawPointsForBiomeCreation = this.FilterPointsWithoutSpecifiedExtProps(oldPointBag: tempRawPointsForBiomeCreation, extPropNames: ExtBoardProps.allBiomes, xyRaw: true);
                }

                if (tempRawPointsForBiomeCreation.Any())
                {
                    var biomeName = this.biomeCountByName.OrderBy(kvp => kvp.Value).First().Key;
                    this.biomeCountByName[biomeName]++;

                    ConcurrentBag<Point> biomePoints = this.FloodFillExtProps(
                          startingPoints: new ConcurrentBag<Point> { tempRawPointsForBiomeCreation.First() },
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
                }
                else break;
            }
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
        }

        private void ExtEndCreation()
        {
            this.extBoardProps.EndCreationAndSave();
            this.tempPointsForCreatedBiomes = null;
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

            List<Point> offsetList = new()
            {
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

        public IEnumerable<Sprite> GetSpritesFromSurroundingCells(Sprite sprite, Cell.Group groupName)
        {
            Cell cell = sprite.currentCell == null ? this.FindMatchingCell(sprite.position) : sprite.currentCell;
            return cell.GetSpritesFromSurroundingCells(groupName);
        }

        public IEnumerable<Cell> GetCellsInsideRect(Rectangle rectangle, int padding)
        {
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
            List<Cell> cellsWithinDistance = new();

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

        public List<Sprite> GetSpritesWithinDistance(Sprite mainSprite, int distance, Cell.Group groupName)
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

        public IEnumerable<BoardPiece> GetPiecesWithinDistance(Sprite mainSprite, int distance, Cell.Group groupName, int offsetX = 0, int offsetY = 0, bool compareWithBottom = false)
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
                            currentSprite => Vector2.Distance(new Vector2(currentSprite.GfxRect.Center.X, currentSprite.GfxRect.Bottom), centerPos) <= distance &&
                            currentSprite != mainSprite));
                }
            }
            else
            {
                foreach (Cell cell in cellsWithinDistance)
                {
                    spritesWithinDistance.AddRange(
                        cell.spriteGroups[groupName].Values.Where(
                            currentSprite => Vector2.Distance(currentSprite.position, centerPos) <= distance &&
                            currentSprite != mainSprite));
                }
            }

            var piecesWithinDistance = spritesWithinDistance.Select(sprite => sprite.boardPiece);
            return piecesWithinDistance;
        }

        public IEnumerable<BoardPiece> GetPiecesInsideTriangle(Point point1, Point point2, Point point3, Cell.Group groupName)
        {
            int xMin = (int)Math.Min(Math.Min(point1.X, point2.X), point3.X);
            int xMax = (int)Math.Max(Math.Max(point1.X, point2.X), point3.X);
            int yMin = (int)Math.Min(Math.Min(point1.Y, point2.Y), point3.Y);
            int yMax = (int)Math.Max(Math.Max(point1.Y, point2.Y), point3.Y);

            Rectangle rect = new(x: xMin, y: yMin, width: xMax - xMin, height: yMax - yMin);

            var cellsInsideRect = this.GetCellsInsideRect(rectangle: rect, padding: 1);

            var spritesInsideTriangle = new List<Sprite>();
            foreach (Cell cell in cellsInsideRect)
            {
                spritesInsideTriangle.AddRange(cell.spriteGroups[groupName].Values.Where(
                      currentSprite => rect.Contains(currentSprite.position) && Helpers.IsPointInsideTriangle(point: new Point((int)currentSprite.position.X, (int)currentSprite.position.Y), triangleA: point1, triangleB: point2, triangleC: point3)));
            }

            var piecesInsideTriangle = spritesInsideTriangle.Select(sprite => sprite.boardPiece);
            return piecesInsideTriangle;
        }

        public IEnumerable<BoardPiece> GetPiecesInCameraView(Cell.Group groupName, bool compareWithCameraRect = false)
        {
            Camera camera = this.world.camera;
            var visibleCells = this.GetCellsInsideRect(rectangle: camera.viewRect, padding: 1);
            var spritesInCameraView = new List<Sprite>();

            if (compareWithCameraRect)
            {
                foreach (Cell cell in visibleCells) // making sure that every piece is actually inside camera rect
                {
                    spritesInCameraView.AddRange(cell.spriteGroups[groupName].Values.Where(sprite => camera.viewRect.Intersects(sprite.GfxRect)));
                }
            }
            else
            {
                foreach (Cell cell in visibleCells) // visibleCells area is larger than camera view
                {
                    spritesInCameraView.AddRange(cell.spriteGroups[groupName].Values);
                }
            }

            return spritesInCameraView.Select(sprite => sprite.boardPiece);
        }

        private List<Cell> GetAllCells()
        {
            List<Cell> allCells = new();

            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                { allCells.Add(this.cellGrid[x, y]); }
            }
            return allCells;
        }

        public ConcurrentBag<Sprite> GetSpritesFromAllCells(Cell.Group groupName, bool visitedByPlayerOnly = false)
        {
            var cells = (IEnumerable<Cell>)this.allCells;
            if (visitedByPlayerOnly) cells = this.allCells.Where(cell => cell.VisitedByPlayer);

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

        public ConcurrentBag<Sprite> GetSpritesForRect(Cell.Group groupName, Rectangle rectangle, bool visitedByPlayerOnly = false, int padding = 1)
        {
            var cells = this.GetCellsInsideRect(rectangle: rectangle, padding: padding);
            if (visitedByPlayerOnly) cells = cells.Where(cell => cell.VisitedByPlayer);

            var allSprites = new ConcurrentBag<Sprite> { };

            Parallel.ForEach(cells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName].Values)
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

        public void DrawBackground(Camera camera)
        {
            bool updateFog = false;
            Rectangle cameraRect = camera.viewRect;

            foreach (Cell cell in this.GetCellsInsideRect(rectangle: camera.viewRect, padding: 0))
            {
                cell.DrawBackground();

                if (this.world.MapEnabled && !cell.VisitedByPlayer && cameraRect.Intersects(cell.rect) && camera.IsTrackingPlayer && this.world.Player.CanSeeAnything)
                {
                    cell.SetAsVisited();
                    updateFog = true;
                }
            }

            if (updateFog) this.world.map.dirtyFog = true;
        }

        public int DrawSprites(List<Sprite> blockingLightSpritesList)
        {
            // Sprites should be drawn all at once, because cell-based drawing causes Y sorting order incorrect
            // in cases of sprites overlapping cell boundaries.

            if (Preferences.drawSunShadows)
            {
                AmbientLight.SunLightData sunLightData = AmbientLight.SunLightData.CalculateSunLight(currentDateTime: this.world.islandClock.IslandDateTime, weather: this.world.weather);
                if (sunLightData.sunShadowsColor != Color.Transparent)
                {
                    float sunVisibility = Math.Max(this.world.weather.SunVisibility, this.world.weather.LightningPercentage); // lightning emulates sun
                    if (sunVisibility > 0f)
                    {
                        Color sunShadowsColor = sunLightData.sunShadowsColor * sunVisibility;

                        foreach (Sprite shadowSprite in blockingLightSpritesList)
                        {
                            if (!shadowSprite.Visible) continue;

                            Rectangle cameraRect = this.world.camera.viewRect;

                            if (!shadowSprite.IsInCameraRect && !shadowSprite.boardPiece.pieceInfo.hasFlatShadow)
                            {
                                bool shadowLeftSide = sunLightData.sunPos.X < 0;
                                bool shadowTopSide = sunLightData.sunPos.Y > 0; // must be reversed

                                if (shadowSprite.position.Y < cameraRect.Top &&
                                    (shadowTopSide || shadowSprite.GfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Top - shadowSprite.position.Y)))
                                    continue;

                                if (shadowSprite.position.Y > cameraRect.Bottom &&
                                    (!shadowTopSide || shadowSprite.GfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Bottom - shadowSprite.position.Y)))
                                    continue;

                                if (shadowSprite.position.X > cameraRect.Right &&
                                    (shadowLeftSide || shadowSprite.GfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Right - shadowSprite.position.X)))
                                    continue;

                                if (shadowSprite.position.X < cameraRect.Left &&
                                    (!shadowLeftSide || shadowSprite.GfxRect.Height * sunLightData.sunShadowsLength < Math.Abs(cameraRect.Left - shadowSprite.position.X)))
                                    continue;
                            }

                            Vector2 sunPos = new(shadowSprite.GfxRect.Center.X + sunLightData.sunPos.X, shadowSprite.GfxRect.Bottom + sunLightData.sunPos.Y);
                            float shadowAngle = Helpers.GetAngleBetweenTwoPoints(start: sunPos, end: shadowSprite.position);

                            Sprite.DrawShadow(color: sunShadowsColor, shadowSprite: shadowSprite, lightPos: sunPos, shadowAngle: shadowAngle, yScaleForce: sunLightData.sunShadowsLength);
                        }
                    }
                }
            }

            var visiblePieces = world.Grid.GetPiecesInCameraView(groupName: Cell.Group.Visible, compareWithCameraRect: true);

            foreach (BoardPiece piece in visiblePieces.OrderBy(o => o.sprite.AnimFrame.layer).ThenBy(o => o.sprite.GfxRect.Bottom))
            { piece.sprite.Draw(); }

            StatBar.DrawAll();

            return visiblePieces.Count();
        }

        public void DrawDebugData(bool drawCellData, bool drawPieceData)
        {
            if (!drawCellData && !drawPieceData) return;

            var visibleCells = this.GetCellsInsideRect(rectangle: this.world.camera.viewRect, padding: 0);

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
                    cellGrid[x, y] = new Cell(grid: this, cellNoX: x, cellNoY: y, cellWidth: this.cellWidth, cellHeight: this.cellHeight);
                }
            }

            return cellGrid;
        }

        private void CalculateSurroundingCells() // surrouding cells are calculated only once, because they do not change
        {
            foreach (Cell cell in this.allCells)
            { cell.surroundingCells = this.GetCellsWithinDistance(cell: cell, distance: 1); }
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1)) timeLeftString = timeSpan.ToString("ss");
            else if (timeSpan < TimeSpan.FromHours(1)) timeLeftString = timeSpan.ToString("mm\\:ss");
            else if (timeSpan < TimeSpan.FromDays(1)) timeLeftString = timeSpan.ToString("hh\\:mm\\:ss");
            else timeLeftString = timeSpan.ToString("dd\\:hh\\:mm\\:ss");

            return timeLeftString;
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

        private static Vector2 CalculateMaxFrameSize()
        {
            int frameMaxWidth = 0;
            int frameMaxHeight = 0;

            foreach (var frameList in AnimData.frameListById)
            {
                foreach (AnimFrame frame in frameList.Value)
                {
                    if (frame.ignoreWhenCalculatingMaxSize) continue;

                    int scaledWidth = (int)(frame.gfxWidth * frame.scale);
                    int scaledHeight = (int)(frame.gfxHeight * frame.scale);

                    if (scaledWidth > frameMaxWidth) frameMaxWidth = scaledWidth;
                    if (scaledHeight > frameMaxHeight) frameMaxHeight = scaledHeight;
                }
            }

            return new Vector2(frameMaxWidth, frameMaxHeight);
        }

        public void LoadClosestTexturesInCameraView(Camera camera, bool visitedByPlayerOnly, int maxNoToLoad)
        {
            if (SonOfRobinGame.LastUpdateDelay > 20 || SonOfRobinGame.fps.FPS < 20) return;

            var cellsInCameraViewWithNoTexturesSearch = this.GetCellsInsideRect(rectangle: camera.viewRect, padding: 1).Where(cell => cell.boardGraphics.Texture == null);
            if (visitedByPlayerOnly) cellsInCameraViewWithNoTexturesSearch = cellsInCameraViewWithNoTexturesSearch.Where(cell => cell.VisitedByPlayer);

            var cellsInCameraViewWithNoTextures = cellsInCameraViewWithNoTexturesSearch.ToList();
            if (!cellsInCameraViewWithNoTextures.Any()) return;

            if (SonOfRobinGame.BoardTextureProcessor.CanTakeNewCellsNow)
            {
                var cellsToProcess = cellsInCameraViewWithNoTextures.Where(cell => !cell.boardGraphics.PNGTemplateExists);
                SonOfRobinGame.BoardTextureProcessor.AddCellsToProcess(cellsToProcess);
            }

            Vector2 cameraCenter = camera.CurrentPos;

            var cellsWithPNGByDistance = cellsInCameraViewWithNoTextures
                .Where(cell => cell.boardGraphics.PNGTemplateExists)
                .OrderBy(cell => cell.GetDistance(cameraCenter));

            int noOfLoadedTexturesAtTheStart = this.loadedTexturesCount;
            foreach (Cell cell in cellsWithPNGByDistance)
            {
                cell.boardGraphics.LoadTexture();
                if (this.loadedTexturesCount - noOfLoadedTexturesAtTheStart >= maxNoToLoad || this.world.WorldElapsedUpdateTime.Milliseconds > 14 || SonOfRobinGame.fps.FPS < 20) return;
            }
        }

        public void UnloadTexturesIfMemoryLow(Camera camera)
        {
            if (SonOfRobinGame.CurrentUpdate % 60 != 0 || DateTime.Now - this.lastUnloadedTime < TimeSpan.FromSeconds(60)) return;

            if (SonOfRobinGame.os == OS.Windows)
            {
                if (!SonOfRobinGame.WindowsMemoryLow) return;
            }
            else
            {
                if (this.loadedTexturesCount < Preferences.maxTexturesToLoad) return;
            }

            var cellsInCameraView = this.GetCellsInsideRect(rectangle: camera.viewRect, padding: 1);
            var cellsToUnload = this.allCells.Where(cell => !cellsInCameraView.Contains(cell) && cell.boardGraphics.Texture != null);

            foreach (Cell cell in cellsToUnload)
            {
                cell.boardGraphics.UnloadTexture();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Unloaded texture from cell {cell.cellNoX},{cell.cellNoY}.", color: Color.Pink);
            }
            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Finished unloading textures.", color: Color.Pink);
            this.lastUnloadedTime = DateTime.Now;
        }

        public void UnloadAllTextures()
        {
            foreach (Cell cell in this.allCells)
            {
                cell.boardGraphics.UnloadTexture();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Unloaded texture from cell {cell.cellNoX},{cell.cellNoY}.", color: Color.Pink);
            }
            GC.Collect();
            this.lastUnloadedTime = DateTime.Now;
        }
    }
}