using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public readonly struct PieceCreationData
    {
        public readonly Level.LevelType levelType;
        public readonly PieceTemplate.Name name;
        public readonly float multiplier;
        public readonly int tempDecorMultiplier;
        public readonly bool temporaryDecoration;
        public readonly bool doNotReplenish;
        private readonly int minDepth;
        private readonly int maxAmount;
        public readonly int maxAmountIncreaseForDepthLevel;

        public PieceCreationData(PieceTemplate.Name name, Level.LevelType levelType, float multiplier = 1, int maxAmount = -1, bool temporaryDecoration = false, int tempDecorMultiplier = 1, bool doNotReplenish = false, int minDepth = 0, int maxAmountIncreaseForDepthLevel = 0)
        {
            this.levelType = levelType;
            this.name = name;
            this.multiplier = multiplier;
            this.tempDecorMultiplier = tempDecorMultiplier;
            this.maxAmount = maxAmount; // -1 == no limit
            this.temporaryDecoration = temporaryDecoration; // only created dynamically in camera view and not saved (ignores multiplier and maxAmount)
            this.doNotReplenish = doNotReplenish;
            this.minDepth = minDepth;
            this.maxAmountIncreaseForDepthLevel = maxAmountIncreaseForDepthLevel;
        }

        public int GetMaxAmount(Level level)
        {
            return this.maxAmount == -1 || this.maxAmountIncreaseForDepthLevel == 0 ?
                this.maxAmount :
                this.maxAmount + (this.maxAmountIncreaseForDepthLevel * Math.Max(level.depth - this.minDepth, 0));
        }

        public static List<PieceCreationData> CreateDataList(int maxAnimalsPerName, Level level)
        {
            var dataList = new List<PieceCreationData>
            {
                new PieceCreationData(name: PieceTemplate.Name.GrassRegular, multiplier: 1.7f, maxAmount: 1000, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.GrassGlow, multiplier: 0.1f, maxAmount: 60, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.GrassDesert, multiplier: 2.0f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.PlantPoison, multiplier: 1.0f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.Rushes, multiplier: 2.0f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.WaterLily, multiplier: 0.15f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.FlowersPlain, multiplier: 0.4f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.FlowersRed, multiplier: 0.1f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.FlowersMountain, multiplier: 0.1f, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.TreeSmall, multiplier: 1.0f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.TreeBig, multiplier: 1.0f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.Oak, multiplier: 0.03f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.AppleTree, multiplier: 0.03f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.CherryTree, multiplier: 0.03f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.BananaTree, multiplier: 0.03f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.TomatoPlant, multiplier: 0.005f, maxAmount: 10, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.CarrotPlant, multiplier: 0.005f, maxAmount: 10, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.CoffeeShrub, multiplier: 0.002f, maxAmount: 4, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.PalmTree, multiplier: 1.0f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.Cactus, multiplier: 0.2f, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.MineralsSmall, multiplier: 0.12f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.MineralsBig, multiplier: 0.1f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.MineralsMossySmall, multiplier: 0.2f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.MineralsMossyBig, multiplier: 0.1f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.Clam, multiplier: 1f, maxAmount: 25, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.CrateRegular, multiplier: 0.1f, maxAmount: 2, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.BeachDigSite, multiplier: 0.3f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.ForestDigSite, multiplier: 0.22f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.DesertDigSite, multiplier: 0.1f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.GlassDigSite, multiplier: 0.08f, maxAmount: 200, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SwampDigSite, multiplier: 0.11f, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.RuinsDigSite, multiplier: 0.12f, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.Totem, multiplier: 0.1f, maxAmount: 2, doNotReplenish: true, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.RuinsColumn, multiplier: 0.04f, doNotReplenish: true, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.RuinsRubble, multiplier: 0.07f, doNotReplenish: true, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.RuinsWall, multiplier: 0.07f, doNotReplenish: true, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.Rabbit, multiplier: 0.6f, maxAmount: maxAnimalsPerName, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.Fox, multiplier: 0.4f, maxAmount: maxAnimalsPerName, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.Frog, multiplier: 0.2f, maxAmount: maxAnimalsPerName, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.LavaFlame, temporaryDecoration: true, tempDecorMultiplier: 2, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.LavaGas, temporaryDecoration: true, tempDecorMultiplier: 2, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SwampGas, temporaryDecoration: true, tempDecorMultiplier: 3, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SwampGeyser, temporaryDecoration: true, tempDecorMultiplier: 1, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.SeaWave, temporaryDecoration: true, tempDecorMultiplier: 1, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.SoundSeaWind, temporaryDecoration: true, tempDecorMultiplier: 1, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SoundLakeWaves, temporaryDecoration: true, tempDecorMultiplier: 2, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SoundNightCrickets, temporaryDecoration: true, tempDecorMultiplier: 1, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SoundNoonCicadas, temporaryDecoration: true, tempDecorMultiplier: 1, levelType: Level.LevelType.Island),
                new PieceCreationData(name: PieceTemplate.Name.SoundLava, temporaryDecoration: true, tempDecorMultiplier: 2, levelType: Level.LevelType.Island),

                new PieceCreationData(name: PieceTemplate.Name.CaveEntranceOutside, multiplier: 0.2f, maxAmount: 6, levelType: Level.LevelType.Island),

                // cave

                new PieceCreationData(name: PieceTemplate.Name.CaveEntranceInside, multiplier: 1.2f, minDepth: 1, maxAmount: 2, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.CaveWeakMinerals, multiplier: 6f, minDepth: 1, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.CoalDeposit, multiplier: 0.5f, minDepth: 1, maxAmount: 10, maxAmountIncreaseForDepthLevel: 8, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.IronDeposit, multiplier: 0.5f, minDepth: 2, maxAmount: 10, maxAmountIncreaseForDepthLevel: 5, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.CrystalDepositSmall, multiplier: 0.5f, maxAmount: 4, minDepth: 3, maxAmountIncreaseForDepthLevel: 1, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.CrystalDepositBig, multiplier: 0.5f, maxAmount: 3, minDepth: 4, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.MushroomPlant, multiplier: 0.5f, maxAmount: 50, minDepth: 1, doNotReplenish: false, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.Bear, multiplier: 1.5f, maxAmount: 13, minDepth: 1, maxAmountIncreaseForDepthLevel: 8, doNotReplenish: true, levelType: Level.LevelType.Cave),

                // Caves don't use temporaryDecoration, because it is better to just create everything at the start (caves are small).
                // This way, some pieces can be placed with better precision.

                new PieceCreationData(name: PieceTemplate.Name.LavaFlame, multiplier: 1.0f, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.LavaGas, multiplier: 1.0f, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.SoundLava, multiplier: 1.0f, doNotReplenish: true, levelType: Level.LevelType.Cave),
                new PieceCreationData(name: PieceTemplate.Name.SoundCaveWaterDrip, multiplier: 1.0f, doNotReplenish: true, levelType: Level.LevelType.Cave),
                };

            //{ // for testing creation of selected pieces
            //    List<PieceTemplate.Name> debugNamesToCheck = new List<PieceTemplate.Name> { PieceTemplate.Name.MineralsMossyBig, PieceTemplate.Name.MineralsMossySmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.MineralsSmall };
            //    dataList = dataList.Where(record => debugNamesToCheck.Contains(record.name)).ToList();
            //}

            return dataList = dataList.Where(creationData => level.levelType == creationData.levelType && level.depth >= creationData.minDepth).ToList();
        }
    }
}