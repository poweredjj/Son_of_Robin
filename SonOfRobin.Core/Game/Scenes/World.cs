using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
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

        public bool creationInProgress;
        public readonly DateTime creationStart;
        public DateTime creationEnd;
        public TimeSpan creationDuration;
        public readonly bool demoMode;
        public bool inTransitionPlayed;

        private Object saveGameData;
        public bool piecesLoadingMode; // allows to precisely place pieces during loading a saved game
        public bool createMissinPiecesOutsideCamera;
        private DateTime lastSaved;

        public readonly int seed;
        public FastNoiseLite noise;
        public readonly Random random;
        public readonly int width;
        public readonly int height;
        public readonly Camera camera;
        public MapMode mapMode;
        public List<MapMode> mapCycle;
        public readonly Map mapBig;
        public readonly Map mapSmall;

        public Player player;

        public HintEngine hintEngine;

        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;
        public Dictionary<string, Tracking> trackingQueue;
        public List<Cell> plantCellsQueue;
        public List<Sprite> plantSpritesQueue;
        public Dictionary<int, List<WorldEvent>> eventQueue;
        public Grid grid;
        public int currentFrame;
        public int currentUpdate;
        public int updateMultiplier;
        public string debugText;
        public int processedPlantsCount;
        public readonly List<PieceTemplate.Name> doNotCreatePiecesList;

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
        public World(int width, int height, int seed = -1, Object saveGameData = null, bool demoMode = false) :
              base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.WorldMain)
        {
            this.demoMode = demoMode;
            if (this.demoMode) this.InputType = InputTypes.None;
            this.inTransitionPlayed = false;
            this.saveGameData = saveGameData;
            this.piecesLoadingMode = saveGameData != null;
            this.createMissinPiecesOutsideCamera = false;
            this.lastSaved = DateTime.Now;
            this.creationInProgress = true;
            this.creationStart = DateTime.Now;
            if (SonOfRobinGame.platform == Platform.Mobile) SonOfRobinGame.KeepScreenOn = true;

            if (seed == -1)
            {
                Random oneTimeRandom = new Random();
                seed = oneTimeRandom.Next(1, 100000);
            }

            this.seed = seed;
            this.random = new Random(seed);
            this.currentFrame = 0;
            this.currentUpdate = 0;
            this.updateMultiplier = 1;

            this.noise = new FastNoiseLite(this.seed);

            this.width = width;
            this.height = height;

            this.pieceCountByName = new Dictionary<PieceTemplate.Name, int>();
            foreach (PieceTemplate.Name templateName in (PieceTemplate.Name[])Enum.GetValues(typeof(PieceTemplate.Name)))
            { this.pieceCountByName[templateName] = 0; }
            this.pieceCountByClass = new Dictionary<Type, int> { };

            this.trackingQueue = new Dictionary<string, Tracking>();
            this.eventQueue = new Dictionary<int, List<WorldEvent>>();
            this.hintEngine = new HintEngine(world: this);

            this.plantSpritesQueue = new List<Sprite>();
            this.plantCellsQueue = new List<Cell>();
            this.processedPlantsCount = 0;
            this.doNotCreatePiecesList = new List<PieceTemplate.Name> { };
            this.camera = new Camera(this);
            this.camera.TrackCoords(new Vector2(0, 0));
            this.mapMode = MapMode.None;
            this.mapCycle = SonOfRobinGame.platform == Platform.Mobile ? new List<MapMode> { MapMode.None, MapMode.Big } : new List<MapMode> { MapMode.None, MapMode.Small, MapMode.Big };
            this.mapBig = new Map(world: this, fullScreen: true, touchLayout: TouchLayout.Map);
            this.mapSmall = new Map(world: this, fullScreen: false, touchLayout: TouchLayout.Empty);
            this.grid = new Grid(this);
            this.rightTriggerPreviousFrame = 0f;

            SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
        }

        public override void Remove()
        {
            this.mapBig.Remove(); // map is a separate scene - need to be removed as well
            this.mapSmall.Remove(); // map is a separate scene - need to be removed as well
            base.Remove();
        }

        public void CompleteCreation()
        {
            if (this.grid.creationInProgress)
            {
                SonOfRobinGame.game.IsFixedTimeStep = false; // speeds up the creation process
                this.grid.RunNextCreationStage();
            }

            else
            {
                ProgressBar.Hide();
                this.touchLayout = TouchLayout.WorldMain;
                this.creationInProgress = false;
                if (SonOfRobinGame.platform == Platform.Mobile) SonOfRobinGame.KeepScreenOn = false;
                this.creationEnd = DateTime.Now;
                this.creationDuration = this.creationEnd - this.creationStart;
                Craft.PopulateAllCategories();

                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"World creation time: {creationDuration:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

                if (this.saveGameData is null)
                {
                    CreateMissingPieces(outsideCamera: false, multiplier: 1.0f);

                    this.currentFrame = 0;
                    this.currentUpdate = 0;
                    if (!this.demoMode) this.player = (Player)this.PlacePlayer();
                    if (this.demoMode) this.camera.TrackLiveAnimal();
                }

                else
                { this.Deserialize(); }

                if (!this.demoMode)
                {
                    this.camera.TrackPiece(trackedPiece: this.player, fluidMotion: false);
                    this.UpdateViewParams(manualScale: 1f);
                    this.camera.Update(); // to render cells in camera view correctly
                    SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.player);
                }

                this.grid.LoadAllTexturesInCameraView();
                if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
            }

        }

        private void Deserialize()
        {
            var saveGameDataDict = (Dictionary<string, Object>)this.saveGameData;

            // deserializing header

            var headerData = (Dictionary<string, Object>)saveGameDataDict["header"];

            this.currentFrame = (int)headerData["currentFrame"];
            this.currentUpdate = (int)headerData["currentUpdate"];
            this.hintEngine.disabledHints = (List<HintEngine.Type>)headerData["disabledHints"];

            // deserializing pieces

            var piecesByOldId = new Dictionary<string, BoardPiece> { }; // needed to match old IDs to new pieces (with new, different IDs)

            var pieceDataList = (List<Object>)saveGameDataDict["pieces"];

            var newPiecesByOldTargetIds = new Dictionary<string, BoardPiece> { };


            foreach (Dictionary<string, Object> pieceData in pieceDataList)
            {
                // repeated in StorageSlot

                PieceTemplate.Name templateName = (PieceTemplate.Name)pieceData["base_name"];

                bool female = pieceData.ContainsKey("animal_female") && (bool)pieceData["animal_female"];

                var newBoardPiece = PieceTemplate.CreateOnBoard(world: this, position: new Vector2((float)pieceData["sprite_positionX"], (float)pieceData["sprite_positionY"]), templateName: templateName, female: female);
                newBoardPiece.Deserialize(pieceData);
                piecesByOldId[(string)pieceData["base_old_id"]] = newBoardPiece;

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
                if (piecesByOldId.ContainsKey(kvp.Key)) newAnimal.target = piecesByOldId[kvp.Key];
            }

            // deserializing tracking

            var trackingDataList = (List<Object>)saveGameDataDict["tracking"];
            foreach (Dictionary<string, Object> trackingData in trackingDataList)
            { Tracking.Deserialize(world: this, trackingData: trackingData, piecesByOldId: piecesByOldId); }

            // deserializing planned events

            var eventDataList = (List<Object>)saveGameDataDict["events"];
            foreach (Dictionary<string, Object> eventData in eventDataList)
            { WorldEvent.Deserialize(world: this, eventData: eventData, piecesByOldId: piecesByOldId); }

            // finalizing

            this.saveGameData = null;
            this.piecesLoadingMode = false;

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Game has been loaded.", color: Color.Cyan);
        }

        public void AutoSave(bool force = false)
        {
            if (this.demoMode || (!force && this.currentUpdate % 600 != 0) || !this.player.alive) return;
            if (this.player.activeState != BoardPiece.State.PlayerControlledWalking) return;
            if (GetTopSceneOfType(typeof(Menu)) != null) return;
            Scene invScene = GetTopSceneOfType(typeof(Inventory));
            if (invScene != null)
            {
                Inventory inventory = (Inventory)invScene;
                if (inventory.layout != Inventory.Layout.SingleBottom) return;
            }

            TimeSpan timeSinceLastSave = DateTime.Now - this.lastSaved;
            if (!force && timeSinceLastSave < TimeSpan.FromMinutes(Preferences.autoSaveDelayMins)) return;

            string timeElapsedTxt = timeSinceLastSave.ToString("mm\\:ss");
            string message = force ? "Autosaving before returning to menu..." : $"{timeElapsedTxt} elapsed - autosaving...";

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: message, color: Color.LightBlue);

            string saveSlotName = "0";

            if (force)
            { this.Save(saveSlotName: saveSlotName, showMessage: false); }
            else
            {
                var saveParams = new Dictionary<string, Object> { { "saveSlotName", saveSlotName }, { "showMessage", false } };
                new Scheduler.Task(menu: null, actionName: Scheduler.ActionName.SaveGame, executeHelper: saveParams);
            }
        }

        public void Save(string saveSlotName, bool showMessage = false)
        {
            // preparing save directory

            string saveDir = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);

            if (Directory.Exists(saveDir))
            {
                System.IO.DirectoryInfo dirInfo = new DirectoryInfo(saveDir);
                foreach (FileInfo file in dirInfo.GetFiles()) file.Delete();
            }
            else
            { Directory.CreateDirectory(saveDir); }

            // saving header data

            var headerData = new Dictionary<string, Object>
            {
                {"seed", this.seed },
                {"width", this.width },
                {"height", this.height },
                {"currentFrame", this.currentFrame },
                {"currentUpdate", this.currentUpdate },
                {"disabledHints", this.hintEngine.disabledHints },
                {"realDateTime", DateTime.Now },
                {"saveVersion", SaveManager.saveVersion },
            };

            string headerPath = Path.Combine(saveDir, "header.sav");
            LoaderSaver.Save(path: headerPath, savedObj: headerData);

            // saving pieces data
            uint maxPiecesInPackage = 10000; // using small piece packages lowers ram usage during writing binary files
            var pieceDataPackages = new Dictionary<int, List<Object>> { };
            int currentPackageNo = 0;
            pieceDataPackages[currentPackageNo] = new List<object> { };

            var allSprites = this.grid.GetAllSprites(Cell.Group.All);
            foreach (Sprite sprite in allSprites)
            {
                if (sprite.boardPiece.exists && sprite.boardPiece.serialize)
                {
                    pieceDataPackages[currentPackageNo].Add(sprite.boardPiece.Serialize());
                    if (pieceDataPackages[currentPackageNo].Count >= maxPiecesInPackage)
                    {
                        currentPackageNo++;
                        pieceDataPackages[currentPackageNo] = new List<object> { };
                    }
                }
            }

            foreach (var kvp in pieceDataPackages)
            {
                string piecesPath = Path.Combine(saveDir, $"pieces_{kvp.Key}.sav");
                LoaderSaver.Save(path: piecesPath, savedObj: kvp.Value);
            }

            // saving tracking data

            var trackingData = new List<Object> { };
            foreach (Tracking tracking in this.trackingQueue.Values)
            { trackingData.Add(tracking.Serialize()); }

            string trackingPath = Path.Combine(saveDir, "tracking.sav");
            LoaderSaver.Save(path: trackingPath, savedObj: trackingData);

            // saving planned event data

            var eventData = new List<Object> { };
            foreach (var eventList in this.eventQueue.Values)
            {
                foreach (var plannedEvent in eventList)
                {
                    eventData.Add(plannedEvent.Serialize());
                }
            }
            string eventPath = Path.Combine(saveDir, "event.sav");
            LoaderSaver.Save(path: eventPath, savedObj: eventData);

            if (showMessage) new TextWindow(text: "Game has been saved.", textColor: Color.White, bgColor: Color.DarkGreen, useTransition: false, animate: false);
            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Game saved in slot {saveSlotName}.", color: Color.LightBlue);

            this.lastSaved = DateTime.Now;
        }

        public static World Load(string saveSlotName)
        {
            string saveDir = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);

            if (!Directory.Exists(saveDir))
            {
                new TextWindow(text: $"Save slot {saveSlotName} is empty.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
                return null;
            }

            // loading header

            string headerPath = Path.Combine(saveDir, "header.sav");
            var headerData = (Dictionary<string, Object>)LoaderSaver.Load(path: headerPath);

            if (headerData is null)
            {
                new TextWindow(text: $"Error while reading save header for slot {saveSlotName}.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
                return null;
            }

            int seed = (int)headerData["seed"];
            int width = (int)headerData["width"];
            int height = (int)headerData["height"];

            // loading pieces

            int currentPackageNo = 0;

            if (!File.Exists(Path.Combine(saveDir, $"pieces_{currentPackageNo}.sav")))
            {
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Error while reading saved pieces for slot {saveSlotName}.", color: Color.Orange);
                return null;
            }

            var piecesData = new List<Object> { };

            while (true)
            {
                if (!File.Exists(Path.Combine(saveDir, $"pieces_{currentPackageNo}.sav"))) break;

                string piecesPath = Path.Combine(saveDir, $"pieces_{currentPackageNo}.sav");
                var packageData = (List<Object>)LoaderSaver.Load(path: piecesPath);
                piecesData.AddRange(packageData);
                currentPackageNo++;
            }

            // loading tracking

            string trackingPath = Path.Combine(saveDir, "tracking.sav");
            if (!File.Exists(trackingPath))
            {
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: "Error while reading tracking data.", color: Color.Orange);
                return null;
            }
            var trackingData = (List<Object>)LoaderSaver.Load(path: trackingPath);

            // loading planned events

            string eventPath = Path.Combine(saveDir, "event.sav");
            if (!File.Exists(eventPath))
            {
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: "Error while reading event data.", color: Color.Orange);
                return null;
            }
            var eventData = (List<Object>)LoaderSaver.Load(path: eventPath);

            // packing all loaded data into dictionary
            var saveGameData = new Dictionary<string, Object> {
                {"header", headerData},
                {"pieces", piecesData},
                {"tracking", trackingData},
                {"events", eventData},
            };

            // creating new world (using header data)

            World loadedWorld = new World(width: width, height: height, seed: seed, saveGameData: saveGameData);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Loading game from slot {saveSlotName} has started...", color: Color.LightBlue);

            return loadedWorld;
        }

        private BoardPiece PlacePlayer()
        {
            for (int tryIndex = 0; tryIndex < 65535; tryIndex++)
            {
                player = (Player)PieceTemplate.CreateOnBoard(world: this, position: new Vector2(random.Next(0, this.width), random.Next(0, this.height)), templateName: PieceTemplate.Name.Player);
                if (player.sprite.placedCorrectly) return player;
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

        public void CreateMissingPieces(uint maxAmountToCreateAtOnce = 300000, bool outsideCamera = false, float multiplier = 1.0f, bool clearDoNotCreateList = false)
        {
            if (clearDoNotCreateList) doNotCreatePiecesList.Clear();

            // preparing a dictionary of pieces to create

            var creationDataList = new List<PieceCreationData>
            {
                new PieceCreationData(name: PieceTemplate.Name.GrassRegular, multiplier: 2.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.GrassDesert, multiplier: 2.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.Rushes, multiplier: 2.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.WaterLily, multiplier: 1.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.FlowersPlain, multiplier: 0.4f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.FlowersMountain, multiplier: 0.1f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.TreeSmall, multiplier: 1.0f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.TreeBig, multiplier: 1.0f, maxAmount: 0),
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
                new PieceCreationData(name: PieceTemplate.Name.Shell, multiplier: 1.5f, maxAmount: 0),
                new PieceCreationData(name: PieceTemplate.Name.Rabbit, multiplier: 0.1f, maxAmount: Animal.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Fox, multiplier: 0.1f, maxAmount: Animal.maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Frog, multiplier: 0.1f, maxAmount: Animal.maxAnimalsPerName),
            };

            Vector2 notReallyUsedPosition = new Vector2(-100, -100); // -100, -100 will be converted to a random position on the map - needed for effective creation of new sprites 
            int minPieceAmount = Math.Max(Convert.ToInt32((long)width * (long)height / 300000 * multiplier), 0); // 300000

            int minAmount;
            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (PieceCreationData creationData in creationDataList)
            {
                if (doNotCreatePiecesList.Contains(creationData.name)) continue;

                minAmount = Math.Max((int)(minPieceAmount * creationData.multiplier), 4);
                int amountToCreate = Math.Max(minAmount - this.pieceCountByName[creationData.name], 0);
                if (creationData.maxAmount > 0) amountToCreate = Math.Min(amountToCreate, creationData.maxAmount);
                if (amountToCreate > 0) amountToCreateByName[creationData.name] = amountToCreate;
            }

            if (amountToCreateByName.Keys.ToList().Count == 0) return;

            // creating pieces

            this.createMissinPiecesOutsideCamera = outsideCamera;
            int piecesCreated = 0;
            int amountLeftToCreate;
            PieceTemplate.Name pieceName;

            foreach (var kvp in amountToCreateByName)
            {
                pieceName = kvp.Key;
                amountLeftToCreate = kvp.Value; // separate from the iterator below (needs to be modified)

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
                        if (!this.doNotCreatePiecesList.Contains(pieceName))
                        {
                            this.doNotCreatePiecesList.Add(pieceName);
                            new WorldEvent(eventName: WorldEvent.EventName.RestorePieceCreation, delay: 15000, world: this, boardPiece: null, eventHelper: pieceName);
                        }
                        break;
                    }
                }

                if (piecesCreated > maxAmountToCreateAtOnce) break;
            }

            if (piecesCreated > 0) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Created {piecesCreated} new pieces.");
            this.createMissinPiecesOutsideCamera = false;
        }

        public void UpdateViewParams(float manualScale = 1f)
        {
            this.viewParams.scaleX = manualScale;
            this.viewParams.scaleY = manualScale;

            float scaleMultiplier = this.demoMode ? 2f : 1f;
            this.viewParams.scaleX *= 1 / (Preferences.worldScale * scaleMultiplier);
            this.viewParams.scaleY *= 1 / (Preferences.worldScale * scaleMultiplier);

            this.camera.Update();
            this.viewParams.posX = this.camera.viewPos.X;
            this.viewParams.posY = this.camera.viewPos.Y;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.creationInProgress)
            {
                this.CompleteCreation();
                return;
            }

            float manualScale = this.ProcessInput();
            this.UpdateViewParams(manualScale: manualScale);

            this.grid.UnloadTexturesIfMemoryLow();
            this.grid.LoadClosestTextureInCameraView();

            if (!this.demoMode && !this.inTransitionPlayed)
            {
                float zoomScale = 0.7f;

                this.AddTransition(new Transition(type: Transition.TransType.In, duration: 30, scene: this, blockInput: false, paramsToChange: new Dictionary<string, float> {
                    { "posX", this.viewParams.posX - (this.camera.ScreenWidth * zoomScale * 0.25f) },
                    { "posY", this.viewParams.posY - (this.camera.ScreenHeight * zoomScale * 0.25f) },
                    { "scaleX", this.viewParams.scaleX * zoomScale },
                    { "scaleY", this.viewParams.scaleY * zoomScale } },
                    pingPongPause: true));
                this.inTransitionPlayed = true;
            }

            if (this.currentUpdate % 100 == 0 && Preferences.debugCreateMissingPieces) this.CreateMissingPieces(maxAmountToCreateAtOnce: 500, outsideCamera: true, multiplier: 0.05f);
            if (this.demoMode) this.camera.TrackLiveAnimal();
            this.AutoSave();

            for (int i = 0; i < this.updateMultiplier; i++)
            {
                WorldEvent.ProcessQueue(this);
                this.UpdateAllAnims();
                this.ProcessActiveStateMachines();
                Tracking.ProcessTrackingQueue(this);
                this.ProcessPlantQueue();
                this.currentUpdate++;
            }

            this.currentFrame++;
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

            if (TouchInput.RightStick.X != 0 || TouchInput.RightStick.Y != 0)
            { this.analogCameraCorrection = new Vector2(TouchInput.RightStick.X / 30, TouchInput.RightStick.Y / 60); }
            else
            {
                this.analogCameraCorrection = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.Circular).ThumbSticks.Right;
                this.analogCameraCorrection = new Vector2(analogCameraCorrection.X * 10, analogCameraCorrection.Y * -10);
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
            if (VirtButton.IsButtonDown(VButName.ZoomOut) || Keyboard.IsPressed(Keys.NumPad1)) manualScale = 2f;

            // shooting mode

            bool triggerPressed = leftTrigger > 0.4f;
            bool rightStickTilted = Math.Abs(analogCameraCorrection.X) > 0.05f || Math.Abs(analogCameraCorrection.Y) > 0.05f;

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
                if (rightStickTilted) actionKeyList.Add(ActionKeys.ShootingMode);
            }
            else
            {
                if ((triggerPressed && rightStickTilted) || Keyboard.IsPressed(Keys.Space)) actionKeyList.Add(ActionKeys.ShootingMode);
            }

            return manualScale;
        }

        private void ProcessActionKeyList()
        {
            if (this.demoMode) return;

            if (this.actionKeyList.Contains(World.ActionKeys.PauseMenu)) MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Pause);

            if (this.player.activeState == BoardPiece.State.PlayerControlledSleep) return;


            if (this.actionKeyList.Contains(World.ActionKeys.FieldCraft))
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CraftBasic);
                return;
            }

            if (this.actionKeyList.Contains(World.ActionKeys.Inventory))
            {
                Scene.SetInventoryLayout(Scene.InventoryLayout.InventoryAndToolbar, player: this.player);
                return;
            }

            if (this.actionKeyList.Contains(World.ActionKeys.MapToggle)) this.ToggleMapMode();

        }

        public void ToggleMapMode()
        {

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

        public void ProcessActiveStateMachines()
        {
            var nonPlantList = this.grid.GetAllSprites(Cell.Group.StateMachinesNonPlants);

            foreach (Sprite sprite in nonPlantList)
            {
                if (sprite.boardPiece.maxAge > 0) sprite.boardPiece.GrowOlder();
                if (sprite.boardPiece.GetType() == typeof(Animal))
                {
                    Animal animal = (Animal)sprite.boardPiece;
                    if (this.currentUpdate % 10 == 0) animal.ExpendEnergy(1);

                    if (animal.alive && (animal.hitPoints <= 0 || animal.efficiency == 0))
                    {
                        animal.Kill();
                        continue;
                    }
                }
                else if (sprite.boardPiece.GetType() == typeof(Player))
                {
                    Player player = (Player)sprite.boardPiece;
                    if (player.hitPoints <= 0)
                    {
                        player.Kill();
                        continue;
                    }
                }

                sprite.boardPiece.StateMachineWork();
            }
        }

        public void ProcessPlantQueue()
        {
            this.processedPlantsCount = 0;

            if ((SonOfRobinGame.lastUpdateDelay > 20 || SonOfRobinGame.lastDrawDelay > 20) && this.updateMultiplier == 1) return;

            if (this.plantCellsQueue.Count == 0)
            {
                this.plantCellsQueue = new List<Cell>(this.grid.allCells.ToList());
                //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Plants cells queue replenished ({this.plantCellsQueue.Count})");
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

            TimeSpan updateTimeElapsed;

            while (true)
            {
                if (plantSpritesQueue.Count == 0) return;

                Sprite plantSprite = this.plantSpritesQueue[0];
                this.plantSpritesQueue.RemoveAt(0);

                plantSprite.boardPiece.StateMachineWork();
                plantSprite.boardPiece.GrowOlder();
                if (plantSprite.boardPiece.efficiency < 0.2 || plantSprite.boardPiece.Mass < 1) plantSprite.boardPiece.Kill();
                this.processedPlantsCount++;

                updateTimeElapsed = DateTime.Now - Scene.startUpdateTime;
                if (updateTimeElapsed.Milliseconds > 9 * this.updateMultiplier)
                {
                    if (updateTimeElapsed.Milliseconds > 13 && this.updateMultiplier == 1) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Update time exceeded - {updateTimeElapsed.Milliseconds}ms.");

                    return;
                }
            }
        }

        public void UpdateAllAnims()
        {
            var visibleSpritesList = this.grid.GetSpritesInCameraView(camera: this.camera, groupName: Cell.Group.Visible);

            foreach (var sprite in visibleSpritesList)
            { sprite.UpdateAnimation(); }
        }

        public List<Sprite> GetVisibleSprites()
        { return this.grid.GetSpritesInCameraView(camera: this.camera, groupName: Cell.Group.Visible); }


        public override void Draw()
        {
            if (this.creationInProgress) return;

            // drawing blue background (to ensure drawing even if screen is larger than map)
            SonOfRobinGame.graphicsDevice.Clear(BoardGraphics.colorsByName[BoardGraphics.Colors.WaterDeep]);

            // drawing background and sprites
            this.grid.DrawBackground(camera: this.camera);

            // drawing sprites
            var noOfDisplayedSprites = this.grid.DrawSprites(camera: this.camera);

            // drawing grid cells
            if (Preferences.debugShowCellData) this.grid.DrawDebugData();

            // updating debugText
            this.debugText = $"objects {this.PieceCount}, visible {noOfDisplayedSprites}";
        }

    }
}
