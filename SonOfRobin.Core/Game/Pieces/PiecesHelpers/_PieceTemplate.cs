using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin

{
    public class PieceTemplate
    {
        public enum Name
        {
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

            AcornTree,
            AppleTree,
            CherryTree,
            BananaTree,
            TomatoPlant,

            Apple,
            Banana,
            Cherry,
            Tomato,
            Acorn,

            RawMeat,
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
            ChestIron,
            CrateStarting,
            CrateRegular,

            WorkshopEssential,
            WorkshopBasic,
            WorkshopAdvanced,
            WorkshopAlchemy,

            Furnace,
            CookingPot,

            Stick,
            Stone,
            WoodLog,
            WoodPlank,
            Nail,
            Shell,
            Clam,

            CoalDeposit,
            IronDeposit,
            GlassDeposit,

            CrystalDepositSmall,
            CrystalDepositBig,

            Coal,
            IronOre,
            IronBar,
            GlassSand,
            Crystal,

            Backlight,
            BloodSplatter,
            Attack,
            Miss,
            Zzz,
            Heart,
            Crosshair,
            Exclamation,
            FlameRegular,
            CookingTrigger,
            FireplaceTriggerOn,
            FireplaceTriggerOff,

            Hand,
            AxeWood,
            AxeStone,
            AxeIron,
            PickaxeWood,
            PickaxeStone,
            PickaxeIron,
            SpearWood,
            SpearStone,
            SpearPoisoned,
            SpearIron,
            ScytheStone,
            ScytheIron,

            BowWood,

            ArrowWood,
            ArrowPoisoned,
            ArrowIron,

            DebrisPlant,
            DebrisStone,
            DebrisWood,
            DebrisLeaf,
            DebrisCrystal,
            BloodDrop,

            TentSmall,
            TentMedium,
            TentBig,

            BackpackMedium,
            BeltMedium,
            Map,

            TorchSmall,
            TorchBig,
            Campfire,

            HerbsBlack,
            HerbsBlue,
            HerbsCyan,
            HerbsGreen,
            HerbsYellow,
            HerbsRed,
            HerbsViolet,

            EmptyBottle,
            PotionHealing,
            PotionMaxHP,
            PotionStrength,
            PotionHaste,
            PotionMaxStamina,
            PotionFatigue,
            BottleOfPoison,
            BottleOfOil

        }

        public static BoardPiece CreateOnBoard(Name templateName, World world, Vector2 position, bool female, int generation = 0)
        {
            return CreatePiece(templateName: templateName, world: world, position: position, female: female, generation: generation);
        }

        public static BoardPiece CreateOnBoard(Name templateName, World world, Vector2 position, int generation = 0)
        {
            bool female = SonOfRobinGame.random.Next(2) == 1;
            return CreatePiece(templateName: templateName, world: world, position: position, female: female, generation: generation);
        }

        public static BoardPiece CreateOffBoard(Name templateName, World world, int generation = 0, bool addToPieceCount = true, bool randomSex = true, bool female = false)
        {
            // BoardPiece will have to be created on board first and then removed from it.

            bool piecesLoadingMode = world.freePiecesPlacingMode;
            world.freePiecesPlacingMode = true;

            if (randomSex) female = SonOfRobinGame.random.Next(2) == 1;
            BoardPiece piece = CreatePiece(templateName: templateName, world: world, position: new Vector2(0, 0), female: female, generation: generation);
            piece.sprite.RemoveFromGrid();
            WorldEvent.RemovePieceFromQueue(world: world, pieceToRemove: piece);

            world.freePiecesPlacingMode = piecesLoadingMode;

            if (!addToPieceCount) piece.RemoveFromPieceCount();
            return piece;
        }

        private static BoardPiece CreatePiece(Name templateName, World world, Vector2 position, bool female, int generation = 0)
        {
            var shallowWaterToVolcano = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});

            switch (templateName)
            {
                case Name.Player:
                    {
                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll, AllowedFields.RangeName.Volcano, AllowedFields.RangeName.NoDanger });

                        return new Player(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Blonde, speed: 3, allowedFields: allowedFields, minDistance: 0, maxDistance: 65535, generation: generation, invWidth: 4, invHeight: 2, toolbarWidth: 3, toolbarHeight: 1, readableName: "player", description: "This is you.", yield: yield, activeState: BoardPiece.State.PlayerControlledWalking);
                    }

                case Name.PlayerGhost:
                    {
                        Player spectator = new Player(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Blonde, speed: 3, allowedFields: new AllowedFields(), minDistance: 0, maxDistance: 65535, generation: generation, invWidth: 4, invHeight: 2, toolbarWidth: 3, toolbarHeight: 1, readableName: "player ghost", description: "A metaphysical representation of player's soul.", blocksMovement: false, ignoresCollisions: true, floatsOnWater: true, activeState: BoardPiece.State.PlayerControlledGhosting);

                        spectator.sprite.lightEngine = new LightEngine(size: 650, opacity: 1.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        spectator.speed = 5;
                        spectator.sprite.opacity = 0.5f;
                        spectator.sprite.color = new Color(150, 255, 255);
                        spectator.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.SkyBlue * 0.7f, textureSize: spectator.sprite.frame.textureSize, priority: 0, framesLeft: -1));

                        return spectator;
                    }

                case Name.GrassRegular:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 50, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsGreen, chanceToDrop: 3, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 150 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.GrassRegular, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 35, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.53f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A regular grass.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesTotal: 25), yield: yield);
                    }

                case Name.GrassGlow:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 50, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsGreen, chanceToDrop: 100, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 150 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        // readableName is the same as "regular grass", to make it appear identical to the regular grass

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.GrassRegular, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 1000, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.49f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A special type of grass.", allowedDensity: new AllowedDensity(radious: 350, maxNoOfPiecesSameName: 1), yield: yield, lightEngine: new LightEngine(size: 0, opacity: 0.3f, colorActive: true, color: Color.Blue * 3f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: true, castShadows: false), maxExistingNumber: 300);
                    }

                case Name.GrassDesert:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 105, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 115) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsRed, chanceToDrop: 3, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 40 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 250 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.GrassDesert, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 60, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 900, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.63f, generation: generation, staysAfterDeath: 300, readableName: "desert grass", description: "A grass, that grows on sand.", allowedDensity: new AllowedDensity(radious: 75, maxNoOfPiecesTotal: 0), yield: yield);
                    }

                case Name.Rushes:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 77, max: 95) },
                            { TerrainName.Humidity, new AllowedRange(min: 128, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsCyan, chanceToDrop: 1, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 220 }, { TerrainName.Height, 92 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);


                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.Rushes, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant, minDistance: 0, maxDistance: 120, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 0.62f, generation: generation, staysAfterDeath: 300, readableName: "rushes", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 120, maxNoOfPiecesTotal: 60), yield: yield);
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WaterLily1, AnimData.PkgName.WaterLily2, AnimData.PkgName.WaterLily3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 30, max: 70) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 128) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsBlue, chanceToDrop: 10, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 80 }, { TerrainName.Height, 45 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 25, maxDistance: 80, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 2000, reproduction: reproduction, massToBurn: 4, massTakenMultiplier: 0.5f, generation: generation, staysAfterDeath: 300, floatsOnWater: true, readableName: "water lily", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesSameName: 3), yield: yield, maxHitPoints: 10);
                    }

                case Name.FlowersPlain:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.FlowersYellow1, AnimData.PkgName.FlowersWhite };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 140, max: 255) }});

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsBlack, chanceToDrop: 10, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 180 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "regular flower", description: "A flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), yield: yield);
                    }

                case Name.FlowersRed:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 140, max: 255) }});

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsRed, chanceToDrop: 20, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 180 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.FlowersRed, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "red flower", description: "A red flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), lightEngine: new LightEngine(size: 0, opacity: 0.2f, colorActive: true, color: Color.Red * 1.5f, addedGfxRectMultiplier: 3f, isActive: true, glowOnlyAtNight: true, castShadows: false), yield: yield);
                    }

                case Name.FlowersMountain:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min:160, max: 210
                            ) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsYellow, chanceToDrop: 40, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Height, 175 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.FlowersYellow2, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 250, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 4000, reproduction: reproduction, massToBurn: 3, massTakenMultiplier: 0.98f, generation: generation, staysAfterDeath: 300, readableName: "mountain flower", description: "A mountain flower.", allowedDensity: new AllowedDensity(radious: 240, maxNoOfPiecesSameName: 0), yield: yield);
                    }

                case Name.TreeSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.TreeSmall1, AnimData.PkgName.TreeSmall2 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 105, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 80, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.3f);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                        firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                        finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 50, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 1.35f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 50, readableName: "small tree", description: "A small tree.", allowedDensity: new AllowedDensity(radious: 200, maxNoOfPiecesSameName: 2));
                    }

                case Name.TreeBig:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, readableName: "big tree", description: "A big tree.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2));
                    }

                case Name.AcornTree:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Acorn);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "oak", description: "Acorns can grow on it.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2));
                    }

                case Name.AppleTree:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Apple);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "apple tree", description: "Apples can grow on it.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2));
                    }

                case Name.CherryTree:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 120f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Cherry);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "cherry tree", description: "Cherries can grow on it.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2));
                    }

                case Name.BananaTree:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 210) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 8000 }, { 2, 10000 }, { 3, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 250f, yOffsetPercent: -0.29f, areaWidthPercent: 0.85f, areaHeightPercent: 0.3f, fruitName: Name.Banana);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                         firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 80, maxNumberToDrop: 1) },
                         finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.PalmTree, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 100, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "banana tree", description: "Bananas can grow on it.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3));
                    }

                case Name.TomatoPlant:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 100, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 100, max: 200) }});

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Plant, Yield.DebrisType.Leaf },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 900 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 3, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Tomato);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimData.PkgName.TomatoPlant, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                           maxHitPoints: 40, minDistance: 20, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 1000, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 0.855f, generation: generation, staysAfterDeath: 300, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "tomato plant", description: "Tomatoes can grow on it.", allowedDensity: new AllowedDensity(radious: 150, maxNoOfPiecesSameName: 2), yield: yield);
                    }

                case Name.PalmTree:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 210) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 8000 }, { 2, 10000 }, { 3, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);

                        var yield = new Yield(debrisTypeList: new List<Yield.DebrisType> { Yield.DebrisType.Wood, Yield.DebrisType.Leaf },
                           firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 80, maxNumberToDrop: 1) },
                           finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.PalmTree, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 100, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, readableName: "palm tree", description: "A palm tree.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3));
                    }

                case Name.Cactus:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 90) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.HerbsViolet, chanceToDrop: 40, maxNumberToDrop: 1) });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 60 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 10000 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimData.PkgName.Cactus, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 50, maxDistance: 600, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 10, maxHitPoints: 80, massTakenMultiplier: 1.65f, generation: generation, staysAfterDeath: 5000, readableName: "cactus", description: "A desert plant.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 1), yield: yield);
                    }

                case Name.WaterRock:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WaterRock1, AnimData.PkgName.WaterRock2, AnimData.PkgName.WaterRock3, AnimData.PkgName.WaterRock4, AnimData.PkgName.WaterRock5, AnimData.PkgName.WaterRock6, AnimData.PkgName.WaterRock7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 50, max: 74) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                      firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1) },
                      finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, floatsOnWater: true, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "water rock", description: "Can be mined for stone.");
                    }

                case Name.MineralsSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsSmall1, AnimData.PkgName.MineralsSmall2, AnimData.PkgName.MineralsSmall3, AnimData.PkgName.MineralsSmall4 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.");
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsBig1, AnimData.PkgName.MineralsBig2, AnimData.PkgName.MineralsBig3, AnimData.PkgName.MineralsBig4 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                       firstDroppedPieces: new List<Yield.DroppedPiece> { },
                       finalDroppedPieces: new List<Yield.DroppedPiece> {
                           new Yield.DroppedPiece(pieceName: Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                           new Yield.DroppedPiece(pieceName: Name.MineralsSmall, chanceToDrop: 100, maxNumberToDrop: 2),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.");
                    }

                case Name.Backlight:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Backlight, minDistance: 0, maxDistance: 0, destructionDelay: -1, allowedFields: allowedFields, generation: generation, readableName: "backlight", description: "A visual effect.");
                    }

                case Name.BloodSplatter:
                    {
                        var allowedFields = new AllowedFields();
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.BloodSplatter1, AnimData.PkgName.BloodSplatter2, AnimData.PkgName.BloodSplatter3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: animPkg, destructionDelay: 250, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 10, generation: generation, readableName: "bloodSplatter", description: "A pool of blood.");
                    }

                case Name.Attack:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Attack, minDistance: 0, maxDistance: 3, destructionDelay: -1, allowedFields: allowedFields, generation: generation, readableName: "attack", description: "A visual effect.");
                    }

                case Name.Miss:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Miss, minDistance: 0, maxDistance: 3, destructionDelay: 60, allowedFields: allowedFields, generation: generation, readableName: "miss", description: "A visual effect.");
                    }

                case Name.Zzz:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Zzz, minDistance: 0, maxDistance: 0, destructionDelay: 0, allowedFields: allowedFields, generation: generation, fadeInAnim: true, serialize: false, readableName: "zzz", description: "A visual effect.");
                    }

                case Name.Heart:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Heart, destructionDelay: 40, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true, readableName: "heart", description: "A visual effect.");
                    }

                case Name.Crosshair:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Crosshair, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "crosshair", description: "A visual effect.");
                    }

                case Name.Exclamation:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Exclamation, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true, serialize: false, readableName: "crosshair", description: "A visual effect.");
                    }

                case Name.FlameRegular:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: false, readableName: "flame", description: "A burning flame.");
                    }

                case Name.CookingTrigger:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "cooking starter", description: "Starts cooking process.");
                    }

                case Name.FireplaceTriggerOn:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Flame, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "fireplace on", description: "Ignites the fireplace.");
                    }

                case Name.FireplaceTriggerOff:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WaterDrop, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "fireplace off", description: "Extinginguishes the fire.");
                    }

                case Name.CrateStarting:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName>() { AllowedFields.RangeName.GroundAll });

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeStone, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 1),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Crate, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 5, placeAtBeachEdge: true, isShownOnMiniMap: true, readableName: "supply crate", description: "Contains valuable items.");
                    }

                case Name.CrateRegular:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName>() { AllowedFields.RangeName.GroundAll });

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeStone, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeStone, chanceToDrop: 50, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Burger, chanceToDrop: 100, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: Name.EmptyBottle, chanceToDrop: 30, maxNumberToDrop: 1),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Crate, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 40, placeAtBeachEdge: true, isShownOnMiniMap: true, readableName: "supply crate", description: "Contains valuable items.");
                    }

                case Name.ChestWooden:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Container(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ChestWooden, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 3, storageHeight: 2, maxHitPoints: 40, readableName: "wooden chest", description: "Can store items.");
                    }

                case Name.ChestIron:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Container(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ChestMetal, allowedFields: allowedFields, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 6, storageHeight: 4, maxHitPoints: 50, readableName: "iron chest", description: "Can store items.");
                    }

                case Name.WorkshopEssential:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WorkshopEssential, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftEssential, maxHitPoints: 30, readableName: "essential workshop", description: "Essential crafting workshop.");
                    }

                case Name.WorkshopBasic:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WorkshopBasic, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftBasic, maxHitPoints: 30, readableName: "basic workshop", description: "Basic crafting workshop.");
                    }

                case Name.WorkshopAdvanced:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WorkshopAdvanced, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAdvanced, maxHitPoints: 80, readableName: "advanced workshop", description: "Advanced crafting workshop.");
                    }

                case Name.WorkshopAlchemy:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WorkshopAlchemy, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftAlchemy, maxHitPoints: 30, readableName: "alchemy lab", description: "For potion making.");
                    }

                case Name.Furnace:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Furnace, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftFurnace, maxHitPoints: 40, readableName: "furnace", description: "For ore smelting.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true));
                    }

                case Name.CookingPot:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        var cookingPot = new Cooker(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.CookingPot, allowedFields: allowedFields, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, maxHitPoints: 30, foodMassMultiplier: 1.4f, readableName: "cooking pot", description: "For cooking.", ingredientSpace: 3, boosterSpace: 3);

                        cookingPot.sprite.AssignNewName("off");
                        return cookingPot;
                    }

                case Name.Clam:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll });

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Clam, blocksMovement: false, allowedFields: allowedFields,
                            category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 200, generation: generation, mass: 100, stackSize: 6, rotatesWhenDropped: true, placeAtBeachEdge: true, readableName: "clam", description: "Have to be cooked before eating.");
                    }

                case Name.Shell:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Shell1, AnimData.PkgName.Shell2, AnimData.PkgName.Shell3, AnimData.PkgName.Shell4 };

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll });

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: false, allowedFields: allowedFields,
                            category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 6, rotatesWhenDropped: true, placeAtBeachEdge: true, readableName: "shell", description: "Not really useful.");
                    }

                case Name.Stick:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Stick1, AnimData.PkgName.Stick2, AnimData.PkgName.Stick3, AnimData.PkgName.Stick4, AnimData.PkgName.Stick5, AnimData.PkgName.Stick6, AnimData.PkgName.Stick7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Wood, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stick", description: "Crafting material.");
                    }

                case Name.Stone:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Stone1, AnimData.PkgName.Stone2, };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: true, allowedFields: allowedFields, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stone", description: "Crafting material.");
                    }

                case Name.WoodLog:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WoodLog, blocksMovement: true, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood log", description: "Crafting material.");
                    }

                case Name.WoodPlank:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.WoodPlank, blocksMovement: true, category: BoardPiece.Category.Wood,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, yield: null, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood plank", description: "Crafting material.");
                    }

                case Name.Nail:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Nail, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 50, floatsOnWater: false, yield: null, maxHitPoints: 1, rotatesWhenDropped: true, readableName: "nail", description: "Crafting material.");
                    }

                case Name.IronDeposit:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 2)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 6)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.IronDeposit, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, isShownOnMiniMap: true, readableName: "iron deposit", description: "Can be mined for iron.");
                    }

                case Name.GlassDeposit:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 90) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                                firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.GlassSand, chanceToDrop: 100, maxNumberToDrop: 1)},
                                finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.GlassSand, chanceToDrop: 100, maxNumberToDrop: 3)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.GlassDeposit, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, isShownOnMiniMap: true, readableName: "glass sand deposit", description: "Can be mined for glass sand.");
                    }


                case Name.CrystalDepositBig:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Crystal,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CrystalDepositSmall, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.CrystalDepositBig, allowedFields: allowedFields, category: BoardPiece.Category.Crystal,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, isShownOnMiniMap: true, readableName: "big crystal deposit", description: "Can be mined for crystals.");
                    }

                case Name.CrystalDepositSmall:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Crystal,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 4)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 100, maxNumberToDrop: 12)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.CrystalDepositSmall, allowedFields: allowedFields, category: BoardPiece.Category.Crystal,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 150, isShownOnMiniMap: true, readableName: "small crystal deposit", description: "Can be mined for crystals.");
                    }

                case Name.CoalDeposit:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.safeZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 4)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 12)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.CoalDeposit, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, isShownOnMiniMap: true, readableName: "coal deposit", description: "Can be mined for coal.");
                    }

                case Name.Coal:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Coal, blocksMovement: false, allowedFields: allowedFields,
                            category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "coal", description: "Crafting material and fuel.");
                    }

                case Name.Crystal:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Crystal, blocksMovement: false, allowedFields: allowedFields,
                            category: BoardPiece.Category.Crystal, rotatesWhenDropped: true,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, readableName: "crystal", description: "Crafting material.");
                    }

                case Name.IronOre:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.IronOre, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "iron ore", description: "Can be used to make iron bars.");
                    }

                case Name.GlassSand:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.GlassSand, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "glass sand", description: "Can be used to make glass.");
                    }

                case Name.IronBar:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.IronBar, blocksMovement: true, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, floatsOnWater: false, rotatesWhenDropped: true, readableName: "iron bar", description: "Crafting material.");
                    }

                case Name.Apple:
                    {
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Apple, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 70, readableName: "apple", description: "Can be eaten or cooked.");
                    }

                case Name.Cherry:
                    {
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Cherry, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 16, mass: 40, readableName: "cherry", description: "Can be eaten or cooked.");
                    }

                case Name.Banana:
                    {
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Banana, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 80, readableName: "banana", description: "Can be eaten or cooked.");
                    }

                case Name.Tomato:
                    {
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Tomato, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 20, readableName: "tomato", description: "Can be eaten or cooked.");
                    }

                case Name.Acorn:
                    {
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Acorn, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 100, readableName: "acorn", description: "Have to be cooked before eating.", canBeEatenRaw: false);
                    }

                case Name.HerbsGreen:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxHP, value: 50f, autoRemoveDelay: 5 * 60 * 60)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsGreen, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 10, rotatesWhenDropped: true, readableName: "green herbs", description: "Potion ingredient.", mass: 30, buffList: buffList);
                    }

                case Name.HerbsViolet:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Fatigue, value: -120f, isPermanent: true)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsViolet, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 10, rotatesWhenDropped: true, readableName: "violet herbs", description: "Potion ingredient.", mass: 30, buffList: buffList);
                    }

                case Name.HerbsBlack:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.RegenPoison, value: (int)-15, autoRemoveDelay: 60 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsBlack, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "black herbs", description: "Contain poison.", mass: 30, buffList: buffList);
                    }

                case Name.HerbsBlue:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxStamina, value: 100f, autoRemoveDelay: 60 * 60 * 3)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsBlue, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "blue herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList);
                    }

                case Name.HerbsCyan:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Haste, value: (int)2, autoRemoveDelay: 60 * 30)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsCyan, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "cyan herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList);
                    }

                case Name.HerbsRed:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsRed, blocksMovement: false, allowedFields: shallowWaterToVolcano,
                            category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "red herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList);
                    }

                case Name.HerbsYellow:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Strength, value: (int)2, autoRemoveDelay: 60 * 60 * 3)};

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.HerbsYellow, blocksMovement: false, allowedFields: shallowWaterToVolcano, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 20, rotatesWhenDropped: true, readableName: "yellow herbs", description: "Potion and meal ingredient.", mass: 30, buffList: buffList);
                    }

                case Name.EmptyBottle:
                    {
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.EmptyBottle, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, floatsOnWater: false, readableName: "empty bottle", description: "Can be used to make a potion.");
                    }

                case Name.PotionHealing:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionRed, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "healing potion", description: "Restores hit points.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.PotionMaxHP:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxHP, value: 200f, autoRemoveDelay: 5 * 60 * 60)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionGreen, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "max health increase potion", description: "Increases max health for some time.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.PotionStrength:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Strength, value: (int)5, autoRemoveDelay: 60 * 60 * 3)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionYellow, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "strength potion", description: "Increases strength for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.PotionHaste:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Haste, value: (int)4, autoRemoveDelay: 60 * 30)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionCyan, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "haste potion", description: "Gives inhuman speed for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.PotionMaxStamina:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxStamina, value: 200f, autoRemoveDelay: 60 * 60 * 3)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionBlue, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "max stamina potion", description: "Increases maximum stamina for a short while.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.PotionFatigue:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                             new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Fatigue, value: (float)-800, isPermanent: true)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionViolet, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetDrinked, rotatesWhenDropped: true, floatsOnWater: false, readableName: "fatigue remove potion", description: "Removes fatigue.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.BottleOfPoison:
                    {
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 30 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionBlack, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, floatsOnWater: false, readableName: "bottle of poison", description: "Crafting material.", buffList: buffList, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.BottleOfOil:
                    {
                        return new Potion(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PotionLightYellow, blocksMovement: false, category: BoardPiece.Category.Indestructible, allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, floatsOnWater: false, readableName: "bottle of oil", description: "Crafting material.", buffList: null, convertsToWhenUsed: Name.EmptyBottle);
                    }

                case Name.RawMeat:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.RawMeat, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 100, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "raw meat", description: "Can be eaten or cooked.");
                    }

                case Name.Fat:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Fat, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 50, toolbarTask: Scheduler.TaskName.Empty, rotatesWhenDropped: true, readableName: "fat", description: "Can be cooked or crafted.");
                    }

                case Name.Leather:
                    {
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Leather, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: shallowWaterToVolcano,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, mass: 100, rotatesWhenDropped: true, readableName: "leather", description: "Crafting material.");
                    }

                case Name.Burger:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Burger, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 300, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, floatsOnWater: false, readableName: "hamburger", description: "Synthetic food, that will remain fresh forever. Can be eaten.");
                    }

                case Name.Meal:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.MealStandard, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 200, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "cooked meal", description: "Can be eaten.");
                    }

                case Name.Rabbit:
                    {
                        List<AnimData.PkgName> packageNames;
                        if (female)
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitLightGray, AnimData.PkgName.RabbitBeige, AnimData.PkgName.RabbitWhite }; }
                        else
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitBrown, AnimData.PkgName.RabbitDarkBrown, AnimData.PkgName.RabbitGray, AnimData.PkgName.RabbitBlack, AnimData.PkgName.RabbitLightBrown }; }

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName>() { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.GroundAll });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 200 }, { 1, 500 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 70, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 50, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 50, maxNumberToDrop: 1) });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 1.5f,
                            minDistance: 10, maxDistance: 45, maxHitPoints: 150, mass: 35, maxMass: 5000, massBurnedMultiplier: 1, awareness: 200, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 300, sightRange: 300, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.Tomato, Name.Meal }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "rabbit", description: "A small animal.");
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
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 70, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 80, maxNumberToDrop: 1) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 1.5f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 300, mass: 60, maxMass: 15000, awareness: 80, female: female, massBurnedMultiplier: 1.3f, matureAge: 2000, maxAge: 30000, pregnancyDuration: 4000, maxChildren: 6, maxStamina: 800, sightRange: 450, eats: new List<Name> { Name.Rabbit, Name.Player, Name.RawMeat, Name.Fat, Name.Burger, Name.Meal }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "fox", description: "An animal.");
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
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 100, maxNumberToDrop: 3), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 100, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 100, maxNumberToDrop: 2) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll, AllowedFields.RangeName.Danger });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 2.4f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 1600, mass: 80, maxMass: 15000, awareness: 50, female: female, massBurnedMultiplier: 0.5f, matureAge: 4000, maxAge: 50000, pregnancyDuration: 3500, maxChildren: 5, maxStamina: 1300, sightRange: 700, eats: new List<Name> { Name.Rabbit, Name.Player, Name.RawMeat, Name.Fat, Name.Burger, Name.Fox, Name.Meal }, strength: 140, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "tiger", description: "Very dangerous animal.");
                    }

                case Name.Frog:
                    {
                        List<AnimData.PkgName> packageNames;
                        if (female)
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog2, AnimData.PkgName.Frog8 }; }
                        else
                        { packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog1, AnimData.PkgName.Frog3, AnimData.PkgName.Frog4, AnimData.PkgName.Frog5, AnimData.PkgName.Frog6, AnimData.PkgName.Frog7 }; }

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                        firstDroppedPieces: new List<Yield.DroppedPiece> { },
                        finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 40, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Fat, chanceToDrop: 60, maxNumberToDrop: 1), });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundSand });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 300 }, { 1, 800 }, { 2, 65535 } };

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 1.5f,
                       minDistance: 5, maxDistance: 30, maxHitPoints: 150, mass: 10, maxMass: 1200, massBurnedMultiplier: 1, awareness: 100, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 200, sightRange: 150, eats: new List<Name> { Name.WaterLily, Name.Rushes }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "frog", description: "A water animal.");
                    }

                case Name.Hand:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Wood, 0.5f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Hand, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: true, multiplierByCategory: multiplierByCategory, maxHitPoints: 1, readableName: "hand", description: "Basic 'tool' to break stuff.");
                    }

                case Name.AxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.Stone, 1f }, { BoardPiece.Category.Wood, 5f }, { BoardPiece.Category.Animal, 3f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.AxeWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden axe", description: "Basic logging tool.");
                    }

                case Name.AxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 2f }, { BoardPiece.Category.Wood, 8f }, { BoardPiece.Category.Animal, 5f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.AxeStone, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone axe", description: "Average logging tool.");
                    }

                case Name.AxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 3f }, { BoardPiece.Category.Wood, 15 }, { BoardPiece.Category.Animal, 8f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.AxeIron, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron axe", description: "Advanced logging tool.");
                    }

                case Name.SpearWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 7f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.SpearWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 50, readableName: "wooden spear", description: "Essential melee weapon.");
                    }

                case Name.SpearStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 8f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.SpearStone, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "stone spear", description: "Simple melee weapon.");
                    }

                case Name.SpearPoisoned:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 8f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.RegenPoison, value: (int)-15, autoRemoveDelay: 30 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.SpearPoisoned, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "poisoned stone spear", description: "Melee weapon with poison applied.", buffList: buffList);
                    }

                case Name.SpearIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 8f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.SpearIron, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 10, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "iron spear", description: "Advanced melee weapon.");
                    }

                case Name.PickaxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.SmallPlant, 0.0f }, { BoardPiece.Category.Stone, 5f }, { BoardPiece.Category.Wood, 1f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PickaxeWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden pickaxe", description: "Basic mining tool.");
                    }

                case Name.PickaxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 8f }, { BoardPiece.Category.Wood, 2f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PickaxeStone, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone pickaxe", description: "Average mining tool.");
                    }

                case Name.PickaxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 15f }, { BoardPiece.Category.Wood, 3f }, { BoardPiece.Category.Crystal, 3f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> { { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) } });

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.PickaxeIron, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron pickaxe", description: "Advanced mining tool. Can break crystals");
                    }

                case Name.ScytheStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 2f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> { { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) } });
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ScytheStone, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, readableName: "stone scythe", description: "Can cut down small plants.", range: 20);
                    }

                case Name.ScytheIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 3f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> { { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) } });
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ScytheIron, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 200, readableName: "iron scythe", description: "Can cut down small plants easily.", range: 40);
                    }

                case Name.BowWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 5f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.ArrowWood, Name.ArrowIron, Name.ArrowPoisoned };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.BowWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "wooden bow", description: "Projectile weapon, uses arrows for ammo.");
                    }

                case Name.ArrowWood:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});

                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ArrowWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 10, indestructible: false, maxHitPoints: 25, stackSize: 15, canBeStuck: true, readableName: "wooden arrow", description: "Basic arrow.");
                    }

                case Name.ArrowPoisoned:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.RegenPoison, value: (int)-20, autoRemoveDelay: 30 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ArrowPoisoned, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 10, indestructible: false, maxHitPoints: 25, stackSize: 15, canBeStuck: true, readableName: "poisoned arrow", description: "Poisoned arrow.", buffList: buffList);
                    }

                case Name.ArrowIron:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});

                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.ArrowIron, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 20, indestructible: false, maxHitPoints: 40, stackSize: 15, canBeStuck: true, readableName: "iron arrow", description: "Strong arrow.");
                    }

                case Name.DebrisStone:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.DebrisStone, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "stone debris", description: "Floats around after hitting stone things.");
                    }

                case Name.DebrisPlant:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.DebrisPlant, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "plant debris", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisWood:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.DebrisWood, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "wood debris", description: "Floats around after hitting wood things.");
                    }

                case Name.DebrisLeaf:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Leaf1, AnimData.PkgName.Leaf2, AnimData.PkgName.Leaf3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "leaf", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisCrystal:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.DebrisCrystal, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: true, readableName: "crystal shard", description: "Floats around after hitting crystal things.");
                    }

                case Name.BloodDrop:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.BloodDrop1, AnimData.PkgName.BloodDrop2, AnimData.PkgName.BloodDrop3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: false, readableName: "blood drop", description: "Floats around after hitting living things.");
                    }

                case Name.TentSmall:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.28f, canBeAttacked: false);

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.TentSmall, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "small tent", description: "Basic shelter for sleeping.\nProtects against enemies.");
                    }

                case Name.TentMedium:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.3f, fatigueRegen: 0.4f, canBeAttacked: false);
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxHP, value: 100f, sleepFrames: 1 * 60 * 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.TentMedium, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "medium tent", description: "Average shelter for sleeping.\nProtects against enemies.", buffList: buffList, lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));
                    }

                case Name.TentBig:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.5f, fatigueRegen: 0.8f, canBeAttacked: false);

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxHP, value: 100f,sleepFrames: 1 * 60 * 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Strength, value: 1,sleepFrames: 1 * 60 * 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.TentBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 200, sleepEngine: sleepEngine, readableName: "big tent", description: "Luxurious shelter for sleeping.\nProtects against enemies.", buffList: buffList, lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));
                    }

                case Name.BackpackMedium:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.InvWidth, value: (byte)2),
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        return new Equipment(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.BackpackMedium, blocksMovement: true, category: BoardPiece.Category.Animal,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium backpack", description: "Expands inventory space.");
                    }

                case Name.BeltMedium:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.ToolbarWidth, value: (byte)3)};

                        return new Equipment(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.BeltMedium, blocksMovement: true, category: BoardPiece.Category.Animal,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium belt", description: "Expands belt space.");
                    }

                case Name.Map:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.EnableMap, value: null) };

                        return new Equipment(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Map, blocksMovement: false, category: BoardPiece.Category.Animal,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "map", description: "Keeps track of visited places.");
                    }

                case Name.TorchSmall:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.LightSource, value: 4) };

                        return new PortableLight(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.SmallTorch, blocksMovement: false, category: BoardPiece.Category.Wood,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 250, readableName: "small torch", description: "A portable light source.");
                    }

                case Name.TorchBig:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.LightSource, value: 6) };

                        return new PortableLight(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.BigTorch, blocksMovement: false, category: BoardPiece.Category.Wood,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 600, readableName: "big torch", description: "A portable light source. Burns for a long time.");
                    }

                case Name.Campfire:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        ushort range = 150;

                        var fireplace = new Fireplace(name: templateName, world: world, position: position, animPackage: AnimData.PkgName.Campfire, allowedFields: allowedFields, category: BoardPiece.Category.Stone, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 3, storageHeight: 3, maxHitPoints: 30, readableName: "campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        fireplace.sprite.AssignNewName("off");
                        return fireplace;
                    }


                default: { throw new DivideByZeroException($"Unsupported template name - {templateName}."); }

            }
        }

    }
}
