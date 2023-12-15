using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public abstract class Entry
    {
        public readonly SpriteFontBase font;
        public readonly Menu menu;
        public readonly int index;
        public readonly string name;
        public readonly List<Texture2D> imageList;
        protected TextWithImages textWithImages;
        public Color textColor;
        public Color shadowColor;
        public Color bgColor;
        public TriSliceBG triSliceBG;
        protected int lastFlashFrame = 0;
        protected readonly bool rebuildsMenu;
        protected readonly bool rebuildsMenuInstantScroll;
        protected readonly bool rebuildsAllMenus;
        public Scheduler.ExecutionDelegate infoTextListDlgt; // for on-demand data retrieval
        public List<InfoWindow.TextEntry> infoTextList;
        private readonly float infoWindowMaxLineHeightPercentOverride;

        public Entry(Menu menu, string name, bool rebuildsMenu = false, bool rebuildsMenuInstantScroll = false, bool rebuildsAllMenus = false, List<InfoWindow.TextEntry> infoTextList = null, float infoWindowMaxLineHeightPercentOverride = 0f, List<Texture2D> imageList = null)
        {
            this.font = SonOfRobinGame.FontTommy.GetFont(60);
            this.menu = menu;
            this.name = name;
            this.imageList = imageList;
            this.textWithImages = null;
            this.index = this.menu.entryList.Count;
            this.rebuildsMenu = rebuildsMenu;
            this.rebuildsMenuInstantScroll = rebuildsMenuInstantScroll;
            this.rebuildsAllMenus = rebuildsAllMenus;
            this.textColor = Color.White;
            this.shadowColor = Color.Black * 0.8f;
            this.bgColor = Color.White;
            this.triSliceBG = TriSliceBG.GetBGForPreset(TriSliceBG.Preset.MenuGray);
            this.infoTextList = infoTextList;
            this.infoWindowMaxLineHeightPercentOverride = infoWindowMaxLineHeightPercentOverride;

            this.menu.entryList.Add(this);

            if (this.menu.activeIndex == -1 && this.GetType() != typeof(Separator)) this.menu.activeIndex = this.index;
        }

        public virtual string DisplayedText
        { get { return this.name; } }

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
                Rectangle screenRect = new Rectangle(0, 0, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight);
                return screenRect.Contains(this.Rect);
            }
        }

        public bool IsPartiallyVisible
        {
            get
            {
                Rectangle screenRect = new Rectangle(0, 0, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight);
                return !screenRect.Contains(this.Rect) && screenRect.Intersects(this.Rect);
            }
        }

        public bool IsVisible
        {
            get
            {
                Rectangle screenRect = new Rectangle(0, 0, SonOfRobinGame.ScreenWidth, SonOfRobinGame.ScreenHeight);
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

        protected float OpacityFade
        { get { return Math.Max((float)this.lastFlashFrame - (float)SonOfRobinGame.CurrentUpdate, 0) * 0.015f; } }

        public virtual void NextValue(bool touchMode)
        {
            this.menu.touchMode = touchMode;
            // if (Preferences.DebugMode) SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: "NextValue", color: Color.White);
        }

        public virtual void PreviousValue(bool touchMode)
        {
            this.menu.touchMode = touchMode;
            // if (Preferences.DebugMode) SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: "PreviousValue", color: Color.White);
        }

        public virtual void Invoke()
        {
            // if (Preferences.DebugMode) SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: "Invoke", color: Color.White);
        }

        public virtual void ProcessTouch()
        { }

        protected float GetOpacity(bool active)
        {
            if (this.menu.touchMode) return 1f;

            if (active)
            {
                this.lastFlashFrame = SonOfRobinGame.CurrentUpdate + 20;
                return 1f;
            }

            return Math.Min(0.6f + this.OpacityFade, 1);
        }

        public virtual void Draw(bool active, string textOverride = null, List<Texture2D> imageList = null)
        {
            if (this.imageList != null) imageList = this.imageList;

            float opacity = this.GetOpacity(active: active);
            Rectangle rect = this.Rect;

            this.triSliceBG.Draw(triSliceRect: rect, color: this.bgColor * opacity * this.menu.viewParams.Opacity, keepExactRectSize: true);

            string text = textOverride != null ? textOverride : this.DisplayedText;

            if (this.textWithImages == null || this.textWithImages.TextOriginal != text || !this.textWithImages.ImageListEqual(imageList))
            {
                // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"{SonOfRobinGame.currentUpdate} new menu textWithImages '{text}'");
                this.textWithImages = new TextWithImages(font: font, text: text, imageList: imageList);
            }

            this.textWithImages.Update();

            float maxTextHeight = rect.Height * 0.6f;
            float maxTextWidth = rect.Width * 0.85f;

            float textScale = Math.Min(maxTextWidth / this.textWithImages.textWidth, maxTextHeight / this.textWithImages.textHeight);

            Vector2 textPos = new Vector2(
                rect.Center.X - (this.textWithImages.textWidth / 2 * textScale),
                rect.Center.Y - (this.textWithImages.textHeight / 2 * textScale));

            Color textColorCalculated = new Color( // to avoid transparency (which would ruin shadows)
                r: (byte)((float)this.textColor.R * opacity),
                g: (byte)((float)this.textColor.G * opacity),
                b: (byte)((float)this.textColor.B * opacity));

            this.textWithImages.Draw(position: textPos, color: textColorCalculated * menu.viewParams.Opacity, textScale: textScale, inflatePercent: this.imageList == null ? 0.3f : 0f, drawShadow: true, shadowColor: this.shadowColor);
        }

        protected void UpdateHintWindow()
        {
            if (this.infoTextList == null && this.infoTextListDlgt != null)
            {
                new Scheduler.Task(taskName: Scheduler.TaskName.ExecuteDelegate, executeHelper: this.infoTextListDlgt, delay: 0);
                this.infoTextListDlgt = null;
            }

            if (this.infoTextList == null || !this.menu.inputActive)
            {
                SonOfRobinGame.HintWindow.TurnOff();
                return;
            }

            InfoWindow hintWindow = SonOfRobinGame.HintWindow;

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
                    throw new ArgumentException($"Unsupported layout - {this.menu.layout}.");
            }

            // keeping the window inside screen bounds
            windowPos.X = Math.Max(windowPos.X, 0);
            windowPos.Y = Math.Max(windowPos.Y, 0);
            int maxX = (int)((SonOfRobinGame.ScreenWidth * hintWindow.viewParams.ScaleX) - infoWindowSize.X);
            int maxY = (int)((SonOfRobinGame.ScreenHeight * hintWindow.viewParams.ScaleY) - infoWindowSize.Y);
            windowPos.X = Math.Min(windowPos.X, maxX);
            windowPos.Y = Math.Min(windowPos.Y, maxY);

            SonOfRobinGame.HintWindow.TurnOn(newPosX: (int)windowPos.X, newPosY: (int)windowPos.Y, entryList: this.infoTextList, maxLineHeightPercentOverride: this.infoWindowMaxLineHeightPercentOverride, addTransition: true);
        }
    }
}