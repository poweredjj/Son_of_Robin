using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class World : Scene
    {
        public Vector2 analogMovementLeftStick;
        public Vector2 analogMovementRightStick;
        public Vector2 analogCameraCorrection;
        public bool WorldCreationInProgress { get; private set; }

        private bool plantsProcessing;
        public const int buildDuration = (int)(60 * 2.5);
        private const int populatingFramesTotal = 20;
        private int populatingFramesLeft;
        public readonly DateTime creationStart;
        public DateTime creationEnd;
        public TimeSpan creationDuration;
        public readonly bool demoMode;

        private Task task;
        private Object saveGameData;
        public Dictionary<string, BoardPiece> piecesByOldID; // for deserialization only
        public bool createMissingPiecesOutsideCamera;
        public DateTime lastSaved;

        private bool soundPaused;
        public bool BuildMode { get; private set; }

        private bool spectatorMode;
        private bool cineMode;
        public readonly int seed;
        public readonly int resDivider;
        public int maxAnimalsPerName;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Rectangle worldRect;
        public readonly Camera camera;
        private RenderTarget2D darknessMask;
        public readonly Map map;
        public readonly PlayerPanel playerPanel;

        public readonly CraftStats craftStats;
        public readonly KitchenStats cookStats;
        public readonly KitchenStats brewStats;

        public List<PieceTemplate.Name> identifiedPieces; // pieces that were "looked at" in inventory
        private bool mapEnabled;
        public Player Player { get; private set; }
        public HintEngine HintEngine { get; private set; }
        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;
        private readonly List<PieceCreationData> creationDataListRegular;
        private readonly List<PieceCreationData> creationDataListTemporaryDecorations;
        private readonly List<Sprite> temporaryDecorationSprites;
        public List<Cell> plantCellsQueue;
        public List<Sprite> plantSpritesQueue;
        public List<Sprite> nonPlantSpritesQueue;
        public Grid Grid { get; private set; }
        public int CurrentFrame { get; private set; }
        public int CurrentUpdate { get; private set; } // can be used to measure time elapsed on island
        public int updateMultiplier;
        public readonly IslandClock islandClock;
        public readonly Weather weather;
        public readonly WorldEventManager worldEventManager;
        public readonly TrackingManager trackingManager;
        public readonly SolidColorManager solidColorManager;
        public readonly SMTypesManager stateMachineTypesManager;
        public readonly CoolingManager coolingManager;
        private readonly ScrollingSurfaceManager scrollingSurfaceManager;
        public readonly SwayManager swayManager;
        public string debugText;
        public int ProcessedNonPlantsCount { get; private set; }
        public int ProcessedPlantsCount { get; private set; }
        private readonly List<Sprite> blockingLightSpritesList;
        private readonly List<Sprite> lightSprites;
        public List<PieceTemplate.Name> doNotCreatePiecesList;
        public List<PieceTemplate.Name> discoveredRecipesForPieces;
        public readonly DateTime createdTime; // for calculating time spent in game
        private TimeSpan timePlayed; // real time spent while playing (differs from currentUpdate because of island time compression via updateMultiplier)

        public World(int width, int height, int seed, int resDivider, PieceTemplate.Name playerName, Object saveGameData = null, bool demoMode = false) :
            base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.seed = seed;
            this.random = new Random(seed);

            this.demoMode = demoMode;
            this.cineMode = false;
            this.BuildMode = false;
            this.spectatorMode = false;
            if (this.demoMode)
            {
                this.InputType = InputTypes.None;
                this.soundActive = false;
            }
            this.saveGameData = saveGameData;
            this.piecesByOldID = new Dictionary<string, BoardPiece>();
            this.createMissingPiecesOutsideCamera = false;
            this.lastSaved = DateTime.Now;
            this.WorldCreationInProgress = true;
            this.plantsProcessing = false;
            this.populatingFramesLeft = populatingFramesTotal;
            this.creationStart = DateTime.Now;

            if (seed < 0) throw new ArgumentException($"Seed value cannot be negative - {seed}.");

            this.resDivider = resDivider;
            this.CurrentFrame = 0;
            this.CurrentUpdate = 0;
            this.createdTime = DateTime.Now;
            this.TimePlayed = TimeSpan.Zero;
            this.updateMultiplier = 1;
            this.islandClock = this.saveGameData == null ? new IslandClock(0) : new IslandClock();
            this.worldEventManager = new WorldEventManager(this);
            this.trackingManager = new TrackingManager(this);
            this.coolingManager = new CoolingManager(this);
            this.weather = new Weather(world: this, islandClock: this.islandClock);
            this.scrollingSurfaceManager = new ScrollingSurfaceManager(world: this);
            this.swayManager = new SwayManager(this);
            this.width = width;
            this.height = height;
            this.worldRect = new Rectangle(x: 0, y: 0, width: this.width, height: this.height);
            this.viewParams.Width = width; // it does not need to be updated, because world size is constant
            this.viewParams.Height = height; // it does not need to be updated, because world size is constant

            this.maxAnimalsPerName = (int)(this.width * this.height * 0.0000004);
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"maxAnimalsPerName {maxAnimalsPerName}");

            var creationDataList = PieceCreationData.CreateDataList(maxAnimalsPerName: this.maxAnimalsPerName);
            this.creationDataListRegular = creationDataList.Where(data => !data.temporaryDecoration).ToList();
            this.creationDataListTemporaryDecorations = creationDataList.Where(data => data.temporaryDecoration).ToList();

            foreach (PieceCreationData pieceCreationData in this.creationDataListTemporaryDecorations)
            {
                if (PieceInfo.GetInfo(pieceCreationData.name).serialize) throw new ArgumentException($"Serialized piece cannot be temporary - {pieceCreationData.name}.");
            }
            this.temporaryDecorationSprites = new List<Sprite>();

            this.pieceCountByName = new Dictionary<PieceTemplate.Name, int>();
            foreach (PieceTemplate.Name templateName in PieceTemplate.allNames) this.pieceCountByName[templateName] = 0;
            this.pieceCountByClass = new Dictionary<Type, int> { };
            this.HintEngine = new HintEngine(world: this);
            this.plantSpritesQueue = new List<Sprite>();
            this.nonPlantSpritesQueue = new List<Sprite>();
            this.plantCellsQueue = new List<Cell>();
            this.ProcessedNonPlantsCount = 0;
            this.ProcessedPlantsCount = 0;
            this.blockingLightSpritesList = new List<Sprite>();
            this.lightSprites = new List<Sprite>();
            this.doNotCreatePiecesList = new List<PieceTemplate.Name> { };
            this.discoveredRecipesForPieces = new List<PieceTemplate.Name> { };
            this.camera = new Camera(world: this, useWorldScale: true, useFluidMotionForMove: true, useFluidMotionForZoom: true);
            this.camera.TrackCoords(new Vector2(0, 0));
            this.MapEnabled = false;
            this.map = new Map(world: this, touchLayout: TouchLayout.Map);
            this.playerPanel = new PlayerPanel(world: this);
            this.debugText = "";
            if (saveGameData == null)
            {
                this.Player = (Player)PieceTemplate.Create(templateName: playerName, world: this); // temporary boardPiece, to store playerName only
                this.Grid = new Grid(world: this, resDivider: resDivider);
            }
            else this.Deserialize(gridOnly: true);

            this.AddLinkedScene(this.map);
            this.AddLinkedScene(this.playerPanel);

            this.solidColorManager = new SolidColorManager(this);
            this.stateMachineTypesManager = new SMTypesManager(this);
            this.craftStats = new CraftStats();
            this.cookStats = new KitchenStats();
            this.brewStats = new KitchenStats();
            this.identifiedPieces = new List<PieceTemplate.Name> { PieceTemplate.Name.KnifeSimple };
            if (this.demoMode) this.solidColorManager.Add(new SolidColor(color: Color.White, viewOpacity: 0.4f, clearScreen: false, priority: 1));
            this.soundPaused = false;

            SonOfRobinGame.Game.IsFixedTimeStep = false; // speeds up the creation process
        }

        public PieceTemplate.Name PlayerName
        {
            get
            {
                Player player = this.Player;
                return player == null ? PieceTemplate.Name.Empty : this.Player.name;
            }
        }

        public bool PopulatingInProgress
        { get { return this.populatingFramesLeft > 0; } }

        public bool MapEnabled
        {
            get { return this.mapEnabled; }
            set
            {
                if (this.mapEnabled == value) return;
                this.mapEnabled = value;

                if (this.mapEnabled) this.ToggleMapMode();
                else this.map.TurnOff();
            }
        }

        public Sound DialogueSound
        { get { return this.Player.soundPack.GetSound(PieceSoundPack.Action.PlayerSpeak); } }

        public bool CineMode
        {
            get { return cineMode; }
            set
            {
                cineMode = value;

                if (cineMode)
                {
                    this.touchLayout = TouchLayout.Empty; // CineSkip should not be used here, because World class cannot execute the skip properly
                    this.tipsLayout = ControlTips.TipsLayout.Empty;
                    this.InputType = InputTypes.None;

                    new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null);
                    this.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player), typeof(AmbientSound), typeof(VisualEffect) }, everyFrame: true, nthFrame: true);

                    this.Player.activeState = BoardPiece.State.PlayerControlledByCinematic;
                    this.Player.pointWalkTarget = Vector2.Zero;
                    this.Player.sprite.CharacterStand(); // to avoid playing walk animation while standing
                }
                else
                {
                    this.touchLayout = TouchLayout.WorldMain;
                    this.tipsLayout = ControlTips.TipsLayout.WorldMain;
                    this.InputType = InputTypes.Normal;

                    this.Player.activeState = BoardPiece.State.PlayerControlledWalking;
                    this.Player.pointWalkTarget = Vector2.Zero;

                    this.solidColorManager.RemoveAll(0);

                    this.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 1f);
                    this.camera.TrackPiece(this.Player);

                    new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null);
                }
            }
        }

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
                    Sound.QuickPlay(SoundData.Name.AngelChorus);

                    this.Player.RemoveFromStateMachines();

                    BoardPiece spectator = PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.camera.TrackedPos, templateName: PieceTemplate.Name.PlayerGhost, closestFreeSpot: true);
                    spectator.sprite.AssignNewPackage(this.Player.sprite.animPackage);

                    spectator.sprite.orientation = this.Player != null ? this.Player.sprite.orientation : Sprite.Orientation.right;

                    bool isTestDemoness = this.PlayerName == PieceTemplate.Name.PlayerTestDemoness;

                    this.Player = (Player)spectator;

                    this.camera.TrackPiece(this.Player);
                    this.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 0.1f);

                    this.tipsLayout = ControlTips.TipsLayout.WorldSpectator;
                    this.touchLayout = TouchLayout.WorldSpectator;

                    string text;

                    if (isTestDemoness)
                    {
                        text = "How can this be? I was supposed to be immortal!";
                    }
                    else
                    {
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
                        "I was trying to escape this island, not this life.\nDamn..."
                    };
                        text = textList[this.random.Next(0, textList.Count)];
                    }

                    new TextWindow(text: text, textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 60 * 6, priority: 1, animSound: this.DialogueSound);
                }
                else
                {
                    BoardPiece spectator = this.Player;

                    bool playerFound = false;
                    foreach (Sprite sprite in this.Grid.GetSpritesFromAllCells(Cell.Group.All))
                    {
                        if (PieceInfo.IsPlayer(sprite.boardPiece.name) && sprite.boardPiece.alive)
                        {
                            this.Player = (Player)sprite.boardPiece;
                            this.Player.AddToStateMachines();

                            playerFound = true;
                            break;
                        }
                    }

                    if (!playerFound)
                    {
                        this.CreateAndPlacePlayer();
                        this.Player.sprite.MoveToClosestFreeSpot(this.camera.TrackedPos);
                        this.camera.TrackPiece(this.Player);
                        if (spectator != null) this.Player.sprite.orientation = spectator.sprite.orientation;
                    }

                    this.camera.TrackPiece(this.Player);

                    spectator.Destroy();

                    this.tipsLayout = ControlTips.TipsLayout.WorldMain;
                    this.touchLayout = TouchLayout.WorldMain;

                    new TextWindow(text: "I live... Again!", textColor: Color.White, bgColor: Color.Black, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 60 * 3, priority: 1, animSound: this.DialogueSound);
                }
            }
        }

        public TimeSpan TimePlayed
        {
            get
            { return timePlayed + (DateTime.Now - this.createdTime); }
            set { timePlayed = value; }
        }

        public bool CanProcessMoreCameraRectPiecesNow
        { get { return UpdateTimeElapsed.Milliseconds <= 13 * this.updateMultiplier; } }

        public bool CanProcessMoreNonPlantsNow
        { get { return UpdateTimeElapsed.Milliseconds <= 7 * this.updateMultiplier; } }

        public bool CanProcessMorePlantsNow
        { get { return UpdateTimeElapsed.Milliseconds <= 9 * this.updateMultiplier; } }

        public bool CanProcessAnyStateMachineNow
        { get { return !this.plantsProcessing || UpdateTimeElapsed.Milliseconds <= 9 * this.updateMultiplier; } }

        public float PieceCount
        {
            get
            {
                int pieceCount = 0;
                foreach (PieceTemplate.Name templateName in PieceTemplate.allNames)
                {
                    pieceCount += this.pieceCountByName[templateName];
                }
                return pieceCount;
            }
        }

        private Vector2 DarknessMaskScale
        {
            get
            {
                Rectangle extendedViewRect = this.camera.ExtendedViewRect;

                int maxDarknessWidth = Math.Min((int)(SonOfRobinGame.GfxDevMgr.PreferredBackBufferWidth * 1.2f), 2000) / Preferences.DarknessResolution;
                int maxDarknessHeight = Math.Min((int)(SonOfRobinGame.GfxDevMgr.PreferredBackBufferHeight * 1.2f), 1500) / Preferences.DarknessResolution;

                return new Vector2((float)extendedViewRect.Width / (float)maxDarknessWidth, (float)extendedViewRect.Height / (float)maxDarknessHeight);
            }
        }

        public void CompleteCreation()
        {
            if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                SonOfRobinGame.Game.IsFixedTimeStep = Preferences.FrameSkip;

                SonOfRobinGame.FullScreenProgressBar.TurnOff();

                bool menuFound = GetTopSceneOfType(typeof(Menu)) != null;

                this.Remove();
                new TextWindow(text: "Entering the island has been cancelled.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, closingTask: menuFound ? Scheduler.TaskName.Empty : Scheduler.TaskName.OpenMainMenu);
                return;
            };

            if (this.Grid.CreationInProgress)
            {
                SonOfRobinGame.Game.IsFixedTimeStep = false; // speeds up the creation process
                this.Grid.ProcessNextCreationStage();
                return;
            }

            if (this.saveGameData == null && this.PopulatingInProgress)
            {
                if (this.demoMode)
                {
                    CreateMissingPieces(initialCreation: true, outsideCamera: false, multiplier: 1f);
                    this.populatingFramesLeft = 0;
                }
                else
                {
                    float percentage = FullScreenProgressBar.CalculatePercentage(currentLocalStep: populatingFramesTotal - this.populatingFramesLeft, totalLocalSteps: populatingFramesTotal, currentGlobalStep: 1 + Grid.allStagesCount, totalGlobalSteps: SonOfRobinGame.enteringIslandGlobalSteps);

                    string detailedInfo = Preferences.progressBarShowDetails ? "populating..." : null;

                    SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: LoadingTips.GetTip(), optionalText: detailedInfo);

                    if (this.task == null || this.task.IsCompleted) this.task = Task.Run(() => this.ProcessNextPopulatingStep());
                    return;
                }
            }

            this.task = null;

            SonOfRobinGame.FullScreenProgressBar.TurnOff();
            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.WorldCreationInProgress = false;
            this.creationEnd = DateTime.Now;
            this.creationDuration = this.creationEnd - this.creationStart;
            this.lastSaved = DateTime.Now;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"World creation time: {creationDuration:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

            bool newGameStarted = this.saveGameData == null;

            if (newGameStarted)
            {
                this.CurrentFrame = 0;
                this.CurrentUpdate = 0;

                if (this.demoMode) this.camera.TrackLiveAnimal(fluidMotion: false);
                else
                {
                    this.CreateAndPlacePlayer();

                    if (this.PlayerName != PieceTemplate.Name.PlayerTestDemoness) PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.Player.sprite.position, templateName: PieceTemplate.Name.CrateStarting, closestFreeSpot: true);
                    PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.Player.sprite.position, templateName: PieceTemplate.Name.PredatorRepellant, closestFreeSpot: true);
                }
            }
            else this.Deserialize(gridOnly: false);

            if (!this.demoMode)
            {
                this.camera.TrackPiece(trackedPiece: this.Player, moveInstantly: true);
                this.UpdateViewParams();
                this.camera.Update(cameraCorrection: Vector2.Zero); // to render cells in camera view correctly
                Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.Player);
            }

            this.CreateNewDarknessMask();
            this.Grid.LoadAllTexturesInCameraView();
            if (!this.demoMode) this.map.ForceRender();
            this.CreateTemporaryDecorations(ignoreDuration: true);

            SonOfRobinGame.Game.IsFixedTimeStep = Preferences.FrameSkip;

            if (!this.demoMode && newGameStarted) this.HintEngine.ShowGeneralHint(type: HintEngine.Type.CineIntroduction, ignoreDelay: true);
            GC.Collect();
        }

        private void ProcessNextPopulatingStep()
        {
            bool piecesCreated = CreateMissingPieces(initialCreation: true, maxAmountToCreateAtOnce: (uint)(300000 / populatingFramesTotal), outsideCamera: false, multiplier: 1f, addToDoNotCreateList: false);
            if (!piecesCreated) this.populatingFramesLeft = 0;

            this.populatingFramesLeft--;
        }

        public override void Remove()
        {
            this.Grid.Destroy();
            Sound.StopAll();
            base.Remove();
        }

        private void Deserialize(bool gridOnly)
        {
            var saveGameDataDict = (Dictionary<string, Object>)this.saveGameData;

            // deserializing grid
            {
                if (gridOnly) // grid has to be deserialized first
                {
                    var gridData = (Dictionary<string, Object>)saveGameDataDict["grid"];
                    this.Grid = Grid.Deserialize(world: this, gridData: gridData, resDivider: this.resDivider);
                    return;
                }
            }

            // deserializing header
            {
                var headerData = (Dictionary<string, Object>)saveGameDataDict["header"];

                this.CurrentFrame = (int)(Int64)headerData["currentFrame"];
                this.CurrentUpdate = (int)(Int64)headerData["currentUpdate"];
                this.islandClock.Initialize((int)(Int64)headerData["clockTimeElapsed"]);
                this.TimePlayed = TimeSpan.Parse((string)headerData["TimePlayed"]);
                this.mapEnabled = (bool)headerData["MapEnabled"];
                this.maxAnimalsPerName = (int)(Int64)headerData["maxAnimalsPerName"];
                this.doNotCreatePiecesList = (List<PieceTemplate.Name>)headerData["doNotCreatePiecesList"];
                this.discoveredRecipesForPieces = (List<PieceTemplate.Name>)headerData["discoveredRecipesForPieces"];
                this.stateMachineTypesManager.Deserialize((Dictionary<string, Object>)headerData["stateMachineTypesManager"]);
                this.craftStats.Deserialize((Dictionary<string, Object>)headerData["craftStats"]);
                this.cookStats.Deserialize((Dictionary<string, Object>)headerData["cookStats"]);
                this.brewStats.Deserialize((Dictionary<string, Object>)headerData["brewStats"]);
                this.identifiedPieces = (List<PieceTemplate.Name>)headerData["identifiedPieces"];
            }

            // deserializing hints
            {
                var hintsData = (Dictionary<string, Object>)saveGameDataDict["hints"];
                this.HintEngine.Deserialize(hintsData);
            }

            // deserializing weather
            {
                var weatherData = (Dictionary<string, Object>)saveGameDataDict["weather"];
                this.weather.Deserialize(weatherData);
            }

            // deserializing pieces
            {
                var pieceDataBag = (ConcurrentBag<Object>)saveGameDataDict["pieces"];

                foreach (Dictionary<string, Object> pieceData in pieceDataBag)
                {
                    // repeated in StorageSlot

                    PieceTemplate.Name templateName = (PieceTemplate.Name)(Int64)pieceData["base_name"];

                    var spriteData = (Dictionary<string, Object>)pieceData["base_sprite"];
                    int spritePosX = (int)(Int64)spriteData["posX"];
                    int spritePosY = (int)(Int64)spriteData["posY"];

                    var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this, position: new Vector2(spritePosX, spritePosY), templateName: templateName, ignoreCollisions: true, id: (string)pieceData["base_id"]);
                    if (!newBoardPiece.sprite.IsOnBoard) throw new ArgumentException($"{newBoardPiece.name} could not be placed correctly.");

                    newBoardPiece.Deserialize(pieceData: pieceData);

                    if (PieceInfo.IsPlayer(templateName))
                    {
                        this.Player = (Player)newBoardPiece;
                        this.camera.TrackPiece(trackedPiece: this.Player, moveInstantly: true);
                    }
                }

                // deserializing tracking

                var trackingDataList = (List<Object>)saveGameDataDict["tracking"];
                this.trackingManager.Deserialize(trackingDataList);

                // deserializing planned events

                var eventDataList = (List<Object>)saveGameDataDict["events"];
                this.worldEventManager.Deserialize(eventDataList);

                // deserializing cooling data

                var coolingData = (Dictionary<string, Object>)saveGameDataDict["cooling"];
                this.coolingManager.Deserialize(coolingData);

                // removing not needed data

                this.piecesByOldID = null; // not needed anymore, "null" will make any attempt to access it afterwards crash the game
            }

            // finalizing

            this.saveGameData = null;
        }

        private void CreateAndPlacePlayer()
        {
            for (int tryIndex = 0; tryIndex < 65535; tryIndex++)
            {
                PieceTemplate.Name playerName = this.PlayerName; // taken from a temporary Player boardPiece

                this.Player = (Player)PieceTemplate.CreateAndPlaceOnBoard(world: this, randomPlacement: true, position: Vector2.Zero, templateName: playerName);

                if (this.Player.sprite.IsOnBoard)
                {
                    this.Player.sprite.orientation = Sprite.Orientation.up;
                    this.Player.sprite.CharacterStand();
                    this.Player.sprite.allowedTerrain.RemoveTerrain(Terrain.Name.Biome); // player should be spawned in a safe place, but able to go everywhere afterwards
                    this.Player.sprite.allowedTerrain.ClearExtProperties();

                    switch (playerName)
                    {
                        // adding character bonuses, that would not be possible to add in PieceTemplate.Create()

                        case PieceTemplate.Name.PlayerBoy:
                            // bonuses creation placeholder
                            break;

                        case PieceTemplate.Name.PlayerGirl:
                            // bonuses creation placeholder
                            break;

                        case PieceTemplate.Name.PlayerTestDemoness:
                            var pieceNamesToEquip = new List<PieceTemplate.Name> {
                                PieceTemplate.Name.BootsProtective, PieceTemplate.Name.Map, PieceTemplate.Name.BackpackBig, PieceTemplate.Name.BeltBig, PieceTemplate.Name.HatSimple, PieceTemplate.Name.Dungarees
                            };

                            foreach (PieceTemplate.Name name in pieceNamesToEquip)
                            {
                                BoardPiece piece = PieceTemplate.Create(world: this, templateName: name);
                                this.Player.EquipStorage.AddPiece(piece);
                            }

                            this.map.TurnOff();

                            var pieceNamesForToolbar = new List<PieceTemplate.Name> {PieceTemplate.Name.AxeCrystal, PieceTemplate.Name.PickaxeCrystal, PieceTemplate.Name.SpearCrystal, PieceTemplate.Name.TorchBig, PieceTemplate.Name.ScytheCrystal, PieceTemplate.Name.ShovelCrystal,  PieceTemplate.Name.BowAdvanced
                            };

                            foreach (PieceTemplate.Name name in pieceNamesForToolbar)
                            {
                                BoardPiece piece = PieceTemplate.Create(world: this, templateName: name);
                                this.Player.ToolStorage.AddPiece(piece);
                            }

                            for (int i = 0; i < 50; i++)
                            {
                                BoardPiece piece = PieceTemplate.Create(world: this, templateName: PieceTemplate.Name.ArrowBurning);
                                this.Player.ToolStorage.AddPiece(piece);
                            }

                            foreach (Cell cell in this.Grid.allCells)
                            {
                                cell.SetAsVisited();
                            }

                            var piecesForInventoryWithCount = new Dictionary<PieceTemplate.Name, int>();

                            foreach (PieceTemplate.Name name in PieceTemplate.allNames)
                            {
                                Craft.Recipe recipe = Craft.GetRecipe(name);
                                if (recipe != null)
                                {
                                    foreach (var kvp in recipe.ingredients)
                                    {
                                        PieceTemplate.Name ingredientName = kvp.Key;
                                        int ingredientCount = kvp.Value;

                                        if (!piecesForInventoryWithCount.ContainsKey(ingredientName) || piecesForInventoryWithCount[ingredientName] < ingredientCount)
                                        {
                                            piecesForInventoryWithCount[ingredientName] = ingredientCount;
                                        }
                                    }
                                }
                            }

                            foreach (var kvp in piecesForInventoryWithCount)
                            {
                                PieceTemplate.Name name = kvp.Key;
                                int count = kvp.Value;

                                bool pieceAdded = false;
                                for (int i = 0; i < count; i++)
                                {
                                    BoardPiece piece = PieceTemplate.Create(world: this, templateName: name);
                                    pieceAdded = this.Player.PieceStorage.AddPiece(piece);
                                    if (!pieceAdded) break;
                                }
                                if (!pieceAdded) break;
                            }

                            foreach (PieceStorage storage in this.Player.CraftStoragesToPutInto)
                            {
                                foreach (BoardPiece piece in storage.GetAllPieces())
                                {
                                    if (!this.identifiedPieces.Contains(piece.name)) this.identifiedPieces.Add(piece.name);
                                }
                            }

                            break;

                        default:
                            break;
                    }

                    return;
                }
            }

            throw new DivideByZeroException("Cannot place player sprite.");
        }

        public bool CreateMissingPieces(bool initialCreation, uint maxAmountToCreateAtOnce = 300000, bool outsideCamera = false, float multiplier = 1.0f, bool clearDoNotCreateList = false, bool addToDoNotCreateList = true)
        {
            if (clearDoNotCreateList) doNotCreatePiecesList.Clear();
            if (!initialCreation && !this.CanProcessMorePlantsNow) return false;

            int minPieceAmount = Math.Max(Convert.ToInt32((long)width * (long)height / 300000 * multiplier), 0); // 300000
            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (PieceCreationData creationData in creationDataListRegular)
            {
                if (doNotCreatePiecesList.Contains(creationData.name)) continue;

                int minAmount = Math.Max((int)(minPieceAmount * creationData.multiplier), 4);
                if (creationData.maxAmount > -1) minAmount = Math.Min(minAmount, creationData.maxAmount);

                int amountToCreate = Math.Max(minAmount - this.pieceCountByName[creationData.name], 0);
                if (amountToCreate > 0) amountToCreateByName[creationData.name] = amountToCreate;
            }

            if (!amountToCreateByName.Keys.Any()) return false;

            // creating pieces

            this.createMissingPiecesOutsideCamera = outsideCamera;
            int piecesCreated = 0;

            foreach (var kvp in amountToCreateByName)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                int amountLeftToCreate = kvp.Value; // separate from the iterator below (needs to be modified)

                for (int i = 0; i < kvp.Value * 5; i++)
                {
                    if (!initialCreation && !this.CanProcessMorePlantsNow) return false;

                    var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this, randomPlacement: true, position: Vector2.Zero, templateName: pieceName);
                    if (newBoardPiece.sprite.IsOnBoard)
                    {
                        if (initialCreation && newBoardPiece.GetType() == typeof(Plant) && this.random.Next(2) == 0)
                        {
                            Plant newPlant = (Plant)newBoardPiece;
                            newPlant.SimulateUnprocessedGrowth();
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
            this.createMissingPiecesOutsideCamera = false;
            return piecesCreated > 0;
        }

        private void CreateTemporaryDecorations(bool ignoreDuration)
        {
            if (!ignoreDuration && !this.CanProcessMoreNonPlantsNow) return;

            DateTime creationStarted = DateTime.Now;
            int createdDecorationsCount = 0;

            foreach (Cell cell in this.Grid.GetCellsInsideRect(viewRect: camera.viewRect, addPadding: true)
                .Where(cell => !cell.temporaryDecorationsCreated)
                .OrderBy(x => this.random.Next()))
            {
                foreach (PieceCreationData pieceCreationData in this.creationDataListTemporaryDecorations)
                {
                    if (cell.allowedNames.Contains(pieceCreationData.name))
                    {
                        for (int count = 0; count < pieceCreationData.tempDecorMultiplier; count++)
                        {
                            for (int tryNo = 0; tryNo < 3; tryNo++)
                            {
                                Vector2 randomPosition = new Vector2(this.random.Next(cell.xMin, cell.xMax), this.random.Next(cell.yMin, cell.yMax));

                                var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(templateName: pieceCreationData.name, world: this, position: randomPosition);
                                if (newBoardPiece.sprite.IsOnBoard)
                                {
                                    this.temporaryDecorationSprites.Add(newBoardPiece.sprite);
                                    newBoardPiece.isTemporaryDecoration = true;
                                    createdDecorationsCount++;
                                    break;
                                }

                                if (!ignoreDuration && !this.CanProcessMoreNonPlantsNow)
                                {
                                    this.ShowTempDecorsMessage(createdDecorationsCount: createdDecorationsCount, creationStarted: creationStarted, completed: false);
                                    return;
                                }
                            }
                        }
                    }
                }

                cell.temporaryDecorationsCreated = true;
            }

            this.ShowTempDecorsMessage(createdDecorationsCount: createdDecorationsCount, creationStarted: creationStarted, completed: true);
        }

        private void ShowTempDecorsMessage(int createdDecorationsCount, DateTime creationStarted, bool completed)
        {
            if (createdDecorationsCount > 0)
            {
                TimeSpan tempDecorCreationDuration = DateTime.Now - creationStarted;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Temp decors created: {createdDecorationsCount} total: {this.temporaryDecorationSprites.Count} duration: {tempDecorCreationDuration:\\:ss\\.fff} completed: {completed}");
            }
        }

        private void DestroyTemporaryDecorationsOutsideCamera()
        {
            if (!this.CanProcessMoreNonPlantsNow) return;

            DateTime creationStarted = DateTime.Now;
            int destroyedDecorationsCount = 0;

            Rectangle extendedCameraRect = this.camera.viewRect;
            extendedCameraRect.Inflate(this.camera.viewRect.Width, this.camera.viewRect.Height); // to destroying newly created pieces around camera edge

            foreach (Sprite sprite in this.temporaryDecorationSprites.ToList())
            {
                if (!extendedCameraRect.Contains(sprite.position) && sprite.boardPiece.exists)
                {
                    if (sprite.currentCell != null) sprite.currentCell.temporaryDecorationsCreated = false;
                    sprite.boardPiece.Destroy();
                    this.temporaryDecorationSprites.Remove(sprite);
                    destroyedDecorationsCount++;
                }

                if (!this.CanProcessMoreNonPlantsNow) break;
            }

            if (destroyedDecorationsCount > 0)
            {
                TimeSpan tempDecorDestroyDuration = DateTime.Now - creationStarted;
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Temp decors destroyed: {destroyedDecorationsCount} duration: {tempDecorDestroyDuration:\\:ss\\.fff}");
            }
            return;
        }

        public void UpdateViewParams()
        {
            this.camera.Update(cameraCorrection: this.analogCameraCorrection);
            this.camera.SetViewParams(this);

            // width and height are set once in constructor
        }

        public override void Update(GameTime gameTime)
        {
            if (this.WorldCreationInProgress)
            {
                this.CompleteCreation();
                return;
            }

            if (this.demoMode) this.camera.SetZoom(zoom: 2f, setInstantly: true);

            this.ProcessInput();
            this.UpdateViewParams();
            this.weather.Update();
            this.swayManager.Update();

            if (this.soundPaused && this.inputActive)
            {
                ManagedSoundInstance.ResumeAll();
                this.soundPaused = false;
            }

            this.Grid.UnloadTexturesIfMemoryLow(this.camera);
            this.Grid.LoadClosestTexturesInCameraView(camera: this.camera, visitedByPlayerOnly: false, loadMoreThanOne: false);

            this.scrollingSurfaceManager.Update(this.weather.FogPercentage > 0);

            if (this.demoMode) this.camera.TrackLiveAnimal(fluidMotion: true);

            bool createMissingPieces = this.CurrentUpdate % 200 == 0 && Preferences.debugCreateMissingPieces && !this.CineMode && !this.BuildMode;
            if (createMissingPieces) this.CreateMissingPieces(initialCreation: false, maxAmountToCreateAtOnce: 100, outsideCamera: true, multiplier: 0.1f);

            if (!createMissingPieces && this.CurrentUpdate % 31 == 0)
            {
                this.DestroyTemporaryDecorationsOutsideCamera();
                this.CreateTemporaryDecorations(ignoreDuration: false);
            }

            for (int i = 0; i < this.updateMultiplier; i++)
            {
                this.trackingManager.ProcessQueue();
                this.worldEventManager.ProcessQueue();
                this.coolingManager.Update();
                if (!this.BuildMode) this.UpdateAllAnims();

                if (this.Player != null)
                {
                    this.ProcessOneNonPlant(this.Player);
                    this.Player.UpdateDistanceWalked();
                    this.Player.UpdateLastSteps();
                }
                if (this.map.MapMarker != null && this.map.MapMarker.sprite.IsOnBoard) this.ProcessOneNonPlant(this.map.MapMarker);

                this.StateMachinesProcessCameraView();

                if (!createMissingPieces && (!Preferences.Cap30FPS || this.CurrentFrame % 2 == 0))
                {
                    this.StateMachinesProcessNonPlantQueue();
                    this.plantsProcessing = true;
                    this.StateMachinesProcessPlantQueue();
                    this.plantsProcessing = false;
                }

                this.CurrentUpdate++;
                this.islandClock.Advance();
            }
        }

        private void ProcessInput()
        {
            if (this.demoMode || this.CineMode) return;

            // analog movement (left stick)
            this.analogMovementLeftStick = InputMapper.Analog(InputMapper.Action.WorldWalk);
            this.analogMovementLeftStick *= 4;

            // analog movement (right stick)
            this.analogMovementRightStick = InputMapper.Analog(InputMapper.Action.WorldCameraMove);
            this.analogMovementRightStick *= 4;

            // analog camera control
            if (this.Player.activeState != BoardPiece.State.PlayerControlledSleep)
            {
                this.analogCameraCorrection = InputMapper.Analog(InputMapper.Action.WorldCameraMove);
                this.analogCameraCorrection *= 10;
            }

            float zoomOutForce = InputMapper.TriggerForce(InputMapper.Action.WorldCameraZoomOut);

            // camera zoom control (to keep the current zoom level, when other scene is above the world, that scene must block updates below)
            if (!Preferences.CanZoomOut)
            {
                // zoomOutForce combined with worldScale is considered cheating and therefore not allowed

                if (zoomOutForce > 0.4f) this.HintEngine.ShowGeneralHint(type: HintEngine.Type.ZoomOutLocked, ignoreDelay: true); // to avoid showing the message at minimal (accidental, unintended) trigger pressure

                zoomOutForce = 0;
            }
            else VirtButton.ButtonHighlightOnNextFrame(VButName.ZoomOut);

            this.camera.SetZoom(zoom: 1f / (1f + zoomOutForce), zoomSpeedMultiplier: 3f);
        }

        public void ToggleMapMode()
        {
            if (!this.MapEnabled)
            {
                if (!this.HintEngine.ShowGeneralHint(HintEngine.Type.MapNegative)) new TextWindow(text: "I don't have map equipped.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.DialogueSound);
                return;
            }

            if (!this.map.CheckIfPlayerCanReadTheMap(showMessage: true)) return;

            if (this.map.transManager.HasAnyTransition || this.map.transManager.HasAnyTransition) return;

            this.map.SwitchToNextMode();
        }

        public static World GetTopWorld()
        {
            var worldScene = GetTopSceneOfType(typeof(World));
            return worldScene == null ? null : (World)worldScene;
        }

        private void StateMachinesProcessCameraView()
        {
            if (!this.CanProcessMoreCameraRectPiecesNow)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Camera view SM: no time to start processing queue - {UpdateTimeElapsed.Milliseconds}ms.");
                return;
            }

            foreach (Sprite sprite in this.camera.GetVisibleSprites(groupName: Cell.Group.StateMachinesNonPlants, compareWithCameraRect: true))
            {
                this.ProcessOneNonPlant(sprite.boardPiece);

                if (!this.CanProcessMoreCameraRectPiecesNow)
                {
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Camera view SM: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");
                    return;
                }
            }
        }

        private void StateMachinesProcessNonPlantQueue()
        {
            if (!this.CanProcessMoreNonPlantsNow) return;
            if ((SonOfRobinGame.LastUpdateDelay > 20 || SonOfRobinGame.LastDrawDelay > 20) && this.updateMultiplier == 1) return;

            if (this.nonPlantSpritesQueue.Count == 0)
            {
                this.nonPlantSpritesQueue = this.Grid.GetSpritesFromAllCells(groupName: Cell.Group.StateMachinesNonPlants).OrderBy(sprite => sprite.boardPiece.lastFrameSMProcessed).ToList();
                return;
            }

            this.ProcessedNonPlantsCount = 0;

            while (true)
            {
                if (nonPlantSpritesQueue.Count == 0) return;

                BoardPiece currentNonPlant = this.nonPlantSpritesQueue[0].boardPiece;
                this.nonPlantSpritesQueue.RemoveAt(0);

                //if (currentNonPlant.name != PieceTemplate.Name.Player) MessageLog.AddMessage(msgType: MsgType.User, message: $"{this.currentUpdate} processing '{currentNonPlant.readableName}'");

                this.ProcessOneNonPlant(currentNonPlant);
                this.ProcessedNonPlantsCount++;

                if (!this.CanProcessMoreNonPlantsNow)
                {
                    if (UpdateTimeElapsed.Milliseconds > 13 && this.updateMultiplier == 1) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Non plants: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");

                    return;
                }
            }
        }

        private void ProcessOneNonPlant(BoardPiece piece)
        {
            if (piece.maxAge > 0) piece.GrowOlder(timeDelta: 1);

            if (piece.GetType() == typeof(Animal))
            {
                Animal nonPlant = (Animal)piece;
                if (nonPlant.isPregnant && nonPlant.alive) nonPlant.pregnancyFramesLeft = Math.Max(nonPlant.pregnancyFramesLeft - 1, 0);
                if (this.CurrentUpdate % 10 == 0) nonPlant.ExpendEnergy(1);

                if (nonPlant.alive && (nonPlant.HitPoints <= 0 || nonPlant.efficiency == 0 || nonPlant.currentAge >= nonPlant.maxAge))
                {
                    nonPlant.Kill();
                    return;
                }
            }
            else if (piece.GetType() == typeof(Player))
            {
                Player player = (Player)piece;
                if (player.HitPoints <= 0 && player.alive)
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
            if ((SonOfRobinGame.LastUpdateDelay > 20 || SonOfRobinGame.LastDrawDelay > 20) && this.updateMultiplier == 1) return;

            this.ProcessedPlantsCount = 0;

            if (!this.plantCellsQueue.Any())
            {
                this.plantCellsQueue = new List<Cell>(this.Grid.allCells.ToList());
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

            while (true)
            {
                if (plantSpritesQueue.Count == 0) return;

                Plant currentPlant = (Plant)this.plantSpritesQueue[0].boardPiece;
                this.plantSpritesQueue.RemoveAt(0);

                if (currentPlant.sprite.IsInCameraRect && !Preferences.debugShowPlantGrowthInCamera) continue;

                currentPlant.StateMachineWork();
                if (currentPlant.currentAge >= currentPlant.maxAge || currentPlant.efficiency < 0.2 || currentPlant.Mass < 1) currentPlant.Kill();
                this.ProcessedPlantsCount++;

                if (!this.CanProcessMorePlantsNow)
                {
                    if (UpdateTimeElapsed.Milliseconds > 13 && this.updateMultiplier == 1) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Plants: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");
                    return;
                }
            }
        }

        public void EnterBuildMode(Craft.Recipe recipe)
        {
            if (this.BuildMode) throw new ArgumentException("Is already in build mode.");

            this.BuildMode = true;

            this.touchLayout = TouchLayout.WorldBuild;
            this.tipsLayout = ControlTips.TipsLayout.WorldBuild;
            this.Player.activeState = BoardPiece.State.PlayerControlledBuilding;

            this.stateMachineTypesManager.DisableMultiplier();
            this.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player), typeof(AmbientSound), typeof(VisualEffect) }, everyFrame: true, nthFrame: true);
            this.islandClock.Pause();

            Scene craftMenu = GetTopSceneOfType(typeof(Menu));
            craftMenu?.MoveToBottom(); // there is no craft menu if planting
            SonOfRobinGame.HintWindow.TurnOff();

            this.Player.recipeToBuild = recipe;
            this.Player.simulatedPieceToBuild = PieceTemplate.CreateAndPlaceOnBoard(templateName: recipe.pieceToCreate, world: this, position: this.Player.sprite.position, ignoreCollisions: true); // "template" piece should be placed - collisions doesn't matter...
            this.Player.simulatedPieceToBuild.sprite.MoveToClosestFreeSpot(this.Player.sprite.position); // ...but the starting position should be correct for building, if possible
            this.Player.buildDurationForOneFrame = recipe.GetRealDuration(craftStats: this.craftStats, player: this.Player) / buildDuration;
            this.Player.buildFatigueForOneFrame = recipe.GetRealFatigue(craftStats: this.craftStats, player: this.Player) / buildDuration;
        }

        public void BuildPiece()
        {
            if (!this.BuildMode) throw new ArgumentException("Is not in build mode.");

            this.touchLayout = TouchLayout.Empty;
            this.tipsLayout = ControlTips.TipsLayout.Empty;
            this.Player.activeState = BoardPiece.State.PlayerWaitForBuilding;

            BoardPiece builtPiece = this.Player.recipeToBuild.TryToProducePieces(player: this.Player, showMessages: false)[0];

            bool plantMode = builtPiece.GetType() == typeof(Plant);

            Yield debrisYield = new Yield(boardPiece: builtPiece, debrisType: plantMode ? Yield.DebrisType.Leaf : Yield.DebrisType.Star);

            if (plantMode)
            {
                Sound.QuickPlay(SoundData.Name.Planting);
            }
            else
            {
                Sound.QuickPlay(SoundData.Name.Sawing);
                Sound.QuickPlay(SoundData.Name.Hammering);
            }

            this.Player.simulatedPieceToBuild.Destroy();
            this.Player.simulatedPieceToBuild = null;

            builtPiece.sprite.opacity = 0f;
            new OpacityFade(sprite: builtPiece.sprite, destOpacity: 1f, duration: buildDuration);

            new WorldEvent(eventName: WorldEvent.EventName.FinishBuilding, world: this, delay: buildDuration, boardPiece: null);
            new WorldEvent(eventName: WorldEvent.EventName.PlaySoundByName, world: this, delay: buildDuration, boardPiece: null, eventHelper: plantMode ? SoundData.Name.MovingPlant : SoundData.Name.Chime);
            new WorldEvent(eventName: WorldEvent.EventName.YieldDropDebris, world: this, delay: buildDuration, boardPiece: null, eventHelper: debrisYield);
        }

        public void ExitBuildMode(bool restoreCraftMenu, bool showCraftMessages = false)
        {
            if (!this.BuildMode) throw new ArgumentException("Is not in build mode.");

            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.Player.activeState = BoardPiece.State.PlayerControlledWalking;

            Player.world.stateMachineTypesManager.DisableMultiplier();
            Player.world.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
            this.islandClock.Resume();

            Scene craftMenu = GetBottomSceneOfType(typeof(Menu));
            if (restoreCraftMenu) craftMenu?.MoveToTop(); // there is no craft menu if planting
            else craftMenu?.Remove();

            if (showCraftMessages) this.Player.recipeToBuild.UnlockNewRecipesAndShowSummary(this);

            this.Player.recipeToBuild = null;
            if (this.Player.simulatedPieceToBuild != null)
            {
                this.Player.simulatedPieceToBuild.Destroy();
                this.Player.simulatedPieceToBuild = null;
            }

            this.BuildMode = false;
        }

        public void OpenPauseMenu()
        {
            ManagedSoundInstance.PauseAll();
            this.soundPaused = true;
            MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Pause);
        }

        public void OpenStatsMenu()
        {
            ManagedSoundInstance.PauseAll();
            this.soundPaused = true;
            MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Stats);
        }

        public void ShakeScreen()
        {
            Vector2 screenShake = new Vector2(this.random.Next(-20, 20), this.random.Next(-20, 20));

            this.transManager.AddMultipleTransitions(outTrans: true, duration: this.random.Next(4, 10), playCount: -1, replaceBaseValue: false, stageTransform: Transition.Transform.Sinus, pingPongCycles: false, cycleMultiplier: 0.02f, paramsToChange: new Dictionary<string, float> { { "PosX", screenShake.X }, { "PosY", screenShake.Y } });
        }

        public void FlashRedOverlay()
        {
            SolidColor redOverlay = new SolidColor(color: Color.Red, viewOpacity: 0.0f);
            redOverlay.transManager.AddTransition(new Transition(transManager: redOverlay.transManager, outTrans: true, duration: 20, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f, endRemoveScene: true));

            this.solidColorManager.Add(redOverlay);
        }

        public void UpdateAllAnims()
        {
            foreach (var sprite in this.camera.GetVisibleSprites(groupName: Cell.Group.Visible, compareWithCameraRect: true))
            {
                if (this.stateMachineTypesManager.CanBeProcessed(sprite.boardPiece)) sprite.UpdateAnimation();
            }
        }

        public void AddPauseMenuTransitions()
        {
            if (this.demoMode || this.solidColorManager.AnySolidColorPresent) return;

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

        public Vector2 TranslateWorldToScreenPos(Vector2 worldPos)
        {
            return new Vector2(
                x: (worldPos.X + this.viewParams.DrawPos.X) / this.viewParams.ScaleX,
                y: (worldPos.Y + this.viewParams.DrawPos.Y) / this.viewParams.ScaleY);
        }

        public Vector2 KeepVector2InWorldBounds(Vector2 vector2)
        {
            vector2.X = Math.Min(Math.Max(vector2.X, 0), this.width - 1);
            vector2.Y = Math.Min(Math.Max(vector2.Y, 0), this.height - 1);

            return vector2;
        }

        public bool SpecifiedPiecesCountIsMet(Dictionary<PieceTemplate.Name, int> piecesToCount)
        {
            foreach (var kvp in piecesToCount)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                if (this.pieceCountByName[pieceName] < kvp.Value) return false;
            }

            return true;
        }

        public override void RenderToTarget()
        {
            // preparing a list of sprites, that cast shadows

            this.blockingLightSpritesList.Clear();
            this.lightSprites.Clear();

            if ((Preferences.drawSunShadows && AmbientLight.SunLightData.CalculateSunLight(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather).sunShadowsColor != Color.Transparent) ||
                (Preferences.drawShadows && AmbientLight.CalculateLightAndDarknessColors(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather).darknessColor != Color.Transparent))
            {
                this.blockingLightSpritesList.AddRange(this.camera.GetVisibleSprites(groupName: Cell.Group.ColMovement).OrderBy(o => o.gfxRect.Bottom));
            }

            var lightSpritesToAdd = this.UpdateDarknessMask(blockingLightSpritesList: blockingLightSpritesList);
            this.lightSprites.AddRange(lightSpritesToAdd);
        }

        public override void Draw()
        {
            if (this.WorldCreationInProgress) return;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            // drawing water surface
            this.scrollingSurfaceManager.DrawAllWater();

            // drawing background (ground, leaving "holes" for water)
            this.Grid.DrawBackground(camera: this.camera);

            // drawing sprites
            var noOfDisplayedSprites = this.Grid.DrawSprites(camera: this.camera, blockingLightSpritesList: this.blockingLightSpritesList);

            // updating debugText
            if (Preferences.DebugMode) this.debugText = $"objects {this.PieceCount}, visible {noOfDisplayedSprites}";

            // drawing debug cell data
            this.Grid.DrawDebugData(drawCellData: Preferences.debugShowCellData, drawPieceData: Preferences.debugShowPieceData);

            // drawing light and darkness
            SonOfRobinGame.SpriteBatch.End();

            this.DrawLightAndDarkness(lightSprites);

            // drawing field tips
            if (Preferences.showFieldControlTips)
            {
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
                FieldTip.DrawFieldTips(world: this);
                SonOfRobinGame.SpriteBatch.End();
            }

            this.CurrentFrame += Preferences.Cap30FPS ? 2 : 1;
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
            this.darknessMask = new RenderTarget2D(SonOfRobinGame.GfxDev, (int)(extendedViewRect.Width / darknessMaskScale.X), (int)(extendedViewRect.Height / darknessMaskScale.Y));

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Creating new darknessMask - {darknessMask.Width}x{darknessMask.Height}");
        }

        private List<Sprite> UpdateDarknessMask(List<Sprite> blockingLightSpritesList)
        {
            // searching for light sources

            var lightSprites = this.camera.GetVisibleSprites(groupName: Cell.Group.LightSource).OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom).ToList();

            // returning if darkness is transparent

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather);

            if (ambientLightData.darknessColor == Color.Transparent)
            {
                SetRenderTarget(this.darknessMask);
                SonOfRobinGame.SpriteBatch.Begin();
                SonOfRobinGame.GfxDev.Clear(ambientLightData.darknessColor);
                SonOfRobinGame.SpriteBatch.End();

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
                    foreach (RenderTarget2D shadowMask in SonOfRobinGame.tempShadowMaskList) shadowMask.Dispose();
                    SonOfRobinGame.tempShadowMaskList.Clear();
                }
                else
                {
                    if (SonOfRobinGame.tempShadowMaskList.Count - 1 < tempShadowMaskIndex) SonOfRobinGame.tempShadowMaskList.Add(new RenderTarget2D(graphicsDevice: SonOfRobinGame.GfxDev, width: SonOfRobinGame.lightSphere.Width, height: SonOfRobinGame.lightSphere.Height));

                    Rectangle lightRect = lightSprite.lightEngine.Rect;
                    lightSprite.lightEngine.tempShadowMaskIndex = tempShadowMaskIndex;

                    // drawing shadows onto shadow mask

                    RenderTarget2D tempShadowMask = SonOfRobinGame.tempShadowMaskList[tempShadowMaskIndex];

                    SetRenderTarget(tempShadowMask);
                    SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
                    SonOfRobinGame.GfxDev.Clear(Color.Black);
                    SonOfRobinGame.SpriteBatch.End();

                    Matrix scaleMatrix = Matrix.CreateScale( // to match drawing size with rect size (light texture size differs from light rect size)
                        (float)tempShadowMask.Width / (float)lightRect.Width,
                        (float)tempShadowMask.Height / (float)lightRect.Height,
                        1f);

                    // first pass - drawing shadows
                    SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlend);
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        if (shadowSprite == lightSprite || !lightSprite.lightEngine.castShadows || !lightRect.Intersects(shadowSprite.gfxRect)) continue;

                        float shadowAngle = Helpers.GetAngleBetweenTwoPoints(start: lightSprite.gfxRect.Center, end: shadowSprite.position);

                        Sprite.DrawShadow(color: Color.White, shadowSprite: shadowSprite, lightPos: lightSprite.position, shadowAngle: shadowAngle, drawOffsetX: -lightRect.X, drawOffsetY: -lightRect.Y);
                    }
                    SonOfRobinGame.SpriteBatch.End();

                    // second pass - erasing shadow from original sprites' position
                    SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlendRedraw);
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        // the lightSprite should be also redrawn, to avoid being overdrawn with any shadow
                        if (lightRect.Intersects(shadowSprite.gfxRect)) shadowSprite.DrawRoutine(calculateSubmerge: true, offsetX: -lightRect.X, offsetY: -lightRect.Y);
                    }
                    SonOfRobinGame.SpriteBatch.End();

                    // drawing light on top of shadows

                    SonOfRobinGame.SpriteBatch.Begin(blendState: lightBlend);
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.lightSphere, tempShadowMask.Bounds, Color.White);
                    SonOfRobinGame.SpriteBatch.End();

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

            SetRenderTarget(this.darknessMask);
            SonOfRobinGame.SpriteBatch.Begin(blendState: darknessMaskBlend);
            SonOfRobinGame.GfxDev.Clear(ambientLightData.darknessColor);

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
                    SonOfRobinGame.SpriteBatch.Draw(tempShadowMask, lightRect, Color.White * lightSprite.lightEngine.Opacity);
                }
                else
                {
                    lightRect.Inflate(lightRect.Width * 0.1f, lightRect.Height * 0.1f); // light level compensation
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.lightSphere, lightRect, Color.White * lightSprite.lightEngine.Opacity);
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.lightSphere, lightRect, Color.White * lightSprite.lightEngine.Opacity * 0.08f); // light level compensation
                }
            }

            SonOfRobinGame.SpriteBatch.End();

            return lightSprites;
        }

        private void DrawLightAndDarkness(List<Sprite> lightSprites)
        {
            Vector2 darknessMaskScale = this.DarknessMaskScale;

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather);
            Rectangle extendedViewRect = this.camera.ExtendedViewRect;

            // drawing ambient light

            BlendState ambientBlend = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.One
            };
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Immediate, blendState: ambientBlend);
            if (ambientLightData.lightColor != Color.Transparent) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, extendedViewRect, ambientLightData.lightColor);

            // drawing point lights

            SonOfRobinGame.SpriteBatch.End();
            BlendState colorLightBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Deferred, blendState: colorLightBlend);

            foreach (var lightSprite in lightSprites)
            {
                if (lightSprite.lightEngine.ColorActive)
                {
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.lightSphere, lightSprite.lightEngine.Rect, lightSprite.lightEngine.Color);
                }
            }

            float fogPercentage = this.weather.FogPercentage;
            float lightningPercentage = this.weather.LightningPercentage;

            if (ambientLightData.darknessColor != Color.Transparent || fogPercentage > 0 || lightningPercentage > 0)
            {
                SonOfRobinGame.SpriteBatch.End();
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

                // drawing fog

                if (fogPercentage > 0) this.scrollingSurfaceManager.DrawFog(Math.Min(fogPercentage * 2f, 1f));

                // drawing lightning

                if (lightningPercentage > 0) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, extendedViewRect, new Color(240, 251, 255) * lightningPercentage * 0.7f);

                // drawing darkness

                if (ambientLightData.darknessColor != Color.Transparent) SonOfRobinGame.SpriteBatch.Draw(this.darknessMask, new Rectangle(x: extendedViewRect.X, y: extendedViewRect.Y, width: (int)(this.darknessMask.Width * darknessMaskScale.X), height: (int)(this.darknessMask.Height * darknessMaskScale.Y)), Color.White);
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}