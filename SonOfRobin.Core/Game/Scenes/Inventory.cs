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
        public enum LayoutType : byte
        {
            None = 0,
            Toolbar = 1,
            Inventory = 2,
            FieldStorage = 3,
        }

        public enum TransDirection : byte
        {
            Up = 0,
            Down = 1,
            Left = 2,
            Right = 3,
        }

        public enum Type : byte
        {
            SingleCenter = 0,
            SingleBottom = 1,
            DualLeft = 2,
            DualRight = 3,
            DualTop = 4,
            DualBottom = 5,
        }

        private const int minFramesToDragByTouch = 15;
        private const float marginPercent = 0.01f;

        private static readonly SpriteFont font = SonOfRobinGame.FontTommy40;

        private static readonly Sound soundOpen = new Sound(SoundData.Name.InventoryOpen);
        private static readonly Sound soundNavigate = new Sound(SoundData.Name.Navigation);
        private static readonly Sound soundSwitch = new Sound(SoundData.Name.Select);
        private static readonly Sound soundEnterContextMenu = new Sound(SoundData.Name.Invoke);
        private static readonly Sound soundPickUp = new Sound(SoundData.Name.PickUpItem, volume: 0.8f);
        public static readonly Sound soundCombine = new Sound(SoundData.Name.AnvilHit);
        public static readonly Sound soundApplyPotion = new Sound(SoundData.Name.Drink);

        public static LayoutType Layout { get; private set; } = LayoutType.None;

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

        public Inventory(PieceStorage storage, BoardPiece piece, TransDirection transDirection,
            Type layout = Type.SingleCenter, bool blocksUpdatesBelow = true, Inventory otherInventory = null, InputTypes inputType = InputTypes.Normal) :

            base(inputType: inputType, priority: 1, blocksUpdatesBelow: blocksUpdatesBelow, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Inventory, tipsLayout: ControlTips.TipsLayout.InventorySelect)
        {
            this.storage = storage;
            this.piece = piece;
            this.type = layout;
            this.transDirection = transDirection;

            if (this.storage.lastUsedSlot != null)
            {
                Point lastUsedSlotPos = this.storage.GetSlotPos(this.storage.lastUsedSlot);

                this.cursorX = lastUsedSlotPos.X;
                this.cursorY = lastUsedSlotPos.Y;
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

            this.UpdateViewParams(); // needed for transition

            if (this.piece.GetType() == typeof(Container))
            {
                var container = (Container)piece;
                container.Open();
            }
            else this.piece.soundPack.Play(PieceSoundPack.Action.Open);

            if (!this.transManager.HasAnyTransition) this.transManager.AddMultipleTransitions(paramsToChange: this.GetTransitionsParams(), outTrans: false, duration: 12, refreshBaseVal: false);
        }

        private bool IgnoreUpdateAndDraw
        {
            get
            {
                if (this.type != Type.SingleBottom) return false;

                if (this.piece.world.map.FullScreen || this.piece.world.CineMode || this.piece.world.demoMode) return true;

                Player player = this.piece.world.Player;
                if (player.activeState == BoardPiece.State.PlayerControlledSleep || !player.alive || this.piece.world.SpectatorMode) return true;

                var ignoredTypes = new List<System.Type> { typeof(TextWindow), typeof(ControlTips), typeof(StackView), typeof(DebugScene), typeof(MessageLog), typeof(InfoWindow), typeof(FullScreenProgressBar), typeof(TouchOverlay), typeof(FpsCounter) };

                var stackToSearch = DrawStack.Where(scene => !ignoredTypes.Contains(scene.GetType())).ToList();
                if (stackToSearch.Count == 0) return true;

                stackToSearch.Reverse();
                Scene topScene = stackToSearch.First();

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
                int margin = (int)Math.Min(SonOfRobinGame.VirtualWidth * marginPercent, SonOfRobinGame.VirtualHeight * marginPercent);
                return Math.Max(margin, 3);
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

                return SonOfRobinGame.VirtualHeight * 0.8f;
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

                float tileSize = Math.Min(maxTileWidth, maxTileHeight);

                return (int)tileSize;
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

        public static void SetLayout(LayoutType newLayout, BoardPiece fieldStorage = null, Player player = null)
        {
            if (Layout == newLayout)
            {
                Inventory topInventory = GetTopInventory();
                if (topInventory?.piece.world == World.GetTopWorld()) return;
            }

            if (fieldStorage != null && fieldStorage.PieceStorage.storageType != PieceStorage.StorageType.Fireplace && player != null && !player.CanSeeAnything)
            {
                new TextWindow(text: $"It is too dark to use the | {fieldStorage.readableName}...", imageList: new List<Texture2D> { PieceInfo.GetTexture(fieldStorage.name) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: player.world.DialogueSound);

                return;
            }

            RemoveAllScenesOfType(typeof(Inventory));

            switch (newLayout)
            {
                case LayoutType.None:
                    break;

                case LayoutType.Toolbar:
                    {
                        new Inventory(piece: player, storage: player.ToolStorage, layout: Type.SingleBottom, inputType: InputTypes.Always, blocksUpdatesBelow: false, transDirection: TransDirection.Down);

                        break;
                    }

                case LayoutType.Inventory:
                    {
                        soundOpen.Play();

                        var virtStoragePackList = new List<VirtPieceStoragePack>
                        {
                            new VirtPieceStoragePack(storage: player.PieceStorage),
                            new VirtPieceStoragePack(storage: player.ToolStorage, newRow: true),
                        };
                        PieceStorage virtualStorage = new VirtualPieceStorage(storagePiece: player, virtStoragePackList: virtStoragePackList, label: "Inventory", padding: 1);

                        Inventory inventory = new Inventory(piece: player, storage: player.EquipStorage, layout: Type.DualRight, transDirection: TransDirection.Right);

                        Inventory equip = new Inventory(piece: player, storage: virtualStorage, layout: Type.DualLeft, otherInventory: inventory, transDirection: TransDirection.Left);

                        inventory.otherInventory = equip;

                        break;
                    }

                case LayoutType.FieldStorage:
                    {
                        var virtStoragePackList = new List<VirtPieceStoragePack>
                        {
                            new VirtPieceStoragePack(storage: player.PieceStorage),
                            new VirtPieceStoragePack(storage: player.ToolStorage, newRow: true),
                        };
                        PieceStorage virtualStorage = new VirtualPieceStorage(storagePiece: player, virtStoragePackList: virtStoragePackList, label: "Inventory", padding: 1);

                        Inventory inventoryLeft = new Inventory(piece: player, storage: virtualStorage, layout: Type.DualLeft, transDirection: TransDirection.Left);
                        Inventory inventoryRight = new Inventory(piece: fieldStorage, storage: fieldStorage.PieceStorage, layout: Type.DualRight, otherInventory: inventoryLeft, transDirection: TransDirection.Right);
                        inventoryLeft.otherInventory = inventoryRight;

                        break;
                    }

                default:
                    throw new ArgumentException($"Unknown inventory layout '{newLayout}'.");
            }

            Layout = newLayout;
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
                    throw new ArgumentException($"Unsupported transDirection - {transDirection}.");
            }

            return paramsToChange;
        }

        public override void Remove()
        {
            this.ReleaseHeldPieces(slot: null, forceReleaseAll: true);
            SonOfRobinGame.HintWindow.TurnOff();

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
                SonOfRobinGame.HintWindow.TurnOff();
                return;
            }

            var entryList = new List<InfoWindow.TextEntry> {
                new InfoWindow.TextEntry(imageList: new List<Texture2D> { AnimData.framesForPkgs[selectedPiece.sprite.AnimPackage].texture }, text: $"| {Helpers.FirstCharToUpperCase(selectedPiece.readableName)}" , color: Color.White, scale: 1.5f), // AnimData.framesForPkgs is used for texture, to avoid animating (jitter)
                new InfoWindow.TextEntry(text: selectedPiece.description, color: Color.White)
            };

            float smallScale = 0.7f;

            var extInfoTextList = new List<string>();
            var extInfoImageList = new List<Texture2D>();

            PieceInfo.Info pieceInfo = selectedPiece.pieceInfo;

            if (selectedPiece.pieceInfo.toolbarTask == Scheduler.TaskName.GetEaten)
            {
                float fedPercent = (float)Math.Round(this.piece.world.Player.ConvertMassToFedPercent(selectedPiece.Mass) * 100, 1);
                extInfoTextList.Add($"| +{fedPercent}%");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleBurger));
            }

            var durabilityTypeList = new List<System.Type> { typeof(Tool), typeof(PortableLight), typeof(Projectile) };
            if (durabilityTypeList.Contains(selectedPiece.GetType()))
            {
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleHeart));

                if (selectedPiece.pieceInfo.toolIndestructible)
                {
                    extInfoTextList.Add($"|  |");
                    extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleInfinity));
                }
                else extInfoTextList.Add($"| {Math.Round(selectedPiece.HitPoints)}/{Math.Round(selectedPiece.maxHitPoints)}");
            }

            if (selectedPiece.pieceInfo.toolShootsProjectile)
            {
                extInfoTextList.Add($"| {Math.Round(60f / (float)pieceInfo.toolHitCooldown, 1)}/s");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleSpeed));
            }

            if (pieceInfo.toolRange > 0)
            {
                extInfoTextList.Add($"| {pieceInfo.toolRange}");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleArea));
            }

            if (selectedPiece.StackSize > 1 && !slot.locked)
            {
                extInfoTextList.Add($"| {selectedPiece.StackSize}");
                extInfoImageList.Add(TextureBank.GetTexture(TextureBank.TextureName.SimpleStack));
            }

            if (extInfoTextList.Count > 0)
            {
                entryList.Add(new InfoWindow.TextEntry(text: String.Join("  ", extInfoTextList), imageList: extInfoImageList, scale: smallScale, color: new Color(230, 230, 230)));
            }

            if (selectedPiece.GetType() == typeof(Seed))
            {
                Seed seeds = (Seed)selectedPiece;

                entryList.Add(new InfoWindow.TextEntry(text: $"| {Helpers.FirstCharToUpperCase(PieceInfo.GetInfo(seeds.PlantToGrow).readableName)} seeds.", imageList: new List<Texture2D> { PieceInfo.GetInfo(seeds.PlantToGrow).texture }, scale: smallScale, color: new Color(208, 255, 199)));
            }

            if (selectedPiece.GetType() == typeof(Projectile))
            {
                entryList.Add(new InfoWindow.TextEntry(text: $"| {selectedPiece.pieceInfo.projectileHitMultiplier}", imageList: new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.Biceps) }, scale: smallScale, color: Color.White));
            }

            var affinityEntries = PieceInfo.GetCategoryAffinityTextEntryList(pieceName: selectedPiece.name, scale: 1f);
            entryList.AddRange(affinityEntries);

            var combineEntries = PieceInfo.GetCombinesWithTextEntryList(pieceName: selectedPiece.name, scale: 0.7f);
            entryList.AddRange(combineEntries);

            if (selectedPiece.buffList != null && selectedPiece.buffList.Count > 0)
            {
                if (selectedPiece.pieceInfo.CanHurtAnimals) entryList.Add(new InfoWindow.TextEntry(text: "Target receives:", color: Color.White, scale: smallScale));

                foreach (Buff buff in selectedPiece.buffList)
                { entryList.Add(new InfoWindow.TextEntry(text: buff.description, color: buff.isPositive ? Color.Cyan : new Color(255, 120, 70), scale: 1f)); }
            }

            int margin = this.Margin;
            int tileSize = this.TileSize;
            InfoWindow hintWindow = SonOfRobinGame.HintWindow;

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
                    throw new ArgumentException($"Unsupported layout - {type}.");
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

        protected override void AdaptToNewSize()
        {
            this.UpdateViewParams();
        }

        public override void Update()
        {
            if (this.IgnoreUpdateAndDraw) return;

            this.storage.Update();
            if (this.otherInventory != null)
            {
                this.otherInventory.storage.Update();
                this.otherInventory.UpdateViewParams();
            }

            if (this.CursorX >= this.storage.Width) this.CursorX = this.storage.Width - 1; // in case storage was resized
            if (this.CursorY >= this.storage.Height) this.CursorY = this.storage.Height - 1; // in case storage was resized
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

        public void UpdateViewParams()
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
                    throw new ArgumentException($"Unknown inventory type '{this.type}'.");
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
                    throw new ArgumentException($"Unknown inventory layout '{this.type}'.");
            }

            if (switchToSecondaryInv)
            {
                this.MoveOtherInventoryToTop();
                this.MoveDraggedPiecesToOtherInv();
            }
        }

        private void ProcessInput()
        {
            if (!this.piece.world.Player.alive) return;

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
                            Point currentSlotPos = this.storage.GetSlotPos(slot);

                            if (this.CursorX == currentSlotPos.X && this.CursorY == currentSlotPos.Y) this.touchHeldFrames++;
                            else this.touchHeldFrames = 0;

                            if (currentSlotPos.X != this.CursorX || currentSlotPos.Y != this.CursorY) soundNavigate.Play();

                            this.CursorX = currentSlotPos.X;
                            this.CursorY = currentSlotPos.Y;

                            return;
                        }
                        else if (
                            touch.State == TouchLocationState.Released &&
                            this.touchHeldFrames < minFramesToDragByTouch &&
                            SonOfRobinGame.CurrentUpdate >= this.disableTouchContextMenuUntilFrame &&
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
            if (world?.Player?.activeState == BoardPiece.State.PlayerControlledShooting) return;

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

            var lockedButWorking = new List<PieceTemplate.Name> { PieceTemplate.Name.CookingTrigger, PieceTemplate.Name.BrewTrigger, PieceTemplate.Name.FireplaceTriggerOn, PieceTemplate.Name.FireplaceTriggerOff, PieceTemplate.Name.MeatHarvestTrigger, PieceTemplate.Name.OfferTrigger };
            if (slot.locked && !lockedButWorking.Contains(piece.name)) return;

            Vector2 slotPos = this.GetSlotPos(slot: slot, margin: this.Margin, tileSize: this.TileSize);
            slotPos += new Vector2(this.viewParams.PosX, this.viewParams.PosY);
            slotPos.X += this.Margin + this.TileSize;
            Vector2 percentPos = new(slotPos.X / SonOfRobinGame.VirtualWidth, slotPos.Y / SonOfRobinGame.VirtualHeight);

            bool addEquip = this.type != Type.SingleCenter && piece.GetType() == typeof(Equipment) && !slot.locked && this.otherInventory.storage.storageType == PieceStorage.StorageType.Equip;
            bool addMove = !addEquip && this.type != Type.SingleCenter && !slot.locked && this.otherInventory.storage.CanFitThisPiece(piece);
            bool addDrop = !slot.locked;
            bool addCook = piece.name == PieceTemplate.Name.CookingTrigger;
            bool addBrew = piece.name == PieceTemplate.Name.BrewTrigger;
            bool addIgnite = piece.name == PieceTemplate.Name.FireplaceTriggerOn;
            bool addExtinguish = piece.name == PieceTemplate.Name.FireplaceTriggerOff;
            bool addHarvest = piece.name == PieceTemplate.Name.MeatHarvestTrigger;
            bool addFieldHarvest = piece.GetType() == typeof(Animal) && !piece.alive && piece.world.Player.Skill == Player.SkillName.Hunter;
            bool addOffer = piece.name == PieceTemplate.Name.OfferTrigger;

            new PieceContextMenu(piece: piece, storage: this.storage, slot: slot, percentPosX: percentPos.X, percentPosY: percentPos.Y, addEquip: addEquip, addMove: addMove, addDrop: addDrop, addCook: addCook, addBrew: addBrew, addIgnite: addIgnite, addExtinguish: addExtinguish, addHarvest: addHarvest, addFieldHarvest: addFieldHarvest, addOffer: addOffer);
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
                SetLayout(newLayout: LayoutType.Toolbar, player: this.storage.world.Player);
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
                    new RumbleEvent(force: 0.17f, durationSeconds: 0, smallMotor: true, fadeInSeconds: 0.035f, fadeOutSeconds: 0.035f);

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
            if (Preferences.EnableTouchButtons || this.otherInventory == null) return false;

            var pressTouches = TouchInput.TouchPanelState.Where(touch => touch.State == TouchLocationState.Pressed);
            if (pressTouches.Count() == 0) return false;

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
                    SetLayout(newLayout: LayoutType.Toolbar, player: this.storage.world.Player);
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
                SetLayout(newLayout: LayoutType.Toolbar, player: this.storage.world.Player);
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

        public void ReleaseHeldPieces(StorageSlot slot, bool forceReleaseAll = false)
        {
            if (this.draggedPieces.Count == 0) return;

            //MessageLog.AddMessage(msgType: MsgType.Debug, message: $"ReleaseHeldPieces");

            PieceSoundPack firstPieceSoundPack = this.draggedPieces[0].soundPack;

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

            if (this.draggedPieces.Count == initialDraggedCount && this.draggedPieces[0].name == initialTopPieceName)
            {
                if (this.TryToCombinePieces(slot)) return; // will display a confirmation menu, if combination is possible
                if (this.TryToApplyPotion(slot: slot, execute: false)) return; // will display a confirmation menu, if combination is possible
                this.SwapDraggedAndSlotPieces(slot: slot);
            }

            if (this.draggedByTouch)
            {
                this.ReleaseHeldPieces(slot: slot, forceReleaseAll: true); // in case of touch (or mouse) drag, dragged pieces should be released after swap
                this.disableTouchContextMenuUntilFrame = SonOfRobinGame.CurrentUpdate + 15;
            }

            PieceTemplate.Name topPieceName = this.draggedPieces.Count > 0 ? this.draggedPieces[0].name : PieceTemplate.Name.Empty;
            if (this.draggedPieces.Count == initialDraggedCount && topPieceName == initialTopPieceName) Sound.QuickPlay(SoundData.Name.Error);
            else
            {
                firstPieceSoundPack.Play(action: PieceSoundPack.Action.IsDropped, ignore3D: true, ignoreCooldown: true);
                new RumbleEvent(force: 0.12f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.06f, fadeOutSeconds: 0.06f);
            }
        }

        private bool TryToCombinePieces(StorageSlot slot)
        {
            if (this.draggedPieces.Count == 0 || slot.PieceCount == 0) return false;

            BoardPiece combinedPiece = PieceCombiner.TryToCombinePieces(piece1: this.draggedPieces[0], piece2: slot.TopPiece);
            if (combinedPiece != null)
            {
                var optionList = new List<object>();
                optionList.Add(new Dictionary<string, object> { { "label", "yes" }, { "taskName", Scheduler.TaskName.InventoryCombineItems }, { "executeHelper", this } });
                optionList.Add(new Dictionary<string, object> { { "label", "no" }, { "taskName", this.draggedByTouch ? Scheduler.TaskName.InventoryReleaseHeldPieces : Scheduler.TaskName.Empty }, { "executeHelper", this } });

                var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", "Combine items?" }, { "customOptionList", optionList } };
                new Scheduler.Task(taskName: Scheduler.TaskName.OpenConfirmationMenu, turnOffInputUntilExecution: true, executeHelper: confirmationData);

                return true;
            }

            return false;
        }

        public bool TryToApplyPotion(StorageSlot slot, bool execute)
        {
            if (this.draggedPieces.Count == 0 || slot.PieceCount == 0) return false;
            if (this.draggedPieces.Count > 1 && slot.PieceCount > 1) return false;
            if (this.draggedPieces[0].name != PieceTemplate.Name.PotionGeneric && slot.pieceList[0].name != PieceTemplate.Name.PotionGeneric) return false;

            Potion potion;
            List<BoardPiece> targetPieces;
            bool potionInsideSlot;

            if (this.draggedPieces[0].name == PieceTemplate.Name.PotionGeneric)
            {
                potion = (Potion)this.draggedPieces[0];
                targetPieces = slot.pieceList;
                potionInsideSlot = false;
            }
            else
            {
                potion = (Potion)slot.pieceList[0];
                targetPieces = this.draggedPieces;
                potionInsideSlot = true;
            }

            var piecesThatCanReceiveBuffs = targetPieces.Where(
                piece => piece.buffList.Count == 0 &&
                piece.pieceInfo.CanHurtAnimals)
                .ToList();

            if (piecesThatCanReceiveBuffs.Count == 0) return false;

            var allowedBuffTypes = new List<BuffEngine.BuffType> { BuffEngine.BuffType.RegenPoison, BuffEngine.BuffType.Speed, BuffEngine.BuffType.Strength };
            var buffsThatCanBeMoved = potion.buffList.Where(buff => allowedBuffTypes.Contains(buff.type) && !buff.isPositive).ToList();
            if (buffsThatCanBeMoved.Count == 0) return false;

            string counterText = targetPieces.Count > 1 ? $" x{targetPieces.Count}" : "";

            if (!execute)
            {
                var optionList = new List<object>();
                optionList.Add(new Dictionary<string, object> { { "label", "yes" }, { "taskName", Scheduler.TaskName.InventoryApplyPotion }, { "executeHelper", this } });
                optionList.Add(new Dictionary<string, object> { { "label", "no" }, { "taskName", this.draggedByTouch ? Scheduler.TaskName.InventoryReleaseHeldPieces : Scheduler.TaskName.Empty }, { "executeHelper", this } });

                var confirmationData = new Dictionary<string, Object> { { "blocksUpdatesBelow", true }, { "question", $"Apply {potion.readableName} to {targetPieces[0].readableName}{counterText}?" }, { "customOptionList", optionList } };
                new Scheduler.Task(taskName: Scheduler.TaskName.OpenConfirmationMenu, turnOffInputUntilExecution: true, executeHelper: confirmationData);
            }
            else // execute == true
            {
                foreach (BoardPiece receivingPiece in piecesThatCanReceiveBuffs)
                {
                    foreach (Buff buff in buffsThatCanBeMoved)
                    {
                        receivingPiece.buffList.Add(Buff.CopyBuff(buff));
                    }

                    if (buffsThatCanBeMoved.Count > 0)
                    {
                        string plusSign = buffsThatCanBeMoved.Count > 1 ? "+" : "";
                        receivingPiece.readableName = $"{receivingPiece.readableName} of {buffsThatCanBeMoved[0].PotionText}{plusSign}";
                    }
                }

                BoardPiece emptyContainter = PieceTemplate.Create(templateName: potion.convertsToWhenUsed, world: potion.world);

                if (potionInsideSlot) slot.DestroyPieceAndReplaceWithAnother(emptyContainter);
                else
                {
                    this.draggedPieces.Clear();
                    this.draggedPieces.Add(emptyContainter);
                }

                soundApplyPotion.Play();
                new RumbleEvent(force: 0.27f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.085f, fadeOutSeconds: 0.085f);

                // pieceInfo.readableName is used to show original name (before the change)
                new TextWindow(text: $"{Helpers.FirstCharToUpperCase(potion.readableName)} | has been used on | {targetPieces[0].pieceInfo.readableName}{counterText}.", imageList: new List<Texture2D> { potion.sprite.AnimFrame.texture, targetPieces[0].sprite.AnimFrame.texture }, textColor: Color.White, bgColor: new Color(0, 214, 222), useTransition: true, animate: true);
            }

            return true;
        }

        private void SwapDraggedAndSlotPieces(StorageSlot slot)
        {
            if (!slot.CanFitThisPiece(piece: this.draggedPieces[0], pieceCount: this.draggedPieces.Count, treatSlotAsEmpty: true)) return;

            var slotPieces = this.storage.RemoveAllPiecesFromSlot(slot: slot);

            foreach (BoardPiece piece in this.draggedPieces)
            { slot.AddPiece(piece); }

            this.draggedPieces[0].soundPack.Play(action: PieceSoundPack.Action.IsDropped, ignore3D: true, ignoreCooldown: true);
            new RumbleEvent(force: 0.12f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.035f, fadeOutSeconds: 0.035f);

            this.draggedPieces = slotPieces;
        }

        public static Inventory GetTopInventory()
        {
            var inventoryScene = GetTopSceneOfType(typeof(Inventory));
            return inventoryScene == null ? null : (Inventory)inventoryScene;
        }

        public override void Draw()
        {
            if (this.IgnoreUpdateAndDraw) return;

            float opacity = Math.Max(1f - ((this.piece.world).cineCurtains.showPercentage * 2f), 0) * this.viewParams.drawOpacity;

            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            int margin = this.Margin;
            int tileSize = this.TileSize;
            int spriteSize = Convert.ToInt32((float)tileSize * 0.7f);
            int spriteOffset = (tileSize - spriteSize) / 2;

            this.DrawMainLabel(opacity: opacity);

            Rectangle bgRect = this.BgRect;

            if (this.type != Type.SingleBottom)
            {
                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bgRect, Color.BlanchedAlmond * 0.7f * opacity);
                Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * opacity, borderWidth: 2);
            }

            foreach (StorageSlot slot in this.storage.AllSlots)
            {
                if (slot.hidden) continue;

                Vector2 slotPosWithMargin = this.GetSlotPos(slot: slot, margin: margin, tileSize: tileSize);

                Point slotPos = this.storage.GetSlotPos(slot);

                bool isActive = this.inputActive && slotPos.X == this.CursorX && slotPos.Y == this.CursorY;
                Rectangle tileRect = new Rectangle((int)slotPosWithMargin.X, (int)slotPosWithMargin.Y, tileSize, tileSize);

                Color outlineColor = isActive ? Color.LawnGreen : Color.White;
                Color fillColor = isActive ? Color.LightSeaGreen : Color.White;

                SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, tileRect, fillColor * 0.35f * opacity);
                Helpers.DrawRectangleOutline(rect: tileRect, color: outlineColor * opacity * 0.8f, borderWidth: 2);

                this.DrawSlotLabel(slot: slot, tileRect: tileRect, opacity: opacity);

                Rectangle destRect = isActive ? tileRect : new Rectangle((int)slotPosWithMargin.X + spriteOffset, (int)slotPosWithMargin.Y + spriteOffset, spriteSize, spriteSize);
                slot.Draw(destRect: destRect, opacity: opacity, drawNewIcon: this.type != Type.SingleBottom);

                Rectangle quantityRect = new(x: tileRect.X, y: tileRect.Y + (tileRect.Height / 2), width: tileRect.Width, height: tileRect.Height / 2);
                DrawQuantity(pieceCount: slot.PieceCount, destRect: quantityRect, opacity: opacity);
            }

            if (this.InputType != InputTypes.Always) this.DrawCursor();

            SonOfRobinGame.SpriteBatch.End();
        }

        private void DrawSlotLabel(StorageSlot slot, Rectangle tileRect, float opacity)
        {
            if (slot.label == "" || !slot.IsEmpty) return;

            Rectangle labelRect = tileRect;
            labelRect.Inflate(-(int)(tileRect.Width * 0.1), -(int)(tileRect.Height * 0.4));

            Helpers.DrawTextInsideRectWithOutline(font: font, text: slot.label, rectangle: labelRect, color: Color.White * opacity, outlineColor: new Color(50, 50, 50) * opacity, outlineSize: 1, alignX: Helpers.AlignX.Center, alignY: Helpers.AlignY.Center, drawTestRect: false);
        }

        private void DrawMainLabel(float opacity)
        {
            if (this.type == Type.SingleBottom) return;

            Rectangle bgRect = this.BgRect;

            string label = this.storage.Label;

            Vector2 labelSize = font.MeasureString(label);
            float maxTextWidth = bgRect.Width * 0.3f;
            float maxTextHeight = bgRect.Height * 0.1f;
            float textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);

            float textWidth = labelSize.X * textScale;
            float textHeight = labelSize.Y * textScale;

            Vector2 labelPos = new Vector2((bgRect.Width - textWidth) / 2, bgRect.Top - textHeight);
            float shadowOffset = textHeight * 0.06f;
            Vector2 shadowPos = labelPos + new Vector2(shadowOffset, shadowOffset);

            SonOfRobinGame.SpriteBatch.DrawString(font, label, position: shadowPos, color: Color.Black * 0.5f * opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
            SonOfRobinGame.SpriteBatch.DrawString(font, label, position: labelPos, color: Color.White * opacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);
        }

        public static void DrawQuantity(int pieceCount, Rectangle destRect, float opacity, bool ignoreSingle = true)
        {
            if (ignoreSingle && pieceCount <= 1) return;

            string countTxt = $"x{pieceCount}";

            Helpers.DrawTextInsideRectWithOutline(font: font, text: countTxt, rectangle: destRect, color: Color.White * opacity, outlineColor: Color.Black * opacity, outlineSize: 1, alignX: Helpers.AlignX.Left, alignY: Helpers.AlignY.Bottom);
        }

        private Vector2 GetSlotPos(StorageSlot slot, int margin, int tileSize)
        {
            Point slotPos = this.storage.GetSlotPos(slot);

            int slotPosX = margin + (slotPos.X * (margin + tileSize));
            int slotPosy = margin + (slotPos.Y * (margin + tileSize));
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
            if (this.CursorX == -1 || this.CursorY == -1 || !this.inputActive || (this.draggedPieces.Count == 0 && Input.currentControlType == Input.ControlType.Touch)) return;

            Texture2D cursorTexture = TextureBank.GetTexture(TextureBank.TextureName.Cursor);
            int tileSize = this.TileSize;
            Vector2 slotPos = this.GetSlotPos(posX: this.CursorX, posY: this.CursorY, tileSize: this.TileSize, margin: this.Margin);

            slotPos += new Vector2(tileSize * 0.5f, tileSize * 0.5f);

            Rectangle sourceRectangle = new(0, 0, cursorTexture.Width, cursorTexture.Height);
            Rectangle destinationRectangle = new((int)slotPos.X, (int)slotPos.Y, tileSize, tileSize);

            if (draggedPieces.Count > 0)
            {
                BoardPiece shownPiece = draggedPieces[draggedPieces.Count - 1];
                Rectangle shownPieceRect = new Rectangle(
                    destinationRectangle.X + (int)(destinationRectangle.Width * 0.75f),
                    destinationRectangle.Y,
                    destinationRectangle.Width,
                    destinationRectangle.Height);

                shownPiece.sprite.DrawAndKeepInRectBounds(destRect: shownPieceRect, opacity: viewParams.drawOpacity);

                if (shownPiece.sprite.boardPiece.HitPoints < shownPiece.sprite.boardPiece.maxHitPoints)
                {
                    new StatBar(label: "", value: (int)shownPiece.sprite.boardPiece.HitPoints, valueMax: (int)shownPiece.sprite.boardPiece.maxHitPoints, colorMin: new Color(255, 0, 0), colorMax: new Color(0, 255, 0),
                        posX: shownPieceRect.Center.X, posY: shownPieceRect.Y + (int)(shownPieceRect.Width * 0.8f), width: (int)(shownPieceRect.Width * 0.8f), height: (int)(shownPieceRect.Height * 0.1f));

                    StatBar.FinishThisBatch();
                    StatBar.DrawAll();
                }

                Rectangle quantityRect = new(x: shownPieceRect.X, y: shownPieceRect.Y + (shownPieceRect.Height / 2), width: shownPieceRect.Width, height: shownPieceRect.Height / 2);
                DrawQuantity(pieceCount: draggedPieces.Count, destRect: quantityRect, opacity: this.viewParams.drawOpacity);
            }

            SonOfRobinGame.SpriteBatch.Draw(cursorTexture, destinationRectangle, sourceRectangle, Color.White);
        }
    }
}