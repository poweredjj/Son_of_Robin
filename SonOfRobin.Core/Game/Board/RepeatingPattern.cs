using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RepeatingPattern
    {
        public enum Name
        { grass, stones, water, ground, sand }; // lowercase to match filenames

        private static Dictionary<Color, Name> namesForBaseColors = new Dictionary<Color, Name>
        {
            { new Color(11,46,176,255), Name.water },
            { new Color(35,78,207,255), Name.water },
            { new Color(65,105,225,255), Name.water },
            { new Color(141,181,67,255), Name.grass },
            { new Color(78,186,0,255), Name.grass },
            { new Color(180,180,180,255), Name.stones },
            { new Color(209,209,209,255), Name.stones },
            { new Color(225,225,225,255), Name.stones },
            { new Color(207,167,58,255), Name.ground },
            { new Color(173,128,54,255), Name.ground },
            { new Color(214,199,133,255), Name.sand },
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
            texture.Dispose(); // texture is not needed after that

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
                return patternDict[namesForBaseColors[baseColor]];
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