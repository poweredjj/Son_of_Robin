using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RepeatingPattern
    {
        public enum Name
        { // lowercase to match filenames
            // plain colors
            just_red,

            just_blue,
            just_white,
            just_green,
            just_cyan,

            // textures
            grass_good,

            grass_bad,
            water_supershallow,
            water_shallow,
            water_medium,
            water_deep,
            mountain_low,
            mountain_medium,
            mountain_high,
            ground_good,
            ground_bad,
            sand,
            beach_bright,
            beach_dark,
            swamp,
            lava,
            volcano_edge,
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