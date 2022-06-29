using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Preferences
    {
        public enum WorldSize { small, medium, large, gigantic } // lower case, for proper display in menu

        public static int newWorldWidth;
        public static int newWorldHeight;

        public static int newWorldResDivider = 2;
        public static readonly Dictionary<Object, Object> namesForResDividers = new Dictionary<Object, Object> { { 10, "low" }, { 3, "medium" }, { 2, "high" }, { 1, "ultra" } };
        public static readonly Dictionary<Object, Object> namesForDarknessRes = new Dictionary<Object, Object> { { 4, "very low" }, { 3, "low" }, { 2, "medium" }, { 1, "high" } };
        public static readonly Dictionary<Object, Object> namesForFieldControlTipsScale = new Dictionary<Object, Object> { { 0.15f, "micro" }, { 0.25f, "small" }, { 0.4f, "medium" }, { 0.5f, "large" }, { 0.6f, "huge" }, { 0.75f, "gigantic" } };

        public static bool randomSeed = true;
        public static char seedDigit1 = '0';
        public static char seedDigit2 = '0';
        public static char seedDigit3 = '0';
        public static char seedDigit4 = '0';
        public static int NewWorldSeed
        {
            get
            {
                if (randomSeed)
                {
                    Random oneTimeRandom = new Random();
                    return oneTimeRandom.Next(0, 9999);
                }
                return Convert.ToInt32($"{seedDigit1}{seedDigit2}{seedDigit3}{seedDigit4}");
            }
        }

        private static bool customizeWorld = false;
        public static bool CustomizeWorld
        {
            get { return customizeWorld; }
            set
            {
                customizeWorld = value;
                if (!customizeWorld)
                {
                    SelectedWorldSize = selectedWorldSize;  // to restore base values
                    newWorldResDivider = 2;
                    randomSeed = true;
                }
            }
        }

        private static bool debugMode = false;

        private static float globalScale = 1f;
        public static float GlobalScale
        {
            get { return globalScale; }
            set
            {
                if (globalScale == value) return;

                globalScale = value;
                Scene.ResizeAllScenes();
            }
        }
        public static float menuScale = 0.75f;
        public static float worldScale = 1f;
        private static bool fullScreenMode = true;
        public static bool loadWholeMap = true;
        private static bool frameSkip = true;
        public static bool showDemoWorld = true;
        public static int autoSaveDelayMins = 5;
        public static int mobileMaxLoadedTextures = 1000;
        public static int displayResX = 1920;
        public static int displayResY = 1080;
        public static bool showFieldControlTips = true;
        public static float fieldControlTipsScale = 0.25f;
        private static ButtonScheme.Type controlTipsScheme = ButtonScheme.Type.M;
        public static bool showHints = true;
        public static bool showLighting = true;
        public static bool showDebris = true;
        public static bool useMultipleThreads = true;
        private static bool mouseGesturesEmulateTouch = false;
        public static int darknessResolution = 1;
        public static bool drawShadows = true;
        public static bool drawSunShadows = true;
        private static bool showControlTips = true;
        public static bool ShowControlTips
        {
            get { return showControlTips; }
            set
            {
                if (showControlTips == value) return;
                showControlTips = value;
                ControlTipsScheme = ControlTipsScheme; // to refresh data
            }
        }
        public static bool MouseGesturesEmulateTouch
        {
            get { return mouseGesturesEmulateTouch; }
            set
            {
                if (mouseGesturesEmulateTouch == value) return;
                mouseGesturesEmulateTouch = value;

                if (mouseGesturesEmulateTouch) SonOfRobinGame.game.IsMouseVisible = true;
                if (!mouseGesturesEmulateTouch && FullScreenMode) SonOfRobinGame.game.IsMouseVisible = false;

                TouchInput.SetEmulationByMouse();
            }
        }
        public static bool ShowTouchTips
        { get { return SonOfRobinGame.platform == Platform.Mobile && !showControlTips; } }

        private static bool enableTouch = false;
        public static bool EnableTouch
        {
            get { return enableTouch; }
            set
            {
                if (SonOfRobinGame.platform == Platform.Mobile) value = true; // mobile should always have touch enabled
                enableTouch = value;

                if (enableTouch)
                {
                    if (SonOfRobinGame.touchOverlay == null) SonOfRobinGame.touchOverlay = new TouchOverlay();
                    if (SonOfRobinGame.platform != Platform.Mobile) MouseGesturesEmulateTouch = true;
                }
                else
                {
                    if (SonOfRobinGame.touchOverlay != null)
                    {
                        SonOfRobinGame.touchOverlay.Remove();
                        SonOfRobinGame.touchOverlay = null;
                    }
                    MouseGesturesEmulateTouch = false;
                }
            }
        }

        public static bool zoomedOut = false; // used to store virtual button value

        // debug variables should not be saved to preferences file
        public static bool debugShowRects = false;
        public static bool debugShowCellData = false;
        public static bool debugShowAnimalTargets = false;
        public static bool debugShowStates = false;
        public static bool debugShowStatBars = false;
        public static bool debugShowFruitRects = false;
        public static bool debugCreateMissingPieces = true;
        public static bool debugShowAllMapPieces = false;
        private static bool debugShowWholeMap = false;
        public static bool DebugShowWholeMap
        {
            get { return debugShowWholeMap; }
            set
            {
                if (debugShowWholeMap == value) return;
                debugShowWholeMap = value;

                World world = World.GetTopWorld();
                if (world != null)
                {
                    world.mapBig.ForceRender();
                    world.mapSmall.ForceRender();
                }
            }
        }

        private static WorldSize selectedWorldSize = WorldSize.medium;
        public static WorldSize SelectedWorldSize
        {
            get { return selectedWorldSize; }
            set
            {
                selectedWorldSize = value;

                switch (selectedWorldSize)
                {
                    case WorldSize.small:
                        newWorldWidth = 10000;
                        newWorldHeight = 10000;
                        break;

                    case WorldSize.medium:
                        newWorldWidth = 30000;
                        newWorldHeight = 30000;
                        break;

                    case WorldSize.large:
                        newWorldWidth = 40000;
                        newWorldHeight = 40000;
                        break;

                    case WorldSize.gigantic:
                        newWorldWidth = 60000;
                        newWorldHeight = 60000;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported worldSize - {selectedWorldSize}.");

                }

                //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Setting world size to {newWorldWidth}x{newWorldHeight}");

            }
        }

        private static bool debugGodMode = false;
        public static bool DebugGodMode
        {
            get { return debugGodMode; }
            set
            {
                debugGodMode = value;
                debugShowWholeMap = debugGodMode;
                debugShowAllMapPieces = debugGodMode;
                World world = World.GetTopWorld();
                if (world != null)
                {
                    world.MapEnabled = debugGodMode;
                    if (debugGodMode)
                    {
                        world.mapBig.ForceRender();
                        world.mapSmall.ForceRender();
                    }
                }
            }
        }

        public static ButtonScheme.Type ControlTipsScheme
        {
            get { return controlTipsScheme; }
            set
            {
                controlTipsScheme = value;
                ButtonScheme.ChangeType(value);
                InputVis.Refresh();
                Tutorials.RefreshData();
                PieceHint.RefreshData();
                ControlTips.RefreshTopTipsLayout();
            }
        }

        public static int MaxThreadsToUse
        { get { return useMultipleThreads ? Environment.ProcessorCount : 1; } }

        public static bool FullScreenMode
        {
            get { return fullScreenMode; }
            set
            {
                fullScreenMode = value;
                ShowAppliedAfterRestartMessage("Fullscreen");
            }
        }

        public static List<Object> AvailableScreenModes
        {
            get
            {
                var availableScreenModes = new List<Object> { };

                foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    if (mode.Width >= 640 && mode.Height >= 480) availableScreenModes.Add($"{mode.Width}x{mode.Height}");
                }

                return availableScreenModes;
            }
        }

        public static string FullScreenResolution
        {
            get { return $"{displayResX}x{displayResY}"; }
            set
            {
                displayResX = Convert.ToInt32(value.Split('x')[0]);
                displayResY = Convert.ToInt32(value.Split('x')[1]);
            }
        }

        public static bool FrameSkip
        {
            get { return frameSkip; }
            set
            {
                frameSkip = value;
                SonOfRobinGame.game.IsFixedTimeStep = value;
            }
        }

        private static void ShowAppliedAfterRestartMessage(string settingName)
        {
            new TextWindow(text: $"{settingName} setting will be applied after restart.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: false);
        }

        public static bool DebugMode
        {
            get { return debugMode; }
            set
            {
                debugMode = value;

                if (debugMode)
                {
                    Scene existingDebug = Scene.GetTopSceneOfType(typeof(DebugScene));
                    if (existingDebug == null) new DebugScene();

                    Scene existingStackView = Scene.GetTopSceneOfType(typeof(StackView));
                    if (existingStackView == null) new StackView();
                }
                else
                {
                    Scene existingDebug = Scene.GetTopSceneOfType(typeof(DebugScene));
                    if (existingDebug != null) existingDebug.Remove();

                    Scene existingStackView = Scene.GetTopSceneOfType(typeof(StackView));
                    if (existingStackView != null) existingStackView.Remove();
                }

            }
        }
        public static void Initialize()
        {
            // initial values, that must be set, after setting the platform variable

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
                if (SonOfRobinGame.graphics.PreferredBackBufferWidth > 1500) globalScale = 2f;

                menuScale = 1.5f;
                loadWholeMap = false;
                showControlTips = false;
                showFieldControlTips = false;
            }
            else
            {
                loadWholeMap = true;
                showControlTips = true;
                showFieldControlTips = true;
            }

            EnableTouch = SonOfRobinGame.platform == Platform.Mobile;
            MouseGesturesEmulateTouch = SonOfRobinGame.fakeMobileMode;
        }

        public static void Save()
        {
            var prefsData = new Dictionary<string, Object> { };

            prefsData["debugMode"] = debugMode;
            prefsData["customizeWorld"] = customizeWorld;
            prefsData["newWorldResDivider"] = newWorldResDivider;
            prefsData["selectedWorldSize"] = SelectedWorldSize;
            prefsData["newWorldWidth"] = newWorldWidth;
            prefsData["newWorldHeight"] = newWorldHeight;
            prefsData["randomSeed"] = randomSeed;
            prefsData["seedDigit1"] = seedDigit1;
            prefsData["seedDigit2"] = seedDigit2;
            prefsData["seedDigit3"] = seedDigit3;
            prefsData["seedDigit4"] = seedDigit4;
            prefsData["globalScale"] = globalScale;
            prefsData["menuScale"] = menuScale;
            prefsData["worldScale"] = worldScale;
            prefsData["loadWholeMap"] = loadWholeMap;
            prefsData["fullScreenMode"] = fullScreenMode;
            prefsData["frameSkip"] = frameSkip;
            prefsData["showDemoWorld"] = showDemoWorld;
            prefsData["autoSaveDelayMins"] = autoSaveDelayMins;
            prefsData["mobileMaxLoadedTextures"] = mobileMaxLoadedTextures;
            prefsData["displayResX"] = displayResX;
            prefsData["displayResY"] = displayResY;
            prefsData["showControlTips"] = showControlTips;
            prefsData["showFieldControlTips"] = showFieldControlTips;
            prefsData["fieldControlTipsScale"] = fieldControlTipsScale;
            prefsData["showHints"] = showHints;
            prefsData["showLighting"] = showLighting;
            prefsData["showDebris"] = showDebris;
            prefsData["useMultipleThreads"] = useMultipleThreads;
            prefsData["controlTipsScheme"] = ControlTipsScheme;
            prefsData["EnableTouch"] = EnableTouch;
            prefsData["darknessResolution"] = darknessResolution;
            prefsData["drawShadows"] = drawShadows;
            prefsData["drawSunShadows"] = drawSunShadows;
            prefsData["currentMappingGamepad"] = InputMapper.currentMappingGamepad;
            prefsData["currentMappingKeyboard"] = InputMapper.currentMappingKeyboard;

            FileReaderWriter.Save(path: SonOfRobinGame.prefsPath, savedObj: prefsData);

            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Preferences saved.", color: Color.White);
        }

        public static void Load()
        {
            bool prefsLoaded = false;

            var prefsData = (Dictionary<string, Object>)FileReaderWriter.Load(path: SonOfRobinGame.prefsPath);
            if (prefsData != null)
            {
                try
                {
                    debugMode = (bool)prefsData["debugMode"];
                    CustomizeWorld = (bool)prefsData["customizeWorld"];
                    SelectedWorldSize = (WorldSize)prefsData["selectedWorldSize"];
                    newWorldResDivider = (int)prefsData["newWorldResDivider"];
                    newWorldWidth = (int)prefsData["newWorldWidth"];
                    newWorldHeight = (int)prefsData["newWorldHeight"];
                    randomSeed = (bool)prefsData["randomSeed"];
                    seedDigit1 = (char)prefsData["seedDigit1"];
                    seedDigit2 = (char)prefsData["seedDigit2"];
                    seedDigit3 = (char)prefsData["seedDigit3"];
                    seedDigit4 = (char)prefsData["seedDigit4"];
                    globalScale = (float)prefsData["globalScale"];
                    menuScale = (float)prefsData["menuScale"];
                    worldScale = (float)prefsData["worldScale"];
                    fullScreenMode = (bool)prefsData["fullScreenMode"];
                    loadWholeMap = (bool)prefsData["loadWholeMap"];
                    frameSkip = (bool)prefsData["frameSkip"];
                    showDemoWorld = (bool)prefsData["showDemoWorld"];
                    autoSaveDelayMins = (int)prefsData["autoSaveDelayMins"];
                    mobileMaxLoadedTextures = (int)prefsData["mobileMaxLoadedTextures"];
                    displayResX = (int)prefsData["displayResX"];
                    displayResY = (int)prefsData["displayResY"];
                    showControlTips = (bool)prefsData["showControlTips"];
                    showFieldControlTips = (bool)prefsData["showFieldControlTips"];
                    fieldControlTipsScale = (float)prefsData["fieldControlTipsScale"];
                    showHints = (bool)prefsData["showHints"];
                    showLighting = (bool)prefsData["showLighting"];
                    showDebris = (bool)prefsData["showDebris"];
                    useMultipleThreads = (bool)prefsData["useMultipleThreads"];
                    controlTipsScheme = (ButtonScheme.Type)prefsData["controlTipsScheme"];
                    EnableTouch = (bool)prefsData["EnableTouch"];
                    darknessResolution = (int)prefsData["darknessResolution"];
                    drawShadows = (bool)prefsData["drawShadows"];
                    drawSunShadows = (bool)prefsData["drawSunShadows"];
                    InputMapper.currentMappingGamepad = (MappingPackage)prefsData["currentMappingGamepad"];
                    InputMapper.currentMappingKeyboard = (MappingPackage)prefsData["currentMappingKeyboard"];
                    InputMapper.newMappingGamepad = InputMapper.currentMappingGamepad.MakeCopy();
                    InputMapper.newMappingKeyboard = InputMapper.currentMappingKeyboard.MakeCopy();

        prefsLoaded = true;
                }
                catch (KeyNotFoundException)
                {
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: "KeyNotFoundException while loading preferences.", color: Color.White);
                }
            }

            if (!prefsLoaded)
            {
                CustomizeWorld = CustomizeWorld; // to refresh world sizes
            }

            if (SonOfRobinGame.platform == Platform.Mobile) fullScreenMode = true; // window mode makes no sense on mobile
            if (SonOfRobinGame.ThisIsWorkMachine) fullScreenMode = false;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Preferences loaded.", color: Color.White);
        }

        public static void CheckIfResolutionIsSupported()
        {
            bool resolutionSupported = false;
            foreach (var displayMode in SonOfRobinGame.graphicsDevice.Adapter.SupportedDisplayModes)
            {
                if (displayMode.Width == displayResX && displayMode.Height == displayResY)
                {
                    resolutionSupported = true;
                    break;
                }
            }

            if (!resolutionSupported)
            {
                displayResX = SonOfRobinGame.graphicsDevice.Adapter.CurrentDisplayMode.Width;
                displayResY = SonOfRobinGame.graphicsDevice.Adapter.CurrentDisplayMode.Height;
            }
        }

    }
}
