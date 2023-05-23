﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin

{
    public class PieceTemplate
    {
        private static readonly Dictionary<Name, int> opacityFadeDurationByName = new Dictionary<Name, int> {
            { Name.BurningFlame, 30 },
            { Name.Zzz, 30 },
            { Name.BubbleExclamationRed, 30 },
            { Name.JarTreasure, 30 },
            { Name.ChestTreasureNormal, 30 },
            { Name.ChestTreasureBig, 30 },
            { Name.SwampGas, 180 },
            { Name.LavaGas, 120 },
            { Name.LavaFlame, 30 },
            };

        public enum Name
        {
            Empty, // to be used instead of null

            PlayerBoy,
            PlayerGirl,
            PlayerTestDemoness,
            PlayerGhost,

            GrassRegular,
            GrassGlow,
            GrassDesert,

            PlantPoison,
            Rushes,
            WaterLily,

            FlowersPlain,
            FlowersRed,
            FlowersMountain,

            Cactus,

            SeedsGeneric,
            CoffeeRaw,
            CoffeeRoasted,

            TreeSmall,
            TreeBig,
            PalmTree,
            Oak,
            AppleTree,
            CherryTree,
            BananaTree,

            CarrotPlant,
            TomatoPlant,
            CoffeeShrub,

            Apple,
            Banana,
            Cherry,
            Tomato,
            Carrot,
            Acorn,
            MeatRaw,
            MeatDried,
            Fat,
            Leather,

            Burger,
            Meal,

            Rabbit,
            Fox,
            Tiger,
            Frog,

            MineralsBig,
            MineralsSmall,
            MineralsMossyBig,
            MineralsMossySmall,

            JarTreasure,
            JarBroken,
            CrateStarting,
            CrateRegular,

            ChestWooden,
            ChestStone,
            ChestIron,
            ChestCrystal,
            ChestTreasureNormal,
            ChestTreasureBig,

            Campfire,

            WorkshopEssential,
            WorkshopBasic,
            WorkshopAdvanced,
            WorkshopMaster,

            WorkshopLeatherBasic,
            WorkshopLeatherAdvanced,

            AlchemyLabStandard,
            AlchemyLabAdvanced,

            Furnace,
            Anvil,
            HotPlate,
            CookingPot,
            UpgradeBench,


            Stick,
            WoodLogRegular,
            WoodLogHard,
            WoodPlank,
            Stone,
            Granite,
            Clay,
            Rope,
            Clam,

            CoalDeposit,
            IronDeposit,

            BeachDigSite,
            ForestDigSite,
            DesertDigSite,
            GlassDigSite,
            SwampDigSite,

            CrystalDepositSmall,
            CrystalDepositBig,

            Coal,
            IronOre,
            IronBar,
            IronRod,
            IronNail,
            IronPlate,
            GlassSand,
            Crystal,

            Backlight,
            BloodSplatter,
            Attack,
            Miss,
            Zzz,
            Heart,
            MapMarker,
            MusicNote,
            Crosshair,

            BubbleExclamationRed,
            BubbleExclamationBlue,
            BubbleCraftGreen,

            RainDrop,
            Explosion,
            BurningFlame,

            CookingTrigger,
            UpgradeTrigger,
            BrewTrigger,
            FireplaceTriggerOn,
            FireplaceTriggerOff,

            KnifeSimple,
            AxeWood,
            AxeStone,
            AxeIron,
            AxeCrystal,

            PickaxeWood,
            PickaxeStone,
            PickaxeIron,
            PickaxeCrystal,

            SpearWood,
            SpearStone,
            SpearIron,
            SpearCrystal,

            ScytheStone,
            ScytheIron,
            ScytheCrystal,

            ShovelStone,
            ShovelIron,
            ShovelCrystal,

            BowBasic,
            BowAdvanced,

            ArrowWood,
            ArrowStone,
            ArrowIron,
            ArrowCrystal,
            ArrowBurning,

            DebrisPlant,
            DebrisStone,
            DebrisWood,
            DebrisLeaf,
            DebrisCrystal,
            DebrisCeramic,
            DebrisStar,
            DebrisSoot,
            DebrisHeart,
            BloodDrop,

            TentSmall,
            TentMedium,
            TentBig,

            BackpackSmall,
            BackpackMedium,
            BackpackBig,

            BeltSmall,
            BeltMedium,
            BeltBig,
            Map,

            Dungarees,
            HatSimple,
            BootsProtective,

            TorchSmall,
            TorchBig,

            LanternEmpty,
            LanternFull,
            Candle,

            HumanSkeleton,
            PredatorRepellant,

            HerbsBlack,
            HerbsBlue,
            HerbsCyan,
            HerbsGreen,
            HerbsYellow,
            HerbsRed,
            HerbsViolet,

            EmptyBottle,

            PotionGeneric,
            PotionCoffee,
            BottleOfOil,

            Hole,
            TreeStump,

            LavaFlame,
            SwampGas,
            LavaGas,

            SoundSeaWaves,
            SoundLakeWaves,
            SoundSeaWind,
            SoundNightCrickets,
            SoundNoonCicadas,
            SoundLava,
        }

        public static readonly Name[] allNames = (Name[])Enum.GetValues(typeof(Name));

        public static BoardPiece Create(Name templateName, World world, int generation = 0, string id = null)
        {
            return CreatePiece(templateName: templateName, world: world, id: id, generation: generation);
        }

        public static BoardPiece CreateAndPlaceOnBoard(Name templateName, World world, Vector2 position, bool randomPlacement = false, int generation = 0, bool ignoreCollisions = false, string id = null, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false)
        {
            BoardPiece boardPiece = CreatePiece(templateName: templateName, world: world, id: id, generation: generation);

            boardPiece.PlaceOnBoard(randomPlacement: randomPlacement, position: position, ignoreCollisions: ignoreCollisions, closestFreeSpot: closestFreeSpot, minDistanceOverride: minDistanceOverride, maxDistanceOverride: maxDistanceOverride, ignoreDensity: ignoreDensity, addPlannedDestruction: true);

            if (boardPiece.sprite.IsOnBoard)
            {
                // duplicated in Yield
                boardPiece.soundPack.Play(PieceSoundPack.Action.HasAppeared);
                if (boardPiece.appearDebris != null) boardPiece.appearDebris.DropDebris(ignoreProcessingTime: true);

                // adding opacityFade

                if (boardPiece.sprite.IsInCameraRect && (opacityFadeDurationByName.ContainsKey(templateName) || boardPiece.GetType() == typeof(Plant)))
                {
                    float destOpacity = boardPiece.sprite.opacity;
                    boardPiece.sprite.opacity = 0f;
                    new OpacityFade(sprite: boardPiece.sprite, destOpacity: destOpacity,
                        duration: opacityFadeDurationByName.ContainsKey(templateName) ? opacityFadeDurationByName[templateName] : 30);
                }
            }

            return boardPiece;
        }

        private static BoardPiece CreatePiece(Name templateName, World world, int generation = 0, string id = null)
        {
            if (id == null) id = Helpers.GetUniqueHash();

            Random random = BoardPiece.Random;

            AllowedTerrain shallowWaterToVolcano = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});

            switch (templateName)
            {
                case Name.PlayerBoy:
                    {
                        var soundPack = new PieceSoundPack();
                        AddPlayerCommonSounds(soundPack: soundPack, female: false);

                        return new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerBoy, allowedTerrain: CreatePlayerAllowedTerrain(), minDistance: 0, maxDistance: 65535, generation: generation, readableName: "boy", description: "This is you.", yield: CreatePlayerYield(), activeState: BoardPiece.State.PlayerControlledWalking, lightEngine: new LightEngine(size: 300, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true), soundPack: soundPack, strength: 2, speed: 3.5f, maxStamina: 400, maxHitPoints: 600, maxFatigue: 3000, craftLevel: 1, cookLevel: 1, brewLevel: 1, invWidth: 4, invHeight: 2, toolbarWidth: 3, toolbarHeight: 1, fireAffinity: 0.5f);
                    }

                case Name.PlayerGirl:
                    {
                        var soundPack = new PieceSoundPack();
                        AddPlayerCommonSounds(soundPack: soundPack, female: true);

                        return new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerGirl, allowedTerrain: CreatePlayerAllowedTerrain(), minDistance: 0, maxDistance: 65535, generation: generation, readableName: "girl", description: "This is you.", yield: CreatePlayerYield(), activeState: BoardPiece.State.PlayerControlledWalking, lightEngine: new LightEngine(size: 300, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true), soundPack: soundPack, strength: 1, speed: 3, maxStamina: 300, maxHitPoints: 400, maxFatigue: 2000, craftLevel: 2, cookLevel: 2, brewLevel: 1, invWidth: 4, invHeight: 3, toolbarWidth: 4, toolbarHeight: 1, fireAffinity: 0.5f);
                    }

                case Name.PlayerTestDemoness:
                    {
                        var soundPack = new PieceSoundPack();
                        AddPlayerCommonSounds(soundPack: soundPack, female: true);

                        Player testDemoness = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerTestDemoness, allowedTerrain: CreatePlayerAllowedTerrain(), minDistance: 0, maxDistance: 65535, generation: generation, readableName: "demoness", description: "This is you.", yield: CreatePlayerYield(), activeState: BoardPiece.State.PlayerControlledWalking, lightEngine: new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.Red * 1f, isActive: false, castShadows: true), soundPack: soundPack, strength: 100, speed: 8, maxStamina: 50000, maxHitPoints: 100000, maxFatigue: 50000, craftLevel: 5, cookLevel: 5, brewLevel: 5, invWidth: 6, invHeight: 4, toolbarWidth: 5, toolbarHeight: 1, fireAffinity: 0f);

                        return testDemoness;
                    }

                case Name.PlayerGhost:
                    {
                        var soundPack = new PieceSoundPack();

                        foreach (PieceSoundPack.Action action in new List<PieceSoundPack.Action> { PieceSoundPack.Action.StepGrass, PieceSoundPack.Action.StepWater, PieceSoundPack.Action.StepSand, PieceSoundPack.Action.StepRock, PieceSoundPack.Action.SwimShallow, PieceSoundPack.Action.SwimDeep, PieceSoundPack.Action.StepLava, PieceSoundPack.Action.StepMud })
                        {
                            soundPack.AddAction(action: action, sound: new Sound(name: SoundData.Name.StepGhost, cooldown: 30, ignore3DAlways: true, volume: 0.8f, maxPitchVariation: 0.2f));
                        }

                        Player spectator = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, allowedTerrain: new AllowedTerrain(), minDistance: 0, maxDistance: 65535, generation: generation, readableName: "player ghost", description: "A metaphysical representation of player's soul.", blocksMovement: false, ignoresCollisions: true, floatsOnWater: true, activeState: BoardPiece.State.PlayerControlledGhosting, lightEngine: new LightEngine(size: 650, opacity: 1.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true), soundPack: soundPack, strength: 2, speed: 3.5f, maxStamina: 400, maxHitPoints: 400, maxFatigue: 2000, craftLevel: 1, invWidth: 1, invHeight: 1, toolbarWidth: 1, toolbarHeight: 1, fireAffinity: 0f);

                        spectator.speed = 5;
                        spectator.sprite.opacity = 0.5f;
                        spectator.sprite.color = new Color(150, 255, 255);
                        spectator.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.SkyBlue * 0.7f, textureSize: spectator.sprite.frame.textureSize, priority: 0, framesLeft: -1));

                        return spectator;
                    }

                case Name.GrassRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 50, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsGreen, chanceToDrop: 3, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };

                        var maxMassForSize = new int[] { 150 };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 35, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 600, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.53f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A regular grass.", allowedDensity: new AllowedDensity(radious: 60, maxNoOfPiecesSameName: 17), yield: yield, fireAffinity: 0.3f);
                    }

                case Name.GrassGlow:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 50, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsGreen, chanceToDrop: 100, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        var maxMassForSize = new int[] { 150 };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        // readableName is the same as "regular grass", to make it appear identical to the regular grass

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 1000, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.49f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A special type of grass.", allowedDensity: new AllowedDensity(radious: 350, maxNoOfPiecesSameName: 1), yield: yield, lightEngine: new LightEngine(size: 0, opacity: 0.3f, colorActive: true, color: Color.Blue * 3f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: true, castShadows: false), maxExistingNumber: 300, fireAffinity: 0.3f);
                    }

                case Name.GrassDesert:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 115) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsRed, chanceToDrop: 3, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 40 } };
                        var maxMassForSize = new int[] { 250 };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.GrassDesert, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 60, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 900, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.63f, generation: generation, staysAfterDeath: 300, readableName: "desert grass", description: "A grass, that grows on sand.", allowedDensity: new AllowedDensity(radious: 75, maxNoOfPiecesTotal: 0), yield: yield, fireAffinity: 0.8f);
                    }

                case Name.PlantPoison:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                                { Terrain.Name.Biome, new AllowedRange(min: (byte)(Terrain.biomeMin + (255 - Terrain.biomeMin) / 2), max:255 ) },
                                { Terrain.Name.Height, new AllowedRange(min: 112, max:156 ) },
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsBlack, chanceToDrop: 20, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Biome, 245 } };
                        var maxMassForSize = new int[] { 800 };
                        var reproduction = new PlantReproductionData(massNeeded: 1000, massLost: 190, bioWear: 0.34f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.PlantPoison, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 30, maxDistance: 90, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 950, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.63f, generation: generation, staysAfterDeath: 300, readableName: "poisonous plant", description: "Poisonous plant.", allowedDensity: new AllowedDensity(radious: 70, maxNoOfPiecesTotal: 4), yield: yield, fireAffinity: 0.3f, dropSeedChance: 70);
                    }

                case Name.Rushes:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 77, max: 95) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 128, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsCyan, chanceToDrop: 1, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 220 }, { Terrain.Name.Height, 92 } };
                        var maxMassForSize = new int[] { 400 };
                        var reproduction = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.Rushes, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant, minDistance: 0, maxDistance: 120, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 600, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 0.62f, generation: generation, staysAfterDeath: 300, readableName: "rushes", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 120, maxNoOfPiecesTotal: 40), yield: yield, fireAffinity: 0.2f);
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WaterLily1, AnimData.PkgName.WaterLily2, AnimData.PkgName.WaterLily3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>{
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 12), max: (byte)(Terrain.waterLevelMax - 1)) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 140) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) }
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsBlue, chanceToDrop: 10, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 80 }, { Terrain.Name.Height, 45 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 25, maxDistance: 80, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: null, maxAge: 1800, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.4f, generation: generation, staysAfterDeath: 300, floatsOnWater: true, readableName: "water lily", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesSameName: 3), yield: yield, maxHitPoints: 10, fireAffinity: 0f);
                    }

                case Name.FlowersPlain:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.FlowersYellow1, AnimData.PkgName.FlowersWhite };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 140, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsYellow, chanceToDrop: 3, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        var maxMassForSize = new int[] { 400 };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "regular flower", description: "A flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), yield: yield, fireAffinity: 0.6f, dropSeedChance: 20);
                    }

                case Name.FlowersRed:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 140, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsRed, chanceToDrop: 20, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        var maxMassForSize = new int[] { 400 };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.FlowersRed, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "red flower", description: "A red flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), lightEngine: new LightEngine(size: 0, opacity: 0.2f, colorActive: true, color: Color.Red * 1.5f, addedGfxRectMultiplier: 3f, isActive: true, glowOnlyAtNight: true, castShadows: false), yield: yield, fireAffinity: 0.6f, dropSeedChance: 20);
                    }

                case Name.FlowersMountain:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min:160, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) }
                        });

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsYellow, chanceToDrop: 40, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Height, 175 } };
                        var maxMassForSize = new int[] { 500 };
                        var reproduction = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.FlowersYellow2, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 250, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 4000, reproduction: reproduction, massToBurn: 3, massTakenMultiplier: 0.98f, generation: generation, staysAfterDeath: 300, readableName: "mountain flower", description: "A mountain flower.", allowedDensity: new AllowedDensity(radious: 240, maxNoOfPiecesSameName: 0), yield: yield, fireAffinity: 0.6f, dropSeedChance: 15);
                    }

                case Name.TreeSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.TreeSmall1, AnimData.PkgName.TreeSmall2 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 80, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) }
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        var maxMassForSize = new int[] { 1000, 2500 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.3f);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 50, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 30000, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 1.35f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 50, readableName: "small tree", description: "A small tree.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 1), soundPack: soundPack, adultSizeMass: maxMassForSize[1], fireAffinity: 0.65f);
                    }

                case Name.TreeBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassForSize = new int[] { 1000, 2500 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 100, readableName: "big tree", description: "A big tree.", allowedDensity: new AllowedDensity(radious: 360, maxNoOfPiecesSameName: 1), soundPack: soundPack, adultSizeMass: maxMassForSize[1], fireAffinity: 0.65f);
                    }

                case Name.Oak:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassForSize = new int[] { 1000, 2500 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Acorn);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        // adultSizeMass should be greater than animSize for sapling (to avoid showing fruits on sapling)

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "oak", description: "Acorns can grow on it.", allowedDensity: new AllowedDensity(radious: 360, maxNoOfPiecesSameName: 1), soundPack: soundPack, adultSizeMass: maxMassForSize[1], fireAffinity: 0.65f, dropSeedChance: 20);
                    }

                case Name.AppleTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassForSize = new int[] { 1000, 2500 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Apple);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        // adultSizeMass should be greater than animSize for sapling (to avoid showing fruits on sapling)

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "apple tree", description: "Apples can grow on it.", allowedDensity: new AllowedDensity(radious: 360, maxNoOfPiecesSameName: 1), soundPack: soundPack, adultSizeMass: maxMassForSize[1], fireAffinity: 0.65f);
                    }

                case Name.CherryTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassForSize = new int[] { 1000, 2500 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 120f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Cherry);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.TreeStump, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        // adultSizeMass should be greater than animSize for sapling (to avoid showing fruits on sapling)

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "cherry tree", description: "Cherries can grow on it.", allowedDensity: new AllowedDensity(radious: 360, maxNoOfPiecesSameName: 1), soundPack: soundPack, adultSizeMass: maxMassForSize[1], fireAffinity: 0.65f);
                    }

                case Name.BananaTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        var maxMassForSize = new int[] { 2500, 8000, 10000 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 250f, yOffsetPercent: -0.29f, areaWidthPercent: 0.85f, areaHeightPercent: 0.3f, fruitName: Name.Banana);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 80, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.PalmTree, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 200, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 160, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "banana tree", description: "Bananas can grow on it.", allowedDensity: new AllowedDensity(radious: 400, maxNoOfPiecesSameName: 2), soundPack: soundPack, adultSizeMass: maxMassForSize[0], fireAffinity: 0.65f);
                    }

                case Name.PalmTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        var maxMassForSize = new int[] { 2500, 8000, 10000 };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 80, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodLogHard, chanceToDrop: 40, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3)});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyTree, maxPitchVariation: 1f));

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.PalmTree, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 200, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 15000, yield: yield, maxHitPoints: 160, readableName: "palm tree", description: "A palm tree.", allowedDensity: new AllowedDensity(radious: 400, maxNoOfPiecesSameName: 2), soundPack: soundPack, adultSizeMass: maxMassForSize[0], fireAffinity: 0.65f);
                    }

                case Name.TomatoPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 200) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        var maxMassForSize = new int[] { 450, 900 };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 50f, yOffsetPercent: -0.05f, areaWidthPercent: 0.85f, areaHeightPercent: 0.8f, fruitName: Name.Tomato);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.TomatoPlant, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                           maxHitPoints: 40, minDistance: 20, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 1000, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 0.855f, generation: generation, staysAfterDeath: 500, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "tomato plant", description: "Tomatoes can grow on it.", allowedDensity: new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 2), yield: yield, fireAffinity: 0.4f);
                    }

                case Name.CoffeeShrub:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 80, max: 140) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 250) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        var maxMassForSize = new int[] { 600 };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 2, oneFruitMass: 50f, yOffsetPercent: -0.05f, areaWidthPercent: 0.85f, areaHeightPercent: 0.8f, fruitName: Name.CoffeeRaw);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.CoffeeShrub, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                           maxHitPoints: 40, minDistance: 20, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 1000, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 0.855f, generation: generation, staysAfterDeath: 500, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "coffee shrub", description: "Coffee can grow on it.", allowedDensity: new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 2), yield: yield, fireAffinity: 0.4f);
                    }

                case Name.CarrotPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 200) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 150 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 1, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Carrot, hiddenFruits: true);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.CarrotPlant, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                           maxHitPoints: 40, minDistance: 20, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: null, maxAge: 1000, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 0.855f, generation: generation, staysAfterDeath: 500, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "carrot plant", description: "Carrots can grow on it.", allowedDensity: new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 2), yield: yield, fireAffinity: 0.4f, dropSeedChance: 8);
                    }

                case Name.Cactus:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) }
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsViolet, chanceToDrop: 40, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 60 } };
                        var maxMassForSize = new int[] { 10000 };
                        var reproduction = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.Cactus, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 50, maxDistance: 600, bestEnvironment: bestEnvironment, mass: 1, maxMassForSize: maxMassForSize, maxAge: 30000, reproduction: reproduction, massToBurn: 10, maxHitPoints: 80, massTakenMultiplier: 1.65f, generation: generation, staysAfterDeath: 10000, readableName: "cactus", description: "A desert plant.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 1), yield: yield, fireAffinity: 0.3f, dropSeedChance: 50);
                    }

                case Name.MineralsSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsSmall1, AnimData.PkgName.MineralsSmall2, AnimData.PkgName.MineralsSmall3, AnimData.PkgName.MineralsMossySmall4 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 140, max: 180) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 25, maxNumberToDrop: 1)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.", movesWhenDropped: false, isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.MineralsMossySmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsMossySmall1, AnimData.PkgName.MineralsMossySmall2, AnimData.PkgName.MineralsMossySmall3, AnimData.PkgName.MineralsMossySmall4 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 140, max: 180) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 129, max: 255) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 25, maxNumberToDrop: 1)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.", movesWhenDropped: false, isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsBig1, AnimData.PkgName.MineralsBig2, AnimData.PkgName.MineralsBig3, AnimData.PkgName.MineralsBig4, AnimData.PkgName.MineralsBig5, AnimData.PkgName.MineralsBig6, AnimData.PkgName.MineralsBig7, AnimData.PkgName.MineralsBig8, AnimData.PkgName.MineralsBig9, AnimData.PkgName.MineralsBig10, AnimData.PkgName.MineralsBig11, AnimData.PkgName.MineralsBig12, AnimData.PkgName.MineralsBig13, AnimData.PkgName.MineralsBig14 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 150, max: 190) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 25, maxNumberToDrop: 2)
                       });

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.", movesWhenDropped: false, allowedDensity: new AllowedDensity(radious: 130, maxNoOfPiecesSameName: 0), isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.MineralsMossyBig:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsMossyBig1, AnimData.PkgName.MineralsMossyBig2, AnimData.PkgName.MineralsMossyBig3, AnimData.PkgName.MineralsMossyBig4, AnimData.PkgName.MineralsMossyBig5, AnimData.PkgName.MineralsMossyBig6, AnimData.PkgName.MineralsMossyBig7, AnimData.PkgName.MineralsMossyBig8, AnimData.PkgName.MineralsMossyBig9, AnimData.PkgName.MineralsMossyBig10, AnimData.PkgName.MineralsMossyBig11, AnimData.PkgName.MineralsMossyBig12 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 150, max: 190) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 129, max: 255) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.MineralsMossySmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.MineralsMossySmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 25, maxNumberToDrop: 2)
                       });

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.", movesWhenDropped: false, allowedDensity: new AllowedDensity(radious: 130, maxNoOfPiecesSameName: 0), isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.Backlight:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Backlight, minDistance: 0, maxDistance: 0, destructionDelay: -1, allowedTerrain: allowedTerrain, generation: generation, readableName: "backlight", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.BloodSplatter:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.BloodSplatter1, AnimData.PkgName.BloodSplatter2, AnimData.PkgName.BloodSplatter3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, destructionDelay: 250, allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 10, generation: generation, readableName: "bloodSplatter", description: "A pool of blood.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.Attack:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Attack, minDistance: 0, maxDistance: 3, destructionDelay: -1, allowedTerrain: allowedTerrain, generation: generation, readableName: "attack", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.MapMarker:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MapMarker, minDistance: 0, maxDistance: 0, allowedTerrain: allowedTerrain, generation: generation, readableName: "map marker", description: "Map marker.", activeState: BoardPiece.State.MapMarkerShowAndCheck, visible: false, serialize: false, fireAffinity: 0f);
                    }

                case Name.Empty:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, minDistance: 0, maxDistance: 3, destructionDelay: 0, allowedTerrain: allowedTerrain, generation: generation, readableName: "empty", description: "Should not be used.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.Miss:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Miss, minDistance: 0, maxDistance: 3, destructionDelay: 60, allowedTerrain: allowedTerrain, generation: generation, readableName: "miss", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.Zzz:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Zzz, minDistance: 0, maxDistance: 0, destructionDelay: 0, allowedTerrain: allowedTerrain, generation: generation, serialize: false, readableName: "zzz", description: "A visual effect.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);

                        return visualEffect;
                    }

                case Name.Heart:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Heart, destructionDelay: 40, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, readableName: "heart", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.MusicNote:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MusicNoteSmall, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, readableName: "music note", description: "Sound visual.", activeState: BoardPiece.State.Empty, serialize: false, fireAffinity: 0f);
                    }

                case Name.Crosshair:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crosshair, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "crosshair", description: "A visual effect.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);
                    }

                case Name.BubbleExclamationRed:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleExclamationRed, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "red exclamation", description: "A visual effect.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);

                        return visualEffect;
                    }

                case Name.BubbleExclamationBlue:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleExclamationBlue, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "blue exclamation", description: "A visual effect.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);

                        return visualEffect;
                    }

                case Name.BubbleCraftGreen:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleCraftGreen, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "green exclamation with plus", description: "A visual effect.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);

                        return visualEffect;
                    }

                case Name.CookingTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "cooking starter", description: "Starts cooking.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);
                    }

                case Name.BrewTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "brewing starter", description: "Starts brewing.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);
                    }

                case Name.UpgradeTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Upgrade, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "upgrade", description: "Upgrades item.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);
                    }

                case Name.FireplaceTriggerOn:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "fireplace on", description: "Ignites the fireplace.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);
                    }

                case Name.FireplaceTriggerOff:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WaterDrop, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "fireplace off", description: "Extinginguishes fire.", activeState: BoardPiece.State.Empty, fireAffinity: 0f);
                    }

                case Name.CrateStarting:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeStone, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crate, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 5, readableName: "supply crate", description: "Contains valuable items.", soundPack: soundPack, isAffectedByWind: true, fireAffinity: 0.7f);
                    }

                case Name.CrateRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeStone, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeStone, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: Name.EmptyBottle, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.BackpackSmall, chanceToDrop: 15, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.BeltSmall, chanceToDrop: 15, maxNumberToDrop: 1),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crate, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 40, readableName: "supply crate", description: "Contains valuable items.", soundPack: soundPack, isAffectedByWind: true, fireAffinity: 0.7f);
                    }

                case Name.ChestWooden:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        byte storageWidth = 3;
                        byte storageHeight = 2;

                        return new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestWooden, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 40, readableName: "wooden chest", description: $"Can store items ({storageWidth}x{storageHeight}).", soundPack: soundPack, fireAffinity: 1.0f);
                    }

                case Name.ChestStone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        byte storageWidth = 4;
                        byte storageHeight = 4;

                        return new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestStone, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 50, readableName: "stone chest", description: $"Can store items ({storageWidth}x{storageHeight}).", soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.ChestIron:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsHit, sound: new Sound(name: SoundData.Name.HitWood, maxPitchVariation: 0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        byte storageWidth = 6;
                        byte storageHeight = 4;

                        return new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestIron, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 60, readableName: "iron chest", description: $"Can store items ({storageWidth}x{storageHeight}).", soundPack: soundPack, fireAffinity: 0.0f);
                    }

                case Name.ChestCrystal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsHit, sound: new Sound(name: SoundData.Name.HitWood, maxPitchVariation: 0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        return new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestCrystal, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Crystal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: 1, storageHeight: 1, maxHitPoints: 300, readableName: "crystal chest", description: "All crystal chests share their contents.", soundPack: soundPack, fireAffinity: 0.0f, pieceStorageIsGlobal: true);
                    }

                case Name.ChestTreasureNormal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronPlate, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodPlank, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.IronNail, chanceToDrop: 100, maxNumberToDrop: 5),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.HasAppeared, sound: new Sound(name: SoundData.Name.Chime));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        var treasureChest = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureBlue, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: 2, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", yield: yield, appearDebris: new Yield(debrisType: Yield.DebrisType.Star), animName: "closed", soundPack: soundPack, fireAffinity: 0.0f);

                        // this yield is used to randomize chest contents every time
                        var chestContentsYield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.SpearIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ScytheIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ShovelIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.IronBar, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.BowAdvanced, chanceToDrop: 15, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PotionCoffee, chanceToDrop: 1, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 10, maxNumberToDrop: 1),
                            });

                        while (true)
                        {
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces();

                            if (chestContents.Count >= 2)
                            {
                                foreach (BoardPiece piece in chestContents)
                                {
                                    treasureChest.PieceStorage.AddPiece(piece);
                                    if (treasureChest.PieceStorage.EmptySlotsCount == 0) break;
                                }
                                break;
                            }
                        }

                        return treasureChest;
                    }

                case Name.ChestTreasureBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronPlate, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.WoodPlank, chanceToDrop: 100, maxNumberToDrop: 4),
                                new Yield.DroppedPiece(pieceName: Name.IronNail, chanceToDrop: 100, maxNumberToDrop: 10),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.HasAppeared, sound: new Sound(name: SoundData.Name.Chime));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        var treasureChest = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureRed, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: 3, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", yield: yield, appearDebris: new Yield(debrisType: Yield.DebrisType.Star), animName: "closed", soundPack: soundPack, fireAffinity: 0.0f);

                        // this yield is used to randomize chest contents every time
                        var chestContentsYield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.SpearIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ScytheIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ShovelIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.IronBar, chanceToDrop: 10, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: Name.PotionCoffee, chanceToDrop: 4, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 10, maxNumberToDrop: 3),
                            });

                        while (true)
                        {
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces();

                            if (chestContents.Count >= 4)
                            {
                                foreach (BoardPiece piece in chestContents)
                                {
                                    treasureChest.PieceStorage.AddPiece(piece);
                                    if (treasureChest.PieceStorage.EmptySlotsCount == 0) break;
                                }
                                break;
                            }
                        }

                        return treasureChest;
                    }

                case Name.JarTreasure:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        var yield = new Yield(debrisType: Yield.DebrisType.Ceramic,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.JarBroken, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                new Yield.DroppedPiece(pieceName: Name.MeatDried, chanceToDrop: 50, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: Name.BowBasic, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.BowAdvanced, chanceToDrop: 2, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.AxeStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ScytheStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ShovelStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.SpearStone, chanceToDrop: 10, maxNumberToDrop: 1),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.HasAppeared, sound: new Sound(name: SoundData.Name.Ding1));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsHit, sound: new Sound(name: SoundData.Name.HitCeramic, maxPitchVariation: 0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyCeramic3, maxPitchVariation: 0.5f));

                        Decoration decoration = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarWhole, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 40, readableName: "sealed jar", description: "Contains supplies.", movesWhenDropped: false, soundPack: soundPack, isAffectedByWind: true, fireAffinity: 0.4f);

                        return decoration;
                    }

                case Name.JarBroken:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        var yield = new Yield(debrisType: Yield.DebrisType.Ceramic,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.Clay, chanceToDrop: 20, maxNumberToDrop: 1) });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsHit, sound: new Sound(name: SoundData.Name.HitCeramic, maxPitchVariation: 0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DestroyCeramic1, SoundData.Name.DestroyCeramic2 }, maxPitchVariation: 0.5f));

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarBroken, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                    minDistance: 0, maxDistance: 50, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 20, readableName: "broken jar", description: "Broken Jar.", movesWhenDropped: false, soundPack: soundPack, isAffectedByWind: true, fireAffinity: 0.5f);
                    }

                case Name.WorkshopEssential:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopEssential, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftEssential, maxHitPoints: 30, readableName: "essential workshop", description: "Essential crafting workshop.", canBeUsedDuringRain: true, fireAffinity: 0.8f);
                    }

                case Name.WorkshopBasic:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopBasic, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftBasic, maxHitPoints: 30, readableName: "basic workshop", description: "Basic crafting workshop.", canBeUsedDuringRain: true, fireAffinity: 0.8f);
                    }

                case Name.WorkshopAdvanced:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopAdvanced, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAdvanced, maxHitPoints: 80, readableName: "advanced workshop", description: "Advanced crafting workshop.", canBeUsedDuringRain: true, fireAffinity: 0.8f);
                    }

                case Name.WorkshopMaster:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopMaster, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftMaster, maxHitPoints: 80, readableName: "master workshop", description: "Master's crafting workshop.", canBeUsedDuringRain: true, fireAffinity: 0.8f);
                    }

                case Name.WorkshopLeatherBasic:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherBasic, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftLeatherBasic, maxHitPoints: 30, readableName: "basic leather workshop", description: "For making basic items out of leather.", canBeUsedDuringRain: true, fireAffinity: 0.8f);
                    }

                case Name.WorkshopLeatherAdvanced:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherAdvanced, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftLeatherAdvanced, maxHitPoints: 30, readableName: "advanced leather workshop", description: "For making advanced items out of leather.", canBeUsedDuringRain: true, fireAffinity: 0.8f);
                    }

                case Name.Furnace:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Furnace, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftFurnace, maxHitPoints: 40, readableName: "furnace", description: "For ore smelting.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), canBeUsedDuringRain: false, fireAffinity: 0f);
                    }

                case Name.Anvil:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Anvil, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAnvil, maxHitPoints: 80, readableName: "anvil", description: "For metal forming.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 1f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), canBeUsedDuringRain: true, fireAffinity: 0f);
                    }

                case Name.HotPlate:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.FryingPan, isLooped: true));
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.StoneMove1, ignore3DAlways: true));

                        var hotPlate = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HotPlate, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, maxHitPoints: 20, foodMassMultiplier: 1.1f, readableName: "hot plate", description: "For cooking.", ingredientSpace: 1, soundPack: soundPack, canBeUsedDuringRain: false, fireAffinity: 0f);

                        hotPlate.sprite.AssignNewName("off");
                        return hotPlate;
                    }

                case Name.CookingPot:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Cooking, isLooped: true));
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.PotLid, ignore3DAlways: true));

                        var cookingPot = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CookingPot, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, maxHitPoints: 30, foodMassMultiplier: 1.4f, readableName: "cooking pot", description: "For cooking. Can be used during rain.", ingredientSpace: 3, soundPack: soundPack, canBeUsedDuringRain: true, fireAffinity: 0f);

                        cookingPot.sprite.AssignNewName("off");
                        return cookingPot;
                    }

                case Name.AlchemyLabStandard:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.BoilingPotionLoop, isLooped: true));
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.BoilingPotion, ignore3DAlways: true));

                        var alchemyLab = new AlchemyLab(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AlchemyLabStandard, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, maxHitPoints: 30, readableName: "alchemy lab", description: "For potion brewing.", boosterSpace: 1, soundPack: soundPack, fireAffinity: 0.3f);

                        alchemyLab.sprite.AssignNewName("off");
                        return alchemyLab;
                    }

                case Name.AlchemyLabAdvanced:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.BoilingPotionLoop, isLooped: true));
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.BoilingPotion, ignore3DAlways: true));

                        var alchemyLab = new AlchemyLab(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AlchemyLabAdvanced, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, maxHitPoints: 30, readableName: "advanced alchemy lab", description: "For advanced potion brewing.", boosterSpace: 3, soundPack: soundPack, fireAffinity: 0.3f);

                        alchemyLab.sprite.AssignNewName("off");
                        return alchemyLab;
                    }

                case Name.UpgradeBench:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.ToolsMove, ignore3DAlways: true));

                        var combineWorkshop = new UpgradeBench(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.UpgradeBench, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, maxHitPoints: 30, readableName: "upgrade bench", description: "For upgrading items.", soundPack: soundPack, fireAffinity: 0.8f);

                        combineWorkshop.sprite.AssignNewName("off");
                        return combineWorkshop;
                    }

                case Name.Clam:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clam, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 200, generation: generation, mass: 100, stackSize: 6, rotatesWhenDropped: true, readableName: "clam", description: "Have to be cooked before eating.", fireAffinity: 0.3f);
                    }

                case Name.Stick:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Stick1, AnimData.PkgName.Stick2, AnimData.PkgName.Stick3, AnimData.PkgName.Stick4, AnimData.PkgName.Stick5, AnimData.PkgName.Stick6, AnimData.PkgName.Stick7 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropStick, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: animPkg, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stick", description: "Crafting material.", soundPack: soundPack, fireAffinity: 1.0f);
                    }

                case Name.Stone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Stone, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stone", description: "Crafting material.", fireAffinity: 0f);
                    }

                case Name.Granite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Granite, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "granite", description: "Crafting material.", fireAffinity: 0f);
                    }

                case Name.Clay:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropMud, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clay, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "clay", description: "Crafting material.", soundPack: soundPack, fireAffinity: 0.1f);
                    }

                case Name.Rope:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropRope, cooldown: 15, maxPitchVariation: 0.4f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Rope, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Flesh, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 6, floatsOnWater: false, rotatesWhenDropped: true, readableName: "rope", description: "Crafting material.", soundPack: soundPack, fireAffinity: 0.5f);
                    }

                case Name.WoodLogRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogRegular, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "regular wood log", description: "Crafting material.", fireAffinity: 1.0f);
                    }

                case Name.WoodLogHard:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogHard, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "hard wood log", description: "Crafting material.", fireAffinity: 0.9f);
                    }

                case Name.WoodPlank:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodPlank, blocksMovement: true, category: BoardPiece.Category.Wood,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, yield: null, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood plank", description: "Crafting material.", fireAffinity: 1.0f);
                    }

                case Name.IronNail:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Nail, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 50, floatsOnWater: false, yield: null, maxHitPoints: 1, rotatesWhenDropped: true, readableName: "nail", description: "Crafting material.", fireAffinity: 0f);
                    }

                case Name.BeachDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax + 5), (byte)(Terrain.waterLevelMax + 25)) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: Name.JarTreasure, chanceToDrop: 2, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Clay, chanceToDrop: 20, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Clam, chanceToDrop: 30, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 20, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Dirt, blocksMovement: false,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 20, readableName: "dig site", description: "May contain some buried items.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.ForestDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: Name.JarTreasure, chanceToDrop: 4, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Clay, chanceToDrop: 25, maxNumberToDrop: 2),
                                    new Yield.DroppedPiece(pieceName: Name.Acorn, chanceToDrop: 10, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 25, maxNumberToDrop: 1)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Dirt, blocksMovement: false,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.DesertDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax + 10), max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: Name.JarTreasure, chanceToDrop: 6, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 25, maxNumberToDrop: 3),
                                    new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 10, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Dirt, blocksMovement: false,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.GlassDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                                firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: Name.GlassSand, chanceToDrop: 100, maxNumberToDrop: 4),
                                    new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 30, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSiteGlass, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Dirt, blocksMovement: false,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.SwampDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                                 firstDroppedPieces: new List<Yield.DroppedPiece> { },
                                 finalDroppedPieces: new List<Yield.DroppedPiece> {
                                    new Yield.DroppedPiece(pieceName: Name.Hole, chanceToDrop: 100, maxNumberToDrop: 1), // must go first
                                    new Yield.DroppedPiece(pieceName: Name.ChestTreasureBig, chanceToDrop: 2, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.ChestTreasureNormal, chanceToDrop: 8, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.JarTreasure, chanceToDrop: 15, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.GlassSand, chanceToDrop: 20, maxNumberToDrop: 2),
                                    new Yield.DroppedPiece(pieceName: Name.Clay, chanceToDrop: 50, maxNumberToDrop: 1),
                                    new Yield.DroppedPiece(pieceName: Name.Acorn, chanceToDrop: 20, maxNumberToDrop: 1)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Dirt, blocksMovement: false,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 50, readableName: "dig site", description: "May contain some buried items.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.IronDeposit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 2)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 6)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronDeposit, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 300, readableName: "iron deposit", description: "Can be mined for iron.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.CrystalDepositBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 180, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Crystal,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 70, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 3)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Crystal,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 300, readableName: "big crystal deposit", description: "Can be mined for crystals.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.CrystalDepositSmall:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 180, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Crystal,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 3)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 8)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositSmall, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Crystal,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 150, readableName: "small crystal deposit", description: "Can be mined for crystals.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.CoalDeposit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 4)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 12)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoalDeposit, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassForSize: null, generation: generation, yield: yield, maxHitPoints: 300, readableName: "coal deposit", description: "Can be mined for coal.", isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.Coal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Coal, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "coal", description: "Crafting material and fuel.", soundPack: soundPack, fireAffinity: 0.6f);
                    }

                case Name.Crystal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropCrystal, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crystal, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Crystal, rotatesWhenDropped: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, readableName: "crystal", description: "Crafting material.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.IronOre:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronOre, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "iron ore", description: "Can be used to make iron bars.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.GlassSand:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlassSand, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "glass sand", description: "Can be used to make glass.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.IronBar:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropIronBar, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronBar, blocksMovement: true, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron bar", description: "Crafting material.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.IronRod:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropIronRod, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronRod, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 18, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron rod", description: "Crafting material.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.IronPlate:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropIronPlate, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronPlate, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 18, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron plate", description: "Crafting material.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.SeedsGeneric:
                    {
                        // stackSize should be 1, to avoid mixing different kinds of seeds together

                        return new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SeedBag, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, stackSize: 1,
                            minDistance: 0, maxDistance: 1000, generation: generation, mass: 5, readableName: "seeds", description: "Can be planted.", fireAffinity: 0.4f);
                    }

                case Name.Acorn:
                    {
                        BoardPiece acorn = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Acorn, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, stackSize: 6,
                            minDistance: 0, maxDistance: 1000, generation: generation, mass: 5, readableName: "acorn", description: "Can be planted or cooked.", fireAffinity: 0.6f);

                        ((Seed)acorn).PlantToGrow = Name.Oak; // every "non-generic" seed must have its PlantToGrow attribute set

                        return acorn;
                    }

                case Name.CoffeeRaw:
                    {
                        BoardPiece coffeeRaw = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeRaw, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, stackSize: 12,
                            minDistance: 0, maxDistance: 1000, generation: generation, mass: 5, readableName: "raw coffee", description: "Can be planted or roasted.", fireAffinity: 0.6f);

                        ((Seed)coffeeRaw).PlantToGrow = Name.CoffeeShrub; // every "non-generic" seed must have its PlantToGrow attribute set
                        return coffeeRaw;
                    }

                case Name.CoffeeRoasted:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Fatigue, value: (float)-800, isPermanent: true),
                             new Buff(type: BuffEngine.BuffType.MaxStamina, value: 80f, autoRemoveDelay: 60 * 60 * 1),
                        };

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeRoasted, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Indestructible, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "roasted coffee", description: "Potion ingredient.", fireAffinity: 0f, buffList: buffList);
                    }

                case Name.Apple:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Apple, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, mightContainSeeds: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 70, readableName: "apple", description: "Can be eaten or cooked.", fireAffinity: 0.15f);
                    }

                case Name.Cherry:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Cherry, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, mightContainSeeds: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 16, mass: 40, readableName: "cherry", description: "Can be eaten or cooked.", fireAffinity: 0.15f);
                    }

                case Name.Banana:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Banana, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, mightContainSeeds: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 80, readableName: "banana", description: "Can be eaten or cooked.", fireAffinity: 0.15f);
                    }

                case Name.Tomato:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Tomato, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, mightContainSeeds: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 20, readableName: "tomato", description: "Can be eaten or cooked.", fireAffinity: 0.15f);
                    }

                case Name.Carrot:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Carrot, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, mightContainSeeds: false,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 20, readableName: "carrot", description: "Can be eaten or cooked.", fireAffinity: 0.15f);
                    }

                case Name.HerbsGreen:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.MaxHP, value: 50f, autoRemoveDelay: 5 * 60 * 60)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsGreen, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 10, rotatesWhenDropped: true, readableName: "green herbs", description: "Potion ingredient.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.HerbsViolet:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Fatigue, value: -120f, isPermanent: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsViolet, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 10, rotatesWhenDropped: true, readableName: "violet herbs", description: "Potion ingredient.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.HerbsBlack:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-15, autoRemoveDelay: 60 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlack, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "black herbs", description: "Contain poison.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.HerbsBlue:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.MaxStamina, value: 100f, autoRemoveDelay: 60 * 60 * 3)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlue, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "blue herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.HerbsCyan:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Haste, value: (int)2, autoRemoveDelay: 60 * 30)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsCyan, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "cyan herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.HerbsRed:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsRed, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "red herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.HerbsYellow:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Strength, value: (int)2, autoRemoveDelay: 60 * 60 * 3)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsYellow, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "yellow herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.EmptyBottle:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.EmptyBottle, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, floatsOnWater: false, readableName: "empty bottle", description: "Can be used to make a potion.", soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.PotionCoffee:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionBrown, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "coffee", description: "Coffee-based drink.", buffList: null, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.PotionGeneric:
                    {
                        // A generic potion, which animPackage and buffs will be set later.

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionTransparent, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "potion", description: "A potion.", buffList: null, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.BottleOfOil:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionLightYellow, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, floatsOnWater: false, readableName: "bottle of oil", description: "Crafting material.", buffList: null, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack, fireAffinity: 1.0f);
                    }

                case Name.MeatRaw:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatRaw, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, buffList: buffList,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 6, mass: 100, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "raw meat", description: "Poisonous, but safe after cooking.", soundPack: soundPack, fireAffinity: 0.2f);
                    }

                case Name.MeatDried:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatDried, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 6, mass: 130, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "dried meat", description: "Can be eaten or cooked.", soundPack: soundPack, fireAffinity: 0.6f);
                    }

                case Name.Fat:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Fat, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 50, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, readableName: "fat", description: "Can be cooked or crafted.", soundPack: soundPack, fireAffinity: 0.6f);
                    }

                case Name.Leather:
                    {
                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Leather, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, mass: 100, rotatesWhenDropped: true, readableName: "leather", description: "Crafting material.", fireAffinity: 0.7f);
                    }

                case Name.Burger:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Burger, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 300, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, floatsOnWater: false, readableName: "hamburger", description: "Synthetic food, that will remain fresh forever. Can be eaten.", fireAffinity: 0.2f);
                    }

                case Name.Meal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MealStandard, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "cooked meal", description: "Can be eaten.", fireAffinity: 0.2f);
                    }

                case Name.Rabbit:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitBrown, AnimData.PkgName.RabbitDarkBrown, AnimData.PkgName.RabbitGray, AnimData.PkgName.RabbitBlack, AnimData.PkgName.RabbitLightBrown };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitLightGray, AnimData.PkgName.RabbitBeige, AnimData.PkgName.RabbitWhite };

                        var maleAnimPkgName = malePackageNames[random.Next(0, malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[random.Next(0, femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.GroundAll });
                        var maxMassForSize = new int[] { 200, 500 };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 70, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 50, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 50, maxNumberToDrop: 1) });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CrySmallAnimal1, SoundData.Name.CrySmallAnimal2, SoundData.Name.CrySmallAnimal3, SoundData.Name.CrySmallAnimal4 }, maxPitchVariation: 0.3f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatHerbivore1, SoundData.Name.EatHerbivore2, SoundData.Name.EatHerbivore3, SoundData.Name.EatHerbivore4, SoundData.Name.EatHerbivore5 }, maxPitchVariation: 0.25f, cooldown: 35));

                        return new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                            minDistance: 10, maxDistance: 45, maxHitPoints: 150, mass: 35, maxMass: 5000, massBurnedMultiplier: 1, awareness: 200, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 300, sightRange: 400, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.TomatoPlant, Name.Tomato, Name.Meal, Name.Carrot, Name.CarrotPlant }, strength: 30, maxMassForSize: maxMassForSize, generation: generation, yield: yield, readableName: "rabbit", description: "A small animal.", soundPack: soundPack, fireAffinity: 0.65f);
                    }

                case Name.Fox:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxGinger, AnimData.PkgName.FoxRed, AnimData.PkgName.FoxBlack, AnimData.PkgName.FoxChocolate, AnimData.PkgName.FoxBrown };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxWhite, AnimData.PkgName.FoxGray, AnimData.PkgName.FoxYellow };

                        var maleAnimPkgName = malePackageNames[random.Next(0, malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[random.Next(0, femalePackageNames.Count)];

                        var maxMassForSize = new int[] { 500, 1000 };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 70, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 80, maxNumberToDrop: 1) });

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.WaterShallow });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CrySmallAnimal1, SoundData.Name.CrySmallAnimal2, SoundData.Name.CrySmallAnimal3, SoundData.Name.CrySmallAnimal4 }, maxPitchVariation: 0.3f, pitchChange: -0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPredator1, SoundData.Name.EatPredator2, SoundData.Name.EatPredator3, SoundData.Name.EatPredator4 }, maxPitchVariation: 0.25f, cooldown: 60));

                        var eats = new List<Name> { Name.Rabbit, Name.MeatRaw, Name.Fat, Name.Burger, Name.MeatDried, Name.Meal };
                        eats.AddRange(PieceInfo.GetPlayerNames());

                        return new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 300, mass: 60, maxMass: 15000, awareness: 80, massBurnedMultiplier: 1.3f, matureAge: 2000, maxAge: 30000, pregnancyDuration: 4000, maxChildren: 6, maxStamina: 800, sightRange: 500, eats: eats, strength: 30, maxMassForSize: maxMassForSize, generation: generation, yield: yield, readableName: "fox", description: "An animal.", soundPack: soundPack, fireAffinity: 0.65f);
                    }

                case Name.Tiger:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.TigerOrangeMedium, AnimData.PkgName.TigerGray, AnimData.PkgName.TigerOrangeLight, AnimData.PkgName.TigerOrangeDark, AnimData.PkgName.TigerBrown, AnimData.PkgName.TigerBlack };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.TigerWhite, AnimData.PkgName.TigerYellow };

                        var maleAnimPkgName = malePackageNames[random.Next(0, malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[random.Next(0, femalePackageNames.Count)];

                        var maxMassForSize = new int[] { 500, 2000 };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 100, maxNumberToDrop: 3), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 100, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 100, maxNumberToDrop: 2) });

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 160, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(name: SoundData.Name.TigerRoar, maxPitchVariation: 0.3f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPredator1, SoundData.Name.EatPredator2, SoundData.Name.EatPredator3, SoundData.Name.EatPredator4 }, maxPitchVariation: 0.25f, cooldown: 60));

                        var eats = new List<Name> { Name.Rabbit, Name.MeatRaw, Name.Fat, Name.Burger, Name.MeatDried, Name.Fox, Name.Meal };
                        eats.AddRange(PieceInfo.GetPlayerNames());

                        return new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 2.4f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 1600, mass: 80, maxMass: 15000, awareness: 50, massBurnedMultiplier: 0.5f, matureAge: 4000, maxAge: 50000, pregnancyDuration: 3500, maxChildren: 5, maxStamina: 1300, sightRange: 700, eats: eats, strength: 140, maxMassForSize: maxMassForSize, generation: generation, yield: yield, readableName: "tiger", description: "Very dangerous animal.", soundPack: soundPack, fireAffinity: 0.3f);
                    }

                case Name.Frog:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog1, AnimData.PkgName.Frog3, AnimData.PkgName.Frog4, AnimData.PkgName.Frog5, AnimData.PkgName.Frog6, AnimData.PkgName.Frog7 };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog2, AnimData.PkgName.Frog8 };

                        var maleAnimPkgName = malePackageNames[random.Next(0, malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[random.Next(0, femalePackageNames.Count)];

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                        firstDroppedPieces: new List<Yield.DroppedPiece> { },
                        finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 40, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1), });

                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundSand }
                            );

                        var maxMassForSize = new int[] { 300, 800 };

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryFrog1, SoundData.Name.CryFrog2, SoundData.Name.CryFrog3, SoundData.Name.CryFrog4, }, maxPitchVariation: 0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatHerbivore1, SoundData.Name.EatHerbivore2, SoundData.Name.EatHerbivore3, SoundData.Name.EatHerbivore4, SoundData.Name.EatHerbivore5 }, maxPitchVariation: 0.25f, cooldown: 35));

                        return new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                       minDistance: 5, maxDistance: 30, maxHitPoints: 150, mass: 10, maxMass: 1200, massBurnedMultiplier: 1, awareness: 100, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 200, sightRange: 200, eats: new List<Name> { Name.WaterLily, Name.Rushes }, strength: 30, maxMassForSize: maxMassForSize, generation: generation, yield: yield, readableName: "frog", description: "A water animal.", soundPack: soundPack, fireAffinity: 0.15f);
                    }

                case Name.KnifeSimple:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Wood, 0.5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.KnifeSimple, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Indestructible,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: true, multiplierByCategory: multiplierByCategory, maxHitPoints: 1, readableName: "simple knife", description: "An old knife.", fireAffinity: 0f);
                    }

                case Name.AxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.Stone, 1f }, { BoardPiece.Category.Wood, 5f }, { BoardPiece.Category.Flesh, 3f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden axe", description: "Basic logging tool.", fireAffinity: 0.8f);
                    }

                case Name.AxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 2f }, { BoardPiece.Category.Wood, 8f }, { BoardPiece.Category.Flesh, 5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone axe", description: "Average logging tool.", fireAffinity: 0.4f);
                    }

                case Name.AxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 3f }, { BoardPiece.Category.Wood, 15 }, { BoardPiece.Category.Flesh, 8f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron axe", description: "Advanced logging tool.", fireAffinity: 0.2f);
                    }

                case Name.AxeCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 6f }, { BoardPiece.Category.Stone, 6f }, { BoardPiece.Category.Wood, 40 }, { BoardPiece.Category.Flesh, 10f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "crystal axe", description: "Deluxe logging tool.", fireAffinity: 0.1f);
                    }

                case Name.ShovelStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 1.5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 80, readableName: "stone shovel", description: "Basic shovel.", fireAffinity: 0.4f);
                    }

                case Name.ShovelIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "iron shovel", description: "Advanced shovel.", fireAffinity: 0.2f);
                    }

                case Name.ShovelCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 15f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Crystal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "crystal shovel", description: "Deluxe shovel.", fireAffinity: 0.1f);
                    }

                case Name.SpearWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 7f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 50, readableName: "wooden spear", description: "Essential melee weapon.", fireAffinity: 0.8f);
                    }

                case Name.SpearStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 8f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "stone spear", description: "Simple melee weapon.", fireAffinity: 0.4f);
                    }

                case Name.SpearIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 8f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 10, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "iron spear", description: "Advanced melee weapon.", fireAffinity: 0.2f);
                    }

                case Name.SpearCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 14f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 10, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 250, readableName: "crystal spear", description: "Deluxe melee weapon.", fireAffinity: 0.1f);
                    }

                case Name.PickaxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.SmallPlant, 0.0f }, { BoardPiece.Category.Stone, 5f }, { BoardPiece.Category.Wood, 1f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden pickaxe", description: "Basic mining tool.", fireAffinity: 0.8f);
                    }

                case Name.PickaxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 8f }, { BoardPiece.Category.Wood, 2f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone pickaxe", description: "Average mining tool.", fireAffinity: 0.4f);
                    }

                case Name.PickaxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 15f }, { BoardPiece.Category.Wood, 3f }, { BoardPiece.Category.Crystal, 3f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron pickaxe", description: "Advanced mining tool.", fireAffinity: 0.2f);
                    }

                case Name.PickaxeCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 6f }, { BoardPiece.Category.Stone, 30f }, { BoardPiece.Category.Wood, 6f }, { BoardPiece.Category.Crystal, 6f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "crystal pickaxe", description: "Deluxe mining tool.", fireAffinity: 0.1f);
                    }

                case Name.ScytheStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 2f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "stone scythe", description: "Can cut down small plants.", range: 20, fireAffinity: 0.4f);
                    }

                case Name.ScytheIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 3f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "iron scythe", description: "Can cut down small plants easily.", range: 40, fireAffinity: 0.2f);
                    }

                case Name.ScytheCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 6f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 250, readableName: "crystal scythe", description: "Brings an end to all small plants.", range: 80, fireAffinity: 0.1f);
                    }

                case Name.BowBasic:
                    {
                        int hitPower = 3;

                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, hitPower } }; // used only for hintWindow
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.ArrowWood, Name.ArrowStone, Name.ArrowIron, Name.ArrowCrystal, Name.ArrowBurning };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowBasic, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: hitPower, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "basic bow", description: "Projectile weapon.", fireAffinity: 0.8f);
                    }

                case Name.BowAdvanced:
                    {
                        int hitPower = 6;

                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, hitPower } }; // used only for hintWindow
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.ArrowWood, Name.ArrowStone, Name.ArrowIron, Name.ArrowCrystal, Name.ArrowBurning };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowAdvanced, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, hitPower: hitPower, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 300, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "advanced bow", description: "Projectile weapon.", fireAffinity: 0.3f);
                    }

                case Name.ArrowWood:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowWood, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, baseHitPower: 8, indestructible: false, maxHitPoints: 15, stackSize: 15, canBeStuck: true, readableName: "wooden arrow", description: "Very weak arrow.", fireAffinity: 1f);
                    }

                case Name.ArrowStone:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowStone, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, baseHitPower: 12, indestructible: false, maxHitPoints: 25, stackSize: 15, canBeStuck: true, readableName: "stone arrow", description: "Basic arrow.", fireAffinity: 0.8f);
                    }

                case Name.ArrowIron:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowIron, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, baseHitPower: 20, indestructible: false, maxHitPoints: 40, stackSize: 15, canBeStuck: true, readableName: "iron arrow", description: "Strong arrow.", fireAffinity: 0.7f);
                    }

                case Name.ArrowBurning:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowBurning, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, baseHitPower: 10, indestructible: false, maxHitPoints: 1, stackSize: 15, canBeStuck: true, readableName: "burning arrow", description: "Will start a fire.", fireAffinity: 0.7f, isBurning: true, lightEngine: new LightEngine(size: 100, opacity: 0.8f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true));
                    }

                case Name.ArrowCrystal:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowCrystal, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, baseHitPower: 40, indestructible: false, maxHitPoints: 50, stackSize: 15, canBeStuck: true, readableName: "crystal arrow", description: "Deluxe arrow. Deals serious damage.", fireAffinity: 0.4f);
                    }

                case Name.DebrisStone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisStone, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, readableName: "stone debris", description: "Floats around after hitting stone things.");
                    }

                case Name.DebrisPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisPlant, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, readableName: "plant debris", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisWood:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisWood, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, readableName: "wood debris", description: "Floats around after hitting wood things.");
                    }

                case Name.DebrisLeaf:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisLeaf1, AnimData.PkgName.DebrisLeaf2, AnimData.PkgName.DebrisLeaf3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "leaf", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisCrystal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisCrystal, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "crystal shard", description: "Floats around after hitting crystal things.");
                    }

                case Name.DebrisCeramic:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisCeramic1, AnimData.PkgName.DebrisCeramic2 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "ceramic debris", description: "Floats around after hitting ceramic things.");
                    }

                case Name.DebrisSoot:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisSoot1, AnimData.PkgName.DebrisSoot2, AnimData.PkgName.DebrisSoot3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "soot debris", description: "Remnants of burning things.");
                    }

                case Name.DebrisHeart:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisHeart1, AnimData.PkgName.DebrisHeart2, AnimData.PkgName.DebrisHeart3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        Debris heartDebris = new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 80, rotatesWhenDropped: true, readableName: "heart debris", description: "Explosion of love.");

                        heartDebris.sprite.opacity = 0.7f;

                        return heartDebris;
                    }

                case Name.DebrisStar:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisStar1, AnimData.PkgName.DebrisStar2, AnimData.PkgName.DebrisStar3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        Debris starDebris = new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 40, destructionDelayMaxOffset: 15, rotatesWhenDropped: true, readableName: "star debris", description: "Floats around after hitting stars. I guess...", isAffectedByWind: false);

                        var colors = new List<Color> { Color.White, Color.PaleGreen, Color.LightCyan, Color.Linen, Color.SeaShell, Color.LavenderBlush, Color.GhostWhite };
                        Color color = colors[random.Next(0, colors.Count)];
                        starDebris.sprite.color = color;

                        return starDebris;
                    }

                case Name.BloodDrop:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.BloodDrop1, AnimData.PkgName.BloodDrop2, AnimData.PkgName.BloodDrop3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassForSize: null, destructionDelay: 180, rotatesWhenDropped: false, readableName: "blood drop", description: "Floats around after hitting living things.", isAffectedByWind: false);
                    }

                case Name.TentSmall:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -50f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.MaxStamina, value: -50f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.56f, hitPointsChange: 0.05f, islandClockMultiplier: 3, canBeAttacked: false, waitingAfterSleepPossible: false, wakeUpBuffs: wakeUpBuffs);

                        return new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentSmall, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassForSize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "small tent", description: "Basic shelter for sleeping.\nProtects against enemies.", fireAffinity: 1.0f);
                    }

                case Name.TentMedium:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.3f, fatigueRegen: 0.8f, hitPointsChange: 0.1f, islandClockMultiplier: 4, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        return new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentMedium, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassForSize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "medium tent", description: "Average shelter for sleeping.\nProtects against enemies.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true), fireAffinity: 0.7f);
                    }

                case Name.TentBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.Strength, value: 1, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.5f, fatigueRegen: 1.3f, hitPointsChange: 0.25f, islandClockMultiplier: 4, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        return new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassForSize: null, maxHitPoints: 200, sleepEngine: sleepEngine, readableName: "big tent", description: "Luxurious shelter for sleeping.\nProtects against enemies.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true), fireAffinity: 0.6f);
                    }

                case Name.BackpackSmall:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)2),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackSmall, blocksMovement: true, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small backpack", description: "Expands inventory space.", fireAffinity: 0.5f);
                    }

                case Name.BackpackMedium:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)3),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackMedium, blocksMovement: true, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium backpack", description: "Expands inventory space.", fireAffinity: 0.5f);
                    }

                case Name.BackpackBig:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)4),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackBig, blocksMovement: true, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big backpack", description: "Expands inventory space.", fireAffinity: 0.5f);
                    }

                case Name.BeltSmall:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)1)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltSmall, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small belt", description: "Expands belt space.", fireAffinity: 0.5f);
                    }

                case Name.BeltMedium:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)3)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltMedium, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium belt", description: "Expands belt space.", fireAffinity: 0.5f);
                    }

                case Name.BeltBig:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)5)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltBig, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big belt", description: "Expands belt space.", fireAffinity: 0.5f);
                    }

                case Name.Map:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.EnableMap, value: null) };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Map, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Accessory,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "map", description: "Keeps track of visited places.", fireAffinity: 0.8f);
                    }

                case Name.HatSimple:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.HeatProtection, value: null)
                        };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HatSimple, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Head,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "hat", description: "Simple hat.", fireAffinity: 0.8f);
                    }

                case Name.BootsProtective:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.SwampProtection, value: null)
                        };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsProtective, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "protective boots", description: "Allow to walk safely over swamp area.", fireAffinity: 0.3f);
                    }

                case Name.Dungarees:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f),
                           new Buff(type: BuffEngine.BuffType.MaxStamina, value: 100f),
                        };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Dungarees, blocksMovement: false, category: BoardPiece.Category.Flesh, equipType: Equipment.EquipType.Chest,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "dungarees", description: "Dungarees.", fireAffinity: 0.5f);
                    }

                case Name.TorchSmall:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.LightSource, value: 4) };

                        return new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SmallTorch, blocksMovement: false, category: BoardPiece.Category.Wood, canBeUsedDuringRain: false,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 250, readableName: "small torch", description: "A portable light source.", fireAffinity: 1f);
                    }

                case Name.TorchBig:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.LightSource, value: 6) };

                        return new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BigTorch, blocksMovement: false, category: BoardPiece.Category.Wood, canBeUsedDuringRain: false,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 600, readableName: "big torch", description: "Burns for a long time.", fireAffinity: 1f);
                    }

                case Name.LanternFull:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.MetalicClank, cooldown: 15, maxPitchVariation: 0.3f));

                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.LightSource, value: 8) };

                        return new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Lantern, blocksMovement: true, category: BoardPiece.Category.Metal, canBeUsedDuringRain: true,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 500, readableName: "lantern", description: "Can be used during rain. Refillable.", convertsToWhenUsedUp: Name.LanternEmpty, soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.LanternEmpty:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.MetalicClank, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.LanternFrame, allowedTerrain: new AllowedTerrain(), category: BoardPiece.Category.Metal,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, maxHitPoints: 400, readableName: "empty lantern", description: "Needs a candle to be put inside.", movesWhenDropped: true, rotatesWhenDropped: true, isAffectedByWind: true, canBePickedUp: true, soundPack: soundPack, fireAffinity: 0f);
                    }

                case Name.Candle:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGeneric, cooldown: 15, maxPitchVariation: 0.4f));

                        Decoration decoration = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Candle, allowedTerrain: new AllowedTerrain(), category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, stackSize: 6, maxHitPoints: 100, readableName: "candle", description: "Can be put inside lantern.", movesWhenDropped: true, rotatesWhenDropped: true, isAffectedByWind: true, canBePickedUp: true, soundPack: soundPack, fireAffinity: 0.2f);

                        return decoration;
                    }

                case Name.Campfire:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        ushort range = 150;

                        var fireplace = new Fireplace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Campfire, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassForSize: null, generation: generation, storageWidth: 2, storageHeight: 2, maxHitPoints: 30, readableName: "campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        fireplace.sprite.AssignNewName("off");
                        return fireplace;
                    }

                case Name.PredatorRepellant:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SkullAndBones, minDistance: 0, maxDistance: 0, destructionDelay: 60 * 60 * 5, allowedTerrain: allowedTerrain, generation: generation, readableName: "predator repellent", description: "Scares predators and is invisible.", activeState: BoardPiece.State.ScareAnimalsAway, visible: false, serialize: false, fireAffinity: 0f);
                    }

                case Name.Hole:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hole, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, maxHitPoints: 1, readableName: "hole", description: "Empty dig site.", movesWhenDropped: false, destructionDelay: 60 * 30, isAffectedByWind: false, fireAffinity: 0f);
                    }

                case Name.TreeStump:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.WoodLogRegular, chanceToDrop: 20, maxNumberToDrop: 1) });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyStump));

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeStump, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, maxHitPoints: 50, readableName: "tree stump", description: "This was once a tree.", movesWhenDropped: false, yield: yield, soundPack: soundPack, isAffectedByWind: false, fireAffinity: 0.8f);
                    }

                case Name.HumanSkeleton:
                    {
                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HumanSkeleton, allowedTerrain: new AllowedTerrain(), category: BoardPiece.Category.Flesh,
                            minDistance: 0, maxDistance: 500, maxMassForSize: null, generation: generation, yield: null, maxHitPoints: 100, readableName: "skeleton", description: "Human remains.", movesWhenDropped: false, isAffectedByWind: false, fireAffinity: 0.3f);
                    }

                case Name.BurningFlame:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll });
                        var maxMassForSize = new int[] { 100, 250, 500, 750, 1000, 2000, 2500 };

                        Flame flame = new Flame(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 0, generation: generation, serialize: true, canBePickedUp: false, readableName: "flame", description: "A burning flame.", activeState: BoardPiece.State.FlameBurn, maxMassForSize: maxMassForSize);

                        return flame;
                    }

                case Name.LavaFlame:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        VisualEffect lavalight = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, minDistance: 0, maxDistance: 500, destructionDelay: 0, allowedTerrain: allowedTerrain, generation: generation, readableName: "lava flame", description: "Decorational flame on lava.", activeState: BoardPiece.State.Empty, visible: true, lightEngine: new LightEngine(size: 150, opacity: 0.3f, colorActive: true, color: Color.Orange * 0.6f, addedGfxRectMultiplier: 3f, isActive: true, glowOnlyAtNight: false, castShadows: true), ignoresCollisions: false, allowedDensity: new AllowedDensity(radious: 130, maxNoOfPiecesSameName: 0), serialize: false, fireAffinity: 0f);

                        lavalight.sprite.AssignNewSize((byte)BoardPiece.Random.Next(1, 4));

                        return lavalight;
                    }

                case Name.SwampGas:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4, AnimData.PkgName.Fog5, AnimData.PkgName.Fog6, AnimData.PkgName.Fog7, AnimData.PkgName.Fog8 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 400, maxNoOfPiecesSameName: 2);

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, destructionDelay: 0, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, minDistance: 0, maxDistance: 0, generation: generation, readableName: "gas", description: "Swamp gas.", activeState: BoardPiece.State.FogMoveRandomly, serialize: false, ignoresCollisions: false, visible: true, fireAffinity: 1f, fogExplodesWhenBurns: true);

                        visualEffect.sprite.color = Color.LimeGreen;
                        visualEffect.sprite.opacity = 0.4f;

                        return visualEffect;
                    }

                case Name.LavaGas:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4, AnimData.PkgName.Fog5, AnimData.PkgName.Fog6, AnimData.PkgName.Fog7, AnimData.PkgName.Fog8 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3);

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, destructionDelay: 0, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, minDistance: 0, maxDistance: 0, generation: generation, readableName: "gas", description: "Lava gas.", activeState: BoardPiece.State.FogMoveRandomly, serialize: false, ignoresCollisions: false, visible: true, fireAffinity: 1.0f);

                        visualEffect.sprite.color = new Color(255, 206, 28);
                        visualEffect.sprite.opacity = 0.45f;

                        return visualEffect;
                    }

                case Name.RainDrop:
                    {
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.RainDrops, destructionDelay: 0, allowedTerrain: new AllowedTerrain(), minDistance: 0, maxDistance: 0, generation: generation, readableName: "rain drop", description: "A single drop of rain.", activeState: BoardPiece.State.RainInitialize, serialize: false, ignoresCollisions: true, visible: true, fireAffinity: 0f);

                        visualEffect.sprite.opacity = 0.75f;

                        return visualEffect;
                    }

                case Name.Explosion:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.HasAppeared, sound: new Sound(name: SoundData.Name.ShootFire, maxPitchVariation: 0.6f));

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Explosion, destructionDelay: -1, allowedTerrain: new AllowedTerrain(), minDistance: 0, maxDistance: 0, generation: generation, readableName: "explosion", description: "An explosion.", activeState: BoardPiece.State.Empty, serialize: false, ignoresCollisions: true, visible: true, fireAffinity: 0f, lightEngine: new LightEngine(size: 150, opacity: 1f, colorActive: true, color: Color.Orange * 0.3f, isActive: true, castShadows: true), soundPack: soundPack);

                        return visualEffect;
                    }

                case Name.SoundSeaWaves:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, true } });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 1);

                        Sound sound = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.SeaWave1, SoundData.Name.SeaWave2, SoundData.Name.SeaWave3, SoundData.Name.SeaWave4, SoundData.Name.SeaWave5, SoundData.Name.SeaWave6, SoundData.Name.SeaWave7, SoundData.Name.SeaWave8, SoundData.Name.SeaWave9, SoundData.Name.SeaWave10, SoundData.Name.SeaWave11, SoundData.Name.SeaWave12, SoundData.Name.SeaWave13 }, maxPitchVariation: 0.8f, volume: 0.7f);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient sea waves sound", description: "Ambient sound for sea waves.", sound: sound, playDelay: 300, playDelayMaxVariation: 200, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Cyan;

                        return ambientSound;
                    }

                case Name.SoundSeaWind:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, true } });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 1);

                        Sound sound = new Sound(name: SoundData.Name.SeaWind, maxPitchVariation: 0.5f, volume: 0.6f, isLooped: true, volumeFadeFrames: 60);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient sea wind sound", description: "Ambient sound for sea wind.", sound: sound, playDelay: 0, generatesWind: true, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.White;

                        return ambientSound;
                    }

                case Name.SoundLakeWaves:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.NoBiome },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 200, maxNoOfPiecesSameClass: 0);

                        Sound sound = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.LakeWave1, SoundData.Name.LakeWave2, SoundData.Name.LakeWave3, SoundData.Name.LakeWave4, SoundData.Name.LakeWave5, SoundData.Name.LakeWave6, SoundData.Name.LakeWave7 }, maxPitchVariation: 0.8f, volume: 0.7f);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient lake waves sound", description: "Ambient sound for lake waves.", sound: sound, playDelay: 300, playDelayMaxVariation: 200, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Blue;

                        return ambientSound;
                    }

                case Name.SoundNightCrickets:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 200, maxNoOfPiecesSameClass: 0);

                        Sound sound = new Sound(name: SoundData.Name.NightCrickets, maxPitchVariation: 0.3f, volume: 0.2f, isLooped: true, volumeFadeFrames: 60);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient night crickets sound", description: "Ambient sound for crickets at night.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds, partOfDayList: new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Night });

                        ambientSound.sprite.color = Color.Black;

                        return ambientSound;
                    }

                case Name.SoundNoonCicadas:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 200, maxNoOfPiecesSameClass: 0);

                        Sound sound = new Sound(nameList: new List<SoundData.Name> { SoundData.Name.Cicadas1, SoundData.Name.Cicadas2, SoundData.Name.Cicadas3 }, maxPitchVariation: 0.3f, volume: 0.7f, isLooped: true, volumeFadeFrames: 60);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient noon cicadas sound", description: "Ambient sound for cicadas at noon.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds, partOfDayList: new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Noon }, playOnlyWhenIsSunny: true);

                        ambientSound.sprite.color = Color.Green;

                        return ambientSound;
                    }

                case Name.SoundLava:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 80, maxNoOfPiecesSameName: 1);

                        Sound sound = new Sound(name: SoundData.Name.Lava, maxPitchVariation: 0.5f, volume: 1f, isLooped: true, volumeFadeFrames: 60);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient lava sound", description: "Ambient sound for lava.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Yellow;

                        return ambientSound;
                    }

                default: { throw new ArgumentException($"Unsupported template name - {templateName}."); }
            }
        }

        private static void AddPlayerCommonSounds(PieceSoundPack soundPack, bool female)
        {
            soundPack.AddAction(action: PieceSoundPack.Action.PlayerBowDraw, sound: new Sound(name: SoundData.Name.BowDraw, maxPitchVariation: 0.15f, volume: 0.6f, ignore3DAlways: true));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerBowRelease, sound: new Sound(name: SoundData.Name.BowRelease, maxPitchVariation: 0.3f, ignore3DAlways: true));

            soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPlayer1, SoundData.Name.EatPlayer2, SoundData.Name.EatPlayer3, SoundData.Name.EatPlayer4 }, maxPitchVariation: 0.3f));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerSnore, sound: new Sound(name: female ? SoundData.Name.SnoringFemale : SoundData.Name.SnoringMale, maxPitchVariation: 0.3f, ignore3DAlways: true, isLooped: true, volume: 0.5f));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerPant, sound: new Sound(name: female ? SoundData.Name.PantFemale : SoundData.Name.PantMale, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 0.8f));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerYawn, sound: new Sound(name: female ? SoundData.Name.YawnFemale : SoundData.Name.YawnMale, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 1f));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerStomachGrowl, sound: new Sound(name: SoundData.Name.StomachGrowl, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 1f));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerSprint, sound: new Sound(name: SoundData.Name.Sprint, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 0.5f));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerSpeak, sound: new Sound(name: SoundData.Name.Beep, cooldown: 4, volume: 0.12f, pitchChange: female ? 0.5f : -0.5f, maxPitchVariation: 0.07f, ignore3DAlways: true));

            soundPack.AddAction(action: PieceSoundPack.Action.PlayerPulse, sound: new Sound(name: SoundData.Name.Pulse, isLooped: true, ignore3DAlways: true, volume: 0.7f));

            if (female)
            {
                soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryPlayerFemale1, SoundData.Name.CryPlayerFemale2, SoundData.Name.CryPlayerFemale3, SoundData.Name.CryPlayerFemale4 }, maxPitchVariation: 0.2f));
                soundPack.AddAction(action: PieceSoundPack.Action.Die, sound: new Sound(name: SoundData.Name.DeathPlayerFemale));
            }
            else
            {
                soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryPlayerMale1, SoundData.Name.CryPlayerMale2, SoundData.Name.CryPlayerMale3, SoundData.Name.CryPlayerMale4 }, maxPitchVariation: 0.2f));
                soundPack.AddAction(action: PieceSoundPack.Action.Die, sound: new Sound(name: SoundData.Name.DeathPlayerMale));
            }
        }

        private static Yield CreatePlayerYield()
        {
            return new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HumanSkeleton, chanceToDrop: 100, maxNumberToDrop: 1) });
        }

        private static AllowedTerrain CreatePlayerAllowedTerrain()
        {
            return new AllowedTerrain(
                rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.Volcano, AllowedTerrain.RangeName.NoBiome },
                extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });
        }
    }
}