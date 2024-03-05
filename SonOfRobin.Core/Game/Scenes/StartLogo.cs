using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class StartLogo : Scene
    {
        private readonly Texture2D logoTexture;
        private readonly Texture2D distortionTexture;
        private readonly HeatMaskDistortionInstance heatMaskDistortInstance;
        private readonly RenderTarget2D logoDistortionRenderTarget;
        private readonly Random random;
        private readonly HashSet<Wave> waveSet;

        private int nextWaveSpawnFrame;

        public StartLogo() : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.StartGame, tipsLayout: ControlTips.TipsLayout.StartGame)
        {
            this.random = new Random();
            this.waveSet = [];
            this.nextWaveSpawnFrame = 0;

            this.logoTexture = TextureBank.GetTexture(TextureBank.TextureName.GameLogo);
            this.distortionTexture = TextureBank.GetTexture(TextureBank.TextureName.LogoWaveDistortion);

            this.logoDistortionRenderTarget = new RenderTarget2D(SonOfRobinGame.GfxDev, this.logoTexture.Width, this.logoTexture.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);

            this.heatMaskDistortInstance = new HeatMaskDistortionInstance(baseTexture: this.logoTexture, distortTexture: this.logoDistortionRenderTarget);
            this.viewParams.Opacity = 0f;

            this.transManager.AddTransition(new Transition(transManager: this.transManager, outTrans: false, baseParamName: "Opacity", targetVal: 1f, duration: 90, startSwapParams: true));
        }

        public override void Remove()
        {
            base.Remove();

            this.logoDistortionRenderTarget.Dispose();
        }

        private static int LogoHeight { get { return (int)(SonOfRobinGame.ScreenHeight * 0.33f); } }

        public override void Update()
        {
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

            if (SonOfRobinGame.CurrentUpdate > this.nextWaveSpawnFrame && this.waveSet.Count < 2)
            {
                this.nextWaveSpawnFrame = SonOfRobinGame.CurrentUpdate + this.random.Next(60 * 3, 60 * 20);
                this.waveSet.Add(new Wave(random: this.random, logoTexture: this.logoTexture, waveTexture: this.distortionTexture));
            }

            var wavesToDisposeList = new List<Wave>();

            foreach (Wave wave in this.waveSet)
            {
                wave.Update();
                if (wave.Finished) wavesToDisposeList.Add(wave);
            }

            foreach (Wave wave in wavesToDisposeList)
            {
                this.waveSet.Remove(wave);
            }
        }

        public override void RenderToTarget()
        {
            SetRenderTarget(this.logoDistortionRenderTarget);

            SonOfRobinGame.SpriteBatch.Begin(blendState: BlendState.Additive);
            SonOfRobinGame.GfxDev.Clear(Color.Black);

            foreach (Wave wave in this.waveSet)
            {
                wave.Draw();
            }

            SonOfRobinGame.SpriteBatch.End();
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix, sortMode: SpriteSortMode.Immediate, blendState: BlendState.AlphaBlend);

            this.heatMaskDistortInstance.TurnOn(currentUpdate: SonOfRobinGame.CurrentUpdate, drawColor: Color.White);

            int logoHeight = LogoHeight;
            Rectangle imageRect = new(x: 0, y: (int)(logoHeight * 0.1f), width: SonOfRobinGame.ScreenWidth, height: logoHeight);
            Helpers.DrawTextureInsideRect(texture: this.logoTexture, rectangle: imageRect, color: Color.White * this.viewParams.drawOpacity);

            SonOfRobinGame.SpriteBatch.End();
        }

        public class Wave
        {
            private static readonly SpriteEffects[] spriteEffectsArray = [SpriteEffects.None, SpriteEffects.FlipHorizontally, SpriteEffects.FlipVertically];

            private readonly Texture2D waveTexture;
            private readonly Rectangle logoRect;
            private readonly Rectangle baseRect;
            private readonly float opacity;
            private readonly float frameDistance;
            private readonly SpriteEffects spriteEffects;
            private readonly Random random;

            private float posY;
            private bool directionForward;
            public bool Finished { get; private set; }

            public Wave(Random random, Texture2D logoTexture, Texture2D waveTexture)
            {
                this.random = random;

                this.opacity = (this.random.NextSingle() * 1.4f) + 0.5f;
                this.frameDistance = (this.random.NextSingle() * 3.5f) + 4f;
                this.spriteEffects = spriteEffectsArray[this.random.Next(spriteEffectsArray.Length)];

                this.waveTexture = waveTexture;
                this.logoRect = new Rectangle(x: 0, y: 0, width: logoTexture.Width, height: logoTexture.Height);
                float distortTextureScale = (float)this.logoRect.Width / (float)waveTexture.Width;
                this.baseRect = new(x: 0, y: 0, width: this.logoRect.Width, height: (int)(waveTexture.Height * distortTextureScale));

                this.posY = -this.baseRect.Height;
                this.directionForward = true;
                this.Finished = false;
            }

            public void Update()
            {
                if (this.Finished) return;

                this.posY += this.frameDistance * (this.directionForward ? 1f : -1f);

                if (this.directionForward && this.posY > this.baseRect.Height / 2) this.directionForward = false;
                if (!this.directionForward && this.posY + this.baseRect.Height <= 0) this.Finished = true;
            }

            public Rectangle WaveRect
            {
                get
                {
                    Rectangle rect = this.baseRect;
                    rect.Y = (int)this.posY;
                    return rect;
                }
            }

            public void Draw()
            {
                if (this.Finished) return;

                SonOfRobinGame.SpriteBatch.Draw(texture: this.waveTexture, sourceRectangle: this.waveTexture.Bounds, destinationRectangle: this.WaveRect, color: Color.White * this.opacity, rotation: 0f, origin: Vector2.Zero, effects: this.spriteEffects, layerDepth: 0f);
            }
        }
    }
}