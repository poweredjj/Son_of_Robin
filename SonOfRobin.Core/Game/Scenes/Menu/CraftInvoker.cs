using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    class CraftInvoker : Invoker
    {
        private readonly Craft.Recipe recipe;
        private readonly PieceStorage storage;
        public CraftInvoker(Menu menu, string name, Scheduler.TaskName taskName, Craft.Recipe recipe, PieceStorage storage, Object executeHelper = null, bool closesMenu = false, bool rebuildsMenu = false) : base(menu: menu, name: name, taskName: taskName, executeHelper: executeHelper, closesMenu: closesMenu, rebuildsMenu: rebuildsMenu)
        {
            this.recipe = recipe;
            this.storage = storage;
        }

        public override void Invoke()
        {
            base.Invoke();
        }

        public override void Draw(bool active)
        {
            bool canBeCrafted = recipe.CheckIfStorageContainsAllIngredients(storage);

            if (active)
            {
                this.UpdateInfoText();
                this.UpdateHintWindow();
            }

            this.GetOpacity(active: active);
            float opacityFade = this.OpacityFade;
            Rectangle outerEntryRect = this.Rect;
            if (active || opacityFade > 0) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, outerEntryRect, this.rectColor * opacityFade * 2);

            float innerMargin = outerEntryRect.Height * 0.1f;
            Rectangle innerEntryRect = new Rectangle(outerEntryRect.X + (int)innerMargin, outerEntryRect.Y + (int)innerMargin, outerEntryRect.Width - (int)(innerMargin * 2), outerEntryRect.Height - (int)(innerMargin * 2));

            Helpers.DrawRectangleOutline(rect: outerEntryRect, color: this.outlineColor, borderWidth: 2);

            // making a set of lists to draw

            var pieceNames = new List<PieceTemplate.Name> { };
            var pieceTxts = new List<string> { };
            var pieceCounters = new List<int> { };
            var bgColors = new List<Color> { };

            pieceNames.Add(recipe.pieceToCreate);
            pieceTxts.Add($"own\n{this.storage.CountPieceOccurences(recipe.pieceToCreate)}");
            pieceCounters.Add(recipe.amountToCreate);
            bgColors.Add(canBeCrafted ? Color.Green : Color.Red);

            foreach (var kvp in recipe.ingredients)
            {
                var pieceName = kvp.Key;
                var countNeeded = kvp.Value;
                int pieceCount = this.storage.CountPieceOccurences(pieceName);

                pieceNames.Add(pieceName);
                pieceTxts.Add($"own\n{pieceCount}");
                bgColors.Add(pieceCount >= countNeeded ? Color.White * 0f : Color.Red);
                pieceCounters.Add(countNeeded);
            }

            // calculating rects

            int rectWidth = Math.Min(innerEntryRect.Width / pieceNames.Count, innerEntryRect.Height * 2);

            int margin = innerEntryRect.Width / pieceNames.Count > rectWidth ? (int)innerMargin * 3 : (int)innerMargin;

            rectWidth -= margin;
            AnimFrame frame;
            Rectangle pieceRect;
            Color bgColor;

            // drawing lists

            for (int i = 0; i < pieceNames.Count; i++)
            {
                PieceTemplate.Name pieceName = pieceNames[i];
                string pieceTxt = pieceTxts[i];
                int pieceCounter = pieceCounters[i];
                bgColor = bgColors[i];
                frame = PieceInfo.info[pieceName].frame;

                pieceRect = new Rectangle(innerEntryRect.X + ((rectWidth + margin) * i), innerEntryRect.Y, rectWidth, innerEntryRect.Height);

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, pieceRect, bgColor * 0.4f * this.menu.viewParams.Opacity);

                //Helpers.DrawRectangleOutline(rect: pieceRect, color: Color.YellowGreen, borderWidth: 2); // testing rect size
                this.DrawFrameAndText(frame: frame, cellRect: pieceRect, gfxCol: Color.White, txtCol: Color.White, text: pieceTxt);
                Inventory.DrawQuantity(pieceCount: pieceCounter, destRect: pieceRect, opacity: this.menu.viewParams.drawOpacity, ignoreSingle: false);
            }
        }
        private void UpdateInfoText()
        {
            PieceInfo.Info pieceInfo = PieceInfo.info[recipe.pieceToCreate];
            bool canBeCrafted = recipe.CheckIfStorageContainsAllIngredients(storage);

            var entryList = new List<InfoWindow.TextEntry> {
                new InfoWindow.TextEntry(frame: PieceInfo.info[recipe.pieceToCreate].frame, text: pieceInfo.readableName, color: Color.White, scale: 1.5f),
                new InfoWindow.TextEntry(text: pieceInfo.description, color: Color.White)};

            if (pieceInfo.buffList != null)
            {
                foreach (BuffEngine.Buff buff in pieceInfo.buffList)
                { entryList.Add(new InfoWindow.TextEntry(text: buff.Description, color: Color.Cyan, scale: 1f)); }
            }
            if (!canBeCrafted) entryList.Add(new InfoWindow.TextEntry(text: "Not enough ingredients.", color: Color.OrangeRed, scale: 1f));

            this.infoTextList = entryList;
        }

        private void DrawFrameAndText(AnimFrame frame, Rectangle cellRect, string text, Color txtCol, Color gfxCol)
        {
            int middleMargin = (int)(cellRect.Width * 0.05f);

            Rectangle leftRect = new Rectangle(cellRect.X, cellRect.Y, (cellRect.Width / 2) - middleMargin, cellRect.Height);
            Rectangle rightRect = new Rectangle(cellRect.X + (cellRect.Width / 2) + middleMargin, cellRect.Y, (cellRect.Width / 2) - middleMargin, cellRect.Height);

            frame.DrawAndKeepInRectBounds(destBoundsRect: leftRect, color: gfxCol * this.menu.viewParams.Opacity);
            this.DrawText(text: text, containingRect: rightRect, txtCol: txtCol);
        }

        private void DrawText(string text, Rectangle containingRect, Color txtCol)
        {
            Vector2 textSize = font.MeasureString(text);

            float maxTextHeight = containingRect.Height;
            float maxTextWidth = containingRect.Width;

            float textScale = Math.Min(maxTextWidth / textSize.X, maxTextHeight / textSize.Y);

            Vector2 textPos = new Vector2(
                containingRect.Center.X - (textSize.X / 2 * textScale),
                containingRect.Center.Y - (textSize.Y / 2 * textScale));

            SonOfRobinGame.spriteBatch.DrawString(font, text, position: textPos, color: txtCol * menu.viewParams.Opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

    }
}
