using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
            CreateMeshDefinitions,
            LoadAnimsJson,
            LoadAnimsPlants,
            LoadAnimsChars,
            LoadAnimsMisc1,
            LoadAnimsMisc2,
            SaveAnimsJson,
            BuildMappings,
            MakeItemsInfo,
            RemoveUnneededData,
            MakeCraftRecipes,
            CreateScenes,
            WaitForBackgroundTasksToFinish,
            MakeDemoWorld,
            SetControlTips,
            OpenMainMenu,
        }

        private static readonly List<string> firstWordList = new List<string>
        {
            "counting", "placing", "brushing", "sewing", "fixing", "swabbing", "dodging", "taming", "battling", "training", "catching", "solving", "removing", "inserting", "mourning", "burying", "tickling", "scanning", "chasing", "rebuilding", "digitizing", "enumerating", "downloading", "transmitting", "placing", "reading", "indexing", "herding", "checking", "migrating", "resolving", "binding", "initializing", "creating", "making", "preparing", "opening", "closing", "conquering", "moving", "inviting", "importing", "serializing", "polishing", "enumerating", "decompressing", "expanding", "deconstructing", "building", "naming"
        };

        private static readonly List<string> secondWordList = new List<string>
        {
            "foxes", "holes", "pirate ships", "goat horns", "tigers", "rabbits", "puzzles", "pebbles", "healthy meals", "drops of water", "grass blades", "skeletons", "crabs", "seagulls", "mermaids", "goats", "lambs", "treasure chests", "bananas", "apples", "parrots", "seashells", "polygons", "pixels", "waves", "monkeys", "apes", "treasures", "coconuts", "barrels of rum"
        };

        private static readonly int allStepsCount = ((Step[])Enum.GetValues(typeof(Step))).Length;

        private static readonly Dictionary<Step, string> namesForSteps = new Dictionary<Step, string> {
            { Step.Initial, "starting" },
            { Step.MobileWait, "adding mobile waiting time" },
            { Step.StartBgTasks, "starting background tasks" },
            { Step.CreateMeshDefinitions, "creating mesh definitions" },
            { Step.LoadAnimsJson, "loading animations (json)" },
            { Step.LoadAnimsPlants, "loading animations (plants)" },
            { Step.LoadAnimsChars, "loading animations (characters)" },
            { Step.LoadAnimsMisc1, "loading animations (others 1)" },
            { Step.LoadAnimsMisc2, "loading animations (others 2)" },
            { Step.SaveAnimsJson, "saving animations (json)" },
            { Step.BuildMappings, "building input mappings" },
            { Step.WaitForBackgroundTasksToFinish, "waiting for background tasks to finish" },
            { Step.CreateScenes, "creating helper scenes" },
            { Step.MakeItemsInfo, "creating items info" },
            { Step.RemoveUnneededData, "removing unneeded data" },
            { Step.MakeCraftRecipes, "preparing craft recipes" },
            { Step.MakeDemoWorld, "making demo world" },
            { Step.SetControlTips, "setting control tips" },
            { Step.OpenMainMenu, "opening main menu" },
        };

        private readonly DateTime startTime;
        private Task backgroundTask;
        private Step currentStep;
        private readonly SpriteFontBase font;
        private readonly Texture2D splashScreenTexture;
        private int mobileWaitingTimes;

        private bool TimeoutReached
        { get { return DateTime.Now - this.startTime > TimeSpan.FromSeconds(15); } }

        private DateTime lastFunnyActionNameCreated;
        private string lastFunnyActionName;
        private List<string> usedFunnyWordsList;

        private string FunnyActionName
        {
            get
            {
                if (DateTime.Now - this.lastFunnyActionNameCreated < TimeSpan.FromSeconds(0.8f)) return this.lastFunnyActionName;

                this.lastFunnyActionNameCreated = DateTime.Now;

                var tempFirstWordList = firstWordList.Where(word => !usedFunnyWordsList.Contains(word)).ToList();
                var tempSecondWordList = secondWordList.Where(word => !usedFunnyWordsList.Contains(word)).ToList();

                if (tempFirstWordList.Count == 0) tempFirstWordList = firstWordList;
                if (tempSecondWordList.Count == 0) tempSecondWordList = secondWordList;

                string firstWord = tempFirstWordList[SonOfRobinGame.random.Next(tempFirstWordList.Count)];
                string secondWord = tempSecondWordList[SonOfRobinGame.random.Next(tempSecondWordList.Count)];

                this.usedFunnyWordsList.Add(firstWord);
                this.usedFunnyWordsList.Add(secondWord);

                this.lastFunnyActionName = $"{firstWord} {secondWord}";
                return this.lastFunnyActionName;
            }
        }

        private string NextStepName
        { get { return (int)this.currentStep == allStepsCount ? "opening main menu" : namesForSteps[this.currentStep]; } }

        public InitialLoader() : base(inputType: InputTypes.None, priority: 0, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty, alwaysUpdates: true)
        {
            this.startTime = DateTime.Now;
            this.lastFunnyActionNameCreated = DateTime.MinValue;
            this.lastFunnyActionName = "";
            this.usedFunnyWordsList = new List<string>();
            this.currentStep = 0;
            this.font = SonOfRobinGame.FontPressStart2P.GetFont(SonOfRobinGame.VirtualWidth > 1000 ? 16 : 8);
            this.splashScreenTexture = SonOfRobinGame.SplashScreenTexture;
            this.mobileWaitingTimes = SonOfRobinGame.platform == Platform.Mobile ? 30 : 0;

            SonOfRobinGame.Game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
        }

        private void ProcessBackgroundTasks()
        {
            SonOfRobinGame.LoadFonts();
            SonOfRobinGame.LoadEffects();
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
                        this.ProcessBackgroundTasks();
                    }
                    else
                    {
                        this.backgroundTask = Task.Run(() => this.ProcessBackgroundTasks());
                    }
                    break;

                case Step.CreateMeshDefinitions:
                    MeshDefinition.CreateMeshDefinitions();
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
                    break;

                case Step.BuildMappings:
                    InputMapper.RebuildMappings();
                    break;

                case Step.MakeItemsInfo:
                    PieceInfo.CreateAllInfo();
                    break;

                case Step.RemoveUnneededData:
                    AnimData.DeleteUsedAtlases();
                    AnimData.textureDict.Clear(); // not needed afterwards
                    AnimData.jsonDict.Clear(); // not needed afterwards
                    break;

                case Step.MakeCraftRecipes:
                    Craft.PopulateAllCategories();
                    PieceInfo.Info.GetYieldForAntiCraft(); // need to be invoked after populating craft recipes
                    break;

                case Step.CreateScenes:
                    Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
                    SonOfRobinGame.CreateHintAndProgressWindows();
                    break;

                case Step.WaitForBackgroundTasksToFinish:
                    {
                        bool tasksCompleted = false;

                        if (this.backgroundTask == null || (this.backgroundTask != null && this.backgroundTask.IsCompleted)) tasksCompleted = true;
                        else if (this.backgroundTask.IsFaulted || this.TimeoutReached)
                        {
                            SonOfRobinGame.ErrorLog.AddEntry(obj: this, exception: this.backgroundTask.Exception, showTextWindow: false);

                            this.ProcessBackgroundTasks();
                            tasksCompleted = true;
                        }

                        if (!tasksCompleted) currentStep--;

                        break;
                    }

                case Step.MakeDemoWorld:
                    if (Preferences.showDemoWorld && SonOfRobinGame.LicenceValid)
                    {
                        if (World.GetTopWorld() == null) new World(seed: GridTemplate.demoWorldSeed, width: 10000, height: 10000, resDivider: 20, demoMode: true, playerName: PieceTemplate.Name.PlayerBoy); // playerName is not used in demoWorld

                        World demoWorld = World.GetTopWorld();

                        while (true)
                        {
                            demoWorld.Update();

                            if (!demoWorld.ActiveLevel.creationInProgress && !demoWorld.PopulatingInProgress) break;
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
                MessageLog.Add(debugMessage: true, text: $"Initial loading time: {loadingDuration:hh\\:mm\\:ss\\.fff}.", textColor: Color.GreenYellow);

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

            string text = SonOfRobinGame.ThisIsWorkMachine || SonOfRobinGame.ThisIsHomeMachine ? $"{this.NextStepName}..." : $"{this.FunnyActionName}...";
            // text = $"{this.FunnyActionName}..."; // for testing

            Vector2 textSize = Helpers.MeasureStringCorrectly(font: this.font, stringToMeasure: text);

            int textPosX = (int)((SonOfRobinGame.VirtualWidth / 2) - (textSize.X / 2));
            int textPosY = (int)(SonOfRobinGame.VirtualHeight * 0.75);

            this.font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: text, position: new Vector2(textPosX, textPosY), color: Color.White);

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