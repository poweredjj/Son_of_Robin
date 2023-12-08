using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SonOfRobin
{
    public class PieceInfo
    {
        private static readonly Dictionary<PieceTemplate.Name, Info> info = new();
        public static readonly Dictionary<Type, List<PieceTemplate.Name>> namesForType = new();
        public static bool HasBeenInitialized { get; private set; } = false;

        public class Info
        {
            public readonly PieceTemplate.Name name;
            public readonly string readableName;
            public readonly string description;
            public readonly AllowedTerrain allowedTerrain;
            public readonly Type type;
            public readonly bool convertsWhenUsed;
            public readonly PieceTemplate.Name convertsToWhenUsed;
            public readonly Equipment.EquipType equipType;
            public bool isCarnivorous;
            public readonly List<Buff> buffList;
            public readonly AnimData.PkgName animPkgName;
            public AnimFrame CroppedFrame { get { return AnimData.GetCroppedFrameForPackage(this.animPkgName); } }
            public Texture2D Texture { get { return this.CroppedFrame.texture; } }
            public List<PieceTemplate.Name> eats;
            public List<PieceTemplate.Name> isEatenBy;
            public List<PieceTemplate.Name> combinesWith;
            public readonly bool hasFruit;
            public readonly float massTakenMultiplier;
            public readonly float maxHitPoints;
            public readonly float startHitPoints;
            public readonly BoardPiece.State initialActiveState;
            public readonly int strength;
            public readonly bool canBeHit;
            public readonly bool createdByPlayer;
            public readonly float speed;
            public readonly Color color;
            public readonly float opacity;
            public readonly LightEngine lightEngine;
            public readonly string animName;
            public readonly int animSize;
            public readonly PieceTemplate.Name fruitName;
            public PieceTemplate.Name isSpawnedBy;
            public readonly float cookerFoodMassMultiplier;
            public readonly bool getsPushedByWaves;
            public readonly bool containerStorageIsGlobal;

            // "template" data not present in BoardPiece (set directly in PieceInfo)
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
            public readonly bool canShrink;
            public readonly bool canBePickedUp;
            public readonly byte stackSize;
            public readonly int destructionDelay;
            public readonly Scheduler.TaskName toolbarTask;
            public readonly Scheduler.TaskName boardTask;
            public readonly bool blocksMovement;
            public readonly bool blocksPlantGrowth;
            public readonly int placeMinDistance;
            public readonly int placeMaxDistance;
            public readonly bool ignoresCollisions;
            public readonly bool floatsOnWater;
            public readonly bool isAffectedByWind;
            public readonly AllowedDensity allowedDensity;
            public readonly int staysAfterDeath;
            public Yield Yield { get; private set; }
            public readonly Yield appearDebris;
            public readonly Dictionary<ParticleEngine.Preset, Color> windParticlesDict;
            public readonly int animalMaxMass;
            public readonly float animalMassBurnedMultiplier;
            public readonly int animalAwareness;
            public readonly int animalMatureAge;
            public readonly int animalPregnancyDuration;
            public readonly int animalMaxChildren;
            public readonly float animalRetaliateChance; // 0 - 1, used only for animals that do not eat player
            public readonly int animalSightRange;
            public readonly PieceSoundPackTemplate pieceSoundPackTemplate;
            public readonly int ambsoundPlayDelay;
            public readonly int ambsoundPlayDelayMaxVariation;
            public readonly List<IslandClock.PartOfDay> ambsoundPartOfDayList;
            public readonly bool ambsoundPlayOnlyWhenIsSunny;
            public readonly bool ambsoundGeneratesWind;
            public readonly bool visFogExplodesWhenBurns;
            public readonly bool destroysPlantsWhenBuilt;
            public readonly bool hasFlatShadow;
            public readonly bool shadowNotDrawn;
            public readonly float fertileGroundSoilWealthMultiplier;
            public readonly int inOpacityFadeDuration;
            public readonly int toolRange;
            public readonly bool toolIndestructible;
            public readonly bool toolShootsProjectile;
            public readonly List<PieceTemplate.Name> toolCompatibleAmmo;
            public readonly int toolHitCooldown;
            public Dictionary<BoardPiece.Category, float> toolMultiplierByCategory;
            public readonly float projectileHitMultiplier;
            public readonly bool projectileCanBeStuck;
            public readonly bool projectileCanExplode;
            public readonly int delayAfterCreationMinutes;
            public readonly TextureBank.TextureName interactVirtButtonName;

            public bool CanHurtAnimals
            {
                get
                {
                    return
                        this.toolMultiplierByCategory != null &&
                        this.toolMultiplierByCategory.ContainsKey(BoardPiece.Category.Flesh) &&
                        this.toolMultiplierByCategory[BoardPiece.Category.Flesh] > 0f;
                }
            }

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
                this.readableName = piece.readableName;
                this.description = piece.description;
                this.buffList = piece.buffList;
                this.canBeHit = piece.canBeHit;
                this.createdByPlayer = piece.createdByPlayer;
                this.color = piece.sprite.color;
                this.opacity = piece.sprite.opacity;
                this.lightEngine = piece.sprite.lightEngine;
                this.animName = piece.sprite.AnimName;
                this.eats = this.type == typeof(Animal) ? ((Animal)piece).Eats : null;
                this.equipType = this.type == typeof(Equipment) ? ((Equipment)piece).equipType : Equipment.EquipType.None;
                this.cookerFoodMassMultiplier = this.type == typeof(Cooker) ? ((Cooker)piece).foodMassMultiplier : 0f;
                this.containerStorageIsGlobal = false;
                this.getsPushedByWaves = false;

                this.convertsToWhenUsed = PieceTemplate.Name.Empty;
                this.convertsWhenUsed = false;
                if (this.type == typeof(Potion))
                {
                    this.convertsWhenUsed = true;
                    this.convertsToWhenUsed = PieceTemplate.Name.EmptyBottle;
                }

                if (this.type == typeof(Shelter))
                {
                    Shelter shelter = (Shelter)piece;
                    SleepEngine sleepEngine = shelter.sleepEngine;
                    this.buffList.AddRange(sleepEngine.wakeUpBuffs);
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
                this.toolShootsProjectile = false;
                this.toolCompatibleAmmo = null;
                this.toolHitCooldown = 30;
                this.toolRange = 0;
                this.canShrink = false;
                this.canBePickedUp = false;
                this.stackSize = 1;
                this.destructionDelay = 0;
                this.boardTask = Scheduler.TaskName.Empty;
                this.toolbarTask = Scheduler.TaskName.Empty;
                this.blocksMovement = false;
                this.blocksPlantGrowth = false;
                this.placeMinDistance = 0;
                this.placeMaxDistance = 100;
                this.ignoresCollisions = false;
                this.floatsOnWater = false;
                this.isAffectedByWind = false;
                this.allowedDensity = null;
                this.staysAfterDeath = this.type == typeof(Animal) ? 30 * 60 : 0;
                this.Yield = null;
                this.appearDebris = null; // yield that is used to make debris when placing this piece
                this.windParticlesDict = new Dictionary<ParticleEngine.Preset, Color>();
                this.animalMaxMass = 0;
                this.animalMassBurnedMultiplier = 0;
                this.animalAwareness = 0;
                this.animalMatureAge = 0;
                this.animalPregnancyDuration = 0;
                this.animalMaxChildren = 0;
                this.animalRetaliateChance = -1;
                this.animalSightRange = 0;
                this.ambsoundPlayDelay = -1;
                this.ambsoundPlayDelayMaxVariation = 0;
                this.ambsoundPartOfDayList = null;
                this.ambsoundPlayOnlyWhenIsSunny = false;
                this.ambsoundGeneratesWind = false;
                this.visFogExplodesWhenBurns = false;
                this.destroysPlantsWhenBuilt = false;
                this.hasFlatShadow = true;
                this.shadowNotDrawn = false;
                this.fertileGroundSoilWealthMultiplier = 0;
                this.inOpacityFadeDuration = 0;
                this.toolMultiplierByCategory = null;
                this.projectileHitMultiplier = 0f;
                this.projectileCanBeStuck = false;
                this.projectileCanExplode = false;
                this.delayAfterCreationMinutes = 10;
                this.interactVirtButtonName = TextureBank.TextureName.Empty;

                var customSoundsForActions = new Dictionary<PieceSoundPackTemplate.Action, Sound>();

                // setting values for names

                switch (this.name)
                {
                    case PieceTemplate.Name.Empty:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.ignoresCollisions = true;
                        break;

                    case PieceTemplate.Name.PlayerBoy:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        this.fireAffinity = 0.5f;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 65535;
                        this.getsPushedByWaves = true;
                        this.Yield = CreatePlayerYield();
                        AddPlayerCommonSounds(customSoundsForActions: customSoundsForActions, female: false);
                        break;

                    case PieceTemplate.Name.PlayerGirl:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        this.fireAffinity = 0.5f;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 65535;
                        this.getsPushedByWaves = true;
                        this.Yield = CreatePlayerYield();
                        AddPlayerCommonSounds(customSoundsForActions: customSoundsForActions, female: true);
                        break;

                    case PieceTemplate.Name.PlayerTestDemoness:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 65535;
                        this.getsPushedByWaves = true;
                        this.Yield = CreatePlayerYield();
                        AddPlayerCommonSounds(customSoundsForActions: customSoundsForActions, female: true);
                        break;

                    case PieceTemplate.Name.PlayerGhost:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 50000;
                        this.placeMaxDistance = 65535;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;

                        foreach (PieceSoundPackTemplate.Action action in new List<PieceSoundPackTemplate.Action> { PieceSoundPackTemplate.Action.StepGrass, PieceSoundPackTemplate.Action.StepWater, PieceSoundPackTemplate.Action.StepSand, PieceSoundPackTemplate.Action.StepRock, PieceSoundPackTemplate.Action.SwimShallow, PieceSoundPackTemplate.Action.SwimDeep, PieceSoundPackTemplate.Action.StepLava, PieceSoundPackTemplate.Action.StepMud })
                        {
                            customSoundsForActions[action] = new Sound(name: SoundData.Name.StepGhost, cooldown: 30, ignore3DAlways: true, volume: 0.8f, maxPitchVariation: 0.2f);
                        }

                        break;

                    case PieceTemplate.Name.GrassRegular:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 100, 150 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.blocksPlantGrowth = true;
                        this.placeMaxDistance = 80;
                        this.allowedDensity = new AllowedDensity(radius: 75, maxNoOfPiecesSameName: 8);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsGreen, chanceToDrop: 3, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.GrassGlow:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 100, 150 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.plantMaxExistingNumber = 500;
                        this.blocksPlantGrowth = true;
                        this.placeMaxDistance = 700;
                        this.allowedDensity = new AllowedDensity(radius: 500, maxNoOfPiecesSameName: 0);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsGreen, chanceToDrop: 100, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.GrassDesert:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.8f;
                        this.maxMassForSize = new int[] { 250 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 40 } };
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 60;
                        this.allowedDensity = new AllowedDensity(radius: 75, maxNoOfPiecesTotal: 0);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsRed, chanceToDrop: 3, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.PlantPoison:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 800 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1000, massLost: 190, bioWear: 0.34f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Biome, 245 } };
                        this.plantDropSeedChance = 70;
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 30;
                        this.placeMaxDistance = 90;
                        this.allowedDensity = new AllowedDensity(radius: 70, maxNoOfPiecesTotal: 4);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsBlack, chanceToDrop: 15, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsBrown, chanceToDrop: 8, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsDarkViolet, chanceToDrop: 8, maxNumberToDrop: 1),
                            }
                            );
                        break;

                    case PieceTemplate.Name.Rushes:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.2f;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 220 }, { Terrain.Name.Height, 92 } };
                        this.blocksPlantGrowth = true;
                        this.placeMaxDistance = 120;
                        this.allowedDensity = new AllowedDensity(radius: 120, maxNoOfPiecesTotal: 40);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsCyan, chanceToDrop: 1, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.WaterLily:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.plantMassToBurn = 5;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 80 }, { Terrain.Name.Height, 45 } };
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 25;
                        this.placeMaxDistance = 80;
                        this.floatsOnWater = true;
                        this.allowedDensity = new AllowedDensity(radius: 82, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsBlue, chanceToDrop: 10, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.FlowersPlain:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        this.plantDropSeedChance = 20;
                        this.blocksPlantGrowth = true;
                        this.allowedDensity = new AllowedDensity(radius: 100, maxNoOfPiecesSameName: 0);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindPetal] = Color.White;
                        this.delayAfterCreationMinutes = 30;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsYellow, chanceToDrop: 3, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.FlowersRed:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 400 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        this.plantDropSeedChance = 20;
                        this.blocksPlantGrowth = true;
                        this.allowedDensity = new AllowedDensity(radius: 100, maxNoOfPiecesSameName: 0);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindPetal] = new Color(237, 123, 214);
                        this.delayAfterCreationMinutes = 30;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsRed, chanceToDrop: 20, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.FlowersMountain:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 500 };
                        this.plantMassToBurn = 3;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Height, 175 } };
                        this.plantDropSeedChance = 15;
                        this.blocksPlantGrowth = true;
                        this.placeMaxDistance = 250;
                        this.allowedDensity = new AllowedDensity(radius: 240, maxNoOfPiecesSameName: 0);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindPetal] = new Color(237, 222, 12);
                        this.delayAfterCreationMinutes = 30;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsYellow, chanceToDrop: 40, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.TomatoPlant:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.4f;
                        this.maxMassForSize = new int[] { 450, 900 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 20;
                        this.placeMaxDistance = 200;
                        this.allowedDensity = new AllowedDensity(radius: 150, maxNoOfPiecesSameName: 2);
                        this.isAffectedByWind = true;
                        this.delayAfterCreationMinutes = 60;
                        this.hasFlatShadow = false;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });
                        break;

                    case PieceTemplate.Name.CoffeeShrub:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.4f;
                        this.maxMassForSize = new int[] { 600 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 20;
                        this.placeMaxDistance = 200;
                        this.allowedDensity = new AllowedDensity(radius: 150, maxNoOfPiecesSameName: 2);
                        this.isAffectedByWind = true;
                        this.delayAfterCreationMinutes = 60;
                        this.hasFlatShadow = false;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });
                        break;

                    case PieceTemplate.Name.CarrotPlant:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.4f;
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.plantDropSeedChance = 8;
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 20;
                        this.placeMaxDistance = 200;
                        this.allowedDensity = new AllowedDensity(radius: 150, maxNoOfPiecesSameName: 2);
                        this.isAffectedByWind = true;
                        this.hasFlatShadow = false;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });
                        break;

                    case PieceTemplate.Name.Cactus:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        this.maxMassForSize = new int[] { 10000 };
                        this.plantMassToBurn = 10;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 60 } };
                        this.plantDropSeedChance = 50;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 50;
                        this.placeMaxDistance = 600;
                        this.allowedDensity = new AllowedDensity(radius: 300, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;
                        this.delayAfterCreationMinutes = 30;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsViolet, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HerbsDarkGreen, chanceToDrop: 12, maxNumberToDrop: 1),
                            }
                            );
                        break;

                    case PieceTemplate.Name.MushroomPlant:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.fireAffinity = 0.6f;
                        this.maxMassForSize = new int[] { 450, 900 };
                        this.plantMassToBurn = 9;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 230 } };
                        this.plantDropSeedChance = 40;
                        this.blocksPlantGrowth = true;
                        this.placeMinDistance = 20;
                        this.placeMaxDistance = 200;
                        this.allowedDensity = new AllowedDensity(radius: 250, maxNoOfPiecesSameName: 6);
                        this.isAffectedByWind = true;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisGrass },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Mushroom, chanceToDrop: 10, maxNumberToDrop: 1)
                            });
                        break;

                    case PieceTemplate.Name.TreeSmall:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 15;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.3f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 50;
                        this.placeMaxDistance = 300;
                        this.allowedDensity = new AllowedDensity(radius: 300, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2)});
                        break;

                    case PieceTemplate.Name.TreeBig:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 40;
                        this.placeMaxDistance = 400;
                        this.allowedDensity = new AllowedDensity(radius: 360, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});
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
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 40;
                        this.placeMaxDistance = 400;
                        this.allowedDensity = new AllowedDensity(radius: 360, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});
                        break;

                    case PieceTemplate.Name.AppleTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 40;
                        this.placeMaxDistance = 400;
                        this.allowedDensity = new AllowedDensity(radius: 360, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});
                        break;

                    case PieceTemplate.Name.CherryTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 1000, 2500 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 35;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 40;
                        this.placeMaxDistance = 400;
                        this.allowedDensity = new AllowedDensity(radius: 360, maxNoOfPiecesSameName: 1);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});
                        break;

                    case PieceTemplate.Name.PalmTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 2500, 8000, 10000 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 12;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 200;
                        this.placeMaxDistance = 400;
                        this.allowedDensity = new AllowedDensity(radius: 400, maxNoOfPiecesSameName: 2);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 80, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        break;

                    case PieceTemplate.Name.BananaTree:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 2500, 8000, 10000 };
                        this.plantAdultSizeMass = this.maxMassForSize[1];
                        this.plantMassToBurn = 12;
                        this.plantReproductionData = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        this.boardTask = Scheduler.TaskName.DropFruit;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonDropFruit;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 200;
                        this.placeMaxDistance = 400;
                        this.allowedDensity = new AllowedDensity(radius: 400, maxNoOfPiecesSameName: 2);
                        this.isAffectedByWind = true;
                        this.windParticlesDict[ParticleEngine.Preset.WindLeaf] = Color.White;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood, ParticleEngine.Preset.DebrisLeaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 80, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});
                        break;

                    case PieceTemplate.Name.Acorn:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        this.stackSize = 6;
                        this.toolbarTask = Scheduler.TaskName.Plant;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.SeedsGeneric:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 5;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Plant;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.CoffeeRaw:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 5;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.toolbarTask = Scheduler.TaskName.Plant;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.CoffeeRoasted:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Apple:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 60;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        this.stackSize = 10;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Banana:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 80;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        this.stackSize = 10;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Cherry:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 30;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        this.stackSize = 16;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Tomato:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.15f;
                        this.plantBestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        this.canBePickedUp = true;
                        this.stackSize = 10;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Carrot:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.15f;
                        this.canBePickedUp = true;
                        this.stackSize = 10;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.MeatRawRegular:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 6;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f);
                        break;

                    case PieceTemplate.Name.MeatRawPrime:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 250;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 6;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f);
                        break;

                    case PieceTemplate.Name.Mushroom:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 60;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 8;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1 }, cooldown: 15, maxPitchVariation: 0.8f);
                        break;

                    case PieceTemplate.Name.MeatDried:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 250;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f);
                        break;

                    case PieceTemplate.Name.Fat:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.6f;
                        this.canBePickedUp = true;
                        this.stackSize = 5;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f);
                        break;

                    case PieceTemplate.Name.Leather:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.fireAffinity = 0.7f;
                        this.canBePickedUp = true;
                        this.stackSize = 4;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.HideCloth:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.stackSize = 4;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Burger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 560;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 5;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Meal:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.GetEaten;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Rabbit:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 35;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 200, 500 };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 10;
                        this.placeMaxDistance = 45;
                        this.animalMaxMass = 5000;
                        this.animalMassBurnedMultiplier = 1f;
                        this.animalAwareness = 200;
                        this.animalMatureAge = 1200;
                        this.animalPregnancyDuration = 2000;
                        this.animalMaxChildren = 8;
                        this.animalRetaliateChance = 0.1f;
                        this.animalSightRange = 400;
                        this.canBePickedUp = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.Cry] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CrySmallAnimal1, SoundData.Name.CrySmallAnimal2, SoundData.Name.CrySmallAnimal3, SoundData.Name.CrySmallAnimal4 }, maxPitchVariation: 0.3f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Eat] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatHerbivore1, SoundData.Name.EatHerbivore2, SoundData.Name.EatHerbivore3, SoundData.Name.EatHerbivore4, SoundData.Name.EatHerbivore5 }, maxPitchVariation: 0.25f, cooldown: 35);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisBlood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatRawRegular, chanceToDrop: 70, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatRawPrime, chanceToDrop: 1, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Fat, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Leather, chanceToDrop: 50, maxNumberToDrop: 1)
                            });
                        break;

                    case PieceTemplate.Name.Fox:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 60;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 500, 1000 };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 10;
                        this.placeMaxDistance = 45;
                        this.animalMaxMass = 15000;
                        this.animalMassBurnedMultiplier = 1.3f;
                        this.animalAwareness = 80;
                        this.animalMatureAge = 2000;
                        this.animalPregnancyDuration = 4000;
                        this.animalMaxChildren = 6;
                        this.animalRetaliateChance = 0.6f;
                        this.animalSightRange = 500;
                        this.canBePickedUp = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.Cry] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CrySmallAnimal1, SoundData.Name.CrySmallAnimal2, SoundData.Name.CrySmallAnimal3, SoundData.Name.CrySmallAnimal4 }, maxPitchVariation: 0.3f, pitchChange: -0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Eat] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPredator1, SoundData.Name.EatPredator2, SoundData.Name.EatPredator3, SoundData.Name.EatPredator4 }, maxPitchVariation: 0.25f, cooldown: 60);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisBlood },
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> {
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatRawRegular, chanceToDrop: 70, maxNumberToDrop: 2),
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatRawPrime, chanceToDrop: 5, maxNumberToDrop: 1),
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1),
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Leather, chanceToDrop: 80, maxNumberToDrop: 1) }
                          );
                        break;

                    case PieceTemplate.Name.Bear:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 80;
                        this.fireAffinity = 0.65f;
                        this.maxMassForSize = new int[] { 500, 2000 };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 10;
                        this.placeMaxDistance = 45;
                        this.animalMaxMass = 15000;
                        this.animalMassBurnedMultiplier = 0.5f;
                        this.animalAwareness = 50;
                        this.animalMatureAge = 4000;
                        this.animalPregnancyDuration = 3500;
                        this.animalMaxChildren = 5;
                        this.animalRetaliateChance = 1f;
                        this.animalSightRange = 700;
                        this.canBePickedUp = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.Cry] = new Sound(name: SoundData.Name.BearRoar, maxPitchVariation: 0.3f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Eat] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPredator1, SoundData.Name.EatPredator2, SoundData.Name.EatPredator3, SoundData.Name.EatPredator4 }, maxPitchVariation: 0.25f, cooldown: 60);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisBlood },
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> {
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatRawPrime, chanceToDrop: 70, maxNumberToDrop: 1),
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Fat, chanceToDrop: 70, maxNumberToDrop: 2),
                              new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Leather, chanceToDrop: 60, maxNumberToDrop: 2)
                          });
                        break;

                    case PieceTemplate.Name.Frog:
                        this.category = BoardPiece.Category.Flesh;
                        this.startingMass = 10;
                        this.fireAffinity = 0.15f;
                        this.maxMassForSize = new int[] { 300, 800 };
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMinDistance = 10;
                        this.placeMaxDistance = 45;
                        this.animalMaxMass = 1200;
                        this.animalMassBurnedMultiplier = 1;
                        this.animalAwareness = 100;
                        this.animalMatureAge = 1200;
                        this.animalPregnancyDuration = 2000;
                        this.animalMaxChildren = 8;
                        this.animalRetaliateChance = 0.05f;
                        this.animalSightRange = 250;
                        this.canBePickedUp = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.Cry] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryFrog1, SoundData.Name.CryFrog2, SoundData.Name.CryFrog3, SoundData.Name.CryFrog4, }, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Eat] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatHerbivore1, SoundData.Name.EatHerbivore2, SoundData.Name.EatHerbivore3, SoundData.Name.EatHerbivore4, SoundData.Name.EatHerbivore5 }, maxPitchVariation: 0.25f, cooldown: 35);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisBlood },
                        firstDroppedPieces: new List<Yield.DroppedPiece> { },
                        finalDroppedPieces: new List<Yield.DroppedPiece> {
                            new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatRawRegular, chanceToDrop: 40, maxNumberToDrop: 1),
                            new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1),
                        });
                        break;

                    case PieceTemplate.Name.MineralsSmall:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 500;
                        this.delayAfterCreationMinutes = 60;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 25, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.MineralsMossySmall:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 500;
                        this.delayAfterCreationMinutes = 60;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 25, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.MineralsBig:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 500;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(radius: 130, maxNoOfPiecesSameName: 0);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 25, maxNumberToDrop: 2)
                                });
                        break;

                    case PieceTemplate.Name.MineralsMossyBig:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 500;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(radius: 130, maxNoOfPiecesSameName: 0);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MineralsMossySmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MineralsMossySmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 25, maxNumberToDrop: 2)
                       });
                        break;

                    case PieceTemplate.Name.JarTreasureRich:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.4f;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 50;
                        this.inOpacityFadeDuration = 30;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.HasAppeared] = new Sound(name: SoundData.Name.Ding1);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitCeramic, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyCeramic3, maxPitchVariation: 0.5f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisCeramic },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarBroken, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.MeatDried, chanceToDrop: 50, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.BowBasic, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.BowAdvanced, chanceToDrop: 2, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.AxeStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.PickaxeStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ScytheStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ShovelStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.SpearStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                });
                        break;

                    case PieceTemplate.Name.JarTreasurePoor:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.4f;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 50;
                        this.inOpacityFadeDuration = 30;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.HasAppeared] = new Sound(name: SoundData.Name.Ding1);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitCeramic, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyCeramic3, maxPitchVariation: 0.5f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisCeramic },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarBroken, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ArrowStone, chanceToDrop: 15, maxNumberToDrop: 30),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.BowBasic, chanceToDrop: 5, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.AxeStone, chanceToDrop: 3, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.PickaxeStone, chanceToDrop: 3, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ScytheStone, chanceToDrop: 3, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ShovelStone, chanceToDrop: 3, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.SpearStone, chanceToDrop: 3, maxNumberToDrop: 1),
                                });
                        break;

                    case PieceTemplate.Name.JarBroken:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.5f;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 50;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitCeramic, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DestroyCeramic1, SoundData.Name.DestroyCeramic2 }, maxPitchVariation: 0.5f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisCeramic },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 20, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.CrateStarting:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.7f;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 50;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.AxeStone, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TorchBig, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TorchBig, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TorchBig, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TentModernPacked, chanceToDrop: 100, maxNumberToDrop: 1),
                                });
                        break;

                    case PieceTemplate.Name.CrateRegular:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.7f;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 50;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.AxeStone, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.PickaxeStone, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Burger, chanceToDrop: 100, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.EmptyBottle, chanceToDrop: 5, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.BackpackSmall, chanceToDrop: 15, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.BeltSmall, chanceToDrop: 15, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.GlassesVelvet, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TorchBig, chanceToDrop: 80, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TorchBig, chanceToDrop: 30, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TentModernPacked, chanceToDrop: 70, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.TentModernPacked, chanceToDrop: 30, maxNumberToDrop: 1),
                                });
                        break;

                    case PieceTemplate.Name.ChestWooden:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.movesWhenDropped = false;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOpenContainer;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitWood, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f);

                        break;

                    case PieceTemplate.Name.ChestStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.2f;
                        this.movesWhenDropped = false;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOpenContainer;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitWood, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f);

                        break;

                    case PieceTemplate.Name.ChestIron:
                        this.category = BoardPiece.Category.Metal;
                        this.movesWhenDropped = false;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOpenContainer;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitWood, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f);

                        break;

                    case PieceTemplate.Name.ChestCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.movesWhenDropped = false;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOpenContainer;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        this.containerStorageIsGlobal = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsHit] = new Sound(name: SoundData.Name.HitWood, maxPitchVariation: 0.5f);
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f);

                        break;

                    case PieceTemplate.Name.ChestTreasureNormal:
                        this.category = BoardPiece.Category.Metal;
                        this.movesWhenDropped = false;
                        this.boardTask = Scheduler.TaskName.OpenAndDestroyTreasureChest;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOpenContainer;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.inOpacityFadeDuration = 30;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;
                        this.appearDebris = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStarSmall });
                        customSoundsForActions[PieceSoundPackTemplate.Action.HasAppeared] = new Sound(name: SoundData.Name.Chime);

                        break;

                    case PieceTemplate.Name.ChestTreasureBig:
                        this.category = BoardPiece.Category.Metal;
                        this.movesWhenDropped = false;
                        this.boardTask = Scheduler.TaskName.OpenAndDestroyTreasureChest;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOpenContainer;
                        this.blocksMovement = true;
                        this.inOpacityFadeDuration = 30;
                        this.isAffectedByWind = true;
                        this.getsPushedByWaves = true;
                        this.appearDebris = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStarSmall });
                        customSoundsForActions[PieceSoundPackTemplate.Action.HasAppeared] = new Sound(name: SoundData.Name.Chime);
                        break;

                    case PieceTemplate.Name.CampfireSmall:
                        this.category = BoardPiece.Category.Stone;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonUseCampfire;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.CampfireMedium:
                        this.category = BoardPiece.Category.Stone;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonUseCampfire;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.WorkshopEssential:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.WorkshopBasic:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.WorkshopAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.WorkshopMaster:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.WorkshopLeatherBasic:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.WorkshopLeatherAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.AlchemyLabStandard:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        this.boardTask = Scheduler.TaskName.InteractWithLab;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonBrew;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.AlchemyLabAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.boardTask = Scheduler.TaskName.InteractWithLab;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonBrew;
                        this.fireAffinity = 0.3f;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.FurnaceConstructionSite:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.0f; // protected from fire
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        this.allowedDensity = new AllowedDensity(radius: 500, forbidOverlapSameClass: true);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Open] = new Sound(name: SoundData.Name.FireBurst, ignore3DAlways: true);
                        break;

                    case PieceTemplate.Name.FurnaceComplete:
                        this.category = BoardPiece.Category.Stone;
                        this.boardTask = Scheduler.TaskName.InteractWithFurnace;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonSmelt;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsOn] = new Sound(name: SoundData.Name.Smelting, isLooped: true);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Open] = new Sound(name: SoundData.Name.FireBurst, ignore3DAlways: true);
                        break;

                    case PieceTemplate.Name.Anvil:
                        this.category = BoardPiece.Category.Metal;
                        this.boardTask = Scheduler.TaskName.OpenCraftMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonUseAnvil;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.HotPlate:
                        this.category = BoardPiece.Category.Stone;
                        this.boardTask = Scheduler.TaskName.InteractWithCooker;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCook;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsOn] = new Sound(name: SoundData.Name.FryingPan, isLooped: true);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Open] = new Sound(name: SoundData.Name.StoneMove1, ignore3DAlways: true);

                        break;

                    case PieceTemplate.Name.CookingPot:
                        this.category = BoardPiece.Category.Metal;
                        this.boardTask = Scheduler.TaskName.InteractWithCooker;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCook;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsOn] = new Sound(name: SoundData.Name.Cooking, isLooped: true);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Open] = new Sound(name: SoundData.Name.PotLid, ignore3DAlways: true);
                        break;

                    case PieceTemplate.Name.WorkshopMeatHarvesting:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonHarvestMeat;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = true;
                        break;

                    case PieceTemplate.Name.MeatDryingRackRegular:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.blocksMovement = false;
                        this.hasFlatShadow = true;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        break;

                    case PieceTemplate.Name.MeatDryingRackWide:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.blocksMovement = false;
                        this.hasFlatShadow = true;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        break;

                    case PieceTemplate.Name.BoatConstructionSite:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.0f; // protected from fire
                        this.boardTask = Scheduler.TaskName.OpenContainer;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonCraft;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        this.allowedDensity = new AllowedDensity(radius: 1000, forbidOverlapSameClass: true);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Open] = new Sound(name: SoundData.Name.WoodCreak, ignore3DAlways: true);
                        break;

                    case PieceTemplate.Name.BoatCompleteStanding:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.0f; // protected from fire
                        this.boardTask = Scheduler.TaskName.OpenBoatMenu;
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        break;

                    case PieceTemplate.Name.BoatCompleteCruising:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.0f; // protected from fire
                        this.isAffectedByWind = false;
                        this.hasFlatShadow = true;
                        this.serialize = false;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.ShipRescue:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 0.0f; // protected from fire
                        this.isAffectedByWind = false;
                        this.hasFlatShadow = true;
                        this.serialize = false;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Totem:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.0f; // protected from fire
                        this.boardTask = Scheduler.TaskName.InteractWithTotem;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonOffer;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        this.isAffectedByWind = false;
                        this.allowedDensity = new AllowedDensity(radius: 2000, maxNoOfPiecesSameName: 0);
                        break;

                    case PieceTemplate.Name.RuinsColumn:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.allowedDensity = new AllowedDensity(radius: 200, maxNoOfPiecesSameName: 0);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 80, maxNumberToDrop: 2)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 30, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.RuinsRubble:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.allowedDensity = new AllowedDensity(radius: 100, maxNoOfPiecesSameName: 0);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 80, maxNumberToDrop: 3)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 15, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.RuinsWall:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.allowedDensity = new AllowedDensity(radius: 150, maxNoOfPiecesSameName: 0);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 80, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.Stick:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropStick, cooldown: 15, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.WoodLogRegular:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 4;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.WoodLogHard:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.9f;
                        this.canBePickedUp = true;
                        this.stackSize = 4;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.WoodPlank:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Stone:
                        this.category = BoardPiece.Category.Stone;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.Granite:
                        this.category = BoardPiece.Category.Stone;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 1000;
                        this.hasFlatShadow = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Clay:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 1000;
                        this.hasFlatShadow = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropMud, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.Rope:
                        this.category = BoardPiece.Category.Leather;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.stackSize = 6;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropRope, cooldown: 15, maxPitchVariation: 0.4f);
                        break;

                    case PieceTemplate.Name.Clam:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 50;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.stackSize = 6;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.CaveWeakMinerals:
                        this.category = BoardPiece.Category.Stone;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 500;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 5, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 2, maxNumberToDrop: 1),
                            });
                        break;

                    case PieceTemplate.Name.CoalDeposit:
                        this.category = BoardPiece.Category.Stone;
                        this.placeMaxDistance = 1000;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Coal, chanceToDrop: 100, maxNumberToDrop: 4)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Coal, chanceToDrop: 100, maxNumberToDrop: 10)});
                        break;

                    case PieceTemplate.Name.IronDeposit:
                        this.category = BoardPiece.Category.Stone;
                        this.placeMaxDistance = 1000;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 2)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 6)});
                        break;

                    case PieceTemplate.Name.BeachDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        this.placeMaxDistance = 1000;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarTreasureRich, chanceToDrop: 2, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 20, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clam, chanceToDrop: 30, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 20, maxNumberToDrop: 2)});
                        break;

                    case PieceTemplate.Name.ForestDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        this.placeMaxDistance = 1000;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarTreasureRich, chanceToDrop: 4, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 25, maxNumberToDrop: 2),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Acorn, chanceToDrop: 10, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 25, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.DesertDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        this.placeMaxDistance = 1000;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarTreasureRich, chanceToDrop: 6, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 25, maxNumberToDrop: 3),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 10, maxNumberToDrop: 2)});
                        break;

                    case PieceTemplate.Name.GlassDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        this.placeMaxDistance = 1000;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.GlassSand, chanceToDrop: 100, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 30, maxNumberToDrop: 2)});
                        break;

                    case PieceTemplate.Name.SwampDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        this.placeMaxDistance = 1000;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                                 firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                 finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ChestTreasureBig, chanceToDrop: 2, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.ChestTreasureNormal, chanceToDrop: 8, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarTreasureRich, chanceToDrop: 15, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.GlassSand, chanceToDrop: 15, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 50, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Acorn, chanceToDrop: 20, maxNumberToDrop: 1)});
                        break;

                    case PieceTemplate.Name.RuinsDigSite:
                        this.category = BoardPiece.Category.Dirt;
                        this.placeMaxDistance = 1000;
                        this.delayAfterCreationMinutes = 60;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                                 firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                 finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.JarTreasurePoor, chanceToDrop: 25, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Stone, chanceToDrop: 40, maxNumberToDrop: 3),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Granite, chanceToDrop: 20, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 20, maxNumberToDrop: 1)}
                                    );
                        break;

                    case PieceTemplate.Name.CrystalDepositSmall:
                        this.category = BoardPiece.Category.Crystal;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 1000;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisCrystal },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 3)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 8)});
                        break;

                    case PieceTemplate.Name.CrystalDepositBig:
                        this.category = BoardPiece.Category.Crystal;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 1000;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisCrystal },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Crystal, chanceToDrop: 70, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 3)});
                        break;

                    case PieceTemplate.Name.Coal:
                        this.category = BoardPiece.Category.Stone;
                        this.canBePickedUp = true;
                        this.stackSize = 8;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.IronOre:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        this.stackSize = 8;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.IronBar:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        this.stackSize = 5;
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropIronBar, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.IronRod:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        this.stackSize = 18;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropIronRod, cooldown: 15, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.IronNail:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        this.stackSize = 50;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.IronPlate:
                        this.category = BoardPiece.Category.Metal;
                        this.canBePickedUp = true;
                        this.stackSize = 18;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropIronPlate, cooldown: 15, maxPitchVariation: 0.8f);
                        break;

                    case PieceTemplate.Name.GlassSand:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        this.stackSize = 8;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;

                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.Crystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.canBePickedUp = true;
                        this.stackSize = 12;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropCrystal, cooldown: 15, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.Backlight:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.destructionDelay = -1;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.BloodSplatter:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.destructionDelay = 250;
                        this.placeMaxDistance = 10;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Attack:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.destructionDelay = -1;
                        this.placeMaxDistance = 3;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Miss:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.destructionDelay = 60;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Zzz:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        this.inOpacityFadeDuration = 30;
                        break;

                    case PieceTemplate.Name.Heart:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.destructionDelay = 40;
                        this.placeMaxDistance = 2;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Orbiter:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.MapMarker:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.MusicNote:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Crosshair:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.BubbleExclamationRed:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        this.inOpacityFadeDuration = 30;
                        break;

                    case PieceTemplate.Name.BubbleExclamationBlue:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.BubbleCraftGreen:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.Explosion:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.destructionDelay = -1;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        this.floatsOnWater = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.HasAppeared] = new Sound(name: SoundData.Name.ShootFire, maxPitchVariation: 0.6f);
                        break;

                    case PieceTemplate.Name.CookingTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.SmeltingTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.UpgradeTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.MeatHarvestTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.OfferTrigger:
                        this.category = BoardPiece.Category.Indestructible;
                        this.canBePickedUp = true;
                        break;

                    case PieceTemplate.Name.ConstructTrigger:
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
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.toolHitCooldown = 50;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Wood, 1.0f } };
                        break;

                    case PieceTemplate.Name.AxeWood:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 40;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.Stone, 1f }, { BoardPiece.Category.Wood, 5f }, { BoardPiece.Category.Flesh, 3f }, { BoardPiece.Category.Leather, 5f } };
                        break;

                    case PieceTemplate.Name.AxeStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 30;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 2f }, { BoardPiece.Category.Wood, 8f }, { BoardPiece.Category.Flesh, 5f }, { BoardPiece.Category.Leather, 8f } };
                        break;

                    case PieceTemplate.Name.AxeIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 25;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 3f }, { BoardPiece.Category.Wood, 15 }, { BoardPiece.Category.Flesh, 8f }, { BoardPiece.Category.Leather, 15f } };
                        break;

                    case PieceTemplate.Name.AxeCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 20;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 6f }, { BoardPiece.Category.Stone, 6f }, { BoardPiece.Category.Wood, 40 }, { BoardPiece.Category.Flesh, 10f }, { BoardPiece.Category.Leather, 40f } };
                        break;

                    case PieceTemplate.Name.PickaxeWood:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 40;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.Stone, 5f }, { BoardPiece.Category.Wood, 1f } };
                        break;

                    case PieceTemplate.Name.PickaxeStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 30;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 8f }, { BoardPiece.Category.Wood, 2f } };
                        break;

                    case PieceTemplate.Name.PickaxeIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 25;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 15f }, { BoardPiece.Category.Wood, 3f }, { BoardPiece.Category.Crystal, 3f } };
                        break;

                    case PieceTemplate.Name.PickaxeCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 20;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 6f }, { BoardPiece.Category.Stone, 30f }, { BoardPiece.Category.Wood, 6f }, { BoardPiece.Category.Crystal, 6f } };
                        break;

                    case PieceTemplate.Name.SpearWood:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 40;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 30f } };
                        break;

                    case PieceTemplate.Name.SpearStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 30;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 40f } };
                        break;

                    case PieceTemplate.Name.SpearIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 25;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 60f } };
                        break;

                    case PieceTemplate.Name.SpearCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 20;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 85 } };
                        break;

                    case PieceTemplate.Name.ScytheStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 35;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 6f } };
                        break;

                    case PieceTemplate.Name.ScytheIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 30;
                        this.toolRange = 10;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 6f } };
                        break;

                    case PieceTemplate.Name.ScytheCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 25;
                        this.toolRange = 23;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 12f } };
                        break;

                    case PieceTemplate.Name.ShovelStone:
                        this.category = BoardPiece.Category.Stone;
                        this.fireAffinity = 0.4f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 40;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 4f } };
                        break;

                    case PieceTemplate.Name.ShovelIron:
                        this.category = BoardPiece.Category.Metal;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 30;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 12f } };
                        break;

                    case PieceTemplate.Name.ShovelCrystal:
                        this.category = BoardPiece.Category.Crystal;
                        this.fireAffinity = 0.1f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolHitCooldown = 25;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 20f } };
                        break;

                    case PieceTemplate.Name.BowBasic:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.getsPushedByWaves = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 9 } };
                        this.toolShootsProjectile = true;
                        this.toolCompatibleAmmo = new List<PieceTemplate.Name> { PieceTemplate.Name.ArrowWood, PieceTemplate.Name.ArrowStone, PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowCrystal, PieceTemplate.Name.ArrowExploding };
                        break;

                    case PieceTemplate.Name.BowAdvanced:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.Hit;
                        this.floatsOnWater = true;
                        this.toolMultiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 36 } };
                        this.toolShootsProjectile = true;
                        this.getsPushedByWaves = true;
                        this.toolCompatibleAmmo = new List<PieceTemplate.Name> { PieceTemplate.Name.ArrowWood, PieceTemplate.Name.ArrowStone, PieceTemplate.Name.ArrowIron, PieceTemplate.Name.ArrowCrystal, PieceTemplate.Name.ArrowExploding };
                        break;

                    case PieceTemplate.Name.ArrowWood:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 15;
                        this.floatsOnWater = true;
                        this.projectileHitMultiplier = 2;
                        this.projectileCanBeStuck = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.ArrowStone:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 15;
                        this.floatsOnWater = true;
                        this.projectileHitMultiplier = 3f;
                        this.projectileCanBeStuck = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.ArrowIron:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 15;
                        this.floatsOnWater = true;
                        this.projectileHitMultiplier = 4f;
                        this.projectileCanBeStuck = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.ArrowCrystal:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 15;
                        this.floatsOnWater = true;
                        this.projectileHitMultiplier = 7f;
                        this.projectileCanBeStuck = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.ArrowExploding:
                        this.category = BoardPiece.Category.Indestructible;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 15;
                        this.floatsOnWater = true;
                        this.projectileHitMultiplier = 3f;
                        this.projectileCanBeStuck = true;
                        this.projectileCanExplode = true;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.TentModernPacked:
                        this.category = BoardPiece.Category.Leather;
                        this.canBePickedUp = true;
                        this.stackSize = 1;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.TentModern:
                        this.category = BoardPiece.Category.Leather;
                        this.fireAffinity = 0.4f;
                        this.boardTask = Scheduler.TaskName.OpenShelterMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonGoToSleep;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.TentSmall:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.boardTask = Scheduler.TaskName.OpenShelterMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonGoToSleep;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.TentMedium:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.7f;
                        this.boardTask = Scheduler.TaskName.OpenShelterMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonGoToSleep;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.TentBig:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.6f;
                        this.boardTask = Scheduler.TaskName.OpenShelterMenu;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonGoToSleep;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.destroysPlantsWhenBuilt = true;
                        break;

                    case PieceTemplate.Name.BackpackSmall:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 200;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BackpackMedium:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BackpackBig:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 700;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BackpackLuxurious:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 900;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BeltSmall:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 200;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BeltMedium:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 300;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BeltBig:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 400;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BeltLuxurious:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Map:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 100;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.Dungarees:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 150;
                        this.fireAffinity = 0.5f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.HatSimple:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 100;
                        this.fireAffinity = 0.8f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BootsProtective:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BootsMountain:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BootsAllTerrain:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.BootsSpeed:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.GlovesStrength:
                        this.category = BoardPiece.Category.Leather;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.GlassesVelvet:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 500;
                        this.fireAffinity = 0.3f;
                        this.canBePickedUp = true;
                        this.placeMaxDistance = 500;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlastic, cooldown: 15, maxPitchVariation: 0.1f);
                        break;

                    case PieceTemplate.Name.TorchSmall:
                        this.category = BoardPiece.Category.Wood;
                        this.startingMass = 100;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 3;
                        this.toolbarTask = Scheduler.TaskName.SwitchLightSource;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.TorchBig:
                        this.category = BoardPiece.Category.Wood;
                        this.startingMass = 180;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.stackSize = 3;
                        this.toolbarTask = Scheduler.TaskName.SwitchLightSource;
                        this.getsPushedByWaves = true;
                        break;

                    case PieceTemplate.Name.LanternFull:
                        this.category = BoardPiece.Category.Metal;
                        this.startingMass = 350;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.SwitchLightSource;
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.MetalicClank, cooldown: 15, maxPitchVariation: 0.3f);

                        break;

                    case PieceTemplate.Name.LanternEmpty:
                        this.category = BoardPiece.Category.Metal;
                        this.startingMass = 300;
                        this.canBePickedUp = true;
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.MetalicClank, cooldown: 15, maxPitchVariation: 0.3f);
                        break;

                    case PieceTemplate.Name.Candle:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 6;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropGeneric, cooldown: 15, maxPitchVariation: 0.4f);
                        break;

                    case PieceTemplate.Name.HumanSkeleton:
                        this.category = BoardPiece.Category.Flesh;
                        this.fireAffinity = 0.3f;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 500;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.PredatorRepellant:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.fireAffinity = 0.0f;
                        this.destructionDelay = 60 * 60 * 5;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        break;

                    case PieceTemplate.Name.HerbsBlack:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsBrown:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsDarkViolet:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsDarkGreen:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsBlue:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsCyan:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsGreen:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsYellow:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsRed:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.HerbsViolet:
                        this.category = BoardPiece.Category.SmallPlant;
                        this.startingMass = 30;
                        this.fireAffinity = 0.2f;
                        this.canBePickedUp = true;
                        this.stackSize = 20;
                        this.placeMaxDistance = 200;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f);
                        break;

                    case PieceTemplate.Name.EmptyBottle:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 100;
                        this.canBePickedUp = true;
                        this.stackSize = 3;
                        this.placeMaxDistance = 1000;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f);
                        break;

                    case PieceTemplate.Name.PotionGeneric:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.canBePickedUp = true;
                        this.toolbarTask = Scheduler.TaskName.GetDrinked;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f);
                        break;

                    case PieceTemplate.Name.BottleOfOil:
                        this.category = BoardPiece.Category.Indestructible;
                        this.startingMass = 200;
                        this.fireAffinity = 1.0f;
                        this.canBePickedUp = true;
                        this.getsPushedByWaves = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDropped] = new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f);
                        break;

                    case PieceTemplate.Name.Hole:
                        this.category = BoardPiece.Category.Indestructible;
                        this.movesWhenDropped = false;
                        this.destructionDelay = 60 * 30;
                        this.blocksMovement = true;
                        this.placeMaxDistance = 500;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.TreeStump:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 0.8f;
                        this.movesWhenDropped = false;
                        this.blocksMovement = true;
                        this.hasFlatShadow = false;
                        this.placeMaxDistance = 500;
                        customSoundsForActions[PieceSoundPackTemplate.Action.IsDestroyed] = new Sound(name: SoundData.Name.DestroyStump);

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.WoodLogRegular, chanceToDrop: 20, maxNumberToDrop: 1) });
                        break;

                    case PieceTemplate.Name.LavaFlame:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 500;
                        this.inOpacityFadeDuration = 30;
                        this.allowedDensity = new AllowedDensity(radius: 100, maxNoOfPiecesSameName: 1);
                        break;

                    case PieceTemplate.Name.SwampGas:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.fireAffinity = 1.0f;
                        this.placeMaxDistance = 0;
                        this.visFogExplodesWhenBurns = true;
                        this.allowedDensity = new AllowedDensity(radius: 370, maxNoOfPiecesSameName: 3);
                        this.inOpacityFadeDuration = 180;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.WeatherFog:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.inOpacityFadeDuration = 60 * 3;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.SwampGeyser:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.allowedDensity = new AllowedDensity(radius: 320, maxNoOfPiecesSameName: 1);
                        break;

                    case PieceTemplate.Name.LavaGas:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.fireAffinity = 1.0f;
                        this.placeMaxDistance = 0;
                        this.allowedDensity = new AllowedDensity(radius: 100, maxNoOfPiecesSameName: 1);
                        this.inOpacityFadeDuration = 120;
                        this.floatsOnWater = true;
                        break;

                    case PieceTemplate.Name.SoundLakeWaves:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.floatsOnWater = true;
                        this.ambsoundPlayDelay = 300;
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.LakeWave1, SoundData.Name.LakeWave2, SoundData.Name.LakeWave3, SoundData.Name.LakeWave4, SoundData.Name.LakeWave5, SoundData.Name.LakeWave6, SoundData.Name.LakeWave7 }, maxPitchVariation: 0.8f, volume: 0.7f);
                        break;

                    case PieceTemplate.Name.SoundSeaWind:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.floatsOnWater = true;
                        this.ambsoundPlayDelay = 0;
                        this.ambsoundPlayDelayMaxVariation = 200;
                        this.ambsoundGeneratesWind = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(name: SoundData.Name.SeaWind, maxPitchVariation: 0.5f, volume: 0.6f, isLooped: true, volumeFadeFrames: 60);
                        break;

                    case PieceTemplate.Name.SoundNightCrickets:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.floatsOnWater = true;
                        this.ambsoundPlayDelay = 0;
                        this.ambsoundPartOfDayList = new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Night };
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(name: SoundData.Name.NightCrickets, maxPitchVariation: 0.3f, volume: 0.2f, isLooped: true, volumeFadeFrames: 60);
                        break;

                    case PieceTemplate.Name.SoundNoonCicadas:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.floatsOnWater = true;
                        this.ambsoundPlayDelay = 0;
                        this.ambsoundPartOfDayList = new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Noon };
                        this.ambsoundPlayOnlyWhenIsSunny = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.Cicadas1, SoundData.Name.Cicadas2, SoundData.Name.Cicadas3 }, maxPitchVariation: 0.3f, volume: 0.7f, isLooped: true, volumeFadeFrames: 60);
                        break;

                    case PieceTemplate.Name.SoundLava:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.floatsOnWater = true;
                        this.ambsoundPlayDelay = 0;
                        this.allowedDensity = new AllowedDensity(radius: 100, maxNoOfPiecesSameName: 1);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(name: SoundData.Name.Lava, maxPitchVariation: 0.5f, volume: 1f, isLooped: true, volumeFadeFrames: 60);
                        break;

                    case PieceTemplate.Name.SeaWave:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 500;
                        this.floatsOnWater = true;
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.SeaWave1, SoundData.Name.SeaWave2, SoundData.Name.SeaWave3, SoundData.Name.SeaWave4, SoundData.Name.SeaWave5, SoundData.Name.SeaWave6, SoundData.Name.SeaWave7, SoundData.Name.SeaWave8, SoundData.Name.SeaWave9, SoundData.Name.SeaWave10, SoundData.Name.SeaWave11, SoundData.Name.SeaWave12, SoundData.Name.SeaWave13 }, maxPitchVariation: 0.8f, volume: 0.8f);
                        break;

                    case PieceTemplate.Name.SoundCaveWaterDrip:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.floatsOnWater = true;
                        this.ambsoundPlayDelay = 60 * 30;
                        this.allowedDensity = new AllowedDensity(radius: 400, maxNoOfPiecesSameName: 0);
                        customSoundsForActions[PieceSoundPackTemplate.Action.Ambient] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.WaterDrop }, maxPitchVariation: 1f, volume: 1.0f);
                        break;

                    case PieceTemplate.Name.ParticleEmitterEnding:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        break;

                    case PieceTemplate.Name.ParticleEmitterWeather:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        break;

                    case PieceTemplate.Name.HastePlayerClone:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        break;

                    case PieceTemplate.Name.EmptyVisualEffect:
                        this.category = BoardPiece.Category.Indestructible;
                        this.serialize = false;
                        this.placeMaxDistance = 0;
                        this.ignoresCollisions = true;
                        break;

                    case PieceTemplate.Name.FertileGroundSmall:
                        this.category = BoardPiece.Category.Dirt;
                        this.blocksPlantGrowth = false;
                        this.blocksMovement = false;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);
                        this.destroysPlantsWhenBuilt = true;
                        this.fertileGroundSoilWealthMultiplier = 1.4f;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 70, maxNumberToDrop: 1) });

                        break;

                    case PieceTemplate.Name.FertileGroundMedium:
                        this.category = BoardPiece.Category.Dirt;
                        this.blocksPlantGrowth = false;
                        this.blocksMovement = false;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);
                        this.destroysPlantsWhenBuilt = true;
                        this.fertileGroundSoilWealthMultiplier = 1.7f;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 70, maxNumberToDrop: 2) });

                        break;

                    case PieceTemplate.Name.FertileGroundLarge:
                        this.category = BoardPiece.Category.Dirt;
                        this.blocksPlantGrowth = false;
                        this.blocksMovement = false;
                        this.allowedDensity = new AllowedDensity(forbidOverlapSameClass: true);
                        this.destroysPlantsWhenBuilt = true;
                        this.fertileGroundSoilWealthMultiplier = 2.2f;

                        this.Yield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisStone },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.Clay, chanceToDrop: 70, maxNumberToDrop: 3) });

                        break;

                    case PieceTemplate.Name.FenceHorizontalShort:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.boardTask = Scheduler.TaskName.MakePlayerJumpOverThisPiece;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonJump;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.FenceVerticalShort:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.boardTask = Scheduler.TaskName.MakePlayerJumpOverThisPiece;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonJump;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.FenceHorizontalLong:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.boardTask = Scheduler.TaskName.MakePlayerJumpOverThisPiece;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonJump;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.FenceVerticalLong:
                        this.category = BoardPiece.Category.Wood;
                        this.fireAffinity = 1.0f;
                        this.boardTask = Scheduler.TaskName.MakePlayerJumpOverThisPiece;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonJump;
                        this.blocksMovement = true;
                        this.destroysPlantsWhenBuilt = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.CaveEntranceOutside:
                        this.category = BoardPiece.Category.Indestructible;
                        this.boardTask = Scheduler.TaskName.UseEntrance;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonEnterExit;
                        this.allowedDensity = new AllowedDensity(radius: 6000, maxNoOfPiecesSameClass: 0);
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        this.delayAfterCreationMinutes = 30;
                        break;

                    case PieceTemplate.Name.CaveEntranceInside:
                        this.category = BoardPiece.Category.Indestructible;
                        this.boardTask = Scheduler.TaskName.UseEntrance;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonEnterExit;
                        this.allowedDensity = new AllowedDensity(radius: 500, maxNoOfPiecesBlocking: 0);
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.CaveExit:
                        this.category = BoardPiece.Category.Indestructible;
                        this.boardTask = Scheduler.TaskName.UseEntrance;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonEnterExit;
                        this.allowedDensity = new AllowedDensity(radius: 1000, maxNoOfPiecesSameClass: 0);
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        break;

                    case PieceTemplate.Name.CaveExitEmergency:
                        this.category = BoardPiece.Category.Indestructible;
                        this.boardTask = Scheduler.TaskName.UseEntrance;
                        this.interactVirtButtonName = TextureBank.TextureName.VirtButtonEnterExit;
                        this.allowedDensity = new AllowedDensity(radius: 1000, maxNoOfPiecesSameClass: 0);
                        this.blocksMovement = true;
                        this.hasFlatShadow = true;
                        break;

                    default:
                        throw new ArgumentException($"Unsupported name - {this.name}.");
                }

                // adding dust puff to Yield
                if (this.blocksMovement &&
                    this.Yield != null &&
                    this.type != typeof(Animal) &&
                    this.type != typeof(Player)
                    )
                    this.Yield.FinalDebrisTypeList.Add(ParticleEngine.Preset.DustPuff);

                // setting pieceSoundTemplate

                this.pieceSoundPackTemplate = new PieceSoundPackTemplate(pieceInfo: this);
                foreach (var kvp in customSoundsForActions)
                {
                    PieceSoundPackTemplate.Action action = kvp.Key;
                    Sound sound = kvp.Value;
                    this.pieceSoundPackTemplate.AddAction(action: action, sound: sound, replaceExisting: true);
                }

                // setting some variables, that need params non-present in boardPiece
                if (this.maxMassForSize != null) piece.sprite.AssignNewSize((byte)(this.maxMassForSize.Length - 1));
                this.animPkgName = piece.sprite.AnimPackage;

                // checking for params, that need to be set

                if (this.category == BoardPiece.Category.NotSet) throw new ArgumentNullException($"{this.name} - category not set.");
                if (this.startingMass <= 0) throw new ArgumentNullException($"{this.name} - starting mass incorrect value.");

                if (this.type == typeof(Plant))
                {
                    if (this.plantMassToBurn == 0) throw new ArgumentNullException($"{this.name} - plantMassToBurn not set.");
                    if (this.plantReproductionData == null) throw new ArgumentNullException($"{this.name} - plantReproductionData not set.");
                    if (this.plantBestEnvironment == null) throw new ArgumentNullException($"{this.name} - plantBestEnvironment not set.");
                }
                else if (this.type == typeof(Animal))
                {
                    if (this.animalMaxMass == 0) throw new ArgumentNullException($"{this.name} - animalMaxMass not set.");
                    if (this.animalMassBurnedMultiplier == 0) throw new ArgumentNullException($"{this.name} - animalMassBurnedMultiplier not set.");
                    if (this.animalAwareness == 0) throw new ArgumentNullException($"{this.name} - animalAwareness not set.");
                    if (this.animalMatureAge == 0) throw new ArgumentNullException($"{this.name} - animalMatureAge not set.");
                    if (this.animalPregnancyDuration == 0) throw new ArgumentNullException($"{this.name} - animalPregnancyDuration not set.");
                    if (this.animalMaxChildren == 0) throw new ArgumentNullException($"{this.name} - animalMaxChildren not set.");
                    if (this.animalRetaliateChance == -1) throw new ArgumentNullException($"{this.name} - animalRetaliateChance not set.");
                    if (this.animalSightRange == 0) throw new ArgumentNullException($"{this.name} - animalSightRange not set.");
                }
                else if (this.type == typeof(AmbientSound))
                {
                    if (this.ambsoundPlayDelay == -1) throw new ArgumentNullException($"{this.name} - ambsoundPlayDelay not set.");
                }
                else if (this.type == typeof(ConstructionSite))
                {
                    this.shadowNotDrawn = true;
                }
            }

            private static void AddPlayerCommonSounds(Dictionary<PieceSoundPackTemplate.Action, Sound> customSoundsForActions, bool female)
            {
                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerBowDraw] = new Sound(name: SoundData.Name.BowDraw, maxPitchVariation: 0.15f, volume: 0.6f, ignore3DAlways: true);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerBowRelease] = new Sound(name: SoundData.Name.BowRelease, maxPitchVariation: 0.3f, ignore3DAlways: true);
                customSoundsForActions[PieceSoundPackTemplate.Action.Eat] = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPlayer1, SoundData.Name.EatPlayer2, SoundData.Name.EatPlayer3, SoundData.Name.EatPlayer4 }, maxPitchVariation: 0.3f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerSnore] = new Sound(name: female ? SoundData.Name.SnoringFemale : SoundData.Name.SnoringMale, maxPitchVariation: 0.3f, ignore3DAlways: true, isLooped: true, volume: 0.5f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerPant] = new Sound(name: female ? SoundData.Name.PantFemale : SoundData.Name.PantMale, maxPitchVariation: 0.2f, ignore3DAlways: true, volume: 0.8f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerYawn] = new Sound(name: female ? SoundData.Name.YawnFemale : SoundData.Name.YawnMale, maxPitchVariation: 0.2f, ignore3DAlways: true, volume: 1f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerStomachGrowl] = new Sound(name: SoundData.Name.StomachGrowl, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 1f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerSprint] = new Sound(name: SoundData.Name.Sprint, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 0.5f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerJump] = new Sound(name: SoundData.Name.Jump, pitchChange: female ? 0.4f : -0.18f, maxPitchVariation: 0.15f, ignore3DAlways: true, volume: 0.5f);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerSpeak] = new Sound(name: SoundData.Name.Beep, cooldown: 4, volume: 0.12f, pitchChange: female ? 0.5f : -0.5f, maxPitchVariation: 0.07f, ignore3DAlways: true);

                customSoundsForActions[PieceSoundPackTemplate.Action.PlayerPulse] = new Sound(name: SoundData.Name.Pulse, isLooped: true, ignore3DAlways: true, volume: 0.7f);

                customSoundsForActions[PieceSoundPackTemplate.Action.Die] = new Sound(name: female ? SoundData.Name.DeathPlayerFemale : SoundData.Name.DeathPlayerMale);

                customSoundsForActions[PieceSoundPackTemplate.Action.Cry] = female ?
                    new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryPlayerFemale1, SoundData.Name.CryPlayerFemale2, SoundData.Name.CryPlayerFemale3, SoundData.Name.CryPlayerFemale4 }, maxPitchVariation: 0.2f) :
                    new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryPlayerMale1, SoundData.Name.CryPlayerMale2, SoundData.Name.CryPlayerMale3, SoundData.Name.CryPlayerMale4 }, maxPitchVariation: 0.2f);
            }

            public static void GetYieldForAntiCraft()
            {
                foreach (Info info in info.Values)
                {
                    if (info.Yield == null && Yield.antiCraftRecipes.ContainsKey(info.name)) info.Yield = Yield.antiCraftRecipes[info.name].ConvertToYield();
                }
            }

            private static Yield CreatePlayerYield()
            {
                return new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisBlood },
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: PieceTemplate.Name.HumanSkeleton, chanceToDrop: 100, maxNumberToDrop: 1) });
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

            return info[pieceName].Texture;
        }

        public static void CreateAllInfo()
        {
            // creates one instance of every piece type - to get required info out of it
            {
                if (SonOfRobinGame.os == OS.Windows) // using parallel here freezes on mobile and linux
                {
                    ConcurrentDictionary<PieceTemplate.Name, Info> infoByNameConcurrentDict = new();
                    Parallel.ForEach(PieceTemplate.allNames, SonOfRobinGame.defaultParallelOptions, name =>
                    {
                        infoByNameConcurrentDict[name] = new Info(piece: PieceTemplate.CreatePiece(templateName: name, world: null));
                    });

                    foreach (PieceTemplate.Name name in PieceTemplate.allNames)
                    {
                        info[name] = infoByNameConcurrentDict[name];
                    }
                }
                else
                {
                    foreach (PieceTemplate.Name name in PieceTemplate.allNames)
                    {
                        info[name] = new Info(piece: PieceTemplate.CreatePiece(templateName: name, world: null));
                    }
                }
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

        public static HashSet<PieceTemplate.Name> GetNamesForEquipType(Equipment.EquipType equipType)
        {
            var equipNameList = new HashSet<PieceTemplate.Name>();
            foreach (Info currentInfo in AllInfo)
            {
                if (currentInfo.equipType == equipType) equipNameList.Add(currentInfo.name);
            }

            return equipNameList;
        }

        public static List<InfoWindow.TextEntry> GetCategoryAffinityTextEntryList(PieceTemplate.Name pieceName, float scale = 1f)
        {
            var entryList = new List<InfoWindow.TextEntry>();

            var multiplierByCategory = info[pieceName].toolMultiplierByCategory;
            if (multiplierByCategory == null) return entryList;

            string text = "|     ";
            var imageList = new List<Texture2D> { TextureBank.GetTexture(TextureBank.TextureName.Biceps) };

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
            if (combinesWith.Count == 0) return entryList;

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