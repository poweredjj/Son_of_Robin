using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin

{
    public class PieceTemplate
    {
        public enum Name : ushort
        {
            Empty = 0, // to be used instead of null

            PlayerBoy = 1,
            PlayerGirl = 2,
            PlayerTestDemoness = 3,
            PlayerGhost = 4,

            GrassRegular = 5,
            GrassGlow = 6,
            GrassDesert = 7,

            PlantPoison = 8,
            Rushes = 9,
            WaterLily = 10,

            FlowersPlain = 11,
            FlowersRed = 12,
            FlowersMountain = 13,

            Cactus = 14,
            MushroomPlant = 226,

            SeedsGeneric = 15,
            CoffeeRaw = 16,
            CoffeeRoasted = 17,

            TreeSmall = 18,
            TreeBig = 19,
            PalmTree = 20,
            Oak = 21,
            AppleTree = 22,
            CherryTree = 23,
            BananaTree = 24,

            CarrotPlant = 25,
            TomatoPlant = 26,
            CoffeeShrub = 27,

            Apple = 28,
            Banana = 29,
            Cherry = 30,
            Tomato = 31,
            Carrot = 32,
            Acorn = 33,
            Mushroom = 229,
            MeatRawRegular = 34,
            MeatRawPrime = 35,
            MeatDried = 36,
            Fat = 37,
            Leather = 38,

            Burger = 39,
            Meal = 40,

            Rabbit = 41,
            Fox = 42,
            Tiger = 43,
            Bear = 225,
            Frog = 44,

            MineralsBig = 45,
            MineralsSmall = 46,
            MineralsMossyBig = 47,
            MineralsMossySmall = 48,
            CaveWeakMinerals = 224,

            JarTreasureRich = 49,
            JarTreasurePoor = 50,
            JarBroken = 51,
            CrateStarting = 52,
            CrateRegular = 53,

            ChestWooden = 54,
            ChestStone = 55,
            ChestIron = 56,
            ChestCrystal = 57,
            ChestTreasureNormal = 58,
            ChestTreasureBig = 59,

            CampfireSmall = 60,
            CampfireMedium = 61,

            FertileGroundSmall = 62,
            FertileGroundMedium = 63,
            FertileGroundLarge = 64,

            WorkshopEssential = 65,
            WorkshopBasic = 66,
            WorkshopAdvanced = 67,
            WorkshopMaster = 68,

            WorkshopMeatHarvesting = 69,
            MeatDryingRackRegular = 70,
            MeatDryingRackWide = 71,
            WorkshopLeatherBasic = 72,
            WorkshopLeatherAdvanced = 73,

            AlchemyLabStandard = 74,
            AlchemyLabAdvanced = 75,

            BoatConstructionSite = 231,
            BoatComplete = 232,

            Totem = 76,
            RuinsColumn = 77,
            RuinsRubble = 78,
            RuinsWall = 79,

            FenceHorizontalShort = 80,
            FenceVerticalShort = 81,
            FenceHorizontalLong = 82,
            FenceVerticalLong = 83,

            Furnace = 84,
            Anvil = 85,
            HotPlate = 86,
            CookingPot = 87,

            Stick = 88,
            WoodLogRegular = 89,
            WoodLogHard = 90,
            WoodPlank = 91,
            Stone = 92,
            Granite = 93,
            Clay = 94,
            Rope = 95,
            Clam = 96,

            CoalDeposit = 97,
            IronDeposit = 98,

            BeachDigSite = 99,
            ForestDigSite = 100,
            DesertDigSite = 101,
            GlassDigSite = 102,
            SwampDigSite = 103,
            RuinsDigSite = 104,
            SwampGeyser = 105,
            SwampGas = 106,

            CrystalDepositSmall = 107,
            CrystalDepositBig = 108,

            Coal = 109,
            IronOre = 110,
            IronBar = 111,
            IronRod = 112,
            IronNail = 113,
            IronPlate = 114,
            GlassSand = 115,
            Crystal = 116,

            Backlight = 117,
            BloodSplatter = 118,
            Attack = 119,
            Miss = 120,
            Zzz = 121,
            Heart = 122,
            MapMarker = 123,
            MusicNote = 124,
            Crosshair = 125,

            BubbleExclamationRed = 126,
            BubbleExclamationBlue = 127,
            BubbleCraftGreen = 128,

            Explosion = 130,

            CookingTrigger = 131,
            UpgradeTrigger = 132,
            BrewTrigger = 133,
            MeatHarvestTrigger = 134,
            OfferTrigger = 135,
            ConstructTrigger = 230,
            FireplaceTriggerOn = 136,
            FireplaceTriggerOff = 137,

            KnifeSimple = 138,
            AxeWood = 139,
            AxeStone = 140,
            AxeIron = 141,
            AxeCrystal = 142,

            PickaxeWood = 143,
            PickaxeStone = 144,
            PickaxeIron = 145,
            PickaxeCrystal = 146,

            SpearWood = 147,
            SpearStone = 148,
            SpearIron = 149,
            SpearCrystal = 150,

            ScytheStone = 151,
            ScytheIron = 152,
            ScytheCrystal = 153,

            ShovelStone = 154,
            ShovelIron = 155,
            ShovelCrystal = 156,

            BowBasic = 157,
            BowAdvanced = 158,

            ArrowWood = 159,
            ArrowStone = 160,
            ArrowIron = 161,
            ArrowCrystal = 162,
            ArrowExploding = 163,

            TentModern = 216,
            TentModernPacked = 217,
            TentSmall = 164,
            TentMedium = 165,
            TentBig = 166,

            BackpackSmall = 167,
            BackpackMedium = 168,
            BackpackBig = 169,
            BackpackLuxurious = 170,

            BeltSmall = 171,
            BeltMedium = 172,
            BeltBig = 173,
            BeltLuxurious = 174,
            Map = 175,

            Dungarees = 176,
            HatSimple = 177,
            BootsProtective = 178,
            BootsMountain = 179,
            BootsAllTerrain = 180,
            BootsSpeed = 181,
            GlovesStrength = 182,
            GlassesVelvet = 183,

            TorchSmall = 184,
            TorchBig = 185,

            LanternEmpty = 186,
            LanternFull = 187,
            Candle = 188,

            HumanSkeleton = 189,

            HerbsBlack = 190,
            HerbsBlue = 191,
            HerbsCyan = 192,
            HerbsGreen = 193,
            HerbsYellow = 194,
            HerbsRed = 195,
            HerbsViolet = 196,
            HerbsBrown = 197,
            HerbsDarkViolet = 198,
            HerbsDarkGreen = 199,

            EmptyBottle = 200,
            PotionGeneric = 201,
            BottleOfOil = 202,

            Hole = 203,
            TreeStump = 204,

            LavaFlame = 205,
            LavaGas = 206,

            SoundLakeWaves = 207,
            SoundSeaWind = 208,
            SoundNightCrickets = 209,
            SoundNoonCicadas = 210,
            SoundLava = 211,
            SoundCaveWaterDrip = 228,

            SeaWave = 212,
            PredatorRepellant = 213,
            WeatherFog = 219,

            ParticleEmitterEnding = 214,
            ParticleEmitterWeather = 218,
            EmptyVisualEffect = 215,
            HastePlayerClone = 220,

            CaveEntranceOutside = 221,
            CaveEntranceInside = 223,
            CaveExit = 222,
            CaveExitEmergency = 227,

            // obsolete below (kept for compatibility with old saves)
        }

        public static readonly Name[] allNames = (Name[])Enum.GetValues(typeof(Name));

        public static BoardPiece CreateAndPlaceOnBoard(Name templateName, World world, Vector2 position, bool randomPlacement = false, bool ignoreCollisions = false, bool precisePlacement = false, int id = -1, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false, bool createdByPlayer = false)
        {
            BoardPiece boardPiece = CreatePiece(templateName: templateName, world: world, id: id);

            if (createdByPlayer)
            {
                boardPiece.createdByPlayer = true;
                boardPiece.canBeHit = false; // to protect item from accidental player hit
            }

            boardPiece.PlaceOnBoard(randomPlacement: randomPlacement, position: position, ignoreCollisions: ignoreCollisions, precisePlacement: precisePlacement, closestFreeSpot: closestFreeSpot, minDistanceOverride: minDistanceOverride, maxDistanceOverride: maxDistanceOverride, ignoreDensity: ignoreDensity, addPlannedDestruction: true);

            if (boardPiece.sprite.IsOnBoard)
            {
                // duplicated in Yield
                boardPiece.activeSoundPack.Play(PieceSoundPackTemplate.Action.HasAppeared);
                boardPiece.pieceInfo.appearDebris?.DropDebris(piece: boardPiece);

                // adding opacityFade
                if (boardPiece.sprite.IsInCameraRect && boardPiece.pieceInfo.inOpacityFadeDuration > 0)
                {
                    float destOpacity = boardPiece.sprite.opacity;
                    boardPiece.sprite.opacity = 0f;
                    new OpacityFade(sprite: boardPiece.sprite, destOpacity: destOpacity, duration: boardPiece.pieceInfo.inOpacityFadeDuration);
                }
            }

            return boardPiece;
        }

        public static BoardPiece CreatePiece(Name templateName, World world, int id = -1)
        {
            if (id == -1) id = Helpers.GetUniqueID();

            switch (templateName)
            {
                case Name.Empty:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, allowedTerrain: allowedTerrain, readableName: "empty", description: "Should not be used.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.PlayerBoy:
                    {
                        BoardPiece boardPiece = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerBoy, allowedTerrain: CreatePlayerAllowedTerrain(), readableName: "boy", description: "This is you.", activeState: BoardPiece.State.PlayerControlledWalking);

                        return boardPiece;
                    }

                case Name.PlayerGirl:
                    {
                        BoardPiece boardPiece = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerGirl, allowedTerrain: CreatePlayerAllowedTerrain(), readableName: "girl", description: "This is you.", activeState: BoardPiece.State.PlayerControlledWalking);

                        return boardPiece;
                    }

                case Name.PlayerTestDemoness:
                    {
                        Player boardPiece = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerTestDemoness, allowedTerrain: CreatePlayerAllowedTerrain(), readableName: "demoness", description: "This is you.", activeState: BoardPiece.State.PlayerControlledWalking)
                        {
                            maxFatigue = 50000,
                            maxHitPoints = 100000,
                            maxFedLevel = 400000,
                            speed = 8f,
                            strength = 100,
                        };

                        boardPiece.fedLevel = boardPiece.maxFedLevel;
                        boardPiece.HitPoints = boardPiece.maxHitPoints;
                        boardPiece.sprite.lightEngine = new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.Red * 1f, isActive: false, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        return boardPiece;
                    }

                case Name.PlayerGhost:
                    {
                        Player spectator = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, allowedTerrain: new AllowedTerrain(), readableName: "player ghost", description: "A metaphysical representation of player's soul.", activeState: BoardPiece.State.PlayerControlledGhosting);

                        spectator.sprite.lightEngine = new LightEngine(size: 650, opacity: 1.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        spectator.sprite.lightEngine.AssignSprite(spectator.sprite);
                        spectator.speed = 5;
                        spectator.sprite.opacity = 0.5f;
                        spectator.sprite.color = new Color(150, 255, 255);
                        spectator.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.SkyBlue * 0.7f, textureSize: spectator.sprite.AnimFrame.textureSize, priority: 0, framesLeft: -1));

                        return spectator;
                    }

                case Name.GrassRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 96, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 50, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain, maxAge: 600, massTakenMultiplier: 0.53f, readableName: "regular grass", description: "A regular grass.");

                        return boardPiece;
                    }

                case Name.GrassGlow:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 96, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 50, max: 255) }});

                        // readableName is the same as "regular grass", to make it appear identical to the regular grass

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain,
                            maxAge: 1000, massTakenMultiplier: 0.49f, readableName: "regular grass", description: "A special type of grass.", lightEngine: new LightEngine(size: 0, opacity: 0.3f, colorActive: true, color: Color.Blue * 3f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: true, castShadows: false));

                        return boardPiece;
                    }

                case Name.GrassDesert:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 115) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GrassDesert, allowedTerrain: allowedTerrain,
                             maxAge: 900, massTakenMultiplier: 0.63f, readableName: "desert grass", description: "A grass, that grows on sand.");

                        return boardPiece;
                    }

                case Name.PlantPoison:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                                { Terrain.Name.Biome, new AllowedRange(min: (byte)(Terrain.biomeMin + (255 - Terrain.biomeMin) / 2), max:255 ) },
                                { Terrain.Name.Height, new AllowedRange(min: 112, max:156 ) },
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlantPoison, allowedTerrain: allowedTerrain,
                            maxAge: 950, massTakenMultiplier: 0.63f, readableName: "poisonous plant", description: "Poisonous plant.");

                        return boardPiece;
                    }

                case Name.Rushes:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 77, max: 95) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 128, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Rushes, allowedTerrain: allowedTerrain, maxAge: 600, massTakenMultiplier: 0.62f, readableName: "rushes", description: "A water plant.");

                        return boardPiece;
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WaterLily1, AnimData.PkgName.WaterLily2, AnimData.PkgName.WaterLily3 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>{
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 12), max: (byte)(Terrain.waterLevelMax - 1)) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 140) },
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                            maxAge: 1800, massTakenMultiplier: 0.4f, readableName: "water lily", description: "A water plant.", maxHitPoints: 10);

                        return boardPiece;
                    }

                case Name.FlowersPlain:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.FlowersYellow1, AnimData.PkgName.FlowersWhite };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 140, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                           maxAge: 550, massTakenMultiplier: 1f, readableName: "regular flower", description: "A flower.");

                        return boardPiece;
                    }

                case Name.FlowersRed:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 140, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FlowersRed, allowedTerrain: allowedTerrain,
                           maxAge: 550, massTakenMultiplier: 1f, readableName: "red flower", description: "A red flower.");

                        return boardPiece;
                    }

                case Name.FlowersMountain:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min:Terrain.rocksLevelMin, max: Terrain.volcanoEdgeMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FlowersYellow2, allowedTerrain: allowedTerrain,
                         maxAge: 4000, massTakenMultiplier: 0.98f, readableName: "mountain flower", description: "A mountain flower.");

                        return boardPiece;
                    }

                case Name.TreeSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.TreeSmall1, AnimData.PkgName.TreeSmall2 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 80, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                             maxAge: 30000, massTakenMultiplier: 1.35f, maxHitPoints: 50, readableName: "small tree", description: "A small tree.");

                        return boardPiece;
                    }

                case Name.TreeBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, readableName: "big tree", description: "A big tree.");

                        return boardPiece;
                    }

                case Name.Oak:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Acorn);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, fruitEngine: fruitEngine, readableName: "oak", description: "Acorns can grow on it.");

                        return boardPiece;
                    }

                case Name.AppleTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Apple);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, fruitEngine: fruitEngine, readableName: "apple tree", description: "Apples can grow on it.");

                        return boardPiece;
                    }

                case Name.CherryTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 120f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Cherry);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, fruitEngine: fruitEngine, readableName: "cherry tree", description: "Cherries can grow on it.");

                        return boardPiece;
                    }

                case Name.BananaTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 250f, yOffsetPercent: -0.29f, areaWidthPercent: 0.85f, areaHeightPercent: 0.3f, fruitName: Name.Banana);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PalmTree, allowedTerrain: allowedTerrain,
                            maxAge: 15000, massTakenMultiplier: 1.5f, maxHitPoints: 160, fruitEngine: fruitEngine, readableName: "banana tree", description: "Bananas can grow on it.");

                        return boardPiece;
                    }

                case Name.PalmTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PalmTree, allowedTerrain: allowedTerrain,
                            maxAge: 15000, massTakenMultiplier: 1.5f, maxHitPoints: 160, readableName: "palm tree", description: "A palm tree.");

                        return boardPiece;
                    }

                case Name.TomatoPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 200) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 50f, yOffsetPercent: -0.05f, areaWidthPercent: 0.85f, areaHeightPercent: 0.8f, fruitName: Name.Tomato);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TomatoPlant, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, fruitEngine: fruitEngine, readableName: "tomato plant", description: "Tomatoes can grow on it.");

                        return boardPiece;
                    }

                case Name.CoffeeShrub:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 80, max: 140) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 250) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 2, oneFruitMass: 50f, yOffsetPercent: -0.05f, areaWidthPercent: 0.85f, areaHeightPercent: 0.8f, fruitName: Name.CoffeeRaw);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeShrub, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, fruitEngine: fruitEngine, readableName: "coffee shrub", description: "Coffee can grow on it.");

                        return boardPiece;
                    }

                case Name.CarrotPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 200) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 1, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Carrot, hiddenFruits: true);

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CarrotPlant, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, fruitEngine: fruitEngine, readableName: "carrot plant", description: "Carrots can grow on it.");

                        return boardPiece;
                    }

                case Name.Cactus:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 99, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Cactus, allowedTerrain: allowedTerrain,
                            maxAge: 30000, maxHitPoints: 80, massTakenMultiplier: 1.65f, readableName: "cactus", description: "A desert plant.");

                        return boardPiece;
                    }

                case Name.MushroomPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.volcanoEdgeMin - 1) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 255) },
                        });

                        BoardPiece boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MushroomPlant, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, readableName: "mushroom plant", description: "Growing mushrooms.");

                        return boardPiece;
                    }

                case Name.MineralsSmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsSmall1, AnimData.PkgName.MineralsSmall2, AnimData.PkgName.MineralsSmall3, AnimData.PkgName.MineralsMossySmall4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                               maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.");

                        return boardPiece;
                    }

                case Name.MineralsMossySmall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsMossySmall1, AnimData.PkgName.MineralsMossySmall2, AnimData.PkgName.MineralsMossySmall3, AnimData.PkgName.MineralsMossySmall4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 129, max: 255) },
                        });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                              maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.");

                        return boardPiece;
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsBig1, AnimData.PkgName.MineralsBig2, AnimData.PkgName.MineralsBig3, AnimData.PkgName.MineralsBig4, AnimData.PkgName.MineralsBig5, AnimData.PkgName.MineralsBig6, AnimData.PkgName.MineralsBig7, AnimData.PkgName.MineralsBig8, AnimData.PkgName.MineralsBig9, AnimData.PkgName.MineralsBig10, AnimData.PkgName.MineralsBig11, AnimData.PkgName.MineralsBig12, AnimData.PkgName.MineralsBig13, AnimData.PkgName.MineralsBig14 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                              maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.");

                        return boardPiece;
                    }

                case Name.MineralsMossyBig:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.MineralsMossyBig1, AnimData.PkgName.MineralsMossyBig2, AnimData.PkgName.MineralsMossyBig3, AnimData.PkgName.MineralsMossyBig4, AnimData.PkgName.MineralsMossyBig5, AnimData.PkgName.MineralsMossyBig6, AnimData.PkgName.MineralsMossyBig7, AnimData.PkgName.MineralsMossyBig8, AnimData.PkgName.MineralsMossyBig9, AnimData.PkgName.MineralsMossyBig10, AnimData.PkgName.MineralsMossyBig11, AnimData.PkgName.MineralsMossyBig12 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 129, max: 255) },
                        });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                              maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.");

                        return boardPiece;
                    }

                case Name.Backlight:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Backlight, allowedTerrain: allowedTerrain, readableName: "backlight", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.BloodSplatter:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.BloodSplatter1, AnimData.PkgName.BloodSplatter2, AnimData.PkgName.BloodSplatter3 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];
                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                             readableName: "bloodSplatter", description: "A pool of blood.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.Attack:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Attack, allowedTerrain: allowedTerrain, readableName: "attack", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.MapMarker:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MapMarker, allowedTerrain: allowedTerrain, readableName: "map marker", description: "Map marker.", activeState: BoardPiece.State.MapMarkerShowAndCheck, visible: false);

                        return boardPiece;
                    }

                case Name.Miss:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Miss, allowedTerrain: allowedTerrain, readableName: "miss", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.Zzz:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Zzz, allowedTerrain: allowedTerrain, readableName: "zzz", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return visualEffect;
                    }

                case Name.Heart:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Heart, allowedTerrain: allowedTerrain, readableName: "heart", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.MusicNote:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MusicNoteSmall, allowedTerrain: allowedTerrain, readableName: "music note", description: "Sound visual.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.Crosshair:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crosshair, allowedTerrain: allowedTerrain, readableName: "crosshair", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return boardPiece;
                    }

                case Name.BubbleExclamationRed:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleExclamationRed, allowedTerrain: allowedTerrain, readableName: "red exclamation", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return visualEffect;
                    }

                case Name.BubbleExclamationBlue:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleExclamationBlue, allowedTerrain: allowedTerrain, readableName: "blue exclamation", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return visualEffect;
                    }

                case Name.BubbleCraftGreen:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleCraftGreen, allowedTerrain: allowedTerrain, readableName: "green exclamation with plus", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        return visualEffect;
                    }

                case Name.CookingTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "cooking starter", description: "Starts cooking.");

                        return boardPiece;
                    }

                case Name.BrewTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "brewing starter", description: "Starts brewing.");

                        return boardPiece;
                    }

                case Name.UpgradeTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Upgrade, allowedTerrain: allowedTerrain, readableName: "upgrade", description: "Upgrades item.");

                        return boardPiece;
                    }

                case Name.MeatHarvestTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.KnifeSimple, allowedTerrain: allowedTerrain, readableName: "harvest", description: "Harvests meat from the animal.");

                        return boardPiece;
                    }

                case Name.OfferTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "offer", description: "Offers gifts to gods.");

                        return boardPiece;
                    }

                case Name.ConstructTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hammer, allowedTerrain: allowedTerrain, readableName: "construct", description: "Starts next construction level.");

                        return boardPiece;
                    }

                case Name.FireplaceTriggerOn:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "fireplace on", description: "Ignites the fireplace.");

                        return boardPiece;
                    }

                case Name.FireplaceTriggerOff:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        BoardPiece boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WaterDrop, allowedTerrain: allowedTerrain, readableName: "fireplace off", description: "Extinginguishes fire.");

                        return boardPiece;
                    }

                case Name.CrateStarting:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crate, allowedTerrain: allowedTerrain,
                              maxHitPoints: 5, readableName: "supply crate", description: "Contains valuable items.");

                        return boardPiece;
                    }

                case Name.CrateRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crate, allowedTerrain: allowedTerrain,
                              maxHitPoints: 40, readableName: "supply crate", description: "Contains valuable items.");

                        return boardPiece;
                    }

                case Name.ChestWooden:
                    {
                        byte storageWidth = 3;
                        byte storageHeight = 2;

                        BoardPiece boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestWooden, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 40, readableName: "wooden chest", description: $"Can store items ({storageWidth}x{storageHeight}).");

                        return boardPiece;
                    }

                case Name.ChestStone:
                    {
                        byte storageWidth = 4;
                        byte storageHeight = 4;

                        BoardPiece boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestStone, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 50, readableName: "stone chest", description: $"Can store items ({storageWidth}x{storageHeight}).");

                        return boardPiece;
                    }

                case Name.ChestIron:
                    {
                        byte storageWidth = 6;
                        byte storageHeight = 4;

                        BoardPiece boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestIron, allowedTerrain: AllowedTerrain.GetFieldCraft(), storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 60, readableName: "iron chest", description: $"Can store items ({storageWidth}x{storageHeight}).");

                        return boardPiece;
                    }

                case Name.ChestCrystal:
                    {
                        BoardPiece boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestCrystal, allowedTerrain: AllowedTerrain.GetFieldCraft(), storageWidth: 1, storageHeight: 1, maxHitPoints: 300, readableName: "crystal chest", description: "All crystal chests share their contents.");

                        return boardPiece;
                    }

                case Name.ChestTreasureNormal:
                    {
                        var treasureChest = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureBlue, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), storageWidth: 2, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", animName: "closed");

                        // this yield is used to randomize chest contents every time
                        var chestContentsYield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.SpearIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ScytheIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ShovelIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.IronBar, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.BowAdvanced, chanceToDrop: 15, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.EmptyBottle, chanceToDrop: 2, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 10, maxNumberToDrop: 1),
                            });

                        while (true)
                        {
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces(piece: treasureChest);

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
                        var treasureChest = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureRed, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), storageWidth: 3, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", animName: "closed");

                        // this yield is used to randomize chest contents every time
                        var chestContentsYield = new Yield(firstDebrisTypeList: new List<ParticleEngine.Preset> { ParticleEngine.Preset.DebrisWood },
                            firstDroppedPieces: new List<Yield.DroppedPiece> { },
                            finalDroppedPieces: new List<Yield.DroppedPiece> {
                                new Yield.DroppedPiece(pieceName: Name.AxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.PickaxeIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.SpearIron, chanceToDrop: 30, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ScytheIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.ShovelIron, chanceToDrop: 10, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.IronBar, chanceToDrop: 10, maxNumberToDrop: 3),
                                new Yield.DroppedPiece(pieceName: Name.EmptyBottle, chanceToDrop: 4, maxNumberToDrop: 1),
                                new Yield.DroppedPiece(pieceName: Name.Crystal, chanceToDrop: 10, maxNumberToDrop: 3),
                            });

                        while (true)
                        {
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces(piece: treasureChest);

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

                case Name.JarTreasureRich:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        Decoration decoration = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarWhole, allowedTerrain: allowedTerrain,
                              maxHitPoints: 40, readableName: "sealed jar", description: "Contains supplies.");

                        return decoration;
                    }

                case Name.JarTreasurePoor:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        Decoration decoration = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarWhole, allowedTerrain: allowedTerrain,
                              maxHitPoints: 40, readableName: "sealed jar", description: "Contains supplies.");

                        return decoration;
                    }

                case Name.JarBroken:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarBroken, allowedTerrain: allowedTerrain,
                              maxHitPoints: 20, readableName: "broken jar", description: "Broken Jar.");

                        return boardPiece;
                    }

                case Name.WorkshopEssential:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopEssential, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftEssential, maxHitPoints: 30, readableName: "essential workshop", description: "Essential crafting workshop.", canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.WorkshopBasic:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopBasic, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftBasic, maxHitPoints: 30, readableName: "basic workshop", description: "Basic crafting workshop.", canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.WorkshopAdvanced:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopAdvanced, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftAdvanced, maxHitPoints: 80, readableName: "advanced workshop", description: "Advanced crafting workshop.", canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.WorkshopMaster:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopMaster, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftMaster, maxHitPoints: 80, readableName: "master workshop", description: "Master's crafting workshop.", canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.WorkshopLeatherBasic:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherBasic, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftLeatherBasic, maxHitPoints: 30, readableName: "basic leather workshop", description: "For making basic items out of leather.", canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.WorkshopLeatherAdvanced:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherAdvanced, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftLeatherAdvanced, maxHitPoints: 30, readableName: "advanced leather workshop", description: "For making advanced items out of leather.", canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.Furnace:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Furnace, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftFurnace, maxHitPoints: 40, readableName: "furnace", description: "For ore smelting.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 0.7f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), canBeUsedDuringRain: false);

                        return boardPiece;
                    }

                case Name.Anvil:
                    {
                        BoardPiece boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Anvil, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftAnvil, maxHitPoints: 80, readableName: "anvil", description: "For metal forming.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 1f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), canBeUsedDuringRain: true);

                        return boardPiece;
                    }

                case Name.HotPlate:
                    {
                        var hotPlate = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HotPlate, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 20, foodMassMultiplier: 2.6f, readableName: "hot plate", description: "For cooking.", ingredientSpace: 1, canBeUsedDuringRain: false);

                        hotPlate.sprite.AssignNewName("off");
                        return hotPlate;
                    }

                case Name.CookingPot:
                    {
                        var cookingPot = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CookingPot, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, foodMassMultiplier: 3.2f, readableName: "cooking pot", description: "For cooking. Can be used during rain.", ingredientSpace: 3, canBeUsedDuringRain: true);

                        cookingPot.sprite.AssignNewName("off");
                        return cookingPot;
                    }

                case Name.AlchemyLabStandard:
                    {
                        var alchemyLab = new AlchemyLab(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AlchemyLabStandard, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "alchemy lab", description: "For potion brewing.", boosterSpace: 1);

                        alchemyLab.sprite.AssignNewName("off");
                        return alchemyLab;
                    }

                case Name.AlchemyLabAdvanced:
                    {
                        var alchemyLab = new AlchemyLab(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AlchemyLabAdvanced, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "advanced alchemy lab", description: "For advanced potion brewing.", boosterSpace: 3);

                        alchemyLab.sprite.AssignNewName("off");
                        return alchemyLab;
                    }

                case Name.WorkshopMeatHarvesting:
                    {
                        var meatHarvestingWorkshop = new MeatHarvestingWorkshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopMeatHarvesting, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "meat workshop", description: "For animal processing.");

                        meatHarvestingWorkshop.sprite.AssignNewName("off");
                        return meatHarvestingWorkshop;
                    }

                case Name.MeatDryingRackRegular:
                    {
                        var meatDryingRack = new MeatDryingRack(name: templateName, storageWidth: 2, storageHeight: 2, world: world, id: id, animPackage: AnimData.PkgName.MeatDryingRackRegular, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "meat drying rack", description: "Regular rack for meat drying.");

                        meatDryingRack.sprite.AssignNewName("off");
                        return meatDryingRack;
                    }

                case Name.MeatDryingRackWide:
                    {
                        var meatDryingRack = new MeatDryingRack(name: templateName, storageWidth: 3, storageHeight: 2, world: world, id: id, animPackage: AnimData.PkgName.MeatDryingRackWide, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 50, readableName: "meat drying rack (wide)", description: "Wide rack for meat drying.");

                        meatDryingRack.sprite.AssignNewName("off");
                        return meatDryingRack;
                    }

                case Name.Totem:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                                { Terrain.Name.Biome, new AllowedRange(min: (byte)(Terrain.biomeMin + (255 - Terrain.biomeMin) / 2), max:255 ) },
                            },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        var totem = new Totem(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Totem, allowedTerrain: allowedTerrain, maxHitPoints: 30, readableName: "totem", description: "A totem with magical powers.");

                        return totem;
                    }

                case Name.BoatConstructionSite:
                    {
                        Dictionary<int, Dictionary<Name, int>> ingredientsForLevels = new()
                        {
                            { 0, new Dictionary<Name, int>{
                                { Name.WoodPlank, 7 },
                                { Name.IronNail, 3 },
                            } },

                            { 1, new Dictionary<Name, int>{
                                { Name.IronPlate, 4 },
                                { Name.BottleOfOil, 8 },
                            } },

                            { 2, new Dictionary<Name, int>{
                                { Name.IronRod, 6 },
                                { Name.Leather, 8 },
                            } },

                            // TODO add more levels
                        };

                        Dictionary<int, string> descriptionsForLevels = new()
                        {
                            { 0, "basic frame" },
                            { 1, "sturdy hull" },
                            { 2, "sailing rig" },
                        };

                        // TODO add each level description

                        // TODO add inventory open sound

                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        BoardPiece boardPiece = new ConstructionSite(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BoatConstruction, allowedTerrain: allowedTerrain, materialsForLevels: ingredientsForLevels, descriptionsForLevels: descriptionsForLevels, convertsIntoWhenFinished: Name.BoatComplete, readableName: "boat construction site", description: "Boat construction site.");

                        return boardPiece;
                    }

                case Name.BoatComplete:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BoatComplete, allowedTerrain: allowedTerrain,
                             rotatesWhenDropped: true, readableName: "boat", description: "Can be used to escape the island.");

                        return boardPiece;
                    }

                case Name.RuinsColumn:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() { { Terrain.Name.Biome, new AllowedRange(min: Terrain.biomeMin, max: 255) } },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.RuinsColumn, allowedTerrain: allowedTerrain,
                               maxHitPoints: 40, readableName: "column base", description: "Ancient column remains.");

                        return boardPiece;
                    }

                case Name.RuinsRubble:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() { { Terrain.Name.Biome, new AllowedRange(min: Terrain.biomeMin, max: 255) } },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.RuinsRubble, allowedTerrain: allowedTerrain,
                               maxHitPoints: 40, readableName: "rubble", description: "A pile of rubble.");

                        return boardPiece;
                    }

                case Name.RuinsWall:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.RuinsWallHorizontal1, AnimData.PkgName.RuinsWallHorizontal2, AnimData.PkgName.RuinsWallWallVertical };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() { { Terrain.Name.Biome, new AllowedRange(min: Terrain.biomeMin, max: 255) } },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                               maxHitPoints: 130, readableName: "wall", description: "Ancient wall remains.");

                        return boardPiece;
                    }

                case Name.Clam:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clam, allowedTerrain: allowedTerrain,
                             rotatesWhenDropped: true, readableName: "clam", description: "Have to be cooked before eating.");

                        return boardPiece;
                    }

                case Name.Stick:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Stick, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "stick", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.Stone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Stone, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "stone", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.Granite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Granite, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "granite", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.Clay:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clay, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "clay", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.Rope:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Rope, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "rope", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.WoodLogRegular:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogRegular, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 5, rotatesWhenDropped: true, readableName: "regular wood log", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.WoodLogHard:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogHard, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 5, rotatesWhenDropped: true, readableName: "hard wood log", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.WoodPlank:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodPlank,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood plank", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.IronNail:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Nail, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                              maxHitPoints: 1, rotatesWhenDropped: true, readableName: "nail", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.BeachDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax + 5), (byte)(Terrain.waterLevelMax + 25)) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 20, readableName: "dig site", description: "May contain some buried items.");

                        return boardPiece;
                    }

                case Name.ForestDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");

                        return boardPiece;
                    }

                case Name.DesertDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax + 10), max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");

                        return boardPiece;
                    }

                case Name.GlassDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSiteGlass, allowedTerrain: allowedTerrain,
                            maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");

                        return boardPiece;
                    }

                case Name.SwampDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 50, readableName: "dig site", description: "May contain some buried items.");

                        return boardPiece;
                    }

                case Name.RuinsDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSiteRuins, allowedTerrain: allowedTerrain,
                            maxHitPoints: 40, readableName: "dig site", description: "May contain some buried items.");

                        return boardPiece;
                    }

                case Name.Coal:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Coal, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             readableName: "coal", description: "Crafting material and fuel.");

                        return boardPiece;
                    }

                case Name.Crystal:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crystal, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                            rotatesWhenDropped: true, readableName: "crystal", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.IronOre:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronOre, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             readableName: "iron ore", description: "Can be used to make iron bars.");

                        return boardPiece;
                    }

                case Name.GlassSand:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlassSand, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             readableName: "glass sand", description: "Can be used to make glass.");

                        return boardPiece;
                    }

                case Name.IronBar:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronBar,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "iron bar", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.IronRod:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronRod,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "iron rod", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.IronPlate:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronPlate,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "iron plate", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.SeedsGeneric:
                    {
                        // stackSize should be 1, to avoid mixing different kinds of seeds together

                        BoardPiece boardPiece = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SeedBag, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), readableName: "seeds", description: "Can be planted.");

                        return boardPiece;
                    }

                case Name.Acorn:
                    {
                        BoardPiece acorn = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Acorn, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             readableName: "acorn", description: "Can be planted or cooked.");

                        ((Seed)acorn).PlantToGrow = Name.Oak; // every "non-generic" seed must have its PlantToGrow attribute set

                        return acorn;
                    }

                case Name.CoffeeRaw:
                    {
                        BoardPiece coffeeRaw = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeRaw, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), readableName: "raw coffee", description: "Can be planted or roasted.");

                        ((Seed)coffeeRaw).PlantToGrow = Name.CoffeeShrub; // every "non-generic" seed must have its PlantToGrow attribute set

                        return coffeeRaw;
                    }

                case Name.CoffeeRoasted:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Fatigue, value: (float)-350, isPermanent: true),
                        };

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeRoasted, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, readableName: "roasted coffee", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.Apple:
                    {
                        BoardPiece boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Apple, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "apple", description: "Can be eaten or cooked.");

                        return boardPiece;
                    }

                case Name.Cherry:
                    {
                        BoardPiece boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Cherry, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "cherry", description: "Can be eaten or cooked.");

                        return boardPiece;
                    }

                case Name.Banana:
                    {
                        BoardPiece boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Banana, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "banana", description: "Can be eaten or cooked.");

                        return boardPiece;
                    }

                case Name.Tomato:
                    {
                        BoardPiece boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Tomato, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "tomato", description: "Can be eaten or cooked.");

                        return boardPiece;
                    }

                case Name.Carrot:
                    {
                        BoardPiece boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Carrot, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: false,
                             readableName: "carrot", description: "Can be eaten or cooked.");

                        return boardPiece;
                    }

                case Name.HerbsGreen:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.MaxHP, value: 50f, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsGreen, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "green herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsViolet:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Fatigue, value: -120f, isPermanent: true, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsViolet, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "violet herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsBlack:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-15, autoRemoveDelay: 60 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlack, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "black herbs", description: "Contain poison.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsBrown:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Strength, value: (int)-1, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBrown, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "brown herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsDarkViolet:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Speed, value: -0.5f, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsDarkViolet, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "dark violet herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsDarkGreen:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Speed, value: 0.5f, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsDarkGreen, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "dark green herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsBlue:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.RemovePoison, value: null, isPermanent: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlue, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "blue herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsCyan:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Haste, value: (int)2, autoRemoveDelay: 60 * 30, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsCyan, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "cyan herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsRed:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsRed, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "red herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.HerbsYellow:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Strength, value: (int)2, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsYellow, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "yellow herbs", description: "Potion ingredient.", buffList: buffList);

                        return boardPiece;
                    }

                case Name.EmptyBottle:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.EmptyBottle, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "empty bottle", description: "Can be used to make a potion.");

                        return boardPiece;
                    }

                case Name.PotionGeneric:
                    {
                        // A generic potion, which animPackage and buffs will be set later.

                        BoardPiece boardPiece = new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionTransparent, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "potion", description: "A potion.", buffList: null);

                        return boardPiece;
                    }

                case Name.BottleOfOil:
                    {
                        BoardPiece boardPiece = new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionLightYellow, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "bottle of oil", description: "Crafting material.", buffList: null);

                        return boardPiece;
                    }

                case Name.MeatRawRegular:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatRawRegular,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList,
                             rotatesWhenDropped: true, readableName: "raw meat", description: "Poisonous, but safe after cooking.");

                        return boardPiece;
                    }

                case Name.MeatRawPrime:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatRawPrime,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList, rotatesWhenDropped: true, readableName: "prime raw meat", description: "Poisonous, but safe after cooking.");

                        return boardPiece;
                    }

                case Name.Mushroom:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Mushroom,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList,
                             rotatesWhenDropped: true, readableName: "mushroom", description: "Poisonous, but safe after cooking.");

                        return boardPiece;
                    }

                case Name.MeatDried:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)20, autoRemoveDelay: 60 * 60, increaseIDAtEveryUse: true)};

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatDried, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList, rotatesWhenDropped: true, readableName: "dried meat", description: "Can be eaten or cooked.");

                        return boardPiece;
                    }

                case Name.Fat:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Fat,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             rotatesWhenDropped: true, readableName: "fat", description: "Can be cooked or crafted.");

                        return boardPiece;
                    }

                case Name.Leather:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Leather,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "leather", description: "Crafting material.");

                        return boardPiece;
                    }

                case Name.Burger:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Burger,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             rotatesWhenDropped: true, readableName: "burger", description: "Will remain fresh forever.");

                        return boardPiece;
                    }

                case Name.Meal:
                    {
                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MealStandard,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "cooked meal", description: "Can be eaten.");

                        return boardPiece;
                    }

                case Name.Rabbit:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitBrown, AnimData.PkgName.RabbitDarkBrown, AnimData.PkgName.RabbitGray, AnimData.PkgName.RabbitBlack, AnimData.PkgName.RabbitLightBrown };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitLightGray, AnimData.PkgName.RabbitBeige, AnimData.PkgName.RabbitWhite };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.GroundAll });

                        BoardPiece boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                            maxHitPoints: 150, maxAge: 30000, maxStamina: 300, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.TomatoPlant, Name.Tomato, Name.Meal, Name.Carrot, Name.CarrotPlant, Name.Mushroom }, strength: 30, readableName: "rabbit", description: "A small animal.");

                        return boardPiece;
                    }

                case Name.Fox:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxGinger, AnimData.PkgName.FoxRed, AnimData.PkgName.FoxBlack, AnimData.PkgName.FoxChocolate, AnimData.PkgName.FoxBrown };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxWhite, AnimData.PkgName.FoxGray, AnimData.PkgName.FoxYellow };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.WaterShallow });

                        var eats = new List<Name> { Name.Rabbit, Name.MeatRawRegular, Name.MeatRawPrime, Name.Fat, Name.Burger, Name.MeatDried, Name.Meal, Name.Mushroom };

                        BoardPiece boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                         maxHitPoints: 300, maxAge: 30000, maxStamina: 800, eats: eats, strength: 30, readableName: "fox", description: "An animal.");

                        return boardPiece;
                    }

                case Name.Bear:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.BearBrown, AnimData.PkgName.BearBlack, AnimData.PkgName.BearDarkBrown, AnimData.PkgName.BearGray, AnimData.PkgName.BearRed, AnimData.PkgName.BearBeige };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.BearWhite, AnimData.PkgName.BearOrange };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var eats = new List<Name> { Name.MushroomPlant, Name.Mushroom, Name.MeatRawRegular, Name.MeatRawPrime, Name.Fat, Name.Burger, Name.MeatDried, Name.Meal };
                        eats.AddRange(PieceInfo.GetPlayerNames());

                        BoardPiece boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), speed: 1.5f,
                         maxHitPoints: 650, maxAge: 30000, maxStamina: 1200, eats: eats, strength: 75, readableName: "bear", description: "Cave animal.");

                        return boardPiece;
                    }

                case Name.Tiger:
                    {
                        // TODO remove in the future

                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.TigerOrangeMedium, AnimData.PkgName.TigerGray, AnimData.PkgName.TigerOrangeLight, AnimData.PkgName.TigerOrangeDark, AnimData.PkgName.TigerBrown, AnimData.PkgName.TigerBlack };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.TigerWhite, AnimData.PkgName.TigerYellow };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.rocksLevelMin, max: Terrain.volcanoEdgeMin) },
                        });

                        var eats = new List<Name> { Name.Rabbit, Name.Mushroom, Name.MeatRawRegular, Name.MeatRawPrime, Name.Fat, Name.Burger, Name.MeatDried, Name.Fox, Name.Meal };
                        eats.AddRange(PieceInfo.GetPlayerNames());

                        BoardPiece boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 2.4f,
                         maxHitPoints: 2400, maxAge: 50000, maxStamina: 1300, eats: eats, strength: 140, readableName: "tiger", description: "Very dangerous animal.");

                        return boardPiece;
                    }

                case Name.Frog:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog1, AnimData.PkgName.Frog3, AnimData.PkgName.Frog4, AnimData.PkgName.Frog5, AnimData.PkgName.Frog6, AnimData.PkgName.Frog7 };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.Frog2, AnimData.PkgName.Frog8 };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundSand }
                            );

                        BoardPiece boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                            maxHitPoints: 150, maxAge: 30000, maxStamina: 200, eats: new List<Name> { Name.WaterLily, Name.Rushes }, strength: 30, readableName: "frog", description: "A water animal.");

                        return boardPiece;
                    }

                case Name.KnifeSimple:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.KnifeSimple, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 1, readableName: "simple knife", description: "An old knife.");

                        return boardPiece;
                    }

                case Name.AxeWood:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeWood, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 75, readableName: "wooden axe", description: "Basic logging tool.");

                        return boardPiece;
                    }

                case Name.AxeStone:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 100, readableName: "stone axe", description: "Average logging tool.");

                        return boardPiece;
                    }

                case Name.AxeIron:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 170, readableName: "iron axe", description: "Advanced logging tool.");

                        return boardPiece;
                    }

                case Name.AxeCrystal:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 200, readableName: "crystal axe", description: "Deluxe logging tool.");

                        return boardPiece;
                    }

                case Name.ShovelStone:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 80, readableName: "stone shovel", description: "Basic shovel.");

                        return boardPiece;
                    }

                case Name.ShovelIron:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 100, readableName: "iron shovel", description: "Advanced shovel.");

                        return boardPiece;
                    }

                case Name.ShovelCrystal:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 120, readableName: "crystal shovel", description: "Deluxe shovel.");

                        return boardPiece;
                    }

                case Name.SpearWood:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearWood, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 50, readableName: "wooden spear", description: "Essential melee weapon.");

                        return boardPiece;
                    }

                case Name.SpearStone:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 75, readableName: "stone spear", description: "Simple melee weapon.");

                        return boardPiece;
                    }

                case Name.SpearIron:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 120, readableName: "iron spear", description: "Advanced melee weapon.");

                        return boardPiece;
                    }

                case Name.SpearCrystal:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 250, readableName: "crystal spear", description: "Deluxe melee weapon.");

                        return boardPiece;
                    }

                case Name.PickaxeWood:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeWood, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 75, readableName: "wooden pickaxe", description: "Basic mining tool.");

                        return boardPiece;
                    }

                case Name.PickaxeStone:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 100, readableName: "stone pickaxe", description: "Average mining tool.");

                        return boardPiece;
                    }

                case Name.PickaxeIron:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 170, readableName: "iron pickaxe", description: "Advanced mining tool.");

                        return boardPiece;
                    }

                case Name.PickaxeCrystal:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 200, readableName: "crystal pickaxe", description: "Deluxe mining tool.");

                        return boardPiece;
                    }

                case Name.ScytheStone:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 120, readableName: "stone scythe", description: "Can cut down small plants.");

                        return boardPiece;
                    }

                case Name.ScytheIron:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 200, readableName: "iron scythe", description: "Can cut down small plants easily.");

                        return boardPiece;
                    }

                case Name.ScytheCrystal:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 250, readableName: "crystal scythe", description: "Brings an end to all small plants.");

                        return boardPiece;
                    }

                case Name.BowBasic:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowBasic, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 150, readableName: "basic bow", description: "Projectile weapon.");

                        return boardPiece;
                    }

                case Name.BowAdvanced:
                    {
                        BoardPiece boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowAdvanced, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 300, readableName: "advanced bow", description: "Projectile weapon.");

                        return boardPiece;
                    }

                case Name.ArrowWood:
                    {
                        BoardPiece boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowWood, allowedTerrain: new AllowedTerrain(), maxHitPoints: 15, readableName: "wooden arrow", description: "Very weak arrow.");

                        return boardPiece;
                    }

                case Name.ArrowStone:
                    {
                        BoardPiece boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowStone, allowedTerrain: new AllowedTerrain(), maxHitPoints: 25, readableName: "stone arrow", description: "Basic arrow.");

                        return boardPiece;
                    }

                case Name.ArrowIron:
                    {
                        BoardPiece boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowIron, allowedTerrain: new AllowedTerrain(), maxHitPoints: 40, readableName: "iron arrow", description: "Strong arrow.");

                        return boardPiece;
                    }

                case Name.ArrowExploding:
                    {
                        BoardPiece boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowExploding, allowedTerrain: new AllowedTerrain(), maxHitPoints: 1, readableName: "exploding arrow", description: "Will start a fire.", lightEngine: new LightEngine(size: 100, opacity: 0.8f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true));

                        return boardPiece;
                    }

                case Name.ArrowCrystal:
                    {
                        BoardPiece boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowCrystal, allowedTerrain: new AllowedTerrain(), maxHitPoints: 50, readableName: "crystal arrow", description: "Deluxe arrow. Deals serious damage.");

                        return boardPiece;
                    }

                case Name.TentModernPacked:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        BoardPiece boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentModernPacked, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "packed tent", description: "Can be assembled into a comfortable shelter.");

                        return boardPiece;
                    }

                case Name.TentModern:
                    {
                        var wakeUpBuffs = new List<Buff> { };

                        SleepEngine sleepEngine = new(minFedPercent: 0.3f, fatigueRegen: 0.8f, hitPointsChange: 0.1f, minFatiguePercentPossibleToGet: 0f, updateMultiplier: 10, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        BoardPiece boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentModern, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "modern tent", description: "Modern tent for sleeping.\nAlso allows waiting inside.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));

                        return boardPiece;
                    }

                case Name.TentSmall:
                    {
                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -50f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new(minFedPercent: 0.2f, fatigueRegen: 0.56f, hitPointsChange: 0.05f, minFatiguePercentPossibleToGet: 0.25f, updateMultiplier: 8, canBeAttacked: false, waitingAfterSleepPossible: false, wakeUpBuffs: wakeUpBuffs);

                        BoardPiece boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentSmall, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "small tent", description: "Basic shelter for sleeping.\nNot very comfortable.");

                        return boardPiece;
                    }

                case Name.TentMedium:
                    {
                        var wakeUpBuffs = new List<Buff> { };

                        SleepEngine sleepEngine = new(minFedPercent: 0.3f, fatigueRegen: 0.8f, hitPointsChange: 0.1f, minFatiguePercentPossibleToGet: 0f, updateMultiplier: 10, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        BoardPiece boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentMedium, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "medium tent", description: "Average shelter for sleeping.\nAlso allows waiting inside.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));

                        return boardPiece;
                    }

                case Name.TentBig:
                    {
                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.Strength, value: 1, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new(minFedPercent: 0.5f, fatigueRegen: 1.3f, hitPointsChange: 0.25f, minFatiguePercentPossibleToGet: 0f, updateMultiplier: 14, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        BoardPiece boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentBig, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 200, sleepEngine: sleepEngine, readableName: "big tent", description: "Luxurious shelter for sleeping.\nAlso allows waiting inside.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));

                        return boardPiece;
                    }

                case Name.BackpackSmall:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)2),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackSmall, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small backpack", description: "Expands inventory space.");

                        return boardPiece;
                    }

                case Name.BackpackMedium:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)3),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackMedium, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium backpack", description: "Expands inventory space.");

                        return boardPiece;
                    }

                case Name.BackpackBig:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)4),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackBig, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big backpack", description: "Expands inventory space.");

                        return boardPiece;
                    }

                case Name.BackpackLuxurious:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)5),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)3)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackLuxurious, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "luxurious backpack", description: "Expands inventory space.");

                        return boardPiece;
                    }

                case Name.BeltSmall:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)1)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltSmall, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small belt", description: "Expands belt space.");

                        return boardPiece;
                    }

                case Name.BeltMedium:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)3)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltMedium, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium belt", description: "Expands belt space.");

                        return boardPiece;
                    }

                case Name.BeltBig:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)5)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltBig, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big belt", description: "Expands belt space.");

                        return boardPiece;
                    }

                case Name.BeltLuxurious:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)6)};

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltLuxurious, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "luxurious belt", description: "Expands belt space.");

                        return boardPiece;
                    }

                case Name.Map:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.EnableMap, value: null) };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Map, equipType: Equipment.EquipType.Map,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "map", description: "Keeps track of visited places.");

                        return boardPiece;
                    }

                case Name.HatSimple:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.HeatProtection, value: null)
                        };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HatSimple, equipType: Equipment.EquipType.Head,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "hat", description: "Simple hat.");

                        return boardPiece;
                    }

                case Name.BootsProtective:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.SwampProtection, value: null)
                        };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsProtective, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "protective boots", description: "Allow safe walking over swamp area.");

                        return boardPiece;
                    }

                case Name.BootsMountain:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.FastMountainWalking, value: null) };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsMountain, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "mountain boots", description: "Allow fast walking in the mountains.");

                        return boardPiece;
                    }

                case Name.BootsAllTerrain:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.SwampProtection, value: null),
                            new Buff(type: BuffEngine.BuffType.FastMountainWalking, value: null),
                        };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsAllTerrain, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "all terrain boots", description: "Allow walking over any terrain.");

                        return boardPiece;
                    }

                case Name.BootsSpeed:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.Speed, value: 1f),
                            new Buff(type: BuffEngine.BuffType.ExtendSprintDuration, value: 60 * 2),
                        };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsSpeed, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "boots of speed", description: "Increase speed.");

                        return boardPiece;
                    }

                case Name.GlovesStrength:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.Strength, value: 1) };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlovesStrength, equipType: Equipment.EquipType.Accessory,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "gloves of strength", description: "Increase strength.");

                        return boardPiece;
                    }

                case Name.GlassesVelvet:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.CanSeeThroughFog, value: null) };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlassesBlue, equipType: Equipment.EquipType.Accessory,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "velvet glasses", description: "Pair of mysterious glasses.");

                        return boardPiece;
                    }

                case Name.Dungarees:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f),
                           new Buff(type: BuffEngine.BuffType.MaxFatigue, value: 400f),
                        };

                        BoardPiece boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Dungarees, equipType: Equipment.EquipType.Chest,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "dungarees", description: "Dungarees.");

                        return boardPiece;
                    }

                case Name.TorchSmall:
                    {
                        LightEngine storedLightEngine = new LightEngine(size: 400, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true);

                        BoardPiece boardPiece = new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SmallTorch, canBeUsedDuringRain: false, storedLightEngine: storedLightEngine,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, maxHitPoints: 250, readableName: "small torch", description: "A portable light source.");

                        return boardPiece;
                    }

                case Name.TorchBig:
                    {
                        LightEngine storedLightEngine = new LightEngine(size: 600, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true);

                        BoardPiece boardPiece = new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BigTorch, canBeUsedDuringRain: false, storedLightEngine: storedLightEngine,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, maxHitPoints: 600, readableName: "big torch", description: "Burns for a long time.");

                        return boardPiece;
                    }

                case Name.LanternFull:
                    {
                        LightEngine storedLightEngine = new LightEngine(size: 800, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true);

                        BoardPiece boardPiece = new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Lantern, canBeUsedDuringRain: true, storedLightEngine: storedLightEngine,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, maxHitPoints: 500, readableName: "lantern", description: "Can be used during rain. Refillable.", convertsToWhenUsedUp: Name.LanternEmpty);

                        return boardPiece;
                    }

                case Name.LanternEmpty:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.LanternFrame, allowedTerrain: new AllowedTerrain(),
                             maxHitPoints: 400, readableName: "empty lantern", description: "Needs a candle to be put inside.", rotatesWhenDropped: true);

                        return boardPiece;
                    }

                case Name.Candle:
                    {
                        Decoration decoration = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Candle, allowedTerrain: new AllowedTerrain(),
                             maxHitPoints: 100, readableName: "candle", description: "Can be put inside lantern.", rotatesWhenDropped: true);

                        return decoration;
                    }

                case Name.CampfireSmall:
                    {
                        ushort range = 150;

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.volcanoEdgeMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fireplace = new Fireplace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CampfireSmall, allowedTerrain: allowedTerrain, storageWidth: 2, storageHeight: 1, maxHitPoints: 30, readableName: "small campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        fireplace.sprite.AssignNewName("off");
                        return fireplace;
                    }

                case Name.CampfireMedium:
                    {
                        ushort range = 210;

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.volcanoEdgeMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fireplace = new Fireplace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CampfireMedium, allowedTerrain: allowedTerrain, storageWidth: 2, storageHeight: 2, maxHitPoints: 30, readableName: "medium campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        fireplace.sprite.AssignNewName("off");
                        return fireplace;
                    }

                case Name.PredatorRepellant:
                    {
                        BoardPiece boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SkullAndBones, allowedTerrain: new AllowedTerrain(), readableName: "predator repellent", description: "Scares predators and is invisible.", activeState: BoardPiece.State.ScareAnimalsAway, visible: false);

                        return boardPiece;
                    }

                case Name.Hole:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hole, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 1, readableName: "hole", description: "Empty dig site.");

                        return boardPiece;
                    }

                case Name.TreeStump:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeStump, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 50, readableName: "tree stump", description: "This was once a tree.");

                        return boardPiece;
                    }

                case Name.HumanSkeleton:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HumanSkeleton, allowedTerrain: new AllowedTerrain(),
                              maxHitPoints: 100, readableName: "skeleton", description: "Human remains.");

                        return boardPiece;
                    }

                case Name.LavaFlame:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        VisualEffect lavaFlame = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "lava flame", description: "Decorational flame on lava.", activeState: BoardPiece.State.Empty, visible: true, lightEngine: new LightEngine(size: 180, opacity: 0.45f, colorActive: true, color: Color.Orange * 0.6f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: false, castShadows: true));

                        lavaFlame.sprite.AssignNewSize((byte)BoardPiece.Random.Next(1, 4));
                        ParticleEngine.TurnOn(sprite: lavaFlame.sprite, preset: ParticleEngine.Preset.LavaFlame, particlesToEmit: 1);
                        ParticleEngine.TurnOn(sprite: lavaFlame.sprite, preset: ParticleEngine.Preset.HeatBig, particlesToEmit: 1);

                        return lavaFlame;
                    }

                case Name.WeatherFog:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.WeatherFog1, AnimData.PkgName.WeatherFog2, AnimData.PkgName.WeatherFog3, AnimData.PkgName.WeatherFog4, AnimData.PkgName.WeatherFog5, AnimData.PkgName.WeatherFog6, AnimData.PkgName.WeatherFog7, AnimData.PkgName.WeatherFog8, AnimData.PkgName.WeatherFog9 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: new AllowedTerrain(), readableName: "weather fog", description: "Localized clump of fog.", activeState: BoardPiece.State.WeatherFogMoveRandomly);

                        visualEffect.sprite.opacity = (SonOfRobinGame.random.NextSingle() * 0.2f) + 0.15f;

                        return visualEffect;
                    }

                case Name.SwampGas:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, readableName: "gas", description: "Swamp gas.", activeState: BoardPiece.State.FogMoveRandomly);

                        visualEffect.sprite.color = Color.LimeGreen;
                        visualEffect.sprite.opacity = 0.4f;

                        return visualEffect;
                    }

                case Name.SwampGeyser:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Biome, new AllowedRange(min: 190, max: 255) }},
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerZero, allowedTerrain: allowedTerrain, readableName: "swamp geyser", description: "Swamp geyser.", activeState: BoardPiece.State.Empty);

                        visualEffect.sprite.opacity = 0;

                        ParticleEngine.TurnOn(sprite: visualEffect.sprite, preset: ParticleEngine.Preset.SwampGas, particlesToEmit: 1);
                        ParticleEngine.TurnOn(sprite: visualEffect.sprite, preset: ParticleEngine.Preset.HeatMedium, particlesToEmit: 1);

                        return visualEffect;
                    }

                case Name.LavaGas:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, readableName: "gas", description: "Lava gas.", activeState: BoardPiece.State.FogMoveRandomly, visible: true);

                        visualEffect.sprite.color = new Color(255, 206, 28);
                        visualEffect.sprite.opacity = 0.45f;

                        return visualEffect;
                    }

                case Name.Explosion:
                    {
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Explosion, allowedTerrain: new AllowedTerrain(), readableName: "explosion", description: "An explosion.", activeState: BoardPiece.State.Empty, visible: true, lightEngine: new LightEngine(size: 150, opacity: 1f, colorActive: true, color: Color.Orange * 0.3f, isActive: true, castShadows: true));

                        return visualEffect;
                    }

                case Name.SeaWave:
                    {
                        var packageNames = new List<AnimData.PkgName> { AnimData.PkgName.SeaWave };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Count)];

                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, true } });

                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, readableName: "sea wave", description: "Sea wave.", activeState: BoardPiece.State.SeaWaveMove, visible: true);

                        visualEffect.sprite.opacity = 0f;

                        return visualEffect;
                    }

                case Name.SoundSeaWind:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, true } });

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient sea wind sound", description: "Ambient sound for sea wind.", visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.White;

                        return ambientSound;
                    }

                case Name.SoundLakeWaves:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.NoBiome },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient lake waves sound", description: "Ambient sound for lake waves.", visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Blue;

                        return ambientSound;
                    }

                case Name.SoundNightCrickets:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient night crickets sound", description: "Ambient sound for crickets at night.", visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Black;

                        return ambientSound;
                    }

                case Name.SoundNoonCicadas:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient noon cicadas sound", description: "Ambient sound for cicadas at noon.", visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Green;

                        return ambientSound;
                    }

                case Name.SoundLava:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient lava sound", description: "Ambient sound for lava.", visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Yellow;

                        return ambientSound;
                    }

                case Name.SoundCaveWaterDrip:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.volcanoEdgeMin - 1) },
                        });

                        AmbientSound ambientSound = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient cave water", description: "Ambient sound for cave water dripping.", visible: Preferences.debugShowSounds);

                        ambientSound.sprite.color = Color.Blue;

                        return ambientSound;
                    }

                case Name.ParticleEmitterEnding:
                    {
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerZero, allowedTerrain: new AllowedTerrain(), readableName: "particle emitter", description: "Emits particles.", activeState: BoardPiece.State.EmitParticles, visible: true);

                        visualEffect.sprite.opacity = 0f;

                        return visualEffect;
                    }

                case Name.ParticleEmitterWeather:
                    {
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerThree, allowedTerrain: new AllowedTerrain(), readableName: "weather particle emitter", description: "Emits particles (for weather effects).", activeState: BoardPiece.State.Empty, visible: true);

                        visualEffect.sprite.opacity = 0f;

                        return visualEffect;
                    }

                case Name.EmptyVisualEffect:
                    {
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Empty, allowedTerrain: new AllowedTerrain(), readableName: "empty visual effect", description: "Empty visual effect.", activeState: BoardPiece.State.Empty, visible: true);

                        return visualEffect;
                    }

                case Name.HastePlayerClone:
                    {
                        VisualEffect visualEffect = new VisualEffect(name: templateName, world: world, id: id, animPackage: world?.Player != null ? world.Player.sprite.AnimPackage : AnimData.PkgName.Empty, allowedTerrain: new AllowedTerrain(), readableName: "haste player clone", description: "Haste player clone.", activeState: BoardPiece.State.HasteCloneFollowPlayer, visible: true);

                        return visualEffect;
                    }

                case Name.FertileGroundSmall:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        FertileGround patch = new FertileGround(world: world, id: id, animPackage: AnimData.PkgName.FertileGroundSmall, name: templateName, allowedTerrain: allowedTerrain, readableName: "fertile ground (small)", description: "Seeds can be planted here.", maxHitPoints: 60);

                        return patch;
                    }

                case Name.FertileGroundMedium:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        FertileGround patch = new FertileGround(world: world, id: id, animPackage: AnimData.PkgName.FertileGroundMedium, name: templateName, allowedTerrain: allowedTerrain, readableName: "fertile ground (medium)", description: "Seeds can be planted here.", maxHitPoints: 80);

                        return patch;
                    }

                case Name.FertileGroundLarge:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        FertileGround patch = new FertileGround(world: world, id: id, animPackage: AnimData.PkgName.FertileGroundLarge, name: templateName, allowedTerrain: allowedTerrain, readableName: "fertile ground (large)", description: "Seeds can be planted here.", maxHitPoints: 100);

                        return patch;
                    }

                case Name.FenceHorizontalShort:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceHorizontalShort, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 120, readableName: "short fence (horizontal)", description: "A short fence.");

                        return boardPiece;
                    }

                case Name.FenceVerticalShort:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceVerticalShort, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 120, readableName: "short fence (vertical)", description: "A short fence.");

                        return boardPiece;
                    }

                case Name.FenceHorizontalLong:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceHorizontalLong, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 220, readableName: "long fence (horizontal)", description: "A long fence.");

                        return boardPiece;
                    }

                case Name.FenceVerticalLong:
                    {
                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceVerticalLong, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 220, readableName: "long fence (vertical)", description: "A long fence.");

                        return boardPiece;
                    }

                case Name.CaveEntranceOutside:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.rocksLevelMin + 1, max: Terrain.volcanoEdgeMin - 1) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        BoardPiece boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveEntrance, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave entrance", description: "Cave entrance.", goesDown: true, maxDepth: 1, levelType: Level.LevelType.Cave, activeState: BoardPiece.State.CaveEntranceDisappear);

                        return boardPiece;
                    }

                case Name.CaveEntranceInside:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 118, max: 123) },
                            });

                        BoardPiece boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveEntrance, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave entrance", description: "Cave entrance.", goesDown: true, maxDepth: 4, levelType: Level.LevelType.Cave, activeState: BoardPiece.State.CaveEntranceDisappear);

                        return boardPiece;
                    }

                case Name.CaveExit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 118, max: 119) },
                            });

                        BoardPiece boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveExit, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave exit", description: "Cave exit.", goesDown: false, maxDepth: 9999, levelType: Level.LevelType.Cave); // levelType is ignored here

                        boardPiece.sprite.lightEngine = new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.LightBlue * 0.3f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        return boardPiece;
                    }

                case Name.CaveExitEmergency:
                    {
                        // the same as regular, but can be placed anywhere

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 118, max: Terrain.volcanoEdgeMin - 1) },
                            });

                        BoardPiece boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveExit, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave exit", description: "Cave exit.", goesDown: false, maxDepth: 9999, levelType: Level.LevelType.Cave); // levelType is ignored here

                        boardPiece.sprite.lightEngine = new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.LightBlue * 0.3f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        return boardPiece;
                    }

                case Name.CaveWeakMinerals:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: 135) },
                            });

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MineralsCave, allowedTerrain: allowedTerrain,
                               maxHitPoints: 10, readableName: "weak minerals", description: "Weak cave minerals.");

                        return boardPiece;
                    }

                case Name.IronDeposit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: 125) }});

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronDeposit, allowedTerrain: allowedTerrain,
                            maxHitPoints: 300, readableName: "iron deposit", description: "Can be mined for iron.");

                        return boardPiece;
                    }

                case Name.CoalDeposit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: 125) }});

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoalDeposit, allowedTerrain: allowedTerrain,
                            maxHitPoints: 300, readableName: "coal deposit", description: "Can be mined for coal.");

                        return boardPiece;
                    }

                case Name.CrystalDepositBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: Terrain.volcanoEdgeMin - 4) }});

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositBig, allowedTerrain: allowedTerrain,
                            maxHitPoints: 300, readableName: "big crystal deposit", description: "Can be mined for crystals.");

                        boardPiece.sprite.lightEngine = new LightEngine(size: 400, opacity: 1.2f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        return boardPiece;
                    }

                case Name.CrystalDepositSmall:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: Terrain.volcanoEdgeMin - 4) }});

                        BoardPiece boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositSmall, allowedTerrain: allowedTerrain,
                            maxHitPoints: 150, readableName: "small crystal deposit", description: "Can be mined for crystals.");

                        boardPiece.sprite.lightEngine = new LightEngine(size: 650, opacity: 0.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        return boardPiece;
                    }

                default: { throw new ArgumentException($"Unsupported template name - {templateName}."); }
            }
        }

        private static AllowedTerrain CreatePlayerAllowedTerrain()
        {
            return new AllowedTerrain(
                rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.Volcano, AllowedTerrain.RangeName.NoBiome },
                extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });
        }
    }
}