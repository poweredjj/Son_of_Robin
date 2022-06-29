using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Cooker : BoardPiece
    {
        public static readonly List<PieceTemplate.Name> ingredientNames = new List<PieceTemplate.Name> { PieceTemplate.Name.RawMeat, PieceTemplate.Name.Apple, PieceTemplate.Name.Cherry, PieceTemplate.Name.Banana, PieceTemplate.Name.Tomato, PieceTemplate.Name.Acorn, PieceTemplate.Name.Clam, PieceTemplate.Name.Fat };

        private static readonly List<PieceTemplate.Name> boosterNames = new List<PieceTemplate.Name> { PieceTemplate.Name.HerbsRed, PieceTemplate.Name.HerbsYellow, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsBlue, PieceTemplate.Name.HerbsBlack, PieceTemplate.Name.HerbsCyan, PieceTemplate.Name.HerbsViolet, PieceTemplate.Name.HerbsGreen };

        private static readonly List<PieceTemplate.Name> fuelNames = new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLog, PieceTemplate.Name.WoodPlank };

        private readonly int ingredientSpace;
        private readonly int boosterSpace;

        private readonly float foodMassMultiplier;
        private int cookingStartFrame;
        private int cookingDoneFrame;

        private readonly StorageSlot mealSlot;

        private TimeSpan TimeToFinishCooking
        { get { return TimeSpan.FromSeconds((int)(Math.Ceiling((float)(this.cookingDoneFrame - (float)this.world.currentUpdate) / 60f))); } }

        public Cooker(World world, Vector2 position, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, float foodMassMultiplier, string readableName, string description, Category category, int ingredientSpace, int boosterSpace,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true))
        {
            this.activeState = State.Empty;
            this.boardTask = Scheduler.TaskName.OpenContainer;
            this.cookingDoneFrame = 0;
            this.foodMassMultiplier = foodMassMultiplier;
            this.ingredientSpace = ingredientSpace;
            this.boosterSpace = boosterSpace;

            this.pieceStorage = this.CreateAndConfigureStorage();
            this.mealSlot = this.pieceStorage.GetSlot(1, 0);
        }

        private PieceStorage CreateAndConfigureStorage()
        {
            byte storageWidth = (byte)Math.Max(this.ingredientSpace, this.boosterSpace);
            storageWidth = Math.Max(storageWidth, (byte)3);
            byte storageHeight = 3;

            var storage = new PieceStorage(width: storageWidth, height: storageHeight, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Cooking, stackLimit: 1);

            foreach (StorageSlot slot in storage.AllSlots)
            {
                slot.locked = true;
                slot.hidden = true;
            }

            StorageSlot flameTriggerSlot = storage.GetSlot(0, 0);
            flameTriggerSlot.locked = false;
            flameTriggerSlot.hidden = false;
            flameTriggerSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.CookingTrigger };
            BoardPiece flameTrigger = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.CookingTrigger, world: this.world);
            flameTriggerSlot.AddPiece(flameTrigger);
            flameTriggerSlot.locked = true;

            StorageSlot mealSlot = storage.GetSlot(1, 0);
            mealSlot.locked = false;
            mealSlot.hidden = false;
            mealSlot.allowedPieceNames = new List<PieceTemplate.Name> { PieceTemplate.Name.Meal };
            mealSlot.label = "meal";

            StorageSlot fuelSlot = storage.GetSlot(2, 0);
            fuelSlot.locked = false;
            fuelSlot.hidden = false;
            fuelSlot.allowedPieceNames = fuelNames;
            fuelSlot.label = "fuel";

            for (int x = 0; x < this.ingredientSpace; x++)
            {
                StorageSlot ingredientSlot = storage.GetSlot(x, 1);
                ingredientSlot.locked = false;
                ingredientSlot.hidden = false;
                ingredientSlot.label = "ingredient";
                ingredientSlot.allowedPieceNames = ingredientNames;
            }

            for (int x = 0; x < this.boosterSpace; x++)
            {
                StorageSlot boosterSlot = storage.GetSlot(x, 2);
                boosterSlot.locked = false;
                boosterSlot.hidden = false;
                boosterSlot.label = "booster";
                boosterSlot.allowedPieceNames = boosterNames;
            }

            return storage;
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
            this.cookingStartFrame = (int)pieceData["cooker_cookingStartFrame"];
            this.cookingDoneFrame = (int)pieceData["cooker_cookingDoneFrame"];
        }

        public void TurnOn()
        {
            this.sprite.AssignNewName(animName: "on");
            this.sprite.lightEngine.Activate();
        }

        public void TurnOff()
        {
            this.sprite.AssignNewName(animName: "off");
            this.sprite.lightEngine.Deactivate();
        }

        public override void DrawStatBar()
        {
            if (this.world.currentUpdate < this.cookingDoneFrame)
            {
                int cookingDuration = this.cookingDoneFrame - this.cookingStartFrame;
                int cookingCurrentFrame = this.world.currentUpdate - this.cookingStartFrame;

                new StatBar(label: "", value: cookingCurrentFrame, valueMax: cookingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false, texture: AnimData.framesForPkgs[AnimData.PkgName.Flame].texture);
            }

            base.DrawStatBar();
            StatBar.FinishThisBatch();
        }

        public void Cook()
        {
            // checking stored pieces

            if (!this.mealSlot.IsEmpty)
            {
                BoardPiece mealLeftInside = this.mealSlot.TopPiece;

                new TextWindow(text: "I have to take out previously cooked | meal first.", imageList: new List<Texture2D> { mealLeftInside.sprite.frame.texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            var storedIngredients = this.pieceStorage.GetAllPieces().Where(piece => ingredientNames.Contains(piece.name)).ToList();
            var storedBoosters = this.pieceStorage.GetAllPieces().Where(piece => boosterNames.Contains(piece.name)).ToList();
            var storedFuel = this.pieceStorage.GetAllPieces().Where(piece => fuelNames.Contains(piece.name)).ToList();

            if (storedIngredients.Count == 0 && storedFuel.Count == 0)
            {
                new TextWindow(text: "I need at least one | | | ingredient and | fuel to cook.", imageList: new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.RawMeat].texture, AnimData.framesForPkgs[AnimData.PkgName.Tomato].texture, AnimData.framesForPkgs[AnimData.PkgName.Clam].texture, AnimData.framesForPkgs[AnimData.PkgName.WoodLog].texture }, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            if (storedFuel.Count == 0)
            {
                string fuelMarkers = "";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name fuel in fuelNames)
                {
                    fuelMarkers += "| ";
                    imageList.Add(PieceInfo.GetInfo(fuel).frame.texture);
                }

                new TextWindow(text: $"I don't have any {fuelMarkers} fuel.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            if (storedIngredients.Count == 0)
            {
                string ingredientMarkers = "";
                var imageList = new List<Texture2D>();

                foreach (PieceTemplate.Name ingredient in ingredientNames)
                {
                    ingredientMarkers += "| ";
                    imageList.Add(PieceInfo.GetInfo(ingredient).frame.texture);
                }

                new TextWindow(text: $"I don't have any {ingredientMarkers} ingredients.", imageList: imageList, textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            // calculating meal mass

            float cookedMass = 0;

            foreach (BoardPiece ingredient in storedIngredients)
            {
                cookedMass += ingredient.Mass;
            }
            int cookingTime = (int)(cookedMass * 3);
            cookedMass *= foodMassMultiplier;

            // getting all buffs from boosters

            var buffList = new List<BuffEngine.Buff>();

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

            BoardPiece meal = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Meal, world: this.world);
            buffList = BuffEngine.MergeSameTypeBuffsInList(world: this.world, buffList: buffList); // merging the same buffs (to add values of non-stackable buffs)
            meal.buffList = buffList;
            this.pieceStorage.AddPiece(piece: meal, dropIfDoesNotFit: true);

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

            this.pieceStorage.DestroySpecifiedPieces(piecesToDestroyDict);

            // blocking the cooker for "cooking duration"

            Inventory.SetLayout(newLayout: Inventory.Layout.Toolbar, player: this.world.player);
            this.TurnOn();
            new TextWindow(text: "Cooking...", textColor: Color.White, bgColor: Color.Green, useTransition: false, animate: true, checkForDuplicate: true, autoClose: true, inputType: Scene.InputTypes.None, blockInputDuration: 45, priority: 1);

            this.cookingStartFrame = this.world.currentUpdate;
            this.cookingDoneFrame = this.world.currentUpdate + cookingTime;
            this.boardTask = Scheduler.TaskName.ShowCookingProgress;
            this.showStatBarsTillFrame = this.world.currentUpdate + cookingTime;

            new WorldEvent(eventName: WorldEvent.EventName.FinishCooking, world: this.world, delay: cookingTime, boardPiece: this);

            this.world.hintEngine.Disable(PieceHint.Type.Cooker);
            this.world.hintEngine.Disable(Tutorials.Type.Cook);
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