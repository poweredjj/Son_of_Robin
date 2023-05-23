using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    [Serializable]
    public struct SleepEngine
    {
        public static SleepEngine OutdoorSleepDry = new SleepEngine(minFedPercent: 0.2f, fatigueRegen: 0.4f, hitPointsChange: -0.1f, islandClockMultiplier: 2, waitingAfterSleepPossible: false, canBeAttacked: true, wakeUpBuffs: new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -100f, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.MaxStamina, value: -100f, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true)});

        public static SleepEngine OutdoorSleepWet = new SleepEngine(minFedPercent: 0.1f, fatigueRegen: 0.2f, hitPointsChange: -0.2f, islandClockMultiplier: 2, waitingAfterSleepPossible: false, canBeAttacked: true, wakeUpBuffs: new List<Buff> {
                            new Buff(type: BuffEngine.BuffType.Strength, value: -1, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.MaxHP, value: -150f, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true),
                            new Buff(type: BuffEngine.BuffType.MaxStamina, value: -150f, sleepMinutesNeededForActivation: 60, autoRemoveDelay: 5 * 60 * 60, increaseIDAtEveryUse: true) });

        private readonly float minFedPercent;
        private readonly float fatigueRegen;
        private readonly float hitPointsChange;
        public readonly bool canBeAttacked;
        public readonly int islandClockMultiplier;
        public readonly bool waitingAfterSleepPossible;
        public readonly List<Buff> wakeUpBuffs;

        public SleepEngine(float minFedPercent, float fatigueRegen, int islandClockMultiplier, bool canBeAttacked, bool waitingAfterSleepPossible, float hitPointsChange = 0f, List<Buff> wakeUpBuffs = null)
        {
            this.minFedPercent = minFedPercent;
            this.hitPointsChange = hitPointsChange;
            this.fatigueRegen = fatigueRegen;
            this.canBeAttacked = canBeAttacked;
            this.islandClockMultiplier = islandClockMultiplier;
            this.waitingAfterSleepPossible = waitingAfterSleepPossible;
            this.wakeUpBuffs = wakeUpBuffs == null ? new List<Buff>() : wakeUpBuffs;
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