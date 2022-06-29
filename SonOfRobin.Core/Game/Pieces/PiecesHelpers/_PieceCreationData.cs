using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public struct PieceCreationData
    {
        public readonly PieceTemplate.Name name;
        public readonly float multiplier;
        public readonly int maxAmount;
        public readonly bool doNotReplenish;

        public PieceCreationData(PieceTemplate.Name name, float multiplier, int maxAmount, bool doNotReplenish = false)
        {
            this.name = name;
            this.multiplier = multiplier;
            this.maxAmount = maxAmount; // -1 == no limit
            this.doNotReplenish = doNotReplenish; // create missing after world creation
        }

        public static List<PieceCreationData> CreateDataList(bool addAgressiveAnimals, int maxAnimalsPerName)
        {
            var dataList = new List<PieceCreationData>
                {
                     new PieceCreationData(name: PieceTemplate.Name.GrassRegular, multiplier: 2.0f, maxAmount: 1000),
                     new PieceCreationData(name: PieceTemplate.Name.GrassGlow, multiplier: 0.1f, maxAmount: 40),
                     new PieceCreationData(name: PieceTemplate.Name.GrassDesert, multiplier: 2.0f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.Rushes, multiplier: 2.0f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.WaterLily, multiplier: 1.0f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.FlowersPlain, multiplier: 0.4f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.FlowersRed, multiplier: 0.1f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.FlowersMountain, multiplier: 0.1f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.TreeSmall, multiplier: 1.0f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.TreeBig, multiplier: 1.0f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.AcornTree, multiplier: 0.03f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.AppleTree, multiplier: 0.03f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.CherryTree, multiplier: 0.03f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.BananaTree, multiplier: 0.03f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.TomatoPlant, multiplier: 0.03f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.PalmTree, multiplier: 1.0f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.Cactus, multiplier: 0.2f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.WaterRock, multiplier: 0.5f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.MineralsSmall, multiplier: 0.5f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.MineralsBig, multiplier: 0.3f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.IronDeposit, multiplier: 0.02f, maxAmount: 30),
                     new PieceCreationData(name: PieceTemplate.Name.CoalDeposit, multiplier: 0.02f, maxAmount: 30),
                     new PieceCreationData(name: PieceTemplate.Name.CrystalDepositBig, multiplier: 0.01f, maxAmount: 10),
                     new PieceCreationData(name: PieceTemplate.Name.Shell, multiplier: 1f, maxAmount: 25),
                     new PieceCreationData(name: PieceTemplate.Name.Clam, multiplier: 1f, maxAmount: 25),
                     new PieceCreationData(name: PieceTemplate.Name.BeachDigSite, multiplier: 0.8f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.ForestDigSite, multiplier: 0.3f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.DesertDigSite, multiplier: 0.2f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.GlassDigSite, multiplier: 0.1f, maxAmount: 200),
                     new PieceCreationData(name: PieceTemplate.Name.DangerDigSite, multiplier: 0.15f, maxAmount: -1),
                     new PieceCreationData(name: PieceTemplate.Name.CrateRegular, multiplier: 0.1f, maxAmount: 2),
                     new PieceCreationData(name: PieceTemplate.Name.Rabbit, multiplier: 0.6f, maxAmount: maxAnimalsPerName),
                     new PieceCreationData(name: PieceTemplate.Name.Fox, multiplier: 0.4f, maxAmount: maxAnimalsPerName),
                     new PieceCreationData(name: PieceTemplate.Name.Tiger, multiplier: 0.4f, maxAmount: maxAnimalsPerName),
                     new PieceCreationData(name: PieceTemplate.Name.Frog, multiplier: 0.2f, maxAmount: maxAnimalsPerName),
                     new PieceCreationData(name: PieceTemplate.Name.LavaLight, multiplier: 0.5f, maxAmount: -1, doNotReplenish: true),
                     new PieceCreationData(name: PieceTemplate.Name.SoundSeaWaves, multiplier: 0.4f, maxAmount: -1, doNotReplenish: true),
                     new PieceCreationData(name: PieceTemplate.Name.SoundSeaWind, multiplier: 0.4f, maxAmount: -1, doNotReplenish: true),
                     new PieceCreationData(name: PieceTemplate.Name.SoundLakeWaves, multiplier: 1.2f, maxAmount: -1, doNotReplenish: true),
                     new PieceCreationData(name: PieceTemplate.Name.SoundDesertWind, multiplier: 1.0f, maxAmount: -1, doNotReplenish: true),
                     new PieceCreationData(name: PieceTemplate.Name.SoundNightCrickets, multiplier: 1.0f, maxAmount: -1, doNotReplenish: true),
                     new PieceCreationData(name: PieceTemplate.Name.SoundNoonCicadas, multiplier: 1.0f, maxAmount: -1, doNotReplenish: true),
                };

            if (!addAgressiveAnimals) dataList = dataList.Where(data => PieceInfo.GetInfo(data.name).eats == null || !PieceInfo.GetInfo(data.name).eats.Contains(PieceTemplate.Name.Player)).ToList();

            return dataList;
        }
    }

}
