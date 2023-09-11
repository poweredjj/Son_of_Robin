using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
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
        public const float version = 0.504f;
        public static readonly DateTime lastChanged = new(2023, 09, 05);

        public static readonly int enteringIslandGlobalSteps = 4 + Grid.allStagesCount;
        public static ContentManager ContentMgr { get; private set; } // for things other than textures (for textures use TextureBank)
        public static Game Game { get; private set; }

        public static Platform platform;
        public static OS os;
        public static bool fakeMobileMode = false;
        public static GraphicsDeviceManager GfxDevMgr { get; private set; }
        public static GraphicsDevice GfxDev { get; private set; }
        public static RasterizerState RasterizeStateNoCulling { get; private set; }
        public static BasicEffect BasicEffect { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }
        public static Effect EffectColorize { get; private set; }
        public static Effect EffectBurn { get; private set; }
        public static Effect EffectBorder { get; private set; }
        public static Effect EffectSketch { get; private set; }
        public static InfoWindow HintWindow { get; private set; }
        public static InfoWindow SmallProgressBar { get; private set; }
        public static FullScreenProgressBar FullScreenProgressBar { get; private set; }
        public static ControlTips ControlTips { get; private set; }
        public static TouchOverlay touchOverlay;
        public static FpsCounter fpsCounter;
        public static BoardTextureProcessor BoardTextureProcessor { get; private set; }
        public static ErrorLog ErrorLog { get; private set; }
        public static SpriteFont FontPixelMix5 { get; private set; }
        public static SpriteFont FontPressStart2P5 { get; private set; }
        public static SpriteFont FontFreeSansBold10 { get; private set; }
        public static SpriteFont FontFreeSansBold12 { get; private set; }
        public static SpriteFont FontFreeSansBold24 { get; private set; }
        public static SpriteFont FontTommy20 { get; private set; }
        public static SpriteFont FontTommy40 { get; private set; }
        public static Texture2D WhiteRectangle { get; private set; }
        public static Texture2D GradientLeft { get; private set; }
        public static Texture2D GradientRight { get; private set; }
        public static Texture2D GradientTop { get; private set; }
        public static Texture2D GradientBottom { get; private set; }
        public static Texture2D SplashScreenTexture { get; private set; }

        public static List<RenderTarget2D> tempShadowMaskList;
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
        public static readonly string animCachePath = Path.Combine(gameDataPath, "graphics_cache");
        public static readonly string errorsPath = Path.Combine(gameDataPath, "errors");
        public static readonly string prefsPath = Path.Combine(gameDataPath, "preferences.json");
        public static string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        public static bool ThisIsWorkMachine
        { get { return gameDataPath.Contains("msmidowi"); } }

        public static bool ThisIsHomeMachine
        { get { return gameDataPath.Contains("Users\\Marcin"); } }

        public static bool quitGame = false;

        public static readonly int initialWindowWidth = ThisIsWorkMachine ? 700 : 1280;
        public static readonly int initialWindowHeight = ThisIsWorkMachine ? 250 : 720;

        public static int VirtualWidth
        { get { return Convert.ToInt32(GfxDevMgr.PreferredBackBufferWidth / Preferences.GlobalScale); } }

        public static int VirtualHeight
        { get { return Convert.ToInt32(GfxDevMgr.PreferredBackBufferHeight / Preferences.GlobalScale); } }

        private static void MoveWindowOnWorkMachine(Game game) // method used, to make the code to be commented closer
        {
            if (ThisIsWorkMachine) game.Window.Position = new Point(-7, 758); // COMMENT THIS LINE on ANDROID
        }

        public static bool LicenceValid
        { get { return DateTime.Now - lastChanged < TimeSpan.FromDays(90); } }

        public static bool KeepScreenOn
        { set { if (platform == Platform.Mobile && !fakeMobileMode) DeviceDisplay.KeepScreenOn = value; } }

        public SonOfRobinGame()
        {
            GfxDevMgr = new GraphicsDeviceManager(this);
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

            GradientLeft = TextureBank.GetTexturePersistent("gradient_left");
            GradientRight = TextureBank.GetTexturePersistent("gradient_right");
            GradientTop = TextureBank.GetTexturePersistent("gradient_top");
            GradientBottom = TextureBank.GetTexturePersistent("gradient_bottom");

            SplashScreenTexture = TextureBank.GetTexturePersistent("loading_gfx");
            BoardTextureProcessor = new BoardTextureProcessor();
            ErrorLog = new ErrorLog();

            if (!Directory.Exists(gameDataPath)) Directory.CreateDirectory(gameDataPath);
            if (!Directory.Exists(worldTemplatesPath)) Directory.CreateDirectory(worldTemplatesPath);
            if (!Directory.Exists(saveGamesPath)) Directory.CreateDirectory(saveGamesPath);
            if (!Directory.Exists(animCachePath)) Directory.CreateDirectory(animCachePath);
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
            RasterizeStateNoCulling = new RasterizerState
            {
                CullMode = CullMode.None
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

            FontPressStart2P5 = ContentMgr.Load<SpriteFont>("fonts/PressStart2P"); // needed for InitialLoader
        }

        public static void LoadFonts()
        {
            FontPixelMix5 = ContentMgr.Load<SpriteFont>("fonts/PixelMix");
            FontFreeSansBold10 = ContentMgr.Load<SpriteFont>("fonts/FreeSansBold10");
            FontFreeSansBold12 = ContentMgr.Load<SpriteFont>("fonts/FreeSansBold12");
            FontFreeSansBold24 = ContentMgr.Load<SpriteFont>("fonts/FreeSansBold24");
            FontTommy20 = ContentMgr.Load<SpriteFont>("fonts/Tommy20");
            FontTommy40 = ContentMgr.Load<SpriteFont>("fonts/Tommy40");
        }

        public static void LoadEffects()
        {
            EffectColorize = ContentMgr.Load<Effect>("effects/Colorize");
            EffectBurn = ContentMgr.Load<Effect>("effects/Burn");
            EffectBorder = ContentMgr.Load<Effect>("effects/Border");
            EffectSketch = ContentMgr.Load<Effect>("effects/Sketch");
        }

        public static void LoadInitialTextures()
        {
            lightSphere = TextureBank.GetTexture(TextureBank.TextureName.LightSphereWhite);
            tempShadowMaskList = new List<RenderTarget2D> { };
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

            if (LastUpdateDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(debugMessage: true, message: $"Update delay {LastUpdateDelay}ms.", color: Color.Orange);

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
            if (LastDrawDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(debugMessage: true, message: $"Draw delay {LastDrawDelay}ms.", color: Color.Orange);
        }
    }
}