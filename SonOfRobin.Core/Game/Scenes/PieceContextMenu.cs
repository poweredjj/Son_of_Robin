using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceContextMenu : Scene
    {
        protected enum ContextAction { Drop, DropAll, Move, Eat, Plant, Cook }

        private static readonly SpriteFont font = SonOfRobinGame.fontTommy40;
        private static readonly float marginPercent = 0.03f;
        private static readonly float entryWidthPercent = 0.8f;
        private static readonly float entryHeightPercent = 0.1f;

        private readonly BoardPiece piece;
        private readonly PieceStorage storage;
        private readonly StorageSlot slot;
        private readonly List<ContextAction> actionList;
        private readonly float percentPosX, percentPosY;
        private int activeEntry;
        private bool showCursor;

        private int Margin { get { return Convert.ToInt32(SonOfRobinGame.VirtualHeight * marginPercent); } }

        private Vector2 MenuPos
        {
            get
            {
                Vector2 menuPos = new Vector2(this.percentPosX * SonOfRobinGame.VirtualWidth, this.percentPosY * SonOfRobinGame.VirtualHeight);
                Rectangle bgRect = this.BgRect;

                if (menuPos.X + bgRect.Width > SonOfRobinGame.VirtualWidth) menuPos.X = SonOfRobinGame.VirtualWidth - bgRect.Width;
                if (menuPos.Y + bgRect.Height > SonOfRobinGame.VirtualHeight) menuPos.Y = SonOfRobinGame.VirtualHeight - bgRect.Height;

                return menuPos;
            }
        }

        private int ActiveEntry
        {
            get { return this.activeEntry; }
            set
            {
                this.activeEntry = value;
                if (this.activeEntry <= -1) this.activeEntry = this.actionList.Count - 1;
                if (this.activeEntry >= this.actionList.Count) this.activeEntry = 0;
            }
        }

        private ContextAction ActiveAction { get { return this.actionList[this.ActiveEntry]; } }

        private float TextScale
        {
            get
            {
                float maxTextWidth = SonOfRobinGame.VirtualWidth * entryWidthPercent;
                float maxTextHeight = SonOfRobinGame.VirtualHeight * entryHeightPercent;
                Vector2 labelSize;
                float textScale;
                float minScale = 9999f;

                foreach (ContextAction action in this.actionList)
                {
                    labelSize = font.MeasureString(this.GetActionLabel(action));
                    textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);
                    if (textScale < minScale) minScale = textScale;
                }

                if (SonOfRobinGame.platform == Platform.Desktop) minScale *= 0.7f;

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

                foreach (ContextAction action in this.actionList)
                {
                    Vector2 labelSize = font.MeasureString(this.GetActionLabel(action));
                    textWidth = labelSize.X * textScale;
                    textHeight = labelSize.Y * textScale;

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

                int width = (int)maxEntrySize.X + (margin * 2);
                int height = (((int)maxEntrySize.Y + margin) * (this.actionList.Count + 1)) + margin; // piece image is shown as one entry

                return new Rectangle(0, 0, width, height);
            }
        }

        public PieceContextMenu(BoardPiece piece, PieceStorage storage, StorageSlot slot, float percentPosX, float percentPosY, bool addMove = false, bool addDrop = true, bool addCook = false) : base(inputType: InputTypes.Normal, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.PieceContext)
        {
            this.piece = piece;
            this.storage = storage;
            this.slot = slot;
            this.actionList = this.GetContextActionList(addMove: addMove, addDrop: addDrop, addCook: addCook);
            this.percentPosX = percentPosX;
            this.percentPosY = percentPosY;
            this.activeEntry = 0;
            this.showCursor = SonOfRobinGame.platform != Platform.Mobile;

            this.UpdateViewSizes();

            this.transManager.AddMultipleTransitions(outTrans: false, duration: 8, paramsToChange:
                new Dictionary<string, float> { { "PosY", this.viewParams.PosY + SonOfRobinGame.VirtualHeight }, { "Opacity", 0f } });
        }

        private List<ContextAction> GetContextActionList(bool addMove = false, bool addDrop = false, bool addCook = false)
        {
            var contextActionList = new List<ContextAction> { };

            if (this.piece.toolbarTask == Scheduler.TaskName.GetEaten) contextActionList.Add(ContextAction.Eat);
            if (this.piece.GetType() == typeof(Fruit)) contextActionList.Add(ContextAction.Plant);
            if (addMove) contextActionList.Add(ContextAction.Move);
            if (addDrop) contextActionList.Add(ContextAction.Drop);
            if (addCook) contextActionList.Add(ContextAction.Cook);
            if (this.slot.PieceCount > 1) contextActionList.Add(ContextAction.DropAll);

            return contextActionList;
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding)
            {
                this.transManager.AddMultipleTransitions(outTrans: true, duration: 8, endRemoveScene: true, paramsToChange:
                    new Dictionary<string, float> { { "PosY", this.viewParams.PosY + SonOfRobinGame.VirtualHeight }, { "Opacity", 0f } });
                return;
            }

            base.Remove();
        }

        public override void Update(GameTime gameTime)
        {
            this.UpdateViewSizes();
            this.ProcessInput();
        }

        private void UpdateViewSizes()
        {
            Rectangle bgRect = this.BgRect;
            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = bgRect.Height;

            Vector2 menuPos = this.MenuPos;
            this.viewParams.PosX = menuPos.X;
            this.viewParams.PosY = menuPos.Y;
        }

        private void ProcessInput()
        {
            if (Keyboard.HasBeenPressed(Keys.Escape) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B))
            {
                this.Remove();
                return;
            }

            if (Keyboard.HasBeenPressed(key: Keys.W, repeat: true) ||
                Keyboard.HasBeenPressed(key: Keys.Up, repeat: true) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadUp, analogAsDigital: true, repeat: true))
            {
                this.ActiveEntry -= 1;
                this.showCursor = true;
            }

            if (Keyboard.HasBeenPressed(key: Keys.S, repeat: true) ||
                Keyboard.HasBeenPressed(key: Keys.Down, repeat: true) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadDown, analogAsDigital: true, repeat: true))
            {
                this.ActiveEntry += 1;
                this.showCursor = true;
            }

            if (Keyboard.HasBeenPressed(Keys.Enter) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.A))
            {
                this.ExecuteAction();
                this.Remove();
                return;
            }

            this.ProcessTouch();
        }

        private void ProcessTouch()
        {
            if (SonOfRobinGame.platform != Platform.Mobile) return;

            Rectangle bgRect = this.BgRect;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = (touch.Position / Preferences.globalScale) - this.viewParams.DrawPos;

                if (touch.State == TouchLocationState.Released)
                {
                    if (bgRect.Contains(touchPos))
                    {
                        Rectangle entryRect;

                        for (int entryNo = 0; entryNo < this.actionList.Count; entryNo++)
                        {
                            entryRect = this.GetEntryRect(entryNo);
                            if (entryRect.Contains(touchPos))
                            {
                                this.ActiveEntry = entryNo;
                                this.ExecuteAction();
                                this.Remove();
                                return;
                            }
                        }

                    }
                    else
                    {
                        this.Remove();
                        return;
                    }

                }
            }
        }

        private void ExecuteAction()
        {
            ContextAction action = this.ActiveAction;

            switch (action)
            {
                case ContextAction.Drop:
                    this.storage.DropPiecesFromSlot(slot: this.slot, addMovement: true);
                    return;

                case ContextAction.DropAll:
                    this.storage.DropPiecesFromSlot(slot: this.slot, dropAllPieces: true, addMovement: true);
                    return;

                case ContextAction.Move:

                    var invScene = Scene.GetSecondTopSceneOfType(typeof(Inventory));
                    if (invScene == null) return;
                    Inventory secondInventory = (Inventory)invScene;

                    List<BoardPiece> piecesToMove = this.storage.RemoveAllPiecesFromSlot(slot: this.slot);

                    if (piecesToMove.Count == 0) return;
                    bool pieceMoved;

                    foreach (BoardPiece piece in piecesToMove)
                    {
                        pieceMoved = secondInventory.storage.AddPiece(piece);
                        if (!pieceMoved) this.storage.AddPiece(piece);
                    }

                    return;

                case ContextAction.Eat:
                    BoardPiece food = (BoardPiece)this.slot.TopPiece;
                    World world = World.GetTopWorld();

                    var executeHelper = new Dictionary<string, Object> { };
                    executeHelper["player"] = world.player;
                    executeHelper["toolbarPiece"] = food;
                    executeHelper["buttonHeld"] = false;
                    executeHelper["highlightOnly"] = false;

                    new Scheduler.Task(menu: null, taskName: food.toolbarTask, delay: 0, executeHelper: executeHelper);

                    return;

                case ContextAction.Plant:
                    Fruit fruit = (Fruit)this.slot.TopPiece;

                    bool plantedWithSuccess = false;

                    for (int i = 0; i < 35; i += 5)
                    {
                        Sprite.ignoreDensityOverride = true;
                        Sprite.maxDistanceOverride = i;
                        BoardPiece newPlantPiece = PieceTemplate.CreateOnBoard(world: fruit.world, position: fruit.world.player.sprite.position, templateName: fruit.spawnerName);
                        if (newPlantPiece.sprite.placedCorrectly)
                        {
                            Plant newPlant = (Plant)newPlantPiece;
                            newPlant.massTakenMultiplier *= 1.5f; // when the player plants something, it should grow better than normal

                            this.slot.RemoveTopPiece();
                            plantedWithSuccess = true;
                            break;
                        }
                    }

                    if (!plantedWithSuccess) new TextWindow(text: "I cannot plant it here.", textColor: Color.Black, bgColor: Color.White, useTransition: true, animate: true);

                    return;

                case ContextAction.Cook:
                    Cooker cooker = (Cooker)this.storage.storagePiece;
                    cooker.Cook();

                    return;

                default:
                    throw new DivideByZeroException($"Unsupported context action - {action}.");
            }
        }

        private string GetActionLabel(ContextAction action)
        {
            return Helpers.ToSentenceCase(Convert.ToString(action));
        }

        public override void Draw()
        {
            Rectangle bgRect = this.BgRect;
            float margin = this.Margin;
            Vector2 maxEntrySize = this.MaxEntrySize;

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, Color.DodgerBlue * 0.9f * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);

            float textScale = this.TextScale;

            Rectangle pieceRect = new Rectangle(bgRect.X + (int)margin, bgRect.Y + (int)margin, (int)maxEntrySize.Y, (int)maxEntrySize.Y);
            this.piece.sprite.DrawAndKeepInRectBounds(destRect: pieceRect, opacity: viewParams.drawOpacity);

            int displayedPos;
            string actionLabel;
            bool isActive;
            Color textColor;
            Vector2 textPos, shadowPos;
            Rectangle entryRect;
            float shadowOffset = Math.Max(maxEntrySize.Y * 0.05f, 1);
            int entryNo = 0;

            foreach (ContextAction action in this.actionList)
            {
                displayedPos = entryNo + 1;
                isActive = entryNo == this.ActiveEntry && showCursor;
                textColor = isActive ? Color.White : Color.LightSkyBlue;
                actionLabel = this.GetActionLabel(action);

                entryRect = this.GetEntryRect(entryNo);
                textPos = new Vector2(entryRect.X, entryRect.Y);
                shadowPos = new Vector2(textPos.X + shadowOffset, textPos.Y + shadowOffset);

                SonOfRobinGame.spriteBatch.DrawString(font, actionLabel, position: shadowPos, color: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                SonOfRobinGame.spriteBatch.DrawString(font, actionLabel, position: textPos, color: textColor * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                entryNo++;
            }
        }
        private Rectangle GetEntryRect(int entryNo)
        {
            int displayedPos = entryNo + 1;
            int margin = this.Margin;
            Vector2 maxEntrySize = MaxEntrySize;

            return new Rectangle(margin, margin + (int)((float)displayedPos * (this.MaxEntrySize.Y + (float)margin)), (int)maxEntrySize.X, (int)maxEntrySize.Y);
        }


    }
}
