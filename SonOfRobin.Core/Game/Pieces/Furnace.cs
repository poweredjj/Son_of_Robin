using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Furnace : BoardPiece
    {
        public static readonly Dictionary<PieceTemplate.Name, PieceTemplate.Name> transformDict = new()
        {
            { PieceTemplate.Name.IronOre, PieceTemplate.Name.IronBar },
            { PieceTemplate.Name.GlassSand, PieceTemplate.Name.EmptyBottle },
            { PieceTemplate.Name.CoffeeRaw, PieceTemplate.Name.CoffeeRoasted },
        };

        private readonly byte storageWidth;

        private int smeltingStartFrame;
        private int smeltingDoneFrame;
        public bool IsOn { get; private set; }

        private TimeSpan TimeToFinishSmelting
        { get { return TimeSpan.FromSeconds((int)Math.Ceiling((float)(this.smeltingDoneFrame - (float)this.world.CurrentUpdate) / 60f)); } }

        public Furnace(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty)
        {
            this.storageWidth = 5;
            this.IsOn = false;
            this.smeltingDoneFrame = 0;

            this.PieceStorage = new PieceStorage(width: this.storageWidth, height: 3, storagePiece: this, storageType: PieceStorage.StorageType.Smelting, stackLimit: 1);
            this.ConfigureStorage();
        }

        private StorageSlot[] MatSlots
        {
            get
            {
                var matSlots = new StorageSlot[this.storageWidth];

                for (int i = 0; i < this.storageWidth; i++)
                {
                    matSlots[i] = this.PieceStorage.GetSlot(i, 0);
                }

                return matSlots;
            }
        }

        private StorageSlot[] FuelSlots
        {
            get
            {
                var fuelSlots = new StorageSlot[this.storageWidth];

                for (int i = 0; i < this.storageWidth; i++)
                {
                    fuelSlots[i] = this.PieceStorage.GetSlot(i, 1);
                }

                return fuelSlots;
            }
        }

        private StorageSlot FlameTriggerSlot
        { get { return this.PieceStorage.GetSlot(this.storageWidth / 2, 2); } }

        private void ConfigureStorage()
        {
            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot flameTriggerSlot = this.FlameTriggerSlot;

            flameTriggerSlot.locked = false;
            flameTriggerSlot.hidden = false;
            if (flameTriggerSlot.IsEmpty) flameTriggerSlot.AddPiece(PieceTemplate.CreatePiece(templateName: PieceTemplate.Name.SmeltingTrigger, world: this.world));
            flameTriggerSlot.locked = true;

            foreach (StorageSlot matSlot in this.MatSlots)
            {
                matSlot.locked = false;
                matSlot.hidden = false;
                matSlot.label = "material";
                matSlot.stackLimit = 1;
                matSlot.allowedPieceNames = new HashSet<PieceTemplate.Name>(transformDict.Keys);
            }

            foreach (StorageSlot fuelSlot in this.FuelSlots)
            {
                fuelSlot.locked = false;
                fuelSlot.hidden = false;
                fuelSlot.label = "coal";
                fuelSlot.stackLimit = 1;
                fuelSlot.allowedPieceNames = new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Coal };
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["furnace_smeltingStartFrame"] = this.smeltingStartFrame;
            pieceData["furnace_smeltingDoneFrame"] = this.smeltingDoneFrame;
            pieceData["furnace_IsOn"] = this.IsOn;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.smeltingStartFrame = (int)(Int64)pieceData["furnace_smeltingStartFrame"];
            this.smeltingDoneFrame = (int)(Int64)pieceData["furnace_smeltingDoneFrame"];
            if ((bool)pieceData["furnace_IsOn"]) this.TurnOn();
            this.ConfigureStorage();
        }

        public void TurnOn()
        {
            if (this.visualAid == null || !this.visualAid.exists)
            {
                // visualAid is needed to emit particles from the chimney (cannot emit from secondary location using only one emitter)
                this.visualAid = PieceTemplate.CreateAndPlaceOnBoard(templateName: PieceTemplate.Name.ParticleEmitterEnding, world: this.world, position: new Vector2(this.sprite.GfxRect.Center.X, this.sprite.GfxRect.Top + 5), randomPlacement: false, precisePlacement: true);
                this.visualAid.sprite.AssignNewPackage(newAnimPackage: AnimData.PkgName.WhiteSpotLayerTwo, checkForCollision: false);
                this.visualAid.sprite.opacity = 0.001f; // to draw particles only
                ParticleEngine.TurnOn(sprite: this.visualAid.sprite, preset: ParticleEngine.Preset.Smelting);
            }

            this.IsOn = true;
            this.sprite.AssignNewName(newAnimName: "on");
            this.sprite.lightEngine.Activate();
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.HeatSmelting);
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOn);
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.IsOn);
        }

        public void TurnOff()
        {
            this.IsOn = false;
            this.sprite.AssignNewName(newAnimName: "off");
            this.sprite.lightEngine.Deactivate();

            ParticleEngine.TurnOff(sprite: this.sprite, preset: ParticleEngine.Preset.HeatSmelting);
            if (this.visualAid != null) ParticleEngine.TurnOff(sprite: this.visualAid.sprite, preset: ParticleEngine.Preset.Smelting);

            this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.IsOn);
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOff);
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.CookingFinish, duration: 8);
        }

        public override void DrawStatBar()
        {
            if (this.world.CurrentUpdate < this.smeltingDoneFrame)
            {
                int smeltingDuration = this.smeltingDoneFrame - this.smeltingStartFrame;
                int smeltingCurrentFrame = this.world.CurrentUpdate - this.smeltingStartFrame;

                new StatBar(label: "", value: smeltingCurrentFrame, valueMax: smeltingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.GetCroppedFrameForPackage(AnimData.PkgName.Flame).texture);
            }

            base.DrawStatBar();
        }

        public void Smelt()
        {
            // checking stored pieces

            var allowedMaterials = transformDict.Keys.ToHashSet();
            StorageSlot[] matSlots = this.MatSlots;
            StorageSlot[] fuelSlots = this.FuelSlots;

            List<BoardPiece> storedFuel = new();
            foreach (StorageSlot fuelSlot in fuelSlots)
            {
                storedFuel.AddRange(fuelSlot.GetAllPieces(remove: false));
            }

            List<BoardPiece> storedMats = new();
            foreach (StorageSlot matSlot in matSlots)
            {
                storedMats.AddRange(matSlot.GetAllPieces(remove: false));
            }

            int processedMatsCount = storedMats.Where(piece => !allowedMaterials.Contains(piece.name)).Count();

            if (processedMatsCount > 0)
            {
                string matText = processedMatsCount == 1 ? "material" : "materials";

                new TextWindow(text: $"I have to take out processed {matText} first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedMats.Count == 0 && storedFuel.Count == 0)
            {
                new TextWindow(text: $"I need at least one | | | material and some | {PieceInfo.GetInfo(PieceTemplate.Name.Coal).readableName} to start smelting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.GlassSand), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw), PieceInfo.GetTexture(PieceTemplate.Name.Coal) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedMats.Count == 0)
            {
                new TextWindow(text: "I don't have any | | | materials.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.GlassSand), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedFuel.Count == 0)
            {
                new TextWindow(text: $"I need some | {PieceInfo.GetInfo(PieceTemplate.Name.Coal).readableName} to start smelting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Coal) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedMats.Count > storedFuel.Count)
            {
                new TextWindow(text: $"I need more | {PieceInfo.GetInfo(PieceTemplate.Name.Coal).readableName}.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Coal) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            Player player = this.world.Player;

            // calculating smelting time

            int smeltingTime = Preferences.debugInstantCookBrewSmelt ? 30 : 60 * 60 * storedMats.Count;

            // registering stats

            this.world.smeltStats.RegisterCooking(baseList: storedMats);

            // transforming materials

            foreach (StorageSlot matSlot in matSlots)
            {
                if (!matSlot.IsEmpty)
                {
                    BoardPiece material = matSlot.GetAllPieces(remove: true)[0];
                    BoardPiece processedMaterial = PieceTemplate.CreatePiece(templateName: transformDict[material.name], world: this.world);

                    matSlot.allowedPieceNames.Add(processedMaterial.name);
                    matSlot.AddPiece(processedMaterial);
                    matSlot.allowedPieceNames.Remove(processedMaterial.name);
                }
            }

            // removing used coal

            int fuelRemovedCount = 0;
            foreach (StorageSlot fuelSlot in fuelSlots)
            {
                List<BoardPiece> fuelPieces = fuelSlot.GetAllPieces(remove: true);
                fuelRemovedCount += fuelPieces.Count;
                if (fuelRemovedCount >= storedMats.Count) break;
            }

            // blocking the furnace for "smelting duration"

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);
            this.TurnOn();
            new TextWindow(text: "Smelting...", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

            this.smeltingStartFrame = this.world.CurrentUpdate;
            this.smeltingDoneFrame = this.world.CurrentUpdate + smeltingTime;
            this.showStatBarsTillFrame = this.world.CurrentUpdate + smeltingTime;

            new LevelEvent(eventName: LevelEvent.EventName.FinishSmelting, level: this.level, delay: smeltingTime, boardPiece: this);

            this.world.HintEngine.Disable(PieceHint.Type.Furnace);
            this.world.HintEngine.Disable(Tutorials.Type.Smelt);
        }

        public void ShowSmeltingProgress()
        {
            new TextWindow(text: $"Smelting will be done in {TimeSpanToString(this.TimeToFinishSmelting)}.", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            string timeLeftString;

            if (timeSpan < TimeSpan.FromMinutes(1)) timeLeftString = $"{timeSpan.TotalSeconds} s";
            else timeLeftString = timeSpan.ToString("mm\\:ss");

            return timeLeftString;
        }
    }
}