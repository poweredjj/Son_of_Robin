using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    internal class Selector : Entry
    {
        private readonly Dictionary<object, object> valueDict; // real value : displayed name
        private int activeIndex;
        private readonly Sound soundSelect;
        private readonly Object targetObj;
        private readonly string propertyName;
        private readonly bool captureInput;
        private readonly bool captureButtons;
        private readonly bool captureKeys;

        private bool captureModeActive;

        private Object ActiveName
        { get { return valueDict.Values.ElementAt(activeIndex); } }

        private Object ActiveValue
        { get { return valueDict.Keys.ElementAt(activeIndex); } }

        private bool ActiveNameIsTexture
        { get { return this.ActiveName.GetType() == typeof(Texture2D); } }

        public override string DisplayedText
        { get { return $"{this.name}   < {this.ActiveName} >"; } }

        public Selector(Menu menu, string name, List<Object> valueList, Object targetObj, string propertyName, bool rebuildsMenu = false, bool rebuildsAllMenus = false, List<InfoWindow.TextEntry> infoTextList = null, bool rebuildsMenuInstantScroll = false, bool captureInput = false, bool captureButtons = false, bool captureKeys = false, SoundData.Name sound = SoundData.Name.Empty) : base(menu: menu, name: name, rebuildsMenu: rebuildsMenu, infoTextList: infoTextList, rebuildsAllMenus: rebuildsAllMenus, rebuildsMenuInstantScroll: rebuildsMenuInstantScroll)
        {
            this.targetObj = targetObj;
            this.propertyName = propertyName;

            this.valueDict = new Dictionary<object, object> { };
            foreach (var value in valueList)
            { valueDict[value] = value; }

            this.captureInput = captureInput;
            this.captureButtons = captureButtons;
            this.captureKeys = captureKeys;
            if (this.captureInput && !this.captureButtons && !this.captureKeys) throw new ArgumentException("No input could be captured.");
            this.captureModeActive = false;

            this.soundSelect = sound == SoundData.Name.Empty ? this.menu.soundSelect : new Sound(sound);

            this.CompleteCreation();
        }

        public Selector(Menu menu, string name, Dictionary<object, object> valueDict, Object targetObj, string propertyName, bool rebuildsMenu = false, bool rebuildsMenuInstantScroll = false, bool rebuildsAllMenus = false, List<InfoWindow.TextEntry> infoTextList = null, bool captureInput = false, bool captureButtons = false, bool captureKeys = false, SoundData.Name sound = SoundData.Name.Empty) :

            base(menu: menu, name: name, rebuildsMenu: rebuildsMenu, rebuildsMenuInstantScroll: rebuildsMenuInstantScroll, rebuildsAllMenus: rebuildsAllMenus, infoTextList: infoTextList)
        {
            this.targetObj = targetObj;
            this.propertyName = propertyName;

            this.valueDict = valueDict;

            this.captureInput = captureInput;
            this.captureButtons = captureButtons;
            this.captureKeys = captureKeys;
            if (this.captureInput && !this.captureButtons && !this.captureKeys) throw new ArgumentException("No input could be captured.");
            this.captureModeActive = false;

            this.soundSelect = sound == SoundData.Name.Empty ? this.menu.soundSelect : new Sound(sound);

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
            if (this.rebuildsAllMenus) Menu.RebuildAllMenus();
        }

        public override void NextValue(bool touchMode)
        {
            base.NextValue(touchMode: touchMode);
            this.menu.ChangeActiveItem(this);
            this.activeIndex++;
            if (this.activeIndex >= this.valueDict.Count()) this.activeIndex = 0;

            this.SetNewValueToTargetObject();

            if (Sound.menuOn) this.soundSelect.Play();
        }

        public override void PreviousValue(bool touchMode)
        {
            base.PreviousValue(touchMode: touchMode);
            this.menu.ChangeActiveItem(this);
            this.activeIndex--;
            if (this.activeIndex < 0) this.activeIndex = this.valueDict.Count() - 1;

            this.SetNewValueToTargetObject();

            if (Sound.menuOn) this.soundSelect.Play();
        }

        public override void Invoke()
        {
            if (!this.captureInput) return;
            if (Sound.menuOn) this.menu.soundInvoke.Play();
            this.menu.ChangeActiveItem(this);
            this.captureModeActive = true;
            new InputGrabber(targetObj: this.targetObj, targetPropertyName: this.propertyName, grabButtons: this.captureButtons, grabKeys: this.captureKeys);
        }

        public override void Draw(bool active, string textOverride = null, List<Texture2D> imageList = null)
        {
            if (this.captureModeActive)
            {
                if (InputGrabber.GrabberIsActive)
                {
                    string buttonOrKey = this.captureKeys ? "KEY" : "BUTTON";
                    base.Draw(active: active, textOverride: $"{this.name}   < PRESS {buttonOrKey} >");
                    return;
                }
                else
                {
                    if (Sound.menuOn) this.menu.soundInvoke.Play();
                    this.menu.Rebuild(instantScroll: false);
                    return;
                }
            }

            if (active) this.UpdateHintWindow();

            if (this.ActiveNameIsTexture)
            {
                base.Draw(active: active, textOverride: $"{this.name}   <  |  >", imageList: new List<Texture2D> { (Texture2D)this.ActiveName });
                return;
            }
            else
            {
                base.Draw(active: active, textOverride: textOverride);
                return;
            }
        }

        public override void ProcessTouch()
        {
            Rectangle rect = this.Rect;
            rect.X += (int)this.menu.viewParams.PosX;
            rect.Y += (int)this.menu.viewParams.PosY;

            Rectangle leftRect, middleRect, rightRect;

            if (this.captureInput)
            {
                int width = rect.Width / 3;
                int xPos = 0;

                leftRect = new Rectangle(x: rect.Left + xPos, y: rect.Top, width: width, height: rect.Height);
                xPos += width;
                middleRect = new Rectangle(x: rect.Left + xPos, y: rect.Top, width: width, height: rect.Height);
                xPos += width;
                rightRect = new Rectangle(x: rect.Left + xPos, y: rect.Top, width: width, height: rect.Height);
            }
            else
            {
                leftRect = new Rectangle(x: rect.Left, y: rect.Top, width: rect.Width / 2, height: rect.Height);
                middleRect = new Rectangle(0, 0, 0, 0);
                rightRect = new Rectangle(x: rect.Left + rect.Width / 2, y: rect.Top, width: rect.Width / 2, height: rect.Height);
            }

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 position = touch.Position / Preferences.GlobalScale;

                if (leftRect.Contains(position) && touch.State == TouchLocationState.Released && this.menu.CanInterpretTouchReleaseAsButtonPress)
                {
                    this.PreviousValue(touchMode: true);
                    return;
                }

                if (middleRect.Contains(position) && touch.State == TouchLocationState.Released && this.menu.CanInterpretTouchReleaseAsButtonPress)
                {
                    this.Invoke();
                    return;
                }

                if (rightRect.Contains(position) && touch.State == TouchLocationState.Released && this.menu.CanInterpretTouchReleaseAsButtonPress)
                {
                    this.NextValue(touchMode: true);
                    return;
                }
            }
        }
    }
}