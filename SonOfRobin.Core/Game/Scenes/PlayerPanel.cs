using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class PlayerPanel : Scene
    {
        private static readonly SpriteFontBase itemCounterFont = SonOfRobinGame.FontTommy.GetFont(40);
        private static readonly SpriteFontBase buffFont = SonOfRobinGame.FontTommy.GetFont(40);
        private static readonly SpriteFontBase locationFont = SonOfRobinGame.FontTommy.GetFont(40);
        private const int posY = 4;
        private static readonly Color locationOutlineColor = new(0, 36, 156);

        private readonly World world;
        private bool isHidden;

        private static int CounterSize
        { get { return (int)(SonOfRobinGame.VirtualWidth * 0.05f); } }

        public Rectangle CounterRect
        {
            get
            {
                int counterSize = CounterSize;
                Rectangle counterRect = new Rectangle(x: BarWidth + (int)(counterSize * 0.25f), y: posY, width: counterSize, height: counterSize);
                return counterRect;
            }
        }

        public bool IsCounterActivatedByTouch
        {
            get
            {
                var pressTouches = TouchInput.TouchPanelState.Where(touch => touch.State == TouchLocationState.Pressed);
                if (pressTouches.Count() == 0) return false;

                Rectangle counterRect = this.CounterRect;
                counterRect.X += (int)this.viewParams.drawPosX;
                counterRect.Y += (int)this.viewParams.drawPosY;

                foreach (TouchLocation touch in pressTouches)
                {
                    if (counterRect.Contains(touch.Position / Preferences.GlobalScale)) return true;
                }

                return false;
            }
        }

        private static int BarWidth
        { get { return (int)(SonOfRobinGame.VirtualWidth * 0.27f); } }

        private static int BarHeight
        { get { return (int)(BarWidth * 0.03f); } }

        private int IconWidthHeight
        { get { return (int)(BarWidth * 0.1f); } }

        private int IconMargin
        { get { return (int)(BarWidth * 0.03f); } }

        private bool ShouldBeHidden
        {
            get
            {
                if (Preferences.debugDisablePlayerPanel) return true;
                if (this.world.BuildMode) return false;

                Scene menuScene = GetTopSceneOfType(typeof(Menu));
                if (menuScene != null && !menuScene.transManager.HasAnyTransition) return true;

                Scene inventoryScene = GetTopSceneOfType(typeof(Inventory));
                if (inventoryScene != null && ((Inventory)inventoryScene).type != Inventory.Type.SingleBottom && !inventoryScene.transManager.HasAnyTransition) return true;

                return this.world.map.FullScreen ||
                    this.world.CineMode ||
                    this.world.cineCurtains.Enabled ||
                    this.world.SpectatorMode ||
                    this.world.Player == null ||
                    !this.world.Player.sprite.IsOnBoard;
            }
        }

        public PlayerPanel(World world) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.world = world;
            this.isHidden = false;
            this.AdaptToNewSize();
        }

        public override void Update()
        {
            if (this.ShouldBeHidden) return;
        }

        protected override void AdaptToNewSize()
        {
            this.viewParams.Width = (int)BarWidth;
            this.viewParams.CenterView(horizontally: true, vertically: false);
        }

        private void HideOrShow(bool hide)
        {
            if (this.isHidden != hide)
            {
                var paramsToChange = new Dictionary<string, float> { { "PosY", this.viewParams.PosY - (SonOfRobinGame.VirtualHeight / 2) } };
                this.transManager.AddMultipleTransitions(paramsToChange: paramsToChange, outTrans: !hide, duration: 12, refreshBaseVal: false);
            }

            this.isHidden = hide;
        }

        public override void Draw()
        {
            bool shouldBeHidden = this.ShouldBeHidden;
            this.HideOrShow(!shouldBeHidden);

            if (shouldBeHidden && !this.transManager.HasAnyTransition) return;

            this.AdaptToNewSize();

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Player player = world.Player;
            int currentPosY = 0;

            // drawing stat bars
            {
                int width = BarWidth;
                int height = BarHeight;
                int posY = PlayerPanel.posY;

                int posX = 0;
                int vOffsetCorrection = 4;

                StatBar.ChangeBatchFont(spriteFontBase: SonOfRobinGame.FontFreeSansBold.GetFont(12));

                new StatBar(width: width, height: height, label: "food", value: (int)player.fedLevel, valueMax: (int)player.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: AnimData.framesForPkgs[AnimData.PkgName.Burger].texture);
                new StatBar(width: width, height: height, label: "fatigue", value: (int)player.Fatigue, valueMax: (int)player.maxFatigue, colorMin: new Color(255, 255, 0), colorMax: new Color(255, 0, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: TextureBank.GetTexture(TextureBank.TextureName.Bed));
                new StatBar(width: width, height: height, label: "health", value: (int)player.HitPoints, valueMax: (int)player.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: AnimData.framesForPkgs[AnimData.PkgName.Heart].texture);

                currentPosY = StatBar.BatchHeight + 5; // must be invoked before drawing bars

                StatBar.FinishThisBatch();
                StatBar.DrawAll();
            }

            // drawing stored items counter
            {
                int counterSize = CounterSize;
                Rectangle counterRect = this.CounterRect;

                AnimData.framesForPkgs[AnimData.PkgName.BackpackMediumOutline].DrawAndKeepInRectBounds(destBoundsRect: counterRect, color: Color.White * this.viewParams.drawOpacity, opacity: 0.85f);

                int occupiedSlotCount = player.PieceStorage.OccupiedSlots.Count + (player.ToolStorage.OccupiedSlots.Count - 1); // -1 to count out hand "tool"
                int totalSlotCount = player.PieceStorage.AllSlots.Count + (player.ToolStorage.AllSlots.Count - 1); // -1 to count out hand "tool"

                Rectangle occupiedRect = new Rectangle(x: counterRect.X, y: counterRect.Y, width: counterSize / 2, height: counterSize / 2);
                Rectangle totalRect = new Rectangle(x: counterRect.X + (counterSize / 2), y: counterRect.Y + (counterSize / 2), width: counterSize / 2, height: counterSize / 2);
                Rectangle slashRect = counterRect;
                slashRect.X += (int)(slashRect.Width * 0.2f);

                int shadowOffset = (int)Math.Max(counterSize * 0.05f, 1);
                Color textColor = Color.White * this.viewParams.drawOpacity;
                Color shadowColor = Color.Black * 0.8f * this.viewParams.drawOpacity;
                var alignX = Helpers.AlignX.Center;
                var alignY = Helpers.AlignY.Center;

                Helpers.DrawTextInsideRectWithShadow(font: itemCounterFont, text: "/", rectangle: counterRect, color: textColor, shadowColor: shadowColor, alignX: alignX, alignY: alignY, shadowOffset: shadowOffset);
                Helpers.DrawTextInsideRectWithShadow(font: itemCounterFont, text: Convert.ToString(occupiedSlotCount), rectangle: occupiedRect, color: textColor, shadowColor: shadowColor, alignX: alignX, alignY: alignY, shadowOffset: shadowOffset);
                Helpers.DrawTextInsideRectWithShadow(font: itemCounterFont, text: Convert.ToString(totalSlotCount), rectangle: totalRect, color: textColor, shadowColor: shadowColor, alignX: alignX, alignY: alignY, shadowOffset: shadowOffset);
            }

            // drawing location name

            NamedLocations.Location location = this.world.Grid.namedLocations.PlayerLocation;
            if (location != null && location.hasBeenDiscovered)
            {
                Rectangle nameRect = new(x: 0, y: currentPosY, width: BarWidth, height: BarHeight * 2);

                Helpers.DrawTextInsideRect(font: locationFont, text: location.name, rectangle: nameRect, color: Color.White * this.viewParams.drawOpacity, effect: FontSystemEffect.Stroked, effectAmount: 2, drawTestRect: false);

                currentPosY += nameRect.Height + 5;
            }

            // drawing status icons
            {
                int iconPosY = currentPosY;
                int iconPosX = 0;
                int iconWidthHeight = this.IconWidthHeight;
                int margin = this.IconMargin;

                foreach (Buff buff in player.buffEngine.BuffList)
                {
                    string buffText = buff.iconText;

                    if (buffText == null) continue;

                    Color progressColor, bgColor, frameColor, txtColor;

                    if (buff.isPositive)
                    {
                        bgColor = Color.Black * 0.8f;
                        progressColor = Color.Green * 0.8f;
                        frameColor = Color.GreenYellow;
                        txtColor = Color.White;
                    }
                    else
                    {
                        bgColor = Color.Black * 0.4f;
                        progressColor = Color.Red * 0.8f;
                        frameColor = Color.OrangeRed;
                        txtColor = Color.White;
                    }

                    Rectangle iconRect = new Rectangle(iconPosX, iconPosY, iconWidthHeight, iconWidthHeight);
                    Rectangle progressRect;

                    if (buff.autoRemoveDelay > 0)
                    {
                        int buffFramesLeft = Math.Max(buff.endFrame - world.CurrentUpdate, 0);
                        float buffProgress = 1f - ((float)buffFramesLeft / (float)buff.autoRemoveDelay);
                        int bgRectShrink = (int)((float)iconWidthHeight * buffProgress);

                        progressRect = new Rectangle(iconRect.X, iconRect.Y + bgRectShrink, iconWidthHeight, iconWidthHeight - bgRectShrink);
                    }
                    else progressRect = iconRect;

                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, iconRect, bgColor * this.viewParams.drawOpacity);
                    SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, progressRect, progressColor * this.viewParams.drawOpacity);

                    int textMargin = (int)(iconWidthHeight * 0.1f);
                    Rectangle textRect = new Rectangle(x: iconRect.X + textMargin, y: iconRect.Y + textMargin, width: iconWidthHeight - (textMargin * 2), height: iconWidthHeight - (textMargin * 2));

                    if (buffText.Contains("\n"))
                    {
                        string topText = buffText.Split('\n')[0];
                        string bottomText = buffText.Split('\n')[1];

                        Rectangle textRectTop = new Rectangle(x: textRect.X, y: textRect.Y, width: textRect.Width, height: textRect.Height / 2);
                        Rectangle textRectBottom = new Rectangle(x: textRect.X, y: textRect.Y + (textRect.Height / 2), width: textRect.Width, height: textRect.Height / 2);

                        Helpers.DrawTextInsideRect(font: buffFont, text: topText, rectangle: textRectTop, color: txtColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
                        Helpers.DrawTextInsideRect(font: buffFont, text: bottomText, rectangle: textRectBottom, color: txtColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
                    }
                    else
                    {
                        Helpers.DrawTextInsideRect(font: buffFont, text: buffText, rectangle: textRect, color: txtColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
                    }

                    Helpers.DrawRectangleOutline(rect: iconRect, color: frameColor * this.viewParams.drawOpacity, borderWidth: 2);

                    iconPosX += iconWidthHeight + margin;
                    if (iconPosX > BarWidth)
                    {
                        iconPosY += iconWidthHeight + margin;
                        iconPosX = 0;
                    }
                }
            }

            // drawing map markers

            if (!this.transManager.HasAnyTransition)
            {
                foreach (var kvp in this.world.map.mapMarkerByColor)
                {
                    Color markerColor = kvp.Key;
                    BoardPiece markerPiece = kvp.Value;

                    if (!this.transManager.HasAnyTransition &&
                        markerPiece != null &&
                        markerPiece.exists &&
                        markerPiece.sprite.opacity > 0)
                    {
                        // calculating and drawing everything as Vector2 / float to avoid jerky marker motion

                        Camera camera = this.world.camera;
                        Vector2 markerPos = markerPiece.sprite.position;
                        Texture2D markerTexture = markerPiece.sprite.AnimFrame.texture;

                        float tipsHeight = 0; // to avoid drawing marker under ControlTips
                        if (Preferences.ShowControlTips)
                        {
                            ControlTips topTips = ControlTips.GetTopTips();
                            if (topTips != null) tipsHeight = (float)topTips.viewParams.Height / topTips.viewParams.ScaleY;
                        }

                        float markerHeight = Preferences.MapMarkerRealSize * this.world.viewParams.ScaleY;
                        float markerScale = markerHeight / markerTexture.Height;
                        float markerWidth = markerTexture.Width * markerScale;

                        float cameraLeft = this.world.viewParams.PosX * -1;
                        float cameraRight = cameraLeft + camera.viewRect.Width;
                        float cameraTop = this.world.viewParams.PosY * -1;
                        float cameraBottom = cameraTop + camera.viewRect.Height - (tipsHeight * this.world.viewParams.ScaleY);

                        Vector2 offset = Vector2.Zero;

                        if (markerPos.X < cameraLeft) offset.X = cameraLeft - markerPos.X;
                        if (markerPos.X + markerWidth > cameraRight) offset.X = -(markerPos.X + markerWidth - cameraRight);
                        if (markerPos.Y < cameraTop) offset.Y = cameraTop - markerPos.Y;
                        if (markerPos.Y + markerHeight > cameraBottom) offset.Y = -(markerPos.Y + markerHeight - cameraBottom);

                        markerPos += offset;

                        Vector2 markerScreenPos = this.world.TranslateWorldToScreenPos(markerPos);
                        markerScreenPos.X -= this.viewParams.DrawPos.X;
                        markerScreenPos.Y -= this.viewParams.DrawPos.Y;

                        Vector2 markerPosRightBottom = new Vector2(markerPos.X + markerWidth, markerPos.Y + markerHeight);
                        Vector2 markerScreenPosRightBottom = this.world.TranslateWorldToScreenPos(markerPosRightBottom);
                        markerScreenPosRightBottom.X -= this.viewParams.DrawPos.X;
                        markerScreenPosRightBottom.Y -= this.viewParams.DrawPos.Y;

                        float markerDrawScale = (markerScreenPosRightBottom.Y - markerScreenPos.Y) / (float)markerTexture.Height;

                        markerPiece.sprite.effectCol.AddEffect(new ColorizeInstance(color: markerColor, priority: 0));
                        markerPiece.sprite.effectCol.TurnOnNextEffect(scene: this, currentUpdateToUse: this.world.CurrentUpdate);

                        SonOfRobinGame.SpriteBatch.Draw(texture: markerTexture, position: markerScreenPos, scale: markerDrawScale, sourceRectangle: markerTexture.Bounds, color: Color.White * markerPiece.sprite.opacity, rotation: 0, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 0);
                    }
                }
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}