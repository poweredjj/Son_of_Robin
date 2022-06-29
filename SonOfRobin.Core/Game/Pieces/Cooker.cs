using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class Cooker : BoardPiece
    {
        private static readonly List<PieceTemplate.Name> ingredientNames = new List<PieceTemplate.Name> { PieceTemplate.Name.RawMeat, PieceTemplate.Name.Apple, PieceTemplate.Name.Cherry, PieceTemplate.Name.Banana, PieceTemplate.Name.Tomato, PieceTemplate.Name.Acorn, PieceTemplate.Name.Herbs };

        private static readonly List<PieceTemplate.Name> fuelNames = new List<PieceTemplate.Name> { PieceTemplate.Name.WoodLog, PieceTemplate.Name.WoodPlank, PieceTemplate.Name.Coal };

        private readonly float foodMassMultiplier;
        private int cookingStartFrame;
        private int cookingDoneFrame;

        private TimeSpan TimeToFinishCooking
        { get { return TimeSpan.FromSeconds((int)(Math.Ceiling((float)(this.cookingDoneFrame - (float)this.world.currentUpdate) / 60f))); } }

        public Cooker(World world, Vector2 position, AnimPkg animPackage, PieceTemplate.Name name, AllowedFields allowedFields, Dictionary<byte, int> maxMassBySize, byte storageWidth, byte storageHeight, float foodMassMultiplier, string readableName, string description, Category category,
            byte animSize = 0, string animName = "off", bool blocksMovement = true, ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = false, int generation = 0, Yield yield = null, int maxHitPoints = 1, bool fadeInAnim = false) :

            base(world: world, position: position, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: blocksMovement, minDistance: minDistance, maxDistance: maxDistance, name: name, destructionDelay: destructionDelay, allowedFields: allowedFields, floatsOnWater: floatsOnWater, maxMassBySize: maxMassBySize, generation: generation, canBePickedUp: false, yield: yield, maxHitPoints: maxHitPoints, fadeInAnim: fadeInAnim, isShownOnMiniMap: true, readableName: readableName, description: description, category: category, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true))
        {
            this.activeState = State.Empty;
            this.boardTask = Scheduler.TaskName.OpenContainer;
            this.cookingDoneFrame = 0;
            this.foodMassMultiplier = foodMassMultiplier;
            this.pieceStorage = new PieceStorage(width: storageWidth, height: storageHeight, world: this.world, storagePiece: this, storageType: PieceStorage.StorageType.Cooking);

            var allowedPieceNames = new List<PieceTemplate.Name>(ingredientNames);
            allowedPieceNames.AddRange(fuelNames);
            allowedPieceNames.Add(PieceTemplate.Name.CookingTrigger);
            allowedPieceNames.Add(PieceTemplate.Name.Meal);

            this.pieceStorage.AssignAllowedPieceNames(allowedPieceNames);

            BoardPiece flameTrigger = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.CookingTrigger, world: this.world);
            StorageSlot flameSlot = this.pieceStorage.FindCorrectSlot(flameTrigger);
            this.pieceStorage.AddPiece(flameTrigger);
            flameSlot.locked = true;
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

                new StatBar(label: "", value: cookingCurrentFrame, valueMax: cookingDuration, colorMin: new Color(255, 0, 0), colorMax: new Color(255, 128, 0), posX: this.sprite.gfxRect.Center.X, posY: this.sprite.gfxRect.Bottom, ignoreIfAtMax: false);
            }

            base.DrawStatBar();
            StatBar.FinishThisBatch();
        }

        public void Cook()
        {
            var storedIngredients = this.pieceStorage.GetAllPieces().Where(piece => ingredientNames.Contains(piece.name)).ToList();
            var storedFuel = this.pieceStorage.GetAllPieces().Where(piece => fuelNames.Contains(piece.name)).ToList();

            if (storedIngredients.Count == 0 && storedFuel.Count == 0)
            {
                new TextWindow(text: "I need ingredients and fuel (wood or coal) to cook.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            if (storedFuel.Count == 0)
            {
                new TextWindow(text: "I don't have wood or coal.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            if (storedIngredients.Count == 0)
            {
                new TextWindow(text: "I have fuel, but no ingredients.", textColor: Color.Black, bgColor: Color.White, useTransition: false, animate: true);
                return;
            }

            float fuelPerPiece = 500f; // food mass cooked with one fuel piece (regardless of type)
            int singleMealMass = 250;

            float cookedMass = 0f;
            float fuelLeft = 0;
            var piecesToDestroy = new Dictionary<PieceTemplate.Name, byte> { };

            foreach (BoardPiece ingredient in storedIngredients)
            {
                if (fuelLeft == 0)
                {
                    if (storedFuel.Count > 0)
                    {
                        BoardPiece fuel = storedFuel[0];
                        storedFuel.RemoveAt(0);
                        fuelLeft += fuelPerPiece;

                        if (piecesToDestroy.ContainsKey(fuel.name)) piecesToDestroy[fuel.name]++;
                        else piecesToDestroy[fuel.name] = 1;
                    }
                }

                if (fuelLeft > 0)
                {
                    fuelLeft = Math.Max(fuelLeft - ingredient.Mass, 0);
                    cookedMass += ingredient.Mass;

                    if (piecesToDestroy.ContainsKey(ingredient.name)) piecesToDestroy[ingredient.name]++;
                    else piecesToDestroy[ingredient.name] = 1;
                }
                else
                { break; }
            }

            int cookingTime = (int)(cookedMass * 2);
            cookedMass *= this.foodMassMultiplier;

            this.pieceStorage.DestroySpecifiedPieces(piecesToDestroy);

            while (true)
            {
                BoardPiece meal = PieceTemplate.CreateOffBoard(templateName: PieceTemplate.Name.Meal, world: this.world);
                meal.Mass = Math.Min(cookedMass, singleMealMass);
                if (meal.Mass == singleMealMass)
                {
                    meal.buffList.Add(new BuffEngine.Buff(world: this.world, type: BuffEngine.BuffType.MaxHp, value: (float)20, autoRemoveDelay: 3 * 60 * 60, isPositive: true));
                    if (this.world.random.Next(0, 5) == 0) meal.buffList.Add(new BuffEngine.Buff(world: this.world, type: BuffEngine.BuffType.Strength, value: 1, autoRemoveDelay: 3 * 60 * 60, isPositive: true));
                }
                this.pieceStorage.AddPiece(piece: meal, dropIfDoesNotFit: true);
                cookedMass = Math.Max(cookedMass - meal.Mass, 0);

                if (cookedMass == 0) break;
            }

            Scene.SetInventoryLayout(newLayout: Scene.InventoryLayout.Toolbar, player: this.world.player);
            this.TurnOn();
            new TextWindow(text: "Cooking...", textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: false);

            this.cookingStartFrame = this.world.currentUpdate;
            this.cookingDoneFrame = this.world.currentUpdate + cookingTime;
            this.boardTask = Scheduler.TaskName.ShowCookingProgress;
            this.showStatBarsTillFrame = this.world.currentUpdate + cookingTime;

            new WorldEvent(eventName: WorldEvent.EventName.FinishCooking, world: this.world, delay: cookingTime, boardPiece: this);

            this.world.hintEngine.Disable(PieceHint.Type.Cooker);
        }

        public void ShowCookingProgress()
        {
            new TextWindow(text: $"Cooking will be done in {TimeSpanToString(this.TimeToFinishCooking)}.", textColor: Color.White, bgColor: Color.Green, useTransition: true, animate: false);
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