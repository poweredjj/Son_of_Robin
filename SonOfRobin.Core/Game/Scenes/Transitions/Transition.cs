using System;

namespace SonOfRobin
{
    public class Transition
    {
        public enum Transform { Linear, Sinus, Cosinus }

        private readonly TransManager transManager;
        private readonly Scene scene;
        private readonly ViewParams viewParams;

        private bool outTrans;

        private readonly string baseParamName;
        public string BaseParamName { get { return baseParamName; } }
        private readonly string drawParamName;
        private float sourceVal;
        private float targetVal;
        private Transform stageTransform;
        public float TargetVal { get { return targetVal; } }
        private readonly bool refreshBaseVal;
        private readonly bool replaceBaseValue;

        private readonly int duration;
        private int cyclesLeft;
        private readonly bool pingPongCycles;
        private readonly float cycleMultiplier;
        private int sameAsSourceCount;
        private int endFrame;
        private int cycleStartFrame;
        private readonly int startDelay;
        private int cycleDelay;

        private readonly bool requireInputActiveAtRepeats;

        private readonly bool endTurnOffUpdate;
        private readonly bool endTurnOffDraw;
        public readonly bool endRemoveScene;
        private readonly bool endCopyToBase;

        private float SourceVal
        {
            get { return this.refreshBaseVal ? (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: this.baseParamName) : this.sourceVal; }
            set { this.sourceVal = value; }
        }

        public Transition(TransManager transManager, bool outTrans, string baseParamName, float targetVal, int duration, int playCount = 1, bool requireInputActiveAtRepeats = false, bool startSwapParams = false, bool endTurnOffUpdate = false, bool endTurnOffDraw = false, bool endRemoveScene = false, bool endCopyToBase = false, bool pingPongCycles = true, bool refreshBaseVal = true, int startDelay = 0, int cycleDelay = 0, bool replaceBaseValue = true, Transform stageTransform = Transform.Linear, float cycleMultiplier = 1f, bool storeForLaterUse = false)
        // Every parameter above should be put into TransManager.AddMultipleTransitions() method.

        {
            this.transManager = transManager;
            this.scene = this.transManager.scene;
            this.viewParams = scene.viewParams;
            this.startDelay = startDelay;
            this.cycleDelay = cycleDelay;
            this.cycleStartFrame = storeForLaterUse ? -1 : SonOfRobinGame.currentUpdate + this.startDelay + this.cycleDelay;
            this.cycleMultiplier = cycleMultiplier;
            this.sameAsSourceCount = 0;
            this.outTrans = outTrans; // true: moving "out" of base param, false: moving back in direction of base param
            this.baseParamName = baseParamName;
            this.refreshBaseVal = refreshBaseVal; // true: refreshing base value every frame, false: or keeping the starting value for the whole transition
            this.replaceBaseValue = replaceBaseValue;
            this.SourceVal = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: this.baseParamName);
            this.drawParamName = $"draw{Helpers.FirstCharToUpperCase(this.baseParamName)}";
            this.targetVal = targetVal;
            this.stageTransform = stageTransform;
            this.duration = duration;
            this.cyclesLeft = playCount; // -1 == infinite
            this.requireInputActiveAtRepeats = requireInputActiveAtRepeats; // will pause at the end of every cycle, waiting for user input to be active
            this.pingPongCycles = pingPongCycles; // every second cycle will be backwards

            this.endTurnOffUpdate = endTurnOffUpdate;
            this.endTurnOffDraw = endTurnOffDraw;
            this.endRemoveScene = endRemoveScene;
            this.endCopyToBase = endCopyToBase;

            if (this.endRemoveScene)
            {
                this.scene.blocksDrawsBelow = false;
                this.scene.blocksUpdatesBelow = false;
                this.scene.inputActive = false; // scene that is being removed, should not accept any input
                this.scene.InputType = Scene.InputTypes.None;
                this.scene.touchLayout = TouchLayout.Empty;
            }

            if (startSwapParams) this.SwapBaseTargetParams();

