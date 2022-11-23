using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SonOfRobin
{
    internal class Separator : Entry
    {
        private readonly bool isEmpty;
        public override string DisplayedText
        { get { return this.name; } }

        public Separator(Menu menu, string name, bool isEmpty = false) : base(menu: menu, name: name)
        {
            this.textColor = new Color(70, 70, 70, 255);
            this.rectColor = Color.White;
            this.isEmpty = isEmpty;
        }

        public override void Draw(bool active, string textOverride = null, List<Texture2D> imageList = null)
        {
            if (!isEmpty) base.Draw(active: true);
        }
    }
}