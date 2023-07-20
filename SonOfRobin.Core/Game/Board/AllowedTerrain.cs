using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class AllowedTerrain
    {
        public enum RangeName
        { All, WaterAll, WaterShallow, WaterMedium, WaterDeep, GroundSand, GroundAll, Volcano, NoBiome, Biome };

        private readonly Dictionary<Terrain.Name, AllowedRange> rangesByTerrainName;
        private readonly Dictionary<ExtBoardProps.Name, bool> extPropertiesDict;
        public bool HasBeenChanged { get; private set; }

        public AllowedTerrain(Dictionary<Terrain.Name, AllowedRange> rangeDict = null, Dictionary<ExtBoardProps.Name, bool> extPropertiesDict = null)
        {
            // var exampleRangeDict = new Dictionary<string, AllowedRange>() { { Terrain.Name.Height, new AllowedRange(min: 0, max: 128) } };

            this.rangesByTerrainName = rangeDict == null ? new Dictionary<Terrain.Name, AllowedRange>() : rangeDict;
            this.extPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.Name, bool>() : extPropertiesDict;

            this.HasBeenChanged = false;
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
                        throw new ArgumentException($"Unsupported range name - {rangeName}.");
                }
            }

            this.rangesByTerrainName = rangeDict;

            this.extPropertiesDict = extPropertiesDict == null ? new Dictionary<ExtBoardProps.Name, bool>() : extPropertiesDict;

            this.HasBeenChanged = false;
        }

        private static void AddRangeToRangeDict(Dictionary<Terrain.Name, AllowedRange> rangeDict, Terrain.Name terrainName, byte min, byte max)
        {
            if (!rangeDict.ContainsKey(terrainName)) rangeDict[terrainName] = new AllowedRange(min: min, max: max);
            else rangeDict[terrainName].ExpandRange(expandedMin: min, expandedMax: max);
        }

        public void CopyTerrainFromTemplate(AllowedTerrain allowedTerrainTemplate)
        {
            this.rangesByTerrainName.Clear();
            foreach (var kvp in allowedTerrainTemplate.rangesByTerrainName)
            {
                this.AddUpdateTerrain(terrainName: kvp.Key, allowedRange: kvp.Value);
            }

            this.extPropertiesDict.Clear();
            foreach (var kvp in allowedTerrainTemplate.extPropertiesDict)
            {
                this.AddUpdateNameInExtProperties(name: kvp.Key, value: kvp.Value);
            }

            this.HasBeenChanged = true;
        }

        public void AddUpdateTerrain(Terrain.Name terrainName, AllowedRange allowedRange)
        {
            this.rangesByTerrainName[terrainName] = allowedRange;
            this.HasBeenChanged = true;
        }

        public void RemoveTerrain(Terrain.Name terrainName)
        {
            this.rangesByTerrainName.Remove(terrainName);
            this.HasBeenChanged = true;
        }

        public void ClearTerrain()
        {
            this.rangesByTerrainName.Clear();
        }

        public void AddUpdateNameInExtProperties(ExtBoardProps.Name name, bool value)
        {
            this.extPropertiesDict[name] = value;
            this.HasBeenChanged = true;
        }

        public void RemoveNameInExtProperties(ExtBoardProps.Name name)
        {
            if (this.extPropertiesDict.ContainsKey(name)) this.extPropertiesDict.Remove(name);
            this.HasBeenChanged = true;
        }

        public void ClearExtProperties()
        {
            this.extPropertiesDict.Clear();
            this.HasBeenChanged = true;
        }

        public bool IsInRange(Terrain.Name terrainName, byte fieldValue)
        {
            return this.rangesByTerrainName[terrainName].IsInRange(fieldValue);
        }

        public bool CanStandHere(Vector2 position, World world)
        {
            foreach (var kvp in this.extPropertiesDict)
            {
                bool value = world.Grid.GetExtProperty(name: kvp.Key, position: position);
                if (value != kvp.Value) return false;
            }

            foreach (var kvp in this.rangesByTerrainName)
            {
                var fieldValue = world.Grid.GetFieldValue(position: position, terrainName: kvp.Key);
                if (!kvp.Value.IsInRange(fieldValue)) return false;
            }

            return true;
        }

        public byte GetMinValForTerrainName(Terrain.Name terrainName)
        {
            return this.rangesByTerrainName.ContainsKey(terrainName) ? this.rangesByTerrainName[terrainName].Min : (byte)0;
        }

        public byte GetMaxValForTerrainName(Terrain.Name terrainName)
        {
            return this.rangesByTerrainName.ContainsKey(terrainName) ? this.rangesByTerrainName[terrainName].Max : (byte)255;
        }

        public Dictionary<ExtBoardProps.Name, bool> GetExtPropertiesDict()
        {
            return this.extPropertiesDict.ToDictionary(entry => entry.Key, entry => entry.Value); // to avoid exposing original dictionary
        }

        public Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> terrainDict = new Dictionary<string, object>
            {
                { "currentRangesByTerrainName", this.rangesByTerrainName },
                { "currentExtPropertiesDict", this.extPropertiesDict },
            };

            return terrainDict;
        }

        public static AllowedTerrain Deserialize(Object terrainData)
        {
            var terrainDict = (Dictionary<string, Object>)terrainData;

            Dictionary<Terrain.Name, AllowedRange> rangeDict;
            Dictionary<ExtBoardProps.Name, bool> extPropertiesDict;

            rangeDict = (Dictionary<Terrain.Name, AllowedRange>)terrainDict["currentRangesByTerrainName"];
            extPropertiesDict = (Dictionary<ExtBoardProps.Name, bool>)terrainDict["currentExtPropertiesDict"];

            return new(rangeDict: rangeDict, extPropertiesDict: extPropertiesDict)
            {
                HasBeenChanged = true
            };
        }
    }
}