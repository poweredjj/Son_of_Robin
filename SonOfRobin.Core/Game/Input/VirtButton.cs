using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public enum VButName : byte
    {
        Confirm,
        Interact,
        UseTool,
        Shoot,
        PickUp,
        HighlightPickups,
        Inventory,
        FieldCraft,
        Map,
        Sprint,
        ZoomOut,
        WakeUp,
        PauseMenu,
        Return,

        InvDragSingle,
        InvSort,

        MapToggleMarker,
        MapDeleteMarkers,
        MapCenterPlayer,
        MapToggleLocations,

        DebugPause,
        DebugPlay,
        DebugFastForward,
        DebugClear,
        DebugBreakAll,
        DebugBreakVisible,
        DebugClockAdvance,
    }

    public class VirtButton
    {
        private static readonly Dictionary<VButName, string> labelTexturesByNames = new()
        {
            { VButName.Confirm, "check_mark" },
            { VButName.Interact, "interact" },
            { VButName.UseTool, "use_item" },
            { VButName.Shoot, "shoot" },
            { VButName.PickUp, "pick_up" },
            { VButName.HighlightPickups, "highlight" },
            { VButName.Inventory, "inventory" },
            { VButName.FieldCraft, "craft" },
            { VButName.Map, "map" },
            { VButName.Sprint, "sprint" },
            { VButName.ZoomOut, "zoom_out" },
            { VButName.WakeUp, "wake_up" },
            { VButName.PauseMenu, "pause_menu" },
            { VButName.Return, "return" },
            { VButName.InvDragSingle, "drag_single" },
            { VButName.InvSort, "sort" },
            { VButName.MapToggleMarker, "create_marker" },
            { VButName.MapDeleteMarkers, "delete_markers" },
            { VButName.MapCenterPlayer, "go_to_player" },
            { VButName.MapToggleLocations, "location_names" },
        };

        public static Dictionary<VButName, VirtButton> buttonsByName = new Dictionary<VButName, VirtButton> { };

        private readonly int frameCreated;
        private readonly bool checksTouchFromPrevLayout;

        private readonly float posX0to1;
        private readonly float posY0to1;
        private readonly float width0to1;
        private readonly float height0to1;

        private readonly string label;
        private readonly Texture2D labelTexture;
        private Texture2D temporaryLabelTexture;
        private int temporaryLabelTextureFramesLeft;

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

        public VirtButton(VButName name, string label, float posX0to1, float posY0to1, float width0to1, float height0to1, Color bgColorPressed, Color bgColorReleased, Color textColor,
            bool switchButton = false, bool hidden = false, Object activeCoupledObj = null, string activeCoupledVarName = "", bool isHighlighted = true, string highlightCoupledVarName = null, Object highlightCoupledObj = null, bool checksTouchFromPrevLayout = false)
        {
            this.frameCreated = SonOfRobinGame.CurrentUpdate;
            this.checksTouchFromPrevLayout = checksTouchFromPrevLayout;

            this.label = label;
            this.labelTexture = GetLabelTexture(name);
            this.bgColorPressed = bgColorPressed;
            this.bgColorReleased = bgColorReleased;
            this.textColor = textColor;

            this.textureReleased = TextureBank.GetTexture(TextureBank.TextureName.VirtButtonBGReleased);
            this.texturePressed = TextureBank.GetTexture(TextureBank.TextureName.VirtButtonBGPressed);

            this.hidden = hidden;

            this.posX0to1 = posX0to1;
            this.posY0to1 = posY0to1;
            this.width0to1 = width0to1;
            this.height0to1 = height0to1;

            this.activeCoupledObj = activeCoupledObj;
            this.activeCoupledVarName = activeCoupledVarName;
            bool initialValue = this.activeCoupledVarName != "" && this.GetCoupledVar();

            this.isDown = initialValue;
            this.highlighter = new Highlighter(isOn: isHighlighted, coupledObj: highlightCoupledObj, coupledVarName: highlightCoupledVarName);
            this.switchedState = initialValue;
            this.wasDownLastFrame = false;

            this.switchButton = switchButton;

            buttonsByName[name] = this;
        }

        public static Texture2D GetLabelTexture(VButName name)
        {
            return labelTexturesByNames.ContainsKey(name) ? TextureBank.GetTexture($"input/VirtButton/{labelTexturesByNames[name]}") : null;
        }

        private bool HasBeenPressed
        { get { return this.IsActive && !this.wasDownLastFrame; } }

        private bool HasBeenReleased
        { get { return !this.IsActive && this.wasDownLastFrame; } }

        private bool IsActive
        { get { return this.switchButton ? this.switchedState : this.isDown; } }

        private int Width
        { get { return (int)(SonOfRobinGame.VirtualWidth * this.width0to1); } }

        private int Height
        { get { return (int)(SonOfRobinGame.VirtualWidth * this.height0to1); } }  // VirtualWidth is repeated to maintain button proportions

        private Vector2 PosCenter
        { get { return new Vector2(SonOfRobinGame.VirtualWidth * this.posX0to1, SonOfRobinGame.VirtualHeight * this.posY0to1); } }

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

        private SpriteFont Font
        {
            get
            {
                SpriteFont font = SonOfRobinGame.FontTommy40;
                if (font == null) font = SonOfRobinGame.FontPressStart2P5;

                return font;
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

        private bool JustCreated
        { get { return this.frameCreated == SonOfRobinGame.CurrentUpdate; } }

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

        public static void ButtonChangeTextureOnNextFrame(VButName buttonName, Texture2D texture)
        {
            if (!Input.InputActive || !buttonsByName.ContainsKey(buttonName)) return;
            buttonsByName[buttonName].AssingTemporaryLabelTexture(texture: texture);
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

        public void AssingTemporaryLabelTexture(Texture2D texture, int duration = 1)
        {
            this.temporaryLabelTexture = texture;
            this.temporaryLabelTextureFramesLeft = duration;
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
                //MessageLog.AddMessage(debugMessage: true, message: $"Touch position: {touch.Position.X},{touch.Position.Y}.", color: Color.GreenYellow);
                Vector2 position = touch.Position / Preferences.GlobalScale;

                if (this.Rect.Contains(position))
                {
                    // MessageLog.AddMessage(debugMessage: true, message: $"Button {this.label} is down.", color: Color.GreenYellow);

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
            float opacityMultiplier = this.highlighter.IsOn ? 1f : 0.5f;

            if (this.IsActive)
            {
                Rectangle srcRectPressed = new Rectangle(0, 0, this.texturePressed.Width, this.texturePressed.Height);
                SonOfRobinGame.SpriteBatch.Draw(this.texturePressed, gfxRect, srcRectPressed, this.bgColorPressed * opacityMultiplier);
            }

            Rectangle srcRectReleased = new Rectangle(0, 0, this.textureReleased.Width, this.textureReleased.Height);
            SonOfRobinGame.SpriteBatch.Draw(this.textureReleased, gfxRect, srcRectReleased, this.bgColorReleased * opacityMultiplier);

            Rectangle wholeLabelRect = gfxRect;
            wholeLabelRect.Inflate(-wholeLabelRect.Width * 0.08f, -wholeLabelRect.Height * 0.2f);

            if (this.labelTexture == null)
            {
                var labelLines = this.label.Split('\n');
                int labelRectHeight = wholeLabelRect.Height / labelLines.Length;
                int labelRectYOffset = 0;

                SpriteFont font = this.Font;

                foreach (string currentLabel in labelLines)
                {
                    Rectangle currentLabelRect = new Rectangle(x: wholeLabelRect.X, y: wholeLabelRect.Y + labelRectYOffset, width: wholeLabelRect.Width, height: labelRectHeight);

                    Helpers.DrawTextInsideRectWithOutline(font: font, text: currentLabel, rectangle: currentLabelRect, color: this.textColor * opacityMultiplier, outlineColor: new Color(0, 0, 0, 128), outlineSize: 1, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center, drawTestRect: false);

                    labelRectYOffset += labelRectHeight;
                }
            }
            else // labelTexture, instead of label text
            {
                Texture2D textureToDraw = this.labelTexture;

                if (this.temporaryLabelTexture != null)
                {
                    textureToDraw = this.temporaryLabelTexture;

                    this.temporaryLabelTextureFramesLeft--;
                    if (this.temporaryLabelTextureFramesLeft <= 0) this.temporaryLabelTexture = null;
                }

                Helpers.DrawTextureInsideRect(texture: textureToDraw, rectangle: wholeLabelRect, color: Color.White * opacityMultiplier);
            }
        }
    }
}