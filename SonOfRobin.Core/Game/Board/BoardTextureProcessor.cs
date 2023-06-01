using Microsoft.Xna.Framework;
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
        private ConcurrentBag<Cell> cellsToProcess;

        public BoardTextureProcessor()
        {
            this.cellsToProcess = new ConcurrentBag<Cell>();
            this.StartBackgroundTask();
        }

        private void StartBackgroundTask()
        {
            this.backgroundTask = Task.Run(() => this.ProcessingLoop());
        }

        public void AddCellToProcess(Cell cell)
        {
            this.cellsToProcess.Add(cell);

            if (this.backgroundTask != null && this.backgroundTask.IsFaulted)
            {
                new TextWindow(text: $"An error occured while processing background task:\n{this.backgroundTask.Exception}",
                    textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, priority: -1, inputType: Scene.InputTypes.Normal);

                this.StartBackgroundTask(); // starting new task, if previous one has failed
            }
        }

        private void ProcessingLoop()
        {
            while (true)
            {
                if (!this.cellsToProcess.Any()) Thread.Sleep(1);
                else
                {
                    foreach (Cell cell in this.cellsToProcess)
                    {
                        try
                        {
                            cell.boardGraphics.CreateAndSavePngTemplate();
                        }
                        catch (System.AggregateException) { } // if main thread is using png file
                        catch (IOException) { } // if main thread is using png file
                    }

                    this.cellsToProcess.Clear();
                }
            }
        }
    }
}