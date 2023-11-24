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

            LoadingWheel = 1,
            LightSphereWhite = 2,
            Cursor = 3,
            Bed = 4,
            Animal = 5,
            New = 6,
            Map = 7,
            MapEdges = 8,
            WhiteCircleSmall = 9,
            Biceps = 10,
            WhiteHorizontalLine = 11,

            SimpleArrowUp = 12,
            SimpleArrowRight = 13,
            SimpleHeart = 14,
            SimpleInfinity = 15,
            SimpleArea = 16,
            SimpleBurger = 17,
            SimpleSapling = 18,
            SimpleSleep = 19,
            SimpleHourglass = 20,
            SimpleMoon = 21,
            SimpleSpeed = 22,
            SimpleStack = 23,

            BuffPoison = 24,
            BuffRegen = 25,
            BuffHeat = 26,
            BuffWet = 27,
            BuffHPPlus = 28,
            BuffHPMinus = 29,
            BuffMaxHPPlus = 30,
            BuffMaxHPMinus = 31,
            BuffStrPlus = 32,
            BuffStrMinus = 33,
            BuffLowHP = 34,
            BuffSpeed = 35,
            BuffSprint = 36,
            BuffCannotSprint = 37,
            BuffHaste = 38,
            BuffMountainWalking = 39,
            BuffWidth = 40,
            BuffHeight = 41,
            BuffPoisonRemove = 42,

            RepeatingOceanFloor = 43,
            RepeatingWaterCaustics = 44,
            RepeatingFog = 45,

            VirtualJoypadBackground = 46,
            VirtualJoypadStick = 47,

            InputMouseButtonLeft = 48,
            InputMouseButtonMiddle = 49,
            InputMouseButtonRight = 50,
            InputMouseScrollUp = 51,
            InputMouseScrollDown = 52,

            VirtButtonBGReleased = 53,
            VirtButtonBGPressed = 54,

            VirtButtonEat = 55,
            VirtButtonDrink = 56,
            VirtButtonBow = 57,
            VirtButtonLighter = 58,
            VirtButtonPlant = 59,
            VirtButtonDropFruit = 60,
            VirtButtonOpenContainer = 61,
            VirtButtonGoToSleep = 62,
            VirtButtonUseCampfire = 63,
            VirtButtonCook = 64,
            VirtButtonCraft = 65,
            VirtButtonBrew = 66,
            VirtButtonHarvestMeat = 67,
            VirtButtonUseAnvil = 68,
            VirtButtonJump = 69,
            VirtButtonOffer = 70,
            VirtButtonEnterExit = 71,

            ParticleCircleSharp = 72,
            ParticleCircleSoft = 73,
            ParticleWeatherRain = 74,
            ParticleBubble = 75,
            ParticleSmokePuff = 76,
            ParticleDustPuff = 77,
            ParticleDebrisWood = 78,
            ParticleDebrisLeaf = 79,
            ParticleDebrisPetal = 80,
            ParticleDebrisGrass = 81,
            ParticleDebrisStone = 82,
            ParticleDebrisCrystal = 83,
            ParticleDebrisCeramic = 84,
            ParticleDebrisBlood = 85,
            ParticleDebrisStar = 86,
            ParticleDebrisHeart = 87,
            ParticleDebrisSoot = 88,

            RepeatingPerlinNoiseColor = 89,
            RepeatingWaterDrops = 90,
            RepeatingGrassGood = 91,
            RepeatingMapGrassGood = 92,
            RepeatingGrassBad = 93,
            RepeatingMapGrassBad = 94,
            RepeatingWaterSuperShallow = 95,
            RepeatingMapWaterSuperShallow = 96,
            RepeatingWaterMedium = 97,
            RepeatingMapWaterMedium = 98,
            RepeatingWaterDeep = 99,
            RepeatingMapWaterDeep = 100,
            RepeatingMountainLow = 101,
            RepeatingMapMountainLow = 102,
            RepeatingMountainMedium = 103,
            RepeatingMapMountainMedium = 104,
            RepeatingMountainHigh = 105,
            RepeatingMapMountainHigh = 106,
            RepeatingGroundBase = 107,
            RepeatingMapGroundBase = 108,
            RepeatingGroundGood = 109,
            RepeatingMapGroundGood = 110,
            RepeatingGroundBad = 111,
            RepeatingMapGroundBad = 112,
            RepeatingSand = 113,
            RepeatingMapSand = 114,
            RepeatingBeachBright = 115,
            RepeatingMapBeachBright = 116,
            RepeatingBeachDark = 117,
            RepeatingMapBeachDark = 118,
            RepeatingSwamp = 119,
            RepeatingMapSwamp = 120,
            RepeatingRuins = 121,
            RepeatingMapRuins = 122,
            RepeatingLava = 123,
            RepeatingMapLava = 124,
            RepeatingVolcanoEdge = 125,
            RepeatingMapVolcanoEdge = 126,
            RepeatingCaveFloor = 127,
            RepeatingMapCaveFloor = 128,
            RepeatingCaveWall = 129,
            RepeatingMapCaveWall = 130,

            TriSliceBGMessageLogLeft = 131,
            TriSliceBGMessageLogMid = 132,
            TriSliceBGMessageLogRight = 133,

            TriSliceBGBuffLeft = 134,
            TriSliceBGBuffMid = 135,
            TriSliceBGBuffRight = 136,

            TriSliceBGMenuGoldLeft = 137,
            TriSliceBGMenuGoldMid = 138,
            TriSliceBGMenuGoldRight = 139,

            TriSliceBGMenuSilverLeft = 140,
            TriSliceBGMenuSilverMid = 141,
            TriSliceBGMenuSilverRight = 142,

            TriSliceBGMenuBrownLeft = 143,
            TriSliceBGMenuBrownMid = 144,
            TriSliceBGMenuBrownRight = 145,

            TriSliceBGMenuGrayLeft = 146,
            TriSliceBGMenuGrayMid = 147,
            TriSliceBGMenuGrayRight = 148,
        }

        private static readonly Dictionary<TextureName, string> filenamesForTextureNames = new Dictionary<TextureName, string>
        {
            { TextureName.Empty, "missing_texture" },
            { TextureName.LoadingWheel, "loading_wheel" },
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