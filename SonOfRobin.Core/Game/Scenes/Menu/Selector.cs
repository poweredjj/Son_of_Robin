using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    class Selector : Entry
    {
        private readonly Dictionary<object, object> valueDict; // real value : displayed name
        private int activeIndex;
        private readonly Object targetObj;
        private readonly string propertyName;

        private Object ActiveName { get { return valueDict.Values.ToList()[activeIndex]; } }
        private Object ActiveValue { get { return valueDict.Keys.ToList()[activeIndex]; } }

        public override string DisplayedText { get { return $"{this.name}: < {this.ActiveName} >"; } }

        public Selector(Menu menu, string name, List<Object> valueList, Object targetObj, string propertyName, bool rebuildsMenu = false, List<InfoWindow.TextEntry> infoTextList = null, bool rebuildsMenuInstantScroll = false) : base(menu: menu, name: name, rebuildsMenu: rebuildsMenu, infoTextList: infoTextList, rebuildsMenuInstantScroll: rebuildsMenuInstantScroll)
        {
            this.targetObj = targetObj;
            this.propertyName = propertyName;

            this.valueDict = new Dictionary<object, object> { };
            foreach (var value in valueList)
            { valueDict[value] = value; }

            this.CompleteCreation();
        }

        public Selector(Menu menu, string name, Dictionary<object, object> valueDict, Object targetObj, string propertyName, bool rebuildsMenu = false, bool rebuildsMenuInstantScroll = false, List<InfoWindow.TextEntry> infoTextList = null) : base(menu: menu, name: name, rebuildsMenu: rebuildsMenu, rebuildsMenuInstantScroll: rebuildsMenuInstantScroll, infoTextList: infoTextList)
        {
            this.targetObj = targetObj;
            this.propertyName = propertyName;

            this.valueDict = valueDict;

            this.CompleteCreation();
        }

        private void CompleteCreation()
        {
            this.activeIndex = 0;
            this.SetDefaultValue();

            this.rectColor = Color.SteelBlue;
        }

        private void SetDefaultValue()
        {
            Object defaultValue = Helpers.GetProperty(targetObj: this.targetObj, propertyName: this.propertyName);

            int index = 0;
            foreach (var kvp in this.valueDict)
            {
                if (kvp.Key.Equals(defaultValue) || object.ReferenceEquals(kvp.Key, defaultValue))
                {
                    this.activeIndex = index;
                    return;
                }
                index++;
            }
        }

        private void SetNewValueToTargetObject()
        {
            Helpers.SetProperty(targetObj: this.targetObj, propertyName: this.propertyName, newValue: this.ActiveValue);
            if (this.rebuildsMenu) this.menu.Rebuild(instantScroll: this.rebuildsMenuInstantScroll);
        }

        public override void NextValue(bool touchMode)
        {
            base.NextValue(touchMode: touchMode);
            this.activeIndex++;
            if (this.activeIndex >= this.valueDict.ToList().Count) this.activeIndex = 0;

            this.SetNewValueToTargetObject();
        }

        public override void PreviousValue(bool touchMode)
        {
            base.PreviousValue(touchMode: touchMode);
            this.activeIndex--;
            if (this.activeIndex < 0) this.activeIndex = this.valueDict.ToList().Count - 1;

            this.SetNewValueToTargetObject();
        }

        public override void Draw(bool active)
        {
            if (active) this.UpdateHintWindow();
            base.Draw(active);
        }


        public override void ProcessTouch()
        {
            Rectangle rect = this.Rect;
            rect.X += (int)this.menu.viewParams.PosX;
            rect.Y += (int)this.menu.viewParams.PosY;


            Rectangle leftHalfRect = new Rectangle(rect.Left, rect.Top, rect.Width / 2, rect.Height);
            Rectangle rightHalfRect = new Rectangle(rect.Left + rect.Width / 2, rect.Top, rect.Width / 2, rect.Height);


            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 position = touch.Position / Preferences.GlobalScale;

                if (leftHalfRect.Contains(position) && touch.State == TouchLocationState.Pressed)
                {
                    this.PreviousValue(touchMode: true);
                    return;
                }

                if (rightHalfRect.Contains(position) && touch.State == TouchLocationState.Pressed)
                {
                    this.NextValue(touchMode: true);
                    return;
                }

            }
        }


    }
}
