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
            MushroomPlant = 15,

            SeedsGeneric = 16,
            CoffeeRaw = 17,
            CoffeeRoasted = 18,

            TreeSmall = 19,
            TreeBig = 20,
            PalmTree = 21,
            Oak = 22,
            AppleTree = 23,
            CherryTree = 24,
            BananaTree = 25,

            CarrotPlant = 26,
            TomatoPlant = 27,
            CoffeeShrub = 28,

            Apple = 29,
            Banana = 30,
            Cherry = 31,
            Tomato = 32,
            Carrot = 33,
            Acorn = 34,
            Mushroom = 35,
            MeatRawRegular = 36,
            MeatRawPrime = 37,
            MeatDried = 38,
            Fat = 39,
            Leather = 40,

            Burger = 41,
            Meal = 42,

            Rabbit = 43,
            Fox = 44,
            Bear = 45,
            Frog = 46,

            MineralsBig = 47,
            MineralsSmall = 48,
            MineralsMossyBig = 49,
            MineralsMossySmall = 50,
            CaveWeakMinerals = 51,

            JarTreasureRich = 52,
            JarTreasurePoor = 53,
            JarBroken = 54,
            CrateStarting = 55,
            CrateRegular = 56,

            ChestWooden = 57,
            ChestStone = 58,
            ChestIron = 59,
            ChestCrystal = 60,
            ChestTreasureNormal = 61,
            ChestTreasureBig = 62,

            CampfireSmall = 63,
            CampfireMedium = 64,

            FertileGroundSmall = 65,
            FertileGroundMedium = 66,
            FertileGroundLarge = 67,

            WorkshopEssential = 68,
            WorkshopBasic = 69,
            WorkshopAdvanced = 70,
            WorkshopMaster = 71,

            WorkshopMeatHarvesting = 72,
            MeatDryingRackRegular = 73,
            MeatDryingRackWide = 74,
            WorkshopLeatherBasic = 75,
            WorkshopLeatherAdvanced = 76,

            AlchemyLabStandard = 77,
            AlchemyLabAdvanced = 78,

            BoatConstructionSite = 79,
            BoatCompleteStanding = 80,
            BoatCompleteCruising = 81,
            ShipRescue = 82,

            Totem = 83,
            RuinsColumn = 84,
            RuinsRubble = 85,
            RuinsWall = 86,

            FenceHorizontalShort = 87,
            FenceVerticalShort = 88,
            FenceHorizontalLong = 89,
            FenceVerticalLong = 90,

            FurnaceConstructionSite = 235,
            FurnaceComplete = 91,
            Anvil = 92,
            HotPlate = 93,
            CookingPot = 94,

            Stick = 95,
            WoodLogRegular = 96,
            WoodLogHard = 97,
            WoodPlank = 98,
            Stone = 99,
            Granite = 100,
            Clay = 101,
            Rope = 102,
            HideCloth = 103,
            Clam = 104,

            CoalDeposit = 105,
            IronDeposit = 106,

            BeachDigSite = 107,
            ForestDigSite = 108,
            DesertDigSite = 109,
            GlassDigSite = 110,
            SwampDigSite = 111,
            RuinsDigSite = 112,
            SwampGeyser = 113,
            SwampGas = 114,

            CrystalDepositSmall = 115,
            CrystalDepositBig = 116,

            Coal = 117,
            IronOre = 118,
            IronBar = 119,
            IronRod = 120,
            IronNail = 121,
            IronPlate = 122,
            GlassSand = 123,
            Crystal = 124,

            Backlight = 125,
            BloodSplatter = 126,
            Attack = 127,
            Miss = 128,
            Zzz = 129,
            Heart = 130,
            Orbiter = 131,
            MapMarker = 132,
            MusicNote = 133,
            Crosshair = 134,

            BubbleExclamationRed = 135,
            BubbleExclamationBlue = 136,
            BubbleCraftGreen = 137,

            Explosion = 138,

            CookingTrigger = 139,
            SmeltingTrigger = 236,
            UpgradeTrigger = 140,
            BrewTrigger = 141,
            MeatHarvestTrigger = 142,
            OfferTrigger = 143,
            ConstructTrigger = 144,
            FireplaceTriggerOn = 145,
            FireplaceTriggerOff = 146,

            KnifeSimple = 147,
            AxeWood = 148,
            AxeStone = 149,
            AxeIron = 150,
            AxeCrystal = 151,

            PickaxeWood = 152,
            PickaxeStone = 153,
            PickaxeIron = 154,
            PickaxeCrystal = 155,

            SpearWood = 156,
            SpearStone = 157,
            SpearIron = 158,
            SpearCrystal = 159,

            ScytheStone = 160,
            ScytheIron = 161,
            ScytheCrystal = 162,

            ShovelStone = 163,
            ShovelIron = 164,
            ShovelCrystal = 165,

            BowBasic = 166,
            BowAdvanced = 167,

            ArrowWood = 168,
            ArrowStone = 169,
            ArrowIron = 170,
            ArrowCrystal = 171,
            ArrowExploding = 172,

            TentModern = 173,
            TentModernPacked = 174,
            TentSmall = 175,
            TentMedium = 176,
            TentBig = 177,

            BackpackSmall = 178,
            BackpackMedium = 179,
            BackpackBig = 180,
            BackpackLuxurious = 181,

            BeltSmall = 182,
            BeltMedium = 183,
            BeltBig = 184,
            BeltLuxurious = 185,
            Map = 186,

            Dungarees = 187,
            HatSimple = 188,
            BootsProtective = 189,
            BootsMountain = 190,
            BootsAllTerrain = 191,
            BootsSpeed = 192,
            GlovesStrength = 193,
            GlassesVelvet = 194,

            TorchSmall = 195,
            TorchBig = 196,

            LanternEmpty = 197,
            LanternFull = 198,
            Candle = 199,

            HumanSkeleton = 200,

            HerbsBlack = 201,
            HerbsBlue = 202,
            HerbsCyan = 203,
            HerbsGreen = 204,
            HerbsYellow = 205,
            HerbsRed = 206,
            HerbsViolet = 207,
            HerbsBrown = 208,
            HerbsDarkViolet = 209,
            HerbsDarkGreen = 210,

            EmptyBottle = 211,
            PotionGeneric = 212,
            BottleOfOil = 213,

            Hole = 214,
            TreeStump = 215,

            LavaFlame = 216,
            LavaGas = 217,

            SoundLakeWaves = 218,
            SoundSeaWind = 219,
            SoundNightCrickets = 220,
            SoundNoonCicadas = 221,
            SoundLava = 222,
            SoundCaveWaterDrip = 223,

            SeaWave = 224,
            PredatorRepellant = 225,
            WeatherFog = 226,
            WaterEdgeDistort = 237,

            ParticleEmitterEnding = 227,
            ParticleEmitterWeather = 228,
            EmptyVisualEffect = 229,
            HastePlayerClone = 230,

            CaveEntranceOutside = 231,
            CaveEntranceInside = 232,
            CaveExit = 233,
            CaveExitEmergency = 234,

            // obsolete below (kept for compatibility with old saves)
        }

        public static readonly Name[] allNames = (Name[])Enum.GetValues(typeof(Name));

        public static BoardPiece CreateAndPlaceOnBoard(Name templateName, World world, Vector2 position, bool randomPlacement = false, bool ignoreCollisions = false, bool precisePlacement = false, int id = -1, bool closestFreeSpot = false, int minDistanceOverride = -1, int maxDistanceOverride = -1, bool ignoreDensity = false, bool createdByPlayer = false)
        {
            BoardPiece boardPiece = CreatePiece(templateName: templateName, world: world, id: id, createdByPlayer: createdByPlayer);

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

        public static BoardPiece CreatePiece(Name templateName, World world, int id = -1, bool createdByPlayer = false)
        {
            if (id == -1) id = Helpers.GetUniqueID();

            BoardPiece boardPiece;

            switch (templateName)
            {
                case Name.Empty:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, allowedTerrain: allowedTerrain, readableName: "empty", description: "Should not be used.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.PlayerBoy:
                    {
                        boardPiece = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerBoy, allowedTerrain: CreatePlayerAllowedTerrain(), readableName: "boy", description: "This is you.", activeState: BoardPiece.State.PlayerControlledWalking);

                        break;
                    }

                case Name.PlayerGirl:
                    {
                        boardPiece = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlayerGirl, allowedTerrain: CreatePlayerAllowedTerrain(), readableName: "girl", description: "This is you.", activeState: BoardPiece.State.PlayerControlledWalking);

                        break;
                    }

                case Name.PlayerTestDemoness:
                    {
                        Player player = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DragonBonesTestFemaleMage, allowedTerrain: CreatePlayerAllowedTerrain(), readableName: "demoness", description: "This is you.", activeState: BoardPiece.State.PlayerControlledWalking)
                        {
                            maxFatigue = 50000,
                            maxHitPoints = 100000,
                            maxFedLevel = 400000,
                            speed = 8f,
                            strength = 100,
                        };

                        player.fedLevel = player.maxFedLevel;
                        player.HitPoints = player.maxHitPoints;
                        player.sprite.lightEngine = new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.Red * 1f, isActive: false, castShadows: true);
                        player.sprite.lightEngine.AssignSprite(player.sprite);

                        boardPiece = player;

                        break;
                    }

                case Name.PlayerGhost:
                    {
                        Player spectator = new Player(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.NoAnim, allowedTerrain: new AllowedTerrain(), readableName: "player ghost", description: "A metaphysical representation of player's soul.", activeState: BoardPiece.State.PlayerControlledGhosting);

                        spectator.sprite.lightEngine = new LightEngine(size: 650, opacity: 1.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        spectator.sprite.lightEngine.AssignSprite(spectator.sprite);
                        spectator.speed = 5;
                        spectator.sprite.opacity = 0.5f;
                        spectator.sprite.color = new Color(150, 255, 255);
                        spectator.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.SkyBlue * 0.7f, textureSize: new Vector2(spectator.sprite.AnimFrame.Texture.Width, spectator.sprite.AnimFrame.Texture.Height), priority: 0, framesLeft: -1));

                        boardPiece = spectator;

                        break;
                    }

                case Name.GrassRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 96, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 50, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain, maxAge: 600, massTakenMultiplier: 0.53f, readableName: "regular grass", description: "A regular grass.");

                        break;
                    }

                case Name.GrassGlow:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 96, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 50, max: 255) }});

                        // readableName is the same as "regular grass", to make it appear identical to the regular grass

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GrassRegular, allowedTerrain: allowedTerrain,
                            maxAge: 1000, massTakenMultiplier: 0.49f, readableName: "regular grass", description: "A special type of grass.", lightEngine: new LightEngine(size: 0, opacity: 0.3f, colorActive: true, color: Color.Blue * 3f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: true, castShadows: false));

                        break;
                    }

                case Name.GrassDesert:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 115) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GrassDesert, allowedTerrain: allowedTerrain,
                             maxAge: 900, massTakenMultiplier: 0.63f, readableName: "desert grass", description: "A grass, that grows on sand.");

                        break;
                    }

                case Name.PlantPoison:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                                { Terrain.Name.Biome, new AllowedRange(min: (byte)(Terrain.biomeMin + (255 - Terrain.biomeMin) / 2), max:255 ) },
                                { Terrain.Name.Height, new AllowedRange(min: 112, max:156 ) },
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PlantPoison, allowedTerrain: allowedTerrain,
                            maxAge: 950, massTakenMultiplier: 0.63f, readableName: "poisonous plant", description: "Poisonous plant.");

                        break;
                    }

                case Name.Rushes:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 77, max: 95) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 128, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Rushes, allowedTerrain: allowedTerrain, maxAge: 600, massTakenMultiplier: 0.62f, readableName: "rushes", description: "A water plant.");

                        break;
                    }

                case Name.WaterLily:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.WaterLily1, AnimData.PkgName.WaterLily2, AnimData.PkgName.WaterLily3 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>{
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 12), max: (byte)(Terrain.waterLevelMax - 1)) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 140) },
                        },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                            maxAge: 1800, massTakenMultiplier: 0.4f, readableName: "water lily", description: "A water plant.", maxHitPoints: 10);

                        break;
                    }

                case Name.FlowersPlain:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.FlowersYellow1, AnimData.PkgName.FlowersWhite };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 140, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                           maxAge: 550, massTakenMultiplier: 1f, readableName: "regular flower", description: "A flower.");

                        break;
                    }

                case Name.FlowersRed:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 140, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FlowersRed, allowedTerrain: allowedTerrain,
                           maxAge: 550, massTakenMultiplier: 1f, readableName: "red flower", description: "A red flower.");

                        break;
                    }

                case Name.FlowersMountain:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min:Terrain.rocksLevelMin, max: Terrain.volcanoEdgeMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FlowersYellow2, allowedTerrain: allowedTerrain,
                         maxAge: 4000, massTakenMultiplier: 0.98f, readableName: "mountain flower", description: "A mountain flower.");

                        break;
                    }

                case Name.TreeSmall:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.TreeSmall1, AnimData.PkgName.TreeSmall2 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 80, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                             maxAge: 30000, massTakenMultiplier: 1.35f, maxHitPoints: 50, readableName: "small tree", description: "A small tree.");

                        break;
                    }

                case Name.TreeBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, readableName: "big tree", description: "A big tree.");

                        break;
                    }

                case Name.Oak:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Acorn);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, fruitEngine: fruitEngine, readableName: "oak", description: "Acorns can grow on it.");

                        break;
                    }

                case Name.AppleTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 500f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Apple);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, fruitEngine: fruitEngine, readableName: "apple tree", description: "Apples can grow on it.");

                        break;
                    }

                case Name.CherryTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 6, oneFruitMass: 120f, yOffsetPercent: -0.1f, areaWidthPercent: 0.72f, areaHeightPercent: 0.65f, fruitName: Name.Cherry);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeBig, allowedTerrain: allowedTerrain,
                            maxAge: 30000, massTakenMultiplier: 3.1f, maxHitPoints: 100, fruitEngine: fruitEngine, readableName: "cherry tree", description: "Cherries can grow on it.");

                        break;
                    }

                case Name.BananaTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 250f, yOffsetPercent: -0.29f, areaWidthPercent: 0.85f, areaHeightPercent: 0.3f, fruitName: Name.Banana);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PalmTree, allowedTerrain: allowedTerrain,
                            maxAge: 15000, massTakenMultiplier: 1.5f, maxHitPoints: 160, fruitEngine: fruitEngine, readableName: "banana tree", description: "Bananas can grow on it.");

                        break;
                    }

                case Name.PalmTree:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax, max: 159) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 210) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PalmTree, allowedTerrain: allowedTerrain,
                            maxAge: 15000, massTakenMultiplier: 1.5f, maxHitPoints: 160, readableName: "palm tree", description: "A palm tree.");

                        break;
                    }

                case Name.TomatoPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 200) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 4, oneFruitMass: 50f, yOffsetPercent: -0.05f, areaWidthPercent: 0.85f, areaHeightPercent: 0.8f, fruitName: Name.Tomato);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TomatoPlant, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, fruitEngine: fruitEngine, readableName: "tomato plant", description: "Tomatoes can grow on it.");

                        break;
                    }

                case Name.CoffeeShrub:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 80, max: 140) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 250) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 2, oneFruitMass: 50f, yOffsetPercent: -0.05f, areaWidthPercent: 0.85f, areaHeightPercent: 0.8f, fruitName: Name.CoffeeRaw);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeShrub, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, fruitEngine: fruitEngine, readableName: "coffee shrub", description: "Coffee can grow on it.");

                        break;
                    }

                case Name.CarrotPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 200) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        var fruitEngine = new FruitEngine(maxNumber: 1, oneFruitMass: 50f, yOffsetPercent: -0.1f, areaWidthPercent: 0.8f, areaHeightPercent: 0.7f, fruitName: Name.Carrot, hiddenFruits: true);

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CarrotPlant, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, fruitEngine: fruitEngine, readableName: "carrot plant", description: "Carrots can grow on it.");

                        break;
                    }

                case Name.Cactus:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 99, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Cactus, allowedTerrain: allowedTerrain,
                            maxAge: 30000, maxHitPoints: 80, massTakenMultiplier: 1.65f, readableName: "cactus", description: "A desert plant.");

                        break;
                    }

                case Name.MushroomPlant:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.volcanoEdgeMin - 1) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 100, max: 255) },
                        });

                        boardPiece = new Plant(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MushroomPlant, allowedTerrain: allowedTerrain,
                           maxHitPoints: 40, maxAge: 1000, massTakenMultiplier: 0.855f, readableName: "mushroom plant", description: "Growing mushrooms.");

                        break;
                    }

                case Name.MineralsSmall:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.MineralsSmall1, AnimData.PkgName.MineralsSmall2, AnimData.PkgName.MineralsSmall3, AnimData.PkgName.MineralsSmall4, AnimData.PkgName.MineralsSmall5, AnimData.PkgName.MineralsSmall6, AnimData.PkgName.MineralsSmall7, AnimData.PkgName.MineralsSmall8, AnimData.PkgName.MineralsSmall9, AnimData.PkgName.MineralsSmall10 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                               maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.");

                        break;
                    }

                case Name.MineralsMossySmall:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.MineralsMossySmall1, AnimData.PkgName.MineralsMossySmall2, AnimData.PkgName.MineralsMossySmall3, AnimData.PkgName.MineralsMossySmall4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 129, max: 255) },
                        });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                              maxHitPoints: 60, readableName: "small minerals", description: "Can be mined for stone.");

                        break;
                    }

                case Name.MineralsBig:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.MineralsBig1, AnimData.PkgName.MineralsBig2, AnimData.PkgName.MineralsBig3, AnimData.PkgName.MineralsBig4, AnimData.PkgName.MineralsBig5, AnimData.PkgName.MineralsBig6, AnimData.PkgName.MineralsBig7, AnimData.PkgName.MineralsBig8, AnimData.PkgName.MineralsBig9, AnimData.PkgName.MineralsBig10, AnimData.PkgName.MineralsBig11, AnimData.PkgName.MineralsBig12, AnimData.PkgName.MineralsBig13, AnimData.PkgName.MineralsBig14 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                              maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.");

                        break;
                    }

                case Name.MineralsMossyBig:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.MineralsMossyBig1, AnimData.PkgName.MineralsMossyBig2, AnimData.PkgName.MineralsMossyBig3, AnimData.PkgName.MineralsMossyBig4, AnimData.PkgName.MineralsMossyBig5, AnimData.PkgName.MineralsMossyBig6, AnimData.PkgName.MineralsMossyBig7, AnimData.PkgName.MineralsMossyBig8, AnimData.PkgName.MineralsMossyBig9, AnimData.PkgName.MineralsMossyBig10, AnimData.PkgName.MineralsMossyBig11, AnimData.PkgName.MineralsMossyBig12 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 165, max: Terrain.volcanoEdgeMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 129, max: 255) },
                        });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                              maxHitPoints: 100, readableName: "big minerals", description: "Can be mined for stone.");

                        break;
                    }

                case Name.Backlight:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Backlight, allowedTerrain: allowedTerrain, readableName: "backlight", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.BloodSplatter:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.BloodSplatter1, AnimData.PkgName.BloodSplatter2, AnimData.PkgName.BloodSplatter3 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                             readableName: "bloodSplatter", description: "A pool of blood.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.Attack:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Attack, allowedTerrain: allowedTerrain, readableName: "attack", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.MapMarker:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MapMarker, allowedTerrain: allowedTerrain, readableName: "map marker", description: "Map marker.", activeState: BoardPiece.State.MapMarkerShowAndCheck, visible: false);

                        break;
                    }

                case Name.Miss:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Miss, allowedTerrain: allowedTerrain, readableName: "miss", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.Zzz:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Zzz, allowedTerrain: allowedTerrain, readableName: "zzz", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.Heart:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Heart, allowedTerrain: allowedTerrain, readableName: "heart", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.Orbiter:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crosshair, allowedTerrain: allowedTerrain, readableName: "orbiter", description: "Moves randomly around target.", activeState: BoardPiece.State.OscillateAroundTarget);
                        boardPiece.sprite.Visible = false;

                        // needs visualAid (target) set to work correctly

                        break;
                    }

                case Name.MusicNote:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MusicNoteSmall, allowedTerrain: allowedTerrain, readableName: "music note", description: "Sound visual.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.Crosshair:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crosshair, allowedTerrain: allowedTerrain, readableName: "crosshair", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.BubbleExclamationRed:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleExclamationRed, allowedTerrain: allowedTerrain, readableName: "red exclamation", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.BubbleExclamationBlue:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleExclamationBlue, allowedTerrain: allowedTerrain, readableName: "blue exclamation", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.BubbleCraftGreen:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BubbleCraftGreen, allowedTerrain: allowedTerrain, readableName: "green exclamation with plus", description: "A visual effect.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.CookingTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "cooking starter", description: "Starts cooking.");

                        break;
                    }

                case Name.SmeltingTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "smelting starter", description: "Starts smelting.");

                        break;
                    }

                case Name.BrewTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "brewing starter", description: "Starts brewing.");

                        break;
                    }

                case Name.UpgradeTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Upgrade, allowedTerrain: allowedTerrain, readableName: "upgrade", description: "Upgrades item.");

                        break;
                    }

                case Name.MeatHarvestTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.KnifeSimple, allowedTerrain: allowedTerrain, readableName: "harvest", description: "Harvests meat from the animal.");

                        break;
                    }

                case Name.OfferTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "offer", description: "Offers gifts to gods.");

                        break;
                    }

                case Name.ConstructTrigger:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hammer, allowedTerrain: allowedTerrain, readableName: "construct", description: "Construct next level.");

                        break;
                    }

                case Name.FireplaceTriggerOn:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "fireplace on", description: "Ignites the fireplace.");

                        break;
                    }

                case Name.FireplaceTriggerOff:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new Trigger(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WaterDrop, allowedTerrain: allowedTerrain, readableName: "fireplace off", description: "Extinginguishes fire.");

                        break;
                    }

                case Name.CrateStarting:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crate, allowedTerrain: allowedTerrain,
                              maxHitPoints: 5, readableName: "supply crate", description: "Contains valuable items.");

                        break;
                    }

                case Name.CrateRegular:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crate, allowedTerrain: allowedTerrain,
                              maxHitPoints: 40, readableName: "supply crate", description: "Contains valuable items.");

                        break;
                    }

                case Name.ChestWooden:
                    {
                        byte storageWidth = 3;
                        byte storageHeight = 2;

                        boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestWooden, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 40, readableName: "wooden chest", description: $"Can store items ({storageWidth}x{storageHeight}).");

                        break;
                    }

                case Name.ChestStone:
                    {
                        byte storageWidth = 4;
                        byte storageHeight = 4;

                        boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestStone, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 50, readableName: "stone chest", description: $"Can store items ({storageWidth}x{storageHeight}).");

                        break;
                    }

                case Name.ChestIron:
                    {
                        byte storageWidth = 6;
                        byte storageHeight = 4;

                        boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestIron, allowedTerrain: AllowedTerrain.GetFieldCraft(), storageWidth: storageWidth, storageHeight: storageHeight, maxHitPoints: 60, readableName: "iron chest", description: $"Can store items ({storageWidth}x{storageHeight}).");

                        break;
                    }

                case Name.ChestCrystal:
                    {
                        boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestCrystal, allowedTerrain: AllowedTerrain.GetFieldCraft(), storageWidth: 1, storageHeight: 1, maxHitPoints: 300, readableName: "crystal chest", description: "All crystal chests share their contents.");

                        break;
                    }

                case Name.ChestTreasureNormal:
                    {
                        boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureBlue, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), storageWidth: 2, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", animName: "closed");

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
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces(piece: boardPiece);

                            if (chestContents.Count >= 2)
                            {
                                foreach (BoardPiece piece in chestContents)
                                {
                                    boardPiece.PieceStorage.AddPiece(piece);
                                    if (boardPiece.PieceStorage.EmptySlotsCount == 0) break;
                                }
                                break;
                            }
                        }

                        break;
                    }

                case Name.ChestTreasureBig:
                    {
                        boardPiece = new Container(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ChestTreasureRed, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), storageWidth: 3, storageHeight: 2, maxHitPoints: 50, readableName: "treasure chest", description: "Contains treasure.", animName: "closed");

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
                            List<BoardPiece> chestContents = chestContentsYield.GetAllPieces(piece: boardPiece);

                            if (chestContents.Count >= 4)
                            {
                                foreach (BoardPiece piece in chestContents)
                                {
                                    boardPiece.PieceStorage.AddPiece(piece);
                                    if (boardPiece.PieceStorage.EmptySlotsCount == 0) break;
                                }
                                break;
                            }
                        }

                        break;
                    }

                case Name.JarTreasureRich:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarWhole, allowedTerrain: allowedTerrain,
                              maxHitPoints: 40, readableName: "sealed jar", description: "Contains supplies.");

                        break;
                    }

                case Name.JarTreasurePoor:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarWhole, allowedTerrain: allowedTerrain,
                              maxHitPoints: 40, readableName: "sealed jar", description: "Contains supplies.");

                        break;
                    }

                case Name.JarBroken:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.GroundAll });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.JarBroken, allowedTerrain: allowedTerrain,
                              maxHitPoints: 20, readableName: "broken jar", description: "Broken Jar.");

                        break;
                    }

                case Name.WorkshopEssential:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopEssential, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftEssential, maxHitPoints: 30, readableName: "essential workshop", description: "Essential crafting workshop.", canBeUsedDuringRain: true);

                        break;
                    }

                case Name.WorkshopBasic:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopBasic, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftBasic, maxHitPoints: 30, readableName: "basic workshop", description: "Basic crafting workshop.", canBeUsedDuringRain: true);

                        break;
                    }

                case Name.WorkshopAdvanced:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopAdvanced, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftAdvanced, maxHitPoints: 80, readableName: "advanced workshop", description: "Advanced crafting workshop.", canBeUsedDuringRain: true);

                        break;
                    }

                case Name.WorkshopMaster:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopMaster, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftMaster, maxHitPoints: 80, readableName: "master workshop", description: "Master's crafting workshop.", canBeUsedDuringRain: true);

                        break;
                    }

                case Name.WorkshopLeatherBasic:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherBasic, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftLeatherBasic, maxHitPoints: 30, readableName: "basic leather workshop", description: "For making basic items out of leather.", canBeUsedDuringRain: true);

                        break;
                    }

                case Name.WorkshopLeatherAdvanced:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopLeatherAdvanced, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftLeatherAdvanced, maxHitPoints: 30, readableName: "advanced leather workshop", description: "For making advanced items out of leather.", canBeUsedDuringRain: true);

                        break;
                    }

                case Name.FurnaceConstructionSite:
                    {
                        Dictionary<int, Dictionary<Name, int>> ingredientsForLevels = new()
                        {
                            { 0, new Dictionary<Name, int>{
                                { Name.WoodPlank, 4 },
                                { Name.Stone, 5 },
                                { Name.Stick, 4 },
                            } },

                           { 1, new Dictionary<Name, int>{
                                { Name.Clay, 1 },
                                { Name.Fat, 3 },
                                { Name.Leather, 3 },
                                { Name.Stone, 2 },
                            } },

                            { 2, new Dictionary<Name, int>{
                                { Name.Granite, 2 },
                                { Name.Stone, 5 },
                            } },
                        };

                        Dictionary<int, string> descriptionsForLevels = new()
                        {
                            { 0, "base" },
                            { 1, "insulation" },
                            { 2, "top" },
                        };

                        boardPiece = new ConstructionSite(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FurnaceConstructionSite, allowedTerrain: AllowedTerrain.GetFieldCraft(), materialsForLevels: ingredientsForLevels, descriptionsForLevels: descriptionsForLevels, convertsIntoWhenFinished: Name.FurnaceComplete, maxHitPoints: 80, readableName: "furnace construction site", description: "Furnace construction site.");

                        break;
                    }

                case Name.FurnaceComplete:
                    {
                        boardPiece = new Furnace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FurnaceComplete, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                            maxHitPoints: 200, readableName: "furnace", description: "For ore smelting.");

                        break;
                    }

                case Name.Anvil:
                    {
                        boardPiece = new Workshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Anvil, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              craftMenuTemplate: MenuTemplate.Name.CraftAnvil, maxHitPoints: 80, readableName: "anvil", description: "For metal forming.", emitsLightWhenCrafting: true, lightEngine: new LightEngine(size: 0, opacity: 1f, colorActive: true, color: Color.Orange * 0.25f, addedGfxRectMultiplier: 8f, isActive: false, castShadows: true), canBeUsedDuringRain: true);

                        break;
                    }

                case Name.HotPlate:
                    {
                        boardPiece = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HotPlate, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 20, foodMassMultiplier: 2.6f, readableName: "hot plate", description: "For cooking.", ingredientSpace: 1, canBeUsedDuringRain: false);

                        boardPiece.sprite.AssignNewName("off");
                        break;
                    }

                case Name.CookingPot:
                    {
                        boardPiece = new Cooker(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CookingPot, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, foodMassMultiplier: 3.2f, readableName: "cooking pot", description: "For cooking. Can be used during rain.", ingredientSpace: 3, canBeUsedDuringRain: true);

                        boardPiece.sprite.AssignNewName("off");
                        break;
                    }

                case Name.AlchemyLabStandard:
                    {
                        boardPiece = new AlchemyLab(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AlchemyLabStandard, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "alchemy lab", description: "For potion brewing.", boosterSpace: 1);

                        boardPiece.sprite.AssignNewName("off");
                        break;
                    }

                case Name.AlchemyLabAdvanced:
                    {
                        boardPiece = new AlchemyLab(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AlchemyLabAdvanced, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "advanced alchemy lab", description: "For advanced potion brewing.", boosterSpace: 3);

                        boardPiece.sprite.AssignNewName("off");
                        break;
                    }

                case Name.WorkshopMeatHarvesting:
                    {
                        boardPiece = new MeatHarvestingWorkshop(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WorkshopMeatHarvesting, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "meat workshop", description: "For animal processing.");

                        boardPiece.sprite.AssignNewName("off");

                        break;
                    }

                case Name.MeatDryingRackRegular:
                    {
                        boardPiece = new MeatDryingRack(name: templateName, storageWidth: 2, storageHeight: 2, world: world, id: id, animPackage: AnimData.PkgName.MeatDryingRackRegular, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 30, readableName: "meat drying rack", description: "Regular rack for meat drying.");

                        boardPiece.sprite.AssignNewName("off");
                        break;
                    }

                case Name.MeatDryingRackWide:
                    {
                        boardPiece = new MeatDryingRack(name: templateName, storageWidth: 3, storageHeight: 2, world: world, id: id, animPackage: AnimData.PkgName.MeatDryingRackWide, allowedTerrain: AllowedTerrain.GetFieldCraft(), maxHitPoints: 50, readableName: "meat drying rack (wide)", description: "Wide rack for meat drying.");

                        boardPiece.sprite.AssignNewName("off");
                        break;
                    }

                case Name.Totem:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                                { Terrain.Name.Biome, new AllowedRange(min: (byte)(Terrain.biomeMin + (255 - Terrain.biomeMin) / 2), max:255 ) },
                            },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        boardPiece = new Totem(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Totem, allowedTerrain: allowedTerrain, maxHitPoints: 30, readableName: "totem", description: "A totem with magical powers.");

                        break;
                    }

                case Name.BoatConstructionSite:
                    {
                        Dictionary<int, Dictionary<Name, int>> ingredientsForLevels = new()
                        {
                            { 0, new Dictionary<Name, int>{
                                { Name.WoodPlank, 8 },
                                { Name.WoodLogHard, 4 },
                            } },

                            { 1, new Dictionary<Name, int>{
                                { Name.IronPlate, 5 },
                                { Name.IronNail, 6 },
                                { Name.BottleOfOil, 8 },
                            } },

                            { 2, new Dictionary<Name, int>{
                                { Name.IronRod, 6 },
                                { Name.HideCloth, 10 },
                                { Name.Rope, 4 },
                            } },

                            { 3, new Dictionary<Name, int>{
                                { Name.IronNail, 7 },
                                { Name.WoodLogHard, 4 },
                                { Name.Granite, 3 },
                                { Name.Clay, 4 },
                            } },

                            { 4, new Dictionary<Name, int>{
                                { Name.BottleOfOil, 6 },
                                { Name.Leather, 4 },
                                { Name.Clay, 4 },
                                { Name.Rope, 4 },
                                { Name.WoodLogHard, 4 },
                                { Name.LanternFull, 3 },
                                { Name.Candle, 5 },
                            } },

                            { 5, new Dictionary<Name, int>{
                                { Name.MeatDried, 8 },
                                { Name.Carrot, 6 },
                                { Name.Apple, 6 },
                                { Name.Banana, 6 },
                                { Name.Mushroom, 6 },
                                { Name.CoffeeRoasted, 4 },
                            } },
                        };

                        Dictionary<int, string> descriptionsForLevels = new()
                        {
                            { 0, "basic frame" },
                            { 1, "sturdy hull" },
                            { 2, "sailing rig" },
                            { 3, "steering system" },
                            { 4, "final touches" },
                            { 5, "supplies" },
                        };

                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        boardPiece = new ConstructionSite(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BoatConstruction, allowedTerrain: allowedTerrain, materialsForLevels: ingredientsForLevels, descriptionsForLevels: descriptionsForLevels, convertsIntoWhenFinished: Name.BoatCompleteStanding, maxHitPoints: 60, readableName: "boat construction site", description: "Boat construction site.");

                        break;
                    }

                case Name.BoatCompleteStanding:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BoatCompleteStanding, allowedTerrain: allowedTerrain,
                             rotatesWhenDropped: true, readableName: "boat", description: "Can be used to escape the island.");

                        break;
                    }

                case Name.BoatCompleteCruising:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BoatCompleteCruising, allowedTerrain: allowedTerrain, readableName: "boat", description: "Actively used to escape the island.", activeState: BoardPiece.State.EndingBoatCruise);

                        break;
                    }

                case Name.ShipRescue:
                    {
                        var allowedTerrain = new AllowedTerrain();

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShipRescue, allowedTerrain: allowedTerrain, readableName: "rescue ship", description: "Appears at game end and rescues the player.", activeState: BoardPiece.State.Empty);

                        break;
                    }

                case Name.RuinsColumn:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() { { Terrain.Name.Biome, new AllowedRange(min: Terrain.biomeMin, max: 255) } },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.RuinsColumn, allowedTerrain: allowedTerrain,
                               maxHitPoints: 40, readableName: "column base", description: "Ancient column remains.");

                        break;
                    }

                case Name.RuinsRubble:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() { { Terrain.Name.Biome, new AllowedRange(min: Terrain.biomeMin, max: 255) } },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.RuinsRubble, allowedTerrain: allowedTerrain,
                               maxHitPoints: 40, readableName: "rubble", description: "A pile of rubble.");

                        break;
                    }

                case Name.RuinsWall:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.RuinsWallHorizontal1, AnimData.PkgName.RuinsWallHorizontal2, AnimData.PkgName.RuinsWallWallVertical };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() { { Terrain.Name.Biome, new AllowedRange(min: Terrain.biomeMin, max: 255) } },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain,
                               maxHitPoints: 130, readableName: "wall", description: "Ancient wall remains.");

                        break;
                    }

                case Name.Clam:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clam, allowedTerrain: allowedTerrain,
                             rotatesWhenDropped: true, readableName: "clam", description: "Have to be cooked before eating.");

                        break;
                    }

                case Name.Stick:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Stick, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "stick", description: "Crafting material.");

                        break;
                    }

                case Name.Stone:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Stone, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "stone", description: "Crafting material.");

                        break;
                    }

                case Name.Granite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Granite, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "granite", description: "Crafting material.");

                        break;
                    }

                case Name.Clay:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Clay, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "clay", description: "Crafting material.");

                        break;
                    }

                case Name.Rope:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Rope, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "rope", description: "Crafting material.");

                        break;
                    }

                case Name.HideCloth:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HideCloth, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "hidecloth", description: "Crafting material.");

                        break;
                    }

                case Name.WoodLogRegular:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogRegular, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 5, rotatesWhenDropped: true, readableName: "regular wood log", description: "Crafting material.");

                        break;
                    }

                case Name.WoodLogHard:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodLogHard, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 5, rotatesWhenDropped: true, readableName: "hard wood log", description: "Crafting material.");

                        break;
                    }

                case Name.WoodPlank:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WoodPlank,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), maxHitPoints: 5, rotatesWhenDropped: true, readableName: "wood plank", description: "Crafting material.");

                        break;
                    }

                case Name.IronNail:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Nail, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                              maxHitPoints: 1, rotatesWhenDropped: true, readableName: "nail", description: "Crafting material.");

                        break;
                    }

                case Name.BeachDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax + 5), (byte)(Terrain.waterLevelMax + 25)) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 20, readableName: "dig site", description: "May contain some buried items.");

                        break;
                    }

                case Name.ForestDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");

                        break;
                    }

                case Name.DesertDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax + 10), max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSite, allowedTerrain: allowedTerrain,
                            maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");

                        break;
                    }

                case Name.GlassDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.waterLevelMax + 10, max: Terrain.rocksLevelMin) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 90) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSiteGlass, allowedTerrain: allowedTerrain,
                            maxHitPoints: 30, readableName: "dig site", description: "May contain some buried items.");

                        break;
                    }

                case Name.SwampDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSiteSwamp, allowedTerrain: allowedTerrain,
                            maxHitPoints: 50, readableName: "dig site", description: "May contain some buried items.");

                        break;
                    }

                case Name.RuinsDigSite:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeRuins, true } });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.DigSiteRuins, allowedTerrain: allowedTerrain,
                            maxHitPoints: 40, readableName: "dig site", description: "May contain some buried items.");

                        break;
                    }

                case Name.Coal:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Coal, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             readableName: "coal", description: "Used for smelting (using a funace).");

                        break;
                    }

                case Name.Crystal:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Crystal, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                            rotatesWhenDropped: true, readableName: "crystal", description: "Crafting material.");

                        break;
                    }

                case Name.IronOre:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronOre, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             readableName: "iron ore", description: "Can be used to make iron bars (using a furnace).");

                        break;
                    }

                case Name.GlassSand:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlassSand, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             readableName: "glass sand", description: "Can be used to make glass (using a furnace).");

                        break;
                    }

                case Name.IronBar:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronBar,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "iron bar", description: "Crafting material.");

                        break;
                    }

                case Name.IronRod:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronRod,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "iron rod", description: "Crafting material.");

                        break;
                    }

                case Name.IronPlate:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronPlate,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "iron plate", description: "Crafting material.");

                        break;
                    }

                case Name.SeedsGeneric:
                    {
                        // stackSize should be 1, to avoid mixing different kinds of seeds together

                        boardPiece = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SeedBag, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), readableName: "seeds", description: "Can be planted.");

                        break;
                    }

                case Name.Acorn:
                    {
                        boardPiece = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Acorn, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             readableName: "acorn", description: "Can be planted or cooked.");

                        ((Seed)boardPiece).PlantToGrow = Name.Oak; // every "non-generic" seed must have its PlantToGrow attribute set

                        break;
                    }

                case Name.CoffeeRaw:
                    {
                        boardPiece = new Seed(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeRaw, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), readableName: "raw coffee", description: "Can be planted or roasted (using a furnace).");

                        ((Seed)boardPiece).PlantToGrow = Name.CoffeeShrub; // every "non-generic" seed must have its PlantToGrow attribute set

                        break;
                    }

                case Name.CoffeeRoasted:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Fatigue, value: (float)-350, isPermanent: true),
                        };

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoffeeRoasted, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, readableName: "roasted coffee", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.Apple:
                    {
                        boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Apple, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "apple", description: "Can be eaten or cooked.");

                        break;
                    }

                case Name.Cherry:
                    {
                        boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Cherry, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "cherry", description: "Can be eaten or cooked.");

                        break;
                    }

                case Name.Banana:
                    {
                        boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Banana, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "banana", description: "Can be eaten or cooked.");

                        break;
                    }

                case Name.Tomato:
                    {
                        boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Tomato, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: true,
                             readableName: "tomato", description: "Can be eaten or cooked.");

                        break;
                    }

                case Name.Carrot:
                    {
                        boardPiece = new Fruit(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Carrot, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), mightContainSeeds: false,
                             readableName: "carrot", description: "Can be eaten or cooked.");

                        break;
                    }

                case Name.HerbsGreen:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.MaxHP, value: 50f, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsGreen, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "green herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsViolet:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Fatigue, value: -120f, isPermanent: true, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsViolet, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "violet herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsBlack:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-15, autoRemoveDelay: 60 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlack, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "black herbs", description: "Contain poison.", buffList: buffList);

                        break;
                    }

                case Name.HerbsBrown:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Strength, value: (int)-1, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBrown, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "brown herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsDarkViolet:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Speed, value: -0.5f, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsDarkViolet, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "dark violet herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsDarkGreen:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Speed, value: 0.5f, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsDarkGreen, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "dark green herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsBlue:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.RemovePoison, value: null, isPermanent: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsBlue, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "blue herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsCyan:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Haste, value: (int)2, autoRemoveDelay: 60 * 30, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsCyan, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "cyan herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsRed:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.HP, value: (float)200, isPermanent: true, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsRed, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "red herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.HerbsYellow:
                    {
                        var buffList = new List<Buff> {
                             new Buff(type: BuffEngine.BuffType.Strength, value: (int)2, autoRemoveDelay: 60 * 60 * 3, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HerbsYellow, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "yellow herbs", description: "Potion ingredient.", buffList: buffList);

                        break;
                    }

                case Name.EmptyBottle:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.EmptyBottle, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "empty bottle", description: "Can be used to make a potion.");

                        break;
                    }

                case Name.PotionGeneric:
                    {
                        // A generic potion, which animPackage and buffs will be set later.

                        boardPiece = new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionTransparent, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "potion", description: "A potion.", buffList: null);

                        break;
                    }

                case Name.BottleOfOil:
                    {
                        boardPiece = new Potion(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PotionLightYellow, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "bottle of oil", description: "Crafting material.", buffList: null);

                        break;
                    }

                case Name.MeatRawRegular:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatRawRegular,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList,
                             rotatesWhenDropped: true, readableName: "raw meat", description: "Poisonous, but safe after cooking.");

                        break;
                    }

                case Name.MeatRawPrime:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatRawPrime,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList, rotatesWhenDropped: true, readableName: "prime raw meat", description: "Poisonous, but safe after cooking.");

                        break;
                    }

                case Name.Mushroom:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)-30, autoRemoveDelay: 16 * 60, canKill: true, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Mushroom,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList,
                             rotatesWhenDropped: true, readableName: "mushroom", description: "Poisonous, but safe after cooking.");

                        break;
                    }

                case Name.MeatDried:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.RegenPoison, value: (int)20, autoRemoveDelay: 60 * 60, increaseIDAtEveryUse: true)};

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MeatDried, allowedTerrain: AllowedTerrain.GetBeachToVolcano(), buffList: buffList, rotatesWhenDropped: true, readableName: "dried meat", description: "Can be eaten or cooked.");

                        break;
                    }

                case Name.Fat:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Fat,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             rotatesWhenDropped: true, readableName: "fat", description: "Can be cooked or crafted.");

                        break;
                    }

                case Name.Leather:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Leather,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                             rotatesWhenDropped: true, readableName: "leather", description: "Crafting material.");

                        break;
                    }

                case Name.Burger:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Burger,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             rotatesWhenDropped: true, readableName: "burger", description: "Will remain fresh forever.");

                        break;
                    }

                case Name.Meal:
                    {
                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MealStandard,
                            allowedTerrain: AllowedTerrain.GetBeachToVolcano(), rotatesWhenDropped: true, readableName: "cooked meal", description: "Can be eaten.");

                        break;
                    }

                case Name.Rabbit:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitBrown, AnimData.PkgName.RabbitDarkBrown, AnimData.PkgName.RabbitGray, AnimData.PkgName.RabbitBlack, AnimData.PkgName.RabbitLightBrown };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.RabbitLightGray, AnimData.PkgName.RabbitBeige, AnimData.PkgName.RabbitWhite };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.GroundAll });

                        boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                            maxHitPoints: 200, maxAge: 30000, maxStamina: 300, eats: new List<Name> { Name.GrassRegular, Name.GrassDesert, Name.FlowersMountain, Name.FlowersPlain, Name.Apple, Name.Cherry, Name.TomatoPlant, Name.Tomato, Name.Meal, Name.Carrot, Name.CarrotPlant, Name.Mushroom, Name.CoffeeShrub }, strength: 30, readableName: "rabbit", description: "A small animal.");

                        break;
                    }

                case Name.Fox:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxGinger, AnimData.PkgName.FoxRed, AnimData.PkgName.FoxBlack, AnimData.PkgName.FoxChocolate, AnimData.PkgName.FoxBrown };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.FoxWhite, AnimData.PkgName.FoxGray, AnimData.PkgName.FoxYellow };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var allowedTerrain = new AllowedTerrain(rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.WaterShallow });

                        var eats = new List<Name> { Name.Rabbit, Name.MeatRawRegular, Name.MeatRawPrime, Name.Fat, Name.Burger, Name.MeatDried, Name.Meal, Name.Mushroom };

                        boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                         maxHitPoints: 400, maxAge: 30000, maxStamina: 800, eats: eats, strength: 30, readableName: "fox", description: "An animal.");

                        break;
                    }

                case Name.Bear:
                    {
                        var malePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.BearBrown, AnimData.PkgName.BearBlack, AnimData.PkgName.BearDarkBrown, AnimData.PkgName.BearGray, AnimData.PkgName.BearRed, AnimData.PkgName.BearBeige };

                        var femalePackageNames = new List<AnimData.PkgName> { AnimData.PkgName.BearWhite, AnimData.PkgName.BearOrange };

                        var maleAnimPkgName = malePackageNames[BoardPiece.Random.Next(malePackageNames.Count)];
                        var femaleAnimPkgName = femalePackageNames[BoardPiece.Random.Next(femalePackageNames.Count)];

                        var eats = new List<Name> { Name.MushroomPlant, Name.Mushroom, Name.MeatRawRegular, Name.MeatRawPrime, Name.Fat, Name.Burger, Name.MeatDried, Name.Meal };
                        eats.AddRange(PieceInfo.GetPlayerNames());

                        boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), speed: 1.5f,
                         maxHitPoints: 650, maxAge: 30000, maxStamina: 1200, eats: eats, strength: 75, readableName: "bear", description: "Cave animal.");

                        break;
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

                        boardPiece = new Animal(name: templateName, world: world, id: id, maleAnimPkgName: maleAnimPkgName, femaleAnimPkgName: femaleAnimPkgName, allowedTerrain: allowedTerrain, speed: 1.5f,
                            maxHitPoints: 150, maxAge: 30000, maxStamina: 200, eats: new List<Name> { Name.WaterLily, Name.Rushes }, strength: 30, readableName: "frog", description: "A water animal.");

                        break;
                    }

                case Name.KnifeSimple:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.KnifeSimple, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 1, readableName: "simple knife", description: "An old knife.");

                        break;
                    }

                case Name.AxeWood:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeWood, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 75, readableName: "wooden axe", description: "Basic logging tool.");

                        break;
                    }

                case Name.AxeStone:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 100, readableName: "stone axe", description: "Average logging tool.");

                        break;
                    }

                case Name.AxeIron:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 170, readableName: "iron axe", description: "Advanced logging tool.");

                        break;
                    }

                case Name.AxeCrystal:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.AxeCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 200, readableName: "crystal axe", description: "Deluxe logging tool.");

                        break;
                    }

                case Name.ShovelStone:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 80, readableName: "stone shovel", description: "Basic shovel.");

                        break;
                    }

                case Name.ShovelIron:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 100, readableName: "iron shovel", description: "Advanced shovel.");

                        break;
                    }

                case Name.ShovelCrystal:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ShovelCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 120, readableName: "crystal shovel", description: "Deluxe shovel.");

                        break;
                    }

                case Name.SpearWood:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearWood, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 50, readableName: "wooden spear", description: "Essential melee weapon.");

                        break;
                    }

                case Name.SpearStone:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 75, readableName: "stone spear", description: "Simple melee weapon.");

                        break;
                    }

                case Name.SpearIron:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 120, readableName: "iron spear", description: "Advanced melee weapon.");

                        break;
                    }

                case Name.SpearCrystal:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SpearCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 250, readableName: "crystal spear", description: "Deluxe melee weapon.");

                        break;
                    }

                case Name.PickaxeWood:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeWood, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 75, readableName: "wooden pickaxe", description: "Basic mining tool.");

                        break;
                    }

                case Name.PickaxeStone:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 100, readableName: "stone pickaxe", description: "Average mining tool.");

                        break;
                    }

                case Name.PickaxeIron:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 170, readableName: "iron pickaxe", description: "Advanced mining tool.");

                        break;
                    }

                case Name.PickaxeCrystal:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.PickaxeCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 200, readableName: "crystal pickaxe", description: "Deluxe mining tool.");

                        break;
                    }

                case Name.ScytheStone:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheStone, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 120, readableName: "stone scythe", description: "Can cut down small plants.");

                        break;
                    }

                case Name.ScytheIron:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheIron, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 200, readableName: "iron scythe", description: "Can cut down small plants easily.");

                        break;
                    }

                case Name.ScytheCrystal:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ScytheCrystal, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 250, readableName: "crystal scythe", description: "Brings an end to all small plants.");

                        break;
                    }

                case Name.BowBasic:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowBasic, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 150, readableName: "basic bow", description: "Projectile weapon.");

                        break;
                    }

                case Name.BowAdvanced:
                    {
                        boardPiece = new Tool(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BowAdvanced, allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(),
                              maxHitPoints: 300, readableName: "advanced bow", description: "Projectile weapon.");

                        break;
                    }

                case Name.ArrowWood:
                    {
                        boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowWood, allowedTerrain: new AllowedTerrain(), maxHitPoints: 15, readableName: "wooden arrow", description: "Very weak arrow.");

                        break;
                    }

                case Name.ArrowStone:
                    {
                        boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowStone, allowedTerrain: new AllowedTerrain(), maxHitPoints: 25, readableName: "stone arrow", description: "Basic arrow.");

                        break;
                    }

                case Name.ArrowIron:
                    {
                        boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowIron, allowedTerrain: new AllowedTerrain(), maxHitPoints: 40, readableName: "iron arrow", description: "Strong arrow.");

                        break;
                    }

                case Name.ArrowExploding:
                    {
                        boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowExploding, allowedTerrain: new AllowedTerrain(), maxHitPoints: 1, readableName: "exploding arrow", description: "Will start a fire.", lightEngine: new LightEngine(size: 100, opacity: 0.8f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true));

                        break;
                    }

                case Name.ArrowCrystal:
                    {
                        boardPiece = new Projectile(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.ArrowCrystal, allowedTerrain: new AllowedTerrain(), maxHitPoints: 50, readableName: "crystal arrow", description: "Deluxe arrow. Deals serious damage.");

                        break;
                    }

                case Name.TentModernPacked:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 0, max: Terrain.volcanoEdgeMin) }});

                        boardPiece = new Collectible(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentModernPacked, allowedTerrain: allowedTerrain, rotatesWhenDropped: true, readableName: "packed tent", description: "Can be assembled into a comfortable shelter.");

                        break;
                    }

                case Name.TentModern:
                    {
                        var wakeUpBuffs = new List<Buff> { };

                        SleepEngine sleepEngine = new(minFedPercent: 0.3f, fatigueRegen: 0.8f, hitPointsChange: 0.1f, minFatiguePercentPossibleToGet: 0f, updateMultiplier: 10, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentModern, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "modern tent", description: "Modern tent for sleeping.\nAlso allows waiting inside.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));

                        break;
                    }

                case Name.TentSmall:
                    {
                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -50f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new(minFedPercent: 0.2f, fatigueRegen: 0.56f, hitPointsChange: 0.05f, minFatiguePercentPossibleToGet: 0.25f, updateMultiplier: 8, canBeAttacked: false, waitingAfterSleepPossible: false, wakeUpBuffs: wakeUpBuffs);

                        boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentSmall, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "small tent", description: "Basic shelter for sleeping.\nNot very comfortable.");

                        break;
                    }

                case Name.TentMedium:
                    {
                        var wakeUpBuffs = new List<Buff> { };

                        SleepEngine sleepEngine = new(minFedPercent: 0.3f, fatigueRegen: 0.8f, hitPointsChange: 0.1f, minFatiguePercentPossibleToGet: 0f, updateMultiplier: 10, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentMedium, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 120, sleepEngine: sleepEngine, readableName: "medium tent", description: "Average shelter for sleeping.\nAlso allows waiting inside.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));

                        break;
                    }

                case Name.TentBig:
                    {
                        var wakeUpBuffs = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.Strength, value: 1, sleepMinutesNeededForActivation: 90, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)};

                        SleepEngine sleepEngine = new(minFedPercent: 0.5f, fatigueRegen: 1.3f, hitPointsChange: 0.25f, minFatiguePercentPossibleToGet: 0f, updateMultiplier: 14, canBeAttacked: false, waitingAfterSleepPossible: true, wakeUpBuffs: wakeUpBuffs);

                        boardPiece = new Shelter(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TentBig, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                             maxHitPoints: 200, sleepEngine: sleepEngine, readableName: "big tent", description: "Luxurious shelter for sleeping.\nAlso allows waiting inside.", lightEngine: new LightEngine(size: 0, opacity: 0.45f, colorActive: false, color: Color.Transparent, addedGfxRectMultiplier: 4f, isActive: true, castShadows: true));

                        break;
                    }

                case Name.BackpackSmall:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)2),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackSmall, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small backpack", description: "Expands inventory space.");

                        break;
                    }

                case Name.BackpackMedium:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)3),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackMedium, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium backpack", description: "Expands inventory space.");

                        break;
                    }

                case Name.BackpackBig:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)4),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)2)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackBig, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big backpack", description: "Expands inventory space.");

                        break;
                    }

                case Name.BackpackLuxurious:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.InvWidth, value: (byte)5),
                            new Buff(type: BuffEngine.BuffType.InvHeight, value: (byte)3)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BackpackLuxurious, equipType: Equipment.EquipType.Backpack,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "luxurious backpack", description: "Expands inventory space.");

                        break;
                    }

                case Name.BeltSmall:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)1)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltSmall, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "small belt", description: "Expands belt space.");

                        break;
                    }

                case Name.BeltMedium:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)3)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltMedium, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "medium belt", description: "Expands belt space.");

                        break;
                    }

                case Name.BeltBig:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)5)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltBig, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "big belt", description: "Expands belt space.");

                        break;
                    }

                case Name.BeltLuxurious:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.ToolbarWidth, value: (byte)6)};

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BeltLuxurious, equipType: Equipment.EquipType.Belt,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "luxurious belt", description: "Expands belt space.");

                        break;
                    }

                case Name.Map:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.EnableMap, value: null) };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Map, equipType: Equipment.EquipType.Map,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "map", description: "Keeps track of visited places.");

                        break;
                    }

                case Name.HatSimple:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.HeatProtection, value: null)
                        };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HatSimple, equipType: Equipment.EquipType.Head,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "hat", description: "Simple hat.");

                        break;
                    }

                case Name.BootsProtective:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.SwampProtection, value: null)
                        };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsProtective, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "protective boots", description: "Allow safe walking over swamp area.");

                        break;
                    }

                case Name.BootsMountain:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.FastMountainWalking, value: null) };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsMountain, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "mountain boots", description: "Allow fast walking in the mountains.");

                        break;
                    }

                case Name.BootsAllTerrain:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.SwampProtection, value: null),
                            new Buff(type: BuffEngine.BuffType.FastMountainWalking, value: null),
                        };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsAllTerrain, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "all terrain boots", description: "Allow walking over any terrain.");

                        break;
                    }

                case Name.BootsSpeed:
                    {
                        var buffList = new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.Speed, value: 1f),
                            new Buff(type: BuffEngine.BuffType.ExtendSprintDuration, value: 60 * 2),
                        };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BootsSpeed, equipType: Equipment.EquipType.Legs,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "boots of speed", description: "Increase speed.");

                        break;
                    }

                case Name.GlovesStrength:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.Strength, value: 1) };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlovesStrength, equipType: Equipment.EquipType.Accessory,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "gloves of strength", description: "Increase strength.");

                        break;
                    }

                case Name.GlassesVelvet:
                    {
                        var buffList = new List<Buff> { new Buff(type: BuffEngine.BuffType.CanSeeThroughFog, value: null) };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.GlassesBlue, equipType: Equipment.EquipType.Accessory,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "velvet glasses", description: "Pair of mysterious glasses.");

                        break;
                    }

                case Name.Dungarees:
                    {
                        var buffList = new List<Buff>
                        {
                           new Buff(type: BuffEngine.BuffType.MaxHP, value: 100f),
                           new Buff(type: BuffEngine.BuffType.MaxFatigue, value: 400f),
                        };

                        boardPiece = new Equipment(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Dungarees, equipType: Equipment.EquipType.Chest,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, buffList: buffList, maxHitPoints: 100, readableName: "dungarees", description: "Dungarees.");

                        break;
                    }

                case Name.TorchSmall:
                    {
                        LightEngine storedLightEngine = new LightEngine(size: 400, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true);

                        boardPiece = new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SmallTorch, canBeUsedDuringRain: false, storedLightEngine: storedLightEngine,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, maxHitPoints: 250, readableName: "small torch", description: "A portable light source.");

                        break;
                    }

                case Name.TorchBig:
                    {
                        LightEngine storedLightEngine = new LightEngine(size: 600, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true);

                        boardPiece = new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.BigTorch, canBeUsedDuringRain: false, storedLightEngine: storedLightEngine,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, maxHitPoints: 600, readableName: "big torch", description: "Burns for a long time.");

                        break;
                    }

                case Name.LanternFull:
                    {
                        LightEngine storedLightEngine = new LightEngine(size: 800, opacity: 0.9f, colorActive: true, color: Color.Orange * 0.2f, isActive: false, castShadows: true);

                        boardPiece = new PortableLight(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Lantern, canBeUsedDuringRain: true, storedLightEngine: storedLightEngine,
                            allowedTerrain: AllowedTerrain.GetShallowWaterToVolcano(), rotatesWhenDropped: true, maxHitPoints: 500, readableName: "lantern", description: "Can be used during rain. Refillable.", convertsToWhenUsedUp: Name.LanternEmpty);

                        break;
                    }

                case Name.LanternEmpty:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.LanternFrame, allowedTerrain: new AllowedTerrain(),
                             maxHitPoints: 400, readableName: "empty lantern", description: "Needs a candle to be put inside.", rotatesWhenDropped: true);

                        break;
                    }

                case Name.Candle:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Candle, allowedTerrain: new AllowedTerrain(),
                             maxHitPoints: 100, readableName: "candle", description: "Can be put inside lantern.", rotatesWhenDropped: true);

                        break;
                    }

                case Name.CampfireSmall:
                    {
                        ushort range = 150;

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.volcanoEdgeMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Fireplace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CampfireSmall, allowedTerrain: allowedTerrain, storageWidth: 2, storageHeight: 1, maxHitPoints: 30, readableName: "small campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        boardPiece.sprite.AssignNewName("off");

                        break;
                    }

                case Name.CampfireMedium:
                    {
                        ushort range = 210;

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.volcanoEdgeMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new Fireplace(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CampfireMedium, allowedTerrain: allowedTerrain, storageWidth: 2, storageHeight: 2, maxHitPoints: 30, readableName: "medium campfire", description: "When burning, emits light and scares off animals.", lightEngine: new LightEngine(size: range * 6, opacity: 1.0f, colorActive: true, color: Color.Orange * 0.3f, isActive: false, castShadows: true), scareRange: range);

                        boardPiece.sprite.AssignNewName("off");

                        break;
                    }

                case Name.PredatorRepellant:
                    {
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SkullAndBones, allowedTerrain: new AllowedTerrain(), readableName: "predator repellent", description: "Scares predators and is invisible.", activeState: BoardPiece.State.ScareAnimalsAway, visible: false);

                        break;
                    }

                case Name.Hole:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Hole, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 1, readableName: "hole", description: "Empty dig site.");

                        break;
                    }

                case Name.TreeStump:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.TreeStump, allowedTerrain: AllowedTerrain.GetBeachToVolcano(),
                             maxHitPoints: 50, readableName: "tree stump", description: "This was once a tree.");

                        break;
                    }

                case Name.HumanSkeleton:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.HumanSkeleton, allowedTerrain: new AllowedTerrain(),
                              maxHitPoints: 100, readableName: "skeleton", description: "Human remains.");

                        break;
                    }

                case Name.LavaFlame:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Flame, allowedTerrain: allowedTerrain, readableName: "lava flame", description: "Decorational flame on lava.", activeState: BoardPiece.State.Empty, visible: true, lightEngine: new LightEngine(size: 180, opacity: 0.45f, colorActive: true, color: Color.Orange * 0.6f, addedGfxRectMultiplier: 4f, isActive: true, glowOnlyAtNight: false, castShadows: true));

                        boardPiece.sprite.AssignNewSize((byte)BoardPiece.Random.Next(1, 4));
                        ParticleEngine.TurnOn(sprite: boardPiece.sprite, preset: ParticleEngine.Preset.LavaFlame, particlesToEmit: 1);
                        ParticleEngine.TurnOn(sprite: boardPiece.sprite, preset: ParticleEngine.Preset.HeatBig, particlesToEmit: 1);

                        break;
                    }

                case Name.WaterEdgeDistort:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.waterLevelMax - 40), max: (byte)(Terrain.waterLevelMax - 7)) },
                        });

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerZero, allowedTerrain: allowedTerrain, readableName: "water edge distortion", description: "Distorts water edge.", activeState: BoardPiece.State.Empty, visible: true);

                        boardPiece.sprite.opacity = 0;
                        ParticleEngine.TurnOn(sprite: boardPiece.sprite, preset: ParticleEngine.Preset.DistortWaterEdge, particlesToEmit: 1);

                        break;
                    }

                case Name.WeatherFog:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.WeatherFog1, AnimData.PkgName.WeatherFog2, AnimData.PkgName.WeatherFog3, AnimData.PkgName.WeatherFog4, AnimData.PkgName.WeatherFog5, AnimData.PkgName.WeatherFog6, AnimData.PkgName.WeatherFog7, AnimData.PkgName.WeatherFog8, AnimData.PkgName.WeatherFog9 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: new AllowedTerrain(), readableName: "weather fog", description: "Localized clump of fog.", activeState: BoardPiece.State.WeatherFogMoveRandomly);

                        boardPiece.sprite.opacity = (SonOfRobinGame.random.NextSingle() * 0.2f) + 0.15f;

                        break;
                    }

                case Name.SwampGas:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, readableName: "gas", description: "Swamp gas.", activeState: BoardPiece.State.FogMoveRandomly);

                        boardPiece.sprite.color = Color.LimeGreen;
                        boardPiece.sprite.opacity = 0.4f;

                        break;
                    }

                case Name.SwampGeyser:
                    {
                        var allowedTerrain = new AllowedTerrain(
                            rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Biome, new AllowedRange(min: 190, max: 255) }},
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.BiomeSwamp, true } });

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerZero, allowedTerrain: allowedTerrain, readableName: "swamp geyser", description: "Swamp geyser.", activeState: BoardPiece.State.Empty);

                        boardPiece.sprite.opacity = 0;

                        ParticleEngine.TurnOn(sprite: boardPiece.sprite, preset: ParticleEngine.Preset.SwampGas, particlesToEmit: 1);
                        ParticleEngine.TurnOn(sprite: boardPiece.sprite, preset: ParticleEngine.Preset.HeatMedium, particlesToEmit: 1);

                        break;
                    }

                case Name.LavaGas:
                    {
                        var packageNames = new AnimData.PkgName[] { AnimData.PkgName.Fog1, AnimData.PkgName.Fog2, AnimData.PkgName.Fog3, AnimData.PkgName.Fog4 };
                        var animPkg = packageNames[BoardPiece.Random.Next(packageNames.Length)];

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: animPkg, allowedTerrain: allowedTerrain, readableName: "gas", description: "Lava gas.", activeState: BoardPiece.State.FogMoveRandomly, visible: true);

                        boardPiece.sprite.color = new Color(255, 206, 28);
                        boardPiece.sprite.opacity = 0.45f;

                        break;
                    }

                case Name.Explosion:
                    {
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Explosion, allowedTerrain: new AllowedTerrain(), readableName: "explosion", description: "An explosion.", activeState: BoardPiece.State.Empty, visible: true, lightEngine: new LightEngine(size: 150, opacity: 1f, colorActive: true, color: Color.Orange * 0.3f, isActive: true, castShadows: true));

                        break;
                    }

                case Name.SeaWave:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, true } });

                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.SeaWave, allowedTerrain: allowedTerrain, readableName: "sea wave", description: "Sea wave.", activeState: BoardPiece.State.SeaWaveMove, visible: true);

                        boardPiece.sprite.opacity = 0f;
                        if (world != null) boardPiece.sprite.AssignNewSize((byte)world.random.Next(0, 7));

                        break;
                    }

                case Name.SoundSeaWind:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, true } });

                        boardPiece = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient sea wind sound", description: "Ambient sound for sea wind.", visible: Preferences.debugShowSounds);

                        boardPiece.sprite.color = Color.White;

                        break;
                    }

                case Name.SoundLakeWaves:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(
                            rangeNameList: new List<AllowedTerrain.RangeName>() { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.NoBiome },
                            extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.Sea, false } });

                        boardPiece = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient lake waves sound", description: "Ambient sound for lake waves.", visible: Preferences.debugShowSounds);

                        boardPiece.sprite.color = Color.Blue;

                        break;
                    }

                case Name.SoundNightCrickets:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient night crickets sound", description: "Ambient sound for crickets at night.", visible: Preferences.debugShowSounds);

                        boardPiece.sprite.color = Color.Black;

                        break;
                    }

                case Name.SoundNoonCicadas:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 115, max: 150) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 120, max: 255) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient noon cicadas sound", description: "Ambient sound for cicadas at noon.", visible: Preferences.debugShowSounds);

                        boardPiece.sprite.color = Color.Green;

                        break;
                    }

                case Name.SoundLava:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: (byte)(Terrain.lavaMin + 1), max: 255) },
                        });

                        boardPiece = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient lava sound", description: "Ambient sound for lava.", visible: Preferences.debugShowSounds);

                        boardPiece.sprite.color = Color.Yellow;

                        break;
                    }

                case Name.SoundCaveWaterDrip:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 100, max: Terrain.volcanoEdgeMin - 1) },
                        });

                        boardPiece = new AmbientSound(name: templateName, world: world, id: id, allowedTerrain: allowedTerrain, readableName: "ambient cave water", description: "Ambient sound for cave water dripping.", visible: Preferences.debugShowSounds);

                        boardPiece.sprite.color = Color.Blue;

                        break;
                    }

                case Name.ParticleEmitterEnding:
                    {
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerZero, allowedTerrain: new AllowedTerrain(), readableName: "particle emitter", description: "Emits particles.", activeState: BoardPiece.State.EmitParticles, visible: true);

                        boardPiece.sprite.opacity = 0f;

                        break;
                    }

                case Name.ParticleEmitterWeather:
                    {
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.WhiteSpotLayerThree, allowedTerrain: new AllowedTerrain(), readableName: "weather particle emitter", description: "Emits particles (for weather effects).", activeState: BoardPiece.State.Empty, visible: true);

                        boardPiece.sprite.opacity = 0f;

                        break;
                    }

                case Name.EmptyVisualEffect:
                    {
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.Empty, allowedTerrain: new AllowedTerrain(), readableName: "empty visual effect", description: "Empty visual effect.", activeState: BoardPiece.State.Empty, visible: true);

                        break;
                    }

                case Name.HastePlayerClone:
                    {
                        boardPiece = new VisualEffect(name: templateName, world: world, id: id, animPackage: world?.Player != null ? world.Player.sprite.AnimPkg.name : AnimData.PkgName.Empty, allowedTerrain: new AllowedTerrain(), readableName: "haste player clone", description: "Haste player clone.", activeState: BoardPiece.State.HasteCloneFollowPlayer, visible: true);

                        break;
                    }

                case Name.FertileGroundSmall:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new FertileGround(world: world, id: id, animPackage: AnimData.PkgName.FertileGroundSmall, name: templateName, allowedTerrain: allowedTerrain, readableName: "fertile ground (small)", description: "Seeds can be planted here.", maxHitPoints: 60);

                        break;
                    }

                case Name.FertileGroundMedium:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new FertileGround(world: world, id: id, animPackage: AnimData.PkgName.FertileGroundMedium, name: templateName, allowedTerrain: allowedTerrain, readableName: "fertile ground (medium)", description: "Seeds can be planted here.", maxHitPoints: 80);

                        break;
                    }

                case Name.FertileGroundLarge:
                    {
                        AllowedTerrain allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 105, max: Terrain.rocksLevelMin) },
                        }, extPropertiesDict: ExtBoardProps.GetNoBiomeExtProps());

                        boardPiece = new FertileGround(world: world, id: id, animPackage: AnimData.PkgName.FertileGroundLarge, name: templateName, allowedTerrain: allowedTerrain, readableName: "fertile ground (large)", description: "Seeds can be planted here.", maxHitPoints: 100);

                        break;
                    }

                case Name.FenceHorizontalShort:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceHorizontalShort, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 120, readableName: "short fence (horizontal)", description: "A short fence.");

                        break;
                    }

                case Name.FenceVerticalShort:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceVerticalShort, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 120, readableName: "short fence (vertical)", description: "A short fence.");

                        break;
                    }

                case Name.FenceHorizontalLong:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceHorizontalLong, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 220, readableName: "long fence (horizontal)", description: "A long fence.");

                        break;
                    }

                case Name.FenceVerticalLong:
                    {
                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.FenceVerticalLong, allowedTerrain: AllowedTerrain.GetFieldCraft(),
                              maxHitPoints: 220, readableName: "long fence (vertical)", description: "A long fence.");

                        break;
                    }

                case Name.CaveEntranceOutside:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: Terrain.rocksLevelMin + 1, max: Terrain.volcanoEdgeMin - 1) },
                            { Terrain.Name.Humidity, new AllowedRange(min: 0, max: 128) },
                        });

                        boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveEntrance, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave entrance", description: "Cave entrance.", goesDown: true, maxDepth: 1, levelType: Level.LevelType.Cave, hasWater: false, activeState: BoardPiece.State.CaveEntranceDisappear);

                        break;
                    }

                case Name.CaveEntranceInside:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 118, max: 123) },
                            });

                        boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveEntrance, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave entrance", description: "Cave entrance.", goesDown: true, maxDepth: 4, levelType: Level.LevelType.Cave, hasWater: false, activeState: BoardPiece.State.CaveEntranceDisappear);

                        break;
                    }

                case Name.CaveExit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 118, max: 119) },
                            });

                        boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveExit, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave exit", description: "Cave exit.", goesDown: false, maxDepth: 9999, levelType: Level.LevelType.Cave, hasWater: false); // levelType and hasWater are ignored here

                        boardPiece.sprite.lightEngine = new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.LightBlue * 0.3f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        break;
                    }

                case Name.CaveExitEmergency:
                    {
                        // the same as regular, but can be placed anywhere

                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 118, max: Terrain.volcanoEdgeMin - 1) },
                            });

                        boardPiece = new Entrance(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CaveExit, allowedTerrain: allowedTerrain,
                              maxHitPoints: 220, readableName: "cave exit", description: "Cave exit.", goesDown: false, maxDepth: 9999, levelType: Level.LevelType.Cave, hasWater: false); // levelType and hasWater are ignored here

                        boardPiece.sprite.lightEngine = new LightEngine(size: 500, opacity: 1.0f, colorActive: true, color: Color.LightBlue * 0.3f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        break;
                    }

                case Name.CaveWeakMinerals:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: 135) },
                            });

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.MineralsCave, allowedTerrain: allowedTerrain,
                               maxHitPoints: 10, readableName: "weak minerals", description: "Weak cave minerals.");

                        break;
                    }

                case Name.IronDeposit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: 125) }});

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.IronDeposit, allowedTerrain: allowedTerrain,
                            maxHitPoints: 250, readableName: "iron deposit", description: "Can be mined for iron.");

                        break;
                    }

                case Name.CoalDeposit:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: 125) }});

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CoalDeposit, allowedTerrain: allowedTerrain,
                            maxHitPoints: 250, readableName: "coal deposit", description: "Can be mined for coal.");

                        break;
                    }

                case Name.CrystalDepositBig:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: Terrain.volcanoEdgeMin - 4) }});

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositBig, allowedTerrain: allowedTerrain,
                            maxHitPoints: 250, readableName: "big crystal deposit", description: "Can be mined for crystals.");

                        boardPiece.sprite.lightEngine = new LightEngine(size: 400, opacity: 1.2f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        break;
                    }

                case Name.CrystalDepositSmall:
                    {
                        var allowedTerrain = new AllowedTerrain(rangeDict: new Dictionary<Terrain.Name, AllowedRange>() {
                            { Terrain.Name.Height, new AllowedRange(min: 116, max: Terrain.volcanoEdgeMin - 4) }});

                        boardPiece = new Decoration(name: templateName, world: world, id: id, animPackage: AnimData.PkgName.CrystalDepositSmall, allowedTerrain: allowedTerrain,
                            maxHitPoints: 125, readableName: "small crystal deposit", description: "Can be mined for crystals.");

                        boardPiece.sprite.lightEngine = new LightEngine(size: 650, opacity: 0.4f, colorActive: true, color: Color.Blue * 5f, isActive: true, castShadows: true);
                        boardPiece.sprite.lightEngine.AssignSprite(boardPiece.sprite);

                        break;
                    }

                default: { throw new ArgumentException($"Unsupported template name - {templateName}."); }
            }

            if (createdByPlayer) // has to be checked for every created piece
            {
                boardPiece.createdByPlayer = true;
                boardPiece.canBeHit = false; // to protect item from accidental player hit
            }

            return boardPiece; // every piece created should be returned here
        }

        private static AllowedTerrain CreatePlayerAllowedTerrain()
        {
            return new AllowedTerrain(
                rangeNameList: new List<AllowedTerrain.RangeName> { AllowedTerrain.RangeName.WaterShallow, AllowedTerrain.RangeName.WaterMedium, AllowedTerrain.RangeName.GroundAll, AllowedTerrain.RangeName.Volcano, AllowedTerrain.RangeName.NoBiome },
                extPropertiesDict: new Dictionary<ExtBoardProps.Name, bool> { { ExtBoardProps.Name.OuterBeach, true } });
        }
    }
}