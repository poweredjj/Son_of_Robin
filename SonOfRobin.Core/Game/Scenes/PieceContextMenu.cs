using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceContextMenu : Scene
    {
        protected enum ContextAction { Drop, DropAll, Move, Eat, Drink, Plant, Cook, Switch, Ignite, Extinguish, Upgrade }

        private static readonly SpriteFont font = SonOfRobinGame.FontTommy40;
        private static readonly float marginPercent = 0.03f;
        private static readonly float entryWidthPercent = 0.8f;
        private static readonly float entryHeightPercent = 0.1f;

        private static readonly Sound soundOpen = new Sound(SoundData.Name.Invoke);
        private static readonly Sound soundNavigate = new Sound(SoundData.Name.Navigation);
        private static readonly Sound soundReturn = new Sound(SoundData.Name.Navigation);

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

        public PieceContextMenu(BoardPiece piece, PieceStorage storage, StorageSlot slot, float percentPosX, float percentPosY, bool addMove = false, bool addDrop = true, bool addCook = false, bool addIgnite = false, bool addExtinguish = false, bool addUpgrade = false) : base(inputType: InputTypes.Normal, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.PieceContext)
        {
            this.piece = piece;
            this.storage = storage;
            this.slot = slot;
            this.actionList = this.GetContextActionList(addMove: addMove, addDrop: addDrop, addCook: addCook, addIgnite: addIgnite, addExtinguish: addExtinguish, addUpgrade: addUpgrade);
            this.percentPosX = percentPosX;
            this.percentPosY = percentPosY;
            this.activeEntry = 0;
            this.showCursor = Input.currentControlType != Input.ControlType.Touch;

            this.UpdateViewSizes();

            this.transManager.AddMultipleTransitions(outTrans: false, duration: 8, paramsToChange:
                new Dictionary<string, float> { { "PosY", this.viewParams.PosY + SonOfRobinGame.VirtualHeight }, { "Opacity", 0f } });
        }

        private List<ContextAction> GetContextActionList(bool addMove = false, bool addDrop = false, bool addCook = false, bool addIgnite = false, bool addExtinguish = false, bool addUpgrade = false)
        {
            var contextActionList = new List<ContextAction> { };

            if (this.piece.toolbarTask == Scheduler.TaskName.GetEaten) contextActionList.Add(ContextAction.Eat);
            if (this.piece.toolbarTask == Scheduler.TaskName.GetDrinked) contextActionList.Add(ContextAction.Drink);
            if (this.piece.GetType() == typeof(Fruit)) contextActionList.Add(ContextAction.Plant);
            if (this.piece.GetType() == typeof(PortableLight) && this.piece.IsOnPlayersToolbar) contextActionList.Add(ContextAction.Switch);
            if (addMove) contextActionList.Add(ContextAction.Move);
            if (addDrop) contextActionList.Add(ContextAction.Drop);
            if (addCook) contextActionList.Add(ContextAction.Cook);
            if (addIgnite) contextActionList.Add(ContextAction.Ignite);
            if (addExtinguish) contextActionList.Add(ContextAction.Extinguish);
            if (addUpgrade) contextActionList.Add(ContextAction.Upgrade);
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
            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalCancelReturnSkip))
            {
                soundReturn.Play();
                this.Remove();
                return;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalUp))
            {
                soundNavigate.Play();
                this.ActiveEntry -= 1;
                this.showCursor = true;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalDown))
            {
                soundNavigate.Play();
                this.ActiveEntry += 1;
                this.showCursor = true;
            }

            if (InputMapper.HasBeenPressed(InputMapper.Action.GlobalConfirm))
            {
                soundOpen.Play();
                this.ExecuteAction();
                this.Remove();
                return;
            }

            this.ProcessTouch();
        }

        private void ProcessTouch()
        {
            Rectangle bgRect = this.BgRect;

            foreach (TouchLocation touch in TouchInput.TouchPanelState)
            {
                Vector2 touchPos = (touch.Position / Preferences.GlobalScale) - this.viewParams.DrawPos;

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
                    {
                        this.storage.DropPiecesFromSlot(slot: this.slot, addMovement: true);
                        return;
                    }

                case ContextAction.DropAll:
                    {
                        this.storage.DropPiecesFromSlot(slot: this.slot, dropAllPieces: true, addMovement: true);
                        return;
                    }

                case ContextAction.Move:
                    {
                        var invScene = Scene.GetSecondTopSceneOfType(typeof(Inventory));
                        if (invScene == null) return;
                        Inventory secondInventory = (Inventory)invScene;

                        List<BoardPiece> piecesToMove = this.storage.RemoveAllPiecesFromSlot(slot: this.slot);

                        if (piecesToMove.Count == 0) return;

                        piecesToMove[0].soundPack.Play(action: PieceSoundPack.Action.IsDropped, ignore3D: true, ignoreCooldown: true);

                        foreach (BoardPiece piece in piecesToMove)
                        {
                            bool pieceMoved = secondInventory.storage.AddPiece(piece);
                            if (!pieceMoved) this.storage.AddPiece(piece);
                        }

                        return;
                    }

                case ContextAction.Eat:
                    {
                        BoardPiece food = this.slot.TopPiece;
                        World world = World.GetTopWorld();

                        var executeHelper = new Dictionary<string, Object> { };
                        executeHelper["player"] = world.Player;
                        executeHelper["slot"] = this.slot;
                        executeHelper["toolbarPiece"] = food;
                        executeHelper["buttonHeld"] = false;
                        executeHelper["highlightOnly"] = false;

                        new Scheduler.Task(taskName: food.toolbarTask, delay: 0, executeHelper: executeHelper);

                        return;
                    }

                case ContextAction.Drink:
                    {
                        BoardPiece potion = this.slot.TopPiece;
                        World world = World.GetTopWorld();

                        var executeHelper = new Dictionary<string, Object> { };
                        executeHelper["player"] = world.Player;
                        executeHelper["slot"] = this.slot;
                        executeHelper["toolbarPiece"] = potion;
                        executeHelper["buttonHeld"] = false;
                        executeHelper["highlightOnly"] = false;

                        new Scheduler.Task(taskName: potion.toolbarTask, delay: 0, executeHelper: executeHelper);

                        return;
                    }

                case ContextAction.Plant:
                    {
                        // plant is "crafted" to allow for planning its position

                        Fruit fruit = (Fruit)this.slot.TopPiece;
                        Player player = fruit.world.Player;

                        if (!player.CanSeeAnything)
                        {
                            new TextWindow(text: $"It is too dark to plant | {fruit.readableName}...", imageList: new List<Texture2D> { PieceInfo.GetTexture(fruit.name) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 0, animSound: player.world.DialogueSound);

                            return;
                        }

                        if (player.IsVeryTired)
                        {
                            new TextWindow(text: "I'm too tired to plant anything...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 0, animSound: player.world.DialogueSound);

                            return;
                        }

                        PieceTemplate.Name plantName = PieceInfo.GetInfo(fruit.name).isSpawnedBy;

                        Craft.Recipe plantRecipe = new Craft.Recipe(pieceToCreate: plantName, ingredients: new Dictionary<PieceTemplate.Name, byte> { { fruit.name, 1 } }, fatigue: PieceInfo.GetInfo(plantName).blocksMovement ? 100 : 50, maxLevel: 0, durationMultiplier: 1f, fatigueMultiplier: 1f, isReversible: false, checkIfAlreadyAdded: false);

                        Inventory.SetLayout(newLayout: Inventory.Layout.Toolbar, player: player);
                        plantRecipe.TryToProducePieces(player: player, showMessages: false);

                        return;
                    }

                case ContextAction.Cook:
                    {
                        Cooker cooker = (Cooker)this.storage.storagePiece;
                        cooker.Cook();

                        return;
                    }

                case ContextAction.Switch:
                    {
                        PortableLight portableLight = (PortableLight)this.piece;
                        portableLight.IsOn = !portableLight.IsOn;

                        return;
                    }

                case ContextAction.Ignite:
                    {
                        Fireplace fireplace = (Fireplace)this.storage.storagePiece;
                        fireplace.IsOn = true;

                        return;
                    }

                case ContextAction.Upgrade:
                    {
                        UpgradeBench upgradeBench = (UpgradeBench)this.storage.storagePiece;
                        upgradeBench.Upgrade();

                        return;
                    }

                case ContextAction.Extinguish:
                    {
                        Fireplace fireplace = (Fireplace)this.storage.storagePiece;
                        Sound.QuickPlay(SoundData.Name.WaterSplash);
                        fireplace.IsOn = false;

                        return;
                    }

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

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bgRect, Color.DodgerBlue * 0.9f * this.viewParams.drawOpacity);
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

                SonOfRobinGame.SpriteBatch.DrawString(font, actionLabel, position: shadowPos, color: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                SonOfRobinGame.SpriteBatch.DrawString(font, actionLabel, position: textPos, color: textColor * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

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
