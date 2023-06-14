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
        private Task backgroundTask;
        private ConcurrentDictionary<DateTime, Cell> cellsToProcessByRequestTime;
        private DateTime lastClearTime;

        public int RequestsInQueueCount
        { get { return this.cellsToProcessByRequestTime.Count; } }

        public BoardTextureProcessor()
        {
            this.lastClearTime = DateTime.Now;
            this.cellsToProcessByRequestTime = new ConcurrentDictionary<DateTime, Cell>();
            this.StartBackgroundTask();
        }

        private void StartBackgroundTask()
        {
            this.backgroundTask = Task.Run(() => this.ProcessingLoop());
        }

        public void AddCellToProcess(Cell cell)
        {
            this.cellsToProcessByRequestTime[DateTime.Now] = cell;

            if (this.backgroundTask != null && this.backgroundTask.IsFaulted)
            {
                if (SonOfRobinGame.platform != Platform.Mobile) SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: false);

                MessageLog.AddMessage(msgType: MsgType.Debug, message: "An error occured while processing background task. Restarting task.", color: Color.Orange);

                this.StartBackgroundTask(); // starting new task, if previous one had failed
            }
        }

        private void ProcessingLoop()
        {
            var cellsToProcess = new List<Cell>();

            while (true)
            {
                try
                {
                    this.RemoveOldRequests();

                    if (!this.cellsToProcessByRequestTime.Any()) Thread.Sleep(1); // to avoid high CPU usage
                    else
                    {
                        // newest request always takes the priority
                        while (true)
                        {
                            DateTime requestTimeToUse = this.cellsToProcessByRequestTime.OrderByDescending(kvp => kvp.Key).First().Key;

                            Cell cell;
                            bool removedCorrectly = this.cellsToProcessByRequestTime.TryRemove(requestTimeToUse, out cell);
                            if (removedCorrectly && cell != null && !cell.grid.world.HasBeenRemoved && !cellsToProcess.Contains(cell)) cellsToProcess.Add(cell);

                            if (!removedCorrectly || cellsToProcess.Count > 32 || !this.cellsToProcessByRequestTime.Any()) break;
                        }

                        if (cellsToProcess.Any()) this.ProcessCellsBatch(cellsToProcess);
                    }
                }
                catch (Exception ex)
                {
                    SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: ex, showTextWindow: false);
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"An error occured while processing background task: {ex.Message}");
                }
            }
        }

        private void ProcessCellsBatch(List<Cell> cellsToProcess)
        {
            if (cellsToProcess.Count >= 8)
            {
                Parallel.ForEach(cellsToProcess, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse / 2 }, cell =>
                {
                    cell.boardGraphics.CreateBitmapFromTerrainAndSaveAsPNG();
                });
            }
            else
            {
                foreach (Cell cell in cellsToProcess)
                {
                    cell.boardGraphics.CreateBitmapFromTerrainAndSaveAsPNG();
                }
            }

            cellsToProcess.Clear();
        }

        private void RemoveOldRequests()
        {
            if (DateTime.Now - this.lastClearTime < TimeSpan.FromSeconds(10) || this.RequestsInQueueCount == 0) return;

            int requestCountBefore = this.RequestsInQueueCount;

            TimeSpan requestTimeout = TimeSpan.FromSeconds(30);
            var recentCellsToProcess = this.cellsToProcessByRequestTime.Where(kvp => DateTime.Now - kvp.Key < requestTimeout);

            this.cellsToProcessByRequestTime.Clear();
            foreach (var kvp in recentCellsToProcess)
            {
                this.cellsToProcessByRequestTime[kvp.Key] = kvp.Value;
            }

            int requestCountAfter = recentCellsToProcess.Count();
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"BgTxProcessor - removing old: {requestCountBefore} -> {requestCountAfter}", color: Color.LimeGreen);

            this.lastClearTime = DateTime.Now;
        }
    }
}