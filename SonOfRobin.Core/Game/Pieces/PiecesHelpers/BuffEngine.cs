using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType { InvWidth, InvHeight, ToolbarWidth, ToolbarHeight, Speed, EnableMap };

        [Serializable]
        public struct Buff
        {
            public readonly int id;
            public readonly BuffType type;
            public readonly object value;
            public Buff(World world, BuffType type, object value)
            {
                this.id = world.currentBuffId;
                world.currentBuffId++;
                this.type = type;
                this.value = value;
            }
        }

        private readonly Dictionary<int, Buff> buffDict;
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

        public void AddBuffs(List<Buff> buffList, int autoRemoveDelay = 0)
        {
            foreach (Buff buff in buffList)
            { this.AddBuff(buff: buff, autoRemoveDelay: autoRemoveDelay); }
        }

        public void RemoveBuffs(List<Buff> buffList)
        {
            foreach (Buff buff in buffList)
            { this.RemoveBuff(buff.id); }
        }

        public void AddBuff(Buff buff, int autoRemoveDelay = 0)
        {
            if (this.buffDict.ContainsKey(buff.id)) throw new DivideByZeroException($"Buff has been added twice - id {buff.id} type {buff.type}.");

            bool hadThisBuffBefore = this.HasBuff(buff.type);
            this.buffDict[buff.id] = buff;
            this.ProcessBuff(buff: buff, add: true, hadThisBuffBefore: hadThisBuffBefore);

            // AutoRemoveDelay should not be used for equip!
            // It should only be used for temporary buffs (food, etc.).

            if (autoRemoveDelay > 0) new WorldEvent(eventName: WorldEvent.EventName.RemoveBuff, world: this.piece.world, delay: autoRemoveDelay, boardPiece: this.piece, eventHelper: buff.id);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Buff added - id {buff.id} type {buff.type} value {buff.value}.");
        }

        public void RemoveBuff(int buffId)
        {
            if (!this.buffDict.ContainsKey(buffId)) throw new DivideByZeroException($"Buff not found during removal - id {buffId}.");
            Buff buffToRemove = this.buffDict[buffId];

            BuffType typeToCheck = buffDict[buffId].type;
            this.buffDict.Remove(buffId);
            bool stillHasThisBuff = this.HasBuff(typeToCheck);
            this.ProcessBuff(buff: buffToRemove, add: false, stillHasThisBuff: stillHasThisBuff);

            MessageLog.AddMessage(currentFrame: SonOfRobinGame.currentUpdate, msgType: MsgType.Debug, message: $"Buff removed - id {buffToRemove.id} type {buffToRemove.type} value {buffToRemove.value}.");
        }

        public bool HasBuff(BuffType buffType)
        {
            var buffsOfType = this.buffDict.Values.Where(buff => buff.type == buffType).ToList();
            return buffsOfType.Count > 0;
        }

        private void ProcessBuff(Buff buff, bool add, bool hadThisBuffBefore = false, bool stillHasThisBuff = false)
        {
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

                default:
                    throw new DivideByZeroException($"Unsupported buff type - {buff.type}.");
            }
        }


    }
}
