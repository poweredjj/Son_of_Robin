using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SonOfRobin
{
    internal class Separator : Entry
    {
        private readonly bool isEmpty;

        public Separator(Menu menu, string name, bool isEmpty = false, List<InfoWindow.TextEntry> infoTextList = null) :
            base(menu: menu, name: name, infoTextList: isEmpty ? null : infoTextList)
        {
            this.isEmpty = isEmpty;
        }

        public override string DisplayedText
        { get { return this.name; } }

        public override void Draw(bool active, string textOverride = null, List<Texture2D> imageList = null)
        {
            if (!isEmpty) base.Draw(active: true);
        }
    }
}