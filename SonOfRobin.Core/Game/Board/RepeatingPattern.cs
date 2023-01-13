using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class RepeatingPattern
    {
        private static readonly Dictionary<string, RepeatingPattern> patternDict = new Dictionary<string, RepeatingPattern>();

        public readonly string name;
        private readonly Color[,] colorGrid;
        private readonly int width;
        private readonly int height;

        public static void ConvertAllTexturesToPatterns()
        {
            var nameList = new List<string> { "sample" };

            foreach (string textureName in nameList)
            {
                Texture2D texture = SonOfRobinGame.ContentMgr.Load<Texture2D>($"gfx/repeating textures/{textureName}");
                new RepeatingPattern(name: textureName, texture: texture);
            }
        }

        public RepeatingPattern(string name, Texture2D texture)
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
    }
}