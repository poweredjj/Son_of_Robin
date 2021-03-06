using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Linq;

namespace SonOfRobin
{
    public class PlayerPanel : Scene
    {
        private static readonly SpriteFont itemCounterFont = SonOfRobinGame.fontTommy40;
        private static readonly SpriteFont buffFont = SonOfRobinGame.fontTommy40;
        private static readonly int posY = 4;

        private readonly World world;

        private int CounterSize { get { return (int)(SonOfRobinGame.VirtualWidth * 0.05f); } }
        public Rectangle CounterRect
        {
            get
            {
                int counterSize = this.CounterSize;
                Rectangle counterRect = new Rectangle(x: this.BarWidth + (int)(counterSize * 0.25f), y: posY, width: counterSize, height: counterSize);
                return counterRect;
            }
        }

        public bool IsCounterActivatedByTouch
        {
            get
            {
                var pressTouches = TouchInput.TouchPanelState.Where(touch => touch.State == TouchLocationState.Pressed).ToList();
                if (pressTouches.Count == 0) return false;

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
        private int BarWidth { get { return (int)(SonOfRobinGame.VirtualWidth * 0.27f); } }
        private int BarHeight { get { return (int)(BarWidth * 0.03f); } }
        private int IconWidthHeight { get { return (int)(BarWidth * 0.1f); } }
        private int IconMargin { get { return (int)(BarWidth * 0.03f); } }
        private bool IgnoreUpdateAndDraw
        {
            get
            {
                if (Preferences.debugDisablePlayerPanel) return true;

                Scene menuScene = GetTopSceneOfType(typeof(Menu));
                if (menuScene != null && !menuScene.transManager.HasAnyTransition) return true;

                Scene inventoryScene = GetTopSceneOfType(typeof(Inventory));
                if (inventoryScene != null && ((Inventory)inventoryScene).type != Inventory.Type.SingleBottom && !inventoryScene.transManager.HasAnyTransition) return true;

                return this.world.mapMode == World.MapMode.Big ||
                    this.world.CineMode ||
                    this.world.SpectatorMode ||
                    this.world.player == null;
            }
        }

        public PlayerPanel(World world) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.world = world;
            this.AdaptToNewSize();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.IgnoreUpdateAndDraw) return;
        }

        protected override void AdaptToNewSize()
        {
            this.viewParams.Width = (int)this.BarWidth;
            this.viewParams.CenterView(horizontally: true, vertically: false);
        }

