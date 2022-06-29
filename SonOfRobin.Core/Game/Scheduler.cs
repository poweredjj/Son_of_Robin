using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Scheduler
    {
        public enum TaskName { Empty, CreateNewWorld, CreateNewWorldNow, QuitGame, OpenMainMenu, OpenCreateMenu, OpenOptionsMenu, OpenTutorialsMenu, OpenLoadMenu, OpenSaveMenu, OpenDebugMenu, OpenConfirmationMenu, OpenCreateAnyPieceMenu, OpenGameOverMenu, SaveGame, LoadGame, LoadGameNow, ReturnToMainMenu, SavePrefs, ProcessConfirmation, OpenCraftMenu, Craft, Hit, CreateNewPiece, CreateDebugPieces, OpenContainer, DeleteObsoleteSaves, DropFruit, GetEaten, ExecuteTaskWithDelay, AddWorldEvent, OpenTextWindow, SleepInsideShelter, SleepOutside, TempoFastForward, TempoStop, TempoPlay, CameraTrackPiece, CameraTrackPlayer, CameraZoom, ShowCookingProgress, RestoreHints, OpenMainMenuIfSpecialKeysArePressed, CheckForPieceHints, ShowHint, ExecuteTaskList, ExecuteTaskChain, ShowTutorial, RemoveScene }

        private readonly static Dictionary<int, List<Task>> queue = new Dictionary<int, List<Task>>();

        public static void ProcessQueue()
        {
            var framesToProcess = queue.Keys.Where(frameNo => SonOfRobinGame.currentUpdate >= frameNo).ToList();
            if (framesToProcess.Count == 0) return;

            foreach (int frameNo in framesToProcess)
            {
                foreach (Task task in queue[frameNo])
                { task.Execute(); }

                queue.Remove(frameNo);
            }
        }

        public struct Task
        {
            private readonly Menu menu;
            private readonly TaskName taskName;
            private Object executeHelper;
            private readonly int delay;
            private int frame;
            private readonly bool turnOffInput;
            private bool rebuildsMenu;

            public Task(Menu menu, TaskName taskName, Object executeHelper, bool turnOffInput = false, int delay = 0, bool rebuildsMenu = false, bool storeForLaterUse = false)
            {
                this.taskName = taskName;
                this.executeHelper = executeHelper;
                this.menu = menu;
                this.turnOffInput = turnOffInput;
                this.delay = delay;
                this.rebuildsMenu = rebuildsMenu;

                if (!storeForLaterUse)
                {
                    if (this.turnOffInput) Input.GlobalInputActive = false;

                    this.frame = SonOfRobinGame.currentUpdate + this.delay;

                    if (this.delay == 0) this.Execute();
                    else this.AddToQueue();
                }
                else
                { this.frame = -1; }
            }

            private void AddToQueue()
            {
                if (this.turnOffInput) Input.GlobalInputActive = false;
                if (this.frame == -1) this.frame = SonOfRobinGame.currentUpdate + this.delay;
                if (!queue.ContainsKey(this.frame)) queue[this.frame] = new List<Task>();
                queue[this.frame].Add(this);
            }

            public void Execute()
            {
                this.RunTask();
                if (this.turnOffInput && this.taskName != TaskName.ExecuteTaskChain) Input.GlobalInputActive = true;
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
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                        return;

                    case TaskName.OpenCreateMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CreateNewIsland);
                        return;

                    case TaskName.OpenLoadMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Load);
                        return;

                    case TaskName.OpenSaveMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Save);
                        return;

                    case TaskName.OpenOptionsMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Options);
                        return;

                    case TaskName.OpenTutorialsMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Tutorials);
                        return;

                    case TaskName.OpenDebugMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Debug);
                        return;

                    case TaskName.OpenCreateAnyPieceMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CreateAnyPiece);
                        return;

                    case TaskName.OpenGameOverMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.GameOver);
                        return;

                    case TaskName.OpenCraftMenu:
                        {
                            Workshop workshop = (Workshop)executeHelper;
                            workshop.TurnOn();

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);

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

                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.CreateNewIsland);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Load);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Main);

                        new Task(menu: null, taskName: TaskName.CreateNewWorldNow, turnOffInput: true, delay: 13, executeHelper: null);

                        return;

                    case TaskName.CreateNewWorldNow:
                        new World(width: Preferences.newWorldWidth, height: Preferences.newWorldHeight, seed: Preferences.newWorldSeed);

                        return;

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
                            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Could not create selected item, because no world was found.");
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
                                    MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Max number of attempts exceeded while trying to create '{templateName}'.");
                                    break;
                                }
                            }

                            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"{piecesCreated} '{templateName}' pieces created.");
                        }
                        return;

                    case TaskName.SaveGame:
                        // example executeHelper for this task
                        // var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", "1" }, { "showMessage", false }, {"quitGameAfterSaving", false} };

                        if (this.rebuildsMenu)
                        {// menu should be rebuilt after the game has been saved
                            new Task(menu: this.menu, taskName: TaskName.Empty, turnOffInput: false, delay: 12, executeHelper: null, rebuildsMenu: true);
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

                        new Task(menu: null, taskName: TaskName.LoadGameNow, turnOffInput: true, delay: 17, executeHelper: this.executeHelper);

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
                            if (world != null) Scene.SetInventoryLayout(newLayout: Scene.InventoryLayout.InventoryAndChest, player: world.player, chest: container);
                        }

                        return;

                    case TaskName.Craft:
                        {
                            var recipe = (Craft.Recipe)executeHelper;

                            recipe.TryToProducePieces(storage: World.GetTopWorld().player.pieceStorage);
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

                            bool shootsProjectile = activeTool.shootsProjectile;
                            if (shootsProjectile) activeTool.Use(shootingPower: shootingPower, targetPiece: null);
                            else
                            {
                                var nearbyPieces = player.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY);
                                var piecesToHit = nearbyPieces.Where(piece => piece.yield != null).ToList();
                                BoardPiece targetPiece = BoardPiece.FindClosestPiece(sprite: player.sprite, pieceList: piecesToHit, offsetX: offsetX, offsetY: offsetY);
                                if (targetPiece == null) return;

                                if (highlightOnly)
                                {
                                    if (player.world.inputActive) Tutorials.ShowTutorial(type: Tutorials.Type.Hit, ignoreIfShown: true);
                                    Sprite sprite = targetPiece.sprite;
                                    sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Red, textureSize: sprite.frame.originalTextureSize, priority: 0));
                                }
                                else
                                {
                                    activeTool.Use(shootingPower: shootingPower, targetPiece: targetPiece);
                                }
                            }
                            return;
                        }

                    case TaskName.GetEaten:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            BoardPiece food = (BoardPiece)executeData["toolbarPiece"];
                            bool highlightOnly = false;
                            if (executeData.ContainsKey("highlightOnly")) highlightOnly = (bool)executeData["highlightOnly"];
                            if (highlightOnly) return;

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (player.hitPoints == player.maxHitPoints && player.fedLevel >= player.maxFedLevel * 0.95)
                            {
                                new TextWindow(text: "I am full.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

                                return;
                            }

                            foreach (BuffEngine.Buff buff in food.buffList)
                            { player.buffEngine.AddBuff(buff: buff, world: World.GetTopWorld()); }

                            player.AcquireEnergy(food.Mass * 40f);

                            food.hitPoints = 0;
                        }
                        return;

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

                            new TextWindow(text: text, useTransition: useTransition, useTransitionOpen: useTransitionOpen, useTransitionClose: useTransitionClose, bgColor: bgColor, textColor: textColor, framesPerChar: framesPerChar, animate: animate, checkForDuplicate: checkForDuplicate, closingTask: closingTask, closingTaskHelper: closingTaskHelper, blockInputDuration: blockInputDuration);
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
                                    player.Kill();

                                    var bgColor = new List<byte> { Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B };
                                    var textWindowData = new Dictionary<string, Object> { { "text", "You have drowned." }, { "bgColor", bgColor } };

                                    new TextWindow(text: "You have drowned.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: true, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 220);

                                    return;
                                }

                                sleepEngine = SleepEngine.OutdoorSleepWet;
                            }
                            else { sleepEngine = SleepEngine.OutdoorSleepDry; }

                            player.GoToSleep(sleepEngine: sleepEngine, zzzPos: player.sprite.position);
                            return;
                        }

                    case TaskName.SleepInsideShelter:
                        {
                            Shelter shelterPiece = (Shelter)executeHelper;
                            SleepEngine sleepEngine = shelterPiece.sleepEngine;
                            World.GetTopWorld()?.player.GoToSleep(sleepEngine: sleepEngine, new Vector2(shelterPiece.sprite.gfxRect.Center.X, shelterPiece.sprite.gfxRect.Center.Y));

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
                            if (world == null) return;
                            if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;
                            world.updateMultiplier = 0;

                            return;
                        }

                    case TaskName.TempoPlay:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

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

                    case TaskName.CameraTrackPlayer:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            world.camera.TrackPiece(world.player);

                            return;
                        }

                    case TaskName.CameraZoom:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            float zoom = (float)executeHelper;
                            world.camera.SetZoom(zoom);

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
                                task.AddToQueue();
                            }

                            return;
                        }

                    case TaskName.ExecuteTaskChain:
                        {
                            List<Object> taskChain = (List<Object>)executeHelper;

                            Task task = (Task)taskChain[0];
                            taskChain.RemoveAt(0);
                            task.AddToQueue();
                            if (taskChain.Count == 0)
                            {
                                Input.GlobalInputActive = true; // to ensure that input will be active at the end
                                return;
                            }

                            if (task.taskName == TaskName.OpenTextWindow)
                            {
                                // If text window will be opened, the delay will depend on the player, so it is unknown.
                                // So, the next task should be run after closing this text window.

                                var executeHelper = task.executeHelper;
                                var textWindowData = (Dictionary<string, Object>)executeHelper;
                                textWindowData["closingTask"] = TaskName.ExecuteTaskChain;
                                textWindowData["closingTaskHelper"] = taskChain;

                                task.executeHelper = textWindowData;
                            }
                            else
                            {
                                // in other cases, the delay should be known
                                new Task(menu: null, taskName: TaskName.ExecuteTaskChain, executeHelper: taskChain, delay: task.delay, turnOffInput: true);
                            }
                        }

                        return;


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

                            world.hintEngine.CheckForPieceHintToShow(forcedMode: true, ignoreInputActive: true);

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
                            Tutorials.Type type = (Tutorials.Type)executeHelper;
                            Tutorials.ShowTutorial(type: type, ignoreIfShown: false, checkHintsSettings: false);

                            return;
                        }

                    case TaskName.ReturnToMainMenu:
                        CloseGame(quitGame: false);
                        return;

                    case TaskName.QuitGame:
                        CloseGame(quitGame: true);
                        return;

                    case TaskName.RemoveScene:
                        var removeData = (Dictionary<string, Object>)executeHelper;
                        Scene scene = (Scene)removeData["scene"];
                        bool fadeOut = (bool)removeData["fadeOut"];
                        int fadeOutDuration = (int)removeData["fadeOutDuration"];
                        if (fadeOut) scene.AddTransition(new Transition(type: Transition.TransType.To, duration: fadeOutDuration, scene: scene, blockInput: false, removeScene: true,
                            paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }));
                        else scene.Remove();


                        return;


                    default:
                        throw new DivideByZeroException($"Unsupported taskName - {taskName}.");
                }

            }

            private static void CloseGame(bool quitGame)
            {
                World world = World.GetTopWorld();

                bool autoSave = world != null && !world.demoMode && world.player.alive;

                var worldScenes = Scene.GetAllScenesOfType(typeof(World));
                foreach (World currWorld in worldScenes)
                { if (!currWorld.demoMode) world.Remove(); }
                Scene.RemoveAllScenesOfType(typeof(Menu));
                Scene.RemoveAllScenesOfType(typeof(Inventory));
                Scene.RemoveAllScenesOfType(typeof(PieceContextMenu));

                if (autoSave)
                {
                    var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", "0" }, { "showMessage", false }, { "quitGameAfterSaving", quitGame } };
                    new Task(menu: null, taskName: TaskName.SaveGame, executeHelper: saveParams, delay: 17);
                }

                if (!autoSave && quitGame) SonOfRobinGame.quitGame = true;
                if (!quitGame) new Task(menu: null, taskName: TaskName.OpenMainMenu, turnOffInput: true, delay: autoSave ? 30 : 0, executeHelper: null);
            }
        }

    }
}
