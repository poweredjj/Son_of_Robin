using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class InitialLoader : Scene
    {
        public enum Step : byte
        {
            Initial,
            MobileWait,
            StartBgTasks,
            CreateSeamless,
            LoadAnimsJson,
            LoadAnimsPlants,
            LoadAnimsChars,
            LoadAnimsMisc1,
            LoadAnimsMisc2,
            SaveAnimsJson,
            LoadKeysGfx,
            MakeItemsInfo,
            MakeCraftRecipes,
            CreateScenes,
            WaitForBackgroundTasksToFinish1,
            WaitForBackgroundTasksToFinish2,
            MakeDemoWorld,
            SetControlTips,
            OpenMainMenu,
        }

        private static readonly int allStepsCount = ((Step[])Enum.GetValues(typeof(Step))).Length;

        private static readonly Dictionary<Step, string> namesForSteps = new Dictionary<Step, string> {
            { Step.Initial, "starting" },
            { Step.StartBgTasks, "starting background tasks" },
            { Step.MobileWait, "adding mobile waiting time" },
            { Step.CreateSeamless, "creating seamless textures" },
            { Step.LoadAnimsJson, "loading animations (json)" },
            { Step.LoadAnimsPlants, "loading animations (plants)" },
            { Step.LoadAnimsChars, "loading animations (characters)" },
            { Step.LoadAnimsMisc1, "loading animations (others 1)" },
            { Step.LoadAnimsMisc2, "loading animations (others 2)" },
            { Step.SaveAnimsJson, "saving animations (json)" },
            { Step.LoadKeysGfx, "loading keyboard textures" },
            { Step.WaitForBackgroundTasksToFinish1, "waiting for background tasks to finish (first batch)" },
            { Step.WaitForBackgroundTasksToFinish2, "waiting for background tasks to finish (second batch)" },
            { Step.CreateScenes, "creating helper scenes" },
            { Step.MakeItemsInfo, "creating items info" },
            { Step.MakeCraftRecipes, "preparing craft recipes" },
            { Step.MakeDemoWorld, "making demo world" },
            { Step.SetControlTips, "setting control tips" },
            { Step.OpenMainMenu, "opening main menu" },
        };

        private DateTime startTime;
        private Task backgroundTask1;
        private Task backgroundTask2;
        private Step currentStep;
        private readonly SpriteFont font;
        private readonly Texture2D splashScreenTexture;
        private int mobileWaitingTimes;
        private bool TimeoutReached
        { get { return DateTime.Now - this.startTime > TimeSpan.FromSeconds(15); } }

        private string NextStepName
        { get { return (int)this.currentStep == allStepsCount ? "opening main menu" : namesForSteps[this.currentStep]; } }

        public InitialLoader() : base(inputType: InputTypes.None, priority: 0, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty, alwaysUpdates: true)
        {
            this.startTime = DateTime.Now;
            this.currentStep = 0;
            this.font = SonOfRobinGame.FontPressStart2P5;
            this.splashScreenTexture = SonOfRobinGame.SplashScreenTexture;
            this.mobileWaitingTimes = SonOfRobinGame.platform == Platform.Mobile ? 30 : 0;

            SonOfRobinGame.Game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
        }

        private void ProcessBackgroundTasks1()
        {
            SoundData.LoadAllSounds();
            SonOfRobinGame.LoadEffects();
        }

        private void ProcessBackgroundTasks2()
        {
            SonOfRobinGame.LoadFonts();
            GridTemplate.DeleteObsolete();
        }

        public override void Update()
        {
            bool finish = false;

            switch (this.currentStep)
            {
                case Step.Initial:
                    SonOfRobinGame.LoadInitialTextures();
                    SonOfRobinGame.CreateControlTips();
                    break;

                case Step.MobileWait:
                    this.mobileWaitingTimes--;
                    if (this.mobileWaitingTimes > 0) currentStep--;
                    break;

                case Step.StartBgTasks:
                    if (SonOfRobinGame.platform == Platform.Mobile) // background tasks are not processed correctly on mobile
                    {
                        this.ProcessBackgroundTasks1();
                        this.ProcessBackgroundTasks2();
                    }
                    else
                    {
                        this.backgroundTask1 = Task.Run(() => this.ProcessBackgroundTasks1());
                        this.backgroundTask2 = Task.Run(() => this.ProcessBackgroundTasks2());
                    }
                    break;

                case Step.CreateSeamless:
                    RepeatingPattern.ConvertAllTexturesToPatterns();
                    break;

                case Step.LoadAnimsJson:
                    if (!AnimData.LoadJsonDict()) AnimData.PurgeDiskCache();
                    break;

                case Step.LoadAnimsPlants:
                    AnimData.CreateAnimsPlants();
                    break;

                case Step.LoadAnimsChars:
                    AnimData.CreateAnimsCharacters();
                    break;

                case Step.LoadAnimsMisc1:
                    AnimData.CreateAnimsMisc1();
                    break;

                case Step.LoadAnimsMisc2:
                    AnimData.CreateAnimsMisc2();
                    break;

                case Step.SaveAnimsJson:
                    AnimData.SaveJsonDict();
                    AnimData.DeleteUsedAtlases();
                    AnimData.textureDict.Clear(); // not needed afterwards
                    AnimData.jsonDict.Clear(); // not needed afterwards
                    break;

                case Step.LoadKeysGfx:
                    KeyboardScheme.LoadAllKeys();
                    InputMapper.RebuildMappings();
                    break;

                case Step.MakeItemsInfo:
                    PieceInfo.CreateAllInfo();
                    break;

                case Step.MakeCraftRecipes:
                    Craft.PopulateAllCategories();
                    PieceInfo.Info.GetYieldForAntiCraft(); // need to be invoked after populating craft recipes
                    break;

                case Step.CreateScenes:
                    SolidColor solidColor = new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
                    solidColor.MoveToBottom();
                    new MessageLog();
                    Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
                    SonOfRobinGame.CreateHintAndProgressWindows();
                    break;

                case Step.WaitForBackgroundTasksToFinish1:
                    {
                        bool tasksCompleted = false;

                        if (this.backgroundTask1 == null || (this.backgroundTask1 != null && this.backgroundTask1.IsCompleted)) tasksCompleted = true;
                        else if (this.backgroundTask1.IsFaulted || this.TimeoutReached)
                        {
                            SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask1.Exception, showTextWindow: false);

                            this.ProcessBackgroundTasks1();
                            tasksCompleted = true;
                        }

                        if (!tasksCompleted) currentStep--;

                        break;
                    }

                case Step.WaitForBackgroundTasksToFinish2:
                    {
                        bool tasksCompleted = false;

                        if (this.backgroundTask2 == null || (this.backgroundTask2 != null && this.backgroundTask2.IsCompleted)) tasksCompleted = true;
                        else if (this.backgroundTask2.IsFaulted || this.TimeoutReached)
                        {
                            SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask2.Exception, showTextWindow: false);

                            this.ProcessBackgroundTasks2();
                            tasksCompleted = true;
                        }

                        if (!tasksCompleted) currentStep--;

                        break;
                    }

                case Step.MakeDemoWorld:
                    if (Preferences.showDemoWorld && SonOfRobinGame.LicenceValid)
                    {
                        if (World.GetTopWorld() == null) new World(seed: GridTemplate.demoWorldSeed, width: 4000, height: 4000, resDivider: 5, demoMode: true, playerName: PieceTemplate.Name.PlayerBoy); // playerName is not used in demoWorld

                        World demoWorld = World.GetTopWorld();

                        while (true)
                        {
                            demoWorld.Update();

                            if (!demoWorld.WorldCreationInProgress && !demoWorld.PopulatingInProgress) break;
                            else SonOfRobinGame.CurrentUpdateAdvance(); // manually changing the counter, to avoid softlock
                        }
                    }

                    break;

                case Step.SetControlTips:
                    Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to load default control tips
                    break;

                case Step.OpenMainMenu:
                    Preferences.FrameSkip = Preferences.FrameSkip; // to apply valid FrameSkip value

                    if (SonOfRobinGame.LicenceValid)
                    {
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                    }
                    else
                    {
                        new TextWindow(text: "This version of 'Son of Robin' has expired.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: true, blockInputDuration: 60, closingTask: Scheduler.TaskName.OpenMainMenuIfSpecialKeysArePressed);
                    }
                    Preferences.ShowFpsCounter = Preferences.ShowFpsCounter; // to display fps counter (if set)
                    break;

                default:
                    if ((int)this.currentStep < allStepsCount) throw new ArgumentException("Not all steps has been processed.");

                    finish = true;
                    break;
            }

            this.currentStep++;

            if (finish)
            {
                TimeSpan loadingDuration = DateTime.Now - this.startTime;
                MessageLog.AddMessage(debugMessage: true, message: $"Initial loading time: {loadingDuration:hh\\:mm\\:ss\\.fff}.", color: Color.GreenYellow);

                this.Remove();
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            SonOfRobinGame.GfxDev.Clear(Color.DarkBlue);

            Rectangle splashRect = new Rectangle(x: 0, y: -SonOfRobinGame.VirtualHeight / 8, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);
            splashRect.Inflate(-(int)(SonOfRobinGame.VirtualWidth * 0.42), -(int)(SonOfRobinGame.VirtualHeight * 0.42));

            Helpers.DrawTextureInsideRect(texture: this.splashScreenTexture, rectangle: splashRect, color: Color.White);

            string text = $"{this.NextStepName}...";
            Vector2 textSize = this.font.MeasureString(text);

            int textPosX = (int)((SonOfRobinGame.VirtualWidth / 2) - (textSize.X / 2));
            int textPosY = (int)(SonOfRobinGame.VirtualHeight * 0.75);

            SonOfRobinGame.SpriteBatch.DrawString(this.font, text, position: new Vector2(textPosX, textPosY), color: Color.White, origin: Vector2.Zero, scale: 1, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            int progressBarFullLength = (int)(SonOfRobinGame.VirtualWidth * 0.8f);
            int progressBarCurrentLength = (int)(progressBarFullLength * ((float)this.currentStep / (float)allStepsCount));

            int barPosX = (SonOfRobinGame.VirtualWidth / 2) - (progressBarFullLength / 2);
            int barPosY = textPosY + (int)(textSize.Y * 1.5);

            Rectangle progressBarFullRect = new Rectangle(x: barPosX, y: barPosY, width: progressBarFullLength, height: (int)(textSize.Y * 3));
            Rectangle progressBarFilledRect = new Rectangle(x: barPosX, y: barPosY, width: progressBarCurrentLength, height: progressBarFullRect.Height);

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, progressBarFullRect, Color.White * 0.5f);
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, progressBarFilledRect, Color.White * 1f);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}