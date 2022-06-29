using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType { InvWidth, InvHeight, ToolbarWidth, ToolbarHeight, Speed, Strength, MaxHp, EnableMap, Tired };

        [Serializable]
        public class Buff
        {
            public int id;
            public readonly bool increaseIDAtEveryUse;
            public readonly BuffType type;
            public readonly object value;
            public readonly int autoRemoveDelay;
            public int activationFrame;
            public int endFrame;
            public readonly bool isPositive;
            public readonly int sleepFrames;
            public Buff(World world, BuffType type, object value, bool isPositive, int autoRemoveDelay = 0, int sleepFrames = 0, bool increaseIDAtEveryUse = false)
            {
                this.increaseIDAtEveryUse = increaseIDAtEveryUse; // for buffs that could stack (like sleeping buffs)
                this.id = world.currentBuffId;
                world.currentBuffId++;
                this.type = type;
                this.value = value;
                this.isPositive = isPositive;
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

                        case BuffType.MaxHp:
                            description = $"Max HP {sign}{this.value}.";
                            break;

                        case BuffType.EnableMap:
                            description = "Shows map of visited places.";
                            break;

                        case BuffType.Tired:
                            description = "Decreases multiple stats.";
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

                        case BuffType.Speed:
                            if ((float)this.value > 0) return $"SPD\n+{this.value}";
                            else return $"SPD\n{this.value}";

                        case BuffType.Strength:
                            if ((int)this.value > 0) return $"STR\n+{this.value}";
                            else return $"STR\n{this.value}";

                        case BuffType.MaxHp:
                            if ((float)this.value > 0) return $"HP\n+{this.value}";
                            else return $"HP\n{this.value}";

                        case BuffType.EnableMap:
                            return null;

                        case BuffType.Tired:
                            return "tired";

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

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Buff added - id {buff.id} type {buff.type} value {buff.value}.");
        }

        public void RemoveEveryBuffOfType(BuffType buffType)
        {
            foreach (var buffId in buffDict.Keys.ToList())
            {
                if (this.buffDict[buffId].type == buffType) this.buffDict.Remove(buffId);
            }
        }

        public void RemoveBuff(int buffId)
        {
            if (!this.buffDict.ContainsKey(buffId)) throw new DivideByZeroException($"Buff not found during removal - id {buffId}.");
            Buff buffToRemove = this.buffDict[buffId];

            BuffType typeToCheck = buffDict[buffId].type;
            this.buffDict.Remove(buffId);
            bool stillHasThisBuff = this.HasBuff(typeToCheck);
            this.ProcessBuff(buff: buffToRemove, add: false, stillHasThisBuff: stillHasThisBuff, world: null);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Buff removed - id {buffToRemove.id} type {buffToRemove.type} value {buffToRemove.value}.");
        }

        public bool HasBuff(BuffType buffType)
        {
            var buffsOfType = this.buffDict.Values.Where(buff => buff.type == buffType).ToList();
            return buffsOfType.Count > 0;
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
                        if (add) player.invWidth += (byte)buff.value;
                        else player.invWidth -= (byte)buff.value;

                        player.pieceStorage.Resize(player.invWidth, player.invHeight);

                        break;
                    }

                case BuffType.InvHeight:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.invHeight += (byte)buff.value;
                        else player.invHeight -= (byte)buff.value;

                        player.pieceStorage.Resize(player.invWidth, player.invHeight);

                        break;
                    }

                case BuffType.ToolbarWidth:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.toolbarWidth += (byte)buff.value;
                        else player.toolbarWidth -= (byte)buff.value;

                        player.toolStorage.Resize(player.toolbarWidth, player.toolbarHeight);

                        break;
                    }

                case BuffType.ToolbarHeight:
                    {
                        Player player = (Player)this.piece;
                        if (add) player.toolbarHeight += (byte)buff.value;
                        else player.toolbarHeight -= (byte)buff.value;

                        player.toolStorage.Resize(player.toolbarWidth, player.toolbarHeight);

                        break;
                    }

                case BuffType.Speed:
                    {
                        if (add) this.piece.speed += (float)buff.value;
                        else this.piece.speed -= (float)buff.value;

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

                case BuffType.EnableMap:
                    {
                        Player player = (Player)this.piece;

                        if (add)
                        {
                            player.world.mapEnabled = true;
                            if (!hadThisBuffBefore && SonOfRobinGame.platform == Platform.Desktop) player.world.ToggleMapMode();
                        }
                        else
                        {
                            if (!stillHasThisBuff)
                            {
                                player.world.mapEnabled = false;
                                player.world.DisableMap();
                            }
                        }

                        break;
                    }

                case BuffType.Tired:
                    {
                        // this buff exists only to show negative status icon
                        break;
                    }


                case BuffType.Strength:
                    {
                        int value = (int)buff.value;
                        if (!add) value *= -1;
                        this.piece.strength += value;

                        break;
                    }

                default:
                    throw new DivideByZeroException($"Unsupported buff type - {buff.type}.");
            }
        }


    }
}
