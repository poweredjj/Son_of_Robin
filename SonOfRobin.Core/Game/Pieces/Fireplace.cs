using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Fireplace : BoardPiece
    {
        private static readonly Dictionary<PieceTemplate.Name, int> fuelFramesByName = new()
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
        private int currentCycleEndFrame;
        private int burnStartFrame;
        private int burnAllFuelEndFrame;

        public Fireplace(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, byte storageWidth, byte storageHeight, string readableName, string description, ushort scareRange,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1, LightEngine lightEngine = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, lightEngine: lightEngine, activeState: State.Empty)
        {
            this.scareRange = scareRange;
            this.isOn = false;
            this.burnStartFrame = 0;
            this.currentCycleEndFrame = 0;
            this.burnAllFuelEndFrame = 0;

            this.PieceStorage = new PieceStorage(width: storageWidth, height: (byte)(storageHeight + 1), storagePiece: this, storageType: PieceStorage.StorageType.Fireplace);

            var allowedPieceNames = new HashSet<PieceTemplate.Name>(fuelNames)
            {
                PieceTemplate.Name.FireplaceTriggerOn,
                PieceTemplate.Name.FireplaceTriggerOff,
            };
            this.PieceStorage.AssignAllowedPieceNames(allowedPieceNames);

            BoardPiece flameTrigger = PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.FireplaceTriggerOn, world: this.world);

            StorageSlot flameSlot = this.PieceStorage.GetSlot(0, 0);
            this.PieceStorage.AddPiece(flameTrigger);
            flameSlot.locked = true;

            BoardPiece waterTrigger = PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.FireplaceTriggerOff, world: this.world);
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
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOn);
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.IsOn);
                }
                else
                {
                    this.activeState = State.Empty;
                    this.showStatBarsTillFrame = 0;
                    this.burnAllFuelEndFrame = 0;
                    this.sprite.AssignNewName(newAnimName: "off");
                    this.sprite.lightEngine.Deactivate();
                    ParticleEngine.TurnOff(sprite: this.sprite, preset: ParticleEngine.Preset.Fireplace);
                    this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.IsOn);
                    this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOff);
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
                else MessageLog.AddMessage(message: $"{Helpers.FirstCharToUpperCase(this.readableName)} has burned out.");
                return false;
            }

            if (!this.IsOn) this.burnStartFrame = this.world.CurrentUpdate;

            PieceTemplate.Name currentlyBurningFuelName = storedFuel[0].name;

            this.PieceStorage.DestroyOneSpecifiedPiece(currentlyBurningFuelName);
            this.currentCycleEndFrame = this.world.CurrentUpdate + fuelFramesByName[currentlyBurningFuelName];

            this.UpdateBurnAllFuelEndFrame();

            return true;
        }

        private void UpdateBurnAllFuelEndFrame()
        {
            this.burnAllFuelEndFrame = this.currentCycleEndFrame;
            foreach (BoardPiece fuelPiece in this.StoredFuel)
            {
                this.burnAllFuelEndFrame += fuelFramesByName[fuelPiece.name];
            }
            this.showStatBarsTillFrame = this.burnAllFuelEndFrame;
        }

        public override void SM_FireplaceBurn()
        {
            if (this.world.CurrentUpdate % 15 != 0) return;

            bool stopBurning = this.world.weather.IsRaining || (this.world.CurrentUpdate >= this.currentCycleEndFrame && !this.StartFire(showMessage: false));
            if (stopBurning)
            {
                this.IsOn = false;
                return;
            }

            this.UpdateBurnAllFuelEndFrame();

            var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: this.scareRange, compareWithBottom: true);
            var animalPieces = nearbyPieces.Where(piece => piece.GetType() == typeof(Animal));

            bool hintShown = this.world.HintEngine.shownGeneralHints.Contains(HintEngine.Type.AnimalScaredOfFire);

            foreach (BoardPiece piece in animalPieces)
            {
                MessageLog.AddMessage(debugMessage: true, message: $"{Helpers.FirstCharToUpperCase(this.readableName)} - scaring off {piece.readableName}.");

                Animal animal = (Animal)piece;
                animal.target = this;
                animal.aiData.Reset();
                animal.activeState = State.AnimalFlee;

                if (!hintShown && PieceInfo.GetInfo(animal.name).isCarnivorous && animal.sprite.IsInCameraRect)
                {
                    this.world.HintEngine.ShowGeneralHint(type: HintEngine.Type.AnimalScaredOfFire, ignoreDelay: true, piece: animal);
                    hintShown = true;
                }
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
            pieceData["fireplace_burnStartFrame"] = this.burnStartFrame;
            pieceData["fireplace_currentCycleEndFrame"] = this.currentCycleEndFrame;
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.isOn = (bool)pieceData["fireplace_isOn"];
            this.burnStartFrame = (int)(Int64)pieceData["fireplace_burnStartFrame"];

            this.currentCycleEndFrame = (int)(Int64)pieceData["fireplace_currentCycleEndFrame"];

            this.UpdateBurnAllFuelEndFrame();
        }
    }
}