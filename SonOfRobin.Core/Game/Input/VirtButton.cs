using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;


namespace SonOfRobin
{
    public enum VButName
    {
        Interact,
        UseTool,
        Shoot,
        PickUp,
        Inventory,
        Equip,
        FieldCraft,
        Map,
        Run,
        ZoomOut,
        PauseMenu,
        Return,
        DragSingle,

        DebugPause,
        DebugPlay,
        DebugFastForward,
        DebugClear,
        DebugRemoveTopScene,
    }

    public class VirtButton
    {
        private readonly float posX0to1;
        private readonly float posY0to1;
        private readonly float width0to1;
        private readonly float height0to1;

        private readonly string label;

        private readonly Color colorPressed;
        private readonly Color colorReleased;
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
        private readonly static SpriteFont font = SonOfRobinGame.fontSmall;

        public static Dictionary<VButName, VirtButton> buttonsByName = new Dictionary<VButName, VirtButton> { };
        private bool HasBeenPressed { get { return this.IsActive && !this.wasDownLastFrame; } }
        private bool HasBeenReleased { get { return !this.IsActive && this.wasDownLastFrame; } }
        private bool IsActive
        { get { return this.switchButton ? this.switchedState : this.isDown; } }

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
                    x: Convert.ToInt32(posUpperLeft.X),
                    y: Convert.ToInt32(posUpperLeft.Y),
                    width: Width, height: Height);
            }
        }

        public VirtButton(VButName name, string label, float posX0to1, float posY0to1, float width0to1, float height0to1, Color colorPressed, Color colorReleased,
            bool switchButton = false, bool hidden = false,
            Object activeCoupledObj = null, string activeCoupledVarName = "",
            bool isHighlighted = true, string highlightCoupledVarName = null, Object highlightCoupledObj = null)
        {
            this.label = label;
            this.colorPressed = colorPressed;
            this.colorReleased = colorReleased;
            this.textureReleased = SonOfRobinGame.textureByName["virtual_button"];
            this.texturePressed = SonOfRobinGame.textureByName["virtual_button_pressed"];

            this.hidden = hidden;

            this.posX0to1 = posX0to1;
            this.posY0to1 = posY0to1;
            this.width0to1 = width0to1;
            this.height0to1 = height0to1;

            this.activeCoupledObj = activeCoupledObj;
            this.activeCoupledVarName = activeCoupledVarName;
            bool initialValue = false;
            if (this.activeCoupledVarName != "") initialValue = (bool)Helpers.GetProperty(targetObj: this.activeCoupledObj, propertyName: this.activeCoupledVarName);

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
            { button.Update(); }
        }

        public static void RemoveAll()
        {
            buttonsByName = new Dictionary<VButName, VirtButton> { };
        }

        private void Update()
        {
            this.wasDownLastFrame = this.isDown;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Touch position: {touch.Position.X},{touch.Position.Y}.", color: Color.GreenYellow);
                Vector2 position = touch.Position / Preferences.globalScale;

                if (this.Rect.Contains(position))
                {
                    // MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Button {this.label} is down.", color: Color.GreenYellow);

                    if (touch.State == TouchLocationState.Pressed || (touch.State == TouchLocationState.Moved && this.wasDownLastFrame))
                    {
                        this.isDown = true;
                        if (!this.wasDownLastFrame)
                        {
                            this.switchedState = !this.switchedState;
                            this.SetCoupledPref();
                        }

                        return;
                    }
                }
            }

            if (this.isDown)
            {
                this.isDown = false;
                this.SetCoupledPref();
            }
        }

        private void SetCoupledPref()
        { if (this.activeCoupledVarName != "") Helpers.SetProperty(targetObj: new Preferences(), propertyName: this.activeCoupledVarName, newValue: this.IsActive); }

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
                SonOfRobinGame.spriteBatch.Draw(this.texturePressed, gfxRect, sourceRectangle, this.colorPressed * opacityMultiplier);
            }

            Vector2 posCenter = this.PosCenter;

            sourceRectangle = new Rectangle(0, 0, this.textureReleased.Width, this.textureReleased.Height);

            SonOfRobinGame.spriteBatch.Draw(this.textureReleased, gfxRect, sourceRectangle, this.colorReleased * opacityMultiplier);

            var labelSize = font.MeasureString(this.label);
            float targetTextWidth = this.Width * 0.85f;
            float textScale = targetTextWidth / labelSize.X;

            Vector2 posLabelUpperLeft = posCenter - new Vector2(labelSize.X / 2 * textScale, labelSize.Y / 2 * textScale);

            SonOfRobinGame.spriteBatch.DrawString(font, this.label, position: posLabelUpperLeft, color: Color.White * opacityMultiplier, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

    }
}
