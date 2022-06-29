using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class LoaderSaver : Scene
    {
        private readonly static string headerName = "header.sav";
        private readonly static string hintsName = "hints.sav";
        private readonly static string trackingName = "tracking.sav";
        private readonly static string eventsName = "events.sav";
        private readonly static string gridName = "grid.sav";
        public readonly static string tempPrefix = "_save_temp_";

        private readonly static int maxPiecesInPackage = 5000; // using small piece packages lowers ram usage during writing binary files
        private string CurrentPiecesName { get { return $"pieces_{this.currentPiecePackageNo}.sav"; } }
        private string CurrentPiecesPath { get { return Path.Combine(this.saveMode ? saveTempPath : savePath, this.CurrentPiecesName); } }

        private readonly bool quitGameAfterSaving;
        private readonly bool saveMode;
        private readonly string modeText;
        private readonly bool showSavedMessage;
        private bool processingComplete;
        private World world;
        private List<Sprite> spritesToSave;
        private readonly string saveSlotName;
        private readonly string savePath;
        private readonly string saveTempPath;
        private int currentPieceIndex;
        private int currentPiecePackageNo;
        private bool allPiecesProcessed;

        private bool directoryChecked;

        // loading mode uses data variables
        private Dictionary<string, Object> headerData;
        private Dictionary<string, Object> hintsData;
        private Dictionary<string, Object> gridData;
        private List<Object> trackingData;
        private List<Object> eventsData;
        private List<Object> piecesData;

        // saving mode uses flags instead of data variables - to save ram
        private bool headerSaved;
        private bool hintsSaved;
        private bool gridSaved;
        private bool trackingSaved;
        private bool eventsSaved;
        private bool piecesSaved;

        private string nextStepName;
        private int processedSteps;
        private int allSteps;

        private int PiecesFilesCount { get { return Directory.GetFiles(this.savePath).Where(file => file.Contains("pieces_")).ToList().Count; } }

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

        private bool CancelPressed
        {
            get
            {
                return
                    Keyboard.IsPressed(Keys.Escape) ||
                    GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B) ||
                    VirtButton.IsButtonDown(VButName.Return);
            }
        }

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
                    {"header", this.headerData},
                    {"hints", this.hintsData},
                    {"grid", this.gridData},
                    {"pieces", this.piecesData},
                    {"tracking", this.trackingData},
                    {"events", this.eventsData},
            };
            }
        }

        public LoaderSaver(bool saveMode, string saveSlotName, World world = null, bool showSavedMessage = false, bool quitGameAfterSaving = false) : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            SonOfRobinGame.game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
            this.drawActive = false;
            this.saveMode = saveMode;
            this.modeText = this.saveMode ? "Saving" : "Loading";
            this.showSavedMessage = showSavedMessage;
            this.quitGameAfterSaving = quitGameAfterSaving;
            this.processingComplete = false;
            this.world = world;
            if (this.world != null) this.spritesToSave = this.world.grid.GetAllSprites(Cell.Group.All).ToList();
            this.processedSteps = 0;
            this.saveSlotName = saveSlotName;
            this.saveTempPath = this.GetSaveTempPath();
            this.savePath = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);
            this.ErrorOccured = false;
            this.nextStepName = "";
            this.directoryChecked = false;
            this.headerSaved = false;
            this.hintsSaved = false;
            this.gridSaved = false;
            this.trackingSaved = false;
            this.eventsSaved = false;
            this.piecesSaved = false;
            this.currentPiecePackageNo = 0;
            this.currentPieceIndex = 0;
            this.allPiecesProcessed = false;
            this.piecesData = new List<Object> { };
            this.allSteps = this.saveMode ? 7 + (this.spritesToSave.Count / maxPiecesInPackage) : 6 + this.PiecesFilesCount;

            if (this.saveMode) DeleteAllSaveTemps();
            if (SonOfRobinGame.platform == Platform.Mobile) SonOfRobinGame.KeepScreenOn = true;
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

        public override void Remove()
        {
            if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
            if (SonOfRobinGame.platform == Platform.Mobile) SonOfRobinGame.KeepScreenOn = false;
            SonOfRobinGame.progressBar.TurnOff(addTransition: false);
            base.Remove();
            if (this.saveMode && this.quitGameAfterSaving) SonOfRobinGame.quitGame = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.processingComplete)
            {
                this.Remove();
                return;
            }

            if (this.CancelPressed)
            {
                if (this.saveMode) DeleteAllSaveTemps();
                this.Remove();

                new TextWindow(text: $"{this.modeText} has been cancelled.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, closingTask: this.TextWindowTask);
                return;
            }

            if (this.saveMode) this.ProcessNextSavingStep();
            else this.ProcessNextLoadingStep();

            if (!this.ErrorOccured) this.UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            SonOfRobinGame.progressBar.TurnOn(curVal: this.processedSteps, maxVal: this.allSteps, text: $"{this.modeText} game - {this.nextStepName}...");

            // for proper ControlTips alignment
            this.viewParams.width = SonOfRobinGame.progressBar.viewParams.width;
            this.viewParams.height = SonOfRobinGame.progressBar.viewParams.height;
            this.viewParams.posX = SonOfRobinGame.progressBar.viewParams.posX;
            this.viewParams.posY = SonOfRobinGame.progressBar.viewParams.posY;

            // new TextWindow(text: $"{this.modeText} game - {this.nextStepName}...", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: false, blocksUpdatesBelow: true); // testing
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
                    {"seed", this.world.seed },
                    {"width", this.world.width },
                    {"height", this.world.height },
                    {"currentFrame", this.world.currentFrame },
                    {"currentUpdate", this.world.currentUpdate },
                    {"currentPieceId", this.world.currentPieceId },
                    {"currentBuffId", this.world.currentBuffId },
                    {"mapEnabled", this.world.mapEnabled },
                    {"realDateTime", DateTime.Now },
                    {"saveVersion", SaveHeaderManager.saveVersion },
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
                var hintsData = this.world.hintEngine.Serialize();
                FileReaderWriter.Save(path: hintsPath, savedObj: hintsData);

                this.hintsSaved = true;
                this.nextStepName = "grid";
                return;
            }

            // saving grid data
            if (!this.gridSaved)
            {
                string gridPath = Path.Combine(this.saveTempPath, gridName);
                var gridData = this.world.grid.Serialize();
                FileReaderWriter.Save(path: gridPath, savedObj: gridData);

                this.gridSaved = true;
                this.nextStepName = "pieces batch 1";
                return;
            }

            // saving pieces data
            if (!this.piecesSaved)
            {
                var pieceDataPackage = new List<object> { };
                BoardPiece piece;

                while (true)
                {
                    try
                    {
                        piece = this.spritesToSave[this.currentPieceIndex].boardPiece;
                        if (piece.exists && piece.serialize) pieceDataPackage.Add(piece.Serialize());

                        this.currentPieceIndex++;
                        if (pieceDataPackage.Count >= maxPiecesInPackage) break;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        this.piecesSaved = true;
                        break;
                    }
                }

                FileReaderWriter.Save(path: this.CurrentPiecesPath, savedObj: pieceDataPackage);
                this.currentPiecePackageNo++;
                this.nextStepName = $"pieces batch {currentPiecePackageNo + 1}";

                if (this.piecesSaved) this.nextStepName = "tracking";
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
            Directory.Move(this.saveTempPath, this.savePath);

            if (this.showSavedMessage) new TextWindow(text: "Game has been saved.", textColor: Color.White, bgColor: Color.DarkGreen, useTransition: false, animate: false);
            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Game saved in slot {saveSlotName}.", color: Color.LightBlue);

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

                if (headerData is null)
                {
                    new TextWindow(text: $"Error while reading save header for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
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

                if (hintsData is null)
                {
                    new TextWindow(text: $"Error while reading hints for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
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

                this.nextStepName = "pieces batch 1";
                return;
            }

            // loading pieces
            if (!this.allPiecesProcessed)
            {
                if (!File.Exists(this.CurrentPiecesPath)) // first check - this save file should exist
                {
                    new TextWindow(text: "Error while reading pieces data.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, closingTask: this.TextWindowTask);
                    this.ErrorOccured = true;
                    return;
                }

                var packageData = (List<Object>)FileReaderWriter.Load(path: this.CurrentPiecesPath);
                this.piecesData.AddRange(packageData);
                this.currentPiecePackageNo++;
                this.nextStepName = $"pieces batch {currentPiecePackageNo + 1}";

                if (File.Exists(this.CurrentPiecesPath)) return; // second check - if file is missing, then loading is complete

                this.allPiecesProcessed = true;
                this.nextStepName = "creating world";
                return;
            }

            // creating new world (using header data)
            int seed = (int)this.headerData["seed"];
            int width = (int)this.headerData["width"];
            int height = (int)this.headerData["height"];

            this.world = new World(width: width, height: height, seed: seed, saveGameData: this.SaveGameData);
            this.MoveToTop();

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Game has been loaded from slot {saveSlotName}.", color: Color.LightBlue);

            // deleting other non-demo worlds
            var existingWorlds = GetAllScenesOfType(typeof(World));
            foreach (World currWorld in existingWorlds)
            { if (currWorld != this.world && !currWorld.demoMode) currWorld.Remove(); }

            this.processingComplete = true;
        }

    }
}
