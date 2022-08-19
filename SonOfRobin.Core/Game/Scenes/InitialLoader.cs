using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class InitialLoader : Scene
    {
        public enum Step { Initial, LoadEffects, LoadFonts, DeleteObsoleteSaves, CreateControlTips, LoadSounds, LoadTextures, CreateAnims, LoadKeysGfx, CreateScenes, MakeItemsInfo, MakeCraftRecipes, SetControlTips, OpenMainMenu }

        private static readonly int allStepsCount = ((Step[])Enum.GetValues(typeof(Step))).Length;

        private readonly static Dictionary<Step, string> namesForSteps = new Dictionary<Step, string> {
            { Step.Initial, "starting" },
            { Step.LoadEffects, "loading effects" },
            { Step.LoadFonts, "loading fonts" },
            { Step.DeleteObsoleteSaves, "deleting obsolete saves" },
            { Step.CreateControlTips, "creating control tips" },
            { Step.LoadSounds, "loading sounds" },
            { Step.LoadTextures, "loading textures" },
            { Step.CreateAnims, "creating animations" },
            { Step.LoadKeysGfx, "loading keyboard textures" },
            { Step.CreateScenes, "creating helper scenes" },
            { Step.MakeItemsInfo, "creating items info" },
            { Step.MakeCraftRecipes, "preparing craft recipes" },
            { Step.SetControlTips, "setting control tips" },
            { Step.OpenMainMenu, "opening main menu" },
        };

        private Step currentStep;
        private readonly SpriteFont font;
        private readonly Texture2D splashScreenTexture;

        private string NextStepName
        { get { return (int)this.currentStep == allStepsCount ? "opening main menu" : namesForSteps[this.currentStep]; } }

        public InitialLoader() : base(inputType: InputTypes.None, priority: 1, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.currentStep = 0;
            this.font = SonOfRobinGame.fontPressStart2P5;
            this.splashScreenTexture = SonOfRobinGame.splashScreenTexture;
        }

        public override void Update(GameTime gameTime)
        {
            SonOfRobinGame.game.IsFixedTimeStep = false; // if turned on, some screen updates will be missing
            bool finish = false;

            switch (this.currentStep)
            {
                case Step.Initial:
                    break;

                case Step.LoadEffects:
                    SonOfRobinGame.effectColorize = SonOfRobinGame.content.Load<Effect>("effects/Colorize");
                    SonOfRobinGame.effectBorder = SonOfRobinGame.content.Load<Effect>("effects/Border");
                    break;

                case Step.LoadFonts:
                    SonOfRobinGame.LoadFonts();
                    break;

                case Step.DeleteObsoleteSaves:
                    SaveHeaderManager.DeleteObsoleteSaves();
                    break;

                case Step.CreateControlTips:
                    SonOfRobinGame.controlTips = new ControlTips();
                    break;

                case Step.LoadSounds:
                    SoundData.LoadAllSounds();
                    break;

                case Step.LoadTextures:
                    AnimData.LoadAllTextures();
                    break;

                case Step.CreateAnims:
                    AnimData.CreateAllAnims();
                    AnimFrame.DeleteUsedAtlases();
                    break;

                case Step.LoadKeysGfx:
                    KeyboardScheme.LoadAllKeys();
                    InputMapper.RebuildMappings();
                    break;

                case Step.CreateScenes:
                    SolidColor solidColor = new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
                    solidColor.MoveToBottom();
                    new MessageLog();
                    Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
                    SonOfRobinGame.hintWindow = new InfoWindow(bgColor: Color.RoyalBlue, bgOpacity: 0.85f);
                    SonOfRobinGame.progressBar = new InfoWindow(bgColor: Color.SeaGreen, bgOpacity: 0.85f);
                    break;

                case Step.MakeItemsInfo:
                    PieceInfo.CreateAllInfo();
                    break;

                case Step.MakeCraftRecipes:
                    Craft.PopulateAllCategories();
                    break;

                case Step.SetControlTips:
                    Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to load default control tips
                    break;

                case Step.OpenMainMenu:
                    if (Preferences.FrameSkip) SonOfRobinGame.game.IsFixedTimeStep = true;

                    if (SonOfRobinGame.LicenceValid)
                    {
                        if (Preferences.showDemoWorld) new World(seed: 777, width: 1500, height: 1000, resDivider: 2, playerFemale: false, demoMode: true, initialMaxAnimalsMultiplier: 100, addAgressiveAnimals: true);
                        MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
                    }
                    else
                    {
                        new TextWindow(text: "This version of 'Son of Robin' has expired.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: true, blockInputDuration: 60, closingTask: Scheduler.TaskName.OpenMainMenuIfSpecialKeysArePressed);
                    }
                    this.splashScreenTexture.Dispose();
                    break;

                default:
                    if ((int)this.currentStep < allStepsCount) throw new ArgumentException("Not all steps has been processed.");

                    finish = true;
                    break;
            }

            this.currentStep++;

            if (finish)
            {
                SonOfRobinGame.initialLoadingFinished = true;
                this.Remove();
                GC.Collect();
            }
        }

        public override void Draw()
        {
            SonOfRobinGame.graphicsDevice.Clear(Color.DarkBlue);

            Rectangle splashRect = new Rectangle(x: 0, y: -SonOfRobinGame.VirtualHeight / 8, width: SonOfRobinGame.VirtualWidth, height: SonOfRobinGame.VirtualHeight);
            splashRect.Inflate(-(int)(SonOfRobinGame.VirtualWidth * 0.42), -(int)(SonOfRobinGame.VirtualHeight * 0.42));

            Helpers.DrawTextureInsideRect(texture: this.splashScreenTexture, rectangle: splashRect, color: Color.White);

            string text = $"{this.NextStepName}...";
            Vector2 textSize = this.font.MeasureString(text);

            int textPosX = (int)((SonOfRobinGame.VirtualWidth / 2) - (textSize.X / 2));
            int textPosY = (int)(SonOfRobinGame.VirtualHeight * 0.75);

            SonOfRobinGame.spriteBatch.DrawString(this.font, text, position: new Vector2(textPosX, textPosY), color: Color.White, origin: Vector2.Zero, scale: 1, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            int progressBarFullLength = (int)(SonOfRobinGame.VirtualWidth * 0.8f);
            int progressBarCurrentLength = (int)(progressBarFullLength * ((float)this.currentStep / (float)allStepsCount));

            int barPosX = (SonOfRobinGame.VirtualWidth / 2) - (progressBarFullLength / 2);
            int barPosY = textPosY + (int)(textSize.Y * 1.5);

            Rectangle progressBarFullRect = new Rectangle(x: barPosX, y: barPosY, width: progressBarFullLength, height: (int)(textSize.Y * 3));
            Rectangle progressBarFilledRect = new Rectangle(x: barPosX, y: barPosY, width: progressBarCurrentLength, height: progressBarFullRect.Height);

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, progressBarFullRect, Color.White * 0.5f);
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, progressBarFilledRect, Color.White * 1f);
        }

    }
}
