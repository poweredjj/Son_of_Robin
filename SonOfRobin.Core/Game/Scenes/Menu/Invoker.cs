using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    internal class Invoker : Entry
    {
        private readonly bool closesMenu;
        private readonly Sound soundInvoke;
        public readonly Scheduler.TaskName taskName;
        public readonly Object executeHelper; // misc variables used in execution stage
        public readonly int taskDelay;
        private bool invokedByDoubleTouch;

        public Invoker(Menu menu, string name, Scheduler.TaskName taskName, Object executeHelper = null, int taskDelay = 0, bool closesMenu = false, bool rebuildsMenu = false, List<InfoWindow.TextEntry> infoTextList = null, SoundData.Name sound = SoundData.Name.Empty, bool playSound = true, List<Texture2D> imageList = null, float infoWindowMaxLineHeightPercentOverride = 0f, bool invokedByDoubleTouch = false) :

            base(menu: menu, name: name, rebuildsMenu: rebuildsMenu, infoTextList: infoTextList, imageList: imageList, infoWindowMaxLineHeightPercentOverride: infoWindowMaxLineHeightPercentOverride)
        {
            this.taskName = taskName;
            this.taskDelay = taskDelay;
            this.executeHelper = executeHelper;
            this.closesMenu = closesMenu;
            this.soundInvoke = sound == SoundData.Name.Empty ? this.menu.soundInvoke : new Sound(sound);
            if (!playSound) this.soundInvoke = new Sound(SoundData.Name.Empty);
            this.invokedByDoubleTouch = invokedByDoubleTouch;
        }

        public override string DisplayedText
        { get { return this.name; } }

        public override void Invoke()
        {
            if (Sound.menuOn) this.soundInvoke.Play();
            if (this.closesMenu) this.menu.Remove();
            this.menu.ChangeActiveItem(this);
            new Scheduler.Task(taskName: this.taskName, executeHelper: this.executeHelper, delay: this.taskDelay, turnOffInputUntilExecution: true);
            if (this.rebuildsMenu) new Scheduler.Task(taskName: Scheduler.TaskName.RebuildMenu, executeHelper: this.menu, delay: this.taskDelay, turnOffInputUntilExecution: true);
        }

        public override void Draw(bool active, string textOverride = null, List<Texture2D> imageList = null)
        {
            if (active) this.UpdateHintWindow();
            base.Draw(active);
        }

        public override void ProcessTouch()
        {
            Rectangle rect = this.Rect;
            rect.X += (int)this.menu.viewParams.PosX;
            rect.Y += (int)this.menu.viewParams.PosY;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 position = touch.Position / Preferences.GlobalScale;

                if (rect.Contains(position) && touch.State == TouchLocationState.Released && this.menu.CanInterpretTouchReleaseAsButtonPress)
                {
                    if (this == this.menu.lastTouchedEntry || !this.invokedByDoubleTouch) this.Invoke();
                    else
                    {
                        this.menu.lastTouchedEntry = this;
                        this.menu.SetActiveIndex(this.index);
                    }

                    return;
                }
            }
        }
    }
}