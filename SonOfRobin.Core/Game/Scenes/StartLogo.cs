using Microsoft.Xna.Framework;
using MonoGame.Extended.Tweening;

namespace SonOfRobin
{
    public class StartLogo : Scene
    {
        private readonly Tweener tweener;
        public float logoOffsetY;

        public StartLogo() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.StartGame, tipsLayout: ControlTips.TipsLayout.StartGame)
        {
            this.tweener = new Tweener();

            this.logoOffsetY = -LogoHeight * 1.5f;

            this.tweener.TweenTo(target: this, expression: scene => scene.logoOffsetY, toValue: 0f, duration: 3.5f, delay: 1)
                .Easing(EasingFunctions.BounceOut);
        }

        private static int LogoHeight { get { return (int)(SonOfRobinGame.ScreenHeight * 0.33f); } }

        public override void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            if (InputMapper.IsPressed(InputMapper.Action.GlobalConfirm) || TouchInput.IsBeingTouchedInAnyWay)
            {
                SolidColor colorOverlay = new(color: Color.White, viewOpacity: 0f, clearScreen: false, priority: 1);
                colorOverlay.transManager.AddTransition(new Transition(transManager: colorOverlay.transManager, outTrans: true, startDelay: 0, duration: 60, playCount: 1, stageTransform: Transition.Transform.Linear, baseParamName: "Opacity", targetVal: 0.4f));

                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                Sound.QuickPlay(SoundData.Name.TurnPage);

                this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: 30, endRemoveScene: true, startSwapParams: true));
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            int logoHeight = LogoHeight;

            Rectangle imageRect = new(x: 0, y: (int)(logoHeight * 0.1f) + (int)this.logoOffsetY, width: SonOfRobinGame.ScreenWidth, height: logoHeight);
            Helpers.DrawTextureInsideRect(texture: TextureBank.GetTexture(TextureBank.TextureName.GameLogo), rectangle: imageRect, color: Color.White * this.viewParams.drawOpacity);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}