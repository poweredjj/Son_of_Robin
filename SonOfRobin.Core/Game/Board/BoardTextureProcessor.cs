using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.IO;
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
                new TextWindow(text: $"An error occured while processing background task:\n{this.backgroundTask.Exception}",
                    textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, priority: -1, inputType: Scene.InputTypes.Normal);

                this.StartBackgroundTask(); // starting new task, if previous one had failed
            }
        }

        private void ProcessingLoop()
        {
            while (true)
            {
                this.RemoveOldRequests();

                if (!this.cellsToProcessByRequestTime.Any()) Thread.Sleep(1); // to avoid high CPU usage
                else
                {
                    // newest request always takes the priority
                    var cellsToProcessByRequestTimeDescending = this.cellsToProcessByRequestTime.OrderByDescending(kvp => kvp.Key);
                    if (cellsToProcessByRequestTimeDescending.Any())
                    {
                        DateTime requestTimeToUse = cellsToProcessByRequestTimeDescending.First().Key;

                        Cell cell;
                        this.cellsToProcessByRequestTime.TryRemove(requestTimeToUse, out cell);

                        if (cell != null)
                        {
                            try
                            {
                                if (!cell.grid.world.HasBeenRemoved) cell.boardGraphics.CreateAndSavePngTemplate();
                            }
                            catch (AggregateException) { } // if main thread is using png file
                            catch (IOException) { } // if main thread is using png file
                        }
                    }                 
                }
            }
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