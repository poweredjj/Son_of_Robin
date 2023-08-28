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
            MapEdges = 7,
            WhiteCircleSmall = 8,
            Biceps = 9,

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

            ScrollingOceanFloor = 22,
            ScrollingWaterCaustics1 = 23,
            ScrollingWaterCaustics2 = 24,
            ScrollingFog = 25,

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
            ParticleBubble = 53,
            ParticleDebrisWood = 54,
            ParticleDebrisLeaf = 55,
            ParticleDebrisGrass = 56,
            ParticleDebrisStone = 57,
            ParticleDebrisCrystal = 58,
            ParticleDebrisCeramic = 59,
            ParticleDebrisBlood = 60,
            ParticleDebrisStar = 61,
            ParticleDebrisHeart = 62,
            ParticleDebrisSoot = 63,
        }

        private static readonly Dictionary<TextureName, string> filenamesForTextureNames = new Dictionary<TextureName, string>
        {
            { TextureName.LoadingWheel, "loading_wheel" },
            { TextureName.LightSphereWhite, "light_white" },
            { TextureName.Cursor, "cursor" },
            { TextureName.Bed, "bed" },
            { TextureName.Animal, "animal" },
            { TextureName.New, "new" },
            { TextureName.MapEdges, "parchment_edges" },
            { TextureName.WhiteCircleSmall, "small_white_circle" },
            { TextureName.Biceps, "biceps" },

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

            { TextureName.ScrollingOceanFloor, "scrolling textures/ocean_floor" },
            { TextureName.ScrollingWaterCaustics1, "scrolling textures/water_caustics1" },
            { TextureName.ScrollingWaterCaustics2, "scrolling textures/water_caustics2" },
            { TextureName.ScrollingFog, "scrolling textures/fog" },

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
            { TextureName.ParticleBubble, "particles/bubble_16x16" },
            { TextureName.ParticleDebrisWood, "particles/debris_wood" },
            { TextureName.ParticleDebrisLeaf, "particles/debris_leaf" },
            { TextureName.ParticleDebrisGrass, "particles/debris_grass" },
            { TextureName.ParticleDebrisStone, "particles/debris_stone" },
            { TextureName.ParticleDebrisCrystal, "particles/debris_crystal" },
            { TextureName.ParticleDebrisCeramic, "particles/debris_ceramic" },
            { TextureName.ParticleDebrisBlood, "particles/debris_blood" },
            { TextureName.ParticleDebrisStar, "particles/debris_star" },
            { TextureName.ParticleDebrisHeart, "particles/debris_heart" },
            { TextureName.ParticleDebrisSoot, "particles/debris_soot" },
        };

        private const string gfxFolderName = "gfx";

        private static ContentManager persistentTexturesManager;
        private static ContentManager temporaryTexturesManager;

        private static Dictionary<string, Texture2D> textureByNamePersistent = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Texture2D> textureByNameTemporary = new Dictionary<string, Texture2D>();

        public static void AssignContentManagers(ContentManager persistentManager, ContentManager temporaryManager)
        {
            persistentTexturesManager = persistentManager;
            temporaryTexturesManager = temporaryManager;
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

        public static Texture2D GetTexturePersistent(string fileName)
        {
            if (!textureByNamePersistent.ContainsKey(fileName)) textureByNamePersistent[fileName] = persistentTexturesManager.Load<Texture2D>($"{gfxFolderName}/{fileName}");

            return textureByNamePersistent[fileName];
        }

        public static Texture2D GetTextureTemporary(string fileName)
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