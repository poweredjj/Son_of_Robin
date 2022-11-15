using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    [Serializable]
    public class AllowedTerrain
    {
        public enum RangeName
        { All, WaterAll, WaterShallow, WaterMedium, WaterDeep, GroundSand, GroundAll, Volcano, NoBiome, Biome };

        private readonly Dictionary<Terrain.Name, AllowedRange> initialRangesByTerrainName; // never to be changed
        private Dictionary<Terrain.Name, AllowedRange> currentRangesByTerrainName;

        private readonly Dictionary<ExtBoardProps.Name, bool> initialExtPropertiesDict;
        private Dictionary<ExtBoardProps.Name, bool> currentExtPropertiesDict;

        public AllowedTerrain(Dictionary<ExtBoardProps.Name, bool> extPropertiesDict = null)
        {
            // no fields defined - all fields allowed
            this.initialRangesByTerrainName = new Dictionary<Terrain.Name, AllowedRange> { };
            MakeRangeDictReadOnly(this.initialRangesByTerrainName);
            this.currentRangesByTerrainName = null;

            this.initialExtPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.Name, bool>() : extPropertiesDict;
            this.currentExtPropertiesDict = null;
        }

        public AllowedTerrain(Dictionary<Terrain.Name, AllowedRange> rangeDict, Dictionary<ExtBoardProps.Name, bool> extPropertiesDict = null)
        {
            // var exampleRangeDict = new Dictionary<string, AllowedRange>() { { Terrain.Name.Height, new AllowedRange(min: 0, max: 128) } };

            this.initialRangesByTerrainName = rangeDict;
            MakeRangeDictReadOnly(this.initialRangesByTerrainName);
            this.currentRangesByTerrainName = null;

            this.initialExtPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.Name, bool>() : extPropertiesDict;
            this.currentExtPropertiesDict = null;
        }

        public AllowedTerrain(List<RangeName> rangeNameList, Dictionary<ExtBoardProps.Name, bool> extPropertiesDict = null)
        {
            // var exampleRangeNameList = new List<string>() { BoardColors.WaterShallow, "ground_all" };

            var rangeDict = new Dictionary<Terrain.Name, AllowedRange> { };

            foreach (var rangeName in rangeNameList)
            {
                switch (rangeName)
                {
                    case RangeName.All:
                        foreach (Terrain.Name terrainName in Terrain.allTerrains)
                        {
                            AddRangeToRangeDict(rangeDict: rangeDict, terrainName: terrainName, min: 0, max: 255);
                        }
                        break;

                    case RangeName.WaterAll:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: 0, max: Terrain.waterLevelMax);
                        break;

                    case RangeName.WaterShallow:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: Convert.ToByte((Terrain.waterLevelMax / 3) * 2), max: Terrain.waterLevelMax);
                        break;

                    case RangeName.WaterMedium:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: Convert.ToByte(Terrain.waterLevelMax / 3), max: Convert.ToByte((Terrain.waterLevelMax / 3) * 2));
                        break;

                    case RangeName.WaterDeep:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: 0, max: Convert.ToByte(Terrain.waterLevelMax / 3));
                        break;

                    case RangeName.GroundSand:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: (byte)(Terrain.waterLevelMax + 1), max: 105);
                        break;

                    case RangeName.GroundAll:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: (byte)(Terrain.waterLevelMax + 1), max: (byte)(Terrain.volcanoEdgeMin - 1));
                        break;

                    case RangeName.Volcano:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Height, min: (byte)(Terrain.volcanoEdgeMin - 1), max: 255);
                        break;

                    case RangeName.NoBiome:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Biome, min: 0, max: Terrain.biomeMin);
                        break;

                    case RangeName.Biome:
                        AddRangeToRangeDict(rangeDict: rangeDict, terrainName: Terrain.Name.Biome, min: (byte)(Terrain.biomeMin + 1), max: 255);
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported range name - {rangeName}.");
                }
            }

            this.initialRangesByTerrainName = rangeDict;
            MakeRangeDictReadOnly(this.initialRangesByTerrainName);

            this.initialExtPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.Name, bool>() : extPropertiesDict;
            this.currentExtPropertiesDict = null;
        }

        private static void MakeRangeDictReadOnly(Dictionary<Terrain.Name, AllowedRange> rangeDict)
        {
            foreach (AllowedRange allowedRange in rangeDict.Values)
            {
                allowedRange.MakeReadOnly();
            }
        }

        public static void AddRangeToRangeDict(Dictionary<Terrain.Name, AllowedRange> rangeDict, Terrain.Name terrainName, byte min, byte max)
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

        public void RemoveTerrain(Terrain.Name terrainName)
        {
            if (this.currentRangesByTerrainName is null) this.currentRangesByTerrainName = CopyRangeDict(this.initialRangesByTerrainName);

            this.currentRangesByTerrainName.Remove(terrainName);
        }

        private static Dictionary<Terrain.Name, AllowedRange> CopyRangeDict(Dictionary<Terrain.Name, AllowedRange> rangeDict)
        {
            var newRangeDict = new Dictionary<Terrain.Name, AllowedRange>();

            foreach (var kvp in rangeDict)
            {
                newRangeDict[kvp.Key] = kvp.Value.GetRangeCopy(isReadOnly: false);
            }

            return newRangeDict;
        }

        private static Dictionary<ExtBoardProps.Name, bool> CopyExtPropertiesDict(Dictionary<ExtBoardProps.Name, bool> extPropertiesDict)
        {
            var newExtPropertiesDict = new Dictionary<ExtBoardProps.Name, bool>();

            foreach (var kvp in extPropertiesDict)
            {
                newExtPropertiesDict[kvp.Key] = kvp.Value;
            }

            return newExtPropertiesDict;
        }

        public void AddUpdateNameInExtProperties(ExtBoardProps.Name name, bool value)
        {
            if (this.currentExtPropertiesDict is null) this.currentExtPropertiesDict = CopyExtPropertiesDict(this.initialExtPropertiesDict);

            this.currentExtPropertiesDict[name] = value;
        }

        public void RemoveNameInExtProperties(ExtBoardProps.Name name)
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

                if (kvp.Key == ExtBoardProps.Name.BiomeSwamp)
                {
                    var a = 1;
                }

                if (value != kvp.Value) return false;
            }

            var rangesToCheck = this.currentRangesByTerrainName == null ? this.initialRangesByTerrainName : this.currentRangesByTerrainName;

            foreach (var kvp in rangesToCheck)
            {
                var fieldValue = world.grid.GetFieldValue(position: position, terrainName: kvp.Key);
                if (!kvp.Value.IsInRange(fieldValue)) return false;
            }

            return true;
        }

        public AllowedRange GetInitialRangeForTerrainName(Terrain.Name terrainName)
        {
            if (this.initialRangesByTerrainName.ContainsKey(terrainName)) return this.initialRangesByTerrainName[terrainName];
            else return null;
        }

        public Dictionary<ExtBoardProps.Name, bool> GetInitialExtPropertiesDict()
        {
            return this.initialExtPropertiesDict;
        }
    }
}