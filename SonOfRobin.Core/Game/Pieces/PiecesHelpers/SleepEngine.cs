using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    [Serializable]
    public struct SleepEngine
    {
        public static readonly SleepEngine OutdoorSleepDry = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.4f, hitPointsChange: -0.1f, minFatiguePercentPossibleToGet: 0.5f, updateMultiplier: 5, waitingAfterSleepPossible: false, canBeAttacked: true, wakeUpBuffs: new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -100f, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)});

        public static readonly SleepEngine OutdoorSleepWet = new SleepEngine(minFedPercent: 0.1f, fatigueRegen: 0.2f, hitPointsChange: -0.2f, minFatiguePercentPossibleToGet: 0.6f, updateMultiplier: 5, waitingAfterSleepPossible: false, canBeAttacked: true, wakeUpBuffs: new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.Strength, value: -1, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -150f, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)});

        private readonly float minFedPercent;
        private readonly float fatigueRegen;
        private readonly float hitPointsChange;
        public readonly float minFatiguePercentPossibleToGet; // 0 - 1
        public readonly bool canBeAttacked;
        public readonly int updateMultiplier;
        public readonly bool waitingAfterSleepPossible;
        public readonly List<Buff> wakeUpBuffs;

        public SleepEngine(float minFedPercent, float fatigueRegen, int updateMultiplier, bool canBeAttacked, bool waitingAfterSleepPossible, float minFatiguePercentPossibleToGet, float hitPointsChange = 0f, List<Buff> wakeUpBuffs = null)
        {
            this.minFedPercent = minFedPercent;
            this.minFatiguePercentPossibleToGet = minFatiguePercentPossibleToGet;
            this.hitPointsChange = hitPointsChange;
            this.fatigueRegen = fatigueRegen;
            this.canBeAttacked = canBeAttacked;
            this.updateMultiplier = updateMultiplier;
            this.waitingAfterSleepPossible = waitingAfterSleepPossible;
            this.wakeUpBuffs = wakeUpBuffs == null ? new List<Buff>() : wakeUpBuffs;
        }

        public void Execute(Player player)
        {
            player.Fatigue -= this.fatigueRegen;
            if (player.FedPercent > this.minFedPercent) player.ExpendEnergy(energyAmount: 0.1f, addFatigue: false);

            player.HitPoints += this.hitPointsChange;
            if (this.hitPointsChange < 0) player.HitPoints = Math.Max(player.HitPoints, player.maxHitPoints * 0.15f);
        }
    }
}