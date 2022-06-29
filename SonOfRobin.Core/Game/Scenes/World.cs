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
            UseToolbarPiece,
            Interact,
            PickUp,
            Inventory,
            BasicCraft,
            Run,
            MapToggle,
            PauseMenu
        }

        public enum MapMode
        {
            None,
            Small,
            Big
        }

        public List<ActionKeys> actionKeyList = new List<ActionKeys>();
        public Vector2 analogMovement;
        public Vector2 analogCameraCorrection;

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
        public readonly string templatePath;
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

        public Dictionary<PieceTemplate.Name, int> pieceCountByName;
        public Dictionary<Type, int> pieceCountByClass;
        public Dictionary<string, Tracking> trackingQueue;
        public List<Cell> plantCellsQueue;
        public List<Sprite> plantSpritesQueue;
        public Dictionary<int, List<PlannedEvent>> eventQueue;
        public Grid grid;
        public int currentFrame;
        public int currentUpdate;
        public int updateMultiplier;
        public string debugText;
        public int processedPlantsCount;
        private int noOfPlantsToProcess;
        private int noOfUpdatesWithDelay;
        private int lastUpdateWithDelay;
        public int plantsProcessingUpperLimit;
        private Dictionary<PieceTemplate.Name, int> creationDelayByPiece;

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
              base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: true, blocksDrawsBelow: true, touchLayout: TouchLayout.Empty)
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
            this.eventQueue = new Dictionary<int, List<PlannedEvent>>();

            this.plantSpritesQueue = new List<Sprite>();
            this.plantCellsQueue = new List<Cell>();
            this.processedPlantsCount = 0;
            this.noOfPlantsToProcess = 300;
            this.noOfUpdatesWithDelay = 0;
            this.lastUpdateWithDelay = 0;
            this.plantsProcessingUpperLimit = 5000;
            this.creationDelayByPiece = new Dictionary<PieceTemplate.Name, int> { };
            this.camera = new Camera(this);
            this.camera.TrackCoords(new Vector2(0, 0));
            this.mapMode = MapMode.None;
            this.mapCycle = SonOfRobinGame.platform == Platform.Mobile ? new List<MapMode> { MapMode.None, MapMode.Big } : new List<MapMode> { MapMode.None, MapMode.Small, MapMode.Big };
            this.mapBig = new Map(world: this, fullScreen: true);
            this.mapSmall = new Map(world: this, fullScreen: false);
            this.grid = new Grid(this);
            this.templatePath = Path.Combine(SonOfRobinGame.worldTemplatesPath, $"seed_{this.seed}_{width}x{height}_{this.grid.cellWidth}x{this.grid.cellHeight}");
            if (!Directory.Exists(this.templatePath)) Directory.CreateDirectory(this.templatePath);

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
                this.creationEnd = DateTime.Now;
                this.creationDuration = this.creationEnd - this.creationStart;

                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"World creation time: {creationDuration.ToString("hh\\:mm\\:ss\\.fff")}.", color: Color.GreenYellow);

                if (this.saveGameData is null)
                {
                    if (!this.demoMode)
                    {
                        this.player = (Player)this.PlacePlayer();
                        this.camera.TrackPiece(trackedPiece: this.player, fluidMotion: false);
                    }

                    CreateMissingPieces(outsideCamera: false, multiplier: 1.0f);

                    if (this.demoMode) this.camera.TrackLiveAnimal();

                    this.currentFrame = 0;
                    this.currentUpdate = 0;
                }

                else
                { this.Deserialize(); }

                if (!this.demoMode) Scene.SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.player);
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

            // deserializing pieces

            var piecesByOldId = new Dictionary<string, BoardPiece> { }; // needed to match old IDs to new pieces (with new, different IDs)

            var pieceDataList = (List<Object>)saveGameDataDict["pieces"];

            var newPiecesByOldTargetIds = new Dictionary<string, BoardPiece> { };


            foreach (Dictionary<string, Object> pieceData in pieceDataList)
            {
                // repeated in StorageSlot

                PieceTemplate.Name templateName = (PieceTemplate.Name)pieceData["base_name"];

                bool female = pieceData.ContainsKey("animal_female") ? (bool)pieceData["animal_female"] : false;

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
            { new Tracking(world: this, trackingData: trackingData, piecesByOldId: piecesByOldId); }

            // deserializing planned events

            var eventDataList = (List<Object>)saveGameDataDict["events"];
            foreach (Dictionary<string, Object> eventData in eventDataList)
            { PlannedEvent.Deserialize(world: this, eventData: eventData, piecesByOldId: piecesByOldId); }

            // finalizing

            this.saveGameData = null;
            this.piecesLoadingMode = false;

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Game has been loaded.", color: Color.Cyan);
        }

        public void AutoSave(bool force = false)
        {
            if (this.demoMode || (!force && this.currentUpdate % 600 != 0)) return;

            TimeSpan timeSinceLastSave = DateTime.Now - this.lastSaved;
            if (!force && timeSinceLastSave < TimeSpan.FromMinutes(Preferences.autoSaveDelayMins)) return;

            string timeElapsedTxt = timeSinceLastSave.ToString("mm\\:ss");
            string message = force ? "Autosaving before returning to menu..." : $"{timeElapsedTxt} elapsed - autosaving...";

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: message, color: Color.LightBlue);

            string saveSlotName = "0";

            if (force)
            { this.Save(saveSlotName: saveSlotName); }
            else
            { new Scheduler.Task(menu: null, actionName: Scheduler.ActionName.SaveGame, executeHelper: saveSlotName); }

        }

        public void Save(string saveSlotName)
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
                if (sprite.boardPiece.exists)
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

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Game saved in slot {saveSlotName}.", color: Color.LightBlue);

            this.lastSaved = DateTime.Now;
        }

        public static World Load(string saveSlotName)
        {
            string saveDir = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);

            if (!Directory.Exists(saveDir))
            {
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Save slot {saveSlotName} is empty.", color: Color.Orange);
                return null;
            }

            // loading header

            string headerPath = Path.Combine(saveDir, "header.sav");
            var headerData = (Dictionary<string, Object>)LoaderSaver.Load(path: headerPath);

            if (headerData is null)
            {
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"Error while reading save header for slot {saveSlotName}.", color: Color.Orange);
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

        public void CreateMissingPieces(uint maxAmountToCreateAtOnce = 100000, bool outsideCamera = false, float multiplier = 1.0f)
        {
            // preparing a dictionary of pieces to create

            var multiplierByPiece = new Dictionary<PieceTemplate.Name, float> {
                { PieceTemplate.Name.GrassRegular, 10.0f },
                { PieceTemplate.Name.GrassDesert, 10.0f },
                { PieceTemplate.Name.Rushes, 5.0f },
                { PieceTemplate.Name.WaterLily, 1.0f },
                { PieceTemplate.Name.FlowersPlain, 0.3f },
                { PieceTemplate.Name.FlowersMountain, 0.1f },
                { PieceTemplate.Name.TreeSmall, 1.0f },
                { PieceTemplate.Name.TreeBig, 1.0f },
                { PieceTemplate.Name.AppleTree, 1.0f },
                { PieceTemplate.Name.CherryTree, 1.0f },
                { PieceTemplate.Name.BananaTree, 1.0f },
                { PieceTemplate.Name.PalmTree, 1.0f },
                { PieceTemplate.Name.Cactus, 0.2f },
                { PieceTemplate.Name.WaterRock, 0.5f },
                { PieceTemplate.Name.Minerals, 0.5f },
                { PieceTemplate.Name.Shell, 1.5f },
                { PieceTemplate.Name.Rabbit, 0.1f },
                { PieceTemplate.Name.Fox, 0.1f },
                { PieceTemplate.Name.Frog, 0.1f },
            };
            Vector2 notReallyUsedPosition = new Vector2(-100, -100); // -100, -100 will be converted to a random position on the map - needed for effective creation of new sprites 
            int minPieceAmount = Math.Max(Convert.ToInt32(width * height / 300000 * multiplier), 0); // 300000

            PieceTemplate.Name pieceName;
            float pieceMultiplier;
            int minAmount;
            var amountToCreateByName = new Dictionary<PieceTemplate.Name, int> { };

            foreach (var kvp in multiplierByPiece)
            {
                pieceName = kvp.Key;

                if (creationDelayByPiece.ContainsKey(pieceName) && this.currentUpdate < creationDelayByPiece[pieceName]) continue;

                pieceMultiplier = kvp.Value;
                minAmount = Math.Max((int)(minPieceAmount * pieceMultiplier), 4);

                int amountToCreate = Math.Max(minAmount - this.pieceCountByName[pieceName], 0);
                if (amountToCreate > 0) amountToCreateByName[pieceName] = amountToCreate;
            }

            if (amountToCreateByName.Keys.ToList().Count == 0) return;

            // creating pieces

            this.createMissinPiecesOutsideCamera = outsideCamera;
            int piecesCreated = 0;
            int amountLeftToCreate;

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
                        this.creationDelayByPiece[pieceName] = this.currentUpdate + 15000;
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
                PlannedEvent.ProcessQueue(this);
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

            // analog movement

            if (TouchInput.LeftStick.X != 0 || TouchInput.LeftStick.Y != 0)
            { this.analogMovement = TouchInput.LeftStick / 15; }
            else
            {
                this.analogMovement = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).ThumbSticks.Left;
                this.analogMovement = new Vector2(analogMovement.X * 4, analogMovement.Y * -4);
            }

            // analog camera control

            if (TouchInput.RightStick.X != 0 || TouchInput.RightStick.Y != 0)
            { this.analogCameraCorrection = new Vector2(TouchInput.RightStick.X / 8, TouchInput.RightStick.Y / 15); }
            else
            {
                this.analogCameraCorrection = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).ThumbSticks.Right;
                this.analogCameraCorrection = new Vector2(analogCameraCorrection.X * 25, analogCameraCorrection.Y * -25);
            }

            // interact

            if (Keyboard.HasBeenPressed(Keys.LeftAlt) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.X) ||
                VirtButton.HasButtonBeenPressed(VButName.Interact)
               ) actionKeyList.Add(ActionKeys.Interact);

            // use tool

            if (Keyboard.IsPressed(Keys.Space) ||
                GamePad.IsPressed(playerIndex: PlayerIndex.One, button: Buttons.A) ||
                VirtButton.IsButtonDown(VButName.UseTool)
               ) actionKeyList.Add(ActionKeys.UseToolbarPiece);

            // pick up

            if (Keyboard.HasBeenPressed(Keys.RightAlt) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Y) ||
                VirtButton.HasButtonBeenPressed(VButName.PickUp)
                ) actionKeyList.Add(ActionKeys.PickUp);

            // inventory

            if (Keyboard.HasBeenPressed(Keys.Enter) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadLeft) ||
                VirtButton.HasButtonBeenPressed(VButName.Inventory)
                ) actionKeyList.Add(ActionKeys.Inventory);


            // basic craft 

            if (Keyboard.HasBeenPressed(Keys.RightControl) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadUp) ||
                VirtButton.HasButtonBeenPressed(VButName.BasicCraft)
               ) actionKeyList.Add(ActionKeys.BasicCraft);

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

            ProcessActionKeyList();

            // camera zoom control

            float leftTrigger = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).Triggers.Left;
            float rightTrigger = GamePad.GetState(index: (int)PlayerIndex.One, deadZoneMode: GamePadDeadZone.IndependentAxes).Triggers.Right;

            float manualScale = leftTrigger != 0 ? 1 + (leftTrigger * 0.6f) : 1 - (rightTrigger * 0.7f);
            if (VirtButton.IsButtonDown(VButName.ZoomOut) || Keyboard.IsPressed(Keys.NumPad1)) manualScale = 2f;

            return manualScale;
        }

        private void ProcessActionKeyList()
        {
            if (this.actionKeyList.Contains(World.ActionKeys.PauseMenu)) MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Pause);

            if (this.actionKeyList.Contains(World.ActionKeys.BasicCraft))
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CraftBasic);
                return;
            }

            if (this.actionKeyList.Contains(World.ActionKeys.Inventory))
            {
                Scene.SetInventoryLayout(Scene.InventoryLayout.InventoryAndToolbar, player: this.player);
                return;
            }

            if (this.actionKeyList.Contains(World.ActionKeys.MapToggle))
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
                        throw new DivideByZeroException($"Unsupported mapMode - {mapMode}."); ;
                }

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

                    if (animal.alive && (animal.hitPoints <= 0 || animal.efficiency == 0)) animal.Kill();
                }

                sprite.boardPiece.StateMachineWork();
            }
        }

        public void ProcessPlantQueue()
        {
            if ((SonOfRobinGame.lastUpdateDelay > 20 || SonOfRobinGame.lastDrawDelay > 20) && this.updateMultiplier == 1 && this.currentUpdate > 200)
            {
                this.noOfUpdatesWithDelay++;
                this.lastUpdateWithDelay = this.currentUpdate;

                if (this.noOfUpdatesWithDelay > 6)
                {
                    this.plantsProcessingUpperLimit = Math.Max(noOfPlantsToProcess, 500);
                    this.noOfPlantsToProcess = 100;
                    this.noOfUpdatesWithDelay = 0;
                }
                else { this.noOfPlantsToProcess = Math.Max(10, this.noOfPlantsToProcess - 100); }
                return;
            }
            else
            {
                this.noOfUpdatesWithDelay = 0;
                this.noOfPlantsToProcess = Math.Min(this.noOfPlantsToProcess + 1, this.plantsProcessingUpperLimit);
                if (this.currentUpdate - lastUpdateWithDelay > 600 && this.currentUpdate % 20 == 0) this.plantsProcessingUpperLimit += 1;
            }

            this.processedPlantsCount = 0;
            int plantsLeftToProcess = this.noOfPlantsToProcess;
            int noOfCellsToAdd = 500;

            if (this.plantCellsQueue.Count == 0)
            {
                this.plantCellsQueue = new List<Cell>(this.grid.allCells.ToList());
                //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Plants cells queue replenished ({this.plantCellsQueue.Count})");
                return; // to avoid doing too many calculations in one frame
            }

            if (this.plantSpritesQueue.Count == 0)
            {
                for (int i = 0; i < noOfCellsToAdd; i++)
                {
                    int randomCellNo = random.Next(0, plantCellsQueue.Count);
                    Cell cell = this.plantCellsQueue[randomCellNo];
                    this.plantCellsQueue.RemoveAt(randomCellNo);
                    this.plantSpritesQueue.AddRange(cell.spriteGroups[Cell.Group.StateMachinesPlants].Values); // not shuffled to save cpu time

                    if (plantCellsQueue.Count == 0) break;
                }
                return; // to avoid doing too many calculations in one frame
            }

            while (true)
            {
                if (plantsLeftToProcess == 0 || plantSpritesQueue.Count == 0) return;

                Sprite plantSprite = this.plantSpritesQueue[0];
                this.plantSpritesQueue.RemoveAt(0);

                plantSprite.boardPiece.StateMachineWork();
                plantSprite.boardPiece.GrowOlder();
                if (plantSprite.boardPiece.efficiency < 0.2 || plantSprite.boardPiece.Mass < 1) plantSprite.boardPiece.Kill();
                this.processedPlantsCount++;
                plantsLeftToProcess--;
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
