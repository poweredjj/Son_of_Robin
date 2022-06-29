using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class MenuTemplate
    {
        public enum Name { Main, Options, CreateNewIsland, Pause, Load, Save, Tutorials, GameOver, Debug, CreateAnyPiece, GenericConfirm, CraftBasic, CraftNormal, CraftCooking, CraftFurnace }

        public static Menu CreateConfirmationMenu(Object confirmationData)
        {
            var confirmationDict = (Dictionary<string, Object>)confirmationData;

            string question = (string)confirmationDict["question"];

            var menu = new Menu(templateName: Name.GenericConfirm, name: question, blocksUpdatesBelow: false, canBeClosedManually: true);
            new Separator(menu: menu, name: "", isEmpty: true);
            new Invoker(menu: menu, name: "no", closesMenu: true, taskName: Scheduler.TaskName.Empty);
            new Invoker(menu: menu, name: "yes", closesMenu: true, taskName: Scheduler.TaskName.ProcessConfirmation, executeHelper: confirmationData);

            return menu;
        }

        public static Menu CreateMenuFromTemplate(Name templateName)
        {
            Menu menu;
            World world;

            switch (templateName)
            {
                case Name.Main:
                    menu = new Menu(templateName: templateName, name: "Son of Robin", blocksUpdatesBelow: false, canBeClosedManually: false);
                    new Separator(menu: menu, name: "", isEmpty: true);

                    if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenLoadMenu);
                    new Invoker(menu: menu, name: "create new island", taskName: Scheduler.TaskName.OpenCreateMenu,
                           infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "start new game", color: Color.White, scale: 1f) });
                    new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenOptionsMenu,
                        infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "options, settings, etc.", color: Color.White, scale: 1f) });

                    string text = $"Son of Robin {SonOfRobinGame.version.ToString().Replace(",", ".")}\nLast updated: {SonOfRobinGame.lastChanged:yyyy-MM-dd.}\n\nThis is a very early alpha version of the game.\nCode by Marcin Smidowicz.\nFastNoiseLite by Jordan Peck.\nStudies.Joystick by Luiz Ossinho.\nPNG library 'Big Gustave' by Eliot Jones.\nVarious free assets used.";

                    var textWindowData = new Dictionary<string, Object> { { "text", text }, };
                    new Invoker(menu: menu, name: "about", taskName: Scheduler.TaskName.OpenTextWindow, executeHelper: textWindowData,
                        infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "information about the game", color: Color.White, scale: 1f) });

                    if (SonOfRobinGame.platform != Platform.Mobile) new Invoker(menu: menu, name: "quit game", closesMenu: true, taskName: Scheduler.TaskName.QuitGame);

                    return menu;

                case Name.Options:
                    menu = new Menu(templateName: templateName, name: "OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);
                    new Selector(menu: menu, name: "debug mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "DebugMode", rebuildsMenu: true);
                    if (Preferences.DebugMode) new Invoker(menu: menu, name: "debug menu", taskName: Scheduler.TaskName.OpenDebugMenu);

                    if (SonOfRobinGame.platform != Platform.Mobile)
                    {
                        new Selector(menu: menu, name: "fullscreen mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "FullScreenMode", rebuildsMenu: true);
                        if (Preferences.FullScreenMode) new Selector(menu: menu, name: "resolution", valueList: Preferences.AvailableScreenModes, targetObj: new Preferences(), propertyName: "FullScreenResolution");
                    }

                    new Selector(menu: menu, name: "show gamepad tips", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showControlTips", rebuildsMenu: true);
                    new Selector(menu: menu, name: "gamepad type", valueDict: new Dictionary<object, object> { { ButtonScheme.Type.M, "M" }, { ButtonScheme.Type.S, "S" }, { ButtonScheme.Type.N, "N" } }, targetObj: new Preferences(), propertyName: "ControlTipsScheme");

                    new Selector(menu: menu, name: "show hints", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showHints");
                    new Selector(menu: menu, name: "frameskip", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "FrameSkip");
                    new Selector(menu: menu, name: "load whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "loadWholeMap", rebuildsMenu: true);
                    if (SonOfRobinGame.platform == Platform.Mobile && !Preferences.loadWholeMap) new Selector(menu: menu, name: "max buffered map blocks", valueList: new List<Object> { 100, 500, 1000, 2000, 4000 }, targetObj: new Preferences(), propertyName: "mobileMaxLoadedTextures");
                    new Selector(menu: menu, name: "global scale", valueList: new List<Object> { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }, targetObj: new Preferences(), propertyName: "globalScale");
                    new Selector(menu: menu, name: "menu scale", valueList: new List<Object> { 0.5f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f }, targetObj: new Preferences(), propertyName: "menuScale");
                    new Selector(menu: menu, name: "world scale", valueList: new List<Object> { 0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f }, targetObj: new Preferences(), propertyName: "worldScale");
                    new Selector(menu: menu, name: "show demo world", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showDemoWorld");
                    new Selector(menu: menu, name: "autosave delay (minutes)", valueList: new List<Object> { 5, 10, 15, 30, 60 }, targetObj: new Preferences(), propertyName: "autoSaveDelayMins");
                    new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                    return menu;

                case Name.CreateNewIsland:
                    menu = new Menu(templateName: templateName, name: "CREATE NEW ISLAND", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);
                    new Invoker(menu: menu, name: "start game", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld);

                    List<Object> sizeList;
                    if (SonOfRobinGame.platform == Platform.Desktop)
                    { sizeList = new List<Object> { 1000, 2000, 4000, 8000, 15000, 20000, 25000, 30000, 40000, 50000, 60000 }; }
                    else
                    { sizeList = new List<Object> { 1000, 2000, 4000, 8000, 15000, 20000, 25000, 30000 }; }

                    new Selector(menu: menu, name: "width", valueList: sizeList, targetObj: new Preferences(), propertyName: "newWorldWidth");
                    new Selector(menu: menu, name: "height", valueList: sizeList, targetObj: new Preferences(), propertyName: "newWorldHeight");
                    new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                    return menu;

                case Name.Pause:
                    world = World.GetTopWorld();

                    menu = new Menu(templateName: templateName, name: "PAUSE", blocksUpdatesBelow: true, canBeClosedManually: true);
                    new Invoker(menu: menu, name: "return to game", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                    if (world?.player?.activeState == BoardPiece.State.PlayerControlledWalking) new Invoker(menu: menu, name: "save game", taskName: Scheduler.TaskName.OpenSaveMenu);
                    if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenLoadMenu);
                    new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenOptionsMenu);
                    if (world.hintEngine.shownTutorials.Count > 0) new Invoker(menu: menu, name: "tutorials", taskName: Scheduler.TaskName.OpenTutorialsMenu);
                    new Invoker(menu: menu, name: "return to main menu", closesMenu: true, taskName: Scheduler.TaskName.ReturnToMainMenu);
                    if (SonOfRobinGame.platform != Platform.Mobile)
                    {
                        var confirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit?" }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                        new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                    }
                    return menu;

                case Name.GameOver:
                    menu = new Menu(templateName: templateName, name: "GAME OVER", blocksUpdatesBelow: true, canBeClosedManually: false, layout: Menu.Layout.Middle);
                    if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenLoadMenu);
                    new Invoker(menu: menu, name: "return to main menu", closesMenu: true, taskName: Scheduler.TaskName.ReturnToMainMenu);
                    if (SonOfRobinGame.platform != Platform.Mobile)
                    {
                        var confirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit?" }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                        new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                    }
                    return menu;

                case Name.Load:
                    menu = new Menu(templateName: templateName, name: "LOAD GAME", blocksUpdatesBelow: false, canBeClosedManually: true);
                    foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                    {
                        new Invoker(menu: menu, name: saveInfo.FullDescription, closesMenu: true, taskName: Scheduler.TaskName.LoadGame, executeHelper: saveInfo.folderName,
                             infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: saveInfo.AdditionalInfo, color: Color.White, scale: 1f) });
                    }
                    new Separator(menu: menu, name: "", isEmpty: true);
                    new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                    return menu;

                case Name.Save:
                    menu = new Menu(templateName: templateName, name: "SAVE GAME", blocksUpdatesBelow: false, canBeClosedManually: true);

                    world = World.GetTopWorld();

                    var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", SaveHeaderManager.NewSaveSlotName }, { "showMessage", true } };
                    new Invoker(menu: menu, name: "new save", taskName: Scheduler.TaskName.SaveGame, executeHelper: saveParams, rebuildsMenu: true,
                        infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "make new save", color: Color.White, scale: 1f) });
                    new Separator(menu: menu, name: "", isEmpty: true);

                    foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                    {
                        if (!saveInfo.autoSave)
                        {
                            saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", saveInfo.folderName }, { "showMessage", true } };
                            var confirmationData = new Dictionary<string, Object> { { "question", "The save will be overwritten. Continue?" }, { "taskName", Scheduler.TaskName.SaveGame }, { "executeHelper", saveParams } };
                            new Invoker(menu: menu, name: saveInfo.FullDescription, taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData, closesMenu: true,
                             infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: saveInfo.AdditionalInfo, color: Color.White, scale: 1f) });
                        }
                    }

                    new Separator(menu: menu, name: "", isEmpty: true);
                    new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                    return menu;

                case Name.CreateAnyPiece:
                    menu = new Menu(templateName: templateName, name: "CREATE ANY ITEM", blocksUpdatesBelow: false, canBeClosedManually: true);

                    Dictionary<string, Object> createData;
                    world = World.GetTopWorld();
                    if (world == null) return menu;

                    foreach (PieceTemplate.Name pieceName in (PieceTemplate.Name[])Enum.GetValues(typeof(PieceTemplate.Name)))
                    {
                        createData = new Dictionary<string, Object> { { "position", world.player.sprite.position }, { "templateName", pieceName } };

                        new Invoker(menu: menu, name: $"{pieceName}", taskName: Scheduler.TaskName.CreateDebugPieces, createData);
                    }

                    return menu;

                case Name.Debug:
                    menu = new Menu(templateName: templateName, name: "DEBUG MENU", blocksUpdatesBelow: false, canBeClosedManually: true);
                    new Invoker(menu: menu, name: "delete obsolete saves", taskName: Scheduler.TaskName.DeleteObsoleteSaves);

                    world = World.GetTopWorld();

                    if (world != null && !world.demoMode) new Invoker(menu: menu, name: "create any piece", taskName: Scheduler.TaskName.OpenCreateAnyPieceMenu);
                    new Selector(menu: menu, name: "create missing pieces", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugCreateMissingPieces");
                    new Selector(menu: menu, name: "show whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowWholeMap");
                    new Selector(menu: menu, name: "show all items on map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowAllMapPieces");
                    new Invoker(menu: menu, name: "restore all hints", taskName: Scheduler.TaskName.RestoreHints);
                    new Selector(menu: menu, name: "god mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugGodMode");
                    new Selector(menu: menu, name: "show fruit rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowFruitRects");
                    new Selector(menu: menu, name: "show sprite rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowRects");
                    new Selector(menu: menu, name: "show cells", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowCellData");
                    new Selector(menu: menu, name: "show all stat bars", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowStatBars");
                    new Selector(menu: menu, name: "show states", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowStates");
                    new Selector(menu: menu, name: "use multiple threads", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugUseMultipleThreads");

                    new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                    return menu;

                case Name.CraftBasic:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Field, label: "FIELD CRAFT");

                case Name.CraftNormal:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Workshop, label: "WORKSHOP");

                case Name.CraftFurnace:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Furnace, label: "FURNACE");

                case Name.Tutorials:
                    world = World.GetTopWorld();
                    menu = new Menu(templateName: templateName, name: "TUTORIALS", blocksUpdatesBelow: true, canBeClosedManually: true);

                    foreach (Tutorials.Tutorial tutorial in Tutorials.TutorialsInMenu)
                    {
                        if (world.hintEngine.shownTutorials.Contains(tutorial.type))
                        {
                            new Invoker(menu: menu, name: tutorial.name, taskName: Scheduler.TaskName.ShowTutorial, executeHelper: tutorial.type);
                        }
                    }
                    new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                    return menu;


                default:
                    throw new DivideByZeroException($"Unsupported menu templateName - {templateName}.");
            }
        }

        private static Menu CreateCraftMenu(Name templateName, Craft.Category category, string label)
        {
            Tutorials.ShowTutorial(type: Tutorials.Type.Craft, ignoreIfShown: true);

            PieceStorage storage = World.GetTopWorld().player.pieceStorage;

            Menu menu = new Menu(templateName: templateName, name: label, blocksUpdatesBelow: false, canBeClosedManually: true, layout: SonOfRobinGame.platform == Platform.Mobile ? Menu.Layout.Right : Menu.Layout.Left, alwaysShowSelectedEntry: true);
            menu.bgColor = Color.LemonChiffon * 0.5f;

            var recipeList = Craft.recipesByCategory[category];
            foreach (Craft.Recipe recipe in recipeList)
            {
                new CraftInvoker(menu: menu, name: recipe.pieceToCreate.ToString(), closesMenu: false, taskName: Scheduler.TaskName.Craft, rebuildsMenu: true, executeHelper: recipe, recipe: recipe, storage: storage);
            }

            menu.EntriesRectColor = Color.SaddleBrown;
            menu.EntriesTextColor = Color.PaleGoldenrod;
            menu.EntriesOutlineColor = Color.SaddleBrown;

            return menu;
        }

    }
}
