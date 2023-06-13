﻿using Microsoft.Xna.Framework;
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
        public const string headerName = "header.json";
        public const string hintsName = "hints.json";
        public const string trackingName = "tracking.json";
        public const string eventsName = "events.json";
        public const string coolingName = "cooling.json";
        public const string weatherName = "weather.json";
        public const string gridName = "grid.json";
        public const string piecesPrefix = "pieces_";
        public const string tempPrefix = "_save_temp_";

        private const int maxPiecesInPackage = 5000;

        private readonly DateTime createdTime;
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

        private bool gridTemplateFound;

        // loading mode uses data variables
        private Dictionary<string, Object> headerData;

        private Dictionary<string, Object> hintsData;
        private Dictionary<string, Object> weatherData;
        private Dictionary<string, Object> gridData;
        private List<Object> trackingData;
        private List<Object> eventsData;
        private Dictionary<string, Object> coolingData;
        private readonly ConcurrentBag<Object> piecesData;
        private string currentStepName;
        private int processedSteps;
        private readonly int allSteps;
        private Task backgroundTask;

        private int PiecesFilesCount
        { get { return Directory.GetFiles(this.savePath).Where(path => path.Contains(piecesPrefix)).Count(); } }

        private static List<string> SaveTempPaths
        {
            get
            {
                var tempNames = Directory.GetDirectories(SonOfRobinGame.saveGamesPath).Where(dir => dir.StartsWith(tempPrefix));
                List<string> tempPaths = new() { };
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
                    { "cooling", this.coolingData },
            };
            }
        }

        public LoaderSaver(bool saveMode, string saveSlotName, World world = null, bool showSavedMessage = false) : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.createdTime = DateTime.Now;
            this.drawActive = false;
            this.saveMode = saveMode;
            this.modeText = this.saveMode ? "Saving" : "Loading";
            this.showSavedMessage = showSavedMessage;
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
            this.currentStepName = "";
            this.gridTemplateFound = false;
            this.currentPiecePackageNo = 0;
            this.piecesData = new ConcurrentBag<object> { };
            this.allSteps = this.saveMode ? 8 + this.piecePackagesToSave.Count : 7 + this.PiecesFilesCount;

            if (this.saveMode) DeleteAllSaveTemps();
        }

        private List<List<BoardPiece>> PreparePiecePackages()
        {
            var piecePackages = new List<List<BoardPiece>>();

            List<BoardPiece> currentPieceList = new();
            piecePackages.Add(currentPieceList);

            foreach (Sprite sprite in this.world.Grid.GetSpritesFromAllCells(Cell.Group.All))
            {
                if (sprite.boardPiece.exists && sprite.IsOnBoard && sprite.boardPiece.serialize)
                {
                    // there is no point in serializing dead small plants
                    if (!sprite.boardPiece.alive && !sprite.blocksMovement && sprite.boardPiece.GetType() == typeof(Plant)) continue;

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
            return Path.Combine(this.saveMode ? saveTempPath : savePath, $"{piecesPrefix}{packageNo}.json");
        }

        public override void Remove()
        {
            if (this.saveMode || this.ErrorOccured) SonOfRobinGame.FullScreenProgressBar.TurnOff();

            base.Remove();
            Menu.RebuildAllMenus();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.backgroundTask != null && this.backgroundTask.IsFaulted)
            {
                SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: true);

                this.ErrorOccured = true;
            }

            if (!this.ErrorOccured) this.UpdateProgressBar();

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

            if (this.backgroundTask == null)
            {
                this.backgroundTask = this.saveMode ? Task.Run(() => this.ProcessSaving()) : Task.Run(() => this.ProcessLoading());
            }
            else
            {
                if (this.backgroundTask.IsCompleted)
                {
                    if (!this.saveMode && !this.HasBeenRemoved) this.FinishLoading();
                    this.processingComplete = true;
                }
            }

            if (this.ErrorOccured)
            {
                this.Remove();
                bool menuFound = GetTopSceneOfType(typeof(Menu)) != null;
                if (!menuFound) MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
            }
        }

        private void UpdateProgressBar()
        {
            if (this.HasBeenRemoved) return;

            int currentGlobalStep = 0;
            int totalGlobalSteps = this.saveMode || this.gridTemplateFound ? 1 : SonOfRobinGame.enteringIslandGlobalSteps;

            float percentage = FullScreenProgressBar.CalculatePercentage(currentLocalStep: Math.Min(this.processedSteps, this.allSteps), totalLocalSteps: this.allSteps, currentGlobalStep: currentGlobalStep, totalGlobalSteps: totalGlobalSteps);

            string text = LoadingTips.GetTip();
            string optionalText = Preferences.progressBarShowDetails ? $"{this.modeText.ToLower()} game - {this.currentStepName}..." : null;

            if (this.saveMode && !Preferences.progressBarShowDetails)
            {
                text = "saving game...";
                optionalText = null;
            }

            SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: text, optionalText: optionalText);
        }

        private static void DeleteAllSaveTemps()
        {
            foreach (string tempPath in SaveTempPaths)
            { Directory.Delete(tempPath); }
        }

        private void ProcessSaving()
        {
            // preparing save directory
            {
                this.processedSteps++;
                this.currentStepName = "directory";
                Directory.CreateDirectory(this.saveTempPath);
            }

            // saving header data
            {
                this.processedSteps++;
                this.currentStepName = "header";

                var headerData = new Dictionary<string, Object>
                {
                    { "seed", this.world.seed },
                    { "width", this.world.width },
                    { "height", this.world.height },
                    { "maxAnimalsPerName", this.world.maxAnimalsPerName },
                    { "playerName", this.world.initialPlayerName },
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
                    { "cookStats", this.world.cookStats.Serialize() },
                    { "brewStats", this.world.brewStats.Serialize() },
                    { "identifiedPieces", this.world.identifiedPieces },
                    { "saveVersion", SaveHeaderManager.saveVersion },
            };

                string headerPath = Path.Combine(this.saveTempPath, headerName);
                FileReaderWriter.Save(path: headerPath, savedObj: headerData, compress: false);
            }

            // saving hints data
            {
                this.processedSteps++;
                this.currentStepName = "hints";

                string hintsPath = Path.Combine(this.saveTempPath, hintsName);
                var hintsData = this.world.HintEngine.Serialize();
                FileReaderWriter.Save(path: hintsPath, savedObj: hintsData, compress: true);
            }

            // saving weather data
            {
                this.processedSteps++;
                this.currentStepName = "weather";

                string weatherPath = Path.Combine(this.saveTempPath, weatherName);
                var weatherData = this.world.weather.Serialize();
                FileReaderWriter.Save(path: weatherPath, savedObj: weatherData, compress: true);
            }

            // saving grid data
            {
                this.processedSteps++;
                this.currentStepName = "grid";

                string gridPath = Path.Combine(this.saveTempPath, gridName);
                var gridData = this.world.Grid.Serialize();
                FileReaderWriter.Save(path: gridPath, savedObj: gridData, compress: true);
            }

            // saving pieces data
            {
                bool piecesSaved = false;

                while (true)
                {
                    this.processedSteps++;
                    this.currentStepName = $"pieces {currentPiecePackageNo + 1}";

                    var packagesToProcess = new List<List<BoardPiece>>();

                    for (int i = 0; i < Preferences.MaxThreadsToUse; i++)
                    {
                        packagesToProcess.Add(this.piecePackagesToSave[0]);
                        this.piecePackagesToSave.RemoveAt(0);

                        if (this.piecePackagesToSave.Count == 0)
                        {
                            piecesSaved = true;
                            break;
                        }
                    }

                    Parallel.For(0, packagesToProcess.Count, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, packageIndex =>
                    {
                        var package = packagesToProcess[packageIndex];

                        var pieceDataPackage = new List<object> { };
                        foreach (BoardPiece piece in package)
                        {
                            if (piece.sprite.opacityFade != null) piece.sprite.opacityFade.Finish(); // Finish() might destroy the piece...
                            if (piece.exists) pieceDataPackage.Add(piece.Serialize()); // ...so "exists" must be checked afterwards
                        }

                        FileReaderWriter.Save(path: this.GetCurrentPiecesPath(this.currentPiecePackageNo + packageIndex), savedObj: pieceDataPackage, compress: true);
                    });

                    this.currentPiecePackageNo += packagesToProcess.Count;
                    this.processedSteps += packagesToProcess.Count - 1;

                    if (piecesSaved) break;
                }
            }

            // saving tracking data
            {
                this.processedSteps++;
                this.currentStepName = "tracking";

                var trackingData = this.world.trackingManager.Serialize();

                string trackingPath = Path.Combine(this.saveTempPath, trackingName);
                FileReaderWriter.Save(path: trackingPath, savedObj: trackingData, compress: true);
            }

            // saving world event data
            {
                this.processedSteps++;
                this.currentStepName = "events";

                var eventData = this.world.worldEventManager.Serialize();

                string eventPath = Path.Combine(this.saveTempPath, eventsName);
                FileReaderWriter.Save(path: eventPath, savedObj: eventData, compress: true);
            }

            // saving cooling data
            {
                this.processedSteps++;
                this.currentStepName = "cooling";

                var coolingData = this.world.coolingManager.Serialize();

                string coolingPath = Path.Combine(this.saveTempPath, coolingName);
                FileReaderWriter.Save(path: coolingPath, savedObj: coolingData, compress: true);
            }

            this.processedSteps++;
            this.currentStepName = "replacing save slot data";
            if (Directory.Exists(this.savePath))
            {
                try
                { Directory.Delete(path: this.savePath, recursive: true); }
                catch (IOException)
                { throw new ArgumentException("An error occured while deleting previous save directory."); }
            }

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
            { throw new ArgumentException("An error occured during renaming temp save directory."); }

            if (this.showSavedMessage)
            {
                new TextWindow(text: "Game has been saved.", textColor: Color.White, bgColor: Color.DarkGreen, useTransition: false, animate: false);
                Sound.QuickPlay(name: SoundData.Name.Ding2, volume: 1f);
            }
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Game saved in slot {saveSlotName} (time elapsed {this.TimeElapsed}s).", color: Color.LightBlue);

            this.world.lastSaved = DateTime.Now;
            this.processingComplete = true;
        }

        private void ProcessLoading()
        {
            // checking directory
            {
                this.processedSteps++;
                this.currentStepName = "directory";

                if (!Directory.Exists(this.savePath)) throw new ArgumentException($"Directory for save slot {saveSlotName} does not exist.");
            }

            // loading header
            {
                this.processedSteps++;
                this.currentStepName = "header";

                string headerPath = Path.Combine(this.savePath, headerName);
                this.headerData = (Dictionary<string, Object>)FileReaderWriter.Load(path: headerPath);

                if (this.headerData == null) throw new ArgumentException($"Error while reading save header for slot {saveSlotName}.");
                {
                    Grid templateGrid = Grid.GetMatchingTemplateFromSceneStack(seed: (int)(Int64)this.headerData["seed"], width: (int)(Int64)this.headerData["width"], height: (int)(Int64)this.headerData["height"], ignoreCellSize: true);

                    this.gridTemplateFound = templateGrid != null;
                }
            }

            // loading grid
            {
                this.processedSteps++;
                this.currentStepName = "grid";

                string gridPath = Path.Combine(this.savePath, gridName);
                this.gridData = (Dictionary<string, Object>)FileReaderWriter.Load(path: gridPath);

                if (this.gridData == null) throw new ArgumentException($"Error while reading grid for slot {saveSlotName}.");
            }

            // loading hints
            {
                this.processedSteps++;
                this.currentStepName = "hints";

                string hintsPath = Path.Combine(this.savePath, hintsName);
                this.hintsData = (Dictionary<string, Object>)FileReaderWriter.Load(path: hintsPath);

                if (hintsData == null) throw new ArgumentException($"Error while reading hints for slot {saveSlotName}.");
            }

            // loading hints
            {
                this.processedSteps++;
                this.currentStepName = "weather";

                string weatherPath = Path.Combine(this.savePath, weatherName);
                this.weatherData = (Dictionary<string, Object>)FileReaderWriter.Load(path: weatherPath);

                if (weatherData == null) throw new ArgumentException($"Error while reading weather for slot {saveSlotName}.");
            }

            // loading tracking
            {
                this.processedSteps++;
                this.currentStepName = "tracking";

                string trackingPath = Path.Combine(this.savePath, trackingName);

                if (!FileReaderWriter.PathExists(trackingPath)) throw new ArgumentException($"Error while reading tracking data for slot {saveSlotName}.");

                this.trackingData = (List<Object>)FileReaderWriter.Load(path: trackingPath);
            }

            // loading planned events
            {
                this.processedSteps++;
                this.currentStepName = "events";

                string eventPath = Path.Combine(this.savePath, eventsName);
                if (!FileReaderWriter.PathExists(eventPath)) throw new ArgumentException($"Error while reading events for slot {saveSlotName}.");

                this.eventsData = (List<Object>)FileReaderWriter.Load(path: eventPath);
            }

            // loading cooling data
            {
                this.processedSteps++;
                this.currentStepName = "cooling";

                string coolingPath = Path.Combine(this.savePath, coolingName);
                if (!FileReaderWriter.PathExists(coolingPath)) throw new ArgumentException($"Error while reading cooling data for slot {saveSlotName}.");

                this.coolingData = (Dictionary<string, Object>)FileReaderWriter.Load(path: coolingPath);
            }

            // loading pieces
            {
                bool allPiecesProcessed = false;

                while (true)
                {
                    this.processedSteps++;
                    this.currentStepName = $"pieces {currentPiecePackageNo + 1}";

                    // first check - this save file should exist
                    if (this.currentPiecePackageNo == 0 && !FileReaderWriter.PathExists(this.GetCurrentPiecesPath(this.currentPiecePackageNo)))
                    {
                        throw new ArgumentException($"Error while reading pieces data for slot {saveSlotName}.");
                    }

                    Parallel.For(0, Preferences.MaxThreadsToUse, new ParallelOptions { MaxDegreeOfParallelism = Preferences.MaxThreadsToUse }, threadNo =>
                    {
                        int packageToLoad = this.currentPiecePackageNo + threadNo;

                        string currentPiecesPath = this.GetCurrentPiecesPath(this.currentPiecePackageNo + threadNo);
                        if (FileReaderWriter.PathExists(currentPiecesPath))
                        {
                            var packageData = (List<Object>)FileReaderWriter.Load(path: currentPiecesPath);
                            foreach (var item in packageData)
                            {
                                this.piecesData.Add(item);
                            }
                        }
                        else
                        {
                            allPiecesProcessed = true; // second check - if file is missing, then there are no more packages to load
                        }
                    });

                    this.currentPiecePackageNo += Preferences.MaxThreadsToUse;
                    this.processedSteps = Math.Min(this.processedSteps + Preferences.MaxThreadsToUse - 1, this.allSteps);

                    if (allPiecesProcessed) break;
                }
            }

            this.processedSteps++;
            this.currentStepName = "creating world";
        }

        private void FinishLoading() // steps that cannot be run in another thread
        {
            // creating new world (using header data)
            int seed = (int)(Int64)this.headerData["seed"];
            int width = (int)(Int64)this.headerData["width"];
            int height = (int)(Int64)this.headerData["height"];
            int resDivider = (int)(Int64)this.headerData["resDivider"];
            PieceTemplate.Name playerName = (PieceTemplate.Name)(Int64)this.headerData["playerName"];

            this.world = new World(width: width, height: height, seed: seed, saveGameData: this.SaveGameData, playerName: playerName, resDivider: resDivider);
            this.MoveToTop();

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Game has been loaded from slot {saveSlotName} (time elapsed {this.TimeElapsed}s).", color: Color.LightBlue);

            // deleting other non-demo worlds
            var existingWorlds = GetAllScenesOfType(typeof(World));
            foreach (World currWorld in existingWorlds)
            {
                if (currWorld != this.world && !currWorld.demoMode) currWorld.Remove();
            }
        }
    }
}