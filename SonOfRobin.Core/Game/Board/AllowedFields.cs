using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class AllowedFields
    {

        private Dictionary<TerrainName, AllowedRange> rangesByTerrainName;
        private readonly World world;

        public AllowedFields(World world)
        {
            // No fields defined - all fields allowed.

            this.world = world;
            this.rangesByTerrainName = new Dictionary<TerrainName, AllowedRange> { };
        }

        public AllowedFields(World world, Dictionary<TerrainName, AllowedRange> rangeDict)
        {
            // var exampleRangeDict = new Dictionary<string, AllowedRange>() { { TerrainName.Height, new AllowedRange(min: 0, max: 128) } };

            this.world = world;
            this.rangesByTerrainName = rangeDict;
        }


        public AllowedFields(World world, List<string> rangeNameList)
        {
            // var exampleRangeNameList = new List<string>() { BoardColors.WaterShallow, "ground_all" };

            this.world = world;
            this.rangesByTerrainName = new Dictionary<TerrainName, AllowedRange> { };

            foreach (var rangeName in rangeNameList)
            {
                switch (rangeName)
                {
                    case "all":

                        foreach (TerrainName terrainName in (TerrainName[])Enum.GetValues(typeof(TerrainName)))
                        { this.AddRange(terrainName: terrainName, min: 0, max: 255); }
                        break;

                    case "water_all":
                        this.AddRange(terrainName: TerrainName.Height, min: 0, max: Terrain.waterLevelMax);
                        break;

                    case "water_shallow":
                        this.AddRange(terrainName: TerrainName.Height, min: Convert.ToByte((Terrain.waterLevelMax / 3) * 2), max: Terrain.waterLevelMax);
                        break;

                    case "water_medium":
                        this.AddRange(terrainName: TerrainName.Height, min: Convert.ToByte(Terrain.waterLevelMax / 3), max: Convert.ToByte((Terrain.waterLevelMax / 3) * 2));
                        break;

                    case "water_deep":
                        this.AddRange(terrainName: TerrainName.Height, min: 0, max: Convert.ToByte(Terrain.waterLevelMax / 3));
                        break;

                    case "ground_sand":
                        this.AddRange(terrainName: TerrainName.Height, min: (byte)(Terrain.waterLevelMax + 1), max: 105);
                        break;

                    case "ground_all":
                        this.AddRange(terrainName: TerrainName.Height, min: (byte)(Terrain.waterLevelMax + 1), max: (byte)(Terrain.volcanoLevelMin - 1));
                        break;

                    case "volcano":
                        this.AddRange(terrainName: TerrainName.Height, min: (byte)(Terrain.volcanoLevelMin - 1), max: 255);
                        break;

                    default:
                        throw new DivideByZeroException($"Unsupported range name - {rangeName}.");
                }
            }
        }

        private void AddRange(TerrainName terrainName, byte min, byte max)
        {
            if (!this.rangesByTerrainName.ContainsKey(terrainName))
            { this.rangesByTerrainName[terrainName] = new AllowedRange(min: min, max: max); }

            else
            { this.rangesByTerrainName[terrainName].ExpandRange(expandedMin: min, expandedMax: max); }
        }

        public bool CanStandHere(Vector2 position)
        {
            foreach (var kvp in this.rangesByTerrainName)
            {
                var fieldValue = this.world.grid.GetFieldValue(position: position, terrainName: kvp.Key);
                bool terrainOk = kvp.Value.IsInRange(fieldValue);

                if (!terrainOk) { return false; }
            }

            return true;
        }

    }
}
