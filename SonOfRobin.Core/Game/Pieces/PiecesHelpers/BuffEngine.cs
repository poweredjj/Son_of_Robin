using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType { InvWidth, InvHeight, ToolbarWidth, ToolbarHeight, Speed, Strength, HP, MaxHp, EnableMap, Tired, Hungry, LightSource, RegenPoison };

        [Serializable]
        public class Buff
        {
            public int id;
            public readonly bool increaseIDAtEveryUse;
            public readonly BuffType type;
            public readonly object value;
            public readonly bool canKill;
            public readonly int autoRemoveDelay;
            public int activationFrame;
            public int endFrame;
            public readonly bool isPositive;
            public readonly bool isPermanent;
            public readonly int sleepFrames;
            public Buff(World world, BuffType type, object value, bool isPositive, int autoRemoveDelay = 0, int sleepFrames = 0, bool isPermanent = false, bool canKill = false, bool increaseIDAtEveryUse = false)
            {
                this.increaseIDAtEveryUse = increaseIDAtEveryUse; // for buffs that could stack (like sleeping buffs)
                this.id = world.currentBuffId;
                world.currentBuffId++;
                this.type = type;
                this.value = value;
                this.canKill = canKill;
                this.isPositive = isPositive;
                this.isPermanent = isPermanent;
                this.sleepFrames = sleepFrames;

                // AutoRemoveDelay should not be used for equip!
                // It should only be used for temporary buffs (food, status effects, etc.).
                this.autoRemoveDelay = autoRemoveDelay;
                this.endFrame = 0; // to be assigned during activation
                this.activationFrame = 0; // to be assigned during activation
            }

            public bool HadEnoughSleepForBuff(World world)
            {
                int framesSlept = world.currentUpdate - world.player.wentToSleepFrame;
                return framesSlept >= this.sleepFrames;
            }

            public string Description
            {
                get
                {
                    string description;
                    string sign = $"{this.value}".StartsWith("-") ? "" : "+";

                    switch (this.type)
                    {
                        case BuffType.InvWidth:
                            description = $"Inventory width {sign}{this.value}.";
                            break;

                        case BuffType.InvHeight:
                            description = $"Inventory height {sign}{this.value}.";
                            break;

                        case BuffType.ToolbarWidth:
                            description = $"Toolbar width {sign}{this.value}.";
                            break;

                        case BuffType.ToolbarHeight:
                            description = $"Toolbar height {sign}{this.value}.";
                            break;

                        case BuffType.Speed:
                            description = $"Speed {sign}{this.value}.";
                            break;

                        case BuffType.Strength:
                            description = $"Strength {sign}{this.value}.";
                            break;

                        case BuffType.HP:
                            description = $"HP {sign}{this.value}.";
                            break;

                        case BuffType.MaxHp:
                            description = $"Max HP {sign}{this.value}.";
                            break;

                        case BuffType.EnableMap:
                            description = "Shows map of visited places.";
                            break;

                        case BuffType.Tired:
                            description = "Decreases multiple stats.";
                            break;

                        case BuffType.Hungry:
                            description = "Hungry";
                            break;

                        case BuffType.LightSource:
                            description = $"Light source {sign}{this.value}.";
                            break;

                        case BuffType.RegenPoison:
                            description = this.isPositive ? $"Regen {sign}{this.value}" : $"Poison {sign}{this.value}";
                            break;

                        default:
                            throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
                    }

                    if (this.sleepFrames > 0) description = $"After a long sleep: {Helpers.FirstCharToLowerCase(description)}";

                    return description;
                }
            }

            public string IconText
            {
                get
                {

                    string sign = $"{this.value}".StartsWith("-") ? "" : "+";

                    switch (this.type)
                    {
                        case BuffType.InvWidth:
                            return null;

                        case BuffType.InvHeight:
                            return null;

                        case BuffType.ToolbarWidth:
                            return null;

                        case BuffType.ToolbarHeight:
                            return null;

                        case BuffType.EnableMap:
                            return null;

                        case BuffType.Speed:
                            return $"SPD\n{sign}{this.value}";

                        case BuffType.Strength:
                            return $"STR\n{sign}{this.value}";

                        case BuffType.HP:
                            return $"HP\n{sign}{this.value}";

                        case BuffType.MaxHp:
                            return $"MAX HP\n{sign}{this.value}";

                        case BuffType.Tired:
                            return "tired";

                        case BuffType.Hungry:
                            return "hungry";

                        case BuffType.LightSource:
                            return $"LIGHT\n{sign}{this.value}";

                        case BuffType.RegenPoison:
                            return this.isPositive ? $"REGEN\n{sign}{this.value}" : $"POISON\n{sign}{this.value}";

                        default:
                            throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
                    }
                }
            }

        }

        public readonly Dictionary<int, Buff> buffDict;
        private readonly BoardPiece piece;

        public BuffEngine(BoardPiece piece)
        {
            this.piece = piece;
            this.buffDict = new Dictionary<int, Buff> { };
        }

        public BuffEngine(BoardPiece piece, Dictionary<int, Buff> buffDict)
        {
            this.piece = piece;
            this.buffDict = buffDict;
        }

        public Dictionary<string, Object> Serialize()
        {
            var buffDataDict = new Dictionary<string, Object>
            {
              {"buffDict", this.buffDict},
            };

            return buffDataDict;
        }

        public static BuffEngine Deserialize(BoardPiece piece, Object buffEngineData)
        {
            var buffDataDict = (Dictionary<string, Object>)buffEngineData;
            var buffDict = (Dictionary<int, Buff>)buffDataDict["buffDict"];

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
                buff.id = world.currentBuffId;
                world.currentBuffId++;
            }

            if (this.buffDict.ContainsKey(buff.id)) throw new DivideByZeroException($"Buff has been added twice - id {buff.id} type {buff.type}.");

            if (buff.sleepFrames > 0 && !buff.HadEnoughSleepForBuff(world)) return;

            bool hadThisBuffBefore = this.HasBuff(buff.type);
            this.ProcessBuff(world: world, buff: buff, add: true, hadThisBuffBefore: hadThisBuffBefore);
            this.buffDict[buff.id] = buff;
            if (buff.autoRemoveDelay > 0) new WorldEvent(eventName: WorldEvent.EventName.RemoveBuff, world: this.piece.world, delay: buff.autoRemoveDelay, boardPiece: this.piece, eventHelper: buff.id);

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff added - id {buff.id} type {buff.type} value {buff.value}.");

            if (buff.isPermanent) this.RemoveBuff(buff.id); // permanent buff should only change value and not be stored and displayed
        }

        public void RemoveEveryBuffOfType(BuffType buffType)
        {
            foreach (var buffId in buffDict.Keys.ToList())
            {
                if (this.buffDict[buffId].type == buffType) this.buffDict.Remove(buffId);
            }
        }

        public void RemoveBuff(int buffId, bool checkIfHasThisBuff = true)
        {
            if (!this.buffDict.ContainsKey(buffId))
            {
                if (checkIfHasThisBuff) throw new DivideByZeroException($"Buff not found during removal - id {buffId}.");
                else return;
            }

            Buff buffToRemove = this.buffDict[buffId];

            BuffType typeToCheck = buffDict[buffId].type;
            this.buffDict.Remove(buffId);
            bool stillHasThisBuff = this.HasBuff(typeToCheck);
            if (!buffToRemove.isPermanent) this.ProcessBuff(buff: buffToRemove, add: false, stillHasThisBuff: stillHasThisBuff, world: null);

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff removed - id {buffToRemove.id} type {buffToRemove.type} value {buffToRemove.value}.");
        }

        private List<Buff> FindBuffsOfType(BuffType buffType)
        {
            return this.buffDict.Values.Where(buff => buff.type == buffType).ToList();
        }

        public bool HasBuff(BuffType buffType)
        {
            foreach (Buff buff in this.buffDict.Values)
            {
                if (buff.type == buffType) return true;
            }

            return false;
        }

        public bool HasBuff(int buffID)
        {
            foreach (Buff buff in this.buffDict.Values)
            {
                if (buff.id == buffID) return true;
            }

            return false;
        }

        private void ProcessBuff(World world, Buff buff, bool add, bool hadThisBuffBefore = false, bool stillHasThisBuff = false)
        {
            if (add)
            {
                buff.activationFrame = world.currentUpdate;
                buff.endFrame = world.currentUpdate + buff.autoRemoveDelay;
            }

            switch (buff.type)
            {
                case BuffType.InvWidth:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.InvWidth += (byte)buff.value;
                        else player.InvWidth -= (byte)buff.value;

                        break;
                    }

                case BuffType.InvHeight:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.InvHeight += (byte)buff.value;
                        else player.InvHeight -= (byte)buff.value;

                        break;
                    }

                case BuffType.ToolbarWidth:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.ToolbarWidth += (byte)buff.value;
                        else player.ToolbarWidth -= (byte)buff.value;

                        break;
                    }

                case BuffType.ToolbarHeight:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.ToolbarHeight += (byte)buff.value;
                        else player.ToolbarHeight -= (byte)buff.value;

                        break;
                    }

                case BuffType.Speed:
                    {
                        if (add) this.piece.speed += (float)buff.value;
                        else this.piece.speed -= (float)buff.value;

                        break;
                    }

                case BuffType.Strength:
                    {
                        int value = (int)buff.value;
                        if (!add) value *= -1;
                        this.piece.strength += value;

                        break;
                    }

                case BuffType.MaxHp:
                    {
                        if (add) this.piece.maxHitPoints += (float)buff.value;
                        else
                        {
                            this.piece.maxHitPoints -= (float)buff.value;
                            this.piece.hitPoints = Math.Min(this.piece.hitPoints, this.piece.maxHitPoints);
                        }

                        break;
                    }

                case BuffType.HP:
                    {
                        if (add) this.piece.hitPoints += (float)buff.value;
                        else this.piece.hitPoints -= (float)buff.value;

                        this.piece.hitPoints = Math.Min(this.piece.hitPoints, this.piece.maxHitPoints);
                        this.piece.hitPoints = Math.Max(this.piece.hitPoints, 0);

                        break;
                    }

                case BuffType.EnableMap:
                    {
                        Player player = (Player)this.piece;

                        if (add) player.world.MapEnabled = true;
                        else
                        {
                            if (!stillHasThisBuff)
                            {
                                player.world.MapEnabled = false;
                            }
                        }

                        break;
                    }

                case BuffType.Tired:
                    {
                        // this buff exists only to show negative status icon
                        break;
                    }

                case BuffType.Hungry:
                    {
                        // this buff exists only to show negative status icon
                        break;
                    }

                case BuffType.LightSource:
                    {
                        Player player = (Player)this.piece;
                        LightEngine playerLightSource = player.sprite.lightEngine;

                        if (add)
                        {
                            playerLightSource.Size = (int)buff.value * 100;
                            playerLightSource.Activate();
                        }
                        else
                        {
                            if (!stillHasThisBuff) playerLightSource.Deactivate();
                        }

                        break;
                    }

                case BuffType.RegenPoison:
                    {
                        if (add)
                        {
                            int delay = 60 * 5;

                            var regenPoisonData = new Dictionary<string, Object> {
                            { "buffID", buff.id }, { "charges", buff.autoRemoveDelay / delay }, { "delay", delay }, { "hpChange", buff.value }, { "canKill", buff.canKill } };
                            new WorldEvent(eventName: WorldEvent.EventName.RegenPoison, world: world, delay: delay, boardPiece: this.piece, eventHelper: regenPoisonData);

                            if ((int)buff.value < 0) this.piece.sprite.color = Color.Chartreuse;
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

                        break;
                    }

                default:
                    throw new DivideByZeroException($"Unsupported buff type - {buff.type}.");
            }
        }

    }
}
