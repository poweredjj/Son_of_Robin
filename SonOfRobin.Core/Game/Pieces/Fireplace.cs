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
            { PieceTemplate.Name.Stick, 60 * 20 },
            { PieceTemplate.Name.WoodLogRegular, 60 * 60 * 1 },
            { PieceTemplate.Name.WoodLogHard, 60 * 60 * 3 },
            { PieceTemplate.Name.WoodPlank, 60 * 60 * 1 },
            { PieceTemplate.Name.Coal, 60 * 60 * 5 },
        };

        private static readonly List<PieceTemplate.Name> fuelNames = fuelFramesByName.Keys.ToList();

        private readonly ushort scareRange;
        private bool isOn;
        private int currentCycleBurningFramesLeft;
        private int burnStartFrame;
        private int burnAllFuelEndFrame;

        public Fireplace(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, byte storageWidth, byte storageHeight, string readableName, string description, ushort scareRange,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, lightEngine: lightEngine, activeState: State.Empty)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Bonfire, maxPitchVariation: 0.5f, isLooped: true));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));
            this.soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.StoneMove2, ignore3DAlways: true));

            this.scareRange = scareRange;
            this.isOn = false;
            this.burnStartFrame = 0;
            this.currentCycleBurningFramesLeft = 0;
            this.burnAllFuelEndFrame = 0;

            this.PieceStorage = new PieceStorage(width: storageWidth, height: (byte)(storageHeight + 1), storagePiece: this, storageType: PieceStorage.StorageType.Fireplace);

            var allowedPieceNames = new List<PieceTemplate.Name>(fuelNames);
            allowedPieceNames.Add(PieceTemplate.Name.FireplaceTriggerOn);
            allowedPieceNames.Add(PieceTemplate.Name.FireplaceTriggerOff);
            this.PieceStorage.AssignAllowedPieceNames(allowedPieceNames);

            BoardPiece flameTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.FireplaceTriggerOn, world: this.world);

            StorageSlot flameSlot = this.PieceStorage.GetSlot(0, 0);
            this.PieceStorage.AddPiece(flameTrigger);
            flameSlot.locked = true;

            BoardPiece waterTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.FireplaceTriggerOff, world: this.world);
            StorageSlot waterSlot = this.PieceStorage.GetSlot(1, 0);
            this.PieceStorage.AddPiece(waterTrigger);
            waterSlot.locked = true;

            for (int x = 2; x < storageWidth; x++) // locking and hiding the rest of the slots in first row
            {
                StorageSlot slot = this.PieceStorage.GetSlot(x, 0);
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
                    this.sprite.AssignNewName(newAnimName: "on");
                    this.sprite.lightEngine.Activate();
                    ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.Fireplace);
                    this.world.HintEngine.Disable(Tutorials.Type.KeepingAnimalsAway);
                    this.soundPack.Play(PieceSoundPack.Action.TurnOn);
                    this.soundPack.Play(PieceSoundPack.Action.IsOn);
                }
                else
                {
                    this.activeState = State.Empty;
                    this.showStatBarsTillFrame = 0;
                    this.burnAllFuelEndFrame = 0;
                    this.sprite.AssignNewName(newAnimName: "off");
                    this.sprite.lightEngine.Deactivate();
                    ParticleEngine.TurnOff(sprite: this.sprite, preset: ParticleEngine.Preset.Fireplace);
                    this.soundPack.Stop(PieceSoundPack.Action.IsOn);
                    this.soundPack.Play(PieceSoundPack.Action.TurnOff);
                }

                if (Inventory.Layout == Inventory.LayoutType.FieldStorage) Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);
            }
        }

        private List<BoardPiece> StoredFuel
        { get { return this.PieceStorage.GetAllPieces().Where(piece => fuelNames.Contains(piece.name)).ToList(); } }

        private bool StartFire(bool showMessage)
        {
            if (this.world.weather.IsRaining)
            {
                if (showMessage) new TextWindow(text: "I cannot use it during rain.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return false;
            }

            var storedFuel = this.StoredFuel;
            if (storedFuel.Count == 0)
            {
                if (showMessage) new TextWindow(text: "I don't have wood or coal to burn.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                else MessageLog.AddMessage(msgType: MsgType.User, message: $"{Helpers.FirstCharToUpperCase(this.readableName)} has burned out.");
                return false;
            }

            if (!this.IsOn) this.burnStartFrame = this.world.CurrentUpdate;

            BoardPiece fuel = storedFuel[0];
            storedFuel.RemoveAt(0);

            this.PieceStorage.DestroyOneSpecifiedPiece(fuel.name);
            this.currentCycleBurningFramesLeft = fuelFramesByName[fuel.name];

            this.UpdateEndFrame(storedFuel);

            return true;
        }

        private void UpdateEndFrame()
        { this.UpdateEndFrame(this.StoredFuel); }

        private void UpdateEndFrame(List<BoardPiece> storedFuel)
        {
            this.burnAllFuelEndFrame = this.world.CurrentUpdate + this.currentCycleBurningFramesLeft;
            foreach (BoardPiece fuelPiece in storedFuel)
            {
                this.burnAllFuelEndFrame += fuelFramesByName[fuelPiece.name];
            }
            this.showStatBarsTillFrame = this.burnAllFuelEndFrame;
        }

        public override void SM_FireplaceBurn()
        {
            this.currentCycleBurningFramesLeft--;

            bool stopBurning = this.world.weather.IsRaining || (this.currentCycleBurningFramesLeft <= 0 && !this.StartFire(showMessage: false));
            if (stopBurning)
            {
                this.IsOn = false;
                return;
            }

            if (this.world.CurrentUpdate % 10 != 0) return;
            this.UpdateEndFrame();

            var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: this.scareRange, compareWithBottom: true);
            var animalPieces = nearbyPieces.Where(piece => piece.GetType() == typeof(Animal));

            foreach (BoardPiece piece in animalPieces)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"{Helpers.FirstCharToUpperCase(this.readableName)} - scaring off {piece.readableName}.");

                Animal animal = (Animal)piece;
                animal.target = this;
                animal.aiData.Reset();
                animal.activeState = State.AnimalFlee;

                if (PieceInfo.GetInfo(animal.name).isCarnivorous && animal.sprite.IsInCameraRect) this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.AnimalScaredOfFire, ignoreDelay: true, piece: animal);
            }
        }

        public override void DrawStatBar()
        {
            if (this.world.CurrentUpdate < this.burnAllFuelEndFrame)
            {
                int burningDuration = this.burnAllFuelEndFrame - this.burnStartFrame;
                int burningCurrentFrame = burningDuration - (this.world.CurrentUpdate - this.burnStartFrame);

                new StatBar(label: "", value: burningCurrentFrame, valueMax: burningDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 255, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.WoodLogRegular].texture);
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
            this.currentCycleBurningFramesLeft = (int)(Int64)pieceData["fireplace_currentCycleBurningFramesLeft"];
            this.burnStartFrame = (int)(Int64)pieceData["fireplace_burnStartFrame"];
            this.burnAllFuelEndFrame = (int)(Int64)pieceData["fireplace_burnAllFuelEndFrame"];
        }
    }
}