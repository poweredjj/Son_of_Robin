using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class LoaderSaver : Scene
    {
        private const string headerName = "header.json";
        private const string hintsName = "hints.json";
        private const string trackingName = "tracking.json";
        private const string eventsName = "events.json";
        private const string weatherName = "weather.json";
        private const string gridName = "grid.json";
        public const string tempPrefix = "_save_temp_";

        private const int maxPiecesInPackage = 1000; // using small piece packages lowers ram usage during writing binary files

        private readonly DateTime createdTime;
        private readonly bool quitGameAfterSaving;
        private readonly bool saveMode;
        private readonly string modeText;
        private readonly bool showSavedMessage;
        private bool processingComplete;
        private World world;
        private readonly List<List<BoardPiece>> piecePackagesToSave;
        private readonly string saveSlotName;
        private readonly string savePath;
        private readonly string saveTempPath;
        private int currentPiecePackageNo;
        private bool allPiecesProcessed;

        private bool directoryChecked;
        private bool gridTemplateFound;

        // loading mode uses data variables
        private Dictionary<string, Object> headerData;

        private Dictionary<string, Object> hintsData;
        private Dictionary<string, Object> weatherData;
        private Dictionary<string, Object> gridData;
        private List<Object> trackingData;
        private List<Object> eventsData;
        private readonly ConcurrentBag<Object> piecesData;

        // saving mode uses flags instead of data variables - to save ram
        private bool headerSaved;

        private bool hintsSaved;
        private bool weatherSaved;
        private bool gridSaved;
        private bool trackingSaved;
        private bool eventsSaved;
        private bool piecesSaved;

        private string nextStepName;
        private int processedSteps;
        private readonly int allSteps;

        private int PiecesFilesCount
        { get { return Directory.GetFiles(this.savePath).Where(path => path.Contains("pieces_")).ToList().Count; } }

        private static List<string> SaveTempPaths
        {
            get
            {
                List<string> tempNames = Directory.GetDirectories(SonOfRobinGame.saveGamesPath).Where(dir => dir.StartsWith(tempPrefix)).ToList();
                List<string> tempPaths = new List<string> { };
                foreach (string name in tempNames)
                {
                    tempPaths.Add(Path.Combine(SonOfRobinGame.saveGamesPath, name));
                }
                return tempPaths;
            }
        }

        private int TimeElapsed
        { get { return (int)(DateTime.Now - this.createdTime).TotalSeconds; } }

        private bool errorOccured;

        private bool ErrorOccured
        {
            get { return errorOccured; }
            set
            {
                errorOccured = value;
                if (errorOccured) this.Remove();
            }
        }

        private Scheduler.TaskName TextWindowTask
        {
            get
            {
                if (this.saveMode) return Scheduler.TaskName.Empty;

                World world = World.GetTopWorld();
                if (world == null || world.demoMode) return Scheduler.TaskName.OpenMainMenu;
                else return Scheduler.TaskName.Empty;
            }
        }

        private Dictionary<string, Object> SaveGameData
        {
            get
            {
                return new Dictionary<string, Object> {
                    { "header", this.headerData },
                    { "hints", this.hintsData },
                    { "weather", this.weatherData },
                    { "grid", this.gridData },
                    { "pieces", this.piecesData },
                    { "tracking", this.trackingData },
                    { "events", this.eventsData },
            };
            }
        }

        public LoaderSaver(bool saveMode, string saveSlotName, World world = null, bool showSavedMessage = false, bool quitGameAfterSaving = false) : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.createdTime = DateTime.Now;
            SonOfRobinGame.Game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
            this.drawActive = false;
            this.saveMode = saveMode;
            this.modeText = this.saveMode ? "Saving" : "Loading";
            this.showSavedMessage = showSavedMessage;
            this.quitGameAfterSaving = quitGameAfterSaving;
            this.processingComplete = false;
            this.world = world;
            if (this.world != null)
            {
                this.world.swayManager.FinishAndRemoveAllEvents(); // all SwayEvents should be cleared, to ensure that original values will be saved
                this.piecePackagesToSave = this.PreparePiecePackages();
            }

            this.processedSteps = 0;
            this.saveSlotName = saveSlotName;
            this.saveTempPath = this.GetSaveTempPath();
            this.savePath = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);
            this.ErrorOccured = false;
            this.nextStepName = "";
            this.directoryChecked = false;
            this.gridTemplateFound = false;
            this.headerSaved = false;
            this.hintsSaved = false;
            this.weatherSaved = false;
            this.gridSaved = false;
            this.trackingSaved = false;
            this.eventsSaved = false;
            this.piecesSaved = false;
            this.currentPiecePackageNo = 0;
            this.allPiecesProcessed = false;
            this.piecesData = new ConcurrentBag<object> { };
            this.allSteps = this.saveMode ? 8 + this.piecePackagesToSave.Count : 7 + this.PiecesFilesCount;

            if (this.saveMode) DeleteAllSaveTemps();
        }

        private List<List<BoardPiece>> PreparePiecePackages()
        {
            var piecePackages = new List<List<BoardPiece>>();

            List<BoardPiece> currentPieceList = new List<BoardPiece>();
            piecePackages.Add(currentPieceList);

            foreach (Sprite sprite in this.world.Grid.GetSpritesFromAllCells(Cell.Group.All))
            {
                if (sprite.boardPiece.exists && sprite.IsOnBoard && sprite.boardPiece.serialize)
                {
                    currentPieceList.Add(sprite.boardPiece);
                    if (currentPieceList.Count >= maxPiecesInPackage)
                    {
                        currentPieceList = new List<BoardPiece>();
                        piecePackages.Add(currentPieceList);
                    }
                }
            }

            return piecePackages;
        }

        private string GetSaveTempPath()
        {
            int tempNo = 0;
            string currentTempPath;

            while (true)
            {
                currentTempPath = Path.Combine(SonOfRobinGame.saveGamesPath, $"{tempPrefix}{tempNo}");
                if (!Directory.Exists(this.savePath)) break;
            }

            return currentTempPath;
        }

        private string GetCurrentPiecesPath(int packageNo)
        {
            return Path.Combine(this.saveMode ? saveTempPath : savePath, $"pieces_{packageNo}.sav");
        }

        public override void Remove()
        {
            if (Preferences.FrameSkip) SonOfRobinGame.Game.IsFixedTimeStep = true;
            if (this.saveMode || this.ErrorOccured) SonOfRobinGame.FullScreenProgressBar.TurnOff();

            base.Remove();
            if (this.saveMode && this.quitGameAfterSaving) SonOfRobinGame.quitGame = true;
            Menu.RebuildAllMenus();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.processingComplete)
            {
                this.Remove();
                return;
            }

            if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                if (this.saveMode) DeleteAllSaveTemps();
                SonOfRobinGame.FullScreenProgressBar.TurnOff();
                this.Remove();

                new TextWindow(text: $"{this.modeText} has been cancelled.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, closingTask: this.TextWindowTask);
                return;
            }

            if (this.saveMode) this.ProcessNextSavingStep();
            else this.ProcessNextLoadingStep();

            if (!this.ErrorOccured && !this.processingComplete) this.UpdateProgressBar();
            if (this.ErrorOccured) this.Remove();
        }

        private void UpdateProgressBar()
        {
            int currentGlobalStep = 0;
            int totalGlobalSteps = this.saveMode || this.gridTemplateFound ? 1 : SonOfRobinGame.enteringIslandGlobalSteps;

            float percentage = FullScreenProgressBar.CalculatePercentage(currentLocalStep: this.processedSteps, totalLocalSteps: this.allSteps, currentGlobalStep: currentGlobalStep, totalGlobalSteps: totalGlobalSteps);

            string text = LoadingTips.GetTip();
            string optionalText = Preferences.progressBarShowDetails ? $"{this.modeText} game - {this.nextStepName}..." : null;

            if (this.saveMode && !Preferences.progressBarShowDetails)
            {
                text = "Saving game...";
                optionalText = null;
            }

            SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: text, optionalText: optionalText);
        }

        private static void DeleteAllSaveTemps()
        {
            foreach (string tempPath in SaveTempPaths)
            { Directory.Delete(tempPath); }
        }

        private void ProcessNextSavingStep()
        {
            this.processedSteps++;

            // preparing save directory
            if (!this.directoryChecked)
            {
                Directory.CreateDirectory(this.saveTempPath);

                this.directoryChecked = true;
                this.nextStepName = "header";
                return;
            }

            // saving header data
            if (!this.headerSaved)
            {
                var headerData = new Dictionary<string, Object>
                {
                    { "seed", this.world.seed },
                    { "width", this.world.width },
                    { "height", this.world.height },
                    { "maxAnimalsPerName", this.world.maxAnimalsPerName },
                    { "playerName", this.world.PlayerName },
                    { "resDivider", this.world.resDivider },
                    { "currentFrame", this.world.CurrentFrame },
                    { "currentUpdate", this.world.CurrentUpdate },
                    { "clockTimeElapsed", this.world.islandClock.ElapsedUpdates },
                    { "TimePlayed", this.world.TimePlayed },
                    { "MapEnabled", this.world.MapEnabled },
                    { "realDateTime", DateTime.Now },
                    { "doNotCreatePiecesList", this.world.doNotCreatePiecesList },
                    { "discoveredRecipesForPieces", this.world.discoveredRecipesForPieces },
                    { "stateMachineTypesManager", this.world.stateMachineTypesManager.Serialize() },
                    { "craftStats", this.world.craftStats.Serialize() },
                    { "identifiedPieces", this.world.identifiedPieces },
                    { "saveVersion", SaveHeaderManager.saveVersion },
            };

                string headerPath = Path.Combine(this.saveTempPath, headerName);
                FileReaderWriter.Save(path: headerPath, savedObj: headerData);

                this.headerSaved = true;
                this.nextStepName = "hints";
                return;
            }

            // saving hints data
            if (!this.hintsSaved)
            {
                string hintsPath = Path.Combine(this.saveTempPath, hintsName);
                var hintsData = this.world.HintEngine.Serialize();
                FileReaderWriter.Save(path: hintsPath, savedObj: hintsData);

                this.hintsSaved = true;
                this.nextStepName = "weather";
                return;
            }

            // saving weather data
            if (!this.weatherSaved)
            {
                string weatherPath = Path.Combine(this.saveTempPath, weatherName);
                var weatherData = this.world.weather.Serialize();
                FileReaderWriter.Save(path: weatherPath, savedObj: weatherData);

                this.weatherSaved = true;
                this.nextStepName = "grid";
                return;
            }

            // saving grid data
            if (!this.gridSaved)
            {
                string gridPath = Path.Combine(this.saveTempPath, gridName);
                var gridData = this.world.Grid.Serialize();
                FileReaderWriter.Save(path: gridPath, savedObj: gridData);

                this.gridSaved = true;
                this.nextStepName = "pieces 1";
                return;
            }

            // saving pieces data
            if (!this.piecesSaved)
            {
                var packagesToProcess = new List<List<BoardPiece>>();

                for (int i = 0; i < Preferences.MaxThreadsToUse; i++)
                {
                    packagesToProcess.Add(this.piecePackagesToSave[0]);
                    this.piecePackagesToSave.RemoveAt(0);

                    if (this.piecePackagesToSave.Count == 0)
                    {
                        this.piecesSaved = true;
                        break;
                    }
                }

                Parallel.For(0, packagesToProcess.Count, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, packageIndex =>
                {
                    var package = packagesToProcess[packageIndex];

                    var pieceDataPackage = new List<object> { };
                    foreach (BoardPiece piece in package)
                    {
                        pieceDataPackage.Add(piece.Serialize());
                    }

                    FileReaderWriter.Save(path: this.GetCurrentPiecesPath(this.currentPiecePackageNo + packageIndex), savedObj: pieceDataPackage);
                });

                this.currentPiecePackageNo += packagesToProcess.Count;
                this.processedSteps += packagesToProcess.Count - 1;

                this.nextStepName = this.piecesSaved ? "tracking" : $"pieces {currentPiecePackageNo + 1}";

                return;
            }

            // saving tracking data
            if (!this.trackingSaved)
            {
                var trackingData = new List<Object> { };
                foreach (Tracking tracking in this.world.trackingQueue.Values)
                { trackingData.Add(tracking.Serialize()); }

                string trackingPath = Path.Combine(this.saveTempPath, trackingName);
                FileReaderWriter.Save(path: trackingPath, savedObj: trackingData);

                this.trackingSaved = true;
                this.nextStepName = "events";
                return;
            }

            // saving world event data
            if (!this.eventsSaved)
            {
                var eventData = new List<Object> { };
                foreach (var eventList in this.world.eventQueue.Values)
                {
                    foreach (var plannedEvent in eventList)
                    { eventData.Add(plannedEvent.Serialize()); }
                }
                string eventPath = Path.Combine(this.saveTempPath, eventsName);
                FileReaderWriter.Save(path: eventPath, savedObj: eventData);

                this.eventsSaved = true;
                this.nextStepName = "replacing save slot data";
                return;
            }

            if (Directory.Exists(this.savePath)) Directory.Delete(path: this.savePath, recursive: true);

            bool movedCorrectly = false;
            for (int i = 0; i < 15; i++)
            {
                try
                {
                    Directory.Move(this.saveTempPath, this.savePath);
                    movedCorrectly = true;
                }
                catch (IOException)
                { Thread.Sleep(200); }

                if (movedCorrectly) break;
            }

            if (!movedCorrectly)
            {
                new TextWindow(text: "An error occured during renaming temp save directory.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask, priority: -1, inputType: InputTypes.Normal);
                this.ErrorOccured = true;
                return;
            }

            if (this.showSavedMessage)
            {
                new TextWindow(text: "Game has been saved.", textColor: Color.White, bgColor: Color.DarkGreen, useTransition: false, animate: false);
                Sound.QuickPlay(name: SoundData.Name.Ding2, volume: 1f);
            }
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Game saved in slot {saveSlotName} (time elapsed {this.TimeElapsed}s).", color: Color.LightBlue);

            this.world.lastSaved = DateTime.Now;
            this.processingComplete = true;
        }

        private void ProcessNextLoadingStep()
        {
            this.processedSteps++;

            // checking directory
            if (!this.directoryChecked)
            {
                if (!Directory.Exists(this.savePath))
                {
                    new TextWindow(text: $"Save slot {saveSlotName} is empty.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                this.directoryChecked = true;
                this.nextStepName = "header";
                return;
            }

            // loading header
            if (this.headerData == null)
            {
                string headerPath = Path.Combine(this.savePath, headerName);
                this.headerData = (Dictionary<string, Object>)FileReaderWriter.Load(path: headerPath);

                if (this.headerData == null)
                {
                    new TextWindow(text: $"Error while reading save header for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                else
                {
                    Grid templateGrid = Grid.GetMatchingTemplateFromSceneStack(seed: (int)(Int64)this.headerData["seed"], width: (int)(Int64)this.headerData["width"], height: (int)(Int64)this.headerData["height"], ignoreCellSize: true);

                    this.gridTemplateFound = templateGrid != null;
                }

                this.nextStepName = "grid";
                return;
            }

            // loading grid
            if (this.gridData == null)
            {
                string gridPath = Path.Combine(this.savePath, gridName);
                this.gridData = (Dictionary<string, Object>)FileReaderWriter.Load(path: gridPath);

                if (this.gridData == null)
                {
                    new TextWindow(text: $"Error while reading grid for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                this.nextStepName = "hints";
                return;
            }

            // loading hints
            if (this.hintsData == null)
            {
                string hintsPath = Path.Combine(this.savePath, hintsName);
                this.hintsData = (Dictionary<string, Object>)FileReaderWriter.Load(path: hintsPath);

                if (hintsData == null)
                {
                    new TextWindow(text: $"Error while reading hints for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                this.nextStepName = "weather";
                return;
            }

            // loading hints
            if (this.weatherData == null)
            {
                string weatherPath = Path.Combine(this.savePath, weatherName);
                this.weatherData = (Dictionary<string, Object>)FileReaderWriter.Load(path: weatherPath);

                if (weatherData == null)
                {
                    new TextWindow(text: $"Error while reading weather for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                this.nextStepName = "tracking";
                return;
            }

            // loading tracking
            if (this.trackingData == null)
            {
                string trackingPath = Path.Combine(this.savePath, trackingName);
                if (!File.Exists(trackingPath))
                {
                    new TextWindow(text: "Error while reading tracking data.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                else this.trackingData = (List<Object>)FileReaderWriter.Load(path: trackingPath);
                this.nextStepName = "events";
                return;
            }

            // loading planned events
            if (this.eventsData == null)
            {
                string eventPath = Path.Combine(this.savePath, eventsName);
                if (!File.Exists(eventPath))
                {
                    new TextWindow(text: "Error while reading events data.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                }
                else this.eventsData = (List<Object>)FileReaderWriter.Load(path: eventPath);

                this.nextStepName = "pieces 1";
                return;
            }

            // loading pieces
            if (!this.allPiecesProcessed)
            {
                if (this.currentPiecePackageNo == 0 && !File.Exists(this.GetCurrentPiecesPath(this.currentPiecePackageNo))) // first check - this save file should exist
                {
                    new TextWindow(text: "Error while reading pieces data.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                    return;
                }

                Parallel.For(0, Preferences.MaxThreadsToUse, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, threadNo =>
                {
                    int packageToLoad = this.currentPiecePackageNo + threadNo;

                    string currentPiecesPath = this.GetCurrentPiecesPath(this.currentPiecePackageNo + threadNo);
                    if (File.Exists(currentPiecesPath))
                    {
                        var packageData = (List<Object>)FileReaderWriter.Load(path: currentPiecesPath);
                        foreach (var item in packageData)
                        {
                            this.piecesData.Add(item);
                        }
                    }
                    else
                    {
                        this.allPiecesProcessed = true; // second check - if file is missing, then there are no more packages to load
                    }
                });

                this.currentPiecePackageNo += Preferences.MaxThreadsToUse;
                this.processedSteps = Math.Min(this.processedSteps + Preferences.MaxThreadsToUse - 1, this.allSteps);

                this.nextStepName = this.allPiecesProcessed ? "creating world" : $"pieces {currentPiecePackageNo + 1}";
                return;
            }

            // creating new world (using header data)
            int seed = (int)this.headerData["seed"];
            int width = (int)this.headerData["width"];
            int height = (int)this.headerData["height"];
            int resDivider = (int)this.headerData["resDivider"];
            PieceTemplate.Name playerName = (PieceTemplate.Name)this.headerData["playerName"];

            this.world = new World(width: width, height: height, seed: seed, saveGameData: this.SaveGameData, playerName: playerName, resDivider: resDivider);
            this.MoveToTop();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Game has been loaded from slot {saveSlotName} (time elapsed {this.TimeElapsed}s).", color: Color.LightBlue);

            // deleting other non-demo worlds
            var existingWorlds = GetAllScenesOfType(typeof(World));
            foreach (World currWorld in existingWorlds)
            {
                if (currWorld != this.world && !currWorld.demoMode) currWorld.Remove();
            }

            this.processingComplete = true;
        }
    }
}