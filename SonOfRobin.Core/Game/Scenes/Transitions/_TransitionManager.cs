using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class TransManager
    {
        public readonly Scene scene;
        private List<Transition> transitions;
        public bool sceneRemoved; // needed to prevent from executing any transitions after removing scene

        public bool IsEnding
        {
            get
            {
                foreach (Transition transition in this.transitions)
                { if (transition.endRemoveScene) return true; }
                return false;
            }
        }

        public bool HasAnyTransition { get { return this.transitions.Count > 0; } }
        public TransManager(Scene scene)
        {
            this.scene = scene;
            this.transitions = new List<Transition>();
            this.sceneRemoved = false;
        }

        public void AddTransition(Transition transToAdd)
        {
            World world = World.GetTopWorld();
            if (world != null && world.updateMultiplier > 1)
            {
                int newDuration = Math.Max(transToAdd.Duration / world.updateMultiplier, 5);
                transToAdd.SetNewDuration(newDuration); // to avoid sluggish transitions when updateMultiplier is active
            }

            this.transitions.Add(transToAdd);
        }

        public void AddMultipleTransitions(Dictionary<string, float> paramsToChange, bool outTrans, int duration, int playCount = 1, bool requireInputActiveAtRepeats = false, Scene inputActiveAtRepeatsScene = null, bool startSwapParams = false, bool endTurnOffUpdate = false, bool endTurnOffDraw = false, bool endRemoveScene = false, bool endCopyToBase = false, bool pingPongCycles = true, bool refreshBaseVal = true, int startDelay = 0, int cycleDelay = 0, bool replaceBaseValue = true, Transition.Transform stageTransform = Transition.Transform.Linear, float cycleMultiplier = 1f, bool storeForLaterUse = false)
        {
            // This method parameters should be kept up to date with Transition() constructor

            foreach (var kvp in paramsToChange)
            {
                Transition transition = new Transition(transManager: this, outTrans: outTrans, baseParamName: kvp.Key, targetVal: kvp.Value, duration: duration, playCount: playCount, requireInputActiveAtRepeats: requireInputActiveAtRepeats, inputActiveAtRepeatsScene: inputActiveAtRepeatsScene, startSwapParams: startSwapParams, endTurnOffUpdate: endTurnOffUpdate, endTurnOffDraw: endTurnOffDraw, endRemoveScene: endRemoveScene, endCopyToBase: endCopyToBase, pingPongCycles: pingPongCycles, refreshBaseVal: refreshBaseVal, startDelay: startDelay, cycleDelay: cycleDelay, replaceBaseValue: replaceBaseValue, stageTransform: stageTransform, cycleMultiplier: cycleMultiplier, storeForLaterUse: storeForLaterUse);

                this.AddTransition(transition);
            }
        }

        public void ClearPreviousTransitions(bool copyDrawToBase = true)
        {
            if (copyDrawToBase)
            {
                foreach (Transition transition in this.transitions) transition.CopyDrawToBase();
            }
            this.transitions.Clear();
        }

        public void RemoveTransition(Transition transToRemove)
        {
            this.transitions = this.transitions.Where(transition => transition != transToRemove).ToList();
        }

        public void Update()
        {
            foreach (Transition transition in this.transitions.ToList())
            {
                transition.Update();
                if (this.sceneRemoved) break;
            }
        }

        public float GetTargetValue(string baseParamName)
        {
            foreach (Transition transition in this.transitions)
            {
                if (transition.BaseParamName == baseParamName) return transition.TargetVal;
            }

            return -99999;
        }

    }
}
