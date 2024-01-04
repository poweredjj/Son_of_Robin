using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class AnimPkg
    {
        public readonly AnimData.PkgName pkgName;
        public readonly Rectangle colRect;
        private readonly Dictionary<int, Dictionary<string, Anim>> animDict;
        public readonly bool horizontalOrientationsOnly;

        public AnimPkg(AnimData.PkgName pkgName, int colWidth, int colHeight, bool horizontalOrientationsOnly = false)
        {
            this.pkgName = pkgName;
            this.colRect = new Rectangle(x: -colWidth / 2, y: -colHeight / 2, width: colWidth, height: colHeight); // colRect should always be centered
            this.animDict = [];
            this.horizontalOrientationsOnly = horizontalOrientationsOnly;
        }

        public void AddAnim(Anim anim)
        {
            if (!this.animDict.ContainsKey(anim.size)) this.animDict[anim.size] = [];
            this.animDict[anim.size][anim.name] = anim;
        }

        public Anim GetAnim(int size, string name)
        {
            return this.animDict[size][name];
        }

        public Rectangle GetColRectForPos(Vector2 position)
        {
            return new(
                x: (int)position.X + this.colRect.X,
                y: (int)position.Y + this.colRect.Y,
                width: this.colRect.Width,
                height: this.colRect.Height);
        }
    }
}