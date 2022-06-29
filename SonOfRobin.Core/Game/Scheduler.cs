using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class Scheduler
    {
        public enum TaskName { Empty, CreateNewWorld, CreateNewWorldNow, QuitGame, OpenMainMenu, OpenCreateMenu, OpenIslandTemplateMenu, OpenSetSeedMenu, OpenOptionsMenu, OpenTutorialsMenu, OpenLoadMenu, OpenSaveMenu, OpenDebugMenu, OpenConfirmationMenu, OpenCreateAnyPieceMenu, OpenGameOverMenu, SaveGame, LoadGame, LoadGameNow, ReturnToMainMenu, SavePrefs, ProcessConfirmation, OpenCraftMenu, Craft, Hit, CreateNewPiece, CreateDebugPieces, OpenContainer, DeleteObsoleteSaves, DropFruit, GetEaten, ExecuteTaskWithDelay, AddWorldEvent, OpenTextWindow, SleepInsideShelter, SleepOutside, TempoFastForward, TempoStop, TempoPlay, CameraTrackPiece, CameraTrackCoords, CameraSetZoom, ShowCookingProgress, RestoreHints, OpenMainMenuIfSpecialKeysArePressed, CheckForPieceHints, ShowHint, ExecuteTaskList, ExecuteTaskChain, ShowTutorial, RemoveScene, ChangeSceneInputType, SetCineMode, AddTransition, SkipCinematics, DeleteTemplates, SetSpectatorMode, SwitchLightSource }

        private readonly static Dictionary<int, List<Task>> queue = new Dictionary<int, List<Task>>();
        private static int inputTurnedOffUntilFrame = 0;

        public static void ProcessQueue()
        {
            if (SonOfRobinGame.currentUpdate >= inputTurnedOffUntilFrame) Input.GlobalInputActive = true;

            var framesToProcess = queue.Keys.Where(frameNo => SonOfRobinGame.currentUpdate >= frameNo).ToList();
            if (framesToProcess.Count == 0) return;

            foreach (int frameNo in framesToProcess)
            {
                foreach (Task task in queue[frameNo]) // if System.InvalidOperationException occurs, most likely some task has been added to current frame queue
                { task.Execute(); }

                queue.Remove(frameNo);
            }
        }

        public static void RemoveAllTasksOfName(TaskName taskName)
        {
            foreach (int frameNo in queue.Keys.ToList())
            {
                queue[frameNo] = queue[frameNo].Where(task => task.taskName != taskName).ToList();
            }
        }

        public struct Task
        {
            private readonly Menu menu;
            public readonly TaskName taskName;
            private Object executeHelper;
            private readonly int delay;
            private int frame;
            private readonly bool turnOffInputUntilExecution;
            private bool rebuildsMenu;

            public string TaskText
            {
                get
                {
                    string turnOffInputText = this.turnOffInputUntilExecution ? "(input off)" : "";
                    return $"{this.taskName} {turnOffInputText}";
                }
            }

            public Task(Menu menu, TaskName taskName, Object executeHelper, bool turnOffInputUntilExecution = false, int delay = 0, bool rebuildsMenu = false, bool storeForLaterUse = false)
            {
                this.taskName = taskName;
                this.executeHelper = executeHelper;
                this.menu = menu;
                this.turnOffInputUntilExecution = turnOffInputUntilExecution;
                this.delay = delay;
                this.rebuildsMenu = rebuildsMenu;

                if (!storeForLaterUse)
                {
                    this.frame = SonOfRobinGame.currentUpdate + this.delay;

                    if (this.turnOffInputUntilExecution) this.TurnOffInput();

                    this.Process();
                }
                else this.frame = -1;
            }

            private void TurnOffInput()
            {
                Input.GlobalInputActive = false;
                inputTurnedOffUntilFrame = Math.Max(this.frame, inputTurnedOffUntilFrame);
            }

            public void Process()
            {
                // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{SonOfRobinGame.currentUpdate} processing task {this.TaskText}", color: Color.LightYellow);

                if (this.delay <= 0) this.Execute();
                else this.AddToQueue();
            }

            private void AddToQueue()
            {
                if (this.delay == 0) throw new ArgumentException($"Tried to add task with delay {this.delay} to queue.");

                //  MessageLog.AddMessage(msgType: MsgType.User, message: $"Adding to queue '{this.taskName}' - delay {this.delay}.", color: Color.White); // for testing

                if (this.turnOffInputUntilExecution) this.TurnOffInput();
                if (this.frame == -1) this.frame = SonOfRobinGame.currentUpdate + this.delay;
                if (!queue.ContainsKey(this.frame)) queue[this.frame] = new List<Task>();
                queue[this.frame].Add(this);
            }

            public void Execute()
            {
                this.RunTask();
                if (this.rebuildsMenu) this.menu.Rebuild();
            }

            private void RunTask()
            {
                World world;

                switch (taskName)
                {
                    case TaskName.Empty:
                        return;

                    case TaskName.OpenMainMenu:
                        OpenMenu(templateName: MenuTemplate.Name.Main, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenCreateMenu:
                        OpenMenu(templateName: MenuTemplate.Name.CreateNewIsland, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenIslandTemplateMenu:
                        OpenMenu(templateName: MenuTemplate.Name.OpenIslandTemplate, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenSetSeedMenu:
                        OpenMenu(templateName: MenuTemplate.Name.SetSeed, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenLoadMenu:
                        OpenMenu(templateName: MenuTemplate.Name.Load, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenSaveMenu:
                        OpenMenu(templateName: MenuTemplate.Name.Save, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenOptionsMenu:
                        OpenMenu(templateName: MenuTemplate.Name.Options, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenTutorialsMenu:
                        OpenMenu(templateName: MenuTemplate.Name.Tutorials, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenDebugMenu:
                        OpenMenu(templateName: MenuTemplate.Name.Debug, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenCreateAnyPieceMenu:
                        OpenMenu(templateName: MenuTemplate.Name.CreateAnyPiece, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenGameOverMenu:
                        Scene.RemoveAllScenesOfType(typeof(TextWindow));

                        OpenMenu(templateName: MenuTemplate.Name.GameOver, executeHelper: executeHelper);
                        return;

                    case TaskName.OpenCraftMenu:
                        {
                            Workshop workshop = (Workshop)executeHelper;

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);
                            if (menu == null) return; // if crafting was impossible at the moment

                            workshop.TurnOn();

                            var worldEventData = new Dictionary<string, object> {
                                {"boardPiece", workshop },
                                {"delay", 300 },
                                {"eventName", WorldEvent.EventName.TurnOffWorkshop },
                            };

                            menu.AddClosingTask(closingTask: TaskName.AddWorldEvent, closingTaskHelper: worldEventData);
                            return;
                        }

                    case TaskName.OpenConfirmationMenu:
                        MenuTemplate.CreateConfirmationMenu(confirmationData: executeHelper);
                        return;

                    case TaskName.CreateNewWorld:
                        if (menu != null) menu.MoveToTop();

                        Scene.RemoveAllScenesOfType(typeof(Menu));

                        new Task(menu: null, taskName: TaskName.CreateNewWorldNow, turnOffInputUntilExecution: true, delay: 13, executeHelper: executeHelper);

                        return;

                    case TaskName.CreateNewWorldNow:

                        {
                            // example executeHelper for this task
                            // var createData = new Dictionary<string, Object> { { "width", width }, { "height", height }, { "seed", seed }};

                            var createData = (Dictionary<string, Object>)executeHelper;
                            int width = (int)createData["width"];
                            int height = (int)createData["height"];
                            int seed = (int)createData["seed"];

                            new World(width: width, height: height, seed: seed);

                            return;
                        }

                    case TaskName.CreateNewPiece:
                        world = World.GetTopWorld();
                        if (world != null)
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Vector2 position = (Vector2)executeData["position"];
                            PieceTemplate.Name templateName = (PieceTemplate.Name)executeData["templateName"];
                            BoardPiece piece = PieceTemplate.CreateOnBoard(world: world, position: position, templateName: templateName);
                            if (piece.sprite.placedCorrectly) piece.sprite.MoveToClosestFreeSpot(position);
                        }
                        return;

                    case TaskName.CreateDebugPieces:
                        world = World.GetTopWorld();
                        if (world == null)
                        {
                            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Could not create selected item, because no world was found.");
                            return;
                        }

                        {
                            Player player = world.player;

                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Vector2 position = (Vector2)executeData["position"];
                            PieceTemplate.Name templateName = (PieceTemplate.Name)executeData["templateName"];

                            int piecesCreated = 0;
                            int amountToCreate = 0;
                            int attemptNo = 0;
                            BoardPiece piece;

                            while (true)
                            {
                                piece = PieceTemplate.CreateOnBoard(world: world, position: position, templateName: templateName);
                                amountToCreate = piece.stackSize;

                                if (piece.sprite.placedCorrectly)
                                {
                                    bool piecePickedUp = player.PickUpPiece(piece);

                                    if (!piecePickedUp) piece.sprite.MoveToClosestFreeSpot(position);
                                    piecesCreated++;
                                }

                                if (piecesCreated >= amountToCreate) break;

                                attemptNo++;
                                if (attemptNo == 1000)
                                {
                                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Max number of attempts exceeded while trying to create '{templateName}'.");
                                    break;
                                }
                            }

                            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{piecesCreated} '{templateName}' pieces created.");
                        }
                        return;

                    case TaskName.SaveGame:
                        // example executeHelper for this task
                        // var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", "1" }, { "showMessage", false }, {"quitGameAfterSaving", false} };

                        if (this.rebuildsMenu)
                        {// menu should be rebuilt after the game has been saved
                            new Task(menu: this.menu, taskName: TaskName.Empty, turnOffInputUntilExecution: false, delay: 12, executeHelper: null, rebuildsMenu: true);
                            this.rebuildsMenu = false;
                        }
;
                        var saveParams = (Dictionary<string, Object>)executeHelper;
                        world = (World)saveParams["world"];
                        string saveSlotName = (string)saveParams["saveSlotName"];
                        bool showMessage = false;
                        if (saveParams.ContainsKey("showMessage")) showMessage = (bool)saveParams["showMessage"];
                        bool quitGameAfterSaving = false;
                        if (saveParams.ContainsKey("quitGameAfterSaving")) quitGameAfterSaving = (bool)saveParams["quitGameAfterSaving"];

                        new LoaderSaver(saveMode: true, saveSlotName: saveSlotName, world: world, showSavedMessage: showMessage, quitGameAfterSaving: quitGameAfterSaving);

                        return;

                    case TaskName.LoadGame:
                        if (menu != null) menu.MoveToTop();

                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Load);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Main);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Pause);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.GameOver);

                        new Task(menu: null, taskName: TaskName.LoadGameNow, turnOffInputUntilExecution: true, delay: 17, executeHelper: this.executeHelper);

                        return;

                    case TaskName.LoadGameNow:
                        new LoaderSaver(saveMode: false, saveSlotName: (string)executeHelper);

                        return;

                    case TaskName.SavePrefs:
                        Preferences.Save();
                        return;

                    case TaskName.ProcessConfirmation:
                        var confirmationData = (Dictionary<string, Object>)executeHelper;
                        new Task(menu: null, taskName: (TaskName)confirmationData["taskName"], executeHelper: confirmationData["executeHelper"]);

                        return;

                    case TaskName.OpenContainer:
                        {
                            BoardPiece container = (BoardPiece)executeHelper;

                            world = World.GetTopWorld();
                            if (world == null) return;

                            if (container.GetType() == typeof(Cooker) && world.player.AreEnemiesNearby)
                            {
                                new TextWindow(text: "I can't cook with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
                                return;
                            }

                            Scene.SetInventoryLayout(newLayout: Scene.InventoryLayout.InventoryAndChest, player: world.player, chest: container);
                        }

                        return;

                    case TaskName.Craft:
                        {
                            var recipe = (Craft.Recipe)executeHelper;

                            recipe.TryToProducePieces(player: World.GetTopWorld().player);
                        }
                        return;

                    case TaskName.Hit:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            Tool activeTool = (Tool)executeData["toolbarPiece"];
                            int shootingPower = (int)executeData["shootingPower"];
                            int offsetX = (int)executeData["offsetX"];
                            int offsetY = (int)executeData["offsetY"];
                            bool highlightOnly = (bool)executeData["highlightOnly"];
                            world = player.world;

                            if (activeTool.shootsProjectile)
                            {
                                activeTool.Use(shootingPower: shootingPower, targets: null);
                                return;
                            }

                            var targets = new List<BoardPiece> { };

                            var nearbyPieces = world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: activeTool.range == 0 ? (ushort)60 : (ushort)activeTool.range, offsetX: offsetX, offsetY: offsetY, compareWithBottom: true);

                            nearbyPieces = nearbyPieces.Where(piece => piece.yield != null && piece.exists).ToList();

                            if (activeTool.range == 0)
                            {
                                var animals = nearbyPieces.Where(piece => piece.GetType() == typeof(Animal)).ToList();
                                if (animals.Count > 0) nearbyPieces = animals;

                                BoardPiece targetPiece = BoardPiece.FindClosestPiece(sprite: player.sprite, pieceList: nearbyPieces, offsetX: offsetX, offsetY: offsetY);
                                if (targetPiece == null) return;
                                targets.Add(targetPiece);
                            }
                            else
                            {
                                Rectangle areaRect = new Rectangle(x: 0, y: 0, width: world.width, height: world.height);

                                switch (player.sprite.orientation)
                                {
                                    case Sprite.Orientation.left:
                                        areaRect.Width = player.sprite.gfxRect.Left;
                                        break;

                                    case Sprite.Orientation.right:
                                        areaRect.X = player.sprite.gfxRect.Center.X;
                                        areaRect.Width = world.width - player.sprite.gfxRect.Center.X;
                                        break;

                                    case Sprite.Orientation.up:
                                        areaRect.Height = player.sprite.gfxRect.Center.Y;
                                        break;

                                    case Sprite.Orientation.down:
                                        areaRect.Y = player.sprite.gfxRect.Center.Y;
                                        areaRect.Height = world.height - player.sprite.gfxRect.Center.Y;
                                        break;

                                    default:
                                        throw new ArgumentException($"Unsupported orientation - {player.sprite.orientation}.");
                                }

                                targets = nearbyPieces.Where(piece => areaRect.Contains(piece.sprite.position)).ToList();
                            }

                            activeTool.Use(shootingPower: shootingPower, targets: targets, highlightOnly: highlightOnly);

                            return;
                        }

                    case TaskName.GetEaten:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            BoardPiece food = (BoardPiece)executeData["toolbarPiece"];
                            bool highlightOnly = false;
                            if (executeData.ContainsKey("highlightOnly")) highlightOnly = (bool)executeData["highlightOnly"];

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (player.hitPoints == player.maxHitPoints && player.fedLevel >= player.maxFedLevel * 0.95)
                            {
                                if (!highlightOnly) new TextWindow(text: "I am full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

                                return;
                            }

                            if (highlightOnly)
                            {
                                if (SonOfRobinGame.platform == Platform.Mobile) VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            foreach (BuffEngine.Buff buff in food.buffList)
                            { player.buffEngine.AddBuff(buff: buff, world: World.GetTopWorld()); }

                            player.AcquireEnergy(food.Mass * 40f);

                            food.hitPoints = 0;
                        }
                        return;


                    case TaskName.SwitchLightSource:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            PortableLight portableLight = (PortableLight)executeData["toolbarPiece"];
                            bool highlightOnly = (bool)executeData["highlightOnly"];

                            if (highlightOnly)
                            {
                                if (SonOfRobinGame.platform == Platform.Mobile) VirtButton.ButtonHighlightOnNextFrame(VButName.UseTool);
                                ControlTips.TipHighlightOnNextFrame(tipName: "use item");
                                return;
                            }

                            bool buttonHeld = (bool)executeData["buttonHeld"];
                            if (buttonHeld) return;

                            portableLight.IsOn = !portableLight.IsOn;

                            return;
                        }

                    case TaskName.DropFruit:
                        {
                            Plant fruitPlant = (Plant)executeHelper;
                            fruitPlant.DropFruit();
                        }
                        return;

                    case TaskName.DeleteObsoleteSaves:
                        SaveHeaderManager.DeleteObsoleteSaves();
                        return;

                    case TaskName.ExecuteTaskWithDelay:
                        {
                            // example executeHelper for this task
                            // var delayData = new Dictionary<string, Object> { { "taskName", TaskName.TurnOffWorkshop }, { "executeHelper", workshop }, { "delay", 300 } };

                            var delayData = (Dictionary<string, Object>)executeHelper;
                            new Task(menu: null, taskName: (TaskName)delayData["taskName"], executeHelper: delayData["executeHelper"], delay: (int)delayData["delay"]);

                            return;
                        }

                    case TaskName.AddWorldEvent:
                        {
                            world = (World)Scene.GetTopSceneOfType(typeof(World));
                            if (world == null) return;

                            var worldEventData = (Dictionary<string, Object>)executeHelper;

                            WorldEvent.EventName eventName = (WorldEvent.EventName)worldEventData["eventName"];
                            BoardPiece boardPiece = (BoardPiece)worldEventData["boardPiece"];
                            int delay = (int)worldEventData["delay"];

                            new WorldEvent(eventName: eventName, world: world, delay: delay, boardPiece: boardPiece);

                            return;
                        }

                    case TaskName.OpenTextWindow:
                        {
                            var textWindowData = (Dictionary<string, Object>)executeHelper;

                            string text = (string)textWindowData["text"];

                            bool checkForDuplicate = false;
                            if (textWindowData.ContainsKey("checkForDuplicate")) checkForDuplicate = (bool)textWindowData["checkForDuplicate"];

                            bool useTransition = true;
                            if (textWindowData.ContainsKey("useTransition")) useTransition = (bool)textWindowData["useTransition"];

                            bool useTransitionOpen = false;
                            if (textWindowData.ContainsKey("useTransitionOpen")) useTransitionOpen = (bool)textWindowData["useTransitionOpen"];

                            bool useTransitionClose = false;
                            if (textWindowData.ContainsKey("useTransitionClose")) useTransitionClose = (bool)textWindowData["useTransitionClose"];

                            bool animate = true;
                            if (textWindowData.ContainsKey("animate")) useTransition = (bool)textWindowData["animate"];

                            TaskName closingTask = TaskName.Empty;
                            if (textWindowData.ContainsKey("closingTask")) closingTask = (TaskName)textWindowData["closingTask"];

                            Object closingTaskHelper = null;
                            if (textWindowData.ContainsKey("closingTaskHelper")) closingTaskHelper = textWindowData["closingTaskHelper"];

                            bool blocksUpdatesBelow = false;
                            if (textWindowData.ContainsKey("blocksUpdatesBelow")) blocksUpdatesBelow = (bool)textWindowData["blocksUpdatesBelow"];

                            int blockInputDuration = 0;
                            if (textWindowData.ContainsKey("blockInputDuration")) blockInputDuration = (int)textWindowData["blockInputDuration"];

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

                            new TextWindow(text: text, useTransition: useTransition, useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose, bgColor: bgColor, textColor: textColor, framesPerChar: framesPerChar, animate: animate, checkForDuplicate: checkForDuplicate, closingTask: closingTask, closingTaskHelper: closingTaskHelper, blockInputDuration: blockInputDuration, blocksUpdatesBelow: blocksUpdatesBelow);
                            return;
                        }

                    case TaskName.SleepOutside:
                        {
                            Player player = World.GetTopWorld()?.player;
                            if (player == null) return;

                            SleepEngine sleepEngine;

                            if (player.sprite.IsInWater)
                            {
                                if (player.sprite.CanDrownHere)
                                {
                                    player.hitPoints = 0;
                                    player.sprite.Visible = false;
                                    player.Kill();

                                    new TextWindow(text: "You have drowned.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 220);

                                    return;
                                }

                                sleepEngine = SleepEngine.OutdoorSleepWet;
                            }
                            else { sleepEngine = SleepEngine.OutdoorSleepDry; }

                            player.GoToSleep(sleepEngine: sleepEngine, zzzPos: player.sprite.position, wakeUpBuffs: new List<BuffEngine.Buff> { });
                            return;
                        }

                    case TaskName.SleepInsideShelter:
                        {
                            Shelter shelterPiece = (Shelter)executeHelper;
                            SleepEngine sleepEngine = shelterPiece.sleepEngine;
                            World.GetTopWorld()?.player.GoToSleep(sleepEngine: sleepEngine, zzzPos: new Vector2(shelterPiece.sprite.gfxRect.Center.X, shelterPiece.sprite.gfxRect.Center.Y), wakeUpBuffs: shelterPiece.buffList);

                            return;
                        }

                    case TaskName.TempoFastForward:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            SonOfRobinGame.game.IsFixedTimeStep = false;
                            world.updateMultiplier = (int)executeHelper;

                            return;
                        }

                    case TaskName.TempoStop:
                        {
                            world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;
                            if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
                            world.updateMultiplier = 0;

                            return;
                        }

                    case TaskName.TempoPlay:
                        {
                            world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
                            world.updateMultiplier = 1;

                            return;
                        }

                    case TaskName.CameraTrackPiece:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            BoardPiece piece = (BoardPiece)executeHelper;
                            world.camera.TrackPiece(piece);

                            return;
                        }

                    case TaskName.CameraTrackCoords:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            world.camera.TrackCoords(position: (Vector2)executeHelper);

                            return;
                        }

                    case TaskName.CameraSetZoom:
                        {
                            // example executeHelper for this task
                            // var saveParams = new Dictionary<string, Object> { { "zoom", 2f }, { "zoomSpeedMultiplier", 2f }};

                            world = World.GetTopWorld();
                            if (world == null) return;

                            var zoomData = (Dictionary<string, Object>)executeHelper;
                            float zoom = (float)zoomData["zoom"];
                            float zoomSpeedMultiplier = 1f;
                            if (zoomData.ContainsKey("zoomSpeedMultiplier")) zoomSpeedMultiplier = (float)zoomData["zoomSpeedMultiplier"];

                            world.camera.SetZoom(zoom: zoom, zoomSpeedMultiplier: zoomSpeedMultiplier);

                            return;
                        }

                    case TaskName.ShowCookingProgress:
                        {
                            Cooker cooker = (Cooker)executeHelper;
                            cooker.ShowCookingProgress();

                            return;
                        }

                    case TaskName.RestoreHints:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;
                            world.hintEngine.RestoreAllHints();

                            return;
                        }

                    case TaskName.ExecuteTaskList:
                        {
                            List<Object> taskList = (List<Object>)executeHelper;
                            foreach (Object taskObject in taskList)
                            {
                                Task task = (Task)taskObject;
                                task.Process();
                            }

                            return;
                        }

                    case TaskName.ExecuteTaskChain:
                        {
                            List<Object> taskChain = (List<Object>)executeHelper;

                            Task currentTask = (Task)taskChain[0];
                            taskChain.RemoveAt(0);

                            if (currentTask.taskName == TaskName.OpenTextWindow)
                            {
                                // If text window will be opened, the delay will depend on the player, so it is unknown.
                                // So, the next task should be run after closing this text window.

                                if (taskChain.Count > 0)
                                {
                                    var executeHelper = currentTask.executeHelper;
                                    var textWindowData = (Dictionary<string, Object>)executeHelper;
                                    textWindowData["closingTask"] = TaskName.ExecuteTaskChain;
                                    textWindowData["closingTaskHelper"] = taskChain;
                                    currentTask.executeHelper = textWindowData;
                                }
                                currentTask.Process();
                            }
                            else
                            {
                                // in other cases, the delay should be known

                                currentTask.Process(); // must go before new Task(), to maintain correct execute order

                                if (taskChain.Count > 0) new Task(menu: null, taskName: TaskName.ExecuteTaskChain, executeHelper: taskChain, delay: currentTask.delay, turnOffInputUntilExecution: true);
                            }

                            if (taskChain.Count == 0) Input.GlobalInputActive = true; // to ensure that input will be active at the end


                            return;
                        }

                    case TaskName.OpenMainMenuIfSpecialKeysArePressed:
                        {
                            if (Keyboard.IsPressed(Keys.LeftControl) ||
                                GamePad.IsPressed(playerIndex: PlayerIndex.One, button: Buttons.Start) ||
                                VirtButton.IsButtonDown(VButName.Interact))
                            {
                                new Task(menu: null, taskName: TaskName.OpenMainMenu, executeHelper: null);
                            }
                            else
                            {
                                new Task(menu: null, taskName: TaskName.QuitGame, executeHelper: null);
                            }

                            return;
                        }

                    case TaskName.CheckForPieceHints:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            List<PieceHint.Type> typesToCheckOnly = null;
                            if (executeHelper != null) typesToCheckOnly = (List<PieceHint.Type>)executeHelper;

                            world.hintEngine.CheckForPieceHintToShow(forcedMode: true, ignoreInputActive: true, typesToCheckOnly: typesToCheckOnly);

                            return;
                        }

                    case TaskName.ShowHint:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            var hintType = (HintEngine.Type)executeHelper;
                            world.hintEngine.ShowGeneralHint(type: hintType, ignoreDelay: true);

                            return;
                        }

                    case TaskName.ShowTutorial:
                        {
                            // For now ShowTutorial is only used in menus; should this change - menuMode would have to be passed here as well.

                            Tutorials.Type type = (Tutorials.Type)executeHelper;
                            Tutorials.ShowTutorial(type: type, ignoreIfShown: false, checkHintsSettings: false, ignoreDelay: true, menuMode: true);

                            return;
                        }

                    case TaskName.ReturnToMainMenu:
                        CloseGame(quitGame: false);
                        return;

                    case TaskName.QuitGame:
                        CloseGame(quitGame: true);
                        return;

                    case TaskName.RemoveScene:
                        {
                            var removeData = (Dictionary<string, Object>)executeHelper;
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

                            var inputData = (Dictionary<string, Object>)executeHelper;

                            Scene scene = (Scene)inputData["scene"];
                            Scene.InputTypes inputType = (Scene.InputTypes)inputData["inputType"];

                            scene.InputType = inputType;
                            return;
                        }

                    case TaskName.SetCineMode:
                        {
                            world = World.GetTopWorld();
                            if (world == null || world.demoMode) return;

                            world.CineMode = (bool)executeHelper;
                            return;
                        }

                    case TaskName.AddTransition:
                        {
                            // example executeHelper for this task
                            // var transData = new Dictionary<string, Object> { { "scene", scene }, { "transition",  transition }};

                            var transData = (Dictionary<string, Object>)executeHelper;

                            Scene scene = (Scene)transData["scene"];
                            Transition transition = (Transition)transData["transition"];

                            scene.transManager.AddTransition(transition);
                            return;
                        }

                    case TaskName.SkipCinematics:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Skipping cinematics", color: Color.White);

                            var textWindows = Scene.GetAllScenesOfType(typeof(TextWindow));
                            foreach (var scene in textWindows)
                            {
                                TextWindow textWindow = (TextWindow)scene;
                                textWindow.RemoveWithoutExecutingTask(); // every cine task will be tied to text window
                            }
                            world.CineMode = false;

                            return;
                        }

                    case TaskName.DeleteTemplates:
                        {
                            Dictionary<string, Object> deleteData;
                            List<string> pathsToDelete;
                            int pathCount;

                            bool firstRun = executeHelper == null;

                            if (firstRun)
                            {
                                var templatePaths = Directory.GetDirectories(SonOfRobinGame.worldTemplatesPath);
                                var correctSaves = SaveHeaderManager.CorrectSaves;
                                var pathsToKeep = new List<string>();

                                foreach (string templatePath in templatePaths)
                                {
                                    GridTemplate gridTemplate = GridTemplate.LoadHeader(templatePath);
                                    if (gridTemplate == null || gridTemplate.IsObsolete) continue;

                                    if (correctSaves.Where(saveHeader =>
                                    saveHeader.seed == gridTemplate.seed &&
                                    saveHeader.width == gridTemplate.width &&
                                    saveHeader.height == gridTemplate.height
                                    ).ToList().Count > 0) pathsToKeep.Add(templatePath);
                                }

                                pathsToDelete = templatePaths.Where(path => !pathsToKeep.Contains(path)).ToList();

                                if (pathsToDelete.Count == 0)
                                {
                                    new TextWindow(text: "No obsolete templates were found.", textColor: Color.White, bgColor: Color.Blue, useTransition: true, animate: true);
                                    return;
                                }

                                executeHelper = new Dictionary<string, Object> { { "pathsToDelete", pathsToDelete }, { "pathCount", pathsToDelete.Count } };
                            }

                            SonOfRobinGame.game.IsFixedTimeStep = false;

                            deleteData = (Dictionary<string, Object>)executeHelper;
                            pathsToDelete = (List<string>)deleteData["pathsToDelete"];
                            pathCount = (int)deleteData["pathCount"];

                            string currentPath = pathsToDelete[0];

                            SonOfRobinGame.progressBar.TurnOn(curVal: pathCount - pathsToDelete.Count + 1, maxVal: pathCount, text: $"Deleting templates...\n{Path.GetFileName(currentPath)}", addTransition: false, turnOffInput: true);

                            if (!firstRun)
                            {
                                // first run should only turn on progress bar                              
                                pathsToDelete.RemoveAt(0);
                                Directory.Delete(path: currentPath, recursive: true);
                            }

                            if (pathsToDelete.Count == 0)
                            {
                                if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
                                SonOfRobinGame.progressBar.TurnOff();
                                return;
                            }

                            var newDeleteData = new Dictionary<string, Object> { { "pathsToDelete", pathsToDelete }, { "pathCount", pathCount } };
                            new Task(menu: null, taskName: TaskName.DeleteTemplates, turnOffInputUntilExecution: true, delay: 1, executeHelper: newDeleteData);

                            return;
                        }

                    case TaskName.SetSpectatorMode:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            world.SpectatorMode = (bool)executeHelper;

                            return;
                        }

                    default:
                        throw new DivideByZeroException($"Unsupported taskName - {taskName}.");
                }
            }

            private static void OpenMenu(MenuTemplate.Name templateName, Object executeHelper)
            {
                Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: templateName);
                if (executeHelper != null)
                {
                    var scenesToLink = (List<Scene>)executeHelper;
                    menu.AddLinkedScenes(scenesToLink);
                }
            }

            private static void CloseGame(bool quitGame)
            {
                World world = World.GetTopWorld();

                bool autoSave = world != null && !world.demoMode && world.player.alive;

                var worldScenes = Scene.GetAllScenesOfType(typeof(World));
                foreach (World currWorld in worldScenes)
                { if (!currWorld.demoMode) world.Remove(); }
                Scene.RemoveAllScenesOfType(typeof(TextWindow));
                Scene.RemoveAllScenesOfType(typeof(Menu));
                Scene.RemoveAllScenesOfType(typeof(Inventory));
                Scene.RemoveAllScenesOfType(typeof(PieceContextMenu));

                if (autoSave)
                {
                    var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", "0" }, { "showMessage", false }, { "quitGameAfterSaving", quitGame } };
                    new Task(menu: null, taskName: TaskName.SaveGame, executeHelper: saveParams, delay: 17);
                }

                if (!autoSave && quitGame) SonOfRobinGame.quitGame = true;
                if (!quitGame) new Task(menu: null, taskName: TaskName.OpenMainMenu, turnOffInputUntilExecution: true, delay: autoSave ? 30 : 0, executeHelper: null);
            }
        }

    }
}
