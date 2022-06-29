using System.Collections.Generic;

namespace SonOfRobin
{
    public class Transition
    {
        public enum TransType { To, From, PingPong }

        private TransType type;
        protected bool moveForward;

        protected readonly Scene scene;
        protected readonly ViewParams viewParams;
        private readonly bool blockInput;
        public readonly bool removeScene;
        public readonly bool copyToBaseAtTheEnd;
        private readonly bool turnOffUpdate;
        private readonly bool turnOffDraw;
        private readonly bool pingPongPause;

        private readonly int duration;
        private int startFrame;
        private int endFrame;
        private bool paused;

        private readonly Scene.InputTypes orgSceneInputType;
        private readonly Dictionary<string, float> paramsToChange;
        private readonly Dictionary<string, float> pausedParams;

        private bool Paused
        {
            get { return this.paused; }
            set
            {
                this.paused = value;
                if (!this.paused) SetStartEndFrames();
            }
        }
        protected float TransitionStage { get { return ((float)this.endFrame - (float)SonOfRobinGame.currentUpdate) / (float)this.duration; } }
        public Transition(TransType type, Scene scene, int duration, Dictionary<string, float> paramsToChange, bool blockInput = false, bool removeScene = false, bool copyToBaseAtTheEnd = false, bool pingPongPause = false, bool turnOffUpdate = false, bool turnOffDraw = false)
        {
            this.type = type;
            this.moveForward = type != TransType.From;

            this.duration = duration;
            this.paramsToChange = paramsToChange;

            this.pausedParams = new Dictionary<string, float> { };
            foreach (string param in paramsToChange.Keys)
            { this.pausedParams[param] = 0f; }

            this.scene = scene;
            this.viewParams = scene.viewParams;
            this.orgSceneInputType = this.scene.InputType;

            this.pingPongPause = pingPongPause;
            this.blockInput = blockInput;
            this.removeScene = removeScene;
            this.copyToBaseAtTheEnd = copyToBaseAtTheEnd;
            this.turnOffUpdate = turnOffUpdate;
            this.turnOffDraw = turnOffDraw;

            if (this.blockInput)
            {
                this.scene.inputActive = false; // to prevent any input until the end of this scene update
                this.scene.InputType = Scene.InputTypes.None;
            }

            this.paused = false;
            SetStartEndFrames();

            if (this.removeScene)
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
            if (this.Paused && this.scene.inputActive) this.Paused = false;

            float transStage = this.moveForward ? 1f - this.TransitionStage : this.TransitionStage;
            float antiTransStage = 1f - transStage;

            string sourceParamName, destParamName;

            foreach (var kvp in this.paramsToChange)
            {
                sourceParamName = kvp.Key;
                destParamName = $"draw{Helpers.FirstCharToUpperCase(kvp.Key.ToString())}";

                if (this.Paused)
                { Helpers.SetProperty(targetObj: this.viewParams, propertyName: destParamName, newValue: pausedParams[sourceParamName]); }
                else
                {
                    float paramTransValue = kvp.Value;
                    float sourceValue = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: sourceParamName);
                    float targetValue = (paramTransValue * transStage) + (sourceValue * antiTransStage);
                    this.pausedParams[sourceParamName] = targetValue;
                    Helpers.SetProperty(targetObj: this.viewParams, propertyName: destParamName, newValue: targetValue);
                }
            }

            if (!this.Paused && SonOfRobinGame.currentUpdate >= this.endFrame) this.Finish();
        }

        private void CopyDrawToBaseParams()
        {
            string sourceParamName, destParamName;

            foreach (var kvp in this.paramsToChange)
            {
                destParamName = kvp.Key;
                sourceParamName = $"draw{Helpers.FirstCharToUpperCase(kvp.Key.ToString())}";

                float sourceValue = (float)Helpers.GetProperty(targetObj: this.viewParams, propertyName: sourceParamName);
                Helpers.SetProperty(targetObj: this.viewParams, propertyName: destParamName, newValue: sourceValue);
            }
        }

        private void Finish()
        {
            if (this.type == TransType.PingPong)
            {
                this.moveForward = !this.moveForward;
                this.type = TransType.From;
                this.Paused = this.pingPongPause;
                return;
            }

            this.Paused = true;

            if (this.copyToBaseAtTheEnd) this.CopyDrawToBaseParams();
            if (this.turnOffUpdate) this.scene.updateActive = false;
            if (this.turnOffDraw) this.scene.drawActive = false;
            if (this.removeScene) this.scene.Remove();
            if (this.blockInput) this.scene.InputType = this.orgSceneInputType;
            this.scene.RemoveTransition(); // must go last, otherwise scene.Remove() will be stuck in a loop
        }
    }
}
