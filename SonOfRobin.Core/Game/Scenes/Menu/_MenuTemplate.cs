using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class MenuTemplate
    {
        public enum Name { Main, Options, CreateNewIsland, Pause, Load, Save, Debug, GenericConfirm, CraftBasic, CraftNormal }

        public static Menu CreateConfirmationMenu(Object confirmationData)
        {
            var confirmationDict = (Dictionary<string, Object>)confirmationData;

            string question = (string)confirmationDict["question"];

            var menu = new Menu(templateName: Name.GenericConfirm, name: question, blocksUpdatesBelow: false, canBeClosedManually: true);
            new Separator(menu: menu, name: "", isEmpty: true);
            new Invoker(menu: menu, name: "no", closesMenu: true, actionName: Scheduler.ActionName.Empty);
            new Invoker(menu: menu, name: "yes", closesMenu: true, actionName: Scheduler.ActionName.ProcessConfirmation, executeHelper: confirmationData);

            return menu;
        }

        public static Menu CreateMenuFromTemplate(Name templateName)
        {
            Menu menu;

            switch (templateName)
            {
                case Name.Main:
                    menu = new Menu(templateName: templateName, name: "Son of Robin", blocksUpdatesBelow: false, canBeClosedManually: false);
                    new Separator(menu: menu, name: "", isEmpty: true);
                    if (SaveManager.AnySavesExist) new Invoker(menu: menu, name: "load game", actionName: Scheduler.ActionName.OpenLoadMenu);
                    new Invoker(menu: menu, name: "create new island", actionName: Scheduler.ActionName.OpenCreateMenu);
                    new Invoker(menu: menu, name: "options", actionName: Scheduler.ActionName.OpenOptionsMenu);
                    if (SonOfRobinGame.platform != Platform.Mobile) new Invoker(menu: menu, name: "quit game", closesMenu: true, actionName: Scheduler.ActionName.QuitGame);
                    return menu;

                case Name.Options:
                    menu = new Menu(templateName: templateName, name: "OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingSavesPrefs: true);
                    new Selector(menu: menu, name: "debug mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "DebugMode", rebuildsMenu: true);
                    if (Preferences.DebugMode) new Invoker(menu: menu, name: "debug menu", actionName: Scheduler.ActionName.OpenDebugMenu);
                    if (SonOfRobinGame.platform != Platform.Mobile) new Selector(menu: menu, name: "fullscreen mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "FullScreenMode");
                    new Selector(menu: menu, name: "frameskip", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "FrameSkip");
                    new Selector(menu: menu, name: "global scale", valueList: new List<Object> { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }, targetObj: new Preferences(), propertyName: "globalScale");
                    new Selector(menu: menu, name: "menu scale", valueList: new List<Object> { 0.5f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f }, targetObj: new Preferences(), propertyName: "menuScale");
                    new Selector(menu: menu, name: "world scale", valueList: new List<Object> { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f }, targetObj: new Preferences(), propertyName: "worldScale");
                    new Selector(menu: menu, name: "show demo world", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showDemoWorld");
                    new Selector(menu: menu, name: "autosave delay (minutes)", valueList: new List<Object> { 5, 10, 15, 30, 60 }, targetObj: new Preferences(), propertyName: "autoSaveDelayMins");
                    new Invoker(menu: menu, name: "return", closesMenu: true, actionName: Scheduler.ActionName.SavePrefs);
                    return menu;

                case Name.CreateNewIsland:
                    menu = new Menu(templateName: templateName, name: "CREATE NEW ISLAND", blocksUpdatesBelow: false, canBeClosedManually: true, closingSavesPrefs: true);
                    new Invoker(menu: menu, name: "start game", closesMenu: true, actionName: Scheduler.ActionName.CreateNewWorld);
                    new Selector(menu: menu, name: "width", valueList: new List<Object> { 1000, 2000, 4000, 8000, 16000, 20000, 30000 }, targetObj: new Preferences(), propertyName: "newWorldWidth");
                    new Selector(menu: menu, name: "height", valueList: new List<Object> { 1000, 2000, 4000, 8000, 16000, 20000, 30000 }, targetObj: new Preferences(), propertyName: "newWorldHeight");
                    new Invoker(menu: menu, name: "return", closesMenu: true, actionName: Scheduler.ActionName.SavePrefs);
                    return menu;

                case Name.Pause:
                    menu = new Menu(templateName: templateName, name: "PAUSE", blocksUpdatesBelow: true, canBeClosedManually: true);
                    new Invoker(menu: menu, name: "return to game", closesMenu: true, actionName: Scheduler.ActionName.Empty);
                    new Invoker(menu: menu, name: "save game", actionName: Scheduler.ActionName.OpenSaveMenu);
                    if (SaveManager.AnySavesExist) new Invoker(menu: menu, name: "load game", actionName: Scheduler.ActionName.OpenLoadMenu);
                    new Invoker(menu: menu, name: "options", actionName: Scheduler.ActionName.OpenOptionsMenu);
                    new Invoker(menu: menu, name: "return to main menu", closesMenu: true, actionName: Scheduler.ActionName.ReturnToMainMenu);
                    if (SonOfRobinGame.platform != Platform.Mobile)
                    {
                        var confirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit?" }, { "actionName", Scheduler.ActionName.QuitGame }, { "executeHelper", null } };
                        new Invoker(menu: menu, name: "quit game", actionName: Scheduler.ActionName.OpenConfirmationMenu, executeHelper: confirmationData);
                    }
                    return menu;

                case Name.Load:
                    menu = new Menu(templateName: templateName, name: "LOAD GAME", blocksUpdatesBelow: false, canBeClosedManually: true);
                    foreach (SaveInfo saveInfo in SaveManager.CorrectSaves)
                    { new Invoker(menu: menu, name: saveInfo.FullDescription, closesMenu: true, actionName: Scheduler.ActionName.LoadGame, executeHelper: saveInfo.folderName); }
                    new Separator(menu: menu, name: "", isEmpty: true);
                    new Invoker(menu: menu, name: "return", closesMenu: true, actionName: Scheduler.ActionName.Empty);
                    return menu;

                case Name.Save:
                    menu = new Menu(templateName: templateName, name: "SAVE GAME", blocksUpdatesBelow: false, canBeClosedManually: true);

                    new Invoker(menu: menu, name: "new save", actionName: Scheduler.ActionName.SaveGame, executeHelper: SaveManager.NewSaveSlotName, rebuildsMenu: true);
                    new Separator(menu: menu, name: "", isEmpty: true);

                    foreach (SaveInfo saveInfo in SaveManager.CorrectSaves)
                    {
                        if (!saveInfo.autoSave)
                        {
                            var confirmationData = new Dictionary<string, Object> { { "question", "The save will be overwritten. Continue?" }, { "actionName", Scheduler.ActionName.SaveGame }, { "executeHelper", saveInfo.folderName } };
                            new Invoker(menu: menu, name: saveInfo.FullDescription, actionName: Scheduler.ActionName.OpenConfirmationMenu, executeHelper: confirmationData, closesMenu: true);
                        }
                    }

                    new Separator(menu: menu, name: "", isEmpty: true);
                    new Invoker(menu: menu, name: "return", closesMenu: true, actionName: Scheduler.ActionName.Empty);
                    return menu;

                case Name.Debug:
                    menu = new Menu(templateName: templateName, name: "DEBUG MENU", blocksUpdatesBelow: false, canBeClosedManually: true);
                    new Invoker(menu: menu, name: "delete obsolete saves", actionName: Scheduler.ActionName.DeleteObsoleteSaves);
                    new Selector(menu: menu, name: "use multiple threads", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugUseMultipleThreads");
                    new Selector(menu: menu, name: "show sprite rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowRects");
                    new Selector(menu: menu, name: "show cells", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowCellData");
                    new Selector(menu: menu, name: "show states", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowStates");
                    new Selector(menu: menu, name: "show all stat bars", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowStatBars");
                    new Selector(menu: menu, name: "create missing pieces", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugCreateMissingPieces");

                    new Invoker(menu: menu, name: "return", closesMenu: true, actionName: Scheduler.ActionName.Empty);

                    return menu;

                case Name.CraftBasic:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Basic, label: "BASIC CRAFT");

                case Name.CraftNormal:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Normal, label: "NORMAL CRAFT");

                default:
                    throw new DivideByZeroException($"Unsupported menu templateName - {templateName}.");
            }
        }

        private static Menu CreateCraftMenu(Name templateName, Craft.Category category, string label)
        {
            Craft.PopulateAllCategories();

            PieceStorage storage = World.GetTopWorld().player.pieceStorage;

            Menu menu = new Menu(templateName: templateName, name: label, blocksUpdatesBelow: false, canBeClosedManually: true, layout: Menu.Layout.Middle);
            menu.bgColor = Color.LemonChiffon * 0.5f;

            var recipeList = Craft.recipesByCategory[category];
            foreach (Craft.Recipe recipe in recipeList)
            {
                new CraftInvoker(menu: menu, name: recipe.pieceToCreate.ToString(), closesMenu: false, actionName: Scheduler.ActionName.Craft, rebuildsMenu: true, executeHelper: recipe.pieceToCreate, recipe: recipe, storage: storage);
            }

            menu.EntriesRectColor = Color.SaddleBrown;
            menu.EntriesTextColor = Color.PaleGoldenrod;
            menu.EntriesOutlineColor = Color.SaddleBrown;

            return menu;
        }

    }
}
