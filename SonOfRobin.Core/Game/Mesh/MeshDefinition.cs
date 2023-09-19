using System.Collections.Generic;
using System.Linq;
using static SonOfRobin.BoardMeshGenerator;

namespace SonOfRobin
{
    public readonly struct MeshDefinition
    {
        public static readonly Dictionary<TextureBank.TextureName, MeshDefinition> meshDefByTextureName = new Dictionary<TextureBank.TextureName, MeshDefinition>();
        public static readonly List<MeshDefinition> meshDefBySearchPriority = new List<MeshDefinition>();

        public readonly TextureBank.TextureName textureName;
        public readonly TextureBank.TextureName mapTextureName;
        public readonly RawMapDataSearchForTexture search;
        public readonly int drawPriority;
        public readonly int searchPriority;

        public MeshDefinition(TextureBank.TextureName textureName, TextureBank.TextureName mapTextureName, RawMapDataSearchForTexture search, int drawPriority = 0, int searchPriority = 0)
        {
            this.textureName = textureName;
            this.mapTextureName = mapTextureName;
            this.search = search;
            this.drawPriority = drawPriority;
            this.searchPriority = searchPriority;

            meshDefByTextureName[textureName] = this;
        }

        public static void CreateMeshDefinitions()
        {
            // needed to fill small holes, that will occur between other meshes
            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundBase,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundBase,
                searchPriority: 0,
                drawPriority: -1,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingWaterDeep,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterDeep,
                searchPriority: 1,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: (byte)(Terrain.waterLevelMax / 3)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingWaterMedium,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterMedium,
                searchPriority: 7,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: (byte)(Terrain.waterLevelMax / 3) + 1, maxVal: (byte)(Terrain.waterLevelMax / 3 * 2)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingWaterSuperShallow,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterSuperShallow,
                searchPriority: 12,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax - 3, maxVal: Terrain.waterLevelMax),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingBeachBright,
                mapTextureName: TextureBank.TextureName.RepeatingMapBeachBright,
                searchPriority: 5,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 95),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingBeachDark,
                mapTextureName: TextureBank.TextureName.RepeatingMapBeachDark,
                searchPriority: 6,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 96, maxVal: 105),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingSand,
                mapTextureName: TextureBank.TextureName.RepeatingMapSand,
                searchPriority: 15,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundBad,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundBad,
                searchPriority: 4,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 76, maxVal: 115),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundGood,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundGood,
                searchPriority: 13,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 116, maxVal: 120),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGrassBad,
                mapTextureName: TextureBank.TextureName.RepeatingMapGrassBad,
                searchPriority: 2,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 121, maxVal: 160),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGrassGood,
                mapTextureName: TextureBank.TextureName.RepeatingMapGrassGood,
                searchPriority: 9,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 161, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingMountainLow,
                mapTextureName: TextureBank.TextureName.RepeatingMapMountainLow,
                searchPriority: 3,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.rocksLevelMin, maxVal: 178),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingMountainMedium,
                mapTextureName: TextureBank.TextureName.RepeatingMapMountainMedium,
                searchPriority: 10,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 179, maxVal: 194),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingMountainHigh,
                mapTextureName: TextureBank.TextureName.RepeatingMapMountainHigh,
                searchPriority: 14,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 195, maxVal: Terrain.volcanoEdgeMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingVolcanoEdge,
                mapTextureName: TextureBank.TextureName.RepeatingMapVolcanoEdge,
                searchPriority: 16,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.volcanoEdgeMin, maxVal: Terrain.lavaMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingLava,
                mapTextureName: TextureBank.TextureName.RepeatingMapLava,
                searchPriority: 17,
                search: new(
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingSwamp,
                mapTextureName: TextureBank.TextureName.RepeatingMapSwamp,
                searchPriority: 8,
                search: new(
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true)})
                );

            new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingRuins,
                mapTextureName: TextureBank.TextureName.RepeatingMapRuins,
                searchPriority: 11,
                search: new(
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true)})
                );

            meshDefBySearchPriority.AddRange(meshDefByTextureName.Values.OrderBy(search => search.searchPriority));
        }
    }
}