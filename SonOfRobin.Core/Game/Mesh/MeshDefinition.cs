using MonoGame.Extended.Tweening;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class MeshDefinition
    {
        public static readonly Dictionary<TextureBank.TextureName, MeshDefinition> meshDefByTextureName = new Dictionary<TextureBank.TextureName, MeshDefinition>();
        public static readonly List<MeshDefinition> meshDefBySearchPriority = new List<MeshDefinition>();

        public readonly TextureBank.TextureName textureName;
        public readonly TextureBank.TextureName mapTextureName;
        public readonly MeshGenerator.RawMapDataSearchForTexture search;
        public readonly int drawPriority;

        public readonly Tweener tweener;
        public float textureOffsetX;
        public float textureOffsetY;
        public float textureScaleX;
        public float textureScaleY;

        public MeshDefinition(TextureBank.TextureName textureName, TextureBank.TextureName mapTextureName, MeshGenerator.RawMapDataSearchForTexture search, int drawPriority = 0)
        {
            this.textureName = textureName;
            this.mapTextureName = mapTextureName;
            this.search = search;
            this.drawPriority = drawPriority;

            TextureBank.GetTexture(textureName); // preloading texture, to avoid errors when generating meshes in parallel
            TextureBank.GetTexture(mapTextureName); // preloading texture, to avoid errors when generating meshes in parallel

            this.tweener = new Tweener();
            this.textureOffsetX = 0f;
            this.textureOffsetY = 0f;
            this.textureScaleX = 1f;
            this.textureScaleY = 1f;

            meshDefByTextureName[textureName] = this;
        }

        public static void CreateMeshDefinitions()
        {
            // needed to fill small holes, that will occur between other meshes
            MeshDefinition groundBase = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundBase,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundBase,
                drawPriority: -1,
                search: new(
                searchPriority: 0,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition waterDeep = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingWaterDeep,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterDeep,
                search: new(
                searchPriority: 1,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 0, maxVal: (byte)(Terrain.waterLevelMax / 3)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition waterMedium = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingWaterMedium,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterMedium,
                search: new(
                searchPriority: 7,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: (byte)(Terrain.waterLevelMax / 3) + 1, maxVal: (byte)(Terrain.waterLevelMax / 3 * 2)),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition waterSuperShallow = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingWaterSuperShallow,
                mapTextureName: TextureBank.TextureName.RepeatingMapWaterSuperShallow,
                search: new(
                searchPriority: 12,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax - 3, maxVal: Terrain.waterLevelMax),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition beachBright = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingBeachBright,
                mapTextureName: TextureBank.TextureName.RepeatingMapBeachBright,
                search: new(
                searchPriority: 5,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.waterLevelMax + 1, maxVal: 95),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition beachDark = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingBeachDark,
                mapTextureName: TextureBank.TextureName.RepeatingMapBeachDark,
                search: new(
                searchPriority: 6,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 96, maxVal: 105),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition sand = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingSand,
                mapTextureName: TextureBank.TextureName.RepeatingMapSand,
                search: new(
                searchPriority: 15,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 0, maxVal: 75),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition groundBad = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundBad,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundBad,
                search: new(
                searchPriority: 4,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 76, maxVal: 115),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition groundGood = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGroundGood,
                mapTextureName: TextureBank.TextureName.RepeatingMapGroundGood,
                search: new(
                searchPriority: 13,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 116, maxVal: 120),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition grassBad = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGrassBad,
                mapTextureName: TextureBank.TextureName.RepeatingMapGrassBad,
                search: new(
                searchPriority: 2,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 121, maxVal: 160),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition grassGood = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingGrassGood,
                mapTextureName: TextureBank.TextureName.RepeatingMapGrassGood,
                search: new(
                searchPriority: 9,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 106, maxVal: Terrain.rocksLevelMin - 1),
                    new SearchEntryTerrain(name: Terrain.Name.Humidity, minVal: 161, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition mountainLow = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingMountainLow,
                mapTextureName: TextureBank.TextureName.RepeatingMapMountainLow,
                search: new(
                searchPriority: 3,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.rocksLevelMin, maxVal: 178),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition mountainMedium = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingMountainMedium,
                mapTextureName: TextureBank.TextureName.RepeatingMapMountainMedium,
                search: new(
                searchPriority: 10,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 179, maxVal: 194),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition mountainHigh = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingMountainHigh,
                mapTextureName: TextureBank.TextureName.RepeatingMapMountainHigh,
                search: new(
                searchPriority: 14,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: 195, maxVal: Terrain.volcanoEdgeMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition volcanoEdge = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingVolcanoEdge,
                mapTextureName: TextureBank.TextureName.RepeatingMapVolcanoEdge,
                search: new(
                searchPriority: 16,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.volcanoEdgeMin, maxVal: Terrain.lavaMin - 1),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            MeshDefinition lava = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingLava,
                mapTextureName: TextureBank.TextureName.RepeatingMapLava,
                search: new(
                searchPriority: 17,
                searchEntriesTerrain: new List<SearchEntryTerrain> {
                    new SearchEntryTerrain(name: Terrain.Name.Height, minVal: Terrain.lavaMin, maxVal: 255),
                    },
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: false),
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: false)})
                );

            lava.tweener.TweenTo(target: lava, expression: meshDef => meshDef.textureOffsetX, toValue: 0.2f, duration: 20, delay: 5)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            lava.tweener.TweenTo(target: lava, expression: meshDef => meshDef.textureOffsetY, toValue: 0.2f, duration: 20, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            MeshDefinition swamp = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingSwamp,
                mapTextureName: TextureBank.TextureName.RepeatingMapSwamp,
                search: new(
                searchPriority: 8,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeSwamp, value: true)})
                );

            swamp.tweener.TweenTo(target: swamp, expression: meshDef => meshDef.textureOffsetX, toValue: -0.002f, duration: 2.0f, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.CircleInOut);

            swamp.tweener.TweenTo(target: swamp, expression: meshDef => meshDef.textureOffsetY, toValue: 0.002f, duration: 1.7f, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.CircleInOut);

            swamp.tweener.TweenTo(target: swamp, expression: meshDef => meshDef.textureScaleX, toValue: 0.98f, duration: 240, delay: 3)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);

            swamp.tweener.TweenTo(target: swamp, expression: meshDef => meshDef.textureScaleY, toValue: 1.01f, duration: 120, delay: 0)
                .RepeatForever(repeatDelay: 0f)
                .AutoReverse()
                .Easing(EasingFunctions.QuadraticInOut);


            MeshDefinition ruins = new MeshDefinition(
                textureName: TextureBank.TextureName.RepeatingRuins,
                mapTextureName: TextureBank.TextureName.RepeatingMapRuins,
                search: new(
                searchPriority: 11,
                searchEntriesExtProps: new List<SearchEntryExtProps> {
                    new SearchEntryExtProps(name: ExtBoardProps.Name.BiomeRuins, value: true)})
                );

            meshDefBySearchPriority.AddRange(meshDefByTextureName.Values.OrderBy(meshDef => meshDef.search.searchPriority));
        }

        public static void UpdateTweeners()
        {
            foreach (MeshDefinition meshDef in meshDefBySearchPriority)
            {
                meshDef.tweener.Update((float)SonOfRobinGame.CurrentGameTime.ElapsedGameTime.TotalSeconds);
            }
        }
    }
}