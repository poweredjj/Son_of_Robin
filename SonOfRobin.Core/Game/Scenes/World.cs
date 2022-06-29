using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class World : Scene
    {
        public enum MapMode
        {
            None,
            Small,
            Big
        }

        public Vector2 analogMovementLeftStick;
        public Vector2 analogMovementRightStick;
        public Vector2 analogCameraCorrection;

        private float manualScale;
        public bool worldCreationInProgress;
        private bool plantsProcessing;
        private readonly static int initialPiecesCreationFramesTotal = 15;
        private int initialPiecesCreationFramesLeft;
        public readonly DateTime creationStart;
        public DateTime creationEnd;
        public TimeSpan creationDuration;
        public readonly bool demoMode;

        public int currentPieceId;
        public int currentBuffId;

        private Object saveGameData;
        public Dictionary<string, BoardPiece> piecesByOldId; // lookup table for deserializing
        public bool freePiecesPlacingMode; // allows to precisely place pieces during loading a saved game
        public bool createMissinPiecesOutsideCamera;
        public DateTime lastSaved;
        private bool sprintMode;

        public bool SprintMode
        {
            get { return this.sprintMode; }
            set
            {
                if (this.sprintMode == value) return;

                this.sprintMode = value;
                if (this.sprintMode) this.player.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Cyan, textureSize: this.player.sprite.frame.textureSize, priority: 0, framesLeft: -1));
                else this.player.sprite.effectCol.RemoveEffectsOfType(effect: SonOfRobinGame.effectBorder);
            }
        }

        private bool buildMode;

        public bool BuildMode { get { return this.buildMode; } }

        private bool spectatorMode;
        public bool SpectatorMode
        {
            get { return this.spectatorMode; }
            set
            {
                if (this.spectatorMode == value) return;
                this.spectatorMode = value;

                this.solidColorManager.RemoveAll(fadeOutDuration: 60);

                if (this.spectatorMode)
                {
                    this.player.RemoveFromStateMachines();

                    BoardPiece spectator = PieceTemplate.CreateOnBoard(world: this, position: this.camera.TrackedPos, templateName: PieceTemplate.Name.PlayerGhost);
                    spectator.sprite.MoveToClosestFreeSpot(this.camera.TrackedPos);
                    spectator.sprite.orientation = this.player != null ? this.player.sprite.orientation : Sprite.Orientation.right;

                    this.player = (Player)spectator;

                    this.camera.TrackPiece(this.player);
                    this.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.1f);

                    this.tipsLayout = ControlTips.TipsLayout.WorldSpectator;
                    this.touchLayout = TouchLayout.WorldSpectator;

                    var textList = new List<string> {
                        "What is this? Am I a ghost?\nIt seems that life on the island goes on...\nI might as well go sightseeing.",
                        "Is this the end? I don't see any white light...",
                        "I have died and STILL am forced to stay on this island?\nNooooooooooooooo...",
                        "Now that I'm dead, I finally don't have to worry about eating and sleeping.",
                        "Is this the afterlife...?\nUm... No, it's still this deserted island.",
                        "My body has died, but my spirit is eternal...\nAt least I hope so.",
                        "Time to meet my maker.\nBut I don't see him anywhere...",
                        "What happened? Why I'm floating in the air?\nOh, I see. I'm dead.",
                        "This is not the endgame I was hoping for.\nAt least the weather is fine.",
                        "I was trying to escape this island, not this life.\nWell, shit..."
                    };
                    var text = textList[this.random.Next(0, textList.Count)];

                    new TextWindow(text: text, textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 60 * 6, priority: 1);
                }
                else
                {
                    BoardPiece spectator = this.player;

                    bool playerFound = false;
                    foreach (Sprite sprite in this.grid.GetAllSprites(Cell.Group.ColAll))
                    {
                        if (sprite.boardPiece.name == PieceTemplate.Name.Player && sprite.boardPiece.alive)
                        {
                            this.player = (Player)sprite.boardPiece;
                            this.player.AddToStateMachines();

                            playerFound = true;
                            break;
                        }
                    }

                    if (!playerFound)
                    {
                        this.player = (Player)this.PlacePlayer();
                        this.player.sprite.MoveToClosestFreeSpot(this.camera.TrackedPos);
                        this.camera.TrackPiece(this.player);
                        if (spectator != null) this.player.sprite.orientation = spectator.sprite.orientation;
                    }

                    this.camera.TrackPiece(this.player);

                    spectator.Destroy();

                    this.tipsLayout = ControlTips.TipsLayout.WorldMain;
                    this.touchLayout = TouchLayout.WorldMain;

                    new TextWindow(text: "I live... Again!", textColor: Color.White, bgColor: Color.Black, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 60 * 3, priority: 1);
                }
            }
        }

        private bool cineMode;
        public bool CineMode
        {
            get { return cineMode; }
            set
            {
                cineMode = value;

                var taskChain = new List<Object>();

                if (cineMode)
                {
                    this.touchLayout = TouchLayout.Empty; // CineSkip should not be used here, because World class cannot execute the skip properly
                    this.tipsLayout = ControlTips.TipsLayout.Empty;

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ChangeSceneInputType, delay: 0, executeHelper: new Dictionary<string, Object> { { "scene", this }, { "inputType", InputTypes.None } }, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null, storeForLaterUse: true));
                }
                else
                {
                    this.touchLayout = TouchLayout.WorldMain;
                    this.tipsLayout = ControlTips.TipsLayout.WorldMain;

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.SolidColorRemoveAll, delay: 0, executeHelper: new Dictionary<string, Object> { { "manager", this.solidColorManager }, { "delay", 10 } }, storeForLaterUse: true));

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 1f }, { "zoomSpeedMultiplier", 1f } }, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraTrackPiece, delay: 0, executeHelper: this.player, storeForLaterUse: true));

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.ChangeSceneInputType, delay: 0, executeHelper: new Dictionary<string, Object> { { "scene", this }, { "inputType", Scene.InputTypes.Normal } }, storeForLaterUse: true));
                }

                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
            }
        }

        public readonly int seed;
        public readonly int resDivider;
        public int maxAnimalsPerName;
        public FastNoiseLite noise;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Camera camera;
        private RenderTarget2D darknessMask;
        public MapMode mapMode;
        public List<MapMode> mapCycle;
        public readonly Map mapBig;
        public readonly Map mapSmall;
        public readonly PlayerPanel playerPanel;
        public readonly SolidColorManager solidColorManager;
        private bool mapEnabled;
        public bool MapEnabled
        {
            get { return this.mapEnabled; }
            set
            {
                if (this.mapEnabled == value) return;
                this.mapEnabled = value;

                if (this.mapEnabled)
                {
                    if (SonOfRobinGame.platform == Platform.Desktop) this.ToggleMapMode();
                }
                else
                {
                    this.mapSmall.TurnOff();
                    this.mapBig.TurnOff();
                    this.mapMode = MapMode.None;
                }
            }
        }

        public Player player;
        public HintEngine hintEngine;
        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;
        public Dictionary<string, Tracking> trackingQueue;
        public List<Cell> plantCellsQueue;
        public List<Sprite> plantSpritesQueue;
        public List<Sprite> animalSpritesQueue;
        public Dictionary<int, List<WorldEvent>> eventQueue;
        public Grid grid;
        public int currentFrame;
        public int currentUpdate; // can be used to measure time elapsed on island
        public int updateMultiplier;
        public int bulletTimeMultiplier; // 1: normal tempo, more: world is slowed down
        public readonly IslandClock islandClock;
        public string debugText;
        public int processedAnimalsCount;
        public int processedPlantsCount;
        public List<PieceTemplate.Name> doNotCreatePiecesList;
        public List<PieceTemplate.Name> discoveredRecipesForPieces;
        public readonly DateTime createdTime; // for calculating time spent in game
        private TimeSpan timePlayed; // real time spent while playing (differs from currentUpdate because of island time compression via updateMultiplier)
        public TimeSpan TimePlayed
        {
            get
            { return timePlayed + (DateTime.Now - this.createdTime); }
            set { timePlayed = value; }
        }
        public bool CanProcessMoreCameraRectPiecesNow { get { return UpdateTimeElapsed.Milliseconds <= 13 * this.updateMultiplier; } }
        public bool CanProcessMoreAnimalsNow { get { return UpdateTimeElapsed.Milliseconds <= 7 * this.updateMultiplier; } }
        public bool CanProcessMorePlantsNow { get { return UpdateTimeElapsed.Milliseconds <= 9 * this.updateMultiplier; } }
        public bool CanProcessAnyStateMachineNow { get { return !this.plantsProcessing || UpdateTimeElapsed.Milliseconds <= 9 * this.updateMultiplier; } }

        private MapMode NextMapMode
        {
            get
            {
                bool modeFound = false;
                foreach (MapMode mapMode in this.mapCycle)
                {
                    if (modeFound) return mapMode;
                    if (mapMode == this.mapMode) modeFound = true;
                }
                return this.mapCycle[0];
            }
        }

        public float PieceCount
        {
            get
            {
                int pieceCount = 0;
                foreach (PieceTemplate.Name templateName in (PieceTemplate.Name[])Enum.GetValues(typeof(PieceTemplate.Name)))
                { pieceCount += this.pieceCountByName[templateName]; }
                return pieceCount;
            }
        }
        private Vector2 DarknessMaskScale
        {
            get
            {
                Rectangle extendedViewRect = this.camera.ExtendedViewRect;

                int maxDarknessWidth = Math.Min((int)(SonOfRobinGame.graphics.PreferredBackBufferWidth * 1.2f), 2000) / Preferences.DarknessResolution;
                int maxDarknessHeight = Math.Min((int)(SonOfRobinGame.graphics.PreferredBackBufferHeight * 1.2f), 1500) / Preferences.DarknessResolution;

                return new Vector2((float)extendedViewRect.Width / (float)maxDarknessWidth, (float)extendedViewRect.Height / (float)maxDarknessHeight);
            }
        }

        public World(int width, int height, int seed, int resDivider, Object saveGameData = null, bool demoMode = false) :
              base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.demoMode = demoMode;
            this.cineMode = false;
            this.buildMode = false;
            this.spectatorMode = false;
            this.sprintMode = false;
            if (this.demoMode) this.InputType = InputTypes.None;
            this.saveGameData = saveGameData;
            this.freePiecesPlacingMode = saveGameData != null;
            this.createMissinPiecesOutsideCamera = false;
            this.lastSaved = DateTime.Now;
            this.worldCreationInProgress = true;
            this.plantsProcessing = false;
            this.initialPiecesCreationFramesLeft = initialPiecesCreationFramesTotal;
            this.creationStart = DateTime.Now;

            if (seed < 0) throw new ArgumentException($"Seed value cannot be negative - {seed}.");

            this.resDivider = resDivider;
            this.seed = seed;
            this.random = new Random(seed);
            this.currentFrame = 0;
            this.currentUpdate = 0;
            this.createdTime = DateTime.Now;
            this.TimePlayed = TimeSpan.Zero;
            this.updateMultiplier = 1;
            this.bulletTimeMultiplier = 1;
            this.islandClock = new IslandClock(world: this);

            this.currentPieceId = 0;
            this.currentBuffId = 0;

            this.noise = new FastNoiseLite(this.seed);

            this.width = width;
            this.height = height;
            this.viewParams.Width = width; // it does not need to be updated, because world size is constant
            this.viewParams.Height = height; // it does not need to be updated, because world size is constant

            this.maxAnimalsPerName = (int)(this.width * this.height * 0.00000001 * Preferences.newWorldMaxAnimalsMultiplier);
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"maxAnimalsPerName {maxAnimalsPerName}");

            this.pieceCountByName = new Dictionary<PieceTemplate.Name, int>();
            foreach (PieceTemplate.Name templateName in (PieceTemplate.Name[])Enum.GetValues(typeof(PieceTemplate.Name)))
            { this.pieceCountByName[templateName] = 0; }
            this.pieceCountByClass = new Dictionary<Type, int> { };

            this.trackingQueue = new Dictionary<string, Tracking>();
            this.eventQueue = new Dictionary<int, List<WorldEvent>>();
            this.hintEngine = new HintEngine(world: this);

            this.plantSpritesQueue = new List<Sprite>();
            this.animalSpritesQueue = new List<Sprite>();
            this.plantCellsQueue = new List<Cell>();
            this.processedAnimalsCount = 0;
            this.processedPlantsCount = 0;
            this.doNotCreatePiecesList = new List<PieceTemplate.Name> { };
            this.discoveredRecipesForPieces = new List<PieceTemplate.Name> { };
            this.camera = new Camera(this);
            this.camera.TrackCoords(new Vector2(0, 0));
            this.MapEnabled = false;
            this.mapMode = MapMode.None;
            this.mapCycle = SonOfRobinGame.platform == Platform.Mobile ? new List<MapMode> { MapMode.None, MapMode.Big } : new List<MapMode> { MapMode.None, MapMode.Small, MapMode.Big };
            this.mapBig = new Map(world: this, fullScreen: true, touchLayout: TouchLayout.Map);
            this.mapSmall = new Map(world: this, fullScreen: false, touchLayout: TouchLayout.Empty);
            this.playerPanel = new PlayerPanel(world: this);
            this.debugText = "";
            if (saveGameData == null) this.grid = new Grid(world: this, resDivider: resDivider);
            else { this.Deserialize(gridOnly: true); }

            this.AddLinkedScene(this.mapBig);
            this.AddLinkedScene(this.mapSmall);
            this.AddLinkedScene(this.playerPanel);

            this.solidColorManager = new SolidColorManager(this);
            if (this.demoMode) this.solidColorManager.Add(new SolidColor(color: Color.White, viewOpacity: 0.4f, clearScreen: false, priority: 1));

            SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
        }

        public void CompleteCreation()
        {
            if (this.grid.creationInProgress)
            {
                if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
                {
                    SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;

                    SonOfRobinGame.progressBar.TurnOff(addTransition: true);
                    bool menuFound = GetTopSceneOfType(typeof(Menu)) != null;

                    this.Remove();
                    new TextWindow(text: $"Entering the island has been cancelled.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, closingTask: menuFound ? Scheduler.TaskName.Empty : Scheduler.TaskName.OpenMainMenu);
                    return;
                };

                SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
                this.grid.RunNextCreationStage();
                return;
            }

            if (this.saveGameData == null && this.initialPiecesCreationFramesLeft > 0)
            {
                if (this.demoMode) CreateMissingPieces(outsideCamera: false, multiplier: 1f, addFruits: true);
                else
                {
                    string seedText = String.Format("{0:0000}", this.seed);

                    SonOfRobinGame.progressBar.TurnOn(
                    curVal: initialPiecesCreationFramesTotal - this.initialPiecesCreationFramesLeft,
                    maxVal: initialPiecesCreationFramesTotal,
                    text: $"preparing island\nseed {seedText}\n{this.width} x {this.height}\npopulating...");

                    if (this.initialPiecesCreationFramesLeft != initialPiecesCreationFramesTotal)
                    { // first iteration should only display progress bar
                        bool piecesCreated = CreateMissingPieces(maxAmountToCreateAtOnce: (uint)(300000 / initialPiecesCreationFramesTotal), outsideCamera: false, multiplier: 1f, addToDoNotCreateList: false, addFruits: true);
                        if (!piecesCreated) this.initialPiecesCreationFramesLeft = 0;
                    }
                    this.initialPiecesCreationFramesLeft--;

                    return;
                }
            }

            SonOfRobinGame.progressBar.TurnOff(addTransition: true);
            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.worldCreationInProgress = false;
            this.creationEnd = DateTime.Now;
            this.creationDuration = this.creationEnd - this.creationStart;
            PieceInfo.CreateAllInfo(world: this);
            Craft.PopulateAllCategories();
            this.lastSaved = DateTime.Now;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"World creation time: {creationDuration:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

            bool newGameStarted = this.saveGameData == null;

            if (newGameStarted)
            {
                this.currentFrame = 0;
                this.currentUpdate = 0;

                if (this.demoMode) this.camera.TrackLiveAnimal(fluidMotion: false);
                else
                {
                    this.player = (Player)this.PlacePlayer();

                    BoardPiece crate = PieceTemplate.CreateOnBoard(world: this, position: this.player.sprite.position, templateName: PieceTemplate.Name.CrateStarting);
                    if (crate.sprite.placedCorrectly) crate.sprite.MoveToClosestFreeSpot(this.player.sprite.position);
                }
            }
            else
            { this.Deserialize(gridOnly: false); }

            if (!this.demoMode)
            {
                this.camera.TrackPiece(trackedPiece: this.player, fluidMotion: false);
                this.UpdateViewParams(manualScale: 1f);
                this.camera.Update(); // to render cells in camera view correctly
                Inventory.SetLayout(newLayout: Inventory.Layout.Toolbar, player: this.player);
            }

            this.CreateNewDarknessMask();
            this.grid.LoadAllTexturesInCameraView();
            if (!this.demoMode)
            {
                this.mapBig.ForceRender();
                this.mapSmall.ForceRender();
            }
            SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;
            if (!this.demoMode && newGameStarted) this.hintEngine.ShowGeneralHint(type: HintEngine.Type.CineIntroduction, ignoreDelay: true);
        }

        private void Deserialize(bool gridOnly)
        {
            var saveGameDataDict = (Dictionary<string, Object>)this.saveGameData;

            // deserializing grid

            if (gridOnly) // grid has to be deserialized first
            {
                var gridData = (Dictionary<string, Object>)saveGameDataDict["grid"];
                this.grid = Grid.Deserialize(world: this, gridData: gridData, resDivider: this.resDivider);
                return;
            }

            // deserializing header

            var headerData = (Dictionary<string, Object>)saveGameDataDict["header"];

            this.currentFrame = (int)headerData["currentFrame"];
            this.currentUpdate = (int)headerData["currentUpdate"];
            this.TimePlayed = (TimeSpan)headerData["TimePlayed"];
            this.currentPieceId = (int)headerData["currentPieceId"];
            this.currentBuffId = (int)headerData["currentBuffId"];
            this.MapEnabled = (bool)headerData["MapEnabled"];
            this.maxAnimalsPerName = (int)headerData["maxAnimalsPerName"];
            this.doNotCreatePiecesList = (List<PieceTemplate.Name>)headerData["doNotCreatePiecesList"];
            this.discoveredRecipesForPieces = (List<PieceTemplate.Name>)headerData["discoveredRecipesForPieces"];

            // deserializing hints

            var hintsData = (Dictionary<string, Object>)saveGameDataDict["hints"];
            this.hintEngine.Deserialize(hintsData);

            // deserializing pieces

            this.piecesByOldId = new Dictionary<string, BoardPiece> { }; // needed to match old IDs to new pieces (with new, different IDs)
            var pieceDataBag = (ConcurrentBag<Object>)saveGameDataDict["pieces"];
            var newPiecesByOldTargetIds = new Dictionary<string, BoardPiece> { };

            foreach (Dictionary<string, Object> pieceData in pieceDataBag)
            {
                // repeated in StorageSlot

                PieceTemplate.Name templateName = (PieceTemplate.Name)pieceData["base_name"];

                bool female = pieceData.ContainsKey("animal_female") && (bool)pieceData["animal_female"];

                var newBoardPiece = PieceTemplate.CreateOnBoard(world: this, position: new Vector2((float)pieceData["sprite_positionX"], (float)pieceData["sprite_positionY"]), templateName: templateName, female: female);
                newBoardPiece.Deserialize(pieceData: pieceData);

                if (templateName == PieceTemplate.Name.Player)
                {
                    this.player = (Player)newBoardPiece;
                    this.camera.TrackPiece(trackedPiece: this.player, fluidMotion: false);
                }

                if (newBoardPiece.GetType() == typeof(Animal) && pieceData["animal_target_old_id"] != null)
                { newPiecesByOldTargetIds[(string)pieceData["animal_target_old_id"]] = newBoardPiece; }
            }

            foreach (var kvp in newPiecesByOldTargetIds)
            {
                Animal newAnimal = (Animal)kvp.Value;
                if (this.piecesByOldId.ContainsKey(kvp.Key)) newAnimal.target = this.piecesByOldId[kvp.Key];
            }

            // deserializing tracking

            var trackingDataList = (List<Object>)saveGameDataDict["tracking"];
            foreach (Dictionary<string, Object> trackingData in trackingDataList)
            { Tracking.Deserialize(world: this, trackingData: trackingData, piecesByOldId: this.piecesByOldId); }

            // deserializing planned events

            var eventDataList = (List<Object>)saveGameDataDict["events"];
            foreach (Dictionary<string, Object> eventData in eventDataList)
            { WorldEvent.Deserialize(world: this, eventData: eventData, piecesByOldId: this.piecesByOldId); }

            // finalizing

            this.piecesByOldId = null;
            this.saveGameData = null;
            this.freePiecesPlacingMode = false;

            MessageLog.AddMessage(msgType: MsgType.User, message: "Game has been loaded.", color: Color.Cyan);
        }

        public void AutoSave()
        {
            if (this.demoMode || this.currentUpdate % 600 != 0 || !this.player.alive) return;
            TimeSpan timeSinceLastSave = DateTime.Now - this.lastSaved;
            if (timeSinceLastSave < TimeSpan.FromMinutes(Preferences.autoSaveDelayMins)) return;

            if (this.player.activeState != BoardPiece.State.PlayerControlledWalking) return;
            if (GetTopSceneOfType(typeof(Menu)) != null) return;
            Scene invScene = GetTopSceneOfType(typeof(Inventory));
            if (invScene != null)
            {
                Inventory inventory = (Inventory)invScene;
                if (inventory.type != Inventory.Type.SingleBottom) return;
            }

            string timeElapsedTxt = timeSinceLastSave.ToString("mm\\:ss");
            MessageLog.AddMessage(msgType: MsgType.User, message: $"{timeElapsedTxt} elapsed - autosaving...", color: Color.LightBlue);

            var saveParams = new Dictionary<string, Object> { { "world", this }, { "saveSlotName", "0" }, { "showMessage", false } };
            new Scheduler.Task(taskName: Scheduler.TaskName.SaveGame, executeHelper: saveParams);
        }

        private BoardPiece PlacePlayer()
        {
            for (int tryIndex = 0; tryIndex < 65535; tryIndex++)
            {
                player = (Player)PieceTemplate.CreateOnBoard(world: this, position: new Vector2(random.Next(0, this.width), random.Next(0, this.height)), templateName: PieceTemplate.Name.Player);
                if (player.sprite.placedCorrectly)
                {
                    player.sprite.orientation = Sprite.Orientation.up;
                    player.sprite.CharacterStand();
                    player.sprite.allowedFields.RemoveTerrain(TerrainName.Danger); // player should be spawned in a safe place, but able to go everywhere afterwards
                    return player;
                }
            }

            throw new DivideByZeroException("Cannot place player sprite.");
        }

        public struct PieceCreationData
        {
            public readonly PieceTemplate.Name name;
            public readonly float multiplier;
            public readonly int maxAmount;
            public PieceCreationData(PieceTemplate.Name name, float multiplier, int maxAmount)
            {
                this.name = name;
                this.multiplier = multiplier;
                this.maxAmount = maxAmount; // -1 == no limit
            }
        }

        public bool CreateMissingPieces(uint maxAmountToCreateAtOnce = 300000, bool outsideCamera = false, float multiplier = 1.0f, bool clearDoNotCreateList = false, bool addToDoNotCreateList = true, bool addFruits = false)
        {
            if (clearDoNotCreateList) doNotCreatePiecesList.Clear();

            // preparing a dictionary of pieces to create

            var creationDataList = new List<PieceCreationData>
            {
                new PieceCreationData(name: PieceTemplate.Name.GrassRegular, multiplier: 2.0f, maxAmount: 1000),
                new PieceCreationData(name: PieceTemplate.Name.GrassGlow, multiplier: 0.1f, maxAmount: 40),
                new PieceCreationData(name: PieceTemplate.Name.GrassDesert, multiplier: 2.0f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.Rushes, multiplier: 2.0f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.WaterLily, multiplier: 1.0f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.FlowersPlain, multiplier: 0.4f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.FlowersRed, multiplier: 0.1f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.FlowersMountain, multiplier: 0.1f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.TreeSmall, multiplier: 1.0f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.TreeBig, multiplier: 1.0f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.AcornTree, multiplier: 0.03f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.AppleTree, multiplier: 0.03f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.CherryTree, multiplier: 0.03f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.BananaTree, multiplier: 0.03f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.TomatoPlant, multiplier: 0.03f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.PalmTree, multiplier: 1.0f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.Cactus, multiplier: 0.2f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.WaterRock, multiplier: 0.5f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.MineralsSmall, multiplier: 0.5f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.MineralsBig, multiplier: 0.3f, maxAmount: -1),
                new PieceCreationData(name: PieceTemplate.Name.IronDeposit, multiplier: 0.02f, maxAmount: 30),
                new PieceCreationData(name: PieceTemplate.Name.CoalDeposit, multiplier: 0.02f, maxAmount: 30),
                new PieceCreationData(name: PieceTemplate.Name.GlassDeposit, multiplier: 0.02f, maxAmount: 30),
                new PieceCreationData(name: PieceTemplate.Name.CrystalDepositBig, multiplier: 0.01f, maxAmount: 25),
                new PieceCreationData(name: PieceTemplate.Name.Shell, multiplier: 1f, maxAmount: 25),
                new PieceCreationData(name: PieceTemplate.Name.Clam, multiplier: 1f, maxAmount: 25),
                new PieceCreationData(name: PieceTemplate.Name.CrateRegular, multiplier: 0.1f, maxAmount: 2),
                new PieceCreationData(name: PieceTemplate.Name.Rabbit, multiplier: 0.6f, maxAmount: this.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Fox, multiplier: 0.4f, maxAmount: this.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Tiger, multiplier: 0.4f, maxAmount: this.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Frog, multiplier: 0.2f, maxAmount: this.maxAnimalsPerName),
            };

            Vector2 notReallyUsedPosition = new Vector2(-100, -100); // -100, -100 will be converted to a random position on the map - needed for effective creation of new sprites 
            int minPieceAmount = Math.Max(Convert.ToInt32((long)width * (long)height / 300000 * multiplier), 0); // 300000

            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (PieceCreationData creationData in creationDataList)
            {
                if (doNotCreatePiecesList.Contains(creationData.name)) continue;

                int minAmount = Math.Max((int)(minPieceAmount * creationData.multiplier), 4);
                if (creationData.maxAmount > -1) minAmount = Math.Min(minAmount, creationData.maxAmount);

                int amountToCreate = Math.Max(minAmount - this.pieceCountByName[creationData.name], 0);
                if (amountToCreate > 0) amountToCreateByName[creationData.name] = amountToCreate;
            }

            if (amountToCreateByName.Keys.ToList().Count == 0) return false;

            // creating pieces

            this.createMissinPiecesOutsideCamera = outsideCamera;
            int piecesCreated = 0;

            foreach (var kvp in amountToCreateByName)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                int amountLeftToCreate = Convert.ToInt32(kvp.Value); // separate from the iterator below (needs to be modified)

                for (int i = 0; i < kvp.Value * 5; i++)
                {
                    var newBoardPiece = PieceTemplate.CreateOnBoard(world: this, position: notReallyUsedPosition, templateName: pieceName);
                    if (newBoardPiece.sprite.placedCorrectly)
                    {
                        if (addFruits && newBoardPiece.GetType() == typeof(Plant) && this.random.Next(2) == 0)
                        {
                            Plant newPlant = (Plant)newBoardPiece;
                            if (newPlant.fruitEngine != null)
                            {
                                newPlant.fruitEngine.AddFruit();
                                newPlant.Mass = newPlant.reproduction.massNeeded / 2;
                            }
                        }

                        piecesCreated++;
                        amountLeftToCreate--;
                    }

                    if (amountLeftToCreate <= 0)
                    {
                        if (!this.doNotCreatePiecesList.Contains(pieceName) && addToDoNotCreateList)
                        {
                            this.doNotCreatePiecesList.Add(pieceName);
                            new WorldEvent(eventName: WorldEvent.EventName.RestorePieceCreation, delay: 15000, world: this, boardPiece: null, eventHelper: pieceName);
                        }
                        break;
                    }
                }

                if (piecesCreated >= maxAmountToCreateAtOnce) break;
            }

            if (piecesCreated > 0) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Created {piecesCreated} new pieces.");
            this.createMissinPiecesOutsideCamera = false;
            return piecesCreated > 0;
        }

        public void UpdateViewParams(float manualScale = 1f)
        {
            this.viewParams.ScaleX = manualScale;
            this.viewParams.ScaleY = manualScale;

            float scaleMultiplier = this.demoMode ? 2f : this.camera.currentZoom;
            this.viewParams.ScaleX *= 1 / (Preferences.WorldScale * scaleMultiplier);
            this.viewParams.ScaleY *= 1 / (Preferences.WorldScale * scaleMultiplier);

            this.camera.Update();
            this.viewParams.PosX = this.camera.viewPos.X;
            this.viewParams.PosY = this.camera.viewPos.Y;

            // width and height are set once in constructor
        }

        public override void Update(GameTime gameTime)
        {
            if (this.worldCreationInProgress)
            {
                this.CompleteCreation();
                return;
            }

            this.manualScale = 1f;
            this.ProcessInput();
            this.UpdateViewParams(manualScale: this.manualScale);

            this.grid.UnloadTexturesIfMemoryLow();
            this.grid.LoadClosestTextureInCameraView();

            if (this.demoMode) this.camera.TrackLiveAnimal(fluidMotion: true);
            // this.AutoSave(); // autosave is not needed anymore

            bool createMissingPieces = this.currentUpdate % 200 == 0 && Preferences.debugCreateMissingPieces;
            if (createMissingPieces) this.CreateMissingPieces(maxAmountToCreateAtOnce: 100, outsideCamera: true, multiplier: 0.1f);

            for (int i = 0; i < this.updateMultiplier; i++)
            {
                Tracking.ProcessTrackingQueue(this);
                WorldEvent.ProcessQueue(this);
                if (!this.BuildMode) this.UpdateAllAnims();

                if (this.player != null) this.ProcessOneNonPlant(this.player);

                if (this.currentUpdate % this.bulletTimeMultiplier == 0 && !this.BuildMode)
                {
                    this.StateMachinesProcessCameraView();

                    if (!createMissingPieces)
                    {
                        this.StateMachinesProcessAnimalQueue();
                        this.plantsProcessing = true;
                        this.StateMachinesProcessPlantQueue();
                        this.plantsProcessing = false;
                    }
                }
                this.currentUpdate++;
            }
        }

        private void ProcessInput()
        {
            if (this.demoMode || this.CineMode) return;

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldPauseMenu))
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Pause);
                return;
            }

            // analog movement (left stick)
            this.analogMovementLeftStick = InputMapper.Analog(InputMapper.Action.WorldWalk);
            this.analogMovementLeftStick *= 4;

            // analog movement (right stick)
            this.analogMovementRightStick = InputMapper.Analog(InputMapper.Action.WorldCameraMove);
            this.analogMovementRightStick *= 4;

            // analog camera control
            if (this.player.activeState != BoardPiece.State.PlayerControlledSleep)
            {
                this.analogCameraCorrection = InputMapper.Analog(InputMapper.Action.WorldCameraMove);
                this.analogCameraCorrection *= 10;
            }

            // camera zoom control (to keep the current zoom level, when other scene is above the world, that scene must block updates below)
            float leftTrigger = InputMapper.TriggerForce(InputMapper.Action.WorldCameraZoomOut);
            this.manualScale = 1f + leftTrigger;

            if (!this.player.alive || this.player.activeState != BoardPiece.State.PlayerControlledWalking) return;

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldSprintToggle)) this.SprintMode = true;

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldFieldCraft))
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CraftField);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldInventory) || this.playerPanel.IsCounterActivatedByTouch)
            {
                Inventory.SetLayout(Inventory.Layout.InventoryAndToolbar, player: this.player);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldEquip))
            {
                Inventory.SetLayout(Inventory.Layout.InventoryAndEquip, player: this.player);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.WorldMapToggle))
            {
                this.ToggleMapMode();
                return;
            }
        }

        public void ToggleMapMode()
        {
            if (!this.MapEnabled)
            {
                if (!this.hintEngine.ShowGeneralHint(HintEngine.Type.MapNegative)) new TextWindow(text: "I don't have map equipped.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1);
                return;
            }

            if (this.mapSmall.transManager.HasAnyTransition || this.mapBig.transManager.HasAnyTransition) return;

            this.mapMode = this.NextMapMode;

            switch (this.mapMode)
            {
                case MapMode.None:
                    this.mapSmall.TurnOff();
                    this.mapBig.TurnOff();
                    break;

                case MapMode.Small:
                    this.mapSmall.TurnOn();
                    this.mapBig.TurnOff();
                    break;

                case MapMode.Big:
                    this.mapSmall.TurnOff();
                    this.mapBig.TurnOn();
                    break;


                default:
                    throw new DivideByZeroException($"Unsupported mapMode - {mapMode}.");
            }
        }

        public static World GetTopWorld()
        {
            World world;
            var worldScene = GetTopSceneOfType(typeof(World));
            if (worldScene == null) return null;
            world = (World)worldScene;
            return world;
        }

        private void StateMachinesProcessCameraView()
        {
            if (!this.CanProcessMoreCameraRectPiecesNow)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Camera view SM: no time to start processing queue - {UpdateTimeElapsed.Milliseconds}ms.");
                return;
            }

            foreach (Sprite sprite in this.grid.GetSpritesInCameraView(camera: this.camera, groupName: Cell.Group.StateMachinesNonPlants))
            {
                if (!this.camera.viewRect.Intersects(sprite.gfxRect)) continue; // returned sprite list is a bit bigger than camera view

                this.ProcessOneNonPlant(sprite.boardPiece);

                if (!this.CanProcessMoreCameraRectPiecesNow)
                {
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Camera view SM: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");
                    return;
                }
            }
        }

        private void StateMachinesProcessAnimalQueue()
        {
            // actually it processes every state machine that is not a plant

            if (!this.CanProcessMoreAnimalsNow) return;
            if ((SonOfRobinGame.lastUpdateDelay > 20 || SonOfRobinGame.lastDrawDelay > 20) && this.updateMultiplier == 1) return;

            if (this.animalSpritesQueue.Count == 0)
            {
                this.animalSpritesQueue = this.grid.GetAllSprites(groupName: Cell.Group.StateMachinesNonPlants).OrderBy(sprite => sprite.boardPiece.lastFrameSMProcessed).ToList();
                return;
            }

            this.processedAnimalsCount = 0;

            while (true)
            {
                if (animalSpritesQueue.Count == 0) return;

                BoardPiece currentAnimal = this.animalSpritesQueue[0].boardPiece;
                this.animalSpritesQueue.RemoveAt(0);

                this.ProcessOneNonPlant(currentAnimal);
                this.processedAnimalsCount++;

                if (!this.CanProcessMoreAnimalsNow)
                {
                    if (UpdateTimeElapsed.Milliseconds > 13 && this.updateMultiplier == 1) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Animals: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");

                    return;
                }
            }
        }

        private void ProcessOneNonPlant(BoardPiece piece)
        {
            if (piece.maxAge > 0) piece.GrowOlder();
            if (piece.GetType() == typeof(Animal))
            {
                Animal animal = (Animal)piece;
                if (animal.isPregnant && animal.alive) animal.pregnancyFramesLeft = Math.Max(animal.pregnancyFramesLeft - 1, 0);
                if (this.currentUpdate % 10 == 0) animal.ExpendEnergy(1);

                if (animal.alive && (animal.hitPoints <= 0 || animal.efficiency == 0 || animal.currentAge >= animal.maxAge))
                {
                    animal.Kill();
                    return;
                }
            }
            else if (piece.GetType() == typeof(Player))
            {
                Player player = (Player)piece;
                if (player.hitPoints <= 0 && player.alive)
                {
                    player.Kill();
                    return;
                }
            }

            piece.StateMachineWork();
        }

        private void StateMachinesProcessPlantQueue()
        {
            if (!this.CanProcessMorePlantsNow) return;
            if ((SonOfRobinGame.lastUpdateDelay > 20 || SonOfRobinGame.lastDrawDelay > 20) && this.updateMultiplier == 1) return;

            this.processedPlantsCount = 0;

            if (this.plantCellsQueue.Count == 0)
            {
                this.plantCellsQueue = new List<Cell>(this.grid.allCells.ToList());
                //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Plants cells queue replenished ({this.plantCellsQueue.Count})");
                return; // to avoid doing too many calculations in one update
            }

            if (this.plantSpritesQueue.Count == 0)
            {
                int noOfCellsToAdd = 500;

                for (int i = 0; i < noOfCellsToAdd; i++)
                {
                    int randomCellNo = random.Next(0, plantCellsQueue.Count);
                    Cell cell = this.plantCellsQueue[randomCellNo];
                    this.plantCellsQueue.RemoveAt(randomCellNo);
                    this.plantSpritesQueue.AddRange(cell.spriteGroups[Cell.Group.StateMachinesPlants].Values); // not shuffled to save cpu time

                    if (plantCellsQueue.Count == 0) break;
                }
                return; // to avoid doing too many calculations in one update
            }

            Plant currentPlant;

            while (true)
            {
                if (plantSpritesQueue.Count == 0) return;

                currentPlant = (Plant)this.plantSpritesQueue[0].boardPiece;
                this.plantSpritesQueue.RemoveAt(0);

                currentPlant.StateMachineWork();
                currentPlant.GrowOlder();
                if (currentPlant.currentAge >= currentPlant.maxAge || currentPlant.efficiency < 0.2 || currentPlant.Mass < 1) currentPlant.Kill();
                this.processedPlantsCount++;

                if (!this.CanProcessMorePlantsNow)
                {
                    if (UpdateTimeElapsed.Milliseconds > 13 && this.updateMultiplier == 1) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Plants: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");
                    return;
                }
            }
        }
        public void EnterBuildMode(Craft.Recipe recipe)
        {
            this.buildMode = true;

            this.touchLayout = TouchLayout.WorldBuild;
            this.tipsLayout = ControlTips.TipsLayout.WorldBuild;

            Scene craftMenu = GetTopSceneOfType(typeof(Menu));
            craftMenu.MoveToBottom();
            SonOfRobinGame.hintWindow.TurnOff();

            this.player.recipeToBuild = recipe;
            this.freePiecesPlacingMode = true;
            this.player.pieceToBuild = PieceTemplate.CreateOnBoard(templateName: recipe.pieceToCreate, world: this, position: this.player.sprite.position);
            this.freePiecesPlacingMode = false;
            this.player.pieceToBuild.sprite.MoveToClosestFreeSpot(this.player.pieceToBuild.sprite.position);
            this.player.activeState = BoardPiece.State.PlayerControlledBuilding;
        }

        public void ExitBuildMode(bool craftPiece)
        {
            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;

            this.player.activeState = BoardPiece.State.PlayerControlledWalking;

            Scene craftMenu = GetBottomSceneOfType(typeof(Menu));
            craftMenu.MoveToTop();

            if (craftPiece) this.player.recipeToBuild.TryToProducePieces(player: this.player);

            this.player.recipeToBuild = null;
            this.player.pieceToBuild.Destroy();
            this.player.pieceToBuild = null;
            this.buildMode = false;
        }

        public void UpdateAllAnims()
        {
            var visibleSpritesList = this.grid.GetSpritesInCameraView(camera: this.camera, groupName: Cell.Group.Visible, compareWithCameraRect: true);

            foreach (var sprite in visibleSpritesList)
            { sprite.UpdateAnimation(); }
        }

        public void UpdateFogOfWar()
        {
            this.mapBig.dirtyFog = true;
            this.mapSmall.dirtyFog = true;
        }

        public void AddPauseMenuTransitions()
        {
            if (this.demoMode) return;

            SolidColor blackOverlay = new SolidColor(color: Color.Black, viewOpacity: 0f);
            blackOverlay.viewParams.Opacity = 0f;

            blackOverlay.transManager.AddMultipleTransitions(paramsToChange: new Dictionary<string, float> {
                { "Opacity", 0.4f }},
                duration: 20, outTrans: true, requireInputActiveAtRepeats: true, inputActiveAtRepeatsScene: this, playCount: 2, refreshBaseVal: false, endRemoveScene: true);

            this.solidColorManager.Add(blackOverlay);
        }

        public Vector2 TranslateScreenToWorldPos(Vector2 screenPos)
        {
            return new Vector2(
                x: (screenPos.X / Preferences.GlobalScale * this.viewParams.ScaleX) - this.viewParams.DrawPos.X,
                y: (screenPos.Y / Preferences.GlobalScale * this.viewParams.ScaleY) - this.viewParams.DrawPos.Y);
        }

        public override void Draw()
        {
            // preparing a list of sprites, that cast shadows

            List<Sprite> blockingLightSpritesList;
            if ((Preferences.drawSunShadows && AmbientLight.SunLightData.CalculateSunLight(this.islandClock.IslandDateTime).sunShadowsColor != Color.Transparent) || (Preferences.drawShadows && AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime).darknessColor != Color.Transparent))
            {
                blockingLightSpritesList = this.grid.GetSpritesInCameraView(camera: this.camera, groupName: Cell.Group.ColBlocking, compareWithCameraRect: false);
                blockingLightSpritesList = blockingLightSpritesList.OrderBy(o => o.gfxRect.Bottom).ToList();
            }
            else blockingLightSpritesList = new List<Sprite>();

            var lightSprites = this.UpdateDarknessMask(blockingLightSpritesList: blockingLightSpritesList); // Has to be called first, because calling SetRenderTarget() after any draw will wipe the screen black.

            // drawing blue background (to ensure drawing even if screen is larger than map)
            SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            if (this.worldCreationInProgress) return;

            // drawing background and sprites
            this.grid.DrawBackground(camera: this.camera);

            // drawing sprites
            var noOfDisplayedSprites = this.grid.DrawSprites(camera: this.camera, blockingLightSpritesList: blockingLightSpritesList);

            // drawing grid cells
            if (Preferences.debugShowCellData) this.grid.DrawDebugData();

            // drawing light and darkness
            this.DrawLightAndDarkness(lightSprites);

            // drawing field tips
            FieldTip.DrawFieldTips(world: this);

            // updating debugText
            if (Preferences.DebugMode) this.debugText = $"objects {this.PieceCount}, visible {noOfDisplayedSprites}";

            this.currentFrame++;
        }

        protected override void AdaptToNewSize()
        {
            this.CreateNewDarknessMask();
        }

        public void CreateNewDarknessMask()
        {
            Vector2 darknessMaskScale = this.DarknessMaskScale;
            Rectangle extendedViewRect = this.camera.ExtendedViewRect;

            if (this.darknessMask != null) this.darknessMask.Dispose();
            this.darknessMask = new RenderTarget2D(SonOfRobinGame.graphicsDevice, (int)(extendedViewRect.Width / darknessMaskScale.X), (int)(extendedViewRect.Height / darknessMaskScale.Y));

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Creating new darknessMask - {darknessMask.Width}x{darknessMask.Height}");
        }

        private List<Sprite> UpdateDarknessMask(List<Sprite> blockingLightSpritesList)
        {
            // Has to be called a the beggining, because calling SetRenderTarget() after any draw will wipe the screen black.

            // searching for light sources

            var foundSprites = this.grid.GetSpritesInCameraView(camera: camera, groupName: Cell.Group.LightSource);
            var lightSprites = foundSprites.OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom).ToList();

            // returning if darkness is transparent

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime);

            if (ambientLightData.darknessColor == Color.Transparent)
            {
                SonOfRobinGame.graphicsDevice.SetRenderTarget(this.darknessMask); // SetRenderTarget() wipes the target black!
                SonOfRobinGame.graphicsDevice.Clear(ambientLightData.darknessColor);

                SonOfRobinGame.spriteBatch.End();
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
                SonOfRobinGame.graphicsDevice.SetRenderTarget(null); // SetRenderTarget() wipes the target black!

                return lightSprites;
            }

            Vector2 darknessMaskScale = this.DarknessMaskScale;

            // preparing shadow masks

            // BlendFunction.Min and BlendFunction.Max will not work on Android!
            var shadowBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            var shadowBlendRedraw = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceColor,
            };

            BlendState lightBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.SourceAlpha,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.DestinationColor,
            };

            int tempShadowMaskIndex = 0;
            foreach (var lightSprite in lightSprites)
            {
                if (!Preferences.drawShadows)
                {
                    foreach (RenderTarget2D shadowMask in SonOfRobinGame.tempShadowMaskList)
                    { shadowMask.Dispose(); }
                    SonOfRobinGame.tempShadowMaskList.Clear();
                }
                else
                {
                    if (SonOfRobinGame.tempShadowMaskList.Count - 1 < tempShadowMaskIndex) SonOfRobinGame.tempShadowMaskList.Add(new RenderTarget2D(graphicsDevice: SonOfRobinGame.graphicsDevice, width: SonOfRobinGame.lightSphere.Width, height: SonOfRobinGame.lightSphere.Height));

                    Rectangle lightRect = lightSprite.lightEngine.Rect;
                    lightSprite.lightEngine.tempShadowMaskIndex = tempShadowMaskIndex;

                    // drawing shadows onto shadow mask

                    RenderTarget2D tempShadowMask = SonOfRobinGame.tempShadowMaskList[tempShadowMaskIndex];

                    SonOfRobinGame.spriteBatch.End();
                    SonOfRobinGame.graphicsDevice.SetRenderTarget(tempShadowMask); // SetRenderTarget() wipes the target black!
                    SonOfRobinGame.graphicsDevice.Clear(Color.Black);

                    Matrix scaleMatrix = Matrix.CreateScale( // to match drawing size with rect size (light texture size differs from light rect size)
                        (float)tempShadowMask.Width / (float)lightRect.Width,
                        (float)tempShadowMask.Height / (float)lightRect.Height,
                        1f);

                    SonOfRobinGame.spriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlend);

                    // first pass - drawing shadows
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        if (shadowSprite == lightSprite || !lightSprite.lightEngine.castShadows || !lightRect.Intersects(shadowSprite.gfxRect)) continue;

                        float shadowAngle = Helpers.GetAngleBetweenTwoPoints(start: lightSprite.gfxRect.Center, end: shadowSprite.position);

                        Sprite.DrawShadow(color: Color.White, shadowSprite: shadowSprite, lightPos: lightSprite.position, shadowAngle: shadowAngle, drawOffsetX: -lightRect.X, drawOffsetY: -lightRect.Y);
                    }

                    SonOfRobinGame.spriteBatch.End();
                    SonOfRobinGame.spriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlendRedraw);

                    // second pass - erasing shadow from original sprites' position
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        // the lightSprite should be also redrawn, to avoid being overdrawn with any shadow
                        if (lightRect.Intersects(shadowSprite.gfxRect)) shadowSprite.DrawRoutine(calculateSubmerge: true, offsetX: -lightRect.X, offsetY: -lightRect.Y);
                    }

                    // drawing light on top of shadows

                    SonOfRobinGame.spriteBatch.End();
                    SonOfRobinGame.spriteBatch.Begin(blendState: lightBlend);
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSphere, tempShadowMask.Bounds, Color.White);

                    tempShadowMaskIndex++;
                }
            }

            // preparing darkness mask

            var darknessMaskBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            SonOfRobinGame.spriteBatch.End();

            SonOfRobinGame.spriteBatch.Begin(blendState: darknessMaskBlend);
            SonOfRobinGame.graphicsDevice.SetRenderTarget(this.darknessMask); // SetRenderTarget() wipes the target black!
            SonOfRobinGame.graphicsDevice.Clear(ambientLightData.darknessColor);

            // subtracting shadow masks from darkness

            Rectangle extendedViewRect = this.camera.ExtendedViewRect;
            foreach (var lightSprite in lightSprites)
            {
                Rectangle lightRect = lightSprite.lightEngine.Rect;

                lightRect.X = (int)(((float)lightRect.X - (float)extendedViewRect.X) / darknessMaskScale.X);
                lightRect.Y = (int)(((float)lightRect.Y - (float)extendedViewRect.Y) / darknessMaskScale.Y);
                lightRect.Width = (int)((float)lightRect.Width / darknessMaskScale.X);
                lightRect.Height = (int)((float)lightRect.Height / darknessMaskScale.Y);

                if (Preferences.drawShadows)
                {
                    RenderTarget2D tempShadowMask = SonOfRobinGame.tempShadowMaskList[lightSprite.lightEngine.tempShadowMaskIndex];
                    lightSprite.lightEngine.tempShadowMaskIndex = -1; // discarding the index
                    SonOfRobinGame.spriteBatch.Draw(tempShadowMask, lightRect, Color.White * lightSprite.lightEngine.Opacity);
                }
                else
                {
                    lightRect.Inflate(lightRect.Width * 0.1f, lightRect.Height * 0.1f); // light level compensation
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSphere, lightRect, Color.White * lightSprite.lightEngine.Opacity);
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSphere, lightRect, Color.White * lightSprite.lightEngine.Opacity * 0.08f); // light level compensation
                }
            }

            SonOfRobinGame.spriteBatch.End();
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.graphicsDevice.SetRenderTarget(null); // SetRenderTarget() wipes the target black!

            return lightSprites;
        }

        private void DrawLightAndDarkness(List<Sprite> lightSprites)
        {
            if (!Preferences.showLighting) return;

            Vector2 darknessMaskScale = this.DarknessMaskScale;

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime);
            Rectangle extendedViewRect = this.camera.ExtendedViewRect;

            // drawing ambient light

            SonOfRobinGame.spriteBatch.End();
            BlendState ambientBlend = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.One
            };
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Immediate, blendState: ambientBlend);
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, extendedViewRect, ambientLightData.lightColor);

            // drawing point lights

            SonOfRobinGame.spriteBatch.End();
            BlendState colorLightBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Deferred, blendState: colorLightBlend);

            foreach (var lightSprite in lightSprites)
            {
                if (lightSprite.lightEngine.ColorActive)
                {
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSphere, lightSprite.lightEngine.Rect, lightSprite.lightEngine.Color);
                }
            }

            // drawing darkness
            if (ambientLightData.darknessColor != Color.Transparent)
            {
                SonOfRobinGame.spriteBatch.End();
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);

                SonOfRobinGame.spriteBatch.Draw(this.darknessMask, new Rectangle(x: extendedViewRect.X, y: extendedViewRect.Y, width: (int)(this.darknessMask.Width * darknessMaskScale.X), height: (int)(this.darknessMask.Height * darknessMaskScale.Y)), Color.White);
            }
        }
    }
}
