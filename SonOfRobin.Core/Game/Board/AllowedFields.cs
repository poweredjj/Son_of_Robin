using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    [Serializable]
    public class AllowedFields
    {
        public enum RangeName { All, WaterAll, WaterShallow, WaterMedium, WaterDeep, GroundSand, GroundAll, Volcano, NoDanger, Danger };

        public static readonly TerrainName[] allTerrains = (TerrainName[])Enum.GetValues(typeof(TerrainName));

        private Dictionary<TerrainName, AllowedRange> rangesByTerrainName;

        public AllowedFields()
        {
            // No fields defined - all fields allowed.

            this.rangesByTerrainName = new Dictionary<TerrainName, AllowedRange> { };
        }

        public AllowedFields(Dictionary<TerrainName, AllowedRange> rangeDict)
        {
            // var exampleRangeDict = new Dictionary<string, AllowedRange>() { { TerrainName.Height, new AllowedRange(min: 0, max: 128) } };

            this.rangesByTerrainName = rangeDict;
        }


        public AllowedFields(List<RangeName> rangeNameList)
        {
            // var exampleRangeNameList = new List<string>() { BoardColors.WaterShallow, "ground_all" };

            this.rangesByTerrainName = new Dictionary<TerrainName, AllowedRange> { };

            foreach (var rangeName in rangeNameList)
            {

                switch (rangeName)
                {
                    case RangeName.All:
                        foreach (TerrainName terrainName in allTerrains)
                        { this.AddRange(terrainName: terrainName, min: 0, max: 255); }
                        break;

                    case RangeName.WaterAll:
                        this.AddRange(terrainName: TerrainName.Height, min: 0, max: Terrain.waterLevelMax);
                        break;

                    case RangeName.WaterShallow:
                        this.AddRange(terrainName: TerrainName.Height, min: Convert.ToByte((Terrain.waterLevelMax / 3) * 2), max: Terrain.waterLevelMax);
                        break;

                    case RangeName.WaterMedium:
                        this.AddRange(terrainName: TerrainName.Height, min: Convert.ToByte(Terrain.waterLevelMax / 3), max: Convert.ToByte((Terrain.waterLevelMax / 3) * 2));
                        break;

                    case RangeName.WaterDeep:
                        this.AddRange(terrainName: TerrainName.Height, min: 0, max: Convert.ToByte(Terrain.waterLevelMax / 3));
                        break;

                    case RangeName.GroundSand:
                        this.AddRange(terrainName: TerrainName.Height, min: (byte)(Terrain.waterLevelMax + 1), max: 105);
                        break;

                    case RangeName.GroundAll:
                        this.AddRange(terrainName: TerrainName.Height, min: (byte)(Terrain.waterLevelMax + 1), max: (byte)(Terrain.volcanoEdgeMin - 1));
                        break;

                    case RangeName.Volcano:
                        this.AddRange(terrainName: TerrainName.Height, min: (byte)(Terrain.volcanoEdgeMin - 1), max: 255);
                        break;

                    case RangeName.NoDanger:
                        this.AddRange(terrainName: TerrainName.Danger, min: 0, max: Terrain.safeZoneMax);
                        break;

                    case RangeName.Danger:
                        this.AddRange(terrainName: TerrainName.Danger, min: (byte)(Terrain.safeZoneMax + 1), max: 255);
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported range name - {rangeName}.");
                }

            }
        }

        public void AddRange(TerrainName terrainName, byte min, byte max)
        {
            if (!this.rangesByTerrainName.ContainsKey(terrainName))
            { this.rangesByTerrainName[terrainName] = new AllowedRange(min: min, max: max); }

            else
            { this.rangesByTerrainName[terrainName].ExpandRange(expandedMin: min, expandedMax: max); }
        }

        public void RemoveTerrain(TerrainName terrainName)
        {
            this.rangesByTerrainName.Remove(terrainName);
        }

        public bool CanStandHere(Vector2 position, World world)
        {
            foreach (var kvp in this.rangesByTerrainName)
            {
                var fieldValue = world.grid.GetFieldValue(position: position, terrainName: kvp.Key);
                bool terrainOk = kvp.Value.IsInRange(fieldValue);

                if (!terrainOk) return false;
            }

            return true;
        }

    }
}
