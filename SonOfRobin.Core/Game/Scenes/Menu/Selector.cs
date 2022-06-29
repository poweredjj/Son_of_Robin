using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        private Object ActiveName
        { get { return valueDict.Values.ToList()[activeIndex]; } }
        private Object ActiveValue { get { return valueDict.Keys.ToList()[activeIndex]; } }
        private bool ActiveNameIsTexture { get { return this.ActiveName.GetType() == typeof(Texture2D); } }

        public override string DisplayedText { get { return $"{this.name}   < {this.ActiveName} >"; } }

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

            if (!this.ActiveNameIsTexture)
            {
                base.Draw(active);
                return;
            }

            Rectangle rect = this.Rect;

            float opacity = this.GetOpacity(active: active);
            float opacityFade = this.OpacityFade;
            if (active || opacityFade > 0) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, rect, this.rectColor * opacityFade * 2);

            string textLeft = $"{this.name}   < ";
            string textRight = " >";

            float maxWidth = rect.Width * 0.85f;
            float maxTextHeight = rect.Height * 0.6f;
            float maxTextureHeight = rect.Height * 0.9f;

            Texture2D texture = (Texture2D)this.ActiveName;
            float textureScale = maxTextureHeight / (float)texture.Height;

            Vector2 textLeftSize = font.MeasureString(textLeft);
            Vector2 textRightSize = font.MeasureString(textRight);
            float textScale = Math.Min(maxWidth / (textLeftSize.X + textRightSize.X), maxTextHeight / Math.Max(textLeftSize.Y, textRightSize.Y));

            float textureWidthAfterScale, fullWidthAfterScale;

            while (true)
            {
                textureWidthAfterScale = (float)texture.Width * textureScale;
                fullWidthAfterScale = ((textLeftSize.X + textRightSize.X) * textScale) + textureWidthAfterScale;

                if (fullWidthAfterScale <= maxWidth) break;
                else
                {
                    textureScale *= maxWidth / fullWidthAfterScale;
                    textScale *= maxWidth / fullWidthAfterScale;
                }
            }

            float textureHeightAfterScale = (float)texture.Height * textureScale;
            float textHeightAfterScale = Math.Max(textLeftSize.Y * textScale, textRightSize.Y * textScale);


            Vector2 leftTextPos = new Vector2(
                rect.Center.X - (fullWidthAfterScale / 2),
                rect.Center.Y - (textHeightAfterScale / 2));

            SonOfRobinGame.spriteBatch.DrawString(font, textLeft, position: leftTextPos, color: this.textColor * opacity * menu.viewParams.Opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            Vector2 rightTextPos = new Vector2(
                rect.Center.X - (fullWidthAfterScale / 2) + (textLeftSize.X * textScale) + textureWidthAfterScale,
                rect.Center.Y - (textHeightAfterScale / 2));

            SonOfRobinGame.spriteBatch.DrawString(font, textRight, position: rightTextPos, color: this.textColor * opacity * menu.viewParams.Opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

            Rectangle textureRect = new Rectangle(
                x: (int)(rect.Center.X - (fullWidthAfterScale / 2) + (textLeftSize.X * textScale)),
                y: (int)(rect.Center.Y - (textureHeightAfterScale / 2)),
                width: (int)textureWidthAfterScale,
                height: (int)textureHeightAfterScale);

            Helpers.DrawTextureInsideRect(texture: texture, rectangle: textureRect, color: Color.White * opacity, drawTestRect: false);

            Helpers.DrawRectangleOutline(rect: this.Rect, color: this.outlineColor, borderWidth: 2);
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
