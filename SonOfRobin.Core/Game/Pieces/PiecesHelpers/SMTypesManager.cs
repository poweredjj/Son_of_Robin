using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class SMTypesManager // short for "State Machine Types Manager"
    {
        private readonly Level level;
        private readonly List<Type> allTypes;
        private List<Type> enabledTypesEveryFrame;
        private List<Type> enabledTypesNthFrame;
        private int nthFrameMultiplier;
        private readonly Dictionary<Type, int> deltaCountersForTypes;

        public SMTypesManager(Level level)
        {
            this.level = level;
            this.allTypes = new List<Type>();
            this.deltaCountersForTypes = new Dictionary<Type, int>();

            foreach (PieceInfo.Info info in PieceInfo.AllInfo)
            {
                if (!this.allTypes.Contains(info.type))
                {
                    this.allTypes.Add(info.type);
                    this.deltaCountersForTypes[info.type] = 0;
                }
            }

            this.enabledTypesEveryFrame = new List<Type>();
            this.enabledTypesNthFrame = new List<Type>();

            this.nthFrameMultiplier = 0; // 0 == disabled

            this.EnableAllTypes(everyFrame: true, nthFrame: true);
        }

        public void IncreaseDeltaCounters(int increaseVal)
        {
            foreach (Type type in allTypes)
            {
                if (this.enabledTypesEveryFrame.Contains(type)) this.deltaCountersForTypes[type] += increaseVal;
                else if (this.enabledTypesNthFrame.Contains(type))
                {
                    if (this.nthFrameMultiplier == 0) this.deltaCountersForTypes[type] += increaseVal;
                    else if (this.level.world.CurrentUpdate % this.nthFrameMultiplier == 0) this.deltaCountersForTypes[type] += increaseVal;
                }
            }
        }

        public int GetDeltaCounterForType(Type type)
        {
            return this.deltaCountersForTypes[type];
        }

        private List<int> ConvertTypeListToIntList(List<Type> typeList)
        {
            // Type list cannot be serialized directly.

            var intList = new List<int>();

            foreach (Type type in typeList)
            {
                intList.Add(this.ConvertIntToType(type));
            }
            return intList;
        }

        private int ConvertIntToType(Type type)
        {
            for (int i = 0; i < this.allTypes.Count; i++)
            {
                if (type == this.allTypes[i]) return i;
            }

            throw new ArgumentException($"Cannot find type {type} in allTypes.");
        }

        private List<Type> ConvertIntListToTypeList(List<int> intList)
        {
            // Type list cannot be serialized directly.

            var typeList = new List<Type>();

            foreach (int intToConvert in intList)
            {
                typeList.Add(this.allTypes[intToConvert]);
            }

            return typeList;
        }

        public void EnableMultiplier(int multiplier)
        {
            if (multiplier <= 0) throw new ArgumentException($"Multiplier ({multiplier}) must be greater than 0.");

            this.nthFrameMultiplier = multiplier;
        }

        public void DisableMultiplier()
        {
            this.nthFrameMultiplier = 0;
        }

        public void EnableAllTypes(bool everyFrame = false, bool nthFrame = false)
        {
            if (everyFrame) this.enabledTypesEveryFrame = this.allTypes.ToList();
            if (nthFrame) this.enabledTypesNthFrame = this.allTypes.ToList();
        }

        public void DisableAllTypes(bool everyFrame = false, bool nthFrame = false)
        {
            if (everyFrame) this.enabledTypesEveryFrame.Clear();
            if (nthFrame) this.enabledTypesNthFrame.Clear();
        }

        public void SetOnlyTheseTypes(List<Type> enabledTypes, bool everyFrame = false, bool nthFrame = false)
        {
            if (everyFrame) this.enabledTypesEveryFrame = enabledTypes.ToList();
            if (nthFrame) this.enabledTypesNthFrame = enabledTypes.ToList();
        }

        public void AddTheseTypes(List<Type> typesToAdd, bool everyFrame = false, bool nthFrame = false)
        {
            foreach (Type type in typesToAdd)
            {
                if (everyFrame && !this.enabledTypesEveryFrame.Contains(type)) this.enabledTypesEveryFrame.Add(type);
                if (nthFrame && !this.enabledTypesNthFrame.Contains(type)) this.enabledTypesNthFrame.Add(type);
            }
        }

        public void RemoveTheseTypes(List<Type> typesToRemove, bool everyFrame = false, bool nthFrame = false)
        {
            foreach (Type type in typesToRemove)
            {
                if (everyFrame && this.enabledTypesEveryFrame.Contains(type)) this.enabledTypesEveryFrame.Remove(type);
                if (nthFrame && this.enabledTypesNthFrame.Contains(type)) this.enabledTypesNthFrame.Remove(type);
            }
        }

        public bool CanBeProcessed(BoardPiece boardPiece)
        {
            Type type = boardPiece.GetType();

            if (this.enabledTypesEveryFrame.Contains(type)) return true;
            if (this.nthFrameMultiplier != 0 && this.level.world.CurrentUpdate % this.nthFrameMultiplier == 0) return this.enabledTypesNthFrame.Contains(type);

            return false;
        }
    }
}