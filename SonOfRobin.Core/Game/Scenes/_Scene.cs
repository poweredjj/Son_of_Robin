using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public abstract class Scene
    {
        public enum InputTypes
        {
            None,
            Always,
            Normal
        }

        public enum InventoryLayout
        {
            None,
            Toolbar,
            InventoryAndToolbar,
            InventoryAndChest,
            InventoryAndEquip
        }

        public static List<Scene> sceneStack = new List<Scene> { };
        public static InventoryLayout inventoryLayout = InventoryLayout.None;

        public readonly int priority;
        public bool blocksUpdatesBelow;
        public bool blocksDrawsBelow;
        public bool hidesSameScenesBelow;

        public bool drawActive;
        public bool updateActive;
        public bool inputActive;
        public TouchLayout touchLayout;
        public ControlTips.TipsLayout tipsLayout;

        private readonly bool alwaysUpdates;
        private readonly bool alwaysDraws;

        public ViewParams viewParams;
        protected Transition transition;

        private readonly int sceneID;
        private static int sceneIdCounter = 0;

        public static bool processingUpdate; // true = update, false = draw
        public static DateTime startUpdateTime;

        private InputTypes inputType;

        public InputTypes InputType
        {
            get { return inputType; }
            set { this.inputType = value; }
        }
        public static List<Scene> UpdateStack
        {
            get
            {
                var createdStack = new List<Scene> { };
                if (sceneStack.Count == 0) return createdStack;

                bool ignoreScenes = false;
                for (int i = sceneStack.Count - 1; i >= 0; i--)
                {
                    Scene scene = sceneStack[i];
                    if ((!ignoreScenes && scene.updateActive) || scene.alwaysUpdates) createdStack.Add(scene);
                    if (scene.blocksUpdatesBelow) ignoreScenes = true;
                }

                createdStack.Reverse();
                return createdStack;
            }
        }

        public static List<Scene> DrawStack
        {
            get
            {
                var createdStack = new List<Scene> { };
                if (sceneStack.Count == 0) return createdStack;

                bool ignoreScenes = false;
                for (int i = sceneStack.Count - 1; i >= 0; i--)
                {
                    Scene scene = sceneStack[i];
                    if ((!ignoreScenes && scene.drawActive) || scene.alwaysDraws) createdStack.Add(scene);

                    if (scene.blocksDrawsBelow) ignoreScenes = true;
                }

                createdStack.Reverse();
                createdStack = createdStack.OrderByDescending(o => o.priority).ToList();

                return createdStack;
            }
        }

        public static List<Scene> TransitionStack
        {
            get
            {
                var createdStack = new List<Scene> { };
                foreach (Scene scene in sceneStack)
                { if (scene.transition != null) createdStack.Add(scene); }

                return createdStack;
            }
        }

        public Scene(InputTypes inputType, TouchLayout touchLayout, ControlTips.TipsLayout tipsLayout, int priority = 1, bool blocksUpdatesBelow = false, bool blocksDrawsBelow = false, bool alwaysUpdates = false, bool alwaysDraws = false, bool hidesSameScenesBelow = false)
        {
            this.viewParams = new ViewParams();
            this.priority = priority;
            this.blocksUpdatesBelow = blocksUpdatesBelow;
            this.blocksDrawsBelow = blocksDrawsBelow;
            this.hidesSameScenesBelow = hidesSameScenesBelow;

            this.InputType = inputType;
            this.updateActive = true;
            this.drawActive = true;
            this.touchLayout = touchLayout;
            this.tipsLayout = tipsLayout;

            this.alwaysUpdates = alwaysUpdates;
            this.alwaysDraws = alwaysDraws;
            this.sceneID = sceneIdCounter;
            sceneIdCounter++;

            if (this.hidesSameScenesBelow) this.HideSameScenesBelow();

            sceneStack.Add(this);
        }

        public virtual void Remove()
        {
            sceneStack = sceneStack.Where(scene => scene.sceneID != this.sceneID).ToList();
            if (this.hidesSameScenesBelow) this.ShowTopSceneOfSameType();
        }

        public void AddTransition(Transition transition)
        {
            this.transition = transition;
            transition.Update();
        }

        public void RemoveTransition()
        { this.transition = null; }


        private void HideSameScenesBelow()
        {
            var existingScenesOfSameType = Scene.GetAllScenesOfType(this.GetType());
            foreach (Scene scene in existingScenesOfSameType)
            {
                if (scene != this) scene.drawActive = false;
            }
        }

        private void ShowTopSceneOfSameType()
        {
            var scene = (Menu)Scene.GetTopSceneOfType(this.GetType());
            if (scene != null) scene.drawActive = true;
        }

        protected Scene GetSceneBelow(bool ignorePriority0 = true)
        {
            Scene previousScene = null;
            foreach (Scene scene in sceneStack)
            {
                if (scene.transition != null && scene.transition.removeScene) continue;
                if (scene.priority == 0 && ignorePriority0) continue;
                if (scene == this) return previousScene;
                previousScene = scene;
            }

            return null;
        }

        private static void UpdateInputActive()
        {
            bool normalInputSet = false;
            ControlTips topTips = ControlTips.GetTopTips();

            for (int i = sceneStack.Count - 1; i >= 0; i--)
            {
                Scene scene = sceneStack[i];

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
                            if (topTips != null)
                            {
                                topTips.SwitchToLayout(scene.tipsLayout);
                                topTips.currentScene = scene;
                            }

                            normalInputSet = true;
                        }
                        else
                        { scene.inputActive = false; }
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported inputType - {scene.InputType}.");
                }
            }

            if (!normalInputSet)
            {
                TouchInput.SwitchToLayout(TouchLayout.Empty);
                topTips?.SwitchToLayout(ControlTips.TipsLayout.Empty);
            }
        }

        public static List<Scene> GetAllScenesOfType(Type type)
        { return sceneStack.Where(scene => scene.GetType().Name == type.Name && (scene.transition == null || !scene.transition.removeScene)).ToList(); }

        public static void RemoveAllScenesOfType(Type type)
        {
            foreach (Scene scene in GetAllScenesOfType(type: type))
            { scene.Remove(); }
        }

        public static Scene GetTopSceneOfType(Type type)
        {
            var foundScenes = GetAllScenesOfType(type);
            return foundScenes.Count > 0 ? foundScenes[foundScenes.Count - 1] : null;
        }

        public static Scene GetSecondTopSceneOfType(Type type)
        {
            var foundScenes = GetAllScenesOfType(type);
            return foundScenes.Count > 1 ? foundScenes[foundScenes.Count - 2] : null;
        }

        public static Scene GetBottomSceneOfType(Type type)
        {
            var foundScenes = GetAllScenesOfType(type);
            return foundScenes.Count > 0 ? foundScenes[0] : null;
        }

        public static void RemoveTopScene()
        {
            Scene scene = sceneStack[sceneStack.Count - 1];
            scene.Remove();
            if (sceneStack.Count > 0) sceneStack[sceneStack.Count - 1].inputActive = true;
        }

        public void MoveToTop()
        {
            sceneStack = sceneStack.Where(scene => scene != this).ToList();
            sceneStack.Add(this);
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

                if (scene != this)
                { previousItemIndex = i; }
                else
                { break; }
            }

            if (!down) previousItemIndex = Math.Min(previousItemIndex + 1, sceneStack.Count - 1);

            sceneStack = sceneStack.Where(scene => scene != this).ToList();
            sceneStack.Insert(previousItemIndex, this);
        }

        public static void SetInventoryLayout(InventoryLayout newLayout, BoardPiece chest = null, Player player = null)
        {

            var invScenes = Scene.GetAllScenesOfType(typeof(Inventory));
            foreach (Scene scene in invScenes)
            {
                Inventory invScene = (Inventory)scene;
                invScene.Remove();
            }

            switch (newLayout)
            {
                case InventoryLayout.None:
                    break;

                case InventoryLayout.Toolbar:
                    {
                        new Inventory(piece: player, storage: player.toolStorage, layout: Inventory.Layout.SingleBottom, inputType: InputTypes.Always);

                        break;
                    }

                case InventoryLayout.InventoryAndToolbar:
                    {
                        Inventory toolbar = new Inventory(piece: player, storage: player.toolStorage, layout: Inventory.Layout.DualBottom);
                        Inventory inventory = new Inventory(piece: player, storage: player.pieceStorage, layout: Inventory.Layout.DualTop, otherInventory: toolbar);
                        toolbar.otherInventory = inventory;

                        break;
                    }

                case InventoryLayout.InventoryAndChest:
                    {
                        Inventory inventoryLeft = new Inventory(piece: player, storage: player.pieceStorage, layout: Inventory.Layout.DualLeft, blocksUpdatesBelow: false);
                        Inventory inventoryRight = new Inventory(piece: chest, storage: chest.pieceStorage, layout: Inventory.Layout.DualRight, blocksUpdatesBelow: false, otherInventory: inventoryLeft);
                        inventoryLeft.otherInventory = inventoryRight;

                        break;
                    }

                case InventoryLayout.InventoryAndEquip:
                    {
                        Inventory inventoryLeft = new Inventory(piece: player, storage: player.pieceStorage, layout: Inventory.Layout.DualLeft, blocksUpdatesBelow: false);
                        Inventory inventoryRight = new Inventory(piece: player, storage: player.equipStorage, layout: Inventory.Layout.DualRight, blocksUpdatesBelow: false, otherInventory: inventoryLeft);
                        inventoryLeft.otherInventory = inventoryRight;

                        break;
                    }

                default:
                    throw new DivideByZeroException($"Unknown inventory layout '{newLayout}'.");
            }

            inventoryLayout = newLayout;
        }

        public virtual void Update(GameTime gameTime)
        { throw new DivideByZeroException("This method should not be executed."); }

        public virtual void Draw()
        { throw new DivideByZeroException("This method should not be executed."); }

        public static void UpdateAllTransitions()
        {
            // transitions are updated independently from scene updates - to keep the transitions going even when scene.Update() is not processed

            foreach (Scene scene in sceneStack)
            { scene.viewParams.CopyBaseToDrawParams(); } // all draw params should be reset at the start

            foreach (Scene scene in TransitionStack)
            { scene.transition.Update(); }
        }

        public static void UpdateAllScenesInStack(GameTime gameTime)
        {
            processingUpdate = true;
            startUpdateTime = DateTime.Now;

            Scheduler.ProcessQueue();

            Input.UpdateInput(gameTime: gameTime);
            UpdateInputActive();

            foreach (Scene scene in UpdateStack)
            {
                Input.InputActive = scene.inputActive;
                scene.Update(gameTime: gameTime);
            }

            UpdateAllTransitions();

            if (sceneStack.Count == 0) throw new DivideByZeroException("SceneStack is empty.");
        }

        public static void DrawAllScenesInStack()
        {
            processingUpdate = false;

            SonOfRobinGame.graphicsDevice.SetRenderTarget(null); // setting rendertarget to backbuffer

            Scene previousScene = DrawStack[0];

            bool spriteBatchNotEnded = false;
            bool firstSpriteBatchStarted = false;

            foreach (Scene scene in DrawStack)
            {
                bool createNewMatrix = !firstSpriteBatchStarted ||
                                      (!(scene.viewParams.DrawPos == previousScene.viewParams.DrawPos &&
                                         scene.viewParams.drawRot == previousScene.viewParams.drawRot &&
                                         scene.viewParams.drawScaleX == previousScene.viewParams.drawScaleX &&
                                          scene.viewParams.drawScaleY == previousScene.viewParams.drawScaleY
                                         ));

                if (!firstSpriteBatchStarted || createNewMatrix)
                {

                    Matrix scaleMatrix = Matrix.CreateScale(
                                        (float)SonOfRobinGame.graphics.PreferredBackBufferWidth / SonOfRobinGame.VirtualWidth / scene.viewParams.drawScaleX,
                                        (float)SonOfRobinGame.graphics.PreferredBackBufferHeight / SonOfRobinGame.VirtualHeight / scene.viewParams.drawScaleY,
                                        1f);

                    Matrix rotationMatrix = Matrix.CreateRotationZ(scene.viewParams.drawRot);
                    Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(scene.viewParams.drawPosX, scene.viewParams.drawPosY, 0));

                    // needed for rotation to center around the camera
                    Matrix translateToOrigin = Matrix.CreateTranslation(-(float)scene.viewParams.drawWidth / 2f, -(float)scene.viewParams.drawHeight / 2f, 0);
                    // needed for rotation to center around the camera
                    Matrix translateBackToPosition = Matrix.CreateTranslation((float)scene.viewParams.drawWidth / 2f, (float)scene.viewParams.drawHeight / 2f, 0);

                    Matrix compositeMatrix = translateToOrigin * rotationMatrix * translateBackToPosition * translationMatrix * scaleMatrix;

                    if (spriteBatchNotEnded) SonOfRobinGame.spriteBatch.End();

                    SonOfRobinGame.spriteBatch.Begin(transformMatrix: compositeMatrix, samplerState: Microsoft.Xna.Framework.Graphics.SamplerState.AnisotropicClamp);
                    spriteBatchNotEnded = true;
                    firstSpriteBatchStarted = true;
                }

                scene.Draw();

                previousScene = scene;
            }

            if (spriteBatchNotEnded) SonOfRobinGame.spriteBatch.End();
        }

    }
}
