using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;
using System;

namespace SonOfRobin
{
    public class StartLogo : Scene
    {
        private readonly Texture2D logoTexture;
        private readonly Texture2D distortionTexture;
        private readonly Tweener tweener;
        private readonly MainLogoInstance logoEffectInstance;

        //public float logoEffectIntensity;
        public float logoEffectOffsetY;

        private readonly Random random;
        private float waveDirectionMultiplier;

        public StartLogo() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.StartGame, tipsLayout: ControlTips.TipsLayout.StartGame)
        {
            this.random = new Random();

            this.waveDirectionMultiplier = 1f;

            this.logoTexture = TextureBank.GetTexture(TextureBank.TextureName.GameLogo);
            this.distortionTexture = TextureBank.GetTexture(TextureBank.TextureName.LogoWaveDistortion);
            this.logoEffectInstance = new MainLogoInstance(baseTexture: this.logoTexture, distortTexture: this.distortionTexture);
            this.logoEffectInstance.intensityForTweener = 0f;
            this.viewParams.Opacity = 0f;

            this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 1f, duration: 90, startSwapParams: true));

            this.tweener = new Tweener();

            this.SetTweener(delaySeconds: 0.01f);
        }

        private void SetTweener(float delaySeconds = 0f)
        {
            if (this.logoEffectInstance.intensityForTweener > 0) return; // exit at half cycle (before completing autoreverse)

            this.waveDirectionMultiplier *= -1f;
            if (delaySeconds == 0 && this.waveDirectionMultiplier == -1f) delaySeconds = this.random.Next(0, 6);
            float duration = (this.random.NextSingle() * 2f) + 0.5f;
            float intensity = (this.random.NextSingle() * 1.2f) + 0.3f;

            MessageLog.Add(debugMessage: true, text: $"{SonOfRobinGame.CurrentUpdate} setting tweener - delaySeconds: {delaySeconds}, duration: {duration}, intensity: {intensity}");

            this.tweener.TweenTo(target: this.logoEffectInstance, expression: logoEffectInstance => logoEffectInstance.intensityForTweener, toValue: intensity, duration: duration, delay: delaySeconds)
            .Easing(EasingFunctions.SineInOut)
            .AutoReverse()
            .OnEnd(t => this.SetTweener());
        }

        private static int LogoHeight { get { return (int)(SonOfRobinGame.ScreenHeight * 0.33f); } }

        public override void Update()
        {
            this.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);

            if (this.logoEffectInstance.intensityForTweener == 0) this.logoEffectOffsetY = 0;

            this.logoEffectOffsetY += 0.01f * this.waveDirectionMultiplier;

            if (InputMapper.IsPressed(InputMapper.Action.GlobalConfirm) || TouchInput.IsBeingTouchedInAnyWay)
            {
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                Sound.QuickPlay(SoundData.Name.TurnPage);

                this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 0f, duration: 30, startSwapParams: true));
            }

            if (this.viewParams.Opacity == 0 && this.inputActive)
            {
                this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 1f, duration: 30, startSwapParams: true));
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

            this.logoEffectInstance.distortTextureOffset = new Vector2(0f, this.logoEffectOffsetY);
            this.logoEffectInstance.TurnOn(currentUpdate: SonOfRobinGame.CurrentUpdate, drawColor: Color.White);

            int logoHeight = LogoHeight;

            Rectangle imageRect = new(x: 0, y: (int)(logoHeight * 0.1f), width: SonOfRobinGame.ScreenWidth, height: logoHeight);
            Helpers.DrawTextureInsideRect(texture: this.logoTexture, rectangle: imageRect, color: Color.White * this.viewParams.drawOpacity);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}