using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Scheduler
    {
        public enum ActionName { Empty, CreateNewWorld, CreateNewWorldNow, QuitGame, OpenMainMenu, OpenCreateMenu, OpenOptionsMenu, OpenLoadMenu, OpenSaveMenu, OpenDebugMenu, OpenConfirmationMenu, SaveGame, SaveGameNow, LoadGame, LoadGameNow, ReturnToMainMenu, SavePrefs, ProcessConfirmation, OpenCraftMenu, Craft, Hit, CreateNewPiece, OpenContainer, DeleteObsoleteSaves, DropFruit }

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

            public Task(Menu menu, ActionName actionName, Object executeHelper, bool turnOffInput = false, uint delay = 0, bool rebuildsMenu = false)
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

                    case ActionName.OpenCraftMenu:
                        Workshop workshop = (Workshop)executeHelper;
                        MenuTemplate.CreateMenuFromTemplate(templateName: workshop.craftMenuTemplate);
                        return;

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
                        if (world != null) world.Save(saveSlotName: (string)executeHelper);
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
                            var pieceName = (PieceTemplate.Name)executeHelper;
                            Craft.Recipe recipe = Craft.GetRecipe(pieceName);

                            recipe.TryToProducePiece(storage: World.GetTopWorld().player.pieceStorage);
                        }
                        return;

                    case ActionName.Hit:
                        {
                            var executeData = (Dictionary<string, Object>)executeHelper;
                            Player player = (Player)executeData["player"];
                            Tool activeTool = (Tool)executeData["tool"];
                            int offsetX = (int)executeData["offsetX"];
                            int offsetY = (int)executeData["offsetY"];

                            var nearbyPieces = player.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.All, mainSprite: player.sprite, distance: 35, offsetX: offsetX, offsetY: offsetY);
                            var piecesToHit = nearbyPieces.Where(piece => piece.yield != null).ToList();

                            activeTool.Hit(BoardPiece.FindClosestPiece(sprite: player.sprite, pieceList: piecesToHit, offsetX: offsetX, offsetY: offsetY));
                            if (activeTool.hitPoints <= 0) player.toolStorage.DestroyBrokenPieces();
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


                    default:
                        throw new DivideByZeroException($"Unsupported actionName - {actionName}.");
                }

            }

        }


    }
}
