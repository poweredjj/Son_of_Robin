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
        public Vector2 analogMovementLeftStick;
        public Vector2 analogMovementRightStick;
        public Vector2 analogCameraCorrection;

        public bool worldCreationInProgress;
        private bool plantsProcessing;
        private readonly static int initialPiecesCreationFramesTotal = 20;
        public readonly static int buildDuration = (int)(60 * 2.5);
        private int initialPiecesCreationFramesLeft;
        public readonly DateTime creationStart;
        public DateTime creationEnd;
        public TimeSpan creationDuration;
        public readonly bool demoMode;
        public readonly bool playerFemale;

        private Object saveGameData;
        public bool createMissingPiecesOutsideCamera;
        public DateTime lastSaved;
        public bool BuildMode { get; private set; }

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
                    Sound.QuickPlay(SoundData.Name.AngelChorus);

                    this.player.RemoveFromStateMachines();

                    BoardPiece spectator = PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.camera.TrackedPos, templateName: PieceTemplate.Name.PlayerGhost, closestFreeSpot: true, female: this.playerFemale, randomSex: false);

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

                    new TextWindow(text: text, textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 60 * 6, priority: 1, animSound: this.DialogueSound);
                }
                else
                {
                    BoardPiece spectator = this.player;

                    bool playerFound = false;
                    foreach (Sprite sprite in this.grid.GetSpritesFromAllCells(Cell.Group.All))
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

                    new TextWindow(text: "I live... Again!", textColor: Color.White, bgColor: Color.Black, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 60 * 3, priority: 1, animSound: this.DialogueSound);
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

                if (cineMode)
                {
                    this.touchLayout = TouchLayout.Empty; // CineSkip should not be used here, because World class cannot execute the skip properly
                    this.tipsLayout = ControlTips.TipsLayout.Empty;
                    this.InputType = InputTypes.None;

                    new Scheduler.Task(taskName: Scheduler.TaskName.TempoStop, delay: 0, executeHelper: null);
                    this.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player), typeof(AmbientSound) }, everyFrame: true, nthFrame: true);

                    this.player.activeState = BoardPiece.State.PlayerControlledByCinematic;
                    this.player.pointWalkTarget = Vector2.Zero;
                    this.player.sprite.CharacterStand(); // to avoid playing walk animation while standing
                }
                else
                {
                    this.touchLayout = TouchLayout.WorldMain;
                    this.tipsLayout = ControlTips.TipsLayout.WorldMain;
                    this.InputType = InputTypes.Normal;

                    this.player.activeState = BoardPiece.State.PlayerControlledWalking;
                    this.player.pointWalkTarget = Vector2.Zero;

                    this.solidColorManager.RemoveAll(0);

                    this.camera.SetZoom(zoom: 1f, zoomSpeedMultiplier: 1f);
                    this.camera.TrackPiece(this.player);

                    new Scheduler.Task(taskName: Scheduler.TaskName.TempoPlay, delay: 0, executeHelper: null);
                }
            }
        }

        public readonly int seed;
        public readonly int resDivider;
        public readonly int initialMaxAnimalsMultiplier;
        public int maxAnimalsPerName;
        public bool addAgressiveAnimals;
        public FastNoiseLite noise;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Camera camera;
        private RenderTarget2D darknessMask;
        public readonly Map map;
        public readonly PlayerPanel playerPanel;
        public readonly SolidColorManager solidColorManager;
        public readonly SMTypesManager stateMachineTypesManager;
        public readonly CraftStats craftStats;
        public List<PieceTemplate.Name> identifiedPieces; // pieces that were "looked at" in inventory
        private bool mapEnabled;
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

        public Sound DialogueSound { get { return this.player.soundPack.GetSound(PieceSoundPack.Action.PlayerSpeak); } }

        public Player player;
        public HintEngine hintEngine;
        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;
        private List<PieceCreationData> creationDataList;
        public Dictionary<string, Tracking> trackingQueue;
        public List<Cell> plantCellsQueue;
        public List<Sprite> plantSpritesQueue;
        public List<Sprite> nonPlantSpritesQueue;
        public Dictionary<int, List<WorldEvent>> eventQueue;
        public Grid grid;
        public int currentFrame;
        public int currentUpdate; // can be used to measure time elapsed on island
        public int updateMultiplier;
        public readonly IslandClock islandClock;
        public string debugText;
        public int processedNonPlantsCount;
        public int processedPlantsCount;
        private readonly List<Sprite> blockingLightSpritesList;
        private readonly List<Sprite> lightSprites;
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
        public bool CanProcessMoreNonPlantsNow { get { return UpdateTimeElapsed.Milliseconds <= 7 * this.updateMultiplier; } }
        public bool CanProcessMorePlantsNow { get { return UpdateTimeElapsed.Milliseconds <= 9 * this.updateMultiplier; } }
        public bool CanProcessAnyStateMachineNow { get { return !this.plantsProcessing || UpdateTimeElapsed.Milliseconds <= 9 * this.updateMultiplier; } }

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

                int maxDarknessWidth = Math.Min((int)(SonOfRobinGame.graphics.PreferredBackBufferWidth * 1.2f), 2000) / Preferences.DarknessResolution;
                int maxDarknessHeight = Math.Min((int)(SonOfRobinGame.graphics.PreferredBackBufferHeight * 1.2f), 1500) / Preferences.DarknessResolution;

                return new Vector2((float)extendedViewRect.Width / (float)maxDarknessWidth, (float)extendedViewRect.Height / (float)maxDarknessHeight);
            }
        }

        public World(int width, int height, int seed, int resDivider, bool addAgressiveAnimals, int initialMaxAnimalsMultiplier, bool playerFemale, Object saveGameData = null, bool demoMode = false) :
              base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.QuitLoading, tipsLayout: ControlTips.TipsLayout.QuitLoading)
        {
            this.demoMode = demoMode;
            this.playerFemale = playerFemale;
            this.cineMode = false;
            this.BuildMode = false;
            this.spectatorMode = false;
            if (this.demoMode)
            {
                this.InputType = InputTypes.None;
                this.soundActive = false;
            }
            this.saveGameData = saveGameData;
            this.createMissingPiecesOutsideCamera = false;
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
            this.islandClock = this.saveGameData == null ? new IslandClock(0) : new IslandClock();

            this.noise = new FastNoiseLite(this.seed);

            this.width = width;
            this.height = height;
            this.viewParams.Width = width; // it does not need to be updated, because world size is constant
            this.viewParams.Height = height; // it does not need to be updated, because world size is constant

            this.initialMaxAnimalsMultiplier = initialMaxAnimalsMultiplier;
            this.maxAnimalsPerName = (int)(this.width * this.height * 0.00000001 * initialMaxAnimalsMultiplier);
            this.addAgressiveAnimals = addAgressiveAnimals;
            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"maxAnimalsPerName {maxAnimalsPerName}");
            this.creationDataList = PieceCreationData.CreateDataList(addAgressiveAnimals: this.addAgressiveAnimals, maxAnimalsPerName: this.maxAnimalsPerName);

            this.pieceCountByName = new Dictionary<PieceTemplate.Name, int>();
            foreach (PieceTemplate.Name templateName in PieceTemplate.allNames) this.pieceCountByName[templateName] = 0;
            this.pieceCountByClass = new Dictionary<Type, int> { };

            this.trackingQueue = new Dictionary<string, Tracking>();
            this.eventQueue = new Dictionary<int, List<WorldEvent>>();
            this.hintEngine = new HintEngine(world: this);

            this.plantSpritesQueue = new List<Sprite>();
            this.nonPlantSpritesQueue = new List<Sprite>();
            this.plantCellsQueue = new List<Cell>();
            this.processedNonPlantsCount = 0;
            this.processedPlantsCount = 0;
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
            if (saveGameData == null) this.grid = new Grid(world: this, resDivider: resDivider);
            else this.Deserialize(gridOnly: true);

            this.AddLinkedScene(this.map);
            this.AddLinkedScene(this.playerPanel);

            this.solidColorManager = new SolidColorManager(this);
            this.stateMachineTypesManager = new SMTypesManager(this);
            this.craftStats = new CraftStats();
            this.identifiedPieces = new List<PieceTemplate.Name> { PieceTemplate.Name.Hand };
            if (this.demoMode) this.solidColorManager.Add(new SolidColor(color: Color.White, viewOpacity: 0.4f, clearScreen: false, priority: 1));

            SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
        }

        public void CompleteCreation()
        {
            if (InputMapper.IsPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;

                SonOfRobinGame.progressBar.TurnOff(addTransition: true);
                bool menuFound = GetTopSceneOfType(typeof(Menu)) != null;

                this.Remove();
                new TextWindow(text: "Entering the island has been cancelled.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, closingTask: menuFound ? Scheduler.TaskName.Empty : Scheduler.TaskName.OpenMainMenu);
                return;
            };

            if (this.grid.creationInProgress)
            {
                SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
                this.grid.ProcessNextCreationStage();
                return;
            }

            if (this.saveGameData == null && this.initialPiecesCreationFramesLeft > 0)
            {
                if (this.demoMode) CreateMissingPieces(initialCreation: true, outsideCamera: false, multiplier: 1f);
                else
                {
                    string seedText = String.Format("{0:0000}", this.seed);

                    SonOfRobinGame.progressBar.TurnOn(
                    curVal: initialPiecesCreationFramesTotal - this.initialPiecesCreationFramesLeft,
                    maxVal: initialPiecesCreationFramesTotal,
                    text: $"preparing island\nseed {seedText}\n{this.width} x {this.height}\npopulating...");

                    if (this.initialPiecesCreationFramesLeft != initialPiecesCreationFramesTotal) // first iteration should only display progress bar
                    {
                        bool piecesCreated = CreateMissingPieces(initialCreation: true, maxAmountToCreateAtOnce: (uint)(300000 / initialPiecesCreationFramesTotal), outsideCamera: false, multiplier: 1f, addToDoNotCreateList: false);
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
                    PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.player.sprite.position, templateName: PieceTemplate.Name.CrateStarting, closestFreeSpot: true);
                    PieceTemplate.CreateAndPlaceOnBoard(world: this, position: this.player.sprite.position, templateName: PieceTemplate.Name.PredatorRepellant, closestFreeSpot: true);
                }
            }
            else this.Deserialize(gridOnly: false);

            if (!this.demoMode)
            {
                this.camera.TrackPiece(trackedPiece: this.player, moveInstantly: true);
                this.UpdateViewParams();
                this.camera.Update(cameraCorrection: Vector2.Zero); // to render cells in camera view correctly
                Inventory.SetLayout(newLayout: Inventory.Layout.Toolbar, player: this.player);
            }

            this.CreateNewDarknessMask();
            this.grid.LoadAllTexturesInCameraView();
            if (!this.demoMode) this.map.ForceRender();

            SonOfRobinGame.game.IsFixedTimeStep = Preferences.FrameSkip;

            if (!this.demoMode && newGameStarted) this.hintEngine.ShowGeneralHint(type: HintEngine.Type.CineIntroduction, ignoreDelay: true);
            GC.Collect();
        }

        public override void Remove()
        {
            Sound.StopAll();
            base.Remove();
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
            this.islandClock.Initialize((int)headerData["clockTimeElapsed"]);
            this.TimePlayed = (TimeSpan)headerData["TimePlayed"];
            this.mapEnabled = (bool)headerData["MapEnabled"];
            this.maxAnimalsPerName = (int)headerData["maxAnimalsPerName"];
            this.addAgressiveAnimals = (bool)headerData["addAgressiveAnimals"];
            this.doNotCreatePiecesList = (List<PieceTemplate.Name>)headerData["doNotCreatePiecesList"];
            this.discoveredRecipesForPieces = (List<PieceTemplate.Name>)headerData["discoveredRecipesForPieces"];
            this.stateMachineTypesManager.Deserialize((Dictionary<string, Object>)headerData["stateMachineTypesManager"]);
            this.craftStats.Deserialize((Dictionary<string, Object>)headerData["craftStats"]);
            this.identifiedPieces = (List<PieceTemplate.Name>)headerData["identifiedPieces"];

            // deserializing hints

            var hintsData = (Dictionary<string, Object>)saveGameDataDict["hints"];
            this.hintEngine.Deserialize(hintsData);

            // deserializing pieces

            var pieceDataBag = (ConcurrentBag<Object>)saveGameDataDict["pieces"];
            var piecesByID = new Dictionary<string, BoardPiece> { };
            var animalsByTargetID = new Dictionary<string, BoardPiece> { };

            foreach (Dictionary<string, Object> pieceData in pieceDataBag)
            {
                // repeated in StorageSlot

                PieceTemplate.Name templateName = (PieceTemplate.Name)pieceData["base_name"];

                bool female = false;
                bool randomSex = true;

                if (pieceData.ContainsKey("base_female"))
                {
                    randomSex = false;
                    female = (bool)pieceData["base_female"];
                }

                var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this, position: new Vector2((float)pieceData["sprite_positionX"], (float)pieceData["sprite_positionY"]), templateName: templateName, female: female, randomSex: randomSex, ignoreCollisions: true, id: (string)pieceData["base_id"]);
                if (!newBoardPiece.sprite.IsOnBoard) throw new ArgumentException($"{newBoardPiece.name} could not be placed correctly.");

                newBoardPiece.Deserialize(pieceData: pieceData);

                if (templateName == PieceTemplate.Name.Player)
                {
                    this.player = (Player)newBoardPiece;
                    this.camera.TrackPiece(trackedPiece: this.player, moveInstantly: true);
                }

                piecesByID[newBoardPiece.id] = newBoardPiece;

                if (newBoardPiece.GetType() == typeof(Animal) && pieceData["animal_target_id"] != null)
                { animalsByTargetID[(string)pieceData["animal_target_id"]] = newBoardPiece; }
            }

            foreach (var kvp in animalsByTargetID)
            {
                Animal newAnimal = (Animal)kvp.Value;
                if (piecesByID.ContainsKey(kvp.Key)) newAnimal.target = piecesByID[kvp.Key];
            }

            // deserializing tracking

            var trackingDataList = (List<Object>)saveGameDataDict["tracking"];
            foreach (Dictionary<string, Object> trackingData in trackingDataList)
            {
                Tracking.Deserialize(world: this, trackingData: trackingData, piecesByID: piecesByID);
            }

            // deserializing planned events

            var eventDataList = (List<Object>)saveGameDataDict["events"];
            foreach (Dictionary<string, Object> eventData in eventDataList)
            {
                WorldEvent.Deserialize(world: this, eventData: eventData, piecesByID: piecesByID);
            }

            // finalizing

            this.saveGameData = null;

            MessageLog.AddMessage(msgType: MsgType.User, message: "Game has been loaded.", color: Color.Cyan);
        }


        private BoardPiece PlacePlayer()
        {
            for (int tryIndex = 0; tryIndex < 65535; tryIndex++)
            {
                player = (Player)PieceTemplate.CreateAndPlaceOnBoard(world: this, position: new Vector2(random.Next(0, this.width), random.Next(0, this.height)), templateName: PieceTemplate.Name.Player, randomSex: false, female: this.playerFemale);
                if (player.sprite.IsOnBoard)
                {
                    player.sprite.orientation = Sprite.Orientation.up;
                    player.sprite.CharacterStand();
                    player.sprite.allowedFields.RemoveTerrain(TerrainName.Danger); // player should be spawned in a safe place, but able to go everywhere afterwards
                    return player;
                }
            }

            throw new DivideByZeroException("Cannot place player sprite.");
        }
        public bool CreateMissingPieces(bool initialCreation, uint maxAmountToCreateAtOnce = 300000, bool outsideCamera = false, float multiplier = 1.0f, bool clearDoNotCreateList = false, bool addToDoNotCreateList = true)
        {
            if (clearDoNotCreateList) doNotCreatePiecesList.Clear();

            if (!initialCreation && !this.CanProcessMorePlantsNow) return false;

            Vector2 notReallyUsedPosition = new Vector2(-100, -100); // -100, -100 will be converted to a random position on the map - needed for effective creation of new sprites 
            int minPieceAmount = Math.Max(Convert.ToInt32((long)width * (long)height / 300000 * multiplier), 0); // 300000

            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (PieceCreationData creationData in creationDataList)
            {
                if (doNotCreatePiecesList.Contains(creationData.name) || creationData.doNotReplenish && !initialCreation) continue;

                int minAmount = Math.Max((int)(minPieceAmount * creationData.multiplier), 4);
                if (creationData.maxAmount > -1) minAmount = Math.Min(minAmount, creationData.maxAmount);

                int amountToCreate = Math.Max(minAmount - this.pieceCountByName[creationData.name], 0);
                if (amountToCreate > 0) amountToCreateByName[creationData.name] = amountToCreate;
            }

            if (amountToCreateByName.Keys.ToList().Count == 0) return false;

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

                    var newBoardPiece = PieceTemplate.CreateAndPlaceOnBoard(world: this, position: notReallyUsedPosition, templateName: pieceName);
                    if (newBoardPiece.sprite.IsOnBoard)
                    {
                        if (initialCreation && newBoardPiece.GetType() == typeof(Plant) && this.random.Next(2) == 0)
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
            this.createMissingPiecesOutsideCamera = false;
            return piecesCreated > 0;
        }

        public void UpdateViewParams()
        {
            this.camera.Update(cameraCorrection: this.analogCameraCorrection);
            this.camera.SetViewParams(this);

            // width and height are set once in constructor
        }

        public override void Update(GameTime gameTime)
        {
            if (this.worldCreationInProgress)
            {
                this.CompleteCreation();
                return;
            }

            if (this.demoMode) this.camera.SetZoom(zoom: 2f, setInstantly: true);

            this.ProcessInput();
            this.UpdateViewParams();
            //SoundEffect.DistanceScale = this.camera.viewRect.Width * 0.065f;

            this.grid.UnloadTexturesIfMemoryLow(this.camera);
            this.grid.LoadClosestTextureInCameraView(camera: this.camera, visitedByPlayerOnly: false);

            if (this.demoMode) this.camera.TrackLiveAnimal(fluidMotion: true);

            bool createMissingPieces = this.currentUpdate % 200 == 0 && Preferences.debugCreateMissingPieces && !this.CineMode && !this.BuildMode;
            if (createMissingPieces) this.CreateMissingPieces(initialCreation: false, maxAmountToCreateAtOnce: 100, outsideCamera: true, multiplier: 0.1f);

            for (int i = 0; i < this.updateMultiplier; i++)
            {
                Tracking.ProcessTrackingQueue(this);
                WorldEvent.ProcessQueue(this);
                if (!this.BuildMode) this.UpdateAllAnims();

                if (this.player != null) this.ProcessOneNonPlant(this.player);

                this.StateMachinesProcessCameraView();

                if (!createMissingPieces)
                {
                    this.StateMachinesProcessNonPlantQueue();
                    this.plantsProcessing = true;
                    this.StateMachinesProcessPlantQueue();
                    this.plantsProcessing = false;
                }

                this.currentUpdate++;
                this.islandClock.Advance();
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

            float zoomOutForce = InputMapper.TriggerForce(InputMapper.Action.WorldCameraZoomOut);

            // camera zoom control (to keep the current zoom level, when other scene is above the world, that scene must block updates below)

            if (!this.CineMode)
            {
                if (!Preferences.CanZoomOut)
                {
                    // zoomOutForce combined with worldScale is considered cheating and therefore not allowed

                    if (zoomOutForce > 0.4f) this.hintEngine.ShowGeneralHint(type: HintEngine.Type.ZoomOutLocked, ignoreDelay: true); // to avoid showing the message at minimal (accidental, unintended) trigger pressure

                    zoomOutForce = 0;
                }
                else VirtButton.ButtonHighlightOnNextFrame(VButName.ZoomOut);

                this.camera.SetZoom(zoom: 1f / (1f + zoomOutForce), zoomSpeedMultiplier: 3f);
            }


            if (!this.player.alive || this.player.activeState != BoardPiece.State.PlayerControlledWalking) return;

            if (!this.player.buffEngine.HasBuff(BuffEngine.BuffType.Sprint) && !this.player.buffEngine.HasBuff(BuffEngine.BuffType.SprintCooldown))
            {
                VirtButton.ButtonHighlightOnNextFrame(VButName.Sprint);
                ControlTips.TipHighlightOnNextFrame(tipName: "sprint");
                if (InputMapper.HasBeenPressed(InputMapper.Action.WorldSprintToggle)) this.player.buffEngine.AddBuff(buff: new BuffEngine.Buff(type: BuffEngine.BuffType.Sprint, autoRemoveDelay: 3 * 60, value: 2f), world: this);
            }

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
                if (!this.hintEngine.ShowGeneralHint(HintEngine.Type.MapNegative)) new TextWindow(text: "I don't have map equipped.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: this.DialogueSound);
                return;
            }

            if (!this.map.CheckIfPlayerCanReadTheMap(showMessage: true)) return;

            if (this.map.transManager.HasAnyTransition || this.map.transManager.HasAnyTransition) return;

            this.map.SwitchToNextMode();
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
            if ((SonOfRobinGame.lastUpdateDelay > 20 || SonOfRobinGame.lastDrawDelay > 20) && this.updateMultiplier == 1) return;

            if (this.nonPlantSpritesQueue.Count == 0)
            {
                this.nonPlantSpritesQueue = this.grid.GetSpritesFromAllCells(groupName: Cell.Group.StateMachinesNonPlants).OrderBy(sprite => sprite.boardPiece.lastFrameSMProcessed).ToList();
                return;
            }

            this.processedNonPlantsCount = 0;

            while (true)
            {
                if (nonPlantSpritesQueue.Count == 0) return;

                BoardPiece currentNonPlant = this.nonPlantSpritesQueue[0].boardPiece;
                this.nonPlantSpritesQueue.RemoveAt(0);

                //if (currentNonPlant.name != PieceTemplate.Name.Player) MessageLog.AddMessage(msgType: MsgType.User, message: $"{this.currentUpdate} processing '{currentNonPlant.readableName}'");

                this.ProcessOneNonPlant(currentNonPlant);
                this.processedNonPlantsCount++;

                if (!this.CanProcessMoreNonPlantsNow)
                {
                    if (UpdateTimeElapsed.Milliseconds > 13 && this.updateMultiplier == 1) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Non plants: update time exceeded - {UpdateTimeElapsed.Milliseconds}ms.");

                    return;
                }
            }
        }

        private void ProcessOneNonPlant(BoardPiece piece)
        {
            if (piece.maxAge > 0) piece.GrowOlder();

            if (piece.GetType() == typeof(Animal))
            {
                Animal nonPlant = (Animal)piece;
                if (nonPlant.isPregnant && nonPlant.alive) nonPlant.pregnancyFramesLeft = Math.Max(nonPlant.pregnancyFramesLeft - 1, 0);
                if (this.currentUpdate % 10 == 0) nonPlant.ExpendEnergy(1);

                if (nonPlant.alive && (nonPlant.hitPoints <= 0 || nonPlant.efficiency == 0 || nonPlant.currentAge >= nonPlant.maxAge))
                {
                    nonPlant.Kill();
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


            while (true)
            {
                if (plantSpritesQueue.Count == 0) return;

                Plant currentPlant = (Plant)this.plantSpritesQueue[0].boardPiece;
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
            if (this.BuildMode) throw new ArgumentException("Is already in build mode.");

            this.BuildMode = true;

            this.touchLayout = TouchLayout.WorldBuild;
            this.tipsLayout = ControlTips.TipsLayout.WorldBuild;
            this.player.activeState = BoardPiece.State.PlayerControlledBuilding;

            this.stateMachineTypesManager.DisableMultiplier();
            this.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(Player) }, everyFrame: true, nthFrame: true);
            this.islandClock.Pause();

            Scene craftMenu = GetTopSceneOfType(typeof(Menu));
            craftMenu?.MoveToBottom(); // there is no craft menu if planting
            SonOfRobinGame.hintWindow.TurnOff();

            this.player.recipeToBuild = recipe;
            this.player.simulatedPieceToBuild = PieceTemplate.CreateAndPlaceOnBoard(templateName: recipe.pieceToCreate, world: this, position: this.player.sprite.position, ignoreCollisions: true); // "template" piece should be placed - collisions doesn't matter...
            this.player.simulatedPieceToBuild.sprite.MoveToClosestFreeSpot(this.player.sprite.position); // ...but the starting position should be correct for building, if possible
            this.player.buildDurationForOneFrame = recipe.GetRealDuration(this.craftStats) / buildDuration;
            this.player.buildFatigueForOneFrame = recipe.GetRealFatigue(this.craftStats) / buildDuration;
        }

        public void BuildPiece()
        {
            if (!this.BuildMode) throw new ArgumentException("Is not in build mode.");

            this.touchLayout = TouchLayout.Empty;
            this.tipsLayout = ControlTips.TipsLayout.Empty;
            this.player.activeState = BoardPiece.State.PlayerWaitForBuilding;

            BoardPiece builtPiece = this.player.recipeToBuild.TryToProducePieces(player: this.player, showMessages: false)[0];

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

            this.player.simulatedPieceToBuild.Destroy();
            this.player.simulatedPieceToBuild = null;

            builtPiece.sprite.opacity = 0f;
            builtPiece.sprite.opacityFade = new OpacityFade(sprite: builtPiece.sprite, destOpacity: 1f, duration: buildDuration);

            new WorldEvent(eventName: WorldEvent.EventName.FinishBuilding, world: this, delay: buildDuration, boardPiece: null);
            new WorldEvent(eventName: WorldEvent.EventName.PlaySoundByName, world: this, delay: buildDuration, boardPiece: null, eventHelper: plantMode ? SoundData.Name.MovingPlant : SoundData.Name.Chime);
            new WorldEvent(eventName: WorldEvent.EventName.YieldDropDebris, world: this, delay: buildDuration, boardPiece: null, eventHelper: debrisYield);
        }

        public void ExitBuildMode(bool restoreCraftMenu, bool showCraftMessages = false)
        {
            if (!this.BuildMode) throw new ArgumentException("Is not in build mode.");

            this.touchLayout = TouchLayout.WorldMain;
            this.tipsLayout = ControlTips.TipsLayout.WorldMain;
            this.player.activeState = BoardPiece.State.PlayerControlledWalking;

            player.world.stateMachineTypesManager.DisableMultiplier();
            player.world.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
            this.islandClock.Resume();

            Scene craftMenu = GetBottomSceneOfType(typeof(Menu));
            if (restoreCraftMenu) craftMenu?.MoveToTop(); // there is no craft menu if planting
            else craftMenu?.Remove();

            if (showCraftMessages) this.player.recipeToBuild.UnlockNewRecipesAndShowSummary(this);

            this.player.recipeToBuild = null;
            if (this.player.simulatedPieceToBuild != null)
            {
                this.player.simulatedPieceToBuild.Destroy();
                this.player.simulatedPieceToBuild = null;
            }

            this.BuildMode = false;
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

        public override void RenderToTarget()
        {
            // preparing a list of sprites, that cast shadows

            this.blockingLightSpritesList.Clear();
            this.lightSprites.Clear();

            if ((Preferences.drawSunShadows && AmbientLight.SunLightData.CalculateSunLight(this.islandClock.IslandDateTime).sunShadowsColor != Color.Transparent) ||
                (Preferences.drawShadows && AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime).darknessColor != Color.Transparent))
            {
                this.blockingLightSpritesList.AddRange(this.camera.GetVisibleSprites(groupName: Cell.Group.ColMovement).OrderBy(o => o.gfxRect.Bottom));
            }

            var lightSpritesToAdd = this.UpdateDarknessMask(blockingLightSpritesList: blockingLightSpritesList); // Has to be called first, because calling SetRenderTarget() after any draw will wipe the screen black.
            this.lightSprites.AddRange(lightSpritesToAdd);
        }

        public override void Draw()
        {
            // drawing blue background (to ensure drawing even if screen is larger than map)
            SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            if (this.worldCreationInProgress) return;

            // drawing background
            this.grid.DrawBackground(camera: this.camera);

            // drawing sprites
            var noOfDisplayedSprites = this.grid.DrawSprites(camera: this.camera, blockingLightSpritesList: this.blockingLightSpritesList);

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
            // searching for light sources

            var lightSprites = this.camera.GetVisibleSprites(groupName: Cell.Group.LightSource).OrderBy(o => o.frame.layer).ThenBy(o => o.gfxRect.Bottom).ToList();

            // returning if darkness is transparent

            AmbientLight.AmbientLightData ambientLightData = AmbientLight.CalculateLightAndDarknessColors(this.islandClock.IslandDateTime);

            if (ambientLightData.darknessColor == Color.Transparent)
            {
                SetRenderTarget(this.darknessMask);
                SonOfRobinGame.spriteBatch.Begin();
                SonOfRobinGame.graphicsDevice.Clear(ambientLightData.darknessColor);
                SonOfRobinGame.spriteBatch.End();

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
                    if (SonOfRobinGame.tempShadowMaskList.Count - 1 < tempShadowMaskIndex) SonOfRobinGame.tempShadowMaskList.Add(new RenderTarget2D(graphicsDevice: SonOfRobinGame.graphicsDevice, width: SonOfRobinGame.lightSphere.Width, height: SonOfRobinGame.lightSphere.Height));

                    Rectangle lightRect = lightSprite.lightEngine.Rect;
                    lightSprite.lightEngine.tempShadowMaskIndex = tempShadowMaskIndex;

                    // drawing shadows onto shadow mask

                    RenderTarget2D tempShadowMask = SonOfRobinGame.tempShadowMaskList[tempShadowMaskIndex];

                    SetRenderTarget(tempShadowMask);
                    SonOfRobinGame.spriteBatch.Begin(transformMatrix: this.TransformMatrix);
                    SonOfRobinGame.graphicsDevice.Clear(Color.Black);
                    SonOfRobinGame.spriteBatch.End();

                    Matrix scaleMatrix = Matrix.CreateScale( // to match drawing size with rect size (light texture size differs from light rect size)
                        (float)tempShadowMask.Width / (float)lightRect.Width,
                        (float)tempShadowMask.Height / (float)lightRect.Height,
                        1f);

                    // first pass - drawing shadows
                    SonOfRobinGame.spriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlend);
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        if (shadowSprite == lightSprite || !lightSprite.lightEngine.castShadows || !lightRect.Intersects(shadowSprite.gfxRect)) continue;

                        float shadowAngle = Helpers.GetAngleBetweenTwoPoints(start: lightSprite.gfxRect.Center, end: shadowSprite.position);

                        Sprite.DrawShadow(color: Color.White, shadowSprite: shadowSprite, lightPos: lightSprite.position, shadowAngle: shadowAngle, drawOffsetX: -lightRect.X, drawOffsetY: -lightRect.Y);
                    }
                    SonOfRobinGame.spriteBatch.End();

                    // second pass - erasing shadow from original sprites' position
                    SonOfRobinGame.spriteBatch.Begin(transformMatrix: scaleMatrix, blendState: shadowBlendRedraw);
                    foreach (Sprite shadowSprite in blockingLightSpritesList)
                    {
                        // the lightSprite should be also redrawn, to avoid being overdrawn with any shadow
                        if (lightRect.Intersects(shadowSprite.gfxRect)) shadowSprite.DrawRoutine(calculateSubmerge: true, offsetX: -lightRect.X, offsetY: -lightRect.Y);
                    }
                    SonOfRobinGame.spriteBatch.End();

                    // drawing light on top of shadows

                    SonOfRobinGame.spriteBatch.Begin(blendState: lightBlend);
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.lightSphere, tempShadowMask.Bounds, Color.White);
                    SonOfRobinGame.spriteBatch.End();

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
            SonOfRobinGame.spriteBatch.Begin(blendState: darknessMaskBlend);
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
