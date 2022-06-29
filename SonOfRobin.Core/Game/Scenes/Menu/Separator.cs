using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    class Separator : Entry
    {
        private readonly bool isEmpty;
        public override string DisplayedText { get { return this.name; } }

        public Separator(Menu menu, string name, bool isEmpty = false) : base(menu: menu, name: name)
        {
            this.textColor = new Color(70, 70, 70, 255);
            this.rectColor = Color.White;
            this.isEmpty = isEmpty;
        }

        public override void Draw(bool active)
        {
            if (!isEmpty) base.Draw(active: true);
        }


    }
}
