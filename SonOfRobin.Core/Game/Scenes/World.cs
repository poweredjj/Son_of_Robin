using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class World : Scene
    {
        public const int buildDuration = (int)(60 * 2.5);
        private const int populatingFramesTotal = 8;
        public static int DestroyedNotReleasedWorldCount { get; private set; } = 0;

        private int populatingFramesLeft;
        public readonly DateTime creationStart;
        public DateTime creationEnd;
        public TimeSpan creationDuration;
        public readonly bool demoMode;
        private Task backgroundTask;
        private Object saveGameData;
        public Dictionary<int, BoardPiece> piecesByIDForDeserialization; // for deserialization only
        public bool createMissingPiecesOutsideCamera;
        private bool soundPaused;
        public bool BuildMode { get; private set; }
        private bool spectatorMode;
        private bool cineMode;
        public readonly PieceTemplate.Name initialPlayerName;
        public readonly int seed;
        public readonly int resDivider;
        public readonly Random random;
        public readonly Camera camera;
        private RenderTarget2D darknessAndHeatMask; // used for darkness and for heat
        private RenderTarget2D cameraViewRenderTarget;
        public RenderTarget2D FinalRenderTarget { get; private set; }
        public EffInstance globalEffect;
        public EffInstance heatMaskDistortInstance;
        public readonly Tweener tweenerForGlobalEffect;
        public readonly Map map;
        public readonly PlayerPanel playerPanel;
        public readonly CineCurtains cineCurtains;

        public readonly CraftStats craftStats;
        public readonly KitchenStats cookStats;
        public readonly KitchenStats brewStats;
        public readonly MeatHarvestStats meatHarvestStats;

        public List<PieceTemplate.Name> identifiedPieces; // pieces that were "looked at" in inventory
        private bool mapEnabled;
        public Level ActiveLevel { get; private set; }
        public Level IslandLevel { get; private set; }
        public Player Player { get; private set; }
        public HintEngine HintEngine { get; private set; }

        public Grid Grid
        { get { return this.ActiveLevel.grid; } }

        public int CurrentFrame { get; private set; }
        public int CurrentUpdate { get; private set; } // can be used to measure time elapsed on island
        public int updateMultiplier;
        public readonly IslandClock islandClock;
        public readonly Weather weather;
        public readonly SolidColorManager solidColorManager;
        private readonly ScrollingSurfaceManager scrollingSurfaceManager;
        public readonly SwayManager swayManager;
        public string debugText;
        public int ProcessedNonPlantsCount { get; private set; }
        public int ProcessedPlantsCount { get; private set; }
        public List<PieceTemplate.Name> discoveredRecipesForPieces;
        public readonly DateTime createdTime; // for calculating time spent in game
        private TimeSpan timePlayed; // real time spent while playing (differs from currentUpdate because of island time compression via updateMultiplier)

        public World(int width, int height, int seed, int resDivider, PieceTemplate.Name playerName, Object saveGameData = null, bool demoMode = false, int cellWidthOverride = 0, int cellHeightOverride = 0) :
            base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.seed = seed;
            this.random = new Random(seed);
            this.resDivider = resDivider;
            this.saveGameData = saveGameData;

            Dictionary<string, object> gridSerializedData = null;
            if (this.saveGameData != null)
            {
                var saveGameDataDict = (Dictionary<string, Object>)this.saveGameData;
                gridSerializedData = (Dictionary<string, Object>)saveGameDataDict["grid"];
            }
            this.IslandLevel = new Level(world: this, type: Level.LevelType.Island, hasWater: true, hasWeather: true, plansWeather: true, seed: this.seed, width: width, height: height, cellWidthOverride: cellWidthOverride, cellHeightOverride: cellHeightOverride, gridSerializedData: gridSerializedData);
            this.ActiveLevel = this.IslandLevel;

            this.demoMode = demoMode;
            this.cineMode = false;

            this.BuildMode = false;
            this.spectatorMode = false;
            if (this.demoMode)
            {
                this.InputType = InputTypes.None;
                this.soundActive = false;
            }
            this.piecesByIDForDeserialization = new Dictionary<int, BoardPiece>();
            this.createMissingPiecesOutsideCamera = false;
            this.populatingFramesLeft = populatingFramesTotal;
            this.creationStart = DateTime.Now;

            if (seed < 0) throw new ArgumentException($"Seed value cannot be negative - {seed}.");

            this.CurrentFrame = 0;
            this.CurrentUpdate = 0;
            this.createdTime = DateTime.Now;
            this.TimePlayed = TimeSpan.Zero;
            this.updateMultiplier = 1;
            this.islandClock = this.saveGameData == null ? new IslandClock(0) : new IslandClock();
            this.scrollingSurfaceManager = new ScrollingSurfaceManager(world: this);
            this.swayManager = new SwayManager(this);
            this.HintEngine = new HintEngine(world: this);
            this.ProcessedNonPlantsCount = 0;
            this.ProcessedPlantsCount = 0;
            this.discoveredRecipesForPieces = new List<PieceTemplate.Name> { };
            this.camera = new Camera(world: this, useWorldScale: true, useFluidMotionForMove: true, useFluidMotionForZoom: true);
            this.camera.TrackCoords(Vector2.Zero);
            this.cameraViewRenderTarget = null;
            this.FinalRenderTarget = null;
            this.darknessAndHeatMask = null;
            this.globalEffect = null;
            this.heatMaskDistortInstance = null;
            this.tweenerForGlobalEffect = new Tweener();
            this.MapEnabled = false;
            this.map = new Map(world: this, touchLayout: TouchLayout.Map);
            this.playerPanel = new PlayerPanel(world: this);
            this.cineCurtains = new CineCurtains();
            this.initialPlayerName = playerName;
            this.debugText = "";

            this.AddLinkedScene(this.map);
            this.AddLinkedScene(this.playerPanel);
            this.AddLinkedScene(this.cineCurtains);

            this.solidColorManager = new SolidColorManager(this);
            this.weather = new Weather(world: this, islandClock: this.islandClock);
            this.craftStats = new CraftStats();
            this.cookStats = new KitchenStats();
            this.brewStats = new KitchenStats();
            this.meatHarvestStats = new MeatHarvestStats();
            this.identifiedPieces = new List<PieceTemplate.Name> { PieceTemplate.Name.KnifeSimple };
            if (this.demoMode) this.solidColorManager.Add(new SolidColor(color: Color.White, viewOpacity: 0.4f, clearScreen: false, priority: 1));
            this.soundPaused = false;
        }

        public override void Remove()
        {
            this.ActiveLevel.Destroy(); // TODO add destroy() to all levels here
            Sound.StopAll();
            RumbleManager.StopAll();
            base.Remove();
            this.darknessAndHeatMask?.Dispose();
            this.cameraViewRenderTarget?.Dispose();
            this.FinalRenderTarget?.Dispose();
            DestroyedNotReleasedWorldCount++;
            new Scheduler.Task(taskName: Scheduler.TaskName.GCCollectIfWorldNotRemoved, delay: 60 * 10, executeHelper: 6); // needed to properly release memory after removing world
            MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} world seed {this.seed} id {this.id} {this.IslandLevel.width}x{this.IslandLevel.height} remove() completed.", textColor: new Color(255, 180, 66));
        }

        ~World()
        {
            DestroyedNotReleasedWorldCount--;
            MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} world seed {this.seed} id {this.id} {this.IslandLevel.width}x{this.IslandLevel.height} no longer referenced.", textColor: new Color(120, 255, 174));
        }

        public void EnterNewLevel(Level newLevel)
        {
            if (newLevel.creationInProgress) this.touchLayout = TouchLayout.Empty;
            this.globalEffect = null;

            this.ActiveLevel.playerReturnPos = this.Player.sprite.position;
            this.Player.sprite.RemoveFromBoard();

            var piecesToMoveEvents = new List<BoardPiece> { this.Player };
            foreach (PieceStorage storage in new List<PieceStorage> { this.Player.PieceStorage, this.Player.ToolStorage, this.Player.EquipStorage })
            {
                piecesToMoveEvents.AddRange(storage.GetAllPieces());
            }
            var removedEvents = this.ActiveLevel.levelEventManager.RemovePiecesFromQueueAndGetRemovedEvents(piecesToMoveEvents.ToHashSet());

            this.ActiveLevel = newLevel;
            this.populatingFramesLeft = populatingFramesTotal;

            this.ActiveLevel.levelEventManager.AddToQueue(eventList: removedEvents, addFadeOut: false);
            if (!this.ActiveLevel.creationInProgress) this.Player.MoveToActiveLevel();
        }

        public PieceTemplate.Name PlayerName
        { get { return this.Player == null ? this.initialPlayerName : this.Player.name; } }

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
        { get { return this.Player.pieceInfo.pieceSoundPackTemplate.GetSound(PieceSoundPackTemplate.Action.PlayerSpeak); } }

        public bool CineMode
        {
            get { return cineMode; }
            set
            {
                if (this.cineMode == value) return;

                this.cineMode = value;

                if (value)
                {
                    this.touchLayout = TouchLayout.Empty; // CineSkip should not be used here, because World class cannot execute the skip properly
                    this.tipsLayout = ControlTips.TipsLayout.Empty;
                    this.InputType = InputTypes.None;

                    new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null);
                    this.ActiveLevel.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player), typeof(AmbientSound), typeof(VisualEffect) }, everyFrame: true, nthFrame: true);

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
                    this.camera.ResetMovementSpeed();
                    this.camera.TrackPiece(this.Player);
                    this.globalEffect = null;

                    if (this.cineCurtains.Enabled) this.cineCurtains.Enabled = false;

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
                    spectator.sprite.AssignNewPackage(this.Player.sprite.AnimPackage);

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

                        text = textList[this.random.Next(textList.Count)];
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

        public static TimeSpan WorldElapsedUpdateTime
        { get { return UpdateTimeElapsed + LastDrawDuration; } }

        public static bool CanProcessMoreCameraRectPiecesNow
        { get { return WorldElapsedUpdateTime.Milliseconds <= Preferences.StateMachinesDurationFrameMS; } }

        public static bool CanProcessMoreOffCameraRectPiecesNow
        { get { return WorldElapsedUpdateTime.Milliseconds <= Preferences.StateMachinesDurationFrameMS; } }

        public float PieceCount
        {
            get
            {
                int pieceCount = 0;
                foreach (PieceTemplate.Name templateName in PieceTemplate.allNames)
                {
                    pieceCount += this.ActiveLevel.pieceCountByName[templateName];
                }
                return pieceCount;
            }
        }

        public int HeatQueueSize
        { get { return this.ActiveLevel.heatedPieces.Count; } }

        public void CompleteCreation()
        {
            if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                SonOfRobinGame.FullScreenProgressBar.TurnOff();

                bool menuFound = GetTopSceneOfType(typeof(Menu)) != null;

                this.Remove();
                new TextWindow(text: "Entering the island has been cancelled.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, closingTask: menuFound ? Scheduler.TaskName.Empty : Scheduler.TaskName.OpenMainMenu);
                return;
            };

            if (this.Grid.CreationInProgress)
            {
                this.Grid.CompleteCreation();
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
                    float percentage = FullScreenProgressBar.CalculatePercentage(currentLocalStep: populatingFramesTotal - this.populatingFramesLeft, totalLocalSteps: populatingFramesTotal, currentGlobalStep: Grid.allStagesCount + 1, totalGlobalSteps: SonOfRobinGame.enteringIslandGlobalSteps);

                    SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: percentage, text: LoadingTips.GetTip(), optionalText: Preferences.progressBarShowDetails ? "populating..." : null);

                    if (this.backgroundTask == null) this.backgroundTask = Task.Run(() => this.ProcessAllPopulatingSteps());
                    else
                    {
                        if (this.backgroundTask.IsCompleted || this.backgroundTask.IsFaulted) this.populatingFramesLeft = 0;
                        if (this.backgroundTask.IsFaulted)
                        {
                            SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: true);
                            this.populatingFramesLeft = 0;
                        }
                    }

                    return;
                }
            }

            bool newGameStarted = this.saveGameData == null;

            if (newGameStarted)
            {
                if (this.demoMode) this.camera.TrackDemoModeTarget(firstRun: true);
                else
                {
                    this.CreateAndPlacePlayer();

                    if (this.PlayerName != PieceTemplate.Name.PlayerTestDemoness) PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.Player.sprite.position, templateName: PieceTemplate.Name.CrateStarting, closestFreeSpot: true);
                    PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.Player.sprite.position, templateName: PieceTemplate.Name.PredatorRepellant, closestFreeSpot: true);
                }
            }
            else
            {
                if (this.backgroundTask == null)
                {
                    this.backgroundTask = Task.Run(() => this.Deserialize());

                    SonOfRobinGame.FullScreenProgressBar.TurnOn(percentage: 1, text: LoadingTips.GetTip(), optionalText: Preferences.progressBarShowDetails ? "entering the island..." : null);
                }
                else
                {
                    if (this.backgroundTask.IsFaulted)
                    {
                        SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: true);
                        SonOfRobinGame.FullScreenProgressBar.TurnOff();
                        bool menuFound = GetTopSceneOfType(typeof(Menu)) != null;
                        if (!menuFound) MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                        this.Remove();
                        return;
                    }
                    if (this.backgroundTask.IsCompleted) this.saveGameData = null;
                }
            }

            if (this.saveGameData != null) return; // waiting for background task to finish

            this.backgroundTask = null;
            SonOfRobinGame.FullScreenProgressBar.TurnOff();
            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.ActiveLevel.creationInProgress = false;

            if (this.Player != null && this.Player.level != this.ActiveLevel) this.Player.MoveToActiveLevel();

            this.creationEnd = DateTime.Now;
            this.creationDuration = this.creationEnd - this.creationStart;

            MessageLog.Add(debugMessage: true, text: $"World creation time: {creationDuration:hh\\:mm\\:ss\\.fff}.", textColor: Color.GreenYellow);

            if (!this.demoMode)
            {
                this.camera.TrackPiece(trackedPiece: this.Player, moveInstantly: true);
                this.UpdateViewParams();
                Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.Player);
            }

            this.RefreshRenderTargets();
            if (!this.demoMode) this.map.ForceRender();
            this.CreateTemporaryDecorations(ignoreDuration: true);

            if (!this.demoMode && newGameStarted) this.HintEngine.ShowGeneralHint(type: HintEngine.Type.CineIntroduction, ignoreDelay: true);
            if (this.ActiveLevel.levelType == Level.LevelType.OpenSea) this.HintEngine.ShowGeneralHint(type: HintEngine.Type.CineEndingPart2, ignoreDelay: true);
            else Craft.UnlockRecipesAddedInGameUpdate(world: this);
        }

        private void ProcessAllPopulatingSteps()
        {
            DateTime startTime = DateTime.Now;

            while (true)
            {
                int piecesCreatedCount = CreateMissingPieces(initialCreation: true, maxAmountToCreateAtOnce: (uint)(300000 / populatingFramesTotal), outsideCamera: false, multiplier: 1f, addToDoNotCreateList: false);
                if (piecesCreatedCount < 500) this.populatingFramesLeft = 0;

                this.populatingFramesLeft--;
                if (!this.PopulatingInProgress || this.HasBeenRemoved) break;
            }

            TimeSpan populatingDuration = DateTime.Now - startTime;
            MessageLog.Add(debugMessage: true, text: $"Populating duration: {populatingDuration:hh\\:mm\\:ss\\.fff}.", textColor: Color.GreenYellow);
        }

        private void Deserialize()
        {
            var saveGameDataDict = (Dictionary<string, Object>)this.saveGameData;

            if (this.HasBeenRemoved) return; // to avoid processing if cancelled

            // deserializing header
            {
                var headerData = (Dictionary<string, Object>)saveGameDataDict["header"];

                this.CurrentFrame = (int)(Int64)headerData["currentFrame"];
                this.CurrentUpdate = (int)(Int64)headerData["currentUpdate"];
                this.islandClock.Initialize((int)(Int64)headerData["clockTimeElapsed"]);
                this.TimePlayed = TimeSpan.Parse((string)headerData["TimePlayed"]);
                this.mapEnabled = (bool)headerData["MapEnabled"];
                this.ActiveLevel.doNotCreatePiecesList = (List<PieceTemplate.Name>)headerData["doNotCreatePiecesList"];
                this.discoveredRecipesForPieces = (List<PieceTemplate.Name>)headerData["discoveredRecipesForPieces"];
                this.craftStats.Deserialize((Dictionary<string, Object>)headerData["craftStats"]);
                this.cookStats.Deserialize((Dictionary<string, Object>)headerData["cookStats"]);
                this.brewStats.Deserialize((Dictionary<string, Object>)headerData["brewStats"]);
                this.meatHarvestStats.Deserialize((Dictionary<string, Object>)headerData["meatHarvestStats"]);
                this.identifiedPieces = (List<PieceTemplate.Name>)headerData["identifiedPieces"];
                if (headerData.ContainsKey("mapData")) this.map.Deserialize(headerData["mapData"]);
                if (headerData.ContainsKey("playerLastSteps"))
                {
                    List<Point> lastStepsPointList = (List<Point>)headerData["playerLastSteps"];
                    this.IslandLevel.playerLastSteps.AddRange(lastStepsPointList.Select(p => new Vector2(p.X, p.Y)).ToList());
                }
            }

            if (this.HasBeenRemoved) return; // to avoid processing if cancelled

            // deserializing hints
            {
                var hintsData = (Dictionary<string, Object>)saveGameDataDict["hints"];
                this.HintEngine.Deserialize(hintsData);
            }

            if (this.HasBeenRemoved) return; // to avoid processing if cancelled

            // deserializing weather
            {
                var weatherData = (Dictionary<string, Object>)saveGameDataDict["weather"];
                this.weather.Deserialize(weatherData);
            }

            if (this.HasBeenRemoved) return; // to avoid processing if cancelled

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

                    var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this, position: new Vector2(spritePosX, spritePosY), templateName: templateName, ignoreCollisions: true, id: (int)(Int64)pieceData["base_id"]);
                    if (!newBoardPiece.sprite.IsOnBoard) throw new ArgumentException($"{newBoardPiece.name} could not be placed correctly.");

                    newBoardPiece.Deserialize(pieceData: pieceData);
                    if (newBoardPiece.HeatLevel > 0) this.ActiveLevel.heatedPieces.Add(newBoardPiece);

                    if (PieceInfo.IsPlayer(templateName))
                    {
                        this.Player = (Player)newBoardPiece;
                        this.camera.TrackPiece(trackedPiece: this.Player, moveInstantly: true);
                    }
                }

                if (this.HasBeenRemoved) return; // to avoid processing if cancelled

                // deserializing tracking

                var trackingDataList = (List<Object>)saveGameDataDict["tracking"];
                this.IslandLevel.trackingManager.Deserialize(trackingDataList);

                if (this.HasBeenRemoved) return; // to avoid processing if cancelled

                // deserializing planned events

                var eventDataList = (List<Object>)saveGameDataDict["events"];
                this.ActiveLevel.levelEventManager.Deserialize(eventDataList);

                if (this.HasBeenRemoved) return; // to avoid processing if cancelled

                // removing not needed data

                this.piecesByIDForDeserialization = null; // not needed anymore, "null" will make any attempt to access it afterwards crash the game
            }
        }

        private void CreateAndPlacePlayer()
        {
            if (this.Player != null) return;

            for (int tryIndex = 0; tryIndex < 65535; tryIndex++)
            {
                PieceTemplate.Name playerName = this.initialPlayerName; // taken from a temporary Player boardPiece

                this.Player = (Player)PieceTemplate.CreateAndPlaceOnBoard(world: this, randomPlacement: true, position: Vector2.Zero, templateName: playerName);

                if (this.Player.sprite.IsOnBoard)
                {
                    this.Player.sprite.orientation = Sprite.Orientation.up;
                    this.Player.sprite.CharacterStand();
                    this.Player.sprite.allowedTerrain.RemoveTerrain(Terrain.Name.Biome); // player should be spawned in a safe place, but able to go everywhere afterwards
                    this.Player.sprite.allowedTerrain.ClearExtProperties();

                    BoardPiece startingItem = PieceTemplate.CreatePiece(world: this, templateName: Preferences.newWorldStartingItem);
                    startingItem.createdByPlayer = true;
                    startingItem.canBeHit = false;
                    this.Player.PieceStorage.AddPiece(piece: startingItem, dropIfDoesNotFit: true); // should not equip the item automatically

                    if (playerName == PieceTemplate.Name.PlayerTestDemoness)
                    {
                        var pieceNamesToEquip = new List<PieceTemplate.Name> {
                                PieceTemplate.Name.BootsAllTerrain, PieceTemplate.Name.Map, PieceTemplate.Name.BackpackLuxurious, PieceTemplate.Name.BeltLuxurious, PieceTemplate.Name.HatSimple, PieceTemplate.Name.Dungarees
                            };

                        foreach (PieceTemplate.Name name in pieceNamesToEquip)
                        {
                            BoardPiece piece = PieceTemplate.CreatePiece(world: this, templateName: name);
                            this.Player.EquipStorage.AddPiece(piece);
                        }

                        this.map.TurnOff();

                        var pieceNamesForToolbar = new List<PieceTemplate.Name> {PieceTemplate.Name.AxeCrystal, PieceTemplate.Name.PickaxeCrystal, PieceTemplate.Name.SpearCrystal, PieceTemplate.Name.TorchBig, PieceTemplate.Name.ScytheCrystal, PieceTemplate.Name.ShovelCrystal,  PieceTemplate.Name.BowAdvanced
                            };

                        foreach (PieceTemplate.Name name in pieceNamesForToolbar)
                        {
                            BoardPiece piece = PieceTemplate.CreatePiece(world: this, templateName: name);
                            this.Player.ToolStorage.AddPiece(piece);
                        }

                        for (int i = 0; i < 50; i++)
                        {
                            BoardPiece piece = PieceTemplate.CreatePiece(world: this, templateName: PieceTemplate.Name.ArrowExploding);
                            this.Player.ToolStorage.AddPiece(piece);
                        }

                        foreach (Cell cell in this.Grid.allCells) cell.SetAsVisited();

                        this.Grid.namedLocations.SetAllLocationsAsDiscovered();
                        foreach (HintEngine.Type generalHintType in HintEngine.allTypes.Where(hintType => hintType != HintEngine.Type.CineIntroduction)) this.HintEngine.Disable(generalHintType);
                        foreach (PieceHint.Type pieceHintType in PieceHint.allTypes) this.HintEngine.Disable(pieceHintType);
                        foreach (Tutorials.Type tutorialType in Tutorials.allTypes) this.HintEngine.Disable(tutorialType);

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
                                BoardPiece piece = PieceTemplate.CreatePiece(world: this, templateName: name);
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
                    }

                    return;
                }
            }

            throw new ArgumentException("Cannot place player sprite.");
        }

        public int CreateMissingPieces(bool initialCreation, uint maxAmountToCreateAtOnce = 300000, bool outsideCamera = false, float multiplier = 1.0f, bool clearDoNotCreateList = false, bool addToDoNotCreateList = true)
        {
            if (clearDoNotCreateList) this.ActiveLevel.doNotCreatePiecesList.Clear();
            if (!initialCreation && !CanProcessMoreOffCameraRectPiecesNow) return 0;

            int minPieceAmount = Math.Max(Convert.ToInt32((long)this.ActiveLevel.width * (long)this.ActiveLevel.height / 300000 * multiplier), 0); // 300000
            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (PieceCreationData creationData in this.ActiveLevel.creationDataArrayRegular)
            {
                if (this.ActiveLevel.doNotCreatePiecesList.Contains(creationData.name) || (creationData.doNotReplenish && !initialCreation)) continue;

                int minAmount = Math.Max((int)(minPieceAmount * creationData.multiplier), 4);
                int maxAmount = creationData.GetMaxAmount(level: this.ActiveLevel);

                if (maxAmount > -1) minAmount = Math.Min(minAmount, maxAmount);

                int amountToCreate = Math.Max(minAmount - this.ActiveLevel.pieceCountByName[creationData.name], 0);
                if (amountToCreate > 0) amountToCreateByName[creationData.name] = amountToCreate;
            }

            if (amountToCreateByName.Keys.Count == 0) return 0;

            // creating pieces

            this.createMissingPiecesOutsideCamera = outsideCamera;
            int piecesCreatedCount = 0;

            foreach (var kvp in amountToCreateByName)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                int amountLeftToCreate = kvp.Value; // separate from the iterator below (needs to be modified)

                for (int i = 0; i < kvp.Value * 5; i++)
                {
                    if (!initialCreation && !CanProcessMoreOffCameraRectPiecesNow) return piecesCreatedCount;

                    var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this, randomPlacement: true, position: Vector2.Zero, templateName: pieceName);
                    if (newBoardPiece.sprite.IsOnBoard)
                    {
                        if (initialCreation && newBoardPiece.GetType() == typeof(Plant) && this.random.Next(2) == 0)
                        {
                            Plant newPlant = (Plant)newBoardPiece;
                            newPlant.SimulateUnprocessedGrowth();
                        }

                        piecesCreatedCount++;
                        amountLeftToCreate--;
                    }

                    if (amountLeftToCreate <= 0)
                    {
                        if (!this.ActiveLevel.doNotCreatePiecesList.Contains(pieceName) && addToDoNotCreateList)
                        {
                            this.ActiveLevel.doNotCreatePiecesList.Add(pieceName);
                            new LevelEvent(eventName: LevelEvent.EventName.RestorePieceCreation, delay: PieceInfo.GetInfo(pieceName).delayAfterCreationMinutes * 60 * 60, level: this.ActiveLevel, boardPiece: null, eventHelper: pieceName);
                        }
                        break;
                    }
                }

                if (piecesCreatedCount >= maxAmountToCreateAtOnce) break;
            }

            if (piecesCreatedCount > 0) MessageLog.Add(debugMessage: true, text: $"Created {piecesCreatedCount} new pieces.");
            this.createMissingPiecesOutsideCamera = false;
            return piecesCreatedCount;
        }

        private void CreateTemporaryDecorations(bool ignoreDuration)
        {
            if (!ignoreDuration && !CanProcessMoreOffCameraRectPiecesNow) return;

            DateTime creationStarted = DateTime.Now;
            int createdDecorationsCount = 0;

            foreach (Cell cell in this.Grid.GetCellsInsideRect(rectangle: camera.viewRect, addPadding: true)
                .Where(cell => !cell.temporaryDecorationsCreated)
                .OrderBy(x => this.random.Next()))
            {
                foreach (PieceCreationData pieceCreationData in this.ActiveLevel.creationDataArrayTemporaryDecorations)
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
                                    this.ActiveLevel.temporaryDecorationSprites.Add(newBoardPiece.sprite);
                                    newBoardPiece.isTemporaryDecoration = true;
                                    createdDecorationsCount++;
                                    break;
                                }

                                if (!ignoreDuration && !CanProcessMoreOffCameraRectPiecesNow)
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
                MessageLog.Add(debugMessage: true, text: $"Temp decors created: {createdDecorationsCount} total: {this.ActiveLevel.temporaryDecorationSprites.Count} duration: {tempDecorCreationDuration:\\:ss\\.fff} completed: {completed}");
            }
        }

        private void DestroyTemporaryDecorationsOutsideCamera()
        {
            if (!CanProcessMoreOffCameraRectPiecesNow) return;

            DateTime creationStarted = DateTime.Now;
            int destroyedDecorationsCount = 0;

            Rectangle extendedCameraRect = this.camera.viewRect;
            extendedCameraRect.Inflate(this.camera.viewRect.Width, this.camera.viewRect.Height); // to destroying newly created pieces around camera edge

            foreach (Sprite sprite in this.ActiveLevel.temporaryDecorationSprites.ToList())
            {
                if (!extendedCameraRect.Contains(sprite.position) && sprite.boardPiece.exists)
                {
                    if (sprite.currentCell != null) sprite.currentCell.temporaryDecorationsCreated = false;
                    sprite.boardPiece.Destroy();
                    this.ActiveLevel.temporaryDecorationSprites.Remove(sprite);
                    destroyedDecorationsCount++;
                }

                if (!CanProcessMoreOffCameraRectPiecesNow) break;
            }

            if (destroyedDecorationsCount > 0)
            {
                TimeSpan tempDecorDestroyDuration = DateTime.Now - creationStarted;
                MessageLog.Add(debugMessage: true, text: $"Temp decors destroyed: {destroyedDecorationsCount} duration: {tempDecorDestroyDuration:\\:ss\\.fff}");
            }
            return;
        }

        public void UpdateViewParams()
        {
            Vector2 analogCameraCorrection = Vector2.Zero;

            if (!this.demoMode && this.Player?.activeState != BoardPiece.State.PlayerControlledSleep)
            {
                analogCameraCorrection = InputMapper.Analog(InputMapper.Action.WorldCameraMove) * new Vector2(this.camera.viewRect.Width, this.camera.viewRect.Height) * new Vector2(0.05f, 0.03f);
            }

            this.viewParams.Width = this.ActiveLevel.width;
            this.viewParams.Height = this.ActiveLevel.height;

            this.camera.Update(cameraCorrection: analogCameraCorrection, calculateAheadCorrection: true);
            this.camera.SetViewParams(this);

            // width and height are set once in constructor
        }

        public override void Update()
        {
            if (this.ActiveLevel.creationInProgress)
            {
                this.CompleteCreation();
                return;
            }

            this.ProcessInput();
            this.UpdateViewParams();
            MeshDefinition.UpdateAllDefs();
            this.tweenerForGlobalEffect.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            this.weather.Update();
            this.swayManager.Update();

            if (this.soundPaused && this.inputActive)
            {
                ManagedSoundInstance.ResumeAll();
                this.soundPaused = false;
            }

            this.scrollingSurfaceManager.Update(updateFog: this.weather.FogPercentage > 0, updateHotAir: this.weather.HeatPercentage > 0);
            this.ActiveLevel.recentParticlesManager.Update();

            if (this.demoMode) this.camera.TrackDemoModeTarget(firstRun: false);

            bool createMissingPieces = this.CurrentUpdate % 200 == 0 && Preferences.debugCreateMissingPieces && !this.CineMode && !this.BuildMode;
            if (createMissingPieces) this.CreateMissingPieces(initialCreation: false, maxAmountToCreateAtOnce: 100, outsideCamera: true, multiplier: 0.1f);

            if (!createMissingPieces && this.CurrentUpdate % 31 == 0)
            {
                this.DestroyTemporaryDecorationsOutsideCamera();
                this.CreateTemporaryDecorations(ignoreDuration: false);
            }

            this.ActiveLevel.trackingManager.ProcessQueue();
            this.ActiveLevel.levelEventManager.ProcessQueue();

            this.ProcessHeatQueue();

            if (!this.BuildMode) this.UpdateAllAnims();

            if (this.Player != null)
            {
                this.ProcessOneNonPlant(this.Player);
                this.Player.UpdateDistanceWalked();
                this.Player.UpdateLastSteps();
            }

            foreach (BoardPiece mapMarker in this.ActiveLevel.mapMarkerByColor.Values)
            {
                if (mapMarker != null && mapMarker.sprite.IsOnBoard) this.ProcessOneNonPlant(mapMarker);
            }

            this.StateMachinesProcessCameraView();

            if (!createMissingPieces)
            {
                // random to ensure that both SM types will get processed on odd and even frames
                if (this.random.Next(2) == 0) this.StateMachinesProcessPlantQueue();
                else this.StateMachinesProcessNonPlantQueue();
            }

            this.CurrentUpdate += this.updateMultiplier;
            this.islandClock.Advance(this.updateMultiplier);
            this.ActiveLevel.stateMachineTypesManager.IncreaseDeltaCounters(this.updateMultiplier);
        }

        private void ProcessInput()
        {
            if (this.demoMode || this.CineMode) return;

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
            int processedPiecesCount = 0;

            foreach (BoardPiece piece in this.Grid.GetPiecesInCameraView(groupName: Cell.Group.StateMachinesNonPlants, compareWithCameraRect: true))
            {
                this.ProcessOneNonPlant(piece);
                processedPiecesCount++;

                if (processedPiecesCount > 30 && !CanProcessMoreCameraRectPiecesNow) // even in the worst case, some pieces must be processed
                {
                    // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Camera view SM: no time to finish processing queue - {this.WorldElapsedUpdateTime.Milliseconds}ms.");
                    return;
                }
            }
        }

        private void ProcessHeatQueue()
        {
            foreach (BoardPiece boardPiece in new HashSet<BoardPiece>(this.ActiveLevel.heatedPieces))
            {
                boardPiece.ProcessHeat();
            }
        }

        public void RemovePieceFromHeatQueue(BoardPiece boardPiece)
        {
            try
            { this.ActiveLevel.heatedPieces.Remove(boardPiece); }
            catch (KeyNotFoundException)
            { }
        }

        private void StateMachinesProcessNonPlantQueue()
        {
            this.ProcessedNonPlantsCount = 0;

            if (!CanProcessMoreOffCameraRectPiecesNow)
            {
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Non-plant SM: no time to start processing queue - {this.WorldElapsedUpdateTime.Milliseconds}ms.");
                return;
            }

            if (this.ActiveLevel.nonPlantSpritesQueue.Count == 0)
            {
                // var startTime = DateTime.Now; // for testing

                this.ActiveLevel.nonPlantSpritesQueue = new Queue<Sprite>(
                    this.Grid.GetSpritesFromAllCells(groupName: Cell.Group.StateMachinesNonPlants)
                    .Where(sprite => sprite.boardPiece.FramesSinceLastProcessed > 0)
                    .OrderBy(sprite => sprite.boardPiece.lastFrameSMProcessed)
                    );

                // var duration = DateTime.Now - startTime; // for testing
                // SonOfRobinGame.messageLog.AddMessage(text: $"{this.CurrentUpdate} created new nonPlantSpritesQueue ({this.nonPlantSpritesQueue.Count}) - duration {duration.Milliseconds}ms"); // for testing

                if (!CanProcessMoreOffCameraRectPiecesNow) return;
            }

            while (true)
            {
                if (ActiveLevel.nonPlantSpritesQueue.Count == 0) return;

                BoardPiece currentNonPlant = this.ActiveLevel.nonPlantSpritesQueue.Dequeue().boardPiece;

                this.ProcessOneNonPlant(currentNonPlant);
                this.ProcessedNonPlantsCount++;

                if (!CanProcessMoreOffCameraRectPiecesNow) return;
            }
        }

        private void ProcessOneNonPlant(BoardPiece piece)
        {
            int timeDelta = piece.FramesSinceLastProcessed;

            if (piece.GetType() == typeof(Animal))
            {
                Animal animal = (Animal)piece;
                if (animal.isPregnant && animal.alive) animal.pregnancyFramesLeft = Math.Max(animal.pregnancyFramesLeft - timeDelta, 0);
                animal.ExpendEnergy(0.1f * timeDelta);

                if (animal.alive && (animal.HitPoints <= 0 || animal.efficiency == 0 || animal.currentAge >= animal.maxAge))
                {
                    animal.Kill();
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
            if (piece.maxAge > 0 && piece.alive) piece.GrowOlder(timeDelta: Math.Min(timeDelta, 200)); // limiting aging to reasonable amount
        }

        private void StateMachinesProcessPlantQueue()
        {
            this.ProcessedPlantsCount = 0;

            if (!CanProcessMoreOffCameraRectPiecesNow)
            {
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Plant SM: no time to start processing queue - {this.WorldElapsedUpdateTime.Milliseconds}ms.");
                return;
            }

            if (this.ActiveLevel.plantCellsQueue.Count == 0)
            {
                this.ActiveLevel.plantCellsQueue = new Queue<Cell>(this.Grid.allCells);
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Plants cells queue replenished ({this.plantCellsQueue.Count})");

                if (!CanProcessMoreOffCameraRectPiecesNow) return;
            }

            if (this.ActiveLevel.plantSpritesQueue.Count == 0)
            {
                var newPlantSpritesList = new List<Sprite>();

                while (true)
                {
                    Cell cell = this.ActiveLevel.plantCellsQueue.Dequeue();
                    newPlantSpritesList.AddRange(cell.spriteGroups[Cell.Group.StateMachinesPlants]); // not shuffled to save cpu time

                    if (this.ActiveLevel.plantCellsQueue.Count == 0 || !CanProcessMoreOffCameraRectPiecesNow) break;
                }

                this.ActiveLevel.plantSpritesQueue = new Queue<Sprite>(newPlantSpritesList);

                if (!CanProcessMoreOffCameraRectPiecesNow) return;
            }

            while (true)
            {
                if (this.ActiveLevel.plantSpritesQueue.Count == 0) return;

                Plant currentPlant = (Plant)this.ActiveLevel.plantSpritesQueue.Dequeue().boardPiece;
                if (currentPlant.sprite.IsInCameraRect && !Preferences.debugShowPlantGrowthInCamera) continue;

                currentPlant.StateMachineWork();
                if (currentPlant.currentAge >= currentPlant.maxAge || currentPlant.efficiency < 0.15 || currentPlant.Mass < 1) currentPlant.Destroy();
                this.ProcessedPlantsCount++;

                if (!CanProcessMoreOffCameraRectPiecesNow) return;
            }
        }

        public void EnterBuildMode(Craft.Recipe recipe)
        {
            if (this.BuildMode) throw new ArgumentException("Is already in build mode.");

            this.BuildMode = true;

            Inventory.SetLayout(Inventory.LayoutType.None, player: this.Player);
            this.touchLayout = TouchLayout.WorldBuild;
            this.tipsLayout = ControlTips.TipsLayout.WorldBuild;
            this.Player.activeState = BoardPiece.State.PlayerControlledBuilding;
            this.Player.RemovePassiveMovement(); // player moved by wind could lose "connection" to a chest, resulting in a crash

            this.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
            this.ActiveLevel.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player), typeof(AmbientSound), typeof(VisualEffect) }, everyFrame: true, nthFrame: true);
            this.islandClock.Pause();

            Scene craftMenu = GetTopSceneOfType(typeof(Menu));
            craftMenu?.MoveToBottom(); // there is no craft menu if planting
            SonOfRobinGame.HintWindow.TurnOff();

            this.Player.recipeToBuild = recipe;
            this.Player.simulatedPieceToBuild = PieceTemplate.CreateAndPlaceOnBoard(templateName: recipe.pieceToCreate, world: this, position: this.Player.sprite.position, ignoreCollisions: true, createdByPlayer: true); // "template" piece should be placed - collisions doesn't matter...
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
            if (builtPiece.pieceInfo.destroysPlantsWhenBuilt) builtPiece.DestroyContainedPlants(delay: buildDuration);

            bool plantMode = builtPiece.GetType() == typeof(Plant);

            Yield debrisYield = new Yield(firstDebrisTypeList:
                new List<ParticleEngine.Preset> { plantMode ? ParticleEngine.Preset.DebrisLeaf : ParticleEngine.Preset.DebrisStarSmall });

            if (plantMode) Sound.QuickPlay(SoundData.Name.Planting);
            else
            {
                Sound.QuickPlay(SoundData.Name.Sawing);
                Sound.QuickPlay(SoundData.Name.Hammering);
            }

            this.Player.simulatedPieceToBuild?.Destroy();
            this.Player.simulatedPieceToBuild = null;

            builtPiece.sprite.opacity = 0f;
            new OpacityFade(sprite: builtPiece.sprite, destOpacity: 1f, duration: buildDuration);

            new LevelEvent(eventName: LevelEvent.EventName.FinishBuilding, level: this.ActiveLevel, delay: buildDuration, boardPiece: null);
            new LevelEvent(eventName: LevelEvent.EventName.PlaySoundByName, level: this.ActiveLevel, delay: buildDuration, boardPiece: null, eventHelper: plantMode ? SoundData.Name.MovingPlant : SoundData.Name.Chime);
            new LevelEvent(eventName: LevelEvent.EventName.YieldDropDebris, level: this.ActiveLevel, delay: buildDuration, boardPiece: null, eventHelper: new Dictionary<string, Object> { { "piece", builtPiece }, { "yield", debrisYield } });
        }

        public void ExitBuildMode(bool restoreCraftMenu, bool showCraftMessages = false)
        {
            if (!this.BuildMode) throw new ArgumentException("Is not in build mode.");

            Inventory.SetLayout(Inventory.LayoutType.Toolbar, player: this.Player);
            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.Player.activeState = BoardPiece.State.PlayerControlledWalking;

            this.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
            this.ActiveLevel.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
            this.islandClock.Resume();

            this.Player.simulatedPieceToBuild?.Destroy();
            this.Player.simulatedPieceToBuild = null;

            Scene craftMenu = GetBottomSceneOfType(typeof(Menu));
            if (restoreCraftMenu)  // there is no craft menu if planting
            {
                if (craftMenu != null)
                {
                    Menu rebuiltCraftMenu = ((Menu)craftMenu).Rebuild(instantScroll: true); // rebuilding, to avoid counting simulated piece as built piece
                    rebuiltCraftMenu?.MoveToTop();
                }
            }
            else craftMenu?.Remove();

            if (showCraftMessages) this.Player.recipeToBuild.UnlockNewRecipesAndShowSummary(this);
            this.Player.recipeToBuild = null;

            this.BuildMode = false;
        }

        public void OpenMenu(MenuTemplate.Name templateName, object executeHelper = null)
        {
            ManagedSoundInstance.PauseAll();
            RumbleManager.StopAll();
            this.soundPaused = true;
            MenuTemplate.CreateMenuFromTemplate(templateName: templateName, executeHelper: executeHelper);
        }

        public void FlashRedOverlay()
        {
            SolidColor redOverlay = new(color: Color.Red, viewOpacity: 0.0f);
            redOverlay.transManager.AddTransition(new Transition(transManager: redOverlay.transManager, outTrans: true, duration: 20, playCount: 1, stageTransform: Transition.Transform.Sinus, baseParamName: "Opacity", targetVal: 0.5f, endRemoveScene: true));

            this.globalEffect = new MosaicInstance(textureSize: new Vector2(this.cameraViewRenderTarget.Width, this.cameraViewRenderTarget.Height), blurSize: new Vector2(8, 8), framesLeft: 20);
            this.globalEffect.intensityForTweener = 0f;
            this.tweenerForGlobalEffect
                .TweenTo(target: this.globalEffect, expression: effect => effect.intensityForTweener, toValue: 1f, duration: 10f / 60f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticIn);

            this.solidColorManager.Add(redOverlay);
        }

        public void UpdateAllAnims()
        {
            foreach (BoardPiece piece in this.Grid.GetPiecesInCameraView(groupName: Cell.Group.Visible, compareWithCameraRect: true))
            {
                if (this.ActiveLevel.stateMachineTypesManager.CanBeProcessed(piece)) piece.sprite.UpdateAnimation(checkForCollision: true);
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
            return new Vector2(
                x: Math.Clamp(value: vector2.X, min: 0, max: this.ActiveLevel.width - 1),
                y: Math.Clamp(value: vector2.Y, min: 0, max: this.ActiveLevel.height - 1));
        }

        public bool SpecifiedPiecesCountIsMet(Dictionary<PieceTemplate.Name, int> piecesToCount)
        {
            foreach (var kvp in piecesToCount)
            {
                PieceTemplate.Name pieceName = kvp.Key;
                if (this.ActiveLevel.pieceCountByName[pieceName] < kvp.Value) return false;
            }

            return true;
        }

        public override void RenderToTarget()
        {
            if (this.ActiveLevel.creationInProgress || SonOfRobinGame.IgnoreThisDraw) return;

            // drawing darkness

            Matrix worldMatrix = this.TransformMatrix;

            var blockingLightSpritesList = new List<Sprite>();

            if ((Preferences.drawSunShadows &&
                AmbientLight.SunLightData.CalculateSunLight(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather).sunShadowsColor != Color.Transparent) ||
                (AmbientLight.CalculateLightAndDarknessColors(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather, level: this.ActiveLevel).darknessColor != Color.Transparent))
            {
                blockingLightSpritesList = this.Grid.GetPiecesInCameraView(groupName: Cell.Group.ColMovement)
                    .OrderBy(o => o.sprite.GfxRect.Bottom)
                    .Select(o => o.sprite).ToList();
            }

            var lightSprites = this.UpdateDarknessMask(blockingLightSpritesList: blockingLightSpritesList);

            // switching to camera view RenderTarget
            SetRenderTarget(this.cameraViewRenderTarget);

            // drawing water surface
            if (this.ActiveLevel.hasWater) this.scrollingSurfaceManager.DrawAllWater();
            else SonOfRobinGame.GfxDev.Clear(Color.Black);

            // drawing background (ground, leaving "holes" for water)
            SetupPolygonDrawing(allowRepeat: true, transformMatrix: worldMatrix);
            int trianglesDrawn = this.Grid.DrawBackground();

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: worldMatrix);

            // drawing sprites
            var drawnPieces = this.Grid.DrawSprites(blockingLightSpritesList: blockingLightSpritesList);

            // updating debugText
            if (Preferences.DebugMode) this.debugText = $"objects {this.PieceCount}, visible {drawnPieces.Count} tris {trianglesDrawn}";

            // drawing debug cell data
            this.Grid.DrawDebugData(drawCellData: Preferences.debugShowCellData, drawPieceData: Preferences.debugShowPieceData);

            // drawing light and darkness
            if (Preferences.debugShowOutsideCamera) SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: this.camera.viewRect, color: Color.White, thickness: 3f);
            SonOfRobinGame.SpriteBatch.End();

            this.DrawLightAndDarkness(lightSprites);

            // drawing highlighted pieces
            if (Preferences.pickupsHighlighted)
            {
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: worldMatrix);
                this.DrawHighlightedPieces(drawnPieces);
                SonOfRobinGame.SpriteBatch.End();
            }

            // drawing heat deformation
            SetRenderTarget(this.darknessAndHeatMask);
            SonOfRobinGame.GfxDev.Clear(Color.Black);

            if (this.weather.HeatPercentage > 0)
            {
                this.scrollingSurfaceManager.hotAir.Draw(opacityOverride: this.weather.HeatPercentage * 0.4f, endSpriteBatch: false);
                SonOfRobinGame.SpriteBatch.End();
            }

            BlendState multiplyBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: worldMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Immediate, blendState: multiplyBlend);

            this.ActiveLevel.recentParticlesManager.DrawDistortion(); // SpriteSortMode.Immediate is needed to draw particles properly

            SonOfRobinGame.SpriteBatch.End();

            // drawing all effects
            SetRenderTarget(this.FinalRenderTarget);
            SonOfRobinGame.SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

            this.heatMaskDistortInstance.TurnOn(currentUpdate: this.CurrentUpdate, drawColor: Color.White);
            SonOfRobinGame.SpriteBatch.Draw(this.cameraViewRenderTarget, this.cameraViewRenderTarget.Bounds, Color.White);
            SonOfRobinGame.SpriteBatch.End();

            // additional "permanent" effects can be added here (like curves, etc.), but everything should finally be rendered on FinalRenderTarget
        }

        public override void Draw()
        {
            if (this.ActiveLevel.creationInProgress) return;

            // drawing FinalRenderTarget

            if (Preferences.halfFramerate) SonOfRobinGame.GfxDev.Clear(Color.Black); // needed to eliminate flickering
            SonOfRobinGame.SpriteBatch.Begin(sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

            if (this.globalEffect != null)
            {
                this.globalEffect.TurnOn(currentUpdate: this.CurrentUpdate, drawColor: Color.White);
                if (this.globalEffect.framesLeft == 0) this.globalEffect = null;
            }

            SonOfRobinGame.SpriteBatch.Draw(this.FinalRenderTarget, this.FinalRenderTarget.Bounds, Color.White * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.End();

            // drawing field tips

            if (Preferences.showFieldControlTips)
            {
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);
                FieldTip.DrawFieldTips(world: this);
                SonOfRobinGame.SpriteBatch.End();
            }

            this.CurrentFrame += Preferences.halfFramerate ? 2 : 1;
        }

        private List<Sprite> UpdateDarknessMask(List<Sprite> blockingLightSpritesList)
        {
            // searching for light sources

            var lightSprites = this.Grid.GetPiecesInCameraView(groupName: Cell.Group.LightSource)
                .OrderBy(o => o.sprite.AnimFrame.layer)
                .ThenBy(o => o.sprite.GfxRect.Bottom)
                .Select(o => o.sprite)
                .ToList();

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather, level: this.ActiveLevel);

            // preparing darkness mask

            SetRenderTarget(this.darknessAndHeatMask);
            SonOfRobinGame.GfxDev.Clear(ambientLightData.darknessColor);

            if (ambientLightData.darknessColor == Color.Transparent) return lightSprites;

            Matrix worldMatrix = this.TransformMatrix;

            var darknessMaskBlend = new BlendState
            {
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            // preparing and drawing shadow masks

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

            foreach (var lightSprite in lightSprites)
            {
                if (SonOfRobinGame.tempShadowMask == null) SonOfRobinGame.tempShadowMask = new RenderTarget2D(graphicsDevice: SonOfRobinGame.GfxDev, width: SonOfRobinGame.lightSphere.Width, height: SonOfRobinGame.lightSphere.Height);

                Rectangle lightRect = lightSprite.lightEngine.Rect;

                // drawing shadows onto shadow mask

                RenderTarget2D tempShadowMask = SonOfRobinGame.tempShadowMask;

                SetRenderTarget(tempShadowMask);
                SonOfRobinGame.GfxDev.Clear(Color.Black);

                Matrix scaleMatrix = Matrix.CreateScale( // to match drawing size with rect size (light texture size differs from light rect size)
                    (float)tempShadowMask.Width / (float)lightRect.Width,
                    (float)tempShadowMask.Height / (float)lightRect.Height,
                    1f);

                // first pass - drawing shadows
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlend);
                foreach (Sprite shadowSprite in blockingLightSpritesList)
                {
                    if (shadowSprite == lightSprite || !lightSprite.lightEngine.castShadows || !lightRect.Intersects(shadowSprite.GfxRect)) continue;

                    float shadowAngle = Helpers.GetAngleBetweenTwoPoints(start: lightSprite.GfxRect.Center, end: shadowSprite.position);

                    Sprite.DrawShadow(color: Color.White, shadowSprite: shadowSprite, lightPos: lightSprite.position, shadowAngle: shadowAngle, drawOffsetX: -lightRect.X, drawOffsetY: -lightRect.Y);
                }
                SonOfRobinGame.SpriteBatch.End();

                // second pass - erasing shadow from original sprites' position
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlendRedraw);
                foreach (Sprite shadowSprite in blockingLightSpritesList)
                {
                    // the lightSprite should be also redrawn, to avoid being overdrawn with any shadow
                    if (lightRect.Intersects(shadowSprite.GfxRect)) shadowSprite.DrawRoutine(calculateSubmerge: true, offsetX: -lightRect.X, offsetY: -lightRect.Y);
                }
                SonOfRobinGame.SpriteBatch.End();

                // drawing light on top of shadows

                SonOfRobinGame.SpriteBatch.Begin(blendState: lightBlend);
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.lightSphere, tempShadowMask.Bounds, Color.White);
                SonOfRobinGame.SpriteBatch.End();

                // subtracting shadow mask from darkness

                SetRenderTarget(this.darknessAndHeatMask);
                SonOfRobinGame.SpriteBatch.Begin(transformMatrix: worldMatrix, blendState: darknessMaskBlend);
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.tempShadowMask, lightRect, Color.White * lightSprite.lightEngine.Opacity);
                SonOfRobinGame.SpriteBatch.End();
            }

            return lightSprites;
        }

        private void DrawLightAndDarkness(List<Sprite> lightSprites)
        {
            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(currentDateTime: this.islandClock.IslandDateTime, weather: this.weather, this.ActiveLevel);
            Rectangle cameraRect = this.camera.viewRect;

            // drawing ambient light

            BlendState ambientBlend = new()
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.One
            };
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, samplerState: SamplerState.AnisotropicClamp, sortMode: SpriteSortMode.Immediate, blendState: ambientBlend);
            if (ambientLightData.lightColor != Color.Transparent) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, cameraRect, ambientLightData.lightColor);

            // drawing point lights

            SonOfRobinGame.SpriteBatch.End();
            BlendState colorLightBlend = new()
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
                if (lightSprite.lightEngine.ColorActive && cameraRect.Intersects(lightSprite.lightEngine.Rect))
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

                if (fogPercentage > 0 && (this.Player == null || !this.Player.buffEngine.HasBuff(BuffEngine.BuffType.CanSeeThroughFog)))
                {
                    this.scrollingSurfaceManager.fog.Draw(Math.Min(fogPercentage, 1f));
                }

                // drawing lightning

                if (lightningPercentage > 0) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.camera.viewRect, new Color(240, 251, 255) * lightningPercentage * 0.7f);

                // drawing darkness

                if (ambientLightData.darknessColor != Color.Transparent)
                {
                    SonOfRobinGame.SpriteBatch.End();
                    SonOfRobinGame.SpriteBatch.Begin();
                    SonOfRobinGame.SpriteBatch.Draw(this.darknessAndHeatMask, this.darknessAndHeatMask.Bounds, Color.White);
                }
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        private void DrawHighlightedPieces(List<BoardPiece> drawnPieces)
        {
            if (this.demoMode || !this.Player.CanSeeAnything) return;

            float pulseOpacity = (float)(this.CurrentFrame % 120) / 120;
            pulseOpacity = (float)Math.Cos(pulseOpacity * Math.PI - Math.PI / 2);

            Color highlightColor = Helpers.Blend2Colors(firstColor: new Color(2, 68, 156), secondColor: new Color(235, 243, 255), firstColorOpacity: 1f - pulseOpacity, secondColorOpacity: pulseOpacity);
            foreach (BoardPiece piece in drawnPieces)
            {
                if (!piece.pieceInfo.canBePickedUp || (piece.GetType() == typeof(Animal) && piece.alive)) continue;

                piece.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: highlightColor * 0.8f, drawFill: false, borderThickness: (int)(2 * (1f / piece.sprite.AnimFrame.scale)), textureSize: piece.sprite.AnimFrame.textureSize, priority: 0, framesLeft: 1));
                piece.sprite.Draw();
            }
        }

        protected override void AdaptToNewSize()
        {
            this.UpdateViewParams();
            this.RefreshRenderTargets();
        }

        public void RefreshRenderTargets()
        {
            int newWidth = SonOfRobinGame.GfxDevMgr.PreferredBackBufferWidth;
            int newHeight = SonOfRobinGame.GfxDevMgr.PreferredBackBufferHeight;

            if (this.cameraViewRenderTarget == null || this.cameraViewRenderTarget.Width != newWidth || this.cameraViewRenderTarget.Height != newHeight)
            {
                this.cameraViewRenderTarget?.Dispose();
                this.cameraViewRenderTarget = new RenderTarget2D(SonOfRobinGame.GfxDev, newWidth, newHeight, false, SurfaceFormat.Color, DepthFormat.None);
                MessageLog.Add(debugMessage: true, text: $"Creating new camera view target (world) - {this.cameraViewRenderTarget.Width}x{this.cameraViewRenderTarget.Height}");
            }

            if (this.FinalRenderTarget == null || this.FinalRenderTarget.Width != newWidth || this.FinalRenderTarget.Height != newHeight)
            {
                this.FinalRenderTarget?.Dispose();
                this.FinalRenderTarget = new RenderTarget2D(SonOfRobinGame.GfxDev, newWidth, newHeight, false, SurfaceFormat.Color, DepthFormat.None);
                MessageLog.Add(debugMessage: true, text: $"Creating new final render target (world) - {this.FinalRenderTarget.Width}x{this.FinalRenderTarget.Height}");
            }

            if (this.darknessAndHeatMask == null || this.darknessAndHeatMask.Width != newWidth || this.darknessAndHeatMask.Height != newHeight)
            {
                this.darknessAndHeatMask?.Dispose();
                this.darknessAndHeatMask = new RenderTarget2D(SonOfRobinGame.GfxDev, newWidth, newHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                MessageLog.Add(debugMessage: true, text: $"Creating new darkness mask (world) - {this.darknessAndHeatMask.Width}x{this.darknessAndHeatMask.Height}");
            }

            this.heatMaskDistortInstance = new HeatMaskDistortionInstance(baseTexture: this.cameraViewRenderTarget, distortTexture: this.darknessAndHeatMask);
        }
    }
}