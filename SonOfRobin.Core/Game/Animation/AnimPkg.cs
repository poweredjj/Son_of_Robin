using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AnimPkg
    {
        public readonly AnimData.PkgName name;
        public readonly Rectangle colRect;
        private readonly Dictionary<int, Dictionary<string, Anim>> animDict;
        public readonly bool horizontalOrientationsOnly;
        public AnimFrame presentationFrame; // to be used in messages, menus, etc.
        public List<Anim> AllAnimList { get; private set; } // "flat" list of all anims
        public int[] AllAnimSizes { get { return this.animDict.Keys.ToArray(); } }

        public AnimPkg(AnimData.PkgName pkgName, int colWidth, int colHeight, bool horizontalOrientationsOnly = false)
        {
            this.name = pkgName;
            this.colRect = new Rectangle(x: -colWidth / 2, y: -colHeight / 2, width: colWidth, height: colHeight); // colRect should always be centered
            this.animDict = [];
            this.AllAnimList = [];
            this.horizontalOrientationsOnly = horizontalOrientationsOnly;
        }

        public void AddAnim(Anim anim)
        {
            if (!this.animDict.ContainsKey(anim.size)) this.animDict[anim.size] = [];
            if (this.animDict[anim.size].ContainsKey(anim.name)) return; // to prevent from adding multiple animations with the same size and name
            this.animDict[anim.size][anim.name] = anim;
            this.AllAnimList.Add(anim);
            this.presentationFrame = anim.frameArray[0];
        }

        public AnimPkg MakeCopyWithEditedColOffset(int colWidth, int colHeight)
        {
            AnimPkg animPkg = new AnimPkg(pkgName: this.name, colWidth: colWidth, colHeight: colHeight, horizontalOrientationsOnly: this.horizontalOrientationsOnly);

            foreach (Anim anim in this.AllAnimList)
            {
                animPkg.AddAnim(anim);
            }

            animPkg.presentationFrame = this.presentationFrame;

            return animPkg;
        }

        public void EditGfxOffsetCorrection(Vector2 gfxOffsetCorrection)
        {
            foreach (Anim anim in this.AllAnimList)
            {
                anim.EditGfxOffsetCorrection(gfxOffsetCorrection);
            }
        }

        public Anim GetAnim(int size, string name)
        {
            return this.animDict[size][name];
        }

        public string[] GetAnimNamesForSize(int size)
        {
            return this.animDict[size].Keys.ToArray();
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