        public override void Draw()
        {
            if (this.IgnoreUpdateAndDraw) return;

            this.AdaptToNewSize();

            Player player = world.player;
            int width = this.BarWidth;
            int height = this.BarHeight;
            int barsHeight;
            int posY = PlayerPanel.posY;

            // drawing stat bars
            {
                int posX = 0;

                int vOffsetCorrection = 4;

                StatBar.ChangeBatchFont(spriteFont: SonOfRobinGame.fontFreeSansBold12);

                new StatBar(width: width, height: height, label: "food", value: (int)player.fedLevel, valueMax: (int)player.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: AnimData.framesForPkgs[AnimData.PkgName.Burger].texture);
                new StatBar(width: width, height: height, label: "fatigue", value: (int)player.Fatigue, valueMax: (int)player.maxFatigue, colorMin: new Color(255, 255, 0), colorMax: new Color(255, 0, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: AnimData.framesForPkgs[AnimData.PkgName.Bed].texture);
                new StatBar(width: width, height: height, label: "stamina", value: (int)player.stamina, valueMax: (int)player.maxStamina, colorMin: new Color(100, 100, 100), colorMax: new Color(255, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: AnimData.framesForPkgs[AnimData.PkgName.Biceps].texture);
                new StatBar(width: width, height: height, label: "health", value: (int)player.hitPoints, valueMax: (int)player.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection, texture: AnimData.framesForPkgs[AnimData.PkgName.Heart].texture);

                barsHeight = StatBar.BatchHeight; // must be invoked before drawing bars

                StatBar.FinishThisBatch();
                StatBar.DrawAll();
            }

            // drawing stored items counter
            {
                int counterSize = this.CounterSize;
                Rectangle counterRect = this.CounterRect;

                AnimData.framesForPkgs[AnimData.PkgName.BackpackMediumOutline].DrawAndKeepInRectBounds(destBoundsRect: counterRect, color: Color.White * this.viewParams.drawOpacity, opacity: 0.85f);

                int occupiedSlotCount = player.pieceStorage.OccupiedSlots.Count + (player.toolStorage.OccupiedSlots.Count - 1); // -1 to count out hand "tool"
                int totalSlotCount = player.pieceStorage.AllSlots.Count + (player.toolStorage.AllSlots.Count - 1); // -1 to count out hand "tool"

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

            // drawing status icons
            {
                int iconPosY = barsHeight + 5;
                int iconPosX = 0;
                int iconWidthHeight = this.IconWidthHeight;
                int margin = this.IconMargin;

                BuffEngine.Buff buff;
                string buffText, topText, bottomText;
                Rectangle textRect, iconRect, progressRect, textRectTop, textRectBottom;
                Color progressColor, bgColor, frameColor, txtColor;
                float buffProgress;
                int buffFramesLeft, bgRectShrink, textMargin;

                foreach (var kvp in player.buffEngine.buffDict)
                {
                    buff = kvp.Value;
                    buffText = buff.iconText;

                    if (buffText == null) continue;

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

                    iconRect = new Rectangle(iconPosX, iconPosY, iconWidthHeight, iconWidthHeight);

                    if (buff.autoRemoveDelay > 0)
                    {
                        buffFramesLeft = Math.Max(buff.endFrame - world.currentUpdate, 0);
                        buffProgress = 1f - ((float)buffFramesLeft / (float)buff.autoRemoveDelay);
                        bgRectShrink = (int)((float)iconWidthHeight * buffProgress);

                        progressRect = new Rectangle(iconRect.X, iconRect.Y + bgRectShrink, iconWidthHeight, iconWidthHeight - bgRectShrink);
                    }
                    else progressRect = iconRect;

                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, iconRect, bgColor * this.viewParams.drawOpacity);
                    SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, progressRect, progressColor * this.viewParams.drawOpacity);

                    textMargin = (int)(iconWidthHeight * 0.1f);
                    textRect = new Rectangle(x: iconRect.X + textMargin, y: iconRect.Y + textMargin, width: iconWidthHeight - (textMargin * 2), height: iconWidthHeight - (textMargin * 2));

                    if (buffText.Contains("\n"))
                    {
                        topText = buffText.Split('\n')[0];
                        bottomText = buffText.Split('\n')[1];

                        textRectTop = new Rectangle(x: textRect.X, y: textRect.Y, width: textRect.Width, height: textRect.Height / 2);
                        textRectBottom = new Rectangle(x: textRect.X, y: textRect.Y + (textRect.Height / 2), width: textRect.Width, height: textRect.Height / 2);

                        Helpers.DrawTextInsideRect(font: buffFont, text: topText, rectangle: textRectTop, color: txtColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
                        Helpers.DrawTextInsideRect(font: buffFont, text: bottomText, rectangle: textRectBottom, color: txtColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
                    }
                    else
                    {
                        Helpers.DrawTextInsideRect(font: buffFont, text: buffText, rectangle: textRect, color: txtColor * this.viewParams.drawOpacity, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center);
                    }

                    Helpers.DrawRectangleOutline(rect: iconRect, color: frameColor * this.viewParams.drawOpacity, borderWidth: 2);

                    iconPosX += iconWidthHeight + margin;
                    if (iconPosX > width)
                    {
                        iconPosY += iconWidthHeight + margin;
                        iconPosX = 0;
                    }
                }
            }
        }

    }
}
