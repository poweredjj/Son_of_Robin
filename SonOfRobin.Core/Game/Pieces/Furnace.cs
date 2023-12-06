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

        private int smeltingStartFrame;
        private int smeltingDoneFrame;
        public bool IsOn { get; private set; }

        private TimeSpan TimeToFinishSmelting
        { get { return TimeSpan.FromSeconds((int)Math.Ceiling((float)(this.smeltingDoneFrame - (float)this.world.CurrentUpdate) / 60f)); } }

        public Furnace(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "off", int maxHitPoints = 1) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, maxHitPoints: maxHitPoints, readableName: readableName, description: description, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty)
        {
            this.IsOn = false;
            this.smeltingDoneFrame = 0;

            this.PieceStorage = new PieceStorage(width: 3, height: 3, storagePiece: this, storageType: PieceStorage.StorageType.Smelting, stackLimit: 1);
            this.ConfigureStorage();
        }

        private StorageSlot[] MatSlots
        {
            get
            {
                var matSlots = new List<StorageSlot>();
                for (int i = 0; i <= 2; i++)
                {
                    matSlots.Add(this.PieceStorage.GetSlot(i, 0));
                }

                return matSlots.ToArray();
            }
        }

        private StorageSlot FuelSlot
        { get { return this.PieceStorage.GetSlot(1, 1); } }

        private StorageSlot FlameTriggerSlot
        { get { return this.PieceStorage.GetSlot(1, 2); } }

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

            StorageSlot fuelSlot = this.FuelSlot;
            fuelSlot.locked = false;
            fuelSlot.hidden = false;
            fuelSlot.allowedPieceNames = new HashSet<PieceTemplate.Name> { PieceTemplate.Name.Coal };
            fuelSlot.label = "coal";
            fuelSlot.stackLimit = 255;

            foreach (StorageSlot matSlot in this.MatSlots)
            {
                matSlot.locked = false;
                matSlot.hidden = false;
                matSlot.label = "material";
                matSlot.allowedPieceNames = new HashSet<PieceTemplate.Name>(transformDict.Keys);
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
            this.IsOn = (bool)pieceData["furnace_IsOn"];
            this.ConfigureStorage();
        }

        public void TurnOn()
        {
            this.IsOn = true;
            this.sprite.AssignNewName(newAnimName: "on");
            this.sprite.lightEngine.Activate();
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.Cooking); // TODO replace with a dedicated particle preset
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.HeatMedium);
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOn);
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.IsOn);
        }

        public void TurnOff()
        {
            this.IsOn = false;
            this.sprite.AssignNewName(newAnimName: "off");
            this.sprite.lightEngine.Deactivate();
            ParticleEngine.TurnOff(sprite: this.sprite, preset: ParticleEngine.Preset.Cooking);
            ParticleEngine.TurnOff(sprite: this.sprite, preset: ParticleEngine.Preset.HeatMedium);
            this.activeSoundPack.Stop(PieceSoundPackTemplate.Action.IsOn);
            this.activeSoundPack.Play(PieceSoundPackTemplate.Action.TurnOff);
            ParticleEngine.TurnOn(sprite: this.sprite, preset: ParticleEngine.Preset.CookingFinish, duration: 8);
        }

        public override void DrawStatBar()
        {
            if (this.world.CurrentUpdate < this.smeltingDoneFrame)
            {
                int cookingDuration = this.smeltingDoneFrame - this.smeltingStartFrame;
                int cookingCurrentFrame = this.world.CurrentUpdate - this.smeltingStartFrame;

                new StatBar(label: "", value: cookingCurrentFrame, valueMax: cookingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.GfxRect.Center.X, posY: this.sprite.GfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.GetCroppedFrameForPackage(AnimData.PkgName.Flame).texture);
            }

            base.DrawStatBar();
        }

        public void Smelt()
        {
            // checking stored pieces

            var allowedMaterials = transformDict.Keys.ToHashSet();
            StorageSlot[] matSlots = this.MatSlots;

            List<BoardPiece> storedMats = new();
            foreach (StorageSlot matSlot in matSlots)
            {
                storedMats.AddRange(matSlot.GetAllPieces(remove: false));
            }

            var processedMats = storedMats.Where(piece => !allowedMaterials.Contains(piece.name)).ToArray();

            if (processedMats.Length > 0)
            {
                string matText = processedMats.Length == 1 ? "material" : "materials";

                new TextWindow(text: $"I have to take out processed {matText} first.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            var storedFuel = this.FuelSlot.GetAllPieces(remove: false);

            if (storedMats.Count == 0 && storedFuel.Count == 0)
            {
                new TextWindow(text: "I need at least one | | | material and some | coal to start smelting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.GlassSand), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw), PieceInfo.GetTexture(PieceTemplate.Name.Coal) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedMats.Count == 0)
            {
                new TextWindow(text: "I don't have any | | | materials.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.IronOre), PieceInfo.GetTexture(PieceTemplate.Name.GlassSand), PieceInfo.GetTexture(PieceTemplate.Name.CoffeeRaw) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedFuel.Count == 0)
            {
                new TextWindow(text: "I need some | coal to start smelting.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Coal) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            this.FuelSlot.GetAllPieces(remove: true);

            Player player = this.world.Player;

            // calculating meal mass

            int smeltingTime = (int)(60 * 60 * 3);
            if (Preferences.debugInstantCookBrewSmelt) smeltingTime = 30;

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

            // blocking the furnace for "smelting duration"

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);
            this.TurnOn();
            new TextWindow(text: "Smelting...", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

            this.smeltingStartFrame = this.world.CurrentUpdate;
            this.smeltingDoneFrame = this.world.CurrentUpdate + smeltingTime;
            this.showStatBarsTillFrame = this.world.CurrentUpdate + smeltingTime;

            new LevelEvent(eventName: LevelEvent.EventName.FinishSmelting, level: this.level, delay: smeltingTime, boardPiece: this);

            // TODO add hint disabling here
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