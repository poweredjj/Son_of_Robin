using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Studies.Joystick.Input;
using System;
using System.Linq;

namespace SonOfRobin
{
    public enum TouchLayout
    {
        Uninitialized = 0, // initial layout for TouchOverlay
        Empty = 1,
        MenuRightReturn = 2,
        MenuMiddleReturn = 3,
        MenuLeftReturn = 4,
        CaptureInputCancel = 5,
        WorldMain = 6,
        WorldShoot = 7,
        WorldSleep = 8,
        WorldBuild = 9,
        WorldSpectator = 10,
        Inventory = 11,
        Map = 12,
        QuitLoading = 13,
        TextWindowOk = 14,
        TextWindowCancel = 15,
        TextWindowOkCancel = 16,
    }

    public class TouchInput
    {
        public static TouchLayout currentLayout;

        private static float stickScale = -100; // dummy value
        private static int screenWidth = -100;
        private static int screenHeight = -100;

        private static int emulatedTouchID = 0;
        private static Vector2 emulatedTouchLastPos = Vector2.Zero;
        private static TouchLocationState emulatedTouchLastState = TouchLocationState.Invalid;

        public static bool ShowSticks
        {
            get { return ShowLeftStick || ShowRightStick; }
            set
            {
                ShowLeftStick = value;
                ShowRightStick = value;
            }
        }

        public static bool ShowLeftStick { get; private set; }
        public static bool ShowRightStick { get; private set; }

        public static DualStick dualStick;

        private static Vector2 leftStick = Vector2.Zero;
        private static Vector2 rightStick = Vector2.Zero;

        private static Vector2 emptyStick = Vector2.Zero;

        public static Vector2 LeftStick
        { get { return Input.InputActive && Preferences.enableTouchJoysticks ? leftStick : emptyStick; } }

        public static Vector2 RightStick
        { get { return Input.InputActive && Preferences.enableTouchJoysticks ? rightStick : emptyStick; } }

        private static TouchCollection lastFrameTouchPanelState = new TouchCollection { };
        private static TouchCollection touchPanelState = new TouchCollection { };
        private static readonly TouchCollection emptyTouchList = new TouchCollection { };

        public static TouchCollection TouchPanelState
        { get { return Input.InputActive ? touchPanelState : emptyTouchList; } }

        public static bool IsGestureAvailable
        { get { return Input.InputActive && TouchPanel.IsGestureAvailable; } } // should be used before reading gesture directly

        public static bool IsBeingTouchedInAnyWay
        { get { return TouchPanelState.Count > 0; } }

        private static DateTime lastPressedTime = DateTime.Now;
        private static Vector2 lastPressPos = new Vector2(-100, -100); //  (-100, -100) == null equivalent
        private static Vector2 lastReleasedPos = new Vector2(-100, -100); //  (-100, -100) == null equivalent

        private static void UpdateLastPressedReleasedPos()
        {
            foreach (TouchLocation touch in touchPanelState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    lastPressPos = touch.Position;
                    lastPressedTime = DateTime.Now;
                    lastReleasedPos.X = -100;
                    lastReleasedPos.Y = -100;
                    return;
                }

                if (touch.State == TouchLocationState.Released)
                {
                    lastReleasedPos = touch.Position;
                    return;
                }
            }
        }

        public static bool IsPressedReleasedWithinDistanceAndDuration(int maxDistance, TimeSpan maxDuration, DateTime ignorePressesBefore)
        {
            if (!Input.InputActive ||
                (lastPressPos.X == -100 && lastPressPos.Y == -100) ||
                (lastReleasedPos.X == -100 && lastReleasedPos.Y == -100) ||
                lastPressedTime < ignorePressesBefore) return false;

            float distance = Vector2.Distance(lastPressPos, lastReleasedPos);

            TimeSpan pressReleaseDuration = DateTime.Now - lastPressedTime;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Press release distance {distance}, max dist {maxDistance}, duration {pressReleaseDuration.Milliseconds} ms, maxDuration {maxDuration.Milliseconds} ms.");

            return distance <= maxDistance && pressReleaseDuration <= maxDuration;
        }

