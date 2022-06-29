using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Scheduler
    {
        public enum ActionName { Empty, CreateNewWorld, CreateNewWorldNow, QuitGame, OpenMainMenu, OpenCreateMenu, OpenOptionsMenu, OpenLoadMenu, OpenSaveMenu, OpenDebugMenu, OpenConfirmationMenu, OpenCreateAnyPieceMenu, OpenGameOverMenu, SaveGame, SaveGameNow, LoadGame, LoadGameNow, ReturnToMainMenu, SavePrefs, ProcessConfirmation, OpenCraftMenu, Craft, Hit, CreateNewPiece, CreateDebugPieces, OpenContainer, DeleteObsoleteSaves, DropFruit, GetEaten, ExecuteActionWithDelay, AddWorldEvent, OpenTextWindow, SleepInsideShelter, SleepOutside, EnableFastForward }

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
            private readonly ActionName actionName;
            private readonly Object executeHelper;
            private readonly int frame;
            private readonly bool turnedOffInput;
            private bool rebuildsMenu;

            public Task(Menu menu, ActionName actionName, Object executeHelper, bool turnOffInput = false, int delay = 0, bool rebuildsMenu = false)
            {
                this.actionName = actionName;
                this.executeHelper = executeHelper;
                this.menu = menu;
                this.turnedOffInput = turnOffInput;
                this.frame = SonOfRobinGame.currentUpdate + (int)delay;
                this.rebuildsMenu = rebuildsMenu;

                if (turnOffInput) Input.GlobalInputActive = false;

                if (delay == 0)
                { this.Execute(); }
                else
                { this.AddToQueue(); }
            }

            private void AddToQueue()
            {
                if (!queue.ContainsKey(this.frame)) queue[this.frame] = new List<Task>();
                queue[this.frame].Add(this);
            }

            public void Execute()
            {
                this.RunAction();
                if (this.turnedOffInput) Input.GlobalInputActive = true;
                if (this.rebuildsMenu) this.menu.Rebuild();
            }

            private void RunAction()
            {
                World world;

                switch (actionName)
                {
                    case ActionName.Empty:
                        return;

                    case ActionName.OpenMainMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                        return;

                    case ActionName.OpenCreateMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CreateNewIsland);
                        return;

                    case ActionName.OpenLoadMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Load);
                        return;

                    case ActionName.OpenSaveMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Save);
                        return;

                    case ActionName.OpenOptionsMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Options);
                        return;

                    case ActionName.OpenDebugMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Debug);
                        return;

                    case ActionName.OpenCreateAnyPieceMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.CreateAnyPiece);
                        return;

                    case ActionName.OpenGameOverMenu:
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.GameOver);
                        return;

                    case ActionName.OpenCraftMenu:
                        {
                            Workshop workshop = (Workshop)executeHelper;
                            workshop.TurnOn();

                            Menu menu = MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);

                            var worldEventData = new Dictionary<string, object> {
                                {"boardPiece", workshop },
                                {"delay", 300 },
                                {"eventName", WorldEvent.EventName.TurnOffWorkshop },
                            };

                            menu.AddClosingAction(closingAction: ActionName.AddWorldEvent, closingActionHelper: worldEventData);
                            return;
                        }

                    case ActionName.OpenConfirmationMenu:
                        MenuTemplate.CreateConfirmationMenu(confirmationData: executeHelper);
                        return;

                    case ActionName.CreateNewWorld:
                        ProgressBar.TurnOnBgColor();
                        ProgressBar.ChangeValues(curVal: 0, maxVal: 5, text: "Creating new island...");

                        if (menu != null) menu.MoveToTop();

                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.CreateNewIsland);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Load);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Main);

                        new Task(menu: null, actionName: ActionName.CreateNewWorldNow, turnOffInput: true, delay: 13, executeHelper: null);

                        return;

                    case ActionName.CreateNewWorldNow:
                        new World(width: Preferences.newWorldWidth, height: Preferences.newWorldHeight, seed: Preferences.newWorldSeed);

                        return;

                    case ActionName.QuitGame:
                        world = World.GetTopWorld();
                        if (world != null && !world.demoMode) world.AutoSave(force: true);

                        SonOfRobinGame.quitGame = true;
                        return;

                    case ActionName.CreateNewPiece:
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

                    case ActionName.CreateDebugPieces:
                        world = World.GetTopWorld();
                        if (world != null)
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
                                if (piece.sprite.placedCorrectly)
                                {
                                    bool piecePickedUp = player.PickUpPiece(piece);
                                    if (!piecePickedUp) piece.sprite.MoveToClosestFreeSpot(position);
                                    amountToCreate = piece.stackSize;
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
                        }
                        return;

                    case ActionName.SaveGame:
                        if (this.rebuildsMenu)
                        {// menu should be rebuilt after the game has been saved
                            new Task(menu: this.menu, actionName: ActionName.Empty, turnOffInput: true, delay: 2, executeHelper: this.executeHelper, rebuildsMenu: true);
                            this.rebuildsMenu = false;
                        }

                        ProgressBar.ChangeValues(curVal: 1, maxVal: 2, text: "Saving game...");
                        new Task(menu: null, actionName: ActionName.SaveGameNow, turnOffInput: true, delay: 1, executeHelper: this.executeHelper);

                        return;

                    case ActionName.SaveGameNow:
                        world = (World)Scene.GetTopSceneOfType(typeof(World));
                        if (world != null)
                        {
                            // example executeHelper for this action
                            // var saveParams = new Dictionary<string, Object> { { "saveSlotName", "1" }, { "showMessage", false } };

                            var saveParams = (Dictionary<string, Object>)executeHelper;
                            string saveSlotName = (string)saveParams["saveSlotName"];
                            bool showMessage = false;
                            if (saveParams.ContainsKey("showMessage")) showMessage = (bool)saveParams["showMessage"];

                            world.Save(saveSlotName: saveSlotName, showMessage: showMessage);
                        }
                        ProgressBar.Hide();
                        return;

                    case ActionName.LoadGame:
                        ProgressBar.TurnOnBgColor();
                        ProgressBar.ChangeValues(curVal: 0, maxVal: 5, text: "Loading game...");

                        if (menu != null) menu.MoveToTop();

                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Load);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Main);
                        Menu.RemoveEveryMenuOfTemplate(MenuTemplate.Name.Pause);

                        new Task(menu: null, actionName: ActionName.LoadGameNow, turnOffInput: true, delay: 13, executeHelper: this.executeHelper);

                        return;

                    case ActionName.LoadGameNow:
                        World loadedWorld = World.Load(saveSlotName: (string)executeHelper);

                        if (loadedWorld == null)
                        {
                            ProgressBar.Hide();

                            world = World.GetTopWorld();
                            if (world != null && world.demoMode) new Task(menu: null, actionName: ActionName.OpenMainMenu, turnOffInput: false, delay: 1, executeHelper: this.executeHelper);
                            return;
                        }

                        var existingWorlds = Scene.GetAllScenesOfType(typeof(World));
                        foreach (World currWorld in existingWorlds)
                        { if (currWorld != loadedWorld && !currWorld.demoMode) currWorld.Remove(); }

                        return;

                    case ActionName.ReturnToMainMenu:
                        world = World.GetTopWorld();
                        if (world != null && !world.demoMode) world.AutoSave(force: true);

                        var worldScenes = Scene.GetAllScenesOfType(typeof(World));
                        foreach (World currWorld in worldScenes)
                        { if (!currWorld.demoMode) world.Remove(); }
                        Scene.RemoveAllScenesOfType(typeof(Menu));
                        Scene.RemoveAllScenesOfType(typeof(Inventory));
                        Scene.RemoveAllScenesOfType(typeof(PieceContextMenu));
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);

                        return;

                    case ActionName.SavePrefs:
                        Preferences.Save();
                        return;

                    case ActionName.ProcessConfirmation:
                        var confirmationData = (Dictionary<string, Object>)executeHelper;
                        new Task(menu: null, actionName: (ActionName)confirmationData["actionName"], executeHelper: confirmationData["executeHelper"]);

                        return;

                    case ActionName.OpenContainer:
                        {
                            BoardPiece container = (BoardPiece)executeHelper;

                            world = World.GetTopWorld();
                            if (world != null) Scene.SetInventoryLayout(newLayout: Scene.InventoryLayout.InventoryAndChest, player: world.player, chest: container);
                        }

                        return;

                    case ActionName.Craft:
                        {
                            var recipe = (Craft.Recipe)executeHelper;

                            recipe.TryToProducePieces(storage: World.GetTopWorld().player.pieceStorage);
                        }
                        return;

                    case ActionName.Hit:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            Tool activeTool = (Tool)executeData["toolbarPiece"];
                            int shootingPower = (int)executeData["shootingPower"];
                            int offsetX = (int)executeData["offsetX"];
                            int offsetY = (int)executeData["offsetY"];

                            var nearbyPieces = player.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY);
                            var piecesToHit = nearbyPieces.Where(piece => piece.yield != null).ToList();

                            activeTool.Use(shootingPower: shootingPower, targetPiece: BoardPiece.FindClosestPiece(sprite: player.sprite, pieceList: piecesToHit, offsetX: offsetX, offsetY: offsetY));
                        }

                        return;

                    case ActionName.GetEaten:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            BoardPiece food = (BoardPiece)executeData["toolbarPiece"];

                            if (executeData.ContainsKey("buttonHeld"))
                            {
                                bool buttonHeld = (bool)executeData["buttonHeld"];
                                if (buttonHeld) return;
                            }

                            if (player.hitPoints == player.maxHitPoints && player.fedLevel >= player.maxFedLevel * 0.95)
                            {
                                new TextWindow(text: "You are full.", textColor: Color.White, bgColor: Color.DarkGreen, useTransition: false, animate: false);
                                return;
                            }

                            player.AcquireEnergy(food.Mass * 40f);

                            food.hitPoints = 0;
                        }
                        return;

                    case ActionName.DropFruit:
                        {
                            Plant fruitPlant = (Plant)executeHelper;
                            fruitPlant.DropFruit();
                        }
                        return;

                    case ActionName.DeleteObsoleteSaves:
                        SaveManager.DeleteObsoleteSaves();
                        return;

                    case ActionName.ExecuteActionWithDelay:
                        {
                            // example executeHelper for this action
                            // var delayData = new Dictionary<string, Object> { { "actionName", ActionName.TurnOffWorkshop }, { "executeHelper", workshop }, { "delay", 300 } };

                            var delayData = (Dictionary<string, Object>)executeHelper;
                            new Task(menu: null, actionName: (ActionName)delayData["actionName"], executeHelper: delayData["executeHelper"], delay: (int)delayData["delay"]);

                            return;
                        }

                    case ActionName.AddWorldEvent:
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

                    case ActionName.OpenTextWindow:
                        {
                            var textWindowData = (Dictionary<string, Object>)executeHelper;

                            string text = (string)textWindowData["text"];

                            bool checkForDuplicate = false;
                            if (textWindowData.ContainsKey("checkForDuplicate")) checkForDuplicate = (bool)textWindowData["checkForDuplicate"];

                            bool useTransition = true;
                            if (textWindowData.ContainsKey("useTransition")) useTransition = (bool)textWindowData["useTransition"];

                            bool animate = true;
                            if (textWindowData.ContainsKey("animate")) useTransition = (bool)textWindowData["animate"];

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

                            new TextWindow(text: text, useTransition: useTransition, bgColor: bgColor, textColor: textColor, framesPerChar: framesPerChar, animate: animate, checkForDuplicate: checkForDuplicate);
                            return;
                        }

                    case ActionName.SleepOutside:
                        {
                            Player player = World.GetTopWorld()?.player;
                            if (player == null) return;

                            SleepEngine sleepEngine;

                            if (player.sprite.IsInWater)
                            {
                                if (player.sprite.CanDrownHere)
                                {
                                    player.hitPoints = 0;

                                    var bgColor = new List<byte> { Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B };
                                    var textWindowData = new Dictionary<string, Object> { { "text", "You have drowned." }, { "bgColor", bgColor }};
                                    new Task(menu: null, actionName: ActionName.OpenTextWindow, turnOffInput: true, delay: 1, executeHelper: textWindowData);
                                    return;
                                }

                                sleepEngine = SleepEngine.OutdoorSleepWet;
                            }
                            else { sleepEngine = SleepEngine.OutdoorSleepDry; }

                            player.GoToSleep(sleepEngine: sleepEngine, zzzPos: player.sprite.position);
                            return;
                        }

                    case ActionName.SleepInsideShelter:
                        {
                            Shelter shelterPiece = (Shelter)executeHelper;
                            SleepEngine sleepEngine = shelterPiece.sleepEngine;
                            World.GetTopWorld()?.player.GoToSleep(sleepEngine: sleepEngine, new Vector2(shelterPiece.sprite.gfxRect.Center.X, shelterPiece.sprite.gfxRect.Center.Y));

                            return;
                        }

                    case ActionName.EnableFastForward:
                        {
                            world = World.GetTopWorld();
                            if (world == null) return;

                            SonOfRobinGame.game.IsFixedTimeStep = false;
                            world.updateMultiplier = (int)executeHelper;

                            return;
                        }

                    default:
                        throw new DivideByZeroException($"Unsupported actionName - {actionName}.");
                }

            }

        }


    }
}
