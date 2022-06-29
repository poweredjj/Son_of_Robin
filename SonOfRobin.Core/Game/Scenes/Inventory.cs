using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Inventory : Scene
    {
        public enum Layout { SingleCenter, SingleBottom, DualLeft, DualRight, DualTop, DualBottom }

        private static readonly int minFramesToDragByTouch = 4;
        private static readonly float marginPercent = 0.05f;
        private static readonly SpriteFont font = SonOfRobinGame.fontHuge;

        public readonly string label;
        public readonly Layout layout;
        private readonly BoardPiece piece;
        public readonly PieceStorage storage;
        private int cursorX;
        private int cursorY;

        public List<BoardPiece> draggedPieces;
        private int touchHeldFrames;
        private bool draggedByTouch;
        private bool showEmptyCursor;

        public Inventory otherInventory;

        private bool IgnoreUpdateAndDraw
        {
            get
            {
                Scene topScene = Scene.sceneStack[sceneStack.Count - 1];
                return this.layout == Layout.SingleBottom && topScene.GetType() != typeof(Inventory);
            }
        }

        private int CursorX
        {
            get { return this.cursorX; }
            set
            {
                this.cursorX = value;
                this.KeepCursorInBoundsAndSwitchInv();
            }
        }

        private int CursorY
        {
            get { return this.cursorY; }
            set
            {
                this.cursorY = value;
                this.KeepCursorInBoundsAndSwitchInv();
            }
        }

        private int Margin
        {
            get
            {
                int margin = Convert.ToInt32(Math.Min(SonOfRobinGame.VirtualWidth * marginPercent, SonOfRobinGame.VirtualHeight * marginPercent));
                if (this.layout != Layout.SingleCenter) margin /= 3;
                return margin;
            }
        }
        private float BgMaxWidth
        {
            get
            {
                return this.layout == Layout.SingleCenter || this.layout == Layout.DualTop ? SonOfRobinGame.VirtualWidth * 0.8f : SonOfRobinGame.VirtualWidth * 0.37f;
            }
        }
        private float BgMaxHeight
        {
            get
            {
                if (this.layout == Layout.SingleBottom || this.layout == Layout.DualBottom) return SonOfRobinGame.VirtualHeight * 0.25f;
                if (this.layout == Layout.DualTop) return SonOfRobinGame.VirtualHeight * 0.5f;

                return SonOfRobinGame.VirtualHeight * 0.75f;
            }
        }
        public StorageSlot ActiveSlot { get { return this.storage.GetSlot((byte)this.CursorX, (byte)this.CursorY); } }

        private int TileSize
        {
            get
            {
                int margin = this.Margin;
                float maxTileWidth = (BgMaxWidth / this.storage.width) - margin;
                float maxTileHeight = (BgMaxHeight / this.storage.height) - margin;
                return Convert.ToInt32(Math.Min(maxTileWidth, maxTileHeight));
            }
        }

        private Rectangle BgRect
        {
            get
            {
                int margin = this.Margin;
                int tileSize = this.TileSize;

                int width = ((tileSize + margin) * this.storage.width) + margin;
                int height = ((tileSize + margin) * this.storage.height) + margin;

                return new Rectangle(0, 0, width, height);
            }
        }

        public Inventory(PieceStorage storage, BoardPiece piece, string label, Layout layout = Layout.SingleCenter, bool blocksUpdatesBelow = false, Inventory otherInventory = null, InputTypes inputType = InputTypes.Normal) : base(inputType: inputType, priority: 1, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Inventory)
        {
            this.storage = storage;
            this.piece = piece;
            this.label = label;
            this.layout = layout;

            this.cursorX = 0;
            this.cursorY = SonOfRobinGame.platform == Platform.Mobile && this.layout != Layout.SingleBottom ? -1 : 0;

            this.draggedPieces = new List<BoardPiece> { };
            this.touchHeldFrames = 0;
            this.draggedByTouch = false;
            this.showEmptyCursor = SonOfRobinGame.platform != Platform.Mobile;
            this.otherInventory = otherInventory;

            this.UpdateViewParams();

            if (this.piece.GetType() == typeof(Container))
            {
                var container = (Container)piece;
                container.Open();
            }

            if (this.layout != Layout.SingleBottom && this.layout != Layout.DualBottom)
            {
                var paramsToChange = new Dictionary<string, float> { { "posY", this.viewParams.posY + SonOfRobinGame.VirtualHeight }, { "opacity", 0f } };
                this.AddTransition(new Transition(type: Transition.TransType.In, duration: 12, scene: this, blockInput: false, paramsToChange: paramsToChange));
            }

        }

        public override void Remove()
        {
            this.ReleaseHeldPieces(slot: null, forceReleaseAll: true);

            if (this.transition == null && this.layout != Layout.SingleBottom && this.layout != Layout.DualBottom)
            {
                if (this.piece.GetType() == typeof(Container))
                {
                    var container = (Container)piece;
                    container.Close();
                }

                var paramsToChange = new Dictionary<string, float> { { "posY", this.viewParams.posY + SonOfRobinGame.VirtualHeight }, { "opacity", 0f } };
                this.AddTransition(new Transition(type: Transition.TransType.Out, duration: 12, scene: this, blockInput: true, paramsToChange: paramsToChange, removeScene: true));

                return;
            }

            base.Remove();
        }

        public override void Update(GameTime gameTime)
        {
            if (this.IgnoreUpdateAndDraw) return;

            this.UpdateViewParams();
            this.ProcessInput();
        }

        private void UpdateViewParams()
        {
            Rectangle bgRect = this.BgRect;
            this.viewParams.width = bgRect.Width;
            this.viewParams.height = bgRect.Height;

            this.viewParams.CenterView();

            this.viewParams.opacity = this.inputActive && this.layout != Layout.SingleBottom ? 1f : 0.75f;
            int centerX = SonOfRobinGame.VirtualWidth / 2;

            switch (this.layout)
            {
                case Layout.SingleCenter:
                    break;
                case Layout.DualLeft:
                    this.viewParams.posX = centerX - (SonOfRobinGame.VirtualWidth / 25) - this.viewParams.posX;
                    break;
                case Layout.DualRight:
                    this.viewParams.posX = centerX + (SonOfRobinGame.VirtualWidth / 25);
                    break;
                case Layout.DualTop:
                    this.viewParams.posY = SonOfRobinGame.VirtualHeight * 0.1f;

                    break;
                case Layout.SingleBottom:
                    this.viewParams.posY = SonOfRobinGame.VirtualHeight - this.viewParams.height;
                    break;
                case Layout.DualBottom:
                    this.viewParams.posY = SonOfRobinGame.VirtualHeight - this.viewParams.height;
                    break;

                default:
                    throw new DivideByZeroException($"Unknown inventory layout '{this.layout}'.");
            }
        }
        private void KeepCursorInBoundsAndSwitchInv()
        {

            bool switchToSecondaryInv = false;

            switch (this.layout)
            {
                case Layout.SingleCenter:
                    if (this.cursorY <= -1) this.cursorY = this.storage.height - 1;
                    if (this.cursorY >= this.storage.height) this.cursorY = 0;

                    if (this.cursorX <= -1) this.cursorX = this.storage.width - 1;
                    if (this.cursorX >= this.storage.width) this.cursorX = 0;
                    break;


                case Layout.DualLeft:
                    if (this.cursorY <= -1) this.cursorY = this.storage.height - 1;
                    if (this.cursorY >= this.storage.height) this.cursorY = 0;

                    if (this.cursorX <= -1) this.cursorX = 0;
                    if (this.cursorX >= this.storage.width)
                    {
                        this.cursorX = this.storage.width - 1;
                        switchToSecondaryInv = true;
                    }

                    break;

                case Layout.DualRight:
                    if (this.cursorY <= -1) this.cursorY = this.storage.height - 1;
                    if (this.cursorY >= this.storage.height) this.cursorY = 0;

                    if (this.cursorX <= -1)
                    {
                        this.cursorX = 0;
                        switchToSecondaryInv = true;
                    }
                    if (this.cursorX >= this.storage.width) this.cursorX = this.storage.width - 1;

                    break;

                case Layout.DualTop:
                    if (this.cursorX <= -1) this.cursorX = this.storage.width - 1;
                    if (this.cursorX >= this.storage.width) this.cursorX = 0;

                    if (this.cursorY <= -1) this.cursorY = 0;

                    if (this.cursorY >= this.storage.height)
                    {
                        this.cursorY = this.storage.height - 1;
                        switchToSecondaryInv = true;
                    }

                    break;


                case Layout.DualBottom:
                    if (this.cursorX <= -1) this.cursorX = this.storage.width - 1;
                    if (this.cursorX >= this.storage.width) this.cursorX = 0;

                    if (this.cursorY <= -1)
                    {
                        this.cursorY = 0;
                        switchToSecondaryInv = true;
                    }
                    if (this.cursorY >= this.storage.height) this.cursorY = this.storage.height - 1;

                    break;

                case Layout.SingleBottom:

                    if (this.cursorX <= -1)
                    {
                        this.cursorX = this.storage.width - 1;
                        this.cursorY -= 1;
                    }
                    if (this.cursorX >= this.storage.width)
                    {
                        this.cursorX = 0;
                        this.cursorY += 1;
                    }

                    if (this.cursorY <= -1) this.cursorY = this.storage.height - 1;
                    if (this.cursorY >= this.storage.height) this.cursorY = 0;

                    break;

                default:
                    throw new DivideByZeroException($"Unknown inventory layout '{this.layout}'.");
            }

            if (switchToSecondaryInv)
            {
                this.MoveOtherInventoryToTop();
                this.MoveDraggedPiecesToOtherInv();
            }

        }

        private void ProcessInput()
        {
            this.SetCursorByTouch();
            if (this.layout == Layout.SingleBottom) { this.MoveCursorByBumpers(); }
            else
            {
                this.MoveCursorByNormalInput();
                if (this.draggedPieces.Count == 0)
                { this.ProcessPieceSelectMode(); }
                else
                { this.ProcessPieceDragMode(); }
            }

        }

        public void SetCursorByTouch()
        {
            if (SonOfRobinGame.platform != Platform.Mobile) return;

            int margin = this.Margin;
            int tileSize = this.TileSize;
            Vector2 slotPos;
            Rectangle slotRect;
            Vector2 touchPos;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                touchPos = (touch.Position / Preferences.globalScale) - this.viewParams.DrawPos;

                foreach (StorageSlot slot in this.storage.AllSlots)
                {
                    slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize);
                    slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

                    if (slotRect.Contains(touchPos))
                    {

                        if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                        {
                            if (this.CursorX == slot.posX && this.CursorY == slot.posY)
                            { this.touchHeldFrames++; }
                            else
                            { this.touchHeldFrames = 0; }

                            this.CursorX = slot.posX;
                            this.CursorY = slot.posY;
                            this.showEmptyCursor = false;
                            return;
                        }
                        else if (touch.State == TouchLocationState.Released && this.touchHeldFrames < minFramesToDragByTouch)
                        {
                            if (this.layout != Layout.SingleBottom)
                            {
                                this.OpenPieceContextMenu();
                                return;
                            }

                        }

                    }
                }

            }

            this.touchHeldFrames = 0; // if no touch was registered
        }

        private bool SwitchToSecondInventoryByTouch()
        {
            if (SonOfRobinGame.platform != Platform.Mobile || this.otherInventory == null) return false;

            Vector2 touchPos;

            Rectangle otherInvBgRect = this.otherInventory.BgRect;
            otherInvBgRect.X += (int)this.otherInventory.viewParams.posX;
            otherInvBgRect.Y += (int)this.otherInventory.viewParams.posY;


            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                touchPos = touch.Position / Preferences.globalScale;
                if (otherInvBgRect.Contains(touchPos))
                {
                    this.MoveOtherInventoryToTop();
                    return true;
                }
            }

            return false;
        }

        private void MoveCursorByNormalInput()
        {
            if (Keyboard.HasBeenPressed(Keys.A) || Keyboard.HasBeenPressed(Keys.Left) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadLeft))
            {
                this.CursorX -= 1;
                this.showEmptyCursor = true;
            }
            if (Keyboard.HasBeenPressed(Keys.D) || Keyboard.HasBeenPressed(Keys.Right) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadRight))
            {
                this.CursorX += 1;
                this.showEmptyCursor = true;
            }
            if (Keyboard.HasBeenPressed(Keys.W) || Keyboard.HasBeenPressed(Keys.Up) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadUp))
            {
                this.CursorY -= 1;
                this.showEmptyCursor = true;
            }
            if (Keyboard.HasBeenPressed(Keys.S) || Keyboard.HasBeenPressed(Keys.Down) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadDown))
            {
                this.CursorY += 1;
                this.showEmptyCursor = true;
            }
        }

        private void MoveCursorByBumpers()
        {
            if (Keyboard.HasBeenPressed(Keys.OemOpenBrackets) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.LeftShoulder))
            {
                this.CursorX -= 1;
                this.showEmptyCursor = true;
            }
            if (Keyboard.HasBeenPressed(Keys.OemCloseBrackets) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.RightShoulder))
            {
                this.CursorX += 1;
                this.showEmptyCursor = true;
            }

        }

        private void OpenPieceContextMenu()
        {
            StorageSlot slot = this.ActiveSlot;
            if (slot == null || slot.locked) return;
            BoardPiece piece = this.storage.GetTopPiece(slot: slot);
            if (piece == null) return;

            Vector2 slotPos = this.GetSlotPos(slot: slot, margin: this.Margin, tileSize: this.TileSize);
            slotPos += new Vector2(this.viewParams.drawPosX, this.viewParams.drawPosY);
            slotPos.X += this.Margin + this.TileSize;
            Vector2 percentPos = new Vector2(slotPos.X / SonOfRobinGame.VirtualWidth, slotPos.Y / SonOfRobinGame.VirtualHeight);

            new PieceContextMenu(piece: piece, storage: this.storage, slot: slot, percentPosX: percentPos.X, percentPosY: percentPos.Y, addMove: this.layout != Layout.SingleCenter);
            return;
        }
        private void MoveOtherInventoryToTop()
        {
            if (this.otherInventory != null) this.otherInventory.MoveToTop();
        }

        private void ProcessPieceSelectMode()
        {
            if (Keyboard.HasBeenPressed(Keys.Escape) ||
                VirtButton.HasButtonBeenPressed(VButName.Return) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B))
            {
                // must go first, to read touch return button!
                SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.storage.world.player);
                return;
            }

            if (this.SwitchToSecondInventoryByTouch()) return;

            if (Keyboard.HasBeenPressed(Keys.Enter) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.A))
            {
                this.OpenPieceContextMenu();
                return;
            }

            if (Keyboard.HasBeenPressed(Keys.Tab) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.LeftShoulder) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.RightShoulder))
            {
                this.MoveOtherInventoryToTop();
                return;
            }

            if (Keyboard.HasBeenPressed(Keys.Space) ||
                Keyboard.HasBeenPressed(Keys.RightAlt) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.X) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Y) ||
                this.touchHeldFrames >= minFramesToDragByTouch)
            {
                this.draggedByTouch = this.touchHeldFrames >= minFramesToDragByTouch;

                StorageSlot activeSlot = this.ActiveSlot;
                if (activeSlot == null) return;

                List<BoardPiece> pickedUpPieces;

                if ((!VirtButton.IsButtonDown(VButName.DragSingle) && this.draggedByTouch) ||
                    GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.X) ||
                    Keyboard.HasBeenPressed(Keys.Space))
                { pickedUpPieces = this.storage.RemoveAllPiecesFromSlot(slot: activeSlot); }

                else
                {
                    pickedUpPieces = new List<BoardPiece> { };
                    BoardPiece pickedUpPiece = this.storage.RemoveTopPiece(slot: activeSlot);
                    if (pickedUpPiece != null) pickedUpPieces.Add(pickedUpPiece);
                }

                if (pickedUpPieces.Count > 0) this.draggedPieces = pickedUpPieces;
            }
        }

        private void MoveDraggedPiecesToOtherInv()
        {
            if (this.otherInventory == null)
            { this.ReleaseHeldPieces(slot: null, forceReleaseAll: true); }
            else
            {
                foreach (BoardPiece piece in this.draggedPieces)
                { this.otherInventory.draggedPieces.Add(piece); }
                this.draggedPieces.Clear();
            }
        }

        private void ProcessPieceDragMode()
        {
            if (Keyboard.HasBeenPressed(Keys.Escape) || VirtButton.HasButtonBeenPressed(VButName.Return) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B))
            {
                SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.storage.world.player);
                return;
            }

            if (Keyboard.HasBeenPressed(Keys.Tab) ||
               GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.LeftShoulder) ||
               GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.RightShoulder))
            {
                this.MoveOtherInventoryToTop();
                this.MoveDraggedPiecesToOtherInv();
                return;
            }

            if (this.SwitchToSecondInventoryByTouch())
            {
                this.MoveDraggedPiecesToOtherInv();
                return;
            }

            if (Keyboard.HasBeenPressed(Keys.Space) ||
                Keyboard.HasBeenPressed(Keys.RightAlt) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.X) ||
                GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.Y) ||
                TouchInput.IsStateAvailable(TouchLocationState.Released))
            {
                this.ReleaseHeldPieces(slot: this.ActiveSlot);
                return;
            }
        }

        private void ReleaseHeldPieces(StorageSlot slot, bool forceReleaseAll = false)
        {
            if (this.draggedPieces.Count == 0) return;

            if (this.draggedByTouch) forceReleaseAll = true;
            var piecesThatDidNotFitIn = new List<BoardPiece> { };

            foreach (BoardPiece piece in this.draggedPieces)
            {
                if (slot != null && slot.CanFitThisPiece(piece))
                { slot.AddPiece(piece); }
                else
                {
                    if (forceReleaseAll)
                    { if (!this.storage.AddPiece(piece)) piecesThatDidNotFitIn.Add(piece); }
                    else
                    { piecesThatDidNotFitIn.Add(piece); }
                }
            }

            if (forceReleaseAll && piecesThatDidNotFitIn.Count > 0)
            {
                foreach (BoardPiece piece in piecesThatDidNotFitIn)
                { this.storage.DropPieceToTheGround(piece: piece, addMovement: false); }
                piecesThatDidNotFitIn.Clear();
            }

            this.draggedPieces = piecesThatDidNotFitIn;
        }

        public override void Draw()
        {
            if (this.IgnoreUpdateAndDraw) return;

            int margin = this.Margin;
            int tileSize = this.TileSize;
            int spriteSize = Convert.ToInt32((float)tileSize * 0.7f);
            int spriteOffset = (tileSize - spriteSize) / 2;
            bool isActive;
            Rectangle tileRect;
            Vector2 slotPos;
            Color outlineColor;
            Color fillColor;

            this.DrawLabel();

            Rectangle bgRect = this.BgRect;

            if (this.layout != Layout.SingleBottom)
            {
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, Color.BlanchedAlmond * 0.7f * this.viewParams.drawOpacity);
                Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);
            }


            foreach (StorageSlot slot in this.storage.AllSlots)
            {
                slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize);

                isActive = slot.posX == this.CursorX && slot.posY == this.CursorY;
                if ((this.draggedByTouch || !this.inputActive)) isActive = false;

                tileRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

                outlineColor = isActive && this.showEmptyCursor ? Color.LawnGreen : Color.White;
                fillColor = (isActive && (this.showEmptyCursor || this.layout == Layout.SingleBottom)) ? Color.LightSeaGreen : Color.White;

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, tileRect, fillColor * 0.35f * this.viewParams.drawOpacity);
                Helpers.DrawRectangleOutline(rect: tileRect, color: outlineColor * this.viewParams.drawOpacity * 0.8f, borderWidth: 2);

                Rectangle destRect = isActive && this.showEmptyCursor ? tileRect : new Rectangle((int)slotPos.X + spriteOffset, (int)slotPos.Y + spriteOffset, spriteSize, spriteSize);
                slot.Draw(destRect: destRect, opacity: this.viewParams.drawOpacity);

                this.DrawQuantity(pieceCount: slot.PieceCount, destRect: tileRect);
            }

            if (this.InputType != InputTypes.Always) this.DrawCursor();
        }

        private void DrawLabel()
        {
            if (this.layout == Layout.SingleBottom) return;

            Rectangle bgRect = this.BgRect;

            Vector2 labelSize = font.MeasureString(this.label);
            float maxTextWidth = bgRect.Width * 0.3f;
            float maxTextHeight = bgRect.Height * 0.1f;
            float textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);

            float textWidth = labelSize.X * textScale;
            float textHeight = labelSize.Y * textScale;

            Vector2 labelPos = new Vector2((bgRect.Width - textWidth) / 2,
                                            bgRect.Top - textHeight);

            float shadowOffset = textHeight * 0.06f;

            Vector2 shadowPos = labelPos + new Vector2(shadowOffset, shadowOffset);

            SonOfRobinGame.spriteBatch.DrawString(font, this.label, position: shadowPos, color: Color.Black * 0.5f * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
            SonOfRobinGame.spriteBatch.DrawString(font, this.label, position: labelPos, color: Color.White * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        public void DrawQuantity(int pieceCount, Rectangle destRect)
        {
            if (pieceCount <= 1) return;

            string countTxt = $"x{pieceCount}";

            Vector2 textSize = font.MeasureString(countTxt);
            float maxTextHeight = destRect.Height * 0.5f;
            float maxTextWidth = destRect.Width * 0.5f;

            float textScale = Math.Min(maxTextWidth / textSize.X, maxTextHeight / textSize.Y);

            Vector2 textPos = new Vector2(
                            destRect.Left + (destRect.Width * 0.05f),
                            destRect.Top + (textSize.Y * textScale));

            float shadowOffset = Math.Max(destRect.Width * 0.01f, 1);

            Vector2 textShadowPos;

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    textShadowPos = new Vector2(textPos.X + (shadowOffset * x), textPos.Y + (shadowOffset * y));

                    SonOfRobinGame.spriteBatch.DrawString(font, countTxt, position: textShadowPos, color: Color.Black * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
                }
            }

            SonOfRobinGame.spriteBatch.DrawString(font, countTxt, position: textPos, color: Color.White * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        private Vector2 GetSlotPos(StorageSlot slot, int margin, int tileSize)
        {
            int slotPosX = margin + (slot.posX * (margin + tileSize));
            int slotPosy = margin + (slot.posY * (margin + tileSize));
            return new Vector2(slotPosX, slotPosy);
        }

        private Vector2 GetSlotPos(int posX, int posY, int margin, int tileSize)
        {
            int slotPosX = margin + (posX * (margin + tileSize));
            int slotPosy = margin + (posY * (margin + tileSize));
            return new Vector2(slotPosX, slotPosy);
        }

        public void DrawCursor()
        {
            if (this.CursorX == -1 || this.CursorY == -1 || !this.inputActive || (!this.showEmptyCursor && this.draggedPieces.Count == 0)) return;

            Texture2D cursorTexture = SonOfRobinGame.textureByName["cursor"];
            int tileSize = this.TileSize;
            Vector2 slotPos = this.GetSlotPos(posX: this.CursorX, posY: this.CursorY, tileSize: this.TileSize, margin: this.Margin);

            slotPos += new Vector2(tileSize * 0.5f, tileSize * 0.5f);

            Rectangle sourceRectangle = new Rectangle(0, 0, cursorTexture.Width, cursorTexture.Height);
            Rectangle destinationRectangle = new Rectangle((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

            if (draggedPieces.Count > 0)
            {
                BoardPiece shownPiece = draggedPieces[draggedPieces.Count - 1];
                Rectangle shownPieceRect = new Rectangle(
                    destinationRectangle.X + (int)(destinationRectangle.Width * 0.75f),
                    destinationRectangle.Y,
                    destinationRectangle.Width,
                    destinationRectangle.Height);

                shownPiece.sprite.Draw(destRect: shownPieceRect, opacity: viewParams.drawOpacity);

                if (shownPiece.sprite.boardPiece.hitPoints < shownPiece.sprite.boardPiece.maxHitPoints)
                {
                    new StatBar(label: "", value: (int)shownPiece.sprite.boardPiece.hitPoints, valueMax: (int)shownPiece.sprite.boardPiece.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0),
                        posX: shownPieceRect.Center.X, posY: shownPieceRect.Y + (int)(shownPieceRect.Width * 0.8f), width: (int)(shownPieceRect.Width * 0.8f), height: (int)(shownPieceRect.Height * 0.1f));

                    StatBar.FinishThisBatch();
                    StatBar.DrawAll();
                }

                this.DrawQuantity(pieceCount: draggedPieces.Count, destRect: shownPieceRect);
            }

            SonOfRobinGame.spriteBatch.Draw(cursorTexture, destinationRectangle, sourceRectangle, Color.White);
        }

    }
}
