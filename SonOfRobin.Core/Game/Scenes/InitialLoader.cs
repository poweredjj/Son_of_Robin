using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class InitialLoader : Scene
    {
        public enum Step : byte
        {
            Initial,
            MobileWait,
            LoadFonts,
            LoadEffects,
            DeleteObsoleteTemplates,
            CreateMeshDefinitions,
            LoadAnimsJson,
            ProcessAnims,
            CheckCellSize,
            PopulatePieceHints,
            BuildMappings,
            MakeItemsInfo,
            MakeCraftRecipes,
            CreateScenes,
            MakeDemoWorld,
            SetControlTips,
            OpenMainMenu,
        }

        private static readonly string[] firstWordArray = new string[]
        {
            "counting", "placing", "brushing", "sewing", "fixing", "swabbing", "dodging", "taming", "battling", "training", "catching", "solving", "removing", "inserting", "mourning", "burying", "tickling", "scanning", "chasing", "rebuilding", "digitizing", "enumerating", "downloading", "transmitting", "placing", "reading", "indexing", "herding", "checking", "migrating", "resolving", "binding", "initializing", "creating", "making", "preparing", "opening", "closing", "conquering", "moving", "inviting", "importing", "serializing", "polishing", "enumerating", "decompressing", "expanding", "deconstructing", "building", "naming"
        };

        private static readonly string[] secondWordArray = new string[]
        {
            "foxes", "holes", "pirate ships", "goat horns", "tigers", "rabbits", "puzzles", "pebbles", "healthy meals", "drops of water", "grass blades", "skeletons", "crabs", "seagulls", "mermaids", "goats", "lambs", "treasure chests", "bananas", "apples", "parrots", "seashells", "polygons", "pixels", "waves", "monkeys", "apes", "treasures", "coconuts", "barrels of rum"
        };

        private static readonly int allStepsCount = ((Step[])Enum.GetValues(typeof(Step))).Length;

        private static readonly Dictionary<Step, string> namesForSteps = new()
        {
            { Step.Initial, "starting" },
            { Step.MobileWait, "adding mobile waiting time" },
            { Step.LoadFonts, "loading fonts" },
            { Step.LoadEffects, "loading effects" },
            { Step.DeleteObsoleteTemplates, "deleting obsolete templates" },
            { Step.CreateMeshDefinitions, "creating mesh definitions" },
            { Step.LoadAnimsJson, "loading animation data" },
            { Step.ProcessAnims, "processing animation data" },
            { Step.CheckCellSize, "checking cell size" },
            { Step.PopulatePieceHints, "populating hints data" },
            { Step.BuildMappings, "building input mappings" },
            { Step.CreateScenes, "creating helper scenes" },
            { Step.MakeItemsInfo, "creating items info" },
            { Step.MakeCraftRecipes, "preparing craft recipes" },
            { Step.MakeDemoWorld, "making demo world" },
            { Step.SetControlTips, "setting control tips" },
            { Step.OpenMainMenu, "opening main menu" },
        };

        private readonly DateTime startTime;
        private Step currentStep;
        private readonly SpriteFontBase font;
        private readonly Texture2D splashScreenTexture;
        private int mobileWaitingTimes;
        private readonly Queue<AnimData.PkgName> animPackagesToLoadQueue;

        private DateTime lastFunnyActionNameCreated;
        private string lastFunnyActionName;
        private readonly List<string> usedFunnyWordsList;

        private string FunnyActionName
        {
            get
            {
                if (DateTime.Now - this.lastFunnyActionNameCreated < TimeSpan.FromSeconds(1.3f)) return this.lastFunnyActionName;

                this.lastFunnyActionNameCreated = DateTime.Now;

                var tempFirstWordList = firstWordArray.Where(word => !usedFunnyWordsList.Contains(word)).ToArray();
                var tempSecondWordList = secondWordArray.Where(word => !usedFunnyWordsList.Contains(word)).ToArray();

                if (tempFirstWordList.Length == 0) tempFirstWordList = firstWordArray;
                if (tempSecondWordList.Length == 0) tempSecondWordList = secondWordArray;

                string firstWord = tempFirstWordList[SonOfRobinGame.random.Next(tempFirstWordList.Length)];
                string secondWord = tempSecondWordList[SonOfRobinGame.random.Next(tempSecondWordList.Length)];

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
            this.animPackagesToLoadQueue = new Queue<AnimData.PkgName>(AnimData.allPkgNames);

            SonOfRobinGame.Game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
        }

        public override void Update()
        {
            bool finish = false;

            DateTime stageStartTime = DateTime.Now;

            switch (this.currentStep)
            {
                case Step.Initial:
                    SonOfRobinGame.LoadInitialTextures();
                    SonOfRobinGame.CreateControlTips();
                    break;

                case Step.MobileWait:
                    this.mobileWaitingTimes--;
                    if (this.mobileWaitingTimes > 0) this.currentStep--;
                    break;

                case Step.LoadFonts:
                    SonOfRobinGame.LoadFonts();
                    break;

                case Step.LoadEffects:
                    SonOfRobinGame.LoadEffects();
                    break;

                case Step.DeleteObsoleteTemplates:
                    GridTemplate.DeleteObsolete();
                    break;

                case Step.CreateMeshDefinitions:
                    MeshDefinition.CreateMeshDefinitions();
                    break;

                case Step.LoadAnimsJson:
                    if (!AnimData.LoadJsonDict()) AnimData.PurgeDiskCache();
                    break;

                case Step.ProcessAnims:
                    DateTime loadingStartTime = DateTime.Now;
                    TimeSpan maxLoadingDuration = TimeSpan.FromSeconds(0.25f);

                    while (true)
                    {
                        if (this.animPackagesToLoadQueue.Count > 0) AnimData.LoadPackage(this.animPackagesToLoadQueue.Dequeue());

                        TimeSpan loadingDuration = DateTime.Now - loadingStartTime;
                        if (loadingDuration > maxLoadingDuration || this.animPackagesToLoadQueue.Count == 0) break;
                    }

                    if (this.animPackagesToLoadQueue.Count > 0) this.currentStep--;
                    else
                    {
                        AnimData.SaveJsonDict();
                        AnimData.DisposeUsedAtlases();
                    }

                    break;

                case Step.CheckCellSize:
                    if (SonOfRobinGame.ThisIsWorkMachine || SonOfRobinGame.ThisIsHomeMachine)
                    {
                        Point cellSize = GridTemplate.CalculateCellSize();
                        bool cellSizeCorrect = cellSize == GridTemplate.ProperCellSize;

                        if (!cellSizeCorrect)
                        {
                            new TextWindow(text: $"Proper cell size --> {cellSize.X}x{cellSize.Y} <--\nSaved cell size: {GridTemplate.ProperCellSize.X}x{GridTemplate.ProperCellSize.Y}\nUPDATE NEEDED", animate: false, useTransition: false, bgColor: Color.DarkRed, textColor: Color.White);
                        }
                    }

                    break;

                case Step.PopulatePieceHints:
                    PieceHint.PopulateData();
                    break;

                case Step.BuildMappings:
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
                    Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
                    SonOfRobinGame.CreateHintAndProgressWindows();
                    break;

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

                    if (SonOfRobinGame.LicenceValid) MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
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

            if (!finish)
            {
                TimeSpan stageDuration = DateTime.Now - stageStartTime;
                MessageLog.Add(debugMessage: true, text: $"Initial loader, stage '{this.currentStep}' - time: {stageDuration:hh\\:mm\\:ss\\.fff}");
            }

            this.currentStep++;

            if (finish)
            {
                TimeSpan loadingDuration = DateTime.Now - this.startTime;
                MessageLog.Add(debugMessage: true, text: $"Initial loader, all stages - time: {loadingDuration:hh\\:mm\\:ss\\.fff}.", textColor: Color.GreenYellow);

                this.Remove();
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            SonOfRobinGame.GfxDev.Clear(Color.DarkBlue);

            Rectangle splashRect = new(x: 0, y: -SonOfRobinGame.VirtualHeight / 8, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);
            splashRect.Inflate(-(int)(SonOfRobinGame.VirtualWidth * 0.42), -(int)(SonOfRobinGame.VirtualHeight * 0.42));

            Helpers.DrawTextureInsideRect(texture: this.splashScreenTexture, rectangle: splashRect, color: Color.White);

            string text = SonOfRobinGame.ThisIsWorkMachine || SonOfRobinGame.ThisIsHomeMachine ? $"{this.NextStepName}..." : $"{this.FunnyActionName}...";
            // text = $"{this.FunnyActionName}..."; // for testing

            Vector2 textSize = Helpers.MeasureStringCorrectly(font: this.font, stringToMeasure: text);

            int textPosX = (int)((SonOfRobinGame.VirtualWidth / 2) - (textSize.X / 2));
            int textPosY = (int)(SonOfRobinGame.VirtualHeight * 0.75);

            this.font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: text, position: new Vector2(textPosX, textPosY), color: Color.White);

            float currentStepNo = (int)this.currentStep + (AnimData.LoadedPkgs.Count / 10);
            float allSteps = allStepsCount + (AnimData.allPkgNames.Length / 10);

            int progressBarFullLength = (int)(SonOfRobinGame.VirtualWidth * 0.8f);
            int progressBarCurrentLength = (int)(progressBarFullLength * (currentStepNo / allSteps));

            int barPosX = (SonOfRobinGame.VirtualWidth / 2) - (progressBarFullLength / 2);
            int barPosY = textPosY + (int)(textSize.Y * 1.5);

            Rectangle progressBarFullRect = new(x: barPosX, y: barPosY, width: progressBarFullLength, height: (int)(textSize.Y * 3));
            Rectangle progressBarFilledRect = new(x: barPosX, y: barPosY, width: progressBarCurrentLength, height: progressBarFullRect.Height);

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, progressBarFullRect, Color.White * 0.5f);
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, progressBarFilledRect, Color.White * 1f);

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}