using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class StartLogo : Scene
    {
        public StartLogo() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.StartGame, tipsLayout: ControlTips.TipsLayout.StartGame)
        {
        }

        public override void Update()
        {
            if (InputMapper.IsPressed(InputMapper.Action.GlobalConfirm) || TouchInput.IsBeingTouchedInAnyWay)
            {
                SolidColor colorOverlay = new(color: Color.White, viewOpacity: 0f, clearScreen: false, priority: 1);
                colorOverlay.transManager.AddTransition(new Transition(transManager: colorOverlay.transManager, outTrans: true, startDelay: 0, duration: 60, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: 0.4f));

                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                Sound.QuickPlay(SoundData.Name.TurnPage);

                this.Remove();
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            // TODO add drawing logo

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}