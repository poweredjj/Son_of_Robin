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
        private int requestNo;
        private Task backgroundTask;
        private ConcurrentDictionary<int, Cell> cellsToProcessByRequestNo;

        public BoardTextureProcessor()
        {
            this.requestNo = 0;
            this.cellsToProcessByRequestNo = new ConcurrentDictionary<int, Cell>();
            this.StartBackgroundTask();
        }

        private void StartBackgroundTask()
        {
            this.backgroundTask = Task.Run(() => this.ProcessingLoop());
        }

        public void AddCellToProcess(Cell cell)
        {
            this.requestNo++;

            this.cellsToProcessByRequestNo[this.requestNo] = cell;

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
                if (!this.cellsToProcessByRequestNo.Any()) Thread.Sleep(1); // to avoid high CPU usage
                else
                {
                    // newest request always takes the priority
                    int requestNoToUse = this.cellsToProcessByRequestNo.OrderByDescending(kvp => kvp.Key).First().Key;

                    Cell cell;
                    this.cellsToProcessByRequestNo.TryRemove(requestNoToUse, out cell);

                    if (cell != null)
                    {
                        try
                        {
                            cell.boardGraphics.CreateAndSavePngTemplate();
                        }
                        catch (AggregateException) { } // if main thread is using png file
                        catch (IOException) { } // if main thread is using png file
                    }
                }
            }
        }
    }
}