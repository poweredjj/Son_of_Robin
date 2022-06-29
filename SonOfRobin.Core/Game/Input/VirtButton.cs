using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public enum VButName
    {
        Confirm,
        Interact,
        UseTool,
        Shoot,
        PickUp,
        Inventory,
        Equip,
        FieldCraft,
        Map,
        Sprint,
        ZoomOut,
        PauseMenu,
        Return,
        DragSingle,

        DebugPause,
        DebugPlay,
        DebugFastForward,
        DebugClear,
        DebugBreakAll,
        DebugRemoveTopScene,
    }

    public class VirtButton
    {
        private readonly int frameCreated;
        private readonly bool checksTouchFromPrevLayout;

        private readonly float posX0to1;
        private readonly float posY0to1;
        private readonly float width0to1;
        private readonly float height0to1;

        private readonly string label;

        private readonly Color bgColorPressed;
        private readonly Color bgColorReleased;
        private readonly Color textColor;
        private readonly Texture2D textureReleased;
        private readonly Texture2D texturePressed;
        private bool isDown;
        private Highlighter highlighter;
        private bool wasDownLastFrame;
        private bool switchedState;
        private readonly bool hidden;
        private readonly bool switchButton;
        private readonly Object activeCoupledObj;
        private readonly string activeCoupledVarName;
        private readonly static SpriteFont font = SonOfRobinGame.fontPressStart2P5;

        public static Dictionary<VButName, VirtButton> buttonsByName = new Dictionary<VButName, VirtButton> { };
        private bool HasBeenPressed { get { return this.IsActive && !this.wasDownLastFrame; } }
        private bool HasBeenReleased { get { return !this.IsActive && this.wasDownLastFrame; } }
        private bool IsActive { get { return this.switchButton ? this.switchedState : this.isDown; } }
        private int Width { get { return Convert.ToInt32(SonOfRobinGame.VirtualWidth * this.width0to1); } }
        private int Height { get { return Convert.ToInt32(SonOfRobinGame.VirtualWidth * this.height0to1); } }  // VirtualWidth is repeated to maintain button proportions
        private Vector2 PosCenter { get { return new Vector2(SonOfRobinGame.VirtualWidth * this.posX0to1, SonOfRobinGame.VirtualHeight * this.posY0to1); } }
        private Rectangle Rect
        {
            get
            {
                int width = this.Width;
                int height = this.Height;

                Vector2 posUpperLeft = PosCenter - (new Vector2(width, height) / 2);
                return new Rectangle(
                    x: (int)posUpperLeft.X,
                    y: (int)posUpperLeft.Y,
                    width: width, height: height);
            }
        }

        public static List<Rectangle> AllButtonRects
        {
            get
            {
                var rectList = new List<Rectangle>();

                foreach (VirtButton button in buttonsByName.Values)
                { rectList.Add(button.Rect); }

                return rectList;
            }
        }

        private bool JustCreated { get { return this.frameCreated == SonOfRobinGame.currentUpdate; } }

        public VirtButton(VButName name, string label, float posX0to1, float posY0to1, float width0to1, float height0to1, Color bgColorPressed, Color bgColorReleased, Color textColor, bool switchButton = false, bool hidden = false,
            Object activeCoupledObj = null, string activeCoupledVarName = "",
            bool isHighlighted = true, string highlightCoupledVarName = null, Object highlightCoupledObj = null, bool checksTouchFromPrevLayout = false)
        {
            this.frameCreated = SonOfRobinGame.currentUpdate;
            this.checksTouchFromPrevLayout = checksTouchFromPrevLayout;

            this.label = label;
            this.bgColorPressed = bgColorPressed;
            this.bgColorReleased = bgColorReleased;
            this.textColor = textColor;

            this.textureReleased = SonOfRobinGame.textureByName["virtual_button"];
            this.texturePressed = SonOfRobinGame.textureByName["virtual_button_pressed"];

            this.hidden = hidden;

            this.posX0to1 = posX0to1;
            this.posY0to1 = posY0to1;
            this.width0to1 = width0to1;
            this.height0to1 = height0to1;

            this.activeCoupledObj = activeCoupledObj;
            this.activeCoupledVarName = activeCoupledVarName;
            bool initialValue = this.activeCoupledVarName != "" ? this.GetCoupledVar() : false;

            this.isDown = initialValue;
            this.highlighter = new Highlighter(isOn: isHighlighted, coupledObj: highlightCoupledObj, coupledVarName: highlightCoupledVarName);
            this.switchedState = initialValue;
            this.wasDownLastFrame = false;

            this.switchButton = switchButton;

            buttonsByName[name] = this;
        }

        public static bool HasButtonBeenPressed(VButName buttonName)
        {
            if (!Input.InputActive || !buttonsByName.ContainsKey(buttonName)) return false;
            return buttonsByName[buttonName].HasBeenPressed;
        }

        public static bool HasButtonBeenReleased(VButName buttonName)
        {
            if (!Input.InputActive || !buttonsByName.ContainsKey(buttonName)) return false;
            return buttonsByName[buttonName].HasBeenReleased;
        }

        public static bool IsButtonDown(VButName buttonName)
        {
            if (!Input.InputActive || !buttonsByName.ContainsKey(buttonName)) return false;
            return buttonsByName[buttonName].IsActive;
        }

        public static void ButtonHighlightOnNextFrame(VButName buttonName)
        {
            if (!Input.InputActive || !buttonsByName.ContainsKey(buttonName)) return;
            buttonsByName[buttonName].highlighter.SetOnForNextRead();
        }
        public static void UpdateAll()
        {
            foreach (VirtButton button in buttonsByName.Values)
            {
                button.Update();
            }
        }

        public static void RemoveAll()
        {
            buttonsByName.Clear();
        }

        private bool UpdateFromCoupledVar()
        {
            if (this.activeCoupledVarName == "") return false;

            bool coupledVar = this.GetCoupledVar();

            if (this.switchButton)
            {
                if (coupledVar != this.switchedState)
                {
                    this.switchedState = coupledVar;
                    return true;
                }
            }
            else
            {
                if (coupledVar != this.isDown)
                {
                    this.isDown = coupledVar;
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            if (this.UpdateFromCoupledVar()) return;

            this.wasDownLastFrame = this.isDown;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Touch position: {touch.Position.X},{touch.Position.Y}.", color: Color.GreenYellow);
                Vector2 position = touch.Position / Preferences.GlobalScale;

                if (this.Rect.Contains(position))
                {
                    // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Button {this.label} is down.", color: Color.GreenYellow);

                    if (touch.State == TouchLocationState.Pressed ||
                        (touch.State == TouchLocationState.Moved && (this.wasDownLastFrame || (this.checksTouchFromPrevLayout && this.JustCreated))))
                    {
                        this.isDown = true;
                        if (!this.wasDownLastFrame)
                        {
                            this.switchedState = !this.switchedState;
                            this.SetCoupledVar();
                        }

                        return;
                    }
                }
            }

            if (this.isDown)
            {
                this.isDown = false;
                this.SetCoupledVar();
            }
        }

        private bool GetCoupledVar()
        {
            return (bool)Helpers.GetProperty(targetObj: this.activeCoupledObj, propertyName: this.activeCoupledVarName);
        }

        private void SetCoupledVar()
        {
            if (this.activeCoupledVarName != "") Helpers.SetProperty(targetObj: this.activeCoupledObj, propertyName: this.activeCoupledVarName, newValue: this.IsActive);
        }

        public static void DrawAll()
        {
            foreach (VirtButton button in buttonsByName.Values)
            { button.Draw(); }
        }

        public void Draw()
        {
            if (this.hidden) return;

            Rectangle gfxRect = this.Rect;
            Rectangle sourceRectangle;
            float opacityMultiplier = this.highlighter.IsOn ? 1f : 0.5f;

            if (this.IsActive)
            {
                sourceRectangle = new Rectangle(0, 0, this.texturePressed.Width, this.texturePressed.Height);
                SonOfRobinGame.spriteBatch.Draw(this.texturePressed, gfxRect, sourceRectangle, this.bgColorPressed * opacityMultiplier);
            }

            Vector2 posCenter = this.PosCenter;

            sourceRectangle = new Rectangle(0, 0, this.textureReleased.Width, this.textureReleased.Height);
            SonOfRobinGame.spriteBatch.Draw(this.textureReleased, gfxRect, sourceRectangle, this.bgColorReleased * opacityMultiplier);

            var labelSize = font.MeasureString(this.label);
            float targetTextWidth = this.Width * 0.85f;
            float textScale = targetTextWidth / labelSize.X;

            Vector2 posLabelUpperLeft = posCenter - new Vector2(labelSize.X / 2 * textScale, labelSize.Y / 2 * textScale);

            SonOfRobinGame.spriteBatch.DrawString(font, this.label, position: posLabelUpperLeft, color: this.textColor * opacityMultiplier, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

    }
}
