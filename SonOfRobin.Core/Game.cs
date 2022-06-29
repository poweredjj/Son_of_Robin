using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

// Version 4.7
// Last changed: 2021.12.01

namespace SonOfRobin
{
    public enum Platform { Desktop, Mobile }

    public class SonOfRobinGame : Game
    {
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

        public static readonly int initialWindowWidth = 600; // 900
        public static readonly int initialWindowHeight = 250; // 250
        public static int VirtualWidth { get { return Convert.ToInt32(graphics.PreferredBackBufferWidth / Preferences.globalScale); } }
        public static int VirtualHeight { get { return Convert.ToInt32(graphics.PreferredBackBufferHeight / Preferences.globalScale); } }

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

            if (Preferences.FullScreenMode)
            {
                graphics.PreferredBackBufferWidth = graphicsDevice.Adapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = graphicsDevice.Adapter.CurrentDisplayMode.Height;
            }
            else
            {
                graphics.PreferredBackBufferWidth = initialWindowWidth;
                graphics.PreferredBackBufferHeight = initialWindowHeight;
            }

            graphics.ApplyChanges();

            this.Window.Position = new Point(-10, 885); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
            //this.Window.Position = new Point(-908, 1317); // THIS LINE MUST BE COMMENTED OUT WHEN COMPILING FOR ANDROID
            this.Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            new SolidColor(color: Color.RoyalBlue, viewOpacity: 1f, clearScreen: true);
            new MessageLog();
            Preferences.DebugMode = Preferences.DebugMode; // to create debugMode scenes
            if (platform == Platform.Mobile) new TouchOverlay();
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

            string[] assetNames = { "no_anim", "recolor_pt2", "demonmaid", "demonmaid2", "fox", "tile_custom01", "actor29rec4", "tileb", "tile_19ba32a6", "backlight_1", "backlight_2", "backlight_3", "backlight_4", "crabs_small", "crabs_big", "frogs_small", "frogs_big", "flowers", "8f296dbbaf43865bc29e99660fe7b5af", "qYFvsmq", "NicePng_pine-tree-clipart-png_1446450", "palmtree_small", "tilees by guth_zpsfn3wpjdu", "attack_effect_sprite_sheets", "miss", "zzz", "heart_16x16", "rabbits", "virtual_joypad_background", "virtual_joypad_stick", "virtual_button", "virtual_button_pressed", "cursor", "chests", "d9ffec650d3104f5c4564c9055787530", "sticks1", "sticks2", "axe_stone", "axe_wooden", "axe_iron", "axe_diamond", "hand", "tools_gravel", "stones", "fancy_food", "fancy_food2", "fancy_food3" };
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
