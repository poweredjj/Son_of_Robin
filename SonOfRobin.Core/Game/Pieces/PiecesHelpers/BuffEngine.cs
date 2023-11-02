using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class BuffEngine
    {
        public enum BuffType : byte
        {
            InvWidth = 0,
            InvHeight = 1,
            ToolbarWidth = 2,
            ToolbarHeight = 3,
            Speed = 4,
            Strength = 5,
            HP = 6,
            MaxHP = 7,
            MaxFatigue = 8,
            EnableMap = 9,
            RegenPoison = 10,
            Haste = 11,
            Fatigue = 12,
            Sprint = 13,
            SprintCooldown = 14,
            ExtendSprintDuration = 15,
            LowHP = 16,
            Tired = 17,
            Hungry = 18,
            Heat = 19,
            HeatProtection = 20,
            SwampProtection = 21,
            Wet = 22,
            HeatLevelLocked = 23,
            FastMountainWalking = 24,
            CanSeeThroughFog = 25,
            RemovePoison = 26,
        };

        private readonly Dictionary<int, Buff> buffDict;
        private readonly BoardPiece piece;

        public List<Buff> BuffList
        { get { return this.buffDict.Values.ToList(); } }

        public bool HasAnyBuff
        { get { return this.buffDict.Count > 0; } }

        public bool HasPoisonBuff
        {
            get
            {
                foreach (Buff regenPoisonBuff in this.GetBuffsOfType(BuffType.RegenPoison))
                {
                    if ((int)regenPoisonBuff.value < 0) return true;
                }

                return false;
            }
        }

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
              { "buffDict", this.buffDict },
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
                buff.id = Helpers.GetUniqueID();
            }

            if (this.buffDict.ContainsKey(buff.id))
            {
                // in case this happens, it is better not to crash the game
                MessageLog.Add(debugMessage: false, text: $"Buff has been added twice - id {buff.id} type {buff.type}.", bgColor: new Color(105, 3, 18));              
                return;
            }

            if (buff.sleepMinutesNeededForActivation > 0 && !buff.HadEnoughSleepForBuff(world)) return;

            bool hadThisBuffBefore = this.HasBuff(buff.type);
            bool buffHasBeenApplied = this.ProcessBuff(world: world, buff: buff, add: true, hadThisBuffBefore: hadThisBuffBefore);
            if (!buffHasBeenApplied)
            {
                MessageLog.Add(debugMessage: true, text: $"Buff id {buff.id} type {buff.type} value {buff.value} could not be applied to '{this.piece.readableName}'.");
                return;
            }
            this.buffDict[buff.id] = buff;
            if (buff.autoRemoveDelay > 0) new LevelEvent(eventName: LevelEvent.EventName.RemoveBuff, level: this.piece.level, delay: buff.autoRemoveDelay, boardPiece: this.piece, eventHelper: buff.id);

            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Buff added for '{this.piece.readableName}' - id {buff.id} type {buff.type} value {buff.value}.");

            if (buff.isPermanent) this.RemoveBuff(buff.id); // permanent buff should only change value and not be stored and displayed
        }

        public void RemoveEveryBuffOfType(BuffType buffType)
        {
            foreach (var buffId in buffDict.Keys.ToList())
            {
                if (this.buffDict[buffId].type == buffType) this.RemoveBuff(buffId);
            }
        }

        public void RemoveBuff(int buffID, bool checkIfHasThisBuff = true)
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

            // SonOfRobinGame.messageLog.AddMessage(debugMessage: true, text: $"Buff removed - id {buffToRemove.id} type {buffToRemove.type} value {buffToRemove.value}.");
        }

        public List<Buff> GetBuffsOfType(BuffType buffType)
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

        public bool HasBuff(int buffID)
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
                        float buffVal = (float)buff.value;
                        if (!add) buffVal *= -1;

                        float correctedBuffVal = Math.Max(buffVal + this.piece.speed, 1f) - this.piece.speed;
                        if (add && correctedBuffVal == 0) return false;

                        if (correctedBuffVal != buffVal) buff.value = add ? correctedBuffVal : -correctedBuffVal;
                        this.piece.speed += correctedBuffVal;

                        return true;
                    }

                case BuffType.Strength:
                    {
                        int buffVal = (int)buff.value;
                        if (!add) buffVal *= -1;

                        int correctedBuffVal = Math.Max(buffVal + this.piece.strength, 1) - this.piece.strength;
                        if (add && correctedBuffVal == 0) return false;

                        if (correctedBuffVal != buffVal) buff.value = add ? correctedBuffVal : -correctedBuffVal;
                        this.piece.strength += correctedBuffVal;

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
                        float buffVal = (float)buff.value;
                        if (!add) buffVal *= -1;

                        float correctedBuffVal = Math.Max(buffVal + this.piece.maxHitPoints, 3f) - this.piece.maxHitPoints;
                        if (add && correctedBuffVal == 0) return false;

                        if (correctedBuffVal != buffVal) buff.value = add ? correctedBuffVal : -correctedBuffVal;
                        this.piece.maxHitPoints += correctedBuffVal;

                        this.piece.HitPoints = Math.Min(this.piece.HitPoints, this.piece.maxHitPoints);

                        return true;
                    }

                case BuffType.MaxFatigue:
                    {
                        if (!this.CheckIfPieceIsPlayer(buff)) return false;
                        Player player = (Player)this.piece;

                        float buffVal = (float)buff.value;
                        if (!add) buffVal *= -1;

                        float correctedBuffVal = Math.Max(buffVal + player.maxFatigue, 10f) - player.maxFatigue;
                        if (add && correctedBuffVal == 0) return false;

                        if (correctedBuffVal != buffVal) buff.value = add ? correctedBuffVal : -correctedBuffVal;
                        player.maxFatigue += correctedBuffVal;

                        player.Fatigue = Math.Min(player.Fatigue, player.maxFatigue);

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
                        // this buff exists only prevent from getting swamp poison
                        return true;
                    }

                case BuffType.FastMountainWalking:
                    {
                        // this buff exists only to allow for fast mountain walking
                        return true;
                    }

                case BuffType.CanSeeThroughFog:
                    {
                        // this buff exists only to disable fog drawing
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

                        if (add) player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerPulse);
                        else player.activeSoundPack.Stop(PieceSoundPackTemplate.Action.PlayerPulse);

                        return true;
                    }

                case BuffType.RegenPoison:
                    {
                        if (add)
                        {
                            if ((int)buff.value > 0 && this.HasPoisonBuff) return false; // regen cannot replace poison

                            if (hadThisBuffBefore) this.RemoveEveryBuffOfType(buff.type);

                            int delay = 60 * 5;

                            var regenPoisonData = new Dictionary<string, Object> {
                            { "buffID", buff.id }, { "charges", buff.autoRemoveDelay / delay }, { "delay", delay }, { "hpChange", buff.value }, { "canKill", buff.canKill } };
                            new LevelEvent(eventName: LevelEvent.EventName.RegenPoison, level: this.piece.level, delay: delay, boardPiece: this.piece, eventHelper: regenPoisonData);

                            if ((int)buff.value < 0)
                            {
                                this.piece.sprite.color = Color.Chartreuse;
                                this.piece.showStatBarsTillFrame = world.CurrentUpdate + buff.autoRemoveDelay;
                            }
                        }
                        else
                        {
                            if (!this.HasPoisonBuff) this.piece.sprite.color = Color.White;
                        }

                        return true;
                    }

                case BuffType.RemovePoison:
                    {
                        foreach (var buffId in buffDict.Keys.ToList())
                        {
                            if (this.buffDict[buffId].type == BuffType.RegenPoison && (int)this.buffDict[buffId].value < 0) this.RemoveBuff(buffId);
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

                            Sound.QuickPlay(SoundData.Name.Whoosh);

                            int cloneCount = 20;
                            for (int cloneNo = 0; cloneNo < cloneCount; cloneNo++)
                            {
                                BoardPiece hastePlayerClone = PieceTemplate.CreateAndPlaceOnBoard(templateName: PieceTemplate.Name.HastePlayerClone, world: player.world, position: player.sprite.position, precisePlacement: true);

                                Sprite hastePlayerCloneSprite = hastePlayerClone.sprite;
                                hastePlayerCloneSprite.opacity = (1f - ((float)cloneNo / (cloneCount + 1))) * 0.3f;
                                new Tracking(level: player.level, targetSprite: player.sprite, followingSprite: hastePlayerCloneSprite, followSlowDown: cloneNo);
                            }

                            player.level.stateMachineTypesManager.EnableMultiplier((int)buff.value);
                            player.level.stateMachineTypesManager.EnableAllTypes(nthFrame: true);
                            player.level.stateMachineTypesManager.RemoveTheseTypes(typesToRemove: new List<Type> { typeof(Animal) }, everyFrame: true);
                        }
                        else
                        {
                            player.level.stateMachineTypesManager.DisableMultiplier();
                            player.level.stateMachineTypesManager.EnableAllTypes(everyFrame: true, nthFrame: true);
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
                            player.sprite.effectCol.AddEffect(new BorderInstance(outlineColor: Color.Cyan, borderThickness: 1, textureSize: player.sprite.AnimFrame.textureSize, priority: 0, framesLeft: -1));
                            player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerSprint);
                        }
                        else
                        {
                            player.speed -= (float)buff.value;
                            player.sprite.effectCol.RemoveEffectsOfType(effect: SonOfRobinGame.EffectBorder);
                            player.activeSoundPack.Play(PieceSoundPackTemplate.Action.PlayerPant);
                            player.buffEngine.AddBuff(buff: new Buff(type: BuffType.SprintCooldown, autoRemoveDelay: 15 * 60, value: null), world: player.world);
                        }

                        return true;
                    }

                case BuffType.ExtendSprintDuration:
                    {
                        // this buff existence is enough to extend sprint duration
                        return (int)buff.value > 0;
                    }

                case BuffType.SprintCooldown:
                    {
                        // this buff exists only to show negative status and prevent sprint
                        return true;
                    }

                case BuffType.HeatLevelLocked:
                    {
                        // this buff exists only to prevent changes to heat level
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
                MessageLog.Add(debugMessage: true, text: $"Buff '{buff.type}' cannot target {this.piece.GetType()} - ignoring.");
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

        public static List<Buff> MergeSameTypeBuffsInList(List<Buff> buffList)
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
                        newBuff = MergeTwoSameTypeBuffs(buff1: newBuff, buff2: buff);
                    }
                    mergedBuffList.Add(newBuff);
                }
            }

            return mergedBuffList;
        }

        public static Buff MergeMultipleSameTypeBuffs(List<Buff> buffList)
        {
            Buff resultBuff = buffList[0];
            buffList.RemoveAt(0);

            foreach (Buff buff in buffList)
            {
                resultBuff = MergeTwoSameTypeBuffs(buff, resultBuff);
            }

            return resultBuff;
        }

        public static Buff MergeTwoSameTypeBuffs(Buff buff1, Buff buff2)
        {
            if (buff1 == null && buff2 != null) return buff2;
            if (buff2 == null && buff1 != null) return buff1;

            if (buff1.type != buff2.type) throw new ArgumentException($"Buffs' types ({buff1.type}, {buff2.type}) are not the same.");
            if (buff1.isPermanent != buff2.isPermanent) throw new ArgumentException($"Buffs' 'isPermanent' ({buff1.type}, {buff2.type}) are not the same.");

            BuffType buffType = buff1.type;
            int autoRemoveDelay = Math.Max(buff1.autoRemoveDelay, buff2.autoRemoveDelay);
            int sleepMinutesNeededForActivation = Math.Max(buff1.sleepMinutesNeededForActivation, buff2.sleepMinutesNeededForActivation);
            bool canKill = buff1.canKill || buff2.canKill;
            bool increaseIDAtEveryUse = buff1.increaseIDAtEveryUse || buff2.increaseIDAtEveryUse;

            object value = buffType switch
            {
                BuffType.InvWidth => (byte)buff1.value + (byte)buff2.value,
                BuffType.InvHeight => (byte)buff1.value + (byte)buff2.value,
                BuffType.ToolbarWidth => (byte)buff1.value + (byte)buff2.value,
                BuffType.ToolbarHeight => (byte)buff1.value + (byte)buff2.value,
                BuffType.Speed => (float)buff1.value + (float)buff2.value,
                BuffType.Strength => (int)buff1.value + (int)buff2.value,
                BuffType.HP => (float)buff1.value + (float)buff2.value,
                BuffType.MaxHP => (float)buff1.value + (float)buff2.value,
                BuffType.MaxFatigue => (float)buff1.value + (float)buff2.value,
                BuffType.RegenPoison => (int)buff1.value + (int)buff2.value,
                BuffType.Haste => (int)buff1.value + (int)buff2.value,
                BuffType.Fatigue => (float)buff1.value + (float)buff2.value,
                BuffType.EnableMap => null,
                BuffType.Tired => null,
                BuffType.LowHP => null,
                BuffType.Hungry => null,
                BuffType.Heat => null,
                BuffType.HeatProtection => null,
                BuffType.SwampProtection => null,
                BuffType.RemovePoison => null,
                BuffType.CanSeeThroughFog => null,
                BuffType.FastMountainWalking => null,
                BuffType.Wet => null,
                BuffType.Sprint => (float)buff1.value + (float)buff2.value,
                BuffType.ExtendSprintDuration => (int)buff1.value + (int)buff2.value,
                BuffType.SprintCooldown => null,
                BuffType.HeatLevelLocked => null,
                _ => throw new ArgumentException($"Unsupported buff type - {buffType}."),
            };
            return new Buff(type: buffType, value: value, autoRemoveDelay: autoRemoveDelay, isPermanent: buff1.isPermanent, sleepMinutesNeededForActivation: sleepMinutesNeededForActivation, canKill: canKill, increaseIDAtEveryUse: increaseIDAtEveryUse);
        }
    }
}