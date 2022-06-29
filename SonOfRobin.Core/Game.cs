using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xamarin.Essentials;

namespace SonOfRobin
{
    public enum Platform { Desktop, Mobile }

    public class SonOfRobinGame : Game
    {
        public static readonly float version = 6.3f;
        public static readonly DateTime lastChanged = new DateTime(2022, 01, 04);

        public static Game game;
        public static Platform platform;
        public static bool fakeMobileMode = false;

        public static GraphicsDeviceManager graphics;
        public static GraphicsDevice graphicsDevice;
        public static SpriteBatch spriteBatch;

        public static SpriteFont fontSuperSmall;
        public static SpriteFont fontSmall;
        public static SpriteFont fontMedium;
        public static SpriteFont fontBig;
        public static SpriteFont fontHuge;

        public static Texture2D whiteRectangle;
        public static Dictionary<string, Texture2D> textureByName;

        public static readonly SimpleFps fps = new SimpleFps();
        public static readonly Random random = new Random();

        public static float lastUpdateDelay = 0;
        public static float lastDrawDelay = 0;
        public static int currentUpdate = 0;

        public static string gameDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SonOfRobin_data");
        public static string worldTemplatesPath = Path.Combine(gameDataPath, "world_templates");
        public static string saveGamesPath = Path.Combine(gameDataPath, "savegames");
        public static string prefsPath = Path.Combine(gameDataPath, "preferences.bin");

        public static bool quitGame = false;

        public static readonly int initialWindowWidth = 900; // 900
        public static readonly int initialWindowHeight = 250; // 250
        public static int VirtualWidth { get { return Convert.ToInt32(graphics.PreferredBackBufferWidth / Preferences.globalScale); } }
        public static int VirtualHeight { get { return Convert.ToInt32(graphics.PreferredBackBufferHeight / Preferences.globalScale); } }

        public static PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes"); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
        public static bool DesktopMemoryLow
        {
            get
            {
                if (platform == Platform.Desktop) return ramCounter.NextValue() < 800;// THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
                return false; // for compatibility with mobile
            }
        }

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

            if (!Directory.Exists(gameDataPath)) Directory.CreateDirectory(gameDataPath);
            if (!Directory.Exists(worldTemplatesPath)) Directory.CreateDirectory(worldTemplatesPath);
            if (!Directory.Exists(saveGamesPath)) Directory.CreateDirectory(saveGamesPath);

            Preferences.Initialize(); // to set some default values
            Preferences.Load();

            if (Preferences.FullScreenMode)
            {
                graphics.IsFullScreen = true;
                IsMouseVisible = false;
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

            graphics.ApplyChanges();

            this.Window.Position = new Point(-10, 758); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
            this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
            new MessageLog();
            Preferences.DebugMode = Preferences.DebugMode; // to create debugMode sce
                                                           // nes
            if (platform == Platform.Mobile) new TouchOverlay();
            if (platform == Platform.Desktop) new ControlTips();
            new ProgressBar();

            if (Preferences.showDemoWorld)
            {
                World demoWorld = new World(seed: 777, width: 1500, height: 1000, demoMode: true); ;
                new SolidColor(color: Color.White, viewOpacity: 0.4f, clearScreen: false);
            }

            MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
        }

        public void OnResize(Object sender, EventArgs e)
        {
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width < 100 ? 100 : Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height < 100 ? 100 : Window.ClientBounds.Height;

            var mapScenes = Scene.GetAllScenesOfType(typeof(Map));
            foreach (Scene scene in mapScenes)
            {
                Map mapScene = (Map)scene;
                mapScene.UpdateResolution();
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            fontSuperSmall = Content.Load<SpriteFont>("fonts/PixelMix");
            fontSmall = Content.Load<SpriteFont>("fonts/PressStart2P");
            fontMedium = Content.Load<SpriteFont>("fonts/FreeSansBold12");
            fontBig = Content.Load<SpriteFont>("fonts/Tommy20");
            fontHuge = Content.Load<SpriteFont>("fonts/Tommy40");

            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });

            textureByName = new Dictionary<string, Texture2D>();

            string[] assetNames = { "no_anim", "recolor_pt2", "demonmaid", "demonmaid2", "fox", "tile_custom01", "actor29rec4", "tileb", "tile_19ba32a6", "backlight_1", "backlight_2", "backlight_3", "backlight_4", "crabs_small", "crabs_big", "frogs_small", "frogs_big", "flowers", "8f296dbbaf43865bc29e99660fe7b5af_2x", "qYFvsmq", "NicePng_pine-tree-clipart-png_1446450", "palmtree_small", "tilees by guth_zpsfn3wpjdu_2x", "attack_effect_sprite_sheets", "miss", "zzz", "heart_16x16", "rabbits", "virtual_joypad_background", "virtual_joypad_stick", "virtual_button", "virtual_button_pressed", "cursor", "chests", "d9ffec650d3104f5c4564c9055787530", "sticks1", "sticks2", "axe_stone", "axe_wooden", "axe_iron", "axe_diamond", "hand", "tools_gravel", "stones", "fancy_food", "fancy_food2", "fancy_food3", "celianna_farmnature_crops_transparent", "weapons1", "steak_t-bone", "Cooked Meat", "big_icons_candacis", "Candacis_flames1", "gems__rpg_maker_mv__by_petschko-d9euoxr", "mv_blacksmith_by_schwarzenacht_dapf6ek", "bows", "arrow_wood", "arrow_iron", "crosshair", "sling", "greatsling", "stone_ammo", "craft_items", "tent_big", "tent_medium", "Xbox 360/360_A", "Xbox 360/360_B", "Xbox 360/360_Back", "Xbox 360/360_Back_Alt", "Xbox 360/360_Dpad", "Xbox 360/360_Dpad_Down", "Xbox 360/360_Dpad_Left", "Xbox 360/360_Dpad_Right", "Xbox 360/360_Dpad_Up", "Xbox 360/360_LB", "Xbox 360/360_Left_Stick", "Xbox 360/360_Left_Stick_Click", "Xbox 360/360_LT", "Xbox 360/360_RB", "Xbox 360/360_Right_Stick", "Xbox 360/360_Right_Stick_Click", "Xbox 360/360_RT", "Xbox 360/360_Start", "Xbox 360/360_Start_Alt", "Xbox 360/360_X", "Xbox 360/360_Y" };
            foreach (string assetName in assetNames)
            {
                Texture2D characterTexture = Content.Load<Texture2D>($"gfx/{assetName}");
                textureByName[assetName] = characterTexture;
            }

            AnimData.CreateAllAnims();
        }
        protected override void Update(GameTime gameTime)
        {
            currentUpdate++;
            lastUpdateDelay = gameTime.ElapsedGameTime.Milliseconds;

            Scene.UpdateAllScenesInStack(gameTime: gameTime);

            base.Update(gameTime);
            fps.Update(gameTime);

            if (lastUpdateDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Update delay {lastUpdateDelay}ms.", color: Color.Orange);

            if (quitGame)
            {
                Preferences.Save();
                this.Exit();
                System.Environment.Exit(0);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            lastDrawDelay = gameTime.ElapsedGameTime.Milliseconds;

            Scene.DrawAllScenesInStack();

            base.Draw(gameTime);

            fps.UpdateFpsCounter();
            if (lastDrawDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Draw delay {lastDrawDelay}ms.", color: Color.Orange);
        }

    }
}