        public static Vector2 GetMovementDelta(bool ignoreLeftStick, bool ignoreRightStick, bool ignoreVirtButtons, bool ignoreInventory, bool ignorePlayerPanel, bool preventLargeJump = true)
        {
            if (!Input.InputActive || (!lastFrameTouchPanelState.Any() && !touchPanelState.Any())) return Vector2.Zero;

            Vector2 prevPos = Vector2.Zero;
            Vector2 currentPos = Vector2.Zero;

            foreach (TouchLocation touch in lastFrameTouchPanelState)
            {
                if (touch.State != TouchLocationState.Released && !IsPointActivatingAnyTouchInterface(point: touch.Position, checkLeftStick: ignoreLeftStick, checkRightStick: ignoreRightStick, checkVirtButtons: ignoreVirtButtons, checkInventory: ignoreInventory, checkPlayerPanel: ignorePlayerPanel))
                {
                    prevPos = touch.Position;
                    break;
                }
            }

            foreach (TouchLocation touch in touchPanelState)
            {
                if (touch.State != TouchLocationState.Released && !IsPointActivatingAnyTouchInterface(point: touch.Position, checkLeftStick: ignoreLeftStick, checkRightStick: ignoreRightStick, checkVirtButtons: ignoreVirtButtons, checkInventory: ignoreInventory, checkPlayerPanel: ignorePlayerPanel))
                {
                    currentPos = touch.Position;
                    break;
                }
            }

            if (prevPos == Vector2.Zero || currentPos == Vector2.Zero) return Vector2.Zero;

            Vector2 movementDelta = prevPos - currentPos;
            if (preventLargeJump && (Math.Abs(movementDelta.X) > 220 || Math.Abs(movementDelta.Y) > 220)) movementDelta = Vector2.Zero;

            return movementDelta;
        }

        public static float GetZoomDelta(bool ignoreLeftStick, bool ignoreRightStick, bool ignoreVirtButtons, bool ignoreInventory, bool ignorePlayerPanel, bool preventLargeJump = true)
        {
            if (!Input.InputActive || (!lastFrameTouchPanelState.Any() && !touchPanelState.Any())) return 0;

            Vector2 prevPos1 = Vector2.Zero;
            Vector2 prevPos2 = Vector2.Zero;

            Vector2 currentPos1 = Vector2.Zero;
            Vector2 currentPos2 = Vector2.Zero;

            foreach (TouchLocation touch in lastFrameTouchPanelState)
            {
                if (touch.State != TouchLocationState.Released && !IsPointActivatingAnyTouchInterface(point: touch.Position, checkLeftStick: ignoreLeftStick, checkRightStick: ignoreRightStick, checkVirtButtons: ignoreVirtButtons, checkInventory: ignoreInventory, checkPlayerPanel: ignorePlayerPanel))
                {
                    if (prevPos1 == Vector2.Zero) prevPos1 = touch.Position;
                    else
                    {
                        prevPos2 = touch.Position;
                        break;
                    }
                }
            }

            foreach (TouchLocation touch in touchPanelState)
            {
                if (touch.State != TouchLocationState.Released && !IsPointActivatingAnyTouchInterface(point: touch.Position, checkLeftStick: ignoreLeftStick, checkRightStick: ignoreRightStick, checkVirtButtons: ignoreVirtButtons, checkInventory: ignoreInventory, checkPlayerPanel: ignorePlayerPanel))
                {
                    if (currentPos1 == Vector2.Zero) currentPos1 = touch.Position;
                    else
                    {
                        currentPos2 = touch.Position;
                        break;
                    }
                }
            }

            if (prevPos1 == Vector2.Zero || prevPos2 == Vector2.Zero || currentPos1 == Vector2.Zero || currentPos2 == Vector2.Zero) return 0;
            if (Vector2.Distance(currentPos1, currentPos2) < 350) return 0; // preventing glitching, when player fingers are too close

            float prevDeltaX = Math.Abs(prevPos1.X - prevPos2.X) / SonOfRobinGame.VirtualWidth;
            float prevDeltaY = Math.Abs(prevPos1.Y - prevPos2.Y) / SonOfRobinGame.VirtualHeight;

            float currentDeltaX = Math.Abs(currentPos1.X - currentPos2.X) / SonOfRobinGame.VirtualWidth;
            float currentDeltaY = Math.Abs(currentPos1.Y - currentPos2.Y) / SonOfRobinGame.VirtualHeight;

            float zoomDelta = ((currentDeltaX - prevDeltaX) + (currentDeltaY - prevDeltaY)) / 2f;

            if (preventLargeJump && zoomDelta > 0.2f) zoomDelta = 0;

            return zoomDelta;
        }

