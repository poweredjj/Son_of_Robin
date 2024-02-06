using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class Scheduler
    {
        public enum TaskName : byte
        {
            Empty,
            CreateNewWorld,
            CreateNewWorldNow,
            OpenMenuTemplate,
            OpenMainMenu,
            SaveGame,
            LoadGame,
            LoadGameNow,
            SavePrefs,
            OpenCraftMenu,
            Craft,
            Plant,
            Hit,
            CreateDebugPieces,
            OpenContainer,
            DropFruit,
            GetEaten,
            GetDrinked,
            ShowTextWindow,
            OpenShelterMenu,
            OpenBoatMenu,
            SleepOutside,
            TempoFastForward,
            TempoStop,
            TempoPlay,
            OpenMainMenuIfSpecialKeysArePressed,
            ExecuteTaskChain,
            RemoveScene,
            ChangeSceneInputType,
            SetCineMode,
            SetCineCurtains,
            AddTransition,
            SolidColorAddOverlay,
            SolidColorRemoveAll,
            SkipCinematics,
            SetSpectatorMode,
            SwitchLightSource,
            ResetControls,
            SaveControls,
            CheckForNonSavedControls,
            CheckForIncorrectPieces,
            RestartIsland,
            PlaySound,
            PlaySoundByName,
            AllowPiecesToBeHit,
            SetPlayerPointWalkTarget,
            InteractWithCooker,
            InteractWithFurnace,
            InteractWithLab,
            InteractWithTotem,
            ExportSave,
            ImportSave,
            GCCollectIfWorldNotRemoved,
            MakePlayerJumpOverThisPiece,
            OpenAndDestroyTreasureChest,
            CheckForPieceHints,
            ExecutePieceHintCheckNow,
            DisposeSaveScreenshotsIfNoMenuPresent,
            UseEntrance,
            AddWeatherEvent,
            ExecuteDelegate,
        }

        public delegate void ExecutionDelegate();

        private static readonly Dictionary<int, Queue<Task>> queue = new();
        private static int inputTurnedOffUntilFrame = 0;

        public static bool HasTaskChainInQueue
        {
            get
            {
                foreach (Queue<Task> taskList in queue.Values)
                {
                    foreach (Task task in taskList)
                    {
                        if (task.taskName == TaskName.ExecuteTaskChain) return true;
                        else if (task.taskName == TaskName.ShowTextWindow)
                        {
                            var textWindowData = (Dictionary<string, Object>)task.ExecuteHelper;
                            if (textWindowData.ContainsKey("closingTask") && (TaskName)textWindowData["closingTask"] == TaskName.ExecuteTaskChain) return true;
                        }
                    }
                }

                return false;
            }
        }

        public static void ProcessQueue()
        {
            if (SonOfRobinGame.CurrentUpdate >= inputTurnedOffUntilFrame) Input.GlobalInputActive = true;

            var framesToProcess = queue.Keys.Where(frameNo => SonOfRobinGame.CurrentUpdate >= frameNo).ToList(); // ToList() is needed; a copy must be iterated
            if (framesToProcess.Count == 0) return;

            foreach (int frameNo in framesToProcess)
            {
                Queue<Task> frameQueue = queue[frameNo];

                while (frameQueue.Count > 0)
                {
                    // if System.InvalidOperationException occurs, most likely some task has been added to current frame queue
                    frameQueue.Dequeue().Execute();
                }

                queue.Remove(frameNo);
            }
        }

        public static void RemoveAllTasksOfName(TaskName taskName)
        {
            foreach (int frameNo in queue.Keys.ToList())
            {
                queue[frameNo] = new Queue<Task>(queue[frameNo].Where(task => task.taskName != taskName));
            }
        }

        public static void ClearQueue()
        {
            MessageLog.Add(debugMessage: true, text: "Clearing Scheduler queue.");
            queue.Clear();
        }

        public struct Task
        {
            public readonly TaskName taskName;
            public Object ExecuteHelper { get; private set; }
            private readonly int delay;
            private int frame;
            private readonly bool turnOffInputUntilExecution;

            public readonly string TaskText
            {
                get
                {
                    string turnOffInputText = this.turnOffInputUntilExecution ? "(input off)" : "";
                    return $"{this.taskName} {turnOffInputText}";
                }
            }

            public Task(TaskName taskName, Object executeHelper = null, bool turnOffInputUntilExecution = false, int delay = 0, bool storeForLaterUse = false)
            {
                this.taskName = taskName;
                this.ExecuteHelper = executeHelper;
                this.turnOffInputUntilExecution = turnOffInputUntilExecution;
                this.delay = delay;

                if (!storeForLaterUse)
                {
                    this.frame = SonOfRobinGame.CurrentUpdate + this.delay;

                    if (this.turnOffInputUntilExecution) this.TurnOffInput();

                    this.Process();
                }
                else this.frame = -1;
            }

            private readonly void TurnOffInput()
            {
                Input.GlobalInputActive = false;
                inputTurnedOffUntilFrame = Math.Max(this.frame, inputTurnedOffUntilFrame);
            }

            public void Process()
            {
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{SonOfRobinGame.currentUpdate} processing task {this.TaskText}", color: Color.LightYellow);

                if (this.delay <= 0) this.Execute();
                else this.AddToQueue();
            }

            private void AddToQueue()
            {
                if (this.delay == 0) throw new ArgumentException($"Tried to add task with delay {this.delay} to queue.");

                //  SonOfRobinGame.messageLog.AddMessage(text: $"Adding to queue '{this.taskName}' - delay {this.delay}.", color: Color.White); // for testing

                if (this.turnOffInputUntilExecution) this.TurnOffInput();
                if (this.frame == -1) this.frame = SonOfRobinGame.CurrentUpdate + this.delay;
                if (!queue.ContainsKey(this.frame)) queue[this.frame] = new Queue<Task>();
                queue[this.frame].Enqueue(this);
            }

            public readonly void Execute()
            {
                switch (taskName)
                {
                    case TaskName.Empty:
                        return;

                    case TaskName.OpenMenuTemplate:
                        {
                            // example executeHelper for this task
                            // var menuTemplateData = new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.Main }, { "scenesToLink", scenesToLink } };

                            var menuTemplateData = (Dictionary<string, Object>)this.ExecuteHelper;
                            MenuTemplate.Name templateName = (MenuTemplate.Name)menuTemplateData["templateName"];

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: templateName);

                            if (menuTemplateData.ContainsKey("scenesToLink"))
                            {
                                List<Scene> scenesToLink = (List<Scene>)menuTemplateData["scenesToLink"];
                                menu.AddLinkedScenes(scenesToLink);
                            }

                            if (templateName == MenuTemplate.Name.GameOver) Scene.RemoveAllScenesOfType(typeof(TextWindow));

                            return;
                        }

                    case TaskName.OpenMainMenu:
                        {
                            new Task(taskName: TaskName.OpenMenuTemplate, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> { { "templateName", MenuTemplate.Name.Main } });

                            return;
                        }

                    case TaskName.OpenCraftMenu:
                        {
                            Workshop workshop = (Workshop)this.ExecuteHelper;

                            if (workshop.world.weather.IsRaining && !workshop.canBeUsedDuringRain)
                            {
                                new TextWindow(text: $"I can't use | {workshop.readableName} during rain.", imageList: new List<ImageObj> { workshop.sprite.AnimFrame.imageObj }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: workshop.world.DialogueSound);

                                return;
                            }

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);
                            if (menu == null) return; // if crafting was impossible at the moment

                            workshop.TurnOn();

                            ExecutionDelegate addLevelEventDlgt = () =>
                            {
                                if (workshop.world.HasBeenRemoved) return;
                                new LevelEvent(eventName: LevelEvent.EventName.TurnOffWorkshop, level: workshop.level, delay: 60 * 5, boardPiece: workshop);
                            };
                            menu.AddClosingTask(closingTask: TaskName.ExecuteDelegate, closingTaskHelper: addLevelEventDlgt);

                            return;
                        }

                    case TaskName.CreateNewWorld:
                        {
                            Scene.RemoveAllScenesOfType(typeof(Menu));
                            new Task(taskName: TaskName.CreateNewWorldNow, turnOffInputUntilExecution: true, delay: 13, executeHelper: this.ExecuteHelper);

                            return;
                        }

                    case TaskName.CreateNewWorldNow:
                        {
                            // example executeHelper for this task
                            // var createData = new Dictionary<string, Object> { { "width", width }, { "height", height }, { "seed", seed }, { "resDivider", resDivider } }

                            int width, height, seed, resDivider;

                            if (this.ExecuteHelper == null)
                            {
                                width = Preferences.newWorldSize;
                                height = Preferences.newWorldSize;
                                seed = Preferences.NewWorldSeed;
                                resDivider = Preferences.newWorldResDivider;
                            }
                            else
                            {
                                var createData = (Dictionary<string, Object>)this.ExecuteHelper;
                                width = (int)createData["width"];
                                height = (int)createData["height"];
                                seed = (int)createData["seed"];
                                resDivider = (int)createData["resDivider"];
                            }

                            Preferences.Save();
                            new World(width: width, height: height, seed: seed, resDivider: resDivider, playerName: Preferences.newWorldPlayerName);

                            return;
                        }

                    case TaskName.RestartIsland:
                        {
                            Scene.RemoveAllScenesOfType(typeof(Menu));
                            Scene.RemoveAllScenesOfType(typeof(TextWindow));
                            World oldWorld = (World)this.ExecuteHelper;

                            new World(width: oldWorld.IslandLevel.width, height: oldWorld.IslandLevel.height, seed: oldWorld.seed, resDivider: oldWorld.resDivider, playerName: Preferences.newWorldPlayerName, cellWidthOverride: oldWorld.Grid.cellWidth, cellHeightOverride: oldWorld.Grid.cellHeight);
                            oldWorld.Remove();

                            return;
                        }

                    case TaskName.CreateDebugPieces:
                        {
                            World world = World.GetTopWorld();
                            if (world == null)
                            {
                                MessageLog.Add(text: "Could not create selected item, because no world was found.");
                                return;
                            }

                            Player player = world.Player;

                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            Vector2 position = (Vector2)executeData["position"];
                            PieceTemplate.Name templateName = (PieceTemplate.Name)executeData["templateName"];

                            int piecesCreated = 0;
                            int attemptNo = 0;

                            while (true)
                            {
                                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: position, templateName: templateName, closestFreeSpot: true);
                                int amountToCreate = piece.StackSize;

                                if (piece.sprite.IsOnBoard)
                                {
                                    player.PickUpPiece(piece);
                                    piecesCreated++;
                                }

                                if (piecesCreated >= amountToCreate) break;

                                attemptNo++;
                                if (attemptNo == 1000)
                                {
                                    MessageLog.Add(text: $"Max number of attempts exceeded while trying to create '{templateName}'.");
                                    break;
                                }
                            }

                            MessageLog.Add(text: $"{piecesCreated} '{templateName}' pieces created.");

                            return;
                        }

                    case TaskName.ExportSave:
                        {
                            string saveSlotName = (string)this.ExecuteHelper;
                            string savePath = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);

                            SaveHeaderInfo saveInfo = new SaveHeaderInfo(folderName: Path.GetFileName(savePath));

                            string exportPath = SonOfRobinGame.downloadsPath;
                            if (!Directory.Exists(exportPath)) exportPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                            string zipPath;
                            int index = 0;
                            while (true)
                            {
                                zipPath = Path.Combine(exportPath, $"Son_of_Robin_save_{saveInfo.width}x{saveInfo.height}_{saveInfo.seed}_{saveSlotName}_{index}.zip.jpg");
                                if (File.Exists(zipPath)) index++;
                                else break;
                            }

                            bool savedCorrectly = Helpers.ZipFiles(savePath, zipPath);

                            if (savedCorrectly)
                            {
                                new TextWindow(text: $"Save has been exported to {zipPath}.", textColor: Color.White, bgColor: Color.DarkGreen, useTransition: false, animate: false);
                                Sound.QuickPlay(name: SoundData.Name.Ding2, volume: 1f);
                            }
                            else
                            {
                                new TextWindow(text: "Cannot export save.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false);
                                Sound.QuickPlay(name: SoundData.Name.Error, volume: 1f);
                            }

                            return;
                        }

                    case TaskName.ImportSave:
                        {
                            string zipPath = (string)this.ExecuteHelper;

                            string saveSlotName = SaveHeaderManager.NewSaveSlotName;
                            string extractPath = Path.Combine(SonOfRobinGame.saveGamesPath, saveSlotName);

                            bool importCorrect = true;
                            string message = "Save has been imported correctly.";

                            bool extractedCorrectly = Helpers.ExtractZipFile(zipPath: zipPath, extractPath: extractPath);
                            if (!extractedCorrectly)
                            {
                                importCorrect = false;
                                message = "Error occured while extracting zip file.";
                            }
                            else
                            {
                                SaveHeaderInfo saveInfo = new SaveHeaderInfo(folderName: Path.GetFileName(saveSlotName));
                                if (!saveInfo.saveIsCorrect)
                                {
                                    Directory.Delete(path: extractPath, recursive: true);

                                    importCorrect = false;
                                    message = "Imported save is incorrect.";
                                }
                                if (saveInfo.saveIsObsolete)
                                {
                                    importCorrect = false;
                                    message = "Imported save is obsolete.";
                                }
                                if (!saveInfo.saveIsObsolete && saveInfo.saveIsCorrupted)
                                {
                                    importCorrect = false;
                                    message = "Imported save is corrupted.";
                                }
                            }

                            Menu.RebuildAllMenus();

                            new TextWindow(text: message, textColor: Color.White, bgColor: importCorrect ? Color.DarkGreen : Color.DarkRed, useTransition: false, animate: false);
                            Sound.QuickPlay(name: importCorrect ? SoundData.Name.Ding2 : SoundData.Name.Error, volume: 1f);

                            return;
                        }

                    case TaskName.SaveGame:
                        {
                            // example executeHelper for this task
                            // var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", "1" }, { "showMessage", false } };

                            var saveParams = (Dictionary<string, Object>)this.ExecuteHelper;
                            World world = (World)saveParams["world"];
                            string saveSlotName = (string)saveParams["saveSlotName"];
                            bool showMessage = false;
                            if (saveParams.ContainsKey("showMessage")) showMessage = (bool)saveParams["showMessage"];
                            world.HintEngine.Disable(Tutorials.Type.HowToSave);

                            new LoaderSaver(saveMode: true, saveSlotName: saveSlotName, world: world, showSavedMessage: showMessage);

                            return;
                        }

                    case TaskName.LoadGame:
                        {
                            Scene.RemoveAllScenesOfType(typeof(Menu));
                            Scene.RemoveAllScenesOfType(typeof(TextWindow), includeWaiting: true);
                            Sound.QuickPlay(SoundData.Name.Select); // this sound should be short, because it will be silenced by LoadGameNow task

                            new Task(taskName: TaskName.LoadGameNow, turnOffInputUntilExecution: true, delay: 17, executeHelper: this.ExecuteHelper);

                            return;
                        }

                    case TaskName.LoadGameNow:
                        {
                            Scene.RemoveAllScenesOfType(typeof(TextWindow), includeWaiting: true);
                            SonOfRobinGame.SmallProgressBar.TurnOff(); // in case there was a progress bar (sleeping, etc.)
                            Sound.StopAll(); // no point in playing any sounds here - loading process glitches sound for a while
                            new LoaderSaver(saveMode: false, saveSlotName: (string)this.ExecuteHelper);

                            return;
                        }

                    case TaskName.SavePrefs:
                        {
                            Preferences.Save();
                            return;
                        }

                    case TaskName.OpenContainer:
                        {
                            BoardPiece container = (BoardPiece)this.ExecuteHelper;
                            if (!container.sprite.IsOnBoard) return;

                            World world = container.world;
                            Inventory.SetLayout(newLayout: Inventory.LayoutType.FieldStorage, player: world.Player, fieldStorage: container);
                        }

                        return;

                    case TaskName.Craft:
                        {
                            // example executeHelper for this task
                            // var craftParams = new Dictionary<string, Object> { { "recipe", recipe }, { "craftOnTheGround", false } };

                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            Craft.Recipe recipe = (Craft.Recipe)executeData["recipe"];
                            bool craftOnTheGround = (bool)executeData["craftOnTheGround"];

                            recipe.TryToProducePieces(player: World.GetTopWorld().Player, showMessages: true, craftOnTheGround: craftOnTheGround);
                        }
                        return;

                    case TaskName.Hit:
                        {
                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            Player player = (Player)executeData["player"];
                            Tool activeTool = (Tool)executeData["toolbarPiece"];
                            int shootingPower = (int)executeData["shootingPower"];
                            bool highlightOnly = (bool)executeData["highlightOnly"];
                            World world = player.world;

                            if (activeTool.pieceInfo.toolShootsProjectile)
                            {
                                activeTool.Use(shootingPower: shootingPower, targets: null);
                                return;
                            }

                            int inflateVal = Math.Max(activeTool.pieceInfo.toolRange, 3);
                            Rectangle focusRect = player.GetFocusRect(inflateX: inflateVal, inflateY: inflateVal);

                            var spritesForRect = world.Grid.GetSpritesForRect(groupName: Cell.Group.Visible, rectangle: focusRect, addPadding: true);
                            if (spritesForRect.Count == 0) return;

                            var targets = new List<BoardPiece>();
                            foreach (Sprite sprite in spritesForRect)
                            {
                                if (sprite.boardPiece.pieceInfo.Yield != null && sprite.boardPiece.alive && sprite.boardPiece != player)
                                {
                                    targets.Add(sprite.boardPiece);
                                }
                            }
                            if (targets.Count == 0) return;

                            if (activeTool.pieceInfo.toolRange == 0)
                            {
                                if (activeTool.pieceInfo.toolMultiplierByCategory.ContainsKey(BoardPiece.Category.Flesh))
                                {
                                    var animals = targets.Where(piece => piece.GetType() == typeof(Animal)).ToList();
                                    if (animals.Count > 0) targets = animals;
                                }

                                Vector2 focusRectCenter = new(focusRect.Center.X, focusRect.Center.Y);
                                BoardPiece firstTarget = targets.OrderBy(piece => Vector2.Distance(focusRectCenter, piece.sprite.position)).First();
                                targets = new List<BoardPiece> { firstTarget };
                            }

                            activeTool.Use(shootingPower: shootingPower, targets: targets, highlightOnly: highlightOnly);

                            return;
                        }

                    case TaskName.GetEaten:
                        {
                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            Player player = (Player)executeData["player"];
                            BoardPiece food = (BoardPiece)executeData["toolbarPiece"];
                            bool highlightOnly = false;
                            if (executeData.ContainsKey("highlightOnly")) highlightOnly = (bool)executeData["highlightOnly"];

                            if (highlightOnly) VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.UseTool, texture: TextureBank.GetTexture(TextureBank.TextureName.VirtButtonEat));

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (player.fedLevel >= player.maxFedLevel * 0.95f)
                            {
                                if (!highlightOnly) new TextWindow(text: "I am full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: player.world.DialogueSound);

                                return;
                            }

                            if (highlightOnly)
                            {
                                VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            World world = World.GetTopWorld();

                            player.activeSoundPack.Play(action: PieceSoundPackTemplate.Action.Eat, ignore3D: true);

                            // adding "generic" regen based on mass (if meal does not contain poison or regen)
                            if (!BuffEngine.BuffListContainsPoisonOrRegen(food.buffList)) food.buffList.Add(new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)(food.Mass / 3), autoRemoveDelay: 60 * 60));
                            player.AcquireEnergy(player.ConvertMassToEnergyAmount(food.Mass));

                            foreach (Buff buff in food.buffList)
                            { player.buffEngine.AddBuff(buff: buff, world: world); }

                            food.HitPoints = 0;
                            foreach (PieceStorage storage in player.CraftStoragesToPutInto)
                            {
                                storage.DestroyBrokenPieces(); // to destroy "broken" food right now
                            }

                            if (food.GetType() == typeof(Fruit) && world.random.Next(12) == 0) // getting seeds
                            {
                                PieceTemplate.Name plantName = food.pieceInfo.isSpawnedBy;
                                BoardPiece seedsPiece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: player.sprite.position, templateName: PieceTemplate.Name.SeedsGeneric, closestFreeSpot: true);

                                Seed seeds = (Seed)seedsPiece;
                                seeds.PlantToGrow = plantName;

                                bool piecePickedUp = player.PickUpPiece(seeds);
                                if (piecePickedUp)
                                {
                                    new Task(taskName: TaskName.PlaySoundByName, delay: 15, executeHelper: SoundData.Name.Ding1);

                                    new TextWindow(text: $"Acquired | seeds for | {PieceInfo.GetInfo(plantName).readableName}.", imageList: new List<ImageObj> { seeds.sprite.AnimFrame.imageObj, PieceInfo.GetImageObj(plantName) }, textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: true, checkForDuplicate: true, inputType: Scene.InputTypes.Normal, blocksUpdatesBelow: true, priority: 0);
                                }
                                else seeds.Destroy(); // seeds should not appear, if there is no room for them to be stored
                            }
                        }
                        return;

                    case TaskName.GetDrinked:
                        {
                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            Player player = (Player)executeData["player"];
                            Potion potion = (Potion)executeData["toolbarPiece"];
                            StorageSlot slot = (StorageSlot)executeData["slot"];
                            bool highlightOnly = false;
                            if (executeData.ContainsKey("highlightOnly")) highlightOnly = (bool)executeData["highlightOnly"];

                            if (highlightOnly) VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.UseTool, texture: TextureBank.GetTexture(TextureBank.TextureName.VirtButtonDrink));

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (highlightOnly)
                            {
                                VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            World world = World.GetTopWorld();

                            Sound.QuickPlay(SoundData.Name.Drink);

                            // all parameters should be increased using buffs

                            foreach (Buff buff in potion.buffList)
                            { player.buffEngine.AddBuff(buff: buff, world: world); }

                            if (potion.pieceInfo.convertsWhenUsed)
                            {
                                BoardPiece emptyContainter = PieceTemplate.CreatePiece(templateName: potion.pieceInfo.convertsToWhenUsed, world: world);
                                slot.DestroyPieceAndReplaceWithAnother(emptyContainter);
                            }
                            else slot.DestroyPieceWithID(potion.id);

                            player.CheckLowHP();

                            return;
                        }

                    case TaskName.SwitchLightSource:
                        {
                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            PortableLight portableLight = (PortableLight)executeData["toolbarPiece"];
                            bool highlightOnly = (bool)executeData["highlightOnly"];
                            bool buttonHeld = (bool)executeData["buttonHeld"];

                            bool canBurnNow = portableLight.CanBurnNow;

                            if (!highlightOnly && !canBurnNow && !buttonHeld)
                            {
                                MessageLog.Add(text: $"Cannot use {portableLight.readableName}.", imageObj: portableLight.pieceInfo.imageObj, bgColor: new Color(105, 3, 18), avoidDuplicates: true);
                                Sound.QuickPlay(name: SoundData.Name.Error, volume: 1f);
                                return;
                            }

                            if (!canBurnNow) return;

                            if (highlightOnly)
                            {
                                VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.UseTool, texture: TextureBank.GetTexture(TextureBank.TextureName.VirtButtonLighter));
                                VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            if (buttonHeld) return;

                            portableLight.IsOn = !portableLight.IsOn;

                            return;
                        }

                    case TaskName.ResetControls:
                        {
                            // example executeHelper for this task
                            // var resetData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "useDefault", true } };

                            var resetData = (Dictionary<string, Object>)this.ExecuteHelper;
                            bool gamepad = (bool)resetData["gamepad"];
                            bool useDefault = (bool)resetData["useDefault"];
                            bool showMessage = !resetData.ContainsKey("showMessage") || (bool)resetData["showMessage"];

                            if (gamepad)
                            {
                                InputPackage sourcePackage = useDefault ? InputMapper.defaultMappingGamepad : InputMapper.currentMappingGamepad;

                                InputMapper.currentMappingGamepad = sourcePackage.MakeCopy();
                                InputMapper.newMappingGamepad = sourcePackage.MakeCopy();
                            }
                            else
                            {
                                InputPackage sourcePackage = useDefault ? InputMapper.defaultMappingKeyboard : InputMapper.currentMappingKeyboard;

                                InputMapper.currentMappingKeyboard = sourcePackage.MakeCopy();
                                InputMapper.newMappingKeyboard = sourcePackage.MakeCopy();
                            }

                            InputMapper.RebuildMappings();
                            Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to refresh tutorials, etc.
                            new Task(taskName: TaskName.SavePrefs);

                            if (showMessage)
                            {
                                string type = gamepad ? "Gamepad" : "Keyboard";
                                new TextWindow(text: $"{type} controls has been reset.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: false);
                            }

                            return;
                        }

                    case TaskName.SaveControls:
                        {
                            // example executeHelper for this task
                            // var saveData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "openMenuIfNotValid", true } };

                            var saveData = (Dictionary<string, Object>)this.ExecuteHelper;
                            bool gamepad = (bool)saveData["gamepad"];
                            bool openMenuIfNotValid = (bool)saveData["openMenuIfNotValid"];

                            InputPackage newMapping = gamepad ? InputMapper.newMappingGamepad : InputMapper.newMappingKeyboard;

                            bool isValid = newMapping.Validate(gamepad: gamepad);
                            if (!isValid)
                            {
                                if (openMenuIfNotValid) MenuTemplate.CreateMenuFromTemplate(templateName: gamepad ? MenuTemplate.Name.Gamepad : MenuTemplate.Name.Keyboard);
                                return;
                            }

                            if (gamepad) InputMapper.currentMappingGamepad = InputMapper.newMappingGamepad.MakeCopy();
                            else InputMapper.currentMappingKeyboard = InputMapper.newMappingKeyboard.MakeCopy();

                            InputMapper.RebuildMappings();
                            Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to refresh tutorials, etc.
                            new Task(taskName: TaskName.SavePrefs);

                            string type = gamepad ? "Gamepad" : "Keyboard";
                            new TextWindow(text: $"{type} controls has been saved.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: false);
                            Sound.QuickPlay(name: SoundData.Name.Ding2, volume: 1f);

                            return;
                        }

                    case TaskName.CheckForNonSavedControls:
                        {
                            var gamepad = (bool)this.ExecuteHelper;

                            InputPackage newControls = gamepad ? InputMapper.newMappingGamepad : InputMapper.newMappingKeyboard;
                            InputPackage currentControls = gamepad ? InputMapper.currentMappingGamepad : InputMapper.currentMappingKeyboard;

                            if (!currentControls.Equals(newControls))
                            {
                                var optionList = new List<object>();

                                var saveData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "openMenuIfNotValid", true } };
                                optionList.Add(new Dictionary<string, object> { { "label", "save controls" }, { "taskName", TaskName.SaveControls }, { "executeHelper", saveData } });
                                var resetData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "useDefault", false }, { "showMessage", false } };
                                optionList.Add(new Dictionary<string, object> { { "label", "undo changes" }, { "taskName", TaskName.ResetControls }, { "executeHelper", resetData } });

                                ExecutionDelegate showConfMenuDlgt = () =>
                                {
                                    MenuTemplate.CreateConfirmationMenu(question: "There are unsaved changes.", customOptions: optionList, blocksUpdatesBelow: true);
                                };

                                new Task(taskName: TaskName.ExecuteDelegate, executeHelper: showConfMenuDlgt);
                            }

                            return;
                        }

                    case TaskName.DropFruit:
                        {
                            Plant fruitPlant = (Plant)this.ExecuteHelper;

                            if (!fruitPlant.world.Player.CanSeeAnything)
                            {
                                MessageLog.Add(debugMessage: false, text: $"It is too dark to shake {fruitPlant.readableName}.", avoidDuplicates: true);
                                return;
                            }

                            bool fruitDropped = fruitPlant.DropFruit(showMessage: true);
                            if (fruitDropped)
                            {
                                World world = fruitPlant.world;
                                world.HintEngine.Disable(Tutorials.Type.ShakeFruit);

                                float rotationChange = 0.1f;
                                if ((fruitPlant.sprite.position - world.Player.sprite.position).X > 0) rotationChange *= -1;

                                world.swayManager.AddSwayEvent(targetSprite: fruitPlant.sprite, sourceSprite: null, targetRotation: fruitPlant.sprite.rotation - rotationChange, playSound: false);
                            }

                            return;
                        }

                    case TaskName.ShowTextWindow:
                        {
                            var textWindowData = (Dictionary<string, Object>)this.ExecuteHelper;

                            string text = (string)textWindowData["text"];

                            List<ImageObj> imageList;
                            if (textWindowData.ContainsKey("imageList")) imageList = (List<ImageObj>)textWindowData["imageList"];
                            else imageList = new List<ImageObj>();

                            bool checkForDuplicate = false;
                            if (textWindowData.ContainsKey("checkForDuplicate")) checkForDuplicate = (bool)textWindowData["checkForDuplicate"];

                            bool useTransition = false;
                            if (textWindowData.ContainsKey("useTransition")) useTransition = (bool)textWindowData["useTransition"];

                            bool useTransitionOpen = false;
                            if (textWindowData.ContainsKey("useTransitionOpen")) useTransitionOpen = (bool)textWindowData["useTransitionOpen"];

                            bool useTransitionClose = false;
                            if (textWindowData.ContainsKey("useTransitionClose")) useTransitionClose = (bool)textWindowData["useTransitionClose"];

                            bool animate = true;
                            if (textWindowData.ContainsKey("animate")) animate = (bool)textWindowData["animate"];

                            bool autoClose = false;
                            if (textWindowData.ContainsKey("autoClose")) autoClose = (bool)textWindowData["autoClose"];

                            bool noInput = false;
                            if (textWindowData.ContainsKey("noInput")) noInput = (bool)textWindowData["noInput"];

                            TaskName closingTask = TaskName.Empty;
                            if (textWindowData.ContainsKey("closingTask")) closingTask = (TaskName)textWindowData["closingTask"];

                            Object closingTaskHelper = null;
                            if (textWindowData.ContainsKey("closingTaskHelper")) closingTaskHelper = textWindowData["closingTaskHelper"];

                            bool blocksUpdatesBelow = false;
                            if (textWindowData.ContainsKey("blocksUpdatesBelow")) blocksUpdatesBelow = (bool)textWindowData["blocksUpdatesBelow"];

                            int blockInputDuration = 0;
                            if (textWindowData.ContainsKey("blockInputDuration")) blockInputDuration = (int)textWindowData["blockInputDuration"];

                            SoundData.Name startingSound = SoundData.Name.Empty;
                            if (textWindowData.ContainsKey("startingSound")) startingSound = (SoundData.Name)textWindowData["startingSound"];

                            Sound animSound = null;
                            if (textWindowData.ContainsKey("animSound")) animSound = (Sound)textWindowData["animSound"];

                            Color bgColor = Color.DarkBlue;
                            Color textColor = Color.White;

                            if (textWindowData.ContainsKey("bgColor"))
                            {
                                var bgColorValues = (List<byte>)textWindowData["bgColor"];
                                bgColor = new Color(bgColorValues[0], bgColorValues[1], bgColorValues[2]);
                            }

                            if (textWindowData.ContainsKey("textColor"))
                            {
                                var textColorValues = (List<byte>)textWindowData["textColor"];
                                textColor = new Color(textColorValues[0], textColorValues[1], textColorValues[2]);
                            }

                            int framesPerChar = 0;
                            if (textWindowData.ContainsKey("framesPerChar")) framesPerChar = (int)textWindowData["framesPerChar"];

                            new TextWindow(text: text, imageList: imageList, useTransition: useTransition, useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose, bgColor: bgColor, textColor: textColor, framesPerChar: framesPerChar, animate: animate, checkForDuplicate: checkForDuplicate, closingTask: closingTask, closingTaskHelper: closingTaskHelper, blockInputDuration: blockInputDuration, blocksUpdatesBelow: blocksUpdatesBelow, startingSound: startingSound, animSound: animSound, autoClose: autoClose, noInput: noInput);
                            return;
                        }

                    case TaskName.SleepOutside:
                        {
                            Player player = World.GetTopWorld()?.Player;
                            if (player == null) return;

                            SleepEngine sleepEngine;

                            if (player.sprite.IsInWater)
                            {
                                if (player.sprite.CanDrownHere)
                                {
                                    player.HitPoints = 0;
                                    player.sprite.Visible = false;
                                    player.Kill();

                                    new TextWindow(text: "You have | drowned.", imageList: [AnimData.GetImageObj(AnimData.PkgName.WaterDrop)], textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 220); ;

                                    return;
                                }

                                sleepEngine = SleepEngine.OutdoorSleepWet;
                            }
                            else sleepEngine = SleepEngine.OutdoorSleepDry;

                            player.GoToSleep(sleepEngine: sleepEngine, zzzPos: player.sprite.position, checkIfSleepIsPossible: false);
                            return;
                        }

                    case TaskName.OpenShelterMenu:
                        {
                            BoardPiece shelterPiece = (BoardPiece)this.ExecuteHelper;
                            shelterPiece.world.OpenMenu(templateName: MenuTemplate.Name.Shelter, executeHelper: this.ExecuteHelper);

                            return;
                        }

                    case TaskName.TempoStop:
                        {
                            World world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            if (Preferences.FrameSkip) SonOfRobinGame.Game.IsFixedTimeStep = true;

                            // world.updateMultiplier = 0 is not used here, to allow for playing ambient sounds

                            world.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
                            world.ActiveLevel.stateMachineTypesManager.SetOnlyTheseTypes(enabledTypes: new List<Type> { typeof(AmbientSound), typeof(VisualEffect) }, everyFrame: true, nthFrame: true);

                            world.islandClock.Pause();

                            return;
                        }

                    case TaskName.TempoPlay:
                        {
                            World world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            if (Preferences.FrameSkip) SonOfRobinGame.Game.IsFixedTimeStep = true;

                            world.updateMultiplier = 1;
                            world.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
                            world.ActiveLevel.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
                            world.islandClock.Resume();

                            return;
                        }

                    case TaskName.TempoFastForward:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            SonOfRobinGame.Game.IsFixedTimeStep = false;

                            world.updateMultiplier = (int)this.ExecuteHelper;
                            world.ActiveLevel.stateMachineTypesManager.DisableMultiplier();
                            world.ActiveLevel.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
                            world.islandClock.Resume();

                            return;
                        }

                    case TaskName.ExecuteTaskChain:
                        {
                            // making a copy of the original taskChain, to avoid modifying it - needed for menus
                            List<Object> taskChain = new((List<Object>)this.ExecuteHelper);

                            if (taskChain.Count == 0) return;

                            Task currentTask = (Task)taskChain[0];
                            taskChain.RemoveAt(0);

                            if (currentTask.taskName == TaskName.ShowTextWindow)
                            {
                                // If text window will be opened, the delay will depend on the player, so it is unknown.
                                // So, the next task should be run after closing this text window.

                                if (taskChain.Count > 0)
                                {
                                    var executeHelper = currentTask.ExecuteHelper;
                                    var textWindowData = (Dictionary<string, Object>)executeHelper;
                                    textWindowData["closingTask"] = TaskName.ExecuteTaskChain;
                                    textWindowData["closingTaskHelper"] = taskChain;
                                    currentTask.ExecuteHelper = textWindowData;
                                }
                                currentTask.Process();
                            }
                            else
                            {
                                // in other cases, the delay should be known

                                currentTask.Process(); // must go before new Task(), to maintain correct execute order

                                if (taskChain.Count > 0) new Task(taskName: TaskName.ExecuteTaskChain, executeHelper: taskChain, delay: currentTask.delay, turnOffInputUntilExecution: true);
                            }

                            if (taskChain.Count == 0) Input.GlobalInputActive = true; // to ensure that input will be active at the end

                            return;
                        }

                    case TaskName.OpenMainMenuIfSpecialKeysArePressed:
                        {
                            Input.GlobalInputActive = true;
                            if (InputMapper.IsPressed(InputMapper.Action.SecretLicenceBypass)) new Task(taskName: TaskName.OpenMainMenu);
                            else CloseGame(quitGame: true);

                            return;
                        }

                    case TaskName.CheckForPieceHints:
                        {
                            // Scheduler variant - can be used anywhere (menus, etc.)

                            World world = World.GetTopWorld();
                            if (world == null) return;

                            // should be allowed when craft menu is on
                            bool craftMenuActive = false;
                            Scene menuScene = Scene.GetTopSceneOfType(typeof(Menu));
                            if (menuScene != null)
                            {
                                Menu menu = (Menu)menuScene;
                                craftMenuActive = menu.templateName.ToString().ToLower().Contains("craft");
                            }

                            if (!craftMenuActive &&
                                (world.CineMode ||
                                Scene.GetTopSceneOfType(typeof(TextWindow)) != null) ||
                                world.Player.activeState != BoardPiece.State.PlayerControlledWalking
                                )
                            {
                                new Task(taskName: this.taskName, delay: 60 * 2, executeHelper: this.ExecuteHelper);
                                return;
                            }

                            new Task(taskName: TaskName.ExecutePieceHintCheckNow, delay: 0, executeHelper: this.ExecuteHelper);

                            return;
                        }

                    case TaskName.ExecutePieceHintCheckNow:
                        {
                            // used to deduplicate unboxing code in Scheduler and LevelEvent

                            // example executeHelper for this task
                            //  var pieceHintData = new Dictionary<string, Object> { { "typesToCheckOnly", new List<PieceHint.Type> { PieceHint.Type.CrateStarting } }, { "fieldPiece", PieceTemplate.Name.Acorn }, { "newOwnedPiece", PieceTemplate.Name.Shell } };

                            World world = World.GetTopWorld();
                            if (world == null) return;

                            var pieceHintData = (Dictionary<string, Object>)this.ExecuteHelper;
                            List<PieceHint.Type> typesToCheckOnly = pieceHintData.ContainsKey("typesToCheckOnly") ? (List<PieceHint.Type>)pieceHintData["typesToCheckOnly"] : null;
                            PieceTemplate.Name fieldPiece = pieceHintData.ContainsKey("fieldPiece") ? (PieceTemplate.Name)pieceHintData["fieldPiece"] : PieceTemplate.Name.Empty;
                            PieceTemplate.Name newOwnedPiece = pieceHintData.ContainsKey("newOwnedPiece") ? (PieceTemplate.Name)pieceHintData["newOwnedPiece"] : PieceTemplate.Name.Empty;

                            world.HintEngine.CheckForPieceHintToShow(ignorePlayerState: true, ignoreInputActive: true, typesToCheckOnly: typesToCheckOnly, fieldPieceNameToCheck: fieldPiece, newOwnedPieceNameToCheck: newOwnedPiece);

                            return;
                        }

                    case TaskName.RemoveScene:
                        {
                            var removeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            Scene scene = (Scene)removeData["scene"];
                            bool fadeOut = (bool)removeData["fadeOut"];
                            int fadeOutDuration = (int)removeData["fadeOutDuration"];

                            if (fadeOut) scene.transManager.AddTransition(new Transition(transManager: scene.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: fadeOutDuration, endRemoveScene: true, startSwapParams: true)); // TODO verify if this works correctly
                            else scene.Remove();

                            return;
                        }

                    case TaskName.ChangeSceneInputType:
                        {
                            // example executeHelper for this task
                            // var inputData = new Dictionary<string, Object> { { "scene", world }, { "inputType",  Scene.InputTypes.Normal }};

                            var inputData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Scene scene = (Scene)inputData["scene"];
                            Scene.InputTypes inputType = (Scene.InputTypes)inputData["inputType"];

                            scene.InputType = inputType;
                            return;
                        }

                    case TaskName.SetCineMode:
                        {
                            World world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            world.CineMode = (bool)this.ExecuteHelper;
                            return;
                        }

                    case TaskName.SetCineCurtains:
                        {
                            World world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            world.cineCurtains.Enabled = (bool)this.ExecuteHelper;
                            return;
                        }

                    case TaskName.AddTransition:
                        {
                            // example executeHelper for this task
                            // var transData = new Dictionary<string, Object> { { "scene", scene }, { "transition",  transition }};

                            var transData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Scene scene = (Scene)transData["scene"];
                            Transition transition = (Transition)transData["transition"];

                            scene.transManager.AddTransition(transition);
                            return;
                        }

                    case TaskName.SolidColorAddOverlay:
                        {
                            World world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            // example executeHelper for this task
                            // var solidColorData = new Dictionary<string, Object> { { "color", Color.Red }, { "opacity", 0.5f }, { "fadeInDurationFrames", 60 * 2 } };

                            var solidColorData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Color color = (Color)solidColorData["color"];
                            float opacity = (float)solidColorData["opacity"];
                            int fadeInDurationFrames = solidColorData.ContainsKey("fadeInDurationFrames") ? (int)solidColorData["fadeInDurationFrames"] : 0;

                            SolidColor newOverlay = new(color: color, viewOpacity: fadeInDurationFrames > 0 ? 0 : opacity);
                            world.solidColorManager.Add(newOverlay);

                            if (fadeInDurationFrames > 0)
                            {
                                newOverlay.transManager.AddTransition(new Transition(transManager: newOverlay.transManager, outTrans: true, startDelay: 0, duration: fadeInDurationFrames, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: opacity, endCopyToBase: true));
                            }

                            return;
                        }

                    case TaskName.SolidColorRemoveAll:
                        {
                            // example executeHelper for this task
                            // var removeData = new Dictionary<string, Object> { { "manager", this.solidColorManager }, { "delay", 10 } }

                            var removeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            SolidColorManager solidColorManager = (SolidColorManager)removeData["manager"];
                            int delay = removeData.ContainsKey("delay") ? (int)removeData["delay"] : 0;

                            solidColorManager.RemoveAll(delay);

                            return;
                        }

                    case TaskName.SkipCinematics:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            MessageLog.Add(debugMessage: true, text: "Skipping cinematics", textColor: Color.White);

                            var textWindows = Scene.GetAllScenesOfType(typeof(TextWindow));
                            foreach (var scene in textWindows)
                            {
                                TextWindow textWindow = (TextWindow)scene;
                                textWindow.RemoveWithoutExecutingTask(); // every cine task will be tied to a text window
                            }
                            ClearQueue(); // to be sure, that no task will be executed
                            world.CineMode = false;

                            return;
                        }

                    case TaskName.SetSpectatorMode:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            world.SpectatorMode = (bool)this.ExecuteHelper;

                            return;
                        }

                    case TaskName.CheckForIncorrectPieces:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            // counting incorrect pieces

                            var incorrectBoardPieces = new List<BoardPiece>();
                            var incorrectStoragePieces = new List<BoardPiece>();
                            var piecesByID = new Dictionary<int, List<BoardPiece>>();
                            var duplicatedPiecesByID = new Dictionary<int, List<BoardPiece>>();
                            var blockedPieces = new List<BoardPiece>();
                            var piecesWithForeignCurrentCell = new List<BoardPiece>();
                            var offBoardPiecesWithPassiveMovement = new List<BoardPiece>();
                            var piecesWithLockedPassiveMovement = new List<BoardPiece>();

                            var blockedIgnoredTypesForNonPlants = new List<Type> { };

                            foreach (Sprite sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                            {
                                if (sprite.currentCell != null && sprite.currentCell.grid.level != world.ActiveLevel) piecesWithForeignCurrentCell.Add(sprite.boardPiece);

                                if (sprite.boardPiece.HasPassiveMovement && !sprite.IsOnBoard) offBoardPiecesWithPassiveMovement.Add(sprite.boardPiece);
                                if (sprite.boardPiece.HasPassiveMovement && sprite.IsOnBoard && !sprite.gridGroups.Contains(Cell.Group.StateMachinesNonPlants)) piecesWithLockedPassiveMovement.Add(sprite.boardPiece);

                                if (sprite.boardPiece.exists && !sprite.IsOnBoard) incorrectBoardPieces.Add(sprite.boardPiece);
                                if (piecesByID.ContainsKey(sprite.id)) piecesByID[sprite.id].Add(sprite.boardPiece);
                                else piecesByID[sprite.id] = new List<BoardPiece> { sprite.boardPiece };

                                if (sprite.boardPiece.exists && sprite.IsOnBoard && sprite.CheckForCollision(ignoreDensity: true))
                                {
                                    bool isPlant = sprite.boardPiece.GetType() == typeof(Plant);

                                    if (!sprite.BlocksMovement && !isPlant) continue;

                                    var blockingSprites = sprite.GetCollidingSprites(new List<Cell.Group> { sprite.BlocksMovement ? Cell.Group.ColMovement : Cell.Group.ColPlantGrowth });

                                    bool pieceIsBlocked = false;

                                    foreach (Sprite blockingSprite in blockingSprites)
                                    {
                                        if (blockedPieces.Contains(blockingSprite.boardPiece)) continue;

                                        if (isPlant && !sprite.BlocksMovement &&
                                            (blockingSprite.boardPiece.GetType() != typeof(Plant)) ||
                                            blockingSprite.BlocksMovement)
                                        {
                                            continue;
                                        }

                                        if (!blockedIgnoredTypesForNonPlants.Contains(blockingSprite.boardPiece.GetType()))
                                        {
                                            // sprite.color = Color.Red;
                                            // blockingSprite.color = Color.Blue;
                                            // world.camera.TrackPiece(sprite.boardPiece);

                                            pieceIsBlocked = true;
                                            break;
                                        }
                                    }

                                    if (pieceIsBlocked) blockedPieces.Add(sprite.boardPiece);
                                }

                                var storageList = new List<PieceStorage> { sprite.boardPiece.PieceStorage };

                                if (sprite.boardPiece.GetType() == typeof(Player)) // need to be updated with new classes using storages other than "this.pieceStorage"
                                {
                                    Player player = (Player)sprite.boardPiece;
                                    storageList.Add(player.ToolStorage);
                                    storageList.Add(player.EquipStorage);
                                    storageList.Add(player.GlobalChestStorage);
                                }

                                foreach (PieceStorage currentStorage in storageList)
                                {
                                    if (currentStorage == null) continue;
                                    foreach (BoardPiece storedPiece in currentStorage.GetAllPieces())
                                    {
                                        if (piecesByID.ContainsKey(storedPiece.id)) piecesByID[storedPiece.id].Add(storedPiece);
                                        else piecesByID[storedPiece.id] = new List<BoardPiece> { storedPiece };

                                        // fruits are in storage, but on board at the same time (but not on the grid) - to display correctly
                                        if (storedPiece.sprite.IsOnBoard && (storedPiece.GetType() != typeof(Fruit) && storedPiece.GetType() != typeof(Seed)))
                                        {
                                            incorrectStoragePieces.Add(storedPiece);
                                        }
                                    }
                                }
                            }

                            foreach (List<BoardPiece> pieceList in piecesByID.Values)
                            {
                                if (pieceList.Count > 1)
                                {
                                    duplicatedPiecesByID[pieceList[0].id] = pieceList;
                                }
                            }

                            // building error list

                            var errorsFound = new List<string>();
                            if (incorrectBoardPieces.Count > 0) errorsFound.Add($"Pieces placed incorrectly on the field: {incorrectBoardPieces.Count}.");
                            if (incorrectStoragePieces.Count > 0) errorsFound.Add($"Pieces placed incorrectly in the storage: {incorrectStoragePieces.Count}.");
                            if (blockedPieces.Count > 0) errorsFound.Add($"Pieces blocked: {blockedPieces.Count}.");
                            if (piecesWithForeignCurrentCell.Count > 0) errorsFound.Add($"Pieces with foreign current cell: {piecesWithForeignCurrentCell.Count}.");
                            if (offBoardPiecesWithPassiveMovement.Count > 0) errorsFound.Add($"Off board pieces with passive movement: {offBoardPiecesWithPassiveMovement.Count}.");
                            if (piecesWithLockedPassiveMovement.Count > 0) errorsFound.Add($"Pieces with locked passive movement: {piecesWithLockedPassiveMovement.Count}.");
                            if (duplicatedPiecesByID.Count > 0)
                            {
                                errorsFound.Add($"Duplicated pieces ({duplicatedPiecesByID.Count} IDs):");

                                Random random = SonOfRobinGame.random;

                                foreach (List<BoardPiece> duplicatedPieceList in duplicatedPiecesByID.Values)
                                {
                                    //Color color = new(random.Next(0, 200), random.Next(0, 200), random.Next(0, 200));
                                    //foreach (BoardPiece duplicatedPiece in duplicatedPieceList)
                                    //{
                                    //    duplicatedPiece.sprite.color = color;
                                    //}

                                    var uniqueNamesArray = duplicatedPieceList.Select(p => p.readableName).Distinct().ToArray();
                                    string uniqueNames = String.Join(", ", uniqueNamesArray);

                                    errorsFound.Add($"- ID {duplicatedPieceList[0].id} x{duplicatedPieceList.Count} {uniqueNames}");
                                }
                            }

                            bool isCorrect = errorsFound.Count == 0;
                            string message = errorsFound.Count > 0 ? String.Join("\n", errorsFound) : "No incorrect pieces found.";

                            // showing message

                            MessageLog.Add(debugMessage: true, text: message);

                            new TextWindow(text: message, animate: false, useTransition: false, bgColor: isCorrect ? Color.Green : Color.DarkRed, textColor: Color.White);

                            return;
                        }

                    case TaskName.PlaySound:
                        {
                            Sound sound = (Sound)this.ExecuteHelper;
                            sound.Play();

                            return;
                        }

                    case TaskName.PlaySoundByName:
                        {
                            SoundData.Name soundName = (SoundData.Name)this.ExecuteHelper;
                            Sound.QuickPlay(soundName);

                            return;
                        }

                    case TaskName.AllowPiecesToBeHit:
                        {
                            var pieceList = (List<BoardPiece>)this.ExecuteHelper;
                            foreach (BoardPiece piece in pieceList)
                            {
                                piece.canBeHit = true;
                            }

                            return;
                        }

                    case TaskName.SetPlayerPointWalkTarget:
                        {
                            World world = World.GetTopWorld();
                            if (world == null || world.Player == null || !world.Player.alive) return;

                            world.Player.pointWalkTarget = (Vector2)this.ExecuteHelper;

                            return;
                        }

                    case TaskName.InteractWithCooker:
                        {
                            Cooker cooker = (Cooker)this.ExecuteHelper;

                            if (cooker.IsOn) cooker.ShowCookingProgress();
                            else new Task(taskName: TaskName.OpenContainer, executeHelper: cooker, delay: 0);

                            return;
                        }

                    case TaskName.InteractWithFurnace:
                        {
                            Furnace furnace = (Furnace)this.ExecuteHelper;

                            if (furnace.IsOn) furnace.ShowSmeltingProgress();
                            else new Task(taskName: TaskName.OpenContainer, executeHelper: furnace, delay: 0);

                            return;
                        }

                    case TaskName.InteractWithLab:
                        {
                            AlchemyLab alchemyLab = (AlchemyLab)this.ExecuteHelper;

                            if (alchemyLab.IsOn) alchemyLab.ShowBrewingProgress();
                            else new Task(taskName: TaskName.OpenContainer, executeHelper: alchemyLab, delay: 0);

                            return;
                        }

                    case TaskName.InteractWithTotem:
                        {
                            Totem totem = (Totem)this.ExecuteHelper;

                            if (totem.CanBeUsedNow) new Task(taskName: TaskName.OpenContainer, delay: 0, executeHelper: this.ExecuteHelper);
                            else
                            {
                                int hoursToActivation = (int)Math.Ceiling(totem.TimeUntilCanBeUsed.TotalMinutes / 60);

                                string timeLeftString = hoursToActivation == 1 ?
                                    $"{hoursToActivation} symbol is glowing" :
                                    $"{hoursToActivation} symbols are glowing";

                                HintEngine.ShowMessageDuringPause(new List<HintMessage> {
                                   new HintMessage(text: $"I cannot approach this | {totem.readableName} now.\n{timeLeftString}.",
                                   blockInputDefaultDuration: true, imageList: new List<ImageObj> { totem.sprite.AnimFrame.imageObj }),
                                   });
                            }

                            return;
                        }

                    case TaskName.Plant:
                        {
                            var executeData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Player player = (Player)executeData["player"];
                            World world = player.world;
                            Seed seeds = (Seed)executeData["toolbarPiece"];
                            PieceTemplate.Name plantName = seeds.PlantToGrow;
                            bool highlightOnly = (bool)executeData["highlightOnly"];

                            if (highlightOnly) VirtButton.ButtonChangeTextureOnNextFrame(buttonName: VButName.UseTool, texture: TextureBank.GetTexture(TextureBank.TextureName.VirtButtonPlant));

                            if (!player.CanSeeAnything)
                            {
                                if (!highlightOnly) new TextWindow(text: $"It is too dark to plant | {PieceInfo.GetInfo(plantName).readableName}.", imageList: new List<ImageObj> { PieceInfo.GetImageObj(plantName) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 70, priority: 0, animSound: world.DialogueSound);

                                return;
                            }

                            if (player.IsVeryTired)
                            {
                                if (!highlightOnly) new TextWindow(text: "I'm too tired to plant anything...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 70, priority: 0, animSound: world.DialogueSound);

                                return;
                            }

                            if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
                            {
                                if (!highlightOnly) new TextWindow(text: $"I can't plant | {PieceInfo.GetInfo(plantName).readableName} with enemies nearby.", imageList: new List<ImageObj> { PieceInfo.GetImageObj(plantName) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 70, priority: 0, animSound: world.DialogueSound);

                                return;
                            }

                            if (highlightOnly)
                            {
                                if (Plant.GetFertileGround(world.Player) != null)
                                {
                                    VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                    ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                }

                                return;
                            }

                            Craft.Recipe plantRecipe = new(pieceToCreate: plantName, ingredients: new Dictionary<PieceTemplate.Name, byte> { { seeds.name, 1 } }, fatigue: PieceInfo.GetInfo(plantName).blocksMovement ? 100 : 50, maxLevel: 0, durationMultiplier: 1f, fatigueMultiplier: 1f, isReversible: false, isTemporary: true, useOnlyIngredientsWithID: seeds.id);

                            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: player);
                            plantRecipe.TryToProducePieces(player: player, showMessages: false);

                            return;
                        }

                    case TaskName.GCCollectIfWorldNotRemoved:
                        {
                            if (World.DestroyedNotReleasedWorldCount == 0) return;

                            MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} invoking GC.Collect()", textColor: new Color(255, 180, 66));
                            GC.Collect();
                            RemoveAllTasksOfName(TaskName.GCCollectIfWorldNotRemoved); // to avoid invoking GC.Collect() multiple times

                            return;
                        }

                    case TaskName.OpenAndDestroyTreasureChest:
                        {
                            Container treasureChest = (Container)this.ExecuteHelper;
                            treasureChest.Open();
                            treasureChest.PieceStorage.DropAllPiecesToTheGround(addMovement: true);

                            new LevelEvent(eventName: LevelEvent.EventName.Destruction, delay: 60 * 2, level: treasureChest.level, boardPiece: treasureChest, eventHelper: 30);

                            return;
                        }

                    case TaskName.MakePlayerJumpOverThisPiece:
                        {
                            BoardPiece obstacle = (BoardPiece)this.ExecuteHelper;
                            Player player = obstacle.world.Player;

                            Vector2 obstaclePos = obstacle.sprite.position;
                            Vector2 playerPos = player.sprite.position;

                            bool horizontal = obstacle.sprite.ColRect.Width > obstacle.sprite.ColRect.Height;

                            // checking for proper player alignment

                            bool playerOnTheLeft = playerPos.X < obstaclePos.X;
                            bool playerOnTheTop = playerPos.Y < obstaclePos.Y;

                            var canJump = player.sprite.orientation switch
                            {
                                Sprite.Orientation.left => !horizontal && !playerOnTheLeft,
                                Sprite.Orientation.right => !horizontal && playerOnTheLeft,
                                Sprite.Orientation.up => horizontal && !playerOnTheTop,
                                Sprite.Orientation.down => horizontal && playerOnTheTop,
                                _ => throw new ArgumentException($"Unsupported orientation - '{player.sprite.orientation}'."),
                            };

                            if (!canJump)
                            {
                                Sound.QuickPlay(SoundData.Name.Error);
                                return;
                            }

                            // calculating jump movement

                            if (horizontal)
                            {
                                {
                                    int yDiff = (int)Math.Abs(obstaclePos.Y - playerPos.Y) * 2;

                                    if (playerPos.Y < obstaclePos.Y) playerPos.Y += yDiff;
                                    else playerPos.Y -= yDiff;
                                }
                            }
                            else
                            {
                                {
                                    int xDiff = (int)Math.Abs(obstaclePos.X - playerPos.X) * 2;

                                    if (playerPos.X < obstaclePos.X) playerPos.X += xDiff;
                                    else playerPos.X -= xDiff;
                                }
                            }

                            // performing jump

                            bool hasJumped = player.sprite.MoveToClosestFreeSpot(startPosition: playerPos, maxDistance: 15);
                            if (hasJumped)
                            {
                                player.activeSoundPack.Play(action: PieceSoundPackTemplate.Action.PlayerJump, ignore3D: true);
                                player.Fatigue += 10;
                            }
                            else Sound.QuickPlay(SoundData.Name.Error);

                            return;
                        }

                    case TaskName.DisposeSaveScreenshotsIfNoMenuPresent:
                        {
                            if (Scene.GetTopSceneOfType(typeof(Menu)) != null) new Task(taskName: TaskName.DisposeSaveScreenshotsIfNoMenuPresent, delay: 60 * 3);
                            else SaveHeaderInfo.DisposeScreenshots();

                            return;
                        }

                    case TaskName.UseEntrance:
                        {
                            Entrance entrance = (Entrance)this.ExecuteHelper;
                            entrance.Enter();
                            return;
                        }

                    case TaskName.OpenBoatMenu:
                        {
                            BoardPiece boatPiece = (BoardPiece)this.ExecuteHelper;
                            boatPiece.world.OpenMenu(templateName: MenuTemplate.Name.Boat, executeHelper: this.ExecuteHelper);

                            return;
                        }

                    case TaskName.AddWeatherEvent:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            WeatherEvent weatherEvent = (WeatherEvent)this.ExecuteHelper;

                            DateTime islandDateTime = world.islandClock.IslandDateTime;

                            if (weatherEvent.startTime < islandDateTime) // pushing forward event, to avoid starting "in the past"
                            {
                                weatherEvent = new WeatherEvent(type: weatherEvent.type, intensity: weatherEvent.intensity, startTime: islandDateTime, duration: weatherEvent.duration, transitionLength: weatherEvent.transitionLength);
                            }

                            world.weather.AddEvent(weatherEvent);

                            return;
                        }

                    case TaskName.ExecuteDelegate:
                        {
                            ((Delegate)this.ExecuteHelper).DynamicInvoke();
                            return;
                        }

                    default:
                        throw new ArgumentException($"Unsupported taskName - {taskName}.");
                }
            }

            public static void CloseGame(bool quitGame)
            {
                SonOfRobinGame.SmallProgressBar.TurnOff();

                var worldScenes = Scene.GetAllScenesOfType(typeof(World));
                foreach (World currWorld in worldScenes)
                {
                    if (!currWorld.demoMode) currWorld.Remove();
                }

                Scene.RemoveAllScenesOfType(typeof(TextWindow));
                Scene.RemoveAllScenesOfType(typeof(Menu));
                Scene.RemoveAllScenesOfType(typeof(Inventory));
                Scene.RemoveAllScenesOfType(typeof(PieceContextMenu));

                if (quitGame) SonOfRobinGame.quitGame = true;
                else new Task(taskName: TaskName.OpenMainMenu, turnOffInputUntilExecution: true, delay: 0);
            }
        }
    }
}