            //  MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.User, message: $"New trans: {this.scene.GetType()} {this.baseParamName} {this.sourceVal} -> {this.targetVal}", color: Color.LightCyan); // for testing

            if (!storeForLaterUse) this.Update();
        }

        private void UpdateEndFrame()
        {
            this.endFrame = SonOfRobinGame.currentUpdate + this.duration;
        }

        private float ComputeStageTransform(float inputValue)
        {
            switch (this.stageTransform)
            {
                case Transform.Linear:
                    return inputValue;

                case Transform.Sinus:
                    return (float)Math.Sin(Math.PI * 2 * inputValue);

                case Transform.Cosinus:
                    return (float)Math.Cos(Math.PI * 2 * inputValue);

                default:
                    throw new ArgumentException($"Unsupported targetTransform - '{stageTransform}'.");
            }
        }

        public void Update()
        {
            if (this.cycleStartFrame == -1) this.cycleStartFrame = SonOfRobinGame.currentUpdate + this.startDelay + this.cycleDelay;
            if (SonOfRobinGame.currentUpdate < this.cycleStartFrame) return;
            if (SonOfRobinGame.currentUpdate == this.cycleStartFrame) this.UpdateEndFrame();

            float antiTransStage = ((float)this.endFrame - (float)SonOfRobinGame.currentUpdate) / (float)this.duration;
            antiTransStage = Math.Max(antiTransStage, 0f);
            if (!this.outTrans) antiTransStage = 1f - antiTransStage;

            float transStage = 1f - antiTransStage;
            if (!this.replaceBaseValue) antiTransStage = 1f;

            float targetValue = (this.SourceVal * antiTransStage) + (this.targetVal * this.ComputeStageTransform(transStage));
            Helpers.SetProperty(targetObj: this.viewParams, propertyName: this.drawParamName, newValue: targetValue);

            if (SonOfRobinGame.currentUpdate >= this.endFrame) this.StartNextCycle();
        }

        private void StartNextCycle()
        {
            bool paused = this.requireInputActiveAtRepeats && !this.scene.inputActive;
            if (paused) return;

            this.cycleStartFrame = SonOfRobinGame.currentUpdate + this.cycleDelay + 1;

            bool repeat;

            if (this.cyclesLeft == -1) repeat = true;
            else
            {
                this.cyclesLeft--;
                repeat = cyclesLeft != 0;
            }

            if (repeat)
            {
                if (this.pingPongCycles) this.outTrans = !this.outTrans;
                this.targetVal *= this.cycleMultiplier;

                bool sameAsSource = Math.Round(this.targetVal, 4) == Math.Round(this.sourceVal, 4);
                if (sameAsSource) this.sameAsSourceCount++;
                else this.sameAsSourceCount = 0;

                if (this.sameAsSourceCount >= 3)
                {
                    MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Transition removed - target value is the same as source.");
                    this.Finish();
                }
            }
            else this.Finish();
        }

        private void Finish()
        {
            if (this.endCopyToBase) this.CopyTargetToBase();
            if (this.endTurnOffUpdate) this.scene.updateActive = false;
            if (this.endTurnOffDraw) this.scene.drawActive = false;
            if (this.endRemoveScene)
            {
                this.transManager.sceneRemoved = true;
                this.scene.Remove();
            }

            this.transManager.RemoveTransition(this);
        }

        private void SwapBaseTargetParams()
        {
            float baseVal = (float)this.SourceVal;

            Helpers.SetProperty(targetObj: this.viewParams, propertyName: this.baseParamName, newValue: this.targetVal);
            this.SourceVal = this.targetVal;
            this.targetVal = baseVal;
        }

        private void CopyTargetToBase()
        {
            Helpers.SetProperty(targetObj: this.viewParams, propertyName: this.baseParamName, newValue: this.targetVal);
            this.SourceVal = this.targetVal;
        }

        public void CopyDrawToBase()
        {
            float drawVal = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: this.drawParamName);
            Helpers.SetProperty(targetObj: this.viewParams, propertyName: this.baseParamName, newValue: drawVal);
            this.SourceVal = drawVal;
        }

    }
}