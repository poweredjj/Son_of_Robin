using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    class CraftInvoker : Invoker
    {
        private struct InvokerDrawParams
        {
            public readonly bool isMain;
            public readonly bool isEmpty;
            public readonly PieceTemplate.Name name;
            public readonly AnimFrame frame;
            public readonly string text;
            public readonly Color bgColor;
            public readonly int counter;

            public InvokerDrawParams(bool isMain, PieceTemplate.Name name, string text, Color bgColor, int counter, bool isEmpty = false)
            {
                this.isMain = isMain;
                this.isEmpty = isEmpty;
                this.name = name;
                this.text = text;
                this.frame = PieceInfo.GetInfo(name).frame;
                this.bgColor = bgColor;
                this.counter = counter;
            }

            public InvokerDrawParams(bool isEmpty)
            {
                // isEmpty is not really used - this constructor is for making an "empty" entry
                this.isMain = false;
                this.isEmpty = true;
                this.name = PieceTemplate.Name.Player;
                this.frame = PieceInfo.GetInfo(name).frame;
                this.text = "";
                this.bgColor = Color.White;
                this.counter = 0;
            }
        }

        private readonly Craft.Recipe recipe;
        private readonly List<PieceStorage> storageList;
        public CraftInvoker(Menu menu, string name, Scheduler.TaskName taskName, Craft.Recipe recipe, List<PieceStorage> storageList, Object executeHelper = null, bool closesMenu = false, bool rebuildsMenu = false) : base(menu: menu, name: name, taskName: taskName, executeHelper: executeHelper, closesMenu: closesMenu, rebuildsMenu: rebuildsMenu, playSound: false)
        {
            this.recipe = recipe;
            this.storageList = storageList;
        }

        public override void Invoke()
        {
            base.Invoke();
        }

        public override void Draw(bool active, string textOverride = null, List<Texture2D> imageList = null)
        {
            bool canBeCrafted = recipe.CheckIfStorageContainsAllIngredients(storageList);
            bool hasBeenCrafted = World.GetTopWorld().craftStats.HasBeenCrafted(recipe);

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

            // making a set of draw data structs

            var drawParamsList = new List<InvokerDrawParams> { };

            drawParamsList.Add(new InvokerDrawParams(
                isMain: true,
                name: recipe.pieceToCreate,
                text: $"own\n{ PieceStorage.CountPieceOccurencesInMultipleStorages(storageList: this.storageList, pieceName: recipe.pieceToCreate)}",
                counter: recipe.amountToCreate,
                bgColor: canBeCrafted ? Color.Green : Color.Red));

            drawParamsList.Add(new InvokerDrawParams(isEmpty: true));

            foreach (var kvp in recipe.ingredients)
            {
                var pieceName = kvp.Key;
                var countNeeded = kvp.Value;
                int pieceCount = PieceStorage.CountPieceOccurencesInMultipleStorages(storageList: this.storageList, pieceName: pieceName);

                drawParamsList.Add(new InvokerDrawParams(
                    isMain: false,
                    name: pieceName,
                    text: $"own\n{pieceCount}",
                    counter: countNeeded,
                    bgColor: pieceCount >= countNeeded ? Color.Blue * 0.8f : Color.Red));
            }

            // calculating rects
            int rectWidth = (int)(Math.Min(innerEntryRect.Width / ((float)drawParamsList.Count - 0.5f), innerEntryRect.Height * 2));
            int margin = innerEntryRect.Width / drawParamsList.Count > rectWidth ? (int)innerMargin * 3 : (int)innerMargin;

            rectWidth -= margin;
            Rectangle pieceRect;
            float mainPieceSizeMultiplier = 0.3f;
            float bgColorOpacity;
            int mainPiecePixelsToAddWidth, mainPiecePixelsToAddHeight;

            // drawing entries

            int rectX = 0;

            for (int i = 0; i < drawParamsList.Count; i++)
            {
                InvokerDrawParams drawParams = drawParamsList[i];
                if (drawParams.isEmpty)
                {
                    rectX += (int)(rectWidth * 0.5f);
                    continue;
                }

                pieceRect = new Rectangle(innerEntryRect.X + rectX, innerEntryRect.Y, rectWidth, innerEntryRect.Height);

                if (drawParams.isMain)
                {
                    if (active)
                    {
                        mainPiecePixelsToAddWidth = (int)(pieceRect.Width * mainPieceSizeMultiplier);
                        mainPiecePixelsToAddHeight = (int)(pieceRect.Width * mainPieceSizeMultiplier);

                        pieceRect.Width += mainPiecePixelsToAddWidth;
                        pieceRect.Height += mainPiecePixelsToAddHeight;
                        pieceRect.X -= mainPiecePixelsToAddWidth / 2;
                        pieceRect.Y -= mainPiecePixelsToAddHeight / 2;
                    }

                    Helpers.DrawRectangleOutline(rect: pieceRect, color: Color.White * this.menu.viewParams.drawOpacity, borderWidth: 2);
                }

                bgColorOpacity = drawParams.isMain ? 0.7f : 0.35f;

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, pieceRect, drawParams.bgColor * bgColorOpacity * this.menu.viewParams.drawOpacity);

                //Helpers.DrawRectangleOutline(rect: pieceRect, color: Color.YellowGreen, borderWidth: 2); // testing rect size
                this.DrawFrameAndText(frame: drawParams.frame, cellRect: pieceRect, gfxCol: Color.White, txtCol: Color.White, text: drawParams.text);

                Rectangle quantityRect = new Rectangle(x: pieceRect.X, y: pieceRect.Y + (pieceRect.Height / 2), width: (int)(pieceRect.Width * 0.5f), height: pieceRect.Height / 2);
                Inventory.DrawQuantity(pieceCount: drawParams.counter, destRect: quantityRect, opacity: this.menu.viewParams.drawOpacity, ignoreSingle: false);

                rectX += rectWidth + margin;

                // drawing "new" icon
                if (!hasBeenCrafted)
                {
                    int rectSize = (int)(outerEntryRect.Height * 0.6f);
                    int rectOffset = (int)(rectSize * 0.2f);

                    Rectangle newRect = new Rectangle(x: outerEntryRect.X - rectOffset, y: outerEntryRect.Y - rectOffset, width: (int)(outerEntryRect.Height * 0.6f), height: (int)(outerEntryRect.Height * 0.6f));
                    Texture2D newIconTexture = AnimData.framesForPkgs[AnimData.PkgName.NewIcon].texture;

                    Helpers.DrawTextureInsideRect(texture: newIconTexture, rectangle: newRect, color: Color.White * this.menu.viewParams.Opacity, alignX: Helpers.AlignX.Left, alignY: Helpers.AlignY.Top);
                }
            }
        }
        private void UpdateInfoText()
        {
            PieceInfo.Info pieceInfo = PieceInfo.GetInfo(recipe.pieceToCreate);

            bool canBeCrafted = recipe.CheckIfStorageContainsAllIngredients(storageList);

            var entryList = new List<InfoWindow.TextEntry> {
                new InfoWindow.TextEntry(imageList: new List<Texture2D> { PieceInfo.GetInfo(recipe.pieceToCreate).texture }, text:$"|  {pieceInfo.readableName}" , color: Color.White, scale: 1.5f, animate: true, charsPerFrame: 2),
                new InfoWindow.TextEntry(text: pieceInfo.description, color: Color.White, animate: true, charsPerFrame: 2)};

            if (pieceInfo.buffList != null)
            {
                foreach (BuffEngine.Buff buff in pieceInfo.buffList)
                { entryList.Add(new InfoWindow.TextEntry(text: buff.description, color: Color.Cyan, scale: 1f, animate: true, charsPerFrame: 2)); }
            }
            if (!canBeCrafted)
            {
                var missingPiecesDict = PieceStorage.CheckMultipleStoragesForSpecifiedPieces(storageList: this.storageList, quantityByPiece: recipe.ingredients);

                var ingredientsTextLines = new List<string>();
                var missingIngredientsImages = new List<Texture2D>();

                foreach (var kvp in missingPiecesDict)
                {
                    PieceInfo.Info missingPieceInfo = PieceInfo.GetInfo(kvp.Key);
                    ingredientsTextLines.Add($"|  {missingPieceInfo.readableName} x{kvp.Value}");
                    missingIngredientsImages.Add(missingPieceInfo.texture);
                }

                entryList.Add(new InfoWindow.TextEntry(text: "Missing ingredients:\n" + String.Join("\n", ingredientsTextLines), color: Color.DarkOrange, scale: 1f, imageList: missingIngredientsImages, animate: true, charsPerFrame: 2));
            }

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
