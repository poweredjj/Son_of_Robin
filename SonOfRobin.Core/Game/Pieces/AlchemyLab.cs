using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AlchemyLab : BoardPiece
    {
        public static readonly List<PieceTemplate.Name> baseNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Apple, PieceTemplate.Name.Cherry, PieceTemplate.Name.Banana, PieceTemplate.Name.Tomato, PieceTemplate.Name.Carrot, PieceTemplate.Name.CoffeeRoasted };

        private static readonly List<PieceTemplate.Name> boosterNames = new List<PieceTemplate.Name> { PieceTemplate.Name.HerbsYellow, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsBlue, PieceTemplate.Name.HerbsBlack, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsViolet, PieceTemplate.Name.HerbsGreen, PieceTemplate.Name.HerbsRed };

        private static readonly List<PieceTemplate.Name> fuelNames = new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLogRegular, PieceTemplate.Name.WoodPlank, PieceTemplate.Name.WoodLogHard };

        public readonly bool canBeUsedDuringRain;
        private readonly int baseSpace;
        private readonly int boosterSpace;

        private int brewingStartFrame;
        private int brewingDoneFrame;
        public bool IsOn { get; private set; }

        private TimeSpan TimeToFinishBrewing
        { get { return TimeSpan.FromSeconds((int)Math.Ceiling((float)(this.brewingDoneFrame - (float)this.world.CurrentUpdate) / 60f)); } }

        public AlchemyLab(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, string readableName, string description, Category category, int baseSpace, int boosterSpace, bool canBeUsedDuringRain, float fireAffinity,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty, soundPack: soundPack, boardTask: Scheduler.TaskName.InteractWithCooker, fireAffinity: fireAffinity)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));

            this.IsOn = false;
            this.brewingDoneFrame = 0;
            this.baseSpace = baseSpace;
            this.boosterSpace = boosterSpace;
            this.canBeUsedDuringRain = canBeUsedDuringRain;

            this.CreateAndConfigureStorage();
        }

        private StorageSlot FlameTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        private StorageSlot BottleSlot
        { get { return this.PieceStorage.GetSlot(1, 0); } }

        private StorageSlot FuelSlot
        { get { return this.PieceStorage.GetSlot(2, 0); } }

        private void CreateAndConfigureStorage()
        {
            byte storageWidth = (byte)Math.Max(this.baseSpace, this.boosterSpace);
            storageWidth = Math.Max(storageWidth, (byte)3);
            byte storageHeight = 3;

            this.PieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, storagePiece: this, storageType: PieceStorage.StorageType.Lab, stackLimit: 1);

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot flameTriggerSlot = this.FlameTriggerSlot;

            flameTriggerSlot.locked = false;
            flameTriggerSlot.hidden = false;
            flameTriggerSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.BrewTrigger };
            BoardPiece flameTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.BrewTrigger, world: this.world);
            flameTriggerSlot.AddPiece(flameTrigger);
            flameTriggerSlot.locked = true;

            StorageSlot bottleSlot = this.BottleSlot;
            bottleSlot.locked = false;
            bottleSlot.hidden = false;
            bottleSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.EmptyBottle, PieceTemplate.Name.PotionGeneric };
            bottleSlot.label = "bottle";

            StorageSlot fuelSlot = this.FuelSlot;
            fuelSlot.locked = false;
            fuelSlot.hidden = false;
            fuelSlot.allowedPieceNames = fuelNames;
            fuelSlot.label = "fuel";

            for (int x = 0; x < this.baseSpace; x++)
            {
                StorageSlot baseSlot = this.PieceStorage.GetSlot(x, 1);
                baseSlot.locked = false;
                baseSlot.hidden = false;
                baseSlot.label = "base";
                baseSlot.allowedPieceNames = baseNames;
            }

            for (int x = 0; x < this.boosterSpace; x++)
            {
                StorageSlot boosterSlot = this.PieceStorage.GetSlot(x, 2);
                boosterSlot.locked = false;
                boosterSlot.hidden = false;
                boosterSlot.label = "booster";
                boosterSlot.allowedPieceNames = boosterNames;
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();

            pieceData["lab_brewingStartFrame"] = this.brewingStartFrame;
            pieceData["lab_brewingDoneFrame"] = this.brewingDoneFrame;
            pieceData["lab_IsOn"] = this.IsOn;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.brewingStartFrame = (int)(Int64)pieceData["lab_brewingStartFrame"];
            this.brewingDoneFrame = (int)(Int64)pieceData["lab_brewingDoneFrame"];
            this.IsOn = (bool)pieceData["lab_IsOn"];
        }

        public void TurnOn()
        {
            this.IsOn = true;
            this.sprite.AssignNewName(animName: "on");
            this.sprite.lightEngine.Activate();
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);
            this.soundPack.Play(PieceSoundPack.Action.IsOn);
        }

        public void TurnOff()
        {
            this.IsOn = false;
            this.sprite.AssignNewName(animName: "off");
            this.sprite.lightEngine.Deactivate();
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.soundPack.Play(PieceSoundPack.Action.TurnOff);
        }

        public override void DrawStatBar()
        {
            if (this.world.CurrentUpdate < this.brewingDoneFrame)
            {
                int cookingDuration = this.brewingDoneFrame - this.brewingStartFrame;
                int cookingCurrentFrame = this.world.CurrentUpdate - this.brewingStartFrame;

                new StatBar(label: "", value: cookingCurrentFrame, valueMax: cookingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.Flame].texture);
            }

            base.DrawStatBar();
            StatBar.FinishThisBatch();
        }

        public void Brew()
        {
            // checking stored pieces

            if (!BottleSlot.IsEmpty)
            {
                BoardPiece bottle = BottleSlot.TopPiece;
                if (bottle.name != PieceTemplate.Name.EmptyBottle)
                {
                    new TextWindow(text: "I have to take out previously brewed | potion first.", imageList: new List<Texture2D> { bottle.sprite.frame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                    return;
                }
            }

            var storedBases = this.PieceStorage.GetAllPieces().Where(piece => baseNames.Contains(piece.name)).ToList();
            var storedBoosters = this.PieceStorage.GetAllPieces().Where(piece => boosterNames.Contains(piece.name)).ToList();
            var storedFuel = this.PieceStorage.GetAllPieces().Where(piece => fuelNames.Contains(piece.name)).ToList();

            if (!storedBases.Any() && !storedBoosters.Any() && !storedFuel.Any())
            {
                new TextWindow(text: "I need at least one | | | base, one booster | and | fuel to cook.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.Tomato), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.HerbsRed), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (!storedFuel.Any())
            {
                string fuelMarkers = "";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name fuel in fuelNames)
                {
                    fuelMarkers += "| ";
                    imageList.Add(PieceInfo.GetInfo(fuel).texture);
                }

                new TextWindow(text: $"I don't have any {fuelMarkers} fuel.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (!storedBases.Any())
            {
                string baseMarkers = "";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name baseName in baseNames)
                {
                    baseMarkers += "| ";
                    imageList.Add(PieceInfo.GetInfo(baseName).texture);
                }

                new TextWindow(text: $"I don't have any {baseMarkers} bases.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (!storedBoosters.Any())
            {
                string boosterMarkers = "";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name booster in boosterNames)
                {
                    boosterMarkers += "| ";
                    imageList.Add(PieceInfo.GetInfo(booster).texture);
                }

                new TextWindow(text: $"I don't have any {boosterMarkers} boosters.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            // getting all buffs from boosters

            var buffList = new List<Buff>();

            foreach (BoardPiece booster in storedBoosters)
            {
                if (booster.buffList == null)
                {
                    MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Booster {booster.readableName} has no buffList - ignoring.");
                    continue;
                }
                buffList.AddRange(booster.buffList);
            }

            // creating potion

            BoardPiece potion = PieceTemplate.Create(templateName: PieceTemplate.Name.PotionGeneric, world: this.world);
            buffList = BuffEngine.MergeSameTypeBuffsInList(world: this.world, buffList: buffList); // merging the same buffs (to add values of non-stackable buffs)
            potion.buffList = buffList;
            this.PieceStorage.AddPiece(piece: potion, dropIfDoesNotFit: true);

            // destroying every inserted piece

            var piecesToDestroyList = new List<BoardPiece>();
            piecesToDestroyList.AddRange(storedBases);
            piecesToDestroyList.AddRange(storedBoosters);
            piecesToDestroyList.AddRange(storedFuel);

            var piecesToDestroyDict = new Dictionary<PieceTemplate.Name, byte> { };
            foreach (BoardPiece pieceToDestroy in piecesToDestroyList)
            {
                if (piecesToDestroyDict.ContainsKey(pieceToDestroy.name)) piecesToDestroyDict[pieceToDestroy.name]++;
                else piecesToDestroyDict[pieceToDestroy.name] = 1;
            }

            this.PieceStorage.DestroySpecifiedPieces(piecesToDestroyDict);

            // blocking the lab for "cooking duration"

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);
            this.TurnOn();
            new TextWindow(text: "Brewing...", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

            int brewingTime = 60 * 30;

            this.brewingStartFrame = this.world.CurrentUpdate;
            this.brewingDoneFrame = this.world.CurrentUpdate + brewingTime;
            this.showStatBarsTillFrame = this.world.CurrentUpdate + brewingTime;

            new WorldEvent(eventName: WorldEvent.EventName.FinishBrewing, world: this.world, delay: brewingTime, boardPiece: this);

            // this.world.HintEngine.Disable(PieceHint.Type.Cooker); // TODO add AlchemyLab PieceHint
            // this.world.HintEngine.Disable(Tutorials.Type.Cook); // TODO add PotionBrew Tutorial
        }

        public void ShowCookingProgress()
        {
            new TextWindow(text: $"Cooking will be done in {TimeSpanToString(this.TimeToFinishBrewing)}.", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
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