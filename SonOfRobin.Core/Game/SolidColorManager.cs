using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SolidColorManager
    {
        private readonly Scene parentScene;
        private List<Scene> solidColors;

        public SolidColorManager(Scene scene)
        {
            this.parentScene = scene;
            this.solidColors = new List<Scene>();
        }

        public bool AnySolidColorPresent
        {
            get
            {
                this.CleanRemovedScenes();
                return this.solidColors.Count > 0;
            }
        }

        public void Add(SolidColor solidColor)
        {
            solidColor.MoveAboveScene(this.parentScene);

            this.solidColors.Add(solidColor);
            this.parentScene.AddLinkedScene(solidColor);

            this.CleanRemovedScenes();
        }

        private void CleanRemovedScenes()
        {
            this.solidColors = this.solidColors.Where(solidColor => !solidColor.HasBeenRemoved).ToList();
        }

        public void RemoveAll(int fadeOutDuration = 0)
        {
            foreach (Scene scene in this.solidColors)
            {
                if (fadeOutDuration > 0)
                {
                    scene.transManager.AddTransition(new Transition(transManager: scene.transManager, outTrans: true, duration: fadeOutDuration, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: 0.0f, endRemoveScene: true));
                }
                else scene.Remove();
            }
        }
    }
}