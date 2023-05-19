using System.Collections.Generic;

namespace SonOfRobin
{
    public struct PieceCreationData
    {
        public readonly PieceTemplate.Name name;
        public readonly float multiplier;
        public readonly int tempDecorMultiplier;
        public readonly int maxAmount;
        public readonly bool temporaryDecoration;

        public PieceCreationData(PieceTemplate.Name name, float multiplier = 1, int maxAmount = -1, bool temporaryDecoration = false, int tempDecorMultiplier = 1)
        {
            this.name = name;
            this.multiplier = multiplier;
            this.tempDecorMultiplier = tempDecorMultiplier;
            this.maxAmount = maxAmount; // -1 == no limit
            this.temporaryDecoration = temporaryDecoration; // only created dynamically in camera view and not saved (ignores multiplier and maxAmount)
        }

        public static List<PieceCreationData> CreateDataList(int maxAnimalsPerName)
        {
            var dataList = new List<PieceCreationData>
            {
                new PieceCreationData(name: PieceTemplate.Name.GrassRegular, multiplier: 2.0f, maxAmount: 1000),
                new PieceCreationData(name: PieceTemplate.Name.GrassGlow, multiplier: 0.1f, maxAmount: 40),
                new PieceCreationData(name: PieceTemplate.Name.GrassDesert, multiplier: 2.0f),
                new PieceCreationData(name: PieceTemplate.Name.PlantPoison, multiplier: 1.0f),
                new PieceCreationData(name: PieceTemplate.Name.Rushes, multiplier: 2.0f),
                new PieceCreationData(name: PieceTemplate.Name.WaterLily, multiplier: 0.15f),
                new PieceCreationData(name: PieceTemplate.Name.FlowersPlain, multiplier: 0.4f),
                new PieceCreationData(name: PieceTemplate.Name.FlowersRed, multiplier: 0.1f),
                new PieceCreationData(name: PieceTemplate.Name.FlowersMountain, multiplier: 0.1f),

                new PieceCreationData(name: PieceTemplate.Name.TreeSmall, multiplier: 1.0f),
                new PieceCreationData(name: PieceTemplate.Name.TreeBig, multiplier: 1.0f),
                new PieceCreationData(name: PieceTemplate.Name.Oak, multiplier: 0.03f),
                new PieceCreationData(name: PieceTemplate.Name.AppleTree, multiplier: 0.03f),
                new PieceCreationData(name: PieceTemplate.Name.CherryTree, multiplier: 0.03f),
                new PieceCreationData(name: PieceTemplate.Name.BananaTree, multiplier: 0.03f),
                new PieceCreationData(name: PieceTemplate.Name.TomatoPlant, multiplier: 0.01f),
                new PieceCreationData(name: PieceTemplate.Name.CarrotPlant, multiplier: 0.01f),
                new PieceCreationData(name: PieceTemplate.Name.CoffeeShrub, multiplier: 0.01f),
                new PieceCreationData(name: PieceTemplate.Name.PalmTree, multiplier: 1.0f),
                new PieceCreationData(name: PieceTemplate.Name.Cactus, multiplier: 0.2f),

                new PieceCreationData(name: PieceTemplate.Name.MineralsSmall, multiplier: 0.12f),
                new PieceCreationData(name: PieceTemplate.Name.MineralsBig, multiplier: 0.1f),
                new PieceCreationData(name: PieceTemplate.Name.MineralsMossySmall, multiplier: 0.2f),
                new PieceCreationData(name: PieceTemplate.Name.MineralsMossyBig, multiplier: 0.1f),
                new PieceCreationData(name: PieceTemplate.Name.IronDeposit, multiplier: 0.02f, maxAmount: 35),
                new PieceCreationData(name: PieceTemplate.Name.CoalDeposit, multiplier: 0.02f, maxAmount: 35),
                new PieceCreationData(name: PieceTemplate.Name.CrystalDepositBig, multiplier: 0.01f, maxAmount: 10),

                // new PieceCreationData(name: PieceTemplate.Name.Shell, multiplier: 1f, maxAmount: 25), // turned off until it will be used in some recipe
                new PieceCreationData(name: PieceTemplate.Name.Clam, multiplier: 1f, maxAmount: 25),
                new PieceCreationData(name: PieceTemplate.Name.CrateRegular, multiplier: 0.1f, maxAmount: 2),

                new PieceCreationData(name: PieceTemplate.Name.BeachDigSite, multiplier: 0.3f),
                new PieceCreationData(name: PieceTemplate.Name.ForestDigSite, multiplier: 0.22f),
                new PieceCreationData(name: PieceTemplate.Name.DesertDigSite, multiplier: 0.1f),
                new PieceCreationData(name: PieceTemplate.Name.GlassDigSite, multiplier: 0.08f, maxAmount: 200),
                new PieceCreationData(name: PieceTemplate.Name.SwampDigSite, multiplier: 0.11f),

                new PieceCreationData(name: PieceTemplate.Name.Rabbit, multiplier: 0.6f, maxAmount: maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Fox, multiplier: 0.4f, maxAmount: maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Tiger, multiplier: 0.4f, maxAmount: maxAnimalsPerName),
                new PieceCreationData(name: PieceTemplate.Name.Frog, multiplier: 0.2f, maxAmount: maxAnimalsPerName),

                new PieceCreationData(name: PieceTemplate.Name.LavaFlame, temporaryDecoration: true, tempDecorMultiplier: 2),
                new PieceCreationData(name: PieceTemplate.Name.SwampGas, temporaryDecoration: true, tempDecorMultiplier: 3),
                new PieceCreationData(name: PieceTemplate.Name.LavaGas, temporaryDecoration: true, tempDecorMultiplier: 2),

                new PieceCreationData(name: PieceTemplate.Name.SoundSeaWaves, temporaryDecoration: true, tempDecorMultiplier: 1),
                new PieceCreationData(name: PieceTemplate.Name.SoundSeaWind, temporaryDecoration: true, tempDecorMultiplier: 1),
                new PieceCreationData(name: PieceTemplate.Name.SoundLakeWaves, temporaryDecoration: true, tempDecorMultiplier: 2),
                new PieceCreationData(name: PieceTemplate.Name.SoundNightCrickets, temporaryDecoration: true, tempDecorMultiplier: 1),
                new PieceCreationData(name: PieceTemplate.Name.SoundNoonCicadas, temporaryDecoration: true, tempDecorMultiplier: 1),
                new PieceCreationData(name: PieceTemplate.Name.SoundLava, temporaryDecoration: true, tempDecorMultiplier: 2),
                };

            //{ // for testing creation of selected pieces
            //    List<PieceTemplate.Name> debugNamesToCheck = new List<PieceTemplate.Name> { PieceTemplate.Name.MineralsMossyBig, PieceTemplate.Name.MineralsMossySmall, PieceTemplate.Name.MineralsBig, PieceTemplate.Name.MineralsSmall };
            //    dataList = dataList.Where(record => debugNamesToCheck.Contains(record.name)).ToList();
            //}

            return dataList;
        }
    }
}