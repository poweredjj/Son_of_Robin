using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MenuTemplate
    {
        public enum Name { Main, Options, Performance, Controls, Gamepad, Keyboard, Scale, OtherOptions, CreateNewIsland, SetSeed, OpenIslandTemplate, Pause, Load, Save, Tutorials, GameOver, Debug, CreateAnyPiece, GenericConfirm, CraftBasic, CraftNormal, CraftCooking, CraftFurnace }

        public static Menu CreateConfirmationMenu(Object confirmationData)
        {
            var confirmationDict = (Dictionary<string, Object>)confirmationData;

            string question = (string)confirmationDict["question"];

            var menu = new Menu(templateName: Name.GenericConfirm, name: question, blocksUpdatesBelow: false, canBeClosedManually: true, priority: 0);
            new Separator(menu: menu, name: "", isEmpty: true);
            new Invoker(menu: menu, name: "no", closesMenu: true, taskName: Scheduler.TaskName.Empty);
            new Invoker(menu: menu, name: "yes", closesMenu: true, taskName: Scheduler.TaskName.ProcessConfirmation, executeHelper: confirmationData);

            return menu;
        }

        public static Menu CreateMenuFromTemplate(Name templateName)
        {
            Menu menu;
            World world;

            // Range example - valueList: Enumerable.Range(-1, 300).Cast<object>().ToList()

            switch (templateName)
            {
                case Name.Main:
                    {
                        menu = new Menu(templateName: templateName, name: "Son of Robin", blocksUpdatesBelow: false, canBeClosedManually: false);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "create new island", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.CreateNewIsland } },
                               infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "start new game", color: Color.White, scale: 1f) });
                        new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenMenuTemplate, new Dictionary<string, Object> { { "templateName", Name.Options } },
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "options, settings, etc.", color: Color.White, scale: 1f) });

                        string text = $"Son of Robin {SonOfRobinGame.version.ToString().Replace(",", ".")}\nLast updated: {SonOfRobinGame.lastChanged:yyyy-MM-dd.}\n\nThis is a very early alpha version of the game.\nCode by Marcin Smidowicz.\nFastNoiseLite by Jordan Peck.\nController icons by Nicolae (Xelu) Berbece.\nStudies.Joystick by Luiz Ossinho.\nPNG library 'Big Gustave' by Eliot Jones.\nVarious free assets used.";

                        var textWindowData = new Dictionary<string, Object> { { "text", text }, };
                        new Invoker(menu: menu, name: "about", taskName: Scheduler.TaskName.OpenTextWindow, executeHelper: textWindowData,
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "information about the game", color: Color.White, scale: 1f) });

                        if (SonOfRobinGame.platform != Platform.Mobile) new Invoker(menu: menu, name: "quit game", closesMenu: true, taskName: Scheduler.TaskName.QuitGame);

                        return menu;
                    }

                case Name.Options:
                    {
                        menu = new Menu(templateName: templateName, name: "OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        new Invoker(menu: menu, name: "controls", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Controls } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "controls settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "performance", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Performance } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "performance settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "scale", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Scale } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "scale settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "other", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.OtherOptions } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "misc. settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "debug", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Debug } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "only for the brave ones ;)", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Scale:
                    {
                        menu = new Menu(templateName: templateName, name: "SCALE", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        new Selector(menu: menu, name: "world scale", valueList: new List<Object> { 0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f }, targetObj: new Preferences(), propertyName: "worldScale");
                        new Selector(menu: menu, name: "global scale", valueList: new List<Object> { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }, targetObj: new Preferences(), propertyName: "GlobalScale", rebuildsMenu: true, rebuildsMenuInstantScroll: true);
                        new Selector(menu: menu, name: "menu scale", valueList: new List<Object> { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f }, targetObj: new Preferences(), propertyName: "menuScale", rebuildsMenu: true, rebuildsMenuInstantScroll: true);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.OtherOptions:
                    {
                        world = World.GetTopWorld();

                        menu = new Menu(templateName: templateName, name: "OTHER OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);


                        new Selector(menu: menu, name: "show hints", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showHints");

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            new Selector(menu: menu, name: "fullscreen mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "FullScreenMode", rebuildsMenu: true);
                            if (Preferences.FullScreenMode) new Selector(menu: menu, name: "resolution", valueList: Preferences.AvailableScreenModes, targetObj: new Preferences(), propertyName: "FullScreenResolution");
                        }

                        if (world == null || world.demoMode) new Selector(menu: menu, name: "load whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "loadWholeMap", rebuildsMenu: true);
                        if (SonOfRobinGame.platform == Platform.Mobile && !Preferences.loadWholeMap) new Selector(menu: menu, name: "max buffered map blocks", valueList: new List<Object> { 100, 500, 1000, 2000, 4000 }, targetObj: new Preferences(), propertyName: "mobileMaxLoadedTextures");

                        new Selector(menu: menu, name: "show demo world", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showDemoWorld");
                        new Selector(menu: menu, name: "autosave delay (minutes)", valueList: new List<Object> { 5, 10, 15, 30, 60 }, targetObj: new Preferences(), propertyName: "autoSaveDelayMins");
                        new Invoker(menu: menu, name: "delete old island templates", taskName: Scheduler.TaskName.DeleteTemplates);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Performance:
                    {
                        menu = new Menu(templateName: templateName, name: "DETAILS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        new Selector(menu: menu, name: "sun shadows", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "drawSunShadows");

                        new Selector(menu: menu, name: "light and darkness", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showLighting", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "draw light and darkness", color: Color.White, scale: 1f) }, rebuildsMenu: true);

                        if (Preferences.showLighting)
                        {
                            new Selector(menu: menu, name: "darkness resolution", valueDict: Preferences.namesForDarknessRes, targetObj: new Preferences(), propertyName: "darknessResolution");
                            new Selector(menu: menu, name: "shadows", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "drawShadows", rebuildsMenu: true);
                        }

                        new Selector(menu: menu, name: "debris", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showDebris", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "show small debris when hitting objects", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "frameskip", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "FrameSkip", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "skip frames to maintain speed", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "use multiple CPU cores", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "useMultipleThreads", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "use all available CPU cores for some operations", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Controls:
                    {
                        menu = new Menu(templateName: templateName, name: "CONTROLS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        new Invoker(menu: menu, name: "configure gamepad", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Gamepad } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all gamepad controls", color: Color.White, scale: 1f) });
                        new Invoker(menu: menu, name: "configure keyboard", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Keyboard } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all keyboard controls", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (SonOfRobinGame.platform != Platform.Mobile) new Selector(menu: menu, name: "on screen controls", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "EnableTouch", rebuildsMenu: true);

                        new Selector(menu: menu, name: "gamepad type", valueDict: new Dictionary<object, object> { { ButtonScheme.Type.M, "M" }, { ButtonScheme.Type.S, "S" }, { ButtonScheme.Type.N, "N" } }, targetObj: new Preferences(), propertyName: "ControlTipsScheme");

                        new Selector(menu: menu, name: "show control tips", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "ShowControlTips", rebuildsMenu: true);
                        if (Preferences.ShowControlTips)
                        {
                            new Selector(menu: menu, name: "show control tips on field", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "showFieldControlTips", rebuildsMenu: true);
                            if (Preferences.showFieldControlTips) new Selector(menu: menu, name: "field tips scale", valueDict: Preferences.namesForFieldControlTipsScale, targetObj: new Preferences(), propertyName: "fieldControlTipsScale");
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }


                case Name.Gamepad:
                    {
                        menu = new Menu(templateName: templateName, name: "CONFIGURE GAMEPAD", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        var allButtonsList = new List<Buttons> { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.Start, Buttons.Back, Buttons.LeftStick, Buttons.RightStick, Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp, Buttons.DPadDown };
                        var allButtonsDict = new Dictionary<object, object>();

                        foreach (Buttons button in allButtonsList)
                        { allButtonsDict[button] = InputVis.GetTexture(button); }

                        var analogSticksList = new List<InputMapper.AnalogType> { InputMapper.AnalogType.PadLeft, InputMapper.AnalogType.PadRight };
                        var analogSticksDict = new Dictionary<object, object>();
                        foreach (InputMapper.AnalogType analog in analogSticksList)
                        { analogSticksDict[analog] = InputVis.GetTexture(analog); }

                        CreateControlsMappingEntries(menu: menu, gamepad: true, analogSticksDict: analogSticksDict, keysOrButtonsDict: allButtonsDict);

                        return menu;
                    }

                case Name.Keyboard:
                    {
                        menu = new Menu(templateName: templateName, name: "CONFIGURE KEYBOARD", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        Dictionary<object, object> allKeysDict = KeyboardScheme.KeyTextures.ToDictionary(k => (object)k.Key, k => (object)k.Value);

                        var analogSticksList = new List<InputMapper.AnalogType> { InputMapper.AnalogType.Arrows, InputMapper.AnalogType.WASD, InputMapper.AnalogType.Numpad };
                        var analogSticksDict = new Dictionary<object, object>();
                        foreach (InputMapper.AnalogType analog in analogSticksList)
                        { analogSticksDict[analog] = InputVis.GetTexture(analog); }

                        CreateControlsMappingEntries(menu: menu, gamepad: false, analogSticksDict: analogSticksDict, keysOrButtonsDict: allKeysDict);

                        return menu;
                    }

                case Name.CreateNewIsland:
                    {
                        menu = new Menu(templateName: templateName, name: "CREATE NEW ISLAND", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs);

                        new Invoker(menu: menu, name: "start game", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld, executeHelper: new Dictionary<string, Object> { { "width", Preferences.newWorldWidth }, { "height", Preferences.newWorldHeight }, { "seed", Preferences.NewWorldSeed }, { "resDivider", Preferences.newWorldResDivider } });

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Selector(menu: menu, name: "customize", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "CustomizeWorld", rebuildsMenu: true);

                        if (Preferences.CustomizeWorld)
                        {
                            List<Object> sizeList;
                            if (SonOfRobinGame.platform == Platform.Desktop)
                            { sizeList = new List<Object> { 1000, 2000, 4000, 8000, 10000, 15000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 100000 }; }
                            else
                            { sizeList = new List<Object> { 1000, 2000, 4000, 8000, 10000, 15000, 20000, 30000, 40000, 50000 }; }

                            new Selector(menu: menu, name: "width", valueList: sizeList, targetObj: new Preferences(), propertyName: "newWorldWidth", rebuildsMenu: true);
                            new Selector(menu: menu, name: "height", valueList: sizeList, targetObj: new Preferences(), propertyName: "newWorldHeight", rebuildsMenu: true);

                            new Selector(menu: menu, name: "terrain detail", valueDict: Preferences.namesForResDividers, targetObj: new Preferences(), propertyName: "newWorldResDivider");

                            new Selector(menu: menu, name: "random seed", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "randomSeed", rebuildsMenu: true);

                            if (!Preferences.randomSeed) new Invoker(menu: menu, name: "set seed", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.SetSeed } });
                        }
                        else
                        {
                            List<Object> sizeList = new List<Object> { Preferences.WorldSize.small, Preferences.WorldSize.medium, Preferences.WorldSize.large };
                            if (SonOfRobinGame.platform == Platform.Desktop)
                            {
                                sizeList.Add(Preferences.WorldSize.gigantic);
                            }

                            new Selector(menu: menu, name: "size", valueList: sizeList, targetObj: new Preferences(), propertyName: "SelectedWorldSize", rebuildsMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"{Preferences.newWorldWidth}x{Preferences.newWorldHeight}", color: Color.White, scale: 1f) });
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (GridTemplate.CorrectTemplatesExist)
                        {
                            new Invoker(menu: menu, name: "open previous island", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.OpenIslandTemplate } },
                          infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "enter a fresh instance of a previously visited island", color: Color.White, scale: 1f) });
                            new Separator(menu: menu, name: "", isEmpty: true);
                        }

                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.SetSeed:
                    {
                        menu = new Menu(templateName: templateName, name: "ENTER SEED", blocksUpdatesBelow: false, canBeClosedManually: true);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        var digitList = new List<Object> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

                        new Separator(menu: menu, name: $"current seed: {String.Format("{0:0000}", Preferences.NewWorldSeed)}");

                        for (int i = 0; i < 4; i++)
                        {
                            new Selector(menu: menu, name: $"digit {i + 1}", valueList: digitList, targetObj: new Preferences(), propertyName: $"seedDigit{i + 1}", rebuildsMenu: true);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.OpenIslandTemplate:
                    {
                        menu = new Menu(templateName: templateName, name: "CREATE ISLAND FROM TEMPLATE", blocksUpdatesBelow: false, canBeClosedManually: true);

                        foreach (GridTemplate gridTemplate in GridTemplate.CorrectTemplates)
                        {
                            new Invoker(menu: menu, name: $"{gridTemplate.width}x{gridTemplate.height}  seed  {String.Format("{0:0000}", gridTemplate.seed)}  detail {Preferences.namesForResDividers[gridTemplate.resDivider]}", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld, executeHelper: new Dictionary<string, Object> { { "width", gridTemplate.width }, { "height", gridTemplate.height }, { "seed", gridTemplate.seed }, { "resDivider", gridTemplate.resDivider } });
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.Pause:
                    {
                        world = World.GetTopWorld();

                        menu = new Menu(templateName: templateName, name: "PAUSE", blocksUpdatesBelow: true, canBeClosedManually: true);
                        new Invoker(menu: menu, name: "return to game", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        if (!world.SpectatorMode && world.player?.activeState == BoardPiece.State.PlayerControlledWalking && world.player.alive) new Invoker(menu: menu, name: "save game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Save } });
                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Options } });
                        if (world.hintEngine.shownTutorials.Count > 0) new Invoker(menu: menu, name: "tutorials", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Tutorials } });
                        new Invoker(menu: menu, name: "return to main menu", closesMenu: true, taskName: Scheduler.TaskName.ReturnToMainMenu);
                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            var confirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit?" }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                            new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                        }
                        return menu;
                    }

                case Name.GameOver:
                    {
                        menu = new Menu(templateName: templateName, name: "GAME OVER", blocksUpdatesBelow: false, canBeClosedManually: false, layout: Menu.Layout.Middle);
                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "return to main menu", closesMenu: true, taskName: Scheduler.TaskName.ReturnToMainMenu);
                        new Invoker(menu: menu, name: "enter spectator mode", closesMenu: true, taskName: Scheduler.TaskName.SetSpectatorMode, executeHelper: true);

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            var confirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit?" }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                            new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                        }
                        return menu;
                    }

                case Name.Load:
                    {
                        menu = new Menu(templateName: templateName, name: "LOAD GAME", blocksUpdatesBelow: false, canBeClosedManually: true);
                        foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                        {
                            new Invoker(menu: menu, name: saveInfo.FullDescription, closesMenu: true, taskName: Scheduler.TaskName.LoadGame, executeHelper: saveInfo.folderName,
                                 infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: saveInfo.AdditionalInfo, color: Color.White, scale: 1f) });
                        }
                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.Save:
                    {
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
                    }

                case Name.CreateAnyPiece:
                    {
                        menu = new Menu(templateName: templateName, name: "CREATE ANY ITEM", blocksUpdatesBelow: false, canBeClosedManually: true);

                        Dictionary<string, Object> createData;
                        world = World.GetTopWorld();
                        if (world == null) return menu;

                        foreach (PieceTemplate.Name pieceName in (PieceTemplate.Name[])Enum.GetValues(typeof(PieceTemplate.Name)))
                        {
                            createData = new Dictionary<string, Object> { { "position", world.player.sprite.position }, { "templateName", pieceName } };

                            new Invoker(menu: menu, name: $"{PieceInfo.info[pieceName].readableName}", taskName: Scheduler.TaskName.CreateDebugPieces, createData);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Debug:
                    {
                        menu = new Menu(templateName: templateName, name: "DEBUG MENU", blocksUpdatesBelow: false, canBeClosedManually: true);

                        world = World.GetTopWorld();

                        new Selector(menu: menu, name: "debug mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "DebugMode", rebuildsMenu: true);

                        if (world != null && !world.demoMode) new Invoker(menu: menu, name: "create any piece", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.CreateAnyPiece } });
                        new Selector(menu: menu, name: "create missing pieces", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugCreateMissingPieces");
                        new Selector(menu: menu, name: "god mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "DebugGodMode", rebuildsMenu: true);
                        new Selector(menu: menu, name: "show whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "DebugShowWholeMap");
                        new Selector(menu: menu, name: "show all items on map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowAllMapPieces");
                        new Invoker(menu: menu, name: "restore all hints", taskName: Scheduler.TaskName.RestoreHints);
                        new Selector(menu: menu, name: "show fruit rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowFruitRects");
                        new Selector(menu: menu, name: "show sprite rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowRects");
                        new Selector(menu: menu, name: "show cells", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowCellData");
                        new Selector(menu: menu, name: "show animal targets", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowAnimalTargets");
                        new Selector(menu: menu, name: "show all stat bars", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowStatBars");
                        new Selector(menu: menu, name: "show states", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: new Preferences(), propertyName: "debugShowStates");

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        return menu;
                    }

                case Name.CraftBasic:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Field, label: "FIELD CRAFT");

                case Name.CraftNormal:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Workshop, label: "WORKSHOP");

                case Name.CraftFurnace:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Furnace, label: "FURNACE");

                case Name.Tutorials:
                    {
                        world = World.GetTopWorld();
                        menu = new Menu(templateName: templateName, name: "TUTORIALS", blocksUpdatesBelow: true, canBeClosedManually: true);

                        foreach (Tutorials.Tutorial tutorial in Tutorials.TutorialsInMenu)
                        {
                            if (world.hintEngine.shownTutorials.Contains(tutorial.type))
                            {
                                new Invoker(menu: menu, name: tutorial.name, taskName: Scheduler.TaskName.ShowTutorial, executeHelper: tutorial.type);
                            }
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        return menu;
                    }


                default:
                    throw new DivideByZeroException($"Unsupported menu templateName - {templateName}.");
            }
        }

        private static Menu CreateCraftMenu(Name templateName, Craft.Category category, string label)
        {
            Player player = World.GetTopWorld().player;
            List<PieceStorage> storageList = player.CraftStorages;

            if (player.AreEnemiesNearby)
            {
                new TextWindow(text: "I can't craft with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
                return null;
            }

            Tutorials.ShowTutorial(type: Tutorials.Type.Craft, ignoreIfShown: true, ignoreDelay: true, menuMode: true);

            Menu menu = new Menu(templateName: templateName, name: label, blocksUpdatesBelow: true, canBeClosedManually: true, layout: SonOfRobinGame.platform == Platform.Mobile ? Menu.Layout.Right : Menu.Layout.Left, alwaysShowSelectedEntry: true);
            menu.bgColor = Color.LemonChiffon * 0.5f;

            var recipeList = Craft.recipesByCategory[category];
            foreach (Craft.Recipe recipe in recipeList)
            {
                new CraftInvoker(menu: menu, name: recipe.pieceToCreate.ToString(), closesMenu: false, taskName: Scheduler.TaskName.Craft, rebuildsMenu: true, executeHelper: recipe, recipe: recipe, storageList: storageList);
            }

            menu.EntriesRectColor = Color.SaddleBrown;
            menu.EntriesTextColor = Color.PaleGoldenrod;
            menu.EntriesOutlineColor = Color.SaddleBrown;

            return menu;
        }

        private static void CreateControlsMappingEntries(Menu menu, bool gamepad, Dictionary<object, object> analogSticksDict, Dictionary<object, object> keysOrButtonsDict)
        {
            MappingPackage newMapping = gamepad ? InputMapper.newMappingGamepad : InputMapper.newMappingKeyboard;

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "general");
            foreach (string propertyName in new List<string> { "leftStick", "rightStick", })
            {
                if (!gamepad && propertyName == "rightStick") continue;
                new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: analogSticksDict, targetObj: newMapping, propertyName: propertyName);
            }

            foreach (string propertyName in new List<string> { "confirm", "cancel" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "field");
            foreach (string propertyName in new List<string> { "interact", "useTool", "pickUp", "run", "zoomOut" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "menus");
            foreach (string propertyName in new List<string> { "pauseMenu", "craft", "equip", "inventory" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "inventory");
            foreach (string propertyName in new List<string> { "pickOne", "pickStack", })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "toolbar");
            foreach (string propertyName in new List<string> { "toolbarPrev", "toolbarNext", })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName); }

            new Separator(menu: menu, name: "", isEmpty: true);

            new Invoker(menu: menu, name: "save", taskName: Scheduler.TaskName.SaveControls, executeHelper: gamepad, rebuildsMenu: true);
            new Invoker(menu: menu, name: "reset", taskName: Scheduler.TaskName.ResetControls, executeHelper: gamepad, rebuildsMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all controls to default", color: Color.White, scale: 1f) });

            new Separator(menu: menu, name: "", isEmpty: true);
            new Invoker(menu: menu, name: "return without saving", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
        }

    }
}
