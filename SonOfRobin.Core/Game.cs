using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SonOfRobin
{
    public enum Platform : byte
    {
        Desktop,
        Mobile,
    }

    public enum OS : byte
    {
        Windows,
        DesktopGL,
        Android,
        iOS,
    }

    public class SonOfRobinGame : Game
    {
        public const float version = 0.6042f;
        public static readonly DateTime lastChanged = new(2024, 01, 19);

        public static readonly int enteringIslandGlobalSteps = 4 + Grid.allStagesCount;
        public static ContentManager ContentMgr { get; private set; } // for things other than textures (for textures use TextureBank)
        public static Game Game { get; private set; }

        public static bool fakeMobileMode = false;
        public static Platform platform;
        public static OS os;

        private static void MoveWindowOnWorkMachine(Game game)
        {
            if (ThisIsWorkMachine) game.Window.Position = new Point(-7, 758); // COMMENT THIS LINE on ANDROID
        }

        public static GraphicsDeviceManager GfxDevMgr { get; private set; }
        public static GraphicsDevice GfxDev { get; private set; }
        public static RasterizerState RasterizeStateNoCulling { get; private set; }
        public static RasterizerState RasterizeStateNoCullingWireframe { get; private set; }
        public static BasicEffect BasicEffect { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static Effect EffectColorize { get; private set; }
        public static Effect EffectBurn { get; private set; }
        public static Effect EffectBorder { get; private set; }
        public static Effect EffectSketch { get; private set; }
        public static Effect EffectBlur { get; private set; }
        public static Effect EffectMosaic { get; private set; }
        public static Effect EffectPixelate { get; private set; }
        public static Effect EffectDistort { get; private set; }
        public static Effect EffectRain { get; private set; }
        public static Effect EffectHeatMaskDistortion { get; private set; }
        public static Effect EffectShadowMerge { get; private set; }
        public static InfoWindow HintWindow { get; private set; }
        public static InfoWindow SmallProgressBar { get; private set; }
        public static FullScreenProgressBar FullScreenProgressBar { get; private set; }
        public static ControlTips ControlTips { get; private set; }
        public static TouchOverlay touchOverlay;
        public static FpsCounter fpsCounter;
        public static MessageLog MessageLog { get; private set; }
        public static ErrorLog ErrorLog { get; private set; }
        public static FontSystemSettings PixelatedFontSettings { get; private set; } // needed for "pixelated" fonts to be sharp
        public static FontSystem FontFreeSansBold { get; private set; }
        public static FontSystem FontPressStart2P { get; private set; }
        public static FontSystem FontVCROSD { get; private set; }
        public static FontSystem FontPixelMix { get; private set; }
        public static FontSystem FontTommy { get; private set; }
        public static Texture2D WhiteRectangle { get; private set; }
        public static Texture2D GradientLeft { get; private set; }
        public static Texture2D GradientRight { get; private set; }
        public static Texture2D GradientTop { get; private set; }
        public static Texture2D GradientBottom { get; private set; }
        public static Texture2D SplashScreenTexture { get; private set; }

        public static readonly ParallelOptions defaultParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        public static RenderTarget2D tempShadowMask;
        public static Texture2D lightSphere;
        public static readonly SimpleFps fps = new();
        public static readonly Random random = new();
        public static int CurrentUpdate { get; private set; }
        public static int CurrentDraw { get; private set; }
        public static float LastUpdateDelay { get; private set; }
        public static float LastDrawDelay { get; private set; }
        public static GameTime CurrentGameTime { get; private set; }

        public static readonly string gameDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SonOfRobin_data");
        public static readonly string worldTemplatesPath = Path.Combine(gameDataPath, "world_templates");
        public static readonly string saveGamesPath = Path.Combine(gameDataPath, "savegames");
        public static readonly string errorsPath = Path.Combine(gameDataPath, "errors");
        public static readonly string prefsPath = Path.Combine(gameDataPath, "preferences.json");
        public static string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        public static bool IgnoreThisDraw { get { return Preferences.halfFramerate && CurrentDraw % 2 != 0; } }

        public static bool ThisIsWorkMachine
        { get { return gameDataPath.Contains("msmidowi"); } }

        public static bool ThisIsHomeMachine
        { get { return gameDataPath.Contains("Users\\Marcin"); } }

        public static bool quitGame = false;

        public static readonly int initialWindowWidth = ThisIsWorkMachine ? 700 : 1280;
        public static readonly int initialWindowHeight = ThisIsWorkMachine ? 250 : 720;

        public static int ScreenWidth { get { return GfxDevMgr.PreferredBackBufferWidth; } }
        public static int ScreenHeight { get { return GfxDevMgr.PreferredBackBufferHeight; } }

        public static bool LicenceValid
        { get { return DateTime.Now - lastChanged < TimeSpan.FromDays(90); } }

        public static bool KeepScreenOn
        { set { if (platform == Platform.Mobile && !fakeMobileMode) DeviceDisplay.KeepScreenOn = value; } }

        public SonOfRobinGame()
        {
            GfxDevMgr = new GraphicsDeviceManager(this) { GraphicsProfile = GraphicsProfile.HiDef };
        }

        protected override void Initialize()
        {
            if (fakeMobileMode) platform = Platform.Mobile;

            CurrentUpdate = 0;
            CurrentDraw = 0;

            base.Initialize();
            Game = this;

            WhiteRectangle = new Texture2D(base.GraphicsDevice, 1, 1);
            WhiteRectangle.SetData(new[] { Color.White });

            GradientLeft = TextureBank.GetTexture("gradient_left");
            GradientRight = TextureBank.GetTexture("gradient_right");
            GradientTop = TextureBank.GetTexture("gradient_top");
            GradientBottom = TextureBank.GetTexture("gradient_bottom");

            SplashScreenTexture = TextureBank.GetTexture(TextureBank.TextureName.LoadingGfx);
            ErrorLog = new ErrorLog();
            SolidColor solidColor = new(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
            solidColor.MoveToBottom();
            MessageLog = new MessageLog();

            if (!Directory.Exists(gameDataPath)) Directory.CreateDirectory(gameDataPath);
            if (!Directory.Exists(worldTemplatesPath)) Directory.CreateDirectory(worldTemplatesPath);
            if (!Directory.Exists(saveGamesPath)) Directory.CreateDirectory(saveGamesPath);
            if (!Directory.Exists(errorsPath)) Directory.CreateDirectory(errorsPath);

            Preferences.Initialize(); // to set some default values
            Preferences.Load();

            if (Preferences.FullScreenMode)
            {
                GfxDevMgr.IsFullScreen = true;
                IsMouseVisible = Preferences.MouseGesturesEmulateTouch;
            }
            else
            {
                GfxDevMgr.IsFullScreen = false;
                IsMouseVisible = true;
            }

            GfxDev = base.GraphicsDevice;
            BasicEffect = new(GfxDev);
            RasterizeStateNoCulling = new RasterizerState { CullMode = CullMode.None };
            RasterizeStateNoCullingWireframe = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame
            };

            Preferences.CheckIfResolutionIsSupported();

            if (Preferences.FullScreenMode)
            {
                GfxDevMgr.PreferredBackBufferWidth = Preferences.displayResX;
                GfxDevMgr.PreferredBackBufferHeight = Preferences.displayResY;
            }
            else
            {
                GfxDevMgr.PreferredBackBufferWidth = initialWindowWidth;
                GfxDevMgr.PreferredBackBufferHeight = initialWindowHeight;
            }

            GfxDevMgr.SynchronizeWithVerticalRetrace = Preferences.VSync;
            GfxDevMgr.ApplyChanges();

            MoveWindowOnWorkMachine(this);
            this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            SoundEffect.DistanceScale = 125;

            KeepScreenOn = true;

            new InitialLoader();
            // new AnimViewer(); // for testing
        }

        public void OnResize(Object sender, EventArgs e)
        {
            GfxDevMgr.PreferredBackBufferWidth = Window.ClientBounds.Width < 100 ? 100 : Window.ClientBounds.Width;
            GfxDevMgr.PreferredBackBufferHeight = Window.ClientBounds.Height < 100 ? 100 : Window.ClientBounds.Height;

            Scene.ScheduleAllScenesResize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(base.GraphicsDevice);
            ContentMgr = new ContentManager(Services, "Content");
            TextureBank.AssignContentManagers(persistentManager: new ContentManager(Services, "Content"), temporaryManager: new ContentManager(Services, "Content"));

            FontSystemDefaults.FontResolutionFactor = 2.0f;
            FontSystemDefaults.KernelWidth = 2;
            FontSystemDefaults.KernelHeight = 2;

            PixelatedFontSettings = new FontSystemSettings { FontResolutionFactor = 1.0f, KernelWidth = 1, KernelHeight = 1 };

            FontPressStart2P = new FontSystem(PixelatedFontSettings); // needed for InitialLoader
            FontPressStart2P.AddFont(TitleContainer.OpenStream("Content/fonts/PressStart2P.ttf"));

            FontVCROSD = new FontSystem(PixelatedFontSettings); // needed for MessageLog
            FontVCROSD.AddFont(TitleContainer.OpenStream("Content/fonts/VCR_OSD_MONO_1.001.ttf"));
        }

        public static void LoadFonts()
        {
            FontFreeSansBold = new FontSystem();
            FontFreeSansBold.AddFont(TitleContainer.OpenStream("Content/fonts/FreeSansBold.ttf"));

            FontPixelMix = new FontSystem(PixelatedFontSettings);
            FontPixelMix.AddFont(TitleContainer.OpenStream("Content/fonts/pixelmix.ttf"));

            FontTommy = new FontSystem();
            FontTommy.AddFont(TitleContainer.OpenStream("Content/fonts/MADE_TOMMY_Medium_PERSONAL_USE.otf"));
        }

        public static void LoadEffects()
        {
            EffectColorize = ContentMgr.Load<Effect>("effects/Colorize");
            EffectBurn = ContentMgr.Load<Effect>("effects/Burn");
            EffectBorder = ContentMgr.Load<Effect>("effects/Border");
            EffectSketch = ContentMgr.Load<Effect>("effects/Sketch");
            EffectBlur = ContentMgr.Load<Effect>("effects/Blur");
            EffectMosaic = ContentMgr.Load<Effect>("effects/Mosaic");
            EffectPixelate = ContentMgr.Load<Effect>("effects/Pixelate");
            EffectDistort = ContentMgr.Load<Effect>("effects/Distort");
            EffectRain = ContentMgr.Load<Effect>("effects/Rain");
            EffectHeatMaskDistortion = ContentMgr.Load<Effect>("effects/HeatMaskDistortion");
            EffectShadowMerge = ContentMgr.Load<Effect>("effects/ShadowMerge");
        }

        public static void LoadInitialTextures()
        {
            lightSphere = TextureBank.GetTexture(TextureBank.TextureName.LightSphereWhite);
        }

        public static void CreateHintAndProgressWindows()
        {
            HintWindow = new InfoWindow(bgColor: Color.RoyalBlue, bgOpacity: 0.85f);
            SmallProgressBar = new InfoWindow(bgColor: Color.SeaGreen, bgOpacity: 0.85f);
            FullScreenProgressBar = new FullScreenProgressBar();
        }

        public static void CreateControlTips()
        {
            ControlTips = new ControlTips();
        }

        public static void CurrentUpdateAdvance()
        {
            // using outside Update() loop can potentially lead to glitches
            CurrentUpdate++;
        }

        protected override void Update(GameTime gameTime)
        {
            CurrentGameTime = gameTime;

            CurrentUpdateAdvance();
            LastUpdateDelay = gameTime.ElapsedGameTime.Milliseconds;

            Scene.AllScenesInStackUpdate();

            base.Update(gameTime);
            fps.Update(gameTime);

            if (LastUpdateDelay >= 20 && IsFixedTimeStep) MessageLog.Add(debugMessage: true, text: $"Update delay {LastUpdateDelay}ms.", textColor: Color.Orange);

            if (quitGame)
            {
                Preferences.Save();
                this.Exit();
                Environment.Exit(0);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            CurrentGameTime = gameTime;

            CurrentDraw++;

            LastDrawDelay = gameTime.ElapsedGameTime.Milliseconds;

            ManagedSoundInstance.Update();
            Sound.UpdateAll();
            Scene.AllScenesInStackDraw();

            base.Draw(gameTime);

            fps.UpdateFpsCounter();
            if (LastDrawDelay >= 20 && IsFixedTimeStep) MessageLog.Add(debugMessage: true, text: $"Draw delay {LastDrawDelay}ms.", textColor: Color.Orange);
        }
    }
}