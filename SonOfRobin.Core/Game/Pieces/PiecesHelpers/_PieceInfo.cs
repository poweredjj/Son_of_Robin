using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class PieceInfo
    {
        private static readonly Dictionary<PieceTemplate.Name, Info> info = new Dictionary<PieceTemplate.Name, Info>();
        public static readonly Dictionary<Type, List<PieceTemplate.Name>> namesForType = new Dictionary<Type, List<PieceTemplate.Name>>();
        public static bool HasBeenInitialized { get; private set; } = false;

        public class Info
        {
            public readonly PieceTemplate.Name name;
            public readonly string readableName;
            public readonly string description;
            public readonly AllowedTerrain allowedTerrain;
            public readonly int stackSize;
            public readonly Type type;
            public readonly bool blocksMovement;
            public readonly bool convertsWhenUsed;
            public readonly PieceTemplate.Name convertsToWhenUsed;
            public readonly bool shootsProjectile;
            public readonly Equipment.EquipType equipType;
            public bool isCarnivorous;
            public readonly List<Buff> buffList;
            public readonly AnimFrame frame;
            public readonly Texture2D texture;
            public readonly Scheduler.TaskName toolbarTask;
            public readonly Scheduler.TaskName boardTask;
            public List<PieceTemplate.Name> eats;
            public List<PieceTemplate.Name> isEatenBy;
            public List<PieceTemplate.Name> combinesWith;
            public readonly bool hasFruit;
            public readonly float massTakenMultiplier;
            public readonly float maxHitPoints;
            public readonly int toolRange;
            public readonly float startHitPoints;
            public readonly BoardPiece.State initialActiveState;
            public readonly int strength;
            public readonly bool canBeHit;
            public readonly float speed;
            public readonly Color color;
            public readonly float opacity;
            public readonly LightEngine lightEngine;
            public readonly string animName;
            public readonly byte animSize;
            public readonly PieceTemplate.Name fruitName;
            public PieceTemplate.Name isSpawnedBy;
            public Dictionary<BoardPiece.Category, float> strengthMultiplierByCategory;
            public readonly float cookerFoodMassMultiplier;

            // data not present in BoardPiece (set directly in PieceInfo)
            public readonly bool serialize;

            public readonly BoardPiece.Category category;
            public readonly float startingMass;
            public readonly float fireAffinity;
            public readonly int[] maxMassForSize;
            public readonly bool movesWhenDropped;
            public readonly float plantAdultSizeMass; // adultSizeMass should be greater than animSize for sapling (to avoid showing fruits on sapling)
            public readonly float plantMassToBurn;
            public readonly PlantReproductionData plantReproductionData;
            public readonly Dictionary<Terrain.Name, byte> plantBestEnvironment;
            public readonly int plantMaxExistingNumber;
            public readonly int plantDropSeedChance;
            public readonly bool toolIndestructible;
            public readonly bool canShrink;
            public readonly bool canBePickedUp;

            public Info(BoardPiece piece)
            {
                this.animSize = piece.sprite.AnimSize;
                this.name = piece.name;
                this.type = piece.GetType();
                this.allowedTerrain = piece.sprite.allowedTerrain;
                this.maxHitPoints = piece.maxHitPoints;
                this.startHitPoints = piece.HitPoints;
                this.strength = piece.strength;
                this.speed = piece.speed;
                this.initialActiveState = piece.activeState;
                this.blocksMovement = piece.sprite.blocksMovement;
                this.readableName = piece.readableName;
                this.description = piece.description;
                this.stackSize = piece.stackSize;
                this.buffList = piece.buffList;
                this.toolbarTask = piece.toolbarTask;
                this.boardTask = piece.boardTask;
                this.canBeHit = piece.canBeHit;
                this.color = piece.sprite.color;
                this.opacity = piece.sprite.opacity;
                this.lightEngine = piece.sprite.lightEngine;
                this.animName = piece.sprite.AnimName;
                if (this.type == typeof(Animal)) this.eats = ((Animal)piece).Eats;
                this.equipType = this.type == typeof(Equipment) ? ((Equipment)piece).equipType : Equipment.EquipType.None;
                this.cookerFoodMassMultiplier = this.type == typeof(Cooker) ? ((Cooker)piece).foodMassMultiplier : 0f;

                this.convertsWhenUsed = false;
                if (this.type == typeof(Potion))
                {
                    this.convertsWhenUsed = true;
                    this.convertsToWhenUsed = ((Potion)piece).convertsToWhenUsed;
                }

                this.shootsProjectile = false;
                this.toolRange = 0;
                if (this.type == typeof(Tool))
                {
                    Tool tool = (Tool)piece;
                    this.shootsProjectile = tool.shootsProjectile;
                    this.strengthMultiplierByCategory = tool.multiplierByCategory;
                    this.toolRange = tool.range;
                }

                if (this.type == typeof(Shelter))
                {
                    Shelter shelter = (Shelter)piece;
                    SleepEngine sleepEngine = shelter.sleepEngine;
                    this.buffList.AddRange(sleepEngine.wakeUpBuffs);
                }

                if (this.type == typeof(Projectile))
                {
                    // "emulating" tool multiplier list
                    this.strengthMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, ((Projectile)piece).baseHitPower / 4 } };
                }

                this.isEatenBy = new List<PieceTemplate.Name> { };

                this.combinesWith = PieceCombiner.CombinesWith(this.name);

                this.hasFruit = false;
                this.massTakenMultiplier = 1;
                if (this.type == typeof(Plant))
                {
                    Plant plant = (Plant)piece;
                    if (plant.fruitEngine != null)
                    {
                        this.fruitName = plant.fruitEngine.fruitName;
                        this.hasFruit = true;
                    }
                    this.massTakenMultiplier = plant.massTakenMultiplier;
                }

                // setting default values params non-present in boardPiece

                this.category = BoardPiece.Category.NotSet;
                this.startingMass = 1;
                this.serialize = true;
                this.fireAffinity = 0f;
                this.maxMassForSize = null;
                this.movesWhenDropped = true;
                this.plantAdultSizeMass = 0f;
                this.plantMassToBurn = 0;
                this.plantReproductionData = null;
                this.plantBestEnvironment = null;
                this.plantMaxExistingNumber = 0;
                this.plantDropSeedChance = 0;
                this.toolIndestructible = false;
                this.canShrink = false;
                this.canBePickedUp = false;

                // setting values for names

                switch (this.name)
                {
                    case PieceTemplate.Name.Empty:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.PlayerBoy:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        this.fireAffinity = 0.5f;
                        break;

                    case PieceTemplate.Name.PlayerGirl:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        this.fireAffinity = 0.5f;
                        break;

                    case PieceTemplate.Name.PlayerTestDemoness:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        break;

                    case PieceTemplate.Name.PlayerGhost:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        break;

                    case PieceTemplate.Name.GrassRegular:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 100, 150 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        break;

                    case PieceTemplate.Name.GrassGlow:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 100, 150 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.plantMaxExistingNumber = 300;
                        break;

                    case PieceTemplate.Name.GrassDesert:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.8f;
                        this.maxMassForSize = new int[] { 250 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 40 } };
                        break;

                    case PieceTemplate.Name.PlantPoison:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 800 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1000, massLost: 190, bioWear: 0.34f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Biome, 245 } };
                        this.plantDropSeedChance = 70;
                        break;

                    case PieceTemplate.Name.Rushes:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.2f;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 220 }, { Terrain.Name.Height, 92 } };
                        break;

                    case PieceTemplate.Name.WaterLily:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 80 }, { Terrain.Name.Height, 45 } };
                        break;

                    case PieceTemplate.Name.FlowersPlain:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        this.plantDropSeedChance = 20;
                        break;

                    case PieceTemplate.Name.FlowersRed:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        this.plantDropSeedChance = 20;
                        break;

                    case PieceTemplate.Name.FlowersMountain:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 500 };
                        this.plantMassToBurn = 3;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Height, 175 } };
                        this.plantDropSeedChance = 15;
                        break;

                    case PieceTemplate.Name.TomatoPlant:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.4f;
                        this.maxMassForSize = new int[] { 450, 900 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        break;

                    case PieceTemplate.Name.CoffeeShrub:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.4f;
                        this.maxMassForSize = new int[] { 600 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        break;

                    case PieceTemplate.Name.Cactus:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 10000 };
                        this.plantMassToBurn = 10;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 60 } };
                        this.plantDropSeedChance = 50;
                        break;

                    case PieceTemplate.Name.TreeSmall:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 15;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.3f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        break;

                    case PieceTemplate.Name.TreeBig:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        break;

                    case PieceTemplate.Name.Oak:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        this.plantDropSeedChance = 20;
                        break;

                    case PieceTemplate.Name.AppleTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        break;

                    case PieceTemplate.Name.CherryTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        break;

                    case PieceTemplate.Name.PalmTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 2500, 8000, 10000 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 12;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        break;

                    case PieceTemplate.Name.BananaTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 2500, 8000, 10000 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 12;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        break;

                    case PieceTemplate.Name.CarrotPlant:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.4f;
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.plantDropSeedChance = 8;
                        break;

                    case PieceTemplate.Name.Acorn:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.SeedsGeneric:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 5;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.CoffeeRaw:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 5;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.CoffeeRoasted:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Apple:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 60;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Banana:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 80;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Cherry:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Tomato:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.15f;
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Carrot:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.MeatRawRegular:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.MeatRawPrime:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 250;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.MeatDried:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 250;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Fat:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Leather:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.fireAffinity = 0.7f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Burger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 560;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Meal:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Rabbit:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 35;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 200, 500 };
                        break;

                    case PieceTemplate.Name.Fox:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 60;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 500, 1000 };
                        break;

                    case PieceTemplate.Name.Tiger:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 80;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 500, 2000 };
                        break;

                    case PieceTemplate.Name.Frog:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 10;
                        this.fireAffinity = 0.15f;
                        this.maxMassForSize = new int[] { 300, 800 };
                        break;

                    case PieceTemplate.Name.MineralsBig:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.MineralsSmall:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.MineralsMossyBig:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.MineralsMossySmall:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.JarTreasure:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.4f;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.JarBroken:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.5f;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.CrateStarting:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.7f;
                        break;

                    case PieceTemplate.Name.CrateRegular:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.7f;
                        break;

                    case PieceTemplate.Name.ChestWooden:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.ChestStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.2f;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.ChestIron:
                        this.category = BoardPiece.Category.Metal;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.ChestCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.ChestTreasureNormal:
                        this.category = BoardPiece.Category.Metal;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.ChestTreasureBig:
                        this.category = BoardPiece.Category.Metal;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.Campfire:
                        this.category = BoardPiece.Category.Stone;
                        break;

                    case PieceTemplate.Name.WorkshopEssential:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.WorkshopBasic:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.WorkshopAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.WorkshopMaster:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.WorkshopLeatherBasic:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.WorkshopLeatherAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.AlchemyLabStandard:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        break;

                    case PieceTemplate.Name.AlchemyLabAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        break;

                    case PieceTemplate.Name.Furnace:
                        this.category = BoardPiece.Category.Stone;
                        break;

                    case PieceTemplate.Name.Anvil:
                        this.category = BoardPiece.Category.Metal;
                        break;

                    case PieceTemplate.Name.HotPlate:
                        this.category = BoardPiece.Category.Stone;
                        break;

                    case PieceTemplate.Name.CookingPot:
                        this.category = BoardPiece.Category.Metal;
                        break;

                    case PieceTemplate.Name.UpgradeBench:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        break;

                    case PieceTemplate.Name.Stick:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.WoodLogRegular:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.WoodLogHard:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.9f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.WoodPlank:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Stone:
                        this.category = BoardPiece.Category.Stone;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Granite:
                        this.category = BoardPiece.Category.Stone;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Clay:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Rope:
                        this.category = BoardPiece.Category.Flesh;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Clam:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.CoalDeposit:
                        this.category = BoardPiece.Category.Stone;
                        break;

                    case PieceTemplate.Name.IronDeposit:
                        this.category = BoardPiece.Category.Stone;
                        break;

                    case PieceTemplate.Name.BeachDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        break;

                    case PieceTemplate.Name.ForestDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        break;

                    case PieceTemplate.Name.DesertDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        break;

                    case PieceTemplate.Name.GlassDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        break;

                    case PieceTemplate.Name.SwampDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        break;

                    case PieceTemplate.Name.CrystalDepositSmall:
                        this.category = BoardPiece.Category.Crystal;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.CrystalDepositBig:
                        this.category = BoardPiece.Category.Crystal;
                        break;

                    case PieceTemplate.Name.Coal:
                        this.category = BoardPiece.Category.Stone;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.IronOre:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.IronBar:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.IronRod:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.IronNail:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.IronPlate:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.GlassSand:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Crystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Backlight:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.BloodSplatter:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.Attack:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.Miss:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.Zzz:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.Heart:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.MapMarker:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.MusicNote:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.Crosshair:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.BubbleExclamationRed:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.BubbleExclamationBlue:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.BubbleCraftGreen:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.RainDrop:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.Explosion:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.BurningFlame:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 60;
                        this.maxMassForSize = new int[] { 100, 250, 500, 750, 1000, 2000, 2500 };
                        this.canShrink = true;
                        break;

                    case PieceTemplate.Name.CookingTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.UpgradeTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BrewTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.FireplaceTriggerOn:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.FireplaceTriggerOff:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.KnifeSimple:
                        this.category = BoardPiece.Category.Wood;
                        this.toolIndestructible = true;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.AxeWood:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.AxeStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.AxeIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.AxeCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.PickaxeWood:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.PickaxeStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.PickaxeIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.PickaxeCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.SpearWood:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.SpearStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.SpearIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.SpearCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ScytheStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ScytheIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ScytheCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ShovelStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ShovelIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ShovelCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BowBasic:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BowAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ArrowWood:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ArrowStone:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ArrowIron:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.7f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ArrowCrystal:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ArrowBurning:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.DebrisPlant:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisStone:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisWood:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisLeaf:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisCrystal:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisCeramic:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisStar:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisSoot:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.DebrisHeart:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.BloodDrop:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.TentSmall:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        break;

                    case PieceTemplate.Name.TentMedium:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.7f;
                        break;

                    case PieceTemplate.Name.TentBig:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.6f;
                        break;

                    case PieceTemplate.Name.BackpackSmall:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 200;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BackpackMedium:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 500;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BackpackBig:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 700;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BeltSmall:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 200;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BeltMedium:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 300;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BeltBig:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 400;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Map:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 100;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Dungarees:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 150;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HatSimple:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 100;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BootsProtective:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.TorchSmall:
                        this.category = BoardPiece.Category.Wood;
                        this.startingMass = 100;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.TorchBig:
                        this.category = BoardPiece.Category.Wood;
                        this.startingMass = 180;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.LanternEmpty:
                        this.category = BoardPiece.Category.Metal;
                        this.startingMass = 300;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.LanternFull:
                        this.category = BoardPiece.Category.Metal;
                        this.startingMass = 350;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Candle:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HumanSkeleton:
                        this.category = BoardPiece.Category.Flesh;
                        this.fireAffinity = 0.3f;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.PredatorRepellant:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.fireAffinity = 0.0f;
                        break;

                    case PieceTemplate.Name.HerbsBlack:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HerbsBlue:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HerbsCyan:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HerbsGreen:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HerbsYellow:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HerbsRed:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.HerbsViolet:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.EmptyBottle:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.PotionGeneric:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.PotionCoffee:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.BottleOfOil:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.Hole:
                        this.category = BoardPiece.Category.Indestructible;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.TreeStump:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.movesWhenDropped = false;
                        break;

                    case PieceTemplate.Name.LavaFlame:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SwampGas:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.fireAffinity = 1.0f;
                        break;

                    case PieceTemplate.Name.LavaGas:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.fireAffinity = 1.0f;
                        break;

                    case PieceTemplate.Name.SoundSeaWavesObsolete:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SoundLakeWaves:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SoundSeaWind:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SoundNightCrickets:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SoundNoonCicadas:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SoundLava:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.SeaWave:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    case PieceTemplate.Name.ParticleEmitter:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported name - {this.name}.");
                }

                // setting some variables, that need params non-present in boardPiece

                if (this.maxMassForSize != null) piece.sprite.AssignNewSize((byte)(this.maxMassForSize.Length - 1));
                this.frame = piece.sprite.AnimFrame;
                this.texture = this.frame.texture;

                // checking for params, that need to be set

                if (this.category == BoardPiece.Category.NotSet) throw new ArgumentNullException($"{this.name} - category not set.");
                if (this.startingMass <= 0) throw new ArgumentNullException($"{this.name} - starting mass incorrect value.");

                if (this.type == typeof(Plant))
                {
                    if (this.plantMassToBurn == 0) throw new ArgumentNullException($"{this.name} - plantMassToBurn not set.");
                    if (this.plantReproductionData == null) throw new ArgumentNullException($"{this.name} - plantReproductionData not set.");
                    if (this.plantBestEnvironment == null) throw new ArgumentNullException($"{this.name} - plantBestEnvironment not set.");
                }
            }

            public List<string> BuffDescList
            {
                get
                {
                    var buffDescList = new List<string> { };
                    if (buffList == null) return buffDescList;

                    foreach (var buff in buffList)
                    {
                        buffDescList.Add(buff.description);
                    }
                    return buffDescList;
                }
            }
        }

        public static List<Info> AllInfo
        { get { return info.Values.ToList(); } }

        public static Info GetInfo(PieceTemplate.Name pieceName)
        {
            return info[pieceName];
        }

        public static Info TryToGetInfo(PieceTemplate.Name pieceName)
        {
            return info.ContainsKey(pieceName) ? info[pieceName] : null;
        }

        public static Texture2D GetTexture(PieceTemplate.Name pieceName)
        {
            // to simplify frequently used query

            return info[pieceName].texture;
        }

        public static void CreateAllInfo()
        {
            // creates one instance of every piece type - to get required info out of it

            foreach (PieceTemplate.Name name in PieceTemplate.allNames)
            {
                BoardPiece piece = PieceTemplate.Create(templateName: name, world: null);
                info[name] = new Info(piece: piece);
            }

            // getting isEatenBy data

            foreach (Info potentialPrey in info.Values)
            {
                if (potentialPrey.eats != null)
                {
                    // animal will either hunt the player or run away
                    if (!ContainsPlayer(potentialPrey.eats)) potentialPrey.isEatenBy.AddRange(PieceInfo.GetPlayerNames());

                    foreach (Info potentialPredator in info.Values)
                    {
                        if (potentialPredator.eats != null && potentialPredator.eats.Contains(potentialPrey.name))
                        {
                            potentialPrey.isEatenBy.Add(potentialPredator.name);
                            if (potentialPrey.type == typeof(Animal)) potentialPredator.isCarnivorous = true;
                        }
                    }
                }
            }

            // getting fruitSpawner data

            foreach (Info fruitSpawnerInfo in info.Values.Where(info => info.hasFruit))
            {
                info[fruitSpawnerInfo.fruitName].isSpawnedBy = fruitSpawnerInfo.name;
            }

            // getting names for type data

            foreach (Info currentInfo in info.Values)
            {
                if (!namesForType.ContainsKey(currentInfo.type)) namesForType[currentInfo.type] = new List<PieceTemplate.Name>();
                namesForType[currentInfo.type].Add(currentInfo.name);
            }

            HasBeenInitialized = true;
        }

        public static List<PieceTemplate.Name> GetIsEatenBy(PieceTemplate.Name name)
        {
            if (info.ContainsKey(name) && info[name].isEatenBy != null) return info[name].isEatenBy;

            return new List<PieceTemplate.Name> { };
        }

        public static bool IsPlayer(PieceTemplate.Name pieceName)
        {
            return info[pieceName].type == typeof(Player) && pieceName != PieceTemplate.Name.PlayerGhost;
        }

        public static bool ContainsPlayer(List<PieceTemplate.Name> pieceNameList)
        {
            foreach (PieceTemplate.Name pieceName in pieceNameList)
            {
                if (IsPlayer(pieceName)) return true;
            }
            return false;
        }

        public static List<PieceTemplate.Name> GetPlayerNames()
        {
            var playerNameList = new List<PieceTemplate.Name>();
            foreach (Info currentInfo in AllInfo)
            {
                if (IsPlayer(currentInfo.name)) playerNameList.Add(currentInfo.name);
            }

            return playerNameList;
        }

        public static List<PieceTemplate.Name> GetNamesForEquipType(Equipment.EquipType equipType)
        {
            var equipNameList = new List<PieceTemplate.Name>();
            foreach (Info currentInfo in AllInfo)
            {
                if (currentInfo.equipType == equipType) equipNameList.Add(currentInfo.name);
            }

            return equipNameList;
        }

        public static List<InfoWindow.TextEntry> GetCategoryAffinityTextEntryList(PieceTemplate.Name pieceName, float scale = 1f)
        {
            var entryList = new List<InfoWindow.TextEntry>();

            var multiplierByCategory = info[pieceName].strengthMultiplierByCategory;
            if (multiplierByCategory == null) return entryList;

            string text = "|     ";
            var imageList = new List<Texture2D> { AnimData.framesForPkgs[AnimData.PkgName.Biceps].texture };

            foreach (BoardPiece.Category category in BoardPiece.allCategories) // allCategories is used to keep the same order for every tool
            {
                if (multiplierByCategory.ContainsKey(category) && multiplierByCategory[category] > 0)
                {
                    float multiplier = multiplierByCategory[category];

                    text += $"| {multiplier}   ";
                    imageList.Add(BoardPiece.GetTextureForCategory(category));
                }
            }

            entryList.Add(new InfoWindow.TextEntry(text: text, scale: scale, imageList: imageList, color: Color.White));

            return entryList;
        }

        public static List<InfoWindow.TextEntry> GetCombinesWithTextEntryList(PieceTemplate.Name pieceName, float scale = 1f)
        {
            var entryList = new List<InfoWindow.TextEntry>();

            var combinesWith = info[pieceName].combinesWith;
            if (!combinesWith.Any()) return entryList;

            string text = "Combines with: ";
            var imageList = new List<Texture2D>();

            foreach (PieceTemplate.Name combineName in combinesWith)
            {
                text += "| ";
                imageList.Add(GetTexture(combineName));
            }

            entryList.Add(new InfoWindow.TextEntry(text: text, scale: scale, imageList: imageList, color: Color.White));

            return entryList;
        }
    }
}