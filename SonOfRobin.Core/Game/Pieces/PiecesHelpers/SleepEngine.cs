using System;

namespace SonOfRobin
{
    [Serializable]
    public struct SleepEngine
    {
        public static SleepEngine OutdoorSleepDry = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.4f, hitPointsChange: -0.1f, islandClockMultiplier: 2, canBeAttacked: true);
        public static SleepEngine OutdoorSleepWet = new SleepEngine(minFedPercent: 0.1f, fatigueRegen: 0.2f, hitPointsChange: -0.2f, islandClockMultiplier: 2, canBeAttacked: true);

        private readonly float minFedPercent;
        private readonly float fatigueRegen;
        private readonly float hitPointsChange;
        public readonly bool canBeAttacked;
        public readonly int islandClockMultiplier;

        public SleepEngine(float minFedPercent, float fatigueRegen, int islandClockMultiplier, bool canBeAttacked, float hitPointsChange = 0f)
        {
            this.minFedPercent = minFedPercent;
            this.hitPointsChange = hitPointsChange;
            this.fatigueRegen = fatigueRegen;
            this.canBeAttacked = canBeAttacked;
            this.islandClockMultiplier = islandClockMultiplier;
        }

        public void Execute(Player player)
        {
            player.Fatigue -= this.fatigueRegen;
            if (player.FedPercent > this.minFedPercent) player.ExpendEnergy(energyAmount: 0.1f, addFatigue: false);

            player.hitPoints += this.hitPointsChange;
            if (this.hitPointsChange > 0) player.hitPoints = Math.Min(player.hitPoints, player.maxHitPoints);
            else player.hitPoints = Math.Max(player.hitPoints, player.maxHitPoints * 0.15f);
        }

    }
}
