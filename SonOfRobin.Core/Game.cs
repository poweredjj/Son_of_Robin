using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xamarin.Essentials;

namespace SonOfRobin
{
    public enum Platform { Desktop, Mobile }
    public enum OS { Windows, Linux, OSX, Android, iOS }

    public class SonOfRobinGame : Game
    {
        public static readonly float version = 9.2f;
        public static readonly DateTime lastChanged = new DateTime(2022, 08, 17);

        public static ContentManager content;

        public static Game game;
        public static Platform platform;
        public static OS os;
        public static bool fakeMobileMode = false;

        public static GraphicsDeviceManager graphics;
        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch spriteBatch;
        public static Effect effectColorize;
        public static Effect effectBorder;

        public static InfoWindow hintWindow;
        public static InfoWindow progressBar;
        public static ControlTips controlTips;
        public static TouchOverlay touchOverlay;

        public static SpriteFont fontPixelMix5;
        public static SpriteFont fontPressStart2P5;
        public static SpriteFont fontFreeSansBold12;
        public static SpriteFont fontFreeSansBold24;
        public static SpriteFont fontTommy20;
        public static SpriteFont fontTommy40;

        public static Texture2D whiteRectangle;
        public static Texture2D splashScreenTexture;
        public static List<RenderTarget2D> tempShadowMaskList;
        public static Texture2D lightSphere;
        public static Dictionary<string, Texture2D> textureByName = new Dictionary<string, Texture2D>();

        public static readonly SimpleFps fps = new SimpleFps();
        public static readonly Random random = new Random();

        public static int currentUpdate = 0;
        public static float lastUpdateDelay = 0;
        public static float lastDrawDelay = 0;

        public static string gameDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SonOfRobin_data");
        public static string worldTemplatesPath = Path.Combine(gameDataPath, "world_templates");
        public static string saveGamesPath = Path.Combine(gameDataPath, "savegames");
        public static string prefsPath = Path.Combine(gameDataPath, "preferences.bin");

        public static bool ThisIsWorkMachine { get { return gameDataPath.Contains("msmidowi"); } }

        public static bool quitGame = false;

        public static readonly int initialWindowWidth = ThisIsWorkMachine ? 700 : 1280;
        public static readonly int initialWindowHeight = ThisIsWorkMachine ? 250 : 720;
        public static int VirtualWidth { get { return Convert.ToInt32(graphics.PreferredBackBufferWidth / Preferences.GlobalScale); } }
        public static int VirtualHeight { get { return Convert.ToInt32(graphics.PreferredBackBufferHeight / Preferences.GlobalScale); } }

        public static PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes"); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID AND LINUX
        public static bool DesktopMemoryLow
        {
            get
            {
                if (platform == Platform.Desktop) return ramCounter.NextValue() < 800; // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID AND LINUX
                return false; // for compatibility with mobile
            }
        }
        public static bool LicenceValid { get { return DateTime.Now - lastChanged < TimeSpan.FromDays(30) || overrideLicence; } }
        public static bool overrideLicence = false;

        public static bool KeepScreenOn
        { set { if (platform == Platform.Mobile && !fakeMobileMode) DeviceDisplay.KeepScreenOn = value; } }
        public SonOfRobinGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            if (fakeMobileMode) platform = Platform.Mobile;

            base.Initialize();
            game = this;

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

            splashScreenTexture = content.Load<Texture2D>("gfx/loading_gfx");

            if (!Directory.Exists(gameDataPath)) Directory.CreateDirectory(gameDataPath);
            if (!Directory.Exists(worldTemplatesPath)) Directory.CreateDirectory(worldTemplatesPath);
            if (!Directory.Exists(saveGamesPath)) Directory.CreateDirectory(saveGamesPath);

            Preferences.Initialize(); // to set some default values
            Preferences.Load();

            if (Preferences.FullScreenMode)
            {
                graphics.IsFullScreen = true;
                IsMouseVisible = Preferences.MouseGesturesEmulateTouch;
            }
            else
            {
                graphics.IsFullScreen = false;
                IsMouseVisible = true;
            }

            graphicsDevice = GraphicsDevice;
            Preferences.CheckIfResolutionIsSupported();

            if (Preferences.FullScreenMode)
            {
                graphics.PreferredBackBufferWidth = Preferences.displayResX;
                graphics.PreferredBackBufferHeight = Preferences.displayResY;
            }
            else
            {
                graphics.PreferredBackBufferWidth = initialWindowWidth;
                graphics.PreferredBackBufferHeight = initialWindowHeight;
            }

            graphics.SynchronizeWithVerticalRetrace = Preferences.VSync;
            graphics.ApplyChanges();

            if (ThisIsWorkMachine) this.Window.Position = new Point(-10, 758); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
            this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            SoundEffect.DistanceScale = 1;

            KeepScreenOn = true;

            new InitialLoader();
        }

        public void OnResize(Object sender, EventArgs e)
        {
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width < 100 ? 100 : Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height < 100 ? 100 : Window.ClientBounds.Height;

            Scene.ResizeAllScenes();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            content = Content;

            fontPressStart2P5 = content.Load<SpriteFont>("fonts/PressStart2P"); // needed for InitialLoader
        }

        public static void LoadFonts()
        {
            fontPixelMix5 = content.Load<SpriteFont>("fonts/PixelMix");
            fontFreeSansBold12 = content.Load<SpriteFont>("fonts/FreeSansBold12");
            fontFreeSansBold24 = content.Load<SpriteFont>("fonts/FreeSansBold24");
            fontTommy20 = content.Load<SpriteFont>("fonts/Tommy20");
            fontTommy40 = content.Load<SpriteFont>("fonts/Tommy40");
        }
        protected override void Update(GameTime gameTime)
        {
            currentUpdate++;
            lastUpdateDelay = gameTime.ElapsedGameTime.Milliseconds;

            Scene.UpdateAllScenesInStack(gameTime: gameTime);

            base.Update(gameTime);
            fps.Update(gameTime);

            if (lastUpdateDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Update delay {lastUpdateDelay}ms.", color: Color.Orange);

            if (quitGame)
            {
                Preferences.Save();
                this.Exit();
                Environment.Exit(0);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            lastDrawDelay = gameTime.ElapsedGameTime.Milliseconds;

            SoundInstanceManager.CleanUpActiveInstances();
            Sound.UpdateAll();
            Scene.DrawAllScenesInStack();

            base.Draw(gameTime);

            fps.UpdateFpsCounter();
            if (lastDrawDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Draw delay {lastDrawDelay}ms.", color: Color.Orange);
        }

    }
}
