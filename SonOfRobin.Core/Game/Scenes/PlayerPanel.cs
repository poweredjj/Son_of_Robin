using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class PlayerPanel : Scene
    {
        private const int posY = 4;

        private readonly SpriteFontBase itemCounterFont;
        private SpriteFontBase buffFont;
        private readonly SpriteFontBase locationFont;
        private readonly World world;
        private bool isHidden;
        private readonly TriSliceBG triSliceBG;

        private static int CounterSize
        { get { return (int)(SonOfRobinGame.ScreenWidth * 0.05f); } }

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
                    if (counterRect.Contains(touch.Position)) return true;
                }

                return false;
            }
        }

        private static int BarWidth
        { get { return (int)(SonOfRobinGame.ScreenWidth * 0.27f); } }

        private static int BarHeight
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
            this.locationFont = SonOfRobinGame.FontTommy.GetFont(60);
            this.itemCounterFont = SonOfRobinGame.FontTommy.GetFont(60);

            this.triSliceBG = TriSliceBG.GetBGForPreset(TriSliceBG.Preset.Buff);

            this.world = world;
            this.isHidden = false;
            this.AdaptToNewSize();
        }

        public override void Update()
        {
            if (this.ShouldBeHidden) return;
            if (this.buffFont == null || this.buffFont.FontSize != Preferences.buffFontSize)
            {
                this.buffFont = SonOfRobinGame.FontVCROSD.GetFont(Preferences.buffFontSize);
            }
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
                var paramsToChange = new Dictionary<string, float> { { "PosY", this.viewParams.PosY - (SonOfRobinGame.ScreenHeight / 2) } };
                this.transManager.AddMultipleTransitions(paramsToChange: paramsToChange, outTrans: !hide, duration: 5, refreshBaseVal: false);
            }

            this.isHidden = hide;
        }

        public override void Draw()
        {
            bool shouldBeHidden = this.ShouldBeHidden;
            this.HideOrShow(!shouldBeHidden);

            if ((shouldBeHidden && !this.transManager.HasAnyTransition)) return;

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

                new StatBar(width: width, height: height, label: "food", value: (int)player.fedLevel, valueMax: (int)player.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, image: AnimData.GetImageObj(AnimData.PkgName.Burger));
                new StatBar(width: width, height: height, label: "fatigue", value: (int)player.Fatigue, valueMax: (int)player.maxFatigue, colorMin: new Color(255, 255, 0), colorMax: new Color(255, 0, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, image: TextureBank.GetImageObj(TextureBank.TextureName.Bed));
                new StatBar(width: width, height: height, label: "health", value: (int)player.HitPoints, valueMax: (int)player.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, image: AnimData.GetImageObj(AnimData.PkgName.Heart));

                currentPosY = StatBar.BatchHeight + 5; // must be invoked before drawing bars

                StatBar.FinishThisBatch();
                StatBar.DrawAll();
            }

            // drawing stored items counter
            {
                int counterSize = CounterSize;
                Rectangle counterRect = this.CounterRect;

                Helpers.DrawTextureInsideRect(texture: TextureBank.GetTexture(TextureBank.TextureName.BackpackMediumOutline), rectangle: counterRect, color: Color.White * this.viewParams.drawOpacity * 0.85f);

                int occupiedSlotCount = player.PieceStorage.OccupiedSlots.Count + (player.ToolStorage.OccupiedSlots.Count - 1); // -1 to count out hand "tool"
                int totalSlotCount = player.PieceStorage.AllSlots.Count + (player.ToolStorage.AllSlots.Count - 1); // -1 to count out hand "tool"

                Rectangle occupiedRect = new(x: counterRect.X, y: counterRect.Y, width: counterSize / 2, height: counterSize / 2);
                Rectangle totalRect = new(x: counterRect.X + (counterSize / 2), y: counterRect.Y + (counterSize / 2), width: counterSize / 2, height: counterSize / 2);
                Rectangle slashRect = counterRect;
                slashRect.X += (int)(slashRect.Width * 0.2f);

                int shadowOffset = (int)Math.Max(counterSize * 0.05f, 1);
                Color textColor = Color.White * this.viewParams.drawOpacity;
                Color shadowColor = Color.Black * 0.8f * this.viewParams.drawOpacity;
                var alignX = Helpers.AlignX.Center;
                var alignY = Helpers.AlignY.Center;

                Helpers.DrawTextInsideRectWithShadow(font: this.itemCounterFont, text: "/", rectangle: counterRect, color: textColor, shadowColor: shadowColor, alignX: alignX, alignY: alignY, shadowOffset: shadowOffset);
                Helpers.DrawTextInsideRectWithShadow(font: this.itemCounterFont, text: Convert.ToString(occupiedSlotCount), rectangle: occupiedRect, color: textColor, shadowColor: shadowColor, alignX: alignX, alignY: alignY, shadowOffset: shadowOffset);
                Helpers.DrawTextInsideRectWithShadow(font: this.itemCounterFont, text: Convert.ToString(totalSlotCount), rectangle: totalRect, color: textColor, shadowColor: shadowColor, alignX: alignX, alignY: alignY, shadowOffset: shadowOffset);
            }

            // drawing location name

            NamedLocations.Location location = this.world.Grid.namedLocations.PlayerLocation;
            if (location != null && location.hasBeenDiscovered)
            {
                Rectangle nameRect = new(x: 0, y: currentPosY, width: BarWidth, height: BarHeight * 2);

                Helpers.DrawTextInsideRect(font: this.locationFont, text: location.name, rectangle: nameRect, color: Color.Black * this.viewParams.drawOpacity, effect: FontSystemEffect.Blurry, effectAmount: 4, drawTestRect: false, drawTimes: 2);

                Helpers.DrawTextInsideRect(font: this.locationFont, text: location.name, rectangle: nameRect, color: Color.White * this.viewParams.drawOpacity, drawTestRect: false);

                currentPosY += nameRect.Height + 5;
            }

            // drawing buff bars
            {
                Vector2 shadowOffset = new Vector2(2, 2);
                Color shadowColor = Color.Black * 0.7f;

                if (player.buffEngine.BuffList.Where(buff => buff.playerPanelText != null).Any())
                {
                    foreach (Buff buff in player.buffEngine.BuffList)
                    {
                        string buffText = buff.playerPanelText;
                        if (buffText == null) continue;

                        currentPosY += 3; // adding margin (must go first, to add margin before first entry)
                        int bgInflateSize = 8;

                        float opacity = this.viewParams.drawOpacity;

                        string buffTextFormatted = buffText;
                        float buffProgress = 1f;

                        if (buff.autoRemoveDelay > 0)
                        {
                            int buffFramesLeft = Math.Max(buff.endFrame - this.world.CurrentUpdate, 0);
                            buffProgress = 1f - ((float)buffFramesLeft / (float)buff.autoRemoveDelay);
                            int buffSecondsLeft = (int)Math.Ceiling((float)buffFramesLeft / 60f);
                            string timespanString = Helpers.ConvertTimeSpanToString(TimeSpan.FromSeconds(buffSecondsLeft));
                            if (timespanString.Length <= 2) timespanString += "s";
                            if (timespanString.StartsWith("0")) timespanString = " " + timespanString.Remove(0, 1);
                            buffTextFormatted += " /c[#b6f3fa]" + timespanString;

                            opacity *= (float)Helpers.ConvertRange(oldMin: buff.endFrame, oldMax: buff.endFrame - 30, newMin: 0, newMax: 1, oldVal: this.world.CurrentUpdate, clampToEdges: true);
                        }

                        RichTextLayout richTextLayout = new RichTextLayout { Font = this.buffFont, Text = buffTextFormatted };
                        Point buffTextSize = richTextLayout.Measure(100000);

                        Rectangle buffBGRect = new Rectangle(
                            x: 0,
                            y: currentPosY,
                            width: (int)buffTextSize.X + (bgInflateSize * 4),
                            height: (int)buffTextSize.Y + (bgInflateSize * 2));

                        Vector2 textPos = new(bgInflateSize * 2, currentPosY + bgInflateSize);

                        Rectangle imageRect = Rectangle.Empty;
                        if (buff.iconTexture != null)
                        {
                            imageRect = new Rectangle(x: (int)textPos.X, y: buffBGRect.Top, width: buffBGRect.Height, height: buffBGRect.Height);
                            int textureMargin = bgInflateSize;
                            buffBGRect.Width += imageRect.Width + textureMargin;
                            textPos.X += imageRect.Width + textureMargin;
                            textPos.Y -= 2; // making room for progress rect
                            imageRect.Inflate(0, -3); // texture should be a little smaller than the background
                        }

                        Color bgColor = buff.isPositive ? new Color(4, 92, 27) : new Color(128, 48, 3);

                        this.triSliceBG.Draw(triSliceRect: buffBGRect, color: bgColor * 0.7f * opacity);
                        if (buff.iconTexture != null) Helpers.DrawTextureInsideRect(texture: buff.iconTexture, rectangle: imageRect, color: Color.White * opacity, drawTestRect: false, alignX: Helpers.AlignX.Left);

                        if (buffProgress < 1)
                        {
                            Rectangle progressRect = new Rectangle(x: (int)textPos.X, y: (int)(textPos.Y + buffTextSize.Y) + 2, width: (int)(buffTextSize.X * (1f - buffProgress)), height: 2);
                            Rectangle shadowRect = progressRect;
                            shadowRect.Offset(shadowOffset.X, shadowOffset.Y);

                            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: shadowRect, color: shadowColor * opacity);
                            SonOfRobinGame.SpriteBatch.Draw(texture: SonOfRobinGame.WhiteRectangle, destinationRectangle: progressRect, color: Color.White * opacity);
                        }

                        richTextLayout.IgnoreColorCommand = true; // otherwise shadow would be colored
                        richTextLayout.Draw(SonOfRobinGame.SpriteBatch, textPos + shadowOffset, shadowColor * opacity);
                        richTextLayout.IgnoreColorCommand = false;
                        richTextLayout.Draw(SonOfRobinGame.SpriteBatch, textPos, Color.White * opacity);

                        currentPosY += buffBGRect.Height;
                    }
                }
            }

            // drawing map markers

            if (!this.transManager.HasAnyTransition)
            {
                foreach (var kvp in this.world.ActiveLevel.mapMarkerByColor)
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
                        ImageObj markerImage = markerPiece.sprite.AnimFrame.imageObj;

                        float tipsHeight = 0; // to avoid drawing marker under ControlTips
                        if (Preferences.ShowControlTips)
                        {
                            ControlTips topTips = ControlTips.GetTopTips();
                            if (topTips != null) tipsHeight = (float)topTips.viewParams.Height / topTips.viewParams.ScaleY;
                        }

                        float markerHeight = Preferences.MapMarkerRealSize * this.world.viewParams.ScaleY;
                        float markerScale = markerHeight / markerImage.Height;
                        float markerWidth = markerImage.Width * markerScale;

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

                        markerPos += new Vector2(markerWidth / 2, markerHeight / 2);

                        Vector2 markerScreenPos = this.world.TranslateWorldToScreenPos(markerPos);
                        markerScreenPos.X -= this.viewParams.DrawPos.X;
                        markerScreenPos.Y -= this.viewParams.DrawPos.Y;

                        Vector2 markerPosRightBottom = new(markerPos.X + markerWidth, markerPos.Y + markerHeight);
                        Vector2 markerScreenPosRightBottom = this.world.TranslateWorldToScreenPos(markerPosRightBottom);
                        markerScreenPosRightBottom.X -= this.viewParams.DrawPos.X;
                        markerScreenPosRightBottom.Y -= this.viewParams.DrawPos.Y;

                        float markerDrawScale = (markerScreenPosRightBottom.Y - markerScreenPos.Y) / (float)markerImage.Height;

                        markerPiece.sprite.effectCol.AddEffect(new ColorizeInstance(color: markerColor, priority: 0));
                        markerPiece.sprite.effectCol.TurnOnNextEffect(scene: this, currentUpdateToUse: this.world.CurrentUpdate, drawColor: Color.White);

                        markerPiece.sprite.AnimFrame.Draw(position: markerScreenPos, color: Color.White, rotation: 0f, opacity: markerPiece.sprite.opacity, scale: markerDrawScale);
                    }
                }
            }

            SonOfRobinGame.SpriteBatch.End();
        }
    }
}