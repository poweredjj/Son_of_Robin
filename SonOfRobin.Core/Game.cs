using Microsoft.Xna.Framework;
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

    public class SonOfRobinGame : Game
    {
        public static readonly float version = 8.5f;
        public static readonly DateTime lastChanged = new DateTime(2022, 05, 11);

        public static ContentManager content;

        public static Game game;
        public static Platform platform;
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
        public static List<RenderTarget2D> tempShadowMaskList;
        public static Texture2D lightSphere;
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

            if (!Directory.Exists(gameDataPath)) Directory.CreateDirectory(gameDataPath);
            if (!Directory.Exists(worldTemplatesPath)) Directory.CreateDirectory(worldTemplatesPath);
            if (!Directory.Exists(saveGamesPath)) Directory.CreateDirectory(saveGamesPath);
            new Scheduler.Task(taskName: Scheduler.TaskName.DeleteObsoleteSaves, executeHelper: null, delay: 0);

            controlTips = new ControlTips();
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

            graphics.ApplyChanges();

            if (ThisIsWorkMachine) this.Window.Position = new Point(-10, 758); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
            this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            AnimData.CreateAllAnims();
            AnimFrame.DeleteUsedAtlases();

            KeyboardScheme.LoadAllKeys();
            InputMapper.RebuildMappings();

            Preferences.ControlTipsScheme = Preferences.ControlTipsScheme; // to load default control tips

            new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
            new MessageLog();
            Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
            hintWindow = new InfoWindow(bgColor: Color.RoyalBlue, bgOpacity: 0.85f);
            progressBar = new InfoWindow(bgColor: Color.SeaGreen, bgOpacity: 0.85f);

            KeepScreenOn = true;

            if (LicenceValid)
            {
                if (Preferences.showDemoWorld) new World(seed: 777, width: 1500, height: 1000, resDivider: 2, demoMode: true);
                MenuTemplate.CreateMenuFromTemplate(templateName: MenuTemplate.Name.Main);
            }
            else
            {
                var textWindow = new TextWindow(text: "This version of 'Son of Robin' has expired.", textColor: Color.White, bgColor: Color.DarkBlue, useTransition: false, animate: true, blockInputDuration: 60, closingTask: Scheduler.TaskName.OpenMainMenuIfSpecialKeysArePressed);
            }
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

            effectColorize = Content.Load<Effect>("effects/Colorize");
            effectBorder = Content.Load<Effect>("effects/Border");

            fontPixelMix5 = Content.Load<SpriteFont>("fonts/PixelMix");
            fontPressStart2P5 = Content.Load<SpriteFont>("fonts/PressStart2P");
            fontFreeSansBold12 = Content.Load<SpriteFont>("fonts/FreeSansBold12");
            fontFreeSansBold24 = Content.Load<SpriteFont>("fonts/FreeSansBold24");
            fontTommy20 = Content.Load<SpriteFont>("fonts/Tommy20");
            fontTommy40 = Content.Load<SpriteFont>("fonts/Tommy40");

            textureByName = new Dictionary<string, Texture2D>();

            string[] assetNames = { "no_anim", "fox", "tile_custom01", "actor29rec4", "tileb", "tile_19ba32a6", "backlight_1", "backlight_2", "backlight_3", "backlight_4", "crabs_small", "crabs_big", "frogs_small", "frogs_big", "flowers", "8f296dbbaf43865bc29e99660fe7b5af_2x", "qYFvsmq", "NicePng_pine-tree-clipart-png_1446450", "palmtree_small", "tilees by guth_zpsfn3wpjdu_2x", "attack", "miss", "zzz", "heart_16x16", "rabbits", "virtual_joypad_background", "virtual_joypad_stick", "virtual_button", "virtual_button_pressed", "cursor", "chests", "d9ffec650d3104f5c4564c9055787530", "sticks1", "sticks2", "axe_wooden", "hand", "tools_gravel", "stones", "fancy_food", "fancy_food2", "celianna_farmnature_crops_transparent", "steak_t-bone", "Cooked Meat", "big_icons_candacis", "Candacis_flames1", "gems__rpg_maker_mv__by_petschko-d9euoxr", "mv_blacksmith_by_schwarzenacht_dapf6ek", "bows", "arrow_wood", "arrow_iron", "crosshair", "stone_small", "craft_items", "tent_big", "tent_medium", "flames", "bag", "bag_outline", "backpack", "belt", "parchment", "exclamation", "scythe_stone", "scythe_iron", "grass_blade", "tiger", "plus", "acorn", "light_white", "small_torch_on", "small_torch_off", "big_torch_on", "big_torch_off", "water_drop", "tile_rtp-addons", "bottle_empty", "herbs_black", "herbs_blue", "herbs_cyan", "herbs_green", "herbs_red", "herbs_violet", "herbs_yellow", "rpg_maker_vx_ace_tilesets_1_by_hishimy_d8e7pjd", "Mouse/Mouse_Left_Key_Light", "Mouse/Mouse_Middle_Key_Light", "Mouse/Mouse_Right_Key_Light", "Mouse/Mouse_Scroll_Up_Light", "Mouse/Mouse_Scroll_Down_Light", "potion_black", "arrow_poison", "spear_wood", "spear_stone", "spear_iron", "spear_poison", "alchemy_lab", "workshop_basic", "workshop_advanced", "workshop_essential", "piece_of_fat", "bottle_oil" };

            foreach (string assetName in assetNames)
            {
                textureByName[assetName] = Content.Load<Texture2D>($"gfx/{assetName}");
            }

            lightSphere = textureByName["light_white"];
            tempShadowMaskList = new List<RenderTarget2D> { };
            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });
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

            Scene.DrawAllScenesInStack();

            base.Draw(gameTime);

            fps.UpdateFpsCounter();
            if (lastDrawDelay >= 20 && IsFixedTimeStep) MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Draw delay {lastDrawDelay}ms.", color: Color.Orange);
        }

    }
}
