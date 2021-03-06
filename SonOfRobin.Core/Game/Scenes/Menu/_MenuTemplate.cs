using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MenuTemplate
    {
        public enum Name { Main, Options, Sound, Graphics, Controls, Gamepad, Keyboard, Scale, OtherOptions, CreateNewIsland, SetSeed, OpenIslandTemplate, Pause, Load, Save, Tutorials, GameOver, Debug, SoundTest, GfxListTest, Shelter, CreateAnyPiece, GenericConfirm, CraftField, CraftEssential, CraftBasic, CraftAdvanced, CraftMaster, CraftAlchemy, CraftFurnace, CraftAnvil, CraftLeatherBasic, CraftLeatherAdvanced }

        public static Menu CreateConfirmationMenu(Object confirmationData)
        {
            var confirmationDict = (Dictionary<string, Object>)confirmationData;

            string question = (string)confirmationDict["question"];
            bool blocksUpdatesBelow = confirmationDict.ContainsKey("blocksUpdatesBelow") ? (bool)confirmationDict["blocksUpdatesBelow"] : false;

            var menu = new Menu(templateName: Name.GenericConfirm, name: question, blocksUpdatesBelow: blocksUpdatesBelow, canBeClosedManually: true, priority: 0, templateExecuteHelper: null);
            new Separator(menu: menu, name: "", isEmpty: true);

            if (confirmationDict.ContainsKey("customOptionList"))
            {
                var optionList = (List<object>)confirmationDict["customOptionList"];

                foreach (var optionData in optionList)
                {
                    var optionDict = (Dictionary<string, Object>)optionData;

                    new Invoker(menu: menu, name: (string)optionDict["label"], closesMenu: true, taskName: (Scheduler.TaskName)optionDict["taskName"], executeHelper: optionDict["executeHelper"]);
                }
            }
            else
            {
                string labelYes = confirmationDict.ContainsKey("labelYes") ? (string)confirmationDict["labelYes"] : "yes";
                string labelNo = confirmationDict.ContainsKey("labelNo") ? (string)confirmationDict["labelNo"] : "no";

                new Invoker(menu: menu, name: labelNo, closesMenu: true, taskName: Scheduler.TaskName.Empty);
                new Invoker(menu: menu, name: labelYes, closesMenu: true, taskName: Scheduler.TaskName.ProcessConfirmation, executeHelper: confirmationData);
            }

            return menu;
        }


        public static Menu CreateMenuFromTemplate(Name templateName, object executeHelper = null)
        {
            Preferences preferences = new Preferences();

            // Range example - valueList: Enumerable.Range(-1, 300).Cast<object>().ToList()

            switch (templateName)
            {
                case Name.Main:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "Son of Robin", blocksUpdatesBelow: false, canBeClosedManually: false, templateExecuteHelper: executeHelper);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "create new island", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.CreateNewIsland } },
                               infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "start new game", color: Color.White, scale: 1f) });
                        new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenMenuTemplate, new Dictionary<string, Object> { { "templateName", Name.Options } },
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "options, settings, etc.", color: Color.White, scale: 1f) });

                        string text = $"Son of Robin {SonOfRobinGame.version.ToString().Replace(",", ".")}\nLast updated: {SonOfRobinGame.lastChanged:yyyy-MM-dd.}\n\nThis is a very early alpha version of the game.\nCode: Marcin Smidowicz.\nFastNoiseLite: Jordan Peck.\nSounds: freesound.org.\nController icons: Nicolae (Xelu) Berbece.\nStudies.Joystick: Luiz Ossinho.\nPNG library 'Big Gustave': Eliot Jones.\nVarious free graphics assets used.";

                        var textWindowData = new Dictionary<string, Object> { { "text", text } };
                        new Invoker(menu: menu, name: "about", taskName: Scheduler.TaskName.ShowTextWindow, executeHelper: textWindowData,
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "information about the game", color: Color.White, scale: 1f) });

                        if (SonOfRobinGame.platform != Platform.Mobile) new Invoker(menu: menu, name: "quit game", closesMenu: true, taskName: Scheduler.TaskName.QuitGame);

                        return menu;
                    }

                case Name.Options:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Invoker(menu: menu, name: "controls", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Controls } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "controls settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "graphics", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Graphics } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "graphics settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "sound", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Sound } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "sound settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "scale", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Scale } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "scale settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "other", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.OtherOptions } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "misc. settings", color: Color.White, scale: 1f) });

                        new Invoker(menu: menu, name: "debug", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Debug } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "only for the brave ones ;)", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Scale:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "SCALE", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Selector(menu: menu, name: "world scale", valueList: new List<Object> { 0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f }, targetObj: preferences, propertyName: "WorldScale");
                        new Selector(menu: menu, name: "global scale", valueList: new List<Object> { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }, targetObj: preferences, propertyName: "GlobalScale", rebuildsMenu: true, rebuildsMenuInstantScroll: true);
                        new Selector(menu: menu, name: "menu scale", valueList: new List<Object> { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f, 4.5f, 5f }, targetObj: preferences, propertyName: "menuScale", rebuildsMenu: true, rebuildsMenuInstantScroll: true);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.OtherOptions:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new Menu(templateName: templateName, name: "OTHER OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Selector(menu: menu, name: "show hints", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showHints");

                        if (world == null || world.demoMode) new Selector(menu: menu, name: "load whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "loadWholeMap", rebuildsMenu: true);

                        new Selector(menu: menu, name: "use multiple CPU cores", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "useMultipleThreads", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "use all available CPU cores for some operations", color: Color.White, scale: 1f) });

                        if (SonOfRobinGame.platform == Platform.Mobile && !Preferences.loadWholeMap) new Selector(menu: menu, name: "max buffered map blocks", valueList: new List<Object> { 100, 500, 1000, 2000, 4000 }, targetObj: preferences, propertyName: "mobileMaxLoadedTextures");

                        new Selector(menu: menu, name: "show demo world", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showDemoWorld");
                        //new Selector(menu: menu, name: "autosave delay (minutes)", valueList: new List<Object> { 5, 10, 15, 30, 60 }, targetObj: preferences, propertyName: "autoSaveDelayMins"); // autosave is not needed anymore
                        new Invoker(menu: menu, name: "delete old island templates", taskName: Scheduler.TaskName.DeleteTemplates);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Graphics:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new Menu(templateName: templateName, name: "GRAPHICS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            new Selector(menu: menu, name: "fullscreen mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "FullScreenMode", rebuildsMenu: true);
                            if (Preferences.FullScreenMode) new Selector(menu: menu, name: "resolution", valueList: Preferences.AvailableScreenModes, targetObj: preferences, propertyName: "FullScreenResolution");

                            new Selector(menu: menu, name: "vertical sync", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "VSync", rebuildsMenu: true);
                        }

                        new Selector(menu: menu, name: "frameskip", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "FrameSkip", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "skip frames to maintain speed", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "sun shadows", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "drawSunShadows");

                        new Selector(menu: menu, name: "light and darkness", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showLighting", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "draw light and darkness", color: Color.White, scale: 1f) }, rebuildsMenu: true);

                        if (Preferences.showLighting)
                        {
                            new Selector(menu: menu, name: "darkness resolution", valueDict: Preferences.namesForDarknessRes, targetObj: preferences, propertyName: "DarknessResolution");
                            new Selector(menu: menu, name: "shadows", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "drawShadows", rebuildsMenu: true);
                        }

                        new Selector(menu: menu, name: "debris", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showDebris", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "show small debris when hitting objects", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Sound:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "SOUND", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        Sound sound = new Sound(SoundData.Name.Empty); // sound name doesn't really matter

                        new Selector(menu: menu, name: "sound", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: sound, propertyName: "globalOn", rebuildsMenu: true);

                        if (Sound.globalOn)
                        {
                            new Selector(menu: menu, name: "menu sounds", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: sound, propertyName: "menuOn");

                            new Selector(menu: menu, name: "text sounds", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: sound, propertyName: "textWindowAnimOn");

                            var valueDict = new Dictionary<object, object>();

                            for (int i = 0; i < 105; i += 5)
                            {
                                valueDict[(float)i / 100] = $"{i}%";
                            }

                            new Selector(menu: menu, name: "volume", valueDict: valueDict, targetObj: sound, propertyName: "globalVolume");
                        }
                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Controls:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "CONTROLS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Invoker(menu: menu, name: "configure gamepad", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Gamepad } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all gamepad controls", color: Color.White, scale: 1f) });
                        new Invoker(menu: menu, name: "configure keyboard", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Keyboard } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all keyboard controls", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (SonOfRobinGame.platform != Platform.Mobile && SonOfRobinGame.os == OS.Windows) new Selector(menu: menu, name: "on screen buttons", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "EnableTouchButtons", rebuildsMenu: true);

                        if (Preferences.EnableTouchButtons)
                        {
                            new Selector(menu: menu, name: "on screen joysticks", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "enableTouchJoysticks");
                        }

                        if (SonOfRobinGame.os == OS.Windows || SonOfRobinGame.os == OS.Android || SonOfRobinGame.os == OS.iOS)
                        {
                            new Selector(menu: menu, name: SonOfRobinGame.platform == Platform.Mobile ? "touch to walk" : "point to walk", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "PointToWalk", rebuildsMenu: true);
                            if (Preferences.PointToWalk) new Selector(menu: menu, name: SonOfRobinGame.platform == Platform.Mobile ? "touch to interact" : "point to interact", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "PointToInteract");
                        }

                        new Selector(menu: menu, name: "gamepad type", valueDict: new Dictionary<object, object> { { ButtonScheme.Type.M, "M" }, { ButtonScheme.Type.S, "S" }, { ButtonScheme.Type.N, "N" } }, targetObj: preferences, propertyName: "ControlTipsScheme");

                        new Selector(menu: menu, name: "show control tips", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "ShowControlTips", rebuildsMenu: true);
                        if (Preferences.ShowControlTips)
                        {
                            new Selector(menu: menu, name: "show control tips on field", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showFieldControlTips", rebuildsMenu: true);
                            if (Preferences.showFieldControlTips) new Selector(menu: menu, name: "field tips scale", valueDict: Preferences.namesForFieldControlTipsScale, targetObj: preferences, propertyName: "fieldControlTipsScale");
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Gamepad:
                    {
                        bool gamepad = true;
                        Menu menu = new Menu(templateName: templateName, name: "CONFIGURE GAMEPAD", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.CheckForNonSavedControls, closingTaskHelper: gamepad, templateExecuteHelper: executeHelper);

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
                        bool gamepad = false;
                        Menu menu = new Menu(templateName: templateName, name: "CONFIGURE KEYBOARD", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.CheckForNonSavedControls, closingTaskHelper: gamepad, templateExecuteHelper: executeHelper);

                        Dictionary<object, object> allKeysDict = KeyboardScheme.KeyTextures.ToDictionary(k => (object)k.Key, k => (object)k.Value);
                        CreateControlsMappingEntries(menu: menu, gamepad: false, analogSticksDict: null, keysOrButtonsDict: allKeysDict);

                        return menu;
                    }

                case Name.CreateNewIsland:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "CREATE NEW ISLAND", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Invoker(menu: menu, name: "start game", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld, sound: SoundData.Name.NewGameStart);
                        new Invoker(menu: menu, name: "reset settings", closesMenu: false, taskName: Scheduler.TaskName.ResetNewWorldSettings, rebuildsMenu: true);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        new Selector(menu: menu, name: "character", valueDict: new Dictionary<object, object> { { true, AnimData.framesForPkgs[AnimData.PkgName.PlayerFemale].texture }, { false, AnimData.framesForPkgs[AnimData.PkgName.PlayerMale].texture } }, targetObj: preferences, propertyName: "newWorldPlayerFemale", rebuildsAllMenus: true);

                        new Selector(menu: menu, name: "animals population", valueDict: Preferences.namesForAnimalsMultiplier, targetObj: preferences, propertyName: "newWorldMaxAnimalsMultiplier", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "agressive animals", valueDict: new Dictionary<object, object> { { true, "yes" }, { false, "no" } }, targetObj: preferences, propertyName: "newWorldAgressiveAnimals", rebuildsAllMenus: true);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Selector(menu: menu, name: "customize", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "CustomizeWorld", rebuildsMenu: true);

                        if (Preferences.CustomizeWorld)
                        {
                            List<Object> sizeList;
                            if (SonOfRobinGame.platform == Platform.Desktop)
                            { sizeList = new List<Object> { 1000, 2000, 4000, 8000, 10000, 15000, 20000, 30000, 40000, 50000, 60000, 70000, 80000, 90000, 100000 }; }
                            else
                            { sizeList = new List<Object> { 1000, 2000, 4000, 8000, 10000, 15000, 20000, 30000, 40000, 50000 }; }

                            new Selector(menu: menu, name: "width", valueList: sizeList, targetObj: preferences, propertyName: "newWorldWidth");
                            new Selector(menu: menu, name: "height", valueList: sizeList, targetObj: preferences, propertyName: "newWorldHeight");
                            new Selector(menu: menu, name: "terrain detail", valueDict: Preferences.namesForResDividers, targetObj: preferences, propertyName: "newWorldResDivider");

                            new Selector(menu: menu, name: "random seed", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "randomSeed", rebuildsMenu: true);

                            if (!Preferences.randomSeed) new Invoker(menu: menu, name: "set seed", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.SetSeed } });
                        }
                        else
                        {
                            List<Object> sizeList = new List<Object> { Preferences.WorldSize.small, Preferences.WorldSize.medium, Preferences.WorldSize.large };
                            if (SonOfRobinGame.platform == Platform.Desktop)
                            {
                                sizeList.Add(Preferences.WorldSize.gigantic);
                            }

                            new Selector(menu: menu, name: "size", valueList: sizeList, targetObj: preferences, propertyName: "SelectedWorldSize", rebuildsMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"{Preferences.newWorldWidth}x{Preferences.newWorldHeight}", color: Color.White, scale: 1f) });
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
                        Menu menu = new Menu(templateName: templateName, name: "ENTER SEED", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        var digitList = new List<Object> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

                        new Separator(menu: menu, name: $"current seed: {String.Format("{0:0000}", Preferences.NewWorldSeed)}");

                        for (int i = 0; i < 4; i++)
                        {
                            new Selector(menu: menu, name: $"digit {i + 1}", valueList: digitList, targetObj: preferences, propertyName: $"seedDigit{i + 1}", rebuildsMenu: true);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.OpenIslandTemplate:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "CREATE ISLAND FROM TEMPLATE", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        new Selector(menu: menu, name: "character", valueDict: new Dictionary<object, object> { { true, AnimData.framesForPkgs[AnimData.PkgName.PlayerFemale].texture }, { false, AnimData.framesForPkgs[AnimData.PkgName.PlayerMale].texture } }, targetObj: preferences, propertyName: "newWorldPlayerFemale", rebuildsAllMenus: true);

                        new Selector(menu: menu, name: "animals population", valueDict: Preferences.namesForAnimalsMultiplier, targetObj: preferences, propertyName: "newWorldMaxAnimalsMultiplier", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "agressive animals", valueDict: new Dictionary<object, object> { { true, "yes" }, { false, "no" } }, targetObj: preferences, propertyName: "newWorldAgressiveAnimals", rebuildsAllMenus: true);

                        new Separator(menu: menu, name: "", isEmpty: true);

                        foreach (GridTemplate gridTemplate in GridTemplate.CorrectTemplates)
                        {
                            new Invoker(menu: menu, name: $"{gridTemplate.width}x{gridTemplate.height}  seed  {String.Format("{0:0000}", gridTemplate.seed)}  detail {Preferences.namesForResDividers[gridTemplate.resDivider]}", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld, executeHelper: new Dictionary<string, Object> { { "width", gridTemplate.width }, { "height", gridTemplate.height }, { "seed", gridTemplate.seed }, { "resDivider", gridTemplate.resDivider }, { "initialMaxAnimalsMultiplier", Preferences.newWorldMaxAnimalsMultiplier }, { "addAgressiveAnimals", Preferences.newWorldAgressiveAnimals }, { "playerFemale", Preferences.newWorldPlayerFemale } }, sound: SoundData.Name.NewGameStart);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.Pause:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new Menu(templateName: templateName, name: "PAUSE", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper, soundOpen: SoundData.Name.PaperMove1, soundClose: SoundData.Name.PaperMove2);
                        new Invoker(menu: menu, name: "return to game", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        if ((Preferences.debugSaveEverywhere || Preferences.DebugMode) && !world.SpectatorMode && world.player?.activeState == BoardPiece.State.PlayerControlledWalking && world.player.alive) new Invoker(menu: menu, name: "save game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Save } });
                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Options } });
                        if (world.hintEngine.shownTutorials.Count > 0) new Invoker(menu: menu, name: "tutorials", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Tutorials } });

                        var restartConfirmationData = new Dictionary<string, Object> { { "question", "Restart this island?" }, { "taskName", Scheduler.TaskName.RestartWorld }, { "executeHelper", World.GetTopWorld() } };
                        new Invoker(menu: menu, name: "restart this island", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: restartConfirmationData);

                        var returnConfirmationData = new Dictionary<string, Object> { { "question", "Do you really want to exit? You will lose unsaved progress." }, { "taskName", Scheduler.TaskName.ReturnToMainMenu }, { "executeHelper", null } };
                        new Invoker(menu: menu, name: "return to main menu", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: returnConfirmationData);

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            var quitConfirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit? You will lose unsaved progress." }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                            new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: quitConfirmationData);
                        }
                        return menu;
                    }

                case Name.GameOver:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "GAME OVER", blocksUpdatesBelow: false, canBeClosedManually: false, layout: Menu.Layout.Middle, templateExecuteHelper: executeHelper);
                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "enter spectator mode", closesMenu: true, taskName: Scheduler.TaskName.SetSpectatorMode, executeHelper: true);
                        new Invoker(menu: menu, name: "restart this island", closesMenu: true, taskName: Scheduler.TaskName.RestartWorld, executeHelper: World.GetTopWorld());
                        new Invoker(menu: menu, name: "return to main menu", closesMenu: true, taskName: Scheduler.TaskName.ReturnToMainMenu);

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            var confirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit?" }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                            new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData);
                        }
                        return menu;
                    }

                case Name.Load:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "LOAD GAME", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);
                        foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                        {
                            new Invoker(menu: menu, name: saveInfo.FullDescription, closesMenu: true, taskName: Scheduler.TaskName.LoadGame, executeHelper: saveInfo.folderName,
                                 infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"| {saveInfo.AdditionalInfo}", imageList: new List<Texture2D> { saveInfo.AddInfoTexture }, color: Color.White, scale: 1f) }); // sound won't play here, because loading game stops all sounds
                        }
                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.Save:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "SAVE GAME", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        World world = World.GetTopWorld();

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
                                new Invoker(menu: menu, name: saveInfo.FullDescription, taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData, closesMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"| {saveInfo.AdditionalInfo}", imageList: new List<Texture2D> { saveInfo.AddInfoTexture }, color: Color.White, scale: 1f) });
                            }
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.CreateAnyPiece:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "CREATE ANY ITEM", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);
                        World world = World.GetTopWorld();
                        if (world == null) return menu;
                        var pieceCounterDict = new Dictionary<PieceTemplate.Name, int> { };

                        foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
                        { pieceCounterDict[pieceName] = 0; }

                        foreach (Sprite sprite in world.grid.GetAllSprites(Cell.Group.All))
                        { pieceCounterDict[sprite.boardPiece.name]++; }

                        foreach (PieceTemplate.Name pieceName in pieceCounterDict.Keys)
                        {
                            if (pieceName == PieceTemplate.Name.Player || pieceName == PieceTemplate.Name.PlayerGhost) continue; // creating these pieces would cause many glitches

                            var imageList = new List<Texture2D> { PieceInfo.GetTexture(pieceName) };
                            Dictionary<string, Object> createData = new Dictionary<string, Object> { { "position", world.player.sprite.position }, { "templateName", pieceName } };

                            new Invoker(menu: menu, name: $"{PieceInfo.GetInfo(pieceName).readableName} | ({pieceCounterDict[pieceName]})", imageList: imageList, taskName: Scheduler.TaskName.CreateDebugPieces, executeHelper: createData, rebuildsMenu: true);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Debug:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "DEBUG MENU", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        World world = World.GetTopWorld();

                        new Selector(menu: menu, name: "debug mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "DebugMode", rebuildsAllMenus: true);

                        if (world != null && !world.demoMode) new Invoker(menu: menu, name: "create any piece", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.CreateAnyPiece } });
                        new Invoker(menu: menu, name: "sound test", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.SoundTest } });
                        new Invoker(menu: menu, name: "graphics list", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.GfxListTest } });
                        new Selector(menu: menu, name: "save everywhere", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugSaveEverywhere", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "create missing pieces", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugCreateMissingPieces");
                        new Selector(menu: menu, name: "god mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "DebugGodMode", rebuildsMenu: true);
                        new Selector(menu: menu, name: "show whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "DebugShowWholeMap");
                        new Selector(menu: menu, name: "show sounds", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowSounds");
                        new Selector(menu: menu, name: "show all items on map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAllMapPieces");
                        new Selector(menu: menu, name: "show all recipes", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAllRecipes");
                        new Invoker(menu: menu, name: "restore all hints", taskName: Scheduler.TaskName.RestoreHints);
                        new Selector(menu: menu, name: "show fruit rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowFruitRects");
                        new Selector(menu: menu, name: "show sprite rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowRects");
                        new Selector(menu: menu, name: "show cells", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowCellData");
                        new Selector(menu: menu, name: "show animal targets", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAnimalTargets");
                        new Selector(menu: menu, name: "disable player panel", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugDisablePlayerPanel");
                        new Selector(menu: menu, name: "show all stat bars", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowStatBars");
                        new Selector(menu: menu, name: "show states", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowStates");
                        if (world != null)
                        {
                            new Invoker(menu: menu, name: "show crafted pieces", taskName: Scheduler.TaskName.ShowCraftStats, executeHelper: false);
                            new Invoker(menu: menu, name: "show used ingredients", taskName: Scheduler.TaskName.ShowCraftStats, executeHelper: true);
                            new Invoker(menu: menu, name: "check incorrect pieces", taskName: Scheduler.TaskName.CheckForIncorrectPieces);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        return menu;
                    }

                case Name.Shelter:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "SHELTER", blocksUpdatesBelow: true, canBeClosedManually: true, layout: Menu.Layout.Right, templateExecuteHelper: executeHelper, soundOpen: SoundData.Name.ClothRustle1, soundClose: SoundData.Name.ClothRustle2);

                        menu.bgColor = Color.DarkBlue * 0.7f;

                        new Invoker(menu: menu, name: "rest", closesMenu: true, taskName: Scheduler.TaskName.SleepInsideShelter, executeHelper: executeHelper, taskDelay: 15);
                        new Invoker(menu: menu, name: "save game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Save } });

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        menu.EntriesRectColor = Color.DarkBlue;
                        menu.EntriesTextColor = Color.White;
                        menu.EntriesOutlineColor = Color.DodgerBlue;

                        return menu;
                    }

                case Name.CraftField:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Field, label: "FIELD CRAFT", soundOpen: SoundData.Name.InventoryOpen);

                case Name.CraftEssential:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Essential, label: "ESSENTIAL WORKSHOP", soundOpen: SoundData.Name.ToolsMove);

                case Name.CraftBasic:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Basic, label: "BASIC WORKSHOP", soundOpen: SoundData.Name.ToolsMove);

                case Name.CraftAdvanced:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Advanced, label: "ADVANCED WORKSHOP", soundOpen: SoundData.Name.ToolsMove);

                case Name.CraftMaster:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Master, label: "MASTER WORKSHOP", soundOpen: SoundData.Name.ToolsMove);

                case Name.CraftAlchemy:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Alchemy, label: "ALCHEMY", soundOpen: SoundData.Name.BoilingPotion);

                case Name.CraftFurnace:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Furnace, label: "FURNACE", soundOpen: SoundData.Name.FireBurst);

                case Name.CraftAnvil:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.Anvil, label: "ANVIL", soundOpen: SoundData.Name.HammerHits);

                case Name.CraftLeatherBasic:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.LeatherBasic, label: "BASIC LEATHER WORKSHOP", soundOpen: SoundData.Name.LeatherMove);

                case Name.CraftLeatherAdvanced:
                    return CreateCraftMenu(templateName: templateName, category: Craft.Category.LeatherAdvanced, label: "ADVANCED LEATHER WORKSHOP", soundOpen: SoundData.Name.LeatherMove);

                case Name.Tutorials:
                    {
                        World world = World.GetTopWorld();
                        Menu menu = new Menu(templateName: templateName, name: "TUTORIALS", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        foreach (Tutorials.Tutorial tutorial in Tutorials.TutorialsInMenu)
                        {
                            if (world.hintEngine.shownTutorials.Contains(tutorial.type))
                            {
                                new Invoker(menu: menu, name: tutorial.name, taskName: Scheduler.TaskName.ShowTutorialInMenu, executeHelper: tutorial.type);
                            }
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        return menu;
                    }

                case Name.SoundTest:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "SOUND TEST", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper, soundNavigate: SoundData.Name.Empty);

                        foreach (SoundData.Name soundName in (SoundData.Name[])Enum.GetValues(typeof(SoundData.Name)))
                        {
                            if (soundName == SoundData.Name.Empty) continue;
                            new Invoker(menu: menu, name: soundName.ToString(), taskName: Scheduler.TaskName.Empty, sound: soundName);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        return menu;
                    }

                case Name.GfxListTest:
                    {
                        Menu menu = new Menu(templateName: templateName, name: "GRAPHICS LIST", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        foreach (AnimData.PkgName pkgName in (AnimData.PkgName[])Enum.GetValues(typeof(AnimData.PkgName)))
                        {
                            var textureByName = new Dictionary<object, object>();

                            foreach (var kvp in AnimData.frameListById)
                            {
                                if (!kvp.Key.StartsWith($"{pkgName}-")) continue;

                                int counter = 0;
                                foreach (AnimFrame frame in kvp.Value)
                                {
                                    textureByName[$"{kvp.Key} - {frame}"] = frame.texture;
                                    counter++;
                                }
                            }

                            if (textureByName.Count == 0) new Invoker(menu: menu, name: $"{pkgName} NO FRAMES", taskName: Scheduler.TaskName.Empty, playSound: false);
                            else if (textureByName.Count > 1) new Selector(menu: menu, name: pkgName.ToString(), valueDict: textureByName, targetObj: preferences, propertyName: "neededForMenus");
                            else new Invoker(menu: menu, name: $"{pkgName}  |", imageList: new List<Texture2D> { AnimData.framesForPkgs[pkgName].texture }, taskName: Scheduler.TaskName.Empty, playSound: false);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        return menu;
                    }

                default:
                    throw new DivideByZeroException($"Unsupported menu templateName - {templateName}.");
            }
        }

        private static Menu CreateCraftMenu(Name templateName, Craft.Category category, string label, SoundData.Name soundOpen)
        {
            World world = World.GetTopWorld();
            Player player = world.player;
            List<PieceStorage> storageList = player.CraftStorages;

            if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
            {
                new TextWindow(text: "I can't craft with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ShowTutorialOnTheField, closingTaskHelper: new Dictionary<string, Object> { { "tutorial", Tutorials.Type.KeepingAnimalsAway }, { "world", world }, { "ignoreDelay", true } }, animSound: world.DialogueSound);

                return null;
            }

            if (player.IsVeryTired)
            {
                new TextWindow(text: "I'm too tired to craft anything...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1, animSound: player.world.DialogueSound);
                return null;
            }

            Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Craft, world: World.GetTopWorld(), ignoreDelay: true);

            Menu menu = new Menu(templateName: templateName, name: label, blocksUpdatesBelow: true, canBeClosedManually: true, layout: SonOfRobinGame.platform == Platform.Mobile ? Menu.Layout.Right : Menu.Layout.Left, alwaysShowSelectedEntry: true, templateExecuteHelper: null, soundOpen: soundOpen);
            menu.bgColor = Color.LemonChiffon * 0.5f;

            var recipeList = Craft.GetRecipesForCategory(category: category, includeHidden: false, discoveredRecipes: world.discoveredRecipesForPieces);
            foreach (Craft.Recipe recipe in recipeList)
            {
                var craftParams = new Dictionary<string, object> { { "recipe", recipe }, { "craftOnTheGround", false } };

                new CraftInvoker(menu: menu, name: recipe.pieceToCreate.ToString(), closesMenu: false, taskName: Scheduler.TaskName.Craft, rebuildsMenu: true, executeHelper: craftParams, recipe: recipe, storageList: storageList);
            }

            menu.EntriesRectColor = Color.SaddleBrown;
            menu.EntriesTextColor = Color.PaleGoldenrod;
            menu.EntriesOutlineColor = Color.SaddleBrown;

            return menu;
        }

        private static void CreateControlsMappingEntries(Menu menu, bool gamepad, Dictionary<object, object> analogSticksDict, Dictionary<object, object> keysOrButtonsDict)
        {
            InputPackage newMapping = gamepad ? InputMapper.newMappingGamepad : InputMapper.newMappingKeyboard;

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "general");

            bool captureButtons = gamepad;
            bool captureKeys = !gamepad;

            if (gamepad)
            {
                foreach (string propertyName in new List<string> { "leftStick", "rightStick", })
                {
                    new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: analogSticksDict, targetObj: newMapping, propertyName: propertyName);
                }
            }
            else
            {
                foreach (string propertyName in new List<string> { "left", "right", "up", "down" })
                { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }
            }

            foreach (string propertyName in new List<string> { "confirm", "cancel" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "field");
            foreach (string propertyName in new List<string> { "interact", "useTool", "pickUp", "sprint", "zoomOut" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "menus");
            foreach (string propertyName in new List<string> { "pauseMenu", "craft", "equip", "inventory" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "inventory");
            foreach (string propertyName in new List<string> { "invSwitch", "invPickOne", "invPickStack", "invSort" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "toolbar");
            foreach (string propertyName in new List<string> { "toolbarPrev", "toolbarNext" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);

            var saveData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "openMenuIfNotValid", false } };
            new Invoker(menu: menu, name: "save", taskName: Scheduler.TaskName.SaveControls, executeHelper: saveData, rebuildsMenu: true, sound: SoundData.Name.Empty, playSound: false);
            var resetData = new Dictionary<string, Object> { { "gamepad", gamepad }, { "useDefault", true } };
            new Invoker(menu: menu, name: "reset", taskName: Scheduler.TaskName.ResetControls, executeHelper: resetData, rebuildsMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all controls to default", color: Color.White, scale: 1f) });

            new Separator(menu: menu, name: "", isEmpty: true);
            new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
        }

    }
}
