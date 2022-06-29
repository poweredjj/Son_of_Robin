using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin

{
    public class Menu : Scene
    {
        public static bool nextMenuNoStartTransition = false;

        public enum Layout { Middle, Left, Right }

        public readonly MenuTemplate.Name templateName;
        private readonly string name;
        public readonly bool canBeClosedManually;
        private Scheduler.ActionName closingAction;
        private Object closingActionHelper;
        public List<Entry> entryList;
        public int activeIndex;
        private float currentScrollPosition;
        private bool touchMode;
        public readonly Layout layout;
        public Color bgColor;

        public int EntryBgWidth
        {
            get
            {
                switch (this.layout)
                {
                    case Layout.Middle:
                        return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.65f);

                    case Layout.Left:
                        return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.5f);

                    case Layout.Right:
                        return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.5f);

                    default:
                        throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
                }
            }
        }

        public Color EntriesTextColor
        {
            set
            {
                foreach (Entry entry in this.entryList)
                {
                    if (entry.GetType() != typeof(Separator))
                    { entry.textColor = value; }
                    else
                    { entry.rectColor = value; }
                }
            }
        }

        public Color EntriesRectColor
        {
            set
            {
                foreach (Entry entry in this.entryList)
                {
                    if (entry.GetType() != typeof(Separator))
                    { entry.rectColor = value; }
                    else
                    { entry.textColor = value; }
                }
            }
        }

        public Color EntriesOutlineColor
        {
            set
            {
                foreach (Entry entry in this.entryList)
                { entry.outlineColor = value; }
            }
        }

        public bool ScrollActive { get { return this.FullyVisibleEntries.Count < this.entryList.Count || this.PartiallyVisibleEntries.Count > 0; } }
        private float ScrollbarVisiblePercent { get { return (float)this.viewParams.height / (((float)this.EntryHeight + (float)this.EntryMargin) * (float)this.entryList.Count); } }
        private int ScrollbarWidgetHeight { get { return (int)(this.ScrollbarVisiblePercent * this.viewParams.height); } }
        public int ScrollbarPosX { get { return this.EntryBgWidth - this.ScrollbarWidth; } }
        public int ScrollbarWidth { get { return this.ScrollActive ? Convert.ToInt32(EntryBgWidth * 0.08f) : 0; } }
        public float ScrollbarMultiplier { get { return (float)this.viewParams.height / ((float)this.MaxScrollPos + (float)this.viewParams.height); } }
        private Rectangle ScrollWholeRect { get { return new Rectangle(this.ScrollbarPosX, 0, this.ScrollbarWidth, this.viewParams.height); } }
        public int EntryWidth { get { return Convert.ToInt32(EntryBgWidth * 0.8f); } }
        public int EntryHeight { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.08f * Preferences.menuScale); } }
        public int EntryMargin { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.025f * Preferences.menuScale); } }
        public Entry ActiveEntry { get { return this.entryList[activeIndex]; } }
        public List<Entry> VisibleEntries { get { return this.entryList.Where(entry => entry.IsVisible).ToList(); } }
        public List<Entry> PartiallyVisibleEntries { get { return this.entryList.Where(entry => entry.IsPartiallyVisible).ToList(); } }
        public List<Entry> FullyVisibleEntries { get { return this.entryList.Where(entry => entry.IsFullyVisible).ToList(); } }

        private int MaxScrollPos
        {
            get
            {
                int lastItemPos = (int)entryList[entryList.Count - 1].Position.Y;
                return lastItemPos + this.EntryHeight + this.EntryMargin - SonOfRobinGame.VirtualHeight;
            }
        }

        private int TargetScrollPosition
        {
            get
            {
                int targetScrollPos = (int)this.ActiveEntry.Position.Y + (this.EntryHeight / 2) - (SonOfRobinGame.VirtualHeight / 2); // active item should be centered - if possible
                return KeepScrollInBounds(targetScrollPos);
            }
        }
        public int KeepScrollInBounds(int scrollPos)
        {
            int minPos = 0;
            int maxPos = this.MaxScrollPos;
            int limitedScrollPos = Math.Max(Math.Min(scrollPos, maxPos), minPos);
            return limitedScrollPos;
        }
        public int CurrentScrollPosition
        {
            get { return (int)currentScrollPosition; }
            set
            {
                if (this.touchMode) return;

                int targetPos = TargetScrollPosition;
                float posDiff = targetPos - currentScrollPosition;
                currentScrollPosition += posDiff / 20f;
            }
        }
        public Menu(MenuTemplate.Name templateName, bool blocksUpdatesBelow, bool canBeClosedManually, string name, Layout layout = Layout.Right, Scheduler.ActionName closingAction = Scheduler.ActionName.Empty, Object closingActionHelper = null) : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, hidesSameScenesBelow: true, touchLayout: TouchLayout.Empty, tipsLayout: canBeClosedManually ? ControlTips.TipsLayout.Menu : ControlTips.TipsLayout.MenuWithoutClosing)
        {
            this.layout = layout;
            this.touchMode = SonOfRobinGame.platform == Platform.Mobile;
            this.templateName = templateName;
            this.name = name;
            this.activeIndex = -1; // dummy value, to be changed
            this.currentScrollPosition = 0;
            this.entryList = new List<Entry> { };
            this.canBeClosedManually = canBeClosedManually;
            this.closingAction = closingAction;
            this.closingActionHelper = closingActionHelper;
            this.SetViewPosAndSize();
            this.bgColor = Color.Black * 0.6f;

            new Separator(menu: this, name: this.name);
            this.SetTouchLayout();
            this.AddStartTransitions();
        }

        private void SetTouchLayout()
        {
            if (!this.canBeClosedManually)
            {
                this.touchLayout = TouchLayout.Empty;
                return;
            }

            switch (this.layout)
            {
                case Layout.Middle:
                    this.touchLayout = TouchLayout.MenuMiddleReturn;
                    return;

                case Layout.Left:
                    this.touchLayout = TouchLayout.MenuLeftReturn;
                    return;

                case Layout.Right:
                    this.touchLayout = TouchLayout.MenuRightReturn;
                    return;

                default:
                    throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
            }
        }

        public override void Remove()
        {
            if (this.transition == null)
            {
                Scene sceneBelow = this.GetSceneBelow();
                if (sceneBelow != null && sceneBelow.GetType() != typeof(Menu))
                {

                    Dictionary<string, float> paramsToChange;

                    switch (this.layout)
                    {
                        case Layout.Middle:
                            paramsToChange = new Dictionary<string, float> { { "posY", this.viewParams.posY + this.viewParams.height } };
                            break;

                        case Layout.Left:
                            paramsToChange = new Dictionary<string, float> { { "posX", this.viewParams.posX - this.viewParams.width } };
                            break;

                        case Layout.Right:
                            paramsToChange = new Dictionary<string, float> { { "posX", this.viewParams.posX + this.viewParams.width } };
                            break;

                        default:
                            throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
                    }

                    this.AddTransition(new Transition(type: Transition.TransType.Out, duration: 12, scene: this, blockInput: false, paramsToChange: paramsToChange, removeScene: true));
                    return;
                }
            }

            base.Remove();
            new Scheduler.Task(menu: this, actionName: this.closingAction, executeHelper: this.closingActionHelper);
        }

        public void AddClosingAction(Scheduler.ActionName closingAction, Object closingActionHelper)
        {
            this.closingAction = closingAction;
            this.closingActionHelper = closingActionHelper;
        }

        public void Rebuild()
        {
            nextMenuNoStartTransition = true;
            Menu rebuiltMenu = MenuTemplate.CreateMenuFromTemplate(templateName: this.templateName);

            Entry activeEntry = this.ActiveEntry;
            for (int i = 0; i < rebuiltMenu.entryList.Count; i++)
            {
                Entry entry = rebuiltMenu.entryList[i];
                if (entry.index == activeEntry.index)
                {
                    rebuiltMenu.activeIndex = i;
                    break;
                }
            }

            rebuiltMenu.currentScrollPosition = this.currentScrollPosition;

            sceneStack.Remove(rebuiltMenu);

            for (int i = sceneStack.Count - 1; i >= 0; i--)
            {
                Scene scene = sceneStack[i];
                if (scene == this) sceneStack[i] = rebuiltMenu;
            }
        }

        private void AddStartTransitions()
        {
            if (nextMenuNoStartTransition)
            {
                nextMenuNoStartTransition = false;
                return;
            }

            Scene sceneBelow = this.GetSceneBelow();

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"sceneBelow {sceneBelow}", color: Color.White);

            if (sceneBelow != null && sceneBelow.GetType() != typeof(Menu))
            {
                Dictionary<string, float> paramsToChange;

                switch (this.layout)
                {
                    case Layout.Middle:
                        paramsToChange = new Dictionary<string, float> { { "posY", this.viewParams.posY + this.viewParams.height } };
                        break;

                    case Layout.Left:
                        paramsToChange = new Dictionary<string, float> { { "posX", this.viewParams.posX - this.viewParams.width } };
                        break;

                    case Layout.Right:
                        paramsToChange = new Dictionary<string, float> { { "posX", this.viewParams.posX + this.viewParams.width } };
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
                }

                this.AddTransition(new Transition(type: Transition.TransType.In, duration: 12, scene: this, blockInput: false, paramsToChange: paramsToChange));

                World topWorld = World.GetTopWorld();
                if (topWorld != null && !topWorld.demoMode)
                {
                    topWorld.UpdateViewParams();
                    float zoomScale = 0.7f;

                    topWorld.AddTransition(new Transition(type: Transition.TransType.PingPong, duration: 20, scene: topWorld, blockInput: false, paramsToChange: new Dictionary<string, float> {
                    { "posX", topWorld.viewParams.posX - (topWorld.camera.ScreenWidth * zoomScale * 0.25f) },
                    { "posY", topWorld.viewParams.posY - (topWorld.camera.ScreenHeight * zoomScale * 0.25f) },
                    { "scaleX", topWorld.viewParams.scaleX * zoomScale },
                    { "scaleY", topWorld.viewParams.scaleY * zoomScale } },

                    pingPongPause: true));
                }
            }
        }

        public static void RemoveEveryMenuOfTemplate(MenuTemplate.Name templateName)
        {
            foreach (Menu menu in GetEveryMenuOfTemplate(templateName: templateName))
            {
                menu.Remove();
                MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Menu '{menu.templateName}' removed.", color: Color.White);
            }
        }
        public static List<Menu> GetEveryMenuOfTemplate(MenuTemplate.Name templateName)
        {
            var menuListOfTemplate = new List<Menu> { };

            foreach (Scene scene in sceneStack)
            { if (scene.GetType() == typeof(Menu)) menuListOfTemplate.Add((Menu)scene); }

            return menuListOfTemplate.Where(menu => menu.templateName == templateName).ToList();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.activeIndex == -1) this.NextItem(); // searching for first non-separator menu item
            this.CurrentScrollPosition = this.TargetScrollPosition;
            this.SetViewPosAndSize();

            this.ProcessInput();
        }

        private void ProcessInput()
        {
            if (
                Keyboard.HasBeenPressed(Keys.Escape) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Start) ||
                VirtButton.HasButtonBeenPressed(VButName.Return) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B)
                )
            {
                if (this.canBeClosedManually)
                {
                    this.Remove();
                    return;
                }
            }

            if (SonOfRobinGame.platform == Platform.Mobile)
            {
                if (this.ScrollActive) this.ScrollByTouch();

                var visibleEntries = this.VisibleEntries;
                foreach (Entry entry in visibleEntries)
                { entry.ProcessTouch(); }
            }

            if (Keyboard.HasBeenPressed(Keys.W) || Keyboard.HasBeenPressed(Keys.Up) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadUp, analogAsDigital: true)) this.PreviousItem();
            if (Keyboard.HasBeenPressed(Keys.S) || Keyboard.HasBeenPressed(Keys.Down) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadDown, analogAsDigital: true)) this.NextItem();
            if (Keyboard.HasBeenPressed(Keys.A) || Keyboard.HasBeenPressed(Keys.Left) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadLeft, analogAsDigital: true)) this.ActiveEntry.PreviousValue();
            if (Keyboard.HasBeenPressed(Keys.D) || Keyboard.HasBeenPressed(Keys.Right) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadRight, analogAsDigital: true)) this.ActiveEntry.NextValue();

            if (Keyboard.HasBeenPressed(Keys.Enter) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.A)) this.ActiveEntry.Invoke();
        }

        private void ScrollByTouch()
        {
            Rectangle scrollWholeRect = this.ScrollWholeRect;
            scrollWholeRect.X += (int)this.viewParams.posX;
            scrollWholeRect.Y += (int)this.viewParams.posY;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = touch.Position / Preferences.globalScale;

                if (scrollWholeRect.Contains(touchPos))
                {
                    touchMode = true;
                    this.currentScrollPosition = ((touchPos.Y - (this.ScrollbarWidgetHeight / 2)) / this.ScrollbarMultiplier);
                    this.currentScrollPosition = KeepScrollInBounds(Convert.ToInt32(this.currentScrollPosition));
                    return;
                }
            }
        }

        private void NextItem()
        { this.ChangeActiveItem(add: true); }

        private void PreviousItem()
        { this.ChangeActiveItem(add: false); }

        private void ChangeActiveItem(bool add)
        {
            this.touchMode = false;

            int valueToAdd = add ? 1 : -1;

            int oldIndex = this.activeIndex;
            while (true)
            {
                this.activeIndex += valueToAdd;
                if (this.activeIndex == this.entryList.Count || this.activeIndex == -1)
                {
                    this.activeIndex = oldIndex;
                    return;
                }

                Entry activeItem = this.ActiveEntry;
                if (activeItem.GetType() != typeof(Separator)) return;
            }
        }

        public override void Draw()
        {
            Rectangle rect = new Rectangle(0, 0, this.EntryBgWidth, SonOfRobinGame.VirtualHeight);

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, rect, this.bgColor);

            var visibleEntries = VisibleEntries;

            for (int i = 0; i < visibleEntries.Count; i++)
            {
                Entry menuItem = visibleEntries[i];
                bool itemActive = menuItem == this.ActiveEntry && !this.touchMode;

                menuItem.Draw(active: itemActive);
            }

            this.DrawScrollbar();
        }


        private void DrawScrollbar()
        {
            if (!this.ScrollActive) return;

            Rectangle scrollWholeRect = this.ScrollWholeRect;
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, scrollWholeRect, this.bgColor * 0.5f * this.viewParams.opacity);
            Helpers.DrawRectangleOutline(rect: scrollWholeRect, color: this.entryList[0].outlineColor * 0.5f * this.viewParams.opacity, borderWidth: 2);

            int scrollPos = (int)(this.CurrentScrollPosition * this.ScrollbarMultiplier);

            Rectangle widgetRect = new Rectangle(this.ScrollbarPosX, scrollPos, this.ScrollbarWidth, this.ScrollbarWidgetHeight);
            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, widgetRect, this.entryList[0].outlineColor * 0.75f * this.viewParams.opacity);
            Helpers.DrawRectangleOutline(rect: widgetRect, color: this.entryList[0].textColor * this.viewParams.opacity, borderWidth: 2);
        }

        private void SetViewPosAndSize()
        {
            this.viewParams.width = this.EntryBgWidth;
            this.viewParams.height = SonOfRobinGame.VirtualHeight;

            switch (this.layout)
            {
                case Menu.Layout.Middle:
                    this.viewParams.posX = (SonOfRobinGame.VirtualWidth / 2) - (this.viewParams.width / 2);
                    break;

                case Menu.Layout.Left:
                    this.viewParams.posX = 0;
                    break;

                case Menu.Layout.Right:
                    this.viewParams.posX = SonOfRobinGame.VirtualWidth - this.viewParams.width;
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
            }

            this.viewParams.posY = 0;
        }
    }
}
