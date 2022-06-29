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
        public CraftInvoker(Menu menu, string name, Scheduler.ActionName actionName, Craft.Recipe recipe, PieceStorage storage, Object executeHelper = null, bool closesMenu = false, bool rebuildsMenu = false) : base(menu: menu, name: name, actionName: actionName, executeHelper: executeHelper, closesMenu: closesMenu, rebuildsMenu: rebuildsMenu)
        {
            this.recipe = recipe;
            this.storage = storage;
        }

        public override void Invoke()
        {
            if (recipe.CheckIfStorageContainsAllIngredients(storage)) base.Invoke();
        }

        public override void Draw(bool active)
        {
            bool canBeCrafted = recipe.CheckIfStorageContainsAllIngredients(storage);

            Color notAvailableColor = Color.Red;
            this.GetOpacity(active: active);
            float opacityFade = this.OpacityFade;
            Rectangle outerEntryRect = this.Rect;
            if (active || opacityFade > 0) SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, outerEntryRect, this.rectColor * opacityFade * 2);
            Color txtCol = active ? this.textColor : this.rectColor;

            float innerMargin = outerEntryRect.Height * 0.1f;
            Rectangle innerEntryRect = new Rectangle(outerEntryRect.X + (int)innerMargin, outerEntryRect.Y + (int)innerMargin, outerEntryRect.Width - (int)(innerMargin * 2), outerEntryRect.Height - (int)(innerMargin * 2));

            Helpers.DrawRectangleOutline(rect: outerEntryRect, color: this.outlineColor, borderWidth: 2);

            // making a set of lists to draw

            var pieceNames = new List<PieceTemplate.Name> { };
            var pieceTxts = new List<string> { };
            var pieceColors = new List<Color> { };

            pieceNames.Add(recipe.pieceToCreate);
            pieceTxts.Add($"({this.storage.CountPieceOccurences(recipe.pieceToCreate)})");
            pieceColors.Add(canBeCrafted ? Color.White : notAvailableColor);

            foreach (var kvp in recipe.ingredients)
            {
                var pieceName = kvp.Key;
                var countNeeded = kvp.Value;
                int pieceCount = this.storage.CountPieceOccurences(pieceName);

                pieceNames.Add(pieceName);
                pieceTxts.Add($"{pieceCount}/{countNeeded}");
                pieceColors.Add(pieceCount >= countNeeded ? Color.White : notAvailableColor);
            }

            // calculating rects

            int rectWidth = Math.Min(innerEntryRect.Width / pieceNames.Count, innerEntryRect.Height * 2);

            int margin = innerEntryRect.Width / pieceNames.Count > rectWidth ? (int)innerMargin * 3 : (int)innerMargin;

            rectWidth -= margin;
            AnimFrame frame;
            Rectangle pieceRect;

            // drawing lists

            for (int i = 0; i < pieceNames.Count; i++)
            {
                var pieceName = pieceNames[i];
                var pieceTxt = pieceTxts[i];
                var pieceColor = pieceColors[i];
                frame = Craft.framesToDisplay[pieceName];

                pieceRect = new Rectangle(innerEntryRect.X + ((rectWidth + margin) * i), innerEntryRect.Y, rectWidth, innerEntryRect.Height);

                if (i == 0)  SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, pieceRect, this.textColor * 0.3f * this.menu.viewParams.opacity);

                //Helpers.DrawRectangleOutline(rect: pieceRect, color: Color.YellowGreen, borderWidth: 2); // testing rect size
                this.DrawFrameAndText(frame: frame, cellRect: pieceRect, gfxCol: pieceColor, txtCol: txtCol, text: pieceTxt);
            }
        }

        private void DrawFrameAndText(AnimFrame frame, Rectangle cellRect, string text, Color txtCol, Color gfxCol)
        {
            int middleMargin = (int)(cellRect.Width * 0.05f);

            Rectangle leftRect = new Rectangle(cellRect.X, cellRect.Y, (cellRect.Width / 2) - middleMargin, cellRect.Height);
            Rectangle rightRect = new Rectangle(cellRect.X + (cellRect.Width / 2) + middleMargin, cellRect.Y, (cellRect.Width / 2) - middleMargin, cellRect.Height);

            frame.DrawAndKeepInRectBounds(destBoundsRect: leftRect, color: gfxCol * this.menu.viewParams.opacity);
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

            SonOfRobinGame.spriteBatch.DrawString(font, text, position: textPos, color: txtCol * menu.viewParams.opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

        }


    }
}
