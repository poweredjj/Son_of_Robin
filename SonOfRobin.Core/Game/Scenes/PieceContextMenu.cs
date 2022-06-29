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
        protected enum ContextAction { Drop, Move, Eat, Plant }

        private static readonly SpriteFont font = SonOfRobinGame.fontHuge;
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
                float minScale = 9999f;

                foreach (ContextAction action in this.actionList)
                {
                    Vector2 labelSize = font.MeasureString(Convert.ToString(action));
                    float maxTextWidth = SonOfRobinGame.VirtualWidth * entryWidthPercent;
                    float maxTextHeight = SonOfRobinGame.VirtualHeight * entryHeightPercent;
                    float textScale = Math.Min(maxTextWidth / labelSize.X, maxTextHeight / labelSize.Y);
                    if (textScale < minScale) minScale = textScale;
                }

                return minScale;
            }
        }

        private Vector2 MaxEntrySize
        {
            get
            {
                float maxEncounteredtWidth = 0f;
                float maxEncounteredtHeight = 0f;
                float textScale = this.TextScale;
                float textWidth, textHeight;

                foreach (ContextAction action in this.actionList)
                {
                    Vector2 labelSize = font.MeasureString(Convert.ToString(action).ToLower());
                    textWidth = labelSize.X * textScale;
                    textHeight = labelSize.Y * textScale;

                    if (textWidth > maxEncounteredtWidth) maxEncounteredtWidth = textWidth;
                    if (textHeight > maxEncounteredtHeight) maxEncounteredtHeight = textHeight;
                }

                return new Vector2(maxEncounteredtWidth, maxEncounteredtHeight);
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

        public PieceContextMenu(BoardPiece piece, PieceStorage storage, StorageSlot slot, float percentPosX, float percentPosY, bool addMove = false) : base(inputType: InputTypes.Normal, priority: 1, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty)
        {
            this.piece = piece;
            this.storage = storage;
            this.slot = slot;
            this.actionList = this.GetContextActionList(addMove: addMove);
            this.percentPosX = percentPosX;
            this.percentPosY = percentPosY;
            this.activeEntry = 0;
            this.showCursor = SonOfRobinGame.platform != Platform.Mobile;

            this.UpdateViewSizes();

            this.AddTransition(new Transition(type: Transition.TransType.In, duration: 8, scene: this, blockInput: false, paramsToChange: new Dictionary<string, float> { { "posY", this.viewParams.posY + SonOfRobinGame.VirtualHeight }, { "opacity", 0f } }));
        }

        private List<ContextAction> GetContextActionList(bool addMove = false)
        {
            var contextActionList = new List<ContextAction> { };

            if (this.piece.GetType() == typeof(Fruit))
            {
                contextActionList.Add(ContextAction.Eat);
                contextActionList.Add(ContextAction.Plant);
            }

            if (addMove) contextActionList.Add(ContextAction.Move);

            contextActionList.Add(ContextAction.Drop); // should go last

            return contextActionList;
        }

        public override void Remove()
        {
            if (this.transition == null)
            {
                this.AddTransition(new Transition(type: Transition.TransType.Out, duration: 8, scene: this, blockInput: true, paramsToChange: new Dictionary<string, float> { { "posY", this.viewParams.posY + SonOfRobinGame.VirtualHeight }, { "opacity", 0f } }, removeScene: true));
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
            this.viewParams.width = bgRect.Width;
            this.viewParams.height = bgRect.Height;

            Vector2 menuPos = this.MenuPos;
            this.viewParams.posX = menuPos.X;
            this.viewParams.posY = menuPos.Y;
        }

        private void ProcessInput()
        {
            if (Keyboard.HasBeenPressed(Keys.Escape) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.B))
            {
                this.Remove();
                return;
            }

            if (Keyboard.HasBeenPressed(Keys.W) || Keyboard.HasBeenPressed(Keys.Up) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadUp))
            {
                this.ActiveEntry -= 1;
                this.showCursor = true;
            }

            if (Keyboard.HasBeenPressed(Keys.S) || Keyboard.HasBeenPressed(Keys.Down) || GamePad.HasBeenPressed(playerIndex: PlayerIndex.One, button: Buttons.DPadDown))
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
                    this.storage.DropTopPieceFromSlot(slot: this.slot);
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
                    // TODO add eating code
                    return;

                case ContextAction.Plant:
                    Fruit fruit = (Fruit)this.slot.GetTopPiece();

                    for (int i = 0; i < 170; i += 15)
                    {
                        Sprite.maxDistanceOverride = i;
                        BoardPiece newPlant = PieceTemplate.CreateOnBoard(world: fruit.world, position: fruit.world.player.sprite.position, templateName: fruit.spawnerName);
                        if (newPlant.sprite.placedCorrectly)
                        {
                            this.slot.RemoveTopPiece();
                            break;
                        }
                    }

                    return;

                default:
                    throw new DivideByZeroException($"Unsupported context action - {action}.");
            }

        }

        public override void Draw()
        {
            Rectangle bgRect = this.BgRect;
            float margin = this.Margin;
            Vector2 maxEntrySize = this.MaxEntrySize;

            SonOfRobinGame.spriteBatch.Draw(SonOfRobinGame.whiteRectangle, bgRect, Color.DodgerBlue * 1f * this.viewParams.drawOpacity);
            Helpers.DrawRectangleOutline(rect: bgRect, color: Color.White * this.viewParams.drawOpacity, borderWidth: 2);

            float textScale = this.TextScale;

            Rectangle pieceRect = new Rectangle(bgRect.X + (int)margin, bgRect.Y + (int)margin, (int)maxEntrySize.Y, (int)maxEntrySize.Y);
            this.piece.sprite.Draw(destRect: pieceRect, opacity: viewParams.drawOpacity);

            int displayedPos;
            string actionName;
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
                actionName = Convert.ToString(action).ToLower();

                entryRect = this.GetEntryRect(entryNo);
                textPos = new Vector2(entryRect.X, entryRect.Y);
                shadowPos = new Vector2(textPos.X + shadowOffset, textPos.Y + shadowOffset);

                SonOfRobinGame.spriteBatch.DrawString(font, actionName, position: shadowPos, color: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

                SonOfRobinGame.spriteBatch.DrawString(font, actionName, position: textPos, color: textColor * this.viewParams.drawOpacity, origin: Vector2.Zero, scale: textScale, rotation: 0, effects: SpriteEffects.None, layerDepth: 0);

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
