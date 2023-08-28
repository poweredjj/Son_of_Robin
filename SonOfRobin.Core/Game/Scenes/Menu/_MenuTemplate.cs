using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class MenuTemplate
    {
        public enum Name : byte
        {
            Main = 0,
            Options = 1,
            Sound = 2,
            Graphics = 3,
            Controls = 4,
            Gamepad = 5,
            Keyboard = 6,
            Scale = 7,
            OtherOptions = 8,
            CreateNewIsland = 9,
            RestartIsland = 35,
            SetSeed = 10,
            OpenIslandTemplate = 11,
            Pause = 12,
            Stats = 13,
            Load = 14,
            Save = 15,
            Tutorials = 16,
            GameOver = 17,
            Debug = 18,
            SoundTest = 19,
            GfxListTest = 20,
            Shelter = 21,
            CreateAnyPiece = 22,
            GenericConfirm = 23,
            CraftField = 24,
            CraftEssential = 25,
            CraftBasic = 26,
            CraftAdvanced = 27,
            CraftMaster = 28,
            CraftFurnace = 29,
            CraftAnvil = 30,
            CraftLeatherBasic = 31,
            CraftLeatherAdvanced = 32,
            ExportSave = 33,
            ImportSave = 34,
        }

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
                new Invoker(menu: menu, name: "no", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                new Invoker(menu: menu, name: "yes", closesMenu: true, taskName: Scheduler.TaskName.ProcessConfirmation, executeHelper: confirmationData);
            }

            return menu;
        }

        public static Menu CreateMenuFromTemplate(Name templateName, object executeHelper = null)
        {
            Preferences preferences = new();

            // Range example - valueList: Enumerable.Range(-1, 300).Cast<object>().ToList()

            switch (templateName)
            {
                case Name.Main:
                    {
                        Menu menu = new(templateName: templateName, name: "Son of Robin", blocksUpdatesBelow: false, canBeClosedManually: false, templateExecuteHelper: executeHelper);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "create new island", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.CreateNewIsland } },
                               infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "start new game", color: Color.White, scale: 1f) });
                        new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenMenuTemplate, new Dictionary<string, Object> { { "templateName", Name.Options } },
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "options, settings, etc.", color: Color.White, scale: 1f) });

                        var aboutTaskChain = new List<object>
                        {
                            new Scheduler.Task(taskName: Scheduler.TaskName.ShowTextWindow, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> {
                            { "text", $"Son of Robin {SonOfRobinGame.version.ToString().Replace(",", ".")}\nLast updated: {SonOfRobinGame.lastChanged:yyyy-MM-dd.}\n\nThis is a very early alpha version of the game.\nCode: Ahoy! Games.\nFastNoiseLite: Jordan Peck.\nSounds: | freesound.org.\nController icons: | Nicolae (Xelu) Berbece.\nStudies.Joystick: Luiz Ossinho.\nImageSharp library: Six Labors.\nVarious free graphics assets used." },
                            { "imageList", new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MusicNote), ButtonScheme.dpad } },
                            { "bgColor", new List<Byte> { 63, 167, 212 } }
                            }, storeForLaterUse: true),

                            new Scheduler.Task(taskName: Scheduler.TaskName.ShowTextWindow, turnOffInputUntilExecution: true, delay: 0, executeHelper: new Dictionary<string, Object> {
                            { "text", $"| Testers |\n\nFaye\nHellwoman\nGlonfindel\nRetro Marek" },
                            { "imageList", new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Star].texture, AnimData.framesForPkgs[AnimData.PkgName.Star].texture } },
                            { "bgColor", new List<Byte> { 85, 189, 113 } }
                            }, storeForLaterUse: true)
                        };

                        new Invoker(menu: menu, name: "about", taskName: Scheduler.TaskName.ExecuteTaskChain, executeHelper: aboutTaskChain,
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "information about the game", color: Color.White, scale: 1f) });

                        if (SonOfRobinGame.platform != Platform.Mobile) new Invoker(menu: menu, name: "quit game", closesMenu: true, taskName: Scheduler.TaskName.QuitGame);

                        return menu;
                    }

                case Name.Options:
                    {
                        Menu menu = new(templateName: templateName, name: "OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

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
                        Menu menu = new(templateName: templateName, name: "SCALE", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        var worldScaleList = new List<Object> { 0.75f, 1f, 1.25f, 1.5f, 2f };
                        if (Preferences.debugEnableExtremeZoomLevels || SonOfRobinGame.ThisIsWorkMachine || SonOfRobinGame.ThisIsHomeMachine)
                        {
                            worldScaleList.InsertRange(0, new List<Object> { 0.075f, 0.1f, 0.125f, 0.25f, 0.3f, 0.4f, 0.5f });
                            worldScaleList.AddRange(new List<Object> { 2.5f, 3f, 3.5f });
                        }
                        new Selector(menu: menu, name: "world scale", valueList: worldScaleList, targetObj: preferences, propertyName: "WorldScale");

                        new Selector(menu: menu, name: "global scale", valueList: new List<Object> { 1f, 1.5f, 2f }, targetObj: preferences, propertyName: "GlobalScale", rebuildsMenu: true, rebuildsMenuInstantScroll: true);
                        new Selector(menu: menu, name: "menu scale", valueList: new List<Object> { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f }, targetObj: preferences, propertyName: "menuScale", rebuildsMenu: true, rebuildsMenuInstantScroll: true);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.OtherOptions:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new(templateName: templateName, name: "OTHER OPTIONS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "export save", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.ExportSave } });

                        new Invoker(menu: menu, name: "import save", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.ImportSave } });

                        new Selector(menu: menu, name: "show hints", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showHints");
                        new Selector(menu: menu, name: "progress bar info", valueDict: new Dictionary<object, object> { { true, "detailed" }, { false, "simple" } }, targetObj: preferences, propertyName: "progressBarShowDetails");

                        new Selector(menu: menu, name: "map marker size", valueDict: Preferences.namesForMapMarkerScale, targetObj: preferences, propertyName: "mapMarkerScale");

                        new Selector(menu: menu, name: "use multiple CPU cores", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "useMultipleThreads", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "use all available CPU cores for some operations", color: Color.White, scale: 1f) });

                        if (SonOfRobinGame.os != OS.Windows) new Selector(menu: menu, name: "max map textures to load", valueDict: new Dictionary<object, object> { { 1000, "1000" }, { 2000, "2000" }, { 4000, "4000" }, { 8000, "8000" }, { 2147483647, "no limit" } }, targetObj: preferences, propertyName: "maxTexturesToLoad");

                        new Selector(menu: menu, name: "plants / animals processing time", valueDict: new Dictionary<object, object> { { 0.2f, "20%" }, { 0.3f, "30%" }, { 0.4f, "40%" }, { 0.5f, "50%" }, { 0.6f, "60%" }, { 0.7f, "70%" }, { 0.8f, "80%" }, { 0.9f, "90%" }, { 0.95f, "95%" } }, targetObj: preferences, propertyName: "StateMachinesDurationFramePercent", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "max time used to process\nanimals and plants for each frame", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "show demo world", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showDemoWorld");
                        new Invoker(menu: menu, name: "delete incompatible saves", taskName: Scheduler.TaskName.DeleteIncompatibleSaves);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Graphics:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new(templateName: templateName, name: "GRAPHICS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Selector(menu: menu, name: "FPS counter", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "ShowFpsCounter", rebuildsMenu: true);

                        if (Preferences.ShowFpsCounter)
                        {
                            new Selector(menu: menu, name: "FPS counter position", valueDict: new Dictionary<object, object> { { true, "right" }, { false, "left" } }, targetObj: preferences, propertyName: "FpsCounterPosRight");
                            new Selector(menu: menu, name: "FPS graph", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "FpsCounterShowGraph", rebuildsMenu: true);

                            if (Preferences.FpsCounterShowGraph) new Selector(menu: menu, name: "FPS graph length", valueDict: new Dictionary<object, object> { { 70, "short" }, { 120, "medium" }, { 250, "long" } }, targetObj: preferences, propertyName: "FpsCounterGraphLength");
                        }

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            new Selector(menu: menu, name: "fullscreen mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "FullScreenMode", rebuildsMenu: true);
                            if (Preferences.FullScreenMode) new Selector(menu: menu, name: "resolution", valueList: Preferences.AvailableScreenModes, targetObj: preferences, propertyName: "FullScreenResolution");

                            new Selector(menu: menu, name: "cut framerate in half", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "HalfFramerate", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "limit framerate", color: Color.White, scale: 1f) }, rebuildsMenu: true);
                        }

                        new Selector(menu: menu, name: "frameskip", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "FrameSkip", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "skip frames to maintain speed", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "shadows (sun)", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "drawSunShadows");

                        new Selector(menu: menu, name: "darkness resolution", valueDict: Preferences.namesForDarknessRes, targetObj: preferences, propertyName: "DarknessResolution");
                        new Selector(menu: menu, name: "shadows (light sourced)", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "drawShadows", rebuildsMenu: true);

                        new Selector(menu: menu, name: "particles", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showParticles", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "show small debris when hitting objects", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "max flame lights", valueDict: new Dictionary<Object, Object> { { 0, "zero" }, { 1, "few" }, { 2, "some" }, { 3, "many" }, { 5, "too many" }, { 9999, "unlimited" } }, targetObj: preferences, propertyName: "maxFlameLightsPerCell");

                        new Selector(menu: menu, name: "high quality water", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "highQualityWater", infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "show water animations", color: Color.White, scale: 1f) });

                        new Selector(menu: menu, name: "plants sway", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "plantsSway");

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Sound:
                    {
                        Menu menu = new(templateName: templateName, name: "SOUND", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        Sound sound = new(SoundData.Name.Empty); // sound name doesn't really matter

                        new Selector(menu: menu, name: "sound", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: sound, propertyName: "GlobalOn", rebuildsMenu: true);

                        if (Sound.GlobalOn)
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
                        Menu menu = new(templateName: templateName, name: "CONTROLS", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Invoker(menu: menu, name: "configure gamepad", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Gamepad } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all gamepad controls", color: Color.White, scale: 1f) });
                        new Invoker(menu: menu, name: "configure keyboard", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Keyboard } }, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "set all keyboard controls", color: Color.White, scale: 1f) });

                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (Input.currentControlType != Input.ControlType.Touch || SonOfRobinGame.platform != Platform.Mobile) new Selector(menu: menu, name: "on screen buttons", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "EnableTouchButtons", rebuildsMenu: true);

                        if (Input.currentControlType != Input.ControlType.Touch) new Selector(menu: menu, name: "swap mouse buttons", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "swapMouseButtons");

                        if (Preferences.EnableTouchButtons)
                        {
                            new Selector(menu: menu, name: "on screen joysticks", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "enableTouchJoysticks");
                        }

                        new Selector(menu: menu, name: SonOfRobinGame.platform == Platform.Mobile ? "touch to walk" : "point to walk", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "PointToWalk", rebuildsMenu: true);
                        if (Preferences.PointToWalk) new Selector(menu: menu, name: SonOfRobinGame.platform == Platform.Mobile ? "touch to interact" : "point to interact", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "PointToInteract");

                        new Selector(menu: menu, name: "always run", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "alwaysRun");

                        new Selector(menu: menu, name: "gamepad type", valueDict: new Dictionary<object, object> { { ButtonScheme.Type.Xbox360, "Xbox 360" }, { ButtonScheme.Type.XboxSeries, "Xbox Series" }, { ButtonScheme.Type.DualShock4, "Dual Shock 4" }, { ButtonScheme.Type.DualSense, "Dual Sense" }, { ButtonScheme.Type.SwitchProController, "Switch Pro Controller" } }, targetObj: preferences, propertyName: "ControlTipsScheme");

                        new Selector(menu: menu, name: "rumble", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "rumbleEnabled");

                        new Selector(menu: menu, name: "show control tips (general)", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "ShowControlTips");

                        new Selector(menu: menu, name: "show control tips (field)", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "showFieldControlTips", rebuildsMenu: true);

                        if (Preferences.showFieldControlTips) new Selector(menu: menu, name: "field tips scale", valueDict: Preferences.namesForFieldControlTipsScale, targetObj: preferences, propertyName: "fieldControlTipsScale");

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Gamepad:
                    {
                        bool gamepad = true;
                        Menu menu = new(templateName: templateName, name: "CONFIGURE GAMEPAD", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.CheckForNonSavedControls, closingTaskHelper: gamepad, templateExecuteHelper: executeHelper);

                        var allButtonsList = new List<Buttons> { Buttons.A, Buttons.B, Buttons.X, Buttons.Y, Buttons.LeftShoulder, Buttons.RightShoulder, Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.Start, Buttons.Back, Buttons.LeftStick, Buttons.RightStick, Buttons.DPadLeft, Buttons.DPadRight, Buttons.DPadUp, Buttons.DPadDown };

                        var allButtonsDict = new Dictionary<object, object>();

                        foreach (Buttons button in allButtonsList)
                        { allButtonsDict[new StoredInput(button)] = InputVis.GetTexture(button); }

                        var analogSticksList = new List<StoredInput> { new StoredInput(InputMapper.AnalogType.PadLeft), new StoredInput(InputMapper.AnalogType.PadRight) };
                        var analogSticksDict = new Dictionary<object, object>();
                        foreach (StoredInput storedInput in analogSticksList)
                        { analogSticksDict[storedInput] = InputVis.GetTexture(storedInput); }

                        CreateControlsMappingEntries(menu: menu, gamepad: true, analogSticksDict: analogSticksDict, keysOrButtonsDict: allButtonsDict);

                        return menu;
                    }

                case Name.Keyboard:
                    {
                        bool gamepad = false;
                        Menu menu = new(templateName: templateName, name: "CONFIGURE KEYBOARD", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.CheckForNonSavedControls, closingTaskHelper: gamepad, templateExecuteHelper: executeHelper);

                        Dictionary<object, object> allKeysDict = KeyboardScheme.KeyTextures.ToDictionary(k => (object)new StoredInput(k.Key), k => (object)k.Value);
                        CreateControlsMappingEntries(menu: menu, gamepad: false, analogSticksDict: null, keysOrButtonsDict: allKeysDict);

                        return menu;
                    }

                case Name.CreateNewIsland:
                    {
                        Menu menu = new(templateName: templateName, name: "CREATE NEW ISLAND", blocksUpdatesBelow: false, canBeClosedManually: true, closingTask: Scheduler.TaskName.SavePrefs, templateExecuteHelper: executeHelper);

                        new Invoker(menu: menu, name: "start game", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld, sound: SoundData.Name.NewGameStart);
                        new Invoker(menu: menu, name: "reset settings", closesMenu: false, taskName: Scheduler.TaskName.ResetNewWorldSettings, rebuildsMenu: true);
                        new Separator(menu: menu, name: "", isEmpty: true);

                        CreateCharacterSelection(menu);

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Selector(menu: menu, name: "customize", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "CustomizeWorld", rebuildsMenu: true);

                        if (Preferences.CustomizeWorld)
                        {
                            List<Object> sizeList = new() { 10000, 15000, 20000, 25000, 30000, 40000 };
                            if (SonOfRobinGame.platform == Platform.Desktop) sizeList.AddRange(new List<Object> { 50000, 60000 });

                            if (SonOfRobinGame.ThisIsWorkMachine || SonOfRobinGame.ThisIsHomeMachine || Preferences.debugEnableExtremeMapSizes)
                            {
                                sizeList.InsertRange(0, new List<Object> { 1000, 2000, 4000, 8000 });
                                sizeList.AddRange(new List<Object> { 70000, 80000, 90000, 100000, 150000, 200000 });
                            }

                            new Selector(menu: menu, name: "size", valueList: sizeList, targetObj: preferences, propertyName: "newWorldSize");
                            new Selector(menu: menu, name: "terrain detail", valueDict: Preferences.namesForResDividers, targetObj: preferences, propertyName: "newWorldResDivider");

                            new Selector(menu: menu, name: "random seed", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "randomSeed", rebuildsMenu: true);

                            if (!Preferences.randomSeed) new Invoker(menu: menu, name: "set seed", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.SetSeed } });
                        }
                        else
                        {
                            List<Object> sizeList = new() { Preferences.WorldSize.small, Preferences.WorldSize.medium, Preferences.WorldSize.large };
                            if (SonOfRobinGame.platform == Platform.Desktop)
                            {
                                sizeList.Add(Preferences.WorldSize.gigantic);
                            }

                            new Selector(menu: menu, name: "size", valueList: sizeList, targetObj: preferences, propertyName: "SelectedWorldSize", rebuildsMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"{Preferences.newWorldSize}x{Preferences.newWorldSize}", color: Color.White, scale: 1f) });
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);

                        if (GridTemplate.CorrectNonDemoTemplatesOfProperSizeExist)
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
                        Menu menu = new(templateName: templateName, name: "ENTER SEED", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);
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
                        Menu menu = new(templateName: templateName, name: "CREATE ISLAND FROM TEMPLATE", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        CreateCharacterSelection(menu);

                        new Separator(menu: menu, name: "", isEmpty: true);

                        foreach (GridTemplate gridTemplate in GridTemplate.CorrectNonDemoTemplatesOfProperSize)
                        {
                            string detailLevelName = Preferences.namesForResDividers.ContainsKey(gridTemplate.resDivider) ? (string)Preferences.namesForResDividers[gridTemplate.resDivider] : $"{gridTemplate.resDivider}";

                            new Invoker(menu: menu, name: $"{gridTemplate.width}x{gridTemplate.height}  seed  {String.Format("{0:0000}", gridTemplate.seed)}  detail {detailLevelName}", closesMenu: true, taskName: Scheduler.TaskName.CreateNewWorld, executeHelper: new Dictionary<string, Object> { { "width", gridTemplate.width }, { "height", gridTemplate.height }, { "seed", gridTemplate.seed }, { "resDivider", gridTemplate.resDivider }, { "playerName", Preferences.newWorldPlayerName }, { "startingItem", Preferences.newWorldStartingItem } }, sound: SoundData.Name.NewGameStart);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.RestartIsland:
                    {
                        Menu menu = new(templateName: templateName, name: "RESTART ISLAND", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        CreateCharacterSelection(menu);

                        new Invoker(menu: menu, name: "restart now", taskName: Scheduler.TaskName.RestartIsland, executeHelper: World.GetTopWorld());

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);
                        return menu;
                    }

                case Name.Pause:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new(templateName: templateName, name: "PAUSE", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper, soundOpen: SoundData.Name.PaperMove1, soundClose: SoundData.Name.PaperMove2);
                        new Invoker(menu: menu, name: "return to game", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        if ((Preferences.debugSaveEverywhere || Preferences.DebugMode) && !world.SpectatorMode && world.Player?.activeState == BoardPiece.State.PlayerControlledWalking && world.Player.alive) new Invoker(menu: menu, name: "save game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Save } });

                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });

                        new Invoker(menu: menu, name: "options", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Options } });

                        new Invoker(menu: menu, name: "stats", taskName: Scheduler.TaskName.OpenMenuTemplate, new Dictionary<string, Object> { { "templateName", Name.Stats } },
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "statistics, levels, etc.", color: Color.White, scale: 1f) });

                        if (world.HintEngine.shownTutorials.Count > 0) new Invoker(menu: menu, name: "tutorials", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Tutorials } });

                        new Invoker(menu: menu, name: "restart this island", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.RestartIsland } });

                        var returnConfirmationData = new Dictionary<string, Object> { { "question", "Do you really want to exit? You will lose unsaved progress." }, { "taskName", Scheduler.TaskName.ReturnToMainMenu }, { "executeHelper", null } };
                        new Invoker(menu: menu, name: "return to main menu", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: returnConfirmationData);

                        if (SonOfRobinGame.platform != Platform.Mobile)
                        {
                            var quitConfirmationData = new Dictionary<string, Object> { { "question", "Do you really want to quit? You will lose unsaved progress." }, { "taskName", Scheduler.TaskName.QuitGame }, { "executeHelper", null } };
                            new Invoker(menu: menu, name: "quit game", taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: quitConfirmationData);
                        }
                        return menu;
                    }

                case Name.Stats:
                    {
                        World world = World.GetTopWorld();

                        Menu menu = new(templateName: templateName, name: "STATS", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper, soundClose: SoundData.Name.PaperMove2, alwaysShowSelectedEntry: true, soundNavigate: SoundData.Name.Tick, soundInvoke: SoundData.Name.Tick)
                        {
                            bgColor = new Color(75, 37, 110) * 0.75f
                        };

                        Player player = world.Player;

                        new Invoker(menu: menu, name: "return to game", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        // player stats
                        {
                            var textLines = new List<string>();
                            var imageList = new List<Texture2D>();

                            textLines.Add("| Player stats\n");
                            imageList.Add(PieceInfo.GetTexture(player.name));

                            textLines.Add($"| Strength: {player.strength}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.AxeIron].texture);

                            textLines.Add($"| Speed: {Math.Round(player.speed)}");
                            imageList.Add(TextureBank.GetTexture(textureName: TextureBank.TextureName.Animal));

                            textLines.Add($"| HP: {Math.Round(player.HitPoints)} / {Math.Round(player.maxHitPoints)}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Heart].texture);

                            textLines.Add($"| Fatigue: {Math.Round(player.Fatigue)} / {Math.Round(player.maxFatigue)}");
                            imageList.Add(TextureBank.GetTexture(TextureBank.TextureName.Bed));

                            textLines.Add($"| Food: {Math.Round(player.FedPercent * 100)}%");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Burger].texture);

                            textLines.Add($"| Inventory size: {player.InvWidth}x{player.InvHeight}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.BackpackMedium].texture);

                            string toolbarText = $"| Toolbar size: {player.ToolbarWidth}";
                            if (player.ToolbarHeight > 1) toolbarText += $"x{player.ToolbarHeight}";
                            textLines.Add(toolbarText);
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.BeltSmall].texture);

                            textLines.Add($"| Craft level: {player.CraftLevel}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.WorkshopAdvanced].texture);

                            if (world.Player.ResourcefulCrafter)
                            {
                                textLines.Add($"| Resourceful crafter |");
                                imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.WorkshopAdvanced].texture);
                                imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Star].texture);
                            }

                            textLines.Add($"| Cooking level: {player.CookLevel}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.MealStandard].texture);

                            textLines.Add($"| Brewing level: {player.BrewLevel}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture);

                            textLines.Add($"| Meat harvesting level: {player.HarvestLevel}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.MeatRawPrime].texture);

                            var infoTextList = new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList, color: Color.White, scale: 1f) };

                            var positiveBuffTextLines = new List<string>();
                            var negativeBuffTextLines = new List<string>();

                            foreach (Buff buff in player.buffEngine.BuffList)
                            {
                                if (buff.statMenuText != null)
                                {
                                    if (buff.isPositive) positiveBuffTextLines.Add(buff.statMenuText);
                                    else negativeBuffTextLines.Add(buff.statMenuText);
                                }
                            }

                            if (positiveBuffTextLines.Count > 0)
                            {
                                infoTextList.Add(new InfoWindow.TextEntry(text: String.Join("\n", positiveBuffTextLines), color: Color.Cyan, scale: 0.75f));
                            }

                            if (negativeBuffTextLines.Count > 0)
                            {
                                infoTextList.Add(new InfoWindow.TextEntry(text: String.Join("\n", negativeBuffTextLines), color: new Color(255, 120, 70), scale: 0.75f));
                            }

                            Invoker invoker = new(menu: menu, name: "player", taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);
                            Color color = new(31, 197, 219);
                            invoker.rectColor = color;
                            invoker.outlineColor = color;
                        }

                        // island info
                        {
                            int plantCount = world.pieceCountByClass.ContainsKey(typeof(Plant)) ? world.pieceCountByClass[typeof(Plant)] : 0;
                            int animalCount = world.pieceCountByClass.ContainsKey(typeof(Animal)) ? world.pieceCountByClass[typeof(Animal)] : 0;

                            var textLines = new List<string>();
                            var imageList = new List<Texture2D>();

                            textLines.Add("Island info\n");

                            textLines.Add($"| Size: {world.width}x{world.height}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.Map].texture);

                            textLines.Add($"All Objects: {world.PieceCount}");

                            textLines.Add($"| Plants: {plantCount}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.GrassRegular].texture);

                            textLines.Add($"| Animals: {animalCount}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.FoxGinger].texture);

                            string timePlayedString = string.Format("{0:D2}:{1:D2}", (int)Math.Floor(world.TimePlayed.TotalHours), world.TimePlayed.Minutes);
                            textLines.Add($"Time played: {timePlayedString}");
                            textLines.Add($"Distance walked: {player.DistanceWalkedKilometers} km");
                            textLines.Add($"Map discovered: {Math.Round(world.Grid.VisitedCellsPercentage * 100, 1)}%");
                            textLines.Add($"Locations found: {world.Grid.namedLocations.DiscoveredLocationsCount}/{world.Grid.namedLocations.AllLocationsCount}");
                            textLines.Add($"Island day: {world.islandClock.CurrentDayNo}");

                            var infoTextList = new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList, color: Color.White, scale: 1f) };

                            Invoker invoker = new(menu: menu, name: "island", taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);
                            Color color = new(148, 115, 55);
                            invoker.rectColor = color;
                            invoker.outlineColor = color;
                        }

                        // general craft stats
                        {
                            var textLines = new List<string>();
                            var imageList = new List<Texture2D>();

                            textLines.Add("| General craft stats\n");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.WorkshopAdvanced].texture);

                            textLines.Add($"|  Items crafted: {world.craftStats.CraftedPiecesTotal}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.AxeIron].texture);

                            textLines.Add($"|  Ingredients used: {world.craftStats.UsedIngredientsTotal}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.WoodLogRegular].texture);

                            textLines.Add($"|  Ingredients saved: {world.craftStats.SmartCraftingReducedIngredientCount}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.ChestIron].texture);

                            var infoTextList = new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList, color: Color.White, scale: 1f) };

                            Invoker invoker = new(menu: menu, name: "craft general", taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);
                            Color color = new(168, 74, 145);
                            invoker.rectColor = color;
                            invoker.outlineColor = color;
                        }

                        // crafting / planting lists

                        world.craftStats.CreateMenuEntriesForCraftedPiecesSummary(menu);
                        world.craftStats.CreateMenuEntriesForUsedIngredientsSummary(menu);
                        world.craftStats.CreateMenuEntriesForVegetationPlantedSummary(menu);

                        // cooking stats
                        {
                            var textLines = new List<string>();
                            var imageList = new List<Texture2D>();

                            textLines.Add("| Cooking stats\n");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.CookingPot].texture);

                            textLines.Add($"| Meals made: {world.cookStats.TotalCookCount}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.MealStandard].texture);

                            textLines.Add($"\n| Ingredients used:");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.HerbsCyan].texture);

                            textLines.Add($"Unique types: {world.cookStats.IngredientNamesCount}");
                            textLines.Add($"Total: {world.cookStats.AllIngredientsCount}");

                            var infoTextList = new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList, color: Color.White, scale: 1f) };

                            Invoker invoker = new(menu: menu, name: "cooking stats", taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);
                            Color color = new(181, 22, 83);
                            invoker.rectColor = color;
                            invoker.outlineColor = color;
                        }

                        // brewing stats
                        {
                            var textLines = new List<string>();
                            var imageList = new List<Texture2D>();

                            textLines.Add("| Potion brewing stats\n");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.AlchemyLabStandard].texture);

                            textLines.Add($"| Potions made: {world.brewStats.TotalCookCount}");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.PotionRed].texture);

                            textLines.Add($"\n| Ingredients used:");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.HerbsCyan].texture);

                            textLines.Add($"Unique types: {world.brewStats.IngredientNamesCount}");
                            textLines.Add($"Total: {world.brewStats.AllIngredientsCount}");
                            textLines.Add($"Bases: {world.brewStats.BaseCount}");
                            textLines.Add($"Boosters: {world.brewStats.BoosterCount}");

                            var infoTextList = new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList, color: Color.White, scale: 1f) };

                            Invoker invoker = new(menu: menu, name: "potion brewing stats", taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);
                            Color color = new(102, 212, 157);
                            invoker.rectColor = color;
                            invoker.outlineColor = color;
                        }

                        // harvesting stats
                        {
                            var textLines = new List<string>();
                            var imageList = new List<Texture2D>();

                            textLines.Add("| Meat harvesting stats");
                            imageList.Add(AnimData.framesForPkgs[AnimData.PkgName.MeatRawPrime].texture);

                            textLines.Add("\nProcessed:");
                            foreach (var kvp in world.meatHarvestStats.HarvestedAnimalCountByName)
                            {
                                PieceTemplate.Name animalName = kvp.Key;
                                int harvestCount = kvp.Value;
                                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(animalName);

                                textLines.Add($"|  x{harvestCount} {pieceInfo.readableName}");
                                imageList.Add(pieceInfo.texture);
                            }

                            textLines.Add("\nObtained:");
                            var obtainedBonusPieceCountByName = world.meatHarvestStats.ObtainedBonusPieceCountByName;
                            foreach (var kvp in world.meatHarvestStats.ObtainedBasePieceCountByName)
                            {
                                PieceTemplate.Name animalName = kvp.Key;
                                int harvestCount = kvp.Value;
                                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(animalName);

                                int bonusCount = obtainedBonusPieceCountByName.ContainsKey(animalName) ? obtainedBonusPieceCountByName[animalName] : 0;

                                string basePlusBonusText = obtainedBonusPieceCountByName.ContainsKey(animalName) ? $" ({harvestCount}+{bonusCount}) " : "";

                                textLines.Add($"|  x{harvestCount + bonusCount}{basePlusBonusText} {pieceInfo.readableName}");
                                imageList.Add(pieceInfo.texture);
                            }

                            textLines.Add($"\nTotal animals processed: {world.meatHarvestStats.TotalHarvestCount}");
                            textLines.Add($"Total items obtained: {world.meatHarvestStats.ObtainedTotalPieceCount} ({world.meatHarvestStats.ObtainedBasePieceCount}+{world.meatHarvestStats.ObtainedBonusPieceCount})");

                            var infoTextList = new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: String.Join("\n", textLines), imageList: imageList, color: Color.White, scale: 1f) };

                            Invoker invoker = new(menu: menu, name: "meat harvesting stats", taskName: Scheduler.TaskName.Empty, infoTextList: infoTextList);
                            Color color = new(245, 140, 245);
                            invoker.rectColor = color;
                            invoker.outlineColor = color;
                        }

                        return menu;
                    }

                case Name.GameOver:
                    {
                        Menu menu = new(templateName: templateName, name: "GAME OVER", blocksUpdatesBelow: false, canBeClosedManually: false, layout: Menu.Layout.Middle, templateExecuteHelper: executeHelper);
                        if (SaveHeaderManager.AnySavesExist) new Invoker(menu: menu, name: "load game", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.Load } });
                        new Invoker(menu: menu, name: "enter spectator mode", closesMenu: true, taskName: Scheduler.TaskName.SetSpectatorMode, executeHelper: true);
                        new Invoker(menu: menu, name: "restart this island", closesMenu: true, taskName: Scheduler.TaskName.RestartIsland, executeHelper: World.GetTopWorld());
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
                        Menu menu = new(templateName: templateName, name: "LOAD GAME", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);
                        foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                        {
                            Scheduler.TaskName taskName;
                            Object saveExecuteHelper;
                            bool playSound;
                            SoundData.Name soundName;
                            bool closeMenu;

                            if (saveInfo.saveIsObsolete)
                            {
                                taskName = Scheduler.TaskName.ShowTextWindow;
                                saveExecuteHelper = new Dictionary<string, Object> {
                                    { "text", $"| This save version ({saveInfo.saveVersion}) is not compatible with current save version ({SaveHeaderManager.saveVersion})." },
                                    { "imageList", new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.BubbleExclamationRed].texture } },
                                    { "bgColor", new List<Byte> { 200, 0, 0 } },
                                    { "animate", false },
                                    };
                                playSound = true;
                                closeMenu = false;
                                soundName = SoundData.Name.Error;
                            }
                            else
                            {
                                taskName = Scheduler.TaskName.LoadGame;
                                saveExecuteHelper = saveInfo.folderName;
                                playSound = false;
                                closeMenu = false;
                                soundName = SoundData.Name.Empty;
                            }

                            Invoker loadInvoker = new Invoker(menu: menu, name: saveInfo.FullDescription, closesMenu: closeMenu, taskName: taskName, playSound: playSound, sound: soundName, executeHelper: saveExecuteHelper,
                                   infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"| {saveInfo.AdditionalInfo}", imageList: new List<Texture2D> { saveInfo.AddInfoTexture }, color: Color.White, scale: 1f) }); // sound won't play here, because loading game stops all sounds

                            if (saveInfo.saveIsObsolete || saveInfo.saveIsCorrupted)
                            {
                                loadInvoker.rectColor = Color.DarkRed;
                                loadInvoker.textColor = new Color(255, 116, 82);
                            }
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.Save:
                    {
                        Menu menu = new(templateName: templateName, name: "SAVE GAME", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        World world = World.GetTopWorld();

                        var saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", SaveHeaderManager.NewSaveSlotName }, { "showMessage", true } };
                        new Invoker(menu: menu, name: "new save", taskName: Scheduler.TaskName.SaveGame, executeHelper: saveParams, rebuildsMenu: true,
                            infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: "make new save", color: Color.White, scale: 1f) });
                        new Separator(menu: menu, name: "", isEmpty: true);

                        foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                        {
                            saveParams = new Dictionary<string, Object> { { "world", world }, { "saveSlotName", saveInfo.folderName }, { "showMessage", true } };
                            var confirmationData = new Dictionary<string, Object> { { "question", "The save will be overwritten. Continue?" }, { "taskName", Scheduler.TaskName.SaveGame }, { "executeHelper", saveParams } };

                            new Invoker(menu: menu, name: saveInfo.FullDescription, taskName: Scheduler.TaskName.OpenConfirmationMenu, executeHelper: confirmationData, closesMenu: true, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"| {saveInfo.AdditionalInfo}", imageList: new List<Texture2D> { saveInfo.AddInfoTexture }, color: Color.White, scale: 1f) });
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.ExportSave:
                    {
                        Menu menu = new(templateName: templateName, name: "EXPORT SAVE", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        foreach (SaveHeaderInfo saveInfo in SaveHeaderManager.CorrectSaves)
                        {
                            new Invoker(menu: menu, name: saveInfo.FullDescription, taskName: Scheduler.TaskName.ExportSave, executeHelper: saveInfo.folderName, infoTextList: new List<InfoWindow.TextEntry> { new InfoWindow.TextEntry(text: $"| {saveInfo.AdditionalInfo}", imageList: new List<Texture2D> { saveInfo.AddInfoTexture }, color: Color.White, scale: 1f) });
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.ImportSave:
                    {
                        Menu menu = new(templateName: templateName, name: "IMPORT SAVE", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        string importPath = SonOfRobinGame.downloadsPath;
                        if (!Directory.Exists(importPath)) importPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                        var saveZipPaths = Directory.GetFiles(importPath)
                            .Where(file => file.ToLower().Contains("son_of_robin_save_") &&
                            (file.EndsWith(".zip") || file.EndsWith(".jpg"))); // workaround - media files are always shown on mobile

                        if (saveZipPaths.Count() > 0)
                        {
                            foreach (string saveZipFile in saveZipPaths)
                            {
                                new Invoker(menu: menu, name: Path.GetFileName(Path.GetFileNameWithoutExtension(saveZipFile)), taskName: Scheduler.TaskName.ImportSave, executeHelper: saveZipFile);
                            }
                        }
                        else
                        {
                            Separator separator = new Separator(menu: menu, name: "no saves to import");
                            separator.rectColor = Color.DarkRed;
                            separator.textColor = new Color(255, 116, 82);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);
                        return menu;
                    }

                case Name.CreateAnyPiece:
                    {
                        Menu menu = new(templateName: templateName, name: "CREATE ANY ITEM", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);
                        World world = World.GetTopWorld();
                        if (world == null) return menu;
                        var pieceCounterDict = new Dictionary<PieceTemplate.Name, int> { };

                        foreach (PieceTemplate.Name pieceName in PieceTemplate.allNames)
                        { pieceCounterDict[pieceName] = 0; }

                        foreach (Sprite sprite in world.Grid.GetSpritesFromAllCells(Cell.Group.All))
                        { pieceCounterDict[sprite.boardPiece.name]++; }

                        foreach (PieceTemplate.Name pieceName in pieceCounterDict.Keys)
                        {
                            if (PieceInfo.GetInfo(pieceName).type == typeof(Player)) continue; // creating these pieces would cause many glitches

                            var imageList = new List<Texture2D> { PieceInfo.GetTexture(pieceName) };
                            Dictionary<string, Object> createData = new() { { "position", world.Player.sprite.position }, { "templateName", pieceName } };

                            new Invoker(menu: menu, name: $"{PieceInfo.GetInfo(pieceName).readableName} | ({pieceCounterDict[pieceName]})", imageList: imageList, taskName: Scheduler.TaskName.CreateDebugPieces, executeHelper: createData, rebuildsMenu: true);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.SavePrefs);

                        return menu;
                    }

                case Name.Debug:
                    {
                        Menu menu = new(templateName: templateName, name: "DEBUG MENU", blocksUpdatesBelow: false, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        World world = World.GetTopWorld();
                        bool nonDemoWorldActive = world != null && !world.demoMode;

                        new Selector(menu: menu, name: "debug mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "DebugMode", rebuildsAllMenus: true);

                        if (nonDemoWorldActive) new Invoker(menu: menu, name: "create any piece", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.CreateAnyPiece } });
                        new Invoker(menu: menu, name: "sound test", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.SoundTest } });
                        new Invoker(menu: menu, name: "graphics list", taskName: Scheduler.TaskName.OpenMenuTemplate, executeHelper: new Dictionary<string, Object> { { "templateName", Name.GfxListTest } });
                        new Selector(menu: menu, name: "save everywhere", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugSaveEverywhere", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "show all recipes", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAllRecipes");
                        new Selector(menu: menu, name: "show plant growth", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowPlantGrowthInCamera");
                        new Selector(menu: menu, name: "show anim size change", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAnimSizeChangeInCamera");
                        new Selector(menu: menu, name: "fast plant growth", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugFastPlantGrowth");
                        new Selector(menu: menu, name: "show named location areas", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowNamedLocationAreas");
                        if (world != null) new Invoker(menu: menu, name: "discover all locations", taskName: Scheduler.TaskName.SetAllNamedLocationsAsDiscovered, executeHelper: world);
                        new Selector(menu: menu, name: "create missing pieces", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugCreateMissingPieces");
                        if (SonOfRobinGame.platform != Platform.Mobile) new Selector(menu: menu, name: "vertical sync", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "VSync", rebuildsMenu: true);
                        new Selector(menu: menu, name: "enable test characters", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugEnableTestCharacters", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "enable extreme zoom levels", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugEnableExtremeZoomLevels", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "enable extreme map sizes", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugEnableExtremeMapSizes", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "instant cooking and brewing", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugInstantCookBrew", rebuildsAllMenus: true);
                        new Selector(menu: menu, name: "god mode", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "DebugGodMode", rebuildsMenu: true);
                        new Selector(menu: menu, name: "show whole map", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "DebugShowWholeMap");
                        new Selector(menu: menu, name: "allow fullscreen map animation", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugAllowMapAnimation");
                        new Selector(menu: menu, name: "show sounds", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowSounds");
                        new Selector(menu: menu, name: "show all tutorials in menu", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAllTutorials");
                        new Invoker(menu: menu, name: "restore all hints", taskName: Scheduler.TaskName.RestoreHints);
                        new Selector(menu: menu, name: "show fruit rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowFruitRects");
                        new Selector(menu: menu, name: "show sprite rects", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowRects");
                        new Selector(menu: menu, name: "show piece data", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowPieceData");
                        new Selector(menu: menu, name: "show animal targets", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowAnimalTargets");
                        new Selector(menu: menu, name: "disable player panel", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugDisablePlayerPanel");
                        new Selector(menu: menu, name: "show states", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowStates");
                        new Selector(menu: menu, name: "show cells", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowCellData");
                        new Selector(menu: menu, name: "show focus rect", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowFocusRect");
                        new Selector(menu: menu, name: "show outside camera", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowOutsideCamera");

                        new Selector(menu: menu, name: "show all stat bars", valueDict: new Dictionary<object, object> { { true, "on" }, { false, "off" } }, targetObj: preferences, propertyName: "debugShowStatBars");

                        if (nonDemoWorldActive)
                        {
                            new Invoker(menu: menu, name: "check incorrect pieces", taskName: Scheduler.TaskName.CheckForIncorrectPieces);
                        }

                        new Separator(menu: menu, name: "", isEmpty: true);
                        new Invoker(menu: menu, name: "return", closesMenu: true, taskName: Scheduler.TaskName.Empty);

                        return menu;
                    }

                case Name.Shelter:
                    {
                        Menu menu = new(templateName: templateName, name: "SHELTER", blocksUpdatesBelow: true, canBeClosedManually: true, layout: Menu.Layout.Right, templateExecuteHelper: executeHelper, soundOpen: SoundData.Name.ClothRustle1, soundClose: SoundData.Name.ClothRustle2)
                        {
                            bgColor = Color.DarkBlue * 0.7f
                        };

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
                        Menu menu = new(templateName: templateName, name: "TUTORIALS", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper);

                        foreach (Tutorials.Tutorial tutorial in Tutorials.TutorialsInMenu)
                        {
                            if (world.HintEngine.shownTutorials.Contains(tutorial.type) || Preferences.debugShowAllTutorials)
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
                        Menu menu = new(templateName: templateName, name: "SOUND TEST", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper, soundNavigate: SoundData.Name.Empty);

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
                        Menu menu = new(templateName: templateName, name: "GRAPHICS LIST", blocksUpdatesBelow: true, canBeClosedManually: true, templateExecuteHelper: executeHelper);

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
                    throw new ArgumentException($"Unsupported menu templateName - {templateName}.");
            }
        }

        private static void CreateCharacterSelection(Menu menu)
        {
            var playerSelectorValueDict = new Dictionary<object, object>();

            List<PieceTemplate.Name> playerNames = PieceInfo.GetPlayerNames();
            if (!Preferences.EnableTestCharacters) playerNames.Remove(PieceTemplate.Name.PlayerTestDemoness); // add every test character here

            foreach (PieceTemplate.Name playerName in playerNames)
            {
                playerSelectorValueDict[playerName] = PieceInfo.GetTexture(playerName);
            }

            new Selector(menu: menu, name: "character", valueDict: playerSelectorValueDict, targetObj: new Preferences(), propertyName: "newWorldPlayerName", rebuildsAllMenus: true);

            var startingItemNames = new List<PieceTemplate.Name> { PieceTemplate.Name.BeltSmall, PieceTemplate.Name.Map, PieceTemplate.Name.BootsSpeed, PieceTemplate.Name.GlovesStrength };

            var startingItemSelectorValueDict = new Dictionary<object, object>();
            foreach (PieceTemplate.Name itemName in startingItemNames)
            {
                PieceInfo.Info pieceInfo = PieceInfo.GetInfo(itemName);
                startingItemSelectorValueDict[itemName] = new List<object> { pieceInfo.readableName, pieceInfo.texture };
            }

            new Selector(menu: menu, name: "starting item", valueDict: startingItemSelectorValueDict, targetObj: new Preferences(), propertyName: "newWorldStartingItem", rebuildsAllMenus: true);
        }

        private static Menu CreateCraftMenu(Name templateName, Craft.Category category, string label, SoundData.Name soundOpen)
        {
            World world = World.GetTopWorld();
            Player player = world.Player;

            if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
            {
                new TextWindow(text: "I can't craft with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 0, closingTask: Scheduler.TaskName.ShowTutorialInGame, closingTaskHelper: new Dictionary<string, Object> { { "tutorial", Tutorials.Type.KeepingAnimalsAway }, { "world", world }, { "ignoreDelay", true } }, animSound: world.DialogueSound);

                return null;
            }

            if (!player.CanSeeAnything)
            {
                new HintMessage(text: "It is too dark to craft.", boxType: HintMessage.BoxType.Dialogue, delay: 1, blockInput: false, animate: true, useTransition: false).ConvertToTask(storeForLaterUse: false); // converted to task for display in proper order (after other messages, not before)

                return null;
            }

            if (player.IsVeryTired)
            {
                new HintMessage(text: "I'm too tired to craft...", boxType: HintMessage.BoxType.Dialogue, delay: 1, blockInput: false, animate: true, useTransition: false).ConvertToTask(storeForLaterUse: false); // converted to task for display in proper order (after other messages, not before)

                return null;
            }

            Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.Craft, world: World.GetTopWorld(), ignoreDelay: true);

            List<PieceStorage> storageList = player.GetCraftStoragesToTakeFrom(showCraftMarker: true);

            Menu menu = new(templateName: templateName, name: label, blocksUpdatesBelow: true, canBeClosedManually: true, layout: SonOfRobinGame.platform == Platform.Mobile ? Menu.Layout.Right : Menu.Layout.Left, alwaysShowSelectedEntry: true, templateExecuteHelper: null, soundOpen: soundOpen);
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
            foreach (string propertyName in new List<string> { "inventory", "pauseMenu", "craft" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "inventory");
            foreach (string propertyName in new List<string> { "invSwitch", "invPickOne", "invPickStack", "invSort" })
            { new Selector(menu: menu, name: newMapping.GetReadablePropertyName(propertyName), valueDict: keysOrButtonsDict, targetObj: newMapping, propertyName: propertyName, captureInput: true, captureKeys: captureKeys, captureButtons: captureButtons); }

            new Separator(menu: menu, name: "", isEmpty: true);
            new Separator(menu: menu, name: "map");
            foreach (string propertyName in new List<string> { "mapToggleMarker", "mapZoomIn", "mapZoomOut", "mapCenterPlayer", "mapToggleLocations" })
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