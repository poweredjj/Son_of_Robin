using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public abstract class Entry
    {
        public static readonly SpriteFont font = SonOfRobinGame.fontHuge;

        public readonly Menu menu;
        public readonly int index;
        public readonly string name;
        public Color textColor;
        public Color rectColor;
        public Color outlineColor;
        protected int lastFlashFrame = 0;
        protected readonly bool rebuildsMenu;
        public List<InfoWindow.TextEntry> infoTextList;
        public virtual string DisplayedText { get { return this.name; } }

        public Vector2 Position
        {
            get
            {
                return new Vector2((menu.EntryBgWidth - menu.EntryWidth) / 2,
                                  ((menu.EntryHeight + menu.EntryMargin) * this.index) + menu.EntryMargin);
            }
        }

        public bool IsFullyVisible
        {
            get
            {
                Rectangle screenRect = new Rectangle(0, 0, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight);
                return screenRect.Contains(this.Rect);
            }
        }

        public bool IsPartiallyVisible
        {
            get
            {
                Rectangle screenRect = new Rectangle(0, 0, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight);
                return !screenRect.Contains(this.Rect) && screenRect.Intersects(this.Rect);
            }
        }

        public bool IsVisible
        {
            get
            {
                Rectangle screenRect = new Rectangle(0, 0, SonOfRobinGame.VirtualWidth, SonOfRobinGame.VirtualHeight);
                return screenRect.Intersects(this.Rect);
            }
        }

        public Rectangle Rect
        {
            get
            {
                var position = this.Position;
                return new Rectangle(x: (int)position.X, y: (int)position.Y - this.menu.CurrentScrollPosition, width: menu.EntryWidth, height: menu.EntryHeight);
            }
        }
        protected float OpacityFade { get { return Math.Max((float)this.lastFlashFrame - (float)SonOfRobinGame.currentUpdate, 0) * 0.015f; } }

        public Entry(Menu menu, string name, bool rebuildsMenu = false, List<InfoWindow.TextEntry> infoTextList = null)
        {
            this.menu = menu;
            this.name = name;
            this.index = this.menu.entryList.Count;
            this.rebuildsMenu = rebuildsMenu;
            this.textColor = Color.White;
            this.rectColor = Color.Black;
            this.outlineColor = Color.White;
            this.infoTextList = infoTextList;

            this.menu.entryList.Add(this);

            if (this.menu.activeIndex == -1 && this.GetType() != typeof(Separator)) this.menu.activeIndex = this.index;
        }

        public virtual void NextValue()
        {
            if (Preferences.DebugMode) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "NextValue", color: Color.White);
        }

        public virtual void PreviousValue()
        {
            if (Preferences.DebugMode) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "PreviousValue", color: Color.White);
        }

        public virtual void Invoke()
        {
            if (Preferences.DebugMode) MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: "Invoke", color: Color.White);
        }

        public virtual void ProcessTouch()
        { }

        protected float GetOpacity(bool active)
        {
            if (active) this.lastFlashFrame = SonOfRobinGame.currentUpdate + 20;
            float opacity = active ? 1 : 0.6f;
            float opacityFade = Math.Max((float)this.lastFlashFrame - (float)SonOfRobinGame.currentUpdate, 0) * 0.015f;
            opacity = Math.Min(opacity + opacityFade, 1);

            return opacity;
        }

        public virtual void Draw(bool active)
        {
            float opacity = this.GetOpacity(active: active);
            float opacityFade = this.OpacityFade;
            Rectangle rect = this.Rect;

            if (active || opacityFade > 0) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, rect, this.rectColor * opacityFade * 2);

            Helpers.DrawRectangleOutline(rect: this.Rect, color: this.outlineColor, borderWidth: 2);

            Vector2 textSize = font.MeasureString(this.DisplayedText);

            float maxTextHeight = rect.Height * 0.5f;
            float maxTextWidth = rect.Width * 0.85f;

            float textScale = Math.Min(maxTextWidth / textSize.X, maxTextHeight / textSize.Y);

            Vector2 textPos = new Vector2(
                rect.Center.X - (textSize.X / 2 * textScale),
                rect.Center.Y - (textSize.Y / 2 * textScale));

            SonOfRobinGame.spriteBatch.DrawString(font, this.DisplayedText, position: textPos, color: this.textColor * opacity * menu.viewParams.Opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        protected void UpdateHintWindow()
        {
            if (this.infoTextList == null || !this.menu.inputActive)
            {
                SonOfRobinGame.hintWindow.TurnOff();
                return;
            }

            InfoWindow hintWindow = SonOfRobinGame.hintWindow;

            Vector2 windowPos;
            Vector2 infoWindowSize = hintWindow.MeasureEntries(this.infoTextList);

            Vector2 entryPos = this.Position;
            ViewParams menuViewParams = this.menu.viewParams;

            switch (this.menu.layout)
            {
                case Menu.Layout.Middle:
                    windowPos = new Vector2(
                       menuViewParams.PosX + menuViewParams.Width - this.menu.ScrollbarWidth,
                       menuViewParams.PosY + entryPos.Y - this.menu.CurrentScrollPosition + (this.menu.EntryHeight / 2) - (infoWindowSize.Y / 2));
                    break;

                case Menu.Layout.Left:
                    windowPos = new Vector2(
                       menuViewParams.PosX + menuViewParams.Width - this.menu.ScrollbarWidth,
                       menuViewParams.PosY + entryPos.Y - this.menu.CurrentScrollPosition + (this.menu.EntryHeight / 2) - (infoWindowSize.Y / 2));
                    break;

                case Menu.Layout.Right:
                    windowPos = new Vector2(
                       menuViewParams.PosX - infoWindowSize.X + this.menu.ScrollbarWidth,
                       menuViewParams.PosY + entryPos.Y - this.menu.CurrentScrollPosition + (this.menu.EntryHeight / 2) - (infoWindowSize.Y / 2));
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported layout - {this.menu.layout}.");
            }

            // keeping the window inside screen bounds
            windowPos.X = Math.Max(windowPos.X, 0);
            windowPos.Y = Math.Max(windowPos.Y, 0);
            int maxX = (int)((SonOfRobinGame.VirtualWidth * hintWindow.viewParams.ScaleX) - infoWindowSize.X);
            int maxY = (int)((SonOfRobinGame.VirtualHeight * hintWindow.viewParams.ScaleY) - infoWindowSize.Y);
            windowPos.X = Math.Min(windowPos.X, maxX);
            windowPos.Y = Math.Min(windowPos.Y, maxY);

            SonOfRobinGame.hintWindow.TurnOn(newPosX: (int)windowPos.X, newPosY: (int)windowPos.Y, entryList: this.infoTextList, addTransition: true);
        }

    }
}
