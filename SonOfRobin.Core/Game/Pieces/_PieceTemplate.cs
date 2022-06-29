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

            AppleTree,
            CherryTree,
            BananaTree,
            TomatoPlant,

            Apple,
            Banana,
            Cherry,
            Tomato,

            RawMeat,
            Leather,

            CookedMeat,

            Rabbit,
            Fox,
            Frog,
            Crab,

            WaterRock,
            MineralsSmall,
            MineralsBig,

            ChestWooden,
            ChestIron,

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

            Hand,
            AxeWood,
            AxeStone,
            AxeIron,
            PickaxeWood,
            PickaxeStone,
            PickaxeIron,
            BatWood,

            Sling,
            GreatSling,
            BowWood,

            StoneAmmo,
            ArrowWood,
            ArrowIron,

            DebrisStone,
            DebrisWood,
            BloodDrop,

            TentSmall,
            TentMedium,
            TentBig
        }

        public static BoardPiece CreateOnBoard(Name templateName, World world, Vector2 position, bool female, int generation = 0)
        {
            return PieceFactory(templateName: templateName, world: world, position: position, female: female, generation: generation);
        }

        public static BoardPiece CreateOnBoard(Name templateName, World world, Vector2 position, int generation = 0)
        {
            bool female = SonOfRobinGame.random.Next(2) == 1;
            return PieceFactory(templateName: templateName, world: world, position: position, female: female, generation: generation);
        }

        public static BoardPiece CreateOffBoard(Name templateName, World world, int generation = 0, bool addToPieceCount = true, bool randomSex = true, bool female = false)
        {
            // BoardPiece will have to be created on board first and then removed from it.

            bool piecesLoadingMode = world.piecesLoadingMode;
            world.piecesLoadingMode = true;

            if (randomSex) female = SonOfRobinGame.random.Next(2) == 1;
            BoardPiece piece = PieceFactory(templateName: templateName, world: world, position: new Vector2(0, 0), female: female, generation: generation);
            piece.sprite.RemoveFromGrid();
            WorldEvent.RemovePieceFromQueue(world: world, pieceToRemove: piece);

            world.piecesLoadingMode = piecesLoadingMode;

            if (!addToPieceCount) piece.RemoveFromPieceCount();
            return piece;
        }

        public static BoardPiece PieceFactory(Name templateName, World world, Vector2 position, bool female, int generation = 0)
        {
            switch (templateName)
            {
                case Name.Player:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundAll });
                        return new Player(name: templateName, world: world, position: position, animPackage: AnimPkg.Blonde, speed: 3, allowedFields: allowedFields, minDistance: 0, maxDistance: 65535, generation: generation, storageWidth: 8, storageHeight: 4);
                    }

                case Name.GrassRegular:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 50, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 150 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 180, bioWear: 0.3f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.GrassRegular, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 35, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.53f, generation: generation, staysAfterDeath: 300);
                    }

                case Name.GrassDesert:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 105, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 115) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 40 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 250 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 650, massLost: 300, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.GrassDesert, allowedFields: allowedFields,
                            minDistance: 20, maxDistance: 55, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 900, reproduction: reproduction, massToBurn: 5, massTakenMultiplier: 0.63f, generation: generation, staysAfterDeath: 300);
                    }

                case Name.Rushes:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 77, max: 95) },
                            { TerrainName.Humidity, new AllowedRange(min: 128, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 220 }, { TerrainName.Height, 92 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 500, massLost: 40, bioWear: 0.41f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.Rushes, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 120, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 600, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 0.62f, generation: generation, staysAfterDeath: 300);
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.WaterLily1, AnimPkg.WaterLily2, AnimPkg.WaterLily3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 30, max: 70) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 128) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 80 }, { TerrainName.Height, 45 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1500, massLost: 1000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: animPkg, allowedFields: allowedFields,
                            minDistance: 25, maxDistance: 80, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 2000, reproduction: reproduction, massToBurn: 4, massTakenMultiplier: 0.5f, generation: generation, staysAfterDeath: 300, floatsOnWater: true);
                    }

                case Name.FlowersPlain:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.FlowersYellow1, AnimPkg.FlowersWhite };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 140, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 180 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 400 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 700, massLost: 600, bioWear: 0.36f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: animPkg, allowedFields: allowedFields,
                          minDistance: 40, maxDistance: 70, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 550, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 1f, generation: generation, staysAfterDeath: 300);
                    }

                case Name.FlowersMountain:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min:160, max: 210) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Height, 175 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 2500, massLost: 2000, bioWear: 0.7f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.FlowersYellow2, allowedFields: allowedFields,
                          minDistance: 45, maxDistance: 120, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 4000, reproduction: reproduction, massToBurn: 3, massTakenMultiplier: 0.98f, generation: generation, staysAfterDeath: 300);
                    }

                case Name.TreeSmall:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.TreeSmall1, AnimPkg.TreeSmall2 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 105, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 80, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.3f);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                        firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 300, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 1) },
                        finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 2),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: animPkg, allowedFields: allowedFields,
                            minDistance: 50, maxDistance: 170, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 15, massTakenMultiplier: 1.35f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 50);
                    }

                case Name.TreeBig:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 400, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 2) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 4),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields,
                            minDistance: 65, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100);
                    }

                case Name.AppleTree:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Apple);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 400, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 2) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 4),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields,
                            minDistance: 65, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardAction: Scheduler.ActionName.DropFruit);
                    }

                case Name.CherryTree:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 115, max: 150) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 255) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 210 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 22000, bioWear: 0.37f);
                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 120f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Cherry);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 400, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 50, maxNumberToDrop: 2) },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 4),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.TreeBig, allowedFields: allowedFields,
                            minDistance: 65, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 35, massTakenMultiplier: 3.1f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 100, fruitEngine: fruitEngine, boardAction: Scheduler.ActionName.DropFruit);
                    }

                case Name.BananaTree:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 210) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 8000 }, { 2, 10000 }, { 3, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);
                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 250f, yOffsetPercent: -0.29f, areaWidthPercent: 0.85f, areaHeightPercent: 0.3f, fruitName: Name.Banana);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                         firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 500, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 80, maxNumberToDrop: 2) },
                         finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 6),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.PalmTree, allowedFields: allowedFields,
                            minDistance: 100, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160, fruitEngine: fruitEngine, boardAction: Scheduler.ActionName.DropFruit);
                    }

                case Name.TomatoPlant:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 100, max: 160) },
                            { TerrainName.Humidity, new AllowedRange(min: 100, max: 200) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 150 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 900 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 1300, massLost: 300, bioWear: 0.32f);
                        var fruitEngine = new FruitEngine(maxNumber: 3, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Tomato);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: false, animPackage: AnimPkg.TomatoPlant, allowedFields: allowedFields,
                          minDistance: 80, maxDistance: 150, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 1000, reproduction: reproduction, massToBurn: 9, massTakenMultiplier: 0.855f, generation: generation, staysAfterDeath: 300, fruitEngine: fruitEngine, boardAction: Scheduler.ActionName.DropFruit);
                    }

                case Name.PalmTree:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 120, max: 210) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 170 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 2500 }, { 1, 8000 }, { 2, 10000 }, { 3, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 40000, massLost: 20000, bioWear: 0.6f);

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                           firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 500, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 80, maxNumberToDrop: 2) },
                           finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.WoodLog, chanceToDrop: 100, maxNumberToDrop: 6),
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 3),});

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.PalmTree, allowedFields: allowedFields,
                            minDistance: 100, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 15000, reproduction: reproduction, massToBurn: 12, massTakenMultiplier: 1.5f, generation: generation, staysAfterDeath: 5000, yield: yield, maxHitPoints: 160);
                    }

                case Name.Cactus:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) },
                            { TerrainName.Humidity, new AllowedRange(min: 0, max: 90) }});

                        var bestEnvironment = new Dictionary<TerrainName, byte>() { { TerrainName.Humidity, 60 } };
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 10000 }, { 1, 65535 } };
                        var reproduction = new PlantReproductionData(massNeeded: 20000, massLost: 18000, bioWear: 0.69f);

                        return new Plant(name: templateName, world: world, position: position, blocksMovement: true, animPackage: AnimPkg.Cactus, allowedFields: allowedFields,
                            minDistance: 100, maxDistance: 200, bestEnvironment: bestEnvironment, mass: 1, maxMassBySize: maxMassBySize, maxAge: 30000, reproduction: reproduction, massToBurn: 10, massTakenMultiplier: 1.65f, generation: generation, staysAfterDeath: 5000);
                    }

                case Name.WaterRock:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.WaterRock1, AnimPkg.WaterRock2, AnimPkg.WaterRock3, AnimPkg.WaterRock4, AnimPkg.WaterRock5, AnimPkg.WaterRock6, AnimPkg.WaterRock7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 50, max: 74) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                      firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 350, maxNumberToDrop: 1) },
                      finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, floatsOnWater: true,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60);
                    }

                case Name.MineralsSmall:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.MineralsSmall1, AnimPkg.MineralsSmall2, AnimPkg.MineralsSmall3, AnimPkg.MineralsSmall4 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                       firstDroppedPieces: new List<Yield.DroppedPiece> {
                           new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 200, maxNumberToDrop: 1),
                           new Yield.DroppedPiece(pieceName: Name.StoneAmmo, chanceToDrop: 250, maxNumberToDrop: 1)},
                       finalDroppedPieces: new List<Yield.DroppedPiece> {
                           new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 2),
                           new Yield.DroppedPiece(pieceName: Name.StoneAmmo, chanceToDrop: 100, maxNumberToDrop: 2),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 60);
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.MineralsBig1, AnimPkg.MineralsBig2, AnimPkg.MineralsBig3, AnimPkg.MineralsBig4 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                       firstDroppedPieces: new List<Yield.DroppedPiece> {
                           new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 350, maxNumberToDrop: 1),
                           new Yield.DroppedPiece(pieceName: Name.StoneAmmo, chanceToDrop: 200, maxNumberToDrop: 1),
                       },
                       finalDroppedPieces: new List<Yield.DroppedPiece> {
                           new Yield.DroppedPiece(pieceName: Name.Stone, chanceToDrop: 100, maxNumberToDrop: 4),
                           new Yield.DroppedPiece(pieceName: Name.StoneAmmo, chanceToDrop: 100, maxNumberToDrop: 3),
                       });

                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 500, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 100);
                    }

                case Name.Backlight:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Backlight, minDistance: 0, maxDistance: 0, destructionDelay: -1, allowedFields: allowedFields, generation: generation);
                    }

                case Name.BloodSplatter:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        var packageNames = new List<AnimPkg> { AnimPkg.BloodSplatter1, AnimPkg.BloodSplatter2, AnimPkg.BloodSplatter3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: animPkg, destructionDelay: 250, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 10, generation: generation);
                    }

                case Name.Attack:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Attack, minDistance: 0, maxDistance: 3, destructionDelay: -1, allowedFields: allowedFields, generation: generation);
                    }

                case Name.Miss:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Miss, minDistance: 0, maxDistance: 3, destructionDelay: 60, allowedFields: allowedFields, generation: generation);
                    }

                case Name.Zzz:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Zzz, minDistance: 0, maxDistance: 0, destructionDelay: 60, allowedFields: allowedFields, generation: generation, fadeInAnim: true);
                    }

                case Name.Heart:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Heart, destructionDelay: 40, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, fadeInAnim: true);
                    }

                case Name.Crosshair:
                    {
                        var allowedFields = new AllowedFields(world: world);
                        return new VisualEffect(name: templateName, world: world, position: position, animPackage: AnimPkg.Crosshair, destructionDelay: 0, allowedFields: allowedFields, minDistance: 0, maxDistance: 2, generation: generation, serialize: false);
                    }

                case Name.ChestWooden:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Container(name: templateName, world: world, position: position, animPackage: AnimPkg.ChestWooden, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 3, storageHeight: 3, maxHitPoints: 40);
                    }

                case Name.ChestIron:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Container(name: templateName, world: world, position: position, animPackage: AnimPkg.ChestMetal, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, storageWidth: 4, storageHeight: 3, maxHitPoints: 50);
                    }

                case Name.RegularWorkshop:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimPkg.WoodenTable, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftNormal, maxHitPoints: 30);
                    }

                case Name.Furnace:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimPkg.Furnace, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftFurnace, maxHitPoints: 40);
                    }

                case Name.CookingPot:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Workshop(name: templateName, world: world, position: position, animPackage: AnimPkg.CookingPot, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, craftMenuTemplate: MenuTemplate.Name.CraftCooking, maxHitPoints: 30);
                    }

                case Name.Shell:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.Shell1, AnimPkg.Shell2, AnimPkg.Shell3, AnimPkg.Shell4, AnimPkg.Shell5, AnimPkg.Shell6, AnimPkg.Shell7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 98) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 200, generation: generation, stackSize: 6, rotatesWhenDropped: true);
                    }

                case Name.Stick:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.Stick1, AnimPkg.Stick2, AnimPkg.Stick3, AnimPkg.Stick4, AnimPkg.Stick5, AnimPkg.Stick6, AnimPkg.Stick7 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: false, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true);
                    }

                case Name.Stone:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.Stone1, AnimPkg.Stone2, };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: animPkg, blocksMovement: true, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, rotatesWhenDropped: true);
                    }

                case Name.WoodLog:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Wood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Stick, chanceToDrop: 100, maxNumberToDrop: 2)});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.WoodLog, blocksMovement: true, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, floatsOnWater: false, yield: yield, maxHitPoints: 5, rotatesWhenDropped: true);
                    }

                case Name.WoodPlank:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.WoodPlank, blocksMovement: true, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 12, floatsOnWater: false, yield: null, maxHitPoints: 5, rotatesWhenDropped: true);
                    }

                case Name.Nail:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.Nail, blocksMovement: false, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 50, floatsOnWater: false, yield: null, maxHitPoints: 1, rotatesWhenDropped: true);
                    }

                case Name.IronDeposit:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 400, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.IronOre, chanceToDrop: 100, maxNumberToDrop: 3)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimPkg.IronDeposit, allowedFields: allowedFields,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300);
                    }

                case Name.CoalDeposit:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 165, max: 210) }});

                        var yield = new Yield(debrisType: Yield.DebrisType.Stone,
                            firstDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 400, maxNumberToDrop: 1)},
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.Coal, chanceToDrop: 100, maxNumberToDrop: 3)});

                        return new Decoration(name: templateName, world: world, position: position, animPackage: AnimPkg.CoalDeposit, allowedFields: allowedFields,
                          minDistance: 0, maxDistance: 1000, maxMassBySize: null, generation: generation, yield: yield, maxHitPoints: 300);
                    }

                case Name.Coal:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.Coal, blocksMovement: false, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false);
                    }

                case Name.IronOre:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.IronOre, blocksMovement: false, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 8, floatsOnWater: false);
                    }

                case Name.IronBar:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.IronBar, blocksMovement: true, allowedFields: allowedFields, minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, floatsOnWater: false, rotatesWhenDropped: true);
                    }

                case Name.Apple:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoLevelMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Apple, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 70);
                    }

                case Name.Cherry:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoLevelMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Cherry, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 16, mass: 40);
                    }

                case Name.Banana:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoLevelMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Banana, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 80);
                    }

                case Name.Tomato:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoLevelMin) }});
                        return new Fruit(name: templateName, world: world, position: position, animPackage: AnimPkg.Tomato, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 10, mass: 20);
                    }

                case Name.RawMeat:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.RawMeat, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 100, toolbarAction: Scheduler.ActionName.GetEaten, rotatesWhenDropped: true);
                    }

                case Name.Leather:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 20), max: Terrain.volcanoLevelMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.Leather, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 4, mass: 100, rotatesWhenDropped: true);
                    }

                case Name.CookedMeat:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Collectible(name: templateName, world: world, position: position, animPackage: AnimPkg.CookedMeat, blocksMovement: false, allowedFields: allowedFields,
                            minDistance: 0, maxDistance: 1000, generation: generation, stackSize: 5, mass: 300, toolbarAction: Scheduler.ActionName.GetEaten, rotatesWhenDropped: true);
                    }

                case Name.Rabbit:
                    {
                        List<AnimPkg> packageNames;
                        if (female)
                        { packageNames = new List<AnimPkg> { AnimPkg.RabbitLightGray, AnimPkg.RabbitBeige, AnimPkg.RabbitWhite }; }
                        else
                        { packageNames = new List<AnimPkg> { AnimPkg.RabbitBrown, AnimPkg.RabbitDarkBrown, AnimPkg.RabbitGray, AnimPkg.RabbitBlack, AnimPkg.RabbitLightBrown }; }

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName>() { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.GroundAll });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 200 }, { 1, 500 }, { 2, 65535 } };

                        var yield = new Yield(debrisType: Yield.DebrisType.Blood,
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 200, maxNumberToDrop: 2), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 50, maxNumberToDrop: 1) });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 2.5f,
                            minDistance: 10, maxDistance: 45, maxHitPoints: 150, mass: 35, maxMass: 5000, massBurnedMultiplier: 1, awareness: 200, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 300, sightRange: 300, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.Tomato }, isEatenBy: new List<Name> { Name.Fox, Name.Player }, strength: 1, maxMassBySize: maxMassBySize, generation: generation, yield: yield);
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
                          finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 200, maxNumberToDrop: 3), new Yield.DroppedPiece(pieceName: Name.Leather, chanceToDrop: 80, maxNumberToDrop: 1) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.GroundAll });

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 2f,
                         minDistance: 5, maxDistance: 30, maxHitPoints: 300, mass: 60, maxMass: 15000, awareness: 80, female: female, massBurnedMultiplier: 1.3f, matureAge: 2000, maxAge: 30000, pregnancyDuration: 4000, maxChildren: 6, maxStamina: 800, sightRange: 400, eats: new List<Name> { Name.Rabbit }, isEatenBy: new List<Name> { Name.Player }, strength: 30, maxMassBySize: maxMassBySize, generation: generation, yield: yield);
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
                        finalDroppedPieces: new List<Yield.DroppedPiece> { new Yield.DroppedPiece(pieceName: Name.RawMeat, chanceToDrop: 200, maxNumberToDrop: 1) });

                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.WaterMedium, AllowedFields.RangeName.GroundSand });
                        var maxMassBySize = new Dictionary<byte, int>() { { 0, 300 }, { 1, 800 }, { 2, 65535 } };

                        return new Animal(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, speed: 2.5f,
                       minDistance: 5, maxDistance: 30, maxHitPoints: 50, mass: 10, maxMass: 1200, massBurnedMultiplier: 1, awareness: 100, female: female, matureAge: 1200, maxAge: 30000, pregnancyDuration: 2000, maxChildren: 8, maxStamina: 200, sightRange: 150, eats: new List<Name> { Name.WaterLily, Name.Rushes }, isEatenBy: new List<Name> { Name.Player }, strength: 1, maxMassBySize: maxMassBySize, generation: generation, yield: yield);
                    }

                case Name.Crab:
                    // TODO use "female" here
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.CrabRed, AnimPkg.CrabBrown, AnimPkg.CrabGreen, AnimPkg.CrabLightBlue, AnimPkg.CrabGray, AnimPkg.CrabYellow, AnimPkg.CrabBeige, AnimPkg.CrabDarkBrown };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.WaterShallow, AllowedFields.RangeName.GroundAll });
                        return new Decoration(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, maxMassBySize: null, generation: generation);
                    }

                case Name.Hand:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Wood, 0.5f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.Hand, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: true, multiplierByCategory: multiplierByCategory, maxHitPoints: 1);
                    }

                case Name.AxeWood:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Metal, 1f }, { Tool.TargetCategory.Stone, 1f }, { Tool.TargetCategory.Wood, 5f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.AxeWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75);
                    }

                case Name.AxeStone:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Metal, 2f }, { Tool.TargetCategory.Stone, 2f }, { Tool.TargetCategory.Wood, 10f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.AxeStone, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150);
                    }

                case Name.AxeIron:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Metal, 3f }, { Tool.TargetCategory.Stone, 3f }, { Tool.TargetCategory.Wood, 20 } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.AxeIron, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 250);
                    }

                case Name.PickaxeWood:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Metal, 1f }, { Tool.TargetCategory.SmallPlant, 0.0f }, { Tool.TargetCategory.Stone, 5f }, { Tool.TargetCategory.Wood, 1f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.PickaxeWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 75);
                    }

                case Name.PickaxeStone:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Metal, 2f }, { Tool.TargetCategory.Stone, 10f }, { Tool.TargetCategory.Wood, 2f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.PickaxeStone, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150);
                    }

                case Name.PickaxeIron:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Metal, 3f }, { Tool.TargetCategory.Stone, 20f }, { Tool.TargetCategory.Wood, 3f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.PickaxeIron, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 1, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 250);
                    }

                case Name.BatWood:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Animal, 4f } };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.BatWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150);
                    }

                case Name.Sling:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Animal, 3f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.StoneAmmo };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.Sling, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 3, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 80, shootsProjectile: true, compatibleAmmo: compatibleAmmo);
                    }

                case Name.GreatSling:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Animal, 3f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.StoneAmmo };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.GreatSling, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 4, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 120, shootsProjectile: true, compatibleAmmo: compatibleAmmo);
                    }

                case Name.BowWood:
                    {
                        var multiplierByCategory = new Dictionary<Tool.TargetCategory, float> { { Tool.TargetCategory.Animal, 3f } };
                        var compatibleAmmo = new List<PieceTemplate.Name> { Name.ArrowWood, Name.ArrowIron };

                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});
                        return new Tool(name: templateName, world: world, position: position, animPackage: AnimPkg.BowWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, hitPower: 5, indestructible: false, multiplierByCategory: multiplierByCategory, maxHitPoints: 150, shootsProjectile: true, compatibleAmmo: compatibleAmmo);
                    }

                case Name.StoneAmmo:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});
                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimPkg.StoneSmall, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 6, indestructible: false, maxHitPoints: 10, stackSize: 10, canBeStuck: false);
                    }

                case Name.ArrowWood:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});
                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimPkg.ArrowWood, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 10, indestructible: false, maxHitPoints: 25, stackSize: 15, canBeStuck: true);
                    }

                case Name.ArrowIron:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: 0, max: 255) }});
                        return new Projectile(name: templateName, world: world, position: position, animPackage: AnimPkg.ArrowIron, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 100, maxMassBySize: null, generation: generation, baseHitPower: 20, indestructible: false, maxHitPoints: 40, stackSize: 15, canBeStuck: true);
                    }

                case Name.DebrisStone:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimPkg.DebrisStone, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null);
                    }

                case Name.DebrisWood:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: AnimPkg.DebrisWood, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null);
                    }

                case Name.BloodDrop:
                    {
                        var packageNames = new List<AnimPkg> { AnimPkg.BloodDrop1, AnimPkg.BloodDrop2, AnimPkg.BloodDrop3 };
                        var animPkg = packageNames[world.random.Next(0, packageNames.Count)];

                        var allowedFields = new AllowedFields(world: world, rangeNameList: new List<AllowedFields.RangeName> { AllowedFields.RangeName.All });

                        return new Debris(name: templateName, world: world, position: position, animPackage: animPkg, allowedFields: allowedFields, floatsOnWater: true, minDistance: 0, maxDistance: 500, maxMassBySize: null, destructionDelay: 180, rotatesWhenDropped: false);
                    }

                case Name.TentSmall:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.14f, canBeAttacked: false);

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimPkg.TentSmall, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine);
                    }

                case Name.TentMedium:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.3f, fatigueRegen: 0.2f, canBeAttacked: false);

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimPkg.TentMedium, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 120, sleepEngine: sleepEngine);
                    }

                case Name.TentBig:
                    {
                        var allowedFields = new AllowedFields(world: world, rangeDict: new Dictionary<TerrainName, AllowedRange>() {
                            { TerrainName.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.volcanoLevelMin) }});

                        SleepEngine sleepEngine = new SleepEngine(minFedPercent: 0.5f, fatigueRegen: 0.4f, canBeAttacked: false);

                        return new Shelter(name: templateName, world: world, position: position, animPackage: AnimPkg.TentBig, allowedFields: allowedFields, floatsOnWater: false, minDistance: 0, maxDistance: 500, maxMassBySize: null, maxHitPoints: 200, sleepEngine: sleepEngine);
                    }

                default: { throw new DivideByZeroException($"Unsupported template name - {templateName}."); }

            }
        }

    }
}
