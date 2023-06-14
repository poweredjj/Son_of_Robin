using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class BoardTextureProcessor
    {
        public struct ProcessingTask
        {
            private static readonly TimeSpan maxWaitingTime = TimeSpan.FromSeconds(5);

            public readonly Cell cell;
            private readonly DateTime addedTime;
            public bool IsTimedOut { get { return DateTime.Now - this.addedTime > maxWaitingTime; } }

            public ProcessingTask(Cell cell)
            {
                this.cell = cell;
                this.addedTime = DateTime.Now;
            }

            public void Process()
            {
                if (!this.IsTimedOut && this.cell.boardGraphics.Texture == null) this.cell.boardGraphics.CreateBitmapFromTerrainAndSaveAsPNG();
            }
        }

        private static readonly TimeSpan cooldownTime = TimeSpan.FromMilliseconds(300);

        private Task backgroundTask;
        private readonly ConcurrentBag<ProcessingTask> tasksToProcess;
        private DateTime lastCellAddedTime;

        public TimeSpan TimeSinceLastCellAdded
        { get { return DateTime.Now - lastCellAddedTime; } }

        public bool CanTakeNewCellsNow // information about being in a "cooldown" phase (new cells can be added, if that's really needed)
        { get { return this.TimeSinceLastCellAdded > cooldownTime; } }

        public BoardTextureProcessor()
        {
            this.lastCellAddedTime = DateTime.MinValue;
            this.tasksToProcess = new ConcurrentBag<ProcessingTask>();
            this.StartBackgroundTask();
        }

        private void StartBackgroundTask()
        {
            this.backgroundTask = Task.Run(() => this.ProcessingLoop());
        }

        public void AddCellsToProcess(IEnumerable<Cell> cellList)
        {
            foreach (Cell cell in cellList)
            {
                this.tasksToProcess.Add(new ProcessingTask(cell));
            }

            this.lastCellAddedTime = DateTime.Now;
            this.CheckAndRunBackgroundTask();
        }

        public void AddCellToProcess(Cell cell)
        {
            this.tasksToProcess.Add(new ProcessingTask(cell));

            this.lastCellAddedTime = DateTime.Now;
            this.CheckAndRunBackgroundTask();
        }

        private void CheckAndRunBackgroundTask()
        {
            if (this.backgroundTask != null && this.backgroundTask.IsFaulted)
            {
                if (SonOfRobinGame.platform != Platform.Mobile) SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: false);
                MessageLog.AddMessage(msgType: MsgType.Debug, message: "An error occured while processing background task. Restarting task.", color: Color.Orange);

                this.StartBackgroundTask(); // starting new task, if previous one had failed
            }
        }

        private void ProcessingLoop()
        {
            while (true)
            {
                try
                {
                    if (!this.tasksToProcess.Any()) Thread.Sleep(1); // to avoid high CPU usage
                    else
                    {
                        var tasksBatch = this.tasksToProcess.ToList();
                        this.tasksToProcess.Clear();
                        this.ProcessCellsBatch(tasksBatch);
                    }
                }
                catch (Exception ex)
                {
                    SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: ex, showTextWindow: false);
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"An error occured while processing background task: {ex.Message}");
                }
            }
        }

        private void ProcessCellsBatch(List<ProcessingTask> tasksToProcess)
        {
            if (!tasksToProcess.Any()) return;

            Vector2 cameraCenter = tasksToProcess[0].cell.grid.world.camera.CurrentPos;

            var tasksByDistance = tasksToProcess
                .Where(task => !task.IsTimedOut)
                .Distinct()
                .OrderBy(task => task.cell.GetDistance(cameraCenter))
                .Take(100) // to avoid taking too much time processing one batch
                .ToList();

            if (tasksByDistance.Count() >= 8)
            {
                Parallel.ForEach(tasksByDistance, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse / 2 }, task =>
                {
                    task.Process();
                });
            }
            else
            {
                foreach (ProcessingTask task in tasksByDistance)
                {
                    task.Process();
                }
            }
        }
    }
}