using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    internal class CraftInvoker : Invoker
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
                this.name = PieceTemplate.Name.Empty;
                this.frame = PieceInfo.GetInfo(name).frame;
                this.text = "";
                this.bgColor = Color.White;
                this.counter = 0;
            }
        }

        private readonly Craft.Recipe recipe;
        private readonly List<PieceStorage> storageList;
        private readonly List<InvokerDrawParams> drawParamsList;

        public CraftInvoker(Menu menu, string name, Scheduler.TaskName taskName, Craft.Recipe recipe, List<PieceStorage> storageList, Object executeHelper = null, bool closesMenu = false, bool rebuildsMenu = true) : base(menu: menu, name: name, taskName: taskName, executeHelper: executeHelper, closesMenu: closesMenu, rebuildsMenu: rebuildsMenu, playSound: false)
        {
            this.recipe = recipe;
            this.storageList = storageList;
            this.drawParamsList = new List<InvokerDrawParams>();

            bool canBeCrafted = recipe.CheckIfStorageContainsAllIngredients(storageList);

            int mainPieceOwnedCount = PieceInfo.GetInfo(recipe.pieceToCreate).canBePickedUp ?
                PieceStorage.CountPieceOccurencesInMultipleStorages(storageList: this.storageList, pieceName: recipe.pieceToCreate) :
                this.storageList[0].world.pieceCountByName[recipe.pieceToCreate];

            this.drawParamsList.Add(new InvokerDrawParams(
                isMain: true,
                name: recipe.pieceToCreate,
                text: $"own\n{mainPieceOwnedCount}",
                counter: recipe.amountToCreate,
                bgColor: canBeCrafted ? Color.Green : Color.Red));

            this.drawParamsList.Add(new InvokerDrawParams(isEmpty: true));

            foreach (var kvp in this.recipe.ingredients)
            {
                var pieceName = kvp.Key;
                var countNeeded = kvp.Value;
                int pieceCount = PieceStorage.CountPieceOccurencesInMultipleStorages(storageList: this.storageList, pieceName: pieceName);

                this.drawParamsList.Add(new InvokerDrawParams(
                    isMain: false,
                    name: pieceName,
                    text: $"own\n{pieceCount}",
                    counter: countNeeded,
                    bgColor: pieceCount >= countNeeded ? Color.Blue * 0.8f : Color.Red));
            }
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
            if (active || opacityFade > 0) SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, outerEntryRect, this.rectColor * opacityFade * 2);

            float innerMargin = outerEntryRect.Height * 0.1f;
            Rectangle innerEntryRect = new Rectangle(outerEntryRect.X + (int)innerMargin, outerEntryRect.Y + (int)innerMargin, outerEntryRect.Width - (int)(innerMargin * 2), outerEntryRect.Height - (int)(innerMargin * 2));

            Helpers.DrawRectangleOutline(rect: outerEntryRect, color: this.outlineColor, borderWidth: 2);

            // making a set of draw data structs

            // calculating rects
            int rectWidth = (int)(Math.Min(innerEntryRect.Width / ((float)this.drawParamsList.Count - 0.5f), innerEntryRect.Height * 2));
            int margin = innerEntryRect.Width / this.drawParamsList.Count > rectWidth ? (int)innerMargin * 3 : (int)innerMargin;

            rectWidth -= margin;
            Rectangle pieceRect;
            float mainPieceSizeMultiplier = 0.3f;
            float bgColorOpacity;
            int mainPiecePixelsToAddWidth, mainPiecePixelsToAddHeight;

            // drawing entries

            int rectX = 0;

            for (int i = 0; i < this.drawParamsList.Count; i++)
            {
                InvokerDrawParams drawParams = this.drawParamsList[i];
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

                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, pieceRect, drawParams.bgColor * bgColorOpacity * this.menu.viewParams.drawOpacity);

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
                    Texture2D newIconTexture = TextureBank.GetTexture(TextureBank.TextureName.New);

                    Helpers.DrawTextureInsideRect(texture: newIconTexture, rectangle: newRect, color: Color.White * this.menu.viewParams.Opacity, alignX: Helpers.AlignX.Left, alignY: Helpers.AlignY.Top);
                }
            }
        }

        private void UpdateInfoText()
        {
            World world = World.GetTopWorld();

            PieceInfo.Info pieceInfo = PieceInfo.GetInfo(this.recipe.pieceToCreate);

            bool canBeCrafted = this.recipe.CheckIfStorageContainsAllIngredients(storageList);

            var entryList = new List<InfoWindow.TextEntry> {
                new InfoWindow.TextEntry(imageList: new List<Texture2D> { PieceInfo.GetInfo(this.recipe.pieceToCreate).texture }, text:$"|  {pieceInfo.readableName}" , color: Color.White, scale: 1.5f, animate: true, charsPerFrame: 2),
                new InfoWindow.TextEntry(text: pieceInfo.description, color: Color.White, animate: true, charsPerFrame: 2)};

            if (pieceInfo.buffList != null)
            {
                foreach (Buff buff in pieceInfo.buffList)
                {
                    entryList.Add(new InfoWindow.TextEntry(text: (buff.iconTexture != null ? "| " : "") + buff.description, imageList: buff.iconTexture != null ? new List<Texture2D> { buff.iconTexture } : null, color: buff.isPositive ? Color.Cyan : new Color(255, 120, 70), scale: 1f, animate: true, charsPerFrame: 2, minMarkerWidthMultiplier: 2f, imageAlignX: Helpers.AlignX.Center));
                }
            }

            float smallScale = 0.7f;

            var extInfoTextList = new List<string>();
            var extInfoImageList = new List<Texture2D>();

            var durabilityTypeList = new List<Type> { typeof(Tool), typeof(PortableLight), typeof(Projectile) };
            if (durabilityTypeList.Contains(pieceInfo.type))
            {
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleHeart));

                if (pieceInfo.toolIndestructible)
                {
                    extInfoTextList.Add($"|  |");
                    extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleInfinity));
                }
                else extInfoTextList.Add($"| {Math.Round(pieceInfo.maxHitPoints)}");
            }

            if (pieceInfo.toolRange > 0)
            {
                extInfoTextList.Add($"| {pieceInfo.toolRange}");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleArea));
            }

            if (pieceInfo.cookerFoodMassMultiplier > 0)
            {
                extInfoTextList.Add($"| x{pieceInfo.cookerFoodMassMultiplier}");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleBurger));
            }

            if (pieceInfo.fertileGroundSoilWealthMultiplier > 0)
            {
                extInfoTextList.Add($"| x{pieceInfo.fertileGroundSoilWealthMultiplier}");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleSapling));
            }

            float fatigue = (int)(recipe.GetRealFatigue(craftStats: world.craftStats, player: world.Player) / world.Player.maxFatigue * 100);
            fatigue = world.Player.GetFinalFatigueValue(fatigue);

            extInfoTextList.Add($"| {(int)fatigue}%");
            extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleSleep));

            TimeSpan duration = IslandClock.ConvertUpdatesCountToTimeSpan(recipe.GetRealDuration(craftStats: world.craftStats, player: world.Player));
            bool endsAtNight = duration >= world.islandClock.TimeUntilPartOfDay(IslandClock.PartOfDay.Night);
            string durationString = string.Format("| {0:D1}:{1:D2}", (int)Math.Floor(duration.TotalHours), duration.Minutes);
            if (endsAtNight) durationString = $"{durationString} | |";
            extInfoTextList.Add(durationString);
            extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleHourglass));
            if (endsAtNight)
            {
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleArrowRight));
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleMoon));
            }

            entryList.Add(new InfoWindow.TextEntry(text: String.Join("  ", extInfoTextList), imageList: extInfoImageList, scale: smallScale, color: new Color(230, 230, 230)));

            var affinityEntries = PieceInfo.GetCategoryAffinityTextEntryList(pieceName: this.recipe.pieceToCreate, scale: smallScale);
            entryList.AddRange(affinityEntries);

            if (pieceInfo.type == typeof(Projectile))
            {
                entryList.Add(new InfoWindow.TextEntry(text: $"| {pieceInfo.projectileHitMultiplier}", imageList: new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.Biceps) }, scale: smallScale, color: Color.White));
            }

            if (!canBeCrafted)
            {
                var missingPiecesDict = PieceStorage.CheckMultipleStoragesForSpecifiedPieces(storageList: this.storageList, quantityByPiece: this.recipe.ingredients);

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

            int recipeLevelCurrent = world.craftStats.GetRecipeLevel(this.recipe);
            int recipeLevelMax = this.recipe.maxLevel;

            if (recipeLevelCurrent == recipeLevelMax)
            {
                entryList.Add(new InfoWindow.TextEntry(text: "Level master |", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Star].texture }, color: Color.Gold, scale: 1f, animate: true, charsPerFrame: 1));
            }
            else entryList.Add(new InfoWindow.TextEntry(text: $"Level {recipeLevelCurrent + 1}/{recipeLevelMax + 1}", color: Color.GreenYellow, scale: 1f, animate: true, charsPerFrame: 1));

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
            Vector2 textSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: text);

            float maxTextHeight = containingRect.Height;
            float maxTextWidth = containingRect.Width;

            float textScale = Math.Min(maxTextWidth / textSize.X, maxTextHeight / textSize.Y);

            Vector2 textPos = new Vector2(
                containingRect.Center.X - (textSize.X / 2 * textScale),
                containingRect.Center.Y - (textSize.Y / 2 * textScale));

            font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: text, position: textPos, color: txtCol * menu.viewParams.Opacity, scale: new Vector2(textScale));
        }
    }
}