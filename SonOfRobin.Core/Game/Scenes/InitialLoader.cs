using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace SonOfRobin
{
    public class InitialLoader : Scene
    {
        public enum Step { Initial, MakeDirs, LoadSounds, LoadTextures, CreateAnims, MakingItemsInfo, MakingCraftRecipes, SettingControlTips }
        public static readonly int allStepsCount = ((Step[])Enum.GetValues(typeof(Step))).Length;

        private Step currentStep;
        private string nextStepName;

        public InitialLoader() : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.currentStep = Step.Initial;
            this.nextStepName = "";

            SonOfRobinGame.game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
        }

        public override void Update(GameTime gameTime)
        {
            bool finish = false;


            switch (this.currentStep)
            {
                case Step.Initial:
                    this.nextStepName = "making directories";
                    break;

                case Step.MakeDirs:
                    if (!Directory.Exists(SonOfRobinGame.gameDataPath)) Directory.CreateDirectory(SonOfRobinGame.gameDataPath);
                    if (!Directory.Exists(SonOfRobinGame.worldTemplatesPath)) Directory.CreateDirectory(SonOfRobinGame.worldTemplatesPath);
                    if (!Directory.Exists(SonOfRobinGame.saveGamesPath)) Directory.CreateDirectory(SonOfRobinGame.saveGamesPath);

                    this.nextStepName = "loading sounds";
                    break;

                case Step.LoadSounds:
                    SoundData.LoadAllSounds();

                    this.nextStepName = "loading textures";
                    break;

                case Step.LoadTextures:
                    AnimData.LoadAllTextures();

                    this.nextStepName = "creating animations";
                    break;

                case Step.CreateAnims:
                    AnimData.CreateAllAnims();
                    AnimFrame.DeleteUsedAtlases();

                    this.nextStepName = "creating items info";
                    break;

                case Step.MakingItemsInfo:
                    PieceInfo.CreateAllInfo();

                    this.nextStepName = "preparing craft recipes";
                    break;

                case Step.MakingCraftRecipes:
                    Craft.PopulateAllCategories();

                    this.nextStepName = "setting control tips";
                    break;

                case Step.SettingControlTips:
                    Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to load default control tips

                    this.nextStepName = "";
                    break;

                default:
                    finish = true;
                    break;
            }

            this.currentStep++;

            if (finish)
            {
                if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;

                GC.Collect();

                SonOfRobinGame.KeepScreenOn = true;

                new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);

                if (SonOfRobinGame.LicenceValid)
                {
                    if (Preferences.showDemoWorld) new World(seed: 777, width: 1500, height: 1000, resDivider: 2, playerFemale: false, demoMode: true, initialMaxAnimalsMultiplier: 100, addAgressiveAnimals: true);
                    MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                }
                else
                {
                    new TextWindow(text: "This version of 'Son of Robin' has expired.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: true, blockInputDuration: 60, closingTask: Scheduler.TaskName.OpenMainMenuIfSpecialKeysArePressed);
                }

                this.Remove();
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.graphicsDevice.Clear(Color.DarkBlue);

            SpriteFont font = SonOfRobinGame.fontPressStart2P5;
            if (font == null) return;

            string text = $"{this.nextStepName} {(int)this.currentStep}/{allStepsCount}";

            Vector2 textSize = font.MeasureString(text);

            int posX = (int)((SonOfRobinGame.VirtualWidth / 2) - (textSize.X / 2));

            SonOfRobinGame.spriteBatch.DrawString(font, text, position: new Vector2(posX, (int)(SonOfRobinGame.VirtualHeight * 0.75)), color: Color.White, origin: Vector2.Zero, scale: 1, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

        }

    }
}
