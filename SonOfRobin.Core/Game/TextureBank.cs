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
            MonoGame = 150,

            SimpleArrowUp = 13,
            SimpleArrowRight = 14,
            SimpleHeart = 15,
            SimpleInfinity = 16,
            SimpleArea = 17,
            SimpleBurger = 18,
            SimpleSapling = 19,
            SimpleSleep = 20,
            SimpleHourglass = 21,
            SimpleMoon = 22,
            SimpleSpeed = 23,
            SimpleStack = 24,

            BuffPoison = 25,
            BuffRegen = 26,
            BuffHeat = 27,
            BuffWet = 28,
            BuffHPPlus = 29,
            BuffHPMinus = 30,
            BuffMaxHPPlus = 31,
            BuffMaxHPMinus = 32,
            BuffStrPlus = 33,
            BuffStrMinus = 34,
            BuffLowHP = 35,
            BuffSpeed = 36,
            BuffSprint = 37,
            BuffCannotSprint = 38,
            BuffHaste = 39,
            BuffMountainWalking = 40,
            BuffWidth = 41,
            BuffHeight = 42,
            BuffPoisonRemove = 43,

            RepeatingOceanFloor = 44,
            RepeatingWaterCaustics = 45,
            RepeatingFog = 46,

            VirtualJoypadBackground = 47,
            VirtualJoypadStick = 48,

            InputMouseButtonLeft = 49,
            InputMouseButtonMiddle = 50,
            InputMouseButtonRight = 51,
            InputMouseScrollUp = 52,
            InputMouseScrollDown = 53,

            VirtButtonBGReleased = 54,
            VirtButtonBGPressed = 55,

            VirtButtonEat = 56,
            VirtButtonDrink = 57,
            VirtButtonBow = 58,
            VirtButtonLighter = 59,
            VirtButtonPlant = 60,
            VirtButtonDropFruit = 61,
            VirtButtonOpenContainer = 62,
            VirtButtonGoToSleep = 63,
            VirtButtonUseCampfire = 64,
            VirtButtonCook = 65,
            VirtButtonCraft = 66,
            VirtButtonBrew = 67,
            VirtButtonHarvestMeat = 68,
            VirtButtonUseAnvil = 69,
            VirtButtonJump = 70,
            VirtButtonOffer = 71,
            VirtButtonEnterExit = 72,

            ParticleCircleSharp = 73,
            ParticleCircleSoft = 74,
            ParticleWeatherRain = 75,
            ParticleBubble = 76,
            ParticleSmokePuff = 77,
            ParticleDustPuff = 78,
            ParticleDebrisWood = 79,
            ParticleDebrisLeaf = 80,
            ParticleDebrisPetal = 81,
            ParticleDebrisGrass = 82,
            ParticleDebrisStone = 83,
            ParticleDebrisCrystal = 84,
            ParticleDebrisCeramic = 85,
            ParticleDebrisBlood = 86,
            ParticleDebrisStar = 87,
            ParticleDebrisHeart = 88,
            ParticleDebrisSoot = 89,

            RepeatingPerlinNoiseColor = 90,
            RepeatingWaterDrops = 91,
            RepeatingGrassGood = 92,
            RepeatingMapGrassGood = 93,
            RepeatingGrassBad = 94,
            RepeatingMapGrassBad = 95,
            RepeatingWaterSuperShallow = 96,
            RepeatingMapWaterSuperShallow = 97,
            RepeatingWaterMedium = 98,
            RepeatingMapWaterMedium = 99,
            RepeatingWaterDeep = 100,
            RepeatingMapWaterDeep = 101,
            RepeatingMountainLow = 102,
            RepeatingMapMountainLow = 103,
            RepeatingMountainMedium = 104,
            RepeatingMapMountainMedium = 105,
            RepeatingMountainHigh = 106,
            RepeatingMapMountainHigh = 107,
            RepeatingGroundBase = 108,
            RepeatingMapGroundBase = 109,
            RepeatingGroundGood = 110,
            RepeatingMapGroundGood = 111,
            RepeatingGroundBad = 112,
            RepeatingMapGroundBad = 113,
            RepeatingSand = 114,
            RepeatingMapSand = 115,
            RepeatingBeachBright = 116,
            RepeatingMapBeachBright = 117,
            RepeatingBeachDark = 118,
            RepeatingMapBeachDark = 119,
            RepeatingSwamp = 120,
            RepeatingMapSwamp = 121,
            RepeatingRuins = 122,
            RepeatingMapRuins = 123,
            RepeatingLava = 124,
            RepeatingMapLava = 125,
            RepeatingVolcanoEdge = 126,
            RepeatingMapVolcanoEdge = 127,
            RepeatingCaveFloor = 128,
            RepeatingMapCaveFloor = 129,
            RepeatingCaveWall = 130,
            RepeatingMapCaveWall = 131,

            TriSliceBGMessageLogLeft = 132,
            TriSliceBGMessageLogMid = 133,
            TriSliceBGMessageLogRight = 134,

            TriSliceBGBuffLeft = 135,
            TriSliceBGBuffMid = 136,
            TriSliceBGBuffRight = 137,

            TriSliceBGMenuGoldLeft = 138,
            TriSliceBGMenuGoldMid = 139,
            TriSliceBGMenuGoldRight = 140,

            TriSliceBGMenuSilverLeft = 141,
            TriSliceBGMenuSilverMid = 142,
            TriSliceBGMenuSilverRight = 143,

            TriSliceBGMenuBrownLeft = 144,
            TriSliceBGMenuBrownMid = 145,
            TriSliceBGMenuBrownRight = 146,

            TriSliceBGMenuGrayLeft = 147,
            TriSliceBGMenuGrayMid = 148,
            TriSliceBGMenuGrayRight = 149,
        }

        private static readonly Dictionary<TextureName, string> filenamesForTextureNames = new Dictionary<TextureName, string>
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
            { TextureName.RepeatingPerlinNoiseColor, "repeating_textures/perlin_noise_color" },
            { TextureName.RepeatingWaterDrops, "repeating_textures/water_drops" },
            { TextureName.RepeatingGrassGood, "repeating_textures/grass_good" },
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

        public static void AssignContentManagers(ContentManager persistentManager, ContentManager temporaryManager)
        {
            persistentTexturesManager = persistentManager;
            temporaryTexturesManager = temporaryManager;
        }

        public static string GetTextureNameString(TextureName textureName)
        {
            return filenamesForTextureNames[textureName];
        }

        public static Texture2D GetTexture(TextureName textureName, bool persistent = true)
        {
            return GetTexture(fileName: filenamesForTextureNames[textureName], persistent: persistent);
        }

        public static Texture2D GetTexture(string fileName, bool persistent = true)
        {
            if (persistent) return GetTexturePersistent(fileName);
            else return GetTextureTemporary(fileName);
        }

        private static Texture2D GetTexturePersistent(string fileName)
        {
            if (!textureByNamePersistent.ContainsKey(fileName)) textureByNamePersistent[fileName] = persistentTexturesManager.Load<Texture2D>($"{gfxFolderName}/{fileName}");

            return textureByNamePersistent[fileName];
        }

        private static Texture2D GetTextureTemporary(string fileName)
        {
            if (!textureByNameTemporary.ContainsKey(fileName)) textureByNameTemporary[fileName] = temporaryTexturesManager.Load<Texture2D>($"{gfxFolderName}/{fileName}");

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