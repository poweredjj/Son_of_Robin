using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class Grid
    {
        public enum Stage
        { GenerateTerrain, SetExtData, ProcessTextures, LoadTextures }

        private static readonly int allStagesCount = ((Stage[])Enum.GetValues(typeof(Stage))).Length;

        private static readonly Dictionary<Stage, string> namesForStages = new Dictionary<Stage, string> {
            { Stage.GenerateTerrain, "generating terrain" },
            { Stage.SetExtData, "setting extended data" },
            { Stage.ProcessTextures, "processing textures" },
            { Stage.LoadTextures, "loading textures" },
        };

        private Stage currentStage;
        private DateTime stageStartTime;
        private int stageStartFrame;
        public bool creationInProgress;

        public readonly GridTemplate gridTemplate;
        public readonly World world;
        private readonly int noOfCellsX;
        private readonly int noOfCellsY;
        public readonly int cellWidth;
        public readonly int cellHeight;
        public readonly int resDivider;
        public readonly Cell[,] cellGrid;
        public readonly List<Cell> allCells;
        public readonly List<Cell> cellsToProcessOnStart;
        private readonly Dictionary<PieceTemplate.Name, List<Cell>> cellListsForPieceNames; // pieces and cells that those pieces can (initially) be placed into
        private DateTime lastCellProcessedTime;
        public int loadedTexturesCount;

        private static readonly TimeSpan textureLoadingDelay = TimeSpan.FromMilliseconds(10);
        public bool ProcessingStageComplete
        { get { return this.cellsToProcessOnStart.Count == 0; } }
        public List<Cell> CellsVisitedByPlayer
        { get { return this.allCells.Where(cell => cell.VisitedByPlayer).ToList(); } }
        public List<Cell> CellsNotVisitedByPlayer
        { get { return this.allCells.Where(cell => !cell.VisitedByPlayer).ToList(); } }

        public Grid(World world, int resDivider, int cellWidth = 0, int cellHeight = 0)
        {
            this.creationInProgress = true;
            this.currentStage = 0;

            this.world = world;
            this.resDivider = resDivider;

            if (cellWidth == 0 && cellHeight == 0)
            {
                Vector2 maxFrameSize = CalculateMaxFrameSize();
                this.cellWidth = Convert.ToInt32(maxFrameSize.X * 1.3);
                this.cellHeight = Convert.ToInt32(maxFrameSize.Y * 1.3);

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

            this.lastCellProcessedTime = DateTime.Now;

            if (this.CopyBoardFromTemplate())
            {
                this.creationInProgress = false;
                return;
            }

            this.cellsToProcessOnStart = new List<Cell>();

            this.PrepareNextStage();
        }

        public Dictionary<string, Object> Serialize()
        {
            var cellData = new List<Object> { };

            foreach (Cell cell in this.allCells)
            {
                cellData.Add(cell.Serialize());
            }

            Dictionary<string, Object> gridData = new Dictionary<string, object>
            {
                {"cellWidth", this.cellWidth },
                {"cellHeight", this.cellHeight },
                {"cellData", cellData },
            };

            return gridData;
        }

        public static Grid Deserialize(Dictionary<string, Object> gridData, World world, int resDivider)
        {
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
                if (!oldWorld.worldCreationInProgress && oldWorld.grid != null && !oldWorld.grid.creationInProgress && newWorld.seed == oldWorld.seed && newWorld.width == oldWorld.width && newWorld.height == oldWorld.height)
                {
                    Grid templateGrid = oldWorld.grid;

                    for (int x = 0; x < templateGrid.noOfCellsX; x++)
                    {
                        for (int y = 0; y < templateGrid.noOfCellsY; y++)
                            this.cellGrid[x, y].CopyFromTemplate(templateGrid.cellGrid[x, y]);
                    }

                    this.FillCellListsForPieceNames();

                    return true;
                }
            }
            return false;
        }

        public void ProcessNextCreationStage()
        {
            if (this.currentStage != Stage.LoadTextures && SonOfRobinGame.currentUpdate < this.stageStartFrame + 3)
            {
                // first frame of each stage should update progress bar
                this.UpdateProgressBar();
                return;
            }

            List<Cell> cellProcessingQueue;

            switch (currentStage)
            {
                case Stage.GenerateTerrain:

                    cellProcessingQueue = new List<Cell> { };

                    for (int i = 0; i < 40 * Preferences.newWorldResDivider; i++)
                    {
                        cellProcessingQueue.Add(this.cellsToProcessOnStart[0]);
                        this.cellsToProcessOnStart.RemoveAt(0);
                        if (this.cellsToProcessOnStart.Count == 0) break;
                    }

                    Parallel.ForEach(cellProcessingQueue, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
                    {
                        cell.ComputeHeight();
                    });

                    Parallel.ForEach(cellProcessingQueue, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
                    {
                        cell.ComputeHumidity();
                    });

                    Parallel.ForEach(cellProcessingQueue, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
                    {
                        cell.ComputeDanger();
                        cell.FillAllowedNames(); // needs to be invoked after calculating final terrain
                    });

                    this.FillCellListsForPieceNames();

                    break;

                case Stage.SetExtData:

                    this.CalculateExtPropertiesForWholeMap();
                    break;

                case Stage.ProcessTextures:

                    cellProcessingQueue = new List<Cell> { };

                    for (int i = 0; i < 30; i++)
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
                        int maxCellsToProcess = 40;

                        for (int i = 0; i < maxCellsToProcess; i++)
                        {
                            Cell cell = this.cellsToProcessOnStart[0];
                            this.cellsToProcessOnStart.RemoveAt(0);
                            cell.boardGraphics.LoadTexture();

                            if (this.ProcessingStageComplete)
                            {
                                this.creationInProgress = false;
                                return;
                            }
                        }
                    }
                    else
                    {
                        this.cellsToProcessOnStart.Clear();
                        this.creationInProgress = false;
                        return;
                    }

                    break;

                default:
                    if ((int)this.currentStage < allStagesCount) throw new ArgumentException("Not all steps has been processed.");

                    this.creationInProgress = false;
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

        private void CalculateExtPropertiesForWholeMap()
        {
            var cellsWithoutExtPropertiesSet = this.allCells.Where(cell => cell.ExtBoardProperties.CreationInProgress);
            if (!cellsWithoutExtPropertiesSet.Any())
            {
                this.cellsToProcessOnStart.Clear();
                return;
            }

            // TODO add ext calculation code here

            int width = this.world.width;
            int height = this.world.height;
            int dividedWidth = width / this.resDivider;
            int dividedHeight = height / this.resDivider;

            bool[,] processedMap = new bool[dividedWidth, dividedHeight]; // working inside "compressed" map space, to avoid unnecessary calculations

            List<Point> offsetList = new List<Point> {
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, -1),
                new Point(0, 1),
            };

            List<Point> nextPointsList = new List<Point> { new Point(0, 0) }; // algorithm will only work, if water surrounds the island and is present at the edge 

            while (true)
            {
                foreach (Point currentPoint in nextPointsList.ToList())
                {
                    nextPointsList.Clear();

                    int realX = currentPoint.X * this.resDivider;
                    int realY = currentPoint.Y * this.resDivider;

                    byte heightVal = this.GetFieldValue(terrainName: TerrainName.Height, x: realX, y: realY);

                    bool water = heightVal < Terrain.waterLevelMax;

                    if (water)
                    {
                        this.SetExtProperty(name: ExtBoardProperties.ExtPropName.Sea, value: true, x: realX, y: realY);

                        foreach (Point currentOffset in offsetList)
                        {
                            Point nextPoint = new Point(currentPoint.X + currentOffset.X, currentPoint.Y + currentOffset.Y);

                            if (nextPoint.X >= 0 && nextPoint.X < dividedWidth &&
                                nextPoint.Y >= 0 && nextPoint.Y < dividedHeight &&
                                !processedMap[nextPoint.X, nextPoint.Y])
                            {
                                nextPointsList.Add(nextPoint);
                            }
                        }

                    }
                    else
                    {
                        this.SetExtProperty(name: ExtBoardProperties.ExtPropName.OuterBeachEdge, value: true, x: realX, y: realY);
                    }

                    processedMap[currentPoint.X, currentPoint.Y] = true;
                }

                if (!nextPointsList.Any()) break;
            }



            // TODO add ext calculation code here

            foreach (Cell cell in cellsWithoutExtPropertiesSet)
            {
                cell.ExtBoardProperties.EndCreationAndSave();
            }

            this.cellsToProcessOnStart.Clear();
        }

        public List<Cell> GetCellsForPieceName(PieceTemplate.Name pieceName)
        {
            return this.cellListsForPieceNames[pieceName].ToList(); // ToList() - to avoid modifying original list
        }

        public Cell GetRandomCellForPieceName(PieceTemplate.Name pieceName) // to avoid calling GetCellsForPieceName() for getting one random cell only
        {
            var cellList = this.cellListsForPieceNames[pieceName];
            return cellList[this.world.random.Next(0, cellList.Count)];
        }

        private void PrepareNextStage()
        {
            this.stageStartFrame = SonOfRobinGame.currentUpdate;
            this.stageStartTime = DateTime.Now;
            this.cellsToProcessOnStart.Clear();
            this.cellsToProcessOnStart.AddRange(this.allCells);
        }

        private void UpdateProgressBar()
        {
            if (this.world.demoMode) return;

            TimeSpan timeLeft = CalculateTimeLeft(startTime: this.stageStartTime, completeAmount: this.allCells.Count - this.cellsToProcessOnStart.Count, totalAmount: this.allCells.Count);
            string timeLeftString = TimeSpanToString(timeLeft + TimeSpan.FromSeconds(1));

            string seedText = String.Format("{0:0000}", this.world.seed);
            string message = $"preparing island\nseed {seedText}\n{this.world.width} x {this.world.height}\n{namesForStages[this.currentStage]} {timeLeftString}";

            SonOfRobinGame.progressBar.TurnOn(
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
            { return TimeSpan.FromMinutes(30); }
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
                    spritesWithinDistance.AddRange(cell.spriteGroups[groupName].Values.Where(
                                          currentSprite => Vector2.Distance(new Vector2(currentSprite.gfxRect.Center.X, currentSprite.gfxRect.Bottom), centerPos) <= distance &&
                                          currentSprite != mainSprite).ToList());
                }
            }
            else
            {
                foreach (Cell cell in cellsWithinDistance)
                {
                    spritesWithinDistance.AddRange(cell.spriteGroups[groupName].Values.Where(
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
                cell.DrawBackground();

                if (this.world.MapEnabled && !cell.VisitedByPlayer && cameraRect.Intersects(cell.rect) && camera.IsTrackingPlayer && this.world.player.CanSeeAnything)
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

        public void DrawDebugData()
        {
            var visibleCells = this.GetCellsInsideRect(this.world.camera.viewRect);

            foreach (Cell cell in visibleCells)
            { cell.DrawDebugData(groupName: Cell.Group.ColMovement); }
        }

        public byte GetFieldValue(TerrainName terrainName, Vector2 position)
        {
            int cellNoX = (int)Math.Floor(position.X / this.cellWidth);
            int cellNoY = (int)Math.Floor(position.Y / this.cellHeight);

            int posInsideCellX = (int)position.X % this.cellWidth;
            int posInsideCellY = (int)position.Y % this.cellHeight;

            return this.cellGrid[cellNoX, cellNoY].terrainByName[terrainName].GetMapData(posInsideCellX, posInsideCellY);
        }

        public byte GetFieldValue(TerrainName terrainName, int x, int y)
        {
            int cellNoX = x / this.cellWidth;
            int cellNoY = y / this.cellHeight;

            int posInsideCellX = x % this.cellWidth;
            int posInsideCellY = y % this.cellHeight;

            return this.cellGrid[cellNoX, cellNoY].terrainByName[terrainName].GetMapData(posInsideCellX, posInsideCellY);
        }

        public bool GetExtProperty(ExtBoardProperties.ExtPropName name, Vector2 position)
        {
            int cellNoX = (int)Math.Floor(position.X / this.cellWidth);
            int cellNoY = (int)Math.Floor(position.Y / this.cellHeight);

            int posInsideCellX = (int)position.X % this.cellWidth;
            int posInsideCellY = (int)position.Y % this.cellHeight;

            return this.cellGrid[cellNoX, cellNoY].ExtBoardProperties.GetValue(name: name, x: posInsideCellX, y: posInsideCellY, xyRaw: false);
        }

        public bool GetExtProperty(ExtBoardProperties.ExtPropName name, int x, int y)
        {
            int cellNoX = x / this.cellWidth;
            int cellNoY = y / this.cellHeight;

            int posInsideCellX = x % this.cellWidth;
            int posInsideCellY = y % this.cellHeight;

            return this.cellGrid[cellNoX, cellNoY].ExtBoardProperties.GetValue(name: name, x: posInsideCellX, y: posInsideCellY, xyRaw: false);
        }

        public void SetExtProperty(ExtBoardProperties.ExtPropName name, bool value, int x, int y)
        {
            int cellNoX = x / this.cellWidth;
            int cellNoY = y / this.cellHeight;

            int posInsideCellX = x % this.cellWidth;
            int posInsideCellY = y % this.cellHeight;

            this.cellGrid[cellNoX, cellNoY].ExtBoardProperties.SetValue(name: name, value: value, x: posInsideCellX, y: posInsideCellY, xyRaw: false);
        }

        public Cell FindMatchingCell(Vector2 position)
        {
            return this.cellGrid[(int)(position.X / this.cellWidth), (int)(position.Y / this.cellHeight)];
        }

        private static int FindMatchingCellInSingleAxis(int position, int cellLength)
        {
            return position / cellLength;
        }

        private Cell[,] MakeGrid()
        {
            Cell[,] cellGrid = new Cell[this.noOfCellsX, this.noOfCellsY];

            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                {
                    cellGrid[x, y] = new Cell(world: this.world, grid: this, cellNoX: x, cellNoY: y, width: this.cellWidth, height: this.cellHeight, random: this.world.random);
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
                foreach (var frame in frameList.Value)
                {
                    if (frame.gfxWidth > frameMaxWidth) frameMaxWidth = frame.gfxWidth;
                    if (frame.gfxHeight > frameMaxHeight) frameMaxHeight = frame.gfxHeight;
                }
            }

            return new Vector2(frameMaxWidth, frameMaxHeight);
        }

        public void LoadClosestTextureInCameraView(Camera camera, bool visitedByPlayerOnly)
        {
            if (Preferences.loadWholeMap || DateTime.Now - this.lastCellProcessedTime < textureLoadingDelay) return;

            var cellsInCameraViewWithNoTextures = this.GetCellsInsideRect(camera.viewRect).Where(cell => cell.boardGraphics.texture == null);
            if (visitedByPlayerOnly) cellsInCameraViewWithNoTextures = cellsInCameraViewWithNoTextures.Where(cell => cell.VisitedByPlayer);
            if (!cellsInCameraViewWithNoTextures.Any()) return;

            Vector2 cameraCenter = camera.CurrentPos;

            var cellsByDistance = cellsInCameraViewWithNoTextures.OrderBy(cell => cell.GetDistance(cameraCenter));
            cellsByDistance.First().boardGraphics.LoadTexture();
            this.lastCellProcessedTime = DateTime.Now;
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
            var cellsToUnload = this.allCells.Where(cell => !cellsInCameraView.Contains(cell) && cell.boardGraphics.texture != null).ToList();

            foreach (Cell cell in cellsToUnload)
            {
                cell.boardGraphics.UnloadTexture();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Unloaded texture from cell {cell.cellNoX},{cell.cellNoY}.", color: Color.Pink);
            }
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