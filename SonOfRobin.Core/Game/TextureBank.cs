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
            Map = 130,
            MapEdges = 7,
            WhiteCircleSmall = 8,
            Biceps = 9,
            WhiteHorizontalLine = 107,

            SimpleArrowUp = 10,
            SimpleArrowRight = 11,
            SimpleHeart = 12,
            SimpleInfinity = 13,
            SimpleArea = 14,
            SimpleBurger = 15,
            SimpleSapling = 16,
            SimpleSleep = 17,
            SimpleHourglass = 18,
            SimpleMoon = 19,
            SimpleSpeed = 20,
            SimpleStack = 21,

            BuffPoison = 108,
            BuffRegen = 109,
            BuffHeat = 110,
            BuffWet = 111,
            BuffHPPlus = 112,
            BuffHPMinus = 113,
            BuffMaxHPPlus = 114,
            BuffMaxHPMinus = 115,
            BuffStrPlus = 116,
            BuffStrMinus = 117,
            BuffLowHP = 118,
            BuffSpeed = 119,
            BuffSprint = 120,
            BuffCannotSprint = 121,
            BuffHaste = 122,
            BuffMountainWalking = 123,
            BuffWidth = 127,
            BuffHeight = 128,
            BuffPoisonRemove = 129,

            RepeatingOceanFloor = 22,
            RepeatingWaterCaustics = 23,
            RepeatingFog = 25,

            VirtualJoypadBackground = 26,
            VirtualJoypadStick = 27,

            InputMouseButtonLeft = 28,
            InputMouseButtonMiddle = 29,
            InputMouseButtonRight = 30,
            InputMouseScrollUp = 31,
            InputMouseScrollDown = 32,

            VirtButtonBGReleased = 33,
            VirtButtonBGPressed = 34,

            VirtButtonEat = 35,
            VirtButtonDrink = 36,
            VirtButtonBow = 37,
            VirtButtonLighter = 38,
            VirtButtonPlant = 39,
            VirtButtonDropFruit = 40,
            VirtButtonOpenContainer = 41,
            VirtButtonGoToSleep = 42,
            VirtButtonUseCampfire = 43,
            VirtButtonCook = 44,
            VirtButtonCraft = 45,
            VirtButtonBrew = 46,
            VirtButtonHarvestMeat = 47,
            VirtButtonUseAnvil = 48,
            VirtButtonJump = 49,
            VirtButtonOffer = 50,

            ParticleCircleSharp = 51,
            ParticleCircleSoft = 52,
            ParticleWeatherRain = 100,
            ParticleBubble = 53,
            ParticleSmokePuff = 101,
            ParticleDustPuff = 102,
            ParticleDebrisWood = 54,
            ParticleDebrisLeaf = 55,
            ParticleDebrisPetal = 103,
            ParticleDebrisGrass = 56,
            ParticleDebrisStone = 57,
            ParticleDebrisCrystal = 58,
            ParticleDebrisCeramic = 59,
            ParticleDebrisBlood = 60,
            ParticleDebrisStar = 61,
            ParticleDebrisHeart = 62,
            ParticleDebrisSoot = 63,

            RepeatingGrassGood = 64,
            RepeatingGrassBad = 65,
            RepeatingWaterSuperShallow = 66,
            RepeatingWaterMedium = 67,
            RepeatingWaterDeep = 68,
            RepeatingMountainLow = 69,
            RepeatingMountainMedium = 70,
            RepeatingMountainHigh = 71,
            RepeatingGroundBase = 72,
            RepeatingGroundGood = 73,
            RepeatingGroundBad = 74,
            RepeatingSand = 75,
            RepeatingBeachBright = 76,
            RepeatingBeachDark = 77,
            RepeatingSwamp = 78,
            RepeatingRuins = 79,
            RepeatingLava = 80,
            RepeatingVolcanoEdge = 81,
            RepeatingPerlinNoise = 140,

            RepeatingMapGrassGood = 82,
            RepeatingMapGrassBad = 83,
            RepeatingMapWaterSuperShallow = 84,
            RepeatingMapWaterMedium = 85,
            RepeatingMapWaterDeep = 86,
            RepeatingMapMountainLow = 87,
            RepeatingMapMountainMedium = 88,
            RepeatingMapMountainHigh = 89,
            RepeatingMapGroundBase = 90,
            RepeatingMapGroundGood = 91,
            RepeatingMapGroundBad = 92,
            RepeatingMapSand = 93,
            RepeatingMapBeachBright = 94,
            RepeatingMapBeachDark = 95,
            RepeatingMapSwamp = 96,
            RepeatingMapRuins = 97,
            RepeatingMapLava = 98,
            RepeatingMapVolcanoEdge = 99,

            TriSliceBGMessageLogLeft = 104,
            TriSliceBGMessageLogMid = 105,
            TriSliceBGMessageLogRight = 106,

            TriSliceBGBuffLeft = 124,
            TriSliceBGBuffMid = 125,
            TriSliceBGBuffRight = 126,

            TriSliceBGMenuGoldLeft = 131,
            TriSliceBGMenuGoldMid = 132,
            TriSliceBGMenuGoldRight = 133,

            TriSliceBGMenuSilverLeft = 134,
            TriSliceBGMenuSilverMid = 135,
            TriSliceBGMenuSilverRight = 136,

            TriSliceBGMenuBrownLeft = 137,
            TriSliceBGMenuBrownMid = 138,
            TriSliceBGMenuBrownRight = 139,
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

            { TextureName.RepeatingGrassGood, "repeating_textures/grass_good" },
            { TextureName.RepeatingGrassBad, "repeating_textures/grass_bad" },
            { TextureName.RepeatingWaterSuperShallow, "repeating_textures/water_supershallow" },
            { TextureName.RepeatingWaterMedium, "repeating_textures/water_medium" },
            { TextureName.RepeatingWaterDeep, "repeating_textures/water_deep" },
            { TextureName.RepeatingMountainLow, "repeating_textures/mountain_low" },
            { TextureName.RepeatingMountainMedium, "repeating_textures/mountain_medium" },
            { TextureName.RepeatingMountainHigh, "repeating_textures/mountain_high" },
            { TextureName.RepeatingGroundBase, "repeating_textures/ground_base" },
            { TextureName.RepeatingGroundGood, "repeating_textures/ground_good" },
            { TextureName.RepeatingGroundBad, "repeating_textures/ground_bad" },
            { TextureName.RepeatingSand, "repeating_textures/sand" },
            { TextureName.RepeatingBeachBright, "repeating_textures/beach_bright" },
            { TextureName.RepeatingBeachDark, "repeating_textures/beach_dark" },
            { TextureName.RepeatingSwamp, "repeating_textures/swamp" },
            { TextureName.RepeatingRuins, "repeating_textures/ruins" },
            { TextureName.RepeatingLava, "repeating_textures/lava" },
            { TextureName.RepeatingVolcanoEdge, "repeating_textures/volcano_edge" },
            { TextureName.RepeatingPerlinNoise, "repeating_textures/perlin_noise" },

            { TextureName.RepeatingMapGrassGood, "repeating_textures/map_grass_good" },
            { TextureName.RepeatingMapGrassBad, "repeating_textures/map_grass_bad" },
            { TextureName.RepeatingMapWaterSuperShallow, "repeating_textures/map_water_supershallow" },
            { TextureName.RepeatingMapWaterMedium, "repeating_textures/map_water_medium" },
            { TextureName.RepeatingMapWaterDeep, "repeating_textures/map_water_deep" },
            { TextureName.RepeatingMapMountainLow, "repeating_textures/map_mountain_low" },
            { TextureName.RepeatingMapMountainMedium, "repeating_textures/map_mountain_medium" },
            { TextureName.RepeatingMapMountainHigh, "repeating_textures/map_mountain_high" },
            { TextureName.RepeatingMapGroundBase, "repeating_textures/map_ground_base" },
            { TextureName.RepeatingMapGroundGood, "repeating_textures/map_ground_good" },
            { TextureName.RepeatingMapGroundBad, "repeating_textures/map_ground_bad" },
            { TextureName.RepeatingMapSand, "repeating_textures/map_sand" },
            { TextureName.RepeatingMapBeachBright, "repeating_textures/map_beach_bright" },
            { TextureName.RepeatingMapBeachDark, "repeating_textures/map_beach_dark" },
            { TextureName.RepeatingMapSwamp, "repeating_textures/map_swamp" },
            { TextureName.RepeatingMapRuins, "repeating_textures/map_ruins" },
            { TextureName.RepeatingMapLava, "repeating_textures/map_lava" },
            { TextureName.RepeatingMapVolcanoEdge, "repeating_textures/map_volcano_edge" },

            { TextureName.RepeatingOceanFloor, "repeating_textures/ocean_floor" },
            { TextureName.RepeatingWaterCaustics, "repeating_textures/water_caustics" },
            { TextureName.RepeatingFog, "repeating_textures/fog" },

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