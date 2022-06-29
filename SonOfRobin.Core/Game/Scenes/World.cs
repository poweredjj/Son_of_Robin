using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class World : Scene
    {
        public enum ActionKeys
        {
            Left,
            Right,
            Up,
            Down,
            UseToolbarPiecePress,
            UseToolbarPieceHold,
            Interact,
            PickUp,
            Inventory,
            Equip,
            FieldCraft,
            Run,
            MapToggle,
            PauseMenu,
            ShootingMode,
        }

        public enum MapMode
        {
            None,
            Small,
            Big
        }

        public static readonly List<ActionKeys> DirectionKeys = new List<ActionKeys> { ActionKeys.Left, ActionKeys.Right, ActionKeys.Up, ActionKeys.Down };
        public static readonly List<ActionKeys> InteractKeys = new List<ActionKeys> { ActionKeys.Interact, ActionKeys.PickUp, ActionKeys.Inventory, ActionKeys.FieldCraft, ActionKeys.Run, ActionKeys.MapToggle, ActionKeys.PauseMenu };

        public bool ActionKeysContainDirection
        {
            get
            {
                foreach (ActionKeys actionKey in this.actionKeyList)
                { if (DirectionKeys.Contains(actionKey)) return true; }

                return false;
            }
        }

        public List<ActionKeys> actionKeyList = new List<ActionKeys> { };
        public Vector2 analogMovementLeftStick;
        public Vector2 analogMovementRightStick;
        public Vector2 analogCameraCorrection;
        public float rightTriggerPreviousFrame;

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
        private bool spectatorMode;
        public bool SpectatorMode
        {
            get { return this.spectatorMode; }
            set
            {
                if (this.spectatorMode == value) return;
                this.spectatorMode = value;

                this.colorOverlay.transManager.AddTransition(new Transition(transManager: this.colorOverlay.transManager, outTrans: true, duration: 30, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: 0.0f));

                if (this.spectatorMode)
                {
                    if (this.player != null) this.player.RemoveFromStateMachines();

                    this.spectator = PieceTemplate.CreateOnBoard(world: this, position: this.camera.TrackedPos, templateName: PieceTemplate.Name.PlayerGhost);
                    this.spectator.sprite.orientation = this.player != null ? this.player.sprite.orientation : Sprite.Orientation.right;

                    this.camera.TrackPiece(this.spectator);
                    this.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.1f);

                    this.MapEnabled = true;
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
                    if (this.player == null || !this.player.alive)
                    {
                        this.player = (Player)this.PlacePlayer();
                        this.player.sprite.MoveToClosestFreeSpot(this.camera.TrackedPos);
                        this.camera.TrackPiece(this.player);
                        if (this.spectator != null) this.player.sprite.orientation = this.spectator.sprite.orientation;
                    }
                    else this.player.AddToStateMachines();

                    this.camera.TrackPiece(this.player);

                    if (this.spectator != null)
                    {
                        this.spectator.Destroy();
                        this.spectator = null;
                    }

                    this.MapEnabled = this.player.equipStorage.CountPieceOccurences(PieceTemplate.Name.Map) > 0;
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

                    taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.ChangeSceneInputType, delay: 0, executeHelper: new Dictionary<string, Object> { { "scene", this }, { "inputType", InputTypes.None } }, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null, storeForLaterUse: true));
                }
                else
                {
                    this.touchLayout = TouchLayout.WorldMain;
                    this.tipsLayout = ControlTips.TipsLayout.WorldMain;

                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.AddTransition, delay: 0, executeHelper: new Dictionary<string, Object> {
                            { "scene", this.colorOverlay },
                            { "transition", new Transition(transManager: this.colorOverlay.transManager, outTrans: true, baseParamName: "Opacity", targetVal: 0f, duration: 10, endCopyToBase: true, storeForLaterUse: true) } }, menu: null, storeForLaterUse: true));

                    taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(taskName: Scheduler.TaskName.CameraSetZoom, delay: 0, executeHelper: new Dictionary<string, Object> { { "zoom", 1f }, { "zoomSpeedMultiplier", 1f } }, menu: null, storeForLaterUse: true));
                    taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.CameraTrackPiece, delay: 0, executeHelper: this.player, storeForLaterUse: true));

                    taskChain.Add(new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.ChangeSceneInputType, delay: 0, executeHelper: new Dictionary<string, Object> { { "scene", this }, { "inputType", Scene.InputTypes.Normal } }, storeForLaterUse: true));
                }

                new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.ExecuteTaskChain, turnOffInputUntilExecution: true, executeHelper: taskChain);
            }
        }

        public readonly int seed;
        public readonly int resDivider;
        public FastNoiseLite noise;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Camera camera;
        private RenderTarget2D darknessMask;
        private float darknessMaskScale;
        public MapMode mapMode;
        public List<MapMode> mapCycle;
        public readonly Map mapBig;
        public readonly Map mapSmall;
        public readonly SolidColor colorOverlay;
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
        public BoardPiece spectator;
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
        public readonly IslandClock islandClock;
        public string debugText;
        public int processedAnimalsCount;
        public int processedPlantsCount;
        public readonly List<PieceTemplate.Name> doNotCreatePiecesList;
        public readonly DateTime createdTime; // for calculating time spent in game
        private TimeSpan timePlayed; // real time spent while playing (differs from currentUpdate because of island time compression via updateMultiplier)
        public TimeSpan TimePlayed
        {
            get
            { return timePlayed + (DateTime.Now - createdTime); }
            set { timePlayed = value; }
        }

        public bool CanProcessMoreCameraRectPiecesNow { get { return UpdateTimeElapsed.Milliseconds <= 4 * this.updateMultiplier; } }
        public bool CanProcessMoreAnimalsNow { get { return UpdateTimeElapsed.Milliseconds <= 6 * this.updateMultiplier; } }
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
        public World(int width, int height, int seed, int resDivider, Object saveGameData = null, bool demoMode = false) :
              base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.demoMode = demoMode;
            this.cineMode = false;
            this.spectatorMode = false;
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
            this.islandClock = new IslandClock(world: this);

            this.currentPieceId = 0;
            this.currentBuffId = 0;

            this.noise = new FastNoiseLite(this.seed);

            this.width = width;
            this.height = height;
            this.viewParams.Width = width; // it does not need to be updated, because world size is constant
            this.viewParams.Height = height; // it does not need to be updated, because world size is constant

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
            this.camera = new Camera(this);
            this.camera.TrackCoords(new Vector2(0, 0));
            this.MapEnabled = false;
            this.mapMode = MapMode.None;
            this.mapCycle = SonOfRobinGame.platform == Platform.Mobile ? new List<MapMode> { MapMode.None, MapMode.Big } : new List<MapMode> { MapMode.None, MapMode.Small, MapMode.Big };
            this.mapBig = new Map(world: this, fullScreen: true, touchLayout: TouchLayout.Map);
            this.mapSmall = new Map(world: this, fullScreen: false, touchLayout: TouchLayout.Empty);
            if (saveGameData == null) this.grid = new Grid(world: this, resDivider: resDivider);
            else { this.Deserialize(gridOnly: true); }
            this.rightTriggerPreviousFrame = 0f;

            this.AddLinkedScene(this.mapBig);
            this.AddLinkedScene(this.mapSmall);
            this.AddLinkedScene(new PlayerPanel(world: this));

            this.colorOverlay = new SolidColor(color: Color.White, viewOpacity: this.demoMode ? 0.4f : 0f, clearScreen: false, priority: 1);
            this.AddLinkedScene(colorOverlay);

            SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
        }

        public void CompleteCreation()
        {
            if (this.grid.creationInProgress)
            {
                if (Keyboard.IsPressed(Keys.Escape) ||
                    GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B) ||
                    VirtButton.IsButtonDown(VButName.Return))
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
                if (this.demoMode) CreateMissingPieces(outsideCamera: false, multiplier: 1f);
                else
                {
                    string seedText = String.Format("{0:0000}", this.seed);

                    SonOfRobinGame.progressBar.TurnOn(
                    curVal: initialPiecesCreationFramesTotal - this.initialPiecesCreationFramesLeft,
                    maxVal: initialPiecesCreationFramesTotal,
                    text: $"preparing island\nseed {seedText}\n{this.width} x {this.height}\npopulating...");

                    if (this.initialPiecesCreationFramesLeft != initialPiecesCreationFramesTotal)
                    { // first iteration should only display progress bar
                        bool piecesCreated = CreateMissingPieces(maxAmountToCreateAtOnce: (uint)(300000 / initialPiecesCreationFramesTotal), outsideCamera: false, multiplier: 1f, addToDoNotCreateList: false);
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
            this.spectator = null;

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
                SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.player);
            }

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

            // deserializing hints

            var hintsData = (Dictionary<string, Object>)saveGameDataDict["hints"];
            this.hintEngine.Deserialize(hintsData);

            // deserializing pieces

            this.piecesByOldId = new Dictionary<string, BoardPiece> { }; // needed to match old IDs to new pieces (with new, different IDs)
            var pieceDataList = (List<Object>)saveGameDataDict["pieces"];
            var newPiecesByOldTargetIds = new Dictionary<string, BoardPiece> { };

            foreach (Dictionary<string, Object> pieceData in pieceDataList)
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
                if (inventory.layout != Inventory.Layout.SingleBottom) return;
            }

            string timeElapsedTxt = timeSinceLastSave.ToString("mm\\:ss");
            MessageLog.AddMessage(msgType: MsgType.User, message: $"{timeElapsedTxt} elapsed - autosaving...", color: Color.LightBlue);

            var saveParams = new Dictionary<string, Object> { { "world", this }, { "saveSlotName", "0" }, { "showMessage", false } };
            new Scheduler.Task(menu: null, taskName: Scheduler.TaskName.SaveGame, executeHelper: saveParams);
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
                this.maxAmount = maxAmount; // 0 == no limit
            }
        }

        public bool CreateMissingPieces(uint maxAmountToCreateAtOnce = 300000, bool outsideCamera = false, float multiplier = 1.0f, bool clearDoNotCreateList = false, bool addToDoNotCreateList = true)
        {
            if (clearDoNotCreateList) doNotCreatePiecesList.Clear();

            // preparing a dictionary of pieces to create

            var creationDataList = new List<PieceCreationData>
            {
                new PieceCreationData(name: PieceTemplate.Name.GrassRegular, multiplier: 2.0f, maxAmount: 1000),
                new PieceCreationData(name: PieceTemplate.Name.GlowGrass, multiplier: 0.1f, maxAmount: 40),
                new PieceCreationData(name: PieceTemplate.Name.GrassDesert, multiplier: 2.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.Rushes, multiplier: 2.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.WaterLily, multiplier: 1.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.FlowersPlain, multiplier: 0.4f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.FlowersMountain, multiplier: 0.1f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.TreeSmall, multiplier: 1.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.TreeBig, multiplier: 1.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.AcornTree, multiplier: 0.03f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.AppleTree, multiplier: 0.03f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.CherryTree, multiplier: 0.03f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.BananaTree, multiplier: 0.03f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.TomatoPlant, multiplier: 0.03f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.PalmTree, multiplier: 1.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.Cactus, multiplier: 0.2f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.WaterRock, multiplier: 0.5f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.MineralsSmall, multiplier: 0.5f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.MineralsBig, multiplier: 0.3f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.IronDeposit, multiplier: 0.01f, maxAmount: 80),
                new PieceCreationData(name: PieceTemplate.Name.CoalDeposit, multiplier: 0.01f, maxAmount: 80),
                new PieceCreationData(name: PieceTemplate.Name.Shell, multiplier: 1f, maxAmount: 50),
                new PieceCreationData(name: PieceTemplate.Name.CrateRegular, multiplier: 0.1f, maxAmount: 1),
                new PieceCreationData(name: PieceTemplate.Name.Rabbit, multiplier: 0.2f, maxAmount: Animal.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Fox, multiplier: 0.2f, maxAmount: Animal.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Tiger, multiplier: 0.2f, maxAmount: Animal.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Frog, multiplier: 0.2f, maxAmount: Animal.maxAnimalsPerName),
            };

            Vector2 notReallyUsedPosition = new Vector2(-100, -100); // -100, -100 will be converted to a random position on the map - needed for effective creation of new sprites 
            int minPieceAmount = Math.Max(Convert.ToInt32((long)width * (long)height / 300000 * multiplier), 0); // 300000

            int minAmount;
            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (PieceCreationData creationData in creationDataList)
            {
                if (doNotCreatePiecesList.Contains(creationData.name)) continue;

                minAmount = Math.Max((int)(minPieceAmount * creationData.multiplier), 4);
                if (creationData.maxAmount > 0) minAmount = Math.Min(minAmount, creationData.maxAmount);

                int amountToCreate = Math.Max(minAmount - this.pieceCountByName[creationData.name], 0);
                if (amountToCreate > 0) amountToCreateByName[creationData.name] = amountToCreate;
            }

            if (amountToCreateByName.Keys.ToList().Count == 0) return false;

            // creating pieces

            this.createMissinPiecesOutsideCamera = outsideCamera;
            int piecesCreated = 0;
            int amountLeftToCreate;
            PieceTemplate.Name pieceName;

            foreach (var kvp in amountToCreateByName)
            {
                pieceName = kvp.Key;
                amountLeftToCreate = Convert.ToInt32(kvp.Value); // separate from the iterator below (needs to be modified)

                for (int i = 0; i < kvp.Value * 5; i++)
                {
                    var newBoardPiece = PieceTemplate.CreateOnBoard(world: this, position: notReallyUsedPosition, templateName: pieceName);
                    if (newBoardPiece.sprite.placedCorrectly)
                    {
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
            this.viewParams.ScaleX *= 1 / (Preferences.worldScale * scaleMultiplier);
            this.viewParams.ScaleY *= 1 / (Preferences.worldScale * scaleMultiplier);

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

            float manualScale = this.ProcessInput();
            this.UpdateViewParams(manualScale: manualScale);

            this.grid.UnloadTexturesIfMemoryLow();
            this.grid.LoadClosestTextureInCameraView();

            if (this.demoMode) this.camera.TrackLiveAnimal(fluidMotion: true);
            this.AutoSave();

            bool createMissingPieces = this.currentUpdate % 200 == 0 && Preferences.debugCreateMissingPieces;

            for (int i = 0; i < this.updateMultiplier; i++)
            {
                WorldEvent.ProcessQueue(this);
                this.UpdateAllAnims();

                this.StateMachinesProcessCameraView();

                if (createMissingPieces)
                {
                    this.CreateMissingPieces(maxAmountToCreateAtOnce: 100, outsideCamera: true, multiplier: 0.1f);
                    createMissingPieces = false;
                }
                else
                {
                    this.StateMachinesProcessAnimalQueue();
                    this.plantsProcessing = true;
                    this.StateMachinesProcessPlantQueue();
                    this.plantsProcessing = false;
                }

                Tracking.ProcessTrackingQueue(this);
                this.currentUpdate++;
            }
        }

        private float ProcessInput()
        {
            actionKeyList.Clear();

            // pause menu

            if (GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Start) || Keyboard.HasBeenPressed(Keys.Escape) || VirtButton.HasButtonBeenPressed(VButName.PauseMenu)) actionKeyList.Add(ActionKeys.PauseMenu);

            // directional movement

            if (Keyboard.IsPressed(Keys.W) || Keyboard.IsPressed(Keys.Up)) actionKeyList.Add(ActionKeys.Up);
            if (Keyboard.IsPressed(Keys.S) || Keyboard.IsPressed(Keys.Down)) actionKeyList.Add(ActionKeys.Down);
            if (Keyboard.IsPressed(Keys.A) || Keyboard.IsPressed(Keys.Left)) actionKeyList.Add(ActionKeys.Left);
            if (Keyboard.IsPressed(Keys.D) || Keyboard.IsPressed(Keys.Right)) actionKeyList.Add(ActionKeys.Right);

            // analog movement (left stick)

            if (TouchInput.LeftStick.X != 0 || TouchInput.LeftStick.Y != 0)
            { this.analogMovementLeftStick = TouchInput.LeftStick / 15; }
            else
            {
                this.analogMovementLeftStick = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.Circular).ThumbSticks.Left;
                this.analogMovementLeftStick = new Vector2(analogMovementLeftStick.X * 4, analogMovementLeftStick.Y * -4);
            }

            // analog movement (right stick)

            if (TouchInput.RightStick.X != 0 || TouchInput.RightStick.Y != 0)
            { this.analogMovementRightStick = TouchInput.RightStick / 15; }
            else
            {
                this.analogMovementRightStick = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.Circular).ThumbSticks.Right;
                this.analogMovementRightStick = new Vector2(analogMovementRightStick.X * 4, analogMovementRightStick.Y * -4);
            }

            // analog camera control

            if (!this.demoMode && this.player.activeState != BoardPiece.State.PlayerControlledSleep)
            {
                if (TouchInput.RightStick.X != 0 || TouchInput.RightStick.Y != 0)
                { this.analogCameraCorrection = new Vector2(TouchInput.RightStick.X / 30, TouchInput.RightStick.Y / 60); }
                else
                {
                    this.analogCameraCorrection = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.Circular).ThumbSticks.Right;
                    this.analogCameraCorrection = new Vector2(analogCameraCorrection.X * 10, analogCameraCorrection.Y * -10);
                }
            }

            // interact

            if (Keyboard.HasBeenPressed(Keys.RightShift) ||
            GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.A) ||
            VirtButton.HasButtonBeenPressed(VButName.Interact)
           ) actionKeyList.Add(ActionKeys.Interact);

            // use tool

            float rightTrigger = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).Triggers.Right;
            if (rightTrigger > 0.5f) actionKeyList.Add(ActionKeys.UseToolbarPieceHold);
            if (rightTriggerPreviousFrame <= 0.5f && rightTrigger > 0.5f) actionKeyList.Add(ActionKeys.UseToolbarPiecePress);

            if (Keyboard.HasBeenPressed(Keys.Space) ||
                VirtButton.HasButtonBeenPressed(VButName.UseTool)
               ) actionKeyList.Add(ActionKeys.UseToolbarPiecePress);


            if (Keyboard.IsPressed(Keys.Space) ||
               VirtButton.IsButtonDown(VButName.UseTool)
              ) actionKeyList.Add(ActionKeys.UseToolbarPieceHold);

            // pick up

            if (Keyboard.HasBeenPressed(Keys.RightControl) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.X) ||
                VirtButton.HasButtonBeenPressed(VButName.PickUp)
                ) actionKeyList.Add(ActionKeys.PickUp);

            // inventory

            if (Keyboard.HasBeenPressed(Keys.Enter) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Y) ||
                VirtButton.HasButtonBeenPressed(VButName.Inventory)
                ) actionKeyList.Add(ActionKeys.Inventory);

            // equip

            if (Keyboard.HasBeenPressed(Keys.Back) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadLeft) ||
                VirtButton.HasButtonBeenPressed(VButName.Equip)
                ) actionKeyList.Add(ActionKeys.Equip);

            // basic craft 

            if (Keyboard.HasBeenPressed(Keys.NumPad4) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadUp) ||
                VirtButton.HasButtonBeenPressed(VButName.FieldCraft)
               ) actionKeyList.Add(ActionKeys.FieldCraft);

            // run

            if (Keyboard.IsPressed(Keys.NumPad0) ||
                GamePad.IsPressed(playerIndex: PlayerIndex.One, button: Buttons.B) ||
                VirtButton.IsButtonDown(VButName.Run)
                ) actionKeyList.Add(ActionKeys.Run);

            // map

            if (Keyboard.HasBeenPressed(Keys.M) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadRight) ||
                VirtButton.HasButtonBeenPressed(VButName.Map)
                ) actionKeyList.Add(ActionKeys.MapToggle);

            rightTriggerPreviousFrame = rightTrigger;
            ProcessActionKeyList();

            // camera zoom control

            float leftTrigger = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).Triggers.Left;

            float manualScale = 1 - (leftTrigger * 0.15f);
            if (Preferences.zoomedOut || Keyboard.IsPressed(Keys.NumPad1)) manualScale = 2f;

            // shooting mode

            bool triggerPressed = leftTrigger > 0.4f;
            bool rightStickTilted = Math.Abs(analogCameraCorrection.X) > 0.05f || Math.Abs(analogCameraCorrection.Y) > 0.05f;

            if (Preferences.EnableTouch)
            {
                if (rightStickTilted) actionKeyList.Add(ActionKeys.ShootingMode);
            }
            else
            {
                if ((triggerPressed && rightStickTilted) || Keyboard.IsPressed(Keys.Space)) actionKeyList.Add(ActionKeys.ShootingMode);
            }

            return manualScale;
        }

        public Vector2 CalculateMovementFromInput(bool horizontalPriority = true)
        {
            Vector2 movement = new Vector2(0f, 0f);

            if (this.analogMovementLeftStick == Vector2.Zero)
            {
                // movement using keyboard and gamepad buttons

                // X axis movement is bigger than Y, to ensure horizontal orientation priority
                Dictionary<World.ActionKeys, Vector2> movementByDirection;

                if (horizontalPriority)
                {
                    movementByDirection = new Dictionary<World.ActionKeys, Vector2>(){
                        {World.ActionKeys.Up, new Vector2(0f, -500f)},
                        {World.ActionKeys.Down, new Vector2(0f, 500f)},
                        {World.ActionKeys.Left, new Vector2(-500f, 0f)},
                        {World.ActionKeys.Right, new Vector2(500f, 0f)},
                        };
                }
                else
                {
                    movementByDirection = new Dictionary<World.ActionKeys, Vector2>(){
                        {World.ActionKeys.Up, new Vector2(0f, -500f)},
                        {World.ActionKeys.Down, new Vector2(0f, 500f)},
                        {World.ActionKeys.Left, new Vector2(-500f, 0f)},
                        {World.ActionKeys.Right, new Vector2(500f, 0f)},
                        };
                }

                foreach (var kvp in movementByDirection)
                { if (this.actionKeyList.Contains(kvp.Key)) movement += kvp.Value; }
            }
            else
            {
                // analog movement
                movement = this.analogMovementLeftStick;
            }

            //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"movement {movement.X},{movement.Y}", color: Color.Orange); // for testing

            return movement;
        }

        private void ProcessActionKeyList()
        {
            if (this.demoMode || this.CineMode) return;

            if (this.actionKeyList.Contains(ActionKeys.PauseMenu)) MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Pause);
            if (this.SpectatorMode) return;

            if (!this.player.alive || this.player.activeState != BoardPiece.State.PlayerControlledWalking) return;


            if (this.actionKeyList.Contains(ActionKeys.FieldCraft))
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CraftBasic);
                return;
            }

            if (this.actionKeyList.Contains(ActionKeys.Inventory))
            {
                Scene.SetInventoryLayout(Scene.InventoryLayout.InventoryAndToolbar, player: this.player);
                return;
            }

            if (this.actionKeyList.Contains(ActionKeys.Equip))
            {
                Scene.SetInventoryLayout(Scene.InventoryLayout.InventoryAndEquip, player: this.player);
                return;
            }

            if (this.actionKeyList.Contains(ActionKeys.MapToggle)) this.ToggleMapMode();
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
            if (this.player != null) this.ProcessOneNonPlant(this.player); // to ensure processing (even if it is outside of camera rect)
            if (this.spectator != null) this.ProcessOneNonPlant(this.spectator); // to ensure processing (even if it is outside of camera rect)

            if (!this.CanProcessMoreCameraRectPiecesNow)
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Camera view SM: no time to start processing queue - {UpdateTimeElapsed.Milliseconds}ms.");
                return;
            }

            foreach (Sprite sprite in this.grid.GetSpritesInCameraView(camera: this.camera, groupName: Cell.Group.StateMachinesNonPlants))
            {
                if (!this.camera.viewRect.Intersects(sprite.gfxRect)) continue; // returned sprite list is a bit bigger than camera view

                this.ProcessOneNonPlant(sprite.boardPiece);

                if (!this.CanProcessMoreCameraRectPiecesNow)
                {
                    //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Camera view SM: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");
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

            BoardPiece currentAnimal;
            while (true)
            {
                if (animalSpritesQueue.Count == 0) return;

                currentAnimal = this.animalSpritesQueue[0].boardPiece;
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

            float zoomScale = 0.7f;

            this.transManager.AddMultipleTransitions(paramsToChange: new Dictionary<string, float> {
                { "PosX", this.viewParams.PosX - (this.camera.ScreenWidth * zoomScale * 0.25f) },
                { "PosY", this.viewParams.PosY - (this.camera.ScreenHeight * zoomScale * 0.25f) },
                { "ScaleX", this.viewParams.ScaleX * zoomScale },
                { "ScaleY", this.viewParams.ScaleY * zoomScale }},
               duration: 20, outTrans: true, requireInputActiveAtRepeats: true, playCount: 2, refreshBaseVal: true);
        }

        public override void Draw()
        {
            var lightSprites = this.UpdateDarknessMask(); // Has to be called first, because calling SetRenderTarget() after any draw will wipe the screen black.

            // drawing blue background (to ensure drawing even if screen is larger than map)
            SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            if (this.worldCreationInProgress) return;

            // drawing background and sprites
            this.grid.DrawBackground(camera: this.camera);

            // drawing sprites
            var noOfDisplayedSprites = this.grid.DrawSprites(camera: this.camera);

            // drawing grid cells
            if (Preferences.debugShowCellData) this.grid.DrawDebugData();

            // drawing light and darkness
            this.DrawLightAndDarkness(lightSprites);

            // updating debugText
            this.debugText = $"objects {this.PieceCount}, visible {noOfDisplayedSprites}";

            this.currentFrame++;
        }

        private List<Sprite> UpdateDarknessMask()
        {
            // Has to be called first, because calling SetRenderTarget() after any draw will wipe the screen black.

            if (!Preferences.showLighting) return new List<Sprite>();

            Rectangle extendedViewRect = this.camera.ExtendedViewRect;
            Rectangle darknessRect = new Rectangle(x: extendedViewRect.X, y: extendedViewRect.Y,
                width: (SonOfRobinGame.graphics.PreferredBackBufferWidth / 3) + 200, height: (SonOfRobinGame.graphics.PreferredBackBufferHeight / 3) + 200);

            this.darknessMaskScale = Math.Max((float)extendedViewRect.Width / (float)darknessRect.Width, (float)extendedViewRect.Height / (float)darknessRect.Height);

            if (this.darknessMask == null || this.darknessMask.Width != darknessRect.Width || this.darknessMask.Height != darknessRect.Height)
            {
                if (this.darknessMask != null) this.darknessMask.Dispose();
                this.darknessMask = new RenderTarget2D(SonOfRobinGame.graphicsDevice, darknessRect.Width, darknessRect.Height);
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Creating new darknessMask - {darknessMask.Width}x{darknessMask.Height}");
            }

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime);

            var foundSprites = this.grid.GetSpritesInCameraView(camera: camera, groupName: Cell.Group.LightSource);
            var lightSprites = foundSprites.OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom).ToList();

            if (lightSprites.Count > 0)
            {
                SonOfRobinGame.graphicsDevice.SetRenderTarget(this.darknessMask);
                SonOfRobinGame.graphicsDevice.Clear(ambientLightData.darknessColor);

                var blend = new BlendState
                {
                    AlphaBlendFunction = BlendFunction.ReverseSubtract,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.One,

                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.One,
                    ColorBlendFunction = BlendFunction.Add,
                };

                SonOfRobinGame.spriteBatch.End();
                SonOfRobinGame.spriteBatch.Begin(blendState: blend);

                Rectangle lightRect;

                foreach (var sprite in lightSprites)
                {
                    lightRect = sprite.lightEngine.Rect;

                    lightRect.X = (int)(((float)lightRect.X - (float)extendedViewRect.X) / this.darknessMaskScale);
                    lightRect.Y = (int)(((float)lightRect.Y - (float)extendedViewRect.Y) / this.darknessMaskScale);
                    lightRect.Width = (int)((float)lightRect.Width / this.darknessMaskScale);
                    lightRect.Height = (int)((float)lightRect.Height / this.darknessMaskScale);

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSourceBlack, lightRect, Color.White * sprite.lightEngine.Opacity);
                }

                SonOfRobinGame.spriteBatch.End();
                SonOfRobinGame.graphicsDevice.SetRenderTarget(null);
                SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
            }

            return lightSprites;
        }

        private void DrawLightAndDarkness(List<Sprite> lightSprites)
        {
            if (!Preferences.showLighting) return;

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime);
            Rectangle extendedViewRect = this.camera.ExtendedViewRect;

            // drawing ambient light

            SonOfRobinGame.spriteBatch.End();
            BlendState blend = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.One
            };
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Immediate, blendState: blend);
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, extendedViewRect, ambientLightData.lightColor * this.viewParams.drawOpacity);

            // drawing point lights

            SonOfRobinGame.spriteBatch.End();
            blend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
            };

            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Immediate, blendState: blend);

            foreach (var sprite in lightSprites)
            {
                if (sprite.lightEngine.ColorActive)
                {
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSourceWhite, sprite.lightEngine.Rect, sprite.lightEngine.Color);
                }
            }

            // drawing darkness

            SonOfRobinGame.spriteBatch.End();
            SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
            SonOfRobinGame.spriteBatch.Draw(this.darknessMask, new Rectangle(x: extendedViewRect.X, y: extendedViewRect.Y, width: (int)(this.darknessMask.Width * this.darknessMaskScale), height: (int)(this.darknessMask.Height * this.darknessMaskScale)), Color.White);
        }

    }
}
