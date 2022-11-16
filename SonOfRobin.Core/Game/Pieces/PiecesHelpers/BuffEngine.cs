using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType { InvWidth, InvHeight, ToolbarWidth, ToolbarHeight, Speed, Strength, HP, MaxHP, MaxStamina, EnableMap, LightSource, RegenPoison, Haste, Fatigue, Sprint, SprintCooldown, LowHP, Tired, Hungry, Heat, HeatProtection, SwampProtection, Wet };

        [Serializable]
        public class Buff
        {
            public string id;
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
            public readonly int sleepFramesNeededForActivation;
            public Buff(BuffType type, object value, int autoRemoveDelay = 0, int sleepFramesNeededForActivation = 0, bool isPermanent = false, bool canKill = false, bool increaseIDAtEveryUse = false)
            {
                this.increaseIDAtEveryUse = increaseIDAtEveryUse; // for buffs that could stack (like sleeping buffs)
                this.id = Helpers.GetUniqueHash();
                // AutoRemoveDelay should not be used for equip!
                // It should only be used for temporary buffs (food, status effects, etc.).
                this.autoRemoveDelay = autoRemoveDelay;
                this.type = type;
                this.value = value;
                this.canKill = canKill;
                this.isPermanent = isPermanent;
                this.sleepFramesNeededForActivation = sleepFramesNeededForActivation;
                this.isPositive = this.GetIsPositive();
                this.description = this.GetDescription();
                this.iconText = this.GetIconText();

                this.endFrame = 0; // to be assigned during activation
                this.activationFrame = 0; // to be assigned during activation
            }

            public bool HadEnoughSleepForBuff(World world)
            {
                int framesSlept = world.CurrentUpdate - world.player.wentToSleepFrame;
                return framesSlept >= this.sleepFramesNeededForActivation;
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

                    case BuffType.LowHP:
                        return false;

                    case BuffType.Hungry:
                        return false;

                    case BuffType.Heat:
                        return false;

                    case BuffType.HeatProtection:
                        return true;

                    case BuffType.SwampProtection:
                        return true;

                    case BuffType.Wet:
                        return true;

                    case BuffType.Sprint:
                        return true;

                    case BuffType.SprintCooldown:
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

                    case BuffType.LowHP:
                        description = "Near death.";
                        break;

                    case BuffType.Hungry:
                        description = "Hungry.";
                        break;

                    case BuffType.Heat:
                        description = "Heat.";
                        break;

                    case BuffType.HeatProtection:
                        description = "Heat protection.";
                        break;

                    case BuffType.SwampProtection:
                        description = "Swamp poison protection.";
                        break;

                    case BuffType.Wet:
                        description = "Wet.";
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

                    case BuffType.Sprint:
                        description = $"Sprint {sign}{this.value}{duration}.";
                        break;

                    case BuffType.SprintCooldown:
                        description = $"Cannot sprint for {duration}.";
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
                }

                if (this.sleepFramesNeededForActivation > 0) description = $"After a long sleep: {Helpers.FirstCharToLowerCase(description)}";

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

                    case BuffType.LowHP:
                        return "LOW\nHEALTH";

                    case BuffType.Hungry:
                        return "HUNGRY";

                    case BuffType.Heat:
                        return "HEAT";

                    case BuffType.HeatProtection:
                        return "HEAT\nPROTECT";

                    case BuffType.SwampProtection:
                        return "SWAMP\nPROTECT";

                    case BuffType.Wet:
                        return "WET";

                    case BuffType.LightSource:
                        return $"LIGHT\n{sign}{this.value}";

                    case BuffType.RegenPoison:
                        return this.isPositive ? $"REGEN\n{sign}{this.value}" : $"POISON\n{sign}{this.value}";

                    case BuffType.Haste:
                        return $"HASTE\n{sign}{this.value}";

                    case BuffType.Fatigue:
                        return $"FATIGUE\n{sign}{this.value}";

                    case BuffType.Sprint:
                        return $"SPRINT\n{sign}{this.value}";

                    case BuffType.SprintCooldown:
                        return "CANNOT\nSPRINT";

                    default:
                        throw new DivideByZeroException($"Unsupported buff type - {this.type}.");
                }
            }
        }

        public readonly Dictionary<string, Buff> buffDict;
        private readonly BoardPiece piece;

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

            if (this.buffDict.ContainsKey(buff.id)) throw new DivideByZeroException($"Buff has been added twice - id {buff.id} type {buff.type}.");

            if (buff.sleepFramesNeededForActivation > 0 && !buff.HadEnoughSleepForBuff(world)) return;

            bool hadThisBuffBefore = this.HasBuff(buff.type);
            bool buffHasBeenApplied = this.ProcessBuff(world: world, buff: buff, add: true, hadThisBuffBefore: hadThisBuffBefore);
            if (!buffHasBeenApplied)
            {
                MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff id {buff.id} type {buff.type} value {buff.value} could not be applied to '{this.piece.readableName}'.");
                return;
            }
            this.buffDict[buff.id] = buff;
            if (buff.autoRemoveDelay > 0) new WorldEvent(eventName: WorldEvent.EventName.RemoveBuff, world: this.piece.world, delay: buff.autoRemoveDelay, boardPiece: this.piece, eventHelper: buff.id);

            MessageLog.AddMessage(msgType: MsgType.Debug, message: $"Buff added for '{this.piece.readableName}' - id {buff.id} type {buff.type} value {buff.value}.");

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
                        if (add) this.piece.hitPoints += (float)buff.value;
                        else this.piece.hitPoints -= (float)buff.value;

                        this.piece.hitPoints = Math.Min(this.piece.hitPoints, this.piece.maxHitPoints);
                        this.piece.hitPoints = Math.Max(this.piece.hitPoints, 0);

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

                        this.piece.hitPoints = Math.Min(this.piece.hitPoints, this.piece.maxHitPoints);

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

                case BuffType.LightSource:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;

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
                        else player.Fatigue = Math.Min(player.MaxFatigue, player.Fatigue - (float)buff.value);

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
                            player.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Cyan, textureSize: player.sprite.frame.textureSize, priority: 0, framesLeft: -1));
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
            int sleepFrames = Math.Max(buff1.sleepFramesNeededForActivation, buff2.sleepFramesNeededForActivation);
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
                    throw new DivideByZeroException($"Unsupported buff type - {buffType}.");
            }

            return new Buff(type: buffType, value: value, autoRemoveDelay: autoRemoveDelay, isPermanent: buff1.isPermanent, sleepFramesNeededForActivation: sleepFrames, canKill: canKill, increaseIDAtEveryUse: increaseIDAtEveryUse);
        }

    }
}
