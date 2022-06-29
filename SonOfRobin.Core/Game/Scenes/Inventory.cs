using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Inventory : Scene
    {
        public enum Layout { SingleCenter, SingleBottom, DualLeft, DualRight, DualTop, DualBottom }

        private static readonly int minFramesToDragByTouch = 15;
        private static readonly float marginPercent = 0.05f;
        private static readonly SpriteFont font = SonOfRobinGame.fontTommy40;

        public readonly Layout layout;
        private readonly BoardPiece piece;
        public readonly PieceStorage storage;
        private int cursorX;
        private int cursorY;

        public List<BoardPiece> draggedPieces;
        private int touchHeldFrames;
        private bool draggedByTouch;
        private StorageSlot lastTouchedSlot;

        public Inventory otherInventory;
        private bool IgnoreUpdateAndDraw
        {
            get
            {
                if (this.piece.world.mapMode == World.MapMode.Big || this.piece.world.CineMode || this.piece.world.demoMode) return true;

                Player player = this.piece.world.player;
                if (player.activeState == BoardPiece.State.PlayerControlledSleep || !player.alive || this.piece.world.SpectatorMode) return true;

                Scene topScene = sceneStack[sceneStack.Count - 1];
                if (topScene.GetType() == typeof(TextWindow)) topScene = sceneStack[sceneStack.Count - 2];
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
                if (this.layout == Layout.SingleBottom || this.layout == Layout.DualBottom) return SonOfRobinGame.VirtualHeight * 0.15f;
                if (this.layout == Layout.DualTop) return SonOfRobinGame.VirtualHeight * 0.5f;

                return SonOfRobinGame.VirtualHeight * 0.75f;
            }
        }
        public StorageSlot ActiveSlot
        { get { return this.storage.GetSlot((byte)this.CursorX, (byte)this.CursorY); } }

        public BoardPiece SelectedPiece
        {
            get
            {
                StorageSlot activeSlot = this.ActiveSlot;
                if (activeSlot == null) return null;
                return activeSlot.TopPiece;
            }
        }

        private int TileSize
        {
            get
            {
                int margin = this.Margin;
                float maxTileWidth = (BgMaxWidth / this.storage.Width) - margin;
                float maxTileHeight = (BgMaxHeight / this.storage.Height) - margin;
                return Convert.ToInt32(Math.Min(maxTileWidth, maxTileHeight));
            }
        }

        private Rectangle BgRect
        {
            get
            {
                int margin = this.Margin;
                int tileSize = this.TileSize;

                int width = ((tileSize + margin) * this.storage.Width) + margin;
                int height = ((tileSize + margin) * this.storage.Height) + margin;

                return new Rectangle(0, 0, width, height);
            }
        }

        public Inventory(PieceStorage storage, BoardPiece piece, Layout layout = Layout.SingleCenter, bool blocksUpdatesBelow = true, Inventory otherInventory = null, InputTypes inputType = InputTypes.Normal) : base(inputType: inputType, priority: 1, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Inventory, tipsLayout: ControlTips.TipsLayout.InventorySelect)
        {
            this.storage = storage;
            this.piece = piece;
            this.layout = layout;

            if (this.storage.lastUsedSlot != null)
            {
                this.cursorX = this.storage.lastUsedSlot.posX;
                this.cursorY = this.storage.lastUsedSlot.posY;
            }
            else
            {
                this.cursorX = 0;
                this.cursorY = 0;
            }

            this.draggedPieces = new List<BoardPiece> { };
            this.touchHeldFrames = 0;
            this.draggedByTouch = false;
            this.lastTouchedSlot = null;
            this.otherInventory = otherInventory;

            this.UpdateViewParams();

            if (this.piece.GetType() == typeof(Container))
            {
                var container = (Container)piece;
                container.Open();
            }

            if (this.layout != Layout.SingleBottom && this.layout != Layout.DualBottom && !this.transManager.HasAnyTransition)
            {
                this.transManager.AddMultipleTransitions(paramsToChange: new Dictionary<string, float> { { "PosY", this.viewParams.PosY - SonOfRobinGame.VirtualHeight }, { "Opacity", 0f } }, outTrans: false, duration: 12, refreshBaseVal: false);
            }
        }

        public override void Remove()
        {
            this.ReleaseHeldPieces(slot: null, forceReleaseAll: true);
            SonOfRobinGame.hintWindow.TurnOff();

            if (!this.transManager.IsEnding && this.layout != Layout.SingleBottom && this.layout != Layout.DualBottom)
            {
                if (this.piece.GetType() == typeof(Container))
                {
                    var container = (Container)piece;
                    container.Close();
                }

                this.transManager.AddMultipleTransitions(paramsToChange: new Dictionary<string, float> {
                    { "PosY", this.viewParams.PosY - SonOfRobinGame.VirtualHeight }, { "Opacity", 0f } },
                    outTrans: true, duration: 12, refreshBaseVal: false, endRemoveScene: true);

                return;
            }

            base.Remove();
        }

        private void UpdateHintWindow()
        {
            BoardPiece selectedPiece = this.draggedPieces.Count == 0 ? this.SelectedPiece : this.draggedPieces[0];
            StorageSlot slot = this.storage.GetSlot(x: this.CursorX, y: this.CursorY);
            if (selectedPiece == null || slot == null)
            {
                SonOfRobinGame.hintWindow.TurnOff();
                return;
            }

            var entryList = new List<InfoWindow.TextEntry> {
                new InfoWindow.TextEntry(frame:selectedPiece.sprite.frame, text: Helpers.FirstCharToUpperCase(selectedPiece.readableName), color: Color.White, scale: 1.5f),
                new InfoWindow.TextEntry(text: selectedPiece.description, color: Color.White)};

            if (selectedPiece.buffList != null)
            {
                foreach (BuffEngine.Buff buff in selectedPiece.buffList)
                { entryList.Add(new InfoWindow.TextEntry(text: buff.Description, color: Color.Cyan, scale: 1f)); }
            }

            int margin = this.Margin;
            int tileSize = this.TileSize;
            InfoWindow hintWindow = SonOfRobinGame.hintWindow;

            Vector2 slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize) + new Vector2(this.viewParams.PosX, this.viewParams.PosY);
            Vector2 windowPos;
            Vector2 infoWindowSize = hintWindow.MeasureEntries(entryList);

            switch (this.layout)
            {
                case Layout.SingleCenter:
                    windowPos = new Vector2(
                        this.viewParams.PosX + this.viewParams.Width + margin,
                        slotPos.Y + (tileSize / 2) - (infoWindowSize.Y / 2));
                    break;

                case Layout.SingleBottom:
                    return; // not used, anyway

                case Layout.DualLeft:
                    windowPos = new Vector2(
                        this.viewParams.PosX + this.viewParams.Width + margin,
                        slotPos.Y + (tileSize / 2) - (infoWindowSize.Y / 2));
                    break;

                case Layout.DualRight:
                    windowPos = new Vector2(
                        this.viewParams.PosX - margin - infoWindowSize.X,
                        slotPos.Y + (tileSize / 2) - (infoWindowSize.Y / 2));
                    break;

                case Layout.DualTop:
                    windowPos = new Vector2(
                        slotPos.X + (tileSize / 2) - (infoWindowSize.X / 2),
                        this.viewParams.PosY + this.viewParams.Height + margin);
                    break;

                case Layout.DualBottom:
                    windowPos = slotPos;
                    windowPos.X -= (infoWindowSize.X / 2) - (tileSize / 2);
                    windowPos.Y -= infoWindowSize.Y + (margin * 2);
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported layout - {layout}.");
            }

            // keeping the window inside screen bounds
            windowPos.X = Math.Max(windowPos.X, 0);
            windowPos.Y = Math.Max(windowPos.Y, 0);
            int maxX = (int)((SonOfRobinGame.VirtualWidth * hintWindow.viewParams.ScaleX) - infoWindowSize.X);
            int maxY = (int)((SonOfRobinGame.VirtualHeight * hintWindow.viewParams.ScaleY) - infoWindowSize.Y);
            windowPos.X = Math.Min(windowPos.X, maxX);
            windowPos.Y = Math.Min(windowPos.Y, maxY);

            hintWindow.TurnOn(newPosX: (int)windowPos.X, newPosY: (int)windowPos.Y, entryList: entryList);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.IgnoreUpdateAndDraw) return;

            if (this.CursorX >= this.storage.Width) this.CursorX = this.storage.Width - 1; // in case storage was resized
            if (this.CursorY >= this.storage.Height) this.CursorY = this.storage.Height - 1; // in case storage was resized
            this.storage.DestroyBrokenPieces();
            this.UpdateViewParams();
            if (this.layout != Layout.SingleBottom && this.inputActive) this.UpdateHintWindow();
            this.ProcessInput();
            this.storage.lastUsedSlot = this.ActiveSlot;
        }

        private void UpdateViewParams()
        {
            Rectangle bgRect = this.BgRect;
            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = bgRect.Height;

            this.viewParams.CenterView();
            this.viewParams.Opacity = this.inputActive && this.layout != Layout.SingleBottom ? 1f : 0.75f;

            int centerX = SonOfRobinGame.VirtualWidth / 2;
            float posY;

            switch (this.layout)
            {
                case Layout.SingleCenter:
                    break;

                case Layout.DualLeft:
                    this.viewParams.PosX = centerX - (SonOfRobinGame.VirtualWidth / 25) - this.viewParams.PosX;
                    break;

                case Layout.DualRight:
                    this.viewParams.PosX = centerX + (SonOfRobinGame.VirtualWidth / 25);
                    break;

                case Layout.DualTop:
                    this.viewParams.PosY = SonOfRobinGame.VirtualHeight * 0.1f;
                    break;

                case Layout.SingleBottom:
                    posY = Preferences.ShowControlTips ? 0.95f : 1f; // little margin for ControlTips at the bottom
                    this.viewParams.PosY = (SonOfRobinGame.VirtualHeight * posY) - this.viewParams.Height;
                    break;

                case Layout.DualBottom:
                    posY = Preferences.ShowControlTips ? 0.95f : 1f; // little margin for ControlTips at the bottom
                    this.viewParams.PosY = (SonOfRobinGame.VirtualHeight * posY) - this.viewParams.Height;
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
                    if (this.cursorY <= -1) this.cursorY = this.storage.Height - 1;
                    if (this.cursorY >= this.storage.Height) this.cursorY = 0;

                    if (this.cursorX <= -1) this.cursorX = this.storage.Width - 1;
                    if (this.cursorX >= this.storage.Width) this.cursorX = 0;
                    break;


                case Layout.DualLeft:
                    if (this.cursorY <= -1) this.cursorY = this.storage.Height - 1;
                    if (this.cursorY >= this.storage.Height) this.cursorY = 0;

                    if (this.cursorX <= -1) this.cursorX = 0;
                    if (this.cursorX >= this.storage.Width)
                    {
                        this.cursorX = this.storage.Width - 1;
                        switchToSecondaryInv = true;
                        this.otherInventory.cursorX = 0;
                        this.otherInventory.cursorY = Math.Min(this.cursorY, this.otherInventory.storage.Height - 1);
                    }

                    break;

                case Layout.DualRight:
                    if (this.cursorY <= -1) this.cursorY = this.storage.Height - 1;
                    if (this.cursorY >= this.storage.Height) this.cursorY = 0;

                    if (this.cursorX <= -1)
                    {
                        this.cursorX = 0;
                        switchToSecondaryInv = true;
                        this.otherInventory.cursorX = this.otherInventory.storage.Width - 1;
                        this.otherInventory.cursorY = Math.Min(this.cursorY, this.otherInventory.storage.Height - 1);
                    }
                    if (this.cursorX >= this.storage.Width) this.cursorX = this.storage.Width - 1;

                    break;

                case Layout.DualTop:
                    if (this.cursorX <= -1) this.cursorX = this.storage.Width - 1;
                    if (this.cursorX >= this.storage.Width) this.cursorX = 0;

                    if (this.cursorY <= -1) this.cursorY = 0;

                    if (this.cursorY >= this.storage.Height)
                    {
                        this.cursorY = this.storage.Height - 1;
                        switchToSecondaryInv = true;
                        this.otherInventory.cursorX = Math.Min(this.cursorX, this.otherInventory.storage.Width - 1);
                        this.otherInventory.cursorY = 0;
                    }

                    break;

                case Layout.DualBottom:
                    if (this.cursorX <= -1) this.cursorX = this.storage.Width - 1;
                    if (this.cursorX >= this.storage.Width) this.cursorX = 0;

                    if (this.cursorY <= -1)
                    {
                        this.cursorY = 0;
                        switchToSecondaryInv = true;
                        this.otherInventory.cursorX = Math.Min(this.cursorX, this.otherInventory.storage.Width - 1);
                        this.otherInventory.cursorY = this.otherInventory.storage.Height - 1;
                    }
                    if (this.cursorY >= this.storage.Height) this.cursorY = this.storage.Height - 1;

                    break;

                case Layout.SingleBottom:

                    if (this.cursorX <= -1)
                    {
                        this.cursorX = this.storage.Width - 1;
                        this.cursorY -= 1;
                    }
                    if (this.cursorX >= this.storage.Width)
                    {
                        this.cursorX = 0;
                        this.cursorY += 1;
                    }

                    if (this.cursorY <= -1) this.cursorY = this.storage.Height - 1;
                    if (this.cursorY >= this.storage.Height) this.cursorY = 0;

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
            if (!this.piece.world.player.alive) return;

            this.SetCursorByTouch();
            if (this.layout == Layout.SingleBottom) this.MoveCursorByBumpers();
            else
            {
                this.MoveCursorByNormalInput();
                if (this.draggedPieces.Count == 0) this.ProcessPieceSelectMode();
                else this.ProcessPieceDragMode();
            }
        }

        public void SetCursorByTouch()
        {
            if (!Preferences.EnableTouch) return;

            int margin = this.Margin;
            int tileSize = this.TileSize;
            Vector2 slotPos;
            Rectangle slotRect;
            Vector2 touchPos;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                touchPos = (touch.Position / Preferences.GlobalScale) - this.viewParams.DrawPos;

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
                            return;
                        }
                        else if (touch.State == TouchLocationState.Released && this.touchHeldFrames < minFramesToDragByTouch)
                        {
                            if (this.layout != Layout.SingleBottom)
                            {
                                if (this.lastTouchedSlot == this.ActiveSlot) this.OpenPieceContextMenu();
                                else this.lastTouchedSlot = this.ActiveSlot;
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
            if (!Preferences.EnableTouch || this.otherInventory == null) return false;

            Vector2 touchPos;

            Rectangle otherInvBgRect = this.otherInventory.BgRect;
            otherInvBgRect.X += (int)this.otherInventory.viewParams.PosX;
            otherInvBgRect.Y += (int)this.otherInventory.viewParams.PosY;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                touchPos = touch.Position / Preferences.GlobalScale;
                if (otherInvBgRect.Contains(touchPos))
                {
                    this.lastTouchedSlot = null;
                    this.MoveOtherInventoryToTop();
                    return true;
                }
            }

            return false;
        }

        private void MoveCursorByNormalInput()
        {
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalLeft)) this.CursorX -= 1;
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalRight)) this.CursorX += 1;
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalUp)) this.CursorY -= 1;
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalDown)) this.CursorY += 1;
        }

        private void MoveCursorByBumpers()
        {
            World world = World.GetTopWorld();
            if (world?.player.activeState == BoardPiece.State.PlayerControlledShooting) return;

            if (InputMapper.HasBeenPressed(InputMapper.Action.ToolbarPrev)) this.CursorX -= 1;
            if (InputMapper.HasBeenPressed(InputMapper.Action.ToolbarNext)) this.CursorX += 1;
        }

        private void OpenPieceContextMenu()
        {
            StorageSlot slot = this.ActiveSlot;
            if (slot == null) return;
            BoardPiece piece = this.storage.GetTopPiece(slot: slot);
            if (piece == null) return;

            var lockedButWorking = new List<PieceTemplate.Name> { PieceTemplate.Name.CookingTrigger, PieceTemplate.Name.FireplaceTriggerOn, PieceTemplate.Name.FireplaceTriggerOff };
            if (slot.locked && !lockedButWorking.Contains(piece.name)) return;

            Vector2 slotPos = this.GetSlotPos(slot: slot, margin: this.Margin, tileSize: this.TileSize);
            slotPos += new Vector2(this.viewParams.PosX, this.viewParams.PosY);
            slotPos.X += this.Margin + this.TileSize;
            Vector2 percentPos = new Vector2(slotPos.X / SonOfRobinGame.VirtualWidth, slotPos.Y / SonOfRobinGame.VirtualHeight);

            bool addMove = this.layout != Layout.SingleCenter && !slot.locked && this.otherInventory.storage.CanFitThisPiece(piece);
            bool addDrop = !slot.locked;
            bool addCook = piece.name == PieceTemplate.Name.CookingTrigger;
            bool addIgnite = piece.name == PieceTemplate.Name.FireplaceTriggerOn;
            bool addExtinguish = piece.name == PieceTemplate.Name.FireplaceTriggerOff;

            new PieceContextMenu(piece: piece, storage: this.storage, slot: slot, percentPosX: percentPos.X, percentPosY: percentPos.Y, addMove: addMove, addDrop: addDrop, addCook: addCook, addIgnite: addIgnite, addExtinguish: addExtinguish);
            return;
        }
        private void MoveOtherInventoryToTop()
        {
            if (this.otherInventory != null) this.otherInventory.MoveToTop();
        }

        private void ProcessPieceSelectMode()
        {
            this.tipsLayout = ControlTips.TipsLayout.InventorySelect;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                // must go first, to read touch return button!
                SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.storage.world.player);
                return;
            }

            if (this.SwitchToSecondInventoryByTouch()) return;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm))
            {
                this.OpenPieceContextMenu();
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.InvSwitch))
            {
                this.MoveOtherInventoryToTop();
                return;
            }

            this.draggedByTouch = this.touchHeldFrames >= minFramesToDragByTouch;

            if (this.draggedByTouch ||
                InputMapper.HasBeenPressed(InputMapper.Action.InvPickOne) ||
                InputMapper.HasBeenPressed(InputMapper.Action.InvPickStack))
            {
                StorageSlot activeSlot = this.ActiveSlot;
                if (activeSlot == null) return;

                List<BoardPiece> pickedUpPieces;

                if ((this.draggedByTouch && !VirtButton.IsButtonDown(VButName.DragSingle)) ||
                    InputMapper.HasBeenPressed(InputMapper.Action.InvPickStack))
                {
                    pickedUpPieces = this.storage.RemoveAllPiecesFromSlot(slot: activeSlot);
                }
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
            if (this.otherInventory == null) this.ReleaseHeldPieces(slot: null, forceReleaseAll: true);
            else
            {
                if (this.draggedPieces.Count > 0) this.otherInventory.tipsLayout = ControlTips.TipsLayout.InventoryDrag;
                else this.otherInventory.tipsLayout = ControlTips.TipsLayout.InventorySelect;

                foreach (BoardPiece piece in this.draggedPieces)
                { this.otherInventory.draggedPieces.Add(piece); }
                this.draggedPieces.Clear();
            }
        }

        private void ProcessPieceDragMode()
        {
            this.tipsLayout = ControlTips.TipsLayout.InventoryDrag;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                SetInventoryLayout(newLayout: InventoryLayout.Toolbar, player: this.storage.world.player);
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.InvSwitch))
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

            this.draggedByTouch = TouchInput.IsStateAvailable(TouchLocationState.Released);

            if (InputMapper.HasBeenPressed(InputMapper.Action.InvRelease) ||
                this.draggedByTouch)
            {
                this.ReleaseHeldPieces(slot: this.ActiveSlot);
                return;
            }
        }

        private void ReleaseHeldPieces(StorageSlot slot, bool forceReleaseAll = false)
        {
            if (this.draggedPieces.Count == 0) return;

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"ReleaseHeldPieces");

            int initialDraggedCount = this.draggedPieces.Count;
            PieceTemplate.Name initialTopPieceName = this.draggedPieces[0].name;

            if (this.draggedByTouch) forceReleaseAll = true;
            var piecesThatDidNotFitIn = new List<BoardPiece> { };

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"forceReleaseAll {forceReleaseAll}");

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
                {
                    if (this.otherInventory != null) this.otherInventory.storage.AddPiece(piece, dropIfDoesNotFit: true, addMovement: false);
                    else this.storage.DropPieceToTheGround(piece: piece, addMovement: false);
                }
                piecesThatDidNotFitIn.Clear();
            }

            this.draggedPieces = piecesThatDidNotFitIn;

            if (this.draggedPieces.Count == initialDraggedCount && this.draggedPieces[0].name == initialTopPieceName) this.SwapDraggedAndSlotPieces(slot: slot);
        }

        private void SwapDraggedAndSlotPieces(StorageSlot slot)
        {
            var slotPieces = this.storage.RemoveAllPiecesFromSlot(slot: slot);

            bool swapPossible = true;
            foreach (BoardPiece piece in this.draggedPieces)
            {
                if (!slot.CanFitThisPiece(piece))
                {
                    swapPossible = false;
                    break;
                }
            }

            if (swapPossible)
            {
                foreach (BoardPiece piece in this.draggedPieces)
                { slot.AddPiece(piece); }
                this.draggedPieces = slotPieces;
            }
            else
            {
                foreach (BoardPiece piece in slotPieces)
                { slot.AddPiece(piece); }
            }
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

            this.DrawMainLabel();

            Rectangle bgRect = this.BgRect;

            if (this.layout != Layout.SingleBottom)
            {
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, Color.BlanchedAlmond * 0.7f * this.viewParams.drawOpacity);
                Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);
            }

            foreach (StorageSlot slot in this.storage.AllSlots)
            {
                if (slot.hidden) continue;

                slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize);

                isActive = slot.posX == this.CursorX && slot.posY == this.CursorY;
                if (!this.inputActive) isActive = false;

                tileRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

                outlineColor = isActive ? Color.LawnGreen : Color.White;
                fillColor = isActive ? Color.LightSeaGreen : Color.White;

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, tileRect, fillColor * 0.35f * this.viewParams.drawOpacity);
                Helpers.DrawRectangleOutline(rect: tileRect, color: outlineColor * this.viewParams.drawOpacity * 0.8f, borderWidth: 2);

                this.DrawSlotLabel(slot: slot, tileRect: tileRect);

                Rectangle destRect = isActive ? tileRect : new Rectangle((int)slotPos.X + spriteOffset, (int)slotPos.Y + spriteOffset, spriteSize, spriteSize);
                slot.Draw(destRect: destRect, opacity: this.viewParams.drawOpacity);

                DrawQuantity(pieceCount: slot.PieceCount, destRect: tileRect, opacity: this.viewParams.drawOpacity);
            }

            if (this.InputType != InputTypes.Always) this.DrawCursor();
        }

        private void DrawSlotLabel(StorageSlot slot, Rectangle tileRect)
        {
            if (slot.label == "" || !slot.IsEmpty) return;

            //Helpers.DrawRectangleOutline(rect: tileRect, color: Color.Red, borderWidth: 2); // for testing

            Vector2 labelSize = font.MeasureString(slot.label);

            float maxTextWidth = tileRect.Width * 0.6f;
            float maxTextHeight = tileRect.Height * 0.18f;
            float textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);

            float textWidth = labelSize.X * textScale;
            float textHeight = labelSize.Y * textScale;

            Vector2 labelPos = new Vector2(tileRect.X + ((tileRect.Width - textWidth) / 2),
                                           tileRect.Y + (tileRect.Height - textHeight) / 2);

            //Helpers.DrawRectangleOutline(rect: new Rectangle((int)labelPos.X, (int)labelPos.Y, (int)textWidth, (int)textHeight), color: Color.Green, borderWidth: 2); // for testing

            float shadowOffset = textHeight * 0.06f;

            Vector2 shadowPos = labelPos + new Vector2(shadowOffset, shadowOffset);

            SonOfRobinGame.spriteBatch.DrawString(font, slot.label, position: shadowPos, color: Color.Black * 0.5f * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
            SonOfRobinGame.spriteBatch.DrawString(font, slot.label, position: labelPos, color: Color.White * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        private void DrawMainLabel()
        {
            if (this.layout == Layout.SingleBottom) return;

            Rectangle bgRect = this.BgRect;

            string label = Convert.ToString(this.storage.storageType);

            Vector2 labelSize = font.MeasureString(label);
            float maxTextWidth = bgRect.Width * 0.3f;
            float maxTextHeight = bgRect.Height * 0.1f;
            float textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);

            float textWidth = labelSize.X * textScale;
            float textHeight = labelSize.Y * textScale;

            Vector2 labelPos = new Vector2((bgRect.Width - textWidth) / 2,
                                            bgRect.Top - textHeight);

            float shadowOffset = textHeight * 0.06f;

            Vector2 shadowPos = labelPos + new Vector2(shadowOffset, shadowOffset);

            SonOfRobinGame.spriteBatch.DrawString(font, label, position: shadowPos, color: Color.Black * 0.5f * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
            SonOfRobinGame.spriteBatch.DrawString(font, label, position: labelPos, color: Color.White * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        public static void DrawQuantity(int pieceCount, Rectangle destRect, float opacity, bool ignoreSingle = true)
        {
            if (ignoreSingle && pieceCount <= 1) return;

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

                    SonOfRobinGame.spriteBatch.DrawString(font, countTxt, position: textShadowPos, color: Color.Black * opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
                }
            }

            SonOfRobinGame.spriteBatch.DrawString(font, countTxt, position: textPos, color: Color.White * opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
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
            if (this.CursorX == -1 || this.CursorY == -1 || !this.inputActive || (this.draggedPieces.Count == 0 && SonOfRobinGame.platform == Platform.Mobile)) return;

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

                shownPiece.sprite.DrawAndKeepInRectBounds(destRect: shownPieceRect, opacity: viewParams.drawOpacity);

                if (shownPiece.sprite.boardPiece.hitPoints < shownPiece.sprite.boardPiece.maxHitPoints)
                {
                    new StatBar(label: "", value: (int)shownPiece.sprite.boardPiece.hitPoints, valueMax: (int)shownPiece.sprite.boardPiece.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0),
                        posX: shownPieceRect.Center.X, posY: shownPieceRect.Y + (int)(shownPieceRect.Width * 0.8f), width: (int)(shownPieceRect.Width * 0.8f), height: (int)(shownPieceRect.Height * 0.1f));

                    StatBar.FinishThisBatch();
                    StatBar.DrawAll();
                }

                DrawQuantity(pieceCount: draggedPieces.Count, destRect: shownPieceRect, opacity: this.viewParams.drawOpacity);
            }

            SonOfRobinGame.spriteBatch.Draw(cursorTexture, destinationRectangle, sourceRectangle, Color.White);
        }

    }
}
