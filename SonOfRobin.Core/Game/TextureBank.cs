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
            BackpackMediumOutline = 14,
            Star = 15,
            FogCloud = 16,
            GfxCorrupted = 17,
            GameLogo = 18,
            LogoWaveDistortion = 19,

            SimpleArrowUp = 20,
            SimpleArrowRight = 21,
            SimpleHeart = 22,
            SimpleInfinity = 23,
            SimpleArea = 24,
            SimpleBurger = 25,
            SimpleSapling = 26,
            SimpleSleep = 27,
            SimpleHourglass = 28,
            SimpleMoon = 29,
            SimpleSpeed = 30,
            SimpleStack = 31,

            BuffPoison = 32,
            BuffRegen = 33,
            BuffHeat = 34,
            BuffWet = 35,
            BuffHPPlus = 36,
            BuffHPMinus = 37,
            BuffMaxHPPlus = 38,
            BuffMaxHPMinus = 39,
            BuffStrPlus = 40,
            BuffStrMinus = 41,
            BuffLowHP = 42,
            BuffSpeed = 43,
            BuffSprint = 44,
            BuffCannotSprint = 45,
            BuffHaste = 46,
            BuffMountainWalking = 47,
            BuffWidth = 48,
            BuffHeight = 49,
            BuffPoisonRemove = 50,
            BuffHungry = 51,

            RepeatingOceanFloor = 52,
            RepeatingWaterCaustics = 53,
            RepeatingFog = 54,
            RepeatingStars = 55,
            RepeatingCloudsWhite = 56,
            RepeatingCloudsDark = 57,
            RepeatingCloudsShadows = 58,
            RepeatingLavaDistortion = 59,

            VirtualJoypadBackground = 60,
            VirtualJoypadStick = 61,

            InputMouseButtonLeft = 62,
            InputMouseButtonMiddle = 63,
            InputMouseButtonRight = 64,
            InputMouseScrollUp = 65,
            InputMouseScrollDown = 66,

            VirtButtonBGReleased = 67,
            VirtButtonBGPressed = 68,

            VirtButtonEat = 69,
            VirtButtonDrink = 70,
            VirtButtonBow = 71,
            VirtButtonLighter = 72,
            VirtButtonPlant = 73,
            VirtButtonDropFruit = 74,
            VirtButtonOpenContainer = 75,
            VirtButtonGoToSleep = 76,
            VirtButtonUseCampfire = 77,
            VirtButtonCook = 78,
            VirtButtonCraft = 79,
            VirtButtonBrew = 80,
            VirtButtonSmelt = 81,
            VirtButtonHarvestMeat = 82,
            VirtButtonUseAnvil = 83,
            VirtButtonJump = 84,
            VirtButtonOffer = 85,
            VirtButtonEnterExit = 86,

            ParticleCircleSharp = 87,
            ParticleCircleSoft = 88,
            ParticleWeatherRain = 89,
            ParticleBubble = 90,
            ParticleSmokePuff = 91,
            ParticleDustPuff = 92,
            ParticleWaterEdgeDistortion = 93,
            ParticleDebrisWood = 94,
            ParticleDebrisLeaf = 95,
            ParticleDebrisPetal = 96,
            ParticleDebrisGrass = 97,
            ParticleDebrisStone = 98,
            ParticleDebrisCrystal = 99,
            ParticleDebrisCeramic = 100,
            ParticleDebrisBlood = 101,
            ParticleDebrisStar = 102,
            ParticleDebrisHeart = 103,
            ParticleDebrisSoot = 104,

            RepeatingPerlinNoiseColor = 105,
            RepeatingWaterDrops = 106,
            RepeatingGrassGood = 107,
            RepeatingGrassGoodBadNormal = 108,
            RepeatingMapGrassGood = 109,
            RepeatingGrassBad = 110,
            RepeatingMapGrassBad = 111,
            RepeatingWaterSuperShallow = 112,
            RepeatingMapWaterSuperShallow = 113,
            RepeatingWaterMedium = 114,
            RepeatingMapWaterMedium = 115,
            RepeatingWaterDeep = 116,
            RepeatingMapWaterDeep = 117,
            RepeatingMountainLow = 118,
            RepeatingMountainAllNormal = 119,
            RepeatingMapMountainLow = 120,
            RepeatingMountainBase = 121,
            RepeatingMapMountainBase = 122,
            RepeatingMountainBaseNormal = 123,
            RepeatingMountainMedium = 124,
            RepeatingMapMountainMedium = 125,
            RepeatingMountainHigh = 126,
            RepeatingMapMountainHigh = 127,
            RepeatingGroundBase = 128,
            RepeatingMapGroundBase = 129,
            RepeatingGroundGood = 130,
            RepeatingGroundGoodNormal = 131,
            RepeatingMapGroundGood = 132,
            RepeatingGroundBad = 133,
            RepeatingGroundBadNormal = 134,
            RepeatingMapGroundBad = 135,
            RepeatingSand = 136,
            RepeatingSandNormal = 137,
            RepeatingMapSand = 138,
            RepeatingBeachBright = 139,
            RepeatingMapBeachBright = 140,
            RepeatingBeachDark = 141,
            RepeatingMapBeachDark = 142,
            RepeatingSwamp = 143,
            RepeatingMapSwamp = 144,
            RepeatingRuins = 145,
            RepeatingRuinsNormal = 146,
            RepeatingMapRuins = 147,
            RepeatingLava = 148,
            RepeatingMapLava = 149,
            RepeatingVolcanoEdge = 150,
            RepeatingVolcanoEdgeNormal = 151,
            RepeatingMapVolcanoEdge = 152,
            RepeatingCaveFloor = 153,
            RepeatingCaveFloorNormal = 154,
            RepeatingMapCaveFloor = 155,
            RepeatingCaveWall = 156,
            RepeatingCaveWallNormal = 157,
            RepeatingMapCaveWall = 158,

            TriSliceBGMessageLogLeft = 159,
            TriSliceBGMessageLogMid = 160,
            TriSliceBGMessageLogRight = 161,

            TriSliceBGBuffLeft = 162,
            TriSliceBGBuffMid = 163,
            TriSliceBGBuffRight = 164,

            TriSliceBGMenuGoldLeft = 165,
            TriSliceBGMenuGoldMid = 166,
            TriSliceBGMenuGoldRight = 167,

            TriSliceBGMenuSilverLeft = 168,
            TriSliceBGMenuSilverMid = 169,
            TriSliceBGMenuSilverRight = 170,

            TriSliceBGMenuBrownLeft = 171,
            TriSliceBGMenuBrownMid = 172,
            TriSliceBGMenuBrownRight = 173,

            TriSliceBGMenuGrayLeft = 174,
            TriSliceBGMenuGrayMid = 175,
            TriSliceBGMenuGrayRight = 176,
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
            { TextureName.RepeatingGrassGoodBadNormal, "repeating_textures/grass_good_bad_normal" },
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
            { TextureName.RepeatingMountainAllNormal, "repeating_textures/mountain_all_normal" },
            { TextureName.RepeatingMapMountainLow, "repeating_textures/map_mountain_low" },
            { TextureName.RepeatingMountainMedium, "repeating_textures/mountain_medium" },
            { TextureName.RepeatingMapMountainBase, "repeating_textures/map_mountain_base" },
            { TextureName.RepeatingMountainBase, "repeating_textures/mountain_base" },
            { TextureName.RepeatingMountainBaseNormal, "repeating_textures/mountain_base_normal" },
            { TextureName.RepeatingMapMountainMedium, "repeating_textures/map_mountain_medium" },
            { TextureName.RepeatingMountainHigh, "repeating_textures/mountain_high" },
            { TextureName.RepeatingMapMountainHigh, "repeating_textures/map_mountain_high" },
            { TextureName.RepeatingGroundBase, "repeating_textures/ground_base" },
            { TextureName.RepeatingMapGroundBase, "repeating_textures/map_ground_base" },
            { TextureName.RepeatingGroundGood, "repeating_textures/ground_good" },
            { TextureName.RepeatingGroundGoodNormal, "repeating_textures/ground_good_normal" },
            { TextureName.RepeatingMapGroundGood, "repeating_textures/map_ground_good" },
            { TextureName.RepeatingGroundBad, "repeating_textures/ground_bad" },
            { TextureName.RepeatingGroundBadNormal, "repeating_textures/ground_bad_normal" },
            { TextureName.RepeatingMapGroundBad, "repeating_textures/map_ground_bad" },
            { TextureName.RepeatingSand, "repeating_textures/sand" },
            { TextureName.RepeatingSandNormal, "repeating_textures/sand_normal" },
            { TextureName.RepeatingMapSand, "repeating_textures/map_sand" },
            { TextureName.RepeatingBeachBright, "repeating_textures/beach_bright" },
            { TextureName.RepeatingMapBeachBright, "repeating_textures/map_beach_bright" },
            { TextureName.RepeatingBeachDark, "repeating_textures/beach_dark" },
            { TextureName.RepeatingMapBeachDark, "repeating_textures/map_beach_dark" },
            { TextureName.RepeatingSwamp, "repeating_textures/swamp" },
            { TextureName.RepeatingMapSwamp, "repeating_textures/map_swamp" },
            { TextureName.RepeatingRuins, "repeating_textures/ruins" },
            { TextureName.RepeatingRuinsNormal, "repeating_textures/ruins_normal" },
            { TextureName.RepeatingMapRuins, "repeating_textures/map_ruins" },
            { TextureName.RepeatingLava, "repeating_textures/lava" },
            { TextureName.RepeatingMapLava, "repeating_textures/map_lava" },
            { TextureName.RepeatingVolcanoEdge, "repeating_textures/volcano_edge" },
            { TextureName.RepeatingVolcanoEdgeNormal, "repeating_textures/volcano_edge_normal" },
            { TextureName.RepeatingMapVolcanoEdge, "repeating_textures/map_volcano_edge" },
            { TextureName.RepeatingCaveFloor, "repeating_textures/cave_floor" },
            { TextureName.RepeatingCaveFloorNormal, "repeating_textures/cave_floor_normal" },
            { TextureName.RepeatingMapCaveFloor, "repeating_textures/map_cave_floor" },
            { TextureName.RepeatingCaveWall, "repeating_textures/cave_wall" },
            { TextureName.RepeatingCaveWallNormal, "repeating_textures/cave_wall_normal" },
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