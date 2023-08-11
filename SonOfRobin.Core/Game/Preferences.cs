using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Preferences
    {
        public enum WorldSize : byte
        {
            // lower case, for proper display in menu
            small = 0,
            medium = 1,
            large = 2,
            gigantic = 3,
        }

        public static readonly Dictionary<Object, Object> namesForResDividers = new Dictionary<Object, Object> { { 12, "low" }, { 8, "medium" }, { 5, "high" } };
        public static readonly Dictionary<Object, Object> namesForDarknessRes = new Dictionary<Object, Object> { { 4, "very low" }, { 3, "low" }, { 2, "medium" }, { 1, "high" } };
        public static readonly Dictionary<Object, Object> namesForFieldControlTipsScale = new Dictionary<Object, Object> { { 0.15f, "micro" }, { 0.25f, "small" }, { 0.4f, "medium" }, { 0.5f, "large" }, { 0.6f, "huge" }, { 0.75f, "gigantic" } };
        public static readonly Dictionary<Object, Object> namesForMapMarkerScale = new Dictionary<Object, Object> { { 0.25f, "small" }, { 0.5f, "medium" }, { 1f, "big" }, { 2f, "huge" }, { 3f, "gigantic" } };

        public static int newWorldSize;

        public static int newWorldResDivider;
        public static PieceTemplate.Name newWorldPlayerName;
        public static PieceTemplate.Name newWorldStartingItem;

        public static bool randomSeed;
        public static char seedDigit1 = '0';
        public static char seedDigit2 = '0';
        public static char seedDigit3 = '0';
        public static char seedDigit4 = '0';

        public static string neededForMenus = ""; // needed for menus, which display some values, but do not need to set anything

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
                    newWorldResDivider = 5;
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
                Scene.ScheduleAllScenesResize();
            }
        }

        public static float menuScale = 0.75f;
        private static float worldScale = 1.5f;

        public static float WorldScale
        {
            get { return worldScale; }
            set
            {
                if (worldScale == value) return;
                worldScale = value;

                World world = World.GetTopWorld();
                if (world != null) world.CreateNewDarknessMask();
            }
        }

        public static bool CanZoomOut
        { get { return worldScale >= 1; } }

        private static bool fullScreenMode = false;
        private static bool vSync = true;
        private static bool frameSkip = false;
        private static bool halfFramerate = false;
        public static bool progressBarShowDetails = false;
        public static bool showDemoWorld = true;
        private static bool pointToWalk = true;
        public static bool swapMouseButtons = false;
        public static bool rumbleEnabled = true;     
        public static int stateMachinesDurationFrameMS { get; private set; }
        private static float stateMachinesDurationFramePercent = 0.90f;
        public static float StateMachinesDurationFramePercent
        {
            get { return stateMachinesDurationFramePercent; }
            set
            {
                stateMachinesDurationFramePercent = value;
                stateMachinesDurationFrameMS = (int)(1d / (double)60 * 1000d * stateMachinesDurationFramePercent);
                // MessageLog.AddMessage(msgType: MsgType.User, message: $"{stateMachinesDurationFramePercent * 100}% -> {stateMachinesDurationFrameMS}ms");
            }
        }

        public static bool PointToWalk
        {
            get { return pointToWalk; }
            set
            {
                pointToWalk = value;
                InputMapper.RebuildMappings();
            }
        }

        private static bool pointToInteract = true;

        public static bool PointToInteract
        {
            get { return pointToInteract; }
            set
            {
                pointToInteract = value;
                InputMapper.RebuildMappings();
            }
        }

        public static int maxTexturesToLoad = 1000;
        public static int displayResX = 1920;
        public static int displayResY = 1080;
        public static bool showFieldControlTips = true;
        public static float fieldControlTipsScale = 0.4f;
        private static ButtonScheme.Type controlTipsScheme = ButtonScheme.Type.XboxSeries;
        public static float mapMarkerScale = 1f;
        public static bool showHints = true;
        public static bool highQualityWater = true;
        public static bool plantsSway = true;
        public static bool showParticles = true;
        public static bool useMultipleThreads = true;
        private static int darknessResolution = 1;
        public static int maxFlameLightsPerCell = 3;

        public static int DarknessResolution
        {
            get { return darknessResolution; }
            set
            {
                if (darknessResolution == value) return;
                darknessResolution = value;
                World world = World.GetTopWorld();
                if (world != null) world.CreateNewDarknessMask();
            }
        }

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

        private static bool mouseGesturesEmulateTouch = false;

        public static bool MouseGesturesEmulateTouch
        {
            get { return mouseGesturesEmulateTouch; }
            set
            {
                if (mouseGesturesEmulateTouch == value) return;
                mouseGesturesEmulateTouch = value;

                if (mouseGesturesEmulateTouch) SonOfRobinGame.Game.IsMouseVisible = true;
                if (!mouseGesturesEmulateTouch && FullScreenMode) SonOfRobinGame.Game.IsMouseVisible = false;

                TouchInput.SetEmulationByMouse();
            }
        }

        public static bool ShowTouchTips
        { get { return Input.currentControlType == Input.ControlType.Touch; } }

        public static bool enableTouchJoysticks = false;

        private static bool enableTouchButtons = false;

        public static bool EnableTouchButtons
        {
            get { return enableTouchButtons; }
            set
            {
                if (SonOfRobinGame.platform == Platform.Mobile && Input.currentControlType == Input.ControlType.Touch) value = true; // when not using joypad / keyboard, mobile should always have touch buttons enabled
                enableTouchButtons = value;

                if (enableTouchButtons)
                {
                    if (SonOfRobinGame.touchOverlay == null) SonOfRobinGame.touchOverlay = new TouchOverlay();
                }
                else
                {
                    if (SonOfRobinGame.touchOverlay != null)
                    {
                        SonOfRobinGame.touchOverlay.Remove();
                        SonOfRobinGame.touchOverlay = null;
                    }
                }
            }
        }

        private static bool fpsCounterPosRight = true;

        public static bool FpsCounterPosRight
        {
            get { return fpsCounterPosRight; }
            set
            {
                if (fpsCounterPosRight == value) return;

                fpsCounterPosRight = value;
                Scene.ScheduleAllScenesResize();
            }
        }

        private static bool fpsCounterShowGraph = true;
        private static int fpsCounterGraphLength = 70;

        public static int FpsCounterGraphLength
        {
            get { return fpsCounterGraphLength; }
            set
            {
                if (fpsCounterGraphLength == value) return;

                fpsCounterGraphLength = value;

                Scene counterScene = Scene.GetTopSceneOfType(typeof(FpsCounter));
                if (counterScene != null)
                {
                    FpsCounter fpsCounter = (FpsCounter)counterScene;
                    fpsCounter.ResizeFpsHistory(fpsCounterGraphLength);
                }
                Scene.ScheduleAllScenesResize();
            }
        }

        public static bool FpsCounterShowGraph
        {
            get { return fpsCounterShowGraph; }
            set
            {
                if (fpsCounterShowGraph == value) return;

                fpsCounterShowGraph = value;
                Scene.ScheduleAllScenesResize();
            }
        }

        private static bool showFpsCounter = false;

        public static bool ShowFpsCounter
        {
            get { return showFpsCounter; }
            set
            {
                showFpsCounter = value;

                if (showFpsCounter)
                {
                    if (SonOfRobinGame.fpsCounter == null) SonOfRobinGame.fpsCounter = new FpsCounter();
                }
                else
                {
                    if (SonOfRobinGame.fpsCounter != null)
                    {
                        SonOfRobinGame.fpsCounter.Remove();
                        SonOfRobinGame.fpsCounter = null;
                    }
                }
            }
        }

        public static bool zoomedOut = false; // used to store virtual button value

        // debug variables should not be saved to preferences file
        public static bool debugShowRects = false;

        public static bool debugShowCellData = false;
        public static bool debugShowPieceData = false;
        public static bool debugShowAnimalTargets = false;
        public static bool debugShowStates = false;
        public static bool debugShowStatBars = false;
        public static bool debugShowFruitRects = false;
        public static bool debugCreateMissingPieces = true;
        private static bool debugShowWholeMap = false;
        public static bool debugShowAllRecipes = false;
        public static bool debugShowAllTutorials = false;
        public static bool debugSaveEverywhere = false;
        public static bool debugShowSounds = false;
        public static bool debugDisablePlayerPanel = false;
        public static bool debugAllowMapAnimation = false;
        public static bool debugShowPlantGrowthInCamera = false;
        public static bool debugFastPlantGrowth = false;
        public static bool debugShowAnimSizeChangeInCamera = false;
        public static bool debugEnableTestCharacters = false;
        public static bool debugEnableExtremeZoomLevels = false;
        public static bool debugEnableExtremeMapSizes = false;
        public static bool debugInstantCookBrew = false;
        public static bool debugShowOutsideCamera = false;
        public static bool debugShowFocusRect = false;
        public static bool EnableTestCharacters { get { return debugEnableTestCharacters || SonOfRobinGame.ThisIsHomeMachine || SonOfRobinGame.ThisIsWorkMachine; } }
        public static bool DebugShowWholeMap
        {
            get { return debugShowWholeMap; }
            set
            {
                if (debugShowWholeMap == value) return;
                debugShowWholeMap = value;

                World world = World.GetTopWorld();
                if (world != null) world.map.ForceRender();
            }
        }

        private static WorldSize selectedWorldSize;

        public static WorldSize SelectedWorldSize
        {
            get { return selectedWorldSize; }
            set
            {
                selectedWorldSize = value;

                switch (selectedWorldSize)
                {
                    case WorldSize.small:
                        newWorldSize = 10000;
                        break;

                    case WorldSize.medium:
                        newWorldSize = 30000;
                        break;

                    case WorldSize.large:
                        newWorldSize = 40000;
                        break;

                    case WorldSize.gigantic:
                        newWorldSize = 60000;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported worldSize - {selectedWorldSize}.");
                }

                //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Setting world size to {newWorldSize}x{newWorldSize}");
            }
        }

        private static bool debugGodMode = false;

        public static bool DebugGodMode
        {
            get { return debugGodMode; }
            set
            {
                if (debugGodMode == value) return;
                debugGodMode = value;
                debugShowWholeMap = debugGodMode;
                debugAllowMapAnimation = debugGodMode;
                World world = World.GetTopWorld();
                if (world != null)
                {
                    if (debugMode || !world.Player.buffEngine.HasBuff(BuffEngine.BuffType.EnableMap)) world.MapEnabled = debugGodMode;
                    world.map.ForceRender();
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

        public static bool HalfFramerate
        {
            get { return halfFramerate; }
            set
            {
                if (halfFramerate == value) return;

                halfFramerate = value;
                ShowAppliedAfterRestartMessage("FPS cap");
            }
        }

        private static void ApplyFPSCap() // doesn't work correctly on mobile
        {
            SonOfRobinGame.GfxDevMgr.PreparingDeviceSettings += (sender, e) =>
            {
                e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = halfFramerate ? PresentInterval.Two : PresentInterval.One;
            };
        }

        public static bool VSync
        {
            get { return vSync; }
            set
            {
                vSync = value;
                ShowAppliedAfterRestartMessage("Vertical sync");
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
                SonOfRobinGame.Game.IsFixedTimeStep = value;
                SonOfRobinGame.Game.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            }
        }

        public static void ResetNewWorldSettings()
        {
            newWorldPlayerName = PieceTemplate.Name.PlayerBoy;
            newWorldStartingItem = PieceTemplate.Name.BeltSmall;
            SelectedWorldSize = WorldSize.medium;
            CustomizeWorld = false;
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

            ResetNewWorldSettings();

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
                if (SonOfRobinGame.GfxDevMgr.PreferredBackBufferWidth > 1500) globalScale = 2f;

                menuScale = 1.25f;
                showControlTips = false;
                enableTouchJoysticks = true;
                rumbleEnabled = false;
            }
            else
            {
                showControlTips = true;
            }

            EnableTouchButtons = SonOfRobinGame.platform == Platform.Mobile;
            MouseGesturesEmulateTouch = true; // mouse input is used through touch emulation
            StateMachinesDurationFramePercent = StateMachinesDurationFramePercent; // to refresh miliseconds variable
        }

        public static void Save()
        {
            var prefsData = new Dictionary<string, Object> { };

            prefsData["debugMode"] = debugMode;
            prefsData["customizeWorld"] = customizeWorld;
            prefsData["newWorldResDivider"] = newWorldResDivider;
            prefsData["selectedWorldSize"] = SelectedWorldSize;
            prefsData["newWorldSize"] = newWorldSize;
            prefsData["newWorldPlayerName"] = newWorldPlayerName;
            prefsData["newWorldStartingItem"] = newWorldStartingItem;
            prefsData["randomSeed"] = randomSeed;
            prefsData["seedDigit1"] = seedDigit1;
            prefsData["seedDigit2"] = seedDigit2;
            prefsData["seedDigit3"] = seedDigit3;
            prefsData["seedDigit4"] = seedDigit4;
            prefsData["globalScale"] = globalScale;
            prefsData["menuScale"] = menuScale;
            prefsData["worldScale"] = worldScale;
            prefsData["fullScreenMode"] = fullScreenMode;
            prefsData["frameSkip"] = frameSkip;
            prefsData["cap30FPS"] = halfFramerate;
            prefsData["showDemoWorld"] = showDemoWorld;
            prefsData["displayResX"] = displayResX;
            prefsData["displayResY"] = displayResY;
            prefsData["showControlTips"] = showControlTips;
            prefsData["showFieldControlTips"] = showFieldControlTips;
            prefsData["fieldControlTipsScale"] = fieldControlTipsScale;
            prefsData["mapMarkerScale"] = mapMarkerScale;
            prefsData["showHints"] = showHints;
            prefsData["useMultipleThreads"] = useMultipleThreads;
            prefsData["darknessResolution"] = darknessResolution;
            prefsData["drawShadows"] = drawShadows;
            prefsData["drawSunShadows"] = drawSunShadows;
            prefsData["controlTipsScheme"] = ControlTipsScheme;
            prefsData["EnableTouchButtons"] = EnableTouchButtons;
            prefsData["showFpsCounter"] = showFpsCounter;
            prefsData["fpsCounterPosRight"] = fpsCounterPosRight;
            prefsData["fpsCounterShowGraph"] = fpsCounterShowGraph;
            prefsData["fpsCounterGraphLength"] = FpsCounterGraphLength;
            prefsData["enableTouchJoysticks"] = enableTouchJoysticks;
            prefsData["pointToWalk"] = pointToWalk;
            prefsData["pointToInteract"] = pointToInteract;
            prefsData["currentMappingGamepad"] = InputMapper.currentMappingGamepad.Serialize();
            prefsData["currentMappingKeyboard"] = InputMapper.currentMappingKeyboard.Serialize();
            prefsData["soundGlobalOn"] = Sound.GlobalOn;
            prefsData["soundGlobalVolume"] = Sound.globalVolume;
            prefsData["soundMenuOn"] = Sound.menuOn;
            prefsData["textWindowAnimOn"] = Sound.textWindowAnimOn;
            prefsData["vSync"] = vSync;
            prefsData["progressBarShowDetails"] = progressBarShowDetails;
            prefsData["swapMouseButtons"] = swapMouseButtons;
            prefsData["maxTexturesToLoad"] = maxTexturesToLoad;
            prefsData["highQualityWater"] = highQualityWater;
            prefsData["plantsSway"] = plantsSway;
            prefsData["rumbleEnabled"] = rumbleEnabled;
            prefsData["showParticles"] = showParticles;
            prefsData["maxFlameLightsPerCell"] = maxFlameLightsPerCell;
            prefsData["newWorldStartingItem"] = newWorldStartingItem;
            prefsData["StateMachinesDurationFramePercent"] = StateMachinesDurationFramePercent;

            FileReaderWriter.Save(path: SonOfRobinGame.prefsPath, savedObj: prefsData, compress: false);

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
                    CustomizeWorld = (bool)prefsData["customizeWorld"];
                    SelectedWorldSize = (WorldSize)(Int64)prefsData["selectedWorldSize"];
                    newWorldResDivider = (int)(Int64)prefsData["newWorldResDivider"];
                    newWorldSize = (int)(Int64)prefsData["newWorldSize"];
                    newWorldPlayerName = (PieceTemplate.Name)(Int64)prefsData["newWorldPlayerName"];
                    randomSeed = (bool)prefsData["randomSeed"];
                    seedDigit1 = ((string)prefsData["seedDigit1"])[0];
                    seedDigit2 = ((string)prefsData["seedDigit2"])[0];
                    seedDigit3 = ((string)prefsData["seedDigit3"])[0];
                    seedDigit4 = ((string)prefsData["seedDigit4"])[0];
                    globalScale = (float)(double)prefsData["globalScale"];
                    menuScale = (float)(double)prefsData["menuScale"];
                    worldScale = (float)(double)prefsData["worldScale"];
                    fullScreenMode = (bool)prefsData["fullScreenMode"];
                    frameSkip = (bool)prefsData["frameSkip"];
                    showDemoWorld = (bool)prefsData["showDemoWorld"];
                    displayResX = (int)(Int64)prefsData["displayResX"];
                    displayResY = (int)(Int64)prefsData["displayResY"];
                    showControlTips = (bool)prefsData["showControlTips"];
                    showFieldControlTips = (bool)prefsData["showFieldControlTips"];
                    fieldControlTipsScale = (float)(double)prefsData["fieldControlTipsScale"];
                    mapMarkerScale = (float)(double)prefsData["mapMarkerScale"];
                    showHints = (bool)prefsData["showHints"];
                    useMultipleThreads = (bool)prefsData["useMultipleThreads"];
                    darknessResolution = (int)(Int64)prefsData["darknessResolution"];
                    drawShadows = (bool)prefsData["drawShadows"];
                    drawSunShadows = (bool)prefsData["drawSunShadows"];
                    controlTipsScheme = (ButtonScheme.Type)(Int64)prefsData["controlTipsScheme"];
                    EnableTouchButtons = (bool)prefsData["EnableTouchButtons"];
                    showFpsCounter = (bool)prefsData["showFpsCounter"];
                    fpsCounterPosRight = (bool)prefsData["fpsCounterPosRight"];
                    fpsCounterShowGraph = (bool)prefsData["fpsCounterShowGraph"];
                    FpsCounterGraphLength = (int)(Int64)prefsData["fpsCounterGraphLength"];
                    enableTouchJoysticks = (bool)prefsData["enableTouchJoysticks"];
                    pointToWalk = (bool)prefsData["pointToWalk"];
                    pointToInteract = (bool)prefsData["pointToInteract"];
                    InputPackage loadedMappingGamepad = InputPackage.Deserialize(prefsData["currentMappingGamepad"]);
                    InputPackage loadedMappingKeyboard = InputPackage.Deserialize(prefsData["currentMappingKeyboard"]);
                    if (!loadedMappingGamepad.IsObsolete) InputMapper.currentMappingGamepad = loadedMappingGamepad;
                    if (!loadedMappingKeyboard.IsObsolete) InputMapper.currentMappingKeyboard = loadedMappingKeyboard;
                    InputMapper.newMappingGamepad = InputMapper.currentMappingGamepad.MakeCopy();
                    InputMapper.newMappingKeyboard = InputMapper.currentMappingKeyboard.MakeCopy();
                    Sound.GlobalOn = (bool)prefsData["soundGlobalOn"];
                    Sound.globalVolume = (float)(double)prefsData["soundGlobalVolume"];
                    Sound.menuOn = (bool)prefsData["soundMenuOn"];
                    Sound.textWindowAnimOn = (bool)prefsData["textWindowAnimOn"];
                    vSync = (bool)prefsData["vSync"];
                    progressBarShowDetails = (bool)prefsData["progressBarShowDetails"];
                    swapMouseButtons = (bool)prefsData["swapMouseButtons"];
                    maxTexturesToLoad = (int)(Int64)prefsData["maxTexturesToLoad"];
                    highQualityWater = (bool)prefsData["highQualityWater"];
                    plantsSway = (bool)prefsData["plantsSway"];
                    halfFramerate = (bool)prefsData["cap30FPS"];
                    debugMode = (bool)prefsData["debugMode"];
                    rumbleEnabled = (bool)prefsData["rumbleEnabled"];
                    showParticles = (bool)prefsData["showParticles"];
                    maxFlameLightsPerCell = (int)(Int64)prefsData["maxFlameLightsPerCell"];
                    StateMachinesDurationFramePercent = (int)(Int64)prefsData["StateMachinesDurationFramePercent"];

                    prefsLoaded = true;
                }
                catch (KeyNotFoundException) { MessageLog.AddMessage(msgType: MsgType.Debug, message: "KeyNotFoundException while loading preferences.", color: Color.White); }
                catch (InvalidCastException) { MessageLog.AddMessage(msgType: MsgType.Debug, message: "InvalidCastException while loading preferences.", color: Color.White); }
            }

            if (prefsLoaded)
            {
                ApplyFPSCap();
            }
            else
            {
                CustomizeWorld = CustomizeWorld; // to refresh world sizes
            }

            if (SonOfRobinGame.platform == Platform.Mobile && !SonOfRobinGame.fakeMobileMode)
            {
                fullScreenMode = true; // window mode makes no sense on mobile
                vSync = true; // vSync cannot be turned off on mobile
            }
            if (SonOfRobinGame.ThisIsWorkMachine) fullScreenMode = false;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: "Preferences loaded.", color: Color.White);
        }

        public static void CheckIfResolutionIsSupported()
        {
            bool resolutionSupported = false;
            foreach (var displayMode in SonOfRobinGame.GfxDev.Adapter.SupportedDisplayModes)
            {
                if (displayMode.Width == displayResX && displayMode.Height == displayResY)
                {
                    resolutionSupported = true;
                    break;
                }
            }

            if (!resolutionSupported)
            {
                displayResX = SonOfRobinGame.GfxDev.Adapter.CurrentDisplayMode.Width;
                displayResY = SonOfRobinGame.GfxDev.Adapter.CurrentDisplayMode.Height;
            }
        }
    }
}