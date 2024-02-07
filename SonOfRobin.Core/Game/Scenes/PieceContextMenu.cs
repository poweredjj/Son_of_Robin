using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class PieceContextMenu : Scene
    {
        protected enum ContextAction
        {
            Drop,
            DropAll,
            FieldHarvest,
            Harvest,
            Move,
            Eat,
            Drink,
            Plant,
            Cook,
            Smelt,
            Switch,
            Ignite,
            Extinguish,
            Brew,
            Equip,
            Offer,
            Construct,
            Empty,
        }

        private const float marginPercent = 0.03f;
        private const float entryWidthPercent = 0.8f;
        private const float entryHeightPercent = 0.1f;

        private static readonly Sound soundOpen = new(SoundData.Name.Invoke);
        private static readonly Sound soundNavigate = new(SoundData.Name.Navigation);
        private static readonly Sound soundReturn = new(SoundData.Name.Navigation);

        private readonly SpriteFontBase font;
        private readonly BoardPiece piece;
        private readonly PieceStorage storage;
        private readonly StorageSlot slot;
        private readonly List<ContextAction> actionList;
        private readonly float percentPosX, percentPosY;
        private int activeEntry;
        private bool showCursor;

        private int Margin
        { get { return Convert.ToInt32(SonOfRobinGame.ScreenHeight * marginPercent); } }

        private Vector2 MenuPos
        {
            get
            {
                Vector2 menuPos = new(this.percentPosX * SonOfRobinGame.ScreenWidth, this.percentPosY * SonOfRobinGame.ScreenHeight);
                Rectangle bgRect = this.BgRect;

                if (menuPos.X + bgRect.Width > SonOfRobinGame.ScreenWidth) menuPos.X = SonOfRobinGame.ScreenWidth - bgRect.Width;
                if (menuPos.Y + bgRect.Height > SonOfRobinGame.ScreenHeight) menuPos.Y = SonOfRobinGame.ScreenHeight - bgRect.Height;

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

        private ContextAction ActiveAction
        { get { return this.actionList[this.ActiveEntry]; } }

        private float TextScale
        {
            get
            {
                float maxTextWidth = SonOfRobinGame.ScreenWidth * entryWidthPercent;
                float maxTextHeight = SonOfRobinGame.ScreenHeight * entryHeightPercent;
                Vector2 labelSize;
                float textScale;
                float minScale = 9999f;

                foreach (ContextAction action in this.actionList)
                {
                    labelSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: this.GetActionLabel(action));
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
                    Vector2 labelSize = Helpers.MeasureStringCorrectly(font: font, stringToMeasure: this.GetActionLabel(action));
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

        public PieceContextMenu(BoardPiece piece, PieceStorage storage, StorageSlot slot, float percentPosX, float percentPosY, bool addEquip = false, bool addMove = false, bool addDrop = true, bool addCook = false, bool addSmelt = false, bool addBrew = false, bool addIgnite = false, bool addExtinguish = false, bool addHarvest = false, bool addFieldHarvest = false, bool addOffer = false, bool addConstruct = false, bool addEmpty = false) : base(inputType: InputTypes.Normal, priority: 0, blocksUpdatesBelow: false, blocksDrawsBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.PieceContext)
        {
            this.font = SonOfRobinGame.FontTommy.GetFont(60);
            this.piece = piece;
            this.storage = storage;
            this.slot = slot;
            this.actionList = this.GetContextActionList(addEquip: addEquip, addMove: addMove, addDrop: addDrop, addCook: addCook, addSmelt: addSmelt, addBrew: addBrew, addIgnite: addIgnite, addExtinguish: addExtinguish, addHarvest: addHarvest, addFieldHarvest: addFieldHarvest, addOffer: addOffer, addConstruct: addConstruct, addEmpty: addEmpty);
            this.percentPosX = percentPosX;
            this.percentPosY = percentPosY;
            this.activeEntry = 0;
            this.showCursor = Input.CurrentControlType != Input.ControlType.Touch;

            this.UpdateViewSizes();

            this.transManager.AddMultipleTransitions(outTrans: false, duration: 8, paramsToChange:
                new Dictionary<string, float> { { "PosY", this.viewParams.PosY + SonOfRobinGame.ScreenHeight }, { "Opacity", 0f } });
        }

        private List<ContextAction> GetContextActionList(bool addEquip = false, bool addMove = false, bool addDrop = false, bool addCook = false, bool addSmelt = false, bool addBrew = false, bool addIgnite = false, bool addExtinguish = false, bool addHarvest = false, bool addFieldHarvest = false, bool addOffer = false, bool addConstruct = false, bool addEmpty = false)
        {
            var contextActionList = new List<ContextAction> { };

            if (addEquip) contextActionList.Add(ContextAction.Equip);
            if (addMove) contextActionList.Add(ContextAction.Move);
            if (this.piece.pieceInfo.toolbarTask == Scheduler.TaskName.GetEaten) contextActionList.Add(ContextAction.Eat);
            if (this.piece.pieceInfo.toolbarTask == Scheduler.TaskName.GetDrinked) contextActionList.Add(ContextAction.Drink);
            if (this.piece.GetType() == typeof(Seed)) contextActionList.Add(ContextAction.Plant);
            if (this.piece.GetType() == typeof(PortableLight) && this.piece.IsOnPlayersToolbar) contextActionList.Add(ContextAction.Switch);
            if (addFieldHarvest) contextActionList.Add(ContextAction.FieldHarvest);
            if (addCook) contextActionList.Add(ContextAction.Cook);
            if (addSmelt) contextActionList.Add(ContextAction.Smelt);
            if (addBrew) contextActionList.Add(ContextAction.Brew);
            if (addIgnite) contextActionList.Add(ContextAction.Ignite);
            if (addExtinguish) contextActionList.Add(ContextAction.Extinguish);
            if (addHarvest) contextActionList.Add(ContextAction.Harvest);
            if (addOffer) contextActionList.Add(ContextAction.Offer);
            if (addConstruct) contextActionList.Add(ContextAction.Construct);
            if (addEmpty) contextActionList.Add(ContextAction.Empty);
            if (addDrop) contextActionList.Add(ContextAction.Drop);
            if (this.slot.PieceCount > 1) contextActionList.Add(ContextAction.DropAll);

            return contextActionList;
        }

        public override void Remove()
        {
            if (!this.transManager.IsEnding)
            {
                this.transManager.AddMultipleTransitions(outTrans: true, duration: 8, endRemoveScene: true, paramsToChange:
                    new Dictionary<string, float> { { "PosY", this.viewParams.PosY + SonOfRobinGame.ScreenHeight }, { "Opacity", 0f } });
                return;
            }

            base.Remove();
        }

        public override void Update()
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
                Vector2 touchPos = touch.Position - this.viewParams.DrawPos;

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
                        bool droppedCorrectly = this.storage.DropPiecesFromSlot(slot: this.slot, addMovement: true);
                        if (!droppedCorrectly) new TextWindow(text: "Cannot drop the item.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, startingSound: SoundData.Name.Error);
                        return;
                    }

                case ContextAction.DropAll:
                    {
                        bool droppedCorrectly = this.storage.DropPiecesFromSlot(slot: this.slot, dropAllPieces: true, addMovement: true);
                        if (!droppedCorrectly) new TextWindow(text: "Cannot drop the items.", textColor: Color.White, bgColor: Color.DarkRed, useTransition: false, animate: false, startingSound: SoundData.Name.Error);
                        return;
                    }

                case ContextAction.Equip:
                    {
                        var invScene = GetSecondTopSceneOfType(typeof(Inventory));
                        if (invScene == null) return;

                        PieceStorage equipStorage = ((Inventory)invScene).storage;
                        if (equipStorage.storageType != PieceStorage.StorageType.Equip) return;

                        BoardPiece equipPiece = this.slot.RemoveTopPiece();

                        bool pieceMoved = equipStorage.AddPiece(equipPiece); // trying to place item in a free slot
                        if (!pieceMoved)
                        {
                            // trying to switch with other equipped item
                            foreach (StorageSlot currentSlot in equipStorage.OccupiedSlots)
                            {
                                if (currentSlot.CanFitThisPiece(piece: equipPiece, treatSlotAsEmpty: true))
                                {
                                    BoardPiece previouslyEquippedPiece = currentSlot.RemoveTopPiece();
                                    this.storage.AddPiece(previouslyEquippedPiece);
                                    currentSlot.AddPiece(equipPiece);

                                    pieceMoved = true;
                                    break;
                                }
                            }
                        }

                        if (!pieceMoved) // this shouldn't happen in any normal circumstances
                        {
                            this.slot.AddPiece(equipPiece);
                            return;
                        }

                        equipPiece.activeSoundPack.Play(action: PieceSoundPackTemplate.Action.IsDropped, ignore3D: true, ignoreCooldown: true);
                        new RumbleEvent(force: 0.20f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.06f, fadeOutSeconds: 0.06f);

                        return;
                    }

                case ContextAction.Move:
                    {
                        var invScene = GetSecondTopSceneOfType(typeof(Inventory));
                        if (invScene == null) return;

                        PieceStorage targetStorage = ((Inventory)invScene).storage;

                        List<BoardPiece> piecesToMove = this.storage.RemoveAllPiecesFromSlot(slot: this.slot);
                        if (piecesToMove.Count == 0) return;

                        piecesToMove[0].activeSoundPack.Play(action: PieceSoundPackTemplate.Action.IsDropped, ignore3D: true, ignoreCooldown: true);
                        new RumbleEvent(force: 0.20f, durationSeconds: 0, bigMotor: true, fadeInSeconds: 0.06f, fadeOutSeconds: 0.06f);

                        foreach (BoardPiece pieceToMove in piecesToMove)
                        {
                            bool pieceMoved = targetStorage.AddPiece(pieceToMove);
                            if (!pieceMoved) this.slot.AddPiece(pieceToMove);
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

                        new Scheduler.Task(taskName: food.pieceInfo.toolbarTask, delay: 0, executeHelper: executeHelper);

                        return;
                    }

                case ContextAction.Drink:
                    {
                        BoardPiece potion = this.slot.TopPiece;
                        World world = potion.world;

                        var executeHelper = new Dictionary<string, Object> { };
                        executeHelper["player"] = world.Player;
                        executeHelper["slot"] = this.slot;
                        executeHelper["toolbarPiece"] = potion;
                        executeHelper["buttonHeld"] = false;
                        executeHelper["highlightOnly"] = false;

                        new Scheduler.Task(taskName: potion.pieceInfo.toolbarTask, delay: 0, executeHelper: executeHelper);

                        return;
                    }

                case ContextAction.Plant:
                    {
                        // plant is "crafted" to allow for planning its position

                        Seed seeds = (Seed)this.slot.TopPiece;

                        var executeHelper = new Dictionary<string, Object> { // must be compliant with UseToolbarPiece() dict
                            { "player", seeds.world.Player },
                            { "toolbarPiece", seeds },
                            { "highlightOnly", false },
                            };

                        new Scheduler.Task(taskName: Scheduler.TaskName.Plant, executeHelper: executeHelper);

                        return;
                    }

                case ContextAction.Cook:
                    {
                        Cooker cooker = (Cooker)this.storage.storagePiece;
                        World world = cooker.world;
                        Player player = world.Player;

                        if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
                        {
                            Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                            { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.KeepingAnimalsAway, world: world, ignoreDelay: true); };

                            new TextWindow(text: "I cannot cook with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ExecuteDelegate, closingTaskHelper: showTutorialDlgt, animSound: world.DialogueSound);
                            return;
                        }

                        if (world.weather.IsRaining && !cooker.canBeUsedDuringRain)
                        {
                            new TextWindow(text: "I cannot cook during rain.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                            return;
                        }

                        if (player.IsVeryTired)
                        {
                            new TextWindow(text: "I'm too tired to cook...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                            return;
                        }

                        cooker.Cook();

                        return;
                    }

                case ContextAction.Smelt:
                    {
                        Furnace furnace = (Furnace)this.storage.storagePiece;
                        World world = furnace.world;
                        Player player = world.Player;

                        if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
                        {
                            Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                            { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.KeepingAnimalsAway, world: world, ignoreDelay: true); };

                            new TextWindow(text: "I cannot smelt with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ExecuteDelegate, closingTaskHelper: showTutorialDlgt, animSound: world.DialogueSound);
                            return;
                        }

                        if (player.IsVeryTired)
                        {
                            new TextWindow(text: "I'm too tired to smelt...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                            return;
                        }

                        furnace.Smelt();

                        return;
                    }

                case ContextAction.Brew:
                    {
                        AlchemyLab alchemyLab = (AlchemyLab)this.storage.storagePiece;
                        World world = alchemyLab.world;
                        Player player = world.Player;

                        if (player.AreEnemiesNearby && !player.IsActiveFireplaceNearby)
                        {
                            Scheduler.ExecutionDelegate showTutorialDlgt = () =>
                            { if (!world.HasBeenRemoved) Tutorials.ShowTutorialOnTheField(type: Tutorials.Type.KeepingAnimalsAway, world: world, ignoreDelay: true); };

                            new TextWindow(text: "I cannot brew with enemies nearby.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, closingTask: Scheduler.TaskName.ExecuteDelegate, closingTaskHelper: showTutorialDlgt, animSound: world.DialogueSound);
                            return;
                        }

                        if (world.weather.IsRaining)
                        {
                            new TextWindow(text: "I cannot brew during rain.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                            return;
                        }

                        if (player.IsVeryTired)
                        {
                            new TextWindow(text: "I'm too tired to brew...", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: InputTypes.None, blockInputDuration: 45, priority: 1, animSound: world.DialogueSound);
                            return;
                        }

                        alchemyLab.Brew();

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

                case ContextAction.Harvest:
                    {
                        MeatHarvestingWorkshop workshop = (MeatHarvestingWorkshop)this.storage.storagePiece;
                        workshop.HarvestMeat();

                        return;
                    }

                case ContextAction.FieldHarvest:
                    {
                        Player player = (Player)this.storage.storagePiece;
                        player.HarvestMeatInTheField(this.slot);

                        return;
                    }

                case ContextAction.Offer:
                    {
                        Totem totem = (Totem)this.storage.storagePiece;
                        totem.Offer();

                        return;
                    }

                case ContextAction.Construct:
                    {
                        ConstructionSite constructionSite = (ConstructionSite)this.storage.storagePiece;
                        constructionSite.Construct();

                        return;
                    }

                case ContextAction.Empty:
                    {
                        Potion potion = (Potion)this.piece;

                        Sound.QuickPlay(SoundData.Name.Glug);

                        BoardPiece emptyContainter = PieceTemplate.CreatePiece(templateName: potion.pieceInfo.convertsToWhenUsed, world: potion.world);

                        if (this.slot.CanFitThisPiece(piece: emptyContainter, treatSlotAsEmpty: true))
                        {
                            this.slot.DestroyPieceAndReplaceWithAnother(emptyContainter);
                        }
                        else
                        {
                            this.slot.DestroyPieceWithID(potion.id);
                            this.slot.storage.AddPiece(piece: emptyContainter, dropIfDoesNotFit: true);
                        }

                        return;
                    }

                case ContextAction.Extinguish:
                    {
                        Fireplace fireplace = (Fireplace)this.storage.storagePiece;
                        if (fireplace.IsOn) Sound.QuickPlay(SoundData.Name.WaterSplash);
                        fireplace.IsOn = false;

                        return;
                    }

                default:
                    throw new ArgumentException($"Unsupported context action - {action}.");
            }
        }

        private string GetActionLabel(ContextAction action)
        {
            return Helpers.ToSentenceCase(Convert.ToString(action));
        }

        public override void Draw()
        {
            SonOfRobinGame.SpriteBatch.Begin(transformMatrix: this.TransformMatrix);

            Rectangle bgRect = this.BgRect;
            float margin = this.Margin;
            Vector2 maxEntrySize = this.MaxEntrySize;

            SonOfRobinGame.SpriteBatch.Draw(SonOfRobinGame.WhiteRectangle, bgRect, Color.DodgerBlue * 0.9f * this.viewParams.drawOpacity);
            SonOfRobinGame.SpriteBatch.DrawRectangle(rectangle: bgRect, color: Color.White * this.viewParams.drawOpacity, thickness: 2f);

            float textScale = this.TextScale;

            Rectangle pieceRect = new Rectangle(bgRect.X + (int)margin, bgRect.Y + (int)margin, (int)maxEntrySize.Y, (int)maxEntrySize.Y);
            this.piece.sprite.DrawAndKeepInRectBounds(destRect: pieceRect, opacity: viewParams.drawOpacity);

            float shadowOffset = Math.Max(maxEntrySize.Y * 0.05f, 1);
            int entryNo = 0;

            foreach (ContextAction action in this.actionList)
            {
                int displayedPos = entryNo + 1;
                bool isActive = entryNo == this.ActiveEntry && showCursor;
                Color textColor = isActive ? Color.White : Color.LightSkyBlue;
                string actionLabel = this.GetActionLabel(action);

                Rectangle entryRect = this.GetEntryRect(entryNo);
                Vector2 textPos = new Vector2(entryRect.X, entryRect.Y);
                Vector2 shadowPos = new Vector2(textPos.X + shadowOffset, textPos.Y + shadowOffset);
                Vector2 textScaleVector = new Vector2(textScale);

                font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: actionLabel, position: shadowPos, color: Color.MidnightBlue * this.viewParams.drawOpacity * 0.7f, scale: textScaleVector, effect: FontSystemEffect.Blurry, effectAmount: 3);

                font.DrawText(batch: SonOfRobinGame.SpriteBatch, text: actionLabel, position: textPos, color: textColor * this.viewParams.drawOpacity, scale: textScaleVector);

                entryNo++;
            }

            SonOfRobinGame.SpriteBatch.End();
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