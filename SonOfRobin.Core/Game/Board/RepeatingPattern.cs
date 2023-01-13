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
            grass_good,
            grass_bad,
            sea_supershallow,
            sea_shallow,
            sea_medium,
            sea_deep,
            mountain_low,
            mountain_medium,
            mountain_high,
            ground_good,
            ground_bad,
            sand,
            beach_bright,
            beach_dark,
            swamp,
            lava_bright,
            lava_dark,
            volcano_edge,
        };

        private static Dictionary<int, Name> namesForBaseColors = new Dictionary<int, Name>
        {
            { new Color(11,46,176,255).GetHashCode(), Name.sea_deep },
            { new Color(35,78,207,255).GetHashCode(), Name.sea_medium },
            { new Color(65,105,225,255).GetHashCode(), Name.sea_shallow },
            { new Color(85,125,245,255).GetHashCode(), Name.sea_supershallow },
            { new Color(141,181,67,255).GetHashCode(), Name.grass_bad },
            { new Color(78,186,0,255).GetHashCode(), Name.grass_good },
            { new Color(180,180,180,255).GetHashCode(), Name.mountain_low },
            { new Color(209,209,209,255).GetHashCode(), Name.mountain_medium },
            { new Color(225,225,225,255).GetHashCode(), Name.mountain_high },
            { new Color(207,167,58,255).GetHashCode(), Name.ground_bad },
            { new Color(173,128,54,255).GetHashCode(), Name.ground_good },
            { new Color(240,230,153,255).GetHashCode(), Name.beach_bright },
            { new Color(214,199,133,255).GetHashCode(), Name.beach_dark },
            { new Color(227,210,102,255).GetHashCode(), Name.sand },
            { new Color(83, 97, 55, 128).GetHashCode(), Name.swamp },
            { new Color(255,81,0,255).GetHashCode(), Name.lava_dark },
            { new Color(255,179,0,255).GetHashCode(), Name.lava_bright },
            { new Color(64,64,64,255).GetHashCode(), Name.volcano_edge },
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
                Texture2D texture = SonOfRobinGame.ContentMgr.Load<Texture2D>($"gfx/repeating textures/{name}");
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

        private static RepeatingPattern GetPatternForBaseColor(Color baseColor)
        {
            try
            {
                return patternDict[namesForBaseColors[baseColor.GetHashCode()]];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static Color GetValueForBaseColor(Color baseColor, int x, int y)
        {
            RepeatingPattern pattern = GetPatternForBaseColor(baseColor);

            return pattern == null ? baseColor : pattern.GetValue(x, y);
        }
    }
}