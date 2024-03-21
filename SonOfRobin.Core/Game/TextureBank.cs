using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class TextureBank
    {
        public enum TextureName : byte
        {
            // predefined names for easier texture loading

            Empty = 0,

            LoadingGfx = 1,
            LoadingWheel = 2,
            LightSphereWhite = 3,
            Cursor = 4,
            Bed = 5,
            Animal = 6,
            New = 7,
            Map = 8,
            MapEdges = 9,
            WhiteCircleSmall = 10,
            Biceps = 11,
            WhiteHorizontalLine = 12,
            MonoGame = 13,
            BackpackMediumOutline = 151,
            Star = 152,
            FogCloud = 153,
            GfxCorrupted = 155,
            GameLogo = 158,
            LogoWaveDistortion = 159,

            SimpleArrowUp = 14,
            SimpleArrowRight = 15,
            SimpleHeart = 16,
            SimpleInfinity = 17,
            SimpleArea = 18,
            SimpleBurger = 19,
            SimpleSapling = 20,
            SimpleSleep = 21,
            SimpleHourglass = 22,
            SimpleMoon = 23,
            SimpleSpeed = 24,
            SimpleStack = 25,

            BuffPoison = 26,
            BuffRegen = 27,
            BuffHeat = 28,
            BuffWet = 29,
            BuffHPPlus = 30,
            BuffHPMinus = 31,
            BuffMaxHPPlus = 32,
            BuffMaxHPMinus = 33,
            BuffStrPlus = 34,
            BuffStrMinus = 35,
            BuffLowHP = 36,
            BuffSpeed = 37,
            BuffSprint = 38,
            BuffCannotSprint = 39,
            BuffHaste = 40,
            BuffMountainWalking = 41,
            BuffWidth = 42,
            BuffHeight = 43,
            BuffPoisonRemove = 44,
            BuffHungry = 157,

            RepeatingOceanFloor = 45,
            RepeatingWaterCaustics = 46,
            RepeatingFog = 47,
            RepeatingStars = 160,
            RepeatingCloudsWhite = 161,
            RepeatingCloudsDark = 162,
            RepeatingCloudsShadows = 163,
            RepeatingLavaDistortion = 164,

            VirtualJoypadBackground = 48,
            VirtualJoypadStick = 49,

            InputMouseButtonLeft = 50,
            InputMouseButtonMiddle = 51,
            InputMouseButtonRight = 52,
            InputMouseScrollUp = 53,
            InputMouseScrollDown = 54,

            VirtButtonBGReleased = 55,
            VirtButtonBGPressed = 56,

            VirtButtonEat = 57,
            VirtButtonDrink = 58,
            VirtButtonBow = 59,
            VirtButtonLighter = 60,
            VirtButtonPlant = 61,
            VirtButtonDropFruit = 62,
            VirtButtonOpenContainer = 63,
            VirtButtonGoToSleep = 64,
            VirtButtonUseCampfire = 65,
            VirtButtonCook = 66,
            VirtButtonCraft = 67,
            VirtButtonBrew = 68,
            VirtButtonSmelt = 154,
            VirtButtonHarvestMeat = 69,
            VirtButtonUseAnvil = 70,
            VirtButtonJump = 71,
            VirtButtonOffer = 72,
            VirtButtonEnterExit = 73,

            ParticleCircleSharp = 74,
            ParticleCircleSoft = 75,
            ParticleWeatherRain = 76,
            ParticleBubble = 77,
            ParticleSmokePuff = 78,
            ParticleDustPuff = 79,
            ParticleWaterEdgeDistortion = 156,
            ParticleDebrisWood = 80,
            ParticleDebrisLeaf = 81,
            ParticleDebrisPetal = 82,
            ParticleDebrisGrass = 83,
            ParticleDebrisStone = 84,
            ParticleDebrisCrystal = 85,
            ParticleDebrisCeramic = 86,
            ParticleDebrisBlood = 87,
            ParticleDebrisStar = 88,
            ParticleDebrisHeart = 89,
            ParticleDebrisSoot = 90,

            RepeatingPerlinNoiseColor = 91,
            RepeatingWaterDrops = 92,
            RepeatingGrassGood = 93,
            RepeatingGrassGoodNormal = 165,
            RepeatingMapGrassGood = 94,
            RepeatingGrassBad = 95,
            RepeatingMapGrassBad = 96,
            RepeatingWaterSuperShallow = 97,
            RepeatingMapWaterSuperShallow = 98,
            RepeatingWaterMedium = 99,
            RepeatingMapWaterMedium = 100,
            RepeatingWaterDeep = 101,
            RepeatingMapWaterDeep = 102,
            RepeatingMountainLow = 103,
            RepeatingMapMountainLow = 104,
            RepeatingMountainMedium = 105,
            RepeatingMapMountainMedium = 106,
            RepeatingMountainHigh = 107,
            RepeatingMapMountainHigh = 108,
            RepeatingGroundBase = 109,
            RepeatingMapGroundBase = 110,
            RepeatingGroundGood = 111,
            RepeatingMapGroundGood = 112,
            RepeatingGroundBad = 113,
            RepeatingMapGroundBad = 114,
            RepeatingSand = 115,
            RepeatingMapSand = 116,
            RepeatingBeachBright = 117,
            RepeatingMapBeachBright = 118,
            RepeatingBeachDark = 119,
            RepeatingMapBeachDark = 120,
            RepeatingSwamp = 121,
            RepeatingMapSwamp = 122,
            RepeatingRuins = 123,
            RepeatingMapRuins = 124,
            RepeatingLava = 125,
            RepeatingMapLava = 126,
            RepeatingVolcanoEdge = 127,
            RepeatingMapVolcanoEdge = 128,
            RepeatingCaveFloor = 129,
            RepeatingMapCaveFloor = 130,
            RepeatingCaveWall = 131,
            RepeatingMapCaveWall = 132,

            TriSliceBGMessageLogLeft = 133,
            TriSliceBGMessageLogMid = 134,
            TriSliceBGMessageLogRight = 135,

            TriSliceBGBuffLeft = 136,
            TriSliceBGBuffMid = 137,
            TriSliceBGBuffRight = 138,

            TriSliceBGMenuGoldLeft = 139,
            TriSliceBGMenuGoldMid = 140,
            TriSliceBGMenuGoldRight = 141,

            TriSliceBGMenuSilverLeft = 142,
            TriSliceBGMenuSilverMid = 143,
            TriSliceBGMenuSilverRight = 144,

            TriSliceBGMenuBrownLeft = 145,
            TriSliceBGMenuBrownMid = 146,
            TriSliceBGMenuBrownRight = 147,

            TriSliceBGMenuGrayLeft = 148,
            TriSliceBGMenuGrayMid = 149,
            TriSliceBGMenuGrayRight = 150,
        }

        private static readonly Dictionary<TextureName, string> filenamesForTextureNames = new()
        {
            { TextureName.Empty, "missing_texture" },
            { TextureName.LoadingWheel, "loading_wheel" },
            { TextureName.LoadingGfx, "loading_gfx" },
            { TextureName.LightSphereWhite, "light_white" },
            { TextureName.Cursor, "cursor" },
            { TextureName.Bed, "bed" },
            { TextureName.Animal, "animal" },
            { TextureName.New, "new" },
            { TextureName.Map, "map_big" },
            { TextureName.MapEdges, "map_big_edges" },
            { TextureName.WhiteCircleSmall, "small_white_circle" },
            { TextureName.Biceps, "biceps" },
            { TextureName.WhiteHorizontalLine, "line_horizontal_white" },
            { TextureName.MonoGame, "monogame" },
            { TextureName.BackpackMediumOutline, "backpack_medium_outline" },
            { TextureName.Star, "star" },
            { TextureName.FogCloud, "weather_fog_1" },
            { TextureName.GfxCorrupted, "gfx_corrupted" },
            { TextureName.GameLogo, "game_logo" },
            { TextureName.LogoWaveDistortion, "logo_wave_distortion" },

            { TextureName.BuffPoison, "buffs/buff_poison" },
            { TextureName.BuffRegen, "buffs/buff_regen" },
            { TextureName.BuffHeat, "buffs/buff_heat" },
            { TextureName.BuffWet, "buffs/buff_wet" },
            { TextureName.BuffHPPlus, "buffs/buff_hp_plus" },
            { TextureName.BuffHPMinus, "buffs/buff_hp_minus" },
            { TextureName.BuffMaxHPPlus, "buffs/buff_max_hp_plus" },
            { TextureName.BuffMaxHPMinus, "buffs/buff_max_hp_minus" },
            { TextureName.BuffStrPlus, "buffs/buff_str_plus" },
            { TextureName.BuffStrMinus, "buffs/buff_str_minus" },
            { TextureName.BuffLowHP, "buffs/buff_low_hp" },
            { TextureName.BuffSpeed, "buffs/buff_speed" },
            { TextureName.BuffSprint, "buffs/buff_sprint" },
            { TextureName.BuffCannotSprint, "buffs/buff_cannot_sprint" },
            { TextureName.BuffHaste, "buffs/buff_haste" },
            { TextureName.BuffMountainWalking, "buffs/buff_mountain_walking" },
            { TextureName.BuffWidth, "buffs/buff_width" },
            { TextureName.BuffHeight, "buffs/buff_height" },
            { TextureName.BuffPoisonRemove, "buffs/buff_poison_remove" },
            { TextureName.BuffHungry, "burger" },

            { TextureName.SimpleArrowUp, "simple_icons/arrow_up" },
            { TextureName.SimpleArrowRight, "simple_icons/arrow_right" },
            { TextureName.SimpleHeart, "simple_icons/heart" },
            { TextureName.SimpleInfinity, "simple_icons/infinity" },
            { TextureName.SimpleArea, "simple_icons/area" },
            { TextureName.SimpleBurger, "simple_icons/burger" },
            { TextureName.SimpleSapling, "simple_icons/sapling" },
            { TextureName.SimpleSleep, "simple_icons/sleep" },
            { TextureName.SimpleHourglass, "simple_icons/hourglass" },
            { TextureName.SimpleMoon, "simple_icons/moon" },
            { TextureName.SimpleSpeed, "simple_icons/speed" },
            { TextureName.SimpleStack, "simple_icons/stack" },

            { TextureName.VirtualJoypadBackground, "virtual_joypad_background" },
            { TextureName.VirtualJoypadStick, "virtual_joypad_stick" },

            { TextureName.InputMouseButtonLeft, "input/Mouse/Mouse_Left_Key_Light" },
            { TextureName.InputMouseButtonMiddle, "input/Mouse/Mouse_Middle_Key_Light" },
            { TextureName.InputMouseButtonRight, "input/Mouse/Mouse_Right_Key_Light" },
            { TextureName.InputMouseScrollUp, "input/Mouse/Mouse_Scroll_Up_Light" },
            { TextureName.InputMouseScrollDown, "input/Mouse/Mouse_Scroll_Down_Light" },

            { TextureName.VirtButtonBGReleased, "virtual_button" },
            { TextureName.VirtButtonBGPressed, "virtual_button_pressed" },

            { TextureName.VirtButtonEat, "input/VirtButton/eat" },
            { TextureName.VirtButtonDrink, "input/VirtButton/drink" },
            { TextureName.VirtButtonBow, "input/VirtButton/bow" },
            { TextureName.VirtButtonLighter, "input/VirtButton/lighter" },
            { TextureName.VirtButtonPlant, "input/VirtButton/plant" },
            { TextureName.VirtButtonDropFruit, "input/VirtButton/drop_fruit" },
            { TextureName.VirtButtonOpenContainer, "input/VirtButton/open_container" },
            { TextureName.VirtButtonGoToSleep, "input/VirtButton/sleep" },
            { TextureName.VirtButtonUseCampfire, "input/VirtButton/campfire" },
            { TextureName.VirtButtonCook, "input/VirtButton/cook" },
            { TextureName.VirtButtonCraft, "input/VirtButton/craft" },
            { TextureName.VirtButtonBrew, "input/VirtButton/brew" },
            { TextureName.VirtButtonSmelt, "input/VirtButton/smelt" },
            { TextureName.VirtButtonHarvestMeat, "input/VirtButton/harvest_meat" },
            { TextureName.VirtButtonUseAnvil, "input/VirtButton/use_anvil" },
            { TextureName.VirtButtonJump, "input/VirtButton/jump" },
            { TextureName.VirtButtonOffer, "input/VirtButton/offer" },
            { TextureName.VirtButtonEnterExit, "input/VirtButton/enter_exit" },

            { TextureName.ParticleCircleSharp, "particles/circle_16x16_sharp" },
            { TextureName.ParticleCircleSoft, "particles/circle_16x16_soft" },
            { TextureName.ParticleWeatherRain, "particles/water_drop" },
            { TextureName.ParticleBubble, "particles/bubble_16x16" },
            { TextureName.ParticleSmokePuff, "particles/smoke_puff" },
            { TextureName.ParticleDustPuff, "particles/dust_puff" },
            { TextureName.ParticleWaterEdgeDistortion, "particles/water_edge_distortion" },
            { TextureName.ParticleDebrisWood, "particles/debris_wood" },
            { TextureName.ParticleDebrisLeaf, "particles/debris_leaf" },
            { TextureName.ParticleDebrisPetal, "particles/petal_gray" },
            { TextureName.ParticleDebrisGrass, "particles/debris_grass" },
            { TextureName.ParticleDebrisStone, "particles/debris_stone" },
            { TextureName.ParticleDebrisCrystal, "particles/debris_crystal" },
            { TextureName.ParticleDebrisCeramic, "particles/debris_ceramic" },
            { TextureName.ParticleDebrisBlood, "particles/debris_blood" },
            { TextureName.ParticleDebrisStar, "particles/debris_star" },
            { TextureName.ParticleDebrisHeart, "particles/debris_heart" },
            { TextureName.ParticleDebrisSoot, "particles/debris_soot" },

            { TextureName.RepeatingOceanFloor, "repeating_textures/ocean_floor" },
            { TextureName.RepeatingWaterCaustics, "repeating_textures/water_caustics" },
            { TextureName.RepeatingFog, "repeating_textures/fog" },
            { TextureName.RepeatingStars, "repeating_textures/stars" },
            { TextureName.RepeatingCloudsWhite, "repeating_textures/clouds_white" },
            { TextureName.RepeatingCloudsDark, "repeating_textures/clouds_dark" },
            { TextureName.RepeatingCloudsShadows, "repeating_textures/clouds_shadows" },
            { TextureName.RepeatingLavaDistortion, "repeating_textures/lava_distortion" },
            { TextureName.RepeatingPerlinNoiseColor, "repeating_textures/perlin_noise_color" },
            { TextureName.RepeatingWaterDrops, "repeating_textures/water_drops" },
            { TextureName.RepeatingGrassGood, "repeating_textures/grass_good" },
            { TextureName.RepeatingGrassGoodNormal, "repeating_textures/grass_good_normal" },
            { TextureName.RepeatingMapGrassGood, "repeating_textures/map_grass_good" },
            { TextureName.RepeatingGrassBad, "repeating_textures/grass_bad" },
            { TextureName.RepeatingMapGrassBad, "repeating_textures/map_grass_bad" },
            { TextureName.RepeatingWaterSuperShallow, "repeating_textures/water_supershallow" },
            { TextureName.RepeatingMapWaterSuperShallow, "repeating_textures/map_water_supershallow" },
            { TextureName.RepeatingWaterMedium, "repeating_textures/water_medium" },
            { TextureName.RepeatingMapWaterMedium, "repeating_textures/map_water_medium" },
            { TextureName.RepeatingWaterDeep, "repeating_textures/water_deep" },
            { TextureName.RepeatingMapWaterDeep, "repeating_textures/map_water_deep" },
            { TextureName.RepeatingMountainLow, "repeating_textures/mountain_low" },
            { TextureName.RepeatingMapMountainLow, "repeating_textures/map_mountain_low" },
            { TextureName.RepeatingMountainMedium, "repeating_textures/mountain_medium" },
            { TextureName.RepeatingMapMountainMedium, "repeating_textures/map_mountain_medium" },
            { TextureName.RepeatingMountainHigh, "repeating_textures/mountain_high" },
            { TextureName.RepeatingMapMountainHigh, "repeating_textures/map_mountain_high" },
            { TextureName.RepeatingGroundBase, "repeating_textures/ground_base" },
            { TextureName.RepeatingMapGroundBase, "repeating_textures/map_ground_base" },
            { TextureName.RepeatingGroundGood, "repeating_textures/ground_good" },
            { TextureName.RepeatingMapGroundGood, "repeating_textures/map_ground_good" },
            { TextureName.RepeatingGroundBad, "repeating_textures/ground_bad" },
            { TextureName.RepeatingMapGroundBad, "repeating_textures/map_ground_bad" },
            { TextureName.RepeatingSand, "repeating_textures/sand" },
            { TextureName.RepeatingMapSand, "repeating_textures/map_sand" },
            { TextureName.RepeatingBeachBright, "repeating_textures/beach_bright" },
            { TextureName.RepeatingMapBeachBright, "repeating_textures/map_beach_bright" },
            { TextureName.RepeatingBeachDark, "repeating_textures/beach_dark" },
            { TextureName.RepeatingMapBeachDark, "repeating_textures/map_beach_dark" },
            { TextureName.RepeatingSwamp, "repeating_textures/swamp" },
            { TextureName.RepeatingMapSwamp, "repeating_textures/map_swamp" },
            { TextureName.RepeatingRuins, "repeating_textures/ruins" },
            { TextureName.RepeatingMapRuins, "repeating_textures/map_ruins" },
            { TextureName.RepeatingLava, "repeating_textures/lava" },
            { TextureName.RepeatingMapLava, "repeating_textures/map_lava" },
            { TextureName.RepeatingVolcanoEdge, "repeating_textures/volcano_edge" },
            { TextureName.RepeatingMapVolcanoEdge, "repeating_textures/map_volcano_edge" },
            { TextureName.RepeatingCaveFloor, "repeating_textures/mountain_low" }, // repeated (needs a separate texture)
            { TextureName.RepeatingMapCaveFloor, "repeating_textures/map_mountain_low" }, // repeated (needs a separate texture)
            { TextureName.RepeatingCaveWall, "repeating_textures/cave_wall" },
            { TextureName.RepeatingMapCaveWall, "repeating_textures/map_cave_wall" },

            { TextureName.TriSliceBGMessageLogLeft, "tri_slice_bg/message_bg_left" },
            { TextureName.TriSliceBGMessageLogMid, "tri_slice_bg/message_bg_mid" },
            { TextureName.TriSliceBGMessageLogRight, "tri_slice_bg/message_bg_right" },

            { TextureName.TriSliceBGBuffLeft, "tri_slice_bg/buff_bg_left" },
            { TextureName.TriSliceBGBuffMid, "tri_slice_bg/buff_bg_mid" },
            { TextureName.TriSliceBGBuffRight, "tri_slice_bg/buff_bg_right" },

            { TextureName.TriSliceBGMenuGoldLeft, "tri_slice_bg/menu_gold_bg_left" },
            { TextureName.TriSliceBGMenuGoldMid, "tri_slice_bg/menu_gold_bg_mid" },
            { TextureName.TriSliceBGMenuGoldRight, "tri_slice_bg/menu_gold_bg_right" },

            { TextureName.TriSliceBGMenuSilverLeft, "tri_slice_bg/menu_silver_bg_left" },
            { TextureName.TriSliceBGMenuSilverMid, "tri_slice_bg/menu_silver_bg_mid" },
            { TextureName.TriSliceBGMenuSilverRight, "tri_slice_bg/menu_silver_bg_right" },

            { TextureName.TriSliceBGMenuBrownLeft, "tri_slice_bg/menu_brown_bg_left" },
            { TextureName.TriSliceBGMenuBrownMid, "tri_slice_bg/menu_brown_bg_mid" },
            { TextureName.TriSliceBGMenuBrownRight, "tri_slice_bg/menu_brown_bg_right" },

            { TextureName.TriSliceBGMenuGrayLeft, "tri_slice_bg/menu_gray_bg_left" },
            { TextureName.TriSliceBGMenuGrayMid, "tri_slice_bg/menu_gray_bg_mid" },
            { TextureName.TriSliceBGMenuGrayRight, "tri_slice_bg/menu_gray_bg_right" },
        };

        private const string gfxFolderName = "gfx";

        private static ContentManager persistentTexturesManager;
        private static ContentManager temporaryTexturesManager;

        private static readonly Dictionary<string, Texture2D> textureByNamePersistent = new();
        private static readonly Dictionary<string, Texture2D> textureByNameTemporary = new();
        public static int LoadedTexturesCountPersistent { get { return textureByNamePersistent.Values.Count; } }
        public static int LoadedTexturesCountTemporary { get { return textureByNameTemporary.Values.Count; } }

        public static void AssignContentManagers(ContentManager persistentManager, ContentManager temporaryManager)
        {
            persistentTexturesManager = persistentManager;
            temporaryTexturesManager = temporaryManager;
        }

        public static string GetTextureNameString(TextureName textureName)
        {
            return filenamesForTextureNames[textureName];
        }

        public static void LoadAllTextures()
        {
            foreach (string filename in filenamesForTextureNames.Values)
            {
                GetTexturePersistent(filename);
            }
        }

        public static Texture2D GetTexture(TextureName textureName, bool persistent = true)
        {
            return GetTexture(fileName: filenamesForTextureNames[textureName], persistent: persistent);
        }

        public static ImageObj GetImageObj(TextureName textureName, bool persistent = true)
        {
            return new TextureObj(GetTexture(fileName: filenamesForTextureNames[textureName], persistent: persistent));
        }

        public static ImageObj GetImageObj(string fileName, bool persistent = true)
        {
            return new TextureObj(GetTexture(fileName: fileName, persistent: persistent));
        }

        public static Texture2D GetTexture(string fileName, bool persistent = true)
        {
            if (persistent) return GetTexturePersistent(fileName);
            else return GetTextureTemporary(fileName);
        }

        private static Texture2D GetTexturePersistent(string fileName)
        {
            if (!textureByNamePersistent.ContainsKey(fileName))
            {
                // MessageLog.Add(debugMessage: true, text: $"Loading texture: {fileName}");
                textureByNamePersistent[fileName] = persistentTexturesManager.Load<Texture2D>($"{gfxFolderName}/{fileName}");
            }

            return textureByNamePersistent[fileName];
        }

        private static Texture2D GetTextureTemporary(string fileName)
        {
            if (!textureByNameTemporary.ContainsKey(fileName))
            {
                // MessageLog.Add(debugMessage: true, text: $"Loading texture (temporary): {fileName}");
                textureByNameTemporary[fileName] = temporaryTexturesManager.Load<Texture2D>($"{gfxFolderName}/{fileName}");
            }

            return textureByNameTemporary[fileName];
        }

        public static void DisposeTexture(string fileName)
        {
            var textureDict = textureByNameTemporary.ContainsKey(fileName) ? textureByNameTemporary : textureByNamePersistent;

            try
            {
                textureDict[fileName].Dispose();
                textureDict.Remove(fileName);
            }
            catch (KeyNotFoundException)
            { }
        }

        public static void FlushTemporaryTextures()
        {
            temporaryTexturesManager.Unload(); // will make all temporary textures blank
            textureByNameTemporary.Clear();
        }
    }
}