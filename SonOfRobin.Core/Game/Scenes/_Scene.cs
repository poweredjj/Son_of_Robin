using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public abstract class Scene
    {
        public enum InputTypes : byte
        {
            None = 0,
            Always = 1,
            Normal = 2,
        }

        public enum ProcessingModes : byte
        {
            Update = 0,
            RenderToTarget = 1,
            Draw = 2,
        }

        public static List<Scene> sceneStack = new() { };
        public static List<Scene> waitingScenes = new() { };
        private static bool adaptScenesToNewSize = false;

        public readonly int priority;
        public bool HasBeenRemoved { get; private set; }
        public bool blocksUpdatesBelow;
        public bool blocksDrawsBelow;
        private readonly bool hidesSameScenesBelow;

        public bool soundActive;
        public readonly SoundData.Name startingSound;
        public bool inputActive;
        public bool updateActive;
        public bool drawActive;
        public TouchLayout touchLayout;
        public ControlTips.TipsLayout tipsLayout;

        private readonly bool alwaysUpdates;
        private readonly bool alwaysDraws;

        public ViewParams viewParams;
        public readonly TransManager transManager;

        public readonly string id;

        private readonly List<Scene> linkedScenes;

        public static Scene currentlyProcessedScene;
        public static ProcessingModes ProcessingMode { get; private set; }

        public static DateTime startUpdateTime;
        public static DateTime startDrawTime;

        public static TimeSpan UpdateTimeElapsed
        { get { return DateTime.Now - startUpdateTime; } }

        public static TimeSpan DrawTimeElapsed
        { get { return DateTime.Now - startDrawTime; } }

        public static TimeSpan LastUpdateDuration { get; private set; } = TimeSpan.Zero;

        public static TimeSpan LastDrawDuration { get; private set; } = TimeSpan.Zero;

        public InputTypes InputType { get; set; }

        private bool OtherScenesOfThisTypePresent
        {
            get
            {
                foreach (Scene scene in GetAllScenesOfType(type: this.GetType(), includeEnding: true))
                {
                    if (scene.id != this.id) return true;
                }
                return false;
            }
        }

        private static int currentUpdateStackUpdateNo = -1;
        private static readonly List<Scene> currentUpdateStack = new();

        private static int currentDrawStackDrawNo = -1;
        private static readonly List<Scene> currentDrawStack = new();

        public static List<Scene> UpdateStack
        {
            get
            {
                if (SonOfRobinGame.CurrentUpdate == currentUpdateStackUpdateNo) return currentUpdateStack;

                currentUpdateStackUpdateNo = SonOfRobinGame.CurrentUpdate;
                currentUpdateStack.Clear();

                if (sceneStack.Count == 0) return currentUpdateStack;

                var sortedSceneStack = sceneStack.OrderByDescending(o => o.priority).ToList();
                sortedSceneStack.Reverse();

                bool ignoreScenes = false;

                foreach (Scene scene in sortedSceneStack)
                {
                    if (!scene.HasBeenRemoved && ((!ignoreScenes && scene.updateActive) || scene.alwaysUpdates)) currentUpdateStack.Add(scene);
                    if (scene.blocksUpdatesBelow) ignoreScenes = true;
                }

                currentUpdateStack.Reverse();
                return currentUpdateStack;
            }
        }

        public static List<Scene> DrawStack
        {
            get
            {
                if (SonOfRobinGame.CurrentDraw == currentDrawStackDrawNo) return currentDrawStack;

                currentDrawStackDrawNo = SonOfRobinGame.CurrentDraw;
                currentDrawStack.Clear();

                if (sceneStack.Count == 0) return currentDrawStack;

                var sortedSceneStack = sceneStack.OrderByDescending(o => o.priority).ToList();
                sortedSceneStack.Reverse();

                var hiddenTypes = new List<Type>(); // to hide same types below

                foreach (Scene scene in sortedSceneStack)
                {
                    Type sceneType = scene.GetType();

                    if (hiddenTypes.Contains(sceneType)) continue;

                    if ((scene.drawActive || scene.alwaysDraws) && scene.viewParams.drawOpacity > 0f && !scene.HasBeenRemoved)
                    {
                        currentDrawStack.Add(scene);
                        if (scene.hidesSameScenesBelow && !hiddenTypes.Contains(sceneType)) hiddenTypes.Add(sceneType);
                    }

                    if (scene.blocksDrawsBelow) break;
                }

                currentDrawStack.Reverse();
                return currentDrawStack;
            }
        }

        public Matrix TransformMatrix
        {
            get
            {
                Matrix scaleMatrix = Matrix.CreateScale(
                                           (float)SonOfRobinGame.GfxDevMgr.PreferredBackBufferWidth / SonOfRobinGame.VirtualWidth / this.viewParams.drawScaleX,
                                           (float)SonOfRobinGame.GfxDevMgr.PreferredBackBufferHeight / SonOfRobinGame.VirtualHeight / this.viewParams.drawScaleY,
                                           1f);

                Matrix rotationMatrix = Matrix.CreateRotationZ(this.viewParams.drawRot);
                Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(this.viewParams.drawPosX, this.viewParams.drawPosY, 0));

                // needed for rotation to center around the camera
                Matrix translateToOrigin = Matrix.CreateTranslation(-(float)this.viewParams.drawWidth / 2f, -(float)this.viewParams.drawHeight / 2f, 0);
                // needed for rotation to center around the camera
                Matrix translateBackToPosition = Matrix.CreateTranslation((float)this.viewParams.drawWidth / 2f, (float)this.viewParams.drawHeight / 2f, 0);

                Matrix compositeMatrix = translateToOrigin * rotationMatrix * translateBackToPosition * translationMatrix * scaleMatrix;
                return compositeMatrix;
            }
        }

        public Scene(InputTypes inputType, TouchLayout touchLayout, ControlTips.TipsLayout tipsLayout, int priority = 1, bool blocksUpdatesBelow = false, bool blocksDrawsBelow = false, bool alwaysUpdates = false, bool alwaysDraws = false, bool hidesSameScenesBelow = false, SoundData.Name startingSound = SoundData.Name.Empty, bool waitForOtherScenesOfTypeToEnd = false)
        {
            this.viewParams = new ViewParams();
            this.transManager = new TransManager(scene: this);
            this.priority = priority;
            this.HasBeenRemoved = false;
            this.blocksUpdatesBelow = blocksUpdatesBelow;
            this.blocksDrawsBelow = blocksDrawsBelow;
            this.hidesSameScenesBelow = hidesSameScenesBelow;

            this.InputType = inputType;
            this.updateActive = true;
            this.drawActive = true;
            this.soundActive = true;
            this.startingSound = startingSound;
            this.touchLayout = touchLayout;
            this.tipsLayout = tipsLayout;

            this.linkedScenes = new List<Scene> { }; // scenes that will also be removed on Remove()
            this.alwaysUpdates = alwaysUpdates;
            this.alwaysDraws = alwaysDraws;
            this.id = Helpers.GetUniqueHash();

            if (waitForOtherScenesOfTypeToEnd && this.OtherScenesOfThisTypePresent) waitingScenes.Add(this);
            else this.Activate();
        }

        private void Activate()
        {
            Sound.QuickPlay(this.startingSound);
            sceneStack.Add(this);
            UpdateInputActiveTipsTouch(); // to avoid one frame delay in updating tips and touch overlay
        }

        public static void ScheduleAllScenesResize()
        {
            adaptScenesToNewSize = true;

            // has to be scheduled, to avoid resizing during Draw()
        }

        private static void ResizeAllScenes()
        {
            adaptScenesToNewSize = false;

            foreach (Scene currentScene in sceneStack)
            { currentScene.AdaptToNewSize(); }
        }

        protected virtual void AdaptToNewSize()
        {
            // Every scene should have its code that adapts to new screen size.
        }

        public virtual void Remove()
        {
            // removing all linked scenes
            foreach (Scene scene in this.linkedScenes.ToList())
            { scene.Remove(); }
            this.linkedScenes.Clear();

            // removing all links to current scene (to avoid having it linked when removed directly)
            foreach (Scene currentScene in sceneStack)
            {
                foreach (Scene linkedScene in currentScene.linkedScenes.ToList())
                { if (this.id == linkedScene.id) currentScene.linkedScenes.Remove(linkedScene); }
            }

            // rebuilding sceneStack and waitingScenes
            sceneStack = sceneStack.Where(scene => scene.id != this.id).ToList();
            waitingScenes = waitingScenes.Where(scene => scene.id != this.id).ToList();

            this.HasBeenRemoved = true;
        }

        public void AddLinkedScene(Scene scene)
        { this.linkedScenes.Add(scene); }

        public void AddLinkedScenes(List<Scene> sceneList)
        { this.linkedScenes.AddRange(sceneList); }

        protected Scene GetSceneBelow(bool ignorePriorityLessThan1 = true)
        {
            Scene previousScene = null;
            foreach (Scene scene in sceneStack)
            {
                if (scene.transManager.IsEnding) continue;
                if (scene.priority < 1 && ignorePriorityLessThan1 && scene != this) continue;
                if (scene == this) return previousScene;
                previousScene = scene;
            }

            return null;
        }

        public static void UpdateInputActiveTipsTouch()
        {
            bool normalInputSet = false;

            var inputStack = sceneStack.OrderByDescending(o => o.priority).ToList();

            for (int i = inputStack.Count - 1; i >= 0; i--)
            {
                Scene scene = inputStack[i];

                switch (scene.InputType)
                {
                    case InputTypes.None:
                        scene.inputActive = false;
                        break;

                    case InputTypes.Always:
                        scene.inputActive = true;
                        break;

                    case InputTypes.Normal:
                        if (!normalInputSet)
                        {
                            scene.inputActive = true;
                            TouchInput.SwitchToLayout(scene.touchLayout);
                            SonOfRobinGame.ControlTips.AssignScene(scene: scene);

                            normalInputSet = true;
                        }
                        else scene.inputActive = false;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported inputType - {scene.InputType}.");
                }
            }

            if (!normalInputSet)
            {
                TouchInput.SwitchToLayout(TouchLayout.Empty);
                if (SonOfRobinGame.ControlTips != null) SonOfRobinGame.ControlTips.AssignScene(scene: null);
            }
        }

        public static List<Scene> GetAllScenesOfType(Type type, bool includeEnding = false, bool includeWaiting = false)
        {
            List<Scene> scenesOfType;

            if (includeEnding) scenesOfType = sceneStack.Where(scene => scene.GetType().Name == type.Name).OrderByDescending(o => o.priority).ToList();
            else scenesOfType = sceneStack.Where(scene => scene.GetType().Name == type.Name && !scene.transManager.IsEnding).OrderByDescending(o => o.priority).ToList();

            if (includeWaiting) scenesOfType.AddRange(waitingScenes.Where(scene => scene.GetType().Name == type.Name));

            return scenesOfType;
        }

        public static void RemoveAllScenesOfType(Type type, bool includeWaiting = true)
        {
            foreach (Scene scene in GetAllScenesOfType(type: type)) scene.Remove();

            if (includeWaiting) waitingScenes = waitingScenes.Where(scene => scene.GetType() != type).ToList();
        }

        public static Scene GetTopSceneOfType(Type type, bool includeEndingScenes = false, bool includeWaiting = false)
        {
            var foundScenes = GetAllScenesOfType(type: type, includeEnding: includeEndingScenes, includeWaiting: includeWaiting);
            return foundScenes.Count > 0 ? foundScenes[^1] : null;
        }

        public static Scene GetSecondTopSceneOfType(Type type, bool includeEndingScenes = false, bool includeWaiting = false)
        {
            var foundScenes = GetAllScenesOfType(type: type, includeEnding: includeEndingScenes, includeWaiting: includeWaiting);
            return foundScenes.Count > 1 ? foundScenes[^2] : null;
        }

        public static Scene GetBottomSceneOfType(Type type, bool includeEndingScenes = false, bool includeWaiting = false)
        {
            var foundScenes = GetAllScenesOfType(type: type, includeEnding: includeEndingScenes, includeWaiting: includeWaiting);
            return foundScenes.Count > 0 ? foundScenes[0] : null;
        }

        public static void RemoveTopScene()
        {
            Scene scene = sceneStack[^1];
            scene.Remove();
            if (sceneStack.Count > 0) sceneStack[^1].inputActive = true;
        }

        public void MoveToTop()
        {
            sceneStack = sceneStack.Where(scene => scene != this).ToList();
            sceneStack.Add(this);
        }

        public void MoveToBottom()
        {
            sceneStack = sceneStack.Where(scene => scene != this).ToList();
            sceneStack.Insert(0, this);
        }

        public void MoveDown()
        { this.MoveUpOrDown(down: true); }

        public void MoveUp()
        { this.MoveUpOrDown(down: false); }

        private void MoveUpOrDown(bool down)
        {
            int previousItemIndex = 0;

            for (int i = 0; i < sceneStack.Count; i++)
            {
                Scene scene = sceneStack[i];

                if (scene != this) previousItemIndex = i;
                else break;
            }

            if (!down) previousItemIndex = Math.Min(previousItemIndex + 1, sceneStack.Count - 1);

            sceneStack = sceneStack.Where(scene => scene != this).ToList();
            sceneStack.Insert(previousItemIndex, this);
        }

        public void MoveAboveScene(Scene masterScene)
        {
            sceneStack = sceneStack.Where(scene => scene != this).ToList();

            int masterSceneIndex = sceneStack.FindIndex(scene => scene == masterScene);
            if (masterSceneIndex == -1) throw new ArgumentException($"Cannot find master scene '{masterScene.GetType()}'.");

            sceneStack.Insert(masterSceneIndex + 1, this);
        }

        public virtual void Update()
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void RenderToTarget()
        {
            // empty by default
        }

        public virtual void Draw()
        { throw new DivideByZeroException("This method should not be executed."); }

        public static void UpdateAllTransitions()
        {
            // transitions are updated independently from scene updates - to keep the transitions going even when scene.Update() is not processed

            foreach (Scene scene in sceneStack.ToList())
            {
                scene.transManager.Update();
            }
        }

        public static void AllScenesInStackUpdate()
        {
            startUpdateTime = DateTime.Now;
            ProcessingMode = ProcessingModes.Update;

            CheckWaitingScenes(); // has to be check at the start to avoid screen flashing

            if (adaptScenesToNewSize) ResizeAllScenes();

            Scheduler.ProcessQueue();

            UpdateInputActiveTipsTouch();
            Input.UpdateInput();

            currentlyProcessedScene = null;

            foreach (Scene scene in UpdateStack.ToList())
            {
                currentlyProcessedScene = scene;
                Input.InputActive = scene.inputActive && SonOfRobinGame.Game.IsActive;
                scene.Update();
            }

            UpdateAllTransitions();

            if (sceneStack.Count == 0) throw new DivideByZeroException("SceneStack is empty.");

            LastUpdateDuration = UpdateTimeElapsed;
        }

        private static void CheckWaitingScenes()
        {
            var restoredScenesIDs = new List<string>();

            foreach (Scene scene in waitingScenes)
            {
                if (!scene.OtherScenesOfThisTypePresent)
                {
                    scene.Activate();
                    restoredScenesIDs.Add(scene.id);
                }
            }

            if (restoredScenesIDs.Count == 0) return;

            waitingScenes = waitingScenes.Where(scene => !restoredScenesIDs.Contains(scene.id)).ToList();
        }

        public static void AllScenesInStackRenderToTarget(List<Scene> drawStack)
        {
            ProcessingMode = ProcessingModes.RenderToTarget;

            foreach (Scene scene in drawStack)
            {
                scene.RenderToTarget();
            }

            SetRenderTargetToNull();
        }

        public static void AllScenesInStackDraw()
        {
            startDrawTime = DateTime.Now;

            var drawStack = DrawStack;
            AllScenesInStackRenderToTarget(drawStack);

            ProcessingMode = ProcessingModes.Draw;

            currentlyProcessedScene = null;

            foreach (Scene scene in drawStack)
            {
                currentlyProcessedScene = scene;

                scene.Draw();
            }

            LastDrawDuration = DrawTimeElapsed;
        }

        public static void SetRenderTarget(RenderTarget2D newRenderTarget)
        {
            if (ProcessingMode == ProcessingModes.Draw) throw new ArgumentException($"Cannot set RenderTarget during {ProcessingMode}.");

            SonOfRobinGame.GfxDev.SetRenderTarget(newRenderTarget); // do not use SetRenderTarget() anywhere else!
        }

        public static void SetRenderTargetToNull()
        {
            if (ProcessingMode == ProcessingModes.Draw) throw new ArgumentException($"Cannot set RenderTarget during {ProcessingMode}.");

            SonOfRobinGame.GfxDev.SetRenderTarget(null); // do not use SetRenderTarget() anywhere else!
        }
    }
}