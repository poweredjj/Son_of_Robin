using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Preferences
    {
        public static int newWorldWidth = 8000;
        public static int newWorldHeight = 4000;
        public static int newWorldSeed = -1;
        private static bool debugMode = false;
        public static float globalScale = 1f;
        public static float menuScale = 1f;
        public static float worldScale = 1f;
        private static bool fullScreenMode = true;
        private static bool frameSkip = true;
        public static bool showDemoWorld = true;
        public static int autoSaveDelayMins = 30;

        // debug variables should not be saved to preferences file
        public static bool debugUseMultipleThreads = true;
        public static bool debugShowRects = false;
        public static bool debugShowCellData = false;
        public static bool debugShowStates = false;
        public static bool debugShowStatBars = false;
        public static bool debugCreateMissingPieces = true;

        public static int MaxThreadsToUse
        { get { return debugUseMultipleThreads ? Environment.ProcessorCount : 1; } }

        public static bool FullScreenMode
        {
            get { return fullScreenMode; }
            set
            {
                fullScreenMode = value;
                ShowAppliedAfterRestartMessage("Fullscreen");
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
            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"{settingName} setting will be applied after restart.", color: Color.White);
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
                globalScale = 2f;
                menuScale = 1.5f;
            }
        }

        public static void Save()
        {
            var prefsData = new Dictionary<string, Object> { };

            prefsData["newWorldWidth"] = newWorldWidth;
            prefsData["newWorldHeight"] = newWorldHeight;
            prefsData["newWorldSeed"] = newWorldSeed;
            prefsData["debugMode"] = debugMode;
            prefsData["globalScale"] = globalScale;
            prefsData["menuScale"] = menuScale;
            prefsData["worldScale"] = worldScale;
            prefsData["fullScreenMode"] = fullScreenMode;
            prefsData["frameSkip"] = frameSkip;
            prefsData["showDemoWorld"] = showDemoWorld;
            prefsData["autoSaveDelayMins"] = autoSaveDelayMins;

            LoaderSaver.Save(path: SonOfRobinGame.prefsPath, savedObj: prefsData);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Preferences saved.", color: Color.White);
        }

        public static void Load()
        {

            var prefsData = (Dictionary<string, Object>)LoaderSaver.Load(path: SonOfRobinGame.prefsPath);
            if (prefsData != null)
            {
                try
                {
                    newWorldWidth = (int)prefsData["newWorldWidth"];
                    newWorldHeight = (int)prefsData["newWorldHeight"];
                    newWorldSeed = (int)prefsData["newWorldSeed"];
                    debugMode = (bool)prefsData["debugMode"];
                    globalScale = (float)prefsData["globalScale"];
                    menuScale = (float)prefsData["menuScale"];
                    worldScale = (float)prefsData["worldScale"];
                    fullScreenMode = (bool)prefsData["fullScreenMode"];
                    frameSkip = (bool)prefsData["frameSkip"];
                    showDemoWorld = (bool)prefsData["showDemoWorld"];
                    autoSaveDelayMins = (int)prefsData["autoSaveDelayMins"];
                }
                catch (KeyNotFoundException)
                { MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "KeyNotFoundException while loading preferences.", color: Color.White); }
            }

            if (SonOfRobinGame.platform == Platform.Mobile) fullScreenMode = true; // window mode makes no sense on mobile
            if (SonOfRobinGame.fakeMobileMode) fullScreenMode = false; // fakeMobileMode uses mouse (and mouse cursor is not visible in fullscreen mode)

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Preferences loaded.", color: Color.White);

        }

    }
}