        public static bool IsStateAvailable(TouchLocationState state)
        {
            var matchingTypes = TouchPanelState.Where(touch => touch.State == state);
            return matchingTypes.Any();
        }

        private static int lastFrameLayoutChanged = 0;

        public static int FramesSinceLayoutChanged
        { get { return SonOfRobinGame.CurrentUpdate - lastFrameLayoutChanged; } }

        public static void GetState()
        {
            lastFrameTouchPanelState = touchPanelState;
            touchPanelState = (SonOfRobinGame.platform == Platform.Desktop || SonOfRobinGame.fakeMobileMode) && SonOfRobinGame.os != OS.Windows ?
                ConvertMouseToTouch() : TouchPanel.GetState();

            UpdateLastPressedReleasedPos();

            // ShowDebugTouchMessages(touchPanelState); // for testing only

            if (!Preferences.EnableTouchButtons) return;
            Refresh();

            dualStick.Update(SonOfRobinGame.CurrentGameTime);
            leftStick = dualStick.LeftStick.GetRelativeVector(dualStick.aliveZoneSize) / dualStick.aliveZoneSize;
            rightStick = dualStick.RightStick.GetRelativeVector(dualStick.aliveZoneSize) / dualStick.aliveZoneSize;

            VirtButton.UpdateAll();
        }

        private static TouchCollection ConvertMouseToTouch()
        {
            // Automatic touch to mouse conversion works only in WindowsDX build.
            // In other cases (DesktopGL), this emulation does the job.

            Vector2 mousePos = new Vector2(Mouse.XValue, Mouse.YValue);

            if (Mouse.LeftHasBeenPressed)
            {
                emulatedTouchID++;
                emulatedTouchLastPos = mousePos;
                emulatedTouchLastState = TouchLocationState.Pressed;

                TouchLocation[] touches = { new TouchLocation(id: emulatedTouchID, state: TouchLocationState.Pressed, position: mousePos) };
                return new TouchCollection(touches: touches);
            }

            if (!Mouse.LeftHasBeenPressed && Mouse.LeftIsDown)
            {
                TouchLocation[] touches = { new TouchLocation(id: emulatedTouchID, state: TouchLocationState.Moved, previousState: emulatedTouchLastState, position: mousePos, previousPosition: emulatedTouchLastPos) };

                emulatedTouchLastPos = mousePos;
                emulatedTouchLastState = TouchLocationState.Moved;

                return new TouchCollection(touches: touches);
            }

            if (Mouse.LeftHasBeenReleased)
            {
                TouchLocation[] touches = { new TouchLocation(id: emulatedTouchID, state: TouchLocationState.Released, previousState: emulatedTouchLastState, position: mousePos, previousPosition: emulatedTouchLastPos) };

                emulatedTouchLastPos = Vector2.Zero;
                emulatedTouchLastState = TouchLocationState.Invalid;

                return new TouchCollection(touches: touches);
            }

            return new TouchCollection();
        }

        private static void ShowDebugTouchMessages(TouchCollection touchPanelState)
        {
            // for testing only

            foreach (TouchLocation touch in touchPanelState)
            {
                MessageLog.AddMessage(msgType: MsgType.User, message: $"{SonOfRobinGame.CurrentUpdate} touch {touch.State} x:{touch.Position.X} y:{touch.Position.Y}");
            }
        }

