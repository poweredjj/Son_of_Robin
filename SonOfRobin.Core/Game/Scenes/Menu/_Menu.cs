using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin

{
    public class Menu : Scene
    {
        public static bool nextMenuNoStartTransition = false;
        private static readonly float baseScrollSpeed = 20f;
        public enum Layout { Middle, Left, Right }

        public readonly MenuTemplate.Name templateName;
        private readonly string name;
        public readonly bool canBeClosedManually;
        private Scheduler.TaskName closingTask;
        private Object closingTaskHelper;
        public List<Entry> entryList;
        public int activeIndex;
        private float currentScrollPosition;
        public bool touchMode;
        private readonly bool alwaysShowSelectedEntry;
        public readonly Layout layout;
        public Color bgColor;
        public Entry lastTouchedEntry;
        private float scrollSpeed;
        public bool instantScrollForOneFrame;
        public readonly Sound soundNavigate;
        public readonly Sound soundClose;
        public readonly Sound soundSelect;
        public readonly Sound soundInvoke;
        public readonly object templateExecuteHelper; // needed for correct rebuild

        public int EntryBgWidth
        {
            get
            {
                switch (this.layout)
                {
                    case Layout.Middle:
                        return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.7f);

                    case Layout.Left:
                        return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.65f);

                    case Layout.Right:
                        return Convert.ToInt32(SonOfRobinGame.VirtualWidth * 0.65f);

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
                    if (entry.GetType() != typeof(Separator)) entry.textColor = value;
                    else entry.rectColor = value;
                }
            }
        }

        public Color EntriesRectColor
        {
            set
            {
                foreach (Entry entry in this.entryList)
                {
                    if (entry.GetType() != typeof(Separator)) entry.rectColor = value;
                    else entry.textColor = value;
                }
            }
        }

        public Color EntriesOutlineColor
        {
            set
            {
                foreach (Entry entry in this.entryList)
                {
                    entry.outlineColor = value;
                }
            }
        }

        public static bool CanInterpretTouchReleaseAsButtonPress
        {
            get
            {
                return TouchInput.IsPressedReleasedWithinDistanceAndDuration(maxDistance: (int)(SonOfRobinGame.VirtualHeight * 0.03), maxDuration: TimeSpan.FromMilliseconds(400));
            }
        }

        public bool ScrollActive { get { return this.FullyVisibleEntries.Count < this.entryList.Count || this.PartiallyVisibleEntries.Count > 0; } }
        private float ScrollbarVisiblePercent { get { return (float)this.viewParams.Height / (((float)this.EntryHeight + (float)this.EntryMargin) * (float)this.entryList.Count); } }
        private int ScrollbarWidgetHeight { get { return (int)(this.ScrollbarVisiblePercent * this.viewParams.Height); } }
        public int ScrollbarPosX { get { return this.EntryBgWidth - this.ScrollbarWidth; } }
        public int ScrollbarWidth { get { return this.ScrollActive ? Convert.ToInt32(EntryBgWidth * 0.08f) : 0; } }
        public float ScrollbarMultiplier { get { return (float)this.viewParams.Height / ((float)this.MaxScrollPos + (float)this.viewParams.Height); } }
        private Rectangle ScrollWholeRect { get { return new Rectangle(this.ScrollbarPosX, 0, this.ScrollbarWidth, this.viewParams.Height); } }
        public int EntryWidth { get { return Convert.ToInt32(EntryBgWidth * 0.8f); } }
        public int EntryHeight { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.08f * Preferences.menuScale); } }
        public int EntryMargin { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * 0.025f * Preferences.menuScale); } }
        private Rectangle BgRect { get { return new Rectangle(0, 0, this.EntryBgWidth, SonOfRobinGame.VirtualHeight); } }
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
            get
            {
                if (this.instantScrollForOneFrame)
                {
                    currentScrollPosition = this.TargetScrollPosition;
                    this.instantScrollForOneFrame = false;
                }

                return (int)currentScrollPosition;
            }
            set
            {
                if (this.touchMode) return;

                int targetPos = TargetScrollPosition;
                float posDiff = targetPos - currentScrollPosition;
                currentScrollPosition += posDiff / this.scrollSpeed;
            }
        }
        public Menu(MenuTemplate.Name templateName, bool blocksUpdatesBelow, bool canBeClosedManually, string name, object templateExecuteHelper, bool alwaysShowSelectedEntry = false, Layout layout = Layout.Right, Scheduler.TaskName closingTask = Scheduler.TaskName.Empty, Object closingTaskHelper = null, int priority = 1, SoundData.Name soundNavigate = SoundData.Name.Navigation, SoundData.Name soundOpen = SoundData.Name.Empty, SoundData.Name soundClose = SoundData.Name.Navigation, SoundData.Name soundSelect = SoundData.Name.Select, SoundData.Name soundInvoke = SoundData.Name.Invoke) : base(inputType: InputTypes.Normal, priority: priority, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, hidesSameScenesBelow: true, touchLayout: TouchLayout.Empty, tipsLayout: canBeClosedManually ? ControlTips.TipsLayout.Menu : ControlTips.TipsLayout.MenuWithoutClosing)
        {
            this.layout = layout;
            this.alwaysShowSelectedEntry = alwaysShowSelectedEntry;
            this.touchMode = Input.currentControlType == Input.ControlType.Touch;
            this.lastTouchedEntry = null;
            this.templateName = templateName;
            this.name = name;
            this.activeIndex = -1; // dummy value, to be changed
            this.currentScrollPosition = 0;
            this.entryList = new List<Entry> { };
            this.canBeClosedManually = canBeClosedManually;
            this.closingTask = closingTask;
            this.closingTaskHelper = closingTaskHelper;
            this.SetViewPosAndSize();
            this.bgColor = Color.Black * 0.6f;
            this.scrollSpeed = baseScrollSpeed;
            this.instantScrollForOneFrame = false;
            this.templateExecuteHelper = templateExecuteHelper;

            if (Sound.menuOn && soundOpen != SoundData.Name.Empty && !this.ThisMenuIsBeingRebuilt) Sound.QuickPlay(soundOpen);

            this.soundClose = new Sound(soundClose);
            this.soundNavigate = new Sound(soundNavigate);
            this.soundSelect = new Sound(soundSelect);
            this.soundInvoke = new Sound(soundInvoke);

            new Separator(menu: this, name: this.name);
            this.SetTouchLayout();
            this.AddStartTransitions();

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Menu {this.name} - created with closing task '{this.closingTask}'.");
        }

        private bool ThisMenuIsBeingRebuilt
        {
            get
            {
                List<Menu> menuScenes = GetEveryMenuOfTemplate(this.templateName);

                foreach (Menu menu in menuScenes)
                {
                    if (menu.id != this.id && menu.name == this.name) return true;
                }

                return false;
            }
        }


        protected override void AdaptToNewSize()
        {
            this.SetViewPosAndSize();
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
            if (!this.transManager.IsEnding)
            {
                Scene sceneBelow = this.GetSceneBelow();
                if (sceneBelow != null && sceneBelow.GetType() != typeof(Menu))
                {
                    Dictionary<string, float> paramsToChange;

                    switch (this.layout)
                    {
                        case Layout.Middle:
                            paramsToChange = new Dictionary<string, float> { { "PosY", this.viewParams.PosY + this.viewParams.Height } };
                            break;

                        case Layout.Left:
                            paramsToChange = new Dictionary<string, float> { { "PosX", this.viewParams.PosX - this.viewParams.Width } };
                            break;

                        case Layout.Right:
                            paramsToChange = new Dictionary<string, float> { { "PosX", this.viewParams.PosX + this.viewParams.Width } };
                            break;

                        default:
                            throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
                    }

                    this.transManager.AddMultipleTransitions(paramsToChange: paramsToChange, outTrans: true, duration: 12, endRemoveScene: true);

                    return;
                }
            }

            SonOfRobinGame.HintWindow.TurnOff();
            SonOfRobinGame.ProgressBar.TurnOff();
            base.Remove();
            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Menu {this.name} - executing closing task '{this.closingTask}'.");

            new Scheduler.Task(taskName: this.closingTask, executeHelper: this.closingTaskHelper);
        }

        public void AddClosingTask(Scheduler.TaskName closingTask, Object closingTaskHelper)
        {
            this.closingTask = closingTask;
            this.closingTaskHelper = closingTaskHelper;
            //  MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Menu {this.name} - adding closing task '{this.closingTask}'.");
        }

        public static void RebuildAllMenus()
        {
            var existingMenus = GetAllScenesOfType(typeof(Menu));
            foreach (Scene menuScene in existingMenus)
            {
                Menu menu = (Menu)menuScene;
                menu.Rebuild(instantScroll: false);
            }
        }

        public void Rebuild(bool instantScroll)
        {
            nextMenuNoStartTransition = true;
            Menu rebuiltMenu = MenuTemplate.CreateMenuFromTemplate(templateName: this.templateName, executeHelper: this.templateExecuteHelper);

            if (rebuiltMenu == null)
            {
                // if menu refuses to be rebuild, after changing game parameters (craft, for example)

                this.Remove();
                return;
            }

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
            rebuiltMenu.instantScrollForOneFrame = instantScroll;
            rebuiltMenu.AddClosingTask(closingTask: this.closingTask, closingTaskHelper: this.closingTaskHelper);

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

            //  MessageLog.AddMessage(msgType: MsgType.User, message: $"sceneBelow {sceneBelow}", color: Color.White);

            if (sceneBelow != null && sceneBelow.GetType() != typeof(Menu))
            {
                Dictionary<string, float> paramsToChange;

                switch (this.layout)
                {
                    case Layout.Middle:
                        paramsToChange = new Dictionary<string, float> { { "PosY", this.viewParams.PosY + this.viewParams.Height } };
                        break;

                    case Layout.Left:
                        paramsToChange = new Dictionary<string, float> { { "PosX", this.viewParams.PosX - this.viewParams.Width } };
                        break;

                    case Layout.Right:
                        paramsToChange = new Dictionary<string, float> { { "PosX", this.viewParams.PosX + this.viewParams.Width } };
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
                }

                this.transManager.AddMultipleTransitions(paramsToChange: paramsToChange, outTrans: false, duration: 12);

                World topWorld = World.GetTopWorld();
                if (topWorld != null && GetTopSceneOfType(typeof(TextWindow)) == null) topWorld.AddPauseMenuTransitions();
            }
        }

        public static void RemoveEveryMenuOfTemplate(MenuTemplate.Name templateName)
        {
            foreach (Menu menu in GetEveryMenuOfTemplate(templateName: templateName))
            {
                menu.Remove();
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Menu '{menu.templateName}' removed.", color: Color.White);
            }
        }

        public static void RemoveEveryMenuWithTemplateOtherThan(MenuTemplate.Name templateName)
        {
            foreach (Menu menu in GetAllScenesOfType(typeof(Menu)))
            {
                if (menu.templateName != templateName)
                {
                    menu.Remove();
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Menu '{menu.templateName}' removed.", color: Color.White);
                }
            }
        }

        public static List<Menu> GetEveryMenuOfTemplate(MenuTemplate.Name templateName)
        {
            var menuListOfTemplate = new List<Menu> { };

            foreach (Menu menu in GetAllScenesOfType(typeof(Menu)))
            {
                if (menu.templateName == templateName) menuListOfTemplate.Add(menu);
            }

            return menuListOfTemplate;
        }

        public override void Update(GameTime gameTime)
        {
            SonOfRobinGame.ProgressBar.TurnOff();

            if (this.activeIndex == -1) this.NextItem(); // searching for first non-separator menu item
            this.CurrentScrollPosition = this.TargetScrollPosition;

            var activeEntry = this.ActiveEntry;
            if (activeEntry.infoTextList != null && !activeEntry.IsFullyVisible) SonOfRobinGame.HintWindow.TurnOff();

            this.ProcessInput();
        }

        private void ProcessInput()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip) || this.CanExitByTouch())
            {
                if (this.canBeClosedManually)
                {
                    if (Sound.menuOn) this.soundClose.Play();
                    this.Remove();
                    return;
                }
            }

            if (this.ScrollActive) this.ScrollByTouch();

            var visibleEntries = this.VisibleEntries;
            foreach (Entry entry in visibleEntries)
            {
                entry.ProcessTouch();
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalUp)) this.PreviousItem();
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalDown)) this.NextItem();
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalLeft)) this.ActiveEntry.PreviousValue(touchMode: false);
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalRight)) this.ActiveEntry.NextValue(touchMode: false);
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm)) this.ActiveEntry.Invoke();
        }

        private bool CanExitByTouch()
        {
            if (Preferences.EnableTouchButtons) return false;

            var pressTouches = TouchInput.TouchPanelState.Where(touch => touch.State == TouchLocationState.Pressed).ToList();
            if (pressTouches.Count == 0) return false;

            Rectangle menuRect = this.BgRect;
            menuRect.X += (int)this.viewParams.drawPosX;
            menuRect.Y += (int)this.viewParams.drawPosY;

            foreach (TouchLocation touch in pressTouches)
            {
                if (!menuRect.Contains(touch.Position / Preferences.GlobalScale)) return true;
            }

            return false;
        }

        private void ScrollByTouch()
        {
            // scroll by scrollbar

            Rectangle scrollWholeRect = this.ScrollWholeRect;
            scrollWholeRect.X += (int)this.viewParams.PosX;
            scrollWholeRect.Y += (int)this.viewParams.PosY;

            int mouseScrollValue = (int)(SonOfRobinGame.VirtualHeight * 0.2f);

            int mouseScroll = 0;
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalScrollUp)) mouseScroll = -mouseScrollValue;
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalScrollDown)) mouseScroll = mouseScrollValue;

            if (mouseScroll != 0)
            {
                this.touchMode = true;
                this.currentScrollPosition += mouseScroll;
                this.currentScrollPosition = KeepScrollInBounds(Convert.ToInt32(this.currentScrollPosition));

                return;
            }

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = touch.Position / Preferences.GlobalScale;

                if (scrollWholeRect.Contains(touchPos))
                {
                    this.touchMode = true;
                    this.currentScrollPosition = (touchPos.Y - (this.ScrollbarWidgetHeight / 2)) / this.ScrollbarMultiplier;
                    this.currentScrollPosition = KeepScrollInBounds(Convert.ToInt32(this.currentScrollPosition));
                    return;
                }
            }

            // "regular" scroll

            bool touchedWithinMenuArea = false; // not counting scrollbar
            Rectangle bgRect = this.BgRect;
            bgRect.X += (int)this.viewParams.PosX;
            bgRect.Y += (int)this.viewParams.PosY;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = touch.Position / Preferences.GlobalScale;
                if (bgRect.Contains(touchPos) && !scrollWholeRect.Contains(touchPos))
                {
                    touchedWithinMenuArea = true;
                    break;
                }
            }

            if (touchedWithinMenuArea)
            {
                float swipeScroll = TouchInput.GetMovementDelta(ignoreLeftStick: false, ignoreRightStick: false, ignoreVirtButtons: true, ignoreInventory: false, ignorePlayerPanel: false).Y / Preferences.GlobalScale;
                if (swipeScroll != 0) this.touchMode = true;
                this.currentScrollPosition += swipeScroll;
                this.currentScrollPosition = KeepScrollInBounds(Convert.ToInt32(this.currentScrollPosition));
            }
        }

        private void NextItem()
        { this.ChangeActiveItem(add: true); }

        private void PreviousItem()
        { this.ChangeActiveItem(add: false); }

        private void ChangeActiveItem(bool add)
        {
            if (Sound.menuOn) this.soundNavigate.Play();

            this.touchMode = false;

            int valueToAdd = add ? 1 : -1;

            this.scrollSpeed = baseScrollSpeed;

            while (true)
            {
                this.activeIndex += valueToAdd;

                if (this.activeIndex == -1 || this.activeIndex == this.entryList.Count)
                {
                    this.scrollSpeed = baseScrollSpeed / 5f;
                    if (this.activeIndex == this.entryList.Count) this.activeIndex = 0;
                    if (this.activeIndex == -1) this.activeIndex = this.entryList.Count - 1;
                }

                Entry activeItem = this.ActiveEntry;
                if (activeItem.GetType() != typeof(Separator)) return;
            }
        }

        public void ChangeActiveItem(Entry entry)
        {
            for (int i = 0; i < this.entryList.Count; i++)
            {
                if (entry == this.entryList[i])
                {
                    this.activeIndex = i;
                    return;
                }
            }

            throw new ArgumentException($"Entry {entry.name} not found when trying to set as active.");
        }

        public void SetActiveIndex(int index)
        {
            if (Sound.menuOn) this.soundNavigate.Play();


            this.touchMode = false;
            this.activeIndex = index;
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, this.BgRect, this.bgColor);

            var visibleEntries = VisibleEntries;
            for (int i = 0; i < visibleEntries.Count; i++)
            {
                Entry menuItem = visibleEntries[i];
                bool itemActive = menuItem == this.ActiveEntry && (this.alwaysShowSelectedEntry || !this.touchMode);

                menuItem.Draw(active: itemActive);
            }

            this.DrawScrollbar();
        }

        private void DrawScrollbar()
        {
            if (!this.ScrollActive) return;

            Rectangle scrollWholeRect = this.ScrollWholeRect;
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, scrollWholeRect, this.bgColor * 0.5f * this.viewParams.Opacity);
            Helpers.DrawRectangleOutline(rect: scrollWholeRect, color: this.entryList[0].outlineColor * 0.5f * this.viewParams.Opacity, borderWidth: 2);

            int scrollPos = (int)(this.CurrentScrollPosition * this.ScrollbarMultiplier);

            Rectangle widgetRect = new Rectangle(this.ScrollbarPosX, scrollPos, this.ScrollbarWidth, this.ScrollbarWidgetHeight);
            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, widgetRect, this.entryList[0].outlineColor * 0.75f * this.viewParams.Opacity);
            Helpers.DrawRectangleOutline(rect: widgetRect, color: this.entryList[0].textColor * this.viewParams.Opacity, borderWidth: 2);
        }

        public void SetViewPosAndSize()
        {
            this.viewParams.Width = this.EntryBgWidth;
            this.viewParams.Height = SonOfRobinGame.VirtualHeight;

            switch (this.layout)
            {
                case Menu.Layout.Middle:
                    this.viewParams.PosX = (SonOfRobinGame.VirtualWidth / 2) - (this.viewParams.Width / 2);
                    break;

                case Menu.Layout.Left:
                    this.viewParams.PosX = 0;
                    break;

                case Menu.Layout.Right:
                    this.viewParams.PosX = SonOfRobinGame.VirtualWidth - this.viewParams.Width;
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported menu layout - {this.layout}.");
            }

            this.viewParams.PosY = 0;
        }
    }
}
