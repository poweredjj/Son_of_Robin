﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AlchemyLab : BoardPiece
    {
        public static readonly List<PieceTemplate.Name> baseNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Apple, PieceTemplate.Name.Cherry, PieceTemplate.Name.Banana, PieceTemplate.Name.Tomato, PieceTemplate.Name.Carrot, PieceTemplate.Name.CoffeeRoasted, PieceTemplate.Name.Fat };

        private static readonly List<PieceTemplate.Name> boosterNames = new List<PieceTemplate.Name> { PieceTemplate.Name.HerbsYellow, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsBlue, PieceTemplate.Name.HerbsBlack, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsViolet, PieceTemplate.Name.HerbsGreen, PieceTemplate.Name.HerbsRed };

        private static readonly List<PieceTemplate.Name> fuelNames = new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLogRegular, PieceTemplate.Name.WoodPlank, PieceTemplate.Name.WoodLogHard };

        private static readonly List<PieceTemplate.Name> potionNames = new List<PieceTemplate.Name> { PieceTemplate.Name.EmptyBottle, PieceTemplate.Name.BottleOfOil, PieceTemplate.Name.PotionCoffee, PieceTemplate.Name.PotionGeneric };

        private readonly int boosterSpace;

        private int brewingStartFrame;
        private int brewingDoneFrame;
        public bool IsOn { get; private set; }

        private TimeSpan TimeToFinishBrewing
        { get { return TimeSpan.FromSeconds((int)Math.Ceiling((float)(this.brewingDoneFrame - (float)this.world.CurrentUpdate) / 60f)); } }

        public AlchemyLab(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, string readableName, string description, Category category, int boosterSpace, float fireAffinity,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty, soundPack: soundPack, boardTask: Scheduler.TaskName.InteractWithLab, fireAffinity: fireAffinity)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));

            this.IsOn = false;
            this.brewingDoneFrame = 0;
            this.boosterSpace = boosterSpace;

            this.CreateAndConfigureStorage();
        }

        private StorageSlot FlameTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        private StorageSlot BottleSlot
        { get { return this.PieceStorage.GetSlot(1, 0); } }

        private StorageSlot BaseSlot
        { get { return this.PieceStorage.GetSlot(2, 0); } }

        private StorageSlot FuelSlot
        { get { return this.PieceStorage.GetSlot(3, 0); } }

        private void CreateAndConfigureStorage()
        {
            byte storageWidth = (byte)(this.boosterSpace + 1);
            storageWidth = Math.Max(storageWidth, (byte)4);
            byte storageHeight = 2;

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
            bottleSlot.allowedPieceNames = potionNames;
            bottleSlot.label = "bottle";

            StorageSlot fuelSlot = this.FuelSlot;
            fuelSlot.locked = false;
            fuelSlot.hidden = false;
            fuelSlot.allowedPieceNames = fuelNames;
            fuelSlot.label = "fuel";

            StorageSlot baseSlot = this.BaseSlot;
            baseSlot.locked = false;
            baseSlot.hidden = false;
            baseSlot.label = "base";
            baseSlot.allowedPieceNames = baseNames;

            for (int x = 0; x < this.boosterSpace; x++)
            {
                StorageSlot boosterSlot = this.PieceStorage.GetSlot(x + 1, 1);
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
                int brewingDuration = this.brewingDoneFrame - this.brewingStartFrame;
                int brewingCurrentFrame = this.world.CurrentUpdate - this.brewingStartFrame;

                new StatBar(label: "", value: brewingCurrentFrame, valueMax: brewingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.Flame].texture);
            }

            base.DrawStatBar();
            StatBar.FinishThisBatch();
        }

        public void Brew()
        {
            // checking stored pieces

            if (BottleSlot.IsEmpty)
            {
                {
                    new TextWindow(text: $"I need an | { PieceInfo.GetInfo(PieceTemplate.Name.EmptyBottle).readableName }.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.EmptyBottle) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                    return;
                }
            }
            else
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
            var storedPotions = this.PieceStorage.GetAllPieces().Where(piece => potionNames.Contains(piece.name)).ToList();

            if (!storedBases.Any() && !storedBoosters.Any() && !storedFuel.Any())
            {
                new TextWindow(text: "I need at least one | | | base and | fuel to brew.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.Apple), PieceInfo.GetTexture(PieceTemplate.Name.Tomato), PieceInfo.GetTexture(PieceTemplate.Name.Banana), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
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

                new TextWindow(text: $"I don't have any {baseMarkers} base.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            // getting all buffs from base and boosters

            var buffList = new List<Buff> { };

            var listsToGetBoostersFrom = new List<List<BoardPiece>> { storedBases, storedBoosters };

            foreach (List<BoardPiece> pieceList in listsToGetBoostersFrom)
            {
                foreach (BoardPiece booster in pieceList)
                {
                    if (booster.buffList == null)
                    {
                        MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Booster {booster.readableName} has no buffList - ignoring.");
                        continue;
                    }
                    buffList.AddRange(booster.buffList);
                }
            }

            bool customPotion = true;
            PieceTemplate.Name potionName = PieceTemplate.Name.PotionGeneric;

            {
                PieceTemplate.Name baseName = storedBases[0].name;

                switch (baseName)
                {
                    case PieceTemplate.Name.Fat:
                        potionName = PieceTemplate.Name.BottleOfOil;
                        break;

                    case PieceTemplate.Name.CoffeeRoasted:
                        potionName = PieceTemplate.Name.PotionCoffee;
                        break;

                    default:
                        customPotion = false;
                        break;
                }
            }

            // creating potion

            BoardPiece potion = PieceTemplate.Create(templateName: potionName, world: this.world);
            buffList = BuffEngine.MergeSameTypeBuffsInList(world: this.world, buffList: buffList); // merging the same buffs (to add values of non-stackable buffs)
            potion.buffList = buffList;

            // setting potion color

            if (!customPotion && storedBoosters.Any())
            {
                var colorByBoosterDict = new Dictionary<PieceTemplate.Name, AnimData.PkgName> {
                    { PieceTemplate.Name.HerbsBlack, AnimData.PkgName.PotionBlack  },
                    { PieceTemplate.Name.HerbsBlue, AnimData.PkgName.PotionBlue  },
                    { PieceTemplate.Name.HerbsCyan, AnimData.PkgName.PotionCyan  },
                    { PieceTemplate.Name.HerbsGreen, AnimData.PkgName.PotionGreen  },
                    { PieceTemplate.Name.HerbsRed, AnimData.PkgName.PotionRed  },
                    { PieceTemplate.Name.HerbsViolet, AnimData.PkgName.PotionViolet  },
                    { PieceTemplate.Name.HerbsYellow, AnimData.PkgName.PotionYellow  },
                };

                PieceTemplate.Name boosterName = storedBoosters[0].name;

                potion.sprite.AssignNewPackage(colorByBoosterDict[boosterName]);
            }

            // destroying every inserted piece

            var piecesToDestroyList = new List<BoardPiece>();
            piecesToDestroyList.AddRange(storedBases);
            piecesToDestroyList.AddRange(storedBoosters);
            piecesToDestroyList.AddRange(storedFuel);
            piecesToDestroyList.AddRange(storedPotions);

            var piecesToDestroyDict = new Dictionary<PieceTemplate.Name, byte> { };
            foreach (BoardPiece pieceToDestroy in piecesToDestroyList)
            {
                if (piecesToDestroyDict.ContainsKey(pieceToDestroy.name)) piecesToDestroyDict[pieceToDestroy.name]++;
                else piecesToDestroyDict[pieceToDestroy.name] = 1;
            }
            this.PieceStorage.DestroySpecifiedPieces(piecesToDestroyDict);

            // inserting potion

            this.PieceStorage.AddPiece(piece: potion, dropIfDoesNotFit: true);

            // blocking the lab for "brewing duration"

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);
            this.TurnOn();
            new TextWindow(text: "Brewing...", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

            int brewingTime = 60 * 15 * (1 + storedBoosters.Count());
            // brewingTime = 80; // for testing

            this.brewingStartFrame = this.world.CurrentUpdate;
            this.brewingDoneFrame = this.world.CurrentUpdate + brewingTime;
            this.showStatBarsTillFrame = this.world.CurrentUpdate + brewingTime;

            new WorldEvent(eventName: WorldEvent.EventName.FinishBrewing, world: this.world, delay: brewingTime, boardPiece: this);

            this.world.HintEngine.Disable(PieceHint.Type.AlchemyLab);
            this.world.HintEngine.Disable(Tutorials.Type.PotionBrew);
        }

        public void ShowBrewingProgress()
        {
            new TextWindow(text: $"Brewing will be done in { TimeSpanToString(this.TimeToFinishBrewing) }.", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
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