using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;


namespace SonOfRobin
{
    class Invoker : Entry
    {
        private readonly bool closesMenu;
        public readonly Scheduler.TaskName taskName;
        public readonly Object executeHelper; // misc variables used in execution stage
        public override string DisplayedText { get { return this.name; } }

        public Invoker(Menu menu, string name, Scheduler.TaskName taskName, Object executeHelper = null, bool closesMenu = false, bool rebuildsMenu = false) : base(menu: menu, name: name, rebuildsMenu: rebuildsMenu)
        {
            this.taskName = taskName;
            this.closesMenu = closesMenu;
            this.rectColor = Color.LightSlateGray;
            this.executeHelper = executeHelper;
        }

        public override void Invoke()
        {
            if (this.closesMenu) this.menu.Remove();
            new Scheduler.Task(menu: this.menu, taskName: this.taskName, executeHelper: this.executeHelper, rebuildsMenu: rebuildsMenu, delay: 0);
        }

        public override void ProcessTouch()
        {
            Rectangle rect = this.Rect;
            rect.X += (int)this.menu.viewParams.posX;
            rect.Y += (int)this.menu.viewParams.posY;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 position = touch.Position / Preferences.globalScale;

                if (rect.Contains(position) && touch.State == TouchLocationState.Pressed)
                {
                    this.Invoke();
                    return;
                }
            }
        }

    }
}
