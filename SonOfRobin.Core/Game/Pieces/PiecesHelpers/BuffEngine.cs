﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType
        { InvWidth, InvHeight, ToolbarWidth, ToolbarHeight, Speed, Strength, HP, MaxHP, MaxStamina, EnableMap, NotUsedKeptForCompatibility1, RegenPoison, Haste, Fatigue, Sprint, SprintCooldown, LowHP, Tired, Hungry, Heat, HeatProtection, SwampProtection, Wet };

        private readonly Dictionary<string, Buff> buffDict;
        private readonly BoardPiece piece;
        public List<Buff> BuffList
        { get { return this.buffDict.Values.ToList(); } }
        public bool HasAnyBuff
        { get { return this.buffDict.Any(); } }

        public BuffEngine(BoardPiece piece)
        {
            this.piece = piece;
            this.buffDict = new Dictionary<string, Buff> { };
        }

        public BuffEngine(BoardPiece piece, Dictionary<string, Buff> buffDict)
        {
            this.piece = piece;
            this.buffDict = buffDict;
        }

        public Dictionary<string, Object> Serialize()
        {
            var buffDataDict = new Dictionary<string, Object>
            {
              { "buffDict", this.buffDict },
            };

            return buffDataDict;
        }

        public static BuffEngine Deserialize(BoardPiece piece, Object buffEngineData)
        {
            var buffDataDict = (Dictionary<string, Object>)buffEngineData;
            var buffDict = (Dictionary<string, Buff>)buffDataDict["buffDict"];

            return new BuffEngine(piece: piece, buffDict: buffDict);
        }

        public void AddBuffs(World world, List<Buff> buffList)
        {
            foreach (Buff buff in buffList)
            { this.AddBuff(world: world, buff: buff); }
        }

        public void RemoveBuffs(List<Buff> buffList)
        {
            foreach (Buff buff in buffList)
            { this.RemoveBuff(buff.id); }
        }

        public void AddBuff(Buff buff, World world)
        {
            if (buff.increaseIDAtEveryUse)
            {
                buff.id = Helpers.GetUniqueHash();
            }

            if (this.buffDict.ContainsKey(buff.id)) throw new ArgumentException($"Buff has been added twice - id {buff.id} type {buff.type}.");

            if (buff.sleepMinutesNeededForActivation > 0 && !buff.HadEnoughSleepForBuff(world)) return;

            bool hadThisBuffBefore = this.HasBuff(buff.type);
            bool buffHasBeenApplied = this.ProcessBuff(world: world, buff: buff, add: true, hadThisBuffBefore: hadThisBuffBefore);
            if (!buffHasBeenApplied)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff id {buff.id} type {buff.type} value {buff.value} could not be applied to '{this.piece.readableName}'.");
                return;
            }
            this.buffDict[buff.id] = buff;
            if (buff.autoRemoveDelay > 0) new WorldEvent(eventName: WorldEvent.EventName.RemoveBuff, world: this.piece.world, delay: buff.autoRemoveDelay, boardPiece: this.piece, eventHelper: buff.id);

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff added for '{this.piece.readableName}' - id {buff.id} type {buff.type} value {buff.value}.");

            if (buff.isPermanent) this.RemoveBuff(buff.id); // permanent buff should only change value and not be stored and displayed
        }

        public void RemoveEveryBuffOfType(BuffType buffType)
        {
            foreach (var buffId in buffDict.Keys.ToList())
            {
                if (this.buffDict[buffId].type == buffType) this.RemoveBuff(buffId);
            }
        }

        public void RemoveBuff(string buffID, bool checkIfHasThisBuff = true)
        {
            if (!this.buffDict.ContainsKey(buffID))
            {
                if (checkIfHasThisBuff) throw new ArgumentException($"Buff not found during removal - id {buffID}.");
                else return;
            }

            Buff buffToRemove = this.buffDict[buffID];

            BuffType typeToCheck = buffDict[buffID].type;
            this.buffDict.Remove(buffID);
            bool stillHasThisBuff = this.HasBuff(typeToCheck);
            if (!buffToRemove.isPermanent) this.ProcessBuff(buff: buffToRemove, add: false, stillHasThisBuff: stillHasThisBuff, world: null);

            // MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff removed - id {buffToRemove.id} type {buffToRemove.type} value {buffToRemove.value}.");
        }

        private List<Buff> FindBuffsOfType(BuffType buffType)
        {
            return this.buffDict.Values.Where(buff => buff.type == buffType).ToList();
        }

        public bool HasBuff(BuffType buffType, bool isPositive)
        {
            foreach (Buff buff in this.buffDict.Values)
            {
                if (buff.type == buffType && buff.isPositive == isPositive) return true;
            }

            return false;
        }

        public bool HasBuff(BuffType buffType)
        {
            foreach (Buff buff in this.buffDict.Values)
            {
                if (buff.type == buffType) return true;
            }

            return false;
        }

        public bool HasBuff(string buffID)
        {
            foreach (Buff buff in this.buffDict.Values)
            {
                if (buff.id == buffID) return true;
            }

            return false;
        }

        private bool ProcessBuff(World world, Buff buff, bool add, bool hadThisBuffBefore = false, bool stillHasThisBuff = false)
        {
            if (add)
            {
                buff.activationFrame = world.CurrentUpdate;
                buff.endFrame = world.CurrentUpdate + buff.autoRemoveDelay;
            }

            switch (buff.type)
            {
                case BuffType.InvWidth:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;
                        if (add) player.InvWidth += (byte)buff.value;
                        else player.InvWidth -= (byte)buff.value;

                        return true;
                    }

                case BuffType.InvHeight:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;
                        if (add) player.InvHeight += (byte)buff.value;
                        else player.InvHeight -= (byte)buff.value;

                        return true;
                    }

                case BuffType.ToolbarWidth:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;
                        if (add) player.ToolbarWidth += (byte)buff.value;
                        else player.ToolbarWidth -= (byte)buff.value;

                        return true;
                    }

                case BuffType.ToolbarHeight:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;
                        if (add) player.ToolbarHeight += (byte)buff.value;
                        else player.ToolbarHeight -= (byte)buff.value;

                        return true;
                    }

                case BuffType.Speed:
                    {
                        if (add)
                        {
                            float newSpeed = this.piece.speed + (float)buff.value;
                            if (newSpeed <= 0) return false;

                            this.piece.speed = newSpeed;
                        }
                        else this.piece.speed -= (float)buff.value;

                        return true;
                    }

                case BuffType.Strength:
                    {
                        if (add)
                        {
                            int newStrength = this.piece.strength + (int)buff.value;
                            if (newStrength <= 0) return false;

                            this.piece.strength = newStrength;
                        }
                        else this.piece.strength -= (int)buff.value;

                        return true;
                    }

                case BuffType.HP:
                    {
                        if (add) this.piece.HitPoints += (float)buff.value;
                        else this.piece.HitPoints -= (float)buff.value;

                        return true;
                    }

                case BuffType.MaxHP:
                    {
                        if (add)
                        {
                            float newMaxHP = this.piece.maxHitPoints + (float)buff.value;
                            if (newMaxHP < 3) return false;

                            this.piece.maxHitPoints = newMaxHP;
                        }
                        else this.piece.maxHitPoints -= (float)buff.value;

                        this.piece.HitPoints = Math.Min(this.piece.HitPoints, this.piece.maxHitPoints);

                        return true;
                    }

                case BuffType.MaxStamina:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;

                        float valueChange = add ? (float)buff.value : -(float)buff.value;

                        if (add)
                        {
                            float newMaxStamina = player.maxStamina + valueChange;
                            if (newMaxStamina <= 10) return false;

                            player.maxStamina = newMaxStamina;
                        }
                        else player.maxStamina += valueChange;

                        if (valueChange > 0) player.stamina = player.maxStamina;
                        else player.stamina = Math.Min(player.stamina, player.maxStamina);

                        return true;
                    }

                case BuffType.EnableMap:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;

                        if (add) player.world.MapEnabled = true;
                        else
                        {
                            if (!stillHasThisBuff) player.world.MapEnabled = false;
                        }

                        return true;
                    }

                case BuffType.Tired:
                    {
                        // this buff exists only to show negative status icon
                        return true;
                    }

                case BuffType.Hungry:
                    {
                        // this buff exists only to show negative status icon
                        return true;
                    }

                case BuffType.Heat:
                    {
                        if (this.HasBuff(BuffType.Heat) || this.HasBuff(BuffType.HeatProtection) || this.HasBuff(BuffType.Wet)) return false;
                        return true;
                    }

                case BuffType.HeatProtection:
                    {
                        this.RemoveEveryBuffOfType(BuffType.Heat);
                        return true;
                    }

                case BuffType.SwampProtection:
                    {
                        // this buff exists only to show status icon and prevent from getting swamp poison
                        return true;
                    }

                case BuffType.Wet:
                    {
                        if (hadThisBuffBefore) this.RemoveEveryBuffOfType(BuffType.Wet); // to refresh buff duration
                        this.RemoveEveryBuffOfType(BuffType.Heat);
                        return true;
                    }

                case BuffType.LowHP:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;

                        if (add) player.soundPack.Play(PieceSoundPack.Action.PlayerPulse);
                        else player.soundPack.Stop(PieceSoundPack.Action.PlayerPulse);

                        return true;
                    }

                case BuffType.RegenPoison:
                    {
                        if (add)
                        {
                            if (hadThisBuffBefore) this.RemoveEveryBuffOfType(buff.type);

                            int delay = 60 * 5;

                            var regenPoisonData = new Dictionary<string, Object> {
                            { "buffID", buff.id }, { "charges", buff.autoRemoveDelay / delay }, { "delay", delay }, { "hpChange", buff.value }, { "canKill", buff.canKill } };
                            new WorldEvent(eventName: WorldEvent.EventName.RegenPoison, world: world, delay: delay, boardPiece: this.piece, eventHelper: regenPoisonData);

                            if ((int)buff.value < 0)
                            {
                                this.piece.sprite.color = Color.Chartreuse;
                                this.piece.showStatBarsTillFrame = world.CurrentUpdate + buff.autoRemoveDelay;
                            }
                        }
                        else
                        {
                            bool hasPoisonBuff = false;
                            foreach (Buff regenPoisonBuff in this.FindBuffsOfType(buff.type))
                            {
                                if ((int)regenPoisonBuff.value < 0)
                                {
                                    hasPoisonBuff = true;
                                    break;
                                }
                            }

                            if (!hasPoisonBuff) this.piece.sprite.color = Color.White;
                        }

                        return true;
                    }

                case BuffType.Haste:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;
                        Player player = (Player)this.piece;

                        if (add)
                        {
                            if (hadThisBuffBefore) this.RemoveEveryBuffOfType(buff.type);
                            player.world.stateMachineTypesManager.EnableMultiplier((int)buff.value);
                            player.world.stateMachineTypesManager.EnableAllTypes(nthFrame: true);
                            player.world.stateMachineTypesManager.RemoveTheseTypes(typesToRemove: new List<Type> { typeof(Animal) }, everyFrame: true);
                        }
                        else
                        {
                            player.world.stateMachineTypesManager.DisableMultiplier();
                            player.world.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
                        }

                        return true;
                    }

                case BuffType.Fatigue:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

                        Player player = (Player)this.piece;

                        if (add) player.Fatigue = Math.Max(0, player.Fatigue + (float)buff.value);
                        else player.Fatigue = Math.Min(player.maxFatigue, player.Fatigue - (float)buff.value);

                        return true;
                    }

                case BuffType.Sprint:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;
                        Player player = (Player)this.piece;

                        if (add)
                        {
                            if (hadThisBuffBefore) this.RemoveEveryBuffOfType(buff.type);
                            player.speed += (float)buff.value;
                            player.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Cyan, textureSize: player.sprite.animFrame.textureSize, priority: 0, framesLeft: -1));
                            player.soundPack.Play(PieceSoundPack.Action.PlayerSprint);
                        }
                        else
                        {
                            player.speed -= (float)buff.value;
                            player.sprite.effectCol.RemoveEffectsOfType(effect: SonOfRobinGame.EffectBorder);
                            player.soundPack.Play(PieceSoundPack.Action.PlayerPant);
                            player.buffEngine.AddBuff(buff: new Buff(type: BuffType.SprintCooldown, autoRemoveDelay: 15 * 60, value: null), world: player.world);
                        }

                        return true;
                    }

                case BuffType.SprintCooldown:
                    {
                        // this buff exists only to show negative status and prevent sprint
                        return true;
                    }

                default:
                    throw new ArgumentException($"Unsupported buff type - {buff.type}.");
            }
        }

        private bool CheckIfPieceIsPlayer(Buff buff)
        {
            if (this.piece.GetType() != typeof(Player))
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff '{buff.type}' cannot target {this.piece.GetType()} - ignoring.");
                return false;
            }

            return true;
        }

        public static bool BuffListContainsPoison(List<Buff> buffList)
        {
            int regenValue = 0;

            foreach (Buff buff in buffList)
            {
                if (buff.type == BuffType.RegenPoison) regenValue += (int)buff.value;
            }

            return regenValue < 0;
        }

        public static bool BuffListContainsPoisonOrRegen(List<Buff> buffList)
        {
            int regenValue = 0;

            foreach (Buff buff in buffList)
            {
                if (buff.type == BuffType.RegenPoison) regenValue += (int)buff.value;
            }

            return regenValue != 0;
        }

        public static List<Buff> MergeSameTypeBuffsInList(World world, List<Buff> buffList)
        {
            var buffsListsByType = new Dictionary<BuffType, List<Buff>>();

            foreach (Buff buff in buffList)
            {
                if (buffsListsByType.ContainsKey(buff.type)) buffsListsByType[buff.type].Add(buff);
                else buffsListsByType[buff.type] = new List<Buff> { buff };
            }

            var mergedBuffList = new List<Buff>();

            foreach (List<Buff> sameTypeBuffList in buffsListsByType.Values)
            {
                if (sameTypeBuffList.Count == 1) mergedBuffList.Add(sameTypeBuffList[0]);
                else
                {
                    Buff newBuff = null;
                    foreach (Buff buff in sameTypeBuffList)
                    {
                        newBuff = MergeTwoSameTypeBuffs(world: world, buff1: newBuff, buff2: buff);
                    }
                    mergedBuffList.Add(newBuff);
                }
            }

            return mergedBuffList;
        }

        public static Buff MergeTwoSameTypeBuffs(World world, Buff buff1, Buff buff2)
        {
            if (buff1 == null && buff2 != null) return buff2;
            if (buff2 == null && buff1 != null) return buff1;

            if (buff1.type != buff2.type) throw new ArgumentException($"Buffs' types ({buff1.type}, {buff2.type}) are not the same.");
            if (buff1.isPermanent != buff2.isPermanent) throw new ArgumentException($"Buffs' 'isPermanent' ({buff1.type}, {buff2.type}) are not the same.");

            BuffType buffType = buff1.type;

            object value;
            int autoRemoveDelay = Math.Max(buff1.autoRemoveDelay, buff2.autoRemoveDelay);
            int sleepMinutesNeededForActivation = Math.Max(buff1.sleepMinutesNeededForActivation, buff2.sleepMinutesNeededForActivation);
            bool canKill = buff1.canKill || buff2.canKill;
            bool increaseIDAtEveryUse = buff1.increaseIDAtEveryUse || buff2.increaseIDAtEveryUse;

            switch (buffType)
            {
                case BuffType.InvWidth:
                    value = (byte)buff1.value + (byte)buff2.value;
                    break;

                case BuffType.InvHeight:
                    value = (byte)buff1.value + (byte)buff2.value;
                    break;

                case BuffType.ToolbarWidth:
                    value = (byte)buff1.value + (byte)buff2.value;
                    break;

                case BuffType.ToolbarHeight:
                    value = (byte)buff1.value + (byte)buff2.value;
                    break;

                case BuffType.Speed:
                    value = (float)buff1.value + (float)buff2.value;
                    break;

                case BuffType.Strength:
                    value = (int)buff1.value + (int)buff2.value;
                    break;

                case BuffType.HP:
                    value = (float)buff1.value + (float)buff2.value;
                    break;

                case BuffType.MaxHP:
                    value = (float)buff1.value + (float)buff2.value;
                    break;

                case BuffType.MaxStamina:
                    value = (float)buff1.value + (float)buff2.value;
                    break;

                case BuffType.RegenPoison:
                    value = (int)buff1.value + (int)buff2.value;
                    break;

                case BuffType.Haste:
                    value = (int)buff1.value + (int)buff2.value;
                    break;

                case BuffType.Fatigue:
                    value = (float)buff1.value + (float)buff2.value;
                    break;

                case BuffType.EnableMap:
                    value = null;
                    break;

                case BuffType.Tired:
                    value = null;
                    break;

                case BuffType.Hungry:
                    value = null;
                    break;

                case BuffType.Heat:
                    value = null;
                    break;

                case BuffType.HeatProtection:
                    value = null;
                    break;

                case BuffType.SwampProtection:
                    value = null;
                    break;

                case BuffType.Wet:
                    value = null;
                    break;

                case BuffType.Sprint:
                    value = (float)buff1.value + (float)buff2.value;
                    break;

                case BuffType.SprintCooldown:
                    value = null;
                    break;

                default:
                    throw new ArgumentException($"Unsupported buff type - {buffType}.");
            }

            return new Buff(type: buffType, value: value, autoRemoveDelay: autoRemoveDelay, isPermanent: buff1.isPermanent, sleepMinutesNeededForActivation: sleepMinutesNeededForActivation, canKill: canKill, increaseIDAtEveryUse: increaseIDAtEveryUse);
        }
    }
}