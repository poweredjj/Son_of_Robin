using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class TransitionNew
    {
        private readonly Scene scene;
        private readonly ViewParams viewParams;

        private readonly Scene.InputTypes orgSceneInputType;
        private readonly Dictionary<string, float> paramsToChange;
        private readonly int duration;
        private int repeatCount;

        private bool paused;

        public readonly bool endRemoveScene;
        public readonly bool startSwapParams;
        public readonly bool endRevertToBase;
        private readonly bool startBlockInput;
        private readonly bool requireInputActiveAtRepeats;
        private readonly bool endTurnOffUpdate;
        private readonly bool endTurnOffDraw;

        private int startFrame;
        private int endFrame;
        private float TransitionStage
        {
            get
            {
                float transitionStage = ((float)this.endFrame - (float)SonOfRobinGame.currentUpdate) / (float)this.duration;
                transitionStage = Math.Min(transitionStage, 1f);
                return transitionStage;
            }
        }

        public TransitionNew(Scene scene, int duration, Dictionary<string, float> paramsToChange, bool startBlockInput = false, bool endRemoveScene = false, bool endTurnOffUpdate = false, bool endTurnOffDraw = false, bool startSwapParams = false, bool endRevertToBase = false, int repeatCount = 1, bool requireInputActiveAtRepeats = false)
        {
            this.scene = scene;
            this.viewParams = scene.viewParams;
            this.duration = duration;
            this.paramsToChange = paramsToChange;
            this.repeatCount = repeatCount;
            this.requireInputActiveAtRepeats = requireInputActiveAtRepeats;

            this.startSwapParams = startSwapParams;
            this.startBlockInput = startBlockInput;
            if (this.startBlockInput)
            {
                this.scene.inputActive = false; // to prevent any input until the end of this scene update
                this.scene.InputType = Scene.InputTypes.None;
            }

            this.endRevertToBase = endRevertToBase;
            this.endRemoveScene = endRemoveScene;
            this.endTurnOffUpdate = endTurnOffUpdate;
            this.endTurnOffDraw = endTurnOffDraw;

            this.SetStartEndFrames();

            if (this.endRemoveScene)
            {
                this.scene.blocksDrawsBelow = false;
                this.scene.blocksUpdatesBelow = false;
                this.scene.InputType = Scene.InputTypes.None;
                this.scene.touchLayout = TouchLayout.Empty;
            }
        }

        private void SetStartEndFrames()
        {
            this.startFrame = SonOfRobinGame.currentUpdate;
            this.endFrame = this.startFrame + this.duration;
        }

        public void Update()
        {
            float transStage = this.TransitionStage;
            float antiTransStage = 1f - transStage;

            string sourceParamName, destParamName;

            foreach (var kvp in this.paramsToChange)
            {
                sourceParamName = kvp.Key;
                destParamName = $"draw{Helpers.FirstCharToUpperCase(kvp.Key.ToString())}";

                float paramTransValue = kvp.Value;
                float sourceValue = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: sourceParamName);
                float targetValue = (paramTransValue * transStage) + (sourceValue * antiTransStage);
                Helpers.SetProperty(targetObj: this.viewParams, propertyName: destParamName, newValue: targetValue);
            }

            if (SonOfRobinGame.currentUpdate >= this.endFrame) this.StartNextCycle();
        }

        private void StartNextCycle()
        {
            bool paused = this.requireInputActiveAtRepeats && !this.scene.inputActive;
            if (paused) return;


            bool repeat;

            if (this.repeatCount == -1) repeat = true;
            else
            {
                this.repeatCount--;
                repeat = repeatCount != 0;
            }

            if (repeat)
            {
                if (this.endRevertToBase) this.SwapStartWithEnd();
                this.SetStartEndFrames();


            }
            else this.Finish();
        }

        private void Finish()
        {
            // TODO add this and verify this whole class
        }

        private void SwapStartWithEnd()
        {
            string drawParamName;

            foreach (string baseParamName in this.paramsToChange.Keys.ToList())
            {
                drawParamName = $"draw{Helpers.FirstCharToUpperCase(baseParamName)}";
                float drawValue = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: drawParamName);
                Helpers.SetProperty(targetObj: this.viewParams, propertyName: baseParamName, newValue: this.paramsToChange[baseParamName]);
                this.paramsToChange[baseParamName] = drawValue;
            }
        }


        private void CopyDrawToBaseParams()
        {
            string sourceParamName;

            foreach (string destParamName in this.paramsToChange.Keys)
            {
                sourceParamName = $"draw{Helpers.FirstCharToUpperCase(destParamName)}";

                float sourceValue = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: sourceParamName);
                Helpers.SetProperty(targetObj: this.viewParams, propertyName: destParamName, newValue: sourceValue);
            }
        }

        private void CopyBaseToDrawParams()
        {
            string destParamName;

            foreach (string sourceParamName in this.paramsToChange.Keys)
            {
                destParamName = $"draw{Helpers.FirstCharToUpperCase(sourceParamName)}";

                float sourceValue = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: sourceParamName);
                Helpers.SetProperty(targetObj: this.viewParams, propertyName: destParamName, newValue: sourceValue);
            }
        }

    }
}
