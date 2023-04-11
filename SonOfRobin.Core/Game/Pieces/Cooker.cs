using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Cooker : BoardPiece
    {
        public static readonly List<PieceTemplate.Name> ingredientNames = new List<PieceTemplate.Name> { PieceTemplate.Name.MeatRaw, PieceTemplate.Name.Apple, PieceTemplate.Name.Cherry, PieceTemplate.Name.Banana, PieceTemplate.Name.Tomato, PieceTemplate.Name.Acorn, PieceTemplate.Name.Clam, PieceTemplate.Name.Fat };

        private static readonly List<PieceTemplate.Name> boosterNames = new List<PieceTemplate.Name> { PieceTemplate.Name.HerbsYellow, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsBlue, PieceTemplate.Name.HerbsBlack, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsViolet, PieceTemplate.Name.HerbsGreen }; // herbs that increase HP should not be accepted here (player should use potions for HP restoration)

        private static readonly List<PieceTemplate.Name> fuelNames = new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLogRegular, PieceTemplate.Name.WoodPlank, PieceTemplate.Name.WoodLogHard };

        public readonly bool canBeUsedDuringRain;
        private readonly int ingredientSpace;
        private readonly int boosterSpace;

        private readonly float foodMassMultiplier;
        private int cookingStartFrame;
        private int cookingDoneFrame;

        private TimeSpan TimeToFinishCooking
        { get { return TimeSpan.FromSeconds((int)(Math.Ceiling((float)(this.cookingDoneFrame - (float)this.world.CurrentUpdate) / 60f))); } }

        public Cooker(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, int[] maxMassForSize, float foodMassMultiplier, string readableName, string description, Category category, int ingredientSpace, int boosterSpace, bool canBeUsedDuringRain,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, PieceSoundPack soundPack = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), activeState: State.Empty, soundPack: soundPack)
        {
            this.boardTask = Scheduler.TaskName.OpenContainer;

            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOn, sound: new Sound(name: SoundData.Name.StartFireBig));
            this.soundPack.AddAction(action: PieceSoundPack.Action.TurnOff, sound: new Sound(name: SoundData.Name.EndFire));

            this.cookingDoneFrame = 0;
            this.foodMassMultiplier = foodMassMultiplier;
            this.ingredientSpace = ingredientSpace;
            this.boosterSpace = boosterSpace;
            this.canBeUsedDuringRain = canBeUsedDuringRain;

            this.CreateAndConfigureStorage();
        }

        private StorageSlot FlameTriggerSlot
        { get { return this.PieceStorage.GetSlot(0, 0); } }

        private StorageSlot MealSlot
        { get { return this.PieceStorage.GetSlot(1, 0); } }

        private StorageSlot FuelSlot
        { get { return this.PieceStorage.GetSlot(2, 0); } }

        private void CreateAndConfigureStorage()
        {
            byte storageWidth = (byte)Math.Max(this.ingredientSpace, this.boosterSpace);
            storageWidth = Math.Max(storageWidth, (byte)3);
            byte storageHeight = 3;

            this.PieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, storagePiece: this, storageType: PieceStorage.StorageType.Cooking, stackLimit: 1);

            foreach (StorageSlot slot in this.PieceStorage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot flameTriggerSlot = this.FlameTriggerSlot;

            flameTriggerSlot.locked = false;
            flameTriggerSlot.hidden = false;
            flameTriggerSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.CookingTrigger };
            BoardPiece flameTrigger = PieceTemplate.Create(templateName: PieceTemplate.Name.CookingTrigger, world: this.world);
            flameTriggerSlot.AddPiece(flameTrigger);
            flameTriggerSlot.locked = true;

            StorageSlot mealSlot = this.MealSlot;
            mealSlot.locked = false;
            mealSlot.hidden = false;
            mealSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Meal };
            mealSlot.label = "meal";

            StorageSlot fuelSlot = this.FuelSlot;
            fuelSlot.locked = false;
            fuelSlot.hidden = false;
            fuelSlot.allowedPieceNames = fuelNames;
            fuelSlot.label = "fuel";

            for (int x = 0; x < this.ingredientSpace; x++)
            {
                StorageSlot ingredientSlot = this.PieceStorage.GetSlot(x, 1);
                ingredientSlot.locked = false;
                ingredientSlot.hidden = false;
                ingredientSlot.label = "ingredient";
                ingredientSlot.allowedPieceNames = ingredientNames;
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

            pieceData["cooker_cookingStartFrame"] = this.cookingStartFrame;
            pieceData["cooker_cookingDoneFrame"] = this.cookingDoneFrame;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.cookingStartFrame = (int)(Int64)pieceData["cooker_cookingStartFrame"];
            this.cookingDoneFrame = (int)(Int64)pieceData["cooker_cookingDoneFrame"];
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(animName: "on");
            this.sprite.lightEngine.Activate();
            this.soundPack.Play(PieceSoundPack.Action.TurnOn);
            this.soundPack.Play(PieceSoundPack.Action.IsOn);
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(animName: "off");
            this.sprite.lightEngine.Deactivate();
            this.soundPack.Stop(PieceSoundPack.Action.IsOn);
            this.soundPack.Play(PieceSoundPack.Action.TurnOff);
        }

        public override void DrawStatBar()
        {
            if (this.world.CurrentUpdate < this.cookingDoneFrame)
            {
                int cookingDuration = this.cookingDoneFrame - this.cookingStartFrame;
                int cookingCurrentFrame = this.world.CurrentUpdate - this.cookingStartFrame;

                new StatBar(label: "", value: cookingCurrentFrame, valueMax: cookingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.Flame].texture);
            }

            base.DrawStatBar();
            StatBar.FinishThisBatch();
        }

        public void Cook()
        {
            // checking stored pieces

            if (!MealSlot.IsEmpty)
            {
                BoardPiece mealLeftInside = MealSlot.TopPiece;

                new TextWindow(text: "I have to take out previously cooked | meal first.", imageList: new List<Texture2D> { mealLeftInside.sprite.frame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            var storedIngredients = this.PieceStorage.GetAllPieces().Where(piece => ingredientNames.Contains(piece.name)).ToList();
            var storedBoosters = this.PieceStorage.GetAllPieces().Where(piece => boosterNames.Contains(piece.name)).ToList();
            var storedFuel = this.PieceStorage.GetAllPieces().Where(piece => fuelNames.Contains(piece.name)).ToList();

            if (storedIngredients.Count == 0 && storedFuel.Count == 0)
            {
                new TextWindow(text: "I need at least one | | | ingredient and | fuel to cook.", imageList: new List<Texture2D> { PieceInfo.GetTexture(PieceTemplate.Name.MeatRaw), PieceInfo.GetTexture(PieceTemplate.Name.Tomato), PieceInfo.GetTexture(PieceTemplate.Name.Clam), PieceInfo.GetTexture(PieceTemplate.Name.WoodLogRegular) }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            if (storedFuel.Count == 0)
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

            if (storedIngredients.Count == 0)
            {
                string ingredientMarkers = "";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name ingredient in ingredientNames)
                {
                    ingredientMarkers += "| ";
                    imageList.Add(PieceInfo.GetInfo(ingredient).texture);
                }

                new TextWindow(text: $"I don't have any {ingredientMarkers} ingredients.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true, animSound: this.world.DialogueSound);
                return;
            }

            // calculating meal mass

            float cookedMass = 0;

            foreach (BoardPiece ingredient in storedIngredients)
            {
                cookedMass += ingredient.Mass;
            }
            int cookingTime = (int)(cookedMass * 6);
            cookedMass *= foodMassMultiplier * this.world.Player.cookingSkill;

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

            // creating meal

            BoardPiece meal = PieceTemplate.Create(templateName: PieceTemplate.Name.Meal, world: this.world);
            buffList = BuffEngine.MergeSameTypeBuffsInList(world: this.world, buffList: buffList); // merging the same buffs (to add values of non-stackable buffs)
            meal.buffList = buffList;
            this.PieceStorage.AddPiece(piece: meal, dropIfDoesNotFit: true);

            // destroying every inserted piece

            var piecesToDestroyList = new List<BoardPiece>();
            piecesToDestroyList.AddRange(storedIngredients);
            piecesToDestroyList.AddRange(storedBoosters);
            piecesToDestroyList.AddRange(storedFuel);

            var piecesToDestroyDict = new Dictionary<PieceTemplate.Name, byte> { };
            foreach (BoardPiece pieceToDestroy in piecesToDestroyList)
            {
                if (piecesToDestroyDict.ContainsKey(pieceToDestroy.name)) piecesToDestroyDict[pieceToDestroy.name]++;
                else piecesToDestroyDict[pieceToDestroy.name] = 1;
            }

            this.PieceStorage.DestroySpecifiedPieces(piecesToDestroyDict);

            // blocking the cooker for "cooking duration"

            Inventory.SetLayout(newLayout: Inventory.LayoutType.Toolbar, player: this.world.Player);
            this.TurnOn();
            new TextWindow(text: "Cooking...", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

            this.cookingStartFrame = this.world.CurrentUpdate;
            this.cookingDoneFrame = this.world.CurrentUpdate + cookingTime;
            this.boardTask = Scheduler.TaskName.ShowCookingProgress;
            this.showStatBarsTillFrame = this.world.CurrentUpdate + cookingTime;

            new WorldEvent(eventName: WorldEvent.EventName.FinishCooking, world: this.world, delay: cookingTime, boardPiece: this);

            this.world.HintEngine.Disable(PieceHint.Type.Cooker);
            this.world.HintEngine.Disable(Tutorials.Type.Cook);
        }

        public void ShowCookingProgress()
        {
            new TextWindow(text: $"Cooking will be done in {TimeSpanToString(this.TimeToFinishCooking)}.", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);
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