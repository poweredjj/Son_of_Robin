using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RepeatingPattern
    {
        public enum Name : byte
        {
            // lowercase to match filenames

            // plain colors:

            just_red = 0,
            just_blue = 1,
            just_white = 2,
            just_green = 3,
            just_cyan = 4,

            // textures:

            grass_good = 5,

            grass_bad = 6,
            water_supershallow = 7,
            water_shallow = 8,
            water_medium = 9,
            water_deep = 10,
            mountain_low = 11,
            mountain_medium = 12,
            mountain_high = 13,
            ground_good = 14,
            ground_bad = 15,
            sand = 16,
            beach_bright = 17,
            beach_dark = 18,
            swamp = 19,
            lava = 20,
            volcano_edge = 21,
        };

        public static readonly Name[] allNames = (Name[])Enum.GetValues(typeof(Name));

        public static readonly Dictionary<Name, RepeatingPattern> patternDict = new Dictionary<Name, RepeatingPattern>();

        public readonly Name name;
        private readonly Color[,] colorGrid;
        private readonly int width;
        private readonly int height;

        public static void ConvertAllTexturesToPatterns()
        {
            foreach (Name name in allNames)
            {
                Texture2D texture = TextureBank.GetTexture($"repeating textures/{name}");
                new RepeatingPattern(name: name, texture: texture);
            }
        }

        public RepeatingPattern(Name name, Texture2D texture)
        {
            if (patternDict.ContainsKey(name)) throw new ArgumentException($"Repeating pattern has already been added for '{name}'.");

            this.name = name;
            this.width = texture.Width;
            this.height = texture.Height;
            this.colorGrid = GfxConverter.ConvertTextureToGrid(texture: texture, x: 0, y: 0, width: texture.Width, height: texture.Height);
            texture.Dispose(); // the texture is not needed after that

            patternDict[this.name] = this;
        }

        public Color GetValue(Point point)
        {
            return this.GetValue(point.X, point.Y);
        }

        public Color GetValue(Vector2 vector)
        {
            return this.GetValue((int)vector.X, (int)vector.Y);
        }

        public Color GetValue(int x, int y)
        {
            return colorGrid[x % width, y % height];
        }
    }
}