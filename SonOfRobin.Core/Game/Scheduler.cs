using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.Tweening;
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
            Empty = 0,

            CreateNewWorld = 1,
            CreateNewWorldNow = 2,
            QuitGame = 3,
            OpenMenuTemplate = 4,
            OpenMainMenu = 5,
            OpenConfirmationMenu = 6,
            SaveGame = 7,
            LoadGame = 8,
            LoadGameNow = 9,
            ReturnToMainMenu = 10,
            SavePrefs = 11,
            ProcessConfirmation = 12,
            OpenCraftMenu = 13,
            Craft = 14,
            Plant = 15,
            Hit = 16,
            CreateNewPiece = 17,
            CreateDebugPieces = 18,
            OpenContainer = 19,
            DeleteIncompatibleSaves = 20,
            DropFruit = 21,
            GetEaten = 22,
            GetDrinked = 23,
            ExecuteTaskWithDelay = 24,
            AddWorldEvent = 25,
            ShowTextWindow = 26,
            OpenShelterMenu = 27,
            OpenBoatMenu = 28,
            SleepInsideShelter = 29,
            SleepOutside = 30,
            ForceWakeUp = 31,
            TempoFastForward = 32,
            TempoStop = 33,
            TempoPlay = 34,
            CameraTrackPiece = 35,
            CameraTrackCoordsSmoothly = 36,
            CameraTrackCoordsInstantly = 106,
            CameraSetZoom = 37,
            CameraSetMovementSpeed = 38,
            CameraResetMovementSpeed = 39,
            ShowCookingProgress = 40,
            ShowBrewingProgress = 41,
            RestoreHints = 42,
            OpenMainMenuIfSpecialKeysArePressed = 43,
            ShowHint = 44,
            ExecuteTaskChain = 45,
            ShowTutorialInMenu = 46,
            ShowTutorialInGame = 47,
            RemoveScene = 48,
            ChangeSceneInputType = 49,
            SetCineMode = 50,
            SetCineCurtains = 51,
            AddTransition = 52,
            SolidColorAddOverlay = 53,
            SolidColorRemoveAll = 54,
            SkipCinematics = 55,
            SetSpectatorMode = 56,
            SwitchLightSource = 57,
            ResetControls = 58,
            SaveControls = 59,
            CheckForNonSavedControls = 60,
            RebuildMenu = 61,
            RebuildAllMenus = 62,
            CheckForIncorrectPieces = 63,
            RestartIsland = 64,
            ResetNewWorldSettings = 65,
            PlaySound = 66,
            PlaySoundByName = 67,
            AllowPiecesToBeHit = 68,
            SetPlayerPointWalkTarget = 69,
            StopSound = 70,
            RemoveAllScenesOfType = 71,
            WaitUntilMorning = 72,
            ActivateLightEngine = 73,
            DeactivateLightEngine = 74,
            AddPassiveMovement = 75,
            AddFadeInAnim = 76,
            InteractWithCooker = 77,
            InteractWithLab = 78,
            InteractWithTotem = 79,
            InventoryCombineItems = 80,
            InventoryReleaseHeldPieces = 81,
            RemoveBuffs = 82,
            ExportSave = 83,
            ImportSave = 84,
            GCCollectIfWorldNotRemoved = 85,
            DestroyAndDropDebris = 86,
            MakePlayerJumpOverThisPiece = 87,
            InventoryApplyPotion = 88,
            OpenAndDestroyTreasureChest = 89,
            SetAllNamedLocationsAsDiscovered = 90,
            AddRumble = 91,
            CheckForPieceHints = 92,
            ExecutePieceHintCheckNow = 93,
            TurnOnWindParticles = 94,
            DisposeSaveScreenshotsIfNoMenuPresent = 95,
            SetGlobalWorldEffect = 96,
            SetGlobalWorldTweener = 97,
            UseEntrance = 98,
            UseBoat = 99,
            IslandClockAdvance = 100,
            ChangeActiveState = 101,
            ChangeTipsLayout = 102,
            ChangeTouchLayout = 103,
            FinishConstructionAnimation = 104,
            EnterNewLevel = 105,
        }

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

                    case TaskName.RebuildMenu:
                        {
                            Menu menu = (Menu)this.ExecuteHelper;
                            menu.Rebuild(instantScroll: false);
                            return;
                        }

                    case TaskName.RebuildAllMenus:
                        {
                            Menu.RebuildAllMenus();
                            return;
                        }

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
                                new TextWindow(text: $"I can't use | {workshop.readableName} during rain.", imageList: new List<Texture2D> { workshop.sprite.CroppedAnimFrame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: workshop.world.DialogueSound);

                                return;
                            }

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);
                            if (menu == null) return; // if crafting was impossible at the moment

                            workshop.TurnOn();

                            var worldEventData = new Dictionary<string, object> {
                                {"boardPiece", workshop },
                                {"delay", 300 },
                                {"eventName", LevelEvent.EventName.TurnOffWorkshop },
                            };

                            menu.AddClosingTask(closingTask: TaskName.AddWorldEvent, closingTaskHelper: worldEventData);
                            return;
                        }

                    case TaskName.OpenConfirmationMenu:
                        {
                            MenuTemplate.CreateConfirmationMenu(confirmationData: this.ExecuteHelper);
                            return;
                        }

                    case TaskName.ProcessConfirmation:
                        {
                            var confirmationData = (Dictionary<string, Object>)this.ExecuteHelper;
                            new Task(taskName: (TaskName)confirmationData["taskName"], executeHelper: confirmationData["executeHelper"]);

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

                    case TaskName.ResetNewWorldSettings:
                        {
                            Preferences.ResetNewWorldSettings();
                            return;
                        }

                    case TaskName.CreateNewPiece:
                        {
                            World world = World.GetTopWorld();
                            if (world != null)
                            {
                                var executeData = (Dictionary<string, Object>)this.ExecuteHelper;
                                Vector2 position = (Vector2)executeData["position"];
                                PieceTemplate.Name templateName = (PieceTemplate.Name)executeData["templateName"];
                                BoardPiece piece = PieceTemplate.CreateAndPlaceOnBoard(world: world, position: position, templateName: templateName);
                                if (piece.sprite.IsOnBoard) piece.sprite.MoveToClosestFreeSpot(position);
                            }
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

                                    new TextWindow(text: $"Acquired | seeds for | {PieceInfo.GetInfo(plantName).readableName}.", imageList: new List<Texture2D> { seeds.sprite.CroppedAnimFrame.texture, PieceInfo.GetTexture(plantName) }, textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: true, checkForDuplicate: true, inputType: Scene.InputTypes.Normal, blocksUpdatesBelow: true, priority: 0);
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
                                MessageLog.Add(text: $"Cannot use {portableLight.readableName}.", texture: portableLight.pieceInfo.CroppedFrame.texture, bgColor: new Color(105, 3, 18), avoidDuplicates: true);
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

                                var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", "There are unsaved changes." }, { "customOptionList", optionList } };

                                new Task(taskName: TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
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

                    case TaskName.DeleteIncompatibleSaves:
                        {
                            SaveHeaderManager.DeleteIncompatibleSaves();
                            return;
                        }

                    case TaskName.ExecuteTaskWithDelay:
                        {
                            // example executeHelper for this task
                            // var delayData = new Dictionary<string, Object> { { "taskName", TaskName.TurnOffWorkshop }, { "executeHelper", workshop }, { "delay", 300 } };

                            var delayData = (Dictionary<string, Object>)this.ExecuteHelper;
                            new Task(taskName: (TaskName)delayData["taskName"], executeHelper: delayData["executeHelper"], delay: (int)delayData["delay"]);

                            return;
                        }

                    case TaskName.AddWorldEvent:
                        {
                            World world = (World)Scene.GetTopSceneOfType(typeof(World));
                            if (world == null) return;

                            var worldEventData = (Dictionary<string, Object>)this.ExecuteHelper;

                            LevelEvent.EventName eventName = (LevelEvent.EventName)worldEventData["eventName"];
                            BoardPiece boardPiece = (BoardPiece)worldEventData["boardPiece"];
                            int delay = (int)worldEventData["delay"];

                            new LevelEvent(eventName: eventName, level: boardPiece.level, delay: delay, boardPiece: boardPiece);

                            return;
                        }

                    case TaskName.ShowTextWindow:
                        {
                            var textWindowData = (Dictionary<string, Object>)this.ExecuteHelper;

                            string text = (string)textWindowData["text"];

                            List<Texture2D> imageList;
                            if (textWindowData.ContainsKey("imageList")) imageList = (List<Texture2D>)textWindowData["imageList"];
                            else imageList = new List<Texture2D>();

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

                            new TextWindow(text: text, imageList: imageList, useTransition: useTransition, useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose, bgColor: bgColor, textColor: textColor, framesPerChar: framesPerChar, animate: animate, checkForDuplicate: checkForDuplicate, closingTask: closingTask, closingTaskHelper: closingTaskHelper, blockInputDuration: blockInputDuration, blocksUpdatesBelow: blocksUpdatesBelow, startingSound: startingSound, animSound: animSound);
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

                                    new TextWindow(text: "You have | drowned.", imageList: new List<Texture2D> { AnimData.croppedFramesForPkgs[AnimData.PkgName.WaterDrop].texture }, textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 220); ;

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

                    case TaskName.SleepInsideShelter:
                        {
                            Shelter shelterPiece = (Shelter)this.ExecuteHelper;
                            SleepEngine sleepEngine = shelterPiece.sleepEngine;
                            World.GetTopWorld()?.Player.GoToSleep(sleepEngine: sleepEngine, zzzPos: new Vector2(shelterPiece.sprite.GfxRect.Center.X, shelterPiece.sprite.GfxRect.Center.Y), checkIfSleepIsPossible: true);

                            return;
                        }

                    case TaskName.ForceWakeUp:
                        {
                            Player player = (Player)this.ExecuteHelper;
                            player.WakeUp(force: true);
                            return;
                        }

                    case TaskName.WaitUntilMorning:
                        {
                            Player player = (Player)this.ExecuteHelper;
                            player.sleepMode = Player.SleepMode.WaitMorning;
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

                    case TaskName.CameraTrackPiece:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            BoardPiece piece = (BoardPiece)this.ExecuteHelper;
                            world.camera.TrackPiece(piece);

                            return;
                        }

                    case TaskName.CameraTrackCoordsSmoothly:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            world.camera.TrackCoords(position: (Vector2)this.ExecuteHelper, moveInstantly: false);

                            return;
                        }

                    case TaskName.CameraTrackCoordsInstantly:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            world.camera.TrackCoords(position: (Vector2)this.ExecuteHelper, moveInstantly: true);

                            return;
                        }

                    case TaskName.CameraSetZoom:
                        {
                            // example executeHelper for this task
                            // var zoomData = new Dictionary<string, Object> { { "zoom", 2f }, { "zoomSpeedMultiplier", 2f }, { "setInstantly", false } };

                            World world = World.GetTopWorld();
                            if (world == null) return;

                            var zoomData = (Dictionary<string, Object>)this.ExecuteHelper;
                            float zoom = (float)zoomData["zoom"];
                            bool setInstantly = zoomData.ContainsKey("setInstantly") && (bool)zoomData["setInstantly"];
                            float zoomSpeedMultiplier = zoomData.ContainsKey("zoomSpeedMultiplier") ? (float)zoomData["zoomSpeedMultiplier"] : 1f;

                            world.camera.SetZoom(zoom: zoom, zoomSpeedMultiplier: zoomSpeedMultiplier, setInstantly: setInstantly);

                            return;
                        }

                    case TaskName.CameraSetMovementSpeed:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            float movementSpeed = (float)this.ExecuteHelper;

                            world.camera.SetMovementSpeed(movementSpeed);
                            return;
                        }

                    case TaskName.CameraResetMovementSpeed:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            world.camera.SetMovementSpeed(1f);
                            return;
                        }

                    case TaskName.ShowCookingProgress:
                        {
                            Cooker cooker = (Cooker)this.ExecuteHelper;
                            cooker.ShowCookingProgress();

                            return;
                        }

                    case TaskName.ShowBrewingProgress:
                        {
                            AlchemyLab alchemyLab = (AlchemyLab)this.ExecuteHelper;
                            alchemyLab.ShowBrewingProgress();

                            return;
                        }

                    case TaskName.RestoreHints:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;
                            world.HintEngine.RestoreAllHints();

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
                            else new Task(taskName: TaskName.QuitGame);

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
                            // used to deduplicate unboxing code in Scheduler and WorldEvent

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

                    case TaskName.ShowHint:
                        {
                            World world = World.GetTopWorld();
                            if (world == null) return;

                            var hintType = (HintEngine.Type)this.ExecuteHelper;
                            world.HintEngine.ShowGeneralHint(type: hintType, ignoreDelay: true);

                            return;
                        }

                    case TaskName.ShowTutorialInMenu:
                        {
                            Tutorials.ShowTutorialInMenu(type: (Tutorials.Type)this.ExecuteHelper);

                            return;
                        }

                    case TaskName.ShowTutorialInGame:
                        {
                            var tutorialData = (Dictionary<string, Object>)this.ExecuteHelper;

                            // example executeHelper for this task
                            // var tutorialData = new Dictionary<string, Object> { { "tutorial", Tutorials.Type.KeepingAnimalsAway }, { "world", world }, { "ignoreHintsSetting", false }, { "ignoreDelay", false }, { "ignoreIfShown", false } };

                            Tutorials.Type type = (Tutorials.Type)tutorialData["tutorial"];
                            World worldToUse = (World)tutorialData["world"];

                            bool ignoreHintsSetting = tutorialData.ContainsKey("ignoreHintsSetting") && (bool)tutorialData["ignoreHintsSetting"];
                            bool ignoreDelay = tutorialData.ContainsKey("ignoreDelay") && (bool)tutorialData["ignoreDelay"];
                            bool ignoreIfShown = !tutorialData.ContainsKey("ignoreIfShown") || (bool)tutorialData["ignoreIfShown"];

                            Tutorials.ShowTutorialOnTheField(type: type, world: worldToUse, ignoreHintsSetting: ignoreHintsSetting, ignoreDelay: ignoreDelay, ignoreIfShown: ignoreIfShown);

                            return;
                        }

                    case TaskName.ReturnToMainMenu:
                        SonOfRobinGame.SmallProgressBar.TurnOff();
                        CloseGame(quitGame: false);
                        return;

                    case TaskName.QuitGame:
                        SonOfRobinGame.SmallProgressBar.TurnOff();
                        CloseGame(quitGame: true);
                        return;

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
                            // var solidColorData = new Dictionary<string, Object> { { "color", Color.Red }, { "opacity", 0.5f } };

                            var solidColorData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Color color = (Color)solidColorData["color"];
                            float opacity = (float)solidColorData["opacity"];

                            SolidColor newOverlay = new(color: color, viewOpacity: opacity);
                            world.solidColorManager.Add(newOverlay);

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

                            var blockedIgnoredTypesForNonPlants = new List<Type> { };

                            foreach (Sprite sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                            {
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
                            if (duplicatedPiecesByID.Count > 0)
                            {
                                errorsFound.Add($"Duplicated pieces ({duplicatedPiecesByID.Count}):");

                                foreach (List<BoardPiece> duplicatedPieceList in duplicatedPiecesByID.Values)
                                {
                                    errorsFound.Add($"- {duplicatedPieceList[0].readableName} ID {duplicatedPieceList[0].id} x{duplicatedPieceList.Count}");
                                }
                            }

                            bool isCorrect = errorsFound.Count == 0;
                            string message = errorsFound.Count > 0 ? String.Join("\n", errorsFound) : "No incorrect pieces found.";

                            // showing message

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
;
                            return;
                        }

                    case TaskName.SetPlayerPointWalkTarget:
                        {
                            var setData = (Dictionary<Player, Vector2>)this.ExecuteHelper;

                            foreach (var kvp in setData)
                            {
                                kvp.Key.pointWalkTarget = kvp.Value;
                            }

                            return;
                        }

                    case TaskName.StopSound:
                        {
                            Sound sound = (Sound)this.ExecuteHelper;
                            sound.Stop(skipFade: true);

                            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{sound.Id} {sound.SoundNameList[0]} fade out ended - stopping.");

                            return;
                        }

                    case TaskName.RemoveAllScenesOfType:
                        {
                            Type type = (Type)this.ExecuteHelper;
                            Scene.RemoveAllScenesOfType(type);

                            return;
                        }

                    case TaskName.ActivateLightEngine:
                        {
                            LightEngine lightEngine = (LightEngine)this.ExecuteHelper;
                            lightEngine.Activate();

                            return;
                        }

                    case TaskName.DeactivateLightEngine:
                        {
                            LightEngine lightEngine = (LightEngine)this.ExecuteHelper;
                            lightEngine.Deactivate();

                            return;
                        }

                    case TaskName.AddPassiveMovement:
                        {
                            // example executeHelper for this task
                            // var movementData = new Dictionary<string, Object> { { "boardPiece", piece }, { "movement",  movement }};

                            var movementData = (Dictionary<string, Object>)this.ExecuteHelper;

                            BoardPiece piece = (BoardPiece)movementData["boardPiece"];
                            Vector2 movement = (Vector2)movementData["movement"];

                            if (piece.sprite.IsOnBoard) piece.AddPassiveMovement(movement: movement, force: true);

                            return;
                        }

                    case TaskName.AddFadeInAnim: // not in use at the moment (replaced with PieceTemplate.CreateAndPlaceOnBoard() functionality)
                        {
                            Sprite sprite = (Sprite)this.ExecuteHelper;
                            if (!sprite.IsOnBoard) return;

                            if (sprite.IsInCameraRect)
                            {
                                sprite.opacity = 0f;
                                new OpacityFade(sprite: sprite, destOpacity: 1f);
                            }
                            else sprite.opacity = 1f;

                            return;
                        }

                    case TaskName.InteractWithCooker:
                        {
                            Cooker cooker = (Cooker)this.ExecuteHelper;
                            TaskName taskName = cooker.IsOn ? TaskName.ShowCookingProgress : TaskName.OpenContainer;
                            new Task(taskName: taskName, delay: 0, executeHelper: this.ExecuteHelper);
                            return;
                        }

                    case TaskName.InteractWithLab:
                        {
                            AlchemyLab alchemyLab = (AlchemyLab)this.ExecuteHelper;
                            TaskName taskName = alchemyLab.IsOn ? TaskName.ShowBrewingProgress : TaskName.OpenContainer;
                            new Task(taskName: taskName, delay: 0, executeHelper: this.ExecuteHelper);
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
                                   blockInput: true, imageList: new List<Texture2D> { totem.sprite.CroppedAnimFrame.texture }),
                                   });
                            }

                            return;
                        }

                    case TaskName.InventoryCombineItems:
                        {
                            Inventory inventory = (Inventory)this.ExecuteHelper;
                            StorageSlot activeSlot = inventory.ActiveSlot;

                            BoardPiece piece1 = inventory.draggedPieces[0];
                            inventory.draggedPieces.RemoveAt(0);
                            BoardPiece piece2 = activeSlot.TopPiece;

                            BoardPiece combinedPiece = PieceCombiner.TryToCombinePieces(piece1: piece1, piece2: piece2);
                            if (combinedPiece == null) throw new ArgumentNullException($"Previously checked combination of {piece1.readableName} and {piece2.readableName} failed.");

                            activeSlot.RemoveTopPiece();

                            if (activeSlot.CanFitThisPiece(combinedPiece)) activeSlot.AddPiece(combinedPiece);
                            else activeSlot.storage.AddPiece(piece: combinedPiece, dropIfDoesNotFit: true);

                            Inventory.soundCombine.Play();
                            new RumbleEvent(force: 0.27f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.085f, fadeOutSeconds: 0.085f);

                            new TextWindow(text: $"{piece1.readableName} | + {piece2.readableName} | = {combinedPiece.readableName} |", imageList: new List<Texture2D> { piece1.sprite.CroppedAnimFrame.texture, piece2.sprite.CroppedAnimFrame.texture, combinedPiece.sprite.AnimFrame.texture }, textColor: Color.White, bgColor: new Color(0, 214, 222), useTransition: true, animate: true);

                            return;
                        }

                    case TaskName.InventoryApplyPotion:
                        {
                            Inventory inventory = (Inventory)this.ExecuteHelper;
                            inventory.TryToApplyPotion(slot: inventory.ActiveSlot, execute: true);

                            return;
                        }

                    case TaskName.InventoryReleaseHeldPieces:
                        {
                            Inventory inventory = (Inventory)this.ExecuteHelper;
                            inventory.ReleaseHeldPieces(slot: inventory.ActiveSlot, forceReleaseAll: true);
                            return;
                        }

                    case TaskName.RemoveBuffs:
                        {
                            // example executeHelper for this task
                            // var removeData = new Dictionary<string, Object> { { "storagePiece", piece }, { "buffList",  buffList }};

                            var removeData = (Dictionary<string, Object>)this.ExecuteHelper;
                            BoardPiece storagePiece = (BoardPiece)removeData["storagePiece"];
                            List<Buff> buffList = (List<Buff>)removeData["buffList"];

                            storagePiece.buffEngine.RemoveBuffs(buffList);
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
                                if (!highlightOnly) new TextWindow(text: $"It is too dark to plant | {PieceInfo.GetInfo(plantName).readableName}.", imageList: new List<Texture2D> { PieceInfo.GetTexture(plantName) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 70, priority: 0, animSound: world.DialogueSound);

                                return;
                            }

                            if (player.IsVeryTired)
                            {
                                if (!highlightOnly) new TextWindow(text: "I'm too tired to plant anything...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 70, priority: 0, animSound: world.DialogueSound);

                                return;
                            }

                            if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
                            {
                                if (!highlightOnly) new TextWindow(text: $"I can't plant | {PieceInfo.GetInfo(plantName).readableName} with enemies nearby.", imageList: new List<Texture2D> { PieceInfo.GetTexture(plantName) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: false, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 70, priority: 0, animSound: world.DialogueSound);

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

                    case TaskName.DestroyAndDropDebris:
                        {
                            BoardPiece piece = (BoardPiece)this.ExecuteHelper;

                            piece.pieceInfo.Yield?.DropDebris(piece: piece);
                            piece.Destroy();

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

                    case TaskName.SetAllNamedLocationsAsDiscovered:
                        {
                            World world = (World)this.ExecuteHelper;
                            world.Grid.namedLocations.SetAllLocationsAsDiscovered();

                            return;
                        }

                    case TaskName.MakePlayerJumpOverThisPiece:
                        {
                            BoardPiece obstacle = (BoardPiece)this.ExecuteHelper;
                            Player player = obstacle.world.Player;

                            Vector2 obstaclePos = obstacle.sprite.position;
                            Vector2 playerPos = player.sprite.position;

                            bool horizontal = obstacle.sprite.AnimFrame.colWidth > obstacle.sprite.AnimFrame.colHeight;

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

                    case TaskName.AddRumble:
                        {
                            // example executeHelper for this task
                            // var rumbleData = new Dictionary<string, Object> { { "force", 0.3f }, { "bigMotor", true }, { "fadeInSeconds", 0.6f }, { "durationSeconds", 1.5f }, { "fadeOutSeconds", 0.6f } };

                            var rumbleData = (Dictionary<string, Object>)this.ExecuteHelper;

                            float force = (float)rumbleData["force"];
                            bool smallMotor = rumbleData.ContainsKey("smallMotor") && (bool)rumbleData["smallMotor"];
                            bool bigMotor = rumbleData.ContainsKey("bigMotor") && (bool)rumbleData["bigMotor"];
                            float fadeInSeconds = rumbleData.ContainsKey("fadeInSeconds") ? (float)rumbleData["fadeInSeconds"] : 0f;
                            float durationSeconds = (float)rumbleData["durationSeconds"];
                            float fadeOutSeconds = rumbleData.ContainsKey("fadeOutSeconds") ? (float)rumbleData["fadeOutSeconds"] : 0f;
                            float minSecondsSinceLastRumbleSmallMotor = rumbleData.ContainsKey("minSecondsSinceLastRumbleSmallMotor") ? (float)rumbleData["minSecondsSinceLastRumbleSmallMotor"] : 0f;
                            float minSecondsSinceLastRumbleBigMotor = rumbleData.ContainsKey("minSecondsSinceLastRumbleBigMotor") ? (float)rumbleData["minSecondsSinceLastRumbleBigMotor"] : 0f;

                            new RumbleEvent(force: force, smallMotor: smallMotor, bigMotor: bigMotor, fadeInSeconds: fadeInSeconds, durationSeconds: durationSeconds, fadeOutSeconds: fadeOutSeconds, minSecondsSinceLastRumbleSmallMotor: minSecondsSinceLastRumbleSmallMotor, minSecondsSinceLastRumbleBigMotor: minSecondsSinceLastRumbleBigMotor);

                            return;
                        }

                    case TaskName.TurnOnWindParticles:
                        {
                            BoardPiece piece = (BoardPiece)this.ExecuteHelper;
                            Sprite sprite = piece.sprite;
                            float windOriginX = piece.world.weather.WindOriginX;

                            foreach (var kvp in piece.pieceInfo.windParticlesDict)
                            {
                                ParticleEngine.Preset preset = kvp.Key;
                                Color color = kvp.Value;

                                bool hasPreset = sprite.particleEngine != null && sprite.particleEngine.HasPreset(preset); // to prevent from adding modifiers more than once
                                ParticleEngine.TurnOn(sprite: sprite, preset: preset, particlesToEmit: sprite.BlocksMovement ? 5 : 2, duration: sprite.BlocksMovement ? 2 : 1);

                                Random random = piece.world.random;

                                ParticleEmitter particleEmitter = ParticleEngine.GetEmitterForPreset(sprite: sprite, preset: preset);
                                if (!hasPreset && particleEmitter != null)
                                {
                                    particleEmitter.Parameters.Color = HslColor.FromRgb(color);
                                    particleEmitter.Profile = Profile.Spray(direction: windOriginX == 0 ? Vector2.UnitX : -Vector2.UnitX, spread: 2f);

                                    Vector2 windDirection = new Vector2(windOriginX == 0 ? 1f : -1f, 0.2f);
                                    particleEmitter.Modifiers.Add(new LinearGravityModifier { Direction = windDirection, Strength = random.Next(60, 250) });

                                    int vortexCount = random.Next(1, 4);
                                    for (int i = 0; i < vortexCount; i++)
                                    {
                                        int vortexX = (i * 250) + (random.Next(60, 180) * (windOriginX == 0 ? 1 : -1));
                                        if (i == 0) vortexX = 0;

                                        float angle1 = random.NextSingle() * (float)Math.PI * 2;
                                        float angle2 = angle1 - (float)Math.PI;
                                        int distance = random.Next(100, 300);

                                        Vector2 vortexOffset1 = new((int)Math.Round(distance * Math.Cos(angle1)), (int)Math.Round(distance * Math.Sin(angle1)));
                                        Vector2 vortexOffset2 = new((int)Math.Round(distance * Math.Cos(angle2)), (int)Math.Round(distance * Math.Sin(angle2)));

                                        vortexOffset1.X += vortexX;
                                        vortexOffset2.X += vortexX;

                                        foreach (Vector2 offset in new Vector2[] { vortexOffset1, vortexOffset2 })
                                        {
                                            particleEmitter.Modifiers.Add(new VortexModifier
                                            {
                                                Mass = random.Next(10, 35),
                                                MaxSpeed = 0.5f,
                                                Position = offset,
                                            });
                                        }
                                    }
                                }
                            }

                            return;
                        }

                    case TaskName.DisposeSaveScreenshotsIfNoMenuPresent:
                        {
                            if (Scene.GetTopSceneOfType(typeof(Menu)) != null) new Task(taskName: TaskName.DisposeSaveScreenshotsIfNoMenuPresent, delay: 60 * 3);
                            else SaveHeaderInfo.DisposeScreenshots();

                            return;
                        }

                    case TaskName.SetGlobalWorldEffect:
                        {
                            World world = World.GetTopWorld();
                            if (world != null) world.globalEffect = (EffInstance)this.ExecuteHelper;

                            return;
                        }

                    case TaskName.SetGlobalWorldTweener:
                        {
                            // example executeHelper for this task
                            // var tweenData = new Dictionary<string, Object> { { "startIntensity", 1f }, { "tweenIntensity", 0f }, { "autoreverse", true }, { "easing", "SineOut" }, { "durationSeconds", 1f } };

                            World world = World.GetTopWorld();

                            if (world.globalEffect == null)
                            {
                                MessageLog.Add(debugMessage: true, text: $"Scheduler: world doesn't have globalEffect. Cannot apply tweener.");
                                return;
                            }

                            var tweenData = (Dictionary<string, Object>)this.ExecuteHelper;

                            float tweenIntensity = (float)tweenData["tweenIntensity"];
                            bool autoreverse = tweenData.ContainsKey("autoreverse") && (bool)tweenData["autoreverse"];
                            float durationSeconds = (float)tweenData["durationSeconds"];
                            string easing = (string)tweenData["easing"];

                            world.tweenerForGlobalEffect.CancelAndCompleteAll();

                            if (tweenData.ContainsKey("startIntensity"))
                            {
                                float startIntensity = (float)tweenData["startIntensity"];
                                world.globalEffect.intensityForTweener = startIntensity;
                            }

                            world.tweenerForGlobalEffect.TweenTo(target: world.globalEffect, expression: effect => effect.intensityForTweener, toValue: tweenIntensity, duration: durationSeconds);

                            Tween tween = world.tweenerForGlobalEffect.FindTween(target: world.globalEffect, memberName: "intensityForTweener");

                            if (autoreverse) tween.AutoReverse();

                            if (easing == "QuadraticIn") tween.Easing(EasingFunctions.QuadraticIn);
                            else if (easing == "SineInOut") tween.Easing(EasingFunctions.SineInOut);
                            else if (easing == "SineOut") tween.Easing(EasingFunctions.SineOut);
                            else if (easing == "Linear") tween.Easing(EasingFunctions.Linear);

                            return;
                        }

                    case TaskName.UseEntrance:
                        {
                            Entrance entrance = (Entrance)this.ExecuteHelper;
                            entrance.Enter();
                            return;
                        }

                    case TaskName.UseBoat:
                        {
                            BoardPiece boat = (BoardPiece)this.ExecuteHelper;
                            boat.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.CineEndingPart1, ignoreDelay: true, piece: boat);

                            return;
                        }

                    case TaskName.IslandClockAdvance:
                        {
                            // example executeHelper for this task
                            // var clockAdvanceData = new Dictionary<string, Object> { { "islandClock", this.world.islandClock }, { "amount", 60 * 60 * 4 }, { "ignorePause", true }, { "ignoreMultiplier", false } };

                            var clockAdvanceData = (Dictionary<string, Object>)this.ExecuteHelper;

                            IslandClock islandClock = (IslandClock)clockAdvanceData["islandClock"];
                            int amount = (int)clockAdvanceData["amount"];
                            bool ignorePause = clockAdvanceData.ContainsKey("ignorePause") ? (bool)clockAdvanceData["ignorePause"] : false;
                            bool ignoreMultiplier = clockAdvanceData.ContainsKey("ignoreMultiplier") ? (bool)clockAdvanceData["ignoreMultiplier"] : false;

                            islandClock.Advance(amount: amount, ignorePause: ignorePause, ignoreMultiplier: ignoreMultiplier);

                            return;
                        }

                    case TaskName.ChangeActiveState:
                        {
                            // example executeHelper for this task
                            // var stateData = new Dictionary<string, Object> { { "boardPiece", player }, { "state", State.PlayerControlledWalking } };

                            var stateData = (Dictionary<string, Object>)this.ExecuteHelper;

                            BoardPiece boardPiece = (BoardPiece)stateData["boardPiece"];
                            BoardPiece.State state = (BoardPiece.State)stateData["state"];

                            boardPiece.activeState = state;
                            boardPiece.AddToStateMachines();

                            return;
                        }

                    case TaskName.ChangeTipsLayout:
                        {
                            // example executeHelper for this task
                            // var tipsData = new Dictionary<string, Object> { { "scene", world }, { "layout", ControlTips.TipsLayout.Empty } };

                            var tipsData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Scene scene = (Scene)tipsData["scene"];
                            ControlTips.TipsLayout layout = (ControlTips.TipsLayout)tipsData["layout"];

                            scene.tipsLayout = layout;

                            return;
                        }

                    case TaskName.ChangeTouchLayout:
                        {
                            // example executeHelper for this task
                            // var touchData = new Dictionary<string, Object> { { "scene", world }, { "layout", TouchLayout.Empty } };

                            var touchData = (Dictionary<string, Object>)this.ExecuteHelper;

                            Scene scene = (Scene)touchData["scene"];
                            TouchLayout layout = (TouchLayout)touchData["layout"];

                            scene.touchLayout = layout;

                            return;
                        }

                    case TaskName.FinishConstructionAnimation:
                        {
                            BoardPiece constructionSite = (BoardPiece)this.ExecuteHelper;
                            ConstructionSite.FinishConstructionAnimation(constructionSite);
                            return;
                        }

                    case TaskName.OpenBoatMenu:
                        {
                            BoardPiece boatPiece = (BoardPiece)this.ExecuteHelper;
                            boatPiece.world.OpenMenu(templateName: MenuTemplate.Name.Boat, executeHelper: this.ExecuteHelper);

                            return;
                        }

                    case TaskName.EnterNewLevel:
                        {
                            Level level = (Level)this.ExecuteHelper;
                            level.world.EnterNewLevel(level);

                            return;
                        }

                    default:
                        throw new ArgumentException($"Unsupported taskName - {taskName}.");
                }
            }

            private static void CloseGame(bool quitGame)
            {
                World world = World.GetTopWorld();

                var worldScenes = Scene.GetAllScenesOfType(typeof(World));
                foreach (World currWorld in worldScenes)
                {
                    if (!currWorld.demoMode) world.Remove();
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