        public static bool IsPointActivatingAnyTouchInterface(Vector2 point, bool checkLeftStick = true, bool checkRightStick = true, bool checkVirtButtons = true, bool checkInventory = true, bool checkPlayerPanel = true)
        {
            Vector2 scaledPoint = point / Preferences.GlobalScale;

            if (Preferences.enableTouchJoysticks && (checkLeftStick || checkRightStick) && IsPointInsideSticks(point: scaledPoint, checkLeftStick: checkLeftStick, checkRightStick: checkRightStick)) return true;

            if (checkVirtButtons)
            {
                foreach (Rectangle virtButtonRect in VirtButton.AllButtonRects)
                {
                    if (virtButtonRect.Contains(scaledPoint)) return true;
                }
            }

            if (checkInventory)
            {
                foreach (Inventory inventory in Scene.GetAllScenesOfType(typeof(Inventory)))
                {
                    Rectangle invRect = inventory.BgRect;
                    invRect.X += (int)inventory.viewParams.drawPosX;
                    invRect.Y += (int)inventory.viewParams.drawPosY;

                    invRect.Inflate(2, 2);

                    if (invRect.Contains(scaledPoint)) return true;
                }
            }

            if (checkPlayerPanel)
            {
                foreach (PlayerPanel playerPanel in Scene.GetAllScenesOfType(typeof(PlayerPanel)))
                {
                    Rectangle counterRect = playerPanel.CounterRect;
                    counterRect.X += (int)playerPanel.viewParams.drawPosX;
                    counterRect.Y += (int)playerPanel.viewParams.drawPosY;

                    counterRect.Inflate(2, 2);

                    if (counterRect.Contains(scaledPoint)) return true;
                }
            }

            return false;
        }

        public static bool IsPointInsideSticks(Vector2 point, bool checkLeftStick = true, bool checkRightStick = true)
        {
            if (!Preferences.EnableTouchButtons || !ShowSticks) return false;

            var sticks = dualStick.Draw(getBGRectsOnly: true);
            var leftStick = sticks[0];
            var rightStick = sticks[1];

            foreach (Rectangle stickRect in sticks)
            {
                if (stickRect == leftStick && !checkLeftStick) continue;
                if (stickRect == rightStick && !checkRightStick) continue;

                stickRect.Inflate(8, 8); // to add some margin around the stick
                if (stickRect.Contains(point)) return true;
            }

            return false;
        }

        private static void Refresh()
        {
            if (stickScale == Preferences.GlobalScale && screenWidth == SonOfRobinGame.GfxDevMgr.PreferredBackBufferWidth && screenHeight == SonOfRobinGame.GfxDevMgr.PreferredBackBufferHeight) return;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Changing touch sticks scale from {stickScale} to {Preferences.GlobalScale}.", color: Color.White);

            SetEmulationByMouse();

            dualStick = new DualStick(aliveZoneSize: TouchPanel.DisplayHeight * 0.25f, deadZoneSize: 16f); // DualStick accepts touch panel size values
            dualStick.LeftStick.SetAsFixed();
            dualStick.RightStick.SetAsFixed();

            stickScale = Preferences.GlobalScale;
            screenWidth = SonOfRobinGame.GfxDevMgr.PreferredBackBufferWidth;
            screenHeight = SonOfRobinGame.GfxDevMgr.PreferredBackBufferHeight;
        }

        public static void SetEmulationByMouse()
        {
            TouchPanel.EnableMouseGestures = Preferences.MouseGesturesEmulateTouch;
            TouchPanel.EnableMouseTouchPoint = Preferences.MouseGesturesEmulateTouch;
            if (!Preferences.EnableTouchButtons) touchPanelState = new TouchCollection { };
        }

