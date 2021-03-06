using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Inventory : Scene
    {
        public enum Layout
        {
            None,
            Toolbar,
            InventoryAndToolbar,
            InventoryAndChest,
            InventoryAndEquip
        }

        public enum TransDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        public enum Type { SingleCenter, SingleBottom, DualLeft, DualRight, DualTop, DualBottom }

        private static readonly int minFramesToDragByTouch = 15;
        private static readonly float marginPercent = 0.05f;
        private static readonly SpriteFont font = SonOfRobinGame.fontTommy40;

        private static readonly Sound soundOpen = new Sound(SoundData.Name.InventoryOpen);
        private static readonly Sound soundNavigate = new Sound(SoundData.Name.Navigation);
        private static readonly Sound soundSwitch = new Sound(SoundData.Name.Select);
        private static readonly Sound soundEnterContextMenu = new Sound(SoundData.Name.Invoke);
        private static readonly Sound soundPickUp = new Sound(SoundData.Name.PickUpItem, volume: 0.8f);

        public static Layout layout = Layout.None;

        public readonly Type type;
        private readonly TransDirection transDirection;
        private readonly BoardPiece piece;
        public readonly PieceStorage storage;
        private int cursorX;
        private int cursorY;

        public List<BoardPiece> draggedPieces;
        private int touchHeldFrames;
        private int disableTouchContextMenuUntilFrame;
        private bool draggedByTouch;
        private StorageSlot lastTouchedSlot;

        public Inventory otherInventory;
        private bool IgnoreUpdateAndDraw
        {
            get
            {
                if (this.type != Type.SingleBottom) return false;

                if (this.piece.world.mapMode == World.MapMode.Big || this.piece.world.CineMode || this.piece.world.demoMode) return true;

                Player player = this.piece.world.player;
                if (player.activeState == BoardPiece.State.PlayerControlledSleep || !player.alive || this.piece.world.SpectatorMode) return true;

                var ignoredTypes = new List<System.Type> { typeof(TextWindow), typeof(ControlTips), typeof(StackView), typeof(DebugScene), typeof(MessageLog), typeof(InfoWindow), typeof(TouchOverlay) };

                var stackToSearch = DrawStack.Where(scene => !ignoredTypes.Contains(scene.GetType())).ToList();
                stackToSearch.Reverse();
                Scene topScene = stackToSearch[0];

                return topScene.GetType() != typeof(Inventory);
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
                if (this.type != Type.SingleCenter) margin /= 3;
                return margin;
            }
        }
        private float BgMaxWidth
        {
            get
            {
                return this.type == Type.SingleCenter || this.type == Type.DualTop ? SonOfRobinGame.VirtualWidth * 0.8f : SonOfRobinGame.VirtualWidth * 0.37f;
            }
        }
        private float BgMaxHeight
        {
            get
            {
                if (this.type == Type.SingleBottom || this.type == Type.DualBottom) return SonOfRobinGame.VirtualHeight * 0.15f;
                if (this.type == Type.DualTop) return SonOfRobinGame.VirtualHeight * 0.5f;

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

        public Rectangle BgRect
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

        public static void SetLayout(Layout newLayout, BoardPiece chest = null, Player player = null)
        {
            RemoveAllScenesOfType(typeof(Inventory));

            switch (newLayout)
            {
                case Layout.None:
                    break;

                case Layout.Toolbar:
                    {
                        new Inventory(piece: player, storage: player.toolStorage, layout: Type.SingleBottom, inputType: InputTypes.Always, blocksUpdatesBelow: false, transDirection: TransDirection.Down);

                        break;
                    }

                case Layout.InventoryAndToolbar:
                    {
                        soundOpen.Play();

                        Inventory toolbar = new Inventory(piece: player, storage: player.toolStorage, layout: Type.DualBottom, transDirection: TransDirection.Down);
                        Inventory inventory = new Inventory(piece: player, storage: player.pieceStorage, layout: Type.DualTop, otherInventory: toolbar, transDirection: TransDirection.Up);
                        toolbar.otherInventory = inventory;

                        break;
                    }

                case Layout.InventoryAndChest:
                    {
                        Inventory inventoryLeft = new Inventory(piece: player, storage: player.pieceStorage, layout: Type.DualLeft, transDirection: TransDirection.Left);
                        Inventory inventoryRight = new Inventory(piece: chest, storage: chest.pieceStorage, layout: Type.DualRight, otherInventory: inventoryLeft, transDirection: TransDirection.Right);
                        inventoryLeft.otherInventory = inventoryRight;

                        break;
                    }

                case Layout.InventoryAndEquip:
                    {
                        soundOpen.Play();

                        Inventory inventoryLeft = new Inventory(piece: player, storage: player.pieceStorage, layout: Type.DualLeft, transDirection: TransDirection.Left);
                        Inventory inventoryRight = new Inventory(piece: player, storage: player.equipStorage, layout: Type.DualRight, otherInventory: inventoryLeft, transDirection: TransDirection.Right);
                        inventoryLeft.otherInventory = inventoryRight;

                        player.world.hintEngine.Disable(Tutorials.Type.Equip);
                        break;
                    }

                default:
                    throw new DivideByZeroException($"Unknown inventory layout '{newLayout}'.");
            }

            layout = newLayout;
        }

        public Inventory(PieceStorage storage, BoardPiece piece, TransDirection transDirection, Type layout = Type.SingleCenter, bool blocksUpdatesBelow = true, Inventory otherInventory = null, InputTypes inputType = InputTypes.Normal) : base(inputType: inputType, priority: 1, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Inventory, tipsLayout: ControlTips.TipsLayout.InventorySelect)
        {
            this.storage = storage;
            this.piece = piece;
            this.type = layout;
            this.transDirection = transDirection;

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
            this.disableTouchContextMenuUntilFrame = 0;
            this.draggedByTouch = false;
            this.lastTouchedSlot = null;
            this.otherInventory = otherInventory;

            this.UpdateViewParams();

            if (this.piece.GetType() == typeof(Container))
            {
                var container = (Container)piece;
                container.Open();
            }
            else this.piece.soundPack.Play(PieceSoundPack.Action.Open);

            if (!this.transManager.HasAnyTransition) this.transManager.AddMultipleTransitions(paramsToChange: this.GetTransitionsParams(), outTrans: false, duration: 12, refreshBaseVal: false);
        }

        private Dictionary<string, float> GetTransitionsParams()
        {
            var paramsToChange = new Dictionary<string, float> { { "Opacity", 0f } };

            switch (this.transDirection)
            {
                case TransDirection.Up:
                    paramsToChange["PosY"] = this.viewParams.PosY - SonOfRobinGame.VirtualHeight;
                    break;

                case TransDirection.Down:
                    paramsToChange["PosY"] = SonOfRobinGame.VirtualHeight;
                    break;

                case TransDirection.Left:
                    paramsToChange["PosX"] = this.viewParams.PosX - SonOfRobinGame.VirtualWidth;
                    break;

                case TransDirection.Right:
                    paramsToChange["PosX"] = SonOfRobinGame.VirtualWidth;
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported transDirection - {transDirection}.");
            }

            return paramsToChange;
        }

        public override void Remove()
        {
            this.ReleaseHeldPieces(slot: null, forceReleaseAll: true);
            SonOfRobinGame.hintWindow.TurnOff();

            if (!this.transManager.IsEnding)
            {
                if (this.piece.GetType() == typeof(Container))
                {
                    var container = (Container)piece;
                    container.Close();
                }

                this.transManager.AddMultipleTransitions(paramsToChange: this.GetTransitionsParams(),
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
                new InfoWindow.TextEntry(imageList: new List<Texture2D> {selectedPiece.sprite.frame.texture}, text: $"| {Helpers.FirstCharToUpperCase(selectedPiece.readableName)}" , color: Color.White, scale: 1.5f),
                new InfoWindow.TextEntry(text: selectedPiece.description, color: Color.White)
            };

            if (!slot.locked) entryList.Add(new InfoWindow.TextEntry(text: $"Max stack size: {selectedPiece.stackSize}.", color: Color.White));

            if (selectedPiece.buffList != null)
            {
                foreach (BuffEngine.Buff buff in selectedPiece.buffList)
                { entryList.Add(new InfoWindow.TextEntry(text: buff.description, color: buff.isPositive ? Color.Cyan : new Color(255, 120, 70), scale: 1f)); }
            }

            int margin = this.Margin;
            int tileSize = this.TileSize;
            InfoWindow hintWindow = SonOfRobinGame.hintWindow;

            Vector2 slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize) + new Vector2(this.viewParams.PosX, this.viewParams.PosY);
            Vector2 windowPos;
            Vector2 infoWindowSize = hintWindow.MeasureEntries(entryList);

            switch (this.type)
            {
                case Type.SingleCenter:
                    windowPos = new Vector2(
                        this.viewParams.PosX + this.viewParams.Width + margin,
                        slotPos.Y + (tileSize / 2) - (infoWindowSize.Y / 2));
                    break;

                case Type.SingleBottom:
                    return; // not used, anyway

                case Type.DualLeft:
                    windowPos = new Vector2(
                        this.viewParams.PosX + this.viewParams.Width + margin,
                        slotPos.Y + (tileSize / 2) - (infoWindowSize.Y / 2));
                    break;

                case Type.DualRight:
                    windowPos = new Vector2(
                        this.viewParams.PosX - margin - infoWindowSize.X,
                        slotPos.Y + (tileSize / 2) - (infoWindowSize.Y / 2));
                    break;

                case Type.DualTop:
                    windowPos = new Vector2(
                        slotPos.X + (tileSize / 2) - (infoWindowSize.X / 2),
                        this.viewParams.PosY + this.viewParams.Height + margin);
                    break;

                case Type.DualBottom:
                    windowPos = slotPos;
                    windowPos.X -= (infoWindowSize.X / 2) - (tileSize / 2);
                    windowPos.Y -= infoWindowSize.Y + (margin * 2);
                    break;

                default:
                    throw new DivideByZeroException($"Unsupported layout - {type}.");
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
            if (this.type != Type.SingleBottom && this.inputActive) this.UpdateHintWindow();
            this.ProcessInput();
            this.SetActivePieceAsIdentified();
            this.storage.lastUsedSlot = this.ActiveSlot;
        }

        private void SetActivePieceAsIdentified()
        {
            if (!this.inputActive || this.type == Type.SingleBottom) return;
            BoardPiece piece = this.ActiveSlot.TopPiece;
            if (piece == null) return;

            if (piece.world.identifiedPieces.Contains(piece.name)) return;

            piece.world.identifiedPieces.Add(piece.name);
        }

        private void UpdateViewParams()
        {
            Rectangle bgRect = this.BgRect;
            this.viewParams.Width = bgRect.Width;
            this.viewParams.Height = bgRect.Height;

            this.viewParams.CenterView();
            this.viewParams.Opacity = this.inputActive && this.type != Type.SingleBottom ? 1f : 0.75f;

            int centerX = SonOfRobinGame.VirtualWidth / 2;
            float posY;

            switch (this.type)
            {
                case Type.SingleCenter:
                    break;

                case Type.DualLeft:
                    this.viewParams.PosX = centerX - (SonOfRobinGame.VirtualWidth / 25) - this.viewParams.PosX;
                    break;

                case Type.DualRight:
                    this.viewParams.PosX = centerX + (SonOfRobinGame.VirtualWidth / 25);
                    break;

                case Type.DualTop:
                    this.viewParams.PosY = SonOfRobinGame.VirtualHeight * 0.1f;
                    break;

                case Type.SingleBottom:
                    posY = Preferences.ShowControlTips ? 0.95f : 1f; // little margin for ControlTips at the bottom
                    this.viewParams.PosY = (SonOfRobinGame.VirtualHeight * posY) - this.viewParams.Height;
                    break;

                case Type.DualBottom:
                    posY = Preferences.ShowControlTips ? 0.95f : 1f; // little margin for ControlTips at the bottom
                    this.viewParams.PosY = (SonOfRobinGame.VirtualHeight * posY) - this.viewParams.Height;
                    break;

                default:
                    throw new DivideByZeroException($"Unknown inventory type '{this.type}'.");
            }
        }
        private void KeepCursorInBoundsAndSwitchInv()
        {
            bool switchToSecondaryInv = false;

            switch (this.type)
            {
                case Type.SingleCenter:
                    if (this.cursorY <= -1) this.cursorY = this.storage.Height - 1;
                    if (this.cursorY >= this.storage.Height) this.cursorY = 0;

                    if (this.cursorX <= -1) this.cursorX = this.storage.Width - 1;
                    if (this.cursorX >= this.storage.Width) this.cursorX = 0;
                    break;


                case Type.DualLeft:
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

                case Type.DualRight:
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

                case Type.DualTop:
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

                case Type.DualBottom:
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

                case Type.SingleBottom:

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
                    throw new DivideByZeroException($"Unknown inventory layout '{this.type}'.");
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
            if (this.type == Type.SingleBottom) this.MoveCursorByBumpers();
            else
            {
                this.MoveCursorByNormalInput();
                if (this.draggedPieces.Count == 0) this.ProcessPieceSelectMode();
                else this.ProcessPieceDragMode();
            }
        }

        public void SetCursorByTouch()
        {
            int margin = this.Margin;
            int tileSize = this.TileSize;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = (touch.Position / Preferences.GlobalScale) - this.viewParams.DrawPos;

                foreach (StorageSlot slot in this.storage.AllSlots)
                {
                    Vector2 slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize);
                    Rectangle slotRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

                    if (slotRect.Contains(touchPos))
                    {
                        if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                        {
                            if (this.CursorX == slot.posX && this.CursorY == slot.posY) this.touchHeldFrames++;
                            else this.touchHeldFrames = 0;

                            if (slot.posX != this.CursorX || slot.posY != this.CursorY) soundNavigate.Play();

                            this.CursorX = slot.posX;
                            this.CursorY = slot.posY;

                            return;
                        }
                        else if (
                            touch.State == TouchLocationState.Released &&
                            this.touchHeldFrames < minFramesToDragByTouch &&
                            SonOfRobinGame.currentUpdate >= this.disableTouchContextMenuUntilFrame &&
                            this.type != Type.SingleBottom)
                        {
                            if (this.lastTouchedSlot == this.ActiveSlot) this.OpenPieceContextMenu();
                            else this.lastTouchedSlot = this.ActiveSlot;
                            return;
                        }
                    }
                }

            }

            this.touchHeldFrames = 0; // if no touch was registered
        }

        private void MoveCursorByNormalInput()
        {
            bool playSound = false;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalLeft))
            {
                this.CursorX -= 1;
                playSound = true;
            }
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalRight))
            {
                this.CursorX += 1;
                playSound = true;
            }
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalUp))
            {
                this.CursorY -= 1;
                playSound = true;
            }
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalDown))
            {
                this.CursorY += 1;
                playSound = true;
            }

            if (playSound) soundNavigate.Play();
        }

        private void MoveCursorByBumpers()
        {
            World world = World.GetTopWorld();
            if (world?.player?.activeState == BoardPiece.State.PlayerControlledShooting) return;

            bool playSound = false;

            if (InputMapper.HasBeenPressed(InputMapper.Action.ToolbarPrev))
            {
                this.CursorX -= 1;
                playSound = true;
            }
            if (InputMapper.HasBeenPressed(InputMapper.Action.ToolbarNext))
            {
                this.CursorX += 1;
                playSound = true;
            }

            if (playSound) soundNavigate.Play();
        }

        private void OpenPieceContextMenu()
        {
            soundEnterContextMenu.Play();

            StorageSlot slot = this.ActiveSlot;
            if (slot == null) return;
            BoardPiece piece = this.storage.GetTopPiece(slot: slot);
            if (piece == null) return;

            var lockedButWorking = new List<PieceTemplate.Name> { PieceTemplate.Name.CookingTrigger, PieceTemplate.Name.FireplaceTriggerOn, PieceTemplate.Name.FireplaceTriggerOff, PieceTemplate.Name.UpgradeTrigger };
            if (slot.locked && !lockedButWorking.Contains(piece.name)) return;

            Vector2 slotPos = this.GetSlotPos(slot: slot, margin: this.Margin, tileSize: this.TileSize);
            slotPos += new Vector2(this.viewParams.PosX, this.viewParams.PosY);
            slotPos.X += this.Margin + this.TileSize;
            Vector2 percentPos = new Vector2(slotPos.X / SonOfRobinGame.VirtualWidth, slotPos.Y / SonOfRobinGame.VirtualHeight);

            bool addMove = this.type != Type.SingleCenter && !slot.locked && this.otherInventory.storage.CanFitThisPiece(piece);
            bool addDrop = !slot.locked;
            bool addCook = piece.name == PieceTemplate.Name.CookingTrigger;
            bool addIgnite = piece.name == PieceTemplate.Name.FireplaceTriggerOn;
            bool addExtinguish = piece.name == PieceTemplate.Name.FireplaceTriggerOff;
            bool addCombine = piece.name == PieceTemplate.Name.UpgradeTrigger;

            new PieceContextMenu(piece: piece, storage: this.storage, slot: slot, percentPosX: percentPos.X, percentPosY: percentPos.Y, addMove: addMove, addDrop: addDrop, addCook: addCook, addIgnite: addIgnite, addExtinguish: addExtinguish, addUpgrade: addCombine);
            return;
        }
        private void MoveOtherInventoryToTop()
        {
            if (this.otherInventory != null)
            {
                soundSwitch.Play();
                this.otherInventory.MoveToTop();
            }
        }

        private void ProcessPieceSelectMode()
        {
            this.tipsLayout = ControlTips.TipsLayout.InventorySelect;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                // must go first, to read touch return button!
                SetLayout(newLayout: Layout.Toolbar, player: this.storage.world.player);
                return;
            }

            if (this.SwitchToSecondInventoryByTouch()) return;
            if (this.ExitByOutsideTouch()) return;

            if (InputMapper.HasBeenPressed(InputMapper.Action.InvSort))
            {
                soundNavigate.Play();
                this.storage.Sort();
                return;
            }

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

                if ((this.draggedByTouch && !VirtButton.IsButtonDown(VButName.InvDragSingle)) ||
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

                if (pickedUpPieces.Count > 0)
                {
                    soundPickUp.Play();
                    this.draggedPieces = pickedUpPieces;
                }
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

        private bool SwitchToSecondInventoryByTouch()
        {
            if (this.otherInventory == null) return false;

            Rectangle otherInvBgRect = this.otherInventory.BgRect;
            otherInvBgRect.X += (int)this.otherInventory.viewParams.drawPosX;
            otherInvBgRect.Y += (int)this.otherInventory.viewParams.drawPosY;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = touch.Position / Preferences.GlobalScale;
                if (otherInvBgRect.Contains(touchPos))
                {
                    this.lastTouchedSlot = null;
                    this.MoveOtherInventoryToTop();
                    return true;
                }
            }

            return false;
        }

        private bool ExitByOutsideTouch()
        {
            if (Preferences.EnableTouchButtons || this.type == Type.SingleBottom) return false;

            var pressTouches = TouchInput.TouchPanelState.Where(touch => touch.State == TouchLocationState.Pressed).ToList();
            if (pressTouches.Count == 0) return false;

            int inflateSize = (int)(SonOfRobinGame.VirtualHeight * 0.02);

            Rectangle thisInvBgRect = this.BgRect;
            thisInvBgRect.X += (int)this.viewParams.drawPosX;
            thisInvBgRect.Y += (int)this.viewParams.drawPosY;
            thisInvBgRect.Inflate(inflateSize, inflateSize);

            Rectangle otherInvBgRect = this.otherInventory.BgRect;
            otherInvBgRect.X += (int)this.otherInventory.viewParams.drawPosX;
            otherInvBgRect.Y += (int)this.otherInventory.viewParams.drawPosY;
            otherInvBgRect.Inflate(inflateSize, inflateSize);

            foreach (TouchLocation touch in pressTouches)
            {
                Vector2 touchPos = touch.Position / Preferences.GlobalScale;
                if (!thisInvBgRect.Contains(touchPos) && !otherInvBgRect.Contains(touchPos))
                {
                    SetLayout(newLayout: Layout.Toolbar, player: this.storage.world.player);
                    return true;
                }
            }

            return false;
        }

        private void ProcessPieceDragMode()
        {
            this.tipsLayout = ControlTips.TipsLayout.InventoryDrag;

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                SetLayout(newLayout: Layout.Toolbar, player: this.storage.world.player);
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

            this.draggedPieces[0].soundPack.Play(action: PieceSoundPack.Action.IsDropped, ignore3D: true, ignoreCooldown: true);

            //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"ReleaseHeldPieces");

            int initialDraggedCount = this.draggedPieces.Count;
            PieceTemplate.Name initialTopPieceName = this.draggedPieces[0].name;

            var piecesThatDidNotFitIn = new List<BoardPiece> { };

            //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"forceReleaseAll {forceReleaseAll}");

            foreach (BoardPiece piece in this.draggedPieces)
            {
                if (slot != null && slot.CanFitThisPiece(piece)) slot.AddPiece(piece);
                else
                {
                    if (forceReleaseAll)
                    {
                        if (!this.storage.AddPiece(piece)) piecesThatDidNotFitIn.Add(piece);
                    }
                    else piecesThatDidNotFitIn.Add(piece);
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

            if (this.draggedByTouch)
            {
                this.ReleaseHeldPieces(slot: slot, forceReleaseAll: true); // in case of touch (or mouse) drag, dragged pieces should be released after swap
                this.disableTouchContextMenuUntilFrame = SonOfRobinGame.currentUpdate + 15;
            }
        }

        private void SwapDraggedAndSlotPieces(StorageSlot slot)
        {
            var slotPieces = this.storage.RemoveAllPiecesFromSlot(slot: slot);

            bool swapPossible = slot.CanFitThisPiece(piece: this.draggedPieces[0], pieceCount: this.draggedPieces.Count);
            if (swapPossible)
            {
                foreach (BoardPiece piece in this.draggedPieces)
                { slot.AddPiece(piece); }

                this.draggedPieces[0].soundPack.Play(action: PieceSoundPack.Action.IsDropped, ignore3D: true, ignoreCooldown: true);
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

            this.DrawMainLabel();

            Rectangle bgRect = this.BgRect;

            if (this.type != Type.SingleBottom)
            {
                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, Color.BlanchedAlmond * 0.7f * this.viewParams.drawOpacity);
                Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);
            }

            foreach (StorageSlot slot in this.storage.AllSlots)
            {
                if (slot.hidden) continue;

                Vector2 slotPos = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize);

                bool isActive = this.inputActive && slot.posX == this.CursorX && slot.posY == this.CursorY;
                Rectangle tileRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

                Color outlineColor = isActive ? Color.LawnGreen : Color.White;
                Color fillColor = isActive ? Color.LightSeaGreen : Color.White;

                SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, tileRect, fillColor * 0.35f * this.viewParams.drawOpacity);
                Helpers.DrawRectangleOutline(rect: tileRect, color: outlineColor * this.viewParams.drawOpacity * 0.8f, borderWidth: 2);

                this.DrawSlotLabel(slot: slot, tileRect: tileRect);

                Rectangle destRect = isActive ? tileRect : new Rectangle((int)slotPos.X + spriteOffset, (int)slotPos.Y + spriteOffset, spriteSize, spriteSize);
                slot.Draw(destRect: destRect, opacity: this.viewParams.drawOpacity, drawNewIcon: this.type != Type.SingleBottom);

                Rectangle quantityRect = new Rectangle(x: tileRect.X, y: tileRect.Y + (tileRect.Height / 2), width: tileRect.Width, height: tileRect.Height / 2);
                DrawQuantity(pieceCount: slot.PieceCount, destRect: quantityRect, opacity: this.viewParams.drawOpacity);
            }

            if (this.InputType != InputTypes.Always) this.DrawCursor();
        }

        private void DrawSlotLabel(StorageSlot slot, Rectangle tileRect)
        {
            if (slot.label == "" || !slot.IsEmpty) return;

            Rectangle labelRect = tileRect;
            labelRect.Inflate(-(int)(tileRect.Width * 0.1), -(int)(tileRect.Height * 0.4));

            Helpers.DrawTextInsideRectWithOutline(font: font, text: slot.label, rectangle: labelRect, color: Color.White, outlineColor: new Color(50, 50, 50), outlineSize: 1, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center, drawTestRect: false);
        }

        private void DrawMainLabel()
        {
            if (this.type == Type.SingleBottom) return;

            Rectangle bgRect = this.BgRect;

            string label = Convert.ToString(this.storage.storageType);

            Vector2 labelSize = font.MeasureString(label);
            float maxTextWidth = bgRect.Width * 0.3f;
            float maxTextHeight = bgRect.Height * 0.1f;
            float textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);

            float textWidth = labelSize.X * textScale;
            float textHeight = labelSize.Y * textScale;

            Vector2 labelPos = new Vector2((bgRect.Width - textWidth) / 2, bgRect.Top - textHeight);
            float shadowOffset = textHeight * 0.06f;
            Vector2 shadowPos = labelPos + new Vector2(shadowOffset, shadowOffset);

            SonOfRobinGame.spriteBatch.DrawString(font, label, position: shadowPos, color: Color.Black * 0.5f * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
            SonOfRobinGame.spriteBatch.DrawString(font, label, position: labelPos, color: Color.White * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        public static void DrawQuantity(int pieceCount, Rectangle destRect, float opacity, bool ignoreSingle = true)
        {
            if (ignoreSingle && pieceCount <= 1) return;

            string countTxt = $"x{pieceCount}";

            Helpers.DrawTextInsideRectWithOutline(font: font, text: countTxt, rectangle: destRect, color: Color.White * opacity, outlineColor: Color.Black * opacity, outlineSize: 1, alignX: Helpers.AlignX.Left, alignY: Helpers.AlignY.Bottom);
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

                Rectangle quantityRect = new Rectangle(x: shownPieceRect.X, y: shownPieceRect.Y + (shownPieceRect.Height / 2), width: shownPieceRect.Width, height: shownPieceRect.Height / 2);
                DrawQuantity(pieceCount: draggedPieces.Count, destRect: quantityRect, opacity: this.viewParams.drawOpacity);
            }

            SonOfRobinGame.spriteBatch.Draw(cursorTexture, destinationRectangle, sourceRectangle, Color.White);
        }

    }
}
