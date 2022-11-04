using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    [Serializable]
    public class AllowedTerrain
    {
        public enum RangeName
        { All, WaterAll, WaterShallow, WaterMedium, WaterDeep, GroundSand, GroundAll, Volcano, NoDanger, Danger };

        public static readonly TerrainName[] allTerrains = (TerrainName[])Enum.GetValues(typeof(TerrainName));

        private readonly Dictionary<TerrainName, AllowedRange> initialRangesByTerrainName; // never to be changed
        private Dictionary<TerrainName, AllowedRange> currentRangesByTerrainName;

        private readonly Dictionary<ExtBoardProps.ExtPropName, bool> initialExtPropertiesDict;
        private Dictionary<ExtBoardProps.ExtPropName, bool> currentExtPropertiesDict;

        public AllowedTerrain(Dictionary<ExtBoardProps.ExtPropName, bool> extPropertiesDict = null)
        {
            // no fields defined - all fields allowed
            this.initialRangesByTerrainName = new Dictionary<TerrainName, AllowedRange> { };
            MakeRangeDictReadOnly(this.initialRangesByTerrainName);
            this.currentRangesByTerrainName = null;

            this.initialExtPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.ExtPropName, bool>() : extPropertiesDict;
            this.currentExtPropertiesDict = null;
        }

        public AllowedTerrain(Dictionary<TerrainName, AllowedRange> rangeDict, Dictionary<ExtBoardProps.ExtPropName, bool> extPropertiesDict = null)
        {
            // var exampleRangeDict = new Dictionary<string, AllowedRange>() { { TerrainName.Height, new AllowedRange(min: 0, max: 128) } };

            this.initialRangesByTerrainName = rangeDict;
            MakeRangeDictReadOnly(this.initialRangesByTerrainName);
            this.currentRangesByTerrainName = null;

            this.initialExtPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.ExtPropName, bool>() : extPropertiesDict;
            this.currentExtPropertiesDict = null;
        }

        public AllowedTerrain(List<RangeName> rangeNameList, Dictionary<ExtBoardProps.ExtPropName, bool> extPropertiesDict = null)
        {
            // var exampleRangeNameList = new List<string>() { BoardColors.WaterShallow, "ground_all" };

            var rangeDict = new Dictionary<TerrainName, AllowedRange> { };

            foreach (var rangeName in rangeNameList)
            {
                switch (rangeName)
                {
                    case RangeName.All:
                        foreach (TerrainName terrainName in allTerrains)
                        {
                            AddRangeToRangeDict(rangeDict: rangeDict, terrainName: terrainName, min: 0, max: 255);
                        }
                        break;

                    case RangeName.WaterAll:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: 0, max: Terrain.waterLevelMax);
                        break;

                    case RangeName.WaterShallow:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: Convert.ToByte((Terrain.waterLevelMax / 3) * 2), max: Terrain.waterLevelMax);
                        break;

                    case RangeName.WaterMedium:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: Convert.ToByte(Terrain.waterLevelMax / 3), max: Convert.ToByte((Terrain.waterLevelMax / 3) * 2));
                        break;

                    case RangeName.WaterDeep:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: 0, max: Convert.ToByte(Terrain.waterLevelMax / 3));
                        break;

                    case RangeName.GroundSand:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: (byte)(Terrain.waterLevelMax + 1), max: 105);
                        break;

                    case RangeName.GroundAll:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: (byte)(Terrain.waterLevelMax + 1), max: (byte)(Terrain.volcanoEdgeMin - 1));
                        break;

                    case RangeName.Volcano:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Height, min: (byte)(Terrain.volcanoEdgeMin - 1), max: 255);
                        break;

                    case RangeName.NoDanger:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Danger, min: 0, max: Terrain.safeZoneMax);
                        break;

                    case RangeName.Danger:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: TerrainName.Danger, min: (byte)(Terrain.safeZoneMax + 1), max: 255);
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported range name - {rangeName}.");
                }
            }

            this.initialRangesByTerrainName = rangeDict;
            MakeRangeDictReadOnly(this.initialRangesByTerrainName);

            this.initialExtPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.ExtPropName, bool>() : extPropertiesDict;
            this.currentExtPropertiesDict = null;
        }

        private static void MakeRangeDictReadOnly(Dictionary<TerrainName, AllowedRange> rangeDict)
        {
            foreach (AllowedRange allowedRange in rangeDict.Values)
            {
                allowedRange.MakeReadOnly();
            }
        }

        public static void AddRangeToRangeDict(Dictionary<TerrainName, AllowedRange> rangeDict, TerrainName terrainName, byte min, byte max)
        {
            if (!rangeDict.ContainsKey(terrainName))
            {
                rangeDict[terrainName] = new AllowedRange(min: min, max: max);
            }
            else
            {
                rangeDict[terrainName].ExpandRange(expandedMin: min, expandedMax: max);
            }
        }

        public void RemoveTerrain(TerrainName terrainName)
        {
            if (this.currentRangesByTerrainName is null) this.currentRangesByTerrainName = CopyRangeDict(this.initialRangesByTerrainName);

            this.currentRangesByTerrainName.Remove(terrainName);
        }

        private static Dictionary<TerrainName, AllowedRange> CopyRangeDict(Dictionary<TerrainName, AllowedRange> rangeDict)
        {
            var newRangeDict = new Dictionary<TerrainName, AllowedRange>();

            foreach (var kvp in rangeDict)
            {
                newRangeDict[kvp.Key] = kvp.Value.GetRangeCopy(isReadOnly: false);
            }

            return newRangeDict;
        }

        private static Dictionary<ExtBoardProps.ExtPropName, bool> CopyExtPropertiesDict(Dictionary<ExtBoardProps.ExtPropName, bool> extPropertiesDict)
        {
            var newExtPropertiesDict = new Dictionary<ExtBoardProps.ExtPropName, bool>();

            foreach (var kvp in extPropertiesDict)
            {
                newExtPropertiesDict[kvp.Key] = kvp.Value;
            }

            return newExtPropertiesDict;
        }

        public void AddUpdateNameInExtProperties(ExtBoardProps.ExtPropName name, bool value)
        {
            if (this.currentExtPropertiesDict is null) this.currentExtPropertiesDict = CopyExtPropertiesDict(this.initialExtPropertiesDict);

            this.currentExtPropertiesDict[name] = value;
        }

        public void RemoveNameInExtProperties(ExtBoardProps.ExtPropName name)
        {
            if (this.currentExtPropertiesDict is null) this.currentExtPropertiesDict = CopyExtPropertiesDict(this.initialExtPropertiesDict);

            if (this.currentExtPropertiesDict.ContainsKey(name)) this.currentExtPropertiesDict.Remove(name);
        }

        public void ClearExtProperties()
        {
            if (this.currentExtPropertiesDict is null) this.currentExtPropertiesDict = CopyExtPropertiesDict(this.initialExtPropertiesDict);

            this.currentExtPropertiesDict.Clear();
        }

        public bool CanStandHere(Vector2 position, World world)
        {
            var extPropertiesDict = this.currentExtPropertiesDict == null ? this.initialExtPropertiesDict : this.currentExtPropertiesDict;

            foreach (var kvp in extPropertiesDict)
            {
                bool value = world.grid.GetExtProperty(name: kvp.Key, position: position);
                if (value != kvp.Value) return false;
            }

            var rangesToCheck = this.currentRangesByTerrainName == null ? this.initialRangesByTerrainName : this.currentRangesByTerrainName;

            foreach (var kvp in rangesToCheck)
            {
                var fieldValue = world.grid.GetFieldValue(position: position, terrainName: kvp.Key);
                bool terrainOk = kvp.Value.IsInRange(fieldValue);

                if (!terrainOk) return false;
            }

            return true;
        }

        public AllowedRange GetInitialRangeForTerrainName(TerrainName terrainName)
        {
            if (this.initialRangesByTerrainName.ContainsKey(terrainName)) return this.initialRangesByTerrainName[terrainName];
            else return null;
        }

        public Dictionary<ExtBoardProps.ExtPropName, bool> GetInitialExtPropertiesDict()
        {
            return this.initialExtPropertiesDict;
        }
    }
}