        public static void SwitchToLayout(TouchLayout touchLayout)
        {
            if (!Preferences.EnableTouchButtons || touchLayout == currentLayout) return;

            World world = World.GetTopWorld();
            Preferences preferences = new Preferences();

            currentLayout = touchLayout;
            lastFrameLayoutChanged = SonOfRobinGame.CurrentUpdate;

            VirtButton.RemoveAll();

            if (Preferences.DebugMode) AddDebugButtons();

            float size = 0.085f;

            switch (touchLayout)
            {
                case TouchLayout.Empty:
                    {
                        ShowSticks = false;
                        return;
                    }

                case TouchLayout.WorldMain:
                    {
                        ShowSticks = true;
                        float xShift = 0.09f;
                        float yShift = 0.20f;

                        // right side

                        float xPos = 0.76f;
                        float yPos = 0.12f;

                        new VirtButton(name: VButName.Map, label: "MAP", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, highlightCoupledObj: world, highlightCoupledVarName: "MapEnabled");

                        xPos += xShift;
                        new VirtButton(name: VButName.ZoomOut, label: "ZOOM\nOUT", bgColorPressed: Color.Orange, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, switchButton: true, activeCoupledObj: preferences, activeCoupledVarName: "zoomedOut", isHighlighted: false);

                        xPos += xShift;
                        new VirtButton(name: VButName.Sprint, label: "SPRINT", bgColorPressed: Color.Red, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, isHighlighted: false);
                        yPos += yShift;

                        new VirtButton(name: VButName.Interact, label: "INTERACT", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, isHighlighted: false);
                        xPos -= xShift;

                        new VirtButton(name: VButName.UseTool, label: "USE\nITEM", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, isHighlighted: false, checksTouchFromPrevLayout: true);

                        xPos -= xShift;
                        new VirtButton(name: VButName.PickUp, label: "PICK\nUP", bgColorPressed: Color.LightBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, isHighlighted: false);

                        // left side

                        xPos = 0.06f;
                        yPos = 0.12f;

                        new VirtButton(name: VButName.Inventory, label: "INVENTORY", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        xPos += xShift;
                        new VirtButton(name: VButName.FieldCraft, label: "FIELD\nCRAFT", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        xPos += xShift;
                        new VirtButton(name: VButName.PauseMenu, label: "MENU", bgColorPressed: Color.Yellow, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.WorldShoot:
                    {
                        ShowSticks = true;

                        if (Preferences.enableTouchJoysticks)
                        {
                            // "shoot" button makes no sense without touch joysticks

                            new VirtButton(name: VButName.Shoot, label: "SHOOT", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.06f, posY0to1: 0.32f, width0to1: size, height0to1: size);
                        }

                        new VirtButton(name: VButName.UseTool, label: "USE\nITEM", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.85f, posY0to1: 0.32f, width0to1: size, height0to1: size, isHighlighted: true, checksTouchFromPrevLayout: true);

                        new VirtButton(name: VButName.ZoomOut, label: "ZOOM\nOUT", bgColorPressed: Color.Orange, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.85f, posY0to1: 0.12f, width0to1: size, height0to1: size, switchButton: true, activeCoupledObj: preferences, activeCoupledVarName: "zoomedOut", isHighlighted: false);

                        return;
                    }

                case TouchLayout.Inventory:
                    {
                        ShowSticks = false;

                        float yPos = 0.12f;

                        new VirtButton(name: VButName.Return, label: "RETURN", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.06f, posY0to1: yPos, width0to1: size, height0to1: size);

                        new VirtButton(name: VButName.InvDragSingle, label: "DRAG\nSINGLE", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.94f, posY0to1: yPos, width0to1: size, height0to1: size, switchButton: true);

                        new VirtButton(name: VButName.InvSort, label: "SORT", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.94f, posY0to1: 0.89f, width0to1: size, height0to1: size);
                        return;
                    }

                case TouchLayout.WorldSleep:
                    {
                        ShowSticks = false;

                        new VirtButton(name: VButName.Return, label: "STOP\nREST", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.94f, posY0to1: 0.32f, width0to1: size, height0to1: size, highlightCoupledObj: world.Player, highlightCoupledVarName: "CanWakeNow");

                        return;
                    }

                case TouchLayout.WorldBuild:
                    {
                        ShowSticks = true;

                        float xShift = 0.09f;

                        float xPos = 0.94f;
                        float yPos = 0.32f;

                        new VirtButton(name: VButName.ZoomOut, label: "ZOOM\nOUT", bgColorPressed: Color.Orange, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.85f, posY0to1: 0.12f, width0to1: size, height0to1: size, switchButton: true, activeCoupledObj: preferences, activeCoupledVarName: "zoomedOut", isHighlighted: false);

                        new VirtButton(name: VButName.Return, label: "CANCEL", bgColorPressed: Color.Red, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        xPos -= xShift;

                        new VirtButton(name: VButName.Confirm, label: "PLACE", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, isHighlighted: false);

                        return;
                    }

                case TouchLayout.WorldSpectator:
                    {
                        ShowSticks = true;
                        float xShift = 0.09f;

                        // right side

                        float xPos = 0.76f;
                        float yPos = 0.12f;

                        xPos += xShift * 2;

                        new VirtButton(name: VButName.ZoomOut, label: "ZOOM\nOUT", bgColorPressed: Color.Orange, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size, switchButton: true, activeCoupledObj: preferences, activeCoupledVarName: "zoomedOut", isHighlighted: false);

                        // left side
                        new VirtButton(name: VButName.PauseMenu, label: "MENU", bgColorPressed: Color.Yellow, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.06f, posY0to1: 0.12f, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.Map:
                    {
                        ShowSticks = false;

                        float xPos = 0.76f;
                        float yPos = 0.12f;
                        float xShift = 0.09f;

                        new VirtButton(name: VButName.Return, label: "RETURN", bgColorPressed: Color.CornflowerBlue, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        xPos += xShift;

                        new VirtButton(name: VButName.MapCenterPlayer, label: "GO TO\nPLAYER", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        xPos += xShift;

                        new VirtButton(name: VButName.MapToggleMarker, label: "TOGGLE\nMARKER", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.MenuLeftReturn:
                    {
                        ShowSticks = false;

                        new VirtButton(name: VButName.Return, label: "RETURN", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.7f, posY0to1: 0.85f, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.MenuRightReturn:
                    {
                        ShowSticks = false;

                        new VirtButton(name: VButName.Return, label: "RETURN", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.06f, posY0to1: 0.85f, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.CaptureInputCancel:
                    {
                        ShowSticks = false;

                        float xPos = 0.06f;
                        float yPos = 0.85f;
                        new VirtButton(name: VButName.Return, label: "CANCEL\nINPUT", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.QuitLoading:
                    {
                        ShowSticks = false;

                        float xPos = 0.06f;
                        float yPos = 0.15f;
                        new VirtButton(name: VButName.Return, label: "CANCEL", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.TextWindowOk:
                    {
                        ShowSticks = false;

                        float xPos = 0.94f;
                        float yPos = 0.85f;

                        new VirtButton(name: VButName.Confirm, label: "CONFIRM", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        new VirtButton(name: VButName.Return, label: "hidden\nstart", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.05f, posY0to1: 0.1f, width0to1: size, height0to1: size, hidden: true);

                        return;
                    }

                case TouchLayout.TextWindowCancel:
                    {
                        ShowSticks = false;

                        float xPos = 0.06f;
                        float yPos = 0.85f;

                        new VirtButton(name: VButName.Return, label: "SKIP", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.TextWindowOkCancel:
                    {
                        ShowSticks = false;

                        float yPos = 0.85f;

                        new VirtButton(name: VButName.Return, label: "SKIP", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.06f, posY0to1: yPos, width0to1: size, height0to1: size);

                        new VirtButton(name: VButName.Confirm, label: "CONFIRM", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: 0.94f, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                case TouchLayout.MenuMiddleReturn:
                    {
                        ShowSticks = false;

                        size = 0.07f;
                        float xPos = 0.04f;
                        float yPos = 0.15f;
                        new VirtButton(name: VButName.Return, label: "RETURN", bgColorPressed: Color.LightGreen, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: size, height0to1: size);

                        return;
                    }

                default:
                    throw new ArgumentException($"Unsupported touch layout - {touchLayout}.");
            }
        }

        public static void AddDebugButtons()
        {
            float xPos = 0.32f;
            float yPos = 0.07f;
            float width = 0.065f;
            float height = 0.065f;
            float xShift = 0.07f;
            float yShift = 0.2f;

            new VirtButton(name: VButName.DebugPause, label: "||", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);

            xPos += xShift;
            new VirtButton(name: VButName.DebugPlay, label: "|>", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);

            xPos += xShift;
            new VirtButton(name: VButName.DebugFastForward, label: ">>", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);

            xPos = 0.32f;
            yPos += yShift;
            new VirtButton(name: VButName.DebugClear, label: "clear", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);

            xPos += xShift;
            new VirtButton(name: VButName.DebugBreakAll, label: "break\nall", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);

            xPos += xShift;
            new VirtButton(name: VButName.DebugBreakVisible, label: "break\nvisible", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);

            xPos += xShift;
            new VirtButton(name: VButName.DebugClockAdvance, label: "clock\nadvance", bgColorPressed: Color.Violet, bgColorReleased: Color.White, textColor: Color.White, posX0to1: xPos, posY0to1: yPos, width0to1: width, height0to1: height);
        }
    }
}