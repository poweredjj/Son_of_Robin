using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SonOfRobin
{
    public class PlayerPanel : Scene
    {
        World world;

        private static readonly SpriteFont font = SonOfRobinGame.fontHuge;

        private int BarWidth { get { return (int)(SonOfRobinGame.VirtualWidth * 0.35f); } }
        private int BarHeight { get { return (int)(BarWidth * 0.03f); } }
        private int IconWidthHeight { get { return (int)(BarWidth * 0.1f); } }
        private int IconMargin { get { return (int)(BarWidth * 0.03f); } }

        private bool IgnoreUpdateAndDraw
        { get { return this.world.mapMode == World.MapMode.Big || this.world.player == null; } }


        public PlayerPanel(World world) : base(inputType: InputTypes.None, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.world = world;

        }

        public override void Update(GameTime gameTime)
        {
            if (this.IgnoreUpdateAndDraw) return;

            this.viewParams.width = (int)this.BarWidth;
            this.viewParams.CenterView(horizontally: true, vertically: false);
        }

        public override void Draw()
        {
            if (this.IgnoreUpdateAndDraw) return;

            Player player = world.player;
            int width = this.BarWidth;
            int height = this.BarHeight;

            // drawing stat bars

            int posX = 0;
            int posY = 4;

            int vOffsetCorrection = 3;

            StatBar.ChangeBatchFont(spriteFont: SonOfRobinGame.fontMedium);

            new StatBar(width: width, height: height, label: "food", value: (int)player.fedLevel, valueMax: (int)player.maxFedLevel, colorMin: new Color(0, 128, 255), colorMax: new Color(0, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection);
            new StatBar(width: width, height: height, label: "fatigue", value: (int)player.Fatigue, valueMax: (int)player.maxFatigue, colorMin: new Color(255, 255, 0), colorMax: new Color(255, 0, 0), posX: posX, posY: posY, ignoreIfAtMax: false, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection);
            new StatBar(width: width, height: height, label: "stamina", value: (int)player.stamina, valueMax: (int)player.maxStamina, colorMin: new Color(100, 100, 100), colorMax: new Color(255, 255, 255), posX: posX, posY: posY, ignoreIfAtMax: true, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection);
            new StatBar(width: width, height: height, label: "health", value: (int)player.hitPoints, valueMax: (int)player.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0), posX: posX, posY: posY, ignoreIfAtMax: true, centerX: false, drawFromTop: true, labelAtLeft: true, vOffsetCorrection: vOffsetCorrection);

            int barsHeight = StatBar.BatchHeight; // must be invoked before drawing bars

            StatBar.FinishThisBatch();
            StatBar.DrawAll();

            // drawing status icons

            int iconPosY = barsHeight + 5;
            int iconPosX = 0;
            int iconWidthHeight = this.IconWidthHeight;
            int margin = this.IconMargin;

            BuffEngine.Buff buff;
            string buffText;
            Color progressColor, bgColor, frameColor, txtColor;
            Rectangle iconRect, progressRect;
            Vector2 txtPos, txtSize;
            float txtScale, txtOffsetX, txtOffsetY, txtScaleX, txtScaleY, buffProgress;
            int buffFramesLeft, bgRectShrink;

            foreach (var kvp in player.buffEngine.buffDict)
            {
                buff = kvp.Value;
                buffText = buff.IconText;

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

                txtSize = font.MeasureString(buff.IconText);

                txtScaleX = iconWidthHeight / txtSize.X;
                txtScaleY = iconWidthHeight / txtSize.Y;
                txtScale = Math.Min(txtScaleX, txtScaleY) * 0.8f;
                txtSize *= txtScale;

                txtOffsetX = (iconWidthHeight / 2) - (txtSize.X / 2);
                txtOffsetY = (iconWidthHeight / 2) - (txtSize.Y / 2);
                txtPos = new Vector2(iconRect.X + txtOffsetX, iconRect.Y + txtOffsetY);

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, iconRect, bgColor * this.viewParams.drawOpacity);
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, progressRect, progressColor * this.viewParams.drawOpacity);
                SonOfRobinGame.spriteBatch.DrawString(font, buff.IconText, position: txtPos, color: txtColor * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: txtScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

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
