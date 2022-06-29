using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Fireplace : BoardPiece
    {
        private static readonly Dictionary<PieceTemplate.Name, int> fuelFramesByName = new Dictionary<PieceTemplate.Name, int>
        {
            // number of frames each piece will burn for
            {PieceTemplate.Name.Stick, 60 * 20},
            {PieceTemplate.Name.WoodLogRegular, 60 * 60 * 1},
            {PieceTemplate.Name.WoodLogHard, 60 * 60 * 3},
            {PieceTemplate.Name.WoodPlank, 60 * 60 * 1},
            {PieceTemplate.Name.Coal, 60 * 60 * 5},
        };

        private static readonly List<PieceTemplate.Name> fuelNames = fuelFramesByName.Keys.ToList();

        private readonly ushort scareRange;
        private bool isOn;
        private int currentCycleBurningFramesLeft;
        private int burnStartFrame;
        private int burnAllFuelEndFrame;

        public Fireplace(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, byte storageWidth, byte storageHeight, string readableName, string description, Category category, ushort scareRange,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, category: category, lightEngine: lightEngine, activeState: State.Empty)
        {
            this.boardTask = Scheduler.TaskName.OpenContainer;
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Bonfire, maxPitchVariation: 0.5f, isLooped: true));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));

            this.scareRange = scareRange;
            this.isOn = false;
            this.burnStartFrame = 0;
            this.currentCycleBurningFramesLeft = 0;
            this.burnAllFuelEndFrame = 0;

            this.pieceStorage = new PieceStorage(width: storageWidth, height: (byte)(storageHeight + 1), world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Fireplace);

            var allowedPieceNames = new List<PieceTemplate.Name>(fuelNames);
            allowedPieceNames.Add(PieceTemplate.Name.FireplaceTriggerOn);
            allowedPieceNames.Add(PieceTemplate.Name.FireplaceTriggerOff);
            this.pieceStorage.AssignAllowedPieceNames(allowedPieceNames);

            BoardPiece flameTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.FireplaceTriggerOn, world: this.world);

            StorageSlot flameSlot = this.pieceStorage.GetSlot(0, 0);
            this.pieceStorage.AddPiece(flameTrigger);
            flameSlot.locked = true;

            BoardPiece waterTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.FireplaceTriggerOff, world: this.world);
            StorageSlot waterSlot = this.pieceStorage.GetSlot(1, 0);
            this.pieceStorage.AddPiece(waterTrigger);
            waterSlot.locked = true;

            for (int x = 2; x < storageWidth; x++) // locking and hiding the rest of the slots in first row
            {
                StorageSlot slot = this.pieceStorage.GetSlot(x, 0);
                slot.hidden = true;
                slot.locked = true;
            }
        }

        public bool IsOn
        {
            get { return this.isOn; }
            set
            {
                if (this.isOn == value)
                {
                    if (this.isOn) new TextWindow(text: "The fire has already started.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                    else new TextWindow(text: "It is not burning right now.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);

                    return;
                }

                if (value && !this.StartFire(showMessage: true)) return;

                this.isOn = value;
                if (this.isOn)
                {
                    this.activeState = State.FireplaceBurn;
                    this.AddToStateMachines();
                    this.sprite.AssignNewName(animName: "on");
                    this.sprite.lightEngine.Activate();
                    this.world.hintEngine.Disable(Tutorials.Type.KeepingAnimalsAway);
                    this.soundPack.Play(PieceSoundPack.Action.IsOn);
                }
                else
                {
                    this.activeState = State.Empty;
                    this.showStatBarsTillFrame = 0;
                    this.burnAllFuelEndFrame = 0;
                    this.sprite.AssignNewName(animName: "off");
                    this.sprite.lightEngine.Deactivate();
                    this.soundPack.Stop(PieceSoundPack.Action.IsOn);
                    this.soundPack.Play(PieceSoundPack.Action.TurnOff);
                }

                if (Inventory.layout == Inventory.Layout.InventoryAndChest) Inventory.SetLayout(newLayout: Inventory.Layout.Toolbar, player: this.world.player);
            }
        }

        private List<BoardPiece> StoredFuel { get { return this.pieceStorage.GetAllPieces().Where(piece => fuelNames.Contains(piece.name)).ToList(); } }

        private bool StartFire(bool showMessage)
        {
            var storedFuel = this.StoredFuel;
            if (storedFuel.Count == 0)
            {
                if (showMessage) new TextWindow(text: "I don't have wood or coal to burn.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                else MessageLog.AddMessage(msgType: MsgType.User, message: $"{Helpers.FirstCharToUpperCase(this.readableName)} has burned out.");
                return false;
            }

            if (!this.IsOn) this.burnStartFrame = this.world.currentUpdate;

            BoardPiece fuel = storedFuel[0];
            storedFuel.RemoveAt(0);

            this.pieceStorage.DestroyOneSpecifiedPiece(fuel.name);
            this.currentCycleBurningFramesLeft = fuelFramesByName[fuel.name];
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);

            this.UpdateEndFrame(storedFuel);

            return true;
        }

        private void UpdateEndFrame()
        { this.UpdateEndFrame(this.StoredFuel); }

        private void UpdateEndFrame(List<BoardPiece> storedFuel)
        {
            this.burnAllFuelEndFrame = this.world.currentUpdate + this.currentCycleBurningFramesLeft;
            foreach (BoardPiece fuelPiece in storedFuel)
            {
                this.burnAllFuelEndFrame += fuelFramesByName[fuelPiece.name];
            }
            this.showStatBarsTillFrame = this.burnAllFuelEndFrame;
        }

        public override void SM_FireplaceBurn()
        {
            this.currentCycleBurningFramesLeft--;

            bool stopBurning = this.currentCycleBurningFramesLeft <= 0 && !this.StartFire(showMessage: false);
            if (stopBurning)
            {
                this.IsOn = false;
                return;
            }

            if (this.world.currentUpdate % 10 != 0) return;
            this.UpdateEndFrame();

            var nearbyPieces = this.world.grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: this.scareRange, compareWithBottom: true);
            var animalPieces = nearbyPieces.Where(piece => piece.GetType() == typeof(Animal));

            foreach (BoardPiece piece in animalPieces)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{Helpers.FirstCharToUpperCase(this.readableName)} - scaring off {piece.readableName}.");

                Animal animal = (Animal)piece;
                animal.target = this;
                animal.aiData.Reset(animal);
                animal.activeState = State.AnimalFlee;

                if (PieceInfo.GetInfo(animal.name).isCarnivorous) this.world.hintEngine.ShowGeneralHint(type: HintEngine.Type.AnimalScaredOfFire, ignoreDelay: true, piece: animal);
            }
        }

        public override void DrawStatBar()
        {
            if (this.world.currentUpdate < this.burnAllFuelEndFrame)
            {
                int burningDuration = this.burnAllFuelEndFrame - this.burnStartFrame;
                int burningCurrentFrame = burningDuration - (this.world.currentUpdate - this.burnStartFrame);

                new StatBar(label: "", value: burningCurrentFrame, valueMax: burningDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 255, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.WoodLogRegular].texture);
            }

            base.DrawStatBar();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["fireplace_isOn"] = this.isOn;
            pieceData["fireplace_currentCycleBurningFramesLeft"] = this.currentCycleBurningFramesLeft;
            pieceData["fireplace_burnStartFrame"] = this.burnStartFrame;
            pieceData["fireplace_burnAllFuelEndFrame"] = this.burnAllFuelEndFrame;
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.isOn = (bool)pieceData["fireplace_isOn"];
            this.currentCycleBurningFramesLeft = (int)pieceData["fireplace_currentCycleBurningFramesLeft"];
            this.burnStartFrame = (int)pieceData["fireplace_burnStartFrame"];
            this.burnAllFuelEndFrame = (int)pieceData["fireplace_burnAllFuelEndFrame"];
        }
    }
}