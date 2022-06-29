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
        public bool creationInProgress;
        private int creationStage;
        private DateTime stageStartTime;
        private readonly World world;
        private readonly int noOfCellsX;
        private readonly int noOfCellsY;
        public readonly int cellWidth;
        public readonly int cellHeight;
        public readonly Cell[,] cellGrid;
        public readonly List<Cell> allCells;
        public List<Cell> cellsToProcess;

        public bool ProcessingStepComplete
        {
            get
            { return this.cellsToProcess.Count == 0; }
        }

        public Grid(World world)
        {
            this.creationInProgress = true;
            this.creationStage = 0;

            this.world = world;

            Vector2 maxFrameSize = CalculateMaxFrameSize();
            this.cellWidth = Convert.ToInt32(maxFrameSize.X * 1.5);
            this.cellHeight = Convert.ToInt32(maxFrameSize.Y * 1.5);

            this.noOfCellsX = (int)Math.Ceiling((float)this.world.width / (float)cellWidth);
            this.noOfCellsY = (int)Math.Ceiling((float)this.world.height / (float)cellHeight);

            this.cellGrid = this.MakeGrid();
            this.allCells = this.GetAllCells();
            this.CalculateSurroundingCells();

            if (this.CopyBoardFromTemplate())
            {
                this.creationInProgress = false;
                return;
            }

            this.PrepareNextCreationStage();
        }

        public bool CopyBoardFromTemplate()
        {
            var existingWorlds = Scene.GetAllScenesOfType(typeof(World));
            if (existingWorlds.Count == 0) return false;

            World newWorld = this.world;

            foreach (Scene scene in existingWorlds)
            {
                World oldWorld = (World)scene;
                if (!oldWorld.creationInProgress && oldWorld.grid != null && !oldWorld.grid.creationInProgress && newWorld.seed == oldWorld.seed && newWorld.width == oldWorld.width && newWorld.height == oldWorld.height)
                {
                    Grid templateGrid = oldWorld.grid;

                    for (int x = 0; x < templateGrid.noOfCellsX; x++)
                    {
                        for (int y = 0; y < templateGrid.noOfCellsY; y++)
                            this.cellGrid[x, y].CopyFromTemplate(templateGrid.cellGrid[x, y]);
                    }

                    return true;
                }
            }
            return false;
        }

        public void PrepareNextCreationStage()
        {
            this.stageStartTime = DateTime.Now;
            this.cellsToProcess = this.allCells.ToList();
            this.creationStage++;
        }

        public void RunNextCreationStage()
        {
            switch (this.creationStage)
            {
                case 1:
                    CreationStage1();
                    return;
                case 2:
                    CreationStage2();
                    return;
                default:
                    throw new DivideByZeroException($"Unsupported creationStage - {creationStage}.");
            }
        }

        public void CreationStage1()
        // can be processed using parallel
        {
            var cellProcessingQueue = new List<Cell> { };

            for (int i = 0; i < 25; i++)
            {
                cellProcessingQueue.Add(this.cellsToProcess[0]);
                this.cellsToProcess.RemoveAt(0);
                if (this.cellsToProcess.Count == 0) break;
            }

            Parallel.ForEach(cellProcessingQueue, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            { cell.RunNextCreationStage(); });

            Parallel.ForEach(cellProcessingQueue, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            { cell.RunNextCreationStage(); });

            this.UpdateProgressBar();

            if (this.ProcessingStepComplete) this.PrepareNextCreationStage();
        }

        public void CreationStage2()
        // cannot be processed using parallel (textures don't work in parallel processing)
        {
            for (int i = 0; i < 25; i++)
            {
                Cell cell = this.cellsToProcess[0];
                this.cellsToProcess.RemoveAt(0);
                cell.RunNextCreationStage();

                if (this.ProcessingStepComplete)
                {
                    this.creationInProgress = false;
                    return;
                }
            }

            if (!this.ProcessingStepComplete) this.UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            if (this.world.demoMode) return;

            TimeSpan timeLeft = CalculateTimeLeft(
                startTime: this.stageStartTime, completeAmount: this.allCells.Count - this.cellsToProcess.Count, totalAmount: this.allCells.Count);
            string timeLeftString = FormatTimeSpanString(timeLeft + TimeSpan.FromSeconds(1));

            string message;

            switch (this.creationStage)
            {
                case 1:
                    message = $":: preparing island ::\nseed {this.world.seed}\n{this.world.width} x {this.world.height}\npreparation time left {timeLeftString}";
                    break;

                case 2:
                    message = $":: preparing island ::\nseed {this.world.seed}\n{this.world.width} x {this.world.height}\nstarting in {timeLeftString}";
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported creationStage - {creationStage}.");
            }

            ProgressBar.TurnOnBgColor();

            ProgressBar.ChangeValues(
                            curVal: this.allCells.Count - this.cellsToProcess.Count,
                            maxVal: this.allCells.Count,
                            text: message);

        }

        private static string FormatTimeSpanString(TimeSpan timeSpan)
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
            { return TimeSpan.FromHours(1); }
        }

        public void AddToGroup(Sprite sprite, Cell.Group groupName)
        { sprite.currentCell.AddToGroup(sprite: sprite, groupName: groupName); }

        public void RemoveFromGroup(Sprite sprite, Cell.Group groupName)
        {
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

            if (sprite.currentCell == null)
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
            Cell cell = (sprite.currentCell is null) ? this.FindMatchingCell(sprite.position) : sprite.currentCell;
            return cell.GetSpritesFromSurroundingCells(groupName);
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
                { if (Vector2.Distance(currentSprite.position, mainSprite.position) <= distance && currentSprite != mainSprite) spritesWithinDistance.Add(currentSprite); }
            }

            return spritesWithinDistance;
        }

        public List<BoardPiece> GetPiecesWithinDistance(Sprite mainSprite, ushort distance, Cell.Group groupName, int offsetX = 0, int offsetY = 0)
        {
            var cellsWithinDistance = this.GetCellsWithinDistance(position: mainSprite.position, distance: distance);
            var piecesWithinDistance = new List<BoardPiece>();

            Vector2 centerPos = mainSprite.position + new Vector2(offsetX, offsetY);

            foreach (Cell cell in cellsWithinDistance)
            {
                foreach (Sprite currentSprite in cell.spriteGroups[groupName].Values)
                { if (Vector2.Distance(currentSprite.position, centerPos) <= distance && currentSprite != mainSprite) piecesWithinDistance.Add(currentSprite.boardPiece); }
            }

            return piecesWithinDistance;
        }

        public List<Cell> GetCellsInCameraView(Camera camera)
        {
            Rectangle viewRect = camera.viewRect;

            // +1 cell on each side, to ensure visibility of sprites, that cross their cell's boundaries
            int xMinCellNo = Math.Max(FindMatchingCellInSingleAxis(position: viewRect.Left, cellLength: this.cellWidth) - 1, 0);
            int xMaxCellNo = Math.Min(FindMatchingCellInSingleAxis(position: viewRect.Right, cellLength: this.cellWidth) + 1, this.noOfCellsX - 1);
            int yMinCellNo = Math.Max(FindMatchingCellInSingleAxis(position: viewRect.Top, cellLength: this.cellHeight) - 1, 0);
            int yMaxCellNo = Math.Min(FindMatchingCellInSingleAxis(position: viewRect.Bottom, cellLength: this.cellHeight) + 1, this.noOfCellsY - 1);

            List<Cell> visibleCells = new List<Cell>();

            for (int x = xMinCellNo; x <= xMaxCellNo; x++)
            {
                for (int y = yMinCellNo; y <= yMaxCellNo; y++)
                { visibleCells.Add(this.cellGrid[x, y]); }
            }

            return visibleCells;
        }

        public List<Sprite> GetSpritesInCameraView(Camera camera, Cell.Group groupName)
        {
            var visibleCells = this.GetCellsInCameraView(camera: camera);
            List<Sprite> visibleSprites = new List<Sprite>();

            foreach (Cell cell in visibleCells)
            {
                visibleSprites.AddRange(cell.spriteGroups[groupName].Values.ToList());
            }

            return visibleSprites;
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

        public ConcurrentBag<Sprite> GetAllSprites(Cell.Group groupName)
        {
            var allSprites = new ConcurrentBag<Sprite> { };

            Parallel.ForEach(this.allCells, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, cell =>
            {
                foreach (Sprite sprite in cell.spriteGroups[groupName].Values)
                { allSprites.Add(sprite); }
            });

            return allSprites;
        }

        public void DrawBackground(Camera camera)
        {
            var visibleCells = this.GetCellsInCameraView(camera: camera).OrderBy(o => o.cellNoY);
            foreach (Cell cell in visibleCells)
            { cell.DrawBackground(); }
        }

        public int DrawSprites(Camera camera)
        {
            // Sprites should be drawn all at once, because cell-based drawing causes Y sorting order incorrect
            // in cases of sprites overlapping cell boundaries.

            var visibleSprites = this.GetSpritesInCameraView(camera: camera, groupName: Cell.Group.Visible);
            var sortedSprites = visibleSprites.OrderBy(o => o.frame.layer).ThenBy(o => o.position.Y).ToList();

            foreach (Sprite sprite in sortedSprites)
            { sprite.Draw(); }

            StatBar.DrawAll();

            return visibleSprites.Count;
        }

        public void DrawDebugData()
        {
            var visibleCells = this.GetCellsInCameraView(camera: this.world.camera);

            foreach (Cell cell in visibleCells)
            { cell.DrawDebugData(groupName: Cell.Group.ColBlocking); }
        }

        public byte GetFieldValue(TerrainName terrainName, Vector2 position)
        {
            int cellNoX = Convert.ToInt32(Math.Floor(position.X / cellWidth));
            int cellNoY = Convert.ToInt32(Math.Floor(position.Y / cellHeight));

            int posInsideCellX = (int)position.X % cellWidth;
            int posInsideCellY = (int)position.Y % cellHeight;

            return this.cellGrid[cellNoX, cellNoY].terrainByName[terrainName].mapData[posInsideCellX, posInsideCellY];
        }


        private Cell FindMatchingCell(Vector2 position)
        {
            return this.cellGrid[(int)Math.Floor(position.X / this.cellWidth), (int)Math.Floor(position.Y / this.cellHeight)];
        }

        private static int FindMatchingCellInSingleAxis(int position, int cellLength)
        {
            return (int)Math.Floor((float)position / (float)cellLength);
        }


        private Cell[,] MakeGrid()
        {
            Cell[,] cellGrid = new Cell[this.noOfCellsX, this.noOfCellsY];

            for (int x = 0; x < this.noOfCellsX; x++)
            {
                for (int y = 0; y < this.noOfCellsY; y++)
                {
                    int xMin = x * this.cellWidth;
                    int xMax = Math.Min(((x + 1) * this.cellWidth) - 1, this.world.width - 1);
                    int yMin = y * this.cellHeight;
                    int yMax = Math.Min(((y + 1) * this.cellHeight) - 1, this.world.height - 1);

                    cellGrid[x, y] = new Cell(world: world, cellNoX: x, cellNoY: y, xMin: xMin, xMax: xMax, yMin: yMin, yMax: yMax, random: this.world.random);
                }
            }

            return cellGrid;
        }

        private void CalculateSurroundingCells() // surrouding cells are calculated only once, because they do not change
        {
            foreach (Cell cell in this.allCells)
            { cell.surroundingCells = GetCellsWithinDistance(cell: cell, distance: 1); }

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
                    if (frame.gfxWidth > frameMaxWidth) { frameMaxWidth = frame.gfxWidth; }
                    if (frame.gfxHeight > frameMaxHeight) { frameMaxHeight = frame.gfxHeight; }
                }
            }

            return new Vector2(frameMaxWidth, frameMaxHeight);
        }
    }
}
