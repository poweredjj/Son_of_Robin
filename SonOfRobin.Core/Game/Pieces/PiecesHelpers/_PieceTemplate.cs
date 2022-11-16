using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin

{
    public class PieceTemplate
    {
        public enum Name
        {
            Empty, // to be used instead of null

            Player,
            PlayerGhost,

            GrassRegular,
            GrassGlow,
            GrassDesert,
            Rushes,
            WaterLily,
            FlowersPlain,
            FlowersRed,
            FlowersMountain,
            Cactus,
            TreeSmall,
            TreeBig,
            PalmTree,

            Oak,
            AppleTree,
            CherryTree,
            BananaTree,
            TomatoPlant,

            Apple,
            Banana,
            Cherry,
            Tomato,
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

            WaterRock,
            MineralsSmall,
            MineralsBig,

            ChestWooden,
            ChestStone,
            ChestIron,
            ChestTreasureNormal,
            ChestTreasureBig,

            JarTreasure,
            JarBroken,
            CrateStarting,
            CrateRegular,

            WorkshopEssential,
            WorkshopBasic,
            WorkshopAdvanced,
            WorkshopMaster,

            WorkshopLeatherBasic,
            WorkshopLeatherAdvanced,

            WorkshopAlchemy,

            Furnace,
            Anvil,
            HotPlate,
            CookingPot,
            UpgradeBench,

            Stick,
            Stone,
            Granite,
            Clay,
            Rope,
            WoodLogRegular,
            WoodLogHard,
            WoodPlank,
            Nail,
            Shell,
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
            Exclamation,
            FlameRegular,
            CookingTrigger,
            UpgradeTrigger,
            FireplaceTriggerOn,
            FireplaceTriggerOff,

            Hand,
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

            BowWood,

            ArrowWood,
            ArrowStone,
            ArrowIron,
            ArrowCrystal,

            DebrisPlant,
            DebrisStone,
            DebrisWood,
            DebrisLeaf,
            DebrisCrystal,
            DebrisCeramic,
            DebrisStar,
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

            HatSimple,
            BootsProtective,

            TorchSmall,
            TorchBig,

            Campfire,

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
            PotionHealing,
            PotionMaxHPIncrease,
            PotionMaxHPDecrease,
            PotionStrength,
            PotionHaste,
            PotionMaxStamina,
            PotionFatigue,
            PotionPoison,
            PotionSlowdown,
            PotionWeakness,
            BottleOfOil,

            Hole,
            TreeStump,

            LavaLight,
            SwampGas,

            SoundSeaWaves,
            SoundLakeWaves,
            SoundSeaWind,
            SoundDesertWind,
            SoundNightCrickets,
            SoundNoonCicadas,
            SoundLava,
        }

        public static readonly Name[] allNames = (Name[])Enum.GetValues(typeof(Name));

        public static BoardPiece Create(Name templateName, World world, int generation = 0, bool randomSex = true, bool female = false, string id = null)
        {
            if (randomSex) female = BoardPiece.Random.Next(2) == 1;

            return CreatePiece(templateName: templateName, world: world, id: id, female: female, generation: generation);
        }

        public static BoardPiece CreateAndPlaceOnBoard(Name templateName, World world, Vector2 position, bool randomPlacement = false, int generation = 0, bool ignoreCollisions = false, string id = null, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false, bool randomSex = true, bool female = false)
        {
            if (randomSex) female = BoardPiece.Random.Next(2) == 1;

            BoardPiece boardPiece = CreatePiece(templateName: templateName, world: world, id: id, female: female, generation: generation);

            boardPiece.PlaceOnBoard(randomPlacement: randomPlacement, position: position, ignoreCollisions: ignoreCollisions, closestFreeSpot: closestFreeSpot, minDistanceOverride: minDistanceOverride, maxDistanceOverride: maxDistanceOverride, ignoreDensity: ignoreDensity);

            if (boardPiece.sprite.IsOnBoard)
            {
                boardPiece.soundPack.Play(PieceSoundPack.Action.HasAppeared);
                if (boardPiece.appearDebris != null) boardPiece.appearDebris.DropDebris(ignoreProcessingTime: true);
            }

            return boardPiece;
        }

        private static BoardPiece CreatePiece(Name templateName, World world, bool female, int generation = 0, string id = null)
        {
            if (id == null) id = Helpers.GetUniqueHash();

            Random random = BoardPiece.Random;

            AllowedTerrain shallowWaterToVolcano = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});

            switch (templateName)
            {
                case Name.Player:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.Volcano, AllowedTerrain.RangeName.NoBiome },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HumanSkeleton, chanceToDrop: 100, maxNumberToDrop: 1) });

                        var soundPack = new PieceSoundPack();

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerBowDraw, sound: new Sound(name: SoundData.Name.BowDraw, maxPitchVariation: 0.15f, volume: 0.6f, ignore3DAlways: true));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerBowRelease, sound: new Sound(name: SoundData.Name.BowRelease, maxPitchVariation: 0.3f, ignore3DAlways: true));

                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPlayer1, SoundData.Name.EatPlayer2, SoundData.Name.EatPlayer3, SoundData.Name.EatPlayer4 }, maxPitchVariation: 0.3f));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerSnore, sound: new Sound(name: female ? SoundData.Name.SnoringFemale : SoundData.Name.SnoringMale, maxPitchVariation: 0.3f, ignore3DAlways: true, isLooped: true, volume: 0.5f));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerYawn, sound: new Sound(name: female ? SoundData.Name.YawnFemale : SoundData.Name.YawnMale, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 1f));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerStomachGrowl, sound: new Sound(name: SoundData.Name.StomachGrowl, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 1f));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerSprint, sound: new Sound(name: SoundData.Name.Sprint, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 0.5f));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerPant, sound: new Sound(name: female ? SoundData.Name.PantFemale : SoundData.Name.PantMale, maxPitchVariation: 0.3f, ignore3DAlways: true, volume: 0.8f));

                        if (!female)
                        {
                            soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryPlayerMale1, SoundData.Name.CryPlayerMale2, SoundData.Name.CryPlayerMale3, SoundData.Name.CryPlayerMale4 }, maxPitchVariation: 0.2f));
                            soundPack.AddAction(action: PieceSoundPack.Action.Die, sound: new Sound(name: SoundData.Name.DeathPlayerMale));
                        }
                        else
                        {
                            soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryPlayerFemale1, SoundData.Name.CryPlayerFemale2, SoundData.Name.CryPlayerFemale3, SoundData.Name.CryPlayerFemale4 }, maxPitchVariation: 0.2f));
                            soundPack.AddAction(action: PieceSoundPack.Action.Die, sound: new Sound(name: SoundData.Name.DeathPlayerFemale));
                        }

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerSpeak, sound: new Sound(name: SoundData.Name.Beep, cooldown: 4, volume: 0.12f, pitchChange: female ? 0.5f : -0.5f, maxPitchVariation: 0.07f, ignore3DAlways: true));

                        soundPack.AddAction(action: PieceSoundPack.Action.PlayerPulse, sound: new Sound(name: SoundData.Name.Pulse, isLooped: true, ignore3DAlways: true, volume: 0.7f));

                        return new Player(name: templateName, world: world, id: id, animPackage: female ? AnimData.PkgName.PlayerFemale : AnimData.PkgName.PlayerMale, speed: 3, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 65535, generation: generation, invWidth: 4, invHeight: 2, toolbarWidth: 3, toolbarHeight: 1, readableName: "player", description: "This is you.", yield: yield, activeState: BoardPiece.State.PlayerControlledWalking, lightEngine: new LightEngine(size: 300, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true), soundPack: soundPack, female: female);
                    }

                case Name.PlayerGhost:
                    {
                        var soundPack = new PieceSoundPack();

                        foreach (PieceSoundPack.Action action in new List<PieceSoundPack.Action> { PieceSoundPack.Action.StepGrass, PieceSoundPack.Action.StepWater, PieceSoundPack.Action.StepSand, PieceSoundPack.Action.StepRock, PieceSoundPack.Action.SwimShallow, PieceSoundPack.Action.SwimDeep, PieceSoundPack.Action.StepLava })
                        {
                            soundPack.AddAction(action: action, sound: new Sound(name: SoundData.Name.StepGhost, cooldown: 30, ignore3DAlways: true, volume: 0.8f, maxPitchVariation: 0.2f));
                        }

                        Player spectator = new Player(name: templateName, world: world, id: id, animPackage: female ? AnimData.PkgName.PlayerFemale : AnimData.PkgName.PlayerMale, speed: 3, allowedTerrain: new AllowedTerrain(), minDistance: 0, maxDistance: 65535, generation: generation, invWidth: 4, invHeight: 2, toolbarWidth: 3, toolbarHeight: 1, readableName: "player ghost", description: "A metaphysical representation of player's soul.", blocksMovement: false, ignoresCollisions: true, floatsOnWater: true, activeState: BoardPiece.State.PlayerControlledGhosting, lightEngine: new LightEngine(size: 650, opacity: 1.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true), soundPack: soundPack, female: false);

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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 150 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 35, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.53f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A regular grass.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesTotal: 25), yield: yield);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 150 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        // readableName is the same as "regular grass", to make it appear identical to the regular grass

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 1000, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.49f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A special type of grass.", allowedDensity: new AllowedDensity(radious: 350, maxNoOfPiecesSameName: 1), yield: yield, lightEngine: new LightEngine(size: 0, opacity: 0.3f, colorActive: true, color: Color.Blue * 3f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: true, castShadows: false), maxExistingNumber: 300);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 250 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.GrassDesert, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 60, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 900, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.63f, generation: generation, staysAfterDeath: 300, readableName: "desert grass", description: "A grass, that grows on sand.", allowedDensity: new AllowedDensity(radious: 75, maxNoOfPiecesTotal: 0), yield: yield);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.Rushes, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant, minDistance: 0, maxDistance: 120, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 0.62f, generation: generation, staysAfterDeath: 300, readableName: "rushes", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 120, maxNoOfPiecesTotal: 60), yield: yield);
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WaterLily1, AnimData.PkgName.WaterLily2, AnimData.PkgName.WaterLily3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>{
                            { Terrain.Name.Height, new AllowedRange(min: 30, max: 70) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) }
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsBlue, chanceToDrop: 10, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 80 }, { Terrain.Name.Height, 45 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                            minDistance: 25, maxDistance: 80, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 2000, reproduction: reproduction, massToBurn: 4, massTakenMultiplier: 0.5f, generation: generation, staysAfterDeath: 300, floatsOnWater: true, readableName: "water lily", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesSameName: 3), yield: yield, maxHitPoints: 10);
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
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsBlack, chanceToDrop: 10, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 180 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "regular flower", description: "A flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), yield: yield);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.FlowersRed, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "red flower", description: "A red flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), lightEngine: new LightEngine(size: 0, opacity: 0.2f, colorActive: true, color: Color.Red * 1.5f, addedGfxRectMultiplier: 3f, isActive: true, glowOnlyAtNight: true, castShadows: false), yield: yield);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.FlowersYellow2, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 250, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 4000, reproduction: reproduction, massToBurn: 3, massTakenMultiplier: 0.98f, generation: generation, staysAfterDeath: 300, readableName: "mountain flower", description: "A mountain flower.", allowedDensity: new AllowedDensity(radious: 240, maxNoOfPiecesSameName: 0), yield: yield);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
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
                            minDistance: 50, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 1.35f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 50, readableName: "small tree", description: "A small tree.", allowedDensity: new AllowedDensity(radious: 200, maxNoOfPiecesSameName: 2), soundPack: soundPack);
                    }

                case Name.TreeBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
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
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, readableName: "big tree", description: "A big tree.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2), soundPack: soundPack);
                    }

                case Name.Oak:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
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

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "oak", description: "Acorns can grow on it.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2), soundPack: soundPack);
                    }

                case Name.AppleTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
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

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "apple tree", description: "Apples can grow on it.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2), soundPack: soundPack);
                    }

                case Name.CherryTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
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

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "cherry tree", description: "Cherries can grow on it.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2), soundPack: soundPack);
                    }

                case Name.BananaTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 8000 }, { 2, 10000 }, { 3, 65535 } };
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
                            minDistance: 100, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "banana tree", description: "Bananas can grow on it.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3), soundPack: soundPack);
                    }

                case Name.PalmTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var bestEnvironment = new Dictionary<Terrain.Name, byte>() { { Terrain.Name.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 8000 }, { 2, 10000 }, { 3, 65535 } };
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
                            minDistance: 100, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, readableName: "palm tree", description: "A palm tree.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3), soundPack: soundPack);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 900 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 3, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Tomato);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: false, animPackage: AnimData.PkgName.TomatoPlant, allowedTerrain: allowedTerrain, category: BoardPiece.Category.SmallPlant,
                           maxHitPoints: 40, minDistance: 20, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 1000, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 0.855f, generation: generation, staysAfterDeath: 300, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "tomato plant", description: "Tomatoes can grow on it.", allowedDensity: new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 2), yield: yield);
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
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 10000 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);

                        return new Plant(name: templateName, world: world, id: id, blocksMovement: true, animPackage: AnimData.PkgName.Cactus, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 50, maxDistance: 600, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 10, maxHitPoints: 80, massTakenMultiplier: 1.65f, generation: generation, staysAfterDeath: 5000, readableName: "cactus", description: "A desert plant.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 1), yield: yield);
                    }

                case Name.WaterRock:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WaterRock1, AnimData.PkgName.WaterRock2, AnimData.PkgName.WaterRock3, AnimData.PkgName.WaterRock4, AnimData.PkgName.WaterRock5, AnimData.PkgName.WaterRock6, AnimData.PkgName.WaterRock7 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 50, max: 74) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                            });

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "water rock", description: "Can be mined for stone.");
                    }

                case Name.MineralsSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsSmall1, AnimData.PkgName.MineralsSmall2, AnimData.PkgName.MineralsSmall3, AnimData.PkgName.MineralsSmall4 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 140, max: 180) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 25, maxNumberToDrop: 1)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.", movesWhenDropped: false);
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsBig1, AnimData.PkgName.MineralsBig2, AnimData.PkgName.MineralsBig3, AnimData.PkgName.MineralsBig4 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 140, max: 180) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Granite, chanceToDrop: 25, maxNumberToDrop: 2)
                       });

                        return new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.", movesWhenDropped: false);
                    }

                case Name.Backlight:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Backlight, minDistance: 0, maxDistance: 0, destructionDelay: -1, allowedTerrain: allowedTerrain, generation: generation, readableName: "backlight", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.BloodSplatter:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.BloodSplatter1, AnimData.PkgName.BloodSplatter2, AnimData.PkgName.BloodSplatter3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, destructionDelay: 250, allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 10, generation: generation, readableName: "bloodSplatter", description: "A pool of blood.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.Attack:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Attack, minDistance: 0, maxDistance: 3, destructionDelay: -1, allowedTerrain: allowedTerrain, generation: generation, readableName: "attack", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.MapMarker:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MapMarker, minDistance: 0, maxDistance: 0, allowedTerrain: allowedTerrain, generation: generation, readableName: "map marker", description: "Map marker.", activeState: BoardPiece.State.MapMarkerShowAndCheck, visible: false, serialize: false);
                    }

                case Name.Empty:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, minDistance: 0, maxDistance: 3, destructionDelay: 0, allowedTerrain: allowedTerrain, generation: generation, readableName: "empty", description: "Should not be used.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.Miss:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Miss, minDistance: 0, maxDistance: 3, destructionDelay: 60, allowedTerrain: allowedTerrain, generation: generation, readableName: "miss", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.Zzz:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Zzz, minDistance: 0, maxDistance: 0, destructionDelay: 0, allowedTerrain: allowedTerrain, generation: generation, fadeInAnim: true, serialize: false, readableName: "zzz", description: "A visual effect.", activeState: BoardPiece.State.Empty);
                    }

                case Name.Heart:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Heart, destructionDelay: 40, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true, readableName: "heart", description: "A visual effect.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.MusicNote:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MusicNoteSmall, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: false, readableName: "music note", description: "Sound visual.", activeState: BoardPiece.State.Empty, serialize: false);
                    }

                case Name.Crosshair:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crosshair, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "crosshair", description: "A visual effect.", activeState: BoardPiece.State.Empty);
                    }

                case Name.Exclamation:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Exclamation, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true, serialize: false, readableName: "crosshair", description: "A visual effect.", activeState: BoardPiece.State.Empty);
                    }

                case Name.FlameRegular:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: false, readableName: "flame", description: "A burning flame.", activeState: BoardPiece.State.Empty);
                    }

                case Name.CookingTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "cooking starter", description: "Starts cooking process.", activeState: BoardPiece.State.Empty);
                    }

                case Name.UpgradeTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Upgrade, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "upgrade", description: "Upgrades item.", activeState: BoardPiece.State.Empty);
                    }

                case Name.FireplaceTriggerOn:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "fireplace on", description: "Ignites the fireplace.", activeState: BoardPiece.State.Empty);
                    }

                case Name.FireplaceTriggerOff:
                    {
                        var allowedTerrain = new AllowedTerrain();
                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WaterDrop, destructionDelay: 0, allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "fireplace off", description: "Extinginguishes the fire.", activeState: BoardPiece.State.Empty);
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
                            minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 5, readableName: "supply crate", description: "Contains valuable items.", soundPack: soundPack);
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
                            minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 40, readableName: "supply crate", description: "Contains valuable items.", soundPack: soundPack);
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
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 40, readableName: "wooden chest", description: $"Can store items ({storageWidth}x{storageHeight}).", soundPack: soundPack);
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
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 50, readableName: "stone chest", description: $"Can store items ({storageWidth}x{storageHeight}).", soundPack: soundPack);
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

                        return new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestIron, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 60, readableName: "iron chest", description: $"Can store items ({storageWidth}x{storageHeight}).", soundPack: soundPack);
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
                                new Yield.DroppedPiece(pieceName: Name.Nail, chanceToDrop: 100, maxNumberToDrop: 5),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.HasAppeared, sound: new Sound(name: SoundData.Name.Chime));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        var treasureChest = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureBlue, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 2, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", yield: yield, appearDebris: new Yield(debrisType: Yield.DebrisType.Star), animName: "closed", soundPack: soundPack);

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
                                new Yield.DroppedPiece(pieceName: Name.PotionFatigue, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PotionHaste, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PotionHealing, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PotionMaxHPIncrease, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PotionMaxStamina, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PotionStrength, chanceToDrop: 20, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 10, maxNumberToDrop: 1),
                            });

                        while (true)
                        {
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces();

                            if (chestContents.Count >= 2)
                            {
                                foreach (BoardPiece piece in chestContents)
                                {
                                    treasureChest.pieceStorage.AddPiece(piece);
                                    if (treasureChest.pieceStorage.EmptySlotsCount == 0) break;
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
                                new Yield.DroppedPiece(pieceName: Name.Nail, chanceToDrop: 100, maxNumberToDrop: 10),
                                });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.HasAppeared, sound: new Sound(name: SoundData.Name.Chime));
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDestroyed, sound: new Sound(name: SoundData.Name.DestroyBox, maxPitchVariation: 0.5f));

                        var treasureChest = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureRed, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 3, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", yield: yield, appearDebris: new Yield(debrisType: Yield.DebrisType.Star), animName: "closed", soundPack: soundPack);

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
                                new Yield.DroppedPiece(pieceName: Name.PotionFatigue, chanceToDrop: 20, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.PotionHaste, chanceToDrop: 20, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.PotionHealing, chanceToDrop: 20, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.PotionMaxHPIncrease, chanceToDrop: 20, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.PotionMaxStamina, chanceToDrop: 20, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.PotionStrength, chanceToDrop: 20, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 10, maxNumberToDrop: 3),
                            });

                        while (true)
                        {
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces();

                            if (chestContents.Count >= 4)
                            {
                                foreach (BoardPiece piece in chestContents)
                                {
                                    treasureChest.pieceStorage.AddPiece(piece);
                                    if (treasureChest.pieceStorage.EmptySlotsCount == 0) break;
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
                                new Yield.DroppedPiece(pieceName: Name.BowWood, chanceToDrop: 10, maxNumberToDrop: 1),
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

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarWhole, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 40, readableName: "sealed jar", description: "Contains supplies.", movesWhenDropped: false, soundPack: soundPack);
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
                    minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 20, readableName: "broken jar", description: "Broken Jar.", movesWhenDropped: false, soundPack: soundPack);
                    }

                case Name.WorkshopEssential:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopEssential, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftEssential, maxHitPoints: 30, readableName: "essential workshop", description: "Essential crafting workshop.");
                    }

                case Name.WorkshopBasic:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopBasic, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftBasic, maxHitPoints: 30, readableName: "basic workshop", description: "Basic crafting workshop.");
                    }

                case Name.WorkshopAdvanced:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopAdvanced, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAdvanced, maxHitPoints: 80, readableName: "advanced workshop", description: "Advanced crafting workshop.");
                    }

                case Name.WorkshopMaster:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopMaster, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftMaster, maxHitPoints: 80, readableName: "master workshop", description: "Master's crafting workshop.");
                    }

                case Name.WorkshopLeatherBasic:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherBasic, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftLeatherBasic, maxHitPoints: 30, readableName: "basic leather workshop", description: "For making basic items out of leather.");
                    }

                case Name.WorkshopLeatherAdvanced:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherAdvanced, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftLeatherAdvanced, maxHitPoints: 30, readableName: "advanced leather workshop", description: "For making advanced items out of leather.");
                    }

                case Name.WorkshopAlchemy:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopAlchemy, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAlchemy, maxHitPoints: 30, readableName: "alchemy lab", description: "For potion making.");
                    }

                case Name.Furnace:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Furnace, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftFurnace, maxHitPoints: 40, readableName: "furnace", description: "For ore smelting.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true));
                    }

                case Name.Anvil:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Anvil, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAnvil, maxHitPoints: 80, readableName: "anvil", description: "For metal forming.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 1f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true));
                    }

                case Name.HotPlate:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.FryingPan, isLooped: true));
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.StoneMove1, ignore3DAlways: true));

                        var hotPlate = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HotPlate, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, maxHitPoints: 20, foodMassMultiplier: 1.1f, readableName: "hot plate", description: "For cooking.", ingredientSpace: 1, boosterSpace: 1, soundPack: soundPack);

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

                        var cookingPot = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CookingPot, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, maxHitPoints: 30, foodMassMultiplier: 1.4f, readableName: "cooking pot", description: "For cooking.", ingredientSpace: 3, boosterSpace: 3, soundPack: soundPack);

                        cookingPot.sprite.AssignNewName("off");
                        return cookingPot;
                    }

                case Name.UpgradeBench:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Open, sound: new Sound(name: SoundData.Name.ToolsMove, ignore3DAlways: true));

                        var combineWorkshop = new UpgradeBench(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.UpgradeBench, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, maxHitPoints: 30, readableName: "upgrade bench", description: "For upgrading items.", soundPack: soundPack);

                        combineWorkshop.sprite.AssignNewName("off");
                        return combineWorkshop;
                    }

                case Name.Clam:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clam, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 200, generation: generation, mass: 100, stackSize: 6, rotatesWhenDropped: true, readableName: "clam", description: "Have to be cooked before eating.");
                    }

                case Name.Shell:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Shell1, AnimData.PkgName.Shell2, AnimData.PkgName.Shell3, AnimData.PkgName.Shell4 };

                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        return new Collectible(name: templateName, world: world, id: id, animPackage: animPkg, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 6, rotatesWhenDropped: true, readableName: "shell", description: "Not really useful.");
                    }

                case Name.Stick:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Stick1, AnimData.PkgName.Stick2, AnimData.PkgName.Stick3, AnimData.PkgName.Stick4, AnimData.PkgName.Stick5, AnimData.PkgName.Stick6, AnimData.PkgName.Stick7 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropStick, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: animPkg, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stick", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.Stone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Stone, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stone", description: "Crafting material.");
                    }

                case Name.Granite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Granite, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "granite", description: "Crafting material.");
                    }

                case Name.Clay:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropMud, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clay, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "clay", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.Rope:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropRope, cooldown: 15, maxPitchVariation: 0.4f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Rope, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Flesh, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 6, floatsOnWater: false, rotatesWhenDropped: true, readableName: "rope", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.WoodLogRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogRegular, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "regular wood log", description: "Crafting material.");
                    }

                case Name.WoodLogHard:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogHard, blocksMovement: true, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "hard wood log", description: "Crafting material.");
                    }

                case Name.WoodPlank:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodPlank, blocksMovement: true, category: BoardPiece.Category.Wood,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, yield: null, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood plank", description: "Crafting material.");
                    }

                case Name.Nail:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Nail, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Metal,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 50, floatsOnWater: false, yield: null, maxHitPoints: 1, rotatesWhenDropped: true, readableName: "nail", description: "Crafting material.");
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
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 20, readableName: "dig site", description: "May contain some buried items.");
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
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");
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
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");
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

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Dirt, blocksMovement: false,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");
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
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 50, readableName: "dig site", description: "May contain some buried items.");
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
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, readableName: "iron deposit", description: "Can be mined for iron.");
                    }

                case Name.CrystalDepositBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 180, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Crystal,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Crystal,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, readableName: "big crystal deposit", description: "Can be mined for crystals.");
                    }

                case Name.CrystalDepositSmall:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 180, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var yield = new Yield(debrisType: Yield.DebrisType.Crystal,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 4)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 12)});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositSmall, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Crystal,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 150, readableName: "small crystal deposit", description: "Can be mined for crystals.");
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
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, readableName: "coal deposit", description: "Can be mined for coal.");
                    }

                case Name.Coal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Coal, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "coal", description: "Crafting material and fuel.", soundPack: soundPack);
                    }

                case Name.Crystal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropCrystal, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crystal, blocksMovement: false, allowedTerrain: allowedTerrain,
                            category: BoardPiece.Category.Crystal, rotatesWhenDropped: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, readableName: "crystal", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.IronOre:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronOre, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "iron ore", description: "Can be used to make iron bars.", soundPack: soundPack);
                    }

                case Name.GlassSand:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropSand, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlassSand, blocksMovement: false, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "glass sand", description: "Can be used to make glass.", soundPack: soundPack);
                    }

                case Name.IronBar:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropIronBar, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronBar, blocksMovement: true, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron bar", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.IronRod:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropIronRod, cooldown: 15, maxPitchVariation: 0.6f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronRod, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 18, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron rod", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.IronPlate:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropIronPlate, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronPlate, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 18, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron plate", description: "Crafting material.", soundPack: soundPack);
                    }

                case Name.Apple:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Apple, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 70, readableName: "apple", description: "Can be eaten or cooked.");
                    }

                case Name.Cherry:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Cherry, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 16, mass: 40, readableName: "cherry", description: "Can be eaten or cooked.");
                    }

                case Name.Banana:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Banana, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 80, readableName: "banana", description: "Can be eaten or cooked.");
                    }

                case Name.Tomato:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Tomato, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 20, readableName: "tomato", description: "Can be eaten or cooked.");
                    }

                case Name.Acorn:
                    {
                        return new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Acorn, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 100, readableName: "acorn", description: "Have to be cooked before eating.", canBeEatenRaw: false);
                    }

                case Name.HerbsGreen:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.MaxHP, value: 50f, autoRemoveDelay: 5 * 60 * 60)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsGreen, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 10, rotatesWhenDropped: true, readableName: "green herbs", description: "Potion ingredient.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.HerbsViolet:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Fatigue, value: -120f, isPermanent: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsViolet, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 10, rotatesWhenDropped: true, readableName: "violet herbs", description: "Potion ingredient.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.HerbsBlack:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-15, autoRemoveDelay: 60 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlack, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "black herbs", description: "Contain poison.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.HerbsBlue:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.MaxStamina, value: 100f, autoRemoveDelay: 60 * 60 * 3)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlue, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "blue herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.HerbsCyan:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Haste, value: (int)2, autoRemoveDelay: 60 * 30)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsCyan, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "cyan herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.HerbsRed:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsRed, blocksMovement: false, allowedTerrain: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "red herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.HerbsYellow:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Strength, value: (int)2, autoRemoveDelay: 60 * 60 * 3)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropPlant, cooldown: 15, maxPitchVariation: 0.7f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsYellow, blocksMovement: false, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "yellow herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList, soundPack: soundPack);
                    }

                case Name.EmptyBottle:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.EmptyBottle, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, floatsOnWater: false, readableName: "empty bottle", description: "Can be used to make a potion.", soundPack: soundPack);
                    }

                case Name.PotionHealing:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionRed, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "healing potion", description: "Restores hit points.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionMaxHPIncrease:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.MaxHP, value: 200f, autoRemoveDelay: 5 * 60 * 60)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionGreen, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "max health increase potion", description: "Increases max health for some time.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionMaxHPDecrease:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.MaxHP, value: -50f, autoRemoveDelay: 1 * 60 * 60)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionDarkGreen, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "max health decrease potion", description: "Decreases max health for some time.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionStrength:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Strength, value: (int)5, autoRemoveDelay: 60 * 60 * 3)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionYellow, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "strength potion", description: "Increases strength for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionWeakness:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Strength, value: (int)-2, autoRemoveDelay: 60 * 60 * 1, increaseIDAtEveryUse: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionDarkYellow, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "weakness potion", description: "Decreases strength for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionHaste:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Haste, value: (int)4, autoRemoveDelay: 60 * 30)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionCyan, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "haste potion", description: "Gives inhuman speed for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionMaxStamina:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.MaxStamina, value: 200f, autoRemoveDelay: 60 * 60 * 3)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionBlue, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "max stamina potion", description: "Increases maximum stamina for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionFatigue:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(type: BuffEngine.BuffType.Fatigue, value: (float)-800, isPermanent: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionViolet, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "fatigue remove potion", description: "Removes fatigue.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionPoison:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 30 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionBlack, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "poison potion", description: "Contains poison.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.PotionSlowdown:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.Speed, value: -1f, autoRemoveDelay: 30 * 60, canKill: false, increaseIDAtEveryUse: true)};

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionDarkViolet, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "slowdown potion", description: "Decreases speed.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.BottleOfOil:
                    {
                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(name: SoundData.Name.DropGlass, cooldown: 15, maxPitchVariation: 0.3f));

                        return new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionLightYellow, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, floatsOnWater: false, readableName: "bottle of oil", description: "Crafting material.", buffList: null, convertsToWhenUsed: Name.EmptyBottle, soundPack: soundPack);
                    }

                case Name.MeatRaw:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatRaw, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 6, mass: 100, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "raw meat", description: "Can be eaten or cooked.", soundPack: soundPack);
                    }

                case Name.MeatDried:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatDried, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 6, mass: 130, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "dried meat", description: "Can be eaten. Does not spoil.", soundPack: soundPack);
                    }

                case Name.Fat:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.IsDropped, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.DropMeat1, SoundData.Name.DropMeat2, SoundData.Name.DropMeat3 }, cooldown: 15, maxPitchVariation: 0.8f));

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Fat, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 50, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, readableName: "fat", description: "Can be cooked or crafted.", soundPack: soundPack);
                    }

                case Name.Leather:
                    {
                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Leather, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, mass: 100, rotatesWhenDropped: true, readableName: "leather", description: "Crafting material.");
                    }

                case Name.Burger:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Burger, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 300, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, floatsOnWater: false, readableName: "hamburger", description: "Synthetic food, that will remain fresh forever. Can be eaten.");
                    }

                case Name.Meal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MealStandard, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedTerrain: allowedTerrain, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "cooked meal", description: "Can be eaten.");
                    }

                case Name.Rabbit:
                    {
                        List<AnimData.PkgName> packageNames;
                        if (female)
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitLightGray, AnimData.PkgName.RabbitBeige, AnimData.PkgName.RabbitWhite }; }
                        else
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitBrown, AnimData.PkgName.RabbitDarkBrown, AnimData.PkgName.RabbitGray, AnimData.PkgName.RabbitBlack, AnimData.PkgName.RabbitLightBrown }; }

                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.GroundAll });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 200 }, { 1, 500 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 70, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 50, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 50, maxNumberToDrop: 1) });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CrySmallAnimal1, SoundData.Name.CrySmallAnimal2, SoundData.Name.CrySmallAnimal3, SoundData.Name.CrySmallAnimal4 }, maxPitchVariation: 0.3f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatHerbivore1, SoundData.Name.EatHerbivore2, SoundData.Name.EatHerbivore3, SoundData.Name.EatHerbivore4, SoundData.Name.EatHerbivore5 }, maxPitchVariation: 0.25f, cooldown: 35));

                        return new Animal(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, speed: 1.5f,
                            minDistance: 10, maxDistance: 45, maxHitPoints: 150, mass: 35, maxMass: 5000, massBurnedMultiplier: 1, awareness: 200, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 300, sightRange: 300, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.Tomato, Name.Meal }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "rabbit", description: "A small animal.", soundPack: soundPack);
                    }

                case Name.Fox:
                    {
                        List<AnimData.PkgName> packageNames;
                        if (female)
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxWhite, AnimData.PkgName.FoxGray, AnimData.PkgName.FoxYellow }; }
                        else
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxGinger, AnimData.PkgName.FoxRed, AnimData.PkgName.FoxBlack, AnimData.PkgName.FoxChocolate, AnimData.PkgName.FoxBrown }; }
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 1000 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 70, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 80, maxNumberToDrop: 1) });

                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CrySmallAnimal1, SoundData.Name.CrySmallAnimal2, SoundData.Name.CrySmallAnimal3, SoundData.Name.CrySmallAnimal4 }, maxPitchVariation: 0.3f, pitchChange: -0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPredator1, SoundData.Name.EatPredator2, SoundData.Name.EatPredator3, SoundData.Name.EatPredator4 }, maxPitchVariation: 0.25f, cooldown: 60));

                        return new Animal(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, speed: 1.5f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 300, mass: 60, maxMass: 15000, awareness: 80, female: female, massBurnedMultiplier: 1.3f, matureAge: 2000, maxAge: 30000, pregnancyDuration: 4000, maxChildren: 6, maxStamina: 800, sightRange: 450, eats: new List<Name> { Name.Rabbit, Name.Player, Name.MeatRaw, Name.Fat, Name.Burger, Name.MeatDried, Name.Meal }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "fox", description: "An animal.", soundPack: soundPack);
                    }

                case Name.Tiger:
                    {
                        List<AnimData.PkgName> packageNames;
                        if (female)
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.TigerWhite, AnimData.PkgName.TigerYellow }; }
                        else
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.TigerOrangeMedium, AnimData.PkgName.TigerGray, AnimData.PkgName.TigerOrangeLight, AnimData.PkgName.TigerOrangeDark, AnimData.PkgName.TigerBrown, AnimData.PkgName.TigerBlack }; }
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 2000 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 100, maxNumberToDrop: 3), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 100, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 100, maxNumberToDrop: 2) });

                        var animPkg = packageNames[random.Next(0, packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 160, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(name: SoundData.Name.TigerRoar, maxPitchVariation: 0.3f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatPredator1, SoundData.Name.EatPredator2, SoundData.Name.EatPredator3, SoundData.Name.EatPredator4 }, maxPitchVariation: 0.25f, cooldown: 60));

                        return new Animal(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, speed: 2.4f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 1600, mass: 80, maxMass: 15000, awareness: 50, female: female, massBurnedMultiplier: 0.5f, matureAge: 4000, maxAge: 50000, pregnancyDuration: 3500, maxChildren: 5, maxStamina: 1300, sightRange: 700, eats: new List<Name> { Name.Rabbit, Name.Player, Name.MeatRaw, Name.Fat, Name.Burger, Name.MeatDried, Name.Fox, Name.Meal }, strength: 140, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "tiger", description: "Very dangerous animal.", soundPack: soundPack);
                    }

                case Name.Frog:
                    {
                        List<AnimData.PkgName> packageNames;
                        if (female) packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog2, AnimData.PkgName.Frog8 };
                        else
                        {
                            packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog1, AnimData.PkgName.Frog3, AnimData.PkgName.Frog4, AnimData.PkgName.Frog5, AnimData.PkgName.Frog6, AnimData.PkgName.Frog7 };
                        }

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                        firstDroppedPieces: new List<Yield.DroppedPiece> { },
                        finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.MeatRaw, chanceToDrop: 40, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1), });

                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundSand },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } }
                            );

                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 300 }, { 1, 800 }, { 2, 65535 } };

                        var soundPack = new PieceSoundPack();
                        soundPack.AddAction(action: PieceSoundPack.Action.Cry, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.CryFrog1, SoundData.Name.CryFrog2, SoundData.Name.CryFrog3, SoundData.Name.CryFrog4, }, maxPitchVariation: 0.5f));
                        soundPack.AddAction(action: PieceSoundPack.Action.Eat, sound: new Sound(nameList: new List<SoundData.Name> { SoundData.Name.EatHerbivore1, SoundData.Name.EatHerbivore2, SoundData.Name.EatHerbivore3, SoundData.Name.EatHerbivore4, SoundData.Name.EatHerbivore5 }, maxPitchVariation: 0.25f, cooldown: 35));

                        return new Animal(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, speed: 1.5f,
                       minDistance: 5, maxDistance: 30, maxHitPoints: 150, mass: 10, maxMass: 1200, massBurnedMultiplier: 1, awareness: 100, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 200, sightRange: 150, eats: new List<Name> { Name.WaterLily, Name.Rushes }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "frog", description: "A water animal.", soundPack: soundPack);
                    }

                case Name.Hand:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Wood, 0.5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hand, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Indestructible,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: true, multiplierByCategory: multiplierByCategory, maxHitPoints: 1, readableName: "hand", description: "Basic 'tool' to break stuff.");
                    }

                case Name.AxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.Stone, 1f }, { BoardPiece.Category.Wood, 5f }, { BoardPiece.Category.Flesh, 3f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden axe", description: "Basic logging tool.");
                    }

                case Name.AxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 2f }, { BoardPiece.Category.Wood, 8f }, { BoardPiece.Category.Flesh, 5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone axe", description: "Average logging tool.");
                    }

                case Name.AxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 3f }, { BoardPiece.Category.Wood, 15 }, { BoardPiece.Category.Flesh, 8f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron axe", description: "Advanced logging tool.");
                    }

                case Name.AxeCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 6f }, { BoardPiece.Category.Stone, 6f }, { BoardPiece.Category.Wood, 40 }, { BoardPiece.Category.Flesh, 10f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "crystal axe", description: "Deluxe logging tool.");
                    }

                case Name.ShovelStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 1.5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 80, readableName: "stone shovel", description: "Basic shovel.");
                    }

                case Name.ShovelIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 5f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "iron shovel", description: "Advanced shovel.");
                    }

                case Name.ShovelCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Dirt, 15f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Crystal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "crystal shovel", description: "Deluxe shovel.");
                    }

                case Name.SpearWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 7f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 50, readableName: "wooden spear", description: "Essential melee weapon.");
                    }

                case Name.SpearStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 8f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "stone spear", description: "Simple melee weapon.");
                    }

                case Name.SpearIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 8f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 10, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "iron spear", description: "Advanced melee weapon.");
                    }

                case Name.SpearCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 14f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 10, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 250, readableName: "crystal spear", description: "Deluxe melee weapon.");
                    }

                case Name.PickaxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.SmallPlant, 0.0f }, { BoardPiece.Category.Stone, 5f }, { BoardPiece.Category.Wood, 1f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden pickaxe", description: "Basic mining tool.");
                    }

                case Name.PickaxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 8f }, { BoardPiece.Category.Wood, 2f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone pickaxe", description: "Average mining tool.");
                    }

                case Name.PickaxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 15f }, { BoardPiece.Category.Wood, 3f }, { BoardPiece.Category.Crystal, 3f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron pickaxe", description: "Advanced mining tool. Can break crystals.");
                    }

                case Name.PickaxeCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 6f }, { BoardPiece.Category.Stone, 30f }, { BoardPiece.Category.Wood, 6f }, { BoardPiece.Category.Crystal, 6f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "crystal pickaxe", description: "Deluxe mining tool.");
                    }

                case Name.ScytheStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 2f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheStone, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "stone scythe", description: "Can cut down small plants.", range: 20);
                    }

                case Name.ScytheIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 3f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheIron, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "iron scythe", description: "Can cut down small plants easily.", range: 40);
                    }

                case Name.ScytheCrystal:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 6f } };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheCrystal, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 250, readableName: "crystal scythe", description: "Brings an end to all small plants.", range: 80);
                    }

                case Name.BowWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Flesh, 5f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.ArrowWood, Name.ArrowStone, Name.ArrowIron, Name.ArrowCrystal };

                        return new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowWood, allowedTerrain: shallowWaterToVolcano, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "wooden bow", description: "Projectile weapon, uses arrows for ammo.");
                    }

                case Name.ArrowWood:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowWood, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 8, indestructible: false, maxHitPoints: 15, stackSize: 15, canBeStuck: true, readableName: "wooden arrow", description: "Very weak arrow.");
                    }

                case Name.ArrowStone:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowStone, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 12, indestructible: false, maxHitPoints: 25, stackSize: 15, canBeStuck: true, readableName: "stone arrow", description: "Basic arrow.");
                    }

                case Name.ArrowIron:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowIron, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 20, indestructible: false, maxHitPoints: 40, stackSize: 15, canBeStuck: true, readableName: "iron arrow", description: "Strong arrow.");
                    }

                case Name.ArrowCrystal:
                    {
                        return new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowCrystal, allowedTerrain: shallowWaterToVolcano, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 40, indestructible: false, maxHitPoints: 50, stackSize: 15, canBeStuck: true, readableName: "crystal arrow", description: "Deluxe arrow. Deals serious damage.");
                    }

                case Name.DebrisStone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisStone, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "stone debris", description: "Floats around after hitting stone things.");
                    }

                case Name.DebrisPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisPlant, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "plant debris", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisWood:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisWood, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "wood debris", description: "Floats around after hitting wood things.");
                    }

                case Name.DebrisLeaf:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Leaf1, AnimData.PkgName.Leaf2, AnimData.PkgName.Leaf3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "leaf", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisCrystal:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DebrisCrystal, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "crystal shard", description: "Floats around after hitting crystal things.");
                    }

                case Name.DebrisCeramic:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisCeramic1, AnimData.PkgName.DebrisCeramic2 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "ceramic debris", description: "Floats around after hitting ceramic things.");
                    }

                case Name.DebrisStar:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.DebrisStar1, AnimData.PkgName.DebrisStar2, AnimData.PkgName.DebrisStar3 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.All });

                        Debris starDebris = new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 40, destructionDelayMaxOffset: 15, rotatesWhenDropped: true, readableName: "star debris", description: "Floats around after hitting stars. I guess...");

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

                        return new Debris(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: false, readableName: "blood drop", description: "Floats around after hitting living things.");
                    }

                case Name.TentSmall:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.56f, hitPointsChange: 0.05f, islandClockMultiplier: 3, canBeAttacked: false);

                        return new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentSmall, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "small tent", description: "Basic shelter for sleeping.\nProtects against enemies.");
                    }

                case Name.TentMedium:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.3f, fatigueRegen: 0.8f, hitPointsChange: 0.1f, islandClockMultiplier: 4, canBeAttacked: false);
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.MaxHP, value: 100f, sleepFramesNeededForActivation: 1 * 60 * 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        return new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentMedium, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "medium tent", description: "Average shelter for sleeping.\nProtects against enemies.", buffList: buffList, lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));
                    }

                case Name.TentBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.5f, fatigueRegen: 1.3f, hitPointsChange: 0.25f, islandClockMultiplier: 4, canBeAttacked: false);

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.MaxHP, value: 100f,sleepFramesNeededForActivation: 1 * 60 * 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new BuffEngine.Buff(type: BuffEngine.BuffType.Strength, value: 1,sleepFramesNeededForActivation: 1 * 60 * 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        return new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentBig, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 200, sleepEngine: sleepEngine, readableName: "big tent", description: "Luxurious shelter for sleeping.\nProtects against enemies.", buffList: buffList, lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));
                    }

                case Name.BackpackSmall:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)2),
                            new BuffEngine.Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackSmall, blocksMovement: true, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small backpack", description: "Expands inventory space.");
                    }

                case Name.BackpackMedium:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)3),
                            new BuffEngine.Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackMedium, blocksMovement: true, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium backpack", description: "Expands inventory space.");
                    }

                case Name.BackpackBig:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)3),
                            new BuffEngine.Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)3)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackBig, blocksMovement: true, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big backpack", description: "Expands inventory space.");
                    }

                case Name.BeltSmall:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)1)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltSmall, blocksMovement: false, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small belt", description: "Expands belt space.");
                    }

                case Name.BeltMedium:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)3)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltMedium, blocksMovement: false, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium belt", description: "Expands belt space.");
                    }

                case Name.BeltBig:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)5)};

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltBig, blocksMovement: false, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big belt", description: "Expands belt space.");
                    }

                case Name.Map:
                    {
                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(type: BuffEngine.BuffType.EnableMap, value: null) };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Map, blocksMovement: false, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "map", description: "Keeps track of visited places.");
                    }

                case Name.HatSimple:
                    {
                        var buffList = new List<BuffEngine.Buff>
                        {
                           new BuffEngine.Buff(type: BuffEngine.BuffType.HeatProtection, value: null)
                        };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HatSimple, blocksMovement: false, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "hat", description: "Simple hat");
                    }

                case Name.BootsProtective:
                    {
                        var buffList = new List<BuffEngine.Buff>
                        {
                           new BuffEngine.Buff(type: BuffEngine.BuffType.SwampProtection, value: null)
                        };

                        return new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsProtective, blocksMovement: false, category: BoardPiece.Category.Flesh,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "protective boots", description: "Allow to walk safely over swamp area.");
                    }

                case Name.TorchSmall:
                    {
                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(type: BuffEngine.BuffType.LightSource, value: 4) };

                        return new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SmallTorch, blocksMovement: false, category: BoardPiece.Category.Wood,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 250, readableName: "small torch", description: "A portable light source.");
                    }

                case Name.TorchBig:
                    {
                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(type: BuffEngine.BuffType.LightSource, value: 6) };

                        return new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BigTorch, blocksMovement: false, category: BoardPiece.Category.Wood,
                            allowedTerrain: shallowWaterToVolcano, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 600, readableName: "big torch", description: "A portable light source. Burns for a long time.");
                    }

                case Name.Campfire:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        ushort range = 150;

                        var fireplace = new Fireplace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Campfire, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Stone, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 2, storageHeight: 2, maxHitPoints: 30, readableName: "campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        fireplace.sprite.AssignNewName("off");
                        return fireplace;
                    }

                case Name.PredatorRepellant:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        return new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SkullAndBones, minDistance: 0, maxDistance: 0, destructionDelay: 60 * 60 * 5, allowedTerrain: allowedTerrain, generation: generation, readableName: "predator repellent", description: "Scares predators and is invisible.", activeState: BoardPiece.State.ScareAnimalsAway, visible: false, serialize: false);
                    }

                case Name.Hole:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hole, allowedTerrain: allowedTerrain, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, maxHitPoints: 1, readableName: "hole", description: "Empty dig site.", movesWhenDropped: false, destructionDelay: 60 * 30);
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
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, maxHitPoints: 50, readableName: "tree stump", description: "This was once a tree.", movesWhenDropped: false, yield: yield, soundPack: soundPack);
                    }

                case Name.HumanSkeleton:
                    {
                        return new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HumanSkeleton, allowedTerrain: new AllowedTerrain(), category: BoardPiece.Category.Flesh,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: null, maxHitPoints: 100, readableName: "skeleton", description: "Human remains.", movesWhenDropped: false);
                    }

                case Name.LavaLight:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) },
                        });

                        VisualEffect lavalight = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpot, minDistance: 0, maxDistance: 500, destructionDelay: 0, allowedTerrain: allowedTerrain, generation: generation, readableName: "lava light", description: "Emits light on lava.", activeState: BoardPiece.State.Empty, visible: true, lightEngine: new LightEngine(size: 150, opacity: 0.3f, colorActive: true, color: Color.Orange * 0.6f, addedGfxRectMultiplier: 3f, isActive: true, glowOnlyAtNight: false, castShadows: true), ignoresCollisions: false, allowedDensity: new AllowedDensity(radious: 130, maxNoOfPiecesSameName: 0), serialize: true);

                        lavalight.sprite.color = Color.Orange;

                        return lavalight;
                    }

                case Name.SwampGas:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4, AnimData.PkgName.Fog5, AnimData.PkgName.Fog6, AnimData.PkgName.Fog7, AnimData.PkgName.Fog8 };
                        var animPkg = packageNames[random.Next(0, packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 120, maxNoOfPiecesSameName: 2);

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, destructionDelay: 0, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, minDistance: 0, maxDistance: 0, generation: generation, fadeInAnim: false, readableName: "gas", description: "Swamp gas.", activeState: BoardPiece.State.FogMoveRandomly, serialize: true, ignoresCollisions: false, visible: true);

                        visualEffect.sprite.color = Color.LimeGreen;
                        visualEffect.sprite.opacity = 0.3f;

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

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient sea wind sound", description: "Ambient sound for sea wind.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.White;

                        return ambientSound;
                    }

                case Name.SoundDesertWind:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: 160) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 115) }});

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 170, maxNoOfPiecesSameName: 0);

                        Sound sound = new Sound(name: SoundData.Name.DesertWind, maxPitchVariation: 0.5f, volume: 0.2f, isLooped: true, volumeFadeFrames: 60);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient desert wind sound", description: "Ambient sound for desert wind.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Orange;

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

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient noon cicadas sound", description: "Ambient sound for cicadas at noon.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds, partOfDayList: new List<IslandClock.PartOfDay> { IslandClock.PartOfDay.Noon });

                        ambientSound.sprite.color = Color.Green;

                        return ambientSound;
                    }

                case Name.SoundLava:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                            { Terrain.Name.Biome, new AllowedRange(min: 0, max: (byte)(Terrain.biomeMin - 1)) }
                        });

                        AllowedDensity allowedDensity = new AllowedDensity(radious: 160, maxNoOfPiecesSameName: 1);

                        Sound sound = new Sound(name: SoundData.Name.Lava, maxPitchVariation: 0.5f, volume: 1f, isLooped: true, volumeFadeFrames: 60);

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, allowedDensity: allowedDensity, readableName: "ambient lava sound", description: "Ambient sound for lava.", sound: sound, playDelay: 0, visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Yellow;

                        return ambientSound;
                    }

                default: { throw new DivideByZeroException($"Unsupported template name - {templateName}."); }
            }
        }
    }
}