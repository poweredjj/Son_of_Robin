using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType { InvWidth, InvHeight, ToolbarWidth, ToolbarHeight, Speed, Strength, HP, MaxHP, MaxStamina, EnableMap, Tired, Hungry, LightSource, RegenPoison, Haste, Fatigue };

        [Serializable]
        public class Buff
        {
            public int id;
            public readonly bool increaseIDAtEveryUse;
            public readonly BuffType type;
            public readonly bool isPositive;
            public readonly string description;
            public readonly string iconText;
            public readonly object value;
            public readonly bool canKill;
            public readonly int autoRemoveDelay;
            public int activationFrame;
            public int endFrame;
            public readonly bool isPermanent;
            public readonly int sleepFrames;
            public Buff(World world, BuffType type, object value, int autoRemoveDelay = 0, int sleepFrames = 0, bool isPermanent = false, bool canKill = false, bool increaseIDAtEveryUse = false)
            {
                this.increaseIDAtEveryUse = increaseIDAtEveryUse; // for buffs that could stack (like sleeping buffs)
                this.id = world.currentBuffId;
                world.currentBuffId++;
                this.type = type;
                this.value = value;
                this.canKill = canKill;
                this.isPermanent = isPermanent;
                this.sleepFrames = sleepFrames;
                this.isPositive = this.GetIsPositive();
                this.description = this.GetDescription();
                this.iconText = this.GetIconText();

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

            private bool GetIsPositive()
            {
                switch (this.type)
                {
                    case BuffType.InvWidth:
                        return (byte)this.value >= 0;

                    case BuffType.InvHeight:
                        return (byte)this.value >= 0;

                    case BuffType.ToolbarWidth:
                        return (byte)this.value >= 0;

                    case BuffType.ToolbarHeight:
                        return (byte)this.value >= 0;

                    case BuffType.Speed:
                        return (float)this.value >= 0;

                    case BuffType.Strength:
                        return (int)this.value >= 0;

                    case BuffType.HP:
                        return (float)this.value >= 0;

                    case BuffType.MaxHP:
                        return (float)this.value >= 0;

                    case BuffType.MaxStamina:
                        return (float)this.value >= 0;

                    case BuffType.LightSource:
                        return (int)this.value >= 0;

                    case BuffType.RegenPoison:
                        return (int)this.value >= 0;

                    case BuffType.Haste:
                        return (int)this.value >= 0;

                    case BuffType.Fatigue:
                        return (float)this.value <= 0; // reversed, because fatigue is a "negative" stat

                    case BuffType.EnableMap:
                        return true;

                    case BuffType.Tired:
                        return false;

                    case BuffType.Hungry:
                        return false;

                    default:
                        throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
                }
            }

            private string SignString { get { return $"{this.value}".StartsWith("-") ? "" : "+"; } }

            private string GetDescription()
            {
                string description;
                string sign = this.SignString;
                string duration = this.autoRemoveDelay == 0 ? "" : $" for {Math.Round(this.autoRemoveDelay / 60f)}s";

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
                        description = $"Speed {sign}{this.value}{duration}.";
                        break;

                    case BuffType.Strength:
                        description = $"Strength {sign}{this.value}{duration}.";
                        break;

                    case BuffType.HP:
                        description = $"Health {sign}{this.value}.";
                        break;

                    case BuffType.MaxHP:
                        description = $"Max health {sign}{this.value}{duration}.";
                        break;

                    case BuffType.MaxStamina:
                        description = $"Max stamina {sign}{this.value}{duration}.";
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
                        description = this.isPositive ? $"Regen {sign}{this.value}{duration}." : $"Poison {sign}{this.value}{duration}.";
                        break;

                    case BuffType.Haste:
                        description = $"Haste {sign}{this.value}{duration}.";
                        break;

                    case BuffType.Fatigue:
                        description = $"Fatigue {sign}{this.value}.";
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
                }

                if (this.sleepFrames > 0) description = $"After a long sleep: {Helpers.FirstCharToLowerCase(description)}";

                return description;
            }

            private string GetIconText()
            {
                string sign = this.SignString;

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

                    case BuffType.MaxHP:
                        return $"MAX HP\n{sign}{this.value}";

                    case BuffType.MaxStamina:
                        return $"STAMINA\n{sign}{this.value}";

                    case BuffType.Tired:
                        return "TIRED";

                    case BuffType.Hungry:
                        return "HUNGRY";

                    case BuffType.LightSource:
                        return $"LIGHT\n{sign}{this.value}";

                    case BuffType.RegenPoison:
                        return this.isPositive ? $"REGEN\n{sign}{this.value}" : $"POISON\n{sign}{this.value}";

                    case BuffType.Haste:
                        return $"HASTE\n{sign}{this.value}";

                    case BuffType.Fatigue:
                        return $"FATIGUE\n{sign}{this.value}";

                    default:
                        throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
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

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff added for '{this.piece.readableName}' - id {buff.id} type {buff.type} value {buff.value}.");

            if (buff.isPermanent) this.RemoveBuff(buff.id); // permanent buff should only change value and not be stored and displayed
        }

        public void RemoveEveryBuffOfType(BuffType buffType)
        {
            foreach (var buffId in buffDict.Keys.ToList())
            {
                if (this.buffDict[buffId].type == buffType) this.buffDict.Remove(buffId);
            }
        }

        public void RemoveBuff(int buffID, bool checkIfHasThisBuff = true)
        {
            if (!this.buffDict.ContainsKey(buffID))
            {
                if (checkIfHasThisBuff) throw new DivideByZeroException($"Buff not found during removal - id {buffID}.");
                else return;
            }

            Buff buffToRemove = this.buffDict[buffID];

            BuffType typeToCheck = buffDict[buffID].type;
            this.buffDict.Remove(buffID);
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
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;
                        if (add) player.InvWidth += (byte)buff.value;
                        else player.InvWidth -= (byte)buff.value;

                        return;
                    }

                case BuffType.InvHeight:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;
                        if (add) player.InvHeight += (byte)buff.value;
                        else player.InvHeight -= (byte)buff.value;

                        return;
                    }

                case BuffType.ToolbarWidth:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;
                        if (add) player.ToolbarWidth += (byte)buff.value;
                        else player.ToolbarWidth -= (byte)buff.value;

                        return;
                    }

                case BuffType.ToolbarHeight:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;
                        if (add) player.ToolbarHeight += (byte)buff.value;
                        else player.ToolbarHeight -= (byte)buff.value;

                        return;
                    }

                case BuffType.Speed:
                    {
                        if (add) this.piece.speed += (float)buff.value;
                        else this.piece.speed -= (float)buff.value;

                        return;
                    }

                case BuffType.Strength:
                    {
                        int value = (int)buff.value;
                        if (!add) value *= -1;
                        this.piece.strength += value;

                        return;
                    }

                case BuffType.HP:
                    {
                        if (add) this.piece.hitPoints += (float)buff.value;
                        else this.piece.hitPoints -= (float)buff.value;

                        this.piece.hitPoints = Math.Min(this.piece.hitPoints, this.piece.maxHitPoints);
                        this.piece.hitPoints = Math.Max(this.piece.hitPoints, 0);

                        return;
                    }

                case BuffType.MaxHP:
                    {
                        if (add) this.piece.maxHitPoints += (float)buff.value;
                        else
                        {
                            this.piece.maxHitPoints -= (float)buff.value;
                            this.piece.hitPoints = Math.Min(this.piece.hitPoints, this.piece.maxHitPoints);
                        }

                        return;
                    }

                case BuffType.MaxStamina:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;

                        if (add)
                        {
                            player.maxStamina += (float)buff.value;
                            player.stamina = player.maxStamina;
                        }
                        else
                        {
                            player.maxStamina -= (float)buff.value;
                            player.stamina = Math.Min(player.stamina, player.maxStamina);
                        }

                        return;
                    }

                case BuffType.EnableMap:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;

                        if (add) player.world.MapEnabled = true;
                        else
                        {
                            if (!stillHasThisBuff)
                            {
                                player.world.MapEnabled = false;
                            }
                        }

                        return;
                    }

                case BuffType.Tired:
                    {
                        // this buff exists only to show negative status icon
                        return;
                    }

                case BuffType.Hungry:
                    {
                        // this buff exists only to show negative status icon
                        return;
                    }

                case BuffType.LightSource:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

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

                        return;
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
                                this.piece.showStatBarsTillFrame = world.currentUpdate + buff.autoRemoveDelay;
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

                        return;
                    }

                case BuffType.Haste:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;

                        if (add)
                        {
                            if (hadThisBuffBefore) this.RemoveEveryBuffOfType(buff.type);
                            player.world.bulletTimeMultiplier = (int)buff.value;
                        }
                        else player.world.bulletTimeMultiplier = 1;

                        return;
                    }

                case BuffType.Fatigue:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return;

                        Player player = (Player)this.piece;

                        if (add) player.Fatigue = Math.Max(0, player.Fatigue + (float)buff.value);
                        else player.Fatigue = Math.Min(player.maxFatigue, player.Fatigue - (float)buff.value);

                        return;
                    }

                default:
                    throw new DivideByZeroException($"Unsupported buff type - {buff.type}.");
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
            int sleepFrames = Math.Max(buff1.sleepFrames, buff2.sleepFrames);
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

                case BuffType.LightSource:
                    value = (int)buff1.value + (int)buff2.value;
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

                default:
                    throw new DivideByZeroException($"Unsupported buff type - {buffType}.");
            }

            return new Buff(world: world, type: buffType, value: value, autoRemoveDelay: autoRemoveDelay, isPermanent: buff1.isPermanent, sleepFrames: sleepFrames, canKill: canKill, increaseIDAtEveryUse: increaseIDAtEveryUse);
        }

    }
}
