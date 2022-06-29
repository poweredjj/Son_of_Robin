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
            GrassDesert,
            Rushes,
            WaterLily,
            FlowersPlain,
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
            Leather,

            CookedMeat,
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

            RegularWorkshop,
            Furnace,
            CookingPot,

            Stick,
            Stone,
            WoodLog,
            WoodPlank,
            Nail,
            Shell,
            CoalDeposit,
            IronDeposit,
            Coal,
            IronOre,
            IronBar,

            Backlight,
            BloodSplatter,
            Attack,
            Miss,
            Zzz,
            Heart,
            Crosshair,
            Exclamation,
            FlameRegular,
            FlameTrigger,

            Hand,
            AxeWood,
            AxeStone,
            AxeIron,
            PickaxeWood,
            PickaxeStone,
            PickaxeIron,
            BatWood,
            Scythe,

            Sling,
            GreatSling,
            BowWood,

            StoneAmmo,
            ArrowWood,
            ArrowIron,

            DebrisPlant,
            DebrisStone,
            DebrisWood,
            BloodDrop,

            TentSmall,
            TentMedium,
            TentBig,

            BackpackMedium,
            BeltMedium,

            Map,
            Torch
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
            switch (templateName)
            {
                case Name.Player:
                    {
                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll, AllowedFields.RangeName.Volcano, AllowedFields.RangeName.NoDanger });

                        return new Player(name: templateName, world: world, position: position, animPackage: AnimPkg.Blonde, speed: 3, allowedFields: allowedFields, minDistance: 0, maxDistance: 65535, generation: generation, invWidth: 4, invHeight: 2, toolbarWidth: 3, toolbarHeight: 1, readableName: "player", description: "This is you.", yield: yield);
                    }

                case Name.PlayerGhost:
                    {
                        var spectator = new Spectator(name: templateName, fadeInAnim: false, world: world, position: position, animPackage: AnimPkg.Blonde, destructionDelay: 0, minDistance: 0, maxDistance: 0, generation: generation, readableName: "player ghost", description: "A metaphysical representation of player's soul.");

                        spectator.sprite.opacity = 0.5f;
                        spectator.sprite.color = new Color(150, 255, 255);
                        spectator.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.SkyBlue * 0.7f, textureSize: spectator.sprite.frame.originalTextureSize, priority: 0, framesLeft: -1));


                        return spectator;
                    }

                case Name.GrassRegular:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 50, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { }, // TODO add some drops
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 150 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.GrassRegular, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 0, maxDistance: 35, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.53f, generation: generation, staysAfterDeath: 300, readableName: "regular grass", description: "A regular grass.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesTotal: 25), yield: yield);
                    }

                case Name.GrassDesert:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 105, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 115) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { }, // TODO add some drops
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 40 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 250 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.GrassDesert, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 60, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 900, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.63f, generation: generation, staysAfterDeath: 300, readableName: "desert grass", description: "A grass, that grows on sand.", allowedDensity: new AllowedDensity(radious: 75, maxNoOfPiecesTotal: 0), yield: yield);
                    }

                case Name.Rushes:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 77, max: 95) },
                            { TerrainName.Humidity, new AllowedRange(min: 128, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { }, // TODO add some drops
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 220 }, { TerrainName.Height, 92 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);


                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.Rushes, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant, minDistance: 0, maxDistance: 120, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 0.62f, generation: generation, staysAfterDeath: 300, readableName: "rushes", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 120, maxNoOfPiecesTotal: 60), yield: yield);
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.WaterLily1, AnimPkg.WaterLily2, AnimPkg.WaterLily3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 30, max: 70) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 128) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { }, // TODO add some drops
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 80 }, { TerrainName.Height, 45 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                            minDistance: 25, maxDistance: 80, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 2000, reproduction: reproduction, massToBurn: 4, massTakenMultiplier: 0.5f, generation: generation, staysAfterDeath: 300, floatsOnWater: true, readableName: "water lily", description: "A water plant.", allowedDensity: new AllowedDensity(radious: 50, maxNoOfPiecesSameName: 3), yield: yield);
                    }

                case Name.FlowersPlain:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.FlowersYellow1, AnimPkg.FlowersWhite };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 140, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { }, // TODO add some drops
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 180 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 100, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300, readableName: "regular flower", description: "A flower.", allowedDensity: new AllowedDensity(radious: 100, maxNoOfPiecesSameName: 0), yield: yield);
                    }

                case Name.FlowersMountain:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min:160, max: 210
                            ) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.saveZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { }, // TODO add some drops
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Height, 175 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.FlowersYellow2, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
                          minDistance: 0, maxDistance: 250, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 4000, reproduction: reproduction, massToBurn: 3, massTakenMultiplier: 0.98f, generation: generation, staysAfterDeath: 300, readableName: "mountain flower", description: "A mountain flower.", allowedDensity: new AllowedDensity(radious: 240, maxNoOfPiecesSameName: 0), yield: yield, lightEngine: new LightEngine(size: 0, opacity: 0.3f, colorActive: true, color: Color.Blue * 0.6f, addedGfxRectMultiplier: 4f, isActive: true));
                    }

                case Name.TreeSmall:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.TreeSmall1, AnimPkg.TreeSmall2 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 105, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 80, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.3f);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
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

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 40, maxDistance: 300, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, readableName: "big tree", description: "A big tree.", allowedDensity: new AllowedDensity(radious: 260, maxNoOfPiecesSameName: 2));
                    }

                case Name.AcornTree:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.saveZoneMax, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Acorn);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
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

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
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

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
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

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                         firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 80, maxNumberToDrop: 1) },
                         finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.PalmTree, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 100, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, fruitEngine: fruitEngine, boardTask: Scheduler.TaskName.DropFruit, readableName: "banana tree", description: "Bananas can grow on it.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3));
                    }

                case Name.TomatoPlant:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 100, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 100, max: 200) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { });

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 900 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 3, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Tomato);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.TomatoPlant, allowedFields: allowedFields, category: BoardPiece.Category.SmallPlant,
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

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                           firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 80, maxNumberToDrop: 2) },
                           finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 4),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.PalmTree, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 100, maxDistance: 400, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, readableName: "palm tree", description: "A palm tree.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 3));
                    }

                case Name.Cactus:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 90) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Plant,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { }); // TODO add some drops

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 60 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 10000 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.Cactus, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 50, maxDistance: 600, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 10, maxHitPoints: 80, massTakenMultiplier: 1.65f, generation: generation, staysAfterDeath: 5000, readableName: "cactus", description: "A desert plant.", allowedDensity: new AllowedDensity(radious: 300, maxNoOfPiecesSameName: 1), yield: yield);
                    }

                case Name.WaterRock:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.WaterRock1, AnimPkg.WaterRock2, AnimPkg.WaterRock3, AnimPkg.WaterRock4, AnimPkg.WaterRock5, AnimPkg.WaterRock6, AnimPkg.WaterRock7 };
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
                        var packageNames = new List<AnimPkg> { AnimPkg.MineralsSmall1, AnimPkg.MineralsSmall2, AnimPkg.MineralsSmall3, AnimPkg.MineralsSmall4 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.StoneAmmo, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.StoneAmmo, chanceToDrop: 100, maxNumberToDrop: 2),
                                });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.");
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.MineralsBig1, AnimPkg.MineralsBig2, AnimPkg.MineralsBig3, AnimPkg.MineralsBig4 };
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
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Backlight, minDistance: 0, maxDistance: 0, destructionDelay: -1, allowedFields: allowedFields, generation: generation, readableName: "backlight", description: "A visual effect.");
                    }

                case Name.BloodSplatter:
                    {
                        var allowedFields = new AllowedFields();
                        var packageNames = new List<AnimPkg> { AnimPkg.BloodSplatter1, AnimPkg.BloodSplatter2, AnimPkg.BloodSplatter3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: animPkg, destructionDelay: 250, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 10, generation: generation, readableName: "bloodSplatter", description: "A pool of blood.");
                    }

                case Name.Attack:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Attack, minDistance: 0, maxDistance: 3, destructionDelay: -1, allowedFields: allowedFields, generation: generation, readableName: "attack", description: "A visual effect.");
                    }

                case Name.Miss:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Miss, minDistance: 0, maxDistance: 3, destructionDelay: 60, allowedFields: allowedFields, generation: generation, readableName: "miss", description: "A visual effect.");
                    }

                case Name.Zzz:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Zzz, minDistance: 0, maxDistance: 0, destructionDelay: 0, allowedFields: allowedFields, generation: generation, fadeInAnim: true, serialize: false, readableName: "zzz", description: "A visual effect.");
                    }

                case Name.Heart:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Heart, destructionDelay: 40, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true, readableName: "heart", description: "A visual effect.");
                    }

                case Name.Crosshair:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Crosshair, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: false, readableName: "crosshair", description: "A visual effect.");
                    }

                case Name.Exclamation:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Exclamation, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true, serialize: false, readableName: "crosshair", description: "A visual effect.");
                    }

                case Name.FlameRegular:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Flame, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: false, readableName: "flame", description: "A burning flame.");
                    }

                case Name.FlameTrigger:
                    {
                        var allowedFields = new AllowedFields();
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Flame, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: true, canBePickedUp: true, readableName: "cooking", description: "Starts cooking process.");
                    }

                case Name.CrateStarting:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName>() { AllowedFields.RangeName.GroundAll });

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeStone, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CookedMeat, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CookedMeat, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CookedMeat, chanceToDrop: 100, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.CookedMeat, chanceToDrop: 100, maxNumberToDrop: 1),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimPkg.Crate, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
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
                                new Yield.DroppedPiece(pieceName: Name.CookedMeat, chanceToDrop: 100, maxNumberToDrop: 3),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimPkg.Crate, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 50, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 40, placeAtBeachEdge: true, isShownOnMiniMap: true, readableName: "supply crate", description: "Contains valuable items.");
                    }

                case Name.ChestWooden:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Container(name: templateName, world: world, position: position, animPackage: AnimPkg.ChestWooden, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 3, storageHeight: 3, maxHitPoints: 40, readableName: "wooden chest", description: "can store items.");
                    }

                case Name.ChestIron:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Container(name: templateName, world: world, position: position, animPackage: AnimPkg.ChestMetal, allowedFields: allowedFields, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 4, storageHeight: 3, maxHitPoints: 50, readableName: "iron chest", description: "Can store items.");
                    }

                case Name.RegularWorkshop:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimPkg.WoodenTable, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftNormal, maxHitPoints: 30, readableName: "crafting workshop", description: "For advanced item crafting.");
                    }

                case Name.Furnace:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimPkg.Furnace, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftFurnace, maxHitPoints: 40, readableName: "furnace", description: "For ore smelting.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false));
                    }

                case Name.CookingPot:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        var cookingPot = new Cooker(name: templateName, world: world, position: position, animPackage: AnimPkg.CookingPot, allowedFields: allowedFields, category: BoardPiece.Category.Metal, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 4, storageHeight: 4, maxHitPoints: 30, foodMassMultiplier: 1.2f, readableName: "cooking pot", description: "For cooking.");

                        cookingPot.sprite.AssignNewName("off");
                        return cookingPot;
                    }

                case Name.Shell:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.Shell1, AnimPkg.Shell2, AnimPkg.Shell3, AnimPkg.Shell4, AnimPkg.Shell5, AnimPkg.Shell6, AnimPkg.Shell7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 98) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: false, allowedFields: allowedFields,
                            category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 6, rotatesWhenDropped: true, placeAtBeachEdge: true, readableName: "shell", description: "Not really useful.");
                    }

                case Name.Stick:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.Stick1, AnimPkg.Stick2, AnimPkg.Stick3, AnimPkg.Stick4, AnimPkg.Stick5, AnimPkg.Stick6, AnimPkg.Stick7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Wood, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stick", description: "Crafting material.");
                    }

                case Name.Stone:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.Stone1, AnimPkg.Stone2, };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: true, allowedFields: allowedFields, category: BoardPiece.Category.Stone, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true, readableName: "stone", description: "Crafting material.");
                    }

                case Name.WoodLog:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.WoodLog, blocksMovement: true, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood log", description: "Crafting material.");
                    }

                case Name.WoodPlank:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.WoodPlank, blocksMovement: true, category: BoardPiece.Category.Wood,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, yield: null, maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood plank", description: "Crafting material.");
                    }

                case Name.Nail:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.Nail, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 50, floatsOnWater: false, yield: null, maxHitPoints: 1, rotatesWhenDropped: true, readableName: "nail", description: "Crafting material.");
                    }

                case Name.IronDeposit:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.saveZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 3)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimPkg.IronDeposit, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, isShownOnMiniMap: true, readableName: "iron deposit", description: "Can be mined for iron.");
                    }

                case Name.CoalDeposit:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) },
                            { TerrainName.Danger, new AllowedRange(min: Terrain.saveZoneMax, max: 255) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 3)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimPkg.CoalDeposit, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300, isShownOnMiniMap: true, readableName: "coal deposit", description: "Can be mined for coal.");
                    }

                case Name.Coal:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.Coal, blocksMovement: false, allowedFields: allowedFields,
                            category: BoardPiece.Category.Stone,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "coal", description: "Crafting material and fuel.");
                    }

                case Name.IronOre:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.IronOre, blocksMovement: false, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false, readableName: "iron ore", description: "Can be used to make iron bars.");
                    }

                case Name.IronBar:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.IronBar, blocksMovement: true, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, floatsOnWater: false, rotatesWhenDropped: true, readableName: "Iron bar", description: "Crafting material.");
                    }

                case Name.Apple:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Apple, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 70, readableName: "apple", description: "Can be eaten or cooked.");
                    }

                case Name.Cherry:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Cherry, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 16, mass: 40, readableName: "cherry", description: "Can be eaten or cooked.");
                    }

                case Name.Banana:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Banana, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 80, readableName: "banana", description: "Can be eaten or cooked.");
                    }

                case Name.Tomato:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Tomato, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 20, readableName: "tomato", description: "Can be eaten or cooked.");
                    }

                case Name.Acorn:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Acorn, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 100, readableName: "acorn", description: "Have to be cooked before eating.", canBeEatenRaw: false);
                    }

                case Name.RawMeat:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.RawMeat, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 100, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "raw meat", description: "Can be eaten or cooked.");
                    }

                case Name.Leather:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoEdgeMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.Leather, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, mass: 100, rotatesWhenDropped: true, readableName: "leather", description: "Crafting material.");
                    }

                case Name.CookedMeat:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.CookedMeat, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 300, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, floatsOnWater: false, readableName: "cooked meat", description: "Can be eaten.");
                    }

                case Name.Meal:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.MealStandard, blocksMovement: false, category: BoardPiece.Category.Indestructible,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 200, toolbarTask: Scheduler.TaskName.GetEaten, rotatesWhenDropped: true, readableName: "cooked meal", description: "Can be eaten.");
                    }

                case Name.Rabbit:
                    {
                        List<AnimPkg> packageNames;
                        if (female)
                        { packageNames = new List<AnimPkg> { AnimPkg.RabbitLightGray, AnimPkg.RabbitBeige, AnimPkg.RabbitWhite }; }
                        else
                        { packageNames = new List<AnimPkg> { AnimPkg.RabbitBrown, AnimPkg.RabbitDarkBrown, AnimPkg.RabbitGray, AnimPkg.RabbitBlack, AnimPkg.RabbitLightBrown }; }

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName>() { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.GroundAll });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 200 }, { 1, 500 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 100, maxNumberToDrop: 1), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 50, maxNumberToDrop: 1) });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 1.5f,
                            minDistance: 10, maxDistance: 45, maxHitPoints: 150, mass: 35, maxMass: 5000, massBurnedMultiplier: 1, awareness: 200, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 300, sightRange: 300, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.Tomato }, strength: 1, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "rabbit", description: "A small animal.");
                    }

                case Name.Fox:
                    {
                        List<AnimPkg> packageNames;
                        if (female)
                        { packageNames = new List<AnimPkg> { AnimPkg.FoxWhite, AnimPkg.FoxGray, AnimPkg.FoxYellow }; }
                        else
                        { packageNames = new List<AnimPkg> { AnimPkg.FoxGinger, AnimPkg.FoxRed, AnimPkg.FoxBlack, AnimPkg.FoxChocolate, AnimPkg.FoxBrown }; }
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 1000 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 100, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 80, maxNumberToDrop: 1) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 1.5f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 300, mass: 60, maxMass: 15000, awareness: 80, female: female, massBurnedMultiplier: 1.3f, matureAge: 2000, maxAge: 30000, pregnancyDuration: 4000, maxChildren: 6, maxStamina: 800, sightRange: 450, eats: new List<Name> { Name.Rabbit, Name.Player, Name.RawMeat, Name.CookedMeat }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "fox", description: "An animal.");
                    }

                case Name.Tiger:
                    {
                        List<AnimPkg> packageNames;
                        if (female)
                        { packageNames = new List<AnimPkg> { AnimPkg.TigerWhite, AnimPkg.TigerYellow }; }
                        else
                        { packageNames = new List<AnimPkg> { AnimPkg.TigerOrangeMedium, AnimPkg.TigerGray, AnimPkg.TigerOrangeLight, AnimPkg.TigerOrangeDark, AnimPkg.TigerBrown, AnimPkg.TigerBlack }; }
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 2000 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                          firstDroppedPieces: new List<Yield.DroppedPiece> { },
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 100, maxNumberToDrop: 3), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 100, maxNumberToDrop: 2) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll, AllowedFields.RangeName.Danger });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 2.2f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 1000, mass: 60, maxMass: 15000, awareness: 50, female: female, massBurnedMultiplier: 0.5f, matureAge: 4000, maxAge: 50000, pregnancyDuration: 3500, maxChildren: 5, maxStamina: 1300, sightRange: 700, eats: new List<Name> { Name.Rabbit, Name.Player, Name.RawMeat, Name.CookedMeat, Name.Fox }, strength: 80, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "tiger", description: "Very dangerous animal.");
                    }

                case Name.Frog:
                    {
                        List<AnimPkg> packageNames;
                        if (female)
                        { packageNames = new List<AnimPkg> { AnimPkg.Frog2, AnimPkg.Frog8 }; }
                        else
                        { packageNames = new List<AnimPkg> { AnimPkg.Frog1, AnimPkg.Frog3, AnimPkg.Frog4, AnimPkg.Frog5, AnimPkg.Frog6, AnimPkg.Frog7 }; }

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                        firstDroppedPieces: new List<Yield.DroppedPiece> { },
                        finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 70, maxNumberToDrop: 1) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundSand });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 300 }, { 1, 800 }, { 2, 65535 } };

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 1.5f,
                       minDistance: 5, maxDistance: 30, maxHitPoints: 150, mass: 10, maxMass: 1200, massBurnedMultiplier: 1, awareness: 100, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 200, sightRange: 150, eats: new List<Name> { Name.WaterLily, Name.Rushes }, strength: 1, maxMassBySize: maxMassBySize, generation: generation, yield: yield, readableName: "frog", description: "A water animal.");
                    }

                case Name.Hand:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Wood, 0.5f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.Hand, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: true, multiplierByCategory: multiplierByCategory, maxHitPoints: 1, readableName: "hand", description: "Basic 'tool' to break stuff.");
                    }

                case Name.AxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.Stone, 1f }, { BoardPiece.Category.Wood, 5f }, { BoardPiece.Category.Animal, 3f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.AxeWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden axe", description: "Basic logging tool.");
                    }

                case Name.AxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 2f }, { BoardPiece.Category.Wood, 8f }, { BoardPiece.Category.Animal, 5f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.AxeStone, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone axe", description: "Average logging tool.");
                    }

                case Name.AxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 3f }, { BoardPiece.Category.Wood, 15 }, { BoardPiece.Category.Animal, 8f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.AxeIron, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron axe", description: "Advanced logging tool.");
                    }

                case Name.BatWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 8f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.BatWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden bat", description: "A simple melee weapon.");
                    }

                case Name.PickaxeWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 1f }, { BoardPiece.Category.SmallPlant, 0.0f }, { BoardPiece.Category.Stone, 5f }, { BoardPiece.Category.Wood, 1f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.PickaxeWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75, readableName: "wooden pickaxe", description: "Basic mining tool.");
                    }

                case Name.PickaxeStone:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 2f }, { BoardPiece.Category.Stone, 8f }, { BoardPiece.Category.Wood, 2f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.PickaxeStone, allowedFields: allowedFields, category: BoardPiece.Category.Stone,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 100, readableName: "stone pickaxe", description: "Average mining tool.");
                    }

                case Name.PickaxeIron:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Metal, 3f }, { BoardPiece.Category.Stone, 15f }, { BoardPiece.Category.Wood, 3f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> { { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) } });

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.PickaxeIron, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron pickaxe", description: "Advanced mining tool.");
                    }

                case Name.Scythe:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.SmallPlant, 3f } };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> { { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) } });
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.Scythe, allowedFields: allowedFields, category: BoardPiece.Category.Metal,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 170, readableName: "iron scythe", description: "Can cut down small plants.", range: 40);
                    }

                case Name.Sling:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 3f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.StoneAmmo };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.Sling, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 80, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "basic sling", description: "A simple projectile weapon, uses small stones for ammo."
);
                    }

                case Name.GreatSling:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 4f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.StoneAmmo };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.GreatSling, allowedFields: allowedFields, category: BoardPiece.Category.Indestructible,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 4, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "great sling", description: "A simple projectile weapon, uses small stones for ammo."
                            );
                    }

                case Name.BowWood:
                    {
                        var multiplierByCategory = new Dictionary<BoardPiece.Category, float> { { BoardPiece.Category.Animal, 5f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.ArrowWood, Name.ArrowIron };

                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.BowWood, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150, shootsProjectile: true, compatibleAmmo: compatibleAmmo, readableName: "wooden bow", description: "Projectile weapon, uses arrows for ammo.");
                    }

                case Name.StoneAmmo:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});

                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimPkg.StoneSmall, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 6, indestructible: false, maxHitPoints: 10, stackSize: 10, canBeStuck: false, readableName: "small stone", description: "Sling ammo.");
                    }

                case Name.ArrowWood:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});

                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimPkg.ArrowWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 10, indestructible: false, maxHitPoints: 25, stackSize: 15, canBeStuck: true, readableName: "wooden arrow", description: "Basic bow ammo.");
                    }

                case Name.ArrowIron:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange> {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});

                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimPkg.ArrowIron, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 20, indestructible: false, maxHitPoints: 40, stackSize: 15, canBeStuck: true, readableName: "iron arrow", description: "Advanced bow ammo.");
                    }

                case Name.DebrisStone:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimPkg.DebrisStone, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "stone debris", description: "Floats around after hitting stone things.");
                    }

                case Name.DebrisPlant:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimPkg.DebrisPlant, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "plant debris", description: "Floats around after hitting plant things.");
                    }

                case Name.DebrisWood:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimPkg.DebrisWood, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, readableName: "wood debris", description: "Floats around after hitting wood things.");
                    }

                case Name.BloodDrop:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.BloodDrop1, AnimPkg.BloodDrop2, AnimPkg.BloodDrop3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: false, readableName: "blood drop", description: "Floats around after hitting living things.");
                    }

                case Name.TentSmall:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.28f, canBeAttacked: false);

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimPkg.TentSmall, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "small tent", description: "Basic shelter for sleeping.\nProtects against enemies.");
                    }

                case Name.TentMedium:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.3f, fatigueRegen: 0.4f, canBeAttacked: false);
                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxHp, value: 100f, sleepFrames: 1 * 60 * 60, isPositive: true, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimPkg.TentMedium, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "medium tent", description: "Average shelter for sleeping.\nProtects against enemies.", buffList: buffList, lightEngine: new LightEngine(size: 0, opacity: 0.65f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true));
                    }

                case Name.TentBig:
                    {
                        var allowedFields = new AllowedFields(rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoEdgeMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.5f, fatigueRegen: 0.8f, canBeAttacked: false);

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.MaxHp, value: 100f,sleepFrames: 1 * 60 * 60, isPositive: true, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.Strength, value: 1,sleepFrames: 1 * 60 * 60, isPositive: true, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimPkg.TentBig, allowedFields: allowedFields, category: BoardPiece.Category.Wood,
                            floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 200, sleepEngine: sleepEngine, readableName: "big tent", description: "Luxurious shelter for sleeping.\nProtects against enemies.", buffList: buffList, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true));
                    }

                case Name.BackpackMedium:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.InvWidth, value: (byte)2, isPositive: true),
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.InvHeight, value: (byte)2, isPositive: true)};

                        return new Equipment(name: templateName, world: world, position: position, animPackage: AnimPkg.BackpackMedium, blocksMovement: true, category: BoardPiece.Category.Animal,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium backpack", description: "Expands inventory space.");
                    }

                case Name.BeltMedium:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> {
                            new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.ToolbarWidth, value: (byte)3, isPositive: true)};

                        return new Equipment(name: templateName, world: world, position: position, animPackage: AnimPkg.BeltMedium, blocksMovement: true, category: BoardPiece.Category.Animal,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 500, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium belt", description: "Expands belt space.");
                    }

                case Name.Map:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.EnableMap, value: null, isPositive: true) };

                        return new Equipment(name: templateName, world: world, position: position, animPackage: AnimPkg.Map, blocksMovement: false, category: BoardPiece.Category.Animal,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 1, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "map", description: "Keeps track of visited places.");
                    }

                case Name.Torch:
                    {
                        var allowedFields = new AllowedFields(rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });

                        var buffList = new List<BuffEngine.Buff> { new BuffEngine.Buff(world: world, type: BuffEngine.BuffType.LightSource, value: 10, isPositive: true) };

                        return new PortableLight(name: templateName, world: world, position: position, animPackage: AnimPkg.Torch, blocksMovement: false, category: BoardPiece.Category.Wood,
                            allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 3, mass: 100, rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 400, readableName: "torch", description: "A portable light source.");
                    }

                default: { throw new DivideByZeroException($"Unsupported template name - {templateName}."); }

            }
        }

    }
}
