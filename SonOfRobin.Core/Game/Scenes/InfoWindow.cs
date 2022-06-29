using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class InfoWindow : Scene
    {
        public struct TextEntry
        {
            public readonly Color color;
            public readonly string text;
            public readonly float scale;
            public readonly int sizeX;
            public readonly int sizeY;
            public readonly AnimFrame frame;
            public TextEntry(Color color, string text, float scale = 1f, AnimFrame frame = null)
            {
                this.color = color;
                this.text = text;
                this.scale = scale;
                this.frame = frame;
                Vector2 entrySize = font.MeasureString(this.text) * this.scale;
                this.sizeX = (int)entrySize.X;
                this.sizeY = (int)entrySize.Y;
            }

            private bool Equals(TextEntry entryToCompare)
            {
                return entryToCompare.text == this.text && entryToCompare.color == this.color && entryToCompare.scale == this.scale && entryToCompare.frame == this.frame;
            }

            public static bool CompareTwoLists(List<TextEntry> list1, List<TextEntry> list2)
            {
                if (list1.Count != list2.Count) return false;

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!list1[i].Equals(list2[i])) return false;
                }

                return true;
            }
        }

        private static readonly SpriteFont font = SonOfRobinGame.fontHuge;
        private static readonly float marginPercent = 0.02f;
        private static readonly float entryWidthPercent = 0.3f;
        private static readonly float entryHeightPercent = 0.045f;

        private List<TextEntry> entryList;
        public bool isActive;

        private int Margin { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * marginPercent); } }
        private float TextScale
        {
            get
            {
                float textScale;
                float minScale = 9999f;
                float maxTextWidth = SonOfRobinGame.VirtualWidth * entryWidthPercent;
                float maxTextHeight = SonOfRobinGame.VirtualHeight * entryHeightPercent;

                foreach (TextEntry entry in this.entryList)
                {
                    textScale = Math.Min(maxTextWidth / entry.sizeX, maxTextHeight / entry.sizeY);
                    if (textScale < minScale) minScale = textScale;
                }

                if (SonOfRobinGame.platform == Platform.Mobile) minScale *= 1.5f;

                return minScale;
            }
        }

        private Vector2 MaxEntrySize
        {
            get
            {
                float maxEncounteredWidth = 0f;
                float maxEncounteredHeight = 0f;
                float textScale = this.TextScale;
                float textWidth, textHeight;

                foreach (TextEntry entry in this.entryList)
                {
                    textWidth = entry.sizeX * textScale;
                    textHeight = entry.sizeY * textScale;

                    if (entry.frame != null) textWidth += Margin + textHeight;

                    if (textWidth > maxEncounteredWidth) maxEncounteredWidth = textWidth;
                    if (textHeight > maxEncounteredHeight) maxEncounteredHeight = textHeight;
                }

                return new Vector2(maxEncounteredWidth, maxEncounteredHeight);
            }
        }

        private Rectangle BgRect
        {
            get
            {
                int margin = this.Margin;
                Vector2 maxEntrySize = this.MaxEntrySize;
                float textScale = this.TextScale;

                int width = (int)(maxEntrySize.X + (margin * 2));
                int height = margin;

                foreach (TextEntry entry in this.entryList)
                {
                    height += (int)((entry.sizeY * textScale) + margin);
                }

                return new Rectangle(0, 0, width, height);
            }
        }

        public InfoWindow() : base(inputType: InputTypes.None, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: true, alwaysDraws: true, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.isActive = false;
            this.viewParams.opacity = 0f;
            this.entryList = new List<TextEntry> { };
        }

        public static InfoWindow GetTopInfoWindow()
        {
            InfoWindow infoWindow;
            var infoScene = GetTopSceneOfType(typeof(InfoWindow));
            if (infoScene == null) return null;
            infoWindow = (InfoWindow)infoScene;
            return infoWindow;
        }

        public static void TurnOffTopWindow()
        {
            // a "shortcut" for easy turning off
            InfoWindow infoWindow = GetTopInfoWindow();
            if (infoWindow == null) return;
            infoWindow.TurnOff();
        }

        public Vector2 MeasureEntries(List<TextEntry> entryList)
        {
            List<TextEntry> savedEntryList = this.entryList;
            this.entryList = entryList;
            this.UpdateViewSizes();
            Vector2 windowSize = new Vector2(this.viewParams.width, this.viewParams.height);

            this.entryList = savedEntryList;

            return windowSize;
        }

        public void TurnOn(List<TextEntry> entryList, int newPosX, int newPosY, bool addTransition = true)
        {
            if (entryList == null) return;
            if (this.isActive && this.viewParams.posX == newPosX && this.viewParams.posY == newPosY && TextEntry.CompareTwoLists(this.entryList, entryList)) return;

            //MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"isActive {this.isActive} entry '{entryList[0].text} {SonOfRobinGame.currentUpdate}'");

            this.entryList = entryList;

            var paramsToChange = new Dictionary<string, float> { };

            if (this.isActive)
            {
                if (newPosX != this.viewParams.posX) paramsToChange["posX"] = this.viewParams.posX;
                if (newPosY != this.viewParams.posY) paramsToChange["posY"] = this.viewParams.posY;
            }
            else
            {
                paramsToChange["opacity"] = 0f;
            }

            this.viewParams.posX = newPosX;
            this.viewParams.posY = newPosY;

            this.UpdateViewSizes();

            if (addTransition) this.AddTransition(new Transition(type: Transition.TransType.From, duration: 6, scene: this, blockInput: false, paramsToChange: paramsToChange));

            this.isActive = true;
            this.viewParams.opacity = 1f;
        }


        public void TurnOff(bool addTransition = true)
        {
            if (!this.isActive) return;

            if (addTransition) this.AddTransition(new Transition(type: Transition.TransType.To, duration: 6, scene: this, blockInput: false, copyToBaseAtTheEnd: true, paramsToChange: new Dictionary<string, float> { { "opacity", 0f } }));
            else this.viewParams.opacity = 0f;

            this.isActive = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!this.isActive) return;

            this.UpdateViewSizes();
        }

        public void UpdateViewSizes()
        {
            Rectangle bgRect = this.BgRect;
            this.viewParams.width = bgRect.Width;
            this.viewParams.height = bgRect.Height;
        }

        public override void Draw()
        {
            Rectangle bgRect = this.BgRect;
            int margin = this.Margin;

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, Color.RoyalBlue * 0.9f * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);

            float textScale = this.TextScale;

            Vector2 basePos, textPos, shadowPos;
            float shadowOffset = Math.Max(4f * textScale, 1);

            int posX = margin;
            int posY = margin;

            Vector2 maxEntrySize = this.MaxEntrySize;

            foreach (TextEntry entry in this.entryList)
            {
                basePos = new Vector2(posX, posY);
                textPos = basePos;

                if (entry.frame != null)
                {
                    Rectangle gfxRect = new Rectangle((int)basePos.X, (int)basePos.Y, (int)maxEntrySize.Y, (int)maxEntrySize.Y);
                    entry.frame.DrawAndKeepInRectBounds(destBoundsRect: gfxRect, color: Color.White * this.viewParams.drawOpacity);

                    textPos.X += (int)maxEntrySize.Y + margin;
                }

                shadowPos = new Vector2(textPos.X + shadowOffset, textPos.Y + shadowOffset);

                SonOfRobinGame.spriteBatch.DrawString(font, entry.text, position: shadowPos, color: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, origin: Vector2.Zero, scale: textScale * entry.scale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                SonOfRobinGame.spriteBatch.DrawString(font, entry.text, position: textPos, color: entry.color * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale * entry.scale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                posY += (int)((entry.sizeY * textScale) + margin);
            }
        }

    }
}
