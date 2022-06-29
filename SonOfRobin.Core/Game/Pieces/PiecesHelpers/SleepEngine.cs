using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SonOfRobin
{
    [Serializable]
    public struct SleepEngine
    {
        public static SleepEngine OutdoorSleepDry = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.1f, hitPointsDecrease: 0.1f, canBeAttacked: true);
        public static SleepEngine OutdoorSleepWet = new SleepEngine(minFedPercent: 0.1f, fatigueRegen: 0.05f, hitPointsDecrease: 0.2f, canBeAttacked: true);

        private readonly float minFedPercent;
        private readonly float fatigueRegen;
        private readonly float hitPointsRegen;
        private readonly float hitPointsDecrease;
        public readonly bool canBeAttacked;

        public SleepEngine(float minFedPercent, float fatigueRegen, bool canBeAttacked, float hitPointsRegen = 0f, float hitPointsDecrease = 0f, List<BuffEngine.Buff> buffList = null)
        {
            this.minFedPercent = minFedPercent;
            this.hitPointsRegen = hitPointsRegen;
            this.hitPointsDecrease = hitPointsDecrease;
            this.fatigueRegen = fatigueRegen;
            this.canBeAttacked = canBeAttacked;

            Debug.Assert(this.hitPointsRegen >= 0);
            Debug.Assert(this.hitPointsDecrease >= 0);

            if (this.hitPointsRegen > 0) Debug.Assert(this.hitPointsDecrease == 0);
            if (this.hitPointsDecrease > 0) Debug.Assert(this.hitPointsRegen == 0);
        }

        public void Execute(Player player)
        {
            player.Fatigue -= this.fatigueRegen;
            if (player.FedPercent > this.minFedPercent) player.ExpendEnergy(energyAmount: 0.1f, addFatigue: false);
            if (this.hitPointsRegen > 0) player.hitPoints = Math.Min(player.hitPoints += this.hitPointsRegen, player.maxHitPoints);
            if (this.hitPointsDecrease > 0 && player.HitPointsPercent > 0.15f) player.hitPoints = Math.Max(player.hitPoints -= this.hitPointsDecrease, 0);
        }

    }
}
