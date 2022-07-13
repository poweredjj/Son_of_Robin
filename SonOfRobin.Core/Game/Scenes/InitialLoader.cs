using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace SonOfRobin
{
    public class InitialLoader : Scene
    {
        public enum Step { Initial, LoadEffects, LoadFonts, MakeDirs, DeleteObsoleteSaves, CreateControlTips, LoadPrefs, LoadSounds, LoadTextures, CreateAnims, LoadKeysGfx, CreateScenes, MakeItemsInfo, MakeCraftRecipes, SetControlTips }
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
                    this.nextStepName = "loading effects";
                    break;

                case Step.LoadEffects:
                    SonOfRobinGame.effectColorize = SonOfRobinGame.content.Load<Effect>("effects/Colorize");
                    SonOfRobinGame.effectBorder = SonOfRobinGame.content.Load<Effect>("effects/Border");

                    this.nextStepName = "loading fonts";
                    break;

                case Step.LoadFonts:
                    SonOfRobinGame.LoadFonts();

                    this.nextStepName = "deleting obsolete saves";
                    break;

                case Step.MakeDirs:
                    if (!Directory.Exists(SonOfRobinGame.gameDataPath)) Directory.CreateDirectory(SonOfRobinGame.gameDataPath);
                    if (!Directory.Exists(SonOfRobinGame.worldTemplatesPath)) Directory.CreateDirectory(SonOfRobinGame.worldTemplatesPath);
                    if (!Directory.Exists(SonOfRobinGame.saveGamesPath)) Directory.CreateDirectory(SonOfRobinGame.saveGamesPath);

                    this.nextStepName = "deleting obsolete saves";
                    break;

                case Step.DeleteObsoleteSaves:
                    SaveHeaderManager.DeleteObsoleteSaves();

                    this.nextStepName = "creating control tips";
                    break;

                case Step.CreateControlTips:
                    SonOfRobinGame.controlTips = new ControlTips();

                    this.nextStepName = "loading preferences";
                    break;

                case Step.LoadPrefs:
                    Preferences.Initialize(); // to set some default values
                    Preferences.Load();

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

                    this.nextStepName = "loading keyboard textures";
                    break;

                case Step.LoadKeysGfx:
                    KeyboardScheme.LoadAllKeys();
                    InputMapper.RebuildMappings();

                    this.nextStepName = "creating helper scenes";
                    break;

                case Step.CreateScenes:
                    SolidColor solidColor = new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
                    solidColor.MoveToBottom();
                    new MessageLog();
                    Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
                    SonOfRobinGame.hintWindow = new InfoWindow(bgColor: Color.RoyalBlue, bgOpacity: 0.85f);
                    SonOfRobinGame.progressBar = new InfoWindow(bgColor: Color.SeaGreen, bgOpacity: 0.85f);

                    this.nextStepName = "creating items info";
                    break;

                case Step.MakeItemsInfo:
                    PieceInfo.CreateAllInfo();

                    this.nextStepName = "preparing craft recipes";
                    break;

                case Step.MakeCraftRecipes:
                    Craft.PopulateAllCategories();

                    this.nextStepName = "setting control tips";
                    break;

                case Step.SetControlTips:
                    Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to load default control tips

                    this.nextStepName = "";
                    break;

                default:
                    if ((int)this.currentStep < allStepsCount) throw new ArgumentException("Not all steps has been processed.");

                    finish = true;
                    break;
            }

            this.currentStep++;

            if (finish)
            {
                if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;

                SonOfRobinGame.KeepScreenOn = true;

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
                GC.Collect();
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

            int progressBarFullLength = (int)(SonOfRobinGame.VirtualWidth * 0.8f);
            int progressBarCurrentLength = (int)(progressBarFullLength * ((float)this.currentStep / (float)allStepsCount));

            Rectangle progressBarFullRect = new Rectangle(x: (SonOfRobinGame.VirtualWidth / 2) - (progressBarFullLength / 2), y: (int)(SonOfRobinGame.VirtualHeight * 0.82f), width: progressBarFullLength, height: (int)(SonOfRobinGame.VirtualHeight * 0.07f));

            Rectangle progressBarFilledRect = new Rectangle(x: progressBarFullRect.X, y: progressBarFullRect.Y, width: progressBarCurrentLength, height: progressBarFullRect.Height);

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, progressBarFullRect, Color.White * 0.5f);
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, progressBarFilledRect, Color.White * 1f);
        }

    }
}
