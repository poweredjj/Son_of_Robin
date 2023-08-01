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

            LoadingWheel = 12,
            LightSphereWhite = 13,
            Cursor = 21,
            Bed = 22,
            Animal = 30,
            New = 31,
            MapEdges = 32,
            WhiteCircleSmall = 33,
            Biceps = 34,

            SimpleArrowUp = 0,
            SimpleArrowRight = 1,
            SimpleHeart = 2,
            SimpleInfinity = 3,
            SimpleArea = 4,
            SimpleBurger = 5,
            SimpleSapling = 6,
            SimpleSleep = 7,
            SimpleHourglass = 8,
            SimpleMoon = 9,
            SimpleSpeed = 19,
            SimpleStack = 20,

            ScrollingOceanFloor = 23,
            ScrollingWaterCaustics1 = 24,
            ScrollingWaterCaustics2 = 25,
            ScrollingFog = 26,

            VirtualJoypadBackground = 10,
            VirtualJoypadStick = 11,

            InputMouseButtonLeft = 14,
            InputMouseButtonMiddle = 15,
            InputMouseButtonRight = 16,
            InputMouseScrollUp = 17,
            InputMouseScrollDown = 18,

            VirtButtonBGReleased = 28,
            VirtButtonBGPressed = 29,

            VirtButtonEat = 35,
            VirtButtonDrink = 36,
            VirtButtonBow = 37,
            VirtButtonLighter = 38,
            VirtButtonPlant = 39,
            VirtButtonDropFruit = 40,
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